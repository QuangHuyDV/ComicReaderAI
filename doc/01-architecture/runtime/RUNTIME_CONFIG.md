# runtime/RUNTIME_CONFIG.md

# CRAI Runtime Configuration

> Project: CRAI  
> Version: 1.0  
> Status: Architecture Draft

---

## 1. Purpose

Tài liệu này định nghĩa cách CRAI tải, hợp nhất, xác thực, snapshot, kích hoạt, thay đổi và persist configuration của Runtime.

Runtime Configuration kiểm soát behavior mà không cần sửa source code hoặc rebuild application.

Ví dụ:

- source/target language;
- capture/observation policy;
- Business Module policy;
- provider selection;
- execution timeout;
- Scheduler admission;
- Work Queue capacity;
- retry budget;
- cancellation grace period;
- authority validation;
- Artifact publication;
- Resource Lease;
- retention/cache;
- memory/GPU/native budget;
- presentation;
- Storage;
- privacy;
- diagnostics;
- feature flags.

Tài liệu này không định nghĩa UI layout cuối cùng của Settings.

Provider-specific option vẫn phải cô lập sau provider configuration contract.

---

## 2. Core Principles

1. Configuration là data, không phải executable code.
2. Defaults phải tạo được một Runtime hợp lệ.
3. Configuration phải validate trước activation.
4. Active configuration luôn là immutable snapshot.
5. Module chỉ nhận typed view cần thiết.
6. Secret chỉ tồn tại dưới dạng reference trong normal config.
7. Activation boundary phải explicit.
8. Existing Attempt không bị mutation bởi config mới.
9. Runtime authority không được tự thay đổi bởi config file.
10. Config change có thể yêu cầu admission freeze, drain hoặc restart.
11. Persistence phải atomic.
12. Migration phải deterministic.
13. Invalid config không được partially activate.
14. Privacy restriction có precedence cao hơn provider preference.
15. Configuration event không chứa secret.
16. Config diagnostics không chứa reading content mặc định.

---

## 3. Configuration Categories

```text
Runtime Configuration
├── Bootstrap Configuration
├── Application Configuration
├── User Preferences
├── Session Configuration
├── Provider Configuration
└── Diagnostic Configuration
```

### Bootstrap Configuration

Giá trị cần trước khi main Runtime container được tạo.

### Application Configuration

System-wide Runtime policy.

### User Preferences

User-facing preferences không làm thay đổi architecture.

### Session Configuration

Temporary configuration cho một Reading Session.

### Provider Configuration

Selection, capability và provider-specific options.

### Diagnostic Configuration

Logs, metrics, traces, snapshots và development diagnostics.

---

## 4. Activation Modes

Mỗi field phải định nghĩa một activation mode:

```text
IMMEDIATE
NEW_WORK_ONLY
SESSION_RESTART
COMPONENT_RESTART
APPLICATION_RESTART
IMMUTABLE
```

### IMMEDIATE

Có thể apply ngay mà không phá active work.

### NEW_WORK_ONLY

Chỉ WorkItem/Attempt tạo sau activation dùng snapshot mới.

### SESSION_RESTART

Session hiện tại giữ snapshot cũ; config mới dùng cho Session sau.

### COMPONENT_RESTART

Cần drain và recreate Runtime Component.

### APPLICATION_RESTART

Chỉ có hiệu lực sau application restart.

### IMMUTABLE

Không thay đổi qua normal runtime settings.

---

## 5. Top-Level Schema

```yaml
schemaVersion: 1

bootstrap:
  environment: production
  profile: desktop-mvp
  dataDirectory: auto
  secretStore: operating-system
  recoveryMode: safe

application:
  modules: {}
  runtime: {}
  scheduling: {}
  queues: {}
  retry: {}
  cancellation: {}
  authority: {}
  publication: {}
  resources: {}
  leases: {}
  cache: {}
  storage: {}
  security: {}
  features: {}

preferences:
  language: {}
  reading: {}
  capture: {}
  presentation: {}
  accessibility: {}

providers:
  profiles: {}
  instances: {}

diagnostics:
  logging: {}
  tracing: {}
  metrics: {}
  snapshots: {}
  debug: {}
```

Session config được giữ riêng:

```yaml
session:
  sessionId: generated
  configurationSnapshotId: generated
  source: {}
  language: {}
  reading: {}
  providers: {}
  presentation: {}
  privacy: {}
  quality: {}
```

