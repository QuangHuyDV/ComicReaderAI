# CRAI Windows Platform Technology

Status: Proposed Baseline
Version: 0.1.0
Updated: 2026-08-14
Path: 04-technology/WINDOWS_PLATFORM.md
Depends On:
- 04-technology/TECH_STACK.md
- 04-technology/PERSISTENCE.md

## 1. Purpose

Tài liệu này định nghĩa technology direction cho Windows-specific implementation của CRAI.

Phạm vi chính:

- Windows application/window integration
- screen/window capture
- region selection
- coordinate systems
- DPI
- global hotkeys
- clipboard
- Side Panel window behavior
- Overlay feasibility
- capture exclusion
- multi-monitor behavior
- native handle/resource lifecycle
- Windows user-data integration
- Windows secret-storage boundary

Tài liệu này không khóa những implementation cần feasibility evidence trước khi prototype hoàn thành.

Đặc biệt:

```text
Primary Capture API
    → Candidate Decision

Final Overlay Implementation
    → Candidate Decision

Packaging Format
    → Out of scope here
```

## 2. Platform Baseline

Selected:

```text
Initial Platform
    Windows x64

Application Runtime
    .NET 10 LTS

Desktop UI
    Avalonia UI

Platform Integration
    C# adapters
    +
    Windows native APIs when required
```

Windows-first là implementation priority.

Business Architecture vẫn phải giữ platform-independent contracts.

## 3. Platform Boundary

Canonical dependency:

```text
Business Modules
    ↓
Application / Public Contracts
    ↓
Platform Capability Interfaces
    ↓
Crai.Platform.Windows
    ↓
Win32 / WinRT / DirectX / Windows APIs
```

Không cho Business Modules gọi trực tiếp:

- HWND APIs
- Win32
- COM
- WinRT capture objects
- DXGI
- Direct3D
- Windows clipboard handles

Native/platform types không crossing public Business boundaries.

## 4. Selected Windows Integration Strategy

Selected strategy:

```text
Use Avalonia for normal application UI.

Use Windows APIs only for platform capabilities
that Avalonia does not own or cannot satisfy reliably.
```

Không tạo một Windows-native UI stack thứ hai chỉ vì CRAI cần một vài Win32 capability.

Không mặc định thêm WinUI 3 hoặc WPF vào cùng application.

Nếu một capability cần native implementation:

```text
Avalonia App
    ↓
Platform Contract
    ↓
Windows Adapter
    ↓
Native API
```

## 5. Avalonia Responsibility

Avalonia nên sở hữu normal desktop presentation:

- main application window
- Side Panel
- settings
- dialogs
- standard input
- normal window rendering
- theme
- application-level UI composition

Avalonia trên Windows sử dụng Win32 trực tiếp và hỗ trợ per-monitor DPI behavior cùng transparency capabilities.

Tuy nhiên CRAI không giả định rằng Avalonia abstraction một mình sẽ giải quyết toàn bộ:

- capture
- click-through Overlay
- capture exclusion
- global hotkey
- source-window tracking
- low-level z-order behavior

Các phần này phải được feasibility-test riêng.

## 6. Windows Capability Adapters

Initial platform capability direction:

```text
IWindowService
ICaptureProvider
IRegionSelectionService
IGlobalHotkeyService
IClipboardService
IOverlayHost
IDisplayService
IPlatformNotificationService
IPlatformDataPathService
ISecretProtectionService
```

Tên interface cuối cùng phải theo authoritative architecture/module contracts.

`WINDOWS_PLATFORM.md` chỉ định technology direction, không redefine public contracts.

## 7. Native Handle Boundary

Windows implementation có thể cần:

```text
HWND
HMONITOR
Direct3D device/resource
DXGI output/resource
WinRT capture item/session
```

Các handle này phải nằm dưới Platform/Resource boundary.

Public Business contracts phải dùng provider/platform-neutral identities hoặc descriptors.

Ví dụ:

```text
Business Window Identity
    ↓
Windows Adapter Mapping
    ↓
HWND
```

