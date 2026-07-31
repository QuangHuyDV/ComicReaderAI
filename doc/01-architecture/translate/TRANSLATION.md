# Translation Architecture

## Purpose

The Translation Architecture converts translation-ready text segments into translated content.

It receives structured requests from the Translation Context layer, executes translation through an available translation provider, validates the result, and returns translated segments while preserving their original identities and order.

The architecture must support both AI-based translation and traditional machine translation without depending on a specific provider.

---

## Position in the Pipeline

```text
OCR Reading Order
        │
        ▼
Text Model
        │
        ▼
Segmentation
        │
        ▼
Translation Context
        │
        ▼
Translation
        │
        ▼
Presentation
```

The Translation layer does not extract text, reconstruct reading order, or display translated content.

---

## Responsibilities

The Translation Architecture is responsible for:

* Receiving translation requests
* Selecting an appropriate translation provider
* Selecting a translation model or translation strategy
* Grouping compatible segments into translation batches
* Building provider-specific requests
* Executing translation
* Handling timeouts, retries, and cancellation
* Validating translated output
* Mapping translated results back to source segments
* Preserving segment identity and order
* Reporting translation progress
* Producing presentation-ready translation results

---

## Non-Responsibilities

The Translation Architecture is not responsible for:

* Capturing screen content
* Detecting text regions
* Performing OCR
* Reconstructing reading order
* Segmenting raw text
* Building domain-level context
* Rendering text or comic overlays
* Permanently storing books or chapters
* Managing user interface state

These responsibilities belong to other CRAI modules.

---

## High-Level Flow

```text
Translation Request
        │
        ▼
Request Validation
        │
        ▼
Translation Planning
        │
        ▼
Provider Selection
        │
        ▼
Batch Construction
        │
        ▼
Provider Request Mapping
        │
        ▼
Translation Execution
        │
        ▼
Response Parsing
        │
        ▼
Result Validation
        │
        ▼
Segment Mapping
        │
        ▼
Translation Result
```

Each stage should remain independently observable and cancelable.

---

## Translation Request

The Translation layer receives one or more translation targets together with their prepared context.

```ts
interface TranslationRequest {
  requestId: string;

  sourceLanguage: string;
  targetLanguage: string;

  segments: TranslationTarget[];

  context: TranslationContext;

  preferences?: TranslationPreferences;
  execution?: TranslationExecutionOptions;

  requestRevision: number;
}
```

A translation request may contain one segment or several compatible segments.

---

## Translation Target

A translation target represents content that must produce translated output.

```ts
interface TranslationTarget {
  segmentId: string;
  sourceText: string;

  segmentType: SegmentType;
  sourceOrder: number;

  blockId?: string;
  pageId?: string;
  chapterId?: string;

  sourceRevision: number;
}
```

Every translation target must have a stable `segmentId`.

The translated result must reference the same identity.

---

## Translation Result

A successful request produces a structured result.

```ts
interface TranslationResult {
  requestId: string;

  sourceLanguage: string;
  targetLanguage: string;

  segments: TranslatedSegment[];

  provider: TranslationProviderInfo;
  usage?: TranslationUsage;

  startedAt: string;
  completedAt: string;

  requestRevision: number;
}
```

Each translated segment should be independently traceable.

```ts
interface TranslatedSegment {
  segmentId: string;

  sourceText: string;
  translatedText: string;

  sourceOrder: number;
  sourceRevision: number;

  status: TranslationSegmentStatus;

  warnings?: TranslationWarning[];
  metadata?: TranslationMetadata;
}
```

Possible segment statuses include:

```ts
type TranslationSegmentStatus =
  | "translated"
  | "partially_translated"
  | "unchanged"
  | "failed"
  | "cancelled";
```

---

## Translation Planning

Before calling a provider, the Translation layer creates a translation plan.

The plan determines:

* Which segments can be translated together
* Which provider should be used
* Which model or engine should be used
* Whether streaming is enabled
* Maximum request size
* Retry policy
* Timeout policy
* Output format
* Validation strategy

```ts
interface TranslationPlan {
  planId: string;
  requestId: string;

  providerId: string;
  modelId?: string;

  batches: TranslationBatch[];

  streaming: boolean;

  timeoutMs: number;
  maxAttempts: number;
}
```

