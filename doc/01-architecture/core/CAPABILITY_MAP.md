# CRAI Capability Map

> **Project:** CRAI
> **Path:** `doc/01-architecture/core/CAPABILITY_MAP.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-10

---

# 1. Purpose

This document defines the product and architecture capabilities CRAI may require to support minimally disruptive foreign-language reading.

A capability describes:

```text
what CRAI must be able to do
```

It does not directly define:

```text
source-code folders
modules
services
classes
packages
deployment units
Runtime workers
provider implementations
```

The Capability Map exists so product requirements remain visible independently from implementation structure.

---

# 2. Capability Map vs Module Map

These concepts are different.

```text
Capability
    = what CRAI needs to be able to do

Module
    = which architecture boundary owns behavior/data

Runtime
    = how executable work is scheduled and performed

Infrastructure
    = technical mechanism supporting the system
```

One capability may involve several modules.

One module may implement several related capabilities.

---

# 3. Central Architecture Rule

Capability descriptions must not silently assign ownership that belongs elsewhere.

Example:

```text
Capability:
Retry recoverable Translation work

Does NOT imply:
Translation module owns retry execution
```

The actual architecture may be:

```text
Translation
    classifies retryable failure

Runtime
    owns Retry Policy / Attempt creation
```

---

# 4. Current Product Direction

CRAI is intended to help Vietnamese users read foreign-language content with minimal interruption.

Initial language priorities:

```text
Simplified Chinese   → Vietnamese
Traditional Chinese  → Vietnamese
English              → Vietnamese
```

The architecture remains language-neutral.

---

# 5. Primary Reading Modes

CRAI recognizes two primary reading modes.

## 5.1 Structured Text Reading

Examples:

```text
web novel
light novel
HTML chapter
copied text
text document
future EPUB/document formats
```

Preferred rule:

```text
reliable structured text
    ↓
use text path
```

Do not use OCR unnecessarily.

---

## 5.2 Image-Based Reading

Examples:

```text
manga
manhua
manhwa
screenshots
scanned pages
image-based readers
```

These may require:

```text
Capture
Recognition
Text Processing
Translation
Presentation
```

depending on source quality.

---

# 6. Capability Status Model

Capability status remains independent from implementation status.

Use:

| Status      | Meaning                                           |
| ----------- | ------------------------------------------------- |
| `Candidate` | Potentially useful; product need not confirmed    |
| `Proposed`  | Expected to be useful; design not final           |
| `Validated` | Confirmed through research/prototype/user testing |
| `MVP`       | Explicitly included in first usable product       |
| `Deferred`  | Useful but intentionally postponed                |
| `Rejected`  | Reviewed and currently unsuitable                 |

Do not promote:

```text
Proposed → Validated
```

merely because architecture documentation exists.

Validation requires evidence.

---

# 7. Feasibility Scale

| Level     | Meaning                                               |
| --------- | ----------------------------------------------------- |
| `High`    | Commonly achievable with established technology       |
| `Medium`  | Feasible but quality/platform/engineering-sensitive   |
| `Low`     | Possible but currently unreliable or disproportionate |
| `Unknown` | Requires prototype/research evidence                  |

Feasibility is not priority.

---

# 8. Top-Level Capability Map

```text
CRAI
├── Source Acquisition
├── Source Observation
├── Content Classification
├── Recognition
├── Text Processing
├── Translation
├── Knowledge and Consistency
├── Presentation
├── User Interaction
├── Reading Session
├── Runtime Execution and Responsiveness
├── Storage and Recovery
├── Provider Management
├── Privacy and Security
├── Diagnostics and Quality
└── Import, Export, and Integration
```

These are capability groups.

They are not automatically one-to-one module boundaries.

---

# 9. Source Acquisition

## 9.1 Purpose

Obtain content from a user-authorized source using the highest-quality representation available.

Preferred order:

```text
Structured semantic text
    ↓
Document text
    ↓
Image with metadata
    ↓
Raw image
    ↓
