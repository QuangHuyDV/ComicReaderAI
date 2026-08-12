# CRAI Architecture Ownership Map

> **Project:** CRAI
> **Path:** `doc/01-architecture/modules/OWNERSHIP_MAP.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-10

---

# 1. Purpose

This document defines authoritative ownership for architecture concepts across CRAI.

Its purpose is to ensure:

```text
One concept
    ↓
One semantic owner
    ↓
Many consumers may reference it
```

A consumer may:

```text
use
reference
project
observe
persist
transport
cache
```

a concept without becoming its owner.

---

# 2. Central Ownership Rule

The core rule is:

```text
One architectural concept
        ↓
One authoritative semantic owner
        ↓
0..N consumers
```

There must not be:

```text
Document A
    defines Concept X

Document B
    defines Concept X differently
```

---

# 3. Owner

An Owner is the architecture boundary allowed to define:

```text
semantic meaning
canonical contract
authoritative state
lifecycle when applicable
revision semantics
validation rules
module-owned errors/events
```

---

# 4. Consumer

A Consumer may:

```text
reference an owner contract
consume immutable output
display/project it
persist it
transport it
observe it
```

but must not redefine its semantics.

---

# 5. Ownership vs Storage

Persisting data does not transfer semantic ownership.

Example:

```text
Preferences
    owns Preference semantics

Storage
    persists Preference data
```

Storage does not become the owner of `Preference`.

---

# 6. Ownership vs Runtime

Executing work does not transfer semantic ownership.

Example:

```text
Translation
    owns TranslationArtifact semantics

Runtime
    executes Translation WorkItem/Attempt
```

Runtime does not become semantic owner of Translation output.

---

# 7. Ownership vs UI

Displaying data does not transfer ownership.

Example:

```text
Reading Session
    owns ReadingContext

UI Adapter
    projects ReadingContext state
```

UI Adapter does not own ReadingContext.

---

# 8. Ownership vs Diagnostics

Observing data does not transfer ownership.

Example:

```text
Recognition
    owns REC-* errors

Diagnostics
    observes REC-* error
```

Diagnostics does not rename it into a Diagnostics-owned error.

---

# 9. Architecture Areas

Current CRAI architecture separates:

```text
Core Architecture

Business / Application Modules

Cross-Cutting Modules

Runtime Architecture

Detailed OCR Architecture

Infrastructure

