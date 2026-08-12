# OCR Architecture

Status: Draft
Version: 1.2.0
Layer: OCR Architecture
Entry Point: 01-architecture/ocr/

## 1. Purpose

Thư mục `01-architecture/ocr/` định nghĩa Canonical OCR Architecture của CRAI.

Trong CRAI, OCR không chỉ là character recognition.

OCR là toàn bộ quá trình chuyển dữ liệu hình ảnh thành structured source-language information có geometry, text structure, writing direction, spatial structure, quality evidence và reading precedence phù hợp cho downstream processing.

OCR Architecture độc lập với:

- OCR Engine cụ thể
- AI Provider cụ thể
- Framework
- Runtime implementation
- persistence implementation
- UI implementation

Mục tiêu là để mọi implementation tuân theo cùng semantic contracts và ownership boundaries.

## 2. Scope

OCR Architecture định nghĩa:

- canonical OCR pipeline
- image preprocessing semantics
- text-region detection
- source-text recognition
- text-direction semantics
- spatial layout semantics
- OCR Document assembly
- OCR quality evaluation
- reading-order semantics
- OCR Provider abstraction
- OCR semantic compatibility
- boundary giữa OCR semantics và Runtime execution

OCR Architecture không sở hữu:

- source acquisition
- Translation
- semantic Text Processing
- Presentation hoặc Rendering
- business workflow
- Runtime scheduling
- Runtime Retry Policy
- Runtime cancellation authority
- Runtime execution authority
- Runtime Artifact lifecycle
- global Cache lifecycle
- Event Bus transport
- telemetry transport
- logging implementation
- provider routing hoặc fallback decision

Các concern đó thuộc authoritative architecture hoặc module tương ứng.

## 3. Architecture Position

Canonical high-level flow:

```text
Capture / Import
    ↓
Source Image
    ↓
OCR Architecture
    ↓
OCR Document
    +
Quality Report
    +
Reading Order Result
    ↓
Structured Source Data
    ↓
Text Processing
    ↓
Translation
    ↓
Presentation
```

OCR là boundary giữa image-domain information và structured source-language information.

OCR không dịch nội dung.

## 4. Canonical OCR Pipeline

Luồng chuẩn:

```text
Request Validation
    ↓
Input Resolution
    ↓
Image Normalization
    ↓
OCR Profile Resolution
    ↓
Reuse Compatibility Evaluation
    ↓
Preprocessing
    ↓
Text Detection
    ↓
Region Preparation
    ↓
Text Recognition
    ↓
Geometry Reconstruction
    ↓
Text Direction Analysis
    ↓
Layout Analysis
    ↓
OCR Postprocessing
    ↓
OCR Document
    ├── Quality Assessment
    └── Reading Order
    ↓
OCR Result Finalization
```

`PIPELINE.md` là authoritative owner của flow và stage boundary này.

Runtime Artifact Publication nằm sau OCR Result Finalization và không phải OCR processing stage.

## 5. Documents

Mỗi tài liệu trong thư mục chỉ sở hữu một nhóm semantics rõ ràng.

### README.md

Vai trò:

- entry point của OCR Architecture
- bản đồ tài liệu
- high-level ownership summary
- cross-architecture boundary summary

README không định nghĩa lại chi tiết contract đã có owner riêng.

### PIPELINE.md

Sở hữu:

- Canonical OCR Pipeline
- stage ordering
- stage dependency
- stage input/output boundary
- OCR Result Finalization boundary
- integration boundary với Runtime execution

Không sở hữu thuật toán chi tiết của từng stage.

### PREPROCESS.md

Sở hữu image-preparation semantics trước Detection/Recognition.

Có thể bao gồm:

- decode normalization
- resize
- denoise
- grayscale
- contrast
- thresholding
- orientation normalization
- perspective correction
- transform metadata

Preprocessing không thực hiện Text Detection hoặc Recognition.

### DETECTION.md

Trả lời:

```text
Where is the text?
```

