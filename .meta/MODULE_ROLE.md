# CRAI Module Design Rules

> **Project:** CRAI
> **Document:** Module Design Rules
> **Version:** 1.0
> **Status:** Active
> **Language:** English for technical definitions; Vietnamese for discussion and explanation.

---

## 1. Purpose

This document defines the rules for designing, adding, changing, and connecting modules in CRAI.

It does not define the complete list of CRAI modules.

The actual module catalog is maintained separately in:

```text
.meta/MODULES.md
```

All modules, including experimental modules, must follow the rules in this document unless an approved architecture decision explicitly defines an exception.

---

## 2. Module Definition

A module is a cohesive part of the system with:

* one primary responsibility;
* a clearly defined boundary;
* explicit inputs and outputs;
* explicit dependencies;
* contracts for communicating with other modules;
* independently testable behavior.

A folder is not automatically a module.

A class, package, service, plugin, or process may be part of a module without being a separate module.

Create a new module only when the responsibility requires an independent boundary.

---

## 3. User Experience and Content Type First

Module design must begin from the reading experience, content type, and use case.

CRAI currently recognizes two primary content categories:

Text Content

Examples:

web novels;
light novels;
copied text;
HTML articles;
TXT or EPUB content in future versions.

Text content should be extracted directly whenever structured text is available.

It must not be forced through screen capture or OCR merely to reuse the image pipeline.

Image Content

Examples:

manga;
manhua;
manhwa;
scanned pages;
screenshots;
image-based documents.

Image content may require:

acquisition;
change detection;
image preprocessing;
text detection;
OCR;
reading-order reconstruction;
translation;
visual presentation.

The preferred design order is:

Reading experience
    ↓
Content type
    ↓
Use case
    ↓
Processing flow
    ↓
Module responsibility
    ↓
Contract
    ↓
Technology

A technology or provider must not determine the product workflow.

---

## 4. Design Maturity

Not every module design is final.

Every module must have one of these statuses:

### Proposed

The module is being considered.

Its responsibility, contracts, or implementation may change.

### Accepted

The module boundary and responsibility have been reviewed and accepted.

Implementation may begin.

### Implemented

The module exists in the source code and has basic tests.

### Stable

The module contract is considered reliable.

Breaking changes require explicit review.

### Deprecated

The module should no longer be used for new work.

A replacement or migration path must be documented.

### Removed

The module has been removed from the active architecture.

Historical decisions may remain in documentation.

Initial CRAI module designs should normally begin as `Proposed`.

Uncertainty must be documented instead of hidden.

---

## 5. Single Primary Responsibility

Each module must have one primary responsibility.

A module may contain several internal operations, but they must support the same responsibility.

Good examples:

```text
Screen Capture
- capture a selected screen region;
- capture a window;
- capture updated frames.
```

All operations support screen acquisition.

Bad example:

```text
OCR Module
- capture screen;
- detect text;
- translate text;
- render overlay;
- save reading history.
```

This combines unrelated responsibilities and must be separated.

A module description should be expressible in one clear sentence:

> The module is responsible for ...

If the sentence contains multiple unrelated responsibilities connected by “and,” the boundary should be reviewed.

---

## 6. Module Boundaries

Every module must define:

* responsibility;
* owned data;
* accepted inputs;
* produced outputs;
* public contracts;
* allowed dependencies;
* forbidden dependencies;
* failure behavior;
* lifecycle;
* configuration;
* test strategy.

Internal implementation details must remain private unless they are intentionally part of a public contract.

Other modules must not depend on:

* private classes;
* internal folder structures;
* internal database tables;
* provider-specific response models;
* undocumented side effects.

---

## 7. Contract-First Communication

Modules communicate through explicit contracts.

A contract may be:

* an interface;
* a command;
* a query;
* an event;
* a request/response model;
* a provider abstraction;
* a documented data structure.

Prefer:

```text
Consumer
    ↓
Contract
    ↓
Implementation
```

Avoid:

```text
Consumer
    ↓
Concrete implementation
```

Contracts should describe CRAI concepts, not vendor-specific concepts.

Good:

```text
ITextRecognizer
TranslationRequest
DetectedTextRegion
TranslationResult
```

Avoid exposing names such as:

```text
GoogleVisionResponse
DeepLInternalRequest
PaddlePrivateResult
```

Provider-specific models must be converted at the provider boundary.

---

## 8. Dependency Direction

Dependencies must follow the architectural direction defined by CRAI.

The general direction is:

```text
Presentation
    ↓
Application
    ↓
Domain
```

External implementations connect through ports:

```text
Infrastructure
    ↓ implements
Application or Domain contract
```

The domain must not depend on:

* UI frameworks;
* operating-system capture APIs;
* OCR SDKs;
* translation SDKs;
* databases;
* network clients;
* vendor-specific models.

Infrastructure may depend on external libraries, but those dependencies must not leak into higher-level contracts.

Circular dependencies between modules are forbidden.

---

## 9. Capability, Language, and Provider Separation

CRAI must distinguish between:

capability;
language configuration;
provider implementation;
presentation strategy.

Examples:

Text Recognition            = capability
PaddleOCR Adapter           = provider

Translation                 = capability
Chinese → Vietnamese        = language configuration
Remote Translation Adapter  = provider

Text Layout                 = presentation capability
Novel Reader Layout         = presentation strategy

Language pairs are configuration and processing context.

They must not be modeled as provider-specific modules.

CRAI may initially prioritize:

Simplified Chinese  → Vietnamese
Traditional Chinese → Vietnamese
English             → Vietnamese

However, module contracts must use provider-neutral and language-neutral concepts such as:

SourceLanguage
TargetLanguage
LanguageProfile
TranslationRequest
TranslationResult

Core workflows must not hardcode a single language pair.

---

## 10. Plugin Eligibility

Not every replaceable implementation requires a complete plugin system.

Use the following progression:

Clear contract
    ↓
Replaceable implementation
    ↓
Provider adapter
    ↓
Runtime plugin only when justified

A runtime plugin boundary should be introduced only when at least one of the following is real:

multiple providers must be installed independently;
third parties may extend CRAI;
independent distribution is required;
optional dependencies must remain isolated;
providers must be loaded or unloaded at runtime.

For an early MVP, interfaces and provider adapters may be sufficient.

Do not build a dynamic plugin framework only for hypothetical future needs.

---

## 11. Module Size

A module should be large enough to own a meaningful responsibility and small enough to remain understandable.

Do not create modules solely because:

* a folder contains several files;
* a class is large;
* a library is used;
* a technical operation has a name;
* future reuse is imagined without evidence.

Do not combine modules solely because:

* they currently use the same library;
* they are executed consecutively;
* one developer implemented both;
* separating them requires contracts.

Module boundaries should follow responsibility and change patterns.

Components that change for different reasons should normally remain separate.

---

## 12. Data Ownership

Every persistent or long-lived data type must have a clear owner.

Only the owning module may define the authoritative representation and mutation rules for its data.

Other modules may access the data through:

* contracts;
* queries;
* immutable transfer models;
* domain events.

Avoid shared mutable state.

No module may directly modify another module’s internal state.

Examples of data requiring explicit ownership may include:

* reading session;
* captured frame metadata;
* detected text regions;
* translation cache;
* glossary;
* user settings;
* provider credentials;
* processing history.

Ownership will be assigned in `.meta/MODULES.md`.

---

## 13. Shared Models

Do not create a global shared-model package for convenience.

A model should be shared only when it represents a stable concept crossing a deliberate boundary.

Shared contracts should remain minimal.

Avoid creating universal objects such as:

```text
CommonResult
CommonData
GlobalContext
SharedManager
AppHelper
```

These names often hide unclear responsibilities and increase coupling.

Prefer specific types:

```text
CapturedFrame
TextRegion
RecognitionResult
TranslationSegment
OverlayLayout
```

