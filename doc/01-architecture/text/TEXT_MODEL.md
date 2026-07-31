# Text Model

> Status: Draft
> Version: 1.0
> Layer: Text Processing
> Depends On: OCR Postprocessing, Reading Order
> Used By: Segmentation, Context Management, Translation, Presentation, Search, Export
> Next Layer: Text Segmentation

---

# 1. Purpose

## Overview

Text Model định nghĩa mô hình dữ liệu văn bản chuẩn được sử dụng xuyên suốt CRAI sau khi OCR Pipeline hoàn tất.

OCR Document mô tả văn bản theo góc nhìn của hình ảnh:

* vùng chữ nằm ở đâu
* hình học như thế nào
* bố cục ra sao
* hướng chữ thế nào
* thứ tự đọc là gì

Text Document mô tả cùng nội dung đó theo góc nhìn ngôn ngữ:

* văn bản thuộc tài liệu nào
* đoạn nào đứng trước đoạn nào
* câu nào thuộc đoạn nào
* nội dung nào có thể dịch cùng nhau
* kết quả dịch phải ánh xạ về vùng ảnh nào

Text Model là ranh giới giữa:

```text
Visual Domain
```

và:

```text
Language Domain
```

---

## Core Principle

Text Model không được làm mất liên kết với dữ liệu OCR gốc.

Mọi thành phần văn bản được tạo từ OCR phải có khả năng truy ngược về:

* Page
* Panel
* Container
* Region
* Paragraph
* Line
* Word
* Geometry

Ngược lại, Presentation phải có khả năng dùng Text Model để xác định nội dung dịch thuộc vị trí nào trên ảnh.

---

## Objectives

Text Model phải:

* cung cấp Contract văn bản thống nhất
* tách xử lý ngôn ngữ khỏi OCR Provider
* duy trì thứ tự đọc
* duy trì liên kết với OCR Entity
* hỗ trợ truyện tranh
* hỗ trợ tiểu thuyết
* hỗ trợ Webtoon
* hỗ trợ văn bản hỗn hợp
* hỗ trợ chỉnh sửa thủ công
* hỗ trợ dịch và ánh xạ ngược
* hỗ trợ versioning
* hỗ trợ incremental processing

---

## Responsibilities

Text Model chịu trách nhiệm định nghĩa:

* Text Document
* Text Page
* Text Section
* Text Block
* Text Paragraph
* Text Sentence
* Text Span
* Text Token
* Source Reference
* Text Relationship
* Text Metadata
* Text Version

Text Model không chịu trách nhiệm:

* OCR
* xác định Reading Order
* tự động dịch
* sửa ngữ pháp
* nhận diện người nói
* render chữ
* chọn Translation Provider

---

# 2. Scope

Text Model mô tả dữ liệu văn bản sau khi:

* OCR đã nhận dạng nội dung
* OCR Document đã được chuẩn hóa
* Reading Order đã được xác định

Text Model có thể chứa:

* Source Text
* Normalized Text
* Translated Text Reference
* Annotation
* Language Metadata
* Structural Relationship
* Visual Source Reference

Text Model không trực tiếp chứa:

* ảnh nhị phân
* OCR SDK Response nguyên bản
* Translation Provider Response nguyên bản
* dữ liệu render cuối cùng

---

# 3. Terminology

## Text Document

Mô hình văn bản chuẩn đại diện cho một tài liệu hoặc một đơn vị đọc hoàn chỉnh.

Ví dụ:

* một trang truyện
* một chương truyện
* một ảnh Webtoon
* một trang tiểu thuyết
* một tài liệu nhiều trang

---

## Text Node

Tên chung cho một thành phần trong Text Document.

Ví dụ:

* Section
* Block
* Paragraph
* Sentence
* Span
* Token

---

## Source Text

Nội dung được lấy từ OCR hoặc nguồn văn bản ban đầu.

---

## Normalized Text

Nội dung đã được chuẩn hóa về Unicode, khoảng trắng, dấu câu hoặc quy ước hiển thị nhưng chưa thay đổi ý nghĩa.

---

## Display Text

Nội dung được chuẩn bị cho việc hiển thị.

Display Text có thể khác Normalized Text do:

* xuống dòng
* thêm dấu ngắt
* định dạng
* rút gọn hiển thị
* thay thế ký tự tương thích

---

## Source Reference

Liên kết từ Text Node về Entity trong OCR Document hoặc nguồn đầu vào khác.

---

## Text Span

Một đoạn văn bản liên tục có chung thuộc tính.

Ví dụ:

* cùng ngôn ngữ
* cùng kiểu chữ
* cùng Source Region
* cùng vai trò ngữ nghĩa sơ bộ

---

## Token

Đơn vị nhỏ phục vụ xử lý ngôn ngữ.

Token không bắt buộc phải trùng với Word của OCR.

---

## Structural Node

Node thể hiện cấu trúc tài liệu.

Ví dụ:

* Page
* Section
* Block
* Paragraph

---

## Linguistic Node

Node thể hiện cấu trúc ngôn ngữ.

Ví dụ:

* Sentence
* Span
* Token

---

# 4. Goals

Text Model hướng tới:

* Provider Independence
* Structural Consistency
* Traceability
* Extensibility
* Deterministic Mapping
* Incremental Update
* Translation Compatibility
* Presentation Compatibility

---

# 5. Non-Goals

Text Model không:

* quyết định cách OCR
* tự sửa lỗi nhận dạng
* xác định bản dịch tốt nhất
* xác định font hiển thị
* tự động suy luận người nói
* tự động chia Translation Batch
* thay thế Translation Memory
* thay thế Glossary

---

# 6. Architecture Position

```text
Image

↓

OCR Pipeline

↓

OCR Document

↓

Reading Order

↓

Text Model Builder

↓

Text Document

↓

Text Segmentation

↓

Context Assembly

↓

Translation

↓

Presentation
```

Text Model Builder chuyển dữ liệu từ OCR Domain sang Text Domain.

---

# 7. High-Level Transformation

```text
OCR Document
+
Reading Sequence
+
Source Metadata

↓

Entity Mapping

↓

Structural Construction

↓

Text Normalization

↓

Language Metadata Attachment

↓

Source Reference Construction

↓

Text Validation

↓

Text Document
```

---

# 8. Text Model Lifecycle

## Stage 1: Input Validation

Kiểm tra:

* OCR Document
* Reading Sequence
* Entity Reference
* Language Metadata
* Document Metadata

---

## Stage 2: Structural Mapping

Chuyển cấu trúc OCR sang cấu trúc văn bản.