Planning should be deterministic for the same request and configuration.

---

## Provider Abstraction

Translation providers must implement a common contract.

```ts
interface TranslationProvider {
  readonly providerId: string;

  getCapabilities(): TranslationProviderCapabilities;

  translate(
    request: ProviderTranslationRequest,
    signal?: AbortSignal
  ): Promise<ProviderTranslationResponse>;
}
```

Possible providers include:

* Local translation models
* Cloud AI models
* Dedicated machine translation APIs
* User-configured AI services
* Offline dictionaries or rule-based engines

The core architecture must not contain provider-specific logic.

Provider-specific behavior belongs inside provider adapters.

---

## Provider Capabilities

Different providers support different features.

```ts
interface TranslationProviderCapabilities {
  supportedSourceLanguages: string[];
  supportedTargetLanguages: string[];

  supportsAutoLanguageDetection: boolean;
  supportsStreaming: boolean;
  supportsStructuredOutput: boolean;
  supportsGlossary: boolean;
  supportsBatchTranslation: boolean;

  maxInputCharacters?: number;
  maxInputTokens?: number;
}
```

Provider selection must consider these capabilities.

---

## Provider Selection

A provider may be selected using:

* User preference
* Language support
* Content type
* Availability
* Privacy requirements
* Offline mode
* Request size
* Expected quality
* Cost limit
* Latency target
* Provider health

Example priority:

```text
Explicit User Selection
        │
        ▼
Compatible Preferred Provider
        │
        ▼
Compatible Local Provider
        │
        ▼
Compatible Cloud Provider
        │
        ▼
Fallback Provider
```

The selected provider must support the requested source and target languages.

---

## Translation Strategy

Translation behavior may vary by content type.

Possible strategies include:

```ts
type TranslationStrategy =
  | "novel"
  | "comic_dialogue"
  | "comic_caption"
  | "sound_effect"
  | "title"
  | "literal"
  | "natural"
  | "generic";
```

Examples:

* Novel translation prioritizes continuity and natural prose.
* Comic dialogue prioritizes brevity and character voice.
* Comic captions prioritize clarity and available display space.
* Sound effects may be translated, transliterated, explained, or preserved.

The strategy influences execution instructions but must not modify the source text model.

---

## Translation Batch

A translation batch groups compatible segments into one provider request.

```ts
interface TranslationBatch {
  batchId: string;
  requestId: string;

  segments: TranslationTarget[];

  context: TranslationContext;

  estimatedCharacters: number;
  estimatedTokens?: number;

  batchOrder: number;
}
```

Segments may be grouped when they share:

* Source language
* Target language
* Translation strategy
* Chapter or page context
* Provider
* Model
* User preferences

Segments requiring different strategies should normally be placed in separate batches.

---

## Segment and Batch Separation

A text segment is a semantic translation unit.

A translation batch is an execution unit.

```text
Text Segment
    ≠
Translation Batch
```

One batch may contain multiple segments.

A segment should not be divided across batches unless its size exceeds provider limits and no safer strategy is available.

Batch construction must not change logical segment order.

---

## Batch Size

Batch size should balance:

* Translation quality
* Context continuity
* Provider input limits
* Response reliability
* Latency
* Cost
* Cancellation responsiveness

A batch policy may define:

```ts
interface TranslationBatchPolicy {
  maxSegments: number;
  maxCharacters: number;
  maxEstimatedTokens?: number;

  keepParagraphTogether: boolean;
  keepDialogueGroupTogether: boolean;
  keepPanelTogether: boolean;
}
```

The current segment identity must remain preserved even when multiple segments are sent together.

---

## Provider Request Mapping

The provider adapter converts a translation batch into the provider's native request format.

```text
Translation Batch
        │
        ▼
Provider Adapter
        │
        ▼
Provider-Specific Request
```

The mapping may include:

* Translation instructions
* Current target segments
* Supporting context
* Glossary entries
* Character information
* User preferences
* Expected structured output schema

Provider request formatting must remain outside the core translation domain.

---

## Target Isolation

Supporting context must not be mistaken for translation targets.

The request should clearly distinguish:

```text
Content to Translate
```

from:

```text
Context for Understanding Only
```

The provider must return translations only for target segment identifiers.

This prevents neighboring segments from being translated again or returned as duplicated content.

---

## Structured Output

When supported, providers should return structured output.

Example:

```json
{
  "translations": [
    {
      "segmentId": "segment-101",
      "translatedText": "Hắn đã trở lại."
    },
    {
      "segmentId": "segment-102",
      "translatedText": "Mọi người lập tức im lặng."
    }
  ]
}
```

Structured output is preferred because it provides stable segment mapping.

Free-form text responses require additional parsing and should be treated as less reliable.

---

## Translation Execution

Execution sends each batch to the selected provider.

```text
Ready Batch
        │
        ▼
Execution Started
        │
        ├── Success
        │
        ├── Retryable Failure
        │
        ├── Permanent Failure
        │
        └── Cancellation
```

Independent batches may execute concurrently when:

* The provider allows concurrency
* Rate limits are respected
* Logical output order can still be reconstructed
* Shared context does not require sequential translation
* Resource limits are respected

---

## Sequential Translation

Some content benefits from sequential translation.

Examples:

* Long novel conversations
* Pronoun-sensitive passages
* Segments depending on previous translated terminology
* Progressive glossary generation

Sequential execution may be required when the result of one batch becomes context for the next batch.

This behavior should be explicit in the translation plan.

---

## Parallel Translation

Independent batches may execute in parallel.

Examples:

* Unrelated comic panels
* Separate pages
* Independent captions
* Previously contextualized segments

Parallel execution must not affect the final logical order.

Results should be reordered using `sourceOrder`, not completion time.

---

## Streaming

Some providers may return partial translation output.

```ts
interface TranslationStreamEvent {
  requestId: string;
  batchId: string;
  segmentId?: string;

  type:
    | "started"
    | "partial"
    | "segment_completed"
    | "completed"
    | "failed";

  textDelta?: string;
}
```

Streaming can improve perceived latency, especially for novels.

However, partial output must be marked provisional until the complete segment has been validated.

Presentation may display provisional text only when explicitly supported.

---

## Cancellation

Translation must support cancellation.

Cancellation may occur because of:

* User action
* Navigation to another page
* Chapter change
* A newer request replacing an older request
* Application shutdown
* Timeout
* Provider failover

Cancellation should propagate through:

```text
Translation Request
        │
        ▼
Translation Plan
        │
        ▼
Batch Execution
        │
        ▼
Provider Request
```

Cancelled results must not be presented as completed translations.

---

## Timeout

Each provider request should have a configurable timeout.

```ts
interface TranslationExecutionOptions {
  timeoutMs?: number;
  maxAttempts?: number;
  streaming?: boolean;
  priority?: TranslationPriority;
}
```

Timeouts may differ between:

* Interactive comic translation
* Background page translation
* Novel chapter translation
* Local models
* Cloud providers

Interactive reading should favor shorter timeouts and faster fallback behavior.

---

## Retry

Only retry failures that may succeed on another attempt.

Retryable failures may include:

* Temporary network errors
* Provider overload
* Rate limiting
* Temporary service unavailability
* Incomplete structured output

Non-retryable failures may include:

* Unsupported language
* Invalid API credentials
* Invalid request format
* Content exceeding hard provider limits
* Permanently rejected content

Retries should use bounded exponential backoff with optional jitter.

A retry must preserve the same request identity while incrementing its execution attempt.

---

## Provider Fallback

When the preferred provider fails, CRAI may use a fallback provider.

Fallback should occur only when:

* The user allows fallback
* A compatible provider exists
* Privacy requirements remain satisfied
* The translation strategy is supported
* The request is still current

Fallback provider usage must be included in translation metadata.

A local-only request must never fall back to a cloud provider without explicit permission.

---

## Response Parsing

The provider adapter converts the provider response into canonical translated segments.

```text
Provider Response
        │
        ▼
Provider-Specific Parser
        │
        ▼
Canonical Translation Result
```

The parser must verify:

* Response format
* Segment identifiers
* Required fields
* Output count
* Empty translations
* Duplicate segment results
* Unexpected additional content

