# Profile Domain

* **Document:** Domain / Profile
* **Version:** 1.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

The Profile domain defines reusable, versioned and provider-neutral configuration used to control how CRAI processes, translates, validates and presents content.

A Profile represents an intentional set of behavioral choices.

Examples:

* Translate Chinese novels into natural Vietnamese
* Preserve cultivation terminology
* Detect vertical comic text
* Prefer concise speech bubbles
* Render translated text over the original bubble
* Reject Translation when character names are inconsistent
* Use local processing for sensitive Projects
* Prefer low-cost providers for draft Translation
* Use higher-quality providers for approved Translation

Profiles allow CRAI to separate:

```text
What the user wants
```

from:

```text
How a specific provider performs it
```

The Profile domain must remain independent from:

* Provider-specific request payloads
* Provider model identifiers
* Runtime jobs
* User interface form state
* Authentication
* Billing accounts
* Raw prompt strings
* Temporary Session state
* Environment variables
* Deployment configuration

---

# Domain Role

Profiles define reusable processing intent.

```text
Profile
    │
    ├── Translation Profile
    ├── OCR Profile
    ├── Presentation Profile
    ├── Validation Profile
    ├── Routing Profile
    ├── Context Profile
    └── Export Profile
```

A Profile is selected by:

* Workspace
* Project
* Book
* Chapter
* Session
* Operation

The selected Profile Revision is resolved into operation-specific configuration.

```text
Profile Revision
        │
        ▼
Configuration Resolution
        │
        ▼
Operation Context Snapshot
        │
        ▼
Provider-neutral Request
        │
        ▼
Provider Adapter
```

---

# Profile Is Not User Preference

User Preference represents a user’s general UI or workflow preference.

Profile represents reusable processing or presentation behavior.

Examples:

```text
User Preference:
Use dark mode
```

```text
Presentation Profile:
Use 18 px serif font with 1.6 line spacing
```

```text
Translation Profile:
Prefer natural Vietnamese while preserving honorific hierarchy
```

A User Preference may select a default Profile.

It does not replace the Profile model.

---

# Profile Is Not Workspace Policy

A Profile states desired behavior.

A Workspace Policy constrains allowed behavior.

```text
Profile:
Use cloud Translation provider
```

```text
Workspace Policy:
Cloud processing forbidden
```

The operation must be rejected or resolved to an allowed alternative.

Profile cannot override mandatory Workspace policy.

---

# Profile Is Not Provider Configuration

Provider Configuration identifies how CRAI accesses one provider.

It may include:

* Provider type
* Credential reference
* Region
* Available capabilities
* Enabled models
* Rate limits

Profile describes provider-neutral intent.

Example:

```text
Translation Profile:
High quality, natural Vietnamese, moderate latency
```

Routing may resolve that intent to:

```text
Provider Configuration P3
Model M2
```

Provider Configuration and Profile must remain separate.

---

# Profile Is Not Prompt

A Profile may influence prompt compilation.

It must not be modeled as one raw prompt string.

```text
Profile
    ↓
Context Compiler
    ↓
Prompt or Request Compiler
    ↓
Provider Request
```

Raw provider prompts are derived infrastructure artifacts.

Profile remains structured, provider-neutral domain configuration.

---

# Profile Is Not Operation State

Profile does not own:

* Job status
* Provider retry count
* Token usage
* Runtime timeout attempts
* Provider response
* Execution error
* Queue priority state

Those belong to runtime or operation domains.

A Profile may define policies such as desired timeout class or quality tier, but it does not track execution.

---

# Aggregate Boundary

Profile should be modeled as an Aggregate Root.

```text
Profile Aggregate
├── Profile
├── Profile Revision
├── Profile Metadata
├── Profile Scope
├── Profile Lifecycle
├── Profile Review State
├── Profile Lock
└── Profile Revision Lineage
```

Profile owns:

* Stable Profile identity
* Profile type
* Human-readable metadata
* Revision lineage
* Revision lifecycle
* Review and approval state
* Lock state
* Visibility
* Ownership scope
* Compatibility declarations

Profile does not own:

* Provider credentials
* Projects
* Sessions
* Operations
* Translation results
* Prompt artifacts
* Runtime routing results
* User accounts
* Usage ledger

---

# Stable Identity

Critical invariant:

```text
Profile ID
≠
Profile Revision ID
```

Profile ID identifies the reusable configuration concept.

Profile Revision ID identifies one immutable version of that configuration.

Example:

```text
Profile:
Natural Vietnamese Novel Translation

Revision 1:
Literalness = 0.55

Revision 2:
Literalness = 0.45
Honorific preservation = stronger
```

Both revisions belong to the same Profile identity.

---

# Profile Record

Recommended structure:

```text
Profile
├── Profile ID
├── Profile Type
├── Owner Scope
├── Display Name
├── Description
├── Active Revision ID
├── Lifecycle State
├── Visibility
├── Created By
├── Created At
├── Updated At
└── Aggregate Version
```

The mutable Profile record points to active or recommended revisions.

Durable outputs must reference exact Profile Revision IDs.

---

# Profile Revision

Profile Revision is immutable after creation.

Recommended structure:

```text
Profile Revision
├── Profile Revision ID
├── Profile ID
├── Revision Number
├── Parent Revision ID
├── Profile Type
├── Configuration Document
├── Schema Version
├── Compatibility Metadata
├── Review State
├── Content Hash
├── Change Summary
├── Created By
├── Created At
└── Supersedes Revision ID
```

A correction creates a new revision.

It must not mutate an existing revision that has been used by an operation.

---

# Why Revisions Are Required

Without immutable revisions:

* Existing Translation results become unreproducible.
* Cache keys become unstable.
* Audit cannot explain historical behavior.
* Profile updates silently alter future retries.
* Validation results may refer to different rules over time.
* Session recovery may produce different output.
* Shared Workspace configuration becomes unsafe.

Therefore:

> Every durable operation must reference exact Profile Revisions.

---

# Profile Types

Recommended core Profile Types:

* Translation
* OCR
* Presentation
* Validation
* Context
* Routing
* Export

Possible future types:

* Import
* Capture
* Recognition
* Speech
* Audio
* Accessibility
* Review
* Quality
* Cost
* Privacy
* Notification

The Profile framework may support several types, but each type must have its own schema and validation.

---

# Avoid the Universal Profile

CRAI should not create one universal Profile containing every possible option.

An excessively broad Profile would cause:

* Unclear ownership
* High coupling
* Invalid combinations
* Large revisions
* Difficult permission control
* Unnecessary cache invalidation
* Complex user interfaces
* Poor reuse

Recommended composition:

```text
Translation Profile Revision
+
OCR Profile Revision
+
Presentation Profile Revision
+
Validation Profile Revision
+
Context Profile Revision
+
Routing Profile Revision
```

Each controls one coherent capability.

---

# Profile Composition

An operation may use several Profile Revisions together.

Example:

```text
Translation Operation
├── Translation Profile Revision 7
├── Context Profile Revision 3
├── Validation Profile Revision 5
└── Routing Profile Revision 2
```

A comic reading Session may additionally use:

```text
OCR Profile Revision 4
Presentation Profile Revision 8
```

Profile composition occurs in application-level configuration resolution.

It does not create ownership between the Profiles.

---

# Composite Profile

For user convenience, CRAI may support a Composite Profile.

A Composite Profile stores references to compatible Profile Revisions.

```text
Composite Profile Revision
├── Translation Profile Revision
├── OCR Profile Revision
├── Presentation Profile Revision
├── Validation Profile Revision
├── Context Profile Revision
└── Routing Profile Revision
```

A Composite Profile must not copy mutable Profile contents implicitly.

It references exact revisions or explicit selection policies.

---

# Composite Selection Modes

Possible reference modes:

* Exact Revision
* Active Approved Revision
* Latest Compatible Approved Revision
* Workspace Default
* Project Default

For reproducibility, every operation resolves these policies into exact revisions before execution.

---

# Profile Ownership Scope

A Profile may be owned by:

* System
* User
* Workspace
* Project

Possible future ownership:

* Organization
* Marketplace publisher
* External package

Recommended structure:

```text
Profile Owner Scope
├── Scope Type
└── Scope ID
```

---

# System Profile

System Profiles are built-in defaults.

Examples:

* Default Novel Translation
* Default Comic OCR
* Default Overlay Presentation
* Strict Validation
* Low-Cost Draft Routing

System Profiles should be versioned like any other Profile.

They must not be silently changed in place.

---

# User Profile

User Profile is private to one user unless shared.

