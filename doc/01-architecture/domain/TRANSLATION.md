# Translation Domain

* **Document:** Domain / Translation
* **Version:** 1.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

A Translation represents a validated target-language interpretation of one or more source Text Blocks.

It preserves the relationship between:

* Source content
* Source revisions
* Target language
* Translation configuration
* Context
* Glossary
* Generated output
* User corrections
* Review decisions

A Translation is a domain result.

It is not:

* A provider request
* A provider response
* A prompt
* A runtime job
* A cache entry
* A rendered text layer

Provider execution artifacts are normalized and validated before they become Translation domain records.

---

# Domain Role

The Translation domain connects source Text Blocks to user-visible target-language content.

```text
Text Blocks
     │
     ▼
Translation Input Snapshot
     │
     ▼
AI or Translation Provider
     │
     ▼
Normalized Response
     │
     ▼
Validated Translation
     │
     ├──► Review
     ├──► Presentation
     ├──► Rendering
     ├──► Export
     └──► Memory and Glossary Feedback
```

The Translation domain preserves business meaning and revision history without depending on a concrete provider, model or transport protocol.

---

# Ownership Boundary

A Translation belongs to exactly one Page.

```text
Page Aggregate
├── Text Blocks
├── Translation Groups
├── Translations
│   ├── Source References
│   ├── Translation Revisions
│   ├── Review State
│   ├── Quality Metadata
│   └── Active Revision
├── Render Layers
└── Diagnostics
```

A Translation may use contextual information from:

* Other Text Blocks on the Page
* Previous Pages
* Current Chapter
* Character registry
* Glossary
* Reading session
* Project preferences

Context usage does not transfer ownership.

The Translation remains owned by the Page containing its primary source Text Blocks.

---

# Responsibilities

A Translation is responsible for:

* Identifying translated output
* Referencing exact source Text Block revisions
* Declaring source and target languages
* Preserving translated text
* Tracking translation revisions
* Recording translation status
* Tracking review and approval
* Detecting stale results
* Preserving translation lineage
* Linking user corrections
* Supporting presentation and rendering
* Supporting translation history
* Recording quality and warning metadata
* Supporting deterministic cache validation
* Preserving translation alternatives when configured

A Translation is not responsible for:

* Building prompts
* Selecting a provider
* Executing AI requests
* Managing provider authentication
* Retrying network calls
* Performing routing or fallback
* Managing token budgets
* Rendering text into images
* Persisting provider credentials
* Orchestrating Page or Chapter processing

Those responsibilities belong to AI Pipeline, Provider, Runtime, Rendering, Preferences and Storage components.

---

# Identity

Every Translation has a stable identity.

Typical fields include:

* Translation ID
* Page ID
* Translation Group ID
* Source Language
* Target Language
* Translation Type
* Status
* Active Revision
* Created Time
* Updated Time
* Version

A Translation ID identifies one logical translation result associated with a stable source relationship.

Changing translated wording does not necessarily create a new Translation ID.

Instead, it normally creates a new Translation Revision.

A new Translation ID is required when:

* Source membership changes materially
* Text Blocks are split or merged
* Target language changes
* Translation purpose changes
* The previous record represents a different logical result
* Reconciliation cannot preserve identity safely

---

# Translation Types

Recommended translation types include:

| Type              | Description                                                     |
| ----------------- | --------------------------------------------------------------- |
| `direct`          | Standard source-to-target translation                           |
| `contextual`      | Translation generated using surrounding story context           |
| `literal`         | Translation prioritizing close source correspondence            |
| `natural`         | Translation prioritizing target-language readability            |
| `localized`       | Translation adapted to target cultural or stylistic conventions |
| `bilingual`       | Source and target content are presented together                |
| `summary`         | Condensed meaning rather than full translation                  |
| `explanation`     | Translation accompanied by explanatory information              |
| `transliteration` | Script or pronunciation conversion                              |
| `manual`          | Translation entered directly by a user                          |
| `imported`        | Translation imported from an external source                    |

Translation type represents semantic intent.

It must not be derived from a provider or model name.

---

# Source Association

A Translation must reference exact source input.

For a single Text Block:

```text
Translation
└── Text Block ID
    └── Text Block Revision
```

For contextual or grouped translation:

```text
Translation
└── Source Members
    ├── Text Block A / Revision 2
    ├── Text Block B / Revision 1
    └── Text Block C / Revision 4
```

Each source member should include:

* Text Block ID
* Text Block Revision
* Source Sequence
* Effective Source Text Hash
* Source Language
* Role in Translation
* Output Mapping Key

A Translation must never reference only the current mutable Text Block state.

It must preserve the exact revisions used during translation.

---

# Primary and Context Sources

Translation sources are classified by role.

| Role          | Description                                                   |
| ------------- | ------------------------------------------------------------- |
| `primary`     | Source content that must produce translated output            |
| `context`     | Supporting content used to improve interpretation             |
| `glossary`    | Approved terminology constraints                              |
| `character`   | Character information such as names, gender or speaking style |
| `history`     | Previous dialogue or narration                                |
| `instruction` | User or project translation instruction                       |

Only primary sources require direct output mapping.

Context sources influence translation but do not automatically produce separate Translation records.

---

# Translation Group

Several Text Blocks may be processed together using a Translation Group.

```text
Translation Group
├── Ordered Source Members
├── Context Scope
├── Group Revision
├── Translation Profile
└── Output Mapping Strategy
```

A Translation Group is responsible for:

* Preserving source order
* Defining translation context
* Defining grouping boundaries
* Supporting one provider request for several blocks
* Mapping output back to individual Text Blocks

A Translation Group does not own Text Blocks.

It references their exact revisions.

Group membership changes create a new Group Revision.

---

# Output Mapping

Grouped translation must preserve output-to-source mapping.

Recommended provider-independent shape:

```text
Translation Output
├── Item A
│   ├── Mapping Key: block-a
│   └── Target Text
├── Item B
│   ├── Mapping Key: block-b
│   └── Target Text
└── Item C
    ├── Mapping Key: block-c
    └── Target Text
```

Mapping must not depend solely on array position when stable mapping keys are available.

A response is invalid when:

* Required primary blocks are missing
* Unknown mapping keys are returned
* One output is assigned ambiguously
* Output order conflicts with declared mapping
* Several blocks are collapsed without an allowed mapping strategy

---

# Translation Representations

A Translation may preserve several text representations.

```text
Raw Provider Output
        │
        ▼
Normalized Translation
        │
        ▼
Post-Processed Translation
        │
        ▼
User-Corrected Translation
        │
        ▼
Effective Translation
```

## Raw Provider Output

The unmodified provider result.

It is useful for:

* Diagnostics
* Reproducibility
* Response repair
* Provider comparison
* Audit

Raw provider output is not automatically trusted.

It may use provider-specific structure and should normally be stored separately from the canonical Translation.

## Normalized Translation

Provider output converted into the CRAI canonical response format.

Normalization may include:

* Schema conversion
* Mapping-key resolution
* Unicode normalization
* Whitespace normalization
* Provider metadata extraction
* Removal of protocol artifacts

Normalization must not silently change semantic meaning.

## Post-Processed Translation

Translation after deterministic CRAI rules are applied.

Post-processing may include:

* Approved glossary enforcement
* Punctuation normalization
* Spacing normalization
* Formatting cleanup
* Honorific normalization
* Script conversion
* Safe line-break handling

## User-Corrected Translation

An optional translation explicitly edited or approved by the user.

User-corrected translation has higher authority than generated output.

## Effective Translation

The text consumed by Presentation, Rendering and Export.

Resolution order:

```text
Approved User Correction
        ↓ fallback
Current User Correction
        ↓ fallback
Post-Processed Translation
        ↓ fallback
Normalized Translation
```

Raw provider output is not used directly for presentation unless explicitly permitted for diagnostics.

---

# Translation Revision

Translation identity and revision are separate.

```text
Translation ID: stable logical translation
Revision:       version of translated content and decisions
```

A Translation Revision should include:

* Translation ID
* Revision Number
* Parent Revision
* Target Text
* Source Snapshot
* Source Content Hash
* Translation Profile Revision
* Context Revision
* Glossary Revision
* Prompt Template Revision
* Post-Processing Revision
* Creation Source
* Created Time
* Actor
* Status
* Quality Metadata

A revision must be immutable after publication.

Any processing-significant change creates a new revision.

---

