using SnakeGame.Services;

namespace SnakeGame.UI;

/// <summary>
/// 排行榜显示窗体
/// </summary>
public partial class LeaderboardForm : Form
{
    private readonly LeaderboardService _leaderboardService;
    private DataGridView _grid = null!;
    private Button _refreshButton = null!;
    private Label _titleLabel = null!;
    private Label _statusLabel = null!;

    public LeaderboardForm(LeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService;

        this.Text = "🏆 排行榜";
        this.ClientSize = new Size(700, 500);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(30, 30, 30);
        this.ForeColor = Color.White;

        InitializeControls();
        _ = LoadScoresAsync();
    }

    private void InitializeControls()
    {
        // 标题
        _titleLabel = new Label
        {
            Text = "🏆 贪吃蛇排行榜",
            Font = new Font("微软雅黑", 16, FontStyle.Bold),
            ForeColor = Color.Gold,
            AutoSize = true,
            Location = new Point(20, 15),
        };
        this.Controls.Add(_titleLabel);

        // 状态标签
        _statusLabel = new Label
        {
            Text = "正在加载...",
            Font = new Font("微软雅黑", 10),
            ForeColor = Color.LightGray,
            AutoSize = true,
            Location = new Point(20, 45),
        };
        this.Controls.Add(_statusLabel);

        // DataGridView
        _grid = new DataGridView
        {
            Location = new Point(20, 75),
            Size = new Size(660, 350),
            BackgroundColor = Color.FromArgb(45, 45, 45),
            ForeColor = Color.White,
            GridColor = Color.FromArgb(60, 60, 60),
            BorderStyle = BorderStyle.Fixed3D,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                SelectionBackColor = Color.FromArgb(70, 70, 120),
                SelectionForeColor = Color.White,
                Font = new Font("微软雅黑", 10),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
            },
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.Gold,
                Font = new Font("微软雅黑", 10, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
            },
            EnableHeadersVisualStyles = false,
        };

        // 列定义
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "排名",
            Name = "Rank",
            FillWeight = 10,
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "玩家",
            Name = "Player",
            FillWeight = 25,
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "分数",
            Name = "Score",
            FillWeight = 15,
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "蛇长",
            Name = "Length",
            FillWeight = 10,
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "等级",
            Name = "Level",
            FillWeight = 10,
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "难度",
            Name = "Difficulty",
            FillWeight = 10,
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "时间",
            Name = "Time",
            FillWeight = 20,
        });

        this.Controls.Add(_grid);

        // 刷新按钮
        _refreshButton = new Button
        {
            Text = "刷新",
            Font = new Font("微软雅黑", 10),
            Location = new Point(580, 435),
            Size = new Size(100, 35),
            BackColor = Color.FromArgb(60, 60, 120),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
        };
        _refreshButton.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 160);
        _refreshButton.Click += async (s, e) => await LoadScoresAsync();
        this.Controls.Add(_refreshButton);
    }

    private async Task LoadScoresAsync()
    {
        _statusLabel.Text = "正在加载...";
        _grid.Rows.Clear();

        var scores = await _leaderboardService.GetTopScoresAsync(20);

        if (scores.Count == 0)
        {
            _statusLabel.Text = "暂无排行数据。玩一局游戏来提交分数吧！";
            return;
        }

        _statusLabel.Text = $"共 {scores.Count} 条记录";

        for (int i = 0; i < scores.Count; i++)
        {
            var entry = scores[i];
            _grid.Rows.Add(
                i + 1,
                entry.PlayerName,
                entry.Score,
                entry.SnakeLength,
                entry.Level,
                entry.Difficulty,
                entry.SubmittedAt.ToLocalTime().ToString("MM/dd HH:mm"));
        }

        // 高亮前 3 名
        Color[] topColors = { Color.Gold, Color.Silver, Color.FromArgb(205, 127, 50) }; // bronze
        for (int i = 0; i < Math.Min(3, scores.Count); i++)
        {
            _grid.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(
                Math.Min(80 + i * 15, 110), 60, 20);
            _grid.Rows[i].DefaultCellStyle.ForeColor = i switch
            {
                0 => Color.Gold,
                1 => Color.Silver,
                _ => Color.FromArgb(255, 200, 140),
            };
        }
    }
}
