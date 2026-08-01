# runtime/CACHE_POLICY.md

# Runtime Cache Policy

> Project: CRAI  
> Version: 1.0  
> Status: Architecture Draft

---

## 1. Purpose

Tài liệu này định nghĩa cách CRAI Runtime đánh giá, lưu giữ, tái sử dụng, xác thực, hết hạn và loại bỏ các runtime Artifact nhằm tránh lặp lại computation đắt đỏ mà không làm sai business meaning hoặc runtime authority.

Cache là một cơ chế:

- performance optimization;
- Artifact retention;
- result reuse;
- provider-cost reduction;
- latency reduction.

Cache không phải:

- source of truth;
- Business Module;
- Scheduler decision;
- WorkItem terminal outcome;
- durable Storage mặc định;
- nơi sở hữu business semantics.

---

## 2. Architectural Position

```text
Business Module
    → định nghĩa semantic compatibility

Cache Policy
    → quyết định reuse và retention có được phép hay không

Artifact Store
    → quản lý runtime Artifact, metadata và memory retention

Storage
    → cung cấp durable persistence khi use case cho phép

Runtime Control
    → quyết định reused Artifact còn authority trong execution hiện tại hay không
```

Runtime flow khái niệm:

```text
WorkItem becomes eligible
        ↓
Runtime Control requests reuse evaluation
        ↓
Cache Policy builds ReuseQuery
        ↓
Artifact Store / Durable Cache lookup
        ↓
Candidate validation
        ↓
Reusable Artifact accepted?
    ├── Yes → satisfy logical work without new Attempt
    └── No  → continue Scheduler admission
```

---

## 3. Cache Principles

1. Cache là optional optimization.
2. Runtime phải đúng khi cache hoàn toàn trống.
3. Cache entry không sở hữu business truth.
4. Cache key phải deterministic.
5. Reuse chỉ hợp lệ khi semantic compatibility được chứng minh.
6. RevisionId không phải reuse identity mặc định.
7. Cache hit không phải Scheduler decision.
8. Cache hit không phải terminal outcome.
9. Worker không tự quyết định cache reuse.
10. Technical success không đồng nghĩa cache eligible.
11. Failed, canceled, stale và abandoned output không promote mặc định.
12. Cache retention bounded.
13. Cache eviction không được phá active lease.
14. Cache không chứa secret.
15. Durable cache phải qua Storage boundary.
16. Privacy partition phải được tôn trọng.
17. Cache validation failure được xử lý như miss trừ khi integrity failure cần diagnostics.
18. Cache promotion không copy payload mặc định.
19. Cache operation failure không được phá runtime correctness.
20. Cache policy không được thay đổi business semantics.

---

## 4. Cache vs Source of Truth

Source of truth được chia theo ownership:

```text
Business Module
    → business meaning và result correctness

Runtime Control
    → active execution state và authority

Artifact Store
    → accepted runtime Artifact registry

Storage
    → durable persistence mechanics

Cache
    → optional reuse/retention metadata
```

Cache có thể bị xóa bất cứ lúc nào.

Runtime vẫn phải tạo được kết quả đúng khi:

- mọi entry bị evict;
- durable cache unavailable;
- lookup lỗi;
- validation reject;
- cache disabled bởi privacy mode.

---

## 5. Reuse Vocabulary

### 5.1 Reuse Query

Yêu cầu tìm Artifact có thể thỏa mãn logical work hiện tại.

### 5.2 Reuse Candidate

Artifact hoặc durable record có khả năng tương thích nhưng chưa được chấp nhận.

### 5.3 Reusable Artifact

Artifact đã vượt qua identity, compatibility, integrity, privacy và authority checks.

### 5.4 Cache Entry

Metadata liên kết reuse identity với ArtifactRef hoặc durable persistence reference.

### 5.5 Promotion

Thêm retention ownership cho một accepted Artifact.

### 5.6 Eviction

Bỏ retention do capacity hoặc value.

### 5.7 Invalidation

Entry không còn semantically compatible hoặc structurally valid.

### 5.8 Expiration

Entry vượt temporal lifetime.

### 5.9 Removal

Entry bị xóa do user action, privacy action hoặc administrative operation.

---

## 6. Cache Entry Model

Conceptual model:

```text
CacheEntry
├── CacheEntryId
├── CacheKey
├── ArtifactRef
├── ArtifactType
├── OwnerModule
├── OutputContractVersion
├── CompatibilityMetadata
├── ProducerVersion
├── ConfigurationVersions
├── PrivacyPartition
├── CreatedAt
├── LastAccessedAt
├── ExpiresAt
├── SizeEstimate
├── RetentionClass
├── ValidationState
└── IntegrityMetadata
```

Cache Entry không cần copy Artifact payload.

---

## 7. Ownership

Ownership được tách như sau:

| Concern | Owner |
|---|---|
| Semantic cache-key dependency | Business Module |
| Compatibility rule | Business Module |
| Reuse/retention policy | Cache Policy |
| Runtime Artifact lifecycle | Artifact Store |
| Durable persistence mechanics | Storage |
| Current execution relevance | Runtime Control |
| Admission when reuse fails | Scheduler |
| Physical memory disposal | Artifact Store / Resource Manager |

Business Module không tự quản lý physical cache eviction.

Cache Policy không tự định nghĩa business result compatibility.

---

## 8. Artifact Reuse Flow

```text
Logical work identified
        ↓
Build ReuseQuery
        ↓
Search runtime Artifact Store
        ↓
Optional durable lookup through Storage
        ↓
Collect candidates
        ↓
Identity validation
        ↓
Compatibility validation
        ↓
Integrity validation
        ↓
Privacy validation
        ↓
Authority/relevance validation
        ↓
Accept reusable Artifact
```

Nếu bất kỳ bước nào fail:

```text
Treat as miss
```

ngoại trừ integrity corruption cần error diagnostics và invalidation.

---

## 9. Reuse Query

```text
ReuseQuery
├── OwnerModule
├── ArtifactType
├── InputContentIdentity
├── OutputContractVersion
├── DependencyVersions
├── ConfigurationVersions
├── ProviderProfileVersion
├── LanguageProfile
├── PrivacyPartition
├── Scope
└── RequestedRetentionClass
```

Reuse Query không chứa raw secret hoặc mutable execution state.

---

## 10. Cache Key

Cache key phải deterministic và dựa trên semantic dependencies.

Conceptual composition:

```text
CacheKey = Hash(
    OwnerModule
    + ArtifactType
    + InputContentIdentity
    + OutputContractVersion
    + DependencyVersions
    + ConfigurationVersions
    + ProviderProfileVersion
    + LanguageProfile
    + PrivacyPartition
)
```

Không nên dùng raw source content trong public architecture model.

Implementation có thể hash normalized content identity.

---

## 11. Content Identity

`ContentIdentity` mô tả input business content, không phải execution identity.

Ví dụ:

```text
RevisionId
    → execution identity

ContentIdentity
    → reuse identity
```

Artifact có thể được reuse giữa:

- nhiều Revision;
- nhiều Attempt;
- nhiều Session;

nếu compatibility và privacy policy cho phép.

---

## 12. Compatibility Metadata

Compatibility có thể phụ thuộc:

- source content identity;
- module contract version;
- algorithm/model version;
- provider profile;
- prompt/profile version;
- glossary version;
- language pair;
- preprocessing version;
- output contract version;
- presentation profile;
- privacy mode;
- normalization rules;
- context version.

Business Module định nghĩa dependency nào ảnh hưởng semantic result.

---

## 13. Validation Dimensions

Reuse validation phải tách thành:

### 13.1 Identity Match

Input business content tương thích.

### 13.2 Compatibility Match

Versions và configuration dependencies tương thích.

### 13.3 Integrity Validation

Artifact không corrupt và metadata nhất quán.

### 13.4 Privacy Eligibility

Entry được phép reuse trong current privacy partition.

### 13.5 Retention Availability

Artifact payload hoặc durable record vẫn tồn tại.

### 13.6 Authority/Relevance

Runtime Control xác nhận reuse phù hợp với WorkItem hiện tại.

---

## 14. Revision Compatibility

Artifact không cần thuộc current Revision để reusable.

Revision cũ có thể cung cấp Artifact nếu:

- content identity trùng;
- dependency versions tương thích;
- Artifact accepted và valid;
- privacy scope cho phép;
- Artifact chưa bị invalidated;
- output contract đúng.

Revision authority và Artifact reusability là hai khái niệm khác nhau.

---

## 15. Reuse Scopes

```text
REVISION_LOCAL
SESSION
RUNTIME
DURABLE_ELIGIBLE
```

### Revision Local

Chỉ reuse trong một Revision.

### Session

