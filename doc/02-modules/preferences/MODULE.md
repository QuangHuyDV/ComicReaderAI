# Preferences Module

> **Project:** CRAI
> **Module:** `preferences`
> **Path:** `doc/02-modules/preferences/MODULE.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-09

---

# 1. Module Definition

Preferences is the CRAI domain module responsible for defining, validating, storing, resolving, and versioning user-configurable preference semantics.

Its primary responsibility is:

```text
Preference Definitions
        +
Persistent Preference Values
        +
Source-specific Preference Values
        +
Externally supplied Session Overrides
        ↓
Preference Resolution
        ↓
Validated EffectivePreferences
        ↓
Immutable EffectivePreferencesSnapshot
```

Preferences answers:

> **What configurable behavior has the user selected, and what is the valid resolved preference set for a given context?**

Preferences does not answer:

> Which processing stages must now execute?

That belongs to Business Pipeline Orchestration.

Preferences also does not answer:

> How should processing execute?

That belongs to Runtime Control and the owning processing modules.

---

# 2. Module Identity

```text
Module ID: preferences
Module Type: Configuration Domain Module
Primary Domain: User-configurable application behavior
Primary Aggregate: PreferenceStore / PreferenceProfile
Primary Revision: PreferenceRevision
Primary Output: EffectivePreferencesSnapshot
Persistent Owner: Preferences semantics
Persistence Implementation: Storage
Execution Authority: None
Pipeline Decision Owner: Business Pipeline Orchestration
MVP Priority: Required
```

Preferences is not:

```text
Runtime Configuration
Business Pipeline Orchestrator
Reading Session
Provider Registry
Credential Store
Storage Backend
```

---

# 3. Architectural Position

The preferred architecture is:

```text
Application Defaults
        +
Global Preferences
        +
Source Preferences
        ↓
Preferences
        ↓
Validated Persistent Preference State
        ↓
        + ← Session Overrides supplied by Reading Session/Application
        ↓
Preference Resolution
        ↓
EffectivePreferencesSnapshot
        ↓
Application / Business Pipeline Orchestration
        ↓
Runtime Configuration Resolution
        ↓
ConfigurationSnapshot
        ↓
Processing Modules
```

The critical ownership split is:

```text
Preferences
    → user-configurable preference semantics

Reading Session
    → active session-specific choices/overrides

Business Pipeline Orchestration
    → impact on required processing

Runtime Configuration
    → execution-ready configuration

Processing Modules
    → module-specific behavior
```

---

# 4. Why Preferences Exists

CRAI behavior is configurable across many capabilities:

```text
reading
capture
recognition
text processing
translation
presentation
resource behavior
AI-assisted features
```

Without one configuration-domain authority:

* modules may interpret preference keys differently;
* precedence may differ;
* invalid values may enter processing;
* migration becomes inconsistent;
* UI may duplicate validation;
* session configuration may accidentally overwrite persistent defaults.

Preferences centralizes these semantics.

---

# 5. Core Responsibilities

Preferences owns:

```text
PreferenceDefinition
PreferenceKey
PreferenceValue
PreferenceScope
PreferenceSet
PreferenceProfile
PreferenceSchema
PreferenceRevision
persistent Global preferences
persistent Source preferences
preference validation
preference resolution
default values
migration semantics
import/export semantics
preference change facts
sensitive-reference policy
```

Preferences may also produce:

```text
EffectivePreferencesSnapshot
PreferenceChangeSet
PreferenceImpactDescriptor
```

---

# 6. Explicit Non-Responsibilities

Preferences MUST NOT:

* execute Capture;
* execute Recognition;
* perform OCR;
* perform Text Processing;
* perform Translation;
* build Presentation;
* render settings UI;
* manage Reading Session lifecycle;
* own RuntimeRevisionId;
* create WorkItems;
* create Attempts;
* schedule processing;
* retry processing;
* restart processing stages;
* restart pipelines;
* cancel Runtime work;
* invalidate Runtime caches directly;
* select an available provider implementation at execution time;
* store provider secrets directly;
* own Runtime resource limits as authoritative infrastructure constraints.

---

# 7. Preference Semantics vs Execution

Preferences defines:

```text
what the user prefers
```

Runtime and processing modules determine:

```text
how those preferences become executable behavior
```

Example:

```text
translation.provider_preference = LocalFirst
```

may express user preference.

It does not mean Preferences selects the currently healthy provider instance.

---

# 8. Persistent Preferences vs Session Configuration

This is a critical v2 boundary.

Preferences owns persistent preference values such as:

```text
Default
Global
Source
```

Reading Session owns temporary active-session choices and overrides.

Therefore:

```text
Preferences
    → durable user preference

Reading Session
    → session-specific effective override