# Revision Creation Sources

A Translation Revision may originate from:

| Source             | Description                                         |
| ------------------ | --------------------------------------------------- |
| `provider`         | Created from an automated provider result           |
| `user_edit`        | Created by direct user correction                   |
| `review_edit`      | Created during formal review                        |
| `glossary_reapply` | Created after applying updated terminology          |
| `post_process`     | Created by a deterministic post-processing revision |
| `import`           | Imported from an external translation source        |
| `migration`        | Created during schema or data migration             |
| `reconciliation`   | Preserved or rebuilt after source regeneration      |

Creation source describes lineage and authority.

It does not determine approval automatically.

---

# Translation Profile

Translation behavior is configured through a versioned Translation Profile.

Typical profile fields include:

* Translation Profile ID
* Profile Revision
* Translation Type
* Source Language
* Target Language
* Preferred Style
* Formality
* Honorific Policy
* Name Policy
* Sound-Effect Policy
* Localization Policy
* Formatting Rules
* Context Policy
* Glossary Policy
* Provider Preference
* Quality Policy

The Translation domain stores a reference to the effective profile revision.

It does not resolve configuration hierarchy itself.

The Preferences component produces the effective configuration used for execution.

---

# Style

Translation style may include:

* Literal
* Natural
* Concise
* Descriptive
* Formal
* Informal
* Comic dialogue
* Novel prose
* Technical
* Historical
* Child-friendly
* User-defined

Style is a domain-level translation intent.

Concrete prompt wording remains an AI Pipeline concern.

The same style must be expressible across different providers.

---

# Language Pair

Every Translation declares:

* Effective Source Language
* Target Language

Optional fields may include:

* Source Script
* Target Script
* Regional Variant
* Mixed-Language Strategy
* Transliteration Policy

Examples:

```text
zh-Hans → vi
zh-Hant → vi
en → vi
ja → vi
ko → vi
```

A target language change creates a different logical Translation.

Different target languages must not share the same active Translation record.

---

# Glossary Association

A Translation may reference a Glossary Snapshot.

```text
Glossary Snapshot
├── Glossary ID
├── Glossary Revision
├── Selected Entry IDs
├── Applied Entry Revisions
└── Snapshot Hash
```

The Translation should record which glossary entries were applied when practical.

Glossary changes do not silently mutate existing Translation revisions.

Instead, affected translations may become stale or eligible for regeneration.

Approved user translation may override a general glossary rule when explicitly recorded.

---

# Character Context

Character information may influence:

* Name translation
* Pronouns
* Gendered language
* Formality
* Honorifics
* Speech style
* Relationship terms
* Repeated catchphrases

A Translation may record a Character Context Snapshot containing:

* Character IDs
* Character Revisions
* Speaker Mapping
* Relevant Aliases
* Applied Style Rules
* Snapshot Hash

Character changes may invalidate translations only when the changed information affects their output.

---

# Context Snapshot

A Translation must preserve the effective context identity used during generation.

A Context Snapshot may include:

* Context Revision
* Current Page
* Current Chapter
* Previous Text Blocks
* Previous Translation References
* Character References
* Glossary References
* Session Preferences
* User Instructions
* Context Hash

The snapshot does not necessarily store all raw context inline.

It may reference immutable context records or hashes.

Context must be sufficient to explain why a translation may differ from another generation of the same source text.

---

# Lifecycle

```text
Requested
    │
    ▼
Generating
    │
    ▼
Validating
    │
    ├──► Failed
    │
    ▼
Generated
    │
    ▼
Post Processed
    │
    ├──► Needs Review
    │
    └──► Ready
              │
       ┌──────┼─────────┐
       ▼      ▼         ▼
   Approved  Stale   Superseded
```

Lifecycle meaning:

* `Requested`: Translation intent has been created.
* `Generating`: An execution is in progress.
* `Validating`: Normalized provider output is being validated.
* `Generated`: A valid normalized output exists.
* `Post Processed`: Deterministic output processing has completed.
* `Ready`: Translation may be used by Presentation or Rendering.
* `Needs Review`: Translation requires user or reviewer attention.
* `Approved`: Translation has been explicitly accepted.
* `Stale`: One or more required inputs have changed.
* `Superseded`: A newer Translation or incompatible source relationship replaced it.
* `Failed`: No valid translation result was produced.

