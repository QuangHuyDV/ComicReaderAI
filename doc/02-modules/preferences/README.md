# Preferences Module

> **Project:** CRAI
> **Module:** `preferences`
> **Path:** `doc/02-modules/preferences/README.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-09

---

# 1. Overview

The Preferences Module is CRAI's authority for persistent user-configurable preference semantics.

It defines:

```text
what the user prefers
```

and provides deterministic resolution of those preferences into immutable effective snapshots.

Its primary responsibilities are:

```text
Preference Definitions
+
Persistent Preference Values
+
Source-specific Preferences
+
External Session Overrides
        ↓
Validation
        ↓
Resolution
        ↓
EffectivePreferencesSnapshot
```

Preferences does not execute processing.

It does not decide:

```text
whether Capture must run
whether Recognition must restart
whether Translation must rerun
whether Presentation must refresh
whether Runtime work should be cancelled
```

Those decisions belong to other owners.

---

# 2. Architecture Position

The preferred architecture is:

```text
User / Settings UI
        ↓
Preferences
        ↓
Persistent Preference State
        ↓
EffectivePreferencesSnapshot
        ↓
Runtime Configuration Resolution
        ↓
ConfigurationSnapshot
        ↓
Business Pipeline / Runtime
        ↓
Processing Modules
```

For an active reading session:

```text
Persistent Preferences
        +
Reading Session Overrides
        ↓
Preferences Resolution
        ↓
EffectivePreferencesSnapshot
```

Ownership remains separate.

---

# 3. Ownership Model

```text
Preferences
    → persistent user preference semantics

Reading Session
    → temporary active-session configuration

Business Pipeline Orchestration
    → processing consequences

Runtime Configuration
    → execution-ready configuration

Runtime Control
    → execution authority

Processing Modules
    → module-specific processing behavior
```

This separation prevents configuration, business orchestration, and execution from collapsing into one module.

---

# 4. Primary Responsibilities

Preferences owns:

```text
PreferenceDefinition
PreferenceKey
PreferenceValue
PreferenceScope
PreferenceSet
SourcePreferenceProfile
PreferenceSchema
PreferenceRevision

Global persistent preferences
Source persistent preferences

preference validation
scope resolution
default values
schema migration
import/export semantics
preference change facts
sensitive-reference policy
```

Preferences may produce:

```text
EffectivePreferencesSnapshot
PreferenceChangeSet
semantic impact metadata
```

---

# 5. Explicit Non-Responsibilities

Preferences MUST NOT:

* execute Capture;
* execute Recognition;
* execute Text Processing;
* execute Translation;
* build Presentation;
* render UI;
* own Reading Session lifecycle;
* own SessionOverride lifecycle;
* create RuntimeRevisionId;
* create WorkItems;
* create Attempts;
* cancel Runtime work;
* retry Runtime work;
* restart a processing stage;
* restart a pipeline;
* invalidate processing Artifacts directly;
* select a live provider based on current health;
* store credentials directly;
* implement persistence itself.

---

# 6. Why Preferences Exists

CRAI contains many user-configurable behaviors.

Examples:

```text
reading language
capture behavior
recognition strategy
translation style
presentation mode
quality preference
privacy preference
AI-assisted behavior
```

Without one stable configuration-domain authority:

* modules may define conflicting keys;
* validation may differ;
* scope precedence may differ;
* migration becomes inconsistent;
* UI may duplicate domain rules;
* running Attempts may read changing configuration halfway through execution.

Preferences centralizes the user-configuration semantics.

---

# 7. Persistent Preference Scopes

Preferences v2 owns three persistent scopes:

```text
Default
Global
Source
```

Resolution may also include:

```text
SessionOverride
```

but SessionOverride is externally owned.

---

# 8. Resolution Precedence

Effective resolution is:

```text
Session Override
      ↓
Source
      ↓
Global
      ↓
