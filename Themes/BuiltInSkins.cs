using SnakeGame.Models;

namespace SnakeGame.Themes;

/// <summary>
/// 内置皮肤主题定义 — 每套皮肤使用对应的 headX.png / bodyX.png
/// </summary>
public static class BuiltInSkins
{
    private static string ThemePath(string file) =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes", file);

    public static List<SkinTheme> All { get; } = new()
    {
        Create("经典", "head1.png", "body1.png",
            Color.Black, Color.DarkSlateGray, Color.Gray),

        Create("霓虹", "head2.png", "body2.png",
            Color.FromArgb(10, 10, 46), Color.FromArgb(20, 20, 60), Color.Yellow),

        Create("海洋", "head3.png", "body3.png",
            Color.FromArgb(0, 26, 51), Color.FromArgb(0, 40, 70), Color.White),

        Create("复古", "head4.png", "body4.png",
            Color.FromArgb(26, 26, 0), Color.FromArgb(30, 30, 5), Color.Gold),

        Create("丛林", "head5.png", "body5.png",
            Color.FromArgb(13, 26, 13), Color.FromArgb(20, 40, 15), Color.FromArgb(139, 90, 43)),
    };

    private static SkinTheme Create(string name, string headFile, string bodyFile,
        Color bg, Color gridBg, Color wallColor) => new()
    {
        Name = name,
        HeadImageFile = ThemePath(headFile),
        BodyImageFile = ThemePath(bodyFile),
        BackgroundColor = bg,
        GridBackgroundColor = gridBg,
        GridLineColor = Color.FromArgb(50, 100, 100, 100),
        WallColor = wallColor,
        WallChar = "#",
        ObstacleColor = Color.DarkRed,
        ObstacleChar = "X",
        StatusBarBackground = Color.FromArgb(40, 40, 40),
        ScoreColor = Color.Gold,
        HighScoreColor = Color.Orange,
        LengthColor = Color.LightGreen,
        DifficultyColor = Color.Cyan,
        HintColor = Color.LightGray,
        SeparatorColor = Color.Gray,
    };
}