---

## Stage 3: Text Extraction

Lấy Source Text theo Reading Order.

---

## Stage 4: Text Normalization

Chuẩn hóa nội dung nhưng không thay đổi ý nghĩa.

---

## Stage 5: Node Construction

Tạo:

* Page
* Section
* Block
* Paragraph
* Sentence
* Span
* Token

---

## Stage 6: Source Linking

Liên kết Text Node với OCR Entity.

---

## Stage 7: Validation

Kiểm tra cấu trúc, thứ tự và khả năng ánh xạ.

---

## Stage 8: Version Assignment

Gắn Contract Version và Document Version.

---

# 9. Inputs

Text Model Builder nhận:

* OCR Document
* Main Reading Sequence
* Auxiliary Reading Sequence
* OCR Language Metadata
* Document Metadata
* Text Profile

Thông tin tùy chọn:

* Manual Correction
* Source URL
* Chapter Metadata
* Page Metadata
* Existing Text Document
* Previous Version
* User Preference

---

# 10. Outputs

Đầu ra chính:

* Text Document

Đầu ra phụ:

* Mapping Report
* Validation Result
* Diagnostics
* Statistics

---

# 11. Text Document Hierarchy

Cấu trúc chuẩn:

```text
Text Document

├── Metadata

├── Pages

│   └── Page

│       └── Sections

│           └── Section

│               └── Blocks

│                   └── Block

│                       └── Paragraphs

│                           └── Paragraph

│                               └── Sentences

│                                   └── Sentence

│                                       └── Spans

│                                           └── Span

│                                               └── Tokens

├── Auxiliary Content

├── Relationships

├── Source Mapping

├── Annotations

└── Diagnostics
```

Không phải mọi tài liệu đều cần sử dụng đầy đủ mọi cấp.

Ví dụ Speech Bubble ngắn có thể là:

```text
Block
└── Paragraph
    └── Sentence
        └── Span
```

---

# 12. Text Document Model

Text Document nên bao gồm:

* Document ID
* Contract Version
* Document Version
* Source Document ID
* Document Type
* Primary Language
* Languages
* Reading Mode
* Pages
* Auxiliary Content
* Relationships
* Metadata
* Created At
* Updated At

Document Type có thể là:

* Comic
* Manga
* Manhua
* Manhwa
* Webtoon
* Novel
* Plain Text
* Scanned Document
* Mixed
* Unknown

---

# 13. Document Identity

Text Document ID phải ổn định trong vòng đời của cùng một tài liệu.

Document Version thay đổi khi:

* Source Text thay đổi
* cấu trúc Node thay đổi
* Reading Order thay đổi
* Source Mapping thay đổi
* Manual Correction thay đổi

Không cần tăng Document Version khi chỉ có:

* Runtime Metrics mới
* Cache Metadata mới
* Diagnostic không ảnh hưởng nội dung

---

# 14. Text Page Model

Text Page đại diện cho một đơn vị trang logic.

Một Text Page nên chứa:

* Page ID
* Page Index
* Source Page ID
* Page Type
* Reading Mode
* Sections
* Page Metadata
* Source References
* Confidence
* Diagnostics

Page Type có thể là:

* Single Page
* Left Spread Page
* Right Spread Page
* Webtoon Segment
* Continuous Page
* Virtual Page
* Unknown

---

# 15. Virtual Page

Với Webtoon hoặc ảnh dài, hệ thống có thể tạo Virtual Page.

Virtual Page không nhất thiết tương ứng với một ảnh vật lý.

Nó có thể đại diện cho:

* một đoạn cuộn
* một nhóm Panel
* một viewport
* một đơn vị xử lý

Virtual Page phải giữ liên kết với:

* Source Image
* Source Coordinates
* Global Offset

---

# 16. Text Section Model

Section là nhóm logic của nhiều Block.

Ví dụ:

* một Panel
* một chương nhỏ
* phần hội thoại
* phần chú thích
* phần nội dung chính
* phần phụ lục

Section nên chứa:

* Section ID
* Section Type
* Order Index
* Blocks
* Parent Section ID
* Source References
* Metadata
* Confidence

---

# 17. Section Types

Section Type có thể gồm:

* Panel
* Scene
* Dialogue Group
* Narration Group
* Chapter Header
* Body
* Footnote
* Caption Group
* Auxiliary
* Unknown

Section Type không bắt buộc phải mang ý nghĩa ngữ nghĩa sâu.

Ở giai đoạn đầu, Section có thể chỉ ánh xạ từ Panel hoặc Layout Container.

---

# 18. Text Block Model

Block là đơn vị cấu trúc văn bản gắn với một vùng nội dung tương đối độc lập.

Ví dụ:

* Speech Bubble
* Narration Box
* Caption
* SFX
* Sign
* UI Text
* Paragraph Block

Block nên chứa:

* Block ID
* Block Type
* Order Index
* Parent Section ID
* Paragraphs
* Source References
* Geometry Reference
* Reading Role
* Language
* Confidence
* Metadata

---

# 19. Block Types

Các Block Type khuyến nghị:

* Dialogue
* Narration
* Caption
* SFX
* Background Text
* Sign
* Title
* Subtitle
* Header
* Footer
* Footnote
* UI
* Watermark
* Advertisement
* Body Text
* Unknown

Block Type có thể được ánh xạ từ Region Type của OCR Document.

---

# 20. Reading Role

Reading Role quyết định Block tham gia luồng nào.

Các giá trị có thể gồm:

* Main
* Auxiliary
* Excluded
* Reference
* Decorative
* Unknown

Reading Role phải được kế thừa từ Reading Order Result khi có thể.

Text Model không nên tự quyết định lại việc một Region thuộc Main hay Auxiliary nếu Reading Order đã đưa ra kết luận.

---

# 21. Text Paragraph Model

Paragraph là một nhóm Sentence có quan hệ gần nhau trong cùng Block.

Paragraph nên chứa:

* Paragraph ID
* Order Index
* Parent Block ID
* Sentences
* Source Text
* Normalized Text
* Language
* Direction
* Source References
* Confidence
* Metadata

Một Speech Bubble có thể chứa:

* một Paragraph
* nhiều Paragraph
* hoặc không xác định được Paragraph rõ ràng

---

# 22. Paragraph Boundaries

Paragraph Boundary có thể đến từ:

* OCR Paragraph
* khoảng cách dòng
* dấu ngắt đoạn
* Layout
* Bubble Structure
* Manual Correction

Paragraph Boundary không được suy ra chỉ từ ký tự xuống dòng do OCR trả về.