Default
```

The highest-priority valid value wins.

This precedence is deterministic.

---

# 9. Default Scope

Defaults are built into the application.

Properties:

```text
versioned
read-only at runtime
schema-valid
fallback-safe
```

Defaults are not user state.

---

# 10. Global Scope

Global preferences represent user-wide defaults.

Examples:

```text
default target language
default recognition strategy
translation style
presentation mode
quality preference
privacy choices
```

---

# 11. Source Scope

Source preferences apply to one logical content/source profile.

Examples:

```text
comic website
novel website
document profile
application profile
user-defined reading source
```

Example:

```text
Source profile:
    source language = zh
    recognition strategy = Comic
    presentation mode = Overlay
```

---

# 12. Session Overrides

Session-specific overrides belong to Reading Session.

Examples:

```text
temporarily change target language
temporarily disable auto translation
temporarily change presentation mode
temporarily choose another recognition strategy
```

Preferences may consume these values during resolution.

It does not own or persist their lifecycle.

---

# 13. No `SessionPreferenceProfile`

Preferences v2 does not maintain:

```text
SessionPreferenceProfile
```

as its own active-session object.

Instead:

```text
Reading Session
    → owns SessionConfiguration

Preferences
    → owns resolution semantics
```

This prevents duplicate session configuration ownership.

---

# 14. Preference Categories

Preferences may define categories such as:

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

They do not automatically define Runtime ownership.

---

# 15. Reading Preferences

Examples:

```text
source language
target language
reading direction
reading mode
automatic capture preference
automatic translation preference
remember position preference
```

A preference such as:

```text
auto_capture = true
```

does not cause Capture to run directly.

---

# 16. Capture Preferences

Examples:

```text
preferred capture source
cursor inclusion
preferred capture quality
preferred acquisition behavior
privacy choices
```

Hard security/privacy restrictions remain stronger than user preference.

---

# 17. Recognition Preferences

Examples:

```text
recognition strategy
preferred OCR provider class
confidence preference
vertical text preference
region detection preference
GPU preference
```

Preferences expresses intent.

Recognition/Runtime decides executable behavior.

---

# 18. Text Processing Preferences

Examples:

```text
Unicode normalization
whitespace policy
paragraph reconstruction
sentence segmentation
mixed-language handling
formatting preservation
```

---

# 19. Translation Preferences

Examples:

```text
provider preference
model preference
translation style
formality
glossary reference
prompt strategy reference
proper-name policy
terminology policy
```

Secret credentials remain external.

---

# 20. Presentation Preferences

Examples:

```text
presentation mode
font semantics
font size
line height
alignment
background opacity
dual-language mode
overflow preference
bubble fitting preference
```

Preferences must remain independent from native UI objects.

---

# 21. User Performance Preferences

Examples:

```text
quality vs speed preference
battery-conscious mode
background processing preference
preload preference
memory-conscious mode
```

These are preferences, not hard Runtime resource limits.

---

# 22. Preferences vs Runtime Configuration

This distinction is fundamental.

Preferences may say:

```text
user prefers GPU
user prefers low power
user prefers high translation quality
```

Runtime Configuration decides:

```text
GPU actually available?
how many workers?
what memory budget is safe?
which provider is currently usable?
what timeout/deadline applies?
```

Final execution configuration may combine:

```text
EffectivePreferencesSnapshot
+
system capability
+
application policy
+
security policy
+
provider availability
+
Runtime safety limits
```

---

# 23. PreferenceDefinition

Every supported key has one stable definition.

Conceptually:

```text
PreferenceDefinition
├── key
├── category
├── dataType
├── defaultValue
├── allowedPersistentScopes
├── sessionOverrideAllowed
├── validationRules
├── sensitivity
├── semanticImpactTags
├── schemaVersion
├── deprecated
└── replacementKey?
```

---

# 24. Semantic Impact

Preferences may classify what semantic domain changed.

Examples:

```text
ReadingSemantics
CaptureSemantics
RecognitionSemantics
TextProcessingSemantics
TranslationSemantics
PresentationSemantics
ProviderPreference
ResourcePreference
PrivacySemantics
```

These are descriptive.

They are not processing commands.

---

# 25. Removed Restart Impact

Preferences v2 does not define action-like impact values such as:

```text
PresentationRefresh
StageRestart
PipelineRestart
SessionRestart
```

Those require active business/runtime context.

Business Pipeline Orchestration determines the real consequence.

---

# 26. Example — Translation Preference Change

```text
translation.style changes
        ↓
