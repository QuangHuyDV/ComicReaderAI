# Session Domain

* **Document:** Domain / Session
* **Version:** 1.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

The Session domain defines how CRAI represents a bounded period in which a user reads, captures, translates, reviews or presents content.

A Session preserves the working context required to continue an interrupted reading or translation workflow without treating temporary runtime state as permanent domain truth.

Session may preserve:

* Current Project
* Current Book and Chapter
* Current Page
* Reader position
* Active content source
* Source and target languages
* Selected Translation Profile
* Selected Presentation Profile
* Temporary glossary overrides
* Temporary character context
* Display mode
* Capture mode
* Pending user review
* Navigation history
* Resume information
* Device or client context
* References to active processing operations

The Session domain must remain independent from:

* HTTP sessions
* Authentication tokens
* Browser cookies
* WebSocket connections
* AI provider conversations
* Runtime processes
* Background workers
* In-memory caches
* Operating-system windows

---

# Domain Role

Session coordinates a user-facing working context.

```text
User
  │
  ▼
Session
  ├── Content Context
  ├── Reading Position
  ├── Language Selection
  ├── Profile Selection
  ├── Temporary Overrides
  ├── Presentation State
  ├── Review State
  └── Resume State
```

Session references domain objects but does not own them.

```text
Session
├── Project Reference
├── Book Reference
├── Chapter Reference
├── Page Reference
├── Text Block References
├── Translation References
├── Glossary Snapshot Reference
├── Character Context Reference
└── Profile Revision References
```

A Session may coordinate actions across several aggregates while remaining a separate aggregate itself.

---

# Session Is Not Authentication

Authentication answers:

> Who is allowed to access the system?

Session answers:

> What content and working context is the user currently using?

These are different concepts.

```text
Authentication Session
≠
CRAI Reading Session
```

An authentication token may expire while a CRAI Session remains resumable.

A user may sign in from another device and reopen an existing CRAI Session.

Authentication credentials must never be stored inside the Session aggregate.

---

# Session Is Not Runtime Execution

A Session is not:

* Translation Job
* OCR Job
* Capture Process
* Browser Process
* Provider Request
* Queue Consumer
* Worker Lease
* WebSocket Connection
* AI conversation thread
* Cache entry

Runtime operations may reference a Session for correlation.

The Session must not become dependent on the lifetime of those operations.

```text
Session
    │
    ├── starts Translation Operation
    ├── starts OCR Operation
    └── receives Operation Result
```

When a worker restarts, the Session remains valid.

When a Session closes, historical Translation and OCR results remain valid.

---

# Session Is Not Business Content

Session does not own:

* Book content
* Chapter content
* Page images
* TextBlock text
* Translation truth
* Glossary truth
* Character identity
* Language definitions
* Presentation artifacts

Session stores references to exact identities or revisions where reproducibility matters.

---

# Aggregate Boundary

Session should be modeled as an Aggregate Root.

```text
Session Aggregate
├── Session
├── Session Context
├── Reading Position
├── Session Preferences
├── Temporary Overrides
├── Navigation State
├── Review Queue State
├── Resume State
└── Lifecycle State
```

The aggregate owns:

* Session identity
* Session lifecycle
* Current content location
* Selected working profiles
* Temporary Session-level overrides
* User navigation context
* Resume state
* Device-independent working state
* Session-level optimistic concurrency

The aggregate does not own:

* Durable content aggregates
* Runtime job state
* Provider request history
* Browser DOM
* Image capture buffers
* Translation results
* OCR results
* Global user preferences
* Device-specific secrets

---

# Responsibilities

The Session domain is responsible for:

* Creating a resumable reading or translation context
* Tracking the active Project and content location
* Tracking reader progress during the Session
* Selecting source and target languages
* Selecting exact Profile Revisions
* Selecting applicable glossary and character context
* Managing temporary Session-level overrides
* Preserving presentation and interaction mode
* Coordinating pending review state
* Supporting pause, resume and close
* Supporting recovery after interruption
* Supporting multi-device continuation
* Emitting Session lifecycle events
* Maintaining Session revision consistency
* Providing correlation context to application workflows

The Session domain is not responsible for:

* Authenticating users
* Authorizing every referenced aggregate
* Executing OCR
* Executing Translation
* Managing provider retries
* Maintaining WebSocket connections
* Persisting browser cookies
* Downloading remote content
* Rendering overlays
* Capturing screenshots
* Scheduling workers
* Storing provider chat memory

---

# Session Identity

Each Session has a stable identifier.

```text
Session ID
```

Session identity persists through:

* Pause and resume
* Temporary disconnection
* Application restart
* Device change
* Profile changes
* Navigation changes
* Runtime operation failures

A Session should receive a new identity when the user intentionally starts a separate working context.

---

# Session Revision

Session state changes frequently.

The system may represent this using:

* Aggregate version
* Immutable Session revisions
* Event-sourced state
* Periodic snapshots plus events

Recommended conceptual model:

```text
Session Revision
├── Session Revision ID
├── Session ID
├── Revision Number
├── Current Context
├── Current Position
├── Selected Profiles
├── Temporary Overrides
├── Presentation State
├── Parent Revision
├── Changed By
├── Changed At
└── Content Hash
```

Unlike Translation Revision, not every pointer movement needs a long-term immutable business revision.

The implementation may compact high-frequency navigation updates while preserving significant Session transitions.

---

# Durable and Ephemeral Session State

Session state should be classified into two categories.

## Durable Session State

Should normally survive application restart:

* Session ID
* User or owner reference
* Project reference
* Book reference
* Chapter reference
* Current logical Page
* Reading progress
* Source and target language
* Profile revision selections
* Temporary glossary overrides
* Temporary character overrides
* Review queue position
* Session mode
* Pause state
* Resume checkpoint

## Ephemeral Client State

May remain local to a device or process:

* Current mouse position
* Hovered TextBlock
* Animation progress
* Open tooltip
* Temporary drag position
* Active network connection
* Unsubmitted text selection
* UI focus
* Current scroll velocity
* In-memory image bitmap
* Uncommitted capture buffer

Ephemeral state may be checkpointed when useful, but it is not canonical Session truth by default.

---

# Session Ownership

A Session should identify its owner.

Possible ownership models:

* User-owned
* Workspace-owned
* Anonymous local
* Shared collaborative
* System-created

Recommended MVP ownership:

```text
Session
├── Owner User ID
└── Project ID
```

Anonymous local Sessions may use a local installation identity rather than an authenticated account.

---

# Session Scope

A Session may be scoped to:

* Project
* Book
* Chapter
* Page
* External reading source
* Imported document
* Review workflow
* Translation workflow

Recommended default:

```text
Session belongs to one Project.
```

A Session may navigate among Books within the same Project if project policy permits it.

Cross-Project navigation should normally create a new Session or require an explicit context switch.

---

# Session Types

Recommended Session Types:

* Reading
* Live Translation
* Batch Translation Review
* OCR Review
* Glossary Review
* Character Review
* Presentation Preview
* Import Review
* Editing
* Mixed
* Custom

Session Type describes the primary user workflow.

It does not restrict the Session from invoking related capabilities unless policy says otherwise.

---

# Reading Session

A Reading Session focuses on continuity and minimal interruption.

It may preserve:

* Current page or paragraph
* Reading direction
* Overlay visibility
* Translation display mode
* Zoom
* Font size
* Last approved Translation
* Current chapter
* Read progress
* Recently visited locations

---

# Live Translation Session

A Live Translation Session follows content as the user reads.

It may coordinate:

* Browser text extraction
* Screen capture
* Page detection
* OCR requests
* Translation requests
* Overlay presentation
* Automatic navigation
* Context updates

The Session stores configuration and progress.

Runtime capture and translation pipelines remain outside the aggregate.

---

# Review Session

A Review Session focuses on a bounded review queue.

It may preserve:

* Review type
* Ordered item references
* Current review item
* Filters
* Decisions made
* Deferred items
* Completion progress

Review decisions belong to their relevant domain aggregates.

Session only tracks navigation and outstanding work.

---

# Session Lifecycle

Recommended lifecycle states:

```text
Created
→ Active
→ Paused
→ Active
→ Completed
→ Closed
```

Alternative terminal states:

```text
Abandoned
Expired
Archived
```

Possible lifecycle:

```text
Created
  │
  ▼
Active
  ├── Paused ──────┐
  │                 │
  │◄────────────────┘
  │
  ├── Completed
  ├── Abandoned
  ├── Expired
  └── Closed
```

---

# Created State

A newly created Session has:

* Stable Session ID
* Owner
* Session Type
* Initial Project context
* Initial language configuration
* Initial profile selections
* Creation time

It may not yet have a content position.

---

# Active State

An Active Session may:

* Navigate content
* Start application operations
* Update working context
* Save progress
* Create checkpoints
* Receive processing results
* Add temporary overrides

Only one client may hold the primary editing lease in the MVP, though several clients may read the Session.

---

# Paused State

A Paused Session preserves resumable state but should not continue automatic processing unless explicitly configured.

Pause may stop:

* Auto capture
* Auto navigation
* Automatic OCR
* Automatic Translation
* Overlay updates
* Reading timers

Pause does not cancel already persisted domain results.

---

# Completed State

Completed means the Session’s intended workflow has finished.

Examples:

* Chapter fully reviewed
* Reading task completed
* Translation review queue exhausted
* Import review completed

Completed Sessions may remain reopenable according to policy.

---

# Closed State

Closed means the Session is no longer intended for active continuation.

Closing should:

* Save the latest checkpoint
* Release runtime leases
* Stop automatic operations
* Preserve referenced domain results
* Record the close reason
* Emit a lifecycle event

Closing a Session must not delete content, Translations or review decisions.

---

# Abandoned State

Abandoned indicates that the user intentionally stopped the workflow without completion.

The Session remains available for:

* Audit
* Analytics
* Recovery
* Reference

Abandoned state should not imply that created domain artifacts are invalid.

---

# Expired State

Expiration may apply to inactive or temporary Sessions.

Expiration policy may depend on:

* Anonymous ownership
* Local storage limits
* Workspace policy
* Last activity
* Security requirements

Expiration should affect the Session container, not referenced durable domain objects.

---

# Archived State

Archived Sessions remain persisted but are hidden from ordinary active-session lists.

Archiving may follow:

* Completion
* Closure
* Retention policy
* User action

---

# Current Context

Current Context identifies where the Session is operating.

Recommended structure:

```text
Session Context
├── Project ID
├── Book ID
├── Chapter ID
├── Page ID
├── Page Revision or Image Version
├── TextBlock ID
├── TextBlock Revision
├── External Source Reference
├── Context Type
└── Updated At
```

Not every field is required in every Session.

A novel Session may use Chapter and paragraph position without Page.

A browser-based comic Session may use an external source reference and captured Page identity.

---

# Content Location

A content location should use stable domain identifiers whenever available.

Examples:

```text
Project / Book / Chapter / Page
```

```text
Project / Book / Chapter / TextBlock
```

```text
External Source / Resource / DOM Locator
```

```text
Captured Document / Page / Image Region
```

A Session should not rely solely on temporary screen coordinates.

---

# Reading Position

Reading Position represents where the user should resume.

Recommended structure:

```text
Reading Position
├── Position Type
├── Content Reference
├── Logical Order
├── Offset
├── Viewport Anchor
├── Progress Fraction
├── Reading Direction
├── Captured At
└── Confidence
```

Possible Position Types:

* Page
* TextBlock
* Paragraph
* Sentence
* Character Offset
* Image Region
* DOM Locator
* Scroll Anchor
* Review Item
* Timeline Point

---

# Logical and Visual Position

Logical content position and visual viewport position must remain separate.

```text
Logical Position
= Chapter 12, Page 8, TextBlock 4
```

```text
Visual Position
= scroll offset 1,428 px, zoom 125%
```

Logical position is more durable.

Visual position may be device-specific and should be treated as a resume hint.

---

# Reader Progress

Reader progress may be expressed as:

* Current chapter
* Current page
* Highest completed page
* Fraction of Book
* Fraction of Chapter
* Last confirmed location
* Read item set

Session progress is working state.

Long-term reading history may belong to a separate Progress or Library domain.

Session may publish progress updates to that domain.

---

# Progress Commit

High-frequency navigation should not require a durable write for every pixel.

Recommended progress flow:

```text
Client Navigation
       │
       ▼
Ephemeral Position
       │
       ├── periodic checkpoint
       ├── meaningful page transition
       ├── pause
       ├── close
       └── device handoff
       ▼
Committed Reading Position
```

