# Character Domain

* **Document:** Domain / Character
* **Version:** 1.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

The Character domain defines how CRAI represents fictional characters and other speaker-like entities that appear in comics, manga, manhua, novels and related content.

Character information helps CRAI maintain consistency for:

* Character names
* Aliases
* Titles
* Pronouns
* Gendered language
* Forms of address
* Speech style
* Relationships
* Speaker identification
* Translation context
* Glossary terminology
* Dialogue attribution
* Cross-chapter continuity

The Character domain provides stable identity and approved contextual information.

It must remain independent from:

* OCR providers
* Translation providers
* Computer-vision models
* Prompt formats
* Speaker-detection implementations
* Face-recognition implementations

---

# Domain Role

Character is a Project-level domain entity.

```text
Project
   │
   ├── Book
   │    ├── Chapters
   │    ├── Pages
   │    └── Text Blocks
   │
   └── Characters
        ├── Names
        ├── Aliases
        ├── Traits
        ├── Relationships
        ├── Appearances
        └── Context Snapshots
```

Characters may be referenced by:

* Glossary Entries
* Text Blocks
* Dialogue records
* Translation Context
* Translation Revisions
* Page annotations
* Character relationships
* Review findings

Character does not own those external records.

---

# Ownership Boundary

Character should be modeled as an Aggregate Root.

```text
Character Aggregate
├── Character
├── Character Revision
├── Character Name
├── Alias
├── Title
├── Pronoun Profile
├── Speech Profile
├── Character Trait
├── External Reference
├── Review State
└── Lifecycle State
```

The Character aggregate owns:

* Stable Character identity
* Character metadata
* Names and aliases
* Translation-specific name forms
* Titles
* Pronoun preferences
* Speech characteristics
* Stable character traits
* Review and approval state
* Character revision history
* Merge and split lineage

The Character aggregate does not own:

* Page appearances
* Image regions
* Face embeddings
* Dialogue Text Blocks
* Translation results
* Relationship graph as a whole
* Glossary matching results
* AI-generated observations
* Provider execution data

These are stored as references, projections or separate aggregates.

---

# Responsibilities

The Character domain is responsible for:

* Maintaining stable character identity
* Managing original and translated names
* Managing aliases, titles and nicknames
* Describing pronoun and form-of-address preferences
* Describing stable speech characteristics
* Supporting character disambiguation
* Supporting relationship references
* Maintaining immutable character revisions
* Supporting merge and split operations
* Supporting review and approval
* Producing character context snapshots
* Emitting character-related events
* Preserving historical translation context

The Character domain is not responsible for:

* Detecting characters in images
* Recognizing faces
* Detecting speakers
* Assigning dialogue automatically
* Translating dialogue
* Performing entity extraction
* Building AI prompts
* Rendering character labels
* Generating character summaries from copyrighted content

Those operations belong to application, AI, vision, language-processing and presentation capabilities.

---

# Aggregate Structure

Recommended conceptual structure:

```text
Character
├── Character ID
├── Project ID
├── Character Type
├── Canonical Name Reference
├── Active Revision
├── Lifecycle State
├── Review State
├── Created At
├── Updated At
└── Version
```

Character details belong to immutable revisions.

```text
Character Revision
├── Character Revision ID
├── Character ID
├── Names
├── Aliases
├── Titles
├── Pronoun Profile
├── Speech Profile
├── Traits
├── Notes
├── Scope
├── External References
├── Parent Revision
├── Created By
├── Created At
└── Content Hash
```

---

# Character Identity

Each Character has a stable identifier.

```text
Character ID != Character Revision ID
```

The Character ID represents the fictional entity across:

* Chapters
* Pages
* Images
* Name changes
* Aliases
* Disguises
* Translations
* Character development
* Revision history

Editing a character does not create a new Character identity.

A new Character ID should only be created when the content represents a distinct entity.

---

# Character Revision

Character Revision is an immutable representation of known character information at a point in time.

A new revision may be created when:

* A name is corrected
* An alias is added
* A title changes
* Pronoun preferences are corrected
* A speech profile changes
* A relationship note is updated
* Two identities are confirmed as the same
* An assumed identity is disproved
* Translation-specific naming is approved
* User review changes character metadata

Once referenced by a Translation Context Snapshot, a Character Revision must remain immutable.

---

# Character Type

Recommended Character Types:

* Person
* Creature
* Spirit
* Deity
* Artificial Intelligence
* Sentient Object
* Group Speaker
* Narrator
* System Voice
* Unknown Speaker
* Disguised Identity
* Alternate Persona
* Custom

Character Type supports contextual reasoning.

It must not be used as a substitute for identity.

---

# Canonical Character Name

A Character should have one canonical name reference for display and organization.

The canonical name may be:

* Original-language name
* Most authoritative published name
* User-selected name
* Temporary descriptive identifier

Examples:

```text
林月
Lâm Nguyệt
Unknown Woman in Red
System
Narrator
```

The canonical name is not necessarily the name supplied to Translation.

Translation uses the applicable language-specific Character Name.

---

# Character Name

A Character Name is a language-aware representation of the character’s identity.

Recommended structure:

```text
Character Name
├── Name ID
├── Text
├── Language
├── Script
├── Name Type
├── Usage Scope
├── Preference Rank
├── Approval State
├── Valid From
├── Valid Until
└── Notes
```

A Character may have several names.

Example:

```text
Character: CH-001

Names:
- 林月       — Original, zh-Hans
- Lín Yuè   — Romanization
- Lâm Nguyệt — Preferred Vietnamese
- Moon Lin  — Deprecated English adaptation
```

---

# Name Types

Recommended Name Types:

* Original
* Canonical
* Translated
* Localized
* Transliterated
* Romanized
* Legal Name
* Given Name
* Family Name
* Full Name
* Courtesy Name
* Art Name
* Title Name
* Code Name
* Nickname
* Alias
* Disguise Name
* Temporary Identifier
* Unknown Identifier
* Deprecated Form

Name Type and language must remain separate.

---

# Original Name

Original Name represents the character name as it appears in the source content.

Requirements:

* Preserve original script
* Preserve meaningful punctuation
* Preserve source-language distinctions
* Avoid destructive normalization
* Record the applicable language
* Record evidence when extracted automatically

An Original Name should not be replaced by a translated name.

Both forms should remain available.

---

# Translated Name

A Translated Name is a meaning-based target-language rendering.

Example:

```text
黑龙王 → Hắc Long Vương
```

A translated name may preserve semantic meaning rather than pronunciation.

Translated Name and Transliterated Name must remain distinguishable.

---

# Transliterated Name

A Transliterated Name preserves approximate pronunciation across scripts.

Example:

```text
林月 → Lâm Nguyệt
```

The transliteration standard or convention should be recorded when relevant.

Possible metadata:

* Source language
* Source script
* Target script
* Transliteration convention
* Language adaptation policy
* Approval state

---

# Romanized Name

Romanization converts a name to Latin script using a declared standard.

Examples:

* Pinyin
* Hepburn
* Revised Romanization
* Wade–Giles

```text
林月 → Lín Yuè
```

Romanization should not automatically become the preferred Vietnamese character name.

---

# Localized Name

A Localized Name adapts the name for a target audience.

Localization may involve:

* Pronunciation adaptation
* Cultural naming convention
* Historical Sino-Vietnamese reading
* Official publisher terminology
* Simplified spelling
* Removal of unfamiliar diacritics

Localized names should record their authority and scope.

---

# Name Preference

Name preference may depend on:

* Target language
* Project
* Book
* Chapter range
* Translation Profile
* Reader preference
* Publication convention
* Dialogue versus narration
* Speaker relationship
* Historical point in the story

Recommended resolution:

```text
Explicit Translation Override
        ↓
Approved Scope-Specific Name
        ↓
Approved Target-Language Name
        ↓
Project Preferred Name
        ↓
Transliterated Name
        ↓
Original Name
        ↓
Temporary Identifier
```

The resolution result should record why a name was selected.

---

# Name Scope

A name may be valid only in a specific scope.

Examples:

* Childhood name used in early chapters
* Married name used later
* Disguise name used during one story arc
* Title used only after promotion
* Secret identity revealed after a chapter
* Nickname used by one relationship group

Recommended scope properties:

```text
Name Scope
├── Book IDs
├── Chapter Range
├── Page Range
├── Story Arc
├── Speaker Character IDs
├── Audience
├── Translation Profile IDs
└── Exclusions
```

---

# Temporal Name Validity

Character names and titles may change over the story timeline.

Recommended fields:

* Valid From Chapter
* Valid Until Chapter
* Introduced At
* Revealed At
* Deprecated At
* Spoiler Level

Translation must avoid using names that have not yet been revealed within the applicable story context.

---

# Spoiler-Sensitive Identity

Character metadata may contain spoilers.

Examples:

* True name
* Secret identity
* Family relationship
* Hidden faction
* Future title
* Character death
* Betrayal
* Alternate form

Spoiler-sensitive data should carry visibility constraints.

Recommended metadata:

```text
Spoiler Scope
├── Reveal Chapter
├── Reveal Page
├── Minimum Reader Progress
├── Visibility Policy
└── Allowed Processing Context
```

Context construction must not include future information when translating earlier chapters unless explicitly configured.

---

# Alias

An Alias is an alternate form used to refer to the same Character.

Examples:

* Nickname
* Abbreviation
* Code name
* Disguise identity
* Misspelling
* OCR variation
* Honorific form
* Relationship-based address

Recommended structure:

```text
Character Alias
├── Alias ID
├── Text
├── Language
├── Script
├── Alias Type
├── Scope
├── Match Policy
├── Approval State
└── Notes
```

Aliases should reference one Character identity.

---

# Alias Types

Recommended Alias Types:

* Nickname
* Code Name
* Disguise
* Title
* Relationship Address
* Abbreviation
* Romanization Variant
* Translation Variant
* OCR Variant
* Common Misspelling
* Informal Form
* Formal Form
* Self-Reference
* Temporary Alias
* Deprecated Alias

OCR variants must not be treated as approved display names.

---

# Title

A Title represents a role or status associated with a Character.

Examples:

* Sect Master
* Emperor
* Captain
* Young Master
* Saintess
* General
* Doctor
* Professor

Recommended structure:

```text
Character Title
├── Title ID
├── Source Form
├── Target Forms
├── Language Scope
├── Title Type
├── Validity Scope
├── Priority
├── Approval State
└── Glossary Entry Reference
```

Titles may be owned as Character metadata while their terminology translations are managed through Glossary.

---

# Character and Glossary Boundary

Character owns identity and character-specific naming information.

Glossary owns translation terminology rules.

Recommended relationship:

```text
Character
├── Character ID
├── Original Name
├── Preferred Display Name
└── Glossary Entry References
```

```text
Glossary Entry
├── Source Forms
├── Target Forms
├── Matching Rules
├── Translation Rule
└── Character Reference
```

A Character should not duplicate the complete matching and precedence logic of Glossary.

A Glossary Entry should not become the canonical owner of Character identity.

---

# Pronoun Profile

Pronoun Profile describes how the character may be referenced in target-language translation.

Recommended structure:

```text
Pronoun Profile
├── Grammatical Gender
├── Self-Reference Forms
├── Third-Person Forms
├── Address Preferences
├── Formality
├── Language Scope
├── Relationship Overrides
├── Scope
└── Confidence
```

Pronoun Profile should support languages where pronoun use depends heavily on social context.

---

# Grammatical Gender

Possible values may include:

* Masculine
* Feminine
* Neutral
* Nonbinary
* Variable
* Unknown
* Not Applicable

Grammatical Gender is translation metadata.

It must not be treated as a complete representation of identity.

Unknown information must remain unknown rather than being guessed.

---

# Vietnamese Address Context

Vietnamese pronouns and forms of address depend on:

* Age
* Relative age
* Social rank
* Family relationship
* Intimacy
* Formality
* Speaker
* Listener
* Narrative context
* Historical setting
* Genre

A single global pronoun per Character is insufficient.

Recommended model:

```text
Address Rule
├── Speaker Character
├── Listener Character
├── Speaker Self-Reference
├── Listener Address Form
├── Third-Person Reference
├── Formality
├── Scope
└── Approval State
```

Examples:

```text
Character A → Character B:
Self: ta
Address: ngươi

Character B → Character A:
Self: đệ tử
Address: sư phụ
```

---

# Self-Reference

Some characters use distinctive first-person forms.

Examples in Vietnamese translation:

* tôi
* ta
* bản tọa
* trẫm
* bổn cung
* tại hạ
* lão phu
* tiểu nữ
* đệ tử
* thuộc hạ

Self-reference may depend on:

* Target language
* Speaker
* Listener
* Story period
* Formality
* Character role
* Translation style

Self-reference must not be determined only from gender.

---

# Address Form

Address Form defines how one Character refers to another.

Examples:

* ngươi
* anh
* chị
* em
* sư phụ
* sư huynh
* điện hạ
* bệ hạ
* đại nhân
* tiền bối

Address Form belongs to relationship-aware context.

It may reference Glossary Entries for terminology consistency.

---

# Speech Profile

Speech Profile captures stable characteristics that influence translation style.

Recommended structure:

```text
Speech Profile
├── Formality
├── Register
├── Tone
├── Sentence Style
├── Vocabulary Preferences
├── Verbal Habits
├── Self-Reference
├── Honorific Usage
├── Dialect Notes
├── Restrictions
├── Scope
└── Confidence
```

Possible speech characteristics:

* Formal
* Casual
* Archaic
* Childlike
* Polite
* Aggressive
* Technical
* Poetic
* Concise
* Verbose
* Comedic
* Emotionless

Speech Profile should be descriptive rather than provider-specific.

---

# Speech Habit

A Speech Habit may describe repeated behavior such as:

* Catchphrase
* Sentence ending
* Repeated interjection
* Honorific preference
* Stutter
* Archaic vocabulary
* Third-person self-reference
* Deliberate lack of contractions
* Specific punctuation style

Speech habits should not force mechanical replacements that damage natural target-language grammar.

---

# Character Trait

Character Traits represent relatively stable contextual facts.

Examples:

* Approximate age group
* Social rank
* Occupation
* Species
* Affiliation
* Personality descriptor
* Combat role
* Narrative role
* Knowledge level

Recommended structure:

```text
Character Trait
├── Trait Type
├── Value
├── Confidence
├── Source
├── Validity Scope
├── Review State
└── Spoiler Scope
```

Traits should be included only when useful to translation or understanding.

---

# Stable and Dynamic Traits

Traits should distinguish between:

## Stable Traits

Usually persistent across the work:

* Species
* Basic identity
* General speech style
* Original family
* Primary name

## Dynamic Traits

May change by chapter or arc:

* Rank
* Faction
* Age
* Relationship
* Loyalty
* Injury
* Disguise
* Emotional state
* Current title

Dynamic facts should use scoped observations rather than overwrite permanent character identity.

---

# Observation

AI or users may create observations about a Character.

Examples:

* Appears to be angry
* Uses formal speech
* May be the speaker
* Wearing red clothing
* Referred to as “Master”

An Observation is not automatically canonical Character truth.

Recommended structure:

```text
Character Observation
├── Observation ID
├── Character Candidate
├── Observation Type
├── Value
├── Evidence References
├── Confidence
├── Scope
├── Observer
├── Observer Revision
└── Review State
```

Observations belong to a separate extraction or analysis workflow.

Approved observations may later create Character revisions.

---

# Appearance

A Character Appearance links a Character to a content location.

Recommended structure:

```text
Character Appearance
├── Appearance ID
├── Character ID
├── Character Revision
├── Book ID
├── Chapter ID
├── Page ID
├── Image Version
├── Region
├── Appearance Type
├── Confidence
├── Identification Source
└── Review State
```

Appearance is derived or contextual data.

It should not be embedded directly inside the Character aggregate.

---

# Appearance Types

Possible Appearance Types:

* Visible
* Partial
* Silhouette
* Portrait
* Background
* Flashback
* Illustration
* Mentioned
* Speaking Off-Panel
* Narrating
* System Voice
* Unknown

A Character may be present without being visually shown.

---

# Visual Identity

Visual identity may be represented through:

* Face references
* Clothing descriptors
* Hair descriptors
* Color descriptors
* Body features
* Accessories
* Character sheets
* Reference images
* Embedding references

The Character domain may store approved descriptive metadata or references.

Raw embeddings and provider model outputs belong to infrastructure or recognition services.

---

# Face Embeddings

Face embeddings must not be canonical Character data.

Recommended relationship:

```text
Character
    │
    └── Recognition Profile Reference
            └── Face Embeddings
```

Embeddings are:

* Model-specific
* Version-specific
* Rebuildable
* Potentially sensitive
* Not human-readable

They must include model and preprocessing revisions.

---

# Speaker Attribution

Speaker Attribution associates dialogue with a Character.

```text
Speaker Attribution
├── Text Block ID
├── Text Block Revision
├── Character ID
├── Character Revision
├── Attribution Method
├── Confidence
├── Evidence
└── Review State
```

Attribution methods may include:

* User Assigned
* Bubble Tail Analysis
* Proximity
* Face Association
* Dialogue Pattern
* Name Mention
* Novel Speaker Tag
* AI Inference
* Imported Metadata

Speaker Attribution belongs outside the Character aggregate.

---

# Speaker Confidence

Speaker inference may be uncertain.

Recommended normalized confidence:

```text
0.0 to 1.0
```

Possible interpretation:

| Confidence  | Meaning         |
| ----------- | --------------- |
| `0.90–1.00` | Strong evidence |
| `0.70–0.89` | Likely          |
| `0.40–0.69` | Ambiguous       |
| `0.00–0.39` | Weak            |

Thresholds must remain configurable.

A low-confidence speaker attribution must not silently impose character-specific pronouns.

---

# Unknown Speaker

Unknown Speaker should be represented explicitly.

Possible approaches:

* No Character reference
* Project-level Unknown Speaker entity
* Page-scoped temporary candidate

Recommended behavior:

* Preserve uncertainty
* Avoid unsupported gender assumptions
* Avoid relationship-specific address rules
* Allow later reconciliation
* Keep original attribution evidence

Unknown Speaker must not be automatically merged with Narrator.

---

# Narrator

Narrator may be modeled as a Character when it has:

* Stable identity
* Stable speech style
* Repeated voice
* Translation-specific pronoun behavior

For neutral narration without identity, a specialized Narration Context may be sufficient.

Possible narrator types:

* Omniscient Narrator
* First-Person Narrator
* Character Narrator
* Unreliable Narrator
* System Narrator
* Unknown Narrator

---

# System Voice

Comics and novels may include non-character speakers:

* Game system
* Quest notification
* Interface message
* Divine announcement
* AI assistant
* Automated broadcast

These may be modeled as Characters of type `System Voice` when identity and style consistency matter.

Presentation-specific UI labels remain separate.

---

# Character Relationship

A Character Relationship connects two or more Character identities.

Recommended relationship structure:

```text
Character Relationship
├── Relationship ID
├── Source Character ID
├── Target Character ID
├── Relationship Type
├── Direction
├── Address Rules
├── Validity Scope
├── Confidence
├── Review State
└── Revision
```

Relationship should normally be a separate aggregate or graph projection.

Character aggregates may store relationship references but should not own the complete graph.

---

# Relationship Types

Possible relationship types:

* Family
* Parent
* Child
* Sibling
* Spouse
* Romantic
* Friend
* Rival
* Enemy
* Master
* Disciple
* Superior
* Subordinate
* Employer
* Employee
* Leader
* Follower
* Teammate
* Faction Member
* Acquaintance
* Unknown
* Custom

Relationship type should be directional where meaning requires it.

```text
Master → Disciple
```

is not equivalent to:

```text
Disciple → Master
```

---

# Relationship Revision

Relationships may change over time.

Examples:

* Stranger becomes friend
* Disciple becomes rival
* Hidden family relationship is revealed
* Superior becomes subordinate
* Enemy alliance becomes temporary cooperation

Relationship revisions must support:

* Chapter scope
* Story arc
* Reveal point
* Confidence
* Historical context

Translation of earlier chapters must use the applicable historical relationship state.

---

# Character Group

A group may function as one speaker or entity.

Examples:

* Crowd
* Soldiers
* Council
* Villagers
* Audience
* Chorus
* System administrators

A Character Group may be represented as:

* Character Type `Group Speaker`
* Separate Group aggregate referenced by dialogue
* Temporary context entity

The MVP may use Group Speaker characters for simplicity.

---

# Alternate Persona

One Character may have several personas.

Examples:

* Secret identity
* Possessed state
* Transformation
* Disguise
* Split personality
* Reincarnated identity
* Body swap

Possible modeling approaches:

1. One Character with scoped personas
2. Separate Character identities linked through a relationship
3. One underlying identity plus presentation profiles

The choice depends on whether the personas behave as independently referenced entities in the source.

---

# Persona

Recommended Persona structure:

```text
Character Persona
├── Persona ID
├── Character ID
├── Name Forms
├── Appearance Profile
├── Speech Profile
├── Validity Scope
├── Reveal Scope
└── Review State
```

Persona should remain subordinate to Character only when it shares one stable underlying identity.

---

# Reincarnation and Body Swap

Complex identity cases must distinguish:

* Soul identity
* Body identity
* Public identity
* Speaker identity
* Name currently used
* Reader knowledge

The Character domain should avoid forcing one universal identity model.

Domain references may include an Identity Aspect:

* Underlying Character
* Visible Body
* Public Persona
* Speaker Persona

Advanced identity modeling may be deferred beyond MVP.

---

# Character Candidate

Automatic extraction may create Character Candidates.

```text
Character Candidate
├── Candidate ID
├── Proposed Names
├── Appearance References
├── Dialogue References
├── Trait Suggestions
├── Similar Character References
├── Confidence
├── Detection Source
├── Detection Revision
└── Review State
```

Candidates are not canonical Characters.

They require:

* User confirmation
* Policy-based promotion
* Merge into an existing Character
* Rejection

---

# Candidate Promotion

Promoting a candidate may:

* Create a new Character
* Add an alias to an existing Character
* Add appearance evidence
* Add a speaker attribution
* Create a possible duplicate warning

Promotion should preserve all source evidence.

---

# Duplicate Detection

Potential duplicate Characters may be detected through:

* Equivalent names
* Shared aliases
* Visual similarity
* Overlapping appearances
* Dialogue continuity
* Entity references
* Relationship patterns
* User review

Duplicate detection should create a review candidate.

It must not automatically merge authoritative Characters.

---

# Character Merge

Character Merge consolidates two identities confirmed to represent the same entity.

Merge operation should:

* Select a surviving Character ID
* Preserve all Character Revisions
* Preserve names and aliases
* Preserve appearance references
* Preserve speaker attributions
* Preserve relationships
* Preserve Glossary references
* Record redirect lineage
* Detect conflicting metadata
* Emit merge events

Historical references to the merged Character must remain resolvable.

---

# Merge Conflict

Possible merge conflicts include:

* Different original names
* Incompatible pronoun profiles
* Overlapping appearances suggesting separate entities
* Conflicting story scopes
* Different external references
* Different locked translations
* Contradictory relationships

Conflicts require review before final merge.

---

# Character Split

Character Split is needed when one Character identity incorrectly combines several entities.

Examples:

* Twins identified as one person
* Narrator and protagonist merged
* Disguise identity incorrectly merged
* Same surname treated as one Character
* Two speakers assigned to one candidate

Split operation should:

* Create new Character IDs
* Preserve original lineage
* Reassign selected names
* Reassign appearances
* Reassign speaker attributions
* Reassign relationships
* Reconcile Glossary references
* Mark affected Translations for review

---

# Character Lifecycle

Recommended lifecycle states:

* Candidate
* Active
* Inactive
* Missing
* Deceased
* Historical
* Merged
* Split
* Archived
* Rejected

Lifecycle state should not reveal spoilers unless visibility policy permits it.

For example, `Deceased` may be stored with a reveal scope.

---

# Candidate State