Screen capture
```

---

# 10. Acquisition Capabilities

| Capability                         | Description                                | Feasibility | Status    |
| ---------------------------------- | ------------------------------------------ | ----------: | --------- |
| Screen Region Capture              | Capture a selected rectangular screen area |        High | Proposed  |
| Application Window Capture         | Capture a selected application window      |        High | Proposed  |
| Continuous Visual Observation      | Acquire visual source repeatedly           |        High | Proposed  |
| Clipboard Text Input               | Accept explicitly supplied clipboard text  |        High | Proposed  |
| Clipboard Image Input              | Accept explicitly supplied clipboard image |        High | Proposed  |
| Image File Input                   | Import common image formats                |        High | Proposed  |
| Folder Image Input                 | Consume ordered local images               |        High | Candidate |
| Folder Watch                       | Detect new/changed files                   |        High | Deferred  |
| Browser Structured Text Extraction | Obtain semantic browser text               |        High | Proposed  |
| Browser Image Discovery            | Discover usable visual resources           |      Medium | Candidate |
| Direct URL Import                  | Import supported content from URL          |      Medium | Deferred  |
| PDF Text Import                    | Extract text from supported PDFs           |        High | Deferred  |
| Scanned PDF Import                 | Render scanned pages into visual input     |        High | Deferred  |
| EPUB Import                        | Preserve chapter/paragraph structure       |        High | Deferred  |

---

# 11. Acquisition Constraints

Acquisition must not directly decide:

```text
OCR provider
Translation provider
Runtime retry
Presentation layout
```

Platform/native handles remain inside adapters.

Stable CRAI contracts receive normalized source descriptions.

---

# 12. Source Observation

## 12.1 Purpose

Determine whether continuously observed content has changed enough to justify new authoritative processing.

---

# 13. Observation Capabilities

| Capability                  | Description                           | Feasibility | Status    |
| --------------------------- | ------------------------------------- | ----------: | --------- |
| Frame Difference Detection  | Detect significant visual change      |        High | Proposed  |
| Region Difference Detection | Detect changed subregions             |      Medium | Candidate |
| Scroll Detection            | Infer user scrolling                  |      Medium | Proposed  |
| Page Transition Detection   | Infer page/chapter transition         |      Medium | Candidate |
| Stable Frame Detection      | Avoid processing transient motion     |        High | Proposed  |
| Duplicate Frame Detection   | Avoid repeated equivalent visual work |        High | Proposed  |
| Zoom/Scale Change Detection | Detect geometry-affecting changes     |      Medium | Candidate |
| Browser DOM Observation     | Detect semantic page changes          |        High | Proposed  |

---

# 14. Observation Ownership Rule

Observation may determine:

```text
candidate changed
candidate stable
candidate duplicate
```

It does not directly:

```text
cancel Translation
restart Recognition
create downstream Requests
```

Those consequences pass through Application/Runtime authority.

---

# 15. Observation Risks

Important risks:

```text
animation
cursor movement
advertisements
video
loading indicators
slow scrolling
browser repaint
zoom
responsive layout
```

Prototype measurements should include false-positive and missed-change rates.

---

# 16. Content Classification

## 16.1 Purpose

Identify enough source characteristics to select appropriate processing capabilities.

---

# 17. Classification Capabilities

| Capability                  | Description                                  | Feasibility | Status    |
| --------------------------- | -------------------------------------------- | ----------: | --------- |
| Content Type Detection      | Structured text/image/scanned/mixed          |        High | Proposed  |
| Language Detection          | Estimate source language                     |        High | Proposed  |
| Script Detection            | Han/Latin/other script                       |        High | Proposed  |
| Text Presence Detection     | Determine whether image likely contains text |        High | Proposed  |
| Reading Direction Detection | Horizontal/vertical/direction hints          |      Medium | Proposed  |
| Comic Page Detection        | Estimate comic-like layout                   |      Medium | Candidate |
| Prose Detection             | Estimate long-form prose                     |      Medium | Candidate |
| Mixed Content Detection     | Detect combined text/image structures        |      Medium | Candidate |

Classification informs planning.

It does not permanently lock the user into one processing path.

---

# 18. Recognition

## 18.1 Purpose

Convert visual text into normalized Recognition-owned text/geometry information.

Recognition applies only when reliable structured text is unavailable.

---

# 19. Recognition Capabilities

| Capability                   | Description                         | Feasibility | Status    |
| ---------------------------- | ----------------------------------- | ----------: | --------- |
| Image Preprocessing          | Prepare image for Recognition       |        High | Proposed  |
| Text Region Detection        | Locate likely text regions          |        High | Proposed  |
| Text Recognition             | Convert visual glyphs into text     |        High | Proposed  |
| Recognition Confidence       | Preserve confidence/uncertainty     |        High | Proposed  |
| Vertical Chinese Recognition | Recognize vertical Han text         |      Medium | Proposed  |
| Stylized Font Recognition    | Handle decorative/distorted text    |      Medium | Candidate |
| Geometry Preservation        | Preserve recognized-region geometry |        High | Proposed  |
| Text Direction Preservation  | Preserve writing direction          |        High | Proposed  |
| Reading Hints                | Produce ordering/layout hints       |      Medium | Proposed  |
| Speech Region Detection      | Detect likely dialogue containers   |      Medium | Candidate |
| Sound Effect Recognition     | Treat SFX separately                |  Low–Medium | Deferred  |
| Handwritten Recognition      | Recognize handwriting               |  Low–Medium | Deferred  |

---

# 20. Recognition Boundary

Recognition produces Recognition semantics.

Conceptually:

```text
Capture Artifact
    ↓
Recognition
    ↓
RecognitionArtifact
```

Recognition does not construct TranslationUnit.

Recognition does not own Runtime retry.

Recognition does not decide downstream execution.

---

# 21. Recognition Quality Risks

Key risks remain:

```text
low resolution
vertical Chinese
stylized fonts
artwork overlap
unusual reading order
compressed images
poor contrast
```

These require representative test datasets.

---

# 22. Text Processing

## 22.1 Purpose

Transform recognized or structured source text into coherent semantic source-document structure.

---

# 23. Text Processing Capabilities

| Capability                     | Description                               | Feasibility | Status    |
| ------------------------------ | ----------------------------------------- | ----------: | --------- |
| Unicode Normalization          | Normalize code points/punctuation         |        High | Proposed  |
| OCR Artifact Cleanup           | Correct predictable recognition artifacts |      Medium | Proposed  |
| Line Reconstruction            | Reconstruct lines/sentences               |      Medium | Proposed  |
| Paragraph Reconstruction       | Rebuild prose structure                   |      Medium | Proposed  |
| Dialogue Grouping              | Group related dialogue structures         |      Medium | Proposed  |
| Reading Order Reconstruction   | Determine semantic source order           |      Medium | Proposed  |
| Manual Order Correction        | Correct incorrect reconstructed order     |      Medium | Candidate |
| Structural Segmentation        | Build semantic blocks/segments            |        High | Proposed  |
| Decorative Text Classification | Separate optional/decorative text         |      Medium | Candidate |
| Source Language Refinement     | Refine language hints                     |        High | Proposed  |
| Semantic Metadata Preservation | Preserve geometry/order/provenance        |        High | Proposed  |

---

# 24. Text Processing Output Boundary

Preferred:

```text
RecognitionArtifact
or
Structured Source Input
        ↓