---

## 14. Commands, Queries, and Events

Use a command when requesting a state-changing operation.

Examples:

```text
StartReadingSession
UpdateCaptureRegion
ClearTranslationCache
```

Use a query when requesting data without changing system state.

Examples:

```text
GetActiveReadingSession
GetCachedTranslation
GetAvailableProviders
```

Use an event to announce that something has already happened.

Examples:

```text
FrameCaptured
TextRegionsDetected
TranslationCompleted
ReadingSessionStopped
```

Events must use past-tense meaning.

Do not use events as disguised synchronous function calls.

Do not assume an event has exactly one consumer.

---

## 15. Synchronous and Asynchronous Work

Module contracts must clearly state whether an operation is synchronous or asynchronous.

Long-running operations should be asynchronous, including:

* screen capture streams;
* image preprocessing;
* OCR;
* translation over network;
* AI processing;
* disk export;
* large cache operations.

The UI thread must not execute heavy processing.

Cancellation should be supported for operations that may become irrelevant when:

* the user changes pages;
* the captured region changes;
* the source image changes;
* the reading session stops;
* a newer request replaces an older request.

Obsolete results must not overwrite newer results.

---

## 16. Processing Flows

CRAI does not have one mandatory universal pipeline.

It supports multiple processing flows that may share capabilities.

Text Translation Flow

A possible text flow is:

Text Source
    ↓
Text Extraction
    ↓
Structure Preservation
    ↓
Text Normalization
    ↓
Context Building
    ↓
Translation
    ↓
Reading Layout
    ↓
Text Presentation

This flow should preserve, where possible:

paragraphs;
dialogue boundaries;
chapter structure;
emphasis;
punctuation;
intentional line breaks.

Structured source text must not be converted into images or passed through OCR without a documented reason.

Image Translation Flow

A possible image flow is:

Image Source
    ↓
Acquisition or Capture
    ↓
Change Detection
    ↓
Image Preprocessing
    ↓
Text Detection
    ↓
Text Recognition
    ↓
Text Grouping and Reading Order
    ↓
Context Building
    ↓
Translation
    ↓
Comic Layout
    ↓
Image Presentation

The flow is provisional.

Individual stages may be replaced, combined internally, or skipped when the use case does not require them.

Shared Capabilities

The two flows may share:

language detection;
translation orchestration;
glossary;
translation memory;
caching;
reading session;
diagnostics;
provider management.

Shared capability does not imply shared presentation or identical processing contracts.

---

## 16.1 Content Conversion Rules

Content should retain its highest available structural quality.

Preference order:

Structured text
    ↓
Document text
    ↓
Image with text metadata
    ↓
Raw image
    ↓
Screen capture

Do not degrade structured text into an image unnecessarily.

Do not assume OCR output has the same quality or structure as native text.

OCR output may require:

confidence evaluation;
line merging;
paragraph reconstruction;
reading-order correction;
punctuation correction;
language-specific normalization.

Conversions between content types must be explicit and traceable.

---

## 16.2 Translation and Presentation Separation

Translation modules are responsible for producing translated linguistic content.

Translation modules must not decide final visual presentation.

They may return presentation-relevant metadata such as:

source segment identifier;
translated segment;
confidence;
detected language;
character count;
preferred break opportunities;
alignment with source regions.

Presentation modules are responsible for:

font selection;
font size;
line height;
paragraph spacing;
text alignment;
content width;
overlay position;
text wrapping;
overflow behavior;
visual accessibility.

Examples of separate presentation strategies include:

Novel Reader Layout
Comic Overlay Layout
Comic Side Panel
Comic Text Replacement
Parallel Text View

A translation provider must not contain UI-specific layout logic.

A presentation strategy must not call a translation provider directly.

---

## 16.3 Text Presentation Rules

Text-oriented reading modules should preserve readability rather than copying source HTML blindly.

A text layout capability may control:

font family;
font size;
line height;
paragraph spacing;
first-line indentation;
maximum line width;
margins;
theme;
dialogue formatting;
chapter headings.