The checkpoint frequency is an application policy.

---

# Navigation History

Session may preserve recent navigation history.

Recommended structure:

```text
Navigation Entry
├── Content Location
├── Entered At
├── Left At
├── Navigation Cause
└── Resume Eligibility
```

Possible causes:

* Next Page
* Previous Page
* Search Result
* Glossary Reference
* Character Reference
* Review Finding
* External Link
* Resume
* User Jump
* Automatic Follow

Navigation history should be bounded.

It is not intended to become a complete analytics log.

---

# Navigation Stack

A Session may preserve:

* Back stack
* Forward stack
* Recent locations
* Pinned locations

Navigation stack is Session-owned interaction state.

It must not alter the canonical ordering of Book, Chapter or Page aggregates.

---

# Session Mode

Session Mode controls the current interaction behavior.

Recommended values:

* Manual
* Assisted
* Automatic
* Review
* Presentation
* Read Only

## Manual

User explicitly initiates capture, OCR and Translation.

## Assisted

The system detects likely content and proposes actions.

## Automatic

The system may capture and process content according to configured policies.

## Review

The Session prioritizes findings and decisions.

## Presentation

The Session focuses on rendering existing Translations.

## Read Only

The Session cannot modify domain truth.

---

# Capture Mode

Capture Mode describes where source content comes from.

Recommended values:

* Imported File
* Browser DOM
* Browser Screenshot
* Screen Region
* Window Capture
* Clipboard
* Camera
* Manual Text
* Existing Project Content
* External Connector
* Custom

Capture Mode is Session configuration.

The actual capture implementation belongs to a Capture capability.

---

# Content Source Reference

External content sources should be represented by provider-neutral references.

Recommended structure:

```text
Content Source Reference
├── Source Type
├── Canonical Resource Identifier
├── External Resource Version
├── Access Context Reference
├── Locator
├── Last Observed At
└── Fingerprint
```

Sensitive credentials must not be embedded in this value.

---

# Browser Source

For browser-based reading, Session may store:

* Canonical page URL or resource identifier
* Site adapter identifier
* Chapter locator
* DOM anchor
* Content fingerprint
* Scroll hint
* Last detected Page identity

Browser DOM objects themselves are ephemeral and must not be persisted as canonical Session data.

---

# Screen Capture Source

For screen capture, Session may preserve:

* Selected display or window reference
* Capture region policy
* Region normalized coordinates
* Detection mode
* Last successful content fingerprint
* Device-specific resume hint

Operating-system handles are ephemeral and should not be treated as durable cross-device identity.

---

# Language Configuration

Session selects the languages active for the current workflow.

Recommended structure:

```text
Session Language Configuration
├── Configured Source Language
├── Detected Source Language
├── Confirmed Source Language
├── Target Language
├── Translation Language Pair
├── OCR Language Profile
└── Fallback Policy
```

Language values must use the canonical Language model.

Provider-specific language codes remain inside adapters.

---

# Source Language Resolution

Recommended Session-level resolution:

```text
Operation Override
        ↓
TextBlock Confirmed Language
        ↓
Session Confirmed Language
        ↓
Project Configured Language
        ↓
Detected Language
        ↓
und
```

Session configuration should not overwrite confirmed TextBlock language truth.

---

# Target Language

A Session normally has one active target language.

Possible future support:

* Several target languages
* Parallel translations
* Comparison mode
* Original-only mode

Changing the Session target language does not mutate existing Translation Revisions.

It changes which Translation is selected or requested next.

---

# Translation Profile Selection

A Session should reference an exact Translation Profile Revision.

```text
Translation Profile ID
+
Translation Profile Revision ID
```

The Profile may define:

* Translation style
* Formality
* Literalness
* Localization policy
* Provider routing preferences
* Validation policy
* Context policy

The Session must not copy mutable Profile data without revision identity.

---

# Presentation Profile Selection

A Session may reference an exact Presentation Profile Revision.

It may define:

* Font preferences
* Font size
* Line spacing
* Bubble fitting policy
* Novel layout
* Overlay opacity
* Original-text visibility
* Translation placement
* Reading theme

Presentation Profile is separate from Translation Profile.

---

# OCR Profile Selection

A Session may select an OCR Profile Revision.

It may define:

* Languages
* Detection mode
* Confidence thresholds
* Layout assumptions
* Comic or novel mode
* Vertical-text support
* Preprocessing policy

The Session does not store provider-specific OCR configuration as canonical domain data.

---

# Session Preferences

Session Preferences are temporary or Session-specific selections.

Examples:

* Show original text
* Show Translation
* Show both
* Auto translate
* Auto advance
* Pause on low confidence
* Highlight terminology
* Show character names
* Show validation warnings
* Prefer approved Translations
* Use local processing only

Session Preferences override broader defaults only for this Session.

---

# Preference Resolution

Recommended hierarchy:

```text
Operation Override
        ↓
Session Preference
        ↓
Book Preference
        ↓
Project Preference
        ↓
User Preference
        ↓
Application Default
```

The resolved value and its source should be inspectable.

---

# Temporary Override

A Temporary Override modifies behavior within the Session without immediately changing durable Project truth.

Examples:

* Use a different target name for one character
* Preserve one term
* Force one source language
* Select a different Translation Profile
* Disable automatic OCR
* Treat one TextBlock as narration
* Pin a speaker candidate
* Use an alternative font

Recommended structure:

```text
Session Override
├── Override ID
├── Override Type
├── Target Reference
├── Value
├── Scope
├── Priority
├── Created By
├── Created At
├── Expires At
└── Promotion State
```

---

# Override Scope

Possible override scopes:

* Entire Session
* Book
* Chapter
* Page
* TextBlock
* Translation Operation
* Character
* Glossary Entry
* Presentation Surface

More specific overrides should take precedence.

---

# Override Lifetime

An override may expire:

* At Session close
* At Chapter change
* At Page change
* After one operation
* At explicit time
* When promoted to durable truth
* When manually removed

Expiry policy must be explicit.

---

# Override Promotion

A useful Session override may be promoted into another domain.

Examples:

```text
Session term override
→ Glossary Candidate or Glossary Entry Revision
```

```text
Session speaker override
→ Speaker Attribution Revision
```

```text
Session character-name override
→ Character Revision Candidate
```

```text
Session presentation override
→ User or Project Profile update
```

