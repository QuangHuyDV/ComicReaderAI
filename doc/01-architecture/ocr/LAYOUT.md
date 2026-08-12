# Layout Analysis

> **Status:** Draft
> **Version:** 1.2.0
> **Layer:** OCR Architecture
> **Depends On:** Detection, Recognition, Text Direction
> **Next Layer:** OCR Postprocessing, Reading Order

---

# 1. Purpose

Layout Analysis xác định cách các visual entity trong một Page được tổ chức về mặt không gian và cấu trúc.

Nếu:

```text
Detection
    → "Text nằm ở đâu?"

Recognition
    → "Text là gì?"

Text Direction
    → "Text được viết theo hướng nào?"
```

thì Layout Analysis trả lời:

```text
"Các vùng này được tổ chức như thế nào?"
```

Layout Analysis không quyết định thứ tự đọc cuối cùng.

Nó tạo ra cấu trúc không gian mà Reading Order có thể sử dụng.

---

# 2. Scope

Layout Analysis chịu trách nhiệm:

* phân tích cấu trúc Page
* nhóm Region
* xác định Panel
* xác định Container
* xác định Block
* xây dựng Parent-Child relationships
* xây dựng Layout Tree
* xây dựng Spatial Relationship Graph
* chuẩn hóa layout-specific geometry relationships
* tạo Layout Result

Layout Analysis không chịu trách nhiệm:

* Detection
* Recognition
* Translation
* final Reading Order
* semantic text reconstruction
* Rendering
* Runtime scheduling
* Runtime same-work retry
* Event Bus behavior
* global cache lifecycle

---

# 3. Goals

Layout Analysis hướng tới:

* stable structure
* provider independence
* deterministic output
* reusable layout representation
* support for comic, manga, webtoon and document layouts
* explicit hierarchy
* explicit spatial relationships
* preservation of Region identity

---

# 4. Non-Goals

Layout Analysis không thực hiện:

* Character Recognition
* Text Detection
* Translation
* Grammar Analysis
* final Reading Order
* semantic understanding
* speaker identification
* visual rendering
* image enhancement

---

# 5. Architecture Position

```text
Detection
    ↓
Recognition
    ↓
Text Direction
    ↓
Layout Analysis
    ↓
Layout Result
    ↓
OCR Postprocessing
    ↓
Reading Order
```

Layout nằm giữa OCR content extraction và Reading Order.

Nó không thay đổi nội dung Recognition.

---

# 6. Terminology

## Layout

Cấu trúc trực quan của một Page.

---

## Region

Đơn vị visual do Detection sinh ra.

Authoritative Region semantics thuộc `DETECTION.md`.

---

## Panel

Một vùng độc lập trong Page.

Ví dụ:

* comic panel
* illustration frame
* page subsection

---

## Container

Đối tượng trực quan chứa Region, Block hoặc Container khác.

Ví dụ:

* Speech Bubble
* Narration Box
* UI Window
* Tooltip

---

## Block

Một nhóm Region có quan hệ trực quan mạnh.

Ví dụ:

* paragraph block
* title block
* caption block
* dialogue group

---

## Layout Tree

Cây biểu diễn cấu trúc phân cấp của Page.

---

## Spatial Relationship Graph

Graph biểu diễn các quan hệ không gian giữa entity.

---

## Spatial Relationship

Quan hệ như:

* contains
* inside
* overlaps
* intersects
* adjacent
* above
* below
* left_of
* right_of
* aligned
* touching

---

# 7. Core Input

Layout Analysis nhận:

```text
Detection Result
+
Recognition Result
+
Direction Result
+
Geometry
+
Region Type
+
Layout Profile
```

Recognition content có thể hỗ trợ grouping khi contract cho phép, nhưng layout semantics không được phụ thuộc vào semantic meaning của text.

---

# 8. Core Output

Layout Analysis tạo:

```text
Layout Result
```

bao gồm:

```text
Layout Result
├── Metadata
├── Page
├── Panels[]
├── Containers[]
├── Blocks[]
├── Layout Tree
├── Spatial Relationship Graph
├── Diagnostics
└── Statistics
```

---

# 9. High-Level Layout Flow

```text
Detection / Recognition / Direction
              │
              ▼
1. Input Validation
              │
              ▼
2. Spatial Analysis
              │
              ▼
3. Region Grouping
              │
              ▼
4. Container Detection
              │
              ▼
5. Panel Detection
              │
              ▼
6. Hierarchy Construction
              │
              ▼
7. Relationship Analysis
              │
              ▼
8. Layout Result Assembly
```

Implementation có thể gộp một số bước.

Public semantics phải giữ nguyên.

---

# 10. Stage 1 — Input Validation

Layout kiểm tra:

* Region references hợp lệ
* Geometry hợp lệ
* Region Type hợp lệ
* Direction metadata hợp lệ khi cần
* duplicate identity không tồn tại
* referenced image/version nhất quán
* Layout Profile hợp lệ

Layout không tự sửa Region geometry sai.

---

# 11. Stage 2 — Spatial Analysis

Spatial Analysis đánh giá quan hệ hình học giữa các entity.

Có thể xem xét:

* position
* size
* overlap
* containment
* alignment
* proximity
* orientation

Kết quả là evidence cho grouping và hierarchy.

---

# 12. Stage 3 — Region Grouping

Region Grouping xác định các Region có nên thuộc cùng một Block hoặc Container hay không.

Tín hiệu có thể gồm:

* distance
* alignment
* Region Type
* geometry compatibility
* direction compatibility
* common enclosure
* visual continuity

Grouping không dựa vào Translation hoặc semantic text understanding.

---

# 13. Stage 4 — Container Detection

Container đại diện cho visual enclosure hoặc grouping structure.

Ví dụ:

* Speech Bubble
* Narration Box
* UI Window
* Tooltip

Container có thể được suy ra từ:

* Detection hints
* geometry
* visual grouping
* Region relationships

---

# 14. Container Structure

Container có thể chứa:

```text
Container
├── Block
├── Region
└── Container
```

Không bắt buộc mọi Container phải có Block.

Container hierarchy phải acyclic.

---

# 15. Stage 5 — Panel Detection

Panel đại diện cho một subdivision cấp cao của Page.

Panel Detection có thể sử dụng:

* border
* whitespace
* enclosure
* geometry
* visual separation
* provider/layout hints

Panel không phải lúc nào cũng tồn tại.

Ví dụ:

* Webtoon dài
* Plain document
* full-page illustration

có thể không có traditional comic panels.

---

# 16. Panel Semantics

Panel chỉ biểu diễn structural visual scope.

Panel không định nghĩa:

* page reading order
* speaker order
* dialogue order

Những phần đó thuộc `READING_ORDER.md`.

---

# 17. Stage 6 — Hierarchy Construction

Layout Tree có thể có dạng:

```text
Page
├── Panel
│   ├── Container
│   │   ├── Block
│   │   │   ├── Region
│   │   │   └── Region
│   │   └── Region
│   └── Container
└── Panel
```

Không phải mọi node type đều bắt buộc ở mọi Page.

---

# 18. Hierarchy Rules

Một node:

* có tối đa một direct parent trong tree
* có thể có nhiều child
* không được tạo cycle
* phải tham chiếu entity tồn tại
* phải giữ parent-child relationship nhất quán

Layout Tree không được tạo duplicate logical entity chỉ để phù hợp hierarchy.

---

# 19. Page Model

Page là root của Layout Tree.

Page có thể chứa:

* Geometry
* Panel references
* Container references
* Block references
* Metadata
* Statistics

Page không sở hữu Character/Word semantics.

---

# 20. Panel Model

Panel có thể chứa:

* Panel ID
* Geometry
* Container references
* Block references
* Metadata

