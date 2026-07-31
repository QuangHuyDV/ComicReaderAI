# Translation Module

> **Project:** CRAI
> **Module:** Translation
> **Document:** Module Definition
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-07-25

---

## 1. Purpose

The Translation module converts prepared source-language content into target-language text while preserving:

* semantic meaning;
* reading context;
* terminology consistency;
* structural alignment;
* content revision traceability;
* compatibility with the Presentation module.

The module is designed primarily for:

* web novels;
* plain-text novels;
* comics and manga;
* OCR-extracted dialogue;
* captions and short text regions;
* mixed-content reading pages.

The module does not accept responsibility for acquiring, detecting, extracting, or visually rendering source content.

Its primary responsibility is:

```text
Prepared source content
        ↓
Context-aware translation
        ↓
Structurally aligned translated content
```

---

## 2. Architectural Position

The Translation module sits after Text Processing and before Presentation.

```text
Content Source
      ↓
Content Acquisition
      ↓
Content Extraction
      ↓
Text Processing
      ↓
Translation
      ↓
Presentation
```

Supporting modules may participate in the translation process:

```text
Knowledge
Provider Management
Cache
Reading Session
Observability
```

The standard interaction is:

```text
Text Processing
      │
      │ PreparedDocument
      ▼
Translation
      │
      │ TranslationResult
      ▼
Presentation
```

Translation must not bypass Text Processing by consuming raw OCR output directly.

---

## 3. Module Goal

The module must provide translations that are:

* sufficiently accurate for uninterrupted reading;
* context-aware;
* consistent across adjacent segments;
* structurally traceable to source segments;
* replaceable when a better translation becomes available;
* independent from any specific translation provider;
* safe from stale-result publication;
* usable for both novel text and comic dialogue.

The primary optimization target is not isolated sentence accuracy.

The primary optimization target is:

```text
continuous reading quality
```

This includes:

* consistent character names;
* natural pronouns;
* preserved dialogue relationships;
* coherent paragraph flow;
* reasonable terminology;
* minimal visible delay.

---

## 4. Responsibilities

The Translation module owns the following responsibilities.

### 4.1 Translation job orchestration

The module creates and manages translation jobs.

A translation job represents one attempt to translate a specific immutable source revision using a specific translation configuration.

The module controls:

* job creation;
* validation;
* scheduling;
* batching;
* provider execution;
* timeout handling;
* retry handling;
* fallback handling;
* cancellation;
* result assembly;
* result publication.

---

### 4.2 Translation batch construction

The module groups prepared segments into translation batches.

A batch is the primary provider execution unit.

```text
PreparedSegment[]
        ↓
TranslationBatch
        ↓
Provider request
```

Batch construction must consider:

* segment order;
* dialogue relationships;
* paragraph boundaries;
* page boundaries;
* provider limits;
* context requirements;
* latency targets;
* cost targets;
* content profile.

---

### 4.3 Provider abstraction

The module communicates with translation providers through internal provider adapters.

Possible providers include:

* local translation models;
* dedicated machine translation APIs;
* large language models;
* user-configured providers;
* offline translation engines.

Provider-specific request and response models must remain internal.

External modules must not depend on:

* provider SDKs;
* provider message formats;
* provider model names;
* provider error formats;
* provider authentication mechanisms.

---

### 4.4 Translation context management

The module decides what contextual information is supplied during translation.

Context may include:

* previous segments;
* following segments;
* paragraph context;
* dialogue-group context;
* page context;
* chapter summary;
* character names;
* glossary terms;
* previously accepted translations;
* reading-session context.

Context supports translation quality but must not change source ownership.

---

### 4.5 Structural alignment

The module preserves a stable mapping between prepared source segments and translated segments.

```text
PreparedSegmentId
        ↓
TranslatedSegment
```

Each published translated segment must remain traceable to its source segment.

Alignment is required for:

* comic overlays;
* side-by-side novel reading;
* translation replacement;
* selective retry;
* user correction;
* cache reuse;
* stale-result detection.

---

### 4.6 Result validation

The module validates provider output before publication.

Validation may include:

* expected segment presence;
* segment identifier preservation;
* output ordering;
* target-language plausibility;
* empty-result detection;
* duplicate-result detection;
* malformed structured-output detection;
* source leakage detection;
* terminology constraint checks;
* output-length sanity checks.

Validation does not guarantee linguistic correctness.

It prevents structurally unsafe or obviously unusable results from becoming authoritative.

---

### 4.7 Retry and fallback

The module may retry a translation when execution fails or output is invalid.

It may also route subsequent attempts to another provider.

Retry and fallback decisions must consider:

* error category;
* retryability;
* provider availability;
* user configuration;
* latency;
* cost;
* previous attempt history;
* whether partial results already exist.

---

### 4.8 Translation caching

The module may reuse a previous translation when the relevant translation inputs are equivalent.

Cache eligibility may depend on:

* prepared source content;
* source language;
* target language;
* translation profile;
* glossary revision;
* context revision;
* provider policy;
* model policy;
* translation contract version.

Cache reuse must never break source alignment or revision safety.

---

### 4.9 Translation metadata

The module records metadata required for:

* debugging;
* cost tracking;
* quality analysis;
* provider comparison;
* cache decisions;
* retry decisions;
* observability.

Provider metadata must be normalized before leaving the module.

---

## 5. Non-Responsibilities

The Translation module does not own the following responsibilities.

### 5.1 Content acquisition

It does not:

* open websites;
* download pages;
* capture browser content;
* capture screenshots;
* fetch comic images;
* manage browser permissions.

These belong to Content Acquisition or platform integration components.

---

### 5.2 Text extraction

It does not:

* run OCR;
* detect text regions;
* identify speech bubbles;
* extract DOM text;
* detect image captions;
* determine reading order from an image.

These belong to Content Extraction.

---

### 5.3 Text normalization

It does not:

* repair OCR characters;
* reconstruct paragraphs;
* merge broken lines;
* remove extraction artifacts;
* identify prepared translation units from raw text;
* normalize punctuation as a source-processing responsibility.

These belong to Text Processing.

Translation may normalize provider output only when necessary to satisfy its own output contract.

---

### 5.4 Glossary ownership

It does not own persistent storage for:

* character dictionaries;
* series terminology;
* preferred names;
* user corrections;
* global glossaries.

These belong to Knowledge or another dedicated persistence module.

Translation only consumes immutable glossary snapshots or references.

---

### 5.5 Presentation

It does not:

* select fonts;
* calculate font sizes;
* fit text into speech bubbles;
* draw translated text over images;
* position overlays;
* paginate translated novels;
* control reading UI.

These belong to Presentation.

---

### 5.6 Reading-session ownership

It does not own:

* the active page;
* scroll position;
* current chapter;
* reading progress;
* user navigation;
* source lifecycle.

These belong to Reading Session.

---

### 5.7 Provider credential ownership

It does not permanently own raw provider credentials.

Credentials must be supplied through an approved secure configuration or credential-management boundary.

Provider adapters may consume credentials but must not expose them.

---

## 6. Core Processing Units

The module distinguishes between several units.

```text
PreparedDocument
TranslationJob
TranslationBatch
PreparedSegment
TranslatedSegment
TranslationResult
```

These units must not be treated as interchangeable.

---

## 7. Prepared Document

A `PreparedDocument` is the complete input produced by Text Processing for one immutable source revision.

It may represent:

* a novel paragraph range;
* a chapter section;
* one comic page;
* multiple visible comic regions;
* a dynamically captured viewport;
* another prepared reading unit.

The Translation module does not modify the prepared document.

It creates translation work derived from it.

---

## 8. Prepared Segment

A `PreparedSegment` is the smallest stable alignment unit accepted by Translation.

Examples include:

* one speech bubble;
* one narration box;
* one paragraph;
* one dialogue line;
* one caption;
* one prepared sentence group.

A prepared segment is not necessarily a provider request.

This distinction is fundamental:

```text
PreparedSegment
    = alignment unit

TranslationBatch
    = provider execution unit
```

Multiple prepared segments may be translated together in one batch.

---

## 9. Translation Job

A `TranslationJob` represents one translation operation for:

```text
one prepared content revision
+
one translation configuration snapshot
```

A job may contain one or more batches.

```text
TranslationJob
      ├── TranslationBatch 1
      ├── TranslationBatch 2
      └── TranslationBatch 3
```

The job owns execution state.

It does not own the original prepared document.

---

## 10. Translation Batch

A `TranslationBatch` is the primary provider execution unit.

It contains:

* one or more prepared segments;
* ordering information;
* batch-level context;
* terminology constraints;
* translation instructions;
* provider execution metadata.

The batch exists to balance:

* translation quality;
* response latency;
* provider limits;
* request cost;
* failure isolation;
* structural alignment.

---

## 11. Why Batch Translation Is Required

Translating every segment independently would cause several problems:

* lost dialogue context;
* inconsistent names and pronouns;
* excessive provider requests;
* increased cost;
* increased network overhead;
* poorer novel paragraph flow;
* unnatural comic conversations.

Therefore, the architecture must support:

```text
multiple segments
        ↓
one translation batch
        ↓
multiple aligned translated segments
```

