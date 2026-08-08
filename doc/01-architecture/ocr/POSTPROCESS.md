# OCR Postprocessing

> **Status:** Draft
> **Version:** 1.1
> **Layer:** OCR Architecture
> **Depends On:** Detection, Recognition, Text Direction, Layout Analysis
> **Next Layer:** Quality Assessment, Reading Order

---

# 1. Purpose

OCR Postprocessing là giai đoạn chuẩn hóa và hợp nhất các kết quả OCR thành một representation thống nhất.

Nếu:

```text id="p1"
Detection
    → "Text nằm ở đâu?"

Recognition
    → "Text là gì?"

Text Direction
    → "Text được viết theo hướng nào?"

Layout
    → "Các visual entity được tổ chức như thế nào?"
```

thì Postprocessing trả lời:

```text id="p2"
"Làm thế nào để các kết quả OCR trở thành
một OCR Document nhất quán?"
```

Postprocessing không thực hiện OCR mới.

Nó chỉ:

* validate
* normalize
* merge
* complete metadata
* assemble canonical OCR Document

---

# 2. Scope

Postprocessing chịu trách nhiệm:

* Result Validation
* Data Normalization
* Result Merging
* Reference Validation
* Consistency Checking
* Metadata Completion
* OCR Document Assembly
* postprocessing diagnostics
* postprocessing statistics

Postprocessing không chịu trách nhiệm:

* Text Detection
* Character Recognition
* Translation
* semantic rewriting
* Grammar Correction
* Reading Order
* Layout Analysis
* Text Direction Analysis
* Rendering
* Runtime scheduling
* Runtime retry
* cancellation authority
* Event Bus behavior
* global cache lifecycle

---

# 3. Goals

Postprocessing hướng tới:

* canonical data representation
* provider independence
* structural consistency
* explicit references
* serializable output
* immutable published result
* deterministic assembly
* downstream usability
* traceability

---

# 4. Non-Goals

Postprocessing không:

* sửa text Recognition dựa trên ngữ nghĩa
* đoán lại Region
* thay đổi Geometry
* thay đổi Layout Tree
* thay đổi Text Direction
* tự quyết định Retry
* tự quyết định Provider fallback
* tự xác định Reading Order
* dịch source text

---

# 5. Architecture Position

```text id="p3"
Detection Result
       +
Recognition Result
       +
Direction Result
       +
Layout Result
       │
       ▼
OCR Postprocessing
       │
       ▼
OCR Document
       │
       ├──► Quality Assessment
       │
       └──► Reading Order
```

Postprocessing là bước hợp nhất các output riêng lẻ của OCR Architecture.

---

# 6. Terminology

## OCR Document

Canonical provider-neutral representation của kết quả OCR.

---

## Normalization

Đưa dữ liệu từ nhiều source/provider/stage về cùng contract.

---

## Validation

Kiểm tra dữ liệu có hợp lệ theo contract hay không.

---

## Merge

Liên kết nhiều result khác nhau vào cùng một OCR Document.

---

## Consistency

Đảm bảo các entity và reference không mâu thuẫn.

---

## Metadata Completion

Bổ sung metadata còn thiếu ở mức aggregate mà không thay đổi semantic output của upstream stage.

---

## Validation Report

Báo cáo các lỗi hoặc warning được phát hiện trong Postprocessing.

---

# 7. Core Inputs

Postprocessing nhận:

```text id="p4"
Detection Result
+
Recognition Result
+
Direction Result
+
Layout Result
+
Effective OCR Profile
```

Các result phải tham chiếu cùng semantic source:

* Image identity
* Image version
* Session/Revision khi contract yêu cầu
* compatible OCR execution context

---

# 8. Core Output

Output chính:

```text id="p5"
OCR Document
```

Có thể kèm:

```text id="p6"
Validation Report
Processing Statistics
Diagnostics
```

`OCR Document` là output canonical cho downstream OCR consumers.

---

# 9. High-Level Postprocessing Flow

```text id="p7"
Upstream OCR Results
        │
        ▼
1. Input Validation
        │
        ▼
2. Identity / Reference Validation
        │
        ▼
3. Data Normalization
        │
        ▼
4. Result Merge
        │
        ▼
5. Consistency Check
        │
        ▼
6. Metadata Completion
        │
        ▼
7. OCR Document Assembly
        │
        ▼
8. Output Validation
```

---

# 10. Stage 1 — Input Validation

Postprocessing kiểm tra:

* required result tồn tại
* contract version được hỗ trợ
* result structure hợp lệ
* source identity nhất quán
* referenced entity tồn tại
* required metadata có mặt

Postprocessing không tự chạy lại upstream stage để sửa lỗi.

---

# 11. Stage 2 — Identity and Reference Validation

Postprocessing phải kiểm tra lineage giữa các stage.

Ví dụ:

```text id="p8"
Recognition Region Reference
        ↓
must reference
        ↓
Detection Region
```

Tương tự:

```text id="p9"
Direction Line Reference
        ↓
Recognition Line

Layout Region Reference
        ↓
Detection Region
```

Không được tạo reference mồ côi.

---

# 12. Identity Invariants

Entity identity phải giữ xuyên suốt pipeline.

Ví dụ:

```text id="p10"
Detection Region ID
        ↓
Recognition Region Reference
        ↓
Layout Region Reference
        ↓
OCR Document Region Reference
```

Postprocessing không tự đổi ID chỉ để thuận tiện cho serialization.

---

# 13. Stage 3 — Data Normalization

Normalization có thể áp dụng cho:

* ID representation
* Geometry representation
* Confidence representation
* Language Code
* Script Code
* Direction enum
* Writing Mode
* Timestamp format
* Metadata structure
* Provider metadata envelope

Normalization không thay đổi meaning.

---

# 14. Provider Normalization Boundary

Provider-native data phải được normalize trước hoặc trong Postprocessing boundary.

Downstream không được nhìn thấy:

* provider SDK object
* provider-specific enum
* provider-specific coordinate type
* provider-specific confidence schema

Public OCR Document phải dùng CRAI contract.

---

# 15. Stage 4 — Result Merge

Postprocessing hợp nhất:

```text id="p11"
Detection
    → Region / Geometry

Recognition
    → Text Structure

Text Direction
    → Writing Direction

Layout
    → Spatial Structure
```

thành một OCR Document thống nhất.

Merge chỉ liên kết và tổ chức dữ liệu.

Không reinterpret upstream semantics.

---

# 16. Merge Relationships

Ví dụ mapping:

```text id="p12"
Region
  └── Recognition
       ├── Paragraph
       ├── Line
       ├── Word
       └── Character
```

và:

```text id="p13"
Page
  └── Layout
       ├── Panel
       ├── Container
       ├── Block
       └── Region Reference
```

Direction metadata liên kết vào Region/Paragraph/Line tương ứng.

---

# 17. Merge Rules

Merge phải đảm bảo:

* không mất entity
* không duplicate identity
* không overwrite upstream semantic fields
* reference luôn map về entity hợp lệ
* optional data có thể thiếu mà không phá toàn document nếu contract cho phép

---

# 18. Stage 5 — Consistency Check

Sau merge, hệ thống kiểm tra consistency.

Ví dụ:

* Region trong Recognition tồn tại trong Detection
* Layout reference trỏ đúng Region
* Character thuộc Word hợp lệ
* Word thuộc Line hợp lệ
* Line thuộc Paragraph hợp lệ
* Paragraph thuộc Region hợp lệ
* Direction reference hợp lệ
* Geometry không nằm ngoài declared coordinate space
* parent-child graph không lỗi

---

# 19. Structural Consistency

Textual hierarchy:

```text id="p14"
Region
  └── Paragraph
       └── Line
            └── Word
                 └── Character
```

Visual hierarchy:

```text id="p15"
Page
  └── Panel
       └── Container
            └── Block
                 └── Region
```

Hai hierarchy liên kết qua `Region`.

Postprocessing không được gộp chúng thành một hierarchy duy nhất làm mất semantics.

---

# 20. Inconsistency Handling

Nếu phát hiện bất nhất:

* record issue
* classify severity
* preserve original result
* reject only when contract không thể đảm bảo

Không tự sửa khi không đủ evidence.

Ví dụ:

```text id="p16"
Paragraph references missing Region
```

không được tự gán sang Region gần nhất chỉ dựa vào geometry.

---

# 21. Recoverable vs Invalid

Một vấn đề có thể là:

```text id="p17"
Warning
```

nếu OCR Document vẫn usable.

Hoặc:

```text id="p18"
Invalid
```

nếu canonical contract không thể tạo.

Postprocessing chỉ phân loại semantic validity.

Runtime sở hữu execution response.

---

# 22. Stage 6 — Metadata Completion

Postprocessing có thể bổ sung metadata aggregate như:

* OCR Contract Version
* Pipeline Version
* Processing Profile Version
* creation timestamp
* source identity
* language summary
* script summary
* confidence summary
* provider summary
* stage result references

Metadata completion không thay đổi upstream semantic result.

---

# 23. Provider Metadata

Provider Metadata có thể được giữ cho:

* diagnostics
* reproducibility
* compatibility
* benchmark correlation

Nhưng phải:

* optional
* encapsulated
* không trở thành dependency bắt buộc của downstream

---

# 24. Stage 7 — OCR Document Assembly

Postprocessing tạo đúng một canonical `OCR Document` cho một compatible result set.

Conceptual structure:

```text id="p19"
OCR Document
├── Metadata
├── Source
├── Page
│   ├── Panels
│   ├── Containers
│   ├── Blocks
│   └── Regions
├── Recognition
│   ├── Paragraphs
│   ├── Lines
│   ├── Words
│   └── Characters
├── Direction
├── Layout
├── Statistics
└── Diagnostics
```

Exact serialization schema có thể được định nghĩa riêng khi implementation bắt đầu.

---

# 25. OCR Document Ownership

`POSTPROCESS.md` là authoritative owner hiện tại của:

```text id="p20"
OCR Document
```

Các tài liệu:

* Quality
* Reading Order
* Text Processing
* Translation boundary
* Presentation
* Storage

chỉ consume hoặc reference OCR Document.

Không được định nghĩa lại một model khác có cùng ý nghĩa.

---

# 26. OCR Document Identity

OCR Document phải có identity riêng.

Có thể gồm:

* Document ID
* Revision
* Source Image ID
* Source Image Version
* Pipeline Version
* Profile Version

Identity exact form sẽ phụ thuộc Runtime/Artifact contract.

---

# 27. OCR Document Immutability

Published OCR Document phải immutable.

Nếu một upstream stage chạy lại:

```text id="p21"
new Detection Result
```

hoặc:

```text id="p22"
new Recognition Result
```

thì Postprocessing phải tạo:

```text id="p23"
new OCR Document revision
```

không silent-mutate document cũ.

---

# 28. OCR Document Lineage

OCR Document nên giữ references tới upstream result revisions.

Ví dụ:

```text id="p24"
OCR Document
├── Detection Result Ref
├── Recognition Result Ref
├── Direction Result Ref
└── Layout Result Ref
```

Lineage giúp:

* debugging
* compatibility
* reproducibility
* stale-result detection

---

# 29. Stage 8 — Output Validation

Trước khi publish, OCR Document phải được validate lần cuối.

Checks tối thiểu:

* required entities tồn tại
* required references hợp lệ
* source identity nhất quán
* hierarchy hợp lệ
* no duplicate identity
* provider-native structure không leak
* serialization-safe
* contract version hợp lệ

---

# 30. Validation Report

Validation Report có thể chứa:

```text id="p25"
Validation Report
├── Issues[]
│   ├── Code
│   ├── Severity
│   ├── Entity Reference
│   └── Message
├── Warning Count
├── Error Count
└── Valid
```

Validation Report không thay thế OCR Document.

---

# 31. Diagnostics

Diagnostics có thể chứa:

* normalization warning
* missing optional metadata
* merge conflict
* orphan reference
* provider normalization warning
* structural inconsistency
* fallback information từ upstream metadata

Không nên chứa Runtime retry state.

---

# 32. Statistics

Postprocessing có thể tạo statistics như:

* Region count
* Paragraph count
* Line count
* Word count
* Character count
* invalid reference count
* normalization issue count
* processing duration
* confidence summary

Statistics có thể được export sang Telemetry.

Telemetry ownership thuộc Infrastructure.

---

# 33. Confidence Summary

Postprocessing có thể tổng hợp confidence metadata để downstream dễ truy cập.

Nhưng nó không redefine confidence semantics.

Owners vẫn là:

```text id="p26"
Detection Confidence
    → DETECTION.md

Recognition Confidence
    → RECOGNITION.md

Direction Confidence
    → TEXT_DIRECTION.md

Layout Confidence
    → LAYOUT.md
```

Quality mới đánh giá confidence ở mức toàn document.

---

# 34. Postprocessing vs Quality

Boundary bắt buộc:

```text id="p27"
Postprocessing
    → "Is the OCR data structurally consistent?"

Quality
    → "How good/trustworthy is this OCR Document?"
```