```

Preferences may consume session overrides as **resolution input**.

It does not own their lifecycle.

---

# 9. Removed SessionPreferenceProfile Ownership

The previous model made Preferences own:

```text
SessionPreferenceProfile
Session-scoped stored preferences
```

That ownership is removed.

Preferred model:

```text
Preferences
    owns PreferenceDefinition
    and persistent values

Reading Session
    owns SessionConfiguration / session overrides

Preference Resolver
    may combine both
```

This prevents two modules from owning session-specific configuration.

---

# 10. Preference Scope

Persistent Preferences v2 supports:

```text
Default
Global
Source
```

A resolution request may additionally supply:

```text
SessionOverride
```

from Reading Session/Application.

Resolution precedence is:

```text
Session Override
      ↓
Source
      ↓
Global
      ↓
Default
```

But ownership remains different.

---

# 11. Default Preferences

Defaults are application-defined.

Properties:

```text
versioned
read-only at runtime
always valid
complete enough for fallback
migrated with application schema
```

Defaults are not user state.

---

# 12. Global Preferences

Global preferences represent user defaults across CRAI.

Examples:

```text
default source language
default target language
preferred recognition strategy
translation preference
default presentation behavior
default reading behavior
user-facing performance preference
```

They apply unless a more specific value overrides them.

---

# 13. Source Preferences

Source preferences apply to a stable logical source identity.

Examples:

```text
website domain
document profile
application profile
content provider
user-defined source profile
```

Example:

```text
Source: example-comic-site
Source Language: zh
Preferred Presentation: Overlay
Recognition Profile: Comic
```

Source preferences override Global values.

---

# 14. Session Overrides

Session overrides belong to Reading Session/Application.

Examples:

```text
temporarily change target language
temporarily disable auto translation
temporarily prefer another presentation mode
temporarily change recognition profile
temporarily change translation quality
```

They are supplied to preference resolution.

They are not persisted into Global/Source scopes unless the user explicitly promotes them through a Preferences command.

---

# 15. Preference Categories

Preferences may define categories including:

```text
Reading
Capture
Recognition
TextProcessing
Translation
Presentation
UserPerformance
AI
Privacy
```

Categories organize preference semantics.

They do not assign runtime module ownership automatically.

---

# 16. Reading Preferences

Examples:

```text
source language
target language
reading direction
automatic translation preference
automatic capture preference
reading mode
remember reading position preference
content profile
```

A preference such as:

```text
automatic capture = true
```

does not itself schedule Capture work.

Business Pipeline Orchestration interprets the resolved value.

---

# 17. Capture Preferences

Examples:

```text
preferred source type
cursor inclusion preference
preferred acquisition mode
privacy preference
preferred capture quality
```

Hard security limits remain outside ordinary user preference.

For example:

```text
userPreference.allowFullDisplayCapture
```

cannot override a stronger system privacy policy.

---

# 18. Recognition Preferences

Examples:

```text
preferred recognition strategy
preferred OCR provider class
minimum confidence preference
vertical text preference
region detection preference
noise-reduction preference
GPU preference
```

These express user/configuration intent.

Provider availability and actual execution remain external.

---

# 19. Text Processing Preferences

Examples:

```text
Unicode normalization
whitespace policy
paragraph reconstruction preference
segmentation preference
mixed-language handling
formatting preservation
```

---

# 20. Translation Preferences

Examples:

```text
provider preference
model preference
translation style
formality
context preference
glossary reference
prompt strategy reference
proper-name policy
honorific policy
terminology policy
```

Preferences may reference safe IDs.

It does not store secret credentials.

---

# 21. Presentation Preferences

Examples:

```text
presentation mode preference
font semantics
font size
line height
alignment
background opacity
dual-language preference
overflow preference
bubble-fitting preference
```

These remain platform-independent.

Native font/UI objects are forbidden.

---

# 22. User Performance Preferences

Some user-facing behavior may be configurable:

```text
battery saver preference
quality vs speed preference
background processing preference
preload preference
memory-conscious mode
```

These are preferences.

They are not authoritative Runtime resource limits.

---

# 23. Preferences vs Runtime Configuration

This separation is important.

Preferences may say:

```text
user prefers low power mode
user prefers high translation quality
user prefers GPU acceleration
```

Runtime Configuration determines:

```text
actual worker count
hard memory limit
queue capacity
deadline
resource admission
available GPU capability
provider timeout
```

Final Runtime configuration may combine:

```text
user preference
+
system capability
+
application policy
+
runtime safety limits
```

---

# 24. PreferenceDefinition

Every supported preference has a stable definition.

Conceptually:

```text
PreferenceDefinition
├── preferenceKey
├── category
├── valueType
├── defaultValue
├── allowedPersistentScopes
├── sessionOverrideAllowed
├── validationRules
├── sensitivity
├── semanticImpactTags[]
├── schemaVersion
├── deprecated
└── replacementKey?
```

---

# 25. Removed Restart Ownership from PreferenceDefinition

The previous model included fields such as:

```text
requiresPipelineRestart
affectsCacheValidity
```

with execution-oriented semantics.

Those should not encode Runtime actions directly.

Instead use semantic descriptors such as:

```text
semanticImpactTags
```

Examples:

```text
ReadingSemantics
CaptureSemantics
RecognitionSemantics
TranslationSemantics
PresentationSemantics
ProviderPreference
ResourcePreference
PrivacySemantics
```

Business Pipeline Orchestration interprets those impacts.

---

# 26. PreferenceKey

Every preference has one stable key.

Example:

```text
reading.source_language
reading.target_language