Examples:

* Personal Chinese novel style
* Preferred Vietnamese punctuation
* Personal reading font
* Personal local-only routing preference

A User Profile may be used across Workspaces only where policy permits.

---

# Workspace Profile

Workspace Profile is shared across Projects in one Workspace.

Examples:

* Publisher Translation Style
* Team Terminology Validation
* Approved Comic Layout
* Workspace Provider Routing

Workspace administrators may control who can edit or approve it.

---

# Project Profile

Project Profile is specific to one Project.

Examples:

* Novel A Translation Style
* Comic B Bubble Layout
* Project-specific Validation rules
* Project-specific character naming convention

Project Profile may inherit or clone a Workspace Profile.

---

# Profile Scope and Applicability

Profile ownership and Profile applicability are different.

A Workspace-owned Profile may apply to:

* Entire Workspace
* Selected Projects
* Selected Books
* Selected content types
* Selected language pairs
* Selected Session types

Recommended applicability structure:

```text
Profile Applicability
├── Workspace Scope
├── Project Scope
├── Content Types
├── Source Languages
├── Target Languages
├── Session Types
├── Capability Types
└── Classification Restrictions
```

---

# Profile Visibility

Recommended visibility values:

* Private
* Workspace
* Restricted
* Shared Link
* Public
* System

Visibility controls discoverability.

It does not by itself grant permission to edit or use a Profile.

---

# Profile Lifecycle

Recommended lifecycle states:

```text
Draft
→ Active
→ Deprecated
→ Archived
```

Possible states:

* Candidate
* Draft
* Active
* Deprecated
* Archived
* Rejected
* Deleted
* Imported
* Locked

Lifecycle is separate from Review State.

---

# Draft

Draft Profile may be edited by creating replacement revisions.

Draft revisions may be tested in non-production or explicitly allowed Sessions.

Draft status should be visible to consumers.

---

# Active

Active indicates that the Profile is available for normal selection.

Several revisions may remain usable, but one may be marked as:

* Active
* Recommended
* Default
* Latest approved

Durable operations still pin exact revisions.

---

# Deprecated

Deprecated Profile should not be selected by default for new work.

Existing references remain valid.

Deprecation may include:

* Replacement Profile
* Migration recommendation
* Reason
* Effective date

---

# Archived

Archived Profile is hidden from ordinary selection.

Historical operations retain valid references to its revisions.

Archived Profiles must not be hard deleted when referenced.

---

# Review State

Recommended Review States:

* Unreviewed
* In Review
* Changes Requested
* Approved
* Rejected
* Superseded
* Locked

Review State applies to a Profile Revision.

It is distinct from Profile lifecycle.

Example:

```text
Profile:
Active

Revision 8:
Approved

Revision 9:
In Review
```

New production operations may continue using Revision 8.

---

# Approval

Approval may be required for:

* Workspace Profiles
* Publisher Profiles
* Profiles affecting public export
* Profiles using external providers
* Profiles changing terminology policy
* Profiles changing privacy behavior

Approval rules belong to Workspace governance.

---

# Lock

A Profile Revision may be locked.

Lock means the revision cannot be altered—which is already implied by immutability—but may additionally restrict:

* Deprecation
* Replacement
* Use outside approved scope
* Cloning
* Export
* Administrative deletion

Lock should not encourage mutation of an existing revision.

---

# Profile Metadata

Profile metadata may include:

* Display name
* Description
* Tags
* Intended use
* Author
* Supported language pairs
* Supported content types
* Quality tier
* Cost tier
* Latency tier
* Example output references
* Change summary

Metadata that affects behavior must be included in the immutable Revision configuration.

Non-semantic discovery metadata may remain mutable.

---

# Profile Schema

Each Profile Type must have an explicit schema.

Example:

```text
Translation Profile Schema v1
OCR Profile Schema v2
Presentation Profile Schema v1
```

Profile Revision stores its schema version.

Schema validation occurs before activation.

---

# Schema Evolution

Profile schemas will evolve.

Possible changes:

* Add optional field
* Add required field with default
* Rename field
* Split one field into several
* Replace an enum
* Remove obsolete behavior

Schema migration should create a new Profile Revision or a new normalized representation.

Historical revisions remain interpretable through their original schema versions.

---

# Unknown Fields

Import and forward compatibility may require preserving unknown fields.

Policy options:

* Reject unknown fields
* Preserve but ignore
* Preserve and warn
* Allow extension namespace

Recommended approach:

* Canonical schemas reject unknown core fields.
* Extension fields use explicit namespaces.
* Import plans report unsupported data.

---

# Extension Namespace

Possible extension structure:

```text
extensions:
  vendor.example:
    custom_setting: value
```

Extension fields must not silently alter core behavior unless a registered capability understands them.

---

# Translation Profile

Translation Profile defines provider-neutral Translation intent.

Recommended areas:

```text
Translation Profile Revision
├── Language Strategy
├── Translation Style
├── Literalness
├── Fluency
├── Terminology Policy
├── Name Policy
├── Honorific Policy
├── Pronoun Policy
├── Cultural Localization
├── Formatting Policy
├── Content Preservation
├── Ambiguity Policy
├── Dialogue Policy
├── Narration Policy
├── Quality Target
└── Output Constraints
```

---

# Translation Style

Possible style values:

* Literal
* Faithful
* Natural
* Localized
* Literary
* Concise
* Academic
* Conversational
* Subtitle
* Comic
* Custom

Style should not be represented only as an unstructured text label.

It should resolve into explicit configuration fields.

---

# Literalness

Literalness may be represented as:

* Enum
* Ordered level
* Numeric range
* Structured policy

Example:

```text
Literalness:
Balanced

Meaning Preservation:
High

Naturalness:
High

Structural Preservation:
Medium
```

A single numeric slider may be useful in UI but should not replace explicit semantic rules where precision matters.

---

# Terminology Policy

Translation Profile may define:

* Use Glossary Snapshot
* Required term enforcement
* Preferred term enforcement
* Preserve source term
* Add transliteration
* Permit synonyms
* Case sensitivity
* Inflection behavior
* Unknown term handling
* Conflict resolution

The Profile does not own Glossary Entries.

It defines how resolved Glossary context should be applied.

---

# Name Policy

Name policy may define:

* Preserve original script
* Use canonical translated name
* Use romanization
* Use localized name
* Use context-specific alias
* Reveal-safe naming
* First-mention formatting
* Title ordering
* Family-name order
* Unknown-name handling

Character Context Snapshot provides the actual Character data.

---

# Pronoun Policy

Pronoun policy may define:

* Preserve relationship hierarchy
* Prefer natural Vietnamese address
* Avoid unsupported gender assumptions
* Use speaker-listener relationship
* Use scene context
* Preserve self-reference style
* Flag uncertain pronouns
* Allow neutral fallback

The Profile must not hard-code individual Character truth.

---

# Honorific Policy

Possible rules:

* Preserve all honorifics
* Translate honorifics
* Drop culturally redundant honorifics
* Use Vietnamese equivalents
* Use source transliteration
* Resolve by relationship
* Resolve by rank
* Flag ambiguous honorifics

---

# Cultural Localization

Localization options may include:

* Preserve source culture
* Adapt idioms
* Adapt measurements
* Adapt punctuation
* Adapt dates
* Adapt titles
* Preserve food and place names
* Add explanatory notes
* Avoid domesticating proper nouns

Localization is distinct from language translation itself.

---

# Formatting Policy

Translation Profile may define semantic formatting requirements:

* Preserve paragraph boundaries
* Preserve line breaks
* Preserve emphasis
* Preserve speaker labels
* Preserve ruby annotations
* Preserve punctuation intent
* Limit output lines
* Avoid Markdown
* Output structured segments

Visual font and layout belong to Presentation Profile.

---

# Dialogue Policy

Dialogue policy may define:

* Natural spoken Vietnamese
* Maintain character voice
* Preserve interruptions
* Preserve hesitations
* Preserve sentence fragments
* Use Vietnamese quotation conventions
* Avoid over-formalization
* Use Character Speech Profile

---

# Narration Policy

Narration policy may define:

* Literary register
* Tense handling
* Viewpoint preservation
* Internal monologue formatting
* Narrator-person distinction
* Descriptive density
* Sentence-length preference

---

# Ambiguity Policy

Possible behaviors:

* Preserve ambiguity
* Choose most likely interpretation
* Add uncertainty annotation
* Produce alternatives
* Require review
* Use surrounding context
* Avoid adding unstated gender
* Avoid resolving unrevealed identity

---

# Translation Output Constraints

