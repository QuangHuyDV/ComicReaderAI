# OCR Providers

> Status: Draft
> Version: 1.0
> Layer: OCR Infrastructure
> Used By: OCR Pipeline
> Related: Detection, Recognition, OCR Profile

---

# 1. Purpose

## Overview

OCR Providers là tầng trừu tượng hóa (Abstraction Layer) giữa OCR Pipeline và các OCR Engine bên ngoài.

Mục tiêu của tầng này là cho phép CRAI thay đổi, kết hợp hoặc mở rộng OCR Engine mà không ảnh hưởng đến các module còn lại.

Toàn bộ Pipeline phải làm việc với **OCR Provider Contract**, không làm việc trực tiếp với SDK hoặc API của từng nhà cung cấp.

---

## Objectives

OCR Providers phải:

* chuẩn hóa giao diện OCR
* che giấu sự khác biệt giữa các Provider
* hỗ trợ nhiều Provider đồng thời
* hỗ trợ thay thế Provider
* hỗ trợ Retry
* hỗ trợ Failover
* hỗ trợ Benchmark

---

## Responsibilities

OCR Providers chịu trách nhiệm:

* Provider Discovery
* Provider Selection
* Provider Routing
* Request Mapping
* Response Mapping
* Capability Management
* Error Mapping

Không chịu trách nhiệm:

* Translation
* Reading Order
* Layout Analysis
* Presentation
* Business Logic

---

# 2. Scope

OCR Provider Layer chỉ là cầu nối giữa Pipeline và OCR Engine.

Pipeline không được gọi trực tiếp:

* SDK
* REST API
* CLI
* AI Model

của từng Provider.

---

# 3. Terminology

## Provider

Một hệ thống có khả năng thực hiện OCR.

Ví dụ:

* PaddleOCR
* Tesseract
* EasyOCR
* Google Vision
* Azure AI Vision
* AWS Textract
* OCR.Space
* Custom AI Model

---

## Provider Adapter

Thành phần chuyển đổi giữa Contract của CRAI và API của Provider.

---

## Provider Contract

Interface chuẩn mà mọi Provider đều phải triển khai.

---

## Provider Capability

Tập hợp các tính năng mà Provider hỗ trợ.

---

## Provider Profile

Cấu hình sử dụng của một Provider.

---

## Provider Registry

Danh sách các Provider đã được đăng ký trong hệ thống.

---

# 4. Goals

OCR Provider Layer hướng tới:

* Provider Independence
* Extensibility
* Hot Swapping
* Testability
* Maintainability
* High Availability

---

# 5. Non-Goals

Không thực hiện:

* Translation
* OCR Quality Assessment
* Layout Analysis
* Reading Order
* Image Editing

---

# 6. Architecture Position

```text
OCR Pipeline

↓

OCR Provider Layer

↓

Provider Adapter

↓

OCR Engine

↓

OCR Result

↓

Provider Adapter

↓

OCR Pipeline
```

Pipeline không giao tiếp trực tiếp với OCR Engine.

---

# 7. High-Level Architecture

```text
               OCR Pipeline
                     │
                     ▼
             OCR Provider Layer
                     │
      ┌──────────────┼──────────────┐
      ▼              ▼              ▼
 Paddle Adapter   Google Adapter   Azure Adapter
      │              │              │
      ▼              ▼              ▼
 PaddleOCR      Google Vision    Azure Vision
```

Mỗi Adapter chịu trách nhiệm chuyển đổi dữ liệu giữa CRAI và Provider tương ứng.

---

# 8. Provider Lifecycle

## Stage 1

Khởi tạo Provider.

---

## Stage 2

Đăng ký vào Registry.

---

## Stage 3

Kiểm tra Capability.

---

## Stage 4

Nhận Request.

---

## Stage 5

Thực hiện OCR.

---

## Stage 6

Chuyển đổi Result.

---

## Stage 7

Trả về Pipeline.

---

# 9. Provider Contract

Mỗi Provider phải hỗ trợ tối thiểu:

* Initialize
* Health Check
* Detect
* Recognize
* Shutdown