Reuse giữa Revision trong cùng Session.

### Runtime

Reuse giữa Session trong cùng application runtime.

### Durable Eligible

Có thể persist qua Storage và reuse sau restart nếu policy cho phép.

---

## 16. Runtime Memory Cache

Runtime Memory Cache là volatile retention trong Artifact Store.

Đặc điểm:

- process-local;
- bounded;
- mất khi restart;
- fast lookup;
- ArtifactRef-based;
- active lease safe;
- pressure-aware eviction.

---

## 17. Durable Cache Boundary

Durable Cache là một persistence use case thông qua Storage.

```text
Cache Policy
    ↓ approves durable eligibility
Storage
    ↓ persists cache record
Durable Cache Record
```

Storage không quyết định semantic compatibility.

Business Module và Cache Policy vẫn định nghĩa:

- key;
- version;
- validation;
- privacy;
- invalidation semantics.

---

## 18. Durable Cache Record

Conceptual durable record:

```text
DurableCacheRecord
├── CacheKey
├── ArtifactDescriptor
├── CompatibilityMetadata
├── PersistenceVersion
├── RetentionInstruction
├── PrivacyPartition
├── CreatedAt
├── ExpiresAt
└── IntegrityMetadata
```

Exact schema thuộc Storage implementation.

---

## 19. Cache Population

Chỉ accepted Artifact được xét promotion.

Điều kiện:

- Runtime Control chấp nhận Completion;
- Artifact integrity hợp lệ;
- output contract hợp lệ;
- Business Module xác nhận cache eligibility;
- compatibility metadata đầy đủ;
- privacy policy cho phép;
- retention budget còn;
- promotion không gây ảnh hưởng đáng kể tới current work.

---

## 20. Technical Success vs Cache Eligibility

```text
Attempt completed successfully
    ≠
Artifact cache eligible
```

Ví dụ Artifact không promote nếu:

- thiếu dependency version;
- privacy mode là ephemeral;
- output nondeterministic và policy không cho phép;
- result chỉ valid trong current Attempt;
- current resource pressure quá cao;
- retention benefit thấp.

---

## 21. Partial Artifact Policy

### Unvalidated Partial Output

Không được cache.

### Validated Partial Artifact

Có thể cache nếu:

- có explicit contract;
- có stable identity;
- có ordering metadata;
- owner module cho phép;
- downstream semantics không bị sai;
- Cache Policy cho phép.

MVP mặc định:

```text
Do not promote partial Artifact
unless explicitly declared cache eligible.
```

---

## 22. Warning-bearing Artifact

`SUCCEEDED + warnings` vẫn có thể cache nếu:

- Artifact valid;
- warnings không làm sai semantic output;
- warning metadata được giữ;
- owner module cho phép.

Warning không tự ngăn cache promotion.

---

## 23. Cancellation and Cache

Canceled work không promote mặc định.

Nếu physical execution hoàn thành sau cancellation:

- result không có runtime authority;
- result không tự promote;
- future policy có thể cho phép reuse chỉ khi deterministic và explicit;
- MVP: reject promotion.

---

## 24. Stale Result and Cache

Stale result không overwrite valid entry.

MVP:

```text
STALE
    → no cache promotion
```

Future policy có thể xem xét stale-but-valid Artifact riêng, nhưng phải qua explicit validation và không ảnh hưởng current execution.

---

## 25. Failed and Abandoned Result

```text
FAILED
ABANDONED
```

không promote thành successful Artifact.

Negative-result caching là policy riêng trong tương lai.

---

## 26. Retry Interaction

Trước khi tạo/admit new Attempt:

```text
Runtime Control
    ↓
Reuse evaluation
    ↓
Reusable Artifact found?
```

Nếu có:

- không tạo Attempt mới;
- hoặc rút pending retry Attempt;
- WorkItem được thỏa mãn bằng accepted Artifact flow.

Retry Policy không lookup cache trực tiếp.

---

## 27. Cache Hit Semantics

Cache hit nghĩa là:

```text
Compatible reusable Artifact found
```

Cache hit không phải:

- terminal outcome;
- Scheduler decision;
- UI commit;
- authority grant.

Runtime Control vẫn phải xác nhận WorkItem relevance.

---

## 28. In-Flight Reuse Coordination

Nhiều WorkItem có thể cùng cần một reuse key.

Không nên tạo duplicate expensive execution không cần thiết.

Conceptual behavior:

```text
First compatible work computes
Other compatible work observes in-flight candidate
```

Các WorkItem khác có thể:

- wait;
- attach as observer;
- continue independently;
- abandon waiting nếu stale/canceled.

---

## 29. In-Flight Reuse Rules

1. In-flight execution không trở thành shared mutable WorkItem.
2. Mỗi WorkItem giữ identity riêng.
3. Cancellation một WorkItem không tự cancel producer nếu producer còn owner khác.
4. Producer failure không làm các WorkItem khác terminal tự động.
5. Accepted Artifact mới là reusable output.
6. Wait phải bounded và cancelable.
7. Coalescing không bypass authority validation.

---

## 30. Eviction

Eviction bỏ retention vì:

- memory pressure;
- budget;
- low value;
- LRU/LFU policy;
- session end;
- runtime shutdown;
- manual clear;
- retention-class expiry.

Eviction không làm Artifact invalid nếu active owner/lease còn.

---

## 31. Invalidation

Invalidation xảy ra khi entry không còn đúng hoặc compatible.

Ví dụ:

- integrity failure;
- output contract unsupported;
- compatibility metadata corrupt;
- privacy rule changed;
- module version declares incompatibility;
- user correction invalidates old result;
- security policy requires removal.

Version upgrade thường tạo key mới, không nhất thiết scan-delete toàn bộ old entry ngay.

---

## 32. Expiration

Expiration dựa trên temporal policy:

```text
TTL
Idle TTL
Session lifetime
Runtime lifetime
Durable retention window
```

Expired entry không được reuse.

Payload disposal vẫn phải chờ owner/lease.

---

## 33. Removal

Removal có thể do:

- user clear;
- privacy request;
- account/profile removal;
- manual administration;
- corruption remediation;
- security incident.

Durable removal phải đi qua Storage retention/deletion policy.

---

## 34. Privacy Partition

Cache reuse phải partition theo:

- user/profile;
- privacy mode;
- source scope;
- provider profile;
- local/remote eligibility;
- language/profile;
- content sensitivity classification.

Không reuse xuyên partition nếu chưa được phép rõ ràng.

---

## 35. Privacy Modes

### STANDARD

Memory reuse và durable eligibility theo configured policy.

### LOCAL_ONLY

Không reuse Artifact yêu cầu remote-only provenance nếu policy cấm.

Durable cache chỉ dùng local Storage nếu cho phép.

### EPHEMERAL

- không durable promote;
- retention rất ngắn;
- session/runtime disposal ưu tiên;
- raw content không giữ sau use case.

---

## 36. Security

Cache không chứa:

- API key;
- access token;
- provider secret;
- credential;
- raw secret reference resolution;
- unsafe raw path;
- unrestricted private diagnostics.

Cache key cũng không được leak raw sensitive content.

---

## 37. Cache Failure Degradation

Cache failure mặc định degrade thành miss.

```text
Lookup failure
    ↓
Record diagnostics
    ↓
Treat as miss
    ↓
Continue execution
```

Ngoại lệ:

- corruption liên tục;
- Artifact Store integrity failure;
- Storage failure đe dọa correctness;
- resource pressure nghiêm trọng.

---

## 38. Durable Cache Failure

Nếu durable cache unavailable:

- memory cache vẫn có thể hoạt động;
- Runtime vẫn compute bình thường;
- không làm WorkItem fail mặc định;
- diagnostics ghi persistence degradation;
- durable promotion tạm dừng.

---

## 39. Cache Stampede Prevention

Dùng:

- in-flight coalescing;
- bounded lookup;
- per-key coordination;
- current-revision priority;
- negative result không dùng mặc định;
- admission limits;
- provider concurrency limits.

Không lock toàn runtime theo một key.

---

## 40. Negative Cache Boundary

MVP không hỗ trợ negative cache.

```text
Failure
    → not stored as successful Artifact
```

Future deterministic negative cache phải:

- có type riêng;
- có expiry ngắn;
- không lẫn Artifact cache;
- không tạo false success;
- tôn trọng configuration/version/privacy.

---

## 41. Cache Events

Conceptual events:

```text
CACHE_LOOKUP_STARTED
CACHE_HIT
CACHE_MISS
CACHE_CANDIDATE_REJECTED
CACHE_ENTRY_PROMOTED
CACHE_ENTRY_EVICTED
CACHE_ENTRY_INVALIDATED
CACHE_ENTRY_EXPIRED
CACHE_LOOKUP_FAILED
DURABLE_CACHE_DEGRADED
IN_FLIGHT_REUSE_COALESCED
```