Possible constraints:

* Maximum characters
* Maximum lines
* Preserve segment count
* Structured JSON output
* No commentary
* No source repetition
* Include confidence
* Include alternatives
* Include terminology findings
* Include alignment

These remain provider-neutral requirements.

Provider adapters translate them into provider-specific request formats.

---

# OCR Profile

OCR Profile defines provider-neutral text detection and recognition intent.

Recommended areas:

```text
OCR Profile Revision
├── Content Mode
├── Expected Languages
├── Script Policy
├── Text Orientation
├── Detection Policy
├── Recognition Policy
├── Confidence Thresholds
├── Layout Assumptions
├── Preprocessing Intent
├── Reading Order Policy
├── Region Policy
└── Output Requirements
```

---

# OCR Content Mode

Possible modes:

* Comic
* Novel Page
* Web Page
* Screenshot
* Scanned Book
* Subtitle
* Document
* Mixed
* Handwritten
* Custom

Content Mode helps select appropriate detection behavior.

---

# Expected Languages

OCR Profile may define:

* Primary language
* Secondary languages
* Allowed scripts
* Mixed-language handling
* Unknown-language behavior

Language values use canonical Language Value Objects.

Provider-specific OCR language codes remain in adapters.

---

# Text Orientation

Possible values:

* Horizontal
* Vertical
* Mixed
* Auto Detect
* Rotated
* Curved

Chinese and Japanese comic support may require mixed horizontal and vertical text.

---

# Detection Policy

OCR detection policy may define:

* Detect all text
* Detect dialogue only
* Detect narration
* Detect sound effects
* Ignore interface text
* Ignore watermarks
* Merge nearby regions
* Split distinct lines
* Detect speech bubbles
* Detect captions
* Detect page numbers

OCR Profile expresses desired categories.

Actual computer vision implementation remains outside the Profile domain.

---

# OCR Confidence Thresholds

Possible thresholds:

* Minimum region detection confidence
* Minimum recognition confidence
* Auto-accept threshold
* Review-required threshold
* Reject threshold
* Language-confidence threshold
* Reading-order confidence threshold

Thresholds should be validated for coherent ordering.

---

# OCR Preprocessing Intent

Possible settings:

* Denoise
* Contrast enhancement
* Upscale
* Deskew
* Remove background
* Bubble isolation
* Thresholding
* Sharpening
* Preserve color
* Detect inverted text

These are semantic intents.

Provider-specific image parameters belong to adapters or processing pipelines.

---

# OCR Reading Order

Possible policies:

* Left to right
* Right to left
* Top to bottom
* Vertical columns
* Manga order
* Webtoon order
* Automatic
* User-defined

Reading-order output should remain revisable independently from OCR text where architecture requires it.

---

# OCR Region Policy

OCR Profile may define:

* Full Page
* User Selection
* Auto-detected panels
* Speech bubbles only
* Existing regions
* Screen viewport
* Incremental changed regions

It does not own Page Regions or TextBlocks.

---

# OCR Output Requirements

Possible requirements:

* Text
* Region geometry
* Line geometry
* Word confidence
* Language detection
* Reading order
* Character alignment
* Alternative recognition candidates
* Orientation
* Script classification

---

# Presentation Profile

Presentation Profile defines how original and translated content are displayed.

Recommended areas:

```text
Presentation Profile Revision
├── Content Mode
├── Display Mode
├── Typography
├── Layout
├── Overlay
├── Original Visibility
├── Translation Visibility
├── Reading Direction
├── Overflow Policy
├── Accessibility
├── Theme
└── Device Adaptation
```

Presentation Profile does not alter Translation semantics.

---

# Presentation Content Mode

Possible modes:

* Novel
* Comic
* Webtoon
* Parallel Text
* Subtitle
* Review
* Export
* Accessibility
* Custom

---

# Display Mode

Possible values:

* Original Only
* Translation Only
* Side by Side
* Interleaved
* Overlay
* Replacement
* Hover
* Focus
* Comparison
* Review

---

# Typography

Typography configuration may include:

* Font family preference
* Fallback families
* Font size
* Minimum font size
* Maximum font size
* Weight
* Italic policy
* Line height
* Letter spacing
* Word spacing
* Paragraph spacing
* Text alignment
* Vertical text support

Font files themselves are not Profile data.

Profile stores family references and requirements.

---

# Font Resolution

Recommended flow:

```text
Presentation Profile Font Preference
        ↓
Workspace Font Policy
        ↓
Available Device Fonts
        ↓
Bundled or Licensed Font Set
        ↓
Resolved Font
```

The resolved font may differ by device.

The Profile preserves semantic preference and fallback order.

---

# Layout

Presentation layout may define:

* Maximum width
* Margins
* Column count
* Text alignment
* Bubble padding
* Caption placement
* Translation block spacing
* Responsive behavior
* Panel association
* Page flow
* Scroll mode

---

# Overlay Configuration

Comic overlay settings may include:

* Replace original text
* Cover original text
* Preserve bubble background
* Overlay opacity
* Text padding
* Region clipping
* Rotation handling
* Bubble-tail protection
* Low-confidence indicator
* Debug region visibility

Image inpainting and rendering remain presentation capabilities.

---

# Overflow Policy

Possible overflow policies:

* Reduce font size
* Expand region
* Wrap lines
* Condense spacing
* Truncate
* Scroll
* Show popup
* Use external caption
* Require review
* Regenerate concise Translation

Presentation may request a more concise Translation, but must not mutate an approved Translation silently.

---

# Accessibility

Presentation Profile may define:

* High contrast
* Dyslexia-friendly font preference
* Minimum font size
* Screen reader labels
* Keyboard navigation
* Reduced motion
* Color-independent warnings
* Text-to-speech compatibility
* Larger touch targets

---

# Validation Profile

Validation Profile defines which checks apply to processed content and how findings are classified.

Recommended areas:

```text
Validation Profile Revision
├── Validation Rules
├── Severity Mapping
├── Confidence Thresholds
├── Blocking Policy
├── Review Policy
├── Auto-Fix Policy
├── Rule Scope
└── Output Requirements
```

---

# Validation Rule Categories

Possible categories:

* Language mismatch
* Missing Translation
* Empty output
* Hallucinated content
* Omitted content
* Terminology inconsistency
* Character name inconsistency
* Pronoun inconsistency
* Speaker mismatch
* Relationship mismatch
* Spoiler leak
* Formatting mismatch
* Length overflow
* OCR confidence
* Reading-order inconsistency
* Duplicate text
* Unsupported script
* Unsafe provider output
* Invalid structured response

---

# Severity

Recommended severities:

* Information
* Warning
* Error
* Critical
* Blocking

Severity may be overridden by scope.

Example:

```text
Terminology mismatch:
Warning in draft mode
Blocking in publication mode
```

---

# Blocking Policy

Validation Profile may define whether findings:

* Allow continuation
* Require warning acknowledgement
* Require review
* Block approval
* Block export
* Block publication
* Trigger reprocessing
* Trigger fallback routing

---

# Auto-Fix Policy

Auto-fix must be conservative.

Possible values:

* Disabled
* Safe deterministic fixes only
* Suggest fixes
* Apply formatting fixes
* Apply glossary substitutions
* Require user confirmation

An auto-fix that changes Translation meaning must create a new Translation Revision.

---

# Context Profile

Context Profile defines which surrounding information should be compiled for an operation.

Recommended areas:

```text
Context Profile Revision
├── Context Sources
├── Window Sizes
├── Character Context Policy
├── Glossary Context Policy
├── Story Context Policy
├── Spoiler Policy
├── Memory Policy
├── Prior Translation Policy
├── Token Budget
└── Truncation Strategy
```

---

# Context Sources

Possible sources:

* Current TextBlock
* Neighboring TextBlocks
* Current Page
* Previous Pages
* Current Chapter
* Previous Chapter summary
* Character Context Snapshot
* Glossary Snapshot
* Project style guide
* Previous Translation Revisions
* User notes
* Session Memory
* Scene summary

---

# Context Window

Context Profile may define:

* Previous TextBlock count
* Next TextBlock count
* Previous Page count
* Chapter summary inclusion
* Dialogue-turn count
* Maximum source characters
* Maximum estimated tokens

Exact provider token calculation occurs later.

---

# Spoiler Policy

Possible spoiler settings:

* Current position only
* Current Chapter
* Previously read content only
* Project-approved knowledge
* Full Project context
* Explicitly pinned future context

Default reading behavior should avoid future spoilers.

Character aliases and identities must respect reveal boundaries.

