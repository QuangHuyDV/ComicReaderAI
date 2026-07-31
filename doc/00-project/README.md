# Project Documentation

This directory contains product-level documentation for CRAI.

It defines why the product exists, who it serves, and which user problems it must solve.

Architecture documents may depend on this directory, but product documents must not depend on implementation details.

---

## Documents

### `USER_JOURNEY.md`

Describes the primary user journeys and expected reading experience.

Future documents may include:

- `PRODUCT_VISION.md`
- `SCOPE.md`
- `MVP.md`
- `GLOSSARY.md`
- `USE_CASES.md`
- `NON_GOALS.md`

---

## Product Direction

CRAI is a reading assistant designed to reduce interruption when consuming foreign-language comics and novels.

Initial priorities:

1. Chinese to Vietnamese
2. English to Vietnamese
3. Screen comic translation
4. Structured text translation
5. Minimal user interaction during reading

---

## Current MVP

The initial MVP focuses on:

- desktop application
- user-selected screen region
- automatic content-change detection
- OCR for comic text
- Chinese-to-Vietnamese translation
- side-panel presentation
- continuous reading without repeated mouse interaction

The MVP does not initially require:

- browser extension
- translated image replacement
- cloud synchronization
- local comic library
- batch chapter processing

---

## Documentation Rules

Product documents define:

- user goals
- product scope
- expected behavior
- product constraints

They must not define:

- worker thread counts
- queue implementation
- database technology
- OCR library selection
- UI framework selection

Those decisions belong to architecture or implementation documents.