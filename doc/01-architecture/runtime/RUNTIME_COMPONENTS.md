# Runtime Components

* **Document:** Runtime Architecture / Runtime Components
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# 1. Purpose

This document defines the logical runtime components of CRAI and their ownership boundaries.

It acts as the high-level component map for Runtime Architecture.

The Runtime architecture coordinates:

* application runtime startup and shutdown,
* execution scopes,
* WorkItem admission and execution,
* cancellation,
* retry execution,
* execution authority,
* immutable runtime artifacts,
* physical resource lifecycle,
* provider/runtime invocation,
* runtime observability.

This document does NOT:

* define concrete classes or packages;
* choose frameworks;
* define process topology;
* turn Business Modules into Runtime Components;
* own business workflow semantics;
* define provider implementations;
* replace detailed Runtime documents.

---

# 2. Runtime Component Definition

A **Runtime Component** is a logical responsibility participating in execution of CRAI work while the application is running.

A Runtime Component may participate in:

* runtime startup/shutdown;
* execution-state management;
* WorkItem admission;
* Attempt execution;
* cancellation;
* retry execution;
* execution-authority validation;
* runtime artifact management;
* resource lifecycle;
* provider/runtime access;
* runtime telemetry.

A Runtime Component is NOT automatically:

* a Business Module;
* a source-code module;
* a class;
* a thread;
* a process;
* a service;
* a deployment unit.

One logical Runtime Component MAY be implemented through multiple objects, queues, threads or processes.

---

# 3. Core Runtime Concepts

The canonical runtime execution hierarchy is:

```text
Application Instance
        |
        v
Execution Scope
        |
        v
Execution Revision
        |
        v
WorkItem
        |
        v
Attempt
```

For CRAI reading flows:

```text
Execution Scope
    MAY correspond to
Reading Session execution
```

but Runtime does not own Reading Session business semantics.

---

# 3.1 Execution Revision

Runtime MUST use:

```text
ExecutionRevision
```

for runtime freshness/authority semantics.

Avoid using the unqualified word:

```text
Revision
```

where it can be confused with:

* TranslationRevision,
* CharacterRevision,
* ProfileRevision,
* other Domain revisions.

Recommended identity:

```text
ExecutionRevisionId
```

---

# 3.2 Execution Revision vs Domain Revision

```text
ExecutionRevision
    = runtime generation / execution authority
```

```text
TranslationRevision
CharacterRevision
ProfileRevision
    = domain history
```

They are unrelated concepts unless explicitly linked through references.

---

# 3.3 WorkItem

A `WorkItem` represents one logical unit of executable work.

Recommended conceptual structure:

```text
WorkItem
├── workItemId
├── executionScopeId
├── executionRevisionId
├── operationType
├── handlerReference
├── inputArtifactRefs[]
├── configurationReference
├── priority
├── deadline?
├── cancellationScope
└── correlationContext
```

A WorkItem does NOT contain large payloads.

---

# 3.4 Attempt

An `Attempt` is one physical execution attempt of one WorkItem.

```text
WorkItem
   |
   +--> Attempt 1
   +--> Attempt 2
   +--> Attempt 3
```

Retry creates a new:

```text
AttemptId
```

while preserving:

```text
WorkItemId
```

unless a higher-level recovery mechanism explicitly creates another logical WorkItem.

---

# 4. Runtime Architecture Principles

1. Runtime ownership is explicitly partitioned.

2. Runtime Control is the single logical authority for **execution orchestration state**, not every Runtime Component's internal state.

3. Scheduler is the authority for Runtime WorkItem admission.

4. Worker executes Attempts but does not own logical WorkItem state.

5. Worker MUST NOT create downstream business work.

6. Worker and Provider Adapter MUST NOT independently perform orchestration-level Retry.

7. Retry and Fallback remain distinct concepts.

8. Queue and concurrency MUST remain bounded.

9. Large payloads MUST move through `ArtifactRef` or another explicit large-payload handle.

10. Published Runtime Artifacts are immutable.

11. A WorkItem accepts at most one logical terminal outcome.

12. Retry creates a new Attempt identity.

13. Late/stale results have no execution acceptance authority.

14. Cancellation is valid control flow and is not automatically Failure.

15. Runtime does not own Capture, Recognition, Translation, Presentation or Storage business semantics.

16. Runtime authority validation does not replace Business Module validation.

17. Runtime Artifact Store does not replace durable business persistence.

18. Runtime telemetry does not become authoritative runtime state.

19. Telemetry failure MUST NOT corrupt Runtime correctness.

20. Shutdown MUST stop admission before destructive resource cleanup.

---

# 5. Runtime Overview

