# OCR Pipeline

Status: Draft
Version: 1.2.0
Owner: CRAI Architecture
Layer: OCR Architecture

Related documents:
- PREPROCESS.md
- DETECTION.md
- RECOGNITION.md
- TEXT_DIRECTION.md
- LAYOUT.md
- POSTPROCESS.md
- QUALITY.md
- READING_ORDER.md
- PROVIDERS.md

## 1. Purpose

OCR Pipeline định nghĩa luồng xử lý chuẩn của CRAI để chuyển dữ liệu hình ảnh thành dữ liệu nguồn có cấu trúc, có thể truy vết và sẵn sàng cho các bước xử lý tiếp theo.

Pipeline chịu trách nhiệm mô tả:

- các stage OCR chính
- thứ tự và dependency giữa các stage
- input và output của từng stage
- boundary giữa các tài liệu OCR
- điểm kết thúc của OCR processing
- cách các OCR result và artifact reference được truyền giữa các stage
- boundary giữa OCR semantics và Runtime execution

Pipeline không định nghĩa chi tiết thuật toán của từng stage.

Các quy tắc chuyên biệt thuộc tài liệu owner tương ứng.

## 2. Scope

OCR Pipeline áp dụng cho các nguồn ảnh như:

- manga
- manhua
- manhwa
- browser images
- screen captures
- scanned pages
- imported images
- rasterized document pages
- user-selected image regions

Nguồn ngôn ngữ ban đầu gồm:

- Simplified Chinese
- Traditional Chinese
- English

Contract phải giữ khả năng mở rộng sang các ngôn ngữ khác.

## 3. Non-Goals

OCR Pipeline không chịu trách nhiệm:

- Translation
- semantic text rewriting
- grammar correction
- source acquisition
- UI rendering
- overlay rendering
- persistent storage implementation
- Runtime scheduling
- Runtime retry ownership
- Runtime cancellation ownership
- Runtime execution authority
- Runtime Artifact lifecycle
- Event Bus semantics
- telemetry implementation
- resource lifecycle implementation
- provider credential ownership
- provider routing hoặc fallback ownership

Các concern trên thuộc Runtime, AI, Provider, Infrastructure hoặc Business Module tương ứng.

## 4. Architecture Position

Luồng tổng quát:

Capture hoặc Import
-> Source Image Artifact
-> OCR Pipeline
-> OCR Document
-> Quality Assessment
-> Reading Order
-> Ordered Source Data
-> Text Processing
-> Translation
-> Presentation

OCR Pipeline kết thúc khi CRAI có OCR output hoàn chỉnh theo OCR contracts, provider-neutral và đủ semantic metadata cho downstream processing.

Runtime publication, execution authority và downstream Business-stage progression nằm ngoài OCR Pipeline ownership.

## 5. Canonical OCR Flow

Canonical OCR flow gồm:

1. Request Validation
2. Input Resolution
3. Image Normalization
4. OCR Profile Resolution
5. Reuse Compatibility Evaluation
6. Preprocessing
7. Text Detection
8. Region Preparation
9. Text Recognition
10. Geometry Reconstruction
11. Text Direction Analysis
12. Layout Analysis
13. OCR Postprocessing
14. OCR Document Assembly
15. Quality Assessment
16. Reading Order
17. OCR Result Finalization

Một implementation có thể gộp hoặc tối ưu một số bước khi provider hỗ trợ nhiều capability cùng lúc.

Tuy nhiên contract đầu ra phải giữ nguyên semantics của pipeline chuẩn.

Runtime Artifact Publication không phải một OCR processing stage.

Sau OCR Result Finalization, Runtime có thể publish execution payload thông qua Runtime Artifact boundary nếu execution authority vẫn hợp lệ.

## 6. Stage 1 - Request Validation

Mục tiêu là xác nhận request đủ hợp lệ trước khi sử dụng resource đắt tiền.