Preferences commits new value
        ↓
PreferenceChanged
        ↓
TranslationSemantics impact
        ↓
Application / Business Pipeline Orchestration
        ↓
evaluate current ReadingContext
available Artifacts
active execution
        ↓
decide consequence
```

Preferences does not restart Translation itself.

---

# 27. PreferenceRevision

Every committed semantic persistent change advances:

```text
PreferenceRevision
```

or its scoped equivalent.

Example:

```text
Revision 20
    ↓
valid semantic update
    ↓
Revision 21
```

No revision advancement for:

```text
no-op
invalid update
resolution
export
SessionOverride change
```

---

# 28. Revision Domains

CRAI revision domains remain separate.

```text
PreferenceRevision
    → Preferences

ReadingContextRevision
    → Reading Session

RuntimeRevisionId
    → Runtime Control

ConfigurationSnapshot version
    → Runtime Configuration

PresentationRevision
    → Presentation
```

These must not be treated as interchangeable.

---

# 29. EffectivePreferencesSnapshot

Resolved preferences are exposed as:

```text
EffectivePreferencesSnapshot
```

Properties:

```text
validated
complete
immutable
deterministic
contextual
version-provenanced
```

---

# 30. Effective Resolution

Resolution combines:

```text
Application Defaults
        +
Global Preferences
        +
Source Preferences
        +
SessionOverride Input
        ↓
EffectivePreferencesSnapshot
```

There may be many different snapshots simultaneously.

---

# 31. No Global EffectivePreferences State

Preferences v2 does not assume:

```text
one EffectivePreferences object
for the entire application
```

Example:

```text
Session A
    target language = en

Session B
    target language = ja
```

Both may be valid at the same time.

---

# 32. Resolution Provenance

Snapshots should preserve enough provenance to identify:

```text
Global revision
Source revision
Source profile
Session override version/reference
schema version
value origin
```

This supports reproducible Runtime configuration.

---

# 33. Resolution Is Read-Only

Calling:

```text
ResolveEffectivePreferences
```

does not:

```text
persist values
advance PreferenceRevision
change module lifecycle
publish PreferenceChanged
```

It is a deterministic read operation.

---

# 34. Reading Session Relationship

Preferred initialization flow:

```text
Preferences
    ↓
resolved persistent defaults
    ↓
Application
    ↓
Reading Session
    ↓
SessionConfiguration
```

Reading Session then owns active session-specific choices.

---

# 35. Persistent Preference Change During Active Session

A persistent preference change does not silently mutate an active Reading Session.

Preferred:

```text
PreferenceChanged
        ↓
Application policy
        ↓
Should active session adopt it?
        ↓
Reading Session command
```

This keeps session behavior explicit.

---

# 36. Processing Module Relationship

Processing modules should not independently query Preferences during execution.

Incorrect:

```text
Recognition
    ↓
Preferences.get(...)
```

inside an active Attempt.

Preferred:

```text
EffectivePreferencesSnapshot
        ↓
Runtime Configuration
        ↓
ConfigurationSnapshot
        ↓
Runtime Attempt
        ↓
Recognition
```

---

# 37. Why Live Preference Reads Are Avoided

If modules read current Preferences during processing:

```text
Attempt begins with configuration A
        ↓
Preferences changes
        ↓
