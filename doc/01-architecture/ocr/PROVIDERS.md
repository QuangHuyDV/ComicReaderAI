# OCR Providers

> **Status:** Draft
> **Version:** 1.1
> **Layer:** OCR Provider Integration
> **Used By:** OCR Pipeline
> **Related:** Detection, Recognition, OCR Profile, Runtime Provider Selection

---

# 1. Purpose

OCR Provider Layer là abstraction boundary giữa CRAI OCR Architecture và các OCR Engine cụ thể.

Mục tiêu:

```text
CRAI OCR Contract
        ↓
Provider Adapter
        ↓
Provider-native API / SDK / Model
```

Pipeline chỉ làm việc với contract chuẩn của CRAI.

Pipeline không phụ thuộc trực tiếp vào:

* SDK
* REST API
* CLI
* native model interface
* provider-specific response schema

của bất kỳ OCR Provider nào.

---

# 2. Scope

OCR Provider Layer chịu trách nhiệm:

* Provider abstraction
* Provider Adapter boundary
* Provider discovery metadata
* Provider capability declaration
* Provider registration metadata
* request mapping
* response normalization
* provider-specific error mapping
* provider-specific health reporting
* provider metadata isolation

OCR Provider Layer không chịu trách nhiệm:

* Translation
* Reading Order
* Layout semantics
* Quality Assessment
* business workflow
* Runtime scheduling
* Runtime retry execution
* Runtime failover execution
* Runtime cancellation
* global resource lifecycle
* Event Bus semantics

---

# 3. Goals

OCR Provider Layer hướng tới:

* Provider Independence
* Replaceability
* Extensibility
* Testability
* Capability-based integration
* explicit adapter boundaries
* stable CRAI contracts
* provider-specific isolation
* support for multiple providers

---

# 4. Non-Goals

Provider Layer không:

* sở hữu OCR business semantics
* tự quyết định Runtime retry
* tự quyết định fallback execution
* tự sửa OCR Document
* tự quyết định Reading Order
* thực hiện Translation
* expose provider-native model lên downstream
* chứa business logic

---

# 5. Terminology

## OCR Provider

Một implementation có khả năng cung cấp một hoặc nhiều OCR capability.

Ví dụ:

* PaddleOCR
* Tesseract
* EasyOCR
* Google Vision
* Azure AI Vision
* AWS Textract
* custom local model
* custom remote model

---

## Provider Adapter

Thành phần chuyển đổi giữa CRAI contract và provider-native interface.

---

## Provider Contract

Interface chuẩn mà CRAI sử dụng để giao tiếp với Provider.

---

## Provider Capability

Mô tả những gì Provider có thể thực hiện.

---

## Provider Profile

Configuration mô tả cách một Provider được sử dụng.

---

## Provider Descriptor

Metadata mô tả một Provider đã được đăng ký.

---

## Provider Registry

Danh sách các Provider Descriptor có sẵn trong runtime.

Registry không mặc định là decision authority.

---

# 6. Architecture Position

```text
OCR Pipeline
      │
      ▼
OCR Provider Contract
      │
      ▼
Provider Adapter
      │
      ▼
OCR Engine / API / Model
      │
      ▼
Provider-native Result
      │
      ▼
Provider Adapter
      │
      ▼
CRAI OCR Result
```

Provider-specific semantics phải kết thúc tại Adapter boundary.

---

# 7. High-Level Architecture

```text
                OCR Pipeline
                     │
                     ▼
             Provider Contract
                     │
          ┌──────────┼──────────┐
          ▼          ▼          ▼
       Adapter A   Adapter B   Adapter C
          │          │          │
          ▼          ▼          ▼
      PaddleOCR    Tesseract   Cloud OCR
```

Mỗi Adapter độc lập.

Thêm Provider mới không được yêu cầu sửa OCR Pipeline core nếu capability contract vẫn tương thích.

---

# 8. Provider Integration Flow

```text
Provider Descriptor
      │
      ▼
Registration
      │
      ▼
Capability Discovery
      │
      ▼
Provider Available
      │
      ▼
CRAI Request
      │
      ▼
Request Mapping
      │
      ▼
Provider Invocation
      │
      ▼
Response Mapping
      │
      ▼
CRAI Result
```

Đây là integration flow.

Execution lifecycle chi tiết thuộc Runtime.

---

# 9. Provider Contract

