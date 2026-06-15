using SnakeGame.Models;

namespace SnakeGame.Services;

/// <summary>
/// 蛇皮和地图独立管理
///   - 蛇皮 (Tab 切换)：headX.png + bodyX.png 5 组
///   - 地图 (关卡升级时自动切换)：背景色、网格色、墙壁色等
/// </summary>
public class SkinManager
{
    // ── 蛇皮定义 ──
    private static readonly string ThemeDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes");

    public static readonly (string Name, string HeadFile, string BodyFile)[] SnakeSkins =
    {
        ("经典蛇", Path.Combine(ThemeDir, "head1.png"), Path.Combine(ThemeDir, "body1.png")),
        ("霓虹蛇", Path.Combine(ThemeDir, "head2.png"), Path.Combine(ThemeDir, "body2.png")),
        ("岩浆蛇", Path.Combine(ThemeDir, "head3.png"), Path.Combine(ThemeDir, "body3.png")),
        ("冰霜蛇", Path.Combine(ThemeDir, "head4.png"), Path.Combine(ThemeDir, "body4.png")),
        ("金龙蛇", Path.Combine(ThemeDir, "head5.png"), Path.Combine(ThemeDir, "body5.png")),
    };

    private int _snakeSkinIndex;

    // ── 地图定义 ──
    private readonly List<SkinTheme> _mapThemes;
    private int _mapIndex;

    public SkinManager()
    {
        _mapThemes = Themes.BuiltInSkins.All;
        _mapIndex = 0;
        _snakeSkinIndex = 0;
    }

    // ── 蛇皮 ──
    public int SnakeSkinIndex => _snakeSkinIndex;
    public string SnakeSkinName => SnakeSkins[_snakeSkinIndex].Name;
    public string CurrentHeadFile => SnakeSkins[_snakeSkinIndex].HeadFile;
    public string CurrentBodyFile => SnakeSkins[_snakeSkinIndex].BodyFile;
    public int SnakeSkinCount => SnakeSkins.Length;

    public void NextSnakeSkin()
    {
        _snakeSkinIndex = (_snakeSkinIndex + 1) % SnakeSkins.Length;
    }

    // ── 地图 ──
    public SkinTheme CurrentMap => _mapThemes[_mapIndex];
    public int MapIndex => _mapIndex;
    public int MapCount => _mapThemes.Count;

    /// <summary>关卡升级时切换地图</summary>
    public void NextMap()
    {
        _mapIndex = (_mapIndex + 1) % _mapThemes.Count;
    }
}
