# CRAI Domain Model

* **Document:** Domain Overview
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

This directory defines the **business domain model** of CRAI.

The Domain layer describes:

```text
What CRAI knows
What business concepts exist
Which domain owns which truth
Which invariants must hold
How durable business history is preserved
How business concepts relate
```

independently of:

* UI,
* database,
* AI provider,
* OCR engine,
* runtime implementation,
* framework,
* infrastructure,
* programming language.

The Domain Model SHOULD remain valid even if every external technology changes.

---

# Domain Philosophy

CRAI separates:

```text
Business Meaning
        |
        v
Business Intent
        |
        v
Immutable Resolved Inputs
        |
        v
Historical Business Truth
```

from:

```text
Runtime Execution
Provider Calls
Storage
Networking
Queues
Workers
UI
```

The first group belongs to the Domain/Application architecture.

The second belongs to runtime, infrastructure, integration or presentation layers.

---

# Core Domain Principles

CRAI Domain follows several global principles.

---

## Provider Neutrality

Business concepts MUST NOT depend on specific providers.

The Domain MUST NOT require concepts such as:

```text
OpenAI model
Gemini model
Claude model
Qwen model
PaddleOCR engine
provider API key
provider organization
provider language code
```

Providers belong to infrastructure/integration layers.

The Domain expresses intent and constraints.

---

## Stable Identity

Important long-lived business concepts have stable identities.

Example:

```text
Character ID
    !=
Character Revision ID
```

Likewise:

```text
Profile ID
    !=
Profile Revision ID
```

and:

```text
Translation ID
    !=
Translation Revision ID
```

Stable identity represents the continuing business concept.

Revision identity represents one immutable historical state.

---

## Immutable Business History

Historical business artifacts MUST NOT silently change.

Examples include:

* Translation Revision,
* Glossary Snapshot,
* Character Context Snapshot,
* Profile Revision,
* Resolved Profile Snapshot,
* Operation Context Snapshot,
* Resolved Configuration Snapshot.

Corrections produce new revisions or snapshots.

They MUST NOT rewrite previously consumed historical truth.

---

## Explicit Context

Durable operations MUST NOT silently read mutable business state while executing.

Instead of:

```text
Translation
    reads
Current Glossary
Current Character
Current Profile
Current Session
```

CRAI resolves explicit immutable inputs first.

```text
Mutable Sources
        |
        v
Resolution
        |
        v
Immutable Operation Inputs
        |
        v
Durable Operation
```

This is the foundation of reproducibility.

---

## Separation of Truth and Execution

The Domain defines:

```text
what something means
what must be true
what is intended
what historical state was used
```

Infrastructure defines:

```text
how the work executes
```

Example:

```text
Translation Profile
```

may describe:

* desired translation style,
* terminology behavior,
* context policy,
* validation intent.

It MUST NOT require knowledge of:

* HTTP,
* provider SDK,
* API keys,
* concrete prompts,
* worker topology.

---

## Explicit Ownership

Every important business truth SHOULD have one clear owning domain.

Other domains reference or consume that truth.

They SHOULD NOT silently duplicate ownership.

Example:

```text
Character
    owns character identity

Translation
    consumes Character Context Snapshot
```

Translation does not become owner of Character truth.

---

## Scope Is Not Semantic Ownership

A resource may be scoped to a Workspace or Project without Workspace or Project owning its internal semantics.

Example:

```text
Workspace-scoped Profile
```

means the Profile is available/governed within that Workspace.

It does NOT mean Workspace owns Profile revision semantics.

Likewise:

```text
Project Glossary
```

does not mean Project owns Glossary Entry semantic rules.

---

# Domain Landscape

The CRAI Domain consists of several related but distinct areas.

```text
Tenant / Governance
        |
        v
    Workspace
        |
        v
     Project
        |
        +----------------------+
        |                      |
        v                      v
Content Structure         Shared Context
        |                      |
        v                      |
      Book                     |
        |                      |
      Chapter                  |
        |                      |
       Page                    |
        |                      |
       Image                   |
        |                      |
     TextBlock                 |
                               |
                 +-------------+-------------+
                 |             |             |
                 v             v             v
              Glossary      Character      Profile

                 \             |             /
                  \            |            /
                   +-----------+-----------+
                               |
                               v
                            Session
                               |
                               v
                     Operation Resolution
                               |
                               v
                         Translation
```

`Language` is a foundational Value Object used across these domains rather than a content container.

---

# Domain Areas

CRAI Domain can be understood through six major areas.

```text
1. Tenant and Governance
   Workspace

2. Project Boundary
   Project

3. Content Structure
   Book
   Chapter
   Page
   Image
   TextBlock

4. Semantic Context
   Language
   Glossary
   Character
   Profile

5. Working Context
   Session

6. Historical Output
   Translation
```

