# Preferences States

> **Project:** CRAI
> **Module:** `preferences`
> **Path:** `doc/02-modules/preferences/STATES.md`
> **Version:** 2.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-09

---

# 1. Purpose

This document defines the Preferences-owned state model.

It specifies:

```text
Preferences module lifecycle
persistent preference mutation phases
schema migration phases
import phases
PreferenceRevision behavior
candidate preference state
recovery
state invariants
concurrency
shutdown semantics
```

This document does not define:

```text
Reading Session state
SessionOverride lifecycle
RuntimeRevision lifecycle
WorkItem lifecycle
Attempt lifecycle
pipeline restart state
provider health state
Artifact cache state
EffectivePreferencesSnapshot lifecycle
```

---

# 2. State Ownership

Preferences owns:

```text
PreferencesModuleState
PreferenceMutationPhase
PreferenceMigrationPhase
PreferenceImportPhase
PreferenceRevision
CandidatePreferenceState
```

Reading Session owns:

```text
SessionConfiguration
SessionOverride lifecycle
```

Runtime owns:

```text
RuntimeRevision
WorkItem
Attempt
execution cancellation
retry
```

---

# 3. State Model Overview

Preferences v2 separates:

```text
Preferences
├── Module Lifecycle
├── Persistent Mutation Phase
├── Migration Phase
└── Import Phase
```

`EffectivePreferencesSnapshot` is not a module state machine.

Preference resolution is a deterministic read operation over committed persistent state plus optional external SessionOverride input.

---

# 4. Why Effective Preferences Are Not State

The previous model assumed:

```text
READY
    ⇒
one valid EffectivePreferences exists
```

That is no longer correct.

In v2:

```text
EffectivePreferencesSnapshot
=
Default
+
Global
+
Source
+
optional SessionOverride
```

Different contexts may resolve to different snapshots simultaneously.

Therefore Preferences stores authoritative persistent preference state, not one global effective runtime configuration.

---

# 5. PreferencesModuleState

Primary lifecycle:

```text
UNINITIALIZED
LOADING
READY
DEGRADED
STOPPING
STOPPED
```

---

# 6. `UNINITIALIZED`

## Meaning

Preferences has not loaded schema and persisted preference state.

Characteristics:

```text
no authoritative persistent state exposed
no mutation accepted
resolution unavailable
```

## Allowed Next States

```text
LOADING
STOPPED
```

---

# 7. `LOADING`

## Meaning

Preferences is loading and validating:

```text
PreferenceSchema
Default definitions
Global persistent state
Source profiles
stored schema version
```

Possible work:

```text
read persistence
validate stored data
prepare migration if required
build Candidate committed state
```

## Allowed Next States

```text
READY
DEGRADED
STOPPING
```

---

# 8. Loading Success

Successful initialization follows:

```text
UNINITIALIZED
    ↓
LOADING
    ↓
validate loaded state
    ↓
commit authoritative persistent state
    ↓
READY
```

No `EffectivePreferencesSnapshot` must be precomputed for every possible context.

---

# 9. `READY`

## Meaning

Preferences-owned persistent state is valid and available.

While READY:

* queries are allowed;
* effective resolution is allowed;
* persistent updates are allowed;
* imports may be requested;
* migrations may be initiated where supported;
* exports may read committed snapshots.

## Invariants

```text
PreferenceSchema valid
committed persistent PreferenceSets valid
current PreferenceRevision values consistent
no partially committed mutation exposed
```

---

# 10. `DEGRADED`

## Meaning

Preferences remains partially usable, but some Preferences-owned capability is impaired.

Examples:

```text
persistence temporarily unavailable for writes
one Source profile failed recovery
event publication infrastructure unavailable
non-critical migration metadata issue
```

Possible behavior:

* committed read state may remain available;
* mutation may be restricted;
* resolution may continue from known-good state;
* diagnostics must expose degraded reason.

## Allowed Next States

```text
READY
STOPPING
```

---

# 11. When `DEGRADED` Must Not Be Used

Do not enter `DEGRADED` merely because:

```text
one SetPreference command is invalid
one PreferenceRevision conflict occurs
one import document is invalid
one export fails
```

Those are operation outcomes, not module-health transitions.

---

# 12. `STOPPING`

## Meaning

Preferences is shutting down.

Actions may include:

```text
reject new mutations
allow or terminate safe read operations according to policy
flush Preferences-owned persistence coordination if required
release internal resources
```

## Allowed Next State

```text
STOPPED
```

---

# 13. `STOPPED`

## Meaning

Preferences module lifecycle is terminal.

No further mutation or resolution is accepted.

`STOPPED → READY` is invalid.

A new application lifecycle creates/reinitializes a new module instance.

---

# 14. Module Lifecycle Diagram

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

---

# 15. Removed Global `FAILED`

The previous model had:

```text
FAILED
```

as a catch-all state for internal failures.

v2 removes this as the normal global error target.

Reason:

a failed command or import usually does not invalidate already committed Preferences state.

Use:

```text
operation rejection/failure
```

for scoped failures.

Use:

```text
DEGRADED
```

when known-good state remains readable but capability is impaired.

Use controlled shutdown only when authoritative state cannot be trusted safely.

---

# 16. Catastrophic Invariant Failure

If Preferences detects corruption such that committed state cannot be trusted:

```text
Preference invariant violation
        ↓
fail closed
        ↓
reject new mutation/resolution as required
        ↓
DEGRADED or STOPPING
```

Exact recovery belongs to `ERRORS.md`.

A generic `FAILED` lifecycle is not required.

---

# 17. Preference Mutation Phase

Persistent mutation is modeled as a scoped operation phase.

Recommended phases:

```text
VALIDATING
BUILDING_CANDIDATE
COMMITTING
FINISHED
```

Possible outcomes:

```text
COMMITTED
NO_OP
REJECTED
FAILED
CONFLICT
```

These are not module lifecycle states.

---

# 18. Mutation `VALIDATING`

Preferences validates:

```text
PreferenceKey
value
scope
scopeIdentity
expected PreferenceRevision
cross-field rules
schema compatibility
security/privacy constraints
```

No committed state changes occur.

---

# 19. Mutation `BUILDING_CANDIDATE`

A Candidate PreferenceSet is prepared.

Conceptually:

```text
Current PreferenceSet N
        +
requested mutation
        ↓
Candidate PreferenceSet N+1
```

Candidate remains private.

---

# 20. Mutation `COMMITTING`

Preferences atomically commits:

```text
PreferenceSet
+
PreferenceRevision
+
owned indexes/metadata
```

Consumers must not observe partial persistent state.

---

# 21. Mutation `COMMITTED`

Successful semantic change:

```text
PreferenceRevision N
        ↓
N+1
```

A corresponding Preferences-owned fact may then be published.

---

# 22. Mutation `NO_OP`

Request is valid but produces no semantic change.

Examples:

```text
set same value
remove absent explicit value
reset already-empty category
```

Rules:

```text
PreferenceRevision unchanged
no mutation event required
```

---

# 23. Mutation `REJECTED`

Request violates domain rules.

Examples:

```text
unknown key
invalid type
unsupported scope
invalid range
security-policy violation
```

Current persistent state remains unchanged.

---

# 24. Mutation `CONFLICT`

Expected PreferenceRevision does not match current revision.

Example:

```text
expected = 20
current = 21
```

Outcome:

```text
PreferenceRevisionConflict
```

No mutation occurs.

---

# 25. Mutation `FAILED`

A technical/internal Preferences-owned operation failed before a safe commit.

If previous state is known intact:

```text
current committed state remains authoritative
```

The module normally remains READY.

---

# 26. Candidate Preference State

Conceptually:

```text
CandidatePreferenceState
├── basedOnRevision
├── candidateRevision
├── scope
├── scopeIdentity?
├── values
└── changeSet
```

Candidate is never externally authoritative before commit.

---

# 27. Candidate Isolation

During mutation:

```text
Committed PreferenceSet N
+
Candidate PreferenceSet N+1
```

may coexist.

Queries continue reading:

```text
Committed PreferenceSet N
```

until commit completes.

---

# 28. Atomic Commit

Commit must update logically together:

```text
persistent PreferenceSet
PreferenceRevision
scope indexes
schema metadata where applicable
```

Partial visibility is forbidden.

---

# 29. PreferenceRevision Model

`PreferenceRevision` is not a lifecycle state machine.

It is an immutable committed version identifier.

Example:

```text
Revision 30
    ↓
successful semantic mutation
    ↓
Revision 31
```

Revision 30 remains immutable if retained.

---

# 30. Revision Creation Rule

A new revision is created only when persistent Preferences-owned semantic state changes.

No revision for:

```text
resolution
export
invalid mutation
no-op
failed Candidate
SessionOverride change
```

SessionOverride version is externally owned.

---

# 31. Resolution State

Effective resolution is read-only and does not transition module lifecycle.

Conceptually:

```text
READY
    ↓
ResolveEffectivePreferences
    ↓
read committed persistent state
    +
external SessionOverride input
    ↓
validate resolution input
    ↓
EffectivePreferencesSnapshot
```

Module remains:

```text
READY
```

---

# 32. Concurrent Resolution

Multiple effective resolutions may occur concurrently.

Example:

```text
Session A
Source X
SessionOverride A

Session B
Source Y
SessionOverride B
```

Both may resolve different immutable snapshots from the same committed persistent state.

---

# 33. Resolution During Mutation

Recommended semantics:

```text
mutation Candidate in progress
        ↓
resolution reads last committed PreferenceSet
```

A resolver must not observe Candidate state.

After commit, new resolutions may use the new PreferenceRevision.

---

# 34. Existing Effective Snapshot

If Preferences changes after a snapshot is resolved:

```text
EffectivePreferencesSnapshot E1
```

remains immutable.

It does not silently become E2.

---

# 35. SessionOverride Change

SessionOverride changes do not change PreferencesModuleState.

They also do not advance PreferenceRevision.

They may cause a later:

```text
ResolveEffectivePreferences
```

to produce a different snapshot.

---

# 36. Migration Phase

Schema migration is modeled separately.

Recommended phases:

```text
VALIDATING_SOURCE_SCHEMA
BUILDING_MIGRATED_CANDIDATE
VALIDATING_TARGET_SCHEMA
COMMITTING_MIGRATION
FINISHED
```

Possible outcomes:

```text
MIGRATED
REJECTED
FAILED
```

---

# 37. Migration Start

Migration may begin when:

```text
stored schema version
<
current supported schema version
```

or through an explicit maintenance/application lifecycle command.

---

# 38. Migration Candidate

Migration builds a full Candidate state.

Partial migration is forbidden.

```text
Current persisted state
        ↓
migration rules
        ↓
Candidate migrated state
        ↓
target-schema validation
```

---

# 39. Migration Commit

Only after full validation:

```text
Candidate migrated state
        ↓
atomic commit
        ↓
new schema version
+
new PreferenceRevision provenance as defined
```

Exact revision strategy belongs to `CONTRACT.md`.

---

# 40. Migration Failure

If migration fails before commit:

```text
previous known-good state remains unchanged
```

If previous schema is still readable under compatibility policy, module may remain or become READY/DEGRADED accordingly.

Migration failure does not automatically require terminal shutdown.

---

# 41. Import Phase

Import mutates Preferences-owned persistent state and therefore has its own operation phase.

Recommended:

```text
PARSING
VALIDATING
MIGRATING_IF_REQUIRED
BUILDING_CANDIDATE
COMMITTING
FINISHED
```

Possible outcomes:

```text
IMPORTED
REJECTED
FAILED
CONFLICT
```

---

# 42. Why Import and Export Are Separate

Import:

```text
may mutate persistent Preferences state
```

Export:

```text
reads committed state only
```

Therefore they must not share one generic:

```text
EXPORTING
```

module state.

---

# 43. Import Atomicity

Import must be all-or-nothing.

Invalid:

```text
apply valid keys
skip invalid keys
commit partial import
```

unless a future explicitly named best-effort import mode is added.

