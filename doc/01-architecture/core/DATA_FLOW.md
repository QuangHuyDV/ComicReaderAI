# CRAI Data Flow Architecture

> **Project:** CRAI
> **Document:** Data Flow Architecture
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-07-21

---

## 1. Purpose

This document defines how data moves through CRAI from content acquisition to translated presentation.

It describes:

* the major data representations used by CRAI;
* the transformation stages applied to source content;
* the ownership of data at each stage;
* how image-based and text-based flows differ;
* how asynchronous processing results are correlated;
* how stale or cancelled data is prevented from reaching the UI;
* where temporary, cached, session, and persistent data may exist;
* how privacy-sensitive content moves through local and remote providers.

This document does not define:

* the complete runtime state machine;
* the full event catalog;
* source-code package structure;
* provider-specific API formats;
* database schemas;
* concrete IPC or network protocols;
* UI component implementation.

Those concerns are described in their respective architecture documents.

Relevant documents:

```text
.meta/AI_BOOT.md
.meta/PROJECT_RULE.md
.meta/MODULES_RULE.md
.meta/MODULES.md

docs/architecture/CAPABILITY_MAP.md
docs/architecture/STATE_MACHINE.md
docs/architecture/EVENT_BUS.md
docs/architecture/MODULE_DEPENDENCY.md
```

---

## 2. Data Flow Goals

The CRAI data flow must support the following product goals.

### 2.1 Minimal Reading Interruption

The user should not need to repeatedly select, copy, upload, or manually trigger translation while reading.

Once a reading source has been selected, CRAI should process new content with minimal additional interaction.

### 2.2 Prefer the Best Available Source Representation

CRAI should avoid OCR whenever reliable structured text is available.

Preferred source order:

```text
Structured document or webpage text
    ↓
Extracted document text
    ↓
Image with known metadata
    ↓
Raw image
    ↓
Screen capture
```

A lower-quality representation must not replace a higher-quality representation without a justified reason.

### 2.3 Preserve Traceability

Each visible translation should remain traceable to:

* its reading session;
* its source revision;
* its source region or text segment;
* its OCR or extraction output;
* its translation request;
* the provider and configuration used;
* any user correction applied afterward.

### 2.4 Reject Stale Results

A slow OCR or translation result must not overwrite newer content.

Every asynchronous result must be validated against the currently active session and source revision before it is accepted.

### 2.5 Separate Processing from Presentation

Translation data must not depend on whether the UI uses:

* a side panel;
* an overlay;
* a reader view;
* a debug view;
* an exported representation.

Presentation consumes translation results but does not define their semantic structure.

### 2.6 Minimize Sensitive Data Retention

Raw captures, source text, OCR output, translation requests, and user corrections may contain private content.

CRAI must explicitly distinguish between:

* in-memory temporary data;
* temporary local files;
* reusable cache entries;
* reading-session data;
* persistent user knowledge;
* diagnostic metadata.

---

## 3. High-Level Data Flow

The general CRAI data flow is:

```text
Content Source
    ↓
Source Acquisition
    ↓
Source Observation or Import
    ↓
Source Revision Creation
    ↓
Content Classification
    ↓
Extraction
    ↓
Normalization and Structure Reconstruction
    ↓
Translation Unit Construction
    ↓
Context and Knowledge Enrichment
    ↓
Translation
    ↓
Result Validation
    ↓
Presentation Model Construction
    ↓
User Presentation
    ↓
Optional Correction, Cache, or Persistence
```

Not every flow uses every stage.

For example:

* browser text does not normally require OCR;
* a manually pasted sentence may not require observation;
* an image file import may not require stable-frame detection;
* a cached source revision may not require another provider request;
* a translated side panel does not require image rendering.

---

## 4. Data Flow Layers

CRAI data is divided into five conceptual layers.

```text
External Data
    ↓
Source Data
    ↓
Processing Data
    ↓
Result Data
    ↓
Presentation Data
```

### 4.1 External Data

Data that exists outside CRAI.

Examples:

* browser DOM content;
* browser-rendered images;
* application windows;
* screen pixels;
* clipboard content;
* imported files;
* OCR provider responses;
* translation provider responses.

External data is not trusted automatically.

It must be normalized into CRAI-owned representations before entering core processing.

### 4.2 Source Data

Data representing what the user is currently reading.

Examples:

* a captured frame;
* extracted webpage text;
* imported image;
* selected screen bounds;
* webpage metadata;
* chapter identifier;
* source revision identifier.

Source data should preserve enough information to reproduce or explain later processing.

### 4.3 Processing Data

Intermediate representations created during extraction, OCR, normalization, ordering, grouping, and context construction.

Examples:

* detected text regions;
* recognized lines;
* normalized segments;
* reading order;
* translation batches;
* provider request envelopes.

Processing data may be short-lived unless it provides reusable cache or diagnostic value.

### 4.4 Result Data

Semantic outputs that can be reused independently of a specific UI.

Examples:

* translated segments;
* aligned source and target text;
* translation metadata;
* provider information;
* user corrections;
* confidence and warning information.

### 4.5 Presentation Data

UI-ready structures derived from result data.

Examples:

* side-panel entries;
* overlay labels;
* reader paragraphs;
* highlighted regions;
* loading indicators;
* partial-result markers;
* error messages.

Presentation data is disposable and can be rebuilt from result data.

---

## 5. Core Data Identity

Data from concurrent operations must never be correlated by timing or array position alone.

CRAI should use explicit identifiers.

### 5.1 Required Identity Hierarchy

```text
ReadingSessionId
    └── SourceId
          └── SourceRevisionId
                ├── RegionId
                ├── SegmentId
                ├── TranslationUnitId
                └── ProcessingAttemptId
```

### 5.2 Reading Session ID

`ReadingSessionId` identifies one active or historical reading session.

A new session should normally be created when:

* the user selects a new reading source;
* the user explicitly starts a new session;
* the previous session has ended;
* the source mode changes incompatibly;
* recovery cannot safely continue an earlier session.

### 5.3 Source ID

`SourceId` identifies the logical source being read.

Examples:

* selected browser tab;
* selected application window;
* selected screen region;
* imported image;
* imported text file;
* copied text input.

The same source may produce many revisions.

### 5.4 Source Revision ID

`SourceRevisionId` identifies a specific observable version of the source.

Examples:

* a stable comic frame after scrolling;
* a newly loaded chapter;
* an updated DOM extraction;
* a changed image;
* a new clipboard value.

All processing results must reference the revision that produced them.

### 5.5 Region ID

`RegionId` identifies a spatial region within visual content.

A region may represent:

* a speech bubble;
* a narration box;
* a text line;
* a manually selected area;
* a detected text block.

Region identity should remain stable within one source revision.

Cross-revision region matching may be attempted, but must not be assumed.

### 5.6 Segment ID

`SegmentId` identifies a semantic source-text segment.

A segment may be derived from:

* one OCR region;
* several OCR lines;
* one webpage paragraph;
* part of a long paragraph;
* grouped comic dialogue;
* manually corrected source text.

### 5.7 Translation Unit ID

`TranslationUnitId` identifies the unit sent to the translation stage.

One translation unit may contain:

* one segment;
* several adjacent comic segments;
* one paragraph;
* several short paragraphs;
* a bounded portion of a long chapter.

A translation unit must retain alignment with its source segments.

### 5.8 Processing Attempt ID

`ProcessingAttemptId` distinguishes retries, fallback providers, and manual reprocessing of the same source revision.

This prevents an older failed or delayed attempt from being confused with a newer attempt.

---

## 6. Canonical Data Representations

The structures in this section are conceptual contracts, not final programming-language definitions.

---

## 6.1 Source Descriptor

A source descriptor explains where content comes from.

```text
SourceDescriptor
├── sourceId
├── sourceType
├── displayName
├── originMetadata
├── captureOrExtractionConfig
├── privacyClassification
└── createdAt
```

Possible `sourceType` values:

```text
screen-region
application-window
browser-tab
browser-structured-text
clipboard-text
clipboard-image
image-file
text-file
folder-images
document
```

Provider-specific or operating-system-specific handles must remain inside integration adapters whenever possible.

Core modules should receive normalized source descriptors.

---

## 6.2 Source Revision

A source revision represents one version of readable content.

```text
SourceRevision
├── sessionId
├── sourceId
├── revisionId
├── revisionSequence
├── contentKind
├── contentReference
├── sourceHash
├── dimensionsOrStructure
├── capturedAt
├── stabilityMetadata
└── previousRevisionId
```

Possible `contentKind` values:

```text
structured-text
plain-text
image
screen-frame
document-page
mixed
```

The source revision should contain either:

* the source content itself;
* an immutable local reference to it;
* or a controlled handle that remains valid for the required processing lifetime.

---

## 6.3 Visual Frame

A visual frame is the normalized representation of image-based source content.

```text
VisualFrame
├── revisionId
├── pixelDataReference
├── width
├── height
├── pixelFormat
├── scaleFactor
├── cropBounds
├── coordinateSpace
├── frameHash
└── captureMetadata
```

`coordinateSpace` must be explicit.

Possible coordinate spaces include:

```text
captured-frame
screen
application-window
browser-viewport
source-image
rendered-display
```

Regions must not be projected into another coordinate space without a recorded transform.

---

## 6.4 Structured Text Snapshot

A structured text snapshot represents extracted text with document structure.

```text
StructuredTextSnapshot
├── revisionId
├── title
├── languageHint
├── blocks[]
├── sourceStructureMetadata
└── extractionMetadata
```

Each block may contain:

```text
TextBlock
├── blockId
├── blockType
├── text
├── order
├── hierarchy
├── sourceLocator
└── formattingHints
```

Possible block types:

```text
title
heading
paragraph
dialogue
quote
caption
footnote
list-item
unknown
```

Source HTML must not be passed directly into translation providers unless a specific provider contract requires it and sanitization rules have been applied.

---

## 6.5 Detected Region

A detected region represents a potential visual text area.

```text
DetectedRegion
├── regionId
├── revisionId
├── bounds
├── polygon
├── detectionConfidence
├── regionType
├── orientation
├── probableLanguage
├── provisionalOrder
└── detectionMetadata
```

Possible `regionType` values:

```text
speech
narration
caption
sound-effect
body-text
unknown
```

The MVP should not require accurate semantic region classification.

At minimum, detected regions must preserve:

* spatial bounds;
* detection confidence;
* orientation;
* relation to the source revision.

---

## 6.6 OCR Result

OCR output must remain separate from translation data.

```text
OcrResult
├── revisionId
├── attemptId
├── regions[]
├── provider
├── model
├── languageConfiguration
├── startedAt
├── completedAt
├── warnings[]
└── failureMetadata
```

Each OCR region result may contain:

```text
OcrRegionResult
├── regionId
├── rawText
├── normalizedText
├── lines[]
├── charactersOrTokens[]
├── recognitionConfidence
├── orientation
└── providerMetadata
```

Provider-native confidence values may not be directly comparable across providers.

CRAI may normalize them only if the normalization method is documented.

---

## 6.7 Source Segment

A source segment is the normalized unit used to construct translation input.

```text
SourceSegment
├── segmentId
├── revisionId
├── sourceRegionIds[]
├── sourceBlockIds[]
├── rawText
├── normalizedText
├── language
├── readingOrder
├── segmentType
├── contextHints
├── confidence
└── correctionStatus
```

The segment must preserve both:

* the original extracted or recognized text;
* the normalized text used for translation.

User OCR corrections must not silently replace the original OCR result.

---

## 6.8 Translation Unit

A translation unit groups one or more source segments.

```text
TranslationUnit
├── translationUnitId
├── revisionId
├── sourceSegmentIds[]
├── sourceLanguage
├── targetLanguage
├── sourceText
├── alignmentMarkers
├── contextEnvelope
├── glossarySnapshot
├── styleProfile
├── providerPolicy
└── requestConstraints
```

The unit should contain only the context needed for useful translation.

It should not automatically include the entire reading history.

---

## 6.9 Translation Result

