# CRAI Module Dependency Architecture

> **Project:** CRAI
> **Path:** `doc/01-architecture/modules/MODULE_DEPENDENCY.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-10

---

# 1. Purpose

This document defines dependency rules between CRAI architecture modules and supporting layers.

It determines:

```text
which architecture boundary may depend on which other boundary
which contracts may cross module boundaries
which dependencies are prohibited
where implementations are wired
how Runtime interacts with semantic modules
how Infrastructure and Platform adapters remain isolated
```

This document does not redefine semantic ownership.

Semantic ownership is defined by:

```text
OWNERSHIP_MAP.md
```

---

# 2. Central Dependency Rule

The core dependency rule is:

```text
Depend on stable contracts,
not on another owner's internals.
```

And:

```text
Dependency
does not transfer ownership.
```

---

# 3. Governing Architecture

Dependencies must remain consistent with:

```text
01-architecture/core/
├── STATE_MACHINE.md
├── DATA_FLOW.md
├── EVENT_CONVENTION.md
└── EVENT_BUS.md

01-architecture/modules/
├── MODULE_MAP.md
└── OWNERSHIP_MAP.md

01-architecture/runtime/
```

---

# 4. Architecture Areas

CRAI separates:

```text
Composition

Application

Business / Application Modules

Cross-Cutting Modules

Runtime

Infrastructure

Platform Adapters

Shared Primitives
```

These are dependency areas, not necessarily source-code folders.

---

# 5. Current Primary Modules

```text
Capture
Recognition
Text Processing
Translation
Presentation
Reading Session
Preferences
Diagnostics
UI Adapter
```

These are the canonical primary module boundaries under:

```text
02-modules/
```

---

# 6. Runtime Is Separate

Runtime is not a normal semantic processing module.

Runtime owns:

```text
RuntimeRevision
WorkItem
Attempt
queueing
scheduling
retry
cancellation
deadline
supersession
backpressure
resource admission
```

Runtime does not own semantic Artifact meaning.

---

# 7. Infrastructure Is Separate

Infrastructure owns technical mechanisms such as:

```text
Event Bus
Scheduler implementation
Logging
Telemetry
Configuration
Secret Management
Resource Management
Persistence
```

Infrastructure does not own domain semantics.

---

# 8. Platform Adapters

Platform adapters wrap OS/browser/native APIs.

Examples:

```text
screen capture API
window enumeration
clipboard
file picker
notifications
accessibility API
browser extension APIs
secure storage
```

Platform adapters must contain no business orchestration.

---

# 9. Composition Root

Composition is the only architecture area allowed to know concrete implementation classes from multiple layers simultaneously.

Conceptually:

```text
Composition Root
├── creates modules
├── creates Runtime
├── creates Infrastructure
├── creates Platform adapters
├── injects provider implementations
├── wires ports
└── controls boot/shutdown wiring
```

---

# 10. Composition Is Not Business Logic

Composition must not decide:

```text
whether Translation should retry
how ReadingContext changes
which OCR region is valid
how TranslationUnits are constructed
how Presentation layout is computed
```

It only wires owners together.

---

# 11. Dependency Direction

Preferred conceptual direction:

```text
UI / External Input
        ↓
Application
        ↓
Module Public Contracts
        ↕
Runtime Execution Contracts
        ↓
Ports / Abstractions
        ↑
Infrastructure / Platform Implementations
```

This is not a strict class-layer stack where every module must depend downward through every layer.

---

# 12. Abstraction Rule

Business modules may depend on:

```text
stable module-owned contracts
Runtime-facing execution context abstractions
provider ports
storage/query ports
platform-neutral types
```

They must not depend on concrete infrastructure implementations.

---

# 13. Invalid Concrete Dependency

Invalid:

```text
Translation
    → OpenAiHttpClient
```

Preferred:

```text
Translation
    → TranslationProviderPort

Composition
    → OpenAiTranslationAdapter
    → TranslationProviderPort
```

---

# 14. No Horizontal Internal Imports

A module must never import another module's:

```text
internal/
implementation/
private mapper
provider DTO
repository implementation
mutable domain internals
```

Only public contracts may cross the boundary.

---

# 15. Public Boundary Rule

External callers may depend only on:

```text
module public contract
public immutable types
public commands/queries
public Artifact schemas
public module-defined events
```

---

# 16. Deep Import Is Forbidden

Invalid:

```text
translation/internal/provider-response-mapper
```

Valid:

```text
translation/public
```

or the implementation-language equivalent.

---

# 17. Shared Is Not a Dumping Ground

`Shared` may contain only:

```text
technology-neutral primitives
serialization helpers
immutable collections
generic validation primitives
safe string helpers
math/geometry primitives
```

Shared must not contain:

```text
global services
business state
module registry singleton
database singleton
Event Bus singleton
Runtime singleton
```

---

# 18. Service Locator Is Forbidden

Business modules must not use:

```text
GlobalContainer.get(...)
ServiceLocator.resolve(...)
globalApp.services
```

Dependencies are explicit.

---

# 19. Dependency Injection

Allowed mechanisms include:

```text
constructor injection
factory injection
explicit function parameter
explicit module context
```

Dependencies must remain visible in contracts/code.

---

# 20. Application Role

Application coordinates cross-module use cases.

Examples:

```text
StartReading
StopReading
ChangeReadingSource
RetryCurrentOperation
SavePreference
ExportDiagnosticBundle
```

Application coordinates authority.

It does not absorb semantic ownership.

---

# 21. Application May Depend On

Application may depend on public contracts from:

```text
Reading Session
Preferences
Runtime
Diagnostics
UI Adapter integration boundary
relevant semantic modules
```

depending on the use case.

---

# 22. Application Must Not Depend On

Application must not import:

```text
provider SDK
database implementation
OS API
native UI component
Runtime internal queue
Scheduler implementation internals
```

---

# 23. Business Pipeline Orchestration

Business Pipeline Orchestration determines:

```text
logical required work
Artifact dependency relationships
conditional processing paths
BusinessExecutionPlan
```

It may know semantic module contracts.

It must not execute WorkItems directly.

---

# 24. Runtime Dependency

Runtime may invoke semantic module operations through execution adapters/contracts.

Conceptually:

```text
Runtime WorkItem
    ↓
