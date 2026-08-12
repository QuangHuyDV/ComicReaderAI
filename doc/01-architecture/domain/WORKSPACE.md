# Workspace Domain

* **Document:** Domain / Workspace
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

A `Workspace` is CRAI's highest-level tenant, administrative, collaboration and policy boundary.

A Workspace groups:

* principals,
* Projects,
* shared domain resources,
* administrative policies,
* resource entitlements,
* provider configuration references,
* collaboration settings,

under one stable ownership context.

A Workspace may represent:

* one individual,
* one translation team,
* one publisher,
* one organization,
* one research group,
* one local CRAI installation,
* one temporary collaboration group.

Workspace answers:

```text
Who owns this CRAI environment?

Who may participate?

Which Projects belong here?

Which shared resources are available?

Which administrative constraints apply?

Which tenant boundary protects this data?
```

Workspace does NOT answer the detailed semantic questions of Translation, Glossary, Character, OCR, Presentation or Session.

---

# Domain Role

Conceptually:

```text
Workspace
├── Membership
├── Authorization Scope
├── Projects
├── Shared Resource Availability
├── Administrative Defaults
├── Mandatory Policies
├── Entitlement References
├── Provider Configuration References
├── Integration References
├── Usage / Billing Scope
├── Audit Scope
└── Tenant Isolation
```

Workspace is primarily a governance boundary.

It MUST NOT become a super-aggregate containing all Workspace-owned data.

---

# Ownership Hierarchy

Typical logical ownership:

```text
Workspace
│
├── Project
│   ├── Book
│   ├── Chapter
│   ├── Page
│   ├── Image
│   ├── TextBlock
│   ├── Translation
│   ├── Character
│   └── Project-scoped resources
│
├── Workspace-scoped Glossary resources
├── Workspace-scoped Profiles
├── Workspace-scoped policies
├── Memberships
├── Provider Configurations
└── Integrations
```

This hierarchy expresses tenant ownership.

It does NOT imply one aggregate transaction boundary.

---

# Workspace Is Not Project

```text
Workspace
    = tenant, administration, collaboration and policy boundary

Project
    = content and translation business boundary
```

A Workspace MAY own many Projects.

For CRAI MVP:

```text
Project
    belongs to exactly one Workspace
```

Project transfer between Workspaces is a migration workflow.

It MUST NOT be implemented as an ordinary `workspaceId` field update.

---

# Workspace Is Not User

Workspace identity is separate from User identity.

```text
User
    !=
Workspace
```

A user MAY:

* own a Workspace,
* belong to several Workspaces,
* leave a Workspace,
* be removed from a Workspace.

Workspace identity remains stable independently.

---

# Workspace Is Not Authentication Tenant

External identity providers may expose:

* organizations,
* tenants,
* directories,
* teams,
* groups.

These MAY map to a CRAI Workspace.

They MUST NOT become canonical Workspace identity.

```text
CRAI Workspace ID
    !=
Authentication Provider Tenant ID
```

Workspace must remain portable across authentication systems.

---

# Workspace Is Not Provider Account

AI/OCR/storage providers may expose:

* accounts,
* organizations,
* projects,
* subscriptions,
* regions.

These are external/infrastructure identities.

```text
Workspace
    |
    v
ProviderConfiguration
    |
    v
Provider Account / Organization
```

A Workspace MAY use several providers.

One provider account MAY serve multiple Workspaces only under explicit isolation policy.

---

# Workspace Is Not Billing Account

Billing-provider customer identity is not Workspace identity.

```text
Workspace ID
    !=
Billing Customer ID
```

Workspace MAY be the logical billing attribution boundary while billing infrastructure maintains external customer/subscription records.

---

# Aggregate Boundary

Workspace SHOULD be an independently addressable Aggregate Root.

Recommended core aggregate:

```text
Workspace
├── workspaceId
├── workspaceType
├── displayName
├── slug?
├── ownerReference
├── lifecycleStatus
├── defaultLocale?
├── defaultTimeZone?
├── activePolicySetRevisionId?
├── administrativeSettings
├── createdAt
├── updatedAt
└── version
```

---

# Core Workspace Ownership

The Workspace Aggregate owns:

* stable Workspace identity,
* Workspace metadata,
* Workspace lifecycle,
* owner reference,
* administrative settings,
* active policy-set reference,
* aggregate version.

It coordinates but SHOULD NOT directly contain high-cardinality collections.

---

# Separate Aggregates and Domains

The following SHOULD remain separate:

```text
WorkspaceMembership
WorkspaceInvitation
Role
RoleAssignment
PolicySet
Project
Glossary
Profile
ProviderConfiguration
IntegrationConfiguration
Subscription
Entitlement
Quota
UsageLedger
AuditRecord
LegalHold
ServiceAccount
```

This prevents Workspace from becoming an unbounded aggregate.

---

# Workspace Responsibilities

Workspace domain is responsible for:

* stable tenant identity,
* administrative ownership,
* membership boundary,
* Project ownership boundary,
* tenant isolation scope,
* Workspace lifecycle,
* Workspace-scoped policy activation,
* Workspace-scoped defaults,
* availability of shared resources,
* administrative governance,
* billing attribution scope,
* usage attribution scope,
* audit scope,
* collaboration boundary.

---

# Workspace Does Not Own Semantic Truth

Workspace MUST NOT own:

* Translation semantics,
* OCR results,
* TextBlock semantics,
* Character truth,
* Glossary Entry semantics,
* Profile semantics,
* Speaker Attribution,
* Review decisions,
* Presentation artifacts,
* Session working state.

Those remain owned by their respective domains.

Workspace may govern whether and how those resources may be used.

---

# Workspace Identity

Every Workspace has a stable:

```text
workspaceId
```

Workspace identity survives:

* rename,
* slug change,
* ownership transfer,
* membership changes,
* policy changes,
* subscription changes,
* provider changes,
* Project creation/deletion,
* infrastructure migration.

Workspace ID MUST NOT be derived solely from:

* Workspace name,
* owner email,
* organization domain,
* provider account ID,
* billing customer ID,
* storage path.

---

# Workspace Type

Recommended values:

```text
PERSONAL
TEAM
ORGANIZATION
PUBLISHER
LOCAL
TEMPORARY
CUSTOM
```

Workspace Type MAY affect initial defaults.

It MUST NOT hard-code authorization semantics.

---

# Personal Workspace

A Personal Workspace represents a tenant primarily controlled by one user.

It MAY support:

* private Projects,
* personal Profiles,
* personal Glossaries,
* local/provider configuration,
* later collaboration.

The user account and Personal Workspace remain separate identities.

---

# Local Workspace

A Local Workspace MAY exist without a cloud account.

Possible characteristics:

* locally generated Workspace ID,
* device-local persistence,
* no remote membership,
* local provider configuration,
* local policy,
* optional later synchronization.

Workspace identity SHOULD be sufficiently unique for future import/synchronization.

---

# Workspace Metadata

Metadata MAY include:

```text
displayName
description
logoReference
slug
preferredLocale
timeZone
websiteReference
organizationCategory
```

Metadata changes normally MUST NOT invalidate Project business artifacts.

---

# Slug

Slug is a mutable human-readable locator.

Example:

```text
moonlight-translations
```

Slug MUST NOT be canonical identity.

Historical slug redirects MAY be supported.

---

# Workspace Ownership

Workspace ownership represents ultimate administrative responsibility.

Recommended:

```text
WorkspaceOwnership
├── workspaceId
├── ownerPrincipalId
├── assignedAt
├── assignedBy
└── transferState?
```

MVP SHOULD support:

```text
one User owner
```

per active Workspace.

---

# Ownership and Membership

Owner identity and Membership SHOULD remain conceptually distinct.

An owner normally also has an active Membership.

But:

```text
Ownership
    = ultimate administrative responsibility

Membership
    = participation relationship
```

This prevents authorization rules from depending on special-case membership types.

---

# Ownership Transfer

Ownership transfer MUST be:

* explicit,
* validated,
* auditable,
* idempotent.

Recommended workflow:

```text
REQUESTED
    |
    v
PENDING_ACCEPTANCE
    |
    v
ACCEPTED
    |
    v
COMPLETED
```

Alternative outcomes:

```text
CANCELLED
REJECTED
EXPIRED
FAILED
```

---

# Ownership Transfer Invariant

A normal active Workspace MUST NOT become ownerless.

Recommended sequence:

```text
validate target
    |
create transfer
    |
obtain acceptance
    |
assign new owner
    |
adjust previous owner's roles
    |
emit event
```

---

# Membership

`WorkspaceMembership` links a Principal to a Workspace.

Recommended:

```text
WorkspaceMembership
├── membershipId
├── workspaceId
├── principalId
├── principalType
├── status
├── joinedAt
├── updatedAt
└── version
```

Membership SHOULD be a separate Aggregate Root.

---

# Principal Types

Future principal types MAY include:

```text
USER
SERVICE_ACCOUNT
EXTERNAL_COLLABORATOR
AUTOMATION_AGENT
DIRECTORY_GROUP
```

MVP SHOULD initially support:

```text
USER
```

---

# Membership Status

Recommended:

```text
INVITED
PENDING
ACTIVE
SUSPENDED
REMOVED
EXPIRED
```

Membership status is independent from User account status.

---

# Invitation

Invitation is separate from Membership.

Recommended:

```text
WorkspaceInvitation
├── invitationId
├── workspaceId
├── inviteeReference
├── intendedRoleReferences
├── intendedProjectScope?
├── invitedBy
├── createdAt
├── expiresAt
└── status
```

Secret invitation tokens belong to security infrastructure.

---

# Role

Role groups permission assignments.

Examples:

```text
OWNER
ADMINISTRATOR
PROJECT_MANAGER
TRANSLATOR
REVIEWER
TERMINOLOGIST
CHARACTER_EDITOR
READER
GUEST
BILLING_MANAGER
INTEGRATION_MANAGER
```

Roles MAY be:

* system-defined,
* Workspace-defined,
* Project-specific.

---

# Permission

Permission represents an authorized capability.

Examples:

```text
workspace.view
workspace.manage
workspace.members.invite
workspace.members.remove
workspace.roles.assign

project.create
project.view
project.translate
project.review

glossary.manage
character.manage
profile.manage

provider.configure
usage.view
billing.manage
audit.view
workspace.export
workspace.delete
```

Permission identifiers SHOULD remain stable.

Localized labels remain presentation concerns.

---

# Role Assignment

Recommended:

```text
RoleAssignment
├── assignmentId
├── workspaceId
├── membershipId
├── roleId
├── scope
├── assignedBy
├── assignedAt
├── expiresAt?
└── status
```

MVP SHOULD prioritize:

```text
WORKSPACE
PROJECT
```

scopes.

---

# Workspace Role vs Project Role

Workspace-level authority and Project-level authority are separate.

Example:

```text
Workspace Billing Manager
```

does NOT automatically mean:

```text
Project Translator
```

unless explicit authorization policy says otherwise.

---

# Authorization Boundary

Workspace defines tenant scope used by authorization.

Every Workspace-owned resource MUST be traceable to one Workspace either:

* directly,
* or through canonical ownership.

Example:

```text
Translation
    ->
TextBlock
    ->
Chapter
    ->
Project
    ->
Workspace
```

Optional Book/Page levels MUST NOT be required for ownership tracing.

---

# Authorization Context

Application/security infrastructure MAY construct:

```text
AuthorizationContext
├── workspaceId
├── principalId
├── membershipId?
├── effectiveRoleAssignments
├── effectivePermissions
├── policyRevisionId
├── entitlementReferences
├── authenticationContextReference
└── correlationId
```

This is evaluated context.

It MUST NOT be copied into every business aggregate as canonical truth.

---

# Authorization Decision

A typical decision may depend on:

```text
Authenticated Principal
        +
Active Membership
        +
Role Assignments
        +
Resource Scope
        +
Workspace Policy
        +
Entitlements
        +
Resource State
        |
        v
Authorization Decision
```

Permission alone MAY be insufficient.

---

# Authorization vs Policy vs Entitlement

These concepts MUST remain distinct.

```text
Permission
    = principal may perform an action

Policy
    = action is allowed under governance constraints

Entitlement
    = Workspace plan/deployment supports the capability
```

All three MAY need to succeed.

Quota MAY additionally constrain execution.

---

# Tenant Isolation

Workspace is CRAI's primary tenant-isolation boundary.

Workspace-private data MUST NOT leak across Workspaces.

Tenant isolation applies to:

* databases,
* object storage,
* caches,
* search indexes,
* vector stores,
* events,
* background operations,
* provider requests,
* logs,
* usage,
* exports,
* backups.

---

# Tenant Attribution

Workspace-scoped operations SHOULD carry:

```text
workspaceId
```

and where relevant:

```text
projectId
principalId
sessionId
correlationId
```

Session ID is correlation context.

It is not tenant identity.

---

# Project Ownership

For MVP:

```text
Project
├── projectId
└── workspaceId
```

Project MUST belong to one Workspace.

Workspace does not directly contain Project content.

---

# Project Creation

Workspace MAY govern:

* who can create Projects,
* Project-count limits,
* permitted Project types,
* default visibility,
* default language intent,
* default Profile selections,
* default Glossary sources,
* retention defaults,
* provider restrictions,
* storage constraints.

Project creation SHOULD capture relevant immutable configuration references where later reproducibility requires them.

---

# Project Visibility

Possible future values:

```text
PRIVATE
WORKSPACE
RESTRICTED
SHARED_LINK
PUBLIC
```

MVP SHOULD initially prioritize:

```text
PRIVATE
WORKSPACE
RESTRICTED
```

Visibility MUST NOT replace authorization.

---

# Project Transfer

Project transfer between Workspaces is an explicit migration workflow.

It may need to resolve:

* Workspace Glossary dependencies,
* Workspace Profile dependencies,
* Character references,
* memberships,
* storage ownership,
* encryption,
* provider configuration,
* policy compatibility,
* scheduled operations,
* audit lineage.

Dependencies MAY be:

```text
COPIED
REBOUND
DETACHED
REJECTED
MANUALLY_RESOLVED
```

---

# Shared Resources

Workspace MAY make domain resources reusable across Projects.

Examples:

* Glossaries,
* Profiles,
* policy sets,
* templates,
* provider configurations,
* integration configurations.

Critical rule:

```text
Workspace scope
    !=
resource semantic ownership
```

---

# Shared Resource Boundary

Workspace determines:

* tenant ownership,
* availability,
* visibility,
* governance,
* authorization scope.

The owning domain determines:

* semantic structure,
* revision model,
* validation,
* lifecycle,
* resolution semantics.

Example:

```text
Workspace
    makes Profile available

Profile domain
    owns Profile semantics
```

---

# Workspace Glossary Boundary

Workspace MAY own or expose Workspace-scoped Glossary resources.

Workspace does NOT own:

* Glossary Entry resolution semantics,
* matching,
* conflict handling,
* immutable Glossary Snapshot construction.

Those belong to Glossary/Application resolution.

---

# Glossary Consumption

Conceptually:

```text
Workspace-scoped Glossary sources
        +
Project sources
        +
Session intent
        +
Operation intent
        |
        v
Glossary Resolver
        |
        v
GlossarySnapshot
```

Translation consumes the immutable Snapshot.

It MUST NOT read mutable Workspace Glossary state directly during durable execution.

---

# Shared Profile Boundary

Workspace MAY provide Workspace-scoped Profiles.

Possible Profile kinds include:

* Translation,
* OCR,
* Presentation,
* Context,
* Validation,
* Routing.

Workspace does NOT own Profile semantic rules.

The Profile domain owns:

* Profile identity,
* revisions,
* inheritance semantics,
* compatibility,
* resolved Profile snapshots.

---

# Workspace Profile Selection

Workspace MAY define a default selection such as:

```text
WorkspaceProfileSelection
├── profileKind
├── selectionMode
├── profileId?
└── profileRevisionId?
```

Possible modes:

```text
EXACT_REVISION
LATEST_APPROVED_COMPATIBLE
DEFAULT
```

The Workspace selection is intent/default.

Operations MUST resolve it to exact immutable revisions before execution.

---

# Workspace Defaults

Workspace MAY provide administrative defaults.

Examples:

* default locale,
* target Language preference,
* Project visibility,
* Profile selections,
* provider preference,
* retention preference,
* collaboration defaults.

A default means:

```text
Use this when a narrower explicit selection does not exist.
```

Defaults are not mandatory restrictions.

---

# Mandatory Policy

Workspace Policy defines constraints.

Examples:

```text
cloud processing forbidden

allowed provider regions = VN, SG

public sharing forbidden

maximum provider cost tier = STANDARD

human provider review forbidden
```

A mandatory policy cannot be overridden by:

* Project,
* Session,
* Operation,
* user preference.

---

# Default vs Policy

Critical distinction:

```text
Default
    = fallback selection

Policy
    = allowed/required constraint
```

Example:

```text
Workspace default:
    target Language = Vietnamese
```

Project MAY choose another Language if allowed.

But:

```text
Workspace policy:
    cloud providers forbidden
```

Project cannot enable cloud processing.

---

# Configuration Resolution Boundary

Workspace MUST NOT define one universal effective-configuration hierarchy for every CRAI domain.

Instead:

```text
Workspace
    contributes defaults
    +
Workspace
    contributes mandatory constraints
```

Relevant capability resolvers then combine those inputs with:

* Project selections,
* Book/Chapter selections where applicable,
* Session selections,
* Operation overrides,
* User preferences,
* capability-specific rules.

---

# Resolved Configuration

At operation start:

```text
Workspace Defaults
        +
Project Defaults / Selections
        +
Session Selections
        +
Operation Overrides
        +
User Preferences
        +
Mandatory Policies
        +
Capability Validation
        |
        v
ResolvedConfigurationSnapshot
```

The exact resolution algorithm belongs to Profile/Application/Capability resolution.

Workspace provides inputs and constraints.

---

# Reproducibility

Critical rule:

```text
Mutable Workspace configuration
    must not silently alter
already-started durable operations.
```

Any Workspace state materially affecting durable output MUST be captured by immutable revision/snapshot reference.

Examples:

* Policy revision,
* exact Profile Revision,
* Glossary Snapshot,
* Character Context Snapshot,
* Resolved Configuration Snapshot.

---

# Policy Set

Workspace SHOULD reference a versioned Policy Set.

Recommended:

```text
WorkspacePolicySet
├── policySetId
├── workspaceId
├── revisionId
├── status
├── effectiveFrom
├── createdBy
└── approvedBy?
```

Policy changes create immutable revisions.

---

# Policy Categories

Potential categories:

```text
ACCESS
COLLABORATION
PRIVACY
PROVIDER_USAGE
DATA_RESIDENCY
RETENTION
EXPORT
IMPORT
CONTENT_CLASSIFICATION
COST_CONTROL
STORAGE
PROCESSING
AUDIT
SHARING
BACKUP
ENCRYPTION
AI_DATA_USAGE
EXTERNAL_INTEGRATION
```

Not all categories are required for MVP.

---

# Policy Evaluation

Policy evaluation SHOULD produce explainable results.

Recommended:

```text
PolicyDecision
├── decision
├── policyRevisionId
├── matchedRuleReferences
├── denialReasons
├── requiredApprovals
├── allowedAlternatives
└── evaluatedAt
```

Possible decisions:

```text
ALLOW
DENY
ALLOW_WITH_WARNING
REQUIRE_APPROVAL
REQUIRE_LOCAL_PROCESSING
REQUIRE_REDACTION
REQUIRE_STRONGER_AUTHENTICATION
```

---

# Policy Change and Running Operations

Policy revision changes MUST NOT silently rewrite historical operation inputs.

For running/new operations, application policy MUST explicitly decide whether to:

* allow existing operation to finish,
* cancel it,
* suspend it,
* require re-evaluation.

Historical artifacts remain linked to the policy/configuration snapshot used when created.

---

# Privacy Policy

Workspace MAY govern:

* local-only processing,
* cloud processing allowance,
* provider allow/deny rules,
* provider retention,
* content logging,
* telemetry,
* external sharing,
* human review,
* export.

---

# AI Data Usage Policy

Workspace SHOULD explicitly govern whether Workspace data may be used for:

* provider training,
* internal model improvement,
* evaluation,
* embedding generation,
* human review,
* quality analysis.

Consent MUST NOT be inferred merely because a provider is configured.

---

# Data Residency

Workspace MAY require allowed:

* storage regions,
* processing regions,
* backup regions,
* cross-region transfer behavior.

Region identifiers SHOULD remain provider-neutral.

---

# Retention Policy

Workspace MAY define retention requirements for categories such as:

* Session data,
* source artifacts,
* OCR intermediates,
* provider responses,
* Translation revisions,
* temporary files,
* audit records,
* backups.

Retention policy governs owning domains/infrastructure.

Workspace itself MUST NOT implement every deletion lifecycle.

---

# Legal Hold

Legal Hold MAY override normal retention/deletion.

It SHOULD remain a separate compliance-domain entity.

Workspace references applicable holds.

---

# Content Classification

Workspace MAY define classification schemes such as:

```text
PUBLIC
INTERNAL
CONFIDENTIAL
RESTRICTED
LICENSED
PERSONAL
UNRELEASED
```

Classification MAY constrain:

* provider use,
* sharing,
* export,
* logging,
* retention,
* external integration.

Classification semantics SHOULD remain policy-driven.

---

# Workspace Locale

Workspace Locale is an administrative default.

It MAY affect:

* administrative UI,
* notification templates,
* formatting,
* default document conventions.

It MUST NOT automatically become:

* Project source Language,
* Translation target Language,
* every member's UI locale.

---

# Language Defaults

Workspace MAY provide Language-related defaults.

All Language values MUST use canonical Language Value Objects.

Workspace MUST NOT invent provider-specific language codes.

Provider adapters own provider code mapping.

---

# Time Zone

Workspace MAY define an administrative time zone.

It may support:

* scheduling,
* reports,
* audit display,
* retention deadlines,
* notification windows.

Canonical timestamps SHOULD remain UTC.

Time-zone identifiers SHOULD use IANA identifiers.

---

# Provider Configuration

Workspace MAY own authorization to use provider configurations.

Recommended separate aggregate:

```text
ProviderConfiguration
├── providerConfigurationId
├── workspaceId
├── providerType
├── capabilityTypes
├── credentialReference
├── region?
├── policyTags
├── status
└── version
```

---

# Credential Boundary

Workspace domain MUST NOT store:

* raw API keys,
* OAuth refresh tokens,
* passwords,
* private keys,
* provider cookies.

Instead:

```text
ProviderConfiguration
    |
    v
SecretReference
    |
    v
Secure Secret Infrastructure
```

---

# Credential Ownership

Possible future models:

```text
USER_OWNED
WORKSPACE_OWNED
SYSTEM_MANAGED
BRING_YOUR_OWN_KEY
LOCAL_DEVICE_ONLY
```

Workspace Policy determines where such credentials may be used.

---

# Provider Routing

Workspace MAY constrain provider routing.

It MUST NOT execute provider routing itself.

Provider/capability infrastructure owns:

* provider selection execution,
* failover,
* retries,
* provider API calls.

Workspace contributes:

* allowed providers,
* forbidden providers,
* regions,
* cost limits,
* privacy restrictions.

---

# Integration Configuration

Workspace MAY own Integration Configuration records.

Recommended:

```text
IntegrationConfiguration
├── integrationId
├── workspaceId
├── integrationType
├── credentialReference
├── scope
├── status
├── policyTags
└── version
```