Provider / Platform Adapters
```

---

# 10. High-Level Ownership

| Architecture Area     | Owns                                               |
| --------------------- | -------------------------------------------------- |
| Core Architecture     | Cross-project invariants and semantic rules        |
| Business Modules      | Business/domain semantics                          |
| Cross-Cutting Modules | Cross-domain adapter/diagnostic semantics          |
| Runtime               | Execution authority                                |
| OCR Architecture      | Detailed image-to-recognition processing semantics |
| Infrastructure        | Technical mechanisms/contracts                     |
| Provider Adapters     | External API/SDK normalization                     |
| Platform Adapters     | OS/browser/native integration                      |

---

# 11. Core Architecture Ownership

## Owner

```text
01-architecture/core/
```

Owns architecture-wide rules for:

```text
capability semantics
state authority convention
cross-module data-flow convention
event semantics
event distribution principles
```

It does not own module-specific contracts.

---

# 12. Capability Model

## Owner

```text
01-architecture/core/CAPABILITY_MAP.md
```

Owns:

```text
product capability taxonomy
capability feasibility
capability status
prototype gates
```

It does not create module ownership automatically.

---

# 13. State Authority Convention

## Owner

```text
01-architecture/core/STATE_MACHINE.md
```

Owns:

```text
architecture-wide state ownership rules
domain vs Runtime state separation
revision authority principles
Candidate vs Published authority
transition rules
```

Module-specific lifecycle remains module-owned.

---

# 14. Data Flow Convention

## Owner

```text
01-architecture/core/DATA_FLOW.md
```

Owns:

```text
architecture-wide data movement
Artifact-boundary conventions
Candidate → Published flow
cross-module provenance rules
Runtime/data separation
```

Exact Artifact schemas remain module-owned.

---

# 15. Event Semantics Convention

## Owner

```text
01-architecture/core/EVENT_CONVENTION.md
```

Owns:

```text
fact-only event semantics
event naming
event ownership convention
payload conventions
versioning rules
```

---

# 16. Event Bus Architecture

## Owner

```text
01-architecture/core/EVENT_BUS.md
```

Owns:

```text
Event Bus role
delivery semantics
subscriber isolation
ordering principles
deduplication principles
publish-after-commit rule
```

Infrastructure implementation belongs to:

```text
03-infrastructure/event-bus/
```

---

# 17. Module Topology

## Owner

```text
01-architecture/modules/MODULE_MAP.md
```

Owns:

```text
current module list
module grouping
module role classification
legacy-module mapping
```

It does not redefine module-local contracts.

---

# 18. Dependency Rules

## Owner

```text
01-architecture/modules/MODULE_DEPENDENCY.md
```

Owns:

```text
allowed dependency direction
forbidden dependency patterns
cross-module dependency rules
```

---

# 19. Runtime Ownership

## Owner

```text
01-architecture/runtime/
```

Runtime owns execution authority.

---

# 20. RuntimeRevision

## Owner

```text
01-architecture/runtime/
```

Owns:

```text
RuntimeRevisionId
current execution authority
supersession
execution consistency boundary
```

---

# 21. WorkItem

## Owner

```text
01-architecture/runtime/
```

Owns:

```text
WorkItemId
WorkItem lifecycle
dependency readiness
logical executable work
```

Business modules may define semantic work types.

They do not redefine WorkItem lifecycle.

---

# 22. Attempt

## Owner

```text
01-architecture/runtime/
```

Owns:

```text
AttemptId
Attempt lifecycle
execution outcome
provider/config execution identity
```

Every retry creates another Attempt.

---

# 23. Retry

## Owner

```text
01-architecture/runtime/RETRY_POLICY.md
```

Modules may provide:

```text
retryability classification
error semantics
quality information
provider suitability
```

Runtime owns retry execution.

---

# 24. Cancellation

## Owner

```text
01-architecture/runtime/CANCELLATION.md
```

Modules cooperate with cancellation.

They do not commit Runtime cancellation state.

---

# 25. Scheduling

## Owner

Architecture policy:

```text
01-architecture/runtime/SCHEDULER.md
```

Infrastructure mechanism:

```text
03-infrastructure/scheduler/
```

Scheduler does not own business semantics.

---

# 26. Work Queue

## Owner

```text
01-architecture/runtime/WORK_QUEUE.md
```

Owns Runtime queue semantics.

Infrastructure implementation must follow these rules.

---

# 27. Runtime Backpressure

## Owner

```text
01-architecture/runtime/
```

including relevant:

```text
SCHEDULER.md
WORK_QUEUE.md
RESOURCE_LIFECYCLE.md
PERFORMANCE_MODEL.md
```

Business modules must not create competing global backpressure policy.

---

# 28. Runtime Error Model

## Owner

```text
01-architecture/runtime/ERROR_MODEL.md
```

Owns execution-level distinctions such as:

```text
failed
cancelled
timed out
superseded
retry exhausted
```

Module error semantics remain module-owned.

---

# 29. Runtime Observability

## Owner

```text
01-architecture/runtime/RUNTIME_OBSERVABILITY.md
```

Owns Runtime-specific measurement semantics.

Logging/Telemetry transport belongs to Infrastructure.

---

# 30. Cache Policy

## Owner

```text
01-architecture/runtime/CACHE_POLICY.md
```

Owns:

```text
system cache policy
reuse rules at Runtime level
retention/eviction policy where defined
```

Modules own semantic cache compatibility inputs.

---

# 31. Resource Lifecycle

## Owner

```text
01-architecture/runtime/RESOURCE_LIFECYCLE.md
```

Owns Runtime resource lifetime concepts such as:

```text
lease
retention
logical disposal
resource release
```

This must not be confused with semantic Artifact ownership.

---

# 32. Resource Manager

## Owner

```text
03-infrastructure/resource-manager/
```

Owns Infrastructure mechanism for resource management.

It implements architecture policy.

---

# 33. Semantic Artifact Ownership

Each semantic Artifact is owned by the module that defines its meaning.

Runtime does not own semantic Artifact schemas.

---

# 34. Capture Artifact

## Owner

```text
02-modules/capture/
```

Owns:

```text
Capture Artifact semantics
source geometry
capture provenance
accepted capture data
```

---

# 35. RecognitionArtifact

## Owner

```text
02-modules/recognition/
```

Owns:

```text
recognized source text
recognition geometry
confidence
direction/read-order hints as exposed
RecognitionArtifact contract
```

Detailed OCR internals may be specified in:

```text
01-architecture/ocr/
```

but public module boundary remains Recognition-owned.

---

# 36. SourceDocumentArtifact

## Owner

```text
02-modules/text-processing/
```

Owns:

```text
SourceDocument
SourceDocumentArtifact
semantic source-text structure
normalized ordering/grouping
source-document provenance
```

---

# 37. TranslationUnit

## Owner

```text
02-modules/translation/
```

Translation owns:

```text
TranslationUnit
TranslationBatch
context assembly
source-target alignment semantics
```

Text Processing must not redefine TranslationUnit.

---

# 38. TranslationArtifact

## Owner

```text
02-modules/translation/
```

Owns:

```text
semantic translated output
source-target alignment
translation provenance
completeness/warnings
```

---

# 39. PresentationArtifact

## Owner

```text
02-modules/presentation/
```

Owns:

```text
semantic presentation layout
geometry mapping
text fitting
PresentationArtifact
PresentationRevision
```

Native rendering remains UI/platform-owned.

---

# 40. Candidate Artifact

Candidate creation belongs to the producing module execution boundary.

Candidate status is provisional.

Candidate creation does not transfer current authority.

---

# 41. Published Artifact

Semantic publication belongs to the producing module contract together with Runtime authority validation.

Conceptually:

```text
module creates Candidate
    ↓