These areas cooperate without collapsing into one aggregate hierarchy.

---

# Tenant and Governance Boundary

```text
Workspace
```

is CRAI's highest-level:

* tenant boundary,
* administrative boundary,
* collaboration boundary,
* policy boundary,
* isolation boundary.

Workspace governs availability and constraints.

It does NOT own all semantic truth inside the tenant.

---

# Project Boundary

```text
Project
```

is the primary business boundary for a translation, reading or publication collection.

A Project belongs to one Workspace in MVP.

Project coordinates content and Project-scoped configuration.

It does not replace the semantic ownership of:

* Glossary,
* Character,
* Profile,
* Translation.

---

# Content Structure

The normal content structure is:

```text
Project
    |
    v
Book
    |
    v
Chapter
    |
    v
Page
    |
    v
Image
    |
    v
TextBlock
```

This represents the common structural path.

It MUST NOT be interpreted as saying every level is mandatory for every operation.

For example, CRAI MAY support text imported directly into a Project or Chapter without requiring an Image.

---

# Optional Structural Layers

Some structural layers MAY be optional depending on source type.

Examples:

```text
Novel text import

Project
    |
    v
Book
    |
    v
Chapter
    |
    v
TextBlock
```

or:

```text
Standalone document

Project
    |
    v
TextBlock
```

Canonical ownership MUST remain explicit even when intermediate structural levels are absent.

---

# Language

`Language` defines canonical language identity.

It is used by:

* Project,
* Book,
* Chapter,
* TextBlock,
* Translation,
* Glossary,
* Profile,
* Session,
* provider adapters.

Language may describe:

* language,
* script,
* region,
* locale where appropriate.

Provider-specific language identifiers MUST remain outside the Domain.

---

# Glossary

`Glossary` owns terminology truth.

It manages concepts such as:

* Glossary identity,
* Entry identity,
* Entry revisions,
* scope,
* matching semantics,
* terminology constraints.

Durable operations consume:

```text
GlossarySnapshot
```

They MUST NOT depend directly on mutable Glossary state.

---

# Character

`Character` owns character identity and character-related canonical knowledge.

It may include:

* stable identity,
* names,
* aliases,
* descriptions,
* relationships,
* visual references,
* revisions,
* approval state.

Durable contextual operations consume:

```text
CharacterContextSnapshot
```

Character does NOT own runtime speaker attribution decisions.

---

# Profile

`Profile` defines reusable processing intent.

Profile kinds MAY include:

* Translation,
* OCR,
* Presentation,
* Validation,
* Context,
* Routing.

Profile owns:

* stable Profile identity,
* Profile Revision,
* inheritance semantics,
* compatibility rules,
* resolution semantics.

Operations SHOULD consume exact resolved Profile inputs.

---

# Session

`Session` owns resumable temporary working context.

A Session MAY capture:

* active Project,
* reading position,
* selected content,
* temporary overrides,
* working preferences,
* selected Profile intent,
* temporary Glossary sources,
* temporary context choices.

Session is not:

* authentication Session,
* provider conversation,
* runtime worker,
* job,
* queue message,
* durable Translation history.

---

# Translation

`Translation` owns durable translated business history.

Translation owns:

```text
Translation
    |
    v
Translation Revision
```

Each Translation Revision SHOULD reference the immutable inputs that materially influenced it.

Translation MUST NOT store mutable configuration as historical truth.

---

# Aggregate Roots

Expected primary Aggregate Roots include:

```text
Workspace
Project

Book
Chapter
Page
Image
TextBlock

Translation
Glossary
Character
Profile
Session
```

Not every conceptual object needs to be an Aggregate Root.

Some concepts are:

* entities,
* revisions,
* Value Objects,
* immutable snapshots,
* references,
* projections.

Aggregate boundaries are defined in the corresponding domain documents.

---

# Aggregate Relationship Rule

Aggregate relationships SHOULD use stable references.

Conceptually:

```text
Aggregate A
    |
    | stable reference
    v
Aggregate B
```

One Aggregate SHOULD NOT require loading another complete Aggregate merely to preserve its own invariants.

---

# Ownership Model

Ownership is intentionally separated into several meanings.

```text
Tenant Ownership
    Workspace

Business Collection Ownership
    Project

Content Structural Ownership
    Book / Chapter / Page / Image / TextBlock

Semantic Truth Ownership
    Glossary / Character / Profile / Language

Temporary Working State
    Session

Historical Translation Truth
    Translation
```

These ownership types MUST NOT be conflated.

---

# Workspace Ownership

Workspace owns:

* stable tenant identity,
* administrative metadata,
* lifecycle,
* owner reference,
* tenant boundary.

Workspace governs:

* membership,
* Project ownership,
* authorization scope,
* shared-resource availability,
* defaults,
* mandatory policies,
* provider availability,
* privacy,
* usage attribution,
* audit scope.

Workspace does NOT semantically own every resource inside it.

---

# Project Ownership

Project owns the business collection boundary.

It coordinates:

* content hierarchy,
* Project-scoped defaults,
* Project-scoped resource selections,
* Project visibility,
* Project lifecycle.

Project SHOULD reference rather than absorb the internal semantics of Glossary, Profile and Character.

---

# Content Ownership

Content domains own structural source truth.

Typical chain:

```text
Book
    |
Chapter
    |
Page
    |
Image
    |
TextBlock
```

Each domain owns its own identity and lifecycle.

A parent/child structural relationship does not imply that every child must be embedded inside the parent Aggregate.

---

# Temporary Ownership

Session owns temporary working state.

Examples:

* current reading position,
* temporary selection,
* temporary override,
* current working context.

Session MUST NOT become canonical owner of:

* Translation history,
* Character truth,
* Glossary truth,
* Profile definitions,
* Project content.

---

# Historical Output Ownership

Translation owns historical translated output.

A Translation Revision represents what was produced from specific immutable inputs at a specific point in business history.

Changing:

* Profile,
* Glossary,
* Character,
* Workspace defaults,
* Session settings,

MUST NOT silently mutate an existing Translation Revision.

---

# Revision Pattern

Several CRAI domains use a shared conceptual pattern:

```text
Stable Identity
        |
        v
Immutable Revision
        |
        v
Resolved / Context Snapshot
        |
        v
Historical Reference
```

Not every domain requires every stage.

---

# Revision vs Snapshot

A Revision and Snapshot are different concepts.

```text
Revision
    = immutable state of one domain object

Snapshot
    = immutable resolved context assembled for consumption
```

Example:

```text
Profile Revision
```

belongs to Profile history.

```text
Resolved Profile Snapshot
```

may combine several Profile revisions and effective choices for one operation.

---

# Snapshot Pattern

Mutable working state MUST cross an immutable boundary before influencing durable output.

```text
Mutable Sources
        |
        v
Resolver
        |
        v
Immutable Snapshot
        |
        v
Durable Artifact
```

Examples:

```text
Glossary
    |
    v
GlossarySnapshot
```

```text
Character
    |
    v
CharacterContextSnapshot
```

```text
Profile
    |
    v
ResolvedProfileSnapshot
```

```text
Session + Project + Workspace + Operation Intent
    |
    v
ResolvedConfigurationSnapshot
```

---

# Snapshot Composition

An operation may require several immutable inputs.

Conceptually:

```text
Source Content Snapshot
        +
Language Resolution
        +
GlossarySnapshot
        +
CharacterContextSnapshot
        +
ResolvedProfileSnapshot
        +
Session-derived Context
        +
Applicable Policy Revision
        |
        v
OperationContextSnapshot
```

Exact composition depends on the capability.

Not every operation requires every snapshot.

---

# Operation Context

`OperationContextSnapshot` is a conceptual immutable envelope containing the effective business context required by one durable operation.

It MAY reference:

* source content identity/revision,
* source Language,
* target Language,
* GlossarySnapshot,
* CharacterContextSnapshot,
* ResolvedProfileSnapshot,
* ResolvedConfigurationSnapshot,
* applicable policy revision,
* Project,
* Session,
* user intent.

The exact schema SHOULD be capability-specific rather than one universal giant structure.

---

# Configuration Resolution

CRAI MUST NOT assume one universal precedence chain for every configuration field.

Different domains may require different resolution semantics.

Conceptually:

```text
Relevant Defaults
        +
Explicit Selections
        +
Temporary Session Intent
        +
Operation Overrides
        +
User Preferences
        +
Mandatory Policies
        +
Capability Rules
        |
        v
Resolved Configuration
```

---

# Resolution Sources

Depending on the capability, sources MAY include:

* application defaults,
* user preferences,
* Workspace defaults,
* Workspace policies,
* Project configuration,
* Book configuration,
* Chapter configuration,
* Page configuration,
* Session configuration,
* Operation override.

Presence in this list does NOT imply that every field supports every scope.

---

# Resolution Ownership

The domain that owns the semantic concept SHOULD define its resolution semantics.

Examples:

```text
Glossary
    owns Glossary resolution semantics

Profile
    owns Profile resolution semantics

Language
    owns Language normalization semantics

Workspace
    contributes defaults and constraints

Session
    contributes temporary intent
```

Application/capability orchestration combines these resolvers for an operation.

---

# Default vs Constraint

A default and a policy constraint are different.

```text
Default
    = value selected when no narrower explicit choice exists

Constraint
    = rule defining which choices are permitted
```

Example:

```text
Workspace default target Language:
Vietnamese
```

