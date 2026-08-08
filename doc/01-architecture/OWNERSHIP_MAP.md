# Architecture Ownership Map

> **Status:** Draft
> **Scope:** CRAI Architecture
> **Purpose:** Define authoritative ownership for cross-document architectural concepts and prevent duplicated definitions.

---

# 1. Purpose

CRAI có nhiều tài liệu mô tả các phần khác nhau của cùng một hệ thống.

Khi một khái niệm xuất hiện trong nhiều tài liệu, cần phân biệt:

```text
Owner
```

và

```text
Consumer / Reference
```

`Owner` là nơi duy nhất được phép định nghĩa semantics đầy đủ của khái niệm.

Các tài liệu khác:

* có thể sử dụng
* có thể tham chiếu
* có thể mô tả vai trò của khái niệm trong flow của mình

nhưng không nên định nghĩa lại semantics, lifecycle hoặc contract của khái niệm đó.

Mục tiêu của tài liệu này là:

* giảm trùng lặp giữa Architecture, Runtime, Module và Infrastructure
* giữ một source of truth cho mỗi concept
* tránh cùng một state, event hoặc model có nhiều định nghĩa khác nhau
* làm chuẩn cho việc refactor tài liệu cũ
* giúp AI và developer xác định đúng tài liệu cần sửa

---

# 2. Core Ownership Rule

Nguyên tắc chung:

```text
One architectural concept
        ↓
One authoritative owner
        ↓
Many consumers may reference it
```

Không nên tồn tại:

```text
Document A
defines Concept X

Document B
also defines Concept X differently
```

Nếu một concept cần xuất hiện ở nhiều nơi:

```text
Owner
    ↓
defines semantics

Consumer
    ↓
references owner
    ↓
describes only local usage
```

---

# 3. Architecture Layer Ownership

CRAI hiện phân chia trách nhiệm ở mức cao như sau.

| Layer                | Owns                                                                   |
| -------------------- | ---------------------------------------------------------------------- |
| Product / Capability | System capabilities and product intent                                 |
| Business Modules     | Business semantics and module-owned state                              |
| Runtime Architecture | Execution authority and runtime coordination                           |
| OCR Architecture     | Image-to-structured-source processing semantics                        |
| Infrastructure       | Technical services and implementation-neutral infrastructure contracts |
| Storage              | Persistence mechanism                                                  |
| Presentation         | User-visible presentation semantics                                    |
| Provider Integration | External provider adaptation                                           |

Business modules own semantic meaning.

Runtime owns execution authority.

Infrastructure provides technical capabilities.

Architecture documents define processing semantics but should not re-own Runtime or Infrastructure behavior.

---

# 4. Runtime Ownership

Runtime owns execution-level concepts.

## Runtime Control

**Owner:**

```text
01-architecture/runtime/
```

Runtime Control owns:

* execution authority
* current revision authority
* WorkItem lifecycle
* Attempt lifecycle
* terminal outcome acceptance
* downstream work creation
* cancellation coordination
* retry coordination
* Scheduler interaction

OCR stages do not independently decide what work runs next.

---

## WorkItem

**Owner:**

```text
01-architecture/runtime/
```

OCR documents may describe:

```text
OCR work
Detection work
Recognition work
```

but must not redefine WorkItem lifecycle.

---

## Attempt

**Owner:**

```text
01-architecture/runtime/
```

Every retry creates a new Attempt according to Runtime rules.

OCR Provider, Detection, Recognition and Reading Order do not own retry attempts.

---

## Cancellation

**Owner:**

```text
01-architecture/runtime/CANCELLATION.md
```

OCR components must:

* observe cancellation
* stop work when possible
* avoid publishing obsolete results

but do not own cancellation authority.

---

## Retry

**Owner:**

```text
01-architecture/runtime/RETRY_POLICY.md
```

OCR components may provide:

* error information
* quality information
* retry recommendation
* fallback capability information

but must not independently schedule retry.

