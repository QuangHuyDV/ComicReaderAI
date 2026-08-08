# OCR Pipeline

> **Status:** Draft
> **Version:** 1.1.0
> **Owner:** CRAI Architecture
> **Layer:** OCR Architecture
> **Related:** `PREPROCESS.md`, `DETECTION.md`, `RECOGNITION.md`, `TEXT_DIRECTION.md`, `LAYOUT.md`, `POSTPROCESS.md`, `QUALITY.md`, `READING_ORDER.md`, `PROVIDERS.md`

---

# 1. Purpose

OCR Pipeline định nghĩa luồng xử lý chuẩn của CRAI để chuyển dữ liệu hình ảnh thành dữ liệu nguồn có cấu trúc, có thể truy vết và sẵn sàng cho các bước xử lý tiếp theo.

Pipeline chịu trách nhiệm mô tả:

* các stage OCR chính
* thứ tự và dependency giữa các stage
* input/output của từng stage
* boundary giữa các tài liệu OCR
* điểm kết thúc của OCR processing
* cách các artifact OCR được truyền giữa các stage

Pipeline không định nghĩa chi tiết thuật toán của từng stage.

Các quy tắc chuyên biệt thuộc tài liệu owner tương ứng.

---

# 2. Scope

OCR Pipeline áp dụng cho các nguồn ảnh như:

* manga
* manhua
* manhwa
* browser images
* screen captures
* scanned pages
* imported images
* rasterized document pages
* user-selected image regions

Nguồn ngôn ngữ ban đầu gồm:

* Simplified Chinese
* Traditional Chinese
* English

Contract phải giữ khả năng mở rộng sang các ngôn ngữ khác.

---

# 3. Non-Goals

OCR Pipeline không chịu trách nhiệm:

* Translation
* semantic text rewriting
* grammar correction
* source acquisition
* UI rendering
* overlay rendering
* persistent storage implementation
* Runtime scheduling
* Runtime retry ownership
* Runtime cancellation ownership
* Event Bus semantics
* telemetry implementation
* resource lifecycle implementation
* provider credential ownership

Các concern trên thuộc Runtime, Infrastructure hoặc Business Module tương ứng.

---

# 4. Architecture Position

```text
Capture / Import
      │
      ▼
Source Image Artifact
      │
      ▼
OCR Pipeline
      │
      ▼
OCR Document
      │
      ├──► Quality Assessment
      │
      └──► Reading Order
               │
               ▼
        Ordered Source Data
               │
               ▼
        Text Processing
               │
               ▼
          Translation
               │
               ▼
         Presentation
```

OCR Pipeline kết thúc khi CRAI có một `OCR Document` hợp lệ và provider-neutral.

---

# 5. Canonical OCR Flow

```text
Input Image
    │
    ▼
1. Request Validation
    │
    ▼
2. Input Resolution
    │
    ▼
3. Image Normalization
    │
    ▼
4. OCR Profile Resolution
    │
    ▼
5. Cache Compatibility Check
    │
    ├── reusable result ───────────────┐
    │                                  │
    ▼                                  │
6. Preprocessing                       │
    │                                  │
    ▼                                  │
7. Text Detection                      │
    │                                  │
    ▼                                  │
8. Region Preparation                  │
    │                                  │
    ▼                                  │
9. Text Recognition                    │
    │                                  │
    ▼                                  │
10. Geometry Reconstruction            │
    │                                  │
    ▼                                  │
11. Text Direction Analysis            │
    │                                  │
    ▼                                  │
12. Layout Analysis                    │
    │                                  │
    ▼                                  │
13. OCR Postprocessing                 │
    │                                  │
    ▼                                  │
14. OCR Document Assembly ◄────────────┘
    │
    ▼
15. Quality Assessment
    │
    ▼
16. Reading Order
    │
    ▼
17. Result Publication
```

Một implementation có thể gộp hoặc tối ưu một số bước khi provider hỗ trợ nhiều capability cùng lúc.

Tuy nhiên contract đầu ra phải giữ nguyên semantics của pipeline chuẩn.

---

# 6. Stage 1 — Request Validation

Mục tiêu:

xác nhận request đủ hợp lệ trước khi sử dụng resource đắt tiền.

Input có thể chứa:

* Session identity
* Revision identity
* Image Artifact reference
* optional target region
* OCR Profile reference
* language hints
* privacy classification

Validation kiểm tra:

* image tồn tại
* image version hợp lệ
* target region nằm trong bounds
* OCR mode được hỗ trợ
* profile hợp lệ
* privacy policy cho phép xử lý yêu cầu

Output:

```text
Validated OCR Request
```

Runtime state hoặc cancellation lifecycle không được định nghĩa tại đây.

---

# 7. Stage 2 — Input Resolution

Mục tiêu:

xác định chính xác image artifact và region sẽ được OCR.

Possible inputs:

* full page
* normalized page
* user-selected region
* previously detected region
* retry region

Output phải giữ:

* Image ID
* Image Version
* dimensions
* coordinate space
* region bounds
* lineage
* content hash

Pipeline không được trộn geometry của nhiều image version.

---

# 8. Stage 3 — Image Normalization

Mục tiêu:

đưa input về trạng thái hình ảnh ổn định trước các bước OCR tiếp theo.

Có thể bao gồm:

* orientation normalization
* decode normalization
* color-space normalization
* size constraints
* rotation correction
* perspective correction

Source image phải giữ immutable.

Nếu image thay đổi geometry, transform mapping phải được giữ.

Chi tiết thuộc:

```text
PREPROCESS.md
```

---

# 9. Stage 4 — OCR Profile Resolution

OCR Profile mô tả policy xử lý OCR của request.

Profile có thể ảnh hưởng:

* expected language/script
* preprocessing behavior
* detection behavior
* recognition behavior
* quality thresholds
* provider capability requirements
* privacy constraints

Profile resolution phải deterministic.

Output:

```text
Effective OCR Profile
```

Runtime retry policy không thuộc OCR Profile ownership.

---

# 10. Stage 5 — Cache Compatibility Check

Pipeline có thể kiểm tra xem kết quả OCR tương thích đã tồn tại hay chưa.

OCR Architecture chỉ sở hữu **semantic compatibility**.

Compatibility có thể phụ thuộc:

* image content hash
* image version
* region
* OCR profile version
* detection behavior version
* recognition behavior version
* preprocessing version
* relevant language/script hint

Nếu semantic inputs thay đổi, result cũ không được xem là tương đương.

Global cache lifecycle, eviction và storage policy thuộc Runtime.

Chi tiết:

```text
runtime/CACHE_POLICY.md
```

---

# 11. Stage 6 — Preprocessing

Preprocessing chuẩn bị hình ảnh cho Detection và Recognition.

Input:

```text
Resolved Image
+
Effective OCR Profile
```

Output:

```text
Processed Image
+
Transform Metadata
```

Chi tiết thuộc:

```text
PREPROCESS.md
```

---

# 12. Stage 7 — Text Detection

Detection trả lời:

```text
Where is the text?
```

Input:

```text
Processed Image
```

Output:

```text
Detection Result
```

Detection Result chứa các Region cùng geometry và detection-specific metadata.

Pipeline không định nghĩa chi tiết:

* Region Type
* Polygon
* Mask
* Region hierarchy
* merge/split rules

Những nội dung đó thuộc:

```text
DETECTION.md
```

---

# 13. Stage 8 — Region Preparation

Các Region được chuẩn bị để Recognition xử lý.

Có thể bao gồm:

* crop
* padding
* rectification
* local orientation adjustment
* upscaling
* region-specific preprocessing selection

Mọi derived region phải giữ mapping về coordinate space gốc.

Stage này không thay đổi Region identity semantics.

---

# 14. Stage 9 — Text Recognition

Recognition trả lời:

```text
What is the text?
```

Input:

```text
Prepared Region
+
Recognition Policy
```

Output:

```text
Recognition Result
```

Recognition có thể tạo:

* Character
* Word
* Line
* Paragraph
* recognized text
* language/script metadata
* recognition confidence

Provider-native response không được vượt Provider Adapter boundary.

Chi tiết thuộc:

```text
RECOGNITION.md
PROVIDERS.md
```

---

# 15. Stage 10 — Geometry Reconstruction

Mục tiêu:

đưa geometry từ provider hoặc prepared region về canonical coordinate space của CRAI.

Conceptual mapping:

```text
Provider Coordinates
        ↓
Prepared Region Coordinates
        ↓
Processed Image Coordinates
        ↓
Canonical Source Coordinates
```

Pipeline phải giữ:

* exact source image version
* transform lineage
* region identity
* reversible mapping khi đủ dữ liệu

Geometry semantics thuộc Detection/Layout contracts tương ứng.

---

# 16. Stage 11 — Text Direction Analysis

Text Direction trả lời:

```text
How is the text written?
```

Input:

```text
Recognition Result
+
Geometry
```

Output:

```text
Direction Result
```

Direction có thể gồm:

* Writing Mode
* Line Direction
* Paragraph Direction
* Character Flow
* Rotation
* Direction Confidence

Chi tiết thuộc:

```text
TEXT_DIRECTION.md
```

---

# 17. Stage 12 — Layout Analysis

Layout trả lời:

```text
How are the visual regions organized?
```

Input:

```text
Detection Result
+
Recognition Result
+
Direction Result
```

Output:

```text
Layout Result
```

Layout Result có thể chứa:

* Layout Tree
* Panel
* Container
* Block
* spatial relationships
* Relationship Graph

Layout không sở hữu final Reading Order.

Chi tiết thuộc:

```text
LAYOUT.md
```

---

# 18. Stage 13 — OCR Postprocessing

Postprocessing chuẩn hóa và hợp nhất toàn bộ kết quả OCR.

Input:

```text
Detection Result
+
Recognition Result
+
Direction Result
+
Layout Result
```

Postprocessing chịu trách nhiệm:

* validation
* normalization
* result merging
* consistency checking
* metadata completion

Nó không được thay đổi:

* recognized source meaning
* Detection geometry
* Layout decisions
* Direction decisions

Chi tiết thuộc:

```text
POSTPROCESS.md
```

---

# 19. Stage 14 — OCR Document Assembly

Output chính của OCR Pipeline là:

```text
OCR Document
```

Conceptual structure:

```text
OCR Document
├── Metadata
├── Page
│   ├── Panels
│   ├── Containers
│   ├── Blocks
│   └── Regions
├── Recognition
├── Layout
├── Direction
├── Statistics
└── Diagnostics
```

`OCR Document` là provider-neutral.

Authoritative definition hiện thuộc:

```text
POSTPROCESS.md
```

Các tài liệu downstream chỉ tham chiếu model này.

---

# 20. Stage 15 — Quality Assessment

Quality Assessment trả lời:

```text
How trustworthy is the OCR Document?
```

Input:

```text
OCR Document
```

Output:

```text
Quality Report
```

Quality Report có thể chứa:

* Quality Score
* Quality Grade
* Quality Issues
* Confidence Summary
* Recommendation
* Diagnostics

Quality chỉ đánh giá.

Quality không tự:

* retry
* switch provider
* cancel
* continue pipeline

Runtime mới sở hữu các quyết định execution này.

Chi tiết thuộc:

```text
QUALITY.md
```

---

# 21. Stage 16 — Reading Order

Reading Order trả lời:

```text
In what order should the OCR entities be read?
```

Input chính:

```text
OCR Document
+
Layout Tree
+
Direction Metadata
+
Reading Profile
```

Output:

```text
Reading Order Result
```

có thể chứa:

* Reading Order Graph
* Main Sequence
* Auxiliary Sequence
* Reading Confidence
* Diagnostics

Reading Order không thay đổi Recognition, Geometry, Layout hoặc Direction.

Chi tiết thuộc:

```text
READING_ORDER.md
```

---

# 22. Stage 17 — Result Publication

Sau khi OCR output đạt boundary hợp lệ, Runtime có thể publish artifact tương ứng.

Pipeline publication chỉ yêu cầu rằng:

* artifact đã hoàn chỉnh theo contract
* identity và revision hợp lệ
* stale result không được commit
* downstream consumer nhận reference tới artifact hợp lệ

Artifact lifecycle, authority validation và publication semantics thuộc Runtime Architecture.

OCR Pipeline không tự quyết định downstream execution.

---

# 23. Pipeline Output

OCR Pipeline tạo hai output quan trọng.

## OCR Document

Canonical structured OCR artifact.

Owner:

```text
POSTPROCESS.md
```

---

## Reading Order Result

Canonical ordering information cho OCR entities.