may be overridden if allowed.

But:

```text
Workspace policy:
Cloud processing forbidden
```

cannot be overridden by Project, Session or Operation intent.

---

# Context Resolution

Translation never depends only on source text.

Possible context sources include:

```text
TextBlock
Language
Glossary
Character
Profile
Project
Session
Workspace policy
Operation intent
```

These are resolved into immutable operation inputs before durable output is committed.

---

# Translation Input Model

Conceptually:

```text
TextBlock Revision / Source Snapshot
        +
Source Language
        +
Target Language
        +
GlossarySnapshot
        +
CharacterContextSnapshot
        +
ResolvedProfileSnapshot
        +
ResolvedConfigurationSnapshot
        |
        v
Translation Operation
        |
        v
Translation Revision
```

Optional inputs SHOULD be omitted when not required.

---

# Processing Architecture Boundary

The Domain defines the business meaning of processing.

A conceptual capability flow may be:

```text
Source Acquisition
        |
        v
Text Extraction / OCR
        |
        v
TextBlock
        |
        v
Language Resolution
        |
        v
Context Resolution
        |
        v
Profile Resolution
        |
        v
Translation
        |
        v
Validation
        |
        v
Presentation
```

This is a conceptual capability flow.

It is NOT an aggregate hierarchy.

It is NOT a runtime implementation contract.

---

# OCR Boundary

OCR is a capability/module.

Its business outputs may include:

* detected text,
* TextBlocks,
* layout information,
* confidence metadata,
* source relationships.

OCR engines and provider calls remain outside the Domain.

---

# Validation Boundary

Validation is primarily a capability/workflow.

Validation may consume domain truth and produce durable review/validation artifacts where required.

A dedicated `VALIDATION.md` domain document SHOULD only be introduced if CRAI later identifies stable business identity and lifecycle that justify a distinct domain.

---

# Presentation Boundary

Presentation is primarily a capability/module that transforms domain outputs into user-consumable form.

Presentation MAY consume:

* Translation Revision,
* TextBlock layout,
* Profile configuration,
* source Image,
* font/layout policy.

Presentation MUST NOT become owner of Translation truth.

A dedicated Presentation domain SHOULD only exist if CRAI later introduces durable presentation artifacts with their own business identity and lifecycle.

---

# Domain Dependency Philosophy

Domain dependencies SHOULD follow semantic ownership, not execution order.

Example:

```text
Translation
    references
Language
GlossarySnapshot
CharacterContextSnapshot
ResolvedProfileSnapshot
```

This does NOT mean:

```text
Language
    executes before
Glossary
```

Execution ordering belongs to capability/runtime architecture.

---

# Structural Relationship Graph

Typical structural relationships:

```text
Workspace
    |
    v
Project
    |
    v
Book
    |
    v
Chapter
    |
    v
Page
    |
    v
Image
    |
    v
TextBlock
```

Intermediate levels MAY be optional where explicitly supported by their domain contracts.

---

# Semantic Relationship Graph

Conceptually:

```text
                   Language
                      |
                      v

Glossary ------> Operation Context <------ Character
                      ^
                      |
                   Profile
                      ^
                      |
                   Session
                      ^
                      |
Workspace -----> Project
                      |
                      v
                  TextBlock
                      |
                      v
                 Translation
```

This graph expresses semantic contribution.

It does NOT imply aggregate containment.

---

# Workspace / Project / Session / Operation

These scopes serve different purposes.

```text
Workspace
    = tenant governance

Project
    = durable business collection

Session
    = resumable temporary working context

Operation
    = one concrete execution intent
```

They MUST remain separate.

---

# Workspace and Project

```text
Workspace
    governs

Project
    specializes
```

Workspace may provide:

* defaults,
* policy,
* shared-resource availability.

Project may select or specialize permitted values.

---

# Project and Session

```text
Project
    = durable project state

Session
    = temporary working state
```

Session MAY temporarily override permitted Project defaults.

Those overrides MUST NOT silently rewrite Project configuration.

---

# Session and Operation

Session represents ongoing work.

Operation represents one concrete action.

```text
Session
    |
    | contributes intent
    v
Operation Resolution
    |
    v
Immutable Inputs
```

Changing Session state after resolution MUST NOT mutate an already-started durable operation.

---

# Language Boundary

Language identity is canonical business data.

Provider-specific mappings belong to adapters.

```text
Canonical Language
        |
        v
Provider Adapter
        |
        v
Provider-specific Code
```

Domain documents MUST use canonical Language values.

---

# Glossary Boundary

Glossary resolution produces immutable terminology input.

Translation MUST NOT query mutable Glossary state after its operation context has been resolved.

---

# Character Boundary

Character provides canonical identity and contextual truth.

Speaker attribution, visual recognition and inference MAY consume Character information.