Runtime/current-authority validation
    ↓
module Artifact publication boundary
```

Runtime does not redefine Artifact semantics.

---

# 42. Artifact Publication Authority

Publication requires both:

```text
semantic validity
+
current execution authority
```

Therefore publication is a shared boundary:

```text
Module
    → semantic validity

Runtime
    → execution authority

Published Artifact
    → module-owned semantic output
```

Neither side may unilaterally redefine the other's authority.

---

# 43. Artifact Storage

Storage may persist Artifacts.

Persistence does not make Storage the Artifact owner.

---

# 44. Capture Module

## Owner

```text
02-modules/capture/
```

Owns:

```text
CaptureSource semantics
capture candidate semantics
Capture Artifact
source observation integration
capture capability/error semantics
```

---

# 45. Recognition Module

## Owner

```text
02-modules/recognition/
```

Owns public Recognition semantics.

Detailed OCR architecture provides specialized internal semantics.

---

# 46. Text Processing Module

## Owner

```text
02-modules/text-processing/
```

Owns:

```text
normalization beyond Recognition/provider cleanup
cross-region semantic reconstruction
source-document construction
semantic grouping
SourceDocumentArtifact
```

---

# 47. Translation Module

## Owner

```text
02-modules/translation/
```

Owns:

```text
TranslationUnit
TranslationBatch
context assembly
glossary application
provider-response semantic normalization
TranslationArtifact
```

Does not own Runtime retry/cancellation.

---

# 48. Presentation Module

## Owner

```text
02-modules/presentation/
```

Owns:

```text
platform-neutral presentation semantics
PresentationArtifact
semantic geometry/layout
text fitting
```

Does not own:

```text
native UI controls
window lifecycle
UI framework rendering
```

---

# 49. Reading Session

## Owner

```text
02-modules/reading-session/
```

Owns:

```text
SessionId
session lifecycle
SessionConfiguration
ReadingContext
ReadingContextRevision
session-only configuration
```

Does not own:

```text
RuntimeRevision
WorkItem
Attempt
processing-stage state
retry execution
```

---

# 50. Preferences

## Owner

```text
02-modules/preferences/
```

Owns:

```text
PreferenceDefinition
Global preferences
Source-scoped preferences
PreferenceRevision
validation
persistent preference semantics
```

Session overrides belong to Reading Session.

---

# 51. Diagnostics

## Owner

```text
02-modules/diagnostics/
```

Owns:

```text
DiagnosticObservation semantics
DiagnosticHealthSnapshot
DiagnosticCapabilities
support-bundle semantics
diagnostic correlation
```

Does not own source module errors.

---

# 52. UI Adapter

## Owner

```text
02-modules/ui-adapter/
```

Owns:

```text
UiIntent adaptation
ViewModel
NavigationModel
DialogModel
NotificationModel
UI-local lifecycle
UI capability adaptation
```

Does not own:

```text
Reading Session
Preferences
Presentation semantics
Runtime retry
```

---

# 53. ViewModel

## Owner

```text
02-modules/ui-adapter/
```

ViewModel is:

```text
immutable
frontend-facing
non-authoritative
disposable
```

---

# 54. Application Orchestration

## Owner

Application/use-case layer.

Owns:

```text
cross-module use-case coordination
```

Examples:

```text
StartReading
StopReading
ChangeReadingSource
RetryCurrentOperation
```

Application orchestration does not replace module or Runtime ownership.

---

# 55. Business Pipeline Orchestration

## Owner

```text
01-architecture/runtime/BUSINESS_PIPELINE_ORCHESTRATION.md
```

Owns:

```text
logical processing requirements
dependency relationships
conditional processing path
BusinessExecutionPlan
```

Does not own:

```text
Attempt lifecycle
thread scheduling
queue implementation
provider execution
```

---

# 56. Detailed OCR Architecture

Detailed OCR concerns live under:

```text
01-architecture/ocr/
```

These documents refine Recognition/image-processing semantics.

They do not create additional top-level module authority.

---

# 57. Canonical OCR Pipeline

## Owner

```text
01-architecture/ocr/PIPELINE.md
```

Owns detailed OCR internal stage relationships.

It does not own Runtime execution authority.

---

# 58. OCR Preprocessing

## Owner

```text
01-architecture/ocr/PREPROCESS.md
```

Owns:

```text
OCR image preparation
format normalization
orientation correction
noise reduction
contrast enhancement
OCR preprocessing metadata
```

---

# 59. OCR Detection

## Owner

```text
01-architecture/ocr/DETECTION.md
```

Owns detailed OCR concepts:

```text
Detection Result
Region
Region geometry
Detection Confidence
Region Type
detection relationships
```

---

# 60. OCR Recognition

## Owner

```text
01-architecture/ocr/RECOGNITION.md
```

Owns internal OCR recognition representation.

Public cross-module output must remain compatible with Recognition module contracts.

---

# 61. Text Direction

## Owner

```text
01-architecture/ocr/TEXT_DIRECTION.md
```

Owns detailed OCR writing-direction semantics.

---

# 62. OCR Layout

## Owner

```text
01-architecture/ocr/LAYOUT.md
```

Owns detailed OCR visual-layout semantics:

```text
Layout Tree
Panel
Container
Block
spatial relationships
```

This does not make Layout a top-level CRAI module.

---

# 63. OCR Reading Order

## Owner

```text
01-architecture/ocr/READING_ORDER.md
```

Owns OCR reading-order semantics.

Cross-region semantic document reconstruction may later be consumed by Text Processing.

---

# 64. OCR Postprocessing

## Owner

```text
01-architecture/ocr/POSTPROCESS.md
```

Owns provider-neutral aggregation/normalization of detailed OCR outputs.

---

# 65. Legacy `OCR Document`

The old architecture used:

```text
OCR Document
```

as a broad cross-module object.

In Runtime v2:

```text
01-architecture/ocr/
    may retain OCR-internal combined representation

