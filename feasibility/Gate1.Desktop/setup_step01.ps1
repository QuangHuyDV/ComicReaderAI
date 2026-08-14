# Script khởi tạo project CRAI - Bước 0.1
# Chạy sau khi cài .NET 10 SDK xong
# Usage: .\setup_step01.ps1

$ErrorActionPreference = "Stop"
$root = "f:\mydata\myproject\ComicReaderAI"
$dotnet = "dotnet"

Write-Host "=== CRAI Gate 1 — Bước 0.1: Tạo Avalonia project ===" -ForegroundColor Cyan

# Kiểm tra SDK
Write-Host "`n[1/6] Kiểm tra .NET SDK..." -ForegroundColor Yellow
$sdkVersion = & $dotnet --version
Write-Host "    SDK: $sdkVersion" -ForegroundColor Green

# Cài Avalonia templates
Write-Host "`n[2/6] Cài Avalonia templates..." -ForegroundColor Yellow
& $dotnet new install Avalonia.Templates
Write-Host "    Done" -ForegroundColor Green

# Tạo Solution
Write-Host "`n[3/6] Tạo CRAI.sln..." -ForegroundColor Yellow
Set-Location $root
& $dotnet new sln -n "CRAI" -o . --force
Write-Host "    Done" -ForegroundColor Green

# Tạo Avalonia MVVM project
Write-Host "`n[4/6] Tạo Crai.Desktop (Avalonia MVVM)..." -ForegroundColor Yellow
Set-Location "$root\src\Crai.Desktop"
& $dotnet new avalonia.mvvm -n "Crai.Desktop" -o . --force
Write-Host "    Done" -ForegroundColor Green

# Add project to solution
Write-Host "`n[5/6] Add project vào solution..." -ForegroundColor Yellow
Set-Location $root
& $dotnet sln CRAI.sln add "src\Crai.Desktop\Crai.Desktop.csproj"
Write-Host "    Done" -ForegroundColor Green

# Build test
Write-Host "`n[6/6] Build test..." -ForegroundColor Yellow
$start = Get-Date
& $dotnet build "src\Crai.Desktop\Crai.Desktop.csproj" -c Debug
$elapsed = (Get-Date) - $start
Write-Host "    Build time: $($elapsed.TotalSeconds)s" -ForegroundColor Green

Write-Host "`n=== Bước 0.1 DONE ===" -ForegroundColor Cyan
Write-Host "Tiếp theo: chạy app và đo startup time" -ForegroundColor White
Write-Host "  cd $root\src\Crai.Desktop" -ForegroundColor Gray
Write-Host "  dotnet run" -ForegroundColor Gray