Batch support is required from the initial architecture, even if an MVP provider adapter initially uses small batches.

---

## 12. Batch Boundaries

Batch boundaries are implementation decisions owned by Translation.

Possible boundary signals include:

* maximum segment count;
* maximum character count;
* maximum estimated tokens;
* paragraph boundaries;
* dialogue groups;
* comic page boundaries;
* chapter boundaries;
* provider context limits;
* latency budget;
* content profile.

A batch must not span unrelated content merely to reduce request count.

---

## 13. Batch Isolation

A batch should be independently retryable whenever possible.

Failure of one batch must not automatically invalidate successful batches unless:

* cross-batch consistency cannot be preserved;
* the provider output contract requires full-document atomicity;
* the translation configuration explicitly requires atomic completion.

The default design should allow partial job progress.

---

## 14. Translation Profiles

A translation profile describes desired translation behavior.

Initial profiles may include:

```text
NOVEL_NATURAL
COMIC_NATURAL
GENERAL_NATURAL
LITERAL
CUSTOM
```

Profiles should influence:

* sentence naturalness;
* dialogue tone;
* paragraph continuity;
* honorific handling;
* punctuation;
* name preservation;
* explanatory behavior;
* output compactness.

Profiles must not expose provider-specific prompt strings as public contracts.

---

## 15. Novel Translation

Novel translation prioritizes:

* paragraph continuity;
* narrative voice;
* pronoun consistency;
* character-name consistency;
* sentence rhythm;
* readable Vietnamese formatting;
* context across adjacent paragraphs.

Typical flow:

```text
Prepared paragraphs
        ↓
Context-aware batches
        ↓
Aligned translated paragraphs
```

Novel translation may use larger context windows than comic translation.

---

## 16. Comic Translation

Comic translation prioritizes:

* bubble alignment;
* short natural dialogue;
* speaker consistency;
* relationship-aware pronouns;
* context across nearby bubbles;
* output suitable for later visual fitting.

Translation must not perform visual fitting itself.

It may produce optional textual hints such as:

* translation is significantly longer than source;
* output may require condensation;
* sound effect remains untranslated;
* source appears incomplete.

Presentation decides how to display the result.

---

## 17. Text and Image Translation Separation

CRAI distinguishes between:

```text
text-source translation
```

and:

```text
image-originated translation
```

However, Translation itself should not maintain two completely separate translation engines.

The upstream pipeline differs:

```text
DOM or plain text
      ↓
Text Processing
      ↓
Translation
```

```text
Image
      ↓
OCR and region extraction
      ↓
Text Processing
      ↓
Translation
```

After Text Processing, both flows should converge on the same core translation contracts where practical.

Content-profile metadata may indicate whether a segment originated from:

* plain text;
* DOM text;
* OCR;
* comic dialogue;
* narration;
* caption;
* sound effect.

---

## 18. Context Model

Translation quality depends on both translatable content and supporting context.

The module distinguishes:

### 18.1 Translatable segments

Segments expected to produce translated output.

### 18.2 Context-only segments

Segments supplied to improve understanding but not expected to produce new published output.

Example:

```text
Previous translated dialogue
        ↓
Context only

Current untranslated bubbles
        ↓
Translatable content
```

Context-only content must be clearly distinguishable from active translation input.

---

## 19. Context Sources

Context may come from:

* the same prepared document;
* adjacent prepared documents;
* previous translation results;
* chapter summaries;
* character dictionaries;
* series glossaries;
* reading-session context;
* user-selected context.

The module must use context through explicit references or snapshots.

It must not silently query arbitrary application state during provider execution.

---

## 20. Context Revision Safety

Context affects translation output.

Therefore, context-sensitive translation must record enough revision information to determine whether cached or delayed results remain valid.

Relevant revisions may include:

* prepared document revision;
* glossary revision;
* terminology snapshot revision;
* chapter context revision;
* translation profile revision.

A context change does not always require automatic retranslation.

The invalidation policy is defined separately.

---

## 21. Knowledge Integration

The Translation module consumes knowledge such as:

* character names;
* character relationships;
* preferred transliterations;
* aliases;
* location names;
* item names;
* martial-arts terms;
* cultivation terms;
* honorific rules;
* user corrections.

Knowledge must be provided through a stable interface.

Translation must not directly depend on the Knowledge module’s internal database schema.

---

## 22. Terminology Constraints

Terminology may be:

```text
locked
preferred
suggested
contextual
```

Examples:

* A locked character name must not be translated differently.
* A preferred term should be used unless the sentence becomes invalid.
* A suggested term may be ignored when context requires another meaning.
* A contextual term may vary by scene or speaker.