Panel có thể lồng nhau nếu Layout Profile cho phép.

Nested panel relationship phải explicit.

---

# 21. Container Model

Container có thể chứa:

* Container ID
* Geometry
* Region references
* Block references
* Child Container references
* Metadata

Container không được thay đổi Region identity.

---

# 22. Block Model

Block là grouping trung gian giữa Container và Region.

Có thể chứa:

* Block ID
* Geometry
* Region references
* Metadata

Block thường biểu diễn:

* paragraph-like grouping
* title grouping
* caption grouping
* dialogue cluster

---

# 23. Region Ownership

Layout sử dụng Region do Detection sở hữu.

Layout không redefine:

* Region ID
* Bounding Box semantics
* Polygon semantics
* Detection Confidence
* Region Type semantics

Layout chỉ tổ chức Region trong cấu trúc Page.

---

# 24. Spatial Relationship Graph

Ngoài Layout Tree, Layout tạo Graph cho quan hệ không gian.

Ví dụ:

```text
Region A
   │ inside
   ▼
Container X
   │ belongs_to
   ▼
Panel 2
```

Graph phục vụ queries mà tree không biểu diễn thuận tiện.

---

# 25. Standard Spatial Relationships

Layout có thể chuẩn hóa:

```text
contains
inside
overlaps
intersects
adjacent
above
below
left_of
right_of
aligned
touching
```

Các relation này là semantic spatial relations của Layout.

Reading Order có thể sử dụng chúng làm evidence.

---

# 26. Direction of Relationships

Một số relation là directional.

Ví dụ:

```text
A above B
```

khác với:

```text
B above A
```

Trong khi:

```text
A overlaps B
```

có thể được xem là symmetric tùy contract.

Relation semantics phải rõ ràng và versioned.

---

# 27. Relationship Confidence

Nếu Layout cần confidence cho spatial relation, confidence đó thuộc Layout.

Nó không đồng nghĩa:

* Detection Confidence
* Recognition Confidence
* Direction Confidence
* Reading Confidence

Quality có thể aggregate ở bước sau.

---

# 28. Layout Geometry

Layout có thể tạo geometry cho:

* Panel
* Container
* Block

Geometry này phải dựa trên Region/Image coordinate space đã có.

Layout không được tạo coordinate system riêng không thể map ngược.

---

# 29. Geometry Aggregation

Geometry của container-level entity có thể được tạo từ child geometry.

Ví dụ:

```text
Block Geometry
    = envelope of child Regions
```

hoặc strategy phù hợp hơn.

Aggregation algorithm phải deterministic theo Layout Profile/Strategy.

---

# 30. Layout Result Model

```text
Layout Result
├── Metadata
├── Page
├── Panels[]
├── Containers[]
├── Blocks[]
├── Layout Tree
├── Spatial Relationship Graph
├── Confidence / Warnings
├── Diagnostics
└── Statistics
```

Layout Result phải provider-neutral.

---

# 31. Layout Metadata

Metadata có thể chứa:

* Layout Strategy ID
* Layout Strategy Version
* Layout Profile Version
* source image reference
* Detection Result reference
* Recognition Result reference
* Direction Result reference
* creation time

---

# 32. Layout Profile

Layout Profile có thể ảnh hưởng:

* panel detection behavior
* grouping thresholds
* container rules
* block grouping
* overlap handling
* hierarchy strategy
* document type hints

Profile phải versioned nếu thay đổi semantics.

---

# 33. Document Type Hints

Layout có thể nhận hint:

* Comic
* Manga
* Webtoon
* Novel
* Document
* UI

Hint không phải authoritative truth.

Layout vẫn phải dựa trên actual spatial evidence.

---

# 34. Comic Layout

Comic-like layout có thể chứa:

* Panel
* Bubble Container
* Narration Container
* SFX Region
* Background Text

Layout chỉ tổ chức chúng.