Provider Contract nên hỗ trợ các capability ở mức abstraction.

Một Provider có thể triển khai một tập con như:

```text
Initialize
HealthCheck
Detect
Recognize
Shutdown
```

Không phải mọi Provider đều phải hỗ trợ cả Detection và Recognition.

Capability availability phải explicit.

---

# 10. Contract Design Rule

Không nên giả định:

```text
every provider supports everything
```

Thay vào đó:

```text
Provider
    ↓
declares Capabilities
```

Runtime/Pipeline chỉ sử dụng capability được Provider khai báo hỗ trợ.

---

# 11. Provider Descriptor

Provider Descriptor có thể chứa:

```text
Provider ID
Name
Version
Adapter Version
Capabilities
Configuration Reference
Health State
Priority Hint
Metadata
```

Descriptor không nên chứa secret trực tiếp.

Secret ownership thuộc Secret Management.

---

# 12. Provider Registry

Registry quản lý Provider Descriptor.

Registry có thể hỗ trợ:

* register
* unregister
* lookup
* list
* capability filtering
* health filtering

Registry là nguồn discovery.

Không mặc định là nơi quyết định Provider cuối cùng cho một WorkItem.

---

# 13. Registry Boundary

Registry trả lời:

```text
Which Providers are available?
```

Không nhất thiết trả lời:

```text
Which Provider must execute this task?
```

Execution selection authority thuộc Runtime/provider-selection policy tương ứng.

---

# 14. Provider Adapter

Adapter chịu trách nhiệm:

```text
CRAI Request
    ↓
Provider-native Request
```

và:

```text
Provider-native Response
    ↓
CRAI Result
```

Adapter không chứa business logic.

---

# 15. Request Mapping

Request Mapping có thể chuyển:

* image artifact reference
* Region
* language hint
* script hint
* writing mode hint
* OCR profile settings
* provider capability option

sang provider-specific request.

Provider-native fields không được leak ngược lên OCR Architecture.

---

# 16. Response Mapping

Response Mapping phải normalize:

* text
* Region geometry
* Character/Word/Line structure
* confidence
* language/script
* provider metadata
* error state

về CRAI contract tương ứng.

---

# 17. Output Contract Boundary

Adapter không được trả trực tiếp:

* SDK object
* raw REST response
* provider-specific enum
* provider-specific geometry class
* provider-specific error object

cho OCR Pipeline.

---

# 18. Capability Model

Provider Capability mô tả những capability mà Provider hỗ trợ.

Ví dụ:

```text
Detection
Recognition
MultiLanguage
VerticalText
Handwriting
TableDetection
LayoutHint
Batch
Streaming
GPU
LocalExecution
RemoteExecution
```

Capability nên được biểu diễn theo semantics của CRAI.

---

# 19. Capability Ownership

Provider chỉ khai báo:

```text
I support capability X
```

Nó không sở hữu semantics của capability đó.

Ví dụ:

```text
Detection
```

semantics vẫn thuộc `DETECTION.md`.

```text
Recognition
```

semantics vẫn thuộc `RECOGNITION.md`.

---

# 20. Capability Compatibility

Provider capability có thể chứa version hoặc constraint.

Ví dụ:

```text
Recognition:
    languages = [zh, en]
    verticalText = true
```

Runtime/Pipeline có thể dùng metadata này để lọc Provider phù hợp.

---

# 21. Provider Profile

Provider Profile có thể chứa:

* enabled/disabled
* configuration reference
* model selection
* endpoint selection
* timeout hint
* supported language overrides
* capability overrides
* privacy compatibility
* local/remote classification

Profile không được chứa secret plaintext nếu Secret Management có thể cung cấp reference.

---

# 22. Provider Selection Inputs

Provider selection có thể xem xét:

* required capability
* OCR Profile
* language
* script
* image type
* Region Type
* privacy requirement
* quality requirement
* availability
* health
* performance hint

Provider Layer chỉ cung cấp metadata.

Selection execution thuộc owner của Runtime/provider routing.

---

# 23. Provider Routing Boundary

Provider Routing trả lời:

```text
Which available Provider is suitable?
```

Nhưng Provider Adapter không tự điều phối toàn bộ pipeline.

Routing strategy có thể là:

* Default
* Capability-based
* Priority-based
* Rule-based
* Health-aware
* Cost-aware
* Performance-aware
* Privacy-aware

Strategy execution phải giữ Runtime authority.

