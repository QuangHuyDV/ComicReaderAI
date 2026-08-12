# Project Domain

* **Document:** Domain / Project
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

A `Project` is the primary domain container for a reading and translation work managed by CRAI.

A Project represents one logical work or collection of closely related content, such as:

* Manga
* Manhwa
* Manhua
* Comic
* Novel
* Light Novel
* Web Novel
* Mixed text and image content

The Project provides the stable domain boundary used to organize content, configuration, translation context, reading state, and project-scoped resources.

A Project is **not** a single transactional aggregate containing every entity associated with the work.

Instead, it provides the common identity and scope under which multiple domain aggregates and modules operate.

---

# Domain Role

The Project acts as:

```text
Project
│
├── Content Scope
│   ├── Books
│   ├── Chapters
│   ├── Pages
│   └── Assets
│
├── Language Context
│   ├── Source Language
│   └── Target Language
│
├── Translation Context
│   ├── Glossary
│   ├── Character Registry
│   └── Translation Preferences
│
├── Reading Context
│   └── Reading Progress / Sessions
│
└── Project Configuration
```

The Project defines **scope and identity**.

Individual modules remain responsible for their own domain state, lifecycle, processing rules, and transactional boundaries.

---

# Responsibilities

A Project is responsible for defining and maintaining:

* Project identity
* Project metadata
* Project type
* Source and target language context
* Content hierarchy references
* Project-scoped configuration
* Project-scoped glossary association
* Project-scoped character registry association
* Translation preferences
* Reading preferences
* Project lifecycle
* Project-level access scope
* Project-wide resource discovery

A Project may expose derived information such as translation or reading progress, but those values are calculated from module-owned state.

The Project does not perform OCR, translation, layout reconstruction, rendering, recognition, or reading-session processing itself.

---

# Project Boundary

Every project has a stable `projectId`.

Resources belonging to the same logical work should normally reference the same Project.

Example:

```text
Project
  |
  +-- Book
  |     |
  |     +-- Chapter
  |             |
  |             +-- Page
  |
  +-- Glossary
  |
  +-- Character Registry
  |
  +-- Translation Configuration
  |
  +-- Reading State
```

The Project boundary provides:

* logical grouping
* configuration scope
* authorization scope
* resource lookup scope
* project-level lifecycle control

It does **not** imply that all project resources must be updated within one transaction.

---

# Identity

Every Project has an immutable identifier.

Typical fields include:

```text
Project
├── projectId
├── name
├── description
├── projectType
├── sourceLanguage
├── targetLanguage
├── status
├── createdAt
├── updatedAt
└── version
```

`projectId` MUST remain stable throughout the lifetime of the Project.

Project names MAY change.

Project metadata MAY evolve independently from content processing state.

---

# Project Types

Supported project types may include:

```text
MANGA
MANHWA
MANHUA
COMIC
NOVEL
LIGHT_NOVEL
WEB_NOVEL
MIXED_CONTENT
```

Project type describes the dominant content model of the work.

It MAY influence:

* default import behavior
* OCR defaults
* text-direction defaults
* presentation defaults
* translation defaults
* reading behavior

Project type MUST NOT determine module ownership.

Modules decide their behavior through explicit configuration and capabilities rather than relying exclusively on project type.

---

# Content Hierarchy

A Project may contain one or more Books.

A Book may contain Chapters.

A Chapter may contain Pages or other ordered content units.

Typical hierarchy:

```text
Project
└── Book
    └── Chapter
        └── Page
```

Some content types MAY omit certain hierarchy levels.

For example:

```text
Project
└── Chapter
    └── Page
```

or:

```text
Project
└── Book
    └── Chapter
        └── Text Content
```

The Project domain MUST NOT assume that every supported content type is page-based.

---

# Relationships

A Project may reference or scope:

* Books
* Chapters
* Pages
* Source assets
* Derived assets
* Glossaries
* Character registries
* Translation configurations
* Presentation configurations
* Reading state
* Reading sessions
* processing history

These relationships represent domain ownership or scope.

They do not necessarily represent aggregate containment.

---

# Aggregate Boundaries

Project is intentionally not modeled as one large aggregate.

The following entities or concerns may form independent aggregates:

```text
Project

Book Aggregate
Chapter Aggregate
Page Aggregate
Glossary Aggregate
Character Registry Aggregate
Reading Session Aggregate
Translation Job Aggregate
Processing Job Aggregate
```

Each aggregate is responsible for maintaining its own invariants.

Cross-aggregate coordination MUST use application workflows, services, or domain events where appropriate.

A Project operation MUST NOT require loading the complete Project content graph.

---

# Configuration

Project configuration contains defaults and user intent that apply across the Project.

Possible project-level configuration includes:

```text
ProjectConfiguration
├── sourceLanguage
├── targetLanguage
├── readingPreferences
├── translationPreferences
├── presentationPreferences
└── moduleOverrides
```

Project configuration SHOULD describe **policy and preferences**.

Operational execution state belongs to the module responsible for that operation.

For example:

```text
Project
    translation preference
        |
        v
Translation Module
    actual translation job / provider / execution state
```

Similarly:

```text
Project
    OCR preference
        |
        v
OCR Architecture / Recognition Modules
    actual detection / recognition execution
```

Project configuration MUST NOT duplicate module-owned runtime state.

---

# Module Configuration

Project configuration MAY provide defaults consumed by individual modules.

Examples include:

* preferred source language
* target language
* translation quality preference
* preferred OCR strategy
* text direction preference
* presentation preference
* reading preference

Modules MUST validate whether a project-level preference is supported.

Unsupported settings MUST NOT silently alter module behavior.

Module-specific runtime configuration remains owned by the corresponding module.

---

# Language Context

A Project SHOULD define the language context for its source content.

Typical properties:

```text
sourceLanguage
targetLanguages
```

A Project MAY support:

* one source language
* one target language
* multiple target languages
* unknown or auto-detected source language

Content-level language metadata MAY override Project defaults when required.

Language detection results belong to the processing pipeline and MUST NOT automatically mutate Project configuration unless explicitly accepted.

---

# Translation Context

A Project provides the common context required for consistent translation.

This MAY include:

* glossary reference
* character registry reference
* naming conventions
* translation preferences
* terminology rules
* style preferences

These resources MAY be maintained by dedicated aggregates or modules.

The Project references them as project-scoped resources.

Translation execution remains owned by the Translation capability.

---

# Glossary

A Project MAY have one or more glossary resources.

Glossaries may contain:

* character names
* locations
* organizations
* titles
* terminology
* recurring expressions
* protected terms

Glossary ownership MUST remain explicit.

A Project MAY define a default glossary while individual translation operations MAY use additional scoped glossaries.

---

# Character Registry

A Project MAY maintain a Character Registry.

The registry provides stable identity and translation context for recurring characters.

Possible information includes:

* canonical name
* source-language name
* translated name
* aliases
* relationships
* notes
* naming rules

Character Registry state is project-scoped but SHOULD remain an independent aggregate when its complexity requires independent updates.

---

# Progress

Project progress is derived information.

Examples include:

```text
Content Progress
├── chaptersImported
├── pagesImported
└── assetsAvailable

Processing Progress
├── OCR coverage
├── translation coverage
└── presentation coverage

Reading Progress
├── chaptersRead
├── lastReadChapter
└── lastActivity
```

Project MUST NOT own the authoritative processing state for these values.

Instead:

```text
Module State
    |
    v
Derived Project Progress
```

Progress MAY be cached for performance.

Cached progress MUST be reconstructable from authoritative domain state.

---

# Lifecycle

Project lifecycle represents whether the logical work remains available for normal use.

Recommended lifecycle:

```text
Created
   |
   v
Active
   |
   v
Archived
```

Optional transitional states MAY exist when required:

```text
Created
   |
   v
Initializing
   |
   v
Active
```

Importing content MUST NOT normally be modeled as a Project lifecycle state.

An Active Project MAY:

* import additional Books
* import new Chapters
* add new Pages
* rerun OCR
* rerun translations
* change presentation settings
* continue reading

Import and processing lifecycles belong to their respective jobs or modules.

---

# Project Status

Recommended core statuses:

```text
CREATED
ACTIVE
ARCHIVED
```

Optional future statuses MAY include:

```text
DELETING
DELETED
```

`ARCHIVED` means the Project is no longer active for normal modification.

Archived Projects SHOULD remain readable unless explicitly restricted.

A Project MAY be restored from `ARCHIVED` to `ACTIVE`.

---

# Archive Behavior

Archiving a Project SHOULD:

* prevent normal content modification
* prevent new processing jobs
* preserve existing project content
* preserve translation history
* preserve reading state
* preserve derived assets according to retention policy

Archiving MUST NOT imply immediate data deletion.

---

# Deletion

Project deletion is separate from archive.

Deletion MAY require:

* authorization
* retention checks
* active-job checks
* dependency checks
* storage cleanup
* audit recording

Deletion SHOULD normally be asynchronous when the Project contains large numbers of assets or derived artifacts.

A deleted Project MUST NOT leave active processing jobs referencing it.

---

# Permissions

Project may provide an authorization scope.

Possible project-scoped roles include:

```text
OWNER
MAINTAINER
TRANSLATOR
REVIEWER
VIEWER
```

The Project domain defines the scope against which access is evaluated.

Authentication and authorization infrastructure MAY be implemented elsewhere.

Project MUST NOT implement identity-provider behavior.

---

# Events

Typical Project domain events include:

```text
ProjectCreated
ProjectMetadataUpdated
ProjectConfigurationChanged
ProjectActivated
ProjectArchived
ProjectRestored
ProjectDeletionRequested
ProjectDeleted
```

Events SHOULD communicate changes to Project-owned state.

Module-specific execution events MUST remain owned by the corresponding module.

Examples:

```text
OCRCompleted
TranslationCompleted
ReadingSessionStarted
PresentationGenerated
```

These are not Project events simply because they occur within a Project.

---

# Invariants

The following invariants apply to Project:

1. `projectId` is immutable.

2. A Project always has a valid lifecycle status.

3. Project-scoped resources MUST reference an existing Project unless explicitly designed as reusable global resources.

4. Project configuration MUST NOT contain authoritative runtime state owned by another module.

5. Project lifecycle changes MUST NOT implicitly rewrite module-owned processing state.

6. Archived Projects MUST reject normal mutation operations unless explicitly allowed.

7. Deleting a Project MUST prevent creation of new project-scoped processing work.

8. Derived Project progress MUST NOT become the sole authoritative source of child or module state.

9. Aggregate boundaries MUST remain independent from the Project's logical resource hierarchy.

10. Cross-project references SHOULD be avoided unless a feature explicitly requires shared resources.

---

# Transactional Consistency

Project defines a logical consistency boundary, not a global transactional boundary.

A transaction SHOULD modify only the aggregate responsible for the operation.

Example:

```text
Update Project Metadata
    -> Project Aggregate transaction
```

```text
Update Glossary
    -> Glossary Aggregate transaction
```

```text
Translate Chapter
    -> Translation workflow / job transaction
```

```text
Update Reading Progress
    -> Reading state transaction
```

Cross-aggregate consistency SHOULD use:

* application orchestration
* domain events
* eventual consistency
* idempotent handlers

where appropriate.

---

# Cross-Project References

Cross-project references SHOULD remain uncommon.

They MAY be introduced for explicitly shared resources such as:

* reusable glossaries
* reusable translation profiles
* shared provider configurations
* global user preferences

Shared resources MUST NOT be modeled as Project-owned resources.

Instead, Projects SHOULD reference them through stable identifiers.

---

# Failure Isolation

Failure in one project-scoped operation SHOULD NOT corrupt unrelated Project state.

Examples:

```text
OCR failure
    != Project failure

Translation failure
    != Project failure

Presentation failure
    != Project failure

Reading session failure
    != Project failure
```

Module failures SHOULD remain isolated within their module lifecycle.

The Project MAY expose summarized failure information without owning the underlying error state.

---

# Scalability

Project design MUST support large works without requiring the entire content graph to be loaded.

A Project MAY contain:

```text
1..N Books
1..N Chapters
1..N Pages
1..N Assets
1..N Translation Records
1..N Reading Sessions
```

Project APIs SHOULD use identifiers, pagination, and module-specific queries instead of returning complete aggregate graphs.

---

# Example

```text
Project
  projectId: project_001
  name: "Example Manhua"
  projectType: MANHUA
  sourceLanguage: zh-CN
  targetLanguages:
    - vi-VN
  status: ACTIVE

  resources:
    books:
      - book_001

    glossary:
      - glossary_001

    characterRegistry:
      - characters_001

  preferences:
    translationProfile: translation_profile_001
    presentationProfile: presentation_profile_001
```

The identifiers above reference resources owned and managed by their respective domain areas.

The Project does not need to embed their complete state.

---

# Ownership Summary

```text
Project Domain
    owns
        Project identity
        Project metadata
        Project lifecycle
        Project-level preferences
        Project resource scope

    references
        Content hierarchy
        Glossary
        Character registry
        Translation configuration
        Presentation configuration
        Reading state

    does not own
        OCR execution
        Recognition execution
        Translation execution
        Rendering execution
        Presentation execution
        Reading-session execution
        Infrastructure state
```

This separation prevents Project from becoming a God Aggregate and preserves module ownership boundaries.

---

# Related Documents

Domain:

* `README.md`
* `BOOK.md`
* `CHAPTER.md`
* `PAGE.md`
* `IMAGE.md`
* `TEXT_BLOCK.md`
* `TRANSLATION.md`
* `LANGUAGE.md`
* `GLOSSARY.md`
* `CHARACTER.md`
* `SESSION.md`
* `WORKSPACE.md`
* `PROFILE.md`

Architecture:

* `docs/architecture/CAPABILITY_MAP.md`
* `docs/architecture/OWNERSHIP_MAP.md`
* `docs/architecture/STATE_MACHINE.md`
* `docs/architecture/EVENT_BUS.md`
* `docs/architecture/MODULE_DEPENDENCY.md`
* `docs/architecture/DATA_FLOW.md`

Module contracts remain authoritative for module-specific runtime ownership and behavior.