Input có thể chứa:

- ExecutionScopeId
- ExecutionRevisionId
- optional Business correlation như ReadingSessionId
- Image Artifact reference
- optional target region
- OCR Profile reference
- language hints
- resolved privacy hoặc policy constraints

Validation kiểm tra các điều kiện OCR-level như:

- image reference hợp lệ
- image version hợp lệ
- target region nằm trong bounds
- OCR mode được hỗ trợ
- OCR Profile hợp lệ
- resolved policy cho phép loại OCR operation được yêu cầu

Output:

Validated OCR Request

Runtime execution authority, cancellation lifecycle và canonical policy resolution không được định nghĩa tại đây.

## 7. Stage 2 - Input Resolution

Mục tiêu là xác định chính xác image artifact và region sẽ được OCR.

Possible inputs:

- full page
- normalized page
- user-selected region
- previously detected region
- retry execution input
- reusable source Artifact reference

Output phải giữ:

- Image ID
- Image Version
- dimensions
- coordinate space
- region bounds
- lineage
- content identity hoặc hash khi contract yêu cầu

Pipeline không được trộn geometry của nhiều image version.

## 8. Stage 3 - Image Normalization

Mục tiêu là đưa input về trạng thái hình ảnh ổn định trước các bước OCR tiếp theo.

Có thể bao gồm:

- orientation normalization
- decode normalization
- color-space normalization
- size constraints
- rotation correction
- perspective correction

Source image phải giữ immutable.

Nếu image thay đổi geometry, transform mapping phải được giữ.

Chi tiết thuộc PREPROCESS.md.

## 9. Stage 4 - OCR Profile Resolution

OCR Profile mô tả semantic và processing policy của OCR request.

Profile có thể ảnh hưởng:

- expected language hoặc script
- preprocessing behavior
- detection behavior
- recognition behavior
- quality thresholds
- execution capability requirements
- OCR-level policy constraints

Profile resolution phải deterministic đối với cùng input và profile configuration.

Output:

Effective OCR Profile

OCR Profile không sở hữu:

- Runtime Retry Policy
- Scheduler policy
- Provider routing hoặc fallback
- Runtime resource admission

## 10. Stage 5 - Reuse Compatibility Evaluation

OCR Architecture có thể xác định liệu một OCR result đã tồn tại có semantically compatible với request hiện tại hay không.

OCR Architecture sở hữu OCR semantic compatibility.

Compatibility có thể phụ thuộc:

- image content identity
- image version khi semantic relevance yêu cầu
- region
- OCR Profile version
- detection behavior version
- recognition behavior version
- preprocessing version
- relevant language hoặc script hint
- output contract version

Nếu semantic inputs thay đổi, result cũ không được xem là equivalent trừ khi owner contract cho phép.

OCR owner nên có khả năng biểu diễn những dependency này thành owner-defined compatibility metadata hoặc semantic dependency fingerprint.

Runtime Cache Policy sở hữu:

- lookup mechanics
- retention
- eviction
- reuse partition
- cache lifecycle

Chi tiết thuộc ../runtime/CACHE_POLICY.md.

OCR Pipeline không tự sở hữu global Cache lifecycle.

## 11. Stage 6 - Preprocessing

Preprocessing chuẩn bị hình ảnh cho Detection và Recognition.

Input:

Resolved Image
+
Effective OCR Profile

Output:

Processed Image
+
Transform Metadata

Chi tiết thuộc PREPROCESS.md.

## 12. Stage 7 - Text Detection

Detection trả lời câu hỏi:

Where is the text?

Input:

Processed Image

Output:

Detection Result

Detection Result chứa các Region cùng geometry và detection-specific metadata.

Pipeline không định nghĩa chi tiết:

- Region Type
- Polygon
- Mask
- Region hierarchy
- merge hoặc split rules

Những nội dung đó thuộc DETECTION.md.