Runtime retry attempts do not create separate lifecycle states in the domain unless their final outcome changes business state.

---

# Active Revision

A Translation may have many revisions but only one active revision for a given usage context.

```text
Translation
├── Revision 1
├── Revision 2
├── Revision 3
└── Active Revision: 3
```

The active revision must:

* Belong to the Translation
* Reference compatible source revisions
* Match the target language
* Pass required validation
* Not be superseded
* Not be stale unless stale usage is explicitly allowed

Changing the active revision is a domain operation and should emit an event.

---

# Staleness

A Translation becomes stale when one or more translation-significant inputs change.

Typical stale causes include:

* Effective source text changed
* Text Block revision changed
* Text Blocks were split or merged
* Translation Group membership changed
* Source language changed
* Target language changed
* Translation Profile changed
* Glossary changed
* Character information changed
* User instruction changed
* Context policy changed
* Prompt template changed
* Post-processing policy changed
* Safety or validation policy changed

Not every configuration change must invalidate every Translation.

Staleness should be determined from explicit dependency impact.

---

# Stale Impact Classification

Recommended impact categories:

| Impact               | Description                                              |
| -------------------- | -------------------------------------------------------- |
| `none`               | Change cannot affect translation output                  |
| `presentation_only`  | Translation remains valid; only display must refresh     |
| `review_recommended` | Existing output may remain usable but should be reviewed |
| `translation_stale`  | Translation must be regenerated or reapproved            |
| `source_invalid`     | Translation source relationship is no longer valid       |

Examples:

* Font-size change: `presentation_only`
* Glossary entry for unrelated term: `none`
* Character gender correction used in dialogue: `translation_stale`
* Text Block split: `source_invalid`
* Style change from literal to natural: `translation_stale`

---

# Source Hash

A Translation should contain a deterministic source hash.

The source hash may include:

* Ordered primary Text Block IDs
* Text Block revisions
* Effective source texts
* Source languages
* Translation-significant block types
* Translation Group revision

The source hash must not depend on provider execution metadata.

It supports:

* Stale detection
* Cache lookup
* Request deduplication
* Idempotent processing
* Revision comparison
* Audit

---

# Configuration Hash

A separate configuration hash may include:

* Translation Profile revision
* Target language
* Glossary snapshot
* Character context snapshot
* Context policy
* Prompt template revision
* Output schema revision
* Post-processing revision
* Safety policy revision

Translation cache validity should normally require both:

```text
Source Hash
+
Configuration Hash
```

Provider identity may be included in a separate execution hash when provider-specific reproducibility is required.

---

# Translation Alternatives

CRAI may retain several translation alternatives.

```text
Translation
├── Alternative A: literal
├── Alternative B: natural
├── Alternative C: user-edited
└── Active Alternative: C
```

Alternatives may differ by:

* Style
* Provider
* Model
* Context
* Glossary application
* User correction
* Translation strategy

Each alternative must remain traceable to its own source and configuration snapshots.

Alternatives must not be silently merged.

---

# Manual Correction

Users may edit translated text.

Correction rules:

1. Existing Translation Revisions remain immutable.
2. A correction creates a new revision.
3. The correction records its parent revision.
4. The editor and timestamp are preserved.
5. The corrected revision becomes active according to policy.
6. Rendering and Presentation refresh only affected outputs.
7. Correction does not alter source Text Blocks.
8. Correction may optionally create glossary or memory candidates.
9. Automatic regeneration must not overwrite an approved correction silently.

User corrections may be classified as:

* Typographic
* Terminology
* Name correction
* Grammar
* Style
* Meaning correction
* Missing content
* Formatting
* Other

Classification supports quality evaluation and reusable learning.

---

# Approval and Review

Translation review state is separate from translation generation state.

Recommended review statuses:

| Status              | Description                              |
| ------------------- | ---------------------------------------- |
| `unreviewed`        | No review decision exists                |
| `review_requested`  | Review is required                       |
| `in_review`         | A reviewer is evaluating the translation |
| `changes_requested` | Translation requires correction          |
| `approved`          | Translation is accepted                  |
| `rejected`          | Translation must not be used             |
| `waived`            | Review was intentionally skipped         |

