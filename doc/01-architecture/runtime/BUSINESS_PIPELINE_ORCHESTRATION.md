# runtime/BUSINESS_PIPELINE_ORCHESTRATION.md

# Business Pipeline Orchestration

## 1. Purpose

Tài liệu này định nghĩa cách CRAI chuyển một **business request** thành một **Business Execution Plan** gồm các Business Stage cần thiết, dependency giữa chúng và điều kiện đầu vào/đầu ra của từng stage.

Business Pipeline Orchestration trả lời câu hỏi:

> Với use case hiện tại, hệ thống cần thực hiện những bước nghiệp vụ nào và theo thứ tự logic nào?

Tài liệu này không mô tả cách WorkItem được queue, schedule, retry, cancel hoặc thực thi vật lý. Những nội dung đó thuộc `PIPELINE_RUNTIME.md` và các tài liệu Runtime chuyên biệt.

---

## 2. Rename Decision

Tên cũ:

```text
PIPELINE_ORCHESTRATION.md
```

Tên mới:

```text
BUSINESS_PIPELINE_ORCHESTRATION.md
```

Tên mới được chọn để tránh nhầm lẫn giữa:

```text
Business Pipeline Orchestration
    → xác định business workflow và execution plan

Pipeline Runtime
    → thực thi execution plan bằng Revision, WorkItem và Attempt
```

---

## 3. Architectural Position

```text
User / System Intent
        ↓
Application Use Case
        ↓
Business Pipeline Orchestration
        ↓
Business Execution Plan
        ↓
Pipeline Runtime
        ↓
Runtime Control
        ↓
Scheduler / Worker Execution
        ↓
Business Module Contracts
```

Business Pipeline Orchestration nằm giữa Application Use Case và Pipeline Runtime.

Nó không phải Runtime Control, Scheduler hoặc Worker.

---

## 4. Core Separation

CRAI phân biệt ba lớp:

### 4.1 Business Architecture

Trả lời:

> Hệ thống có những capability và Business Module nào?

Ví dụ:

```text
Capture
Recognition
Text Processing
Translation
Presentation
Storage
Reading
```

### 4.2 Business Pipeline Orchestration

Trả lời:

> Use case này cần những Business Stage nào, dependency nào và output nào?

Kết quả là một `BusinessExecutionPlan`.

### 4.3 Pipeline Runtime

Trả lời:

> Execution plan được thực thi, schedule, cancel, retry và xác nhận kết quả như thế nào?

Pipeline Runtime sử dụng:

```text
SessionId
RevisionId
WorkItemId
AttemptId
ArtifactRef
```

---

## 5. Responsibilities

Business Pipeline Orchestration chịu trách nhiệm:

- nhận business request đã được Application xác nhận;
- xác định pipeline variant phù hợp;
- chọn tập Business Stage tối thiểu cần thiết;
- xác định dependency giữa các Business Stage;
- xác định required input và expected output;
- xác định stage bắt buộc và stage tùy chọn;
- xác định các nhánh có thể chạy song song về mặt logic;
- xác định partial-result boundary;
- xác định reusable-output boundary;
- xác định presentation target;
- tạo immutable `BusinessExecutionPlan`;
- version execution plan;
- chuyển execution plan cho Pipeline Runtime.

---

## 6. Non-Responsibilities

Business Pipeline Orchestration không:

- thực thi Capture, Recognition, Translation hoặc Presentation;
- quản lý thread hoặc process;
- sở hữu queue;
- quyết định Scheduler admission;
- tạo Attempt;
- tự retry;
- điều phối cancellation vật lý;
- quản lý timeout;
- quản lý provider concurrency;
- lưu Artifact payload;
- xác nhận stale result;
- chấp nhận terminal outcome;
- commit UI;
- sở hữu business data của module;
- sở hữu durable persistence;
- thay Event Bus hoặc Runtime Control.

---

## 7. Business Stage Definition

`BusinessStage` là một bước logic trong business workflow, tương ứng với trách nhiệm công khai của một Business Module hoặc một use-case boundary đã được định nghĩa rõ.

