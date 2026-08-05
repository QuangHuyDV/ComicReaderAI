# Configuration States

> **Project:** CRAI
>
> **Layer:** Infrastructure
>
> **Module:** Configuration
>
> **Document:** State Specification
>
> **Path:** `03-infrastructure/configuration/STATES.md`
>
> **Status:** Architecture Draft

---

# 1. Purpose

This document defines every state machine owned by the Configuration Infrastructure module.

It specifies:

- lifecycle states
- state transitions
- transition ownership
- terminal states
- transient states
- state invariants
- recovery behavior

This document intentionally excludes:

- commands
- queries
- events
- errors

Those are defined in:

```
CONTRACT.md

EVENTS.md

ERRORS.md
```

---

# 2. State Philosophy

Configuration is entirely state driven.

Every important object owns an explicit lifecycle.

Configuration never relies on hidden boolean flags.

Avoid:

```
loaded = true

validated = true

published = true
```

Prefer

```
LOADING

↓

VALIDATING

↓

PUBLISHED
```

---

# 3. State Machine Inventory

Configuration owns the following state machines.

```
Configuration Source

Configuration Candidate

Configuration Snapshot

Configuration Revision

Configuration Override

Configuration Validation

Configuration Compatibility

Configuration Migration

Consumer Acceptance

Configuration Reload
```

Each machine is independent.

---

# 4. General State Rules

Every state machine must satisfy:

✓ explicit state

✓ explicit transition

✓ deterministic transition

✓ no hidden state

✓ observable transition

✓ immutable history

✓ replay safety

---

# 5. State Categories

Configuration distinguishes:

```
Stable States

Transient States

Terminal States

Failure States
```

Stable states may exist indefinitely.

Transient states are expected to transition.

Terminal states never transition again.

Failure states require recovery or replacement.

---

# 6. Transition Rules

Every transition:

- has exactly one origin state;
- has exactly one destination state;
- has one owner;
- may publish events;
- may fail.

Transitions never occur implicitly.

---

# 7. Transition Ownership

Configuration owns state transitions for:

- configuration sources;
- candidates;
- snapshots;
- overrides;
- validation;
- compatibility;
- migrations.

Consumer modules own only:

```
Consumer Acceptance
```

---

# 8. State Persistence

Published states are persisted according to retention policy.

Transient execution states may remain in memory only.

Persistence policy does not alter lifecycle semantics.

---

# 9. Revision Awareness

Every state transition belongs to exactly one configuration revision.

Transitions never mutate historical revisions.

---

# Part I — Configuration Source State Machine

# 10. Source Lifecycle

A configuration source follows:

```
REGISTERED

↓

ENABLED

↓

LOADING

↓

READY

↓

ENABLED

↓

...

↓

DISABLED

↓

REMOVED
```

Failure transitions may occur during loading.

---

# 11. REGISTERED

Meaning

The source exists.

Properties

- identity assigned;
- metadata available;
- not yet active.

Allowed transitions

```
REGISTERED

↓

ENABLED

REGISTERED

↓

REMOVED
```

---

# 12. ENABLED

Meaning

The source participates in future configuration reloads.

The source has not necessarily been loaded yet.

Allowed transitions

```
ENABLED

↓

LOADING

ENABLED

↓

DISABLED
```

---

# 13. LOADING

Meaning

The source is currently being read.

Possible activities:

- file loading;
- remote retrieval;
- environment scan;
- parsing.

LOADING is transient.

---

# 14. READY

Meaning

The source has been successfully loaded.

Properties

✓ parsed

✓ normalized

✓ available for merge

READY is stable.

---

# 15. FAILED

Meaning

Loading failed.

Possible causes:

- missing file;
- parse failure;
- permission denied;
- network failure.

FAILED does not remove the source.

Recovery is possible.

---

# 16. DISABLED

Meaning

The source remains registered.

It no longer participates in merge.

Historical revisions remain valid.

---

# 17. REMOVED

Meaning

The source no longer exists.

Properties

✓ terminal

No outgoing transitions.

---

# 18. Source State Diagram

```
REGISTERED

↓

ENABLED

↓

LOADING

↙      ↘

READY   FAILED

↓         ↓

ENABLED   ENABLED

↓

DISABLED

↓

REMOVED
```

---

# 19. Source Invariants

A source:

- has one identity;
- one current state;
- deterministic transitions;
- immutable history.