Không dùng `IntPtr HWND` làm canonical business identity.

## 8. Window Discovery

CRAI cần khả năng tìm và mô tả source windows.

Candidate implementation:

```text
Win32 window enumeration
```

Có thể cần các API thuộc nhóm:

- top-level window enumeration
- window visibility
- window bounds
- process/window metadata
- foreground window
- monitor association

Window discovery phải normalize native data thành platform-neutral descriptor trước khi crossing Platform boundary.

## 9. Window Selection

Window Selection có hai possible UX paths:

```text
CRAI-controlled picker
```

hoặc:

```text
Windows system capture picker
```

Không khóa một path duy nhất trước Capture prototype.

CRAI-controlled picker có lợi cho:

- unified CRAI UX
- explicit source identity
- region workflow
- later window tracking

System picker có lợi cho:

- OS-mediated capture selection
- lower implementation complexity
- platform-consistent permissions/UX

Capture feasibility phải xác định trade-off thực tế.

## 10. Active Window Tracking

Candidate baseline:

```text
Win32-based window state observation
+
bounded polling/event integration as needed
```

Requirements:

- detect selected window move
- detect resize
- detect minimize/restore
- detect close
- detect foreground changes when relevant
- maintain source-window correlation
- avoid tight polling loops

Exact event/polling combination chưa khóa.

Không để UI timer trở thành canonical observation mechanism.

## 11. Window Bounds

Window bounds phải phân biệt khi relevant:

```text
Logical UI coordinates
Physical screen pixels
Client bounds
Window bounds
Capture surface bounds
```

Không assume các coordinate space giống nhau.

Window adapter phải expose đủ metadata để Capture/Overlay mapping xác định conversion đúng.

## 12. DPI Strategy

Selected requirement:

```text
Per-monitor DPI aware behavior
```

Avalonia xử lý normal UI scaling.

Platform adapters vẫn phải xử lý rõ pixel-space mapping cho:

- capture
- region selection
- source-window bounds
- Overlay placement
- multi-monitor transition

Canonical rule:

```text
UI logical coordinate
!=
physical capture pixel
```

Mọi conversion phải explicit.

## 13. Coordinate Mapping

CRAI cần canonical transform path:

```text
UI Coordinate
    ↕
Screen Coordinate
    ↕
Window Coordinate
    ↕
Capture Pixel Coordinate
    ↕
OCR Geometry Coordinate
```

Không dùng ad-hoc offset calculations ở UI.

Transform metadata phải được giữ qua Capture/OCR boundary theo Architecture.

## 14. Multi-Monitor

Windows implementation phải hỗ trợ ít nhất:

- source window trên monitor bất kỳ
- monitors có scaling khác nhau
- source window move giữa monitors
- negative virtual-screen coordinates
- different resolutions
- display topology changes

MVP không cần capture nhiều monitors đồng thời nếu Reading flow không yêu cầu.

Nhưng implementation không được assume primary monitor origin `(0,0)` cho mọi source.

## 15. Capture Decision Status

Status:

```text
Candidate Decision
```

Primary candidates:

```text
Windows.Graphics.Capture
DXGI Desktop Duplication
```

Không candidate nào là winner trước Gate 2.

## 16. Windows.Graphics.Capture Candidate

`Windows.Graphics.Capture` là primary candidate cho window/display capture.

Nó phù hợp để evaluate vì Windows cung cấp capture cho display hoặc application window.

Potential strengths cần kiểm chứng:

- window-oriented capture
- display capture
- modern Windows capture path
- suitable frame acquisition
- source resize handling
- potentially simpler window capture integration

Potential concerns cần kiểm chứng:

- source-selection UX
- capture indicator/system behavior
- programmatic source integration
- region-only workflow
- frame-copy overhead
- capture exclusion interaction
- compatibility with selected Windows support floor

Không coi potential strength là confirmed CRAI result trước prototype.

## 17. DXGI Desktop Duplication Candidate

DXGI Desktop Duplication là primary candidate thứ hai.

Nó cung cấp frame từ desktop output qua DXGI surface và metadata như dirty/move regions.

