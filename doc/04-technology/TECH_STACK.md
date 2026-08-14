# CRAI Technology Stack

Status: Proposed Baseline
Version: 0.2.0
Updated: 2026-08-14
Path: 04-technology/TECH_STACK.md

## 1. Purpose

Tài liệu này định nghĩa technology baseline của CRAI cho giai đoạn triển khai.

Technology Selection phải phục vụ kiến trúc đã được khóa trong:

- `.meta/`
- `01-architecture/`
- `02-modules/`
- `03-infrastructure/`

Technology không được trở thành lý do để thay đổi ownership, contract hoặc Runtime boundary đã định nghĩa nếu chưa có một conflict kỹ thuật cụ thể được chứng minh.

Nguyên tắc:

```text
Architecture defines the constraints.

Technology implements those constraints.

Evidence-based decisions refine implementation.

Technology does not redefine architecture by default.
```

## 2. Decision Model

Technology decisions trong CRAI được chia thành ba nhóm.

### 2.1 Selected Baseline

Đã đủ cơ sở để dùng làm implementation baseline.

Ví dụ:

- C#
- .NET 10 LTS
- Avalonia UI
- Windows-first
- Modular Monolith
- SQLite
- predominantly single-process

### 2.2 Candidate Decision

Có hướng rõ ràng nhưng chưa được phép khóa trước khi có feasibility test hoặc benchmark.

Ví dụ:

- OCR engine
- OCR runtime
- Translation provider
- Windows capture API
- Overlay implementation
- packaging format

### 2.3 Deferred Decision

Chưa cần quyết định vì chưa có requirement thực tế đủ mạnh.

Ví dụ:

- IPC mechanism khi chưa có isolated worker
- Linux-specific capture backend
- distributed execution
- plugin sandboxing implementation

## 3. Current Decision Status

| Area | Selection | Status |
| --- | --- | --- |
| Core Language | C# | Selected Baseline |
| Runtime | .NET 10 LTS | Selected Baseline |
| Desktop UI | Avalonia UI | Selected Baseline |
| Initial Platform | Windows x64 | Selected Baseline |
| Architecture Portability | Windows/Linux capable | Selected Baseline |
| Application Topology | Modular Monolith | Selected Baseline |
| Default Process Model | Predominantly single-process | Selected Baseline |
| Persistence Baseline | SQLite | Selected Baseline |
| Native Integration | Platform-specific adapters | Selected Baseline |
| OCR Engine | Not selected | Candidate Decision |
| OCR Runtime | ONNX/native/Python worker candidates | Candidate Decision |
| Translation Provider | Not selected | Candidate Decision |
| Capture API | Windows.Graphics.Capture / DXGI candidates | Candidate Decision |
| Overlay Implementation | Not selected | Candidate Decision |
| IPC | Only when isolation is justified | Deferred |
| Build / Packaging | Not selected | Candidate Decision |
| Testing | xUnit baseline, full stack pending | Candidate Decision |

## 4. Decision Dependency Rules

Một số Technology Decisions phụ thuộc trực tiếp vào kết quả feasibility hoặc benchmark.

Các dependency sau là bắt buộc.

### 4.1 OCR Engine

OCR engine cụ thể không được chọn bằng preference.

```text
OCR Candidates
    ↓
Benchmark on CRAI Data
    ↓
Quality / Performance / Packaging Evidence
    ↓
OCR Engine Decision
```

Decision phải dựa trên:

- Simplified Chinese accuracy
- Traditional Chinese accuracy
- English accuracy
- manga/manhua text
- vertical text
- rotated text
- geometry quality
- latency
- memory
- CPU/GPU requirement
- deployment complexity
- offline capability
- license
- model size
- Windows compatibility

Không được ghi một engine là canonical chỉ vì nó phổ biến.

### 4.2 Translation Provider

Translation provider phải được chọn bằng quality test thực tế.

```text
Translation Candidates
    ↓
Chinese → Vietnamese Evaluation
    ↓
Quality / Context / Cost / Latency Evidence
    ↓
Initial Provider Decision
```