Tên cuối phải tuân theo Event Standard.

---

## 42. Metrics

Theo dõi:

- hit count;
- miss count;
- hit ratio;
- validation reject count;
- compatibility miss count;
- integrity failure count;
- privacy partition miss count;
- promotion count;
- promotion skipped count;
- eviction count by reason;
- invalidation count;
- expiration count;
- retained bytes;
- lookup latency;
- durable lookup latency;
- promotion latency;
- in-flight coalescing count;
- saved execution time;
- saved provider cost;
- current-revision useful hit ratio;
- failed cache operation count.

---

## 43. Useful Cache Metrics

Hit rate đơn thuần chưa đủ.

Ưu tiên:

```text
Useful Hit Ratio
Saved Useful Latency
Saved Provider Cost
Current-Revision Reuse Ratio
Invalid Reuse Prevention Count
```

Cache hit cho obsolete work không phải useful hit.

---

## 44. Observability and Privacy

Cache telemetry chỉ chứa metadata:

- ArtifactType;
- OwnerModule;
- CacheScope;
- decision;
- reason;
- size;
- timing;
- version identifiers.

Không log raw content hoặc key preimage.

---

## 45. MVP Cache Policy

MVP triển khai:

- process-local memory cache;
- bounded Artifact Store retention;
- deterministic cache key;
- Revision/Session/Runtime scopes;
- LRU hoặc weighted LRU;
- no implicit durable cache;
- no negative cache;
- no partial promotion mặc định;
- no stale/canceled/failed promotion;
- privacy partition tối thiểu;
- cache failure → miss;
- in-flight reuse có thể hoãn nếu implementation phức tạp.

---

## 46. MVP Artifact Types

MVP có thể hỗ trợ reuse cho:

```text
Source Artifact
Recognition Artifact
Source Document Artifact
Translation Artifact
Presentation Artifact
```

Artifact type cụ thể do module contract định nghĩa.

Không tạo architecture layer tên `OCR Cache` hoặc `Layout Cache`.

---

## 47. MVP Retention

Conceptual defaults:

| Scope | Retention |
|---|---|
| Revision-local | đến Revision disposal |
| Session | đến Session close hoặc pressure eviction |
| Runtime | bounded LRU |
| Durable | disabled by default |
| Debug/private | disabled by default |

Exact values thuộc `RUNTIME_CONFIG.md`.

---

## 48. Example: Runtime Memory Hit

```text
WorkItem requests Recognition Artifact
        ↓
ReuseQuery built
        ↓
Artifact Store finds candidate
        ↓
Compatibility and integrity valid
        ↓
Runtime Control accepts reuse
        ↓
No new Attempt needed
```

---

## 49. Example: Version Mismatch

```text
Translation Artifact found
        ↓
GlossaryVersion differs
        ↓
Compatibility reject
        ↓
Cache miss
        ↓
New Attempt proceeds
```

Old entry không nhất thiết bị xóa ngay.

---

## 50. Example: Cache Eviction with Lease

```text
LRU selects Artifact
        ↓
Cache retention removed
        ↓
Worker lease active
        ↓
Payload remains
        ↓
Lease released
        ↓
Physical disposal
```

---

## 51. Example: Durable Cache Lookup

```text
Memory miss
    ↓
Durable cache lookup through Storage
    ↓
Record materialized as candidate Artifact
    ↓
Compatibility/integrity validation
    ↓
Runtime Artifact accepted
```

Durable record không tự có runtime authority.

---

## 52. Example: Privacy Partition Miss

```text
Artifact exists in STANDARD partition
        ↓
Current request = EPHEMERAL
        ↓
Reuse not allowed
        ↓
Treat as miss
```

---

## 53. Architecture Invariants

