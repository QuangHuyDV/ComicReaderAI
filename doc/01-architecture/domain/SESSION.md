# Session Domain

* **Document:** Domain / Session
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

A `Session` represents a bounded, resumable user working context inside CRAI.

A Session exists so a user can:

* read content,
* translate while reading,
* review results,
* navigate content,
* preview Presentation,
* temporarily adjust behavior,
* recover after interruption,
* continue on another client or device.

A Session preserves enough working state to continue the user experience without treating temporary runtime state as permanent domain truth.

A Session MAY preserve:

* active Project,
* current content location,
* reading/resume position,
* selected Profile revisions or selection policies,
* source/target language selections,
* Session-level preferences,
* temporary overrides,
* review navigation state,
* Presentation preferences,
* bounded working context,
* checkpoint information,
* references to relevant operations.

---

# Domain Role

Conceptually:

```text
User
  |
  v
Session
  |
  +--> Content Location
  +--> Resume Position
  +--> Profile Selections
  +--> Language Selections
  +--> Session Preferences
  +--> Temporary Overrides
  +--> Review Navigation
  +--> Presentation Preferences
  +--> Working Context
```

Session coordinates domain resources.

It does NOT own those resources merely because they are used within the Session.

Typical references:

```text
Session
├── Project
├── optional Book
├── Chapter
├── optional Page
├── optional TextBlock
├── Translation references
├── Profile selections
├── Glossary context references
└── Character context references
```

---

# Session Is Not Authentication

Authentication answers:

```text
Who may access CRAI?
```

Session answers:

```text
What working context is the user currently using?
```

Therefore:

```text
Authentication Session
    !=
CRAI Session
```

Authentication credentials MUST NOT be stored as Session domain state.

Authentication lifetime MUST NOT define Session lifetime.

---

# Session Is Not Runtime Execution

Session is not:

* OCR Job,
* Translation Execution,
* Capture Process,
* provider request,
* queue task,
* worker lease,
* WebSocket connection,
* browser process,
* AI conversation thread.

Runtime operations MAY reference:

```text
sessionId
```

for correlation and context.

Their lifecycle remains independently owned.

Worker restart MUST NOT invalidate Session identity.

---

# Session Is Not Business Content

Session MUST NOT own:

* Project content,
* Book content,
* Chapter content,
* Pages,
* Images,
* TextBlocks,
* Translation truth,
* Glossary truth,
* Character truth,
* Profile definitions,
* Review decisions,
* Presentation artifacts.

Closing or deleting a Session MUST NOT delete those resources.

---

# Aggregate Boundary

Session SHOULD be an independently addressable Aggregate Root.

Recommended aggregate:

```text
Session
├── sessionId
├── ownerReference
├── projectId
├── sessionType
├── lifecycleStatus
├── currentContext
├── resumePosition
├── profileSelections
├── languageSelections
├── sessionPreferences
├── activeOverrideReferences
├── reviewNavigationReference?
├── presentationPreferences
├── latestCheckpointId?
├── parentSessionId?
├── createdAt
├── updatedAt
└── version
```

The Session Aggregate owns:

* stable Session identity,
* Session lifecycle,
* Project scope,
* current logical working location,
* resume position,
* Session selections,
* Session preferences,
* temporary override references,
* bounded navigation state,
* checkpoint references,
* fork lineage,
* optimistic concurrency.

It does NOT own:

* runtime operations,
* Profile Revisions,
* Glossary Entries,
* Character Revisions,
* Speaker Attribution,
* Review decisions,
* long-term Reading Progress,
* Presentation artifacts,
* provider state.

---

# Identity

Every Session has a stable `sessionId`.

Session identity remains stable across:

* pause/resume,
* client reconnect,
* application restart,
* navigation,
* Profile selection changes,
* runtime operation failure,
* device handoff.

A new Session identity SHOULD be created when the user deliberately starts a separate working context.

---

# Project Scope

For CRAI MVP:

```text
Session
    belongs to exactly one Project
```

The Session MAY navigate within that Project.

Example:

```text
Project
├── Book A
├── Book B
└── direct Chapters
```

Changing to a different Project SHOULD normally:

* close/fork the current working context,
* or create a new Session.

A Session MUST NOT silently mutate its Project ownership while preserving incompatible working context.

---

# Optional Content Hierarchy

Session MUST respect the optional hierarchy established by the content domain.

Possible locations include:

```text
Project
└── Book
    └── Chapter
        └── Page
```

```text
Project
└── Chapter
    └── TextBlock
```

```text
Project
└── Chapter
    └── native text position
```

Book and Page MUST NOT be required for every Session.

---

# Session Types

Recommended high-level types:

```text
READING
LIVE_TRANSLATION
REVIEW
PRESENTATION
EDITING
MIXED
```

More specific review type SHOULD be represented separately.

Example:

```text
sessionType: REVIEW
reviewType: TRANSLATION
```

rather than creating many lifecycle-level Session types such as:

```text
OCR_REVIEW
GLOSSARY_REVIEW
CHARACTER_REVIEW
IMPORT_REVIEW
```

