# Glossary Domain

* **Document:** Domain / Glossary
* **Version:** 1.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

The Glossary domain defines how CRAI manages terminology that should be translated, preserved, normalized or presented consistently across a Project.

A glossary may contain:

* Character names
* Place names
* Organization names
* Skill names
* Item names
* Titles
* Ranks
* Technical terms
* Fictional concepts
* Idioms
* Honorifics
* Sound effects
* Repeated phrases
* Terms that must remain untranslated

The Glossary domain provides stable terminology rules to OCR correction, Translation, validation, review and presentation workflows.

Its primary goal is consistency without coupling domain truth to a particular AI provider, prompt format or matching engine.

---

# Domain Role

Glossary acts as a controlled terminology source.

```text
Project
   │
   ├── Glossary
   │      ├── Entries
   │      ├── Revisions
   │      ├── Scopes
   │      └── Policies
   │
   ▼
Translation Context Snapshot
   │
   ▼
Provider Request
   │
   ▼
Translation Revision
   │
   ▼
Terminology Validation
```

Glossary influences Translation.

It does not own Translation.

A Translation Revision records which Glossary Revision or Glossary Snapshot influenced its result.

---

# Ownership Boundary

Glossary should be modeled as an Aggregate Root.

```text
Glossary Aggregate
├── Glossary
├── Glossary Revision
├── Glossary Entry
├── Entry Revision
├── Term Form
├── Translation Rule
├── Applicability Scope
├── Matching Policy
├── Presentation Policy
└── Review State
```

The Glossary aggregate owns:

* Glossary identity
* Glossary metadata
* Glossary-level configuration
* Entry identity
* Entry revision history
* Entry activation state
* Scope and precedence rules
* Approval state
* Conflict metadata
* Publication state

The aggregate does not own:

* Translation results
* Character aggregates
* Text Blocks
* OCR results
* AI prompts
* Provider execution data
* Search indexes
* Embedding vectors
* Runtime matching caches

Those are referenced or derived artifacts.

---

# Responsibilities

The Glossary domain is responsible for:

* Defining canonical terminology entries
* Defining source and target language scope
* Maintaining terminology revisions
* Preserving user-approved terminology
* Defining term matching behavior
* Defining replacement or preservation rules
* Managing precedence between entries
* Detecting domain-level conflicts
* Supporting glossary snapshots
* Supporting import and export
* Supporting review and publication
* Emitting glossary change events
* Participating in Translation staleness decisions

The Glossary domain is not responsible for:

* Scanning raw text
* Performing linguistic tokenization
* Detecting entities automatically
* Constructing prompts
* Calling translation providers
* Replacing text directly inside source content
* Rendering translated terms
* Training terminology models
* Generating embeddings

These belong to application, AI, language-processing and infrastructure capabilities.

---

# Aggregate Structure

Recommended conceptual structure:

```text
Glossary
├── Glossary ID
├── Project ID
├── Name
├── Description
├── Default Source Language Range
├── Default Target Language Range
├── Status
├── Active Revision
├── Entries
├── Policies
├── Created At
├── Updated At
└── Version
```

Each entry has its own stable identity and revision history.

```text
Glossary Entry
├── Entry ID
├── Entry Type
├── Active Revision
├── Status
├── Priority
├── Review State
└── Lineage
```

The current textual and behavioral definition belongs to the Entry Revision.

---

# Glossary Identity

Glossary identity must remain stable across edits.

```text
Glossary ID != Glossary Revision
```

Changing the Glossary name, description, policies or entries does not create a new Glossary identity.

A new Glossary should only be created when terminology belongs to a meaningfully separate collection or ownership boundary.

Examples:

* One Project-wide glossary
* A glossary imported from a publisher
* A shared language glossary
* A user-specific terminology collection
* A temporary experiment glossary

---

# Glossary Revision

Glossary Revision represents a stable, immutable view of the Glossary at a point in time.

A revision may include:

* Glossary metadata
* Glossary policies
* Included Entry Revisions
* Entry ordering
* Entry activation state
* Publication state
* Revision author
* Revision reason
* Creation time

Recommended structure:

```text
Glossary Revision
├── Revision ID
├── Glossary ID
├── Revision Number
├── Entry Revision References
├── Policy Snapshot
├── Parent Revision
├── Created By
├── Created At
├── Change Reason
└── Content Hash
```

Once published or referenced by a Translation Snapshot, a Glossary Revision must be immutable.

---

# Glossary Entry

A Glossary Entry defines one terminology concept or terminology rule.

Examples:

```text
灵力 → linh lực
李青 → Lý Thanh
宗主 → tông chủ
Ultimate Skill → preserve
```

An entry is not merely a pair of strings.

It may include:

* Several source forms
* Several target forms
* Contextual restrictions
* Matching rules
* Grammatical notes
* Character or entity references
* Presentation rules
* Review information
* Alternative translations

---

# Entry Identity

Each Glossary Entry has a stable identifier.

```text
Entry ID != Entry Revision ID
```

The Entry ID represents the continuing terminology concept.

The Entry Revision represents one immutable definition of that concept.

A spelling correction normally creates a new Entry Revision.

A completely different concept should receive a new Entry ID.

---

# Entry Revision

Recommended structure:

```text
Glossary Entry Revision
├── Entry Revision ID
├── Entry ID
├── Source Forms
├── Target Forms
├── Source Language Range
├── Target Language Range
├── Entry Type
├── Rule Type
├── Matching Policy
├── Applicability Scope
├── Priority
├── Notes
├── References
├── Review State
├── Parent Revision
├── Created By
├── Created At
└── Content Hash
```

Entry revisions are immutable.

