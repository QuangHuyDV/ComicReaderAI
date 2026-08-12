# Plugin Lifecycle

* **Document:** Plugin Architecture / Plugin Lifecycle
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

This document defines the runtime lifecycle of a CRAI plugin implementation.

The lifecycle ensures that plugins are:

* discovered safely,
* validated before execution,
* dependency-resolved before activation,
* loaded predictably,
* initialized before use,
* activated only when ready,
* quiesced before shutdown,
* disposed safely,
* unloaded without corrupting CRAI state.

The lifecycle is coordinated by the CRAI Host through the Plugin Manager.

Plugins MUST NOT arbitrarily mutate their own canonical lifecycle state.

---

# Core Principle

```text id="kkd4fm"
Discovery
    |
    v
Validation
    |
    v
Dependency Resolution
    |
    v
Load
    |
    v
Initialize
    |
    v
Activate
    |
    v
Serve Capability Calls
    |
    v
Quiesce
    |
    v
Stop
    |
    v
Dispose
    |
    v
Unload
```

Lifecycle describes runtime execution readiness.

It does NOT replace:

* installation state,
* enablement state,
* compatibility state,
* health state.

---

# Lifecycle Dimensions

CRAI MUST distinguish at least four independent dimensions:

```text id="d9n33j"
Installation / Registry State
Enablement State
Lifecycle State
Health / Availability State
```

Example:

```text id="9tud9v"
installation:
    INSTALLED

enablement:
    ENABLED

lifecycle:
    ACTIVE

health:
    DEGRADED
```

These states MUST NOT be collapsed into one enum.

---

# Lifecycle vs Installation

`NOT_INSTALLED` is not a runtime lifecycle state.

Installation/package management answers:

```text id="yqmdfz"
Does the plugin artifact exist and is it installed?
```

Lifecycle answers:

```text id="7nq37g"
What is the current runtime state of this plugin instance?
```

---

# Lifecycle vs Enablement

A plugin may be:

```text id="n9v48v"
ENABLED
```

in Registry but not currently:

```text id="qtsjqa"
ACTIVE
```

For example:

* lazy loaded,
* dependency unavailable,
* application not using the capability,
* lifecycle activation not yet started.

---

# Lifecycle vs Compatibility

A plugin may be enabled but incompatible.

Example:

```text id="3r7lyh"
enablement:
    ENABLED

compatibility:
    INCOMPATIBLE_API
```

Such a plugin MUST NOT enter loading/activation.

---

# Lifecycle vs Health

Lifecycle and Health are separate.

Example:

```text id="5fa4zw"
lifecycle:
    ACTIVE

health:
    DEGRADED
```

A plugin does not need to be unloaded merely because one dependency is temporarily degraded.

---

# Lifecycle Ownership

The Plugin Manager coordinates lifecycle transitions.

The plugin implementation:

* executes lifecycle hooks,
* reports outcomes,
* reports readiness/failure,
* releases owned resources.

It MUST NOT directly write canonical Registry/lifecycle state.

---

# Lifecycle Authority

Recommended:

```text id="gozg33"
Plugin Manager
    owns transition coordination

Plugin Implementation
    owns hook execution

Runtime
    owns process/thread/cancellation mechanics

Registry
    may persist latest lifecycle projection

Observability
    records transition telemetry
```

---

# Runtime Instance

Each loaded plugin runtime SHOULD have a distinct:

```text id="t36huk"
runtimeInstanceId
```

This identity is separate from:

```text id="ivm8tx"
pluginId
pluginVersion
```

---

# Why Runtime Instance Exists

The same plugin version may be:

* restarted,
* reloaded,
* instantiated for another isolation scope,
* hosted in another process.

Therefore:

```text id="p2wsws"
pluginId + pluginVersion
    !=
runtime instance identity
```

---

# Lifecycle State Model

Recommended canonical runtime states:

```text id="jo20lo"
DISCOVERED
VALIDATED
RESOLVED
LOADING
LOADED
INITIALIZING
INITIALIZED
ACTIVATING
ACTIVE
QUIESCING
STOPPING
STOPPED
DISPOSING
DISPOSED
UNLOADING
UNLOADED
FAILED
```

Not every implementation must persist every transitional state.

Stable/important states SHOULD remain observable.

---

# Transitional vs Stable States

Possible stable states:

```text id="7r9x2w"
DISCOVERED
VALIDATED
RESOLVED
LOADED
INITIALIZED
ACTIVE
STOPPED
DISPOSED
UNLOADED
FAILED
```

