# Text Direction

> **Status:** Draft
> **Version:** 1.1
> **Layer:** OCR Architecture
> **Depends On:** Detection, Recognition
> **Next Layer:** Layout Analysis, OCR Postprocessing, Reading Order

---

# 1. Purpose

Text Direction xác định cách văn bản được trình bày bên trong từng Region, Line hoặc Paragraph.

Nếu:

```text
Detection
    → "Text nằm ở đâu?"

Recognition
    → "Text là gì?"
```

thì Text Direction trả lời:

```text
"Text được viết theo hướng nào?"
```

Text Direction không xác định thứ tự đọc toàn Page.

Nó chỉ mô tả orientation và writing flow của text ở phạm vi cục bộ.

---

# 2. Scope

Text Direction chịu trách nhiệm:

* xác định Writing Mode
* xác định Line Direction
* xác định Paragraph Direction
* xác định Character Flow
* xác định Rotation metadata
* đánh giá Direction Confidence
* tạo Direction Result

Text Direction không chịu trách nhiệm:

* Text Detection
* Character Recognition
* page-level Reading Order
* Layout grouping
* Translation
* semantic text analysis
* Rendering
* Runtime scheduling
* Runtime retry
* Runtime cancellation
* Event Bus semantics
* global cache lifecycle

---

# 3. Goals

Text Direction hướng tới:

* stable output
* provider independence
* support for multilingual text
* support for vertical text
* support for rotated text
* support for mixed writing mode
* explicit direction semantics
* deterministic processing where possible

---

# 4. Non-Goals

Text Direction không thực hiện:

* OCR
* Translation
* Grammar Analysis
* final Reading Order
* Panel Ordering
* Layout Tree construction
* font recognition
* image correction

---

# 5. Architecture Position

```text
Detection
    ↓
Recognition
    ↓
Text Direction
    ↓
Direction Result
    ↓
Layout Analysis
    ↓
OCR Postprocessing
    ↓
Reading Order
```

Text Direction nằm giữa Recognition và các stage cần hiểu cách text được trình bày.

---

# 6. Terminology

## Text Direction

Khái niệm tổng quát mô tả hướng trình bày và flow của text.

---

## Writing Mode

Kiểu bố trí chính của text.

Ví dụ:

* Horizontal
* Vertical
* Mixed

---

## Line Direction

Hướng phát triển của một Line.

Ví dụ:

* LeftToRight
* RightToLeft
* TopToBottom
* BottomToTop

---

## Character Flow

Thứ tự Character trong cùng một Line.

---

## Paragraph Direction

Hướng tổ chức tổng thể của các Line trong một Paragraph.

---

## Orientation

Hướng visual của text relative với Region/Image.

---

## Rotation

Góc xoay của text.

---

## Direction Confidence

Độ tin cậy của một direction decision.

---

# 7. Core Input

Text Direction nhận:

```text
Detection Result
+
Recognition Result
+
Region Geometry
+
Character / Line Geometry
+
Direction Profile
```

Có thể sử dụng thêm:

* language hint
* script
* provider direction hint
* Region Type
* Rotation metadata

---

# 8. Core Output

Text Direction tạo:

```text
Direction Result
```

bao gồm:

```text
Direction Result
├── Metadata
├── Region Directions[]
│   ├── Writing Mode
│   ├── Line Direction
│   ├── Paragraph Direction
│   ├── Character Flow
│   ├── Rotation
│   └── Confidence
├── Diagnostics
└── Statistics
```

---

# 9. High-Level Direction Flow

```text
Recognition Result
      │
      ▼
1. Input Validation
      │
      ▼
2. Geometry Analysis
      │
      ▼
3. Orientation Analysis
      │
      ▼
4. Writing Mode Resolution
      │
      ▼
5. Line Direction Resolution
      │
      ▼
6. Paragraph Direction Resolution
      │
      ▼
7. Character Flow Resolution
      │
      ▼
8. Direction Result Assembly
```

Không phải mọi implementation đều cần tách riêng tất cả bước nội bộ.

Public semantics phải giữ nguyên.

---

# 10. Stage 1 — Input Validation

Text Direction kiểm tra:

* Region reference hợp lệ
* Recognition Result hợp lệ
* Geometry tồn tại khi cần
* Line/Character references hợp lệ
* Direction Profile hợp lệ

Text Direction không tự sửa Recognition Result hoặc Detection Geometry.

---

# 11. Stage 2 — Geometry Analysis

Geometry Analysis sử dụng:

* Region shape
* Character positions
* Line positions
* Paragraph shape
* rotation hints

để tạo evidence cho direction resolution.

Geometry ownership vẫn thuộc Detection/Recognition contract tương ứng.

---

# 12. Stage 3 — Orientation Analysis

Orientation Analysis xác định text có bị:

* rotated
* tilted
* vertically arranged
* horizontally arranged

hay không.

Orientation của text không đồng nghĩa image orientation.

Image orientation thuộc Preprocessing.

---

# 13. Image Orientation vs Text Direction

Hai khái niệm phải được tách rõ.

```text
PREPROCESS.md
    → Image Orientation

TEXT_DIRECTION.md
    → Text Writing Direction
```

Một image có orientation đúng nhưng text bên trong vẫn có thể:

* vertical
* rotated
* mixed

---

# 14. Stage 4 — Writing Mode Resolution

Writing Mode xác định cách text được tổ chức ở mức tổng quát.

Supported modes tối thiểu:

```text
Horizontal
Vertical
Mixed
Unknown
```

Writing Mode có thể tồn tại ở:

* Region
* Paragraph
* Line

Không bắt buộc toàn Region phải có một mode duy nhất.

---

# 15. Horizontal Writing Mode

Horizontal text thường có:

* Character flow theo chiều ngang
* nhiều Line xếp theo chiều dọc

Ví dụ phổ biến:

* Vietnamese
* English
* French
* German
* horizontal Chinese
* horizontal Japanese

Writing Mode không tự suy ra từ language code.

---

# 16. Vertical Writing Mode

Vertical text thường có:

* Character flow trong cột
* nhiều cột nằm cạnh nhau

Ví dụ:

* traditional Japanese layout
* traditional Chinese layout
* stylized manga text

Vertical writing không mặc định quyết định direction giữa các cột.

---

# 17. Mixed Writing Mode

Một Region có thể chứa nhiều mode.

Ví dụ:

```text
東京
TOKYO
```

hoặc một Bubble có:

* Japanese vertical text
* English horizontal annotation

Trong trường hợp này, direction metadata nên tồn tại ở scope nhỏ hơn thay vì ép toàn Region về một mode.

---

# 18. Stage 5 — Line Direction Resolution

Line Direction mô tả hướng Character flow trong Line.

Recommended values:

```text
LeftToRight
RightToLeft
TopToBottom
BottomToTop
Unknown
```

Line Direction không nhất thiết giống Paragraph Direction.

---

# 19. Character Flow

Character Flow mô tả thứ tự logical của Character trong một Line.

Ví dụ:

```text
A → B → C → D
```

hoặc:

```text
上
↓
下
```

Recognition có thể cung cấp provider hint.

Text Direction mới là owner của normalized Character Flow semantics.

---

# 20. Stage 6 — Paragraph Direction Resolution

Paragraph Direction mô tả cách nhiều Line được tổ chức trong Paragraph.

Ví dụ:

```text
Line 1
   ↓
Line 2
   ↓
Line 3
```

hoặc vertical columns:

```text
Column 3
   ←
Column 2
   ←
Column 1
```

Paragraph Direction không đồng nghĩa page Reading Order.

---

# 21. Stage 7 — Character Flow Resolution

Character Flow phải được giữ riêng với:

* Line Direction
* Paragraph Direction
* Page Reading Mode

Một text structure có thể có:

```text
Character Flow
    = TopToBottom

Column Flow
    = RightToLeft
```

Không nên gom hai chiều này thành một enum duy nhất.

---

# 22. Rotation

Rotation mô tả góc xoay của text Region/Line.

Có thể biểu diễn bằng:

```text
degree
```

theo một convention thống nhất.

Ví dụ:

* 0
* 90
* 180
* 270
* arbitrary angle

Rotation là metadata.

