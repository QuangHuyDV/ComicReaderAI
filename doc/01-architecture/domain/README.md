# CRAI Domain Model

* **Document:** Domain Overview
* **Version:** 1.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

This directory defines the **business domain model** of CRAI.

The purpose of the Domain layer is to describe **what the system knows**, **what the business rules are**, and **how business concepts relate to one another**, independently of:

* UI
* Database
* AI provider
* OCR engine
* Runtime implementation
* Framework
* Infrastructure
* Programming language

The Domain Model should remain valid even if every external technology changes.

---

# Domain Principles

The CRAI Domain follows several core principles.

## Provider Neutral

Business concepts never depend on:

* OpenAI
* Gemini
* Claude
* Qwen
* OCRSpace
* PaddleOCR

Providers are infrastructure.

The Domain only expresses intent.

---

## Immutable Business Truth

Historical business artifacts are immutable.

Examples:

* Translation Revision
* Character Snapshot
* Glossary Snapshot
* Profile Revision
* Session Context Snapshot

Corrections create new revisions.

They never silently modify history.

---

## Stable Identity

Every important business object has a stable identity.

Example:

```text
Character ID
```

is different from

```text
Character Revision ID
```

Likewise:

```text
Profile ID
≠
Profile Revision ID

Translation ID
≠
Translation Revision ID
```

---

## Explicit Context

Business behavior depends on explicit context.

Instead of reading mutable state directly:

```text
Session
```

operations receive:

```text
Context Snapshot
```

This guarantees reproducibility.

---

## Separation of Truth and Execution

The Domain defines:

```text
What should happen
```

Infrastructure defines:

```text
How it happens
```

Example:

```text
Translation Profile
```

does not know:

* prompt
* HTTP request
* API key
* provider model

Those belong elsewhere.

---

# High-Level Domain Hierarchy

The domain is organized around several major business areas.

```text
Workspace
    │
    ▼
Project
    │
    ▼
Book
    │
    ▼
Chapter
    │
    ▼
Page
    │
    ▼
Image
    │
    ▼
TextBlock
```

Processing flows through:

```text
Language
        │
        ▼
Translation
        │
        ▼
Presentation
```

Context is provided by:

```text
Character
Glossary
Profile
Session
```

---

# Domain Dependency

The intended dependency direction is:

```text
Workspace
        │
Project
        │
Book
        │
Chapter
        │
Page
        │
Image
        │
TextBlock
        │
Translation
```

Supporting domains:

```text
Language
Character
Glossary
Profile
Session
```

These domains provide context.

They do not own the content hierarchy.

---

# Core Content Model

The fundamental content hierarchy is:

```text
Workspace
    │
Project
    │
Book
    │
Chapter
    │
Page
    │
Image
    │
TextBlock
```

Each layer owns its own identity.

Each layer may evolve independently.

---

# Processing Model

Business processing follows the conceptual pipeline:

```text
TextBlock
        │
Language Resolution
        │
Context Resolution
        │
Profile Resolution
        │
Translation
        │
Validation
        │
Presentation
```

Execution details are intentionally excluded.

---

# Context Model

Translation never depends only on source text.

Context comes from multiple domains.

```text
TextBlock
+
Language
+
Character Snapshot
+
Glossary Snapshot
+
Profile Snapshot
+
Session Snapshot
```

↓

```text
Operation Context Snapshot
```

↓

```text
Translation Revision
```

---

# Business Aggregates

The primary Aggregate Roots are expected to be:

```text
Workspace
Project
Book
Chapter
Page
Image
TextBlock
Translation
Glossary
Character
Profile
Session
```

Each Aggregate owns its own consistency boundary.

Relationships between Aggregates are references, not ownership.

---

# Immutable Snapshot Pattern

Many domains expose mutable working state.

Durable operations never depend directly on mutable state.

Instead:

```text
Mutable Domain Object
        │
        ▼
Immutable Snapshot
        │
        ▼
Historical Artifact
```

Examples:

```text
Glossary
        ↓
Glossary Snapshot

Character
        ↓
Character Context Snapshot

Profile
        ↓
Resolved Profile Snapshot

Session
        ↓
Operation Context Snapshot
```

---

# Ownership Hierarchy

Ownership is intentionally separated.

```text
Workspace
```

owns collaboration.

```text
Project
```

owns content.

```text
Session
```

owns temporary working state.

```text
Translation
```

owns historical translated results.

---

# Workspace

Workspace defines:

* ownership
* collaboration
* permissions
* policies
* quotas
* shared resources

Workspace is **not** a Project.

---

# Project

Project defines:

* one translation project
* one reading project
* one publication project

Project owns:

* Books
* Project Glossary
* Project Profiles

Project is the primary business boundary.

---

# Book

Book groups related Chapters.

Book may represent:

* Novel
* Comic
* Manga
* Webtoon
* Document

Book owns ordering.

---

# Chapter

Chapter groups Pages.

Chapter defines reading sequence.

---

# Page

Page defines logical reading units.

One Page may contain:

* one Image
* several Images
* rendered content

---

# Image

Image is the visual source.

OCR consumes Images.

Translation never directly owns Images.

---

# TextBlock

TextBlock is the canonical source text unit.

Translation always targets TextBlocks.

OCR produces TextBlocks.

Manual editing also produces TextBlocks.

---

# Language

Language describes language identity.

Language is not Translation.