unless implementation experience proves those need distinct domain semantics.

---

# Session Type Is Intent

Session Type describes the primary workflow.

It does NOT define capability ownership.

Example:

```text
READING
```

may still trigger:

* OCR,
* Translation,
* Presentation.

`LIVE_TRANSLATION` may still include:

* reading,
* review,
* Presentation.

Capabilities remain independently owned.

---

# Session Lifecycle

Recommended lifecycle:

```text
CREATED
   |
   v
ACTIVE
   |
   +--> PAUSED
   |      |
   |      v
   |    ACTIVE
   |
   +--> ENDED
          |
          v
       ARCHIVED
```

Recommended core statuses:

```text
CREATED
ACTIVE
PAUSED
ENDED
ARCHIVED
```

---

# Why Simplify Lifecycle

The previous distinctions:

```text
COMPLETED
CLOSED
ABANDONED
EXPIRED
```

describe useful reasons but create overlapping terminal semantics.

Instead, Session SHOULD use:

```text
lifecycleStatus: ENDED
endReason:
    COMPLETED
    USER_CLOSED
    ABANDONED
    EXPIRED
    ACCESS_REVOKED
    SYSTEM_CLOSED
```

This keeps lifecycle simple while preserving why the Session ended.

---

# Created

`CREATED` means the Session exists but may not yet have a fully resolved content position.

It MUST have:

* Session ID,
* owner,
* Project,
* Session Type,
* initial configuration intent.

---

# Active

`ACTIVE` means the Session is available for normal interaction.

An Active Session MAY:

* navigate,
* update working state,
* create checkpoints,
* change Session selections,
* start application workflows,
* create temporary overrides.

---

# Paused

`PAUSED` means the working context is preserved but automatic user-facing behavior SHOULD stop unless explicitly configured.

Pause MAY request stopping:

* automatic capture,
* auto navigation,
* automatic OCR request generation,
* automatic Translation request generation,
* live Presentation refresh.

Pause MUST NOT invalidate already published domain artifacts.

---

# Ended

`ENDED` means the Session is no longer intended for active continuation.

Ending SHOULD:

* commit a meaningful checkpoint,
* record `endReason`,
* stop Session-driven automation,
* release coordination leases,
* preserve durable external results.

An Ended Session MAY be forked or reopened according to policy.

---

# Archived

`ARCHIVED` means the Session is retained but hidden from ordinary active history.

Archive is primarily retention/discoverability state.

It MUST NOT affect referenced domain artifacts.

---

# Session Revision

Session state may change frequently.

A full immutable business revision for every pointer movement is unnecessary.

Implementation MAY use:

* aggregate versioning,
* meaningful immutable state revisions,
* periodic snapshots,
* event sourcing,
* checkpoint-based persistence.

Critical rule:

```text
high-frequency UI movement
    != durable Session revision
```

---

# Significant Session Revision

A durable revision MAY be warranted when:

* Project/context changes,
* Chapter changes,
* Profile selection changes,
* target language changes,
* major override changes,
* lifecycle changes,
* device handoff occurs,
* review workflow scope changes.

The exact persistence strategy remains an implementation decision.

---

# Durable vs Ephemeral State

Session state MUST distinguish:

```text
Durable Working State
```

from:

```text
Ephemeral Client State
```

---

# Durable Working State

Examples:

* Session identity,
* Project scope,
* logical content location,
* resume position,
* Profile selections,
* language selections,
* Session preferences,
* active Session overrides,
* review position,
* Session mode,
* latest checkpoint,
* fork lineage.

These values SHOULD normally survive restart.

---

# Ephemeral Client State

Examples:

* mouse coordinates,
* current hover,
* animation state,
* UI focus,
* scroll velocity,
* temporary drag state,
* network connection,
* decoded bitmap,
* uncommitted capture buffer.

They MUST NOT become canonical Session domain truth by default.

---

# Current Context

`SessionContext` identifies the logical working scope.

Recommended structure:

```text
SessionContext
├── projectId
├── bookId?
├── chapterId?
├── pageId?
├── textBlockId?
├── textBlockRevision?
├── imageId?
├── externalSourceReference?
├── contextType
└── updatedAt
```

Not every field is required.

The context MUST remain internally compatible.

---

# Content Location

Content location SHOULD prefer stable domain identities when available.

Examples:

```text
Project / Book / Chapter / Page
```

```text
Project / Chapter / TextBlock
```

```text
External Resource / DOM Locator
```

```text
Chapter / Image / Region
```

Temporary device coordinates MUST NOT be the sole durable content identity.

---

# Resume Position

Resume Position answers:

```text
Where should this Session continue?
```

Recommended structure:

```text
ResumePosition
├── positionType
├── logicalContentReference
├── logicalOffset?
├── progressHint?
├── visualResumeHint?
├── capturedAt
└── confidence?
```

---

# Position Types

Possible types include:

```text
PAGE
TEXT_BLOCK
PARAGRAPH
SENTENCE
CHARACTER_OFFSET
IMAGE_REGION
DOM_LOCATOR
REVIEW_ITEM
EXTERNAL_POSITION
```

