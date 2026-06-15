using SnakeGame.Models;

namespace SnakeGame.Services;

/// <summary>
/// 关卡管理器 — 等级追踪、障碍物生成、速度调整
/// </summary>
public class LevelManager
{
    private readonly GameState _state;
    private readonly Random _random;

    public LevelManager(GameState state, Random random)
    {
        _state = state;
        _random = random;
    }

    /// <summary>
    /// 吃掉食物后调用 — 检查是否升级
    /// </summary>
    /// <returns>是否升级了</returns>
    public bool CheckLevelUp(Snake snake, Food food)
    {
        _state.FoodsEatenThisLevel++;

        if (_state.FoodsEatenThisLevel >= GameConfig.FoodsPerLevel
            && _state.Level < GameConfig.MaxLevel)
        {
            _state.Level++;
            _state.FoodsEatenThisLevel = 0;
            SpawnObstacles(snake, food);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 生成当前等级的障碍物
    /// </summary>
    public void SpawnObstacles(Snake snake, Food food)
    {
        // 目标障碍物数 = 等级 * 每级数量，但不超过上限
        int targetCount = Math.Min(_state.Level * GameConfig.ObstaclesPerLevel, GameConfig.MaxObstacles);

        while (_state.Obstacles.Count < targetCount)
        {
            int maxAttempts = 200;
            int attempts = 0;
            Point newObstacle;

            do
            {
                newObstacle = new Point(
                    _random.Next(GameConfig.GridWidth),
                    _random.Next(GameConfig.GridHeight));
                attempts++;
            } while (attempts < maxAttempts &&
                     (IsTooCloseToHead(snake, newObstacle) ||
                      snake.Occupies(newObstacle) ||
                      newObstacle == food.Position ||
                      _state.Obstacles.Contains(newObstacle)));

            if (attempts < maxAttempts)
            {
                _state.Obstacles.Add(newObstacle);
            }
            else
            {
                break; // 找不到合适位置，停止生成
            }
        }
    }

    /// <summary>
    /// 清除所有障碍物
    /// </summary>
    public void ClearObstacles()
    {
        _state.Obstacles.Clear();
    }

    /// <summary>
    /// 检查某位置是否与障碍物碰撞
    /// </summary>
    public bool WouldCollideWithObstacle(Point position)
    {
        return _state.Obstacles.Contains(position);
    }

    /// <summary>
    /// 重置关卡状态
    /// </summary>
    public void Reset()
    {
        _state.Level = 1;
        _state.FoodsEatenThisLevel = 0;
        ClearObstacles();
    }

    private bool IsTooCloseToHead(Snake snake, Point pos)
    {
        if (snake.Length == 0) return false;
        var head = snake.Head;
        return Math.Abs(pos.X - head.X) < GameConfig.ObstacleMinDistanceFromHead
            && Math.Abs(pos.Y - head.Y) < GameConfig.ObstacleMinDistanceFromHead;
    }
}
