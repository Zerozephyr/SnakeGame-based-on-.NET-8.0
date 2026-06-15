namespace SnakeGame.Models;

/// <summary>
/// 游戏运行时状态
/// </summary>
public class GameState
{
    public int Score { get; set; }
    public int HighScore { get; set; }
    public int Level { get; set; } = 1;
    public int FoodsEatenThisLevel { get; set; }
    public bool IsRunning { get; set; }
    public bool IsPaused { get; set; }
    public bool IsAIMode { get; set; }
    public bool IsMusicEnabled { get; set; } = true;
    public DifficultyLevel CurrentDifficulty { get; set; } = DifficultyLevel.Medium;
    public List<Point> Obstacles { get; set; } = new();

    /// <summary>
    /// 当前游戏速度 (ms)
    /// </summary>
    public int CurrentSpeed =>
        Math.Max(GameConfig.MinSpeed,
            (int)CurrentDifficulty - (Level - 1) * GameConfig.SpeedDecrementPerLevel);
}
