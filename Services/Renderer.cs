using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using SnakeGame.Models;

namespace SnakeGame.Services;

public class Renderer : IDisposable
{
    private readonly SkinManager _skinManager;

    // 字体
    private readonly Font _gameFont;
    private readonly Font _statusFont;
    private readonly Font _hintFont;
    private readonly Font _pauseFont;
    private readonly Font _pauseHintFont;

    // 图片缓存
    private readonly Dictionary<string, SkinImageSet> _snakeImageCache = new();
    private readonly List<Bitmap> _foodImages = new();
    private readonly Random _foodRandom = new();

    private bool _disposed;

    public Renderer(SkinManager skinManager)
    {
        _skinManager = skinManager;
        _gameFont = new Font("Consolas", GameConfig.CellSize - 4, FontStyle.Bold);
        _statusFont = new Font("微软雅黑", 10.5f, FontStyle.Bold);
        _hintFont = new Font("微软雅黑", 9);
        _pauseFont = new Font("微软雅黑", 36, FontStyle.Bold);
        _pauseHintFont = new Font("微软雅黑", 14);
        LoadFoodImages();
    }

    // ═══════════════════════════════════════════
    // 图片加载
    // ═══════════════════════════════════════════

    private static string ThemesDir =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes");

    private void LoadFoodImages()
    {
        for (int i = 1; i <= 4; i++)
        {
            string path = Path.Combine(ThemesDir, $"food{i}.png");
            if (File.Exists(path))
                try { _foodImages.Add(new Bitmap(path)); } catch { }
        }
    }

    private SkinImageSet LoadSnakeImages(string headFile, string bodyFile)
    {
        string key = $"{headFile}|{bodyFile}";
        if (_snakeImageCache.TryGetValue(key, out var cached))
            return cached;

        var set = new SkinImageSet();

        if (File.Exists(bodyFile))
            try { set.Body = new Bitmap(bodyFile); } catch { }

        Bitmap? headBase = null;
        if (File.Exists(headFile))
            try { headBase = new Bitmap(headFile); } catch { }

        if (headBase != null)
        {
            set.HeadRight = headBase;
            set.HeadDown  = CloneAndRotate(headBase, RotateFlipType.Rotate90FlipNone);
            set.HeadLeft  = CloneAndRotate(headBase, RotateFlipType.Rotate180FlipNone);
            set.HeadUp    = CloneAndRotate(headBase, RotateFlipType.Rotate270FlipNone);
            // 身体四方向（右0° / 下90° / 左180° / 上270°）
            var bodyBase = set.Body ?? headBase;
            set.BodyRight = bodyBase;
            set.BodyDown  = CloneAndRotate(bodyBase, RotateFlipType.Rotate90FlipNone);
            set.BodyLeft  = CloneAndRotate(bodyBase, RotateFlipType.Rotate180FlipNone);
            set.BodyUp    = CloneAndRotate(bodyBase, RotateFlipType.Rotate270FlipNone);
        }

        _snakeImageCache[key] = set;
        return set;
    }

    private static Bitmap CloneAndRotate(Bitmap src, RotateFlipType rot)
    {
        var clone = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(clone);
        g.Clear(Color.Transparent);
        g.DrawImage(src, 0, 0);
        clone.RotateFlip(rot);
        return clone;
    }

    // ═══════════════════════════════════════════
    // 主绘制入口
    // ═══════════════════════════════════════════

    public void DrawGame(Graphics g, Snake snake, Food food, GameState state, Size clientSize)
    {
        var map = _skinManager.CurrentMap;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(map.BackgroundColor);

        int ox = 20;
        int oy = GameConfig.HeaderHeight;

        // 1. 游戏区域背景
        using (var gridBrush = new SolidBrush(map.GridBackgroundColor))
            g.FillRectangle(gridBrush, ox, oy,
                GameConfig.GridWidth * GameConfig.CellSize,
                GameConfig.GridHeight * GameConfig.CellSize);

        // 2. 网格线（地图主题色）
        DrawGridLines(g, ox, oy, map);

        // 3. 障碍物
        DrawObstacles(g, state.Obstacles, ox, oy, map);

        // 4. 蛇（当前蛇皮的图片）
        DrawSnake(g, snake, ox, oy);

        // 5. 食物
        DrawFood(g, food, ox, oy);

        // 6. 暂停遮罩
        if (state.IsPaused)
            DrawPauseOverlay(g, clientSize);
    }

    // ═══════════════════════════════════════════
    // 网格线
    // ═══════════════════════════════════════════

    private void DrawGridLines(Graphics g, int ox, int oy, SkinTheme map)
    {
        using var gridPen = new Pen(map.GridLineColor);
        int w = GameConfig.GridWidth * GameConfig.CellSize;
        int h = GameConfig.GridHeight * GameConfig.CellSize;
        for (int x = 0; x <= GameConfig.GridWidth; x++)
            g.DrawLine(gridPen, ox + x * GameConfig.CellSize, oy,
                ox + x * GameConfig.CellSize, oy + h);
        for (int y = 0; y <= GameConfig.GridHeight; y++)
            g.DrawLine(gridPen, ox, oy + y * GameConfig.CellSize,
                ox + w, oy + y * GameConfig.CellSize);
    }