Execution Adapter
    ↓
Module Public Operation
```

Runtime must not import module internals.

---

# 25. Module → Runtime

Semantic modules may receive Runtime execution context or return execution results according to Runtime contracts.

They must not directly mutate:

```text
WorkItem state
Attempt state
RuntimeRevision state
Scheduler queue
```

---

# 26. Runtime → Module

Runtime may invoke:

```text
Capture operation
Recognition operation
Text Processing operation
Translation operation
Presentation operation
```

through stable execution interfaces.

Runtime does not reinterpret their semantic result.

---

# 27. No Module-Owned Retry Loop

Invalid:

```text
Translation
    ↓ failure
sleep
    ↓
retry provider
    ↓
retry provider
```

as global execution authority.

Preferred:

```text
Translation Attempt
    ↓
error/result classification
    ↓
Runtime Retry Policy
    ↓
new Attempt
```

---

# 28. No Module-Owned Cancellation Lifecycle

Modules may:

```text
observe cancellation
cooperate with cancellation
abort provider request when supported
```

They do not own Runtime cancellation state.

---

# 29. No Pipeline Orchestrator Stage Machine

Deprecated:

```text
Pipeline Orchestrator
    ↓
OCR_COMPLETED
    ↓
SEGMENTATION_REQUESTED
    ↓
TRANSLATION_REQUESTED
```

Current model:

```text
BusinessExecutionPlan
    ↓
Runtime dependency graph
    ↓
WorkItem readiness
```

---

# 30. Event Bus Is Not Dependency Injection

Modules must not hide mandatory direct dependencies behind Event Bus messages.

If owner A must request owner B to perform work:

```text
explicit contract
```

is preferred.

Events report committed facts.

---

# 31. Event Dependency

A module may subscribe to another owner's Event Bus fact only when:

```text
asynchronous awareness is genuinely required
```

Receiving the event does not grant authority.

---

# 32. No Command Events

Dependencies must not use:

```text
*_REQUESTED
```

Event Bus messages to perform normal command routing.

Use:

```text
Command
Query
UiIntent
Runtime contract
```

instead.

---

# 33. Capture Dependencies

Capture may depend on:

```text
platform-neutral Capture ports
Runtime execution context
configuration contracts
resource abstractions
Diagnostics observation interface
```

Capture may use platform adapters through injected ports.

---

# 34. Capture Must Not Depend On

Capture must not depend directly on:

```text
Recognition implementation
Text Processing
Translation
Presentation
UI Adapter
Preferences storage implementation
Scheduler implementation
```

---

# 35. Recognition Dependencies

Recognition may depend on:

```text
Capture Artifact public contract
OCR/provider abstractions
Runtime execution context
OCR architecture-owned internal contracts
configuration/profile contracts
Diagnostics observation interface
```

---

# 36. Recognition Must Not Depend On

Recognition must not depend directly on:

```text
Text Processing implementation
Translation
Presentation
UI Adapter
Reading Session internals
Runtime Scheduler internals
```

---

# 37. Text Processing Dependencies

Text Processing may consume:

```text
RecognitionArtifact
normalized structured-source input
text primitives
geometry/reference primitives where needed
configuration/profile snapshots
```

---

# 38. Text Processing Must Not Depend On

Text Processing must not depend on:

```text
Translation provider
TranslationUnit
TranslationBatch
Presentation
UI Adapter
Runtime Scheduler implementation
```

---

# 39. Translation Dependencies

Translation may consume:

```text
SourceDocumentArtifact
Glossary/knowledge query contracts where available
Translation provider port
configuration/profile snapshot
Runtime execution context
Diagnostics observation interface
```

---

# 40. Translation Must Not Depend On

Translation must not depend on:

```text
Text Processing internals
Presentation
UI Adapter
Reading Session internals
provider concrete classes
Scheduler concrete implementation
```

---

# 41. TranslationUnit Boundary

Translation owns:

```text
TranslationUnit
TranslationBatch
```

Other modules may reference these only through Translation public contracts where needed.

Text Processing must not create Translation-owned units.

---

# 42. Presentation Dependencies

Presentation may consume:

```text
TranslationArtifact
presentation configuration snapshot
geometry references
semantic typography/layout constraints
```

---

# 43. Presentation Must Not Depend On

Presentation must not depend directly on:

```text
native UI framework
window implementation
Translation provider
Recognition provider
Runtime retry implementation
UI local state
```

---

# 44. UI Adapter Dependencies

UI Adapter may depend on:

```text
Application-facing contracts
public module/application snapshots
PresentationArtifact/projection contracts
Preferences/application settings projection
Diagnostics projection
platform UI ports
```

---

# 45. UI Adapter Must Not Depend On

UI Adapter must not depend directly on:

```text
Translation provider
Recognition provider
Storage implementation
Scheduler implementation
Runtime worker internals
business module internals
```

---

# 46. Reading Session Dependencies

Reading Session may depend on:

```text
session-owned validation
source/context public contracts
configuration primitives
Clock abstraction where needed
persistence port where needed
```

Application may use Reading Session together with Runtime.

Reading Session itself must not orchestrate Runtime internals.

---

# 47. Reading Session Must Not Depend On

Reading Session must not depend on:

```text
Recognition implementation
Translation implementation
Presentation implementation
Scheduler
Work Queue
provider SDK
UI framework
```

---

# 48. Preferences Dependencies

Preferences may depend on:

```text
preference storage port
validation primitives
configuration primitives
Clock where needed
```

---

# 49. Preferences Must Not Depend On

Preferences must not depend on:

```text
UI implementation
Reading Session internals
Runtime execution
Translation provider
Presentation renderer
```

Reading Session may consume resolved/session configuration through Application contracts.

---

# 50. Diagnostics Dependencies

Diagnostics may depend on public observation/snapshot interfaces from:

```text
Runtime
modules
Infrastructure health abstractions
```

It may aggregate state without owning it.

---

# 51. Diagnostics Must Not Depend On Internals

Diagnostics must not directly inspect:

```text
private module state
raw provider client
database implementation internals
Runtime queue internals
native UI internals
```

unless a dedicated diagnostic port explicitly exposes safe information.

---

# 52. Diagnostics and Logging

Diagnostics may depend on:

```text
Logging abstraction
Telemetry abstraction
```

through explicit ports.

Logging infrastructure must not depend on Diagnostics business internals.

---

# 53. Diagnostics and Telemetry

Telemetry carries:

```text
metrics
traces
```

Diagnostics owns diagnostic semantics.

Neither should use the business Event Bus as a mandatory transport replacement.

---

# 54. Provider Management Dependencies

Provider Management may depend on:

```text
provider manifests
capability contracts
configuration
Secret Management port
health observation ports
```

---

# 55. Provider Management Must Not Own Execution

Provider Management must not directly:

```text
create WorkItem
create Attempt
commit retry
commit cancellation
start Translation pipeline
```

Runtime owns execution.

---

# 56. Provider Adapter Boundary

Provider adapters depend inward on semantic provider ports.

Example:

```text
OpenAI Adapter
    → TranslationProviderPort