Reading Order mới quyết định sequence.

---

# 35. Webtoon Layout

Webtoon có thể:

* thiếu closed panels
* có vertical clusters
* có large whitespace
* có content flow dài

Layout phải cho phép structure không phụ thuộc traditional panel model.

---

# 36. Document Layout

Document/novel page có thể dùng:

* Column
* Block
* Header/Footer-like Container
* Paragraph grouping

Layout architecture phải đủ generic để không chỉ phục vụ comic.

---

# 37. Mixed Layout

Một Page có thể chứa:

* comic panel
* UI text
* advertisement
* title
* vertical text
* background sign

Layout Tree và Graph phải giữ được các entity khác loại mà không ép vào một hierarchy giả tạo.

---

# 38. Recognition Integration

Recognition cung cấp:

* Region text structure
* Character/Line geometry
* language/script metadata
* recognition-local grouping

Layout có thể sử dụng các signal này.

Nhưng Layout không thay đổi recognized text.

---

# 39. Text Direction Integration

Text Direction cung cấp:

* Writing Mode
* Line Direction
* Paragraph Direction
* Rotation

Layout có thể dùng Direction Metadata để:

* grouping
* block orientation
* container consistency

Direction semantics vẫn thuộc `TEXT_DIRECTION.md`.

---

# 40. Postprocessing Integration

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

thành `OCR Document`.

Layout không tự tạo canonical OCR Document.

---

# 41. Reading Order Integration

Reading Order sử dụng:

```text
OCR Document
+
Layout Tree
+
Spatial Relationship Graph
+
Direction Metadata
```

để tạo precedence và sequence.

Layout chỉ định nghĩa:

```text
structure
```

Reading Order định nghĩa:

```text
order
```

---

# 42. Layout vs Reading Order

Đây là boundary bắt buộc.

Layout có thể nói:

```text
A is above B
A and B belong to Panel 1
```

Nhưng không nói:

```text
A must be read before B
```

Quyết định đó thuộc Reading Order.

---

# 43. Layout vs Detection

Detection tạo Region và geometry cơ bản.

Layout tạo higher-level structure.

Conceptually:

```text
Detection
    → Region

Layout
    → Panel / Container / Block relationships around Regions
```

---

# 44. Layout vs Recognition

Recognition tạo textual hierarchy bên trong Region.

Layout tạo visual hierarchy trên Page.

Hai hierarchy có thể liên kết nhưng không thay thế nhau.

Ví dụ:

```text
Visual:
Page → Panel → Bubble → Region

Text:
Region → Paragraph → Line → Word → Character
```

---

# 45. Immutability

Published Layout Result phải immutable.

Nếu layout analysis chạy lại:

```text
new Layout Result revision
```

phải được tạo.

Không silent-mutate Layout Tree đã publish.

---

# 46. Entity Identity

Panel, Container và Block phải có identity riêng.

Identity phải stable trong cùng Layout Result revision.

Downstream không nên dựa vào array index như identity.

---

# 47. Determinism

Cùng:

```text
Detection semantic input
+
Recognition semantic input
+
Direction semantic input
+
Layout Profile
+
Layout Strategy Version
```

nên tạo structurally equivalent Layout Result.

Provider nondeterminism nếu có phải được ghi lại.

---

# 48. Provider Independence

Một Layout Provider hoặc OCR Provider có thể trả:

* panel hints
* block hints
* relationship hints

Nhưng provider-native structure phải được normalize về CRAI Layout Contract.

Downstream không phụ thuộc provider-specific schema.

---

# 49. Incremental Layout Semantics

Layout có thể hỗ trợ recomputation theo scope.

Ví dụ:

* Region mới xuất hiện
* một Panel thay đổi
* scroll thêm một phần Webtoon

Semantic rule:

```text
unchanged scope
    → preserve identity/structure when compatible

changed scope
    → recompute affected hierarchy
```