capture.cursor_inclusion

recognition.strategy
recognition.minimum_confidence

translation.provider_preference
translation.style

presentation.mode
presentation.font_size

performance.quality_profile

ai.use_story_context
```

Keys must remain stable after publication.

Renaming requires migration.

---

# 27. PreferenceValue

Supported value types may include:

```text
Boolean
Integer
Decimal
String
Enumeration
Duration
Size
List
Map
StructuredObject
Reference
```

Invalid values never enter committed preference state.

---

# 28. PreferenceScope

Conceptually:

```text
PreferenceScope
├── Default
├── Global
└── Source
```

`SessionOverride` is a resolution input scope rather than Preferences-owned persisted state.

---

# 29. PreferenceSet

A PreferenceSet is an immutable committed set associated with one owned scope.

Examples:

```text
GlobalPreferenceSet
SourcePreferenceSet
```

Conceptually:

```text
PreferenceSet
├── scope
├── scopeIdentity?
├── values
├── preferenceRevision
├── schemaVersion
└── updatedAt
```

---

# 30. SourcePreferenceProfile

A source-specific persistent profile may contain:

```text
SourcePreferenceProfile
├── sourceProfileId
├── sourceIdentity
├── preferenceSet
├── preferenceRevision
└── metadata
```

Source identity must remain logical and platform-independent.

---

# 31. PreferenceRevision

Preferences owns:

```text
PreferenceRevision
```

It represents committed Preferences-owned state.

Revision may be scoped.

Examples:

```text
GlobalPreferenceRevision
SourcePreferenceRevision
```

Exact identity model belongs to `CONTRACT.md`.

---

# 32. PreferenceRevision Rules

1. only Preferences creates it;
2. immutable after commit;
3. advances only after semantic preference change;
4. no-op does not advance it;
5. rejected updates do not advance it;
6. not Runtime execution authority;
7. not ReadingContextRevision;
8. not ConfigurationSnapshot version.

---

# 33. EffectivePreferencesSnapshot

Preferences resolves a complete immutable view.

Conceptually:

```text
EffectivePreferencesSnapshot
├── schemaVersion
├── globalPreferenceRevision
├── sourcePreferenceRevision?
├── sessionOverrideVersion?
├── resolvedValues
├── provenance
├── semanticImpactMetadata?
└── createdAt
```

The snapshot is immutable.

---

# 34. Effective Preferences Resolution

Resolution is deterministic:

```text
Application Defaults
        ↓
Global Preferences
        ↓
Source Preferences
        ↓
Session Overrides
        ↓
Validation
        ↓
EffectivePreferencesSnapshot
```

Given equivalent inputs, the same semantic result must be produced.

---

# 35. Resolution Provenance

An EffectivePreferencesSnapshot should preserve enough provenance to determine:

```text
which scope supplied each value
which schema version was used
which PreferenceRevision values participated
whether session overrides were applied
```

This improves diagnostics and reproducibility.

---

# 36. Resolution Does Not Create Execution Authority

Producing:

```text
EffectivePreferencesSnapshot vX
```

does not automatically:

```text
cancel Runtime work
create Runtime Revision
invalidate Artifact
restart Translation
refresh Presentation
```

Those decisions occur outside Preferences.

---

# 37. Preference Change

A Preferences-owned persistent update follows:

```text
Preference Command
    ↓
validate
    ↓
build Candidate PreferenceSet
    ↓
cross-field validation
    ↓
atomic commit
    ↓
PreferenceRevision + 1
    ↓
Preference-owned fact
```

---

# 38. Candidate Preference State

Updates should use Candidate isolation.

```text
Current PreferenceSet N
        +
Candidate N+1
        ↓
validation
        ↓
commit?
    ├── yes → N+1
    └── no  → N unchanged
