# Preferences Contract

> **Project:** CRAI
> **Module:** `preferences`
> **Path:** `doc/02-modules/preferences/CONTRACT.md`
> **Contract Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-09

---

# 1. Purpose

This document defines the public contract boundary of the Preferences module.

Preferences owns persistent user preference semantics and deterministic preference resolution.

The core contract is:

```text id="2q0dtr"
Preference Definitions
        +
Persistent Preference State
        +
Optional External Session Overrides
        ↓
Preferences Resolution
        ↓
EffectivePreferencesSnapshot
```

Preferences does not own:

```text id="3ix8qx"
Reading Session lifecycle
session override lifecycle
Runtime execution
pipeline restart decisions
stage restart decisions
Runtime retry
provider availability
accepted Artifact cache lifecycle
```

---

# 2. Contract Scope

This file defines:

```text id="wz6g8t"
Preference identifiers
PreferenceDefinition
PreferenceKey
PreferenceValue
PreferenceScope
PreferenceSet
SourcePreferenceProfile
PreferenceRevision
Preference update commands
Preference queries
SessionOverride input
EffectivePreferencesSnapshot
Preference resolution
semantic impact metadata
import/export
reset
migration
validation
versioning
security
```

It does not define:

```text id="1i4evu"
Runtime ConfigurationSnapshot
WorkItem
Attempt
RuntimeRevisionId
ReadingContextRevision
SessionConfiguration lifecycle
pipeline topology
Artifact cache invalidation execution
provider selection
credential storage
```

---

# 3. Architectural Boundary

Persistent flow:

```text id="raet0y"
Application / Settings UI
        ↓
Preference Command
        ↓
Preferences
        ↓
Validation
        ↓
Atomic Persistent Commit
        ↓
PreferenceRevision
```

Resolution flow:

```text id="qoo5pz"
Application Defaults
        +
Global Preferences
        +
Source Preferences
        +
SessionOverride Input
        ↓
ResolveEffectivePreferences
        ↓
EffectivePreferencesSnapshot
```

Execution flow remains external:

```text id="esaa9c"
EffectivePreferencesSnapshot
        ↓
Runtime Configuration Resolution
        ↓
ConfigurationSnapshot
        ↓
Runtime / Processing Modules
```

---

# 4. Contract Principles

## 4.1 Persistent Ownership

Preferences owns only persistent preference scopes:

```text id="od5z8w"
Default
Global
Source
```

Session override values may participate in resolution but remain externally owned.

---

## 4.2 Immutable Output

Published/resolved snapshots are immutable.

```text id="w08mov"
EffectivePreferencesSnapshot
```

must never change after creation.

---

## 4.3 Deterministic Resolution

Equivalent:

```text id="i7nu1b"
schema
global state
source state
session override input
```

must produce semantically equivalent effective output.

---

## 4.4 Configuration Is Not Execution

Preference contracts describe configuration semantics.

They do not issue:

```text id="9yezsy"
RestartStage
RestartPipeline
RetryProcessing
RefreshPresentation
CancelRuntime
```

---

# 5. PreferenceKey

```text id="dzf12u"
PreferenceKey
- value
```

Rules:

* stable after publication;
* globally unique within Preferences schema;
* immutable;
* rename requires migration;
* semantic meaning must not silently change.

Examples:

```text id="w8f9lb"
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

---

# 6. PreferenceRevision

```text id="l53m2b"
PreferenceRevision
- value
```

Represents committed Preferences-owned persistent state.

Rules:

1. created only by Preferences;
2. immutable;
3. monotonic within its defined scope/domain;
4. advances only on semantic change;
5. no-op does not advance it;
6. rejected update does not advance it;
7. is not RuntimeRevisionId;
8. is not ReadingContextRevision;
9. is not ConfigurationSnapshot version.

---

# 7. Revision Scope

Implementations MAY use scoped revisions such as:

```text id="ljscc0"
GlobalPreferenceRevision
SourcePreferenceRevision
```

or one aggregate revision model.

Whichever model is chosen must be explicit and deterministic.

`EffectivePreferencesSnapshot` must preserve the revision provenance required for reproducibility.

---

# 8. PreferenceScope

Persistent scope:

```text id="i7x2yb"
PreferenceScope
- Default
- Global
- Source
```

The old persisted:

```text id="yj1kq5"
Session
```

scope is removed from Preferences ownership.

---

# 9. SessionOverrideScope

For resolution only:

```text id="l0ihjy"
SessionOverrideScope
```

is an externally supplied input domain.

It is not a persisted Preferences scope.

It may be represented conceptually as:

```text id="y5hwwl"
SessionOverrideSet
├── sessionId?
├── overrideVersion?
├── values
└── createdAt?
```

Ownership belongs to Reading Session/Application.

---

# 10. PreferenceDefinition

```text id="u0ybd5"
PreferenceDefinition
├── key
├── category
├── dataType
├── defaultValue
├── allowedPersistentScopes[]
├── sessionOverrideAllowed
├── validationRules
├── sensitivity
├── semanticImpactTags[]
├── schemaVersion
├── deprecated
├── replacementKey?
└── metadata?
```

---

# 11. Removed RestartImpact

The old field:

```text id="mzb708"
RestartImpact
```

is removed.

Preferences must not encode actions such as:

```text id="9gndk0"
PresentationRefresh
StageRestart
PipelineRestart
SessionRestart
```

because these depend on active runtime/business context.

---

# 12. SemanticImpactTag

Instead, PreferenceDefinition may include:

```text id="j0avjq"
SemanticImpactTag
```

Typical values:

```text id="z3szj2"
ReadingSemantics
CaptureSemantics
RecognitionSemantics
TextProcessingSemantics
TranslationSemantics
PresentationSemantics
ProviderPreference
ResourcePreference
PrivacySemantics
ApplicationStatic
```

These describe affected semantic domains only.

---

# 13. ApplicationRestart Metadata

A preference may explicitly require application restart only when that is a true static application constraint.

Example:

```text id="0783a6"
requiresApplicationRestart = true
```

This is different from pipeline/stage restart.

---

# 14. Removed Direct CacheImpact Action

The old `CacheImpact` field is replaced by descriptive compatibility metadata.

Conceptually:

```text id="8spvof"
PreferenceCompatibilityImpact
├── affectedSemanticDomains[]
├── mayAffectArtifactCompatibility
├── mayAffectPresentationCompatibility
└── applicationStatic?
```

Actual cache invalidation is external.

---

# 15. PreferenceValue

```text id="01mvyb"
PreferenceValue
├── key
├── value
├── scope
├── scopeIdentity?
├── preferenceRevision
├── schemaVersion
└── updatedAt
```

Rules:

* immutable once committed;
* must satisfy PreferenceDefinition;
* must not contain secret material;
* session override values are not represented as persisted PreferenceValue owned by Preferences.

---

# 16. PreferenceSet

```text id="ar30ur"
PreferenceSet
├── scope
├── scopeIdentity?
├── values
├── preferenceRevision
├── schemaVersion
└── updatedAt
```

Valid persisted instances:

```text id="d6ao99"
GlobalPreferenceSet
SourcePreferenceSet
```

---

# 17. SourcePreferenceProfile

```text id="hkyjk7"
SourcePreferenceProfile
├── sourceProfileId
├── sourceIdentity
├── preferenceSet
├── sourcePreferenceRevision
├── schemaVersion
└── metadata?
```

Source identity must be logical and serializable.

Native platform handles are forbidden.

---

# 18. EffectivePreferencesSnapshot

```text id="7jceey"
EffectivePreferencesSnapshot
├── schemaVersion
├── globalPreferenceRevision
├── sourcePreferenceRevision?
├── sessionOverrideVersion?
├── resolvedValues
├── resolutionProvenance
├── semanticImpactMetadata?
└── createdAt
```

Properties:

* complete;
* valid;
* immutable;
* deterministic;
* contains no unresolved preference value;
* safe for downstream configuration resolution.

---

# 19. ResolutionProvenance

```text id="f7zbre"
ResolutionProvenance
├── globalRevision
├── sourceProfileId?
├── sourceRevision?
├── sessionOverrideRef?
├── sessionOverrideVersion?
└── valueSources[]
```

Each resolved value should be traceable to its effective source.

---

# 20. EffectivePreferencesSnapshot Is Not Runtime Configuration

Important:

```text id="8slvxg"
EffectivePreferencesSnapshot
≠
ConfigurationSnapshot
```

Effective preferences represent resolved user configuration semantics.

Runtime configuration may additionally apply:

```text id="5puo1t"
system capabilities
provider availability
hard safety limits
resource policy
application policy
```

---

# 21. SetPreference

```text id="2hhko1"
SetPreference
├── requestId
├── contractVersion
├── key
├── value
├── scope
├── scopeIdentity?
├── expectedPreferenceRevision?
└── reason?
```

Allowed persistent scopes:

```text id="n1ts5y"
Global
Source
```

`Default` values are application-defined and not user-mutated at runtime.

`Session` is invalid here.

---

# 22. SetPreferenceResult

```text id="0n8is6"
SetPreferenceResult
├── requestId
├── status
├── previousPreferenceRevision?
├── preferenceRevision?
├── changeSet?
└── rejection?
```

Possible status:

```text id="yu8pjk"
Committed
NoOp
Rejected
```

A persistent mutation does not automatically create an EffectivePreferencesSnapshot for every active session.

---

# 23. RemovePreference

```text id="xh3wzu"
RemovePreference
├── requestId
├── key
├── scope
├── scopeIdentity?
├── expectedPreferenceRevision?
└── reason?
```

Removes one explicit persisted value.

Resolution later falls back to lower-priority values.

---

# 24. ResetCategory

```text id="nhiydn"
ResetCategory
├── requestId
├── category
├── scope
├── scopeIdentity?
├── expectedPreferenceRevision?
└── reason?
```

Valid persisted scopes:

```text id="d4pk3c"
Global
Source
```

---

# 25. ResetScope

```text id="mef8gr"
ResetScope
├── requestId
├── scope
├── scopeIdentity?
├── expectedPreferenceRevision?
└── reason?
```

Supported:

```text id="nlqkch"
Global
Source
```

Session override reset belongs to Reading Session.

---

# 26. PromoteSessionOverride

Optional explicit command:

```text id="lpn2de"
PromoteSessionOverride
```

MAY be introduced later when product UX supports:

```text id="jjsqjv"
Use this setting permanently
```

Such a command must copy externally supplied session override values into a persistent Global/Source scope through normal Preferences validation.

It does not transfer Session ownership to Preferences.

---

# 27. ImportPreferences

```text id="ztcm1f"
ImportPreferences
├── requestId
├── importDocument
├── targetScopePolicy
├── expectedPreferenceRevision?
└── migrationPolicy?
```

Rules:

* full validation before commit;
* sensitive values rejected/excluded;
* atomic;
* migration applied deterministically;
* session overrides are not imported as persistent Session scope.

---

# 28. ImportPreferencesResult

```text id="gy6gse"
ImportPreferencesResult
├── status
├── importedScopes[]
├── previousRevisions
├── resultingRevisions
├── migratedFromSchemaVersion?
├── warnings[]
└── rejection?
```

---

# 29. ExportPreferences

```text id="g0pyok"
ExportPreferences
├── requestedScopes[]
├── sourceProfileIds[]?
├── includeDefaults?
└── exportFormat?
```

Supported persistent scopes:

```text id="l422y8"
Global
Source
```

Session overrides are exported by Reading Session/Application only if a separate feature explicitly requires it.

---

# 30. ExportPreferencesResult

```text id="mq2c1v"
ExportPreferencesResult
├── schemaVersion
├── exportedScopes[]
├── safePreferenceDocument
└── warnings[]
```

Secrets are never included.

---

# 31. ResolveEffectivePreferences

Primary resolution command/query:

```text id="oe57ul"
ResolveEffectivePreferences
├── requestId?
├── sourceProfileRef?
├── sessionOverrides?
├── expectedSchemaVersion?
└── resolutionContext?
```

This operation does not mutate persistent Preferences.

---

# 32. Resolution Input

```text id="m7iv0l"
EffectivePreferenceResolutionInput
├── sourceProfileRef?
├── sessionOverrideSet?
├── resolutionContext?
└── expectedSchemaVersion?
```

---

# 33. Resolution Priority

Resolution precedence:

```text id="gmzq1w"
Session Override
      ↓