```

not:

```text
Translation
    → OpenAI Adapter
```

---

# 57. Provider DTO Rule

Provider-native DTOs stay inside adapters.

Invalid cross-boundary types:

```text
OpenAiResponse
PaddleBoxes
GoogleVisionAnnotation
DeepLResponse
```

Normalize them first.

---

# 58. Infrastructure Dependency Direction

Infrastructure implementations may depend on:

```text
public abstractions/ports
shared primitives
technical libraries
```

They must not depend on Application orchestration.

---

# 59. Invalid Infrastructure Dependency

Invalid:

```text
SQLitePreferencesRepository
    → ReadingSessionApplicationService

CloudRecognitionAdapter
    → RuntimeCoordinator
```

Infrastructure returns data/results through its port.

---

# 60. Platform Dependency Direction

Platform adapters may depend on:

```text
platform-neutral ports
shared primitives
native OS/browser APIs
```

They do not make business decisions.

---

# 61. Invalid Platform Dependency

Invalid:

```text
WindowManager
    → pause Reading Session

ScreenCapture
    → invoke Recognition

ClipboardAdapter
    → start Translation
```

---

# 62. Storage Dependencies

Semantic modules depend on:

```text
repository/storage ports
```

where persistence is required.

Infrastructure implements those ports.

---

# 63. Storage Is Not Universal Database Service

Avoid:

```text
DatabaseService.query(...)
```

inside every module.

Use owner-specific repositories/ports.

---

# 64. Repository Ownership

Repository interface should normally be defined close to the semantic owner requiring persistence.

Example:

```text
Preferences
    owns PreferenceRepository port

Storage implementation
    implements PreferenceRepository
```

---

# 65. Cache Dependencies

Modules may define semantic cache compatibility.

Runtime/cache infrastructure owns:

```text
physical cache lifecycle
eviction
budget
global retention
```

---

# 66. Artifact Storage Dependency

A module may use an Artifact/resource-storage abstraction.

The storage mechanism does not become semantic owner of the Artifact.

---

# 67. Artifact Boundary Rule

Preferred cross-module semantic dependency:

```text
Recognition
    publishes RecognitionArtifact contract

Text Processing
    consumes RecognitionArtifact
```

not:

```text
Text Processing
    reads Recognition internal model
```

---

# 68. Semantic Dependency Chain

Main semantic dependencies:

```text
Capture Artifact
    ↓
Recognition

RecognitionArtifact
    ↓
Text Processing

SourceDocumentArtifact
    ↓
Translation

TranslationArtifact
    ↓
Presentation

Presentation/Application Projection
    ↓
