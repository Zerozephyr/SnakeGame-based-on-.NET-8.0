namespace SnakeGame.Models;

/// <summary>
/// 食物模型 — 位置与颜色
/// </summary>
public class Food
{
    public Point Position { get; private set; }
    public Color Color { get; private set; }

    /// <summary>
    /// 在网格中随机生成一个不被蛇身或障碍物占据的位置
    /// </summary>
    public void Generate(Random random, Snake snake, List<Point> obstacles)
    {
        Point newPos;
        int maxAttempts = GameConfig.GridWidth * GameConfig.GridHeight;
        int attempts = 0;
        do
        {
            newPos = new Point(
                random.Next(GameConfig.GridWidth),
                random.Next(GameConfig.GridHeight));
            attempts++;
        } while ((snake.Occupies(newPos) || obstacles.Contains(newPos)) && attempts < maxAttempts);

        Position = newPos;
        Color = Color.FromArgb(
            random.Next(100, 256),
            random.Next(100, 256),
            random.Next(100, 256));
    }
}
