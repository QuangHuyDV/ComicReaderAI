# CRAI Documentation

> Project: Comic Reader AI  
> Project Key: CRAI  
> Status: Architecture Phase

---

## 1. Purpose

This directory contains the product, architecture, design, implementation, testing, and operational documentation for CRAI.

The documentation is organized by responsibility so that contributors and AI assistants can locate the correct source of truth without reading the entire repository.

---

## 2. Recommended Reading Order

For a new contributor or AI session:

1. `../.meta/AI_BOOT.md`
2. `../.meta/PROJECT.md`
3. `00-project/README.md`
4. `00-project/USER_JOURNEY.md`
5. `01-architecture/README.md`
6. `01-architecture/CAPABILITY_MAP.md`
7. `01-architecture/MODULE_MAP.md`
8. `01-architecture/DATA_FLOW.md`
9. `01-architecture/STATE_MACHINE.md`
10. `01-architecture/EVENT_BUS.md`
11. `01-architecture/flows/README.md`
12. `01-architecture/runtime/README.md`

Read feature-specific directories only when working on that area.

---

## 3. Directory Map

| Directory | Responsibility |
|---|---|
| `00-project` | Product vision, scope, terminology, and user journeys |
| `01-architecture` | System-wide architecture and runtime decisions |
| `02-reader` | Reading-session and reader behavior |
| `03-ocr` | OCR capabilities, providers, and text extraction |
| `04-layout` | Comic layout analysis and reading order |
| `05-translation` | Translation pipeline and language behavior |
| `06-ai` | AI models, orchestration, prompts, and evaluation |
| `07-database` | Persistent storage and schema design |
| `08-cache` | Cache design, keys, and invalidation |
| `09-ui` | User interface and presentation behavior |
| `10-plugin` | Extension and provider plugin architecture |
| `11-testing` | Test strategy and quality requirements |
| `12-release` | Packaging, distribution, and release process |
| `13-devlog` | Development notes and progress records |
| `14-decisions` | Architecture Decision Records |
| `15-prompts` | Maintained AI prompts and prompt contracts |
| `16-rules` | Domain-specific rules not covered by `.meta` |
| `17-roadmap` | Milestones, phases, and delivery planning |
| `18-api` | Internal and external contracts |
| `19-security` | Privacy, permissions, and security boundaries |
| `20-performance` | Benchmarks, profiling, and measured results |

---

## 4. Documentation Boundaries

### `.meta`

Contains active instructions and rules for contributors and AI assistants.

Examples:

- coding rules
- documentation rules
- architecture change rules
- session workflow

### `doc`

Contains knowledge about the product and system.

Examples:

- product requirements
- architecture
- module design
- runtime behavior
- testing plans
- benchmarks

A product or architecture document must not be stored in `.meta`.

---

## 5. Status Labels

Documents should use one of the following statuses:

| Status | Meaning |
|---|---|
| Exploration | The problem is still being investigated |
| Draft | A concrete proposal exists but is not approved |
| Review | The proposal is ready for validation |
| Stable | The decision is accepted and used as a source of truth |
| Deprecated | The document has been replaced |
| Archived | The document is retained only for historical context |

---

## 6. Naming Rules

- Use uppercase snake case for major architecture documents.
- Use `README.md` as the entry point of each significant directory.
- Use plural directory names for collections such as `flows`.
- Avoid ambiguous names such as `NOTES.md` or `MISC.md`.
- Every document should have one primary responsibility.

---

## 7. Current Project Focus

The current architecture focus is:

```text
Screen Comic Translation MVP
```

The current documentation sequence is:

```text
Architecture Foundation
    ↓
Screen Comic Flow
    ↓
Runtime Pipeline
    ↓
Queue and Scheduler
    ↓
Cancellation and Resources
    ↓
Module Contracts
    ↓
Implementation Planning
```