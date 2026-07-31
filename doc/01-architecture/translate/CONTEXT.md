# Translation Context

## Purpose

The Translation Context layer prepares the information required to translate a text segment accurately and consistently.

A segment should not be translated in isolation. Its meaning may depend on nearby sentences, speakers, character names, glossary terms, chapter information, or previous translations.

This layer connects text segmentation with the translation engine.

---

## Position in the Pipeline

```text
OCR Reading Order
        │
        ▼
Text Model
        │
        ▼
Segmentation
        │
        ▼
Translation Context
        │
        ▼
Translation
        │
        ▼
Presentation
```

The Context layer receives structured segments from the Text Architecture and produces translation-ready requests.

---

## Responsibilities

The Translation Context layer is responsible for:

* Selecting the segment to translate
* Collecting nearby segments
* Preserving document and chapter information
* Resolving known character names
* Applying glossary terms
* Providing previous translations when useful
* Identifying dialogue and speaker information
* Limiting context to an appropriate size
* Producing a stable input for translation

It does not perform the translation itself.

---

## Why Context Is Required

The same sentence may have different meanings depending on its surrounding content.

For example:

```text
他回来了。
```

Without context, this may only be translated as:

```text
Anh ấy đã trở lại.
```

However, nearby content may reveal that:

* The subject is a named character
* The speaker is referring to an elder
* The sentence uses a formal or emotional tone
* The character name has an established Vietnamese translation

Context helps the translation engine preserve the intended meaning.

---

## Context Sources

A translation context may contain information from several sources.

### Current Segment

The segment currently being translated.

This is the only content that must produce a translated output.

### Neighboring Segments

Segments appearing before or after the current segment.

They help resolve:

* Pronouns
* Sentence continuation
* Dialogue flow
* Subject references
* Tone
* Scene continuity

Neighboring segments provide context but should not normally be translated again.

### Document Context

General information about the source document.

Examples:

* Book title
* Chapter title
* Page number
* Content type
* Source language
* Target language

### Character Context

Known information about characters.

Examples:

* Original name
* Translated name
* Gender
* Titles
* Relationships
* Preferred pronouns

### Glossary Context

Terms that should be translated consistently.

Examples:

* Character names
* Places
* Organizations
* Skills
* Items
* Cultivation levels
* Repeated technical terms

### Translation History

Previous translations from the same chapter or scene.

Translation history helps maintain:

* Terminology consistency
* Character voice
* Pronoun usage
* Naming conventions
* Sentence continuity

### User Preferences

Translation preferences selected by the user.

Examples:

* Literal or natural translation
* Preferred Vietnamese pronouns
* Name translation rules
* Preserve or translate honorifics
* Reading style
* Genre-specific terminology

---

## Context Model

A translation context can be represented as:

```ts
interface TranslationContext {
  requestId: string;

  sourceLanguage: string;
  targetLanguage: string;

  currentSegment: TranslationSegment;

  previousSegments: ContextSegment[];
  nextSegments: ContextSegment[];

  document?: DocumentContext;
  characters?: CharacterContext[];
  glossary?: GlossaryEntry[];
  translationHistory?: TranslationHistoryEntry[];

  preferences?: TranslationPreferences;

  contextVersion: number;
}
```

The exact implementation may change, but the responsibilities of each field should remain stable.

---

## Current Segment

The current segment is the translation target.

```ts
interface TranslationSegment {
  segmentId: string;
  text: string;

  blockId?: string;
  pageId?: string;
  chapterId?: string;

  segmentType: SegmentType;
  sourceOrder: number;

  speakerId?: string;
  language?: string;
}
```

Possible segment types include:

```ts
type SegmentType =
  | "narration"
  | "dialogue"
  | "thought"
  | "title"
  | "caption"
  | "sound_effect"
  | "unknown";
```

Segment type helps the translation engine choose appropriate wording and formatting.

---

## Neighboring Context

Nearby segments should be selected using logical reading order rather than only physical position.

```text
Previous Segment
        │
        ▼
Current Segment
        │
        ▼
Next Segment
```

For novels, neighboring context usually follows paragraph order.

For comics, neighboring context follows the reading order produced by:

```text
docs/architecture/ocr/READING_ORDER.md
```

Physical proximity alone is not sufficient because two nearby text regions may belong to different panels or conversations.

---

## Context Window

