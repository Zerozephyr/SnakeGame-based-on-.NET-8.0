using SnakeGame.Models;

namespace SnakeGame.Services;

/// <summary>
/// AI 寻路器 — 使用 BFS 找到安全的食物路径
/// </summary>
public class AIPathfinder
{
    private readonly int _gridWidth;
    private readonly int _gridHeight;

    // 四方向：上、下、左、右
    private static readonly Point[] Directions = new[]
    {
        new Point(0, -1), new Point(0, 1),
        new Point(-1, 0), new Point(1, 0),
    };

    public AIPathfinder(int gridWidth, int gridHeight)
    {
        _gridWidth = gridWidth;
        _gridHeight = gridHeight;
    }

    /// <summary>
    /// 找到下一个安全移动方向。返回 null 表示无安全方向。
    /// </summary>
    public Point? FindSafeDirection(Snake snake, Point food, List<Point> obstacles)
    {
        if (snake.Length == 0) return null;

        // 1. 尝试找到前往食物的安全路径
        Point? foodDir = FindPathToTarget(snake, food, obstacles);
        if (foodDir.HasValue)
            return foodDir.Value;

        // 2. 无食物路径时，尝试追蛇尾（生存策略）
        if (snake.Length > 1)
        {
            Point tail = snake.Body[^1];
            Point? tailDir = FindPathToTarget(snake, tail, obstacles);
            if (tailDir.HasValue)
                return tailDir.Value;
        }

        // 3. 最后选择任意安全方向
        return FindAnySafeDirection(snake, obstacles);
    }

    /// <summary>
    /// BFS 寻找到目标的最短路径，返回第一步方向
    /// </summary>
    private Point? FindPathToTarget(Snake snake, Point target, List<Point> obstacles)
    {
        var blocked = BuildBlockedSet(snake, obstacles);
        var start = snake.Head;

        // BFS
        var visited = new Dictionary<Point, Point?>(); // cell -> parent
        var queue = new Queue<Point>();
        queue.Enqueue(start);
        visited[start] = null;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var dir in Directions)
            {
                var next = new Point(current.X + dir.X, current.Y + dir.Y);

                if (visited.ContainsKey(next)) continue;
                if (blocked.Contains(next)) continue;
                if (!IsInBounds(next)) continue;

                visited[next] = current;
                queue.Enqueue(next);

                if (next == target)
                {
                    // 回溯找到第一步
                    return BacktrackFirstStep(start, next, visited);
                }
            }
        }

        return null; // 无路径
    }

    /// <summary>
    /// 找到任意一个安全的移动方向
    /// </summary>
    private Point? FindAnySafeDirection(Snake snake, List<Point> obstacles)
    {
        var blocked = BuildBlockedSet(snake, obstacles);
        var head = snake.Head;

        foreach (var dir in Directions)
        {
            var next = new Point(head.X + dir.X, head.Y + dir.Y);
            if (!blocked.Contains(next) && IsInBounds(next))
            {
                // 安全检查：移动后是否能到达足够多的空格
                if (IsMoveSafe(snake, dir, obstacles))
                    return dir;
            }
        }

        // 绝望情况：返回任意可行方向
        foreach (var dir in Directions)
        {
            var next = new Point(head.X + dir.X, head.Y + dir.Y);
            if (IsInBounds(next) && !obstacles.Contains(next) && !snake.Occupies(next))
                return dir;
        }

        return Directions[0]; // 最终回退
    }

    /// <summary>
    /// 构建不可通过的格子集合
    /// </summary>
    private HashSet<Point> BuildBlockedSet(Snake snake, List<Point> obstacles)
    {
        var blocked = new HashSet<Point>();

        // 加入蛇身（排除尾端，因为它即将移走）
        for (int i = 0; i < snake.Body.Count - 1; i++)
        {
            blocked.Add(snake.Body[i]);
        }

        // 加入障碍物
        foreach (var obs in obstacles)
        {
            blocked.Add(obs);
        }

        return blocked;
    }

    /// <summary>
    /// Flood-fill 验证：移动后从新头部可达的空格数 >= 蛇长
    /// </summary>
    private bool IsMoveSafe(Snake snake, Point direction, List<Point> obstacles)
    {
        var head = snake.Head;
        var newHead = new Point(head.X + direction.X, head.Y + direction.Y);

        // 模拟移动后的蛇身
        var futureBody = new List<Point> { newHead };
        for (int i = 0; i < snake.Body.Count - 1; i++)
        {
            futureBody.Add(snake.Body[i]);
        }

        var blocked = new HashSet<Point>(futureBody);
        foreach (var obs in obstacles)
            blocked.Add(obs);

        // Flood-fill 从 newHead 出发
        var visited = new HashSet<Point>();
        var queue = new Queue<Point>();
        queue.Enqueue(newHead);
        visited.Add(newHead);

        int reachable = 0;
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            reachable++;

            foreach (var dir in Directions)
            {
                var next = new Point(current.X + dir.X, current.Y + dir.Y);
                if (IsInBounds(next) && !blocked.Contains(next) && !visited.Contains(next))
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
        }

        return reachable >= snake.Length;
    }

    private bool IsInBounds(Point p) =>
        p.X >= 0 && p.X < _gridWidth && p.Y >= 0 && p.Y < _gridHeight;

    /// <summary>
    /// 从 BFS visited 字典回溯到第一步
    /// </summary>
    private Point BacktrackFirstStep(Point start, Point end, Dictionary<Point, Point?> visited)
    {
        var path = new List<Point>();
        var current = end;

        while (current != start)
        {
            path.Add(current);
            current = visited[current]!.Value;
        }
        path.Reverse();

        if (path.Count == 0) return Directions[0];

        var firstStep = path[0];
        return new Point(firstStep.X - start.X, firstStep.Y - start.Y);
    }
}