```

Invalid changes never partially mutate committed preference state.

---

# 39. No-Op Preference Update

Equivalent update:

```text
current value = requested value
```

should normally result in:

```text
NoOp
```

without a new PreferenceRevision.

---

# 40. Validation

Preferences owns validation of preference semantics.

Validation includes:

```text
type
range
enum
scope
required value
cross-field consistency
schema compatibility
security/privacy constraints
```

---

# 41. Validation Boundary

Preferences may validate:

```text
font size > 0
confidence in [0,1]
session override allowed?
value conforms to schema?
```

It should not attempt volatile Runtime capability validation such as:

```text
GPU currently available?
provider currently healthy?
current worker capacity?
```

Those belong to capability/provider/runtime layers.

---

# 42. Static Compatibility vs Runtime Availability

Preferences may define static compatibility:

```text
this preference requires capability class X
```

but it must not conclude:

```text
provider X is currently available
```

if availability is dynamic.

---

# 43. ChangeSet

A successful mutation may produce:

```text
PreferenceChangeSet
├── changedKeys[]
├── scope
├── scopeIdentity?
├── previousRevision
├── preferenceRevision
└── semanticImpactTags[]
```

This reports what changed.

It does not prescribe how execution must react.

---

# 44. PreferenceImpactDescriptor

Preferred v2 impact model:

```text
PreferenceImpactDescriptor
├── semanticDomains[]
├── affectedPreferenceKeys[]
├── possibleArtifactCompatibilityImpact?
├── possiblePresentationImpact?
├── securityImpact?
└── requiresApplicationRestart?
```

Only truly application-static settings should declare:

```text
requiresApplicationRestart
```

Runtime stage/pipeline restart is not Preferences-owned.

---

# 45. Removed Impact Levels

The previous action-like levels:

```text
PresentationRefresh
StageRestart
PipelineRestart
SessionRestart
```

are removed as Preferences-owned decisions.

Reason:

they require knowledge of:

```text
current Reading Context
available Artifacts
pipeline topology
Runtime execution
reuse opportunities
```

Those belong to Business Pipeline Orchestration/Runtime.

---

# 46. Business Pipeline Impact

Example:

```text
translation.provider_preference changed
        ↓
Preferences commits new value
        ↓
PreferenceChanged
+ TranslationSemantics impact
        ↓
Business Pipeline Orchestration
        ↓
checks current ReadingContext
and available Artifacts
        ↓
decides whether Translation must run again
```

Preferences never emits:

```text
RestartTranslationStage
```

---

# 47. Cache Impact

Preferences may report semantic information useful for cache compatibility.

For example:

```text
translation target language changed
    → translation semantics affected

presentation font size changed
    → presentation semantics affected
```

Actual cache invalidation belongs to:

```text
Artifact/cache owner
Business Pipeline Orchestration
Runtime policy
```

Preferences does not delete caches.

---

# 48. Reading Session Relationship

Reading Session consumes or derives its initial session configuration from resolved preferences.

Preferred flow:

```text
Preferences
    ↓
persistent/default preference resolution
    ↓
Application
    ↓
Reading Session
    ↓
SessionConfiguration
```

Reading Session then owns active session overrides.

---

# 49. Session Override Resolution

When an effective view is needed:

```text
Persistent Preferences
        +
Reading Session SessionConfiguration/Overrides
        ↓
Preference Resolution
        ↓
EffectivePreferencesSnapshot
```

The resolver may live inside Preferences while ownership of session override data remains Reading Session.

---

# 50. Preference Change While Session Is Active

A persistent preference change does not automatically mutate current Reading Session state.

Possible policy:

```text
PreferenceChanged
        ↓
Application / Business Pipeline Orchestration
        ↓
decide whether active session adopts it
        ↓
Reading Session command if required
```

This avoids hidden mutation of session state.

---

# 51. Processing Module Relationship

Processing modules should not independently:

```text
query Global Preferences
query Source Preferences
resolve scope precedence
listen for preference changes
```

Preferred:

```text
Application / Runtime
    ↓
ConfigurationSnapshotRef
    ↓
Processing Module
```

---

# 52. EffectivePreferences vs ConfigurationSnapshot

These concepts are distinct.

```text
EffectivePreferencesSnapshot
    → resolved user preference semantics

ConfigurationSnapshot
    → immutable execution-ready Runtime/module configuration
```

Runtime Configuration may derive the latter from:

```text
EffectivePreferences
+
system capabilities
+
hard limits
+
provider availability
+
application policy
```

---

# 53. Why Processing Modules Should Not Depend Directly on Preferences

Direct dependency:

```text
Recognition → Preferences
Translation → Preferences
Capture → Preferences
```

creates:

* hidden configuration reads;
* nondeterministic retry behavior;
* difficulty reproducing Attempts;
* inconsistent snapshot timing.

Preferred:

```text
Runtime Attempt
    ↓
ConfigurationSnapshotRef
    ↓