Formatting must be separated into:

Semantic structure
    ↓
Reading layout
    ↓
Rendered appearance

Semantic structure should survive theme or font changes.

---

## 16.4 Image Presentation Rules

Image translation presentation should support multiple strategies because translated Vietnamese text may occupy more space than the source text.

Initial strategies may include:

Overlay near source region
Translation side panel
Numbered source regions with matching translated entries

Advanced strategies may include:

Source text removal
Background reconstruction
Translated text insertion
Speech-bubble-aware layout

Advanced image replacement must not be assumed as an MVP requirement.

OCR, translation, image cleanup, and text rendering must remain separate responsibilities.

Overflow handling must be explicit.

A presentation module must not silently:

reduce text below a readable size;
crop translated content;
cover important image regions without warning;
alter the source image permanently.
---

## 17. Traceability

Processing results should remain traceable across the pipeline.

Where practical, related operations should carry identifiers such as:

```text
ReadingSessionId
SourceId
FrameId
RegionId
RequestId
TraceId
```

This allows CRAI to determine:

* which capture produced an OCR result;
* which OCR result produced a translation;
* whether a result is stale;
* where an error occurred;
* whether cached data can be reused.

Identifiers must not contain vendor-specific assumptions.

---

## 18. Error Boundaries

A module must define how it reports:

* expected failures;
* recoverable failures;
* provider failures;
* invalid inputs;
* cancellation;
* timeouts;
* unavailable capabilities;
* unexpected internal errors.

Do not silently swallow failures.

Do not return `null` or an empty result when that could mean either “nothing found” or “processing failed.”

These cases must be distinguishable.

Provider errors should be translated into CRAI-level errors before crossing the module boundary.

Example:

```text
Provider-specific HTTP 429
    ↓
TranslationRateLimited
```

---

## 19. Resilience

Failure in an optional provider must not unnecessarily terminate the entire application.

Where appropriate, modules may support:

* retry;
* timeout;
* fallback provider;
* cache fallback;
* partial result;
* degraded mode;
* circuit breaking.

Resilience behavior must be intentional and documented.

Automatic retries must be bounded.

A retry must not duplicate irreversible operations.

---

## 20. Configuration

Every configurable module must declare:

* available settings;
* default values;
* validation rules;
* sensitive settings;
* whether changes require restart;
* whether settings are global or session-specific.

Modules must not read arbitrary global configuration directly.

Configuration should be supplied through a defined contract.

Credentials and secrets must not appear in:

* source code;
* logs;
* exported diagnostics;
* version-controlled configuration;
* user-visible error details.

---

## 21. Performance Responsibilities

Each module must understand its main performance risks.

Likely CRAI risks include:

* repeatedly capturing unchanged frames;
* copying large images unnecessarily;
* running OCR on identical content;
* translating duplicate text;
* blocking the UI thread;
* processing stale pages;
* keeping image buffers longer than necessary;
* unbounded queues;
* unbounded cache growth.

Performance optimization must be based on measurement.

However, obviously wasteful data movement and repeated processing should be avoided during initial design.

---

## 22. Caching

Caching is a separate responsibility and must not be hidden inside unrelated modules without documentation.

Every cache must define:

* cache key;
* cached value;
* lifetime;
* invalidation rule;
* maximum size;
* persistence behavior;
* privacy impact;
* failure behavior.

Possible cache layers may include:

* captured image fingerprint;
* OCR result;
* normalized source text;
* translation result;
* layout result.

Cache keys must include all inputs that materially affect the result.

---

## 23. Privacy and Local Processing

Modules must collect and retain only the data required for their responsibility.

When a provider sends data outside the user’s device, the boundary must be explicit.

A module using a remote provider must declare:

* what data is sent;
* why it is sent;
* where practical, whether local processing is available;
* whether credentials are required;
* whether content may be retained by the provider.

Local-first is preferred when it provides an acceptable reading experience.