Provider adapters must convert these concepts into provider-compatible instructions internally.

---

## 23. Provider-Neutral Architecture

The module must remain independent from a particular provider.

The public architecture must not contain provider-specific types such as:

```text
OpenAIMessage
GeminiContent
ClaudeBlock
DeepLRequest
```

Instead, the internal flow is:

```text
TranslationBatch
        ↓
Provider-neutral execution request
        ↓
Provider adapter
        ↓
Provider-specific request
```

Provider adapters translate between CRAI’s internal model and external provider APIs.

---

## 24. Provider Adapter Boundary

Each provider adapter is responsible for:

* provider request construction;
* authentication integration;
* timeout application;
* response parsing;
* provider error normalization;
* provider usage extraction;
* provider capability reporting;
* cancellation support where available.

The adapter must not decide:

* whether the source revision is current;
* whether a result becomes authoritative;
* whether another provider should be tried;
* how results are presented;
* how glossaries are stored.

Those decisions belong to the Translation core or surrounding modules.

---

## 25. Provider Capabilities

Different providers may support different capabilities.

Examples:

* structured output;
* streaming;
* glossary support;
* local execution;
* batch translation;
* deterministic parameters;
* token usage reporting;
* cancellation;
* model selection;
* low-latency mode.

The module should model provider capabilities explicitly rather than assuming all providers behave equally.

---

## 26. Provider Selection

Provider selection may consider:

* user preference;
* provider availability;
* source and target languages;
* content profile;
* expected quality;
* expected latency;
* expected cost;
* offline requirements;
* privacy policy;
* request size;
* provider capabilities;
* failure history.

The exact scoring algorithm is an implementation detail.

Public configuration should express intent, not a hardcoded provider algorithm.

---

## 27. Provider Fallback

Fallback allows another provider to handle a failed or rejected attempt.

Example:

```text
Preferred provider
        ↓ failure
Fallback provider
        ↓ success
Translation result
```

Fallback must not silently change user-enforced constraints.

If the user explicitly requires one provider, fallback must remain disabled unless the policy allows it.

Provider fallback should be exposed through metadata or warnings when relevant.

---

## 28. Local and Remote Translation

The module should support both:

```text
REMOTE_PROVIDER
LOCAL_PROVIDER
```

A local provider may be:

* an embedded translation engine;
* a locally hosted LLM;
* an operating-system translation service;
* another offline model.

Local providers use the same logical adapter boundary as remote providers.

The rest of the system should not need to know whether execution occurred locally or remotely, except through policy and metadata.

---

## 29. Translation Output

The primary output is a collection of translated segments aligned with prepared segments.

```text
TranslationResult
      └── TranslatedSegment[]
```

Each translated segment must include enough identity information to map back to its source.

A result may additionally contain:

* warnings;
* execution metadata;
* usage data;
* detected anomalies;
* completion information;
* missing-segment information.

---

## 30. Result Authority

Not every completed provider request becomes the active translation result.

Before publication, the module must verify:

* the translation job is still active;
* the source revision is still relevant;
* the result has not been cancelled;
* the result has not been superseded;
* output validation passed;
* publication policy allows partial output.

Only then may the result become authoritative.

---

## 31. Stale-Result Rejection

Translation providers may respond after the user has:

* navigated to another page;
* changed the source text;
* changed the target language;
* requested another translation;
* changed the glossary;
* closed the reading session.

A late result must not overwrite newer work.

The minimum stale-result check should compare:

```text
ReadingSessionId
PreparedDocumentId
ContentRevision
TranslationJobId
```

Additional revision information may also participate.

---

## 32. Cancellation Semantics

Cancellation means:

```text
the job must no longer publish authoritative results
```

Cancellation does not guarantee that:

* the remote provider stops immediately;
* network communication is terminated;
* provider billing is avoided;
* internal cleanup finishes instantly.

The module should request physical cancellation when supported.

Logical cancellation remains mandatory even when physical cancellation is unavailable.

---

## 33. Supersession

A job becomes superseded when newer work replaces it.

Examples:

* a new source revision arrives;
* a newer translation request targets the same content;
* target language changes;
* a manual retry replaces the previous attempt;
* the reading session advances and discards old work.

A superseded job is no longer authoritative.

It may still retain diagnostic metadata.

---

## 34. Retry Semantics

A retry is a new execution attempt.

The architecture should distinguish:

```text
TranslationJob
```

from:

```text
TranslationAttempt
```

Recommended model:

```text
TranslationJob
      ├── Attempt 1
      ├── Attempt 2
      └── Attempt 3
```

This is preferable to creating an unrelated job for every provider retry.

A new job should be created when the user or system starts a logically new translation request.

