# Profile Domain

* **Document:** Domain / Profile
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

The `Profile` domain defines reusable, versioned and provider-neutral behavioral configuration used by CRAI capabilities.

A Profile represents **intent**.

Examples:

* translate Chinese novels into natural Vietnamese,
* preserve cultivation terminology,
* prefer concise comic dialogue,
* detect vertical comic text,
* present Translation beside original content,
* require strict terminology validation,
* prefer local execution,
* optimize draft work for lower cost,
* prefer higher quality for publication output.

A Profile describes:

```text
What behavior is desired
```

rather than:

```text
How one provider implements that behavior
```

Profiles MUST remain independent from:

* provider request payloads,
* provider credentials,
* concrete model identifiers,
* runtime jobs,
* retry attempts,
* prompts,
* environment variables,
* deployment settings,
* UI form state,
* billing accounts.

---

# Domain Role

Profile is a shared configuration domain.

Conceptually:

```text
Profile Revision(s)
        |
        v
Configuration Resolution
        |
        +--> Preferences
        +--> Scope Defaults
        +--> Operation Overrides
        +--> Mandatory Policies
        +--> Capability Constraints
        |
        v
Resolved Configuration Snapshot
        |
        v
Capability Execution
```

A capability consumes resolved immutable configuration.

It SHOULD NOT repeatedly read mutable Profile state during execution.

---

# Profile Is Intent

A Profile expresses reusable desired behavior.

Example:

```text
Translation Profile:
    natural Vietnamese
    preserve relationship hierarchy
    preserve ambiguity
```

It does NOT define:

```text
provider = X
model = Y
temperature = 0.7
API key = ...
```

unless an explicitly supported provider preference is represented as non-binding intent.

Concrete provider execution remains the responsibility of Routing and provider adapters.

---

# Profile Is Not User Preference

User Preference describes general user choices.

Examples:

```text
dark mode
default reader font size
default selected Translation Profile
```

Profile describes reusable processing behavior.

Example:

```text
Translation Profile:
    natural Vietnamese
    relationship-aware pronouns
```

A User Preference MAY select a Profile.

It MUST NOT replace Profile semantics.

---

# Profile Is Not Policy

Profile describes desired behavior.

Policy defines mandatory constraints.

Example:

```text
Profile:
    external processing preferred
```

```text
Policy:
    external processing forbidden
```

The Profile MUST NOT override the Policy.

Resolution MUST either:

* produce an allowed compatible configuration,
* request an explicit alternative,
* or reject the operation.

---

# Profile Is Not Provider Configuration

Provider Configuration MAY contain:

* provider identity,
* credential reference,
* region,
* available models,
* rate limits,
* runtime capabilities.

Profile contains provider-neutral intent.

Example:

```text
Routing intent:
    high quality
    interactive latency
    approved providers only
```

Runtime Routing MAY resolve that to a concrete provider/model.

That decision is not Profile identity.

---

# Profile Is Not Prompt

Profile configuration MAY influence prompt construction.

Recommended boundary:

```text
Profile / Resolved Configuration
            |
            v
Context Compiler
            |
            v
Request / Prompt Compiler
            |
            v
Provider Adapter
```

Raw prompt strings are derived execution artifacts.

They MUST NOT become canonical Profile configuration.

---

# Profile Is Not Runtime State

Profile does NOT own:

* execution status,
* queue state,
* provider attempt count,
* timeout attempts,
* token usage,
* latency,
* provider response,
* runtime error,
* fallback attempt state.

A Profile MAY express intent such as:

```text
latencyTier: INTERACTIVE
```

but does not track whether that target was achieved.

---

# Core Concepts

The Profile domain SHOULD distinguish:

```text
Profile
ProfileRevision
ProfileSchema
ProfileApplicability
ProfileSelection
ProfileDefaultSelection
ProfileReview
ProfileAuthority
ProfileCandidate
ProfileImportPlan
ResolvedProfileSnapshot
ResolvedConfigurationSnapshot
ProfileImpact
```

These concepts SHOULD NOT be collapsed into one stateful object.

---

# Aggregate Boundary

`Profile` is an Aggregate Root with stable identity.

Recommended aggregate:

```text
Profile
├── profileId
├── profileType
├── ownerScope
├── displayName
├── description?
├── visibility
├── lifecycleStatus
├── activeRevisionId?
├── createdAt
├── updatedAt
└── version
```

Profile owns:

* Profile identity,
* Profile type,
* ownership scope,
* discovery metadata,
* lifecycle,
* active/recommended Revision references.

Profile does NOT transactionally own:

* operations,
* Sessions,
* Projects,
* Translation artifacts,
* OCR artifacts,
* provider routing results,
* Review workflow,
* runtime snapshots.

---

# Stable Identity

Critical invariant:

```text
profileId != profileRevisionId
```

`profileId` identifies the reusable configuration concept.

`profileRevisionId` identifies one immutable behavioral definition.

Example:

```text
Profile:
    Natural Vietnamese Novel Translation

Revision 1:
    style: natural
    honorificPolicy: preserve

Revision 2:
    style: natural
    honorificPolicy: relationship-aware
```

Both revisions belong to one Profile identity.

---

# Profile Revision

A Profile Revision represents one immutable configuration definition.

Recommended structure:

```text
ProfileRevision
├── profileRevisionId
├── profileId
├── revisionNumber
├── parentRevisionId?
├── profileType
├── schemaVersion
├── configuration
├── compatibility
├── contentHash
├── changeSummary?
├── createdBy
├── createdAt
└── lineageMetadata?
```

Published or externally referenced Profile Revisions MUST be immutable.

Semantic correction creates another Revision.

---

# Revision Immutability

Once a Profile Revision has been consumed by a durable operation, it MUST NOT change.

Without this rule:

* Translation becomes unreproducible,
* cache keys become unstable,
* retries may produce different behavior,
* historical audit becomes ambiguous,
* Session recovery may change semantics.

Durable processing therefore pins exact Profile Revision identities or an exact resolved configuration snapshot.

---

# Profile Type

Profiles MUST be capability-coherent rather than universal.

Recommended core types:

```text
TRANSLATION
OCR
PRESENTATION
VALIDATION
CONTEXT
ROUTING
EXPORT
```

Possible future types:

```text
CAPTURE
IMPORT
RECOGNITION
ACCESSIBILITY
REVIEW
QUALITY
```

Each Profile Type MUST have:

* its own schema,
* validation rules,
* compatibility rules,
* semantic impact rules.

---

# Avoid Universal Profile

CRAI MUST NOT create one Profile containing every configurable behavior.

Bad:

```text
MegaProfile
├── Translation
├── OCR
├── Rendering
├── Routing
├── Export
├── Validation
└── Privacy
```

This creates:

* unclear ownership,
* unnecessary coupling,
* broad invalidation,
* oversized revisions,
* difficult reuse,
* difficult permissions.

Recommended:

```text
TranslationProfileRevision
OCRProfileRevision
PresentationProfileRevision
ValidationProfileRevision
ContextProfileRevision
RoutingProfileRevision
```

These may be composed during operation resolution.

---

# Profile Composition

One operation MAY consume several Profile Revisions.

Example:

```text
Translation Operation

Translation Profile Revision 7
Context Profile Revision 3
Validation Profile Revision 5
Routing Profile Revision 2
```

For image content:

```text
OCR Profile Revision 4
Presentation Profile Revision 8
```

Composition occurs during configuration resolution.

One Profile MUST NOT gain ownership of another Profile merely because both are used together.

---

# Composite Profile

A `CompositeProfile` MAY exist for user convenience.

It SHOULD contain explicit references or selection policies.

Example:

```text
CompositeProfileRevision
├── translationSelection
├── ocrSelection
├── presentationSelection
├── validationSelection
├── contextSelection
└── routingSelection
```

It MUST NOT copy mutable Profile state implicitly.

At operation start, all selection policies MUST resolve to exact immutable revisions.

---

# Profile Ownership

A Profile MAY be owned by:

```text
SYSTEM
USER
WORKSPACE
PROJECT
```

Possible future ownership:

```text
ORGANIZATION
EXTERNAL_PACKAGE
```

Ownership describes:

```text
who controls the Profile
```

It does NOT define:

```text
where the Profile applies
```

---

# Ownership vs Applicability

Ownership and applicability MUST remain separate.

Example:

```text
Workspace-owned Translation Profile

Applicable to:
    Project A
    Project B
    zh-* -> vi
```

Recommended applicability:

```text
ProfileApplicability
├── projectIds?
├── bookIds?
├── chapterIds?
├── contentTypes?
├── sourceLanguageRanges?
├── targetLanguageRanges?
├── sessionTypes?
├── capabilityTypes?
├── classificationRestrictions?
└── exclusions?
```

Book and Chapter MUST remain optional.

Page or TextBlock applicability MAY be introduced only when required.

---

# Visibility

Possible visibility:

```text
PRIVATE
WORKSPACE
RESTRICTED
SHARED
PUBLIC
SYSTEM
```

Visibility controls discoverability.

It MUST NOT by itself imply permission to:

* edit,
* approve,
* use,
* export,
* clone.

Authorization is handled separately.

---

# Profile Lifecycle

Profile lifecycle describes availability of the reusable Profile identity.

Recommended lifecycle:

```text
DRAFT
ACTIVE
DEPRECATED
ARCHIVED
```

Optional:

```text
DELETING
DELETED
```

Only when deletion semantics require explicit states.

---

# Lifecycle Semantics

`DRAFT`

Profile exists but is not normally selected for production use.

`ACTIVE`

Profile is available for normal selection.

`DEPRECATED`

Profile remains valid for historical use but SHOULD NOT be selected by default for new operations.

`ARCHIVED`

Profile remains historically resolvable but is hidden from ordinary selection.

---

# What Is Not Profile Lifecycle

The following MUST NOT be Profile lifecycle states:

```text
CANDIDATE
REJECTED
IMPORTED
LOCKED
APPROVED
SUPERSEDED
```

They describe other concepts:

```text
CANDIDATE
    -> ProfileCandidate workflow

REJECTED / APPROVED
    -> Review decision

IMPORTED
    -> provenance

LOCKED
    -> authority/governance

SUPERSEDED
    -> Revision lineage
```

---

# Profile Revision Status

A Profile Revision MAY have publication state distinct from Profile lifecycle.

Possible revision status:

```text
DRAFT
PUBLISHED
SUPERSEDED
WITHDRAWN
```

This status SHOULD NOT replace Review state.

Example:

```text
Profile:
    ACTIVE

Revision 8:
    PUBLISHED + APPROVED

Revision 9:
    DRAFT + IN_REVIEW
```

---

# Review

Review applies to exact Profile Revisions.

Recommended Review states:

```text
UNREVIEWED
REVIEW_REQUESTED
IN_REVIEW
CHANGES_REQUESTED
APPROVED
REJECTED
```

Review state is separate from:

* Profile lifecycle,
* Revision publication status,
* lock/authority.

---

# Authority and Lock

Lock is governance state.

Recommended structure:

```text
ProfileAuthority
├── profileId
├── profileRevisionId?
├── restrictedActions[]
├── scope
├── authorityLevel
├── actor
├── createdAt
└── reason?
```

Lock MAY restrict:

* replacement,
* deprecation,
* cloning,
* export,
* use outside approved scope,
* administrative deletion.

Because Revision is already immutable, lock MUST NOT imply in-place mutation protection as its primary meaning.

---

# Profile Candidate

Profile Candidate represents a suggested configuration.

Possible sources:

* AI inference,
* user behavior,
* imported presets,
* experiment results,
* correction patterns.

Candidate is NOT canonical Profile state.

Recommended flow:

```text
Candidate
   |
   v
Review
   |
   v
Edit / Validate
   |
   v
Create Profile Revision
   |
   v
Approve
   |
   v
Publish / Activate
```

AI MUST NOT silently change an approved Profile.

---

# Schema

Every Profile Type has an explicit schema.

Example:

```text
translation.profile.schema.v1
ocr.profile.schema.v2
presentation.profile.schema.v1
```

Every Revision stores its schema version.

Schema validation MUST occur before publication or normal use.

---

# Schema Evolution

Schema changes MAY include:

* new optional fields,
* renamed fields,
* new enums,
* split fields,
* removed behavior,
* default changes.

Historical Revisions MUST remain interpretable.

Migration MUST NOT silently rewrite referenced historical Revisions.

Migration SHOULD create:

* a new Revision,
* or a normalized execution representation.

---

# Unknown Fields

Core schemas SHOULD reject unknown unregistered fields.

Extensions MAY use explicit namespaces.

Example:

```text
extensions:
    vendor.example:
        option: value
```

Unknown extensions MUST NOT silently alter core behavior unless a registered capability explicitly understands them.

---

# Translation Profile

Translation Profile defines semantic Translation intent.

Possible areas:

```text
TranslationProfile
├── languageStrategy
├── style
├── meaningPreservation
├── naturalness
├── terminologyPolicy
├── namePolicy
├── honorificPolicy
├── pronounPolicy
├── localizationPolicy
├── ambiguityPolicy
├── dialoguePolicy
├── narrationPolicy
├── semanticFormattingPolicy
├── qualityTarget
└── outputConstraints
```

Translation Profile MUST NOT own:

* Glossary Entries,
* Character facts,
* provider prompts,
* Translation results.

---

# Translation Style

Possible styles MAY include:

```text
LITERAL
FAITHFUL
NATURAL
LOCALIZED
LITERARY
CONCISE
CONVERSATIONAL
COMIC
SUBTITLE
CUSTOM
```

A style label alone SHOULD NOT be the complete behavioral definition.

Structured settings SHOULD define its actual semantics.

---

# Terminology Policy

Translation Profile MAY specify how resolved Glossary context should be used.

Examples:

* required terminology enforcement,
* preferred terminology,
* synonym policy,
* unknown-term handling,
* preserve rule handling,
* conflict behavior.

Profile does NOT own terminology data.

GlossarySnapshot supplies actual terminology.

---

# Character Policy

Translation Profile MAY specify how Character Context should influence Translation.

Examples:

* preserve relationship hierarchy,
* use Character Speech Profile,
* avoid unsupported gender assumptions,
* use relationship-aware address,
* preserve reveal-safe names,
* flag uncertain speaker-dependent language.

Profile MUST NOT encode individual mutable Character truth.

CharacterContextSnapshot supplies actual Character information.

---

# Formatting Boundary

Translation Profile MAY contain semantic output requirements such as:

```text
preserve paragraph boundaries
preserve segment count
avoid commentary
maximum semantic output length
structured alignment
```

Visual settings such as:

```text
font family
font size
line height
overlay geometry
```

belong to Presentation Profile.

---

# OCR Profile

OCR Profile defines provider-neutral detection and recognition intent.

Possible areas:

```text
OCRProfile
├── contentMode
├── expectedLanguages
├── expectedScripts
├── textOrientationPolicy
├── detectionPolicy
├── recognitionPolicy
├── preprocessingIntent
├── confidencePolicy
├── readingOrderPolicy
├── regionPolicy
└── outputRequirements
```

OCR Profile describes desired processing behavior.

It does NOT own OCR results or pipeline execution state.

---

# OCR Preprocessing Intent

Profile MAY express semantic intent such as:

```text
denoise
deskew
upscale
enhance contrast
preserve color
detect inverted text
```

Concrete algorithm parameters remain in processing configuration when they are implementation-specific.

---

# Presentation Profile

Presentation Profile defines display behavior.

Possible areas:

```text
PresentationProfile
├── contentMode
├── displayMode
├── typography
├── layout
├── overlayPolicy
├── sourceVisibility
├── translationVisibility
├── readingDirection
├── overflowPolicy
├── accessibility
├── theme
└── deviceAdaptation
```

