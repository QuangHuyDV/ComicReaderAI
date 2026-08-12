# Runtime Architecture

> **Project:** CRAI
> **Version:** 2.0
> **Status:** Architecture Overview

---

# 1. Purpose

Thư mục `01-architecture/runtime/` định nghĩa kiến trúc Runtime của CRAI.

Runtime là lớp chịu trách nhiệm:

```text
execute
coordinate
schedule
protect authority
manage runtime resources
observe execution
```

trong thời gian ứng dụng đang chạy.

Runtime không sở hữu Business semantics.

Runtime cũng không quyết định cách:

* Recognition/OCR hoạt động;
* Translation hoạt động;
* Presentation hiểu kết quả;
* AI chọn Provider/Model;
* Business pipeline được lập kế hoạch.

Thay vào đó Runtime chịu trách nhiệm biến một execution plan đã được Business layer xác nhận thành quá trình thực thi có kiểm soát.

---

# 2. Architectural Boundary

CRAI phân biệt ba lớp chính:

```text
Business Pipeline Orchestration
        |
        | decides WHAT business work exists
        v
BusinessExecutionPlan
        |
        v
Runtime
        |
        | decides HOW declared work executes safely
        v
Business Modules
        |
        | decide WHAT results mean
        v
Business Result
```

Runtime trả lời:

```text
How should declared business work execute
while preserving:

authority
cancellation
retry
resource safety
bounded concurrency
stale protection
observability
```

---

# 3. Runtime Responsibilities

Runtime chịu trách nhiệm:

* Runtime lifecycle;
* ExecutionScope lifecycle;
* ExecutionRevision lifecycle;
* WorkItem execution coordination;
* Attempt execution coordination;
* Scheduler admission;
* Work Queue;
* execution authority;
* Runtime Artifact publication;
* cancellation;
* Retry;
* Runtime cache/reuse mechanics;
* Runtime resources;
* resource ownership / Lease / retention;
* memory/resource pressure;
* execution contexts;
* process topology;
* Runtime error normalization;
* Runtime configuration;
* performance measurement;
* observability;
* graceful shutdown.

---

# 4. Runtime Does Not Own

Runtime không sở hữu:

* Business workflow semantics;
* OCR/Recognition algorithms;
* Translation strategy;
* source interpretation semantics;
* AI routing policy;
* Provider selection policy;
* Plugin business behavior;
* Domain history;
* Presentation/UI semantics;
* durable Business persistence;
* user/business configuration truth.

Những phần đó thuộc các architecture/module tương ứng.

---

# 5. Core Philosophy

Runtime được xây dựng theo các nguyên tắc:

```text
Business owns semantics.

Runtime owns execution orchestration.

Runtime Control owns execution authority.

Scheduler owns admission.

Worker owns physical Attempt execution.

Runtime Artifact Store owns published execution payload lifecycle.

Business Module owns result correctness.

Presentation owns visible commit.

Storage owns durable persistence.
```

Ngoài ra:

* resources are bounded;
* queues are bounded;
* concurrency is bounded;
* cancellation revokes authority first;
* Retry creates a new Attempt;
* Fallback is not Retry;
* Cache is optional;
* Storage is optional for Runtime correctness;
* Published Runtime Artifacts are immutable;
* resource ownership is explicit;
* physical disposal waits for Lease/retention/use eligibility;
* performance is measured by useful current results;
* observability explains decisions;
* process placement must not change Runtime semantics.

---

# 6. Runtime Priority Philosophy

Runtime ưu tiên:

```text
Correct Current Result
        |
        v
Responsive Control / UI
        |
        v
Useful Result Latency
        |
        v
Predictable Runtime
        |
        v
Stable Resource Usage
        |
        v
Execution Cost Efficiency
        |
        v
Maximum Raw Throughput
```

`Current ExecutionRevision` là một strong freshness signal.

Nó không phải absolute priority override.

CONTROL operations như:

* cancellation;
* shutdown;
* fatal containment;

có thể có priority cao hơn ordinary Business execution.

---

# 7. Canonical Runtime Hierarchy

```text
ApplicationInstance
        |
        v
ExecutionScope
        |
        v
ExecutionRevision
        |
        v
WorkItem
        |
        v
Attempt
        |
        v
Physical Child Operations
```

Đây là hierarchy chuẩn của Runtime v2.

---

# 8. ReadingSession vs ExecutionScope

Critical distinction:

```text
ReadingSession
    = Business / Domain concept
```

