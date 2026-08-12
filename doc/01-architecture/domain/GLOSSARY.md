# Glossary Domain

* **Document:** Domain / Glossary
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

The `Glossary` domain defines terminology knowledge used by CRAI to keep source interpretation and translated output consistent.

Glossary terminology may describe:

* character names,
* aliases,
* locations,
* organizations,
* factions,
* skills,
* techniques,
* items,
* ranks,
* titles,
* honorifics,
* cultural terms,
* fictional concepts,
* technical terminology,
* repeated phrases,
* sound effects,
* preserved terms,
* forbidden translations.

Glossary provides stable terminology intent to capabilities such as:

* Translation,
* terminology validation,
* review,
* source-text assistance,
* Presentation,
* search,
* context construction.

Glossary MUST remain independent from:

* AI provider formats,
* prompts,
* provider execution,
* search-index implementation,
* matching-engine implementation.

---

# Domain Role

Glossary acts as controlled terminology knowledge.

Conceptually:

```text
Glossary
   |
   +--> GlossaryEntry
   +--> GlossaryEntry
   +--> GlossaryEntry
   |
   v
Applicable Entry Resolution
   |
   v
Glossary Snapshot
   |
   v
Translation Context
   |
   v
Translation Revision
```

Glossary influences Translation.

It does NOT own Translation.

A Translation Revision preserves the exact Glossary Snapshot or Entry Revisions that influenced it.

---

# Domain Boundaries

The Glossary domain contains several independently addressable concepts.

Recommended separation:

```text
Glossary
    collection identity
    metadata
    scope
    policies

GlossaryEntry
    terminology concept identity
    lifecycle
    active revision

GlossaryEntryRevision
    immutable terminology definition

GlossarySnapshot
    immutable resolved terminology set

GlossaryMatch
    derived runtime matching result

GlossaryCandidate
    suggestion/review workflow

GlossaryReview
    approval decision

GlossaryConflict
    detected terminology conflict
```

These concepts MAY belong to the same bounded domain.

They MUST NOT be interpreted as one large transactional object that requires loading every entry.

---

# Glossary Aggregate

Recommended Glossary aggregate ownership:

```text
Glossary Aggregate

owns
    glossaryId
    metadata
    scope
    lifecycle
    default language scope
    policy references
    active/published revision reference
```

It references Entries by identity.

It MUST NOT require all GlossaryEntry state to be loaded for ordinary metadata operations.

Example:

```text
renameGlossary(glossaryId)
```

MUST NOT require loading thousands of terminology entries.

---

# Glossary Entry Aggregate

Each `GlossaryEntry` SHOULD be independently addressable.

Recommended ownership:

```text
GlossaryEntry Aggregate

owns
    entryId
    glossaryId
    entry type
    lifecycle
    active revision
    lineage
```

Entry textual and behavioral definitions belong to immutable Entry Revisions.

This enables:

* independent editing,
* optimistic concurrency,
* scalable indexing,
* revision history,
* targeted invalidation.

---

# Responsibilities

The Glossary domain is responsible for:

* Glossary identity,
* terminology entry identity,
* terminology revisions,
* Source Forms,
* Target Forms,
* language applicability,
* terminology intent,
* deterministic precedence policy,
* scope representation,
* conflict representation,
* publication semantics,
* snapshots,
* terminology lineage,
* terminology authority,
* terminology change classification,
* selective Translation impact.

The Glossary domain is NOT responsible for:

* scanning arbitrary source text,
* tokenizing language,
* executing OCR,
* executing entity detection,
* executing fuzzy or semantic matching,
* generating embeddings,
* building prompts,
* invoking Translation providers,
* mutating TextBlock source content,
* rendering translated terminology.

---

# Glossary Identity

Every Glossary has a stable identity.

Typical fields:

```text
Glossary
├── glossaryId
├── ownerScope
├── name
├── description?
├── defaultSourceLanguageRange?
├── defaultTargetLanguageRange?
├── lifecycleStatus
├── activeRevisionId?
├── createdAt
├── updatedAt
└── version
```

`glossaryId` MUST remain stable.

Glossary identity and Glossary Revision identity MUST remain separate.

---

# Glossary Scope

A Glossary MUST explicitly declare its ownership scope.

Possible ownership scopes MAY include:

```text
PROJECT
USER
WORKSPACE
GLOBAL
```

For CRAI MVP, `PROJECT` SHOULD be the primary supported scope.

A Project-scoped Glossary references:

```text
projectId
```

A shared Glossary MUST NOT pretend to be owned by every Project that consumes it.

Projects MAY reference shared Glossaries explicitly when future sharing is supported.

---

# Glossary Revision

A Glossary Revision represents an immutable view of collection-level configuration.

It MAY include:

```text
GlossaryRevision
├── glossaryRevisionId
├── glossaryId
├── revisionNumber
├── policySnapshot
├── defaultLanguageScope
├── publicationMetadata
├── parentRevisionId?
├── createdBy
├── createdAt
├── changeReason?
└── contentHash
```

A Glossary Revision does NOT need to embed complete Entry contents.

It MAY reference included Entry Revisions when a full collection snapshot is required.

Published or externally referenced revisions MUST be immutable.

---

# Glossary Entry

A `GlossaryEntry` represents one continuing terminology concept or terminology rule.

Examples:

```text
灵力 -> linh lực

李青 -> Lý Thanh

Ultimate Skill -> preserve
```

An Entry is not merely a source-target string pair.

It may include:

* multiple Source Forms,
* multiple Target Forms,
* language scope,
* semantic type,
* terminology rule,
* context restrictions,
* entity references,
* matching intent,
* priority.

---

# Entry Identity

Each Entry has a stable identity.

```text
entryId != entryRevisionId
```

`entryId` represents the continuing terminology concept.

`entryRevisionId` represents one immutable definition.

A spelling correction normally creates a new revision.

A genuinely different semantic concept SHOULD create a new Entry identity.

---

# Entry Revision

Recommended structure:

```text
GlossaryEntryRevision
├── entryRevisionId
├── entryId
├── sourceForms[]
├── targetForms[]
├── languageScope
├── entryType
├── rule
├── matchingPolicy
├── applicability
├── priority
├── notes?
├── entityReferences[]
├── parentRevisionId?
├── createdBy
├── createdAt
└── contentHash
```

Entry Revisions MUST be immutable after publication.

Editing an Entry creates a new Entry Revision.

---

# Entry Type

Entry Type describes semantic meaning.

Possible values MAY include:

```text
CHARACTER_NAME
PLACE
ORGANIZATION
FACTION
SPECIES
SKILL
TECHNIQUE
ABILITY
ITEM
WEAPON
RANK
REALM
TITLE
HONORIFIC
RELATIONSHIP_TERM
TECHNICAL_TERM
CULTURAL_TERM
IDIOM
REPEATED_PHRASE
SOUND_EFFECT
MEASUREMENT
GENERAL_TERM
CUSTOM
```

Entry Type describes **what the concept is**.

It MUST NOT directly define how Translation treats it.

---

# Rule Type

Rule Type describes terminology intent.

Recommended values:

```text
TRANSLATE
PRESERVE
TRANSLITERATE
ROMANIZE
NORMALIZE
PREFER
AVOID
FORBID
CONTEXTUAL
INFORMATIONAL
```

Entry Type and Rule Type MUST remain separate.

Example:

```text
entryType: CHARACTER_NAME
ruleType: TRANSLITERATE
```

---

# Translate Rule

`TRANSLATE` defines an intended target-language semantic form.

Example:

```text
灵力
    ->
linh lực
```

The exact grammatical realization MAY still depend on Translation context.

---

# Preserve Rule

`PRESERVE` indicates that the relevant form should remain unchanged.

Example:

```text
MP -> MP
```

Preserve policy MAY still define normalization such as casing or Script.

---

# Transliterate Rule

`TRANSLITERATE` preserves pronunciation across writing systems according to an explicit convention.

Example:

```text
李青 -> Lý Thanh
```

Glossary intent does not execute transliteration algorithms itself.

---

# Romanize Rule

`ROMANIZE` specifies a Latin-script representation according to an explicit standard where applicable.

Example:

```text
東京 -> Tōkyō
```

Romanization standard SHOULD remain explicit.

---

# Normalize Rule

`NORMALIZE` maps multiple equivalent or accepted forms to a preferred terminology representation.

Example:

```text
Hit Point
Health Point
HP

-> HP
```

Normalization MUST NOT perform uncontrolled semantic replacement.

---

# Prefer / Avoid / Forbid

`PREFER` expresses strong preference.

`AVOID` expresses discouraged terminology.

`FORBID` expresses terminology that MUST NOT appear where the rule applies.

These rules SHOULD produce structured validation semantics rather than blindly editing generated text.

---

# Contextual Rule

`CONTEXTUAL` indicates that correct terminology depends on context.

Example:

```text
师父

cultivation context -> sư phụ
modern context      -> thầy
```

Glossary MAY preserve alternatives and contextual evidence.

It MUST NOT claim deterministic resolution when available context is insufficient.

---

# Informational Rule

`INFORMATIONAL` provides terminology/context information without mandating output.

This can help Translation understand world-building concepts without forcing literal replacement.

---

# Source Form

An Entry MAY contain several Source Forms.

Recommended representation:

```text
SourceForm
├── sourceFormId
├── text
├── language
├── script?
├── formType
├── normalizedForm?
├── matchingPolicy?
└── status
```

Possible form types include:

```text
CANONICAL
ALIAS
ABBREVIATION
ALTERNATE_SPELLING
HISTORICAL
SIMPLIFIED
TRADITIONAL
ROMANIZATION
TRANSLITERATION
OCR_VARIANT
COMMON_ERROR
INFLECTED
IMPORTED_ALIAS
```