Presentation Profile MUST NOT change Translation semantic truth.

---

# Typography

Typography intent MAY include:

* font family preference,
* fallback families,
* font size,
* min/max font size,
* weight,
* line height,
* spacing,
* alignment,
* vertical-text support.

Font binaries are NOT Profile data.

---

# Overflow Policy

Possible Presentation actions:

```text
WRAP
REDUCE_FONT_SIZE
EXPAND_REGION
CONDENSE_SPACING
SCROLL
POPUP
EXTERNAL_CAPTION
REQUIRE_REVIEW
REQUEST_CONCISE_RETRANSLATION
```

Presentation MUST NOT silently rewrite an approved Translation.

If semantic Translation text changes, a new Translation Revision is required.

---

# Validation Profile

Validation Profile defines validation behavior.

Possible fields:

```text
ValidationProfile
├── enabledRules
├── severityMapping
├── confidenceThresholds
├── blockingPolicy
├── reviewPolicy
├── autoFixPolicy
└── ruleScope
```

Validation Profile does NOT own validation results.

---

# Validation Auto-Fix

Auto-fix MUST be conservative.

Possible modes:

```text
DISABLED
SUGGEST_ONLY
SAFE_DETERMINISTIC
FORMATTING_ONLY
```

An automatic fix that changes Translation semantics MUST produce a new Translation Revision.

---

# Context Profile

Context Profile defines **context selection policy**.

Possible areas:

```text
ContextProfile
├── contextSources
├── sourceWindowPolicy
├── CharacterContextPolicy
├── GlossaryContextPolicy
├── storyContextPolicy
├── spoilerPolicy
├── memoryPolicy
├── priorTranslationPolicy
├── contextBudget
└── truncationStrategy
```

It MUST NOT contain the actual mutable context.

Execution produces immutable Context Snapshots.

---

# Optional Context Sources

Possible sources include:

* current TextBlock,
* neighboring TextBlocks,
* current Chapter,
* optional current Page,
* optional neighboring Pages,
* Glossary Snapshot,
* Character Context Snapshot,
* previous Translation,
* Session context,
* Chapter summary.

Page-dependent context MUST be optional because text-native content may not have Pages.

---

# Spoiler Policy

Possible policies:

```text
CURRENT_POSITION_ONLY
CURRENT_CHAPTER
PREVIOUSLY_READ_ONLY
PROJECT_APPROVED_KNOWLEDGE
FULL_PROJECT_CONTEXT
EXPLICIT_FUTURE_CONTEXT
```

Normal reading Translation SHOULD avoid future spoiler context by default.

---

# Routing Profile

Routing Profile defines **provider-neutral routing intent**.

Possible fields:

```text
RoutingProfile
├── capabilityRequirements
├── qualityTier
├── costPreference
├── latencyPreference
├── localityPreference
├── privacyIntent
├── providerPreference?
├── fallbackIntent
└── budgetIntent?
```

Routing Profile MUST NOT contain:

* credentials,
* concrete runtime attempt state,
* active rate-limit counters,
* queue state.

---

# Routing Intent vs Mandatory Policy

Routing Profile MAY say:

```text
external providers preferred
```

Workspace Policy MAY say:

```text
external providers forbidden
```

Mandatory Policy wins.

Therefore Routing Profile SHOULD express preferences, not authorization.

---

# Provider Allow / Deny

If provider allow/deny preferences are supported inside Routing Profile, they MUST be treated as user/configuration preference.

Mandatory provider restrictions belong to Policy.

Resolution computes the intersection.

Example:

```text
Profile allowed:
    A, B, C

Policy allowed:
    B, C, D

Effective:
    B, C
```

---

# Retry and Fallback Boundary

Profile MAY define high-level fallback intent:

```text
allow alternate provider
allow lower quality
allow local fallback
stop on semantic degradation
```

Concrete retry attempt counts, backoff timers and provider sequencing belong to runtime execution.

---

# Export Profile

Export Profile defines reusable export intent.

Possible fields:

```text
ExportProfile
├── format
├── includedContent
├── revisionSelectionPolicy
├── layoutPreference
├── metadataPolicy
├── spoilerPolicy
├── watermarkPolicy
├── namingPolicy
├── packagingPolicy
└── compatibilityTarget
```

Actual Export execution remains outside Profile.

---

# Inheritance

Profile inheritance MAY be supported.

Recommended MVP rule:

```text
one exact base Profile Revision
+
explicit overrides
=
new derived Profile Revision
```

Requirements:

* exact base Revision pinned,
* no circular dependency,
* flattened resolved configuration available,
* content hash reproducible.

---

# Clone vs Inherit

Clone:

```text
Revision A
    |
    v
new independent Profile
```

Future changes are independent.

Inherit:

```text
Exact Revision A
+
Overrides
    |
    v
Derived Revision B
```

Follow-latest:

```text
latest approved Revision of A
```

is a selection policy, NOT stable execution identity.

It MUST resolve to an exact Revision before operation start.

---

# Configuration Resolution

Configuration resolution combines intent and constraints.

Conceptual inputs:

```text
Application Defaults

System Profile Selection

User Preference / Default Selection

Workspace Profile Selection

Project Profile Selection

Optional Book / Chapter Selection

Session Selection / Override

Operation Override

Mandatory Policy

Capability Constraints
```

