# Translation Domain

* **Document:** Domain / Translation
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

A `Translation` represents a durable target-language interpretation of one or more exact source TextBlock revisions.

It preserves the relationship between:

* source content,
* source revisions,
* target language,
* translation intent,
* effective configuration,
* context,
* glossary,
* character information,
* translated output,
* user corrections,
* review decisions,
* and revision history.

A Translation is a **domain result**.

It is not:

* a provider request,
* a provider response,
* a prompt,
* a runtime job,
* a retry attempt,
* a cache entry,
* a streaming buffer,
* a rendered text layer.

Provider and runtime artifacts MUST be normalized and validated before durable Translation revisions are published.

---

# Domain Role

Conceptually:

```text
TextBlock Revisions
        |
        v
Translation Input Snapshot
        |
        v
Translation Execution
        |
        v
Normalized Candidate
        |
        v
Domain Validation
        |
        v
Translation Revision
        |
        +--> Review
        |
        +--> Presentation
        |
        +--> Rendering
        |
        +--> Export
```

Translation preserves stable business meaning independently from:

* provider,
* model,
* transport,
* retry strategy,
* infrastructure.

---

# Ownership

Translation is an independently addressable domain resource.

It does NOT belong transactionally to Page.

A Translation belongs to a valid Project/content scope through its source relationship.

Typical relationship:

```text
Project
   |
   v
TextBlock Revision(s)
   |
   v
Translation
```

For image-based content:

```text
Page
  |
  v
TextBlock
  |
  v
Translation
```

For text-native content:

```text
Chapter
  |
  v
TextBlock
  |
  v
Translation
```

`pageId` MAY be available as contextual or indexing metadata.

It MUST NOT be required for Translation identity.

---

# Responsibilities

A Translation is responsible for:

* stable logical Translation identity,
* exact source association,
* target language,
* Translation revision history,
* effective translated content,
* active revision selection,
* source compatibility,
* stale detection,
* Translation lineage,
* correction history,
* configuration/context snapshot references,
* glossary/context dependency references,
* quality metadata,
* domain lifecycle.

A Translation is NOT responsible for:

* prompt construction,
* provider selection,
* provider authentication,
* network execution,
* retries,
* fallback routing,
* token budgeting,
* model invocation,
* streaming transport,
* Presentation layout,
* rendering,
* provider credentials.

---

# Identity

Every Translation has a stable logical identity.

Typical fields include:

```text
Translation
├── translationId
├── projectId
├── targetLanguage
├── purpose
├── sourceRelationshipId
├── activeRevisionId?
├── lifecycleStatus
├── createdAt
├── updatedAt
└── version
```

Optional indexing fields MAY include:

```text
chapterId?
pageId?
translationGroupId?
```

These fields MUST NOT replace exact source-revision references.

---

# Logical Identity

A Translation ID represents one stable translation relationship.

A new Translation ID is normally required when:

* primary source membership changes materially,
* source blocks are split or merged into a different logical result,
* target language changes,
* translation purpose changes,
* reconciliation cannot safely preserve identity.

Changing wording alone SHOULD create a new Translation Revision rather than a new Translation identity.

---

# Translation Purpose

Translation purpose describes semantic intent.

Recommended values MAY include:

```text
DIRECT
LITERAL
NATURAL
LOCALIZED
BILINGUAL
SUMMARY
EXPLANATION
TRANSLITERATION
MANUAL
IMPORTED
```

`CONTEXTUAL` is normally better represented as execution/context strategy rather than a fundamentally different translated-content identity.

Provider or model names MUST NOT define Translation purpose.

---

# Source Association

Every Translation MUST reference at least one exact primary TextBlock revision.

Single source:

```text
Translation
└── Source
    ├── textBlockId
    └── revision
```

Grouped source:

```text
Translation
└── Primary Sources
    ├── Block A / Revision 2
    ├── Block B / Revision 4
    └── Block C / Revision 1
```

Each source member SHOULD preserve:

```text
textBlockId
textBlockRevision
effectiveSourceTextHash
sourceLanguage
sequence
mappingKey
role
```

A Translation MUST NOT depend only on the mutable "current TextBlock".

---

# Source Roles