```text
Application Bootstrap
        |
        v
Runtime Configuration Snapshot
        |
        v
Runtime Control
        |
        +-------------------+--------------------+
        |                   |                    |
        v                   v                    v
Execution Scope         Scheduler          Cancellation
Runtime                     |               Coordination
                            v
                       Work Queues
                            |
                            v
                     Worker Execution
                            |
                            v
                 Execution Adapter Gateway
                            |
                            v
                      Attempt Outcome
                            |
                            v
                     Runtime Control
                            |
              +-------------+-------------+
              |                           |
              v                           v
     Execution State Store          Runtime Artifact Store
                                          |
                                          v
                                   Resource Management
```

Cross-cutting:

```text
Runtime Observability
Event Bus
Configuration
Security
```

---

# 6. Component Groups

Runtime responsibilities are organized into:

```text
Runtime Foundation

Execution Authority

Scheduling and Execution

Execution State and Data

Integration Boundaries

Resource Lifecycle

Observability
```

---

# 7. Runtime Foundation

## 7.1 Application Bootstrap

### Purpose

Initialize the runtime environment according to the boot sequence.

### Responsibilities

Application Bootstrap MAY:

* initialize required infrastructure;
* obtain validated Runtime Configuration;
* initialize Event Bus;
* initialize telemetry foundations;
* initialize runtime stores;
* initialize Plugin/Provider runtime integration;
* wire Runtime Components;
* activate Runtime Control;
* transition application runtime to ready state;
* handle startup failure.

### Ownership

Bootstrap owns only temporary initialization ownership.

Resources MUST be explicitly transferred to their long-lived owners.

### Lifetime

```text
APPLICATION_STARTING
        |
        v
BOOTSTRAPPING
        |
        +--> RUNTIME_READY
        |
        +--> STARTUP_FAILED
```

After successful startup, Bootstrap MUST NOT become the normal runtime orchestrator.

### Related Documents

* `BOOT_SEQUENCE.md`
* `RUNTIME_CONFIG.md`

---

## 7.2 Runtime Configuration Snapshot

### Purpose

Provide immutable configuration identity required for Runtime execution.

### Boundary

Runtime Configuration does NOT own every CRAI configuration source.

Preferred:

```text
Configuration Architecture / Infrastructure
        |
        v
Resolved Runtime Configuration
        |
        v
Runtime Configuration Snapshot
```

### Responsibilities

* validate Runtime-required configuration;
* produce immutable runtime snapshots;
* assign configuration revision/version;
* expose activation state;
* define runtime hot-reload boundary;
* retain secret references rather than secret values.

### Rules

* in-flight execution uses an immutable configuration reference;
* configuration changes MUST NOT silently mutate an Attempt;
* raw secrets MUST NOT enter WorkItems or events;
* Business Modules retain ownership of business configuration semantics.

---

# 8. Execution Authority

## 8.1 Runtime Control

### Purpose

Coordinate execution-level state and authority.

Runtime Control is the logical authority for:

```text
Execution Scope
Execution Revision
WorkItem logical state
accepted WorkItem outcome
execution relevance
cancellation authority
retry lineage
runtime shutdown coordination
```

It is NOT the owner of every mutable runtime state.

---

### Responsibilities

Runtime Control MAY:

* create execution scopes;
* create `ExecutionRevision`;
* materialize WorkItems from orchestration commands;
* request Scheduler admission;
* track logical WorkItem state;
* accept or reject Attempt outcomes;
* detect stale results;
* revoke execution authority;
* coordinate cancellation;
* coordinate Retry requests;
* record accepted Runtime Artifact references;
* notify an owning Orchestrator that an execution result is accepted;
* coordinate graceful shutdown.

---

### Runtime Control Does Not Decide Business Workflow

Critical boundary:

```text
Business / Pipeline Orchestrator
        |
        v
decides logical next work
        |
        v
Runtime Control
        |
        v
materializes executable WorkItem
```

Runtime Control MUST NOT independently infer:

```text
OCR complete
    -> therefore translate

Translation complete
    -> therefore render
```

unless such sequencing is explicitly represented by an orchestration contract.

---

### Ownership

Runtime Control is the single logical writer for its own execution-authority state:

```text
current Execution Revision per execution scope
WorkItem logical state
accepted WorkItem terminal outcome
execution acceptance authority
cancellation authority
retry lineage
runtime shutdown coordination state
```

It does NOT own:

* Scheduler admission state;
* Runtime Configuration state;
* provider health;
* Artifact physical resource state;
* telemetry state;
* Business Module state.

---

### Non-Responsibilities

Runtime Control does NOT:

* perform Recognition;
* perform Translation;
* select AI Models;
* implement providers;
* own business persistence;
* own UI state;
* own Provider Configuration;
* own Domain revisions;
* determine business-valid result semantics.

### Related Documents

* `PIPELINE_RUNTIME.md`
* `PIPELINE_ORCHESTRATION.md`
* `CANCELLATION.md`
* `RETRY_POLICY.md`

---

## 8.2 Execution Scope Runtime

### Purpose