Source
      ↓
Global
      ↓
Default
```

The priority remains unchanged from v1.

Ownership changes, not resolution precedence.

---

# 34. Resolution Rule

For each PreferenceKey:

```text id="z2tbem"
if valid SessionOverride exists
    use SessionOverride
else if Source value exists
    use Source
else if Global value exists
    use Global
else
    use Default
```

---

# 35. Session Override Validation

Session overrides must still satisfy:

```text id="4g5ez3"
PreferenceDefinition
value type
range
sessionOverrideAllowed
cross-field validation
security policy
```

Invalid overrides must not enter EffectivePreferencesSnapshot.

---

# 36. Session Override Persistence Rule

Resolution must never persist session override values implicitly.

Invalid:

```text id="9qc390"
Resolve EffectivePreferences
    ↓
write Session preferences into Preferences store
```

Correct:

```text id="dj9nx4"
Session overrides remain external input
```

---

# 37. GetPreferenceDefinition

```text id="3w8wdq"
GetPreferenceDefinition
- key
```

Returns:

```text id="29kjni"
PreferenceDefinition
```

including:

```text id="2c09s5"
key
type
category
default
allowedPersistentScopes
sessionOverrideAllowed
validationRules
semanticImpactTags
sensitivity
schemaVersion
deprecation metadata
```

No `RestartImpact`.

---

# 38. ListPreferenceDefinitions

```text id="gn4p86"
ListPreferenceDefinitions
├── category?
├── includeDeprecated?
└── schemaVersion?
```

Returns immutable definitions.

---

# 39. GetStoredPreference

Preferred replacement for ambiguous:

```text id="4zt6vz"
GetPreference
```

when querying persisted values.

```text id="43fqkk"
GetStoredPreference
├── key
├── scope
└── scopeIdentity?
```

Returns the explicit persisted value at that scope only.

---

# 40. Why `GetPreference` Was Ambiguous

The old contract said:

```text id="o34vdo"
GetPreference returns resolved value
```

without specifying:

```text id="6m0ne8"
source
session override context
```

Resolution requires context.

Therefore separate:

```text id="4p9qar"
GetStoredPreference
```

from:

```text id="7buwgk"
ResolveEffectivePreferences
```

---

# 41. GetEffectivePreferences

Alias/query form:

```text id="d7b8hn"
GetEffectivePreferences
```

MAY exist if it requires the full resolution input:

```text id="n2e5g6"
sourceProfileRef?
sessionOverrideSet?
```

It must not imply one global EffectivePreferences object exists for the whole application.

---

# 42. ListSourceProfiles

```text id="7s80g5"
ListSourceProfiles
```

Returns immutable source profile summaries.

---

# 43. GetSourceProfile

```text id="zm8a53"
GetSourceProfile
- sourceProfileId
```

Returns:

```text id="0fw04e"
SourcePreferenceProfile
```

---

# 44. GetPreferenceRevisions

Optional:

```text id="1cruql"
GetPreferenceRevisions
```

returns relevant current revision values.

This helps Application/Runtime Configuration build reproducible snapshots.

---

# 45. PreferenceChangeSet

```text id="xlfnqv"
PreferenceChangeSet
├── changedKeys[]
├── scope
├── scopeIdentity?
├── previousRevision
├── preferenceRevision
├── semanticImpactTags[]
└── changedAt
```

This describes configuration change.

It does not prescribe execution actions.

---

# 46. Semantic Impact Contract

Every changed preference may contribute:

```text id="br1m4l"
semanticImpactTags[]
```

Example:

```text id="gpjuej"
presentation.font_size
    → PresentationSemantics