Local-only must not be claimed unless the entire relevant processing path is actually local.

---

## 24. Testability

Every accepted module must have a test strategy.

Depending on responsibility, this may include:

* unit tests;
* contract tests;
* integration tests;
* provider adapter tests;
* pipeline tests;
* performance tests;
* visual comparison tests;
* manual reading-experience tests.

External providers must be replaceable with test doubles.

Tests must not require live paid services unless explicitly marked as optional integration tests.

Module behavior should be testable without starting the complete desktop UI.

---

## 25. Observability

Modules performing significant work should expose enough information for diagnosis.

Relevant information may include:

* processing duration;
* result count;
* provider used;
* cache hit or miss;
* cancellation;
* retry count;
* failure category;
* frame or request identifier.

Do not log:

* credentials;
* access tokens;
* full sensitive images by default;
* private translated content without explicit diagnostic consent.

Logging must help identify the failing stage without exposing unnecessary user data.

---

## 26. Module Lifecycle

A module with runtime resources must define its lifecycle.

Typical lifecycle:

```text
Register
    ↓
Configure
    ↓
Validate
    ↓
Initialize
    ↓
Start
    ↓
Stop
    ↓
Dispose
```

Not every module requires every stage.

Modules must release:

* image buffers;
* file handles;
* network clients when appropriate;
* subscriptions;
* background workers;
* native resources;
* temporary files.

Starting and stopping a reading session must not leak resources.

---

## 27. Public Contract Changes

Public module contracts should remain stable after reaching `Stable` status.

A breaking change includes:

* removing a contract member;
* changing input or output meaning;
* changing error semantics;
* changing lifecycle expectations;
* changing data ownership;
* changing event meaning.

Breaking changes require:

1. documented reason;
2. affected modules;
3. migration plan;
4. tests;
5. version or compatibility consideration.

During the `Proposed` stage, contracts may change more freely, but changes should still be recorded.

---

## 28. Adding a New Module

Before adding a module, document:

```text
Name:
Status:
Responsibility:
Reason for existence:
Owned data:
Inputs:
Outputs:
Public contracts:
Dependencies:
Forbidden dependencies:
Lifecycle:
Failure behavior:
Configuration:
Privacy considerations:
Performance risks:
Test strategy:
Open questions:
```

A module should not be added merely because its name sounds architecturally useful.

Its independent responsibility must be demonstrated.

---

## 29. Changing a Module

Before significantly changing an accepted module, determine:

* whether its primary responsibility changes;
* whether contracts change;
* whether data ownership changes;
* whether another module is affected;
* whether the change creates a circular dependency;
* whether documentation must be updated;
* whether an ADR is required.

Small internal implementation changes do not require architecture approval when they preserve the module contract and project rules.

---

## 30. Splitting a Module

Consider splitting a module when:

* it has multiple independent reasons to change;
* it owns unrelated data;
* one part needs a different lifecycle;
* one part requires optional external dependencies;
* one part should be replaceable independently;
* testing requires starting unrelated behavior;
* module responsibilities cannot be described clearly.

Do not split solely to reduce file count.

---

## 31. Merging Modules

Consider merging modules when:

* neither has a meaningful independent responsibility;
* contracts create ceremony without isolation value;
* they always change together for the same reason;
* they own one inseparable lifecycle;
* separation provides no replacement, testing, or deployment benefit.

Avoid maintaining artificial boundaries.

---

## 32. Naming Rules

Module names must describe capabilities or domain responsibilities.

Prefer nouns or clear noun phrases:

```text
Capture
TextRecognition
Translation
ReadingSession
Overlay
Glossary
```

Avoid vague names:

```text
Common
Utils
Helper
Manager
Processor
Service
Engine
Core2
Misc
```

Words such as `Manager`, `Processor`, `Service`, or `Engine` may be used only when they clearly describe a recognized responsibility.

Provider implementations should include the provider or technology name:

```text
PaddleOcrProvider
WindowsCaptureProvider
GoogleTranslationProvider
```

