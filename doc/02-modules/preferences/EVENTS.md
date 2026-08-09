# Preferences Events

> **Project:** CRAI
> **Module:** `preferences`
> **Path:** `doc/02-modules/preferences/EVENTS.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-09

---

# 1. Purpose

This document defines the event boundary of the Preferences module.

Preferences publishes immutable facts about committed Preferences-owned persistent state.

Typical facts include:

```text
PreferenceChanged
PreferenceRemoved
PreferenceReset
SourcePreferenceProfileCreated
SourcePreferenceProfileDeleted
PreferenceImportCompleted
PreferenceMigrationCompleted
```

Preferences events do not represent:

```text
Runtime execution state
Reading Session state
SessionOverride lifecycle
pipeline restart commands
cache invalidation commands
provider availability
EffectivePreferencesSnapshot lifecycle
```

---

# 2. Core Event Principle

An event describes:

> A Preferences-owned fact that has already been committed.

Correct:

```text
PreferenceChanged
PreferenceRemoved
PreferenceMigrationCompleted
```

Incorrect:

```text
RestartTranslation
RefreshPresentation
InvalidateRecognitionCache
ApplyPreferencesToSession
```

Those are actions owned by other components.

---

# 3. Event Ownership

Preferences owns events about:

```text
persistent Global preferences
persistent Source preferences
PreferenceRevision
preference schema migration
preference import
source preference profile lifecycle
```

Preferences does not own events about:

```text
SessionConfiguration
SessionOverride
RuntimeRevision
WorkItem
Attempt
processing stage restart
provider health
Artifact lifecycle
Presentation lifecycle
```

---

# 4. Event Categories

Preferences v2 has three main public event groups:

```text
Preferences Events
├── Persistent Preference Events
├── Source Profile Events
└── Schema / Import Events
```

There is no mandatory global:

```text
EffectivePreferencesChanged
```

event.

---

# 5. Canonical Event Envelope

Preferences events follow the CRAI canonical Event Convention.

Conceptually:

```text
EventEnvelope
├── eventId
├── eventType
├── eventVersion
├── occurredAt
├── producer
├── aggregateId?
├── aggregateVersion?
├── correlationId?
├── causationId?
├── traceId?
├── payload
└── metadata?
```

The canonical Event Convention remains authoritative if exact field names differ.

Preferences must not define a competing global envelope.

---

# 6. Producer

Preferences-owned events use:

```text
producer = preferences
```

---

# 7. PreferenceRevision in Events

Events describing committed persistent preference mutations should carry the relevant:

```text
PreferenceRevision
```

or scoped revision such as:

```text
GlobalPreferenceRevision
SourcePreferenceRevision
```

according to the final revision model.

Rules:

1. revision is committed before event publication;
2. rejected commands emit no success event;
3. no-op updates emit no mutation event;
4. resolution does not create PreferenceRevision;
5. SessionOverride changes do not create PreferenceRevision.

---

# 8. SchemaVersion

Preferences events that depend on preference schema should include:

```text
PreferenceSchemaVersion
```

This identifies the semantic schema used for the committed state.

It is not the same as:

```text
EventVersion
PreferenceRevision
ContractVersion
```

---

# 9. Persistent Preference Event Set

Recommended core events:

```text
PreferenceChanged
PreferenceRemoved
PreferenceReset
```

These describe committed persistent values only.

---

# 10. PreferenceChanged

## Meaning

One or more persistent preference values were successfully created or changed.

## Payload

```text
PreferenceChangedPayload
├── scope
├── scopeIdentity?
├── previousPreferenceRevision
├── preferenceRevision
├── changedKeys[]
├── semanticImpactTags[]
├── schemaVersion
└── changedAt
```

Valid scopes:

```text
Global
Source
```

Session is not a Preferences-owned persistent scope.

---

# 11. PreferenceChanged Invariants

`PreferenceChanged` means:

```text
persistent preference state changed
```

It does not mean:

```text
active Reading Session changed
Runtime work restarted
cache invalidated
Presentation refreshed
```

Consumers decide how to react through their own ownership rules.

---

# 12. PreferenceRemoved

## Meaning

One or more explicit persistent values were removed.

## Payload

```text
PreferenceRemovedPayload
├── scope
├── scopeIdentity?
├── previousPreferenceRevision
├── preferenceRevision
├── removedKeys[]
├── semanticImpactTags[]
└── removedAt
```

Resolution may subsequently fall back to a lower-priority scope.

---

# 13. PreferenceReset

## Meaning

A persistent scope/category/key set was reset.

Examples:

```text
Global category reset
Source profile category reset
Source scope reset
```

## Payload

```text
PreferenceResetPayload
├── scope
├── scopeIdentity?
├── resetKind
├── affectedKeys[]
├── previousPreferenceRevision
├── preferenceRevision
├── semanticImpactTags[]
└── resetAt
```

Session override reset is not owned by Preferences.

---

# 14. Source Profile Event Set

Recommended:

```text
SourcePreferenceProfileCreated
SourcePreferenceProfileDeleted
SourcePreferenceProfileChanged
```

The third event is optional if normal `PreferenceChanged` with Source scope is sufficient.

---

# 15. SourcePreferenceProfileCreated

## Meaning

A new persistent Source preference profile became authoritative.

## Payload

```text
SourcePreferenceProfileCreatedPayload
├── sourceProfileId
├── sourceIdentity
├── sourcePreferenceRevision
├── schemaVersion
└── createdAt
```

Source identity must remain platform-independent and privacy-safe.

---

# 16. SourcePreferenceProfileDeleted

## Meaning

A Source preference profile was removed from persistent Preferences state.

## Payload

```text
SourcePreferenceProfileDeletedPayload
├── sourceProfileId
├── previousSourcePreferenceRevision?
├── reason?
└── deletedAt
```

Existing historical EffectivePreferencesSnapshots remain immutable.

---

# 17. SourcePreferenceProfileChanged

Optional event:

```text
SourcePreferenceProfileChanged
```

may be emitted when profile-level metadata changes independently from individual preference values.

Avoid duplicating information already represented by `PreferenceChanged`.

---

# 18. Import Event Set

Recommended:

```text
PreferenceImportCompleted
```

Optional:

```text
PreferenceImportRejected
```

is not required for MVP because command failure may remain in command/error contracts and diagnostics.

---

# 19. PreferenceImportCompleted

## Meaning

A preference import successfully committed persistent state.

## Payload

```text
PreferenceImportCompletedPayload
├── importedScopes[]
├── previousRevisions
├── resultingRevisions
├── sourceSchemaVersion
├── targetSchemaVersion
├── importedKeyCount
├── semanticImpactTags[]
└── completedAt
```

---

# 20. Import Completion Is Not Effective State

`PreferenceImportCompleted` does not imply:

```text
all active sessions adopted imported values
all EffectivePreferencesSnapshots changed
all Runtime configuration changed
```

Those decisions are external.

---

# 21. Export Event Policy

A successful export is normally a read-side operation.

Therefore a public:

```text
PreferenceExported
```

event is not required for architecture correctness.

If product audit/history requires it later, it may be added as an audit event.

It should not participate in PreferenceRevision ordering because export does not mutate persistent state.

---

# 22. Migration Event Set

Recommended:

```text
PreferenceMigrationCompleted
```

Optional failure reporting remains in errors/diagnostics.

---

# 23. PreferenceMigrationCompleted

## Meaning

Persistent preference state/schema was successfully migrated and committed.

## Payload

```text
PreferenceMigrationCompletedPayload
├── previousSchemaVersion
├── schemaVersion
├── previousRevisions
├── resultingRevisions
├── migratedKeys[]
├── warnings[]
└── completedAt
```

---

# 24. Validation Failures Are Not Success Facts

The previous event:

```text
PreferenceValidationFailed
```

is not required as a public domain event for MVP.

Invalid commands return:

```text
PreferenceError / Rejected result
```

and may generate diagnostics/metrics.

Reason:

not every rejected user input is an architectural domain fact that all modules need to subscribe to.

---

# 25. Removed `EffectivePreferencesChanged`

The v1 event:

```text
EffectivePreferencesChanged
```

is removed from the default public event set.

Reason:

```text
EffectivePreferencesSnapshot
=
persistent preferences
+
Source context
+
SessionOverride input
```

There may be many effective snapshots simultaneously.

No single global effective state exists.

---

# 26. Why Global Effective Change Is Ambiguous

Example:

```text
Global target_language = vi