## 13. Stage 8 - Region Preparation

Các Region được chuẩn bị để Recognition xử lý.

Có thể bao gồm:

- crop
- padding
- rectification
- local orientation adjustment
- upscaling
- region-specific preprocessing selection

Mọi derived region phải giữ mapping về coordinate space gốc.

Stage này không thay đổi Region identity semantics.

## 14. Stage 9 - Text Recognition

Recognition trả lời câu hỏi:

What is the text?

Input:

Prepared Region
+
Recognition Policy

Output:

Recognition Result

Recognition có thể tạo:

- Character
- Word
- Line
- Paragraph
- recognized source text
- language hoặc script metadata
- recognition confidence

Provider-native response không được vượt Execution Adapter hoặc OCR Provider Adapter boundary.

Chi tiết thuộc:

- RECOGNITION.md
- PROVIDERS.md

## 15. Stage 10 - Geometry Reconstruction

Mục tiêu là đưa geometry từ provider hoặc prepared region về canonical coordinate space của CRAI.

Conceptual mapping:

Provider Coordinates
-> Prepared Region Coordinates
-> Processed Image Coordinates
-> Canonical Source Coordinates

Pipeline phải giữ:

- exact source image version
- transform lineage
- region identity
- reversible mapping khi đủ dữ liệu

Geometry semantics thuộc Detection hoặc Layout contracts tương ứng.

## 16. Stage 11 - Text Direction Analysis

Text Direction trả lời câu hỏi:

How is the text written?

Input:

Recognition Result
+
Geometry

Output:

Direction Result

Direction có thể gồm:

- Writing Mode
- Line Direction
- Paragraph Direction
- Character Flow
- Rotation
- Direction Confidence

Chi tiết thuộc TEXT_DIRECTION.md.

## 17. Stage 12 - Layout Analysis

Layout trả lời câu hỏi:

How are the visual regions organized?

Input:

Detection Result
+
Recognition Result
+
Direction Result

Output:

Layout Result

Layout Result có thể chứa:

- Layout Tree
- Panel
- Container
- Block
- spatial relationships
- Relationship Graph

Layout không sở hữu final Reading Order.

Chi tiết thuộc LAYOUT.md.

## 18. Stage 13 - OCR Postprocessing

Postprocessing chuẩn hóa và hợp nhất toàn bộ kết quả OCR.

Input:

Detection Result
+
Recognition Result
+
Direction Result
+
Layout Result

Postprocessing chịu trách nhiệm:

- validation
- normalization
- result merging
- consistency checking
- metadata completion

Nó không được tự ý thay đổi:

- recognized source meaning
- Detection geometry semantics
- Layout decisions
- Direction decisions

Chi tiết thuộc POSTPROCESS.md.

## 19. Stage 14 - OCR Document Assembly

Output chính của core OCR processing là OCR Document.

Conceptual structure:

OCR Document
- Metadata
- Page
  - Panels
  - Containers
  - Blocks
  - Regions
- Recognition
- Layout
- Direction
- Statistics
- Diagnostics

OCR Document là provider-neutral.

Authoritative definition hiện thuộc POSTPROCESS.md.

Các tài liệu downstream chỉ tham chiếu model này.

## 20. Stage 15 - Quality Assessment

Quality Assessment trả lời câu hỏi:

How trustworthy is the OCR Document?

Input:

OCR Document

Output:

Quality Report

Quality Report có thể chứa:

- Quality Score
- Quality Grade
- Quality Issues
- Confidence Summary
- Recommendation
- Diagnostics

Quality chỉ đánh giá OCR result.

Quality không tự:

- schedule Retry
- select alternative Provider hoặc ExecutionBinding
- cancel execution
- advance downstream Business Stage
- commit Business Result

Chi tiết thuộc QUALITY.md.

Runtime, Recovery và Business owners consume Quality evidence theo contract tương ứng.

## 21. Stage 16 - Reading Order