---

## 6. Bootstrap Configuration

Ví dụ:

```yaml
bootstrap:
  environment: production
  profile: desktop-mvp
  dataDirectory: auto
  configFile: auto
  secretStore: operating-system
  recoveryMode: safe
```

Bootstrap config phải:

- nhỏ;
- valid trước Runtime creation;
- không chứa secret value;
- không phụ thuộc Business Module;
- phần lớn yêu cầu application restart.

---

## 7. Application Runtime Configuration

```yaml
application:
  runtime:
    execution:
      defaultExecutionClass: auto
      maxCpuWorkers: auto
      maxGpuExecutions: 1
      maxNativeSerialExecutions: 1
      maxProviderRequests: 4

    control:
      commandQueueCapacity: 256
      commandWarningThreshold: 128
      maxCommandProcessingMs: 50

    shutdown:
      totalTimeoutMs: 10000
      drainTimeoutMs: 5000
      telemetryFlushTimeoutMs: 1000
```

`auto` nghĩa là Runtime chọn giá trị từ device capability và runtime profile.

---

## 8. Module Configuration

```yaml
application:
  modules:
    capture: true
    observation: true
    classification: true
    recognition: true
    structuring: true
    translation: true
    presentation: true
    knowledge: true
    storage: true
    diagnostics: true
```

Feature flag không thay module dependency rule.

Module disabled phải được semantic validation với dependent capability.

---

## 9. Scheduler Configuration

```yaml
application:
  scheduling:
    currentRevisionFirst: true

    priority:
      control: 1000
      interactive: 100
      retry: 50
      background: 10
      maintenance: 1

    admission:
      enabled: true
      rejectWhenCriticalPressure: true
      deferWhenProviderUnavailable: true
      preserveControlCapacity: true

    replacement:
      removeObsoleteQueuedWork: true
      replacePendingEquivalentWork: true

    fairness:
      enabled: true
      maxInteractiveBurst: 16

    aging:
      enabled: false
```

Scheduler config không được cấp authority.

Nó chỉ định admission policy.

---

## 10. Work Queue Configuration

```yaml
application:
  queues:
    control:
      capacity: 128
      overflow: reject

    interactive:
      capacity: 64
      overflow: replace-obsolete

    background:
      capacity: 32
      overflow: reject

    maintenance:
      capacity: 16
      overflow: reject

    dispatch:
      validateAuthorityBeforeDispatch: true
      validateDeadlineBeforeDispatch: true

    drain:
      removeCanceledWork: true
      removeObsoleteWork: true
```

Không dùng một `maxQueuedJobs` duy nhất cho mọi workload class.

---

## 11. Retry Configuration

Retry là Runtime policy độc lập, không nằm hoàn toàn trong provider config.

```yaml
application:
  retry:
    enabled: true

    attempts:
      maxAttemptsPerWorkItem: 2
      maxConcurrentRetries: 2
      maxDelayedRetries: 16

    strategies:
      immediate: true
      delayed: true
      providerFallback: true
      resourceWait: true

    backoff:
      initialDelayMs: 500
      maximumDelayMs: 5000
      multiplier: 2.0
      jitter: true

    budgets:
      perRevision: 8
      perSession: 32
      globalRuntime: 64

    behavior:
      recheckArtifactReuse: true
      cancelWhenAuthorityRevoked: true
      respectRetryAfter: true
```

Exact values phải được benchmark.

---

## 12. Cancellation Configuration

```yaml
application:
  cancellation:
    cooperative: true
    revokeAuthorityImmediately: true
    removeQueuedWork: true

    grace:
      defaultMs: 1000
      providerMs: 2000
      shutdownMs: 5000

    abandoned:
      trackPhysicalExecution: true
      retainProviderCapacity: true

    checkpoints:
      beforeExpensiveExecution: true
      afterExternalCall: true
      beforeCompletion: true
      beforeUiCommit: true
```

Config không được cho phép hard thread kill trong primary process.

---

## 13. Authority Configuration

```yaml
application:
  authority:
    validation:
      completion: required
      publication: required
      uiCommit: required

    staleCompletion:
      accept: false
      recordDiagnostics: true

    duplicateCompletion:
      firstAcceptedWins: true
      recordDiagnostics: true

    revokedScope:
      denyNewLease: true
      denyPublication: true
      denyDownstreamScheduling: true
```

Authority rule là safety invariant.

User settings không được disable các validation bắt buộc.

