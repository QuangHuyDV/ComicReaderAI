# OCR Postprocessing

> Status: Draft
> Version: 1.0
> Layer: OCR Pipeline
> Depends On: Detection, Recognition, Layout Analysis, Text Direction
> Next Layer: Reading Order, Translation

---

# 1. Purpose

## Overview

OCR Postprocessing là giai đoạn chuẩn hóa và hợp nhất kết quả sau khi các bước OCR hoàn thành.

Đây là tầng cuối của OCR Pipeline trước khi dữ liệu được chuyển sang Reading Order và Translation.

Postprocessing không thực hiện OCR mới mà làm sạch, chuẩn hóa và thống nhất toàn bộ dữ liệu OCR thành một mô hình duy nhất.

Nếu:

* Detection trả lời:

> "Text nằm ở đâu?"

* Recognition trả lời:

> "Text là gì?"

* Layout Analysis trả lời:

> "Các Region được tổ chức như thế nào?"

thì Postprocessing trả lời:

> "Làm thế nào để toàn bộ kết quả OCR trở nên nhất quán và sẵn sàng cho các bước xử lý tiếp theo?"

---

## Objectives

Postprocessing phải:

* hợp nhất kết quả OCR
* chuẩn hóa dữ liệu
* loại bỏ dữ liệu không hợp lệ
* bổ sung Metadata
* đảm bảo tính nhất quán
* sinh OCR Document chuẩn

---

## Responsibilities

Postprocessing chịu trách nhiệm:

* Validation
* Data Normalization
* Result Merging
* Metadata Completion
* Consistency Checking
* OCR Document Generation

Không chịu trách nhiệm:

* OCR
* Machine Translation
* Grammar Correction
* Reading Order
* Rendering

---

# 2. Scope

Postprocessing chỉ thao tác trên dữ liệu OCR đã được sinh ra.

Không xử lý trực tiếp:

* ảnh gốc
* OCR Provider
* UI
* Presentation

---

# 3. Terminology

## OCR Document

Mô hình dữ liệu chuẩn của toàn bộ kết quả OCR.

---

## Normalization

Quá trình chuẩn hóa dữ liệu về cùng một định dạng.

---

## Validation

Kiểm tra tính hợp lệ của dữ liệu.

---

## Merge

Hợp nhất nhiều kết quả thành một mô hình thống nhất.

---

## Consistency

Đảm bảo mọi thành phần trong OCR Document không mâu thuẫn với nhau.

---

## Metadata

Thông tin bổ sung phục vụ Runtime, Debugging và Benchmark.

---

# 4. Goals

Postprocessing hướng tới:

* dữ liệu ổn định
* dễ mở rộng
* độc lập Provider
* dễ Debug
* dễ Serialize
* dễ Cache
* dễ truyền giữa các Module

---

# 5. Non-Goals

Không thực hiện:

* OCR
* AI Translation
* NLP
* Reading Order
* Semantic Analysis
* Layout Detection

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

Translation
```

---

# 7. High-Level Pipeline

```text
Detection Result

+

Recognition Result

+

Layout Result

+

Direction Result

↓

Validation

↓

Normalization

↓

Merge

↓

Consistency Check

↓

Metadata Completion

↓

OCR Document

↓

Pipeline Output
```

---

# 8. Processing Lifecycle

## Stage 1

Thu thập toàn bộ OCR Result.

---

## Stage 2

Kiểm tra tính hợp lệ.

---

## Stage 3

Chuẩn hóa dữ liệu.

---

## Stage 4

Hợp nhất dữ liệu.

---

## Stage 5

Hoàn thiện Metadata.

---

## Stage 6

Sinh OCR Document.

---

# 9. Inputs

Nhận:

* Detection Result
* Recognition Result
* Layout Result
* Direction Result
* OCR Profile

---

# 10. Outputs

Trả về:

* OCR Document
* OCR Metadata
* Processing Statistics
* Validation Report

---

# 11. OCR Document Model

```text
OCR Document

├── Metadata