Không mutate source image.

---

# 23. Rotation Source

Rotation evidence có thể đến từ:

* Detection
* Recognition Provider
* geometry analysis
* orientation estimator

Nếu nhiều source mâu thuẫn, Direction Strategy phải resolve theo profile/priority.

---

# 24. Region Direction

Direction metadata có thể tồn tại ở Region scope.

Ví dụ:

```text
Region Direction
├── Writing Mode
├── Dominant Line Direction
├── Paragraph Direction
├── Rotation
└── Confidence
```

Region-level direction chỉ là summary khi Region chứa nhiều child text units.

---

# 25. Line-Level Direction

Line-level Direction nên được ưu tiên khi mixed text tồn tại.

Ví dụ:

```text
Region = Mixed

Line 1 = Vertical
Line 2 = Horizontal
```

Điều này giúp downstream không mất thông tin.

---

# 26. Paragraph-Level Direction

Paragraph-level Direction có thể mô tả cách các Line liên hệ với nhau.

Nó đặc biệt quan trọng cho:

* vertical manga text
* mixed-language documents
* multi-line labels
* complex bubble layout

---

# 27. Direction Confidence

Direction Confidence phải được đánh giá riêng cho từng loại decision.

Có thể có:

```text
Writing Mode Confidence
Line Direction Confidence
Paragraph Direction Confidence
Rotation Confidence
```

Không bắt buộc một confidence duy nhất cho toàn Result.

---

# 28. Confidence Semantics

Direction Confidence chỉ phản ánh:

```text
độ tin cậy của phân tích hướng
```

Nó không đồng nghĩa:

* Detection Confidence
* Recognition Confidence
* Reading Confidence
* Quality Score

Các confidence này không được cộng/trộn trực tiếp nếu không qua Quality semantics phù hợp.

---

# 29. Confidence Aggregation

Region-level Direction Confidence có thể aggregate từ:

* line-level confidence
* geometry agreement
* provider hint agreement
* script/writing-mode evidence

Aggregation algorithm thuộc Direction Strategy/Profile.

Public contract chỉ yêu cầu semantics rõ ràng.

---

# 30. Direction Strategy

Direction Strategy là thuật toán xác định direction.

Có thể gồm:

* Geometry Strategy
* Provider Hint Strategy
* Script-aware Strategy
* Hybrid Strategy
* AI-assisted Strategy

Mọi Strategy phải tạo cùng `Direction Result` contract.

---

# 31. Geometry Strategy

Có thể sử dụng:

* Character arrangement
* Line aspect ratio
* alignment
* bounding geometry
* relative positions

Geometry là evidence quan trọng nhưng không phải lúc nào cũng đủ.

---

# 32. Script / Language Hints

Script/language có thể hỗ trợ inference.

Ví dụ:

* Han text có thể horizontal hoặc vertical
* Latin thường horizontal nhưng không phải tuyệt đối

Do đó:

```text
Language Hint
```

không phải authoritative direction source.

---

# 33. Provider Hints

OCR Provider có thể trả:

* orientation
* vertical/horizontal hint
* line direction
* text rotation

Provider hints phải được normalize về CRAI Direction model.

Downstream không phụ thuộc provider-native enum.

---

# 34. Detection Integration

Detection cung cấp:

* Region
* Region Geometry
* Rotation hint
* Region Type

Text Direction sử dụng chúng làm evidence.

Detection Direction Hint không phải authoritative Direction Result.

---

# 35. Recognition Integration

Recognition cung cấp:

* Character positions
* Line positions
* Paragraph structure
* Language
* Script
* Writing Mode hint

Text Direction normalize các tín hiệu này thành direction semantics chính thức.

---

# 36. Layout Integration

Layout có thể sử dụng Direction Result để:

* group Region
* infer Block orientation
* validate Container structure
* understand vertical/horizontal layout

Layout không được redefine Text Direction semantics.

---

# 37. Reading Order Integration

Reading Order sử dụng Direction Result như một signal.

Ví dụ:

```text
Text Direction
    → vertical text inside Region

Reading Order
    → which Region comes before another
```

Hai concept khác nhau.

---

