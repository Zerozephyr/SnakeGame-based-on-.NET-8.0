using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SnakeGame.Services;

/// <summary>
/// 排行榜条目（与 API 端对应）
/// </summary>
public class LeaderboardEntry
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("playerName")]
    public string PlayerName { get; set; } = string.Empty;

    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("snakeLength")]
    public int SnakeLength { get; set; }

    [JsonPropertyName("level")]
    public int Level { get; set; }

    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = string.Empty;

    [JsonPropertyName("theme")]
    public string Theme { get; set; } = string.Empty;

    [JsonPropertyName("submittedAt")]
    public DateTime SubmittedAt { get; set; }
}

/// <summary>
/// 排行榜 HTTP 客户端服务 — 与 LeaderboardAPI 通信
/// </summary>
public class LeaderboardService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly bool _isLocal;
    private System.Diagnostics.Process? _apiProcess;
    private bool _disposed;

    // 默认服务器地址
    private const string DefaultServerUrl = "http://yourIP:5123";

    /// <summary>
    /// 读取 server.txt 可覆盖默认地址（仅开发调试用），文件不存在则用编译默认值
    /// </summary>
    private static string LoadServerUrl()
    {
        string configPath = Path.Combine(Application.StartupPath, "server.txt");
        try
        {
            if (File.Exists(configPath))
            {
                string url = File.ReadAllText(configPath).Trim();
                if (!string.IsNullOrEmpty(url) && Uri.TryCreate(url, UriKind.Absolute, out _))
                    return url;
            }
        }
        catch { }
        return DefaultServerUrl;
    }

    public LeaderboardService(string? baseUrl = null)
    {
        if (baseUrl == null)
            baseUrl = LoadServerUrl();

        _baseUrl = baseUrl;
        _isLocal = _baseUrl.Contains("localhost") || _baseUrl.Contains("127.0.0.1");
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl),
            Timeout = TimeSpan.FromSeconds(10),
        };
    }

    /// <summary>
    /// 当前连接的服务器地址
    /// </summary>
    public string ServerUrl => _baseUrl;

    /// <summary>
    /// 启动 API 服务器进程（仅本地时有效）
    /// </summary>
    public async Task<bool> StartApiServerAsync()
    {
        // 远程服务器不需要本地启动
        if (!_isLocal) return await IsApiRunningAsync();

        // 先检查是否已经在运行
        if (await IsApiRunningAsync())
            return true;

        try
        {
            // 定位 API 可执行文件：
            // 游戏运行在 bin/Debug/net8.0-windows/，项目根目录是往上 3 级
            // API 在 LeaderboardAPI/bin/Debug/net8.0/LeaderboardAPI.exe
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));

            var candidates = new List<string>
            {
                // Debug 构建
                Path.Combine(projectRoot, "LeaderboardAPI", "bin", "Debug", "net8.0", "LeaderboardAPI.exe"),
                // Release 构建
                Path.Combine(projectRoot, "LeaderboardAPI", "bin", "Release", "net8.0", "LeaderboardAPI.exe"),
                // 与游戏可执行文件同目录
                Path.Combine(baseDir, "LeaderboardAPI.exe"),
                // 直接在 LeaderboardAPI 目录下通过 dotnet run 场景
                Path.Combine(projectRoot, "LeaderboardAPI", "bin", "Debug", "net8.0", "LeaderboardAPI.dll"),
            };

            string? apiExe = null;
            foreach (var candidate in candidates)
            {
                if (File.Exists(candidate))
                {
                    apiExe = candidate;
                    break;
                }
            }

            if (apiExe == null)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"API 可执行文件未找到。搜索路径:\n{string.Join("\n", candidates)}");
                return false;
            }

            // 如果是 .dll 文件，使用 dotnet 启动
            bool useDotNet = apiExe.EndsWith(".dll");
            string fileName = useDotNet ? "dotnet" : apiExe;
            string arguments = useDotNet ? apiExe : "";

            _apiProcess = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                },
                EnableRaisingEvents = true,
            };

            _apiProcess.Start();

            // 等待 API 启动（最多 5 秒）
            for (int i = 0; i < 25; i++)
            {
                await Task.Delay(200);
                if (await IsApiRunningAsync())
                    return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"启动 API 失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 停止 API 服务器进程
    /// </summary>
    public void StopApiServer()
    {
        try
        {
            _apiProcess?.Kill(entireProcessTree: true);
            _apiProcess?.Dispose();
            _apiProcess = null;
        }
        catch { }
    }

    /// <summary>
    /// 检查 API 是否在运行
    /// </summary>
    public async Task<bool> IsApiRunningAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 获取排行榜前 N 名
    /// </summary>
    public async Task<List<LeaderboardEntry>> GetTopScoresAsync(int top = 20)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/leaderboard?top={top}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<LeaderboardEntry>>()
                    ?? new List<LeaderboardEntry>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"获取排行榜失败: {ex.Message}");
        }

        return new List<LeaderboardEntry>();
    }

    /// <summary>
    /// 提交一条分数
    /// </summary>
    public async Task<bool> SubmitScoreAsync(LeaderboardEntry entry)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/scores", entry);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"提交分数失败: {ex.Message}");
            return false;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        StopApiServer();
        _httpClient.Dispose();
    }
}
