# Language Domain

* **Document:** Domain / Language
* **Version:** 1.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

The Language domain defines how CRAI identifies, validates and transports language-related information across OCR, text extraction, translation, presentation and rendering workflows.

Language information influences:

* OCR provider selection
* Text normalization
* Translation routing
* Glossary lookup
* Prompt construction
* Script handling
* Writing direction
* Font selection
* Line breaking
* User preferences
* Validation
* Cache compatibility

Language must be represented consistently across the system.

Provider-specific language identifiers must never become canonical domain values.

---

# Domain Role

Language is a shared domain concept used by multiple aggregates and modules.

```text
Project
   │
   ├── Source Language
   ├── Target Languages
   │
   ▼
Book / Chapter Overrides
   │
   ▼
Page Language Detection
   │
   ▼
Text Block Language
   │
   ▼
Translation Language Pair
   │
   ▼
Presentation and Rendering
```

Language is normally represented as an immutable Value Object.

It is not an independently owned aggregate.

---

# Responsibilities

The Language domain is responsible for:

* Representing canonical language identity
* Representing script and regional variants
* Validating language tags
* Supporting mixed-language content
* Describing writing characteristics
* Supporting language detection results
* Mapping provider-specific codes
* Supporting translation compatibility
* Supporting locale-sensitive formatting
* Supporting font and layout selection
* Participating in cache and revision identity

The Language domain is not responsible for:

* Detecting language from raw text
* Translating content
* Selecting AI providers
* Loading fonts
* Rendering glyphs
* Constructing prompts
* Managing user interface localization
* Managing dictionaries or glossary entries

Those responsibilities belong to Detection, Translation, Provider, Presentation, Rendering, Localization and Glossary components.

---

# Core Concepts

The domain separates the following concepts:

```text
Language
├── Language Code
├── Script
├── Region
├── Variant
├── Writing Direction
└── Confidence
```

These concepts must not be treated as interchangeable.

For example:

```text
zh-Hans-CN
```

contains:

* Language: Chinese
* Script: Simplified Han
* Region: China

It does not merely mean “Simplified Chinese” as one indivisible string.

---

# Canonical Language Tag

CRAI should use BCP 47-compatible language tags as its canonical external representation.

Examples:

```text
vi
en
en-US
zh
zh-Hans
zh-Hant
zh-Hans-CN
zh-Hant-TW
ja
ko
```

Canonical tags should follow this structure:

```text
language[-Script][-REGION][-variant]
```

Examples:

| Tag          | Meaning                                     |
| ------------ | ------------------------------------------- |
| `vi`         | Vietnamese                                  |
| `en`         | English                                     |
| `en-US`      | English used in the United States           |
| `zh-Hans`    | Chinese written with Simplified Han script  |
| `zh-Hant`    | Chinese written with Traditional Han script |
| `zh-Hant-TW` | Traditional Chinese associated with Taiwan  |
| `ja`         | Japanese                                    |
| `ko`         | Korean                                      |

CRAI should store the canonical normalized form rather than user-entered casing.

---

# Language Value Object

Recommended representation:

```text
Language
├── Tag
├── Base Language
├── Script
├── Region
├── Variants
├── Writing System
└── Metadata
```

Typical fields:

* Canonical Tag
* ISO Language Code
* Script Code
* Region Code
* Variants
* Display Name
* Native Name
* Default Writing Direction
* Default Text Orientation
* Normalization Profile
* Status

The canonical tag is the primary identity.

Derived display names must not be used as identifiers.

---

# Identity and Equality

Two Language values are equal when their normalized canonical tags are equal.

Examples:

```text
ZH-hans
zh-Hans
zh-hans
```

should normalize to:

```text
zh-Hans
```

Language equality must not depend on:

* Display name
* Provider code
* User interface language
* Translated language name
* Font availability
* Detection confidence

When less-specific and more-specific tags are compared, they are compatible but not equal.

Example:

```text
zh
```

is not equal to:

```text
zh-Hans
```

However, it may be considered compatible under a fallback policy.

---

# Base Language

The base language represents the primary linguistic identity.

Examples:

| Code | Language   |
| ---- | ---------- |
| `vi` | Vietnamese |
| `zh` | Chinese    |
| `en` | English    |
| `ja` | Japanese   |
| `ko` | Korean     |
| `th` | Thai       |
| `fr` | French     |
| `de` | German     |
| `es` | Spanish    |
| `ru` | Russian    |

Base language codes should use ISO 639 identifiers accepted by the canonical language-tag standard.

Three-letter language codes may be supported when no suitable two-letter code exists.