Integration execution belongs to integration infrastructure.

---

# Service Accounts

Service Accounts are security principals associated with a Workspace.

They SHOULD remain separate from Workspace Aggregate.

Service Accounts SHOULD have:

* explicit roles,
* limited scopes,
* rotatable credentials,
* expiration where possible,
* audit attribution.

MVP SHOULD NOT allow a Service Account to become Workspace owner.

---

# Subscription Boundary

Subscription SHOULD remain a separate billing-domain aggregate.

Workspace MAY reference:

```text
subscriptionId
```

but SHOULD NOT own billing-provider state.

---

# Entitlement

Entitlement represents what the Workspace deployment/plan enables.

Examples:

* cloud Translation,
* collaboration,
* Character recognition,
* advanced Glossary,
* batch processing,
* export formats,
* maximum Project count.

Entitlement is distinct from Permission.

---

# Quota

Quota constrains resource consumption.

Possible categories:

* Project count,
* members,
* storage,
* OCR pages,
* Translation characters,
* Translation tokens,
* provider cost,
* concurrent operations,
* exports.

Quota SHOULD remain independently versioned.

---

# Usage

Usage SHOULD be recorded in a dedicated ledger/domain.

Workspace is the attribution boundary.

Example usage record:

```text
UsageEntry
├── workspaceId
├── projectId?
├── principalId?
├── capability
├── provider?
├── model?
├── quantity
├── unit
├── estimatedCost?
├── finalCost?
├── occurredAt
└── correlationId
```

Session ID MAY be included for correlation.

It is not the billing owner.

---

# Usage Reservation

Expensive operations MAY reserve quota.

```text
REQUESTED
    |
    v
RESERVED
    |
    v
CONSUMED
```

Alternative outcomes:

```text
RELEASED
EXPIRED
ADJUSTED
REJECTED
```

Reservation belongs to usage/quota infrastructure, not the Workspace aggregate.

---

# Storage Ownership

Workspace is the logical tenant owner of stored private data.

Physical storage MAY include:

* local filesystem,
* object storage,
* relational database,
* search index,
* vector store,
* backups,
* provider file services.

Stored objects MUST remain attributable to Workspace.

---

# Storage Namespace

A logical namespace MAY resemble:

```text
workspace/{workspaceId}/project/{projectId}/...
```

Physical path is infrastructure.

It MUST NOT become canonical domain identity.

---

# Encryption

Workspace Policy MAY define encryption requirements.

Examples:

* platform-managed,
* customer-managed,
* local-only,
* key rotation requirements.

Raw encryption keys remain security infrastructure.

Workspace stores only policy/reference information.

---

# Search Isolation

Workspace-private indexed resources MUST carry enough scope information to enforce tenant isolation.

Typical projection metadata:

```text
workspaceId
projectId?
visibility
classification
permissionScope
spoilerScope?
```

Search index is a projection.

It is not canonical authorization truth.

---

# Cache Isolation

Private cache entries MUST preserve tenant isolation.

Workspace ID MAY be omitted from reusable content cache identity only when:

* all semantic inputs are content-addressed,
* output contains no Workspace-specific private data,
* authorization is checked independently,
* cross-tenant reuse is explicitly safe.

---

# Cross-Workspace Learning

Private Workspace data MUST NOT automatically improve another Workspace.

This includes:

* terminology,
* Character information,
* corrections,
* reading history,
* Translation preferences,
* private source content.

Reusable cross-Workspace knowledge requires explicit policy and provenance.

---

# Workspace Knowledge Boundary

Workspace MUST NOT become an unstructured knowledge store.

Reusable knowledge belongs in explicit domains such as:

* Glossary,
* Profile,
* Character,
* Policy,
* Knowledge Base,
* Templates.

Workspace governs ownership and availability.

---

# Template Boundary

Workspace MAY expose versioned templates.

Examples:

* Project templates,
* Profile templates,
* Glossary templates,
* review workflow templates,
* export templates.

Creating a Project from a template SHOULD record the exact template revision used.

After creation, resulting Project configuration becomes independent unless explicit tracking semantics exist.

---

# Review Governance

Workspace Policy MAY require:

* Translation approval,
* terminology approval,
* Character identity approval,
* export approval,
* publication approval.

Workspace defines governance requirements.

The Review domain owns review artifacts and decisions.

---

# Approval Boundary

Workspace MAY define approval requirements.

Example:

```text
ApprovalRequirement
├── resourceType
├── action
├── requiredRoleReferences
├── requiredApprovalCount
├── separationOfDuties
├── scope
└── policyRevisionId
```

Actual Review/Approval execution belongs to workflow/review capability.

---

# Collaboration Boundary

Workspace MAY define collaboration defaults.

Examples:

* guest access,
* comment visibility,
* review assignment defaults,
* mention behavior,
* notification preferences.

Live presence, messaging and transport remain infrastructure/application concerns.

---

# Workspace Lifecycle

Recommended core lifecycle:

```text
PROVISIONING
    |
    v
ACTIVE
    |
    +--> SUSPENDED
    |       |
    |       v
    |     ACTIVE
    |
    +--> ARCHIVED
    |
    +--> PENDING_DELETION
             |
             v
          DELETED
```

Security-sensitive restrictions MAY use a separate:

```text
LOCKED
```

state or restriction flag.

---

# Provisioning

Provisioning MAY initialize:

* Workspace record,
* owner Membership,
* default roles,
* initial Policy Set,
* administrative defaults,
* storage namespace,
* subscription reference,
* audit scope.

Provisioning MUST be idempotent.

---

# Active

Active Workspace permits ordinary operation subject to:

* authorization,
* policy,
* entitlement,
* quota,
* resource state.

---

# Suspended

Suspension MAY result from:

* administrative action,
* billing failure,
* security issue,
* policy violation,
* legal requirement.

Suspension SHOULD preserve existing business data.

Permitted read/export behavior depends on suspension reason and policy.

---

# Locked

