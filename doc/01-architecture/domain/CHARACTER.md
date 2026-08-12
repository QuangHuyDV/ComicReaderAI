# Character Domain

* **Document:** Domain / Character
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

The `Character` domain defines stable fictional and speaker-like identities used by CRAI across source understanding, dialogue attribution and Translation context.

Character information may support:

* character identity,
* original names,
* aliases,
* personas,
* titles,
* stable traits,
* speech characteristics,
* speaker resolution,
* relationship-aware address,
* Translation context,
* Glossary association,
* continuity across Chapters,
* spoiler-safe context construction.

The Character domain provides authoritative identity and approved character facts.

It MUST remain independent from:

* OCR providers,
* Translation providers,
* computer-vision models,
* prompt formats,
* face-recognition implementations,
* speaker-detection implementations.

---

# Domain Role

Character is primarily a Project-scoped identity.

```text
Project
   |
   +-- Books / Chapters / Pages / TextBlocks
   |
   +-- Characters
         |
         +-- Character Revisions
         +-- Character Names
         +-- Character Profiles
         +-- Personas
```

Characters may be referenced by:

* Glossary Entries,
* Speaker Attributions,
* Character Relationships,
* Character Appearances,
* Translation Context Snapshots,
* Translation Revisions,
* Review records,
* Recognition profiles.

Character does NOT own those external resources merely because they reference Character identity.

---

# Domain Boundaries

Recommended conceptual separation:

```text
Character
    stable identity
    lifecycle
    active revision
    lineage

CharacterRevision
    immutable approved character facts

CharacterName
    identity-related name representation

CharacterPersona
    scoped identity presentation

CharacterRelationship
    relationship between Characters

SpeakerAttribution
    TextBlock-to-Character attribution

CharacterAppearance
    content-location evidence

CharacterCandidate
    unconfirmed identity hypothesis

CharacterObservation
    unconfirmed fact hypothesis

CharacterContextSnapshot
    immutable Translation context
```

These concepts belong to the Character bounded domain or adjacent Character capabilities.

They MUST NOT be interpreted as one large transactional aggregate.

---

# Character Aggregate

Recommended Character Aggregate:

```text
Character
├── characterId
├── projectId
├── characterType
├── activeRevisionId?
├── canonicalNameReference?
├── lifecycleStatus
├── lineage
├── createdAt
├── updatedAt
└── version
```

The Character Aggregate owns:

* stable Character identity,
* Project ownership,
* Character lifecycle,
* active Character Revision,
* merge/split lineage,
* canonical identity references.

It does NOT directly own:

* Speaker Attribution,
* Character Appearance,
* Recognition embeddings,
* complete Relationship graph,
* Glossary rules,
* Translation results,
* provider execution,
* Character Candidates,
* AI observations.

---

# Character Identity

Every canonical Character has a stable identity.

```text
characterId != characterRevisionId
```

`characterId` represents the continuing fictional or speaker-like entity.

It remains stable across:

* name changes,
* aliases,
* translated names,
* appearances,
* Chapters,
* Pages,
* Translation Revisions,
* Character metadata revisions.

Changing known information MUST NOT create a new Character identity.

A new Character identity SHOULD be created only when CRAI represents a distinct entity.

---

# Character Revision

Character details belong to immutable revisions.

Recommended structure:

```text
CharacterRevision
├── characterRevisionId
├── characterId
├── names[]
├── stableTraits[]
├── speechProfile?
├── identityMetadata
├── notes?
├── externalReferences[]
├── spoilerMetadata
├── parentRevisionId?
├── createdBy
├── createdAt
└── contentHash
```

Once referenced by:

* Character Context Snapshot,
* Translation lineage,
* Review,
* Audit,

a Character Revision MUST remain immutable.

Editing semantic Character information creates a new revision.

---

# Character Type

Possible Character Types MAY include:

```text
PERSON
CREATURE
SPIRIT
DEITY
ARTIFICIAL_INTELLIGENCE
SENTIENT_OBJECT
GROUP_SPEAKER
NARRATOR
SYSTEM_VOICE
UNKNOWN_SPEAKER
CUSTOM
```

Character Type describes the kind of identity.

It MUST NOT replace actual identity.

Advanced concepts such as:

* disguised identity,
* alternate persona,
* reincarnated identity,
* possessed identity

SHOULD normally be modeled through Persona or explicit identity relationships rather than Character Type alone.

---

# Character Name

A `CharacterName` represents an identity-related name form.

Recommended structure:

```text
CharacterName
├── nameId
├── characterId
├── text
├── language
├── script?
├── nameType
├── authority?
├── applicability?
├── revealBoundary?
├── status
└── evidenceReferences[]
```

Possible Name Types:

```text
ORIGINAL
CANONICAL
GIVEN_NAME
FAMILY_NAME
FULL_NAME
COURTESY_NAME
ART_NAME
CODE_NAME
NICKNAME
ALIAS
DISGUISE_NAME
TEMPORARY_IDENTIFIER
ROMANIZED
TRANSLITERATED
LOCALIZED
DEPRECATED
```

