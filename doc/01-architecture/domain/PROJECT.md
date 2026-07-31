# Project Domain

- **Document:** Domain / Project
- **Version:** 1.0.0
- **Status:** Draft
- **Owner:** CRAI Architecture

---

# Purpose

A Project is the highest-level business entity in CRAI.

It groups every resource required to translate and manage a single work (manga, comic, novel, manhua, manhwa, etc.) throughout its lifecycle.

The Project acts as the root aggregate for the Domain Model.

---

# Responsibilities

A Project is responsible for:

- Managing project metadata
- Organizing books and chapters
- Defining source/target languages
- Owning glossary and character definitions
- Managing translation settings
- Tracking translation progress
- Managing project-level permissions
- Providing project-wide configuration

---

# Aggregate Structure

```text
Project
├── Books
├── Chapters
├── Pages
├── Characters
├── Glossary
├── Translation Profiles
├── Sessions
├── Assets
└── Settings
```

---

# Identity

Every project has a stable identifier.

Typical fields:

- Project ID
- Name
- Description
- Owner
- Created Time
- Updated Time
- Version
- Status

The identifier never changes.

---

# Project Types

Supported project types may include:

- Manga
- Manhwa
- Manhua
- Comic
- Novel
- Light Novel
- Web Novel
- Mixed Content

Project type influences downstream workflows but not the domain model.

---

# Lifecycle

```text
Created
   │
   ▼
Configured
   │
   ▼
Importing
   │
   ▼
Ready
   │
   ▼
Active
   │
   ▼
Archived
```

Projects may be restored from the Archived state.

---

# Configuration

Project configuration may define:

- Source language
- Target language
- OCR mode
- Translation mode
- Rendering mode
- AI profile
- Default glossary
- Cache policy

---

# Relationships

A Project owns:

- Books
- Chapters
- Pages
- Translation sessions
- Character registry
- Glossary
- Assets

Cross-project references should be avoided.

---

# Progress

Example project metrics:

- Imported pages
- Translated pages
- Reviewed pages
- OCR coverage
- Translation completion
- Last activity

Progress is derived from child entities.

---

# Security

Permissions may include:

- Owner
- Maintainer
- Translator
- Reviewer
- Viewer

Authorization is evaluated at the project boundary.

---

# Events

Typical domain events:

- ProjectCreated
- ProjectUpdated
- ProjectArchived
- ProjectRestored
- ProjectDeleted
- SettingsChanged

---

# Invariants

1. Every entity belongs to exactly one Project.
2. Project ID is immutable.
3. A Project owns its glossary and character registry.
4. Configuration changes are versioned.
5. Archived projects are read-only.
6. Child entities cannot outlive their Project.
7. Project boundaries define transactional consistency.

---

# Related Documents

- README.md
- BOOK.md
- CHAPTER.md
- PAGE.md
- IMAGE.md
- TEXT_BLOCK.md
- TRANSLATION.md
- LANGUAGE.md
- GLOSSARY.md
- CHARACTER.md
- SESSION.md
- WORKSPACE.md
- PROFILE.md