```text
TranslationResult
├── translationUnitId
├── revisionId
├── attemptId
├── translatedSegments[]
├── provider
├── model
├── requestMetadata
├── startedAt
├── completedAt
├── cacheStatus
├── warnings[]
└── failureMetadata
```

Each translated segment may contain:

```text
TranslatedSegment
├── segmentId
├── sourceText
├── translatedText
├── alignment
├── confidenceOrQualityHints
├── terminologyApplied[]
└── correctionStatus
```

Translation alignment must use segment identifiers or explicit markers.

It must not rely only on response order when the provider can merge, split, or omit content.

---

## 6.10 Presentation Model

```text
PresentationModel
├── sessionId
├── revisionId
├── mode
├── entries[]
├── sourceGeometry
├── displayConfiguration
├── status
├── warnings[]
└── generatedAt
```

Possible presentation modes:

```text
side-panel
overlay
reader
debug
export-preview
```

A presentation entry may contain:

```text
PresentationEntry
├── entryId
├── sourceSegmentIds[]
├── sourceRegionIds[]
├── sourceText
├── translatedText
├── displayOrder
├── displayBounds
├── emphasis
├── loadingState
├── correctionState
└── warningState
```

---

## 7. Image-Based Reading Flow

The initial CRAI MVP is expected to prioritize an image-based desktop reading flow.

---

## 7.1 Image Flow Overview

```text
User selects source
    ↓
Source descriptor is created
    ↓
Observation starts
    ↓
Candidate frames are captured
    ↓
Unstable and duplicate frames are rejected
    ↓
A stable source revision is created
    ↓
Text regions are detected
    ↓
OCR recognizes region text
    ↓
Reading order is reconstructed
    ↓
Text is normalized and grouped
    ↓
Translation units are created
    ↓
Context and glossary are attached
    ↓
Translation is requested
    ↓
The result is validated
    ↓
A side-panel or overlay model is created
    ↓
The result is displayed
```

---

## 7.2 Source Selection

The user selects one of the supported visual sources.

Initial expected options:

```text
Screen region
Application window
Image from clipboard
Image file
```

The acquisition layer creates a normalized `SourceDescriptor`.

Operating-system window handles, browser implementation details, and capture-library objects must not escape into core processing modules.

---

## 7.3 Continuous Observation

For continuously observed sources, the observation layer produces candidate frames.

```text
Capture tick
    ↓
Candidate frame
    ↓
Cheap change comparison
    ↓
Potentially changed?
    ├── No  → discard
    └── Yes → wait for stability
                  ↓
             stable comparison
                  ↓
             duplicate check
                  ↓
             create source revision
```

Candidate frames are not automatically source revisions.

A source revision is created only when the observation policy accepts a frame.

---

## 7.4 Stability Detection

Scrolling, page animation, loading indicators, advertisements, and cursor movement may change pixels without representing readable new content.

Stability processing should use bounded heuristics such as:

* time since the last significant change;
* difference between consecutive frames;
* motion amount;
* repeated-frame count;
* detected text-area stability;
* source-specific signals when available.

The observer should emit accepted stable frames, not every captured frame.

---

## 7.5 Duplicate Detection

Duplicate detection should prevent repeated OCR and translation of content already processed.

Possible levels:

```text
Exact pixel hash
Perceptual image hash
Region-aware comparison
Extracted-text comparison
Translation-input hash
```

The MVP may begin with a combination of:

* frame hash;
* perceptual similarity;
* active-session revision history.

A duplicate decision must include its scope.

Examples:

```text
Duplicate within current source
Duplicate within current session
Duplicate found in reusable cache
```

---

## 7.6 Revision Acceptance

Once a stable non-duplicate frame is accepted:

1. a new `SourceRevisionId` is assigned;
2. the revision sequence is incremented;
3. older pending work may be marked stale or cancelled;
4. the accepted frame becomes immutable for that processing attempt;
5. extraction begins.

The active session may continue observing newer frames while the accepted revision is processed.

This creates a pipeline rather than a strictly sequential blocking flow.

---

## 7.7 Region Detection and OCR

The extraction flow may use either:

```text
Full-frame OCR
```

or:

```text
Text-region detection
    ↓
Per-region OCR
```

The architecture must support both.

The selected provider may internally combine detection and recognition, but CRAI should still normalize its output into:

* detected regions;
* recognized text;
* confidence;
* geometry;
* ordering hints.

---

## 7.8 Reading-Order Reconstruction

Comic text order may depend on:

* horizontal or vertical writing;
* top-to-bottom placement;
* right-to-left or left-to-right panel conventions;
* speech-bubble positions;
* page layout;
* source language;
* manual correction.

The ordering stage produces an ordered list of source segments.

The original geometric positions must remain available after ordering.

Ordering does not change region identity.

---

## 7.9 Segment Construction

Recognized lines may need to be:

* joined;
* split;
* normalized;
* grouped by speech bubble;
* grouped by nearby context;
* excluded if likely decorative;
* marked as uncertain.

Example:

```text
OCR lines
    ↓
Line normalization
    ↓
Region text reconstruction
    ↓
Reading-order assignment
    ↓
Source segments
```

Sound effects and decorative text may be retained with a distinct segment type rather than silently deleted.

---

## 7.10 Translation Batch Construction

Comic translations are often too ambiguous when translated region by region.

CRAI should support bounded contextual batching.

```text
Ordered source segments
    ↓
Batching policy
    ↓
Translation units
```

Batching may consider:

* maximum character count;
* maximum token estimate;
* spatial neighborhood;
* panel boundary;
* narration versus dialogue;
* provider limits;
* expected response latency;
* glossary size.

Translation units must preserve segment boundaries using explicit alignment metadata.

---

## 7.11 Image Presentation

The initial presentation direction should be non-destructive.

Preferred order:

```text
Side panel
    ↓
Region-linked translation list
    ↓
Simple overlay
```

The side panel can be built using:

* segment order;
* translated text;
* region references;
* loading status;
* warning status.

Overlay presentation additionally requires:

* source geometry;
* coordinate transforms;
* current source scale;
* current source offset;
* clipping and overflow rules.

A translation result should remain valid even when overlay positioning temporarily becomes invalid.

---

