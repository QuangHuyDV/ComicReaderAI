# Text Processing Module

> **Project:** CRAI
> **Module:** Text Processing
> **Document:** Module Overview
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-07-25

---

## 1. Purpose

The Text Processing module transforms extracted source text into coherent, ordered, and translation-ready text units.

Its main responsibility is to bridge the gap between raw extraction output and the Translation module.

Raw text received from OCR, browser extraction, clipboard input, or imported documents may contain:

* incorrect line breaks;
* duplicated whitespace;
* OCR artifacts;
* fragmented sentences;
* uncertain reading order;
* detached dialogue lines;
* inconsistent punctuation;
* missing structural relationships;
* text segments that are too small or too large for useful translation.

The Text Processing module normalizes and restructures this content while preserving traceability back to the original source.

```text
Extracted source text
        ↓
Validation
        ↓
Normalization
        ↓
Structure reconstruction
        ↓
Translation-unit preparation
        ↓
Context construction
        ↓
Prepared text
```

This module does not translate source text and does not decide how translated content is displayed.

---

## 2. Module Position

The module sits between Content Extraction and Translation.

```text
Content Acquisition
        ↓
Content Observation
        ↓
Content Classification
        ↓
Content Extraction
        ↓
Text Processing
        ↓
Translation
        ↓
Presentation
```

For image-based content:

```text
Captured image
    ↓
Text-region detection
    ↓
OCR recognition
    ↓
Extracted text regions
    ↓
Text Processing
    ↓
Prepared translation segments
```

For structured text:

```text
Browser or document text
    ↓
Extracted paragraphs and metadata
    ↓
Text Processing
    ↓
Prepared translation segments
```

The module supports both flows through a shared processing model while allowing source-specific rules.

---

## 3. Core Responsibility

The Text Processing module is responsible for:

* validating extracted text input;
* preserving source identifiers and traceability;
* normalizing Unicode, whitespace, and punctuation;
* cleaning supported OCR artifacts;
* preserving or correcting reading order;
* merging lines that belong to the same semantic unit;
* splitting text into translation-ready segments;
* reconstructing paragraph and dialogue structure;
* identifying structural hints useful to translation;
* building local context between related segments;
* producing deterministic, versioned processing results;
* reporting warnings when useful output can still be produced;
* rejecting stale or superseded processing results.

The module should improve the structure and usability of source text without silently changing its intended meaning.

---

## 4. Explicit Non-Responsibilities

The Text Processing module must not:

* capture screens or application windows;
* monitor source changes;
* perform image preprocessing;
* detect image text regions;
* perform OCR recognition;
* call translation providers;
* select translation providers;
* translate source text;
* render side panels or overlays;
* replace text inside images;
* persist user glossaries as the system of record;
* own the reading-session lifecycle;
* decide whether a frame or page is still current;
* permanently modify source content.

These responsibilities belong to neighboring modules.

---

## 5. Inputs

The module accepts extracted textual content and structural metadata.

Typical sources include:

* OCR-recognized image regions;
* browser DOM text;
* clipboard text;
* imported document text;
* user-corrected OCR text;
* manually reordered source segments.

An input should normally contain:

```text
Processing request identity
Source identity
Content revision
Ordered or partially ordered source units
Source-language hints
Structural metadata
Geometric metadata when available
Processing options
Trace metadata
```

Image-derived input may additionally contain:

* region identifiers;
* bounding boxes;
* OCR confidence;
* detected reading direction;
* bubble or panel relationships;
* page-relative order;
* orientation information.

Structured-text input may additionally contain:

* paragraph identifiers;
* headings;
* dialogue boundaries;
* DOM or document order;
* chapter metadata;
* formatting hints.

The exact input models are defined in:

```text
modules/text-processing/CONTRACTS.md
```

---

## 6. Outputs

The primary output is prepared source text suitable for translation.

A successful result may contain:

* normalized source text;
* ordered prepared segments;
* paragraph or dialogue grouping;
* source-to-output mappings;
* local context relationships;
* structural hints;
* source-language metadata;
* processing warnings;
* processing trace information;
* input and output revision information.

