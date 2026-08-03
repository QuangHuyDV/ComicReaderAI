# Translation Module

> **Project:** CRAI
> **Module:** Translation
> **Path:** `modules/translation/README.md`
> **Version:** 0.2
> **Status:** Architecture Draft
> **Last Updated:** 2026-08-03

---

## 1. Overview

Translation is the CRAI module responsible for converting prepared source content into structurally aligned translated content.

The module supports translation for:

* novels;
* comics;
* web page text;
* OCR-derived image text;
* future imported reading content.

The initial language priorities are:

```text
Chinese → Vietnamese
English → Vietnamese
```

The architecture remains language-neutral and must not hardcode these language pairs into its core domain model.

---

## 2. Module Position

Translation runs after source content has been acquired, extracted and normalized.

```text
Source
      ↓
Observation
      ↓
Recognition
      ↓
Text Processing
      ↓
Translation
      ↓
Presentation
```

Translation consumes prepared content from Text Processing.

Translation does not directly consume:

* raw page HTML;
* raw browser DOM;
* screenshots;
* source images;
* raw OCR output;
* unnormalized extracted text.

---

## 3. Core Flow

The primary Translation flow is:

```text
PreparedDocument
        ↓
StartTranslation
        ↓
TranslationJob
        ↓
TranslationBatch[]
        ↓
TranslationAttempt[]
        ↓
Provider Adapter
        ↓
TranslatedSegment[]
        ↓
TranslationResultSnapshot
        ↓
TranslationVariant
        ↓
Reading Session acceptance
        ↓
Presentation
```

---

## 4. Main Architectural Concepts

Translation distinguishes six core concepts.

### PreparedSegment

```text
PreparedSegment
    = source alignment unit
```

A prepared segment is the smallest stable source unit whose translated result must remain traceable.

Examples:

* one novel paragraph;
* one dialogue line;
* one comic speech bubble;
* one narration box;
* one caption;
* one sound effect.

Prepared segments are owned by Text Processing.

---

### TranslationJob

```text
TranslationJob
    = one logical translation intent
```

A job records an immutable combination of:

* prepared document;
* source revision;
* selected segments;
* source language;
* target language;
* translation profile;
* context revision;
* terminology revision;
* provider policy;
* publication policy.

A material change in translation intent creates a new job.

Examples:

* changing the target language;
* changing from natural to literal translation;
* translating a newer source revision;
* retranslating with updated terminology.

---

### TranslationAttempt

```text
TranslationAttempt
    = one execution attempt for one translation batch
```

Automatic retry and provider fallback create new attempts.

They do not automatically create new translation jobs.

```text
TranslationJob
      └── TranslationBatch
              ├── Attempt 1
              ├── Attempt 2
              └── Attempt 3
```

---

### TranslationBatch

```text
TranslationBatch
    = provider execution unit
```

A batch contains one or more prepared segments translated together.

Batching improves:

* contextual consistency;
* dialogue continuity;
* name consistency;
* provider request efficiency;
* latency;
* cost.

Batch membership never replaces prepared segment identity.

---

### TranslationResult

```text
TranslationResult
    = assembled aligned output
```

A result snapshot contains:

* translated segments;
* missing segment identities;
* failed segment identities;
* warnings;
* source revision information;
* `TranslationRevision`;
* usage and execution statistics;
* acceptance eligibility and authority metadata.

A result may exist without being authoritative.

---

### TranslationVariant

```text
TranslationVariant
    = immutable translation version
```

Examples:

* natural translation;
* literal translation;
* fallback-provider translation;
* user-corrected translation;
* terminology-updated translation.

Published variants are never edited in place.

A correction creates a new variant.

---

## 5. Responsibilities

Translation owns:

* translation job lifecycle;
* translation batch lifecycle;
* translation attempt lifecycle within each batch;
* provider-neutral execution requirements;
* translation-specific provider selection intent;
* retry eligibility;
* fallback eligibility;
* translation context assembly;
* terminology application;
* output parsing;
* output validation;
* source-to-result alignment;
* partial result assembly;
* final result assembly;
* result compatibility and acceptance eligibility checks;
* translation variants;
* translation cache coordination;
* normalized translation errors;
* translation events;
* translation observability metadata.