```text
ExecutionScope
    = Runtime execution concept
```

Một ReadingSession MAY liên kết với một ExecutionScope.

Runtime MUST NOT redefine ReadingSession lifecycle.

---

# 9. ExecutionRevision

`ExecutionRevision` đại diện cho một generation của execution intent bên trong một ExecutionScope.

Nó dùng để xác định:

* freshness;
* current execution authority;
* replacement;
* stale-result rejection;
* cancellation/drain.

ExecutionRevision KHÔNG phải:

```text
TranslationRevision
CharacterRevision
ProfileRevision
```

Các revision đó thuộc Domain/Business architecture.

---

# 10. WorkItem

`WorkItem` là một logical unit của Runtime execution.

Rules:

* WorkItem identity ổn định;
* Retry không tạo WorkItem mới nếu logical work không đổi;
* WorkItem có thể có nhiều Attempt;
* WorkItem chỉ nhận tối đa một accepted logical terminal outcome;
* WorkItem không chứa large payload;
* WorkItem không tự schedule downstream Business work.

---

# 11. Attempt

`Attempt` là một physical execution attempt của WorkItem.

```text
WorkItem W1
├── Attempt A1
├── Attempt A2
└── Attempt A3
```

Retry:

```text
same WorkItemId
+
new AttemptId
```

Physical Attempt outcome có thể là:

```text
COMPLETED
FAILED
CANCELLED
ABANDONED
```

`STALE` không phải physical Attempt outcome.

---

# 12. Execution Authority

Runtime Control sở hữu execution authority.

Authority trả lời:

```text
May this execution result
still influence current Runtime?
```

Possible decisions:

```text
ACCEPT
REJECT_STALE
REJECT_CANCELLED
REJECT_DUPLICATE
REJECT_INVALID_STATE
REJECT_INTEGRITY
```

Execution authority KHÔNG trả lời:

```text
Is Translation semantically correct?
Should this UI target display the result?
```

---

# 13. Runtime Artifact

Runtime Artifact là immutable execution payload đã được Runtime publication boundary chấp nhận.

```text
Temporary Output
        |
        v
Candidate Runtime Artifact
        |
        v
Execution Authority Validation
        |
        v
Ownership Transfer
        |
        v
Runtime Artifact Publication
```

Runtime Artifact KHÔNG tự động là:

* Domain truth;
* Business Result;
* persisted Business data;
* cache-eligible result.

---

# 14. Business Acceptance

Sau Runtime Artifact publication:

```text
Runtime Artifact
        |
        v
Owning Business Module
        |
        +--> ACCEPT
        |
        +--> REJECT
        |
        +--> REQUEST_RECOVERY
```

Critical rule:

```text
Runtime authority accepted
    !=
Business result accepted
```

---

# 15. Presentation Boundary

Presentation/Application sở hữu:

* target validity;
* UI state;
* visible replacement;
* commit semantics.

Runtime chỉ cung cấp execution relevance.

Presentation tự kiểm tra target/view state trước visible commit.

---

# 16. Runtime Architecture Overview

```text
Business Pipeline Orchestration
        |
        v
BusinessExecutionPlan
        |
        v
Runtime Control
        |
        +------------------------------+
        |                              |
        v                              v
Scheduler                       Execution State Store
        |
        v
Work Queue
        |
        v
Worker / Execution Context
        |
        v
Execution Adapter / Business Module
        |
        v
Attempt Completion
        |
        v
Runtime Control
        |
        v
Execution Authority Validation
        |
        v
Runtime Artifact Store
        |
        v
Business Acceptance
        |
        v
Presentation / Downstream Stage
```

Supporting Runtime infrastructure:

```text
Resource Manager
Cache Policy
Cancellation
Retry Policy
Error Model
Runtime Configuration
Observability
Process Supervisor
```

---

# 17. Runtime Components

Core logical components include:

```text
Runtime Control
Execution State Store
Scheduler
Work Queue
Worker / Execution Resource Pool
Runtime Artifact Store
Resource Manager
Cache Runtime
Cancellation Coordinator
Retry Policy
Provider Runtime Gateway / Execution Adapters
Observability
Process Supervisor
```

Not every logical component requires:

* separate class;
* separate thread;
* separate process.

---

# 18. Runtime Control

Runtime Control là logical authority cho execution-orchestration state.

It MAY own:

* current ExecutionRevision;
* WorkItem logical state;
* Attempt lineage;
* accepted execution outcome;
* execution authority;
* cancellation authority;
* execution replacement;
* shutdown coordination.