---

## 14. Publication Configuration

```yaml
application:
  publication:
    requireAcceptedCandidate: true
    requireOwnershipTransfer: true
    atomicPublication: true
    rejectDuplicatePublication: true
    cleanupRejectedCandidates: true

    partialArtifact:
      enabled: false
      requireExplicitContract: true

    latePublication:
      allowed: false
```

Publication config không cho phép Worker publish trực tiếp.

---

## 15. Resource Configuration

```yaml
application:
  resources:
    budgets:
      managedMemoryMb: auto
      nativeMemoryMb: auto
      gpuMemoryMb: auto
      artifactMemoryMb: auto
      diagnosticsMemoryMb: 32

    pressure:
      elevatedRatio: 0.70
      highRatio: 0.85
      criticalRatio: 0.95
      hysteresisRatio: 0.05

    disposal:
      logicalDisposalImmediate: true
      physicalDisposalTimeoutMs: 5000
      cleanupRetryCount: 2

    draining:
      warningAfterMs: 5000
      leakSuspectAfterMs: 30000
```

User-provided values phải bị clamp vào supported safe range.

---

## 16. Lease Configuration

```yaml
application:
  leases:
    enabled: true
    defaultMaximumHoldMs: diagnostic
    denyAfterLogicalDisposal: true
    trackAcquisitionSite: development-only

    leakDetection:
      enabled: true
      warningAfterMs: 10000
      criticalAfterMs: 60000

    shutdown:
      waitForRelease: true
      timeoutMs: 3000
```

`diagnostic` nghĩa là không hard-expire resource đang dùng; chỉ phát hiện và báo.

---

## 17. Cache Configuration

Cache theo Artifact reuse/retention, không theo architecture stage cố định.

```yaml
application:
  cache:
    runtimeMemory:
      enabled: true
      maximumEntries: 2000
      maximumMemoryMb: auto
      policy: weighted-lru

    scopes:
      revisionLocal: true
      session: true
      runtime: true
      durable: false

    promotion:
      acceptedArtifactOnly: true
      allowPartialArtifact: false
      allowStaleArtifact: false
      allowCanceledArtifact: false

    privacy:
      partitionByProfile: true
      disableDurableInEphemeralMode: true

    inflightReuse:
      enabled: false
```

Business Module định nghĩa compatibility dependency.

Cache Policy định nghĩa reuse/retention behavior.

---

## 18. Storage Configuration

```yaml
application:
  storage:
    backend: local
    configurationPersistence: true
    historyEnabled: false
    glossaryEnabled: true
    correctionMemoryEnabled: true
    durableCacheEnabled: false

    recovery:
      enabled: true
      retainRecoveryPoints: 3
```

Storage config không tự thay business ownership.

---

## 19. Security and Privacy Configuration

```yaml
application:
  security:
    allowRemoteTextProcessing: true
    allowRemoteImageProcessing: false

    persistence:
      persistRawCapture: false
      persistRecognizedText: false
      persistTranslationCache: false

    diagnostics:
      includeReadingContent: false
      exportContentFingerprint: false

    cleanup:
      clearTemporaryDataOnExit: true
```

Provider config không được override global privacy restriction.

---

## 20. Provider Configuration

Provider config chia thành:

```text
Provider Selection
Provider Capability Requirement
Provider Execution Declaration
Provider-Specific Options
Credential Reference
```

Ví dụ:

```yaml
providers:
  profiles:
    recognition-default:
      primary: local-recognition
      fallback: null

      requirements:
        languages: [zh-Hans, en]
        verticalText: preferred
        regionDetection: required

      execution:
        timeoutMs: 10000
        concurrency: 1
        executionClass: CPU
        cancellationSupport: cooperative
        processIsolation: false

    translation-default:
      primary: remote-translator
      fallback: remote-translator-secondary

      requirements:
        targetLanguages: [vi]
        batchTranslation: required
        cancellation: preferred

      execution:
        timeoutMs: 20000
        concurrency: 2
        executionClass: REMOTE_IO
        cancellationSupport: provider-dependent

  instances:
    local-recognition:
      type: local
      credentialRef: null
      options:
        device: auto
        modelProfile: chinese-comic

    remote-translator:
      type: remote
      credentialRef: secret://translation/primary
      options:
        model: configured-provider-model
```

Retry count không nên được lặp ở từng provider nếu Runtime Retry Policy đã sở hữu nó, trừ provider-specific hard limit.