Review metadata may include:

* Reviewer ID
* Review Time
* Reviewed Revision
* Decision
* Comments
* Issue Categories
* Quality Score

Approval applies to an exact Translation Revision.

A newer revision is not automatically approved.

---

# Quality Metadata

A Translation may record quality indicators.

Possible fields include:

* Provider Confidence
* Language Validation Result
* Glossary Compliance
* Source Coverage
* Mapping Completeness
* Warning Count
* Review Score
* User Correction Count
* Suspected Hallucination
* Suspected Omission
* Suspected Added Meaning
* Fluency Score
* Consistency Score

Provider confidence and CRAI validation confidence must remain distinguishable.

Quality metadata assists review and diagnostics.

It does not automatically prove correctness.

---

# Validation

Before entering the `Ready` state, a Translation must pass validation.

Validation may include:

* Translation ID is present
* Page ownership is valid
* Primary source list is not empty
* Every primary Text Block exists
* Every source revision exists
* Source hashes match
* Target language is valid
* Effective translation is not empty
* Output mapping is complete
* No unknown mapping keys exist
* Translation revision is immutable
* Translation Profile reference is valid
* Required glossary constraints are satisfied
* Output schema is valid
* Safety policy is satisfied
* Length limits are respected
* Rejected output is not activated
* Superseded source is not used as current input

Warnings may be accepted when policy allows.

Validation errors prevent activation.

---

# Completeness Validation

For grouped translation, completeness validation should verify:

```text
Expected Primary Source Keys
            =
Returned Translation Keys
```

Allowed exceptions must be explicit.

Examples:

* A sound effect may intentionally remain untranslated.
* Two source blocks may be merged into one target block when the mapping strategy permits it.
* One source paragraph may be split into several display segments.

The output must still preserve deterministic source association.

---

# Language Validation

Language validation may verify:

* Output primarily uses the target language
* Required source terms remain unchanged
* Forbidden scripts are absent
* Transliteration follows policy
* Mixed-language output is intentional
* Provider explanations are not included accidentally
* JSON or formatting syntax is not exposed as translated text

Language validation should allow names, terminology and quoted source text according to profile rules.

---

# Terminology Validation

Terminology validation may check:

* Required glossary entries
* Locked character names
* Location names
* Organization names
* Skills and item names
* Honorific policy
* Repeated terminology consistency
* Forbidden translations

Terminology violations may:

* Trigger deterministic correction
* Mark the Translation as `Needs Review`
* Reject the generated revision
* Request regeneration

Policy determines the appropriate response.

---

# Failure Handling

Typical domain-level failures include:

* Source Text Block missing
* Source revision mismatch
* Empty translation
* Invalid target language
* Incomplete output mapping
* Unknown output mapping key
* Invalid Translation Profile
* Invalid glossary reference
* Context snapshot unavailable
* Output schema invalid
* Translation too long
* Translation too short
* Language validation failed
* Safety validation failed
* Revision conflict
* Approved revision overwrite attempted
* Stale Translation activated
* Superseded source used
* Review state conflict

Provider-specific network, authentication, rate-limit and model errors must be translated into stable execution errors before entering the Translation domain.

---

# Interaction with Provider Execution

The domain and execution layers must remain separate.

```text
Translation Intent
        │
        ▼
Canonical AI Request
        │
        ▼
Provider Adapter
        │
        ▼
Raw Provider Response
        │
        ▼
Canonical AI Response
        │
        ▼
Validation
        │
        ▼
Translation Revision
```

Provider execution metadata may include:

* Provider ID
* Model ID
* Model Version
* Request ID
* Attempt Count
* Fallback Usage
* Token Usage
* Cost
* Latency
* Finish Reason

This metadata is useful for diagnostics and lineage.

It must not become required business logic for consuming a Translation.

---

# Retry and Fallback

Retries and fallback belong to runtime execution.

Domain rules:

* Retry attempts do not create duplicate Translation identities.
* All attempts preserve the same logical request identity.
* Failed attempts may be recorded in diagnostics.
* Only validated successful output creates a Translation Revision.
* Fallback provider usage is recorded as lineage metadata.
* A late result must not overwrite a newer active revision.
* Cancelled work must not publish a Translation Revision.
* Duplicate successful results must be reconciled idempotently.