Owner:

```text
READING_ORDER.md
```

Các bước sau có thể dùng hai artifact này để tạo structured source data cho Text Processing.

---

# 24. Quality as Evaluation

Quality Assessment là một evaluation step, không phải transformation stage.

Conceptually:

```text
OCR Document
      │
      ├──► Quality Assessment
      │        ↓
      │   Quality Report
      │
      └──► Reading Order
```

Quality Report hỗ trợ Runtime Decision nhưng không trực tiếp mutate OCR Document.

---

# 25. Provider Integration

Pipeline không phụ thuộc provider cụ thể.

```text
OCR Pipeline
      │
      ▼
Provider Contract
      │
      ▼
Provider Adapter
      │
      ▼
OCR Engine
```

Provider-specific SDK, request và response phải được giữ bên dưới Adapter boundary.

Chi tiết thuộc:

```text
PROVIDERS.md
```

---

# 26. Execution Granularity

OCR Pipeline có thể hoạt động theo:

* Full Page
* Region
* Incremental Scope

Granularity phải explicit trong request và lineage.

Architecture không yêu cầu mọi implementation phải chạy toàn bộ page.

---

# 27. Parallelizable Work

Một số stage có thể xử lý song song:

* independent region preparation
* independent region recognition
* selected preprocessing candidates
* provider requests khi policy cho phép

Nhưng parallelism phải bounded.

Scheduling và concurrency authority thuộc Runtime.

Pipeline chỉ khai báo:

```text
this work MAY be parallelizable
```

không định nghĩa execution scheduler.

---

# 28. Cancellation Integration

OCR work phải quan sát Runtime cancellation.

Các operation đắt tiền cần hỗ trợ cooperative cancellation khi implementation cho phép.

OCR không sở hữu:

* cancellation state machine
* cancellation authority
* propagation policy

Chi tiết thuộc:

```text
runtime/CANCELLATION.md
```

---

# 29. Retry and Fallback Integration

OCR stages có thể cung cấp:

* failure category
* quality information
* provider capability information
* retry recommendation
* fallback recommendation

Nhưng không tự schedule retry.

Runtime quyết định:

```text
Retry
Fallback
Stop
Continue
```

Chi tiết thuộc:

```text
runtime/RETRY_POLICY.md
```

---

# 30. Resource Integration

OCR là workload có thể tiêu thụ nhiều:

* CPU
* GPU
* RAM
* image buffers
* provider capacity

OCR Architecture có thể khai báo resource requirement.

Resource lifecycle, lease và capacity management thuộc Runtime/Infrastructure.

---

# 31. Observability Integration

OCR Architecture có thể định nghĩa các measurement có ý nghĩa như:

* OCR latency
* detection latency
* recognition latency
* region count
* quality score
* cache compatibility outcome
* provider latency

Telemetry lifecycle và logging transport thuộc:

```text
runtime/RUNTIME_OBSERVABILITY.md
03-infrastructure/logging/
03-infrastructure/telemetry/
```

---

# 32. Privacy

OCR input có thể chứa dữ liệu nhạy cảm.

Pipeline phải giữ các nguyên tắc:

* không log raw image mặc định
* không log toàn bộ recognized text mặc định
* provider call phải tuân theo privacy policy
* local-only input không được gửi sang remote provider
* artifact sharing phải giữ đúng session/privacy boundary

Chi tiết implementation thuộc Infrastructure và Provider Integration.

---

# 33. Stale Result Protection

Một OCR execution có thể hoàn thành sau khi source đã thay đổi.

Do đó mọi output phải giữ identity đủ để Runtime kiểm tra:

```text
Session
Revision
Image Version
Artifact Identity
```

OCR Pipeline không tự commit stale result.

Runtime authority quyết định output còn hợp lệ hay không.

---

# 34. Determinism

Trong cùng:

```text
input semantic identity
+
profile version
+
strategy version
```

pipeline phải tạo output có cấu trúc tương đương, ngoại trừ provider nondeterminism được ghi nhận rõ.

Determinism đặc biệt quan trọng cho:

* cache compatibility
* debugging
* quality regression
* reproducibility

---

# 35. Immutable Source

Original source image không được mutate.

Derived image phải có:

* identity riêng
* parent reference
* transform metadata
* version/lineage rõ ràng

