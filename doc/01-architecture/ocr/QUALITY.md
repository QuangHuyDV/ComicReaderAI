# OCR Quality Assessment

> **Status:** Draft
> **Version:** 1.1
> **Layer:** OCR Architecture
> **Depends On:** OCR Postprocessing
> **Consumes:** OCR Document
> **Next Layer:** Runtime Decision, Reading Order, Text Processing

---

# 1. Purpose

OCR Quality Assessment đánh giá mức độ đáng tin cậy và khả dụng của một `OCR Document`.

Nếu:

```text
Postprocessing
    → "OCR data có nhất quán về cấu trúc không?"
```

thì Quality Assessment trả lời:

```text
"OCR Document này tốt đến mức nào
và có phù hợp để tiếp tục xử lý không?"
```

Quality Assessment không thực hiện OCR mới.

Nó cũng không thay đổi OCR Document.

Nó chỉ:

* evaluate
* aggregate quality signals
* detect quality issues
* classify quality
* generate recommendations
* produce Quality Report

---

# 2. Scope

Quality Assessment chịu trách nhiệm:

* Quality Validation
* Confidence Aggregation
* Quality Dimension Evaluation
* Quality Scoring
* Quality Classification
* Issue Detection
* Recommendation Generation
* Quality Diagnostics
* Quality Report Assembly

Quality Assessment không chịu trách nhiệm:

* Detection
* Recognition
* Text Direction Analysis
* Layout Analysis
* OCR Postprocessing
* Reading Order
* Translation
* text correction
* image enhancement
* Runtime scheduling
* Runtime retry execution
* provider switching execution
* cancellation authority
* Event Bus behavior

---

# 3. Goals

Quality Assessment hướng tới:

* provider independence
* stable evaluation semantics
* explainability
* multi-dimensional evaluation
* deterministic classification where possible
* explicit quality issues
* Runtime-friendly output
* benchmark-friendly metrics
* non-mutating evaluation

---

# 4. Non-Goals

Quality Assessment không:

* sửa OCR Document
* rerun OCR
* tự retry Detection
* tự retry Recognition
* tự đổi Provider
* tự skip Translation
* chỉnh spelling
* chỉnh grammar
* rewrite source text
* thay đổi Geometry
* thay đổi Layout
* thay đổi Direction

---

# 5. Architecture Position

```text
Detection
    ↓
Recognition
    ↓
Text Direction
    ↓
Layout
    ↓
OCR Postprocessing
    ↓
OCR Document
    │
    ├──► Quality Assessment
    │        ↓
    │    Quality Report
    │        ↓
    │    Runtime Decision
    │
    └──► Reading Order
```

Quality Assessment là evaluation layer.

Nó không phải transformation stage của OCR Document.

---

# 6. Terminology

## Quality

Mức độ đáng tin cậy và khả dụng tổng thể của OCR Document.

---

## Confidence

Độ tin cậy của một decision hoặc output cụ thể.

Ví dụ:

* Detection Confidence
* Recognition Confidence
* Direction Confidence
* Layout Confidence
* Reading Confidence

Confidence không đồng nghĩa Quality.

---

## Quality Dimension

Một khía cạnh độc lập được dùng để đánh giá OCR Document.

---

## Quality Score

Điểm tổng hợp của một evaluation scope.

---

## Quality Grade

Phân loại dễ dùng cho Runtime hoặc UI.

Ví dụ:

* Excellent
* Good
* Acceptable
* Poor
* Failed

---

## Quality Issue

Một vấn đề ảnh hưởng tới quality.

---

## Recommendation

Khuyến nghị semantic dành cho consumer.

Recommendation không trực tiếp thực thi action.

---

## Quality Profile

Policy điều khiển cách đánh giá quality.

---

# 7. Core Input

Quality Assessment nhận:

```text
OCR Document
+
OCR Metadata
+
Processing Statistics
+
Quality Profile
```

Có thể sử dụng:

* Detection Confidence
* Recognition Confidence
* Direction Confidence
* Layout Confidence
* Validation Report
* structural diagnostics
* provider metadata đã normalize
* processing statistics

---

# 8. Core Output

Output chính:

```text
Quality Report
```

Conceptual structure:

```text
Quality Report
├── Metadata
├── Overall Score
├── Overall Grade
├── Dimension Scores
├── Confidence Summary
├── Quality Issues
├── Recommendations
├── Diagnostics
└── Statistics
```

---

# 9. High-Level Quality Flow

