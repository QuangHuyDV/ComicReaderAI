# Reading Session Module Contract

- Module: Reading Session
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

This document defines the public contract of the Reading Session Module.

The Reading Session Module owns the lifecycle of a reading session and coordinates the execution of the processing pipeline.

Other modules communicate with Reading Session only through the contracts defined in this document.

---

# Public Commands

## CreateSession

Purpose

Create a new reading session.

Required Data

- Source Identifier
- Source Type
- Session Configuration

Result

- New Session Identifier
- Session Revision = 1

---

## UpdateSession

Purpose

Update an existing session.

Examples

- source changed
- chapter changed
- page changed
- viewport changed
- configuration changed

Result

- Session Revision incremented
- Obsolete processing cancelled if necessary

---

## PauseSession

Purpose

Temporarily pause processing.

Result

- Running tasks may complete.
- No new tasks are scheduled.

---

## ResumeSession

Purpose

Resume a paused session.

Result

- Scheduling continues from the latest revision.

---

## CancelSession

Purpose

Terminate an active session.

Result

- Cancel downstream work.
- Release session resources.

---

## RestartPipeline

Purpose

Restart the processing pipeline from Capture.

Typical Usage

- manual refresh
- OCR retry
- configuration change

Result

- New processing revision.

---

## RetryStage

Purpose

Retry a specific processing stage.

Supported Stages

- Capture
- Recognition
- Text Processing
- Translation
- Presentation

Result

- New processing revision for the affected stage.

---

# Public Queries

## GetSession

Returns

- Session Identifier
- State
- Current Revision
- Active Source
- Active Stage

---

## GetPipelineStatus

Returns

Processing status for:

- Capture
- Recognition
- Text Processing
- Translation
- Presentation

---

## GetCurrentRevision

Returns

Current processing revision.

---

## IsSessionActive

Returns

Whether the session is active.

---

## ListActiveSessions

Returns

All active sessions.

Future implementations may support multiple concurrent sessions.

---

# Consumed Events

The Reading Session Module consumes events from upstream systems.

| Event | Purpose |
|--------|---------|
| UserRequestedSession | Create session |
| SourceChanged | Update source |
| ChapterChanged | Start new revision |
| ViewportChanged | Decide whether pipeline restart is required |
| ConfigurationChanged | Reconfigure session |
| CaptureCompleted | Continue pipeline |
| RecognitionCompleted | Continue pipeline |
| TextProcessingCompleted | Continue pipeline |
| TranslationCompleted | Continue pipeline |
| PresentationCompleted | Mark pipeline completed |
| ModuleFailed | Decide retry or cancellation |

---

# Published Events

| Event | Purpose |
|--------|---------|
| SessionCreated | Session initialized |
| SessionUpdated | Revision updated |
| SessionPaused | Session paused |
| SessionResumed | Session resumed |
| SessionCancelled | Session terminated |
| SessionCompleted | Pipeline finished |
| PipelineStarted | Processing pipeline started |
| PipelineRestarted | Pipeline restarted |
| PipelineCancelled | Pipeline cancelled |
| RevisionCreated | New revision generated |
| RevisionDiscarded | Old revision discarded |

---

# Data Contracts

## ReadingSession

Contains

- SessionId
- State
- Revision
- SourceId
- SourceType
- Configuration
- ActivePipeline
- CreatedAt
- UpdatedAt

---

## SessionConfiguration

Contains

- Source Language
- Target Language
- Translation Provider
- Presentation Mode
- OCR Strategy

---

## ProcessingRevision

Contains

- RevisionId
- SessionId
- ParentRevision
- CreatedAt
- Status

---

# Revision Contract

Every pipeline execution belongs to exactly one revision.

Rules

- Revision numbers are monotonically increasing.
- Older revisions become obsolete.
- Downstream modules must reject obsolete revisions.
- Published results always reference a revision.

---

# Session Lifecycle Contract

The valid lifecycle is:

```text
Created
    ↓
Running
    ↓
Paused
    ↓
Running
    ↓
Completed
```

or

```text
Created
    ↓
Running
    ↓
Cancelled
```

No other transitions are valid.

---

# Processing Contract

Reading Session decides:

- when Capture starts
- when Recognition starts
- when Translation starts
- when Presentation rebuilds

Processing modules only execute work.

They never perform orchestration.

---

# Cancellation Contract

Cancellation guarantees:

- No new downstream tasks are scheduled.
- Obsolete revisions are ignored.
- Pending work may be cancelled.
- Partial results are never promoted.

---

# Retry Contract

Retry may occur:

- automatically
- manually
- after transient failures

Retry always creates a new processing revision.

Previous revisions remain immutable.

---

# Version Contract

Every public contract includes:

- Contract Version
- Session Revision
- Processing Revision

Older contract versions must remain backward compatible whenever possible.

---

# Performance Contract

The Reading Session Module should:

- minimize redundant processing
- maximize cache reuse
- cancel obsolete work immediately
- avoid duplicate scheduling
- support future concurrent sessions

---

# Error Contract

Commands may return:

- Validation Error
- State Error
- Revision Error
- Resource Error
- Internal Error

Detailed definitions are provided in `ERRORS.md`.

---

# Security Contract

The module must never expose:

- browser credentials
- cookies
- authentication tokens
- captured content outside the active session

---

# Privacy Contract

Session metadata may be shared between modules.

Captured content remains owned by processing modules.

---

# Architecture Invariants

The Reading Session Module guarantees:

1. Every processing task belongs to exactly one session.
2. Every processing task references exactly one revision.
3. Only one active revision exists per session.
4. Obsolete revisions are never promoted downstream.
5. Processing modules never orchestrate each other.
6. Reading Session is the single orchestration authority.
7. Session termination prevents further scheduling.

---

# Related Documents

- MODULE.md
- EVENTS.md
- STATES.md
- ERRORS.md
- README.md