---

# 36. Provider Neutrality

Mọi result vượt Provider Adapter boundary phải dùng CRAI contract.

Không downstream component nào được phụ thuộc:

* PaddleOCR response
* Google Vision response
* Azure Vision response
* Tesseract-native structure
* provider-specific SDK object

---

# 37. OCR Architecture Invariants

1. Source image remains immutable.

2. Every geometry-bearing artifact references the exact image version used.

3. Derived images preserve lineage and transform metadata.

4. Detection owns Region semantics.

5. Recognition owns recognized text structure.

6. Text Direction owns writing-direction semantics.

7. Layout owns spatial organization semantics.

8. Postprocessing owns canonical OCR Document assembly.

9. Quality Assessment evaluates but does not mutate OCR Document.

10. Reading Order owns precedence and sequence semantics.

11. Provider-native data never crosses Provider Adapter boundary.

12. OCR stages do not own Runtime scheduling.

13. OCR stages do not own Runtime retry.

14. OCR stages do not own Runtime cancellation authority.

15. OCR stages do not redefine Event Bus semantics.

16. OCR stages do not redefine global cache lifecycle.

17. Stale OCR output cannot obtain presentation authority.

18. Translation remains outside OCR Architecture.

19. OCR output remains source-language information.

20. Detailed concept semantics belong to their authoritative owner documents.

---

# 38. Recommended MVP Pipeline

Phiên bản đầu tiên của CRAI nên giữ pipeline hẹp và dễ kiểm thử:

```text
Normalized Image
      ↓
Text Detection
      ↓
Region Preparation
      ↓
Recognition
      ↓
Basic Direction Analysis
      ↓
Basic Layout Analysis
      ↓
Postprocessing
      ↓
OCR Document
      ↓
Quality Assessment
      ↓
Rule-Based Reading Order
```

Initial scope:

* full-page OCR
* manual region OCR
* Simplified Chinese
* Traditional Chinese
* English
* horizontal text
* vertical text where supported
* one primary OCR provider
* optional fallback provider
* region-level processing
* provider-neutral OCR Document
* basic quality evaluation
* basic reading-order resolution

Advanced features remain optional until validated by real reading sessions.

---

# 39. Detailed Ownership References

| Concern                 | Authoritative Document             |
| ----------------------- | ---------------------------------- |
| OCR Pipeline            | `PIPELINE.md`                      |
| Preprocessing           | `PREPROCESS.md`                    |
| Region / Detection      | `DETECTION.md`                     |
| Recognition             | `RECOGNITION.md`                   |
| Text Direction          | `TEXT_DIRECTION.md`                |
| Layout                  | `LAYOUT.md`                        |
| OCR Document            | `POSTPROCESS.md`                   |
| Quality                 | `QUALITY.md`                       |
| Reading Order           | `READING_ORDER.md`                 |
| OCR Provider Contract   | `PROVIDERS.md`                     |
| Retry                   | `runtime/RETRY_POLICY.md`          |
| Cancellation            | `runtime/CANCELLATION.md`          |
| Scheduling              | `runtime/SCHEDULER.md`             |
| Cache lifecycle         | `runtime/CACHE_POLICY.md`          |
| Resource lifecycle      | `runtime/RESOURCE_LIFECYCLE.md`    |
| Runtime Observability   | `runtime/RUNTIME_OBSERVABILITY.md` |
| Architectural ownership | `architecture/OWNERSHIP_MAP.md`    |

---

# 40. Summary

OCR Pipeline là bản đồ end-to-end của quá trình:

```text
Image
   ↓
Preprocessing
   ↓
Detection
   ↓
Recognition
   ↓
Text Direction
   ↓
Layout
   ↓
Postprocessing
   ↓
OCR Document
   ↓
Quality Assessment
   ↓
Reading Order
   ↓
Structured Source Data
```

`PIPELINE.md` chỉ sở hữu flow và stage boundary.

Mỗi stage chuyên biệt được định nghĩa trong tài liệu owner của nó.

Runtime và Infrastructure chịu trách nhiệm execution, resource, retry, cancellation, telemetry và technical services.

Nguyên tắc quan trọng nhất:

```text
PIPELINE defines where data flows.

Owner documents define what each stage means.

Runtime defines how work executes.
```