OCR Line Break có thể chỉ phản ánh bố cục hình ảnh, không phải ranh giới đoạn ngôn ngữ.

---

# 23. Text Sentence Model

Sentence là đơn vị ngôn ngữ dùng cho:

* Translation
* Context
* Search
* Annotation
* Alignment

Sentence nên chứa:

* Sentence ID
* Order Index
* Parent Paragraph ID
* Source Text
* Normalized Text
* Spans
* Language
* Sentence Boundary Confidence
* Source References
* Metadata

---

# 24. Sentence Boundaries

Sentence Boundary có thể dựa trên:

* dấu câu
* ngôn ngữ
* Writing System
* OCR Line
* Bubble Boundary
* Context
* Rule-based Segmenter
* Language Model
* Manual Override

Sentence Boundary chưa chắc được xác định ngay khi xây dựng Text Document.

Text Model phải cho phép:

* Sentence tạm thời
* Sentence chưa xác định
* Sentence được cập nhật sau Segmentation

---

# 25. Provisional Sentence

Trong bản dựng đầu tiên, một Paragraph có thể được tạo với một Provisional Sentence chứa toàn bộ nội dung.

Sau đó Segmentation Module sẽ:

* tách câu
* gộp câu
* sửa Boundary
* giữ Source Mapping

Cơ chế này giúp Text Model Builder không phụ thuộc chặt vào NLP.

---

# 26. Text Span Model

Span là đoạn văn bản liên tục có cùng đặc điểm.

Span nên chứa:

* Span ID
* Order Index
* Parent Sentence ID
* Source Text
* Normalized Text
* Language
* Script
* Style Hint
* Source References
* Character Range
* Confidence
* Metadata

Span hữu ích khi một Sentence chứa:

* nhiều ngôn ngữ
* nhiều Region
* nhiều Style
* tên riêng
* Ruby Text
* Emphasis
* Inline SFX

---

# 27. Span Boundaries

Span Boundary có thể được tạo khi thay đổi:

* Language
* Script
* Source Region
* Text Style
* Annotation
* Semantic Role
* OCR Confidence Group

Không nên tạo quá nhiều Span nếu không có nhu cầu xử lý.

Một Span cho mỗi ký tự sẽ làm mô hình quá nặng và khó sử dụng.

---

# 28. Text Token Model

Token là đơn vị nhỏ phục vụ xử lý ngôn ngữ.

Token có thể là:

* Word
* Character
* Punctuation
* Number
* Symbol
* Subword
* Whitespace
* Unknown

Token nên chứa:

* Token ID
* Token Type
* Surface Text
* Normalized Text
* Parent Span ID
* Character Range
* Source References
* Language
* Confidence
* Metadata

---

# 29. OCR Word vs Text Token

OCR Word và Text Token là hai khái niệm khác nhau.

OCR Word phụ thuộc vào:

* OCR Engine
* Geometry
* Detection
* Recognition

Text Token phụ thuộc vào:

* ngôn ngữ
* tokenizer
* mục đích xử lý
* mô hình dịch

Ví dụ tiếng Trung:

```text
OCR Word:
我喜欢看漫画
```

có thể trở thành:

```text
Text Token:
我 | 喜欢 | 看 | 漫画
```

Hoặc với Character-based Tokenization:

```text
我 | 喜 | 欢 | 看 | 漫 | 画
```

Text Model phải giữ cả Source Reference thay vì giả định OCR Word bằng Token.

---

# 30. Character Range

Mỗi Sentence, Span và Token nên có Character Range trong Parent Text.

Ví dụ:

```text
Sentence:
"Hello world!"

Span A:
start = 0
end = 5

Span B:
start = 6
end = 12
```

Quy ước `end` nên là exclusive:

```text
[start, end)
```

Quy ước này phải thống nhất trong toàn hệ thống.

---

# 31. Unicode Indexing

Character Index cần chỉ rõ đang sử dụng:

* Unicode Code Point
* UTF-16 Code Unit
* UTF-8 Byte Offset
* Grapheme Cluster

Contract nội bộ nên ưu tiên một chuẩn duy nhất.

Khuyến nghị:

* dùng Unicode Code Point hoặc Grapheme Cluster cho Text Model
* chỉ chuyển sang UTF-16 khi tích hợp UI cần thiết
* không dùng Byte Offset làm chỉ số ngôn ngữ mặc định

---

# 32. Grapheme Cluster

Một ký tự hiển thị có thể gồm nhiều Unicode Code Point.

Ví dụ:

* ký tự có dấu tổ hợp
* emoji
* variation selector
* chữ phức hợp

Text Model không nên giả định:

```text
1 ký tự hiển thị = 1 byte
```

hoặc:

```text
1 ký tự hiển thị = 1 UTF-16 code unit
```

---

# 33. Text Layers

Text Model nên phân biệt các lớp nội dung:

```text
Raw Text
    ↓
Source Text
    ↓
Normalized Text
    ↓
Corrected Text
    ↓
Display Text
```

Không phải mọi Node đều cần có đủ tất cả lớp.

---

# 34. Raw Text

Raw Text là nội dung gần nhất với OCR Provider Result.

Raw Text chỉ nên được giữ khi cần:

* Debug
* Benchmark
* so sánh Provider
* phục hồi dữ liệu

Raw Text không nên là đầu vào mặc định cho Translation.

---

# 35. Source Text

Source Text là nội dung chuẩn được chấp nhận từ OCR Document.

Source Text phải:

* giữ nguyên ý nghĩa
* giữ ký tự quan trọng
* giữ liên kết với OCR Entity
* không tự động sửa nội dung theo phỏng đoán

---

# 36. Normalized Text

Normalized Text có thể áp dụng:

* Unicode Normalization
* chuẩn hóa khoảng trắng
* chuẩn hóa newline
* chuẩn hóa dấu câu tương thích
* loại ký tự điều khiển không hợp lệ
* sửa encoding artifact rõ ràng

Normalized Text không được:

* thay từ
* sửa ngữ pháp
* đoán ký tự OCR sai
* dịch nội dung

---

# 37. Corrected Text

Corrected Text là nội dung đã được:

* người dùng sửa
* spell checker sửa
* OCR correction module sửa
* rule-based correction sửa

Corrected Text phải lưu:

* Correction Source
* Previous Value
* Confidence
* Timestamp
* Actor
* Reason

Không được ghi đè Source Text mà không giữ lịch sử.

---

# 38. Display Text

Display Text là dạng nội dung được Presentation sử dụng.

Display Text có thể áp dụng:

* xuống dòng
* whitespace trình bày
* typographic punctuation
* Ruby Annotation
* abbreviation
* layout-specific formatting

Display Text không phải nguồn chính để Translation hoặc Search xử lý.

---

# 39. Canonical Text

Mỗi Node cần có quy tắc xác định Canonical Text.

Khuyến nghị:

```text
Corrected Text nếu tồn tại
        ↓
Normalized Text
        ↓
Source Text
```

Raw Text và Display Text không nên trở thành Canonical Text mặc định.

---

# 40. Language Model

Language Metadata có thể tồn tại ở:

* Document
* Page
* Section
* Block
* Paragraph
* Sentence
* Span
* Token

Metadata nên gồm:

* Language Code
* Script
* Confidence
* Detection Source
* Is Inherited
* Is Mixed

---

# 41. Language Inheritance

Node con có thể kế thừa Language từ Node cha.

Ví dụ:

```text
Document: zh-Hans
```

thì Block không cần lặp lại Language nếu không khác.

Nếu một Span là tiếng Anh trong Sentence tiếng Trung, Span có thể ghi đè:

```text
Sentence: zh-Hans
Span: en
```

---

# 42. Language Code

Language Code nên tuân theo tiêu chuẩn thống nhất.

Khuyến nghị:

* BCP 47 cho Language Tag
* ISO 15924 cho Script khi cần
* không tạo mã ngôn ngữ nội bộ tùy ý

Ví dụ:

```text
vi
en
zh-Hans
zh-Hant
ja
ko
```

---

# 43. Script Metadata

Script có thể gồm:

* Latin
* Han
* Hiragana
* Katakana
* Hangul
* Arabic
* Cyrillic
* Mixed
* Unknown

Script hỗ trợ:

* Tokenization
* Font Selection
* Translation Routing
* Text Direction
* Normalization

---

# 44. Direction Metadata

Text Node có thể lưu:

* Writing Mode
* Text Direction
* Line Direction
* Block Direction
* Rotation

Direction trong Text Model nên tham chiếu kết quả từ Text Direction Module.

Text Model không nên tự suy luận lại Geometry Direction.

---

# 45. Source Reference Model

Source Reference liên kết Text Node với nguồn tạo ra nó.

Một Source Reference nên chứa:

* Source Type
* Source Document ID
* Source Entity ID
* Source Entity Type
* Character Range
* Geometry Reference
* Page ID
* Confidence
* Mapping Type
* Metadata

---

# 46. Source Types

Source Type có thể là:

* OCR Document
* Plain Text Import
* EPUB
* HTML
* PDF Text Layer
* User Input
* External Subtitle
* Unknown

Thiết kế này giúp Text Model không bị giới hạn chỉ cho OCR.

---

# 47. Mapping Types

Mapping Type có thể gồm:

* Exact
* Merged
* Split
* Derived
* Approximate
* Manual
* Unknown

Ví dụ:

* một Text Sentence từ một OCR Region: `Exact`
* một Sentence gộp từ ba Region: `Merged`
* nhiều Sentence tách từ một Bubble: `Split`

---

# 48. Many-to-Many Mapping

Text Model phải hỗ trợ:

```text
N OCR Entity
        ↕
M Text Node
```

Không được giả định ánh xạ luôn là 1:1.

Ví dụ:

* một câu bị chia qua hai Bubble
* hai dòng OCR tạo thành một câu
* một Bubble chứa ba câu
* nhiều Region bị Merge thành một Paragraph

---

# 49. Source Mapping Index

Text Document nên có Source Mapping Index riêng.

Index hỗ trợ truy vấn:

```text
OCR Region ID
    → Text Nodes
```

và:

```text
Text Node ID
    → OCR Entities
```

Việc này cần thiết cho:

* Highlight
* Overlay
* Manual Edit
* Debug
* Translation Alignment
* Presentation

---

# 50. Geometry Reference

Text Node không nhất thiết sao chép toàn bộ Geometry.

Thay vào đó có thể lưu:

* Geometry Entity ID
* Bounding Box Reference
* Polygon Reference
* Coordinate Space
* Source Image ID

Geometry gốc vẫn thuộc OCR Document.

Text Model chỉ giữ Reference hoặc Snapshot tối thiểu khi cần.

---

# 51. Coordinate Space

Mọi Geometry Reference phải chỉ rõ Coordinate Space.

Ví dụ:

* Original Image Pixels
* Preprocessed Image Pixels
* Normalized Coordinates
* Viewport Coordinates
* Page Coordinates

Không được trộn các hệ tọa độ mà không có Transformation Metadata.

---

# 52. Order Model

Mỗi Node có thứ tự trong Parent Scope.

Các trường khuyến nghị:

* Order Index
* Previous Node ID
* Next Node ID
* Reading Sequence ID

Order Index phải:

* xác định
* ổn định
* không phụ thuộc Map Iteration
* phản ánh Reading Order Result

---

# 53. Global Text Order

Text Document phải cho phép duyệt toàn bộ nội dung chính theo thứ tự tuyến tính.

Ví dụ:

```text
Document
    → Page
        → Section
            → Block
                → Paragraph
                    → Sentence
```

Global Order không được làm mất cấu trúc phân cấp.

---

# 54. Auxiliary Content

Auxiliary Content chứa các Node không thuộc Main Reading Flow.

Ví dụ:

* SFX
* Watermark
* UI
* Advertisement
* Decorative Text
* Metadata Text

Auxiliary Content vẫn phải giữ:

* Source Reference
* Language
* Geometry
* Order cục bộ nếu có

---

# 55. Relationship Model

Ngoài cấu trúc Parent-Child, Text Model có thể có các quan hệ khác.

Ví dụ:

* continues
* references
* overlaps
* alternative
* annotation_of
* translation_of
* reply_to
* belongs_to_speaker
* visually_near
* derived_from

Các quan hệ ngữ nghĩa chưa chắc được tạo ở giai đoạn Text Model Builder.

---

# 56. Relationship Entity

Relationship nên chứa:

* Relationship ID
* Relationship Type
* Source Node ID
* Target Node ID
* Direction
* Confidence
* Source
* Metadata

Relationship không được thay thế cấu trúc Parent-Child cơ bản.

---

# 57. Annotation Model

Annotation dùng để gắn thông tin bổ sung mà không thay đổi Text.

Ví dụ:

* tên riêng
* thuật ngữ
* speaker candidate
* emphasis
* uncertainty
* translation note
* correction suggestion
* glossary match

Annotation nên chứa:

* Annotation ID
* Annotation Type
* Target Node ID
* Character Range
* Value
* Confidence
* Source
* Metadata

---

# 58. Annotation Separation

Annotation không được ghi trực tiếp vào Source Text.

Ví dụ, tên nhân vật không nên được chèn vào chuỗi:

```text
[Character A]: Xin chào
```

Thay vào đó:

```text
Text:
Xin chào

Annotation:
speaker_candidate = Character A
```

Điều này tránh làm sai nội dung gốc.

---

# 59. Style Hint Model

Style Hint có thể lưu thông tin hỗ trợ Presentation:

* Bold
* Italic
* Emphasis
* Handwritten
* Font Size Estimate
* Text Color
* Alignment
* Ruby
* Vertical
* Outline
* Decorative

Style Hint không phải Style cuối cùng.

Presentation có thể ghi đè tùy thiết bị hoặc chế độ hiển thị.

---

# 60. Text Confidence

Confidence có thể tồn tại ở:

* Document
* Page
* Block
* Paragraph
* Sentence
* Span
* Token
* Source Mapping

Confidence trong Text Model có thể tổng hợp từ:

* OCR Confidence
* Reading Order Confidence
* Normalization Confidence
* Boundary Confidence
* Language Detection Confidence

---

# 61. Confidence Separation

Không nên chỉ lưu một Confidence duy nhất.

Ví dụ Sentence có thể có:

* Recognition Confidence
* Boundary Confidence
* Order Confidence
* Language Confidence
* Mapping Confidence

Một câu có Recognition Confidence cao nhưng Sentence Boundary Confidence thấp.

---

# 62. Text Quality Flags

Node có thể mang Quality Flag như:

* Low Recognition Confidence
* Ambiguous Order
* Unknown Language
* Broken Mapping
* Missing Source
* Suspected Duplicate
* Incomplete Text
* Manual Review Required

Quality Flag không được tự động sửa nội dung.

---

# 63. Text Profile

Text Profile điều khiển cách xây dựng Text Document.

Có thể bao gồm:

* Document Type
* Normalization Rules
* Node Granularity
* Language Policy
* Auxiliary Content Policy
* Tokenization Policy
* Source Mapping Policy
* Sentence Initialization Policy

---

# 64. Profile Examples

## Comic Profile

Ưu tiên:

* Panel Section
* Bubble Block
* Dialogue/Narration Type
* Geometry Mapping
* Main/Auxiliary Separation

---

## Novel Profile

Ưu tiên:

* Page
* Column
* Paragraph
* Sentence
* Header/Footer Exclusion

---

## Webtoon Profile

Ưu tiên:

* Continuous Order
* Virtual Page
* Vertical Position
* Incremental Update

---

## Plain Text Profile

Không cần Geometry bắt buộc.

Tập trung vào:

* Paragraph
* Sentence
* Span
* Source Offset

---

# 65. Text Model Builder

Text Model Builder chịu trách nhiệm chuyển:

```text
OCR Document
+
Reading Order Result
```

thành:

```text
Text Document
```

Builder phải triển khai một Contract thống nhất.

---

# 66. Builder Contract

Builder nên hỗ trợ:

* Build Document
* Rebuild Scope
* Update Node
* Validate Document
* Resolve Source Mapping
* Serialize
* Deserialize

Các thao tác chỉnh sửa nội dung người dùng không nên nằm trực tiếp trong Builder Contract chính.

---

# 67. Structural Mapping Rules

Ví dụ ánh xạ mặc định:

```text
OCR Page
→ Text Page

OCR Panel
→ Text Section

OCR Container hoặc Region
→ Text Block

OCR Paragraph
→ Text Paragraph

OCR Line
→ Source Reference hoặc Provisional Span
```

Mapping Rule có thể thay đổi theo Document Type.

---

# 68. Comic Mapping

Cấu trúc khuyến nghị:

```text
Text Document

└── Page

    └── Panel Section

        └── Bubble Block

            └── Paragraph

                └── Provisional Sentence
```

SFX và Background Text có thể được đưa vào Auxiliary Content tùy Profile.

---

# 69. Novel Mapping

Cấu trúc khuyến nghị:

```text
Text Document

└── Page

    └── Body Section

        └── Column Block

            └── Paragraph

                └── Sentence
```

Header, Footer và Footnote có thể nằm trong Section riêng.

---

# 70. Webtoon Mapping

Cấu trúc khuyến nghị:

```text
Text Document

└── Virtual Page

    └── Scene hoặc Panel Section

        └── Bubble Block

            └── Paragraph
```

Virtual Page không được làm thay đổi Global Reading Order.

---

# 71. Normalization Pipeline

Normalization có thể gồm:

```text
Input Text

↓

Unicode Validation

↓

Unicode Normalization

↓

Whitespace Normalization

↓

Line Break Analysis

↓

Control Character Cleanup

↓

Punctuation Compatibility

↓

Normalized Text
```

Mỗi bước nên có thể bật hoặc tắt bằng Profile.

---

# 72. Unicode Normalization Policy

Hệ thống cần chọn rõ một chuẩn như:

* NFC
* NFKC

NFC thường an toàn hơn để giữ hình thức ngôn ngữ.

NFKC chỉ nên dùng khi chấp nhận chuyển đổi các ký tự tương thích.

Normalization Policy phải được lưu trong Metadata.

---

# 73. Whitespace Normalization

Có thể xử lý:

* khoảng trắng lặp
* khoảng trắng đầu cuối
* newline không cần thiết
* non-breaking space
* full-width space

Không nên xóa khoảng trắng nếu nó có ý nghĩa trong:

* ASCII Art
* Code
* biểu thức
* định dạng đặc biệt

---

# 74. OCR Line Break Handling

OCR thường trả về newline theo bố cục ảnh.

Ví dụ:

```text
Tôi rất
vui được gặp
bạn.
```

Normalized Text có thể là:

```text
Tôi rất vui được gặp bạn.
```

Nhưng Source Mapping vẫn phải giữ các Line gốc.

---

# 75. Hyphenation Handling

Với văn bản Latin, từ có thể bị chia cuối dòng:

```text
trans-
lation
```

Normalization có thể gộp thành:

```text
translation
```

Chỉ được gộp khi Confidence đủ cao hoặc có quy tắc rõ ràng.

Phải lưu Correction hoặc Normalization Record.

---

# 76. CJK Text Handling

Với tiếng Trung, Nhật và Hàn:

* không thể dựa hoàn toàn vào khoảng trắng
* dấu câu có thể là full-width
* dòng dọc và dòng ngang có quy tắc khác
* Tokenization nên tách khỏi OCR Word

