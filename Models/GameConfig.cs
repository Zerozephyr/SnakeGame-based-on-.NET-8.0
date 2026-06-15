namespace SnakeGame.Models;

/// <summary>
/// 游戏全局配置常量
/// </summary>
public static class GameConfig
{
    // 绘制参数
    public const int CellSize = 20;
    public const int GridWidth = 30;
    public const int GridHeight = 25;
    public const int HeaderHeight = 140;

    // 速度 (ms)
    public const int InitialSpeed = 150;
    public const int SpeedFast = 80;
    public const int SpeedSlow = 200;
    public const int MinSpeed = 40;
    public const int SpeedDecrementPerLevel = 10;

    // 分数
    public const int PointsPerFood = 10;

    // 关卡
    public const int FoodsPerLevel = 5;
    public const int MaxLevel = 10;
    public const int MaxObstacles = 15;
    public const int ObstaclesPerLevel = 2;
    public const int ObstacleMinDistanceFromHead = 4;

    // 蛇
    public const int InitialSnakeLength = 3;
}

/// <summary>
/// 难度等级
/// </summary>
public enum DifficultyLevel
{
    Slow = 200,
    Medium = 150,
    Fast = 80
}
