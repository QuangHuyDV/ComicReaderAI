# Reading Session Module States

- Module: Reading Session
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

This document defines the state machine of the Reading Session Module.

The Reading Session Module owns the lifecycle of a reading session and coordinates the execution of the entire processing pipeline.

---

# State Principles

## Single Active State

A Reading Session can exist in only one state at any given time.

---

## Deterministic Transitions

Given the same current state and event, the resulting state must always be identical.

---

## Revision Awareness

State transitions that restart processing must create a new Session Revision and Processing Revision.

---

## Immutable History

State transitions do not modify previous revisions.

Historical revisions remain immutable.

---

# State Model

```text
              CreateSession
                   │
                   ▼
              Initialized
                   │
                   ▼
                Running
              ┌────┼────┐
              │    │    │
              ▼    ▼    ▼
          Paused Restarting
              │      │
              └──┐   │
                 ▼   ▼
               Running
                  │
        ┌─────────┴─────────┐
        ▼                   ▼
    Completed          Cancelled
        ▲
        │
      Failed
```

---

# State Summary

| State | Description |
|--------|-------------|
| Initialized | Session created but pipeline not started |
| Running | Processing pipeline is active |
| Paused | Scheduling is temporarily suspended |
| Restarting | Pipeline is rebuilding a new revision |
| Completed | Pipeline finished successfully |
| Cancelled | Session terminated |
| Failed | Session encountered an unrecoverable error |

---

# Initialized

## Meaning

A new Reading Session has been created.

Resources are allocated.

No processing has started.

---

## Allowed Inputs

- StartPipeline
- CancelSession

---

## Exit Conditions

- Pipeline started
- Session cancelled

---

## Invariants

- SessionId exists.
- Revision = 1.
- No processing tasks are running.

---

# Running

## Meaning

The processing pipeline is active.

Reading Session schedules downstream modules.

---

## Allowed Inputs

- CaptureCompleted
- RecognitionCompleted
- TextProcessingCompleted
- TranslationCompleted
- PresentationCompleted
- SourceChanged
- ChapterChanged
- PageChanged
- ViewportChanged
- ConfigurationChanged
- PauseSession
- CancelSession
- ModuleFailed

---

## Exit Conditions

- Pipeline completed
- Restart required
- Session paused
- Session cancelled
- Fatal failure

---

## Invariants

- Exactly one active Processing Revision.
- Downstream scheduling is enabled.
- Obsolete revisions are rejected.

---

# Paused

## Meaning

Scheduling is temporarily suspended.

Running operations may finish naturally.

No new stages are scheduled.

---

## Allowed Inputs

- ResumeSession
- CancelSession

---

## Exit Conditions

- Resume
- Cancel

---

## Invariants

- Session remains valid.
- Revision is unchanged.
- Scheduling is disabled.

---

# Restarting

## Meaning

The current processing pipeline is being replaced by a new revision.

Old revisions become obsolete.

---

## Allowed Inputs

- RevisionCreated
- PipelineStarted
- CancelSession

---

## Exit Conditions

- New pipeline started
- Session cancelled

---

## Invariants

- Exactly one new revision is being created.
- Previous revisions are immutable.
- Old processing may be cancelled.

---

# Completed

## Meaning

Processing finished successfully.

Presentation has been produced.

---

## Allowed Inputs

- RestartPipeline
- CancelSession

---

## Exit Conditions

- Restart
- Cancel

---

## Invariants

- Latest revision completed successfully.
- No active scheduling exists.

---

# Cancelled

## Meaning

The Reading Session has been terminated.

No further processing is allowed.

---

## Allowed Inputs

None.

---

## Exit Conditions

None.

---

## Invariants

- All downstream work is cancelled.
- Session cannot resume.
- Resources may be released.

---

# Failed

## Meaning

The session encountered an unrecoverable failure.

Manual intervention or retry is required.

---

## Allowed Inputs

- RestartPipeline
- CancelSession

---

## Exit Conditions

- Restart
- Cancel

---

## Invariants

- Failed revision remains immutable.
- No automatic scheduling occurs.

---

# State Transition Table

| Current State | Event | Next State |
|---------------|-------|------------|
| Initialized | StartPipeline | Running |
| Initialized | CancelSession | Cancelled |
| Running | PauseSession | Paused |
| Running | SourceChanged | Restarting |
| Running | ChapterChanged | Restarting |
| Running | PageChanged | Restarting |
| Running | ConfigurationChanged | Restarting |
| Running | PresentationCompleted | Completed |
| Running | ModuleFailed | Failed |
| Running | CancelSession | Cancelled |
| Paused | ResumeSession | Running |
| Paused | CancelSession | Cancelled |
| Restarting | PipelineStarted | Running |
| Restarting | CancelSession | Cancelled |
| Completed | RestartPipeline | Restarting |
| Completed | CancelSession | Cancelled |
| Failed | RestartPipeline | Restarting |
| Failed | CancelSession | Cancelled |

---

# Transition Rules

## Source Changed

Always creates a new Session Revision.

A new Processing Revision is generated.

---

## Chapter Changed

Always restarts the pipeline.

---

## Page Changed

The session decides whether incremental processing or full restart is required.

---

## Configuration Changed

Configuration changes invalidate incompatible processing results.

---

## Module Failure

The session determines whether:

- Retry
- Restart
- Failure

should occur.

---

# Scheduling Rules

While Running:

- New stages may be scheduled.
- Previous revisions are ignored.

While Paused:

- No new stages are scheduled.

While Restarting:

- New revision is prepared.
- Previous revision becomes obsolete.

---

# Revision Rules

Rules

1. Every restart creates a new Processing Revision.
2. Revision numbers are monotonically increasing.
3. Previous revisions remain immutable.
4. Only one active revision exists.

---

# Timeout Rules

The session may transition to Failed when:

- Pipeline timeout
- Resource exhaustion
- Internal orchestration failure

Transient downstream failures should trigger retries before entering Failed.

---

# Recovery Rules

The session may recover from:

- Failed
- Completed

by creating a new Processing Revision.

Recovery never modifies previous revisions.

---

# Architecture Invariants

The Reading Session Module guarantees:

1. Every session has exactly one active state.
2. Every processing task belongs to one active session.
3. Every active task references the latest Processing Revision.
4. Previous revisions are immutable.
5. Completed sessions never schedule new work.
6. Cancelled sessions never resume.
7. Only Reading Session controls pipeline execution.

---

# Related Documents

- MODULE.md
- CONTRACT.md
- EVENTS.md
- ERRORS.md
- README.md