Contracts should remain provider-neutral:

```text
ITextRecognizer
IScreenCapture
ITranslator
```

Final naming conventions may be adjusted after the implementation language and repository structure are confirmed.

---

## 33. Documentation Ownership

Every module must have one authoritative documentation location.

The module catalog is:

```text
.meta/MODULES.md
```

Detailed designs may be stored under:

```text
docs/modules/<module-name>/
```

A module document should not duplicate project-wide rules from this file.

It should reference this file and document only module-specific decisions.

---

## 34. Architectural Decisions

A decision should be recorded as an ADR when it:

* introduces or removes a module;
* changes dependency direction;
* changes data ownership;
* selects a major provider strategy;
* changes local versus cloud processing;
* introduces a plugin boundary;
* changes a major pipeline stage;
* creates a significant long-term constraint.

Suggested location:

```text
docs/adr/
```

Temporary exploration does not require an ADR until a direction is accepted.

---

## 35. AI Rules for Module Design

When assisting with module design, AI must:

- begin from documented reading needs;
- identify assumptions;
- distinguish text and image workflows;
- mark uncertain designs as Proposed;
- explain why each boundary exists;
- avoid unnecessary modules;
- distinguish capabilities, language configuration, providers, and presentation;
- show inputs, outputs, dependencies, and owned data;
- identify privacy and performance implications;
- prefer reversible decisions during discovery;
- update relevant documentation when decisions change.

AI must not present an unverified OCR, translation, or language-processing design as unquestionably correct.

When domain knowledge or user requirements are incomplete, AI should:

- state the uncertainty;
- make the smallest safe assumption;
- mark the design as Proposed;
- choose a reversible boundary;
- identify what should be researched or prototyped;
- continue when uncertainty does not block safe progress.

AI should ask for clarification before proceeding only when a decision:

- is difficult to reverse;
- causes significant cost;
- introduces security or privacy risk;
- changes the accepted project scope;
- creates a long-term external dependency;
- conflicts with an explicit user requirement.

---

## 36. Decision Priority

For module-design conflicts, use this priority:

```text
Explicit user requirement
    ↓
Real reading experience
    ↓
Accepted project scope
    ↓
Accepted architecture decisions
    ↓
PROJECT_RULE.md
    ↓
MODULES_RULE.md
    ↓
Implementation convenience
    ↓
AI preference
```

A lower-priority concern must not silently override a higher-priority concern.

When a user-experience requirement conflicts with a technical rule, the conflict must be discussed and documented rather than ignored.

---

## 37. Initial CRAI Design Policy

CRAI is currently in the architecture and discovery stage.

Current product assumptions include:

- Vietnamese is the primary target language.
- Simplified Chinese is a primary source language.
- Traditional Chinese and English should also be supported.
- Text content and image content require separate processing flows.
- Text reading requires dedicated typography and reading layout.
- Comic translation initially favors non-destructive presentation.
- Direct source-text extraction is preferred over OCR when available.
- Screen translation is a possible MVP interaction model.
- Browser integration and imported-content reading remain proposed options.

These assumptions are not permanent architecture laws.

They must be validated through:

- user-experience discussion;
- technical research;
- prototypes;
- real reading tests.

Changing an incorrect proposal is expected and should be documented.

---

## 38. Completion Checklist

A module proposal is ready for review when:

* [ ] Its responsibility is clear.
* [ ] Its reason for existence is documented.
* [ ] Its inputs and outputs are defined.
* [ ] Its owned data is identified.
* [ ] Its dependencies are listed.
* [ ] Forbidden dependencies are listed.
* [ ] Capability and provider concerns are separated.
* [ ] Error behavior is defined.
* [ ] Cancellation requirements are considered.
* [ ] Privacy implications are considered.
* [ ] Performance risks are identified.
* [ ] Its test strategy is described.
* [ ] Open questions are visible.
* [ ] Its status is marked.
* [ ] It follows `PROJECT_RULE.md`.