Evaluation phải bao gồm:

- Simplified Chinese → Vietnamese
- Traditional Chinese → Vietnamese
- novel prose
- dialogue
- manga/manhua context
- glossary handling
- terminology consistency
- context-window behavior
- latency
- streaming behavior nếu dùng
- cost
- privacy

Không được chọn DeepL, Google, OpenAI, Gemini hoặc provider khác chỉ theo reputation.

### 4.3 Overlay Implementation

Overlay implementation cụ thể không được khóa trước Desktop Feasibility.

```text
Desktop Skeleton
    ↓
Avalonia + Windows Native Feasibility
    ↓
Window / DPI / Transparency / Capture Tests
    ↓
Overlay Implementation Decision
```

Overlay implementation phụ thuộc kết quả Gate 1 và Windows platform feasibility.

Có thể sử dụng:

- Avalonia window features
- Win32
- Windows App SDK APIs
- platform-specific native helper

nhưng exact implementation chỉ được chọn sau prototype.

### 4.4 Packaging

Packaging phụ thuộc OCR runtime.

```text
OCR Engine
    ↓
OCR Runtime
    ↓
Native / Model Dependencies
    ↓
Packaging Constraints
    ↓
Packaging Decision
```

Ví dụ:

```text
Pure .NET / ONNX
    → relatively simple packaging

Native DLL OCR
    → native dependency packaging

Python Worker
    → Python runtime / environment / model packaging

Large Local Model
    → model asset and update strategy
```

Không khóa MSIX, MSI, WiX, Inno Setup hoặc packaging format khác trước khi OCR runtime được xác định.

## 5. Core Language

Selected:

```text
C#
```

C# là primary implementation language của CRAI.

Lý do:

- strong typing
- mature async model
- cooperative cancellation
- immutable record support
- desktop integration
- native interop
- HTTP/provider integration
- background workers
- dependency injection
- mature testing ecosystem
- phù hợp Runtime contracts hiện tại

Các architecture concepts như:

```text
ExecutionScope
ExecutionRevision
WorkItem
Attempt
ExecutionBinding
RuntimeArtifactRef
ResourceLease
CancellationContext
```

có thể biểu diễn trực tiếp bằng strongly typed contracts.

C# không bắt buộc mọi Provider hoặc model runtime phải được viết bằng C#.

## 6. Runtime

Selected:

```text
.NET 10 LTS
```

.NET là application runtime chính.

Runtime phải hỗ trợ:

- async/await
- CancellationToken
- bounded concurrency
- bounded channels/queues
- background services
- dependency injection
- HTTP clients
- structured configuration
- native interop
- process management
- local persistence
- testability

CRAI desktop MVP không sử dụng microservice architecture.

## 7. Desktop UI Framework

Selected:

```text
Avalonia UI
```

Mục tiêu:

```text
Windows-first implementation
+
cross-platform architecture
```

Avalonia được chọn vì:

- phù hợp C#/.NET
- desktop-first
- không bắt buộc web runtime
- giữ đường Windows/Linux
- hỗ trợ XAML-style UI
- phù hợp Side Panel MVP
- có thể kết hợp Windows-specific native APIs khi cần

UI layer phải phụ thuộc application/public contracts thay vì Runtime internals.

Canonical direction:

```text
Avalonia View
    ↓
UI Adapter / Presentation Boundary
    ↓
Application Contracts
    ↓
Business Modules
```

Business Logic không nằm trong View.

## 8. Initial Platform

Selected MVP target:

```text
Windows x64
```

Windows được ưu tiên trước vì các capability quan trọng phụ thuộc desktop platform:

- screen capture
- window enumeration
- active-window tracking
- global hotkeys
- region selection
- overlay
- DPI handling
- native window handles
- capture exclusion
- clipboard integration

Cross-platform support không phải MVP acceptance requirement.

## 9. Architecture Portability

Architecture phải giữ khả năng thêm Linux sau MVP.

Platform-specific behavior phải đi qua adapters.

Ví dụ:

```text
IWindowService
ICaptureProvider
IOverlayHost
IGlobalHotkeyService
IClipboardService
IPlatformNotificationService
```

MVP implementation:

```text
WindowsWindowService
WindowsCaptureProvider
WindowsOverlayHost
WindowsGlobalHotkeyService
WindowsClipboardService
```

Không hard-code Win32 vào Business Module contracts.

macOS chưa phải target nhưng không nên bị khóa architecture không cần thiết.

## 10. Application Architecture

Selected:

```text
Modular Monolith
```

CRAI không dùng microservices cho desktop MVP.

Logical modules vẫn giữ ownership độc lập theo `02-modules/`.

Conceptually:

```text
CRAI Application
    │
    ├── Reading
    ├── Capture
    ├── Recognition
    ├── Text Processing
    ├── Translation
    ├── Presentation
    ├── Storage
    ├── Preferences
    ├── Diagnostics
    └── UI Adapter
```

Module boundary không mặc định là process boundary.

## 11. Process Model

Selected default:

```text
Predominantly single-process
```

MVP không tạo process isolation nếu chưa có lý do kỹ thuật cụ thể.

Default:

```text
CRAI Desktop Process
    ├── UI
    ├── Application
    ├── Runtime
    ├── Business Modules
    ├── Infrastructure
    └── compatible Providers
```

Optional isolated worker chỉ được thêm khi dependency yêu cầu:

- Python-only model
- unstable native library
- GPU runtime cần isolation
- memory-heavy model
- crash containment
- security boundary
- incompatible runtime dependency

Isolation không thay đổi Business Architecture.

## 12. Native Platform Integration

Selected strategy:

```text
C# platform adapters
+
Windows native APIs when required
```

Candidate API groups:

- Win32
- WinRT
- Windows App SDK APIs
- DirectX/DXGI APIs where justified

Không tạo native abstraction layer lớn trước use case.

Business Modules không gọi Win32 trực tiếp.

## 13. Windows Capture Technology

Status:

```text
Candidate Decision
```

Primary candidates:

```text
Windows.Graphics.Capture
DXGI Desktop Duplication
```

Secondary fallback candidates chỉ được xem xét nếu primary candidates không đáp ứng requirement.

Evaluation criteria:

- window capture
- display capture
- region capture support
- latency
- frame stability
- DPI correctness
- resize/move behavior
- overlay exclusion feasibility
- GPU copy cost
- CPU copy cost
- memory pressure
- cancellation
- native resource lifecycle
- ease of Avalonia integration

Không chọn DXGI chỉ vì performance lý thuyết.

Không chọn Windows.Graphics.Capture chỉ vì API hiện đại.

Decision phải dựa trên CRAI capture prototype.

## 14. Persistence

Selected baseline:

```text
SQLite
```

SQLite dùng cho durable local data như:

- Preferences
- Reading history
- session/business metadata
- glossary
- character data
- translation memory
- provider configuration metadata
- durable indexes

Không mặc định lưu large binary artifacts trực tiếp trong SQLite.

Large data như:

- screenshots
- page images
- model files
- temporary OCR images
- large generated artifacts

phải theo Artifact/Storage policy tương ứng.

Các concept vẫn tách:

```text
Runtime Artifact Store
!=
Runtime Cache
!=
Persistent Storage
```

Exact .NET SQLite access library chưa được chọn.

Candidate options:

- Microsoft.Data.Sqlite
- Dapper
- EF Core SQLite
- custom thin repository layer

Decision phải dựa vào persistence model thực tế.

## 15. OCR and Recognition Technology

Status:

```text
Candidate Evaluation Required
```

OCR Architecture không phụ thuộc engine cụ thể.

Current benchmark candidates:

```text
PaddleOCR
RapidOCR
ONNX-compatible OCR models
Windows OCR
Remote OCR provider
```

Không candidate nào mặc định là winner.

Canonical boundary:

```text
Recognition
    ↓
Execution Requirement
    ↓
Routing / Provider Management
    ↓
Resolved ExecutionBinding
    ↓
Runtime
    ↓
OCR Provider Adapter
    ↓
OCR Runtime
```