```

```text id="fmvml6"
translation.provider_preference
    → TranslationSemantics
    → ProviderPreference
```

```text id="jkk09e"
recognition.strategy
    → RecognitionSemantics
```

---

# 47. Removed Impact Levels

The following v1 levels are removed:

```text id="ksw6cd"
NoRuntimeEffect
PresentationRefresh
StageRestart
PipelineRestart
SessionRestart
```

as action contracts.

`ApplicationRestart` may remain only as explicit static application metadata.

---

# 48. Why Stage/Pipeline Restart Is External

Determining whether a changed preference requires processing depends on:

```text id="pry6nn"
current ReadingContext
available accepted Artifacts
Artifact provenance
current Runtime intent
pipeline dependencies
reuse policy
active Presentation
```

Preferences does not own those facts.

Business Pipeline Orchestration does.

---

# 49. Cache Compatibility Metadata

Preferences may expose descriptive:

```text id="23qko0"
PreferenceCompatibilityImpact
```

Example:

```text id="817i3g"
target language changed
    → TranslationSemantics affected
```

It must not command:

```text id="94w62e"
delete Translation cache
```

---

# 50. Revision Contract

A successful persistent semantic change creates a new PreferenceRevision.

No-op:

```text id="56ojy4"
same effective stored value
```

should not create a new revision.

Rejected mutations do not advance revision.

---

# 51. PreferenceRevision Conflict

Mutating commands MAY use:

```text id="59dsxt"
expectedPreferenceRevision
```

Guard:

```text id="nl1wec"
expected == current
```

Mismatch returns:

```text id="faek4x"
PreferenceRevisionConflict
```

without mutation.

---

# 52. Resolution Does Not Create PreferenceRevision

Calling:

```text id="i9kxnt"
ResolveEffectivePreferences
```

does not increment persistent PreferenceRevision.

Resolution is read-only.

---

# 53. Effective Snapshot Identity

An EffectivePreferencesSnapshot may expose a derived:

```text id="s5erjs"
effectiveSnapshotId
```

or deterministic fingerprint.

This identity is not a persistent PreferenceRevision.

---

# 54. Session Override Version

If Reading Session provides:

```text id="1busfg"
sessionOverrideVersion
```

Preferences may include it in resolution provenance/cache keys.

Preferences does not create or advance that version.

---

# 55. Validation Contract

Persistent update validation includes:

```text id="bcqi02"
PreferenceKey existence
data type
range
enum
persistent scope
scope identity
cross-field consistency
schema compatibility
security/privacy constraints
deprecation rules
```

---

# 56. Runtime Availability Validation Is External

Preferences should not reject a stable preference merely because:

```text id="pf5ak0"
GPU unavailable right now
provider unhealthy right now
network temporarily offline
```

Those conditions are dynamic.

Runtime/provider resolution handles them.

---

# 57. Static Capability Validation

Preferences MAY reject configuration that is statically impossible for the application build/profile.

Example:

```text id="r08nu7"
preference refers to feature
not included in this application edition
```

This must not depend on volatile runtime state.

---

# 58. Security Contract

Preferences must never expose:

```text id="s00kdp"
API keys
authentication tokens
passwords
provider secrets
private certificates
credential contents
```

---

# 59. CredentialRef

Safe preferences may contain:

```text id="l4gxh4"
CredentialRef
```

Conceptually:

```text id="grm2z6"
CredentialRef
├── credentialId
├── credentialKind
└── displayLabel?
```

Actual secret retrieval belongs to Credential Store/provider infrastructure.

---

# 60. Privacy Preference Contract

Privacy-related preference values remain user intent.

They cannot override stronger system policy.

Example:

```text id="gsugv1"
allowRemoteTranslation = true
```

does not guarantee remote translation may execute.

Final permission may require:

```text id="tfrj17"
user preference
AND
system policy
AND
runtime capability
```

---

# 61. Import Contract

Import validates:

```text id="4f3qrm"
schema version
preference keys
scope
values
source profile identity
migration compatibility
sensitive-reference safety
```

Commit is atomic.

---

# 62. Export Contract

Export includes:

```text id="1l9ohu"
schemaVersion
safe values
scope
source identity where safe
```

and excludes secrets.

---

# 63. Migration Contract

Migration input:

```text id="hh0f0p"
stored preference state
+
source schema version
+
target schema version
```

Output:

```text id="2e9653"
validated Candidate Preference State
```

Migration must complete before authoritative commit.

---

# 64. Migration Failure

Migration failure must preserve last known valid persistent state where possible.

Invalid migrated Candidate must not partially replace current state.

Detailed errors belong to `ERRORS.md`.

---

# 65. Deprecated Keys

Deprecated key handling may include:

```text id="rn4em9"
read old key
map to replacement
warn
migrate on write
remove only after defined removal version
```

---

# 66. Published Events

Core Preferences-owned facts may include:

```text id="7mhhg4"
PreferenceChanged
PreferenceRemoved
PreferenceReset
SourcePreferenceProfileCreated
SourcePreferenceProfileDeleted
PreferenceImportCompleted
PreferenceMigrationCompleted
```

Detailed semantics belong to:

```text id="w3bqen"
EVENTS.md
```

---

# 67. EffectivePreferencesChanged

A global event:

```text id="mgddhu"
EffectivePreferencesChanged
```

is not required for MVP.

Reason:

EffectivePreferencesSnapshot may depend on:

```text id="wxz3rk"
Source context
Session override input
```

and therefore many context-specific effective views may exist simultaneously.

---

# 68. If Effective Snapshot Events Are Added

They must include an explicit resolution identity such as:

```text id="bw1myc"
sourceProfileRef
sessionId?
sessionOverrideVersion?
effectiveSnapshotId
```

and ownership must remain clear.

Do not publish one ambiguous global effective-state event.

---

# 69. Removed Consumed Session Events

Preferences v2 does not require direct subscriptions to:

```text id="9gpq67"
SessionCreated
SessionClosed
```

to create/delete session overrides.

Reading Session owns those overrides.

---

# 70. Storage Load Boundary

Preferences should initialize through explicit Storage/Application lifecycle contracts.

A mandatory public:

```text id="5d59bh"
StorageLoaded event
```

subscription is not required for correctness.

---

# 71. Import Completion Boundary

Preferences itself owns:

```text id="gqakag"
ImportPreferences command
```

and therefore does not need to consume a separate:

```text id="xtifx0"
ImportCompleted
```

event to activate its own import.

---

# 72. Direct Consumed Events

Recommended MVP:

```text id="uuzf1q"
None required.
```

Preferences uses explicit commands/queries.

Optional infrastructure subscriptions may exist only as replaceable implementation details.

---

# 73. Event Publication Timing

Successful preference facts occur after persistent state commit.

```text id="8d9gyy"
validate
    ↓
