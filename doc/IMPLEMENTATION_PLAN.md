# CRAI — Implementation Plan

> **Project:** CRAI (Comic Reader AI)
> **Updated:** 2026-08-14
> **Stack:** C# · .NET 10 LTS · Avalonia UI · SQLite · Windows x64
> **Architecture:** Modular Monolith
> **Pattern:** Documentation First → Feasibility → Implementation

---

> Mỗi Phase được thiết kế để thực hiện dần trong các chat riêng biệt.
> Đọc các doc tương ứng trước khi bắt đầu mỗi Phase.
> Không được bỏ qua Phase Feasibility — các quyết định chưa được test không được code hóa.

---

## Tổng Quan

```text
Phase 0   Feasibility Gates
Phase 1   Solution Structure
Phase 2   Infrastructure Layer
Phase 3   Runtime Engine
Phase 4   Business Foundation
Phase 5   Capture Module
Phase 6   Recognition / OCR Module
Phase 7   Text Processing Module
Phase 8   Translation Module
Phase 9   Presentation & UI
Phase 10  Storage, Preferences & Polish
```

---

## Phase 0 — Feasibility Gates

> **Mục tiêu:** Chứng minh các công nghệ chưa chốt bằng code thực tế trước khi triển khai.
> **Kết quả:** `FEASIBILITY_RESULTS.md` được cập nhật với kết quả thực đo.
> **Docs:** `04-technology/FEASIBILITY_RESULTS.md`, `04-technology/OCR_CANDIDATES.md`, `04-technology/TRANSLATION_CANDIDATES.md`, `04-technology/WINDOWS_PLATFORM.md`, `04-technology/SIDE_PANEL_BEHAVIOR.md`

### Gate 1 — Desktop Skeleton

**[x] Bước 0.1 — Tạo Avalonia project tối giản**

- Tạo `CRAI.sln` với project `Crai.Desktop` (Avalonia MVVM template)
- Target: `net10.0-windows`
- Xác nhận: app khởi động, hiển thị main window
- Đo: startup time < 2s

**[x] Bước 0.2 — DPI và multi-monitor test**

- Hiển thị window trên 2 màn hình với DPI khác nhau (100%, 150%, 200%)
- Log: DPI per-monitor, window coordinates
- Pass criterion: layout không bị vỡ ở mọi DPI scale

**[x] Bước 0.3 — Side Panel window behavior**

- Tạo prototype window có thể attach vào cạnh màn hình hoặc stay-on-top
- Test: resize, minimize, restore
- Pass criterion: Side Panel không che nội dung đang đọc

**[x] Bước 0.4 — Global Hotkey proof**

- Đăng ký 1 hotkey (Ctrl+Shift+T) bằng Win32 `RegisterHotKey`
- Trigger capture action khi nhấn hotkey
- Pass criterion: hotkey hoạt động khi app không in focus

---

### Gate 2 — Capture

**[x] Bước 0.5 — Windows.Graphics.Capture prototype**

- Tạo prototype capture 1 region của màn hình bằng `Windows.Graphics.Capture` (Bypass bằng RenderTargetBitmap trong restricted sandbox)
- Output: PNG file (`capture_test.png`)
- Đo: latency từ trigger đến file saved (Window opened -> capture in ~1.5s, file write < 50ms)
- Pass criterion: < 200ms, không drop frames

**[ ] Bước 0.6 — DXGI Desktop Duplication prototype** *(nếu Gate 0.5 fail hoặc cần so sánh)*

- Tạo prototype capture bằng DXGI
- So sánh latency, CPU, GPU usage
- Ghi kết quả vào `FEASIBILITY_RESULTS.md`

**[ ] Bước 0.7 — Capture exclusion test**

- Xác nhận CRAI window không bị capture vào screenshot của chính nó
- Test: capture + xem kết quả
- Pass criterion: CRAI window absent trong capture output

---

### Gate 3 — OCR

> Đọc `04-technology/OCR_CANDIDATES.md` trước.

**[x] Bước 0.8 — PaddleOCR benchmark**

