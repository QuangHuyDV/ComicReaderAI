# Reading Order

> **Status:** Draft
> **Version:** 1.2.0
> **Layer:** OCR Architecture
> **Depends On:** Detection, Recognition, Text Direction, Layout Analysis, OCR Postprocessing
> **Next Layer:** Text Processing

---

# 1. Purpose

Reading Order xác định thứ tự đọc hợp lý của các entity trong `OCR Document`.

Nếu:

```text
Detection
    → "Text nằm ở đâu?"

Recognition
    → "Text là gì?"

Text Direction
    → "Text được viết theo hướng nào?"

Layout Analysis
    → "Các vùng được tổ chức như thế nào?"
```

thì Reading Order trả lời:

```text
"Các entity này phải được đọc theo thứ tự nào?"
```

Reading Order là ranh giới cuối giữa cấu trúc hình học của OCR và dữ liệu nguồn có thứ tự để chuyển sang Text Processing.

---

# 2. Scope

Reading Order chịu trách nhiệm:

* xác định thứ tự đọc toàn trang
* xác định thứ tự cục bộ trong Panel
* xác định thứ tự trong Container
* xác định thứ tự trong Block
* tạo quan hệ trước-sau
* xây dựng Reading Order Graph
* giải quyết xung đột thứ tự
* tạo Main Reading Sequence
* tạo Auxiliary Sequence
* đánh giá Reading Confidence
* giữ mapping về entity gốc

Reading Order không chịu trách nhiệm:

* OCR
* thay đổi Recognition Result
* thay đổi Geometry
* thay đổi Layout Tree
* thay đổi Text Direction
* Translation
* semantic text reconstruction
* Runtime scheduling
* Runtime same-work retry
* Runtime cancellation
* Event Bus behavior
* global cache lifecycle

---

# 3. Terminology

## Reading Order

Thứ tự logic mà các entity nên được đọc.

---

## Reading Sequence

Danh sách tuyến tính của các entity đã được sắp xếp.

Ví dụ:

```text
Region A
    ↓
Region B
    ↓
Region C
```

---

## Reading Order Graph

Đồ thị biểu diễn quan hệ thứ tự.

```text
A → B
A → C
B → D
C → D
```

Node đại diện cho entity.

Edge đại diện cho:

```text
precedes
```

---

## Precedence Relationship

Quan hệ:

```text
A precedes B
```

nghĩa là A nên được đọc trước B trong một scope cụ thể.

---

## Local Order

Thứ tự trong một phạm vi nhỏ.

Ví dụ:

* Panel
* Container
* Block
* Region

---

## Global Order

Thứ tự của các entity cấp cao trên toàn Page.

---

## Main Sequence

Chuỗi nội dung chính phục vụ Text Processing.

Ví dụ:

* Speech Bubble
* Narration
* important source text

---

## Auxiliary Sequence

Chuỗi nội dung phụ.

Ví dụ:

* SFX
* UI Text
* Watermark
* Advertisement

---

## Reading Profile

Cấu hình ảnh hưởng tới chiến lược Reading Order.

---

## Reading Strategy

Thuật toán cụ thể dùng để tạo Reading Order Result.

---

# 4. Goals

Reading Order phải:

* deterministic
* explainable
* provider-neutral
* stable
* hierarchy-aware
* geometry-aware
* direction-aware
* hỗ trợ nhiều loại tài liệu
* giữ identity của entity gốc
* không làm thay đổi dữ liệu OCR

---

# 5. Non-Goals

Reading Order không thực hiện:

* Character Recognition
* Image Processing
* Grammar Analysis
* Translation
* semantic rewriting
* speaker identification
* font layout
* rendering

---

# 6. Architecture Position

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
Layout Analysis
   ↓
OCR Postprocessing
   ↓
OCR Document
   ↓
Reading Order
   ↓
Ordered Source Data
   ↓
Text Processing
   ↓