```text
OCR Document
     │
     ▼
1. Input Validation
     │
     ▼
2. Quality Context Resolution
     │
     ▼
3. Dimension Evaluation
     │
     ▼
4. Confidence Aggregation
     │
     ▼
5. Issue Detection
     │
     ▼
6. Quality Scoring
     │
     ▼
7. Quality Classification
     │
     ▼
8. Recommendation Generation
     │
     ▼
9. Quality Report Assembly
```

---

# 10. Stage 1 — Input Validation

Quality Assessment kiểm tra:

* OCR Document hợp lệ
* contract version được hỗ trợ
* required metadata tồn tại
* quality profile hợp lệ
* referenced confidence/diagnostic data hợp lệ

Nếu OCR Document structurally invalid hoàn toàn, Quality có thể trả:

```text
Grade = Failed
```

hoặc semantic invalid evaluation result theo profile.

Quality không sửa document.

---

# 11. Stage 2 — Quality Context Resolution

Quality Context có thể phụ thuộc:

* OCR Profile
* Quality Profile
* document type
* source type
* expected language
* expected script
* Region Type distribution
* quality requirement
* processing mode

Quality rules phải explicit và versioned.

---

# 12. Stage 3 — Quality Dimensions

Quality không nên được biểu diễn chỉ bằng một scalar.

Recommended dimensions:

* Detection Quality
* Recognition Quality
* Direction Quality
* Layout Quality
* Structural Quality
* Metadata Quality

Có thể bổ sung:

* Completeness Quality
* Consistency Quality

nếu contract cần.

---

# 13. Detection Quality

Detection Quality có thể xem xét:

* Region count plausibility
* low Detection Confidence ratio
* invalid Region count
* duplicate Region count
* suspicious Geometry
* unresolved Region Type ratio

Quality chỉ đánh giá Detection output.

Không redefine Detection Confidence semantics.

---

# 14. Recognition Quality

Recognition Quality có thể xem xét:

* empty recognition ratio
* Character Confidence
* Line Confidence
* Region Recognition Confidence
* invalid text structure
* suspicious language/script mismatch
* missing expected text structure

Quality không sửa recognized text.

---

# 15. Direction Quality

Direction Quality có thể xem xét:

* Unknown Writing Mode ratio
* ambiguous Direction
* low Direction Confidence
* inconsistent Line/Paragraph Direction
* suspicious Rotation

Direction semantics vẫn thuộc `TEXT_DIRECTION.md`.

---

# 16. Layout Quality

Layout Quality có thể xem xét:

* invalid hierarchy
* orphan Region
* unresolved containment
* ambiguous Panel/Container grouping
* low Layout Confidence nếu có
* structural relationship conflicts

Layout semantics vẫn thuộc `LAYOUT.md`.

---

# 17. Structural Quality

Structural Quality đánh giá canonical OCR Document.

Ví dụ:

* Region thiếu Recognition mapping
* Paragraph không có Line
* Line reference invalid
* Word reference invalid
* Character orphan
* Layout Region missing
* Direction reference missing
* broken parent-child relationship

---

# 18. Metadata Quality

Metadata Quality có thể xem xét:

* missing version
* missing source identity
* missing profile identity
* missing lineage
* malformed provider metadata envelope
* missing creation metadata

Metadata thiếu không mặc định làm OCR unusable.

Severity phụ thuộc field và profile.

---

# 19. Confidence Aggregation

Quality có thể aggregate confidence từ nhiều owner.

Ví dụ:

```text
Detection Confidence
Recognition Confidence
Direction Confidence
Layout Confidence
```

Nhưng aggregation không làm thay đổi ownership.

Quality chỉ consume chúng.

---

# 20. Confidence Ownership

Authoritative confidence semantics:

```text
Detection Confidence
    → DETECTION.md

Recognition Confidence
    → RECOGNITION.md

Direction Confidence
    → TEXT_DIRECTION.md

Layout Confidence
    → LAYOUT.md

Reading Confidence
    → READING_ORDER.md
```

Quality không redefine các confidence này.

---

# 21. Aggregation Rules

Không nên dùng:

```text
average(all confidence)
```

như default universal rule.

Aggregation có thể:

* weighted
* threshold-based
* dimension-based
* worst-case sensitive
* profile-specific

Algorithm thuộc `Quality Profile` hoặc Quality Strategy.

---

# 22. Missing Confidence

Không phải mọi provider/stage đều luôn có confidence đầy đủ.

Quality phải phân biệt:

```text
missing confidence
```

và:

```text
low confidence
```

Hai trường hợp không giống nhau.

---

# 23. Stage 5 — Issue Detection

Quality Issue là observation làm giảm quality.

Ví dụ:

```text
MissingRegion
EmptyRecognition
LowConfidence
InvalidGeometry
InvalidDirection
DuplicateRegion
BrokenHierarchy
MissingMetadata
InconsistentLanguage
AmbiguousLayout
```

Một document có thể có nhiều issue cùng lúc.

---

# 24. Issue Model

Conceptual structure:

```text
Quality Issue
├── Issue Code
├── Dimension
├── Severity
├── Entity Reference
├── Evidence
├── Confidence
└── Metadata
```

Issue phải giữ reference về entity gốc khi applicable.

---

# 25. Issue Severity

Recommended semantic levels:

```text
Info
Warning
Error
Critical
```

Severity không đồng nghĩa Runtime action.

Runtime có thể map severity theo policy riêng.

---

# 26. Quality vs Validation Error

Postprocessing Validation và Quality Issue khác nhau.

Ví dụ:

```text
Invalid Region Reference
    → structural contract problem
```

thuộc Postprocessing Validation.

Trong khi:

```text
Recognition confidence low but structurally valid
```

là Quality Issue.

Boundary:

```text
Postprocessing
    → Is the document structurally valid?

Quality
    → Is the valid document good enough?
```

---

# 27. Stage 6 — Quality Scoring

Quality Score là normalized evaluation value.

Recommended range:

```text
0.0 → 1.0
```

hoặc:

```text
0 → 100
```

Một project phải chọn một canonical scale.

Không nên trộn nhiều scale trong cùng contract.

---

# 28. Dimension Scores

Mỗi dimension có thể có score riêng.

Ví dụ:

```text
Detection     0.92
Recognition   0.81
Direction     0.95
Layout        0.88
Structural    1.00
Metadata      0.97
```

Overall Score không bắt buộc là arithmetic mean.

---

# 29. Overall Score

Overall Score nên được tính bằng Quality Strategy/Profile.

Nó có thể phản ánh:

* weighted dimensions
* hard quality floor
* critical issue penalties
* missing evidence penalties
* confidence distribution

Provider-specific formula không được leak vào public contract.

---

# 30. Stage 7 — Quality Classification

Quality Grade giúp consumer không cần phân tích raw scores.

Recommended default grades:

```text
Excellent
Good
Acceptable
Poor
Failed
```

Exact thresholds thuộc Quality Profile.

---

# 31. Grade Semantics

Grade phải có semantics ổn định.

Ví dụ:

```text
Excellent
    → high confidence, no meaningful issue

Good
    → usable, minor issue

Acceptable
    → usable with caution

Poor
    → likely requires intervention

Failed
    → unsuitable for normal downstream use
```

Exact policy vẫn thuộc profile.

---

# 32. Grade vs Runtime Decision

Grade không phải command.

Ví dụ:

```text
Grade = Poor
```

không có nghĩa Quality tự chạy Retry.

Runtime mới quyết định action.

---

# 33. Stage 8 — Recommendation Generation

Quality có thể sinh recommendations như:

```text
Continue
RetryOCR
RetryDetection
RetryRecognition
SwitchProvider
RequestHigherResolution
ManualReview
SkipTranslation
```

Recommendation chỉ là semantic suggestion.

---

# 34. Recommendation Model

Conceptual structure:

```text
Recommendation
├── Type
├── Reason
├── Target Scope
├── Priority
├── Confidence
└── Evidence
```

---

# 35. Recommendation Ownership Boundary

Quality sở hữu:

```text
what is recommended
```

Runtime sở hữu:

```text
whether the recommendation is executed
```

Ví dụ:

```text
Quality
    → RetryRecognition recommended

Runtime
    → decides Retry / Continue / Stop
```

---

# 36. Multiple Recommendations

Quality Report có thể chứa nhiều recommendation.

Ví dụ:

```text
ManualReview
+
Continue
```

hoặc:

```text
RetryRecognition
+
SwitchProvider
```

Priority phải explicit.

Runtime không nên suy luận priority từ array order.

---

# 37. Stage 9 — Quality Report Assembly

Quality Report phải chứa đủ thông tin để consumer hiểu:

* overall quality
* quality dimensions
* issues
* recommendations
* evidence
* evaluation version

mà không cần chạy lại quality algorithm.

---

# 38. Quality Report Identity

Quality Report phải có identity riêng.

Có thể liên kết với:

* OCR Document ID
* OCR Document Version
* Quality Profile Version
* Quality Strategy Version

Exact artifact identity thuộc Runtime/Artifact contract.

---

# 39. Immutability

Published Quality Report phải immutable.

Nếu:

* OCR Document thay đổi
* Quality Profile thay đổi
* Quality Strategy thay đổi

thì phải tạo:

```text
new Quality Report revision
```

không sửa silent report cũ.

---

# 40. Lineage

Quality Report phải giữ reference tới OCR Document mà nó đánh giá.

Conceptually:

```text
Quality Report
    ↓ evaluates
OCR Document Revision N
```

Không được áp dụng report cũ cho OCR Document revision mới nếu compatibility chưa được xác minh.

---

# 41. Quality Compatibility

Quality Report không còn compatible khi:

* OCR Document revision thay đổi
* Quality Profile thay đổi
* Quality Strategy version thay đổi
* dimension semantics thay đổi

Quality chỉ định nghĩa semantic compatibility.

Cache lifecycle thuộc Runtime.

---

# 42. Determinism

Cùng:

```text
OCR Document semantic identity
+
Quality Profile
+
Quality Strategy Version
```

phải tạo structurally equivalent Quality Report.

Nếu AI-assisted evaluation có nondeterminism, metadata phải ghi rõ.

---

# 43. Quality Profile

Quality Profile có thể định nghĩa:

* required dimensions
* dimension weights
* issue severity
* score normalization
* grade thresholds
* recommendation thresholds
* required metadata
* expected confidence coverage

Profile phải versioned.

---

# 44. Quality Strategy

Quality Strategy là implementation-neutral evaluation algorithm.

Có thể gồm:

* Rule-based Strategy
* Weighted Score Strategy
* Threshold Strategy
* Hybrid Strategy
* AI-assisted Strategy

Mọi strategy phải tạo cùng `Quality Report` contract.

---

# 45. Provider Independence

Quality không được phụ thuộc trực tiếp vào:

* PaddleOCR score semantics
* Google Vision result classes
* Azure-native confidence format
* Tesseract internal structures

Provider outputs phải được normalize trước khi Quality sử dụng.

---

# 46. Provider Comparison

Quality Report có thể hỗ trợ provider comparison gián tiếp.

Ví dụ:

```text
OCR Document A
    → Provider A

OCR Document B
    → Provider B
```

được đánh giá bằng cùng Quality Profile.

Quality semantics phải giữ provider-neutral để comparison có ý nghĩa.

---

# 47. Reading Order Integration

Reading Order có thể consume OCR Document độc lập.

Nếu pipeline policy yêu cầu quality gate trước Reading Order:

```text
OCR Document
    ↓
Quality Report
    ↓
Runtime Decision
    ↓
Reading Order
```

Nếu không:

```text
OCR Document
    ├── Quality
    └── Reading Order
```

Execution ordering cụ thể thuộc Runtime/Pipeline orchestration.

Quality không sở hữu Reading Order.

---

# 48. Runtime Integration

Runtime có thể consume:

* Quality Grade
* Quality Score
* Quality Issues
* Recommendations

để quyết định:

* Continue
* Retry
* Fallback
* Stop
* Manual Review

Runtime vẫn là execution authority.

---

# 49. Retry Integration

Quality có thể recommend:

```text
RetryOCR
RetryDetection
RetryRecognition
```

nhưng không:

* tạo Attempt
* schedule WorkItem
* chọn retry delay
* quyết định retry budget

Những phần đó thuộc Runtime Retry Policy.

---

# 50. Provider Switching Integration

Quality có thể recommend:

```text
SwitchProvider
```

nhưng không chọn hoặc kích hoạt provider trực tiếp.

Provider selection/routing execution thuộc owner tương ứng.

---

# 51. Cache Integration

Quality có thể xác định report compatibility.

Global:

* storage
* retention
* eviction
* cache lookup policy

thuộc Runtime.

---

# 52. Event Integration

Quality có thể tạo semantic facts như:

```text
QualityAssessmentCompleted
QualityBelowThreshold
QualityAssessmentFailed
ManualReviewRecommended
```

Ý nghĩa semantic thuộc Quality.

Event transport/envelope thuộc Event Bus.

---

# 53. Error Integration

Quality-specific semantic errors có thể gồm:

```text
InvalidOCRDocument
InvalidQualityProfile
InsufficientQualityEvidence
QualityStrategyUnavailable
QualityReportInvalid
UnsupportedContractVersion
```

Runtime Error Model sở hữu execution-level normalization.

---

# 54. Diagnostics

Quality Diagnostics có thể chứa:

* dimension breakdown
* issue evidence
* recommendation reason
* confidence coverage
* grade decision
* missing metadata
* fallback evaluation rule

Diagnostics không nên chứa raw OCR text nếu không cần thiết.

---

# 55. Benchmark Integration

