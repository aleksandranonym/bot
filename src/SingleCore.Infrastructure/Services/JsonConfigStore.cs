namespace SingleCore.Infrastructure.Services;

using System.Text.Json;
using SingleCore.Core.Interfaces;
using SingleCore.Core.Models;

public sealed class JsonConfigStore : IConfigStore
{
    private readonly string _configPath;

    public JsonConfigStore(string? configPath = null)
    {
        _configPath = configPath ?? Path.Combine(AppContext.BaseDirectory, "config", "singlecore.config.json");
    }

    public async Task<AppConfig> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_configPath))
        {
            return new AppConfig();
        }

        await using var stream = File.OpenRead(_configPath);
        var config = await JsonSerializer.DeserializeAsync<AppConfig>(stream, cancellationToken: cancellationToken);
        return config ?? new AppConfig();
    }

    public async Task SaveAsync(AppConfig config, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(_configPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        config.LastSavedUtc = DateTime.UtcNow;

        await using var stream = File.Create(_configPath);
        await JsonSerializer.SerializeAsync(stream, config, cancellationToken: cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }
}