Sở hữu:

- Detection Result
- Region
- Region Type
- Geometry
- Detection Confidence
- Detection semantic compatibility

Detection không nhận dạng source text và không sở hữu Layout Tree hoặc Reading Order.

### RECOGNITION.md

Trả lời:

```text
What is the text?
```

Sở hữu:

- Recognition Result
- Character
- Word
- Line
- Paragraph
- recognized source-language text
- Recognition Confidence
- Recognition semantic compatibility

Recognition không thực hiện Translation và không tự chọn OCR Provider.

### TEXT_DIRECTION.md

Trả lời:

```text
How is the text written?
```

Sở hữu:

- Writing Mode
- Line Direction
- Paragraph Direction
- Character Flow
- Rotation metadata
- Direction Confidence
- Direction semantic compatibility

Text Direction không sở hữu page-level Reading Order.

### LAYOUT.md

Trả lời:

```text
How are visual entities spatially organized?
```

Sở hữu:

- Panel
- Container
- Block
- Layout Tree
- Spatial Relationship Graph
- Layout semantic compatibility

Layout định nghĩa structure.

Layout không định nghĩa final precedence.

### POSTPROCESS.md

Trả lời:

```text
How do compatible OCR stage results become one canonical OCR Document?
```

Sở hữu:

- validation
- normalization
- merge
- consistency checking
- aggregate metadata completion
- OCR Document assembly
- OCR Document semantic identity
- OCR Document lineage
- OCR Document semantic compatibility

Postprocessing không semantic-rewrite recognized text và không tính Reading Order.

### QUALITY.md

Trả lời:

```text
How trustworthy is the OCR result?
```

Sở hữu:

- Quality Report
- Quality Score
- Quality Grade
- Quality Issues
- quality dimensions
- Recommendations

Quality evaluates và recommends.

Quality không trực tiếp quyết định Retry, Fallback hoặc downstream Business continuation.

### READING_ORDER.md

Trả lời:

```text
In what order should OCR entities be read?
```

Sở hữu:

- Reading Order Graph
- Main Sequence
- Auxiliary Sequence
- Reading Confidence
- reading-order conflict semantics
- Reading Order semantic compatibility

Reading Order sử dụng Layout và Text Direction làm evidence nhưng không redefine chúng.

### PROVIDERS.md

Sở hữu OCR Provider abstraction:

- Provider Contract
- Provider Adapter
- Provider Descriptor
- Provider Capabilities
- Provider Health representation
- Provider Registry discovery semantics
- provider-neutral request/result boundary

Provider Layer không tự sở hữu global routing decision.

AI Routing, Provider Management hoặc Recovery quyết định provider eligibility, selection và alternative execution theo policy tương ứng.

Runtime thực thi resolved ExecutionBinding.

## 6. Core Data Flow

Canonical semantic flow:

```text
Source Image
    ↓
Preprocessed Image
    ↓
Detection Result
    ↓
Recognition Result
    ↓
Direction Result
    ↓
Layout Result
    ↓
OCR Document
```

Evaluation và ordering:

```text
OCR Document
    ├── Quality Report
    └── Reading Order Result
```

Các stage có thể được tối ưu hoặc thực thi khác thứ tự vật lý khi implementation cho phép, nhưng canonical semantics và dependency contracts phải được giữ.

## 7. Core Ownership

| Concern | Authoritative Document |
| --- | --- |
| OCR flow | `PIPELINE.md` |
| Image preprocessing | `PREPROCESS.md` |
| Region / Geometry | `DETECTION.md` |
| Recognition text model | `RECOGNITION.md` |
| Writing direction | `TEXT_DIRECTION.md` |
| Spatial layout | `LAYOUT.md` |
| OCR Document | `POSTPROCESS.md` |
| OCR quality | `QUALITY.md` |
| Reading precedence | `READING_ORDER.md` |
| OCR Provider abstraction | `PROVIDERS.md` |

Một semantic concept chỉ có một authoritative owner.