    // ═══════════════════════════════════════════
    // 障碍物
    // ═══════════════════════════════════════════

    private void DrawObstacles(Graphics g, List<Point> obstacles, int ox, int oy, SkinTheme map)
    {
        if (obstacles.Count == 0) return;
        int cell = GameConfig.CellSize;
        using Brush obsBrush = new SolidBrush(map.ObstacleColor);
        foreach (var obs in obstacles)
            g.FillRectangle(obsBrush, ox + obs.X * cell + 2, oy + obs.Y * cell + 2, cell - 4, cell - 4);
    }

    // ═══════════════════════════════════════════
    // 蛇 — 图片模式，身体按方向旋转
    // ═══════════════════════════════════════════

    private void DrawSnake(Graphics g, Snake snake, int ox, int oy)
    {
        if (snake.Length == 0) return;

        string headFile = _skinManager.CurrentHeadFile;
        string bodyFile = _skinManager.CurrentBodyFile;
        var imgSet = LoadSnakeImages(headFile, bodyFile);

        int cell = GameConfig.CellSize;

        // 从尾到头绘制
        for (int i = snake.Length - 1; i >= 0; i--)
        {
            Point seg = snake.Body[i];
            int x = ox + seg.X * cell;
            int y = oy + seg.Y * cell;

            if (i == 0)
            {
                // 蛇头 — 按当前移动方向旋转
                var headImg = GetHeadForDirection(imgSet, snake.Direction);
                g.DrawImage(headImg, x, y, cell, cell);
            }
            else
            {
                // 身体 — 按该段方向旋转
                Point prev = snake.Body[i - 1];
                Point bodyDir = new Point(prev.X - seg.X, prev.Y - seg.Y);
                var bodyImg = GetBodyForDirection(imgSet, bodyDir);
                g.DrawImage(bodyImg, x, y, cell, cell);
            }
        }
    }

    private static Bitmap GetHeadForDirection(SkinImageSet set, Point dir)
    {
        if (dir.X == 1)  return set.HeadRight!;
        if (dir.X == -1) return set.HeadLeft!;
        if (dir.Y == -1) return set.HeadUp!;
        return set.HeadDown!; // dir.Y == 1
    }

    private static Bitmap GetBodyForDirection(SkinImageSet set, Point dir)
    {
        if (dir.X == 1)  return set.BodyRight!;
        if (dir.X == -1) return set.BodyLeft!;
        if (dir.Y == -1) return set.BodyUp!;
        return set.BodyDown!; // dir.Y == 1
    }

    // ═══════════════════════════════════════════
    // 食物 — 随机图片
    // ═══════════════════════════════════════════

    private void DrawFood(Graphics g, Food food, int ox, int oy)
    {
        if (_foodImages.Count == 0) return;
        int idx = Math.Abs(food.Position.GetHashCode()) % _foodImages.Count;
        int cell = GameConfig.CellSize;
        int x = ox + food.Position.X * cell;
        int y = oy + food.Position.Y * cell;
        g.DrawImage(_foodImages[idx], x, y, cell, cell);
    }

    // ═══════════════════════════════════════════
    // 状态栏（不重叠地图）
    // ═══════════════════════════════════════════

    /// <summary>
    /// 状态栏 — 3行×3列 网格布局
    /// </summary>
    public void DrawStatusBarEx(Graphics g, GameState state, int snakeLength,
        string? musicFileName, bool isMusicEnabled, Size clientSize)
    {
        var map = _skinManager.CurrentMap;
        int hdrH = GameConfig.HeaderHeight;

        // 深色背景
        using (var headerBrush = new SolidBrush(map.StatusBarBackground))
            g.FillRectangle(headerBrush, 0, 0, clientSize.Width, hdrH);

        string diffText = state.CurrentDifficulty switch
        {
            DifficultyLevel.Slow => "慢",
            DifficultyLevel.Medium => "中",
            DifficultyLevel.Fast => "快",
            _ => "中"
        };

        // 音乐状态
        string musicStatus;
        Color musicColor;
        if (string.IsNullOrEmpty(musicFileName))
        { musicStatus = "无"; musicColor = Color.Red; }
        else
        { musicStatus = isMusicEnabled ? "开" : "关"; musicColor = isMusicEnabled ? Color.LightGreen : Color.Gray; }

        // ─── 3列坐标 ───
        float c1 = 20, c2 = 225, c3 = 430;

        float y1 = 10, y2 = 42, y3 = 74, y4 = 106;

        // Row 1
        DrawStatusItem(g, $"分数: {state.Score}", map.ScoreColor, c1, y1);
        DrawStatusItem(g, $"最高: {state.HighScore}", map.HighScoreColor, c2, y1);
        DrawStatusItem(g, $"长度: {snakeLength}", map.LengthColor, c3, y1);

        // Row 2
        DrawStatusItem(g, $"Lv.{state.Level}", map.DifficultyColor, c1, y2);
        DrawStatusItem(g, $"速度: {diffText} (1/2/3)", map.DifficultyColor, c2, y2);
        DrawStatusItem(g, $"蛇皮: {_skinManager.SnakeSkinName}", map.DifficultyColor, c3, y2);

        // Row 3
        DrawStatusItem(g, $"音乐: {musicStatus} (M)", musicColor, c1, y3);
        DrawStatusItem(g, $"地图: {map.Name}", map.DifficultyColor, c2, y3);
        if (state.IsAIMode)
        {
            using var aiBrush = new SolidBrush(Color.Cyan);
            g.DrawString("[AI 演示]", _statusFont, aiBrush, c3, y3);
        }

        // Row 4 — 操作提示
        using (var hintBrush = new SolidBrush(map.HintColor))
        {
            g.DrawString("WASD/方向键:移动  空格:暂停  Tab:蛇皮  F2:AI  R:重置  M:音乐  L:排行",
                _hintFont, hintBrush, c1, y4);
        }

        // 底部分隔线
        using (var linePen = new Pen(map.SeparatorColor, 1))
            g.DrawLine(linePen, 0, hdrH, clientSize.Width, hdrH);
    }

