namespace SnakeGame.Models;

/// <summary>
/// 蛇的数据模型与基本操作
/// </summary>
public class Snake
{
    public List<Point> Body { get; private set; } = new();
    public Point Direction { get; set; }
    public Point NextDirection { get; set; }

    public Point Head => Body.Count > 0 ? Body[0] : Point.Empty;
    public int Length => Body.Count;

    public Snake()
    {
        Direction = new Point(1, 0);
        NextDirection = new Point(1, 0);
    }

    /// <summary>
    /// 重置蛇到初始位置
    /// </summary>
    public void Reset()
    {
        Body.Clear();
        int startX = GameConfig.GridWidth / 2;
        int startY = GameConfig.GridHeight / 2;
        for (int i = 0; i < GameConfig.InitialSnakeLength; i++)
        {
            Body.Add(new Point(startX - i, startY));
        }
        Direction = new Point(1, 0);
        NextDirection = new Point(1, 0);
    }

    /// <summary>
    /// 移动蛇一步。
    /// </summary>
    /// <param name="grow">true 表示吃到食物，蛇尾不删除</param>
    public void Move(bool grow)
    {
        Direction = NextDirection;
        Point newHead = new Point(Head.X + Direction.X, Head.Y + Direction.Y);
        Body.Insert(0, newHead);
        if (!grow)
        {
            Body.RemoveAt(Body.Count - 1);
        }
    }

    /// <summary>
    /// 检查某坐标是否被蛇身占据
    /// </summary>
    public bool Occupies(Point p) => Body.Contains(p);

    /// <summary>
    /// 计算按当前 NextDirection 移动后的新头部位置（不实际移动）
    /// </summary>
    public Point PeekNewHead()
    {
        return new Point(Head.X + NextDirection.X, Head.Y + NextDirection.Y);
    }

    /// <summary>
    /// 检查是否与自身碰撞（排除尾端，因为尾端即将移走）
    /// </summary>
    public bool WouldCollideWithSelf(Point newHead)
    {
        for (int i = 0; i < Body.Count - 1; i++)
        {
            if (Body[i] == newHead) return true;
        }
        return false;
    }
}
