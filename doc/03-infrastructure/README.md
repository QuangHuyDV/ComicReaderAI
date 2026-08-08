# Infrastructure Architecture

---

# Purpose

The Infrastructure layer provides the technical capabilities required by the CRAI runtime and business modules.

Unlike business modules, infrastructure modules do not own business rules or application workflows.

Their responsibility is to provide reusable technical services through stable abstractions.

Examples include:

- event communication
- scheduling
- resource lifecycle management
- telemetry
- logging
- configuration
- secret handling

---

# Goals

Infrastructure exists to:

- isolate implementation details
- provide reusable technical capabilities
- improve portability
- improve testability
- support future technology replacement

Infrastructure should never define business behavior.

---

# Architecture Position

```text
Application
│
├── Business Modules
│       own business meaning
│
├── Runtime
│       owns execution orchestration
│
└── Infrastructure
        provides technical capabilities
```

Business modules request infrastructure services only through public contracts.

Infrastructure never owns business decisions.

---

# Infrastructure Modules

## Configuration

Owns application configuration loading, validation and configuration providers.

It does not decide how configuration affects business behavior.

---

## Event Bus

Provides asynchronous communication between modules.

It transports events but never interprets business meaning.

---

## Logging

Provides structured logging facilities.

It records information but never performs diagnostics or business analysis.

---

## Resource Manager

Provides lifecycle management for shared runtime resources and artifacts.

Responsibilities include:

- registration
- ownership
- lease
- retention
- disposal

Business modules never manipulate shared resources directly.

---

## Scheduler

Controls admission of work into execution.

It owns scheduling policies but never executes business logic.

---

## Secret Management

Provides secure access to credentials and sensitive configuration.

Secrets never travel through normal public events.

---

## Telemetry

Collects metrics, traces and runtime observations.

Telemetry observes runtime behavior without changing it.

---

# Ownership

Infrastructure owns technical implementation.

Business modules own business semantics.

Runtime owns execution authority.

Storage owns persistence.

---

# Dependency Rules

Business modules may depend on Infrastructure abstractions.

Infrastructure must not depend on business modules.

Infrastructure modules should remain loosely coupled.

Cross-module communication should occur through public contracts or events.

---

# Design Principles

Infrastructure should be:

- implementation-independent
- replaceable
- testable
- deterministic
- observable
- reusable

Business logic must never leak into infrastructure.

---

# Relationship with Runtime

Runtime coordinates execution.

Infrastructure provides the capabilities Runtime requires.

Example:

```text
Runtime
    ↓
Scheduler
    ↓
Resource Manager
    ↓
Telemetry
```

Runtime decides **when** something happens.

Infrastructure decides **how** the technical service is performed.

---

# Relationship with Business Modules

Business modules request infrastructure capabilities.

Examples:

Capture
    → Scheduler

Recognition
    → Resource Manager

Translation
    → Event Bus

Presentation
    → Configuration

Business modules remain unaware of implementation details.

---

# Reading Order

For contributors new to the infrastructure layer:

1. README.md
2. MODULE.md
3. CONTRACT.md
4. STATES.md
5. EVENTS.md
6. ERRORS.md

The README introduces the purpose of each module.

Detailed behavior belongs to the module documents.

---

# Architecture Invariants

The Infrastructure layer guarantees:

- no business ownership
- stable public contracts
- replaceable implementations
- technology independence
- deterministic behavior
- clear module boundaries