# Workspace Domain

* **Document:** Domain / Workspace
* **Version:** 1.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

The Workspace domain defines the highest-level ownership, collaboration and policy boundary inside CRAI.

A Workspace groups users, Projects and shared resources under one administrative context.

It may represent:

* One individual user
* One translation team
* One publisher
* One organization
* One research group
* One local CRAI installation
* One temporary collaboration group

Workspace provides shared governance for:

* Membership
* Roles
* Permissions
* Project ownership
* Shared profiles
* Shared glossaries
* Shared terminology
* Provider configuration references
* Usage limits
* Storage limits
* Privacy policies
* Data residency
* Audit
* Retention
* Billing ownership
* Collaboration defaults

Workspace must remain independent from:

* Authentication provider implementation
* Payment provider implementation
* Cloud deployment topology
* AI provider-specific organizations
* Operating-system user accounts
* Browser profiles
* Runtime worker pools

---

# Domain Role

Workspace is the top-level tenant and collaboration boundary.

```text
Workspace
├── Members
├── Roles
├── Policies
├── Projects
├── Shared Resources
├── Usage
├── Quotas
├── Audit
└── Integrations
```

Typical ownership hierarchy:

```text
Workspace
    │
    ├── Project
    │    ├── Book
    │    ├── Chapter
    │    ├── Page
    │    ├── TextBlock
    │    ├── Translation
    │    ├── Character
    │    └── Project Glossary
    │
    ├── Workspace Glossary
    ├── Shared Profiles
    ├── Members
    └── Policies
```

Workspace governs access and defaults.

Project remains the primary business boundary for one translation or reading collection.

---

# Workspace Is Not Project

Workspace and Project serve different purposes.

```text
Workspace
    = ownership, collaboration and policy boundary

Project
    = content and translation business boundary
```

A Workspace may own many Projects.

A Project normally belongs to one Workspace.

Workspace-level configuration may provide defaults, but Project configuration may specialize those defaults where policy permits.

---

# Workspace Is Not User

A Workspace may contain:

* One user
* Several users
* Service identities
* External collaborators
* Automated agents

The Workspace identity must remain stable independently from any individual member.

Deleting or removing one member must not automatically delete the Workspace.

---

# Workspace Is Not Authentication Organization

An external authentication system may expose:

* Organization
* Tenant
* Directory
* Group
* Team

Those external identities may map to a CRAI Workspace, but they are not the canonical Workspace identity.

```text
CRAI Workspace ID
≠
Authentication Provider Organization ID
```

Workspace must remain portable across authentication providers.

---

# Workspace Is Not Provider Account

AI, OCR or storage providers may have their own account or organization identifiers.

These must remain infrastructure references.

```text
Workspace
    │
    └── Provider Configuration Reference
             └── Provider Account / Organization
```

A Workspace may use several providers.

One provider account may potentially serve several Workspaces under explicit isolation policy.

---

# Aggregate Boundary

Workspace should be modeled as an Aggregate Root.

```text
Workspace Aggregate
├── Workspace
├── Workspace Metadata
├── Workspace Lifecycle
├── Workspace Ownership
├── Workspace Settings
├── Policy References
└── Aggregate Version
```

High-cardinality or independently changing concepts should remain separate aggregates:

* Membership
* Role
* Invitation
* Project
* Subscription
* Usage Ledger
* API Credential
* Audit Record
* Shared Glossary
* Shared Profile
* Provider Configuration

This prevents the Workspace aggregate from becoming excessively large.

---

# Workspace Responsibilities

The Workspace domain is responsible for:

* Maintaining stable Workspace identity
* Defining Workspace ownership
* Defining membership boundaries
* Referencing roles and permissions
* Owning Projects
* Managing Workspace lifecycle
* Providing shared configuration defaults
* Defining policy boundaries
* Defining resource limits
* Defining data visibility
* Defining collaboration rules
* Supporting invitations
* Supporting member removal
* Supporting ownership transfer
* Supporting Workspace suspension and archival
* Emitting Workspace-related events
* Maintaining tenant isolation
* Providing audit scope

The Workspace domain is not responsible for:

* Authenticating passwords
* Issuing access tokens
* Processing payments directly
* Executing OCR or Translation
* Managing provider API calls
* Storing Project content directly
* Performing permission checks inside every domain aggregate
* Managing operating-system files
* Maintaining live user presence
* Implementing collaboration transport

---

# Workspace Identity

Each Workspace has a stable identifier.

```text
Workspace ID
```

Workspace identity persists through:

* Name changes
* Ownership transfer
* Member changes
* Subscription changes
* Provider changes
* Project creation and deletion
* Migration between deployment environments
* Domain name changes
* Authentication-provider changes

Workspace ID must never be derived only from:

* Workspace name
* Owner email
* Company domain
* Provider organization ID
* Billing customer ID

---

# Workspace Record

Recommended conceptual structure:

```text
Workspace
├── Workspace ID
├── Workspace Type
├── Display Name
├── Slug
├── Owner Reference
├── Lifecycle State
├── Default Locale
├── Default Time Zone
├── Policy Set Reference
├── Subscription Reference
├── Created At
├── Updated At
└── Aggregate Version
```

Sensitive credentials must not be embedded in the Workspace aggregate.

---

# Workspace Types

Recommended Workspace Types:

* Personal
* Team
* Organization
* Publisher
* Local
* Temporary
* Educational
* Custom

Workspace Type may influence defaults.

It must not hard-code authorization rules.

Examples:

## Personal

Designed for one primary user.

May still support invited collaborators later.

## Team

Designed for a small translation or reading group.

## Organization

Designed for centrally managed users, policies and Projects.

## Publisher

May require stricter terminology, review and release controls.

## Local

Represents an offline or self-hosted installation without cloud collaboration.

## Temporary

May have limited retention or expiration policies.

---

# Personal Workspace

Every registered user may receive one default Personal Workspace.

A Personal Workspace:

* Has one initial owner
* May own private Projects
* May support limited invitations
* May use personal provider credentials
* May later be upgraded to a Team Workspace

The user account and Personal Workspace remain different identities.

A user may belong to several Workspaces.

---

# Local Workspace

A local-only installation may create a Workspace without a cloud account.

Possible characteristics:

* Device-local identity
* No remote members
* No server-side billing
* Local provider configuration
* Local audit records
* Optional later synchronization

Local Workspace IDs must remain globally unique enough to support later import or synchronization.

---

# Workspace Metadata

Workspace metadata may include:

* Display name
* Description
* Logo reference
* Slug
* Preferred locale
* Time zone
* Website reference
* Contact metadata
* Organization category

Metadata changes normally do not invalidate Project business artifacts.

---

# Workspace Slug

Slug is a human-readable locator.

Example:

```text
moonlight-translations
```

Slug must not be treated as stable identity.

Slug may change.

Historical links may require redirects.

Uniqueness may be:

* Global
* Deployment-specific
* Scoped by region

Workspace ID remains canonical.

---

# Workspace Ownership

Workspace ownership defines ultimate administrative authority.

Recommended model:

```text
Workspace Ownership
├── Workspace ID
├── Owner Principal ID
├── Ownership Type
├── Assigned At
├── Assigned By
└── Transfer State
```

Possible owner principals:

* User
* Organization identity
* System identity
* Legal entity reference

Recommended MVP:

```text
One User Owner per Workspace
```

---

# Owner Responsibilities

The Workspace Owner may normally:

* Update Workspace metadata
* Manage members
* Assign administrative roles
* Create and archive Projects
* Manage policies
* Manage shared provider configuration
* View Workspace usage
* Manage subscription
* Export Workspace data
* Transfer ownership
* Delete or archive the Workspace

Exact permissions should be expressed through authorization policy rather than hard-coded owner checks everywhere.

---

# Ownership Transfer

Ownership transfer must be explicit and auditable.