Represent runtime execution metadata associated with an active application/business scope.

For current CRAI reading flows this MAY correspond to:

```text
ReadingSession execution scope
```

### Responsibilities

* associate execution scope with current Execution Revision;
* maintain cancellation scope;
* maintain execution priority;
* hold configuration references;
* track runtime-owned resources;
* support pause/resume/drain where the owning business workflow permits it.

### Boundary

```text
Reading Module
    = Reading Session business semantics

Execution Scope Runtime
    = runtime metadata used to execute it
```

Runtime MUST NOT redefine Reading Session business lifecycle.

---

## 8.3 Execution Authority Validator

### Purpose

Determine whether an Attempt result is still eligible for Runtime acceptance.

This MAY remain an internal responsibility of Runtime Control in MVP.

### Inputs

Possible inputs:

```text
executionScopeId
executionRevisionId
workItemId
attemptId
currentExecutionRevisionId
WorkItem terminal state
cancellation state
configuration identity
supersession state
```

### Decisions

```text
ACCEPT
REJECT_STALE
REJECT_CANCELLED
REJECT_DUPLICATE
REJECT_INVALID_STATE
```

---

### Execution Authority Is Not Business Validation

Critical distinction:

```text
Runtime Authority Validation
    asks:
    "Is this result still current and eligible?"
```

```text
Business Validation
    asks:
    "Is this result correct/acceptable for the capability?"
```

Both MAY be required.

---

### Business Commit Boundary

Recommended:

```text
Attempt Result
      |
      v
Runtime Authority Validation
      |
      v
Accepted Execution Result
      |
      v
Owning Business Module
      |
      v
Business Validation / Commit
```

Runtime authority MUST NOT be interpreted as automatic domain commit authorization.

---

### Presentation Boundary

Runtime MAY confirm:

```text
this result is current
```

but Presentation/Application owns:

```text
whether/how it becomes visible
```

Runtime Control MUST NOT become the Presentation state owner.

---

# 9. Scheduling and Execution

## 9.1 Scheduler

### Purpose

Own Runtime WorkItem admission decisions.

### Responsibilities

Scheduler MAY consider:

* dependency readiness;
* execution relevance;
* queue capacity;
* priority;
* worker capacity;
* resource budget;
* execution deadline;
* current Execution Revision;
* retry admission;
* shutdown/drain state.

Possible decisions:

```text
ADMIT
DEFER
REJECT
REPLACE
```

---

### Scheduler Authority

Scheduler is the only Runtime component that determines whether a logical WorkItem/Attempt enters Runtime execution capacity.

This does NOT mean downstream external systems cannot reject an invocation.

Example:

```text
Scheduler
    admits WorkItem

Provider Gateway
    may later return RATE_LIMITED
```

The latter is an execution outcome, not Runtime admission.

---

### Non-Responsibilities

Scheduler does NOT:

* execute WorkItems;
* decide business next stage;
* create Retry decisions;
* mutate Runtime Control authority;
* own Artifact payloads;
* select business provider/model routes.

---

## 9.2 Work Queues

### Purpose

Hold admitted executable work using bounded capacity.

### Queue Content

Queue entries SHOULD contain lightweight execution metadata:

```text
executionScopeId
executionRevisionId
workItemId
attemptId
operationType
handlerReference
priority
inputArtifactRefs
configurationReference
cancellationReference
deadline
```

Queue MUST NOT contain:

* screenshots;
* large OCR output;
* full translated documents;
* mutable provider DTOs;
* secrets.

---

### Ownership

Queue owns:

```text
queued execution position
```

It does NOT own:

```text
WorkItem logical state
Artifact ownership
Business state
```

---

## 9.3 Worker Execution

### Purpose

Physically execute one Attempt.

### Responsibilities

Worker MAY:

* receive immutable execution input;
* acquire Artifact leases;
* resolve public capability/handler contracts;
* invoke Business Module or Execution Adapter;
* cooperate with cancellation;
* produce temporary execution output;
* normalize execution outcome;
* submit completion command;
* release leases/resources.

---

### Worker Rules

Worker MUST NOT:

* mutate Runtime Control state directly;
* create downstream WorkItems;
* perform orchestration-level Retry;
* select a new fallback route on its own;
* commit Domain state;
* commit UI/Presentation directly;
* treat physical completion as accepted logical success;
* retain resources after terminal cleanup without explicit lease.

---

### Attempt Outcomes

Possible physical Attempt outcomes:

```text
SUCCEEDED
FAILED
CANCELLED
ABANDONED
```

`STALE` is normally an acceptance decision made after the Attempt outcome is reported.

---

## 9.4 Cancellation Coordinator

### Purpose

Coordinate cancellation propagation.

This MAY remain internal to Runtime Control in MVP.

### Cancellation Scopes