One canonical source form SHOULD be identifiable where meaningful.

---

# OCR Variants

OCR variants MAY assist source matching.

Example:

```text
canonical:
    修炼

observed OCR variants:
    修練
    体炼
```

OCR variants:

* MUST NOT replace canonical source terminology,
* SHOULD preserve evidence,
* SHOULD preserve confidence where derived,
* MAY reference relevant TextBlocks or Images.

They remain terminology/matching aids.

---

# Target Form

An Entry MAY contain multiple Target Forms.

Recommended structure:

```text
TargetForm
├── targetFormId
├── text
├── language
├── script?
├── formType
├── preferenceRank?
├── styleScope?
└── status
```

Possible types:

```text
PREFERRED
APPROVED_ALTERNATIVE
LITERAL
LOCALIZED
TRANSLITERATED
ROMANIZED
PRESERVE_ORIGINAL
ABBREVIATION
DISPLAY_ALIAS
DEPRECATED
FORBIDDEN
```

---

# Canonical Concept

Multiple Source and Target Forms MAY belong to one terminology concept.

```text
GlossaryEntry
├── canonical source
├── source aliases
├── target forms
├── entity references
└── rule
```

This enables CRAI to recognize:

* alternate names,
* nicknames,
* Script variants,
* OCR mistakes,
* romanizations,
* abbreviations

without losing concept identity.

---

# Language Scope

Every active terminology Entry MUST declare language applicability.

Recommended representation:

```text
GlossaryLanguageScope
├── sourceLanguageRange
├── targetLanguageRange
├── sourceScript?
├── targetScript?
└── languagePairRestrictions?
```

Example:

```text
source: zh-Hans
target: vi
```

Such an Entry MUST NOT automatically apply to:

```text
ja -> vi
en -> vi
vi -> en
```

Language matching follows `LANGUAGE.md`.

---

# Language Specificity

When several applicable entries exist, more specific Language matches MAY outrank broader ones.

Example:

```text
zh-Hans-CN
    >
zh-Hans
    >
zh
```

This is compatibility precedence.

It MUST NOT mutate persisted exact Language values.

---

# Applicability Scope

Entry applicability is distinct from Glossary ownership.

Example:

A Project Glossary may contain an Entry only applicable to one Chapter.

Possible applicability restrictions MAY include:

```text
projectId
bookIds?
chapterIds?
pageIds?
characterIds?
contentTypes?
translationProfileIds?
sessionId?
operationId?
exclusions?
```

Optional hierarchy levels MUST be skipped when absent.

A `bookId` or `pageId` MUST NOT be required.

---

# Scope Precedence

Generic specificity MAY follow:

```text
Operation
    >
Session
    >
Page if present
    >
Chapter
    >
Book if present
    >
Project
```

Shared Glossaries MAY introduce:

```text
User
Workspace
Global
```

but these are separate ownership scopes rather than mandatory parents of Project terminology.

Therefore CRAI MUST NOT assume:

```text
Global -> Workspace -> User -> Project
```

is a universal domain hierarchy.

---

# Inclusion / Exclusion

An Entry MAY include broad applicability with explicit exclusions.

At equivalent specificity:

```text
explicit exclusion
    >
broad inclusion
```

Example:

```text
Master -> sư phụ

exclude:
    game rank title
```

---

# Character Association

GlossaryEntry MAY reference Character-domain identity.

Example:

```text
GlossaryEntry
    characterId: character_001
```

Glossary does NOT own Character.

Character domain owns stable Character identity.

Glossary owns terminology-specific representations and rules.

---

# Entity Association

Entries MAY reference external semantic identities such as:

* Character,
* Place,
* Organization,
* Skill,
* Item,
* Rank.

These references improve consistency and disambiguation.

Historical Entry Revisions MUST remain interpretable even if the live referenced entity later changes or is deleted.

---

# Matching Policy

Glossary defines matching **intent**, not matching execution.

Possible policy:

```text
MatchingPolicy
├── matchType
├── caseSensitivity
├── UnicodeNormalization
├── boundaryPolicy
├── ScriptPolicy
├── punctuationPolicy
├── whitespacePolicy
├── OCRTolerance
└── confidenceThreshold?
```

The actual matcher belongs outside the Glossary aggregate.

---

# Match Types

Possible match types include:

```text
EXACT
NORMALIZED_EXACT
PHRASE
WHOLE_TOKEN
PREFIX
SUFFIX
SUBSTRING
REGEX
TOKEN_SEQUENCE
MORPHOLOGICAL
FUZZY
SEMANTIC
ENTITY_LINKED
```

MVP SHOULD prefer deterministic types:

```text
EXACT
NORMALIZED_EXACT
PHRASE
```