Those capabilities MUST NOT silently mutate Character truth.

---

# Profile Boundary

Profile represents reusable intent.

Profile is not:

* provider configuration,
* prompt,
* runtime job configuration,
* UI preference bundle.

A Profile MAY eventually influence those layers through explicit resolution.

---

# Session Boundary

Session is working context.

It is not durable semantic truth merely because it is persisted for resume/recovery.

Persisted temporary state remains temporary business state.

---

# Translation Boundary

Translation is historical business output.

Translation Revision SHOULD remain explainable by its referenced immutable inputs.

---

# Business Events

Domain events represent meaningful business changes.

Examples:

```text
WorkspaceCreated
ProjectArchived

TranslationRevisionCreated

GlossaryRevisionPublished
CharacterRevisionApproved
ProfileRevisionActivated

SessionStarted
SessionPaused
SessionResumed
SessionCompleted
```

Events SHOULD use stable business identifiers.

---

# Domain Events vs Runtime Events

Domain events answer:

```text
What meaningful business fact occurred?
```

Runtime events answer:

```text
What happened during execution?
```

Example:

```text
TranslationRevisionCreated
```

is a business event.

```text
TranslationWorkerRetryScheduled
```

is a runtime/infrastructure event.

The Domain SHOULD NOT depend on runtime event semantics.

---

# Event Payload Philosophy

Domain events SHOULD contain:

* stable identifiers,
* relevant revision identifiers,
* effective timestamps,
* safe reason/status metadata,
* correlation identifiers where useful.

They SHOULD NOT contain unnecessary:

* raw source content,
* provider credentials,
* API secrets,
* huge binary payloads.

---

# Identity Philosophy

Long-lived business objects use stable IDs.

Historical states use revision IDs.

```text
Stable Identity
        |
        +--> Revision 1
        +--> Revision 2
        +--> Revision 3
```

Stable identity does not change because metadata changes.

---

# Reference Philosophy

References between domains SHOULD use:

```text
stable identity
```

when referring to the continuing concept.

They SHOULD use:

```text
revision identity
or
snapshot identity
```

when historical reproducibility requires exact state.

---

# Historical Reference Rule

A durable historical artifact MUST reference exact historical inputs when those inputs can materially affect interpretation or reproducibility.

Example:

```text
TranslationRevision
    |
    +--> GlossarySnapshot ID
    +--> CharacterContextSnapshot ID
    +--> ResolvedProfileSnapshot ID
    +--> ResolvedConfigurationSnapshot ID
```

Exact requirements remain defined by Translation and capability contracts.

---

# Mutable vs Immutable State

CRAI deliberately supports both.

Mutable state is useful for:

* editing,
* configuration,
* active work,
* collaboration,
* Session recovery.

Immutable state is required for:

* history,
* reproducibility,
* audit,
* durable references.

The architecture MUST NOT confuse the two.

---

# Domain Validation

Each domain owns validation of its semantic invariants.

Examples:

```text
Language
    validates canonical Language values

Glossary
    validates Entry semantics

Character
    validates Character revisions

Profile
    validates Profile revisions

Translation
    validates Translation history

Workspace
    validates tenant/lifecycle invariants
```

Cross-domain workflows MAY perform additional application-level validation.

---

# Domain Errors

Domain errors SHOULD use stable machine-readable codes.

Human-readable messages are presentation concerns.

Example:

```text
PROFILE_REVISION_NOT_FOUND
GLOSSARY_ENTRY_CONFLICT
CHARACTER_REVISION_INVALID
TRANSLATION_REVISION_CONFLICT
WORKSPACE_POLICY_DENIED
```

Each domain owns its error namespace.

---

# Concurrency Philosophy

Mutable Aggregate Roots SHOULD use optimistic concurrency where concurrent edits are possible.

Typical pattern:

```text
expectedVersion
        |
        v
validate
        |
        +--> mismatch -> conflict
        |
        v
commit new state
```

Immutable revisions do not require in-place mutation concurrency.

---

# Idempotency Philosophy

Business workflows that may be retried SHOULD define idempotency semantics.

Examples:

* Workspace provisioning,
* import,
* Translation creation,
* revision publication,
* Session checkpointing,
* migration.

Idempotency keys and storage implementation belong to application/infrastructure architecture.

Domain contracts define the business expectation.

---

# Privacy Boundary

Domain objects SHOULD contain only business data necessary for their semantics.

Sensitive infrastructure data such as:

* provider credentials,
* authentication tokens,
* encryption keys,

MUST remain outside ordinary domain aggregates.

Domain references MAY point to secure configuration identifiers where necessary.

---

# Tenant Isolation

All Workspace-private business data MUST remain attributable to its Workspace.

Cross-Workspace access requires explicit sharing, migration or policy.