Recommended flow:

```text
Requested
→ Pending Acceptance
→ Accepted
→ Completed
```

Alternative outcomes:

```text
Cancelled
Expired
Rejected
Failed
```

Transfer should validate:

* Current owner authority
* Target member eligibility
* Target account status
* Subscription constraints
* Legal or organizational policy
* Required confirmations

---

# Ownership Transfer Invariant

A Workspace must not become ownerless during a transfer.

Recommended sequence:

1. Validate target
2. Record pending transfer
3. Obtain required acceptance
4. Assign new owner
5. Downgrade or retain previous owner role
6. Emit transfer event
7. Preserve audit history

---

# Membership

Workspace Membership links a principal to a Workspace.

Membership should normally be a separate Aggregate Root.

```text
Workspace Membership
├── Membership ID
├── Workspace ID
├── Principal ID
├── Membership Type
├── Status
├── Role Assignments
├── Joined At
├── Last Updated At
└── Version
```

Possible principal types:

* User
* Service Account
* External Collaborator
* Automation Agent
* Directory Group

The MVP may support User principals only.

---

# Membership Identity

Membership has its own stable identity.

```text
Membership ID
```

This allows CRAI to preserve:

* Join history
* Role history
* Suspension history
* Removal history
* Re-invitation history

A user removed and later re-added may receive:

* A new Membership ID
* Or a reactivated historical Membership

The policy must be explicit.

---

# Membership Status

Recommended statuses:

* Invited
* Pending
* Active
* Suspended
* Removed
* Expired
* Rejected

Membership Status is separate from User account status.

An active user account may have a suspended Workspace Membership.

---

# Membership Types

Possible Membership Types:

* Owner
* Internal
* External
* Guest
* Service
* Automation
* Directory Managed

Ownership should preferably remain separate from Membership Type when a richer authorization model is used.

---

# Member

A Member is the effective view of:

```text
Principal
+
Active Workspace Membership
```

Workspace does not own the User identity.

It owns the relationship between that identity and the Workspace.

---

# Invitation

Workspace Invitation should be modeled separately from active Membership.

```text
Workspace Invitation
├── Invitation ID
├── Workspace ID
├── Invitee Reference
├── Intended Roles
├── Intended Project Scope
├── Invited By
├── Created At
├── Expires At
├── Status
└── Token Reference
```

Invitation tokens belong to security infrastructure and should be stored securely.

---

# Invitation Status

Recommended states:

```text
Created
→ Sent
→ Accepted
```

Alternative terminal states:

* Rejected
* Revoked
* Expired
* Failed

An accepted invitation creates or activates a Membership.

---

# Invitee Reference

An invitation may target:

* Existing User ID
* Email address hash or secure reference
* Organization directory identity
* Shareable invite link

Plain sensitive invitation details should be minimized in events and logs.

---

# Role

Role groups permission assignments.

Recommended role examples:

* Owner
* Administrator
* Project Manager
* Translator
* Reviewer
* Terminologist
* Character Editor
* Reader
* Guest
* Billing Manager
* Integration Manager

Roles may be:

* System-defined
* Workspace-defined
* Project-specific

---

# Workspace Role and Project Role

Workspace Role applies across Workspace-level capabilities.

Project Role applies within one Project.

```text
Workspace Administrator
```

does not necessarily imply:

```text
Project Translator
```

though policy may grant such inheritance.

Recommended permission resolution:

```text
Explicit Deny
      ↓
Project-Specific Grant
      ↓
Workspace Role Grant
      ↓
Inherited Default
      ↓
No Access
```

The final order depends on the authorization model, but must remain deterministic.

---

# Role Assignment

Recommended structure:

```text
Role Assignment
├── Assignment ID
├── Workspace ID
├── Membership ID
├── Role ID
├── Scope
├── Assigned By
├── Assigned At
├── Expires At
└── Status
```

Scope may be:

* Workspace
* Project
* Book
* Capability
* Resource group

MVP should prioritize Workspace and Project scopes.

---

# Permission

Permission represents one authorized capability.

Examples:

* `workspace.view`
* `workspace.manage`
* `workspace.members.invite`
* `workspace.members.remove`
* `workspace.roles.assign`
* `project.create`
* `project.view`
* `project.translate`
* `project.review`
* `glossary.manage`
* `character.manage`
* `provider.configure`
* `usage.view`
* `billing.manage`
* `audit.view`
* `workspace.export`
* `workspace.delete`

Permissions should be stable identifiers.

Display labels may be localized separately.

---

# Authorization Boundary

Workspace defines the tenant boundary used by authorization.

Every Workspace-owned resource should be attributable to exactly one Workspace, directly or transitively.

Example:

```text
Translation
→ TextBlock
→ Page
→ Chapter
→ Book
→ Project
→ Workspace
```

Authorization queries may use projections for efficiency, but canonical ownership must remain traceable.

---

# Tenant Isolation

Workspace is the primary tenant isolation boundary.

Requirements:

* Workspace data must not leak across tenants.
* Cache entries must preserve tenant isolation where data is private.
* Search indexes must be tenant-scoped.
* Provider requests must not include another Workspace’s context.
* Background jobs must carry Workspace identity.
* Events must include safe Workspace routing metadata.
* Object storage paths must be tenant-aware.
* Logs must avoid exposing cross-tenant data.
* Usage must be attributed to the correct Workspace.

---

# Workspace Context

Application operations should carry Workspace Context.

Recommended structure:

```text
Workspace Context
├── Workspace ID
├── Principal ID
├── Membership ID
├── Effective Roles
├── Effective Permissions
├── Policy Set Revision
├── Correlation ID
└── Authentication Context Reference
```

Workspace Context is an application/security concept.

It must not be persisted as canonical business truth inside every domain object.

---

# Workspace and Project Ownership

A Project normally belongs to one Workspace.

```text
Project
├── Project ID
└── Workspace ID
```

Project transfer between Workspaces should be an explicit migration operation.

Changing `Workspace ID` directly is insufficient because related resources may include:

* Project members
* Shared glossary references
* Shared profiles
* Provider policies
* Storage objects
* Usage history
* Audit history
* Encryption keys
* External integrations

---

# Project Creation

Workspace policy may control:

* Who can create Projects
* Maximum number of Projects
* Allowed Project Types
* Default visibility
* Default languages
* Default Profiles
* Default Glossary sources
* Default retention
* Default provider routing
* Default storage region

Project creation should capture the Workspace policy revision or resulting settings where reproducibility matters.

---

# Project Visibility

Recommended Project visibility values:

* Private
* Workspace
* Restricted
* Shared Link
* Public

## Private

Visible only to explicitly authorized members.

## Workspace

Visible to all Workspace members with appropriate general permissions.

## Restricted

Visible only to assigned Project members or roles.

## Shared Link

Accessible through a controlled share mechanism.

## Public

Publicly discoverable or readable, if supported.

Visibility does not replace permission evaluation.

---

# Project Transfer

Project transfer may occur between Workspaces.

Recommended lifecycle:

```text
Requested
→ Validating
→ Pending Acceptance
→ Migrating
→ Completed
```

Possible failures:

* Unsupported shared resource dependency
* Storage region conflict
* Policy incompatibility
* Subscription limit exceeded
* Missing target permissions
* Encryption incompatibility
* Active processing conflict

---

# Transfer Dependencies

Project transfer should inspect:

* Workspace Glossary references
* Workspace Profile references
* Character references
* Shared attachments
* Provider configuration dependencies
* Access policies
* Project memberships
* Storage ownership
* Usage attribution
* Scheduled operations
* External connector references

Dependencies may need to be:

* Copied
* Rebound
* Detached
* Rejected
* Manually resolved

---

# Workspace Shared Resources

A Workspace may own resources shared across Projects.

Possible shared resources:

* Glossaries
* Translation Profiles
* OCR Profiles
* Presentation Profiles
* Character templates
* Prompt templates
* Validation policies
* Font policies
* Provider routing policies
* Import mappings
* Export templates
* Knowledge references

Shared resources must use immutable revisions when consumed by durable outputs.

---

# Shared Resource Ownership

Recommended structure:

```text
Shared Resource
├── Resource ID
├── Workspace ID
├── Resource Type
├── Active Revision
├── Visibility
├── Review State
├── Created By
└── Created At
```

Specific resource domains remain responsible for their own semantic rules.

Workspace only establishes ownership and availability.

---

# Workspace Glossary

Workspace Glossary provides terminology reusable across Projects.

Recommended precedence:

```text
Operation Glossary Override
        ↓
Session Glossary Override
        ↓
Page / Chapter / Book Glossary
        ↓
Project Glossary
        ↓
Workspace Glossary
        ↓
User Glossary
        ↓
Global Glossary
```

More specific scope should normally take precedence.

Translation still consumes an immutable Glossary Snapshot.

It must not depend directly on mutable Workspace Glossary state.

---

# Shared Profile

Workspace may provide shared:

* Translation Profiles
* OCR Profiles
* Presentation Profiles
* Validation Profiles
* Routing Profiles

Projects may:

* Reference exact shared revisions
* Clone them into Project scope
* Override allowed fields
* Pin a revision
* Follow the latest approved revision

For reproducibility, operations must resolve an exact Profile Revision.

---

# Inheritance Policy

Workspace defaults may be inherited by Projects.

Possible inheritance modes:

* Fixed
* Default
* Overridable
* Required
* Prohibited
* Revision Pinned
* Track Latest Approved

Example:

```text
Workspace Local-Only Policy:
Required
```

Project cannot enable cloud providers.

Example:

```text
Workspace Target Language:
Default vi
```

Project may override it.

---

# Configuration Resolution

Recommended hierarchy:

```text
Operation Configuration
        ↓
Session Configuration
        ↓
Book or Chapter Configuration
        ↓
Project Configuration
        ↓
Workspace Configuration
        ↓
User Preference
        ↓
Application Default
```

Policy constraints are evaluated separately.

A lower scope may override a default but cannot override a mandatory higher-level policy.

---

# Default and Policy Separation

Workspace Default and Workspace Policy are different.

```text
Default
= value used when no narrower choice exists

Policy
= constraint on which values are allowed
```

Example:

```text
Default provider:
Provider A
```

Project may select Provider B.

But:

```text
Policy:
Cloud providers forbidden
```

cannot be overridden by the Project.

---

# Workspace Policy Set

Workspace should reference a versioned Policy Set.

```text
Workspace Policy Set
├── Policy Set ID
├── Workspace ID
├── Policy Revision ID
├── Status
├── Effective From
├── Created By
└── Approved By
```

Policy changes should create new immutable revisions.

---

# Policy Categories

Recommended categories:

* Access
* Collaboration
* Privacy
* Provider Usage
* Data Residency
* Retention
* Export
* Import
* Content Safety
* Cost Control
* Storage
* Processing
* Audit
* Sharing
* Backup
* Encryption
* AI Training Consent
* External Integration

---

# Access Policy

Access policy may define:

* Allowed membership types
* Guest access
* Maximum invitation duration
* Required account verification
* Required multi-factor authentication
* Project visibility defaults
* Public sharing availability
* Session timeout expectations
* Service account usage

Authentication enforcement belongs to security infrastructure.

Workspace policy declares the requirement.

---

# Privacy Policy

Workspace privacy policy may define:

* Local-only processing
* Cloud processing allowance
* Provider allowlist
* Provider denylist
* Content logging restrictions
* Prompt retention restrictions
* Analytics allowance
* Telemetry allowance
* Human review allowance
* External sharing allowance
* Data export requirements

---

# Provider Policy

Provider policy may define:

* Allowed provider types
* Allowed provider accounts
* Allowed regions
* Local-only requirements
* Maximum cost tier
* Sensitive-content restrictions
* Prompt logging restrictions
* Model allowlist
* Model denylist
* Fallback rules

Provider-specific credentials remain outside the Workspace aggregate.

---

# Data Residency

Workspace may define required storage and processing regions.

Recommended representation:

```text
Data Residency Policy
├── Allowed Storage Regions
├── Allowed Processing Regions
├── Cross-Region Transfer Policy
├── Backup Regions
└── Effective Revision
```

Region identifiers should be canonical and provider-neutral.

---

# Retention Policy

Workspace retention policy may define duration for:

* Projects
* Source images
* OCR raw results
* Provider responses
* Translation revisions
* Session data
* Audit logs
* Temporary files
* Deleted-resource tombstones
* Recognition embeddings
* Export packages
* Backups

Retention does not necessarily imply physical deletion immediately.

Legal hold and audit requirements may override normal expiry.

---

# Export Policy

Export policy may define:

* Who can export
* Which formats are allowed
* Whether raw source images may be exported
* Whether provider metadata may be exported
* Whether audit information must be included
* Whether watermarking is required
* Whether spoiler filtering is required
* Whether approval is required
* Maximum export size

---

# Sharing Policy

Sharing policy may control:

* Public links
* Link expiration
* Password protection
* Download permission
* Copy permission
* Comment permission
* Anonymous access
* Search-engine indexing
* Project visibility
* Shared Session access

Sharing tokens belong to security infrastructure.

---

# AI Training Consent

Workspace policy should explicitly represent whether Workspace data may be used for:

* Provider training
* Internal model improvement
* Embedding generation
* Evaluation
* Human review
* Quality analysis

Default should be conservative.

Consent must not be inferred from general provider availability.

---

# Content Classification

Workspace may define content classification levels:

* Public
* Internal
* Confidential
* Restricted
* Licensed
* Personal
* Unreleased

Projects or individual resources may receive classifications.

Policy may restrict:

* Providers
* Export
* Sharing
* Logging
* Retention
* Human review
* External integrations

---

# Workspace Settings

Workspace Settings are mutable administrative preferences.

Examples:

* Default locale
* Time zone
* Default target language
* Default Project visibility
* Default Translation Profile
* Default Presentation Profile
* Notification preferences
* Invitation settings
* Review requirements
* Naming conventions

Settings should not be confused with mandatory policies.

---

# Language Defaults

Workspace may define default:

* Source languages
* Target languages
* OCR languages
* Transliteration policies
* Interface locale

Language values must use canonical Language Value Objects.

Provider language codes remain inside provider adapters.

---

# Workspace Locale

Workspace Locale may affect:

* Administrative interface
* Notification templates
* Date formatting
* Number formatting
* Default document language

It must not automatically become:

* Project source language
* Translation target language
* User interface locale for every member

User preferences may override Workspace Locale where allowed.

---

# Workspace Time Zone

Workspace Time Zone supports:

* Audit display
* Scheduled processing
* Retention deadlines
* Billing periods
* Notification windows
* Reports

Canonical timestamps should remain stored in UTC.

Time zone should use an IANA identifier.

Example:

```text
Asia/Bangkok
```

---

# Shared Provider Configuration

Workspace may reference one or more provider configurations.

Recommended conceptual structure:

```text
Provider Configuration
├── Provider Configuration ID
├── Workspace ID
├── Provider Type
├── Capability Types
├── Credential Reference
├── Region
├── Policy Tags
├── Status
└── Version
```

Credentials must be stored in a secure secret store.

---

# Provider Configuration Status

Recommended states:

* Draft
* Validating
* Active
* Disabled
* Invalid
* Expired
* Revoked
* Archived

Workspace policy determines which Projects may use each configuration.

---

# Credential Boundary

Workspace may own authorization to use a credential reference.

Workspace must not contain:

* Raw API key
* OAuth refresh token
* Password
* Private key
* Provider session cookie

