using System.Numerics;
using ImGuiNET;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace SingleCore.App;

public sealed class ImGuiController : IDisposable
{
    private readonly GL _gl;
    private readonly IWindow _window;
    private readonly uint _vertexArrayHandle;
    private readonly uint _vertexBufferHandle;
    private readonly uint _indexBufferHandle;
    private readonly uint _fontTextureHandle;
    private readonly uint _shaderHandle;
    private readonly int _attribLocationTex;
    private readonly int _attribLocationProj;
    private readonly int _attribLocationPosition;
    private readonly int _attribLocationUv;
    private readonly int _attribLocationColor;
    private readonly int _textureLocation;
    private bool _frameStarted;

    public ImGuiController(GL gl, IWindow window)
    {
        _gl = gl;
        _window = window;

        ImGui.CreateContext();
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        io.Fonts.AddFontDefault();
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

        _vertexArrayHandle = _gl.GenVertexArray();
        _vertexBufferHandle = _gl.GenBuffer();
        _indexBufferHandle = _gl.GenBuffer();
        _fontTextureHandle = _gl.GenTexture();

        var vertexShaderSource = @"#version 330 core
            layout(location = 0) in vec2 Position;
            layout(location = 1) in vec2 UV;
            layout(location = 2) in vec4 Color;
            uniform mat4 ProjMtx;
            out vec2 Frag_UV;
            out vec4 Frag_Color;
            void main()
            {
                Frag_UV = UV;
                Frag_Color = Color;
                gl_Position = ProjMtx * vec4(Position.xy, 0, 1);
            }";

        var fragmentShaderSource = @"#version 330 core
            in vec2 Frag_UV;
            in vec4 Frag_Color;
            uniform sampler2D Texture;
            out vec4 Out_Color;
            void main()
            {
                Out_Color = Frag_Color * texture(Texture, Frag_UV.st);
            }";

        _shaderHandle = CreateProgram(vertexShaderSource, fragmentShaderSource);
        _attribLocationTex = _gl.GetUniformLocation(_shaderHandle, "Texture");
        _attribLocationProj = _gl.GetUniformLocation(_shaderHandle, "ProjMtx");
        _attribLocationPosition = 0;
        _attribLocationUv = 1;
        _attribLocationColor = 2;

        var pixels = new byte[1];
        var width = 1;
        var height = 1;
        var textureData = new byte[width * height * 4];
        _gl.BindTexture(TextureTarget.Texture2D, _fontTextureHandle);
        _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, textureData);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        var fontTexture = io.Fonts.GetTexDataAsRGBA32(out IntPtr pixelsPtr, out int texWidth, out int texHeight);
        _gl.BindTexture(TextureTarget.Texture2D, _fontTextureHandle);
        _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)texWidth, (uint)texHeight, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixelsPtr);

        io.Fonts.SetTexID((IntPtr)_fontTextureHandle);
        _gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void NewFrame()
    {
        if (_frameStarted)
        {
            return;
        }

        var io = ImGui.GetIO();
        io.DisplaySize = new Vector2(_window.Size.X, _window.Size.Y);
        io.DisplayFramebufferScale = new Vector2(1f, 1f);
        io.DeltaTime = 1f / 60f;
        ImGui.NewFrame();
        _frameStarted = true;
    }

    public void Render()
    {
        if (!_frameStarted)
        {
            return;
        }

        ImGui.Render();
        var drawData = ImGui.GetDrawData();
        RenderDrawData(drawData);
        _frameStarted = false;
    }

    public void Dispose()
    {
        ImGui.DestroyContext();
    }

    private void RenderDrawData(ImDrawDataPtr drawData)
    {
        if (drawData.CmdListsCount == 0)
        {
            return;
        }

        var width = (int)(drawData.DisplaySize.X * drawData.FramebufferScale.X);
        var height = (int)(drawData.DisplaySize.Y * drawData.FramebufferScale.Y);
        if (width <= 0 || height <= 0)
        {
            return;
        }

        _gl.Viewport(0, 0, (uint)width, (uint)height);
        _gl.Enable(EnableCap.Blend);
        _gl.BlendEquation(BlendEquationModeEXT.FuncAdd);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        _gl.Disable(EnableCap.CullFace);
        _gl.Disable(EnableCap.DepthTest);

        var projMatrix = Matrix4x4.CreateOrthographicOffCenter(
            0f,
            drawData.DisplaySize.X,
            drawData.DisplaySize.Y,
            0f,
            -1f,
            1f);

        _gl.UseProgram(_shaderHandle);
        _gl.UniformMatrix4(_attribLocationProj, 1, false, projMatrix);
        _gl.Uniform1(_attribLocationTex, 0);

        for (var listIndex = 0; listIndex < drawData.CmdListsCount; listIndex++)
        {
            var cmdList = drawData.CmdLists[listIndex];
            var vertices = cmdList.VtxBuffer.Data; 
            var indices = cmdList.IdxBuffer.Data;

            _gl.BindVertexArray(_vertexArrayHandle);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBufferHandle);
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * 20), vertices, BufferUsageARB.DynamicDraw);
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _indexBufferHandle);
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * 2), indices, BufferUsageARB.DynamicDraw);

            _gl.EnableVertexAttribArray(0);
            _gl.EnableVertexAttribArray(1);
            _gl.EnableVertexAttribArray(2);
            _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, (uint)Unsafe.SizeOf<ImDrawVert>(), (nint)0);
            _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)Unsafe.SizeOf<ImDrawVert>(), (nint)8);
            _gl.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, (uint)Unsafe.SizeOf<ImDrawVert>(), (nint)16);

            var offset = 0;
            for (var cmdIndex = 0; cmdIndex < cmdList.CmdBuffer.Size; cmdIndex++)
            {
                var drawCmd = cmdList.CmdBuffer[cmdIndex];
                if (drawCmd.UserCallback != IntPtr.Zero)
                {
                    throw new NotSupportedException("Custom user callbacks are not supported");
                }

                _gl.ActiveTexture(TextureUnit.Texture0);
                _gl.BindTexture(TextureTarget.Texture2D, (uint)drawCmd.TextureId);
                _gl.Scissor((int)drawCmd.ClipRect.X, (int)(height - drawCmd.ClipRect.W), (int)(drawCmd.ClipRect.Z - drawCmd.ClipRect.X), (int)(drawCmd.ClipRect.W - drawCmd.ClipRect.Y));
                _gl.DrawElements(PrimitiveType.Triangles, (uint)drawCmd.ElemCount, DrawElementsType.UnsignedShort, (nint)offset);
                offset += (int)drawCmd.ElemCount * 2;
            }
        }

        _gl.Disable(EnableCap.Blend);
        _gl.Disable(EnableCap.ScissorTest);
    }

    private uint CreateProgram(string vertexSource, string fragmentSource)
    {
        var vertexShader = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, vertexSource);
        _gl.CompileShader(vertexShader);

        var fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentShader, fragmentSource);
        _gl.CompileShader(fragmentShader);

        var program = _gl.CreateProgram();
        _gl.AttachShader(program, vertexShader);
        _gl.AttachShader(program, fragmentShader);
        _gl.LinkProgram(program);

        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);

        return program;
    }
}
