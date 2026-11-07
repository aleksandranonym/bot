using System.Numerics;
using ImGuiNET;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SingleCore.Core.Interfaces;
using SingleCore.Core.Models;
using SingleCore.Infrastructure.Services;

namespace SingleCore.App;

public static class Program
{
    public static async Task Main()
    {
        try
        {
            var configStore = new JsonConfigStore();
            var logger = new FileLogger();
            var notifications = new NotificationService();

            var config = await configStore.LoadAsync();
            config.Ui ??= new UiSettings();

            logger.Info("SingleCore started");
            notifications.Push("SingleCore initialized");

            var window = Window.Create(new WindowOptions
            {
                Title = "SingleCore",
                Size = new Vector2D<int>(1200, 760),
                WindowBorder = WindowBorder.Resizable,
                VSync = true,
                API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Debug, new APIVersion(3, 3))
            });

            var gl = GL.GetApi(window);
            var menu = new SingleCoreMenu(config.Ui, notifications, logger, gl, window);

            window.Load += () =>
            {
                menu.Initialize();
            };

            window.Update += _ => menu.Update();
            window.Render += _ => menu.Render();
            window.Closing += () =>
            {
                try
                {
                    logger.Info("SingleCore closing");
                    _ = configStore.SaveAsync(config).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    logger.Error("Error while saving config", exception);
                }
            };

            window.Run();
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Fatal app error: {exception}");
            File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "logs", "singlecore.log"), $"[{DateTime.UtcNow:O}] [FATAL] {exception}{Environment.NewLine}");
        }
    }
}