---

# Context Priority

When context exceeds budget, recommended priority may be:

```text
Current Source
    ↓
Required Glossary
    ↓
Confirmed Character Context
    ↓
Immediate Dialogue
    ↓
Current Page
    ↓
Recent Translation
    ↓
Chapter Summary
    ↓
General Project Context
```

Context Profile should make this policy explicit.

---

# Context Truncation

Possible strategies:

* Drop lowest-priority entries
* Summarize older context
* Keep complete dialogue turns
* Preserve required terminology
* Preserve character relationships
* Preserve source boundaries
* Split operation
* Reject when required context cannot fit

---

# Routing Profile

Routing Profile defines provider-neutral execution preferences.

Recommended areas:

```text
Routing Profile Revision
├── Capability Requirements
├── Quality Tier
├── Cost Tier
├── Latency Tier
├── Privacy Tier
├── Locality Preference
├── Provider Allowlist
├── Provider Denylist
├── Fallback Policy
├── Retry Class
├── Model Capability Requirements
└── Budget Constraints
```

Routing Profile does not identify raw credentials.

---

# Quality Tier

Possible values:

* Draft
* Standard
* High
* Publication
* Experimental
* Custom

Quality Tier is an intent used by routing.

It must not map permanently to one provider model.

---

# Cost Tier

Possible values:

* Free
* Low
* Balanced
* Premium
* Unrestricted
* Budget Bound

Workspace policy and quota remain authoritative.

---

# Latency Tier

Possible values:

* Interactive
* Near Real Time
* Standard
* Batch
* Background

This may influence provider and queue selection.

It does not guarantee exact execution time.

---

# Privacy Tier

Possible values:

* Local Only
* Private Cloud Allowed
* Approved Providers Only
* External Allowed
* Public Content

Workspace policy may further restrict the result.

---

# Fallback Policy

Possible fallback actions:

* Retry same provider
* Try another model
* Try another provider
* Fall back to local model
* Reduce context
* Lower quality
* Queue for later
* Require user approval
* Stop

Fallback must not silently violate policy or materially change Translation intent.

---

# Export Profile

Export Profile defines how selected domain content becomes an export package.

Recommended areas:

```text
Export Profile Revision
├── Export Format
├── Included Content
├── Revision Selection
├── Layout
├── Metadata
├── Spoiler Policy
├── Watermark Policy
├── File Naming
├── Packaging
└── Compatibility Target
```

---

# Export Format

Possible formats:

* JSON
* YAML
* CSV
* Markdown
* HTML
* EPUB
* PDF
* Plain Text
* Image Package
* Subtitle Format
* Translation Memory Format
* Custom

Format-specific rendering occurs outside the Profile aggregate.

---

# Revision Selection

Export Profile may define:

* Latest approved Translation
* Latest user-confirmed Translation
* Specific Translation Revision
* Include revision history
* Include source text
* Include OCR alternatives
* Include validation findings
* Include glossary references
* Include character references

---

# Profile Inheritance

Profiles may reuse other Profiles through controlled inheritance.

Possible model:

```text
Base Profile Revision
        +
Override Document
        ↓
Derived Profile Revision
```

The derived Profile Revision must preserve:

* Base Profile Revision ID
* Explicit overrides
* Fully resolved content hash
* Compatibility validation

---

# Inheritance Risks

Deep inheritance may cause:

* Hard-to-understand behavior
* Unexpected changes
* Circular dependencies
* Difficult audit
* Complex migration

Recommended restriction:

* Maximum one direct base revision in MVP
* No circular references
* Resolve to a flattened immutable configuration
* Pin exact base revision
* Do not track mutable “latest” during execution

---

# Clone Versus Inherit

## Clone

Copies one Profile Revision into a new independent Profile.

Future changes are independent.

## Inherit

Creates a new Profile Revision that explicitly references a base revision and overrides selected fields.

## Follow Latest

Tracks future approved revisions of another Profile.

This is convenient but unsafe for reproducibility unless resolved before each operation.

MVP should prioritize Clone and exact-revision inheritance.

---

# Override Semantics

A Profile may be overridden at narrower scopes.

Example:

```text
Workspace Translation Profile
        ↓
Project Translation Profile
        ↓
Session Override
        ↓
Operation Override
```

The system must distinguish:

* Profile Revision override
* Individual field override
* Mandatory policy
* User preference
* Runtime adaptation

---

# Configuration Resolution

Recommended resolution sequence:

```text
Application Defaults
        ↓
System Profile
        ↓
Workspace Profile
        ↓
Project Profile
        ↓
Book or Chapter Profile
        ↓
Session Profile Selection
        ↓
Operation Override
        ↓
Workspace Policy Validation
        ↓
Capability Validation
        ↓
Resolved Configuration Snapshot
```

More specific values override defaults where allowed.

Mandatory policy is enforced after and during resolution.

---

# Resolved Profile Snapshot

An operation should receive an immutable Resolved Profile Snapshot.

Recommended structure:

```text
Resolved Profile Snapshot
├── Snapshot ID
├── Profile Type
├── Source Profile Revision IDs
├── Applied Overrides
├── Applied Defaults
├── Policy Revision ID
├── Resolved Configuration
├── Schema Version
├── Content Hash
├── Created At
└── Resolution Trace
```

This makes behavior reproducible and explainable.

---

# Resolution Trace

Resolution Trace explains where each effective value came from.

Example:

```text
target_style:
  value: natural
  source: Project Translation Profile Revision 12

honorific_policy:
  value: preserve_relationship
  source: Workspace Translation Profile Revision 4

maximum_output_characters:
  value: 120
  source: Operation Override

cloud_processing:
  value: false
  source: Workspace Policy Revision 9
```

---

# Effective Configuration

Effective Configuration is not itself a mutable Profile.

It is a snapshot derived for:

* Session
* Operation
* Export
* Validation
* Presentation

It may combine several Profile Types.

---

# Compatibility

Profiles must declare and validate compatibility.

Possible dimensions:

* Source language
* Target language
* Content type
* Capability type
* Project type
* Session type
* Schema version
* Required context
* Required provider capability
* Required Presentation capability

---

# Language Compatibility

A Translation Profile may support:

* Any source language to Vietnamese
* Chinese to Vietnamese only
* Japanese to Vietnamese only
* One language family
* Any target language

Compatibility should use canonical Language ranges.

Example:

```text
Source:
zh-*

Target:
vi
```

---

# Content Compatibility

Possible content types:

* Novel
* Comic
* Webtoon
* Document
* Subtitle
* Dialogue
* Narration
* Mixed

A comic Translation Profile may impose concise output constraints unsuitable for a novel.

---

# Capability Compatibility

Example:

```text
OCR Profile:
Requires region geometry support
```

A provider returning plain text only is incompatible.

Routing should avoid incompatible provider configurations.

---

# Profile Validation

Profile validation includes:

* Schema validity
* Enum validity
* Range validity
* Required-field presence
* Language compatibility
* Internal threshold consistency
* Reference validity
* Base revision validity
* No inheritance cycles
* Policy compatibility
* Capability compatibility
* Output-constraint consistency
* Ownership scope validity
* Extension namespace validity

---

# Cross-Field Validation

Examples:

* Minimum font size must not exceed maximum font size.
* Auto-accept confidence must exceed review threshold.
* Review threshold must exceed reject threshold.
* Local-only privacy cannot allow cloud-only fallback.
* “Preserve line count” conflicts with unrestricted paragraph restructuring.
* “No source repetition” conflicts with bilingual output.
* Target-language requirement must match supported language range.
* Strict glossary enforcement requires a Glossary context source.
* Character-aware pronouns require Character Context.
* Bubble replacement requires geometry-capable presentation.

---

# Profile Test Case

A Profile Revision may include or reference test cases.

Recommended structure:

```text
Profile Test Case
├── Test Case ID
├── Profile Revision ID
├── Input Reference
├── Context Reference
├── Expected Properties
├── Prohibited Properties
├── Expected Findings
└── Status
```

Expected output should often use properties rather than one exact string.

---

# Translation Profile Test

Example expected properties:

* Uses “sư tôn” for the specified term
* Preserves Character A’s formal register
* Does not reveal Character B’s hidden identity
* Produces no more than three lines
* Does not add explanatory commentary

---

# Profile Evaluation

Profile Evaluation measures Profile behavior against:

* Test datasets
* User ratings
* Validation findings
* Cost
* Latency
* Terminology consistency
* Layout success
* Review acceptance

Evaluation results are derived data.

They must not silently mutate the Profile.

---