The context window defines how much surrounding content is included.

A basic context window may contain:

* Several previous segments
* The current segment
* Several following segments
* Relevant glossary entries
* Relevant character information

The window should be large enough to preserve meaning but small enough to avoid unnecessary processing.

```ts
interface ContextWindowPolicy {
  maxPreviousSegments: number;
  maxNextSegments: number;
  maxCharacters: number;
}
```

The context builder may reduce the window when the total context exceeds the configured limit.

The current segment must never be removed.

---

## Context Selection

Context should be selected by relevance rather than by simply attaching all available data.

Recommended priority:

```text
Current Segment
        │
        ▼
Directly Connected Segments
        │
        ▼
Speaker and Character Information
        │
        ▼
Relevant Glossary Entries
        │
        ▼
Recent Translation History
        │
        ▼
General Document Information
```

For example, a glossary entry should only be included when:

* Its source term appears in the current context
* It is related to an identified character or location
* It is required by a user-defined translation rule

This prevents unrelated information from confusing the translation engine.

---

## Dialogue Context

Dialogue translation may depend on speaker identity and relationships.

```ts
interface CharacterContext {
  characterId: string;

  sourceName: string;
  translatedName?: string;

  gender?: string;
  titles?: string[];
  aliases?: string[];

  relationships?: CharacterRelationship[];
}
```

When speaker identity is uncertain, the context should preserve that uncertainty instead of inventing a speaker.

```ts
interface SpeakerReference {
  characterId?: string;
  confidence?: number;
  manuallyConfirmed?: boolean;
}
```

Manual confirmation should take precedence over automatically inferred speaker information.

---

## Glossary Context

Glossary entries define preferred translations.

```ts
interface GlossaryEntry {
  sourceTerm: string;
  targetTerm: string;

  category?: string;
  description?: string;

  caseSensitive?: boolean;
  mandatory?: boolean;
}
```

Example:

```json
{
  "sourceTerm": "筑基期",
  "targetTerm": "Trúc Cơ kỳ",
  "category": "cultivation_level",
  "mandatory": true
}
```

Mandatory glossary entries must be followed unless the user explicitly overrides them.

The Context layer selects relevant glossary entries. It does not own or permanently store the glossary.

---

## Translation History

Recent translated segments may be included to preserve continuity.

```ts
interface TranslationHistoryEntry {
  segmentId: string;
  sourceText: string;
  translatedText: string;
  sourceOrder: number;
}
```

Translation history should normally come from the same:

1. Dialogue
2. Scene
3. Page
4. Chapter

Distant or unrelated translations should not be included unless they contain required terminology.

---

## Context for Novels

Novel context primarily depends on:

* Paragraph order
* Sentence continuity
* Character references
* Narrative point of view
* Dialogue turns
* Chapter terminology

Example:

```text
Previous paragraph
        │
        ▼
Current paragraph or sentence
        │
        ▼
Next paragraph
```

A long paragraph may be divided into several segments, but all segments should retain their common paragraph identity.

---

## Context for Comics

Comic context primarily depends on:

* Page reading order
* Panel grouping
* Speech bubble relationships
* Speaker identity
* Nearby dialogue
* Visual text type

Example:

```text
Page
 ├── Panel 1
 │    ├── Dialogue A
 │    └── Dialogue B
 │
 └── Panel 2
      └── Dialogue C
```

When translating `Dialogue B`, the most relevant context is usually:

* Dialogue A
* The current panel
* The identified speaker
* Previous dialogue in the same conversation

Text from another nearby panel should not be included unless the reading-order model connects them.

---

## Context Construction

The Context layer builds translation input through the following steps:

```text
Receive Segment
        │
        ▼
Locate Document Position
        │
        ▼
Collect Neighboring Segments
        │
        ▼
Resolve Speaker and Characters
        │
        ▼
Select Relevant Glossary Entries
        │
        ▼
Attach Translation History
        │
        ▼
Apply User Preferences
        │
        ▼
Enforce Context Limits
        │
        ▼
Produce Translation Request
```

Each step should be deterministic when given the same input and configuration.

---

## Translation Request

The output of the Context layer is a translation request.

```ts
interface TranslationRequest {
  requestId: string;

  sourceLanguage: string;
  targetLanguage: string;

  sourceText: string;
  segmentId: string;
  segmentType: SegmentType;

  context: TranslationContext;

  contextVersion: number;
}
```