---

# Script

Script identifies the writing system used to encode the language.

Examples:

| Script Code | Script                  |
| ----------- | ----------------------- |
| `Latn`      | Latin                   |
| `Hans`      | Simplified Han          |
| `Hant`      | Traditional Han         |
| `Jpan`      | Japanese writing system |
| `Kore`      | Korean writing system   |
| `Cyrl`      | Cyrillic                |
| `Arab`      | Arabic                  |
| `Thai`      | Thai                    |
| `Deva`      | Devanagari              |

Language and script must remain separate.

Examples:

```text
zh-Hans
zh-Hant
```

share the same base language but use different scripts.

Script affects:

* OCR recognition models
* Character normalization
* Font fallback
* Glyph coverage
* Line breaking
* Text orientation
* Transliteration
* Translation validation

---

# Region

Region represents geographic or cultural language variation.

Examples:

```text
en-US
en-GB
pt-BR
pt-PT
zh-Hant-TW
zh-Hant-HK
```

Region may affect:

* Vocabulary
* Spelling
* Date formatting
* Punctuation
* Localization
* Formality conventions
* Translation preference

Region must not be inferred automatically from language unless a configured default is explicitly applied.

For example:

```text
en
```

must not silently become:

```text
en-US
```

inside persisted domain data.

---

# Variant

Variants describe additional linguistic conventions not fully represented by language, script or region.

Variants should be used sparingly.

Possible uses include:

* Orthographic conventions
* Historical language forms
* Romanization systems
* Specialized language variants

Provider-specific labels must not be stored as canonical variants.

---

# Language Range

Some configuration requires a range rather than an exact language.

Examples:

```text
zh
zh-*
zh-Hans
```

A Language Range may be used for:

* Provider capability rules
* Font selection
* Glossary applicability
* Translation profile selection
* Validation policy
* Fallback configuration

Language Range and Language must be separate value types.

A range describes compatibility.

A Language describes an actual or selected language identity.

---

# Undefined and Unknown Language

CRAI must distinguish between:

| Value                  | Meaning                                               |
| ---------------------- | ----------------------------------------------------- |
| `und`                  | Language is undetermined                              |
| `unknown` domain state | Detection has not run or no conclusion is available   |
| `mixed` domain state   | Content intentionally contains multiple languages     |
| `invalid`              | Supplied value is not a valid language representation |

The canonical tag `und` may be used when content exists but its language cannot be determined.

Absence of a language value may mean detection has not yet occurred.

These states must not be collapsed into one value.

---

# Mixed-Language Content

A Page, paragraph or Text Block may contain more than one language.

Examples:

* Chinese dialogue containing an English name
* Vietnamese text containing Japanese terminology
* A bilingual page
* Romanized pronunciation beside original characters
* Sound effects written in another script

Recommended representation:

```text
Language Analysis
├── Primary Language
├── Secondary Languages
├── Script Distribution
├── Segment Languages
└── Confidence
```

Mixed-language content may be represented using:

* One primary language
* Zero or more secondary languages
* Optional language spans
* Optional script spans

The domain should not create one Text Block per language unless layout or editing requirements justify it.

---

# Language Span

A Language Span associates part of a text value with a language.

```text
Language Span
├── Start Offset
├── End Offset
├── Language
├── Script
└── Confidence
```

Example:

```text
他使用了 Skill Burst。
```

Possible analysis:

```text
[0..5]  zh-Hans
[5..16] en
[16..17] zh-Hans
```

Offsets must use a clearly defined unit, such as Unicode code-point index.

Byte offsets should not be used for canonical domain mapping.

---

# Script Span

A Script Span identifies writing-system changes independently of language.

This is useful because:

* A language may use several scripts.
* Different languages may share one script.
* A foreign term may use the same language but a different script.
* Romanized names may remain part of the same linguistic expression.

Example:

```text
東京 Tokyo
```

may contain:

* Japanese script
* Latin script

without necessarily being treated as two unrelated languages.

---

# Source Language

Source Language represents the language expected or detected in source content.

It may be declared at multiple levels:

```text
Project Default
      │
      ▼
Book Override
      │
      ▼
Chapter Override
      │
      ▼
Page Detection
      │
      ▼
Text Block Detection
```

More specific values may override broader defaults.

However, detected values and configured values must remain distinguishable.

Recommended fields:

* Declared Language
* Detected Language
* Effective Language
* Detection Confidence
* Resolution Source

---

# Target Language

Target Language defines the language into which source content is translated.

Target Language should normally be explicitly selected.

It may originate from:

* Project settings
* Book or Chapter override
* Reading session preference
* One-time user request
* Translation Profile

Automatic target-language detection should not be used for ordinary translation execution.

A Translation must store its exact target language tag.

---

# Effective Language Resolution

Language configuration may exist at several levels.

Recommended resolution order:

```text
Explicit Operation Override
        ↓ fallback
Text Block Language
        ↓ fallback
Page Language
        ↓ fallback
Chapter Override
        ↓ fallback
Book Override
        ↓ fallback
Project Default
        ↓ fallback
Undetermined
```

Not every use case should use the same hierarchy.

Examples:

* OCR may prioritize Page detection.
* Translation may prioritize Text Block language.
* Rendering may prioritize Translation target language.
* Glossary selection may combine project defaults with exact block language.

The resolved result should include its source.

---

# Language Resolution Result

Recommended structure:

```text
Language Resolution
├── Effective Language
├── Resolution Source
├── Confidence
├── Alternatives
├── Warnings
└── Resolution Revision
```

Resolution sources may include:

* User selected
* Project configured
* Book configured
* Chapter configured
* Page detected
* Text Block detected
* Provider detected
* Imported metadata
* Fallback
* Undetermined

Configured language and detected language must not overwrite each other silently.

---

# Language Detection

Language detection is an application or infrastructure capability.

The domain records its result.

Recommended detection result:

```text
Language Detection Result
├── Primary Candidate
├── Alternative Candidates
├── Script Candidates
├── Confidence
├── Detection Scope
├── Detector ID
├── Detector Version
├── Input Hash
└── Created Time
```

Detection scope may be:

* Project sample
* Book
* Chapter
* Page
* Text Block
* Text span

Provider-specific confidence values should be normalized before use.

---

# Detection Candidate

A candidate may include:

* Language
* Script
* Confidence
* Evidence
* Rank

Example:

```text
1. zh-Hans — 0.93
2. ja      — 0.05
3. ko      — 0.02
```

Confidence is comparative evidence.

It is not a guarantee of correctness.

---

# Detection Confidence

Recommended normalized confidence range:

```text
0.0 to 1.0
```

Suggested interpretation:

| Range       | Meaning              |
| ----------- | -------------------- |
| `0.90–1.00` | Very high confidence |
| `0.75–0.89` | High confidence      |
| `0.50–0.74` | Moderate confidence  |
| `0.25–0.49` | Low confidence       |
| `0.00–0.24` | Very low confidence  |

Thresholds should be configurable.

They must not be treated as universal statistical guarantees across providers.

---

# Language Confirmation

Users may confirm or correct a detected language.

A confirmed language should record:

* Confirmed Language
* Previous Detected Language
* Actor
* Confirmation Time
* Scope
* Reason
* Revision

User confirmation has higher authority than automatic detection for the same scope.

A later detection operation must not silently replace a confirmed language.

---

# Writing Direction

Language and writing direction are related but separate.

Supported writing directions may include:

| Direction | Meaning       |
| --------- | ------------- |
| `ltr`     | Left to right |
| `rtl`     | Right to left |
| `ttb`     | Top to bottom |
| `btt`     | Bottom to top |

Examples:

* Vietnamese usually uses `ltr`.
* English usually uses `ltr`.
* Arabic commonly uses `rtl`.
* Traditional East Asian layouts may use vertical `ttb`.

Writing direction may be overridden at Text Block level.

The default direction from Language is only a fallback.

---

# Text Orientation

Text Orientation describes glyph arrangement within a writing direction.

Possible values:

* Horizontal
* Vertical
* Upright
* Sideways
* Mixed
* Unknown

For comics, orientation may differ between Text Blocks on the same Page.

Example:

```text
Page Language: zh-Hans
Block A: horizontal
Block B: vertical
Block C: rotated sound effect
```

Language must not force all Text Blocks into one orientation.

---

# Reading Direction

Reading Direction describes the order in which content units are consumed.

It is distinct from:

* Language
* Writing Direction
* Text Orientation

Examples:

* A Japanese manga may use right-to-left page and panel order.
* Japanese text inside a speech bubble may be vertical.
* A translated Vietnamese side panel may still display left-to-right.
* A web novel may use top-to-bottom paragraph order.

Reading direction belongs primarily to Book, Page layout or Presentation configuration.

Language only provides defaults where needed.

---

# Punctuation Profile

Languages and scripts may use different punctuation conventions.

Examples include:

* Full-width punctuation
* Chinese quotation marks
* Japanese corner brackets
* Latin quotation marks
* Ellipsis styles
* Sentence-ending punctuation
* Spacing before punctuation
* Repeated emphasis marks