Editing an entry creates a new revision and updates its active revision reference.

---

# Entry Types

Glossary entries should be classified by semantic role.

Recommended entry types:

* Character Name
* Alias
* Place Name
* Organization
* Faction
* Species
* Race
* Skill
* Technique
* Ability
* Item
* Weapon
* Artifact
* Rank
* Realm
* Title
* Honorific
* Relationship Term
* Technical Term
* Cultural Term
* Idiom
* Repeated Phrase
* Sound Effect
* Measurement
* General Term
* Preserve Rule
* Forbidden Translation
* Custom

Entry type helps:

* Context construction
* Conflict resolution
* Validation
* Presentation
* Import and export
* User interface grouping

Entry type must not determine Translation behavior by itself.

Behavior is defined by the Rule Type and related policies.

---

# Source Form

A Source Form is a term representation that may occur in source content.

Recommended structure:

```text
Source Form
├── Text
├── Language
├── Script
├── Form Type
├── Normalized Form
├── Match Policy
├── Case Sensitivity
├── Boundary Policy
└── Status
```

A single entry may have multiple Source Forms.

Example:

```text
Entry: Character / Li Qing

Source Forms:
- 李青
- 李靑
- Li Qing
- Lǐ Qīng
```

All forms point to one terminology concept.

---

# Source Form Types

Possible Source Form types include:

* Canonical
* Alias
* Abbreviation
* Alternate Spelling
* Historical Form
* Simplified Form
* Traditional Form
* Romanization
* Transliteration
* OCR Variant
* Common Error
* Inflected Form
* User Alias
* Imported Alias

The canonical source form should be identifiable.

OCR variants and common errors must not silently replace canonical source text.

They exist only to improve recognition and matching.

---

# Target Form

A Target Form defines an allowed or preferred target-language representation.

Recommended structure:

```text
Target Form
├── Text
├── Language
├── Script
├── Form Type
├── Preference Rank
├── Style Scope
├── Presentation Notes
├── Approval State
└── Status
```

A single entry may have multiple target forms.

Example:

```text
Source: 宗主

Target Forms:
1. tông chủ
2. chưởng môn
3. giáo chủ
```

The preferred form may depend on context, genre or faction type.

---

# Target Form Types

Possible Target Form types include:

* Preferred Translation
* Approved Alternative
* Literal Translation
* Localized Translation
* Transliterated Form
* Romanized Form
* Preserve Original
* Abbreviation
* Display Alias
* Deprecated Translation
* Forbidden Translation

Only approved and applicable forms may be included in a published Glossary Snapshot.

---

# Rule Type

Rule Type describes how Translation should treat the matched concept.

Recommended values:

* Translate
* Preserve
* Transliterate
* Romanize
* Normalize
* Prefer
* Avoid
* Forbid
* Contextual
* Informational

## Translate

Use the selected target form as the preferred semantic translation.

```text
灵力 → linh lực
```

## Preserve

Keep the original term unchanged.

```text
Skill Burst → Skill Burst
```

## Transliterate

Convert the source writing system while preserving pronunciation.

```text
李青 → Lý Thanh
```

## Romanize

Convert into a selected Latin romanization system.

```text
東京 → Tōkyō
```

## Normalize

Normalize several source variants into one target representation.

```text
HP
Hit Point
Health Point
→ HP
```

## Prefer

Encourage a translation but do not require an exact output.

## Avoid

Discourage a target form while allowing contextual exceptions.

## Forbid

A target form must not appear in a valid Translation.

## Contextual

The rule requires context to choose between target forms.

## Informational

Provide context to the model or reviewer without imposing replacement behavior.

---

# Canonical Concept

Several Source Forms and Target Forms may represent one canonical concept.

```text
Canonical Concept
├── Entry ID
├── Entry Type
├── Canonical Source Form
├── Preferred Target Form
├── Aliases
├── External References
└── Domain References
```

The canonical concept allows CRAI to preserve consistency even when text contains:

* Alternate names
* Nicknames
* Abbreviations
* Script variants
* OCR errors
* Different romanizations

---

# Language Scope

Every entry must declare its language applicability.

Recommended scope:

```text
Language Scope
├── Source Language Range
├── Target Language Range
├── Source Script
├── Target Script
└── Language Pair Restrictions
```

Example:

```text
Source: zh-Hans
Target: vi
```

An entry scoped to `zh-Hans → vi` must not automatically apply to:

```text
ja → vi
en → vi
vi → en
```

Language ranges may be used when exact specificity is unnecessary.

---

# Script Scope

Script scope is separate from base language.

Example:

```text
Source Language: zh
Source Script: Hans
```

This rule may apply to Simplified Chinese but not Traditional Chinese.

Script-specific source forms should be explicit where conversion can change meaning or matching behavior.

---

# Project Scope

A Glossary normally belongs to one Project.

Possible scope levels include:

* Global CRAI
* User
* Workspace
* Project
* Book
* Chapter
* Page
* Session
* Translation operation

Recommended precedence:

```text
Operation Override
        ↓
Session Glossary
        ↓
Page Scope
        ↓
Chapter Scope
        ↓
Book Scope
        ↓
Project Scope
        ↓
User Scope
        ↓
Workspace Scope
        ↓
Global Scope
```

More specific scope has higher precedence unless an entry is explicitly locked.

---

# Applicability Scope

An Entry may be restricted beyond language.

Possible restrictions:

* Project
* Book
* Chapter range
* Chapter
* Page range
* Page
* Character
* Faction
* Narrative arc
* Content type
* Translation profile
* Genre
* Source provider
* Import source
* User
* Session

Recommended structure:

```text
Applicability Scope
├── Project ID
├── Book IDs
├── Chapter IDs
├── Page Range
├── Entity References
├── Content Types
├── Translation Profile IDs
└── Exclusions
```

Scope restrictions must be deterministic and serializable.

---

# Inclusion and Exclusion

An entry may define:

* Included scopes
* Excluded scopes

Example:

```text
Term: 王

Default Translation:
- vương

Exception:
- preserve as Wang when used as a surname
```

Another example:

```text
Term: Master

Default:
- sư phụ

Excluded Context:
- game rank title
```

Exclusion rules should take precedence over broad inclusion rules at equal specificity.

---

# Context Conditions

Some terminology cannot be resolved from string matching alone.

Context conditions may include:

* Speaker
* Character
* Entity type
* Surrounding terms
* Part of speech
* Dialogue versus narration
* Chapter range
* Faction
* Gender
* Formality
* World-building context
* Grammatical role

Example:

```text
小姐
```

may mean:

* young lady
* miss
* a title
* a form of address

The Glossary may define alternatives and context notes.

It should not claim deterministic resolution when the context is insufficient.

---

# Character Association

A Glossary Entry may reference a Character aggregate.

```text
Glossary Entry
    │
    └── Character Reference
```

Examples:

* Original name
* Vietnamese name
* Alias
* Title
* Nickname

The Glossary does not own the Character.

Character identity belongs to the Character domain.

Glossary stores references and translation-specific forms.

---

# Entity Association

Entries may reference other domain entities:

* Character
* Place
* Organization
* Item
* Skill
* Species
* Rank
* Book-specific concept

Entity references improve consistency and disambiguation.

Deleting an external entity must not corrupt historical Glossary Revisions.

Historical revisions should preserve immutable references or snapshots.

---

# Matching Policy

Matching Policy defines how a Source Form may be recognized.

Possible properties:

```text
Matching Policy
├── Match Type
├── Case Sensitivity
├── Unicode Normalization
├── Boundary Policy
├── Script Conversion Policy
├── Punctuation Policy
├── Whitespace Policy
├── Inflection Policy
├── OCR Tolerance
└── Confidence Threshold
```

The policy describes intent.

The actual matching algorithm belongs to a matching capability.

---

# Match Types

Recommended match types:

* Exact
* Normalized Exact
* Whole Word
* Phrase
* Prefix
* Suffix
* Substring
* Regular Expression
* Token Sequence
* Morphological
* Fuzzy
* Semantic
* Entity Linked

Not all match types should be enabled in the MVP.

Exact and normalized phrase matching should be preferred because they are deterministic.

---

# Exact Matching

Exact matching requires the source text to equal the Source Form according to the declared normalization policy.

Example:

```text
Source Form: 灵力
Input: 灵力
Result: match
```

Exact matching is predictable and should have high precedence.

---

# Normalized Matching

Normalized matching may account for:

* Unicode normalization
* Full-width and half-width forms
* Repeated whitespace
* Case normalization
* Punctuation normalization
* Simplified formatting differences

Normalization must not perform uncontrolled semantic transformation.

The original text and normalized match value should both be preserved in match results.

---

# Boundary Policy

Boundary Policy controls whether a term may match inside a larger string.

Possible values:

* Exact Text
* Whole Token
* Word Boundary
* Character Boundary
* Phrase Boundary
* Any Position
* Language Specific

Boundary behavior must account for languages without whitespace word separation.

A Latin word-boundary rule is not valid for all Chinese, Japanese or Thai content.

---

# Case Sensitivity

Case sensitivity is primarily relevant to scripts with letter case.

Possible values:

* Sensitive
* Insensitive
* Language Default

Case-insensitive matching should use language-aware normalization where necessary.

---

# Fuzzy Matching

Fuzzy matching may support:

* OCR mistakes
* Missing diacritics
* Character confusion
* Minor spelling errors
* Similar romanizations

Fuzzy matching must produce confidence metadata.

It must not directly enforce a mandatory replacement without additional validation.

A low-confidence fuzzy match should normally be used as a suggestion.

---

# OCR Variant Matching

OCR can produce repeated recognition errors.

An Entry may include OCR variants:

```text
Canonical: 修炼
OCR Variants:
- 修練
- 体炼
- 修炼.
```

OCR variants are matching aids.

They must not be published as preferred source terminology.

Adding an OCR variant should preserve:

* Original observed text
* Occurrence evidence
* Source Page or Text Block references
* Confidence
* Reviewer state

---

# Semantic Matching

Semantic matching may identify terminology even when text differs significantly.

Examples:

* Abbreviated skill names
* Descriptive references
* Pronouns linked to named entities
* Translated aliases

Semantic matching is probabilistic.

It should be treated as:

* Candidate generation
* Context enrichment
* Review assistance

It should not silently override exact approved terminology.

---

# Matching Result

A runtime terminology match should be represented separately from the Glossary Entry.

Recommended structure:

```text
Glossary Match
├── Match ID
├── Entry ID
├── Entry Revision ID
├── Source Form ID
├── Scope
├── Text Block ID
├── Text Block Revision
├── Start Offset
├── End Offset
├── Original Text
├── Normalized Text
├── Match Type
├── Confidence
├── Resolution State
└── Matcher Revision
```

Glossary Match is derived data.

It is not part of the Glossary aggregate.

---

# Matching Resolution

Possible resolution states:

* Accepted
* Rejected
* Ambiguous
* Shadowed
* Overridden
* Conflicted
* Suggested
* Expired

A match may be shadowed by a more specific or higher-priority entry.

---

# Priority

Priority helps resolve competing entries.

Recommended priority dimensions:

1. Explicit operation override
2. Scope specificity
3. Approval level
4. Exactness of language match
5. Match type strength
6. Entry priority
7. Revision recency
8. Stable tie-breaker

Priority must be deterministic.

Revision recency alone should not override an approved and more specific term.

---

# Specificity

Possible specificity ranking:

```text
Exact Page
    >
Chapter
    >
Book
    >
Project
    >
User
    >
Workspace
    >
Global
```

Language specificity may also be evaluated:

```text
zh-Hans-CN
    >
zh-Hans
    >
zh
    >
und
```

A more specific entry may override a broader entry when both apply.

---

# Precedence Resolution

Recommended resolution flow:

```text
Collect Applicable Entries
        │
        ▼
Filter by Language Pair
        │
        ▼
Filter by Scope
        │
        ▼
Evaluate Match Policies
        │
        ▼
Rank by Specificity
        │
        ▼
Apply Explicit Priority
        │
        ▼
Detect Conflicts
        │
        ▼
Produce Resolved Glossary Match
```

The resolution result should record why one entry won.

---

# Conflict

A Glossary Conflict occurs when multiple active entries define incompatible behavior for the same effective context.

Examples:

```text
Entry A:
灵力 → linh lực

Entry B:
灵力 → linh khí
```

or:

```text
Entry A:
Skill Burst → preserve

Entry B:
Skill Burst → Kỹ năng Bộc Phát
```

Conflicts should not be silently resolved when priority is equal and intent differs.

---

# Conflict Types

Recommended conflict types:

* Duplicate Source Form
* Conflicting Target Form
* Preserve versus Translate
* Forbidden versus Preferred
* Overlapping Phrase
* Scope Collision
* Language Scope Collision
* Alias Collision
* Entity Identity Collision
* Circular Reference
* Incompatible Lock
* Ambiguous Context Rule

---

# Overlapping Terms

Glossary terms may overlap.

Example:

```text
天剑
天剑宗
剑宗
```

The matcher should normally prefer the longest applicable match, subject to priority and scope.

However, longest-match behavior is a matching policy, not an aggregate invariant.

Overlapping entries should be detectable during validation.

---

# Duplicate Entries

Two entries are potential duplicates when they share:

* Equivalent source forms
* Compatible language scope
* Overlapping applicability scope
* Similar rule type
* Same referenced concept

Potential duplicates may still be valid when they differ by:

* Character context
* Book
* Chapter
* Meaning
* Part of speech
* Translation profile

Duplicate detection should create review candidates rather than automatically merging entries.

---

# Merge

Glossary entries may be merged when they represent the same concept.

Merge operation should:

* Select a surviving Entry ID
* Preserve all historical revisions
* Move or copy compatible source forms
* Move or copy target forms
* Preserve aliases
* Preserve entity references
* Record lineage
* Mark merged entries as redirected
* Emit an event

Translations referencing old Entry Revisions must remain valid.

---

# Split

An entry may be split when one term was incorrectly used for several concepts.

Example:

```text
Master
```

may need separate entries for:

* Teacher
* Rank
* Owner
* System controller

Split operation should create new Entry identities and preserve lineage from the original entry.

Historical references must remain resolvable.

---

# Entry Status

Recommended entry statuses:

* Draft
* Active
* Inactive
* Deprecated
* Rejected
* Merged
* Archived

## Draft

The entry is editable and not included in published snapshots by default.

## Active

The entry may participate in terminology resolution.

## Inactive

The entry remains stored but is excluded from new snapshots.

## Deprecated

The entry should not be used for new Translations but remains available for history.

## Rejected

The entry was reviewed and intentionally denied.

## Merged

The entry has been consolidated into another Entry.

## Archived

The entry is retained only for historical or audit purposes.

---

# Review State

Review state is separate from entry lifecycle state.

Recommended review states:

* Unreviewed
* Suggested
* Under Review
* Approved
* Needs Changes
* Rejected
* Locked

An Active entry may still be unreviewed if project policy allows provisional terminology.

A Locked entry cannot be changed without explicit elevated action.

---

# Approval

Approval should record:

* Reviewer
* Approved Entry Revision
* Scope
* Approval time
* Review notes
* Source evidence
* Approval policy

Approval applies to an exact Entry Revision.

Editing an approved entry creates a new unapproved revision unless policy allows inherited approval for non-semantic metadata changes.

---

# Locked Terminology

A locked terminology rule represents a highly authoritative decision.

Examples:

* Main character names
* Publisher-approved names
* Official skill names
* Legal or trademarked terms
* User-pinned translations

Locked entries should:

* Have high precedence
* Require explicit permission to edit
* Produce validation errors when violated
* Never be silently overwritten by imported data
* Remain traceable to their authority

Locking must be scoped.

A Project lock must not automatically become a global lock.

---

# Suggested Entry

Glossary candidates may be generated from:

* Repeated source terms
* Named entity recognition
* User corrections
* Translation differences
* Character extraction
* Import
* AI suggestions
* Terminology validation failures

A generated suggestion is not an approved Glossary Entry.

Recommended candidate structure:

```text
Glossary Candidate
├── Candidate ID
├── Proposed Source Forms
├── Proposed Target Forms
├── Language Pair
├── Occurrence Count
├── Evidence References
├── Confidence
├── Generator
├── Generator Revision
└── Review State
```

Candidates belong to a suggestion or review workflow.

They should not automatically affect Translation.

---

# Learning from User Corrections

A user correction may suggest a new or updated glossary term.

Example:

```text
Generated Translation:
linh khí

User Correction:
linh lực
```

The application may propose:

```text
灵力 → linh lực
```