```text
APPLICATION
EXECUTION_SCOPE
EXECUTION_REVISION
WORK_ITEM
ATTEMPT
```

### Responsibilities

* receive cancellation requests;
* revoke relevant execution authority;
* remove cancellable queued work;
* signal active Attempts;
* cancel delayed Retry;
* coordinate drain;
* prevent cancelled work from being resurrected.

### Related Document

* `CANCELLATION.md`

---

## 9.5 Retry Coordinator

### Purpose

Apply Runtime Retry Policy to an eligible WorkItem failure.

This MAY remain internal to Runtime Control in MVP.

### Responsibilities

* classify normalized failure;
* verify WorkItem is still relevant;
* check retry budget;
* calculate delay/backoff;
* respect normalized `Retry-After`;
* cancel delayed Retry when stale/cancelled;
* create a new `AttemptId`;
* request Scheduler admission.

---

### Retry Boundary

Runtime Retry SHOULD normally mean:

```text
same WorkItem
+
compatible execution binding
+
new Attempt
```

---

### Fallback Boundary

Critical rule:

```text
Retry
    !=
Fallback
```

If another architecture decides:

```text
new Provider
new Model
new RoutePlan
new execution binding
```

that is a Fallback/Recovery decision.

Runtime MAY execute the resulting new Attempt but MUST NOT silently select that fallback itself.

For AI operations:

```text
AI Fallback / Routing
    chooses new RoutePlan

Runtime
    executes resulting Attempt
```

---

### Worker/Adapter Retry

Worker and Provider Adapter MUST NOT hide orchestration-level Retry loops.

Low-level transport retry MAY exist only when explicitly defined as part of the adapter contract and semantically invisible to Runtime attempt identity.

---

# 10. Execution State and Data

## 10.1 Execution State Store

### Purpose

Store runtime metadata required to support Execution Revision and WorkItem authority.

This replaces ambiguous usage of generic:

```text
Revision Store
```

---

### Responsibilities

* store Execution Revision metadata;
* map execution scope to current Execution Revision;
* track WorkItems;
* retain accepted ArtifactRefs;
* support supersession;
* support authority validation;
* support drain/cleanup eligibility.

### Ownership

Runtime Control is the logical writer.

Store implementation MAY be separate.

---

### Execution Revision Record

Recommended:

```text
ExecutionRevisionRecord
├── executionRevisionId
├── executionScopeId
├── state
├── parentRevisionId?
├── supersededBy?
├── workItemRefs[]
├── acceptedArtifactRefs[]
├── configurationReference
├── createdAt
└── authorityState
```

---

## 10.2 Runtime Artifact Store

### Purpose

Manage immutable execution artifacts used by Runtime.

### Runtime Artifact

A Runtime Artifact is:

```text
immutable execution data
+
runtime metadata
+
explicit ownership/retention
```

It is NOT automatically a canonical Domain artifact.

---

### Responsibilities

* register Runtime Artifact;
* assign `ArtifactId` / `ArtifactRef`;
* perform atomic publication;
* retain artifact metadata;
* resolve ArtifactRefs;
* coordinate leases;
* determine disposal eligibility;
* support temporary/memory/file backing.

---

### Runtime Artifact Examples

Possible examples:

```text
CapturedImageExecutionArtifact
RecognitionExecutionArtifact
StructuredTextExecutionArtifact
TranslationExecutionArtifact
PresentationInputArtifact
```

Names are illustrative.

Owning architecture defines semantic payload contracts.

---

### Runtime Artifact vs Domain Artifact

```text
Runtime Artifact
    = execution payload
```

```text
TranslationRevision
TextBlock Revision
GlossarySnapshot
    = domain truth/history
```

Runtime MUST NOT collapse these concepts.

---

### Runtime Artifact Store vs Storage

```text
Runtime Artifact Store
    -> execution retention
       immutable temporary artifacts
       runtime leases

Storage Module
    -> durable persistence
       recovery
       retention
       schema evolution
```

---

### Cache Boundary

Runtime Artifact Store MUST NOT own canonical Cache Policy.

Cache architecture MAY reference/promote compatible artifacts.

Artifact Store only executes its runtime retention/lookup responsibility.

---

## 10.3 Resource Manager

### Purpose

Manage physical resources owned or leased by Runtime.

This MAY remain a logical responsibility rather than a standalone component in MVP.

### Resource Types

Examples:

```text
memory buffer
temporary file
native handle
GPU allocation
provider connection
process handle
Artifact backing resource
```

### Responsibilities

* resource registration;
* ownership transfer;
* lease tracking;
* disposal eligibility;
* physical cleanup;
* cleanup retry;
* leak detection;
* shutdown cleanup ordering.

---

### Key Rule

```text
Logical authority lost
    !=
resource immediately disposable
```

A resource may remain physically alive while an existing lease drains.

---

# 11. Integration Boundaries

## 11.1 Event Bus

### Purpose