Session A override = en
Session B override = ja
```

After a persistent Global change:

```text
Session A may still resolve en
Session B may still resolve ja
Session C may resolve the new global value
```

Therefore:

```text
EffectivePreferencesChanged
```

without explicit context identity is ambiguous.

---

# 27. Effective Resolution Is Query/Result

Preferred:

```text
ResolveEffectivePreferences
        ↓
EffectivePreferencesSnapshot
```

This is a deterministic read operation.

It does not publish an event by default.

---

# 28. Optional Effective Snapshot Event

A future event may exist only if there is a clear consumer and explicit resolution identity.

Example:

```text
EffectivePreferencesSnapshotResolved
├── effectiveSnapshotId
├── sourceProfileId?
├── sessionId?
├── sessionOverrideVersion?
├── persistentRevisionProvenance
└── resolvedAt
```

This should be treated as contextual/audit data, not one global configuration state.

---

# 29. SemanticImpactTags in Events

Preference mutation events may carry:

```text
semanticImpactTags[]
```

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

---

# 30. No Restart Commands in Events

Preferences events must not contain action fields such as:

```text
restartStage = true
restartPipeline = true
refreshPresentation = true
invalidateCache = true
cancelRuntime = true
```

Those require external context.

---

# 31. Business Pipeline Consumption

Business Pipeline Orchestration/Application may consume:

```text
PreferenceChanged
PreferenceRemoved
PreferenceReset
```

and combine them with:

```text
current ReadingContext
current SessionConfiguration
available Artifacts
Runtime state
```

to determine consequences.

Preferences does not make that decision.

---

# 32. Reading Session Consumption

Reading Session should not automatically mutate because a persistent Preference event occurred.

Preferred flow:

```text
PreferenceChanged
        ↓