# Profile Candidate

AI or import processes may propose a Profile Candidate.

Examples:

* Infer Translation style from approved chapters
* Infer terminology policy
* Infer OCR settings from document samples
* Infer Presentation settings from user adjustments

Candidate is not canonical Profile truth.

---

# Candidate Promotion

Recommended flow:

```text
Profile Candidate
→ Review
→ Edit
→ Validate
→ Create Profile Revision
→ Approve
→ Activate
```

AI must not silently replace approved Profile configuration.

---

# Import

Profile import may support:

* JSON
* YAML
* Workspace package
* Project package
* External translation tool configuration
* User preset
* Template package

Import must create an Import Plan before applying changes.

---

# Import Plan

Recommended structure:

```text
Profile Import Plan
├── Source Format
├── Detected Profile Type
├── Schema Version
├── Proposed Owner Scope
├── Proposed Profile
├── Proposed Revisions
├── Unsupported Fields
├── Conflicts
├── Required Migrations
└── Validation Findings
```

Import must not overwrite approved revisions silently.

---

# Export

Profile export should preserve:

* Profile ID where portability permits
* Revision ID
* Profile Type
* Schema version
* Configuration
* Revision lineage
* Ownership metadata where appropriate
* Compatibility declarations
* Content hash
* Review state where allowed

Sensitive provider credentials must never be included.

---

# Round-Trip Preservation

Importing an exported Profile should preserve semantics.

Possible identity modes:

* Preserve original IDs
* Generate new IDs and retain external references
* Clone into new ownership scope
* Merge by explicit user decision

---

# Profile Package

A Profile package may include:

* Profile metadata
* One or several revisions
* Test cases
* Example outputs
* Compatibility metadata
* Base Profile references
* Documentation
* License metadata

It must not include raw provider secrets.

---

# Marketplace and Sharing

Future CRAI versions may support sharing Profiles.

Potential features:

* Public Profile catalog
* Workspace template library
* Author attribution
* Version updates
* Ratings
* Compatibility reports
* License
* Trust level
* Security review

Public Profile content should be treated as untrusted input until validated.

---

# External Profile Reference

A Profile may retain references to:

* External preset ID
* Marketplace package ID
* Source repository
* Imported tool configuration
* Publisher style guide identifier

External IDs are not canonical CRAI Profile identity.

---

# Profile Selection

Profile selection may occur through:

* Explicit user choice
* Workspace default
* Project default
* Session default
* Content-type matching
* Language matching
* Template
* Routing rule
* Automatic recommendation

Automatic recommendation must resolve to an exact Profile Revision before use.

---

# Default Profile

A scope may define one default Profile per type.

Example:

```text
Project Default Profiles
├── Translation Profile Revision
├── OCR Profile Revision
├── Presentation Profile Revision
├── Validation Profile Revision
└── Routing Profile Revision
```

Defaults may be changed without rewriting historical operations.

---

# Active Revision

A Profile may designate one Active Revision.

Active Revision means:

* Preferred for new selections
* Not automatically substituted into existing Sessions
* Not automatically applied to already-started operations
* Not a replacement for exact revision references

---

# Follow Latest Approved

Some Project configurations may choose:

```text
Follow Latest Approved Revision
```

At operation start:

1. Resolve latest approved compatible revision.
2. Record exact Revision ID.
3. Create Resolved Profile Snapshot.
4. Execute using that snapshot.

Later approvals do not alter the operation.

---

# Pinning

A Project or Session may pin a Profile Revision.

Pinning protects against automatic changes.

Common use cases:

* Publication consistency
* Long-running Book Translation
* Reproducible review
* Controlled experiment
* Offline operation

---

# Migration

When a newer Profile Revision is published, consumers may:

* Continue using pinned revision
* Adopt immediately
* Adopt for new Chapters
* Fork Session
* Reprocess affected content
* Compare revisions
* Require approval

Profile publication itself must not force reprocessing.

---

# Profile Change Impact

Profile changes may be classified as:

* No Impact
* Presentation Only
* Validation Only
* Context Change
* Routing Only
* Retranslation Recommended
* Retranslation Required
* Re-OCR Recommended
* Re-OCR Required
* Re-export Required

---

# Impact Examples

## Presentation Font Change

```text
Impact:
Presentation Only
```

Translation does not become stale.

## Terminology Enforcement Change

```text
Impact:
Validation or Retranslation Recommended
```

Affected Translations may be identified through Glossary and Profile snapshots.

## Target Style Change

```text
Impact:
Retranslation Recommended
```

## OCR Language Change

```text
Impact:
Re-OCR Recommended
```

## Routing Cost Preference Change

```text
Impact:
Future Execution Only
```

Completed Translation semantics remain unchanged.

---

# Staleness

A domain artifact becomes stale only when a relevant input changed.

Profile update alone does not automatically stale every artifact using the Profile identity.

The system compares exact revisions and impact classifications.

```text
Artifact used Profile Revision 4
Current preferred revision is 5
```

This means the artifact is based on an older Profile.

It does not necessarily mean the artifact is invalid.

---

# Retranslation Scope

When Profile Revision changes, CRAI should identify affected Translations through:

* Exact Profile Revision reference
* Resolved Profile Snapshot
* Profile change impact
* Content type
* Language pair
* Project scope
* Validation findings

Only affected content should be recommended for retranslation.

---

# Profile Diff

CRAI should support semantic Profile Revision diff.

Example:

```text
Revision 7 → Revision 8

Changed:
- Literalness: balanced → natural
- Honorific policy: preserve all → relationship-aware
- Maximum output lines: 5 → 4

Unchanged:
- Target language
- Glossary enforcement
- Character context policy
```

Diff should not rely only on raw JSON text comparison.

---

# Merge

Concurrent Profile changes may require merge.

Three-way merge inputs:

```text
Base Revision
User A Revision
User B Revision
```

Possible outcomes:

* Automatically merged
* Conflict requires review
* Separate revisions retained

Approved revisions are never modified during merge.

---

# Conflict Types

Possible conflicts:

* Same field changed differently
* Base Profile changed
* Incompatible language range
* Policy conflict
* Deleted referenced Profile
* Different schema migration
* Extension namespace collision
* Composite reference conflict

---

# Fork

Fork creates a new Profile identity from an existing Revision.

```text
Source Profile Revision
        ↓
New Profile ID
New Revision 1
Fork Lineage Reference
```

Fork is useful when:

* Project needs permanent specialization
* Workspace wants independent control
* User customizes a System Profile
* Imported Profile should not track upstream

---

# Clone

Clone may be equivalent to Fork without retaining public lineage.

Recommended architecture should retain source reference where privacy and licensing permit it.

---

# Profile Dependencies

A Profile may reference:

* Base Profile Revision
* Glossary requirement
* Character Context requirement
* Validation Profile
* Shared template
* Font family identifiers
* Capability requirement

Dependencies must be explicit.

Hidden dependencies make Profiles unsafe to reuse.

---

# Dependency Graph

Profile dependency graph must be acyclic.

Example:

```text
Composite Profile
├── Translation Profile
├── OCR Profile
├── Presentation Profile
└── Validation Profile
```

A Translation Profile must not indirectly depend back on the Composite Profile.

---

# Profile Hash

Each Profile Revision should have a content hash.

Hash should cover semantic configuration, including:

* Profile Type
* Schema version
* Normalized configuration
* Base Revision reference
* Behavior-affecting compatibility fields

Non-semantic metadata such as display description may be excluded according to canonicalization policy.

---

# Canonicalization

Before hashing:

* Sort map keys
* Normalize enums
* Normalize language tags
* Normalize numeric formats
* Resolve default representation
* Preserve ordered lists where order matters
* Remove non-semantic whitespace
* Validate extension namespace

---

# Cache Integration

Cache keys should use:

* Resolved Profile Snapshot hash
* Source Revision hash
* Context Snapshot hashes
* Pipeline version
* Capability version
* Provider behavior version where required

Cache should not depend only on Profile ID.

```text
Profile ID
```

is insufficient because different revisions may produce different behavior.

---

# Session Integration

Session selects exact Profile Revisions or selection policies.

Recommended Session references:

```text
Session
├── Translation Profile Revision
├── OCR Profile Revision
├── Presentation Profile Revision
├── Validation Profile Revision
├── Context Profile Revision
└── Routing Profile Revision
```

Temporary Session overrides do not mutate the selected Profile.

They contribute to a Resolved Profile Snapshot.

---

# Workspace Integration

Workspace may provide:

* Shared Profiles
* Default Profiles
* Mandatory Profile constraints
* Profile visibility
* Approval rules
* Provider policy
* Profile editing permissions

Workspace does not automatically own User Profiles.

---

# Project Integration

Project may:

* Select Workspace Profile Revisions
* Own Project Profiles
* Pin Profile Revisions
* Define defaults
* Restrict compatible Profile Types
* Follow latest approved shared revisions
* Clone shared Profiles
* Require approved Profiles

---

# Book and Chapter Integration

Books and Chapters may override selected Profile Revisions for specific needs.

Examples:

* One Chapter contains vertical text
* One story arc needs different naming rules
* One bonus chapter uses a different Translation style
* One volume has a different layout

Overrides should be explicit and scoped.

---

# Translation Integration

Translation Revision should reference:

* Translation Profile Revision or Resolved Snapshot
* Context Profile Revision or Resolved Snapshot
* Validation Profile Revision used
* Routing decision reference where relevant

The most important reproducibility reference is the immutable resolved configuration actually used.

---

# OCR Integration

OCR result should reference:

* OCR Profile Revision
* Resolved OCR Profile Snapshot
* Preprocessing pipeline revision
* Recognition engine version
* Source Image Revision

---

# Presentation Integration

Rendered artifact should reference:

* Presentation Profile Revision
* Resolved Presentation Snapshot
* Translation Revision
* Source layout revision
* Rendering engine revision
* Font resolution information where required

Presentation output may be regenerated without changing Translation truth.

---

# Validation Integration

Validation Result should reference:

* Validation Profile Revision
* Rule versions
* Target artifact Revision
* Context snapshots
* Finding severity mapping

Changing Validation Profile creates new validation results.

It does not rewrite old findings.

---

# Provider Adapter Integration

Provider Adapter receives normalized request intent.

Example:

```text
Resolved Translation Profile
        ↓
Translation Request Model
        ↓
Provider Adapter
        ↓
Provider-specific parameters
```

Provider-specific values may include:

* Model ID
* Temperature
* Top-p
* JSON mode
* OCR feature flags
* Maximum tokens
* Safety settings

These should not leak into canonical Profile schemas unless they represent portable semantics.

---

# Provider Hints

Profile may support optional provider hints.

Recommended restrictions:

* Hints are namespaced.
* Hints are non-canonical.
* Core behavior does not depend exclusively on them.
* Unsupported hints can be ignored with warning.
* Policy may forbid hints.
* Hints do not contain credentials.

Example:

```text
extensions:
  provider.example:
    preferred_reasoning_level: medium
```

---

# Provider Neutrality

Core Profile fields should describe intent.

Prefer:

```text
quality_tier: high
```

over:

```text
model: provider-x-model-2026
```

Prefer:

```text
output_structure: strict_json
```

over:

```text
provider_response_format: json_schema_v5
```

Provider adapters perform translation from intent to provider mechanics.

---

# User Correction

User correction may imply a useful Profile change.

Example:

* User repeatedly changes formal narration to natural narration.
* User repeatedly reduces bubble text.
* User repeatedly restores honorifics.
* User repeatedly rejects one romanization system.

The system may create a Profile Change Candidate.

It must not silently mutate the active Profile.

---

# Learning From Corrections

Possible workflow:

```text
User Corrections
        ↓
Pattern Detection
        ↓
Profile Change Candidate
        ↓
Evaluation
        ↓
User Review
        ↓
New Profile Revision
```

Corrections remain attributable to their original artifacts.

---

# Profile Recommendation

CRAI may recommend Profiles based on:

* Content type
* Language pair
* Project history
* User choices
* Device capability
* Workspace policy
* Cost preference
* Quality requirement

Recommendation is not automatic truth.

The resolved choice must be visible and auditable.

---

# Experiment

Users may compare Profile Revisions.

Example:

```text
Source Text
├── Translation Profile Revision 7
└── Translation Profile Revision 8
```

Experiment should record:

* Input Revision
* Context Snapshots
* Profile Revisions
* Provider decisions
* Results
* Ratings
* Validation findings
* Cost
* Latency

Experiment results are derived evaluation data.

---

# A/B Comparison

A/B comparison should avoid changing production defaults automatically.

Promotion flow:

```text
Experiment Result
→ Human Review
→ Revision Approval
→ Active Revision Update
```

---

# Permissions

Possible Profile permissions:

* `profile.view`
* `profile.create`
* `profile.edit`
* `profile.review`
* `profile.approve`
* `profile.activate`
* `profile.deprecate`
* `profile.archive`
* `profile.clone`
* `profile.export`
* `profile.import`
* `profile.delete`
* `profile.use`
* `profile.lock`

Permissions may vary by owner scope and Profile Type.

---

# Role Examples

## Translator

May:

* View approved Translation Profiles
* Create Project Translation Profile drafts
* Test Profile revisions

May not:

* Change Workspace policy
* Approve Workspace Profile without permission

## Reviewer

May:

* Review Profile test results
* Approve selected Profile types
* Compare revisions

## Administrator

May:

* Manage Workspace Profiles
* Set defaults
* Control visibility
* Archive Profiles

## Reader

May:

* Use approved Profiles
* Create private Session overrides

---

# Deletion

Referenced Profile Revisions must not be hard deleted.

Preferred actions:

* Archive Profile
* Deprecate Revision
* Hide from selection
* Remove mutable metadata
* Create tombstone

Historical operations require continued resolution of revision references.

---

# Profile Tombstone

A deleted unreferenced Profile may leave:

```text
Profile Tombstone
├── Profile ID
├── Profile Type
├── Owner Scope
├── Deleted At
├── Deleted By
└── Reason
```

Referenced revisions should remain retained according to audit policy.

---

# Retention

Long-term retention should prioritize:

* Used Profile Revisions
* Approved Profile Revisions
* Profile lineage
* Resolution snapshots
* Audit records
* Test results associated with published revisions

Temporary candidates and failed imports may use shorter retention.

---

# Audit

Audit should record:

* Profile creation
* Revision creation
* Import
* Export
* Review
* Approval
* Activation
* Deprecation
* Archive
* Fork
* Clone
* Lock
* Default selection change
* Visibility change
* Ownership transfer
* Policy rejection

Audit events should not include provider secrets.

---

# Events

Typical Profile domain events include:

* `ProfileCreated`
* `ProfileMetadataUpdated`
* `ProfileRevisionCreated`
* `ProfileRevisionValidated`
* `ProfileRevisionSubmittedForReview`
* `ProfileRevisionApproved`
* `ProfileRevisionRejected`
* `ProfileRevisionLocked`
* `ProfileActivated`
* `ProfileDefaultChanged`
* `ProfileDeprecated`
* `ProfileArchived`
* `ProfileForked`
* `ProfileCloned`
* `ProfileImported`
* `ProfileExported`
* `ProfileCandidateCreated`
* `ProfileCandidatePromoted`
* `ProfileCompatibilityChanged`
* `ProfileImpactAssessed`
* `ResolvedProfileSnapshotCreated`

---

# Event Payload Example

```text
ProfileRevisionApproved
├── Profile ID
├── Profile Revision ID
├── Profile Type
├── Owner Scope
├── Approved By
├── Approved At
├── Schema Version
├── Content Hash
├── Correlation ID
└── Causation ID
```

The complete configuration may be retrieved from canonical storage rather than copied into every event.

---

# Persistence

Recommended canonical tables or collections:

```text
Profile
Profile Revision
Profile Revision Parent
Profile Applicability
Profile Review
Profile Lock
Profile Fork Lineage
Profile Default Selection
Profile Tombstone
Resolved Profile Snapshot
```

Separate derived or supporting data:

```text
Profile Search Index
Profile Evaluation
Profile Test Result
Profile Recommendation
Profile Candidate
Profile Import Plan
Profile Usage Projection
```

---

# Suggested Profile Record

```text
Profile
├── id
├── profile_type
├── owner_scope_type
├── owner_scope_id
├── display_name
├── description
├── visibility
├── lifecycle_state
├── active_revision_id
├── created_by
├── created_at
├── updated_at
└── version
```

---

# Suggested Profile Revision Record

```text
ProfileRevision
├── id
├── profile_id
├── revision_number
├── parent_revision_id
├── schema_version
├── configuration_document
├── compatibility_document
├── review_state
├── content_hash
├── change_summary
├── created_by
├── created_at
└── supersedes_revision_id
```

---

# Suggested Default Selection Record

```text
ProfileDefaultSelection
├── id
├── scope_type
├── scope_id
├── profile_type
├── selection_mode
├── profile_id
├── profile_revision_id
├── set_by
├── set_at
└── version
```