---

# Streaming

Streaming output is provisional until final validation.

```text
Streaming Chunks
      │
      ▼
Provisional Assembly
      │
      ▼
Partial Presentation
      │
      ▼
Final Validation
      │
      ▼
Published Translation Revision
```

Partial streamed text may be displayed temporarily.

It must not become a durable Translation Revision unless:

* The stream completes
* Partial-result policy explicitly permits it
* The result passes validation
* Its incomplete status is preserved

Final streaming and non-streaming execution must produce equivalent Translation domain records for equivalent output.

---

# Cache Interaction

Translation cache is an optimization layer.

A cached result may be used only when compatible with:

* Source Hash
* Configuration Hash
* Output Schema Revision
* Validation Policy Revision
* Target Language
* Translation Type

Cache behavior must not bypass:

* Domain validation
* Source revision checks
* Approval protection
* Stale detection
* Safety rules

A cache entry is not the source of truth.

The Translation Revision is the durable domain record.

---

# Presentation Association

Presentation consumes the effective active Translation Revision.

Recommended mapping:

```text
Text Block
    │
    ├──► Translation
    │       └── Active Revision
    │
    └──► Presentation Item
            ├── Translation Revision ID
            ├── Display Mode
            ├── Source Marker
            └── Layout Metadata
```

Presentation may use:

* Side panel
* Overlay
* Bilingual view
* Reader view
* Tooltip
* Subtitle-like list
* Export view

Presentation settings must not modify Translation content.

Any presentation-specific text shortening must be represented as a derived display value, not as the canonical Translation.

---

# Rendering Association

Rendering uses an exact Translation Revision.

A Render Layer should reference:

* Translation ID
* Translation Revision
* Text Block ID
* Source Geometry Revision
* Rendering Profile Revision

If the active Translation changes, dependent Render Layers become stale.

Previously exported immutable assets remain historical outputs.

---

# Import

Imported translations must preserve:

* Import Source
* Original Identifier
* Source Text association
* Target Language
* Imported Text
* Import Time
* Import Revision
* Trust Level
* Review Status

Imported text must not be treated as approved automatically unless import policy explicitly permits it.

When source mapping is uncertain, the imported result should require review.

---

# Export

Translation export may include:

* Plain translated text
* Bilingual text
* Ordered Text Block translations
* Structured JSON
* Subtitle-like format
* Chapter document
* Translation memory format
* Rendered image association

Exports must reference an exact Translation Revision.

Changing the active Translation later does not mutate previous immutable exports.

---

# Persistence

Recommended persistent separation:

```text
Translation Record
├── identity
├── page ownership
├── target language
├── translation type
├── lifecycle status
├── active revision
└── review summary

Translation Revision
├── immutable target text
├── source snapshot
├── source hash
├── configuration hash
├── context references
├── glossary references
├── lineage
├── quality metadata
└── creation metadata

Translation Execution
├── request identity
├── provider attempts
├── model metadata
├── latency
├── token usage
├── cost
└── errors

Translation Review
├── reviewed revision
├── reviewer
├── decision
├── issues
└── comments
```

Execution records may use a shorter retention period than domain revisions.

Provider payloads should not be stored inside the canonical Translation record.

---

# Retention

Suggested retention policy:

* Active Translation revisions: retain while the Page exists
* Approved revisions: durable retention
* User corrections: durable retention
* Superseded revisions: retain according to history policy
* Failed execution records: retain for diagnostics
* Raw provider responses: short-lived unless debugging is enabled
* Token and cost metadata: retain according to observability policy
* Stale translations: retain when useful for comparison or rollback
* Partial streaming output: discard unless explicitly retained
* Imported translations: retain according to source and licensing policy

Deleting a Translation must not leave dangling Presentation, Rendering, Review or Export references.

---

# Privacy

Translations may contain private or copyrighted content.

Requirements:

* Do not log source or translated text by default.
* Send only required context to remote providers.
* Respect provider retention and training policies.
* Protect user corrections and reading history.
* Allow local-only translation profiles where supported.
* Support temporary sessions without durable persistence.
* Do not expose glossary or character context across Projects.
* Exclude raw content from general metrics and traces.
* Apply deletion and retention policies consistently to provider payloads.

Telemetry should prefer:

* Identifiers
* Hashes
* Text lengths
* Language pairs
* Durations
* Token counts
* Status
* Error categories

---

# Idempotency

Translation creation must be idempotent for the same logical input.

An idempotency key may include:

* Page ID
* Translation Group Revision
* Source Hash
* Configuration Hash
* Target Language
* Translation Type
* Request Purpose

Repeated execution with the same key should not create uncontrolled duplicate Translation records.

A new valid result may create:

* No change, when content is identical
* A new alternative
* A new Translation Revision
* A provider comparison record

The selected behavior must be explicit.

---

# Concurrency

Several translation operations may run concurrently.

Concurrency rules:

1. Every operation reads an immutable source snapshot.
2. Every operation preserves a request revision.
3. Late results are checked before publication.
4. Results based on stale source cannot become active automatically.
5. User corrections cannot be overwritten silently.
6. Only one active revision change is committed atomically.
7. Duplicate executions are reconciled idempotently.
8. Cancellation prevents publication when possible.

Example:

```text
Request A starts from Text Block Revision 2
Request B starts from Text Block Revision 3

Request A finishes after Request B

Request A result:
- may be retained as historical
- must not replace the Revision 3 result
```

---

# Reconciliation

Translations may require reconciliation after Text Block regeneration.

Possible outcomes:

| Outcome          | Description                                                  |
| ---------------- | ------------------------------------------------------------ |
| `preserved`      | Source identity and revision compatibility remain valid      |
| `stale`          | Logical source remains but input changed                     |
| `remapped`       | Translation is safely associated with reconciled Text Blocks |
| `split_required` | One previous translation must map to several new blocks      |
| `merge_required` | Several previous translations may map to one new block       |
| `orphaned`       | No valid source relationship remains                         |
| `superseded`     | A new translation replaces the previous logical result       |

Automatic remapping must require strong evidence.

Ambiguous reconciliation should request review rather than silently attach a Translation to the wrong Text Block.

---

# Events

Typical domain events include:

* `TranslationRequested`
* `TranslationGenerationStarted`
* `TranslationGenerated`
* `TranslationValidated`
* `TranslationPostProcessed`
* `TranslationReady`
* `TranslationFailed`
* `TranslationRevisionCreated`
* `TranslationActivated`
* `TranslationCorrected`
* `TranslationReviewRequested`
* `TranslationApproved`
* `TranslationRejected`
* `TranslationMarkedStale`
* `TranslationRegenerationRequested`
* `TranslationSuperseded`
* `TranslationAlternativeAdded`
* `TranslationMappingFailed`
* `TranslationInvalidated`

Events should carry:

* Translation ID
* Translation Revision
* Page ID
* Source references
* Target language
* Status
* Correlation identifiers
* Changed-field metadata

Events should not contain raw provider credentials or large prompt payloads.

Raw source and target text should be excluded from general event envelopes unless the consumer contract explicitly requires them.

---

# Processing Example: Comic Page

```text
Three ordered Text Blocks
        │
        ▼
Create Translation Group Revision 1
        │
        ▼
Build source and context snapshots
        │
        ▼
Generate Chinese-to-Vietnamese translation
        │
        ▼
Normalize provider response
        │
        ▼
Map output to each Text Block
        │
        ▼
Validate language and completeness
        │
        ▼
Create Translation Revisions
        │
        ▼
Publish active translations
        │
        ▼
Show results in side panel
```

Each output remains associated with its original speech bubble or text region.

---

# Processing Example: Browser Novel

```text
Ordered paragraph Text Blocks
        │
        ▼
Group paragraphs by context and size
        │
        ▼
Translate each group incrementally
        │
        ▼
Map output to paragraph identifiers
        │
        ▼
Validate paragraph coverage
        │
        ▼
Create Translation Revisions
        │
        ▼
Render formatted Vietnamese paragraphs
```

Paragraph grouping improves context without losing paragraph-level identity.

---

# Processing Example: User Correction

```text
Active Translation Revision 2
        │
        ▼
User corrects a character name
        │
        ▼
Create Translation Revision 3
        │
        ├── Parent: Revision 2
        ├── Source: user_edit
        └── Change: terminology
        │
        ▼
Activate Revision 3
        │
        ▼
Invalidate affected render output
        │
        ▼
Create optional glossary candidate
```

