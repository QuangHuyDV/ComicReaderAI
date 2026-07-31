# Reading Session Module

The Reading Session Module is the orchestration center of CRAI.

It manages the lifecycle of a reading activity and coordinates the execution of every processing module.

Unlike other modules, Reading Session does not perform any business processing. Its responsibility is to decide **what should run, when it should run, and whether the produced results are still valid**.

---

# Responsibilities

The Reading Session Module is responsible for:

- Creating reading sessions.
- Maintaining session lifecycle.
- Managing processing revisions.
- Coordinating processing modules.
- Cancelling obsolete work.
- Restarting processing when necessary.
- Tracking pipeline progress.
- Publishing session lifecycle events.

It is **not** responsible for:

- Screen capture
- OCR
- Text normalization
- Translation
- Presentation rendering
- Persistent storage

---

# Position in Architecture

```text
                User
                  │
                  ▼
          Reading Session
                  │
      ┌───────────┼───────────┐
      ▼           ▼           ▼
   Capture   Session Events   Configuration
      │
      ▼
 Recognition
      │
      ▼
Text Processing
      │
      ▼
 Translation
      │
      ▼
 Presentation
      │
      ▼
 UI Adapter
```

Reading Session is the only module responsible for coordinating the processing pipeline.

Processing modules never invoke each other directly.

---

# Session Lifecycle

A typical session follows this lifecycle:

```text
Create Session
       │
       ▼
Initialize
       │
       ▼
Running
       │
       ▼
Processing Pipeline
       │
       ▼
Completed
```

The session may also:

- Pause
- Resume
- Restart
- Cancel
- Fail

depending on user actions or processing results.

---

# Processing Pipeline

The Reading Session Module coordinates the following pipeline:

```text
Capture
    │
    ▼
Recognition
    │
    ▼
Text Processing
    │
    ▼
Translation
    │
    ▼
Presentation
```

Each stage executes independently.

Reading Session determines when each stage begins.

---

# Session Context

Each Reading Session maintains:

- Session Identifier
- Source Identifier
- Source Type
- Session Revision
- Processing Revision
- Current State
- Active Pipeline
- Session Configuration

The session context is shared across the processing pipeline.

---

# Revision Management

Reading Session owns all revisions.

Whenever the reading context changes, a new revision is created.

Examples include:

- browser navigation
- chapter change
- page change
- viewport change
- configuration update

Older revisions become obsolete and must never overwrite newer results.

---

# Scheduling

Reading Session decides when processing should:

- Start
- Continue
- Restart
- Retry
- Stop

Processing modules never schedule themselves.

---

# Cancellation

Reading Session is responsible for cancelling obsolete work.

Typical scenarios include:

- User changes chapter.
- User navigates to another page.
- Reading source changes.
- Session is cancelled.
- A newer revision becomes active.

Cancellation prevents unnecessary computation and stale results.

---

# Event Model

Reading Session consumes events from:

- User Interface
- Browser Adapter
- Capture
- Recognition
- Text Processing
- Translation
- Presentation

It publishes events describing:

- Session lifecycle
- Pipeline lifecycle
- Revision lifecycle

Processing modules publish only processing events.

---

# State Management

Reading Session owns the overall execution state.

Typical states include:

- Initialized
- Running
- Paused
- Restarting
- Completed
- Cancelled
- Failed

Processing modules manage only their own internal states.

---

# Error Handling

Reading Session is responsible for deciding how the system reacts to failures.

Possible actions include:

- Retry
- Restart pipeline
- Cancel session
- Ignore obsolete results
- Enter Failed state

Processing modules report failures but do not decide recovery strategies.

---

# Design Principles

## Single Orchestrator

Only Reading Session coordinates the processing pipeline.

---

## Revision-Based Execution

Every processing task belongs to exactly one processing revision.

---

## Immutable History

Previous revisions are never modified.

---

## Loose Coupling

Modules communicate through events rather than direct dependencies.

---

## Deterministic Scheduling

Given the same session state and events, scheduling decisions are deterministic.

---

# Performance Goals

The module should:

- Avoid duplicate processing.
- Cancel obsolete work immediately.
- Maximize cache reuse.
- Minimize unnecessary pipeline restarts.
- Support future concurrent sessions.

---

# Related Documents

| Document | Description |
|----------|-------------|
| MODULE.md | Module responsibilities and boundaries |
| CONTRACT.md | Public contracts |
| EVENTS.md | Published and consumed events |
| STATES.md | Session state machine |
| ERRORS.md | Error definitions |

---

# Summary

The Reading Session Module is the orchestration layer of CRAI.

It owns the lifecycle of every reading activity, coordinates the processing pipeline, manages revisions, and ensures that only the latest valid results are propagated through the system. By centralizing orchestration, the architecture remains deterministic, loosely coupled, and easier to extend as new processing modules are introduced.