Probabilistic matching MUST NOT silently override approved deterministic terminology.

---

# Boundary Policy

Boundary rules MUST be language-aware.

Possible values:

```text
EXACT_TEXT
WHOLE_TOKEN
WORD_BOUNDARY
CHARACTER_BOUNDARY
PHRASE_BOUNDARY
ANY_POSITION
LANGUAGE_SPECIFIC
```

CRAI MUST NOT assume Latin whitespace word boundaries apply to Chinese, Japanese, Thai, or other languages.

---

# Fuzzy Matching

Fuzzy matching MAY support:

* OCR errors,
* missing diacritics,
* spelling errors,
* similar romanizations.

Fuzzy results MUST contain confidence/evidence.

Low-confidence fuzzy matches SHOULD produce suggestions rather than mandatory terminology enforcement.

---

# Semantic Matching

Semantic matching is probabilistic.

It MAY support:

* candidate generation,
* context enrichment,
* review assistance,
* entity linking.

Semantic matching SHOULD NOT silently defeat exact approved terminology.

---

# Glossary Match

A runtime `GlossaryMatch` is derived state.

Recommended structure:

```text
GlossaryMatch
├── matchId
├── entryId
├── entryRevisionId
├── sourceFormId
├── textBlockId
├── textBlockRevision
├── sourceRange?
├── matchedText?
├── normalizedText?
├── matchType
├── confidence?
├── resolutionState
└── matcherRevision
```

GlossaryMatch does NOT belong to the Glossary aggregate.

---

# Match Resolution

Possible runtime states:

```text
ACCEPTED
REJECTED
AMBIGUOUS
SHADOWED
OVERRIDDEN
CONFLICTED
SUGGESTED
EXPIRED
```

Resolution SHOULD preserve why a term was selected or rejected.

---

# Precedence Resolution

Recommended resolution sequence:

```text
Collect candidate Entry Revisions
          |
          v
Filter by Language Pair
          |
          v
Filter by Applicability
          |
          v
Evaluate Matching Policy
          |
          v
Rank by Scope Specificity
          |
          v
Rank by Language Specificity
          |
          v
Apply Authority / Lock
          |
          v
Apply Explicit Priority
          |
          v
Detect Conflicts
          |
          v
Resolved Terminology Set
```

Resolution MUST be deterministic where deterministic rules exist.

---

# Priority

Possible ranking dimensions include:

1. explicit operation override,
2. scope specificity,
3. authority/lock level,
4. approval,
5. Language-match specificity,
6. Match Type strength,
7. explicit Entry priority,
8. stable tie-breaker.

Revision recency SHOULD NOT automatically defeat an approved more-specific rule.

---

# Conflict

A Glossary Conflict occurs when simultaneously applicable rules define incompatible intent.

Examples:

```text
灵力 -> linh lực
```

and:

```text
灵力 -> linh khí
```

at equivalent authority and scope.

Conflicts MUST NOT be resolved silently when policy cannot determine a winner.

---

# Conflict Types

Possible types include:

```text
DUPLICATE_SOURCE
CONFLICTING_TARGET
PRESERVE_VS_TRANSLATE
FORBID_VS_PREFER
OVERLAPPING_PHRASE
SCOPE_COLLISION
LANGUAGE_COLLISION
ALIAS_COLLISION
ENTITY_COLLISION
INCOMPATIBLE_LOCK
AMBIGUOUS_CONTEXT
```

Conflict detection MAY be asynchronous or derived.

---

# Duplicate Entries

Potential duplicate detection MAY use:

* equivalent Source Forms,
* compatible Language scope,
* overlapping applicability,
* entity references,
* Rule Type,
* semantic similarity.

Potential duplicates SHOULD produce review candidates.

They MUST NOT automatically merge solely from matching strings.

---

# Entry Merge

Entries representing one concept MAY be merged.

Merge SHOULD:

* choose a surviving Entry identity,
* preserve all historical revisions,
* preserve Source Forms,
* preserve Target Forms,
* preserve aliases,
* preserve entity references,
* record lineage,
* redirect old identity when needed.

Historical Translation references MUST remain valid.

---

# Entry Split

An Entry representing multiple concepts MAY be split.

Example:

```text
Master
```

may mean:

* teacher,
* owner,
* rank,
* controller.

Split creates new Entry identities.

Historical lineage MUST remain traceable.

---

# Entry Lifecycle

Recommended statuses:

```text
DRAFT
ACTIVE
INACTIVE
DEPRECATED
MERGED
ARCHIVED
```

`REJECTED` is normally better represented as Review outcome rather than core lifecycle when possible.

Meaning:

* `DRAFT`: not normally used for production resolution.
* `ACTIVE`: eligible for resolution.
* `INACTIVE`: retained but excluded from new snapshots.
* `DEPRECATED`: no longer preferred for new Translation.
* `MERGED`: redirected into another Entry.
* `ARCHIVED`: historical only.

