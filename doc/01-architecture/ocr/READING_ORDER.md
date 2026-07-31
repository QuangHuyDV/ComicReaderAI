# Reading Order

> Status: Draft
> Version: 1.0
> Layer: OCR Pipeline
> Depends On: Detection, Recognition, Text Direction, Layout Analysis, OCR Postprocessing
> Next Layer: Text Model, Segmentation, Translation

---

# 1. Purpose

## Overview

Reading Order là giai đoạn xác định thứ tự đọc hợp lý của các Region, Block, Container và Panel trong OCR Document.

Nếu:

* Detection trả lời:

> "Text nằm ở đâu?"

* Recognition trả lời:

> "Text là gì?"

* Text Direction trả lời:

> "Text được viết theo hướng nào?"

* Layout Analysis trả lời:

> "Các vùng được tổ chức như thế nào?"

thì Reading Order trả lời:

> "Các vùng này phải được đọc theo thứ tự nào?"

Reading Order là cầu nối giữa dữ liệu hình học của OCR và dữ liệu ngôn ngữ dùng cho Text Processing và Translation.

---

## Objectives

Reading Order phải:

* xác định thứ tự đọc của toàn trang
* xác định thứ tự đọc trong từng Panel
* xác định thứ tự đọc trong từng Container
* xác định thứ tự đọc trong từng Block
* hỗ trợ nhiều kiểu truyện
* hỗ trợ nhiều hướng đọc
* duy trì khả năng ánh xạ về Region gốc
* độc lập với Translation Provider

---

## Responsibilities

Reading Order chịu trách nhiệm:

* xây dựng quan hệ trước-sau
* xác định chuỗi đọc
* giải quyết xung đột thứ tự
* sử dụng Layout và Text Direction
* tạo Reading Order Graph
* tạo Ordered OCR Document
* cung cấp Confidence cho từng quyết định

Reading Order không chịu trách nhiệm:

* OCR
* sửa nội dung Recognition
* dịch văn bản
* tách câu ngôn ngữ
* render nội dung
* xác định nhân vật đang nói

---

# 2. Scope

Reading Order hoạt động trên OCR Document đã được Postprocessing chuẩn hóa.

Reading Order có thể xử lý:

* Page
* Panel
* Container
* Block
* Region
* Paragraph
* Line

Reading Order không trực tiếp xử lý:

* Character Recognition
* Image Enhancement
* Translation
* Font Layout
* Text Rendering

---

# 3. Terminology

## Reading Order

Thứ tự logic mà các phần tử văn bản nên được đọc.

---

## Reading Sequence

Danh sách tuyến tính của các phần tử đã được sắp xếp.

Ví dụ:

```text
Region A → Region B → Region C
```

---

## Reading Order Graph

Đồ thị biểu diễn quan hệ trước-sau giữa các phần tử.

Ví dụ:

```text
A → B
A → C
B → D
C → D
```

---

## Precedence Relationship

Quan hệ cho biết một phần tử phải được đọc trước phần tử khác.

---

## Local Order

Thứ tự đọc trong phạm vi nhỏ.

Ví dụ:

* trong một Bubble
* trong một Panel
* trong một Block

---

## Global Order

Thứ tự đọc của toàn bộ trang hoặc toàn bộ tài liệu.

---

## Reading Context

Tập hợp thông tin được dùng để xác định thứ tự đọc.

Ví dụ:

* ngôn ngữ
* loại truyện
* hướng chữ
* kiểu Layout
* Region Type
* Detection Profile

---

## Reading Profile

Cấu hình điều khiển chiến lược Reading Order.

---

# 4. Goals

Reading Order hướng tới:

* đúng ngữ cảnh
* ổn định
* có thể giải thích
* dễ Debug
* hỗ trợ nhiều kiểu bố cục
* không phụ thuộc OCR Provider
* giữ liên kết với dữ liệu gốc

---

# 5. Non-Goals

Reading Order không thực hiện:

* Translation
* Grammar Analysis
* Named Entity Recognition
* Dialogue Attribution
* Speaker Identification
* Semantic Rewriting
* Visual Rendering

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

Reading Order

↓

Text Model

↓

Segmentation

↓

Translation
```

Reading Order là bước cuối cùng còn phụ thuộc mạnh vào Geometry và Layout trước khi dữ liệu được chuyển sang Text Domain.

---

# 7. High-Level Pipeline

```text
OCR Document