---

## 21. User Preferences

```yaml
preferences:
  language:
    source: auto
    target: vi
    preferredSources: [zh-Hans, zh-Hant, en]

  reading:
    contentMode: auto
    translationStyle: natural
    autoProcessStableContent: true

  capture:
    mode: selected-region
    frameIntervalMs: 250
    captureCursor: false

  presentation:
    mode: side-panel
    fontSize: 18
    lineHeight: 1.6
    showSourceText: true
```

User Preferences không chứa Runtime identity hoặc internal lifecycle state.

---

## 22. Session Configuration

```yaml
session:
  sessionId: generated
  configurationSnapshotId: config-snapshot-42

  source:
    type: screen-region
    sourceRef: runtime-managed

  language:
    source: zh-Hans
    target: vi

  providers:
    profile: balanced

  privacy:
    mode: EPHEMERAL

  quality:
    profile: interactive
```

Session-only value không tự overwrite persisted preference.

---

## 23. Diagnostic Configuration

```yaml
diagnostics:
  mode: LOCAL_ONLY

  logging:
    level: info
    structured: true
    includeReadingContent: false

  tracing:
    enabled: true
    revisionTrace: true
    sampleRate: 1.0

  metrics:
    enabled: true
    authority: true
    publication: true
    queue: true
    provider: true
    lease: true
    resourceLifecycle: true
    resourcePressure: true

  snapshots:
    enabled: true
    recentEventBufferSize: 512

  debug:
    overlay: false
    showAuthority: false
    showPublication: false
    showLeases: false
    showRetention: false
```

Remote export disabled mặc định trong MVP.

---

## 24. Configuration Sources

Precedence thấp đến cao:

```text
Built-In Defaults
    ↓
Runtime Profile
    ↓
Persisted Application Configuration
    ↓
Environment Overrides
    ↓
Command-Line Overrides
    ↓
Session Overrides
    ↓
Temporary User Actions
```

Higher layer chỉ override field được cung cấp rõ ràng.

---

## 25. Merge Rules

### Scalar

Higher precedence replaces lower.

### Object

Recursive merge trừ object `replace-only`.

### Array

Mỗi array phải khai báo:

```text
REPLACE
APPEND
UNIQUE_APPEND
MERGE_BY_ID
```

Default là `REPLACE`.

### Null

Phân biệt:

```text
ABSENT
EXPLICIT_NULL
CONCRETE_VALUE
```

Meaning của `null` phải định nghĩa theo field.

---

## 26. Validation Levels

```text
Syntax Validation
    ↓
Schema Validation
    ↓
Semantic Validation
    ↓
Capability Validation
    ↓
Runtime Validation
```

### Syntax

Parse và document structure.

### Schema

Type, required, range, unknown fields.

### Semantic

Cross-field relationship.

### Capability

Provider/module có đáp ứng yêu cầu không.

### Runtime

Resource, permission, model, secret store, endpoint.

---

## 27. Runtime v2 Semantic Validation

Ví dụ:

```text
Publication requires ownership transfer
    → ownership transfer cannot be disabled

Stale completion acceptance enabled
    → validation error

Durable cache enabled
    → Storage durable-cache capability required

EPHEMERAL privacy
    → durable Artifact retention disabled

Provider executionClass = GPU
    → GPU capability required

Lease tracking disabled
    → shared Artifact processing rejected

Current-revision-first disabled
    → allowed only in test profile

Remote image processing enabled
    → explicit security policy required
```

---

## 28. Immutable Configuration Snapshot

```text
EffectiveConfigurationSnapshot
├── ConfigurationSnapshotId
├── SchemaVersion
├── CreatedAt
├── SourceVersions
├── ValidatedConfiguration
├── ActivationMetadata
└── SanitizedSummary
```

Mỗi Session, WorkItem và Attempt phải reference snapshot phù hợp.

Snapshot không chứa resolved secret.

---

## 29. Configuration and Work Identity

Mỗi asynchronous execution reference:

```text
ApplicationInstanceId
SessionId
RevisionId
WorkItemId
AttemptId
ConfigurationSnapshotId
```

Không dùng `JobId` làm vocabulary chuẩn.

Existing Attempt tiếp tục dùng original snapshot.

---

## 30. Configuration Change Flow

