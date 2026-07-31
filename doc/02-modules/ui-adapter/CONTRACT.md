# UI Adapter Module Contract

- Module: UI Adapter
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

This document defines the public contract of the UI Adapter Module.

The UI Adapter provides a stable interface between user interfaces and the CRAI core. It converts UI interactions into application commands and transforms application state into immutable ViewModels.

---

# Public Commands

## OpenView

Open a UI view.

---

## CloseView

Close an existing view.

---

## Navigate

Navigate between application views.

---

## ShowDialog

Display a dialog.

---

## CloseDialog

Close an active dialog.

---

## ShowNotification

Display a user notification.

---

## UpdateTheme

Apply a UI theme change.

---

## UpdateLocalization

Switch application language resources.

---

## SubmitPreferenceChange

Forward a preference update request to the Preferences Module.

---

## StartReadingSession

Forward a request to start a Reading Session.

---

## StopReadingSession

Forward a request to stop the active Reading Session.

---

## RetryPipeline

Forward a retry request to the Reading Session.

---

# Public Queries

## GetCurrentView

Returns the active view.

---

## GetViewModel

Returns the immutable ViewModel for a view.

---

## GetTheme

Returns the active UI theme.

---

## GetLocalization

Returns the active localization resource.

---

## GetWindowState

Returns current window state.

---

# ViewModel Contract

Every ViewModel should:

- Be immutable.
- Contain only presentation data.
- Exclude business logic.
- Be serializable when appropriate.

---

# Consumed Events

| Event | Purpose |
|--------|---------|
| ReadingSessionChanged | Refresh reading UI |
| EffectivePreferencesChanged | Refresh settings and affected views |
| PresentationUpdated | Display rendered content |
| DiagnosticsUpdated | Refresh diagnostics panels |
| StorageReady | Update storage status |

---

# Published Events

| Event | Purpose |
|--------|---------|
| ViewOpened | UI view opened |
| ViewClosed | UI view closed |
| NavigationCompleted | Navigation completed |
| DialogConfirmed | User confirmed dialog |
| DialogCancelled | User cancelled dialog |
| NotificationShown | Notification displayed |
| ThemeChanged | Theme updated |
| LocalizationChanged | Language resources updated |

---

# Navigation Contract

Navigation is expressed through logical view identifiers.

Business modules must not depend on routes, windows or UI framework concepts.

---

# Platform Contract

Supported UI platforms may include:

- Desktop
- Web
- Mobile
- Browser Extension

The public contract remains identical across platforms.

---

# Accessibility Contract

The UI Adapter should support:

- Keyboard navigation
- Screen readers
- High contrast themes
- Scalable text
- Focus management

Accessibility implementation remains platform-specific.

---

# Security Contract

The UI Adapter must never expose:

- Secrets
- Provider credentials
- Sensitive storage information
- Internal persistence details

Only presentation-safe information is displayed.

---

# Error Contract

Operations may return:

- ViewNotFound
- InvalidNavigation
- RenderingFailed
- ResourceMissing
- UnsupportedPlatform
- DialogFailed

Detailed definitions are provided in ERRORS.md.

---

# Architecture Invariants

1. UI Adapter never executes business logic.
2. ViewModels are immutable.
3. UI events become application commands.
4. Application events become UI updates.
5. Platform-specific implementation remains internal.
6. Business modules never depend on UI framework APIs.
7. Public contracts remain stable across supported platforms.

---

# Related Documents

- MODULE.md
- EVENTS.md
- STATES.md
- ERRORS.md
- README.md