1. Cache không là source of truth.
2. Runtime đúng khi cache trống.
3. Cache key deterministic.
4. RevisionId không phải reuse identity mặc định.
5. Business Module sở hữu compatibility semantics.
6. Cache Policy sở hữu reuse/retention policy.
7. Artifact Store sở hữu runtime payload lifecycle.
8. Storage sở hữu durable persistence mechanics.
9. Runtime Control xác nhận current authority.
10. Scheduler không quyết định cache hit.
11. Worker không tự lookup/promote cache.
12. Cache hit không phải terminal outcome.
13. Technical success không đồng nghĩa cache eligible.
14. Failed/canceled/stale/abandoned output không promote mặc định.
15. Unvalidated partial output không cache.
16. Promotion không copy payload mặc định.
17. Eviction không phá active lease.
18. Invalidation khác eviction.
19. Expiration khác invalidation.
20. Durable cache phải qua Storage.
21. Privacy partition bắt buộc.
22. Cache không chứa secret.
23. Lookup failure degrade thành miss mặc định.
24. In-flight reuse không merge WorkItem identity.
25. Cache metrics không chứa raw content.
26. Negative cache tách khỏi successful Artifact cache.
27. Reuse luôn cần compatibility validation.
28. Version change không bắt buộc xóa old entry ngay.
29. Cache retention bounded.
30. Artifact Store và Storage không bị trộn.

---

## 54. Testing Requirements

Test phải bao phủ:

- memory hit;
- memory miss;
- compatibility mismatch;
- integrity failure;
- privacy partition mismatch;
- runtime cache disabled;
- all entries evicted;
- promotion of accepted Artifact;
- rejected promotion for failed/canceled/stale/abandoned;
- validated partial policy;
- cache eviction with active lease;
- durable lookup unavailable;
- durable record incompatible;
- duplicate in-flight key;
- cancellation while waiting for in-flight reuse;
- retry satisfied by cache;
- version upgrade;
- user clear;
- ephemeral mode;
- cache lookup failure degradation;
- metrics privacy.

---

## 55. Open Questions

- MVP có cần in-flight coalescing không?
- Weighted LRU dùng size hay cost?
- Artifact nào durable eligible?
- Durable cache encryption ở Storage layer ra sao?
- Privacy partition key chi tiết gồm gì?
- Translation cache có chia theo provider hay semantic profile?
- User correction invalidation lan truyền thế nào?
- Source Document reuse giữa Session có được phép không?
- Durable cache retention window là bao lâu?
- Negative cache có cần trong tương lai không?
- Cache usefulness được ước lượng ra sao?

---

## 56. Related Documents

| Document | Relationship |
|---|---|
| `PIPELINE_RUNTIME.md` | WorkItem và authority |
| `RUNTIME_COMPONENTS.md` | Artifact Store ownership |
| `MEMORY_MODEL.md` | Retention, lease và disposal |
| `RESOURCE_LIFECYCLE.md` | Ownership transfer |
| `SCHEDULER.md` | Admission khi reuse miss |
| `RETRY_POLICY.md` | Reuse evaluation trước new Attempt |
| `CANCELLATION.md` | Promotion restriction |
| `ERROR_MODEL.md` | Cache failure normalization |
| `PERFORMANCE_MODEL.md` | Saved useful latency |
| `RUNTIME_CONFIG.md` | Capacity, TTL và policy |
| `RUNTIME_OBSERVABILITY.md` | Metrics and events |
| `../../modules/storage/README.md` | Durable persistence boundary |
| `../../modules/*/CONTRACT.md` | Artifact compatibility semantics |

---

## 57. Completion Criteria

`CACHE_POLICY.md` được xem là đồng bộ khi:

- cache được mô tả như Artifact reuse/retention policy;
- OCR/Layout-specific cache layer bị loại;
- source-of-truth ownership đúng;
- CacheEntry và ReuseQuery rõ ràng;
- ContentIdentity tách RevisionId;
- compatibility, integrity, privacy và authority tách riêng;
- Business Module, Cache Policy, Artifact Store, Storage và Runtime Control có boundary rõ;
- promotion chỉ áp dụng accepted Artifact;
- partial policy không tuyệt đối nhưng MVP an toàn;
- eviction/invalidation/expiration/removal tách rõ;
- durable cache qua Storage;
- privacy partition được định nghĩa;
- in-flight reuse và stampede boundary rõ;
- events, metrics và MVP policy đầy đủ.

---

## 58. Summary

CRAI Cache Policy quản lý Artifact reuse mà không chiếm ownership của business truth:

```text
Business Module defines compatibility.

Cache Policy decides reuse and retention.

Artifact Store manages runtime Artifact lifecycle.

Storage provides durable persistence.

Runtime Control decides whether reused output still matters.
```

Runtime phải luôn đúng khi:

```text
Cache = Empty
```

Cache correctness luôn quan trọng hơn cache hit rate.
