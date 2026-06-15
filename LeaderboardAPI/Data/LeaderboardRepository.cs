using Microsoft.Data.Sqlite;
using LeaderboardAPI.Models;

namespace LeaderboardAPI.Data;

/// <summary>
/// SQLite 排行榜数据访问层
/// </summary>
public class LeaderboardRepository
{
    private readonly string _connectionString;

    public LeaderboardRepository(string dbPath = "leaderboard.db")
    {
        _connectionString = $"Data Source={dbPath}";
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Scores (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PlayerName TEXT NOT NULL,
                Score INTEGER NOT NULL,
                SnakeLength INTEGER NOT NULL,
                Level INTEGER NOT NULL,
                Difficulty TEXT NOT NULL,
                Theme TEXT NOT NULL,
                SubmittedAt TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS idx_scores_score ON Scores(Score DESC);
        ";
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// 获取排行榜前 N 名
    /// </summary>
    public async Task<List<ScoreEntry>> GetTopScores(int top = 20)
    {
        var scores = new List<ScoreEntry>();

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Scores ORDER BY Score DESC LIMIT @top";
        command.Parameters.AddWithValue("@top", top);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            scores.Add(new ScoreEntry
            {
                Id = reader.GetInt32(0),
                PlayerName = reader.GetString(1),
                Score = reader.GetInt32(2),
                SnakeLength = reader.GetInt32(3),
                Level = reader.GetInt32(4),
                Difficulty = reader.GetString(5),
                Theme = reader.GetString(6),
                SubmittedAt = DateTime.Parse(reader.GetString(7)),
            });
        }

        return scores;
    }

    /// <summary>
    /// 提交一条新分数
    /// </summary>
    public async Task<int> AddScore(ScoreEntry entry)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Scores (PlayerName, Score, SnakeLength, Level, Difficulty, Theme, SubmittedAt)
            VALUES (@name, @score, @length, @level, @diff, @theme, @time);
            SELECT last_insert_rowid();
        ";
        command.Parameters.AddWithValue("@name", entry.PlayerName);
        command.Parameters.AddWithValue("@score", entry.Score);
        command.Parameters.AddWithValue("@length", entry.SnakeLength);
        command.Parameters.AddWithValue("@level", entry.Level);
        command.Parameters.AddWithValue("@diff", entry.Difficulty);
        command.Parameters.AddWithValue("@theme", entry.Theme);
        command.Parameters.AddWithValue("@time", entry.SubmittedAt.ToString("o"));

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
}