The system must not automatically create or activate the entry unless project policy explicitly permits it.

User correction remains authoritative for the corrected Translation Revision even if the glossary proposal is rejected.

---

# Glossary Snapshot

A Translation operation should use an immutable Glossary Snapshot.

Recommended structure:

```text
Glossary Snapshot
├── Snapshot ID
├── Project ID
├── Language Pair
├── Source Glossary Revisions
├── Included Entry Revisions
├── Resolution Policy Revision
├── Created At
└── Content Hash
```

The snapshot should include only entries that are:

* Active
* Applicable
* Language-compatible
* Allowed by review policy
* Not shadowed
* Not unresolved conflicts

---

# Snapshot Identity

Snapshot identity must be content-addressable or otherwise reproducible.

Two snapshots with identical:

* Entry Revisions
* Resolution policy
* Scope
* Language pair

should produce the same semantic content hash.

The snapshot hash may participate in:

* Translation configuration hash
* Cache key
* Audit lineage
* Staleness detection

---

# Snapshot Immutability

Once referenced by a Translation Revision, a Glossary Snapshot must be immutable.

Later Glossary edits create a new snapshot.

Historical Translation records continue referencing the old snapshot.

This prevents terminology changes from rewriting Translation history.

---

# Translation Integration

Translation should consume glossary context through a snapshot.

```text
Translation Request
├── Source Text
├── Language Pair
├── Translation Profile
├── Context Snapshot
└── Glossary Snapshot
```

The Translation domain records:

* Glossary Snapshot ID
* Glossary Revision references
* Applied Entry Revision references
* Terminology validation results

The Translation aggregate must not embed the entire mutable Glossary.

---

# Prompt Integration

Glossary-to-prompt conversion belongs to the AI Prompt or Context capability.

Conceptual flow:

```text
Glossary Snapshot
        │
        ▼
Context Compiler
        │
        ▼
Provider-Neutral Terminology Instructions
        │
        ▼
Provider Adapter
        │
        ▼
Provider-Specific Prompt
```

Glossary entries must not store provider-specific prompt fragments as canonical business data.

Optional human-written notes may be stored, but providers receive compiled representations.

---

# Context Budget

Large glossaries may exceed provider context limits.

The context compiler may select entries based on:

* Exact matches in source text
* Nearby chapter usage
* Character presence
* Scope
* Priority
* Approval level
* Semantic relevance
* Recent usage
* Context budget

Selection must be traceable.

The Translation result should record which Entry Revisions were actually supplied or applied.

---

# Applied Glossary Entry

An Applied Glossary Entry records terminology used during a Translation.

Recommended structure:

```text
Applied Glossary Entry
├── Entry ID
├── Entry Revision ID
├── Source Form ID
├── Target Form ID
├── Match Reference
├── Application Type
├── Confidence
└── Validation Result
```

Application types may include:

* Prompt Supplied
* Provider Applied
* Post-Processed
* User Confirmed
* Validation Only
* Ignored
* Conflicted

Provider compliance must not be assumed solely because an entry appeared in the prompt.

---

# Terminology Validation

Generated Translation should be validated against the applicable Glossary Snapshot.

Validation may check:

* Required target form is present
* Forbidden target form is absent
* Preserve rule was followed
* Character names are consistent
* Repeated terms use consistent output
* Script policy was followed
* Unapproved alternative was used
* Locked rule was violated
* Ambiguous rule requires review

Validation should produce structured findings.

---

# Validation Finding

Recommended structure:

```text
Terminology Validation Finding
├── Finding ID
├── Translation Revision ID
├── Entry Revision ID
├── Severity
├── Finding Type
├── Source Range
├── Target Range
├── Expected Forms
├── Observed Form
├── Confidence
├── Resolution State
└── Validator Revision
```

Severity may include:

* Information
* Warning
* Error
* Blocking

---

# Validation Types

Recommended terminology findings:

* Required Term Missing
* Forbidden Term Used
* Preserve Rule Violated
* Wrong Target Form
* Inconsistent Translation
* Unapproved Alternative
* Ambiguous Match
* Language Scope Mismatch
* Entity Mismatch
* Capitalization Mismatch
* Script Mismatch
* Partial Phrase Replacement
* Locked Entry Violation

---

# Automatic Post-Processing

Some glossary rules may be enforced after provider generation.

Examples:

* Correcting a character name
* Replacing a forbidden variant
* Restoring preserved Latin text
* Normalizing capitalization

Automatic post-processing must be conservative.

It should only run when:

* The match is deterministic
* The source mapping is known
* The replacement cannot corrupt grammar
* The applied rule allows post-processing
* Offsets or structured output are reliable

Every automatic change must be recorded.

---

# Post-Processing Record

Recommended structure:

```text
Glossary Post-Processing Action
├── Action ID
├── Entry Revision ID
├── Before Text
├── After Text
├── Target Range
├── Rule
├── Confidence
├── Processor Revision
└── Created At
```

Post-processing creates or contributes to a new Translation Revision.

It must not mutate the provider output in place.

---

# Glossary and Translation Staleness

A Glossary change may make a Translation stale.

Possible impact classifications:

* No Impact
* Metadata Only
* Review Recommended
* Validation Required
* Retranslation Recommended
* Retranslation Required

Examples:

| Glossary Change               | Typical Impact                   |
| ----------------------------- | -------------------------------- |
| Entry description changed     | No Impact                        |
| Display order changed         | No Impact                        |
| New unrelated entry added     | No Impact                        |
| Matching alias added          | Validation may be required       |
| Preferred target changed      | Retranslation recommended        |
| Locked character name changed | Retranslation required           |
| Forbidden form added          | Validation required              |
| Entry deactivated             | Review recommended               |
| Language scope corrected      | Depends on affected translations |

