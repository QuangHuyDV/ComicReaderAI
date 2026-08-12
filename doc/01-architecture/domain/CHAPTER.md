# Chapter Domain

* **Document:** Domain / Chapter
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

A `Chapter` represents one logical reading unit within a CRAI Project.

Depending on the source work, a Chapter may represent:

* a manga chapter,
* a comic issue or chapter,
* a manhua or manhwa chapter,
* a novel chapter,
* a web novel episode,
* a special chapter,
* an extra chapter,
* or another ordered reading unit.

A Chapter provides a stable domain identity and content scope for the material that users read and that CRAI processes.

A Chapter is **not** the aggregate root of all OCR, translation, rendering, review, and reading state associated with its content.

Processing capabilities operate on Chapter-scoped resources while retaining ownership of their own execution state.

---

# Domain Role

The Chapter normally sits below Project or Book.

With Book:

```text
Project
└── Book
    └── Chapter
```

Without Book:

```text
Project
└── Chapter
```

The Chapter defines:

* logical reading identity,
* parent scope,
* ordering,
* chapter metadata,
* content-unit membership,
* navigation context,
* and chapter-level preferences.

Processing modules consume Chapter-scoped content but do not become part of the Chapter aggregate.

---

# Parent Relationship

Every Chapter belongs to exactly one Project.

A Chapter MAY additionally belong to one Book.

Typical form:

```text
Chapter
├── projectId
└── bookId?
```

`projectId` is required.

`bookId` is optional.

When `bookId` is present:

* the Book MUST belong to the same Project,
* the Chapter participates in Book-level ordering,
* and Book-level defaults MAY apply.

A Chapter MUST NOT reference a Book from another Project.

---

# Responsibilities

A Chapter is responsible for:

* Chapter identity
* Project association
* Optional Book association
* Chapter metadata
* Logical reading order
* Content-unit membership
* Content-unit ordering
* Source references
* Chapter lifecycle
* Chapter-level preferences
* Navigation metadata
* Chapter-level discovery information

A Chapter MAY expose derived processing summaries.

A Chapter MUST NOT own authoritative state for:

* OCR execution
* recognition execution
* translation execution
* review execution
* rendering execution
* presentation execution
* reading-session execution
* provider execution state

---

# Identity

Every Chapter has an immutable identifier.

Typical fields include:

```text
Chapter
├── chapterId
├── projectId
├── bookId?
├── title
├── originalTitle
├── chapterNumber?
├── sequence
├── sourceReference?
├── lifecycleStatus
├── publicationStatus?
├── createdAt
├── updatedAt
└── version
```

`chapterId` MUST remain immutable.

`projectId` MUST remain stable under normal updates.

Moving a Chapter between Projects MUST require an explicit migration workflow.

Moving a Chapter between Books within the same Project MAY be supported as an explicit structural operation.

---

# Chapter Number vs Sequence

`chapterNumber` and `sequence` are different concepts.

Example:

```text
chapterNumber: "12.5"
sequence: 18
```

or:

```text
chapterNumber: "Extra"
sequence: 19
```

`chapterNumber` is publication metadata intended for display.

`sequence` is the canonical ordering value used for navigation.

The implementation MUST NOT rely on display labels as the only ordering mechanism.

---

# Ordering

Chapter ordering MUST be deterministic within its parent scope.

When the Chapter belongs to a Book:

```text
Book
├── Chapter sequence 10
├── Chapter sequence 20
└── Chapter sequence 30
```

When no Book exists:

```text
Project
├── Chapter sequence 10
├── Chapter sequence 20
└── Chapter sequence 30
```

The canonical parent ordering scope is therefore:

```text
bookId if present
otherwise projectId
```

Duplicate canonical ordering positions MUST be prevented or resolved deterministically.

---

# Content Model

A Chapter groups ordered content units.

The actual content representation depends on the work.

Image-oriented content:

```text
Chapter
└── Page
    └── Image
```

Text-oriented content:

```text
Chapter
└── Text Content
```

Mixed content:

```text
Chapter
├── Page
│   └── Image
└── Text Content
```

The Chapter domain MUST NOT assume that every Chapter contains Pages.

This is required to support novels and other primarily textual content without introducing artificial Page entities.

---

# Content Membership

The Chapter owns the logical relationship between itself and its content units.

Conceptually:

```text
Chapter
    owns:
        content membership
        content ordering

Content Unit
    owns:
        content-specific state
```