Runtime owns scheduling, queue admission, worker execution, retry timing, backoff, execution budgets, resource admission and physical cancellation mechanics. Translation supplies domain policy and execution requirements but does not implement a private scheduler.

---

## 6. Non-Responsibilities

Translation does not own:

* browser navigation;
* web page acquisition;
* image downloading;
* DOM extraction;
* OCR;
* source-region detection;
* reading-order detection;
* text normalization;
* prepared segment creation;
* glossary persistence;
* character database persistence;
* provider credential storage;
* font selection;
* text layout;
* speech-bubble resizing;
* image editing;
* overlay rendering;
* reading-session navigation.

These responsibilities belong to other CRAI modules.

---

## 7. Text and Image Translation

CRAI does not maintain separate translation engines for text and images.

The two pipelines merge after Text Processing.

### Text source

```text
Web page or document
        ↓
Source / Observation
        ↓
Text Processing
        ↓
PreparedDocument
        ↓
Translation
```

### Image source

```text
Image or comic page
        ↓
Observation
        ↓
Recognition
        ↓
Text Processing
        ↓
PreparedDocument
        ↓
Translation
```

Translation receives the same prepared contract in both cases.

Source-specific metadata may remain attached to prepared segments for Presentation.

---

## 8. Novel and Comic Translation

The Translation core remains shared, but translation profiles may apply different policies.

### Novel translation

Typical priorities:

* paragraph continuity;
* narrative flow;
* consistent names;
* consistent pronouns;
* long-range context;
* natural Vietnamese prose.

Recommended profile:

```text
NOVEL_NATURAL
```

---

### Comic translation

Typical priorities:

* speech-bubble alignment;
* concise dialogue;
* speaker consistency;
* neighboring-dialogue context;
* sound-effect handling;
* limited output length.

Recommended profile:

```text
COMIC_NATURAL
```

Translation does not perform visual text fitting.

Presentation owns:

* font size;
* line breaks;
* bubble fitting;
* text overlay;
* image composition.

---

## 9. Translation Profiles

Initial profile identifiers:

```text
NOVEL_NATURAL
COMIC_NATURAL
GENERAL_NATURAL
LITERAL
CUSTOM
```

Profiles express translation intent.

They must not expose provider-native prompt templates through public contracts.

---

## 10. Context Model

Translation distinguishes:

```text
Translatable segments
```

from:

```text
Context-only segments
```

Context-only segments improve translation quality but do not produce new public translated segments.

Possible context sources include:

* preceding segments;
* following segments;
* dialogue groups;
* paragraph groups;
* chapter summaries;
* previous translations;
* character information;
* names and aliases;
* relationships;
* terminology;
* reading-session context.

---

## 11. Knowledge Integration

Translation consumes revisioned Knowledge data such as:

```text
KnowledgeSnapshotId
GlossaryRevision
TermConstraint[]
CharacterContext[]
```

Knowledge owns:

* glossary persistence;
* character information;
* aliases;
* terminology history;
* relationship data.

Translation records which Knowledge revision was used.

An update to Knowledge does not mutate an existing translation job or variant.

It may create a reason for retranslation.

---

## 12. Provider Architecture

Translation uses provider adapters.

```text
TranslationBatch
        ↓
Provider-neutral request
        ↓
Provider Adapter
        ↓
Provider-specific API or local model
```

Adapters hide provider-specific details such as:

* request structures;
* response structures;
* authentication;
* streaming protocol;
* error payloads;
* model identifiers;
* usage metadata.

Possible providers include:

```text
OpenAI
Gemini
Claude
DeepL
Local translation models
Future providers
```

The Translation core must not depend directly on provider SDK types.

---

## 13. Local and Remote Execution

The architecture supports:

```text
LOCAL_PROVIDER
REMOTE_PROVIDER
```

Possible provider policies include:

```text
AUTOMATIC
PREFERRED
REQUIRED
LOCAL_ONLY
REMOTE_ONLY
```

Local-only execution may be required for:

* privacy;
* offline reading;
* cost control;
* restricted content;
* unavailable network access.

Provider selection must obey privacy and locality policy.

---

## 14. Retry Model

Retry operates within the same logical job.

```text
TranslationJob
      ↓
TranslationBatch
      ↓
Attempt 1
      ↓ failure
Attempt 2
      ↓ fallback
Attempt 3
```

A retry creates:

```text
new TranslationAttemptId
```

When failed batch work is reconstructed, it also creates:

```text
new TranslationBatchId
```

Failed attempts and batches remain immutable.

---

## 15. Retranslation Model

A new job is created when the semantic translation intent changes.

```text
Original job
    profile = COMIC_NATURAL

Retranslation request
    profile = LITERAL

New job
    profile = LITERAL
```

Retranslation may be requested because of:

* a different profile;
* a different target language;
* updated terminology;
* updated context;
* user dissatisfaction;
* another provider policy;
* a requested alternative variant.

---

## 16. Partial Results

Translation supports partial results.

```text
Selected segments
    ├── completed
    ├── failed
    └── pending
```

A partial result must explicitly identify:

* completed segments;
* failed segments;
* missing segments.

No selected segment may disappear silently.

Partial publication depends on policy.

Possible publication modes:

```text
ATOMIC
PROGRESSIVE
FINAL_ONLY
```

### Progressive mode

Validated segments may become available while translation continues.

Useful for:

* comic bubbles;
* visible page regions;
* interactive reading;
* low-latency overlays.

### Atomic mode

Partial work may be stored internally, but the complete translation is published together.

Often preferable for:

* novel paragraph groups;
* short chapters;
* tightly connected narrative sections.

---

## 17. Result Authority

Provider output is not automatically authoritative.

Before a result is offered for acceptance, Translation verifies:

```text
TranslationJobId
TranslationAttemptId
PreparedDocumentId
ContentRevision
TargetLanguage
Cancellation state
Supersession state
Validation result
Variant identity
```

A result arriving late may be retained diagnostically.

Translation determines compatibility and acceptance eligibility. Reading Session owns the final decision about whether a result becomes current for the active reading context. Presentation must not treat Translation completion alone as visible authority.

---

## 18. Cancellation

Cancellation means:

```text
the job can no longer publish authoritative results
```

Physical provider cancellation is best-effort.

Logical cancellation is mandatory.

```text
RUNNING
    ↓
CANCELLATION_REQUESTED
    ↓
CANCELLED
```

Provider output received after cancellation cannot become authoritative.

---

## 19. Supersession

A job is superseded when newer work replaces it.

Examples:

* the source revision changed;
* the user requested a new translation;
* the target language changed;
* another profile was selected;
* a corrected variant became active.

```text
Old job
    ↓
SUPERSEDED

New job
    ↓
authoritative
```

Superseded output may remain valid historically for its original source revision.

---

## 20. Invalidation

Invalidation means stored translation data is no longer considered valid.

Possible causes:

* alignment defects;
* corrupted source identity;
* incorrect result assembly;
* administrative quality rejection;
* privacy or security policy;
* invalid terminology application.

```text
AVAILABLE
    ↓
INVALIDATED
```

Invalidated entities cannot return to valid active states.

A replacement requires a new result, variant or job.

---

## 21. State Machines

Translation maintains separate state machines for:

```text
TranslationJob
    ↓
TranslationBatch
    ↓
TranslationAttempt

TranslationResult
TranslationVariant
```

### Job lifecycle

```text
CREATED
    ↓
QUEUED
    ↓
RUNNING
    ↓
PARTIALLY_COMPLETED
    ↓
COMPLETED
```

Alternative paths:

```text
RUNNING
    ├── RETRY_SCHEDULED
    ├── CANCELLATION_REQUESTED → CANCELLED
    ├── FAILED
    └── SUPERSEDED
```

### Batch lifecycle

```text
CREATED
    ↓
READY
    ↓
RUNNING
    ↓
VALIDATING
    ↓
COMPLETED
```

Provider response success does not bypass validation.

See:

```text
modules/translation/STATES.md
```

---

## 22. Events

Translation events communicate completed domain facts.

Examples:

```text
TranslationJobCreated
TranslationAttemptStarted
TranslationBatchCompleted
TranslationPartialResultAvailable
TranslationCompleted
TranslationFailed
TranslationCancelled
TranslationSuperseded
TranslationVariantActivated
```

Events are:

* immutable;
* provider-neutral;
* revision-aware;
* safe for duplicate delivery;
* compact by default.

Events notify consumers that state changed.

Queries remain authoritative for retrieving current state.

See:

```text
modules/translation/EVENTS.md
```

---

## 23. Errors and Warnings

Translation normalizes provider and internal failures into provider-neutral error codes.

Examples:

```text
TRANSLATION_PROVIDER_TIMEOUT
TRANSLATION_PROVIDER_RATE_LIMITED
TRANSLATION_PROVIDER_RESPONSE_MALFORMED
TRANSLATION_ALIGNMENT_FAILED
TRANSLATION_RETRY_LIMIT_EXCEEDED
```

Warnings represent usable degraded outcomes.

Examples:

```text
TRANSLATION_WARNING_MISSING_CONTEXT
TRANSLATION_WARNING_PRONOUN_AMBIGUITY
TRANSLATION_WARNING_PROVIDER_FALLBACK_USED
TRANSLATION_WARNING_UNTRANSLATED_FRAGMENT
```

Cancellation and supersession are lifecycle outcomes, not normal provider failures.

See:

```text
modules/translation/ERRORS.md
```

---

## 24. Cache Strategy

Translation may reuse compatible completed output.

Cache identity should consider at least:

```text
source content identity or hash
prepared document revision
selected segments
source language
target language
translation profile revision
terminology revision
context revision
provider or model policy
```

A cache entry must not be reused when alignment or semantic input is incompatible.

Cache failure should normally degrade gracefully:

```text
Cache unavailable
    ↓
Perform normal translation
```

Strict offline or cache-only modes may apply different policy.

---

## 25. Streaming Strategy

The preferred public streaming unit is:

```text
completed translated segment
```

or:

```text
group of completed translated segments
```

The public contract should not depend directly on provider token streaming.

Recommended flow:

```text
Provider tokens
    ↓ internal adapter handling
Validated segment output
    ↓
TranslationSegmentsCompleted
```

This preserves alignment and keeps provider details internal.

---

## 26. Concurrency

Translation supports:

* multiple jobs;
* multiple concurrent batches;
* independent reading sessions;
* provider concurrency limits.

Default rule:

```text
one active attempt per TranslationBatch unless policy explicitly allows otherwise
```

Multiple batches may run concurrently within one job. Each batch owns its own execution-attempt history.

Final ordering always follows:

```text
PreparedSegment.sequence
```

not:

```text
batch completion order
event arrival order
```

---

## 27. Privacy and Security

Translation content may be private.

Public contracts, events and logs must not expose:

* API keys;
* access tokens;
* authorization headers;
* raw provider prompts;
* full provider responses;
* unnecessary source text;
* unnecessary translated text;
* private glossary contents.

Source text is untrusted data.

```text
source content
    ≠
system instruction
```

Provider output must not control:

* event types;
* state transitions;
* provider selection;
* result identity;
* routing metadata.

---

## 28. Observability

Recommended Translation metrics include:

```text
job count
completion rate
failure rate
partial completion rate

queue duration
translation latency
batch latency

retry count
fallback count

provider timeout rate
provider rate-limit rate
validation failure rate
alignment failure rate

cache hit rate

input and output usage
estimated cost
```