Unknown provider output must not be silently applied.

---

## Result Validation

Translated output should be validated before publication.

Validation may include:

* Every required segment has a result
* Every returned identifier exists in the request
* No duplicate segment identifier exists
* Translation text is not unexpectedly empty
* Source and translated text are not accidentally swapped
* Mandatory glossary terms are respected
* Output does not contain request instructions
* Output does not contain unrelated context
* Output remains within expected size bounds

Validation should detect obvious failures, not judge literary quality as an absolute fact.

---

## Source Equality

A translated result may be identical to the source when:

* The source is a name
* The source is a number
* The source is punctuation
* The source language is already the target language
* The strategy intentionally preserves the content

Source equality should create a warning only when unexpected.

It should not automatically be treated as failure.

---

## Partial Results

A request may return a mixture of successful and failed segments.

```ts
interface TranslationBatchResult {
  batchId: string;

  successfulSegments: TranslatedSegment[];
  failedSegments: TranslationSegmentFailure[];

  status: "completed" | "partial" | "failed";
}
```

Successful segments may be published while failed segments are retried independently when safe.

Partial publication must preserve ordering and clearly identify unavailable translations.

---

## Segment Mapping

Translated text must be mapped back to the original segment using `segmentId`.

Position-based mapping should only be used as a controlled fallback when:

* The provider cannot return identifiers
* Output count matches input count
* Order is guaranteed
* Validation succeeds

Identifier-based mapping is always preferred.

---

## Stale Result Protection

A result must not be applied when its source has changed.

Before accepting a result, CRAI should verify:

* Request ID
* Request revision
* Segment ID
* Segment revision
* Context version
* Active page or chapter state, when relevant

Example:

```text
Translation Started
        │
        ▼
User Corrects OCR Text
        │
        ▼
Source Revision Changes
        │
        ▼
Old Translation Arrives
        │
        ▼
Result Rejected as Stale
```

This prevents outdated translations from replacing newer content.

---

## Translation Metadata

A result may include execution metadata.

```ts
interface TranslationMetadata {
  providerId: string;
  modelId?: string;

  strategy: TranslationStrategy;

  attemptCount: number;
  fallbackUsed: boolean;
  streamed: boolean;

  contextVersion: number;
  translationVersion: number;
}
```

Metadata supports debugging, quality comparison, caching, and reproducibility.

---

## Usage Tracking

Cloud or AI providers may report usage.

```ts
interface TranslationUsage {
  inputCharacters?: number;
  outputCharacters?: number;

  inputTokens?: number;
  outputTokens?: number;

  estimatedCost?: number;
  currency?: string;
}
```

Usage tracking should not be required for providers that do not expose this data.

Cost information should be clearly marked as estimated unless confirmed by the provider.

---

## Translation Preferences

User preferences may affect translation behavior.

```ts
interface TranslationPreferences {
  style?: "literal" | "balanced" | "natural";

  preserveNames?: boolean;
  preserveHonorifics?: boolean;
  preserveFormatting?: boolean;

  translateSoundEffects?: boolean;
  useVietnamesePronouns?: boolean;

  customInstructions?: string;
}
```

Preferences should be converted into structured rules where possible.

Free-form instructions should be treated as optional extensions rather than the only source of translation behavior.

---

## Chinese-to-Vietnamese Considerations

The initial CRAI translation flow prioritizes Chinese-to-Vietnamese content.

Important considerations include:

* Simplified and Traditional Chinese
* Character name consistency
* Sino-Vietnamese terminology
* Personal pronouns
* Family and social titles
* Cultivation levels
* Martial arts techniques
* Sect and organization names
* Historical titles
* Idioms
* Internet slang
* Vertical comic text
* Omitted subjects
* Context-dependent gender

The Translation layer should use glossary, character, and context information rather than applying isolated word replacement.

It must preserve uncertainty when names, speakers, or pronouns cannot be reliably resolved.

---

## Novel Translation

Novel translation should prioritize:

* Paragraph continuity
* Narrative viewpoint
* Character voice
* Dialogue relationships
* Terminology consistency
* Natural Vietnamese sentence flow
* Preservation of paragraph identity

Translation may operate on several segments in one batch while still returning one result per segment.