---

# Suggested Resolved Snapshot Record

```text
ResolvedProfileSnapshot
├── id
├── profile_type
├── source_revision_ids
├── override_references
├── policy_revision_id
├── resolved_configuration
├── schema_version
├── content_hash
├── resolution_trace
└── created_at
```

---

# Translation Profile Example

```yaml
profile_type: translation
schema_version: 1

languages:
  source:
    - zh-Hans
    - zh-Hant
  target:
    - vi

content:
  supported_types:
    - novel
    - dialogue

style:
  mode: natural
  meaning_preservation: high
  structural_preservation: medium
  literary_register: modern

terminology:
  use_glossary: true
  required_terms: strict
  preferred_terms: warning
  unknown_terms: preserve_or_transliterate

characters:
  use_character_context: true
  preserve_speech_profile: true
  prevent_spoiler_reveals: true

pronouns:
  relationship_aware: true
  avoid_unsupported_gender: true
  uncertain_behavior: flag

formatting:
  preserve_paragraphs: true
  preserve_emphasis: true
  output_commentary: false
```

This is a provider-neutral example.

It must not include provider API parameters.

---

# Comic Translation Profile Example

```yaml
profile_type: translation
schema_version: 1

content:
  supported_types:
    - comic
    - webtoon

style:
  mode: concise
  meaning_preservation: high
  naturalness: high

dialogue:
  preserve_fragments: true
  preserve_character_voice: true
  avoid_explanatory_expansion: true

output:
  maximum_lines: 4
  maximum_characters: 120
  preserve_segment_count: true
  alternatives_on_ambiguity: false

validation:
  require_overflow_check: true
```

---

# OCR Profile Example

```yaml
profile_type: ocr
schema_version: 1

content_mode: comic

languages:
  expected:
    - zh-Hans
    - zh-Hant
  allow_mixed_scripts: true

orientation:
  mode: mixed

detection:
  speech_bubbles: true
  narration_boxes: true
  sound_effects: optional
  interface_text: ignore

confidence:
  auto_accept: 0.92
  require_review: 0.65
  reject_below: 0.35

reading_order:
  mode: automatic
  fallback: top_to_bottom

output:
  region_geometry: required
  line_geometry: required
  alternatives: true
```

---

# Presentation Profile Example

```yaml
profile_type: presentation
schema_version: 1

content_mode: comic
display_mode: overlay

typography:
  preferred_fonts:
    - Noto Sans
    - system-ui
  minimum_font_size: 12
  maximum_font_size: 28
  line_height: 1.2
  alignment: center

overlay:
  cover_original_text: true
  preserve_bubble_background: true
  padding: medium
  low_confidence_indicator: true

overflow:
  strategy_order:
    - wrap
    - reduce_font_size
    - external_caption
    - require_review
```

---

# Validation Profile Example

```yaml
profile_type: validation
schema_version: 1

rules:
  language_mismatch:
    enabled: true
    severity: error

  glossary_required_term:
    enabled: true
    severity: error
    block_approval: true

  character_name_mismatch:
    enabled: true
    severity: error

  pronoun_inconsistency:
    enabled: true
    severity: warning

  spoiler_leak:
    enabled: true
    severity: blocking

  presentation_overflow:
    enabled: true
    severity: warning
    block_export: true
```

---

# Context Profile Example

```yaml
profile_type: context
schema_version: 1

sources:
  current_text_block: required
  neighboring_text_blocks:
    previous: 6
    next: 2
  current_page: true
  glossary_snapshot: required
  character_context_snapshot: required
  previous_translation: true
  chapter_summary: optional

spoilers:
  boundary: reader_progress
  future_identity_reveal: forbidden

budget:
  priority:
    - current_source
    - glossary
    - character_context
    - immediate_dialogue
    - current_page
    - chapter_summary
  overflow_strategy: drop_lowest_priority
```

---

# Routing Profile Example

```yaml
profile_type: routing
schema_version: 1

quality_tier: standard
cost_tier: balanced
latency_tier: interactive
privacy_tier: approved_providers_only

requirements:
  structured_output: true
  chinese_to_vietnamese: true

fallback:
  order:
    - another_model_same_provider
    - another_approved_provider
    - local_model
  lower_quality_without_confirmation: false

budget:
  maximum_estimated_cost_per_operation: configured
```

---

# Profile Resolution Example

Workspace default:

```text
Translation Profile Revision W4
```

Project override:

```text
Translation Profile Revision P7
```

Session override:

```text
Maximum output lines = 4
```

Workspace Policy:

```text
Cloud processing forbidden
```

Resolved result:

```text
Translation semantics:
From P7

Maximum output lines:
4

Processing restriction:
Local only

Resolved Profile Snapshot:
RPS-42
```

The operation references `RPS-42`.

---

# Profile Update Example

Current Project selection:

```text
Translation Profile Revision 7
```

A new revision is approved:

```text
Translation Profile Revision 8
```

Possible behavior:

* Existing Sessions remain pinned to Revision 7.
* New Sessions use Revision 8.
* User may explicitly upgrade an active Session.
* Existing Translations remain unchanged.
* CRAI calculates semantic impact.
* Affected content may receive retranslation recommendations.

---

# Session Override Example

Selected Presentation Profile:

```text
Revision 3
Font size: 18
```

User temporarily increases Session font scale:

```text
Font size override: 22
```

CRAI creates a resolved presentation snapshot.

The original Presentation Profile Revision remains unchanged.

The override may later be promoted into a new User Profile Revision.

---

# Profile Fork Example

Workspace Profile:

```text
Natural Vietnamese Novel
Revision 9
```

Project needs stronger classical register.

Project forks Revision 9:

```text
New Profile:
Classical Cultivation Novel

Revision 1:
Base = Workspace Profile Revision 9
Overrides:
- narration register = classical
- honorific preservation = strict
```

Future Workspace Profile changes do not silently alter the Project fork.

---

# Provider Routing Example

Routing Profile requests:

```text
Quality:
High

Privacy:
Approved providers only

Latency:
Interactive
```

Routing service evaluates:

* Workspace policy
* Available provider configurations
* Language support
* Model capabilities
* Current quota
* Provider health
* Cost

The Profile does not select a provider directly unless an explicit allowlist is part of its intent.

---

# Architecture Invariants

1. Profile is a reusable configuration Aggregate Root.
2. Profile ID is different from Profile Revision ID.
3. Profile identity remains stable across revisions.
4. Profile Revisions are immutable.
5. Durable operations reference exact Profile Revisions or resolved snapshots.
6. Active Revision is a selection hint, not historical identity.
7. Profile is separate from User Preference.
8. Profile is separate from Workspace Policy.
9. Profile is separate from Provider Configuration.
10. Profile is separate from Runtime Operation state.
11. Profile is not a raw provider prompt.
12. Core Profile configuration is provider-neutral.
13. Provider credentials never belong to Profile.
14. Provider-specific hints are optional and namespaced.
15. Unsupported provider hints do not redefine core semantics.
16. Each Profile Type has an explicit schema.
17. Different Profile Types remain separate aggregates or typed instances.
18. CRAI does not rely on one universal Profile.
19. Composite Profiles reference exact Profile Revisions.
20. Profile composition is resolved before operation execution.
21. Every operation uses an immutable Resolved Profile Snapshot where overrides or inheritance apply.
22. Resolution records source revisions and applied overrides.
23. Workspace policy may restrict but is not copied into mutable Profile truth.
24. Narrower Profile configuration cannot override mandatory policy.
25. Profile inheritance pins exact base revisions.
26. Profile dependency graphs are acyclic.
27. Derived Profile Revisions preserve lineage.
28. Profile clones receive independent identities.
29. Profile forks preserve source lineage.
30. Profile ownership is separate from applicability.
31. Profile visibility does not imply permission.
32. Workspace membership alone does not grant Profile editing permission.
33. Profile schema versions remain interpretable historically.
34. Schema migration never rewrites used Profile Revisions.
35. Semantic Profile changes create new revisions.
36. Profile changes do not rewrite existing Translation Revisions.
37. Profile changes only stale artifacts when relevant semantic inputs changed.
38. Presentation Profile changes do not alter Translation truth.
39. Routing Profile changes do not alter completed Translation semantics.
40. Validation Profile changes create new validation results.
41. OCR Profile changes do not rewrite prior OCR results.
42. Session overrides do not mutate Profiles.
43. Useful Session overrides require explicit promotion.
44. AI-generated Profile Candidates are not canonical until reviewed.
45. User corrections do not silently alter approved Profiles.
46. Profile import never silently overwrites approved revisions.
47. Profile export excludes secrets.
48. Used Profile Revisions are not hard deleted.
49. Cache keys use revision or resolved snapshot hashes, not Profile ID alone.
50. Every significant Profile lifecycle and approval action is auditable.
51. Compatibility is validated before Profile use.
52. Language compatibility uses canonical Language values.
53. Context-affecting Profiles respect spoiler boundaries.
54. Provider routing must respect Workspace policy, entitlement and quota.
55. Profile evaluation results remain derived data.
56. Profile recommendations resolve to exact revisions before execution.

