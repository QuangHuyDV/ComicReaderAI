# Provider Management

> **Project:** CRAI  
> **Module:** Provider Management  
> **Path:** `02-modules/provider-management/`  
> **Status:** Architecture Complete (Phase 1)

---

# 1. Overview

Provider Management là module chịu trách nhiệm quản lý toàn bộ hệ sinh thái AI Provider của CRAI.

Module này **không thực hiện Translation**, **không OCR**, **không chạy AI model**, **không Scheduling**.

Nó chịu trách nhiệm quyết định:

- Provider nào tồn tại.
- Model nào tồn tại.
- Capability nào được hỗ trợ.
- Provider nào đủ điều kiện sử dụng.
- Provider nào đang khả dụng.
- Provider nào đang bị Rate Limit.
- Provider nào đang khỏe.
- Provider nào nên được lựa chọn.
- Lease nào đang hoạt động.
- Local Model nào đã được load.
- Credential nào còn hợp lệ.

Có thể xem Provider Management là **Control Plane** của toàn bộ AI infrastructure trong CRAI.

Runtime là **Execution Plane**.

---

# 2. Responsibilities

Module này chịu trách nhiệm:

- Provider Registry
- Provider Configuration
- Provider Lifecycle
- Provider Model Registry
- Capability Registry
- Provider Selection
- Provider Lease Management
- Availability Evaluation
- Health Evaluation
- Rate Limit Management
- Quota Tracking
- Circuit Breaker
- Credential State
- Provider Client Lifecycle
- Local Model Lifecycle
- Provider Usage Aggregation
- Outcome Normalization

Module này **không** chịu trách nhiệm:

- Translation
- Recognition
- Reading Session
- Presentation
- Runtime Scheduling
- Runtime Worker
- Secret Storage
- GPU Scheduling
- OCR
- UI

---

# 3. Architecture Position

```text
                 Reading Session
                         │
                         ▼
                  Translation
                         │
                         ▼
               Provider Management
                         │
        ┌────────────────┴────────────────┐
        ▼                                 ▼
 Runtime Scheduler                 Provider Adapter
        │                                 │
        └──────────────┬──────────────────┘
                       ▼
             AI Provider / Local Model
```

Provider Management nằm giữa Capability Module và Runtime.

Nó quyết định:

- Chọn provider nào.
- Chọn model nào.
- Có được phép sử dụng hay không.
- Cấp quyền sử dụng (Lease).

Runtime quyết định:

- Khi nào chạy.
- Worker nào chạy.
- Retry.
- Cancellation.
- Queue.
- Resource Scheduling.

---

# 4. Core Responsibilities

Provider Management chịu trách nhiệm về các Domain sau.

| Domain | Owner |
|---------|-------|
| Provider | ✅ |
| Provider Model | ✅ |
| Capability | ✅ |
| Selection | ✅ |
| Lease | ✅ |
| Availability | ✅ |
| Health | ✅ |
| Rate Limit | ✅ |
| Quota | ✅ |
| Circuit Breaker | ✅ |
| Credential State | ✅ |
| Local Model State | ✅ |
| Provider Client | ✅ |
| Runtime Worker | ❌ Runtime |
| Translation Result | ❌ Translation |
| OCR Result | ❌ Recognition |
| Rendering | ❌ Presentation |

---

# 5. Core Concepts

## Provider

Một hệ thống AI độc lập.

Ví dụ:

- OpenAI
- Gemini
- Claude
- DeepSeek
- Ollama
- llama.cpp

---

## Provider Model

Một model cụ thể của provider.

Ví dụ:

```
gpt-5
gpt-5-mini
gemini-2.5-pro
claude-sonnet
qwen3
llama3
```

---

## Capability

Khả năng mà model hỗ trợ.

Ví dụ:

- Translation
- OCR
- Vision
- Image Understanding
- Structured Output
- Streaming
- Tool Calling

---

## Provider Selection

Quá trình lựa chọn Provider phù hợp nhất.

Selection xét tới:

- Capability
- Policy
- Availability
- Health
- Locality
- Privacy
- Cost
- Rate Limit
- Circuit
- Preference

---

## Provider Lease

Lease đại diện cho quyền sử dụng Provider trong một khoảng thời gian.

Lease tồn tại để:

- tránh race condition
- giới hạn lifetime
- quản lý cancellation
- quản lý quota
- quản lý ownership

---

## Provider Client

Provider Client là đối tượng giao tiếp với Provider.

Ví dụ:

```
OpenAI Client

Gemini Client

Claude Client

Ollama Client
```

---

## Local Model

Model chạy trực tiếp trên máy người dùng.

Ví dụ:

- Ollama
- llama.cpp
- GGUF
- MLX

---

# 6. Module Documents

Thư mục hiện bao gồm:

```
MODULE.md
```

Mô tả:

- Scope
- Responsibility
- Boundary
- Dependency
- Ownership
- Design Principles

---

```
CONTRACT.md
```

Định nghĩa:

- Commands
- Queries
- DTO
- Lease
- Execution Handle
- Capability
- Selection
- Provider Configuration

---

```
STATES.md
```

Bao gồm State Machine của:

- Provider
- Provider Model
- Lease
- Availability
- Health
- Circuit
- Local Model

---

```
EVENTS.md
```

Bao gồm:

- Event Envelope
- Event Ownership
- Event Versioning
- Event Ordering
- Integration Events
- Event Flow

---

```
ERRORS.md
```

Bao gồm:

- Error Contract
- Warning Contract
- Retryability
- Recovery Actions
- Error Mapping
- Security Errors
- Persistence Errors

---

# 7. Dependencies

Provider Management phụ thuộc:

```
Runtime
```

để:

- Resource Admission
- Execution
- Cancellation

---

```
Secret Boundary
```

để:

- Credential Resolution
- Credential Rotation

---

```
Provider Adapter
```

để:

- Provider Integration

---

```
Observability
```

để:

- Metrics
- Logging
- Tracing
- Audit

---

Provider Management **không phụ thuộc**:

- Translation
- Recognition
- Reading Session
- Presentation

Điều này giúp các Capability Module hoàn toàn độc lập với hệ thống Provider.

---

# 8. Consumers

Các module sử dụng Provider Management gồm:

```
Translation
```

để:

- Request Selection
- Request Lease
- Report Outcome

---

```
Recognition
```

để:

- Request OCR Provider
- Request Lease
- Report Outcome

---

```
Administration
```

để:

- Register Provider
- Update Provider
- Enable Provider
- Disable Provider

---

```
Observability
```

để:

- Subscribe Events
- Metrics
- Audit

---

# 9. High-Level Workflow

Luồng tiêu chuẩn:

```text
Translation Request

↓

Provider Selection

↓

Lease Granted

↓

Runtime Execution

↓

Outcome Feedback

↓

Usage Recorded

↓

Lease Released
```

---

Nếu Provider gặp lỗi:

```text
Execution Failure

↓

Outcome Feedback

↓

Health Evaluation

↓

Circuit Evaluation

↓

Availability Updated

↓

Future Selection Changes
```

---

Nếu Local Model:

```text
Install

↓

Validate

↓

Load

↓

Ready

↓

Busy

↓

Unload
```

---

# 10. Ownership Boundary

Provider Management là Owner của:

- Provider
- Provider Model
- Capability Metadata
- Lease
- Availability
- Health
- Circuit
- Credential State
- Local Model State
- Provider Usage

Không phải Owner của:

- Translation
- OCR
- Runtime Execution
- Reading Session
- UI
- Presentation

---

# 11. Public Surface

## Commands

- RegisterProvider
- UpdateProvider
- EnableProvider
- DisableProvider
- ArchiveProvider

- RequestProviderSelection

- RequestProviderLease

- ReleaseProviderLease

- RefreshCapabilities

- RefreshHealth

---

## Queries

- GetProvider
- GetProviderModel
- ListProviders
- ListModels

- GetLease
- ListActiveLeases

- GetAvailability
- GetHealth
- GetCircuit

- GetCapability

---

## Events

- Provider Events
- Model Events
- Selection Events
- Lease Events
- Availability Events
- Health Events
- Circuit Events
- Credential Events
- Local Model Events

---

# 12. Design Principles

Provider Management tuân theo các nguyên tắc:

- Provider Neutral
- Capability Neutral
- Lease Based
- Event Driven
- State Driven
- Immutable Events
- Explicit Ownership
- Query/Event Separation
- Runtime Independent
- Secret Isolation
- Stable Identity
- Versioned Contracts

---

# 13. Core Invariants

Module luôn đảm bảo:

✓ Provider Identity luôn ổn định

✓ Provider Model Identity luôn ổn định

✓ Lease luôn có Owner

✓ Lease luôn có Lifetime

✓ Selection luôn dựa trên Capability

✓ Availability không thay thế Health

✓ Health không thay thế Circuit

✓ Runtime không sở hữu Provider State

✓ Translation không sở hữu Provider State

✓ Secret không bao giờ xuất hiện trong Contract

✓ Event luôn Immutable

✓ Query luôn là Source of Truth

✓ Provider có thể thay thế mà không ảnh hưởng Translation

✓ Local Provider và Remote Provider có cùng abstraction

---

# 14. Related Architecture

Provider Management liên kết chặt chẽ với:

```
docs/architecture/STATE_MACHINE.md

docs/architecture/EVENT_BUS.md

docs/architecture/MODULE_DEPENDENCY.md

docs/architecture/DATA_FLOW.md
```

Runtime liên quan:

```
docs/architecture/runtime/
```

Capability Modules:

```
translation/
recognition/
reading-session/
presentation/
```

---

# 15. Future Extensions

Thiết kế hiện tại cho phép mở rộng:

- MCP Provider
- Agent Provider
- AI Marketplace
- Cloud GPU
- Edge Runtime
- Hybrid Runtime
- Smart Routing
- Provider Cost Optimization
- Provider Benchmark
- Shadow Execution
- A/B Testing
- Multi Provider Execution
- Dynamic Capability Discovery

không cần thay đổi kiến trúc lõi.

---

# 16. Current Status

Architecture Phase 1:

| Document | Status |
|----------|--------|
| MODULE.md | ✅ |
| CONTRACT.md | ✅ |
| STATES.md | ✅ |
| EVENTS.md | ✅ |
| ERRORS.md | ✅ |
| README.md | ✅ |

Provider Management hiện được xem là **hoàn thành ở mức Architecture Specification**.

Các module khác có thể sử dụng Provider Management như một nền tảng ổn định mà không cần biết implementation chi tiết của từng AI Provider.