Translation
```

Reading Order hoạt động trên dữ liệu OCR đã được chuẩn hóa.

Nó không cần truy cập trực tiếp OCR Provider.

---

# 7. Core Input Model

Input chính:

```text
OCR Document
+
Layout Tree
+
Spatial Relationship Graph
+
Direction Metadata
+
Reading Profile
```

Các thông tin có thể được sử dụng:

* Region Type
* Geometry
* Writing Mode
* Line Direction
* Paragraph Direction
* Layout hierarchy
* Detection metadata
* Recognition metadata
* source/document type
* user preference

---

# 8. Core Output Model

Reading Order tạo:

```text
Reading Order Result
```

bao gồm:

```text
Reading Order Result
├── Metadata
├── Reading Order Graph
├── Main Sequence
├── Auxiliary Sequence
├── Local Sequences
├── Confidence
└── Diagnostics
```

Mọi entity trong output phải tham chiếu entity gốc bằng ID.

Reading Order không clone dữ liệu OCR thành một model độc lập mất lineage.

---

# 9. Canonical Reading Order Pipeline

```text
OCR Document
     ↓
1. Input Validation
     ↓
2. Reading Context Resolution
     ↓
3. Hierarchy Analysis
     ↓
4. Candidate Generation
     ↓
5. Candidate Scoring
     ↓
6. Graph Construction
     ↓
7. Conflict Resolution
     ↓
8. Cycle Detection
     ↓
9. Linearization
     ↓
10. Sequence Validation
     ↓
Reading Order Result
```

---

# 10. Stage 1 — Input Validation

Kiểm tra:

* OCR Document hợp lệ
* Layout Tree hợp lệ
* Direction Metadata tồn tại khi cần
* entity reference hợp lệ
* Geometry hợp lệ
* Reading Profile hợp lệ

Reading Order không tự sửa input bị hỏng.

---

# 11. Stage 2 — Reading Context Resolution

Reading Context được tạo từ:

* document type
* page type
* Reading Profile
* layout hierarchy
* writing mode
* direction metadata
* Region Type
* user preference

Context không được dựa vào một tín hiệu duy nhất.

Ví dụ:

```text
Language Code
```

không đủ để quyết định toàn bộ Page Reading Mode.

---

# 12. Stage 3 — Hierarchy Analysis

Reading Order phải ưu tiên Layout Tree khi hierarchy đáng tin cậy.

Ví dụ:

```text
Page
 └── Panel
      └── Container
           └── Block
                └── Region
                     └── Paragraph
                          └── Line
```

Chiến lược mặc định:

1. sắp xếp Panel
2. sắp xếp Container trong Panel
3. sắp xếp Block trong Container
4. sắp xếp Region trong Block
5. sắp xếp Paragraph và Line trong Region

Không nên flatten toàn bộ Page rồi sort mọi Region cùng lúc nếu hierarchy đã tồn tại.

---

# 13. Stage 4 — Candidate Generation

Reading Order tạo các candidate relationship:

```text
A precedes B
```

Candidate có thể chứa:

```text
sourceEntityId
targetEntityId
scope
ruleId
weight
confidence
evidence
```

Candidate nên được tạo trong scope phù hợp.

Ví dụ:

* cùng Parent
* cùng Row
* cùng Column
* cùng Panel
* spatial neighbors

Không cần so sánh tất cả entity với tất cả entity.

---

# 14. Stage 5 — Candidate Scoring

Candidate có thể được đánh giá bằng:

* hierarchy compatibility
* Reading Mode
* vertical position
* horizontal position
* row/column relationship
* spatial distance
* alignment
* Region Type
* Direction metadata
* provider hints

Cách scoring thuộc `Reading Strategy`.

Contract không hard-code một công thức duy nhất.

---

# 15. Rule Priority

Các rule phải có thứ tự ưu tiên rõ ràng.

Một hierarchy khuyến nghị:

```text
1. Parent-Child Constraint
2. Explicit Layout Relationship
3. Reading Mode
4. Row / Column Structure
5. Direction Metadata
6. Spatial Relationship
7. Region Type Preference
8. Provider Hint
9. Stable Fallback
```

Rule ưu tiên thấp không được phá hard constraint của rule ưu tiên cao hơn.

---

# 16. Stage 6 — Graph Construction

Reading Order Graph chứa:

```text
Node
    = Ordered Entity