Promotion requires an explicit application operation.

The Session must not mutate external aggregates automatically.

---

# Glossary Integration

A Session may select:

* Project Glossary
* Specific Glossary Revision
* Glossary Snapshot
* Session-only glossary overrides
* Disabled glossary entries
* Pinned entries

Recommended context flow:

```text
Project Glossary Revisions
          +
Session Overrides
          +
Operation Scope
          ↓
Resolved Glossary Snapshot
```

The Translation Revision references the exact resulting Glossary Snapshot.

The Session itself should not become the historical source of terminology truth.

---

# Character Context Integration

A Session may preserve:

* Active cast
* Confirmed speaker
* Speaker candidates
* Current listener
* Pinned characters
* Temporary character overrides
* Spoiler boundary
* Character context policy

These inputs contribute to a Character Context Snapshot.

The Translation Revision references the exact snapshot used.

---

# Context Snapshot Boundary

Session context is mutable.

Translation context must be immutable.

Therefore:

```text
Mutable Session State
        │
        ▼
Context Resolution
        │
        ▼
Immutable Context Snapshots
        │
        ▼
Translation Revision
```

A Translation must never rely only on “current Session state” after execution.

---

# Session Context Snapshot

For audit or operation coordination, the application may create a Session Context Snapshot.

Recommended structure:

```text
Session Context Snapshot
├── Snapshot ID
├── Session ID
├── Session Revision
├── Content Location
├── Language Configuration
├── Profile Revision References
├── Override References
├── Glossary Snapshot Reference
├── Character Context Snapshot Reference
├── Presentation Context
├── Created At
└── Content Hash
```

This snapshot is distinct from the mutable Session aggregate.

---

# Operation Context

Each processing operation should capture the relevant Session context at start.

```text
Operation Context
├── Session ID
├── Session Revision
├── Session Context Snapshot ID
├── User ID
├── Content References
├── Correlation ID
└── Causation ID
```

Later Session navigation must not change an already-started operation’s context.

---

# Operation References

Session may track active or recent operation references:

* Capture Operation ID
* OCR Operation ID
* Translation Operation ID
* Validation Operation ID
* Rendering Operation ID
* Import Operation ID

These are coordination references.

The authoritative operation lifecycle belongs to runtime or application workflow models.

---

# Active Operation State

Session may show summarized operation states:

* Queued
* Running
* Waiting
* Completed
* Failed
* Cancelled

This state should be projected from operation events.

It should not duplicate the complete operation aggregate.

---

# Cancellation

Cancelling a Session operation and closing a Session are different actions.

```text
Cancel Translation Operation
≠
Close Session
```

Closing a Session may request cancellation of cancellable operations, but already completed domain artifacts remain preserved.

---

# Review Queue

A Session may coordinate a review queue.

Recommended structure:

```text
Session Review Queue
├── Queue Type
├── Query or Source Reference
├── Ordered Item References
├── Current Item
├── Completed Count
├── Deferred Items
├── Filters
└── Sort Policy
```

The queue may include:

* Low-confidence OCR
* Stale Translations
* Terminology findings
* Character conflicts
* Speaker attribution candidates
* Layout overflows
* Import conflicts

Review decisions belong to the affected domain.

---

# Pending Decision

Session may keep references to pending decisions.

Examples:

* Choose Translation alternative
* Confirm OCR text
* Confirm character identity
* Resolve glossary conflict
* Approve speaker
* Fix layout overflow

Pending Decision is navigational workflow state.

The underlying issue or candidate belongs to its responsible domain.

---

# Presentation State

Presentation State describes how content is currently shown.

Recommended structure:

```text
Presentation State
├── Display Mode
├── Active Surface
├── Zoom
├── Theme
├── Original Visibility
├── Translation Visibility
├── Overlay Mode
├── Font Scale
├── Layout Mode
├── Reading Direction
└── Device View Hint
```

Only device-independent settings should normally be persisted centrally.

---

# Display Modes

Recommended display modes:

* Original Only
* Translation Only
* Side by Side
* Interleaved
* Overlay
* Replacement
* Hover Translation
* Focused Text
* Review Comparison

Display Mode does not alter Translation truth.

---

# Overlay State

For comic reading, overlay state may include:

* Overlay enabled
* Original hidden or visible
* Bubble replacement mode
* TextBlock selection
* Opacity
* Debug geometry visibility
* Low-confidence indicator
* Overflow indicator

Precise pixel layout belongs to presentation runtime.

The Session preserves only meaningful resume preferences.

---

# Session Memory

Session Memory is a bounded set of working context used to maintain continuity.

It may include:

* Recent TextBlocks
* Recent Translations
* Recent character mentions
* Recent glossary terms
* Dialogue window
* Current scene summary
* User-pinned context

Session Memory must not be confused with:

* Provider chat memory
* Long-term Project memory
* Canonical Book facts
* Character truth
* Glossary truth

---

# Memory Entries

Recommended structure:

```text
Session Memory Entry
├── Memory Entry ID
├── Memory Type
├── Source References
├── Content or Summary Reference
├── Scope
├── Confidence
├── Created At
├── Expires At
└── Inclusion Policy
```

Possible types:

* Recent Source Text
* Recent Translation
* Scene Summary
* Character Mention
* Terminology Mention
* Dialogue Context
* User Note
* Processing Hint

---

# Memory Window

Session Memory should be bounded by:

* Number of Pages
* Number of TextBlocks
* Number of tokens
* Time
* Chapter boundary
* Scene boundary
* User pinning

Old entries may be evicted from active context while remaining available in durable Project data.

---

# AI Conversation Boundary

An AI provider may expose a conversation or thread identifier.

That identifier belongs to provider execution state.

It must not become Session identity.

```text
CRAI Session ID
≠
Provider Conversation ID
```

The application may associate several provider conversations with one Session.

A Session may switch providers without losing domain continuity.

---

# Resume State

Resume State contains the minimum durable information needed to continue the Session.

Recommended structure:

```text
Resume State
├── Session ID
├── Last Committed Position
├── Current Context
├── Selected Profile Revisions
├── Active Overrides
├── Review Queue Position
├── Presentation Preferences
├── Last Activity At
├── Last Device Reference
└── Resume Token Version
```

Resume State must not contain authentication secrets.

---

# Resume Checkpoint

A checkpoint may be created when:

* Entering a new Page
* Entering a new Chapter
* Pausing
* Closing
* Losing connection
* Switching devices
* Completing a review decision
* Applying a major override
* Receiving a Translation result

Checkpoints should be idempotent.

---

# Recovery

Recovery restores a Session after:

* Application crash
* Browser restart
* Device restart
* Network interruption
* Worker failure
* Client update
* Temporary authentication expiry

Recovery should restore the last committed domain-safe checkpoint.

Uncommitted ephemeral UI actions may be lost.

---

# Recovery Conflict

A recovery conflict may occur when:

* Content changed since checkpoint
* Referenced Page Revision was superseded
* Profile Revision was archived
* Session resumed on several devices
* Project access changed
* External source moved
* Local capture source is unavailable

The Session should preserve the old reference and request a resolution strategy rather than silently switching context.

---

# Multi-Device Session

A Session may be resumed on another device.

Device-independent state may synchronize:

* Logical position
* Profiles
* Overrides
* Review progress
* Display mode
* Target language

Device-specific state may require adaptation:

* Window handle
* Screen region
* Browser tab
* Font availability
* Local file path
* Display scale
* GPU capability

---

# Device Context

Recommended structure:

```text
Session Device Context
├── Device ID
├── Client Type
├── Application Version
├── Capability Profile
├── Local Source References
├── Last Active At
└── Sync State
```

Device Context should be a separate record or value referenced by Session activity.

The Session aggregate should not store sensitive device fingerprints unnecessarily.

---

# Primary Client Lease

To avoid conflicting updates, an Active Session may use a short-lived primary client lease.

```text
Session Lease
├── Session ID
├── Client ID
├── Lease Version
├── Acquired At
├── Expires At
└── Renewal Token Reference
```

The lease belongs to coordination infrastructure.

It is not the same as Session ownership.

---

# Concurrent Access

Possible access modes:

* One active editor
* Several read-only viewers
* Explicit handoff
* Optimistic concurrent editing
* Collaborative editing

Recommended MVP:

```text
One primary active client
+
Optional read-only observers
```

---

# Concurrent Update Resolution

Session updates should use optimistic concurrency.

Possible inputs:

* Session aggregate version
* Expected Session Revision
* Update sequence
* Client lease
* Idempotency key

Conflicting updates may be resolved by field semantics.

Examples:

* Latest logical navigation may win
* Review decisions must never be overwritten
* Overrides require merge or conflict handling
* Profile changes require explicit ordering

---

# Device Handoff

Handoff should:

1. Commit current position
2. Create a checkpoint
3. Release the primary lease
4. Transfer or reacquire active control
5. Adapt device-specific settings
6. Preserve logical context

A handoff must not duplicate processing operations accidentally.

---

# Session Fork

A user may fork a Session to try another configuration.

Example:

```text
Original Session:
zh-Hans → vi
Profile: Natural Vietnamese

Forked Session:
zh-Hans → vi
Profile: Literal Comparison
```

Forking creates a new Session ID.

It may copy:

* Content position
* Context selections
* Profile references
* Overrides
* Review queue

It must preserve lineage to the source Session.

---

# Session Clone and Fork

Clone and Fork may have different semantics.

## Clone

Copies Session configuration for reuse without preserving workflow lineage.

## Fork

Creates a deliberate alternative continuation and records parent Session identity.

The MVP may support only Fork.

---

# Session Merge

Merging Sessions should generally be avoided.

Session state is interaction context, not canonical collaborative content.

When two Sessions produce useful domain changes, those changes should merge in their owning aggregates.

Possible mergeable Session elements:

* Reading progress
* Review queue completion
* Promotable overrides
* User notes

The Session aggregate itself should normally remain separate.

---

# Idempotency

Session operations should support idempotency.

Examples:

* Create Session
* Commit checkpoint
* Pause
* Resume
* Close
* Apply override
* Remove override
* Select Profile Revision
* Update reading position
* Fork Session

Possible idempotency keys:

* Client operation ID
* Session ID
* Expected version
* Checkpoint hash
* Override content hash

---

# High-Frequency Updates

High-frequency state such as scroll position should be debounced or compacted.

Recommended strategy:

```text
Client Updates
    │
    ▼
Local Ephemeral State
    │
    ├── debounce
    ├── threshold crossing
    ├── page transition
    └── lifecycle checkpoint
    ▼
Durable Session Update
```

This prevents the Session event stream from becoming a raw interaction log.

---

# Session Activity

Session Activity may record meaningful actions:

* Session opened
* Chapter entered
* Page entered
* Translation requested
* Review completed
* Override applied
* Session paused
* Session resumed
* Session closed

Fine-grained telemetry should be stored separately from domain events.

---

# Session Events and Telemetry

Domain events communicate meaningful state transitions.

Telemetry measures runtime and user interaction.

```text
SessionPaused
```

is a domain event.

```text
MouseMoved
```

is telemetry or local UI state.

These must not share the same persistence and retention assumptions.

---

# Session Expiration Policy

Expiration should consider:

* Session state
* Ownership type
* Last activity
* Presence of unresolved review work
* Active overrides
* Anonymous status
* Workspace policy

Recommended behavior:

* Active Sessions do not expire unexpectedly.
* Paused anonymous Sessions may expire sooner.
* Completed Sessions may archive rather than expire.
* Referenced domain artifacts remain preserved.

---

# Retention

Retention policy may distinguish:

## Long-Lived

* Session identity
* Final status
* Major checkpoints
* Promoted decisions
* Audit records
* Fork lineage

## Medium-Lived

* Navigation history
* Review queue state
* Session Memory
* Temporary overrides

## Short-Lived

* Device hints
* Connection state
* Operation projections
* Hover and viewport details
* Temporary capture configuration

Retention should be configurable.

---

# Deletion

Deleting a Session must not delete:

* Projects
* Books
* Chapters
* Pages
* TextBlocks
* OCR results
* Translations
* Glossary Entries
* Characters
* Review decisions

Deletion may remove:

* Resume state
* Session navigation history
* Unpromoted overrides
* Temporary memory
* Device-specific hints

Audit or legal policies may require tombstones.

---

# Session Tombstone

A deleted Session may leave:

```text
Session Tombstone
├── Session ID
├── Owner ID
├── Deleted At
├── Deletion Reason
├── Final Status
└── Retention Policy
```

The tombstone should not contain unnecessary content details.