---

## Scheduling

**Owner:**

```text
01-architecture/runtime/SCHEDULER.md
```

OCR architecture may describe parallelizable work but does not define admission or scheduling policy.

---

## Runtime State

**Owner:**

```text
01-architecture/runtime/
```

States such as:

```text
Queued
Running
Cancelled
Failed
Completed
```

when describing execution jobs belong to Runtime unless a document explicitly owns a semantic state independent of execution.

OCR documents should avoid redefining general job state machines.

---

# 5. Resource Ownership

## Artifact Lifecycle

**Owner:**

```text
01-architecture/runtime/RESOURCE_LIFECYCLE.md
```

Runtime owns:

* registration
* ownership transfer
* lease
* retention
* logical disposal
* physical disposal

OCR documents may produce artifacts but do not redefine artifact lifecycle.

---

## Resource Manager

**Owner:**

```text
03-infrastructure/resource-manager/
```

for the Infrastructure implementation contract.

The underlying system-wide resource lifecycle rules remain defined by Runtime Architecture.

Resource Manager should implement those rules rather than redefine them.

---

# 6. OCR Pipeline Ownership

## Canonical OCR Pipeline

**Owner:**

```text
01-architecture/ocr/PIPELINE.md
```

Owns:

* OCR stage ordering
* stage boundaries
* stage input/output relationships
* end-to-end OCR processing flow
* canonical pipeline result boundary

It should not own detailed semantics already owned by specialized OCR documents.

---

# 7. Image Preprocessing Ownership

## Image Preprocessing

**Owner:**

```text
01-architecture/ocr/PREPROCESS.md
```

Owns:

* image validation for OCR preprocessing
* format normalization
* orientation correction
* resolution normalization
* noise reduction
* contrast enhancement
* brightness/color normalization
* preprocessing ROI
* preprocessing metadata

Other OCR documents may consume the processed image but must not redefine preprocessing behavior.

---

# 8. Detection Ownership

## Text Detection

**Owner:**

```text
01-architecture/ocr/DETECTION.md
```

Detection answers:

```text
Where is the text?
```

Owns:

* Detection Result
* Region
* Region identity
* Region geometry
* Bounding Box
* Polygon
* optional Mask
* Detection Confidence
* Classification Confidence
* Region Type
* Region hierarchy
* Region merge/split rules
* Region validation
* detection-specific spatial hints

Detection does not own:

* recognized text
* Reading Order
* Runtime state
* Runtime retry
* Runtime cache policy
* Event Bus semantics

---

## Region Model

**Owner:**

```text
01-architecture/ocr/DETECTION.md
```

Consumers include:

* Recognition
* Layout
* Text Direction
* Postprocessing
* Reading Order
* Presentation

Consumers must reference the Region identity and geometry defined by Detection instead of inventing incompatible Region models.

---

## Region Type

**Owner:**

```text
01-architecture/ocr/DETECTION.md
```

Examples currently include:

* Speech Bubble
* Narration Box
* SFX
* Background Text
* UI Text
* Watermark
* Advertisement
* Unknown Region

Reading Order and Presentation may use Region Type but do not redefine it.

---

# 9. Recognition Ownership

## Text Recognition

**Owner:**

```text
01-architecture/ocr/RECOGNITION.md
```

Recognition answers:

```text
What is the text?
```

Owns:

* Recognition Result
* recognized source text
* Recognition Document structure within a Region
* Character
* Word
* Line
* Paragraph
* recognition Language / Script metadata
* Recognition Confidence

Recognition does not own:

* page reading order
* translation
* layout structure
* Runtime retry
* Runtime scheduling

---

## Character Model

**Owner:**

```text
01-architecture/ocr/RECOGNITION.md
```

---

## Word Model

**Owner:**

```text
01-architecture/ocr/RECOGNITION.md
```

Word is optional or language-dependent where necessary.

---

## Line Model

**Owner:**

```text
01-architecture/ocr/RECOGNITION.md
```