A new attempt should be created when the same logical request is re-executed.

This distinction must be reflected in `CONTRACTS.md`.

---

## 35. Manual Retranslation

The user may request a new translation because:

* the result is poor;
* another provider is preferred;
* another profile is desired;
* glossary entries changed;
* a more literal version is needed;
* a more natural version is needed.

Manual retranslation should create a new logical translation job or translation variant rather than mutating the historical result in place.

The exact variant model will be defined in public contracts.

---

## 36. Partial Results

A translation job may produce partial results when:

* some batches succeed and others fail;
* streaming provides early completed segments;
* cancellation occurs after some segments complete;
* provider output omits some segments;
* timeout occurs after partial progress.

Partial results must identify:

* completed segments;
* missing segments;
* failed batches;
* whether publication is allowed;
* whether retry is possible.

---

## 37. Partial Publication Policy

Partial publication may be appropriate for:

* visible comic bubbles;
* progressively loaded novel paragraphs;
* streaming translation;
* long chapters.

Partial publication may be inappropriate when:

* consistency requires atomic completion;
* output order cannot be guaranteed;
* missing content would make the result misleading;
* the active presentation mode requires a complete block.

The policy should be configurable by content profile or caller intent.

---

## 38. Streaming

Streaming is optional for the initial implementation but must not be blocked by the architecture.

Two streaming models are possible:

### 38.1 Provider token streaming

Raw text tokens arrive incrementally.

This is difficult to align safely and should remain internal until a segment is structurally complete.

### 38.2 Segment completion streaming

Completed translated segments are published progressively.

This is the preferred public streaming model.

```text
Batch processing
      ↓
Segment completed
      ↓
Validated segment update
```

Public consumers should not depend on provider token streams.

---

## 39. Error Boundaries

Errors should be normalized into module-level categories.

Major categories include:

* invalid input;
* unsupported language;
* unsupported content;
* provider unavailable;
* authentication failure;
* rate limited;
* timeout;
* provider rejected request;
* malformed provider response;
* alignment failure;
* output validation failure;
* context unavailable;
* cancelled;
* superseded;
* internal failure.

Detailed error contracts belong in `ERRORS.md`.

---

## 40. Warning Boundaries

Warnings represent usable but imperfect outcomes.

Possible warnings include:

* missing context;
* low-confidence translation;
* terminology conflict;
* provider fallback used;
* source text appears incomplete;
* output significantly longer than source;
* untranslated source fragment;
* partial result;
* ambiguous speaker relationship.

Warnings must not be confused with fatal errors.

---

## 41. Quality Model

Translation quality is multidimensional.

The module may evaluate or record signals related to:

* completeness;
* alignment correctness;
* terminology consistency;
* target-language fluency;
* source-language leakage;
* structural validity;
* length anomaly;
* confidence;
* user correction history.

The module should not claim objective linguistic accuracy based solely on provider confidence.

---

## 42. Confidence

Provider-reported confidence is not universally available or comparable.

Therefore:

* confidence must be optional;
* provider-native confidence must be normalized carefully;
* synthetic confidence must be clearly identified;
* absence of confidence must not invalidate a result.

The public contract should not require every translated segment to contain a numeric confidence value.

---

## 43. Cache Model

Caching should be based on semantic input identity, not only source text.

A conceptual cache identity may include:

```text
source content hash
source language
target language
translation profile
terminology revision
context identity
contract version
provider or model policy
```

Whether provider identity participates depends on cache policy.

A provider-independent cache may maximize reuse.

A provider-specific cache may preserve deterministic user expectations.

---

## 44. Cache Safety

A cached translation must not be reused when:

* source content changed;
* source alignment changed;
* target language changed;
* required glossary constraints changed;
* translation profile changed materially;
* privacy policy forbids reuse;
* result contract is incompatible.

Cache lookup must never be based only on `PreparedSegmentId`, because identifiers may be session-scoped.

---

## 45. Idempotency

Repeated submission of an equivalent translation command should not create uncontrolled duplicate work.

Possible behavior:

```text
equivalent active request
        ↓
return existing job reference
```

or:

```text
equivalent completed request
        ↓
reuse cached result
```

Idempotency rules will be formalized in `CONTRACTS.md`.

---

## 46. Concurrency

The module may execute:

* multiple jobs concurrently;
* multiple batches within one job concurrently;
* multiple provider attempts concurrently where policy allows.

Concurrency must preserve:

* segment ordering;
* result ownership;
* stale-result rejection;
* provider rate limits;
* cost limits;
* cancellation semantics.

Batch completion order must not determine final segment order.

---

## 47. Ordering

Source order comes from Text Processing.

