using System.Media;

namespace SnakeGame.Services;

/// <summary>
/// 音效服务 — 从 Sounds/ 文件夹播放预置 WAV 文件
/// </summary>
public class SoundService : IDisposable
{
    private readonly Dictionary<string, string> _soundFiles = new();
    private bool _enabled = true;
    private bool _disposed;

    private static string SoundsDir =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds");

    public bool Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }

    public SoundService()
    {
        string dir = SoundsDir;
        foreach (var key in new[] { "eat", "death", "levelup" })
        {
            string path = Path.Combine(dir, $"{key}.wav");
            if (File.Exists(path))
                _soundFiles[key] = path;
        }
    }

    public void PlayEat() => PlaySound("eat");
    public void PlayDeath() => PlaySound("death");
    public void PlayLevelUp() => PlaySound("levelup");

    private void PlaySound(string key)
    {
        if (!_enabled || _disposed) return;
        if (!_soundFiles.TryGetValue(key, out string? path)) return;

        Task.Run(() =>
        {
            try
            {
                using var player = new SoundPlayer(path);
                player.PlaySync();
            }
            catch { }
        });
    }

    public void Dispose()
    {
        _disposed = true;
        _soundFiles.Clear();
    }
}