Candidate state
    ↓
atomic commit
    ↓
PreferenceRevision advances
    ↓
publish Preference fact
```

---

# 74. Event Publication Failure

If preference commit succeeds but event publication fails:

```text id="5q52zq"
new Preference state remains committed
```

Do not rerun the mutation merely to recreate the event.

Infrastructure/outbox/reconciliation handles publication recovery.

---

# 75. Public Error Contract

Conceptually:

```text id="5p0u5c"
PreferenceError
├── errorCode
├── category
├── severity
├── recoveryHint?
├── preferenceKey?
├── scope?
├── scopeIdentity?
├── expectedPreferenceRevision?
├── currentPreferenceRevision?
├── schemaVersion?
├── requestId?
├── correlationId?
└── diagnosticRef?
```

---

# 76. Error Categories

Typical:

```text id="4nc5n9"
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

Detailed codes belong to `ERRORS.md`.

---

# 77. No Runtime Errors

Preferences must not return internal codes for:

```text id="7z53ou"
StageRestartFailed
PipelineRestartFailed
ProviderUnavailable
RuntimeTimeout
RetryExhausted
ArtifactInvalidationFailed
```

Those belong elsewhere.

---

# 78. Idempotency

Commands should be idempotent where semantics allow.

Examples:

```text id="8gqom5"
Set same stored value
Remove absent value
Reset already empty scope/category
```

