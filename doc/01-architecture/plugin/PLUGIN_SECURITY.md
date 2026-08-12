# Plugin Security

* **Document:** Plugin Architecture / Plugin Security
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

This document defines the security architecture for CRAI plugins.

Plugin Security protects:

* CRAI Core,
* Workspace data,
* user/principal data,
* runtime resources,
* secrets,
* provider credentials,
* operating-system resources,
* public extension contracts,

from malicious, compromised, misconfigured or faulty plugin behavior.

Security is enforced through:

* trust evaluation,
* explicit permissions,
* Host Service boundaries,
* isolation,
* integrity verification,
* authorization context,
* observability,
* policy-driven containment.

Plugins MUST operate with the minimum authority required for their declared capabilities.

---

# Core Principle

```text
Plugin
   |
   v
Public Capability / Host Service
   |
   v
Permission + Authorization Check
   |
   v
Approved Resource
```

Plugins MUST NOT gain privileged resource access merely because they are loaded or trusted.

---

# Security Architecture

Recommended:

```text
Plugin Artifact
      |
      v
Integrity / Trust Evaluation
      |
      v
Plugin Registry
      |
      v
Permission Evaluation
      |
      v
Plugin Lifecycle
      |
      v
Plugin Runtime
      |
      v
Host Service Boundary
      |
      v
Permission Enforcement
      |
      v
Sensitive Resource
```

Cross-cutting:

```text
Workspace Scope
Principal Authorization
Telemetry
Audit
Isolation
Policy
```

---

# Scope

Plugin Security covers:

* plugin trust,
* package integrity,
* publisher/signature verification,
* permission declarations,
* permission grants,
* Host Service enforcement,
* network access,
* filesystem access,
* clipboard access,
* screen capture,
* storage access,
* event access,
* secret/credential access,
* Workspace isolation,
* principal authorization,
* runtime isolation,
* plugin containment,
* security findings,
* security decisions,
* permission revocation,
* security events,
* audit candidates.

---

# Non-Goals

Plugin Security does NOT own:

* Plugin lifecycle implementation,
* Plugin Registry persistence,
* plugin configuration semantics,
* Secret storage implementation,
* Provider Configuration,
* runtime telemetry backend,
* business capability ownership,
* AI Safety Policy,
* Workspace Policy truth,
* operating-system sandbox implementation.

---

# Design Principles

Plugin Security SHOULD follow:

* Least Privilege
* Zero Trust
* Explicit Authorization
* Capability-Based Access
* Default Deny
* Defense in Depth
* Isolation
* Tenant Safety
* Secret Minimization
* Auditable Privileged Actions
* Revocability
* Fail-Safe Behavior

---

# Trust vs Permission

Critical distinction:

```text
Trust
    = how much confidence CRAI has
      in plugin provenance/integrity
```

```text
Permission
    = what actions/resources
      the plugin is allowed to access
```

A trusted plugin still requires permission grants.

---

# Trust Model

Recommended trust classes:

```text
BUILT_IN
VERIFIED
USER_APPROVED
UNTRUSTED
DEVELOPMENT
BLOCKED
```

Exact names remain configurable.

---

# BUILT_IN

`BUILT_IN` means the plugin/extension is distributed as part of the trusted CRAI application artifact.

This MAY allow simplified approval flows.

It MUST NOT imply unrestricted Host access.

---

# VERIFIED

`VERIFIED` means plugin provenance/integrity has been validated according to configured trust policy.

Possible evidence:

* trusted publisher signature,
* trusted repository,
* approved package hash,
* enterprise approval.

Verification does NOT automatically grant sensitive permissions.

---

# USER_APPROVED

A third-party plugin MAY be explicitly approved by the user/admin.

Approval SHOULD be tied to:

* plugin identity,
* plugin version/range,
* publisher/integrity identity,
* requested permissions.

---

# UNTRUSTED

Unknown, unsigned or unverifiable plugins SHOULD default to:

```text
UNTRUSTED
```