Recommended relationship:

```text
Workspace
    │
    └── Provider Configuration
             │
             └── Secret Reference
                      │
                      └── Secure Secret Store
```

---

# Personal and Shared Credentials

Possible credential ownership:

* User-owned
* Workspace-owned
* System-managed
* Bring-your-own-key
* Local-device-only

Policy must define whether:

* Other members may use the credential
* Usage is visible to administrators
* Costs are charged to Workspace
* Credentials can be used outside selected Projects

---

# Billing Boundary

Workspace is the recommended billing ownership boundary.

Billing may cover:

* Subscription
* Seats
* Storage
* OCR usage
* Translation tokens
* Model execution
* Export bandwidth
* Retention tier
* Collaboration features

Billing provider entities remain infrastructure references.

---

# Subscription

Subscription should be a separate aggregate or billing-domain entity.

Recommended structure:

```text
Workspace Subscription
├── Subscription ID
├── Workspace ID
├── Plan Reference
├── Status
├── Billing Customer Reference
├── Current Period
├── Seat Limit
├── Feature Entitlements
└── Usage Policy Reference
```

The Workspace should not store payment card data.

---

# Subscription Status

Possible states:

* Trial
* Active
* Past Due
* Suspended
* Cancelled
* Expired
* Free
* Internal
* Local License

Subscription state may affect capabilities but should not corrupt existing domain data.

---

# Entitlement

Entitlement represents an available feature or limit.

Examples:

* Maximum Projects
* Maximum members
* Cloud Translation
* Local model support
* Character recognition
* Collaboration
* Export formats
* Audit retention
* Advanced glossary
* Batch processing

Entitlements should be evaluated separately from user permissions.

```text
Permission
= user may perform action

Entitlement
= Workspace plan supports action
```

Both may be required.

---

# Quota

Quota defines a resource limit.

Recommended quota categories:

* Projects
* Members
* Storage bytes
* Source images
* OCR pages
* Translation characters
* Translation tokens
* Provider cost
* Concurrent jobs
* Requests per time window
* Export size
* Audit retention
* Recognition embeddings

---

# Quota Definition

Recommended structure:

```text
Workspace Quota
├── Quota ID
├── Workspace ID
├── Resource Type
├── Limit
├── Period
├── Enforcement Mode
├── Warning Thresholds
├── Effective From
└── Source
```

Quota source may be:

* Subscription
* Administrator override
* Trial
* Promotion
* Local configuration
* System policy

---

# Quota Enforcement

Possible enforcement modes:

* Informational
* Warning
* Soft Limit
* Hard Limit
* Approval Required
* Throttled

Examples:

* Storage may use a hard limit.
* Monthly Translation cost may require approval.
* Concurrent processing may be throttled.
* Project count may block new Project creation.

---

# Usage

Workspace usage should be recorded in a separate Usage domain or ledger.

Examples:

* OCR pages processed
* Translation characters
* Translation tokens
* Storage consumed
* Provider cost
* Processing time
* Exports
* Active seats

Workspace may expose usage summaries but should not own the entire event ledger inside its aggregate.

---

# Usage Attribution

Every billable or quota-relevant operation should include:

* Workspace ID
* Project ID where applicable
* User or service principal
* Capability
* Provider
* Model or engine
* Quantity
* Unit
* Cost estimate
* Final cost
* Timestamp
* Correlation ID

Session ID may be included for correlation but is not the billing owner.

---

# Usage Reservation

Expensive operations may reserve quota before execution.

```text
Requested
→ Reserved
→ Consumed
```

Alternative outcomes:

* Released
* Expired
* Adjusted
* Rejected

Reservation prevents concurrent operations from exceeding limits.

---

# Cost Policy

Workspace cost policy may define:

* Daily limit
* Monthly limit
* Per-operation limit
* Approval threshold
* Provider-specific limits
* User-specific limits
* Project-specific budgets
* Warning thresholds
* Fallback to local model
* Fallback to cheaper provider

Cost policy should remain provider-neutral.

---

# Project Budget

A Workspace may assign budgets to Projects.

```text
Workspace Budget
    │
    ├── Project A Budget
    ├── Project B Budget
    └── Shared Reserve
```

Project budget usage remains attributed to the Workspace billing boundary.

---

# Storage Ownership

Workspace is the recommended logical owner of stored data.

Physical storage may be distributed across:

* Local disk
* Object storage
* Database
* Search index
* Vector store
* Backup storage
* Provider file service

Every stored object should be traceable to a Workspace.

---

# Storage Namespace

Recommended logical path:

```text
workspace/{workspace-id}/project/{project-id}/...
```

Physical path should not be exposed as canonical business identity.

Storage namespace must not depend only on mutable Workspace slug.

---

# Storage Quota

Storage usage may include:

* Original files
* Captured images
* Processed images
* OCR artifacts
* Translation exports
* Reference images
* Cached models
* Backups
* Session recovery data

Derived caches may have separate eviction policies.

---

# Encryption

Workspace security configuration may define:

* Platform-managed encryption
* Workspace-managed key
* Customer-managed key
* Local-only encryption
* Key rotation policy

Encryption keys belong to security infrastructure.

Workspace references key policy and key identifiers, not raw key material.

---

# Key Rotation

Workspace key rotation must preserve access to historical encrypted artifacts.

Possible flow:

```text
Active Key Version N
→ New Key Version N+1
→ New writes use N+1
→ Historical objects migrate or retain key reference
→ Old key retired after validation
```

---

# Shared Search

Workspace-level search may discover:

* Projects
* Books
* Characters
* Glossary Entries
* Translations
* Review items
* Shared Profiles

Search results must respect:

* Workspace membership
* Project visibility
* Resource permissions
* Spoiler restrictions
* Content classification

Search indexes are derived projections.

---

# Workspace Knowledge

A Workspace may eventually contain reusable knowledge:

* Shared terminology
* Translation conventions
* Character naming conventions
* Style guides
* Historical context
* Publisher rules

This knowledge should remain in explicit domains such as:

* Glossary
* Profile
* Policy
* Knowledge Base
* Character Template

Workspace itself should not become an unstructured knowledge dump.

---

# Template

Workspace may own templates for:

* Project creation
* Translation Profile
* Glossary structure
* Character metadata
* Review workflow
* Export layout
* Policy configuration

Templates should be versioned.

Creating a Project from a template should record the template revision used.

---

# Workspace Project Template

Recommended structure:

```text
Project Template
├── Template ID
├── Workspace ID
├── Template Revision
├── Project Type
├── Default Languages
├── Default Profiles
├── Default Policies
├── Default Folder Structure
├── Default Glossary References
└── Review Requirements
```

A Project created from a template becomes independent unless configured to track selected shared revisions.

---

# Review Governance

Workspace may define review requirements.

Examples:

* Translation must be approved before publication
* Locked terminology changes require Terminologist role
* Character identity changes require Reviewer role
* Project export requires Project Manager approval
* Public sharing requires Administrator approval

Governance policies should produce explicit workflow requirements.

---

# Approval Policy

Recommended structure:

```text
Approval Policy
├── Resource Type
├── Action
├── Required Roles
├── Required Approval Count
├── Separation of Duties
├── Scope
└── Policy Revision
```

Example:

```text
Locked Workspace Glossary Entry modification:
- One Terminologist approval
- Editor cannot self-approve
```

Advanced multi-approval workflow may be deferred beyond MVP.

---

# Separation of Duties

Workspace policy may require different members for:

* Creation
* Review
* Approval
* Publication
* Export
* Billing changes
* Provider credential updates

This is especially relevant for publisher or organization Workspaces.

---

# Collaboration Defaults

Workspace may define defaults for:

* Comment visibility
* Review assignment
* Notification behavior
* Presence sharing
* Change tracking
* Mention rules
* Guest permissions
* Conflict handling