---

## Paragraph Model

**Owner:**

```text
01-architecture/ocr/RECOGNITION.md
```

Recognition owns Paragraph construction only within the Recognition boundary defined by its current contract.

Cross-region semantic reconstruction belongs to later processing.

---

## Recognition Confidence

**Owner:**

```text
01-architecture/ocr/RECOGNITION.md
```

Quality Assessment may aggregate Recognition Confidence but does not redefine its semantics.

---

# 10. Text Direction Ownership

## Text Direction

**Owner:**

```text
01-architecture/ocr/TEXT_DIRECTION.md
```

Answers:

```text
How is the text written?
```

Owns:

* Writing Mode
* Line Direction
* Paragraph Direction
* Character Flow
* Rotation metadata
* Direction Confidence

It does not own page-level Reading Order.

---

# 11. Layout Ownership

## Layout Analysis

**Owner:**

```text
01-architecture/ocr/LAYOUT.md
```

Answers:

```text
How are the visual regions organized?
```

Owns:

* Layout Result
* Layout Tree
* Page layout structure
* Panel
* Container
* Block
* spatial relationships
* Relationship Graph
* Region grouping for layout purposes

Layout does not own final reading sequence.

---

## Panel

**Owner:**

```text
01-architecture/ocr/LAYOUT.md
```

---

## Container

**Owner:**

```text
01-architecture/ocr/LAYOUT.md
```

---

## Block

**Owner:**

```text
01-architecture/ocr/LAYOUT.md
```

---

## Layout Tree

**Owner:**

```text
01-architecture/ocr/LAYOUT.md
```

Consumers:

* Reading Order
* Postprocessing
* Presentation where appropriate

---

## Spatial Relationship Graph

**Owner:**

```text
01-architecture/ocr/LAYOUT.md
```

Reading Order may use these relationships as evidence but must not redefine their spatial semantics.

---

# 12. OCR Postprocessing Ownership

## OCR Postprocessing

**Owner:**

```text
01-architecture/ocr/POSTPROCESS.md
```

Owns:

* validation of combined OCR outputs
* provider-neutral normalization
* result merging
* consistency checking
* metadata completion
* OCR Document assembly

Postprocessing does not:

* change recognized meaning
* change Detection geometry
* change Layout decisions
* change Text Direction decisions

---

# 13. OCR Document Ownership

## OCR Document

**Current Owner:**

```text
01-architecture/ocr/POSTPROCESS.md
```

OCR Document is the canonical combined representation produced after:

```text
Detection Result
+
Recognition Result
+
Layout Result
+
Direction Result
```

It contains the normalized OCR state required by downstream processing.

Consumers may include:

* Quality Assessment
* Reading Order
* Text Processing
* Translation boundary
* Presentation
* Storage / diagnostics where permitted

### Proposed ownership rule

Until a dedicated shared-artifact specification is introduced, `POSTPROCESS.md` remains the authoritative owner of `OCR Document`.

Other documents should reference it instead of defining another OCR Document structure.

---

# 14. Quality Ownership

## OCR Quality Assessment

**Owner:**

```text
01-architecture/ocr/QUALITY.md
```

Quality answers:

```text
How trustworthy is the OCR Document?
```

Owns:

* Quality Report
* Quality Score
* Quality Grade
* Quality Issues
* Confidence aggregation
* quality classification
* recommendation generation

Quality does not own Runtime decisions.

---

## Quality Report

**Owner:**

```text
01-architecture/ocr/QUALITY.md
```

Runtime may consume the report and decide whether to:

* continue
* retry
* fallback
* request user review
* stop processing

The recommendation itself does not execute those actions.

---

## Confidence vs Quality

Confidence remains owned by the component that produces it.

Examples:

```text
Detection Confidence
    → DETECTION.md

Recognition Confidence
    → RECOGNITION.md

Direction Confidence
    → TEXT_DIRECTION.md

Reading Confidence
    → READING_ORDER.md
```