Candidate Characters are unconfirmed.

They may be used for:

* Review interfaces
* Temporary attribution
* Duplicate comparison
* Evidence collection

They should not influence authoritative Translation without policy approval.

---

# Active State

Active means the Character may participate in:

* Translation context
* Glossary resolution
* Speaker attribution
* Relationship rules
* Character search

Active does not mean the Character is alive in the story.

---

# Inactive State

Inactive Characters remain stored but are excluded from ordinary current processing.

Possible reasons:

* Imported but unused
* Duplicate under review
* No longer relevant
* User-hidden
* Temporarily disabled

---

# Merged State

Merged Characters redirect to the surviving Character.

The old identity remains available for:

* Historical references
* Audit
* Import reconciliation
* External links

---

# Review State

Recommended Character review states:

* Unreviewed
* AI Suggested
* User Confirmed
* Under Review
* Approved
* Needs Changes
* Rejected
* Locked

Lifecycle and Review State remain separate.

An Active Character may still be unreviewed if provisional processing is allowed.

---

# Approval

Approval applies to an exact Character Revision.

Approval may cover:

* Identity
* Names
* Pronouns
* Speech Profile
* Traits
* Relationship references
* Spoiler metadata

Editing approved semantic data creates a new revision that requires review.

Non-semantic metadata changes may inherit approval according to policy.

---

# Locked Character

A locked Character or Character field represents authoritative information.

Examples:

* Official character name
* Publisher-approved translation
* Main-character identity
* User-pinned pronoun profile
* Confirmed speaker style

Locking may be field-specific.

```text
Character Lock
├── Locked Fields
├── Scope
├── Authority
├── Actor
├── Created At
└── Reason
```

Imported or AI-generated data must not overwrite locked fields.

---

# Character Context Snapshot

Translation should consume immutable Character Context Snapshots.

Recommended structure:

```text
Character Context Snapshot
├── Snapshot ID
├── Project ID
├── Story Scope
├── Character Revision References
├── Relationship Revision References
├── Relevant Name Forms
├── Pronoun Rules
├── Speech Profiles
├── Spoiler Boundary
├── Created At
└── Content Hash
```

The snapshot should include only characters relevant to the translation operation.

---

# Snapshot Selection

Relevant characters may be selected through:

* Speaker attribution
* Characters visible on Page
* Characters mentioned in source text
* Characters active in nearby Pages
* Chapter cast
* Relationship context
* Dialogue history
* User pinning
* Semantic retrieval

Selection must respect:

* Context budget
* Spoiler boundaries
* Project visibility
* Review policy
* Confidence thresholds

---

# Snapshot Immutability

Once referenced by a Translation Revision, a Character Context Snapshot must remain immutable.

Later Character changes produce a new snapshot.

Historical Translation Revisions retain references to the old snapshot.

---

# Translation Integration

Character context may influence:

* Name selection
* Pronouns
* Forms of address
* Formality
* Speech style
* Gendered terms
* Relationship terminology
* Dialogue consistency
* Narrator voice

Conceptual request:

```text
Translation Request
├── Source Text
├── Language Pair
├── Translation Profile
├── Glossary Snapshot
├── Character Context Snapshot
└── Story Context Snapshot
```

Character domain data should be compiled into provider-neutral context before entering provider adapters.

---

# Prompt Integration

Character records must not store canonical provider-specific prompts.

Recommended flow:

```text
Character Context Snapshot
        │
        ▼
Context Compiler
        │
        ▼
Provider-Neutral Character Instructions
        │
        ▼
Provider Adapter
        │
        ▼
Provider-Specific Request
```

Provider-specific prompt formatting belongs to AI infrastructure.

---

# Context Budget

A large cast may exceed provider limits.

Context selection may prioritize:

1. Confirmed speaker
2. Confirmed listener
3. Characters mentioned in current source
4. Visible Page characters
5. Recently active characters
6. Relationship-linked characters
7. Chapter-level main cast
8. Other Project characters

The system should record which Character Revisions were actually included.

---

# Applied Character Context

Translation execution may record:

```text
Applied Character Context
├── Character ID
├── Character Revision ID
├── Context Role
├── Speaker Confidence
├── Selected Name Form
├── Applied Pronoun Rule
├── Applied Speech Profile
└── Application Result
```

Context roles may include:

* Speaker
* Listener
* Mentioned
* Visible
* Narrator
* Relationship Context
* Background Context
* User Pinned

---

# Translation Validation

Character-aware validation may detect:

* Wrong character name
* Deprecated name used
* Name used before reveal
* Incorrect pronoun
* Incorrect self-reference
* Relationship address mismatch
* Speech style inconsistency
* Wrong character attribution
* Inconsistent title
* Character identity conflict
* Spoiler leakage

Validation findings must reference exact Character Revisions and Translation Revisions.

---

# Character Validation Finding

Recommended structure:

```text
Character Validation Finding
├── Finding ID
├── Translation Revision ID
├── Character ID
├── Character Revision ID
├── Finding Type
├── Severity
├── Source Range
├── Target Range
├── Expected Context
├── Observed Text
├── Confidence
├── Validator Revision
└── Resolution State
```

---

# Validation Finding Types

Recommended finding types:

* Character Name Mismatch
* Unapproved Alias Used
* Deprecated Name Used
* Premature Identity Reveal
* Pronoun Mismatch
* Self-Reference Mismatch
* Address Form Mismatch
* Title Mismatch
* Speaker Attribution Conflict
* Speech Profile Inconsistency
* Character Context Missing
* Relationship Context Missing
* Unknown Character Reference
* Locked Character Rule Violation

---

# User Correction

A user may correct:

* Character name
* Speaker identity
* Pronoun
* Self-reference
* Address form
* Title
* Relationship
* Alias
* Character merge or split

A correction may produce:

1. A new Translation Revision
2. A Character update suggestion
3. A Speaker Attribution revision
4. A Glossary Candidate
5. A Relationship revision candidate

Translation correction must not automatically mutate canonical Character data unless policy explicitly allows it.

---

# Learning from Corrections

Example:

```text
Source:
李青说道：“我会回来。”

Generated:
Lý Thanh nói: “Tôi sẽ quay lại.”

User Correction:
Lý Thanh nói: “Ta sẽ trở lại.”
```

Possible inferred suggestion:

```text
Character: Lý Thanh
Self-reference: ta
Scope: cultivation dialogue
```

This remains a candidate until approved.

---

# Character Change Impact

Character changes may affect Translation artifacts.

Possible impact classifications:

* No Impact
* Metadata Only
* Review Recommended
* Validation Required
* Retranslation Recommended
* Retranslation Required

Examples:

| Character Change                  | Typical Impact                    |
| --------------------------------- | --------------------------------- |
| Description updated               | No Impact                         |
| Search tag added                  | No Impact                         |
| Alias added                       | Validation may be useful          |
| Preferred Vietnamese name changed | Retranslation recommended         |
| Locked name corrected             | Retranslation required            |
| Pronoun Profile changed           | Review or retranslation           |
| Relationship rule changed         | Affected dialogues require review |
| Spoiler boundary corrected        | Validation required               |
| Speaker attribution changed       | Retranslation may be required     |
| Character merge                   | Reconciliation required           |

Only affected Translations should become stale.

---

# Affected Translation Detection

Potential evidence:

* Character Context Snapshot references
* Applied Character Context references
* Speaker Attribution references
* Glossary Entry references
* Character Validation findings
* Translation source mentions
* Story scope
* Relationship references

Changing one minor Character should not invalidate every Project Translation.

---

# Character Context Hash

Character context participates in Translation reproducibility.

Possible configuration inputs:

```text
Source Hash
+
Language Pair
+
Translation Profile Revision
+
Glossary Snapshot Hash
+
Character Context Snapshot Hash
+
Story Context Snapshot Hash
+
Prompt Revision
```

Changes to relevant Character context create a new configuration identity.

---

# Import

Characters may be imported from:

* JSON
* YAML
* CSV
* Spreadsheet
* EPUB metadata
* Existing wiki
* Publisher character sheet
* Another CRAI Project
* AI-generated candidates
* User-maintained notes

Import should create a reviewable plan.

It must not silently overwrite approved Characters.

---

# Character Import Plan

Recommended structure:

```text
Character Import Plan
├── Import ID
├── Source Hash
├── Proposed New Characters
├── Proposed Updates
├── Possible Duplicates
├── Name Conflicts
├── Relationship Conflicts
├── Invalid Records
├── Language Mapping
└── Review State
```

---

# Import Conflict Resolution

Possible actions:

* Create Character
* Create Character Revision
* Add Name
* Add Alias
* Add External Reference
* Add Candidate Trait
* Merge with Existing
* Ignore
* Keep Existing
* Require Manual Review

Locked fields cannot be replaced by ordinary import.

---

# Export

Character export may support:

* Full Project cast
* Selected Book
* Selected Chapter range
* Approved Characters only
* Public spoiler-safe view
* Full internal view
* Character context snapshot
* Human review format
* CRAI-native round-trip format

---

# Spoiler-Safe Export

Spoiler-safe export should exclude information not visible at the selected progress point.

This may include:

* True identities
* Future aliases
* Future relationships
* Death state
* Future factions
* Future titles
* Hidden powers
* Revealed family connections

The export must declare its story boundary.

---

# Round-Trip Export

A CRAI-native export should preserve:

* Character IDs
* Character Revision IDs
* Name IDs
* Language and script
* Scope
* Review states
* Locks
* Spoiler metadata
* Merge and split lineage
* External references
* Content hashes

Simple CSV export may lose advanced metadata.

---

# External References

Character may reference:

* Publisher identifier
* Wiki page identifier
* Source platform identifier
* Imported dataset identifier
* User note identifier
* Image reference identifier
* Knowledge graph identifier

External references must not become canonical Character identity.

They may change or disappear.

---

# Persistence

Recommended canonical persistence separation:

```text
Character
Character Revision
Character Name
Character Alias
Character Title
Character Lock
Character Review
Character Merge
Character Split
Character Context Snapshot
Character Context Snapshot Item
```

Separate contextual or derived persistence:

```text
Character Candidate
Character Observation
Character Appearance
Speaker Attribution
Character Relationship
Recognition Profile
Character Search Index
```

Derived data must be rebuildable where possible.

---

# Search Index

Character search may index:

* Original names
* Translated names
* Aliases
* Titles
* Romanizations
* Traits
* Affiliations
* External references
* Notes

Search index content is derived.

Search failure must not affect Character truth.

---

# Recognition Index

Recognition infrastructure may index:

* Face embeddings
* Visual descriptors
* Clothing descriptors
* Character reference images
* Voice or dialogue style vectors

Recognition indexes must reference:

* Character ID
* Character Revision
* Model revision
* Preprocessing revision
* Source evidence

They must remain outside canonical Character persistence.

---

# Cache Participation

Character-related cache keys may include:

* Character Context Snapshot Hash
* Character Revision IDs
* Relationship Revision IDs
* Speaker Attribution Revision
* Story Scope
* Spoiler Boundary
* Translation Profile Revision
* Context Compiler Revision

Mutable Character IDs alone are insufficient for cache correctness.

---

# Concurrency

Concurrent Character editing should use optimistic concurrency.

Possible checks:

* Character aggregate version
* Active Character Revision
* Expected parent revision
* Content hash
* Lock state

Concurrent edits may result in:

* Automatic merge for independent metadata
* Parallel draft revisions
* Manual conflict resolution
* Stale-write rejection

Approved data must not be silently overwritten.

---

# Idempotency

Idempotency may apply to:

* Importing the same Character dataset
* Creating candidates from the same evidence
* Confirming an existing name
* Adding an existing alias
* Approving the same revision
* Merging the same Characters
* Publishing the same context snapshot

Possible idempotency inputs:

* Operation key
* Source hash
* Character content hash
* Evidence hash
* Parent revision

---

# Deletion

Hard deletion should be exceptional.

A Character referenced by:

* Translation Revisions
* Character Context Snapshots
* Glossary Entries
* Speaker Attributions
* Appearances
* Relationships
* Audit records

must not be physically deleted under normal operation.