Text Processing
        ↓
SourceDocumentArtifact
```

---

# 25. Text Processing Does Not Own TranslationUnit

Deprecated capability wording:

```text
Transform extracted text
into coherent translation units
```

Correct v2 wording:

```text
Transform source text
into coherent semantic source-document structure
```

Translation determines:

```text
TranslationUnit
TranslationBatch
context assembly
provider constraints
```

---

# 26. Proper Names and Terms

Detection of possible:

```text
names
terms
entities
recurring vocabulary
```

may contribute metadata upstream.

But final Translation context/terminology usage belongs to Translation/Knowledge contracts.

---

# 27. Translation

## 27.1 Purpose

Translate accepted SourceDocument content into Vietnamese while preserving semantic alignment, context and terminology.

---

# 28. Translation Capabilities

| Capability                   | Description                              | Feasibility | Status    |
| ---------------------------- | ---------------------------------------- | ----------: | --------- |
| TranslationUnit Construction | Build bounded semantic translation units |        High | Proposed  |
| TranslationBatch Planning    | Group compatible units efficiently       |        High | Proposed  |
| Context Assembly             | Select relevant contextual information   |        High | Proposed  |
| Glossary Application         | Apply immutable terminology snapshot     |        High | Proposed  |
| Single Unit Translation      | Translate one unit                       |        High | Proposed  |
| Batch Translation            | Translate related units                  |        High | Proposed  |
| Context-Aware Translation    | Translate using bounded context          |        High | Proposed  |
| Streaming Translation        | Consume partial provider output          |      Medium | Candidate |
| Language Pair Configuration  | Source/target language configuration     |        High | Proposed  |
| Translation Alignment        | Preserve source-target mapping           |        High | Proposed  |
| Translation Quality Hints    | Expose uncertainty when meaningful       |      Medium | Candidate |
| Style Configuration          | Apply reading/style profile              |      Medium | Candidate |
| User Retranslation           | Request another Translation execution    |        High | Proposed  |
| Translation Correction       | Accept user correction separately        |        High | Proposed  |

---

# 29. Removed Translation Execution Ownership

The Translation capability group does not itself own:

```text
Runtime retry
Runtime cancellation
WorkItem lifecycle
Attempt lifecycle
Scheduler admission
global provider fallback execution
```

Those belong to Runtime/provider architecture.

---

# 30. Translation Retry Capability

The product still needs:

```text
recover from retryable Translation failure
```

But architecture ownership is:

```text
Translation
    ↓
classifies result/error

Runtime
    ↓
Retry Policy
    ↓
new Attempt
```

Capability need and module authority are deliberately separate.

---

# 31. Translation Cancellation Capability

Likewise, CRAI needs obsolete Translation work to stop or lose authority.

Correct ownership:

```text
Application / Session condition
        ↓
Runtime Cancellation
        ↓
Attempt/WorkItem state
```

Translation provider adapters cooperate where possible.

---

# 32. Provider Fallback Capability

The product may need:

```text
alternate provider after recoverable failure
```

Implementation may involve:

```text
Provider Management
+
Translation/Recognition policy
+
Runtime new Attempt
```

Do not assign entire fallback lifecycle to Translation alone.

---

# 33. Knowledge and Consistency

## 33.1 Purpose

Maintain deliberate consistency for names, terminology, corrections and reusable translation knowledge.

---

# 34. Knowledge Capabilities

| Capability                    | Description                            | Feasibility | Status    |
| ----------------------------- | -------------------------------------- | ----------: | --------- |
| User Glossary                 | Store preferred translations           |        High | Proposed  |
| Series Glossary               | Scope terminology to one work/series   |        High | Candidate |
| Global Glossary               | User-wide terminology                  |        High | Candidate |
| Translation Memory            | Reuse suitable historical translations |        High | Proposed  |
| Name Dictionary               | Maintain stable name mappings          |        High | Proposed  |
| User Override                 | Preserve explicit corrections          |        High | Proposed  |
| Context Notes                 | Character/world/terminology notes      |        High | Deferred  |
| Similarity Matching           | Reuse near-duplicate knowledge         |      Medium | Candidate |
| Automatic Preference Learning | Infer behavior from corrections        |      Medium | Deferred  |

---

# 35. Knowledge Scope

Knowledge must have explicit scope.

Examples:

```text
Translation Unit
Reading Session
Chapter
Series
Source Profile
Local User
Global
```

Cross-series leakage must never occur implicitly.

---

# 36. Presentation

## 36.1 Purpose

Transform semantic Translation output into readable presentation without changing Translation meaning.

---

# 37. Text Presentation Capabilities

| Capability                         | Description                            | Feasibility | Status    |
| ---------------------------------- | -------------------------------------- | ----------: | --------- |
| Reader Presentation                | Produce readable prose presentation    |        High | Proposed  |
| Parallel Source/Translation Layout | Present source and target together     |        High | Candidate |
| Typography Rules                   | Define readable font/size/line spacing |        High | Proposed  |
| Paragraph Layout                   | Preserve paragraph structure           |        High | Proposed  |
| Reading Width                      | Control readable line width            |        High | Proposed  |
| Alignment/Indentation              | Define semantic text layout            |        High | Proposed  |
| Chapter Presentation               | Present structured chapter navigation  |        High | Deferred  |

---

# 38. Image Presentation Capabilities

| Capability                 | Description                                    | Feasibility | Status   |
| -------------------------- | ---------------------------------------------- | ----------: | -------- |
| Region-Linked Presentation | Associate translations with visual regions     |        High | Proposed |
| Side-Panel Presentation    | Produce ordered translated entries             |        High | Proposed |
| Overlay Layout             | Position translated content relative to source |      Medium | Proposed |
| Overlay Scaling            | Adapt geometry after scale changes             |      Medium | Proposed |
| Overflow Detection         | Detect insufficient display space              |        High | Proposed |
| Adaptive Text Fitting      | Fit Vietnamese text into bounds                |        High | Proposed |
| Source Text Removal        | Remove source text                             |      Medium | Deferred |
| Background Reconstruction  | Restore artwork/background                     |  Medium–Low | Deferred |
| Bubble Text Insertion      | Place Vietnamese text into bubbles             |  Medium–Low | Deferred |
| Translated Image Artifact  | Produce exportable translated image            |        High | Deferred |

---

# 39. Presentation vs UI Adapter

Presentation owns:

```text
semantic display structure
layout
geometry
fitting
PresentationArtifact
```

UI Adapter owns:

```text
platform-facing ViewModel
native interaction
native rendering adaptation
```

Therefore capabilities such as:

```text
Theme
Font selector UI
hover interaction
native window controls
```

should not imply Presentation-domain ownership.

---

# 40. Initial Presentation Direction

Recommended early product path remains:

```text
Side panel
    ↓