Các tài liệu khác chỉ reference hoặc consume concept đó.

## 8. Important Semantic Boundaries

### Detection vs Recognition

```text
Detection
    → where text exists

Recognition
    → what the text is
```

### Text Direction vs Reading Order

```text
Text Direction
    → local writing flow

Reading Order
    → precedence across readable entities
```

### Layout vs Reading Order

```text
Layout
    → spatial structure

Reading Order
    → reading precedence
```

### Postprocessing vs Quality

```text
Postprocessing
    → structural validity and canonical assembly

Quality
    → trustworthiness and usability evaluation
```

### Quality vs Decision

```text
Quality
    → evaluates and recommends

Authoritative policy owner
    → decides action
```

### OCR Document vs Runtime Artifact

```text
OCR Document
    → OCR semantic object

Runtime Artifact
    → Runtime publication/lifecycle object
```

Hai identity này có thể correlate nhưng không được coi là cùng một ownership concept.

## 9. Relationship with Runtime

OCR Architecture định nghĩa semantic work.

Runtime định nghĩa execution mechanics và execution authority.

Runtime sở hữu:

- ExecutionScope
- ExecutionRevision
- WorkItem
- Attempt
- Scheduler admission
- execution state
- cancellation execution
- same-work Retry mechanics
- execution authority
- stale-result rejection
- Runtime Artifact publication
- Runtime resource coordination

OCR stage không được redefine các concept trên.

Conceptually:

```text
OCR semantic work
    ↓
Resolved execution plan / binding
    ↓
Runtime WorkItem
    ↓
Attempt
    ↓
Completion
    ↓
Runtime Control
    ↓
ACCEPT / REJECT_STALE / REJECT_CANCELLED
    ↓
Runtime Artifact Publication
```

## 10. Retry, Recovery and Routing Boundary

Không tồn tại một generic `Runtime Decision` sở hữu mọi action.

Ownership được tách theo concern.

```text
Same-work Retry
    → Runtime Retry Policy

Provider eligibility / selection
    → AI Routing / Provider Management

Alternative execution / Fallback
    → AI Routing / Recovery

Continue / Stop downstream Business flow
    → Business Pipeline Orchestration

Execution mechanics
    → Runtime
```

OCR stages chỉ cung cấp semantic evidence, hints hoặc recommendations khi contract cho phép.

## 11. Relationship with Business Pipeline

OCR Architecture không quyết định khi nào business workflow cần OCR.

Business Pipeline Orchestration sở hữu:

- downstream Business work có tồn tại hay không
- stage dependency ở business level
- Continue / Stop / Replan business flow
- consumption của finalized OCR outputs

Pipeline Runtime materialize executable work từ resolved plan.

Runtime Scheduler chỉ admission dependency-ready work.

## 12. Relationship with Provider Architecture

OCR Provider Layer cung cấp:

- capabilities
- provider descriptors
- adapter contracts
- normalized errors
- health representation
- provider-neutral request/result mapping

Provider-specific SDK hoặc native object không được vượt Adapter boundary.

Conceptually:

```text
OCR Semantic Requirement
    ↓
Routing / Provider Management
    ↓
Resolved ExecutionBinding
    ↓
Runtime
    ↓
Provider Contract
    ↓
Provider Adapter
    ↓
Provider Runtime / OCR Engine
```

OCR stage không tự gọi arbitrary Provider SDK và không tự switch Provider.

## 13. Relationship with Cache

OCR Architecture sở hữu semantic compatibility của OCR-owned result.

Ví dụ:

- Detection compatibility
- Recognition compatibility
- Direction compatibility
- Layout compatibility
- OCR Document compatibility
- Reading Order compatibility

Runtime Cache Policy sở hữu:

- lookup mechanics
- cache storage
- retention
- eviction
- cleanup
- reuse lifecycle

Cache hit không được redefine semantic equivalence.

## 14. Relationship with Infrastructure