Reading Order trả lời câu hỏi:

In what order should the OCR entities be read?

Input chính:

OCR Document
+
Layout Tree
+
Direction Metadata
+
Reading Profile

Output:

Reading Order Result

Reading Order Result có thể chứa:

- Reading Order Graph
- Main Sequence
- Auxiliary Sequence
- Reading Confidence
- Diagnostics

Reading Order không thay đổi Recognition, Geometry, Layout hoặc Direction.

Chi tiết thuộc READING_ORDER.md.

## 22. Stage 17 - OCR Result Finalization

Stage cuối của OCR Pipeline xác nhận rằng OCR outputs đã hoàn chỉnh theo OCR-owned contracts.

Possible finalized outputs:

- OCR Document
- Quality Report
- Reading Order Result

Finalization có thể kiểm tra:

- required OCR structures tồn tại
- output contract version hợp lệ
- semantic references nhất quán
- geometry lineage đầy đủ
- Reading Order tham chiếu đúng OCR entities
- Quality Report tham chiếu đúng OCR Document
- provider-native object không vượt boundary

Stage này không:

- grant Runtime execution authority
- publish Runtime Artifact
- determine whether execution is stale
- commit Business Result
- advance downstream Business Stage

Sau finalization:

Finalized OCR Result Candidate
-> Runtime boundary

Runtime Architecture quyết định execution-authority validation và publication mechanics.

## 23. Runtime Publication Boundary

Runtime publication không phải OCR processing stage.

Conceptually:

Finalized OCR Result Candidate
-> Runtime Control
-> Execution Authority Validation
-> ACCEPT hoặc REJECT
-> Ownership Transfer
-> Runtime Artifact Publication

OCR Pipeline chỉ bảo đảm rằng candidate:

- hoàn chỉnh theo OCR contract
- provider-neutral
- giữ đúng identity và lineage metadata
- đủ information để Runtime correlation và authority validation hoạt động

Runtime sở hữu:

- execution authority
- stale hoặc cancelled rejection
- ownership transfer
- Runtime Artifact publication
- Runtime Artifact lifecycle

## 24. Pipeline Output

OCR Pipeline tạo các output semantic chính.

### OCR Document

Canonical structured OCR artifact hoặc result.

Owner:

POSTPROCESS.md

### Quality Report

Canonical OCR quality evaluation.

Owner:

QUALITY.md

### Reading Order Result

Canonical ordering information cho OCR entities.

Owner:

READING_ORDER.md

Downstream Text Processing có thể dùng OCR Document và Reading Order Result để tạo structured source data.

## 25. Quality as Evaluation

Quality Assessment là một evaluation step, không phải transformation stage.

Conceptually:

OCR Document
-> Quality Assessment
-> Quality Report

OCR Document
-> Reading Order

Quality Report không trực tiếp mutate OCR Document.

Quality Report có thể được Business owner, Runtime Retry evaluation, Routing hoặc Recovery và diagnostics sử dụng như input evidence.

Quality Report không tự thực hiện các quyết định đó.

## 26. Provider Integration

Pipeline không phụ thuộc provider cụ thể.

OCR Pipeline
-> OCR Execution Contract
-> OCR Execution Adapter
-> OCR Engine hoặc Provider Runtime

Provider-specific:

- SDK
- request
- response
- native model structure
- provider error

phải được giữ bên dưới Adapter boundary.

Chi tiết thuộc PROVIDERS.md.

## 27. Execution Binding Boundary

OCR Pipeline có thể mô tả required OCR capabilities nhưng không tự chọn canonical execution route.

Ví dụ capability requirements:

- text detection
- recognition
- vertical-text support
- language hoặc script support
- geometry output
- local-only execution
- GPU capability

Resolved executable implementation được Runtime, AI hoặc Provider architecture biểu diễn thông qua execution binding tương ứng.

## 28. Execution Granularity