---

# Open Decisions

The following decisions should remain open until implementation and prototype testing:

* Whether every Profile Type uses one shared aggregate implementation
* Whether each Profile Type has a dedicated aggregate
* Whether Profile configuration is stored as JSON, typed columns or both
* Whether Profile Revisions are event-sourced
* Whether Profile metadata changes require revisions
* Which metadata fields affect content hash
* Whether descriptions are revisioned
* Whether Active Revision must always be approved
* Whether several revisions can be Active
* Whether Profiles have a separate Recommended Revision
* Whether Draft Revisions may be used in normal Sessions
* Whether System Profiles may be hidden
* Whether users may edit System Profiles through forking only
* Whether Personal Profiles can be used in Team Workspaces
* Whether Workspace administrators can inspect User Profiles
* Whether Project Profiles may be shared outside their Project
* Whether Profile visibility includes Public in MVP
* Whether Profiles support Shared Link
* Whether custom Profile Types are allowed
* Whether extensions are allowed in MVP
* How extension namespaces are registered
* Whether provider hints are supported
* Whether raw provider parameters are ever permitted
* Whether Composite Profile is a core domain type
* Whether Composite Profile references exact revisions only
* Whether Composite Profile may follow latest approved revisions
* Whether Profiles support multiple inheritance
* Whether inheritance is limited to one base
* Whether derived revisions store flattened configuration
* Whether clones retain lineage
* Whether forks retain upstream update notifications
* Whether semantic diff is manually configured or schema-derived
* How Profile change impact is calculated
* Whether impact classification is stored on Revision
* Whether Project Profile updates automatically mark Translations stale
* Whether active Sessions automatically adopt new Profile Revisions
* Whether Session profile upgrades fork the Session
* Whether Profile upgrades apply at Chapter boundaries
* Whether Profile selection is stored directly or through defaults
* Whether Book and Chapter can own Profiles
* Whether TextBlock-level Profile overrides are supported
* Whether field-level operation overrides are supported
* Which fields may be overridden at Session scope
* Whether mandatory Workspace Profile fields exist
* How Profile and Workspace Policy conflicts are presented
* Whether Policy Resolution is included in Resolved Profile Snapshot
* Whether Resolved Profile Snapshots are persisted permanently
* Whether snapshots are content-addressed and deduplicated
* Whether resolution traces are retained long term
* Whether Profile test cases are part of Profile aggregate
* Whether Profile approval requires passing tests
* Whether Profile evaluation supports automatic promotion
* Whether A/B experiments are supported in MVP
* Whether user ratings affect recommendations
* Whether AI may infer Profile Candidates from corrections
* How many corrections are required before a recommendation
* Whether inferred Profiles may include character-specific rules
* Whether Character Speech Profile remains entirely in Character domain
* Whether Translation Profile may override Character speech behavior
* Whether Context Profile is independent from Translation Profile
* Whether Validation Profile references Translation Profile constraints
* Whether Presentation overflow may trigger automatic retranslation
* Whether concise retranslation requires a new Translation Profile
* Whether OCR preprocessing settings belong in OCR Profile or pipeline configuration
* Whether Capture Profile is required
* Whether Routing Profile belongs in domain or application configuration
* Whether Cost Profile should be separate from Routing Profile
* Whether Privacy Profile should be separate from Workspace Policy
* Whether Export Profile is required for MVP
* Whether Review Profile is required
* Whether Notification Profile belongs in Workspace settings
* Whether Profiles may reference Glossary IDs
* Whether Profiles should reference exact Glossary Revisions
* Whether terminology behavior belongs only in Translation Profile
* Whether Context Profile controls Glossary inclusion
* How Profile compatibility ranges are represented
* Whether compatibility uses semantic version constraints
* Whether Profile schema migration runs automatically
* Whether old schemas remain executable indefinitely
* Whether unknown imported fields are preserved
* Whether Profiles can be imported with original IDs
* Whether duplicate imported Profile IDs are remapped
* Whether Profile packages include test datasets
* Whether public Profile packages require signatures
* Whether marketplace Profiles have licensing metadata
* Whether Profile exports include authorship history
* Whether Profile deletion is ever physically allowed
* How long unused Draft Revisions are retained
* Whether archived Profiles remain selectable by ID
* Whether Profile use is recorded for usage analytics
* Whether Profile use history is visible to administrators
* Whether Profile search indexes configuration fields
* Whether public Profiles may contain external links
* Whether Profile examples contain copyrighted source content
* Whether Workspace can require all operations to use approved Profiles
* Whether Service Accounts may create Profile Revisions
* Whether Profile Approval supports separation of duties
* Whether locked Profile Revisions can be deprecated
* Whether ownership transfer of Profile is supported
* Whether Project transfer copies or retains Workspace Profile references
* How Profile references behave during Workspace deletion
* Whether local and cloud installations share System Profile identities

---

# Recommended MVP Scope

The first CRAI MVP should support:

* Stable Profile identity
* Immutable Profile Revisions
* Profile Revision lineage
* Content hash
* Schema version
* Profile ownership by System, User, Workspace and Project
* Private and Workspace visibility
* Draft, Active, Deprecated and Archived lifecycle states
* Unreviewed, Approved and Rejected Review States
* Active Revision
* Exact Revision selection
* Translation Profile
* OCR Profile
* Presentation Profile
* Validation Profile
* Basic Context Profile
* Basic Routing Profile
* Structured provider-neutral schemas
* Source and target language compatibility
* Novel and Comic content compatibility
* Workspace default Profiles
* Project default Profiles
* Session Profile selection
* Session-level temporary overrides
* Operation-level resolved snapshots
* Resolution trace
* Workspace policy validation
* Profile cloning
* Profile forking
* One exact base Profile Revision
* Semantic revision diff
* Basic Profile validation
* Cross-field validation
* Import from JSON and YAML
* Export to JSON and YAML
* Import Plan
* No silent overwrite
* Profile audit events
* Profile permission checks
* Profile deprecation
* Retention of referenced revisions
* Basic Profile change impact classification
* Cache integration using resolved Profile hash

The MVP may defer:

* Public Profile marketplace
* Shared-link Profiles
* Custom Profile Types
* Arbitrary extension namespaces
* Multiple inheritance
* Dynamic follow-latest dependency chains
* Automatic Profile merge
* Real-time collaborative editing
* AI-generated Profile activation
* Automatic learning from corrections
* Advanced Profile recommendations
* A/B experiments
* Automated quality evaluation
* Provider-specific Profile editors
* Public package signatures
* License management
* Composite Profile marketplace packages
* Capture Profile
* Recognition Profile
* Audio Profile
* Speech Profile outside Character domain
* Advanced Export Profile
* Review Profile
* Accessibility Profile as a separate type
* Cost Profile as a separate type
* Privacy Profile as a separate type
* Advanced schema migration
* Cross-Workspace Profile sharing
* Organization-wide Profile federation
* Automatic Project migration to new revisions
* Automatic retranslation after Profile updates
* Multi-stage Profile approvals
* Separation-of-duties enforcement
* Version compatibility solvers
* Semantic package dependency resolution
* Profile usage billing
* Profile analytics dashboards
* Public rating and review systems

---

# Related Documents

* `README.md`
* `WORKSPACE.md`
* `PROJECT.md`
* `BOOK.md`
* `CHAPTER.md`
* `PAGE.md`
* `IMAGE.md`
* `TEXT_BLOCK.md`
* `TRANSLATION.md`
* `LANGUAGE.md`
* `GLOSSARY.md`
* `CHARACTER.md`
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
* `docs/architecture/presentation/FONTS.md`
* `docs/architecture/security/AUTHORIZATION.md`
* `docs/architecture/runtime/JOB.md`
* `docs/architecture/runtime/QUEUE.md`
* `docs/architecture/integration/PROVIDER.md`
* `docs/architecture/operations/USAGE.md`
* `docs/architecture/operations/QUOTA.md`