Translation must preserve or explicitly map that order.

The module must not infer comic reading order from geometry.

It may consume an upstream sequence value and return results aligned to that sequence.

---

## 48. Language Handling

The module should support:

* explicit source language;
* upstream-detected source language;
* unknown source language with detection policy;
* explicit target language.

Initial CRAI priorities are expected to include:

```text
Chinese → Vietnamese
English → Vietnamese
```

The architecture must not hardcode these pairs.

Language identifiers should follow a stable application-level representation.

A later contract decision may adopt BCP 47 language tags.

---

## 49. Mixed-Language Content

A prepared document may contain multiple languages.

Examples:

* Chinese dialogue with English skill names;
* Japanese sound effects beside Chinese text;
* English names inside Vietnamese text;
* numbers and symbols.

The module may support:

* one document-level source language;
* per-segment language hints;
* auto-detected mixed content.

Mixed-language handling must not destroy locked terminology or proper names.

---

## 50. Names and Transliteration

Translation and transliteration are related but separate behaviors.

The module should support policy choices such as:

* preserve original name;
* use established Vietnamese form;
* use Sino-Vietnamese reading;
* use phonetic transliteration;
* use glossary-defined name;
* preserve Latin names.

The Knowledge module supplies preferred mappings.

Translation applies them.

---

## 51. Honorifics and Pronouns

Chinese novels and comics often require contextual interpretation of:

* relationships;
* age;
* rank;
* gender;
* formality;
* cultivation hierarchy;
* family roles.

The translation profile and knowledge context may guide Vietnamese pronouns and honorifics.

The module must not assume that one source pronoun always maps to one Vietnamese pronoun.

When context is insufficient, the module may:

* choose a neutral form;
* preserve ambiguity;
* attach a warning;
* use a configured style rule.

---

## 52. Sound Effects

Comic sound effects may be:

* translated;
* transliterated;
* preserved;
* ignored;
* marked for presentation-specific handling.

Translation should consume an upstream segment-type classification.

It must not detect sound-effect regions itself.

The default MVP may preserve sound effects or translate them only when explicitly requested.

---

## 53. Privacy and Data Handling

Translation input may contain private or sensitive reading content.

The module must support policy decisions regarding:

* remote transmission;
* local-only translation;
* provider logging;
* cache persistence;
* telemetry content;
* diagnostic retention.

Raw source and translated text should not be written to logs by default.

Observability should prefer:

* identifiers;
* lengths;
* hashes;
* durations;
* status;
* provider metadata;
* error categories.

---

## 54. Security

The module must ensure:

* provider credentials are never included in events;
* provider credentials are never stored in translation results;
* raw provider responses are not exposed publicly;
* prompt injection from source content does not alter system-level translation policy;
* untrusted text remains data, not executable instruction;
* external content cannot request access to unrelated application data.

Provider prompts and adapters must clearly separate system instructions, translation rules, context, and source content.

---

## 55. Prompt Injection Boundary

Because novels and comics may contain arbitrary text, source content must be treated as untrusted data.

For LLM-based providers:

```text
source text
≠
trusted instruction
```

The adapter must instruct the provider to translate content without following instructions found inside the source.

Output must still pass structural validation.

---

## 56. Observability

The module should expose operational information such as:

* jobs started;
* jobs completed;
* jobs failed;
* jobs cancelled;
* jobs superseded;
* batch count;
* retry count;
* fallback count;
* provider latency;
* total latency;
* cache-hit rate;
* input and output size;
* estimated or reported usage;
* validation failures.

Observability must avoid leaking content unnecessarily.

---

## 57. Cost Control

Remote providers may charge by:

* request;
* character;
* token;
* model;
* processing time.

The module should support future policies such as:

* prefer lower-cost provider;
* limit maximum tokens;
* limit retries;
* disable expensive fallback;
* use cache before remote execution;
* translate only visible content;
* prefetch within a bounded range.

Cost-control decisions must not be embedded into Presentation.

---

## 58. Latency Strategy

CRAI prioritizes uninterrupted reading.

Latency may be reduced through:

* translation batching;
* cache reuse;
* translating visible content first;
* bounded prefetch;
* concurrent batch execution;
* local providers;
* fast-provider fallback;
* progressive segment publication.

Latency optimization must not allow stale or misaligned output.

---

## 59. Prefetch Support

Translation may support prefetching upcoming content.

Prefetch requests should be lower priority than active visible-content requests.

```text
Visible content
      = foreground priority

Likely upcoming content
      = prefetch priority
```

Prefetched translations must still be revision-safe.

Reading Session decides which content is likely to be needed.

Translation executes the request.

---

## 60. Priority

Translation jobs may carry priority information such as:

```text
INTERACTIVE
VISIBLE
PREFETCH
BACKGROUND
```

The module may schedule higher-priority work first.

Priority must not change semantic output contracts.

---

## 61. Event Interaction

Translation communicates lifecycle changes through the Event Bus.

Expected event categories include:

* translation requested;
* translation started;
* batch started;
* batch completed;
* partial result available;
* translation completed;
* translation failed;
* translation cancelled;
* translation superseded;
* translation invalidated.

Exact event schemas belong in `EVENTS.md`.

---

## 62. Command Interaction

Commands represent intent to change translation state.

Expected command categories include:

* start translation;
* cancel translation;
* request retranslation;
* invalidate translation;
* retry failed work.

Exact command contracts belong in `CONTRACTS.md`.

---

## 63. State Ownership

Translation owns state for:

* translation jobs;
* translation attempts;
* translation batches;
* execution progress;
* normalized translation results;
* translation variants;
* provider execution metadata.

It does not own the lifecycle state of:

* source documents;
* reading sessions;
* OCR jobs;
* overlays;
* glossary entries.

---

## 64. Persistence

Persistent storage may be used for:

* completed translations;
* cache entries;
* job metadata;
* usage statistics;
* user-selected translation variants;
* retry diagnostics.

Ephemeral interactive translation may also operate without persistent storage.

The architecture should not require every translation to be permanently retained.

Retention policy belongs to application configuration and privacy policy.

---

## 65. Translation Variants

A source segment may have multiple valid translated variants.

Examples:

* natural translation;
* literal translation;
* alternative provider result;
* manually corrected translation;
* glossary-updated translation.

The module should preserve the possibility of multiple variants.

However, only one variant should normally be active for a given reading context.

Variant activation must not destroy historical results.

---

## 66. User Corrections

A user correction is not merely provider output.

The Translation module may receive an approved corrected translation, but persistent learning and terminology extraction should involve Knowledge.

Possible flow:

```text
User correction
      ↓
Translation variant
      ↓
Optional Knowledge update
```

The module must not silently update global terminology from every manual edit.

---

## 67. Compatibility with Presentation

Presentation requires:

* source-to-translation alignment;
* stable segment identities;
* translated text;
* output order;
* completion status;
* warnings relevant to display;
* revision information.

Translation must not require Presentation to understand:

* provider request formats;
* retry history internals;
* model prompts;
* raw provider responses.

---

## 68. Compatibility with Text Processing

Translation expects Text Processing to provide:

* normalized source text;
* stable prepared segment identifiers;
* segment sequence;
* content revision;
* document identity;
* source-language information or hints;
* structural metadata;
* context references where available.

Translation must not reinterpret raw extraction geometry as if it owned structure reconstruction.

---

## 69. Compatibility with Reading Session

Reading Session may supply:

* active session identity;
* currently visible content;
* navigation context;
* priority;
* cancellation signal;
* supersession signal;
* prefetch targets.

Translation returns job and result information.

It does not change the active page or scroll position.

---

## 70. Compatibility with Knowledge

Knowledge may supply immutable or revisioned snapshots containing:

* terminology;
* names;
* aliases;
* relationships;
* series-specific rules;
* prior accepted translations.

Translation must record which knowledge revision influenced a result when required for invalidation or reproducibility.

---

## 71. Initial MVP Scope

The MVP Translation module should support:

* Chinese-to-Vietnamese translation;
* English-to-Vietnamese translation;
* prepared text input;
* prepared OCR-derived comic segments;
* multi-segment batches;
* provider abstraction;
* at least one remote provider adapter;
* stable segment alignment;
* timeout handling;
* basic retry;
* cancellation;
* stale-result rejection;
* normalized errors;
* basic terminology injection;
* basic cache reuse;
* completed and partial job outcomes.

---

## 72. Deferred Capabilities

The following may be deferred beyond the MVP:

* automatic provider quality scoring;
* multiple concurrent provider comparison;
* advanced translation-memory search;
* full offline model distribution;
* token-level streaming to UI;
* automatic glossary learning;
* advanced speaker inference;
* automatic pronoun correction across entire novels;
* user voting between variants;
* semantic confidence scoring;
* automated translation quality evaluation;
* distributed translation workers;
* long-term cross-series personalization.

Deferred capabilities must not require breaking the core contracts.

---

## 73. Core Invariants

The following invariants must always hold.

### Invariant 1

Translation never accepts unprepared raw OCR as its canonical public input.

### Invariant 2

Every published translated segment maps to a prepared source segment.

### Invariant 3

Provider-specific payloads never escape the provider-adapter boundary.

### Invariant 4

