# Change Management Rules

> **Project:** CRAI
> **Document:** Change Management Rules
> **Path:** `.meta/CHANGE_RULE.md`
> **Version:** 1.0
> **Status:** Active

---

## 1. Purpose

This document defines the rules for proposing, reviewing, and applying changes to the CRAI codebase and architecture.

These rules apply to all contributors and AI assistants.

---

## 2. Change Scope

Every change must be classified by scope:

### Architecture Change

Any modification to:

- module boundaries
- public contracts
- dependency direction
- runtime execution model
- event definitions that cross module boundaries
- data ownership

**Requires:** documentation update before implementation.

**Requires:** explicit approval from the project owner.

### Module Change

Any modification to:

- internal module behavior
- module-owned data structures
- module state machines
- module error codes

**Requires:** documentation update in the affected module directory.

### Implementation Change

Any modification to source code that does not change architecture or public contracts.

**Requires:** passing tests.

### Documentation Change

Any modification to documentation that does not change architecture or code.

**Requires:** consistency with current architecture decisions.

---

## 3. Architecture Change Rules

Before changing architecture:

1. Identify which documents are affected.
2. Draft the proposed change in writing.
3. Identify what existing documents conflict with the proposed change.
4. Discuss with the project owner before making changes.
5. Update documentation first.
6. Update implementation only after documentation is approved.

Do not silently change architecture in implementation.

Do not rename modules without discussion.

Do not introduce new dependencies without justification.

---

## 4. Change Priority

When changes conflict with each other:

```text
User Request
    ↓
Architecture Decision
    ↓
Project Rules
    ↓
Coding Style
    ↓
Implementation Convenience
```

Never violate a higher priority for convenience.

---

## 5. Scope Control

Only modify files related to the current task.

Avoid:

- unnecessary refactoring
- unrelated fixes during a scoped task
- changing public interfaces without approval
- optimizing prematurely

Keep changes localized.

---

## 6. Breaking Changes

Breaking changes to public contracts must:

- be documented explicitly
- provide a migration path
- be announced before implementation
- not be introduced silently in implementation files

---

## 7. Rollback

Changes that introduce instability should be reverted before the next session.

Architecture changes that are found to conflict with existing decisions must be rolled back and redesigned.

---

## 8. AI Constraints

AI must not:

- invent requirements not discussed with the project owner
- silently rewrite architecture
- rename modules without discussion
- introduce new dependencies without justification
- modify unrelated components
- optimize prematurely
- rewrite working code unless requested

When uncertain: **ask first**.