↓

Input Validation

↓

Reading Context Construction

↓

Hierarchy Analysis

↓

Candidate Relationship Generation

↓

Precedence Scoring

↓

Conflict Resolution

↓

Graph Construction

↓

Cycle Detection

↓

Topological Ordering

↓

Sequence Validation

↓

Reading Order Result
```

---

# 8. Reading Order Lifecycle

## Stage 1: Input Validation

Kiểm tra OCR Document, Layout Tree, Direction Metadata và Region Geometry.

---

## Stage 2: Context Construction

Xây dựng Reading Context từ:

* Language
* Script
* Writing Mode
* Document Type
* Page Type
* Region Type
* Layout Metadata

---

## Stage 3: Candidate Generation

Sinh các quan hệ thứ tự có thể xảy ra giữa các phần tử.

---

## Stage 4: Scoring

Đánh giá mức độ hợp lý của từng quan hệ.

---

## Stage 5: Graph Construction

Tạo Reading Order Graph.

---

## Stage 6: Conflict Resolution

Loại bỏ hoặc giảm ưu tiên các quan hệ mâu thuẫn.

---

## Stage 7: Linearization

Chuyển Graph thành Reading Sequence.

---

## Stage 8: Validation

Kiểm tra tính đầy đủ, tính nhất quán và khả năng ánh xạ.

---

# 9. Inputs

Reading Order nhận:

* OCR Document
* Layout Tree
* Relationship Graph
* Direction Result
* Region Type
* Geometry
* Recognition Metadata
* Reading Profile

Thông tin tùy chọn:

* Document Language
* Source Type
* Manga Mode
* Webtoon Mode
* Novel Mode
* User Preference

---

# 10. Outputs

Reading Order trả về:

* Reading Order Result
* Reading Order Graph
* Reading Sequence
* Local Reading Sequences
* Confidence
* Diagnostics
* Statistics

Reading Order không tạo bản sao tách rời khỏi OCR Document mà phải giữ tham chiếu đến Entity ID gốc.

---

# 11. Reading Order Result Model

```text
Reading Order Result

├── Metadata

├── Global Sequence

├── Panel Sequences

├── Container Sequences

├── Block Sequences

├── Graph

├── Confidence

├── Diagnostics

└── Statistics
```

Mỗi phần tử trong Sequence phải tham chiếu đến một Entity tồn tại trong OCR Document.

---

# 12. Ordered Entity Model

Một Ordered Entity nên bao gồm:

* Entity ID
* Entity Type
* Order Index
* Parent Order ID
* Previous Entity ID
* Next Entity ID
* Confidence
* Source Rules
* Metadata

Entity Type có thể là:

* Panel
* Container
* Block
* Region
* Paragraph
* Line

---

# 13. Reading Hierarchy

Reading Order phải tuân theo cấu trúc phân cấp.

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

Không nên sắp xếp trực tiếp mọi Region trên toàn trang nếu Layout Tree đã cung cấp cấu trúc cha-con đáng tin cậy.

Chiến lược mặc định:

1. Sắp xếp Panel.
2. Sắp xếp Container trong Panel.
3. Sắp xếp Block trong Container.
4. Sắp xếp Region trong Block.
5. Sắp xếp Paragraph và Line trong Region.

---

# 14. Global and Local Order

## Global Order

Global Order mô tả thứ tự của các phần tử cấp cao trên toàn trang.

Ví dụ:

```text
Panel 1 → Panel 2 → Panel 3
```

---

## Local Order

Local Order mô tả thứ tự trong từng phạm vi.

Ví dụ:

```text
Panel 2