Possible transitional states:

```text id="hlndfw"
LOADING
INITIALIZING
ACTIVATING
QUIESCING
STOPPING
DISPOSING
UNLOADING
```

---

# DISCOVERED

`DISCOVERED` means:

* a plugin artifact/descriptor candidate was found,
* no implementation code has been executed.

Discovery semantics are defined by `PLUGIN_DISCOVERY.md`.

---

# Discovery Boundary

`DISCOVERED` does NOT imply:

* trusted,
* compatible,
* enabled,
* dependency-resolved,
* loadable.

---

# VALIDATED

`VALIDATED` means required descriptor-level validation has succeeded.

Validation MAY include:

* descriptor schema,
* Plugin API syntax,
* capability declaration structure,
* platform metadata,
* package integrity structure.

Full activation still requires later checks.

---

# Validation vs Compatibility

Validation answers:

```text id="ct9z73"
Is the descriptor/artifact structurally valid?
```

Compatibility answers:

```text id="75hgs3"
Can this Host/runtime support it?
```

They MAY be evaluated in the same orchestration step but remain semantically distinct.

---

# RESOLVED

`RESOLVED` means runtime prerequisites required before loading/activation have been successfully resolved.

This MAY include:

* required plugin dependencies,
* required capability dependencies,
* compatible versions,
* required Host Services,
* required configuration presence,
* required permission grants,
* selected execution isolation.

---

# Why RESOLVED Exists

Loading before dependency resolution may produce:

* partial activation,
* resource leaks,
* unpredictable startup ordering,
* late version conflicts.

Therefore:

```text id="nx1q5l"
VALIDATED
    does not automatically imply
LOADABLE
```

---

# Resolution Failure

If required dependencies cannot be resolved:

```text id="ikfyn9"
do not load
```

The plugin may remain registered/validated for future resolution.

---

# LOADING

`LOADING` means the Host has started creating the runtime representation.

Possible actions:

* map dynamic library,
* start isolated process,
* create IPC channel,
* construct runtime proxy,
* prepare runtime sandbox.

---

# LOADED

`LOADED` means:

* runtime representation exists,
* implementation can be addressed by the Host,
* normal capability calls are still forbidden.

No capability SHOULD yet be exposed as ACTIVE.

---

# Load Failure

A load failure SHOULD:

* normalize failure,
* release partial loader resources,
* not leave capability bindings active,
* transition to `FAILED` or safe prior state according to policy.

---

# INITIALIZING

`INITIALIZING` means plugin initialization hook is executing.

Possible actions:

* read scoped configuration,
* acquire Host Services,
* validate credentials references,
* create provider clients,
* allocate bounded resources,
* prepare internal state.

---

# INITIALIZED

`INITIALIZED` means initialization succeeded.

The plugin is prepared but MUST NOT yet be selected for normal capability calls unless activation has completed.

---

# Initialization Boundary

Initialization MAY prepare:

```text id="328kp5"
clients
local caches
internal data structures
subscriptions to be activated later
```

It SHOULD NOT:

* publish capability availability prematurely,
* accept ordinary work,
* expose partial runtime state as ACTIVE.

---

# Initialization Count

For one `runtimeInstanceId`:

```text id="g1bc1k"
initialize
```

SHOULD complete successfully at most once.

A failed initialization MAY be retried only according to explicit lifecycle policy.

A successful initialized runtime instance SHOULD NOT be initialized again without disposal/reload.

---

# ACTIVATING

`ACTIVATING` prepares the plugin for serving work.

Possible actions:

* activate event subscriptions,
* start background workers,
* register runtime capability bindings,
* perform readiness check.

---

# ACTIVE

`ACTIVE` means the plugin is eligible to serve capability requests.

Requirements SHOULD include:

```text id="vnl4or"
validated
resolved
loaded
initialized
enabled
not blocked
runtime ready
required capability binding active
```

---

# ACTIVE Replaces Started + Running

The previous model distinguished:

```text id="5qawst"
STARTED
RUNNING
```

but both represented normal operational readiness.

The canonical architecture SHOULD prefer:

```text id="l9q2yv"
ACTIVE
```

unless implementation proves a meaningful need for separate states.

---

# Active Capability Binding

A plugin becoming `ACTIVE` does NOT necessarily mean every declared capability is healthy.

Capability-level availability MAY differ.

Example:

```text id="xfnepz"
plugin lifecycle:
    ACTIVE

capability A:
    AVAILABLE

capability B:
    DEGRADED
```

---

# Capability Registration Timing

Declarative capability declarations exist before runtime activation.

Runtime capability bindings SHOULD become selectable only when activation succeeds.

---

# Capability Binding

Recommended flow:

```text id="6odbjx"
Descriptor Capability Declaration
        |
        v
Validation
        |
        v
Runtime Plugin Initialized
        |
        v
Capability Binding Registered
        |
        v
Capability Available for Selection
```

---

# QUIESCING

`QUIESCING` means the plugin is being removed from normal selection and no new work SHOULD begin.

This is a critical shutdown boundary.

---

# Quiesce Actions

Recommended:

```text id="68il3s"
remove from new capability selection
stop new request acceptance
mark provider draining
stop creating background work
retain active operation tracking
```

---

# Quiesce Before Stop

Critical rule:

```text id="rqlt8b"
stop new work first
then drain/cancel existing work
```

Do NOT dispose resources while new work can still be routed to the plugin.

---

# Capability Unbinding

Runtime capability bindings SHOULD be:

* marked unavailable/draining,
* removed from selection,

before destructive shutdown/disposal begins.

---

# Active Operations

During quiescing, the Host SHOULD know:

```text id="cwcf5x"
activeOperationCount
```

or equivalent runtime execution tracking.

This data belongs to runtime/observability, not Registry truth.

---

# Drain

The shutdown policy MAY allow active work to drain.

Example:

```text id="o9pcrs"
QUIESCING
    |
    v
wait for active operations
    |
    v
STOPPING
```

---

# Cancellation

If draining exceeds deadline, runtime MAY cancel active calls according to:

* Host shutdown policy,
* operation cancellation contract,
* capability semantics.

---

# Shutdown Deadline

Shutdown SHOULD have an explicit deadline.

Plugins MUST NOT be allowed to block application shutdown indefinitely.

---

# STOPPING

`STOPPING` means the plugin stop hook/background shutdown is executing.

Possible actions:

* stop background workers,
* unsubscribe from events,
* flush allowed pending operational state,
* stop timers,
* close long-lived listeners.

---

# STOPPED

`STOPPED` means:

* plugin no longer accepts work,
* runtime object may still hold resources,
* disposal remains required before unload.

---

# DISPOSING

`DISPOSING` means final resource cleanup is running.

Possible actions:

* close clients,
* release handles,
* release memory-owned resources,
* release temporary files,
* release Host Service leases,
* close IPC resources owned by plugin instance.

---

# DISPOSED

`DISPOSED` means the runtime instance no longer owns resources that require plugin-level cleanup.

No capability calls are allowed.

---

# UNLOADING

`UNLOADING` removes the runtime implementation.

Possible actions:

* unload library,
* terminate plugin process,
* destroy proxy,
* remove loader/runtime handles.

---

# UNLOADED

`UNLOADED` means that particular runtime instance no longer exists in executable form.

Registry metadata MAY still exist.

---

# Unloaded vs Removed

```text id="oou8je"
UNLOADED
    = runtime state
```

```text id="3cbs8f"
REMOVED
    = installation/registry state
```

A plugin may be unloaded but remain installed and enabled.

---

# FAILED

`FAILED` represents a lifecycle failure from which the current runtime instance cannot continue normally.

Possible failures:

* load failure,
* initialization failure,
* activation failure,
* unrecoverable internal plugin crash,
* shutdown failure requiring forced cleanup.

---

# Failure Phase

Failure SHOULD retain phase information.

Recommended:

```text id="lc2nu0"
PluginLifecycleFailure
├── runtimeInstanceId
├── pluginId
├── phase
├── failureCode
├── retryability?
├── diagnosticReference?
└── occurredAt
```

---

# FAILED Is Not Automatically Registry Blocked

A runtime failure MUST NOT automatically mean:

```text id="fgxzmx"
plugin blocked permanently
```

Registry/Security policy determines whether future activation remains permitted.

---

# Failure Isolation

A plugin lifecycle failure SHOULD NOT corrupt unrelated plugin state.

Where technical isolation allows:

* unrelated plugins remain active,
* Registry remains consistent,
* capability bindings are cleaned up,
* failed runtime resources are released.

---

# In-Process Limitation

For in-process plugins:

```text id="dvi0pl"
absolute failure isolation cannot always be guaranteed
```

The lifecycle architecture MUST NOT promise process isolation where none exists.