Locked indicates a stronger security/compliance restriction.

Possible reasons:

* suspected compromise,
* ownership dispute,
* encryption issue,
* legal restriction,
* migration conflict.

Locked MUST NOT be treated as ordinary billing suspension.

---

# Archived

Archived Workspace:

* preserves data/history,
* disables ordinary active processing,
* restricts administrative mutation,
* MAY permit export,
* MAY be restorable.

Archive is distinct from deletion.

---

# Pending Deletion

Deletion SHOULD normally be delayed.

```text
ACTIVE / ARCHIVED
        |
        v
PENDING_DELETION
        |
        v
DELETED
```

During the pending period:

* new processing is blocked,
* legal hold is checked,
* export MAY remain available,
* deletion MAY be cancelled according to policy.

---

# Deleted

Deleted means Workspace is no longer available for ordinary use.

It does NOT necessarily mean every physical byte has already been erased.

Physical deletion follows:

* resource retention,
* legal hold,
* audit requirements,
* backup retention,
* infrastructure cleanup.

---

# Deletion Cascade

Workspace deletion MAY logically make Workspace-owned resources inaccessible.

But:

```text
Workspace unavailable
    !=
all physical data immediately erased
```

Each owning domain/infrastructure component remains responsible for its physical retention/deletion semantics.

---

# Workspace Restoration

Restoration MAY be supported from:

```text
SUSPENDED
ARCHIVED
PENDING_DELETION
```

Restoration SHOULD validate:

* authorization,
* subscription/entitlement,
* storage availability,
* encryption keys,
* policy compatibility,
* integration validity.

---

# Workspace Merge and Split

Merge and Split MUST be modeled as migration workflows.

They MUST NOT be ordinary Workspace aggregate mutations.

Potential conflicts include:

* members,
* roles,
* Projects,
* Glossaries,
* Profiles,
* policies,
* credentials,
* storage,
* encryption,
* billing,
* audit lineage.

---

# Migration

Workspace migration MAY move data between:

* deployment environments,
* cloud regions,
* self-hosted/cloud,
* storage systems.

Canonical Workspace identity SHOULD remain stable where possible.

Infrastructure identities MAY change.

---

# Import and Export

Workspace import/export are application workflows.

They MAY include:

* Projects,
* shared Profile revisions,
* Glossary revisions,
* policy revisions,
* Character data,
* Translation history,
* administrative metadata.

Credentials SHOULD normally require reconfiguration.

Import/export MUST respect:

* permissions,
* classification,
* encryption,
* policy,
* legal hold,
* licensing,
* spoiler restrictions.

---

# Audit Boundary

Workspace is CRAI's primary administrative audit scope.

Audit MAY cover:

* membership,
* roles,
* ownership,
* Projects,
* policies,
* provider configuration,
* integrations,
* billing administration,
* quota changes,
* export,
* deletion,
* security changes.

Audit records SHOULD remain a separate append-oriented domain/infrastructure concern.

---

# Audit vs Usage vs Telemetry

These MUST remain distinct.

```text
Audit
    = who changed administrative/business state

Usage
    = what resource was consumed

Telemetry
    = how the system behaved operationally
```

Example:

```text
WorkspaceMemberRemoved
```

is audit/domain history.

```text
Translation characters = 42,000
```

is usage.

```text
Translation latency = 930 ms
```

is telemetry.

---

# Events

Core Workspace events MAY include:

```text
WorkspaceCreated
WorkspaceActivated
WorkspaceUpdated

WorkspaceSuspended
WorkspaceResumed
WorkspaceLocked
WorkspaceArchived

WorkspaceDeletionRequested
WorkspaceDeletionCancelled
WorkspaceDeleted

WorkspaceOwnershipTransferRequested
WorkspaceOwnershipTransferred

WorkspaceMemberInvited
WorkspaceMemberJoined
WorkspaceMemberSuspended
WorkspaceMemberRemoved

WorkspaceRoleAssigned
WorkspaceRoleRevoked

WorkspacePolicyRevisionActivated

WorkspaceSharedResourceMadeAvailable
WorkspaceSharedResourceWithdrawn

ProjectCreatedInWorkspace
ProjectTransferRequested
ProjectTransferred
```

Billing, quota, integration and provider configuration domains SHOULD emit their own events where they own the state transition.

---

# Event Payload

Workspace events SHOULD contain identifiers and safe administrative metadata.

They MUST NOT include:

* raw credentials,
* invitation secrets,
* full source content,
* provider prompts,
* payment card information,
* unnecessary personal data.

---

# Event Routing

Workspace-scoped events SHOULD include:

```text
workspaceId
```

for routing/isolation.

Possession of an event MUST NOT imply authorization to read referenced resources.

---

# Concurrency

Workspace administrative updates SHOULD use optimistic concurrency.

Typical controls:

* expected Workspace version,
* expected active Policy Revision,
* Membership version,
* RoleAssignment version,
* ownership-transfer state,
* idempotency key.

Sensitive workflows MAY require stronger coordination.

---

# Idempotency

Idempotency SHOULD apply to:

* Workspace provisioning,
* invitation creation/acceptance,
* ownership transfer,
* Project creation,
* policy activation,
* suspension,
* deletion request,
* migration request,
* export request.

---

# Validation

Workspace validation SHOULD verify:

* stable Workspace identity,
* valid lifecycle transition,
* eligible owner for normal active states,
* active owner Membership where required,
* valid policy revision ownership,
* Project ownership is unambiguous,
* shared-resource references belong to or are accessible by Workspace,
* Membership scope belongs to Workspace,
* Role assignments are valid,
* mandatory policy cannot be bypassed,
* deletion is not blocked by applicable hold,
* cross-Workspace references satisfy explicit sharing/migration rules.

Semantic validation of Profile, Glossary, Character, Translation and other resources belongs to their owning domains.

---

# Error Codes

Recommended stable codes:

```text
WORKSPACE_NOT_FOUND
WORKSPACE_ACCESS_DENIED
WORKSPACE_SUSPENDED
WORKSPACE_LOCKED
WORKSPACE_ARCHIVED
WORKSPACE_PENDING_DELETION

WORKSPACE_STATE_TRANSITION_INVALID
WORKSPACE_VERSION_CONFLICT
WORKSPACE_SLUG_CONFLICT

WORKSPACE_OWNER_MISSING
WORKSPACE_OWNER_TRANSFER_NOT_ALLOWED
WORKSPACE_TARGET_OWNER_INELIGIBLE

WORKSPACE_MEMBERSHIP_NOT_FOUND
WORKSPACE_MEMBERSHIP_INACTIVE
WORKSPACE_MEMBERSHIP_LIMIT_REACHED

WORKSPACE_INVITATION_EXPIRED
WORKSPACE_INVITATION_REVOKED

WORKSPACE_ROLE_ASSIGNMENT_INVALID
WORKSPACE_PERMISSION_DENIED
WORKSPACE_POLICY_DENIED
WORKSPACE_ENTITLEMENT_MISSING
WORKSPACE_QUOTA_EXCEEDED

WORKSPACE_PROVIDER_FORBIDDEN
WORKSPACE_DATA_RESIDENCY_VIOLATION

WORKSPACE_PROJECT_LIMIT_REACHED
WORKSPACE_PROJECT_TRANSFER_CONFLICT

WORKSPACE_SHARED_RESOURCE_CONFLICT
WORKSPACE_EXPORT_FORBIDDEN
WORKSPACE_LEGAL_HOLD_ACTIVE
WORKSPACE_DELETION_NOT_ALLOWED
```

Subsystem-specific errors SHOULD remain owned by their subsystem.

---

# Architecture Invariants

1. Workspace is CRAI's highest-level tenant and administrative boundary.

2. Workspace is a stable Aggregate Root.

3. Workspace ID is independent from name, slug, owner, provider and billing identity.

4. Workspace is separate from User identity.

5. Workspace is separate from Project.

6. Workspace is separate from authentication-provider tenant identity.

7. Workspace is separate from provider account identity.

8. Workspace is separate from billing-provider customer identity.

9. A Project belongs to exactly one Workspace in MVP.

10. Project transfer is an explicit migration workflow.

11. Workspace governs Project ownership but does not own Project content semantics.

12. Workspace MUST NOT become a super-aggregate containing all tenant resources.

13. High-cardinality independently changing records remain separate aggregates.

14. Normal active Workspace state requires an eligible owner.

15. Ownership transfer MUST NOT leave an active Workspace ownerless.

16. Membership is separate from User identity.

17. Invitation is separate from active Membership.

18. Ownership is distinct from ordinary Membership role semantics.

19. Role is separate from Permission.

20. Workspace Role and Project Role are separate scopes.

21. Workspace membership does not automatically grant access to every Project.

22. Authorization must remain tenant-scoped.

23. Permission is separate from Policy.

24. Permission is separate from Entitlement.

25. Entitlement is separate from Quota.

26. Usage is recorded outside the core Workspace aggregate.

27. Workspace is the logical billing/usage attribution boundary unless a future billing model explicitly defines otherwise.

28. Workspace is the primary tenant-isolation boundary.

29. Every private Workspace-owned resource must be traceable to exactly one Workspace.

30. Workspace-private data MUST NOT leak across tenant boundaries.

31. Workspace scope/availability does not replace resource-domain semantic ownership.

32. Workspace-scoped Glossary resources remain governed by Glossary-domain semantics.

33. Workspace-scoped Profiles remain governed by Profile-domain semantics.

34. Workspace MUST NOT define the universal Glossary precedence algorithm.

35. Workspace MUST NOT define one universal effective-configuration hierarchy for every capability.

36. Workspace contributes defaults and mandatory constraints to configuration resolution.

37. Defaults are separate from mandatory policies.

38. Narrower configuration MAY override defaults where permitted.

39. Narrower configuration MUST NOT override mandatory Workspace policy.

40. Workspace policy revisions are immutable and auditable.

41. Mutable Workspace configuration MUST NOT silently alter already-started durable operations.

42. Workspace inputs affecting durable output MUST cross an immutable snapshot/revision boundary.

43. Translation MUST NOT depend directly on mutable Workspace Glossary state.

44. Translation MUST consume immutable resolved inputs.

45. Dynamic Profile selections MUST resolve to exact immutable Revisions before execution.

46. Language values MUST use canonical Language Value Objects.

47. Provider-specific Language codes remain inside provider adapters.

48. Provider-specific credentials remain outside Workspace aggregate.

49. Raw secrets MUST NOT appear in Workspace events.

50. Workspace MAY authorize provider configuration use without owning provider execution.

51. Workspace policy governs whether external providers may receive content.

52. AI data-use consent is explicit and separate from provider availability.

53. Workspace suspension MUST NOT corrupt existing Project data.

54. Workspace archive is distinct from deletion.

55. Workspace deletion SHOULD be explicit, delayed where possible and auditable.

56. Physical deletion follows resource-specific retention/compliance semantics.

57. Legal Hold MAY block deletion.

58. Workspace Merge and Split are migration workflows.

59. Search projections MUST preserve tenant isolation.

60. Private caches MUST preserve tenant isolation.

61. Cross-Workspace cache reuse requires explicit semantic and privacy safety.

62. Cross-Workspace learning is disabled by default.

63. Workspace itself MUST NOT become an unstructured knowledge dump.

64. Review governance remains separate from Review decision ownership.

65. Collaboration policy remains separate from collaboration transport/runtime.

66. Service Accounts are separate security principals.

67. Service Accounts SHOULD use least privilege.

68. Significant Workspace administrative actions are auditable.

69. Audit, Usage and Telemetry remain distinct.

70. Workspace events MUST NOT carry raw sensitive content unnecessarily.

71. Workspace ID in an event is routing context, not authorization proof.

72. Workspace operations SHOULD use optimistic concurrency.

73. Provisioning and consequential administrative workflows SHOULD be idempotent.