Each prepared segment should remain traceable to one or more original extracted units.

```text
ExtractedUnit A ─┐
                 ├── PreparedSegment 1
ExtractedUnit B ─┘

ExtractedUnit C ──── PreparedSegment 2
```

The module must not remove this relationship merely because several lines are merged or one source unit is split.

The exact output models are defined in:

```text
modules/text-processing/CONTRACTS.md
```

---

## 7. Main Processing Stages

### 7.1 Validation

Validation determines whether the input is processable.

Typical checks include:

* required identifiers are present;
* the input revision is valid;
* source units have stable identifiers;
* source text is represented correctly;
* declared ordering is internally consistent;
* processing options are supported;
* the request is not already invalidated.

Validation should distinguish between:

* fatal invalid input;
* recoverable structural uncertainty;
* empty but valid input;
* low-confidence input that may continue with warnings.

---

### 7.2 Normalization

Normalization creates a consistent textual representation without intentionally altering meaning.

Typical operations include:

* Unicode normalization;
* whitespace normalization;
* line-ending normalization;
* punctuation normalization;
* removal of unsupported control characters;
* preservation of meaningful paragraph boundaries;
* normalization of language-specific punctuation;
* conservative cleanup of known OCR artifacts.

Normalization must be deterministic for the same input revision and configuration.

It should avoid aggressive correction when the intended source text is uncertain.

---

### 7.3 Structure Reconstruction

Structure reconstruction converts fragmented extraction output into useful reading units.

Possible operations include:

* line merging;
* sentence segmentation;
* paragraph reconstruction;
* dialogue grouping;
* reading-order correction;
* region grouping;
* heading preservation;
* separation of dialogue and non-dialogue text;
* retention of uncertainty markers.

Structure reconstruction may use different strategies for:

```text
Comic dialogue
Continuous prose
Browser paragraphs
Document text
Short interface text
Mixed content
```

Source-specific behavior should be implemented through processing policies or profiles rather than separate incompatible module contracts.

---

### 7.4 Translation-Unit Preparation

The module prepares segments that can be translated independently or in related batches.

A translation unit should balance:

* semantic completeness;
* available context;
* provider request limits;
* stable source alignment;
* reading order;
* presentation alignment;
* latency.

A segment should not be split solely to match individual OCR lines when those lines belong to the same sentence.

A segment should also not grow so large that translation alignment and progressive presentation become impractical.

---

### 7.5 Context Construction

Context construction provides relationships that help the Translation module interpret each segment.

Context may include:

* previous segment;
* next segment;
* current paragraph;
* nearby comic regions;
* current dialogue group;
* chapter or page hints;
* detected names or terms;
* source-language hints;
* style or content-type hints.

The Text Processing module prepares structural context.

It does not own the persistent glossary, translation memory, or final provider prompt.

---

### 7.6 Finalization

Finalization:

* verifies output invariants;
* assigns output revisions;
* produces source mappings;
* records warnings;
* confirms the request is still authoritative;
* prepares the final result for publication;
* emits the appropriate completion event.

A result that has been cancelled or superseded must not become authoritative during finalization.

---

## 8. State Model

A processing request is represented by a state-owning job.

The normal successful lifecycle is:

```text
CREATED
    ↓
QUEUED
    ↓
VALIDATING
    ↓
NORMALIZING
    ↓
STRUCTURING
    ↓
BUILDING_CONTEXT
    ↓
FINALIZING
    ↓
COMPLETED
```

Additional lifecycle states support cancellation, failure, and stale-work rejection:

```text
CANCEL_REQUESTED
CANCELLED
SUPERSEDED
FAILED
```

Terminal states are:

```text
COMPLETED
CANCELLED
SUPERSEDED
FAILED
```

A job must enter only one terminal state.

The full state definitions, transition rules, cancellation semantics, retry behavior, and invariants are documented in:

```text
modules/text-processing/STATES.md
```

---

## 9. Cancellation and Supersession

Text processing may run while the user scrolls, changes pages, corrects OCR text, or switches reading sources.

Therefore, the module must support stale-work rejection.

### Cancellation