Potential strengths:

- GPU-oriented desktop frames
- high-performance continuous capture
- dirty-region metadata
- mature desktop duplication path

Potential concerns:

- monitor/output-oriented abstraction
- window/region cropping phải do CRAI xử lý
- rotation handling
- Direct3D/DXGI resource complexity
- device/output lifecycle
- desktop/mode changes có thể invalidate duplication
- multi-monitor coordination
- greater native implementation complexity

Không chọn DXGI chỉ vì theoretical maximum performance.

## 18. Capture Candidate Comparison

Gate 2 phải đo cùng workload.

| Criterion | Windows.Graphics.Capture | DXGI Desktop Duplication |
| --- | --- | --- |
| Window-oriented capture | Evaluate | Requires CRAI mapping/cropping |
| Display capture | Evaluate | Strong candidate |
| Region capture | Crop/processing likely required | Crop/processing required |
| GPU integration | Evaluate | Strong candidate |
| Dirty/move metadata | Evaluate actual need | Available |
| Native complexity | Expected lower | Expected higher |
| Resize handling | Test | Test/recreate as needed |
| Multi-monitor | Test | Per-output handling |
| Overlay exclusion | Test | Test |
| OCR snapshot use | Test | Test |
| Continuous observation | Test | Test |

Bảng này là test plan, không phải benchmark result.

## 19. Capture Gate

Gate 2 phải prototype cả hai candidates nếu practical.

Dataset/scenarios:

1. static novel page;
2. scrolling novel page;
3. manhua page;
4. moving/resizing browser window;
5. 100% DPI;
6. mixed 125%/150% DPI;
7. source moved across monitors;
8. source minimized/restored;
9. fullscreen content nếu relevant;
10. CRAI Side Panel/Overlay visible.

Metrics:

- capture correctness
- first-frame latency
- repeated-frame latency
- CPU
- GPU
- memory
- frame-copy cost
- resize recovery
- monitor-switch recovery
- cancellation latency
- resource cleanup
- implementation complexity

## 20. Capture Decision Rule

Primary Capture API chỉ được khóa sau Gate 2.

Possible outcome:

```text
Windows.Graphics.Capture
    → primary

DXGI
    → fallback/specialized provider
```

hoặc:

```text
DXGI
    → primary

Windows.Graphics.Capture
    → alternate provider
```

hoặc một candidate có thể bị loại hoàn toàn.

Không cần giữ hai implementations production nếu benchmark không chứng minh giá trị.

## 21. Snapshot vs Continuous Capture

CRAI có hai workload khác nhau:

```text
On-demand Snapshot
Continuous Observation/Capture
```

Technology decision phải test cả hai.

OCR flow không nhất thiết cần capture ở video framerate.

Optimization target:

```text
Capture only as often as Reading/Observation semantics require.
```

Không chạy 60 FPS capture chỉ vì API có thể làm vậy.

## 22. Stability Detection

Stability Detection không thuộc Windows API ownership.

Windows capture layer cung cấp observations/frames.

Architecture/owning module quyết định:

- stable enough
- changed enough
- recapture needed

Platform adapter có thể cung cấp efficient evidence như frame metadata nhưng không quyết định Business semantics.

## 23. Region Selection

Status:

```text
Candidate Implementation
```

Recommended direction:

```text
CRAI-owned region-selection surface
+
Windows screen/window coordinate mapping
```

Region selection cần:

- drag selection
- keyboard cancel
- visible selection boundary
- correct DPI mapping
- multi-monitor correctness
- source-window correlation
- no persistent capture of selector UI

Exact implementation có thể dùng Avalonia transparent window kết hợp Win32 behavior.

Phải prototype trước khi khóa.

## 24. Global Hotkeys

Status:

```text
Candidate Windows Adapter
```

Likely direction:

```text
Win32 global hotkey capability
```

Requirements:

- explicit registration/unregistration
- conflict detection
- stable failure result
- lifecycle cleanup
- configurable combinations
- no leaked native registration after shutdown

Exact binding/library chưa khóa.