---

# Review Is Separate

Glossary Entry lifecycle and Review MUST remain separate.

Possible review states:

```text
UNREVIEWED
REVIEW_REQUESTED
IN_REVIEW
APPROVED
CHANGES_REQUESTED
REJECTED
```

Approval applies to an exact Entry Revision.

Editing an approved semantic definition creates a new revision requiring review according to policy.

---

# Locked Terminology

Locking is terminology authority, not ordinary Review lifecycle.

A lock MAY apply to an exact Entry Revision and scope.

Examples:

* main character name,
* official skill name,
* publisher terminology,
* user-pinned Translation.

Locked terminology:

* has high authority,
* requires explicit permission to change,
* cannot be silently replaced by import,
* SHOULD produce validation errors when violated.

A Project lock MUST NOT automatically become global.

---

# Glossary Candidate

`GlossaryCandidate` is a suggestion workflow artifact.

Candidates MAY come from:

* repeated terminology,
* user corrections,
* entity detection,
* Translation inconsistencies,
* imports,
* AI suggestions.

A candidate MUST NOT affect Translation merely because it exists.

It must become an approved/active Entry Revision according to policy.

---

# Learning from User Correction

Example:

```text
generated:
    linh khí

user correction:
    linh lực
```

The correction MAY generate:

```text
candidate:
    灵力 -> linh lực
```

The corrected Translation remains authoritative for its Translation Revision regardless of whether the candidate is accepted.

---

# Glossary Snapshot

Translation SHOULD consume an immutable resolved `GlossarySnapshot`.

Recommended representation:

```text
GlossarySnapshot
├── snapshotId
├── projectId?
├── languagePair
├── scopeContext
├── sourceGlossaryRevisions[]
├── includedEntryRevisions[]
├── resolutionPolicyRevision
├── createdAt
└── contentHash
```

A snapshot is not the mutable Glossary.

---

# Snapshot Resolution

A Snapshot SHOULD contain only Entry Revisions that are:

* active,
* applicable,
* Language-compatible,
* permitted by review policy,
* not shadowed,
* not blocked by unresolved conflicts.

The exact Snapshot MAY be operation-specific.

---

# Snapshot Identity

Snapshot semantic identity SHOULD be reproducible.

Equivalent:

```text
Entry Revisions
+
scope
+
Language Pair
+
resolution policy
```

SHOULD produce equivalent semantic content identity.

`contentHash` MAY participate in:

* Translation configuration identity,
* cache compatibility,
* audit,
* staleness detection.

---

# Snapshot Immutability

A Snapshot referenced by a Translation Revision MUST be immutable.

Glossary changes create a new Snapshot.

Historical Translation Revisions continue referencing old snapshots.

This prevents current terminology edits from rewriting historical Translation meaning.

---

# Snapshot vs Glossary Revision

`GlossaryRevision` and `GlossarySnapshot` are different concepts.

GlossaryRevision:

```text
version of a terminology collection
```

GlossarySnapshot:

```text
resolved effective terminology for a specific context
```

A Snapshot MAY combine entries from multiple Glossaries in future shared-glossary scenarios.

Therefore Snapshot identity MUST NOT be assumed to equal one Glossary Revision.

---

# Translation Integration

Recommended relationship:

```text
TextBlock Revision
       |
       v
Glossary Resolution
       |
       v
Glossary Snapshot
       |
       v
Translation Execution
       |
       v
Translation Revision
```

Translation Revision SHOULD preserve:

* Glossary Snapshot identity,
* relevant Entry Revision identities,
* terminology validation findings where required.

---

# Prompt Integration

Conversion of Glossary knowledge into provider instructions belongs to Context/Prompt compilation.

```text
Glossary Snapshot
      |
      v
Context Compiler
      |
      v
Provider-Neutral Terminology Context
      |
      v
Provider Adapter
```

Glossary Entries MUST NOT store provider-specific prompt fragments as canonical terminology data.

---

# Context Budget

Large Glossaries MAY exceed execution context budgets.

Selection MAY consider:

* actual source matches,
* scope,
* Character presence,
* authority,
* priority,
* semantic relevance,
* nearby context,
* provider context limits.

Selection MUST be traceable when it influences Translation.

The full Glossary Snapshot and the actual supplied subset SHOULD remain distinguishable where needed.

---

# Applied Glossary Entry

Translation lineage MAY record actual terminology usage.

Example:

```text
AppliedGlossaryEntry
├── entryId
├── entryRevisionId
├── sourceFormId?
├── targetFormId?
├── matchId?
├── applicationType
├── confidence?
└── validationResult?
```

Possible application types:

```text
CONTEXT_SUPPLIED
MATCHED
PROVIDER_APPLIED
POST_PROCESSED
USER_CONFIRMED
VALIDATION_ONLY
IGNORED
CONFLICTED
```