Infrastructure cung cấp transport hoặc physical mechanisms như:

- Logging
- Telemetry
- Event Bus
- configuration infrastructure
- physical resource integration

OCR Architecture chỉ sử dụng public contracts.

OCR không định nghĩa lại transport semantics.

Runtime Architecture vẫn sở hữu Runtime-specific scheduling, execution control và resource lifecycle dù implementation có thể sử dụng Infrastructure components bên dưới.

## 15. Error Boundary

Mỗi OCR stage sở hữu semantic meaning của stage-specific errors.

Ví dụ:

```text
Detection semantic failure
Recognition semantic failure
Direction semantic failure
Layout semantic failure
Postprocessing semantic failure
```

Khi crossing Runtime execution boundary:

```text
OCR semantic failure
    ↓
Runtime Error Model
```

Runtime Error Model normalize execution-level representation nhưng không redefine OCR semantics.

## 16. Event Boundary

OCR stage có thể định nghĩa domain facts có semantic meaning trong phạm vi owner của nó.

Event Bus sở hữu:

- envelope
- transport
- delivery
- subscription mechanics
- routing mechanics

OCR Architecture không định nghĩa lại Event Bus.

## 17. Observability Boundary

OCR có thể định nghĩa measurements có semantic value như:

- stage latency
- Region count
- recognition confidence
- Direction Confidence
- quality score
- invalid-reference count
- OCR Document size

Runtime Observability sở hữu execution correlation.

Infrastructure sở hữu telemetry/logging transport và lifecycle.

Raw image, full recognized text hoặc full OCR Document không được log mặc định.

## 18. Privacy Boundary

OCR input và OCR Document có thể chứa sensitive source content.

OCR execution phải tuân resolved Privacy/Policy constraints.

Các nguyên tắc:

- local-only content không được gửi Remote Provider
- raw image không được log mặc định
- full recognized text không được log mặc định
- provider metadata phải được sanitize
- Runtime Artifact publication không được làm mất privacy constraints
- persistence phải tuân policy của authoritative owner

OCR không tự định nghĩa global Privacy Governance.

## 19. Immutability and Lineage

Canonical OCR results phải giữ immutable publication semantics.

Nếu upstream semantic input thay đổi:

```text
Old Result
    ↓
New Upstream Revision
    ↓
New Result Revision
```

Không silent-mutate published result cũ.

Lineage phải đủ để xác định:

- source image
- source image version
- upstream result revisions
- profile/strategy version khi relevant
- semantic compatibility dependencies

Runtime execution identity không thay thế semantic OCR lineage.

## 20. Provider Neutrality

Downstream OCR consumers không được phụ thuộc trực tiếp vào:

- Tesseract-native model
- PaddleOCR-native model
- EasyOCR-native model
- Google Vision response
- Azure Vision response
- Gemini-native response
- provider SDK object

Mọi output crossing OCR Provider Adapter boundary phải dùng CRAI contract hoặc normalized representation.

## 21. Design Principles

OCR Architecture tuân theo:

- Provider Independent
- Semantic Ownership
- Explicit Stage Boundary
- Provider-Neutral Contracts
- Immutable Published Results
- Explicit Lineage
- Deterministic Semantics
- Capability-Based Execution
- Runtime Separation
- Observable
- Replaceable
- Testable

Không stage nào được phụ thuộc trực tiếp vào implementation nội bộ của stage khác.

## 22. Architecture Invariants

1. OCR chỉ tạo source-language information; Translation nằm ngoài OCR.

2. Detection sở hữu Region và Geometry semantics.

3. Recognition sở hữu recognized source-text structure.

4. Text Direction sở hữu local writing-direction semantics.

5. Layout sở hữu spatial organization.

6. Postprocessing sở hữu canonical OCR Document assembly.

7. Quality sở hữu OCR quality evaluation và recommendation.

8. Reading Order sở hữu reading precedence.

9. Provider-native data không crossing Provider Adapter boundary.