Region-linked list
    ↓
Simple overlay
```

Permanent source-image modification remains deferred.

---

# 41. User Interaction

## 41.1 Purpose

Allow users to control reading without repeatedly interrupting the source experience.

---

# 42. Interaction Capabilities

| Capability                 | Description                         | Feasibility | Status    |
| -------------------------- | ----------------------------------- | ----------: | --------- |
| Source Selection           | Select source type/location         |        High | Proposed  |
| One-Time Region Selection  | Continue observing selected area    |        High | Proposed  |
| Pause/Resume Intent        | Control reading session behavior    |        High | Proposed  |
| Stop Reading Intent        | End current reading session         |        High | Proposed  |
| Manual Retranslation       | Ask Application to retranslate      |        High | Proposed  |
| OCR/Recognition Correction | Correct recognized source text      |        High | Candidate |
| Region Adjustment          | Correct detected region geometry    |      Medium | Candidate |
| Reading Mode Selection     | Choose reader/panel/overlay UX      |        High | Proposed  |
| Provider Preference        | Express provider preference         |        High | Proposed  |
| Language Selection         | Choose source/target languages      |        High | Proposed  |
| Quick Glossary Edit        | Edit terminology while reading      |        High | Proposed  |
| Translation Correction     | Correct output                      |        High | Proposed  |
| Keyboard Shortcuts         | Fast reading controls               |        High | Proposed  |
| Hover/Focus Behavior       | Region-local interaction            |        High | Candidate |
| Zoom Reaction              | Rebuild relevant projection/layout  |      Medium | Proposed  |
| Scroll Reaction            | Adapt observation/planning behavior |      Medium | Proposed  |

---

# 43. UI Intent Rule

User interaction produces:

```text
UiIntent
```

or Application commands.

It does not directly:

```text
retry Runtime Attempt
cancel module internals
start downstream pipeline stage
```

---

# 44. Reading Session

## 44.1 Purpose

Maintain the lifecycle and authoritative context of one reading experience.

---

# 45. Reading Session Capabilities

| Capability               | Description                               | Feasibility | Status    |
| ------------------------ | ----------------------------------------- | ----------: | --------- |
| Create Reading Session   | Establish one reading context             |        High | Proposed  |
| Start/Activate Session   | Begin active reading behavior             |        High | Proposed  |
| Pause Session            | Suspend new session-authorized activity   |        High | Proposed  |
| Resume Session           | Resume from current authoritative context |        High | Proposed  |
| Stop Session             | End session lifecycle                     |        High | Proposed  |
| Reading Context Tracking | Own current source/session context        |        High | Proposed  |
| ReadingContextRevision   | Version committed context changes         |        High | Proposed  |
| Session Configuration    | Maintain session-specific configuration   |        High | Proposed  |
| Reading Position         | Track current reading position            |        High | Candidate |
| Session Progress         | Track chapter/session progress            |        High | Deferred  |
| Bookmark                 | Persist selected position                 |        High | Deferred  |
| Session Recovery         | Recover durable session context           |        High | Deferred  |

---

# 46. Removed Reading Session `Processing State Tracking`

Reading Session does not need to own:

```text
capturing
recognizing
translating
presenting
```

as one session state.

Runtime owns execution state.

Application/UI may expose an aggregate progress projection without transferring authority.

---

# 47. Removed Reading Session Stale-Result Authority

The product capability:

```text
prevent stale results from becoming current
```

remains required.

But publication/execution authority belongs to:

```text
Runtime
+
Artifact owner
```

Reading Session provides relevant domain authority such as:

```text
ReadingContextRevision
```

---

# 48. Reading Session Identifiers

Architecture-relevant identities include:

```text
SessionId
ReadingContextRevision
```

Do not define Reading Session as owner of generic:

```text
FrameId
SegmentId
TranslationRequestId
ProcessingAttemptId
```

Those belong to their respective modules/Runtime.

---

# 49. Runtime Execution and Responsiveness

## 49.1 Purpose

Ensure expensive work executes responsively, concurrently and safely without placing execution authority into business modules.

---

# 50. Runtime Capabilities

| Capability                | Description                          | Feasibility | Status    |
| ------------------------- | ------------------------------------ | ----------: | --------- |
| RuntimeRevision           | Define coherent execution authority  |        High | Proposed  |
| WorkItem Planning Support | Represent schedulable logical work   |        High | Proposed  |
| Attempt Execution         | Execute concrete work attempts       |        High | Proposed  |
| Background Execution      | Keep heavy work off UI thread        |        High | Proposed  |
| Work Queue                | Queue runnable WorkItems             |        High | Proposed  |
| Dependency Scheduling     | Run work when prerequisites are met  |        High | Proposed  |
| Cancellation              | Cancel/supersede obsolete execution  |        High | Proposed  |
| Retry Policy              | Create bounded retries               |        High | Proposed  |
| Deadline/Timeout          | Bound queue and execution time       |        High | Proposed  |
| Priority Scheduling       | Prefer user-visible/current work     |        High | Candidate |
| Deduplication             | Avoid redundant execution            |        High | Proposed  |
| Backpressure              | Bound pending work                   |        High | Proposed  |
| Parallel Execution        | Run independent work concurrently    |        High | Candidate |
| Resource Limits           | Bound CPU/RAM/GPU/network usage      |        High | Proposed  |
| Prefetch                  | Execute probable future work         |      Medium | Deferred  |
| Adaptive Quality          | Trade quality/cost/latency by policy |      Medium | Candidate |

---

# 51. Rendering Is Not a Runtime Job Category by Default

The v1 capability:

```text
schedule capture, OCR, translation, and rendering jobs
```

mixes Presentation/UI with Runtime processing.

v2 prefers:

```text
Runtime schedules logical WorkItems
```

according to BusinessExecutionPlan.

Native UI rendering remains UI/platform-owned.

---

# 52. Runtime Does Not Own Business Semantics

Runtime knows:

```text
work identity
dependencies
priority
deadline
retry
cancellation
resources
```

It does not decide:

```text
how OCR text is normalized
how TranslationUnits are constructed
how Presentation layout works
```

---

# 53. Storage and Recovery

## 53.1 Purpose

Retain only data justified by:

```text
user intent
recovery
cache reuse
knowledge consistency
diagnostics
```

---

# 54. Storage Capabilities

| Capability                 | Description                          | Feasibility | Status   |
| -------------------------- | ------------------------------------ | ----------: | -------- |
| Preference Storage         | Persist user preferences             |        High | Proposed |
| Recognition Cache          | Reuse compatible Recognition output  |        High | Proposed |
| Translation Cache          | Reuse compatible translation output  |        High | Proposed |
| Content Fingerprints       | Identify repeated content            |        High | Proposed |
| Glossary Storage           | Persist terminology                  |        High | Proposed |
| Translation Memory Storage | Persist reusable translations        |        High | Proposed |
| Session Context Storage    | Preserve recoverable context         |        High | Deferred |
| Reading History            | Persist reading history              |        High | Deferred |
| Imported Content Storage   | Manage deliberately imported content |        High | Deferred |
| Offline Packages           | Bundle content for offline use       |        High | Deferred |
| Backup/Restore             | Export/restore user-owned CRAI data  |        High | Deferred |

---

# 55. Storage Is Not Artifact Authority

A persisted object is not automatically current.

Cache/storage retrieval still requires:

```text
compatibility checks
scope checks
current authority validation
```

---

# 56. Provider Management

## 56.1 Purpose

Allow interchangeable local/remote processing implementations without leaking provider details into business contracts.

---

# 57. Provider Capabilities

| Capability                        | Description                                   | Feasibility | Status    |
| --------------------------------- | --------------------------------------------- | ----------: | --------- |
| Recognition Provider Registration | Register Recognition implementations          |        High | Proposed  |
| Translation Provider Registration | Register Translation implementations          |        High | Proposed  |
| Local Provider Support            | Use local implementations/models              |        High | Proposed  |
| Remote Provider Support           | Use external APIs                             |        High | Proposed  |
| Capability Discovery              | Describe languages/modes/limits               |        High | Proposed  |
| Provider Availability             | Determine provider usability                  |        High | Candidate |
| Provider Configuration            | Configure endpoint/model/etc.                 |        High | Proposed  |
| Provider Selection Support        | Provide candidates based on capability/policy |        High | Proposed  |
| Provider Fallback Support         | Permit policy-driven alternate execution      |      Medium | Candidate |
| Usage/Cost Observation            | Measure remote usage/cost                     |        High | Candidate |
| Runtime Plugin Loading            | Dynamically load providers                    |      Medium | Deferred  |

---

# 58. Provider Management vs Runtime

Provider Management may answer:

```text
which providers are available?
which capabilities do they support?
```

Runtime owns:

```text
which Attempt is executed
when another Attempt occurs
```

Business/module policy determines semantic suitability.

---

# 59. Privacy and Security

## 59.1 Purpose

Protect reading content, credentials and user-owned knowledge.

---

# 60. Privacy Capabilities

| Capability                   | Description                                     | Feasibility | Status    |
| ---------------------------- | ----------------------------------------------- | ----------: | --------- |
| Local-Only Processing Mode   | Restrict eligible work locally                  |      Medium | Candidate |
| Remote Data Disclosure       | Tell user when content leaves device            |        High | Proposed  |
| Credential Protection        | Protect provider secrets                        |        High | Proposed  |
| Sensitive Logging Prevention | Exclude private content/secrets                 |        High | Proposed  |
| Temporary Data Cleanup       | Remove obsolete temporary data                  |        High | Proposed  |
| Retention Controls           | Configure cache/history retention               |        High | Candidate |
| Private Session Mode         | Minimize persistent footprint                   |        High | Candidate |
| Explicit Export Consent      | Require user action for export                  |        High | Proposed  |
| Clipboard Consent Boundary   | Require explicit clipboard use                  |        High | Proposed  |
| Notification Privacy         | Prevent sensitive external notification leakage |        High | Proposed  |

---

# 61. Privacy Boundary

Remote providers receive only the minimum required data.

Credentials flow only through:

```text
Secret Management
    ↓