---

# 24. Multi-Provider Architecture

CRAI có thể sử dụng nhiều Provider trong cùng OCR flow.

Ví dụ:

```text
Detection
    → Provider A

Recognition
    → Provider B
```

hoặc:

```text
Region A
    → Provider A

Region B
    → Provider B
```

Downstream vẫn chỉ nhận CRAI contracts.

---

# 25. Provider Combination

Nhiều Provider có thể cùng hỗ trợ một stage.

Ví dụ:

```text
Recognition Candidate A
Recognition Candidate B
```

Việc so sánh hoặc lựa chọn output cuối cùng không mặc định thuộc Adapter.

Nó phải do OCR strategy/Quality/Runtime policy tương ứng sở hữu.

---

# 26. Detection Integration

Provider có capability:

```text
Detection
```

phải map output về:

```text
Detection Result
```

theo `DETECTION.md`.

Adapter không redefine Region semantics.

---

# 27. Recognition Integration

Provider có capability:

```text
Recognition
```

phải map output về:

```text
Recognition Result
```

theo `RECOGNITION.md`.

Adapter không redefine Character/Word/Line/Paragraph semantics.

---

# 28. Layout Hints

Một Provider có thể trả layout information.

Thông tin này chỉ được coi là:

```text
Provider Layout Hint
```

cho tới khi được normalize vào CRAI Layout semantics.

Authoritative Layout vẫn thuộc `LAYOUT.md`.

---

# 29. Direction Hints

Provider có thể trả:

* orientation
* writing mode
* line direction

Những giá trị này phải normalize thành hint.

Authoritative Text Direction thuộc `TEXT_DIRECTION.md`.

---

# 30. Provider Health

Provider có thể cung cấp health information.

Recommended semantic states:

```text
Unknown
Initializing
Ready
Degraded
Unavailable
ShuttingDown
```

Health state chỉ cho biết khả năng phục vụ.

Nó không tự kích hoạt retry/fallback.

---

# 31. Health Ownership Boundary

Provider Adapter sở hữu việc:

```text
report provider health
```

Runtime/Resource Manager sở hữu việc:

```text
decide what to do with that health state
```

---

# 32. Error Mapping

Provider-specific errors phải được normalize.

Ví dụ native errors:

```text
HTTP 429
CUDA out of memory
SDK AuthenticationError
model unsupported-language
```

có thể map sang CRAI categories như:

```text
Timeout
InvalidRequest
UnsupportedLanguage
ResourceExhausted
Unavailable
AuthenticationFailed
InternalProviderError
```

---

# 33. Error Boundary

Public OCR layers không được phụ thuộc provider-specific exception class.

Adapter phải giữ original cause trong diagnostics khi cần, nhưng public error semantics phải provider-neutral.

---

# 34. Retry Boundary

Provider có thể báo error là:

```text
retryable
```

hoặc đưa retry hint.

Nhưng Provider không tự sở hữu:

* retry budget
* retry attempt
* delay/backoff
* WorkItem creation
* execution scheduling

Những phần này thuộc Runtime.

---

# 35. Failover Boundary

Provider có thể báo:

```text
Unavailable
UnsupportedCapability
Degraded
```

Runtime có thể dùng các signal đó để fallback.

Provider Adapter không tự chuyển toàn bộ workflow sang Provider khác.

---

# 36. Provider Replacement

Một Provider mới có thể thay Provider cũ nếu:

* contract tương thích
* required capabilities tương thích
* normalized semantics giữ nguyên
* Pipeline không cần biết provider implementation detail

Đây là mục tiêu chính của Provider abstraction.

---

# 37. Local Providers

Local Provider có thể:

* chạy CPU
* chạy GPU
* sử dụng local model
* không gửi dữ liệu ra network

Ví dụ:

* Tesseract
* PaddleOCR
* local custom model

Local không đồng nghĩa luôn nhanh hơn hoặc tốt hơn.

---

# 38. Remote Providers

Remote Provider có thể:

* sử dụng REST API
* cloud AI service
* remote inference

Remote provider phải tuân:

* Privacy Profile
* Secret Management
* network policy
* request minimization

---

# 39. Privacy Classification

Provider Descriptor nên cho biết:

```text
Local
Remote
Hybrid
```

hoặc capability tương đương.

Runtime có thể từ chối Remote Provider khi input được đánh dấu local-only.

