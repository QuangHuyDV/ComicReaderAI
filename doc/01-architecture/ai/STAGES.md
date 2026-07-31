# AI Pipeline Stages

- **Document:** AI Architecture / Pipeline Stages
- **Version:** 1.0.0
- **Status:** Draft
- **Owner:** CRAI Architecture

---

# Purpose

This document defines every processing stage within the CRAI AI Pipeline.

Each stage has a single responsibility, well-defined input/output contracts, and may be replaced independently as long as it preserves its public contract.

---

# Design Principles

- Single responsibility
- Deterministic execution
- Immutable stage input
- Structured output
- Provider independence
- Observable processing
- Failure isolation

---

# Stage Overview

```text
Capture
   │
   ▼
OCR
   │
   ▼
Layout Analysis
   │
   ▼
Text Normalization
   │
   ▼
Context Builder
   │
   ▼
Prompt Builder
   │
   ▼
AI Routing
   │
   ▼
Model Execution
   │
   ▼
Response Validation
   │
   ▼
Post Processing
   │
   ▼
Rendering
```

---

# Stage Definitions

## 1. Capture

### Purpose

Acquire the original reading source.

### Input

- Screen
- Image
- Clipboard
- File
- Browser

### Output

- Raw image
- Metadata
- Capture timestamp

---

## 2. OCR

### Purpose

Extract machine-readable text from captured images.

### Responsibilities

- Text recognition
- Confidence scoring
- Bounding boxes
- Language hints

### Output

- OCR blocks
- Confidence values
- Coordinates

---

## 3. Layout Analysis

### Purpose

Understand document structure and reading order.

### Responsibilities

- Speech bubble detection
- Paragraph grouping
- Reading direction
- Region ordering

### Output

- Structured layout tree

---

## 4. Text Normalization

### Purpose

Prepare OCR output for AI processing.

### Responsibilities

- Remove OCR artifacts
- Normalize whitespace
- Merge fragmented lines
- Repair punctuation
- Unicode normalization

### Output

- Clean text segments

---

## 5. Context Builder

### Purpose

Collect contextual information that improves AI quality.

### Sources

- Previous pages
- Character memory
- Glossary
- User preferences
- Reading history

### Output

- Context package

---

## 6. Prompt Builder

### Purpose

Construct the final prompt sent to the AI provider.

### Responsibilities

- System prompt
- Developer prompt
- User prompt
- Context injection
- Formatting rules

### Output

- Final prompt request

---

## 7. AI Routing

### Purpose

Select the most appropriate provider and model.

### Routing Factors

- Task type
- Cost
- Latency
- Availability
- User preference
- Offline capability

### Output

- Selected provider
- Selected model

---

## 8. Model Execution

### Purpose

Execute the AI request.

### Responsibilities

- Send request
- Receive response
- Handle streaming
- Timeout management

### Output

- Raw provider response

---

## 9. Response Validation

### Purpose

Verify the correctness of AI output.

### Validation

- Required fields
- Format validation
- Language validation
- JSON validation
- Safety checks

### Output

- Validated response

---

## 10. Post Processing

### Purpose

Improve output before presentation.

### Responsibilities

- Terminology correction
- Formatting
- Style normalization
- Glossary replacement
- Minor cleanup

### Output

- Final structured result

---

## 11. Rendering

### Purpose

Present processed results to the user.

### Responsibilities

- Overlay rendering
- Text rendering
- Streaming updates
- Accessibility
- UI synchronization

### Output

- User-visible content

---

# Stage Contracts

Every stage exposes:

- Input Contract
- Output Contract
- Error Contract
- Metrics
- Diagnostics Events

Stages never share mutable runtime state.

---

# Cross-Cutting Concerns

Every stage supports:

- Cancellation
- Timeout
- Retry policy
- Diagnostics
- Tracing
- Metrics collection

---

# Failure Handling

If a stage fails:

1. Report diagnostics.
2. Classify the error.
3. Apply retry or fallback policy.
4. Stop downstream processing when recovery is impossible.

---

# Architecture Invariants

1. Each stage has exactly one primary responsibility.
2. Stages communicate only through defined contracts.
3. Stage execution order is deterministic.
4. Individual stages are independently replaceable.
5. Provider-specific logic is confined to AI Routing and Model Execution.
6. Validation always precedes rendering.
7. Every stage emits diagnostics and metrics.

---

# Related Documents

- README.md
- PIPELINE.md
- REQUEST.md
- RESPONSE.md
- PROMPTS.md
- CONTEXT.md
- MEMORY.md
- MODELS.md
- ROUTING.md
- STREAMING.md
- RETRY.md
- FALLBACK.md
- COST_CONTROL.md
- CACHE.md
- SAFETY.md
- OBSERVABILITY.md