Policy MAY:

* prohibit execution,
* require explicit approval,
* force stronger isolation,
* limit permissions.

---

# DEVELOPMENT

Development plugins MAY have relaxed installation requirements but SHOULD remain clearly identified.

Production deployments MAY disable DEVELOPMENT trust entirely.

---

# BLOCKED

`BLOCKED` means security or administrative policy prohibits activation.

This maps to Registry block semantics.

---

# Trust Evidence

Recommended:

```text
PluginTrustAssessment
├── pluginId
├── pluginVersion
├── artifactIdentity
├── trustLevel
├── evidence[]
├── evaluatorVersion
├── evaluatedAt
└── policyReference
```

---

# Trust Is Version/Artifact Specific

Trust SHOULD NOT attach only to `pluginId`.

Example:

```text
plugin.foo 1.0
    verified

plugin.foo 1.1
    new artifact
    requires evaluation
```

---

# Publisher Trust

Publisher trust MAY contribute to plugin trust.

It MUST NOT replace package-integrity verification.

---

# Permission Model

Plugins declare permissions they require.

CRAI evaluates and grants only allowed permissions.

Conceptually:

```text
Declared Permissions
        |
        v
Security / Policy Evaluation
        |
        v
Permission Grants
        |
        v
Host Service Enforcement
```

---

# Permission Categories

Recommended initial permissions:

```text
NETWORK
FILE_READ
FILE_WRITE
CLIPBOARD_READ
CLIPBOARD_WRITE
SCREEN_CAPTURE
STORAGE_READ
STORAGE_WRITE
EVENT_PUBLISH
EVENT_SUBSCRIBE
NOTIFICATION_SEND
SECRET_REFERENCE_USE
RAW_SECRET_ACCESS
LOCAL_PROCESS_EXECUTION
LOCAL_MODEL_EXECUTION
TEMPORARY_FILE_ACCESS
DIAGNOSTIC_WRITE
```

Exact taxonomy is versioned.

---

# Capability Is Not Permission

The following are capabilities, NOT permissions:

```text
Recognition
OCR
Translation
AI Execution
Dictionary
Export
```

A plugin may implement:

```text
recognition.engine
```

while requesting:

```text
FILE_READ
LOCAL_PROCESS_EXECUTION
```

or no sensitive permission at all.

---

# Permission Declaration

Recommended:

```text
PluginPermissionDeclaration
├── permissionId
├── required
├── requestedScope?
├── justification?
└── constraints?
```

---

# Required Permission

If a required permission is denied:

```text
plugin dependency/security resolution
    = unresolved
```

The plugin MUST NOT activate the affected capability.

---

# Optional Permission

An optional permission MAY enable additional functionality.

Denial SHOULD degrade only the optional feature where supported.

---

# Permission Grant

Recommended:

```text
PluginPermissionGrant
├── grantId
├── pluginId
├── pluginVersionRange?
├── artifactIdentity?
├── permissionId
├── scope
├── constraints
├── grantedBy
├── policyReference?
├── createdAt
├── expiresAt?
└── revision
```

---

# Permission Scope

Permissions SHOULD be scoped where practical.

Examples:

```text
NETWORK:
    api.example.com:443

FILE_READ:
    user-selected-file

FILE_WRITE:
    workspace-export-directory

STORAGE_WRITE:
    plugin-private namespace

EVENT_SUBSCRIBE:
    approved event types
```

---

# Default Deny

If no grant exists:

```text
access denied
```

No implicit permission should arise from:

* trust,
* plugin category,
* plugin activation,
* previously successful calls.

---

# Permission Enforcement

Permission checks SHOULD occur at the sensitive resource boundary.

Recommended:

```text
Plugin
   |
   v
Host Network Service
   |
   v
Permission Check
   |
   v
Network
```

not:

```text
Plugin
   |
   v
Plugin Manager
   |
   v
every privileged resource
```

---

# Plugin Manager Boundary

Plugin Manager MAY:

* verify required grants before activation,
* react to permission revocation,
* coordinate quiesce/stop.

It SHOULD NOT be the only component enforcing access.

---

# Host Services

Sensitive operations SHOULD be exposed through permission-aware Host Services.

Examples:

```text
NetworkService
FileService
ClipboardService
StorageService
CaptureService
EventService
NotificationService
CredentialBroker
TemporaryFileService
ProcessService
```

---

# Network Security

Network access SHOULD be denied by default.

When granted, constraints MAY include:

* host allowlist,
* port allowlist,
* protocol,
* TLS requirements,
* proxy policy,
* request timeout,
* DNS restrictions,
* private-network restrictions.

---

# Network Scope

Avoid granting:

```text
NETWORK = unrestricted internet
```

when plugin only needs:

```text
api.vendor.example:443
```

---

# SSRF / Internal Network Protection

Network Host Service SHOULD protect sensitive internal targets where appropriate.

Possible restrictions:

* loopback,
* metadata endpoints,
* private ranges,
* internal service networks.

---

# File System Security

Plugins SHOULD NOT receive unrestricted filesystem access.

Preferred models:

```text
user-selected handles
plugin-owned directory
temporary directory
explicit approved paths
```

---

# Path Scope

File permissions SHOULD preserve:

* read/write distinction,
* path scope,
* symlink/path traversal safety.

---

# Plugin-Owned Files

Plugin-private files SHOULD live in scoped storage owned by the Host.

Plugins MUST NOT assume arbitrary application-installation directories are writable.

---

# Storage Security

Plugins access persistence through public Storage contracts.

They MUST NOT:

* connect directly to CRAI internal databases,
* read another plugin's namespace,
* mutate canonical business tables directly.

---

# Storage Namespace

Plugin-private storage SHOULD be scoped by:

```text
pluginId
+
Workspace/Project scope where applicable
```

---

# Clipboard Security

Clipboard read/write are distinct permissions.

Clipboard read SHOULD be treated as more sensitive than ordinary write.

---

# Screen Capture Security

Screen capture MUST require explicit permission.

Scope MAY include:

* selected window,
* selected screen,
* user-approved region,
* application surface.

---

# Screen Capture Is Not OCR Permission

A Recognition/OCR plugin does NOT automatically receive screen capture access.

Capture capability ownership remains separate.

---

# Event Security

Plugins may publish/subscribe only through approved public Event contracts.

Permission MAY constrain:

```text
allowed event types
allowed Workspace scope
publish vs subscribe
```

---

# Internal Events

Private runtime/internal events MUST NOT be exposed unless explicitly promoted to a public plugin contract.

---

# Event Payload Security

Event payloads MUST NOT contain:

* raw secrets,
* credential payloads,
* unrestricted internal runtime handles.

Sensitive business content follows event contract/privacy policy.

---

# Notification Security

Notification sending SHOULD require explicit permission.

Plugins MUST NOT spam users or external endpoints outside approved policy.

---

# Process Execution

Launching local processes is high-risk.

`LOCAL_PROCESS_EXECUTION` SHOULD be strongly restricted.

Preferred:

* Host-managed executable,
* fixed command,
* bounded arguments,
* isolated process,
* resource limits.

---

# Local Model Execution

Local model runtime permission MAY be separate from arbitrary process execution.

This allows CRAI to expose a safe model-runtime Host Service without granting shell access.

---

# Secret Management

Secrets remain owned by Secret Management / Provider Management.

Plugin Security governs whether/how a plugin may use them.

---

# Secret Categories

Examples:

```text
API keys
OAuth tokens
Access tokens
Client secrets
Private keys
Certificates
```

---

# Credential Reference

Preferred:

```text
Plugin Configuration
    contains credentialReference
```

then:

```text
Plugin
    |
    v
Credential Broker / Provider Adapter
    |
    v
Secret Management
```

---

# SECRET_REFERENCE_USE

This permission allows a plugin to reference/use an approved credential without necessarily reading raw secret bytes.

