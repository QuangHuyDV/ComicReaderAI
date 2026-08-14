# Architecture Documentation

> **Project:** CRAI
> **Directory:** `doc/01-architecture/`
> **Status:** Complete and consistency-reviewed

This directory contains the system-wide architecture of CRAI, organized by concern.

---

## Purpose

`01-architecture/` defines **how CRAI is designed** — its capabilities, domain models, processing flows, runtime execution model, and all major subsystem architectures.

It does not define business module contracts or infrastructure implementation details.
Those belong to `02-modules/` and `03-infrastructure/` respectively.

---

## Architecture Overview

CRAI architecture is organized into four primary layers:

```text
Business Modules      (02-modules/)
    → own business meaning and contracts

Runtime               (01-architecture/runtime/)
    → owns execution orchestration and authority

Infrastructure        (03-infrastructure/)
    → provides technical capabilities

Storage               (02-modules/storage/)
    → owns persistence mechanisms
```

The domain model and all cross-cutting concerns are defined here in `01-architecture/`.

---

## Subdirectory Map

| Subdirectory | Responsibility |
|---|---|
| `core/` | Foundational architecture: capability map, data flow, event bus, state machine, event conventions |
| `domain/` | Domain model definitions: Book, Chapter, Page, Session, Translation, Workspace, Glossary, Character, Language, Profile |
| `flows/` | End-to-end product flows: Screen Comic, Structured Text, Content Change, Reading Session |
| `modules/` | Module map, ownership map, module dependency rules |
| `runtime/` | Runtime execution model: pipeline, queue, scheduler, cancellation, memory, threading, resource lifecycle, performance |
| `ocr/` | OCR subsystem architecture: pipeline, detection, recognition, preprocessing, postprocessing, reading order, layout, quality, providers |
| `text/` | Text model and segmentation architecture |
| `translate/` | Translation architecture: translation model, context model |
| `ai/` | AI subsystem architecture: pipeline, models, prompts, routing, context, cache, cost control, fallback, safety, streaming |
| `plugin/` | Plugin system: API, lifecycle, discovery, configuration, dependency, registry, security, versioning |

---

## Reading Order

For a new contributor or AI session working on architecture:

1. `core/README.md`
2. `core/CAPABILITY_MAP.md`
3. `domain/README.md`
4. `flows/README.md`
5. `modules/MODULE_MAP.md`
6. `runtime/README.md`

Read `ocr/`, `text/`, `translate/`, `ai/`, `plugin/` only when working on those specific areas.

---

## Core Architecture Principles

### Latest Valid Revision Wins

New user-visible content has higher value than obsolete pending work.

### Everything Is Cancelable

Long-running tasks must support cooperative cancellation or safe result rejection.

### Never Block the UI Thread

Capture processing, OCR, translation, and provider requests must execute outside the UI thread.

### Immutable Processing Inputs

Workers consume immutable references and produce new immutable outputs.

### Bounded Work

Queues, worker counts, retries, and retained revisions must have explicit limits.

### Cache Before Computation

Reusable work should be resolved before expensive processing begins.

### Atomic Presentation

Only a complete and currently valid presentation model may update the UI.

---

## Runtime Execution Model

The canonical Runtime v2 execution flow:

```text
Stable Business Content / Intent
    ↓
ExecutionScope
    ↓
ExecutionRevision
    ↓
BusinessExecutionPlan
    ↓
WorkItem
    ↓
Attempt
    ↓
Candidate Runtime Artifact
    ↓
Execution Authority Validation
    ↓
Ownership Transfer
    ↓
Runtime Artifact Publication
    ↓
Business Acceptance
    ↓
Presentation Commit
```

Full details in `runtime/README.md` and `runtime/PIPELINE_RUNTIME.md`.

---

## Runtime and Domain Boundaries

Runtime decides:

- when work runs
- whether work remains valid
- how much work may run concurrently
- when work should be canceled or retried

Domain modules decide:

- how OCR is performed
- how reading order is resolved
- how translation units are built
- how translation is generated
- how presentation models are constructed

Runtime must not contain domain-specific processing logic.

---

## Current Status

Architecture phase: **Complete and consistency-reviewed**

All subdirectories have been reviewed and synchronized with the Runtime v2 model.

The next project phase is: **Technology Selection**

See `.meta/PROJECT_STATUS.md` for full status details.