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

**[/] Bước 0.8 — PaddleOCR benchmark**

- Tạo standalone console test
- Input: 10 manga pages (Simplified Chinese)
- Đo: accuracy, latency per page, memory, GPU usage
- Ghi kết quả vào `FEASIBILITY_RESULTS.md`

**[ ] Bước 0.9 — Tesseract benchmark** *(nếu cần so sánh)*

- Cùng 10 pages, so sánh accuracy vs PaddleOCR
- Ghi kết quả

**[ ] Bước 0.10 — WinRT OCR benchmark** *(cho fallback)*

- Test với Windows built-in OCR API
- Ghi kết quả: accuracy với Chinese text

**Bước 0.11 — Chọn OCR engine**

- So sánh kết quả Gate 3
- Ghi quyết định vào `FEASIBILITY_RESULTS.md` và `TECH_STACK.md`
- Quyết định: engine, runtime model (ONNX / native / Python worker)

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

**Bước 0.16 — E2E console prototype**

- Capture region → OCR → Translation → print kết quả
- Không cần UI
- Pass criterion: full pipeline < 3s từ capture đến output

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

**Bước 1.1 — Tạo Solution và thư mục**

```
CRAI.sln
src/
  Crai.App/                      <- Avalonia entry point
  Crai.Domain/                   <- Domain models (no external deps)
  Crai.Application/              <- Application contracts and services
  Crai.Infrastructure/           <- Infrastructure implementations
  Crai.Platform.Windows/         <- Windows-specific adapters
  Crai.Modules.Capture/
  Crai.Modules.Recognition/
  Crai.Modules.TextProcessing/
  Crai.Modules.Translation/
  Crai.Modules.Presentation/
  Crai.Modules.Storage/
  Crai.Modules.Preferences/
  Crai.Modules.Diagnostics/
  Crai.Modules.ProviderMgmt/
  Crai.Modules.UiAdapter/
  Crai.Runtime/
tests/
  Crai.Domain.Tests/
  Crai.Application.Tests/
  Crai.Infrastructure.Tests/
  Crai.Modules.*.Tests/
  Crai.Integration.Tests/
```

**Bước 1.2 — Cấu hình project references**

- Domain → không depend on ai cả
- Application → Domain only
- Infrastructure → Application, Domain
- Modules → Application, Domain (không phụ thuộc lẫn nhau)
- Runtime → Application, Infrastructure
- App → tất cả (Composition Root)
- Platform.Windows → Application interfaces only
- Viết `Directory.Build.props` chung

**Bước 1.3 — Cấu hình global settings**