For long chapters, translation should be incremental rather than waiting for the entire chapter.

---

## Comic Translation

Comic translation should prioritize:

* Short and readable wording
* Dialogue tone
* Panel reading order
* Speech bubble relationships
* Character voice
* Available presentation space
* Sound-effect strategy

The Translation layer may receive presentation constraints, such as preferred maximum length, but it must not directly resize or render text.

A shorter translation must not remove essential meaning merely to fit an overlay.

---

## Translation Quality Signals

CRAI may collect non-authoritative quality signals.

Examples:

* Missing glossary term
* Suspiciously short output
* Suspiciously long output
* Unchanged source text
* Mixed-language output
* Unresolved name
* Provider parser fallback
* User correction frequency

These signals may produce warnings or request review.

They should not be represented as objective translation scores unless the measurement method is explicitly defined.

---

## User Corrections

Users may edit translated text.

A manual correction should create a new translation revision rather than silently modifying the provider result.

```ts
interface TranslationRevision {
  translationId: string;
  revision: number;

  translatedText: string;
  source: "provider" | "user" | "system";

  createdAt: string;
}
```

Manual translations must take precedence over automatic translations until the user removes or resets the override.

User corrections may later contribute to glossary or translation-memory suggestions, subject to user consent.

---

## Translation Memory

Translation memory stores reusable source-to-target translation pairs.

It may help with:

* Repeated terms
* Repeated dialogue
* Common interface text
* Character titles
* Previously translated passages

Translation memory should be treated as a candidate source, not unquestionable truth.

A reused translation must remain compatible with:

* Current context
* Current glossary
* Current preferences
* Current source revision

Advanced translation memory is outside the initial MVP.

---

## Caching

Translation results may be cached using a deterministic fingerprint.

Possible fingerprint inputs include:

```text
Source Text
+ Source Language
+ Target Language
+ Segment Type
+ Context Version
+ Glossary Revision
+ Preference Revision
+ Translation Strategy
+ Provider
+ Model
+ Translation Engine Version
```

A cached result must not be reused when relevant translation inputs have changed.

Manual corrections should be stored separately from automatic cache entries.

---

## Error Model

Translation errors should use stable error codes.

```ts
type TranslationErrorCode =
  | "INVALID_REQUEST"
  | "EMPTY_SOURCE_TEXT"
  | "UNSUPPORTED_LANGUAGE"
  | "NO_COMPATIBLE_PROVIDER"
  | "PROVIDER_UNAVAILABLE"
  | "PROVIDER_AUTHENTICATION_FAILED"
  | "PROVIDER_RATE_LIMITED"
  | "REQUEST_TOO_LARGE"
  | "TRANSLATION_TIMEOUT"
  | "TRANSLATION_CANCELLED"
  | "INVALID_PROVIDER_RESPONSE"
  | "SEGMENT_MAPPING_FAILED"
  | "RESULT_VALIDATION_FAILED"
  | "STALE_RESULT"
  | "UNKNOWN_TRANSLATION_ERROR";
```

Errors should include:

* Request ID
* Batch ID
* Provider ID
* Retryability
* Affected segment IDs
* Safe diagnostic information

Provider secrets and full private content must not appear in error logs.

---

## Events

The Translation Architecture may publish events such as:

```text
TranslationRequested
TranslationPlanningStarted
TranslationPlanCreated
TranslationBatchStarted
TranslationBatchProgressed
TranslationSegmentCompleted
TranslationBatchCompleted
TranslationRetryScheduled
TranslationProviderFallbackUsed
TranslationCompleted
TranslationPartiallyCompleted
TranslationFailed
TranslationCancelled
TranslationResultRejectedAsStale
```

Events should contain identifiers and status information rather than unnecessary raw text.

---

## Observability

Useful metrics include:

* Translation request count
* Translation success rate
* Partial completion rate
* Provider failure rate
* Retry count
* Fallback count
* Average translation latency
* Time to first streamed text
* Characters or tokens processed
* Estimated provider cost
* Cache hit rate
* Cancellation rate
* Stale result count
* Validation warning count

Metrics should be separable by provider, model, strategy, and content type.

---

## Privacy

Translation content may contain private or copyrighted material.