```text
User submits proposed changes
        ↓
Candidate Snapshot created
        ↓
Syntax / Schema / Semantic validation
        ↓
Capability and Runtime validation
        ↓
Impact Analysis
        ↓
Affected Runtime Components identified
        ↓
Activation Mode calculated
        ↓
User informed
        ↓
Configuration persisted
        ↓
Optional Admission Freeze
        ↓
Optional Authority Revocation / Drain
        ↓
Component replacement if needed
        ↓
Snapshot activated
        ↓
Events published
```

Active snapshot giữ nguyên cho đến khi activation thành công.

---

## 31. Impact Analysis

Impact Analysis phải xác định:

- changed sections;
- affected components;
- affected Session;
- current WorkItem compatibility;
- authority impact;
- publication compatibility;
- provider replacement;
- queue/admission impact;
- resource drain requirement;
- restart requirement;
- rollback strategy.

---

## 32. Immediate and New-Work Changes

Immediate examples:

- font size;
- side-panel visibility;
- logging level;
- local diagnostic UI.

New-work-only examples:

- timeout;
- Retry Policy;
- queue priority;
- context size;
- provider batching.

Existing WorkItem/Attempt không bị mutate.

---

## 33. Session-Restart Changes

Ví dụ:

- source;
- source language override;
- reading direction;
- privacy mode;
- provider profile;
- processing mode.

New config được persist nhưng pending cho Session mới.

---

## 34. Component-Restart Changes

Ví dụ:

- local model;
- provider endpoint;
- CPU pool size;
- capture backend;
- GPU backend;
- Artifact Store implementation.

Flow:

```text
Stop Admission for affected capability
    ↓
Revoke or preserve authority according to policy
    ↓
Drain Attempts
    ↓
Release Leases
    ↓
Dispose Component Resources
    ↓
Create Replacement
    ↓
Health/Capability Validation
    ↓
Activate New Snapshot
    ↓
Resume Admission
```

---

## 35. Application-Restart Changes

Ví dụ:

- data directory;
- bootstrap profile;
- secure storage implementation;
- graphics backend;
- Event Bus implementation;
- process topology.

Settings UI phải ghi rõ pending restart.

---

## 36. Configuration Events

Conceptual events:

```text
CONFIG_CHANGE_REQUESTED
CONFIG_VALIDATION_SUCCEEDED
CONFIG_VALIDATION_FAILED
CONFIG_PERSISTED
CONFIG_ACTIVATION_DEFERRED
CONFIG_ACTIVATION_STARTED
CONFIG_ACTIVATED
CONFIG_ACTIVATION_FAILED
CONFIG_ROLLBACK_STARTED
CONFIG_ROLLBACK_COMPLETED
```

Event payload không chứa secret hoặc reading content.

---

## 37. Configuration Persistence

Config persistence đi qua Storage capability.

Conceptual flow:

```text
Serialize Validated Snapshot
    ↓
Write Temporary Record
    ↓
Flush
    ↓
Validate Serialized Result
    ↓
Atomic Replace
    ↓
Retain Bounded Backup
```

Storage implementation chi tiết không thuộc Runtime Config.

---

## 38. Secrets

Configuration chỉ chứa reference:

```text
secret://translation/primary
os-keychain://crai/translation/primary
env://CRAI_TRANSLATION_API_KEY
memory://session/provider-token
```

Plain secret trong persisted config phải bị reject.

Secret Resolution qua dedicated service.

---

## 39. Redaction

Mọi config output cho:

- logs;
- events;
- snapshots;
- diagnostics;
- UI preview;
- support bundle;

phải redact:

- credential value;
- token;
- authorization header;
- signed URL;
- embedded credential;
- reading content;
- private path khi cần.

---

## 40. Versioning and Migration

Root:

```yaml
schemaVersion: 1
```

Migration:

```text
Read Version
    ↓
Find Supported Path
    ↓
Create Backup
    ↓
Migrate In Memory
    ↓
Validate
    ↓
Persist Atomically
    ↓
Record Diagnostics
```

Unsupported future version:

- không overwrite;
- không đoán semantic;
- safe mode hoặc stop activation;
- yêu cầu application version phù hợp.

---

## 41. Rollback

Rollback khi:

- component init fail;
- provider validation fail;
- Session không khởi tạo;
- repeated crash;
- explicit user request.

```text
Activation fails
    ↓
Stop affected admission
    ↓
Drain replacement attempt
    ↓
Restore previous valid snapshot
    ↓
Recreate affected components
    ↓
Publish rollback event
```

---