---

# RAW_SECRET_ACCESS

Raw secret access SHOULD be exceptional.

It requires stronger permission/policy.

If granted:

* scope must be minimal,
* lifetime short,
* logs/events must redact,
* plugin MUST NOT persist the value,
* plugin MUST NOT return it through public API.

---

# Plugin Manager Does Not Own Secrets

Critical rule:

```text
Plugin Manager
    does not store/manage canonical secrets
```

It only coordinates lifecycle decisions affected by permission/secret availability.

---

# Secret Exposure

Detected unauthorized secret exposure is a blocking security violation.

Possible action:

```text
revoke
quiesce
stop
block
```

depending on severity/policy.

---

# Configuration Security

Plugins receive only their resolved scoped configuration.

They MUST NOT inspect:

* other plugin configuration,
* arbitrary Workspace settings,
* raw configuration storage.

---

# Workspace Isolation

Plugins MUST preserve Workspace boundaries.

A shared plugin instance MUST NOT mix:

```text
Workspace A data
Workspace B data
```

---

# Workspace Context

Capability calls accessing Workspace data SHOULD carry an authorized opaque Workspace scope/context.

Plugins MUST NOT fabricate another Workspace identity.

---

# Principal Authorization

Where principal/user permissions matter, authorization context MUST propagate through plugin calls.

Plugins MUST NOT elevate principal authority.

---

# Cross-Tenant Violation

Cross-Workspace leakage or access is always a blocking security finding.

---

# Security Context

Recommended:

```text
PluginSecurityContext
├── pluginId
├── runtimeInstanceId
├── workspaceScope?
├── principalScope?
├── permissionGrantReferences[]
├── trustReference
├── correlationId?
└── expiresAt?
```

The exact form may be opaque to plugins.

---

# Security Context Integrity

Plugins MUST NOT:

* modify,
* forge,
* widen,

their security context.

---

# Runtime Isolation

Plugin isolation MAY use:

```text
IN_PROCESS
OUT_OF_PROCESS
SANDBOXED
REMOTE
```

The chosen isolation mode should reflect:

* trust,
* permissions,
* failure risk,
* performance requirements,
* platform capabilities.

---

# In-Process Plugins

In-process plugins share the Host process and therefore cannot be strongly isolated from memory/process failure on many platforms.

Only sufficiently trusted plugins SHOULD use this mode.

---

# Out-of-Process Plugins

Out-of-process execution SHOULD be preferred for higher-risk plugins when feasible.

Benefits:

* crash containment,
* memory isolation,
* process-level termination,
* clearer resource accounting.

---

# Sandboxing

Sandboxing MAY enforce:

* filesystem restrictions,
* network restrictions,
* CPU/memory limits,
* syscall restrictions,
* process restrictions,
* IPC restrictions.

Exact mechanisms are platform-specific.

---

# Sandbox Is Not Permission Replacement

Sandbox and Host permissions provide defense in depth.

A sandboxed plugin still requires explicit permission grants.

---

# Remote Plugins

Remote plugin execution requires:

* authenticated endpoint,
* transport security,
* Workspace isolation,
* authorization,
* data-residency policy,
* request/response validation.

---

# Package Integrity

Plugin artifacts SHOULD have verifiable identity.

Possible evidence:

```text
content hash
package digest
digital signature
trusted repository metadata
```

---

# Signature Verification

Digital signatures MAY establish:

* publisher identity,
* artifact integrity,
* provenance.

Signature verification MUST NOT by itself grant runtime permissions.

---

# Unsigned Plugins

Unsigned plugins MAY:

* be blocked,
* require explicit approval,
* run only in restricted isolation,
* receive limited permissions.

Policy decides.

---

# Integrity Failure

If expected artifact identity/signature fails:

```text
do not load
```

The plugin SHOULD be quarantined or blocked according to policy.

---

# Discovery Boundary

Discovery may read signature metadata/hash artifacts.