---

# Authorization

Every Session operation must revalidate access to referenced resources.

A Session reference does not grant access.

Example:

```text
Session references Project P
```

does not mean the user permanently retains access to Project P.

Authorization is evaluated when resuming, navigating or performing operations.

---

# Access Revocation

When Project access is revoked:

* Active processing should stop where required
* Session should become inaccessible or restricted
* Resume should fail with a clear reason
* Credentials should not remain cached
* Referenced domain artifacts follow Project policy
* Session metadata retention follows security policy

---

# Shared Sessions

Future collaborative support may allow:

* Shared reading
* Review handoff
* Pair translation
* Presentation mode
* Observer access

Shared Sessions require:

* Participant roles
* Presence state
* Edit ownership
* Conflict resolution
* Access revocation
* Spoiler visibility rules

Shared collaboration may be deferred beyond MVP.

---

# Participant

Potential structure:

```text
Session Participant
├── User ID
├── Role
├── Joined At
├── Last Active At
├── Access Scope
└── Presence State
```

Participant presence is partly ephemeral.

Durable participant membership should remain separate from live connection state.

---

# Session Notes

A Session may contain temporary notes.

Examples:

* Check this name later
* Speaker uncertain
* OCR seems wrong
* Use formal style for this scene
* Chapter source is incomplete

Notes may be:

* Session-only
* Attached to a domain object
* Promoted to Project notes
* Converted to review issues

Session-only notes should not silently become canonical content annotations.

---

# Bookmark Integration

Bookmarks may originate inside a Session.

A durable Bookmark should likely belong to a Library, Progress or Annotation domain.

Session stores:

* Current bookmark draft
* Recent bookmark references
* Navigation origin

Closing the Session should not delete promoted Bookmarks.

---

# Annotation Integration

Annotations may include:

* User comments
* Corrections
* Highlights
* Questions
* Translation notes

Session coordinates their creation.

The Annotation domain, when introduced, should own durable annotations.

---

# Offline Session

Offline mode may support:

* Local content
* Local OCR
* Local Translation
* Cached profiles
* Cached glossaries
* Deferred synchronization
* Local checkpoints

Offline Session state should record:

```text
Sync Status
├── Local Revision
├── Server Revision
├── Pending Operations
├── Last Sync At
└── Conflict State
```

---

# Offline Conflict

Possible conflicts:

* Same Session resumed online elsewhere
* Override changed on two devices
* Reading position diverged
* Profile selection changed
* Project content revision changed
* Review item resolved remotely

Conflict resolution should preserve both meaningful decisions when possible.

---

# Synchronization

Session synchronization should distinguish:

* Append-only domain decisions
* Mutable navigation state
* Temporary preferences
* Device-specific hints

Suggested strategies:

| State                | Synchronization Strategy      |
| -------------------- | ----------------------------- |
| Review decision      | Append and validate           |
| Override             | Revisioned merge              |
| Reading position     | Latest meaningful checkpoint  |
| Navigation history   | Bounded union or device-local |
| UI focus             | Do not synchronize            |
| Profile selection    | Last explicit selection       |
| Device source handle | Device-local                  |

---

# Session and Cache

Session identity should rarely be part of reusable content cache keys.

Two Sessions may request the same Translation configuration.

Cache correctness should depend on:

* Source Revision
* Language Pair
* Translation Profile Revision
* Glossary Snapshot
* Character Context Snapshot
* Relevant context hashes
* Pipeline revisions

Session ID may be used for:

* Correlation
* Access filtering
* Temporary projections
* In-flight deduplication

It must not unnecessarily prevent cross-Session reuse of valid domain artifacts.

---

# Session-Specific Results

A result should remain Session-specific only when it depends on Session-only mutable information that was not promoted into a reproducible snapshot.

Recommended rule:

> Any Session context that affects durable output must be captured into an immutable operation or context snapshot.

This allows the result to remain reproducible after the Session changes or expires.

---

# Persistence

Recommended persistence separation:

```text
Session
Session Revision or State Snapshot
Session Checkpoint
Session Override
Session Navigation Entry
Session Review Queue
Session Memory Entry
Session Fork Lineage
Session Tombstone
```

Separate infrastructure persistence:

```text
Session Lease
Session Connection
Session Device Presence
Session Operation Projection
Session Telemetry
Local Ephemeral State
```

---

# Suggested Session Record

```text
Session
├── Session ID
├── Owner ID
├── Project ID
├── Session Type
├── Lifecycle State
├── Current Context
├── Reading Position
├── Language Configuration
├── Translation Profile Revision
├── OCR Profile Revision
├── Presentation Profile Revision
├── Mode
├── Active Override References
├── Review Queue Reference
├── Last Checkpoint
├── Parent Session ID
├── Aggregate Version
├── Created At
├── Last Active At
├── Paused At
├── Completed At
└── Closed At
```

---

# Session Checkpoint Record

```text
Session Checkpoint
├── Checkpoint ID
├── Session ID
├── Session Version
├── Current Context
├── Reading Position
├── Profile References
├── Override References
├── Review Position
├── Presentation State
├── Created At
├── Cause
└── Content Hash
```

---

# Session Override Record

```text
Session Override
├── Override ID
├── Session ID
├── Override Type
├── Target Type
├── Target ID
├── Target Revision
├── Value
├── Scope
├── Priority
├── Status
├── Created By
├── Created At
├── Expires At
└── Promoted Reference
```

---

# Validation

Session validation should verify:

* Owner exists
* Project access remains valid
* Referenced content belongs to the expected scope
* Referenced revisions exist
* Language Pair is valid
* Selected Profile Revisions are compatible
* Reading Position can be resolved
* Overrides have valid targets
* Override scopes do not exceed Session scope
* Lifecycle transitions are valid
* Closed Sessions reject active mutations
* Fork lineage is not circular
* Checkpoint version is consistent
* Spoiler boundary is compatible with current position

---

# Invalid Reference Handling

A referenced domain object may become unavailable or superseded.

Possible resolution states:

* Valid
* Superseded but Resolvable
* Missing
* Access Denied
* Archived
* Incompatible
* Requires Migration

Session recovery should preserve the original reference and record the resolution outcome.

---

# Lifecycle Transition Validation

Examples:

```text
Created → Active
```

valid.

```text
Active → Paused
```

valid.

```text
Paused → Active
```