## 16. OCR Runtime Candidates

Candidate classes:

```text
In-process .NET / native OCR
ONNX Runtime
External native worker
Python OCR worker
Remote OCR provider
```

Preferred rule:

```text
Use in-process when simple and stable.

Use isolated worker when ecosystem or isolation benefits justify it.
```

Python không phải primary application runtime.

Python chỉ là optional worker runtime.

## 17. OCR Benchmark Dataset

OCR benchmark phải sử dụng dữ liệu gần use case thực tế.

Tối thiểu:

- Simplified Chinese novel screenshots
- Traditional Chinese novel screenshots
- Chinese manhua
- mixed Chinese/English
- vertical text
- rotated text
- small font
- low contrast
- noisy comic background
- speech bubbles
- text near illustration edges

Metrics:

- text accuracy
- line accuracy
- region recall
- region precision
- geometry quality
- reading-order usability
- latency
- memory
- initialization time
- model size

Benchmark result phải được ghi riêng, không chèn cảm tính vào TECH_STACK.

## 18. Translation Technology

Status:

```text
Provider Evaluation Required
```

Translation Module phải provider-independent.

Canonical:

```text
Translation Module
    ↓
Translation Contract
    ↓
Routing
    ↓
ExecutionBinding
    ↓
Provider Adapter
```

Initial provider candidates có thể gồm:

```text
DeepL
Google
OpenAI
Gemini
Local model
```

Danh sách này là candidate list, không phải recommendation final.

## 19. Translation Evaluation

Quality benchmark phải tập trung Chinese → Vietnamese.

Tối thiểu test:

- Simplified Chinese
- Traditional Chinese
- novel narration
- dialogue
- formal speech
- informal speech
- names
- titles
- honorific/addressing
- idioms
- long context
- glossary enforcement
- consistency across chapters
- manga dialogue fragments

Evaluation dimensions:

- semantic accuracy
- Vietnamese naturalness
- terminology consistency
- context consistency
- glossary adherence
- formatting preservation
- latency
- streaming quality
- cost
- privacy
- rate limits

Initial provider chỉ được chọn sau evaluation.

## 20. Local Translation

Local Translation là optional capability.

Không bắt buộc MVP phải bundle Local LLM.

Preferred MVP rule:

```text
One reliable remote provider first
+
provider-neutral architecture
```

Local model chỉ được thêm khi:

- quality đủ tốt
- hardware requirement chấp nhận được
- package size hợp lý
- privacy benefit rõ ràng
- operational complexity đáng giá

## 21. Text Flow

Structured text luôn được ưu tiên khi available.

```text
Structured Text Source
    ↓
Text Processing
    ↓
Translation
```

Không ép structured text qua OCR.

## 22. Image Flow

Canonical:

```text
Capture
    ↓
Source Image
    ↓
OCR
    ↓
OCR Document
    ↓
Reading Order
    ↓
Text Processing
    ↓
Translation
    ↓
Presentation
```

Capture implementation phải giữ:

- image identity
- source geometry
- window correlation khi cần
- DPI correctness
- coordinate mapping
- cancellation
- resource ownership

## 23. UI Strategy

MVP presentation priority:

```text
Side Panel first
```

Recommended progression:

```text
Desktop Skeleton
    ↓
Side Panel
    ↓
Region Selection
    ↓
Window-aware Capture
    ↓
Basic Overlay Prototype
    ↓
Overlay Decision
```

Overlay không được block toàn bộ MVP nếu chưa cần.

## 24. Overlay Decision

Status:

```text
Candidate Decision
```

Không khóa implementation sớm.

Phải kiểm tra:

- transparency
- click-through
- always-on-top
- DPI scaling
- window movement
- window resize
- coordinate mapping
- capture exclusion
- multi-monitor
- z-order behavior
- focus behavior
- rendering performance

Possible implementation mechanisms:

```text
Avalonia window features
Win32 extensions
Windows App SDK APIs
small native helper
```