A Session MUST NOT assume all reading positions are Page-based.

---

# Logical vs Visual Position

Logical and visual positions MUST remain distinct.

```text
Logical:
    Chapter 12 / Page 8 / TextBlock 4
```

```text
Visual hint:
    scrollY 1428
    zoom 125%
```

Logical position is durable.

Visual position is primarily a resume hint and MAY be client-specific.

---

# Reading Progress Boundary

Session owns:

```text
current/resume reading position
```

It SHOULD NOT be the authoritative source of long-term reading history.

Long-term reading data MAY eventually belong to:

```text
ReadingProgress
Library
ReadingHistory
```

Session MAY publish committed progress checkpoints to that domain.

---

# Progress Commit

High-frequency reading changes SHOULD be compacted.

Recommended flow:

```text
Client position
      |
      v
Ephemeral state
      |
      +--> meaningful navigation
      +--> periodic checkpoint
      +--> pause
      +--> handoff
      +--> end
      |
      v
Committed Resume Position
```

Session persistence MUST NOT become a pixel-level analytics stream.

---

# Navigation State

Session MAY maintain bounded:

* back stack,
* forward stack,
* recent locations,
* pinned Session locations.

Navigation state is interaction context.

It MUST NOT alter canonical content ordering.

---

# Navigation History

Navigation history SHOULD remain bounded.

It MAY include:

```text
NavigationEntry
├── location
├── enteredAt
├── leftAt?
├── cause
└── resumeEligible
```

It is not intended to become permanent user analytics.

---

# Session Mode

Recommended interaction modes:

```text
MANUAL
ASSISTED
AUTOMATIC
REVIEW
PRESENTATION
READ_ONLY
```

Mode affects user interaction orchestration.

It MUST NOT redefine capability ownership.

---

# Capture Source Selection

Session MAY select a source/capture mode.

Examples:

```text
EXISTING_PROJECT_CONTENT
BROWSER_DOM
BROWSER_CAPTURE
SCREEN_REGION
WINDOW_CAPTURE
IMPORTED_FILE
CLIPBOARD
MANUAL_TEXT
CAMERA
EXTERNAL_CONNECTOR
```

The Session stores intent/reference information.

Actual capture execution belongs to Capture/Acquisition capability.

---

# External Source Reference

External sources SHOULD use a provider-neutral reference.

Recommended:

```text
ContentSourceReference
├── sourceType
├── canonicalResourceIdentifier
├── externalVersion?
├── locator?
├── fingerprint?
├── adapterReference?
└── lastObservedAt?
```

Authentication secrets MUST NOT be embedded.

---

# Browser Source

Session MAY persist:

* canonical URL/resource identity,
* adapter identity,
* Chapter locator,
* DOM anchor,
* content fingerprint,
* stable resume hints.

Browser DOM nodes, tab objects and browser-process handles are ephemeral.

---

# Screen Capture Source

Session MAY store cross-restart capture intent such as:

* selected source type,
* normalized capture region policy,
* capture behavior.

Operating-system handles MUST remain device/runtime state.

---

# Language Selection

Session MAY express language intent for its workflow.

Recommended representation:

```text
SessionLanguageSelection
├── sourceLanguageOverride?
├── targetLanguage?
├── sourceLanguageConfirmation?
└── languagePreferenceReferences?
```

Session MUST use canonical Language values.

---

# Language Resolution Boundary

Session MUST NOT define the universal language resolution algorithm.

The Language domain / relevant consuming capability resolves:

* declared Project language,
* optional Book/Chapter language,
* TextBlock language,
* Session override,
* operation override,
* detection evidence.

Session contributes explicit Session-scoped intent.

It does not become the authority for all language truth.

---

# Target Language

Session MAY select one active target Language for MVP.

Changing target Language:

* affects future Translation selection/request,
* does not mutate old Translation Revisions.

Multi-target Sessions MAY be introduced later.

---

# Profile Selection

After the revised Profile domain, Session SHOULD store **Profile selection intent**, not copied Profile configuration.

Example:

```text
SessionProfileSelections
├── translation
├── ocr
├── presentation
├── context
├── validation
└── routing
```

Each selection MAY be:

```text
EXACT_REVISION
DEFAULT_SELECTION
LATEST_APPROVED_COMPATIBLE
```

For reproducibility, every operation MUST resolve dynamic selections to exact immutable revisions before execution.

---

# Exact Revision Pinning

For predictable Sessions, a selection MAY pin:

```text
profileId
profileRevisionId
```

Pinning is useful for:

* long reading sessions,
* review,
* experiments,
* publication work,
* offline processing.

---

# Session vs Resolved Configuration

Session stores selections and overrides.

It does NOT itself own the final effective operation configuration.

At operation start:

```text
Session Profile Selections
        +
Project / User Defaults
        +
Session Preferences
        +
Session Overrides
        +
Operation Overrides
        +
Mandatory Policy
        +
Capability Validation
        |
        v
ResolvedProfileSnapshot(s)
        |
        v
ResolvedConfigurationSnapshot
```

