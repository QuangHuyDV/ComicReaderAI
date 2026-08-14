# CRAI Feasibility Results

Status: Planned / Evidence Ledger
Version: 0.1.0
Updated: 2026-08-14
Path: 04-technology/FEASIBILITY_RESULTS.md
Depends On:
- 04-technology/TECH_STACK.md
- 04-technology/PERSISTENCE.md
- 04-technology/WINDOWS_PLATFORM.md
- 04-technology/OCR_CANDIDATES.md
- 04-technology/TRANSLATION_CANDIDATES.md

## 1. Purpose

Tài liệu này là canonical evidence ledger cho Technology Feasibility của CRAI.

Nó ghi:

- feasibility gates;
- test plans;
- execution status;
- measured evidence;
- discovered constraints;
- decisions unlocked by evidence;
- rejected alternatives;
- follow-up work.

Tài liệu này không được dùng để ghi kết luận chưa được test.

Initial state:

```text
Gate 1 - Desktop Skeleton
    → NOT RUN

Gate 2 - Capture
    → NOT RUN

Gate 3 - OCR
    → NOT RUN

Gate 4 - Translation
    → NOT RUN

Gate 5 - End-to-End Slice
    → NOT RUN

Gate 6 - Overlay
    → NOT RUN

Gate 7 - Packaging
    → BLOCKED
       by OCR Runtime Decision
```

## 2. Evidence Principle

Technology decision phải theo:

```text
Candidate
    ↓
Prototype / Benchmark
    ↓
Measured Evidence
    ↓
Decision
```

Không theo:

```text
Preference
    ↓
Decision
    ↓
Retroactive justification
```

Nếu chưa có test:

```text
Result
    → UNKNOWN
```

Không dùng `PASS` chỉ vì documentation cho thấy technology theoretically supports capability.

## 3. Status Vocabulary

Gate status chỉ dùng các trạng thái:

```text
NOT RUN
PLANNED
IN PROGRESS
PASSED
PASSED WITH CONDITIONS
FAILED
BLOCKED
SUPERSEDED
```

Meaning:

### NOT RUN

Test chưa bắt đầu.

### PLANNED

Test plan đã đủ rõ để execute.

### IN PROGRESS

Đã có execution/evidence nhưng chưa đủ kết luận.

### PASSED

Gate acceptance criteria đạt.

### PASSED WITH CONDITIONS

Có thể đi tiếp nhưng có known constraints cần ghi rõ.

### FAILED

Candidate/baseline không đáp ứng gate.

### BLOCKED

Không thể chạy do dependency chưa được resolve.

### SUPERSEDED

Evidence cũ đã bị thay thế bởi test mới có version/configuration khác.

## 4. Result Vocabulary

Individual test case dùng:

```text
PASS
FAIL
PARTIAL
NOT RUN
NOT APPLICABLE
BLOCKED
```

Không dùng ambiguous terms như:

- seems fine;
- probably works;
- acceptable maybe;
- looks fast.

## 5. Evidence Classes

Evidence được chia:

```text
E0 - Assumption
E1 - Documentation Evidence
E2 - Prototype Evidence
E3 - Reproducible Benchmark Evidence
E4 - End-to-End Evidence
E5 - Production Evidence
```

### E0

Chưa được dùng để khóa Technology Decision.

### E1

Xác nhận API/capability tồn tại.

Không chứng minh CRAI integration hoạt động.

### E2

Prototype chạy được.

Có thể khóa feasibility direction nhưng chưa đủ cho performance-sensitive decision.

### E3

Benchmark có config/dataset/hardware/version rõ.

Dùng cho OCR/Translation/Capture comparisons.

### E4

Capability chạy đúng trong CRAI vertical slice.

### E5

Evidence từ production/real user workload.

Không có ở Technology Selection ban đầu.

## 6. Evidence Record Format

Mỗi evidence item nên có:

```text
Evidence ID
Date
Gate
Test ID
Evidence Class
Environment
Configuration
Input/Dataset
Procedure
Measured Result
Artifacts
Known Limitations
Conclusion
```

Recommended ID:

```text
EV-G1-001
EV-G2-001
EV-G3-001
...
```

Không overwrite evidence cũ khi test configuration thay đổi đáng kể.

Mark old record:

```text
SUPERSEDED
```

và reference evidence mới.

## 7. Environment Record

Mọi Gate phải ghi environment.

Template:

```text
Environment ID:
Date:
OS:
Windows Build:
CPU:
RAM:
GPU:
GPU Driver:
NPU:
Display Count:
Display Resolution:
Display Scaling:
.NET SDK:
.NET Runtime:
Avalonia Version:
Build Configuration:
Power Mode:
Network:
Notes:
```

Fields không relevant có thể ghi:

```text
N/A
```

## 8. Baseline Documentation Evidence

Current documentation review supports the following baseline assumptions.

### 8.1 .NET 10

Documentation evidence:

```text
.NET 10
    → LTS
    → active supported release
```

This supports continuing Gate 1 with .NET 10.

Evidence class:

```text
E1
```

It does not prove CRAI desktop behavior.

### 8.2 Avalonia on Windows

Documentation evidence supports:

```text
Avalonia
    → Win32-based Windows backend
    → net10.0 target supported
    → per-monitor DPI-aware behavior
    → transparent windows supported
    → HWND available for native interop
```

Evidence class:

```text
E1
```

Important constraint:

```text
Transparent click-through behavior
    → requires native platform interop
```

Therefore final Overlay behavior remains Gate 6 work.

## 9. Gate Dependency Graph

Canonical:

```text
Gate 1
Desktop Skeleton
    |
    +----------------------+
    |                      |
    v                      v
Gate 2                 Gate 3
Capture                OCR
    |                      |
    |                      v
    |                 OCR Runtime
    |                      |
    |                      +-------------------+
    |                                          |
    v                                          v
Gate 5                                     Gate 7
End-to-End                                 Packaging
    ^
    |
Gate 4
Translation

Gate 1 + sufficient Capture integration
    ↓
Gate 6
Overlay
```

More precisely:

```text
Gate 1
    → prerequisite for Gate 2 and Gate 6

Gate 2
    → prerequisite for representative Gate 5

Gate 3
    → prerequisite for Gate 5 and Gate 7

Gate 4
    → prerequisite for Gate 5

Gate 5
    → validates architecture integration

Gate 6
    → may proceed after platform/capture feasibility

Gate 7
    → blocked until OCR runtime is selected
```

## 10. Gate Summary

| Gate | Scope | Status | Blocking Decision |
| --- | --- | --- | --- |
| G1 | Desktop Skeleton | NOT RUN | Desktop baseline validation |
| G2 | Windows Capture | NOT RUN | Primary Capture API |
| G3 | OCR | NOT RUN | OCR engine/runtime |
| G4 | Translation | NOT RUN | Translation provider/model |
| G5 | End-to-End | BLOCKED | Integrated vertical slice |
| G6 | Overlay | BLOCKED | Overlay implementation |
| G7 | Packaging | BLOCKED | Packaging strategy |

No gate currently has measured CRAI evidence.

## 11. Gate 1 - Desktop Skeleton

Status:

```text
NOT RUN
```

Objective:

```text
Verify that:

C#
+
.NET 10
+
Avalonia
+
Windows Platform Adapters

can implement CRAI desktop foundation
without violating architecture boundaries.
```

## 12. Gate 1 Scope

Must prototype:

- application startup;
- dependency injection/composition;
- basic Side Panel;
- Windows platform adapter;
- HWND/native interop access behind adapter;
- per-monitor DPI behavior;
- basic transparent top-level window;
- normal shutdown;
- basic persistence initialization;
- Runtime/Application separation skeleton.

Not required:

- production OCR;
- production Translation;
- final Overlay;
- final Capture backend;
- final Packaging.

## 13. Gate 1 Test Matrix

### G1-T01 - .NET/Avalonia Startup

Expected:

```text
Application starts successfully
on target Windows environment.
```

Status:

```text
NOT RUN
```

### G1-T02 - Side Panel

Expected:

- open;
- resize;
- move;
- minimize/restore;
- close;
- no UI-thread blocking in empty skeleton.

Status:

```text
NOT RUN
```

### G1-T03 - Native Handle Boundary

Expected:

```text
Avalonia Window
    ↓
Platform Adapter
    ↓
HWND access

Business Module
    X
HWND
```

Status:

```text
NOT RUN
```

### G1-T04 - DPI

Test:

- 100%;
- 125%;
- 150%;
- move between mixed-DPI monitors when hardware available.

Expected:

- UI remains correct;
- physical/logical coordinate distinction can be observed and mapped.

Status:

```text
NOT RUN
```