may resolve as `NoOp`.

No-op does not advance PreferenceRevision.

---

# 79. Concurrent Mutation

Persistent mutation for the same revision domain must be logically serialized.

Example:

```text id="50fsej"
Current revision = 10

Command A expects 10
Command B expects 10

B commits 11

A reaches commit
    ↓
PreferenceRevisionConflict
```

---

# 80. Processing Module Boundary

Processing modules should receive:

```text id="k2xvkj"
ConfigurationSnapshotRef
```

or equivalent immutable execution-ready configuration from Runtime/Application.

They should not:

```text id="m24dk9"
query Preferences during processing
subscribe to PreferenceChanged directly
resolve scope precedence
```

---

# 81. Runtime Configuration Boundary

Runtime Configuration may consume:

```text id="wljc1g"
EffectivePreferencesSnapshot
```

and derive:

```text id="mszyqg"
ConfigurationSnapshot
```

using:

```text id="hpcdpq"
hard limits
capabilities
provider availability
application policy
resource state
```

Preferences does not own this derivation unless a separate configuration component is explicitly assigned.

---

# 82. Reading Session Boundary

Reading Session may use persistent/effective preferences to initialize:

```text id="38qhyd"
SessionConfiguration
```

Reading Session then owns:

```text id="kek79f"
temporary session-specific configuration
```