---

# Part II — Configuration Candidate State Machine

# 20. Candidate Lifecycle

A candidate represents unpublished configuration.

Lifecycle

```
CREATED

↓

LOADING

↓

NORMALIZING

↓

MERGING

↓

BINDING

↓

VALIDATING

↓

COMPATIBILITY_CHECK

↓

READY

↓

PUBLISHED
```

Failure may occur from any transient state.

---

# 21. CREATED

The candidate exists.

No processing has begun.

Allowed transition

```
CREATED

↓

LOADING
```

---

# 22. LOADING

Sources are collected.

Input documents are read.

Transient state.

---

# 23. NORMALIZING

All source formats become canonical internal representation.

Examples

```
JSON

↓

Canonical Object

YAML

↓

Canonical Object
```

---

# 24. MERGING

Source precedence is applied.

Winning values are selected.

Origins recorded.

No validation yet.

---

# 25. BINDING

Canonical values become typed configuration sections.

Examples

```
RuntimeConfiguration

TranslationConfiguration

LoggingConfiguration
```

Binding failures terminate processing.

---

# 26. VALIDATING

Structural validation executes.

Includes:

- schema;
- type;
- required field;
- cross-field.

Validation success allows compatibility checking.

---

# 27. COMPATIBILITY_CHECK

Compatibility evaluation begins.

Checks:

- schema versions;
- deprecated fields;
- application compatibility.

No consumer semantics.

---

# 28. READY

Candidate is fully validated.

Eligible for publication.

READY does not imply publication.

---

# 29. PUBLISHED

The candidate becomes:

```
Active Snapshot
```

PUBLISHED is terminal.

Further modifications require a new candidate.

---

# 30. REJECTED

Meaning

Candidate cannot become authoritative.

Reasons

- validation failure;
- compatibility failure;
- administrative rejection.

REJECTED is terminal.

# 31. Candidate Failure States

During candidate construction, failures may occur.

Possible failure states:

```
LOAD_FAILED

NORMALIZATION_FAILED

MERGE_FAILED

BINDING_FAILED

VALIDATION_FAILED

COMPATIBILITY_FAILED
```

Each failure state is terminal.

Recovery requires creation of a new candidate.

---

# 32. LOAD_FAILED

Meaning

At least one required configuration source could not be loaded.

Examples

- required file missing;
- unreadable source;
- remote source unavailable;
- corrupted content.

Possible transitions

```
LOAD_FAILED

↓

DISCARDED
```

or

```
LOAD_FAILED

↓

CREATED
```

through a completely new reload.

---

# 33. NORMALIZATION_FAILED

Meaning

Configuration values cannot be converted into the canonical internal representation.

Examples

```
Invalid Duration

Unknown Primitive

Unsupported Encoding
```

Normalization failures prevent merge.

---

# 34. MERGE_FAILED

Meaning

Configuration merge could not complete.

Typical causes

- conflicting merge strategy;
- invalid inheritance;
- cyclic references;
- unsupported merge policy.

Merge failures terminate the candidate.

---

# 35. BINDING_FAILED

Meaning

Typed configuration objects cannot be created.

Examples

```
RuntimeConfiguration

↓

Binding Failure
```

Binding failures usually indicate schema mismatch.

---

# 36. VALIDATION_FAILED

Meaning

Structural validation failed.

Examples

- required field missing;
- invalid enum;
- duplicate keys;
- incompatible object shape.

Publication is impossible.

---

# 37. COMPATIBILITY_FAILED

Meaning

The configuration is structurally valid but cannot be used.

Examples

- unsupported schema version;
- incompatible application version;
- unsupported module version.

Candidate becomes terminal.

---

# 38. DISCARDED

Meaning

The candidate has been permanently abandoned.

Properties

✓ terminal

No further transitions.

Historical diagnostics remain available.

---

# 39. Candidate State Diagram

```
CREATED

↓

LOADING

↓

NORMALIZING

↓

MERGING

↓

BINDING

↓

VALIDATING

↓

COMPATIBILITY_CHECK

↓

READY

↓

PUBLISHED


Failures

↓

LOAD_FAILED

NORMALIZATION_FAILED

MERGE_FAILED

BINDING_FAILED

VALIDATION_FAILED

COMPATIBILITY_FAILED

↓

DISCARDED
```

---

# 40. Candidate Invariants

A candidate guarantees:

✓ exactly one lifecycle

✓ exactly one terminal state

✓ immutable processing history

✓ publication at most once

---

# Part III — Configuration Snapshot State Machine

# 41. Snapshot Lifecycle

Snapshots are immutable.

Lifecycle

```
CREATED

↓

PUBLISHED

↓

ACTIVE

↓

RETAINED

↓

EXPIRED
```

Snapshots never return to ACTIVE once replaced.

---

# 42. CREATED

Meaning

The snapshot object has been constructed.

The snapshot is not yet authoritative.

Allowed transition

```
CREATED

↓

PUBLISHED
```

---

# 43. PUBLISHED

Meaning

The snapshot has become authoritative.

Publication creates:

```
Configuration Revision
```

PUBLISHED is transient.

Immediately afterwards:

```
ACTIVE
```

---

# 44. ACTIVE

Meaning

This is the configuration currently used by the application.

Properties

✓ authoritative

✓ immutable

✓ queryable

Only one snapshot may be ACTIVE.

---

# 45. RETAINED

Meaning

The snapshot is historical.

Properties

✓ immutable

✓ queryable

✓ non-authoritative

Historical revisions remain available according to retention policy.

---

# 46. EXPIRED

Meaning

Historical retention has ended.

The snapshot is no longer queryable.

Properties

✓ terminal

Metadata may still exist.

---

# 47. Snapshot Transition Rules

```
CREATED

↓

PUBLISHED

↓

ACTIVE

↓

RETAINED

↓

EXPIRED
```

Backward transitions are forbidden.

---

# 48. Snapshot Replacement

When a new snapshot is published:

```
Snapshot R18

↓

ACTIVE

↓

Publish R19

↓

Snapshot R18

↓

RETAINED

Snapshot R19

↓

ACTIVE
```

There is never more than one ACTIVE snapshot.

---

# 49. Snapshot Recovery

Snapshots never recover from EXPIRED.

A retained snapshot may be used as rollback input.

The rollback creates:

```
New Snapshot
```

not reactivation.

---

# 50. Snapshot Invariants

Snapshots guarantee:

✓ immutable contents

✓ immutable revision

✓ single ACTIVE snapshot

✓ append-only history

---

# Part IV — Configuration Revision State Machine

# 51. Revision Lifecycle

Configuration revisions follow:

```
ALLOCATED

↓

ACTIVE

↓

HISTORICAL

↓

EXPIRED
```

---

# 52. ALLOCATED

Meaning

The revision number has been reserved.

The snapshot has not yet become authoritative.

ALLOCATED is transient.

---

# 53. ACTIVE

Meaning

The revision is currently authoritative.

Exactly one revision is ACTIVE.

---

# 54. HISTORICAL

Meaning

A newer revision has replaced it.

Properties

✓ immutable

✓ queryable

✓ rollback source

---

# 55. EXPIRED

Meaning

Historical retention ended.

The revision is no longer available through normal history queries.

---

# 56. Revision Diagram

```
ALLOCATED

↓

ACTIVE

↓

HISTORICAL

↓

EXPIRED
```

---

# 57. Revision Invariants

A revision:

- is allocated once;
- becomes active at most once;
- never returns to ACTIVE;
- never changes identity.

---

# Part V — Configuration Override State Machine

# 58. Override Lifecycle

Overrides follow:

```
CREATED

↓

VALIDATED

↓

ACTIVE

↓

EXPIRED

or

REMOVED
```

---

# 59. CREATED

The override exists.

Validation has not completed.

---

# 60. VALIDATED

Meaning

The override passed:

- schema validation;
- compatibility evaluation;
- policy checks.

Only validated overrides may become ACTIVE.

---

# 61. ACTIVE

Meaning

The override participates in effective configuration resolution.

Properties

✓ highest precedence

✓ visible in diagnostics

✓ revision aware

✓ auditable

ACTIVE is stable.

---

# 62. EXPIRED

Meaning

The override lifetime has elapsed.

Properties

- no longer participates in merge;
- remains available for audit;
- cannot become ACTIVE again.

EXPIRED is terminal.

---

# 63. REMOVED

Meaning

The override was explicitly removed.

Examples

- administrator removed it;
- module cleanup;
- policy enforcement.

REMOVED is terminal.

---

# 64. Override State Diagram

