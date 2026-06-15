@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ================================
echo         贪吃蛇游戏
echo ================================

where dotnet >nul 2>nul
if %errorlevel% neq 0 (
    echo [错误] 未找到 .NET SDK，请安装 .NET 8.0
    echo https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

:: 直接启动，dotnet run 会自动编译
echo 正在启动...
start "" dotnet run --project SnakeGame.csproj
