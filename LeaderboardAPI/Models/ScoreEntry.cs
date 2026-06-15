using System.Text.Json.Serialization;

namespace LeaderboardAPI.Models;

/// <summary>
/// 提交分数请求
/// </summary>
public class ScoreRequest
{
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
}

/// <summary>
/// 排行榜分数记录
/// </summary>
public class ScoreEntry
{
    public int Id { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int Score { get; set; }
    public int SnakeLength { get; set; }
    public int Level { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public static ScoreEntry FromRequest(ScoreRequest req) => new()
    {
        PlayerName = req.PlayerName.Trim(),
        Score = req.Score,
        SnakeLength = req.SnakeLength,
        Level = req.Level,
        Difficulty = req.Difficulty,
        Theme = req.Theme,
    };
}