The order MUST NOT be interpreted as "every layer always exists".

Missing optional scopes are skipped.

---

# Selection vs Field Override

CRAI MUST distinguish:

```text
select another Profile Revision
```

from:

```text
override selected fields
```

and from:

```text
mandatory Policy constraint
```

These have different provenance and validation semantics.

---

# Profile Selection

A scope MAY select Profile behavior through:

* exact Revision,
* active approved Revision,
* latest compatible approved Revision,
* default Profile,
* explicit user selection.

Any dynamic policy MUST resolve to an exact Revision before execution.

---

# Default Selection

A scope MAY define default Profile selection per Profile Type.

Example:

```text
Project Defaults
├── Translation
├── OCR
├── Presentation
├── Validation
├── Context
└── Routing
```

Changing defaults affects future resolution only.

Historical operations remain unchanged.

---

# Active Revision

A Profile MAY point to a preferred active Revision.

`activeRevisionId` means:

```text
preferred default candidate for new selection
```

It does NOT mean:

```text
automatically replace exact revisions already in use
```

---

# Pinning

A Project, Session or operation MAY pin exact Profile Revisions.

Pinning supports:

* publication consistency,
* long-running Translation,
* reproducible review,
* experiments,
* offline work.

Pinned references remain stable until explicitly changed.

---

# Resolved Profile Snapshot

`ResolvedProfileSnapshot` represents the immutable resolved configuration for **one Profile Type**.

Recommended structure:

```text
ResolvedProfileSnapshot
├── snapshotId
├── profileType
├── sourceProfileRevisionIds[]
├── appliedDefaults[]
├── appliedOverrides[]
├── appliedPolicyConstraints[]
├── resolvedConfiguration
├── schemaVersion
├── contentHash
├── resolutionTrace
└── createdAt
```

Example:

```text
Resolved Translation Profile Snapshot
```

or:

```text
Resolved OCR Profile Snapshot
```

---

# Resolved Configuration Snapshot

An operation MAY use several resolved Profile types.

Therefore CRAI SHOULD distinguish a higher-level:

```text
ResolvedConfigurationSnapshot
```

Recommended structure:

```text
ResolvedConfigurationSnapshot
├── snapshotId
├── operationType
├── resolvedProfileSnapshotIds[]
├── policySnapshotReferences[]
├── capabilityResolutionReferences[]
├── operationOverrides[]
├── contentHash
├── resolutionTrace
└── createdAt
```

Example:

```text
Translation Operation

ResolvedConfigurationSnapshot
├── Resolved Translation Profile
├── Resolved Context Profile
├── Resolved Validation Profile
└── Resolved Routing Profile
```

This distinction avoids pretending one Profile Type contains the whole operation configuration.

---

# Resolution Trace

Resolution MUST be explainable.

Example:

```text
translation.style:
    natural
    <- Project Translation Profile Revision 12

honorificPolicy:
    relationship-aware
    <- Workspace Translation Profile Revision 4

maximumCharacters:
    120
    <- Operation Override

externalProcessing:
    false
    <- Workspace Policy Revision 9
```

Resolution Trace is derived provenance.

---

# Policy Snapshot

Mandatory policy that materially affected execution SHOULD be captured by exact revision/reference.

Historical operation reproducibility MUST NOT depend on mutable "current Workspace policy".

---

# Capability Validation

Resolved configuration MUST be validated against runtime capabilities.

Example:

```text
OCR Profile:
    requires region geometry
```

A runtime/provider incapable of returning geometry is incompatible.

Profile remains valid domain intent even if no currently configured provider can satisfy it.

Execution resolution may fail with a compatibility error.

---

# Compatibility

Profile Revision MAY declare compatibility against:

* Language Range,
* target Language,
* content type,
* capability,
* Project type,
* Session type,
* schema version,
* required context,
* Presentation requirements.

Compatibility is declarative intent.

Actual runtime availability remains external.

---

# Profile Validation

Profile Revision validation SHOULD include:

* schema validity,
* range validity,
* enum validity,
* required fields,
* Language compatibility,
* cross-field consistency,
* reference validity,
* inheritance validity,
* ownership validity,
* extension validation.

Runtime capability validation MAY additionally run at resolution time.

---

# Semantic Validation

Example conflicts:

```text
minimumFontSize > maximumFontSize
```

```text
localOnly + cloudOnlyFallback
```

```text
preserveLineCount + unrestrictedParagraphRestructure
```

```text
strictGlossaryEnforcement
without Glossary context
```

```text
Character-aware pronouns
without Character context
```

Such contradictions SHOULD be rejected before normal use.

---

# Profile Test Case

Profile Revision MAY reference reusable test cases.

Recommended:

```text
ProfileTestCase
├── testCaseId
├── profileRevisionId
├── inputReference
├── contextReference
├── expectedProperties[]
├── prohibitedProperties[]
└── expectedFindings[]
```

Tests SHOULD often validate properties rather than one exact Translation string.

---

# Profile Evaluation

Evaluation is derived data.

Possible metrics:

* validation success,
* user acceptance,
* cost,
* latency,
* terminology consistency,
* layout success,
* reviewer acceptance.

Evaluation MUST NOT mutate Profile configuration automatically.

