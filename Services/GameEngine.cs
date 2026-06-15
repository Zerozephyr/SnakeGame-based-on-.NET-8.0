using SnakeGame.Models;

namespace SnakeGame.Services;

/// <summary>
/// 游戏引擎 — 核心游戏循环与规则
/// </summary>
public class GameEngine
{
    private readonly Snake _snake;
    private readonly GameState _state;
    private readonly Food _food;
    private readonly LevelManager _levelManager;
    private readonly AIPathfinder? _aiPathfinder;
    private readonly Random _random;

    // 事件
    public event Action? StateChanged;
    public event Action? GameOver;
    public event Action? FoodEaten;
    public event Action? LevelUp;
    public event Action<bool>? MusicToggled;

    // 公开属性
    public Snake Snake => _snake;
    public GameState State => _state;
    public Food CurrentFood => _food;
    public LevelManager LevelManager => _levelManager;

    public GameEngine(Random? random = null)
    {
        _random = random ?? new Random();
        _snake = new Snake();
        _state = new GameState();
        _food = new Food();
        _levelManager = new LevelManager(_state, _random);
        _aiPathfinder = new AIPathfinder(GameConfig.GridWidth, GameConfig.GridHeight);
    }

    // ═══════════════════════════════════════════
    // 游戏控制
    // ═══════════════════════════════════════════

    public void Reset()
    {
        _snake.Reset();
        _state.Score = 0;
        _state.IsPaused = false;
        _levelManager.Reset();
        _food.Generate(_random, _snake, _state.Obstacles);
    }

    public void StartGame()
    {
        _state.IsRunning = true;
        StateChanged?.Invoke();
    }

    public void StopGame()
    {
        _state.IsRunning = false;
        StateChanged?.Invoke();
    }

    public void TogglePause()
    {
        if (!_state.IsRunning) return;
        _state.IsPaused = !_state.IsPaused;
    }

    public void ToggleAI()
    {
        _state.IsAIMode = !_state.IsAIMode;
    }

    public void ToggleMusic()
    {
        _state.IsMusicEnabled = !_state.IsMusicEnabled;
        MusicToggled?.Invoke(_state.IsMusicEnabled);
    }

    public void SetDifficulty(DifficultyLevel level)
    {
        _state.CurrentDifficulty = level;
    }

    // ═══════════════════════════════════════════
    // 方向控制
    // ═══════════════════════════════════════════

    public bool TrySetDirection(Point newDir)
    {
        // 禁止 180 度掉头
        if (newDir.X == -_snake.Direction.X && newDir.Y == -_snake.Direction.Y)
            return false;

        _snake.NextDirection = newDir;
        return true;
    }

    // ═══════════════════════════════════════════
    // 核心游戏循环
    // ═══════════════════════════════════════════

    public void Tick()
    {
        if (!_state.IsRunning || _state.IsPaused) return;

        // AI 模式：自动计算方向
        if (_state.IsAIMode && _aiPathfinder != null)
        {
            var aiDir = _aiPathfinder.FindSafeDirection(_snake, _food.Position, _state.Obstacles);
            if (aiDir.HasValue)
            {
                _snake.NextDirection = aiDir.Value;
            }
        }

        // 应用方向
        _snake.Direction = _snake.NextDirection;

        // 计算新头部
        Point newHead = _snake.PeekNewHead();

        // 碰撞检测 — 墙壁
        if (newHead.X < 0 || newHead.X >= GameConfig.GridWidth ||
            newHead.Y < 0 || newHead.Y >= GameConfig.GridHeight)
        {
            OnGameOver();
            return;
        }

        // 碰撞检测 — 障碍物
        if (_levelManager.WouldCollideWithObstacle(newHead))
        {
            OnGameOver();
            return;
        }

        // 碰撞检测 — 自身
        if (_snake.WouldCollideWithSelf(newHead))
        {
            OnGameOver();
            return;
        }

        // 检查是否吃到食物
        bool eating = (newHead == _food.Position);

        // 移动蛇
        _snake.Move(eating);

        if (eating)
        {
            _state.Score += GameConfig.PointsPerFood;
            if (_state.Score > _state.HighScore)
            {
                _state.HighScore = _state.Score;
            }
            FoodEaten?.Invoke();

            // 检查升级
            if (_levelManager.CheckLevelUp(_snake, _food))
            {
                LevelUp?.Invoke();
            }

            _food.Generate(_random, _snake, _state.Obstacles);
        }

        StateChanged?.Invoke();
    }

    private void OnGameOver()
    {
        _state.IsRunning = false;
        GameOver?.Invoke();
        StateChanged?.Invoke();
    }
}
