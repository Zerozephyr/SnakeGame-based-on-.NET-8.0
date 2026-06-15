using System.Runtime.InteropServices;

namespace SnakeGame.Services;

/// <summary>
/// 背景音乐管理 — 通过 Windows Media Player COM 播放
/// </summary>
public class MusicService : IDisposable
{
    private dynamic? _wmpPlayer;
    private string? _currentMusicFile;
    private bool _isMusicEnabled = true;
    private bool _disposed;

    public bool IsMusicEnabled
    {
        get => _isMusicEnabled;
        set
        {
            _isMusicEnabled = value;
            if (value)
                Play();
            else
                Stop();
        }
    }

    public bool IsPlaying { get; private set; }
    public string? CurrentMusicFile => _currentMusicFile;

    /// <summary>
    /// 初始化 WMP 并自动查找音乐文件
    /// </summary>
    public void Initialize()
    {
        try
        {
            Type? wmpType = Type.GetTypeFromProgID("WMPlayer.OCX.7");
            if (wmpType != null)
            {
                _wmpPlayer = Activator.CreateInstance(wmpType);
                if (_wmpPlayer != null)
                {
                    _wmpPlayer.settings.volume = 50;
                    _wmpPlayer.settings.setMode("loop", true);
                }
            }

            // 查找音乐文件 — 优先 Sounds/ 子目录，其次根目录
            string[] musicExtensions = { ".wav", ".mp3", ".wma", ".m4a", ".aac" };
            string[] searchNames = { "bgm", "music", "background", "sound" };
            string[] searchDirs = {
                Path.Combine(Application.StartupPath, "Sounds"),
                Application.StartupPath
            };

            foreach (var dir in searchDirs)
            {
                if (!Directory.Exists(dir)) continue;
                foreach (var name in searchNames)
                    foreach (var ext in musicExtensions)
                    {
                        string fullPath = Path.Combine(dir, name + ext);
                        if (File.Exists(fullPath))
                        {
                            _currentMusicFile = fullPath;
                            if (_wmpPlayer != null) _wmpPlayer.URL = _currentMusicFile;
                            return;
                        }
                    }

                // 该目录下任意音频文件
                foreach (var ext in musicExtensions)
                {
                    string[] files = Directory.GetFiles(dir, "*" + ext);
                    if (files.Length > 0)
                    {
                        _currentMusicFile = files[0];
                        if (_wmpPlayer != null) _wmpPlayer.URL = _currentMusicFile;
                        return;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"音乐初始化失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    public void Play()
    {
        try
        {
            if (_wmpPlayer != null && _isMusicEnabled && !string.IsNullOrEmpty(_currentMusicFile))
            {
#pragma warning disable CS8602 // 动态对象空引用（已在 if 条件中检查）
                _wmpPlayer.controls.play();
#pragma warning restore CS8602
                IsPlaying = true;
            }
        }
        catch { }
    }

    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public void Stop()
    {
        try
        {
            if (_wmpPlayer != null)
            {
                _wmpPlayer.controls.stop();
                IsPlaying = false;
            }
        }
        catch { }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Stop();
        if (_wmpPlayer != null)
        {
            try { Marshal.ReleaseComObject(_wmpPlayer); } catch { }
        }
    }
}