Provider compliance MUST NOT be assumed merely because terminology appeared in prompt context.

---

# Terminology Validation

Translation output MAY be validated against the effective Glossary Snapshot.

Possible checks:

* required form missing,
* forbidden form present,
* Preserve violated,
* Character name inconsistency,
* locked terminology violation,
* Script mismatch,
* unapproved alternative,
* ambiguous terminology.

Validation SHOULD produce structured findings.

---

# Validation Finding

Recommended structure:

```text
TerminologyFinding
├── findingId
├── translationRevisionId
├── entryRevisionId
├── severity
├── findingType
├── sourceRange?
├── targetRange?
├── expectedForms[]
├── observedForm?
├── confidence?
├── resolutionState
└── validatorRevision
```

Possible severity:

```text
INFO
WARNING
ERROR
BLOCKING
```

---

# Post-Processing

Glossary-aware automatic post-processing MAY exist but MUST be conservative.

It SHOULD run only where:

* terminology match is deterministic,
* source/output mapping is known,
* grammar cannot reasonably be corrupted,
* rule explicitly permits it,
* change is traceable.

Every automatic semantic modification contributes to a new Translation Revision or explicit Translation transformation record.

Provider output MUST NOT be silently mutated in place.

---

# Translation Staleness

Glossary changes MAY affect existing Translation Revisions.

Possible impact:

```text
NONE
METADATA_ONLY
REVIEW_RECOMMENDED
VALIDATION_REQUIRED
RETRANSLATION_RECOMMENDED
RETRANSLATION_REQUIRED
```

Impact MUST be dependency-aware.

---

# Selective Impact

Example:

```text
unrelated new term
    -> NONE

description changed
    -> NONE

preferred target changed for used term
    -> RETRANSLATION_RECOMMENDED

locked Character name changed
    -> RETRANSLATION_REQUIRED
```

CRAI MUST NOT mark every Project Translation stale after unrelated Glossary changes.

---

# Affected Translation Detection

Evidence MAY include:

* Applied Entry references,
* GlossaryMatch references,
* source text index,
* target text index,
* entity references,
* Translation context,
* scope,
* Snapshot membership.

Impact detection itself MAY be an application/runtime capability.

---

# Import

Glossary Import SHOULD operate through a reviewable Import Plan.

Supported formats MAY eventually include:

```text
CSV
TSV
JSON
YAML
TBX
TMX
Spreadsheet
```

MVP SHOULD prioritize simpler deterministic formats.

Import MUST NOT silently overwrite approved/locked terminology.

---

# Import Plan

Recommended:

```text
GlossaryImportPlan
├── importId
├── sourceFormat
├── sourceHash
├── proposedEntries[]
├── proposedRevisions[]
├── duplicates[]
├── conflicts[]
├── invalidRecords[]
├── languageMapping
├── scopeMapping
└── status
```

Import execution belongs to an import workflow.

---

# Export

Glossary MAY export:

* full collection,
* active entries,
* approved entries,
* selected Language Pair,
* selected scope,
* immutable Snapshot,
* provider-compatible derived format,
* review format.

Provider-specific export is derived representation.

It MUST NOT become canonical persisted Glossary state.

---

# Versioning

Versioned concepts MAY include:

```text
GlossaryRevision
GlossaryEntryRevision
GlossarySnapshot
MatchingPolicyRevision
ResolutionPolicyRevision
ValidationPolicyRevision
ImportFormatVersion
ExportFormatVersion
```

Historical resources MUST remain interpretable after policies evolve.

---

# Concurrency

Entry editing SHOULD support optimistic concurrency.

Possible checks:

```text
expectedEntryRevision
expectedGlossaryVersion
contentHash
```

Concurrent edits MUST NOT silently overwrite each other.

Because Entries are independently addressable, concurrent edits to unrelated Entries SHOULD NOT require locking the entire Glossary.

---

# Idempotency

Applicable operations SHOULD be idempotent.

Examples:

* importing the same source,
* publishing an unchanged Snapshot,
* applying an identical alias change,
* accepting the same revision twice.

Possible keys:

```text
operationId
sourceHash
entryContentHash
parentRevision
importId
```

---

# Deletion

Hard deletion SHOULD be exceptional.

Entry Revisions referenced by:

* Translation Revisions,
* Glossary Snapshots,
* Review,
* Audit,
* Import history

MUST normally remain resolvable.

Preferred operations:

```text
INACTIVATE
DEPRECATE
ARCHIVE
MERGE
REDIRECT
```

Hard deletion MAY be appropriate for:

* unreferenced drafts,
* legal/privacy requirements.

---

# Persistence

Recommended canonical records:

```text
Glossary
GlossaryRevision

GlossaryEntry
GlossaryEntryRevision

GlossarySnapshot
GlossarySnapshotEntry

GlossaryReview
GlossaryConflict
GlossaryCandidate
GlossaryImport
```