UI Adapter
```

---

# 69. Semantic Dependency Is Not Execution Trigger

The relationship:

```text
Translation consumes SourceDocumentArtifact
```

does not mean:

```text
Text Processing calls Translation
```

Runtime/business orchestration determines executable dependencies.

---

# 70. Structured Text Alternate Path

Structured sources may enter:

```text
Text Processing
```

without depending on Capture/Recognition execution.

Therefore dependencies must support alternate processing paths.

---

# 71. Application-to-Runtime Dependency

Application may submit:

```text
BusinessExecutionPlan
RuntimeRevision creation/update request
cancellation/supersession request
```

through Runtime public contracts.

Application must not manipulate Runtime internal queue state.

---

# 72. Runtime-to-Infrastructure Dependency

Runtime may depend on abstractions implemented by:

```text
Scheduler Infrastructure
Resource Manager
Clock
Telemetry
Logging
Configuration
```

through stable contracts.

---

# 73. Scheduler Boundary

Runtime owns scheduling policy semantics.

Infrastructure Scheduler implements scheduling mechanisms.

Business modules must not call the Scheduler directly.

---

# 74. Resource Manager Boundary

Runtime determines resource policy.

Infrastructure Resource Manager implements resource accounting/admission mechanisms.

Modules may declare resource requirements through Runtime contracts.

---

# 75. Cancellation Boundary

Application/owner conditions may request cancellation.

Runtime owns cancellation propagation/execution authority.

Semantic modules only cooperate with the provided cancellation context.

---

# 76. Retry Boundary

Module:

```text
returns semantic/error information
```

Runtime:

```text
evaluates Retry Policy
creates another Attempt
```

Provider Management may contribute availability/suitability information.

---

# 77. Fallback Boundary

Provider fallback may involve:

```text
semantic module
    → suitability/error classification

Provider Management
    → provider candidates

Runtime
    → next Attempt
```

There is no required standalone `Fallback Coordinator` module.

---

# 78. Removed Retry Coordinator Dependency

The v1:

```text
Retry Coordinator
```

is not a current top-level dependency owner.

Retry execution belongs to Runtime.

---

# 79. Removed Cancellation Coordinator Dependency

The v1:

```text
Cancellation Coordinator
```

is replaced by Runtime cancellation architecture.

---

# 80. Removed Resource Coordinator Dependency

The v1 generic:

```text
Resource Coordinator
```

is replaced by:

```text
Runtime resource policy
+
Resource Manager Infrastructure
```

---

# 81. Removed Pipeline Orchestrator Dependency

The v1 `Pipeline Orchestrator` owning pipeline state and next-stage commands is deprecated.

Use:

```text
Business Pipeline Orchestration
+
Runtime dependency graph
```

---

# 82. Dependency Matrix — Semantic Modules

| From            | Allowed Semantic Dependencies                                              |
| --------------- | -------------------------------------------------------------------------- |
| Capture         | platform/source ports, shared contracts                                    |
| Recognition     | Capture Artifact contract, provider ports, OCR detail contracts            |
| Text Processing | RecognitionArtifact / structured-source contracts                          |
| Translation     | SourceDocumentArtifact, glossary/knowledge query contracts, provider ports |
| Presentation    | TranslationArtifact, presentation configuration                            |
| Reading Session | source/session primitives, persistence ports                               |
| Preferences     | persistence/config primitives                                              |
| Diagnostics     | public observations/snapshots                                              |
| UI Adapter      | Application/public projection contracts                                    |

---

# 83. Dependency Matrix — Runtime

| From            | Allowed Dependencies                          |
| --------------- | --------------------------------------------- |
| Application     | Runtime public API                            |
| Runtime         | module execution contracts                    |
| Runtime         | Scheduler abstraction                         |
| Runtime         | Resource Manager abstraction                  |
| Runtime         | Clock                                         |
| Runtime         | Logging/Telemetry abstractions                |
| Runtime         | Configuration                                 |
| Semantic module | Runtime execution context only where required |

---

# 84. Dependency Matrix — Infrastructure

| Infrastructure    | Implements                      |
| ----------------- | ------------------------------- |
| Event Bus         | Event Bus abstraction           |
| Scheduler         | Runtime scheduling abstraction  |
| Logging           | Logging abstraction             |
| Telemetry         | Metrics/tracing abstraction     |
| Resource Manager  | Resource-management abstraction |
| Secret Management | Secret access abstraction       |
| Storage           | repository/storage ports        |
| Configuration     | configuration ports             |

---

# 85. Dependency Matrix — Platform

| Platform Adapter  | Typical Port        |
| ----------------- | ------------------- |
| Screen Capture    | CapturePlatformPort |
| Window Management | WindowPlatformPort  |
| Clipboard         | ClipboardPort       |
| Accessibility     | AccessibilityPort   |
| Notifications     | NotificationPort    |
| File Picker       | FilePickerPort      |
| Secure Storage    | SecretStoragePort   |

Exact names remain implementation decisions.

---

# 86. Forbidden Dependency — Processing Chain

Forbidden:

```text
Capture → Recognition execution

Recognition → Text Processing execution

Text Processing → Translation execution

Translation → Presentation execution
```

through direct imperative calls that bypass Runtime orchestration.

---

# 87. Allowed Artifact Consumption

Allowed:

```text
Text Processing contract
    imports RecognitionArtifact schema