OCR Pipeline có thể hoạt động theo:

- Full Page
- Region
- Incremental Scope

Granularity phải explicit trong request và lineage.

Architecture không yêu cầu mọi implementation phải chạy toàn bộ page.

## 29. Parallelizable Work

Một số OCR operations có thể xử lý song song:

- independent region preparation
- independent region recognition
- selected preprocessing candidates
- multiple execution requests khi resolved policy cho phép

Nhưng parallelism phải bounded.

Scheduling và concurrency admission thuộc Runtime.

OCR Pipeline chỉ khai báo rằng work có thể parallelizable và khi cần cung cấp execution hoặc resource requirements.

Nó không định nghĩa Scheduler.

## 30. Cancellation Integration

OCR work phải quan sát Runtime cancellation.

Các operation đắt tiền cần hỗ trợ cooperative cancellation khi implementation cho phép.

OCR không sở hữu:

- cancellation state machine
- cancellation authority
- propagation policy
- grace period
- abandoned semantics

Chi tiết thuộc ../runtime/CANCELLATION.md.

OCR operation chỉ cần cung cấp safe cancellation checkpoints theo implementation contract.

## 31. Retry and Recovery Integration

OCR stages có thể cung cấp evidence như:

- normalized failure category
- Quality information
- capability information
- Retry hint
- Recovery hint
- alternative-execution capability evidence

Nhưng OCR không tự schedule Retry và không tự select alternative execution route.

Ownership:

Same-work Retry
-> Runtime Retry Policy

Alternative execution hoặc Fallback
-> Routing hoặc Recovery owner

New Attempt creation
-> Pipeline Runtime

Scheduler admission
-> Runtime Scheduler

Business pipeline continuation
-> Business Pipeline Orchestration

Chi tiết thuộc:

- ../runtime/RETRY_POLICY.md
- ../ai/ROUTING.md
- ../ai/FALLBACK.md

## 32. Quality-Driven Recovery

Quality Report có thể indicate các condition như:

- quality below recommended threshold
- low recognition confidence
- geometry inconsistency
- direction uncertainty
- reading-order uncertainty

OCR Quality không trực tiếp quyết định:

- Retry
- Provider Switch
- Fallback
- Stop Pipeline
- Continue Pipeline

Những quyết định đó thuộc authoritative owners tương ứng.

## 33. Resource Integration

OCR là workload có thể tiêu thụ nhiều:

- CPU
- GPU
- RAM
- image buffers
- native hoặc model memory
- provider hoặc runtime capacity

OCR Architecture có thể khai báo:

- ExecutionRequirement
- ResourceCostHint
- ExecutionClass
- ExecutionAffinity

hoặc equivalent contract metadata.

Resource lifecycle, Lease, capacity accounting và physical disposal thuộc Runtime.

## 34. Observability Integration

OCR Architecture có thể định nghĩa measurement có semantic value như:

- OCR latency
- preprocessing latency
- detection latency
- recognition latency
- direction-analysis latency
- layout latency
- region count
- quality score
- reuse compatibility outcome
- execution-binding latency

Telemetry lifecycle và logging transport thuộc:

- ../runtime/RUNTIME_OBSERVABILITY.md
- ../../03-infrastructure/logging/
- ../../03-infrastructure/telemetry/

OCR telemetry không được làm Runtime hoặc provider state trở thành OCR-owned state.

## 35. Privacy

OCR input có thể chứa dữ liệu nhạy cảm.

Pipeline phải giữ các nguyên tắc:

- không log raw image mặc định
- không log toàn bộ recognized text mặc định
- execution call phải tuân theo resolved Privacy hoặc Policy constraints
- local-only input không được gửi sang remote execution binding
- Artifact hoặc result sharing phải giữ đúng resolved reuse hoặc privacy partition
- raw provider request hoặc response không đi vào standard telemetry