Tenant isolation applies regardless of physical storage implementation.

---

# Cross-Workspace Reuse

Private business truth MUST NOT silently become global truth.

Examples:

* Glossary terms,
* Character data,
* Translation corrections,
* reading history,
* Profile customizations.

Cross-Workspace reuse requires explicit provenance and policy.

---

# Technology Independence

The Domain MUST NOT require:

* REST,
* GraphQL,
* gRPC,
* WebSocket,
* SQL,
* Redis,
* Kafka,
* filesystem paths,
* Kubernetes,
* cloud storage,
* provider SDKs.

Those technologies MAY implement Domain requirements but MUST NOT define them.

---

# Domain vs Application Layer

Domain defines:

```text
business concepts
business invariants
semantic ownership
immutable business history
```

Application orchestration defines:

```text
which domains participate
resolution order
workflow coordination
authorization orchestration
capability invocation
```

This distinction is especially important for:

* context resolution,
* Profile resolution,
* Glossary resolution,
* Translation execution,
* review workflows.

---

# Domain vs Runtime

Runtime defines:

* jobs,
* workers,
* retries,
* scheduling,
* queues,
* cancellation,
* concurrency execution,
* process recovery.

Domain defines the business meaning those runtime operations serve.

---

# Domain vs Infrastructure

Infrastructure defines:

* persistence,
* provider clients,
* caches,
* search,
* secret storage,
* telemetry,
* file storage,
* networking.

Domain MUST remain portable across infrastructure implementations.

---

# Domain vs Presentation

Presentation defines:

* UI,
* layout,
* visual rendering,
* interaction,
* user-facing formatting.

Presentation consumes Domain truth.

It MUST NOT become canonical owner of that truth.

---

# Domain vs Security

Security architecture defines:

* authentication,
* credential handling,
* token issuance,
* cryptographic mechanisms,
* authorization execution.

Domain may define:

* ownership,
* Membership,
* roles,
* policy semantics,
* authorization-relevant business scope.

---

# Domain vs Operations

Operational architecture may own:

* telemetry,
* infrastructure health,
* billing integration,
* usage ledger,
* operational audit storage,
* backup execution.

Domain defines the business boundaries to which those records are attributed.

---

# Current Domain Documents

Canonical documents:

```text
README.md

WORKSPACE.md
PROJECT.md

BOOK.md
CHAPTER.md
PAGE.md
IMAGE.md
TEXT_BLOCK.md

LANGUAGE.md
GLOSSARY.md
CHARACTER.md
PROFILE.md
SESSION.md

TRANSLATION.md
```

---

# Current Domain Groups

Recommended grouping:

```text
Governance
    WORKSPACE.md

Business Collection
    PROJECT.md

Content
    BOOK.md
    CHAPTER.md
    PAGE.md
    IMAGE.md
    TEXT_BLOCK.md

Semantic Context
    LANGUAGE.md
    GLOSSARY.md
    CHARACTER.md
    PROFILE.md

Working Context
    SESSION.md

Historical Output
    TRANSLATION.md
```

---

# Future Domain Candidates

Potential future documents include:

```text
ANNOTATION.md
REVIEW.md
COMMENT.md
TAG.md
ATTACHMENT.md
KNOWLEDGE.md
STYLE_GUIDE.md
```

These SHOULD NOT be created merely because the concepts are useful.

A dedicated domain document is justified when the concept has sufficiently stable:

* business identity,
* semantic ownership,
* lifecycle,
* invariants,
* relationships.

---

# Import and Export

`IMPORT` and `EXPORT` SHOULD initially remain workflows/capabilities rather than automatically becoming Domain Aggregate Roots.

A dedicated domain document MAY be introduced later if CRAI requires durable import/export jobs or artifacts with independent business identity and lifecycle.

---

# Annotation and Review

`Annotation` and `Review` are likely stronger future domain candidates because they may require:

* stable identity,
* authorship,
* lifecycle,
* revisions,
* approval state,
* historical references.

Their exact boundaries remain open.

---

# Knowledge and Style Guide

Reusable knowledge and style rules SHOULD remain explicit rather than being hidden inside Workspace, Project or Profile blobs.

Future domains may include:

```text
Knowledge Base
Style Guide
```

only when their semantic boundaries are sufficiently understood.

---

# Recommended Reading Order

Contributors SHOULD read the Domain documents in this order:

```text
1.  README.md

2.  WORKSPACE.md
3.  PROJECT.md

4.  BOOK.md
5.  CHAPTER.md
6.  PAGE.md
7.  IMAGE.md
8.  TEXT_BLOCK.md

9.  LANGUAGE.md
10. GLOSSARY.md
11. CHARACTER.md
12. PROFILE.md

13. SESSION.md

14. TRANSLATION.md
```