Layout sở hữu incremental recomputation semantics. Runtime sở hữu scheduling, execution authority và execution mechanics của recomputation work.

---

# 50. Layout Compatibility

Layout Result có thể không còn compatible khi:

* Region set thay đổi
* Region geometry thay đổi
* Region Type thay đổi
* Direction Result thay đổi đáng kể
* Layout Profile thay đổi
* Layout Strategy Version thay đổi

Layout chỉ định nghĩa semantic compatibility.

Cache lifecycle thuộc Runtime Cache Policy.

---

# 51. Diagnostics

Layout Diagnostics có thể chứa:

* ambiguous grouping
* conflicting containment
* invalid parent-child relation
* overlap warning
* panel detection uncertainty
* orphan Region
* hierarchy repair suggestion

Diagnostics không được tự mutate input.

---

# 52. Runtime Integration

Layout không sở hữu:

* ExecutionScope hoặc ExecutionRevision
* WorkItem hoặc Attempt lifecycle
* queue
* execution state
* Runtime Retry Policy hoặc retry budget
* cancellation authority
* Scheduler behavior
* execution authority
* stale-result rejection
* Runtime Artifact publication

Runtime sở hữu execution mechanics và execution authority trên.

Layout chỉ tạo:

* semantic Layout Result candidate
* Layout-specific semantic failure information
* semantic compatibility information
* execution hoặc resource hints khi contract cho phép

Khi Layout execution hoàn thành, Runtime Control quyết định completion còn execution authority hay phải bị reject vì stale hoặc cancelled trước Runtime publication.

Layout không quyết định downstream Business continuation.

---

# 53. Retry and Recovery Integration

Layout có thể cung cấp:

* low-confidence layout
* invalid hierarchy
* ambiguous grouping
* insufficient evidence
* Retry hint
* Recovery recommendation

Những thông tin này có thể được Quality hoặc authoritative policy owner sử dụng làm evidence.

Layout không tự schedule Retry và không tự chọn alternative execution.

Ownership:

```text
Same-work Retry
    → Runtime Retry Policy

Alternative execution / Fallback
    → AI Routing / Recovery

Downstream Business continuation
    → Business Pipeline Orchestration

Scheduling
    → Runtime Scheduler
```

---

# 54. Cache Integration

Layout có thể xác định semantic invalidation.

Ví dụ:

* Region geometry changed
* Region set changed
* Layout Profile changed
* Strategy Version changed

Global cache storage, retention và eviction thuộc Runtime Cache Policy.

---

# 55. Event Integration

Layout có thể tạo semantic facts như:

```text
LayoutCompleted
LayoutAmbiguous
LayoutFailed
```

Ý nghĩa semantic thuộc Layout.

Transport/envelope thuộc Event Bus.

---

# 56. Error Integration

Layout-specific semantic errors có thể gồm:

* InvalidRegionReference
* InvalidGeometry
* InvalidHierarchy
* LayoutResultInvalid
* ParentChildCycle
* StrategyUnavailable

Layout sở hữu semantic meaning của các Layout-specific errors.

Các lỗi này phải map vào Runtime Error Model khi crossing Runtime execution boundary; Runtime không redefine Layout semantics.

---

# 57. Observability Integration

Useful measurements có thể gồm:

* Panel count
* Container count
* Block count
* hierarchy depth
* orphan Region count
* ambiguous relation count
* layout duration
* strategy identity

Runtime Observability sở hữu execution correlation; telemetry transport và lifecycle thuộc Infrastructure.

---

# 58. Architecture Invariants

Layout Analysis phải luôn đảm bảo:

1. Không thay đổi source image.

2. Không thay đổi Region identity.

3. Không thay đổi Detection Geometry âm thầm.

4. Không thay đổi recognized text.

5. Không thực hiện Translation.

6. Không sở hữu final Reading Order.

7. Layout Tree phải acyclic.