Bubble A → Bubble B → Narration C
```

Việc tách Global Order và Local Order giúp:

* giảm độ phức tạp
* dễ Debug
* dễ thay chiến lược
* tránh xung đột toàn cục không cần thiết

---

# 15. Reading Modes

Hệ thống nên hỗ trợ tối thiểu các Reading Mode sau:

* Left-to-Right Page
* Right-to-Left Page
* Top-to-Bottom Scroll
* Vertical Column
* Mixed Layout
* Unknown

Reading Mode có thể được xác định ở:

* Document Level
* Page Level
* Panel Level
* Container Level

Reading Mode cấp thấp hơn có thể ghi đè cấp cao hơn nếu có Confidence đủ cao.

---

# 16. Left-to-Right Reading

Phù hợp với:

* truyện phương Tây
* tài liệu tiếng Việt
* tài liệu tiếng Anh
* website thông thường

Quy tắc cơ bản:

1. ưu tiên phần tử phía trên
2. trong cùng hàng, ưu tiên phần tử bên trái
3. sau đó chuyển xuống hàng tiếp theo

Ví dụ:

```text
A B
C D
```

Thứ tự:

```text
A → B → C → D
```

---

# 17. Right-to-Left Reading

Phù hợp với manga Nhật hoặc tài liệu có bố cục RTL.

Quy tắc cơ bản:

1. ưu tiên phần tử phía trên
2. trong cùng hàng, ưu tiên phần tử bên phải
3. sau đó chuyển xuống hàng tiếp theo

Ví dụ:

```text
A B
C D
```

Thứ tự:

```text
B → A → D → C
```

Reading Direction của văn bản bên trong Bubble không nhất thiết giống Reading Order của các Bubble trên trang.

---

# 18. Top-to-Bottom Reading

Phù hợp với:

* Webtoon
* trang cuộn dọc
* nội dung theo luồng liên tục
* ảnh dài

Quy tắc chính:

1. ưu tiên phần tử có vị trí cao hơn
2. sử dụng khoảng cách dọc làm tín hiệu chính
3. chỉ dùng quan hệ trái-phải khi các phần tử cùng cụm

Ví dụ:

```text
A
↓
B
↓
C
```

---

# 19. Vertical Column Reading

Với văn bản dọc, cần phân biệt:

* hướng Character trong cột
* hướng chuyển giữa các cột

Ví dụ tiếng Nhật truyền thống:

* Character đi từ trên xuống dưới
* cột đi từ phải sang trái

Reading Order phải lưu riêng:

* Intra-column Order
* Inter-column Order

Không được gộp hai khái niệm này thành một Direction duy nhất.

---

# 20. Mixed Reading Mode

Một trang có thể chứa:

* Panel RTL
* UI LTR
* SFX xoay
* Narration dọc
* quảng cáo ngang

Trong trường hợp Mixed Mode:

* xác định Reading Mode theo từng Container hoặc Block
* không ép toàn bộ trang về một hướng
* ưu tiên Layout Hierarchy
* giữ Unknown khi không đủ bằng chứng

---

# 21. Panel Ordering

Panel Ordering là bước đầu tiên trong truyện tranh có nhiều khung.

Các tín hiệu gồm:

* vị trí hình học
* hàng và cột
* kích thước Panel
* khoảng trắng
* đường viền
* Reading Mode
* Panel Overlap
* Panel Nesting

Panel lớn không mặc định luôn được đọc trước.

Panel lồng bên trong Panel khác phải được xử lý theo Parent-Child Relationship.

---

# 22. Row and Column Analysis

Reading Order có thể nhóm các phần tử thành:

* Row
* Column
* Cluster

Hai phần tử có thể thuộc cùng Row khi:

* vùng chiếu theo trục Y giao nhau đủ lớn
* tâm theo trục Y gần nhau
* chiều cao tương thích

Tương tự, cùng Column khi:

* vùng chiếu theo trục X giao nhau đủ lớn
* tâm theo trục X gần nhau
* chiều rộng tương thích

Ngưỡng phải thuộc Reading Profile, không hard-code trong Contract.

---

# 23. Container Ordering

Trong mỗi Panel, các Container như Speech Bubble và Narration Box cần được sắp xếp riêng.

Các tín hiệu:

* vị trí
* loại Region
* Bubble Tail
* hướng trang
* khoảng cách
* quan hệ chồng lấn
* Alignment

Speech Bubble không mặc định luôn được đọc trước Narration Box.

Quyết định phải dựa trên bố cục cụ thể.

---

# 24. Bubble Ordering

Bubble Ordering là trường hợp đặc biệt quan trọng.

Các quy tắc có thể gồm:

* Top-first
* Direction-aware
* Nearest-neighbor
* Cluster-aware
* Tail-assisted
* Panel-constrained

Bubble Tail có thể hỗ trợ xác định liên kết với nhân vật nhưng không đủ để xác định thứ tự đọc một cách độc lập.

---

# 25. Narration Ordering

Narration Box thường:

* nằm đầu Panel
* nằm cuối Panel
* xen giữa hội thoại
* trải ngang nhiều Panel

Reading Order phải dựa vào Geometry và Layout thay vì luôn đặt Narration trước hoặc sau Dialogue.

Narration toàn trang có thể thuộc Page Level thay vì Panel Level.

---

# 26. SFX Ordering

Sound Effects thường không tham gia luồng hội thoại chính.

Reading Profile nên cho phép:

* Exclude SFX
* Include SFX Inline
* Include SFX After Dialogue
* Include SFX by Position

Mặc định, SFX nên có mức ưu tiên thấp hơn Speech Bubble và Narration nhưng không bị xóa khỏi OCR Document.

---

# 27. Background Text Ordering

Background Text có thể:

* thuộc bối cảnh
* bổ sung ý nghĩa
* không cần dịch
* cần dịch theo yêu cầu người dùng

Reading Profile nên quyết định Background Text có tham gia Reading Sequence hay không.

Nếu tham gia, nó nên được đặt theo vị trí tự nhiên trong Panel nhưng không phá vỡ chuỗi Dialogue chính.

---

# 28. UI, Watermark and Advertisement

Các loại sau thường không thuộc nội dung truyện:

* UI Text
* Watermark
* Advertisement

Mặc định:

* không tham gia Main Reading Sequence
* được giữ trong Auxiliary Sequence
* có thể bật lại theo Profile

Việc loại khỏi Sequence không đồng nghĩa xóa khỏi OCR Document.

---

# 29. Spatial Relationship Rules

Reading Order có thể sử dụng các quan hệ:

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

Ví dụ với LTR:

```text
A above B
```

thường tạo:

```text
A → B
```

Nhưng quan hệ hình học chỉ là tín hiệu, không phải kết luận tuyệt đối.

---

# 30. Precedence Candidate Generation

Với mỗi cặp Entity phù hợp, hệ thống có thể sinh Candidate:

```text
A precedes B
```

Candidate nên lưu:

* Source Entity
* Target Entity
* Rule ID
* Weight
* Confidence
* Scope
* Evidence

Không nên tạo Candidate giữa mọi cặp nếu số lượng Entity lớn.

Có thể giới hạn theo:

* cùng Parent
* cùng Row
* cùng Column
* Neighbor Distance
* Spatial Index

---

# 31. Precedence Scoring

Mỗi Candidate được đánh giá dựa trên nhiều tín hiệu.

Ví dụ:

* Vertical Position Score
* Horizontal Position Score
* Direction Compatibility
* Layout Hierarchy Score
* Region Type Priority
* Distance Score
* Alignment Score
* Provider Hint Score

Tổng điểm không nên chỉ là trung bình đơn giản.

Cách tính phải được cấu hình bởi Reading Strategy.

---

# 32. Rule Priority

Các quy tắc nên có mức ưu tiên rõ ràng.

Ví dụ:

1. Parent-Child Constraint
2. Explicit Layout Relationship
3. Reading Mode
4. Row/Column Grouping
5. Spatial Distance
6. Region Type Preference
7. Provider Hint
8. Fallback Position

Quy tắc ưu tiên thấp không được phá vỡ Constraint của quy tắc ưu tiên cao.

---

# 33. Reading Order Graph

Graph gồm:

* Node: Entity cần sắp xếp
* Edge: Quan hệ precedes
* Weight: mức tin cậy
* Evidence: nguồn quyết định

Ví dụ:

```text
A ──▶ B
│     │
▼     ▼
C ──▶ D
```

Graph có thể là DAG sau khi Conflict Resolution hoàn tất.

---

# 34. Conflict Resolution

Conflict xảy ra khi có các quan hệ như:

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

Chiến lược xử lý:

* loại Edge có Weight thấp hơn
* ưu tiên Constraint cấp cao hơn
* sử dụng Layout Hierarchy
* dùng Fallback Strategy
* ghi Diagnostics

Không được bỏ qua Conflict mà vẫn tiếp tục Linearization.

---

# 35. Cycle Detection

Reading Order Graph phải được kiểm tra Cycle trước khi sắp xếp tuyến tính.

Nếu phát hiện Cycle:

1. xác định Edge yếu nhất
2. kiểm tra Rule Priority
3. loại hoặc giảm Edge phù hợp
4. chạy lại Cycle Detection
5. ghi lại quyết định

Nếu không thể giải quyết, Scope đó phải được đánh dấu `Ambiguous`.

---

# 36. Topological Ordering

Sau khi Graph trở thành DAG, hệ thống có thể sử dụng Topological Sort để tạo Sequence.

Khi có nhiều Node cùng hợp lệ tại một thời điểm, dùng Tie-breaker:

* Reading Mode
* Geometry
* Stable ID
* Detection Index

Tie-breaker phải ổn định để cùng một đầu vào tạo cùng một kết quả.

---

# 37. Stable Ordering

Reading Order phải có tính Deterministic.

Cùng:

* OCR Document
* Reading Profile
* Strategy Version

phải tạo cùng Reading Sequence.

Không nên phụ thuộc vào:

* thứ tự Map
* thứ tự xử lý Thread
* thời điểm hoàn tất Provider
* ID ngẫu nhiên không ổn định

---

# 38. Unknown and Ambiguous Order

Khi không đủ bằng chứng:

* không ép Confidence cao
* dùng Fallback Position Order
* đánh dấu Ambiguous
* ghi Diagnostics

Fallback có thể là:

* Top-to-Bottom, Left-to-Right
* Top-to-Bottom, Right-to-Left
* Detection Index
* Provider Sequence

Fallback phải được ghi rõ trong Result Metadata.

---

# 39. Partial Ordering

Không phải lúc nào cũng cần một thứ tự tuyệt đối cho mọi phần tử.

Ví dụ:

* hai SFX độc lập
* Background Text không liên quan
* Watermark ngoài luồng chính

Hệ thống có thể giữ Partial Order trong Graph và chỉ Linearize Main Sequence.

Auxiliary Entity có thể nằm trong Sequence riêng.

---

# 40. Main and Auxiliary Sequences

Reading Order Result nên hỗ trợ:

## Main Sequence

Nội dung chính cần đưa sang Text Processing và Translation.

Ví dụ:

* Speech Bubble
* Narration
* Important Background Text

## Auxiliary Sequence

Nội dung phụ.

Ví dụ:

* SFX
* UI Text
* Watermark
* Advertisement

Việc phân loại phụ thuộc Reading Profile.

---

# 41. Cross-Panel Relationships

Thông thường, Entity trong Panel trước được đọc trước Entity trong Panel sau.

Tuy nhiên có ngoại lệ:

* Bubble kéo dài qua nhiều Panel
* Narration phủ toàn trang
* Panel lồng nhau
* Layout phi tuyến

Cross-Panel Edge chỉ nên được tạo khi:

* có bằng chứng Layout mạnh
* Parent Scope cho phép
* không phá vỡ Panel Order chính

---

# 42. Cross-Page Reading Order

Reading Order trong file này chủ yếu xử lý Page Level.

Cross-Page Order nên đơn giản:

```text
Page N → Page N+1
```

Các trường hợp đặc biệt:

* trang đôi
* ảnh dài bị cắt
* trang tải không đúng thứ tự
* Webtoon nhiều Segment

cần Metadata từ Source hoặc Document Runtime.

Reading Order không nên tự suy đoán số trang khi thiếu dữ liệu nguồn.

---

# 43. Double-Page Spread

Một ảnh có thể chứa hai trang truyện.

Hệ thống cần xác định:

* Left Page
* Right Page
* Spread Reading Mode

Với manga RTL:

```text
Right Page → Left Page
```

Với comic LTR:

```text
Left Page → Right Page
```

Sau đó mới áp dụng Reading Order nội bộ từng trang.

---

# 44. Webtoon Reading Order

Webtoon thường có đặc điểm:

* ảnh dài
* khoảng cách dọc lớn
* ít Panel kín
* nội dung nối liên tục
* Bubble xen theo chiều dọc

Chiến lược ưu tiên:

1. Vertical Position
2. Cluster theo khoảng cách
3. Local Horizontal Relationship
4. Container Order
5. Stable Fallback

Không nên áp dụng Row-based Comic Layout một cách máy móc.

---

# 45. Novel and Plain Text Reading Order

Với trang tiểu thuyết hoặc văn bản thuần:

* Column Detection quan trọng hơn Panel Detection
* Paragraph Order quan trọng hơn Bubble Order
* Header/Footer có thể tách khỏi Main Sequence
* Footnote cần Sequence riêng hoặc liên kết tham chiếu

Reading Profile phải cho phép chuyển chiến lược theo loại tài liệu.

---

# 46. Mixed-Language Documents

Ngôn ngữ khác nhau có thể dùng hướng khác nhau trong cùng trang.

Ví dụ:

* tiếng Nhật dọc
* tiếng Anh ngang
* UI LTR
* ghi chú xoay

Reading Order nên dùng:

* Page Reading Mode cho cấu trúc lớn
* Local Writing Mode cho nội dung bên trong Entity

Không dùng Language Code đơn lẻ để quyết định toàn bộ thứ tự trang.

---

# 47. Reading Strategy

Reading Strategy là thuật toán cụ thể dùng để xây dựng thứ tự.

Các chiến lược có thể gồm:

* Geometric Strategy
* Layout Tree Strategy
* Graph Strategy
* Manga Strategy
* Webtoon Strategy
* Document Strategy
* Hybrid Strategy
* AI-assisted Strategy

Mọi Strategy phải triển khai cùng Reading Order Contract.

---

# 48. Hybrid Strategy

Hybrid Strategy kết hợp:

* Rule-based Geometry
* Layout Hierarchy
* Direction Metadata
* Provider Hint
* AI Model

Chiến lược này phù hợp cho CRAI vì không có một thuật toán duy nhất hoạt động tốt cho mọi loại truyện.

AI Hint không được tự động ghi đè Constraint cứng nếu không có Confidence đủ cao.

---

# 49. Provider Hints

Một số OCR hoặc Layout Provider có thể trả về Reading Order.

Provider Hint có thể được dùng làm:

* Candidate
* Tie-breaker
* Fallback
* Validation Signal

Không được coi Provider Sequence là nguồn sự thật tuyệt đối.

Provider Hint phải được chuẩn hóa về Entity ID của CRAI.

---

# 50. Reading Confidence

Confidence cần được lưu ở nhiều cấp:

* Edge Confidence
* Local Sequence Confidence
* Panel Order Confidence
* Global Order Confidence

Global Confidence không nhất thiết bằng trung bình của Edge Confidence.

Một Sequence có thể có Confidence cao dù một số Auxiliary Entity có Confidence thấp.

---

# 51. Confidence Factors

Confidence có thể dựa trên:

* độ rõ của Layout
* mức độ đồng thuận giữa Rules
* số Conflict
* số Fallback Decision
* độ tin cậy của Direction
* độ tin cậy của Panel Detection
* khoảng cách hình học
* Provider Agreement

Quality Assessment có thể sử dụng các chỉ số này để quyết định Retry hoặc Manual Review.

---

# 52. Sequence Validation

Reading Sequence phải được kiểm tra:

* không chứa Entity không tồn tại
* không lặp Entity trái quy tắc
* không thiếu Main Entity
* Order Index liên tục
* Previous/Next Reference hợp lệ
* Parent Scope hợp lệ
* không vi phạm Constraint cứng

---

# 53. Completeness Rules

Mỗi Entity được cấu hình tham gia Reading Order phải xuất hiện:

* đúng một lần trong Main Sequence
* hoặc đúng một lần trong Auxiliary Sequence
* hoặc được ghi rõ là Excluded

Không được âm thầm bỏ mất Entity.

---

# 54. Exclusion Rules

Entity có thể bị loại khỏi Main Sequence khi:

* Region Type bị Profile bỏ qua
* Confidence quá thấp
* dữ liệu không hợp lệ
* thuộc Advertisement
* thuộc Watermark
* bị trùng lặp

Exclusion phải được ghi trong Diagnostics với Reason Code.

---

# 55. Duplicate Handling

Hai Region có thể chứa cùng nội dung do:

* Provider Detect trùng
* Region Merge lỗi
* ảnh chồng
* Watermark lặp

Reading Order không trực tiếp xóa dữ liệu OCR nhưng có thể:

* chỉ chọn một Entity vào Main Sequence
* đánh dấu Duplicate Candidate
* chuyển Entity còn lại sang Excluded hoặc Auxiliary

Quyết định cuối về xóa dữ liệu thuộc Postprocessing hoặc Quality Workflow.

---

# 56. Incremental Reading Order

Trong chế độ cuộn hoặc OCR thời gian thực, chỉ một phần trang có thể thay đổi.

Incremental Reading Order phải:

* giữ Order ID cũ khi có thể
* chỉ tính lại Scope bị ảnh hưởng
* nối Sequence mới với Sequence cũ
* tránh làm thay đổi toàn bộ thứ tự không cần thiết

---

# 57. Sequence Stability

Khi thêm Region mới ở cuối Webtoon, thứ tự trước đó không nên bị thay đổi.

Khi thêm Region nằm giữa trang, chỉ Scope liên quan nên được sắp xếp lại.

Sequence Stability đặc biệt quan trọng cho:

* overlay thời gian thực
* subtitle-like display
* reading history
* translation cache

---

# 58. Reading Order Cache

Cache Key nên bao gồm:

* OCR Document ID
* OCR Document Version
* Layout Version
* Direction Version
* Reading Profile
* Strategy Version

Cache phải bị invalid khi:

* Geometry thay đổi
* Layout thay đổi
* Region Type thay đổi
* Reading Mode thay đổi
* Profile thay đổi

---

# 59. Reading Order Events

Các Event khuyến nghị:

```text
ReadingOrderRequested
ReadingOrderStarted
ReadingContextResolved
ReadingCandidateGenerated
ReadingConflictDetected
ReadingConflictResolved
ReadingSequenceGenerated
ReadingOrderCompleted
ReadingOrderFailed
ReadingOrderCancelled
```

Tên Event thực tế phải tuân theo Event Convention chung của CRAI.

---

# 60. Reading Order State Machine

Luồng trạng thái khuyến nghị:

```text
Created