Transport typed asynchronous events.

Event Bus MUST NOT own orchestration.

### Event Bus Does Not

* replace Runtime Control;
* replace Scheduler;
* create business pipeline sequence;
* own query semantics;
* mutate state owners;
* transport raw secrets;
* transport large payloads where ArtifactRef is appropriate.

### Related Documents

* `../core/EVENT_BUS.md`

---

## 11.2 Provider Runtime Gateway

### Purpose

Expose runtime execution access to provider-backed implementations without transferring Provider Management ownership into Runtime.

---

### Why Not Canonical Provider Manager

Canonical provider semantics already belong to Provider Management.

Runtime therefore SHOULD distinguish:

```text
Provider Management
    = provider configuration,
      enablement,
      credentials references,
      provider policy
```

from:

```text
Provider Runtime Gateway
    = executable runtime access
      and short-lived availability
```

---

### Responsibilities

Provider Runtime Gateway MAY:

* expose executable provider/adapter instances;
* resolve execution bindings produced by owning routing/selection logic;
* expose normalized runtime availability;
* enforce provider-instance concurrency;
* expose rate-limit signals;
* invoke provider adapter contracts;
* coordinate provider runtime shutdown;
* normalize execution failures.

---

### Non-Responsibilities

Provider Runtime Gateway does NOT:

* own Provider Configuration;
* own credentials;
* decide Translation/OCR business policy;
* choose AI Model RoutePlan;
* choose Fallback route;
* perform Retry orchestration;
* own authoritative Health projection;
* expose raw provider DTOs;
* commit provider result.

---

### AI Boundary

For AI:

```text
AI Routing
    chooses RoutePlan

Provider Runtime Gateway
    resolves executable deployment/adapter

Worker
    invokes it
```

---

### Plugin Boundary

A provider implementation MAY be supplied by a Plugin.

Runtime consumes the public executable capability.

Runtime does NOT depend on concrete plugin implementation identity unless required by an explicit binding.

---

## 11.3 Secret Access Boundary

### Purpose

Allow privileged adapters to use credentials without distributing raw secret material through Runtime contracts.

This responsibility normally belongs to Infrastructure/Secret Management.

### Runtime Rules

* WorkItems contain secret references only;
* events MUST NOT contain raw secrets;
* configuration snapshots contain references;
* Runtime telemetry MUST redact sensitive values;
* provider adapters receive only minimum required secret access.

---

# 12. Runtime Observability

## 12.1 Purpose

Provide operational visibility without changing Runtime semantics.

### Signals

Runtime Observability MAY include:

* structured logs;
* metrics;
* traces;
* runtime events;
* diagnostic snapshots;
* queue metrics;
* Scheduler decisions;
* WorkItem timelines;
* Attempt timelines;
* provider invocation telemetry;
* resource leak indicators;
* startup/shutdown telemetry.

---

## 12.2 Correlation

Recommended:

```text
ApplicationInstanceId
        |
        v
ExecutionScopeId
        |
        v
ExecutionRevisionId
        |
        v
WorkItemId
        |
        v
AttemptId
```

Other operations MAY add:

```text
requestId
routePlanId
streamId
responseId
correlationId
```

without conflating their meanings.

---

## 12.3 Privacy

Default:

```text
NO CONTENT
```

Ordinary telemetry MUST NOT contain:

* screenshots;
* OCR/source text;
* translated text;
* Prompt;
* full AI Context;
* source URL unless explicitly permitted;
* window title unless explicitly permitted;
* credentials;
* provider request bodies.

---

## 12.4 Telemetry Boundary

Telemetry does NOT own:

* Runtime state;
* Usage Ledger;
* Audit;
* Provider Health truth;
* Domain truth.

Derived Health projections MAY consume telemetry.

---

# 13. Business Module Boundary

Runtime MUST NOT create mirror components such as:

```text
Capture Runtime
Recognition Runtime
Translation Runtime
Presentation Runtime
Storage Runtime
```

merely to reflect Business Module names.

---

## Business Modules Own

```text
business state
business rules
business contracts
business validation
business result semantics
domain commit
```

---

## Runtime Owns

```text
execution state
execution authority
WorkItem admission
Attempt execution
cancellation
Retry execution
runtime Artifact lifecycle
physical resource coordination
runtime observability
```

---

# 14. Runtime / Business Interaction

Correct interaction:

```text
Business / Pipeline Orchestrator
        |
        v
requests logical work
        |
        v
Runtime Control
        |
        v
Scheduler
        |
        v
Work Queue
        |
        v
Worker
        |
        v
Public Module / Adapter Contract
        |
        v
Attempt Result
        |
        v
Runtime Control
        |
        v
Execution Authority Validation
        |
        v
Accepted Execution Result
        |
        v
Owning Business Module / Orchestrator
        |
        v
Business Commit or Next Logical Work
```