- Enable `Nullable Reference Types` cho tất cả projects
- `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
- Cấu hình `editorconfig` / coding style
- Thêm `Directory.Packages.props` (Central Package Management)

**Bước 1.4 — Thiết lập Dependency Injection container**

- Dùng `Microsoft.Extensions.DependencyInjection`
- Tạo `CompositionRoot.cs` trong `Crai.App/`
- Mỗi module đăng ký service qua `IServiceCollection` extension method

**Bước 1.5 — Thiết lập xUnit + test cơ bản**

- Thêm xUnit vào tất cả `*.Tests` projects
- Viết 1 test trivial để xác nhận test runner hoạt động
- Cấu hình `dotnet test` chạy toàn bộ solution

---

## Phase 2 — Infrastructure Layer

> **Mục tiêu:** Triển khai 7 Infrastructure modules.
> **Kết quả:** Logging, Configuration, Event Bus, Telemetry, Scheduler, Resource Manager, Secret Management có implementation và tests.
> **Docs:** Toàn bộ `03-infrastructure/*/MODULE.md` và `CONTRACT.md`

### 2A — Configuration

**Bước 2.1 — `IConfigurationService` interface**

- Định nghĩa trong `Crai.Application/Contracts/Infrastructure/`
- Operations: `Get<T>(key)`, `GetSection<T>(section)`, `Reload()`
- Backed by: `Microsoft.Extensions.Configuration` (appsettings.json + env vars)

**Bước 2.2 — `ConfigurationService` implementation**

- Wrapper around `IConfiguration`
- Hot-reload support (FileSystemWatcher)
- Validate config khi load (DataAnnotations)
- Unit test: load, get, validate, reload

### 2B — Logging

**Bước 2.3 — `IStructuredLogger` interface**

- Operations: `Log(level, message, context)`, `LogError(error, context)`
- Context object: `{module, sessionId, traceId}`
- Không bao giờ log sensitive values

**Bước 2.4 — Logging implementation**

- Backend: `Microsoft.Extensions.Logging` + Serilog
- Sinks: Console (dev), File (production), JSON structured
- Unit test: log output format, no-sensitive-data invariant

### 2C — Event Bus

**Bước 2.5 — Event Bus interfaces**

```csharp
interface IEventBus
    Publish<TEvent>(TEvent @event) where TEvent : ICraiEvent
    Subscribe<TEvent>(IEventHandler<TEvent> handler)
    Unsubscribe<TEvent>(IEventHandler<TEvent> handler)

interface ICraiEvent
    DateTime OccurredAt
```

**Bước 2.6 — In-process Event Bus implementation**

- Dùng `System.Threading.Channels` + `ConcurrentDictionary<Type, List<IHandler>>`
- Dispatch trên background thread
- Error isolation: 1 handler fail không làm hỏng handler khác
- Unit test: publish, subscribe, unsubscribe, error isolation

### 2D — Telemetry

**Bước 2.7 — `ITelemetryService` interface**

- Operations: `RecordMetric(name, value, tags)`, `StartTrace(name)`, `RecordEvent(name, props)`

**Bước 2.8 — Telemetry implementation**

- MVP: in-memory metrics + console output
- Future: OpenTelemetry export
- Unit test: metric recording, no-sensitive-data

### 2E — Scheduler

**Bước 2.9 — Scheduler interfaces**

```csharp
interface IScheduler
    RegisterTask(TaskDefinition definition)
    TriggerNow(TaskId taskId)
    CancelJob(JobId jobId)
    Shutdown(ShutdownOptions options)
```

**Bước 2.10 — Scheduler implementation**

- In-memory scheduler với `Timer` + `PriorityQueue`
- Hỗ trợ: Immediate, Delayed, Interval, Manual triggers
- Retry với exponential backoff
- Cancellation qua `CancellationToken`
- Unit test: trigger, retry, timeout, cancellation, shutdown

### 2F — Resource Manager

**Bước 2.11 — Resource Manager interfaces**

```csharp
interface IResourceManager
    Register(ResourceDescriptor descriptor)
    Resolve<T>(ResourceId id)
    Acquire<T>(ResourceId id) → ResourceLease<T>
    Release(ResourceLease lease)
    Shutdown(ShutdownOptions options)
```

**Bước 2.12 — Resource Manager implementation**

- In-memory registry với `ConcurrentDictionary`
- Lazy initialization, Generation tracking, Lease tracking
- Basic pool support (min/max size, acquire timeout)
- Unit test: register, resolve, acquire, release, generation mismatch, leak detection

### 2G — Secret Management

**Bước 2.13 — Secret Management interfaces**

```csharp
interface ISecretManager
    ResolveSecret(SecretId id, ModuleIdentity caller) → SecretValue
    StoreSecret(SecretId id, string value, SecretPolicy policy)
    RevokeSecret(SecretId id)
```

**Bước 2.14 — Secret Management implementation**

- MVP: encrypted local file (AES-256, key from Windows DPAPI)
- Access scope enforcement, Expiration tracking
- Unit test: store, resolve, revoke, access denied, expiration

---

## Phase 3 — Runtime Engine

> **Mục tiêu:** Triển khai Runtime execution engine: ExecutionScope, ExecutionRevision, WorkItem, Attempt, Artifact Store.
> **Kết quả:** Runtime nhận work, execute, cancel, publish artifacts.
> **Docs:** `01-architecture/runtime/PIPELINE_RUNTIME.md`, `WORK_QUEUE.md`, `SCHEDULER.md`, `CANCELLATION.md`, `MEMORY_MODEL.md`, `THREADING_MODEL.md`

**Bước 3.1 — Core Runtime types**

```csharp
record ExecutionScopeId(Guid Value)
record ExecutionRevisionId(Guid Value)
record WorkItemId(Guid Value)
record AttemptId(Guid Value)
record RuntimeArtifactRef(string Key, int Generation)
```

**Bước 3.2 — `ExecutionScope` và `ExecutionRevision`**

- `ExecutionScope`: gắn với ReadingSession, quản lý cancellation scope
- `ExecutionRevision`: phiên bản nội dung hiện tại, "latest wins"
- Khi revision mới: cancel revision cũ, discard stale artifacts

**Bước 3.3 — `WorkItem` và `Attempt`**

- `WorkItem`: descriptor nhẹ (không chứa payload lớn)
- `Attempt`: 1 lần execute WorkItem, có CancellationToken riêng
- Result: `Success(artifact)` | `Failed(error, retryHint)` | `Cancelled`

**Bước 3.4 — `IRuntimeArtifactStore`**

- Store/retrieve immutable artifacts theo RevisionId + stage
- Cleanup khi revision hết hiệu lực
- Implementation: in-memory `ConcurrentDictionary`

**Bước 3.5 — `PipelineCoordinator`**

- Nhận `ExecutionRevision`
- Tạo `BusinessExecutionPlan` (danh sách WorkItem stages)
- Enqueue WorkItems vào `WorkQueue`

**Bước 3.6 — `WorkQueue`**

- Bounded `System.Threading.Channels.Channel<WorkItem>`
- Priority support (HIGH, NORMAL, LOW)
- Revision validation: reject stale work items

**Bước 3.7 — `WorkerPool`**

- N background workers đọc từ WorkQueue
- `while (!shutdown) { item = await queue.Read(); await Execute(item); }`
- Concurrency limit configurable

**Bước 3.8 — Execution Authority Validation**

- Trước khi commit artifact: xác nhận RevisionId vẫn là current
- Nếu stale: discard artifact, không update UI
- "Latest Valid Revision Wins" invariant

**Bước 3.9 — Artifact Publication flow**

```text
Attempt produces Candidate Artifact
    → Execution Authority Validation
    → (if current) Ownership Transfer to ArtifactStore
    → RuntimeArtifactPublished event
    → Business Module Acceptance
    → Next Stage WorkItem created
```

**Bước 3.10 — Runtime unit tests**

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

> **Mục tiêu:** Capture module với Windows.Graphics.Capture hoặc DXGI (kết quả Gate 2).
> **Kết quả:** App capture region màn hình và expose frame qua Runtime artifacts.
> **Docs:** `02-modules/capture/MODULE.md`, `02-modules/capture/CONTRACT.md`, `04-technology/WINDOWS_PLATFORM.md`

**Bước 5.1 — Capture interfaces (platform-neutral)**

```csharp
interface ICaptureProvider
    StartCapture(CaptureRegion region, CancellationToken ct) → IAsyncEnumerable<CapturedFrame>
    StopCapture()

record CapturedFrame(FrameId Id, ReadingSessionId SessionId,
    ReadOnlyMemory<byte> ImageData, ImageFormat Format,
    CaptureRegion Region, DateTime CapturedAt)
```

**Bước 5.2 — Region Selection UI**

- Overlay window cho phép user vẽ rectangle chọn capture region
- Output: `CaptureRegion` với screen coordinates
- Test: multi-monitor, DPI scaling

**Bước 5.3 — `WindowsCaptureProvider`**

- Implement `ICaptureProvider` dùng Windows.Graphics.Capture API
- Frame output: BGRA byte array
- Frame rate: on-demand trigger hoặc polling 1fps
- Register trong DI container, keyed by platform

**Bước 5.4 — Change Detection**

- So sánh frame mới vs frame cũ (pixel hash hoặc perceptual hash)
- Chỉ trigger OCR khi frame thực sự thay đổi đáng kể
- Threshold configurable

**Bước 5.5 — Capture → Runtime integration**

- Khi frame captured: tạo `ExecutionRevision` mới
- Store `CapturedFrame` vào `RuntimeArtifactStore`
- Enqueue WorkItem cho Recognition stage
- Unit test: capture → revision creation → artifact store

---

## Phase 6 — Recognition Module (OCR)

> **Mục tiêu:** Recognition module với OCR engine đã chọn ở Gate 3.
> **Kết quả:** Image → `RecognitionArtifact` (structured text regions + bounding boxes).
> **Docs:** `02-modules/recognition/MODULE.md`, `01-architecture/ocr/PIPELINE.md`, `01-architecture/ocr/RECOGNITION.md`

**Bước 6.1 — Recognition types**

```csharp
record TextRegion(RegionId Id, BoundingBox Box, string RawText,
    float Confidence, TextDirection Direction)
record RecognitionArtifact(FrameId SourceFrameId,
    IReadOnlyList<TextRegion> Regions,
    LanguageCode DetectedLanguage, DateTime RecognizedAt)
```

**Bước 6.2 — `ITextRecognizer` interface**

```csharp
interface ITextRecognizer
    RecognizeAsync(ReadOnlyMemory<byte> imageData, RecognitionOptions options,
        CancellationToken ct) → Task<CandidateRecognitionArtifact>
```

**Bước 6.3 — OCR engine adapter**

- Implement `ITextRecognizer` cho engine đã chọn
- Wrap output thành `TextRegion` list
- Handle: confidence threshold, region filtering
- Register như replaceable provider

**Bước 6.4 — Image preprocessing**

- Grayscale, contrast enhancement, noise reduction, scale normalization
- Configurable preprocessing pipeline

**Bước 6.5 — Reading order reconstruction**

- Sort TextRegions theo manga reading order (RTL, Top-to-Bottom)
- Group regions vào logical blocks (speech bubbles, narration)

**Bước 6.6 — Recognition → Runtime integration**

- Worker nhận WorkItem `{stage: Recognition, revisionId}`
- Load `CapturedFrame` từ ArtifactStore
- Call `ITextRecognizer`
- Produce `CandidateRecognitionArtifact`
- Submit cho Runtime authority validation
- Unit test: full recognition pipeline, stale revision discard

---

## Phase 7 — Text Processing Module

> **Mục tiêu:** Normalize OCR output, build TranslationUnits.
> **Kết quả:** `RecognitionArtifact` → `SourceDocument` → `TranslationUnit` list.
> **Docs:** `02-modules/text-processing/MODULE.md`, `01-architecture/text/TEXT_MODEL.md`, `01-architecture/text/SEGMENTATION.md`

**Bước 7.1 — Source Document types**

```csharp
record SourceDocument(SourceId Id, IReadOnlyList<TextSegment> Segments,
    LanguagePair LanguagePair, ContentType ContentType)
record TextSegment(string Text, SegmentType Type, RegionId? SourceRegionId)
record TranslationUnit(string SourceText,
    IReadOnlyList<RegionId> SourceRegionIds, ContextHints Hints)
```

**Bước 7.2 — OCR output normalization**

- Merge adjacent regions với cùng logical context
- Fix common OCR artifacts
- Language-specific normalization (Simplified/Traditional Chinese)

**Bước 7.3 — Text segmentation**

- Split thành segments phù hợp cho translation
- Preserve: dialogue boundaries, paragraph structure, emphasis
- Handle: vertical text reflow to horizontal

**Bước 7.4 — Context building**

- Gắn context: page number, region type (dialogue/narration/SFX)
- Build `TranslationUnit` với hints
- Unit test: normalization, segmentation, context building

---

## Phase 8 — Translation Module

> **Mục tiêu:** Translation module với provider đã chọn ở Gate 4.
> **Kết quả:** `TranslationUnit` list → `TranslationResult` list.
> **Docs:** `02-modules/translation/MODULE.md`, `01-architecture/translate/TRANSLATION.md`, `01-architecture/translate/CONTEXT.md`

**Bước 8.1 — Translation types**

```csharp
record TranslationResult(TranslationUnit Source, string TranslatedText,
    float Confidence, string ProviderName, DateTime TranslatedAt)
record TranslationCache(string SourceHash, string TranslatedText,
    LanguagePair Pair, DateTime CachedAt)
```

**Bước 8.2 — `ITranslationProvider` interface**

```csharp
interface ITranslationProvider
    TranslateAsync(IReadOnlyList<TranslationUnit> units, TranslationContext ctx,
        CancellationToken ct) → IAsyncEnumerable<TranslationResult>
    string ProviderName { get; }
    bool SupportsStreaming { get; }
```

**Bước 8.3 — AI Provider adapter** *(provider chọn ở Gate 4)*

- Implement `ITranslationProvider`
- System prompt: manga context, character names, glossary
- Handle: context window, token limits, streaming
- Retry với exponential backoff
- Key retrieval qua Secret Management

**Bước 8.4 — Translation cache**

- Cache key: SHA256(source text + language pair)
- Storage: SQLite
- TTL: configurable (default 30 days)
- Check cache trước khi gọi provider

**Bước 8.5 — Context management**

- Maintain translation context window (last N segments)
- Character/glossary context injection
- Auto-detect language nếu không rõ

**Bước 8.6 — Provider fallback**

- Nếu primary fail: try fallback provider
- Log fallback event qua Event Bus
- Unit test: translate, cache hit/miss, fallback, cancellation

---

## Phase 9 — Presentation & UI

> **Mục tiêu:** Presentation module và Avalonia UI hoàn chỉnh.
> **Kết quả:** Translated text hiển thị trên Side Panel.
> **Docs:** `02-modules/presentation/MODULE.md`, `02-modules/ui-adapter/MODULE.md`

**Bước 9.1 — Presentation types**

```csharp
record PresentationModel(ReadingSessionId SessionId,
    ExecutionRevisionId RevisionId,
    IReadOnlyList<TranslatedSegmentView> Segments,
    PresentationLayout Layout)
record TranslatedSegmentView(string SourceText, string TranslatedText,
    BoundingBox? SourceRegion, SegmentType Type)
```

**Bước 9.2 — `IPresentationBuilder`**

- Input: `TranslationResult` list + `SourceDocument` + `RecognitionArtifact`
- Output: `PresentationModel`
- Xác định layout: Side Panel hoặc Overlay

**Bước 9.3 — Side Panel layout (MVP)**

- Scrollable list: source text (nhỏ, mờ) + translated text (to, rõ)
- Font: Inter hoặc system font, configurable size
- Theme: Dark mode default

**Bước 9.4 — Side Panel Avalonia View**

- `SidePanelWindow.axaml` với ViewModel binding
- `SidePanelViewModel` với `ObservableCollection<SegmentItemViewModel>`
- Update khi nhận `PresentationModelPublished` event
- Smooth scroll animation

**Bước 9.5 — UI Adapter layer**

- `IUiAdapter`: nhận `PresentationModel`, update ViewModel
- ViewModel là immutable projection — không chứa business logic
- Commands: `StartCapture`, `StopCapture`, `OpenSettings`

**Bước 9.6 — Main Window và Navigation**

- Main Window: Side Panel + Settings entry + Status bar
- Status bar: session state, provider status, last capture time
- Hotkey binding display

**Bước 9.7 — Settings Window**

- Language pair selection
- Hotkey configuration
- Provider selection + API key input (via Secret Management)
- Appearance: font size, theme

**Bước 9.8 — Overlay layout** *(defer, sau Gate 6)*

- Transparent `OverlayWindow` over source content
- Positioned translated text near source region
- Click-through khi không tương tác

---

## Phase 10 — Storage, Diagnostics & Polish

> **Mục tiêu:** Hoàn thiện Storage, Diagnostics, Provider Management, polish UX, packaging.
> **Kết quả:** App đầy đủ tính năng MVP, ổn định, có thể đóng gói.
> **Docs:** `02-modules/storage/MODULE.md`, `02-modules/diagnostics/MODULE.md`, `04-technology/PERSISTENCE.md`

**Bước 10.1 — SQLite schema và migrations**

- Entity Framework Core + SQLite provider
- Tables: `reading_sessions`, `translation_cache`, `preferences`, `provider_configs`, `audit_logs`
- EF Core migrations
- Test: migration up/down, data integrity

**Bước 10.2 — Storage module implementation**

- `IStorageService`: versioned save/load/delete
- Implement với SQLite backend
- Soft delete, schema evolution support

**Bước 10.3 — Diagnostics module**

- `IDiagnosticsService`: health snapshot, performance counters
- Collect từ: Runtime, Provider Management, Resource Manager
- Expose qua Settings window (Advanced tab)
- No sensitive data in diagnostics

**Bước 10.4 — Provider Management**

- `IProviderRegistry`: register, unregister, get provider
- Provider health check: ping provider, report status
- Auto-disable failed provider sau N failures
- UI: provider status indicator in status bar

**Bước 10.5 — Error handling và User feedback**

- Mọi lỗi hiển thị thông báo thân thiện (không raw stack trace)
- Retry button cho network errors
- "Provider unavailable" state với clear message

**Bước 10.6 — Boot sequence**

- Thứ tự: Infrastructure → Runtime → Modules → UI
- Fail fast nếu critical component không load
- Progress indicator khi load models nặng (OCR)

**Bước 10.7 — Logging và Telemetry finalization**

- Audit log: không có sensitive data
- Performance counters: capture latency, OCR latency, translation latency
- Structured log review

**Bước 10.8 — Packaging** *(sau Gate 7)*

- Chọn format: MSIX / MSI / portable ZIP (theo Gate 7 kết quả)
- Self-contained .NET runtime hoặc require .NET installed
- Bao gồm OCR models
- Code signing nếu release

**Bước 10.9 — End-to-End acceptance test**

- Mở app → chọn region → hotkey capture → xem translation
- Time: < 3s từ capture đến hiển thị
- Test với: Simplified Chinese manga, Traditional Chinese manhua, English comic
- Memory: < 500MB sau 30 phút sử dụng

**Bước 10.10 — MVP Release Checklist**

- [ ] Tất cả unit tests pass
- [ ] E2E acceptance test pass
- [ ] Không có sensitive data trong logs
- [ ] Hotkeys hoạt động khi app không focus
- [ ] App không crash khi minimize/restore
- [ ] Packaging thành công
- [ ] Cài đặt trên máy sạch thành công
- [ ] Uninstall sạch

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
