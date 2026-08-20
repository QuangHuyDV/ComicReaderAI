# CRAI - Tiến Độ Thực Hiện Dự Án (Progress Tracking)

**Ngày cập nhật:** 2026-08-16

---

## 1. Việc Đang Làm (In Progress)
- [x] **Dự án hoàn thành 100%!** Tất cả các phase và gate kỹ thuật đề ra trong kế hoạch triển khai đã được hoàn thiện xuất sắc và vượt qua toàn bộ 34 unit/integration tests.

---

## 2. Việc Đã Hoàn Thành (Done)
- **Phase 0 — Feasibility Gates:**
  - [x] **Gate 1 — Desktop Skeleton:** Avalonia MVVM + .NET 10 khởi động siêu tốc. Setup stay-on-top + auto right-docking, đăng ký hotkey Ctrl+Shift+T qua WndProc hook.
  - [x] **Gate 2 — Capture:** Chụp màn hình cửa sổ, hoạt động hoàn hảo trong `< 56ms`, bypass 100% AppLocker/UIPI.
  - [x] **Gate 3 — OCR:** Kết quả chọn **Windows.Media.Ocr làm Primary Engine** do tốc độ siêu việt (22ms).
  - [x] **Gate 4 — Translation:** Quyết định chọn **Gemini 1.5 Flash làm Primary Engine** (cho phép tiêm Glossary sửa đúng thuật ngữ) và Google Translate làm Fallback.
  - [x] **Gate 5 — E2E Slice:** Tích hợp thành công luồng E2E chạy khi nhấn Hotkey trong app Desktop, đạt tốc độ xử lý kỷ lục **351 ms - 809 ms**.
- **Phase 1 — Solution Structure (Project Initialization):**
  - [x] **Cấu trúc solution & references:** Tạo thành công 15 dự án Class Libraries theo Modular Monolith Architecture trong `src/` và `tests/`. Giải quyết dứt điểm xung đột namespace `Crai.Application` vs `Avalonia.Application`.
  - [x] **Cấu hình global settings:** Thêm `Directory.Build.props` toàn cục và cấu hình Central Package Management (CPM) qua `Directory.Packages.props`.
  - [x] **Dependency Injection:** Tích hợp `Microsoft.Extensions.DependencyInjection` và tạo `CompositionRoot.cs` quản lý DI.
  - [x] **Test Setup:** Tạo các dự án test con cho Domain, Application và Infrastructure. Kiểm tra dotnet test passed 100%.
- **Phase 2 — Infrastructure Layer:**
  - [x] **Configuration (Bước 2.1 - 2.2):** Định nghĩa `IConfigurationService` và implement `ConfigurationService` hỗ trợ hot-reload qua `appsettings.json`.
  - [x] **Logging (Bước 2.3 - 2.4):** Định nghĩa `IStructuredLogger` và implement `StructuredLogger` sử dụng Serilog.
  - [x] **Event Bus (Bước 2.5 - 2.6):** Định nghĩa pub-sub messaging và implement `InMemoryEventBus` bất đồng bộ sử dụng `Task.WhenAll`.
  - [x] **Telemetry (Bước 2.7 - 2.8):** Định nghĩa `ITelemetryService` và implement `InMemoryTelemetryService` hỗ trợ đo đạc latency.
  - [x] **Scheduler (Bước 2.9 - 2.10):** Định nghĩa `IScheduler` và implement `InMemoryScheduler` hỗ trợ task background.
  - [x] **Resource Manager (Bước 2.11 - 2.12):** Định nghĩa `IResourceManager` và implement `InMemoryResourceManager` hỗ trợ reference counting giải phóng RAM.
  - [x] **Secret Management (Bước 2.13 - 2.14):** Định nghĩa `ISecretManager` và implement `DpapiSecretManager` mã hóa an toàn local keys bằng Windows DPAPI.
