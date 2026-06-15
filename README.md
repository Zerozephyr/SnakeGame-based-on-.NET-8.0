# 🐍 贪吃蛇游戏

基于 .NET 8 WinForms 的贪吃蛇游戏，支持皮肤切换、关卡系统、AI 演示和在线排行榜。

## 🚀 快速开始

### 方式一：独立版（推荐）

通过百度网盘下载压缩包：
https://pan.baidu.com/s/1wYJpRXdw20wcKg2082E6HA?pwd=nq44 提取码: nq44
解压后，运行 `SnakeGame-exe/SnakeGame.exe`，无需安装 .NET。

### 方式二：源码运行

```bash
dotnet run --project SnakeGame.csproj
```
或者双击"StartGame.bat"一键执行

## 🎮 操作说明

| 按键 | 功能 |
|------|------|
| **W A S D** / 方向键 | 控制蛇的移动 |
| **空格** | 暂停 / 继续 |
| **Tab** | 切换蛇皮（经典蛇 / 霓虹蛇 / 岩浆蛇 / 冰霜蛇 / 金龙蛇） |
| **F2** | 切换 AI 演示模式 |
| **1 / 2 / 3** | 切换难度（慢 / 中 / 快） |
| **R** | 重新开始 |
| **M** | 音乐开关 |
| **L** | 查看在线排行榜 |

## 🎨 功能特性

- **5 套蛇皮**：Tab 键循环切换，蛇头随方向旋转，身体块按走向旋转
- **关卡系统**：每吃 5 个食物升级，速度加快并生成障碍物，升级时自动换地图
- **音效系统**：吃食物、死亡、升级均有音效，支持背景音乐
- **AI 演示**：F2 开启，BFS 算法自动寻路吃食物
- **在线排行榜**：游戏分数自动上传，按 L 查看全球排行

## 📁 项目结构

```
SnakeGame/
├── Program.cs                  程序入口
├── SnakeGame.csproj            项目文件
├── SnakeGame.sln               解决方案
├── 启动游戏.bat                 一键启动脚本
│
├── Models/                     数据模型
│   ├── GameConfig.cs           游戏配置
│   ├── Snake.cs                蛇
│   ├── Food.cs                 食物
│   ├── GameState.cs            游戏状态
│   └── SkinTheme.cs            皮肤主题
│
├── Services/                   业务逻辑
│   ├── GameEngine.cs           游戏引擎
│   ├── Renderer.cs             渲染器
│   ├── InputHandler.cs         输入处理
│   ├── SkinManager.cs          皮肤/地图管理
│   ├── LevelManager.cs         关卡管理
│   ├── SoundService.cs         音效服务
│   ├── MusicService.cs         音乐服务
│   ├── AIPathfinder.cs         AI 寻路
│   ├── ScoreService.cs         分数存取
│   └── LeaderboardService.cs   排行榜客户端
│
├── UI/                         界面
│   ├── MainForm.cs             主窗口
│   └── LeaderboardForm.cs      排行榜窗口
│
├── Themes/                     皮肤素材（14 张 PNG）
├── Sounds/                     音频文件（bgm + 3 个音效）
│
└── LeaderboardAPI/             排行榜服务端
    ├── Program.cs              ASP.NET Core Web API
    ├── Models/ScoreEntry.cs    数据模型
    └── Data/LeaderboardRepository.cs  数据访问层
```

## 🌐 排行榜部署

### 1. 发布 API

```bash
cd LeaderboardAPI
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o publish
```

### 2. 上传到服务器

将 `publish/LeaderboardAPI` 上传到服务器的 `/www/wwwroot/snake-api/`。

### 3. 启动服务

```bash
cd /www/wwwroot/snake-api
chmod +x LeaderboardAPI
pm2 start ./LeaderboardAPI --name snake-api --interpreter none
pm2 save
```

### 4. 放行端口

阿里云安全组和宝塔防火墙均放行 TCP `5123`。

### 5. 验证

```bash
curl http://yourIP:5123/api/health
# {"status":"ok","time":"..."}
```

### 修改服务器地址

默认地址编译在程序中（`Services/LeaderboardService.cs` 的 `DefaultServerUrl`），如需临时覆盖，在运行目录下创建 `server.txt` 写入新地址即可。