Ví dụ:

```text
Acquire Content
Recognize Content
Build Source Document
Translate Source Document
Prepare Presentation
Commit Presentation
```

Business Stage không đồng nghĩa với capability nội bộ.

Các khái niệm sau không mặc định là Business Stage:

```text
OCR
Reading Order
Layout Detection
Segmentation
Normalization
Bubble Detection
Provider Call
Cache Lookup
```

Những capability này có thể được module sở hữu và được Pipeline Runtime triển khai thành một hoặc nhiều WorkItem.

---

## 8. Business Modules and Stage Ownership

Business Stage phải có đúng một owner chính.

| Business Stage | Primary owner |
|---|---|
| Acquire structured text or visual source | Capture |
| Recognize visual or structured content | Recognition |
| Build translation-ready source document | Text Processing |
| Produce translation result | Translation |
| Prepare render-ready presentation | Presentation |
| Manage reading intent and session meaning | Reading |
| Persist durable state when explicitly requested | Storage |

Ownership của stage không được chuyển sang Orchestrator.

Orchestrator chỉ tổ chức dependency giữa các stage.

---

## 9. Business Request

Pipeline planning bắt đầu từ một `BusinessRequest`.

Ví dụ khái niệm:

```text
BusinessRequest
├── requestType
├── sessionId
├── sourceDescriptor
├── requestedOutput
├── languageIntent
├── presentationIntent
├── privacyMode
├── userPriority
└── requestMetadata
```

`BusinessRequest` phải mô tả ý định nghiệp vụ, không chứa runtime execution detail như thread, queue hoặc worker identity.

---

## 10. Pipeline Variant

`PipelineVariant` mô tả một loại workflow nghiệp vụ đã được định nghĩa trước.

Các variant ban đầu:

```text
TEXT_READING
IMAGE_READING
CLIPBOARD_TEXT
CLIPBOARD_IMAGE
MANUAL_IMAGE_TRANSLATION
RETRANSLATION
PRESENTATION_REFRESH
RESTORED_READING_SESSION
EXPORT
```

Variant không phải provider profile và không phải implementation strategy.

---

## 11. Business Execution Plan

Kết quả của orchestration là một immutable `BusinessExecutionPlan`.

Ví dụ conceptual model:

```text
BusinessExecutionPlan
├── planId
├── planVersion
├── pipelineVariant
├── sourceIntent
├── requestedOutput
├── stages[]
├── dependencies[]
├── optionalStages[]
├── reusableOutputs[]
├── partialDeliveryPolicy
├── presentationTarget
└── planMetadata
```

Execution plan không chứa:

- active worker;
- queue position;
- retry count;
- cancellation token implementation;
- provider connection;
- mutable Artifact payload;
- terminal runtime state.

---

## 12. Business Stage Plan

Mỗi stage trong plan được mô tả bằng `BusinessStagePlan`.

```text
BusinessStagePlan
├── stageId
├── stageType
├── ownerModule
├── requiredInputs[]
├── expectedOutputs[]
├── dependencies[]
├── optional
├── reusePolicyRef
├── partialOutputPolicy
└── stageConfigurationRef
```

`BusinessStagePlan` là logical declaration.

Pipeline Runtime có thể chuyển một Business Stage thành một hoặc nhiều WorkItem.

---

## 13. Stage Graph

Business Execution Plan được biểu diễn như một Directed Acyclic Graph trong phạm vi một lần planning.

```text
Stage
  ↓
Stage
  ├── Stage
  └── Stage
```

Quy tắc:

- dependency phải explicit;
- graph không được có cycle;
- stage không tự gọi stage tiếp theo;
- stage không biết implementation của stage khác;
- stage chỉ nhận input contract đã được khai báo;
- output không bị stage downstream mutate.

---

## 14. Text Reading Pipeline

Khi nguồn có structured text:

```text
Acquire Structured Text
        ↓
Build Source Document
        ↓
Translate Source Document
        ↓
Prepare Presentation
        ↓
Commit Presentation
```

Nguyên tắc:

- không sử dụng OCR;
- không tạo visual-recognition stage không cần thiết;
- giữ paragraph và reading structure;
- ưu tiên reuse Source Document hoặc Translation Result hợp lệ;
- presentation tách khỏi translation.

---

## 15. Image Reading Pipeline

Khi nguồn chỉ có hình ảnh:

```text
Acquire Visual Source
        ↓
Recognize Content
        ↓
Build Source Document
        ↓
Translate Source Document
        ↓
Prepare Presentation
        ↓
Commit Presentation
```

`Recognize Content` có thể bao gồm nội bộ:

- OCR;
- region recognition;
- reading order;
- layout understanding;
- traceability mapping.

Nhưng các capability này không được đẩy thành Business Stage độc lập chỉ vì Runtime cần nhiều WorkItem.

---

## 16. Clipboard Text Pipeline

```text
Acquire Clipboard Text
        ↓
Build Source Document
        ↓
Translate Source Document
        ↓
Prepare Presentation
```

Pipeline này không cần Capture hình ảnh hoặc Recognition.

---

## 17. Clipboard Image Pipeline

```text
Acquire Clipboard Image
        ↓
Recognize Content
        ↓
Build Source Document
        ↓
Translate Source Document
        ↓
Prepare Presentation
```

---

## 18. Manual Image Translation

```text
Acquire Selected Image
        ↓
Recognize Content
        ↓
Build Source Document
        ↓
Translate Source Document
        ↓
Prepare Presentation
```

Khác với Image Reading liên tục ở chỗ:

- source được người dùng cung cấp trực tiếp;
- không bắt buộc có observation loop;
- không tự sinh revision mới từ screen change;
- output có thể được giữ cho đến khi người dùng đóng kết quả.

---

## 19. Retranslation Pipeline

Khi Source Document vẫn còn hợp lệ:

```text
Existing Source Document
        ↓
Translate Source Document
        ↓
Prepare Presentation
        ↓
Commit Presentation
```

Retranslation có thể xảy ra khi:

- đổi target language;
- đổi translation profile;
- đổi glossary snapshot;
- đổi provider policy;
- người dùng yêu cầu dịch lại.

Recognition và Text Processing không chạy lại nếu input contract còn hợp lệ.

---

## 20. Presentation Refresh Pipeline

Khi Translation Result còn hợp lệ nhưng render configuration thay đổi:

```text
Existing Translation Result
        ↓
Prepare Presentation
        ↓
Commit Presentation
```

Ví dụ:

- đổi font;
- đổi line height;
- đổi reader width;
- đổi Side Panel layout;
- thay đổi overlay bounds;
- đổi source/translation display mode.

---

## 21. Export Pipeline

```text
Accepted Business Result
        ↓
Prepare Export Representation
        ↓
Persist or Deliver Export
```

Export không mặc định thuộc Presentation commit và không được tự động lưu toàn bộ user content.

Storage chỉ tham gia khi use case yêu cầu durable persistence rõ ràng.

---

## 22. Minimal Pipeline Selection

Orchestrator phải chọn pipeline nhỏ nhất tạo được requested output hợp lệ.

Ví dụ:

```text
Requested: render lại với font mới
Required: Presentation
Not required: Capture, Recognition, Text Processing, Translation
```

```text
Requested: dịch lại với glossary mới
Required: Translation, Presentation
Not required: Capture, Recognition, Text Processing
```

```text
Requested: xử lý ảnh mới
Required: Capture, Recognition, Text Processing, Translation, Presentation
```

---

## 23. Input Availability

Một Business Stage chỉ được đưa vào plan khi input cần thiết:

- đã tồn tại;
- có thể được tạo bởi stage upstream;
- hoặc được phép lấy từ reusable output.

Orchestrator không kiểm tra Artifact Store trực tiếp trong quá trình execution.