74. Authorization decisions SHOULD be explainable.

75. Session ID MAY correlate Workspace operations but MUST NOT become tenant identity.

---

# Recommended MVP Scope

CRAI MVP SHOULD support:

* stable Workspace ID,
* one Personal Workspace per registered user or equivalent default tenant,
* one owner,
* User principals,
* active Membership,
* basic invitations,
* fixed system roles,
* Workspace and Project permission scopes,
* one Workspace per Project,
* `PERSONAL`,
* `TEAM`,
* optional `LOCAL`,
* lifecycle:

  * `PROVISIONING`,
  * `ACTIVE`,
  * `SUSPENDED`,
  * `ARCHIVED`,
  * `PENDING_DELETION`,
  * `DELETED`,
* tenant isolation,
* basic Workspace metadata,
* locale,
* time zone,
* Project creation,
* Project visibility:

  * `PRIVATE`,
  * `WORKSPACE`,
  * `RESTRICTED`,
* Workspace-scoped Profile availability,
* Workspace-scoped Glossary availability,
* exact shared-resource revision references,
* Workspace defaults,
* mandatory local/cloud processing policy,
* provider allow/deny policy,
* provider configuration references,
* secure credential references,
* basic entitlement checks,
* basic quota checks,
* Workspace usage attribution,
* storage attribution,
* search/cache tenant isolation,
* immutable Policy revisions,
* audit of administrative changes,
* explicit AI data-use policy,
* archive,
* delayed deletion,
* optimistic concurrency,
* idempotent provisioning.

MVP SHOULD defer:

* co-owners,
* custom Workspace roles,
* complex deny inheritance,
* Directory Groups,
* Service Account administration UI,
* public Projects,
* Workspace Merge,
* Workspace Split,
* automated Project transfer,
* customer-managed encryption keys,
* advanced legal hold,
* multi-Workspace billing subscriptions,
* advanced quota reservation,
* approval chains,
* separation-of-duties workflow,
* advanced Workspace templates,
* Workspace-wide Knowledge Base,
* cross-Workspace sharing,
* cross-Workspace learning,
* advanced migration,
* Workspace-level collaboration presence,
* complex external integrations,
* automated policy migration,
* generic policy-rule engine.

---

# Open Decisions

The following SHOULD remain open until implementation/prototype validation:

* whether every registered user automatically receives a Personal Workspace,
* whether Personal Workspace supports collaborators,
* whether `LOCAL` is a distinct Workspace Type,
* whether Workspace slug is globally unique,
* whether owner transfer requires acceptance,
* whether owner transfer requires stronger authentication,
* whether removed Membership may be reactivated,
* whether reactivation preserves Membership ID,
* whether invitations may target unregistered email addresses,
* whether guest Membership expires automatically,
* whether Workspace roles become customizable,
* whether explicit permission deny is supported,
* exact Workspace-to-Project role inheritance,
* whether all members can discover Workspace-visible Projects,
* whether public Projects are ever supported,
* Project transfer support and migration semantics,
* whether Workspace Glossary is required in MVP,
* whether Workspace Character templates/catalogs are needed,
* whether Workspace Profile defaults use exact revisions or dynamic approved selection,
* whether Projects may clone Workspace Profiles,
* whether Projects may clone Workspace Glossaries,
* which Workspace policies are hard constraints,
* policy-change behavior for running operations,
* compliance state of historical outputs after policy changes,
* data-residency enforcement architecture,
* Workspace storage accounting,
* deduplicated-artifact accounting,
* quota consistency model,
* quota reservation requirements,
* budget approval requirements,
* billing ownership beyond MVP,
* personal provider credentials inside Team Workspaces,
* Project-scoped provider credentials,
* deletion recovery period,
* tombstone retention,
* audit retention after Workspace deletion,
* legal-hold support,
* Workspace export contents,
* Workspace import membership behavior,
* offline Local Workspace synchronization,
* Local-to-cloud Workspace identity migration,
* device-only credential migration,
* shared Session access,
* collaboration infrastructure,
* notification infrastructure,
* Workspace template domain,
* Workspace Knowledge Base domain,
* policy engine architecture,
* approval workflow architecture.

---

# Ownership Summary

```text
Workspace owns
    stable tenant identity
    administrative metadata
    lifecycle
    owner reference
    administrative settings
    active policy reference
    tenant boundary

Workspace governs
    Membership boundary
    Project ownership
    authorization scope
    shared-resource availability
    defaults
    mandatory policies
    provider availability
    privacy
    data residency
    retention requirements
    usage attribution
    billing attribution
    audit scope
    collaboration rules

Workspace references
    Projects
    Memberships
    Roles
    Policy Sets
    Profiles
    Glossaries
    Provider Configurations
    Integrations
    Subscription
    Entitlements
    Quotas
    Legal Holds

Workspace contributes to
    AuthorizationContext
    PolicyDecision
    ResolvedConfigurationSnapshot
    GlossarySnapshot resolution
    Profile resolution
    operation routing constraints

Workspace does not own
    User identity
    authentication
    Project content semantics
    Translation truth
    OCR truth
    Glossary semantics
    Character truth
    Profile semantics
    Session working state
    Review decisions
    provider execution
    raw credentials
    billing-provider state
    Usage Ledger
    Audit Ledger
    telemetry
```

Workspace is therefore CRAI's durable **tenant, governance and collaboration boundary**, not the semantic owner of every resource that exists inside that tenant.

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
* `PROFILE.md`
* `SESSION.md`

Architecture:

* `docs/architecture/CAPABILITY_MAP.md`
* `docs/architecture/OWNERSHIP_MAP.md`
* `docs/architecture/DATA_FLOW.md`
* `docs/architecture/STATE_MACHINE.md`
* `docs/architecture/EVENT_BUS.md`
* `docs/architecture/MODULE_DEPENDENCY.md`

Infrastructure and module contracts remain authoritative for authentication, authorization execution, provider execution, billing integration, storage, secrets, audit persistence, telemetry and collaboration transport.