Derived structures MAY include:

```text
GlossarySearchIndex
GlossaryMatchingIndex
GlossaryEmbeddingIndex
GlossaryUsageIndex
```

Derived indexes MUST be rebuildable.

---

# Search and Matching Indexes

Indexes are infrastructure/derived state.

Index failure MUST NOT corrupt canonical terminology.

Every indexed terminology rule SHOULD remain traceable to exact Entry Revision identity.

---

# Cache

Glossary cache compatibility MAY use:

```text
GlossarySnapshotHash
LanguagePair
scope
MatchingPolicyRevision
MatcherRevision
NormalizationProfileRevision
TextBlockRevision
TranslationProfileRevision
```

Mutable `glossaryId` alone is insufficient.

---

# Security

Glossary actions MAY require permissions such as:

* view,
* suggest,
* create,
* edit,
* approve,
* publish,
* lock,
* import,
* export,
* archive.

Authorization infrastructure remains outside the Glossary domain.

Shared Glossaries SHOULD require stricter policy than Project-scoped Glossaries.

---

# Privacy

Glossary data MAY contain:

* unreleased names,
* plot terminology,
* licensed Translation terminology,
* private user terminology,
* publisher-approved terminology.

Rules SHOULD include:

* prevent implicit cross-Project leakage,
* send only relevant terminology to external providers,
* exclude private notes unless allowed,
* respect local-only execution,
* preserve export audit where required.

---

# Events

Core Glossary events MAY include:

```text
GlossaryCreated
GlossaryMetadataUpdated
GlossaryArchived
GlossaryRestored

GlossaryEntryCreated
GlossaryEntryRevisionPublished
GlossaryEntryActivated
GlossaryEntryDeactivated
GlossaryEntryDeprecated
GlossaryEntryMerged
GlossaryEntrySplit

GlossarySnapshotCreated
```

Review workflow MAY emit:

```text
GlossaryEntryApproved
GlossaryEntryRejected
GlossaryEntryLocked
GlossaryEntryUnlocked
```

Derived/workflow capabilities MAY emit:

```text
GlossaryConflictDetected
GlossaryCandidateCreated
GlossaryImportPlanned
GlossaryImportCompleted
```

The fact these events concern Glossary does not require one aggregate to own every event source.

---

# Architecture Invariants

1. `glossaryId` is stable.

2. Glossary identity and Glossary Revision identity are separate.

3. `entryId` and Entry Revision identity are separate.

4. GlossaryEntry SHOULD be independently addressable.

5. Ordinary Glossary operations MUST NOT require loading all Entries.

6. Published Glossary and Entry Revisions are immutable.

7. Translation references an immutable Glossary Snapshot when terminology context is used.

8. GlossarySnapshot references exact Entry Revisions.

9. Historical Translation Revisions preserve the exact terminology Snapshot they used.

10. Glossary terminology is provider-independent.

11. Provider prompt fragments MUST NOT become canonical Entry data.

12. Active Entries declare valid Language applicability.

13. LanguagePair direction is significant.

14. Source Forms and Target Forms remain distinct.

15. Canonical terminology remains distinguishable from aliases and OCR variants.

16. Entry Type and Rule Type remain separate.

17. Glossary defines matching intent; matching execution belongs outside canonical Entry state.

18. Runtime matches reference exact Entry Revisions.

19. Probabilistic matches MUST NOT silently override authoritative deterministic terminology.

20. Scope precedence MUST be deterministic.

21. Optional Book/Page scopes MUST NOT be required.

22. Shared ownership scopes MUST NOT be treated as mandatory parents of Project scope.

23. Equal-authority incompatible terminology produces an explicit conflict.

24. Conflicts MUST NOT be silently resolved when no deterministic rule exists.

25. Locked terminology MUST NOT be silently overwritten.

26. User Translation corrections MUST NOT automatically mutate Glossary truth.

27. Glossary candidates MUST NOT automatically affect Translation.

28. Provider output MUST remain traceable when Glossary post-processing changes Translation text.

29. Glossary changes invalidate only affected dependent artifacts.

30. New unrelated Entries MUST NOT stale every Translation.

31. Merge and split preserve terminology lineage.

32. Referenced Entry Revisions MUST remain historically resolvable.

33. Derived indexes MUST be rebuildable.

34. Cache compatibility uses immutable Snapshot/revision identity rather than mutable Glossary identity alone.

35. Project-private terminology MUST NOT leak implicitly across Projects.

36. Authoritative terminology changes SHOULD be auditable.

37. Glossary MUST NOT directly modify canonical TextBlock source content.

38. Glossary lifecycle, Entry lifecycle, Review lifecycle, and matching execution state MUST remain separate.

39. GlossarySnapshot is a resolved effective terminology set and MUST NOT be conflated with mutable Glossary state.

40. A Snapshot MAY eventually combine multiple Glossaries without changing Translation domain semantics.