---

# Experimentation

A/B evaluation MAY compare Profile Revisions.

Promotion SHOULD follow:

```text
Experiment
    |
    v
Evaluation
    |
    v
Review
    |
    v
Approval
    |
    v
Default / Active Revision update
```

Experiment result MUST NOT silently alter production defaults.

---

# Profile Change Impact

Changes SHOULD be semantically classified.

Possible impacts:

```text
NONE
FUTURE_EXECUTION_ONLY
PRESENTATION_ONLY
VALIDATION_ONLY
CONTEXT_ONLY
ROUTING_ONLY
RETRANSLATION_RECOMMENDED
RETRANSLATION_REQUIRED
RE_OCR_RECOMMENDED
RE_OCR_REQUIRED
RE_EXPORT_RECOMMENDED
```

---

# Staleness

A new Profile Revision does NOT automatically make all previous artifacts stale.

Example:

```text
Translation used Profile Revision 4

current preferred Revision:
    5
```

This means only:

```text
Translation used an older configuration
```

It does NOT necessarily mean:

```text
Translation invalid
```

Actual staleness depends on semantic impact.

---

# Selective Impact

Examples:

```text
Presentation font changed
    -> PRESENTATION_ONLY
```

```text
Translation naturalness changed
    -> RETRANSLATION_RECOMMENDED
```

```text
Routing cost preference changed
    -> FUTURE_EXECUTION_ONLY
```

```text
OCR expected language changed
    -> RE_OCR_RECOMMENDED
```

Only dependent artifacts SHOULD be affected.

---

# Semantic Diff

Profile Revision diff SHOULD compare semantic fields, not just raw serialized JSON.

Example:

```text
Revision 7 -> Revision 8

Changed:
    literalness
    honorific policy
    maximum output lines

Unchanged:
    target language
    glossary policy
```

Impact classification MAY be derived from this semantic diff.

---

# Dependencies

Profile dependencies MUST be explicit.

Possible dependencies:

* base Profile Revision,
* required Context capability,
* Glossary context requirement,
* Character context requirement,
* font-family identifier,
* Validation Profile reference,
* capability requirement.

Hidden dependencies MUST NOT affect behavior.

---

# Dependency Graph

Profile dependency graph MUST be acyclic.

Example:

```text
CompositeProfile
├── TranslationProfile
├── ContextProfile
└── ValidationProfile
```

TranslationProfile MUST NOT indirectly depend back on the CompositeProfile.

---

# Profile Hash

Every immutable Profile Revision SHOULD have a canonical semantic hash.

Hash SHOULD include:

* Profile Type,
* schema version,
* normalized behavior configuration,
* exact base Revision,
* behavior-affecting compatibility.

It SHOULD exclude clearly non-semantic mutable discovery metadata.

---

# Resolved Snapshot Hash

Execution cache SHOULD prefer:

```text
Resolved Configuration Snapshot Hash
+
Source Revision Hash
+
Context Snapshot Hashes
+
Capability / Pipeline Version
```

rather than mutable Profile ID.

---

# Session Integration

Session MAY select:

* exact Profile Revisions,
* selection policies,
* temporary overrides.

Temporary Session overrides MUST NOT mutate Profile.

They contribute to resolved immutable configuration.

---

# Project Integration

Project MAY:

* own Project Profiles,
* select shared Profiles,
* pin exact Revisions,
* define defaults,
* define selection policies.

Project MUST NOT embed complete Profile runtime state.

---

# Book and Chapter Integration

Optional Book or Chapter scope MAY provide explicit overrides or selections.

Examples:

* Chapter uses vertical OCR,
* one arc uses another naming policy,
* one Book uses different presentation.

These are explicit scope selections.

Book/Chapter MUST NOT be mandatory for Profile resolution.

---

# Translation Integration

Translation Revision SHOULD preserve the immutable configuration actually used.

Preferred reference:

```text
ResolvedConfigurationSnapshot
```

and where useful:

```text
ResolvedTranslationProfileSnapshot
ResolvedContextProfileSnapshot
ResolvedValidationProfileSnapshot
```

Historical Translation MUST remain reproducible after Profiles change.

---

# OCR Integration

OCR execution/results SHOULD preserve:

* resolved OCR configuration identity,
* relevant pipeline version,
* source Image identity/version,
* recognition version.

Profile does NOT own OCR result lifecycle.

---

# Presentation Integration

Presentation output SHOULD preserve:

* exact Presentation configuration,
* Translation Revision,
* source geometry/layout version,
* rendering version,
* font resolution metadata where required.

Changing Presentation Profile MUST NOT alter Translation truth.

---

# Validation Integration

Validation results SHOULD reference:

* exact resolved Validation configuration,
* validated artifact Revision,
* rule revisions,
* relevant context snapshots.

Changing Validation Profile creates new validation interpretation.

It MUST NOT rewrite historical findings.

---

# Provider Integration

Provider Adapter receives normalized execution intent.

```text
Resolved Configuration
        |
        v
Capability Request Model
        |
        v
Provider Adapter
        |
        v
Provider-Specific Parameters
```

Provider-specific parameter values remain outside canonical Profile definitions unless explicitly modeled as opaque optional hints.

---

# Import

Import MUST create a reviewable plan.

Recommended:

```text
ProfileImportPlan
├── sourceFormat
├── detectedProfileType
├── schemaVersion
├── proposedOwnerScope
├── proposedProfile
├── proposedRevisions[]
├── unsupportedFields[]
├── conflicts[]
├── requiredMigrations[]
└── validationFindings[]
```

Import provenance MUST NOT become Profile lifecycle.

---

# Export

Profile export SHOULD preserve:

* Profile identity where allowed,
* exact Revisions,
* Profile Type,
* schema version,
* configuration,
* lineage,
* compatibility,
* content hash.

Credentials and provider secrets MUST NEVER be exported as Profile data.

---

# Fork

Fork creates a new Profile identity from an existing Revision.

```text
Source Revision
     |
     v
New Profile ID
New Revision 1
```

Fork SHOULD preserve lineage where privacy/licensing permits.

---

# Concurrency

Profile editing SHOULD use optimistic concurrency.

Possible checks:

```text
expectedProfileVersion
expectedActiveRevisionId
expectedParentRevisionId
contentHash
```

Published Revisions MUST never be mutated during merge.

Concurrent changes MAY produce parallel draft Revisions.

---

# Deletion

Referenced Profile Revisions MUST NOT normally be physically deleted.

Preferred actions:

* deprecate,
* archive,
* hide,
* tombstone Profile identity.

Historical operations MUST remain able to resolve exact references.

---

# Retention

Durable retention SHOULD prioritize:

* referenced Revisions,
* approved Revisions,
* Profile lineage,
* resolved snapshots referenced by artifacts,
* Review records,
* audit records.

Temporary Candidates and failed imports MAY have shorter retention.

---

# Security

Possible permissions include:

```text
profile.view
profile.use
profile.create
profile.edit
profile.review
profile.approve
profile.activate
profile.deprecate
profile.archive
profile.clone
profile.import
profile.export
profile.lock
```

Authorization belongs to governance/security infrastructure.

Profile domain defines the resource and scope against which permission is evaluated.

---

# Events

Core Profile domain events MAY include:

```text
ProfileCreated
ProfileMetadataUpdated
ProfileActivated
ProfileDeprecated
ProfileArchived

ProfileRevisionCreated
ProfileRevisionPublished
ProfileDefaultChanged

ProfileForked
ProfileCloned
```

Review/governance events MAY include:

```text
ProfileRevisionSubmittedForReview
ProfileRevisionApproved
ProfileRevisionRejected
ProfileAuthorityChanged
```

Workflow events MAY include:

```text
ProfileCandidateCreated
ProfileCandidatePromoted
ProfileImported
ResolvedProfileSnapshotCreated
ResolvedConfigurationSnapshotCreated
ProfileImpactAssessed
```

Not every Profile-related event belongs to the Profile Aggregate.

---

# Persistence

Recommended canonical records:

```text
Profile
ProfileRevision
ProfileApplicability
ProfileDefaultSelection
ProfileAuthority
ProfileReview
ProfileForkLineage
ProfileTombstone
```

Resolved reproducibility records:

```text
ResolvedProfileSnapshot
ResolvedConfigurationSnapshot
```

Workflow/derived records:

```text
ProfileCandidate
ProfileImportPlan
ProfileEvaluation
ProfileTestCase
ProfileTestResult
ProfileRecommendation
ProfileUsageProjection
ProfileSearchIndex
```

---

# Architecture Invariants

1. `profileId` and `profileRevisionId` are different identities.

2. Profile represents reusable provider-neutral behavior intent.

3. Profile MUST NOT contain provider credentials.

4. Profile MUST NOT own runtime execution state.

5. Raw provider prompts MUST NOT be canonical Profile state.

6. Published or referenced Profile Revisions are immutable.

7. Durable executions reference exact Revisions or immutable resolved snapshots.

8. Profile Types remain capability-coherent.

9. CRAI MUST NOT create one universal Profile containing all behavior.

10. Ownership and applicability are distinct.

11. Optional Book/Chapter hierarchy levels MUST NOT be required for Profile resolution.

12. Profile lifecycle is separate from Revision publication state.

13. Review state is separate from Profile lifecycle.

14. Authority/Lock is separate from Review.

15. Candidate is a workflow artifact, not Profile lifecycle.

16. Import provenance is not Profile lifecycle.

17. Superseded is Revision lineage/status, not Review state.

18. User Preference may select Profiles but does not replace them.

19. Mandatory Policy overrides conflicting Profile intent.

20. Provider Configuration is separate from Profile.

21. Routing Profile expresses routing intent, not concrete runtime state.

22. Concrete retry and fallback attempts remain runtime-owned.

23. Translation Profile does not own Glossary Entries.

24. Translation Profile does not own Character facts.

25. Context Profile defines selection policy, not mutable context itself.

26. Presentation Profile MUST NOT mutate Translation semantics.

27. Validation semantic changes that modify Translation produce new Translation Revisions.

28. Dynamic Profile selection resolves to exact Revisions before execution.

29. `activeRevisionId` is a selection convenience, not execution identity.

30. Profile publication does not automatically alter existing Sessions or operations.

31. New Profile Revision does not automatically stale all old artifacts.

32. Staleness is based on semantic impact.

33. Semantic Profile diff SHOULD drive impact classification.

34. ResolvedProfileSnapshot represents one Profile Type.

35. ResolvedConfigurationSnapshot MAY compose several Profile Types for one operation.

