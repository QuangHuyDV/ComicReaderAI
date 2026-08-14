# CRAI Persistence Technology

Status: Proposed Baseline
Version: 0.1.0
Updated: 2026-08-14
Path: 04-technology/PERSISTENCE.md
Depends On: 04-technology/TECH_STACK.md

## 1. Purpose

Tài liệu này định nghĩa technology selection cho durable local persistence của CRAI.

Mục tiêu là chọn cách triển khai Persistence phù hợp với architecture đã khóa mà không làm Storage trở thành owner của business data hoặc trộn Persistent Storage với Runtime Artifact Store và Runtime Cache.

Technology baseline:

```text
Persistent Database
    → SQLite

Primary .NET Driver
    → Microsoft.Data.Sqlite

Data Access Style
    → explicit SQL + thin persistence adapters

ORM
    → not required for MVP

Large Binary Storage
    → filesystem / artifact-specific storage when persistence is explicitly required

Runtime Artifact Store
    → not SQLite by default

Runtime Cache
    → bounded memory by default
```

## 2. Architecture Constraints

Persistence implementation phải tuân các boundary đã định nghĩa trong Architecture và Storage module.

Canonical separation:

```text
Business Module
    │
    │ persistence request
    ▼
Storage Contract
    │
    ▼
Persistence Adapter
    │
    ▼
SQLite / Filesystem
```

Storage sở hữu persistence behavior.

Business Module sở hữu business meaning.

Runtime sở hữu execution mechanics và execution authority.

Runtime Artifact Store sở hữu accepted shared Runtime Artifact payload.

Runtime Cache sở hữu performance-oriented reusable state theo Cache Policy.

Các concern này không được nhập lại thành một database abstraction duy nhất.

## 3. Persistence Is Not Business Ownership

SQLite lưu representation của business-owned data.

SQLite không trở thành semantic owner của dữ liệu đó.

Ví dụ:

```text
Glossary semantics
    → owning Business Module

Glossary persistence
    → Storage capability

SQLite table
    → implementation detail
```

Tương tự:

```text
Reading History semantics
    → Reading / owning Business concern

History persistence
    → Storage

Translation Memory semantics
    → owning Translation/Text concern

Translation Memory persistence
    → Storage
```

Database schema không được dùng để redefine public Business contracts.

## 4. Three Different Storage Concerns

CRAI phải giữ ba concern riêng biệt.

### 4.1 Runtime Artifact Store

Sở hữu accepted shared Runtime Artifact payload và artifact lookup/lifecycle integration.

Ví dụ:

- accepted OCR Runtime Artifact
- accepted Translation Runtime Artifact
- accepted intermediate Runtime Artifact

Artifact Store không mặc định là durable database.

MVP direction:

```text
Runtime Artifact Store
    → in-memory / Runtime-owned representation
```

### 4.2 Runtime Cache

Cache chỉ phục vụ performance/reuse.

Cache không phải source of truth.

MVP direction:

```text
Runtime Cache
    → bounded memory
    → non-durable by default
```

Application phải vẫn hoạt động đúng khi cache trống.

### 4.3 Persistent Storage

Persistent Storage phục vụ durable application state.

MVP direction:

```text
Persistent Storage
    → SQLite
    +
    filesystem when large durable binary content is explicitly required
```

Canonical rule:

```text
Runtime Artifact Store
!=
Runtime Cache
!=
Persistent Storage
```

## 5. Selected Database

Selected:

```text
SQLite
```

SQLite phù hợp CRAI vì:

- embedded
- local-first
- không cần database server
- transaction support
- mature
- portable database file
- schema migration khả thi
- backup/recovery đơn giản hơn server database
- phù hợp desktop single-user application
- deployment footprint nhỏ
- hỗ trợ concurrent readers
- đủ cho expected metadata/history workload

CRAI không có requirement hiện tại cần PostgreSQL, MySQL hoặc database server khác.

## 6. Selected .NET SQLite Driver

Selected baseline:

```text
Microsoft.Data.Sqlite
```

Lý do:

- phù hợp .NET application stack
- thin SQLite access layer
- cho phép explicit SQL
- không buộc application dùng full ORM
- dễ giữ Storage Contract độc lập với persistence implementation
- phù hợp migration và transaction handling chủ động

Public Storage contracts không được expose:

- SqliteConnection
- SqliteCommand
- SqliteDataReader
- SQL strings
- SQLite error codes

Các type đó chỉ tồn tại trong Infrastructure/Persistence implementation.

## 7. ORM Decision

MVP decision:

```text
Full ORM
    → Not Selected
```

Không chọn EF Core làm mandatory persistence abstraction ở baseline.

Lý do:

- CRAI persistence model hiện chưa chứng minh cần rich object graph tracking;
- Storage contracts đã được thiết kế implementation-independent;
- explicit SQL giúp ownership và transaction boundary rõ;
- tránh để ORM entity trở thành domain model ngoài ý muốn;
- migration/schema behavior dễ kiểm soát;
- desktop local database workload dự kiến tương đối nhỏ.

Điều này không cấm EF Core trong tương lai.

EF Core chỉ nên được xem lại nếu implementation chứng minh:

- schema lớn đáng kể;
- mapping boilerplate trở thành chi phí thực;
- query composition phức tạp;
- migration tooling mang lại lợi ích rõ;
- change tracking thực sự cần thiết.

Không thêm ORM chỉ vì convenience ban đầu.

## 8. Data Access Style

Selected direction:

```text
Storage Contract
    ↓
Persistence Adapter
    ↓
Explicit SQL
    ↓
Microsoft.Data.Sqlite
```

Persistence Adapter chịu trách nhiệm:

- command/query mapping;
- parameter binding;
- transaction boundary;
- persistence-version mapping;
- database-error translation;
- row-to-storage-record mapping;
- schema compatibility.

Business Module không chứa SQL.

UI không chứa SQL.

Runtime Control không chứa SQL.

## 9. Repository Pattern

Generic Repository không phải baseline requirement.

Không tạo abstraction kiểu:

```text
IRepository<T>
```

cho toàn bộ CRAI chỉ để che SQLite.

Storage architecture đã cung cấp implementation-independent persistence boundary.

Nếu một persistence concern cần adapter riêng, dùng contract cụ thể theo capability.

Ví dụ conceptually:

```text
IPreferencesPersistence
IReadingHistoryPersistence
IGlossaryPersistence
ITranslationMemoryPersistence
```

hoặc Storage Contract tương ứng đã được authoritative module định nghĩa.

Exact interface shape phải theo existing Storage contracts, không được PERSISTENCE.md tự redefine.

## 10. Data Suitable for SQLite

SQLite phù hợp cho durable structured state như:

- Preferences
- Reading history
- Reading/session business metadata
- glossary metadata/content
- character metadata
- Translation Memory
- provider configuration metadata
- durable indexes
- migration metadata
- recovery metadata
- application schema version
- lightweight user-created structured state

Việc một loại dữ liệu có thể lưu trong SQLite không đồng nghĩa nó được persist mặc định.

Privacy/persistence policy vẫn quyết định có được lưu hay không.

## 11. Data Not Stored in SQLite by Default

Không mặc định lưu large binary content vào SQLite.

Ví dụ:

- raw screenshots
- captured page images
- temporary OCR images
- model files
- large Runtime Artifacts
- GPU/native buffers
- temporary worker payloads

Nếu một binary artifact cần durable persistence:

```text
Storage Policy
    ↓
Filesystem-backed content
    +
SQLite metadata/reference when useful
```

Database có thể lưu:

- stable ID
- relative content reference
- size
- checksum
- media type
- creation metadata
- retention metadata

nhưng không bắt buộc chứa binary payload.

## 12. Filesystem Storage

Filesystem được dùng khi durable large content thực sự cần thiết.

Filesystem storage phải nằm sau Storage implementation boundary.

Business Modules không hard-code:

- absolute path
- user profile path
- temp directory
- application-data directory

Conceptually:

```text
Business Persistence Request
    ↓
Storage
    ↓
Content Storage Adapter
    ↓
Filesystem
```

Exact application-data directories được quyết định trong Windows Platform/Packaging work.

## 13. Default Persistence Policy

CRAI giữ privacy-by-default.

Các content sau không được persist mặc định chỉ vì persistence technology tồn tại:

- screenshot content
- clipboard content
- OCR full text
- translated full text
- prompts
- provider request payloads
- provider response payloads

Durable persistence của sensitive content phải được bật bởi explicit feature/policy hoặc user-facing behavior tương ứng.

