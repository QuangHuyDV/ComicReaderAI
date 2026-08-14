# CRAI Documentation

> Project: Comic Reader AI
> Project Key: CRAI
> Status: Architecture Phase — Technology Selection

---

## 1. Purpose

This directory contains the product, architecture, module, infrastructure, and technology documentation for CRAI.

Documentation is organized by responsibility so that contributors and AI assistants can locate the correct source of truth without reading the entire repository.

---

## 2. Recommended Reading Order

For a new contributor or AI session:

1. `.meta/AI_BOOT.md`
2. `.meta/PROJECT.md`
3. `.meta/PROJECT_STATUS.md`
4. `doc/README.md` (this file)
5. `doc/00-project/README.md`
6. `doc/00-project/USER_JOURNEY.md`
7. `doc/01-architecture/core/README.md`
8. `doc/01-architecture/core/CAPABILITY_MAP.md`
9. `doc/01-architecture/modules/MODULE_MAP.md`
10. `doc/01-architecture/flows/README.md`
11. `doc/01-architecture/runtime/README.md`

Read feature-specific directories only when working on that area.

---

## 3. Directory Map

| Directory | Responsibility |
|---|---|
| `00-project/` | Product vision, scope, and user journeys |
| `01-architecture/` | System-wide architecture (core, runtime, flows, OCR, AI, plugin, domain, text, translate, modules) |
| `02-modules/` | Business module contracts, states, events, and errors |
| `03-infrastructure/` | Infrastructure module contracts, states, events, and errors |
| `04-technology/` | Technology evaluation and selection documents |

### `01-architecture/` subdirectories

| Subdirectory | Responsibility |
|---|---|
| `core/` | Capability map, data flow, event bus, state machine |
| `domain/` | Domain model (Book, Chapter, Session, Translation, etc.) |
| `flows/` | End-to-end product flows (Screen Comic, Structured Text, Content Change, Reading Session) |
| `modules/` | Module map, ownership map, dependency rules |
| `runtime/` | Runtime execution model (Pipeline, Queue, Scheduler, Cancellation, Memory, Threading, etc.) |
| `ocr/` | OCR pipeline, detection, recognition, preprocessing, reading order |
| `text/` | Text model and segmentation architecture |
| `translate/` | Translation architecture and context |
| `ai/` | AI pipeline, models, prompts, routing, cost control |
| `plugin/` | Plugin system, lifecycle, discovery, security, versioning |

### `02-modules/` subdirectories

| Module | Responsibility |
|---|---|
| `capture/` | Screen region capture and frame acquisition |
| `reading-session/` | Reading session identity, state, and lifecycle |
| `recognition/` | Image-to-structured-source (OCR) business boundary |
| `text-processing/` | OCR output normalization and source document preparation |
| `translation/` | Translation orchestration and provider coordination |
| `presentation/` | Semantic layout and rendered output |
| `storage/` | Persistence capability and schema evolution |
| `preferences/` | User preferences and reading configuration |
| `diagnostics/` | Observability and runtime health reporting |
| `provider-management/` | Provider registration, health, and lifecycle |
| `ui-adapter/` | Native UI rendering adapter and view model projection |

### `03-infrastructure/` subdirectories

| Module | Responsibility |
|---|---|
| `configuration/` | Application configuration loading and validation |
| `event-bus/` | Asynchronous inter-module event transport |
| `logging/` | Structured logging |
| `telemetry/` | Metrics, tracing, and observability |
| `scheduler/` | Background job scheduling and execution supervision |
| `resource-manager/` | Shared resource lifecycle, lease, pool, and health |
| `secret-management/` | Secure credential storage and access |

### `04-technology/` files

| File | Responsibility |
|---|---|
| `TECH_STACK.md` | Core language, runtime, and framework candidates |
| `WINDOWS_PLATFORM.md` | Windows desktop platform evaluation |
| `PERSISTENCE.md` | Persistence and database technology evaluation |
| `OCR_CANDIDATES.md` | OCR provider and library candidates |
| `TRANSLATION_CANDIDATES.md` | Translation provider candidates |
| `BUILD_AND_PACKAGING.md` | Build system and packaging evaluation |
| `FEASIBILITY_RESULTS.md` | Feasibility study results and decisions |

---

## 4. Documentation Boundaries

### `.meta/`

Contains active rules and instructions for contributors and AI assistants.

Examples:

- `AI_BOOT.md` — bootstrap for every AI session
- `PROJECT.md` — project identity and goals
- `PROJECT_RULE.md` — engineering principles
- `MODULE_ROLE.md` — module design rules
- `PROJECT_STATUS.md` — current architecture status and history

A product or architecture document must not be stored in `.meta/`.

### `doc/`

Contains knowledge about the product and system.

Examples:

- product requirements and user journeys
- architecture design
- module contracts
- runtime behavior
- technology decisions

---

## 5. Status Labels

Documents should use one of the following statuses:

| Status | Meaning |
|---|---|
| Exploration | The problem is still being investigated |
| Draft | A concrete proposal exists but is not approved |
| Review | The proposal is ready for validation |
| Stable | The decision is accepted and used as source of truth |
| Deprecated | The document has been replaced |
| Archived | Retained only for historical context |

---

## 6. Naming Rules

- Use uppercase snake case for major architecture documents: `CAPABILITY_MAP.md`
- Use `README.md` as the entry point for each significant directory
- Use plural directory names for collections: `flows/`
- Avoid ambiguous names such as `NOTES.md` or `MISC.md`
- Every document should have one primary responsibility

---

## 7. Current Project Status

Architecture phase is complete.

Current phase:

```text
Technology Selection
```

See `.meta/PROJECT_STATUS.md` for full status and next steps.