Application policy
        ↓
Should active session adopt this change?
        ↓
Reading Session command if yes
```

This preserves session ownership.

---

# 33. Processing Module Consumption

Processing modules should generally not subscribe directly to Preferences events.

Invalid:

```text
PreferenceChanged
    ↓
Recognition reloads live preference
```

Preferred:

```text
new Runtime execution
    ↓
ConfigurationSnapshot
    ↓
Recognition
```

---

# 34. No Direct Session Event Consumption

Preferences v2 does not require:

```text
SessionCreated
SessionClosed
```

subscriptions.

Reading Session owns SessionOverride lifecycle.

---

# 35. No `StorageLoaded` Event Dependency

Preferences initialization should use explicit initialization/persistence contracts.

A mandatory:

```text
StorageLoaded
```

Event Bus subscription is not required for correctness.

---

# 36. No `ImportRequested` Event Dependency

Import is invoked through:

```text
ImportPreferences
```

command.

Preferences does not need to subscribe to `ImportRequested` as a hidden command channel.

---

# 37. No `MigrationRequested` Event Dependency

Migration should be invoked through explicit lifecycle/application contract.

Do not use Event Bus as hidden command transport.

---

# 38. Direct Consumed Events

Recommended MVP:

```text
None required.
```

Preferences operates through explicit commands/queries.

Optional infrastructure events may exist internally if correctness does not depend on them.

---

# 39. Event Ordering

Preferences guarantees logical ordering only within the relevant persistent revision domain.

Example:

```text
GlobalPreferenceRevision 20
    ↓
PreferenceChanged revision 21
    ↓
PreferenceReset revision 22
```

Consumers should use revision provenance rather than global transport order.

---

# 40. No Effective Event Ordering Chain

The v1 chain:

```text
PreferenceChanged
    ↓
EffectivePreferencesChanged
```

is removed.

There is no mandatory second event after each persistent mutation.

---

# 41. Import Ordering

Preferred:

```text
ImportPreferences command
        ↓