Không thêm third-party hotkey package nếu thin native adapter đủ.

## 25. Clipboard

Preferred baseline:

```text
Avalonia clipboard API for normal application usage
```

Windows-specific clipboard adapter chỉ thêm khi cần capability Avalonia không cung cấp.

Clipboard content phải được coi là potentially sensitive.

Không log hoặc persist clipboard content mặc định.

## 26. Side Panel

Side Panel là primary MVP presentation mode.

Preferred implementation:

```text
Avalonia Window
```

Requirements:

- resize
- move
- remember placement nếu Preferences cho phép
- multi-monitor
- DPI
- normal focus/input behavior
- non-blocking updates

Side Panel không cần Overlay-level native complexity.

Đây là lý do Side Panel phải được hoàn thiện trước Overlay.

## 27. Overlay Decision Status

Status:

```text
Candidate Decision
```

Final implementation không được khóa trong file này trước Gate 6.

Overlay feasibility phải kiểm tra:

- transparency
- click-through
- always-on-top
- z-order
- focus behavior
- source-window tracking
- DPI
- coordinate mapping
- multi-monitor
- capture exclusion
- rendering latency
- source resize/move
- hide/show lifecycle
- interaction mode

## 28. Overlay Candidate Architecture

Preferred first prototype:

```text
Avalonia transparent top-level window
    +
Windows-specific style/behavior adapter
```

Nếu không đủ:

```text
Avalonia content/rendering
    +
deeper Win32 window management
```

Native helper chỉ thêm khi .NET/Win32 interop không đáp ứng requirement thực.

Không chuyển toàn application sang WinUI/WPF chỉ vì Overlay cần native flags.

## 29. Click-Through

Click-through là Windows-specific window behavior.

Platform adapter phải cho phép Overlay mode chuyển giữa:

```text
Interactive
Click-through
Hidden
```

Business/Presentation layer yêu cầu mode.

Windows adapter thực hiện native behavior.

Exact Win32 mechanism được khóa sau prototype.

## 30. Always-On-Top and Z-Order

Overlay cần z-order behavior rõ.

Không assume `Topmost=true` một mình đáp ứng mọi source-window scenario.

Gate 6 phải test:

- normal browser
- maximized browser
- fullscreen application nếu supported
- task switch
- multiple CRAI windows
- focus transitions

Z-order workaround không được thêm trước khi test cho thấy cần.

## 31. Capture Exclusion

CRAI phải tránh tự capture Presentation/Overlay khi điều đó làm hỏng OCR input.

Windows cung cấp display-affinity mechanism có thể exclude top-level window của chính process khỏi capture trong supported scenarios.

Candidate:

```text
SetWindowDisplayAffinity
+
WDA_EXCLUDEFROMCAPTURE
```

Nhưng đây là feasibility candidate, không phải absolute security guarantee.

Phải test với selected Capture API.

Không dùng capture exclusion như DRM/security boundary.

## 32. Capture Exclusion Test

Gate 6/Gate 2 integration phải kiểm tra:

```text
Source Window
+
CRAI Side Panel
+
CRAI Overlay
+
Selected Capture Provider
```

Expected:

- source remains capturable;
- excluded CRAI window không contaminate capture khi mechanism được hỗ trợ;
- failure được detect/degrade rõ;
- không silently OCR chính translated overlay.

Nếu exclusion không reliable cho một capture path, architecture phải dùng alternative mitigation như:

- capture source window directly;
- hide Overlay during snapshot;
- region masking;
- temporal capture coordination.

Exact mitigation chỉ chọn sau evidence.

## 33. Overlay Exclusion vs Privacy

`WDA_EXCLUDEFROMCAPTURE` hoặc equivalent không được coi là privacy/security guarantee.

Nó chỉ là capture behavior capability.

Sensitive-content protection vẫn phải dựa trên:

- privacy policy
- logging rules
- persistence rules
- provider routing
- secret management

## 34. Source Window Tracking

Overlay cần correlate với selected source window.

Platform adapter phải detect:

- move
- resize
- minimize
- restore
- close
- monitor change

Overlay placement update phải bounded/coalesced.

Không tạo update storm cho từng raw native event nếu Presentation không cần.

## 35. Focus Model

Side Panel và Overlay có focus requirements khác nhau.

```text
Side Panel
    → normal interactive focus

Click-through Overlay
    → should not steal normal reading interaction

Interactive Overlay
    → may accept input intentionally
```

Windows implementation phải explicit focus policy.

Không để focus behavior là side effect ngẫu nhiên của window styles.

## 36. Window Ownership

CRAI windows có thể cần owner/relationship metadata cho correct lifecycle.

Platform implementation phải tránh:

- orphan overlay
- overlay remaining after source disappears
- hidden native windows leaked
- owner shutdown ordering bugs

Exact HWND ownership arrangement là implementation detail.

## 37. Fullscreen Behavior

Fullscreen source support phải được feasibility-tested.

Không mặc định hứa Overlay hoạt động trên mọi exclusive fullscreen application.

Gate phải phân biệt:

- normal window
- borderless fullscreen
- exclusive fullscreen
- protected content

Unsupported scenario phải có stable capability outcome.

## 38. Protected Content

Windows capture APIs có thể không expose protected content.

CRAI không được cố bypass platform content protection.

Expected behavior:

```text
Protected / unavailable capture
    ↓
Stable Capture Failure
    ↓
Higher-level fallback/degradation
```

Không coi black frame là valid OCR image.

## 39. Secure Desktop

CRAI không hỗ trợ capture secure desktop như login/UAC secure surfaces.

Platform access failure phải được normalize.

Không yêu cầu elevated privilege chỉ để vượt secure-desktop boundary.

## 40. Privilege Model

Baseline:

```text
Run as normal desktop user
```

Không require administrator privilege cho normal CRAI operation.

Nếu một future capability yêu cầu elevation, phải có Technology/Security Decision riêng.

## 41. Notifications

Normal application notifications có thể dùng Avalonia/app UI trước.

Native Windows notifications chỉ thêm khi product requirement cần OS-level notification behavior.

Không block MVP trên Windows notification integration.

## 42. System Tray

System tray là optional desktop UX capability.

Không bắt buộc cho first vertical slice.

Nếu dùng, ưu tiên Avalonia-supported capability trước native implementation.

Tray lifecycle phải integrate cleanly với application shutdown.

## 43. Startup

Auto-start with Windows chưa phải MVP baseline requirement.

Không thêm registry/startup-task behavior trước product decision.

## 44. File Dialogs

Use Avalonia/platform storage dialogs cho normal open/save workflow.

Không viết native Win32 file dialog wrapper nếu framework capability đủ.

## 45. Browser Integration Boundary

CRAI có Text Flow và Image Flow.

Windows Platform không quyết định browser extraction technology.

Nếu future Text Flow cần browser extension/accessibility/automation:

```text
Browser Integration
    → separate Technology Decision
```

Không dùng Screen Capture làm default cho structured text chỉ vì Windows capture đã tồn tại.

## 46. Accessibility APIs

Windows accessibility/UI Automation có thể là future candidate cho structured text acquisition.

Status:

```text
Deferred Candidate
```

Chỉ evaluate khi Text Flow implementation cần lấy structured content từ desktop apps mà browser integration không đủ.

## 47. Input Hooks

Low-level keyboard/mouse hooks không phải default.

Global hotkey nên dùng narrow registration mechanism khi đủ.

Input hooks chỉ thêm nếu concrete feature không thể thực hiện bằng safer/narrower API.

## 48. Native Interop Technology

Preferred direction:

```text
.NET P/Invoke / source-generated interop
```

hoặc maintained Windows bindings nếu chúng giảm risk rõ ràng.

Không thêm large native wrapper dependency chỉ để gọi vài APIs.

Interop layer phải:

- isolate unsafe/native concerns;
- translate HRESULT/Win32 errors;
- own native handle cleanup;
- expose stable .NET types upward.

## 49. COM / WinRT