```
CREATED

↓

VALIDATED

↓

ACTIVE

↙        ↘

EXPIRED   REMOVED
```

---

# 65. Override State Rules

An override:

- becomes ACTIVE at most once;
- never leaves ACTIVE except through expiration or removal;
- never returns to ACTIVE.

---

# 66. Override Invariants

Every override guarantees:

✓ explicit owner

✓ explicit lifetime

✓ deterministic activation

✓ immutable history

---

# Part VI — Configuration Validation State Machine

# 67. Validation Lifecycle

Validation follows:

```
CREATED

↓

RUNNING

↓

VALID

or

WARNING

or

INVALID
```

---

# 68. CREATED

Meaning

Validation request exists.

Execution has not started.

Allowed transition

```
CREATED

↓

RUNNING
```

---

# 69. RUNNING

Meaning

Validation engine is executing.

Possible activities:

- schema validation;
- type validation;
- cross-field validation;
- cross-section validation.

RUNNING is transient.

---

# 70. VALID

Meaning

Validation completed successfully.

No blocking violations exist.

Publication is allowed.

---

# 71. WARNING

Meaning

Validation completed.

Warnings exist.

Publication remains allowed.

Warnings should remain observable.

---

# 72. INVALID

Meaning

Blocking validation failures exist.

Publication is forbidden.

INVALID is terminal.

---

# 73. Validation Diagram

```
CREATED

↓

RUNNING

↙     ↓      ↘

VALID WARNING INVALID
```

---

# 74. Validation Recovery

Recovery never mutates the failed validation.

A new validation execution must be started.

---

# 75. Validation Invariants

Validation guarantees:

✓ immutable result

✓ deterministic outcome

✓ one terminal state

✓ replay safety

---

# Part VII — Compatibility State Machine

# 76. Compatibility Lifecycle

```
CREATED

↓

RUNNING

↓

COMPATIBLE

COMPATIBLE_WITH_WARNINGS

MIGRATION_REQUIRED

INCOMPATIBLE
```

---

# 77. CREATED

Compatibility evaluation exists.

Execution has not started.

---

# 78. RUNNING

Compatibility engine evaluates:

- schema versions;
- supported application version;
- deprecated fields;
- compatibility policy.

Transient.

---

# 79. COMPATIBLE

Configuration may proceed.

No compatibility concerns exist.

---

# 80. COMPATIBLE_WITH_WARNINGS

Configuration is usable.

Warnings should be reported.

Publication remains allowed.

---

# 81. MIGRATION_REQUIRED

Configuration cannot be published directly.

Migration must occur first.

---

# 82. INCOMPATIBLE

Configuration cannot be used.

Publication is rejected.

Terminal state.

---

# 83. Compatibility Diagram

```
CREATED

↓

RUNNING

↙       ↓          ↓             ↘

COMPATIBLE

COMPATIBLE_WITH_WARNINGS

MIGRATION_REQUIRED

INCOMPATIBLE
```

---

# 84. Compatibility Recovery

Recovery paths include:

```
Migration

↓

Revalidation

↓

Compatibility
```

Recovery never mutates the original evaluation.

---

# 85. Compatibility Invariants

Compatibility guarantees:

✓ explicit outcome

✓ immutable result

✓ deterministic evaluation

---

# Part VIII — Configuration Migration State Machine

# 86. Migration Lifecycle

```
CREATED

↓

READY

↓

RUNNING

↓

COMPLETED

or

FAILED

or

CANCELLED
```

---

# 87. CREATED

Migration request exists.

Resources not yet allocated.

---

# 88. READY

Migration prerequisites satisfied.

Execution may begin.

---

# 89. RUNNING

Migration engine transforms configuration.

Activities may include:

- field rename;
- schema conversion;
- default insertion;
- deprecated field handling.

---

# 90. COMPLETED

Migration successfully produced a candidate configuration.

Publication has not yet occurred.

---

# 91. FAILED

Migration terminated unsuccessfully.

Candidate configuration is discarded.

FAILED is terminal.

---

# 92. CANCELLED

Migration intentionally stopped.

Possible causes:

- administrative cancellation;
- shutdown;
- newer migration request.

Terminal state.

---

# 93. Migration Diagram

```
CREATED

↓

READY

↓

RUNNING

↙        ↓         ↘

FAILED COMPLETED CANCELLED
```

---

# 94. Migration Recovery

Recovery always starts a new migration.