valid.

```text
Closed → Active
```

normally invalid.

A Closed Session may instead be:

* Reopened by creating a new Session
* Forked
* Explicitly restored under a defined policy

---

# Error Conditions

Typical Session errors:

* Session Not Found
* Session Access Denied
* Session Already Closed
* Invalid Lifecycle Transition
* Session Version Conflict
* Primary Client Lease Conflict
* Project Reference Invalid
* Content Position Unresolvable
* Profile Revision Missing
* Language Pair Incompatible
* Override Target Invalid
* Override Scope Invalid
* Checkpoint Stale
* External Source Unavailable
* Device Context Incompatible
* Resume Conflict
* Spoiler Boundary Violation
* Offline Synchronization Conflict

Errors should be structured and recoverable where possible.

---

# Events

Typical domain events include:

* `SessionCreated`
* `SessionActivated`
* `SessionPaused`
* `SessionResumed`
* `SessionCompleted`
* `SessionClosed`
* `SessionAbandoned`
* `SessionExpired`
* `SessionArchived`
* `SessionContextChanged`
* `SessionPositionChanged`
* `SessionCheckpointCreated`
* `SessionModeChanged`
* `SessionLanguageConfigurationChanged`
* `SessionProfileChanged`
* `SessionOverrideAdded`
* `SessionOverrideChanged`
* `SessionOverrideRemoved`
* `SessionOverridePromoted`
* `SessionReviewQueueChanged`
* `SessionForked`
* `SessionRecovered`
* `SessionResumeConflictDetected`
* `SessionDeviceHandoffCompleted`

High-frequency UI interaction should not emit domain events.

---

# Event Payload Example

```text
SessionCheckpointCreated
├── Session ID
├── Checkpoint ID
├── Session Version
├── Project ID
├── Content Location Reference
├── Position Type
├── Cause
├── Actor
├── Occurred At
├── Correlation ID
└── Causation ID
```

Full source text, credentials and sensitive provider data should not be included.

---

# Comic Reading Example

```text
Session Type:
Live Translation

Project:
Comic A

Current Context:
Chapter 18, Page 12

Capture Mode:
Browser Screenshot

Source Language:
zh-Hans

Target Language:
vi

Profiles:
- OCR Comic Profile Revision 4
- Natural Vietnamese Translation Profile Revision 7
- Comic Overlay Presentation Profile Revision 3

Mode:
Automatic

Current Position:
Page 12, lower panel

Session Preferences:
- Show translated overlay
- Hide original OCR text
- Pause when OCR confidence < 0.65
- Highlight glossary violations
```

When the user closes the application, CRAI stores a checkpoint.

On resume, the system restores the logical Page and attempts to recover the browser source.

---

# Novel Reading Example

```text
Session Type:
Reading

Project:
Novel B

Current Context:
Book 1, Chapter 42

Position:
Paragraph 18, sentence offset 4

Source Language:
zh-Hans

Target Language:
vi

Display:
Interleaved original and translation

Session Memory:
Previous 12 paragraphs

Character Context:
Current scene cast

Temporary Override:
Use “sư tôn” for 师尊 in this chapter
```

The temporary term override contributes to a new immutable Glossary Snapshot for each affected Translation operation.

It does not immediately update the Project Glossary.

---

# Session Resume Example

Initial state:

```text
Chapter 12
Page 8
Overlay enabled
Translation Profile Revision 5
```

The application crashes after the user scrolls halfway into Page 9.

Last committed checkpoint:

```text
Chapter 12
Page 9
Top panel
```

On recovery, CRAI resumes from the checkpoint rather than relying on the lost pixel scroll offset.

---

# Profile Change Example

The user changes:

```text
Translation Profile Revision 5
→
Translation Profile Revision 8
```

Consequences:

* Existing Translation Revisions remain unchanged.
* New Translation requests use Revision 8.
* Existing results may still be displayed.
* The Session may offer retranslation.
* Cache lookup uses the new configuration identity.
* Session records the explicit profile transition.

---

# Temporary Glossary Override Example

```text
Source term:
灵力

Project Glossary:
linh lực

Session Override:
linh khí

Scope:
Current Chapter
```

For operations in the current Chapter:

```text
Project Glossary Revision
+
Session Override
→
Glossary Snapshot GS-91
```

Translation Revision references `GS-91`.

Closing the Session removes the unpromoted override but does not invalidate already-created Translation history.

---

# Speaker Override Example

```text
Detected Speaker:
Character A, confidence 0.58

User Session Override:
Speaker is Character B

Scope:
Current TextBlock
```

The operation uses Character B in its Character Context Snapshot.

The user may later promote the override into a durable Speaker Attribution revision.

---

# Multi-Device Example

Desktop Session:

```text
Logical Position:
Chapter 7, Page 21

Device State:
Browser source and overlay
```

Mobile resume:

```text
Logical Position:
Chapter 7, Page 21

Adapted Presentation:
Translation-only vertical reading

Unavailable State:
Desktop browser window handle
```

The logical Session continues while device-specific capture state is reconfigured.

---

# Offline Example

A user reads imported pages offline.

During the Session:

* OCR runs locally
* Translation runs locally
* Several glossary overrides are added
* Reading position advances
* One character name correction is promoted locally

When connectivity returns:

* Session checkpoint synchronizes
* Domain updates synchronize separately
* Conflicts are resolved by owning aggregates
* Device-only screen state is discarded

---

# Architecture Invariants