Quality outputs rất phù hợp cho:

* regression testing
* provider comparison
* profile tuning
* dataset evaluation

Benchmarking có thể consume Quality Report.

Quality không sở hữu toàn bộ benchmark infrastructure.

---

# 56. Quality Metrics

Useful metrics có thể gồm:

* Overall Quality Score
* Grade distribution
* issue count by type
* low-confidence rate
* failed document rate
* manual review rate
* recommendation distribution

Telemetry transport thuộc Infrastructure.

---

# 57. Privacy

Quality chủ yếu xử lý OCR metadata và structure.

Tuy nhiên:

* issue evidence không nên chứa raw text mặc định
* report không nên copy toàn bộ OCR Document
* sensitive source content phải giữ privacy boundary
* remote AI-assisted Quality Strategy phải tuân Privacy Profile

---

# 58. Architecture Invariants

Quality Assessment phải luôn đảm bảo:

1. Không thực hiện OCR.

2. Không thay đổi OCR Document.

3. Không sửa Recognition content.

4. Không thay đổi Detection Geometry.

5. Không thay đổi Layout.

6. Không thay đổi Text Direction.

7. Không thực hiện Reading Order.

8. Không thực hiện Translation.

9. Confidence và Quality là hai concept khác nhau.

10. Confidence semantics vẫn thuộc stage tạo ra nó.

11. Quality aggregate confidence nhưng không redefine chúng.

12. Quality phải hỗ trợ multi-dimensional evaluation.

13. Overall Score không phải nguồn thông tin duy nhất.

14. Recommendation không phải command.

15. Runtime mới sở hữu action execution.

16. Quality Report phải provider-neutral.

17. Quality Report phải tham chiếu exact OCR Document revision.

18. Published Quality Report phải immutable.

19. Quality Profile/Strategy phải versioned khi thay đổi semantics.

20. Quality không sở hữu Runtime scheduling.

21. Quality không sở hữu Runtime retry execution.

22. Quality không sở hữu cancellation authority.

23. Quality không sở hữu global cache lifecycle.

24. Quality Diagnostics không được leak sensitive OCR content mặc định.

---

# 59. Recommended MVP Quality Assessment

MVP nên tập trung vào:

```text
OCR Document
    ↓
Structural Checks
    ↓
Recognition Confidence Summary
    ↓
Detection Confidence Summary
    ↓
Direction Confidence Summary
    ↓
Issue Detection
    ↓
Basic Score
    ↓
Basic Grade
    ↓
Recommendation
    ↓
Quality Report
```

MVP nên hỗ trợ:

* Detection Quality
* Recognition Quality
* Direction Quality
* Structural Quality
* Overall Score
* Overall Grade
* Quality Issues
* Continue
* RetryRecognition
* RetryOCR
* ManualReview
* provider-neutral Quality Report

Không cần ngay:

* AI-based Quality evaluation
* learned scoring
* complex provider ensemble evaluation
* adaptive threshold learning
* automatic correction

---

# 60. Ownership References

| Concern                      | Owner                          |
| ---------------------------- | ------------------------------ |
| OCR Document                 | `POSTPROCESS.md`               |
| Detection Confidence         | `DETECTION.md`                 |
| Recognition Confidence       | `RECOGNITION.md`               |
| Direction Confidence         | `TEXT_DIRECTION.md`            |
| Layout Quality Signals       | `LAYOUT.md`                    |
| Quality Report               | `QUALITY.md`                   |
| Quality Score / Grade        | `QUALITY.md`                   |
| Quality Issues               | `QUALITY.md`                   |
| Recommendations              | `QUALITY.md`                   |
| Reading Order                | `READING_ORDER.md`             |
| Retry Execution              | Runtime                        |
| Provider Switching Execution | Runtime / Provider Integration |
| Scheduling                   | Runtime                        |
| Cache Lifecycle              | Runtime                        |
| Event Transport              | Event Bus                      |
| Telemetry Transport          | Infrastructure                 |

---

# 61. Summary

Quality Assessment chuyển:

```text
OCR Document
```

thành:

```text
Quality Report
```

với:

```text
Quality Dimensions
+
Confidence Summary
+
Quality Score
+
Quality Grade
+
Quality Issues
+
Recommendations
```

Boundary cốt lõi:

```text
Postprocessing
    → builds a structurally consistent OCR Document

Quality
    → evaluates how trustworthy that document is

Runtime
    → decides what action to execute
```

Nguyên tắc quan trọng nhất:

```text
Quality evaluates.

Quality recommends.

Runtime decides.

Runtime executes.
```