## 42. Safe Mode

Safe Mode:

- built-in defaults;
- remote providers disabled;
- experimental feature disabled;
- automatic continuous capture disabled;
- conservative budget;
- no runtime plugins;
- diagnostics and config recovery available;
- raw persistence disabled;
- strict authority/publication validation vẫn bật.

---

## 43. Typed Configuration Views

Conceptual API:

```text
RuntimeConfigService
├── getBootstrapConfig()
├── getRuntimeExecutionConfig()
├── getSchedulerConfig()
├── getQueueConfig()
├── getRetryConfig()
├── getCancellationConfig()
├── getAuthorityConfig()
├── getPublicationConfig()
├── getResourceConfig()
├── getLeaseConfig()
├── getCacheConfig()
├── getStorageConfig()
├── getSecurityConfig()
├── getProviderProfile()
├── getPresentationConfig()
└── getDiagnosticsConfig()
```

Business Module nhận typed view riêng của mình.

Module không:

- parse file;
- read env trực tiếp;
- mutate snapshot;
- read all config vì convenience;
- resolve arbitrary secret.

---

## 44. Configuration Ownership

| Section | Owner |
|---|---|
| `bootstrap` | Application Bootstrap |
| `application.runtime` | Runtime Control / Infrastructure |
| `application.scheduling` | Scheduler |
| `application.queues` | Work Queue |
| `application.retry` | Retry Policy |
| `application.cancellation` | Cancellation Coordinator |
| `application.authority` | Runtime Control |
| `application.publication` | Artifact Store / Runtime Control |
| `application.resources` | Resource Manager |
| `application.leases` | Resource Manager / Artifact Store |
| `application.cache` | Cache Policy |
| `application.storage` | Storage |
| `application.security` | Security/Privacy |
| `application.features` | Feature Registry |
| `preferences.*` | Owning Business Module |
| `providers.*` | Provider Manager/Adapter |
| `diagnostics` | Runtime Observability |

Configuration Service sở hữu load, merge, validation, persistence, snapshot và activation coordination.

---

## 45. Observability

Config diagnostics phải report:

- schema version;
- active snapshot;
- pending snapshot;
- selected profile;
- loaded sources;
- overridden field;
- validation warning;
- disabled component;
- provider availability;
- activation mode;
- authority-impacting change;
- drain requirement;
- rollback result.

Không report resolved secret.

---

## 46. MVP Configuration

MVP quyết định:

1. Một persisted local config.
2. Built-in defaults + user overrides.
3. OS credential store nếu có.
4. Immutable typed snapshot.
5. Validation trước activation.
6. Current Session giữ snapshot cũ.
7. Provider/source/privacy change yêu cầu Session restart.
8. Bootstrap change yêu cầu application restart.
9. Remote configuration disabled.
10. Runtime plugin loading disabled.
11. Raw capture memory-only mặc định.
12. Durable Artifact cache disabled mặc định.
13. Authority validation bắt buộc.
14. Atomic publication bắt buộc.
15. Resource Lease enabled.
16. Conservative resource budget.
17. Local diagnostics, no content.
18. Bounded config backups.
19. Safe Mode supported.

---

## 47. Example MVP Configuration