Name Type and Language MUST remain separate.

---

# Canonical Name

Character MAY expose one canonical name reference for organization and UI display.

The canonical name MAY be:

* original source name,
* authoritative published name,
* user-selected name,
* temporary descriptive identifier.

Example:

```text
Character CH-001

canonical:
    林月
```

Canonical display identity does NOT determine Translation output.

---

# Original Name

Original names preserve source identity.

Requirements:

* preserve original Script,
* preserve meaningful punctuation,
* preserve Language,
* avoid destructive normalization,
* preserve evidence when automatically extracted.

An original name MUST NOT be destroyed when a translated/localized name is added.

---

# Romanized and Transliterated Names

Romanization and transliteration are different representations.

Example:

```text
Original:
    林月

Romanized:
    Lín Yuè

Vietnamese transliteration/localized reading:
    Lâm Nguyệt
```

The transformation convention SHOULD be recorded when relevant.

Neither representation automatically becomes canonical Translation terminology.

---

# Character Name vs Glossary Terminology

Character and Glossary have different ownership responsibilities.

```text
Character
    owns:
        identity
        identity-related names
        aliases
        name history
        reveal boundaries
        character-specific authority

Glossary
    owns:
        Translation terminology rules
        source matching
        preferred target terminology
        forbidden terminology
        Translation enforcement
```

Example:

```text
Character CH-001

Original Name:
    林月
```

Glossary MAY contain:

```text
GlossaryEntry GE-100
    source: 林月
    target: Lâm Nguyệt
    rule: TRANSLITERATE
    characterId: CH-001
```

Character MUST NOT duplicate complete Glossary matching and precedence logic.

Glossary MUST NOT become the canonical owner of Character identity.

---

# Name Resolution

Character may provide candidate name facts.

Final Translation terminology resolution SHOULD combine:

```text
Character identity
+
Story scope
+
Spoiler boundary
+
Glossary Snapshot
+
Translation Profile
```

Therefore Character alone MUST NOT determine final translated spelling when an authoritative Glossary rule exists.

---

# Alias

An Alias is an alternative identity-related name.

Examples:

* nickname,
* code name,
* disguise name,
* abbreviated name,
* alternate spelling,
* common misspelling.

OCR variants MAY be retained as recognition evidence.

OCR variants MUST NOT automatically become approved display names.

Translation aliases requiring terminology enforcement SHOULD be represented or referenced through Glossary.

---

# Title

Character MAY record character-specific title facts.

Example:

```text
Character:
    CH-001

Title:
    Sect Master

valid:
    Chapters 80+
```

The fact:

```text
CH-001 holds title Sect Master
```

belongs to Character/contextual character knowledge.

The terminology rule:

```text
宗主 -> Tông chủ
```

belongs to Glossary.

Character Title MAY therefore reference a Glossary Entry.

---

# Character Facts

CharacterRevision MAY contain relatively stable character facts useful to interpretation.

Examples:

```text
species
approximateAgeGroup
occupation
generalSocialRank
primaryAffiliation
narrativeRole
stableSpeechStyle
```

Character MUST NOT become a general-purpose story-state database.

---

# Stable vs Dynamic Facts

Stable facts MAY belong to CharacterRevision.

Examples:

* species,
* base identity,
* original family,
* stable speech tendency.

Dynamic facts SHOULD normally use scoped contextual records.

Examples:

* current injury,
* temporary faction,
* current rank,
* current disguise,
* current emotional state,
* current location,
* current allegiance.

Dynamic facts MUST NOT overwrite permanent identity facts.

---

# Story State Is Not Character Lifecycle

Story facts such as:

```text
ALIVE
DECEASED
MISSING
INJURED
IMPRISONED
DISGUISED
```

are NOT Character Aggregate lifecycle states.

They belong to story/context state with appropriate story and spoiler scope.

For example:

```text
Character CH-001
    lifecycle: ACTIVE

StoryState at Chapter 120
    lifeState: DECEASED
```

This prevents domain lifecycle from leaking spoilers or conflating persistence with narrative state.

---

# Character Lifecycle

Recommended canonical lifecycle:

```text
ACTIVE
INACTIVE
ARCHIVED
MERGED
```

Meaning:

`ACTIVE`

Character is available for normal domain use.

`INACTIVE`

Character remains canonical but is excluded from ordinary processing.

`ARCHIVED`

Character is retained primarily for historical purposes.

`MERGED`

Character identity redirects to another canonical Character.

Candidate and Rejected are NOT canonical Character lifecycle states.

Split is an operation/lineage event rather than a permanent lifecycle category for every resulting identity.

---

# Character Candidate

A Character Candidate represents an unconfirmed identity hypothesis.

Recommended structure:

```text
CharacterCandidate
├── candidateId
├── projectId
├── proposedNames[]
├── appearanceReferences[]
├── dialogueReferences[]
├── observations[]
├── possibleCharacterMatches[]
├── confidence
├── evidenceReferences[]
├── detectorRevision?
└── reviewState
```