The operation consumes this immutable snapshot.

This aligns Session with the revised `PROFILE.md`.

---

# Session Preferences

Session Preferences are lightweight Session-scoped user choices.

Examples:

* show original,
* show Translation,
* auto-advance,
* auto-request Translation,
* pause on low confidence,
* highlight terminology warnings,
* prefer approved Translation.

Preferences describe user experience and selection behavior.

---

# Preference vs Profile

Examples:

```text
Session Preference:
    show original = true
```

```text
Presentation Profile:
    typography/layout policy
```

```text
Translation Profile:
    natural Vietnamese
```

These MUST remain distinct.

A Session Preference MAY affect which Profile or presentation mode is selected.

It SHOULD NOT duplicate complete Profile configuration.

---

# Mandatory Policy

Session Preferences and overrides MUST NOT bypass mandatory policy.

Example:

```text
Session:
    prefer cloud processing
```

```text
Policy:
    local only
```

Effective result:

```text
local only
```

or operation rejection.

---

# Session Override

A `SessionOverride` represents temporary user intent that applies only within a bounded Session scope.

Recommended structure:

```text
SessionOverride
├── overrideId
├── sessionId
├── overrideType
├── targetReference?
├── scope
├── value
├── priority?
├── createdBy
├── createdAt
├── expiresAt?
├── status
└── promotedReference?
```

---

# Override Is Intent

Session Override MUST NOT directly become authoritative state in another domain.

Examples:

```text
temporary term preference
```

does NOT equal:

```text
Glossary Entry mutation
```

```text
temporary speaker choice
```

does NOT equal:

```text
Speaker Attribution mutation
```

```text
temporary display font
```

does NOT equal:

```text
Presentation Profile mutation
```

---

# Override Scope

Possible scopes MAY include:

```text
SESSION
BOOK
CHAPTER
PAGE
TEXT_BLOCK
OPERATION
PRESENTATION_SURFACE
```

Character/Glossary-specific overrides MAY include target references but MUST remain Session-owned temporary intent.

Book/Page are optional scopes.

---

# Override Precedence

More specific valid Session overrides MAY outrank broader Session intent.

However:

```text
Mandatory Policy
    >
Session Override
```

and domain-confirmed truth MAY also outrank Session preference depending on the operation.

Resolution rules belong to the relevant resolver.

Session MUST NOT encode one universal precedence algorithm for every override type.

---

# Override Lifetime

Possible expiration:

* end of Session,
* Chapter transition,
* Page transition,
* one operation,
* explicit timestamp,
* manual removal,
* explicit promotion.

Expiration MUST be explicit.

---

# Override Promotion

Promotion is an application workflow.

Examples:

```text
Session terminology override
    ->
GlossaryCandidate / GlossaryEntryRevision
```

```text
Session speaker override
    ->
SpeakerAttributionRevision
```

```text
Session Character-name suggestion
    ->
CharacterCandidate / CharacterRevision proposal
```

```text
Session presentation preference
    ->
Profile/User Preference update
```

Promotion MUST be explicit.

Session MUST NOT mutate external aggregates automatically.

---

# Glossary Integration

Session MAY contribute:

* selected Glossary references,
* disabled/pinned terminology,
* temporary terminology overrides.

Operation-time terminology resolution produces:

```text
GlossarySnapshot
```

Translation references that exact immutable Snapshot.

The Session is NOT historical Glossary truth.

---

# Character Context Integration

Session MAY contribute temporary context such as:

* confirmed speaker override,
* listener selection,
* pinned Characters,
* scene cast,
* spoiler boundary.

Character/context resolution produces:

```text
CharacterContextSnapshot
```

Translation references the exact Snapshot.

Session state itself MUST NOT become historical Character truth.

---

# Context Snapshot Boundary

Critical rule:

```text
Mutable Session
    |
    v
Resolution
    |
    v
Immutable Context Snapshot(s)
    |
    v
Durable Output
```

Any Session state that materially affects durable output MUST cross an immutable snapshot boundary.

---

# Session Context Snapshot

A SessionContextSnapshot MAY be created for operation coordination or recovery.

Recommended:

```text
SessionContextSnapshot
├── snapshotId
├── sessionId
├── sessionVersion
├── contentLocation
├── Session language intent
├── Profile selection references
├── Session override references
├── presentation intent
├── spoiler boundary
├── createdAt
└── contentHash
```

It is immutable.

It does NOT replace:

* GlossarySnapshot,
* CharacterContextSnapshot,
* ResolvedConfigurationSnapshot.

---

# Operation Context

Operation context MAY include:

```text
OperationContext
├── sessionId?
├── sessionContextSnapshotId?
├── contentReferences
├── resolvedConfigurationSnapshotId
├── glossarySnapshotId?
├── characterContextSnapshotId?
├── correlationId
└── causationId
```

Later Session navigation MUST NOT alter this operation context.

---

# Session Reference Is Optional for Durable Artifacts