Processing Module
```

---

# 54. Storage Relationship

Preferences owns:

```text
preference semantics
schema
validation
revision
migration meaning
```

Storage owns:

```text
persistence mechanism
transactions
physical encoding
database/filesystem
```

Preferences accesses Storage only through stable persistence contracts.

---

# 55. Credential Relationship

Preferences may store:

```text
CredentialRef
ProviderAccountRef
PrivateEndpointRef
```

but not secrets.

Credential Store owns:

```text
API keys
tokens
passwords
private certificates
```

---

# 56. Provider Relationship

Preferences may express:

```text
preferred provider
provider class preference
quality/cost preference
local-vs-remote preference
```

Actual provider selection may depend on:

```text
availability
health
capability
policy
cost limits
Runtime context
```

and belongs to Provider Resolution / Business Pipeline / Runtime architecture.

---

# 57. UI Adapter Relationship

UI Adapter may:

```text
query PreferenceDefinitions
query current persistent values
submit preference update commands
display validation errors
request reset
request import/export
```

UI Adapter must not:

* resolve scopes independently;
* bypass validation;
* directly write Storage;
* persist session override as Global preference without explicit command.

---

# 58. Settings UI Is Not Preferences

Preferences owns settings semantics.

UI owns:

```text
form layout
controls
interaction
local draft state
visual validation feedback
```

This allows desktop/browser/mobile settings UI to share one domain model.

---

# 59. Preference Import

Import flow:

```text
Imported Preference Document
        ↓
schema/version check
        ↓
migration if supported
        ↓
full validation
        ↓
Candidate imported state
        ↓
atomic commit
```

Invalid import never partially modifies state.

---

# 60. Preference Export

Export should include:

```text
schema version
safe preference values
scope
source profile identity where allowed
```

Export must exclude:

```text
secret values
credential contents
private tokens
unsafe environment-specific data
```

---

# 61. Reset Semantics

Possible operations:

```text
reset one key
reset one category
reset one Source profile
reset Global preferences
```

Reset removes the explicit value at the selected persistent scope.

Resolution then falls back to the next applicable value.

Session override reset belongs to Reading Session.

---

# 62. Migration

Preferences owns semantic schema migration.

A migration may:

```text
rename key
split key
merge keys
convert value format
remove deprecated key
apply new default
```

Migration must be deterministic and versioned.

---

# 63. Deprecation

PreferenceDefinition may include:

```text
deprecated
deprecatedSince
replacementKey?
removalVersion?
```

Deprecated keys remain readable according to compatibility policy.

---

# 64. Preference Schema

Conceptually:

```text
PreferenceSchema
├── schemaVersion
├── definitions[]
├── migrationRules[]
└── compatibilityMetadata
```

Schema is application-owned and immutable for one released version.

---

# 65. Sensitive Preferences

Sensitive preferences should contain only references.

Examples:

```text
credentialRef
privateEndpointRef
organizationRef
localModelRef
```

The module must not expose secret content through:

```text
events
logs
diagnostics
error payloads
export
```

---

# 66. Privacy Preferences

Preferences may contain user privacy choices such as:

```text
allowRemoteTranslation
allowRemoteRecognition
allowCapturePersistence
allowFullDisplayCapture
```

These are user choices.

They do not override stronger system/security policy.

Effective permission is generally:

```text
user preference
AND
system policy
AND
capability permission
```

---

# 67. Security Precedence

A permissive user preference must never widen authority beyond security policy.

Example:

```text
preference:
allowFullDisplayCapture = true

system privacy policy:
full display forbidden
```

Result:

```text
full display remains forbidden
```

Preferences expresses user intent, not security authority.

---

# 68. Event Ownership

Preferences owns facts about Preferences-owned persistent state.

Core event candidates:

```text
PreferenceChanged
PreferenceReset
SourcePreferenceProfileCreated
SourcePreferenceProfileDeleted
PreferenceMigrationCompleted
PreferenceImportCompleted
```

`EffectivePreferencesChanged` requires more care because EffectivePreferences may include external Session Overrides.

Detailed semantics belong to `EVENTS.md`.

---

# 69. Effective Preferences Events

Preferences SHOULD NOT automatically publish a global:

```text
EffectivePreferencesChanged
```

for every possible session context unless there is a concrete consumer and stable identity model.

Why:

```text
EffectivePreferences
=
persistent preferences
+
source
+
session overrides
```

and Session override ownership is external.

Prefer:

```text
persistent Preference facts
+
explicit resolve/query
```

for MVP.

---

# 70. No Processing Commands Through Events

Preferences events never mean:

```text
restart Recognition
retry Translation
rebuild Presentation
cancel Runtime
```

They describe configuration facts.

Business Pipeline Orchestration/Application decides consequences.

---

# 71. State Ownership

Preferences may own lifecycle such as:

```text
UNINITIALIZED
LOADING
READY
UPDATING
MIGRATING
DEGRADED
STOPPED
```

Detailed model belongs to `STATES.md`.

A global generic `FAILED` state should be used only if Preferences-owned correctness cannot be trusted.

---

# 72. Preference Update State vs Runtime State

```text
Preferences = UPDATING
```

has no direct relationship to:

```text
Runtime Attempt = RUNNING
```

Preferences lifecycle and Runtime execution are independent.

---

# 73. Error Ownership

Preferences owns errors such as:

```text
UnknownPreferenceKey
InvalidPreferenceValue
UnsupportedPreferenceScope
PreferenceRevisionConflict
PreferenceResolutionFailed
SchemaIncompatible
MigrationFailed
ImportValidationFailed
PreferenceInvariantViolation
```

Preferences does not own:

```text
Runtime retry failure
provider unavailable
OCR failure
Translation failure
Capture failure
Presentation failure
```

---

# 74. Persistence Failure Boundary

If Storage fails while Preferences is saving state:

```text
Storage implementation error
        ↓