Preferred operations:

* Reject candidate
* Inactivate
* Archive
* Merge
* Redirect
* Mark as mistaken identity

Hard deletion may apply only to unreferenced drafts or legal requirements.

---

# Retention

The system should retain:

* Referenced Character Revisions
* Character Context Snapshots
* Approved names
* Merge and split lineage
* Speaker attribution history
* Review records
* Import provenance
* Audit events

Temporary unreviewed candidates and derived recognition vectors may have shorter retention policies.

---

# Security

Character permissions may include:

* View
* View Spoilers
* Suggest
* Create
* Edit Draft
* Approve
* Lock
* Merge
* Split
* Import
* Export
* Manage Recognition Data

Spoiler access should be independent from ordinary view permission.

---

# Privacy

Character data may include sensitive Project information:

* Unreleased story details
* Licensed names
* Publisher terminology
* Private reading notes
* User corrections
* Uploaded reference images
* Embeddings derived from images

Requirements:

* Prevent cross-Project leakage
* Respect spoiler visibility
* Minimize context sent to providers
* Exclude irrelevant characters
* Respect local-only mode
* Protect reference images and embeddings
* Record Character exports
* Avoid using Character data to infer real-person identity

CRAI Character recognition is intended for fictional content organization, not real-world biometric identification.

---

# Audit

Important actions should be auditable:

* Character created
* Revision created
* Name changed
* Pronoun changed
* Character approved
* Character locked
* Characters merged
* Character split
* Identity confirmed
* Speaker attribution corrected
* Spoiler boundary changed
* Character exported

Audit records should include:

* Actor
* Time
* Previous revision
* New revision
* Reason
* Scope
* Correlation ID

---

# Events

Typical domain events include:

* `CharacterCreated`
* `CharacterRevisionCreated`
* `CharacterActivated`
* `CharacterInactivated`
* `CharacterApproved`
* `CharacterRejected`
* `CharacterLocked`
* `CharacterUnlocked`
* `CharacterNameAdded`
* `CharacterNameChanged`
* `CharacterAliasAdded`
* `CharacterTitleChanged`
* `CharacterPronounProfileChanged`
* `CharacterSpeechProfileChanged`
* `CharacterCandidateDetected`
* `CharacterCandidatePromoted`
* `CharacterDuplicateDetected`
* `CharactersMerged`
* `CharacterSplit`
* `CharacterRelationshipChanged`
* `CharacterAppearanceDetected`
* `SpeakerAttributed`
* `SpeakerAttributionCorrected`
* `CharacterContextSnapshotCreated`

Events should contain identifiers and revision references rather than full character biographies.

---

# Event Payload Example

```text
CharacterRevisionCreated
├── Project ID
├── Character ID
├── Character Revision ID
├── Parent Revision ID
├── Change Types
├── Story Scope
├── Actor
├── Occurred At
└── Correlation ID
```

Spoiler-sensitive details should not be included in broadly distributed event payloads.

---

# Comic Processing Example

```text
Page contains:
- Character A
- Character B
- Two speech bubbles

Visual Analysis:
- Character A visible near Bubble 1
- Character B visible near Bubble 2

Speaker Attribution:
- Bubble 1 → Character A, confidence 0.94
- Bubble 2 → Character B, confidence 0.82

Character Context:
- A refers to self as “ta”
- A addresses B as “ngươi”
- B refers to A as “sư huynh”

Translation:
- Bubble 1 applies Character A Speech Profile
- Bubble 2 applies Character B relationship rules
```

Attribution confidence and character context remain separately recorded.

---

# Novel Processing Example

```text
Source Paragraph:
Lâm Nguyệt nói: “Sư phụ, con đã trở về.”

Structured Extraction:
- Speaker tag: Lâm Nguyệt
- Listener mention: Sư phụ

Character Resolution:
- Speaker → Character CH-001
- Listener → Character CH-004

Relationship:
- CH-001 is disciple of CH-004

Translation Context:
- Self-reference: đệ tử or con, depending on profile
- Address form: sư phụ
```

Speaker tags from source structure should outrank uncertain AI inference.

---

# Name Resolution Example

Character:

```text
Original:
林月

Romanized:
Lín Yuè

Preferred Vietnamese:
Lâm Nguyệt

Disguise Name:
Bạch Linh

Disguise Scope:
Chapters 40–52
```

When translating Chapter 45, `Bạch Linh` may be selected if the source uses the disguise identity.

When translating Chapter 20, the future disguise name must not enter context.

---

# Pronoun Example

```text
Character A:
- Older sect leader
- Formal, authoritative speech

Character B:
- Junior disciple

A → B:
- Self-reference: ta
- Address: ngươi

B → A:
- Self-reference: đệ tử
- Address: sư phụ
```

These rules are relationship-specific and cannot be derived from a global gender field alone.

---

# Unknown Speaker Example

```text
Text Block:
“Không được tiến thêm bước nào!”

Speaker Candidates:
- Guard A: 0.42
- Guard B: 0.38
- Unknown: 0.20
```

Because confidence is low, Translation should avoid applying a highly specific Character Speech Profile automatically.

The result may be marked for later speaker review.

---

# User Correction Example

```text
Detected Speaker:
Character A

Generated Translation:
“Tôi không đồng ý.”

User Correction:
Speaker is Character B
Translation:
“Bổn cung không đồng ý.”
```

Possible consequences:

1. Create a new Speaker Attribution revision
2. Create a new Translation Revision
3. Revalidate Character B Speech Profile
4. Preserve the original generated result
5. Create a Character context learning candidate

---

# Character Merge Example

```text
Character CH-012:
Masked Swordsman

Character CH-031:
General Lý

Chapter 70 reveals:
They are the same person.
```

Merge policy may:

* Preserve both historical identities
* Keep separate scoped personas
* Select one surviving Character ID
* Record the reveal boundary
* Prevent future context from leaking into earlier chapters

A simple destructive merge would be insufficient.

---

# Architecture Invariants

1. Character is an Aggregate Root with stable identity.
2. Character ID is separate from Character Revision ID.
3. Referenced Character Revisions are immutable.
4. Character identity is independent of any one name.
5. Original, translated, localized, transliterated and romanized names remain distinguishable.
6. Names are language- and script-aware.
7. Name preference may be scope-specific.
8. Future names and identities must respect spoiler boundaries.
9. Character metadata does not depend on AI provider formats.
10. Character recognition execution belongs outside the Character aggregate.
11. Face embeddings are derived infrastructure data.
12. Character Candidates are not canonical Characters.
13. AI observations do not become domain truth without promotion or approval.
14. Speaker Attribution is separate from Character identity.
15. Low-confidence speaker attribution must preserve uncertainty.
16. Unknown Speaker is distinct from Narrator.
17. Pronoun rules may depend on speaker-listener relationships.
18. A global gender field is insufficient for Vietnamese address resolution.
19. Character and Glossary have separate ownership responsibilities.
20. Glossary may reference Character identity but does not own it.
21. Character may reference Glossary Entries but does not own terminology rules.
22. Relationships are directional where domain meaning requires it.
23. Relationship history is scope- and revision-aware.
24. Character Context Snapshots reference exact Character Revisions.
25. Translation Revisions preserve their original Character Context Snapshot.
26. Later Character edits do not rewrite historical Translation context.
27. Context construction must respect story and spoiler scope.
28. Locked Character data cannot be overwritten by ordinary imports or AI suggestions.
29. User Translation corrections do not automatically mutate Character truth.
30. Character merge preserves historical references and lineage.
31. Character split preserves original lineage and evidence.
32. Referenced Characters cannot normally be hard deleted.
33. Derived search and recognition indexes are rebuildable.
34. Cache keys use revisioned Character context rather than mutable IDs alone.
35. Character changes invalidate only affected downstream artifacts.
36. Character data must not leak across Projects.
37. Character recognition must not be repurposed as real-person biometric identification.
38. Every authoritative Character change is auditable.

---

# Open Decisions

The following decisions should remain open until implementation and prototype testing:

* Whether Character is Project-level or Book-level by default
* Whether one Character may belong to several Books in a series
* Whether Character Revision contains all fields or field-level revisions
* Whether names are separate entities or revision-owned value objects
* Whether aliases belong primarily to Character or Glossary
* Whether titles are Character metadata, Glossary Entries or both
* How Vietnamese pronoun rules are represented
* Whether relationship-specific address rules are stored in Character or Relationship
* Whether relationships form a separate Aggregate Root
* How spoiler boundaries are represented
* Whether future information is physically separated from current context
* How alternate personas are modeled
* How reincarnation and body-swap identities are modeled
* Whether masked or disguised identities use separate Character IDs
* Whether Narrator is always a Character
* Whether System Voice is always a Character
* How group speakers are represented
* Which Character Types are supported in MVP
* Whether age and gender metadata are needed
* How uncertain gender information is represented
* Whether character description text is included in AI context
* How much Character context is sent to providers
* Whether context uses structured JSON or compiled natural language
* How speaker candidates are ranked
* Whether low-confidence speaker attribution blocks Translation
* Whether comic Bubble Tail detection is part of MVP
* Whether visual Character recognition runs locally
* Whether face embeddings are persisted
* How reference images are selected and retained
* Whether Character Candidates are a separate aggregate
* Whether repeated names automatically create candidates
* How duplicate Characters are detected
* Whether merges create redirects or one shared identity with personas
* How Character split affects historical Translation records
* Whether Character corrections automatically trigger retranslation
* How Character Context Snapshot size is limited
* How chapter-cast relevance is scored
* How corrections are converted into learning candidates
* Which import formats are supported
* How publisher character sheets are mapped
* Whether Character exports support spoiler-safe filtering
* How Character data is synchronized between Projects in a series
* Whether real-person images are explicitly prohibited from recognition workflows

---

# Recommended MVP Scope

The first CRAI MVP should support:

* Project-level Character aggregate
* Stable Character identity
* Immutable Character Revisions
* Original name
* Preferred Vietnamese name
* Romanized name
* Aliases
* Basic Character Types
* Basic titles
* Basic gender state including Unknown
* Character notes
* Review state
* Approved and locked names
* Chapter-range scope
* Spoiler reveal boundary
* Character references from Glossary Entries
* Manual speaker assignment
* Optional AI speaker candidates
* Speaker Attribution confidence
* Unknown Speaker
* Narrator
* Basic Character relationships
* Vietnamese self-reference notes
* Relationship-specific address notes
* Basic Speech Profile
* Character Context Snapshot
* Translation context integration
* Character-aware validation
* Character merge
* Basic Character Candidate workflow
* JSON import and export
* Audit events
* Selective Translation staleness

The MVP may defer:

* Face recognition
* Persistent face embeddings
* Automatic visual tracking across Pages
* Body and soul identity modeling
* Reincarnation modeling
* Complex persona inheritance
* Real-time collaborative Character editing
* Semantic relationship extraction
* Automatic pronoun learning
* Full relationship graph queries
* Detailed age inference
* Voice recognition
* Global cross-Project Characters
* Series-level identity synchronization
* Advanced spoiler permissions
* Automatic dialogue attribution from bubble tails
* Automated Character merge
* Field-level approval inheritance
* Complex localized naming by region
* Character analytics
* External wiki synchronization
* Advanced group-speaker modeling

---

# Related Documents

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
* `docs/architecture/CAPABILITY_MAP.md`
* `docs/architecture/DATA_FLOW.md`
* `docs/architecture/STATE_MACHINE.md`
* `docs/architecture/EVENT_BUS.md`
* `docs/architecture/MODULE_DEPENDENCY.md`
* `docs/architecture/ai/PIPELINE.md`
* `docs/architecture/ai/CONTEXT.md`
* `docs/architecture/ai/MEMORY.md`
* `docs/architecture/ai/PROMPTS.md`
* `docs/architecture/ai/REQUEST.md`
* `docs/architecture/ai/RESPONSE.md`
* `docs/architecture/ai/ROUTING.md`
* `docs/architecture/ai/CACHE.md`
* `docs/architecture/presentation/LAYOUT.md`
* `docs/architecture/presentation/TYPOGRAPHY.md`
# Character Domain

