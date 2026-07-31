# Book Domain

- **Document:** Domain / Book
- **Version:** 1.0.0
- **Status:** Draft
- **Owner:** CRAI Architecture

---

# Purpose

A Book represents a complete readable work within a Project.

Depending on the project type, a Book may represent an entire manga, comic, novel, manhwa, manhua, or another publication. It organizes chapters, metadata, reading order and publication information.

A Book belongs to exactly one Project and serves as the root aggregate for all chapter-related content.

---

# Responsibilities

A Book is responsible for:

- Managing book metadata
- Organizing chapters
- Defining reading order
- Maintaining publication information
- Tracking translation progress
- Managing book-level assets
- Providing navigation structure

---

# Aggregate Structure

```text
Book
├── Chapters
├── Cover
├── Metadata
├── Assets
├── Statistics
└── Settings
```

---

# Identity

Every Book has a stable identity.

Typical fields:

- Book ID
- Project ID
- Title
- Original Title
- Author
- Artist
- Publisher
- Country
- Source URL
- Created Time
- Updated Time
- Version

Book ID never changes.

---

# Book Types

Supported types include:

- Manga
- Manhwa
- Manhua
- Comic
- Novel
- Light Novel
- Web Novel

Type influences rendering and translation workflows.

---

# Reading Direction

Supported reading modes:

- Left to Right
- Right to Left
- Vertical
- Top to Bottom

Reading direction affects navigation and rendering only.

---

# Lifecycle

```text
Created
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
Completed
   │
   ▼
Archived
```

---

# Metadata

Book metadata may include:

- Genres
- Tags
- Description
- Language
- Release Date
- Cover Image
- Status
- Rating

Metadata is optional but versioned.

---

# Relationships

A Book owns:

- Chapters
- Cover assets
- Book settings
- Book statistics

A Book belongs to one Project only.

---

# Progress

Derived metrics include:

- Chapter count
- Imported chapters
- Translated chapters
- Reviewed chapters
- Completion percentage
- Last read chapter

Progress is computed from child chapters.

---

# Events

Typical domain events:

- BookCreated
- BookImported
- BookUpdated
- BookArchived
- BookDeleted
- ReadingDirectionChanged

---

# Invariants

1. Every Book belongs to exactly one Project.
2. Book ID is immutable.
3. Chapter order is unique within a Book.
4. Reading direction is consistent across the Book unless explicitly overridden.
5. Book metadata is versioned.
6. Chapters cannot exist without a Book.
7. Book statistics are derived, not manually edited.

---

# Related Documents

- README.md
- PROJECT.md
- CHAPTER.md
- PAGE.md
- IMAGE.md
- TEXT_BLOCK.md
- TRANSLATION.md
- CHARACTER.md
- GLOSSARY.md
- SESSION.md