Cancellation means the active job should stop because its result is no longer required.

Examples:

* the reading session stopped;
* the user paused processing;
* the source was closed;
* the caller explicitly cancelled the request.

Cancellation may be cooperative if the current operation cannot stop immediately.

A cancellation request must suppress result publication even if internal processing finishes shortly afterward.

### Supersession

Supersession means a newer content revision has replaced the job's input revision.

Examples:

* a new frame replaced the previous frame;
* the user corrected recognized source text;
* reading order was manually changed;
* a newer extraction result became authoritative.

A superseded result must never replace a result produced from a newer revision.

```text
Revision 12 processing
        ↓
Revision 13 becomes current
        ↓
Revision 12 becomes SUPERSEDED
        ↓
Revision 12 output is not published
```

The Reading Session or orchestration layer determines which content revision is current.

The Text Processing module enforces the supplied revision and authority rules.

---

## 10. Partial Results and Warnings

The module may complete successfully while reporting warnings.

Warnings are appropriate when:

* OCR confidence is low;
* reading order is uncertain;
* some text units are empty;
* some lines could not be merged confidently;
* paragraph reconstruction is incomplete;
* an unsupported structural hint was ignored;
* input metadata is incomplete but processing can continue;
* one source unit is preserved without normalization because correction is unsafe.

Warnings must not be used to hide invariant violations or corrupted output.

A processing result can therefore be:

```text
Completed without warnings
Completed with warnings
Failed
Cancelled
Superseded
```

Partial successful output is acceptable when:

* useful segments remain available;
* failed source units are explicitly identified;
* source mappings remain valid;
* the caller can distinguish processed and unprocessed content;
* the result does not imply full completeness.

---

## 11. Error Model

Errors should be structured and machine-readable.

An error should normally identify:

* error code;
* processing job;
* processing stage;
* affected source unit when applicable;
* retry classification;
* user-facing safety level;
* internal diagnostic details;
* original cause when available.

Errors should distinguish:

```text
Invalid input
Unsupported input
Invariant violation
Processing timeout
Cancellation
Supersession
Internal processing failure
Configuration failure
Resource exhaustion
```

Expected user-content problems should not automatically be treated as internal system failures.

The canonical error definitions are documented in:

```text
modules/text-processing/ERRORS.md
```

---

## 12. Events

The module communicates lifecycle changes and completed results through the CRAI event bus.

Typical events include:

* job created;
* job queued;
* processing started;
* processing stage changed;
* warning recorded;
* job completed;
* cancellation requested;
* job cancelled;
* job superseded;
* job failed.

Events must contain enough identity information to support:

* trace correlation;
* stale-result rejection;
* reading-session association;
* diagnostics;
* UI progress updates;
* downstream translation requests.

State-transition events represent facts that have already occurred.

They must not be used as disguised commands.

The canonical event names and payload contracts are documented in:

```text
modules/text-processing/EVENTS.md
```

---

## 13. Commands and Public Operations

The module may expose operations equivalent to:

```text
PrepareText
CancelTextProcessing
SupersedeTextProcessing
RetryTextProcessing
```

The exact command names depend on the final application boundary, but their semantics must remain explicit.

### Prepare Text

Creates or schedules a processing job for one immutable input revision.

### Cancel Text Processing

Requests cancellation of an active job.

### Supersede Text Processing

Marks a job obsolete because a newer authoritative revision exists.

This may be an explicit command or an orchestration action depending on the final runtime design.

### Retry Text Processing

Creates a new attempt according to the retry rules.

A retry must not silently reuse a mutated input snapshot.

---

## 14. Data Ownership

The Text Processing module owns:

* processing-job lifecycle;
* immutable processing input snapshots;
* temporary intermediate processing state;
* prepared text results;
* source-to-prepared-segment mappings;
* processing warnings;
* module-specific diagnostic metadata.

The module does not own:

* raw screenshots as a permanent source of truth;
* OCR provider configuration;
* translated text;
* user glossary records;
* translation memory;
* reading-session authority;
* presentation layout;
* permanent content libraries.

When persistent storage is used, stored data must clearly distinguish:

```text
Temporary processing data
Reusable derived processing result
User-created correction
External source content
Diagnostic trace
```

---

## 15. Relationship with Neighboring Modules

### 15.1 Content Extraction

Content Extraction produces recognized or extracted source text.

It owns:

* OCR recognition;
* DOM or document extraction;
* detected text regions;
* extraction confidence;
* extraction-specific metadata.

Text Processing consumes those results but does not own the extraction mechanism.

```text
Content Extraction
    produces extracted units
        ↓
Text Processing
    produces prepared segments
```

---

### 15.2 Translation

Translation consumes prepared text.

It owns:

* provider selection;
* translation requests;
* provider retries;
* translated text;
* language-pair execution;
* translation-specific alignment;
* translation confidence;
* translation-provider errors.

Text Processing may provide context hints but must not call a provider directly.

```text
Prepared text result
        ↓
Translation request
        ↓
Translated segments
```

---

### 15.3 Knowledge

The Knowledge module owns reusable names, terms, corrections, and translation memory.

Text Processing may:

* detect likely terms;
* attach term candidates;
* request relevant contextual knowledge through an allowed interface;
* preserve user-corrected source text.

It must not silently persist inferred knowledge as authoritative user preference.

---

### 15.4 Reading Session

Reading Session owns the currently active content and revision.

It determines whether a processing result still belongs to:

* the active frame;
* the active page;
* the active chapter;
* the active source;
* the current user correction revision.

Text Processing must retain the session and revision identifiers needed for this decision.

---

### 15.5 Presentation

Presentation consumes translated and aligned content.

Text Processing may preserve:

* source-region identifiers;
* paragraph identifiers;
* ordering;
* grouping;
* geometry references.

It must not decide:

* font;
* side-panel layout;
* overlay placement;
* translated text wrapping;
* image modification.

---

### 15.6 Event Bus

The Event Bus transports commands, state facts, warnings, and results between modules.

Text Processing must not depend on event delivery alone for internal correctness.

Internal invariants must remain valid even when:

* an event is delivered more than once;
* an event is delayed;
* subscribers restart;
* consumers process events asynchronously.

---

## 16. Idempotency

Repeated processing requests may occur because of:

* event redelivery;
* caller retries;
* reconnects;
* duplicate extraction results;
* UI actions;
* application restart.

The module must support idempotent request handling.

An idempotency decision should consider:

```text
Request identity
Input content revision
Processing configuration revision
Language profile
Source-unit identities
```

Two requests must not be considered identical merely because their visible text is equal.

Different source mappings, revisions, or processing options may require different results.

---

## 17. Concurrency

Independent jobs may run concurrently.

However:

* one job processes one immutable input revision;
* mutation of a job input after creation is forbidden;
* output publication must verify authority;
* duplicate processing should be deduplicated when safe;
* cancellation and supersession must be race-safe;
* one job must not mutate another job's intermediate state;
* ordering inside one result must be deterministic.

Parallel processing of independent source units is allowed when final ordering and grouping remain deterministic.

---

## 18. Language Profiles

The core module must remain language-neutral.

Language-specific behavior should be introduced through profiles or policies.

Initial profiles may include:

```text
Simplified Chinese
Traditional Chinese
English
```

Chinese-focused processing may need to account for:

* Chinese punctuation;
* vertical text;
* sentences split across multiple comic bubbles;
* missing spaces between words;
* simplified and traditional character variants;
* context-dependent pronouns;
* proper-name candidates;
* cultivation, fantasy, and historical terminology.

English-focused processing may need to account for:

* hyphenation across extracted lines;
* sentence punctuation;
* paragraph reconstruction;
* contractions;
* dialogue quotation styles.

Language profiles must not turn the core module into separate incompatible Chinese and English implementations.

---

## 19. Processing Profiles

Processing behavior may also vary by content type.

Possible profiles include:

```text
COMIC_DIALOGUE
COMIC_MIXED_TEXT
NOVEL_PROSE
STRUCTURED_WEB_TEXT
DOCUMENT_TEXT
SHORT_TEXT
```

A profile may influence:

* line-merging thresholds;
* segmentation rules;
* paragraph reconstruction;
* context-window size;
* reading-order assumptions;
* punctuation cleanup;
* warning thresholds.

Profiles should remain configuration or strategy selections rather than public contract fragmentation.

---

## 20. Privacy and Security

Text Processing may handle copyrighted, private, or sensitive reading content.

The module should:

* avoid permanent storage of raw content by default;
* keep temporary data scoped to the active job;
* avoid logging full source text unless explicitly enabled for diagnostics;
* redact or truncate content in normal error logs;
* prevent cross-session data leakage;
* prevent one user's text from entering another user's context;
* preserve clear deletion boundaries;
* treat imported and captured content as user-controlled data.

Any external transmission belongs to modules that call remote providers, not to Text Processing itself.

---

## 21. Observability

The module should expose diagnostics for:

* total processing duration;
* time spent in each processing stage;
* number of input units;
* number of prepared output segments;
* merged-unit count;
* split-unit count;
* warning count;
* retry count;
* cancellation latency;
* supersession count;
* failure category;
* selected language and processing profiles;
* input and output revisions.

Diagnostic data should support tracing relationships such as:

```text
ReadingSessionId
    ↓
ContentRevision
    ↓
ExtractionResultId
    ↓
TextProcessingJobId
    ↓
PreparedSegmentId
    ↓
TranslationRequestId
```

Full source text should not be required for normal observability.

---

## 22. Testing Strategy

### 22.1 Unit Tests

Unit tests should cover:

* Unicode normalization;
* whitespace handling;
* punctuation rules;
* line merging;
* sentence segmentation;
* source mapping;
* deterministic ordering;
* warning generation;
* empty-input behavior;
* cancellation checks;
* supersession checks;
* state-transition validation.

### 22.2 Contract Tests

Contract tests should verify compatibility with:

* Content Extraction output;
* Translation input;
* Event Bus payloads;
* Reading Session revision identifiers.

### 22.3 Golden Dataset Tests

Representative test data should include:

* clear horizontal Chinese comic text;
* vertical Chinese comic text;
* stylized comic text;
* fragmented speech-bubble text;
* English prose;
* Chinese web-novel paragraphs;
* malformed OCR output;
* mixed punctuation;
* duplicate lines;
* missing lines;
* uncertain reading order;
* user-corrected source text.

Golden tests should compare both output text and source mappings.

### 22.4 State Tests

State tests should verify:

* valid transitions;
* invalid transition rejection;
* one terminal state only;
* cancellation during each active stage;
* supersession during each active stage;
* publication suppression after cancellation;
* publication suppression after supersession;
* failure and retry behavior.

---

## 23. MVP Scope

The MVP should include:

* extracted-unit validation;
* Unicode normalization;
* whitespace and punctuation normalization;
* basic OCR artifact cleanup;
* deterministic reading-order preservation;
* basic line merging;
* translation-oriented segmentation;
* simple dialogue or region grouping;
* local context construction;
* source-to-output traceability;
* warnings;
* cancellation;
* stale-result rejection;
* basic diagnostics.

The MVP does not require:

* automatic speaker attribution;
* advanced linguistic parsing;
* semantic rewriting;
* long-term chapter understanding;
* automatic glossary learning;
* complex named-entity recognition;
* full grammar correction;
* model-based OCR repair;
* universal support for every comic layout;
* permanent processing history;
* cloud-based processing coordination.

---

## 24. Important Invariants

The following rules must always hold:

1. One job processes one immutable input revision.

2. Every authoritative output belongs to a known processing job.

3. Every prepared segment remains traceable to its source units.

4. Processing must not silently translate source text.

5. Processing must not silently invent missing source content.

6. Meaning-changing correction must not be presented as normalization.

7. A cancelled job must not publish an authoritative result.

8. A superseded job must not publish an authoritative result.

9. A job enters only one terminal state.

10. Output ordering must be deterministic for the same input and configuration.

11. Warnings must remain distinguishable from failures.

12. Partial results must explicitly identify missing or unresolved units.

13. Module contracts must remain independent from a specific OCR or translation provider.

