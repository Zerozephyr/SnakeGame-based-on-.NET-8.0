using SnakeGame.Models;
using SnakeGame.Services;

namespace SnakeGame.UI;

/// <summary>
/// 主游戏窗体 — 薄编排层，协调各服务
/// </summary>
public partial class MainForm : Form
{
    private readonly GameEngine _engine;
    private readonly Renderer _renderer;
    private readonly InputHandler _inputHandler;
    private readonly ScoreService _scoreService;
    private readonly MusicService _musicService;
    private readonly SoundService _soundService;
    private readonly SkinManager _skinManager;
    private readonly LeaderboardService _leaderboardService;

    private System.Windows.Forms.Timer _gameTimer;

    public MainForm()
    {
        InitializeComponent();

        // 初始化所有服务
        _skinManager = new SkinManager();
        _scoreService = new ScoreService();
        _musicService = new MusicService();
        _soundService = new SoundService();
        _engine = new GameEngine();
        _renderer = new Renderer(_skinManager);
        _inputHandler = new InputHandler(_engine, _skinManager);
        _leaderboardService = new LeaderboardService();

        // 加载最高分
        _engine.State.HighScore = _scoreService.LoadHighScore();

        // 初始化游戏
        InitializeGame();

        // 设置计时器
        _gameTimer = new System.Windows.Forms.Timer();
        _gameTimer.Tick += GameTimer_Tick;
        _gameTimer.Interval = GameConfig.InitialSpeed;

        // 绑定事件
        this.Paint += MainForm_Paint;
        this.KeyDown += MainForm_KeyDown;
        this.Load += MainForm_Load;
        this.FormClosing += MainForm_FormClosing;

        // 订阅引擎事件
        _engine.StateChanged += () => this.Invalidate();
        _engine.FoodEaten += () =>
        {
            _soundService.PlayEat();
            if (_engine.State.Score > _engine.State.HighScore)
            {
                _engine.State.HighScore = _engine.State.Score;
                _scoreService.SaveHighScore(_engine.State.HighScore);
            }
        };
        _engine.LevelUp += () =>
        {
            _soundService.PlayLevelUp();
            _skinManager.NextMap(); // 关卡升级 → 自动换地图
        };
        _engine.GameOver += () =>
        {
            _soundService.PlayDeath();
            _ = OnGameOverAsync();
        };
        _engine.MusicToggled += (enabled) =>
        {
            _musicService.IsMusicEnabled = enabled;
        };
    }

    // ═══════════════════════════════════════════
    // 窗体事件
    // ═══════════════════════════════════════════

    private void MainForm_Load(object? sender, EventArgs e)
    {
        _musicService.Initialize();
        ShowStartMessage();
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _engine.StopGame();
        _musicService.Dispose();
        _soundService.Dispose();
        _renderer.Dispose();
        _leaderboardService.Dispose();
    }

    private void MainForm_Paint(object? sender, PaintEventArgs e)
    {
        _renderer.DrawGame(e.Graphics, _engine.Snake, _engine.CurrentFood, _engine.State, this.ClientSize);
        _renderer.DrawStatusBarEx(
            e.Graphics, _engine.State, _engine.Snake.Length,
            _musicService.CurrentMusicFile, _musicService.IsMusicEnabled,
            this.ClientSize);
        _renderer.DrawBorder(e.Graphics, this.ClientSize);
    }

    private async void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        bool handled = _inputHandler.ProcessKey(e.KeyCode);

        // L 键打开排行榜
        if (e.KeyCode == Keys.L && !handled)
        {
            await OpenLeaderboardAsync();
            handled = true;
        }

        if (handled)
        {
            _gameTimer.Interval = _engine.State.CurrentSpeed;
        }