Observability should prefer:

* identifiers;
* hashes;
* counts;
* durations;
* normalized codes.

Raw reading content should not be logged by default.

---

## 29. Module Dependencies

### Required upstream dependencies

```text
Text Processing
```

Translation requires prepared source content.

### Supporting dependencies

```text
Provider Management
Knowledge
Cache
Reading Session
Observability
Event Bus
```

### Downstream consumers

```text
Presentation
Reading Session
Knowledge review workflows
Observability
```

Conceptual dependency flow:

```text
Text Processing
       ↓
Translation
       ↓
Presentation
```

Supporting flow:

```text
Knowledge ───────────────► Translation
Provider Management ────► Translation
Cache ◄─────────────────► Translation
Reading Session ◄───────► Translation
Observability ◄────────── Translation
```

---

## 30. Public Contract Surface

The initial command surface includes:

```text
StartTranslation
CancelTranslation
RetryTranslation
RequestRetranslation
InvalidateTranslation
SelectTranslationVariant
SubmitTranslationCorrection
```

The initial query surface includes:

```text
GetTranslationJob
GetTranslationResult
GetActiveTranslation
ListTranslationVariants
GetTranslationProgress
GetTranslationSnapshot
GetTranslationVariant
```

See:

```text
modules/translation/CONTRACT.md
```

---

## 31. Recommended Implementation Components

The module may eventually contain components such as:

```text
TranslationCommandService
TranslationQueryService

TranslationJobManager
TranslationBatchManager
TranslationAttemptManager
TranslationBatchPlanner

TranslationContextBuilder
TranslationTerminologyResolver

TranslationProviderSelector
TranslationProviderAdapterRegistry

TranslationOutputParser
TranslationOutputValidator
TranslationAlignmentValidator

TranslationResultAssembler
TranslationVariantManager

TranslationRetryPolicyEvaluator
TranslationCancellationPolicy
TranslationAcceptanceEligibilityChecker

TranslationCacheGateway
TranslationEventPublisher
TranslationRepository
```

These are conceptual responsibilities.

They are not mandatory implementation class names.

---

## 32. Suggested Internal Folder Structure

A possible future implementation layout:

```text
modules/translation/
├── README.md
├── MODULE.md
├── CONTRACT.md
├── EVENTS.md
├── STATES.md
├── ERRORS.md
│
├── domain/
│   ├── job/
│   ├── attempt/
│   ├── batch/
│   ├── result/
│   ├── variant/
│   ├── policies/
│   └── errors/
│
├── application/
│   ├── commands/
│   ├── queries/
│   ├── coordination/
│   └── services/
│
├── providers/
│   ├── contracts/
│   ├── adapters/
│   └── selection/
│
├── validation/
├── alignment/
├── context/
├── terminology/
├── cache/
├── events/
├── persistence/
└── observability/
```

This structure is illustrative.

The final source-code layout should follow CRAI’s language and framework conventions.

---

## 33. MVP Scope

The Translation MVP should support:

* Chinese-to-Vietnamese translation;
* English-to-Vietnamese translation;
* novel prepared text;
* OCR-derived comic prepared text;
* provider abstraction;
* at least one working provider adapter;
* multiple prepared segments per batch;
* translation profiles;
* basic context support;
* basic terminology constraints;
* retry attempts;
* provider fallback;
* cancellation;
* partial results;
* progressive comic translation;
* final result assembly;
* stale-result rejection;
* immutable translation variants;
* basic cache reuse;
* normalized errors and warnings;
* lifecycle events;
* observability metadata.

---

## 34. Deferred Capabilities

Deferred capabilities include:

* simultaneous provider comparison;
* automatic translation quality scoring;
* translation-memory matching;
* automatic glossary learning;
* automatic name discovery;
* advanced speaker inference;
* long-range novel memory;
* collaborative correction review;
* distributed translation workers;
* speculative parallel attempts;
* offline model distribution;
* automatic model downloading;
* cost-budget enforcement;
* provider benchmarking;
* advanced prefetch scheduling.

