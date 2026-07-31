# Image Preprocessing

> Status: Draft
> Version: 1.0
> Layer: OCR Pipeline
> Depends On: Image Source
> Next Layer: Detection

---

# 1. Purpose

## Overview

Image Preprocessing là giai đoạn đầu tiên của OCR Pipeline.

Nhiệm vụ của Preprocessing là chuẩn hóa hình ảnh trước khi đưa vào Detection nhằm tăng độ chính xác và tính ổn định của toàn bộ Pipeline.

Preprocessing không tạo ra dữ liệu OCR mà chỉ cải thiện chất lượng ảnh.

Nếu:

* Detection trả lời:

> "Text nằm ở đâu?"

* Recognition trả lời:

> "Text là gì?"

thì Preprocessing trả lời:

> "Làm thế nào để ảnh đạt trạng thái tốt nhất cho OCR?"

---

## Objectives

Preprocessing phải:

* chuẩn hóa ảnh
* giảm nhiễu
* tăng độ tương phản
* cân bằng sáng
* sửa góc nghiêng
* chuẩn hóa kích thước
* giữ nguyên nội dung ảnh
* độc lập OCR Provider

---

## Responsibilities

Preprocessing chịu trách nhiệm:

* đọc ảnh
* chuẩn hóa định dạng
* hiệu chỉnh chất lượng
* chuẩn hóa màu sắc
* chuẩn hóa độ phân giải
* chuẩn hóa Orientation
* tạo ảnh đầu ra cho Detection

Không chịu trách nhiệm:

* OCR
* Detection
* Recognition
* Translation
* Rendering

---

# 2. Scope

Preprocessing chỉ thao tác trên dữ liệu ảnh.

Không xử lý:

* Character
* Word
* Paragraph
* Region
* Layout
* Reading Order

---

# 3. Terminology

## Source Image

Ảnh đầu vào của OCR Pipeline.

---

## Working Image

Ảnh đang được xử lý trong Pipeline.

---

## Processed Image

Ảnh đầu ra sau Preprocessing.

---

## Enhancement

Các phép cải thiện chất lượng ảnh.

---

## Normalization

Quá trình đưa ảnh về trạng thái chuẩn.

---

## ROI (Region of Interest)

Vùng ảnh cần xử lý.

ROI có thể là:

* toàn bộ ảnh
* một phần ảnh
* Region được chỉ định

---

## Image Profile

Tập hợp các cấu hình điều khiển Preprocessing.

---

# 4. Goals

Preprocessing hướng tới:

* chất lượng ổn định
* tốc độ cao
* không làm mất dữ liệu
* khả năng mở rộng
* tái sử dụng
* hoạt động độc lập với OCR Engine

---

# 5. Non-Goals

Không thực hiện:

* OCR
* Text Detection
* Character Recognition
* Translation
* Image Editing
* Image Restoration chuyên sâu
* AI Enhancement ngoài Pipeline

---

# 6. Architecture Position

```text
Image Source

↓

Image Preprocessing

↓

Detection

↓

Recognition

↓

Layout Analysis

↓

Reading Order
```

---

# 7. High-Level Pipeline

```text
Image Input

↓

Validation

↓

Format Conversion

↓

Orientation Correction

↓

Resolution Normalization

↓

Noise Reduction

↓

Contrast Enhancement

↓

Color Normalization

↓

ROI Processing

↓

Processed Image
```

---

# 8. Processing Lifecycle

## Stage 1

Nhận ảnh.

---

## Stage 2

Kiểm tra định dạng.

---

## Stage 3

Chuẩn hóa Orientation.

---

## Stage 4

Chuẩn hóa kích thước.

---

## Stage 5

Khử nhiễu.

---

## Stage 6

Tăng chất lượng.

---

## Stage 7

Sinh Processed Image.

---

# 9. Inputs

Preprocessing nhận:

* Image
* Image Metadata
* Processing Profile
* ROI (tùy chọn)

---

# 10. Outputs

Trả về:

* Processed Image
* Image Metadata
* Processing Metadata
* Statistics

---

# 11. Processing Result Model

```text
Processing Result

├── Processed Image

├── Image Metadata

├── Applied Operations

├── Quality Metrics

└── Statistics
```

---

# 12. Image Validation

Trước khi xử lý, hệ thống cần kiểm tra:

* định dạng
* kích thước
* khả năng đọc
* độ sâu màu
* dữ liệu hỏng
* metadata cần thiết

Ảnh không hợp lệ phải bị từ chối trước khi vào Pipeline.

---

# 13. Image Format Normalization

Preprocessing cần chuẩn hóa nhiều định dạng ảnh về một Working Format thống nhất.

Các định dạng phổ biến:

* PNG
* JPEG
* WebP
* BMP
* TIFF

Việc chuẩn hóa giúp các bước sau không cần quan tâm đến định dạng gốc.

---

# 14. Orientation Correction

Ảnh có thể:

* bị xoay
* bị lật
* sai EXIF Orientation

Preprocessing cần đưa ảnh về Orientation chuẩn trước khi Detection.

Không thay đổi nội dung hiển thị của ảnh ngoài việc hiệu chỉnh hướng.

---

# 15. Resolution Normalization

Ảnh có độ phân giải quá thấp có thể làm giảm Accuracy.

Ảnh quá lớn lại làm tăng chi phí xử lý.

Preprocessing nên chuẩn hóa độ phân giải theo Processing Profile, đồng thời giữ nguyên tỷ lệ khung hình (Aspect Ratio).

---

# 16. Noise Reduction

Mục tiêu:

* giảm nhiễu cảm biến
* giảm nhiễu nén JPEG
* loại bỏ điểm ảnh bất thường
* giảm hạt ảnh

Không được làm mất nét Character hoặc làm biến dạng đường viền văn bản.

---

# 17. Contrast Enhancement

Tăng độ tương phản giữa văn bản và nền.

Có thể áp dụng:

* Global Contrast
* Adaptive Contrast
* Histogram Equalization
* CLAHE (nếu phù hợp với Profile)

Việc tăng tương phản phải đảm bảo không làm mất chi tiết vùng sáng hoặc vùng tối.

---

# 18. Brightness & Color Normalization

Chuẩn hóa:

* Brightness
* Gamma
* White Balance
* Color Space

Mục tiêu là tạo điều kiện thuận lợi cho Detection mà không làm thay đổi nội dung ảnh.

Đối với OCR thông thường, hệ thống có thể chuyển sang Grayscale nếu Processing Profile cho phép.

---

# 19. ROI Processing

Trong nhiều trường hợp, chỉ một phần ảnh cần OCR.

Preprocessing phải hỗ trợ xử lý theo ROI để:

* giảm thời gian xử lý
* giảm tiêu thụ bộ nhớ
* hỗ trợ OCR thời gian thực
* hỗ trợ dịch màn hình theo vùng

ROI có thể được xác định bởi:

* người dùng
* ứng dụng
* Detection trước đó
* Pipeline Runtime

---

# 20. Processing Metadata

Mỗi bước xử lý nên được ghi nhận dưới dạng Metadata.

Ví dụ:

* ảnh gốc
* kích thước trước/sau
* Orientation
* Color Space
* Resolution
* các phép biến đổi đã áp dụng
* thời gian xử lý
* Processing Profile
* phiên bản thuật toán

Metadata giúp:

* Debug
* Benchmark
* Reproduce kết quả
* So sánh giữa các Processing Profile
* Phân tích hiệu năng của toàn bộ OCR Pipeline