02-modules/recognition/
    owns public RecognitionArtifact boundary
```

Therefore downstream architecture should prefer:

```text
RecognitionArtifact
```

rather than introducing new dependencies on legacy `OCR Document`.

---

# 66. OCR Quality

## Owner

```text
01-architecture/ocr/QUALITY.md
```

Owns detailed OCR quality aggregation.

Quality recommendations do not execute Runtime actions.

---

# 67. OCR Provider Contract

## Owner

```text
01-architecture/ocr/PROVIDERS.md
```

Owns OCR-specific provider adaptation semantics.

Provider-native models must not leak beyond adapter boundaries.

---

# 68. Provider Management

Provider Management owns cross-provider capability/availability/configuration semantics where defined.

Semantic modules own:

```text
provider suitability
semantic request/result normalization
domain error meaning
```

Runtime owns Attempt execution.

---

# 69. Secret Management

## Owner

```text
03-infrastructure/secret-management/
```

Owns secure credential storage/access mechanism.

Credentials never become normal module data.

---

# 70. Storage Mechanism

## Owner

Storage/infrastructure architecture.

Owns:

```text
physical persistence
storage transactions
storage durability
migration
storage querying
```

It does not own semantic meaning of stored records.

---

# 71. Logging

## Owner

```text
03-infrastructure/logging/
```

Owns:

```text
log transport
buffering
sink integration
formatting mechanism
```

---

# 72. Telemetry

## Owner

```text
03-infrastructure/telemetry/
```

Owns:

```text
metric transport
trace transport
sampling/export
telemetry backend integration
```

---

# 73. Event Transport

## Owner

Architecture semantics:

```text
01-architecture/core/EVENT_BUS.md
```

Infrastructure implementation:

```text
03-infrastructure/event-bus/
```

---

# 74. Event Meaning

Exact event meaning belongs to the producer module.

Example:

```text
ReadingContextChanged
```

meaning belongs to Reading Session.

Event Bus only distributes the committed fact.

---

# 75. No Generic `RecognitionCompleted` Ownership

Avoid using generic completion events as cross-module execution authority.

Prefer module-defined factual events such as:

```text
RecognitionArtifactPublished
```

where that is the actual module contract.

---

# 76. Error Ownership

Every module owns its own semantic errors.

Examples:

```text
CAP-*
REC-*
TXT-*
TRN-*
PRES-*
SES-*
PREF-*
DIAG-*
UIA-*
```

Runtime owns:

```text
RUN-*
```

Execution-level classification does not erase source error identity.

---

# 77. Error Projection

UI Adapter may display:

```text
TRN-*
```

but the error remains Translation-owned.

Diagnostics may observe:

```text
REC-*
```

but the error remains Recognition-owned.

---

# 78. Cache Semantic Compatibility

The producing domain/module may define:

```text
which semantic inputs affect compatibility
```

Example:

```text
Translation
    defines glossary/context inputs
    affecting Translation cache validity