Impact should be calculated only for Translations whose source or applied terminology may intersect the changed entry.

---

# Affected Translation Detection

Possible evidence sources:

* Applied Entry references
* Glossary Match references
* Source text index
* Source hash
* Target text index
* Entity references
* Chapter scope
* Translation Context Snapshot

The system should avoid marking every Project Translation stale when one unrelated term changes.

---

# Source Hash and Configuration Hash

Glossary snapshots participate in Translation reproducibility.

Recommended Translation configuration inputs include:

```text
Source Hash
+
Language Pair
+
Translation Profile Revision
+
Context Snapshot Hash
+
Glossary Snapshot Hash
+
Prompt Revision
```

Changing the Glossary Snapshot changes the Translation configuration identity.

---

# Import

Glossary may be imported from:

* CSV
* TSV
* JSON
* YAML
* TMX
* TBX
* Spreadsheet
* Another CRAI Project
* Publisher terminology
* AI-generated suggestion set
* User-maintained dictionary

Import must create a reviewable import plan before modifying active terminology.

---

# Import Plan

Recommended structure:

```text
Glossary Import Plan
├── Import ID
├── Source Format
├── Source Hash
├── Proposed New Entries
├── Proposed Updates
├── Potential Duplicates
├── Conflicts
├── Invalid Rows
├── Language Mapping
├── Scope Mapping
└── Review State
```

Import should not silently overwrite approved entries.

---

# Import Conflict Resolution

Possible actions:

* Create New Entry
* Create New Revision
* Add Source Alias
* Add Target Alternative
* Ignore
* Merge
* Replace Draft
* Keep Existing
* Require Manual Review

Locked entries cannot be replaced by ordinary import.

---

# Export

Glossary export may support:

* Full Glossary
* Active entries only
* Approved entries only
* Selected language pair
* Selected scope
* Selected Book or Chapter
* Revision snapshot
* Provider-compatible format
* Human review format

Provider-compatible export is derived output.

It is not the canonical persisted representation.

---

# Round-Trip Export

A CRAI-native export should preserve:

* Glossary IDs
* Entry IDs
* Entry Revision IDs
* Language scope
* Applicability scope
* Rule types
* Matching policies
* Review states
* Lineage
* External references
* Content hashes

Simple CSV exports may intentionally lose advanced metadata.

The export format should declare its fidelity level.

---

# Versioning

Versioned concepts include:

* Glossary Revision
* Entry Revision
* Glossary Snapshot
* Matching Policy Revision
* Resolution Policy Revision
* Import Format Version
* Export Format Version
* Validation Rule Revision

Historical records must remain interpretable after policies evolve.

---

# Change Types

Recommended entry change classifications:

* Metadata Change
* Source Form Added
* Source Form Removed
* Target Form Added
* Preferred Target Changed
* Rule Type Changed
* Scope Changed
* Language Scope Changed
* Priority Changed
* Review State Changed
* Lock Changed
* Entity Reference Changed
* Entry Merged
* Entry Split
* Entry Deprecated

Change classification helps determine downstream impact.

---

# Concurrency

Concurrent Glossary edits may occur.

Optimistic concurrency should use:

* Glossary version
* Entry active revision
* Expected parent revision
* Content hash

Two users editing the same Entry Revision should not silently overwrite each other.

Possible outcomes:

* Automatic merge for non-conflicting metadata
* New parallel draft revisions
* Manual conflict resolution
* Rejection due to stale version

---

# Idempotency

Glossary operations should support idempotency where applicable.

Examples:

* Importing the same source file
* Creating a candidate from the same evidence
* Approving an already-approved revision
* Publishing an unchanged snapshot
* Applying an identical alias update

Idempotency may use:

* Operation key
* Source hash
* Entry content hash
* Import ID
* Revision parent

---

# Deletion

Hard deletion should be exceptional.

Entries referenced by:

* Translation Revisions
* Glossary Snapshots
* Review records
* Import history
* Audit events

must not be physically deleted under normal operations.

Preferred operations:

* Inactivate
* Deprecate
* Archive
* Merge
* Redirect

Hard deletion may be allowed only for unreferenced drafts or legal privacy requirements.

---

# Retention

The system should retain:

* Published Glossary Revisions
* Referenced Entry Revisions
* Referenced Glossary Snapshots
* Approval records
* Merge and split lineage
* Import provenance
* Conflict decisions

Unreferenced temporary candidates and obsolete search indexes may follow shorter retention policies.

---

# Persistence

Recommended persistence separation:

```text
Glossary
Glossary Revision
Glossary Entry
Glossary Entry Revision
Glossary Snapshot
Glossary Snapshot Entry
Glossary Review
Glossary Conflict
Glossary Import
Glossary Candidate
```

Derived runtime structures may include:

```text
Glossary Search Index
Glossary Matching Index
Glossary Embedding Index
Glossary Usage Index
```

Derived indexes must be rebuildable from canonical records.

---

# Search Index

Glossary search may index:

* Source forms
* Target forms
* Aliases
* Notes
* Entity names
* Language tags
* Entry types
* Scopes

Search index contents are derived.

Index failure must not corrupt Glossary truth.

---

# Matching Index

A matching index may optimize:

* Exact term lookup
* Prefix matching
* Longest phrase matching
* Script-aware matching
* OCR variants
* Language pair filtering

The index must identify the exact Entry Revision from which every rule originated.

---

# Cache Participation

Glossary-related cache keys may include:

* Glossary Snapshot Hash
* Language Pair
* Scope
* Matching Policy Revision
* Matcher Revision
* Normalization Profile Revision
* Text Block Revision
* Translation Profile Revision