This prevents Runtime from becoming the business workflow owner.

---

# 15. Downstream Work

Critical boundary:

```text
Runtime does not infer business next steps.
```

Instead:

```text
Accepted Execution Result
        |
        v
Business / Pipeline Orchestrator
        |
        v
Next Work Decision
        |
        v
Runtime Control
        |
        v
new WorkItem
```

---

# 16. Component Ownership Summary

| Component                      | Primary Ownership                               |
| ------------------------------ | ----------------------------------------------- |
| Application Bootstrap          | Startup sequence and initial ownership transfer |
| Runtime Configuration Snapshot | Active immutable runtime configuration identity |
| Runtime Control                | Execution authority and logical WorkItem state  |
| Execution Scope Runtime        | Scope-specific execution metadata               |
| Execution Authority Validator  | Current/stale/cancelled acceptance decision     |
| Scheduler                      | Runtime WorkItem admission                      |
| Work Queues                    | Bounded queued execution position               |
| Worker Execution               | Physical Attempt execution                      |
| Cancellation Coordinator       | Cancellation propagation coordination           |
| Retry Coordinator              | Same-WorkItem Retry execution                   |
| Execution State Store          | Execution Revision and WorkItem metadata        |
| Runtime Artifact Store         | Immutable runtime execution artifacts           |
| Resource Manager               | Physical resource ownership and disposal        |
| Event Bus                      | Asynchronous event distribution                 |
| Provider Runtime Gateway       | Executable provider/adapter runtime access      |
| Secret Access Boundary         | Privileged secret usage boundary                |
| Runtime Observability          | Runtime telemetry and diagnostics               |

---

# 17. Logical Responsibility vs Standalone Component

Not every logical responsibility needs its own implementation component.

## Likely Standalone

* Runtime Control;
* Runtime Configuration Snapshot;
* Scheduler;
* Work Queues;
* Execution State Store;
* Runtime Artifact Store;
* Worker Execution;
* Event Bus;
* Runtime Observability;
* Provider Runtime Gateway.

## MAY Remain Internal

* Execution Scope Runtime;
* Execution Authority Validator;
* Cancellation Coordinator;
* Retry Coordinator;
* Resource Manager;
* Secret Access Boundary.

Final implementation boundaries depend on:

* Technology Stack;
* Process Topology;
* performance requirements;
* isolation requirements.

---

# 18. Lifetime Model

## 18.1 Application Lifetime

```text
Bootstrap
Runtime Configuration Snapshot
Runtime Control
Scheduler
Work Queues
Execution State Store
Runtime Artifact Store
Provider Runtime Gateway
Event Bus
Runtime Observability
```

---

## 18.2 Execution Scope Lifetime

```text
Execution Scope Runtime
Cancellation Scope
Current Execution Revision
Scope Artifact Ownership
Configuration Reference
```

---

## 18.3 Execution Revision Lifetime

```text
ExecutionRevisionRecord
WorkItems
Accepted ArtifactRefs
Authority
Cancellation Scope
```

---

## 18.4 WorkItem Lifetime

```text
WorkItemId
logical WorkItem state
Attempt lineage
accepted terminal outcome
```

---

## 18.5 Attempt Lifetime

```text
AttemptId
Worker execution context
Artifact leases
execution binding
provider request
temporary resources
Attempt outcome
```

Physical resources MAY outlive the logical Attempt while draining or cleaning up.

---

# 19. Threading and Process Boundary

Logical Runtime Components do not automatically own dedicated threads.

Possible execution contexts:

```text
UI Context
Runtime Control Context
Capture/Observation Context
CPU Worker Pool
Provider I/O Context
GPU Context
Plugin Process
Optional Isolated Process
```

Rules:

1. Runtime Control is a single logical writer, not necessarily one dedicated OS thread.

2. Worker pools MUST NOT mutate Runtime Control state directly.

3. Queue internals MUST NOT be manipulated by arbitrary components.

4. Provider callbacks submit normalized completion signals.

5. Presentation commits occur in the appropriate Presentation/UI context.

6. Process topology MUST NOT change contract semantics.

7. `PROCESS_TOPOLOGY.md` owns process deployment decisions.

---

# 20. Failure Isolation

Typical flow:

```text
Attempt Failure
      |
      v
Retry / Failure Decision
      |
      v
WorkItem Outcome
      |
      v
Execution Revision Degradation
      |
      v
Execution Scope MAY remain active
```

One provider/plugin/worker failure MUST NOT automatically stop the entire Runtime.

---

## Fatal Runtime Failure

Runtime SHOULD enter fatal shutdown only when continuing cannot preserve core invariants.

Examples:

* Runtime Control authority cannot be trusted;
* execution state becomes irreconcilable;
* Artifact/resource ownership cannot be preserved;
* required security boundary is broken;
* required runtime configuration becomes unsafe;
* continued execution risks corrupt side effects/data.