`QUALITY.md` aggregates those values into quality evaluation.

It must not redefine their original meaning.

---

# 15. Reading Order Ownership

## Reading Order

**Owner:**

```text
01-architecture/ocr/READING_ORDER.md
```

Answers:

```text
In what order should the entities be read?
```

Owns:

* Reading Order Graph
* precedence relationships
* Reading Sequence
* local reading sequence
* global reading sequence
* Main Sequence
* Auxiliary Sequence
* Reading Confidence
* ambiguity of ordering
* reading strategy contract
* sequence validation

Reading Order does not modify:

* recognized text
* Detection geometry
* Layout Tree
* Text Direction

---

## Reading Order Graph

**Owner:**

```text
01-architecture/ocr/READING_ORDER.md
```

---

## Main Reading Sequence

**Owner:**

```text
01-architecture/ocr/READING_ORDER.md
```

---

## Auxiliary Sequence

**Owner:**

```text
01-architecture/ocr/READING_ORDER.md
```

---

## Reading Strategies

**Current Owner:**

```text
01-architecture/ocr/READING_ORDER.md
```

The current document defines strategies for:

* LTR
* RTL
* vertical layouts
* mixed layouts
* manga
* webtoon
* documents
* hybrid strategies

### Proposed refactor rule

If Reading Order documentation is split in the future, algorithm/strategy details may move into a dedicated document.

Until that happens, `READING_ORDER.md` remains authoritative.

---

# 16. Provider Ownership

## OCR Provider Abstraction

**Owner:**

```text
01-architecture/ocr/PROVIDERS.md
```

Owns:

* OCR Provider Contract
* Provider Adapter boundary
* OCR-specific capability model
* request/response normalization
* provider-specific error mapping
* provider-specific health reporting interface

Provider SDK/API semantics must not leak across the adapter boundary.

---

## OCR Provider Adapter

**Owner:**

```text
01-architecture/ocr/PROVIDERS.md
```

Adapter responsibilities:

```text
CRAI Request
    ↓
Provider-native Request

Provider-native Response
    ↓
CRAI Contract
```

Adapter does not own business logic.

---

## Provider Selection

The OCR provider document currently describes provider selection inputs.

However, **execution authority remains outside the Provider Adapter**.

Provider information may support decisions using:

* capability
* health
* language support
* privacy constraints
* profile requirements

Actual retry/fallback execution remains governed by Runtime ownership rules.

---

# 17. Event Ownership

## Event Bus Semantics

**Owner:**

```text
01-architecture/EVENT_BUS.md
03-infrastructure/event-bus/
```

depending on whether the concern is architecture semantics or infrastructure implementation contract.

OCR documents may list OCR-specific facts that can be emitted.

They must not redefine:

* event delivery model
* envelope
* ordering guarantees
* subscriber behavior
* retry semantics of Event Bus

---

## OCR-specific Event Meaning

Semantic meaning of a domain-specific event belongs to the architecture/module that owns the action.

Example:

```text
RecognitionCompleted
```

Recognition defines what "Recognition completed" means.

Event Bus defines how that fact is transported.

---

# 18. Error Ownership

## Runtime Error Model

**Owner:**

```text
01-architecture/runtime/ERROR_MODEL.md
```

Owns:

* terminal outcome distinction
* runtime error normalization
* retry classification
* runtime severity
* cancellation vs failure vs stale

---

## OCR-specific Error Meaning

OCR architecture may define stage-specific error categories.

Examples:

```text
InvalidRegion
UnsupportedWritingMode
GraphCycleUnresolved
RecognitionResultInvalid
```

but they must map into the common Runtime error model when crossing Runtime boundaries.

Provider-native errors must never become public OCR errors directly.

---

# 19. Cache Ownership

## Runtime Cache Policy

**Owner:**

```text
01-architecture/runtime/CACHE_POLICY.md
```

OCR documents may define:

* semantic inputs that affect cache validity
* which OCR result is compatible with another input
* stage-specific semantic invalidation conditions

OCR documents should not own:

* eviction scheduling
* memory budget
* physical cache storage
* global cache lifecycle

---

# 20. Observability Ownership

## Runtime Observability

**Owner:**

```text
01-architecture/runtime/RUNTIME_OBSERVABILITY.md
```

Infrastructure implementation:

```text
03-infrastructure/logging/
03-infrastructure/telemetry/
```

OCR documents may define:

* OCR-specific measurements
* useful diagnostic fields
* quality metrics
* provider metrics

but must not redefine telemetry lifecycle or transport.

---

# 21. Performance Ownership

## System-wide Performance

**Owner:**

```text
01-architecture/runtime/PERFORMANCE_MODEL.md
```

OCR documents may specify algorithmic constraints such as:

```text
avoid unbounded pair comparison
support incremental processing
preserve deterministic output
```

but global:

* concurrency limits
* CPU/GPU budgets
* memory budgets
* queue behavior
* scheduling priority

belong to Runtime / Infrastructure.

---

# 22. Translation Boundary

OCR Architecture ends with structured source data.

OCR must never:

* translate source text
* rewrite source meaning
* apply glossary translation
* produce translated render layout

The output consumed by the Text Domain must remain source-language information.

---

# 23. Text Processing Ownership

Text Processing owns semantic preparation after OCR.

Examples include:

* source-text normalization beyond OCR/provider cleanup
* source-document construction where owned by the Text Processing module
* segmentation/reconstruction for translation
* semantic grouping beyond OCR visual structure

OCR normalization must not silently perform language-domain rewriting.

---

# 24. Presentation Ownership

Presentation owns:

* font
* render layout
* side panel
* overlay
* translated text rendering
* visual presentation state

OCR only preserves geometry and structural references required by Presentation.

---

# 25. Ownership Summary

| Concept                    | Authoritative Owner                     |
| -------------------------- | --------------------------------------- |
| Canonical OCR Pipeline     | `ocr/PIPELINE.md`                       |
| Image Preprocessing        | `ocr/PREPROCESS.md`                     |
| Detection Result           | `ocr/DETECTION.md`                      |
| Region                     | `ocr/DETECTION.md`                      |
| Region Type                | `ocr/DETECTION.md`                      |
| Detection Geometry         | `ocr/DETECTION.md`                      |
| Recognition Result         | `ocr/RECOGNITION.md`                    |
| Character                  | `ocr/RECOGNITION.md`                    |
| Word                       | `ocr/RECOGNITION.md`                    |
| Line                       | `ocr/RECOGNITION.md`                    |
| Paragraph                  | `ocr/RECOGNITION.md`                    |
| Writing Mode               | `ocr/TEXT_DIRECTION.md`                 |
| Line Direction             | `ocr/TEXT_DIRECTION.md`                 |
| Character Flow             | `ocr/TEXT_DIRECTION.md`                 |
| Direction Confidence       | `ocr/TEXT_DIRECTION.md`                 |
| Layout Tree                | `ocr/LAYOUT.md`                         |
| Panel                      | `ocr/LAYOUT.md`                         |
| Container                  | `ocr/LAYOUT.md`                         |
| Block                      | `ocr/LAYOUT.md`                         |
| Spatial Relationship Graph | `ocr/LAYOUT.md`                         |
| OCR Document               | `ocr/POSTPROCESS.md`                    |
| OCR Quality Report         | `ocr/QUALITY.md`                        |
| Reading Order Graph        | `ocr/READING_ORDER.md`                  |
| Main Reading Sequence      | `ocr/READING_ORDER.md`                  |
| Auxiliary Sequence         | `ocr/READING_ORDER.md`                  |
| OCR Provider Contract      | `ocr/PROVIDERS.md`                      |
| OCR Provider Adapter       | `ocr/PROVIDERS.md`                      |
| Cancellation               | Runtime                                 |
| Retry                      | Runtime                                 |
| Scheduling                 | Runtime                                 |
| WorkItem                   | Runtime                                 |
| Attempt                    | Runtime                                 |
| Artifact Lifecycle         | Runtime                                 |
| Runtime Cache Policy       | Runtime                                 |
| Runtime Observability      | Runtime                                 |
| Infrastructure Logging     | `03-infrastructure/logging/`            |
| Infrastructure Telemetry   | `03-infrastructure/telemetry/`          |
| Event Transport            | Event Bus Architecture / Infrastructure |
| Resource Manager Contract  | `03-infrastructure/resource-manager/`   |
| Translation semantics      | Translation Module                      |
| Render semantics           | Presentation Module                     |
| Persistence mechanism      | Storage                                 |