## 8. Structured Text Reading Flow

Text-based reading should avoid image capture and OCR when structured text is available.

---

## 8.1 Text Flow Overview

```text
User selects browser or text source
    ↓
Structured text is extracted
    ↓
Relevant reading content is isolated
    ↓
A source revision is created
    ↓
Document structure is normalized
    ↓
Paragraphs are divided into translation units
    ↓
Context and glossary are attached
    ↓
Translation is performed incrementally
    ↓
Translated paragraphs are aligned
    ↓
A reader or in-page presentation model is built
```

---

## 8.2 Browser Extraction

A browser connector may collect:

* document title;
* page URL or safe origin metadata;
* selected article or chapter container;
* headings;
* paragraphs;
* dialogue blocks;
* visible text;
* reading progress;
* source element locators;
* page revision signals.

The connector should send normalized content rather than unrestricted browser internals.

Scripts, hidden elements, navigation text, comments, and unrelated page content should be excluded where possible.

---

## 8.3 Content Isolation

Webpages may contain:

* navigation;
* advertisements;
* comments;
* menus;
* recommendations;
* chapter content;
* hidden text;
* duplicated mobile and desktop layouts.

The content isolation stage identifies the likely reading body.

Possible strategies:

```text
User-selected content container
Site adapter
Semantic article extraction
Density-based extraction
Visible-selection extraction
Fallback manual selection
```

The MVP should prefer predictable extraction over claims of universal automatic support.

---

## 8.4 Text Revision Detection

A new source revision may be created when:

* the chapter URL changes;
* the chapter identifier changes;
* the selected container content changes materially;
* the user opens a new source;
* the browser connector reports navigation;
* the user requests re-extraction.

Minor layout changes should not automatically invalidate the semantic text revision.

---

## 8.5 Paragraph Normalization

Text normalization may include:

* Unicode normalization;
* whitespace cleanup;
* paragraph-boundary preservation;
* dialogue-line preservation;
* punctuation normalization;
* removal of known website artifacts;
* optional Simplified and Traditional Chinese normalization;
* source-language detection.

Formatting should be preserved as semantic hints rather than raw CSS.

---

## 8.6 Long-Content Chunking

Entire chapters may exceed provider limits or create unacceptable latency.

CRAI should translate long content incrementally.

```text
Structured blocks
    ↓
Semantic grouping
    ↓
Size-bounded chunks
    ↓
Translation queue
    ↓
Partial reader updates
```

Chunking should avoid splitting:

* sentences;
* dialogue turns;
* short related paragraphs;
* headings from their immediate content.

The reader may display completed translated blocks while later blocks continue processing.

---

## 8.7 Reader Presentation

Reader presentation should preserve:

* heading hierarchy;
* paragraph separation;
* dialogue formatting;
* reading order;
* chapter title;
* source and target alignment when enabled.

Reader typography is presentation data and must not modify the stored semantic translation result.

---

## 9. Manual Input Flows

CRAI may support direct user input without an observed reading source.

---

## 9.1 Clipboard Text

```text
Clipboard text
    ↓
Input validation
    ↓
Text source revision
    ↓
Normalization
    ↓
Translation unit construction
    ↓
Translation
    ↓
Quick result presentation
```

Clipboard text should be treated as a new revision unless the normalized content is a known duplicate.

---

## 9.2 Clipboard Image

```text
Clipboard image
    ↓
Visual source revision
    ↓
Image extraction flow
    ↓
Translation presentation
```

Continuous observation is not required.

---

## 9.3 Image File

```text
Selected image file
    ↓
Safe file decoding
    ↓
Visual source revision
    ↓
Image extraction flow
```

The original file should remain immutable.

CRAI should process a decoded or controlled internal representation.

---

## 10. Context and Knowledge Flow

Translation context must be deliberately constructed.

It must not be an uncontrolled dump of all previous user data.

---

## 10.1 Context Sources

A translation unit may receive context from:

* neighboring source segments;
* previous segments in the same revision;
* recent translated segments in the same session;
* chapter title;
* series metadata;
* language profile;
* user glossary;
* corrected names and terms;
* selected style profile.

---

## 10.2 Context Envelope

```text
ContextEnvelope
├── localContext
├── previousContext
├── documentContext
├── terminologyContext
├── styleContext
├── sourceMetadata
└── contextPolicy
```

Each context item should identify:

* where it came from;
* why it was included;
* whether it may be sent to a remote provider;
* its maximum retention scope.

---

## 10.3 Glossary Snapshot

Translation requests should use an immutable glossary snapshot.

```text
Current glossary
    ↓
Relevant-term selection
    ↓
Glossary snapshot
    ↓
Translation request
```

If the user changes the glossary while a request is running:

* the active request may finish using the old snapshot;
* the result records which glossary snapshot was used;
* the user may request retranslation using the updated glossary.

---

## 10.4 Correction Flow

User corrections may apply to:

* OCR source text;
* segment ordering;
* translated text;
* a name;
* a recurring term;
* a style preference.

Example:

```text
User edits translated text
    ↓
Correction record is created
    ↓
Presentation updates immediately
    ↓
Optional glossary proposal is generated
    ↓
Future translation context may use the accepted correction
```

A local correction must not silently mutate historical provider output.

Both values should remain distinguishable:

```text
Provider translation
User-corrected translation
```

---

## 11. Asynchronous Processing Flow

OCR and translation are asynchronous and may complete out of order.

---

## 11.1 Processing Envelope

Each asynchronous request should carry a processing envelope.

```text
ProcessingEnvelope
├── sessionId
├── sourceId
├── revisionId
├── attemptId
├── operationType
├── priority
├── createdAt
├── deadline
├── cancellationToken
└── correlationMetadata
```

---

## 11.2 Result Acceptance Rule

A result may update active presentation only when all required checks pass.

```text
Result received
    ↓
Session still exists?
    ├── No → discard or retain only diagnostics
    └── Yes
         ↓
Revision still relevant?
    ├── No → mark stale
    └── Yes
         ↓
Attempt still accepted?
    ├── No → discard
    └── Yes
         ↓
Result structurally valid?
    ├── No → report processing error
    └── Yes → commit result
```

