# Runtime Architecture

> Project: CRAI
> Version: 1.0
> Status: Architecture Overview

---

# 1. Purpose

Thư mục `runtime/` định nghĩa toàn bộ kiến trúc Runtime của CRAI.

Runtime là lớp chịu trách nhiệm điều phối việc thực thi hệ thống trong thời gian chạy (runtime execution).

Runtime không thực hiện nghiệp vụ (business logic).

Runtime cũng không quyết định cách OCR, Translation hay bất kỳ Business Module nào hoạt động.

Thay vào đó Runtime chịu trách nhiệm:

- tạo và quản lý Session
- quản lý Revision
- điều phối WorkItem
- quản lý Attempt
- điều phối Scheduler
- quản lý Queue
- quản lý Resource
- quản lý Artifact
- quản lý Authority
- quản lý Publication
- quản lý Cancellation
- quản lý Retry
- quản lý Performance
- quản lý Observability
- quản lý Lifecycle

Runtime là lớp orchestration của toàn bộ hệ thống.

---

# 2. Scope

Runtime chịu trách nhiệm:

- Runtime lifecycle
- execution orchestration
- resource management
- scheduling
- queue management
- cancellation
- retry
- authority
- publication
- artifact lifecycle
- runtime configuration
- runtime diagnostics
- runtime performance

Runtime không chịu trách nhiệm:

- OCR algorithm
- Translation algorithm
- Manga parsing
- Novel parsing
- Business rules
- UI rendering
- Persistent business storage

Những phần này thuộc các module khác.

---

# 3. Runtime Philosophy

Runtime được xây dựng dựa trên các nguyên tắc sau:

- Runtime owns orchestration.
- Business owns semantics.
- Current Revision First.
- Authority is centralized.
- Publication is atomic.
- Resources are bounded.
- Queues are bounded.
- Cancellation is cooperative.
- Retry is cooperative.
- Cache is optional.
- Storage is optional for correctness.
- Performance is measured by useful results.
- Observability explains runtime decisions.

Runtime ưu tiên:

```text
Correct Current Result

↓

Responsive UI

↓

Predictable Runtime

↓

Efficient Resource Usage

↓

High Throughput
```

---

# 4. Runtime Architecture

```text
                    Runtime

                       │

        ┌──────────────┼──────────────┐

        │                             │

 Runtime Control              Runtime Components

        │                             │

        │                    Scheduler

        │                    Work Queue

        │                    Provider Manager

        │                    Resource Manager

        │                    Artifact Store

        │                    Session Manager

        │                    Presentation Runtime

        │                    Runtime Observability

        │

        ├──── Authority

        ├──── Publication

        ├──── Cancellation

        ├──── Retry

        ├──── Lifecycle

        └──── Configuration
```

Runtime Control là execution authority của toàn bộ Runtime.

---

# 5. Runtime Lifecycle

Một Runtime thông thường trải qua các giai đoạn:

```text
Boot

↓

Ready

↓

Session

↓

Revision

↓

WorkItem

↓

Attempt

↓

Publication

↓

Presentation

↓

Shutdown
```

Mỗi giai đoạn đều được định nghĩa trong các tài liệu tương ứng.

---

# 6. Core Vocabulary

Runtime sử dụng các thuật ngữ sau.

| Concept | Ý nghĩa |
|----------|----------|
| Session | Một phiên đọc |
| Revision | Trạng thái nội dung tại một thời điểm |
| WorkItem | Một đơn vị công việc logic |
| Attempt | Một lần thực thi WorkItem |
| Artifact | Kết quả đã được Runtime chấp nhận |
| Candidate Artifact | Kết quả trước khi publication |
| Authority | Quyền hợp lệ để tạo hoặc publish kết quả |
| Publication | Đưa Artifact vào Runtime |
| Lease | Quyền sử dụng tạm thời Resource |
| Retention | Giữ Resource sau khi hoàn thành |
| Resource | Bộ nhớ, GPU, Provider, File... |
| Runtime Control | Trung tâm điều phối Runtime |

Đây là vocabulary thống nhất của toàn bộ Runtime.

---

# 7. Runtime Layers

```text
Configuration

↓

Runtime Control

↓

Scheduling

↓

Execution

↓

Publication

↓

Presentation

↓

Observability
```