normalized persistence-port failure
        ↓
Preferences update fails
```

Preferences may expose a domain-level persistence coordination error.

It must not expose database-specific exceptions publicly.

---

# 75. Preference Revision Concurrency

Persistent updates should use optimistic concurrency.

Example:

```text
Current PreferenceRevision = 20

Command A expects 20
Command B expects 20

B commits 21

A reaches commit
    ↓
PreferenceRevisionConflict
```

Current committed preference state remains valid.

---

# 76. Resolution Caching

Preferences may cache:

```text
validated PreferenceSets
resolved persistent scope combinations
EffectivePreferencesSnapshots
```

where safe.

Cache keys must include relevant:

```text
PreferenceRevision
Source profile identity
Session override version/ref
schema version
```

---

# 77. Resolution Cache Ownership

Resolution cache is an internal Preferences optimization.

It must not become:

```text
processing Artifact cache
Translation cache
Recognition cache
Presentation cache
```

Those belong elsewhere.

---

# 78. Determinism

Given identical:

```text
PreferenceSchema
Global PreferenceSet
Source PreferenceSet
Session Override input
```

Preferences must produce semantically equivalent:

```text
EffectivePreferencesSnapshot
```

No hidden Runtime availability or event timing may alter resolution.

---

# 79. Platform Independence

Preferences contracts must not contain:

```text
DOM objects
native font handles
native window handles
provider SDK objects
database records
filesystem handles
UI control objects
```

Values must remain serializable semantic configuration.

---

# 80. Dependencies

Preferences may depend on stable abstractions for:

```text
Storage persistence port
Credential references
Source identity primitives
schema/version primitives
diagnostics
event publication
common validation
```

Preferences must not directly depend on:

```text
Capture implementation
Recognition implementation
Translation implementation
Presentation implementation
Scheduler implementation
Work Queue implementation
UI framework
provider SDK
database SDK
```

---

# 81. Dependency Direction

Preferred:

```text
Application
    ↓
Preferences Contract
```

and:

```text
Application / Runtime Configuration
    ↓
EffectivePreferencesSnapshot
```

Processing modules should not import Preferences implementation.

---

# 82. Performance Goals

Preferences should support:

```text
fast deterministic resolution
bounded memory
cheap immutable snapshots
minimal recomputation
efficient Source profiles
atomic persistent updates
fast read paths
```

Preference resolution must not sit on a critical processing hot path in a way that requires repeated Storage access.

---

# 83. Snapshot-First Execution

Before Runtime processing begins, configuration should be resolved into immutable snapshots.

Preferred:

```text
Preferences
    ↓
EffectivePreferencesSnapshot
    ↓
Runtime Configuration Resolver
    ↓
ConfigurationSnapshot
    ↓
Attempt
```

This allows execution to remain reproducible even if Preferences changes concurrently.

---

# 84. Preference Change During Attempt

If preferences change while an Attempt is running:

```text
Attempt continues using its ConfigurationSnapshot
```

unless Runtime/Business Pipeline Orchestration separately establishes newer execution authority.

Processing modules must not read live preferences halfway through execution.

---

# 85. Example — Target Language Change

```text
User changes persistent target language
        ↓
Preferences validates
        ↓
PreferenceRevision + 1
        ↓
PreferenceChanged
        ↓
Application / active-session policy
        ↓
Reading Session update if adopted
        ↓
Business Pipeline Orchestration
        ↓
decides Translation implications
```

Preferences does not restart Translation directly.

---

# 86. Example — Presentation Font Size Change

```text
presentation.font_size changed
        ↓
Preferences commit
        ↓
PresentationSemantics impact
```

Then:

```text
Business/Application context
        ↓
determines active session adoption
        ↓
Presentation command if required
```

No `PresentationRefresh` command is emitted by Preferences itself.

---

# 87. Example — Recognition Provider Preference

```text
recognition.provider_preference changed
        ↓
Preferences commit
        ↓
RecognitionSemantics
+
ProviderPreference impact
```

Provider Resolution later decides the usable provider.

Preferences does not query provider health.

---

# 88. Example — GPU Preference

```text
recognition.prefer_gpu = true
```

means:

```text
user prefers GPU execution
```

not:

```text
GPU is available
```

Runtime capability resolution determines actual execution configuration.

---

# 89. Example — Session Override

```text
Global target language = Vietnamese
Source target language = Vietnamese
Session override = English
        ↓
Resolve
        ↓