---

# 21. Startup

General startup dependency order:

```text
Validated Configuration
        |
        v
Telemetry Foundation
        |
        v
Required Infrastructure
        |
        v
Event Bus
        |
        v
Execution State / Artifact Stores
        |
        v
Plugin / Provider Runtime Integration
        |
        v
Provider Runtime Gateway
        |
        v
Scheduler / Queues
        |
        v
Runtime Control
        |
        v
Accept New Execution Scopes
```

Exact order belongs to `BOOT_SEQUENCE.md`.

---

# 22. Shutdown

Recommended:

```text
Stop New Execution Scope Creation
        |
        v
Stop Scheduler Admission
        |
        v
Revoke / Quiesce Execution Authority
        |
        v
Remove Cancelled/Obsolete Queued Work
        |
        v
Drain or Cancel Active Attempts
        |
        v
Release Artifact Leases
        |
        v
Dispose Scope / Revision Runtime State
        |
        v
Stop Workers / Provider Runtime
        |
        v
Flush Bounded Critical Diagnostics
        |
        v
Dispose Runtime Infrastructure
```

---

# 23. Dependency Rules

1. Runtime Components communicate through explicit public/runtime contracts.

2. Runtime Control MUST NOT depend on concrete provider implementations.

3. Worker MUST NOT deep-import Runtime Control implementation.

4. Scheduler MUST NOT invoke Business Modules directly.

5. Event Bus MUST NOT mutate state owners.

6. Runtime Artifact Store MUST NOT own durable business persistence policy.

7. Storage MUST NOT manage runtime queues or execution authority.

8. Provider Adapter MUST NOT perform hidden orchestration-level Retry.

9. UI/Presentation MUST NOT invoke Workers or Providers directly.

10. Raw secrets MUST NOT appear in WorkItem/event contracts.

11. Components MUST NOT mutate each other's private queues/state/resources.

12. Ownership transfer MUST be explicit.

13. Deep implementation imports across component boundaries are forbidden.

14. Process boundaries MUST preserve logical contracts.

15. Runtime correctness MUST NOT depend on telemetry availability.

16. Runtime MUST NOT infer business workflow progression.

17. Runtime Retry MUST NOT silently become provider/model Fallback.

18. Provider Runtime Gateway MUST consume Provider Management state rather than redefine it.

---

# 24. Runtime Invariants

1. Runtime Control is the logical authority for execution-orchestration state.

2. Runtime Control does NOT own every Runtime Component's state.

3. Current Execution Revision has current execution authority.

4. Scheduler owns Runtime WorkItem admission.

5. Worker owns physical Attempt execution only.

6. Worker does not create downstream business work.

7. Worker and Provider Adapter do not independently perform orchestration Retry.

8. Retry and Fallback are separate.

9. Queue and concurrency are bounded.

10. Large payloads use ArtifactRefs/explicit handles.

11. Published Runtime Artifacts are immutable.

12. One WorkItem accepts at most one logical terminal outcome.

13. Retry creates a new AttemptId.

14. Late Attempts cannot overwrite an already accepted newer outcome.

15. Stale results cannot become accepted execution results.

16. Cancellation is not automatically Failure.

17. Runtime authority validation does not replace Business validation.

18. Runtime Artifact and Domain Artifact are different concepts.

19. Execution Revision and Domain Revision are different concepts.

20. Runtime Artifact Store and Storage are separate boundaries.

21. Cache Policy is not owned by Runtime Artifact Store.

22. Resource lifetime may outlive logical Attempt lifetime while leased/draining.

23. Artifact disposal requires ownership/lease eligibility.

24. Business Modules retain business semantics and commit authority.

25. Runtime does not own Presentation semantics.

26. Provider Management remains separate from Provider Runtime Gateway.

27. AI Routing remains separate from Runtime execution.

28. Plugin lifecycle remains separate from Runtime provider invocation.

29. Runtime telemetry is not authoritative state.

30. Telemetry failure does not change execution outcome.

31. Shutdown stops admission before destructive cleanup.

32. New runtime implementations must preserve these logical ownership boundaries.

---

# 25. Recommended MVP

CRAI MVP SHOULD implement:

* Application Bootstrap;
* immutable Runtime Configuration snapshots;
* Runtime Control;
* Execution Scope Runtime;
* Execution Revision;
* WorkItem;
* Attempt;
* Scheduler;
* bounded Work Queues;
* Worker Execution;
* cancellation propagation;
* same-binding Retry;
* Execution State Store;
* immutable Runtime Artifact Store;
* Artifact leases;
* basic Resource Manager;
* Event Bus;
* Provider Runtime Gateway;
* Secret Access Boundary;
* structured Runtime Observability;
* graceful shutdown.

MVP SHOULD keep these internal where practical:

```text
Execution Authority Validator
Cancellation Coordinator
Retry Coordinator
Execution Scope Runtime
```