Mutable Glossary identity alone is insufficient for cache correctness.

---

# Security

Glossary operations may require permissions such as:

* View
* Suggest
* Create
* Edit Draft
* Approve
* Publish
* Lock
* Import
* Export
* Archive
* Manage Shared Glossary

Shared or global glossaries should require stricter permissions than Project glossaries.

---

# Privacy

Glossaries may contain sensitive information:

* Unreleased character names
* Plot terminology
* Private project titles
* Licensed translations
* Publisher-approved terminology
* User reading history
* Imported copyrighted dictionaries

Requirements:

* Respect Project visibility
* Prevent cross-Project leakage
* Avoid sending irrelevant entries to providers
* Minimize provider context
* Encrypt sensitive persisted data where required
* Record exports
* Respect local-only processing mode
* Exclude private notes from provider context unless explicitly allowed

---

# Audit

Important glossary actions should be auditable:

* Entry created
* Entry changed
* Entry approved
* Entry rejected
* Entry locked
* Entry merged
* Entry split
* Glossary imported
* Conflict resolved
* Snapshot published
* Entry applied automatically
* Entry overridden manually

Audit records should include actor, time, revision and reason.

---

# Events

Typical domain events include:

* `GlossaryCreated`
* `GlossaryMetadataChanged`
* `GlossaryRevisionPublished`
* `GlossaryEntryCreated`
* `GlossaryEntryRevisionCreated`
* `GlossaryEntryActivated`
* `GlossaryEntryDeactivated`
* `GlossaryEntryApproved`
* `GlossaryEntryRejected`
* `GlossaryEntryLocked`
* `GlossaryEntryUnlocked`
* `GlossaryEntryDeprecated`
* `GlossaryEntryMerged`
* `GlossaryEntrySplit`
* `GlossaryConflictDetected`
* `GlossaryConflictResolved`
* `GlossaryCandidateCreated`
* `GlossaryImportPlanned`
* `GlossaryImportCompleted`
* `GlossarySnapshotCreated`
* `GlossaryApplicationChanged`

Events should carry identifiers and revision references rather than full glossary contents whenever possible.

---

# Event Payload Example

```text
GlossaryEntryRevisionCreated
├── Glossary ID
├── Entry ID
├── Entry Revision ID
├── Parent Revision ID
├── Change Types
├── Source Language Range
├── Target Language Range
├── Actor
├── Occurred At
└── Correlation ID
```

Sensitive notes and full source dictionaries should not be included in general event payloads.

---

# Comic Translation Example

```text
Source Text Block:
他正在凝聚灵力。

Applicable Glossary Entry:
灵力 → linh lực

Translation Request:
- Source Language: zh-Hans
- Target Language: vi
- Glossary Snapshot: GS-42

Provider Output:
Hắn đang ngưng tụ linh khí.

Terminology Validation:
Expected: linh lực
Observed: linh khí
Result: violation

Post-Processed Translation:
Hắn đang ngưng tụ linh lực.
```

The provider output remains preserved.

The corrected value becomes a later Translation representation or revision.

---

# Character Name Example

```text
Entry:
Character Name

Source Forms:
- 林月
- Lín Yuè

Preferred Vietnamese Form:
- Lâm Nguyệt

Rule:
Transliterate

Scope:
Book A

Review:
Approved and Locked
```

A Translation containing `Lin Yue` may be flagged because the locked form is `Lâm Nguyệt`.

---

# Contextual Term Example

```text
Source Term:
师父

Possible Target Forms:
- sư phụ
- thầy
- sư tôn

Context Rules:
- Cultivation dialogue: prefer sư phụ
- Modern context: prefer thầy
- Formal sect context: sư tôn may be allowed
```

The Glossary provides context and alternatives.

The Translation engine still evaluates grammar and scene context.

---

# Preserve Rule Example

```text
Source:
MP

Rule:
Preserve

Allowed Target:
MP

Forbidden Targets:
- ma lực
- điểm phép
```

This rule may be applied to game-like content where the abbreviation is intentionally preserved.

---

# Overlapping Term Example

```text
Entries:
天剑宗 → Thiên Kiếm Tông
天剑   → Thiên Kiếm
剑宗   → Kiếm Tông
```

Input:

```text
他来自天剑宗。
```

The matcher should produce the most appropriate full-concept match.

It should not replace the same source range several times.

---

# User Correction Example

```text
Original Source:
灵石

Generated:
đá linh hồn

User Correction:
linh thạch
```

The correction creates:

1. A new Translation Revision
2. A possible Glossary Candidate
3. Evidence linked to the Source Text Block
4. No automatic Glossary activation unless policy allows it

---

# Glossary Update Example

```text
Old Entry:
灵石 → linh thạch

New Entry:
灵石 → linh thạch
Alias added:
linh thạch nguyên
```

Translations that already use `linh thạch` may remain valid.

Adding an alias should not automatically mark every Translation stale.

---

# Architecture Invariants