Privacy hoặc Governance owner quyết định policy.

OCR chỉ tuân theo resolved constraints.

## 36. Stale Result Protection

Một OCR Attempt có thể hoàn thành sau khi execution intent hoặc source đã thay đổi.

Do đó execution output phải giữ đủ Runtime correlation như:

- ExecutionScopeId
- ExecutionRevisionId
- WorkItemId
- AttemptId
- Source Image Version
- Artifact hoặc Candidate Identity

Optional Business correlation có thể gồm:

- ReadingSessionId

OCR Pipeline không tự quyết định output còn current hay stale.

Runtime Control sở hữu execution-authority validation.

Conceptually:

OCR Attempt Completes
-> Completion
-> Runtime Control
-> ACCEPT hoặc REJECT_STALE hoặc REJECT_CANCELLED

## 37. Determinism

Trong cùng:

input semantic identity
+
OCR Profile version
+
strategy hoặc behavior version
+
relevant semantic dependency identity

pipeline nên tạo output có cấu trúc tương đương, ngoại trừ provider hoặc model nondeterminism được ghi nhận rõ.

Determinism đặc biệt quan trọng cho:

- reuse compatibility
- debugging
- quality regression
- reproducibility
- provider comparison

Deterministic pipeline semantics không có nghĩa provider hoặc model output phải byte-identical.

## 38. Immutable Source

Original source image không được mutate.

Derived image phải có:

- identity riêng
- parent reference
- transform metadata
- version hoặc lineage rõ ràng

Transformation không được làm mất khả năng map geometry trở lại canonical source coordinate space khi contract yêu cầu.

## 39. Provider Neutrality

Mọi result vượt OCR Execution Adapter boundary phải dùng CRAI contract.

Không downstream component nào được phụ thuộc trực tiếp:

- PaddleOCR response
- Google Vision response
- Azure Vision response
- Tesseract-native structure
- provider-specific SDK object
- local-model proprietary object

Provider-specific diagnostics chỉ được tồn tại dưới normalized hoặc sanitized contract phù hợp.

## 40. OCR Architecture Invariants

1. Source image remains immutable.
2. Every geometry-bearing artifact references the exact image version used.
3. Derived images preserve lineage and transform metadata.
4. Detection owns Region semantics.
5. Recognition owns recognized source-text structure.
6. Text Direction owns writing-direction semantics.
7. Layout owns spatial-organization semantics.
8. Postprocessing owns canonical OCR Document assembly.
9. Quality Assessment evaluates but does not mutate OCR Document.
10. Reading Order owns precedence and sequence semantics.
11. Provider-native data never crosses OCR Execution Adapter boundary.
12. OCR stages do not own Runtime scheduling.
13. OCR stages do not own Runtime Retry.
14. OCR stages do not own Runtime cancellation authority.
15. OCR stages do not own Runtime execution authority.
16. OCR stages do not redefine Event Bus semantics.
17. OCR stages do not redefine global Cache lifecycle.
18. OCR Result Finalization is distinct from Runtime Artifact Publication.
19. Stale OCR execution output cannot obtain accepted Runtime publication or use authority.
20. Runtime Artifact publication does not imply Business semantic acceptance.
21. Translation remains outside OCR Architecture.
22. OCR output remains source-language information.
23. OCR Quality does not select Retry or Fallback.
24. Alternative execution selection remains Routing or Recovery-owned.
25. ReadingSession is optional Business correlation, not canonical Runtime execution scope.
26. ExecutionScope and ExecutionRevision are Runtime identities, not OCR semantic identities.
27. OCR semantic compatibility is owner-defined.
28. Runtime Cache mechanics do not define OCR semantic equivalence.
29. OCR provider or runtime selection remains outside OCR Pipeline ownership.
30. Detailed concept semantics belong to their authoritative owner documents.

## 41. Recommended MVP Pipeline