Postprocessing validate contract.

Quality đánh giá usability/quality.

---

# 35. Postprocessing vs Reading Order

Postprocessing không tạo precedence.

Nó chỉ tạo structured OCR Document.

```text id="p28"
Postprocessing
    ↓
OCR Document
    ↓
Reading Order
    ↓
Reading Sequence
```

Reading Order mới sở hữu thứ tự đọc.

---

# 36. Postprocessing vs Text Processing

Postprocessing chỉ normalize OCR-specific structure.

Nó không thực hiện:

* semantic source normalization
* sentence reconstruction
* translation segmentation
* glossary preparation

Những phần này thuộc Text Processing.

---

# 37. Text Preservation

Recognized source text phải được giữ nguyên meaning.

Postprocessing không tự:

* sửa spelling
* sửa grammar
* đổi punctuation theo semantic preference
* replace term
* translate content

OCR/provider artifact cleanup chỉ được thực hiện nếu semantics đã được owner document định nghĩa rõ.

---

# 38. Geometry Preservation

Postprocessing không thay đổi canonical Detection Geometry.

Nếu normalization cần đổi representation:

```text id="p29"
Provider Polygon
    ↓
CRAI Polygon
```

thì visual meaning phải giữ nguyên.

---

# 39. Layout Preservation

Postprocessing không reorder hoặc restructure Layout Tree để phục vụ Reading Order.

Layout Result được merge theo semantics đã được `LAYOUT.md` định nghĩa.

---

# 40. Direction Preservation

Postprocessing không sửa:

* Writing Mode
* Line Direction
* Paragraph Direction
* Character Flow

nếu không có explicit upstream revision.

---

# 41. Serialization

OCR Document phải hỗ trợ:

* serialization
* deserialization
* versioning
* persistence
* transport where allowed

Serialization không được:

* thay đổi ID
* mất lineage
* mất metadata bắt buộc
* đổi semantic enum

---

# 42. Schema Versioning

OCR Document phải có Contract Version.

Breaking changes như:

* đổi field meaning
* đổi hierarchy semantics
* đổi required reference
* đổi identity semantics

phải tăng version.

---

# 43. Compatibility

OCR Document compatibility có thể phụ thuộc:

* source identity
* upstream result revisions
* Pipeline Version
* OCR Profile Version
* contract version

Postprocessing chỉ định nghĩa semantic compatibility.

Runtime/Storage quyết định lifecycle cụ thể.

---

# 44. Provider Independence

OCR Document phải provider-neutral.

Downstream không cần biết:

* PaddleOCR
* Tesseract
* Google Vision
* Azure Vision
* Custom Model

đã tạo dữ liệu gốc như thế nào.

---

# 45. Multi-Provider Assembly

Một OCR Document có thể được tạo từ nhiều Provider.

Ví dụ:

```text id="p30"
Detection
    → Provider A

Recognition
    → Provider B

Layout Hint
    → Provider C
```

Sau normalize + merge, downstream vẫn chỉ thấy:

```text id="p31"
CRAI OCR Document
```

---

# 46. Runtime Integration

Postprocessing không sở hữu:

* queue state
* WorkItem state
* Attempt
* retry decision
* cancellation authority
* stale-result authority
* Scheduler behavior

Runtime sở hữu execution.

---

# 47. Retry Integration

Postprocessing có thể báo:

* invalid upstream result
* consistency failure
* missing required data
* structural corruption

Quality/Runtime có thể dùng các signal này.

Postprocessing không tự retry upstream stage.

---

# 48. Cache Integration

Postprocessing có thể xác định semantic incompatibility khi:

* upstream result revision thay đổi
* OCR Profile thay đổi
* Pipeline Version thay đổi
* Postprocessing Strategy Version thay đổi

Global cache:

* storage
* retention
* eviction
* cleanup

thuộc Runtime.

---

# 49. Event Integration

Postprocessing có thể tạo domain facts như:

```text id="p32"
OCRDocumentAssembled
OCRDocumentInvalid
PostprocessingCompleted
PostprocessingFailed
```

Ý nghĩa semantic thuộc Postprocessing.

Transport/envelope thuộc Event Bus.

---

# 50. Error Integration

Postprocessing-specific semantic errors có thể gồm:

```text id="p33"
MissingDetectionResult
MissingRecognitionResult
InvalidRegionReference
InvalidTextHierarchy
InvalidLayoutReference
InvalidDirectionReference
MergeConflict
OCRDocumentInvalid
UnsupportedContractVersion
```