# 38. Text Direction vs Reading Order

Boundary bắt buộc:

```text
Text Direction
    = flow inside textual structure

Reading Order
    = precedence among OCR entities
```

Ví dụ manga:

```text
Characters in column
    = TopToBottom

Columns
    = RightToLeft

Bubbles on Page
    = resolved by Reading Order
```

Không được dùng một Direction field để đại diện cả ba.

---

# 39. Postprocessing Integration

OCR Postprocessing hợp nhất:

```text
Detection Result
+
Recognition Result
+
Direction Result
+
Layout Result
```

thành OCR Document.

Text Direction không tự xây canonical OCR Document.

---

# 40. Immutability

Published Direction Result phải immutable.

Nếu analysis chạy lại:

```text
new Direction Result revision
```

phải được tạo.

Không silent-mutate direction metadata đã publish.

---

# 41. Identity and References

Direction Result phải tham chiếu:

* Region ID
* Line ID
* Paragraph ID

thay vì clone các entity nguồn thành identity mới.

Điều này giữ lineage xuyên suốt OCR pipeline.

---

# 42. Determinism

Cùng:

```text
Recognition semantic input
+
Detection semantic input
+
Direction Profile
+
Direction Strategy Version
```

nên tạo structurally equivalent Direction Result.

Provider nondeterminism nếu có phải được ghi rõ.

---

# 43. Direction Profile

Direction Profile có thể định nghĩa:

* allowed Writing Modes
* confidence threshold
* provider-hint priority
* rotation handling
* mixed-mode behavior
* fallback strategy

Profile phải versioned nếu thay đổi semantics.

---

# 44. Unknown Direction

Khi evidence không đủ:

```text
Unknown
```

phải được sử dụng.

Không được ép thành Horizontal hoặc Vertical chỉ để tránh unknown state.

Unknown là semantic result hợp lệ.

---

# 45. Ambiguous Direction

Nếu nhiều direction candidate có confidence gần nhau:

```text
Ambiguous
```

có thể được biểu diễn bằng Diagnostics/Metadata tùy contract.

Ambiguous không đồng nghĩa execution failure.

---

# 46. Direction Compatibility

Direction Result có thể không còn tương thích khi:

* Recognition Result thay đổi
* Character/Line geometry thay đổi
* Region Geometry thay đổi
* Direction Profile thay đổi
* Direction Strategy Version thay đổi

Text Direction chỉ định nghĩa semantic compatibility.

Global cache lifecycle thuộc Runtime.

---

# 47. Incremental Semantics

Direction Analysis có thể recompute theo Region/Line scope.

Ví dụ:

* một Region được Recognition lại
* một Line thay đổi
* Region geometry thay đổi

Semantic requirement:

```text
unchanged scope
    → preserve compatible direction metadata

changed scope
    → recompute affected direction
```

Execution orchestration thuộc Runtime.

---

# 48. Runtime Integration

Text Direction không sở hữu:

* queue
* execution state
* retry attempt
* cancellation authority
* Scheduler
* stale-result authority

Runtime sở hữu execution.

Text Direction chỉ tạo semantic result/failure information.

---

# 49. Retry Integration

Text Direction có thể báo:

* low confidence
* ambiguous direction
* unsupported writing mode
* invalid geometry

Quality/Runtime có thể dùng các signal này để quyết định bước tiếp.

Text Direction không tự retry.

---

# 50. Cache Integration

Text Direction có thể xác định semantic invalidation.

Global:

* cache storage
* retention
* eviction
* cleanup

thuộc Runtime Cache Policy.

---

# 51. Event Integration

Text Direction có thể tạo domain facts như:

```text
DirectionAnalysisCompleted
DirectionAmbiguous
DirectionAnalysisFailed
```

Ý nghĩa semantic thuộc Text Direction.

Event transport thuộc Event Bus.

---

# 52. Error Integration

Direction-specific semantic errors có thể gồm:

```text
InvalidRecognitionReference
InvalidGeometry
UnsupportedWritingMode
DirectionResultInvalid
StrategyUnavailable
```

Provider-specific errors phải được normalize trước khi crossing boundary.