A cancelled or superseded job never becomes authoritative.

### Invariant 5

A stale result never overwrites a newer translation result.

### Invariant 6

Batch execution never destroys segment-level alignment.

### Invariant 7

Source content is treated as untrusted data, not provider instruction.

### Invariant 8

Translation does not own visual layout or overlay rendering.

### Invariant 9

Translation does not own glossary persistence.

### Invariant 10

Retries preserve attempt history rather than silently mutating prior execution records.

---

## 74. Key Architectural Decisions

The following decisions are established by this document.

### Decision 1 — Translation begins after Text Processing

Raw OCR and raw DOM extraction are outside the Translation boundary.

### Decision 2 — PreparedSegment is the alignment unit

It is not necessarily the provider request unit.

### Decision 3 — TranslationBatch is the provider execution unit

Multiple prepared segments may be translated together.

### Decision 4 — TranslationJob and TranslationAttempt are distinct

Provider retries are attempts within a logical job unless the translation intent changes.

### Decision 5 — Provider integration is adapter-based

Public contracts remain provider-neutral.

### Decision 6 — Result publication is revision-safe

Late, cancelled, and superseded results are rejected.

### Decision 7 — Text-originated and image-originated flows converge

After Text Processing, both use the same core Translation architecture where practical.

### Decision 8 — Partial results are supported

Their publication depends on explicit policy.

### Decision 9 — Segment-completion streaming is preferred

Raw provider token streams should not become the public module contract.

### Decision 10 — Translation variants are immutable historical results

New translations do not overwrite old variants in place.

---

## 75. Open Decisions

The following details remain to be finalized in later documents.

### Contract decisions

* Exact public command names.
* Exact identifiers and revision fields.
* Whether `PreparedDocument` is embedded or referenced.
* Translation variant identity.
* Idempotency-key format.
* Language-tag representation.
* Public partial-result model.

### Event decisions

* Event granularity for batch lifecycle.
* Whether segment-completion events are enabled by default.
* Event payload size limits.
* Event ordering guarantees.

### State decisions

* Exact job states.
* Exact attempt states.
* Exact batch states.
* Transition rules for partial completion.
* Transition rules for manual retranslation.

### Error decisions

* Retryable versus non-retryable categories.
* Provider error normalization.
* Alignment-failure handling.
* Partial-result error representation.

### Policy decisions

* Default batch size.
* Default retry count.
* Default provider fallback behavior.
* Cache identity fields.
* Default context-window size.
* Default sound-effect behavior.

---

## 76. Documentation Order

The remaining Translation documents should be created in this order:

```text
MODULE.md
    ↓
CONTRACTS.md
    ↓
EVENTS.md
    ↓
ERRORS.md
    ↓
STATES.md
    ↓
README.md
```

Responsibilities:

```text
MODULE.md
    Defines boundaries and architectural decisions.

CONTRACTS.md
    Defines public commands and data models.

EVENTS.md
    Defines published integration events.

ERRORS.md
    Defines normalized failures and warnings.

STATES.md
    Defines lifecycle states and transitions.

README.md
    Provides the concise module entry point.
```

---

## 77. Related Documents

```text
.meta/MODULES.md
.meta/MODULES_RULE.md

docs/architecture/CAPABILITY_MAP.md
docs/architecture/STATE_MACHINE.md
docs/architecture/EVENT_BUS.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md

modules/text-processing/MODULE.md
modules/text-processing/CONTRACTS.md
modules/text-processing/EVENTS.md
modules/text-processing/ERRORS.md
modules/text-processing/STATES.md
modules/text-processing/README.md
```

Future related documents:

```text
modules/translation/CONTRACTS.md
modules/translation/EVENTS.md
modules/translation/ERRORS.md
modules/translation/STATES.md
modules/translation/README.md

modules/knowledge/MODULE.md
modules/presentation/MODULE.md
modules/provider-management/MODULE.md
```

---

## 78. Summary

The Translation module is responsible for converting prepared source content into structurally aligned target-language content.

Its central processing model is:

```text
PreparedDocument
      ↓
TranslationJob
      ↓
TranslationBatch[]
      ↓
Provider Adapter
      ↓
TranslatedSegment[]
      ↓
TranslationResult
```

The module is:

* provider-neutral;
* batch-oriented;
* segment-aligned;
* context-aware;
* revision-safe;
* cancellation-aware;
* compatible with partial results;
* designed for both novels and comics.

It deliberately excludes:

* OCR;
* source acquisition;
* source normalization;
* glossary persistence;
* reading-session ownership;
* visual presentation.

This document is the architectural source of truth for all subsequent Translation module contracts, events, errors, states, and implementation documentation.