---

## 11.3 Stale Result

A result is stale when it belongs to valid historical work but is no longer eligible to update the active view.

Examples:

* the user scrolled to a newer frame;
* the source selection changed;
* a manual retranslation superseded the automatic request;
* a fallback provider returned before the original provider;
* the session ended;
* the user changed the source language and restarted processing.

Stale results may be used for bounded cache or diagnostics only when privacy rules allow it.

---

## 11.4 Cancellation

Cancellation should flow downstream from the session or scheduler.

```text
Session or revision cancelled
    ↓
Pending acquisition work stops
    ↓
Extraction requests receive cancellation
    ↓
Translation requests receive cancellation
    ↓
Late results fail acceptance checks
    ↓
Temporary data becomes eligible for cleanup
```

Cancellation is cooperative.

A remote provider request may not always be physically stoppable.

Therefore, result validation remains mandatory even when cancellation is supported.

---

## 11.5 Backpressure

Continuous capture can produce data faster than OCR and translation can process it.

The scheduler must apply backpressure.

Possible policies:

* process only the latest stable revision;
* keep at most one pending revision;
* discard intermediate frames;
* pause capture-triggered processing while scrolling;
* prioritize visible content;
* deprioritize prefetch work;
* limit concurrent provider calls;
* preserve manual requests over automatic work.

Default MVP direction:

```text
Active revision
    +
At most one latest pending revision
```

Intermediate obsolete revisions should be discarded before expensive processing whenever possible.

---

## 12. Cache Data Flow

Caching can reduce latency and provider cost but must not weaken privacy or correctness.

---

## 12.1 Cache Layers

Possible cache layers:

```text
Frame comparison cache
OCR result cache
Normalized segment cache
Translation result cache
Presentation model cache
```

Presentation-model caching is optional because presentation data is comparatively cheap to rebuild.

---

## 12.2 OCR Cache Key

An OCR cache key may include:

```text
Image or region content hash
OCR provider
OCR model
Language configuration
Detection configuration
Image preprocessing version
Normalization version
```

Changing one of these inputs may invalidate reuse.

---

## 12.3 Translation Cache Key

A translation cache key may include:

```text
Normalized source text
Source language
Target language
Context fingerprint
Glossary snapshot fingerprint
Style profile
Translation provider
Translation model
Prompt or adapter version
```

Source text alone is insufficient for context-sensitive translation caching.

---

## 12.4 Cache Scope

Cache entries must declare a scope.

Possible scopes:

```text
processing-attempt
source-revision
reading-session
local-user
series
global-local
```

The MVP should default to conservative scopes.

Suggested initial direction:

```text
Frame comparison cache → reading session
OCR cache              → local user with retention limit
Translation cache      → local user with retention limit
Presentation cache     → active reading session
```

---

## 12.5 Cache Lookup Flow

```text
Processing input created
    ↓
Cache key constructed
    ↓
Eligible cache entry found?
    ├── No → provider or processor execution
    └── Yes
         ↓
     Privacy scope valid?
         ├── No → ignore entry
         └── Yes
              ↓
          Version compatible?
              ├── No → ignore entry
              └── Yes → return cached result
```

Cache hits must still pass revision and attempt acceptance checks.

---

## 13. Storage and Retention Flow

CRAI should distinguish several storage lifetimes.

| Lifetime   | Purpose                                        | Example                    |
| ---------- | ---------------------------------------------- | -------------------------- |
| Immediate  | Exists only during one operation.              | Decoded image buffer       |
| Revision   | Exists while one source revision is processed. | Detected regions           |
| Session    | Exists during the reading session.             | Recent translation context |
| Cache      | Reusable with expiration and key validation.   | OCR result                 |
| Persistent | Explicitly retained for future sessions.       | User glossary              |
| Diagnostic | Retained according to logging policy.          | Stage duration             |

---

## 13.1 Raw Screen Captures

Default architecture direction:

* keep raw captures in memory where practical;
* avoid writing continuous screen captures to disk;
* release obsolete frames promptly;
* exclude raw pixels from normal logs;
* require explicit policy for persisted screenshots;
* clear temporary representations after cancellation or session completion.

---

## 13.2 Source Text

Source text may be needed for:

* alignment;
* correction;
* retranslation;
* cache lookup;
* session recovery;
* diagnostics.

Its retention must follow the selected privacy mode.

Private-session mode may disable persistent source-text storage.

---

## 13.3 Persistent Knowledge

The following may be intentionally persistent:

* glossary entries;
* preferred name translations;
* style settings;
* provider configuration;
* UI preferences;
* explicitly saved corrections;
* optional reading history.

Persistent knowledge must be separated from temporary captured content.

---

## 14. Remote Provider Data Flow

Some OCR or translation implementations may send content outside the local device.

---

## 14.1 Remote Processing Boundary

```text
CRAI internal representation
    ↓
Provider adapter
    ↓
Data minimization
    ↓
Credential attachment
    ↓
Remote request
    ↓
Provider-native response
    ↓
Response validation
    ↓
CRAI canonical result
```

Provider-native objects must not flow directly into core modules or presentation.

---

## 14.2 Data Minimization

Before sending content remotely, CRAI should send only what is required.

Examples:

* crop text regions instead of sending the full screen when supported;
* send selected chapter content rather than the whole webpage;
* include only relevant glossary entries;
* remove unrelated metadata;
* avoid sending window titles or URLs unless necessary;
* omit historical context beyond the configured context policy.

---

## 14.3 Credential Flow

Credentials should flow only through:

```text
Secure configuration
    ↓
Provider adapter
    ↓
Remote request
```

Credentials must not be attached to:

* canonical translation units;
* event payloads;
* logs;
* presentation models;
* cache keys;
* diagnostic exports.

---

## 14.4 Provider Response Validation

Provider responses should be checked for:

* successful status;
* expected schema;
* response size;
* segment alignment;
* missing segments;
* duplicated segments;
* unsupported language behavior;
* safety or content filtering notices;
* rate limiting;
* timeout;
* malformed text;
* unexpected HTML or binary content.

A provider response is not automatically a valid CRAI result.

---