Provider Adapter không được bỏ qua privacy classification.

---

# 40. Secret Management

Provider credential không được lưu trực tiếp trong:

* OCR Profile
* Provider Descriptor public metadata
* Event
* Diagnostics
* Log

Provider Adapter lấy secret thông qua Secret Management abstraction.

---

# 41. Provider Metadata

Result có thể giữ:

* Provider ID
* Provider Version
* Model ID
* Model Version
* Adapter Version
* request capability
* optional provider diagnostics

Metadata phải được sanitize.

---

# 42. Provider Metadata Boundary

Downstream có thể dùng Provider Metadata cho:

* diagnostics
* reproducibility
* compatibility
* benchmark correlation

Nhưng business/semantic logic không nên phụ thuộc provider-native fields.

---

# 43. Provider Metrics

Provider Layer có thể expose measurements như:

* latency
* success rate
* failure rate
* health
* request count
* capability usage
* provider error count
* local resource usage where relevant

Telemetry transport thuộc Infrastructure.

---

# 44. Benchmark Boundary

Provider metrics có thể được benchmark system sử dụng.

Provider Layer không sở hữu toàn bộ Benchmark framework.

Nó chỉ cung cấp measurements/provider identity.

---

# 45. Performance Hints

Provider Descriptor có thể chứa hints như:

* expected latency class
* batch capability
* GPU support
* streaming support

Những hints hỗ trợ Runtime selection.

Chúng không phải hard performance guarantees.

---

# 46. Provider Versioning

Provider integration phải lưu:

* Provider Version
* Adapter Version
* Model Version khi có
* Capability Version khi cần

Thay đổi provider/model có thể ảnh hưởng compatibility của OCR result.

---

# 47. Adapter Versioning

Nếu Adapter mapping thay đổi semantics:

```text
Provider Response
    ↓
CRAI Contract
```

thì Adapter Version phải thay đổi.

Điều này hỗ trợ reproducibility và cache compatibility.

---

# 48. Determinism

Provider có thể không deterministic hoàn toàn.

Adapter phải giữ:

* exact Provider ID
* model/version
* request semantic identity
* relevant provider settings

để output có thể được phân tích và so sánh.

---

# 49. Mock Provider

Provider Contract phải cho phép Mock/Fake Provider.

Điều này hỗ trợ:

* unit tests
* integration tests
* deterministic fixtures
* offline development

Pipeline test không nên luôn phụ thuộc cloud OCR thật.

---

# 50. Unsupported Capability

Nếu Provider không hỗ trợ required capability:

```text
UnsupportedCapability
```

phải được trả explicit.

Không giả vờ chạy capability với output kém xác định.

---

# 51. Partial Capability

Một Provider có thể chỉ hỗ trợ:

```text
Recognition
```

mà không hỗ trợ:

```text
Detection
```

hoặc ngược lại.

Provider architecture phải hỗ trợ composition.

---

# 52. Graceful Degradation

Provider có thể báo một capability bị unavailable trong khi capability khác vẫn usable.

Ví dụ:

```text
Detection = Ready
Recognition = Unavailable
```

Health/capability status nên đủ granular nếu implementation hỗ trợ.

---

# 53. Runtime Integration

Provider Layer không sở hữu:

* WorkItem
* Attempt
* Scheduler
* retry budget
* cancellation authority
* stale-result authority
* final provider routing authority

Runtime sở hữu execution.

---

# 54. Resource Integration

Local Provider có thể cần:

* model session
* GPU context
* worker
* HTTP client
* process

Resource lifecycle thuộc Resource Manager/Runtime.

Provider Adapter chỉ sử dụng resource thông qua contract phù hợp.

---

# 55. Cache Integration

Provider-specific compatibility có thể phụ thuộc:

* Provider ID
* Provider Version
* Model Version
* Adapter Version
* semantic request settings

Những thông tin này có thể tham gia cache compatibility.

Global cache lifecycle thuộc Runtime.

---

# 56. Event Integration

Provider Layer có thể tạo semantic facts như:

```text
ProviderRegistered
ProviderHealthChanged
ProviderUnavailable
ProviderInvocationFailed
```

Domain meaning thuộc Provider Integration.

Event transport/envelope thuộc Event Bus.

---

# 57. Observability Integration

Useful fields:

* Provider ID
* Capability
* Model Version
* request duration
* normalized error category
* health state
* local/remote classification