Text Model không được chèn khoảng trắng máy móc giữa mọi OCR Word.

---

# 77. Ruby and Furigana

Ruby Text có thể được biểu diễn bằng:

* Annotation
* Relationship
* Dedicated Span Metadata

Ví dụ:

```text
Base Text: 漢字
Ruby Text: かんじ
```

Ruby không nên bị nối trực tiếp thành:

```text
漢字かんじ
```

nếu không có ký hiệu phân biệt.

---

# 78. Simplified and Traditional Chinese

Text Model phải giữ nguyên Script nguồn:

* `zh-Hans`
* `zh-Hant`

Việc chuyển đổi Simplified/Traditional là một bước xử lý riêng.

Không được tự động chuyển đổi trong Normalization mặc định.

---

# 79. Punctuation Preservation

Dấu câu có vai trò quan trọng với:

* Sentence Segmentation
* cảm xúc
* Dialogue Style
* Translation
* Presentation

Không nên loại bỏ:

* `…`
* `!?`
* `—`
* dấu ngoặc đặc biệt
* dấu kéo dài
* dấu lặp

Normalization chỉ nên chuyển đổi khi có quy tắc tương đương rõ ràng.

---

# 80. Repeated Punctuation

Truyện tranh thường sử dụng:

```text
!!!
???
……
!?
```

Repeated Punctuation phải được giữ trong Source Text.

Translation hoặc Presentation có thể thay đổi sau, nhưng Text Model không nên rút gọn mặc định.

---

# 81. Empty and Whitespace Nodes

Node không có nội dung có thể xuất hiện do:

* OCR lỗi
* Layout Region trống
* Decorative Region
* Mapping chưa hoàn tất

Text Model Builder có thể:

* loại Node khỏi Main Flow
* giữ Node với Quality Flag
* chuyển Node sang Auxiliary
* ghi Diagnostics

Không được tạo Sentence rỗng hàng loạt mà không có lý do.

---

# 82. Duplicate Text

Duplicate Text có thể xuất phát từ:

* OCR Region trùng
* Bubble chồng
* Provider Merge
* ảnh lặp

Text Model không nên tự xóa dựa chỉ trên chuỗi giống nhau.

Hai Bubble khác nhau có thể thực sự chứa cùng nội dung.

Duplicate Detection phải kết hợp:

* Source Mapping
* Geometry
* Entity Relationship
* OCR Diagnostics

---

# 83. Manual Correction Model

Manual Correction phải được lưu dưới dạng Patch hoặc Revision.

Một Correction nên chứa:

* Correction ID
* Target Node ID
* Previous Text
* New Text
* Character Range
* Actor
* Reason
* Timestamp
* Base Version
* Status

---

# 84. Correction Precedence

Thứ tự ưu tiên nội dung khuyến nghị:

1. Confirmed User Correction
2. Approved Automatic Correction
3. Normalized Text
4. Source Text
5. Raw Text

Correction chưa được xác nhận không nên tự động trở thành Canonical Text.

---

# 85. Immutable Source

Source Text và Source Mapping nên được coi là immutable trong cùng Document Version.

Khi thay đổi Source Text:

* tạo Revision
* tăng Version
* giữ lịch sử
* invalid cache liên quan

Không nên sửa trực tiếp mà không có Audit Record.

---

# 86. Revision Model

Revision nên chứa:

* Revision ID
* Document ID
* Base Version
* New Version
* Change Type
* Changed Nodes
* Actor
* Timestamp
* Reason
* Metadata

Change Type có thể là:

* OCR Update
* Reading Order Update
* Manual Correction
* Segmentation Update
* Structural Update
* Import Update

---

# 87. Incremental Update

Text Model phải hỗ trợ cập nhật một phần.

Ví dụ:

* một Bubble được OCR lại
* một Region được sửa
* Reading Order của một Panel thay đổi
* một Paragraph được chia lại

Không nên tái tạo toàn bộ Document nếu chỉ một Scope thay đổi.

---

# 88. Node Identity Stability

Node ID nên được giữ ổn định nếu Node vẫn đại diện cho cùng nội dung logic.

Ví dụ:

* sửa một ký tự không nhất thiết tạo Block ID mới
* thay đổi Sentence Boundary có thể tạo Sentence ID mới
* thay đổi Source Region hoàn toàn có thể yêu cầu Node mới

Identity Policy phải được định nghĩa rõ.

---

# 89. Stable Node ID

Node ID không nên phụ thuộc hoàn toàn vào:

* Order Index
* Random Runtime ID
* Memory Address
* Provider Result Index

Vì Order có thể thay đổi trong khi Node vẫn là cùng thực thể.

---

# 90. Content Hash

Content Hash có thể dùng cho:

* Cache
* Change Detection
* Deduplication
* Translation Reuse

Hash nên xác định rõ dựa trên:

* Canonical Text
* Language
* Node Type
* Normalization Version

Không nên dùng Hash làm ID duy nhất cho Node vì nhiều Node có thể có cùng nội dung.

---

# 91. Serialization

Text Document phải có thể serialize sang dạng ổn định.

Các yêu cầu:

* giữ Order
* giữ ID
* giữ Source Mapping
* giữ Version
* giữ Unicode chính xác
* không phụ thuộc runtime-specific object

Định dạng có thể là:

* JSON
* MessagePack
* Protobuf
* Internal Database Model

Contract logic không được phụ thuộc một định dạng duy nhất.

---

# 92. Compact Representation

Với Runtime trên trình duyệt hoặc Extension, có thể cần Compact Representation.

Compact Mode có thể:

* bỏ Diagnostics chi tiết
* bỏ Raw Text
* rút gọn Metadata
* dùng Reference Table
* lazy-load Token

Compact Mode không được làm mất:

* Canonical Text
* Order
* Node ID
* Source Mapping cần cho Presentation

---

# 93. Full Representation

Full Representation phù hợp cho:

* Debug
* Benchmark
* Export
* Training Data
* Manual Editing
* Audit

Full Representation có thể chứa:

* Raw Text
* mọi Confidence
* mọi Source Reference
* Normalization Record
* Diagnostics
* Revision History

---

# 94. Validation Rules

Text Document phải được kiểm tra:

* ID không trùng trong cùng Scope
* Parent Reference hợp lệ
* Order Index hợp lệ
* Character Range hợp lệ
* Source Reference tồn tại
* Canonical Text xác định được
* Language Metadata hợp lệ
* không có vòng lặp Parent-Child
* Mapping không tham chiếu Entity không tồn tại

---