```yaml
schemaVersion: 1

bootstrap:
  environment: production
  profile: desktop-mvp
  dataDirectory: auto
  secretStore: operating-system
  recoveryMode: safe

application:
  modules:
    capture: true
    observation: true
    recognition: true
    translation: true
    presentation: true
    storage: true
    diagnostics: true

  runtime:
    execution:
      maxCpuWorkers: auto
      maxGpuExecutions: 1
      maxProviderRequests: 2

    shutdown:
      totalTimeoutMs: 10000
      drainTimeoutMs: 5000

  scheduling:
    currentRevisionFirst: true
    priority:
      control: 1000
      interactive: 100
      retry: 50
      background: 10

  queues:
    control:
      capacity: 128
      overflow: reject
    interactive:
      capacity: 64
      overflow: replace-obsolete
    background:
      capacity: 32
      overflow: reject

  retry:
    enabled: true
    attempts:
      maxAttemptsPerWorkItem: 2
      maxConcurrentRetries: 2
    backoff:
      initialDelayMs: 500
      maximumDelayMs: 5000

  cancellation:
    revokeAuthorityImmediately: true
    grace:
      defaultMs: 1000
      providerMs: 2000

  authority:
    validation:
      completion: required
      publication: required
      uiCommit: required
    staleCompletion:
      accept: false

  publication:
    requireAcceptedCandidate: true
    requireOwnershipTransfer: true
    atomicPublication: true
    allowLatePublication: false

  resources:
    budgets:
      managedMemoryMb: auto
      nativeMemoryMb: auto
      gpuMemoryMb: auto
    pressure:
      elevatedRatio: 0.70
      highRatio: 0.85
      criticalRatio: 0.95

  leases:
    enabled: true
    denyAfterLogicalDisposal: true
    leakDetection:
      enabled: true
      warningAfterMs: 10000

  cache:
    runtimeMemory:
      enabled: true
      maximumEntries: 2000
      policy: weighted-lru
    scopes:
      revisionLocal: true
      session: true
      runtime: true
      durable: false

  storage:
    backend: local
    configurationPersistence: true
    glossaryEnabled: true
    correctionMemoryEnabled: true
    durableCacheEnabled: false

  security:
    allowRemoteTextProcessing: true
    allowRemoteImageProcessing: false
    persistence:
      persistRawCapture: false
      persistRecognizedText: false
      persistTranslationCache: false
    diagnostics:
      includeReadingContent: false

preferences:
  language:
    source: auto
    target: vi
    preferredSources: [zh-Hans, zh-Hant, en]

  reading:
    contentMode: auto
    translationStyle: natural
    autoProcessStableContent: true

  capture:
    frameIntervalMs: 250
    stableDurationMs: 450
    minimumChangeRatio: 0.03

  presentation:
    mode: side-panel
    fontSize: 18
    lineHeight: 1.6
    showSourceText: true

providers:
  profiles:
    recognition-default:
      primary: local-recognition
      fallback: null
      execution:
        timeoutMs: 10000
        concurrency: 1
        executionClass: CPU

    translation-default:
      primary: primary-translator
      fallback: null
      execution:
        timeoutMs: 20000
        concurrency: 2
        executionClass: REMOTE_IO

  instances:
    local-recognition:
      type: local
      credentialRef: null
      options:
        device: auto

    primary-translator:
      type: remote
      credentialRef: secret://translation/primary
      options:
        model: configured-provider-model

diagnostics:
  mode: LOCAL_ONLY

  logging:
    level: info
    structured: true
    includeReadingContent: false

  tracing:
    enabled: true
    revisionTrace: true

  metrics:
    enabled: true
    authority: true
    publication: true
    queue: true
    provider: true
    lease: true
    resourceLifecycle: true

  snapshots:
    enabled: true
    recentEventBufferSize: 512
```

---

## 48. Error Codes

```text
CONFIG_PARSE_FAILED
CONFIG_SCHEMA_UNSUPPORTED
CONFIG_FIELD_UNKNOWN
CONFIG_FIELD_REQUIRED
CONFIG_VALUE_INVALID
CONFIG_RELATION_INVALID
CONFIG_CAPABILITY_MISSING
CONFIG_PROVIDER_NOT_REGISTERED
CONFIG_SECRET_REFERENCE_INVALID
CONFIG_SECRET_NOT_FOUND
CONFIG_PERSIST_FAILED
CONFIG_MIGRATION_FAILED
CONFIG_ACTIVATION_DEFERRED
CONFIG_ACTIVATION_FAILED
CONFIG_DRAIN_FAILED
CONFIG_ROLLBACK_FAILED
CONFIG_RESTART_REQUIRED
```

Errors normalize theo `ERROR_MODEL.md`.

---

## 49. Testing Requirements

Tests phải bao phủ:

### Defaults

- valid schema;
- no credential required;
- safe privacy;
- authority/publication validation enabled;
- durable raw cache disabled.

### Merge

- deterministic precedence;
- array strategy;
- null semantics;
- Session override isolation.

### Validation

- type/range;
- semantic relation;
- provider capability;
- authority invariant;
- publication invariant;
- Lease requirement;
- privacy precedence.

### Activation

- immediate;
- new-work-only;
- Session restart;
- Component restart;
- Application restart;
- admission freeze;
- drain;
- rollback.

### Persistence

- atomic write;
- bounded backup;
- corrupt file recovery;
- old valid snapshot retained.

### Security

- secret reference only;
- no secret in event/log/snapshot;
- remote provider cannot bypass privacy.

### Runtime v2