Default:

```text
full validation
    ↓
atomic commit
```

---

# 44. Import and PreferenceRevision

Successful import that changes persistent semantic state advances applicable PreferenceRevision values.

Import no-op does not create unnecessary revisions.

---

# 45. Export Is a Read Operation

Export does not require a module lifecycle transition.

Typical:

```text
READY
    ↓
ExportPreferences
    ↓
read immutable committed snapshot
    ↓
filter sensitive values
    ↓
serialize
```

Module remains READY.

---

# 46. Export Failure

Export serialization or destination failure:

```text
does not mutate Preferences state
```

and does not move module to DEGRADED unless the failure reveals broader Preferences-owned corruption.

---

# 47. Reset Operations

Reset operations use the normal mutation phase.

Examples:

```text
ResetCategory
ResetScope
RemovePreference
```

Session override reset does not enter Preferences state machine because it belongs to Reading Session.

---

# 48. Source Profile Creation

Creating a SourcePreferenceProfile uses the same persistent mutation semantics:

```text
validate source identity
    ↓
Candidate profile
    ↓
commit
    ↓
SourcePreferenceProfileCreated
```

---

# 49. Source Profile Deletion

Deletion is atomic.

After commit:

* profile no longer resolves;
* historical snapshots remain immutable;
* currently running Runtime Attempts remain unaffected.

---

# 50. Persistent Update Concurrency

Mutation for the same revision domain is logically serialized.

Example:

```text
Current revision = 8

Command A expects 8
Command B expects 8

B commits 9

A reaches commit
    ↓
CONFLICT
```

No automatic merge required for MVP.

---

# 51. Global and Source Concurrency

If revisions are scoped:

```text
GlobalPreferenceRevision
SourcePreferenceRevision
```

independent scopes may mutate concurrently when implementation can preserve atomicity.

The contract must still produce deterministic resolution provenance.

---

# 52. Preference Definitions

Preference definitions are immutable at runtime for one application/schema version.

They do not transition through mutation states during normal execution.

Schema migration may replace the active definition set as one atomic schema transition.

---

# 53. Storage Failure During Mutation

If Storage persistence fails before commit is known successful:

```text
mutation FAILED
previous committed state retained
```

If commit outcome is uncertain:

```text
Preferences may enter DEGRADED
reconciliation/recovery required
```

Detailed classification belongs to `ERRORS.md`.

---

# 54. Event Publication Failure

Event publication occurs after commit.

Example:

```text
PreferenceRevision 20 → 21 committed
        ↓
PreferenceChanged publication fails
```

State remains:

```text
Revision 21
```

Module may remain READY while infrastructure retries publication.

Event publication failure alone does not revert committed preference state.

---

# 55. Event Publication and DEGRADED

Repeated/critical publication infrastructure failure may produce:

```text
DEGRADED
```

only if architecture requires event delivery for system-level operability.

The underlying preference state remains committed.

---

# 56. Reading Session Independence

Reading Session lifecycle does not transition Preferences state.

The following do not change PreferencesModuleState directly:

```text
ReadingSessionCreated
ReadingSessionPaused
ReadingSessionCompleted
ReadingSessionDisposed
```

---

# 57. No Session Override Cleanup State

Preferences does not maintain:

```text
SessionPreferenceProfile
```

and therefore does not need states/events for:

```text
create session overrides
delete session overrides
expire session profile
```

That belongs to Reading Session.

---

# 58. Runtime Independence

The following do not change Preferences state directly:

```text
Runtime Attempt failure
Runtime timeout
Runtime cancellation
Runtime supersession
Retry exhaustion
```

Preferences state and Runtime state are independent.

---

# 59. Processing Failure Independence

These do not transition Preferences module:

```text
Capture failure
OCR failure
Translation failure
Presentation failure
UI apply failure
```

---

# 60. Preference Impact Does Not Create State

A changed preference may carry:

```text
semanticImpactTags
```

but Preferences does not transition into:

```text
RestartingStage
RestartingPipeline
RefreshingPresentation
```

Those states do not belong here.

---