same Attempt reads configuration B
```

execution becomes non-deterministic.

Snapshot-based configuration prevents this.

---

# 38. ConfigurationSnapshot

`ConfigurationSnapshot` is different from:

```text
EffectivePreferencesSnapshot
```

The first is execution-ready.

The second represents resolved user preference semantics.

---

# 39. Business Pipeline Relationship

Business Pipeline Orchestration may use:

```text
PreferenceChangeSet
semanticImpactTags
ReadingContext
available Artifacts
```

to determine:

```text
Capture required?
Recognition required?
Translation needs new Artifact?
Presentation can reuse existing data?
```

Preferences does not answer these questions.

---

# 40. Provider Relationship

Preferences may express:

```text
LocalPreferred
CloudPreferred
provider family preference
quality preference
cost preference
```

Actual provider selection depends on:

```text
availability
health
capabilities
credentials
Runtime policy
```

and belongs outside Preferences.

---

# 41. Storage Relationship

Preferences owns:

```text
schema
validation
resolution
revision
migration semantics
```

Storage owns:

```text
physical persistence
database/file format
transaction implementation
loading/saving mechanism
```

Preferences accesses Storage through stable contracts.

---

# 42. Credential Relationship

Preferences may hold safe references such as:

```text
CredentialRef
ProviderAccountRef
PrivateEndpointRef
```

It must never store:

```text
API key
password
token
private certificate
secret
```

---

# 43. UI Adapter Relationship

UI Adapter may:

```text
query PreferenceDefinitions
display current persistent values
submit preference updates
display validation errors
reset preferences
import/export profiles
```

UI Adapter does not own:

```text
scope precedence
domain validation
migration
revision semantics
```

---

# 44. Settings UI vs Preferences

Preferences owns settings semantics.

UI owns:

```text
forms
controls
visual layout
interaction
draft editing state
```

This keeps Preferences platform-independent.

---

# 45. Persistent Update Flow

```text
Preference Command
    ↓
Validate
    ↓
Candidate Preference State
    ↓
Cross-field Validation
    ↓
Atomic Commit
    ↓
PreferenceRevision
    ↓
Preference Fact
```

---

# 46. Candidate Isolation

During update:

```text
Committed PreferenceSet N
+
Candidate PreferenceSet N+1
```

may coexist internally.

Consumers continue seeing:

```text
Committed PreferenceSet N
```

until commit succeeds.

---

# 47. Atomicity

Preference changes are all-or-nothing.

Invalid:

```text
update three fields
commit two
reject one
```

Correct:

```text
validate full Candidate
        ↓
commit all
or
commit none
```

---

# 48. No-Op Semantics

Equivalent update:

```text
requested value == current stored value
```

should normally return:

```text
NoOp
```

without:

```text
new PreferenceRevision
PreferenceChanged event
```

---

# 49. Validation

Preferences validates:

```text
key existence
type
range
enum
persistent scope
SessionOverride permission
cross-field consistency
schema compatibility
security/privacy constraints
```

---

# 50. Runtime Availability Is Not Preference Validation

Preferences should not reject a stable preference merely because:

```text
GPU unavailable now
provider offline now
network unavailable now
```

Those are dynamic execution conditions.

---

# 51. Schema Migration

Preferences owns semantic schema migration.

Migration may:

```text
rename keys
split keys
merge keys
convert value formats
remove deprecated values
apply new defaults
```

Migration must remain deterministic and atomic.

---

# 52. Import

Import flow:

```text
ImportPreferences
    ↓
parse
    ↓
schema validation
    ↓
migration if required
    ↓
full Candidate validation
    ↓
atomic commit
```

Session overrides are not imported into a persistent Session scope.

---

# 53. Export

Export is a read operation.

```text
Committed Preferences
    ↓
filter safe values
    ↓
serialize
```

It does not:

```text
advance PreferenceRevision
change module state
publish mutation event
```

---

# 54. Reset

Persistent reset may target:

```text
one key
one category
Global scope
Source profile
```

Removing a stored value causes resolution to fall back to the next lower-priority scope.

Session override reset belongs to Reading Session.

---

# 55. Module Lifecycle

Preferences module lifecycle is:

```text
UNINITIALIZED
      ↓