10. OCR stages không sở hữu Runtime scheduling.

11. OCR stages không sở hữu Runtime Retry Policy.

12. OCR stages không sở hữu Runtime cancellation authority.

13. OCR stages không sở hữu Runtime execution authority.

14. OCR stages không sở hữu Runtime Artifact publication.

15. OCR stages không sở hữu global Cache lifecycle.

16. OCR stages không sở hữu provider routing hoặc fallback decision.

17. OCR stages không quyết định downstream Business continuation.

18. Runtime execution identity không thay thế OCR semantic identity.

19. Runtime Cache mechanics không redefine OCR semantic compatibility.

20. Quality recommendation không phải command.

21. Layout structure không phải Reading Order.

22. Text Direction không phải Reading Order.

23. OCR Document không phải Runtime Artifact.

24. Published OCR semantic results phải immutable.

25. Semantic owner document là authoritative source của concept tương ứng.

## 23. Recommended Reading Order

Nên đọc theo dependency và semantic flow:

```text
README.md
    ↓
PIPELINE.md
    ↓
PREPROCESS.md
    ↓
DETECTION.md
    ↓
RECOGNITION.md
    ↓
TEXT_DIRECTION.md
    ↓
LAYOUT.md
    ↓
POSTPROCESS.md
    ↓
QUALITY.md
    ↓
READING_ORDER.md
    ↓
PROVIDERS.md
```

Lý do:

- `PIPELINE.md` cho biết toàn bộ flow.
- `PREPROCESS.md` chuẩn bị image input.
- `DETECTION.md` định nghĩa Region/Geometry.
- `RECOGNITION.md` gắn source text vào Region.
- `TEXT_DIRECTION.md` định nghĩa local writing flow.
- `LAYOUT.md` tổ chức visual structure.
- `POSTPROCESS.md` assemble OCR Document.
- `QUALITY.md` đánh giá document.
- `READING_ORDER.md` xác định precedence.
- `PROVIDERS.md` mô tả execution abstraction bên dưới OCR stages.

`QUALITY.md` và `READING_ORDER.md` đều consume OCR Document và không bắt buộc phải phụ thuộc tuần tự vào nhau trong mọi execution plan.

## 24. Architecture Boundary

OCR Architecture kết thúc tại finalized OCR semantic outputs:

```text
OCR Document
+
Quality Report
+
Reading Order Result
```

Sau đó:

```text
Finalized OCR Result
    ↓
Runtime Authority / Publication Boundary
    ↓
Structured Source Data
    ↓
Text Processing
    ↓
Translation
    ↓
Presentation
```

OCR không sở hữu downstream semantic transformation.

## 25. Out of Scope

OCR Architecture không chịu trách nhiệm cho:

- Translation
- semantic Text Processing
- Rendering
- Overlay
- Browser Automation
- User Interaction
- Runtime Scheduler
- Runtime State Machine
- Runtime Retry Policy
- Runtime cancellation authority
- Runtime resource lifecycle
- Runtime Artifact lifecycle
- Provider routing/fallback policy
- Business Pipeline Orchestration
- Event routing/transport
- Logging transport
- Telemetry transport

## 26. Summary

`01-architecture/ocr/` là canonical architecture boundary cho quá trình:

```text
Image
    ↓
OCR semantic processing
    ↓
OCR Document
    +
Quality Report
    +
Reading Order Result
```

Mỗi document có một owner rõ ràng.

OCR Architecture định nghĩa semantic meaning và compatibility.

Runtime định nghĩa execution mechanics và execution authority.

Routing/Provider Management/Recovery định nghĩa execution choice.

Business Pipeline Orchestration định nghĩa downstream Business progression.

Infrastructure cung cấp transport và physical mechanisms.

Nguyên tắc cốt lõi:

```text
OCR defines what OCR data means.

Owner documents define each OCR semantic concept.

Routing resolves compatible execution choices.

Runtime executes and controls execution authority.

Business orchestration decides what business work happens next.
```