Translation contract
    imports SourceDocumentArtifact schema
```

when these are stable owner-public contracts.

---

# 88. Forbidden Dependency — UI to Infrastructure

Invalid:

```text
UI Adapter → SQLite

UI Adapter → OpenAI Client

UI Adapter → PaddleOCR

UI Adapter → Runtime Scheduler implementation
```

---

# 89. Forbidden Dependency — Infrastructure to Application

Invalid:

```text
Storage → Application use case

Telemetry → RetryCurrentOperation

Recognition provider → Business Pipeline Orchestrator
```

---

# 90. Forbidden Dependency — Runtime to UI

Runtime must not depend on:

```text
ViewModel
DialogModel
React
Flutter
Qt
native UI state
```

---

# 91. Forbidden Dependency — Presentation to UI Framework

Presentation semantic module must remain UI-framework neutral.

UI Adapter/platform performs native adaptation.

---

# 92. Forbidden Dependency — Preferences to Reading Session

Preferences must not mutate Reading Session directly.

Application coordinates:

```text
Preference changed
    ↓
effective configuration evaluation
    ↓
Reading Session/Application action if needed
```

---

# 93. Forbidden Dependency — Diagnostics Ownership

Diagnostics must not call module mutation APIs merely to “fix” observed failures unless an explicit Application recovery use case exists.

---

# 94. Command Ownership

Commands belong to explicit owner interfaces.

Examples:

```text
StartReading
    → Application

PauseReadingSession
    → Reading Session/Application contract

SetPreference
    → Preferences

Cancel Runtime work
    → Runtime
```

They are not Event Bus events.

---

# 95. Event Ownership

Exact events are owned by producing modules and defined in:

```text
02-modules/<module>/EVENTS.md
```

No global event publisher table should redefine them here.

---

# 96. Event Subscription Dependency

Subscription does not create a semantic code dependency on producer internals.

Consumers depend on the event contract only.

---

# 97. Event Bus Dependency

Modules may depend on an Event Publisher abstraction only when they actually publish module-owned facts.

They should not depend on concrete Event Bus implementation.

---

# 98. Application Lifecycle

Application lifecycle may coordinate:

```text
boot
readiness
shutdown
```

It may invoke module lifecycle contracts.

It must not become owner of module-local lifecycle state.

---

# 99. Module Lifecycle

Each module owns its semantic lifecycle according to its `STATES.md`.

Composition/Application coordinates initialization order.

---

# 100. Module Registry

A Module Registry may exist for:

```text
discovery
composition
lifecycle coordination
health visibility
```

It does not own module semantic lifecycle state.

---

# 101. Initialization

Recommended high-level dependency order:

```text
Configuration / Secrets
    ↓
Infrastructure primitives
    ↓
Runtime
    ↓
Semantic modules
    ↓
Application
    ↓
UI Adapter / frontend activation
```

Exact boot sequence belongs to Runtime/Application boot architecture.

---

# 102. Shutdown

Recommended dependency-aware shutdown:

```text
stop accepting new use cases
    ↓
request Runtime cancellation
    ↓
stop session/application activity
    ↓
stop UI/frontends
    ↓
dispose semantic modules
    ↓
flush Diagnostics/Telemetry/Logging
    ↓
dispose Infrastructure
```

Exact authority belongs to boot/shutdown documents.

---

# 103. Shutdown Must Be Bounded

No module may hold shutdown indefinitely because:

```text
provider ignored cancellation
subscriber hung
network request never returned
```

Timeout/bounded shutdown policy applies.

---

# 104. Multi-Process Compatibility

Public boundaries should remain serializable where practical.

Avoid public contracts containing:

```text
thread
mutex
native pointer
framework component
provider SDK object
database connection
closure
```

---

# 105. Multi-Process Identity

Use typed identities such as:

```text
SessionId
ReadingContextRevision
RuntimeRevisionId
WorkItemId
AttemptId
ArtifactId
```

Do not use generic legacy:

```text
PipelineId
TaskId
```

as universal identifiers.

---

# 106. Process Topology

Whether a module executes:

```text
in-process
worker process
remote process
```

must not change its semantic public contract unnecessarily.

---

# 107. Local/Remote Transparency

Example:

```text
RecognitionExecutionPort
```

may be implemented by:

```text
LocalRecognitionAdapter
```

or:

```text
RecognitionWorkerProxy
```

The semantic module should not need to know which topology is active.

---

# 108. Native Handle Rule

Native handles must remain inside Platform/adapter boundaries.

Do not send raw:

```text
HWND
DOM Node
Qt pointer
browser tab object
native graphics surface
```

through stable public contracts.

---

# 109. Artifact Reference Rule

Large data may cross process/module boundaries using:

```text
ArtifactRef
BlobRef
ContentRef
```

where explicitly defined.

A reference does not change semantic ownership.

---

# 110. Provider Plugin Boundary

Provider implementations are replaceable adapters.

A provider plugin may declare:

```text
providerId
capabilities
supported languages
batching support
streaming support
cancellation support
configuration schema
```

Plugin manifest details belong to Provider/plugin architecture.

---

# 111. Provider Plugin Dependency

Plugin implementation:

```text
depends inward on provider port
```

Semantic modules do not import plugin implementation.

---

# 112. Optional Dependencies

Optional dependencies must be explicit.

Examples:

```text
TranslationMemoryQuery?
AdvancedTelemetry?
OptionalProviderCapability?
```

Do not detect optional dependencies through global singletons.

---

# 113. Null Object

A Null Object may be appropriate for true optional technical capabilities such as:

```text
NoopTelemetry
NoopMetrics
```

Do not use Null Object to hide missing mandatory business dependencies.

---

# 114. Architecture Enforcement

Dependency rules should eventually be enforceable through:

```text
import rules
dependency graph tests
cycle detection
public/internal boundaries
module manifest validation
architecture tests
```

Exact tooling depends on technology selection.

---

# 115. Cycle Prevention

Circular dependency is prohibited unless architecture explicitly introduces a neutral owner contract that breaks the cycle.

Invalid:

```text
Translation
    → Knowledge
    → Translation internals