1. Glossary is a terminology Aggregate Root.
2. Glossary identity is separate from Glossary Revision.
3. Entry identity is separate from Entry Revision.
4. Published Glossary and Entry Revisions are immutable.
5. Translation references an immutable Glossary Snapshot.
6. A Glossary Snapshot references exact Entry Revisions.
7. Historical Translations preserve their original Glossary Snapshot.
8. Glossary entries are provider-independent domain data.
9. Provider-specific prompt fragments are not canonical Glossary content.
10. Every active entry declares a source-language and target-language scope.
11. Language Pair direction is significant.
12. Source Forms and Target Forms remain separate.
13. Canonical terms remain distinguishable from aliases and OCR variants.
14. Rule Type is separate from Entry Type.
15. Matching behavior is explicit.
16. Matching execution belongs outside the Glossary aggregate.
17. Runtime matches reference exact Entry Revisions.
18. Exact approved rules outrank probabilistic suggestions.
19. Scope precedence is deterministic.
20. Equal-priority incompatible rules produce a conflict.
21. Conflicts must not be resolved silently.
22. User-approved locked terminology cannot be overwritten by ordinary imports.
23. User corrections do not automatically mutate Glossary truth.
24. Suggested entries do not affect Translation until activated by policy.
25. Automatic post-processing preserves the original provider output.
26. Automatic terminology changes create traceable Translation revisions or actions.
27. Glossary changes invalidate only affected downstream artifacts.
28. New unrelated entries do not make every Translation stale.
29. Inactive or deprecated entries remain available for historical resolution.
30. Merge and split operations preserve lineage.
31. Referenced Entry Revisions cannot be hard deleted normally.
32. Derived matching and search indexes are rebuildable.
33. Cache identity uses Glossary Snapshot content, not mutable Glossary ID alone.
34. Private Project terminology cannot leak into another Project.
35. Every authoritative terminology decision is auditable.
36. The Glossary does not directly modify TextBlock source content.

---

# Open Decisions

The following decisions should remain open until implementation and prototype testing:

* Whether one Project has one Glossary or several composable glossaries
* Whether user-level glossaries are supported in the MVP
* Whether global glossaries are supported
* Whether entries maintain independent revisions or only Glossary-wide revisions
* How large Glossary Snapshots are represented
* Whether snapshots copy entry content or only reference immutable revisions
* Which match types are supported initially
* Whether regular-expression entries are allowed
* Whether fuzzy matching is local-only
* How Chinese simplified and traditional forms are linked
* Whether script conversion creates aliases automatically
* Whether OCR variants are stored directly in Entry Revisions
* Whether aliases require approval
* How overlapping terms are resolved
* How contextual conditions are expressed
* Whether context conditions use structured rules or natural-language notes
* Whether semantic matching is used for production enforcement
* Which terminology violations block Translation publication
* Whether deterministic post-processing is enabled automatically
* How Vietnamese capitalization is handled for names and titles
* How inflected target-language forms are represented
* Whether grammatical target variants belong in Glossary or Translation Profile
* Whether Glossary Candidates are a separate aggregate
* How candidate confidence is normalized
* Whether user corrections automatically create candidates
* How many occurrences are required before suggesting an entry
* Whether imports may create active draft entries
* Which import and export formats are supported first
* Whether TBX or TMX support is necessary
* How shared Glossary permissions work
* Whether locked entries can be overridden at operation scope
* How stale Translation impact is calculated efficiently
* Whether Glossary matching results are persisted or recomputed
* Whether terminology usage analytics are retained
* How provider context-budget selection is scored
* How glossary entries are prioritized when provider token limits are reached
* Whether the entire applicable snapshot or only matched entries enter the prompt
* How manually approved alternative target forms are represented
* Whether character naming belongs primarily to Character or Glossary
* How external dictionary licensing restrictions are enforced

---

# Recommended MVP Scope

The first CRAI MVP should support:

* One Project Glossary
* Stable Glossary identity
* Stable Entry identity
* Immutable Entry Revisions
* Source and target language scope
* `zh-Hans → vi`
* Optional `zh-Hant → vi`
* Source Forms
* One preferred Target Form
* Optional approved alternatives
* Basic Entry Types
* Translate rule
* Preserve rule
* Transliterate rule
* Avoid or forbidden target forms
* Exact matching
* Normalized exact matching
* Phrase matching
* Basic longest-match behavior
* Entry priority
* Project and Book scope
* Draft, Active, Inactive and Deprecated statuses
* Unreviewed and Approved review states
* Locked terminology
* Conflict detection
* Immutable Glossary Snapshot
* Translation Snapshot reference
* Terminology validation
* User-created entries
* Candidate creation from user correction
* CSV and JSON import
* CSV and JSON export
* Audit events
* Selective Translation staleness

The MVP may defer:

* Semantic matching
* Embedding-based terminology search
* Complex morphological matching
* Regular expressions
* Fuzzy automatic enforcement
* Detailed grammatical inflection
* Global shared glossaries
* Workspace-level glossary inheritance
* Full Chapter and Page exception rules
* Advanced context-rule language
* Dynamic entity linking
* TMX and TBX support
* Automatic glossary learning
* Automatic entry merging
* Complex romanization standards
* Cross-Project terminology synchronization
* Collaborative real-time editing
* Provider-specific glossary upload APIs
* Full terminology analytics

---

# Related Documents

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
* `docs/architecture/CAPABILITY_MAP.md`
* `docs/architecture/DATA_FLOW.md`
* `docs/architecture/STATE_MACHINE.md`
* `docs/architecture/EVENT_BUS.md`
* `docs/architecture/MODULE_DEPENDENCY.md`
* `docs/architecture/ai/PIPELINE.md`
* `docs/architecture/ai/CONTEXT.md`
* `docs/architecture/ai/PROMPTS.md`
* `docs/architecture/ai/REQUEST.md`
* `docs/architecture/ai/RESPONSE.md`
* `docs/architecture/ai/MEMORY.md`
* `docs/architecture/ai/ROUTING.md`
* `docs/architecture/ai/CACHE.md`
* `docs/architecture/presentation/FONTS.md`
* `docs/architecture/presentation/LAYOUT.md`
* `docs/architecture/presentation/TYPOGRAPHY.md`