For page-oriented content:

```text
Chapter
    page membership
    page ordering

Page
    page identity
    page metadata
    page source references
```

A Chapter metadata update MUST NOT require loading every Page or content resource.

---

# Page Relationship

A Chapter MAY contain ordered Pages.

```text
Chapter
├── Page 1
├── Page 2
└── Page 3
```

A Page SHOULD normally reference its `chapterId`.

Page ordering MUST be deterministic within the Chapter.

The Chapter SHOULD NOT embed complete Page aggregate state merely to represent membership.

---

# Text Content Relationship

Text-first Chapters MAY contain textual content without page boundaries.

Possible models MAY include:

```text
Chapter
└── Text Document
```

or:

```text
Chapter
└── Content Blocks
```

The final representation is defined by the responsible content domain.

Chapter MUST provide enough scope for ordered reading without forcing textual content into image-oriented structures.

---

# Source Reference

A Chapter MAY preserve information about its source.

Possible values include:

* source URL
* remote chapter identifier
* import source identifier
* acquisition timestamp
* source revision
* external publication identifier

Source references identify where Chapter content came from.

They MUST NOT contain scraper/provider runtime state that belongs to acquisition infrastructure.

---

# Metadata

Chapter metadata MAY include:

* title
* original title
* chapter number
* alternative label
* publication date
* source language override
* description
* special/extra indicator
* source reference
* publication status

Metadata SHOULD describe the Chapter as a reading unit.

Metadata MUST NOT be used to store processing execution status.

---

# Lifecycle

Chapter lifecycle describes availability of the logical Chapter inside CRAI.

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

Optional deletion states MAY include:

```text
DELETING
DELETED
```

A Chapter becoming Active does not imply that OCR or translation is complete.

Likewise, a Chapter can remain Active while any processing capability is pending, running, failed, or complete.

---

# Publication Status

Source publication state MAY be represented separately.

Possible statuses:

```text
UNKNOWN
PUBLISHED
DRAFT
WITHDRAWN
```

This status reflects the source/publication context.

It is separate from CRAI lifecycle state.

---

# Import State

Import is not a Chapter lifecycle state.

Example:

```text
Chapter: ACTIVE

Import Job
    status: RUNNING
```

or:

```text
Chapter: ACTIVE

Import Job
    status: FAILED
```

A failed import operation MUST NOT automatically change the Chapter to a failed domain state.

Import state belongs to the acquisition/import workflow.

---

# Processing Scope

A Chapter is a common processing scope, but not a processing aggregate.

Processing modules MAY accept a `chapterId` as input.

Example:

```text
chapterId
   |
   +--> OCR
   |
   +--> Recognition
   |
   +--> Translation
   |
   +--> Presentation
   |
   +--> Review
```

Each capability owns its execution state.

The Chapter provides identity and scope only.

---

# OCR

OCR execution MUST remain outside the Chapter aggregate.

Conceptually:

```text
Chapter
   |
   v
Pages / Images
   |
   v
OCR Pipeline
   |
   v
Recognition Results
```

The Chapter MAY expose derived OCR coverage.

It MUST NOT own authoritative OCR execution history.

---

# Text Blocks

Text Blocks MAY be associated with Chapter-scoped content.

Typical relation:

```text
Chapter
└── Page
    └── Text Blocks
```

or for text-native content:

```text
Chapter
└── Text Blocks
```

Text Block identity, geometry, recognition state, and translation linkage belong to the appropriate content/recognition domain.

The Chapter only provides higher-level scope.

---

# Translation

Translation execution is owned by the Translation capability.

Typical relationship:

```text
Chapter
   |
   v
Source Content
   |
   v
Translation Module
   |
   v
Translation Records
```

The Chapter MAY expose:

* translation coverage,
* translated content count,
* latest translation activity.

These MUST be derived from translation-owned state.

The Chapter MUST NOT independently store an authoritative `translationCompleted` state.

---

# Rendering and Presentation

Rendering and presentation are not Chapter-owned execution state.

Conceptually:

```text
Chapter Content
      |
      v
Presentation Module
      |
      v
Presentation Output
```

The Chapter MAY provide preferences or source context consumed by Presentation.

Rendered assets and presentation execution state remain owned by their corresponding domains/modules.

---

# Review

Review state belongs to the workflow or domain being reviewed.

Examples:

```text
Translation Review
OCR Review
Presentation Review
```