Live collaboration execution belongs to collaboration infrastructure.

---

# Notification Policy

Workspace may configure notifications for:

* Invitations
* Role changes
* Project creation
* Quota warnings
* Provider failures
* Translation completion
* Review assignment
* Export completion
* Security changes
* Ownership transfer

Notification delivery channels belong to infrastructure.

---

# Service Accounts

A Workspace may own service identities for automation.

Potential use cases:

* Automated imports
* Scheduled batch Translation
* CI pipelines
* External integrations
* Backup
* Reporting

Service Account should be a separate security principal.

---

# Service Account Policy

Service accounts should have:

* Explicit roles
* Limited scopes
* Rotatable credentials
* Expiration where possible
* Audit attribution
* No interactive owner capability by default

A service account must not become Workspace owner in the MVP.

---

# External Integration

Workspace may configure integrations with:

* Cloud storage
* Browser extension
* External libraries
* Publishing systems
* Translation management systems
* Source repositories
* Messaging tools
* Identity providers

Integration configuration should be a separate aggregate.

---

# Integration Configuration

Recommended structure:

```text
Integration Configuration
├── Integration ID
├── Workspace ID
├── Integration Type
├── Credential Reference
├── Scope
├── Status
├── Policy Tags
├── Created By
└── Version
```

Integration credentials remain in secure infrastructure.

---

# Integration Scope

An integration may apply to:

* Entire Workspace
* Selected Projects
* Selected Books
* Selected capabilities
* One user
* One service account

Least privilege should be the default.

---

# Workspace Lifecycle

Recommended lifecycle states:

```text
Provisioning
→ Active
→ Suspended
→ Active
→ Archived
```

Possible terminal or transitional states:

* Pending Deletion
* Deleted
* Transfer Pending
* Migration In Progress
* Locked
* Failed Provisioning

---

# Provisioning State

Provisioning may include:

* Creating Workspace record
* Assigning owner
* Creating default roles
* Creating Personal Project defaults
* Creating policy set
* Initializing storage namespace
* Initializing subscription
* Creating audit scope

Provisioning should be idempotent.

---

# Active State

Active Workspace may:

* Accept member activity
* Create Projects
* Execute processing
* Use shared resources
* Consume quota
* Manage integrations

All actions remain subject to permissions, entitlements and policies.

---

# Suspended State

Suspension may occur because of:

* Administrative action
* Security incident
* Billing failure
* Policy violation
* Quota abuse
* Legal requirement
* Owner request

Suspension may restrict:

* New writes
* Processing operations
* Member invitations
* Provider usage
* Exports
* Public sharing

Read access may remain available depending on suspension reason.

---

# Locked State

Locked may represent a security-sensitive restriction.

Examples:

* Suspected compromise
* Ownership dispute
* Encryption issue
* Legal hold
* Migration conflict

Locked is distinct from ordinary billing suspension.

---

# Archived State

Archived Workspace:

* Preserves Projects and history
* Disables ordinary active processing
* Restricts membership changes
* May permit export
* May be restorable according to policy

Archival is preferred over immediate deletion.

---

# Pending Deletion

Workspace deletion should use a delayed state.

```text
Active
→ Pending Deletion
→ Deleted
```

During the pending period:

* New processing is blocked
* Members are notified where required
* Export may remain available
* Legal holds are checked
* Scheduled deletion may be cancelled

---

# Workspace Deletion

Deleting a Workspace is a high-impact operation.

It may affect:

* Projects
* Source files
* Translations
* Glossaries
* Characters
* Sessions
* Provider configurations
* Integrations
* Usage history
* Audit records
* Shared links
* Backups

Deletion must be policy-driven, auditable and preferably delayed.

---

# Deletion Strategies

Possible strategies:

## Soft Delete

Workspace becomes inaccessible but data remains during retention period.

## Archive

Workspace becomes read-only and retained indefinitely or long-term.

## Scheduled Hard Delete

Data is physically deleted after a defined period.

## Anonymized Retention

Selected billing or audit metadata remains without content.

## Legal Hold

Deletion is blocked until hold release.

---

# Cascade Rules

Workspace deletion may logically cascade to Workspace-owned resources.

However, physical deletion may occur asynchronously and according to each resource’s retention policy.

The domain must distinguish:

```text
Workspace inaccessible
```

from:

```text
All physical data permanently erased
```

---

# Legal Hold

Legal Hold may block deletion or alteration of selected records.

Recommended structure:

```text
Legal Hold
├── Hold ID
├── Workspace ID
├── Scope
├── Reason Reference
├── Effective From
├── Released At
└── Authority
```

Legal Hold is likely a compliance-domain entity rather than part of the Workspace aggregate.

---

# Workspace Restoration

Restoration may be supported from:

* Archived
* Suspended
* Pending Deletion

Restoration must validate:

* Subscription
* Storage availability
* Key availability
* Member access
* Policy compatibility
* External integration validity

Deleted data beyond retention cannot be guaranteed recoverable.

---

# Workspace Merge

Merging Workspaces is complex and should not be treated as a simple aggregate update.

Potential conflicts:

* Duplicate members
* Role conflicts
* Project slug conflicts
* Shared glossary conflicts
* Profile conflicts
* Provider credentials
* Billing ownership
* Storage regions
* Encryption keys
* Policy incompatibilities
* Audit histories

Workspace Merge should be modeled as a migration workflow.

---

# Workspace Split

Workspace Split may move selected Projects and members into a new Workspace.

The operation should explicitly define:

* Projects to move
* Shared resources to copy
* Members to invite
* Provider configurations to detach
* Usage history boundaries
* Billing start point
* Storage migration
* Encryption changes
* Audit lineage

---

# Workspace Migration

Migration may move Workspace data between:

* Cloud regions
* Deployment environments
* Self-hosted and cloud
* Storage providers
* Database clusters
* Organization accounts

Canonical Workspace identity should remain stable where possible.

Infrastructure resource identifiers may change.

---

# Importing a Workspace

Workspace import may restore:

* Metadata
* Projects
* Shared Profiles
* Shared Glossaries
* Membership mappings
* Policy revisions
* Character data
* Translation history
* Audit references

Security-sensitive records such as credentials should normally require reconfiguration.

---

# Exporting a Workspace

Workspace export may support:

* Full backup
* Administrative archive
* Project-only export
* Shared-resource export
* Audit export
* Billing usage export
* User data portability

Workspace export must respect:

* Content permissions
* Encryption
* Classification
* Provider licensing
* Legal hold
* Spoiler policy
* Export policy

---

# Backup

Backup is infrastructure behavior governed by Workspace policy.

Workspace may define:

* Backup enabled
* Frequency
* Retention
* Region
* Encryption requirement
* Recovery point objective
* Recovery time objective

Backup identifiers and storage locations remain infrastructure details.

---

# Audit Boundary

Workspace is the primary audit scope.

Audit records may cover:

* Membership
* Roles
* Projects
* Policies
* Provider configuration
* Billing
* Quotas
* Exports
* Security
* Deletion
* Ownership transfer

Audit should support filtering by:

* Workspace
* Project
* Principal
* Action
* Resource
* Time
* Correlation ID

---

# Audit Visibility

Not every member may view all audit records.

Possible audit permissions:

* View own activity
* View Project activity
* View Workspace administrative activity
* View security activity
* View billing activity
* Export audit logs

Sensitive credential metadata should be masked.

---

# Workspace Activity and Telemetry

Workspace activity records meaningful administrative actions.

Telemetry records operational measurements.

Examples:

```text
WorkspaceMemberRemoved
```

is a domain event.

```text
Translation latency 930 ms
```

is telemetry.

Usage records and telemetry should not be confused with domain audit events.

---

# Event Boundary

Workspace domain events should describe meaningful changes.

They should not carry:

* Raw credentials
* Full Project content
* Provider prompts
* Private source text
* Payment card data
* Invitation secret tokens

---

# Events

Typical Workspace events include:

* `WorkspaceProvisioned`
* `WorkspaceCreated`
* `WorkspaceActivated`
* `WorkspaceUpdated`
* `WorkspaceSuspended`
* `WorkspaceResumed`
* `WorkspaceLocked`
* `WorkspaceArchived`
* `WorkspaceDeletionRequested`
* `WorkspaceDeletionCancelled`
* `WorkspaceDeleted`
* `WorkspaceOwnershipTransferRequested`
* `WorkspaceOwnershipTransferred`
* `WorkspaceMemberInvited`
* `WorkspaceMemberJoined`
* `WorkspaceMemberSuspended`
* `WorkspaceMemberRemoved`
* `WorkspaceRoleAssigned`
* `WorkspaceRoleRevoked`
* `WorkspacePolicyRevisionCreated`
* `WorkspacePolicyActivated`
* `WorkspaceQuotaChanged`
* `WorkspaceQuotaThresholdReached`
* `WorkspaceQuotaExceeded`
* `WorkspaceSubscriptionChanged`
* `WorkspaceProviderConfigurationAdded`
* `WorkspaceProviderConfigurationDisabled`
* `WorkspaceSharedResourcePublished`
* `ProjectCreatedInWorkspace`
* `ProjectTransferRequested`
* `ProjectTransferred`
* `WorkspaceExportRequested`
* `WorkspaceExportCompleted`
* `WorkspaceMigrationStarted`
* `WorkspaceMigrationCompleted`

---

# Event Payload Example

```text
WorkspaceMemberRemoved
├── Workspace ID
├── Membership ID
├── Principal ID
├── Removed By
├── Reason Code
├── Effective At
├── Occurred At
├── Correlation ID
└── Causation ID
```

The event should not include unnecessary personal data.

---

# Workspace Provisioning Example

```text
User creates an account
        │
        ▼
Personal Workspace provisioned
        │
        ├── Owner Membership created
        ├── Default roles created
        ├── Default Policy Set created
        ├── Default Profile references assigned
        ├── Storage namespace initialized
        └── WorkspaceCreated emitted
```

The user account and Personal Workspace remain separate objects.

---

# Team Workspace Example

```text
Workspace:
Moonlight Translation Team

Members:
- Sơn: Owner
- Lan: Translator
- Minh: Reviewer
- An: Terminologist

Projects:
- Novel A
- Comic B

Shared Resources:
- Chinese → Vietnamese Glossary
- Natural Vietnamese Profile
- Comic OCR Profile

Policies:
- Cloud processing allowed
- Public sharing disabled
- Locked terms require Terminologist approval
```

---

# Project Access Example

Member:

```text
Lan
```

Workspace Role:

```text
Translator
```

Project-specific access:

```text
Novel A:
Translator

Comic B:
No Access
```

Being a Workspace member does not automatically grant access to every restricted Project.

---

# Shared Glossary Example

Workspace Glossary contains:

```text
师尊 → sư tôn
灵力 → linh lực
```

Project A overrides:

```text
师尊 → sư phụ
```

Resolution:

```text
Project Glossary
        ↓
Workspace Glossary
```

Translation uses an immutable snapshot containing the exact selected Entry Revisions.

---

# Mandatory Privacy Policy Example

Workspace Policy:

```text
Processing Mode:
Local Only

Cloud AI Providers:
Forbidden
```

Project configuration attempts:

```text
Provider:
Cloud Provider A
```

Result:

```text
Rejected by Workspace Policy
```

Project configuration cannot override a mandatory Workspace restriction.

---

# Default Profile Example

Workspace default:

```text
Translation Profile:
Natural Vietnamese, Revision 7
```

Project A has no override:

```text
Resolved:
Natural Vietnamese, Revision 7
```

Project B selects:

```text
Literal Comparison, Revision 3
```

If Workspace policy permits overrides, Project B uses its explicit choice.

---

# Quota Example

Workspace monthly Translation quota:

```text
10,000,000 source characters
```

Current usage:

```text
9,200,000
```

Thresholds:

```text
80% → warning
95% → strong warning
100% → hard block
```

Before a large batch job begins, the application reserves expected quota.

---

# Provider Credential Example

Workspace Administrator configures an API provider.

Stored in Workspace domain:

```text
Provider Configuration ID
Provider Type
Capability
Region
Credential Reference
Status
```

Stored in secure secret infrastructure:

```text
Raw API Key
```

Raw credentials never appear in ordinary Workspace events or exports.

---

# Ownership Transfer Example

```text
Current Owner:
Sơn

Target Owner:
Lan

Flow:
1. Sơn requests transfer
2. Lan accepts
3. System verifies Lan is an active member
4. Owner role transfers
5. Sơn becomes Administrator
6. Audit event is emitted
```

The Workspace never enters an ownerless state.

---

# Member Removal Example

When a member is removed:

* Active Workspace sessions are revoked or restricted.
* New operations are denied.
* Existing domain contributions remain attributed to the original principal.
* Assigned review work may be reassigned.
* Personal provider credentials are detached.
* Shared domain artifacts remain owned by the Workspace.
* Audit records remain preserved.

---

# Project Transfer Example

Project A moves from Workspace X to Workspace Y.

Dependencies:

```text
Project Glossary:
Owned by Project — move

Workspace Glossary References:
Owned by X — copy or detach

Provider Configuration:
Owned by X — reconfigure

Project Members:
Mapped to Y memberships

Storage:
Migrated to Y namespace
```

The transfer is a workflow, not a direct foreign-key update.

---

# Workspace Suspension Example

Reason:

```text
Billing Past Due
```

Possible enforcement:

* Existing Projects remain readable.
* New cloud Translation operations are blocked.
* Local-only processing may continue.
* Exports remain available for a limited period.
* Administrators may update billing.
* Data is not deleted.

Suspension behavior depends on policy and reason.

---

# Workspace Deletion Example

```text
Owner requests deletion
        │
        ▼
Pending Deletion for 30 days
        │
        ├── processing blocked
        ├── members notified
        ├── exports allowed
        ├── legal hold checked
        └── cancellation allowed
        │
        ▼
Deletion workflow
        ├── revoke integrations
        ├── delete or anonymize data
        ├── retain required audit records
        └── create Workspace tombstone
```

---

# Suggested Persistence

Recommended canonical tables or collections:

```text
Workspace
Workspace Setting
Workspace Policy Set
Workspace Policy Revision
Workspace Ownership Transfer
Workspace Membership
Workspace Invitation
Workspace Role
Workspace Permission
Workspace Role Assignment
Workspace Quota
Workspace Subscription Reference
Workspace Shared Resource
Workspace Tombstone
```

Separate supporting domains:

```text
Usage Ledger
Billing Subscription
Provider Configuration
Secret Reference
Audit Record
Integration Configuration
Service Account
Legal Hold
Export Job
Migration Job
```

---

# Suggested Workspace Record

```text
Workspace
├── id
├── type
├── display_name
├── slug
├── owner_principal_id
├── lifecycle_state
├── default_locale
├── default_time_zone
├── active_policy_revision_id
├── subscription_id
├── created_at
├── updated_at
└── version
```

---

# Suggested Membership Record

```text
WorkspaceMembership
├── id
├── workspace_id
├── principal_type
├── principal_id
├── membership_status
├── membership_type
├── joined_at
├── suspended_at
├── removed_at
├── created_at
├── updated_at
└── version
```

---

# Suggested Invitation Record

```text
WorkspaceInvitation
├── id
├── workspace_id
├── invitee_reference
├── intended_role_ids
├── intended_project_ids
├── invited_by
├── status
├── created_at
├── expires_at
├── accepted_at
└── version
```

---

# Suggested Role Assignment Record