Provider Adapter
```

Never through:

```text
Artifact
Event Bus
ViewModel
cache key
diagnostic export
```

---

# 62. Diagnostics and Quality

## 62.1 Purpose

Make correctness, latency, provider failures and quality problems observable without becoming part of execution authority.

---

# 63. Diagnostics Capabilities

| Capability                        | Description                             | Feasibility | Status    |
| --------------------------------- | --------------------------------------- | ----------: | --------- |
| Runtime Timing                    | Measure WorkItem/Attempt timing         |        High | Proposed  |
| Structured Error Observation      | Preserve owner/error identity           |        High | Proposed  |
| Recognition Confidence Projection | Surface uncertainty                     |        High | Candidate |
| Translation Issue Reporting       | Record translation quality issues       |        High | Candidate |
| End-to-End Correlation            | Correlate revisions/work/artifacts      |        High | Proposed  |
| Provider Diagnostics              | Observe availability/errors/rate limits |        High | Proposed  |
| Debug Recognition Overlay         | Visualize Recognition geometry/order    |        High | Candidate |
| Quality Test Dataset              | Maintain representative samples         |        High | Proposed  |
| Regression Evaluation             | Compare behavior across versions        |      Medium | Proposed  |
| User Feedback Capture             | Record explicit feedback/corrections    |        High | Candidate |
| Capability Health Snapshot        | Report module/capability health         |        High | Proposed  |
| Support Bundle Export             | Export privacy-safe diagnostics         |        High | Candidate |

---

# 64. Removed Generic Stage Trace

Prefer typed trace correlation:

```text
ReadingContextRevision
    ↓