atomic commit
        ↓
PreferenceImportCompleted
```

If individual `PreferenceChanged` events are also emitted for imported keys, the architecture must explicitly define whether that duplication is necessary.

MVP should prefer one clear import completion fact plus committed revision metadata.

---

# 42. Migration Ordering

Preferred:

```text
migration operation
    ↓
atomic commit
    ↓
PreferenceMigrationCompleted
```

No `EffectivePreferencesChanged` event is required afterward.

---

# 43. Event Idempotency

Every event has:

```text
EventId
```

Consumers must tolerate duplicate delivery according to canonical Event Bus semantics.

A duplicate event must not produce duplicate logical state changes.

---

# 44. PreferenceRevision Is Not Event Identity

Two events may theoretically refer to the same committed revision for different facts.

Therefore idempotency uses:

```text
EventId
```

not merely:

```text
PreferenceRevision
```

---

# 45. Event Delivery Semantics

The v1 hard-coded guarantee:

```text
At-least-once
Ordered by PreferenceRevision
```

is removed from module-local specification.

Delivery guarantees belong to:

```text
EVENT_BUS.md
Runtime/Event Bus profile
```

Preferences consumers must obey the canonical delivery contract.

---

# 46. Why Delivery Is External

Different Runtime profiles may provide different:

```text
durability
replay
ordering
delivery
```

semantics.

Preferences should not independently redefine those infrastructure guarantees.

---

# 47. Stale Event Handling

Consumers maintaining projections may compare:

```text
PreferenceRevision
```

within the same revision domain.

A lower stale revision must not overwrite newer projection state.

---

# 48. Cross-Scope Ordering

Do not compare:

```text
GlobalPreferenceRevision
```

numerically with:

```text
SourcePreferenceRevision
```

unless the final contract explicitly defines one global revision domain.

Scoped revision domains are independent.

---

# 49. Event Publication Timing

Correct:

```text
validate
    ↓
Candidate persistent state
    ↓
atomic commit
    ↓
PreferenceRevision advances
    ↓
publish event
```

Incorrect:

```text
publish PreferenceChanged
    ↓
attempt commit
```

---

# 50. Event Publication Failure

If:

```text
PreferenceRevision 30 → 31
```

commits successfully but event publication fails:

```text
revision 31 remains authoritative
```

Do not roll back valid persistent state merely to recreate the event.

---

# 51. Publication Recovery

Possible infrastructure mechanisms:

```text
outbox
publication retry
projection reconciliation
query-based recovery
```

These belong to infrastructure/application policy.

Preferences does not rerun the preference command.

---

# 52. Events Are Not State

Historical:

```text
PreferenceChanged revision 12
```

remains true even if current state is:

```text
revision 20
```

Consumers needing current values should query Preferences.

---

# 53. Events Are Not Resolution Output

A persistent Preference event does not include a fully resolved:

```text
EffectivePreferencesSnapshot
```

for every possible Source/Session context.

That would be impossible to define globally.

---

# 54. Event Payload Size

Preference events should remain small.

Prefer:

```text
changedKeys
scope
scopeIdentity
revision
semanticImpactTags
schemaVersion
```

Avoid embedding entire preference stores unless explicitly required.

---

# 55. Sensitive Preference Events

Events must never expose secret material.

Allowed:

```text
credentialRef changed
provider account reference changed
private endpoint reference changed
```

Forbidden:

```text
API key
token
password
secret endpoint credential
certificate private key
```

---

# 56. Source Identity Privacy

Source profile identities may reveal:

```text
domain
document
application
user-defined profile
```

Use bounded logical identifiers and redact sensitive metadata where appropriate.

---

# 57. Event Versioning

Each event has its own:

```text
EventVersion
```

This is independent from:

```text
PreferenceSchemaVersion
PreferenceRevision
ContractVersion
```

---

# 58. Compatible Event Changes

Compatible additions may include:

```text
optional metadata
optional impact tags
optional diagnostics reference
```

Incompatible semantic changes require major event version change.

---

# 59. PreferenceChanged Example

```text
PreferenceChanged
├── eventId
├── eventVersion
├── schemaVersion = 5
├── scope = Global
├── previousPreferenceRevision = 20
├── preferenceRevision = 21
├── changedKeys =
│   └── translation.style
├── semanticImpactTags =
│   └── TranslationSemantics
└── changedAt
```

This does not mean Translation has restarted.

---

# 60. Source Preference Example

```text
PreferenceChanged
├── scope = Source
├── sourceProfileId = comic-site-A
├── previousSourcePreferenceRevision = 7
├── sourcePreferenceRevision = 8
├── changedKeys =
│   └── reading.source_language
└── semanticImpactTags =
    └── ReadingSemantics
