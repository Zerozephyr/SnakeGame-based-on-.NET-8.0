namespace SnakeGame.Services;

/// <summary>
/// 最高分文件的读写服务
/// </summary>
public class ScoreService
{
    private readonly string _highScoreFile;

    public ScoreService(string? filePath = null)
    {
        _highScoreFile = filePath ?? Path.Combine(Application.StartupPath, "highscore.txt");
    }

    /// <summary>
    /// 从文件加载最高分
    /// </summary>
    public int LoadHighScore()
    {
        try
        {
            if (File.Exists(_highScoreFile))
            {
                string content = File.ReadAllText(_highScoreFile);
                if (int.TryParse(content, out int savedScore))
                {
                    return savedScore;
                }
            }
        }
        catch { }
        return 0;
    }

    /// <summary>
    /// 将最高分保存到文件
    /// </summary>
    public void SaveHighScore(int score)
    {
        try
        {
            File.WriteAllText(_highScoreFile, score.ToString());
        }
        catch { }
    }
}
