# Reading Session Module

- Module: Reading Session
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

The Reading Session Module is the orchestration layer of CRAI.

It owns the lifecycle of a reading activity and coordinates the execution of all processing modules.

A Reading Session represents a continuous interaction between the user and a source of readable content, such as a web page, comic page, novel chapter, PDF, or image collection.

This module does not perform business processing itself. Instead, it determines when processing should start, stop, continue, restart, or be discarded.

---

# Responsibilities

The Reading Session Module is responsible for:

- Creating reading sessions.
- Maintaining the current session context.
- Tracking active reading targets.
- Coordinating processing pipelines.
- Cancelling obsolete work.
- Restarting processing when required.
- Maintaining processing revisions.
- Managing session lifecycle.
- Publishing session-related events.
- Preventing stale results from reaching downstream modules.

---

# Out of Scope

The Reading Session Module is **not** responsible for:

- Capturing images.
- OCR recognition.
- Text normalization.
- Translation.
- Presentation rendering.
- Persistent storage.
- User interface rendering.
- User preferences.

Those responsibilities belong to their respective modules.

---

# Owned Domain

The module owns the following concepts:

- ReadingSession
- SessionContext
- ActiveSource
- ProcessingRevision
- SessionState
- SessionConfiguration

No other module may modify these concepts directly.

---

# Upstream Dependencies

The Reading Session Module may receive requests from:

- User Interface
- Browser Adapter
- Automation
- External API

These requests initiate or modify a session.

---

# Downstream Dependencies

The Reading Session Module coordinates:

- Capture
- Recognition
- Text Processing
- Translation
- Presentation

These modules never coordinate each other directly.

---

# Responsibilities by Lifecycle

## Session Creation

Responsible for:

- Creating a unique session.
- Initializing configuration.
- Assigning initial revision.
- Publishing session creation events.

---

## Session Update

Responsible for detecting changes such as:

- page navigation
- chapter changes
- viewport movement
- source replacement
- configuration changes

and determining whether processing should continue or restart.

---

## Session Termination

Responsible for:

- cancelling running operations
- releasing resources
- publishing completion events

---

# Session Context

Each session contains at least:

- Session Identifier
- Source Identifier
- Current Revision
- Active Pipeline Revision
- Processing State
- User Configuration
- Active Capture Target

---

# Revision Ownership

Reading Session owns every processing revision.

Whenever the reading context changes, a new revision is created.

Older revisions become obsolete.

Downstream modules must reject obsolete revisions.

---

# Processing Coordination

Reading Session decides:

- when Capture starts
- when OCR starts
- when Translation starts
- whether cached results may be reused
- whether Presentation should be rebuilt

Processing modules only execute work.

They never decide scheduling.

---

# Cancellation

The module is responsible for cancelling obsolete work.

Examples:

- page changed
- chapter changed
- user selected another region
- browser navigated
- session closed

Cancellation prevents unnecessary computation.

---

# Event Ownership

Reading Session owns events describing the lifecycle of a reading activity.

Typical examples include:

- session started
- session updated
- session paused
- session resumed
- session cancelled
- session completed

Processing modules publish only processing events.

---

# State Ownership

The Reading Session Module owns:

- session lifecycle
- pipeline lifecycle
- processing revision lifecycle

Processing modules own only their internal execution state.

---

# Design Principles

## Single Active Session Context

A processing operation belongs to exactly one session.

---

## Revision-Based Coordination

Every downstream operation references a session revision.

Results produced by obsolete revisions are discarded.

---

## Deterministic Scheduling

Given the same inputs and configuration, the module produces the same scheduling decisions.

---

## Loose Coupling

Processing modules communicate through events.

They never invoke each other directly.

---

## Fail Isolation

A processing failure does not terminate the session automatically.

The session decides whether to retry, continue, or stop.

---

# Module Boundaries

The Reading Session Module owns orchestration only.

Business processing remains inside processing modules.

This separation keeps orchestration independent from implementation details.

---

# Dependencies

Consumes services provided by:

- Capture
- Recognition
- Text Processing
- Translation
- Presentation

Publishes orchestration decisions to those modules through defined contracts.

---

# Performance Goals

The module should:

- minimize redundant processing
- maximize cache reuse
- reduce unnecessary pipeline restarts
- discard obsolete work immediately
- support multiple concurrent sessions in the future

---

# Architecture Invariants

The Reading Session Module must guarantee:

1. Every processing task belongs to exactly one session.
2. Every task references exactly one revision.
3. Obsolete revisions never reach Presentation.
4. Only one active revision exists per session.
5. Session cancellation cancels all unfinished downstream work.
6. Processing modules never coordinate each other directly.
7. Reading Session is the only orchestration authority.

---

# Related Documents

- CONTRACT.md
- EVENTS.md
- STATES.md
- ERRORS.md
- README.md
```