Language provides:

* source language
* target language
* script
* locale

---

# Translation

Translation stores historical translated content.

Translation owns immutable revisions.

Translation never stores mutable configuration.

---

# Glossary

Glossary owns terminology truth.

Translation consumes:

```text
Glossary Snapshot
```

never mutable Glossary state.

---

# Character

Character owns character identity.

Translation consumes:

```text
Character Context Snapshot
```

Character does not own speaker attribution.

---

# Session

Session owns temporary working context.

Session is resumable.

Session is not:

* authentication
* runtime job
* provider conversation

---

# Profile

Profile defines reusable processing intent.

Profile owns immutable revisions.

Profiles include:

* Translation
* OCR
* Presentation
* Validation
* Context
* Routing

---

# Revision Pattern

Several domains share the same architecture.

```text
Stable Identity
        │
Revision
        │
Immutable Snapshot
        │
Historical Reference
```

This pattern is intentionally reused.

---

# Resolution Pattern

Configuration resolution follows:

```text
Workspace

↓

Project

↓

Book

↓

Chapter

↓

Session

↓

Operation Override

↓

Resolved Snapshot
```

Every operation uses the resolved snapshot.

---

# Domain Relationships

Simplified relationship graph:

```text
Workspace
    │
Project
    │
Book
    │
Chapter
    │
Page
    │
Image
    │
TextBlock
    │
Translation

Translation
├── Language
├── Character Snapshot
├── Glossary Snapshot
├── Profile Snapshot
└── Session Context Snapshot
```

---

# Separation of Concerns

The Domain intentionally excludes:

* REST APIs
* HTTP
* WebSocket
* Queue implementation
* Prompt engineering
* OCR engine
* Provider SDK
* SQL schema
* Cache implementation
* Search engine
* Authentication protocol

These belong to other architecture layers.

---

# Event Philosophy

Domain events describe meaningful business changes.

Examples:

```text
TranslationRevisionCreated
CharacterRevisionApproved
GlossaryRevisionPublished
ProfileActivated
SessionPaused
ProjectArchived
```

They do not describe infrastructure events.

---

# Identity Philosophy

Every long-lived business object owns:

```text
Stable ID
```

Every meaningful historical change owns:

```text
Revision ID
```

Stable identity never changes.

Revisions are append-only.

---

# Snapshot Philosophy

Historical operations reference immutable snapshots.

Never:

```text
Current Profile
```

Instead:

```text
Resolved Profile Snapshot
```

Never:

```text
Current Character
```

Instead:

```text
Character Context Snapshot
```

---

# Business Flow

Conceptually:

```text
Content

↓

Language

↓

Context

↓

Profile

↓

Translation

↓

Validation

↓

Presentation
```

Infrastructure executes this flow.

The Domain defines only the business meaning.

---

# Domain Invariants

The Domain architecture follows these global invariants.

1. Business truth is immutable.
2. Stable identities never change.
3. Revisions are append-only.
4. Historical artifacts never depend on mutable state.
5. Providers never appear in the Domain.
6. Runtime execution never appears in the Domain.
7. Configuration is provider-neutral.
8. Context is explicit.
9. Snapshots guarantee reproducibility.
10. Workspace owns collaboration.
11. Project owns content.
12. Session owns temporary work.
13. Translation owns historical output.
14. Profiles describe intent.
15. Policies constrain intent.
16. Infrastructure executes intent.
17. Aggregates communicate by references.
18. Business relationships are explicit.
19. Business rules are deterministic.
20. Domain concepts remain technology-independent.

---

# Current Domain Documents

Core documents:

```text
README.md

WORKSPACE.md
PROJECT.md

BOOK.md
CHAPTER.md
PAGE.md
IMAGE.md

TEXT_BLOCK.md
LANGUAGE.md
TRANSLATION.md

CHARACTER.md
GLOSSARY.md
PROFILE.md
SESSION.md
```

Future documents may include:

```text
ANNOTATION.md
REVIEW.md
COMMENT.md
TAG.md
ATTACHMENT.md
IMPORT.md
EXPORT.md
KNOWLEDGE.md
STYLE_GUIDE.md
```

---

# Reading Order

Recommended reading sequence for contributors:

1. README.md
2. WORKSPACE.md
3. PROJECT.md
4. BOOK.md
5. CHAPTER.md
6. PAGE.md
7. IMAGE.md
8. TEXT_BLOCK.md
9. LANGUAGE.md
10. GLOSSARY.md
11. CHARACTER.md
12. PROFILE.md
13. SESSION.md
14. TRANSLATION.md

This order moves from ownership, to content, to context, and finally to processing.

---

# Relationship to Other Architecture Documents

This directory defines **business concepts only**.

Other architecture directories define:

```text
architecture/
    domain/
        Business model

    runtime/
        Execution

    integration/
        External systems

    presentation/
        UI rendering

    security/
        Authentication & authorization

    operations/
        Audit, billing, usage

    ai/
        Context compilation
        Prompt generation
        Routing
```

The Domain must remain valid independently of those implementations.

---

# Final Principle

The entire CRAI architecture is built around one central idea:

```text
Mutable Working Context

↓

Immutable Resolved Snapshot

↓

Historical Business Artifact
```

This principle ensures that every Translation, OCR result, validation outcome and presentation can always be reproduced, audited and understood, regardless of future changes to users, profiles, providers or infrastructure.