The translation engine should produce output only for `sourceText`.

Context content is supporting information and must not be returned as additional translated paragraphs.

---

## Context Identity and Versioning

A context should have a stable identity and version.

Its version should change when relevant inputs change, such as:

* Segment text
* Reading order
* Speaker information
* Glossary terms
* Translation history
* User preferences

This prevents an outdated translation result from being applied to a newer context.

A context fingerprint may be generated from:

```text
Segment Revision
+ Neighbor Segment Revisions
+ Glossary Revision
+ Character Revision
+ Preference Revision
+ Context Builder Version
```

---

## Uncertainty

Context data may be incomplete or uncertain.

Examples:

* Unknown speaker
* Uncertain reading order
* Unresolved name
* Low-confidence OCR text
* Missing previous page

Uncertain information should include confidence metadata where available.

The Context layer must not convert uncertain information into confirmed facts.

---

## Manual Overrides

Users may correct:

* Character names
* Speaker identity
* Pronouns
* Glossary terms
* Reading order
* Segment relationships

Manual overrides must take precedence over automatic inference.

They should remain traceable and reversible.

---

## Failure Handling

Context construction should not fail only because optional information is unavailable.

For example:

* Missing character information → continue without character context
* Missing glossary → continue without glossary context
* Missing next segment → translate using available context
* Missing translation history → continue without history

Context construction should fail only when required data is invalid, such as:

* Missing current segment
* Empty source text
* Invalid source order
* Unsupported context version

---

## Design Principles

### Target Isolation

Only the current segment is translated.

Neighboring content is context only.

### Relevance First

Include information because it helps the current translation, not simply because it is available.

### Source Preservation

Every context item should remain traceable to its source segment, document, glossary, or user correction.

### Deterministic Construction

The same inputs and configuration should produce the same translation context.

### Explicit Uncertainty

Unknown or low-confidence information must remain clearly marked.

### Provider Independence

The context model must not depend on a specific AI provider or translation API.

### Bounded Context

Context must respect configurable size limits.

### User Authority

Manual corrections and user-defined terminology have the highest priority.

---

## Interaction with Other Modules

### Text Architecture

Provides:

* Structured text
* Segments
* Segment types
* Logical reading order
* Source mappings

Related documents:

```text
../text/TEXT_MODEL.md
../text/SEGMENTATION.md
```

### OCR Architecture

Provides comic text regions and reading-order information.

Related document:

```text
../ocr/READING_ORDER.md
```

### Domain Architecture

Provides:

* Book information
* Chapter information
* Characters
* Glossary
* User corrections

### Translation Engine

Consumes the completed translation request and produces translated text.

Related document:

```text
TRANSLATION.md
```

### Presentation Architecture

Receives translated segments after translation.

It does not build translation context.

Related document:

```text
../presentation/PRESENTATION.md
```

---

## MVP Scope

The first version should support:

* Current segment
* Previous and next segments
* Document and chapter identifiers
* Source and target languages
* Segment type
* Basic glossary entries
* User translation preferences
* Context size limits

Advanced character relationships, speaker inference, semantic retrieval, and long-term translation memory can be added later.

---

## Future Extensions

Possible future capabilities include:

* Automatic speaker detection
* Character relationship graphs
* Scene-aware context
* Semantic retrieval from earlier chapters
* Learned terminology preferences
* Context ranking
* Context compression
* AI-assisted glossary generation
* Cross-chapter translation memory

These capabilities should extend the context model without changing its primary role.

---

## Invariants

1. Every translation context must contain one current segment.
2. The current segment must never be removed by context-size reduction.
3. Context segments must follow logical reading order.
4. Neighboring segments must not be treated as translation targets.
5. Context items must remain traceable to their sources.
6. Manual overrides must take precedence over automatic inference.
7. Optional missing context must not block translation.
8. Context construction must remain independent of the translation provider.
9. Context versions must change when relevant inputs change.
10. Uncertain information must not be represented as confirmed.
11. Glossary rules marked as mandatory must be preserved.
12. Context construction must not modify the source text model.

---

## Related Documents

```text
../ocr/READING_ORDER.md
../text/TEXT_MODEL.md
../text/SEGMENTATION.md
TRANSLATION.md
../presentation/PRESENTATION.md
```