### G1-T05 - Transparent Window

Expected:

- transparent top-level window can render;
- fallback behavior is detectable;
- no claim yet about click-through.

Status:

```text
NOT RUN
```

### G1-T06 - Composition Boundary

Expected:

```text
UI
    ↓
Application Contract
    ↓
Module/Runtime

Windows native implementation
    isolated in platform project/boundary.
```

Status:

```text
NOT RUN
```

### G1-T07 - Persistence Smoke Test

Expected:

- SQLite database opens;
- migration baseline executes;
- Unicode Chinese/Vietnamese round-trip succeeds.

Status:

```text
NOT RUN
```

### G1-T08 - Shutdown

Expected:

- app exits cleanly;
- platform resources disposed;
- no orphan helper/native windows;
- persistence closes safely.

Status:

```text
NOT RUN
```

## 14. Gate 1 Acceptance Criteria

Gate 1 passes if:

```text
All critical G1 tests
    → PASS

and

No architecture-breaking framework constraint discovered.
```

`PASSED WITH CONDITIONS` is allowed if limitations are:

- platform-local;
- containable behind adapter;
- do not require Business Architecture redesign.

Gate fails if CRAI must fundamentally violate locked architecture to use selected desktop stack.

## 15. Gate 1 Result

Current:

```text
Status:
    NOT RUN

Evidence:
    E1 documentation only

Decision:
    NONE

Next Action:
    Build minimal Desktop Skeleton.
```

## 16. Gate 2 - Windows Capture

Status:

```text
BLOCKED
```

Blocked by:

```text
Gate 1 minimum platform skeleton
```

Objective:

```text
Select primary Windows Capture technology
using CRAI workloads.
```

Primary candidates:

```text
Windows.Graphics.Capture

DXGI Desktop Duplication
```

## 17. Gate 2 Required Scenarios

Must test:

1. static novel page;
2. scrolling novel;
3. manhua;
4. selected application window;
5. selected region;
6. window move;
7. window resize;
8. minimize/restore;
9. 100% DPI;
10. mixed DPI;
11. multiple monitors;
12. cancellation;
13. repeated start/stop;
14. CRAI window visible;
15. Overlay exclusion integration when available.

## 18. Gate 2 Metrics

Record:

```text
First Frame Latency
Warm Capture Latency
CPU
GPU
Working Set
Allocation
Copy Cost
Resize Recovery
Monitor Change Recovery
Cancellation Latency
Failure Rate
Resource Stability
Implementation Complexity
```

Do not optimize for maximum FPS unless Observation requirements need it.

## 19. Gate 2 Candidate Record

### G2-C01

```text
Candidate:
    Windows.Graphics.Capture

Status:
    NOT RUN

Evidence:
    NONE
```

### G2-C02

```text
Candidate:
    DXGI Desktop Duplication

Status:
    NOT RUN

Evidence:
    NONE
```

## 20. Gate 2 Acceptance Criteria

Selected primary candidate must:

- capture required source correctly;
- preserve geometry;
- support Runtime cancellation/recovery sufficiently;
- behave correctly under resize/move;
- work with DPI mapping;
- avoid unacceptable resource growth;
- meet interactive reading latency;
- integrate without leaking native semantics upward.

Secondary backend only retained if it solves a measured gap.

## 21. Gate 2 Result

Current:

```text
Status:
    BLOCKED

Primary Capture:
    UNDECIDED

Fallback Capture:
    UNDECIDED

Evidence:
    NONE
```

## 22. Gate 3 - OCR

Status:

```text
NOT RUN
```

Objective:

```text
Select initial OCR engine/model/runtime
for real CRAI Chinese reading content.
```

Gate 3 can begin with benchmark images before Gate 2 is finalized.

Representative Capture output should later be included in final validation.

## 23. Gate 3 Candidate Set

Initial:

```text
PaddleOCR family

RapidOCR family

Direct ONNX / Windows ML compatible path

Windows OCR

Optional remote OCR

Specialized OCR
    only if primary candidates show measurable gap
```

No winner.

## 24. Gate 3 Dataset

Must include:

```text
Simplified Chinese
Traditional Chinese
English
Mixed Chinese/English
Novel
Manhua
Horizontal
Vertical
Rotated
Small Font
Low Contrast
Artwork Background
Speech Bubbles
Browser-rendered Text
```

Dataset must be versioned.

## 25. Gate 3 Quality Metrics