Effective target language = English
```

Reading Session owns the override.

Preferences owns the resolution semantics.

---

# 90. Example — Session Ends

When Reading Session ends:

```text
Session overrides disappear with Reading Session state
```

Preferences does not need to delete a stored SessionPreferenceProfile because it does not own one.

---

# 91. MVP Scope

Required:

```text
PreferenceDefinition registry
Default values
Global preferences
Source preferences
PreferenceRevision
validation
scope resolution
Session Override input support
EffectivePreferencesSnapshot
atomic update
reset
basic migration
preference change events
Storage abstraction
privacy-safe sensitive references
```

---

# 92. Deferred Scope

Possible later capabilities:

```text
cloud preference synchronization
multi-device profiles
account-scoped preferences
workspace profiles
organization policy
advanced import/export
user profile sharing
experimental preference namespaces
remote administration
```

---

# 93. Architecture Risks

## 93.1 Session Ownership Duplication

Do not reintroduce persistent:

```text
SessionPreferenceProfile
```

inside Preferences if Reading Session already owns active session configuration.

---

## 93.2 Pipeline Orchestration Leakage

Do not add:

```text
RestartStage
RestartPipeline
RefreshPresentation
RetryTranslation
```

as Preferences-owned actions.

---

## 93.3 Runtime Configuration Leakage

Do not let user Preferences become authoritative for unsafe hard limits.

---

## 93.4 Provider Resolution Leakage

Do not make Preferences inspect live provider health to choose execution provider.

---

## 93.5 Live Preference Reads During Attempts

Processing modules must not read mutable current Preferences while processing.

---

# 94. Design Principles

1. Preferences owns user configuration semantics.

2. Preferences does not own execution.

3. Persistent preference state is separate from session-specific configuration.

4. Session overrides may participate in resolution without transferring ownership.

5. Effective preferences are immutable.

6. Preference updates are atomic.

7. Resolution is deterministic.

8. PreferenceRevision is Preferences-owned.

9. PreferenceRevision is not Runtime authority.

10. Preference semantic impact is descriptive, not executable action.

11. Business Pipeline Orchestration interprets processing consequences.

12. Runtime Configuration produces execution-ready snapshots.

13. Processing modules use snapshots, not live Preferences.

14. Storage does not interpret preference semantics.

15. Credentials remain outside Preferences.

16. Security policy may restrict user preference.

17. Public contracts remain serializable.

18. Sensitive values remain protected.

---

# 95. Architecture Invariants

1. Every supported preference has one stable PreferenceKey.

2. Every accepted value conforms to PreferenceDefinition.

3. Persistent scope ownership belongs to Preferences.

4. Session configuration ownership belongs to Reading Session.

5. Preferences may resolve externally supplied Session Overrides.

6. Preferences does not own SessionPreferenceProfile lifecycle.

7. Resolution precedence is deterministic.

8. EffectivePreferencesSnapshot is immutable.

9. Every successful semantic persistent change advances PreferenceRevision.

10. No-op changes do not advance PreferenceRevision.

11. Invalid updates never partially mutate state.

12. PreferenceRevision is not RuntimeRevisionId.

13. PreferenceRevision is not ReadingContextRevision.

14. Preferences does not create WorkItems.

15. Preferences does not create Attempts.

16. Preferences does not cancel Runtime work.

17. Preferences does not execute Runtime retry.

18. Preferences does not decide StageRestart.

19. Preferences does not decide PipelineRestart.

20. Preferences does not decide Presentation refresh.

21. Semantic impact metadata is descriptive only.

22. Business Pipeline Orchestration decides processing implications.

23. Preferences does not invalidate processing caches directly.

24. Preferences does not select live provider instances.

25. Processing modules do not resolve Preferences scopes independently.

26. Processing modules do not read live mutable Preferences during execution.

27. Runtime/module ConfigurationSnapshot is distinct from EffectivePreferencesSnapshot.

28. User performance preference does not override Runtime hard safety limits.

29. User privacy preference cannot widen system authorization.

30. Preferences contains credential references, not secrets.

31. Storage owns persistence implementation.

32. UI Adapter does not implement resolution rules independently.

33. Preference events describe configuration facts only.

34. Preference events do not become workflow commands.

35. Diagnostics remain privacy-safe.

---

# 96. Testing Strategy

Preferences must be testable without:

```text
Capture
Recognition
Translation
Presentation
Scheduler
Runtime worker
provider SDK
UI framework
real database
```

---

# 97. Unit Tests

Test:

```text
PreferenceDefinition validation
PreferenceKey stability
scope validation
value validation
cross-field validation
Global updates
Source updates
PreferenceRevision
no-op behavior
resolution precedence
Session Override input
EffectivePreferences immutability
semantic impact descriptors
reset behavior
migration
sensitive-value protection
```

---

# 98. Ownership Tests

Verify Preferences never:

```text
creates SessionPreferenceProfile as active-session owner
creates RuntimeRevisionId
creates WorkItem
creates Attempt
restarts processing stage
restarts pipeline
invalidates Artifact Store directly
selects provider by live health
stores credential secret
```

---

# 99. Resolution Tests

Verify:

```text
Default
Global
Source
Session Override
```

precedence.

Also test:

* missing Source scope;
* missing Session Override;
* equivalent inputs produce equivalent result;
* invalid Session Override rejected from effective result;
* provenance identifies resolution source.

---

# 100. Concurrency Tests

Test:

```text
two Global updates from same expected revision
two Source updates
Global update racing Source update
migration racing update
import racing update
reset racing update
```

---

# 101. Snapshot Tests

Verify an existing:

```text
EffectivePreferencesSnapshot
```

does not mutate after a newer preference change.

Likewise an existing Runtime:

```text
ConfigurationSnapshot
```

is not silently modified.

---

# 102. Related Documents

```text
.meta/AI_BOOT.md
.meta/PROJECT_RULE.md
.meta/MODULE_ROLE.md
.meta/WORKFLOW.md
.meta/CHANGE_RULE.md