Candidate is NOT a Character.

Candidate MAY be:

```text
PROMOTED
MERGED_INTO_EXISTING
REJECTED
EXPIRED
```

without polluting Character lifecycle.

---

# Candidate Promotion

Candidate promotion MAY:

* create a Character,
* associate evidence with an existing Character,
* create an Alias suggestion,
* create Speaker Attribution,
* create Appearance records,
* create duplicate-review workflow.

Promotion MUST preserve source evidence.

---

# Character Observation

An Observation represents uncertain or extracted character information.

Examples:

```text
appears angry
possibly the speaker
wearing red clothing
uses formal speech
may be called "Master"
```

Observation is NOT canonical Character truth.

Recommended structure:

```text
CharacterObservation
├── observationId
├── characterId?
├── candidateId?
├── observationType
├── value
├── evidenceReferences[]
├── confidence
├── storyScope
├── observer
├── observerRevision
└── reviewState
```

Approved observations MAY later produce Character revisions or contextual facts.

---

# Provenance and Confidence

Canonical truth, Review state and confidence MUST remain separate.

For example:

```text
confidence:
    0.82

provenance:
    AI_INFERENCE

reviewState:
    UNREVIEWED
```

MUST NOT be collapsed into a state such as:

```text
AI_SUGGESTED
```

Provenance MAY include:

```text
USER
IMPORT
AI_INFERENCE
SOURCE_STRUCTURE
PUBLISHER_METADATA
EXTERNAL_REFERENCE
SYSTEM_DERIVED
```

---

# Review

Review applies to exact semantic revisions or proposed changes.

Possible states:

```text
UNREVIEWED
REVIEW_REQUESTED
IN_REVIEW
APPROVED
CHANGES_REQUESTED
REJECTED
```

Approval applies to an exact Character Revision or scoped domain record.

Editing approved semantic content creates a new revision requiring review according to policy.

Review state is NOT Character lifecycle.

---

# Authority and Lock

Locking represents authority.

It MUST remain separate from Review state.

Possible structure:

```text
CharacterAuthority
├── characterId
├── characterRevisionId?
├── fieldPaths[]
├── scope
├── authorityLevel
├── actor
├── reason
└── createdAt
```

Locked data MUST NOT be silently overwritten by:

* AI suggestions,
* imports,
* automatic learning,
* ordinary edits without sufficient authority.

Locks MAY be field-specific.

---

# Pronoun Information

Character MAY contain stable pronoun-related facts.

Examples:

```text
grammaticalGender
preferredThirdPersonReference
stableSelfReferenceTendency
```

Unknown information MUST remain unknown.

CRAI MUST NOT infer unsupported gender information.

A global Character pronoun profile MUST NOT be treated as sufficient for Vietnamese Translation.

---

# Vietnamese Address Context

Vietnamese address depends on relationships and context.

Relevant dimensions include:

* speaker,
* listener,
* relative age,
* hierarchy,
* family relation,
* intimacy,
* formality,
* historical setting,
* story period,
* Translation style.

Therefore:

```text
Character
    alone
```

cannot determine:

```text
ta
tôi
con
đệ tử
ngươi
anh
chị
sư phụ
đại nhân
```

correctly in all dialogue.

---

# Address Rule

Relationship-specific address rules SHOULD belong to relationship/context resolution rather than the base Character aggregate.

Recommended representation:

```text
AddressRule
├── speakerCharacterId
├── listenerCharacterId
├── speakerSelfReference
├── listenerAddressForm
├── thirdPersonReference?
├── language
├── formality?
├── storyScope
├── translationProfileScope?
├── glossaryReferences[]
├── authority?
└── reviewState
```

Example:

```text
A -> B
    self: ta
    address: ngươi

B -> A
    self: đệ tử
    address: sư phụ
```

---

# Speech Profile

Character MAY own stable speech tendencies.

Recommended structure:

```text
SpeechProfile
├── languageScope
├── register
├── formality
├── sentenceStyle
├── vocabularyPreferences[]
├── verbalHabits[]
├── honorificBehavior?
├── dialectNotes?
├── restrictions[]
├── applicability
└── confidence?
```

Speech Profile SHOULD remain descriptive and provider-independent.

---

# Speech Habit

Speech habits MAY include:

* catchphrases,
* repeated interjections,
* characteristic sentence endings,
* archaic vocabulary,
* third-person self-reference,
* unusually concise speech.

Speech habits MUST NOT be converted into blind mechanical replacements that damage target-language grammar.

---

# Character Relationship

A Character Relationship is independently addressable.

Recommended structure:

```text
CharacterRelationship
├── relationshipId
├── sourceCharacterId
├── targetCharacterId
├── relationshipType
├── direction
├── activeRevisionId
└── lifecycleStatus
```

RelationshipRevision MAY contain:

```text
RelationshipRevision
├── relationshipRevisionId
├── relationshipId
├── storyScope
├── revealBoundary
├── confidence?
├── addressRules[]
├── evidenceReferences[]
├── parentRevisionId?
└── contentHash
```

Character does NOT own the complete relationship graph.

---

# Relationship Direction

Relationships MUST preserve direction when meaning differs.

```text
MASTER -> DISCIPLE
```

is not equivalent to:

```text
DISCIPLE -> MASTER
```

Directional relationships MAY provide different Address Rules.

---

# Relationship History

Relationships may change during the story.

Example:

```text
Chapter 1-30:
    STRANGER

Chapter 31-80:
    FRIEND

Chapter 81+:
    RIVAL
```

Translation of earlier Chapters MUST use the applicable historical Relationship Revision.

Future relationship state MUST NOT leak backward through context.

---

# Character Persona

Persona represents a scoped presentation of one underlying Character identity.

Possible examples:

* disguise,
* secret identity,
* transformation,
* public persona,
* possessed presentation.

Recommended structure:

```text
CharacterPersona
├── personaId
├── characterId
├── names[]
├── speechProfile?
├── appearanceProfileReference?
├── storyScope
├── revealBoundary
├── reviewState
└── authority?
```

Persona SHOULD remain subordinate to Character only when one stable underlying identity is known.

---

# Unknown Identity and Spoilers

Before an identity reveal, CRAI MAY intentionally preserve separate provisional identities.

Example:

```text
Chapter 1-69:
    Masked Swordsman

Chapter 70:
    revealed as General Lý
```

The system MUST preserve the earlier reader-visible identity boundary.

A later merge MUST NOT cause earlier Translation context to reveal:

```text
Masked Swordsman == General Lý
```

before Chapter 70.

---

# Complex Identity

Cases such as:

* reincarnation,
* body swap,
* possession,
* multiple consciousnesses,
* clones,
* split personalities

may require distinguishing:

```text
underlying identity
visible body
public identity
speaker identity
reader knowledge
```

CRAI MUST NOT force all such cases into one universal model.

Advanced identity modeling MAY be deferred beyond MVP.

---

# Character Appearance

Character Appearance links identity to content evidence.

Recommended structure:

```text
CharacterAppearance
├── appearanceId
├── characterId?
├── characterRevisionId?
├── candidateId?
├── chapterId
├── pageId?
├── imageVersionId?
├── region?
├── appearanceType
├── confidence?
├── identificationSource
├── evidenceReferences[]
└── reviewState
```

Appearance is contextual/derived data.

It MUST remain outside the Character aggregate.

---

# Recognition Profile

Recognition infrastructure MAY maintain:

```text
RecognitionProfile
├── characterId
├── referenceImageIds[]
├── visualDescriptorReferences[]
├── embeddingReferences[]
├── modelRevision
└── preprocessingRevision
```

Raw embeddings MUST NOT become canonical Character facts.

They are:

* model-specific,
* version-specific,
* rebuildable,
* potentially sensitive.

---

# Speaker Attribution

Speaker Attribution associates a TextBlock Revision with a possible Character.

Recommended structure:

```text
SpeakerAttribution
├── attributionId
├── textBlockId
├── textBlockRevision
├── characterId?
├── characterRevisionId?
├── candidateId?
├── attributionMethod
├── confidence?
├── evidenceReferences[]
├── reviewState
└── attributionRevision
```

Speaker Attribution MUST remain outside Character aggregate.

---

# Attribution Authority

Attribution evidence SHOULD have explicit authority.

Example precedence MAY be:

```text
User Confirmed
    >
Explicit Source Speaker Tag
    >
Imported Authoritative Metadata
    >
Strong Structural Evidence
    >
Visual / Spatial Inference
    >
Dialogue-Style Inference
    >
Generic AI Guess
```

Exact policy belongs to Speaker Attribution capability.

Character MUST NOT encode detector-specific precedence.

---

# Low-Confidence Speaker

Low-confidence attribution MUST preserve uncertainty.

It MUST NOT silently activate highly specific:

* pronouns,
* self-reference,
* relationship address,
* Speech Profile.

The Translation capability MAY:

* use generic context,
* preserve ambiguity,
* request Review,
* retain several speaker candidates.

---

# Unknown Speaker

Unknown Speaker MUST be explicitly representable.

Recommended semantic representation:

```text
SpeakerAttribution
    characterId: null
    resolutionState: UNKNOWN
```

A Project-level Unknown Speaker Character MAY be used for UI or workflow convenience but MUST NOT be required for domain correctness.

Unknown Speaker MUST NOT automatically become Narrator.

---

# Narrator

Narrator MAY be represented as Character when there is stable identity or voice behavior.

Examples:

* first-person narrator,
* recurring narrator,
* named narrator,
* Character narrator.

Neutral narration MAY instead use Narration Context without Character identity.

Therefore Narrator is NOT universally required to be a Character.

---

# System Voice

A repeated system-like speaker MAY be represented as Character when stable identity/style matters.

Examples:

* game system,
* divine announcement,
* AI assistant,
* automated broadcast.

One-off interface text SHOULD NOT require artificial Character creation.

---

# Character Group

Group speakers MAY initially use Character Type:

```text
GROUP_SPEAKER
```

Examples:

* Crowd,
* Soldiers,
* Council,
* Villagers.

Advanced group membership and collective identity MAY later become a separate domain concept.

---

# Spoiler Boundary

Character information MAY contain spoilers.

Examples:

* true name,
* future title,
* secret identity,
* hidden relationship,
* future faction,
* death,
* betrayal.

Spoiler-sensitive information MUST carry explicit applicability or reveal metadata.

Recommended:

```text
RevealBoundary
├── chapterId?
├── pageId?
├── sourcePosition?
├── minimumReaderProgress?
└── visibilityPolicy
```

Book/Page MUST remain optional where the content model does not contain them.

---

# Reader Knowledge vs Story Truth

CRAI SHOULD distinguish:

```text
story truth
```

from:

```text
information available to the reader at a given point
```

Example:

```text
Story truth:
    Masked Swordsman is General Lý

Reader knowledge before Chapter 70:
    identity unknown
```

Translation context MUST normally respect reader knowledge.

This distinction is essential for spoiler-safe processing.

---

# Character Context Snapshot

Translation MUST consume immutable Character Context when Character-specific information affects output.

Recommended structure:

```text
CharacterContextSnapshot
├── snapshotId
├── projectId
├── storyScope
├── spoilerBoundary
├── characterRevisionReferences[]
├── relationshipRevisionReferences[]
├── speakerAttributionReferences[]
├── selectedNameFacts[]
├── addressRules[]
├── speechProfiles[]
├── contextPolicyRevision
├── createdAt
└── contentHash
```

The Snapshot contains resolved effective context.

It is NOT mutable Character state.

---

# Snapshot Selection

Relevant Character context MAY be selected from:

1. confirmed speaker,
2. confirmed listener,
3. explicit Character mentions,
4. visible Characters,
5. nearby dialogue history,
6. relevant Relationships,
7. Chapter cast,
8. user-pinned Characters,
9. semantic retrieval.

Selection MUST respect:

* story boundary,
* spoiler boundary,
* Project visibility,
* confidence,
* Review policy,
* context budget.

---

# Snapshot Immutability

Once referenced by a Translation Revision:

```text
CharacterContextSnapshot
```

MUST remain immutable.

Later Character changes create a new Snapshot.

Historical Translation Revisions retain their original Snapshot.

---

# Character Snapshot vs Character Revision

They MUST remain distinct.

CharacterRevision:

```text
immutable facts for one Character
```

CharacterContextSnapshot:

```text
resolved multi-character context for one Translation situation
```

A Snapshot MAY contain:

* several Characters,
* several Relationships,
* Speaker Attribution,
* applicable Address Rules,
* spoiler filtering.

Therefore:

```text
CharacterContextSnapshot != CharacterRevision
```

---

# Translation Integration

Conceptual flow:

```text
TextBlock Revision
        |
        +--> Speaker Attribution
        |
        +--> Character Resolution
        |
        +--> Relationship Resolution
        |
        +--> Glossary Resolution
        |
        v
Character Context Snapshot
        |
        v
Translation Context Compiler
        |
        v
Translation Execution
```

Translation MUST NOT directly depend on mutable live Character state.

---

# Provider Independence

Character records MUST NOT contain canonical provider-specific prompts.

Recommended flow:

```text
Character Context Snapshot
        |
        v
Context Compiler
        |
        v
Provider-Neutral Character Context
        |
        v
Provider Adapter
```

Provider formatting belongs outside Character domain.

---

# Applied Character Context

Translation lineage MAY record actual applied Character context.

Example:

```text
AppliedCharacterContext
├── characterId
├── characterRevisionId
├── contextRole
├── speakerAttributionId?
├── selectedNameReference?
├── relationshipRevisionId?
├── appliedAddressRule?
├── appliedSpeechProfile?
└── applicationResult
```

Possible roles:

```text
SPEAKER
LISTENER
MENTIONED
VISIBLE
NARRATOR
RELATIONSHIP_CONTEXT
BACKGROUND_CONTEXT
USER_PINNED
```

---

# Character Validation

Character-aware validation MAY detect:

* incorrect Character name,
* future name leakage,
* wrong pronoun,
* wrong self-reference,
* relationship address mismatch,
* Speech Profile inconsistency,
* incorrect speaker attribution,
* title mismatch,
* Character identity conflict,
* spoiler leakage.

Findings MUST reference exact revisions/context where possible.

---

# Validation Finding

Recommended structure:

```text
CharacterValidationFinding
├── findingId
├── translationRevisionId
├── characterId?
├── characterRevisionId?
├── relationshipRevisionId?
├── speakerAttributionId?
├── findingType
├── severity
├── sourceRange?
├── targetRange?
├── expectedContext?
├── observedText?
├── confidence?
├── validatorRevision
└── resolutionState
```