Phiên bản đầu tiên của CRAI nên giữ pipeline hẹp và dễ kiểm thử:

Normalized Image
-> Text Detection
-> Region Preparation
-> Recognition
-> Basic Direction Analysis
-> Basic Layout Analysis
-> Postprocessing
-> OCR Document
-> Quality Assessment
-> Rule-Based Reading Order

Initial scope:

- full-page OCR
- manual region OCR
- Simplified Chinese
- Traditional Chinese
- English
- horizontal text
- vertical text where supported
- one primary OCR execution binding
- optional alternative OCR execution binding when Routing hoặc Recovery permits
- region-level processing
- provider-neutral OCR Document
- basic Quality evaluation
- basic Reading Order resolution

Advanced features remain optional until validated by real CRAI reading usage.

## 42. Detailed Ownership References

| Concern | Authoritative Document |
|---|---|
| OCR Pipeline | PIPELINE.md |
| Preprocessing | PREPROCESS.md |
| Region and Detection | DETECTION.md |
| Recognition | RECOGNITION.md |
| Text Direction | TEXT_DIRECTION.md |
| Layout | LAYOUT.md |
| OCR Document | POSTPROCESS.md |
| Quality | QUALITY.md |
| Reading Order | READING_ORDER.md |
| OCR Execution and Provider Contract | PROVIDERS.md |
| Business Pipeline orchestration | ../runtime/BUSINESS_PIPELINE_ORCHESTRATION.md |
| Pipeline Runtime | ../runtime/PIPELINE_RUNTIME.md |
| Retry | ../runtime/RETRY_POLICY.md |
| Cancellation | ../runtime/CANCELLATION.md |
| Scheduling | ../runtime/SCHEDULER.md |
| Cache lifecycle | ../runtime/CACHE_POLICY.md |
| Resource lifecycle | ../runtime/RESOURCE_LIFECYCLE.md |
| Runtime Observability | ../runtime/RUNTIME_OBSERVABILITY.md |
| AI Routing | ../ai/ROUTING.md |
| AI and execution Fallback | ../ai/FALLBACK.md |
| Architectural ownership | ../models/OWNERSHIP_MAP.md |

## 43. Runtime Boundary Summary

OCR owns:

- Image to OCR semantic processing
- OCR Document
- Quality semantics
- Reading Order semantics
- OCR compatibility semantics

Runtime owns:

- ExecutionScope
- ExecutionRevision
- WorkItem
- Attempt
- Scheduling
- Queueing
- Cancellation
- Retry mechanics
- Execution authority
- Runtime Artifact publication
- Resource lifecycle
- Observability mechanics

Routing hoặc Recovery owns:

- alternative execution selection
- Fallback
- execution-binding replacement

Business Pipeline Orchestration owns:

- whether downstream Business work exists
- whether pipeline continues, replans or stops

## 44. Summary

OCR Pipeline là bản đồ end-to-end của quá trình:

Image
-> Preprocessing
-> Detection
-> Recognition
-> Text Direction
-> Layout
-> Postprocessing
-> OCR Document
-> Quality Assessment
-> Reading Order
-> Finalized OCR Result

Sau đó:

Finalized OCR Result
-> Runtime Authority and Publication Boundary
-> Business or Downstream Processing

PIPELINE.md chỉ sở hữu OCR flow và stage boundary.

Mỗi stage chuyên biệt được định nghĩa trong tài liệu owner của nó.

Runtime chịu trách nhiệm execution mechanics, authority, scheduling, resource, Retry, cancellation và Runtime publication.

Routing hoặc Recovery chịu trách nhiệm alternative execution.

Business Pipeline Orchestration quyết định downstream Business progression.

Nguyên tắc quan trọng nhất:

PIPELINE defines how OCR semantic data flows.

Owner documents define what each OCR stage means.

Runtime defines how OCR work executes.

Routing decides where compatible execution runs.

Business orchestration decides what business work happens next.