Edge
    = Precedence Relationship
```

Mỗi Edge có thể chứa:

```text
weight
confidence
ruleId
evidence
```

Graph là representation chính trước khi linearization.

---

# 17. Stage 7 — Conflict Resolution

Conflict xảy ra khi:

```text
A → B
B → A
```

hoặc:

```text
A → B
B → C
C → A
```

Resolution có thể dựa trên:

* rule priority
* confidence
* hierarchy
* direction
* fallback strategy

Không được âm thầm bỏ qua conflict.

Quyết định phải có thể xuất hiện trong Diagnostics.

---

# 18. Stage 8 — Cycle Detection

Graph phải không còn cycle trước khi linearization.

Nếu có cycle:

1. xác định edge xung đột
2. kiểm tra priority
3. so sánh confidence
4. loại hoặc giảm edge phù hợp
5. kiểm tra lại graph

Nếu không thể giải quyết chắc chắn:

```text
scope = Ambiguous
```

Ambiguous là quality information của Reading Order.

Không đồng nghĩa Runtime failure.

---

# 19. Stage 9 — Linearization

Khi graph trở thành DAG, Reading Order có thể tạo sequence tuyến tính.

Topological ordering là một strategy phù hợp.

Nếu có nhiều node cùng hợp lệ, tie-breaker phải deterministic.

Có thể dùng:

* Reading Mode
* Geometry
* stable source order
* stable entity identity

Không được phụ thuộc:

* thread completion order
* map iteration order
* provider response timing

---

# 20. Stage 10 — Sequence Validation

Sequence phải đảm bảo:

* mọi Entity ID tồn tại
* không duplicate trái contract
* không thiếu Main Entity
* Previous/Next reference hợp lệ
* Parent scope hợp lệ
* hard constraints không bị vi phạm
* entity bị exclude phải có reason

---

# 21. Reading Hierarchy

Reading Order phải giữ logic global/local.

```text
Page
    ↓
Panel Sequence
    ↓
Container Sequence
    ↓
Block Sequence
    ↓
Region Sequence
    ↓
Paragraph / Line Sequence
```

Điều này giúp giảm conflict toàn cục.

---

# 22. Global Order

Global Order tập trung vào entity cấp cao.

Ví dụ:

```text
Panel 1
   ↓
Panel 2
   ↓
Panel 3
```

---

# 23. Local Order

Local Order hoạt động trong một parent scope.

Ví dụ:

```text
Panel 2

Bubble A
   ↓
Bubble B
   ↓
Narration C
```

Local ordering phải giữ parent identity.

---

# 24. Reading Modes

Reading Order phải hỗ trợ tối thiểu:

```text
LeftToRight
RightToLeft
TopToBottom
VerticalColumns
Mixed
Unknown
```

Reading Mode có thể tồn tại ở:

* Document
* Page
* Panel
* Container

Scope nhỏ hơn có thể override scope lớn hơn khi metadata đủ tin cậy.

---

# 25. Left-to-Right

Quy tắc cơ bản:

1. ưu tiên entity phía trên
2. trong cùng row, ưu tiên bên trái
3. chuyển xuống row tiếp theo

Ví dụ:

```text
A B
C D
```

→

```text
A → B → C → D
```

---

# 26. Right-to-Left

Quy tắc cơ bản:

1. ưu tiên entity phía trên
2. trong cùng row, ưu tiên bên phải
3. chuyển xuống row tiếp theo

Ví dụ:

```text
A B
C D
```

→

```text
B → A → D → C
```

Page Reading Order không đồng nghĩa Text Direction bên trong Region.

---

# 27. Top-to-Bottom

Phù hợp cho:

* Webtoon
* long-scroll content
* vertical feed-like pages

Tín hiệu chính:

* vertical position
* vertical distance
* local grouping

Horizontal relation chỉ nên là tín hiệu cục bộ.

---

# 28. Vertical Column Reading

Với vertical text cần tách:

```text
IntraColumnOrder
```

và

```text
InterColumnOrder
```

Ví dụ:

```text
characters:
top → bottom

