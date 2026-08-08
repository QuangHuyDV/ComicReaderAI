# 01-architecture/ocr/README.md

# OCR Architecture

## Purpose

Thư mục `01-architecture/ocr/` mô tả kiến trúc tổng thể của toàn bộ hệ thống OCR trong CRAI.

OCR được hiểu là toàn bộ quá trình chuyển đổi dữ liệu hình ảnh thành văn bản có cấu trúc, không chỉ riêng bước nhận dạng ký tự.

Kiến trúc này độc lập với:

* OCR Engine
* AI Provider
* Framework
* Runtime Implementation

Mục tiêu là định nghĩa **kiến trúc chuẩn (Canonical OCR Architecture)** để mọi implementation đều tuân theo cùng một mô hình.

---

# Scope

OCR Architecture trả lời các câu hỏi:

* OCR pipeline gồm những bước nào?
* Mỗi bước có trách nhiệm gì?
* Dữ liệu thay đổi như thế nào giữa các bước?
* Các bước phụ thuộc nhau ra sao?
* OCR Provider cần đáp ứng capability gì?
* Kết quả OCR được chuẩn hóa như thế nào?

OCR Architecture **không** mô tả:

* business workflow
* scheduling
* retry policy
* resource lifecycle
* event bus
* telemetry implementation
* logging implementation
* UI behavior

Những nội dung này thuộc các phần khác của hệ thống.

---

# Position in Architecture

```text
User Input
      │
      ▼
Capture
      │
      ▼
OCR Architecture
      │
      ▼
Recognized Source Text
      │
      ▼
Text Processing
      │
      ▼
Translation
      │
      ▼
Presentation
```

OCR là cầu nối giữa:

```text
Image

↓

Structured Source Text
```

---

# OCR Documents

Thư mục này được chia thành nhiều tài liệu nhỏ.

Mỗi tài liệu chỉ sở hữu **một phần của kiến trúc**.

---

## README.md

Giới thiệu toàn bộ OCR Architecture.

Là điểm bắt đầu để đọc các tài liệu khác.

---

## PIPELINE.md

Định nghĩa:

* Canonical OCR Pipeline
* thứ tự các stage
* input/output của từng stage
* execution flow

Không mô tả chi tiết thuật toán của từng stage.

---

## PREPROCESS.md

Định nghĩa toàn bộ preprocessing.

Ví dụ:

* resize
* denoise
* contrast
* threshold
* grayscale
* orientation correction
* image normalization

Chỉ quan tâm đến việc chuẩn bị ảnh.

Không thực hiện text detection.

---

## DETECTION.md

Định nghĩa:

* text region detection
* bounding box
* polygon
* text block
* speech bubble
* region confidence

Kết quả của Detection là:

```text
Image Regions
```

Không nhận dạng nội dung chữ.

---

## RECOGNITION.md

Định nghĩa:

* OCR recognition
* language model
* provider interface
* recognition result
* candidate handling

Đầu ra là:

```text
Recognized Source Text
```

Không xử lý nghĩa của văn bản.

---

## LAYOUT.md

Định nghĩa cấu trúc hình học của trang.

Ví dụ:

* page
* panel
* region
* bubble
* line

Đây là spatial structure.

Không quyết định thứ tự đọc.

---

## READING_ORDER.md

Định nghĩa:

* reading order
* region ordering
* panel ordering
* manga
* comic
* webtoon

Đầu ra là:

```text
Ordered Regions
```

---

## TEXT_DIRECTION.md

Định nghĩa:

* horizontal
* vertical
* RTL
* LTR
* mixed direction
* writing orientation

Không thực hiện translation.

---

## POSTPROCESS.md

Xử lý kết quả OCR.

Ví dụ:

* unicode normalization
* whitespace cleanup
* punctuation cleanup
* duplicated symbols
* provider artifact cleanup

Không sửa nghĩa của văn bản.

---

## QUALITY.md

Đánh giá chất lượng OCR.

Bao gồm:

* confidence
* validation
* completeness
* quality metrics

Đầu ra là:

```text
OCR Quality
```

---

## PROVIDERS.md

Định nghĩa abstraction của OCR Provider.

Ví dụ:

* Tesseract
* PaddleOCR
* EasyOCR
* Google Vision
* Azure OCR
* Gemini Vision