Business Module hoạt động bên trong Execution.

Runtime không phụ thuộc implementation của Business Module.

---

# 8. Runtime Documents

Thư mục Runtime bao gồm:

| Document | Mục đích |
|-----------|----------|
| `RUNTIME_COMPONENTS.md` | Danh sách Runtime Component và ownership |
| `BOOT_SEQUENCE.md` | Startup lifecycle |
| `PIPELINE_RUNTIME.md` | Runtime execution pipeline |
| `SCHEDULER.md` | Admission và scheduling |
| `WORK_QUEUE.md` | Queue architecture |
| `THREADING_MODEL.md` | Execution context |
| `RESOURCE_LIFECYCLE.md` | Resource lifecycle |
| `MEMORY_MODEL.md` | Runtime memory |
| `CACHE_POLICY.md` | Artifact reuse |
| `CANCELLATION.md` | Cancellation model |
| `RETRY_POLICY.md` | Retry policy |
| `PERFORMANCE_MODEL.md` | Runtime performance |
| `RUNTIME_OBSERVABILITY.md` | Metrics, Trace, Logs |
| `RUNTIME_CONFIG.md` | Runtime configuration |
| `ERROR_MODEL.md` | Runtime error model |

---

# 9. Recommended Reading Order

Để hiểu Runtime nên đọc theo thứ tự:

```text
README

↓

RUNTIME_COMPONENTS

↓

BOOT_SEQUENCE

↓

PIPELINE_RUNTIME

↓

SCHEDULER

↓

WORK_QUEUE

↓

THREADING_MODEL

↓

RESOURCE_LIFECYCLE

↓

MEMORY_MODEL

↓

CACHE_POLICY

↓

CANCELLATION

↓

RETRY_POLICY

↓

PERFORMANCE_MODEL

↓

RUNTIME_OBSERVABILITY

↓

RUNTIME_CONFIG

↓

ERROR_MODEL
```

Các tài liệu được thiết kế theo thứ tự phụ thuộc.

---

# 10. Relationship With Other Architectures

Runtime không hoạt động độc lập.

Runtime tương tác với:

```text
Core

↓

Runtime

↓

Business Modules

↓

Storage

↓

Presentation
```

Core cung cấp các primitive.

Business Module cung cấp nghiệp vụ.

Storage cung cấp persistence.

Presentation hiển thị kết quả.

Runtime điều phối toàn bộ quá trình.

---

# 11. MVP Scope

Runtime MVP bao gồm:

- single process
- single Runtime Container
- bounded queues
- cooperative cancellation
- bounded retry
- Artifact Store
- Resource Manager
- Runtime Control
- Scheduler
- local storage
- local diagnostics

Không bao gồm:

- distributed runtime
- multi-process runtime
- runtime plugin
- cloud scheduler
- distributed artifact store
- cluster execution

---

# 12. Future Evolution

Kiến trúc Runtime được thiết kế để có thể mở rộng trong tương lai mà không thay đổi các abstraction hiện tại.

Các hướng mở rộng có thể bao gồm:

- distributed execution
- multiple Runtime Process
- cloud provider orchestration
- remote execution
- distributed Artifact Store
- adaptive scheduling
- plugin runtime
- hybrid local/cloud execution

Những khả năng này không nằm trong phạm vi MVP nhưng đã được tính đến trong thiết kế hiện tại.

---

# 13. Design Goals

Runtime hướng tới:

- deterministic execution
- predictable lifecycle
- bounded resources
- observable behavior
- safe cancellation
- atomic publication
- scalable architecture
- implementation independence

Mọi tài liệu trong thư mục `runtime/` đều phải tuân thủ các nguyên tắc này.

---

# 14. Summary

Runtime là lớp orchestration của CRAI.

Runtime không thực hiện nghiệp vụ.

Runtime chịu trách nhiệm điều phối toàn bộ vòng đời của dữ liệu và quá trình xử lý, từ khi nội dung được quan sát cho đến khi kết quả được trình bày cho người dùng.

Các tài liệu trong thư mục này mô tả toàn bộ kiến trúc Runtime của CRAI theo Runtime v2 và là nền tảng cho mọi implementation trong tương lai.