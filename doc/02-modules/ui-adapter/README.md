# UI Adapter Module

The UI Adapter Module is the presentation bridge between the CRAI core architecture and every supported user interface.

It converts user interactions into application commands and transforms application state into immutable ViewModels suitable for rendering on different platforms.

The module isolates business logic from UI frameworks, allowing the same core architecture to power desktop, web, mobile and browser extension interfaces.

---

# Responsibilities

The UI Adapter is responsible for:

- Translating user actions into application commands.
- Transforming application events into UI updates.
- Managing ViewModels.
- Managing navigation.
- Managing dialogs.
- Managing notifications.
- Applying themes.
- Applying localization resources.
- Supporting accessibility features.
- Managing platform-specific integrations.

It is **not** responsible for:

- OCR execution
- Translation
- Reading Session orchestration
- Preference validation
- Data persistence
- Business rule evaluation
- AI processing

---

# Position in Architecture

```text
              User
                │
                ▼
         UI Framework
                │
                ▼
           UI Adapter
        ┌───────┼────────┐
        ▼       ▼        ▼
 Reading Session Preferences Presentation
        │
        ▼
Capture → Recognition → Text Processing → Translation → Presentation
```

The UI Adapter is the only module that communicates directly with UI frameworks.

---

# Responsibilities by Layer

## User Input

Receives:

- Mouse
- Keyboard
- Touch
- Shortcuts
- Menu actions

Converts them into application commands.

---

## View Rendering

Produces immutable ViewModels consumed by UI frameworks.

The UI never renders business objects directly.

---

## Navigation

Manages logical navigation between application views.

Business modules never reference windows, routes or pages.

---

## Dialog Management

Coordinates dialogs such as:

- Confirmation
- Error
- Warning
- Progress
- File selection

---

## Notification Management

Displays:

- Success notifications
- Warnings
- Errors
- Progress updates
- Background task completion

---

## Theme Management

Supports:

- Light theme
- Dark theme
- High contrast
- Custom themes

Theme implementation remains platform-specific.

---

## Localization

Provides localized resources.

Supports switching language without changing business logic.

---

## Accessibility

Supports:

- Keyboard navigation
- Screen readers
- High contrast
- Scalable text
- Focus management

---

# Interaction with Other Modules

## Reading Session

- Starts sessions
- Stops sessions
- Displays progress
- Displays errors

---

## Preferences

- Displays settings
- Submits preference changes
- Displays validation messages

---

## Presentation

Displays translated content using platform-specific rendering.

---

## Storage

Displays storage status and maintenance progress.

Does not access persistence directly.

---

## Diagnostics

Displays logs, metrics and troubleshooting information.

---

# Event Model

The module consumes application events and publishes UI lifecycle events.

Typical consumed events:

- ReadingSessionChanged
- EffectivePreferencesChanged
- PresentationUpdated
- StorageReady

Typical published events:

- ViewOpened
- NavigationCompleted
- DialogConfirmed
- ThemeChanged
- NotificationShown

---

# State Model

Typical internal states:

- Initializing
- Ready
- Rendering
- Navigating
- WaitingForUser
- Updating
- Failed

These states describe UI lifecycle only.

---

# Error Model

Typical errors include:

- ViewNotFound
- InvalidNavigation
- RenderingFailed
- ResourceMissing
- UnsupportedPlatform
- PlatformBindingFailed

Business errors originate from business modules rather than the UI Adapter.

---

# Design Principles

## UI Agnostic

Business modules never depend on a UI framework.

---

## Adapter Pattern

The UI communicates with the application only through the UI Adapter.

---

## Immutable ViewModels

Rendered data is immutable.

---

## Separation of Concerns

UI displays information.

Business modules execute application logic.

---

## Platform Independence

The same application core can support multiple UI technologies.

---

## Accessibility First

Accessibility is considered a first-class architectural concern.

---

# Supported Platforms

Potential platforms include:

- Windows Desktop
- Linux Desktop
- macOS Desktop
- Web
- Browser Extension
- Mobile

Only the UI Adapter changes between platforms.

---

# Related Documents

| Document | Description |
|----------|-------------|
| MODULE.md | Responsibilities and architecture |
| CONTRACT.md | Public contracts |
| EVENTS.md | Event definitions |
| STATES.md | State machine |
| ERRORS.md | Error model |

---

# Summary

The UI Adapter Module isolates presentation concerns from business logic by translating user interactions into application commands and exposing immutable ViewModels to UI frameworks. It enables CRAI to support multiple platforms while keeping the core architecture clean, reusable and independent of any specific UI technology.