Các lỗi phải map về Runtime Error Model khi crossing execution boundary.

---

# 51. Observability Integration

Useful measurements:

* Postprocessing duration
* merge issue count
* invalid reference count
* OCR Document entity counts
* normalization warning count
* output size
* Contract Version

Telemetry transport thuộc Runtime/Infrastructure.

---

# 52. Privacy

OCR Document có thể chứa toàn bộ source text.

Do đó:

* không log full OCR Document mặc định
* diagnostics chỉ lưu metadata cần thiết
* provider metadata phải được sanitize
* serialization/persistence phải tuân Privacy Profile
* local-only content giữ local boundary

---

# 53. Determinism

Cùng:

```text id="p34"
compatible upstream results
+
Postprocessing Profile
+
Postprocessing Strategy Version
```

phải tạo structurally equivalent OCR Document.

Ordering bên trong unordered collection phải có stable serialization rule khi cần reproducibility.

---

# 54. Architecture Invariants

OCR Postprocessing phải luôn đảm bảo:

1. Không thực hiện Detection.

2. Không thực hiện Recognition.

3. Không thực hiện Text Direction Analysis.

4. Không thực hiện Layout Analysis.

5. Không thực hiện Reading Order.

6. Không thực hiện Translation.

7. Không sửa recognized source meaning.

8. Không thay đổi canonical Detection Geometry.

9. Không thay đổi Layout semantics.

10. Không thay đổi Direction semantics.

11. Chỉ validate, normalize, merge và assemble.

12. OCR Document phải provider-neutral.

13. OCR Document phải giữ source/upstream lineage.

14. Entity reference phải hợp lệ.

15. Textual hierarchy và visual hierarchy không được trộn mất semantics.

16. Published OCR Document phải immutable.

17. Upstream rerun tạo OCR Document revision mới.

18. Provider-native model không crossing OCR Document boundary.

19. Postprocessing không sở hữu Runtime scheduling.

20. Postprocessing không sở hữu Runtime retry.

21. Postprocessing không sở hữu cancellation authority.

22. Postprocessing không sở hữu global cache lifecycle.

23. Quality Assessment không được thay thế bằng Postprocessing validation.

24. Reading Order không được tính trong Postprocessing.

---

# 55. Recommended MVP Postprocessing

MVP nên tập trung vào:

```text id="p35"
Detection Result
      +
Recognition Result
      +
Direction Result
      +
Layout Result
      ↓
Reference Validation
      ↓
Normalization
      ↓
Merge
      ↓
Consistency Check
      ↓
OCR Document
```

MVP nên hỗ trợ:

* Region mapping
* Paragraph / Line / Word / Character mapping
* Direction metadata
* Layout Tree references
* provider-neutral enums
* Validation Report
* immutable OCR Document
* versioned contract
* upstream lineage

Không cần ngay:

* complex conflict auto-repair
* semantic correction
* AI-based document reconstruction
* automatic text rewriting
* multi-document merge

---

# 56. Ownership References

| Concern                | Owner               |
| ---------------------- | ------------------- |
| Region / Geometry      | `DETECTION.md`      |
| Recognition Text Model | `RECOGNITION.md`    |
| Writing Direction      | `TEXT_DIRECTION.md` |
| Layout Tree            | `LAYOUT.md`         |
| OCR Document           | `POSTPROCESS.md`    |
| Quality Report         | `QUALITY.md`        |
| Reading Order          | `READING_ORDER.md`  |
| Provider Contract      | `PROVIDERS.md`      |
| Retry                  | Runtime             |
| Cancellation           | Runtime             |
| Scheduling             | Runtime             |
| Cache Lifecycle        | Runtime             |
| Event Transport        | Event Bus           |
| Telemetry Transport    | Infrastructure      |

---

# 57. Summary

OCR Postprocessing chuyển:

```text id="p36"
Detection Result
+
Recognition Result
+
Direction Result
+
Layout Result
```

thành:

```text id="p37"
OCR Document
```

thông qua:

```text id="p38"
Validate
    ↓
Normalize
    ↓
Merge
    ↓
Check Consistency
    ↓
Complete Metadata
    ↓
Assemble OCR Document
```

Boundary cốt lõi:

```text id="p39"
Upstream stages define meaning.

Postprocessing preserves and combines meaning.

Quality evaluates the document.

Reading Order defines precedence.

Runtime owns execution.
```