columns:
right → left
```

Không được biểu diễn cả hai bằng một Direction đơn giản.

---

# 29. Mixed Reading Mode

Một Page có thể chứa nhiều mode.

Ví dụ:

* manga dialogue RTL
* UI LTR
* vertical narration
* rotated SFX

Trong Mixed Mode:

* ưu tiên hierarchy
* xác định mode theo scope
* không ép toàn Page về một direction
* giữ Unknown nếu chưa đủ bằng chứng

---

# 30. Panel Ordering

Panel Ordering sử dụng:

* geometry
* layout hierarchy
* rows/columns
* whitespace
* border
* overlap
* nesting
* Reading Mode

Panel lớn không mặc định đọc trước.

Nested Panel phải tôn trọng Parent-Child Relationship.

---

# 31. Container Ordering

Các Container như:

* Speech Bubble
* Narration Box
* UI Container

được sắp xếp trong parent scope.

Tín hiệu gồm:

* position
* type
* hierarchy
* direction
* overlap
* distance
* alignment

Region Type chỉ là một signal.

Không phải hard truth.

---

# 32. Bubble Ordering

Bubble Ordering có thể dùng:

* top-first
* direction-aware
* cluster-aware
* geometry-aware
* tail-assisted

Bubble tail không đủ để xác định reading order độc lập.

---

# 33. Narration

Narration có thể:

* nằm đầu Panel
* cuối Panel
* xen giữa dialogue
* nằm ở Page scope

Không áp đặt một priority cứng rằng Narration luôn trước hoặc luôn sau Dialogue.

---

# 34. SFX

SFX thường không thuộc dialogue sequence chính.

Reading Profile có thể:

```text
Exclude
IncludeInline
IncludeAfterDialogue
IncludeByPosition
```

SFX không được xóa khỏi OCR Document chỉ vì không thuộc Main Sequence.

---

# 35. UI, Watermark and Advertisement

Mặc định:

```text
Main Sequence
    → excluded

Auxiliary Sequence
    → retained
```

Reading Profile có thể thay đổi behavior.

Exclusion khỏi Reading Sequence không đồng nghĩa xóa entity nguồn.

---

# 36. Spatial Relationships

Reading Order có thể sử dụng các relation đã được Layout định nghĩa:

* above
* below
* left_of
* right_of
* inside
* contains
* overlaps
* adjacent
* aligned
* touching

Reading Order chỉ dùng các relation này làm evidence.

Spatial semantics vẫn thuộc `LAYOUT.md`.

---

# 37. Row and Column Grouping

Reading Strategy có thể nhóm entity thành:

```text
Row
Column
Cluster
```

Ngưỡng grouping phải thuộc Reading Profile hoặc Strategy.

Không hard-code trong public contract.

---

# 38. Provider Hints

Một Provider có thể cung cấp order hint.

Provider hint chỉ được dùng như:

* candidate
* tie-breaker
* fallback signal
* validation signal

Provider sequence không phải authoritative truth.

Hint phải được map về CRAI Entity ID.

---

# 39. Reading Strategies

Reading Order hỗ trợ Strategy abstraction.

Ví dụ:

* Geometric Strategy
* Layout Strategy
* Graph Strategy
* Manga Strategy
* Webtoon Strategy
* Document Strategy
* Hybrid Strategy
* AI-assisted Strategy

Mọi Strategy phải cùng tạo:

```text
Reading Order Result
```

---

# 40. Hybrid Strategy

Hybrid Strategy có thể kết hợp:

* Geometry
* Layout Tree
* Text Direction
* Region Type
* Provider Hint
* AI-generated hint

AI hint không được phá hard constraint nếu không có evidence đủ mạnh.

---

# 41. Partial Ordering

Không phải mọi entity đều cần nằm trong một sequence tuyệt đối.

Ví dụ:

* SFX độc lập
* Watermark
* Background annotation

Graph có thể giữ partial order.

Main Sequence chỉ linearize nội dung cần cho Text Processing.

---

# 42. Main Sequence

Main Sequence chứa nội dung chính cần xử lý tiếp.

Ví dụ:

* Speech Bubble
* Narration
* important Background Text

Selection phụ thuộc Reading Profile.

---

# 43. Auxiliary Sequence

Auxiliary Sequence giữ nội dung không thuộc luồng chính.

Ví dụ:

* SFX
* UI Text
* Watermark
* Advertisement

Entity vẫn phải giữ mapping tới OCR Document.

---

# 44. Unknown and Ambiguous Order

Khi thiếu evidence:

* giữ Confidence thấp
* dùng stable fallback
* đánh dấu Ambiguous
* ghi Diagnostics

Không được giả tạo certainty.

Fallback có thể là:

```text
TopToBottom + LeftToRight
TopToBottom + RightToLeft
Stable Source Order
Provider Hint
```

Fallback phải được ghi trong metadata.

---

# 45. Cross-Panel Relationships

Mặc định:

```text
Panel order
    ↓