```

---

# 79. Cache Physical Lifecycle

Runtime/Infrastructure owns:

```text
eviction
budget
storage
global retention policy
```

according to the current cache architecture.

---

# 80. Performance

## Owner

System-wide performance model:

```text
01-architecture/runtime/PERFORMANCE_MODEL.md
```

Modules may define local performance constraints.

They do not redefine global CPU/GPU/memory/scheduling policy.

---

# 81. Privacy Ownership

Privacy is cross-cutting.

Semantic owners define:

```text
what data is sensitive
what may leave the module/provider boundary
```

Infrastructure implements:

```text
credential protection
transport/storage protection
logging restrictions
```

Core Architecture defines architecture-wide privacy invariants.

---

# 82. Translation Boundary

Recognition/OCR output remains source-language semantic data.

Recognition/OCR must not:

```text
translate
apply translation glossary
rewrite source meaning into target language
create TranslationUnit
```

---

# 83. Text Processing Boundary

Text Processing ends at:

```text
SourceDocumentArtifact
```

It does not own:

```text
TranslationUnit
TranslationBatch
Translation context assembly
```

---

# 84. Translation Boundary

Translation consumes:

```text
SourceDocumentArtifact
```

and owns:

```text
TranslationUnit
TranslationBatch
TranslationArtifact
```

---

# 85. Presentation Boundary

Presentation consumes:

```text
TranslationArtifact
```

and owns:

```text
PresentationArtifact
```

It does not own native rendering implementation.

---

# 86. UI Boundary

UI Adapter consumes Application/module projections.

It owns:

```text
ViewModel
```

not semantic Presentation data.

---

# 87. Reading Session vs Runtime

Reading Session owns:

```text
ReadingContextRevision
```

Runtime owns:

```text
RuntimeRevisionId
```

These must never be merged.

---

# 88. RuntimeRevision vs Artifact

RuntimeRevision describes:

```text
execution authority
```

Artifact describes:

```text
semantic output
```

An Artifact may record Runtime provenance.

It does not become Runtime state.

---

# 89. WorkItem vs Module Operation

A module may expose an operation such as:

```text
Recognize
Translate
BuildPresentation
```

Runtime may represent execution of that operation through a WorkItem.

The module operation is not itself the WorkItem contract.

---

# 90. Attempt vs Provider Request

Attempt is Runtime-owned.

A provider request is module/provider-adapter-owned implementation detail.

One Attempt may internally cause one or more provider interactions according to contract.

Do not equate provider-native request ID with AttemptId.

---

# 91. Candidate vs Published Ownership

Candidate:

```text
module-produced provisional result
```

Published:

```text
module-owned semantic Artifact
accepted under current Runtime authority
```

This distinction is architecture-wide.

---

# 92. Presentation vs Native Rendering

Presentation owns:

```text
what should be presented
```

UI Adapter/platform owns:

```text
how it is rendered/interacted with
```

---

# 93. Theme Ownership

Persistent theme preference:

```text
Preferences
```

Resolved/applied UI appearance:

```text
UI Adapter
```

Do not assign persistent Theme authority to Presentation/UI Adapter.

---

# 94. Localization Ownership

Persistent locale preference:

```text
Preferences
```

Localization resources/application concern:

```text
Application/UI localization layer
UI Adapter
```

depending on implementation boundary.

Domain modules should use stable message keys.

---

# 95. Diagnostics vs Logging

Diagnostics owns:

```text
diagnostic meaning
health aggregation
support semantics
```

Logging infrastructure owns:

```text
log transport
```

---

# 96. Diagnostics vs Telemetry

Diagnostics may define what should be observed.

Telemetry owns:

```text
metrics/traces transport and export
```

---

# 97. Event Bus vs Telemetry

Event Bus transports:

```text
committed architecture facts
```

Telemetry transports:

```text
operational measurements
```

They are not interchangeable.

---

# 98. UI Events vs Business Events

UI-local events such as:

```text
ViewOpened
DialogResponded
NotificationShown
```

remain UI-local by default.

They do not automatically become Event Bus facts.

---

# 99. Ownership Summary — Core Modules

| Concept                          | Owner           |
| -------------------------------- | --------------- |
| CaptureSource / Capture Artifact | Capture         |
| RecognitionArtifact              | Recognition     |
| SourceDocumentArtifact           | Text Processing |
| TranslationUnit                  | Translation     |
| TranslationBatch                 | Translation     |
| TranslationArtifact              | Translation     |
| PresentationArtifact             | Presentation    |
| ReadingContext                   | Reading Session |
| ReadingContextRevision           | Reading Session |
| PreferenceDefinition             | Preferences     |
| PreferenceRevision               | Preferences     |
| DiagnosticHealthSnapshot         | Diagnostics     |
| ViewModel                        | UI Adapter      |

---

# 100. Ownership Summary — Runtime

| Concept                    | Owner   |
| -------------------------- | ------- |
| RuntimeRevisionId          | Runtime |
| WorkItem                   | Runtime |
| Attempt                    | Runtime |
| Retry execution            | Runtime |
| Cancellation execution     | Runtime |
| Work scheduling            | Runtime |
| Work Queue semantics       | Runtime |
| Supersession               | Runtime |
| Runtime deadlines/timeouts | Runtime |
| Runtime resource lifecycle | Runtime |

---

# 101. Ownership Summary — Infrastructure

| Concept                       | Owner                    |
| ----------------------------- | ------------------------ |
| Event Bus implementation      | Infrastructure Event Bus |
| Scheduler implementation      | Infrastructure Scheduler |
| Logging transport             | Logging                  |
| Metrics/tracing transport     | Telemetry                |
| Secret storage/access         | Secret Management        |
| Resource management mechanism | Resource Manager         |
| Physical persistence          | Storage                  |
| Configuration mechanism       | Configuration            |

---

# 102. Ownership Summary — OCR Detail

| Concept                     | Owner                   |
| --------------------------- | ----------------------- |
| OCR Pipeline semantics      | `ocr/PIPELINE.md`       |
| OCR Preprocessing           | `ocr/PREPROCESS.md`     |
| Detection Result            | `ocr/DETECTION.md`      |
| Region / Region Type        | `ocr/DETECTION.md`      |
| Recognition internals       | `ocr/RECOGNITION.md`    |
| Text Direction              | `ocr/TEXT_DIRECTION.md` |
| Layout Tree                 | `ocr/LAYOUT.md`         |
| OCR Postprocessing          | `ocr/POSTPROCESS.md`    |
| OCR Quality                 | `ocr/QUALITY.md`        |
| Reading Order               | `ocr/READING_ORDER.md`  |
| OCR Provider adaptation     | `ocr/PROVIDERS.md`      |
| Public Recognition boundary | Recognition module      |

---

# 103. Consumer Rule

A consumer may define:

```text
how it uses an owned concept
```

It must not redefine:

```text
what that concept means
```

---

# 104. Local Constraint Rule

Consumers may define local constraints.

Example:

```text
Translation accepts only
SourceDocumentArtifact schema >= X
```

This is a Translation input constraint.

Translation must not redefine SourceDocument semantics.

---

# 105. Refactoring Rule — Keep

Keep a definition when the current document is the owner.

---

# 106. Refactoring Rule — Reference

If another document owns the concept:

```text
reference the owner
```

and describe local usage only.

---

# 107. Refactoring Rule — Remove Duplicate Authority

Do not copy another owner's:

```text
state machine
error taxonomy
retry policy
cancellation lifecycle
Artifact schema
event semantics
cache lifecycle
telemetry contract
```

---

# 108. Refactoring Rule — Historical Sections

Historical sections may preserve old terminology if clearly marked as history.

Do not rewrite history merely to make terminology current.

Current-authority sections must use current vocabulary.

---

# 109. Legacy Concepts to Detect

During project-wide review, search for:

```text
pipelineId as universal authority
taskId as universal execution identity
contentRevision as universal revision
ProcessingAttemptId
module-owned retry
module-owned cancellation
direct stage-completion chaining
EffectivePreferencesChanged
generic DiagnosticsUpdated
Event Bus telemetry events
UI Adapter retry pipeline
Presentation native rendering ownership
```

---

# 110. Deprecated Ownership — Runtime Artifact

Deprecated interpretation:

```text
Runtime owns Artifact semantics
```

Correct:

```text
Module owns Artifact semantics

