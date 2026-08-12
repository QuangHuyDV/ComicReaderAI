# Book Domain

* **Document:** Domain / Book
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

A `Book` represents an ordered publication unit within a CRAI Project.

Depending on the source material, a Book may represent:

* a complete manga volume,
* a comic volume,
* a novel,
* a light novel volume,
* a manhua or manhwa volume,
* a web novel grouping,
* or another logical publication unit.

A Book primarily provides:

* publication identity,
* metadata,
* content grouping,
* chapter ordering,
* navigation context,
* and book-level presentation defaults.

A Book belongs to exactly one Project.

A Book is **not** required to be the transactional aggregate root for every Chapter, Page, translation record, asset, or reading state associated with it.

---

# Domain Role

The Book domain sits between Project and Chapter in the normal content hierarchy.

```text
Project
   |
   v
Book
   |
   v
Chapter
   |
   v
Content Units
```

Typical content hierarchy:

```text
Project
└── Book
    ├── Chapter
    ├── Chapter
    └── Chapter
```

The Book defines:

* which Chapters belong to the publication unit,
* their logical order,
* navigation context,
* publication metadata,
* and book-level defaults.

Individual Chapters remain independently addressable domain resources.

---

# Optionality

Not every CRAI Project must use a Book level.

Some works may naturally use:

```text
Project
└── Chapter
```

instead of:

```text
Project
└── Book
    └── Chapter
```

Examples may include:

* continuously published web novels,
* web comics without explicit volumes,
* imported chapter collections,
* temporary reading projects.

Therefore, the domain model MUST NOT require Book as a universal parent for every Chapter unless the Project's content structure requires it.

A Chapter MAY reference a `bookId` when grouped under a Book.

---

# Responsibilities

A Book is responsible for:

* Book identity
* Project association
* Book metadata
* Publication metadata
* Chapter membership
* Chapter ordering
* Book-level navigation structure
* Default reading direction
* Book-level content preferences
* Book lifecycle
* Book-level asset references
* Book-level discovery information

A Book MAY expose derived statistics and progress information.

A Book MUST NOT own authoritative state for:

* OCR execution
* recognition execution
* translation execution
* review execution
* rendering execution
* presentation execution
* reading-session execution

---

# Identity

Every Book has a stable identifier.

Typical fields include:

```text
Book
├── bookId
├── projectId
├── title
├── originalTitle
├── description
├── bookType
├── volumeNumber
├── publicationStatus
├── readingDirection
├── createdAt
├── updatedAt
└── version
```

`bookId` MUST remain immutable.

`projectId` identifies the Project scope to which the Book belongs.

A Book MUST NOT move between Projects through a normal metadata update.

Cross-project migration, if ever supported, MUST be treated as an explicit migration workflow.

---

# Publication Metadata

Book metadata MAY include:

* title
* original title
* alternative titles
* author
* artist
* publisher
* publication country
* source language
* release date
* description
* genres
* tags
* cover asset reference
* source reference
* publication status
* volume number

Metadata fields are optional unless required by a specific workflow.

Metadata SHOULD describe the publication itself rather than runtime processing state.

---

# Book Type

Book type describes the dominant publication form.

Possible values MAY include:

```text
MANGA
MANHWA
MANHUA
COMIC
NOVEL
LIGHT_NOVEL
WEB_NOVEL
MIXED
OTHER
```

Book type MAY influence defaults such as:

* reading direction,
* presentation behavior,
* chapter navigation,
* text layout expectations.

Book type MUST NOT directly determine OCR, translation, or rendering implementations.

Those behaviors are selected by the responsible capability or module.

---

# Project Relationship

A Book belongs to exactly one Project.

```text
Project
   |
   +-- Book A
   |
   +-- Book B
```

The Project provides the broader domain scope.

The Book provides publication-level organization inside that scope.

The relationship does not imply that the Project and Book must be updated in the same transaction.

---

# Chapter Relationship

A Book MAY contain an ordered set of Chapters.

Conceptually:

```text
Book
├── Chapter 1
├── Chapter 2
├── Chapter 3
└── ...
```

The Book owns the **membership and ordering relationship**.

The Chapter owns its own Chapter state.

This distinction is important.

```text
Book
    owns:
        chapter membership
        chapter ordering

Chapter
    owns:
        chapter metadata
        chapter lifecycle
        chapter content references
```

Updating Chapter content MUST NOT require loading the entire Book.

---

# Chapter Ordering

Chapter order MUST be deterministic within a Book.

Possible ordering representations include:

```text
position
sequence
chapterNumber
sortKey
```

The canonical ordering mechanism MUST be defined consistently by the implementation.

Chapter display labels MAY differ from the internal ordering value.

Example:

```text
sequence: 15
displayLabel: "Extra Chapter"
```

Ordering MUST support insertion without requiring unsafe rewriting of unrelated content where practical.

---

# Reading Direction

A Book MAY define a default reading direction.

Possible values include:

```text
LEFT_TO_RIGHT
RIGHT_TO_LEFT
TOP_TO_BOTTOM
VERTICAL
AUTO
```

Reading direction is primarily a presentation and navigation preference.

The Book stores the default user/domain intent.

The Presentation and Reading capabilities interpret that preference.

A lower-level content unit MAY override the default when necessary.

Example:

```text
Project default
    |
    v
Book default
    |
    v
Chapter override
    |
    v
Page/content override
```

The most specific valid configuration takes precedence.

---

# Content Structure

A Book SHOULD NOT assume all content is page-based.

Image-oriented Book:

```text
Book
└── Chapter
    └── Page
        └── Image
```

Text-oriented Book:

```text
Book
└── Chapter
    └── Text Content
```

Mixed Book:

```text
Book
└── Chapter
    ├── Text Content
    └── Image Content
```

This allows CRAI to support novels and comics without forcing both through the same physical content representation.

---

# Assets

A Book MAY reference book-scoped assets such as:

* cover image
* thumbnail
* publication artwork
* imported metadata files
* auxiliary publication assets

The Book stores asset references.

Asset binary lifecycle and storage remain owned by the appropriate asset/storage domain or infrastructure component.

Example:

```text
Book
    coverAssetId
        |
        v
Asset
```

A Book MUST NOT embed large binary assets directly into the domain entity.

---

# Configuration

Book-level configuration MAY provide overrides to Project defaults.

Examples:

```text
BookPreferences
├── readingDirection
├── sourceLanguageOverride
├── presentationPreference
└── translationPreference
```

Book configuration describes user intent or policy.

Runtime processing state MUST remain in the responsible module.

Example:

```text
Book translation preference
          |
          v
Translation Module
          |
          v
Translation Job
```

The Book MUST NOT store provider execution details merely because translation is performed for Chapters belonging to it.

---

# Language

A Book MAY inherit source language from its Project.

```text
Project sourceLanguage
        |
        v
Book
```

A Book MAY override the Project language when the publication differs.

Chapter or content-level language metadata MAY provide further overrides.

Detected language MUST NOT automatically mutate Book metadata unless explicitly accepted by the relevant workflow.

---

# Lifecycle

Book lifecycle represents availability and publication-level domain status.

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

Optional state:

```text
Created
   |
   v
Active
   |
   v
Completed
   |
   v
Archived
```

`Completed` MAY be used when the publication itself is known to be complete.

It MUST NOT mean that OCR, translation, review, or reading has completed.

---

# Publication Status

Publication status and Book lifecycle SHOULD remain conceptually separate.

Possible publication statuses:

```text
UNKNOWN
ONGOING
COMPLETED
HIATUS
CANCELLED
```

Example:

```text
Book lifecycle:
    ACTIVE

Publication status:
    ONGOING
```

The Book may remain active in CRAI even when the original publication is complete.

Likewise, an ongoing publication may be archived locally by the user.

---

# Import

Importing content is an operation, not normally a Book lifecycle state.

An Active Book MAY receive additional Chapters at any time.

Example:

```text
Book: ACTIVE

Import Job A
    completed

Import Job B
    running

Import Job C
    failed
```

Import execution state belongs to the responsible import/acquisition workflow.

A failed import MUST NOT automatically invalidate the Book.

---

# Progress

Book progress is derived information.

Possible metrics include:

```text
Content
├── chapterCount
├── importedChapterCount
└── availableContentCount

Processing
├── OCRCoverage
├── translationCoverage
├── reviewCoverage
└── presentationCoverage

Reading
├── chaptersRead
├── currentChapter
└── lastActivity
```

These values SHOULD be derived from authoritative module state.

The Book MAY cache summarized progress for efficient display.

Cached progress MUST NOT become the authoritative state of the underlying workflow.

---

# Reading Progress

Reading progress does not belong directly to the Book aggregate.

Typical relationship:

```text
Book
   |
   +----> Reading State
             |
             +-- currentChapter
             +-- progress
             +-- lastReadAt
```

Reading state may depend on:

* user
* profile
* device
* project
* book
* reading session

Therefore `lastReadChapter` MUST NOT be treated as universal Book metadata.

---

# Translation Progress

Translation progress is derived from Translation-owned state.

```text
Translation Records
       |
       v
Book Translation Summary
```

The Book MAY expose:

* translated Chapter count
* translation coverage
* latest translation activity

The Book MUST NOT independently maintain authoritative translation completion flags that can diverge from translation records.

---

# Aggregate Boundaries

Book is not a container aggregate that transactionally owns every Chapter.

Recommended model:

```text
Book Aggregate
    Book identity
    Book metadata
    Book lifecycle
    Chapter membership/order references
    Book preferences

Chapter Aggregate
    Chapter identity
    Chapter metadata
    Chapter lifecycle
    Content references
```

Other domain aggregates may include:

```text
Asset
Translation
Reading State
Glossary
Character Registry
Processing Job
```

Cross-aggregate operations SHOULD use application orchestration and domain events where appropriate.

---

# Transactional Consistency

Book-level operations SHOULD only require Book-owned state.

Example:

```text
Rename Book
    -> Book transaction
```

```text
Change Reading Direction
    -> Book transaction
```

```text
Reorder Chapters
    -> Book ordering transaction
```

But:

```text
Translate Chapter
    -> Translation workflow
```

```text
Read Chapter
    -> Reading workflow
```

```text
Run OCR
    -> OCR / Recognition workflow
```

These MUST NOT require a Book-wide transaction.

---

# Navigation

A Book provides a navigation scope for ordered Chapters.

Typical operations include:

```text
firstChapter(bookId)
previousChapter(chapterId)
nextChapter(chapterId)
lastChapter(bookId)
```

Navigation SHOULD rely on explicit ordering data.

Navigation MUST NOT depend on creation timestamps unless timestamps are explicitly the canonical order.

---

# Statistics

Book statistics MAY include:

* Chapter count
* available content count
* translated Chapter count
* reviewed Chapter count
* reading completion
* processing coverage
* last activity

Statistics are derived values.

They MAY be cached or materialized.

Statistics MUST NOT be manually edited as authoritative domain state.

---

# Archive Behavior

Archiving a Book SHOULD:

* prevent normal metadata mutation,
* prevent normal Chapter membership changes,
* preserve Chapter relationships,
* preserve translation state,
* preserve reading history,
* preserve referenced assets according to retention policy.

Archiving a Book MUST NOT automatically archive the entire Project.

Archiving a Project MAY make all Books effectively read-only through Project-level policy.

---

# Deletion

Book deletion SHOULD be distinct from archive.

Deletion MAY require checking:

* existing Chapters,
* active processing jobs,
* reading state,
* derived assets,
* external references,
* retention policy.

Deleting a Book MUST NOT leave invalid Chapter membership references.

Large cleanup operations MAY be executed asynchronously.

---

# Events

Typical Book domain events include:

```text
BookCreated
BookMetadataUpdated
BookActivated
BookArchived
BookRestored
BookDeletionRequested
BookDeleted
BookReadingDirectionChanged
BookChapterAdded
BookChapterRemoved
BookChapterOrderChanged
```

Events SHOULD describe changes to Book-owned state.

The following are NOT Book domain events merely because they concern content inside the Book:

```text
ChapterOCRCompleted
ChapterTranslated
ReadingSessionStarted
PageRendered
```

Those events belong to the responsible domain or module.

---

# Invariants

1. `bookId` is immutable.

2. A Book belongs to exactly one Project.

3. A Book MUST reference an existing Project.

4. A Book's Chapter membership MUST NOT contain duplicate Chapter identities.

5. Chapter order within a Book MUST be deterministic.

6. Book metadata MUST NOT contain authoritative runtime processing state.

7. Book statistics and progress MUST be derived from authoritative underlying state.

8. Book lifecycle MUST remain independent from import, OCR, translation, and reading job lifecycles.

9. A Book MUST NOT require all Chapters to be loaded to perform ordinary Book metadata operations.

10. Reading direction overrides MUST follow deterministic configuration precedence.

11. Archived Books MUST reject normal mutation unless explicitly permitted.

12. Deleting a Book MUST not leave invalid Book-to-Chapter relationships.

---

# Failure Isolation

Failures in Chapter-level or processing operations MUST NOT corrupt the Book.

Examples:

```text
Chapter import failure
    != Book failure

OCR failure
    != Book failure

Translation failure
    != Book failure

Presentation failure
    != Book failure

Reading-session failure
    != Book failure
```

The Book MAY expose summarized issue counts without owning detailed failure state.

---

# Scalability

The Book model MUST support works containing large numbers of Chapters.

Operations SHOULD avoid loading all Chapter data when unnecessary.

Typical API behavior SHOULD use:

* identifiers,
* pagination,
* ordered queries,
* cursors,
* summary projections.

Example:

```text
getBook(bookId)

listChapters(
    bookId,
    cursor,
    limit
)
```

Book metadata retrieval MUST NOT require loading every Chapter.

---

# Example

```text
Book
  bookId: book_001
  projectId: project_001

  title: "Example Manhua"
  originalTitle: "示例漫画"

  bookType: MANHUA

  readingDirection: TOP_TO_BOTTOM

  lifecycleStatus: ACTIVE
  publicationStatus: ONGOING

  coverAssetId: asset_cover_001

  preferences:
    presentationProfileId: presentation_profile_001
```

Chapter resources are queried independently:

```text
Book: book_001

Chapters:
    chapter_001
    chapter_002
    chapter_003
```

The Book does not embed the complete Chapter aggregate state.

---

# Ownership Summary

```text
Book Domain

owns
    Book identity
    Project association
    Book metadata
    Publication metadata
    Book lifecycle
    Chapter membership
    Chapter ordering
    Book-level preferences
    Book-level asset references

references
    Project
    Chapters
    Assets
    presentation preferences
    translation preferences

derives
    processing progress
    translation progress
    reading progress
    statistics

does not own
    Chapter internal state
    Page state
    OCR execution
    Translation execution
    Presentation execution
    Reading-session execution
    Asset binary storage
```

The Book therefore acts as a publication and navigation boundary rather than a God Aggregate.

---

# Related Documents

Domain:

* `README.md`
* `PROJECT.md`
* `CHAPTER.md`
* `PAGE.md`
* `IMAGE.md`
* `TEXT_BLOCK.md`
* `TRANSLATION.md`
* `CHARACTER.md`
* `GLOSSARY.md`
* `SESSION.md`

Architecture:

* `docs/architecture/CAPABILITY_MAP.md`
* `docs/architecture/OWNERSHIP_MAP.md`
* `docs/architecture/STATE_MACHINE.md`
* `docs/architecture/EVENT_BUS.md`
* `docs/architecture/MODULE_DEPENDENCY.md`
* `docs/architecture/DATA_FLOW.md`

Module contracts remain authoritative for module-specific runtime ownership and behavior.