A durable domain artifact MAY retain `sessionId` for provenance/correlation.

But Session MUST NOT be required to reconstruct artifact meaning if all semantic inputs were captured into immutable snapshots.

This allows Session expiry/deletion without corrupting domain history.

---

# Operation References

Session MAY keep active/recent operation IDs.

Examples:

* Capture,
* OCR,
* Translation,
* Validation,
* Presentation,
* Import.

These are coordination references.

The operation lifecycle remains authoritative elsewhere.

---

# Operation Projection

For UI convenience Session MAY expose:

```text
QUEUED
RUNNING
WAITING
COMPLETED
FAILED
CANCELLED
```

This MUST be a projection of operation-owned state.

Session MUST NOT maintain a competing authoritative operation state machine.

---

# Pause vs Cancellation

```text
Pause Session
    !=
Cancel Operation
```

Pause MAY request cancellation or suppression of future automatic operations.

The final action depends on operation cancellation semantics.

---

# Review Session Boundary

A Review Session owns:

* selected review source/query,
* review navigation,
* current item,
* filters,
* deferred references,
* completion position.

It does NOT own the actual review decisions.

Example:

```text
Session
    currentItem: finding_44
```

Review domain:

```text
finding_44
    decision: APPROVED
```

---

# Review Queue

Recommended:

```text
SessionReviewQueue
├── queueType
├── sourceQueryOrReference
├── currentItem
├── filters
├── sortPolicy
├── deferredReferences
└── progressProjection
```

The queue SHOULD NOT necessarily copy every Review item into the Session.

It MAY be query-backed.

---

# Pending Decision

Pending decision references are workflow/navigation state.

Examples:

* OCR correction required,
* Translation review,
* glossary conflict,
* Character candidate,
* speaker attribution candidate,
* Presentation overflow.

The actual decision remains owned by its domain.

---

# Presentation State Boundary

Session MAY preserve resume-worthy presentation intent such as:

* display mode,
* source visibility,
* Translation visibility,
* theme,
* user font scaling,
* overlay enabled,
* active surface.

Detailed pixel layout belongs to Presentation runtime/output.

---

# Device-Independent Presentation State

Persist centrally only when meaningfully portable.

Example:

```text
displayMode: SIDE_BY_SIDE
```

may synchronize.

But:

```text
window width: 1374 px
```

normally remains device-local.

---

# Session Working Context

A Session MAY maintain a bounded working-context projection.

Examples:

* recent TextBlock references,
* recent Translation references,
* nearby Character references,
* recent terminology references,
* scene-summary reference,
* user-pinned context.

This provides continuity.

---

# Session Working Context Is Not Memory Truth

It MUST NOT be confused with:

* Project long-term memory,
* Character truth,
* Glossary truth,
* provider chat history,
* permanent AI memory.

Recommended representation:

```text
SessionWorkingContextEntry
├── entryId
├── sourceReferences
├── contextType
├── summaryReference?
├── scope
├── createdAt
├── expiresAt?
└── inclusionPriority
```

Raw copied content SHOULD be minimized.

---

# Context Budget

Working context SHOULD be bounded by:

* content distance,
* Chapter boundary,
* time,
* item count,
* configured context budget,
* explicit pinning.

Eviction from Session working context MUST NOT delete canonical domain resources.

---

# Provider Conversation Boundary

Provider conversation/thread IDs are runtime state.

```text
CRAI Session ID
    !=
Provider Conversation ID
```

One CRAI Session MAY use multiple providers and multiple provider conversations.

Changing provider MUST NOT change Session identity.

---

# Checkpoint

Checkpoint is a durable resume record.

Recommended:

```text
SessionCheckpoint
├── checkpointId
├── sessionId
├── sessionVersion
├── currentContext
├── resumePosition
├── profileSelections
├── languageSelections
├── activeOverrideReferences
├── reviewNavigation
├── presentationPreferences
├── createdAt
├── cause
└── contentHash
```

---

# Checkpoint Causes

Typical causes:

* meaningful navigation,
* Chapter transition,
* pause,
* device handoff,
* Session end,
* significant configuration change,
* explicit save.

Receiving every runtime result MUST NOT automatically require a checkpoint unless it affects resume state.

---

# Checkpoint Idempotency

Equivalent checkpoint requests SHOULD be idempotent.

The implementation MAY deduplicate checkpoints through content hash or operation identity.

---

# Recovery

Recovery restores the latest safe committed working state after:

* application crash,
* browser restart,
* device restart,
* connection interruption,
* temporary authentication expiry.

Uncommitted ephemeral UI state MAY be lost.

---

# Recovery Conflict

A recovery conflict MAY occur when:

* referenced content was superseded,
* Profile Revision is unavailable,
* Project access changed,
* external source moved,
* Session changed on another device.

Recovery MUST preserve the old reference and surface resolution.

It MUST NOT silently redirect to semantically different content.

---

# Multi-Device

Device-independent state MAY synchronize:

* logical position,
* Session selections,
* target language,
* Session overrides,
* review position,
* portable Presentation preferences.