```

Preferred:

```text
Translation
    → KnowledgeQuery

Knowledge
    does not depend on Translation
```

---

# 116. Shared Contract Placement

A shared contract belongs either:

```text
to the semantic owner
```

or:

```text
to a genuinely neutral architecture primitive
```

Do not move a contract to Shared merely because two modules use it.

---

# 117. Example — Recognition to Text Processing

Correct:

```text
Recognition
    ↓
Published RecognitionArtifact

Runtime
    ↓
Text Processing WorkItem becomes ready

Text Processing
    ↓
consumes RecognitionArtifact
```

Incorrect:

```text
RecognitionService
    ↓
textProcessingService.process()
```

as pipeline control.

---

# 118. Example — Translation to Presentation

Correct:

```text
Published TranslationArtifact
    ↓
Runtime/Application dependency condition
    ↓
Presentation operation
```

Incorrect:

```text
Translation
    ↓
PresentationRenderer.render()
```

---

# 119. Example — Preference Change

Correct:

```text
UI Adapter
    ↓
SavePreferenceIntent
    ↓
Application
    ↓
Preferences
```

then:

```text
PreferenceChanged
    ↓
Application projection / relevant consumers
```

Preferences does not directly mutate UI.

---

# 120. Example — Retry

Correct:

```text
Translation Attempt
    ↓
TRN-* failure
    ↓
Runtime classifies policy
    ↓
Attempt N+1
```

UI Adapter only submits:

```text
RetryCurrentOperationIntent
```

when user action is involved.

---

# 121. Example — Cancellation

Correct:

```text
Reading Session stop
    ↓
Application
    ↓
Runtime cancellation request
    ↓
affected WorkItems/Attempts
```

Reading Session does not traverse module internals to cancel each stage.

---

# 122. Example — Diagnostics

Correct:

```text
Recognition
    ↓
ObserveError(REC-...)
    ↓
Diagnostics
```

Diagnostics does not depend on Recognition private state.

---

# 123. Example — Storage

Correct:

```text
Preferences
    ↓
PreferenceRepository port
    ↓
Storage implementation
```

Invalid:

```text
Preferences
    ↓
SQLiteConnection
```

---

# 124. Example — Platform Capture

Correct:

```text
Capture
    ↓
ScreenCapturePort
    ↓
WindowsScreenCaptureAdapter
```

Capture remains platform-neutral.

---

# 125. Legacy Dependencies Removed

The following v1 architecture dependencies are deprecated:

```text
Session Orchestrator
    → Cancellation Coordinator

Pipeline Orchestrator
    → Retry Coordinator
    → Fallback Coordinator
    → Cancellation Coordinator

OCR
    → Segmentation

Segmentation
    → Translation

Translation
    → Rendering

UI
    → *_REQUESTED Event Bus commands
```

---

# 126. Legacy Module Names Removed

Dependency definitions should no longer use top-level:

```text
OCR Feature
Segmentation Feature
Post-processing Feature
Rendering Feature
Source Watching Feature
Cache Coordination Feature
```

unless referring to historical/internal capability decomposition.

Current primary modules are defined in `MODULE_MAP.md`.

---

# 127. Current Dependency Graph

Conceptually:

```text
                    ┌───────────────┐
                    │   UI Adapter  │
                    └───────┬───────┘
                            ↓
                    ┌───────────────┐
                    │  Application  │
                    └───────┬───────┘
                            │
             ┌──────────────┼──────────────┐
             ↓              ↓              ↓
      Reading Session   Preferences     Diagnostics
             │
             ↓
    Business Execution Planning
             ↓
          Runtime
             │
   ┌─────────┼───────────────────────────────┐
   ↓         ↓          ↓          ↓         ↓
Capture  Recognition  Text Proc  Translation Presentation
```

Infrastructure/Platform implement ports around these boundaries.

---

# 128. Semantic Artifact Graph

```text
Capture Artifact
    ↓
RecognitionArtifact
    ↓
SourceDocumentArtifact
    ↓
TranslationArtifact
    ↓
PresentationArtifact
```

This graph defines data compatibility.

Runtime defines execution.

---

# 129. Runtime Dependency Graph

Conceptually:

```text
RuntimeRevision
    ↓
WorkItems
    ↓
dependency relationships
    ↓
Attempts
```

Runtime WorkItem graph must not be reconstructed through Event Bus subscriptions.

---

# 130. Allowed Direct Module Dependencies

Direct module-to-module dependency may be allowed when it is:

```text
read-only Query dependency