---

# Recommended MVP Scope

Initial CRAI Glossary support SHOULD include:

* one Project-scoped Glossary,
* stable Glossary identity,
* independently addressable Entries,
* immutable Entry Revisions,
* `zh-Hans -> vi`,
* optional `zh-Hant -> vi`,
* Source Forms,
* one preferred Target Form,
* approved alternatives,
* basic Entry Types,
* `TRANSLATE`,
* `PRESERVE`,
* `TRANSLITERATE`,
* `AVOID`,
* `FORBID`,
* exact matching,
* normalized exact matching,
* phrase matching,
* deterministic longest applicable phrase policy,
* explicit Entry priority,
* Project scope,
* optional Book/Chapter scope,
* Draft/Active/Inactive/Deprecated lifecycle,
* basic Review approval,
* locked terminology,
* conflict detection,
* immutable Glossary Snapshot,
* terminology validation,
* user-created entries,
* Candidate generation from user correction,
* CSV/JSON import,
* CSV/JSON export,
* selective Translation staleness.

MVP MAY defer:

* shared/global Glossaries,
* Workspace inheritance,
* semantic matching,
* embedding indexes,
* morphological matching,
* regex enforcement,
* fuzzy automatic enforcement,
* advanced context-rule language,
* detailed inflection,
* dynamic entity linking,
* TBX/TMX,
* automatic terminology learning,
* collaborative editing,
* advanced provider-specific glossary APIs,
* complex Page-range exceptions,
* cross-Project synchronization.

---

# Open Decisions

The following SHOULD remain open until prototype validation:

* whether Project supports one or several composable Glossaries,
* whether GlossaryRevision references all Entry Revisions or only collection-level state,
* Snapshot storage representation,
* supported initial Match Types,
* regex policy,
* fuzzy matching policy,
* Simplified/Traditional relationship modeling,
* OCR-variant persistence,
* alias approval policy,
* contextual-rule representation,
* semantic matching enforcement,
* which terminology violations block Translation publication,
* deterministic post-processing policy,
* Vietnamese grammatical variants,
* Candidate aggregate design,
* Candidate confidence,
* automatic Candidate creation,
* import activation policy,
* future shared Glossary permissions,
* operation-level override of locked terminology,
* selective staleness implementation,
* persisted vs recomputed GlossaryMatch,
* context-budget selection algorithm,
* whole Snapshot vs matched-entry prompt context,
* Character-name ownership boundary,
* licensing restrictions for imported dictionaries.

---

# Ownership Summary

```text
Glossary Domain

Glossary owns
    collection identity
    collection metadata
    ownership scope
    collection policies
    collection lifecycle

GlossaryEntry owns
    terminology concept identity
    entry lifecycle
    active revision
    lineage

GlossaryEntryRevision owns
    source forms
    target forms
    language scope
    terminology rule
    matching intent
    applicability
    priority
    immutable terminology definition

GlossarySnapshot owns
    resolved immutable terminology context

references
    Project
    optional Book
    Chapter
    optional Page
    Character / semantic entities
    Language values

derived
    GlossaryMatch
    conflicts
    candidates
    indexes
    usage analytics

does not own
    TextBlock content
    Translation execution
    provider execution
    prompt generation
    Review execution
    search infrastructure
    embedding infrastructure
```

Glossary is therefore a terminology knowledge domain composed of scalable independently addressable resources, not one monolithic aggregate containing every terminology-related object.

---

# Related Documents

Domain:

* `README.md`
* `PROJECT.md`
* `BOOK.md`
* `CHAPTER.md`
* `PAGE.md`
* `TEXT_BLOCK.md`
* `TRANSLATION.md`
* `LANGUAGE.md`
* `CHARACTER.md`
* `PROFILE.md`
* `SESSION.md`

Architecture:

* `docs/architecture/CAPABILITY_MAP.md`
* `docs/architecture/OWNERSHIP_MAP.md`
* `docs/architecture/DATA_FLOW.md`
* `docs/architecture/STATE_MACHINE.md`
* `docs/architecture/EVENT_BUS.md`
* `docs/architecture/MODULE_DEPENDENCY.md`

AI / Translation:

* `docs/architecture/ai/PIPELINE.md`
* `docs/architecture/ai/CONTEXT.md`
* `docs/architecture/ai/PROMPTS.md`
* `docs/architecture/ai/REQUEST.md`
* `docs/architecture/ai/RESPONSE.md`
* `docs/architecture/ai/MEMORY.md`
* `docs/architecture/ai/ROUTING.md`
* `docs/architecture/ai/CACHE.md`

Presentation:

* `docs/architecture/presentation/FONTS.md`
* `docs/architecture/presentation/LAYOUT.md`
* `docs/architecture/presentation/TYPOGRAPHY.md`

Module contracts remain authoritative for runtime matching, provider execution, review workflows, and infrastructure behavior.