RuntimeRevisionId
    ↓
WorkItemId
    ↓
AttemptId
    ↓
ArtifactId
```

rather than:

```text
frame
segment
translation request
stage
```

as one generic trace model.

---

# 65. Diagnostics Is Not Event Bus Telemetry

Do not require:

```text
LogRecorded
MetricUpdated
TraceCompleted
```

business events.

Use Logging/Telemetry infrastructure.

---

# 66. Import, Export, and Integration

## 66.1 Purpose

Connect CRAI to external content and trusted local tools without contaminating core processing contracts.

---

# 67. Integration Capabilities

| Capability                   | Description                            | Feasibility | Status    |
| ---------------------------- | -------------------------------------- | ----------: | --------- |
| Browser Connector            | Exchange normalized source data        |        High | Proposed  |
| Browser In-Page Presentation | Present via extension frontend         |        High | Candidate |
| File Import                  | Import supported formats               |        High | Deferred  |
| Translation Export           | Export translated text                 |        High | Deferred  |
| Bilingual Export             | Export aligned source/target           |        High | Deferred  |
| Translated Image Export      | Export Presentation output             |        High | Deferred  |
| Region Translation Export    | Export aligned region/text data        |        High | Deferred  |
| Local API                    | Trusted local application integration  |        High | Deferred  |
| Automation Hook              | Trigger supported use cases externally |      Medium | Deferred  |
| Cloud Synchronization        | Sync user-owned settings/knowledge     |        High | Deferred  |

---

# 68. Integration Boundary

External integration must use stable contracts.

Do not expose:

```text
module internals
native handles
provider SDK objects
Runtime queues
mutable Artifacts
```

---

# 69. Cross-Capability Flow — Screen Comic

Product-level capability flow:

```text
Select visual source
    ↓
Observe changes
    ↓
Accept stable visual content
    ↓
Recognize text and geometry
    ↓
Build SourceDocument structure
    ↓
Translate bounded contextual units
    ↓
Build Presentation
    ↓
Display through UI
```

---

# 70. Architecture View — Screen Comic

The same use case maps architecturally to:

```text
Reading Session
    ↓
ReadingContextRevision
    ↓
Business Execution Planning
    ↓
RuntimeRevision
    ↓
Capture Artifact
    ↓
RecognitionArtifact
    ↓
SourceDocumentArtifact
    ↓
TranslationArtifact
    ↓
PresentationArtifact
    ↓
UI Adapter ViewModel
```

Capability flow and authority flow are deliberately separate views.

---

# 71. Cross-Capability Flow — Browser Novel

```text
Browser structured text
    ↓