Previously failed migrations remain immutable.

---

# 95. Migration Invariants

Migration guarantees:

✓ immutable source

✓ deterministic execution

✓ explicit terminal state

✓ replay safety

---

# Part IX — Consumer Acceptance State Machine

# 96. Consumer Acceptance Lifecycle

```
PENDING

↓

ACCEPTED

↓

SUPERSEDED
```

Alternative paths:

```
PENDING

↓

DEFERRED

↓

ACCEPTED
```

or

```
PENDING

↓

REQUIRES_COMPONENT_RESTART
```

or

```
PENDING

↓

REQUIRES_APPLICATION_RESTART
```

or

```
PENDING

↓

REJECTED
```

---

# 97. PENDING

Meaning

The consumer has not yet evaluated the published revision.

Initial state.

---

# 98. ACCEPTED

Meaning

The consumer is operating on the published revision.

Stable state.

---

# 99. DEFERRED

Meaning

Consumer intentionally postponed adoption.

The previous revision may remain active inside the consumer.

---

# 100. REQUIRES_COMPONENT_RESTART

Meaning

Consumer cannot adopt without restarting its own component.

Restart may later transition to:

```
ACCEPTED
```

---

# 101. REQUIRES_APPLICATION_RESTART

Meaning

The consumer cannot safely adopt the published configuration without restarting the entire application.

Properties

- configuration remains valid;
- publication remains valid;
- application restart is required before the consumer may transition to:

```
ACCEPTED
```

---

# 102. REJECTED

Meaning

The consumer refuses to adopt the published configuration.

Possible reasons include:

- unsupported runtime state;
- unsupported feature;
- invalid consumer semantics;
- unrecoverable initialization failure.

A rejected consumer does not invalidate the published configuration.

---

# 103. SUPERSEDED

Meaning

A newer configuration revision has replaced the revision currently referenced by this acceptance record.

Example

```
Revision 20

↓

Accepted

↓

Revision 21 Published

↓

Revision 20 Acceptance

↓

SUPERSEDED
```

SUPERSEDED is terminal.

---

# 104. Consumer Acceptance Diagram

```
                 PENDING
                     │
      ┌──────────────┼──────────────┐
      ▼              ▼              ▼
 ACCEPTED        DEFERRED      REJECTED
      │              │
      │              ▼
      │         ACCEPTED
      │
      ├──────────────┐
      ▼              ▼
REQUIRES_      REQUIRES_
COMPONENT      APPLICATION
RESTART         RESTART
      │              │
      └──────┬───────┘
             ▼
         ACCEPTED
             │
             ▼
        SUPERSEDED
```

---

# 105. Consumer Acceptance Recovery

Recovery depends on the consumer.

Possible recovery paths include:

```
Restart

↓

Accept
```

or

```
Manual Reconfiguration

↓

Accept
```

Configuration Infrastructure does not define recovery logic.

---

# 106. Consumer Acceptance Invariants

Consumer Acceptance guarantees:

✓ one consumer

✓ one revision

✓ one current state

✓ immutable history

✓ deterministic transitions

---

# Part X — Configuration Reload State Machine

# 107. Reload Philosophy

Reload is an infrastructure workflow.

Reload does not directly mutate the active configuration.

Instead it creates a new candidate.

Only after successful validation may publication occur.

---

# 108. Reload Lifecycle

```
IDLE

↓

REQUESTED

↓

DISCOVERING

↓

LOADING

↓

NORMALIZING

↓

MERGING

↓

BINDING

↓

VALIDATING

↓

COMPATIBILITY_CHECK

↓

READY_TO_PUBLISH

↓

PUBLISHING

↓

COMPLETED
```

Failure transitions may occur from every processing stage.

---

# 109. IDLE

Meaning

No reload is currently executing.

Stable state.

Allowed transition

```
IDLE

↓

REQUESTED
```

---

# 110. REQUESTED

Meaning

A reload request has been accepted.

Examples

- administrator request;
- explicit API call;
- startup;
- scheduled reload.

The reload has not yet started.

---

# 111. DISCOVERING

Meaning

Configuration sources are being discovered.

Examples

- configuration directory;
- registered providers;
- environment variables;
- runtime overrides.

Transient state.

---

# 112. LOADING

Meaning

All enabled sources are loaded.

Examples

```
Application File

↓

Load

User File

↓

Load

Environment

↓

Read
```

