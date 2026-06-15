using Microsoft.AspNetCore.Mvc;
using LeaderboardAPI.Data;
using LeaderboardAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// 配置
builder.WebHost.UseUrls("http://0.0.0.0:5123");

// 注册服务
builder.Services.AddSingleton<LeaderboardRepository>();

// CORS — 允许 WinForms 客户端访问
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors();

// ═══ API 端点 ═══

// 获取排行榜
app.MapGet("/api/leaderboard", async (LeaderboardRepository repo, int? top) =>
{
    var scores = await repo.GetTopScores(top ?? 20);
    return Results.Ok(scores);
});

// 提交分数
app.MapPost("/api/scores", async (HttpContext context, LeaderboardRepository repo) =>
{
    ScoreRequest? req;
    try
    {
        req = await context.Request.ReadFromJsonAsync<ScoreRequest>();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = $"JSON 解析失败: {ex.Message}" });
    }

    if (req == null)
        return Results.BadRequest(new { error = "请求体为空或格式错误" });

    if (string.IsNullOrWhiteSpace(req.PlayerName))
        return Results.BadRequest(new { error = "玩家名称不能为空" });

    if (req.Score < 0)
        return Results.BadRequest(new { error = "分数不能为负数" });

    var entry = ScoreEntry.FromRequest(req);
    entry.SubmittedAt = DateTime.UtcNow;

    int id;
    try
    {
        id = await repo.AddScore(entry);
    }
    catch (Exception ex)
    {
        return Results.Problem($"数据库写入失败: {ex.Message}");
    }

    entry.Id = id;
    return Results.Created($"/api/scores/{id}", entry);
});

// 健康检查
app.MapGet("/api/health", () => Results.Ok(new { status = "ok", time = DateTime.UtcNow }));

app.Run();