A generic Chapter-level `REVIEWING` lifecycle state SHOULD NOT be used to represent all of these independent processes.

The Chapter MAY expose a derived review summary if required by the UI.

---

# Processing Progress

Processing progress is derived information.

Possible summary:

```text
ChapterProcessingSummary
├── ocrCoverage
├── recognitionCoverage
├── translationCoverage
├── reviewCoverage
├── presentationCoverage
├── issueCount
└── lastProcessingAt
```

The Chapter MAY cache this summary.

The authoritative values remain in the corresponding module states.

Cached progress MUST be reconstructable.

---

# Reading State

Reading progress MUST NOT be stored as universal Chapter metadata.

Reading state may depend on:

* user
* profile
* device
* reading session
* Project
* Book
* Chapter

Example:

```text
Chapter
    |
    v
Reading State
    |
    ├── openedAt
    ├── progress
    └── completedAt
```

A Chapter itself does not become `READ` or `UNREAD`.

That state belongs to the reader context.

---

# Configuration

Chapter-level configuration MAY override Project or Book defaults.

Possible preferences include:

```text
ChapterPreferences
├── sourceLanguageOverride
├── readingDirectionOverride
├── translationPreference
├── presentationPreference
└── processingHints
```

Preferences express policy or intent.

They MUST NOT contain authoritative runtime execution state.

---

# Configuration Precedence

Recommended precedence:

```text
Project defaults
       |
       v
Book overrides
       |
       v
Chapter overrides
       |
       v
Content-level override
```

The most specific valid configuration wins.

A missing override MUST fall back to its parent scope.

Modules remain responsible for validating whether the effective configuration is supported.

---

# Language

A Chapter normally inherits source language from:

```text
Project
   |
   v
Book
   |
   v
Chapter
```

A Chapter MAY override inherited language metadata.

Content-level language detection MAY identify a different language.

Detected language MUST NOT automatically mutate Chapter configuration unless explicitly accepted.

---

# Assets

A Chapter MAY reference Chapter-level source assets.

Examples:

* source archives
* chapter thumbnails
* imported documents
* chapter-specific images
* auxiliary metadata

The Chapter stores stable references.

Asset binary storage and lifecycle remain owned by the Asset/Storage domain.

---

# Navigation

Chapter provides a logical navigation unit.

Typical operations include:

```text
previousChapter(chapterId)
nextChapter(chapterId)
```

Navigation SHOULD use canonical Chapter ordering.

Navigation MUST NOT depend solely on:

* creation time
* import time
* source URL order

unless explicitly defined as canonical.

---

# Aggregate Boundary

Recommended Chapter aggregate scope:

```text
Chapter Aggregate

owns
    Chapter identity
    Project association
    optional Book association
    Chapter metadata
    Chapter lifecycle
    content membership references
    content ordering
    Chapter-level preferences
```

It does NOT transactionally contain:

```text
Pages
Images
OCR Results
Text Blocks
Translation Records
Translation Sessions
Rendered Assets
Reading Sessions
Processing Jobs
```

Those resources may be Chapter-scoped while retaining independent ownership.

---

# Transactional Consistency

Operations should modify only the aggregate that owns the state.

Examples:

```text
Rename Chapter
    -> Chapter transaction
```

```text
Change Chapter sequence
    -> Chapter / parent ordering transaction
```

```text
Add Page membership
    -> Chapter content-membership operation
```

But:

```text
Recognize image text
    -> OCR / Recognition workflow
```

```text
Translate text
    -> Translation workflow
```

```text
Generate reading presentation
    -> Presentation workflow
```

```text
Update reader progress
    -> Reading workflow
```

These MUST NOT require a Chapter-wide transaction.

---

# Processing History

Chapter MAY provide a query scope for processing history.

Example:

```text
getProcessingHistory(chapterId)
```

The history itself is composed from module-owned events, jobs, or records.

Chapter MUST NOT duplicate the complete processing history as authoritative embedded state.

---

# Events

Typical Chapter domain events include:

```text
ChapterCreated
ChapterMetadataUpdated
ChapterActivated
ChapterArchived
ChapterRestored
ChapterDeleted
ChapterParentChanged
ChapterOrderChanged
ChapterContentAdded
ChapterContentRemoved
ChapterContentOrderChanged
ChapterConfigurationChanged
```

Processing events belong to their respective domains.

Examples:

```text
OCRCompleted
RecognitionCompleted
TranslationStarted
TranslationCompleted
PresentationGenerated
ReviewCompleted
ReadingSessionStarted
```

These are not Chapter domain events merely because they carry a `chapterId`.

---

# Invariants

1. `chapterId` is immutable.

2. Every Chapter belongs to exactly one Project.

3. `bookId` is optional.

4. When `bookId` exists, the Book MUST belong to the same Project.

5. Chapter ordering MUST be deterministic within its canonical parent scope.

6. Chapter content membership MUST NOT contain duplicate content identities.

7. Content ordering MUST be deterministic.

8. Chapter metadata MUST NOT contain authoritative OCR, translation, presentation, review, or reading execution state.

9. Chapter lifecycle MUST remain independent from processing lifecycle.

10. Processing failure MUST NOT automatically invalidate Chapter domain state.

11. Derived processing summaries MUST NOT become the sole authoritative state.

12. Archived Chapters MUST reject normal mutation unless explicitly permitted.

13. Ordinary Chapter metadata operations MUST NOT require loading all associated content.

14. Cross-aggregate operations MUST preserve ownership boundaries.

---

# Failure Isolation

Processing failures SHOULD remain isolated from Chapter domain state.

Examples:

```text
Page import failed
    != Chapter failed

OCR failed
    != Chapter failed

Translation failed
    != Chapter failed

Presentation failed
    != Chapter failed

Reading session failed
    != Chapter failed
```

The Chapter MAY surface summarized issues for navigation or UI purposes.

Detailed failure state remains owned by the responsible module.

---

# Scalability

A Chapter MAY contain large numbers of content resources.

The model MUST NOT require loading the complete Chapter graph for normal operations.

Preferred access patterns include:

```text
getChapter(chapterId)

listPages(
    chapterId,
    cursor,
    limit
)

listContent(
    chapterId,
    cursor,
    limit
)

getProcessingSummary(chapterId)
```

Chapter retrieval SHOULD return Chapter-owned state and references rather than embedding every related processing artifact.

---

# Example: Image-Based Chapter

```text
Chapter
  chapterId: chapter_001
  projectId: project_001
  bookId: book_001

  title: "Chapter 1"
  chapterNumber: "1"
  sequence: 10

  lifecycleStatus: ACTIVE

  preferences:
    readingDirectionOverride: TOP_TO_BOTTOM
```

Content:

```text
chapter_001
├── page_001
├── page_002
└── page_003
```

OCR and translations are queried independently using the Chapter or content identities.

---

# Example: Text-Based Chapter

```text
Chapter
  chapterId: chapter_100
  projectId: project_002
  bookId: null

  title: "Episode 100"
  chapterNumber: "100"
  sequence: 1000

  lifecycleStatus: ACTIVE
```

Content:

```text
chapter_100
└── text_document_100
```

No Page entity is required merely to satisfy hierarchy.

---

# Ownership Summary

```text
Chapter Domain

owns
    Chapter identity
    Project association
    optional Book association
    Chapter metadata
    Chapter lifecycle
    Chapter ordering
    content membership
    content ordering
    Chapter-level preferences
    source references

references
    Project
    optional Book
    Pages
    Text Content
    Assets

derives
    OCR progress
    recognition progress
    translation progress
    review progress
    presentation progress
    processing statistics

does not own
    Page internal state
    Image state
    OCR execution
    Recognition execution
    Text Block execution state
    Translation execution
    Presentation execution
    Review execution
    Reading-session state
    Processing jobs
    Asset binary storage
```

Chapter is therefore a logical reading and content scope, not a monolithic processing aggregate.

---

# Related Documents

Domain:

* `README.md`
* `PROJECT.md`
* `BOOK.md`
* `PAGE.md`
* `IMAGE.md`
* `TEXT_BLOCK.md`
* `TRANSLATION.md`
* `LANGUAGE.md`
* `GLOSSARY.md`
* `CHARACTER.md`
* `SESSION.md`

Architecture:

* `docs/architecture/CAPABILITY_MAP.md`
* `docs/architecture/OWNERSHIP_MAP.md`
* `docs/architecture/STATE_MACHINE.md`
* `docs/architecture/EVENT_BUS.md`
* `docs/architecture/MODULE_DEPENDENCY.md`
* `docs/architecture/DATA_FLOW.md`

Module contracts remain authoritative for module-specific execution ownership and runtime behavior.