* **Document:** Domain / Character
* **Version:** 1.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

The Character domain defines how CRAI represents fictional characters and other speaker-like entities that appear in comics, manga, manhua, novels and related content.

Character information helps CRAI maintain consistency for:

* Character names
* Aliases
* Titles
* Pronouns
* Gendered language
* Forms of address
* Speech style
* Relationships
* Speaker identification
* Translation context
* Glossary terminology
* Dialogue attribution
* Cross-chapter continuity

The Character domain provides stable identity and approved contextual information.

It must remain independent from:

* OCR providers
* Translation providers
* Computer-vision models
* Prompt formats
* Speaker-detection implementations
* Face-recognition implementations

---

# Domain Role

Character is a Project-level domain entity.

```text
Project
   │
   ├── Book
   │    ├── Chapters
   │    ├── Pages
   │    └── Text Blocks
   │
   └── Characters
        ├── Names
        ├── Aliases
        ├── Traits
        ├── Relationships
        ├── Appearances
        └── Context Snapshots
```

Characters may be referenced by:

* Glossary Entries
* Text Blocks
* Dialogue records
* Translation Context
* Translation Revisions
* Page annotations
* Character relationships
* Review findings

Character does not own those external records.

---

# Ownership Boundary

Character should be modeled as an Aggregate Root.

```text
Character Aggregate
├── Character
├── Character Revision
├── Character Name
├── Alias
├── Title
├── Pronoun Profile
├── Speech Profile
├── Character Trait
├── External Reference
├── Review State
└── Lifecycle State
```

The Character aggregate owns:

* Stable Character identity
* Character metadata
* Names and aliases
* Translation-specific name forms
* Titles
* Pronoun preferences
* Speech characteristics
* Stable character traits
* Review and approval state
* Character revision history
* Merge and split lineage

The Character aggregate does not own:

* Page appearances
* Image regions
* Face embeddings
* Dialogue Text Blocks
* Translation results
* Relationship graph as a whole
* Glossary matching results
* AI-generated observations
* Provider execution data

These are stored as references, projections or separate aggregates.

---

# Responsibilities

The Character domain is responsible for:

* Maintaining stable character identity
* Managing original and translated names
* Managing aliases, titles and nicknames
* Describing pronoun and form-of-address preferences
* Describing stable speech characteristics
* Supporting character disambiguation
* Supporting relationship references
* Maintaining immutable character revisions
* Supporting merge and split operations
* Supporting review and approval
* Producing character context snapshots
* Emitting character-related events
* Preserving historical translation context

The Character domain is not responsible for:

* Detecting characters in images
* Recognizing faces
* Detecting speakers
* Assigning dialogue automatically
* Translating dialogue
* Performing entity extraction
* Building AI prompts
* Rendering character labels
* Generating character summaries from copyrighted content

Those operations belong to application, AI, vision, language-processing and presentation capabilities.

---

# Aggregate Structure

Recommended conceptual structure:

```text
Character
├── Character ID
├── Project ID
├── Character Type
├── Canonical Name Reference
├── Active Revision
├── Lifecycle State
├── Review State
├── Created At
├── Updated At
└── Version
```

Character details belong to immutable revisions.

```text
Character Revision
├── Character Revision ID
├── Character ID
├── Names
├── Aliases
├── Titles
├── Pronoun Profile
├── Speech Profile
├── Traits
├── Notes
├── Scope
├── External References
├── Parent Revision
├── Created By
├── Created At
└── Content Hash
```

---

# Character Identity

Each Character has a stable identifier.

```text
Character ID != Character Revision ID
```

The Character ID represents the fictional entity across:

* Chapters
* Pages
* Images
* Name changes
* Aliases
* Disguises
* Translations
* Character development
* Revision history

Editing a character does not create a new Character identity.

A new Character ID should only be created when the content represents a distinct entity.

---

# Character Revision

Character Revision is an immutable representation of known character information at a point in time.

A new revision may be created when:

* A name is corrected
* An alias is added
* A title changes
* Pronoun preferences are corrected
* A speech profile changes
* A relationship note is updated
* Two identities are confirmed as the same
* An assumed identity is disproved
* Translation-specific naming is approved
* User review changes character metadata

Once referenced by a Translation Context Snapshot, a Character Revision must remain immutable.

---

# Character Type

Recommended Character Types:

* Person
* Creature
* Spirit
* Deity
* Artificial Intelligence
* Sentient Object
* Group Speaker
* Narrator
* System Voice
* Unknown Speaker
* Disguised Identity
* Alternate Persona
* Custom

Character Type supports contextual reasoning.

It must not be used as a substitute for identity.

---

# Canonical Character Name

A Character should have one canonical name reference for display and organization.

The canonical name may be:

* Original-language name
* Most authoritative published name
* User-selected name
* Temporary descriptive identifier

Examples:

```text
林月
Lâm Nguyệt
Unknown Woman in Red
System
Narrator
```

The canonical name is not necessarily the name supplied to Translation.

Translation uses the applicable language-specific Character Name.

---

# Character Name

A Character Name is a language-aware representation of the character’s identity.

Recommended structure:

```text
Character Name
├── Name ID
├── Text
├── Language
├── Script
├── Name Type
├── Usage Scope
├── Preference Rank
├── Approval State
├── Valid From
├── Valid Until
└── Notes
```

A Character may have several names.

Example:

```text
Character: CH-001

Names:
- 林月       — Original, zh-Hans
- Lín Yuè   — Romanization
- Lâm Nguyệt — Preferred Vietnamese
- Moon Lin  — Deprecated English adaptation
```

---

# Name Types

Recommended Name Types:

* Original
* Canonical
* Translated
* Localized
* Transliterated
* Romanized
* Legal Name
* Given Name
* Family Name
* Full Name
* Courtesy Name
* Art Name
* Title Name
* Code Name
* Nickname
* Alias
* Disguise Name
* Temporary Identifier
* Unknown Identifier
* Deprecated Form

Name Type and language must remain separate.

---

# Original Name

Original Name represents the character name as it appears in the source content.

Requirements:

* Preserve original script
* Preserve meaningful punctuation
* Preserve source-language distinctions
* Avoid destructive normalization
* Record the applicable language
* Record evidence when extracted automatically

An Original Name should not be replaced by a translated name.

Both forms should remain available.

---

# Translated Name

A Translated Name is a meaning-based target-language rendering.

Example:

```text
黑龙王 → Hắc Long Vương
```

A translated name may preserve semantic meaning rather than pronunciation.

Translated Name and Transliterated Name must remain distinguishable.

---

# Transliterated Name

A Transliterated Name preserves approximate pronunciation across scripts.

Example:

```text
林月 → Lâm Nguyệt
```

The transliteration standard or convention should be recorded when relevant.

Possible metadata:

* Source language
* Source script
* Target script
* Transliteration convention
* Language adaptation policy
* Approval state

---

# Romanized Name

Romanization converts a name to Latin script using a declared standard.

Examples:

* Pinyin
* Hepburn
* Revised Romanization
* Wade–Giles

```text
林月 → Lín Yuè
```

Romanization should not automatically become the preferred Vietnamese character name.

---

# Localized Name

A Localized Name adapts the name for a target audience.

Localization may involve:

* Pronunciation adaptation
* Cultural naming convention
* Historical Sino-Vietnamese reading
* Official publisher terminology
* Simplified spelling
* Removal of unfamiliar diacritics

Localized names should record their authority and scope.

---

# Name Preference

Name preference may depend on:

* Target language
* Project
* Book
* Chapter range
* Translation Profile
* Reader preference
* Publication convention
* Dialogue versus narration
* Speaker relationship
* Historical point in the story

Recommended resolution:

```text
Explicit Translation Override
        ↓
Approved Scope-Specific Name
        ↓
Approved Target-Language Name
        ↓
Project Preferred Name
        ↓
Transliterated Name
        ↓
Original Name
        ↓
Temporary Identifier
```

The resolution result should record why a name was selected.

---

# Name Scope

A name may be valid only in a specific scope.

Examples:

* Childhood name used in early chapters
* Married name used later
* Disguise name used during one story arc
* Title used only after promotion
* Secret identity revealed after a chapter
* Nickname used by one relationship group

Recommended scope properties:

```text
Name Scope
├── Book IDs
├── Chapter Range
├── Page Range
├── Story Arc
├── Speaker Character IDs
├── Audience
├── Translation Profile IDs
└── Exclusions
```

---

# Temporal Name Validity

Character names and titles may change over the story timeline.

Recommended fields:

* Valid From Chapter
* Valid Until Chapter
* Introduced At
* Revealed At
* Deprecated At
* Spoiler Level

Translation must avoid using names that have not yet been revealed within the applicable story context.

---

# Spoiler-Sensitive Identity

Character metadata may contain spoilers.

Examples:

* True name
* Secret identity
* Family relationship
* Hidden faction
* Future title
* Character death
* Betrayal
* Alternate form

Spoiler-sensitive data should carry visibility constraints.

Recommended metadata:

```text
Spoiler Scope
├── Reveal Chapter
├── Reveal Page
├── Minimum Reader Progress
├── Visibility Policy
└── Allowed Processing Context
```

Context construction must not include future information when translating earlier chapters unless explicitly configured.

---

# Alias

An Alias is an alternate form used to refer to the same Character.

Examples:

* Nickname
* Abbreviation
* Code name
* Disguise identity
* Misspelling
* OCR variation
* Honorific form
* Relationship-based address

Recommended structure:

```text
Character Alias
├── Alias ID
├── Text
├── Language
├── Script
├── Alias Type
├── Scope
├── Match Policy
├── Approval State
└── Notes
```

Aliases should reference one Character identity.

---

# Alias Types

Recommended Alias Types:

* Nickname
* Code Name
* Disguise
* Title
* Relationship Address
* Abbreviation
* Romanization Variant
* Translation Variant
* OCR Variant
* Common Misspelling
* Informal Form
* Formal Form
* Self-Reference
* Temporary Alias
* Deprecated Alias

OCR variants must not be treated as approved display names.

---

# Title

A Title represents a role or status associated with a Character.

Examples:

* Sect Master
* Emperor
* Captain
* Young Master
* Saintess
* General
* Doctor
* Professor

Recommended structure:

```text
Character Title
├── Title ID
├── Source Form
├── Target Forms
├── Language Scope
├── Title Type
├── Validity Scope
├── Priority
├── Approval State
└── Glossary Entry Reference
```

Titles may be owned as Character metadata while their terminology translations are managed through Glossary.

---

# Character and Glossary Boundary

Character owns identity and character-specific naming information.

Glossary owns translation terminology rules.

Recommended relationship:

```text
Character
├── Character ID
├── Original Name
├── Preferred Display Name
└── Glossary Entry References
```

```text
Glossary Entry
├── Source Forms
├── Target Forms
├── Matching Rules
├── Translation Rule
└── Character Reference
```

A Character should not duplicate the complete matching and precedence logic of Glossary.

A Glossary Entry should not become the canonical owner of Character identity.

---

# Pronoun Profile

Pronoun Profile describes how the character may be referenced in target-language translation.

Recommended structure:

```text
Pronoun Profile
├── Grammatical Gender
├── Self-Reference Forms
├── Third-Person Forms
├── Address Preferences
├── Formality
├── Language Scope
├── Relationship Overrides
├── Scope
└── Confidence
```

Pronoun Profile should support languages where pronoun use depends heavily on social context.

---

# Grammatical Gender

Possible values may include:

* Masculine
* Feminine
* Neutral
* Nonbinary
* Variable
* Unknown
* Not Applicable

Grammatical Gender is translation metadata.

It must not be treated as a complete representation of identity.

Unknown information must remain unknown rather than being guessed.

---

# Vietnamese Address Context

Vietnamese pronouns and forms of address depend on:

* Age
* Relative age
* Social rank
* Family relationship
* Intimacy
* Formality
* Speaker
* Listener
* Narrative context
* Historical setting
* Genre

A single global pronoun per Character is insufficient.

Recommended model:

```text
Address Rule
├── Speaker Character
├── Listener Character
├── Speaker Self-Reference
├── Listener Address Form
├── Third-Person Reference
├── Formality
├── Scope
└── Approval State
```

Examples:

```text
Character A → Character B:
Self: ta
Address: ngươi

Character B → Character A:
Self: đệ tử
Address: sư phụ
```

---

# Self-Reference

Some characters use distinctive first-person forms.

Examples in Vietnamese translation:

* tôi
* ta
* bản tọa
* trẫm
* bổn cung
* tại hạ
* lão phu
* tiểu nữ
* đệ tử
* thuộc hạ

Self-reference may depend on:

* Target language
* Speaker
* Listener
* Story period
* Formality
* Character role
* Translation style

Self-reference must not be determined only from gender.

---

# Address Form

Address Form defines how one Character refers to another.

Examples:

* ngươi
* anh
* chị
* em
* sư phụ
* sư huynh
* điện hạ
* bệ hạ
* đại nhân
* tiền bối

Address Form belongs to relationship-aware context.

It may reference Glossary Entries for terminology consistency.

---

# Speech Profile

Speech Profile captures stable characteristics that influence translation style.

Recommended structure:

```text
Speech Profile
├── Formality
├── Register
├── Tone
├── Sentence Style
├── Vocabulary Preferences
├── Verbal Habits
├── Self-Reference
├── Honorific Usage
├── Dialect Notes
├── Restrictions
├── Scope
└── Confidence
```

Possible speech characteristics:

* Formal
* Casual
* Archaic
* Childlike
* Polite
* Aggressive
* Technical
* Poetic
* Concise
* Verbose
* Comedic
* Emotionless

Speech Profile should be descriptive rather than provider-specific.

---

# Speech Habit

A Speech Habit may describe repeated behavior such as:

* Catchphrase
* Sentence ending
* Repeated interjection
* Honorific preference
* Stutter
* Archaic vocabulary
* Third-person self-reference
* Deliberate lack of contractions
* Specific punctuation style

Speech habits should not force mechanical replacements that damage natural target-language grammar.

---

# Character Trait

Character Traits represent relatively stable contextual facts.

Examples:

* Approximate age group
* Social rank
* Occupation
* Species
* Affiliation
* Personality descriptor
* Combat role
* Narrative role
* Knowledge level

Recommended structure:

```text
Character Trait
├── Trait Type
├── Value
├── Confidence
├── Source
├── Validity Scope
├── Review State
└── Spoiler Scope
```

Traits should be included only when useful to translation or understanding.

---

# Stable and Dynamic Traits

Traits should distinguish between:

## Stable Traits

Usually persistent across the work:

* Species
* Basic identity
* General speech style
* Original family
* Primary name

## Dynamic Traits

May change by chapter or arc:

* Rank
* Faction
* Age
* Relationship
* Loyalty
* Injury
* Disguise
* Emotional state
* Current title

Dynamic facts should use scoped observations rather than overwrite permanent character identity.

---

# Observation

AI or users may create observations about a Character.

Examples:

* Appears to be angry
* Uses formal speech
* May be the speaker
* Wearing red clothing
* Referred to as “Master”

An Observation is not automatically canonical Character truth.

Recommended structure:

```text
Character Observation
├── Observation ID
├── Character Candidate
├── Observation Type
├── Value
├── Evidence References
├── Confidence
├── Scope
├── Observer
├── Observer Revision
└── Review State
```

Observations belong to a separate extraction or analysis workflow.

Approved observations may later create Character revisions.

---

# Appearance

A Character Appearance links a Character to a content location.

Recommended structure:

```text
Character Appearance
├── Appearance ID
├── Character ID
├── Character Revision
├── Book ID
├── Chapter ID
├── Page ID
├── Image Version
├── Region
├── Appearance Type
├── Confidence
├── Identification Source
└── Review State
```

Appearance is derived or contextual data.

It should not be embedded directly inside the Character aggregate.

---

# Appearance Types

Possible Appearance Types:

* Visible
* Partial
* Silhouette
* Portrait
* Background
* Flashback
* Illustration
* Mentioned
* Speaking Off-Panel
* Narrating
* System Voice
* Unknown

A Character may be present without being visually shown.

---

# Visual Identity

Visual identity may be represented through:

* Face references
* Clothing descriptors
* Hair descriptors
* Color descriptors
* Body features
* Accessories
* Character sheets
* Reference images
* Embedding references

The Character domain may store approved descriptive metadata or references.

Raw embeddings and provider model outputs belong to infrastructure or recognition services.

---

# Face Embeddings

Face embeddings must not be canonical Character data.

Recommended relationship:

```text
Character
    │
    └── Recognition Profile Reference
            └── Face Embeddings
```

Embeddings are:

* Model-specific
* Version-specific
* Rebuildable
* Potentially sensitive
* Not human-readable

They must include model and preprocessing revisions.

---

# Speaker Attribution

Speaker Attribution associates dialogue with a Character.

```text
Speaker Attribution
├── Text Block ID
├── Text Block Revision
├── Character ID
├── Character Revision
├── Attribution Method
├── Confidence
├── Evidence
└── Review State
```

Attribution methods may include:

* User Assigned
* Bubble Tail Analysis
* Proximity
* Face Association
* Dialogue Pattern
* Name Mention
* Novel Speaker Tag
* AI Inference
* Imported Metadata

Speaker Attribution belongs outside the Character aggregate.

---

# Speaker Confidence

Speaker inference may be uncertain.

Recommended normalized confidence:

```text
0.0 to 1.0
```

Possible interpretation:

| Confidence  | Meaning         |
| ----------- | --------------- |
| `0.90–1.00` | Strong evidence |
| `0.70–0.89` | Likely          |
| `0.40–0.69` | Ambiguous       |
| `0.00–0.39` | Weak            |

Thresholds must remain configurable.

A low-confidence speaker attribution must not silently impose character-specific pronouns.

---

# Unknown Speaker

Unknown Speaker should be represented explicitly.

Possible approaches:

* No Character reference
* Project-level Unknown Speaker entity
* Page-scoped temporary candidate

Recommended behavior:

* Preserve uncertainty
* Avoid unsupported gender assumptions
* Avoid relationship-specific address rules
* Allow later reconciliation
* Keep original attribution evidence

Unknown Speaker must not be automatically merged with Narrator.

---

# Narrator

Narrator may be modeled as a Character when it has:

* Stable identity
* Stable speech style
* Repeated voice
* Translation-specific pronoun behavior

For neutral narration without identity, a specialized Narration Context may be sufficient.

Possible narrator types:

* Omniscient Narrator
* First-Person Narrator
* Character Narrator
* Unreliable Narrator
* System Narrator
* Unknown Narrator

---

# System Voice

Comics and novels may include non-character speakers:

* Game system
* Quest notification
* Interface message
* Divine announcement
* AI assistant
* Automated broadcast

These may be modeled as Characters of type `System Voice` when identity and style consistency matter.

Presentation-specific UI labels remain separate.

---

# Character Relationship

A Character Relationship connects two or more Character identities.

Recommended relationship structure:

```text
Character Relationship
├── Relationship ID
├── Source Character ID
├── Target Character ID
├── Relationship Type
├── Direction
├── Address Rules
├── Validity Scope
├── Confidence
├── Review State
└── Revision
```

Relationship should normally be a separate aggregate or graph projection.

Character aggregates may store relationship references but should not own the complete graph.

---

# Relationship Types

Possible relationship types:

* Family
* Parent
* Child
* Sibling
* Spouse
* Romantic
* Friend
* Rival
* Enemy
* Master
* Disciple
* Superior
* Subordinate
* Employer
* Employee
* Leader
* Follower
* Teammate
* Faction Member
* Acquaintance
* Unknown
* Custom

Relationship type should be directional where meaning requires it.

```text
Master → Disciple
```

is not equivalent to:

```text
Disciple → Master
```

---

# Relationship Revision

Relationships may change over time.

Examples:

* Stranger becomes friend
* Disciple becomes rival
* Hidden family relationship is revealed
* Superior becomes subordinate
* Enemy alliance becomes temporary cooperation

Relationship revisions must support:

* Chapter scope
* Story arc
* Reveal point
* Confidence
* Historical context

Translation of earlier chapters must use the applicable historical relationship state.

---

# Character Group

A group may function as one speaker or entity.

Examples:

* Crowd
* Soldiers
* Council
* Villagers
* Audience
* Chorus
* System administrators

A Character Group may be represented as:

* Character Type `Group Speaker`
* Separate Group aggregate referenced by dialogue
* Temporary context entity

The MVP may use Group Speaker characters for simplicity.

---

# Alternate Persona

One Character may have several personas.

Examples:

* Secret identity
* Possessed state
* Transformation
* Disguise
* Split personality
* Reincarnated identity
* Body swap

Possible modeling approaches:

1. One Character with scoped personas
2. Separate Character identities linked through a relationship
3. One underlying identity plus presentation profiles

The choice depends on whether the personas behave as independently referenced entities in the source.

---

# Persona

Recommended Persona structure:

```text
Character Persona
├── Persona ID
├── Character ID
├── Name Forms
├── Appearance Profile
├── Speech Profile
├── Validity Scope
├── Reveal Scope
└── Review State
```

Persona should remain subordinate to Character only when it shares one stable underlying identity.

---

# Reincarnation and Body Swap

Complex identity cases must distinguish:

* Soul identity
* Body identity
* Public identity
* Speaker identity
* Name currently used
* Reader knowledge

The Character domain should avoid forcing one universal identity model.

Domain references may include an Identity Aspect:

* Underlying Character
* Visible Body
* Public Persona
* Speaker Persona

Advanced identity modeling may be deferred beyond MVP.

---

# Character Candidate

Automatic extraction may create Character Candidates.

```text
Character Candidate
├── Candidate ID
├── Proposed Names
├── Appearance References
├── Dialogue References
├── Trait Suggestions
├── Similar Character References
├── Confidence
├── Detection Source
├── Detection Revision
└── Review State
```

Candidates are not canonical Characters.

They require:

* User confirmation
* Policy-based promotion
* Merge into an existing Character
* Rejection

---

# Candidate Promotion

Promoting a candidate may:

* Create a new Character
* Add an alias to an existing Character
* Add appearance evidence
* Add a speaker attribution
* Create a possible duplicate warning

Promotion should preserve all source evidence.

---

# Duplicate Detection

Potential duplicate Characters may be detected through:

* Equivalent names
* Shared aliases
* Visual similarity
* Overlapping appearances
* Dialogue continuity
* Entity references
* Relationship patterns
* User review

Duplicate detection should create a review candidate.

It must not automatically merge authoritative Characters.

---

# Character Merge

Character Merge consolidates two identities confirmed to represent the same entity.

Merge operation should:

* Select a surviving Character ID
* Preserve all Character Revisions
* Preserve names and aliases
* Preserve appearance references
* Preserve speaker attributions
* Preserve relationships
* Preserve Glossary references
* Record redirect lineage
* Detect conflicting metadata
* Emit merge events

Historical references to the merged Character must remain resolvable.

---

# Merge Conflict

Possible merge conflicts include:

* Different original names
* Incompatible pronoun profiles
* Overlapping appearances suggesting separate entities
* Conflicting story scopes
* Different external references
* Different locked translations
* Contradictory relationships

Conflicts require review before final merge.

---

# Character Split

Character Split is needed when one Character identity incorrectly combines several entities.

Examples:

* Twins identified as one person
* Narrator and protagonist merged
* Disguise identity incorrectly merged
* Same surname treated as one Character
* Two speakers assigned to one candidate

Split operation should:

* Create new Character IDs
* Preserve original lineage
* Reassign selected names
* Reassign appearances
* Reassign speaker attributions
* Reassign relationships
* Reconcile Glossary references
* Mark affected Translations for review

---

# Character Lifecycle

Recommended lifecycle states:

* Candidate
* Active
* Inactive
* Missing
* Deceased
* Historical
* Merged
* Split
* Archived
* Rejected

Lifecycle state should not reveal spoilers unless visibility policy permits it.

For example, `Deceased` may be stored with a reveal scope.

---

# Candidate State