Không log:

* API key
* token
* raw authorization header

---

# 58. Security and Privacy

Provider Integration phải luôn đảm bảo:

* secret không leak
* remote request tuân Privacy Profile
* raw private content không log mặc định
* provider response được sanitize khi cần
* local-only content không gửi remote

---

# 59. Architecture Invariants

OCR Provider Layer phải luôn đảm bảo:

1. OCR Pipeline chỉ giao tiếp với CRAI Provider Contract.

2. Provider Adapter không chứa business logic.

3. Provider-native request/response không crossing public boundary.

4. Provider-specific errors phải được normalize.

5. Provider-specific enum không trở thành OCR Architecture enum.

6. Provider capability phải explicit.

7. Một Provider không cần hỗ trợ toàn bộ OCR capabilities.

8. Detection semantics vẫn thuộc `DETECTION.md`.

9. Recognition semantics vẫn thuộc `RECOGNITION.md`.

10. Layout semantics vẫn thuộc `LAYOUT.md`.

11. Text Direction semantics vẫn thuộc `TEXT_DIRECTION.md`.

12. Provider health chỉ là signal, không phải Runtime decision.

13. Provider retryability chỉ là hint, không phải retry execution.

14. Provider Layer không sở hữu Runtime retry.

15. Provider Layer không sở hữu Runtime failover execution.

16. Provider Layer không sở hữu Runtime scheduling.

17. Provider Layer không sở hữu cancellation authority.

18. Provider credential phải đi qua Secret Management.

19. Provider metadata phải optional đối với downstream semantic logic.

20. Provider mới có thể được thêm mà không sửa OCR Pipeline core khi contract tương thích.

21. Provider result phải normalize về CRAI contract trước khi downstream sử dụng.

22. Runtime có thể compose nhiều Provider theo capability.

---

# 60. Recommended MVP Provider Layer

MVP nên giữ đơn giản:

```text
OCR Pipeline
    ↓
Provider Contract
    ↓
One Primary Adapter
    ↓
Primary OCR Engine
```

MVP nên hỗ trợ:

* Provider Descriptor
* Provider Registry
* Capability declaration
* Detect
* Recognize
* basic Health Check
* request mapping
* response normalization
* error mapping
* provider identity/version
* Secret Management integration

Optional MVP:

```text
Secondary Provider
```

để fallback khi thực sự cần.

Không cần ngay:

* weighted provider routing
* automatic cost optimization
* distributed provider fleet
* learned provider selection
* multi-provider voting
* complex capacity balancing

---

# 61. Ownership References

| Concern               | Owner                      |
| --------------------- | -------------------------- |
| OCR Pipeline          | `PIPELINE.md`              |
| Detection Contract    | `DETECTION.md`             |
| Recognition Contract  | `RECOGNITION.md`           |
| Text Direction        | `TEXT_DIRECTION.md`        |
| Layout                | `LAYOUT.md`                |
| OCR Document          | `POSTPROCESS.md`           |
| Quality               | `QUALITY.md`               |
| Provider Contract     | `PROVIDERS.md`             |
| Provider Adapter      | `PROVIDERS.md`             |
| Provider Capabilities | `PROVIDERS.md`             |
| Retry Execution       | Runtime                    |
| Failover Execution    | Runtime                    |
| Scheduling            | Runtime                    |
| Resource Lifecycle    | Runtime / Resource Manager |
| Secret Storage        | Secret Management          |
| Cache Lifecycle       | Runtime                    |
| Event Transport       | Event Bus                  |
| Telemetry Transport   | Infrastructure             |

---

# 62. Summary

OCR Provider Layer chuyển:

```text
CRAI OCR Request
```

thành:

```text
Provider-native Request
```

và:

```text
Provider-native Result
```

thành:

```text
CRAI OCR Contract
```

Architecture tổng quát:

```text
OCR Pipeline
      ↓
Provider Contract
      ↓
Provider Adapter
      ↓
OCR Engine
```

Provider chỉ chịu trách nhiệm:

```text
capability
+
adaptation
+
normalization
+
provider-specific integration
```

Runtime chịu trách nhiệm:

```text
selection execution
+
retry
+
failover
+
scheduling
+
resource coordination
```

Nguyên tắc cốt lõi:

```text
Provider declares capability.

Adapter translates representation.

OCR Architecture defines semantics.

Runtime decides execution.
```