A persistent Preferences update does not silently mutate Reading Session state.

---

# 83. Business Pipeline Boundary

Preference changes may be consumed by Application/Business Pipeline Orchestration.

Input:

```text id="4m8x4n"
PreferenceChangeSet
+
semanticImpactTags
+
current ReadingContext
+
available Artifacts
```

Output:

```text id="b5p6f3"
pipeline decision
```

Preferences does not produce that decision.

---

# 84. Provider Boundary

A preference may say:

```text id="s4rmuv"
translation.provider_preference = LocalFirst
```

Provider Resolution later decides:

```text id="msvmo3"
which currently usable provider instance
```

based on live capability/health.

---

# 85. Version Contract

Public contract uses:

```text id="ld6a4e"
ContractVersion
PreferenceSchemaVersion
PreferenceRevision provenance
```

These are separate identities.

---

# 86. ContractVersion

Describes public API/schema compatibility.

---

# 87. PreferenceSchemaVersion

Describes preference-key/value schema.

---

# 88. PreferenceRevision

Describes committed persistent preference state.

---

# 89. EffectiveSnapshot Version

Optional derived effective snapshot identity describes one resolution result.

It does not replace any of the above.

---

# 90. Unknown Fields

Unknown optional fields should be ignored when safe.

Unknown required enum values must be rejected or handled using explicitly documented compatibility fallback.

---

# 91. Architecture Invariants

1. Preferences owns persistent preference semantics.

2. Persistent scopes are Default, Global, and Source.

3. Session overrides are externally owned.

4. Session overrides may participate in resolution.

5. Preferences does not persist session overrides implicitly.

6. `SessionPreferenceProfile` is not a Preferences-owned active-session contract.

7. Resolution precedence remains Session Override → Source → Global → Default.

8. PreferenceDefinition does not contain Stage/Pipeline restart actions.

9. SemanticImpactTags describe affected domains.

10. Business Pipeline Orchestration interprets processing consequences.

11. Cache invalidation execution is external.

12. PreferenceRevision is Preferences-owned.

13. PreferenceRevision is not RuntimeRevisionId.

14. PreferenceRevision is not ReadingContextRevision.

15. Resolution does not advance PreferenceRevision.

16. EffectivePreferencesSnapshot is immutable.

17. EffectivePreferencesSnapshot is not Runtime ConfigurationSnapshot.

18. Processing modules do not resolve scopes independently.

19. Processing modules do not read live Preferences during execution.

20. Preferences does not select provider by live availability.

21. Preferences does not create Runtime WorkItems.

22. Preferences does not retry Runtime work.

23. Preferences does not restart stages or pipelines.

24. Preferences does not cancel Runtime work.

25. Preferences does not store credential secrets.

26. User preferences cannot override stronger security policy.

27. Updates are atomic.

28. Invalid updates preserve current state.

29. No-op updates do not advance revision.

30. Event publication occurs after persistent state commit.

31. Event publication failure does not roll back valid committed Preferences state.

32. Preferences has no mandatory SessionCreated/SessionClosed event subscription.

33. Public contracts remain serializable.

34. Sensitive values remain protected.

---

# 92. Example — Persistent Global Change