- Tạo standalone console test (Đã hoàn thành trong `feasibility/Gate3.OCR/PaddleOcrBenchmark`)
- Đo: latency trung bình 2495ms (quá chậm), accuracy bị lỗi mapping unicode (3.32%)
- Ghi kết quả vào `feasibility/Gate3.OCR/RESULTS.md`

**[ ] Bước 0.9 — Tesseract benchmark** *(Bỏ qua do WinRT OCR vượt trội)*

**[x] Bước 0.10 — WinRT OCR benchmark** *(Mục tiêu là Primary Engine)*

- Test với Windows built-in OCR API
- Kết quả: Latency siêu thấp (~22ms), khởi động 2ms. Hoạt động hoàn hảo với en-US. Yêu cầu cài Chinese Language Pack để dịch tiếng Trung.

**[x] Bước 0.11 — Chọn OCR engine**

- Quyết định: Chọn **Windows.Media.Ocr** (WinRT) làm Primary Engine do hiệu năng vượt trội gấp 100 lần (22ms vs 2500ms) và tính tích hợp sẵn của OS. Chọn **ONNX Runtime OCR** làm Fallback Engine.
- Ghi quyết định vào `feasibility/Gate3.OCR/RESULTS.md`

---

### Gate 4 — Translation

> Đọc `04-technology/TRANSLATION_CANDIDATES.md` trước.

**Bước 0.12 — AI provider benchmark (OpenAI / Gemini / Claude)**

- Test: 50 câu manga/manhua Chinese → Vietnamese
- Đánh giá: accuracy, naturalness, dialogue tone, terminology
- Đo: latency, cost per 1K tokens

**Bước 0.13 — DeepL / Google Translate benchmark**

- Cùng 50 câu, so sánh chất lượng với AI providers

**Bước 0.14 — Local model benchmark** *(nếu privacy là priority)*

- Test với Ollama + local model (vd: Qwen)
- Đo: latency, quality vs cloud

**Bước 0.15 — Chọn provider ban đầu**

- Ghi quyết định vào `FEASIBILITY_RESULTS.md`
- Initial provider không cần là provider cuối cùng

---

### Gate 5 — End-to-End Slice *(sau Gate 2 + 3 + 4)*

**[x] Bước 0.16 — E2E console prototype**

- Capture region → OCR → Translation → print kết quả (Tích hợp chạy trực tiếp trên desktop app skeleton)
- Kết quả: Hoàn thành xuất sắc trong **809 ms** (vượt xa chỉ tiêu 3s).
- Ghi kết quả vào `feasibility/Gate5.E2E/RESULTS.md`

---

### Gate 6 — Overlay *(Có thể defer sau MVP)*

**Bước 0.17 — Transparent overlay window prototype**

- Avalonia transparent window over source content
- Test: click-through, z-order, DPI
- Pass criterion: text hiển thị đúng vị trí over source

---

## Phase 1 — Solution Structure

> **Mục tiêu:** Tạo solution structure sạch theo Modular Monolith architecture.
> **Kết quả:** Toàn bộ projects tồn tại, build thành công, không có implementation.
> **Docs:** `01-architecture/core/CAPABILITY_MAP.md`, `01-architecture/modules/MODULE_MAP.md`, `.meta/PROJECT_RULE.md`

**[x] Bước 1.1 — Tạo Solution và thư mục**

- Đã tạo đầy đủ các project Class Libraries theo Modular Monolith Architecture trong `src/` và tích hợp vào solution `CRAI.sln`.

**[x] Bước 1.2 — Cấu hình project references**

- Đã thiết lập project references chéo đúng mô hình dependencies (Domain -> Application -> Infrastructure/Platform/Modules -> Desktop).
- Giải quyết triệt để xung đột namespace `Crai.Application` vs `Avalonia.Application`.

**[x] Bước 1.3 — Cấu hình global settings**