---

# User Correction

Users MAY correct:

* Character identity,
* Character name,
* speaker attribution,
* pronoun,
* self-reference,
* address form,
* title,
* relationship,
* alias,
* merge/split decision.

A correction MAY create:

```text
new Translation Revision
new Speaker Attribution Revision
Character change proposal
Glossary Candidate
Relationship Revision proposal
Character Candidate resolution
```

A Translation correction MUST NOT automatically mutate canonical Character truth.

---

# Character Merge

Merge consolidates identities confirmed to represent the same underlying Character.

Merge SHOULD:

* choose surviving Character identity,
* preserve Character Revisions,
* preserve aliases and names,
* preserve Appearances,
* preserve Speaker Attributions,
* preserve Relationships,
* preserve Glossary references,
* preserve redirect lineage,
* preserve reveal boundaries,
* detect conflicts.

Historical references MUST remain resolvable.

---

# Merge Is Spoiler-Aware

Merge MUST NOT imply that all historical contexts knew the merged identity.

Example:

```text
CH-012
    Masked Swordsman

CH-031
    General Lý

Reveal:
    Chapter 70
```

After merge:

```text
canonical underlying identity:
    one Character
```

but historical context before Chapter 70 MUST still preserve the masked identity boundary.

---

# Character Split

Split repairs an incorrectly combined identity.

Split SHOULD:

* create distinct Character IDs,
* preserve source lineage,
* redistribute Names,
* redistribute Appearances,
* redistribute Speaker Attributions,
* redistribute Relationships,
* reconcile Glossary references,
* identify affected Translation artifacts.

Historical evidence MUST NOT be destroyed.

---

# Character Change Impact

Character changes MAY produce:

```text
NONE
METADATA_ONLY
REVIEW_RECOMMENDED
VALIDATION_REQUIRED
RETRANSLATION_RECOMMENDED
RETRANSLATION_REQUIRED
```

Examples:

```text
search note changed
    -> NONE

new unrelated alias
    -> NONE or VALIDATION

preferred applicable name corrected
    -> RETRANSLATION_RECOMMENDED

locked applicable name corrected
    -> RETRANSLATION_REQUIRED

speaker attribution changed
    -> affected dialogue re-evaluation

relationship address rule changed
    -> affected dialogue review
```

Only affected Translation artifacts SHOULD become stale.

---

# Impact Detection

Evidence MAY include:

* Character Context Snapshot references,
* Applied Character Context,
* Speaker Attribution,
* Relationship Revision,
* Glossary Entry references,
* source Character mentions,
* story scope,
* Validation Findings.

Changing one Character MUST NOT invalidate every Translation in the Project.

---

# Import

Character import SHOULD create a reviewable Import Plan.

Possible sources:

* JSON,
* CSV,
* YAML,
* spreadsheet,
* publisher Character sheet,
* external metadata,
* another CRAI Project.

Import MUST NOT silently overwrite approved or locked Character information.

---

# Character Import Plan

Recommended structure:

```text
CharacterImportPlan
├── importId
├── sourceHash
├── proposedCharacters[]
├── proposedRevisions[]
├── proposedNames[]
├── duplicateCandidates[]
├── conflicts[]
├── invalidRecords[]
└── reviewState
```

Import workflow is not Character aggregate state.

---

# Export

Character export MAY support:

* complete Project cast,
* selected story range,
* approved Characters,
* spoiler-safe view,
* internal full view,
* Character Context Snapshot,
* CRAI round-trip format.

Spoiler-safe exports MUST declare their story boundary.

---

# Persistence

Recommended canonical persistence:

```text
Character
CharacterRevision
CharacterName
CharacterPersona
CharacterAuthority
CharacterMergeLineage
CharacterSplitLineage
CharacterContextSnapshot
CharacterContextSnapshotItem
```

Related independently addressable records:

```text
CharacterRelationship
RelationshipRevision
AddressRule

SpeakerAttribution

CharacterAppearance

CharacterCandidate
CharacterObservation

RecognitionProfile
```

Derived infrastructure:

```text
CharacterSearchIndex
CharacterRecognitionIndex
CharacterEmbeddingIndex
CharacterUsageIndex
```

Derived indexes MUST be rebuildable.

---

# Cache Participation

Character-aware cache compatibility MAY include:

```text
CharacterContextSnapshotHash
CharacterRevisionIds
RelationshipRevisionIds
SpeakerAttributionRevisions
StoryScope
SpoilerBoundary
TranslationProfileRevision
ContextCompilerRevision
```

Mutable `characterId` alone is insufficient for cache correctness.

---

# Concurrency

Character edits SHOULD use optimistic concurrency.

Possible checks:

```text
expectedCharacterVersion
expectedCharacterRevisionId
expectedParentRevisionId
contentHash
authorityState
```

Independent Relationship or Speaker Attribution changes SHOULD NOT require locking the Character Aggregate.

---

# Idempotency

Idempotency MAY apply to:

* importing identical Character data,
* promoting the same Candidate,
* adding an existing Name,
* approving an existing Revision,
* merging the same identities,
* creating an equivalent Context Snapshot.

Possible inputs:

```text
operationId
sourceHash
evidenceHash
characterContentHash
parentRevisionId
```

---

# Deletion

Hard deletion SHOULD be exceptional.

Referenced Characters and Character Revisions MUST normally remain resolvable.

Preferred operations:

```text
INACTIVATE
ARCHIVE
MERGE
REDIRECT
```

Rejected Candidates MAY be retained according to workflow retention policy without becoming canonical Characters.

---

# Security

Character permissions MAY include:

* View,
* View Spoilers,
* Suggest,
* Create,
* Edit,
* Approve,
* Lock,
* Merge,
* Split,
* Import,
* Export,
* Manage Recognition Data.

Spoiler access SHOULD remain distinct from ordinary Character visibility.

---

# Privacy

Character data MAY contain:

* unreleased plot details,
* licensed names,
* publisher terminology,
* private reading notes,
* uploaded reference images,
* derived recognition data.

Requirements include:

* prevent cross-Project leakage,
* respect spoiler visibility,
* minimize provider context,
* exclude irrelevant Characters,
* respect local-only execution,
* protect recognition artifacts,
* avoid using fictional-character recognition infrastructure for real-person biometric identification.

---

# Events

Core Character domain events MAY include:

```text
CharacterCreated
CharacterRevisionPublished
CharacterActivated
CharacterInactivated
CharacterArchived
CharactersMerged
CharacterSplit
```

Character naming MAY emit:

```text
CharacterNameAdded
CharacterNameChanged
CharacterNameDeprecated
```

Related capabilities MAY emit:

```text
CharacterCandidateDetected
CharacterCandidatePromoted

CharacterRelationshipChanged

CharacterAppearanceDetected

SpeakerAttributed
SpeakerAttributionCorrected

CharacterContextSnapshotCreated
```

Review/authority workflows MAY emit:

```text
CharacterApproved
CharacterRejected
CharacterLocked
CharacterUnlocked
```

The existence of a Character-related event does NOT imply the Character Aggregate owns the event-producing resource.

---

# Architecture Invariants

1. Character is an Aggregate Root with stable identity.

2. Character ID is separate from Character Revision ID.

3. Referenced Character Revisions are immutable.

4. Character identity is independent of any single name.

5. Character lifecycle is separate from story state.

6. Character Candidate is not a Character lifecycle state.

7. Review state is separate from Character lifecycle.

8. Authority/Lock is separate from Review state.

9. Confidence and provenance are separate from Review state.

10. Original, romanized, transliterated and localized names remain distinguishable.

11. Names are Language- and Script-aware.

12. Character owns identity-related naming facts.

13. Glossary owns Translation terminology rules.

14. Glossary may reference Character identity but MUST NOT own it.

15. Character MUST NOT duplicate Glossary matching and terminology precedence.

16. Translation name selection MAY depend on Character, Glossary, story scope and Translation Profile together.

17. Future names and identities MUST respect spoiler boundaries.

18. Reader knowledge MUST remain distinguishable from complete story truth where spoilers matter.

19. Character metadata is provider-independent.

20. Recognition execution belongs outside Character Aggregate.

21. Raw embeddings are derived infrastructure data.

22. Character Candidate is not canonical Character truth.

23. AI observations do not become canonical Character truth without explicit promotion.

24. Speaker Attribution is separate from Character identity.

25. Speaker Attribution references exact TextBlock Revision and Character identity where known.

26. Low-confidence speaker attribution MUST preserve uncertainty.

27. Unknown Speaker is distinct from Narrator.

28. Narrator is not universally required to be a Character.

29. A global Character pronoun profile is insufficient for relationship-sensitive Vietnamese address.

30. Relationship-specific Address Rules belong to relationship/context resolution.

31. Character Relationship is independently addressable.

32. Relationships preserve direction where meaning requires it.

33. Historical Relationship revisions remain story-scope aware.

34. Character Appearance is outside Character Aggregate.

35. Persona remains subordinate only when a shared underlying Character identity is known.

36. Character Context Snapshot references exact immutable Character and Relationship revisions.

37. Translation preserves the Character Context Snapshot that influenced it.

38. Later Character edits MUST NOT rewrite historical Translation context.

39. Context construction MUST respect story and spoiler boundaries.

40. Locked Character information MUST NOT be silently overwritten.

41. Translation corrections MUST NOT automatically mutate Character truth.

42. Merge and split preserve identity lineage and evidence.

43. Merge MUST preserve historical reveal boundaries.

44. Referenced Character revisions MUST remain historically resolvable.

45. Derived recognition and search indexes are rebuildable.

46. Cache correctness uses immutable revision/Snapshot identity rather than mutable Character ID alone.

47. Character changes invalidate only affected downstream artifacts.

48. Character data MUST NOT leak implicitly across Projects.