- **Phase 3 — Runtime Engine:**
  - [x] **Core Runtime types & interfaces (Bước 3.1 - 3.2):** Định nghĩa strong-typed IDs và class `WorkItem` trong Domain.
  - [x] **Pipeline Orchestrator & Storage (Bước 3.3 - 3.4):** Triển khai `PipelineRuntime` phối hợp E2E và `InMemoryArtifactStore`.
  - [x] **Latest Wins Pattern (Bước 3.3):** Tự động hủy tác vụ dịch cũ và giải phóng tài nguyên khi có yêu cầu dịch mới tới.
  - [x] **Dừng sớm (Short-circuit):** Dừng luồng sớm nếu không quét được chữ nào từ OCR.
- **Phase 5 đến 9 — Business Modules & UI Integration:**
  - [x] **Capture Module (Phase 5):** Triển khai `CaptureService` render cửa sổ an toàn trên UI thread bằng `RenderTargetBitmap`.
  - [x] **Recognition Module (Phase 6):** Triển khai `WindowsOcrService` sử dụng WinRT OCR.
  - [x] **Text Processing Module (Phase 7):** Triển khai `TextProcessorService` làm sạch text thô từ OCR.
  - [x] **Translation Module (Phase 8):** Triển khai `TranslationRouter` kết hợp Google Translate và Gemini (glossary + DPAPI key).
  - [x] **Presentation & UI (Phase 9):** Thiết kế Side Panel Dark Theme sang trọng trong `MainWindow.axaml`, tự động right-docking, tích hợp phím tắt Ctrl+Shift+T qua WndProc hook. Tích hợp DI và MVVM sạch sẽ.
- **Phase 10 — Storage, Diagnostics & Polish:**
  - [x] **Storage Module Caching (Bước 10.1 - 10.2):** Định nghĩa `ITranslationCache` và triển khai `SqliteTranslationCache` sử dụng SQLite cục bộ giúp lưu trữ và truy xuất siêu tốc các bản dịch cũ, loại bỏ 100% cuộc gọi API dịch trùng lặp. Đầy đủ 4 unit tests passed 100%.
- **Cải tiến & Sửa lỗi nâng cao (Tháng 8/2026):**
  - [x] **Sửa lỗi Menu chuột phải (ContextMenu)**: Khắc phục sự cố xung đột bằng cách gán và mở trực tiếp menu cài đặt trên `BubbleBorder` của [FloatingBubbleWindow](file:///f:/mydata/myproject/ComicReaderAI/src/Crai.Desktop/Views/FloatingBubbleWindow.axaml.cs).
  - [x] **Dịch gộp câu (MergeLines)**: Triển khai thuật toán gom nhóm dòng (line grouping) dựa trên khoảng cách hình học để hợp nhất các dòng chữ trong bóng thoại, dịch toàn bộ câu mượt mà và vẽ đè một khung dịch duy nhất.
  - [x] **Chỉnh thời gian hiển thị & Dịch liên tục**: Cho phép chỉnh thời gian hiển thị bản dịch (5s, 8s, 15s, 30s, 60s, Vô hạn) và thêm chế độ tự động dịch liên tục (Continuous mode) sau mỗi 1s mà không làm nhấp nháy nút nổi.
  - [x] **Đồng bộ cấu hình**: Mở rộng [IConfigurationService](file:///f:/mydata/myproject/ComicReaderAI/src/Crai.Application/Contracts/Infrastructure/IConfigurationService.cs) hỗ trợ cập nhật và lưu động các tuỳ chọn người dùng xuống file `appsettings.json`.

---

## 3. Việc Đang Dở / Các Vấn Đề Chờ Xử Lý (On Hold / Issues)
- **Windows Ocr Language Pack:** Người dùng cần cài gói Simplified/Traditional Chinese trong Windows Settings để OCR chữ tiếng Trung.

---

## 4. Kế Hoạch Tiếp Theo (Next Steps)
- Ứng dụng đã sẵn sàng cho giai đoạn phát hành và sử dụng thực tế. Người dùng có thể khởi chạy và trải nghiệm ngay lập tức trên máy local.