LOADING
      ↓
READY
  ↕
DEGRADED
      ↓
STOPPING
      ↓
STOPPED
```

Detailed semantics are defined in:

```text
STATES.md
```

---

# 56. READY Meaning

`READY` means:

```text
Preferences-owned persistent state
is valid and queryable.
```

It does not mean:

```text
one global EffectivePreferences exists.
```

---

# 57. DEGRADED Meaning

`DEGRADED` means known-good state may still be usable while part of Preferences capability is impaired.

Examples:

```text
persistent writes temporarily unavailable
one Source profile unavailable
event publication infrastructure impaired
```

Invalid user input alone does not cause DEGRADED.

---

# 58. No Global `FAILED`

Preferences v2 does not require a generic:

```text
FAILED
```

state for every internal error.

A failed command/import normally preserves the last committed state and leaves the module usable.

---

# 59. Mutation Phases

Persistent mutations use scoped phases:

```text
VALIDATING
    ↓
BUILDING_CANDIDATE
    ↓
COMMITTING
    ↓
COMMITTED
```

with outcomes:

```text
NO_OP
REJECTED
CONFLICT
FAILED
```

These are operation phases, not module lifecycle states.

---

# 60. Import and Export State

Import is a mutation operation.

Export is read-only.

Therefore Preferences v2 does not use one generic:

```text
EXPORTING
```

module state for both.

---

# 61. Events

Preferences publishes committed persistent preference facts.

Recommended core events:

```text
PreferenceChanged
PreferenceRemoved
PreferenceReset
SourcePreferenceProfileCreated
SourcePreferenceProfileDeleted
PreferenceImportCompleted
PreferenceMigrationCompleted
```

---

# 62. No Global `EffectivePreferencesChanged`

Preferences v2 does not require:

```text
EffectivePreferencesChanged
```

as one global event.

Because effective preference state depends on:

```text
Source
+
SessionOverride
```

there may be multiple valid effective snapshots simultaneously.

---

# 63. No Mandatory Consumed Events

Preferences does not require direct subscriptions to:

```text
SessionCreated
SessionClosed
StorageLoaded
ImportRequested
MigrationRequested
```

for correctness.

Explicit commands/lifecycle contracts are preferred.

---

# 64. Event Bus Is Not a Command Channel

Invalid:

```text
ImportRequested event
    ↓
Preferences starts import
```

when an explicit command is available.

Preferred:

```text
ImportPreferences command
```

Events describe facts.

Commands request actions.

---

# 65. Event Publication Timing

Correct:

```text
validate
    ↓
commit
    ↓
PreferenceRevision advances
    ↓
publish event
```

---

# 66. Event Publication Failure

If commit succeeds but event publication fails:

```text
new preference state remains committed.
```

The mutation must not be rolled back merely to recreate the event.

Infrastructure handles publication recovery.

---

# 67. Error Model

Preferences error categories include:

```text
Validation
Scope
Revision
Resolution
Schema
Migration
ImportExport
PersistenceCoordination
Security
Internal
```

Detailed definitions belong to:

```text
ERRORS.md
```

---

# 68. PreferenceRevision Conflict

Optimistic concurrency example:

```text
Current Revision = 20

Command A expects 20
Command B expects 20

B commits 21

A commits?
    ↓
PreferenceRevisionConflict
```

Current committed state remains valid.

---

# 69. Errors Preferences Does Not Own

Preferences does not own:

```text
RuntimeTimeout
RetryExhausted
PipelineRestartFailed
ProviderUnavailable
CaptureFailure
RecognitionFailure
TranslationFailure
PresentationFailure
```

---

# 70. Privacy Preferences

Preferences may store user privacy intent such as:

```text
allowRemoteRecognition
allowRemoteTranslation
allowCapturePersistence
allowFullDisplayCapture
```

But effective authorization is constrained by stronger policies.

---

# 71. Security Precedence

Example:

```text
User preference:
allowFullDisplayCapture = true