Technology Selection không được biến optional history thành hidden permanent logging.

## 14. Reading History

Reading History là durable feature candidate nhưng persistence behavior phải theo Reading/Preferences policy.

Baseline:

```text
History capability
    → supported

Persist every source payload automatically
    → no
```

History record nên ưu tiên metadata cần thiết thay vì raw captured content.

Nếu history feature cho phép reopen/resume content, exact retained payload phải được quyết định bằng product/privacy policy riêng.

## 15. Preferences

Preferences cần durable local persistence.

Preferences là một trong các critical persistence use cases.

Requirements:

- typed values;
- schema/version awareness;
- deterministic defaults;
- atomic update khi practical;
- recovery from invalid values;
- no secrets in plain preference records.

Preferences persistence failure có thể ảnh hưởng startup/configuration behavior và phải được xử lý rõ.

## 16. Secrets

Secrets không được lưu plain trong SQLite.

Ví dụ:

- API keys
- provider access tokens
- credentials
- refresh tokens

SQLite chỉ có thể lưu opaque secret reference hoặc non-secret metadata nếu cần.

Actual secret material phải dùng Secret Management implementation.

Windows-first direction:

```text
Storage Record
    → Secret Reference

Secret Manager
    → OS-backed protected secret
```

Exact secret technology được quyết định trong Windows/Secret feasibility work.

## 17. Provider Configuration

Provider configuration phải tách:

```text
Non-secret Provider Metadata
    → SQLite allowed

Secret Material
    → Secret Manager
```

Ví dụ SQLite có thể lưu:

- provider ID
- enabled state
- preferred profile
- model identifier
- endpoint selection metadata
- non-secret options

Không lưu plaintext API key.

## 18. Translation Memory

SQLite là baseline phù hợp cho Translation Memory.

Requirements có thể bao gồm:

- source text fingerprint
- source language
- target language
- normalized source identity
- translated text/reference
- glossary/profile version
- provider/model metadata khi semantic compatibility cần
- creation/update metadata

Exact Translation Memory semantic key thuộc owning architecture/module.

Persistence layer không tự định nghĩa semantic equivalence.

## 19. Glossary

SQLite phù hợp cho glossary persistence.

Storage implementation có thể hỗ trợ:

- glossary identity
- entry persistence
- version metadata
- import/export metadata
- update transaction

Glossary matching semantics không thuộc Persistence.

## 20. Schema Strategy

SQLite schema là implementation detail nhưng phải versioned.

Selected direction:

```text
Explicit Schema Version
+
Ordered Migrations
```

Database phải biết schema version hiện tại.

Migration phải:

- deterministic;
- ordered;
- transactional khi SQLite operation cho phép;
- idempotence-aware;
- observable;
- fail safely;
- không silently discard user data.

Không dùng ad-hoc startup SQL không version.

## 21. Migration Ownership

Storage/Persistence implementation sở hữu migration mechanics.

Business owner cung cấp semantic migration requirement khi business representation thay đổi.

Conceptually:

```text
Business/Data Contract Change
    ↓
Persistence Migration Requirement
    ↓
Storage Migration
    ↓
New Schema Version
```

Migration code không được nằm trong UI hoặc Business workflow.

## 22. Migration Execution

Startup sequence conceptually:

```text
Open Database
    ↓
Validate Database
    ↓
Read Schema Version
    ↓
Determine Migration Path
    ↓
Run Required Migrations
    ↓
Validate Result
    ↓
Storage Ready
```

Runtime Artifact Store không được initialize như một side effect của SQLite migration.

Storage initialization và Runtime Artifact initialization là hai lifecycle khác nhau.

## 23. Transaction Strategy

SQLite transactions phải được dùng khi Storage Contract hứa atomic behavior.

Rule:

```text
One semantic persistence operation
    → one explicit transaction when atomicity is required
```

Không giữ transaction mở qua:

- remote API call
- OCR execution
- Translation execution
- UI interaction
- long-running Runtime work

Database transaction boundary phải ngắn.

## 24. Concurrency Model

CRAI là desktop single-user application nhưng vẫn có concurrent Runtime work.

Persistence implementation phải serialize hoặc coordinate writes hợp lý.

Baseline direction:

```text
Multiple logical callers
    ↓
Storage/Persistence boundary
    ↓
bounded DB access
```