---

# 26. Document Refactoring Rules

When reviewing an existing document:

## Keep

Content stays when the document is the owner of the concept.

---

## Reference

If another document owns the concept:

```text
See <owner document>.
```

Only local usage should remain.

---

## Remove Duplicate Definitions

Do not copy:

* state machines
* error taxonomies
* retry policies
* cache policies
* lifecycle contracts
* telemetry contracts

when another document already owns them.

---

## Preserve Local Constraints

A consumer may still define local constraints.

Example:

```text
Detection Result may be cached only when
Image Version and Detection Profile remain compatible.
```

This is Detection semantic compatibility.

But:

```text
Cache eviction happens after N minutes.
```

belongs to Runtime cache policy.

---

# 27. Refactor Priority

Based on the current OCR documents, the recommended priority is:

```text
1. PIPELINE.md
2. READING_ORDER.md
3. DETECTION.md
```

These currently contain the most cross-owned Runtime, Cache, Event, Performance or Engineering material.

The following require only minor cleanup:

```text
PREPROCESS.md
RECOGNITION.md
LAYOUT.md
TEXT_DIRECTION.md
POSTPROCESS.md
QUALITY.md
PROVIDERS.md
```

---

# 28. Ownership Invariants

1. Every architecture concept has one authoritative owner.

2. A consumer may reference a concept but must not redefine its semantics.

3. Runtime owns execution authority.

4. Business modules own business semantics.

5. Infrastructure owns technical service contracts and implementations.

6. OCR architecture owns image-to-structured-source processing semantics.

7. Provider Adapters do not own business or Runtime decisions.

8. Retry and cancellation are never independently owned by OCR stages.

9. Cache semantic compatibility may be defined by the producing domain, but cache lifecycle belongs to Runtime.

10. Confidence belongs to the component that produces it; Quality aggregates confidence without redefining it.

11. OCR Document is currently owned by Postprocessing until a dedicated shared-artifact contract supersedes that ownership.

12. Reading Order never mutates Recognition, Geometry, Layout or Direction.

13. Quality Assessment never mutates OCR Document or directly executes Runtime decisions.

14. Event transport semantics belong to Event Bus; domain event meaning belongs to the producing domain.

15. Provider-native models never cross the Provider Adapter boundary.

---

# 29. Refactoring Goal

The goal is not to reduce documentation for its own sake.

The goal is:

```text
One concept
    ↓
One definition
    ↓
Clear references
    ↓
Less contradiction
    ↓
Easier maintenance
```

After the ownership review is applied, detailed documents may remain long where their domain genuinely requires detail.

Length is acceptable.

Duplicate authority is not.

---

# 30. Next Refactoring Sequence

Apply this ownership map in the following order:

```text
PIPELINE.md
    ↓
READING_ORDER.md
    ↓
DETECTION.md
```

Then perform a lightweight consistency pass over:

```text
PREPROCESS.md
RECOGNITION.md
LAYOUT.md
TEXT_DIRECTION.md
POSTPROCESS.md
QUALITY.md
PROVIDERS.md
```

After the OCR document set is consistent, update `PROJECT_STATUS.md` and proceed to the next architecture stabilization area.