# 95. Structural Invariants

Mỗi Node trừ Root phải có Parent hợp lệ.

Node không được thuộc đồng thời hai Parent cấu trúc khác nhau.

Một Sentence không thể trực tiếp thuộc Document nếu Contract yêu cầu Paragraph.

Nếu cần linh hoạt, phải dùng Optional Layer hoặc Synthetic Parent.

---

# 96. Synthetic Nodes

Synthetic Node có thể được tạo để hoàn thiện cấu trúc.

Ví dụ:

* Synthetic Page
* Synthetic Section
* Synthetic Paragraph
* Synthetic Sentence

Synthetic Node phải được đánh dấu:

* `isSynthetic = true`
* Source = Builder
* Reason
* Confidence

---

# 97. Orphan Handling

Node không ánh xạ được vào cấu trúc cha có thể:

* đưa vào Orphan Section
* đưa vào Auxiliary Content
* đánh dấu Invalid
* yêu cầu Manual Review

Không được âm thầm bỏ mất nội dung.

---

# 98. Mapping Validation

Source Mapping phải đảm bảo:

* Source Entity tồn tại
* Character Range không vượt Text
* Mapping Type hợp lệ
* Geometry Reference đúng Coordinate Space
* Mapping Confidence nằm trong phạm vi chuẩn

---

# 99. Text Statistics

Text Document có thể cung cấp:

* Page Count
* Block Count
* Paragraph Count
* Sentence Count
* Token Count
* Character Count
* Language Distribution
* Main/Auxiliary Count
* Low Confidence Node Count
* Mapping Coverage

Statistics không phải nguồn dữ liệu chính, chỉ là dữ liệu dẫn xuất.

---

# 100. Mapping Coverage

Mapping Coverage đo tỷ lệ Text có thể truy ngược về Source Entity.

Mục tiêu với OCR-derived Text Document nên gần:

```text
100%
```

Text được người dùng thêm mới có thể có Source Type là `User Input` thay vì OCR Entity.

---

# 101. Diagnostics

Diagnostics nên ghi nhận:

* Missing Mapping
* Invalid Range
* Unknown Language
* Normalization Change
* Synthetic Node Created
* Orphan Node
* Duplicate Candidate
* Order Mismatch
* Low Confidence Boundary
* Unsupported Script

---

# 102. Explainability

Hệ thống cần có khả năng giải thích:

* Node được tạo từ OCR Entity nào
* tại sao hai dòng được gộp
* tại sao một Block thuộc Auxiliary
* Normalized Text khác Source Text ở đâu
* Sentence Boundary được tạo bởi module nào

---

# 103. Error Model

Các lỗi có thể gồm:

* InvalidOCRDocument
* InvalidReadingSequence
* SourceEntityMissing
* MappingFailed
* InvalidTextRange
* UnsupportedDocumentType
* InvalidLanguageTag
* StructuralValidationFailed
* SerializationFailed
* VersionConflict

---

# 104. Events

Các Event khuyến nghị:

```text
TextDocumentBuildRequested
TextDocumentBuildStarted
TextStructureMapped
TextNormalized
SourceMappingCreated
TextDocumentValidated
TextDocumentCompleted
TextDocumentUpdated
TextDocumentFailed
```

Tên Event thực tế phải tuân theo Event Convention chung của CRAI.

---

# 105. State Model

Luồng trạng thái khuyến nghị:

```text
Created

↓

Building

↓

Mapping

↓

Normalizing

↓

Validating

↓

Ready
```

Trạng thái khác:

```text
Invalid
Failed
Updating
Archived
```

---

# 106. Cache Strategy

Cache Key có thể gồm:

* OCR Document ID
* OCR Document Version
* Reading Order Version
* Text Profile Version
* Builder Version
* Normalization Version

Cache phải invalid khi:

* Source Text thay đổi
* Reading Order thay đổi
* Mapping Rule thay đổi
* Normalization Rule thay đổi
* Manual Correction được áp dụng

---

# 107. Translation Compatibility

Translation Module cần có thể nhận:

* Sentence
* Span
* Segment
* Context Reference
* Source Language
* Target Language
* Source Mapping ID

Translation không nên nhận trực tiếp OCR Provider Result.

---

# 108. Presentation Compatibility

Presentation cần có khả năng:

* lấy Text Node theo Source Region
* lấy Geometry theo Source Reference
* lấy bản dịch tương ứng
* giữ Reading Order
* xác định Block Type
* xác định Direction và Style Hint

Text Model là cầu nối dữ liệu, không quyết định Layout hiển thị cuối.

---

# 109. Search Compatibility

Search Index có thể sử dụng:

* Canonical Text
* Corrected Text
* Translation
* Language
* Document Metadata
* Node Type

Search Result phải có thể điều hướng ngược về:

* Page
* Block
* Source Region
* vị trí ảnh

---

# 110. Export Compatibility

Text Model nên hỗ trợ chuyển sang:

* Plain Text
* Markdown
* HTML
* EPUB
* PDF Text Layer
* Subtitle-like Format
* Translation Dataset

Mỗi Exporter phải chọn rõ:

* Main Content
* Auxiliary Content
* Source Text
* Corrected Text
* Translated Text

---

# 111. Translation Alignment

Bản dịch không nên ghi trực tiếp đè lên Source Text.

Thay vào đó cần liên kết:

```text
Source Text Node
        ↕
Translation Unit
```

Mối liên kết có thể là:

* 1:1
* 1:N
* N:1
* N:M

Text Model phải giữ đủ ID và Range để hỗ trợ Alignment.

---

# 112. Multi-Translation Support

Một Source Node có thể có nhiều bản dịch:

* nhiều ngôn ngữ đích
* nhiều Provider
* nhiều phiên bản
* bản dịch máy
* bản dịch người dùng
* bản dịch đã duyệt

Text Model không nên chứa một trường duy nhất như:

```text
translatedText
```

làm nguồn sự thật duy nhất.

---

# 113. Context Compatibility

Context Module có thể sử dụng:

* Previous Node
* Next Node
* Parent Block
* Section
* Page
* Document Metadata
* Annotations
* Glossary Reference

Do đó Text Model phải giữ cấu trúc và thứ tự, không chỉ giữ chuỗi phẳng.

---

# 114. Dialogue Support

Dialogue Block có thể bổ sung Annotation như:

* Speaker Candidate
* Addressee Candidate
* Dialogue Group
* Emotion
* Bubble Tail Reference

Các thông tin này không bắt buộc ở phiên bản Text Model đầu tiên.

---

# 115. Speaker Separation