```text id="92cgzu"
SetPreference
key = translation.style
scope = Global
value = Natural
        ↓
validate
        ↓
commit
        ↓
GlobalPreferenceRevision 20 → 21
        ↓
PreferenceChanged
```

No pipeline restart occurs inside Preferences.

---

# 93. Example — Source Preference

```text id="i3v7bl"
Global:
target_language = vi

Source profile:
target_language = en

Resolve
    ↓
effective target_language = en
```

---

# 94. Example — Session Override

```text id="i8zgyv"
Global = vi
Source = vi
Reading Session Override = en
        ↓
ResolveEffectivePreferences
        ↓
effective = en
```

Preferences does not store the session override.

---

# 95. Example — Preference Change During Runtime Attempt

```text id="s73o50"
Attempt A starts
ConfigurationSnapshot C1
        ↓
PreferenceChanged
        ↓
new EffectivePreferencesSnapshot E2
```

Attempt A continues using:

```text id="75xjja"
C1
```

unless Runtime establishes newer execution authority separately.

---

# 96. Example — Provider Preference

```text id="ruevto"
SetPreference:
translation.provider_preference = CloudPreferred
        ↓
PreferenceChanged
        ↓
ProviderPreference semantic impact
```

Provider Resolution later selects actual provider.

---

# 97. Example — Presentation Font Size

```text id="vlztqf"
presentation.font_size = 20
        ↓
PreferenceChanged
        ↓
PresentationSemantics impact
```

Application/Business Pipeline/Presentation decides how active output is updated.

Preferences does not emit:

```text id="j1sp9c"
PresentationRefresh
```

command.

---

# 98. Related Documents

```text id="hzxjql"
doc/02-modules/preferences/MODULE.md
doc/02-modules/preferences/STATES.md
doc/02-modules/preferences/EVENTS.md
doc/02-modules/preferences/ERRORS.md
doc/02-modules/preferences/README.md

doc/02-modules/reading-session/MODULE.md
doc/02-modules/reading-session/CONTRACT.md

doc/01-architecture/modules/OWNERSHIP_MAP.md
doc/01-architecture/modules/MODULE_DEPENDENCY.md

doc/01-architecture/runtime/BUSINESS_PIPELINE_ORCHESTRATION.md
doc/01-architecture/runtime/PIPELINE_RUNTIME.md
doc/01-architecture/runtime/RETRY_POLICY.md
doc/01-architecture/runtime/RUNTIME_OBSERVABILITY.md

doc/03-infrastructure/storage/
doc/03-contracts/
```

---

# 99. Completion Criteria

This contract is synchronized when:

* Session is removed as Preferences-owned persistent scope;
* session override input remains supported;
* ResetScope no longer resets Session;
* SessionCreated/SessionClosed consumed events are removed;
* `RestartImpact` is removed;
* `CacheImpact` action ownership is removed;
* semantic impact metadata replaces restart actions;
* EffectivePreferences becomes immutable `EffectivePreferencesSnapshot`;
* `GetPreference` ambiguity is removed;
* persistent reads and effective resolution are separate;
* PreferenceRevision concurrency is explicit;
* processing modules consume execution snapshots instead of live Preferences;
* provider preference is distinct from live provider resolution;
* event publication happens after commit;
* event publication failure preserves committed preference state;
* sensitive values remain reference-only.

---

# 100. Summary

Preferences v2 exposes two distinct boundaries.

Persistent preference mutation:

```text id="p9pxsb"
Preference Command
    ↓
Preferences
    ↓
Validation
    ↓
Atomic Commit
    ↓
PreferenceRevision
    ↓
Preference Fact
```

Effective resolution:

```text id="5bxhho"
Default
+
Global
+
Source
+
External Session Overrides
    ↓
Preferences
    ↓
EffectivePreferencesSnapshot
```

Execution remains external:

```text id="bf71iw"
EffectivePreferencesSnapshot
        ↓
Runtime Configuration
        ↓
ConfigurationSnapshot
        ↓
Runtime / Processing Module
```

The central contract rule is:

```text id="g9pynn"
Preferences owns
persistent user preference semantics.

Reading Session owns
temporary session choices.

Business Pipeline Orchestration owns
processing consequences.

Runtime owns
execution.
```
