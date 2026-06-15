namespace SnakeGame.Models;

/// <summary>
/// 皮肤主题 — 定义所有视觉参数
/// </summary>
public class SkinTheme
{
    public string Name { get; init; } = "默认";

    // 背景
    public Color BackgroundColor { get; init; } = Color.Black;
    public Color GridBackgroundColor { get; init; } = Color.DarkSlateGray;

    // 网格线
    public Color GridLineColor { get; init; } = Color.FromArgb(50, 100, 100, 100);

    // 墙壁
    public Color WallColor { get; init; } = Color.Gray;
    public string WallChar { get; init; } = "#";

    // 蛇 — 图片模式（优先）
    public string? HeadImageFile { get; init; }   // e.g., "head1.png"
    public string? BodyImageFile { get; init; }   // e.g., "body1.png"

    // 蛇 — 字符模式（图片为空时的回退）
    public string SnakeChar { get; init; } = "*";
    public Color SnakeHeadColor { get; init; } = Color.DarkGreen;
    public Color SnakeBodyStart { get; init; } = Color.FromArgb(34, 139, 34);
    public Color SnakeBodyEnd { get; init; } = Color.FromArgb(134, 189, 134);

    // 食物
    public string FoodChar { get; init; } = "@";
    public Color FoodColorMin { get; init; } = Color.FromArgb(100, 0, 0);
    public Color FoodColorMax { get; init; } = Color.FromArgb(255, 255, 255);

    // 障碍物
    public Color ObstacleColor { get; init; } = Color.DarkRed;
    public string ObstacleChar { get; init; } = "X";

    // 状态栏
    public Color StatusBarBackground { get; init; } = Color.FromArgb(40, 40, 40);
    public Color ScoreColor { get; init; } = Color.Gold;
    public Color HighScoreColor { get; init; } = Color.Orange;
    public Color LengthColor { get; init; } = Color.LightGreen;
    public Color DifficultyColor { get; init; } = Color.Cyan;
    public Color HintColor { get; init; } = Color.LightGray;
    public Color SeparatorColor { get; init; } = Color.Gray;
}