Không cho mọi WorkItem tự mở unbounded write workload.

SQLite write concurrency phải được benchmark ở workload thực tế nếu write volume tăng.

## 25. SQLite Journal Mode

Candidate baseline:

```text
WAL
```

WAL là candidate phù hợp cho desktop workload có concurrent reads và bounded writes.

Tuy nhiên exact PRAGMA set phải được xác nhận bằng integration test.

Không hard-code tuning values chỉ theo generic recommendation.

Cần test ít nhất:

- startup/recovery;
- concurrent read/write;
- abrupt process termination;
- migration;
- database copy/backup behavior;
- packaging/user-data location.

## 26. Connection Strategy

Không giữ một global mutable connection object như public application singleton.

Persistence implementation có thể dùng:

- short-lived connections;
- controlled connection factory;
- implementation-owned connection lifecycle.

Exact strategy phải được benchmark nếu workload yêu cầu.

Business code không quản lý connection lifecycle.

## 27. Command Parameterization

Mọi dynamic value phải dùng parameter binding.

Không build SQL bằng string concatenation từ user/provider content.

Requirement này áp dụng cả local desktop application.

## 28. Persistence Identity

Persistence identity không được conflated với:

- ExecutionScopeId
- ExecutionRevisionId
- WorkItemId
- AttemptId
- Runtime Artifact identity
- Domain identity

Storage contracts có thể correlate các identity khi cần nhưng không redefine chúng.

Ví dụ:

```text
Domain Entity ID
    → semantic identity

Persistence Record ID
    → storage identity

Runtime Artifact ID
    → runtime publication identity
```

Có thể mapping 1:1 trong một implementation nhưng ownership vẫn khác.

## 29. Versioning

Cần phân biệt:

```text
Schema Version
Record/Persistence Version
Business Revision
ExecutionRevision
Artifact Revision
```

Không dùng một integer `version` cho mọi semantics nếu chúng có ownership khác nhau.

Persistence layer chỉ sở hữu persistence/schema version mechanics.

## 30. Optimistic Concurrency

Optimistic concurrency không bắt buộc cho mọi table.

Chỉ dùng khi Storage Contract cần phát hiện stale persistent update.

Candidate mechanisms:

- record version
- updated-at token
- explicit expected version

Không map Runtime stale-result rejection sang database optimistic concurrency một cách máy móc.

Đây là hai concern khác nhau.

## 31. Retention

Retention semantics đến từ authoritative owner/policy.

Storage implementation thực thi durable retention.

Ví dụ:

```text
History Retention Policy
    ↓
Storage Retention Operation
    ↓
SQLite / Filesystem Cleanup
```

Runtime Cache eviction không phải durable retention.

Runtime Resource disposal không phải persistent deletion.

## 32. Deletion

Deletion phải có outcome rõ.

Cần phân biệt khi relevant:

- logical removal from active view;
- durable record deletion;
- content-file deletion;
- secret deletion;
- cache eviction;
- Runtime Artifact disposal.

Persistence implementation không được coi các operation này là một `Delete()` generic duy nhất nếu semantics khác nhau.

## 33. Archival

Archival chỉ được implement nếu product requirement cần.

Không tạo archive subsystem trong MVP chỉ vì Storage architecture hỗ trợ archival semantics.

Nếu chưa có concrete use case:

```text
Archival Implementation
    → Deferred
```

## 34. Backup

MVP không cần cloud backup subsystem.

Tuy nhiên local database phải có khả năng được backup an toàn.

Backup strategy phải xem xét:

- SQLite consistency;
- WAL state;
- user-data directory;
- app-running vs app-stopped behavior;
- sensitive data;
- filesystem-backed durable content.

Exact backup UX là future product decision.

## 35. Recovery

Storage startup phải phân biệt ít nhất:

```text
Healthy
Migration Required
Recoverable Failure
Corrupt / Unsafe
Unavailable
```

Không tự động xóa database khi corruption xảy ra.

Recovery có thể:

- retry open;
- restore known-safe state;
- disable non-critical durable feature;
- enter Safe Mode;
- require explicit user action.

Exact response phụ thuộc criticality.

## 36. Failure Criticality

Không phải mọi persistence failure đều fatal.

Ví dụ:

```text
Critical / near-critical
    Preferences/configuration required for safe startup

Potentially degradable
    Reading history
    durable optional cache
    optional analytics metadata
```