14. Source-specific behavior must not break the shared output contract.

15. New processing rules must preserve backward traceability.

---

## 25. Recommended Internal Structure

The eventual implementation may use an internal structure similar to:

```text
text-processing/
├── application/
│   ├── commands/
│   ├── handlers/
│   └── queries/
├── domain/
│   ├── job/
│   ├── segment/
│   ├── mapping/
│   ├── warning/
│   └── policy/
├── processing/
│   ├── validation/
│   ├── normalization/
│   ├── structuring/
│   ├── segmentation/
│   └── context/
├── profiles/
│   ├── language/
│   └── content/
├── infrastructure/
│   ├── persistence/
│   ├── event-bus/
│   └── diagnostics/
└── tests/
```

This is an implementation suggestion, not a required source-code layout.

The final structure must follow the project module rules and should not be fixed before implementation needs are validated.

---

## 26. Documentation Map

The Text Processing module documentation is organized as follows:

```text
modules/text-processing/
├── README.md
├── CONTRACTS.md
├── EVENTS.md
├── ERRORS.md
└── STATES.md
```

### README.md

Defines:

* module purpose;
* boundary;
* responsibilities;
* processing flow;
* relationships;
* MVP scope;
* important invariants.

### CONTRACTS.md

Defines:

* commands;
* inputs;
* outputs;
* data models;
* identifiers;
* mappings;
* compatibility rules.

### EVENTS.md

Defines:

* event names;
* event payloads;
* producers;
* consumers;
* delivery and idempotency expectations.

### ERRORS.md

Defines:

* canonical error codes;
* recoverability;
* retry classification;
* user-safe error handling;
* diagnostic requirements.

### STATES.md

Defines:

* processing-job states;
* state transitions;
* cancellation;
* supersession;
* retries;
* terminal-state rules;
* state invariants.

When names or structures differ between documents, `CONTRACTS.md` is the canonical source for public data-contract names, while `STATES.md` is canonical for lifecycle semantics.

---

## 27. Open Decisions

The following decisions should remain open until representative prototypes are available:

* How aggressively should OCR artifacts be corrected?
* Should uncertain OCR corrections produce alternative text candidates?
* What line-merging strategy works best for Chinese comic bubbles?
* How should vertical and horizontal regions be grouped on mixed pages?
* How large should translation context windows be?
* Should context be attached per segment or per translation batch?
* Which text-processing rules belong in language profiles?
* Which rules belong in content-type profiles?
* Should processing results be cached independently from extraction results?
* How should user OCR corrections invalidate prior processing jobs?
* When is partial completion more useful than complete failure?
* Should model-assisted text cleanup be introduced after the deterministic MVP?
* How should long novel chapters be processed incrementally?
* How much diagnostic source text may be retained safely?

These decisions should be resolved through representative comic and novel test data rather than assumptions.

---

## 28. Related Documents

```text
.meta/AI_BOOT.md
.meta/PROJECT_RULE.md
.meta/MODULES.md
.meta/MODULES_RULE.md

docs/architecture/CAPABILITY_MAP.md
docs/architecture/STATE_MACHINE.md
docs/architecture/EVENT_BUS.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md

modules/content-extraction/README.md
modules/translation/README.md
modules/knowledge/README.md
modules/reading-session/README.md
modules/presentation/README.md

modules/text-processing/CONTRACTS.md
modules/text-processing/EVENTS.md
modules/text-processing/ERRORS.md
modules/text-processing/STATES.md
```

---

## 29. Summary

The Text Processing module converts extracted text into stable, structured, translation-ready content.

Its central boundary is:

```text
Raw extracted text
        ↓
Text Processing
        ↓
Prepared source text
```

It owns normalization, structural reconstruction, segmentation, context preparation, source mappings, warnings, and the lifecycle of text-processing jobs.

It does not own extraction, translation, reading-session authority, persistent knowledge, or presentation.

The module must prioritize:

* conservative meaning preservation;
* deterministic output;
* source traceability;
* cancellation;
* stale-result rejection;
* clear warnings;
* compatibility between image and structured-text flows.
