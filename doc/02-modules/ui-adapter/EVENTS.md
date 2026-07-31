# UI Adapter Module Events

- Module: UI Adapter
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

This document defines the events consumed and published by the UI Adapter Module.

The UI Adapter translates application events into UI updates and user interactions into application commands while remaining independent of any specific UI framework.

---

# Event Principles

## Event-Driven UI

The UI updates in response to application events instead of polling module state.

---

## Immutable Events

All published events are immutable after publication.

---

## Platform Independent

Events describe logical UI behavior rather than platform-specific controls or widgets.

---

## Stateless Communication

Events carry all information required for consumers to process them.

---

# Event Naming Convention

Events use the past-tense convention.

Examples

```text
ViewOpened
NavigationCompleted
ThemeChanged
DialogConfirmed
NotificationShown
```

---

# Consumed Events

## ReadingSessionChanged

Purpose

Refresh reading interface and progress.

---

## EffectivePreferencesChanged

Purpose

Refresh UI settings affected by preference changes.

---

## PresentationUpdated

Purpose

Display newly rendered content.

---

## DiagnosticsUpdated

Purpose

Refresh diagnostics panels and status indicators.

---

## StorageReady

Purpose

Update storage status in the UI.

---

## StorageFailed

Purpose

Display storage failure information.

---

## TranslationCompleted

Purpose

Refresh translated content.

---

## RecognitionCompleted

Purpose

Refresh recognized text when appropriate.

---

# Published Events

## ViewOpened

Purpose

A view has been opened.

---

## ViewClosed

Purpose

A view has been closed.

---

## NavigationCompleted

Purpose

Navigation to a target view completed successfully.

---

## DialogOpened

Purpose

A dialog became visible.

---

## DialogConfirmed

Purpose

The user confirmed a dialog.

---

## DialogCancelled

Purpose

The user cancelled a dialog.

---

## NotificationShown

Purpose

A notification was displayed.

---

## ThemeChanged

Purpose

The active UI theme changed.

---

## LocalizationChanged

Purpose

The active language resources changed.

---

## AccessibilityModeChanged

Purpose

Accessibility configuration changed.

---

# Event Ordering

Typical navigation sequence

```text
OpenView
    ↓
ViewOpened
    ↓
NavigationCompleted
```

Typical dialog sequence

```text
DialogOpened
      ↓
DialogConfirmed
or
DialogCancelled
```

---

# Event Ordering Rules

1. ViewOpened precedes NavigationCompleted.
2. DialogConfirmed and DialogCancelled are mutually exclusive.
3. ThemeChanged occurs after theme resources are applied.
4. LocalizationChanged occurs after language resources are loaded.
5. Published events represent completed UI state changes.

---

# Event Idempotency

Duplicate events should not produce duplicate UI state changes.

Consumers may identify duplicates using:

- EventId
- Timestamp
- View Identifier (when applicable)

---

# Event Delivery

Recommended guarantees:

- At-least-once delivery
- Ordered within the same UI interaction
- Immutable after publication

---

# Failure Handling

If an event cannot be processed:

- Preserve the previous UI state.
- Retry transient operations when appropriate.
- Surface recoverable failures to the user.

---

# Architecture Invariants

1. Events never contain business logic.
2. Events are platform independent.
3. Published events describe completed UI changes.
4. Duplicate events do not corrupt UI state.
5. Sensitive information is never included in UI events.

---

# Future Events

Potential future events include:

- WindowResized
- OverlayActivated
- OverlayHidden
- ShortcutTriggered
- ExtensionConnected
- ExtensionDisconnected

---

# Related Documents

- MODULE.md
- CONTRACT.md
- STATES.md
- ERRORS.md
- README.md