Storage implementation phải trả stable failure outcome để higher-level owner quyết định degradation.

Storage không tự quyết định Business continuation.

## 37. Safe Mode

Nếu persistent state không thể được trusted:

```text
Storage Validation Failure
    ↓
Stable Storage Failure
    ↓
Application/Startup Policy
    ↓
Safe Mode or Recovery UX
```

Persistence technology phải hỗ trợ diagnosis nhưng không tự định nghĩa toàn bộ Safe Mode behavior.

## 38. Cache Persistence

MVP decision:

```text
Persistent Runtime Cache
    → Not Selected
```

Runtime Cache baseline vẫn là bounded memory.

Không dùng SQLite làm cache chỉ vì database đã tồn tại.

Persistent cache chỉ được xem xét nếu measurement cho thấy:

- recomputation cost cao;
- startup reuse có giá trị;
- semantic compatibility có thể xác định an toàn;
- privacy cho phép;
- cleanup/retention complexity đáng giá.

## 39. Artifact Persistence

MVP decision:

```text
Persist all Runtime Artifacts
    → No
```

Runtime Artifact publication không đồng nghĩa durable persistence.

Nếu một Business feature yêu cầu durable artifact:

```text
Accepted Runtime Artifact
    ↓
Business/Storage Persistence Request
    ↓
Persistent Representation
```

Đó là operation riêng.

## 40. Temporary Data

Temporary processing data không được đưa vào durable database mặc định.

Ví dụ:

- preprocessing images;
- OCR crops;
- transient recognition buffers;
- temporary translation chunks;
- intermediate model tensors.

Các dữ liệu này thuộc Runtime Resource/Artifact lifecycle tương ứng.

Nếu isolated worker cần temporary file, file đó vẫn là temporary resource chứ không trở thành persistent business record.

## 41. Database Location

Exact Windows path chưa được khóa trong PERSISTENCE.md.

Requirement:

```text
User-writable
Per-user
Stable across normal application updates
Not inside installation directory
```

Exact path phụ thuộc Windows Platform và Packaging decision.

Persistence code phải nhận location từ platform/configuration boundary thay vì tự hard-code.

## 42. Multiple Profiles

Multi-profile database strategy chưa phải MVP requirement.

Không thiết kế:

- multi-user server tenancy;
- account database sharding;
- remote synchronization;

khi chưa có requirement.

Nếu sau này CRAI có profile concept, database separation/mapping sẽ được quyết định riêng.

## 43. Encryption at Rest

Full SQLite database encryption chưa được chọn.

Reason:

- chưa có explicit requirement cho encrypted entire database;
- secret material đã phải tách sang Secret Manager;
- sensitive content không persist mặc định.

Nếu product requirement sau này yêu cầu encrypted durable content, cần Technology Decision riêng đánh giá:

- encryption library;
- key lifecycle;
- backup/recovery;
- migration;
- platform support;
- license;
- performance.

Không tuyên bố SQLite database hiện tại là encrypted nếu chưa có implementation.

## 44. Search

SQLite search capability có thể được dùng cho history/glossary/Translation Memory khi requirement xuất hiện.

FTS không được bật mặc định trước khi query/use case được xác định.

Candidate:

```text
SQLite FTS5
```

chỉ khi search workload chứng minh cần.

## 45. Serialization

Structured payload trong SQLite phải ưu tiên typed columns cho dữ liệu cần query/index.

Opaque JSON có thể dùng cho:

- provider-neutral extension metadata;
- forward-compatible optional settings;
- payload ít query;

nhưng không được biến toàn database thành untyped JSON blob store.

Serialization format phải version-aware khi persisted lâu dài.

## 46. Time Representation

Persisted timestamps phải có semantics rõ.

Baseline:

```text
UTC for absolute timestamps
```

Local timezone/display conversion thuộc presentation/application concern.

Không lưu ambiguous local timestamp khi dữ liệu cần absolute ordering.

## 47. Text Encoding

Text persistence dùng Unicode.

SQLite text handling phải bảo toàn:

- Simplified Chinese
- Traditional Chinese
- Vietnamese
- English
- mixed-language content

Normalization semantics của source/translated text thuộc Text/Translation owner, không thuộc database driver.

## 48. Indexing