This order moves from:

```text
architecture principles
        |
        v
tenant governance
        |
        v
business collection
        |
        v
content structure
        |
        v
semantic context
        |
        v
working context
        |
        v
historical output
```

---

# Relationship to Architecture Documents

This directory defines business concepts and invariants.

Related architecture documents define cross-domain structure and execution.

Examples:

```text
docs/architecture/
├── CAPABILITY_MAP.md
├── OWNERSHIP_MAP.md
├── DATA_FLOW.md
├── STATE_MACHINE.md
├── EVENT_BUS.md
└── MODULE_DEPENDENCY.md
```

When a cross-domain architecture document and a Domain document appear to conflict:

```text
Domain document
    owns semantic truth of its concept

Cross-domain architecture document
    owns system-wide orchestration/relationship rules
```

The conflict SHOULD be resolved explicitly rather than silently choosing one.

---

# Relationship to Modules

Domain documents answer:

```text
What does this concept mean?
What does it own?
What invariants apply?
```

Module documents answer:

```text
Which capability implements behavior around it?
What contracts are exposed?
What events/states/errors exist operationally?
```

One Domain MAY be used by several Modules.

One Module MAY orchestrate several Domains.

Therefore:

```text
Domain
    !=
Module
```

---

# Domain-to-Module Example

Example:

```text
Translation Domain
    owns
Translation + Translation Revision
```

while:

```text
Translation Module
    may orchestrate
Language
GlossarySnapshot
CharacterContextSnapshot
Profile resolution
Provider execution
Validation
Translation persistence
```

The Module does not gain semantic ownership of those supporting domains.

---

# Domain-to-Infrastructure Example

Example:

```text
Glossary Domain
    owns
Glossary semantics
```

while infrastructure may provide:

```text
database
search index
cache
vector search
```

Replacing those technologies MUST NOT change Glossary's business meaning.

---

# Global Domain Invariants

1. Business semantics remain technology-independent.

2. Stable business identities MUST NOT change because metadata or infrastructure changes.

3. Historical revisions are immutable.

4. Corrections create new historical state rather than rewriting consumed history.

5. Durable business artifacts MUST NOT depend on mutable state.

6. Mutable inputs affecting durable output MUST cross an immutable resolution boundary.

7. Providers MUST NOT become canonical Domain identities.

8. Provider-specific credentials remain outside ordinary Domain aggregates.

9. Provider-specific Language codes remain outside canonical Language identity.

10. Domain configuration remains provider-neutral.

11. Business context is explicit.

12. Snapshots provide reproducible consumption boundaries.

13. Revision and Snapshot are distinct concepts.

14. Aggregates communicate through explicit references.

15. Stable references and historical references MUST be chosen intentionally.

16. Workspace is CRAI's highest-level tenant and governance boundary.

17. Workspace MUST NOT become the semantic owner of every Workspace-scoped resource.

18. Project is the primary durable business collection boundary.

19. A Project belongs to one Workspace in MVP.

20. Content hierarchy and aggregate containment MUST NOT be conflated.

21. Structural content layers MAY be optional where their domain contracts allow it.

22. TextBlock is the canonical textual source unit consumed by text Translation.

23. Language is canonical business identity, not provider configuration.

24. Glossary owns terminology semantics.

25. Translation consumes immutable Glossary snapshots.

26. Character owns Character identity and canonical Character truth.

27. Translation/context processing consumes immutable Character context snapshots.

28. Profile owns reusable processing intent and Profile revision semantics.

29. Profile MUST NOT become a provider prompt/configuration object.

30. Session owns resumable temporary working state.

31. Session MUST NOT become authentication, runtime job or provider conversation state.

32. Session state MAY contribute intent but MUST NOT silently mutate resolved operations.

33. Translation owns historical translated output.

34. Translation Revision MUST remain immutable.

35. Translation MUST NOT store mutable configuration as historical truth.

36. Workspace defaults and Workspace mandatory policies are distinct.

37. Defaults MAY be overridden where policy permits.

38. Mandatory higher-level policy MUST NOT be bypassed by narrower intent.

39. CRAI MUST NOT assume one universal configuration precedence chain for every semantic field.

40. Each owning domain defines its own resolution semantics.

41. Application/capability orchestration combines domain resolvers.

42. Operation resolution SHOULD produce exact immutable inputs before durable execution.

43. Not every capability requires every available context source.

44. Operation Context SHOULD be capability-specific rather than one universal giant snapshot.

45. Domain dependency expresses semantic dependency, not runtime execution order.

46. OCR is primarily a capability/module, not automatically a Domain Aggregate.

47. Validation is primarily a capability/workflow unless stable business identity later justifies a dedicated domain.

48. Presentation is primarily a capability/module unless durable presentation business artifacts later justify a dedicated domain.