Architecture chỉ quan tâm capability.

Không phụ thuộc implementation cụ thể.

---

# Relationship Between Documents

```text
PIPELINE
    │
    ├── PREPROCESS
    ├── DETECTION
    ├── RECOGNITION
    ├── LAYOUT
    ├── READING_ORDER
    ├── TEXT_DIRECTION
    ├── POSTPROCESS
    ├── QUALITY
    └── PROVIDERS
```

`PIPELINE.md` là tài liệu trung tâm.

Các tài liệu còn lại mô tả chi tiết từng stage.

---

# Ownership

Mỗi tài liệu có một owner rõ ràng.

| Document       | Owns                     |
| -------------- | ------------------------ |
| PIPELINE       | Luồng OCR tổng thể       |
| PREPROCESS     | Image preprocessing      |
| DETECTION      | Text region detection    |
| RECOGNITION    | Character recognition    |
| LAYOUT         | Spatial layout           |
| READING_ORDER  | Reading order            |
| TEXT_DIRECTION | Text direction           |
| POSTPROCESS    | OCR output normalization |
| QUALITY        | OCR quality evaluation   |
| PROVIDERS      | OCR provider abstraction |

Một khái niệm chỉ nên được định nghĩa chi tiết tại **một** tài liệu.

Các tài liệu khác chỉ tham chiếu tới owner của khái niệm đó.

---

# Relationship with Runtime

OCR Architecture mô tả **logic xử lý OCR**.

Runtime chịu trách nhiệm:

* scheduling
* cancellation
* retry
* concurrency
* resource management
* execution orchestration

OCR không sở hữu các cơ chế này.

---

# Relationship with Infrastructure

Infrastructure cung cấp:

* Resource Manager
* Scheduler
* Logging
* Telemetry
* Event Bus
* Configuration

OCR chỉ sử dụng thông qua public contract.

OCR không định nghĩa lại các cơ chế Infrastructure.

---

# Relationship with Business Modules

OCR Architecture được sử dụng chủ yếu bởi:

```text
Recognition
```

Business Module chịu trách nhiệm:

* khi nào OCR được gọi
* OCR dùng cho workflow nào
* xử lý kết quả OCR

OCR Architecture chỉ định nghĩa **cách OCR hoạt động**, không quyết định **khi nào OCR được sử dụng**.

---

# Design Principles

Toàn bộ OCR Architecture phải tuân thủ các nguyên tắc:

* Provider Independent
* Deterministic Pipeline
* Immutable Artifacts
* Explicit Stage Boundary
* Capability Based
* Observable
* Replaceable
* Testable

Không stage nào được phụ thuộc trực tiếp vào implementation của stage khác.

---

# Reading Order

Khuyến nghị đọc theo thứ tự:

```text
README.md

↓

PIPELINE.md

↓

PREPROCESS.md

↓

DETECTION.md

↓

RECOGNITION.md

↓

LAYOUT.md

↓

READING_ORDER.md

↓

TEXT_DIRECTION.md

↓

POSTPROCESS.md

↓

QUALITY.md

↓

PROVIDERS.md
```

Mỗi tài liệu mở rộng đúng một phần của Pipeline.

---

# Architecture Boundary

OCR Architecture kết thúc tại:

```text
Recognized Source Text
```

Sau điểm này:

```text
Text Processing

↓

Translation

↓

Presentation
```

thuộc các Capability khác.

---

# Out of Scope

OCR Architecture không chịu trách nhiệm:

* Translation
* Text Processing
* Rendering
* Overlay
* Browser Automation
* User Interaction
* Scheduler
* Runtime State
* Resource Lifecycle
* Event Routing
* Logging
* Telemetry

Các nội dung này được định nghĩa ở các tài liệu tương ứng.

---

# Summary

Thư mục `01-architecture/ocr/` mô tả kiến trúc OCR chuẩn của CRAI.

Mỗi tài liệu chỉ chịu trách nhiệm cho **một khía cạnh** của OCR nhằm:

* giảm trùng lặp
* tăng khả năng bảo trì
* dễ mở rộng
* dễ thay thế provider
* giữ ranh giới trách nhiệm rõ ràng

`README.md` đóng vai trò là bản đồ của toàn bộ nhóm tài liệu OCR và là điểm bắt đầu trước khi đi vào từng tài liệu chi tiết.