Device-specific state MUST adapt.

Examples:

* browser window,
* local file path,
* capture handle,
* display scale,
* font availability,
* GPU capability.

---

# Device Context

Device Context SHOULD remain a separate record/projection.

Example:

```text
SessionDeviceContext
├── sessionId
├── deviceId
├── clientType
├── clientVersion
├── capabilities
├── localSourceReferences
├── lastActiveAt
└── syncState
```

Sensitive fingerprinting data SHOULD be avoided.

---

# Lease

A primary-client lease is coordination infrastructure.

It is NOT Session ownership.

Example:

```text
SessionLease
├── sessionId
├── clientId
├── leaseVersion
├── acquiredAt
└── expiresAt
```

MVP MAY support:

```text
one active writer
+
read-only observers
```

---

# Concurrency

Session updates SHOULD use optimistic concurrency.

Possible controls:

* expected aggregate version,
* expected checkpoint,
* update sequence,
* idempotency key,
* optional client lease.

Different fields MAY require different conflict policy.

---

# Fork

Fork creates a new Session identity for an alternative continuation.

Example:

```text
Session A
    Natural Translation

        |
        v fork

Session B
    Literal Translation
```

Fork MAY copy:

* working location,
* selections,
* preferences,
* overrides,
* review navigation.

It MUST preserve:

```text
parentSessionId
```

---

# Merge

Session merging SHOULD normally be avoided.

Session is interaction context, not canonical collaborative content.

Useful changes produced in different Sessions should be reconciled in their owning domains.

---

# Offline

Offline Session MAY support:

* local content,
* local OCR,
* local Translation,
* cached Profiles,
* cached immutable snapshots,
* local checkpoints,
* deferred synchronization.

Offline domain changes and Session state synchronize separately.

---

# Offline Sync Boundary

Synchronization MUST distinguish:

```text
Session working state
```

from:

```text
canonical domain changes
```

Example:

```text
Reading position
    -> Session/Progress reconciliation

Glossary Entry Revision
    -> Glossary reconciliation

Speaker Attribution
    -> Character/Speaker reconciliation
```

Session MUST NOT become the generic conflict owner for every offline domain resource.

---

# Cache

Session identity SHOULD rarely participate in reusable content cache identity.

Cache correctness SHOULD instead depend on semantic inputs such as:

* source revision,
* exact Language values,
* ResolvedConfigurationSnapshot,
* GlossarySnapshot,
* CharacterContextSnapshot,
* pipeline versions.

`sessionId` MAY still be used for:

* correlation,
* temporary in-flight deduplication,
* access filtering,
* projections.

---

# Durable Result Rule

Critical rule:

```text
Any Session-only information that affects durable output
must be captured into immutable reproducible input.
```

Once captured, the resulting domain artifact SHOULD remain valid even if the Session:

* changes,
* closes,
* expires,
* is archived,
* is deleted.

---

# Retention

Suggested retention categories:

Long-lived:

* Session identity,
* final lifecycle/end reason,
* major checkpoints,
* fork lineage,
* audit.

Medium-lived:

* bounded navigation,
* temporary overrides,
* review navigation,
* working context.

Short-lived:

* device hints,
* operation projections,
* connection state,
* local capture hints.

Retention policy SHOULD remain configurable.

---

# Deletion

Deleting a Session MUST NOT cascade into:

* Project,
* Book,
* Chapter,
* Page,
* TextBlock,
* Image,
* Translation,
* Glossary,
* Character,
* Review,
* Presentation artifacts.

Session deletion MAY remove:

* resume state,
* navigation,
* unpromoted overrides,
* Session working context,
* device hints.

---

# Tombstone

When needed:

```text
SessionTombstone
├── sessionId
├── ownerReference
├── projectId?
├── deletedAt
├── endReason?
└── retentionPolicy
```

Tombstone SHOULD minimize content details.

---

# Authorization

Session references MUST NOT grant authority.

Access MUST be revalidated when:

* resuming,
* navigating,
* starting operations,
* promoting overrides,
* opening referenced Project content.

---

# Access Revocation

When Project access disappears:

* Session use MUST fail appropriately,
* automatic operation generation SHOULD stop,
* credentials MUST NOT remain inside Session state,
* referenced domain artifacts follow their own retention policy.

The Session MAY be ended with:

```text
endReason: ACCESS_REVOKED
```

---

# Events

Core Session events MAY include:

```text
SessionCreated
SessionActivated
SessionPaused
SessionResumed
SessionEnded
SessionArchived

SessionContextChanged
SessionResumePositionChanged
SessionCheckpointCreated
SessionModeChanged
SessionLanguageSelectionChanged
SessionProfileSelectionChanged

SessionOverrideAdded
SessionOverrideChanged
SessionOverrideRemoved
SessionOverridePromoted

SessionReviewNavigationChanged
SessionForked
SessionRecovered
SessionRecoveryConflictDetected
SessionDeviceHandoffCompleted
```

High-frequency UI interaction MUST NOT emit ordinary domain events.

---

# Events vs Telemetry