Nó chỉ khai báo reuse eligibility và input requirement. Pipeline Runtime cùng cache/artifact policy quyết định output nào thực sự có thể reuse.

---

## 24. Output Reuse Declaration

Business Execution Plan có thể khai báo các output được phép reuse:

```text
Recognized Content
Source Document
Translation Result
Presentation Model
```

Reuse chỉ hợp lệ khi toàn bộ input version ảnh hưởng kết quả còn tương thích.

Orchestrator không tự quyết định cache hit.

Chi tiết lookup và promotion thuộc:

- `CACHE_POLICY.md`;
- `MEMORY_MODEL.md`;
- `PIPELINE_RUNTIME.md`.

---

## 25. Optional Stage

Một stage được đánh dấu optional khi pipeline vẫn có thể tạo kết quả hữu ích nếu stage đó bị bỏ qua.

Ví dụ tiềm năng:

- language auto-detection khi người dùng đã chọn ngôn ngữ;
- optional enrichment;
- glossary suggestion;
- background prefetch;
- nonessential diagnostics enrichment.

Stage cốt lõi để tạo requested output không được gắn optional chỉ để che failure.

---

## 26. Conditional Stage

Một stage có thể được thêm vào plan theo điều kiện business.

Ví dụ:

```text
Source has structured text
    → skip Recognition

Source is image
    → include Recognition

Translation Result remains valid
    → skip Translation

Presentation profile changed
    → include Presentation only
```

Điều kiện phải dựa trên business input và versioned metadata, không dựa vào mutable runtime state không kiểm soát.

---

## 27. Parallelizable Business Branches

Orchestrator có thể khai báo các nhánh độc lập về mặt logic.

Ví dụ:

```text
Source Document
      ├── Translate visible section
      └── Prepare adjacent-section plan
```

Hoặc:

```text
Page Collection
      ├── Page A
      ├── Page B
      └── Page C
```

Khai báo parallelizable không có nghĩa tất cả branch sẽ chạy đồng thời.

Scheduler quyết định admission dựa trên resource và priority.

---

## 28. Logical Ordering

Parallel execution không được làm thay đổi thứ tự logic của output.

Execution plan phải giữ metadata cần thiết:

```text
documentOrder
pageOrder
regionOrder
segmentOrder
presentationOrder
```

Business ordering thuộc owner module tương ứng.

Scheduler không được tự suy luận business order.

---

## 29. Visible-First Planning

Đối với nội dung lớn, plan có thể khai báo priority class theo ý nghĩa business:

```text
VISIBLE_CONTENT
NEARBY_CONTENT
CURRENT_DOCUMENT_BACKGROUND
PREFETCH
MAINTENANCE
```

Orchestrator chỉ khai báo business priority.

Scheduler chuyển business priority thành admission decision cụ thể.

Visible-first không cho phép bỏ qua correctness hoặc ordering requirement.

---

## 30. Partial Delivery Boundary

Orchestrator phải xác định output nào có thể được trình bày từng phần.

Ví dụ:

- từng paragraph;
- từng comic region;
- từng page;
- từng document chunk.

Partial delivery chỉ được bật nếu:

- owner module hỗ trợ partial contract;
- partial output có identity và order rõ ràng;
- Presentation có thể commit an toàn;
- Runtime có thể xác nhận authority cho từng phần;
- partial result không làm sai meaning của kết quả cuối.

---

## 31. Incremental Pipeline

Ví dụ conceptual plan:

```text
Recognized Page
        ↓
Source Document Chunks
        ├── Chunk 1 → Translation → Presentation
        ├── Chunk 2 → Translation → Presentation
        └── Chunk 3 → Translation → Presentation
```

Pipeline Runtime có thể tạo WorkItem theo chunk.

Business Orchestrator chỉ xác định:

- chunking boundary hợp lệ;
- dependency logic;
- ordering;
- partial-output semantics.

---

## 32. Plan Versioning

Mỗi Business Execution Plan phải có version.

Plan version thay đổi khi:

- stage graph thay đổi;
- required input thay đổi;
- output contract thay đổi;
- conditional rule thay đổi;
- partial-delivery semantics thay đổi;
- ownership boundary thay đổi.

Provider đổi nhưng business plan không đổi thì không nhất thiết tăng plan version.

---

## 33. Configuration Boundary

Execution plan chỉ tham chiếu configuration cần thiết bằng immutable reference hoặc version.

Ví dụ:

```text
translationProfileVersion
glossaryVersion
renderProfileVersion
recognitionProfileVersion
privacyMode
```

Plan không chứa raw secret hoặc mutable provider configuration.

---

## 34. Privacy Boundary

Pipeline planning phải tôn trọng privacy mode:

```text
STANDARD
LOCAL_ONLY
EPHEMERAL
```

Ví dụ:

- `LOCAL_ONLY` loại cloud-only execution path;
- `EPHEMERAL` không tự thêm durable persistence stage;
- user content không được thêm vào plan metadata;
- provider eligibility phải phù hợp privacy policy.

---

## 35. Storage Boundary

Storage không phải stage mặc định của mọi pipeline.

Storage chỉ xuất hiện khi business use case yêu cầu:

- lưu session snapshot;
- lưu glossary;
- lưu translation memory;
- lưu user correction;
- lưu export;
- lưu durable preference;
- tạo recovery point.

Runtime Artifact Store không được biểu diễn như Storage Stage.

```text
Artifact Store
    → runtime artifact lifecycle

Storage
    → durable persistence capability
```

---

## 36. Interaction with Pipeline Runtime

Business Pipeline Orchestration gửi `BusinessExecutionPlan` sang Pipeline Runtime.

Pipeline Runtime chịu trách nhiệm:

- tạo Revision;
- chuyển Business Stage thành WorkItem;
- tạo Attempt;
- Scheduler admission;
- queue;
- worker execution;
- cancellation;
- retry;
- timeout;
- stale validation;
- artifact publication;
- terminal outcome;
- cleanup.

Orchestrator không can thiệp vào execution state sau khi plan được chấp nhận, trừ khi Application gửi business request mới yêu cầu replan.

---

## 37. Interaction with Runtime Control

```text
Business Orchestrator
        ↓ submits immutable plan
Runtime Control
        ↓ owns runtime authority
```

Runtime Control có thể từ chối plan khi:

- session không còn active;
- request đã obsolete;
- plan version không được hỗ trợ;
- configuration reference không hợp lệ;
- privacy constraint không thỏa mãn;
- runtime đang shutdown.

Runtime Control không tự thay đổi business stage graph.

---

## 38. Interaction with Scheduler

Business Orchestrator không gọi Scheduler trực tiếp.

Luồng đúng:

```text
Business Execution Plan
        ↓
Runtime Control
        ↓ creates WorkItem
Scheduler
        ↓ admission decision
```

Scheduler không được thay đổi business dependency hoặc bỏ stage bắt buộc.

---

## 39. Interaction with Business Modules

Mỗi stage tham chiếu public contract của owner module.

Ví dụ:

```text
Recognize Content Stage
        ↓
Recognition public contract
```

Không được tham chiếu:

- provider implementation;
- internal package;
- raw database model;
- private module state;
- UI implementation.

---

## 40. Interaction with Event Bus

Event Bus chỉ phát notification.

Ví dụ:

```text
BUSINESS_PLAN_CREATED
BUSINESS_PLAN_REJECTED
BUSINESS_PLAN_REPLACED
BUSINESS_STAGE_BECAME_AVAILABLE
```

Event không tự kích hoạt stage tiếp theo.

Runtime Control và Scheduler vẫn sở hữu execution decision.

---

## 41. Replanning

Replanning xảy ra khi business intent hoặc input validity thay đổi.

Ví dụ:

- source type thay đổi;
- người dùng đổi target language;
- người dùng đổi presentation mode;
- privacy mode thay đổi;
- required output thay đổi;
- reusable output bị invalid;
- session chuyển use case.