Exact choice chỉ được khóa sau feasibility prototype.

## 25. IPC

Status:

```text
Deferred Until Required
```

Không tạo IPC architecture nếu không có worker thật.

Khi có isolated worker, IPC contract phải:

- serializable
- versioned
- bounded
- cancellation-aware
- timeout-aware
- provider-neutral
- Runtime-correlation aware

Không truyền internal object graph trực tiếp.

## 26. Eventing

MVP direction:

```text
In-process Event Bus
```

Không dùng:

- Kafka
- RabbitMQ
- Redis Streams
- NATS

cho desktop MVP nếu không có requirement thực tế.

Implementation candidate:

```text
System.Threading.Channels
+
typed Event Bus
```

MediatR không phải requirement.

Chỉ thêm nếu nó đơn giản hóa implementation thay vì tạo thêm abstraction layer.

## 27. Configuration

Baseline:

```text
.NET configuration abstractions
+
local application configuration
```

Exact format chưa khóa.

Consumers dùng immutable typed snapshots.

Secrets không được đặt trong plain configuration.

## 28. Secret Management

Windows-first implementation nên dùng OS-backed secure storage khi khả thi.

Exact implementation cần feasibility test.

Business Modules chỉ consume secret references.

## 29. Logging and Telemetry

Baseline direction:

```text
Microsoft.Extensions.Logging
+
structured logging implementation
```

Exact logging provider chưa khóa.

Runtime Observability phải giữ correlation:

```text
ExecutionScopeId
ExecutionRevisionId
WorkItemId
AttemptId
```

Không log mặc định:

- raw screenshot
- full OCR text
- full translated content
- prompt
- secrets
- provider credentials

## 30. Scheduling and Concurrency

Preferred .NET primitives:

- Task
- CancellationToken
- Channel
- SemaphoreSlim
- bounded queue
- background worker

Architecture yêu cầu:

```text
bounded concurrency
```

Không tạo unbounded task fan-out.

## 31. Resource Management

Image/model workloads phải có explicit resource lifecycle.

Implementation cần dùng:

- IDisposable
- IAsyncDisposable khi cần
- explicit Lease wrapper
- deterministic native-handle cleanup
- bounded buffers

GC không thay thế Resource Lifecycle.

Đặc biệt:

- bitmap/native image
- GPU resource
- model session
- native handle
- mapped memory
- temporary large buffer

## 32. Build System

Baseline:

```text
dotnet CLI
MSBuild
```

Repository phải build được bằng command line.

Expected:

```text
dotnet restore
dotnet build
dotnet test
```

Packaging chưa được khóa.

## 33. Packaging

Status:

```text
Blocked by OCR Runtime Decision
```

Packaging chỉ được quyết định sau khi biết:

- OCR engine
- OCR runtime
- native dependencies
- model assets
- worker process requirement

Candidate Windows formats có thể gồm:

- MSIX
- MSI
- WiX
- Inno Setup
- self-contained folder/package

Không candidate nào được chọn trước feasibility.

## 34. Dependency Enforcement

Module dependency rules phải được enforce bằng:

- project references
- solution structure
- architecture tests khi phù hợp

Ví dụ:

```text
Business Module
    X
Platform Implementation
```

Không dựa hoàn toàn vào convention.

## 35. Testing

Initial baseline:

```text
xUnit
```

Testing stack cần hỗ trợ:

- unit tests
- contract tests
- module tests
- Runtime state tests
- provider adapter tests
- integration tests
- architecture dependency tests
- OCR benchmark tests
- Translation quality tests
- Windows platform integration tests

Mock framework chưa khóa.

Candidates:

- Moq
- NSubstitute
- hand-written fakes

UI E2E framework sẽ chọn sau UI skeleton.

## 36. Proposed Solution Structure

Initial direction:

```text
CRAI.sln

src/
├── Crai.App
├── Crai.Application
├── Crai.Core
├── Crai.Runtime
├── Crai.Composition
│
├── Crai.Modules
│   ├── Reading
│   ├── Capture
│   ├── Recognition
│   ├── TextProcessing
│   ├── Translation
│   ├── Presentation
│   ├── Storage
│   ├── Preferences
│   ├── Diagnostics
│   └── UIAdapter
│
├── Crai.Infrastructure
│   ├── Configuration
│   ├── EventBus
│   ├── Logging
│   ├── Telemetry
│   ├── Scheduling
│   ├── Resources
│   └── Persistence
│
├── Crai.Platform.Windows
│   ├── Capture
│   ├── Windowing
│   ├── Overlay
│   ├── Hotkeys
│   └── Clipboard
│
└── Crai.Providers
    ├── Recognition
    └── Translation

tests/
├── Crai.UnitTests
├── Crai.ContractTests
├── Crai.ArchitectureTests
├── Crai.IntegrationTests
└── Crai.Benchmarks
```

Đây là implementation direction.

Không yêu cầu mỗi folder trở thành assembly riêng.

## 37. Technologies Explicitly Not Selected

### Electron

Không chọn default.

Lý do:

- Chromium overhead
- native bridge vẫn cần
- image/OCR/model memory pressure đã cao

### Flutter

Không chọn default.

Native desktop integration của CRAI quá thường xuyên để Dart bridge trở thành lựa chọn ưu tiên.

### Tauri as Primary Stack

Không chọn default MVP stack.

Rust/Tauri vẫn hợp lệ nhưng sẽ tạo sớm:

```text
Web UI
+
Rust Core
+
IPC boundary
```

trong khi CRAI ưu tiên predominantly single-process.

### C++/Qt as Primary Stack

Không chọn cho toàn application.

Có thể dùng native component riêng khi dependency yêu cầu.

### Python as Primary Runtime

Không chọn.

Python chỉ là optional model/OCR worker runtime.

### Microservices

Không chọn.

### Distributed Broker

Không chọn cho MVP.

## 38. Technology Decision Rules

Một technology chỉ được thêm nếu giải quyết requirement cụ thể.

Không thêm chỉ vì:

- phổ biến
- quen thuộc
- có thể cần sau này
- có nhiều library

Mỗi dependency lớn phải đánh giá:

```text
Architecture Fit
Functionality
Quality
Performance
Memory
Deployment
Security
Privacy
License
Maintenance
Testing
Replaceability
```

## 39. Feasibility Gates

### Gate 1 - Desktop Skeleton

Xác nhận:

- .NET 10 app chạy ổn
- Avalonia Side Panel chạy ổn
- DI/composition hoạt động
- Windows platform adapter boundary hoạt động
- basic transparent window feasibility nếu cần
- DPI behavior đủ tốt

Gate 1 không khóa final Overlay implementation.

Nó chỉ xác nhận stack có khả năng đi tiếp.

### Gate 2 - Capture

Prototype:

- Windows.Graphics.Capture
- DXGI Desktop Duplication

Đánh giá:

- latency
- correctness
- memory
- window behavior
- DPI
- coordinate mapping
- overlay exclusion feasibility

Sau Gate 2 mới khóa primary capture API.

### Gate 3 - OCR

Benchmark OCR candidates trên CRAI dataset.

Sau Gate 3 mới khóa:

- initial OCR engine
- OCR runtime
- process/isolation requirement

### Gate 4 - Translation

Benchmark Chinese → Vietnamese.

Sau Gate 4 mới khóa initial Translation provider.

### Gate 5 - End-to-End Slice

Xác nhận:

```text
Capture
    ↓
Recognition
    ↓
Text Processing
    ↓
Translation
    ↓
Side Panel
```

không bypass Runtime contracts.

### Gate 6 - Overlay Prototype

Chỉ thực hiện sau Gate 1 và platform integration đủ ổn.

Sau Gate 6 mới khóa Overlay implementation strategy.

### Gate 7 - Packaging

Chỉ thực hiện sau OCR Runtime Decision.

Xác nhận package có thể chứa:

- application runtime
- native dependencies
- selected OCR runtime
- model assets nếu có
- worker nếu có
- configuration
- secret integration