---

# Lifecycle Transition Rules

Plugins MUST NOT jump arbitrarily between states.

Examples of allowed forward transitions:

```text id="nt4v6t"
DISCOVERED
    -> VALIDATED

VALIDATED
    -> RESOLVED

RESOLVED
    -> LOADING

LOADING
    -> LOADED

LOADED
    -> INITIALIZING

INITIALIZING
    -> INITIALIZED

INITIALIZED
    -> ACTIVATING

ACTIVATING
    -> ACTIVE
```

---

# Shutdown Transitions

Recommended:

```text id="84n51k"
ACTIVE
    -> QUIESCING
    -> STOPPING
    -> STOPPED
    -> DISPOSING
    -> DISPOSED
    -> UNLOADING
    -> UNLOADED
```

---

# Fast Shutdown

In failure/emergency scenarios some intermediate hooks MAY be skipped if they cannot run safely.

However runtime MUST still attempt:

```text id="7bu567"
capability isolation
resource containment
process termination
```

and record degraded cleanup.

---

# Transition Preconditions

Each transition SHOULD define preconditions.

Example:

```text id="qn7ckt"
RESOLVED -> LOADING
requires:
    enabled
    compatible
    not blocked
    required dependencies resolved
    required permissions granted
```

---

# Transition Postconditions

Example:

```text id="l1qf6g"
ACTIVATING -> ACTIVE
requires:
    runtime ready
    capability bindings valid
    activation hook successful
```

---

# Transition Command

Recommended:

```text id="a6fpr4"
PluginLifecycleCommand
├── commandId
├── pluginId
├── runtimeInstanceId?
├── action
├── reason
├── deadline?
├── correlationId?
└── requestedAt
```

---

# Lifecycle Result

Recommended:

```text id="az2y1u"
PluginLifecycleResult
├── commandId
├── previousState
├── newState
├── success
├── failure?
├── warnings[]
└── completedAt
```

---

# Transition Idempotency

Duplicate lifecycle commands SHOULD be safe where practical.

Examples:

```text id="au6frm"
Stop inactive plugin
    -> no-op / stable success
```

```text id="mblg3f"
Dispose already disposed plugin
    -> no-op / stable success
```

depending on exact lifecycle contract.

---

# Idempotency Boundary

Idempotency means:

```text id="toftpg"
repeating an already-completed lifecycle command
does not corrupt state
```

It does NOT mean every external side effect is perfectly deterministic.

---

# Determinism

Lifecycle transition rules SHOULD be deterministic for identical:

* current state,
* lifecycle command,
* registry/compatibility constraints,
* dependency resolution,
* permission state,
* normalized hook outcome.

---

# External Non-Determinism

Initialization may depend on:

* network availability,
* provider availability,
* filesystem state,
* process startup.

These outcomes MAY vary.

The state machine reaction MUST remain predictable.

---

# Lifecycle Events

Recommended events:

```text id="0v1u4z"
PluginValidationSucceeded
PluginDependenciesResolved
PluginLoadStarted
PluginLoaded
PluginInitializationStarted
PluginInitialized
PluginActivationStarted
PluginActivated
PluginQuiescing
PluginStopping
PluginStopped
PluginDisposed
PluginUnloaded
PluginLifecycleFailed
```

---

# Event Naming

Events SHOULD describe transitions/outcomes precisely.

Avoid one ambiguous event such as:

```text id="t9bfo6"
PluginUpdated
```

for lifecycle behavior.

---

# Lifecycle Event Payload

Recommended:

```text id="kdmo5w"
pluginId
pluginVersion
runtimeInstanceId?
previousState?
newState?
reasonCode?
correlationId?
occurredAt
```

---

# Event Bus Boundary

Not every internal transition must become a durable global Event Bus event.

High-frequency/internal lifecycle details MAY remain telemetry.

Material state changes MAY publish application/runtime events.

---

# Lifecycle Events vs Registry Events

Example:

```text id="6tv0ak"
PluginEnabled
    = Registry/admin state
```

```text id="snc5lo"
PluginActivated
    = runtime lifecycle
```

They MUST remain distinct.

---

# Enable

Enablement is a Registry/admin operation.

Enabling a plugin MAY trigger activation according to runtime policy.

But:

```text id="yord3x"
ENABLED
    !=
ACTIVE
```

---

# Disable

Disabling an ACTIVE plugin SHOULD normally trigger:

```text id="trjob8"
Registry:
    ENABLED -> DISABLED
```