These capabilities must preserve current source alignment and immutable variant rules.

---

## 35. Core Invariants

The Translation module must always preserve these rules.

1. Translation accepts prepared content, not canonical raw OCR.

2. Every public translated segment maps to exactly one prepared segment.

3. Prepared segments are alignment units; batches are execution units.

4. Retry creates a new attempt, not automatically a new job.

5. Retried batch work uses a new batch identity.

6. Provider-specific payloads remain inside provider adapters.

7. Provider response success does not imply accepted translation success.

8. Output must pass parsing, validation and alignment before publication.

9. Missing and failed source segments remain explicit.

10. Cancelled work cannot become authoritative.

11. Superseded work cannot overwrite newer work.

12. Source revision remains immutable within one translation job.

13. Translation variants are immutable.

14. At most one compatible variant is active for one `ReadingSessionId + PreparedDocumentId + ContentRevision + TargetLanguage + TranslationIntentId` scope.

15. Cache reuse must satisfy semantic and alignment compatibility.

16. Public errors and events never contain provider credentials.

17. Source and translated content are minimized in logs and events.

18. State transitions become durable before corresponding events are published.

---

## 36. Documentation Map

The Translation module documentation consists of:

```text
README.md
MODULE.md
CONTRACT.md
EVENTS.md
STATES.md
ERRORS.md
```

### README.md

Entry point and module overview.

Answers:

* What is this module?
* Where is it in the system?
* What should a new developer read?
* What are the main concepts?

### MODULE.md

Architectural boundaries and responsibilities.

Answers:

* What does Translation own?
* What does it not own?
* How does it interact with other modules?
* What architectural decisions are fixed?

### CONTRACT.md

Public commands, queries and data contracts.

Answers:

* What can callers send?
* What can callers receive?
* What identifiers and structures are public?
* How are jobs, attempts, batches and results represented?

### EVENTS.md

Integration event contracts.

Answers:

* What facts does Translation publish?
* What event identity and ordering rules apply?
* What should consumers subscribe to?
* How are progressive results announced?

### STATES.md

Lifecycle state machines.

Answers:

* What states can each entity enter?
* Which transitions are valid?
* How do retries, cancellation and supersession work?
* Which transitions are forbidden?

### ERRORS.md

Normalized errors and warnings.

Answers:

* What can fail?
* Which failures are retryable?
* How are provider errors normalized?
* What state consequences follow each failure?

---

## 37. Recommended Reading Order

For a new developer or AI agent:

```text
README.md
    ↓
MODULE.md
    ↓
CONTRACT.md
    ↓
EVENTS.md
    ↓
STATES.md
    ↓
ERRORS.md
```

For architectural review:

```text
MODULE.md
    ↓
CONTRACT.md
    ↓
STATES.md
    ↓
EVENTS.md
    ↓
ERRORS.md
```

For implementation work:

```text
CONTRACT.md
    ↓
STATES.md
    ↓
ERRORS.md
    ↓
EVENTS.md
```

The files remain complementary.

No single file should be treated as a complete implementation specification on its own.

---

## 38. Decision Authority

When documentation appears inconsistent, use this priority:

```text
Project-wide architecture rules
        ↓
Translation MODULE.md
        ↓
Translation CONTRACT.md
        ↓
Translation STATES.md
        ↓
Translation EVENTS.md
        ↓
Translation ERRORS.md
        ↓
Translation README.md
```

`README.md` summarizes the other files.

It must not introduce architectural rules that contradict them.

---

## 39. Known Open Decisions

The following decisions remain open before implementation:

### Job authority representation

The architecture uses separate concepts:

```text
execution state
+
acceptance / authority context
```

Translation owns execution-domain truth and acceptance eligibility. Reading Session owns whether a compatible result becomes current for the active reading context.

---

### Progressive event granularity

Choose the default:

```text
TranslationSegmentCompleted
```

or:

```text
TranslationSegmentsCompleted
```