Examples:

```text
SessionPaused
```

is domain state.

```text
mouseMoved
scrollVelocityChanged
frameRendered
```

are UI/runtime telemetry.

They MUST NOT share the same persistence assumptions.

---

# Persistence

Recommended canonical Session records:

```text
Session
SessionCheckpoint
SessionOverride
SessionNavigationEntry
SessionReviewNavigation
SessionForkLineage
SessionTombstone
```

Optional Session working-state records:

```text
SessionWorkingContextEntry
SessionStateSnapshot
SessionDeviceContext
```

Infrastructure/projections:

```text
SessionLease
SessionConnection
SessionDevicePresence
SessionOperationProjection
SessionTelemetry
LocalEphemeralState
```

---

# Suggested Session Record

```text
Session
├── sessionId
├── ownerType
├── ownerId?
├── projectId
├── sessionType
├── lifecycleStatus
├── endReason?
├── currentContext
├── resumePosition
├── profileSelections
├── languageSelections
├── sessionPreferences
├── mode
├── activeOverrideReferences
├── reviewNavigationReference?
├── presentationPreferences
├── latestCheckpointId?
├── parentSessionId?
├── createdAt
├── updatedAt
├── endedAt?
└── version
```

---

# Validation

Session validation SHOULD verify:

* valid Session identity,
* valid ownership,
* valid Project scope,
* compatible content references,
* referenced Revisions exist,
* selected Profile policies can be resolved,
* language selections are valid,
* Resume Position is resolvable,
* override targets are valid,
* override scope does not escape Session Project,
* lifecycle transitions are valid,
* Ended/Archived Sessions reject normal active mutations,
* fork lineage is acyclic,
* spoiler boundary is compatible with current location,
* authorization is revalidated externally.

---

# Errors

Possible stable Session errors include:

```text
SESSION_NOT_FOUND
SESSION_ACCESS_DENIED
SESSION_ENDED
SESSION_ARCHIVED
SESSION_VERSION_CONFLICT
SESSION_CONTEXT_INVALID
SESSION_POSITION_UNRESOLVABLE
SESSION_PROFILE_SELECTION_INVALID
SESSION_LANGUAGE_SELECTION_INVALID
SESSION_OVERRIDE_INVALID
SESSION_OVERRIDE_SCOPE_INVALID
SESSION_CHECKPOINT_STALE
SESSION_EXTERNAL_SOURCE_UNAVAILABLE
SESSION_RECOVERY_CONFLICT
SESSION_DEVICE_CONTEXT_INCOMPATIBLE
SESSION_SPOILER_BOUNDARY_VIOLATION
SESSION_OFFLINE_SYNC_CONFLICT
```

Coordination errors such as lease conflicts MAY remain infrastructure errors where appropriate.

---

# Architecture Invariants

1. Session is a resumable user working-context Aggregate Root.

2. Session is not an authentication session.

3. Session is not a provider conversation.

4. Session is not a runtime job.

5. Session references business resources but does not own them.

6. Session belongs to one Project in the MVP.

7. Optional Book and Page hierarchy levels MUST NOT be required.

8. Session identity remains stable across pause/resume and runtime failures.

9. Authentication credentials and provider secrets MUST NOT be stored in Session.

10. Durable Session state remains separate from ephemeral client state.

11. Logical content position remains separate from visual viewport state.

12. Session owns current/resume position, not canonical long-term reading history.

13. High-frequency UI state MUST NOT become canonical Session history.

14. Session events MUST represent meaningful domain changes rather than raw telemetry.

15. Profile selection intent MUST remain distinct from copied Profile configuration.

16. Dynamic Profile selections MUST resolve to exact Revisions before execution.

17. Session MUST NOT own ResolvedConfigurationSnapshot semantics.

18. Session language intent MUST NOT replace Language-domain truth.

19. Session MUST NOT define one universal Language resolution hierarchy.

20. Session Preferences MUST remain distinct from Profile and mandatory Policy.

21. Session Overrides are temporary intent.

22. Session Overrides MUST NOT silently mutate external aggregates.

23. Override promotion is explicit.

24. Glossary truth remains outside Session.

25. Character truth remains outside Session.

26. Speaker override is not canonical Speaker Attribution until promoted.

27. Review navigation remains separate from Review decision.

28. Presentation preferences MUST NOT modify Translation truth.

29. Session working context MUST NOT become canonical Project or Character memory.

30. Provider conversation identity MUST NOT become Session identity.

31. Any Session state affecting durable output MUST be captured into immutable reproducible snapshots.

32. Later Session mutations MUST NOT alter already-started operation context.

33. Durable artifacts MUST remain interpretable without mutable current Session state.

34. Session MAY reference operations but MUST NOT duplicate their authoritative lifecycle.

35. Pause and operation cancellation remain distinct concepts.

36. Session closing/ending MUST NOT delete domain artifacts.

37. Session deletion MUST NOT cascade into business content.

38. Access MUST be revalidated; Session reference does not grant authorization.

39. Device-specific handles are not durable cross-device identity.