then runtime:

```text id="01yq4i"
ACTIVE
    -> QUIESCING
    -> STOPPING
    -> ...
```

Exact orchestration order must preserve no-new-work semantics.

---

# Block

Security/admin blocking an active plugin SHOULD result in rapid removal from new capability selection.

Active work MAY be:

* cancelled immediately,
* drained,
* terminated,

according to block reason/policy.

---

# Lazy Loading

An enabled compatible plugin MAY remain:

```text id="0o7u06"
RESOLVED
```

or unloaded until a capability is needed.

This supports lazy activation.

---

# Lazy Activation Flow

```text id="9kndwe"
Capability Needed
        |
        v
Registry Candidate
        |
        v
Dependency Resolution
        |
        v
Load
        |
        v
Initialize
        |
        v
Activate
```

---

# Restart

Restart is NOT a backwards transition of the same runtime instance.

Preferred:

```text id="iz7p8e"
Runtime Instance A
    ACTIVE
      |
      v
    QUIESCING
      |
      v
    STOPPED
      |
      v
    DISPOSED
      |
      v
    UNLOADED

New Runtime Instance B
      |
      v
    LOADING
      |
      v
    ...
      |
      v
    ACTIVE
```

---

# Restart Identity

On restart:

```text id="q2gjac"
pluginId
pluginVersion
```

MAY remain the same.

But:

```text id="h458e6"
runtimeInstanceId
```

MUST change.

---

# Restart Validation

Restart MAY reuse existing validated Registry metadata if:

* plugin artifact unchanged,
* descriptor unchanged,
* compatibility still valid,
* Security policy permits.

It does NOT necessarily need to repeat filesystem discovery.

---

# Restart Dependency Validation

Required dependencies SHOULD be revalidated before or during restart.

Dynamic capability/provider availability may have changed.

---

# Reload

Reload means:

```text id="5l4nka"
unload current runtime implementation
+
create a new runtime instance
```

It MUST NOT reuse disposed runtime object identity.

---

# Upgrade

Plugin upgrade is separate from ordinary restart.

Example:

```text id="9e4oub"
plugin 1.0
    ->
plugin 2.0
```

Upgrade requires:

* new artifact validation,
* compatibility,
* dependency resolution,
* possible config/data migration.

Details belong to `PLUGIN_VERSIONING.md`.

---

# Dependency Loss While ACTIVE

A required dependency MAY become unavailable while plugin is active.

Possible policies:

```text id="lvii4n"
remain ACTIVE but capability DEGRADED
quiesce affected capability
quiesce plugin
fail plugin
attempt dependency recovery
```

The exact behavior depends on dependency criticality.

---

# Optional Dependency Loss

Loss of an optional dependency SHOULD NOT automatically stop the plugin.

Affected optional functionality may degrade.

---

# Permission Revocation While ACTIVE

If a required permission is revoked:

```text id="tc7a5m"
affected capabilities MUST stop using it
```

Runtime may need to:

* quiesce,
* cancel active operations,
* transition plugin to FAILED/STOPPED,
* re-resolve permissions.

Security policy determines urgency.

---

# Configuration Change While ACTIVE

Configuration updates MAY be:

```text id="qmeuz1"
HOT_RELOADABLE
RESTART_REQUIRED
RELOAD_REQUIRED
UNSUPPORTED_WHILE_ACTIVE
```

The plugin descriptor/configuration schema SHOULD declare behavior where useful.

---

# Hot Configuration

A hot-reloadable change SHOULD use an explicit update contract.

It MUST NOT mutate plugin internal state through shared Host objects.

---

# Health While ACTIVE

Health state MAY transition independently:

```text id="zwzt4r"
AVAILABLE
    -> DEGRADED
    -> AVAILABLE
```

without lifecycle transition.

---

# Health-Driven Lifecycle Action

Health may trigger lifecycle action only through explicit operational policy.

Example:

```text id="os6tmo"
persistent UNAVAILABLE
    ->
restart plugin
```

Health telemetry itself does not directly mutate lifecycle.

---

# Runtime Crash

For out-of-process plugin crash:

```text id="dgq4bf"
ACTIVE
    ->
FAILED
```

The Host SHOULD:

* remove capability availability,
* clean runtime handles,
* record failure,
* decide whether restart is allowed.

---

# Crash Restart

Automatic restart MAY be allowed under a bounded runtime recovery policy.

It MUST avoid infinite crash loops.