49. Character recognition MUST NOT be repurposed as real-person biometric identification.

50. Authoritative Character changes SHOULD be auditable.

---

# Recommended MVP Scope

The first CRAI MVP SHOULD support:

* Project-scoped Characters,
* stable Character identity,
* immutable Character Revisions,
* original name,
* preferred display name,
* romanized/transliterated names where useful,
* aliases,
* basic Character Types,
* basic titles,
* basic stable traits,
* Unknown values,
* basic Speech Profile,
* Review workflow,
* field/revision authority lock,
* Chapter-range applicability,
* spoiler reveal boundary,
* Character references from Glossary,
* manual Speaker Attribution,
* optional AI speaker candidates,
* Speaker Attribution confidence,
* Unknown Speaker,
* optional Narrator Character,
* basic Character Relationships,
* relationship-specific Vietnamese address notes,
* Character Context Snapshot,
* Translation context integration,
* Character-aware validation,
* Character merge,
* basic Character Candidate workflow,
* JSON import/export,
* audit events,
* selective Translation staleness.

MVP MAY defer:

* face recognition,
* persistent face embeddings,
* automatic visual tracking,
* advanced reincarnation/body-swap modeling,
* complex Persona inheritance,
* semantic Relationship extraction,
* automatic pronoun learning,
* full graph queries,
* age inference,
* voice recognition,
* cross-Project Characters,
* Series-level identity synchronization,
* advanced spoiler permissions,
* automatic bubble-tail attribution,
* automated Character merge,
* advanced group-speaker modeling,
* complex localized naming policy,
* Character analytics,
* external wiki synchronization.

---

# Open Decisions

The following SHOULD remain open until prototype validation:

* whether Character can later be shared at Series scope,
* whether Names remain revision-owned values or independently versioned resources,
* exact CharacterName/GlossaryEntry synchronization policy,
* whether translated Character names are stored in Character, Glossary, or both with one authoritative owner,
* exact title ownership boundary,
* AddressRule persistence ownership,
* Relationship Aggregate implementation,
* spoiler-boundary representation,
* whether future information needs physical data separation,
* Persona modeling policy,
* disguise identity merge policy,
* Narrator modeling policy,
* System Voice modeling policy,
* Group Speaker representation,
* initial Character Types,
* minimal gender/age metadata,
* Character context selection budget,
* speaker-candidate ranking,
* low-confidence Translation policy,
* visual recognition MVP timing,
* Candidate aggregate implementation,
* duplicate-detection policy,
* split impact on historical Translation,
* automatic retranslation policy,
* context relevance scoring,
* learning from corrections,
* import formats,
* spoiler-safe export,
* future Series synchronization.

---

# Ownership Summary

```text
Character owns
    stable identity
    Project ownership
    lifecycle
    active Character Revision
    identity lineage

CharacterRevision owns
    immutable character facts
    identity-related names
    stable traits
    stable speech information
    spoiler metadata

Glossary owns
    terminology matching
    preferred Translation forms
    Translation terminology enforcement

CharacterRelationship owns
    relationship identity
    directional relationship state
    historical relationship revisions
    relationship-specific address context

SpeakerAttribution owns
    TextBlock-to-Character attribution
    confidence
    evidence
    attribution revision

CharacterAppearance owns
    content-location identity evidence

CharacterCandidate owns
    unconfirmed identity hypothesis

CharacterObservation owns
    unconfirmed character-fact hypothesis

CharacterContextSnapshot owns
    immutable resolved Character context
    used by Translation

Recognition infrastructure owns
    embeddings
    model-specific visual descriptors
    recognition indexes
```

Character is therefore the authoritative identity domain for fictional and speaker-like entities, while terminology enforcement, speaker inference, relationship resolution, visual recognition and Translation execution remain independently owned capabilities.

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
* `TRANSLATION.md`
* `LANGUAGE.md`
* `GLOSSARY.md`
* `PROFILE.md`
* `SESSION.md`

Architecture:

* `docs/architecture/CAPABILITY_MAP.md`
* `docs/architecture/OWNERSHIP_MAP.md`
* `docs/architecture/DATA_FLOW.md`
* `docs/architecture/STATE_MACHINE.md`
* `docs/architecture/EVENT_BUS.md`
* `docs/architecture/MODULE_DEPENDENCY.md`

AI:

* `docs/architecture/ai/PIPELINE.md`
* `docs/architecture/ai/CONTEXT.md`
* `docs/architecture/ai/MEMORY.md`
* `docs/architecture/ai/PROMPTS.md`
* `docs/architecture/ai/REQUEST.md`
* `docs/architecture/ai/RESPONSE.md`
* `docs/architecture/ai/ROUTING.md`
* `docs/architecture/ai/CACHE.md`

Presentation:

* `docs/architecture/presentation/LAYOUT.md`
* `docs/architecture/presentation/TYPOGRAPHY.md`

Module contracts remain authoritative for recognition execution, speaker attribution, Translation execution, review workflows and infrastructure behavior.