```text
WorkspaceRoleAssignment
├── id
├── workspace_id
├── membership_id
├── role_id
├── scope_type
├── scope_id
├── assigned_by
├── assigned_at
├── expires_at
├── status
└── version
```

---

# Suggested Policy Revision Record

```text
WorkspacePolicyRevision
├── id
├── workspace_id
├── parent_revision_id
├── policy_document
├── content_hash
├── review_state
├── effective_from
├── created_by
├── approved_by
└── created_at
```

---

# Suggested Quota Record

```text
WorkspaceQuota
├── id
├── workspace_id
├── resource_type
├── limit_value
├── unit
├── period
├── enforcement_mode
├── warning_thresholds
├── source
├── effective_from
└── version
```

---

# Authorization Evaluation

A typical authorization decision may require:

```text
Authenticated Principal
        +
Active Workspace Membership
        +
Effective Role Assignments
        +
Resource Scope
        +
Workspace Policy
        +
Subscription Entitlement
        +
Resource State
        ↓
Authorization Decision
```

Permission alone may be insufficient when:

* Workspace is suspended
* Feature is not entitled
* Quota is exhausted
* Policy forbids the provider
* Resource is archived
* Content classification restricts access

---

# Policy Evaluation

Policy evaluation should produce explainable results.

Recommended result:

```text
Policy Decision
├── Decision
├── Policy Revision
├── Matched Rules
├── Denial Reasons
├── Required Approvals
├── Allowed Alternatives
└── Evaluated At
```

Possible decisions:

* Allow
* Deny
* Allow with Warning
* Require Approval
* Require Local Processing
* Require Redaction
* Require Stronger Authentication

---

# Workspace Validation

Workspace validation should verify:

* Workspace has a stable identity.
* Workspace has an eligible owner.
* Owner has an active Membership.
* Slug format is valid.
* Policy revision belongs to the Workspace.
* Subscription reference is valid.
* Workspace state allows the requested transition.
* Role assignments reference active roles.
* Membership scopes belong to the Workspace.
* Quotas use recognized units.
* Shared resources belong to the Workspace.
* Project ownership is unambiguous.
* Ownership transfer has a valid target.
* Deletion is not blocked by legal hold.
* Mandatory policies are not bypassed.

---

# Error Conditions

Typical Workspace errors:

* Workspace Not Found
* Workspace Access Denied
* Workspace Suspended
* Workspace Locked
* Workspace Archived
* Workspace Pending Deletion
* Invalid Workspace State Transition
* Workspace Version Conflict
* Workspace Slug Conflict
* Workspace Owner Missing
* Owner Transfer Not Allowed
* Target Owner Ineligible
* Membership Not Found
* Membership Inactive
* Membership Limit Reached
* Invitation Expired
* Invitation Revoked
* Role Assignment Invalid
* Permission Denied
* Policy Denied
* Entitlement Missing
* Quota Exceeded
* Provider Forbidden
* Data Residency Violation
* Project Limit Reached
* Project Transfer Conflict
* Shared Resource Dependency Conflict
* Export Forbidden
* Legal Hold Active
* Workspace Deletion Not Allowed

Errors should expose safe, actionable reason codes.

---

# Concurrency

Workspace administrative updates should use optimistic concurrency.

Possible checks:

* Workspace aggregate version
* Expected active policy revision
* Membership version
* Role assignment version
* Ownership transfer state
* Subscription revision
* Quota revision

Sensitive operations may additionally require:

* Recent authentication
* Multi-factor verification
* Approval
* Distributed lock
* Idempotency key

---

# Idempotency

Idempotency should apply to:

* Workspace provisioning
* Workspace creation
* Member invitation
* Invitation acceptance
* Role assignment
* Project creation
* Policy activation
* Quota update
* Ownership transfer request
* Ownership transfer completion
* Workspace suspension
* Workspace deletion request
* Export request
* Migration request

Possible idempotency inputs:

* Client operation ID
* Workspace ID
* Principal ID
* Target resource
* Expected version
* Request hash

---

# Cache Isolation

Caches containing Workspace-private data must include Workspace scope.

Examples:

* Character search
* Glossary matching
* Translation context retrieval
* Project listing
* Permission decisions
* Policy decisions
* Usage summaries

Reusable content caches may omit Workspace ID only when:

* Inputs are fully content-addressed
* Output contains no tenant-specific data
* Access is checked independently
* Cross-tenant reuse is explicitly safe

---

# Search Isolation

Search documents should carry:

* Workspace ID
* Project ID
* Visibility
* Classification
* Permission scope
* Spoiler scope where relevant

Search results must be filtered before being returned.

---

# Event Routing

Workspace-scoped events should include Workspace ID for routing.

Consumers must still verify access and resource scope.

Events should not assume that possession of an event grants permission to read all referenced resources.

---

# Observability

Operational telemetry should include:

* Workspace ID or privacy-safe tenant key
* Project ID where relevant
* Capability
* Operation type
* Provider
* Model
* Usage quantity
* Error category
* Latency

Logs should avoid raw source content unless explicitly permitted.

---

# Privacy

Workspace data may include:

* Copyrighted source images
* Unreleased publications
* Private translations
* Personal reading history
* Provider credentials
* Billing metadata
* Member identities
* Character references
* Custom glossaries
* Sensitive annotations

Requirements:

* Strong tenant isolation
* Least-privilege access
* Explicit sharing
* Secure credential storage
* Configurable telemetry
* Configurable provider retention
* Data export capability
* Deletion workflow
* Audit of administrative access
* No silent cross-Workspace learning

---

# Cross-Workspace Learning

CRAI must not automatically use private Workspace data to improve another Workspace.

Potential reusable global knowledge must be:

* Public
* Explicitly contributed
* Anonymized under policy
* Licensed appropriately
* Approved for sharing

Workspace terminology, corrections and character data remain private by default.

---

# Architecture Invariants

1. Workspace is the highest-level tenant and collaboration boundary.
2. Workspace is an Aggregate Root with stable identity.
3. Workspace ID is independent of Workspace name and slug.
4. Workspace is separate from User identity.
5. Workspace is separate from Project.
6. Workspace is separate from authentication-provider organizations.
7. Workspace is separate from AI provider accounts.
8. A Project normally belongs to exactly one Workspace.
9. Project transfer is an explicit migration workflow.
10. Workspace governs ownership and policy, not Project content semantics.
11. Workspace must have an eligible owner in normal active states.
12. Ownership transfer must not leave the Workspace ownerless.
13. Membership is separate from User identity.
14. Invitation is separate from active Membership.
15. Role is separate from Permission.
16. Workspace Role and Project Role are separate scopes.
17. Workspace membership does not automatically grant access to every restricted Project.
18. Authorization requires active Membership and applicable permission.
19. Entitlement is separate from Permission.
20. Quota is separate from Entitlement.
21. Usage is recorded outside the Workspace aggregate.
22. Workspace is the primary billing attribution boundary.
23. Workspace is the primary tenant-isolation boundary.
24. Every Workspace-owned resource must be traceable to one Workspace.
25. Workspace-private data must not leak across tenant boundaries.
26. Workspace Shared Resources use exact revisions when consumed by durable outputs.
27. Translation never depends directly on mutable Workspace Glossary state.
28. Translation consumes immutable resolved snapshots.
29. Workspace defaults are separate from mandatory Workspace policies.
30. Narrower configuration may override defaults but not mandatory policies.
31. Policies are versioned and auditable.
32. Provider-specific credentials remain outside the Workspace aggregate.
33. Raw secrets never appear in Workspace events.
34. Billing-provider entities do not become canonical Workspace identity.
35. Storage objects remain attributable to Workspace.
36. Storage paths do not depend solely on mutable Workspace slug.
37. Workspace suspension does not corrupt or delete existing Project data.
38. Workspace deletion is explicit, delayed where possible and auditable.
39. Physical deletion may follow resource-specific retention policies.
40. Workspace archival is distinct from deletion.
41. Legal hold may block deletion.
42. Workspace Merge and Split are migration workflows, not ordinary aggregate edits.
43. Search and cache projections preserve Workspace isolation.
44. Service accounts use explicit, limited roles.
45. Workspace policy controls whether cloud providers may receive content.
46. AI training consent is explicit and separate from provider configuration.
47. Cross-Workspace learning is disabled by default.
48. Significant administrative actions are auditable.
49. High-cardinality memberships and usage records remain outside the core Workspace aggregate.
50. Authorization decisions must be explainable through roles, policies and entitlements.