Replanning tạo plan mới.

Plan cũ không bị mutate.

Runtime Control quyết định revision hoặc work cũ còn authority hay không.

---

## 42. Plan Replacement

```text
Plan A created
        ↓
Business intent changes
        ↓
Plan B created
        ↓
Runtime Control revokes obsolete authority
```

Business Orchestrator không tự cancel worker.

Nó tạo plan mới và gửi request thay thế phù hợp cho Runtime Control.

---

## 43. Error Boundary

Planning error khác execution error.

### Planning Error

Ví dụ:

- không tìm được pipeline variant;
- thiếu required business input;
- stage graph có cycle;
- owner module không tồn tại;
- privacy policy loại mọi execution path;
- requested output không được hỗ trợ.

### Execution Error

Ví dụ:

- provider timeout;
- OCR failure;
- worker crash;
- resource exhaustion;
- stale result.

Execution error thuộc Runtime Error Model.

---

## 44. Planning Result

Planning có thể kết thúc bằng:

```text
PLAN_CREATED
PLAN_NOT_REQUIRED
PLAN_REJECTED
PLAN_UNSUPPORTED
```

`PLAN_NOT_REQUIRED` có thể xảy ra khi requested output đã tồn tại và còn hợp lệ mà không cần thêm business stage.

Đây không phải WorkItem terminal outcome.

---

## 45. Observability

Business Pipeline Orchestration cần cung cấp metadata không chứa user content:

- plan creation count;
- plan variant;
- number of stages;
- optional-stage count;
- plan rejection reason;
- replan count;
- plan construction latency;
- reuse eligibility count;
- partial-delivery enabled;
- privacy mode classification.

Không log:

- source text;
- OCR text;
- translated text;
- screenshot;
- prompt;
- secret;
- source URL mặc định.

---

## 46. Conceptual Example: Image Reading

```text
Business Request
    requestType = IMAGE_READING
    requestedOutput = SIDE_PANEL_TRANSLATION

        ↓

Business Pipeline Orchestration

        ↓

BusinessExecutionPlan
    1. Acquire Visual Source
    2. Recognize Content
    3. Build Source Document
    4. Translate Source Document
    5. Prepare Presentation
    6. Commit Presentation

        ↓

Pipeline Runtime
    Revision
      ├── WorkItem(s) for Capture
      ├── WorkItem(s) for Recognition
      ├── WorkItem(s) for Text Processing
      ├── WorkItem(s) for Translation
      └── WorkItem(s) for Presentation
```

Số WorkItem không nhất thiết bằng số Business Stage.

---

## 47. Conceptual Example: Novel Text

```text
Business Request
    requestType = TEXT_READING

        ↓

BusinessExecutionPlan
    1. Acquire Structured Text
    2. Build Source Document
    3. Translate Source Document
    4. Prepare Presentation
    5. Commit Presentation
```

Recognition không xuất hiện vì nguồn đã có structured text.

---

## 48. Conceptual Example: Font Change

```text
Business Request
    requestType = PRESENTATION_REFRESH

        ↓

BusinessExecutionPlan
    1. Prepare Presentation
    2. Commit Presentation
```

Translation không chạy lại.

---

## 49. Conceptual Example: Glossary Change

```text
Business Request
    requestType = RETRANSLATION
    glossaryVersion = new

        ↓

BusinessExecutionPlan
    1. Translate Source Document
    2. Prepare Presentation
    3. Commit Presentation
```

Source Document được reuse nếu vẫn hợp lệ.

---

## 50. Dependency Rules

1. Business Orchestrator phụ thuộc public business contract, không phụ thuộc implementation.
2. Business Orchestrator không phụ thuộc Scheduler implementation.
3. Business Orchestrator không gọi Worker.
4. Business Orchestrator không sở hữu Artifact Store.
5. Business Stage không deep-import stage khác.
6. Business Stage graph phải acyclic.
7. Stage owner phải rõ ràng.
8. Required input và expected output phải serializable ở boundary.
9. Provider DTO không được xuất hiện trong execution plan.
10. Secret không được xuất hiện trong execution plan.
11. Storage chỉ được đưa vào plan khi use case yêu cầu durable persistence.
12. Event Bus không điều phối stage graph.
13. Runtime Control không tự thay đổi business dependency.
14. Scheduler không thay đổi business priority semantics.
15. Plan cũ không bị mutate khi replanning.

