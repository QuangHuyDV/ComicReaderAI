# Layout Analysis

> Status: Draft
> Version: 1.0
> Layer: OCR Pipeline
> Depends On: Detection, Recognition
> Next Layer: Reading Order

---

# 1. Purpose

## Overview

Layout Analysis là giai đoạn phân tích cấu trúc trực quan của một trang sau khi Detection và Recognition hoàn thành.

Nếu:

* Detection trả lời:

> "Text nằm ở đâu?"

* Recognition trả lời:

> "Text là gì?"

thì Layout Analysis trả lời:

> "Các vùng này được tổ chức như thế nào?"

Layout Analysis không quan tâm nội dung văn bản mà tập trung vào mối quan hệ không gian giữa các Region.

Kết quả của Layout Analysis là nền tảng để Reading Order, Translation và Presentation hoạt động chính xác.

---

## Objectives

Layout Analysis phải:

* phân tích cấu trúc trang
* nhóm các Region liên quan
* xác định Block
* xác định Container
* xác định mối quan hệ giữa các Region
* hỗ trợ nhiều loại tài liệu
* độc lập với OCR Provider

---

## Responsibilities

Layout Analysis chịu trách nhiệm:

* phân tích bố cục
* gom nhóm Region
* phát hiện Block
* phát hiện Panel
* phát hiện Container
* xác định Parent-Child
* sinh Layout Tree

Không chịu trách nhiệm:

* OCR
* Translation
* Reading Order
* Text Correction
* Rendering

---

# 2. Scope

Layout Analysis chỉ làm việc với dữ liệu đã được Detection và Recognition sinh ra.

Không truy cập:

* OCR Provider
* Image Source
* Translation Result

---

# 3. Terminology

## Layout

Cấu trúc trực quan của một trang.

---

## Region

Đơn vị nhỏ nhất được Detection sinh ra.

---

## Block

Một nhóm Region có quan hệ trực quan.

Ví dụ:

* nhiều đoạn hội thoại
* một đoạn mô tả
* tiêu đề
* ghi chú

---

## Container

Đối tượng chứa nhiều Block hoặc Region.

Ví dụ:

* Speech Bubble
* Narration Box
* UI Panel

---

## Panel

Một phần độc lập của trang truyện.

Thông thường truyện tranh được chia thành nhiều Panel.

---

## Layout Tree

Cây mô tả toàn bộ cấu trúc của trang.

---

## Spatial Relationship

Quan hệ hình học giữa các Region.

Ví dụ:

* nằm trên
* nằm dưới
* bên trái
* bên phải
* chồng lên
* chứa
* giao nhau

---

# 4. Goals

Layout Analysis hướng tới:

* cấu trúc ổn định
* dễ mở rộng
* độc lập Provider
* hỗ trợ nhiều định dạng
* hỗ trợ nhiều ngôn ngữ
* hỗ trợ nhiều loại truyện

---

# 5. Non-Goals

Không thực hiện:

* OCR
* Machine Translation
* Spell Checking
* Reading Order
* Semantic Understanding
* Character Recognition

---

# 6. Architecture Position

```text
Image

↓

Detection

↓

Recognition

↓

Layout Analysis

↓

Reading Order

↓

Translation

↓

Presentation
```

Layout Analysis là tầng trung gian giữa OCR và xử lý ngữ nghĩa.

---

# 7. High-Level Pipeline

```text
Recognition Result

↓

Layout Validation

↓

Spatial Analysis

↓

Region Grouping

↓

Container Detection

↓

Panel Detection

↓

Hierarchy Construction

↓

Relationship Analysis

↓

Layout Tree
```

---

# 8. Layout Lifecycle

## Stage 1

Nhận Recognition Result.

---

## Stage 2

Kiểm tra Region.

---

## Stage 3

Phân tích vị trí.

---

## Stage 4

Nhóm Region.

---

## Stage 5

Xây dựng Container.

---

## Stage 6

Xây dựng Panel.

---

## Stage 7

Tạo Layout Tree.

---

## Stage 8

Xuất Layout Result.

---

# 9. Inputs

Layout Analysis nhận:

* Detection Result
* Recognition Result
* Geometry
* Region Type
* Metadata

---

# 10. Outputs

Trả về:

* Layout Result
* Layout Tree
* Block List
* Panel List
* Relationship Graph
* Statistics

---

# 11. Layout Result Model

```text
Layout Result

├── Layout Metadata

├── Panel List

├── Block List

├── Container List

├── Relationship Graph

├── Layout Tree

└── Statistics
```

---

# 12. Layout Tree

Layout Tree mô tả cấu trúc logic của toàn bộ trang.

```text
Page

├── Panel

│     ├── Container

│     │      ├── Block

│     │      │      ├── Region

│     │      │      └── Region

│     │      └── Block

│     └── Container

└── Panel
```

Mỗi Node đều có ID riêng.

---

# 13. Page Model

Một trang là Root của toàn bộ Layout Tree.

Page chứa:

* Metadata
* Geometry
* Panel List
* Statistics

Page không chứa trực tiếp Character hoặc Word.

---

# 14. Panel Model

Panel biểu diễn một vùng độc lập của trang.

Ví dụ:

* một khung truyện
* một ảnh minh họa
* một banner

Panel nên có:

* Panel ID
* Geometry
* Block List
* Metadata

---

# 15. Container Model

Container là nhóm trực quan của nhiều Region.

Ví dụ:

* Speech Bubble
* Narration Box
* UI Window
* Tooltip

Container có thể chứa:

* Block
* Region
* Container khác

---

# 16. Block Model

Block là đơn vị trung gian giữa Region và Container.

Một Block thường đại diện cho:

* một đoạn văn
* một tiêu đề
* một chú thích
* một cụm hội thoại

Block bao gồm:

* Block ID
* Geometry
* Region List
* Metadata

---

# 17. Relationship Graph

Ngoài Layout Tree, hệ thống xây dựng Relationship Graph.

Ví dụ:

```text
Region A

↓

inside

↓

Bubble

↓

belongs_to

↓

Panel 3
```

Graph giúp truy vấn nhanh các quan hệ mà không cần duyệt toàn bộ cây.

---

# 18. Spatial Relationships

Các quan hệ không gian chuẩn:

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

Mỗi quan hệ nên được biểu diễn dưới dạng có hướng khi cần thiết.

---

# 19. Region Grouping

Region Grouping xác định các Region nên thuộc cùng một Block.

Tiêu chí có thể bao gồm:

* khoảng cách
* hướng đọc
* Region Type
* kích thước
* căn lề
* hình học

Grouping không dựa vào nội dung văn bản.

---

# 20. Container Detection

Container Detection phát hiện các thực thể bao bọc Region.

Ví dụ:

* Bubble chứa nhiều đoạn thoại
* Narration Box chứa nhiều Line
* UI Window chứa nhiều Label

Container có thể được suy ra từ Detection hoặc từ phân tích Layout.