---

# Restart Budget

Possible:

```text id="kb8q6h"
maximumRestartAttempts
restartWindow
backoff
cooldown
```

Exact runtime restart policy belongs to runtime/recovery architecture.

---

# Lifecycle and Capability Selection

Only active, eligible capability bindings may be selected.

Registry declaration alone MUST NOT be enough.

---

# Lifecycle and Capability Requests

When plugin enters `QUIESCING`:

```text id="5lg2zh"
new requests MUST stop
```

Existing requests follow drain/cancellation policy.

---

# Lifecycle and Cancellation

Lifecycle shutdown MUST propagate cancellation to cancellable operations when required.

Cancellation mechanics belong to Runtime.

---

# Lifecycle and Scheduler

Background jobs owned by a plugin SHOULD be registered through Host scheduling where available.

They MUST be stopped/unregistered during quiesce/stop.

---

# Lifecycle and Event Subscriptions

Event subscriptions SHOULD become active during activation.

They MUST be disabled/unsubscribed before or during stop.

---

# Lifecycle and Host Services

Host Service leases/handles MUST NOT remain valid after plugin disposal unless explicitly shared independently of plugin instance.

---

# Lifecycle and Storage

Plugin-private durable data MUST NOT be deleted automatically during unload.

Unload is runtime cleanup, not data removal.

---

# Lifecycle and Cache

Plugin-local in-memory caches may be discarded on dispose.

Canonical/cache data owned elsewhere follows its own lifecycle.

---

# Lifecycle and Registry

Registry MAY store latest lifecycle projection such as:

```text id="zzdkuw"
lastKnownLifecycleState
runtimeInstanceReference?
```

for diagnostics/recovery.

Registry does not execute transitions.

---

# Lifecycle and Observability

Recommended metrics:

```text id="5i9obr"
plugin_load_duration
plugin_initialization_duration
plugin_activation_duration
plugin_shutdown_duration
plugin_restart_count
plugin_lifecycle_failure_count
plugin_quiesce_timeout_count
plugin_force_termination_count
```

---

# Lifecycle Tracing

One lifecycle command MAY produce a trace:

```text id="a9n39w"
Activate Plugin
├── Resolve Dependencies
├── Load
├── Initialize
└── Activate
```

---

# Lifecycle Audit

Material administrative lifecycle actions MAY require Audit:

* manual enable/disable,
* security block,
* forced termination,
* plugin upgrade,
* permission-driven shutdown.

Routine auto-load/lazy-load transitions normally remain telemetry.

---

# Failure Handling

Lifecycle failures SHOULD:

* isolate affected runtime instance,
* prevent invalid capability selection,
* release partial resources where possible,
* preserve Registry consistency,
* expose normalized error,
* record diagnostics.

---

# Lifecycle Failure Categories

Possible:

```text id="wnxfqa"
PLUGIN_LIFECYCLE_NOT_ELIGIBLE
PLUGIN_LIFECYCLE_DEPENDENCY_UNRESOLVED
PLUGIN_LIFECYCLE_LOAD_FAILED
PLUGIN_LIFECYCLE_INITIALIZATION_FAILED
PLUGIN_LIFECYCLE_ACTIVATION_FAILED
PLUGIN_LIFECYCLE_QUIESCE_TIMEOUT
PLUGIN_LIFECYCLE_STOP_FAILED
PLUGIN_LIFECYCLE_DISPOSE_FAILED
PLUGIN_LIFECYCLE_UNLOAD_FAILED
PLUGIN_LIFECYCLE_INVALID_TRANSITION
PLUGIN_LIFECYCLE_PERMISSION_REVOKED
PLUGIN_LIFECYCLE_RUNTIME_CRASHED
PLUGIN_LIFECYCLE_SHUTDOWN_TIMEOUT
```

---

# Partial Initialization Failure

If initialization partially allocates resources then fails:

```text id="ul6nl1"
cleanup MUST be attempted
```

before the runtime instance is abandoned.

---

# Cleanup Failure

Cleanup failure MUST be observable.

For out-of-process plugins, forced process termination MAY provide final containment.

For in-process plugins, cleanup guarantees are weaker.

---

# Forced Termination

Forced termination MAY be used when:

* plugin process ignores shutdown,
* shutdown deadline expires,
* security policy requires immediate isolation.

Forced termination SHOULD be explicit and observable.

---

# Application Shutdown

Recommended shutdown ordering:

```text id="cjrn5t"
Application Shutdown
        |
        v
Stop New Plugin Capability Selection
        |
        v
Quiesce Active Plugins
        |
        v
Drain / Cancel Active Work
        |
        v
Stop Plugins
        |
        v
Dispose
        |
        v
Unload
```

---

# Dependency-Aware Shutdown

Plugins SHOULD shut down in an order respecting resolved dependency edges.

If:

```text id="w7e3an"
Plugin A depends on Plugin B
```

then normally:

```text id="zgi8jt"
A stops before B
```

---

# Startup Ordering

Required dependencies SHOULD become ready before dependents activate.

This does NOT necessarily require strict sequential loading when independent plugins can initialize concurrently.

---

# Parallel Startup

Independent plugins MAY load/initialize concurrently.

Dependency graph and resource constraints determine safe concurrency.

---

# Lifecycle Concurrency

Plugin Manager MUST serialize incompatible lifecycle commands for the same runtime instance.

Example:

```text id="ij6ipj"
start
and
dispose
```

MUST NOT execute concurrently.

---

# Concurrent Capability Calls

Capability-call concurrency is governed by Plugin API/capability contracts.

Lifecycle transition into QUIESCING must coordinate with those calls.

---

# State Persistence

Exact lifecycle state persistence is optional.

If persisted, stale state from a previous process MUST NOT be trusted as proof that a plugin is currently ACTIVE.

---

# Startup Recovery

After application crash/restart:

```text id="e60s39"
previous ACTIVE state
```

should be treated as historical/last-known state.

Runtime must construct a new runtime instance.

---

# Process Epoch

Runtime MAY use:

```text id="03k76j"
runtimeEpochId
```

to distinguish lifecycle state from previous host executions.

---

# Deterministic Recovery

Startup reconciliation SHOULD derive active runtime state from:

* Registry,
* current artifacts,
* compatibility,
* enablement,
* dependencies,
* Security,
* runtime policy.

It MUST NOT trust stale runtime state blindly.

---

# Architecture Invariants

1. Plugin lifecycle is runtime state.

2. Installation state is separate from lifecycle state.

3. Enablement state is separate from lifecycle state.

4. Compatibility state is separate from lifecycle state.

5. Health state is separate from lifecycle state.

6. Plugin Manager coordinates lifecycle transitions.

7. Plugins MUST NOT arbitrarily mutate canonical lifecycle state.

8. Discovery MUST occur without executing plugin implementation code.

9. Validation occurs before loading.

10. Required dependency resolution occurs before activation and SHOULD occur before loading where feasible.

11. `RESOLVED` is distinct from `VALIDATED`.

12. Loading does not imply initialization.

13. Initialization does not imply activation.

14. Activation is required before normal capability selection.

15. `ACTIVE` SHOULD be the canonical normal operational state.

16. Separate `STARTED` and `RUNNING` states are not required unless implementation proves meaningful distinction.

17. Runtime capability availability MUST NOT be published before activation succeeds.

18. Plugin lifecycle may be ACTIVE while one capability is degraded.

19. Health changes do not automatically require lifecycle transitions.

20. New capability work MUST stop before destructive shutdown begins.

21. QUIESCING removes the plugin/capability from new selection.

22. Existing work follows explicit drain/cancellation policy.

23. Shutdown has a bounded deadline.

24. Resources MUST be released before safe unload where possible.

25. Disposal and unload are distinct.

26. Unloaded does not mean uninstalled.

27. Plugin runtime instance identity is distinct from plugin identity/version.

28. Restart creates a new runtime instance.

29. A disposed runtime instance MUST NOT transition backward to LOADED.

30. Restart MUST NOT reuse disposed runtime object identity.

31. Upgrade is distinct from restart.

32. Configuration reload semantics MUST be explicit.

33. Permission revocation may require quiesce/restart/failure according to Security policy.

34. Dependency loss behavior depends on dependency criticality.

35. Optional dependency loss MUST NOT automatically stop the plugin.

36. Plugin lifecycle failures SHOULD remain isolated.

37. Failed runtime state MUST NOT automatically block the plugin permanently.

38. Lifecycle transition rules SHOULD be deterministic.

39. External initialization outcomes MAY be non-deterministic.

40. Lifecycle command idempotency means duplicate commands do not corrupt state.

41. Lifecycle implementation MUST NOT promise impossible deterministic external side effects.

42. Lifecycle commands for one runtime instance MUST be concurrency-safe.

43. Lifecycle events and Registry events are distinct.