    private void DrawStatusItem(Graphics g, string text, Color color, float x, float y)
    {
        using var brush = new SolidBrush(color);
        g.DrawString(text, _statusFont, brush, x, y);
    }

    // ═══════════════════════════════════════════
    // 边界 — 红色色块（包围网格区域）
    // ═══════════════════════════════════════════

    public void DrawBorder(Graphics g, Size clientSize)
    {
        int cell = GameConfig.CellSize;
        int ox = 20;
        int oy = GameConfig.HeaderHeight;
        int w = GameConfig.GridWidth * cell;
        int h = GameConfig.GridHeight * cell;
        int t = cell / 2; // 半格粗

        using Brush borderBrush = new SolidBrush(Color.FromArgb(200, 200, 50, 50));

        // 上（含两角）
        g.FillRectangle(borderBrush, ox - t, oy, w + t * 2, t);
        // 下（含两角）
        g.FillRectangle(borderBrush, ox - t, oy + h, w + t * 2, t);
        // 左（含两角）
        g.FillRectangle(borderBrush, ox - t, oy, t, h + t);
        // 右（含两角）
        g.FillRectangle(borderBrush, ox + w, oy, t, h + t);
    }

    // ═══════════════════════════════════════════
    // 暂停遮罩
    // ═══════════════════════════════════════════

    private void DrawPauseOverlay(Graphics g, Size clientSize)
    {
        using var overlayBrush = new SolidBrush(Color.FromArgb(180, 0, 0, 0));
        g.FillRectangle(overlayBrush, 0, 0, clientSize.Width, clientSize.Height);

        var sf = CreateCenterFormat();
        using var textBrush = new SolidBrush(Color.Yellow);
        g.DrawString("游戏暂停", _pauseFont, textBrush,
            clientSize.Width / 2, clientSize.Height / 2, sf);
        using var hintBrush = new SolidBrush(Color.White);
        g.DrawString("按空格键继续", _pauseHintFont, hintBrush,
            clientSize.Width / 2, clientSize.Height / 2 + 50, sf);
    }

    // ═══════════════════════════════════════════
    // 辅助
    // ═══════════════════════════════════════════

    private static StringFormat CreateCenterFormat() => new()
    {
        Alignment = StringAlignment.Center,
        LineAlignment = StringAlignment.Center
    };

    // ═══════════════════════════════════════════
    // Dispose
    // ═══════════════════════════════════════════

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _gameFont.Dispose();
        _statusFont.Dispose();
        _hintFont.Dispose();
        _pauseFont.Dispose();
        _pauseHintFont.Dispose();
        foreach (var set in _snakeImageCache.Values) set.Dispose();
        _snakeImageCache.Clear();
        foreach (var bmp in _foodImages) bmp.Dispose();
        _foodImages.Clear();
    }

    // ═══════════════════════════════════════════
    // 图片缓存
    // ═══════════════════════════════════════════

    private class SkinImageSet : IDisposable
    {
        public Bitmap? Body;
        public Bitmap? HeadRight, HeadDown, HeadLeft, HeadUp;
        public Bitmap BodyRight = null!, BodyDown = null!, BodyLeft = null!, BodyUp = null!;

        public void Dispose()
        {
            Body?.Dispose();
            HeadRight?.Dispose();
            HeadDown?.Dispose();
            HeadLeft?.Dispose();
            HeadUp?.Dispose();
            BodyRight?.Dispose();
            BodyDown?.Dispose();
            BodyLeft?.Dispose();
            BodyUp?.Dispose();
        }
    }
}