The architecture should:

* Avoid logging raw text by default
* Clearly identify when content is sent to a cloud provider
* Respect local-only mode
* Minimize provider payloads
* Send only relevant context
* Protect provider credentials
* Allow users to clear cached translations
* Avoid using user content for unrelated model training without explicit consent

Provider privacy policies should be surfaced through provider configuration, not hidden by the adapter.

---

## Security

All source and context text must be treated as untrusted input.

Provider instructions must clearly separate system-controlled translation rules from source content.

Text extracted from documents or websites must not be allowed to override:

* Provider configuration
* Security rules
* Output schema
* Privacy settings
* Translation target boundaries

Provider responses must be validated before use.

---

## Performance

Interactive translation should prioritize perceived responsiveness.

Recommended behavior:

* Translate visible content first
* Use small initial batches
* Allow cancellation
* Stream results when safe
* Prefetch nearby segments only after visible content
* Reuse valid cached translations
* Limit concurrent provider calls
* Avoid rebuilding unchanged context

Background translation may use larger batches for efficiency.

---

## MVP Scope

The first implementation should support:

* Chinese-to-Vietnamese translation
* One local or cloud provider adapter
* Provider-independent contracts
* Translation of one or more segments
* Basic batch construction
* Stable segment ID mapping
* Configurable timeout
* Cancellation
* Limited retry
* Basic structured-output validation
* Partial failure handling
* Stale-result protection
* Basic translation caching
* User editing of translated text

The MVP does not require:

* Automatic provider benchmarking
* Advanced semantic translation memory
* Automatic quality scoring
* Character relationship inference
* Complex multi-provider consensus
* Automatic glossary learning
* Cross-book context retrieval

---

## Future Extensions

Possible future capabilities include:

* Multiple provider routing
* Local-first hybrid translation
* Provider quality comparison
* Automatic provider failover
* Semantic translation memory
* Style-preserving translation
* Character-specific language profiles
* Automatic glossary suggestions
* Translation alternatives
* AI-assisted revision
* Quality estimation
* Cross-chapter terminology retrieval
* Adaptive batching
* Cost-aware model selection
* Offline model downloads

These capabilities should extend the provider and planning contracts without changing the core segment identity model.

---

## Design Principles

### Provider Independence

Core translation contracts must not depend on one provider, model, or API format.

### Segment Identity Preservation

Every translation must remain mapped to its original text segment.

### Context Awareness

Translation should use prepared context rather than translating isolated text whenever relevant context is available.

### Deterministic Planning

The same inputs and configuration should create the same translation plan.

### Explicit Execution Policy

Batch size, retries, timeout, fallback, and concurrency must be explicit.

### Safe Partial Completion

Successful segments should remain usable when other segments fail.

### Stale Result Protection

Outdated translations must never replace newer source revisions.

### User Authority

Manual corrections and explicit terminology rules take precedence over automatic results.

### Observable Execution

Every translation request should be traceable through planning, execution, validation, and completion.

### Privacy by Default

Only necessary content should be sent to external providers.

---

## Invariants

1. Every translated segment must reference an existing source segment.
2. Segment identity must remain stable throughout translation.
3. Translation results must be mapped by `segmentId` whenever possible.
4. Batch construction must not change logical source order.
5. Supporting context must not be returned as translated target content.
6. The Translation layer must not modify the source text model.
7. Provider-specific formats must remain inside provider adapters.
8. A provider must support the requested language pair before selection.
9. Cancelled translations must not be published as completed.
10. Stale results must not replace newer source revisions.
11. Manual translations must take precedence over automatic results.
12. Provider fallback must respect privacy and user settings.
13. Local-only requests must not silently use cloud providers.
14. Partial failures must identify affected segments.
15. Parallel execution must not change final output order.
16. Optional usage metadata must not be required for translation success.
17. Raw source content must not be logged by default.
18. Provider responses must be validated before publication.
19. Translation strategy must not remove source traceability.
20. Presentation constraints must not directly alter source segments.

---

## Related Documents

```text
../ocr/READING_ORDER.md
../text/TEXT_MODEL.md
../text/SEGMENTATION.md
CONTEXT.md
../presentation/PRESENTATION.md
```