# 61. Shutdown

Shutdown behavior:

```text
READY / DEGRADED
        ↓
STOPPING
        ↓
STOPPED
```

During STOPPING:

* new mutations rejected;
* new imports rejected;
* new migrations rejected;
* safe read completion policy is implementation-defined;
* resources released deterministically.

---

# 62. Initialization Recovery

If initial load fails but a known-good fallback is available:

```text
LOADING
    ↓
DEGRADED
```

Example:

```text
persistent Source profiles unavailable
but defaults/global fallback valid
```

If no trustworthy state can be established:

```text
LOADING
    ↓
STOPPING
or
remain unavailable according to application startup policy
```

---

# 63. Recovery to READY

Recovery may include:

```text
Storage becomes available
known-good persisted state reloaded
corrupt Source profile isolated
event infrastructure restored
```

After validation:

```text
DEGRADED → READY
```

---

# 64. State Transition Table — Module Lifecycle

| Current         | Trigger                     | Next       |
| --------------- | --------------------------- | ---------- |
| `UNINITIALIZED` | Initialize                  | `LOADING`  |
| `LOADING`       | Valid state committed       | `READY`    |
| `LOADING`       | Partial known-good recovery | `DEGRADED` |
| `READY`         | Operational degradation     | `DEGRADED` |
| `DEGRADED`      | Recovery validated          | `READY`    |
| `READY`         | Shutdown                    | `STOPPING` |
| `DEGRADED`      | Shutdown                    | `STOPPING` |
| `LOADING`       | Shutdown                    | `STOPPING` |
| `STOPPING`      | Cleanup complete            | `STOPPED`  |

---

# 65. State Transition Table — Mutation

| Phase                | Outcome      | Result               |
| -------------------- | ------------ | -------------------- |
| `VALIDATING`         | Invalid      | `REJECTED`           |
| `VALIDATING`         | Conflict     | `CONFLICT`           |
| `VALIDATING`         | Valid        | `BUILDING_CANDIDATE` |
| `BUILDING_CANDIDATE` | Equivalent   | `NO_OP`              |
| `BUILDING_CANDIDATE` | Invalid      | `REJECTED`           |
| `BUILDING_CANDIDATE` | Valid        | `COMMITTING`         |
| `COMMITTING`         | Success      | `COMMITTED`          |
| `COMMITTING`         | Safe failure | `FAILED`             |

---

# 66. State Transition Table — Import

| Phase                   | Outcome            | Next/Result             |
| ----------------------- | ------------------ | ----------------------- |
| `PARSING`               | Parse fail         | `REJECTED`              |
| `PARSING`               | Success            | `VALIDATING`            |
| `VALIDATING`            | Invalid            | `REJECTED`              |
| `VALIDATING`            | Migration required | `MIGRATING_IF_REQUIRED` |
| `VALIDATING`            | Valid              | `BUILDING_CANDIDATE`    |
| `MIGRATING_IF_REQUIRED` | Fail               | `REJECTED` / `FAILED`   |
| `MIGRATING_IF_REQUIRED` | Success            | `BUILDING_CANDIDATE`    |
| `BUILDING_CANDIDATE`    | Success            | `COMMITTING`            |
| `COMMITTING`            | Success            | `IMPORTED`              |
| `COMMITTING`            | Safe failure       | `FAILED`                |

---

# 67. State Transition Table — Migration

| Phase                         | Outcome      | Next/Result                   |
| ----------------------------- | ------------ | ----------------------------- |
| `VALIDATING_SOURCE_SCHEMA`    | Invalid      | `REJECTED`                    |
| `VALIDATING_SOURCE_SCHEMA`    | Valid        | `BUILDING_MIGRATED_CANDIDATE` |
| `BUILDING_MIGRATED_CANDIDATE` | Fail         | `FAILED`                      |
| `BUILDING_MIGRATED_CANDIDATE` | Success      | `VALIDATING_TARGET_SCHEMA`    |
| `VALIDATING_TARGET_SCHEMA`    | Invalid      | `FAILED`                      |
| `VALIDATING_TARGET_SCHEMA`    | Valid        | `COMMITTING_MIGRATION`        |
| `COMMITTING_MIGRATION`        | Success      | `MIGRATED`                    |
| `COMMITTING_MIGRATION`        | Safe failure | `FAILED`                      |