Security evaluation performs trust/integrity decisions.

Discovery MUST NOT self-approve the plugin.

---

# Dependency Security

Dependency resolution MUST consider:

* trust where required,
* permission availability,
* capability security constraints.

A plugin MUST NOT obtain sensitive capability indirectly through a dependency if it would not be permitted directly.

---

# Confused Deputy Protection

Host Services and capability providers SHOULD verify caller security context where privileged action may be performed on behalf of another plugin.

---

# Transitive Permissions

Permissions MUST NOT automatically propagate across plugin dependency edges.

Example:

```text
Plugin A has NETWORK
Plugin B depends on A
```

does NOT mean:

```text
Plugin B has NETWORK
```

---

# Delegation

If Plugin B invokes Plugin A capability and A uses Network internally, A's own permission grant authorizes that use.

B does not inherit the Network permission.

---

# Capability Security Requirements

A public capability MAY itself require certain caller authorization.

Example:

```text
capture.screen
```

may require user-approved capture scope.

This is checked independently from implementation plugin trust.

---

# Runtime Monitoring

Security SHOULD consume operational evidence from:

* telemetry,
* Host Service enforcement,
* runtime,
* health probes,
* sandbox events.

Plugin Manager MUST NOT own all monitoring.

---

# Security Findings

Recommended:

```text
PluginSecurityFinding
├── findingId
├── pluginId
├── runtimeInstanceId?
├── category
├── severity
├── permissionId?
├── resourceReference?
├── reasonCode
├── evidenceReference?
├── detectedAt
└── policyReference?
```

---

# Finding Categories

Possible:

```text
PERMISSION_VIOLATION
SECRET_EXPOSURE
INTEGRITY_FAILURE
SIGNATURE_FAILURE
SANDBOX_VIOLATION
CROSS_WORKSPACE_ACCESS
PRINCIPAL_ESCALATION
UNAUTHORIZED_NETWORK_ACCESS
UNAUTHORIZED_FILE_ACCESS
EVENT_ACCESS_VIOLATION
UNSAFE_PROCESS_EXECUTION
SUSPICIOUS_BEHAVIOR
RESOURCE_ABUSE
```

---

# Severity

Recommended:

```text
INFO
WARNING
HIGH
CRITICAL
```

Exact taxonomy may align with system-wide security architecture.

---

# Security Decision

Recommended:

```text
PluginSecurityDecision
├── decision
├── findings[]
├── requiredActions[]
├── permissionChanges[]
├── lifecycleAction?
├── blockRecommended?
├── policyReference
└── evaluatedAt
```

Possible decisions:

```text
ALLOW
ALLOW_WITH_RESTRICTIONS
DENY_PERMISSION
REVOKE_PERMISSION
QUIESCE_PLUGIN
STOP_PLUGIN
BLOCK_PLUGIN
REQUIRE_USER_APPROVAL
REQUIRE_ADMIN_APPROVAL
```

---

# Lifecycle Action Boundary

Security MAY request:

```text
QUIESCE
STOP
BLOCK
```

but Plugin Lifecycle/Registry perform the actual transition/mutation.

Security does not directly kill arbitrary runtime objects behind lifecycle ownership.

---

# Permission Revocation

Permission grants MUST be revocable.

Revocation MAY occur because of:

* user/admin action,
* policy change,
* trust downgrade,
* security finding,
* plugin update,
* scope expiry.

---

# Revocation While Active

When a required permission is revoked:

```text
new privileged calls
    MUST fail immediately
```

Affected plugin capabilities may need:

* quiesce,
* cancellation,
* restart,
* stop.

Lifecycle policy coordinates this.

---

# Grant Expiration

Permission grants MAY expire.

Expired grants behave as denied.

---

# Plugin Upgrade

A new plugin artifact/version SHOULD trigger reevaluation of:

* trust,
* integrity,
* requested permissions,
* existing grants,
* compatibility.

---

# Permission Expansion

If an upgrade requests additional permissions:

```text
existing approval
    MUST NOT automatically grant them
```

unless policy explicitly authorizes compatible expansion.

---

# Trust Downgrade

If publisher/artifact trust is revoked:

* new activations MUST stop,
* active plugin MAY be quiesced/stopped,
* grants MAY be revoked,
* Registry MAY become BLOCKED.

---

# Resource Limits

Security/resource policy MAY enforce:

```text
CPU
memory
process count
file size
network bandwidth
request rate
temporary storage
```

Resource policy ownership may be shared with Runtime.

---

# Resource Abuse

Exceeding resource limits MAY result in:

```text
throttle
deny operation
quiesce
terminate
```

depending on policy.

---

# Security vs Health

Health degradation is not automatically a security issue.

Security consumes security-relevant evidence only.

---

# Security vs Plugin Safety

Plugin Security protects Host/runtime boundaries.

AI Safety protects AI execution/content/policy semantics.

These are separate concerns.

---

# Security vs Workspace Policy

Workspace/Governance owns authoritative Policy.

Plugin Security consumes applicable policy.

---

# Security vs Configuration

Plugin Configuration may reference permissions/credentials.

Security owns access authorization.

Configuration MUST NOT grant permissions by itself.

---

# Security vs Registry

Registry stores:

* trust references,
* block state,
* permission references.

Security owns trust/permission evaluation.

---

# Security vs Lifecycle

Lifecycle executes activation/quiesce/stop.

Security determines whether execution is allowed and may request containment.

---

# Security vs Observability

Observability records evidence.

Security evaluates security meaning.

---

# Security vs Audit

Security telemetry and Audit are distinct.

Material events MAY require Audit:

* permission granted/revoked,
* trust level changed,
* plugin blocked,
* security override,
* raw secret access approved,
* untrusted plugin approved.

---

# Every Privileged Operation Is Not Necessarily Durable Audit

The old invariant:

```text
Every privileged operation is auditable
```

SHOULD mean:

```text
privileged operations are attributable/observable
```

not necessarily that every network/file call produces a durable Audit record.

Otherwise Audit volume becomes unbounded.

---

# Security Events

Recommended events:

```text
PluginTrustEvaluated
PluginPermissionGranted
PluginPermissionDenied
PluginPermissionRevoked
PluginIntegrityFailed
PluginSecurityViolationDetected
PluginSecurityContainmentRequested
PluginBlocked
PluginUnblocked
```

---

# Event Payload Security

Security events SHOULD contain:

* plugin identity,
* permission/finding code,
* scope reference,
* policy reference,
* correlation.

They MUST NOT contain raw secrets.

---

# Security Diagnostics

Diagnostics MAY expose:

```text
pluginId
pluginVersion
runtimeInstanceId
trust level
granted permission IDs
denied permission IDs
security findings
isolation mode
integrity state
```

---

# Sensitive Diagnostics

Diagnostics MUST NOT expose:

* raw credentials,
* secret values,
* arbitrary private content.

---

# Failure Categories

Possible normalized failures:

```text
PLUGIN_SECURITY_PERMISSION_DENIED
PLUGIN_SECURITY_PERMISSION_REVOKED
PLUGIN_SECURITY_TRUST_REQUIRED
PLUGIN_SECURITY_PLUGIN_BLOCKED
PLUGIN_SECURITY_SIGNATURE_INVALID
PLUGIN_SECURITY_INTEGRITY_FAILURE
PLUGIN_SECURITY_SECRET_ACCESS_DENIED
PLUGIN_SECURITY_SECRET_EXPOSURE
PLUGIN_SECURITY_NETWORK_DENIED
PLUGIN_SECURITY_FILE_ACCESS_DENIED
PLUGIN_SECURITY_CLIPBOARD_DENIED
PLUGIN_SECURITY_CAPTURE_DENIED
PLUGIN_SECURITY_EVENT_ACCESS_DENIED
PLUGIN_SECURITY_STORAGE_ACCESS_DENIED
PLUGIN_SECURITY_SANDBOX_VIOLATION
PLUGIN_SECURITY_CROSS_WORKSPACE_VIOLATION
PLUGIN_SECURITY_PRINCIPAL_ESCALATION
PLUGIN_SECURITY_RESOURCE_LIMIT_EXCEEDED
PLUGIN_SECURITY_POLICY_INVALID
```