8. Spatial Relationship Graph phải tham chiếu entity hợp lệ.

9. Panel, Container và Block phải có identity riêng.

10. Layout Result phải provider-neutral.

11. Provider-native structure không crossing public boundary.

12. Visual hierarchy và textual hierarchy không được trộn thành một model duy nhất.

13. Layout chỉ định nghĩa structure, không định nghĩa precedence.

14. Published Layout Result phải immutable.

15. Rerun tạo revision mới.

16. Layout không sở hữu Runtime scheduling.

17. Layout không sở hữu Runtime Retry Policy hoặc Retry budget.

18. Layout không sở hữu cancellation authority.

19. Layout không sở hữu Runtime execution authority hoặc stale-result decision.

20. Layout không sở hữu Runtime Artifact publication.

21. Layout không sở hữu downstream Business continuation.

22. Layout không sở hữu global cache lifecycle.

23. Layout semantic compatibility không bị Runtime Cache Policy redefine.

24. Layout không tự chọn Provider hoặc Fallback.

25. Reading Order phải có thể sử dụng Layout Result mà không cần suy luận lại toàn bộ spatial structure.

26. Incremental recomputation phải giữ stable identity và structure cho unchanged compatible scopes khi có thể.

---

# 59. Recommended MVP Layout

MVP nên giữ đơn giản:

```text
Detection Result
      ↓
Spatial Analysis
      ↓
Basic Region Grouping
      ↓
Basic Container Detection
      ↓
Basic Panel Detection
      ↓
Layout Tree
      ↓
Spatial Relationship Graph
      ↓
Layout Result
```

MVP nên hỗ trợ:

* Page
* Panel
* Container
* Block
* Region references
* basic parent-child hierarchy
* above/below/left/right
* contains/inside
* overlaps
* provider-neutral Layout Result

Không cần ngay:

* AI semantic layout
* complex multi-page hierarchy
* advanced speaker-aware grouping
* learned layout strategy
* distributed layout processing

---

# 60. Ownership References

| Concern | Owner |
| --- | --- |
| Region | `DETECTION.md` |
| Region Geometry | `DETECTION.md` |
| Recognition Text Model | `RECOGNITION.md` |
| Writing Direction | `TEXT_DIRECTION.md` |
| Panel | `LAYOUT.md` |
| Container | `LAYOUT.md` |
| Block | `LAYOUT.md` |
| Layout Tree | `LAYOUT.md` |
| Spatial Relationship Graph | `LAYOUT.md` |
| Layout semantic compatibility | `LAYOUT.md` |
| OCR Document | `POSTPROCESS.md` |
| Reading Order | `READING_ORDER.md` |
| Same-work Retry | Runtime Retry Policy |
| Alternative execution / Fallback | AI Routing / Recovery |
| Cancellation Authority | Runtime Control / Cancellation |
| Scheduling | Runtime Scheduler |
| Execution Authority / stale-result rejection | Runtime Control |
| Runtime Artifact publication | Runtime Artifact boundary |
| Business continuation | Business Pipeline Orchestration |
| Cache Lifecycle | Runtime Cache Policy |
| Event Transport | Event Bus |
| Execution Error Normalization | Runtime Error Model |
| Telemetry Transport | Infrastructure |

---

# 61. Summary

Layout Analysis chuyển:

```text
Detection / Recognition / Direction
```

thành:

```text
Layout Result
```

gồm:

```text
Page
+
Panel
+
Container
+
Block
+
Layout Tree
+
Spatial Relationship Graph
```

Boundary cốt lõi:

```text
Detection
    → Where are the Regions?

Recognition
    → What text is inside them?

Text Direction
    → How is the text written?

Layout
    → How are the visual entities organized?

Reading Order
    → In what order should they be read?
```

Nguyên tắc quan trọng nhất:

```text
Layout owns structure.

Reading Order owns precedence.

Recognition owns text.

Runtime owns execution mechanics and execution authority.
```