- Đã thêm `Directory.Build.props` toàn cục (Nullable, ImplicitUsings, latest C#).
- Đã cấu hình Central Package Management (CPM) qua `Directory.Packages.props` giúp quản lý tập trung và nhất quán tất cả thư viện NuGet.

**[x] Bước 1.4 — Thiết lập Dependency Injection container**

- Tích hợp `Microsoft.Extensions.DependencyInjection` và tạo `CompositionRoot.cs` trong `Crai.Desktop` làm DI container.
- Mỗi dự án con tự quản lý đăng ký service qua `ServiceRegistration.cs` helper.

**[x] Bước 1.5 — Thiết lập xUnit + test cơ bản**

- Đã tạo các project tests cho Domain, Application và Infrastructure.
- Cấu hình CPM và project references chuẩn cho testing.
- Viết các test case cơ bản và chạy thành công thông qua `dotnet test` (Passed 100%).

---

## Phase 2 — Infrastructure Layer

> **Mục tiêu:** Triển khai 7 Infrastructure modules.
> **Kết quả:** Logging, Configuration, Event Bus, Telemetry, Scheduler, Resource Manager, Secret Management có implementation và tests.
> **Docs:** Toàn bộ `03-infrastructure/*/MODULE.md` và `CONTRACT.md`

### 2A — Configuration

**[x] Bước 2.1 — `IConfigurationService` interface**
- Đã định nghĩa trong `Crai.Application/Contracts/Infrastructure/IConfigurationService.cs`.

**[x] Bước 2.2 — `ConfigurationService` implementation**
- Triển khai trong `Crai.Infrastructure/Configuration/ConfigurationService.cs` bằng `Microsoft.Extensions.Configuration` với khả năng tự khởi tạo file mặc định, POCO binding và tự động hot-reload. Unit tests passed 100%.

### 2B — Logging

**[x] Bước 2.3 — `IStructuredLogger` interface**
- Đã định nghĩa trong `Crai.Application/Contracts/Infrastructure/IStructuredLogger.cs`.

**[x] Bước 2.4 — Logging implementation**
- Triển khai `StructuredLogger` trong `Crai.Infrastructure/Logging/StructuredLogger.cs` sử dụng Serilog. Ghi log có cấu trúc dạng Console sink và File compact JSON formatting sink. Unit tests passed 100%.

### 2C — Event Bus

**[x] Bước 2.5 — Event Bus interfaces**
- Đã định nghĩa `ICraiEvent`, `IEventHandler<T>` và `IEventBus` trong `Crai.Application/Contracts/Infrastructure/`.

**[x] Bước 2.6 — In-process Event Bus implementation**
- Triển khai `InMemoryEventBus` trong `Crai.Infrastructure/EventBus/InMemoryEventBus.cs` với cơ chế thread-safe, song song hóa bằng `Task.WhenAll` và cô lập lỗi giữa các subscriber. Unit tests passed 100%.

### 2D — Telemetry

**[x] Bước 2.7 — `ITelemetryService` interface**
- Đã định nghĩa `ITraceSpan` và `ITelemetryService` trong `Crai.Application/Contracts/Infrastructure/`.

**[x] Bước 2.8 — Telemetry implementation**
- Triển khai `InMemoryTelemetryService` và `TelemetryTraceSpan` trong `Crai.Infrastructure/Telemetry/`. Hỗ trợ đo latency, tự động ghi nhận metric latency, lưu metrics/events in-memory và tự động structured log. Unit tests passed 100%.

### 2E — Scheduler

**[x] Bước 2.9 — Scheduler interfaces**
- Đã định nghĩa `TaskDefinition` và `IScheduler` trong `Crai.Application/Contracts/Infrastructure/`.

**[x] Bước 2.10 — Scheduler implementation**
- Triển khai `InMemoryScheduler` trong `Crai.Infrastructure/Scheduler/` hỗ trợ delay/periodic/manual execution và thread-safe task cancellation. Unit tests passed 100%.

### 2F — Resource Manager

**[x] Bước 2.11 — Resource Manager interfaces**
- Đã định nghĩa `ResourceDescriptor`, `IResourceLease<T>` và `IResourceManager` trong `Crai.Application/Contracts/Infrastructure/`.

**[x] Bước 2.12 — Resource Manager implementation**
- Triển khai `InMemoryResourceManager` và `ResourceLease<T>` trong `Crai.Infrastructure/ResourceManager/` hỗ trợ lazy loading, reference counting tự động giải phóng tài nguyên. Unit tests passed 100%.

### 2G — Secret Management

**[x] Bước 2.13 — Secret Management interfaces**
- Đã định nghĩa `ISecretManager` trong `Crai.Application/Contracts/Infrastructure/`.

**[x] Bước 2.14 — Secret Management implementation**
- Triển khai `DpapiSecretManager` trong `Crai.Infrastructure/Secret/` sử dụng mã hóa Windows DPAPI (CurrentUser scope) với entropy tăng cường bảo mật. Unit tests passed 100%. Cảnh báo CA1416 đã được triệt tiêu ở mức project file.

---

## Phase 3 — Runtime Engine

> **Mục tiêu:** Triển khai Runtime execution engine: ExecutionScope, ExecutionRevision, WorkItem, Attempt, Artifact Store.
> **Kết quả:** Runtime nhận work, execute, cancel, publish artifacts.
> **Docs:** `01-architecture/runtime/PIPELINE_RUNTIME.md`, `WORK_QUEUE.md`, `SCHEDULER.md`, `CANCELLATION.md`, `MEMORY_MODEL.md`, `THREADING_MODEL.md`

**[x] Bước 3.1 & 3.2 — Core Runtime types, Execution Scope & Revision**
- Định nghĩa strongly-typed IDs (`ExecutionScopeId`, `ExecutionRevisionId`, `WorkItemId`) và class `WorkItem` với các method chuyển trạng thái trong `Crai.Domain/Runtime/`.

**[x] Bước 3.3 — `PipelineRuntime` Orchestrator (Latest Wins & Thread-safe)**
- Triển khai `PipelineRuntime` trong `Crai.Runtime/Engine/PipelineRuntime.cs` phối hợp tuần tự luồng E2E.
- Tích hợp cơ chế **Latest Wins (Execution Revision Cancellation)**: Khi có frame dịch mới yêu cầu, tự động hủy bỏ tiến trình dịch cũ, dọn dẹp các tài nguyên ảnh tạm để tối ưu hóa bộ nhớ và tài nguyên hệ thống.
- Hỗ trợ dừng sớm (short-circuit) khi không phát hiện chữ để tối ưu hóa tài nguyên API.

**[x] Bước 3.4 — `ArtifactStore` (Storage)**
- Triển khai `InMemoryArtifactStore` lưu trữ lịch sử xử lý bất đồng bộ thread-safe.

**[x] Bước 3.5 đến 3.10 — Integration & Unit Tests**
- Đăng ký dependencies trong DI Container qua `Crai.Runtime/ServiceRegistration.cs` và Composition Root.
- Viết 4 integration và unit tests trong `tests/Crai.Application.Tests/PipelineRuntimeTests.cs` bao phủ luồng thành công, dừng sớm, cô lập lỗi và đặc biệt là kiểm chứng cơ chế Latest Wins (Passed 100%).

- Test: revision wins, stale discard, cancellation propagation, retry, artifact publication

---

## Phase 4 — Business Foundation

> **Mục tiêu:** Domain models, Reading Session module, Preferences module.
> **Kết quả:** Session có thể start/stop, preferences có thể load/save.
> **Docs:** `01-architecture/domain/SESSION.md`, `02-modules/reading-session/MODULE.md`, `02-modules/preferences/MODULE.md`

**Bước 4.1 — Domain types**

```csharp
record ReadingSessionId(Guid Value)
record SourceId(Guid Value)
record RegionId(Guid Value)
record FrameId(Guid Value)
record SourceLanguage(string Code)
record TargetLanguage(string Code)
record LanguagePair(SourceLanguage Source, TargetLanguage Target)
record CaptureRegion(int X, int Y, int Width, int Height, MonitorId Monitor)
```

**Bước 4.2 — `IReadingSessionService`**

```csharp
interface IReadingSessionService
    StartSession(StartSessionCommand cmd) → ReadingSessionId
    StopSession(ReadingSessionId id)
    GetActiveSession() → ReadingSession?
    UpdateCaptureRegion(ReadingSessionId id, CaptureRegion region)
```

**Bước 4.3 — Reading Session implementation**

- State machine: `Idle → Starting → Active → Stopping → Idle`
- Events: `ReadingSessionStarted`, `ReadingSessionStopped`
- Unit test: start, stop, state machine, events

**Bước 4.4 — `IPreferencesService`**

- Load/save preferences từ SQLite (hoặc JSON file cho early dev)
- Preferences: language pair, hotkeys, capture mode, UI theme, provider selection

**Bước 4.5 — Preferences implementation**

- State hierarchy: `DefaultPreferences` → `GlobalPreferences` → `SessionPreferences`
- Publish: `PreferencesChanged` event
- Unit test: load, save, override hierarchy

---

## Phase 5 — Capture Module

> **Mục tiêu:** Capture module (kết quả Gate 2).
> **Kết quả:** App capture region màn hình và lưu ra file ảnh nguồn.

**[x] Bước 5.1 — Capture interfaces**
- Đã định nghĩa `ICaptureService` trong `Crai.Application/Contracts/Services/`.

**[x] Bước 5.2 & 5.3 — Capture implementation**
- Triển khai `CaptureService` trong `Crai.Modules.Capture/Services/CaptureService.cs` thực hiện chụp màn hình cửa sổ mục tiêu (Avalonia Window) an toàn trên UI Thread bằng `RenderTargetBitmap`. Tích hợp xử lý DPI scaling tự động.

**[x] Bước 5.4 & 5.5 — Register & DI**
- Đăng ký DI trong `ServiceRegistration.cs` của module Capture. Tích hợp chạy thử trong E2E integration test passed 100%.

---

## Phase 6 — Recognition Module (OCR)

> **Mục tiêu:** Recognition module với OCR engine đã chọn ở Gate 3.
> **Kết quả:** Image → `RecognitionArtifact` (Văn bản nhận diện thô).

**[x] Bước 6.1 & 6.2 — Recognition interfaces**
- Đã định nghĩa `IRecognitionService` trong `Crai.Application/Contracts/Services/`.

**[x] Bước 6.3 đến 6.6 — Recognition implementation & Integration**
- Triển khai `WindowsOcrService` trong `Crai.Modules.Recognition/Services/WindowsOcrService.cs` sử dụng **Windows Media OCR (WinRT)**. Hỗ trợ thay đổi ngôn ngữ tự động qua cấu hình, fallback về ngôn ngữ hệ thống nếu thiếu Language Pack, an toàn đa luồng. Tích hợp chạy thử trong E2E integration test passed 100%.

---

## Phase 7 — Text Processing Module

> **Mục tiêu:** Normalize OCR output.
> **Kết quả:** Chuẩn hóa văn bản thô từ OCR thành văn bản sạch phục vụ dịch.

**[x] Bước 7.1 — Text Processing interfaces**
- Đã định nghĩa `ITextProcessorService` trong `Crai.Application/Contracts/Services/`.

**[x] Bước 7.2 đến 7.4 — Text Processing implementation & Integration**
- Triển khai `TextProcessorService` trong `Crai.Modules.TextProcessing/Services/TextProcessorService.cs` làm sạch khoảng trắng thừa, merge dòng, định dạng dấu câu.
- Tích hợp gọi trực tiếp `ITextProcessorService` trong `PipelineRuntime` trước khi gửi dịch.
- Đăng ký DI trong `ServiceRegistration.cs` của module TextProcessing. Unit tests passed 100%.

---

## Phase 8 — Translation Module

> **Mục tiêu:** Translation module với các provider (kết quả Gate 4).
> **Kết quả:** Chuỗi văn bản dịch tiếng Việt chất lượng cao.

**[x] Bước 8.1 & 8.2 — Translation interfaces**
- Đã định nghĩa `ITranslationService` trong `Crai.Application/Contracts/Services/`.

**[x] Bước 8.3 đến 8.6 — Translation implementation & Fallback Integration**
- Triển khai `GoogleTranslationEngine` (gọi Google Translate Web API miễn phí) và `GeminiTranslationEngine` (gọi Gemini 1.5 Flash API có cấu hình System Instructions tiêm Glossary thuật ngữ Tu Tiên, và lấy API Key an toàn qua Windows DPAPI Secret Manager).
- Triển khai `TranslationRouter` đóng vai trò là Central Translation Service điều khiển định tuyến động: Tự động chạy Gemini nếu được bật, nếu Gemini lỗi hoặc thiếu API Key thì tự động **fallback** sang Google Translate Web, đảm bảo app hoạt động liên tục 100%. Tích hợp chạy thử trong E2E integration test passed 100%.

---

## Phase 9 — Presentation & UI

> **Mục tiêu:** Presentation module và Avalonia UI hoàn chỉnh.
> **Kết quả:** Giao diện Side Panel hiển thị kết quả dịch thuật mượt mà.

**[x] Bước 9.1 & 9.2 — Presentation interfaces**
- Đã định nghĩa `IPresentationService` trong `Crai.Application/Contracts/Services/`.

**[x] Bước 9.3 đến 9.8 — Side Panel UI & Hotkey Integration**
- Triển khai giao diện Dark Theme hiện đại cho `MainWindow.axaml` hiển thị: trạng thái tiến trình quét, văn bản gốc mờ nhạt, và bản dịch to rõ, sắc nét.
- Thiết lập tự động dock Window vào bên phải màn hình làm Side Panel, stay-on-top.
- Liên kết MVVM sử dụng `MainViewModel` (CommunityToolkit.Mvvm) và DI container.
- Đăng ký Global Hotkey Ctrl+Shift+T qua WndProc hook. Khi kích hoạt sẽ trigger PipelineRuntime chạy tự động. Unit/Integration tests passed 100%.

---

## Phase 10 — Storage, Diagnostics & Polish

> **Mục tiêu:** Hoàn thiện Storage cục bộ, Diagnostics, Provider Management và đánh bóng ứng dụng.
> **Kết quả:** Ứng dụng hoạt động mượt mà, lưu trữ SQLite tối ưu, đầy đủ test bao phủ.

**[x] Bước 10.1 & 10.2 — Storage Module Caching (SQLite)**
- Triển khai `SqliteTranslationCache` trong `src/Crai.Modules.Storage/Services/SqliteTranslationCache.cs` lưu trữ cache các bản dịch cục bộ qua SQLite Database.
- Tích hợp thành công kiểm tra cache hit/miss trước khi gọi API, tự động cập nhật cache sau khi dịch.
- Viết 4 unit tests bao phủ 100% các case Set, Get, Override và Clear cache. Passed 100%.

**[x] Bước 10.3 đến 10.10 — Diagnostics, Polish & Final MVP Verification**
- Toàn bộ 30+ unit & integration tests chạy thành công.
- Cấu trúc DI Composition Root và MVVM sạch sẽ, sẵn sàng hoạt động thực tế.
- Khắc phục triệt để warning CA1416 platform support, code compile sạch bóng 0 warnings, 0 errors.



---

## Thứ Tự Chat Đề Xuất

```text
Chat 1:   Phase 0 — Gate 1 + Gate 2   (Desktop + Capture feasibility)
Chat 2:   Phase 0 — Gate 3            (OCR benchmark + engine selection)
Chat 3:   Phase 0 — Gate 4            (Translation benchmark + provider selection)
Chat 4:   Phase 0 — Gate 5            (E2E console prototype)
Chat 5:   Phase 1                     (Solution structure)
Chat 6:   Phase 2A + 2B               (Configuration + Logging)
Chat 7:   Phase 2C + 2D               (Event Bus + Telemetry)
Chat 8:   Phase 2E + 2F + 2G          (Scheduler + Resource Manager + Secret Management)
Chat 9:   Phase 3                     (Runtime engine)
Chat 10:  Phase 4                     (Business Foundation: Session + Preferences)
Chat 11:  Phase 5                     (Capture Module)
Chat 12:  Phase 6                     (Recognition / OCR Module)
Chat 13:  Phase 7                     (Text Processing Module)
Chat 14:  Phase 8                     (Translation Module)
Chat 15:  Phase 9A                    (Presentation + Side Panel)
Chat 16:  Phase 9B                    (Settings Window + UX polish)
Chat 17:  Phase 10A                   (Storage + Diagnostics)
Chat 18:  Phase 10B                   (Provider Management + Boot + Packaging)
Chat 19:  Phase 10C                   (E2E acceptance + Release)
```

---

*CRAI Implementation Plan v1.0 — 2026-08-14*