↓

Queued

↓

Analyzing

↓

BuildingGraph

↓

ResolvingConflicts

↓

Linearizing

↓

Validating

↓

Completed
```

Trạng thái kết thúc khác:

```text
Failed
Cancelled
Ambiguous
```

`Ambiguous` có thể là trạng thái hoàn thành có cảnh báo, tùy Runtime Contract.

---

# 61. Error Model

Các lỗi có thể gồm:

* InvalidOCRDocument
* MissingLayoutTree
* MissingDirectionData
* InvalidHierarchy
* GraphCycleUnresolved
* EntityReferenceMissing
* StrategyUnavailable
* ProfileInvalid
* SequenceValidationFailed

Lỗi Provider-specific không được rò rỉ trực tiếp ra Reading Order Contract.

---

# 62. Diagnostics

Diagnostics nên lưu:

* Rule đã áp dụng
* Candidate bị loại
* Conflict
* Cycle
* Fallback Decision
* Excluded Entity
* Ambiguous Scope
* Confidence Breakdown

Diagnostics rất quan trọng vì Reading Order thường khó Debug chỉ bằng Sequence cuối cùng.

---

# 63. Explainability

Mỗi quan hệ quan trọng nên có thể giải thích.

Ví dụ:

```text
Bubble B được đọc trước Bubble A vì:

- Page Mode: RTL
- cùng Row
- B nằm bên phải A
- Confidence: 0.93
```

Explainability không cần bật trong Production Result nhẹ, nhưng phải có khả năng thu thập ở Debug Mode.

---

# 64. Performance Considerations

Reading Order cần tối ưu cho:

* nhiều Region
* ảnh Webtoon dài
* Batch Page
* Runtime thời gian thực

Không nên so sánh toàn bộ cặp Entity theo O(n²) khi có thể dùng:

* Spatial Index
* R-tree
* Grid Partition
* Row/Column Clustering
* Parent Scope Filtering
* Nearest Neighbor Search

---

# 65. Scalability

Reading Order phải hỗ trợ:

* một Region
* một Page
* Double-page Spread
* Webtoon dài
* Chapter nhiều Page
* Batch Processing
* nhiều Session đồng thời

Cross-Page Order không nên làm tăng chi phí tính toán của từng Page độc lập.

---

# 66. Testing Strategy

Cần kiểm thử tối thiểu:

* LTR Comic
* RTL Manga
* Vertical Japanese Text
* Webtoon
* Novel Page
* Mixed Layout
* Overlapping Bubble
* Nested Panel
* Double-page Spread
* Ambiguous Layout
* Missing Direction
* Provider Order Conflict

---

# 67. Golden Test Cases

Reading Order rất phù hợp với Golden Test.

Mỗi Test Case gồm:

* ảnh hoặc OCR Fixture
* Layout Tree
* Expected Graph
* Expected Sequence
* Expected Confidence Range
* Expected Diagnostics

Golden Test phải kiểm tra cả Main Sequence và Auxiliary Sequence.

---

# 68. Benchmark Metrics

Các chỉ số có thể gồm:

* Sequence Accuracy
* Pairwise Order Accuracy
* Panel Order Accuracy
* Bubble Order Accuracy
* Cycle Rate
* Fallback Rate
* Ambiguous Rate
* Processing Latency
* Memory Usage

Pairwise Order Accuracy phù hợp hơn Exact Sequence Accuracy trong các trường hợp có nhiều thứ tự tương đương hợp lệ.

---

# 69. Manual Override

Hệ thống nên hỗ trợ người dùng hoặc công cụ chỉnh sửa:

* đổi vị trí Entity
* kéo thả thứ tự
* loại Entity khỏi Sequence
* thêm Entity vào Main Sequence
* khóa một quan hệ trước-sau

Manual Override phải được lưu tách khỏi OCR Result gốc.

---

# 70. Override Precedence

Thứ tự ưu tiên khuyến nghị:

1. User Override
2. Locked Project Rule
3. Document Profile
4. Reading Strategy
5. Provider Hint
6. Fallback

User Override không nên bị tự động ghi đè khi Pipeline chạy lại, trừ khi người dùng chủ động xóa Override.

---

# 71. Versioning

Reading Order Result phải lưu:

* Contract Version
* Strategy ID
* Strategy Version
* Reading Profile Version
* OCR Document Version

Kết quả cache từ Strategy Version cũ phải được đánh giá lại trước khi tái sử dụng.

---

# 72. Compatibility

Strategy mới phải:

* đọc được OCR Document Contract hiện hành
* không thay đổi Entity ID
* không làm mất Metadata
* xuất đúng Reading Order Result Contract

Nếu có Breaking Change, phải tăng Contract Version.

---

# 73. Security and Privacy

Reading Order chủ yếu xử lý dữ liệu cục bộ và không cần gửi nội dung ra ngoài.

Nếu sử dụng AI-assisted Strategy từ xa:

* phải tuân theo Privacy Profile
* chỉ gửi dữ liệu tối thiểu
* không gửi ảnh nếu chỉ cần Geometry
* không ghi log nội dung nhạy cảm mặc định
* phải hỗ trợ tắt Remote Strategy

---

# 74. Architecture Invariants

Reading Order phải luôn đảm bảo:

* không thay đổi nội dung Recognition
* không thay đổi Geometry
* không thay đổi Layout Tree
* không thay đổi Text Direction
* chỉ tạo quan hệ thứ tự và Sequence
* mọi Ordered Entity phải ánh xạ về Entity gốc
* không được âm thầm loại Entity
* Graph phải không có Cycle trước Linearization
* cùng Input và Strategy Version phải cho cùng Output
* Provider Hint không phải nguồn sự thật tuyệt đối
* Main Sequence và Auxiliary Sequence phải được phân biệt rõ
* Reading Order Result phải độc lập với Translation Provider
* các module phía sau không cần tự suy luận lại thứ tự từ Geometry

---

# 75. Future Extensions

Kiến trúc phải cho phép mở rộng:

* AI-based Reading Order
* Speaker-aware Bubble Ordering
* Cross-page Dialogue Flow
* Temporal Order cho Animated Comic
* Interactive Reading Path
* User-personalized Reading Mode
* Learned Strategy từ Manual Override
* Multi-column Academic Document
* Footnote and Reference Graph
* Multi-page Spread Analysis
* Collaborative Order Editing

---

# 76. Summary

Reading Order chuyển OCR Document từ một tập hợp phần tử có cấu trúc không gian thành một chuỗi đọc có ý nghĩa.

Đầu vào chính:

```text
OCR Document
+
Layout Tree
+
Text Direction
+
Region Type
```

Đầu ra chính:

```text
Reading Order Graph
+
Main Reading Sequence
+
Auxiliary Sequence
+
Confidence
+
Diagnostics
```

Reading Order là ranh giới cuối giữa OCR Domain và Text Domain.

Sau bước này, CRAI có thể xây dựng Text Document, chia Translation Segment và truyền nội dung sang Translation Pipeline mà không cần các module ngôn ngữ tự phân tích lại bố cục ảnh.