        e.Handled = handled;
    }

    private void GameTimer_Tick(object? sender, EventArgs e)
    {
        _engine.Tick();
    }

    // ═══════════════════════════════════════════
    // 初始化和流程
    // ═══════════════════════════════════════════

    private void InitializeGame()
    {
        this.Text = "贪吃蛇游戏";
        this.ClientSize = new Size(
            GameConfig.GridWidth * GameConfig.CellSize + 40,
            GameConfig.GridHeight * GameConfig.CellSize + GameConfig.HeaderHeight + 40);
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.BackColor = Color.Black;
        this.DoubleBuffered = true;
        this.KeyPreview = true;

        _engine.Reset();
    }

    private void ShowStartMessage()
    {
        string musicMsg = _musicService.CurrentMusicFile != null
            ? $"检测到音乐文件: {Path.GetFileName(_musicService.CurrentMusicFile)}"
            : "未检测到音乐文件";

        MessageBox.Show(
            $"欢迎使用贪吃蛇游戏！\n\n{musicMsg}\n\n" +
            "操作说明：\n" +
            "WASD / 方向键 — 控制移动\n" +
            "空格键 — 暂停/继续\n" +
            "Tab — 切换蛇皮\n" +
            "F2 — 切换 AI 演示模式\n" +
            "1/2/3 — 切换难度（慢/中/快）\n" +
            "R — 重新开始\n" +
            "M — 开启/关闭音乐\n" +
            "L — 排行榜\n\n" +
            "按确定开始游戏！",
            "游戏说明",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        _engine.StartGame();
        _gameTimer.Start();
        _musicService.Play();
    }

    private async Task OnGameOverAsync()
    {
        _gameTimer.Stop();
        _musicService.Stop();

        // 确保最高分已保存
        if (_engine.State.Score > _engine.State.HighScore)
        {
            _engine.State.HighScore = _engine.State.Score;
            _scoreService.SaveHighScore(_engine.State.HighScore);
        }

        // 构建游戏结束信息
        string gameOverMsg =
            $"游戏结束！\n\n" +
            $"得分: {_engine.State.Score}\n" +
            $"最高分: {_engine.State.HighScore}\n" +
            $"蛇长度: {_engine.Snake.Length}\n" +
            $"等级: {_engine.State.Level}\n\n";

        // 如果有分数，询问是否提交排行榜
        if (_engine.State.Score > 0)
        {
            gameOverMsg += "是否将分数提交到排行榜？\n\n点击 [是] 提交分数 | 点击 [否] 重新开始 | 点击 [取消] 退出";
        }
        else
        {
            gameOverMsg += "是否重新开始？";
        }

        MessageBoxButtons buttons = _engine.State.Score > 0
            ? MessageBoxButtons.YesNoCancel
            : MessageBoxButtons.YesNo;

        DialogResult result = MessageBox.Show(
            gameOverMsg, "游戏结束", buttons, MessageBoxIcon.Information);

        if (result == DialogResult.Yes)
        {
            if (_engine.State.Score > 0)
            {
                // 提交分数到排行榜
                await SubmitScoreToLeaderboardAsync();
            }

            // 重新开始
            _engine.Reset();
            _engine.StartGame();
            _gameTimer.Start();
            _musicService.Play();
        }
        else if (result == DialogResult.No && _engine.State.Score > 0)
        {
            // 不提交，但重新开始
            _engine.Reset();
            _engine.StartGame();
            _gameTimer.Start();
            _musicService.Play();
        }
        else
        {
            this.Close();
        }
    }

    // ═══════════════════════════════════════════
    // 排行榜
    // ═══════════════════════════════════════════

    private async Task SubmitScoreToLeaderboardAsync()
    {
        // 请求玩家昵称
        string playerName = "Player";

        // 使用简单的 InputBox 样式获取昵称
        using var inputForm = new Form
        {
            Text = "提交分数",
            ClientSize = new Size(350, 160),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.FromArgb(40, 40, 40),
            ForeColor = Color.White,
        };

        var label = new Label
        {
            Text = "请输入你的昵称：",
            Location = new Point(20, 20),
            AutoSize = true,
            Font = new Font("微软雅黑", 11),
            ForeColor = Color.White,
        };
        inputForm.Controls.Add(label);

        var textBox = new TextBox
        {
            Location = new Point(20, 55),
            Size = new Size(300, 25),
            Font = new Font("微软雅黑", 11),
            Text = playerName,
            MaxLength = 20,
        };
        inputForm.Controls.Add(textBox);

        var okButton = new Button
        {
            Text = "提交",
            Location = new Point(140, 95),
            Size = new Size(90, 30),
            DialogResult = DialogResult.OK,
            BackColor = Color.FromArgb(60, 120, 60),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
        };
        okButton.FlatAppearance.BorderColor = Color.Green;
        inputForm.Controls.Add(okButton);

        var cancelButton = new Button
        {
            Text = "跳过",
            Location = new Point(240, 95),
            Size = new Size(90, 30),
            DialogResult = DialogResult.Cancel,
            BackColor = Color.FromArgb(80, 80, 80),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
        };
        cancelButton.FlatAppearance.BorderColor = Color.Gray;
        inputForm.Controls.Add(cancelButton);

        inputForm.AcceptButton = okButton;
        inputForm.CancelButton = cancelButton;

        if (inputForm.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(textBox.Text))
        {
            playerName = textBox.Text.Trim();
        }
        else
        {
            return; // 用户取消
        }

        // 提交分数
        var entry = new LeaderboardEntry
        {
            PlayerName = playerName,
            Score = _engine.State.Score,
            SnakeLength = _engine.Snake.Length,
            Level = _engine.State.Level,
            Difficulty = _engine.State.CurrentDifficulty switch
            {
                DifficultyLevel.Slow => "慢",
                DifficultyLevel.Medium => "中",
                DifficultyLevel.Fast => "快",
                _ => "中"
            },
            Theme = _skinManager.CurrentMap.Name,
        };

        bool submitted = await _leaderboardService.SubmitScoreAsync(entry);

        if (submitted)
        {
            MessageBox.Show(
                $"分数已成功提交到排行榜！\n\n玩家: {playerName}\n分数: {_engine.State.Score}",
                "提交成功",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            // 询问是否查看排行榜
            DialogResult viewResult = MessageBox.Show(
                "是否查看排行榜？",
                "排行榜",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (viewResult == DialogResult.Yes)
            {
                await OpenLeaderboardAsync();
            }
        }
        else
        {
            MessageBox.Show(
                "排行榜服务未启动。\n\n请先启动 LeaderboardAPI 服务器。",
                "提交失败",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private async Task OpenLeaderboardAsync()
    {
        // 确保 API 在运行
        bool apiRunning = await _leaderboardService.IsApiRunningAsync();
        if (!apiRunning)
        {
            apiRunning = await _leaderboardService.StartApiServerAsync();
        }

        if (!apiRunning)
        {
            MessageBox.Show(
                "无法连接到排行榜服务器。\n请确保 LeaderboardAPI 项目已编译并运行。",
                "排行榜不可用",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        using var leaderboardForm = new LeaderboardForm(_leaderboardService);
        leaderboardForm.ShowDialog(this);
    }
}