Runtime Control KHÔNG sở hữu:

* Scheduler internal state;
* Queue position;
* physical resources;
* Provider configuration;
* Business correctness;
* UI state;
* durable persistence.

---

# 19. Scheduler

Scheduler sở hữu Runtime admission.

Canonical decisions:

```text
ADMIT
DEFER
REJECT
REPLACE
```

Scheduler không:

* tạo Retry;
* chọn Fallback;
* quyết định WorkItem success;
* grant execution authority;
* commit Business/UI result.

---

# 20. Work Queue

Work Queue sở hữu:

```text
bounded waiting position
```

Queue không sở hữu:

* admission policy;
* execution authority;
* WorkItem terminal outcome;
* Retry;
* cancellation authority.

Queue lưu lightweight references, không lưu large payload.

---

# 21. Cancellation

Cancellation theo nguyên tắc:

```text
Authority Revoked
        |
        v
Prevent New Execution
        |
        v
Remove Queued Work
        |
        v
Signal Running Attempts
        |
        v
Drain Physical Resources
```

Canonical cancellation scopes:

```text
APPLICATION
EXECUTION_SCOPE
EXECUTION_REVISION
WORK_ITEM
ATTEMPT
```

---

# 22. Retry

Retry là:

```text
same logical WorkItem
+
new physical Attempt
```

Retry Policy sở hữu:

* retry eligibility;
* timing;
* backoff;
* Retry-After;
* retry budget.

Retry Policy không chọn Provider/Model/Fallback.

---

# 23. Retry vs Fallback

```text
Retry
    = repeat compatible execution
```

```text
Fallback
    = change execution route/binding
```

Fallback được quyết định bởi Routing/Recovery architecture.

Runtime chỉ thực thi binding mới nếu được cung cấp.

---

# 24. Cache

Cache là optional optimization.

```text
Cache reuses accepted meaning.

Cache does not define meaning.
```

Business Module định nghĩa semantic compatibility.

Policy/Governance định nghĩa reuse permission.

Cache Policy định nghĩa reuse/retention mechanics.

---

# 25. Resource Model

Runtime resource lifecycle dựa trên các dimension riêng biệt:

```text
Ownership
Visibility
Retention
Lease
Physical Use
Disposal
Integrity
```

Critical rule:

```text
Authority loss
    !=
physical disposal
```

Physical disposal chỉ xảy ra khi:

* owner không còn cần;
* retention hết;
* Lease hết;
* physical operation kết thúc;
* disposal an toàn.

---

# 26. Runtime Memory

Large payload SHOULD sử dụng:

```text
RuntimeArtifactRef
ResourceRef
ResourceLease
```

thay vì repeated copying.

Runtime Queue/Event/Command không chứa:

* screenshot buffer;
* full source text;
* translated payload;
* Prompt;
* raw provider response;
* secret.

---

# 27. Threading Model

Runtime sử dụng logical `ExecutionContext`.

```text
ExecutionContext
    !=
OS Thread
    !=
Task
    !=
Process
```

Core contexts MAY include:

* UI Context;
* Runtime Control Context;
* Execution Resource Pool;
* Provider I/O Context;
* Maintenance/Control Context.

CRAI-specific contexts MAY include:

* Capture Context;
* Observation Context;
* GPU/Native Serial Context.

---

# 28. Process Topology

Process placement không thay đổi Runtime semantics.

MVP mặc định:

```text
Single Main Process
```

Future MAY isolate:

* unstable/native Provider Runtime;
* local AI model;
* third-party Plugin;
* Capture runtime;
* high-risk native dependency.

Canonical identity vẫn giữ nguyên xuyên process boundary:

```text
ExecutionScope
ExecutionRevision
WorkItem
Attempt
```

---

# 29. Process Boundary

Critical distinctions:

```text
Module != Process

Process != Thread

Process != Runtime Authority

Process != Business Owner
```

Worker process chỉ thực hiện physical execution.

Runtime Host vẫn xác nhận Completion authority.

---

# 30. Runtime Lifecycle

High-level Runtime lifecycle:

```text
Boot
        |
        v
Runtime Ready
        |
        v
ExecutionScope Opened
        |
        v
ExecutionRevision Created
        |
        v
WorkItems / Attempts
        |
        v
Runtime Results
        |
        v
Business Acceptance
        |
        v
Presentation
        |
        v
ExecutionScope Close / Replacement
        |
        v
Shutdown
```