Index chỉ thêm cho query path thực.

Initial candidates có thể gồm:

- stable identity
- lookup key
- created/updated time
- foreign/reference identity
- Translation Memory lookup key

Không tạo index speculative trên mọi field.

Index strategy phải dựa vào query patterns và measurement.

## 49. Observability

Persistence layer phải emit structured operational telemetry phù hợp.

Có thể gồm:

- operation type
- duration
- success/failure
- migration version
- database-open duration
- transaction duration
- affected record count khi safe
- recovery state

Không log mặc định:

- SQL parameter chứa source text
- full OCR text
- full translation text
- secret
- raw provider payload

Runtime correlation có thể được propagate khi persistence operation xảy ra trong Runtime work.

## 50. Error Mapping

SQLite-native errors không crossing public Storage boundary.

Conceptually:

```text
SQLite Error
    ↓
Persistence Adapter
    ↓
Stable Storage Error
```

Stable errors có thể phân biệt concern như:

- unavailable
- conflict
- invalid data
- migration failure
- corruption
- capacity/resource failure
- unsupported schema
- unknown persistence failure

Exact public error taxonomy thuộc Storage authoritative contract.

PERSISTENCE.md không redefine nó.

## 51. Cancellation

Database operations nên honor cooperative cancellation khi underlying API/operation cho phép.

Cancellation không được để lại half-committed semantic operation.

Transaction rollback phải xảy ra khi atomic operation bị hủy trước commit.

Runtime cancellation authority vẫn thuộc Runtime.

Persistence chỉ phản ứng với cancellation context được truyền hợp lệ.

## 52. Startup Integration

Recommended startup relationship:

```text
Configuration Bootstrap
    ↓
Resolve Storage Location
    ↓
Open SQLite Backend
    ↓
Validate
    ↓
Migrate
    ↓
Storage Ready
    ↓
Load Preferences / Recovery Metadata
    ↓
Continue Application Startup
```

Runtime Artifact Store được initialize theo Runtime lifecycle riêng.

Không dùng database readiness như implicit Runtime authority.

## 53. Shutdown Integration

Shutdown phải:

- stop accepting new durable operations theo application shutdown policy;
- allow bounded completion/rollback;
- close implementation-owned resources;
- avoid corrupting active transaction;
- flush filesystem-backed durable metadata khi required.

Không cần một complex distributed shutdown protocol.

## 54. Test Strategy

Persistence implementation phải có ít nhất:

### Unit Tests

- mapping
- validation
- version logic
- error translation

### Integration Tests

Dùng real SQLite database cho:

- schema creation
- migration
- transaction rollback
- concurrent reads/writes
- Unicode content
- restart
- corruption/error scenarios khi practical
- cancellation
- retention/deletion

Không mock SQLite cho mọi test.

### Contract Tests

Mọi persistence backend implementation phải thỏa Storage Contract behavior.

### Migration Tests

Phải test upgrade từ supported previous schema versions.

Không chỉ test fresh database.

## 55. Benchmark Requirements

SQLite không cần benchmark synthetic lớn trước MVP.

Chỉ benchmark các risk thực:

- Translation Memory lookup;
- history listing;
- concurrent write bursts;
- startup/migration;
- large glossary;
- filesystem metadata lookup.

Nếu measurement cho thấy SQLite là bottleneck mới xem xét alternative.

Không đổi database dựa trên hypothetical scale.

## 56. Security Requirements

Persistence implementation phải:

- parameterize SQL;
- validate paths;
- không expose arbitrary filesystem path qua untrusted input;
- không persist secrets plaintext;
- không log sensitive values;
- respect privacy policy;
- keep user data in application-owned per-user location;
- handle migration failures safely.

CRAI là local desktop app nhưng local input vẫn không được mặc định trusted tuyệt đối.

## 57. Technology Alternatives

### EF Core SQLite

Status:

```text
Not Selected for MVP Baseline
```

Có thể reconsider nếu persistence complexity tăng.

### Dapper

Status:

```text
Optional Candidate
```

Dapper có thể giảm mapping boilerplate nhưng không cần thiết để bắt đầu.

Không thêm Dapper nếu thin direct mapping đã đủ.

### LiteDB

Status:

```text
Not Selected
```

SQLite phù hợp hơn với requirement về schema evolution, transactions, queryability và ecosystem hiện tại.