local order inside each panel
```

Cross-panel edge chỉ nên xuất hiện khi:

* Layout evidence đủ mạnh
* Parent scope cho phép
* không phá primary panel order

---

# 46. Double-Page Spread

Double-page spread cần xác định page-level order trước.

Ví dụ:

Manga:

```text
Right Page
    ↓
Left Page
```

LTR comic:

```text
Left Page
    ↓
Right Page
```

Sau đó mới xử lý local order trong từng Page.

---

# 47. Webtoon

Webtoon ưu tiên:

```text
Vertical Position
    ↓
Vertical Clustering
    ↓
Local Horizontal Relationship
    ↓
Container Order
```

Không áp dụng row-based comic strategy một cách máy móc.

---

# 48. Novel / Plain Text

Với document/novel:

* Column structure quan trọng hơn Panel
* Paragraph order quan trọng hơn Bubble
* Header/Footer có thể nằm ngoài Main Sequence
* Footnote có thể cần sequence/reference riêng

Reading Strategy phải thích ứng theo document type.

---

# 49. Mixed-Language Content

Language không quyết định toàn bộ page order.

Ví dụ cùng Page có thể có:

* Chinese vertical
* English horizontal
* UI LTR
* rotated label

Page-level order và local writing direction phải được tách riêng.

---

# 50. Reading Confidence

Reading Confidence có thể tồn tại ở:

* Edge
* Local Sequence
* Panel Order
* Global Sequence

Global Confidence không nhất thiết bằng trung bình Edge Confidence.

Quality Assessment có thể sử dụng Reading Confidence trong một evaluation scope chạy sau Reading Order hoặc trong một Quality Profile mở rộng có Reading Order làm input.

Quality Report đánh giá OCR Document trước Reading Order không được giả định đã có Reading Confidence.

---

# 51. Confidence Factors

Có thể gồm:

* Layout clarity
* Direction confidence
* number of conflicts
* fallback usage
* geometric separation
* rule agreement
* provider agreement

Confidence computation thuộc Strategy/Profile.

---

# 52. Explainability

Một ordering decision quan trọng nên có thể giải thích.

Ví dụ:

```text
Bubble B precedes Bubble A

Evidence:
- Page Mode = RTL
- same Row
- B is right of A
- confidence = high
```

Explainability có thể được giữ trong Diagnostics thay vì lightweight production result.

---

# 53. Duplicate Handling

Reading Order không xóa OCR entity.

Nếu có duplicate candidate, nó có thể:

* chọn một entity vào Main Sequence
* đánh dấu entity khác là Auxiliary
* đánh dấu Excluded
* ghi diagnostics

Việc sửa hoặc xóa source OCR data không thuộc Reading Order.

---

# 54. Manual Override

Reading Order có thể hỗ trợ override semantic như:

* reorder entity
* exclude entity
* include entity
* lock precedence relationship

Override phải tồn tại tách khỏi immutable OCR result.

Persistence và user-preference lifecycle của override thuộc authoritative Business/Persistence owner tương ứng. Runtime chỉ thực thi recomputation work khi được yêu cầu.

---

# 55. Override Precedence

Một precedence khuyến nghị:

```text
User Override
    ↓
