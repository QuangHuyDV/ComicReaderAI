# Translation Module Contracts

> **Project:** CRAI
> **Module:** Translation
> **Document:** Public Contracts
> **Version:** 0.1
> **Status:** Architecture Draft
> **Last Updated:** 2026-07-25

---

# 1. Purpose

This document defines every public contract owned by the Translation module.

These contracts describe:

- commands;
- request models;
- response models;
- identifiers;
- configuration models;
- alignment models;
- warnings;
- provider-neutral translation contracts.

The document intentionally avoids implementation details and provider-specific APIs.

---

# 2. Design Principles

The Translation module:

- translates prepared source text;
- preserves structural alignment;
- remains provider-neutral;
- supports multiple translation providers;
- supports retry and fallback;
- never exposes provider-specific payloads outside the module.

Public contracts must remain stable even when the internal translation provider changes.

---

# 3. Public Commands

The module exposes the following public commands.

```text
TranslateText
CancelTranslation
RetryTranslation
InvalidateTranslation
```

No other module should directly invoke provider APIs.

---

# 4. Primary Request

## TranslateTextRequest

Represents one translation job.

Contains:

```text
TranslationJobId
ReadingSessionId
ContentRevision
PreparedDocument
TranslationConfiguration
TraceContext
```

The request always refers to an immutable content revision.

---

# 5. PreparedDocument

Produced by Text Processing.

Contains:

```text
DocumentId

PreparedSegments[]

LanguageProfile

ContentProfile

StructureMetadata

ContextMetadata
```

Translation never accepts raw OCR output.

Only prepared text.

---

# 6. PreparedSegment

Represents one translation unit.

Contains:

```text
PreparedSegmentId

Sequence

SourceText

ContextReference

ParagraphId

DialogueGroupId

RegionId

Flags
```

The source text must already be normalized.

---

# 7. TranslationConfiguration

Represents runtime translation preferences.

Contains:

```text
SourceLanguage

TargetLanguage

TranslationProfile

ProviderPolicy

TerminologyPolicy

ContextPolicy

StreamingPolicy

TimeoutPolicy
```

Configuration affects execution but never changes the source text.

---

# 8. TranslationProfile

Possible values:

```text
Comic

Novel

General

Literal

Natural

Custom
```

Profiles guide style.

They do not determine the provider.

---

# 9. ProviderPolicy

Defines provider selection rules.

Contains:

```text
PreferredProvider

FallbackProviders

RetryPolicy

CostPreference

LatencyPreference

OfflineAllowed
```

Provider names remain opaque identifiers.

---

# 10. TerminologyPolicy

Defines terminology handling.

Contains:

```text
GlossaryEnabled

LockedTerms

PreferredNames

SeriesDictionary

HonorificPolicy
```

The Translation module consumes terminology.

It does not own glossary storage.

---

# 11. ContextPolicy

Defines contextual translation behavior.

Contains:

```text
MaximumContextSegments

PreviousSegments

NextSegments

ParagraphContext

ChapterContext
```

The Translation module decides how much context to send.

---

# 12. Translation Result

## TranslationResult

Contains:

```text
TranslationJobId

ResultRevision

TranslatedSegments[]

Warnings[]

Statistics

ProviderMetadata
```

A result always belongs to exactly one translation job.

---

# 13. TranslatedSegment

Represents translated output.

Contains:

```text
TranslatedSegmentId

PreparedSegmentId

TranslatedText

Confidence

Flags
```

Every translated segment must reference exactly one prepared segment.

---

# 14. Alignment

Translation preserves structural mapping.

```text
PreparedSegment
        ↓
TranslatedSegment
```

Alignment must survive retries and provider changes.

---

# 15. ProviderMetadata

Contains execution metadata.

Examples:

```text
ProviderId

ProviderRequestId

ModelIdentifier

ExecutionTime

TokenUsage

CachedResult

RetryCount
```

Provider-specific payloads are never exposed.

---

# 16. Translation Warning

Possible warning categories:

```text
LowConfidence

LongSentence

AmbiguousMeaning

MissingContext

TerminologyConflict

PartialTranslation

ProviderFallback
```

Warnings do not invalidate successful translations.

---

# 17. Translation Statistics

Contains:

```text
SegmentCount

CharacterCount

InputTokens

OutputTokens

ElapsedTime

ProviderLatency
```

Statistics are informational.

---

# 18. Partial Result

Streaming or interrupted execution may produce:

```text
Completed

CompletedWithWarnings

Partial

Failed

Cancelled

Superseded
```

Partial results must clearly identify missing translated segments.

---

# 19. Retry

Retry always creates:

```text
New TranslationJobId
```

Retries never mutate previous results.

---

# 20. Cancellation

Cancellation suppresses publication.

The module may still finish communicating with the provider internally.

Cancelled jobs never become authoritative.

---

# 21. Supersession

A translation becomes obsolete when:

```text
PreparedDocument Revision
        changes
```

Older translations become:

```text
SUPERSEDED
```

They must never overwrite newer results.

---

# 22. Traceability

The complete mapping is:

```text
ReadingSessionId
        ↓
PreparedDocument
        ↓
PreparedSegment
        ↓
TranslationJob
        ↓
TranslatedSegment
```

No step may lose traceability.

---

# 23. Ownership

Translation owns:

- translation jobs;
- provider execution;
- translated text;
- alignment;
- provider metadata;
- retry behavior.

Translation does NOT own:

- OCR;
- normalization;
- glossary storage;
- presentation;
- reading session.

---

# 24. Compatibility

Every public contract in this document must remain compatible with:

```text
modules/text-processing/CONTRACTS.md

modules/presentation/CONTRACTS.md

modules/knowledge/CONTRACTS.md
```

Provider changes must not require public contract changes.