### JSON Files as Primary Persistence

Status:

```text
Not Selected
```

JSON file vẫn có thể dùng cho export/import hoặc narrow configuration use case, nhưng không thay SQLite làm durable application store.

### PostgreSQL / MySQL

Status:

```text
Not Selected
```

Không có server/database requirement biện minh cho chúng trong desktop MVP.

## 58. Initial Implementation Direction

Recommended implementation boundary:

```text
src/
└── Crai.Infrastructure/
    └── Persistence/
        ├── Sqlite/
        │   ├── ConnectionFactory
        │   ├── MigrationRunner
        │   ├── Schema
        │   ├── Adapters
        │   └── ErrorMapping
        │
        └── FileStorage/
            └── DurableContentAdapter
```

Tên cụ thể có thể thay đổi trong Implementation Planning.

Không tạo project/assembly riêng cho từng folder nếu chưa cần.

## 59. Persistence Feasibility Gate

Trước khi coi persistence baseline implementation-ready, prototype phải xác nhận:

```text
.NET 10
    +
Microsoft.Data.Sqlite
    +
SQLite
```

đáp ứng:

- database creation;
- schema migration;
- Unicode Chinese/Vietnamese;
- atomic transaction;
- restart/reopen;
- concurrent read/write phù hợp;
- cancellation behavior;
- Windows user-data location integration;
- clean error mapping.

Gate này không cần benchmark OCR hoặc Translation.

## 60. Decisions Locked by This Document

Các quyết định được khóa ở baseline:

```text
Persistent DB
    → SQLite

.NET SQLite Driver
    → Microsoft.Data.Sqlite

Data Access
    → explicit SQL + thin adapters

Full ORM
    → not required for MVP

Runtime Cache
    → bounded memory, non-durable by default

Persist All Runtime Artifacts
    → no

Large Durable Binary
    → filesystem-backed when explicitly required

Secrets
    → not plaintext SQLite

Schema
    → explicit version + ordered migrations
```

## 61. Decisions Still Open

Các quyết định chưa khóa:

1. exact database schema;
2. exact Storage adapter interfaces where authoritative contracts leave implementation freedom;
3. whether Dapper adds enough value to use;
4. exact SQLite PRAGMA configuration;
5. exact connection lifecycle strategy;
6. exact Windows user-data path;
7. exact backup UX;
8. history default enable/disable behavior if not already fixed by product policy;
9. durable artifact use cases;
10. whether FTS5 is needed;
11. whether full database encryption is needed;
12. future persistent cache;
13. future multi-profile storage;
14. exact migration support window.

Các decision này phải được giải quyết khi concrete requirement hoặc feasibility evidence xuất hiện.

## 62. Relationship to Other Technology Decisions

Persistence không block OCR benchmark.

Persistence không block Translation benchmark.

Persistence có dependency nhẹ vào Windows Platform cho final data path.

Packaging phải biết:

- SQLite native/runtime deployment behavior;
- database user-data location;
- migration behavior;

nhưng packaging vẫn bị phụ thuộc mạnh hơn vào OCR runtime.

Canonical relationship:

```text
TECH_STACK
    ↓
PERSISTENCE
    ├──────────────→ WINDOWS_PLATFORM
    │
    ├──────────────→ OCR_CANDIDATES
    │
    └──────────────→ TRANSLATION_CANDIDATES
```

## 63. Next Step

Sau khi baseline Persistence được chấp nhận:

```text
04-technology/WINDOWS_PLATFORM.md
```

Tài liệu đó sẽ đánh giá và khóa các technology direction cho:

- Windows capture;
- window tracking;
- region selection;
- global hotkeys;
- clipboard;
- DPI;
- transparency;
- Overlay feasibility;
- platform storage/secret integration boundary.

Nó không được khóa final Overlay implementation trước feasibility gate.

## 64. Final Principle

Persistence của CRAI phải giữ nguyên:

```text
Business owns meaning.

Storage owns persistence behavior.

SQLite is an implementation detail.

Runtime Artifact Store is not the database.

Runtime Cache is not the database.

Sensitive content is not persisted by default.

Evidence, not convenience, expands persistence scope.
```

MVP persistence phải nhỏ, local, predictable, recoverable và dễ thay đổi mà không làm rò rỉ SQLite semantics vào Business Architecture.