Candidate Characters are unconfirmed.

They may be used for:

* Review interfaces
* Temporary attribution
* Duplicate comparison
* Evidence collection

They should not influence authoritative Translation without policy approval.

---

# Active State

Active means the Character may participate in:

* Translation context
* Glossary resolution
* Speaker attribution
* Relationship rules
* Character search

Active does not mean the Character is alive in the story.

---

# Inactive State

Inactive Characters remain stored but are excluded from ordinary current processing.

Possible reasons:

* Imported but unused
* Duplicate under review
* No longer relevant
* User-hidden
* Temporarily disabled

---

# Merged State

Merged Characters redirect to the surviving Character.

The old identity remains available for:

* Historical references
* Audit
* Import reconciliation
* External links

---

# Review State

Recommended Character review states:

* Unreviewed
* AI Suggested
* User Confirmed
* Under Review
* Approved
* Needs Changes
* Rejected
* Locked

Lifecycle and Review State remain separate.

An Active Character may still be unreviewed if provisional processing is allowed.

---

# Approval

Approval applies to an exact Character Revision.

Approval may cover:

* Identity
* Names
* Pronouns
* Speech Profile
* Traits
* Relationship references
* Spoiler metadata

Editing approved semantic data creates a new revision that requires review.

Non-semantic metadata changes may inherit approval according to policy.

---

# Locked Character

A locked Character or Character field represents authoritative information.

Examples:

* Official character name
* Publisher-approved translation
* Main-character identity
* User-pinned pronoun profile
* Confirmed speaker style

Locking may be field-specific.

```text
Character Lock
├── Locked Fields
├── Scope
├── Authority
├── Actor
├── Created At
└── Reason
```

Imported or AI-generated data must not overwrite locked fields.

---

# Character Context Snapshot

Translation should consume immutable Character Context Snapshots.

Recommended structure:

```text
Character Context Snapshot
├── Snapshot ID
├── Project ID
├── Story Scope
├── Character Revision References
├── Relationship Revision References
├── Relevant Name Forms
├── Pronoun Rules
├── Speech Profiles
├── Spoiler Boundary
├── Created At
└── Content Hash
```

The snapshot should include only characters relevant to the translation operation.

---

# Snapshot Selection

Relevant characters may be selected through:

* Speaker attribution
* Characters visible on Page
* Characters mentioned in source text
* Characters active in nearby Pages
* Chapter cast
* Relationship context
* Dialogue history
* User pinning
* Semantic retrieval

Selection must respect:

* Context budget
* Spoiler boundaries
* Project visibility
* Review policy
* Confidence thresholds

---

# Snapshot Immutability

Once referenced by a Translation Revision, a Character Context Snapshot must remain immutable.

Later Character changes produce a new snapshot.

Historical Translation Revisions retain references to the old snapshot.

---

# Translation Integration

Character context may influence:

* Name selection
* Pronouns
* Forms of address
* Formality
* Speech style
* Gendered terms
* Relationship terminology
* Dialogue consistency
* Narrator voice

Conceptual request:

```text
Translation Request
├── Source Text
├── Language Pair
├── Translation Profile
├── Glossary Snapshot
├── Character Context Snapshot
└── Story Context Snapshot
```

Character domain data should be compiled into provider-neutral context before entering provider adapters.

---

# Prompt Integration

Character records must not store canonical provider-specific prompts.

Recommended flow:

```text
Character Context Snapshot
        │
        ▼
Context Compiler
        │
        ▼
Provider-Neutral Character Instructions
        │
        ▼
Provider Adapter
        │
        ▼
Provider-Specific Request
```

Provider-specific prompt formatting belongs to AI infrastructure.

---

# Context Budget

A large cast may exceed provider limits.

Context selection may prioritize:

1. Confirmed speaker
2. Confirmed listener
3. Characters mentioned in current source
4. Visible Page characters
5. Recently active characters
6. Relationship-linked characters
7. Chapter-level main cast
8. Other Project characters

The system should record which Character Revisions were actually included.

---

# Applied Character Context

Translation execution may record:

```text
Applied Character Context
├── Character ID
├── Character Revision ID
├── Context Role
├── Speaker Confidence
├── Selected Name Form
├── Applied Pronoun Rule
├── Applied Speech Profile
└── Application Result
```

Context roles may include:

* Speaker
* Listener
* Mentioned
* Visible
* Narrator
* Relationship Context
* Background Context
* User Pinned

---

# Translation Validation

Character-aware validation may detect:

* Wrong character name
* Deprecated name used
* Name used before reveal
* Incorrect pronoun
* Incorrect self-reference
* Relationship address mismatch
* Speech style inconsistency
* Wrong character attribution
* Inconsistent title
* Character identity conflict
* Spoiler leakage

Validation findings must reference exact Character Revisions and Translation Revisions.

---

# Character Validation Finding

Recommended structure:

```text
Character Validation Finding
├── Finding ID
├── Translation Revision ID
├── Character ID
├── Character Revision ID
├── Finding Type
├── Severity
├── Source Range
├── Target Range
├── Expected Context
├── Observed Text
├── Confidence
├── Validator Revision
└── Resolution State
```

---

# Validation Finding Types

Recommended finding types:

* Character Name Mismatch
* Unapproved Alias Used
* Deprecated Name Used
* Premature Identity Reveal
* Pronoun Mismatch
* Self-Reference Mismatch
* Address Form Mismatch
* Title Mismatch
* Speaker Attribution Conflict
* Speech Profile Inconsistency
* Character Context Missing
* Relationship Context Missing
* Unknown Character Reference
* Locked Character Rule Violation

---

# User Correction

A user may correct:

* Character name
* Speaker identity
* Pronoun
* Self-reference
* Address form
* Title
* Relationship
* Alias
* Character merge or split

A correction may produce:

1. A new Translation Revision
2. A Character update suggestion
3. A Speaker Attribution revision
4. A Glossary Candidate
5. A Relationship revision candidate

Translation correction must not automatically mutate canonical Character data unless policy explicitly allows it.

---

# Learning from Corrections

Example:

```text
Source:
李青说道：“我会回来。”

Generated:
Lý Thanh nói: “Tôi sẽ quay lại.”

User Correction:
Lý Thanh nói: “Ta sẽ trở lại.”
```

Possible inferred suggestion:

```text
Character: Lý Thanh
Self-reference: ta
Scope: cultivation dialogue
```

This remains a candidate until approved.

---

# Character Change Impact

Character changes may affect Translation artifacts.

Possible impact classifications:

* No Impact
* Metadata Only
* Review Recommended
* Validation Required
* Retranslation Recommended
* Retranslation Required

Examples:

| Character Change                  | Typical Impact                    |
| --------------------------------- | --------------------------------- |
| Description updated               | No Impact                         |
| Search tag added                  | No Impact                         |
| Alias added                       | Validation may be useful          |
| Preferred Vietnamese name changed | Retranslation recommended         |
| Locked name corrected             | Retranslation required            |
| Pronoun Profile changed           | Review or retranslation           |
| Relationship rule changed         | Affected dialogues require review |
| Spoiler boundary corrected        | Validation required               |
| Speaker attribution changed       | Retranslation may be required     |
| Character merge                   | Reconciliation required           |

Only affected Translations should become stale.

---

# Affected Translation Detection

Potential evidence:

* Character Context Snapshot references
* Applied Character Context references
* Speaker Attribution references
* Glossary Entry references
* Character Validation findings
* Translation source mentions
* Story scope
* Relationship references

Changing one minor Character should not invalidate every Project Translation.

---

# Character Context Hash

Character context participates in Translation reproducibility.

Possible configuration inputs:

```text
Source Hash
+
Language Pair
+
Translation Profile Revision
+
Glossary Snapshot Hash
+
Character Context Snapshot Hash
+
Story Context Snapshot Hash
+
Prompt Revision
```

Changes to relevant Character context create a new configuration identity.

---

# Import

Characters may be imported from:

* JSON
* YAML
* CSV
* Spreadsheet
* EPUB metadata
* Existing wiki
* Publisher character sheet
* Another CRAI Project
* AI-generated candidates
* User-maintained notes

Import should create a reviewable plan.

It must not silently overwrite approved Characters.

---

# Character Import Plan

Recommended structure:

```text
Character Import Plan
├── Import ID
├── Source Hash
├── Proposed New Characters
├── Proposed Updates
├── Possible Duplicates
├── Name Conflicts
├── Relationship Conflicts
├── Invalid Records
├── Language Mapping
└── Review State
```

---

# Import Conflict Resolution

Possible actions:

* Create Character
* Create Character Revision
* Add Name
* Add Alias
* Add External Reference
* Add Candidate Trait
* Merge with Existing
* Ignore
* Keep Existing
* Require Manual Review

Locked fields cannot be replaced by ordinary import.

---

# Export

Character export may support:

* Full Project cast
* Selected Book
* Selected Chapter range
* Approved Characters only
* Public spoiler-safe view
* Full internal view
* Character context snapshot
* Human review format
* CRAI-native round-trip format

---

# Spoiler-Safe Export

Spoiler-safe export should exclude information not visible at the selected progress point.

This may include:

* True identities
* Future aliases
* Future relationships
* Death state
* Future factions
* Future titles
* Hidden powers
* Revealed family connections

The export must declare its story boundary.

---

# Round-Trip Export

A CRAI-native export should preserve:

* Character IDs
* Character Revision IDs
* Name IDs
* Language and script
* Scope
* Review states
* Locks
* Spoiler metadata
* Merge and split lineage
* External references
* Content hashes

Simple CSV export may lose advanced metadata.

---

# External References

Character may reference:

* Publisher identifier
* Wiki page identifier
* Source platform identifier
* Imported dataset identifier
* User note identifier
* Image reference identifier
* Knowledge graph identifier

External references must not become canonical Character identity.

They may change or disappear.

---

# Persistence

Recommended canonical persistence separation:

```text
Character
Character Revision
Character Name
Character Alias
Character Title
Character Lock
Character Review
Character Merge
Character Split
Character Context Snapshot
Character Context Snapshot Item
```

Separate contextual or derived persistence:

```text
Character Candidate
Character Observation
Character Appearance
Speaker Attribution
Character Relationship
Recognition Profile
Character Search Index
```

Derived data must be rebuildable where possible.

---

# Search Index

Character search may index:

* Original names
* Translated names
* Aliases
* Titles
* Romanizations
* Traits
* Affiliations
* External references
* Notes

Search index content is derived.

Search failure must not affect Character truth.

---

# Recognition Index

Recognition infrastructure may index:

* Face embeddings
* Visual descriptors
* Clothing descriptors
* Character reference images
* Voice or dialogue style vectors

Recognition indexes must reference:

* Character ID
* Character Revision
* Model revision
* Preprocessing revision
* Source evidence

They must remain outside canonical Character persistence.

---

# Cache Participation

Character-related cache keys may include:

* Character Context Snapshot Hash
* Character Revision IDs
* Relationship Revision IDs
* Speaker Attribution Revision
* Story Scope
* Spoiler Boundary
* Translation Profile Revision
* Context Compiler Revision

Mutable Character IDs alone are insufficient for cache correctness.

---

# Concurrency

Concurrent Character editing should use optimistic concurrency.

Possible checks:

* Character aggregate version
* Active Character Revision
* Expected parent revision
* Content hash
* Lock state

Concurrent edits may result in:

* Automatic merge for independent metadata
* Parallel draft revisions
* Manual conflict resolution
* Stale-write rejection

Approved data must not be silently overwritten.

---

# Idempotency

Idempotency may apply to:

* Importing the same Character dataset
* Creating candidates from the same evidence
* Confirming an existing name
* Adding an existing alias
* Approving the same revision
* Merging the same Characters
* Publishing the same context snapshot

Possible idempotency inputs:

* Operation key
* Source hash
* Character content hash
* Evidence hash
* Parent revision

---

# Deletion

Hard deletion should be exceptional.

A Character referenced by:

* Translation Revisions
* Character Context Snapshots
* Glossary Entries
* Speaker Attributions
* Appearances
* Relationships
* Audit records

must not be physically deleted under normal operation.