---

## 51. Invariants

1. Mỗi Business Execution Plan có một `planId` và `planVersion`.
2. Mỗi Business Stage có một owner chính.
3. Business Stage không đồng nghĩa capability nội bộ.
4. Plan chỉ chứa dependency logic, không chứa execution state.
5. Business Orchestrator không sở hữu queue, retry hoặc cancellation.
6. Scheduler không được gọi trực tiếp từ Business Orchestrator.
7. Business Stage không tự kích hoạt stage tiếp theo.
8. Stage output không bị downstream stage mutate.
9. Pipeline variant phải chọn tập stage tối thiểu cần thiết.
10. Structured text được ưu tiên trước Recognition/OCR.
11. Partial delivery phải có identity và logical ordering rõ ràng.
12. Replanning tạo plan mới.
13. Storage không phải stage mặc định.
14. Artifact Store không phải Storage Module.
15. Privacy mode có thể loại execution path không hợp lệ.
16. Execution failure không được định nghĩa lại trong tài liệu này.
17. WorkItem và Attempt chỉ xuất hiện ở Pipeline Runtime.
18. Số Business Stage không quyết định số WorkItem.
19. Plan không chứa user content trong metadata mặc định.
20. Runtime execution không được thay đổi business semantics của plan.

---

## 52. Related Documents

| Document | Relationship |
|---|---|
| `RUNTIME_COMPONENTS.md` | Runtime component và ownership boundary |
| `PIPELINE_RUNTIME.md` | Thực thi Business Execution Plan |
| `SCHEDULER.md` | Admission decision |
| `WORK_QUEUE.md` | Queued WorkItem lifecycle |
| `CANCELLATION.md` | Cancellation propagation |
| `RETRY_POLICY.md` | Retry attempt |
| `CACHE_POLICY.md` | Output reuse và cache validation |
| `MEMORY_MODEL.md` | Revision, Artifact và Lease |
| `ERROR_MODEL.md` | Execution failure và terminal outcome |
| `RUNTIME_CONFIG.md` | Configuration snapshot và version |
| `../core/DATA_FLOW.md` | Business data ownership và artifact flow |
| `../../modules/*/MODULE.md` | Business Module responsibility |
| `../../modules/*/CONTRACT.md` | Public business contract |

Đường dẫn cụ thể cần được điều chỉnh theo cấu trúc repository thực tế.

---

## 53. Completion Criteria

Tài liệu được xem là hoàn chỉnh khi:

- Business Pipeline Orchestration được tách khỏi Pipeline Runtime;
- mọi pipeline variant sử dụng Business Stage thay vì capability nội bộ;
- Text Flow và Image Flow được phân biệt;
- minimal pipeline selection được định nghĩa;
- plan, stage và dependency có ownership rõ ràng;
- reuse, partial delivery và visible-first chỉ được khai báo ở mức business;
- Scheduler, Worker, retry, cancellation và stale không còn thuộc Orchestrator;
- Storage và Artifact Store không bị nhầm;
- privacy boundary được phản ánh;
- planning error được tách khỏi execution error;
- terminology thống nhất với Business Module Architecture.

---

## 54. Summary

Business Pipeline Orchestration chuyển business intent thành một immutable execution plan:

```text
Business Request
        ↓
Pipeline Variant Selection
        ↓
Business Stage Graph
        ↓
Business Execution Plan
        ↓
Pipeline Runtime
```

Ranh giới cốt lõi:

```text
Business Orchestrator decides what business work is required.

Pipeline Runtime decides how that work is executed.

Business Modules decide the meaning and correctness of each result.
```