System policy:
full-display capture prohibited
```

Result:

```text
full-display capture remains prohibited
```

Preferences expresses intent.

It does not grant security authority.

---

# 72. Sensitive Values

Sensitive settings should contain references only.

Example:

```text
translation.credential_ref
```

rather than:

```text
translation.api_key
```

---

# 73. Performance Goals

Preferences should support:

```text
fast deterministic reads
cheap immutable snapshots
minimal Storage access on hot paths
efficient Source profile lookup
bounded resolution cache
atomic updates
concurrent contextual resolution
```

---

# 74. Resolution Cache

Preferences may cache resolved snapshots when safe.

Cache identity must include relevant:

```text
PreferenceRevision
Source profile
Source revision
SessionOverride version/reference
schema version
```

---

# 75. Resolution Cache Is Not Artifact Cache

This cache is only a Preferences optimization.

It is not:

```text
Capture Artifact cache
Recognition Artifact cache
Translation cache
Presentation cache
```

---

# 76. Snapshot-First Runtime Model

Recommended:

```text
Preferences
    ↓
EffectivePreferencesSnapshot
    ↓
Runtime Configuration
    ↓
ConfigurationSnapshot
    ↓
Runtime Attempt
```

This ensures deterministic processing.

---

# 77. Preference Change During Attempt

If Preferences changes while Runtime Attempt A is running:

```text
Attempt A
    continues with ConfigurationSnapshot C1.
```

It does not start reading new live preference state mid-execution.

A future Runtime authority decision may create a new Attempt using newer configuration.

---

# 78. Common Architecture Mistake — Session Scope Ownership

Wrong:

```text
Preferences
    owns SessionPreferenceProfile
```

Correct:

```text
Reading Session
    owns SessionConfiguration

Preferences
    resolves SessionOverride input
```

---

# 79. Common Architecture Mistake — Restart Logic

Wrong:

```text
PreferenceDefinition
    requiresPipelineRestart = true
```

Correct:

```text
PreferenceDefinition
    semanticImpactTags = TranslationSemantics

Business Pipeline Orchestration
    decides actual consequence
```

---

# 80. Common Architecture Mistake — Processing Module Reads Preferences

Wrong:

```text
Translation
    ↓
Preferences.getCurrent()
```

Correct:

```text
Runtime Attempt
    ↓
ConfigurationSnapshot
    ↓
Translation
```

---

# 81. Common Architecture Mistake — Provider Selection

Wrong:

```text
Preferences
    checks provider health
    selects provider
```

Correct:

```text
Preferences
    expresses provider preference

Provider Resolution / Runtime
    selects usable provider
```

---

# 82. Common Architecture Mistake — User Preference as Hard Limit

Wrong:

```text
preferences.memory_budget
    becomes authoritative Runtime memory limit
```

Correct:

```text
user performance preference
+
system hard limits
+
resource policy
        ↓
Runtime Configuration
```

---

# 83. Common Architecture Mistake — EffectivePreferences Global State

Wrong:

```text
Preferences
    owns one global EffectivePreferences
```

Correct:

```text
Preferences
    resolves contextual immutable snapshots
```

---

# 84. Common Architecture Mistake — Event-Driven Configuration Reload

Wrong:

```text
PreferenceChanged
    ↓
every processing module reloads Preferences
```

Correct:

```text
PreferenceChanged
    ↓
Application / Business Pipeline
    ↓