Nếu Windows.Graphics.Capture hoặc Windows APIs yêu cầu WinRT/COM integration:

- initialization/lifetime phải explicit;
- thread assumptions phải documented;
- native object không crossing Platform boundary;
- disposal phải deterministic khi relevant.

Không để UI code trực tiếp quản lý capture COM objects.

## 50. Direct3D / DXGI Resource Lifecycle

Nếu DXGI hoặc GPU-backed capture được chọn:

Runtime Resource ownership phải bao phủ:

- D3D device
- device context
- duplication object
- frame resource
- staging texture
- mapped resource
- shared handle nếu có

Acquire/Release phải balanced.

Device-lost/mode-change recovery phải explicit.

GC không thay thế native release.

## 51. Capture Buffer Strategy

Status:

```text
Candidate Decision
```

Possible path:

```text
GPU Capture Surface
    ↓
GPU crop/convert when useful
    ↓
CPU-readable image only when OCR runtime requires it
```

Không copy full-screen GPU frame về managed RAM nếu OCR chỉ cần một nhỏ region và API cho phép tránh copy đó.

Nhưng optimization chỉ được implement sau measurement.

## 52. Image Format

Platform capture output phải normalize vào image contract phù hợp với Capture/OCR architecture.

Native format như DXGI `B8G8R8A8_UNORM` không được trở thành public OCR semantic requirement.

Adapter chịu trách nhiệm conversion/metadata.

## 53. Color and Alpha

Capture implementation phải test:

- color correctness
- alpha handling
- transparency artifacts
- browser rendering
- HDR/SDR scenario nếu target hardware gặp

MVP không cần full color-management subsystem nếu OCR quality không bị ảnh hưởng.

## 54. HDR

HDR-specific handling là feasibility item, không phải baseline feature.

Nếu captured OCR quality sai trên HDR displays, cần targeted decision.

Không thiết kế HDR pipeline trước evidence.

## 55. Rotation

DXGI Desktop Duplication có rotation considerations.

Platform adapter phải normalize hoặc expose transform metadata để downstream geometry không sai.

Không silently rotate pixels mà mất source transform.

## 56. Cursor

Cursor inclusion phải configurable hoặc normalized theo Capture requirement.

OCR snapshot thường không cần cursor.

Capture implementation phải xác định:

- cursor included by API;
- separate cursor metadata;
- whether cursor is removed/ignored.

Không để cursor artifact làm OCR input xấu nếu có thể tránh.

## 57. Region Cropping

Region cropping có thể xảy ra:

```text
during capture
```

hoặc:

```text
after capture
```

Technology decision phải ưu tiên correctness trước micro-optimization.

Nếu crop sau capture:

- source offset phải được giữ;
- transform metadata phải preserve;
- OCR geometry phải map ngược được.

## 58. Observation Frequency

Observation/capture frequency phải được policy/runtime điều khiển.

Platform adapter không hard-code:

```text
60 FPS
30 FPS
10 FPS
```

như business requirement.

Nó chỉ expose capability và bounded execution mechanism.

## 59. Cancellation

Capture operation phải support cooperative cancellation ở boundary phù hợp.

Blocking native waits phải dùng bounded timeout hoặc mechanism cho phép Runtime lấy lại control.

Đặc biệt nếu DXGI được dùng, frame acquisition không được block vô hạn.

## 60. Error Mapping

Windows-native errors phải được normalize trước khi crossing Platform boundary.

Ví dụ native conditions:

- unsupported
- access denied
- source closed
- device lost
- session disconnected
- capture unavailable
- display topology changed

phải map thành stable platform/capture outcomes theo authoritative contract.

Không leak HRESULT như Business error contract.

## 61. Capability Detection

Platform adapter phải detect capability thay vì assume.

Ví dụ:

```text
Capture supported?
Capture exclusion supported?
Transparency level available?
Selected source still valid?
```

Unsupported capability phải tạo stable result.

Không crash startup chỉ vì optional capability không available.

## 62. Windows Version Floor

Exact minimum supported Windows version chưa khóa trong file này.

Decision phải dựa trên:

- .NET 10 support
- Avalonia support
- selected Capture API
- capture exclusion requirement
- packaging
- target user population

Không chọn Windows floor chỉ dựa trên một API candidate trước Capture/Packaging decisions.

## 63. User Data Path

PERSISTENCE.md yêu cầu per-user stable writable location.

Windows adapter sẽ resolve platform data location.

Requirements:

```text
Per-user
Writable without elevation
Stable across app updates
Separate from install directory
```

Exact folder naming/path được khóa cùng Packaging/Application Identity decision.

## 64. Temporary Path

Temporary capture/worker data nếu cần phải dùng application-managed temporary location.

Temporary files phải:

- have bounded lifetime;
- be cleaned on normal completion;
- tolerate crash leftovers;
- not contain secrets unnecessarily;
- follow privacy rules.

Không dùng current working directory.

## 65. Secret Protection

Status:

```text
Candidate Decision
```

Windows-first implementation phải ưu tiên OS-backed protection.

Requirements:

- no plaintext API key in SQLite;
- no plaintext secret in normal config;
- per-user protection preferred;
- explicit delete/update;
- stable failure behavior.

Exact API/library được quyết định sau focused feasibility check.

## 66. Logging

Windows adapter có thể log:

- capability
- API selected
- duration
- error category
- monitor/window non-sensitive metadata when safe
- recovery event

Không log:

- window title nếu policy coi sensitive và không cần;
- captured pixels;
- clipboard content;
- OCR content;
- secrets.

Native HRESULT có thể xuất hiện trong diagnostic-only structured field nếu privacy-safe, nhưng public error vẫn normalized.

## 67. Diagnostics

Diagnostics cần capture enough platform evidence để debug:

- Windows version
- monitor count
- DPI/scaling
- selected capture backend
- GPU adapter metadata khi relevant
- capture capability
- transparency capability
- exclusion capability result

Không dump screen content.

## 68. Performance Targets

Exact numeric SLA chưa khóa.

Gate phải thu thập:

- first capture latency
- steady capture latency
- CPU
- GPU
- working set
- allocation
- resource count
- cancellation latency

Target phải dựa trên reading experience, không dựa trên video-capture benchmark.

## 69. Testing Strategy

### Unit Tests

Test:

- coordinate transforms
- DPI conversions
- error mapping
- capability mapping
- source identity mapping

### Windows Integration Tests

Test:

- window enumeration
- window close
- move/resize
- capture
- monitor switch
- DPI change
- clipboard
- hotkey
- capture exclusion where supported

### Manual/Visual Tests

Required for:

- Overlay z-order
- click-through
- transparency
- multi-monitor positioning
- fullscreen behavior
- mixed DPI

### Stress Tests

Test repeated:

- start/stop capture
- source switching
- device/display changes
- overlay show/hide
- cancellation
- resource cleanup

## 70. Gate 1 - Desktop Skeleton

Gate 1 must confirm:

```text
.NET 10
+
Avalonia
+
Windows Platform Adapter
```

can support:

- Side Panel
- basic window discovery
- platform service composition
- per-monitor DPI behavior
- basic transparent top-level window prototype
- native HWND access behind adapter
- clean shutdown

Gate 1 does not select final Overlay implementation.

## 71. Gate 2 - Capture

Gate 2 compares:

```text
Windows.Graphics.Capture
vs
DXGI Desktop Duplication
```

using CRAI workloads.

Output:

```text
Capture Feasibility Report
Primary Capture Decision
Fallback decision if justified
Known limitations
```

## 72. Gate 6 - Overlay

Gate 6 starts only after Gate 1 platform viability and sufficient capture integration.

Prototype must test:

```text
Transparent Window
Click-through
Topmost/Z-order
Source Tracking
DPI
Multi-monitor
Capture Exclusion
Interactive Mode
```

Output:

```text
Overlay Feasibility Result
Final Windows Overlay Strategy
Known unsupported scenarios
```

## 73. Decisions Locked by This Document

Locked baseline:

```text
Initial Platform
    → Windows x64

UI
    → Avalonia

Normal UI Windowing
    → Avalonia

Platform Integration
    → dedicated Windows adapters

Native API Access
    → Win32 / WinRT / DirectX only behind adapter

Capture Candidates
    → Windows.Graphics.Capture + DXGI Desktop Duplication

Side Panel
    → Avalonia Window

DPI Requirement
    → per-monitor aware

Privilege
    → normal user by default

Capture Protected/Secure Content Bypass
    → not supported

Business → Native API direct dependency
    → forbidden
```

## 74. Decisions Still Open

1. primary Capture API;
2. whether secondary Capture backend is worth keeping;
3. exact Window Selection UX;
4. exact window-event vs polling strategy;
5. region-selection implementation;
6. exact global hotkey interop;
7. final Overlay implementation;
8. click-through native mechanism;
9. z-order strategy;
10. capture exclusion strategy after provider testing;
11. GPU→CPU buffer path;
12. HDR handling if required;
13. exact minimum Windows version;
14. exact Windows data path;
15. secret-protection API;
16. system tray requirement;
17. auto-start requirement;
18. Windows notification mechanism;
19. native binding library vs direct interop.

## 75. Relationship to OCR Technology

Windows Platform determines capture output and resource behavior.

OCR technology determines required input representation.

The two decisions must meet at an adapter boundary.

```text
Windows Capture
    ↓
Canonical Image / Artifact Boundary
    ↓
OCR Provider Adapter
```

Do not couple OCR engine directly to DXGI/WinRT capture object.

If an OCR runtime supports GPU input efficiently, a later optimization may introduce a specialized compatible path without changing public OCR contracts.

## 76. Relationship to Packaging

This file does not select Packaging.

Packaging must later account for:

- Windows application identity if required;
- native DLLs;
- capture dependencies;
- secret integration;
- OCR runtime;
- model files;
- optional workers.

OCR runtime remains the largest unresolved packaging dependency.

## 77. Relationship to Persistence

Windows Platform supplies platform-specific data/temporary path resolution.

Persistence owns durable storage behavior.

```text
Windows Platform
    → resolves platform location

Persistence
    → manages SQLite/filesystem data
```

Windows Platform does not own database schema.

## 78. Relationship to Runtime

Windows Platform executes native capability work under Runtime-owned execution/cancellation/resource rules where the capability participates in Runtime work.

Runtime owns:

- execution authority
- scheduling
- cancellation authority
- Runtime Artifact publication

Windows adapters own:

- native invocation
- native resource mapping
- capability result
- platform error normalization

## 79. Relationship to Presentation

Presentation owns what should be shown.

Windows Platform owns how Windows-specific window behavior is implemented.

Example:

```text
Presentation
    → Overlay should be click-through

Windows Adapter
    → applies Windows-specific behavior
```

Presentation does not manipulate HWND directly.

## 80. Feasibility Evidence

Results from Gate 1, Gate 2 and Gate 6 must not be written as assumptions into this file.

Measured evidence belongs in:

```text
04-technology/FEASIBILITY_RESULTS.md
```

After evidence exists, this file may be revised from Candidate Decision to Selected Decision for the relevant technology.

## 81. Next Step

After Windows Platform baseline:

```text
04-technology/OCR_CANDIDATES.md
```

That document must evaluate OCR candidates without assuming that a Python, ONNX, native or remote runtime has already won.

Windows Capture and OCR benchmark work may later be prototyped in parallel because their public boundary is already defined.

## 82. Final Principle

Windows implementation must preserve:

```text
Avalonia owns normal UI.

Windows adapters own platform mechanics.

Business modules never own HWND/DXGI/WinRT.

Capture technology is selected by evidence.

Overlay technology is selected after feasibility.

Native resources have explicit lifecycle.

Windows-first does not mean architecture-locked-to-Windows.
```

The goal is not to use the lowest-level Windows API everywhere.

The goal is to use the smallest Windows-specific surface necessary to deliver reliable Capture, Side Panel and later Overlay behavior without contaminating the rest of CRAI.