---

# 113. NORMALIZING

Meaning

Source-specific formats become canonical internal objects.

No merge occurs yet.

---

# 114. MERGING

Meaning

Source precedence is evaluated.

Winning values are selected.

Origin metadata is produced.

---

# 115. BINDING

Meaning

Typed configuration sections are created.

Examples

```
RuntimeConfiguration

TranslationConfiguration

ProviderManagementConfiguration
```

Binding failures terminate the reload.

---

# 116. VALIDATING

Meaning

Structural validation executes.

Checks include:

- schema;
- types;
- required fields;
- cross-field rules;
- cross-section rules.

---

# 117. COMPATIBILITY_CHECK

Meaning

Compatibility evaluation executes.

Checks include:

- supported schema versions;
- migration requirements;
- application compatibility.

---

# 118. READY_TO_PUBLISH

Meaning

Candidate configuration satisfies every prerequisite.

Publication has not yet started.

Administrative policy may still prevent publication.

---

# 119. PUBLISHING

Meaning

The new snapshot becomes authoritative.

Publication includes:

- snapshot creation;
- revision allocation;
- event publication;
- consumer notification.

PUBLISHING is transient.

---

# 120. COMPLETED

Meaning

Reload completed successfully.

Properties

✓ new revision active

✓ candidate consumed

✓ consumers notified

The reload returns to:

```
IDLE
```

after completion.

---

# 121. Reload Failure States

Possible failures.

```
DISCOVERY_FAILED

LOAD_FAILED

NORMALIZATION_FAILED

MERGE_FAILED

BINDING_FAILED

VALIDATION_FAILED

COMPATIBILITY_FAILED

PUBLICATION_FAILED

CANCELLED
```

Every failure state is terminal for that reload execution.

---

# 122. DISCOVERY_FAILED

Meaning

Configuration sources could not be discovered.

Examples

- invalid configuration directory;
- inaccessible registry;
- unsupported source provider.

Recovery requires a new reload request.

---

# 123. LOAD_FAILED

Meaning

At least one required source could not be loaded.

Examples

- missing file;
- permission denied;
- malformed source.

The active snapshot remains unchanged.

---

# 124. NORMALIZATION_FAILED

Meaning

Canonical representation could not be produced.

No merge occurs.

---

# 125. MERGE_FAILED

Meaning

Source precedence or merge strategy failed.

No candidate is published.

---

# 126. BINDING_FAILED

Meaning

Typed configuration objects could not be constructed.

Examples

```
RuntimeConfiguration

↓

Binding Failure
```

---

# 127. VALIDATION_FAILED

Meaning

Structural validation failed.

Publication is forbidden.

---

# 128. COMPATIBILITY_FAILED

Meaning

The configuration is valid but not compatible with the running application.

Migration may be required.

---

# 129. PUBLICATION_FAILED

Meaning

Publication could not complete.

Possible causes:

- revision allocation failure;
- persistence failure;
- snapshot storage failure.

The previous active snapshot remains authoritative.

---

# 130. CANCELLED

Meaning

Reload execution stopped intentionally.

Examples

- administrator cancellation;
- application shutdown;
- higher-priority reload request.

CANCELLED is terminal.

---

# 131. Reload State Diagram

The complete reload lifecycle.

```text
                        IDLE
                          │
                          ▼
                     REQUESTED
                          │
                          ▼
                    DISCOVERING
                          │
                          ▼
                      LOADING
                          │
                          ▼
                   NORMALIZING
                          │
                          ▼
                      MERGING
                          │
                          ▼
                      BINDING
                          │
                          ▼
                    VALIDATING
                          │
                          ▼
               COMPATIBILITY_CHECK
                          │
                          ▼
                  READY_TO_PUBLISH
                          │
                          ▼
                     PUBLISHING
                          │
                          ▼
                     COMPLETED
                          │
                          ▼
                         IDLE
```

Failure transitions:

```text
DISCOVERY_FAILED

LOAD_FAILED

NORMALIZATION_FAILED

MERGE_FAILED

BINDING_FAILED

VALIDATION_FAILED

COMPATIBILITY_FAILED

PUBLICATION_FAILED

CANCELLED
```

---

# 132. Reload Recovery

Reload recovery always begins with a new reload request.

Example

```text
VALIDATION_FAILED

↓

Administrator fixes configuration

↓

REQUESTED

↓

New Reload
```