future Runtime ConfigurationSnapshot
```

---

# 85. Architecture Invariants

1. Preferences is the authority for persistent user preference semantics.

2. Preferences is not Runtime configuration authority.

3. Preferences owns Default, Global, and Source persistent scopes.

4. Reading Session owns temporary Session overrides.

5. Preferences may resolve external SessionOverride input.

6. Preferences does not persist SessionOverride implicitly.

7. Resolution precedence is deterministic.

8. EffectivePreferencesSnapshot is immutable.

9. Multiple effective snapshots may coexist.

10. PreferenceRevision advances only on committed semantic persistent change.

11. No-op does not advance PreferenceRevision.

12. Resolution does not advance PreferenceRevision.

13. SessionOverride changes do not advance PreferenceRevision.

14. PreferenceRevision is not ReadingContextRevision.

15. PreferenceRevision is not RuntimeRevisionId.

16. Preferences does not create WorkItems.

17. Preferences does not create Attempts.

18. Preferences does not cancel Runtime work.

19. Preferences does not execute Runtime retry.

20. Preferences does not restart processing stages.

21. Preferences does not restart pipelines.

22. Preferences does not refresh Presentation directly.

23. Semantic impact metadata is descriptive only.

24. Business Pipeline Orchestration decides processing consequences.

25. Processing modules do not resolve preference scopes.

26. Processing modules do not read live Preferences during execution.

27. Runtime ConfigurationSnapshot is distinct from EffectivePreferencesSnapshot.

28. Preferences does not select live provider implementations.

29. User preference does not override hard Runtime safety limits.

30. User privacy preference does not widen system authorization.

31. Credentials remain outside Preferences.

32. Storage owns persistence implementation.

33. UI Adapter does not duplicate preference business rules.

34. Events describe committed persistent facts.

35. Preferences has no mandatory global EffectivePreferencesChanged event.

36. Event publication failure does not roll back valid committed preference state.

37. Import is atomic.

38. Export is read-only.

39. Global FAILED state is not required for ordinary operation failures.

40. Diagnostics remain privacy-safe.

---

# 86. Document Set

```text
02-modules/
└── preferences/
    ├── README.md
    ├── MODULE.md
    ├── CONTRACT.md
    ├── STATES.md
    ├── EVENTS.md
    └── ERRORS.md
```

---

# 87. Document Responsibilities

## README.md

Provides:

```text
module overview
ownership model
architecture position
recommended reading path
```

## MODULE.md

Defines:

```text
module identity
responsibilities
scope ownership
Runtime boundary
Reading Session boundary
architecture invariants
```

## CONTRACT.md

Defines:

```text
PreferenceDefinition
PreferenceScope
PreferenceRevision
mutation commands
queries
SessionOverride input
EffectivePreferencesSnapshot
resolution contracts
```

## STATES.md

Defines:

```text
module lifecycle
mutation phases
migration phases
import phases
candidate state
revision semantics
```

## EVENTS.md

Defines:

```text
persistent preference facts
Source profile facts
migration/import facts
event ownership
publication semantics
```

## ERRORS.md

Defines:

```text
error codes
validation
revision conflicts
resolution
migration
persistence coordination
security
internal invariants
```

---

# 88. Recommended Reading Order

For a new contributor:

```text
1. README.md
2. MODULE.md
3. CONTRACT.md
4. STATES.md
5. EVENTS.md
6. ERRORS.md
```

---

# 89. Implementation Reading Order

For implementation:

```text
MODULE.md
    ↓
CONTRACT.md
    ↓
STATES.md
    ↓
ERRORS.md
    ↓
EVENTS.md
```

This makes ownership and consistency rules explicit before Event Bus integration.

---

# 90. Testing Priorities

Preferences tests should focus on:

```text
schema validation
scope validation
PreferenceRevision
atomic mutation
no-op behavior
Source profiles
SessionOverride resolution
EffectivePreferencesSnapshot
migration
import/export
privacy
concurrency
event publication timing
```

---

# 91. Ownership Tests

Verify Preferences never:

```text
owns SessionPreferenceProfile
creates RuntimeRevisionId
creates WorkItem
creates Attempt
restarts pipeline
restarts stage
cancels Runtime
selects live provider by health
stores credential secrets
```

---

# 92. Resolution Tests

Verify precedence:

```text
Session Override
    >