Locked Project Rule
    ↓
Document Profile
    ↓
Reading Strategy
    ↓
Provider Hint
    ↓
Fallback
```

User Override không bị tự động mất khi Reading Order chạy lại nếu input identity vẫn tương thích.

---

# 56. Incremental Semantics

Reading Order có thể hỗ trợ recomputation theo scope.

Ví dụ:

* thêm Region mới cuối Webtoon
* thay đổi một Panel
* thay đổi một Container

Semantic requirement:

```text
unchanged scope
    → preserve order when possible

changed scope
    → recompute only affected order
```

Reading Order sở hữu incremental recomputation semantics. Runtime sở hữu scheduling, execution authority và execution mechanics của recomputation work.

---

# 57. Sequence Stability

Ordering không nên thay đổi không cần thiết khi input cũ không đổi.

Điều này quan trọng cho:

* overlay
* reading history
* translation mapping
* user correction
* incremental OCR

Stability là semantic requirement của Reading Order.

---

# 58. Reading Order Compatibility

Result phải lưu đủ version identity để xác định compatibility.

Ví dụ:

* Contract Version
* Strategy ID
* Strategy Version
* Reading Profile Version
* OCR Document Version

Breaking semantic changes phải versioned.

---

# 59. Diagnostics

Diagnostics có thể chứa:

* rule applied
* candidate rejected
* conflict
* cycle resolution
* fallback
* excluded entity
* ambiguous scope
* confidence breakdown

Diagnostics không thay thế Reading Order Result.

---

# 60. Runtime Integration

Reading Order chỉ cung cấp semantic output và semantic recomputation requirements.

Runtime sở hữu:

* ExecutionScope và ExecutionRevision correlation
* WorkItem / Attempt execution mechanics
* scheduling
* cancellation execution
* same-work Retry mechanics
* queue
* execution state
* execution authority
* stale-result rejection

Reading Order không tự:

* schedule lại chính nó
* tạo Attempt
* quyết định Retry budget
* quyết định cancellation authority
* publish Runtime Artifact
* quyết định downstream Business continuation

Nếu Reading Order Result hoàn thành sau khi execution intent đã thay đổi, Runtime Control quyết định result còn execution authority để publish hay phải bị reject như stale/cancelled.

Reading Order semantics không bị Runtime redefine.

---

# 61. Cache Integration

Reading Order định nghĩa semantic compatibility.

Ví dụ result có thể không còn compatible nếu:

* OCR Document thay đổi
* Layout version thay đổi
* Direction version thay đổi
* Reading Profile thay đổi
* Strategy version thay đổi

Global cache lifecycle thuộc Runtime Cache Policy.

---

# 62. Event Integration

Reading Order có thể tạo domain facts như:

```text
ReadingOrderCompleted
ReadingOrderAmbiguous
ReadingOrderFailed
```

Ý nghĩa semantic thuộc Reading Order.

Event delivery, envelope và transport thuộc Event Bus.

---

# 63. Error Integration

Reading Order có thể tạo lỗi semantic như:

```text
InvalidOCRDocument
MissingLayoutTree
MissingDirectionData
InvalidHierarchy
GraphCycleUnresolved
EntityReferenceMissing
StrategyUnavailable
SequenceValidationFailed
```

Các lỗi này phải map vào Runtime Error Model khi crossing Runtime execution boundary; Reading Order vẫn sở hữu semantic meaning của lỗi.

---

# 64. Architecture Invariants

Reading Order luôn phải đảm bảo:

1. Không thay đổi Recognition content.

2. Không thay đổi Detection Geometry.

3. Không thay đổi Layout Tree.

4. Không thay đổi Text Direction.

5. Chỉ tạo order relationships, sequences và ordering diagnostics.

6. Mọi Ordered Entity phải tham chiếu entity nguồn.

7. Không âm thầm bỏ entity.

8. Graph phải acyclic trước linearization.

9. Same semantic input + same Strategy Version phải tạo structurally equivalent order.

10. Provider Hint không phải authoritative truth.

11. Main Sequence và Auxiliary Sequence phải tách rõ.

12. Ambiguous order phải được biểu diễn thay vì che giấu.

13. Reading Order không sở hữu Runtime scheduling.

14. Reading Order không sở hữu Runtime Retry Policy hoặc Retry budget.

15. Reading Order không sở hữu Cancellation authority.

16. Reading Order không sở hữu Runtime execution authority hoặc stale-result decision.

17. Reading Order không sở hữu global cache lifecycle.

18. Reading Order không định nghĩa Event Bus transport semantics.

19. Reading Order không sở hữu Runtime Artifact publication.

20. Reading Order không quyết định downstream Business continuation.

21. Translation không được tự suy luận lại Reading Order từ Geometry nếu Reading Order Result hợp lệ và compatible đã tồn tại.

22. Reading Confidence thuộc Reading Order; Quality chỉ consume nó khi evaluation scope thực sự bao gồm Reading Order output.

23. Manual override phải tách khỏi immutable OCR Document và Reading Order base result.

24. Incremental recomputation phải giữ stable order cho unchanged compatible scopes khi có thể.

---

# 65. Recommended MVP Strategy

MVP nên ưu tiên strategy đơn giản và deterministic.

```text
OCR Document
    ↓