Runtime owns execution authority
and resource/execution lifecycle
```

---

# 111. Deprecated Ownership — OCR Document

Deprecated cross-module assumption:

```text
OCR Document
    → universal downstream Artifact
```

Preferred:

```text
OCR internals
    ↓
RecognitionArtifact
    ↓
Text Processing
```

---

# 112. Deprecated Ownership — Reader

No monolithic Reader owner exists.

Responsibilities are divided among:

```text
Reading Session
Application
Presentation
UI Adapter
```

---

# 113. Deprecated Ownership — Settings

Persistent Settings semantics now belong to:

```text
Preferences
```

UI Settings screen belongs to UI Adapter.

Session-only override belongs to Reading Session.

---

# 114. Deprecated Ownership — Pipeline Cancellation

No processing module owns generic pipeline cancellation.

Runtime owns execution cancellation.

---

# 115. Deprecated Ownership — Stage Retry

No Capture/Recognition/Text Processing/Translation/Presentation module independently owns Runtime retry lifecycle.

---

# 116. Deprecated Ownership — Presentation Font

Presentation may own semantic style/fitting constraints.

Persistent font/theme user preference belongs to Preferences.

Native application of fonts/theme belongs to UI Adapter/platform.

Ownership depends on semantic level.

---

# 117. Open Ownership Questions

The following remain genuinely open:

```text
Should Knowledge become a dedicated module?