---

# Fail-Safe Behavior

For mandatory security checks:

```text
unknown / cannot verify
    -> deny
```

is preferred.

Examples:

```text
cannot validate permission
cannot validate Workspace scope
cannot validate plugin block state
```

---

# Fail Open vs Fail Closed

Each control SHOULD define behavior explicitly.

High-risk controls SHOULD normally fail closed.

---

# Architecture Invariants

1. Plugins operate with least privilege.

2. Plugin trust and permission grants are separate concepts.

3. Trust MUST NOT automatically grant unrestricted permissions.

4. Plugin activation MUST NOT imply sensitive resource access.

5. Sensitive resource access requires explicit permission.

6. Missing permission defaults to denial.

7. Permissions SHOULD be scoped.

8. Permission enforcement SHOULD occur at Host Service/resource boundaries.

9. Plugin Manager coordinates lifecycle but is not the sole permission-enforcement component.

10. Plugin Manager does not own canonical secrets.

11. Raw secrets are owned by Secret/Provider infrastructure.

12. Plugin Configuration SHOULD store credential references, not plaintext secrets.

13. Raw secret access is exceptional.

14. Plugin capability and plugin permission are separate concepts.

15. OCR/Recognition/Translation/AI are capabilities, not generic security permissions.

16. Network access is denied by default.

17. File access is denied/scoped by default.

18. Clipboard read/write SHOULD be separate permissions.

19. Screen capture requires explicit authorization.

20. Recognition capability MUST NOT automatically grant screen capture.

21. Plugins MUST NOT access CRAI internal databases directly.

22. Plugin-private storage MUST be namespace-isolated.

23. Event access MUST be contract/permission controlled.

24. Internal private events MUST NOT be visible by default.

25. Local process execution is high-risk and explicitly controlled.

26. Local model execution MAY use a safer dedicated Host Service.

27. Workspace isolation MUST be preserved.

28. Cross-Workspace access is a blocking security violation.

29. Principal authorization MUST be preserved where required.

30. Plugins MUST NOT forge/elevate security context.

31. Permissions do not propagate automatically through plugin dependencies.

32. Dependency providers use their own granted permissions.

33. Capability caller authorization MAY be separate from provider plugin permission.

34. Package integrity SHOULD be verified before loading according to policy.

35. Signature verification does not grant permissions.

36. Plugin trust SHOULD be artifact/version aware.

37. Plugin upgrades may require trust/permission reevaluation.

38. Additional permissions requested by upgrades MUST NOT be silently granted.

39. Sandbox/isolation and Host permissions are defense-in-depth layers.

40. In-process plugins have limited failure/security isolation.

41. Security architecture SHOULD NOT claim perfect containment for in-process plugins.

42. Out-of-process execution SHOULD be preferred for higher-risk plugins where practical.

43. Security monitoring consumes telemetry; Plugin Manager MUST NOT own all monitoring.

44. Security decisions are structured and explainable.

45. Security MAY request lifecycle containment.

46. Lifecycle owns transition execution.

47. Registry owns block-state persistence.

48. Permission grants MUST be revocable.

49. Revoked permissions MUST stop new privileged operations.

50. Configuration MUST NOT grant permissions merely by setting a value.

51. Security telemetry MUST NOT expose secrets.

52. Material security changes MAY require Audit.

53. Ordinary privileged calls SHOULD be observable/attributable but need not each be durable Audit records.

54. Mandatory security checks SHOULD fail closed when authorization cannot be verified.

55. Security failures SHOULD be normalized.

56. Security violation in one plugin SHOULD be contained to that plugin/resource scope where technically possible.