Historical reload executions remain immutable.

---

# 133. Reload Invariants

Reload guarantees:

✓ only one active reload

✓ deterministic execution order

✓ immutable history

✓ publication only after successful completion

---

# Part XI — Global State Relationships

# 134. Overall Lifecycle

The high-level lifecycle of Configuration Infrastructure is:

```text
Configuration Sources

↓

Reload

↓

Candidate

↓

Validation

↓

Compatibility

↓

Snapshot

↓

Revision

↓

Consumers
```

Each stage owns its own state machine.

---

# 135. Source → Candidate Relationship

A candidate always depends on source state.

Required source states:

```text
READY
```

Invalid source states:

```text
FAILED

REMOVED
```

unless explicitly ignored by policy.

---

# 136. Candidate → Snapshot Relationship

A candidate becomes a snapshot only through:

```text
READY

↓

PUBLISHED
```

No other transition is allowed.

---

# 137. Snapshot → Revision Relationship

Publishing creates:

```text
Snapshot

↓

Revision
```

The relationship is one-to-one.

A snapshot never belongs to multiple revisions.

---

# 138. Revision → Consumer Relationship

Consumers never observe unpublished revisions.

Flow:

```text
Revision Published

↓

Consumers Notified

↓

Consumers Evaluate

↓

Acceptance Recorded
```

---

# 139. Override Relationship

Overrides influence:

```text
Effective Configuration
```

They never modify:

```text
Configuration Source

Snapshot

Historical Revision
```

Overrides only affect future effective revisions.

---

# 140. Migration Relationship

Migration always precedes publication.

```text
Old Configuration

↓

Migration

↓

Candidate

↓

Validation

↓

Publication
```

Migration never modifies published snapshots.

---

# 141. Validation Relationship

Validation operates only on:

```text
Candidate
```

Never on:

```text
Published Snapshot
```

Published snapshots remain immutable.

---

# 142. Compatibility Relationship

Compatibility always follows:

```text
Validation

↓

Compatibility
```

Compatibility never executes before successful validation.

---

# 143. Reload Relationship

Reload owns the orchestration.

Reload does not own:

- schema;
- validation rules;
- compatibility rules.

It coordinates them.

---

# Part XII — State Ownership Matrix

# 144. Ownership Philosophy

Every state machine has exactly one owner.

Ownership defines:

- transitions;
- invariants;
- recovery policy.

Ownership never overlaps.

---

# 145. Ownership Matrix

| State Machine | Owner |
|--------------|-------|
| Configuration Source | Configuration |
| Configuration Candidate | Configuration |
| Configuration Snapshot | Configuration |
| Configuration Revision | Configuration |
| Configuration Override | Configuration |
| Validation | Configuration |
| Compatibility | Configuration |
| Migration | Configuration |
| Reload | Configuration |
| Consumer Acceptance | Consumer Module |

---

# 146. Consumer Ownership

Examples

```text
Translation

↓

Owns Translation Acceptance

Runtime

↓

Owns Runtime Acceptance

Presentation

↓

Owns Presentation Acceptance
```

Configuration records acceptance.

Consumers decide acceptance.

---

# 147. Runtime Ownership Boundary

Runtime owns:

```text
Workers

Queues

Scheduling

Cancellation

Execution
```

Runtime never owns:

```text
Configuration Snapshot State

Revision State

Validation State
```

---

# 148. Secret Management Boundary

Secret Management owns:

```text
Secret Lifecycle
```

Configuration owns:

```text
Secret References
```

Configuration never tracks secret states.

---

# 149. Provider Management Boundary

Provider Management owns:

```text
Provider Health

Lease

Selection

Availability
```

Configuration only publishes configuration consumed by Provider Management.

---

# 150. Ownership Invariants

Every state transition is initiated by exactly one owner.

Shared ownership is forbidden.

---

# Part XIII — Global Transition Rules

# 151. Transition Philosophy

Transitions must be:

- deterministic;
- observable;
- replay safe;
- idempotent where applicable.

Transitions must never skip mandatory intermediate states.

---

# 152. Forward-only Rule

Normal execution proceeds only forward.

Example

```text
LOADING

↓

VALIDATING
```

Allowed.

Example

```text
VALIDATING

↓

LOADING
```

Forbidden.

Recovery requires a new lifecycle.

---

# 153. Terminal State Rule