1. Session is a domain working-context Aggregate Root.
2. Session is not an authentication session.
3. Session is not a provider conversation.
4. Session is not a runtime job.
5. Session is not a WebSocket or process lifetime.
6. Session references durable domain objects but does not own them.
7. Closing a Session does not delete created domain artifacts.
8. Runtime worker failure does not invalidate Session identity.
9. Session identity remains stable across pause and resume.
10. Logical content position remains separate from visual viewport state.
11. Durable Session state remains separate from ephemeral client state.
12. Authentication credentials are never stored in Session.
13. Provider secrets are never stored in Session.
14. Provider-specific conversation IDs do not become Session identity.
15. Source and target languages use canonical Language values.
16. Selected Profiles reference exact revisions.
17. Mutable Session state must not be used as unreproducible Translation context.
18. Context affecting durable output is captured in immutable snapshots.
19. Translation Revisions reference exact context snapshots.
20. Later Session changes do not rewrite completed operation context.
21. Session overrides are temporary until explicitly promoted.
22. Session overrides cannot silently mutate external aggregates.
23. Project Glossary truth remains separate from Session terminology overrides.
24. Character truth remains separate from Session speaker or identity overrides.
25. Review navigation state remains separate from review decisions.
26. Presentation state does not modify Translation truth.
27. High-frequency UI state is not canonical domain history.
28. Session events represent meaningful transitions, not raw telemetry.
29. Session references do not grant authorization.
30. Access is revalidated when the Session is resumed or used.
31. Device-specific handles are not durable cross-device identifiers.
32. Multi-device continuation preserves logical context and adapts local state.
33. Concurrent Session writes use explicit versioning or lease policy.
34. Primary client lease is coordination infrastructure, not ownership.
35. Session checkpoints are idempotent.
36. Session forks receive new identities and preserve lineage.
37. Sessions should not normally be merged.
38. Session deletion does not cascade into Project content.
39. Cache correctness does not depend on Session ID alone.
40. Derived operation projections may be rebuilt from operation events.
41. Expiration affects Session state, not referenced domain truth.
42. Spoiler boundaries are respected during context construction.
43. Session recovery preserves unresolved references rather than silently substituting them.
44. Every significant lifecycle transition is auditable.

---

# Open Decisions

The following decisions should remain open until implementation and prototype testing:

* Whether Session is always persisted or may be local-only
* Whether anonymous Sessions synchronize across devices
* Whether one user may have several active Sessions per Project
* Whether Session belongs strictly to one Project
* Whether switching Project forks or mutates a Session
* Whether Session revisions are fully immutable
* Whether Session should use event sourcing
* Which state changes produce durable revisions
* How frequently reading position is checkpointed
* Whether navigation history is server-side or device-local
* How long completed Sessions are retained
* Whether closed Sessions may be reopened
* Whether reopening creates a new Session
* Whether Session Fork is required in MVP
* Whether collaborative Sessions are supported
* Whether one or several clients may actively edit a Session
* How primary client leases work offline
* How concurrent position updates are resolved
* Which device settings synchronize
* How browser tab identity is recovered
* How external URLs are canonicalized
* How site adapters represent chapter positions
* Whether screen capture regions synchronize
* Whether local file paths are persisted
* How unavailable external sources are reconciled
* Whether Session stores a bounded memory window
* Whether Session Memory is persisted
* How scene boundaries are detected
* How context-budget eviction works
* Whether provider conversation state is reused within a Session
* How provider switching affects context continuity
* Which temporary override types are supported
* Whether overrides may outlive a Session
* Whether overrides can be promoted automatically
* How temporary glossary overrides are compiled into snapshots
* How Session character overrides interact with Speaker Attribution
* Whether review queues store item lists or queries
* How review queues react to newly created findings
* Whether Session progress updates a separate Reading Progress domain
* Whether Bookmarks and Annotations require separate domains
* How offline Session conflicts are presented
* Whether Session deletion leaves a tombstone
* Whether Session analytics are opt-in
* How spoiler boundaries follow reader progress
* Whether Session-level privacy mode disables cloud synchronization
* Whether active operations are cancelled automatically on pause
* Which operations continue after Session close
* How long operation projections remain attached to Session
* Whether Session Type changes during its lifecycle
* Whether Mixed Session Type is necessary
* How Profile compatibility is validated
* Whether Presentation Profile belongs in Session or client preferences
* Whether zoom and font scale are centrally persisted
* Whether target-language changes create a fork or mutate the Session
* How multiple target languages are represented
* Whether Session snapshots are content-addressed
* How much Session context appears in audit events

---

# Recommended MVP Scope

The first CRAI MVP should support:

* Stable Session identity
* User-owned and local anonymous Sessions
* One Project per Session
* Reading and Live Translation Session Types
* Created, Active, Paused, Completed and Closed states
* Current Book, Chapter and Page references
* TextBlock position where available
* Logical reading position
* Periodic checkpoints
* Pause and resume
* Crash recovery
* Source and target Language selection
* Exact Translation Profile Revision
* Exact OCR Profile Revision
* Exact Presentation Profile Revision
* Manual, Assisted and Automatic modes
* Imported File, Browser and Screen Capture modes
* Session Preferences
* Session-level glossary overrides
* Session-level speaker overrides
* Explicit override promotion
* Immutable operation-context snapshots
* Review queue position
* Basic presentation state
* Bounded navigation history
* One primary active client
* Optimistic concurrency
* Basic device handoff
* Session fork
* Session lifecycle events
* Session audit
* Selective retention
* Session deletion without content cascade

The MVP may defer:

* Real-time collaborative Sessions
* Multiple simultaneous editors
* Full Session event sourcing
* Cross-Project Sessions
* Multi-target-language Sessions
* Provider conversation persistence
* Advanced Session Memory
* Semantic scene summaries
* Automatic override promotion
* Complex offline conflict merging
* Shared review queues
* Live presence
* Session chat
* Synchronized mouse or viewport state
* Full browser-tab restoration
* Cross-device screen capture restoration
* Automatic Session merging
* Long-term analytics
* Complex lease negotiation
* Workspace-owned Sessions
* Public shared Sessions
* Full annotation ownership
* Reading social features
* Session templates
* Advanced Session branching
* Server-side storage of device-specific layout details

---

# Related Documents

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
* `docs/architecture/CAPABILITY_MAP.md`
* `docs/architecture/DATA_FLOW.md`
* `docs/architecture/STATE_MACHINE.md`
* `docs/architecture/EVENT_BUS.md`
* `docs/architecture/MODULE_DEPENDENCY.md`
* `docs/architecture/ai/PIPELINE.md`
* `docs/architecture/ai/CONTEXT.md`
* `docs/architecture/ai/MEMORY.md`
* `docs/architecture/ai/PROMPTS.md`
* `docs/architecture/ai/REQUEST.md`
* `docs/architecture/ai/RESPONSE.md`
* `docs/architecture/ai/ROUTING.md`
* `docs/architecture/ai/CACHE.md`
* `docs/architecture/runtime/JOB.md`
* `docs/architecture/runtime/QUEUE.md`
* `docs/architecture/presentation/LAYOUT.md`
* `docs/architecture/presentation/TYPOGRAPHY.md`
* `docs/architecture/presentation/FONTS.md`