57. Removing/blocking a plugin MUST NOT erase canonical CRAI domain truth.

58. New Host Services MUST define explicit permission/security boundaries before plugin exposure.

---

# Recommended MVP Scope

CRAI MVP SHOULD support:

* trust classification,
* BUILT_IN,
* VERIFIED,
* USER_APPROVED,
* UNTRUSTED,
* BLOCKED,
* artifact hash/integrity checking,
* optional signature verification,
* explicit permission declarations,
* explicit permission grants,
* NETWORK,
* FILE_READ,
* FILE_WRITE,
* CLIPBOARD_READ,
* CLIPBOARD_WRITE,
* SCREEN_CAPTURE,
* STORAGE_READ,
* STORAGE_WRITE,
* EVENT_PUBLISH,
* EVENT_SUBSCRIBE,
* SECRET_REFERENCE_USE,
* LOCAL_MODEL_EXECUTION,
* scoped Host Services,
* default deny,
* Workspace isolation,
* permission revocation,
* plugin blocking,
* security findings,
* security decisions,
* security telemetry,
* material security audit events.

MVP SHOULD avoid:

* unrestricted raw filesystem access,
* unrestricted network access,
* arbitrary shell/process execution,
* raw secret access for normal provider plugins,
* open execution of untrusted third-party plugins without isolation.

MVP MAY defer:

* mandatory digital signatures,
* public plugin certificate infrastructure,
* containers,
* WebAssembly sandbox,
* sophisticated syscall sandboxing,
* dynamic behavior anomaly detection,
* automated malware scanning,
* enterprise publisher trust chains,
* per-principal permission grants,
* fine-grained network bandwidth quotas,
* remote-plugin zero-trust transport.

---

# Open Decisions

The following SHOULD remain open until implementation/prototype validation:

* exact trust-level taxonomy,
* plugin approval UX,
* permission taxonomy,
* permission-grant schema,
* permission scope model,
* Workspace vs principal grants,
* grant expiration,
* network allowlist rules,
* private-network/SSRF policy,
* filesystem handle abstraction,
* capture-scope model,
* event permission model,
* notification permission,
* secret-reference interface,
* whether RAW_SECRET_ACCESS exists in MVP,
* local process execution support,
* local model Host Service,
* default isolation model,
* in-process trust threshold,
* out-of-process IPC security,
* sandbox platform strategy,
* package hash algorithm,
* signature format,
* trusted publisher store,
* trust reevaluation rules,
* plugin-update permission migration,
* runtime resource limits,
* resource-abuse policy,
* Security Finding schema,
* Security Decision schema,
* security event persistence,
* Audit thresholds,
* fail-open/fail-closed matrix.

---

# Related Documents

Plugin Architecture:

* `README.md`
* `PLUGIN_SYSTEM.md`
* `PLUGIN_API.md`
* `PLUGIN_REGISTRY.md`
* `PLUGIN_DISCOVERY.md`
* `PLUGIN_LIFECYCLE.md`
* `PLUGIN_DEPENDENCY.md`
* `PLUGIN_CONFIGURATION.md`
* `PLUGIN_VERSIONING.md`

Architecture:

* `../domain/WORKSPACE.md`
* `../modules/OWNERSHIP_MAP.md`
* `../core/EVENT_BUS.md`

AI:

* `../ai/SAFETY.md`
* `../ai/ROUTING.md`

Modules:

* `../../02-modules/provider-management/`
* `../../02-modules/storage/`
* `../../02-modules/capture/`

Infrastructure:

* `../../03-infrastructure/secret-management/`
* `../../03-infrastructure/logging/`
* `../../03-infrastructure/telemetry/`
* `../../03-infrastructure/storage/`
* `../../03-infrastructure/event-bus/`

Runtime:

* `../runtime/CANCELLATION.md`
* `../runtime/RESOURCE_LIFECYCLE.md`
* `../runtime/RUNTIME_COMPONENTS.md`
