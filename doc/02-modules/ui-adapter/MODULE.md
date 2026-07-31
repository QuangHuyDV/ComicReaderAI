# UI Adapter Module

- Module: UI Adapter
- Version: 1.0.0
- Status: Draft
- Owner: CRAI Architecture

---

# Purpose

The UI Adapter Module is the presentation bridge between the CRAI core and user interfaces.

It converts user interactions into application commands and transforms application state into UI-friendly models without embedding business logic.

The UI Adapter isolates the core architecture from any specific UI framework or platform.

---

# Responsibilities

The UI Adapter Module is responsible for:

- Receiving user input.
- Translating UI actions into application commands.
- Presenting application state.
- Managing UI-specific view models.
- Coordinating dialogs and notifications.
- Forwarding preference changes.
- Forwarding Reading Session commands.
- Managing platform-specific integrations.
- Handling localization resources.
- Supporting accessibility features.

---

# Out of Scope

The UI Adapter Module is NOT responsible for:

- OCR execution.
- Translation.
- Text processing.
- Preference validation.
- Reading session orchestration.
- Data persistence.
- Business rule evaluation.
- AI processing.

---

# Core Principles

## UI Agnostic

Business modules never depend on UI frameworks.

Supported platforms may include:

- Desktop
- Web
- Mobile
- Browser Extension

---

## Adapter Pattern

The UI communicates only through adapters.

Business modules never access UI controls directly.

---

## ViewModel Driven

UI receives immutable ViewModels generated from application state.

---

## Event Translation

The module translates:

UI Events

↓

Application Commands

and

Application Events

↓

UI Updates

---

# Owned Domain

UI Adapter owns:

- ViewModels
- Dialog Models
- Notification Models
- Window State
- Theme State
- Localization Resources
- Accessibility Metadata
- Platform Bindings

---

# Interaction with Other Modules

## Reading Session

Sends:

- Start Reading
- Stop Reading
- Resume Reading
- Retry Pipeline

Receives:

- Session State
- Progress
- Errors

---

## Preferences

Displays:

- Preference Definitions
- Current Values
- Validation Errors

Forwards user modifications.

---

## Storage

Displays storage status and maintenance progress.

Never accesses storage directly.

---

## Presentation

Receives rendered content and displays it using platform-specific UI.

---

## Diagnostics

Displays logs, metrics and troubleshooting information.

---

# Event Ownership

UI Adapter owns UI lifecycle events such as:

- ViewOpened
- ViewClosed
- DialogConfirmed
- NotificationShown
- ThemeChanged

Detailed definitions are provided in EVENTS.md.

---

# State Ownership

Typical internal states include:

- Initializing
- Ready
- Rendering
- WaitingForUser
- Updating
- Failed

Detailed definitions are provided in STATES.md.

---

# Error Ownership

UI Adapter owns UI-related errors including:

- ViewNotFound
- InvalidNavigation
- RenderingFailed
- ResourceMissing
- UnsupportedPlatform

Detailed definitions are provided in ERRORS.md.

---

# Design Principles

## Separation of Concerns

UI renders data.

Business modules own logic.

---

## Platform Independence

Business logic is reusable across all supported platforms.

---

## Stateless Communication

Commands and ViewModels are immutable.

---

## Accessibility First

All UI interactions should support accessibility requirements.

---

## Localization Ready

All user-visible text should originate from localization resources.

---

# Architecture Invariants

1. UI Adapter never contains business rules.
2. Business modules never reference UI frameworks.
3. ViewModels are immutable.
4. UI events are translated into application commands.
5. Application events are translated into UI updates.
6. Platform-specific code remains inside the UI Adapter.
7. UI Adapter never bypasses public module contracts.

---

# Related Documents

- CONTRACT.md
- EVENTS.md
- STATES.md
- ERRORS.md
- README.md