## 15. Event-Driven Data Movement

The event bus may notify modules that data is available, but it should not become uncontrolled shared storage.

Events should contain:

* identifiers;
* bounded immutable payloads;
* references to controlled data;
* status metadata.

Events should not normally contain:

* large raw frame buffers;
* provider credentials;
* mutable module-owned objects;
* unrestricted browser DOM trees;
* complete persistent stores.

Example event progression:

```text
source.revision.accepted
    ↓
extraction.requested
    ↓
extraction.completed
    ↓
segments.prepared
    ↓
translation.requested
    ↓
translation.completed
    ↓
presentation.updated
```

The authoritative event names and payload rules belong to:

```text
docs/architecture/EVENT_BUS.md
```

---

## 16. State Machine Relationship

The data flow and state machine describe different concerns.

```text
State machine:
What lifecycle state is the session or operation in?

Data flow:
What data exists, who owns it, and where does it move?
```

Example:

```text
State: TRANSLATING
```

may involve:

```text
TranslationUnit
ProcessingEnvelope
GlossarySnapshot
ProviderRequest
PartialTranslationResult
```

State transitions must not be inferred only from the presence of data.

Similarly, data may remain available after a state changes for cache, correction, or diagnostic purposes.

The authoritative state definitions belong to:

```text
docs/architecture/STATE_MACHINE.md
```

---

## 17. Module Ownership Rules

Every canonical data type must have one owning module or bounded responsibility.

Ownership means responsibility for:

* creating the canonical object;
* validating its invariants;
* controlling mutation;
* determining its lifecycle;
* publishing safe read models or events.

Suggested conceptual ownership:

| Data                   | Owning responsibility                               |
| ---------------------- | --------------------------------------------------- |
| SourceDescriptor       | Source or acquisition management                    |
| SourceRevision         | Reading-session and source-observation coordination |
| VisualFrame            | Capture or image acquisition                        |
| StructuredTextSnapshot | Browser or document extraction                      |
| DetectedRegion         | Visual text extraction                              |
| OcrResult              | OCR processing                                      |
| SourceSegment          | Text understanding                                  |
| TranslationUnit        | Translation orchestration                           |
| TranslationResult      | Translation orchestration                           |
| GlossarySnapshot       | Knowledge and consistency                           |
| PresentationModel      | Presentation orchestration                          |
| ProcessingTrace        | Diagnostics                                         |

The final mapping must match `.meta/MODULES.md`.

---

## 17.1 No Shared Mutable Pipeline Object

CRAI should not pass one large mutable context object through every module.

Avoid:

```text
GlobalProcessingContext
├── frame
├── OCR data
├── translation data
├── UI data
├── settings
├── credentials
├── cache
└── mutable status
```

This creates hidden coupling and makes stale-result protection difficult.

Prefer explicit immutable stage outputs:

```text
SourceRevision
    ↓
ExtractionResult
    ↓
PreparedSegments
    ↓
TranslationResult
    ↓
PresentationModel
```

---

## 17.2 Mutation Rules

Canonical processing results should be immutable after publication.

Corrections should create a new revision or correction record.

Examples:

```text
OCR result
    +
OCR correction
    ↓
Corrected source segment
```

```text
Provider translation
    +
User translation correction
    ↓
Effective translated segment
```

This preserves traceability.

---

## 18. Error Data Flow

Errors should be represented as structured processing outcomes.

---

## 18.1 Error Categories

```text
Source unavailable
Capture failure
Source changed
Unsupported content
Extraction failure
OCR failure
Normalization failure
Ordering failure
Translation failure
Provider timeout
Provider rate limit
Provider authentication failure
Cancellation
Stale result
Presentation alignment failure
Storage failure
Configuration failure
```

---

## 18.2 Recoverable and Terminal Errors

A recoverable error may allow:

* bounded retry;
* fallback provider;
* reduced processing mode;
* manual correction;
* manual retranslate;
* source reselection.

A terminal error ends the current processing path but does not necessarily end the entire reading session.

Example:

```text
Overlay alignment failure
    ↓
Fallback to side panel
```

The translation result remains usable.

---

## 18.3 Partial Results

Partial success should be preserved when useful.

Examples:

* five of seven OCR regions succeeded;
* translation completed for earlier paragraphs;
* one translation batch failed;
* overlay positioning failed but side-panel rendering succeeded.

Partial data must contain explicit completeness status.

It must not appear indistinguishable from a complete result.

---

## 19. Diagnostics Data Flow

Diagnostics should observe processing without becoming part of the business pipeline.

---

## 19.1 Processing Trace

A processing trace may link:

```text
Session
    ↓
Source revision
    ↓
Extraction attempt
    ↓
OCR attempt
    ↓
Translation units
    ↓
Provider attempts
    ↓
Presentation update
```

Suggested trace metadata:

* identifiers;
* stage names;
* timestamps;
* duration;
* queue delay;
* provider;
* retry count;
* cache status;
* cancellation reason;
* stale-result reason;
* content sizes;
* warning codes.

---

## 19.2 Sensitive Diagnostic Exclusions

Normal diagnostics should exclude:

* raw screenshots;
* full chapter contents;
* full OCR text;
* complete translation prompts;
* provider credentials;
* personal tokens;
* unrestricted URLs;
* clipboard contents.

Debug modes that expose content must be explicit and locally controlled.

---

## 19.3 Performance Measurements

Important data-flow timings include:

```text
Source change to accepted revision
Accepted revision to OCR start
OCR duration
Segment preparation duration
Translation queue delay
Translation provider duration
Result validation duration
Presentation construction duration
Time to first visible translation
Time to complete visible translation
```

The most important product measurement is not provider latency alone.

It is:

```text
Source content becomes readable
    ↓
Useful Vietnamese translation becomes visible
```

---

## 20. Presentation Update Strategy

The UI should consume revision-aware updates.

---

## 20.1 Atomic Revision Presentation

For short comic pages, the UI may wait until all expected segments are ready and then publish one complete presentation model.

Advantages:

* stable ordering;
* reduced UI movement;
* easier source-to-translation matching.

Disadvantages:

* higher time to first result;
* one slow segment delays the whole page.

---

## 20.2 Incremental Presentation

For long text or slow translation, the UI may update incrementally.

```text
Revision accepted
    ↓
Loading presentation
    ↓
First translated units
    ↓
Partial presentation
    ↓
Additional translated units
    ↓
Complete presentation
```

Each update must reference the same revision and contain completeness metadata.

---

## 20.3 Recommended Initial Policy

Suggested MVP behavior:

```text
Comic image:
Prefer small contextual batches and bounded incremental updates.

Long text:
Use paragraph or chunk-level incremental updates.

Manual short input:
Return one atomic result.
```

The UI must avoid excessive reordering after entries have become visible.

---

## 21. Coordinate Transformation Flow

Image overlays require controlled geometry transformation.

```text
Source-image coordinates
    ↓
Capture crop transform
    ↓
Window or viewport transform
    ↓
Display scale transform
    ↓
Presentation coordinates
```

Each transformation should be explicit.

An overlay entry must identify:

* the coordinate space of its source region;
* the target coordinate space;
* the transform version;
* whether the transform is still valid.

Overlay geometry may become invalid when:

* the user scrolls;
* the window moves;
* the browser zoom changes;
* display scale changes;
* the source image resizes;
* the selected region changes.

Invalid geometry should suspend or rebuild the overlay.

It must not invalidate the underlying translation result.

---

## 22. Data Flow Security Boundaries

Main security boundaries:

```text
Operating system capture boundary
Browser connector boundary
File import boundary
Local process or IPC boundary
Provider network boundary
Persistent storage boundary
Export boundary
```

At each boundary, CRAI should define:

* accepted data types;
* size limits;
* validation;
* sanitization;
* authentication where required;
* privacy classification;
* logging restrictions;
* timeout and cancellation behavior.

---

## 23. Initial MVP Data Flow

The recommended initial MVP data flow is:

```text
User selects a screen region or application window
    ↓
CRAI creates a reading session and source descriptor
    ↓
The capture adapter produces candidate frames
    ↓
Observation accepts a stable non-duplicate frame
    ↓
A source revision is created
    ↓
The previous obsolete revision is cancelled
    ↓
Text regions are detected
    ↓
Simplified Chinese or English text is recognized
    ↓
Regions are ordered and converted into source segments
    ↓
Nearby segments are grouped into bounded translation units
    ↓
Relevant glossary entries are attached
    ↓
Translation is requested
    ↓
The result is correlated with the active revision
    ↓
A region-linked side-panel model is created
    ↓
The translation is displayed
    ↓
The user may retranslate or correct a term
    ↓
Reusable results may be cached locally
```

---

## 23.1 MVP Data Kept in Memory

Expected memory-only data:

* active raw frames;
* candidate unstable frames;
* decoded provider request bodies;
* cancellation tokens;
* transient presentation calculations;
* obsolete pending revisions;
* temporary coordinate transforms.

---

## 23.2 MVP Data Eligible for Local Cache

Potentially cacheable with retention controls:

* image or region fingerprints;
* OCR results;
* normalized source segments;
* translation results;
* provider-independent alignment data.

---

## 23.3 MVP Persistent Data

Expected persistent data:

* user settings;
* language settings;
* provider configuration references;
* protected credentials;
* glossary entries;
* accepted terminology corrections;
* cache metadata;
* optional diagnostic aggregates.

Raw continuous screen captures should not be persistent by default.

---

## 23.4 MVP Deferred Data Flows

The first MVP should not require:

* downloading complete online series;
* cloud synchronization;
* account-based reading history;
* permanent screenshot libraries;
* translated-image generation;
* background inpainting;
* permanent replacement of text inside speech bubbles;
* EPUB library management;
* PDF reader management;
* public plugin data exchange;
* cross-device glossary synchronization.

---

## 24. Data Flow Invariants

The following invariants must hold.

### Invariant 1

Every processing result belongs to exactly one source revision.

### Invariant 2

Every source revision belongs to one source and one reading session.

### Invariant 3

No asynchronous result may update active presentation without revision validation.

### Invariant 4

Provider-native data must be normalized before entering core processing.

### Invariant 5

Presentation models must not become the authoritative source of translation data.

### Invariant 6

Original OCR and provider translation outputs must remain distinguishable from user corrections.

### Invariant 7

Translation results must preserve alignment with source segments.

### Invariant 8

Raw captured content must not enter normal logs.

### Invariant 9

Credentials must remain inside secure configuration and provider boundaries.

### Invariant 10

Cancellation does not remove the need for stale-result validation.

### Invariant 11

A duplicate cache result must pass the same revision and scope checks as a new result.

### Invariant 12

Overlay geometry failure must not destroy an otherwise valid translation result.

### Invariant 13

Structured text must be preferred over OCR when it is reliable and available.

### Invariant 14

Large continuous sources must be bounded by backpressure and concurrency limits.

### Invariant 15

Each canonical data representation must have one clear owner.

---

## 25. Example: New Comic Frame During Translation

```text
Revision 10 accepted
    ↓
OCR for revision 10 starts
    ↓
Translation for revision 10 starts
    ↓
User scrolls
    ↓
Revision 11 accepted
    ↓
Revision 10 is marked obsolete
    ↓
Cancellation is requested for revision 10
    ↓
OCR and translation for revision 11 start
    ↓
Translation result for revision 10 arrives late
    ↓
Result validation detects inactive revision
    ↓
Revision 10 result is marked stale
    ↓
Active presentation is not updated
    ↓
Revision 11 result arrives
    ↓
Result validation succeeds
    ↓
Revision 11 presentation is displayed
```

This is a required behavior, not an optional optimization.

---

## 26. Example: Cached Translation with Updated Glossary

```text
Source segments are prepared
    ↓
Translation cache key is calculated
    ↓
Matching source-text cache entry exists
    ↓
Glossary fingerprint differs
    ↓
Cache entry is not considered equivalent
    ↓
A new translation request is created
    ↓
The result records the current glossary snapshot
```

A translation produced without the current terminology rules must not be treated as an exact reusable result.

---

## 27. Example: Overlay Becomes Misaligned

