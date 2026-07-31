# Text Direction

> Status: Draft
> Version: 1.0
> Layer: OCR Pipeline
> Depends On: Detection, Recognition
> Next Layer: Layout Analysis, Reading Order

---

# 1. Purpose

## Overview

Text Direction xác định hướng trình bày của văn bản trong từng Region.

Đây là bước giúp hệ thống hiểu văn bản được sắp xếp theo chiều nào trước khi tiến hành phân tích Layout và Reading Order.

Nếu:

* Detection trả lời:

> "Text nằm ở đâu?"

* Recognition trả lời:

> "Text là gì?"

thì Text Direction trả lời:

> "Text được viết theo hướng nào?"

Text Direction không xác định thứ tự đọc toàn bộ trang mà chỉ mô tả hướng của từng Region hoặc từng Block văn bản.

---

## Objectives

Text Direction phải:

* xác định hướng đọc của Region
* xác định hướng của từng Line
* xác định hướng của từng Paragraph
* hỗ trợ nhiều hệ chữ
* hỗ trợ văn bản xoay
* hỗ trợ văn bản dọc
* độc lập với OCR Provider

---

## Responsibilities

Text Direction chịu trách nhiệm:

* phân tích hướng văn bản
* xác định Writing Mode
* xác định Line Direction
* xác định Character Flow
* sinh Direction Metadata

Không chịu trách nhiệm:

* OCR
* Translation
* Reading Order
* Semantic Analysis
* Rendering

---

# 2. Scope

Text Direction chỉ xử lý hướng trình bày của văn bản.

Không xử lý:

* vị trí trang
* thứ tự Panel
* ý nghĩa câu
* dịch thuật

---

# 3. Terminology

## Text Direction

Hướng di chuyển của văn bản.

---

## Writing Mode

Kiểu trình bày của văn bản.

Ví dụ:

* Horizontal
* Vertical
* Mixed

---

## Line Direction

Hướng của từng dòng.

Ví dụ:

* Left → Right
* Right → Left
* Top → Bottom
* Bottom → Top

---

## Character Flow

Thứ tự xuất hiện của Character trong cùng một Line.

---

## Paragraph Direction

Hướng tổng thể của một Paragraph.

---

## Orientation

Góc xoay của văn bản.

---

## Rotation Angle

Góc xoay tính theo độ.

Ví dụ:

* 0°
* 90°
* 180°
* 270°

---

# 4. Goals

Text Direction hướng tới:

* ổn định
* chính xác
* đa ngôn ngữ
* độc lập Provider
* hỗ trợ văn bản hỗn hợp
* hỗ trợ Layout Analysis

---

# 5. Non-Goals

Không thực hiện:

* OCR
* Translation
* Grammar
* Reading Order
* Layout Grouping
* Font Recognition

---

# 6. Architecture Position

```text
Image

↓

Detection

↓

Recognition

↓

Text Direction

↓

Layout Analysis

↓

Reading Order

↓

Translation
```

---

# 7. High-Level Pipeline

```text
Recognition Result

↓

Direction Validation

↓

Orientation Detection

↓

Writing Mode Detection

↓

Line Direction Detection

↓

Paragraph Direction Detection

↓

Direction Metadata

↓

Direction Result
```

---

# 8. Direction Lifecycle

## Stage 1

Nhận Recognition Result.

---

## Stage 2

Phân tích Geometry.

---

## Stage 3

Ước lượng Orientation.

---

## Stage 4

Xác định Writing Mode.

---

## Stage 5

Xác định Direction.

---

## Stage 6

Sinh Direction Metadata.

---

# 9. Inputs

Text Direction nhận:

* Detection Result
* Recognition Result
* Geometry
* Character Position
* Line Position
* Paragraph Position

---

# 10. Outputs

Trả về:

* Direction Result
* Writing Mode
* Line Direction
* Paragraph Direction
* Rotation
* Confidence

---

# 11. Direction Result Model

```text
Direction Result

├── Metadata

├── Region Directions

│      ├── Writing Mode

│      ├── Line Direction

│      ├── Paragraph Direction

│      ├── Character Flow

│      ├── Rotation

│      └── Confidence

└── Statistics
```

---

# 12. Writing Modes

Hệ thống nên hỗ trợ tối thiểu:

* Horizontal Left-to-Right (LTR)
* Horizontal Right-to-Left (RTL)
* Vertical Top-to-Bottom (TTB)
* Vertical Bottom-to-Top (BTT)
* Mixed

Writing Mode là thuộc tính của Region hoặc Paragraph, không phải của toàn bộ trang.

---

# 13. Horizontal Text

Đặc điểm:

* Line nằm ngang
* Character sắp theo chiều ngang
* Khoảng cách giữa các dòng theo chiều dọc

Ví dụ:

* Tiếng Việt
* Tiếng Anh
* Tiếng Pháp
* Tiếng Đức

---

# 14. Vertical Text

Đặc điểm:

* Character xếp theo cột
* Line phát triển theo chiều dọc
* Các cột có thể đi từ phải sang trái hoặc trái sang phải

Ví dụ:

* Manga tiếng Nhật
* Tiếng Trung truyền thống
* Một số tài liệu lịch sử

---

# 15. Mixed Writing Mode

Một Region có thể chứa nhiều Writing Mode.

Ví dụ:

```text
東京
TOKYO
```

hoặc:

```text
こんにちは
ChatGPT
```

Trong trường hợp này, hệ thống phải lưu Direction riêng cho từng Line hoặc Block thay vì ép toàn bộ Region về một hướng.

---

# 16. Character Flow

Character Flow mô tả thứ tự xuất hiện của Character trong một Line.

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

Character Flow là nền tảng để xây dựng Word và Reading Order cục bộ.

---

# 17. Line Direction

Line Direction mô tả hướng phát triển của một Line.

Các giá trị khuyến nghị:

* LeftToRight
* RightToLeft
* TopToBottom
* BottomToTop
* Unknown

Line Direction không nhất thiết giống Paragraph Direction.

---

# 18. Paragraph Direction

Paragraph Direction mô tả hướng tổng thể của Paragraph.

Ví dụ:

Một Paragraph có thể gồm nhiều Line LTR nhưng toàn bộ Paragraph lại được sắp theo chiều từ trên xuống dưới.

Thông tin này đặc biệt quan trọng với manga và tài liệu đa ngôn ngữ.

---

# 19. Rotation Detection

Text có thể bị xoay do:

* ảnh chụp
* truyện nghiêng
* hiệu ứng đồ họa
* SFX

Hệ thống cần phát hiện góc xoay thay vì luôn giả định văn bản nằm ngang.

Rotation không làm thay đổi nội dung Recognition mà chỉ bổ sung Metadata.

---

# 20. Direction Confidence

Direction Confidence biểu diễn mức độ tin cậy của kết quả phân tích hướng.

Nên đánh giá riêng cho:

* Writing Mode
* Line Direction
* Paragraph Direction
* Rotation

Các Confidence này độc lập với Recognition Confidence và Detection Confidence để các bước sau có thể lựa chọn chiến lược xử lý phù hợp.