49. Import and Export are initially workflows/capabilities, not automatically Domain Aggregates.

50. Business events represent meaningful business facts.

51. Runtime events MUST remain distinguishable from Domain events.

52. Domain event payloads SHOULD avoid unnecessary sensitive/raw content.

53. Each domain owns validation of its semantic invariants.

54. Cross-domain validation belongs to application orchestration where appropriate.

55. Domain errors SHOULD use stable machine-readable codes.

56. Mutable Aggregate Roots SHOULD support concurrency control where necessary.

57. Retriable consequential workflows SHOULD define idempotency semantics.

58. Workspace-private business data MUST remain attributable to a Workspace.

59. Cross-Workspace access or reuse requires explicit policy/provenance.

60. Private Workspace truth MUST NOT silently become global truth.

61. Domain MUST NOT depend on persistence technology.

62. Domain MUST NOT depend on network protocol.

63. Domain MUST NOT depend on runtime worker topology.

64. Domain MUST NOT depend on UI implementation.

65. Domain MUST remain understandable independently of the codebase.

---

# Architecture Consistency Checklist

When creating or modifying a Domain document, verify:

```text
[ ] Does the concept have a clear owner?

[ ] Is stable identity separated from revision identity?

[ ] Are mutable and immutable states distinguished?

[ ] Are historical references exact where reproducibility requires them?

[ ] Are provider-specific concepts excluded?

[ ] Are runtime concepts excluded?

[ ] Are infrastructure concepts excluded?

[ ] Is tenant scope explicit?

[ ] Are cross-domain relationships references rather than accidental ownership?

[ ] Are defaults separated from mandatory policies?

[ ] Does the domain own its own semantic resolution rules?

[ ] Is Session state treated as temporary working context?

[ ] Are durable outputs protected from later mutable changes?

[ ] Are event semantics business-oriented?

[ ] Are errors stable and machine-readable?

[ ] Are open implementation choices left outside the Domain?
```

---

# Open Architecture Questions

The following cross-domain questions SHOULD remain explicit until implementation or dedicated architecture documents settle them:

* whether every user automatically receives a Personal Workspace,
* exact Local Workspace semantics,
* exact optionality of Book/Page/Image structural layers,
* whether TextBlock may belong directly to Project or Chapter,
* whether content ownership uses direct parent references or a generalized content locator,
* exact source-content revision model,
* exact `OperationContextSnapshot` structure,
* whether `ResolvedConfigurationSnapshot` is one shared concept or capability-specific family,
* whether Session owns a dedicated immutable Session Snapshot,
* exact Profile inheritance/resolution implementation,
* exact Glossary multi-scope resolution algorithm,
* exact Character Context Snapshot composition,
* exact Workspace/Profile dynamic-selection behavior,
* Project transfer semantics,
* shared-resource cloning semantics,
* future Review domain boundary,
* future Annotation domain boundary,
* future Knowledge domain boundary,
* future Style Guide domain boundary,
* whether durable Validation Result becomes a domain artifact,
* whether durable Presentation Artifact becomes a domain artifact,
* import/export durable artifact model,
* cross-Workspace sharing architecture.

These questions MUST NOT be resolved implicitly by individual modules.

---

# Canonical Mental Model

The CRAI Domain can be summarized as:

```text
Workspace
    |
    | governs
    v
Project
    |
    | organizes
    v
Content
    |
    | produces canonical source units
    v
TextBlock
    |
    +-------------------------------+
    |                               |
    |     Semantic Context          |
    |                               |
    |  Language                     |
    |  Glossary                     |
    |  Character                    |
    |  Profile                      |
    |  Session Intent               |
    |                               |
    +---------------+---------------+
                    |
                    v
             Resolution
                    |
                    v
          Immutable Inputs
                    |
                    v
             Translation
                    |
                    v
       Translation Revision
```

Workspace governs.

Project organizes durable work.

Content domains own source structure.

Language, Glossary, Character and Profile own semantic context.

Session owns temporary working context.

Resolvers convert mutable/current intent into immutable effective inputs.

Translation owns durable translated history.

Infrastructure executes the work.

---

# Final Principle

The central architectural rule of the CRAI Domain is:

```text
Mutable Business State
        |
        v
Explicit Resolution
        |
        v
Immutable Effective Context
        |
        v
Durable Historical Artifact
```

This boundary allows CRAI to remain:

* reproducible,
* auditable,
* provider-neutral,
* technology-independent,
* safe under future configuration changes,
* compatible with collaborative editing,
* suitable for both local and cloud execution.

A future change to:

* Workspace settings,
* Project defaults,
* Glossary entries,
* Character information,
* Profile revisions,
* Session state,
* provider configuration,
* infrastructure,

MUST NOT silently change the meaning of historical business artifacts.