```text
Translation result is displayed as overlay
    ↓
Browser zoom changes
    ↓
Existing coordinate transform becomes invalid
    ↓
Overlay presentation is suspended
    ↓
Side-panel translation remains available
    ↓
New source geometry is captured
    ↓
Presentation model is rebuilt
    ↓
Overlay resumes
```

Translation processing does not need to run again unless the readable source content changed.

---

## 28. Open Decisions

The following decisions require prototype evidence.

### 28.1 Frame Observation

* Which frame-difference algorithm is sufficient for the MVP?
* How long must content remain stable before OCR begins?
* Should cursor movement be ignored?
* Should OCR begin during slow scrolling or only after scrolling stops?
* How many pending source revisions may exist?

### 28.2 Image Processing

* Should the MVP use full-frame OCR or separate text detection and recognition?
* Which preprocessing steps improve Chinese comic OCR?
* Should region crops be cached separately?
* How should vertical and horizontal regions be grouped?

### 28.3 Translation

* What is the ideal contextual batch size for comics?
* How should provider-specific context limits be normalized?
* Should partial streaming results update the side panel?
* Which context fields may be sent remotely by default?
* How should automatic provider fallback affect cache identity?

### 28.4 Text Reading

* Should a browser connector send full chapter snapshots or incremental block changes?
* How should website adapters identify chapter revisions?
* Should translated chapters be retained after the session?
* How much prior chapter context should be reusable?

### 28.5 Storage

* What are the default OCR and translation cache retention periods?
* Should cache storage be content-addressed?
* Should private sessions disable all disk cache writes?
* Should session recovery persist source text?
* Which corrections should become permanent glossary entries?

### 28.6 Presentation

* Should comic results appear atomically or incrementally?
* How much UI movement is acceptable during incremental updates?
* When should overlay automatically fall back to a side panel?
* Should source-region numbering persist after the result is complete?

---

## 29. Validation Scenarios

The data-flow architecture should be validated with representative scenarios.

### Scenario A — Continuous Comic Scrolling

Validate that:

* only stable frames create revisions;
* duplicate pages are ignored;
* stale OCR and translation are rejected;
* CPU and memory remain bounded;
* the latest page becomes readable quickly.

### Scenario B — Vertical Chinese Comic Text

Validate that:

* geometry is preserved;
* vertical orientation survives provider normalization;
* region order can be corrected;
* translated segments remain aligned.

### Scenario C — Long Web Novel Chapter

Validate that:

* unrelated webpage text is excluded;
* paragraph structure remains readable;
* translation occurs incrementally;
* earlier results stay stable while later chunks complete;
* provider limits are respected.

### Scenario D — User Scrolls During Translation

Validate that:

* cancellation propagates;
* late results cannot replace the current page;
* obsolete frame data is cleaned up;
* the latest revision receives priority.

### Scenario E — User Corrects a Name

Validate that:

* the visible result updates;
* the provider result remains traceable;
* a glossary entry may be created explicitly;
* later requests use the updated glossary;
* earlier cache entries are not incorrectly reused.

### Scenario F — Remote Provider Failure

Validate that:

* the failure is classified;
* retry is bounded;
* fallback does not duplicate visible results;
* credentials are not logged;
* the reading session remains recoverable.

### Scenario G — Overlay Alignment Failure

Validate that:

* translation data remains available;
* presentation can fall back to a side panel;
* geometry can be rebuilt independently;
* OCR and translation are not unnecessarily repeated.

---

## 30. Architecture Review Checklist

Before implementing a data flow, verify:

* Does the flow use the highest-quality available source representation?
* Is every result associated with a session, source, revision, and attempt?
* Is the owner of each data structure clear?
* Can stale results be detected?
* Can pending work be cancelled?
* Is backpressure defined?
* Are source and translated segments aligned explicitly?
* Are provider-native structures isolated in adapters?
* Are credentials excluded from events and logs?
* Is raw captured content retained only as long as necessary?
* Are cache keys sensitive to context, glossary, model, and configuration?
* Can presentation be rebuilt without repeating translation?
* Can partial results be represented honestly?
* Can user corrections be traced separately from original outputs?
* Does the flow remain usable when overlay presentation fails?
* Are failure and fallback behaviors defined?
* Is private-session behavior respected?
* Can the complete flow be measured from source change to visible translation?

---

## 31. Next Architecture Work

After this document, the next useful step is to define concrete end-to-end flow specifications.

Recommended documents:

```text
docs/architecture/flows/SCREEN_COMIC_FLOW.md
docs/architecture/flows/BROWSER_TEXT_FLOW.md
docs/architecture/flows/MANUAL_INPUT_FLOW.md
```

Recommended order:

```text
DATA_FLOW.md
    ↓
SCREEN_COMIC_FLOW.md
    ↓
Prototype gates for capture, OCR, and translation
    ↓
BROWSER_TEXT_FLOW.md
    ↓
Detailed module contracts
```

`SCREEN_COMIC_FLOW.md` should be the immediate next document because it represents the recommended first MVP and exercises the most important CRAI architecture concerns:

* continuous observation;
* stable-frame detection;
* duplicate rejection;
* OCR;
* reading order;
* contextual translation;
* stale-result cancellation;
* side-panel presentation;
* user corrections;
* privacy-sensitive captured data.

---

## 32. Summary

CRAI should use a revision-aware, traceable, cancellable data pipeline.

The essential flow is:

```text
Source
    ↓
Immutable source revision
    ↓
Canonical extraction result
    ↓
Prepared source segments
    ↓
Context-bound translation units
    ↓
Validated translation results
    ↓
Disposable presentation models
```

The architecture must ensure that:

* structured text is preferred over OCR;
* continuous capture does not create unbounded work;
* every asynchronous result is correlated explicitly;
* stale results never overwrite newer content;
* translation remains independent from visual presentation;
* source data and corrections remain traceable;
* remote processing is isolated behind provider adapters;
* sensitive reading content is retained only according to explicit policy.

The success of CRAI depends not merely on OCR or translation quality in isolation, but on whether the entire data flow produces useful Vietnamese content quickly and unobtrusively during continuous reading.
