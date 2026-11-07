namespace SingleCore.Core.Models;

public sealed class UiSettings
{
    public bool IsMenuVisible { get; set; } = true;
    public bool EnableAnimations { get; set; } = true;
    public float Opacity { get; set; } = 0.92f;
    public float Scale { get; set; } = 1.0f;
    public int WindowWidth { get; set; } = 420;
    public int WindowHeight { get; set; } = 560;
    public float AccentR { get; set; } = 0.89f;
    public float AccentG { get; set; } = 0.14f;
    public float AccentB { get; set; } = 0.12f;
    public bool ShowMainTab { get; set; } = true;
    public bool ShowVisualsTab { get; set; } = true;
    public bool ShowSettingsTab { get; set; } = true;
    public bool ShowConfigTab { get; set; } = true;
    public int Hotkey { get; set; } = 0x74;
    public bool MinimalMode { get; set; } = false;
    public bool CompactMode { get; set; } = true;
}

public sealed class AppConfig
{
    public string Version { get; set; } = "1.0.0";
    public UiSettings Ui { get; set; } = new();
    public DateTime LastSavedUtc { get; set; } = DateTime.UtcNow;
}