Where should persistent Translation Memory semantic ownership live?

What is the permanent user correction/annotation domain?

Should Reading History become a dedicated domain module?

Should browser/source integration gain a dedicated Source Integration module?
```

These should not redefine existing ownership until explicitly resolved.

---

# 118. Ownership Change Rule

Changing ownership of a concept is an architecture-level breaking change.

Required steps:

```text
identify old owner
identify new owner
document reason
update MODULE_MAP
update MODULE_DEPENDENCY
update producer/consumer contracts
update events/errors/state references
update PROJECT_STATUS
```

---

# 119. No Silent Ownership Migration

Do not gradually let a consumer become an owner through duplicated definitions.

Ownership changes must be explicit.

---

# 120. Document Authority Order

For ownership questions, consult:

```text
OWNERSHIP_MAP.md
    ↓
MODULE_MAP.md
    ↓
module MODULE.md / CONTRACT.md
    ↓
specialized architecture owner document
```

If inconsistency exists, update the stale current-authority document.

---

# 121. Architecture Invariants

1. Every architectural concept has one semantic owner.

2. A consumer may reference but not redefine owner semantics.

3. Runtime owns execution authority.

4. Runtime does not own semantic Artifact meaning.

5. Modules own semantic Artifacts.

6. Candidate does not equal Published.

7. Artifact publication requires semantic validity and current Runtime authority.

8. Reading Session owns ReadingContextRevision.

9. Runtime owns RuntimeRevisionId.

10. WorkItem and Attempt are Runtime-owned.

11. Retry execution is Runtime-owned.

12. Cancellation execution is Runtime-owned.

13. Scheduler does not own business semantics.

14. Storage does not own semantic meaning of persisted data.

15. Cache is not current authority.

16. Event Bus transports committed facts only.

17. Event semantic meaning belongs to producer owner.

18. Diagnostics observes errors without taking ownership.

19. UI Adapter projects state without taking ownership.

20. Presentation owns semantic presentation, not native UI rendering.

21. Preferences owns persistent preferences.

22. Session-only configuration belongs to Reading Session.

23. TranslationUnit belongs to Translation.

24. SourceDocumentArtifact belongs to Text Processing.

25. RecognitionArtifact is the public Recognition boundary.

26. Detailed OCR documents refine Recognition internals without creating competing top-level module ownership.

27. Provider-native models never cross adapter boundaries.

28. Logging and Telemetry remain Infrastructure concerns.

29. Historical terminology may remain historical; current-authority terminology must be current.

30. Ownership migrations are explicit architecture changes.

---

# 122. Current Refactoring Priority

The earlier OCR-specific ownership refactor is complete.

The current priority is:

```text
project-wide ownership consistency review
```

Recommended order:

```text
MODULE_MAP.md
    ↓