stable Artifact schema dependency

explicit semantic port dependency
```

and does not transfer orchestration authority.

---

# 131. Query Dependency

Example:

```text
Translation
    → GlossaryQuery
```

if Knowledge/Glossary remains a separate owner.

The Query must be read-only.

---

# 132. Artifact Schema Dependency

Example:

```text
Translation
    → SourceDocumentArtifact contract
```

This is valid semantic input dependency.

---

# 133. Explicit Port Dependency

Example:

```text
Recognition
    → RecognitionProviderPort
```

Implementation is injected externally.

---

# 134. Forbidden “God Application”

Application must not become:

```text
PipelineOrchestrator
RetryCoordinator
FallbackCoordinator
ResourceCoordinator
CacheCoordinator
ProviderExecutor
StateGodObject
```

Application coordinates use cases while delegating execution to Runtime and semantics to modules.

---

# 135. Forbidden “God Runtime”

Runtime must not become:

```text
OCR semantic engine
Translation context builder
Presentation layout engine
Preference validator
Reading Session owner
```

Runtime owns execution only.

---

# 136. Forbidden “God Core”

Core/shared abstractions must not accumulate:

```text
all domain models
all module states
all errors
all events
all repositories
all configuration semantics
```

Owner-specific concepts remain with owners.

---

# 137. Module Contract Documentation

Every current module documents its boundary in:

```text
02-modules/<module>/
├── MODULE.md
├── CONTRACT.md
├── STATES.md
├── EVENTS.md
├── ERRORS.md
└── README.md
```

Dependency analysis must use those documents rather than legacy source-code sketches.

---

# 138. Source-Code Structure Is Not Yet Canonical

The old v1 source blueprint:

```text
presentation/
application/
features/
core/
infrastructure/
platform/
shared/
```

may inspire implementation.

It is not yet a committed technology-neutral source-code structure.

Final source layout depends on technology selection.

---

# 139. Do Not Freeze TypeScript Structure

Examples such as:

```text
index.ts
contracts.ts
dependency-cruiser
eslint boundaries
```

must remain illustrative until technology stack is chosen.

Architecture rules are language-neutral.

---

# 140. Module Manifest

A Module Manifest may later include:

```text
moduleId
version
dependencies
optionalDependencies
capabilities
lifecycle contract
```

but exact code/schema remains a technology decision.

---

# 141. Module Lifecycle Dependency

Module lifecycle coordination may depend on explicit lifecycle contracts.

A Module Registry may coordinate ordering.

It does not own semantic module state.

---

# 142. Failure Isolation

Dependency design should ensure one optional module/capability failure does not automatically fail all consumers.

Examples:

```text
Diagnostics degraded
    ≠
Translation unavailable

Notification unavailable
    ≠
UI Adapter globally failed
```

---

# 143. Provider Failure Isolation

Provider implementation failure should be returned through module/provider contracts.

It must not directly mutate Application/session state.

---

# 144. Process Failure Isolation

If a worker process fails:

```text
Process Topology / Runtime
    detects failure
```

Semantic modules retain their contracts.

Recovery must not require callers to know whether implementation was local or remote.

---

# 145. Privacy Dependency Rule

Modules should receive only data needed for their responsibility.

Example:

```text
Translation provider
```

must not receive:

```text
whole screen
credential objects
unrelated session history
```

unless explicitly required.

---

# 146. Secret Dependency

Only provider/platform infrastructure requiring credentials should depend on Secret Management interfaces.

Business Artifacts must never carry secrets.

---

# 147. Logging Dependency

Modules may depend on a safe Logging/Diagnostics abstraction.

They must not depend on concrete sinks/backend SDKs.

---

# 148. Telemetry Dependency

Modules/Runtime may emit metrics/traces through Telemetry abstraction.

They must not publish telemetry as business Event Bus events.

---

# 149. Dependency Testing

Required architecture tests should eventually detect:

```text
cycles
deep imports
Infrastructure → Application imports
Platform → business orchestration
UI → Infrastructure imports
business module → provider concrete implementation
module → Runtime internals
Runtime → UI
Shared service locator
```

---

# 150. Dependency Review Checklist

Before adding dependency:

1. Who owns the target concept?

2. Is this a public contract?

3. Is this dependency semantic, execution, infrastructure or platform?

4. Could a Query or Artifact schema be sufficient?

5. Is the caller accidentally taking ownership?

6. Is this hidden orchestration?

7. Is Event Bus being misused as command transport?

8. Does this leak implementation/provider DTO?

9. Does this create a cycle?

10. Does it remain serializable/process-neutral where needed?

11. Can the implementation be injected?

12. Can the dependency be tested independently?

---

# 151. Open Decisions

Still open:

```text
technology stack
source-code package layout
process topology
provider plugin loading model
dependency-enforcement tooling
persistence technology
Artifact physical storage
Knowledge/Translation Memory module boundary
browser integration topology
```

---

# 152. Closed Decisions

No longer open:

```text
Runtime owns WorkItem/Attempt

Runtime owns retry mechanics

Runtime owns cancellation mechanics

Reading Session owns ReadingContextRevision

Text Processing owns SourceDocumentArtifact

Translation owns TranslationUnit/Batch

Presentation is separate from native UI rendering

Event Bus is fact-only

Pipeline stage execution is not controlled through *_REQUESTED events