Default decision:

```text
TranslationSegmentsCompleted
```

Use grouped segment-completion events by default. Individual segment events remain optional for bounded low-latency scenarios.

---

### Embedded translated text in events

Recommended:

```text
Local transient events
    → may embed bounded text

Persistent or distributed events
    → use result references
```

---

### Incomplete final success

Define which segment types may be optional.

Potential future contract:

```text
segmentRequirement:
    REQUIRED
    OPTIONAL
    CONTEXT_ONLY
```

---

### Manual retry after final failure

Recommended behavior:

```text
Automatic retry before final failure
    → same TranslationJob

Manual retry after final failure
    → new derived TranslationJob
```

---

### Progressive cancellation behavior

Define whether already published segments remain visible after user cancellation.

Presentation owns the final display behavior.

Translation must provide correct authority information.

---

## 40. Architecture Completion Status

The initial Translation architecture set now contains:

```text
[x] MODULE.md
[x] CONTRACT.md
[x] EVENTS.md
[x] STATES.md
[x] ERRORS.md
[x] README.md
```

The module architecture is internally synchronized and sufficiently defined for:

* cross-module review;
* provider feasibility analysis;
* initial domain modeling;
* interface prototyping;
* sequence diagram creation;
* MVP implementation planning.

It is not yet a complete implementation specification.

The next implementation-oriented work should derive:

* provider capability requirements;
* provider adapter contract;
* data persistence model;
* sequence diagrams;
* MVP technical tasks;
* test scenarios.

---

## 41. Related Architecture Documents

```text
docs/architecture/CAPABILITY_MAP.md
docs/architecture/STATE_MACHINE.md
docs/architecture/EVENT_BUS.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md
docs/architecture/runtime/PIPELINE_RUNTIME.md
docs/architecture/runtime/WORK_QUEUE.md
docs/architecture/runtime/RETRY_POLICY.md
docs/architecture/runtime/CANCELLATION.md
docs/architecture/runtime/RUNTIME_OBSERVABILITY.md
```

Related module documentation:

```text
modules/text-processing/
modules/provider-management/
modules/knowledge/
modules/reading-session/
modules/presentation/
modules/cache/
modules/observability/
```

---

## 42. Quick Reference

```text
Input:
    PreparedDocument
    PreparedSegment[]

Logical request:
    TranslationJob

Execution unit:
    TranslationBatch

Execution attempt:
    TranslationAttempt

Aligned output:
    TranslatedSegment[]

Assembled output:
    TranslationResult

Immutable alternative:
    TranslationVariant
```

Primary execution:

```text
PreparedSegment[]
        ↓
TranslationBatch[]
        ↓
TranslationAttempt[]
        ↓
Provider Adapter
        ↓
Validated TranslatedSegment[]
        ↓
TranslationResult
```

Retry:

```text
Batch Attempt FAILED
        ↓
Job RETRY_SCHEDULED
        ↓
New Attempt for the affected batch
```

Cancellation:

```text
Job CANCELLATION_REQUESTED
        ↓
Job CANCELLED
```

Stale work:

```text
Old result arrives
        ↓
Authority check fails
        ↓
Result NON_AUTHORITATIVE
```

Correction:

```text
Existing variant
        ↓
User correction
        ↓
New immutable variant
```

---

## 43. Summary

The Translation module converts prepared source segments into aligned translated segments.

Its architecture is centered on:

```text
stable source alignment
provider-neutral execution
context-aware batching
safe retries
partial output
revision-safe authority
immutable variants
normalized errors
```

The most important distinction is:

```text
PreparedSegment
    = alignment unit

TranslationBatch
    = provider execution unit

TranslationAttempt
    = one execution attempt for a batch

TranslationJob
    = logical intent

TranslationResult
    = assembled output

TranslationVariant
    = immutable translation version
```

Translation does not own OCR, extraction or rendering.

It sits between Text Processing and Presentation and provides a stable translation domain independent of any specific external provider.