rather than creating unnecessary standalone services.

---

# 26. Deferred Runtime Capabilities

MVP MAY defer:

* distributed Runtime Control;
* distributed Scheduler;
* distributed Work Queues;
* durable queue replay;
* multi-process Runtime state consensus;
* speculative execution;
* provider racing;
* complex WorkItem DAG execution;
* hot process migration;
* distributed Artifact Store;
* remote worker fleet;
* automatic fallback selection inside Runtime;
* autonomous workflow progression.

---

# 27. Open Decisions

The following remain open until Runtime detailed documents/prototyping confirm them:

* exact `ExecutionRevision` schema;
* exact WorkItem contract;
* exact Attempt contract;
* Execution Scope identity;
* Scheduler admission algorithm;
* queue topology;
* cancellation representation;
* Retry budget ownership;
* Runtime Control concurrency model;
* Execution State persistence;
* Runtime Artifact representation;
* Artifact publication protocol;
* Artifact lease implementation;
* Resource Manager implementation;
* Provider Runtime Gateway interface;
* plugin/provider runtime interaction;
* Configuration snapshot format;
* shutdown timeout;
* stale-result handling details;
* recovery after host crash;
* process topology;
* whether Runtime Control is an actor/event-loop/locked state machine;
* exact orchestration interface between Business Pipeline and Runtime.

---

# 28. Related Documents

| Document                    | Relationship                                 |
| --------------------------- | -------------------------------------------- |
| `BOOT_SEQUENCE.md`          | Startup and shutdown sequencing              |
| `RUNTIME_CONFIG.md`         | Runtime Configuration snapshot               |
| `PIPELINE_ORCHESTRATION.md` | Business/runtime orchestration boundary      |
| `PIPELINE_RUNTIME.md`       | ExecutionRevision, WorkItem and Attempt flow |
| `SCHEDULER.md`              | WorkItem admission                           |
| `WORK_QUEUE.md`             | Bounded queued-work lifecycle                |
| `CANCELLATION.md`           | Cancellation scopes and propagation          |
| `RETRY_POLICY.md`           | Retry eligibility and Attempt creation       |
| `CACHE_POLICY.md`           | Runtime artifact reuse/cache promotion       |
| `MEMORY_MODEL.md`           | Runtime memory/artifact ownership            |
| `THREADING_MODEL.md`        | Execution and concurrency contexts           |
| `RESOURCE_LIFECYCLE.md`     | Ownership, lease and disposal                |
| `PERFORMANCE_MODEL.md`      | Runtime performance/backpressure             |
| `ERROR_MODEL.md`            | Runtime error/outcome model                  |
| `RUNTIME_OBSERVABILITY.md`  | Logs, metrics, traces and diagnostics        |
| `PROCESS_TOPOLOGY.md`       | Process boundaries                           |

Related external architecture:

* `../ai/ROUTING.md`
* `../ai/RETRY.md`
* `../ai/FALLBACK.md`
* `../ai/OBSERVABILITY.md`
* `../plugin/PLUGIN_LIFECYCLE.md`
* `../plugin/PLUGIN_SYSTEM.md`
* `../../02-modules/provider-management/`

---

# 29. Completion Criteria

`RUNTIME_COMPONENTS.md` is synchronized when:

* Runtime Components have explicit ownership;
* Runtime is not a mirror of Business Modules;
* Runtime Control is not a God Component;
* Runtime Control/Scheduler/Worker boundaries are distinct;
* Execution Revision is clearly separated from Domain Revision;
* WorkItem/Attempt identity is consistent;
* Retry and Fallback are distinct;
* Runtime does not select AI/provider fallback itself;
* downstream business workflow remains Orchestrator-owned;
* Runtime Artifact is distinct from Domain Artifact;
* Artifact Store is distinct from Storage and Cache Policy;
* Provider Runtime Gateway is distinct from Provider Management;
* execution authority is distinct from business validation;
* queue/concurrency/resources are bounded;
* startup/shutdown ownership is consistent;
* telemetry is non-authoritative;
* terminology matches the rest of `runtime/`.

---

# 30. Summary

CRAI Runtime is organized around the following ownership model:

```text
Business / Pipeline Orchestrator
    owns logical workflow progression.

Runtime Control
    owns execution authority.

Scheduler
    owns Runtime admission.

Workers
    own physical Attempt execution.

Execution State Store
    owns Runtime execution metadata storage.

Runtime Artifact Store
    owns immutable Runtime execution artifacts.

Resource Manager
    owns physical resource lifecycle.

Provider Runtime Gateway
    owns executable provider runtime access.

Business Modules
    own business semantics and domain commit.

Provider Management
    owns provider configuration and governance.

Storage
    owns durable persistence capability.
```

The central rule is:

```text
Runtime executes work.

Runtime does not own the meaning of the work.
```
