# UI Adapter Module States

- Module: UI Adapter
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

This document defines the internal state machine of the UI Adapter Module.

The UI Adapter manages the lifecycle of views, rendering, navigation, dialogs and user interactions while remaining independent of business logic and UI frameworks.

---

# State Principles

## Single Active State

The UI Adapter operates in one primary state at a time.

---

## Deterministic Transitions

Given the same state and input, the next state is always identical.

---

## Framework Independent

States describe logical UI behavior rather than framework-specific lifecycle events.

---

## Immutable ViewModels

Rendered ViewModels remain immutable until replaced by a newer version.

---

# State Model

```text
          Initialize
               │
               ▼
         Initializing
               │
               ▼
             Ready
      ┌────────┼────────────┐
      ▼        ▼            ▼
 Rendering Navigating WaitingForUser
      │        │            │
      └────────┴────────────┘
               │
               ▼
             Updating
               │
               ▼
             Ready
               │
      ┌────────┴─────────┐
      ▼                  ▼
    Failed            Shutdown
```

---

# State Summary

| State | Description |
|--------|-------------|
| Initializing | Loading UI resources and platform bindings |
| Ready | Waiting for events and user interaction |
| Rendering | Building or refreshing ViewModels |
| Navigating | Changing logical views |
| WaitingForUser | Awaiting user input |
| Updating | Applying UI updates |
| Failed | UI cannot continue safely |
| Shutdown | UI services terminated |

---

# Initializing

## Meaning

Load themes, localization resources, platform bindings and initial ViewModels.

## Exit Conditions

- Initialization completed
- Initialization failed

## Invariants

- User interaction unavailable.
- Commands are queued or rejected.

---

# Ready

## Meaning

The UI is operational.

## Allowed Operations

- OpenView
- Navigate
- ShowDialog
- ShowNotification
- SubmitPreferenceChange
- StartReadingSession

## Invariants

- ViewModels are available.
- Platform bindings are active.

---

# Rendering

## Meaning

Generating or refreshing immutable ViewModels.

## Exit Conditions

- Rendering completed
- Rendering failed

## Invariants

- Business logic is never executed.
- Existing ViewModels remain valid until replaced.

---

# Navigating

## Meaning

Switching between logical application views.

## Exit Conditions

- Navigation completed
- Navigation failed

## Invariants

- Navigation uses logical identifiers only.
- Platform routing remains internal.

---

# WaitingForUser

## Meaning

The UI is idle and awaiting interaction.

## Exit Conditions

- User action received
- Application event received

## Invariants

- No business processing occurs.

---

# Updating

## Meaning

Applying UI changes triggered by application events.

## Exit Conditions

- Update completed
- Update failed

## Invariants

- ViewModels remain immutable.
- Updates are presentation-only.

---

# Failed

## Meaning

An unrecoverable UI failure occurred.

## Allowed Operations

- RetryInitialization
- Shutdown

## Invariants

- UI rendering is suspended.

---

# Shutdown

## Meaning

UI resources have been released.

## Invariants

- No further UI operations are accepted.

---

# State Transition Table

| Current | Event | Next |
|---------|-------|------|
| Initializing | InitializationCompleted | Ready |
| Initializing | InitializationFailed | Failed |
| Ready | ViewRefreshRequested | Rendering |
| Ready | NavigationRequested | Navigating |
| Ready | UserIdle | WaitingForUser |
| Rendering | RenderingCompleted | Ready |
| Rendering | RenderingFailed | Failed |
| Navigating | NavigationCompleted | Ready |
| Navigating | NavigationFailed | Ready |
| WaitingForUser | UserActionReceived | Updating |
| WaitingForUser | ApplicationEventReceived | Updating |
| Updating | UpdateCompleted | Ready |
| Updating | UpdateFailed | Failed |
| Failed | RetryInitialization | Initializing |
| Failed | Shutdown | Shutdown |

---

# Transition Rules

## Rendering

Rendering updates ViewModels only.

---

## Navigation

Navigation never invokes business logic directly.

---

## Updates

Application events are translated into presentation updates.

---

## Recovery

Recovery restores UI functionality without affecting business modules.

---

# Architecture Invariants

1. UI Adapter never executes business logic.
2. Only immutable ViewModels are exposed.
3. Navigation is platform independent.
4. Rendering is separated from application processing.
5. Failed state rejects further UI operations until recovery.
6. Shutdown is terminal.
7. UI lifecycle never bypasses module contracts.

---

# Related Documents

- MODULE.md
- CONTRACT.md
- EVENTS.md
- ERRORS.md
- README.md