---

# Open Decisions

The following decisions should remain open until implementation and prototype testing:

* Whether every user automatically receives a Personal Workspace
* Whether a Personal Workspace may have collaborators
* Whether Local Workspace is a separate type
* Whether Workspace Slugs are globally unique
* Whether Workspace metadata uses immutable revisions
* Whether Workspace is event-sourced
* Whether Workspace Ownership is part of the aggregate or a separate entity
* Whether one Workspace may have several co-owners
* Whether owner transfer requires acceptance
* Whether owner transfer requires multi-factor authentication
* Whether former owners remain Administrators
* Whether removed members can be reactivated
* Whether reactivation reuses the same Membership ID
* Whether invitations may target emails not yet registered
* Whether shareable invitation links are allowed
* Whether guest memberships expire automatically
* Whether Workspace roles are customizable in MVP
* Whether permission denies are supported
* Whether role inheritance is supported
* Whether Project permissions inherit from Workspace roles
* Whether all Workspace members can discover all Projects
* Whether Project visibility includes Public in MVP
* Whether Project transfer is supported in MVP
* Whether Workspace transfer across deployment environments preserves identity
* Whether Shared Glossary is supported in MVP
* Whether Shared Character catalogs are required
* Whether Workspace Profiles track latest approved revisions automatically
* Whether Projects may pin Shared Resource revisions
* Whether Projects may clone Shared Resources
* Whether Workspace policy uses a generic rule engine
* Which policies are hard constraints
* Whether policy changes apply retroactively
* Whether existing Sessions are stopped after policy changes
* Whether existing Translations become noncompliant after provider-policy changes
* How data residency is enforced
* Whether customer-managed encryption keys are supported
* Whether Workspace uses one storage namespace or per-Project namespaces
* How storage is calculated for deduplicated artifacts
* Whether cached models count against Workspace storage
* Whether anonymous Local Workspaces have quotas
* Whether usage is real-time or eventually consistent
* How quota reservation works
* Whether quota overage is supported
* Whether budget approval workflows are required
* Whether billing is per Workspace or organization account
* Whether one subscription may cover several Workspaces
* Whether users may bring personal provider credentials into Team Workspaces
* Whether administrators can inspect personal credential usage
* Whether provider credentials may be Project-scoped
* Whether Workspace-level provider credentials are required
* Whether Workspace deletion supports a recovery period
* How long deletion tombstones are retained
* Which audit records survive deletion
* Whether legal hold is supported in MVP
* Whether Workspace export includes provider responses
* Whether Workspace export includes audit history
* Whether Workspace import recreates memberships
* Whether credentials are excluded from all exports
* Whether Workspace Merge is ever supported
* Whether Workspace Split copies shared glossaries
* Whether member activities remain attributed after account deletion
* Whether service accounts are supported in MVP
* Whether external identity-directory synchronization is required
* Whether shared Sessions are Workspace resources
* Whether public sharing is allowed
* Whether audit logs are immutable
* Whether audit viewing requires a dedicated role
* Whether Workspace analytics are enabled by default
* Whether AI training consent is configurable per Project
* Whether Workspace content classification is required
* Whether classification can be overridden at resource level
* Whether policy evaluation results are persisted
* Whether configuration resolution produces immutable snapshots
* How Workspace state affects local-only processing
* Which operations remain available during billing suspension
* Whether archived Workspaces are billable
* Whether Project count includes archived Projects
* Whether Workspace quota is shared across all Projects
* Whether project-specific budgets are required
* How multi-region backup interacts with residency policy
* Whether Local Workspace can later convert into Team Workspace
* How Workspace identity is synchronized between local and cloud installations

---

# Recommended MVP Scope

The first CRAI MVP should support:

* Stable Workspace identity
* Personal Workspace
* Team Workspace
* One owner per Workspace
* Active, Suspended, Archived and Pending Deletion states
* Workspace display name
* Workspace slug
* Default locale
* Default time zone
* Workspace Membership
* User members
* Member invitation
* Active, Suspended and Removed Membership states
* System-defined roles
* Owner role
* Administrator role
* Translator role
* Reviewer role
* Reader role
* Workspace-level permissions
* Project-specific membership or roles
* One Workspace per Project
* Private and Workspace Project visibility
* Basic Workspace Settings
* Default source and target languages
* Default Profile references
* Basic Workspace Policy Set
* Local-only processing policy
* Cloud-provider allow or deny policy
* Public-sharing policy
* Basic retention policy
* Workspace shared Translation Profiles
* Workspace shared OCR Profiles
* Workspace shared Presentation Profiles
* Optional Workspace Glossary
* Exact shared-resource revisions
* Workspace provider configuration references
* Secure credential references
* Basic Workspace quotas
* Project count quota
* Member count quota
* Storage quota
* Translation usage quota
* Basic usage summaries
* Workspace audit events
* Ownership transfer
* Workspace suspension
* Workspace archival
* Delayed deletion request
* Tenant-scoped search and cache
* Workspace export without raw credentials

The MVP may defer:

* Custom roles
* Explicit permission-deny rules
* Directory-managed memberships
* Organization identity-provider synchronization
* Service accounts
* Automation agents
* Co-owners
* Multi-step administrative approvals
* Advanced separation of duties
* Shared public Workspaces
* Workspace Merge
* Workspace Split
* Automated Project transfer
* Cross-region migration
* Customer-managed encryption keys
* Legal hold
* Advanced content classification
* Multiple subscriptions per Workspace
* One subscription covering several Workspaces
* Complex project budgets
* Real-time quota reservation
* Quota overage billing
* Workspace-level Character catalogs
* Shared AI memory
* Shared browser connectors
* Provider credential delegation
* Personal credentials inside Team Workspaces
* Advanced data residency
* Workspace backup administration
* Immutable audit export
* Public Project discovery
* Guest link collaboration
* Real-time Workspace presence
* Advanced Workspace templates
* Cross-Workspace shared resources
* Federated Workspaces
* Automatic policy migration
* Full compliance reporting
* AI-training contribution workflows

---

# Related Documents

* `README.md`
* `USER.md`
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
* `SESSION.md`
* `PROFILE.md`
* `docs/architecture/CAPABILITY_MAP.md`
* `docs/architecture/DATA_FLOW.md`
* `docs/architecture/STATE_MACHINE.md`
* `docs/architecture/EVENT_BUS.md`
* `docs/architecture/MODULE_DEPENDENCY.md`
* `docs/architecture/security/AUTHENTICATION.md`
* `docs/architecture/security/AUTHORIZATION.md`
* `docs/architecture/security/TENANT_ISOLATION.md`
* `docs/architecture/security/SECRETS.md`
* `docs/architecture/runtime/JOB.md`
* `docs/architecture/runtime/QUEUE.md`
* `docs/architecture/runtime/STORAGE.md`
* `docs/architecture/runtime/CACHE.md`
* `docs/architecture/integration/PROVIDER.md`
* `docs/architecture/integration/CONNECTOR.md`
* `docs/architecture/operations/AUDIT.md`
* `docs/architecture/operations/USAGE.md`
* `docs/architecture/operations/QUOTA.md`
* `docs/architecture/operations/BILLING.md`