Source
    >
Global
    >
Default
```

Also test:

```text
multiple sessions
multiple Source profiles
missing Source scope
missing SessionOverride
invalid SessionOverride
provenance
immutability
determinism
```

---

# 93. Concurrency Tests

Verify:

```text
two Global mutations
two Source mutations
revision conflict
Global and Source concurrent mutation
import racing normal mutation
migration racing mutation
reset racing mutation
```

---

# 94. Snapshot Tests

Verify existing:

```text
EffectivePreferencesSnapshot
ConfigurationSnapshot
```

objects remain immutable after later preference changes.

---

# 95. Related Documents

```text
doc/02-modules/preferences/
├── README.md
├── MODULE.md
├── CONTRACT.md
├── STATES.md
├── EVENTS.md
└── ERRORS.md

doc/02-modules/reading-session/
doc/02-modules/capture/
doc/02-modules/recognition/
doc/02-modules/text-processing/
doc/02-modules/translation/
doc/02-modules/presentation/

doc/01-architecture/core/
├── CAPABILITY_MAP.md
├── DATA_FLOW.md
├── EVENT_BUS.md
├── EVENT_CONVENTION.md
└── STATE_MACHINE.md

doc/01-architecture/modules/
├── MODULE_MAP.md
├── MODULE_DEPENDENCY.md
└── OWNERSHIP_MAP.md

doc/01-architecture/runtime/
├── BUSINESS_PIPELINE_ORCHESTRATION.md
├── PIPELINE_RUNTIME.md
├── RETRY_POLICY.md
├── RESOURCE_LIFECYCLE.md
└── RUNTIME_OBSERVABILITY.md

doc/03-contracts/
doc/03-infrastructure/storage/
```

---

# 96. Completion Checklist

The Preferences module is synchronized when:

* [ ] Preferences owns persistent configuration semantics;
* [ ] persistent scopes are Default, Global, and Source;
* [ ] SessionOverride ownership belongs to Reading Session;
* [ ] Preferences may resolve external SessionOverride values;
* [ ] `SessionPreferenceProfile` ownership is absent;
* [ ] PreferenceRevision semantics are explicit;
* [ ] EffectivePreferencesSnapshot is contextual and immutable;
* [ ] there is no single global EffectivePreferences state;
* [ ] Runtime Configuration is separate;
* [ ] processing modules use immutable execution configuration;
* [ ] RestartImpact action semantics are removed;
* [ ] semantic impact tags are descriptive only;
* [ ] Business Pipeline Orchestration decides processing consequences;
* [ ] provider preference is separate from provider resolution;
* [ ] user performance preferences do not override Runtime hard limits;
* [ ] Credentials remain reference-only;
* [ ] Import is atomic;
* [ ] Export is read-only;
* [ ] global EffectivePreferencesChanged event is not required;
* [ ] module state uses READY/DEGRADED rather than generic failure for ordinary errors;
* [ ] all six Preferences documents use the same ownership model.

---

# 97. Summary

Preferences v2 has three distinct flows.

Persistent preference state:

```text
Preference Command
    ↓
Preferences
    ↓
Validation
    ↓
Atomic Commit
    ↓
PreferenceRevision
```

Effective preference resolution:

```text
Default
+
Global
+
Source
+
Reading Session Override
    ↓
Preferences
    ↓
EffectivePreferencesSnapshot
```

Execution:

```text
EffectivePreferencesSnapshot
        ↓
Runtime Configuration
        ↓
ConfigurationSnapshot
        ↓
Runtime / Processing Modules
```

The ownership model is:

```text
Preferences
    owns what the user prefers

Reading Session
    owns temporary session choices

Business Pipeline Orchestration
    owns what processing those choices require

Runtime
    owns how that processing executes
```

The central invariant is:

```text
Preferences defines preference semantics.

It does not own
the active reading session
or Runtime execution.
```