---

# 31. Runtime Boot

Boot sequence SHOULD conceptually initialize:

```text
Application Host
        |
        v
Runtime Configuration
        |
        v
Runtime Control
        |
        v
Artifact / Resource Infrastructure
        |
        v
Scheduler / Queue
        |
        v
Provider / Plugin Runtime
        |
        v
Business Modules
        |
        v
Presentation
        |
        v
Accept Work
```

Exact order belongs to `BOOT_SEQUENCE.md`.

---

# 32. Runtime Shutdown

Conceptually:

```text
Stop New Admission
        |
        v
Revoke Application Execution Authority
        |
        v
Cancel ExecutionScopes
        |
        v
Remove Queued Work
        |
        v
Drain Attempts
        |
        v
Release Leases / Retention
        |
        v
Shutdown Worker / Provider Runtime
        |
        v
Dispose Runtime Resources
        |
        v
Flush Bounded Diagnostics
```

All waits must remain bounded.

---

# 33. Runtime Configuration

Runtime Configuration controls operational mechanics such as:

* concurrency;
* queue capacity;
* resource budgets;
* Retry limits;
* cancellation grace;
* cache retention;
* worker/process isolation;
* observability limits.

Runtime Configuration không định nghĩa Business semantics.

---

# 34. Error Model

Runtime Error Model tách:

```text
Physical Failure
!= Cancellation
!= Stale
!= Abandoned
!= Business Rejection
!= Presentation Rejection
```

Error Model:

* normalizes;
* classifies;
* correlates;
* provides Retry/Recovery hints.

Error Model không:

* self-Retry;
* choose Fallback;
* commit result.

---

# 35. Performance Model

Primary metric:

```text
Useful Result Latency
```

Useful-result chain:

```text
Current Intent / Source
        |
        v
Execution
        |
        v
Execution Authority Accepted
        |
        v
Runtime Artifact Published
        |
        v
Business Result Accepted
        |
        v
Presentation Committed
```

Raw throughput alone không phải performance success.

---

# 36. Observability

Runtime Observability sử dụng:

```text
Metrics
Traces
Structured Logs
Runtime Events
Diagnostic Snapshots
```

Canonical Runtime correlation:

```text
ApplicationInstanceId
ExecutionScopeId
ExecutionRevisionId
WorkItemId
AttemptId
```

Reading content không xuất hiện trong standard telemetry mặc định.

---

# 37. Core Vocabulary

| Concept                 | Meaning                                              |
| ----------------------- | ---------------------------------------------------- |
| `ApplicationInstance`   | Một Runtime application instance                     |
| `ExecutionScope`        | Runtime execution boundary                           |
| `ExecutionRevision`     | Immutable generation của execution intent/authority  |
| `WorkItem`              | Logical Runtime work                                 |
| `Attempt`               | Một physical execution attempt                       |
| `BusinessExecutionPlan` | Immutable business plan Runtime thực thi             |
| `RuntimeArtifact`       | Immutable published execution payload                |
| `CandidateResource`     | Output/resource trước publication                    |
| `ExecutionAuthority`    | Quyền của execution result ảnh hưởng current Runtime |
| `Publication`           | Đưa Runtime Artifact vào shared Runtime visibility   |
| `BusinessAcceptance`    | Business owner chấp nhận semantic result             |
| `Resource`              | Runtime-managed physical/logical resource            |
| `ResourceLease`         | Quyền sử dụng tạm thời resource                      |
| `Retention`             | Lý do giữ resource tồn tại                           |
| `ExecutionBinding`      | Resolved executable runtime implementation           |
| `RuntimeControl`        | Execution-orchestration authority owner              |

---

# 38. Important Vocabulary Distinctions

```text
ReadingSession
    !=
ExecutionScope
```

```text
Domain Revision
    !=
ExecutionRevision
```

```text
Attempt Completion
    !=
Accepted Execution Outcome
```

```text
Runtime Artifact Publication
    !=
Business Acceptance
```

```text
Business Acceptance
    !=
Presentation Commit
```

```text
Cancellation
    !=
Failure
```

```text
Retry
    !=
Fallback
```

```text
Authority Loss
    !=
Physical Resource Disposal
```

---

# 39. Runtime Documents

Thư mục Runtime gồm:

| Document                             | Mục đích                                                   |
| ------------------------------------ | ---------------------------------------------------------- |
| `README.md`                          | Runtime architecture overview                              |
| `RUNTIME_COMPONENTS.md`              | Runtime components và ownership                            |
| `BUSINESS_PIPELINE_ORCHESTRATION.md` | Business execution-plan boundary                           |
| `BOOT_SEQUENCE.md`                   | Runtime startup/shutdown lifecycle                         |
| `PIPELINE_RUNTIME.md`                | ExecutionScope/ExecutionRevision/WorkItem/Attempt pipeline |
| `SCHEDULER.md`                       | Runtime admission                                          |
| `WORK_QUEUE.md`                      | Bounded waiting infrastructure                             |
| `CANCELLATION.md`                    | Execution authority revocation/cancellation                |
| `RETRY_POLICY.md`                    | Same-work Retry                                            |
| `CACHE_POLICY.md`                    | Result reuse / retention                                   |
| `MEMORY_MODEL.md`                    | Runtime memory/resource model                              |
| `RESOURCE_LIFECYCLE.md`              | Ownership/Lease/retention/disposal                         |
| `THREADING_MODEL.md`                 | Execution contexts / affinity                              |
| `PROCESS_TOPOLOGY.md`                | Process isolation / IPC / crash boundaries                 |
| `PERFORMANCE_MODEL.md`               | Useful-result performance                                  |
| `ERROR_MODEL.md`                     | Runtime normalized error model                             |
| `RUNTIME_OBSERVABILITY.md`           | Metrics / trace / logs / snapshots                         |
| `RUNTIME_CONFIG.md`                  | Operational Runtime configuration                          |

---

# 40. Recommended Reading Order

Recommended:

```text
README
    |
    v
RUNTIME_COMPONENTS
    |
    v
BUSINESS_PIPELINE_ORCHESTRATION
    |
    v
BOOT_SEQUENCE
    |
    v
PIPELINE_RUNTIME
    |
    v
SCHEDULER
    |
    v
WORK_QUEUE
    |
    v
CANCELLATION
    |
    v
RETRY_POLICY
    |
    v
CACHE_POLICY
    |
    v
MEMORY_MODEL
    |
    v
RESOURCE_LIFECYCLE
    |
    v
THREADING_MODEL
    |
    v
PROCESS_TOPOLOGY
    |
    v
PERFORMANCE_MODEL
    |
    v
ERROR_MODEL
    |
    v
RUNTIME_OBSERVABILITY
    |
    v
RUNTIME_CONFIG
```

---

# 41. Relationship With Other Architectures

Runtime tương tác với:

```text
Core
        |
        v
Application / Business Orchestration
        |
        v
Runtime
        |
        v
Business Modules
        |
        +--> AI / Provider Runtime
        |
        +--> Presentation
        |
        +--> Storage
        |
        +--> Plugin Runtime
```

---

# 42. Core Boundary

Core cung cấp primitive chung như:

* Event Bus;
* shared contracts;
* generic infrastructure primitives.

Runtime không được biến Core thành Business orchestrator.

---

# 43. Business Module Boundary

Business Modules:

* Recognition;
* Translation;
* Presentation;
* Provider Management;
* Storage;
* other capability modules;

own their own semantics/contracts.

Runtime chỉ gọi public contracts.

---

# 44. AI / Routing Boundary

AI architecture sở hữu:

* model/provider routing;
* alternative execution selection;
* Fallback;
* model/context strategy.

Runtime chỉ execute resolved binding/plan.

---

# 45. Plugin Boundary

Plugin architecture sở hữu:

* plugin capability;
* trust;
* lifecycle;
* compatibility;
* activation.

Runtime chỉ cung cấp execution/process/resource boundaries.

---

# 46. Storage Boundary

Storage sở hữu:

* durable persistence;
* schema/version;
* recovery;
* durable retention.

Runtime Artifact Store không thay thế Storage.

---

# 47. MVP Scope

Runtime MVP SHOULD bao gồm:

* predominantly single-process Runtime;
* one Runtime Control logical writer;
* ExecutionScope / ExecutionRevision;
* WorkItem / Attempt;
* immutable BusinessExecutionPlan input;
* bounded Scheduler admission;
* bounded Work Queues;
* cooperative cancellation;
* bounded Retry;
* process-local Runtime Artifact Store;
* Resource Manager;
* ResourceLease;
* bounded cache;
* Runtime Error Model;
* Runtime Observability;
* graceful shutdown;
* optional process isolation only where required.

---

# 48. MVP Does Not Require

MVP không yêu cầu:

* distributed Runtime;
* distributed Scheduler;
* distributed Work Queue;
* remote Worker fleet;
* distributed Artifact Store;
* general multi-process topology;
* cross-machine execution;
* cluster coordination;
* speculative execution;
* provider racing;
* Runtime-owned autonomous planning.

---

# 49. Future Evolution

Possible future extensions:

* isolated Provider/Model Worker;
* isolated third-party Plugin Worker;
* isolated Capture Worker;
* remote execution;
* shared-memory Artifact transport;
* durable Runtime work recovery;
* distributed execution;
* adaptive scheduling;
* adaptive concurrency;
* advanced cache coalescing;
* hybrid local/cloud execution.

Các extension này MUST preserve existing Runtime semantics.

---

# 50. Design Goals

Runtime hướng tới:

* deterministic execution semantics;
* predictable authority lifecycle;
* bounded queues/resources;
* explicit ownership;
* safe cancellation;
* immutable publication;
* predictable Retry;
* clean Recovery/Fallback boundary;
* reusable Runtime Artifacts;
* explainable performance;
* observable execution;
* safe resource cleanup;
* implementation independence;
* process-topology independence.

---

# 51. Architecture Invariants

1. Business owns semantics.

2. Runtime owns execution orchestration.

3. ExecutionScope/ExecutionRevision are canonical Runtime identities.

4. ReadingSession is not Runtime scope.

5. Domain revision is not ExecutionRevision.

6. WorkItem and Attempt remain distinct.

7. Retry creates another Attempt.

8. Fallback is not Retry.

9. Scheduler owns admission.

10. Work Queue owns waiting position only.

11. Runtime Control owns execution authority.

12. Worker owns physical Attempt execution only.

13. Physical completion does not grant logical acceptance.

14. Runtime Artifact publication is authority-gated.

15. Runtime Artifact publication does not imply Business acceptance.

16. Business acceptance does not imply Presentation commit.

17. Published Runtime Artifacts are immutable.

18. Runtime Artifact Store and Storage remain separate.

19. Cache is optional for correctness.

20. Cache does not define Business semantics.

21. Ownership, Retention and Lease remain distinct.

22. Authority loss does not imply physical disposal.

23. Native/GPU resources have explicit lifecycle.

24. Cancellation revokes authority before physical stop.

25. Stale is an authority rejection concept.

26. Failure, Cancellation, Stale and Abandoned remain distinct.

27. Business rejection is distinct from Runtime failure.

28. Presentation rejection is distinct from Runtime authority rejection.

29. Process placement does not change Runtime semantics.

30. Module boundary does not imply process boundary.

31. Observability does not own Runtime state.

32. Standard telemetry contains no reading content by default.

33. Runtime Control does not own every Runtime component's internal state.

34. Provider Management does not own provider physical Runtime execution.

35. Performance optimization cannot bypass correctness boundaries.

---

# 52. Completion Criteria

Runtime architecture is internally synchronized when:

* all documents use ExecutionScope/ExecutionRevision vocabulary;
* ReadingSession remains Business-owned;
* Business pipeline planning and Runtime execution are separated;
* Runtime Control authority is narrow and explicit;
* Retry/Fallback are separated;
* Runtime Artifact/Business Result/Presentation commit are separated;
* Queue/Scheduler ownership is explicit;
* Cache semantics remain Business-owned;
* Resource ownership/Lease/disposal remain consistent;
* thread/process topology preserves Runtime semantics;
* Provider Runtime and Provider Management remain separated;
* Error Model uses the same execution/outcome taxonomy;
* Performance uses useful-result metrics;
* Observability traces all critical ownership/authority boundaries;
* MVP stays implementable without unnecessary distributed complexity.

---

# 53. Summary

Runtime là execution orchestration layer của CRAI.

Canonical flow:

```text
BusinessExecutionPlan
        |
        v
ExecutionScope
        |
        v
ExecutionRevision
        |
        v
WorkItem
        |
        v
Attempt
        |
        v
Completion
        |
        v
Execution Authority
        |
        v
Runtime Artifact
        |
        v
Business Acceptance
        |
        v
Presentation / Downstream Work
```

Runtime được xây dựng dựa trên nguyên tắc:

```text
Business decides meaning.

Runtime executes safely.

Authority decides whether execution still matters.

Resources remain bounded and explicitly owned.

Processes/threads may change.

Runtime semantics do not.
```

Các tài liệu trong thư mục này là canonical architecture reference cho Runtime v2 của CRAI.