```

---

# 61. Session Override Example

A user changes target language only for the active Reading Session.

Correct:

```text
Reading Session
    ↓
SessionConfiguration changed
```

Preferences event:

```text
none
```

unless the user explicitly persists/promotes that value to Global/Source preferences.

---

# 62. Promotion Example

If product supports:

```text
Use this setting permanently
```

flow may be:

```text
Reading Session override
    ↓
PromoteSessionOverride
    ↓
Preferences commit persistent value
    ↓
PreferenceChanged
```

Only the persistent promotion emits Preferences event.

---

# 63. Import Example

```text
ImportPreferences
    ↓
parse
    ↓
validate
    ↓
migrate if needed
    ↓
atomic commit
    ↓
PreferenceImportCompleted
```

No global EffectivePreferences event follows automatically.

---

# 64. Migration Example

```text
Schema v4 stored state
    ↓
migration
    ↓
target schema validation
    ↓
atomic commit to schema v5
    ↓
PreferenceMigrationCompleted
```

---

# 65. Export Example

```text
ExportPreferences
    ↓
read committed snapshot
    ↓
remove sensitive values
    ↓
serialize
```

No Preferences state mutation event is required.

---

# 66. Error Event Policy

Normal command errors remain:

```text
command rejection
+
PreferenceError
+
diagnostics
```

rather than public Event Bus events.

Examples:

```text
invalid value
revision conflict
unsupported scope
invalid import
```

---

# 67. Why `PreferenceValidationFailed` Is Removed

Validation failure is usually:

```text
a request outcome
```

not:

```text
a persistent Preferences fact
```

Publishing every invalid UI input to the Event Bus would create noise and unnecessary coupling.

Use diagnostics/metrics where needed.

---

# 68. Observability

Recommended metrics:

```text
preferences_change_total
preferences_reset_total
preferences_remove_total
preferences_import_total
preferences_migration_total
preferences_revision_conflict_total
preferences_event_publish_failure_total
preferences_resolution_total
preferences_resolution_duration_ms
```

Resolution metrics do not require resolution events.

---

# 69. Event Consumers

Potential consumers:

```text
Application
Business Pipeline Orchestration
Persistence projection
Audit/history
Diagnostics
Settings UI projection
```

Processing modules should normally consume ConfigurationSnapshot instead.

---

# 70. Testing — Ownership

Tests must verify Preferences does not publish:

```text
SessionOverrideChanged
RuntimeRestartRequested
PipelineRestartRequested
PresentationRefreshRequested
ArtifactCacheInvalidationRequested
ProviderHealthChanged
```

---

# 71. Testing — Session Independence

Verify:

```text
SessionCreated
SessionClosed
SessionConfigurationChanged
```

do not require Preferences event handlers for correctness.

---

# 72. Testing — Effective Snapshot Independence

Verify resolving:

```text
Session A snapshot
Session B snapshot
Source X snapshot
Source Y snapshot
```

does not publish one ambiguous global `EffectivePreferencesChanged`.

---

# 73. Testing — Revision

Verify:

```text
semantic persistent change
    → event carries new revision

no-op
    → no mutation event

rejected update
    → no success event

SessionOverride change
    → no PreferenceRevision event
