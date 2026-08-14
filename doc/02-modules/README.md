# Business Modules

> **Project:** CRAI
> **Directory:** `doc/02-modules/`
> **Status:** Complete and Runtime v2 synchronized

This directory contains the business module documentation for CRAI.

---

## Purpose

`02-modules/` defines the **business responsibility boundary** of each CRAI business module:

- what the module owns
- what it accepts and produces
- how it communicates with other modules
- what errors it reports
- what states it transitions through

Business modules own **business meaning**. They do not own execution orchestration (that belongs to Runtime) or technical capabilities (those belong to `03-infrastructure/`).

---

## Module List

| Module | Primary Responsibility |
|---|---|
| `reading-session/` | Session identity, state, and lifecycle |
| `capture/` | Screen region capture and frame acquisition |
| `recognition/` | Image-to-structured-source (OCR business boundary) |
| `text-processing/` | OCR output normalization and source document preparation |
| `translation/` | Translation orchestration and provider coordination |
| `presentation/` | Semantic layout and rendered output |
| `storage/` | Persistence capability and schema evolution |
| `preferences/` | User preferences and reading configuration |
| `diagnostics/` | Observability and runtime health reporting |
| `provider-management/` | Provider registration, health, and lifecycle |
| `ui-adapter/` | Native UI rendering adapter and view model projection |

---

## Business Flow

```text
Reading Session
    ↓ establishes context
Capture (image path) or Structured Text Acquisition (text path)
    ↓
Recognition (image path only)
    ↓
Text Processing
    ↓
Translation
    ↓
Presentation
    ↓
UI Adapter
    ↓
Storage / Preferences (throughout)
```

---

## Standard Document Set

Each module directory contains the following standard files:

| File | Responsibility |
|---|---|
| `README.md` | Module overview, purpose, scope, and reading order |
| `MODULE.md` | Detailed module architecture and internal design |
| `CONTRACT.md` | Public interface: operations, types, and communication rules |
| `STATES.md` | State machines for module and its owned entities |
| `EVENTS.md` | Events the module publishes |
| `ERRORS.md` | Error taxonomy, codes, causes, and handling |

The `storage/` module also includes `MIGRATION.md` and `MODELS.md` due to its persistence responsibility.

---

## Module Status

All 11 business modules are:

- **Accepted** (boundaries and contracts reviewed)
- **Runtime v2 synchronized** (aligned with ExecutionScope/ExecutionRevision/WorkItem/Attempt model)
- **Not yet implemented** (implementation has not started)

See `.meta/PROJECT_STATUS.md` for full status.

---

## Design Rules

Module design rules are defined in `.meta/MODULE_ROLE.md`.

Module catalog (identity and ownership) is in `.meta/MODULES.md`.

Module dependency rules are in `01-architecture/modules/MODULE_DEPENDENCY.md`.

Module ownership map is in `01-architecture/modules/OWNERSHIP_MAP.md`.