Translation inputs MAY have roles such as:

```text
PRIMARY
CONTEXT
HISTORY
INSTRUCTION
```

Glossary and Character references SHOULD generally be represented through explicit snapshots rather than pretending they are TextBlock sources.

Only `PRIMARY` inputs require direct output mapping.

---

# Source Snapshot

A Translation Revision MUST preserve a stable source snapshot.

Conceptually:

```text
SourceSnapshot
├── primary TextBlock revisions
├── ordered source hashes
├── source languages
├── source semantic metadata
└── sourceSnapshotHash
```

The snapshot MAY reference immutable records rather than duplicate complete source content.

It MUST be sufficient to determine whether the Translation remains compatible with current source state.

---

# Translation Group

Multiple TextBlocks MAY be translated together for context or efficiency.

```text
TranslationGroup
├── ordered members
├── group revision
├── context policy
└── mapping strategy
```

TranslationGroup is a Translation-domain processing/context structure.

It does NOT own TextBlocks.

Membership changes create a new group revision.

---

# Group vs Translation

A group request does not require one monolithic Translation result.

Recommended model:

```text
TranslationGroup
       |
       v
Execution
       |
       +--> Translation for Block A
       +--> Translation for Block B
       +--> Translation for Block C
```

This preserves TextBlock-level identity while allowing shared context and one provider request.

A future feature MAY support genuine many-to-one or one-to-many Translation relationships when explicitly required.

---

# Output Mapping

Grouped Translation MUST preserve deterministic source-to-output mapping.

Recommended provider-independent mapping:

```text
Output
├── block-a -> translated text
├── block-b -> translated text
└── block-c -> translated text
```

Mapping SHOULD use stable keys.

Array position alone SHOULD NOT be relied upon when stable keys are available.

Invalid mapping includes:

* missing required primary output,
* unknown mapping keys,
* ambiguous assignment,
* accidental output collapse,
* unsupported split/merge mapping.

---

# Translation Representations

A Translation Revision MAY preserve several representations:

```text
Normalized Generated Text
        |
        v
Post-Processed Text
        |
        v
User-Corrected Text
        |
        v
Effective Translation
```

Raw provider payload is execution/diagnostic data and SHOULD normally remain outside the canonical Translation Revision.

---

# Normalized Translation

Generated provider output MUST be normalized into CRAI's canonical representation before publication.

Normalization MAY include:

* schema conversion,
* mapping resolution,
* Unicode normalization,
* whitespace normalization,
* protocol artifact removal.

Normalization MUST NOT silently alter semantic meaning.

---

# Post-Processed Translation

Deterministic post-processing MAY include:

* punctuation cleanup,
* spacing normalization,
* formatting cleanup,
* approved glossary enforcement,
* script conversion,
* honorific normalization,
* safe line-break normalization.

Any processing-significant post-processing policy SHOULD be versioned.

---

# User-Corrected Translation

Users MAY edit translated content.

User correction creates a new immutable Translation Revision.

It MUST NOT mutate previous revisions.

Corrections SHOULD record:

* parent revision,
* actor,
* timestamp,
* correction type,
* changed fields.

Approved user corrections MUST NOT be silently overwritten by later automatic generation.

---

# Effective Translation

Effective translation is the text consumed by downstream capabilities.

Recommended precedence:

```text
Approved User Revision
        |
        v fallback
Current User Revision
        |
        v fallback
Post-Processed Generated Revision
        |
        v fallback
Validated Normalized Revision
```

Raw unvalidated provider output MUST NOT become normal user-visible canonical Translation.

---

# Translation Revision

Translation identity and Translation revision are separate.

```text
translationId
    stable logical relationship

translationRevision
    immutable translated-content version
```

A Translation Revision SHOULD contain:

```text
translationRevisionId
translationId
revisionNumber
parentRevisionId?
targetText
sourceSnapshotId
sourceHash
configurationSnapshotId
configurationHash
contextSnapshotId?
glossarySnapshotId?
characterContextSnapshotId?
creationSource
createdAt
actor?
qualityMetadata?
```

Published revisions MUST be immutable.

---

# Revision Creation Sources

Possible creation sources include:

```text
PROVIDER
USER_EDIT
REVIEW_EDIT
POST_PROCESS
GLOSSARY_REAPPLY
IMPORT
MIGRATION
RECONCILIATION
```

Creation source describes lineage.

It MUST NOT automatically imply approval.

---

# Active Revision

One Translation may contain multiple revisions.

```text
Translation
├── Revision 1
├── Revision 2
├── Revision 3
└── activeRevision -> Revision 3
```

An active revision MUST:

* belong to the Translation,
* reference compatible source revisions,
* match the Translation target language,
* satisfy required validation,
* not be invalidated,
* not be superseded,
* not be stale unless policy explicitly permits stale use.

Changing active revision is a domain operation.

---

# Language Pair

Every Translation Revision MUST preserve:

```text
effectiveSourceLanguage
targetLanguage
```

Optional metadata MAY include:

```text
sourceScript
targetScript
regionalVariant
mixedLanguageStrategy
transliterationPolicy
```

Changing target language creates a different logical Translation.

---

# Translation Profile

Translation behavior MAY be defined by a versioned Translation Profile.

Possible intent fields include:

* target language,
* translation purpose,
* style,
* formality,
* honorific policy,
* naming policy,
* sound-effect policy,
* localization policy,
* formatting policy,
* context policy,
* glossary policy,
* quality policy.

Provider preference MAY exist in effective execution configuration, but provider identity MUST NOT become Translation business identity.

Translation records reference the exact effective configuration revision used.

---

# Configuration Resolution

Translation Domain does NOT resolve Project/Book/Chapter/TextBlock configuration hierarchy itself.

A Preferences/configuration capability SHOULD produce an effective immutable configuration snapshot.

```text
Project preferences
       |
Book override?
       |
Chapter override?
       |
TextBlock override?
       |
       v
Effective Translation Configuration
       |
       v
Translation Execution
```

Translation preserves a reference/hash to that effective configuration.

---

# Context Snapshot

Context used to produce output SHOULD be identifiable.

Possible inputs:

```text
previous TextBlocks
previous Translations
current Chapter context
Character references
user instructions
session context
```

A Context Snapshot SHOULD preserve:

```text
contextRevision
reference identities
relevant revisions
contextHash
```

It need not duplicate all raw context inline.

---

# Glossary Snapshot

Translation MAY reference a Glossary Snapshot.

Example:

```text
GlossarySnapshot
├── glossaryId
├── glossaryRevision
├── selected entries
├── entry revisions
└── snapshotHash
```

Glossary updates MUST NOT silently mutate previous Translation revisions.

Dependency impact determines whether existing output:

* remains valid,
* should be reviewed,
* becomes stale.

---

# Character Context Snapshot

Relevant Character information MAY be snapshotted.

Possible information:

* Character IDs,
* Character revisions,
* aliases,
* speaker mapping,
* naming rules,
* speech-style rules.

Character changes invalidate Translation only when changed information is translation-significant.

---

# Source Hash

Translation SHOULD maintain a deterministic source hash.

Possible inputs:

```text
ordered primary TextBlock IDs
exact TextBlock revisions
effective source hashes
source language
translation-significant semantic metadata
group revision
```

Source hash MUST NOT include provider runtime data.

---

# Configuration Hash

Configuration hash MAY include:

```text
Translation Profile revision
target language
Glossary snapshot
Character Context snapshot
Context policy
instruction revision
output schema revision
post-processing revision
validation policy revision
```

Cache compatibility generally requires:

```text
Source Hash
+
Configuration Hash
```

Provider identity MAY be part of a separate execution/reproducibility hash when required.

---

# Staleness

A Translation Revision becomes stale when a dependency that can affect translated meaning changes.

Typical causes:

* source TextBlock revision changed,
* effective source text changed,
* primary source membership changed,
* source language changed,
* Translation Profile changed,
* relevant glossary entry changed,
* relevant Character information changed,
* relevant user instruction changed,
* context policy changed,
* translation-significant post-processing changed.

Staleness MUST use dependency impact rather than blanket invalidation.

---

# Stale Impact

Recommended impact categories:

```text
NONE
PRESENTATION_ONLY
REVIEW_RECOMMENDED
TRANSLATION_STALE
SOURCE_INVALID
```