Terminal states have no outgoing transitions.

Examples

```text
REMOVED

EXPIRED

REJECTED

DISCARDED
```

Terminal objects remain queryable according to retention policy.

---

# 154. Immutable History Rule

State history is append-only.

Past states are never modified.

Corrections create new lifecycle instances.

---

# 155. Explicit Transition Rule

Every transition has:

- source state;
- destination state;
- transition reason.

Implicit transitions are forbidden.

---

# 156. Replay Rule

Replaying historical events must reconstruct identical state progression.

Hidden transitions are forbidden.

---

# 157. Recovery Rule

Recovery always creates a new execution.

Recovery never mutates failed execution history.

---

# 158. Cancellation Rule

Cancellation transitions directly to:

```text
CANCELLED
```

No intermediate rollback state exists.

Cleanup occurs outside the state machine.

---

# 159. Publication Rule

Publication may occur only from:

```text
READY_TO_PUBLISH
```

Publication from any other state is forbidden.

---

# 160. Replacement Rule

Replacing active configuration always follows:

```text
Old Snapshot

↓

RETAINED

New Snapshot

↓

ACTIVE
```

Both transitions are atomic from the perspective of consumers.

---

# Part XIV — State Invariants

# 161. Source Invariants

Configuration Sources guarantee:

✓ stable identity

✓ one lifecycle

✓ deterministic transitions

✓ immutable history

---

# 162. Candidate Invariants

Candidates guarantee:

✓ unpublished until READY

✓ published at most once

✓ immutable processing history

---

# 163. Snapshot Invariants

Snapshots guarantee:

✓ immutable contents

✓ immutable revision

✓ one active snapshot

---

# 164. Revision Invariants

Revisions guarantee:

✓ total ordering

✓ append-only history

✓ immutable identity

---

# 165. Override Invariants

Overrides guarantee:

✓ explicit scope

✓ explicit lifetime

✓ deterministic precedence

---

# 166. Validation Invariants

Validation guarantees:

✓ immutable result

✓ deterministic execution

✓ replay safety

---

# 167. Compatibility Invariants

Compatibility guarantees:

✓ explicit outcome

✓ immutable evaluation

✓ deterministic rules

---

# 168. Migration Invariants

Migration guarantees:

✓ immutable source

✓ deterministic transformation

✓ one terminal state

---

# 169. Reload Invariants

Reload guarantees:

✓ single active execution

✓ deterministic orchestration

✓ explicit failures

---

# 170. Consumer Acceptance Invariants

Consumer Acceptance guarantees:

✓ one acceptance per consumer per revision

✓ immutable history

✓ explicit adoption state

---

# Part XV — State Specification Summary

# 171. State Machines Covered

This document specifies:

```text
✓ Configuration Source

✓ Configuration Candidate

✓ Configuration Snapshot

✓ Configuration Revision

✓ Configuration Override

✓ Validation

✓ Compatibility

✓ Migration

✓ Consumer Acceptance

✓ Reload
```

---

# 172. Core Architectural Guarantees

Configuration Infrastructure guarantees:

✓ explicit lifecycle

✓ deterministic transitions

✓ immutable history

✓ append-only revisions

✓ immutable snapshots

✓ replay-safe state evolution

✓ ownership isolation

✓ observable state changes

✓ recovery through new lifecycle

✓ transport-independent state model

---

# 173. Relationship to Other Documents

The complete Configuration specification is composed of:

```text
MODULE.md
    │
    ├── Responsibility
    ├── Boundaries
    └── Architecture

CONTRACT.md
    │
    ├── Commands
    ├── Queries
    ├── DTOs
    └── Public API

STATES.md
    │
    ├── Lifecycles
    ├── State Machines
    ├── Transition Rules
    └── Invariants

EVENTS.md
    │
    ├── Domain Events
    ├── Event Ordering
    └── Event Contracts

ERRORS.md
    │
    ├── Error Taxonomy
    ├── Recovery
    └── Retry Policy

README.md
    │
    ├── Navigation
    ├── Concepts
    └── Module Overview
```

---

# 174. End of State Specification

This document defines the complete lifecycle specification for the Configuration Infrastructure module.

All future implementations of the Configuration module must preserve:

- lifecycle semantics;
- ownership boundaries;
- state invariants;
- transition rules;
- replay guarantees;

regardless of implementation language, storage technology, transport protocol, or deployment model.