```

---

# 74. Testing — Event Publication Failure

Verify:

```text
persistent commit succeeds
event publication fails
```

does not roll back committed Preferences state.

---

# 75. Testing — Delivery Independence

Preferences core tests should not assume module-local:

```text
AtLeastOnce
global ordering
```

Delivery behavior should be tested against canonical Event Bus profile.

---

# 76. Testing — Privacy

Verify event payloads do not contain:

```text
secret
credential contents
token
password
private certificate
unsafe source metadata
```

---

# 77. Deprecated v1 Events

Removed/deprecated:

```text
EffectivePreferencesChanged
PreferenceValidationFailed
PreferenceExported
```

as core domain facts.

Removed consumed event dependencies:

```text
StorageLoaded
SessionCreated
SessionClosed
ImportRequested
MigrationRequested
```

Replacement:

```text
explicit commands/queries
persistent preference facts
contextual effective resolution
```

---

# 78. Architecture Invariants

1. Preferences events describe Preferences-owned persistent facts only.

2. Events are immutable.

3. Events are published after commit.

4. No-op updates emit no mutation event.

5. Rejected updates emit no success event.

6. SessionOverride lifecycle emits no Preferences event.

7. Preferences does not own Reading Session events.

8. Preferences does not own Runtime events.

9. Preferences events contain semantic impact metadata only.

10. Preferences events do not contain restart commands.

11. Preferences events do not directly invalidate caches.

12. `EffectivePreferencesChanged` is not a mandatory global event.

13. Effective resolution is query/result based.

14. Multiple EffectivePreferencesSnapshots may coexist.

15. PreferenceRevision orders persistent state only.

16. Scoped PreferenceRevisions must not be compared across domains unless explicitly defined.

17. EventId is the idempotency identity.

18. Delivery guarantees belong to canonical Event Bus.

19. Event publication failure does not roll back committed preference state.

20. Processing modules should not use Preferences events as live configuration.

21. Public events remain small and privacy-safe.

22. Secrets never appear in event payloads.

---

# 79. Related Documents

```text
doc/02-modules/preferences/MODULE.md
doc/02-modules/preferences/CONTRACT.md
doc/02-modules/preferences/STATES.md
doc/02-modules/preferences/ERRORS.md
doc/02-modules/preferences/README.md

doc/02-modules/reading-session/EVENTS.md
doc/02-modules/reading-session/CONTRACT.md

doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/core/EVENT_CONVENTION.md

doc/01-architecture/modules/OWNERSHIP_MAP.md
doc/01-architecture/modules/MODULE_DEPENDENCY.md

doc/01-architecture/runtime/BUSINESS_PIPELINE_ORCHESTRATION.md
doc/01-architecture/runtime/PIPELINE_RUNTIME.md
doc/01-architecture/runtime/RUNTIME_OBSERVABILITY.md

doc/03-infrastructure/storage/
```

---

# 80. Completion Criteria

This specification is synchronized when:

* Preferences publishes only persistent preference facts;
* SessionCreated/SessionClosed consumed-event dependency is removed;
* Import/Migration use explicit commands/lifecycle contracts;
* global `EffectivePreferencesChanged` is removed;
* resolution remains contextual and query-based;
* SessionOverride ownership remains Reading Session;
* restart/cache actions are absent from event payloads;
* semantic impact tags remain descriptive only;
* operation validation failures remain command/error outcomes;
* export is not treated as persistent state mutation event;
* event publication occurs after commit;
* delivery guarantees defer to canonical Event Bus;
* scoped revisions are handled correctly;
* sensitive values remain protected.

---

# 81. Summary

Preferences v2 event flow is:

```text
Persistent Preference Command
        ↓
Preferences
        ↓
Validation
        ↓
Atomic Commit
        ↓
PreferenceRevision
        ↓
Preferences-owned Fact
        ↓
Application / Business Pipeline / Other Consumers
```

Effective resolution remains separate:

```text
Persistent Preferences
+
Source context
+
SessionOverride
        ↓
ResolveEffectivePreferences
        ↓
EffectivePreferencesSnapshot
```

The central event rule is:

```text
Preferences events say
what persistent preference state changed.

They do not say
which session changed
or what processing must restart.
```