Examples:

```text
font change
    -> PRESENTATION_ONLY

unrelated glossary entry change
    -> NONE

speaker gender correction used by Translation
    -> TRANSLATION_STALE

source TextBlock split
    -> SOURCE_INVALID
```

---

# Translation Lifecycle

Translation lifecycle describes durable logical result availability.

Recommended states:

```text
Created
   |
   v
Active
   |
   +--> Stale
   |
   +--> Superseded
   |
   +--> Invalidated
   |
   v
Archived
```

Suggested statuses:

```text
CREATED
ACTIVE
STALE
SUPERSEDED
INVALIDATED
ARCHIVED
```

A Translation becomes `ACTIVE` when it has at least one valid published revision selected for normal use.

---

# Execution Lifecycle Is Separate

The following states belong to Translation Execution:

```text
REQUESTED
QUEUED
GENERATING
STREAMING
VALIDATING
POST_PROCESSING
SUCCEEDED
FAILED
CANCELLED
```

They MUST NOT be core Translation lifecycle states.

Example:

```text
Translation: ACTIVE
activeRevision: 5

Regeneration Execution:
    status: RUNNING
```

The existing Translation remains usable until a new compatible revision is published.

---

# Translation Execution

A Translation Execution represents one attempt/workflow to produce a candidate revision.

Typical fields MAY include:

```text
executionId
logicalRequestId
sourceSnapshotId
configurationSnapshotId
status
startedAt
completedAt
```

Provider-specific attempt metadata remains inside execution/runtime records.

---

# Provider Attempt

An execution MAY contain one or more Provider Attempts.

```text
Translation Execution
├── Attempt 1: Provider A
├── Attempt 2: Provider A retry
└── Attempt 3: Provider B fallback
```

Possible metadata:

```text
providerId
modelId
requestId
attemptNumber
latency
tokenUsage
cost
finishReason
error
```

This metadata is operational lineage.

Consumers of a Translation MUST NOT require it for ordinary business behavior.

---

# Retry and Fallback

Retry/fallback belongs to execution.

Domain rules:

1. retries MUST NOT create duplicate logical Translation identities,
2. attempts share the same immutable logical request inputs,
3. only validated successful output may publish a revision,
4. cancelled work MUST NOT publish active output,
5. late results MUST undergo compatibility checks,
6. duplicate successful results MUST be handled idempotently.

---

# Streaming

Streaming text is provisional execution output.

```text
Provider Stream
      |
      v
Provisional Assembly
      |
      v
Optional Temporary Display
      |
      v
Final Validation
      |
      v
Translation Revision
```

Partial content MUST NOT become a normal durable Translation Revision unless an explicit partial-result policy exists.

Equivalent final streamed and non-streamed results SHOULD produce equivalent domain records.

---

# Validation

Before a candidate becomes a published Translation Revision, validation SHOULD include:

* valid Translation identity,
* valid primary source snapshot,
* every source revision exists,
* source hashes match,
* valid target language,
* non-empty effective output,
* deterministic output mapping,
* valid configuration reference,
* valid glossary/context references,
* output schema validity,
* language validation,
* required safety validation,
* stale source protection,
* superseded source protection.

Page ownership MUST NOT be a universal validation requirement.

---

# Completeness Validation

Grouped execution MUST verify that required primary source mappings are complete.

Conceptually:

```text
Expected Primary Keys
        =
Returned Output Keys
```

Explicit exceptions MAY exist.

Examples:

* sound effect intentionally left untranslated,
* permitted many-to-one mapping,
* permitted one-to-many mapping.

Association MUST remain deterministic.

---

# Review

Translation Review is separate from Translation generation and core lifecycle.

Possible review states include:

```text
UNREVIEWED
REVIEW_REQUESTED
IN_REVIEW
CHANGES_REQUESTED
APPROVED
REJECTED
WAIVED
```

Review MUST reference an exact Translation Revision.

Approval of Revision 3 does NOT imply approval of Revision 4.

---

# Review Ownership

Review records SHOULD be independently owned by the Review workflow/domain.

Translation MAY expose a derived review summary.

Example:

```text
Translation Revision
        |
        v
Review Record
```

Translation MUST NOT duplicate complete Review workflow state as authoritative domain data.

---

# Quality Metadata

Translation revisions MAY preserve stable quality indicators such as:

* language validation,
* glossary compliance,
* source coverage,
* mapping completeness,
* warnings,
* suspected omission,
* suspected added meaning,
* consistency score.

Provider confidence and CRAI validation signals MUST remain distinguishable.

Quality metadata is advisory unless policy explicitly uses it for activation.

---

# Manual Correction

User correction MUST:

1. create a new immutable Translation Revision,
2. preserve the previous revision,
3. record parent revision,
4. preserve actor and time,
5. activate according to policy,
6. invalidate only dependent Presentation/Rendering outputs,
7. never mutate source TextBlocks,
8. optionally produce Glossary/Memory candidates.

Automatic regeneration MUST NOT silently overwrite protected user revisions.

---

# Alternatives

Multiple Translation alternatives MAY be retained.

Example:

```text
Translation
├── Revision/Alternative A
├── Revision/Alternative B
└── selected active revision
```

However, "alternative" and "revision" SHOULD NOT be conflated blindly.

A future model MAY distinguish:

```text
Translation
   |
   +--> Variant: Literal
   |      └── revisions
   |
   +--> Variant: Natural
          └── revisions
```

For MVP, a simpler revision model is acceptable.

---

# Cache

Translation cache is an optimization.

Cache entries are NOT domain truth.

Cached output MUST still pass:

* source compatibility,
* configuration compatibility,
* validation,
* approval protection,
* stale checks,
* safety rules.

A cache hit MAY produce or reuse a valid Translation Revision according to explicit publication policy.

---

# Idempotency

Translation publication MUST be idempotent for equivalent logical input.

Recommended idempotency material:

```text
sourceHash
configurationHash
targetLanguage
translationPurpose
logicalRequestPurpose
```

`pageId` MUST NOT be required as part of universal identity.

This permits legitimate reuse across non-Page content.

---

# Concurrency

Concurrent executions MUST preserve source snapshots.

Rules:

1. each execution reads immutable source/configuration snapshots,
2. late results are compatibility-checked,
3. stale results MUST NOT become active automatically,
4. user revisions MUST NOT be silently overwritten,
5. active-revision changes require atomic consistency,
6. duplicate results are reconciled idempotently,
7. cancellation prevents publication when possible.

---

# Reconciliation

Translation MAY require reconciliation after TextBlock regeneration.

Possible outcomes:

```text
PRESERVED
STALE
REMAPPED
SPLIT_REQUIRED
MERGE_REQUIRED
ORPHANED
SUPERSEDED
```

Automatic remapping MUST require strong evidence.

Ambiguous remapping SHOULD require review rather than attach Translation to incorrect source content.

---

# Presentation Association

Presentation consumes an exact Translation Revision.

```text
TextBlock Revision
        |
        v
Translation Revision
        |
        v
Presentation Item
```

Presentation MAY use:

* side panel,
* overlay,
* bilingual reader,
* tooltip,
* subtitle list,
* formatted document,
* export view.

Presentation MUST NOT mutate canonical Translation content.

Display-specific shortening or wrapping is derived presentation state.

---

# Rendering Association

Rendering SHOULD reference:

```text
translationId
translationRevisionId
textBlockId
sourceGeometryRevision
renderingProfileRevision
```

Changing active Translation MAY invalidate dependent Render/Presentation artifacts.

Historical immutable outputs remain linked to the revision used when created.

---

# Export

Translation export MUST reference exact revision identities.

Possible exports include:

* translated text,
* bilingual text,
* structured TextBlock translation,
* JSON,
* chapter document,
* subtitle-like representation,
* Translation Memory representation.

Changing active Translation later MUST NOT mutate historical exported artifacts.

---

# Import

Imported translations SHOULD preserve:

* external source,
* original identifier,
* source mapping,
* target language,
* imported content,
* import time,
* trust level.

Imported content MUST NOT automatically become approved unless policy explicitly permits it.

Uncertain source mapping SHOULD require review.

---

# Persistence

Recommended separation:

```text
Translation
├── identity
├── Project scope
├── target language
├── translation purpose
├── lifecycle
└── active revision
```

```text
TranslationRevision
├── immutable target content
├── exact source snapshot
├── source hash
├── configuration hash
├── context references
├── glossary references
├── character references
├── lineage
├── quality metadata
└── creation metadata
```

```text
TranslationExecution
├── execution identity
├── source/config snapshot
├── runtime lifecycle
├── provider attempts
├── token/cost/latency metadata
└── execution errors
```

```text
TranslationReview
├── exact revision reference
├── reviewer
├── decision
├── issues
└── comments
```

These records MAY have different retention policies.

---

# Retention

Suggested policy:

* active Translation revisions: durable while source relationship remains relevant,
* approved revisions: durable,
* user corrections: durable,
* superseded revisions: according to history policy,
* stale revisions: retain when useful for history/rollback,
* failed executions: diagnostic retention,
* raw provider responses: short-lived unless required,
* token/cost metadata: observability retention,
* streaming buffers: normally ephemeral.

Retention MUST NOT depend universally on Page lifetime.

---

# Privacy

Translation may contain copyrighted, private, or sensitive content.

Default rules SHOULD include:

* do not log source or translated text,
* send only necessary provider context,
* honor provider retention/training policies,
* support local-only profiles when available,
* support temporary non-persistent sessions,
* isolate Project context,
* exclude raw content from ordinary metrics and traces.

Telemetry SHOULD prefer:

```text
translationId
revisionId
hash
language pair
text length
duration
token usage
status
error category
```

---

# Events

Translation domain events SHOULD represent durable Translation state changes.

Examples:

```text
TranslationCreated
TranslationRevisionPublished
TranslationActivated
TranslationCorrected
TranslationMarkedStale
TranslationSuperseded
TranslationInvalidated
TranslationArchived
TranslationRestored
```

Review events belong to Review.

Execution events belong to Translation execution/runtime:

```text
TranslationExecutionRequested
TranslationExecutionStarted
TranslationExecutionFailed
TranslationExecutionCompleted
```

The fact that an execution event contains `translationId` does not make it a core Translation-domain lifecycle event.

---

# Error Conditions

Stable Translation-domain errors MAY include:

```text
TRANSLATION_NOT_FOUND
TRANSLATION_SOURCE_INVALID
TRANSLATION_SOURCE_REVISION_MISMATCH
TRANSLATION_MAPPING_INVALID
TRANSLATION_TARGET_LANGUAGE_INVALID
TRANSLATION_REVISION_CONFLICT
TRANSLATION_REVISION_STALE
TRANSLATION_REVISION_SUPERSEDED
TRANSLATION_ACTIVATION_INVALID
TRANSLATION_CONFIGURATION_INVALID
TRANSLATION_CONTEXT_INVALID
TRANSLATION_GLOSSARY_INVALID
TRANSLATION_APPROVED_REVISION_PROTECTED
```

Provider/runtime failures remain execution errors rather than Translation-domain errors unless they prevent domain publication.

---

# Aggregate Boundary

Recommended Translation aggregate:

```text
Translation Aggregate

owns
    Translation identity
    Project scope
    target language
    translation purpose
    lifecycle
    active revision selection
    Translation revision history references
```

Translation Revision owns immutable translated-content state.

Translation does NOT own:

```text
TextBlock state
Page state
Provider execution
Retry state
Review workflow
Presentation state
Rendering state
Cache implementation
```

---

# Transactional Consistency

Domain operations:

```text
Activate Translation Revision
    -> Translation transaction
```

```text
Publish validated Revision
    -> Translation publication transaction
```

```text
Mark Translation stale
    -> Translation domain operation
```

Execution:

```text
Call provider
    -> Translation Execution
```

Review:

```text
Approve Translation
    -> Review workflow + exact revision reference
```

Presentation:

```text
Display Translation
    -> Presentation workflow
```

These SHOULD NOT require one global transaction.

---

# Architecture Invariants

1. `translationId` represents stable logical Translation identity.

2. Translation MUST belong to a valid Project/content scope.

3. Translation is NOT required to belong to a Page.