Required:

```text
CER
Character Accuracy
Exact Line Match
Detection Precision/Recall
Missed Region Rate
Duplicate Region Rate
Geometry Quality
Reading-Order Usability
Vertical Text Result
Traditional Chinese Result
```

## 26. Gate 3 Operational Metrics

Required:

```text
Cold Start
Model Load
First Inference
Warm Inference
CPU
Peak Memory
Steady Memory
GPU Memory if applicable
Package/Runtime Footprint
Cancellation Behavior
Repeated-run Stability
```

## 27. Gate 3 Candidate Records

### G3-C01 - PaddleOCR

```text
Exact Version:
    TBD

Model:
    TBD

Runtime:
    TBD

Status:
    NOT RUN
```

### G3-C02 - RapidOCR

```text
Exact Version:
    TBD

Model:
    TBD

Runtime:
    TBD

Status:
    NOT RUN
```

### G3-C03 - Direct ONNX / Windows ML

```text
Model:
    TBD

Runtime:
    TBD

Status:
    NOT RUN
```

### G3-C04 - Windows OCR

```text
API:
    TBD

Language Capability:
    TO VERIFY

Status:
    NOT RUN
```

### G3-C05 - Remote OCR

```text
Provider:
    TBD

Status:
    OPTIONAL / NOT RUN
```

## 28. Gate 3 Decision Rule

First:

```text
Minimum Chinese Quality Threshold
```

Candidates below threshold are rejected regardless of speed.

Then compare survivors:

```text
Quality
Geometry
Latency
Memory
Reliability
Runtime Complexity
Packaging
Offline Capability
License
```

Near-equal quality:

```text
prefer simpler deployment/runtime.
```

## 29. Gate 3 Required Decision Output

Gate 3 must produce:

```text
Initial OCR Provider
Detection Model
Recognition Model
Combined vs Composed Decision
OCR Runtime
In-process vs Worker
CPU Baseline
Optional Acceleration
Model Asset Strategy
Fallback Strategy
Known Weak Cases
```

## 30. Gate 3 Result

Current:

```text
Status:
    NOT RUN

OCR Engine:
    UNDECIDED

OCR Runtime:
    UNDECIDED

Process Topology:
    UNDECIDED

Packaging Dependency:
    BLOCKED
```

## 31. Gate 4 - Translation

Status:

```text
NOT RUN
```

Objective:

```text
Select initial Translation provider/model
using Chinese → Vietnamese CRAI content.
```

## 32. Gate 4 Candidate Classes

Initial:

```text
Dedicated Machine Translation

OpenAI model

Gemini model

Google Translation / Translation LLM path

DeepL
    only if exact language/API eligibility is verified

Local LLM
    optional

Hybrid
    evidence-gated
```

Exact provider/model versions must be pinned before test.

## 33. Gate 4 Dataset

Must include:

- Simplified Chinese;
- Traditional Chinese;
- novel narration;
- dialogue;
- manhua fragments;
- names;
- pronouns/address terms;
- idioms;
- historical/fantasy/xianxia terminology;
- glossary cases;
- long context;
- clean source;
- OCR-noisy source.

## 34. Gate 4 Evaluation Phases

### G4-P1 - Eligibility

Verify:

- language pair;
- API;
- legal/commercial use;
- integration;
- privacy constraints.

### G4-P2 - Clean Quality

Human-evaluate clean Chinese source.

### G4-P3 - Context / Glossary

Test:

- rolling context;
- glossary;
- character consistency;
- terminology.

### G4-P4 - OCR-Noisy Input

Test robustness separately.

### G4-P5 - Operational

Measure:

- latency;
- reliability;
- cost;
- rate-limit behavior;
- cancellation.

## 35. Gate 4 Quality Metrics

Required:

```text
Semantic Faithfulness
Vietnamese Naturalness
Context Correctness
Glossary Adherence
Character Consistency
Terminology Consistency
Formatting Preservation
Hallucination Count
Omission Count
Over-Translation
```

Human review is mandatory for final decision.

## 36. Gate 4 Operational Metrics

Record:

```text
Median Latency
P95 Latency
Time to First Useful Result
Failure Rate
Timeout Rate
Normalized Cost
Context Cost
Batch Behavior
Streaming Behavior if applicable
```

## 37. Gate 4 Candidate Record Template