A Punctuation Profile may define:

* Quote pairs
* Bracket pairs
* Sentence terminators
* Ellipsis form
* Whitespace policy
* Full-width normalization policy
* Vertical punctuation behavior

Punctuation conversion must occur through an explicit normalization or translation policy.

Language identification alone must not mutate punctuation.

---

# Unicode Normalization

Text normalization should use a declared Unicode normalization strategy.

Possible forms:

* NFC
* NFD
* NFKC
* NFKD
* Preserve

Recommended default for canonical text:

```text
NFC
```

Compatibility normalization such as NFKC may change semantic or stylistic distinctions and must be applied deliberately.

Raw OCR text should remain available when destructive normalization occurs.

---

# Chinese Language Handling

Chinese content requires explicit script handling.

Recommended canonical forms:

```text
zh-Hans
zh-Hant
```

Avoid using region alone to infer script.

For example:

* `zh-CN` often implies Simplified Chinese in practice.
* `zh-TW` often implies Traditional Chinese.
* `zh-SG` often uses Simplified Chinese.
* `zh-HK` commonly uses Traditional Chinese.

However, CRAI should preserve explicit script when known.

Preferred canonical translation configuration:

```text
zh-Hans → vi
zh-Hant → vi
```

rather than depending only on regional assumptions.

---

# Simplified and Traditional Chinese

Simplified and Traditional Chinese conversion is not identical to translation.

It is a script-conversion or orthographic transformation.

```text
zh-Hans ⇄ zh-Hant
```

Conversion may be:

* Character based
* Phrase aware
* Region aware
* Terminology aware

The result may require context because one source character can map to several target characters.

Script conversion should be modeled separately from semantic translation when exact distinction matters.

---

# Japanese Language Handling

Japanese content commonly combines:

* Kanji
* Hiragana
* Katakana
* Latin characters
* Arabic numerals

The canonical language may remain:

```text
ja
```

or use a script identifier when a lower-level subsystem requires one.

Mixed Japanese scripts do not imply mixed languages.

OCR and normalization must preserve script distinctions.

---

# Korean Language Handling

Korean content may contain:

* Hangul
* Hanja
* Latin text
* Numbers

The canonical language is normally:

```text
ko
```

Script analysis may still be used for OCR, font selection and validation.

---

# Vietnamese Language Handling

The canonical Vietnamese tag is:

```text
vi
```

Vietnamese output requires:

* Full Unicode diacritic support
* Correct combining-mark handling
* Vietnamese-aware font coverage
* Word and punctuation spacing
* Reliable line wrapping
* Preservation of proper names
* Avoidance of corrupted decomposed characters

Canonical translated text should normally be normalized to NFC.

Rendering must use fonts with complete Vietnamese glyph coverage.

---

# Language Pair

A Language Pair connects one source language to one target language.

```text
Language Pair
├── Source Language
├── Target Language
├── Translation Direction
└── Compatibility Metadata
```

Examples:

```text
zh-Hans → vi
zh-Hant → vi
ja → vi
ko → vi
en → vi
```

Language Pair is a Value Object.

It participates in:

* Translation identity
* Profile matching
* Provider routing
* Glossary selection
* Cache identity
* Validation
* Metrics

A reversed pair is a different value.

```text
zh-Hans → vi
```

is not equal to:

```text
vi → zh-Hans
```

---

# Language Pair Compatibility

A translation provider may support:

* Exact pairs
* Source language families
* Automatic source detection
* Any-to-any translation
* Script-specific pairs
* Region-specific pairs

Compatibility should be evaluated through provider capability mapping.

Example:

```text
Requested: zh-Hans → vi
Provider supports: zh → vi
```

This may be compatible.

However, the domain must preserve the original exact requested pair.

Provider fallback must not weaken stored language identity.

---

# Provider Language Mapping

Providers often use incompatible codes.

Examples:

```text
zh
zh-CN
zh-Hans
zh_chs
ChineseSimplified
auto
```

CRAI should use adapters:

```text
Canonical Language
        │
        ▼
Provider Language Mapper
        │
        ▼
Provider-Specific Code
```

Reverse mapping is also required:

```text
Provider Response Code
        │
        ▼
Provider Language Mapper
        │
        ▼
Canonical Language
```

Provider codes must remain inside provider adapters.

---

# Provider Mapping Record

Recommended mapping structure:

```text
Provider Language Mapping
├── Provider ID
├── Canonical Language Range
├── Provider Code
├── Capability Type
├── Direction
├── Mapping Revision
└── Limitations
```

Limitations may include:

* Detection only
* Translation only
* OCR only
* No vertical text
* No regional distinction
* Script automatically converted
* Target language unsupported
* Experimental support

---

# OCR Language Profile

OCR language selection may differ from Translation language selection.

An OCR Language Profile may include:

* Expected Languages
* Expected Scripts
* Primary Language
* Fallback Languages
* Vertical Text Support
* Dictionary Support
* Character Set
* Detection Mode
* Confidence Threshold

Examples:

```text
Expected:
- zh-Hans
- en

Primary:
- zh-Hans
```

A Page may use one OCR request for several expected languages.

Text Blocks should still preserve their detected effective language individually.

---

# Translation Language Profile

A Translation Language Profile may define:

* Source Language Range
* Target Language
* Source Script Policy
* Target Script Policy
* Automatic Detection Policy
* Mixed-Language Policy
* Transliteration Policy
* Untranslated-Term Policy
* Validation Thresholds

This profile should reference the general Translation Profile rather than duplicate unrelated style settings.

---

# Glossary Language Scope

Every Glossary Entry should declare language scope.

Possible fields:

* Source Language Range
* Target Language Range
* Source Term
* Target Term
* Script
* Region
* Case Sensitivity
* Match Policy

Example:

```text
Source: zh-Hans
Target: vi
Term: 灵力
Translation: linh lực
```

A glossary entry for `zh-Hans → vi` must not automatically apply to unrelated language pairs.

---

# Character Name Language

Character names may have several language-specific forms.

```text
Character Name
├── Original Form
├── Language
├── Script
├── Romanization
├── Target Translation
├── Aliases
└── Approval Status
```

Example:

```text
Original: 李青
Language: zh-Hans
Vietnamese: Lý Thanh
Romanization: Lǐ Qīng
```

Original name, romanization and translated name must remain distinguishable.

---

# Transliteration

Transliteration converts text between writing systems without translating meaning.

Examples:

* Chinese characters to Pinyin
* Japanese kana to Romaji
* Korean Hangul to Latin script
* Cyrillic to Latin script

Transliteration should declare:

* Source Language
* Source Script
* Target Script
* Transliteration Standard
* Result
* Revision

Transliteration is not a Translation unless the product explicitly presents it as one output type.

---

# Romanization

Romanization is a specialized form of transliteration into Latin script.

Examples:

* Pinyin
* Hepburn
* Revised Romanization
* Wade–Giles

Romanization standards must be explicit.

A generated romanized form should not replace the original name or canonical Translation automatically.

---

# Locale

Locale and Language must remain separate.

A Locale may include:

```text
Language
+
Region
+
Formatting Preferences
```

Locale affects:

* Number formatting
* Date formatting
* Time formatting
* Sorting
* Currency display
* User interface formatting

Translation target language does not automatically determine user interface locale.

Example:

```text
Translation target: vi
UI locale: en-US
```

is valid.

---

# Application Localization

Application localization controls the language of the CRAI interface.

It is separate from content language.

Examples:

```text
UI Language: vi
Source Content: zh-Hans
Target Content: vi
```

or:

```text
UI Language: en
Source Content: ja
Target Content: vi
```

The Localization module owns UI messages and resource bundles.

The Language domain provides canonical identifiers only.

---

# Font Compatibility

Language and script influence font requirements.

A font compatibility query may use:

* Language
* Script
* Required Unicode ranges
* Text orientation
* Weight
* Style
* Rendering mode

The Language domain does not select font files directly.

It provides metadata used by the Font or Rendering subsystem.

Font selection must consider actual glyph coverage rather than language name alone.

---

# Line Breaking

Line-breaking rules may vary by language and script.

Examples:

* Vietnamese normally breaks at word boundaries.
* Chinese can often break between characters.
* Japanese requires line-start and line-end restrictions.
* Punctuation may not be allowed at certain line positions.
* Vertical text requires different layout rules.

Language provides the line-breaking profile identifier.

Presentation and Rendering execute the actual layout.

---

# Word Segmentation

Not all languages use whitespace to separate words.

Examples:

* Vietnamese uses whitespace but may contain multi-syllable lexical units.
* Chinese normally has no word spaces.
* Japanese combines several scripts without word spaces.
* Thai commonly omits spaces between words.

Text processing must not assume that splitting on whitespace produces linguistic words.

Word segmentation belongs to a Language Processing capability.

The domain records which segmentation profile or version was used when relevant.

---

# Sentence Segmentation

Sentence boundaries depend on language-specific punctuation and context.

Sentence segmentation may be used for:

* Translation grouping
* Context construction
* Novel paragraph processing
* Quality evaluation
* Reading assistance

Segmentation results should preserve source offsets and segmentation revision.

A sentence is a derived structure.

It must not replace the original Text Block content.

---

# Language Capability

CRAI may maintain a registry of supported capabilities.

```text
Language Capability
├── Language Range
├── OCR Support
├── Translation Support
├── Detection Support
├── Vertical Text Support
├── Transliteration Support
├── Font Support
└── Quality Status
```

Capability status may include:

* Supported
* Experimental
* Partial
* Unsupported
* Provider dependent
* Local only
* Cloud only

Support must be evaluated per operation.

A language may be supported for translation but not OCR.

---

# Capability Matrix

Example conceptual matrix:

| Language  |       OCR | Translation to Vietnamese |         Vertical Text |         Local Mode |
| --------- | --------: | ------------------------: | --------------------: | -----------------: |
| `zh-Hans` | Supported |                 Supported |               Partial | Provider dependent |
| `zh-Hant` |   Partial |                 Supported |               Partial | Provider dependent |
| `ja`      |   Partial |                 Supported |             Supported | Provider dependent |
| `ko`      | Supported |                 Supported |               Limited | Provider dependent |
| `en`      | Supported |                 Supported | Not normally required |          Supported |
| `vi`      | Supported |         Source and target | Not normally required |          Supported |

This matrix is runtime configuration, not a fixed domain truth.

It may change as providers and local models change.

---

# Language Registry

A Language Registry may provide:

* Tag validation
* Canonicalization
* Display names
* Native names
* Script metadata
* Default direction
* Parent-language fallback
* Capability references
* Deprecation metadata

The registry should rely on standardized language metadata.

Application business records should store canonical tags rather than database-specific numeric identifiers alone.

---

# Fallback Hierarchy

Language fallback may remove specificity progressively.

Example:

```text
zh-Hant-TW
    ↓
zh-Hant
    ↓
zh
    ↓
und
```

Another example:

```text
en-GB
   ↓
en
   ↓
und
```

Fallback may be used for:

* Provider matching
* Glossary lookup
* Translation profile lookup
* Font selection
* UI display names

Fallback must not mutate the stored exact language.

The result should record which fallback level was used.

---

# Compatibility Matching

Recommended match strengths:

| Match      | Description                              |
| ---------- | ---------------------------------------- |
| `exact`    | Full canonical tags match                |
| `script`   | Base language and script match           |
| `language` | Base language matches                    |
| `range`    | A configured range includes the language |
| `fallback` | Match obtained through parent fallback   |
| `none`     | No compatible match                      |

Exact matching should be preferred.

Weak fallback should produce diagnostics when it may affect quality.

---

# Language Change

Changing a configured language may affect downstream artifacts.

Possible impacts:

| Change                             | Impact                                          |
| ---------------------------------- | ----------------------------------------------- |
| Display name changed               | No content invalidation                         |
| Region metadata added              | Review may be recommended                       |
| Script changed                     | OCR and Translation may become stale            |
| Source language corrected          | Translation may become stale                    |
| Target language changed            | New Translation required                        |
| Writing-direction override changed | Presentation becomes stale                      |
| Detection confidence updated       | Usually no invalidation                         |
| Language confirmed by user         | Dependent processing may require reconciliation |

Impact must be determined through explicit dependency rules.

---

# Revision Model

Language standards and application mappings evolve.

Versioned components may include:

* Language Registry Revision
* Provider Mapping Revision
* Detection Result Revision
* Language Resolution Revision
* Punctuation Profile Revision
* Normalization Profile Revision
* Segmentation Profile Revision

Canonical language tags themselves should remain stable.

Changes to provider mappings must not rewrite historical Translation records.

---

# Validation

Language validation should verify:

* Tag syntax is valid
* Language code is recognized or explicitly private
* Script code is valid
* Region code is valid
* Variant structure is valid
* Canonical casing is used
* Forbidden provider codes are not persisted
* Source and target language are compatible with the operation
* Required script is supported
* `und` usage follows policy
* Mixed-language state contains sufficient metadata
* Language spans remain inside text boundaries
* Span ranges do not overlap illegally

Invalid language values must not enter persisted domain records.

---

# Private-Use Tags

Private-use language tags may be required for internal experiments.

They must:

* Follow valid private-use syntax
* Be documented
* Remain isolated from external provider mapping
* Not replace standard tags when a standard tag exists
* Include migration rules if promoted to a standard representation

Private-use identifiers should be exceptional.

---

# Error Conditions

Typical errors include:

* Invalid language tag
* Unknown language code
* Invalid script code
* Invalid region code
* Unsupported source language
* Unsupported target language
* Unsupported language pair
* Provider mapping missing
* Ambiguous provider mapping
* Language detection failed
* Detection confidence below threshold
* Mixed language not allowed
* Script mismatch
* Writing direction conflict
* Language span out of range
* Language range invalid
* Target language undetermined
* Confirmed language overwrite attempted
* Unsupported transliteration standard

Errors should use canonical domain error categories.

Provider-specific error messages should be normalized before propagation.

---

# Events

Typical domain events include:

* `LanguageDetected`
* `LanguageDetectionFailed`
* `LanguageConfirmed`
* `LanguageCorrected`
* `LanguageResolutionChanged`
* `SourceLanguageChanged`
* `TargetLanguageChanged`
* `LanguagePairChanged`
* `ScriptDetected`
* `ScriptChanged`
* `MixedLanguageDetected`
* `LanguageCapabilityChanged`
* `ProviderLanguageMappingChanged`
* `LanguageProfileChanged`

Events should include:

* Scope identifier
* Previous value
* New value
* Resolution source
* Confidence
* Actor
* Revision
* Correlation ID

Raw source text should not be included unless explicitly required.

---

# Persistence

Recommended persisted representations:

```text
Language Value
├── Canonical Tag
├── Base Language
├── Script
├── Region
└── Variants
```

Detected language metadata may be stored separately:

```text
Language Detection
├── Scope Type
├── Scope ID
├── Candidates
├── Confidence
├── Detector
├── Input Hash
└── Revision
```

Provider mapping should be stored in configuration:

```text
Provider Language Mapping
├── Provider
├── Operation
├── Canonical Range
├── Provider Code
└── Revision
```

Derived display names should not be duplicated into every business record.

---

# Cache Participation

Language affects cache validity.

OCR cache keys may include:

* Expected Language Range
* Expected Scripts
* OCR Language Profile Revision

Translation cache keys must include:

* Source Language
* Target Language
* Script Policy
* Language Profile Revision

Rendering cache keys may include:

* Target Language
* Script
* Writing Direction
* Text Orientation
* Line-Breaking Profile
* Font Profile

Changing language-significant configuration must invalidate only dependent caches.

---

# Security and Privacy

Language metadata is normally low sensitivity.

However, language use may reveal:

* Reading interests
* Geographic background
* Cultural preferences
* Content origin
* User identity characteristics

Requirements:

* Do not infer user nationality from content language.
* Do not infer ethnicity from selected language.
* Do not use content language as authorization evidence.
* Avoid logging private source text during detection.
* Send only necessary text samples to remote detectors.
* Respect local-only processing settings.
* Keep Project-specific language preferences isolated.

Language detection is a technical classification, not a personal identity claim.

---

# Comic Processing Example

```text
Captured Comic Page
        │
        ▼
Project Source Default: zh-Hans
        │
        ▼
OCR Profile: zh-Hans + en
        │
        ▼
Text Block A detected as zh-Hans
Text Block B detected as zh-Hans
Text Block C detected as en
        │
        ▼
Effective source languages resolved
        │
        ▼
Translation:
zh-Hans → vi
en → vi
        │
        ▼
Vietnamese side-panel presentation
```

Each Text Block preserves its own effective language.

The Page may still retain `zh-Hans` as its primary language.

---

# Traditional Chinese Example

```text
Project Default: zh-Hans
        │
        ▼
Page Detection: zh-Hant
        │
        ▼
Confidence: High
        │
        ▼
Conflict Warning
        │
        ▼
User Confirms zh-Hant
        │
        ▼
Translation Profile:
zh-Hant → vi
```

The detected value must not silently rewrite the Project default.

The user confirmation creates a scoped override.

---

# Mixed-Language Example

Source:

```text
快使用 Ultimate Skill！
```

Analysis:

```text
Primary Language: zh-Hans
Secondary Language: en

Spans:
- 快使用          → zh-Hans
- Ultimate Skill → en
- ！             → inherited punctuation context
```

Translation policy may choose to:

* Translate the English term
* Preserve the English term
* Apply a glossary entry
* Transliterate it
* Show bilingual output

The selected behavior belongs to the Translation Profile.

---

# Novel Processing Example

```text
Browser Paragraph
        │
        ▼
Declared Chapter Language: zh-Hans
        │
        ▼
Paragraph Script Analysis
        │
        ▼
Language Resolution
        │
        ▼
Sentence Segmentation
        │
        ▼
Context-Aware Translation to vi
        │
        ▼
Vietnamese-aware line breaking
```

