# CRAI Module Catalog

> **Project:** CRAI
> **Document:** Module Catalog
> **Path:** `.meta/MODULES.md`
> **Version:** 1.0
> **Status:** Active
> **Last Updated:** 2026-08-14
> **Source of Truth:** This file lists the authoritative module catalog. Detailed contracts belong to their module documents.

---

## 1. Purpose

This document is the authoritative catalog of all CRAI modules.

It records:

- module identity and layer
- primary responsibility
- design maturity status
- data ownership
- pointer to detailed documentation

This file does **not** duplicate detailed contracts, states, events, or error definitions.
Those belong to the module-specific documents.

---

## 2. Module Layers

CRAI organizes modules into three layers:

```text
Business Modules
    → own business meaning, domain semantics, and user-facing behavior

Infrastructure Modules
    → own technical capabilities shared across business modules and runtime

Storage
    → owns persistence mechanisms (treated as a capability, not a business owner)
```

Runtime is an execution framework, not a module.

---

## 3. Business Modules

| Module | Primary Responsibility | Status | Data Ownership | Documents |
|---|---|---|---|---|
| Reading Session | Session identity, state, and lifecycle | Accepted | ReadingSession, SessionConfig, SessionOverride | `02-modules/reading-session/` |
| Capture | Screen region capture and frame acquisition | Accepted | CapturedFrame, CaptureRegion, FrameMetadata | `02-modules/capture/` |
| Recognition | Image-to-structured-source (OCR boundary) | Accepted | RecognitionRequest, PublishedRecognitionArtifact | `02-modules/recognition/` |
| Text Processing | OCR output normalization and source document preparation | Accepted | SourceDocument, SourceDocumentArtifact, TranslationUnit | `02-modules/text-processing/` |
| Translation | Translation orchestration and provider coordination | Accepted | TranslationRequest, TranslationResult, TranslationCache | `02-modules/translation/` |
| Presentation | Semantic layout and rendered output | Accepted | PresentationModel, LayoutConfig, RenderOutput | `02-modules/presentation/` |
| Storage | Persistence capability and schema evolution | Accepted | (implements persistence for owning modules) | `02-modules/storage/` |
| Preferences | User preferences and reading configuration | Accepted | DefaultPreferences, GlobalPreferences, SourcePreferences | `02-modules/preferences/` |
| Diagnostics | Observability and runtime health reporting | Accepted | DiagnosticReport, HealthSnapshot | `02-modules/diagnostics/` |
| Provider Management | Provider registration, health, and lifecycle | Accepted | ProviderRegistration, ProviderHealth, ProviderConfig | `02-modules/provider-management/` |
| UI Adapter | Native UI rendering adapter and view model projection | Accepted | ViewModel, UICommand, UIEvent | `02-modules/ui-adapter/` |

---

## 4. Infrastructure Modules

| Module | Primary Responsibility | Status | Documents |
|---|---|---|---|
| Configuration | Application configuration loading and validation | Accepted | `03-infrastructure/configuration/` |
| Event Bus | Asynchronous inter-module event transport | Accepted | `03-infrastructure/event-bus/` |
| Logging | Structured logging | Accepted | `03-infrastructure/logging/` |
| Telemetry | Metrics, tracing, and observability | Accepted | `03-infrastructure/telemetry/` |
| Scheduler | Background job scheduling and execution supervision | Accepted | `03-infrastructure/scheduler/` |
| Resource Manager | Shared resource lifecycle, lease, pool, and health | Accepted | `03-infrastructure/resource-manager/` |
| Secret Management | Secure credential storage and access | Accepted | `03-infrastructure/secret-management/` |

---

## 5. Data Ownership Rules

Ownership rules follow `MODULE_ROLE.md` Section 12:

- Only the owning module defines authoritative data representation.
- Other modules access owned data through contracts, queries, or immutable transfer models.
- No module may directly modify another module's internal state.
- Shared mutable state is forbidden.

Ownership is assigned in this catalog and enforced by the dependency rules in `01-architecture/modules/OWNERSHIP_MAP.md`.

---

## 6. Module Maturity States

| Status | Meaning |
|---|---|
| Proposed | Under consideration; contracts may change |
| Accepted | Boundary and responsibility reviewed; implementation may begin |
| Implemented | Exists in source code with basic tests |
| Stable | Contract is reliable; breaking changes require explicit review |
| Deprecated | Should not be used for new work; migration path documented |
| Removed | Removed from active architecture |

All current CRAI modules are in **Accepted** status as of 2026-08-14.

Implementation has not started.

---

## 7. Module Communication Rules

Modules communicate through:

- Commands (state-changing requests)
- Queries (data retrieval, no state change)
- Events (past-tense announcements of completed facts)
- Contracts (defined interfaces and data structures)

Direct implementation dependencies between modules are forbidden.

Circular dependencies are forbidden.

---

## 8. Capabilities vs. Modules

Not every capability is a standalone module.

Capabilities such as the following remain internal to their owning module:

```text
Image preprocessing         → internal to Recognition
Text detection              → internal to Recognition
Reading order resolution    → internal to Recognition
Text normalization          → internal to Text Processing
Context preparation         → internal to Translation
Overlay rendering           → internal to Presentation
Cache management            → internal to Translation / Text Processing
```

Creating a module requires a distinct responsibility boundary, not merely a named capability.

---

## 9. Maintenance

Update this catalog when:

- a new module is proposed or accepted;
- a module is deprecated or removed;
- module ownership assignments change;
- module maturity status changes.

Do not duplicate detailed contracts or state definitions here.