36. Resolution MUST preserve provenance through Resolution Trace.

37. Mandatory policies that affect execution SHOULD be revision-addressable in resolved snapshots.

38. Runtime capability validation MUST remain separate from Profile business identity.

39. Profile dependency graph MUST be acyclic.

40. Inheritance MUST reference exact base Revisions.

41. Follow-latest is a selection policy and MUST resolve before execution.

42. Cache identity MUST NOT rely on mutable Profile ID alone.

43. Historical artifacts MUST preserve the effective configuration used.

44. Profile import MUST NOT silently overwrite approved configuration.

45. Referenced Profile Revisions MUST remain historically resolvable.

46. Provider secrets MUST NOT appear in Profile export or events.

47. Derived evaluation results MUST NOT silently mutate Profiles.

48. Automatic recommendations MUST NOT silently change production defaults.

---

# Recommended MVP Scope

The first CRAI MVP SHOULD support:

* stable Profile identity,
* immutable Profile Revisions,
* schema versioning,
* Translation Profile,
* OCR Profile,
* Presentation Profile,
* Context Profile,
* Validation Profile,
* basic Routing Profile,
* System defaults,
* Project-scoped Profiles,
* User/private Profiles where useful,
* exact Revision selection,
* Project default selection,
* optional Chapter overrides,
* Session overrides,
* operation overrides,
* Profile validation,
* Language compatibility,
* immutable ResolvedProfileSnapshot,
* immutable ResolvedConfigurationSnapshot,
* resolution trace,
* Profile content hashes,
* selective impact classification,
* semantic Profile diff,
* basic approval,
* authority lock,
* clone/fork,
* one-level exact Revision inheritance,
* JSON/YAML import/export,
* audit events.

MVP MAY defer:

* Composite Profile as a first-class domain type,
* multi-level inheritance,
* follow-latest inheritance,
* Workspace marketplace,
* public Profile sharing,
* A/B testing automation,
* Profile recommendations,
* AI-generated Profile Candidates,
* complex provider allow/deny preferences,
* Profile packages,
* signatures/licensing,
* Organization-level ownership,
* TextBlock-level overrides,
* full Profile merge UI,
* automatic Profile promotion,
* advanced Profile analytics.

---

# Open Decisions

The following SHOULD remain open until prototype validation:

* whether CompositeProfile is a first-class domain resource,
* whether Workspace ownership is required in MVP,
* exact relationship between User Preference and Profile defaults,
* whether Book-level Profile selection is required,
* whether TextBlock-level overrides are needed,
* which fields may be overridden at Session scope,
* whether individual field overrides are persisted,
* whether Routing Profile remains a Profile domain type,
* whether Privacy intent belongs partly in Routing Profile or exclusively in Policy,
* whether Cost/Quality become independent Profiles,
* whether Export Profile is MVP-critical,
* whether Context Profile remains independent from Translation Profile,
* whether Validation Profile references Translation intent,
* whether Presentation overflow can request concise retranslation automatically,
* whether OCR preprocessing intent belongs fully in OCR Profile,
* exact inheritance semantics,
* whether derived Revisions persist flattened configuration,
* how semantic Profile diff rules are represented,
* whether change impact is schema-derived or manually classified,
* ResolvedProfileSnapshot retention,
* ResolvedConfigurationSnapshot deduplication,
* long-term Resolution Trace retention,
* Profile test-case ownership,
* approval requirements,
* Candidate workflow,
* Profile schema migration lifetime,
* unknown extension handling,
* imported-ID behavior,
* Profile deletion semantics,
* ownership transfer,
* Profile synchronization across Workspaces/installations.

---

# Ownership Summary

```text
Profile Domain

Profile owns
    stable Profile identity
    Profile Type
    ownership
    lifecycle
    discovery metadata
    active/default Revision references

ProfileRevision owns
    immutable behavioral configuration
    schema version
    compatibility declaration
    revision lineage
    semantic content hash

ProfileApplicability owns
    where a Profile may be applied

ProfileDefaultSelection owns
    scope-level selection policy

ProfileReview owns
    approval decision for exact Revision

ProfileAuthority owns
    governance / lock restrictions

ResolvedProfileSnapshot owns
    immutable effective configuration
    for one Profile Type

ResolvedConfigurationSnapshot owns
    immutable operation-level composition
    across several Profile Types

ProfileCandidate owns
    unconfirmed suggested configuration

Runtime owns
    provider choice
    attempts
    retries
    queue state
    concrete fallback
    latency
    token/cost measurements

Policy owns
    mandatory constraints

User Preference owns
    general user choices
    default Profile selections
```

Profile is therefore the reusable intent/configuration domain, while resolved operation configuration, mandatory policy and runtime execution remain explicitly separated.

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
* `CHARACTER.md`
* `SESSION.md`
* `WORKSPACE.md`

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
* `docs/architecture/ai/PROMPTS.md`
* `docs/architecture/ai/ROUTING.md`
* `docs/architecture/ai/CACHE.md`

Presentation:

* `docs/architecture/presentation/LAYOUT.md`
* `docs/architecture/presentation/TYPOGRAPHY.md`
* `docs/architecture/presentation/FONTS.md`

Module contracts remain authoritative for execution behavior, routing, provider adapters, runtime state and capability-specific processing.