44. Routine lifecycle transitions are telemetry, not automatically Audit.

45. Material administrative/security lifecycle actions MAY require Audit.

46. Plugin-private durable data MUST NOT be deleted during ordinary unload.

47. Registry may retain lifecycle projection but does not execute lifecycle transitions.

48. Runtime statistics do not belong to canonical Registry lifecycle state.

49. Startup recovery MUST NOT trust stale ACTIVE state from a previous process as current activity.

50. Required dependencies should activate before dependents become ACTIVE.

51. Dependents should normally stop before their required dependencies.

52. Independent plugins MAY initialize concurrently.

53. Capability requests MUST respect lifecycle state.

54. New requests MUST NOT enter a QUIESCING/STOPPING plugin.

55. Force termination MUST be explicit and observable.

56. In-process plugins have weaker failure/unload isolation than out-of-process plugins.

57. Lifecycle failures MUST preserve capability-binding integrity.

58. Partial initialization failure MUST trigger cleanup attempts.

59. New plugin execution models SHOULD map to the same logical lifecycle semantics.

60. Removing or unloading a plugin MUST NOT erase canonical CRAI domain truth.

---

# Recommended MVP Scope

CRAI MVP SHOULD support:

* `runtimeInstanceId`,
* DISCOVERED,
* VALIDATED,
* RESOLVED,
* LOADING,
* LOADED,
* INITIALIZING,
* INITIALIZED,
* ACTIVATING,
* ACTIVE,
* QUIESCING,
* STOPPING,
* STOPPED,
* DISPOSING,
* DISPOSED,
* UNLOADING,
* UNLOADED,
* FAILED,
* lifecycle command serialization,
* capability binding on activation,
* capability unbinding/quiesce before shutdown,
* bounded shutdown deadline,
* drain/cancel active calls,
* restart using new runtime instance,
* dependency-aware activation,
* dependency-aware shutdown,
* normalized lifecycle failures,
* lifecycle events,
* lifecycle telemetry.

MVP SHOULD NOT require:

* arbitrary hot reload,
* in-place runtime resurrection,
* background workers for every plugin,
* global always-on health polling for every plugin.

MVP MAY defer:

* hot upgrade,
* zero-downtime plugin replacement,
* multiple runtime instances per plugin,
* Workspace-isolated plugin instances,
* automatic crash restart,
* adaptive restart backoff,
* live capability migration,
* complex distributed plugin lifecycle,
* process-level checkpoint restoration.

---

# Open Decisions

The following SHOULD remain open until implementation/prototype validation:

* exact lifecycle enum,
* whether transitional states are persisted,
* exact `runtimeInstanceId` format,
* whether `RESOLVED` is persisted or derived,
* validation/compatibility ordering,
* dependency-resolution timing relative to load,
* activation hook naming,
* quiesce hook existence,
* drain timeout defaults,
* forced termination behavior,
* lazy-load default,
* restart policy,
* restart budget,
* crash-recovery strategy,
* health-triggered restart policy,
* permission-revocation behavior,
* configuration hot-reload semantics,
* configuration restart requirements,
* capability-binding registration ownership,
* capability-level quiescing,
* lifecycle event persistence,
* lifecycle audit thresholds,
* shutdown dependency ordering algorithm,
* startup concurrency,
* runtime epoch identity,
* stale lifecycle-state reconciliation,
* in-process unload guarantees,
* out-of-process IPC shutdown protocol.

---

# Related Documents

Plugin Architecture:

* `README.md`
* `PLUGIN_SYSTEM.md`
* `PLUGIN_API.md`
* `PLUGIN_REGISTRY.md`
* `PLUGIN_DISCOVERY.md`
* `PLUGIN_DEPENDENCY.md`
* `PLUGIN_CONFIGURATION.md`
* `PLUGIN_SECURITY.md`
* `PLUGIN_VERSIONING.md`

Architecture:

* `../modules/MODULE_DEPENDENCY.md`
* `../modules/OWNERSHIP_MAP.md`

Infrastructure:

* `../../03-infrastructure/event-bus/`
* `../../03-infrastructure/logging/`
* `../../03-infrastructure/telemetry/`
* `../../03-infrastructure/configuration/`

Runtime:

* `../runtime/RESOURCE_LIFECYCLE.md`
* `../runtime/CANCELLATION.md`
* `../runtime/RETRY_POLICY.md`
* `../runtime/RUNTIME_COMPONENTS.md`