Layout Hierarchy
    ↓
Text Direction
    ↓
Page Reading Mode
    ↓
Local Geometry Rules
    ↓
Reading Order Graph
    ↓
Conflict Resolution
    ↓
Stable Sequence
```

MVP nên hỗ trợ:

* LTR page
* RTL page
* vertical column text
* Webtoon top-to-bottom
* basic Panel ordering
* Bubble ordering
* Main/Auxiliary separation
* stable fallback
* Ambiguous result
* manual override compatibility

Các strategy nâng cao như:

* speaker-aware ordering
* cross-page semantic flow
* learned user strategy
* AI-based ordering

có thể để sau.

---

# 66. Ownership References

| Concern | Owner |
| --- | --- |
| Region | `DETECTION.md` |
| Region Type | `DETECTION.md` |
| Recognition Text Model | `RECOGNITION.md` |
| Writing Mode / Direction | `TEXT_DIRECTION.md` |
| Layout Tree | `LAYOUT.md` |
| Spatial Relationships | `LAYOUT.md` |
| OCR Document | `POSTPROCESS.md` |
| OCR Quality Report | `QUALITY.md` |
| Reading Order Graph | `READING_ORDER.md` |
| Main/Auxiliary Sequence | `READING_ORDER.md` |
| Reading Confidence | `READING_ORDER.md` |
| Reading semantic compatibility | `READING_ORDER.md` |
| Same-work Retry | Runtime Retry Policy |
| Cancellation authority | Runtime Control / Cancellation |
| Scheduling | Runtime Scheduler |
| Execution Authority / stale-result rejection | Runtime Control |
| Runtime Artifact publication | Runtime Artifact boundary |
| Business continuation | Business Pipeline Orchestration |
| Cache lifecycle | Runtime Cache Policy |
| Event transport | Event Bus |
| Error normalization | Runtime Error Model |
| Override persistence | Business / Persistence owner |

---

# 67. Summary

Reading Order chuyển:

```text
OCR Document
```

từ một tập hợp entity có geometry và hierarchy

thành:

```text
Reading Order Graph
+
Main Reading Sequence
+
Auxiliary Sequence
+
Reading Confidence
```

Flow tổng quát:

```text
OCR Document
    ↓
Hierarchy
    ↓
Candidates
    ↓
Scoring
    ↓
Graph
    ↓
Conflict Resolution
    ↓
Linearization
    ↓
Validated Reading Sequence
```

Nguyên tắc cốt lõi:

```text
Layout defines structure.

Text Direction defines writing direction.

Reading Order defines precedence.

Runtime defines execution mechanics and execution authority.
```