OWNERSHIP_MAP.md
    ↓
MODULE_DEPENDENCY.md
    ↓
architecture flows
    ↓
text / translate architecture
    ↓
remaining legacy references
```

---

# 123. Related Documents

```text
doc/01-architecture/core/
├── CAPABILITY_MAP.md
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

doc/01-architecture/ocr/

doc/02-modules/
├── capture/
├── recognition/
├── text-processing/
├── translation/
├── presentation/
├── reading-session/
├── preferences/
├── diagnostics/
└── ui-adapter/

doc/03-infrastructure/
```

---

# 124. Completion Criteria

This Ownership Map is synchronized when:

* it is project-wide rather than OCR-centric;
* current `02-modules/` ownership is explicit;
* Runtime v2 identities are explicit;
* semantic Artifact ownership is separated from Runtime resource lifecycle;
* Candidate/Published authority is explicit;
* ReadingContextRevision and RuntimeRevisionId are separated;
* TranslationUnit belongs to Translation;
* RecognitionArtifact replaces legacy OCR Document as public module boundary;
* Presentation and UI Adapter ownership are separated;
* Preferences and Reading Session configuration ownership are separated;
* Diagnostics and observability transport ownership are separated;
* Event Bus paths/ownership match `core/`;
* OCR-specific ownership remains available as detailed subordinate architecture;
* old OCR refactoring sequence is no longer the current next step.

---

# 125. Summary

The ownership model is:

```text
Core Architecture
    owns cross-project rules

Business Modules
    own semantic meaning

Runtime
    owns execution

Infrastructure
    provides mechanisms

Provider/Platform Adapters
    isolate external implementations
```

For processing data:

```text
Capture
    owns Capture Artifact

Recognition
    owns RecognitionArtifact

Text Processing
    owns SourceDocumentArtifact

Translation
    owns TranslationUnit
    owns TranslationArtifact

Presentation
    owns PresentationArtifact

UI Adapter
    owns ViewModel
```

For execution:

```text
Reading Session
    owns ReadingContextRevision

Runtime
    owns RuntimeRevision
    owns WorkItem
    owns Attempt
    owns retry/cancellation execution
```

The central invariant is:

```text
The component that stores,
executes,
transports,
observes,
or displays a concept

does not automatically
become its semantic owner.
```