Content isolation
    ↓
Text Processing
    ↓
SourceDocumentArtifact
    ↓
TranslationUnit/Batch construction
    ↓
Translation
    ↓
Presentation
    ↓
Reader UI
```

Recognition is skipped when structured text is reliable.

---

# 72. Imported Content Flow

```text
User imports content
    ↓
classify source
    ↓
normalize through appropriate source path
    ↓
standard Artifact flow
```

Imported content should not create a parallel processing architecture.

---

# 73. Product Feasibility — High Confidence

Likely technically straightforward:

```text
screen/window capture
clipboard/image import
browser text extraction
basic Chinese/English Recognition
language detection
provider-based Translation
side-panel presentation
local Preferences
glossary
cache
bounded Runtime cancellation/retry
diagnostic correlation
```

These remain product feasibility assessments, not prototype validation.

---

# 74. Quality-Sensitive Capabilities

Require representative evidence:

```text
vertical Chinese Recognition
stylized comic Recognition
reading-order reconstruction
speech-bubble detection
continuous change detection
overlay tracking
context-aware comic translation
name/term consistency
generic webpage extraction
```

---

# 75. Deferred Product Capabilities

Remain outside first MVP:

```text
complete online-series downloading
content-library management
EPUB/PDF reader suite
permanent image replacement
inpainting
cloud synchronization
plugin marketplace
speaker attribution
automatic long-term learning
```

---

# 76. Main Product Risk

The largest product risk remains end-to-end usability:

```text
source becomes readable
    ↓
CRAI detects it
    ↓
Recognition/Translation finishes
    ↓
useful Vietnamese content becomes visible
```

Individual provider quality is insufficient if the overall interaction is slow or disruptive.

---

# 77. Recommended MVP Product Goal

The initial MVP should prove one complete image-reading use case:

```text
Desktop app
    ↓
select screen region/window once
    ↓
detect stable changed content
    ↓
recognize Chinese/English
    ↓
translate to Vietnamese
    ↓
show region-linked translations
    ↓
allow quick retranslation/correction
```

---

# 78. Recommended MVP Capability Scope

Likely MVP capabilities:

```text
Screen Region Capture
Window Capture

Stable Content Detection
Duplicate Detection

Text Region Detection
Recognition
Geometry Preservation
Reading Order Reconstruction

Text Normalization
SourceDocument Reconstruction

TranslationUnit Construction
Context-Aware Translation
Alignment

Side-Panel Presentation
Region Linking

Reading Session
ReadingContextRevision

Runtime Work Queue
Cancellation
Retry
Backpressure

Recognition/Translation Cache

Basic Glossary

Preferences

Diagnostics
```

Exact `MVP` status should be assigned only when the implementation scope is formally approved.

---

# 79. Prototype Gate A — Capture and Observation

Validate:

```text
one-time region selection
continuous observation cost
scroll vs stable state
duplicate rejection
usable latency
```

Do not require downstream module cancellation logic inside the prototype.

Runtime implications can be measured separately.

---

# 80. Prototype Gate B — Chinese Recognition

Validate:

```text
Simplified Chinese
horizontal text
vertical text
geometry
confidence
reading-order hints
latency
```

Use representative comic material.

---

# 81. Prototype Gate C — Translation Usefulness

Validate:

```text
Chinese → Vietnamese
contextual grouping
name consistency
terminology consistency
latency
cost
correction speed
```

---

# 82. Prototype Gate D — Presentation

Validate:

```text
side-panel readability
region association
simple overlay
zoom/resize behavior
minimal obstruction
Vietnamese text overflow
```

---

# 83. Prototype Gate E — Structured Novel

After image MVP:

```text
browser content isolation
paragraph preservation
dialogue formatting
incremental Translation
comfortable Vietnamese reader layout
```

---

# 84. Open Product Decisions

Remain open pending prototype evidence:

```text
desktop-only vs multi-frontend MVP
main side-panel/overlay UX
active-window following
automatic session behavior
full-frame vs region Recognition
Recognition provider choice
Translation provider/model
context size
streaming UX
cache retention
browser extraction strategy
```

---

# 85. Architecture Decisions Already Closed

The following are no longer open capability questions:

```text
Reading Session does not own processing execution state

Runtime owns:
    WorkItem
    Attempt
    retry
    cancellation
    scheduling/backpressure

Text Processing owns:
    SourceDocument construction

Translation owns:
    TranslationUnit/Batch/context assembly

Presentation owns:
    semantic presentation

UI Adapter owns:
    platform-facing adaptation

Event Bus carries:
    committed facts

Event Bus does not carry:
    execution commands
```

---

# 86. Capability-to-Architecture Mapping Rule

A capability maps to architecture only after asking:

```text
What semantic authority is involved?

What canonical data is owned?

Does this require execution?

Does this require infrastructure?

Does this cross platform/provider boundaries?
```

---

# 87. Capability-to-Module Transition

A dedicated module boundary is justified when one or more are true:

```text
distinct semantic ownership
distinct lifecycle
stable input/output contract
independent replacement
independent testing
provider/platform isolation
significant coupling reduction
```

---

# 88. Capability Does Not Force One Module

Example:

```text
Provider fallback
```

may involve:

```text
Translation / Recognition
    → semantic error classification

Provider Management
    → provider availability

Runtime
    → new Attempt