doc/01-architecture/core/CAPABILITY_MAP.md
doc/01-architecture/core/DATA_FLOW.md
doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/core/EVENT_CONVENTION.md
doc/01-architecture/core/STATE_MACHINE.md

doc/01-architecture/modules/MODULE_MAP.md
doc/01-architecture/modules/MODULE_DEPENDENCY.md
doc/01-architecture/modules/OWNERSHIP_MAP.md

doc/01-architecture/runtime/BUSINESS_PIPELINE_ORCHESTRATION.md
doc/01-architecture/runtime/PIPELINE_RUNTIME.md
doc/01-architecture/runtime/RETRY_POLICY.md
doc/01-architecture/runtime/RESOURCE_LIFECYCLE.md
doc/01-architecture/runtime/RUNTIME_OBSERVABILITY.md

doc/02-modules/preferences/README.md
doc/02-modules/preferences/CONTRACT.md
doc/02-modules/preferences/STATES.md
doc/02-modules/preferences/EVENTS.md
doc/02-modules/preferences/ERRORS.md

doc/02-modules/reading-session/MODULE.md
doc/02-modules/capture/MODULE.md
doc/02-modules/recognition/MODULE.md
doc/02-modules/translation/MODULE.md
doc/02-modules/presentation/MODULE.md

doc/03-contracts/
doc/03-infrastructure/storage/
```

---

# 103. Documentation Ownership

This file defines:

```text
Preferences module identity
persistent preference ownership
Session Override boundary
scope semantics
PreferenceRevision ownership
resolution semantics
EffectivePreferencesSnapshot
impact classification boundary
Runtime Configuration boundary
Reading Session relationship
Storage relationship
Credential boundary
architecture invariants
```

Detailed public schemas belong to:

```text
CONTRACT.md
```

Detailed lifecycle belongs to:

```text
STATES.md
```

Detailed Preference-owned facts belong to:

```text
EVENTS.md
```

Detailed error taxonomy belongs to:

```text
ERRORS.md
```

---

# 104. Completion Criteria

Preferences is architecturally synchronized when:

* Preferences owns persistent preference semantics;
* active Session Overrides are Reading Session-owned;
* scope resolution can consume Session Overrides without owning them;
* PreferenceRevision ownership is explicit;
* EffectivePreferencesSnapshot is immutable;
* Runtime Configuration is separate from Preferences;
* processing modules consume configuration snapshots rather than live Preferences;
* execution restart decisions are removed from Preferences ownership;
* semantic impact descriptors replace Stage/Pipeline restart actions;
* Business Pipeline Orchestration owns processing consequences;
* provider preference is separate from provider availability/resolution;
* user performance preference is separate from Runtime hard limits;
* sensitive values are references only;
* Storage owns persistence implementation;
* updates are atomic;
* migrations are deterministic;
* tests verify scope, revision, ownership, immutability, and concurrency.

---

# 105. Summary

Preferences v2 is CRAI's user-configuration domain authority.

Its persistent flow is:

```text
Preference Command
    ↓
Preferences
    ↓
Validation
    ↓
Candidate Preference State
    ↓
Atomic Commit
    ↓
PreferenceRevision
```

Its resolution flow is:

```text
Application Defaults
        +
Global Preferences
        +
Source Preferences
        +
Reading Session Overrides
        ↓
Preferences Resolution
        ↓
EffectivePreferencesSnapshot
```

Its execution flow is separate:

```text
EffectivePreferencesSnapshot
        ↓
Runtime Configuration Resolution
        ↓
ConfigurationSnapshot
        ↓
Runtime Attempt
        ↓
Processing Module
```

The central ownership rule is:

```text
Preferences owns
what the user prefers.

Reading Session owns
temporary session choices.

Business Pipeline Orchestration owns
what processing those choices require.

Runtime owns
how that processing executes.
```