Runtime Error Model sở hữu execution normalization.

---

# 53. Observability Integration

Useful measurements có thể gồm:

* Direction analysis duration
* Horizontal Region count
* Vertical Region count
* Mixed Region count
* Unknown count
* confidence distribution
* rotation distribution
* Strategy Version

Telemetry transport thuộc Runtime/Infrastructure.

---

# 54. Privacy

Text Direction chủ yếu thao tác metadata và recognized structure.

Tuy nhiên Diagnostics không nên log raw recognized text khi không cần thiết.

Privacy policy của OCR Document vẫn phải được giữ.

---

# 55. Architecture Invariants

Text Direction phải luôn đảm bảo:

1. Không thực hiện Detection.

2. Không thực hiện Recognition.

3. Không thay đổi recognized text.

4. Không thay đổi Detection Geometry.

5. Không thực hiện Translation.

6. Không sở hữu Layout Tree.

7. Không sở hữu final Reading Order.

8. Writing Mode, Line Direction, Paragraph Direction và Character Flow là các concept riêng biệt.

9. Image Orientation và Text Direction không phải cùng một concept.

10. Provider hint không phải authoritative truth.

11. Unknown Direction là kết quả hợp lệ.

12. Direction Confidence không đồng nghĩa Recognition Confidence.

13. Direction Result phải provider-neutral.

14. Published Direction Result phải immutable.

15. Rerun tạo revision mới.

16. Direction entity phải giữ mapping về Region/Line/Paragraph nguồn.

17. Text Direction không sở hữu Runtime scheduling.

18. Text Direction không sở hữu Runtime retry.

19. Text Direction không sở hữu cancellation authority.

20. Text Direction không sở hữu global cache lifecycle.

---

# 56. Recommended MVP Text Direction

MVP nên tập trung vào:

```text
Recognition Result
      ↓
Geometry Analysis
      ↓
Horizontal / Vertical Detection
      ↓
Line Direction
      ↓
Basic Rotation
      ↓
Direction Result
```

MVP nên hỗ trợ:

* Horizontal LTR
* Horizontal RTL
* Vertical TTB
* Vertical column flow
* Mixed
* Unknown
* basic rotation
* line-level direction
* Direction Confidence
* provider-neutral contract

Không bắt buộc ngay:

* advanced AI direction inference
* complex historical writing systems
* learned mixed-direction strategy
* document-wide semantic direction inference

---

# 57. Ownership References

| Concern                      | Owner               |
| ---------------------------- | ------------------- |
| Region                       | `DETECTION.md`      |
| Region Geometry              | `DETECTION.md`      |
| Character / Line / Paragraph | `RECOGNITION.md`    |
| Writing Mode                 | `TEXT_DIRECTION.md` |
| Line Direction               | `TEXT_DIRECTION.md` |
| Paragraph Direction          | `TEXT_DIRECTION.md` |
| Character Flow               | `TEXT_DIRECTION.md` |
| Rotation Metadata            | `TEXT_DIRECTION.md` |
| Layout Tree                  | `LAYOUT.md`         |
| Reading Order                | `READING_ORDER.md`  |
| OCR Document                 | `POSTPROCESS.md`    |
| Quality                      | `QUALITY.md`        |
| Retry                        | Runtime             |
| Cancellation                 | Runtime             |
| Scheduling                   | Runtime             |
| Cache Lifecycle              | Runtime             |
| Event Transport              | Event Bus           |
| Telemetry Transport          | Infrastructure      |

---

# 58. Summary

Text Direction chuyển:

```text
Recognition Result
+
Geometry
```

thành:

```text
Direction Result
```

với:

```text
Writing Mode
+
Line Direction
+
Paragraph Direction
+
Character Flow
+
Rotation
+
Direction Confidence
```

Boundary tổng quát:

```text
Detection
    → Where is the text?

Recognition
    → What is the text?

Text Direction
    → How is the text written?

Layout
    → How is it organized?

Reading Order
    → In what order should it be read?
```

Nguyên tắc cốt lõi:

```text
Text Direction owns local writing flow.

Layout owns spatial structure.

Reading Order owns precedence.

Runtime owns execution.
```