```

This is one product capability spanning multiple architecture owners.

---

# 89. Capability Does Not Override Ownership

If Capability Map wording conflicts with:

```text
OWNERSHIP_MAP.md
MODULE.md
CONTRACT.md
Runtime architecture
```

the Capability Map must be corrected.

It must not redefine established ownership accidentally.

---

# 90. Cross-Document Authority

Use:

```text
CAPABILITY_MAP.md
    → product need

MODULE_MAP.md
    → module topology

OWNERSHIP_MAP.md
    → semantic ownership

MODULE_DEPENDENCY.md
    → allowed dependencies

02-modules/*/CONTRACT.md
    → stable module interfaces

Runtime architecture
    → execution authority
```

---

# 91. Capability Invariants

1. Capability describes what CRAI can do, not implementation location.

2. Capability status is independent from implementation status.

3. Architecture documentation does not equal prototype validation.

4. Structured text is preferred over OCR where reliable.

5. Recognition does not own TranslationUnit.

6. Text Processing ends at semantic SourceDocument output.

7. Translation owns TranslationUnit/Batch/context construction.

8. Runtime owns WorkItem and Attempt execution.

9. Runtime owns retry and cancellation mechanics.

10. Reading Session owns reading context, not processing stage state.

11. Provider Management does not own WorkItem execution.

12. Presentation does not own native UI rendering.

13. UI Adapter does not own Presentation semantics.

14. Diagnostics observes but does not control business execution.

15. Event Bus does not represent a product execution capability.

16. Cache never becomes authority.

17. Credentials remain inside secure provider boundaries.

18. Product capabilities may span several modules.

19. Module boundaries must not be reverse-engineered mechanically from this hierarchy.

20. Prototype evidence is required before marking capabilities Validated.

---

# 92. Deprecated v1 Capability Assignments

The following v1 ownership implications are deprecated:

```text
Text Understanding
    → TranslationUnit construction
    → Context Building

Translation
    → Runtime Retry Policy
    → Runtime Cancellation
    → complete fallback execution

Reading Session
    → Processing State Tracking
    → stale execution-result authority

Performance and Scheduling
    → rendering jobs

Diagnostics
    → generic stage-oriented ProcessingTrace
```

The product needs remain.

The architecture owners changed.

---

# 93. Preserved v1 Product Principles

The following v1 principles remain valid:

```text
minimal interruption
structured text preferred over OCR
Chinese-first but language-neutral design
non-destructive presentation first
manual correction available
privacy-first captured content handling
local-first MVP
provider replaceability
representative prototype datasets
end-to-end latency as product concern
```

---

# 94. Recommended Review Sequence

After this Capability Map is synchronized:

```text
MODULE_MAP
    ↓
OWNERSHIP_MAP
    ↓
MODULE_DEPENDENCY
    ↓
end-to-end flows
    ↓
prototype gates
    ↓
technology selection
```

Capability Map should no longer be used to invent new module boundaries that have already been architecturally resolved without an explicit reason.

---

# 95. Related Documents

```text
doc/01-architecture/core/
├── CAPABILITY_MAP.md
├── DATA_FLOW.md
├── STATE_MACHINE.md
├── EVENT_BUS.md
├── EVENT_CONVENTION.md
└── README.md

doc/01-architecture/modules/
├── MODULE_MAP.md
├── MODULE_DEPENDENCY.md
└── OWNERSHIP_MAP.md

doc/01-architecture/runtime/
├── BUSINESS_PIPELINE_ORCHESTRATION.md
├── PIPELINE_RUNTIME.md
├── RETRY_POLICY.md
├── CANCELLATION.md
├── SCHEDULER.md
└── WORK_QUEUE.md

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
```

---

# 96. Completion Criteria

This Capability Map is synchronized when:

* Capability and Module concepts remain separate;
* product statuses are not falsely upgraded;
* Recognition and Text Processing boundaries match current modules;
* Text Processing no longer owns TranslationUnit construction;
* Translation owns TranslationUnit/Batch/context preparation;
* Translation no longer appears to own Runtime retry/cancellation;
* Reading Session no longer owns global processing state;
* stale-result publication authority is separated from Reading Session;
* Runtime execution capabilities are explicit;
* WorkItem/Attempt concepts replace generic processing-job language;
* Presentation is separated from UI-native rendering;
* Provider Management and Runtime responsibilities are distinct;
* Diagnostics use typed Runtime/Artifact correlation instead of generic stage traces;
* Event Bus command semantics are absent;
* MVP capabilities remain product-facing rather than module-implementation tasks.

---

# 97. Summary

The CRAI product still needs approximately the same major capabilities envisioned in v1.

What changed is their architectural allocation.

v1 implicitly leaned toward:

```text
Acquisition
    ↓
Extraction
    ↓
Text Understanding
        builds Translation Units
    ↓
Translation
        owns provider/retry/cancel
    ↓
Presentation

Reading Session
    tracks the whole processing lifecycle
```

Runtime v2 uses:

```text
Product Capabilities
        ↓
Architecture Ownership
        ↓

Reading Session
    owns reading context

Capture
    owns acquisition semantics

Recognition
    owns recognized visual text

Text Processing
    owns SourceDocument structure

Translation
    owns TranslationUnits and semantic translation

Presentation
    owns semantic presentation

Runtime
    owns executable work

UI Adapter
    owns platform adaptation
```

The central principle is:

```text
Capability asks:

"What must CRAI be able to do?"

Ownership asks:

"Who is authoritative for it?"

Runtime asks:

"How does the work execute?"

These questions
must remain separate.
```
