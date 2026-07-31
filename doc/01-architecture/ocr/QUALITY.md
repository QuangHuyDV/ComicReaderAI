# OCR Quality Assessment

> Status: Draft
> Version: 1.0
> Layer: OCR Pipeline
> Depends On: Detection, Recognition, Layout Analysis, Text Direction, OCR Postprocessing
> Next Layer: Runtime Decision, Retry Strategy, Translation

---

# 1. Purpose

## Overview

OCR Quality Assessment là tầng đánh giá chất lượng của toàn bộ kết quả OCR.

Khác với Confidence được sinh ra bởi từng module riêng lẻ, Quality Assessment đánh giá **mức độ đáng tin cậy của toàn bộ OCR Document** từ góc nhìn của Pipeline.

Quality Assessment không thực hiện OCR.

Quality Assessment không sửa lỗi OCR.

Nhiệm vụ của nó là xác định:

* kết quả có đủ tốt hay không
* có cần Retry hay không
* có cần đổi OCR Provider hay không
* có thể chuyển sang Translation hay không

---

## Objectives

Quality Assessment phải:

* đánh giá chất lượng OCR
* tổng hợp Confidence
* phát hiện bất thường
* phát hiện dữ liệu thiếu
* sinh Quality Report
* hỗ trợ Runtime Decision

---

## Responsibilities

Quality Assessment chịu trách nhiệm:

* Quality Evaluation
* Confidence Aggregation
* Quality Scoring
* Quality Classification
* Issue Detection
* Recommendation Generation

Không chịu trách nhiệm:

* OCR
* Translation
* Auto Correction
* Reading Order
* Image Processing

---

# 2. Scope

Quality Assessment chỉ đánh giá dữ liệu OCR đã hoàn thành.

Không thao tác trên:

* ảnh
* OCR Provider
* UI
* người dùng

---

# 3. Terminology

## Quality

Mức độ tin cậy của OCR Document.

---

## Confidence

Độ tin cậy của một thành phần cụ thể.

Confidence không đồng nghĩa với Quality.

---

## Quality Score

Điểm đánh giá tổng thể của OCR Document.

---

## Quality Grade

Phân loại chất lượng.

Ví dụ:

* Excellent
* Good
* Fair
* Poor
* Failed

---

## Quality Issue

Một vấn đề làm giảm chất lượng OCR.

---

## Recommendation

Đề xuất dành cho Runtime.

Ví dụ:

* Retry
* Continue
* Switch Provider
* Skip Translation

---

# 4. Goals

Quality Assessment hướng tới:

* khách quan
* ổn định
* độc lập Provider
* dễ Benchmark
* dễ mở rộng
* dễ Debug

---

# 5. Non-Goals

Không thực hiện:

* OCR
* Image Enhancement
* Translation
* Spell Checking
* Grammar Correction
* NLP

---

# 6. Architecture Position

```text
Image

↓

OCR Pipeline

↓

OCR Document

↓

Quality Assessment

↓

Runtime Decision

↓

Reading Order

↓

Translation
```

---

# 7. High-Level Pipeline

```text
OCR Document

↓

Quality Validation

↓

Confidence Aggregation

↓

Issue Detection

↓

Quality Scoring

↓

Quality Classification

↓

Recommendation

↓

Quality Report
```

---

# 8. Assessment Lifecycle

## Stage 1

Nhận OCR Document.

---

## Stage 2

Kiểm tra dữ liệu.

---

## Stage 3

Tổng hợp Confidence.

---

## Stage 4

Phân tích chất lượng.

---

## Stage 5

Phân loại.

---

## Stage 6

Sinh Recommendation.

---

## Stage 7

Xuất Quality Report.

---

# 9. Inputs

Nhận:

* OCR Document
* OCR Metadata
* Processing Statistics
* OCR Profile

---

# 10. Outputs

Trả về:

* Quality Report
* Quality Score
* Quality Grade
* Recommendation
* Diagnostics

---

# 11. Quality Report Model

```text
Quality Report

├── Metadata

├── Overall Score

├── Overall Grade

├── Confidence Summary

├── Quality Issues

├── Recommendations

├── Diagnostics

└── Statistics
```

Quality Report là đầu ra chuẩn của tầng đánh giá chất lượng.

---

# 12. Quality Dimensions

Chất lượng OCR nên được đánh giá theo nhiều chiều độc lập.

Ví dụ:

* Detection Quality
* Recognition Quality
* Layout Quality
* Direction Quality
* Structural Quality
* Metadata Quality

Không nên chỉ sử dụng một điểm số duy nhất để đại diện cho toàn bộ hệ thống.

---

# 13. Confidence Aggregation

Quality Assessment tổng hợp Confidence từ:

* Character
* Word
* Line
* Paragraph
* Region
* Detection
* Recognition
* Direction

Việc tổng hợp không nhất thiết là trung bình cộng.

Thuật toán tổng hợp cần được định nghĩa trong Quality Profile.

---

# 14. Structural Validation

Đánh giá cấu trúc của OCR Document.

Ví dụ:

* Region thiếu Paragraph
* Paragraph không có Line
* Word không có Character
* Geometry bị lỗi
* Parent-Child Relationship không hợp lệ

Các lỗi cấu trúc cần được ghi nhận riêng với lỗi nhận dạng.

---

# 15. Quality Issues

Ví dụ các Issue:

* Missing Region
* Empty Paragraph
* Low Confidence
* Invalid Geometry
* Invalid Direction
* Duplicate Region
* Broken Hierarchy
* Missing Metadata

Một OCR Document có thể chứa nhiều Issue cùng lúc.

---

# 16. Quality Score

Quality Score là điểm số tổng hợp phục vụ Runtime.

Điểm số nên nằm trong một khoảng chuẩn hóa (ví dụ từ 0 đến 100 hoặc từ 0.0 đến 1.0).

Cách tính điểm không được phụ thuộc vào OCR Provider cụ thể.

---

# 17. Quality Grade

Ví dụ phân loại:

* Excellent
* Good
* Acceptable
* Poor
* Failed

Grade giúp Runtime đưa ra quyết định mà không cần phân tích toàn bộ Quality Report.

---

# 18. Recommendations

Quality Assessment có thể sinh các khuyến nghị như:

* Continue
* Retry OCR
* Retry Detection
* Retry Recognition
* Switch OCR Provider
* Skip Translation
* Request Higher Resolution
* Manual Review

Recommendation chỉ mang tính gợi ý, không trực tiếp điều khiển Pipeline.

---

# 19. Diagnostics & Benchmarking

Diagnostics phục vụ:

* Debug
* Benchmark
* So sánh Provider
* Phân tích hiệu năng
* Theo dõi Regression

Các chỉ số Benchmark nên được lưu riêng để không ảnh hưởng đến OCR Document.

---

# 20. Architecture Invariants

Quality Assessment phải luôn đảm bảo:

* không thay đổi OCR Document
* không chỉnh sửa nội dung Recognition
* không thay đổi Geometry
* không thay đổi Layout
* không thay đổi Direction
* chỉ đánh giá và sinh báo cáo
* Quality Report phải độc lập với OCR Provider
* Runtime có thể đưa ra quyết định chỉ dựa trên Quality Report mà không cần truy cập lại toàn bộ OCR Pipeline