The provider-generated revision remains available in history.

---

# Architecture Invariants

1. Every Translation belongs to exactly one Page.
2. A Translation references at least one primary Text Block.
3. Every source reference includes an exact Text Block revision.
4. Translation never modifies source Text Block content.
5. Translation identity and Translation revision are separate.
6. Published Translation Revisions are immutable.
7. Every processing-significant translation change creates a new revision.
8. Target language is part of logical Translation identity.
9. Provider-specific formats never enter the domain directly.
10. Only normalized and validated output becomes a Translation Revision.
11. Grouped translation preserves deterministic output mapping.
12. Context sources do not become primary output sources automatically.
13. User corrections have higher authority than generated output.
14. Approved revisions cannot be overwritten silently.
15. Stale results cannot become active automatically.
16. Source and configuration hashes are deterministic.
17. Retry attempts do not create duplicate logical Translations.
18. Cancelled operations do not publish active revisions.
19. Late results cannot overwrite newer compatible revisions.
20. Cache results must pass the same domain validation as live results.
21. Presentation and Rendering reference exact Translation Revisions.
22. Presentation formatting does not mutate canonical Translation content.
23. Provider identity is lineage metadata, not business identity.
24. Translation history remains traceable.
25. Translation records cannot outlive their Page.
26. Cross-Project context and glossary references are prohibited.
27. Raw source and translated text are excluded from logs by default.
28. Exported immutable output remains linked to the revision used to create it.

---

# Open Decisions

The following decisions should remain open until prototype validation:

* Whether one Translation record represents one Text Block or one Translation Group
* Whether grouped requests create one group result plus child translations
* How many alternatives should be retained
* Whether raw provider output is stored by default
* How long failed execution data is retained
* Whether all user edits become durable revisions
* Which quality checks run in the MVP
* Whether low-confidence translations require automatic review
* How glossary changes determine affected translations
* Whether prompt-template changes invalidate existing translations
* How much previous dialogue should be included as context
* Whether translation cache is shared across Pages with identical content
* Whether provider identity participates in cache compatibility
* How sound effects should be translated
* Whether literal and natural output should be generated together
* How partial streaming output is presented
* Whether automatic regeneration may replace unapproved output
* How Text Block split and merge reconciliation should behave
* Which correction types become glossary or memory candidates
* Whether approved translations may be reused across repeated source content
* How translation quality is measured without reliable reference translations

---

# Recommended MVP Scope

For the first CRAI MVP, Translation should support:

* Simplified Chinese to Vietnamese
* English to Vietnamese
* Ordered Text Block input
* Context-aware grouped translation
* Deterministic source mapping
* Basic Translation Profile
* Basic glossary enforcement
* One active Translation Revision per Text Block
* Manual translation correction
* Stale detection from source changes
* Retry-safe idempotent publication
* Translation cache
* Side-panel presentation
* Basic warnings and diagnostics

The MVP may defer:

* Multiple simultaneous alternatives
* Formal approval workflow
* Advanced quality scoring
* Collaborative review
* Translation memory exchange formats
* Cross-device synchronization
* Automatic glossary learning
* Complex reconciliation after block split or merge
* Full provider comparison history
* Long-term raw response retention

---

# Related Documents

* README.md
* PROJECT.md
* BOOK.md
* CHAPTER.md
* PAGE.md
* IMAGE.md
* TEXT_BLOCK.md
* LANGUAGE.md
* GLOSSARY.md
* CHARACTER.md
* SESSION.md
* PROFILE.md
* `docs/architecture/STATE_MACHINE.md`
* `docs/architecture/EVENT_BUS.md`
* `docs/architecture/DATA_FLOW.md`
* `docs/architecture/ai/PIPELINE.md`
* `docs/architecture/ai/STAGES.md`
* `docs/architecture/ai/REQUEST.md`
* `docs/architecture/ai/RESPONSE.md`
* `docs/architecture/ai/CONTEXT.md`
* `docs/architecture/ai/PROMPTS.md`
* `docs/architecture/ai/CACHE.md`
* `docs/architecture/ai/RETRY.md`
* `docs/architecture/ai/FALLBACK.md`
* `docs/architecture/ai/STREAMING.md`