40. Primary-client lease is infrastructure, not Session ownership.

41. Session checkpoints SHOULD be idempotent.

42. Session fork creates a new identity and preserves lineage.

43. Sessions SHOULD NOT normally merge.

44. Session ID SHOULD NOT unnecessarily prevent reusable content-cache hits.

45. Offline domain conflicts are resolved by their owning domains.

46. Recovery MUST preserve unresolved references rather than silently substitute incompatible content.

47. Spoiler boundaries MUST remain respected during context resolution.

48. Significant Session lifecycle and configuration transitions SHOULD be auditable.

---

# Recommended MVP Scope

The first CRAI MVP SHOULD support:

* stable Session identity,
* one Project per Session,
* user-owned Sessions,
* optional anonymous local Sessions,
* `READING`,
* `LIVE_TRANSLATION`,
* basic `REVIEW`,
* lifecycle:

  * `CREATED`,
  * `ACTIVE`,
  * `PAUSED`,
  * `ENDED`,
  * `ARCHIVED`,
* `endReason`,
* optional Book/Page context,
* Chapter/TextBlock context,
* logical Resume Position,
* periodic checkpoints,
* pause/resume,
* crash recovery,
* canonical source/target Language selections,
* exact Profile Revision pinning,
* Profile selection records,
* Translation/OCR/Presentation/Context/Validation selections,
* Manual/Assisted/Automatic mode,
* Browser DOM,
* Browser capture,
* screen capture,
* imported content,
* Session Preferences,
* temporary terminology override,
* temporary speaker override,
* explicit override promotion,
* immutable operation snapshots,
* Review navigation,
* portable Presentation preferences,
* bounded navigation history,
* bounded working context,
* one primary active writer,
* optimistic concurrency,
* basic device handoff,
* Session fork,
* lifecycle events,
* audit,
* selective retention,
* deletion without content cascade.

MVP MAY defer:

* collaborative Session,
* multiple active writers,
* cross-Project Session,
* multi-target Translation,
* persistent provider conversations,
* advanced Session working memory,
* automatic override promotion,
* advanced offline merge,
* shared review queues,
* live presence,
* Session chat,
* synchronized viewport/mouse state,
* full browser-tab restoration,
* cross-device screen-capture restoration,
* Session merge,
* advanced analytics,
* complex lease negotiation,
* Workspace-owned Sessions,
* public shared Sessions,
* long-term Annotation ownership,
* advanced branching.

---

# Open Decisions

The following SHOULD remain open until prototype validation:

* whether Session state is always server-persisted,
* whether anonymous Sessions synchronize,
* whether several active Sessions per Project are allowed,
* whether Ended Session may resume directly or must Fork,
* exact end-reason taxonomy,
* Session revision persistence strategy,
* event sourcing vs checkpoint model,
* checkpoint frequency,
* navigation-history persistence,
* whether long-term ReadingProgress becomes a dedicated domain,
* whether Review queue is query-backed or snapshot-backed,
* whether bounded Session working context is persisted,
* scene-summary generation,
* Session context budget,
* supported Session override types,
* override precedence per target domain,
* whether any override may outlive Session before promotion,
* Profile-selection persistence model,
* whether exact Revision pinning is default,
* whether Presentation Profile selection belongs partly in user preferences,
* portable vs device-specific Presentation state,
* target-language change behavior,
* multi-target future model,
* basic offline support scope,
* device-handoff synchronization,
* whether SessionContextSnapshot is always persisted,
* ResolvedConfigurationSnapshot retention,
* Session fork requirements,
* whether Session types can change,
* whether `MIXED` is necessary,
* tombstone policy,
* Session analytics privacy policy.

---

# Ownership Summary

```text
Session owns
    stable working-context identity
    Project scope
    lifecycle
    current logical context
    resume position
    Profile selections
    Language selections
    Session Preferences
    temporary override references
    bounded navigation state
    review navigation
    portable presentation preferences
    checkpoints
    fork lineage

Session contributes to
    ResolvedConfigurationSnapshot
    GlossarySnapshot
    CharacterContextSnapshot
    OperationContext

Session references
    Project
    optional Book
    Chapter
    optional Page
    optional TextBlock
    Translation
    Profiles
    Review resources

does not own
    authentication
    long-term reading truth
    OCR execution
    Translation execution
    Review decisions
    Glossary truth
    Character truth
    Profile definitions
    provider runtime
    Presentation artifacts
    device leases
```

Session is therefore the durable-but-bounded **user working-context domain**, not a container for processing state or content truth.

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
* `WORKSPACE.md`

Architecture:

* `docs/architecture/CAPABILITY_MAP.md`
* `docs/architecture/OWNERSHIP_MAP.md`
* `docs/architecture/DATA_FLOW.md`
* `docs/architecture/STATE_MACHINE.md`
* `docs/architecture/EVENT_BUS.md`
* `docs/architecture/MODULE_DEPENDENCY.md`

Module contracts remain authoritative for runtime execution, reading-session behavior, capture, Translation, OCR, Presentation and review workflows.