Preferred operations:

* Reject candidate
* Inactivate
* Archive
* Merge
* Redirect
* Mark as mistaken identity

Hard deletion may apply only to unreferenced drafts or legal requirements.

---

# Retention

The system should retain:

* Referenced Character Revisions
* Character Context Snapshots
* Approved names
* Merge and split lineage
* Speaker attribution history
* Review records
* Import provenance
* Audit events

Temporary unreviewed candidates and derived recognition vectors may have shorter retention policies.

---

# Security

Character permissions may include:

* View
* View Spoilers
* Suggest
* Create
* Edit Draft
* Approve
* Lock
* Merge
* Split
* Import
* Export
* Manage Recognition Data

Spoiler access should be independent from ordinary view permission.

---

# Privacy

Character data may include sensitive Project information:

* Unreleased story details
* Licensed names
* Publisher terminology
* Private reading notes
* User corrections
* Uploaded reference images
* Embeddings derived from images

Requirements:

* Prevent cross-Project leakage
* Respect spoiler visibility
* Minimize context sent to providers
* Exclude irrelevant characters
* Respect local-only mode
* Protect reference images and embeddings
* Record Character exports
* Avoid using Character data to infer real-person identity

CRAI Character recognition is intended for fictional content organization, not real-world biometric identification.

---

# Audit

Important actions should be auditable:

* Character created
* Revision created
* Name changed
* Pronoun changed
* Character approved
* Character locked
* Characters merged
* Character split
* Identity confirmed
* Speaker attribution corrected
* Spoiler boundary changed
* Character exported

Audit records should include:

* Actor
* Time
* Previous revision
* New revision
* Reason
* Scope
* Correlation ID

---

# Events

Typical domain events include:

* `CharacterCreated`
* `CharacterRevisionCreated`
* `CharacterActivated`
* `CharacterInactivated`
* `CharacterApproved`
* `CharacterRejected`
* `CharacterLocked`
* `CharacterUnlocked`
* `CharacterNameAdded`
* `CharacterNameChanged`
* `CharacterAliasAdded`
* `CharacterTitleChanged`
* `CharacterPronounProfileChanged`
* `CharacterSpeechProfileChanged`
* `CharacterCandidateDetected`
* `CharacterCandidatePromoted`
* `CharacterDuplicateDetected`
* `CharactersMerged`
* `CharacterSplit`
* `CharacterRelationshipChanged`
* `CharacterAppearanceDetected`
* `SpeakerAttributed`
* `SpeakerAttributionCorrected`
* `CharacterContextSnapshotCreated`

Events should contain identifiers and revision references rather than full character biographies.

---

# Event Payload Example

```text
CharacterRevisionCreated
├── Project ID
├── Character ID
├── Character Revision ID
├── Parent Revision ID
├── Change Types
├── Story Scope
├── Actor
├── Occurred At
└── Correlation ID
```

Spoiler-sensitive details should not be included in broadly distributed event payloads.

---

# Comic Processing Example

```text
Page contains:
- Character A
- Character B
- Two speech bubbles

Visual Analysis:
- Character A visible near Bubble 1
- Character B visible near Bubble 2

Speaker Attribution:
- Bubble 1 → Character A, confidence 0.94
- Bubble 2 → Character B, confidence 0.82

Character Context:
- A refers to self as “ta”
- A addresses B as “ngươi”
- B refers to A as “sư huynh”

Translation:
- Bubble 1 applies Character A Speech Profile
- Bubble 2 applies Character B relationship rules
```

Attribution confidence and character context remain separately recorded.

---

# Novel Processing Example

```text
Source Paragraph:
Lâm Nguyệt nói: “Sư phụ, con đã trở về.”

Structured Extraction:
- Speaker tag: Lâm Nguyệt
- Listener mention: Sư phụ

Character Resolution:
- Speaker → Character CH-001
- Listener → Character CH-004

Relationship:
- CH-001 is disciple of CH-004

Translation Context:
- Self-reference: đệ tử or con, depending on profile
- Address form: sư phụ
```

Speaker tags from source structure should outrank uncertain AI inference.

---

# Name Resolution Example

Character:

```text
Original:
林月

Romanized:
Lín Yuè

Preferred Vietnamese:
Lâm Nguyệt

Disguise Name:
Bạch Linh

Disguise Scope:
Chapters 40–52
```

When translating Chapter 45, `Bạch Linh` may be selected if the source uses the disguise identity.

When translating Chapter 20, the future disguise name must not enter context.

---

# Pronoun Example

```text
Character A:
- Older sect leader
- Formal, authoritative speech

Character B:
- Junior disciple

A → B:
- Self-reference: ta
- Address: ngươi

B → A:
- Self-reference: đệ tử
- Address: sư phụ
```

These rules are relationship-specific and cannot be derived from a global gender field alone.

---

# Unknown Speaker Example

```text
Text Block:
“Không được tiến thêm bước nào!”

Speaker Candidates:
- Guard A: 0.42
- Guard B: 0.38
- Unknown: 0.20
```

Because confidence is low, Translation should avoid applying a highly specific Character Speech Profile automatically.

The result may be marked for later speaker review.

---

# User Correction Example

```text
Detected Speaker:
Character A

Generated Translation:
“Tôi không đồng ý.”

User Correction:
Speaker is Character B
Translation:
“Bổn cung không đồng ý.”
```

Possible consequences:

1. Create a new Speaker Attribution revision
2. Create a new Translation Revision
3. Revalidate Character B Speech Profile
4. Preserve the original generated result
5. Create a Character context learning candidate

---

# Character Merge Example

```text
Character CH-012:
Masked Swordsman

Character CH-031:
General Lý

Chapter 70 reveals:
They are the same person.
```

Merge policy may:

* Preserve both historical identities
* Keep separate scoped personas
* Select one surviving Character ID
* Record the reveal boundary
* Prevent future context from leaking into earlier chapters

A simple destructive merge would be insufficient.

---

# Architecture Invariants

1. Character is an Aggregate Root with stable identity.
2. Character ID is separate from Character Revision ID.
3. Referenced Character Revisions are immutable.
4. Character identity is independent of any one name.
5. Original, translated, localized, transliterated and romanized names remain distinguishable.
6. Names are language- and script-aware.
7. Name preference may be scope-specific.
8. Future names and identities must respect spoiler boundaries.
9. Character metadata does not depend on AI provider formats.
10. Character recognition execution belongs outside the Character aggregate.
11. Face embeddings are derived infrastructure data.
12. Character Candidates are not canonical Characters.
13. AI observations do not become domain truth without promotion or approval.
14. Speaker Attribution is separate from Character identity.
15. Low-confidence speaker attribution must preserve uncertainty.
16. Unknown Speaker is distinct from Narrator.
17. Pronoun rules may depend on speaker-listener relationships.
18. A global gender field is insufficient for Vietnamese address resolution.
19. Character and Glossary have separate ownership responsibilities.
20. Glossary may reference Character identity but does not own it.
21. Character may reference Glossary Entries but does not own terminology rules.
22. Relationships are directional where domain meaning requires it.
23. Relationship history is scope- and revision-aware.
24. Character Context Snapshots reference exact Character Revisions.
25. Translation Revisions preserve their original Character Context Snapshot.
26. Later Character edits do not rewrite historical Translation context.
27. Context construction must respect story and spoiler scope.
28. Locked Character data cannot be overwritten by ordinary imports or AI suggestions.
29. User Translation corrections do not automatically mutate Character truth.
30. Character merge preserves historical references and lineage.
31. Character split preserves original lineage and evidence.
32. Referenced Characters cannot normally be hard deleted.
33. Derived search and recognition indexes are rebuildable.
34. Cache keys use revisioned Character context rather than mutable IDs alone.
35. Character changes invalidate only affected downstream artifacts.
36. Character data must not leak across Projects.
37. Character recognition must not be repurposed as real-person biometric identification.
38. Every authoritative Character change is auditable.

---

# Open Decisions

The following decisions should remain open until implementation and prototype testing:

* Whether Character is Project-level or Book-level by default
* Whether one Character may belong to several Books in a series
* Whether Character Revision contains all fields or field-level revisions
* Whether names are separate entities or revision-owned value objects
* Whether aliases belong primarily to Character or Glossary
* Whether titles are Character metadata, Glossary Entries or both
* How Vietnamese pronoun rules are represented
* Whether relationship-specific address rules are stored in Character or Relationship
* Whether relationships form a separate Aggregate Root
* How spoiler boundaries are represented
* Whether future information is physically separated from current context
* How alternate personas are modeled
* How reincarnation and body-swap identities are modeled
* Whether masked or disguised identities use separate Character IDs
* Whether Narrator is always a Character
* Whether System Voice is always a Character
* How group speakers are represented
* Which Character Types are supported in MVP
* Whether age and gender metadata are needed
* How uncertain gender information is represented
* Whether character description text is included in AI context
* How much Character context is sent to providers
* Whether context uses structured JSON or compiled natural language
* How speaker candidates are ranked
* Whether low-confidence speaker attribution blocks Translation
* Whether comic Bubble Tail detection is part of MVP
* Whether visual Character recognition runs locally
* Whether face embeddings are persisted
* How reference images are selected and retained
* Whether Character Candidates are a separate aggregate
* Whether repeated names automatically create candidates
* How duplicate Characters are detected
* Whether merges create redirects or one shared identity with personas
* How Character split affects historical Translation records
* Whether Character corrections automatically trigger retranslation
* How Character Context Snapshot size is limited
* How chapter-cast relevance is scored
* How corrections are converted into learning candidates
* Which import formats are supported
* How publisher character sheets are mapped
* Whether Character exports support spoiler-safe filtering
* How Character data is synchronized between Projects in a series
* Whether real-person images are explicitly prohibited from recognition workflows

---

# Recommended MVP Scope

The first CRAI MVP should support:

* Project-level Character aggregate
* Stable Character identity
* Immutable Character Revisions
* Original name
* Preferred Vietnamese name
* Romanized name
* Aliases
* Basic Character Types
* Basic titles
* Basic gender state including Unknown
* Character notes
* Review state
* Approved and locked names
* Chapter-range scope
* Spoiler reveal boundary
* Character references from Glossary Entries
* Manual speaker assignment
* Optional AI speaker candidates
* Speaker Attribution confidence
* Unknown Speaker
* Narrator
* Basic Character relationships
* Vietnamese self-reference notes
* Relationship-specific address notes
* Basic Speech Profile
* Character Context Snapshot
* Translation context integration
* Character-aware validation
* Character merge
* Basic Character Candidate workflow
* JSON import and export
* Audit events
* Selective Translation staleness

The MVP may defer:

* Face recognition
* Persistent face embeddings
* Automatic visual tracking across Pages
* Body and soul identity modeling
* Reincarnation modeling
* Complex persona inheritance
* Real-time collaborative Character editing
* Semantic relationship extraction
* Automatic pronoun learning
* Full relationship graph queries
* Detailed age inference
* Voice recognition
* Global cross-Project Characters
* Series-level identity synchronization
* Advanced spoiler permissions
* Automatic dialogue attribution from bubble tails
* Automated Character merge
* Field-level approval inheritance
* Complex localized naming by region
* Character analytics
* External wiki synchronization
* Advanced group-speaker modeling

---

# Related Documents

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
* `docs/architecture/CAPABILITY_MAP.md`
* `docs/architecture/DATA_FLOW.md`
* `docs/architecture/STATE_MACHINE.md`
* `docs/architecture/EVENT_BUS.md`
* `docs/architecture/MODULE_DEPENDENCY.md`
* `docs/architecture/ai/PIPELINE.md`
* `docs/architecture/ai/CONTEXT.md`
* `docs/architecture/ai/MEMORY.md`
* `docs/architecture/ai/PROMPTS.md`
* `docs/architecture/ai/REQUEST.md`
* `docs/architecture/ai/RESPONSE.md`
* `docs/architecture/ai/ROUTING.md`
* `docs/architecture/ai/CACHE.md`
* `docs/architecture/presentation/LAYOUT.md`
* `docs/architecture/presentation/TYPOGRAPHY.md`