4. Every Translation references at least one primary TextBlock revision.

5. Every primary source reference includes an exact TextBlock revision.

6. Translation MUST NOT modify source TextBlock content.

7. Translation identity and Translation revision are separate.

8. Published Translation Revisions are immutable.

9. Every processing-significant translated-content change creates a new revision.

10. Target language is part of logical Translation identity.

11. Provider-specific formats MUST NOT enter canonical Translation state directly.

12. Only normalized and validated output may become a published generated Translation Revision.

13. Grouped translation MUST preserve deterministic output mapping.

14. Context sources MUST NOT automatically become primary output sources.

15. User corrections have higher authority according to explicit policy.

16. Approved/protected revisions MUST NOT be overwritten silently.

17. Stale revisions MUST NOT become active automatically.

18. Source and configuration hashes MUST be deterministic.

19. Retry attempts MUST NOT create uncontrolled duplicate logical Translations.

20. Cancelled executions MUST NOT publish active revisions.

21. Late execution results MUST NOT overwrite newer incompatible state.

22. Cache results MUST pass normal domain validation.

23. Presentation and Rendering reference exact Translation Revisions.

24. Presentation formatting MUST NOT mutate canonical Translation content.

25. Provider identity is execution lineage, not Translation business identity.

26. Translation history MUST remain traceable.

27. Translation retention MUST NOT depend universally on Page lifetime.

28. Cross-Project context references MUST follow explicit sharing policy and MUST NOT leak private Project context implicitly.

29. Raw source and target content SHOULD be excluded from ordinary logs.

30. Exported immutable output MUST remain linked to the exact Translation Revision used.

31. Translation lifecycle MUST remain independent from Translation Execution lifecycle.

32. Review status MUST apply to an exact Translation Revision and MUST NOT be inferred solely from Translation lifecycle.

---

# MVP Recommendation

For CRAI MVP, Translation SHOULD support:

* Simplified Chinese → Vietnamese,
* English → Vietnamese,
* TextBlock-revision input,
* contextual grouping,
* deterministic source mapping,
* basic Translation Profile,
* glossary application,
* one active Translation per TextBlock + target language,
* immutable Translation revisions,
* manual correction,
* stale detection,
* idempotent publication,
* cache compatibility,
* side-panel Presentation,
* basic validation and warnings.

MVP MAY defer:

* complex Translation variants,
* formal approval workflow,
* collaborative review,
* advanced quality scoring,
* complex split/merge reconciliation,
* long-term provider-response storage,
* automatic glossary learning,
* Translation Memory interchange,
* full provider-comparison history.

---

# Open Decisions

The following SHOULD remain open until prototype validation:

* whether TranslationGroup is durable or ephemeral,
* whether one Translation can intentionally own multiple primary TextBlocks,
* whether grouped execution always produces child Translations per TextBlock,
* exact alternative/variant model,
* raw provider-response retention,
* quality-validation scope for MVP,
* glossary dependency tracking granularity,
* prompt-template impact on staleness,
* context-window policy,
* cache reuse across identical source TextBlocks,
* provider identity in execution-cache compatibility,
* sound-effect Translation policy,
* literal vs natural multi-variant support,
* partial streaming Presentation behavior,
* automatic regeneration policy,
* split/merge reconciliation,
* Translation Memory candidate rules,
* reuse of approved Translation across repeated source content.

---

# Related Documents

Domain:

* `README.md`
* `PROJECT.md`
* `BOOK.md`
* `CHAPTER.md`
* `PAGE.md`
* `IMAGE.md`
* `TEXT_BLOCK.md`
* `LANGUAGE.md`
* `GLOSSARY.md`
* `CHARACTER.md`
* `SESSION.md`
* `PROFILE.md`

Architecture:

* `docs/architecture/CAPABILITY_MAP.md`
* `docs/architecture/OWNERSHIP_MAP.md`
* `docs/architecture/STATE_MACHINE.md`
* `docs/architecture/EVENT_BUS.md`
* `docs/architecture/MODULE_DEPENDENCY.md`
* `docs/architecture/DATA_FLOW.md`

AI / Translation execution:

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

Module contracts remain authoritative for module-specific runtime ownership and execution behavior.