- existing Attempt keeps old snapshot;
- new Attempt uses activated snapshot;
- authority-impacting config revokes correctly;
- publication config cannot disable safety;
- Lease config leak detection;
- resource drain timeout;
- Scheduler/Queue config consistency;
- retry budget validation.

---

## 50. Architecture Invariants

1. Active configuration immutable.
2. Invalid configuration không activate.
3. Existing Attempt không bị mutation.
4. Every WorkItem/Attempt references ConfigurationSnapshotId.
5. Config không cấp runtime authority.
6. Authority safety validation không disable trong production.
7. Publication yêu cầu ownership transfer.
8. Worker không được configured để publish trực tiếp.
9. Retry config tách provider config.
10. Queue capacity bounded.
11. Scheduler preserves control capacity.
12. Cancellation revoke authority trước drain.
13. Resource budget bounded.
14. Lease tracking required cho shared resource.
15. Cache optional cho correctness.
16. Durable cache đi qua Storage.
17. Privacy restriction thắng provider preference.
18. Secret không nằm trong persisted normal config.
19. Secret không nằm trong event/log/snapshot.
20. Component restart theo dependency graph.
21. Config activation có rollback path khi cần.
22. Migration deterministic.
23. Unsupported future schema không overwrite.
24. Persistence atomic.
25. Safe Mode giữ authority/publication safety.
26. Module chỉ nhận typed config view.
27. Configuration Service không sở hữu business semantics.
28. Config event content-free.
29. Runtime diagnostics dùng sanitized snapshot.
30. Config change during shutdown không activate.

---

## 51. Open Questions

- YAML hay JSON?
- OS secret store implementation nào?
- Số backup config?
- Provider health check startup hay first use?
- Component change nên cancel hay drain?
- User có tạo custom runtime profile không?
- Website/series-specific preference profile?
- Adaptive tuning có được tạo temporary override không?
- Hard Lease timeout hay diagnostic-only?
- Resource budget tính theo device profile thế nào?
- Durable cache encryption policy?
- Config rollback có rollback Storage migration không?
- Authority-impacting config nào được phép NEW_WORK_ONLY?

---

## 52. Related Documents

| Document | Relationship |
|---|---|
| `PIPELINE_RUNTIME.md` | WorkItem, Attempt, authority and snapshot identity |
| `RUNTIME_COMPONENTS.md` | Config consumers and owners |
| `SCHEDULER.md` | Admission configuration |
| `WORK_QUEUE.md` | Queue classes and capacity |
| `CANCELLATION.md` | Grace and revocation settings |
| `RETRY_POLICY.md` | Retry budgets and strategy |
| `ERROR_MODEL.md` | Config error normalization |
| `MEMORY_MODEL.md` | Resource budgets |
| `CACHE_POLICY.md` | Reuse and retention settings |
| `RESOURCE_LIFECYCLE.md` | Lease, drain and disposal |
| `THREADING_MODEL.md` | Execution contexts and pools |
| `PERFORMANCE_MODEL.md` | Budget targets |
| `RUNTIME_OBSERVABILITY.md` | Diagnostic settings |
| `BOOT_SEQUENCE.md` | Startup, activation and shutdown |
| `../../modules/storage/README.md` | Configuration persistence |
| `../core/EVENT_BUS.md` | Configuration events |

---

## 53. Completion Criteria

`RUNTIME_CONFIG.md` được xem là đồng bộ khi:

- categories và activation modes rõ;
- immutable snapshot được giữ;
- WorkItem/Attempt dùng ConfigurationSnapshotId;
- Scheduler và Queue config tách riêng;
- Retry không còn chỉ nằm trong provider;
- Authority và Publication safety config tồn tại;
- Resource/Lease/Retention config đầy đủ;
- cache theo Artifact reuse;
- config persistence đi qua Storage;
- component restart có admission freeze và drain;
- events theo Runtime Event Standard;
- typed views khớp Runtime v2;
- MVP config và tests đầy đủ.

---

## 54. Summary

CRAI Runtime Configuration dùng flow:

```text
Sources
    ↓
Merge
    ↓
Validate
    ↓
Immutable Candidate Snapshot
    ↓
Impact Analysis
    ↓
Persist
    ↓
Activate at Safe Boundary
    ↓
Typed Runtime Views
```

Ranh giới cốt lõi:

```text
Configuration controls policy.

Runtime Control owns authority.

Artifact Store owns publication.

Scheduler owns admission.

Resource Manager owns lifecycle.

Storage owns persistence.
```