Sau Gate 7 mới khóa packaging format.

## 40. Decision Graph

Canonical dependency graph:

```text
C# / .NET / Avalonia
        |
        v
Desktop Skeleton
        |
        +------------------+
        |                  |
        v                  v
Capture Prototype      Overlay Feasibility
        |                  |
        v                  v
Capture Decision       Overlay Decision

OCR Candidates
        |
        v
OCR Benchmark
        |
        v
OCR Engine
        |
        v
OCR Runtime
        |
        v
Packaging Constraints
        |
        v
Packaging Decision

Translation Candidates
        |
        v
Chinese → Vietnamese Benchmark
        |
        v
Initial Translation Provider
```

Không được đảo ngược dependency này chỉ để hoàn thành tài liệu nhanh hơn.

## 41. Decisions Still Open

1. exact SQLite access library;
2. exact persistence abstraction;
3. primary Windows capture API;
4. exact region-selection implementation;
5. global hotkey implementation;
6. overlay implementation;
7. OCR engine;
8. OCR runtime;
9. Translation provider;
10. local Translation strategy;
11. secret-storage implementation;
12. structured logging provider;
13. telemetry implementation;
14. IPC mechanism nếu worker xuất hiện;
15. packaging format;
16. architecture-test library;
17. mock/fake strategy;
18. UI E2E framework;
19. update mechanism.

## 42. Implementation Entry Criteria

Implementation skeleton có thể bắt đầu khi:

```text
Core Language
    ✓

Runtime
    ✓

Desktop Framework
    ✓

Initial Platform
    ✓

Application Topology
    ✓

Persistence Baseline
    ✓

Gate 1 Desktop Skeleton
    ✓

MVP Acceptance/Test Strategy
    ✓
```

Không cần đợi OCR/Translation final decision để tạo skeleton và contracts.

Tuy nhiên production MVP không được coi technology-stable trước khi:

```text
Capture Decision
OCR Decision
Translation Decision
Packaging Decision
```

đã vượt feasibility gates.

## 43. Selected Baseline Summary

```text
Language
    C#

Runtime
    .NET 10 LTS

Desktop UI
    Avalonia UI

Initial Platform
    Windows x64

Portability Goal
    Windows/Linux capable

Architecture
    Modular Monolith

Process Model
    Predominantly single-process

Persistence
    SQLite

Capture
    Windows.Graphics.Capture / DXGI
    benchmark required

OCR
    PaddleOCR / RapidOCR / ONNX / Windows OCR / remote candidates
    benchmark required

OCR Runtime
    in-process / ONNX / native / Python worker
    evidence required

Translation
    provider-neutral
    Chinese → Vietnamese quality benchmark required

Overlay
    implementation deferred until feasibility

Packaging
    blocked by OCR runtime

Eventing
    in-process typed Event Bus

Build
    dotnet CLI + MSBuild

Testing
    xUnit baseline
```

## 44. Next Technology Documents

Recommended order:

```text
TECH_STACK.md
    ↓
PERSISTENCE.md
    ↓
WINDOWS_PLATFORM.md
    ↓
OCR_CANDIDATES.md
    ↓
TRANSLATION_CANDIDATES.md
    ↓
FEASIBILITY_RESULTS.md
    ↓
BUILD_AND_PACKAGING.md
    ↓
TESTING.md
    ↓
MVP_IMPLEMENTATION_PLAN.md
```

`FEASIBILITY_RESULTS.md` là nơi ghi evidence và kết luận từ các gates.

Không dùng `TECH_STACK.md` để giả lập kết quả benchmark chưa tồn tại.

## 45. Final Principle

Technology stack của CRAI phải giữ:

```text
Business Architecture stays stable.

Providers remain replaceable.

Platform-specific code stays behind adapters.

Runtime owns execution authority.

Evidence selects implementations.

Unproven assumptions remain candidates.
```

Mục tiêu của Technology Selection là chọn stack nhỏ nhất có thể triển khai CRAI đúng kiến trúc và chỉ khóa những quyết định đã có đủ bằng chứng.
