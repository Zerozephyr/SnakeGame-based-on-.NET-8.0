using SnakeGame.Models;

namespace SnakeGame.Services;

/// <summary>
/// 键盘输入处理
/// </summary>
public class InputHandler
{
    private readonly GameEngine _engine;
    private readonly SkinManager _skinManager;

    // 事件：通知 UI 层刷新
    public event Action? StateChanged;
    public event Action? AIToggled;

    public InputHandler(GameEngine engine, SkinManager skinManager)
    {
        _engine = engine;
        _skinManager = skinManager;
    }

    /// <summary>
    /// 处理按键。返回 true 表示已处理。
    /// </summary>
    public bool ProcessKey(Keys keyCode)
    {
        switch (keyCode)
        {
            // ═══ 方向控制 ═══
            case Keys.W:
            case Keys.Up:
                _engine.TrySetDirection(new Point(0, -1));
                return true;

            case Keys.S:
            case Keys.Down:
                _engine.TrySetDirection(new Point(0, 1));
                return true;

            case Keys.A:
            case Keys.Left:
                _engine.TrySetDirection(new Point(-1, 0));
                return true;

            case Keys.D:
            case Keys.Right:
                _engine.TrySetDirection(new Point(1, 0));
                return true;

            // ═══ 暂停 ═══
            case Keys.Space:
                _engine.TogglePause();
                StateChanged?.Invoke();
                return true;

            // ═══ 重新开始 ═══
            case Keys.R:
                _engine.Reset();
                _engine.StartGame();
                StateChanged?.Invoke();
                return true;

            // ═══ 蛇皮切换（Tab 只换蛇皮）═══
            case Keys.Tab:
                _skinManager.NextSnakeSkin();
                StateChanged?.Invoke();
                return true;

            // ═══ AI 模式 ═══
            case Keys.F2:
                _engine.ToggleAI();
                AIToggled?.Invoke();
                StateChanged?.Invoke();
                return true;

            // ═══ 难度切换 ═══
            case Keys.D1:
            case Keys.NumPad1:
                _engine.SetDifficulty(DifficultyLevel.Slow);
                StateChanged?.Invoke();
                return true;

            case Keys.D2:
            case Keys.NumPad2:
                _engine.SetDifficulty(DifficultyLevel.Medium);
                StateChanged?.Invoke();
                return true;

            case Keys.D3:
            case Keys.NumPad3:
                _engine.SetDifficulty(DifficultyLevel.Fast);
                StateChanged?.Invoke();
                return true;

            // ═══ 音乐开关 ═══
            case Keys.M:
                _engine.ToggleMusic();
                StateChanged?.Invoke();
                return true;

            // ═══ 排行榜 ═══
            case Keys.L:
                // 由 MainForm 订阅此事件
                return false; // 不标记为已处理，让 MainForm 处理
        }

        return false;
    }
}