```text
Candidate ID:

Provider:
Model:
API Mode:
Prompt Version:
Context Strategy:
Glossary Version:
Generation Settings:
Date:

Clean Quality:
Context Quality:
Glossary:
OCR-Noisy:
Latency:
Reliability:
Cost:
Privacy Notes:

Status:
    NOT RUN
```

## 38. Gate 4 Decision Rule

Reject candidate if critical semantic quality fails.

Then compare:

```text
Faithfulness
Natural Vietnamese
Context
Consistency
Glossary
Reliability
Latency
Cost
Privacy
Integration Complexity
```

Fluency cannot compensate for hallucination.

Low cost cannot compensate for semantic failure.

## 39. Gate 4 Required Decision Output

Gate 4 must produce:

```text
Initial Translation Provider
Model/API Mode
Prompt Strategy
Context Strategy
Glossary Mapping
Batch Strategy
Streaming Strategy
Fallback Strategy
Known Weak Cases
```

## 40. Gate 4 Result

Current:

```text
Status:
    NOT RUN

Provider:
    UNDECIDED

Model:
    UNDECIDED

Context:
    UNDECIDED

Fallback:
    UNDECIDED
```

## 41. Gate 5 - End-to-End Slice

Status:

```text
BLOCKED
```

Blocked by:

```text
Representative Capture
+
Selected/Surviving OCR
+
Selected/Surviving Translation
```

Objective:

```text
Verify that selected technologies work
through CRAI architecture,
not only in isolated benchmarks.
```

## 42. Gate 5 Canonical Slice

Must execute:

```text
Reading Source
    ↓
Capture
    ↓
OCR
    ↓
Reading Order
    ↓
Text Processing
    ↓
Translation
    ↓
Presentation
    ↓
Side Panel
```

Runtime path must preserve:

```text
ExecutionScope
ExecutionRevision
WorkItem
Attempt
ExecutionBinding
Cancellation
Accepted Artifact Publication
```

No direct provider bypass.

## 43. Gate 5 Scenarios

At minimum:

### G5-S01 - Novel

```text
Browser novel
    ↓
Capture region
    ↓
Chinese OCR
    ↓
Vietnamese Translation
    ↓
Side Panel
```

### G5-S02 - Manhua

```text
Manhua page
    ↓
Capture
    ↓
Detection/Recognition
    ↓
Reading Order
    ↓
Translation
    ↓
Side Panel
```

### G5-S03 - Rapid Source Change

Source changes while work is in flight.

Expected:

```text
old result
    → stale/rejected

new revision
    → accepted
```

### G5-S04 - Cancellation

User changes/stops reading session.

Expected:

- bounded cancellation;
- no stale publication;
- no resource leak.

### G5-S05 - Provider Failure

OCR or Translation fails.

Expected:

- stable failure;
- UI remains responsive;
- retry/degradation follows policy;
- no corrupted state.

## 44. Gate 5 Metrics

Record:

```text
Time Source Change → Presented Translation
UI Responsiveness
Cancellation Latency
Stale Result Count Accepted
Memory Stability
Failure Recovery
Artifact Lifecycle
Log/Telemetry Correlation
```

Critical:

```text
Accepted stale result
    → 0
```

## 45. Gate 5 Acceptance Criteria

Gate passes if:

- complete vertical slice works;
- architecture boundaries remain intact;
- UI remains responsive;
- stale results are rejected;
- cancellation is bounded;
- resources are released;
- provider failures do not corrupt Runtime state;
- latency is usable for target reading workflow.

## 46. Gate 5 Result

Current:

```text
Status:
    BLOCKED

Evidence:
    NONE

Blocking Gates:
    G2
    G3
    G4
```

## 47. Gate 6 - Overlay

Status:

```text
BLOCKED
```

Blocked by:

```text
Gate 1
+
sufficient Capture integration
```

Objective:

```text
Select actual Windows Overlay implementation
after platform feasibility.
```

## 48. Gate 6 Prototype

Preferred first candidate:

```text
Avalonia transparent top-level window
    +
Windows-specific native behavior adapter
```

Native helper is not introduced unless required by evidence.

## 49. Gate 6 Test Matrix

Must test:

- transparency;
- click-through;
- interactive mode;
- always-on-top;
- z-order;
- source-window move;
- source-window resize;
- source minimize/restore;
- source close;
- 100% DPI;
- mixed DPI;
- multi-monitor;
- focus behavior;
- task switching;
- capture exclusion;
- hide/show;
- repeated lifecycle;
- rendering performance.