Các chức năng mở rộng là tùy chọn nhưng không được phá vỡ Contract chung.

---

# 10. Provider Registry

Registry quản lý toàn bộ Provider có sẵn.

Registry nên lưu:

* Provider ID
* Name
* Version
* Status
* Capabilities
* Priority
* Configuration

Registry là nguồn thông tin duy nhất để Pipeline lựa chọn Provider.

---

# 11. Provider Adapter

Adapter chuyển đổi giữa:

* Request của CRAI
* Request của Provider

và

* Response của Provider
* OCR Document chuẩn

Adapter không chứa Business Logic.

---

# 12. Capability Model

Capability mô tả những gì Provider có thể thực hiện.

Ví dụ:

* Detection
* Recognition
* Multi-language
* Vertical Text
* Handwriting
* Table Detection
* Layout Detection
* GPU Support
* Batch Processing
* Streaming

Capability giúp Runtime lựa chọn Provider phù hợp với từng tình huống.

---

# 13. Provider Selection

Runtime có thể lựa chọn Provider dựa trên:

* OCR Profile
* Language
* Script
* Image Type
* Region Type
* Quality Requirement
* Performance Requirement

Việc lựa chọn phải độc lập với phần còn lại của Pipeline.

---

# 14. Provider Routing

Provider Routing quyết định Provider nào sẽ xử lý một yêu cầu OCR.

Các chiến lược phổ biến:

* Default Provider
* Rule-based Routing
* Capability-based Routing
* Priority Routing
* Weighted Routing
* Load-based Routing

Routing Strategy có thể thay đổi mà không cần sửa Pipeline.

---

# 15. Multi-Provider Strategy

Một yêu cầu OCR có thể sử dụng nhiều Provider.

Ví dụ:

* Provider A thực hiện Detection.
* Provider B thực hiện Recognition.
* Provider C thực hiện Layout Analysis.

Hoặc nhiều Provider cùng xử lý một Region để so sánh kết quả.

Pipeline chỉ nhận một OCR Document thống nhất sau khi Postprocessing hoàn tất.

---

# 16. Failover & Retry

Nếu Provider gặp lỗi, Runtime có thể:

* Retry cùng Provider
* Chuyển sang Provider khác
* Giảm Capability
* Hủy tác vụ

Cơ chế Failover phải minh bạch với các module phía trên.

---

# 17. Provider Health

Mỗi Provider nên cung cấp trạng thái hoạt động.

Ví dụ:

* Initializing
* Ready
* Busy
* Degraded
* Unavailable
* Shutting Down

Health Status giúp Runtime tránh gửi yêu cầu tới Provider không khả dụng.

---

# 18. Error Mapping

Mỗi Provider có hệ thống lỗi riêng.

Provider Layer phải chuyển đổi các lỗi này về Error Model thống nhất của CRAI.

Ví dụ:

* Timeout
* Invalid Request
* Unsupported Language
* Resource Exhausted
* Internal Error
* Authentication Failed

Pipeline không nên xử lý lỗi đặc thù của từng Provider.

---

# 19. Benchmark & Metrics

Provider Layer nên thu thập các chỉ số như:

* Latency
* Throughput
* Success Rate
* Retry Count
* Failure Rate
* Average Confidence
* Memory Usage
* GPU Usage

Các chỉ số này phục vụ:

* Benchmark
* Monitoring
* Auto Routing
* Capacity Planning

---

# 20. Architecture Invariants

OCR Provider Layer phải luôn đảm bảo:

* Pipeline chỉ giao tiếp với Provider Contract.
* Adapter không chứa Business Logic.
* Có thể thay thế Provider mà không thay đổi Pipeline.
* Mọi Provider đều phải chuyển đổi về OCR Document chuẩn của CRAI.
* Không phụ thuộc SDK hoặc API cụ thể ở các tầng phía trên.
* Có thể mở rộng thêm Provider mới mà không sửa mã nguồn của các module hiện có.
* Một Provider lỗi không được làm hỏng toàn bộ OCR Pipeline nếu còn Provider thay thế phù hợp.