---

# 68. State/Event Relationship

Preferences facts publish only after committed state.

Typical:

| Commit                  | Possible Event                   |
| ----------------------- | -------------------------------- |
| Set/Update              | `PreferenceChanged`              |
| Remove                  | `PreferenceRemoved`              |
| Reset                   | `PreferenceReset`                |
| Source profile creation | `SourcePreferenceProfileCreated` |
| Source profile deletion | `SourcePreferenceProfileDeleted` |
| Successful import       | `PreferenceImportCompleted`      |
| Successful migration    | `PreferenceMigrationCompleted`   |

Resolution does not require an event by default.

---

# 69. No `EffectivePreferencesChanged` State Transition

Resolving a new snapshot:

```text
ResolveEffectivePreferences
        ↓
EffectivePreferencesSnapshot E2
```

does not mutate Preferences module state.

No state transition is required.

---

# 70. Effective Snapshot Lifetime

An EffectivePreferencesSnapshot remains immutable even if:

```text
persistent preference changes
Source profile changes
SessionOverride changes
```

New contexts resolve a new snapshot.

Old Runtime Attempts may continue using old derived ConfigurationSnapshot.

---

# 71. Architecture Invariants — Lifecycle

1. Preferences has exactly one module lifecycle state.

2. READY means persistent Preferences-owned state is valid.

3. READY does not imply one global EffectivePreferences exists.

4. DEGRADED means known-good state may still be usable.

5. Invalid commands do not move module out of READY.

6. Revision conflicts do not move module out of READY.

7. Invalid imports do not move module out of READY.

8. STOPPED is terminal.

---

# 72. Architecture Invariants — Mutation

1. Candidate preference state is never externally authoritative.

2. Previous committed state remains visible until commit.

3. Mutation is atomic.

4. Failed validation does not mutate state.

5. Failed Candidate does not mutate state.

6. No-op does not advance revision.

7. Successful semantic mutation advances revision.

8. Resolution does not advance revision.

9. SessionOverride changes do not advance Preferences revision.

---

# 73. Architecture Invariants — Ownership

1. Preferences owns persistent preference lifecycle.

2. Reading Session owns session override lifecycle.

3. Preferences has no SessionPreferenceProfile lifecycle.

4. Runtime state does not mutate Preferences state directly.

5. Processing failure does not mutate Preferences state.

6. Semantic impact metadata does not create restart states.

7. Export is not a persistent mutation state.

8. Import is a persistent mutation operation.

9. EffectivePreferencesSnapshot is not a module state.

10. ConfigurationSnapshot is external.

---

# 74. Architecture Invariants — Migration

1. Migration is all-or-nothing.

2. Migrated Candidate must pass target-schema validation.

3. Partial migration is never exposed.

4. Migration failure preserves known-good state where possible.

5. Migration does not silently expose invalid Preferences.

---

# 75. Testing — Module Lifecycle

Test:

```text
UNINITIALIZED → LOADING
LOADING → READY
LOADING → DEGRADED
READY ↔ DEGRADED
READY → STOPPING → STOPPED
DEGRADED → STOPPING → STOPPED
```

---

# 76. Testing — Mutation

Verify:

```text
valid update → COMMITTED
same value → NO_OP
invalid value → REJECTED
revision mismatch → CONFLICT
safe commit failure → FAILED
```

and committed state remains correct.

---

# 77. Testing — Resolution

Verify resolution while:

```text
READY
mutation Candidate exists
multiple sessions resolve concurrently
```

Resolution must use only committed Preferences state.

---

# 78. Testing — Session Override Independence

Verify:

```text
SessionOverride changes
```

do not:

* create PreferenceRevision;
* change PreferencesModuleState;
* persist session data;
* require SessionCreated/SessionClosed handling.

---

# 79. Testing — Import

Test:

* valid import;
* invalid import;
* schema migration during import;
* atomic failure;
* no partial application;
* revision advancement;
* sensitive value rejection.

---

# 80. Testing — Export

Verify:

* export does not change module state;
* export does not advance revision;
* export during concurrent read is safe;
* sensitive values excluded;
* serialization failure leaves Preferences unchanged.

---

# 81. Testing — Migration

Test:

* successful migration;
* failed source-schema validation;
* failed transformed Candidate;
* failed target-schema validation;
* safe commit failure;
* preservation of known-good state.

---

# 82. Testing — Runtime Independence

Verify these do not change Preferences state:

```text
Runtime Attempt failure
Runtime cancellation
Runtime timeout
Pipeline supersession
Capture failure
Recognition failure
Translation failure
Presentation failure
```

---

# 83. Testing — Event Publication Failure

Verify:

```text
PreferenceRevision commits
event publication fails
```

does not revert committed persistent Preferences state.

---

# 84. Testing — Degraded Recovery

Verify known-good state remains readable when applicable and:

```text
DEGRADED → READY
```

occurs only after validated recovery.

---

# 85. Removed v1 Concepts

Removed/reworked:

```text
Failed global catch-all
Exporting state
EffectivePreferences as one active global state
SessionOverride lifecycle inside Preferences
SessionCreated / SessionClosed state inputs
Restart impact states/actions
```

Replacement:

```text
DEGRADED
read-only EffectivePreferences resolution
external SessionOverride input
semantic impact metadata
scoped Import/Migration/Mutation phases
```

---

# 86. Related Documents

```text
doc/02-modules/preferences/MODULE.md
doc/02-modules/preferences/CONTRACT.md
doc/02-modules/preferences/EVENTS.md
doc/02-modules/preferences/ERRORS.md
doc/02-modules/preferences/README.md

doc/02-modules/reading-session/MODULE.md
doc/02-modules/reading-session/CONTRACT.md

doc/01-architecture/core/STATE_MACHINE.md
doc/01-architecture/core/EVENT_BUS.md
doc/01-architecture/core/EVENT_CONVENTION.md

doc/01-architecture/modules/OWNERSHIP_MAP.md
doc/01-architecture/modules/MODULE_DEPENDENCY.md

doc/01-architecture/runtime/BUSINESS_PIPELINE_ORCHESTRATION.md
doc/01-architecture/runtime/PIPELINE_RUNTIME.md
doc/01-architecture/runtime/RETRY_POLICY.md

doc/03-infrastructure/storage/
```

---

# 87. Completion Criteria

This specification is synchronized when:

* READY no longer implies one global EffectivePreferences object;
* EffectivePreferencesSnapshot is treated as read-only resolved output;
* SessionOverride lifecycle is external;
* Preferences no longer consumes SessionCreated/SessionClosed for state ownership;
* export is removed from module lifecycle;
* import is modeled as a mutation operation;
* global FAILED is removed as default failure handling;
* DEGRADED represents recoverable module impairment;
* invalid commands preserve READY state;
* Candidate mutation state is isolated;
* PreferenceRevision rules match CONTRACT.md;
* migration remains atomic;
* Runtime/processing failures remain external;
* semantic impact does not create processing restart states.

---

# 88. Summary

Preferences v2 state is divided into:

```text
Module Lifecycle
+
Persistent Mutation Phase
+
Migration Phase
+
Import Phase
```

Module lifecycle:

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

Persistent mutation:

```text
VALIDATING
    ↓
BUILDING_CANDIDATE
    ↓
COMMITTING
    ↓
COMMITTED
```

with:

```text
NO_OP
REJECTED
CONFLICT
FAILED
```

as scoped outcomes.

Effective preference resolution remains:

```text
Committed Persistent State
+
External SessionOverride
        ↓
Resolve
        ↓
Immutable EffectivePreferencesSnapshot
```

and does not change module state.

The central rule is:

```text
Preferences state describes
authoritative persistent configuration.

EffectivePreferencesSnapshot describes
one resolved view.

Reading Session owns
temporary session configuration.

Runtime owns
execution.
```
