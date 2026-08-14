# Gate 1 — Desktop Skeleton

## Mục tiêu

Chứng minh Avalonia UI hoạt động đúng trên Windows x64:
- App khởi động < 2s
- DPI scaling đúng trên multi-monitor
- Side Panel window behavior (stay-on-top, dock to right)
- Global Hotkey hoạt động khi app không focus

---

## Bước 1: Cài .NET 10 SDK

Download từ: https://dotnet.microsoft.com/en-us/download/dotnet/10.0

Chọn: **.NET 10 SDK** → Windows → x64 → Installer

Sau khi cài xong, mở terminal mới và kiểm tra:
```
dotnet --version
```
Phải trả về `10.x.x`.

---

## Bước 2: Chạy setup script

```powershell
cd f:\mydata\myproject\ComicReaderAI
.\feasibility\Gate1.Desktop\setup_step01.ps1
```

Script sẽ:
1. Cài Avalonia templates
2. Tạo CRAI.sln
3. Tạo project Crai.Desktop (Avalonia MVVM)
4. Build và báo kết quả

---

## Bước 3: Tích hợp feasibility code

Sau khi project được tạo, copy các file vào project:

```
Gate1.Desktop/DpiDiagnostic.cs    → src/Crai.Desktop/
Gate1.Desktop/SidePanelProto.cs   → src/Crai.Desktop/
Gate1.Desktop/GlobalHotkeyProto.cs → src/Crai.Desktop/
```

Thêm vào `App.axaml.cs` → `OnFrameworkInitializationCompleted()`:

```csharp
if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
{
    var mainWindow = new MainWindow();

    // Bước 0.2 — DPI test
    DpiDiagnostic.Log(mainWindow);

    // Bước 0.3 — Side Panel
    var sidePanel = new SidePanelProto();
    sidePanel.Show();

    desktop.MainWindow = mainWindow;
    mainWindow.Show();
}
```

---

## Bước 4: Chạy và đo kết quả

```powershell
cd f:\mydata\myproject\ComicReaderAI\src\Crai.Desktop
dotnet run
```

Quan sát console output:
- `[DPI Diagnostic]` — thông tin màn hình
- `[Startup] Window opened in XXXms` — startup time

Ghi kết quả vào: `feasibility/Gate1.Desktop/RESULTS.md`

---

## Files trong thư mục này

| File | Mục đích |
|------|---------|
| `README.md` | File này — hướng dẫn |
| `RESULTS.md` | Ghi kết quả feasibility |
| `setup_step01.ps1` | Script tạo project tự động |
| `DpiDiagnostic.cs` | Code cho Bước 0.2 (DPI test) |
| `SidePanelProto.cs` | Code cho Bước 0.3 (Side Panel) |
| `GlobalHotkeyProto.cs` | Code cho Bước 0.4 (Global Hotkey) |