## 50. Gate 6 Capture Exclusion

Must test selected Capture backend against CRAI Overlay.

Possible strategies:

```text
WDA_EXCLUDEFROMCAPTURE

Direct source-window capture

Hide-before-snapshot

Region masking

Temporal coordination
```

Do not select mitigation before test.

Capture exclusion is not a security boundary.

## 51. Gate 6 Acceptance Criteria

Overlay must:

- align correctly with source geometry;
- not steal input in click-through mode;
- become interactive when requested;
- handle DPI/multi-monitor;
- track source window;
- avoid contaminating OCR capture by selected mitigation;
- release native resources;
- not require architecture redesign.

Unsupported scenarios must be explicitly documented.

## 52. Gate 6 Result

Current:

```text
Status:
    BLOCKED

Overlay Strategy:
    UNDECIDED

Click-through:
    UNVERIFIED

Capture Exclusion:
    UNVERIFIED
```

## 53. Gate 7 - Packaging

Status:

```text
BLOCKED
```

Primary blocker:

```text
Gate 3
    ↓
OCR Runtime
```

Additional inputs:

- Windows minimum version;
- application identity needs;
- native capture dependencies;
- local Translation runtime if selected;
- model assets;
- worker process;
- secret integration;
- update strategy.

## 54. Gate 7 Candidate Classes

Potential Windows packaging:

```text
MSIX
MSI / WiX
Inno Setup
Self-contained package
Other justified Windows installer
```

No winner.

## 55. Gate 7 Test Requirements

Must test selected realistic dependency set:

- clean install;
- upgrade;
- uninstall;
- per-user data preservation;
- model assets;
- native DLL resolution;
- OCR runtime;
- worker launch if applicable;
- application startup;
- secret storage;
- rollback/failure behavior;
- package size;
- first-run setup;
- offline install if required.

## 56. Gate 7 Acceptance Criteria

Packaging must:

- reproduce working CRAI installation;
- include required runtime/dependencies;
- not store mutable user DB inside install directory;
- preserve user data across normal upgrade;
- cleanly uninstall application-owned binaries;
- handle model/runtime assets predictably;
- support chosen Windows floor;
- not require manual developer environment setup.

## 57. Gate 7 Result

Current:

```text
Status:
    BLOCKED

Reason:
    OCR Runtime not selected

Packaging Format:
    UNDECIDED
```

## 58. Cross-Gate Architecture Checks

Every Gate must verify architecture, not just technology.

Checklist:

```text
[ ] Business ownership unchanged
[ ] Provider remains replaceable
[ ] Platform code remains behind adapter
[ ] Runtime owns execution authority
[ ] Cancellation ownership preserved
[ ] Artifact ownership preserved
[ ] Persistence not conflated with Artifact Store
[ ] Cache not used as source of truth
[ ] Native resources explicitly released
[ ] Secrets remain outside plaintext persistence
[ ] Sensitive content not logged by default
```

Any violation is a Gate issue even if prototype "works".

## 59. Cross-Gate Privacy Checks

For Capture/OCR/Translation:

```text
[ ] Capture only required region/source
[ ] No raw screenshot logging
[ ] No automatic screenshot persistence
[ ] No OCR text logging by default
[ ] No Translation text logging by default
[ ] Remote payload minimized
[ ] Remote provider use follows policy
[ ] Secrets protected
```

## 60. Cross-Gate Performance Checks

Record where relevant:

```text
Cold Start
Warm Latency
P50
P95
CPU
GPU
RAM
VRAM
Allocation
Package Size
Cancellation Latency
```

No universal threshold is invented in advance.

Thresholds must be defined before final candidate scoring for each Gate.

## 61. Cross-Gate Reliability Checks

Test:

- repeated start/stop;
- cancellation;
- source disappearance;
- dependency failure;
- network failure;
- model/runtime unavailable;
- application restart;
- display change;
- provider timeout.

A one-time happy-path demo is not sufficient evidence.

## 62. Cross-Gate Reproducibility

Any benchmark used for final decision must pin:

```text
Code Commit
Configuration
Dependency Version
Model Version
Dataset Version
OS
Hardware
Date
```

Remote provider test additionally pins:

```text
Provider
Model/API Mode
Region when applicable
Prompt Version
Pricing Snapshot
```

## 63. Decision Record Template

When a Gate concludes, append:

```text
Decision ID:
Gate:
Date:

Selected:
Rejected:

Evidence:
- EV-...

Why Selected:

Known Constraints:

Architecture Impact:
    NONE / DOCUMENTED CHANGE REQUIRED

Follow-up:

Re-evaluation Trigger:
```

Do not delete rejected candidate reasoning.

## 64. Re-evaluation Triggers

Technology decision may reopen if:

- selected dependency becomes unsupported;
- model/API is deprecated;
- quality regression appears;
- packaging becomes unacceptable;
- license changes;
- hardware support changes;
- new requirement invalidates old assumption;
- production evidence contradicts benchmark;
- significantly better candidate appears.

Do not reopen decision merely because a newer version exists.

## 65. Failed Gate Handling

If Gate fails:

```text
Failure
    ↓
Identify exact violated requirement
    ↓
Determine scope
```

Then choose:

```text
A. Change implementation candidate

B. Change technology within same architecture

C. Add contained adapter/workaround

D. Escalate architecture conflict
```

Architecture redesign is last option.

A framework inconvenience alone is not sufficient reason to rewrite architecture.

## 66. Architecture Conflict Record

If technology exposes genuine architecture conflict, record:

```text
Conflict ID:
Gate:
Architecture Document:
Existing Rule:
Observed Technical Constraint:
Evidence:
Candidate Alternatives:
Required Decision:
```

Do not silently modify Architecture while implementing spike.

## 67. Known Documentation-Level Constraints

Current E1 review identifies:

### Constraint DC-01

```text
Avalonia transparent window
    ≠
automatic transparent click-through window
```

Native interop is expected for click-through behavior.

Impact:

```text
Gate 6 remains mandatory.
```

### Constraint DC-02

```text
Avalonia uses per-monitor DPI-aware Windows behavior
```

This supports Gate 1 direction but physical capture coordinate mapping still requires CRAI validation.

Impact:

```text
G1 DPI test remains mandatory.
```

### Constraint DC-03

```text
.NET 10 is current LTS baseline.
```

This supports existing TECH_STACK baseline.

No change required.

## 68. Open Risks Before Testing

Current risk register:

### R-F01 - Capture Backend

Risk:

```text
Chosen capture API may have poor fit
for CRAI source-window/region workflow.
```

Mitigation:

```text
Gate 2 comparative prototype.
```

### R-F02 - OCR Quality

Risk:

```text
Fast/easy-to-package OCR may not meet
Chinese/manhua quality threshold.
```

Mitigation:

```text
Gate 3 quality-first benchmark.
```

### R-F03 - OCR Packaging

Risk:

```text
Best OCR may require Python/native runtime
and significantly affect installer complexity.
```

Mitigation:

```text
Do not lock Packaging before Gate 3.
```

### R-F04 - Translation Quality

Risk:

```text
Popular provider may produce weak
Chinese → Vietnamese literary translation.
```

Mitigation:

```text
Gate 4 blind human evaluation.
```

### R-F05 - Overlay

Risk:

```text
Transparency may work while click-through,
DPI, z-order or capture exclusion does not.
```

Mitigation:

```text
Gate 6 dedicated prototype.
```

### R-F06 - End-to-End Latency

Risk:

```text
Each isolated component passes,
but combined reading latency is poor.
```

Mitigation:

```text
Gate 5 vertical slice.
```

## 69. Evidence Directory Direction

Recommended implementation-time layout:

```text
04-technology/
├── FEASIBILITY_RESULTS.md
└── evidence/
    ├── gate-1-desktop/
    ├── gate-2-capture/
    ├── gate-3-ocr/
    ├── gate-4-translation/
    ├── gate-5-e2e/
    ├── gate-6-overlay/
    └── gate-7-packaging/
```

Large benchmark artifacts do not have to live in documentation repository if size/licensing makes that inappropriate.

`FEASIBILITY_RESULTS.md` must still reference their identity/location.

## 70. Benchmark Data Policy

Do not commit:

- secrets;
- provider credentials;
- private screenshots;
- copyrighted corpus without rights;
- raw sensitive reading history.

Benchmark metadata can remain reproducible without exposing private corpus.

## 71. Current Decision State

As of this document version:

```text
FOUNDATION

C#
    → SELECTED BASELINE

.NET 10 LTS
    → SELECTED BASELINE
    → E1 documentation verified

Avalonia
    → SELECTED BASELINE
    → E1 Windows capability verified
    → CRAI prototype NOT RUN

Windows x64
    → SELECTED BASELINE

SQLite
    → SELECTED BASELINE
    → CRAI prototype NOT RUN


EVIDENCE-GATED

Primary Capture
    → UNDECIDED

OCR Engine
    → UNDECIDED

OCR Runtime
    → UNDECIDED

Translation Provider
    → UNDECIDED

Translation Model
    → UNDECIDED

Overlay Implementation
    → UNDECIDED

Packaging
    → BLOCKED
```

## 72. No False Completion Rule

Technology Selection documentation can be complete while feasibility execution is incomplete.

Therefore distinguish:

```text
Technology Selection Plan
    → documented

Technology Feasibility
    → not yet executed

Technology Decisions requiring evidence
    → still open
```

Do not mark `04-technology/` implementation-ready merely because all Markdown files exist.

## 73. When TECH_STACK.md May Be Updated

After a Gate produces a selected decision:

```text
FEASIBILITY_RESULTS.md
    ↓
evidence accepted
    ↓
TECH_STACK.md
    updated from Candidate
    to Selected Baseline/Selected Decision
```

Example:

```text
Before Gate 3:

OCR Engine
    → Candidate Decision

After Gate 3:

OCR Engine
    → <measured winner>
    → Selected Decision
```

Do not update TECH_STACK first and evidence second.

## 74. When Candidate Files May Be Updated

Candidate documents can be updated when:

- candidate eligibility changes;
- provider/model is deprecated;
- benchmark reveals missing evaluation dimension;
- new candidate materially deserves inclusion.

Do not rewrite candidate criteria merely to make selected winner look better.

## 75. BUILD_AND_PACKAGING Readiness

`BUILD_AND_PACKAGING.md` can be drafted before Gate 3 only as:

```text
constraints
candidate strategies
decision dependencies
test plan
```

It cannot lock final package format while:

```text
OCR Runtime
    → UNDECIDED
```

After Gate 3:

```text
OCR Runtime
    ↓
Packaging dependency set
    ↓
Gate 7 execution
    ↓
Final Packaging Decision
```

## 76. TESTING.md Readiness

`TESTING.md` does not need to wait for all Gates.

It can define:

- xUnit baseline;
- architecture tests;
- integration tests;
- Windows platform tests;
- OCR benchmark harness;
- Translation evaluation harness;
- end-to-end tests.

Exact provider fixtures can be filled after Gate 3/4.

## 77. MVP Implementation Plan Readiness

Full MVP implementation plan should distinguish:

```text
Work that can start now
```

from:

```text
Work blocked by feasibility.
```

Can start after Gate 1:

- solution skeleton;
- core contracts;
- Runtime implementation;
- Side Panel;
- Preferences;
- Storage;
- diagnostics;
- benchmark harnesses.

Evidence-gated:

- production Capture backend;
- production OCR provider;
- production Translation provider;
- final Overlay;
- final Packaging.

## 78. Immediate Execution Order

Recommended practical order:

```text
1. Build Gate 1 Desktop Skeleton.

2. In parallel after skeleton:
   - prepare Gate 2 Capture spike;
   - prepare Gate 3 OCR benchmark corpus/harness;
   - prepare Gate 4 Translation benchmark corpus/harness.

3. Execute Gate 2.

4. Execute Gate 3.

5. Execute Gate 4.

6. Build Gate 5 vertical slice.

7. Execute Gate 6 Overlay prototype.

8. Execute Gate 7 Packaging.
```

Gate 3 and Gate 4 dataset preparation does not need to wait for Gate 2.

## 79. Current Next Action

Current actionable next step:

```text
Gate 1
    Desktop Skeleton
```

Before implementation, remaining technology documents may still be completed:

```text
04-technology/BUILD_AND_PACKAGING.md
04-technology/TESTING.md
```

but their unresolved decisions must remain explicitly evidence-gated.

## 80. Final Principle

This document must remain an evidence ledger, not a prediction ledger.

Canonical rule:

```text
Unknown stays UNKNOWN.

Candidate stays CANDIDATE.

Documentation proves capability exists.

Prototype proves CRAI can integrate it.

Benchmark proves comparative behavior.

End-to-End proves the stack works together.

Only evidence locks the decision.
```

Technology Selection is complete only when the project knows both:

```text
what has been selected
```

and:

```text
which selections still require proof.
```
