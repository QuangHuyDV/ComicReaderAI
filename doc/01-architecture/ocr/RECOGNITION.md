# Text Recognition

> Status: Draft
> Version: 1.0
> Layer: OCR Pipeline
> Depends On: Detection
> Next Layer: Reading Order

---

# 1. Purpose

## Overview

Text Recognition là giai đoạn thứ hai trong OCR Pipeline.

Nếu Detection trả lời câu hỏi:

> **"Text nằm ở đâu?"**

thì Recognition trả lời:

> **"Text là gì?"**

Recognition nhận các Region đã được Detection phát hiện, phân tích nội dung bên trong từng Region và chuyển đổi hình ảnh thành dữ liệu văn bản có cấu trúc.

Recognition không chỉ tạo ra chuỗi ký tự mà còn xây dựng mô hình văn bản đầy đủ phục vụ Translation, Presentation và AI Processing.

---

## Objectives

Recognition phải:

* chuyển đổi hình ảnh thành văn bản
* giữ nguyên ý nghĩa của nội dung gốc
* hỗ trợ nhiều ngôn ngữ
* hỗ trợ nhiều kiểu bố cục
* tạo dữ liệu có cấu trúc
* cung cấp Confidence cho nhiều cấp độ
* độc lập với OCR Provider

---

## Responsibilities

Recognition chịu trách nhiệm:

* nhận Region từ Detection
* tiền xử lý Region nếu cần
* nhận dạng ký tự
* xác định ngôn ngữ
* xây dựng Paragraph
* xây dựng Line
* xây dựng Word
* xây dựng Character
* đánh giá Confidence
* sinh Recognition Result

Recognition không chịu trách nhiệm:

* Translation
* Grammar Correction
* Spell Checking
* Reading Order
* Text Layout
* Rendering

---

# 2. Scope

Recognition chỉ xử lý nội dung bên trong từng Region.

Recognition không quan tâm:

* vị trí của Region trên trang
* thứ tự đọc của trang
* ngữ nghĩa của câu
* dịch thuật

Các vấn đề trên thuộc module khác.

---

# 3. Terminology

## Recognition

Quá trình chuyển hình ảnh thành văn bản.

---

## Recognized Region

Kết quả nhận dạng của một Region.

---

## Character

Đơn vị nhỏ nhất của văn bản.

Ví dụ:

```text
你
A
あ
。
？
！
```

---

## Word

Một nhóm Character tạo thành một từ.

Ví dụ:

```text
Hello

Recognition

ChatGPT
```

Đối với một số ngôn ngữ như tiếng Trung hoặc tiếng Nhật, Word có thể không tồn tại rõ ràng.

---

## Line

Một dòng văn bản.

Line không phụ thuộc Paragraph.

---

## Paragraph

Một nhóm nhiều Line liên quan đến cùng một nội dung.

---

## Script

Hệ thống chữ viết.

Ví dụ:

* Latin
* Han
* Hiragana
* Katakana
* Hangul
* Cyrillic
* Arabic
* Thai

---

## Writing Mode

Cách trình bày văn bản.

Ví dụ:

* Horizontal
* Vertical
* Mixed

---

## Recognition Provider

Engine thực hiện OCR.

Ví dụ:

* PaddleOCR
* EasyOCR
* Tesseract
* Google Vision
* Azure OCR
* Custom AI Model

Recognition Layer không phụ thuộc Provider cụ thể.

---

# 4. Goals

Recognition được thiết kế nhằm đạt các mục tiêu sau.

## Accuracy

Nhận dạng đúng nội dung.

---

## Consistency

Cùng một đầu vào phải tạo ra cùng một Recognition Result.

---

## Extensibility

Có thể thay OCR Provider mà không ảnh hưởng Pipeline.

---

## Provider Independence

Pipeline chỉ làm việc với Recognition Contract.

Không làm việc trực tiếp với API của Provider.

---

## Structured Result

Recognition không trả về String đơn giản.

Recognition phải tạo ra dữ liệu có cấu trúc.

---

## Multi-language

Hỗ trợ đồng thời nhiều ngôn ngữ.

Ví dụ:

```text
你好 World!

こんにちは ChatGPT

한국어 English 日本語
```

---

## High Performance

Cho phép:

* Batch Recognition
* Parallel Recognition
* Incremental Recognition

---

# 5. Non-Goals

Recognition không thực hiện:

* Machine Translation
* NLP
* Text Summarization
* Context Understanding
* Image Captioning
* Character Identification
* Speech Bubble Analysis
* Reading Order

---

# 6. Architecture Position

Recognition nằm ngay sau Detection.

```text
Image

↓

Detection

↓

Recognition

↓

Reading Order

↓

Text Processing

↓

Translation
```

Recognition không truy cập trực tiếp Image Source.

Recognition chỉ làm việc với Detection Result.

---

# 7. Recognition Pipeline

Pipeline chuẩn gồm:

```text
Region

↓

Region Validation

↓

Image Crop

↓

Image Enhancement

↓

Writing Mode Detection

↓

Language Estimation

↓

Character Recognition

↓

Word Construction

↓

Line Construction

↓

Paragraph Construction

↓

Confidence Evaluation

↓

Recognition Result
```

Một số Provider có thể gộp nhiều bước thành một.

Pipeline nội bộ của CRAI vẫn giữ nguyên Contract.

---

# 8. Recognition Lifecycle

## Stage 1

Nhận Detection Result.

---

## Stage 2

Lọc Region không hợp lệ.

---

## Stage 3

Crop ảnh.

---

## Stage 4

Chuẩn hóa ảnh.

---

## Stage 5

Nhận dạng văn bản.

---

## Stage 6

Xây dựng cấu trúc.

---

## Stage 7

Đánh giá Confidence.

---

## Stage 8

Sinh Recognition Result.

---

# 9. Inputs

Recognition nhận:

* Detection Result
* Region Geometry
* Region Type
* Detection Metadata
* Detection Confidence
* Recognition Profile

Không nhận:

* Translation Result
* Reading Order
* Presentation Data

---

# 10. Outputs

Recognition trả về:

* Recognition Result
* Paragraph List
* Line List
* Word List
* Character List
* Confidence
* Metadata

Không trả về:

* Translation
* Reading Order
* Render Layout

---

# 11. Recognition Result Model

Recognition Result là Contract chuẩn giữa Recognition và các module phía sau.

```text
Recognition Result

├── Recognition Metadata

├── Region Results

│      ├── Region

│      ├── Paragraphs

│      ├── Lines

│      ├── Words

│      ├── Characters

│      ├── Language

│      ├── Script

│      ├── Writing Mode

│      ├── Confidence

│      └── Provider Metadata

└── Statistics
```

Recognition Result phải đủ giàu thông tin để các module phía sau không cần truy cập lại OCR Provider.

---

# 12. Recognition Document Model

Recognition không chỉ tạo String.

Recognition xây dựng một cây dữ liệu văn bản.

```text
Recognition Document

└── Region

      └── Paragraph

            └── Line

                  └── Word

                        └── Character
```

Mỗi cấp trong cây có ID độc lập.

Điều này cho phép:

* chỉnh sửa riêng từng Word
* thay thế Character
* render từng Line
* highlight Paragraph
* dịch từng Segment
* cache từng Region

---

# 13. Recognized Text

Recognized Text biểu diễn nội dung văn bản sau khi OCR hoàn tất.

Nội dung phải giữ nguyên theo ảnh gốc.

Recognition không tự ý sửa lỗi chính tả.

Không tự ý chuẩn hóa dấu câu.

Không tự ý thay đổi khoảng trắng nếu chưa có quy tắc rõ ràng.

---

# 14. Character Model

Character là đơn vị nhỏ nhất trong Recognition.

Mỗi Character nên bao gồm:

* Character ID
* Unicode Value
* Bounding Geometry
* Confidence
* Script
* Rotation
* Metadata

Character phải giữ liên kết với Region gốc để hỗ trợ Presentation và Debugging.

---

# 15. Word Model

Word là tập hợp các Character có mối liên hệ ngữ cảnh hoặc khoảng cách phù hợp.

Đối với các ngôn ngữ không có khái niệm từ rõ ràng (ví dụ tiếng Trung hoặc tiếng Nhật), Word có thể được Provider tạo ra hoặc để trống theo Recognition Profile.

Word nên lưu:

* Word ID
* Text
* Character List
* Geometry
* Confidence
* Language
* Metadata

---

# 16. Line Model

Line đại diện cho một dòng văn bản liên tục trong cùng Region.

Line có thể gồm nhiều Word hoặc nhiều Character tùy Writing Mode.

Thông tin tối thiểu:

* Line ID
* Text
* Geometry
* Direction
* Word List
* Confidence
* Metadata

---

# 17. Paragraph Model

Paragraph là tập hợp nhiều Line cùng thuộc một đơn vị nội dung.

Recognition chỉ xây dựng cấu trúc Paragraph trong phạm vi một Region.

Việc nối Paragraph giữa nhiều Region thuộc trách nhiệm của Reading Order và Text Processing.

Paragraph nên bao gồm:

* Paragraph ID
* Line List
* Text
* Geometry
* Language
* Confidence
* Metadata

---

# 18. Confidence Model

Recognition đánh giá độ tin cậy ở nhiều cấp thay vì chỉ một giá trị duy nhất.

Các cấp độ bao gồm:

* Character Confidence
* Word Confidence
* Line Confidence
* Paragraph Confidence
* Region Confidence
* Recognition Confidence

Confidence ở cấp cha không nhất thiết bằng trung bình của cấp con.

Thuật toán tổng hợp Confidence là trách nhiệm của Recognition Provider hoặc Recognition Engine, nhưng Recognition Contract phải luôn lưu đầy đủ các giá trị này để các module phía sau có thể đưa ra quyết định phù hợp.