current primary module list is defined in MODULE_MAP.md
```

---

# 153. Architecture Invariants

1. Dependencies target public contracts.

2. Dependency does not transfer semantic ownership.

3. Deep imports are forbidden.

4. Provider concrete implementations are injected.

5. Infrastructure does not depend on Application.

6. Platform does not make business decisions.

7. UI Adapter does not call Infrastructure directly.

8. Runtime does not depend on UI.

9. Runtime owns WorkItem/Attempt execution.

10. Modules do not own Runtime retry.

11. Modules do not own Runtime cancellation.

12. Scheduler is not called directly by business modules.

13. Event Bus does not route commands.

14. Module events do not directly trigger downstream execution.

15. Semantic Artifacts are valid cross-module dependencies.

16. Module internals are not cross-module dependencies.

17. Query dependencies are read-only.

18. Composition Root is the only place allowed to know concrete implementations broadly.

19. Service Locator is forbidden in business code.

20. Shared contains no business singleton.

21. Provider DTOs never cross adapter boundaries.

22. Storage implementation does not leak into semantic modules.

23. Repository interfaces remain owner-oriented.

24. Public contracts remain platform-neutral.

25. Public contracts should remain serializable where practical.

26. Native handles do not cross stable boundaries.

27. Reading Session does not depend on processing modules.

28. Text Processing does not depend on Translation internals.

29. Translation does not depend on Presentation.

30. Presentation does not depend on UI framework.

31. Diagnostics observes without owning.

32. Preferences does not mutate Reading Session directly.

33. Application coordinates but does not become a god orchestrator.

34. Runtime executes but does not become semantic owner.

35. Infrastructure implements mechanisms but does not become business owner.

---

# 154. Deprecated v1 Architecture

The following current-authority concepts from v1 are deprecated:

```text
Feature Layer as canonical module set

OCR Feature
Segmentation Feature
Post-processing Feature
Rendering Feature

Pipeline Orchestrator
Retry Coordinator
Fallback Coordinator
Cancellation Coordinator
Resource Coordinator

PipelineState
PipelineContext
pipelineId as universal authority

*_REQUESTED command events

OCR_COMPLETED → SEGMENTATION_REQUESTED
SEGMENTATION_COMPLETED → TRANSLATION_REQUESTED
TRANSLATION_COMPLETED → POST_PROCESSING_REQUESTED

Core Scheduler as business-facing dependency
Core Cancellation as feature-facing coordinator
```

---

# 155. Preserved v1 Principles

The following remain valid:

```text
dependency toward abstraction
single semantic owner
provider replaceability
Composition Root wiring
public/internal boundary
deep-import prohibition
cycle prevention
dependency injection
Service Locator prohibition
provider DTO isolation
serializable public contracts
multi-process readiness
Shared discipline
architecture enforcement tests
```

---

# 156. Related Documents

```text
doc/01-architecture/core/
├── STATE_MACHINE.md
├── DATA_FLOW.md
├── EVENT_CONVENTION.md
└── EVENT_BUS.md

doc/01-architecture/modules/
├── MODULE_MAP.md
├── OWNERSHIP_MAP.md
├── MODULE_DEPENDENCY.md
└── README.md

doc/01-architecture/runtime/
├── BUSINESS_PIPELINE_ORCHESTRATION.md
├── PIPELINE_RUNTIME.md
├── WORK_QUEUE.md
├── SCHEDULER.md
├── RETRY_POLICY.md
├── CANCELLATION.md
└── RUNTIME_COMPONENTS.md

doc/02-modules/

doc/03-infrastructure/
```

---

# 157. Completion Criteria

This dependency architecture is synchronized when:

* the current `02-modules/` module set replaces the legacy Feature list;
* Runtime is separated from semantic modules;
* Pipeline Orchestrator is removed as execution authority;
* Retry/Fallback/Cancellation coordinators are removed as top-level application owners;
* direct stage chaining is absent;
* `_REQUESTED` Event Bus commands are absent;
* Artifact contracts replace stage-internal data dependencies;
* Application/Runtime responsibilities are separated;
* Infrastructure/Platform implementations depend inward on ports;
* UI Adapter does not access Infrastructure directly;
* provider concrete classes remain behind adapters;
* deep imports remain prohibited;
* Composition Root remains the implementation-wiring boundary;
* source-code folder layout remains technology-neutral until Technology Selection.

---

# 158. Summary

The v1 dependency model broadly used:

```text
Presentation
    ↓
Application Orchestrators
    ↓
Feature Modules
    ↓
Core

Pipeline Orchestrator
    ↓
Requested Events
    ↓
Feature Stages
```

Runtime v2 uses:

```text
UI Adapter
    ↓
Application
    ↓
Domain / Module Authority
    ↓
Business Execution Planning
    ↓
Runtime
    ↓
WorkItems / Attempts
    ↓
Module Public Execution Contracts
```

Semantic data moves through:

```text
Capture Artifact
    ↓
RecognitionArtifact
    ↓
SourceDocumentArtifact
    ↓
TranslationArtifact
    ↓
PresentationArtifact
```

Infrastructure surrounds this architecture through injected ports.

The central dependency rule is:

```text
Modules depend on contracts.

Runtime executes work.

Application coordinates use cases.

Infrastructure implements mechanisms.

No dependency
may silently transfer ownership.
```