├── Page

│     ├── Panels

│     ├── Containers

│     ├── Blocks

│     └── Regions

├── Recognition

├── Layout

├── Direction

├── Statistics

└── Diagnostics
```

OCR Document là Contract chuẩn giữa OCR Pipeline và các module phía sau.

---

# 12. Result Validation

Trước khi hợp nhất dữ liệu, hệ thống cần kiểm tra:

* Region hợp lệ
* Geometry hợp lệ
* Character hợp lệ
* Layout hợp lệ
* Direction hợp lệ
* Metadata đầy đủ

Các lỗi cần được ghi nhận trong Validation Report thay vì làm hỏng toàn bộ Pipeline khi có thể phục hồi.

---

# 13. Data Normalization

Mục tiêu là đưa toàn bộ dữ liệu từ các OCR Provider về cùng một mô hình thống nhất.

Các nội dung cần chuẩn hóa bao gồm:

* ID
* Geometry
* Confidence
* Language Code
* Script Code
* Direction
* Writing Mode
* Metadata

Sau bước này, các module phía sau không cần biết dữ liệu đến từ Provider nào.

---

# 14. Result Merging

Postprocessing hợp nhất:

* Detection Result
* Recognition Result
* Layout Result
* Direction Result

thành một OCR Document duy nhất.

Quá trình Merge không được làm mất liên kết giữa các thành phần.

Ví dụ:

* Region phải giữ liên kết với Paragraph.
* Paragraph phải giữ liên kết với Line.
* Line phải giữ liên kết với Word.
* Word phải giữ liên kết với Character.

---

# 15. Consistency Checking

Sau khi Merge, hệ thống cần kiểm tra:

* Region tồn tại trong Layout.
* Character thuộc đúng Word.
* Word thuộc đúng Line.
* Line thuộc đúng Paragraph.
* Geometry không mâu thuẫn.
* Direction phù hợp với Recognition.
* Parent-Child Relationship hợp lệ.

Nếu phát hiện bất nhất, hệ thống nên đánh dấu thay vì tự động sửa khi chưa có đủ căn cứ.

---

# 16. Metadata Completion

Bổ sung Metadata phục vụ các bước sau.

Ví dụ:

* OCR Version
* Provider Version
* Pipeline Version
* Processing Time
* Language Summary
* Script Summary
* Confidence Summary
* Creation Time
* Processing Profile

Metadata không làm thay đổi dữ liệu OCR.

---

# 17. Statistics Generation

Sinh thống kê phục vụ Benchmark và Monitoring.

Ví dụ:

* số Region
* số Paragraph
* số Line
* số Word
* số Character
* thời gian xử lý
* bộ nhớ sử dụng
* Confidence trung bình
* số lỗi Validation

Statistics không được sử dụng để thay thế dữ liệu OCR.

---

# 18. Diagnostics

Diagnostics lưu thông tin phục vụ Debug.

Ví dụ:

* Warning
* Validation Error
* Merge Conflict
* Provider Message
* Retry Information

Diagnostics nên được tách khỏi dữ liệu OCR chính để giảm ảnh hưởng đến Runtime.

---

# 19. Serialization

OCR Document phải có khả năng:

* Serialize
* Deserialize
* Cache
* Compress
* Truyền qua Network

Việc tuần tự hóa không được làm mất Metadata hoặc thay đổi ID của các thực thể.

---

# 20. Architecture Invariants

OCR Postprocessing phải luôn đảm bảo:

* không thay đổi nội dung văn bản đã Recognition
* không thay đổi Geometry đã Detection
* không thay đổi Layout đã phân tích
* không thay đổi Direction đã xác định
* chỉ chuẩn hóa và hợp nhất dữ liệu
* tạo đúng một OCR Document cho mỗi phiên xử lý
* OCR Document phải độc lập với OCR Provider
* OCR Document phải đủ thông tin để Reading Order, Translation và Presentation hoạt động mà không cần truy cập lại các kết quả trung gian hoặc OCR Provider