Language identity remains stable through extraction, translation and presentation.

---

# Architecture Invariants

1. Language is represented by a canonical normalized tag.
2. Provider-specific language codes never escape provider adapters.
3. Language, Script, Region and Writing Direction remain separate concepts.
4. Display names are never used as canonical identifiers.
5. Configured language and detected language remain distinguishable.
6. User-confirmed language has higher authority than automatic detection at the same scope.
7. Language equality requires equal normalized canonical tags.
8. Compatible language tags are not necessarily equal.
9. Target language must be explicit before Translation publication.
10. A Language Pair is directional.
11. Reversing a Language Pair creates a different value.
12. Text Blocks may override Page-level language.
13. Mixed-language content preserves primary and secondary language information.
14. Language spans preserve valid source offsets.
15. Script conversion is not automatically treated as semantic translation.
16. Transliteration is distinct from translation.
17. UI localization is distinct from source and target content language.
18. Language does not directly determine reading direction.
19. Language does not directly select a concrete font file.
20. Canonical text normalization policy is explicit.
21. Language changes invalidate only dependent artifacts.
22. Historical Translation records preserve their original language tags.
23. Fallback matching never mutates the stored exact language.
24. Cache keys include language-significant configuration.
25. Language detection confidence is evidence, not guaranteed correctness.
26. Cross-Project language preferences must not leak.
27. Language detection must not be used to infer personal identity.
28. Unsupported or invalid language values cannot enter durable domain state.

---

# Open Decisions

The following decisions should remain open until implementation and prototype testing:

* Whether CRAI stores parsed language components or only canonical tags
* Which language-registry library should be used
* Whether Page-level language detection runs automatically
* Whether Text Block detection is always enabled
* Which confidence threshold triggers user confirmation
* How mixed-language OCR requests are configured
* Whether Chinese script detection occurs before or after OCR
* Whether Chinese script conversion is supported in the MVP
* Whether `zh-CN` is normalized or preserved separately from `zh-Hans`
* How provider codes with weak script distinctions are mapped
* Which romanization standards are supported
* Whether transliteration is stored as Translation or auxiliary output
* How language spans are represented efficiently
* Which Unicode normalization form is used for every processing stage
* How Vietnamese word wrapping is validated
* Whether locale-specific Vietnamese variants are needed
* How script-aware font fallback is configured
* Whether language capability information is static or dynamically discovered
* Whether language changes trigger automatic retranslation
* How unsupported source languages are presented to the user
* Whether source language auto-detection may be delegated to Translation providers
* How language detection works for very short comic sound effects
* Whether punctuation inherits language from neighboring spans
* How private-use language tags are migrated

---

# Recommended MVP Scope

The first CRAI MVP should support:

* Canonical BCP 47-compatible language tags
* `zh-Hans`
* `zh-Hant`
* `en`
* `vi`
* Optional `ja`
* Optional `ko`
* Explicit target language selection
* Project source-language default
* Text Block language override
* Basic language detection metadata
* Basic script detection
* Mixed Chinese and English content
* Provider language-code mapping
* Source-to-target Language Pair
* Language-aware translation cache
* Vietnamese Unicode normalization
* Basic font compatibility metadata
* Horizontal and vertical text orientation metadata

The MVP may defer:

* Detailed language spans
* Automatic regional variant detection
* Advanced transliteration
* Chinese script conversion
* Dynamic provider capability discovery
* Complex locale formatting
* Private-use language tags
* User-defined language registries
* Full linguistic segmentation
* Fine-grained dialect handling
* Cross-language translation memory exchange

---

# Related Documents

* README.md
* PROJECT.md
* BOOK.md
* CHAPTER.md
* PAGE.md
* TEXT_BLOCK.md
* TRANSLATION.md
* GLOSSARY.md
* CHARACTER.md
* PROFILE.md
* SESSION.md
* `docs/architecture/CAPABILITY_MAP.md`
* `docs/architecture/DATA_FLOW.md`
* `docs/architecture/STATE_MACHINE.md`
* `docs/architecture/EVENT_BUS.md`
* `docs/architecture/ai/PIPELINE.md`
* `docs/architecture/ai/CONTEXT.md`
* `docs/architecture/ai/PROMPTS.md`
* `docs/architecture/ai/MODELS.md`
* `docs/architecture/ai/ROUTING.md`
* `docs/architecture/ai/RESPONSE.md`
* `docs/architecture/presentation/FONTS.md`
* `docs/architecture/presentation/LAYOUT.md`
* `docs/architecture/presentation/TYPOGRAPHY.md`