Speaker không nên là trường bắt buộc của Sentence vì:

* OCR chưa xác định được
* Bubble Tail có thể mơ hồ
* nhiều người có thể cùng nói
* Narration không có Speaker trực tiếp

Speaker nên là Annotation hoặc Relationship có Confidence.

---

# 116. Scene and Context Grouping

Section có thể được mở rộng thành Scene Group sau này.

Scene Group hỗ trợ:

* Context Window
* Speaker Tracking
* Translation Consistency
* Character Name Resolution

Text Model cơ bản chỉ cần cho phép Relation hoặc Section Type mở rộng.

---

# 117. Privacy

Text Document có thể chứa nội dung nhạy cảm.

Hệ thống cần hỗ trợ:

* local-only storage
* redacted logging
* encryption at rest
* configurable retention
* deletion by Document ID
* remote processing policy

Diagnostics không nên ghi toàn bộ nội dung văn bản mặc định.

---

# 118. Security

Khi serialize hoặc import Text Document:

* kiểm tra kích thước
* kiểm tra depth
* kiểm tra ID
* kiểm tra Character Range
* chống circular structure
* không thực thi nội dung nhúng
* escape khi xuất HTML

Text Model là dữ liệu, không phải mã thực thi.

---

# 119. Performance Considerations

Text Model cần phù hợp với:

* truyện ngắn
* chương dài
* Webtoon rất dài
* nhiều Token
* xử lý trên trình duyệt
* đồng bộ incremental

Không nên luôn tạo Token cho toàn bộ Document nếu chưa cần.

Có thể dùng lazy tokenization.

---

# 120. Lazy Construction

Các tầng có thể được tạo theo nhu cầu:

* Document, Page, Block: tạo ngay
* Paragraph: tạo ngay hoặc theo Profile
* Sentence: provisional
* Span: khi cần
* Token: lazy

Điều này giảm chi phí cho các luồng chỉ cần dịch theo Bubble.

---

# 121. Memory Optimization

Có thể tối ưu bằng:

* shared string table
* reference table
* compact ID
* lazy diagnostics
* separate source mapping index
* paged loading
* immutable snapshots

Tối ưu không được phá vỡ Contract logic.

---

# 122. Testing Strategy

Cần kiểm thử:

* Comic LTR
* Manga RTL
* Chinese Manhua
* Korean Webtoon
* Novel
* Mixed Language
* Vertical Text
* Multiple Sentences in Bubble
* Sentence Across Lines
* Multiple Regions Merged
* Manual Correction
* Incremental Update
* Auxiliary Text
* Missing Source Mapping

---

# 123. Golden Test

Mỗi Golden Test có thể gồm:

* OCR Document Fixture
* Reading Order Fixture
* Text Profile
* Expected Text Structure
* Expected Canonical Text
* Expected Source Mapping
* Expected Diagnostics

---

# 124. Round-Trip Test

Round-Trip Test cần kiểm tra:

```text
OCR Entity
    → Text Node
    → Source Reference
    → OCR Entity
```

và:

```text
Text Node
    → Source Geometry
    → Presentation Target
```

Không được mất liên kết trong quá trình serialize và deserialize.

---

# 125. Deterministic Output

Cùng:

* OCR Document
* Reading Order
* Text Profile
* Builder Version

phải tạo cùng:

* Node Structure
* Order
* Canonical Text
* Source Mapping

Timestamp và Runtime Metric có thể khác nhưng không được ảnh hưởng Content Hash.

---

# 126. Benchmark Metrics

Các chỉ số có thể gồm:

* Build Latency
* Memory Usage
* Node Count
* Mapping Coverage
* Orphan Rate
* Normalization Change Rate
* Invalid Range Rate
* Incremental Update Latency
* Serialization Size

---

# 127. Compatibility and Versioning

Text Document phải lưu:

* Contract Version
* Builder Version
* Normalization Version
* Text Profile Version
* Source OCR Version
* Reading Order Version

Breaking Change phải tăng Contract Version.

---

# 128. Migration

Khi Contract thay đổi, cần Migration Strategy.

Migration có thể:

* thêm trường mặc định
* chuyển đổi Node Type
* xây lại Mapping Index
* cập nhật Range Convention
* giữ ID cũ nếu có thể

Migration không được âm thầm làm mất Source Text hoặc Correction History.

---

# 129. Architecture Invariants

Text Model phải luôn đảm bảo:

* không phụ thuộc OCR Provider cụ thể
* không phụ thuộc Translation Provider cụ thể
* mọi nội dung OCR-derived phải có Source Reference
* không ghi đè Source Text bằng bản dịch
* không ghi đè Source Text bằng Correction mà không có Revision
* Canonical Text phải xác định được
* Order phải phản ánh Reading Order Result
* cấu trúc Parent-Child không có Cycle
* Character Range phải dùng cùng một quy ước
* Language Metadata phải hỗ trợ kế thừa và ghi đè
* Main Content và Auxiliary Content phải được phân biệt
* Geometry thuộc Source Domain và chỉ được tham chiếu
* cùng Input và Version phải tạo cùng Output
* Text Node phải có thể ánh xạ ngược về Source khi nguồn là OCR
* Translation và Presentation không cần đọc OCR Provider Response nguyên bản

---

# 130. Future Extensions

Text Model phải cho phép mở rộng:

* Speaker Model
* Character Entity
* Scene Graph
* Dialogue Graph
* Semantic Role
* Emotion Annotation
* Translation Memory Link
* Glossary Link
* Named Entity Graph
* Cross-page Sentence
* Cross-bubble Dialogue
* User Comment
* Collaborative Editing
* Version Branching
* AI Correction Proposal
* Alignment với Audiobook
* Subtitle Export
* Accessibility Description

---

# 131. Summary

Text Model chuyển dữ liệu từ OCR Domain sang Language Domain.

Đầu vào:

```text
OCR Document
+
Reading Order Result
+
Text Profile
```

Đầu ra:

```text
Text Document
```

Cấu trúc chính:

```text
Document
└── Page
    └── Section
        └── Block
            └── Paragraph
                └── Sentence
                    └── Span
                        └── Token
```

Text Document phải giữ đồng thời:

* nội dung văn bản
* cấu trúc
* thứ tự đọc
* ngôn ngữ
* Confidence
* Source Mapping
* khả năng chỉnh sửa
* khả năng versioning

Đây là Contract trung tâm để các module sau như Segmentation, Context, Translation, Search, Export và Presentation có thể hoạt động mà không phụ thuộc trực tiếp vào OCR Pipeline.
