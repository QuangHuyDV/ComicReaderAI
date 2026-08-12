# Language Domain

* **Document:** Domain / Language
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

The `Language` domain defines the canonical language concepts used across CRAI.

It provides a consistent representation for:

* source language,
* target language,
* script,
* regional variation,
* mixed-language content,
* language ranges,
* language pairs,
* language resolution,
* language detection results,
* writing-system metadata,
* provider-language interoperability.

Language information may influence:

* OCR configuration,
* recognition,
* text normalization,
* Translation,
* Glossary matching,
* context construction,
* Presentation,
* typography,
* line breaking,
* cache compatibility,
* validation.

Language MUST remain provider-independent.

Provider-specific language identifiers MUST NOT become canonical domain values.

---

# Domain Role

Language is a shared domain concept.

It is normally represented through immutable Value Objects and scoped metadata rather than as one independently owned aggregate.

Conceptually:

```text
Project language intent
          |
          v
Optional content overrides
          |
          v
Detection / confirmation
          |
          v
Effective Language Resolution
          |
          +--> OCR / Recognition
          +--> Translation
          +--> Glossary
          +--> Presentation
```

Different consumers MAY resolve language differently.

There is no single global mutable "current language" for all CRAI operations.

---

# Responsibilities

The Language domain is responsible for:

* canonical language identity,
* script representation,
* regional variation,
* variant representation,
* language-tag validation,
* language equality,
* language compatibility,
* language ranges,
* language pairs,
* language-analysis metadata,
* mixed-language representation,
* language-resolution semantics,
* language-detection result representation,
* confirmation/override semantics,
* writing-system defaults,
* provider-code normalization boundaries.

The Language domain is NOT responsible for:

* performing language detection,
* executing OCR,
* executing Translation,
* selecting providers,
* rendering glyphs,
* loading fonts,
* UI localization resources,
* Glossary content,
* linguistic segmentation execution.

---

# Core Concepts

CRAI MUST distinguish:

```text
Language
Script
Region
Variant
Language Range
Language Pair
Language Analysis
Language Detection Result
Language Resolution
Writing Direction
Text Orientation
Reading Direction
Locale
```

These concepts are related but MUST NOT be treated as interchangeable.

---

# Canonical Language

CRAI SHOULD use BCP 47-compatible language tags as the canonical persisted and exchanged representation of a language.

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

Canonical shape:

```text
language[-Script][-REGION][-variant]
```

Canonical casing SHOULD be normalized.

Examples:

```text
ZH-hans
zh-hans
zh-Hans
```

normalize to:

```text
zh-Hans
```

---

# Language Value Object

Recommended conceptual representation:

```text
Language
├── canonicalTag
├── baseLanguage
├── script?
├── region?
└── variants[]
```

Derived metadata MAY include:

```text
displayName
nativeName
defaultWritingDirection
defaultTextOrientation
```

Derived metadata MUST NOT participate in Language identity unless explicitly defined.

The canonical normalized tag is the primary identity.

---

# Equality

Two Language values are equal when their normalized canonical tags are equal.

Example:

```text
zh-Hans == zh-Hans
```

but:

```text
zh != zh-Hans
```

The latter MAY be compatible under a matching policy, but they are not equal.

Language equality MUST NOT depend on:

* display name,
* provider code,
* font availability,
* detection confidence,
* user-interface language,
* translated language name.

---

# Base Language

Base language identifies the principal linguistic language.

Examples:

```text
vi
zh
en
ja
ko
th
fr
de
es
ru
```

Codes SHOULD follow those accepted by the canonical language-tag standard.

Three-letter codes MAY be supported where appropriate.

---

# Script

Script represents the writing system.

Examples:

```text
Latn
Hans
Hant
Jpan
Kore
Cyrl
Arab
Thai
Deva
```

Language and Script MUST remain distinct.

Example:

```text
zh-Hans
zh-Hant
```

share:

```text
baseLanguage = zh
```

while differing in Script.

Script MAY influence:

* OCR model compatibility,
* character normalization,
* font coverage,
* transliteration,
* line breaking,
* validation.

---

# Region

Region represents geographical or cultural variation.

Examples:

```text
en-US
en-GB
pt-BR
pt-PT
zh-Hant-TW
zh-Hant-HK
```

Region MUST NOT be silently inferred and persisted from base language alone.

For example:

```text
en
```

MUST NOT automatically become:

```text
en-US
```

without an explicit configured default or resolution policy.

---

# Variant

Variants represent additional language conventions not captured by base language, Script, or Region.

Variants SHOULD be used sparingly.

Provider-specific labels MUST NOT become canonical Language variants.

---

# Language Range

A `LanguageRange` represents matching intent rather than an exact language identity.

Examples:

```text
zh
zh-*
zh-Hans
```

LanguageRange MAY be used for:

* provider capability matching,
* Glossary applicability,
* profile matching,
* font compatibility,
* fallback rules.

Language and LanguageRange MUST remain different Value Objects.

---

# Undefined Language

CRAI MUST distinguish:

```text
no value
und
mixed
invalid
```

Meaning:

* **no value** — no language assertion currently exists.
* **`und`** — content exists but language is undetermined.
* **mixed** — content intentionally contains multiple languages.
* **invalid** — an invalid language representation was supplied.

These cases MUST NOT be collapsed.

---

# Mixed-Language Content

Content MAY contain more than one language.

Examples:

* Chinese dialogue containing an English skill name,
* Japanese text containing Latin-script names,
* bilingual content,
* mixed-language captions.

Recommended analysis:

```text
LanguageAnalysis
├── primaryLanguage?
├── secondaryLanguages[]
├── languageSpans[]
├── scriptSpans[]
└── confidence?
```

Mixed-language content SHOULD NOT automatically be split into multiple TextBlocks.

Segmentation depends on content and processing requirements.

---

# Language Span

A LanguageSpan associates a portion of text with a Language.

```text
LanguageSpan
├── startOffset
├── endOffset
├── language
├── script?
└── confidence?
```

Canonical offsets MUST use an explicitly defined text-indexing unit.

Unicode code-point or another stable documented representation SHOULD be preferred.

Raw byte offsets SHOULD NOT be used as canonical domain offsets.

---

# Script Span

A ScriptSpan identifies writing-system variation independently of language.

Example:

```text
東京 Tokyo
```

may use Japanese writing plus Latin script without necessarily representing two unrelated languages.

Language spans and Script spans MUST remain conceptually distinct.

---

# Source Language

Source language represents the language of source content.

CRAI MUST distinguish:

```text
Declared Language
Detected Language
Confirmed Language
Effective Language
```

These are different concepts.

A detected value MUST NOT silently overwrite a configured value.

A configured value MUST NOT erase detection evidence.

---

# Declared Language

Declared language expresses configuration or metadata intent.

Possible sources include:

* Project configuration,
* Book override,
* Chapter override,
* content metadata,
* explicit operation input,
* imported metadata.

Declared values express expected or selected language.

They are not evidence that automatic detection reached the same conclusion.

---

# Detected Language

Detected language is produced by a detection capability.

A detection result SHOULD preserve:

```text
LanguageDetectionResult
├── scope
├── primaryCandidate
├── alternativeCandidates[]
├── scriptCandidates[]
├── confidence?
├── detectorId
├── detectorVersion?
├── inputHash
└── createdAt
```

Detection is execution behavior.

The Language domain defines how its result is represented.

---

# Detection Scope

Detection MAY operate on:

```text
Project sample
Book
Chapter
Page
TextBlock
Text span
Source artifact
```

No one detection scope is universally required.

In particular, text-native content MUST NOT require Page-level detection.

---

# Detection Confidence

Confidence SHOULD use a normalized representation when providers allow it.

Recommended range:

```text
0.0 <= confidence <= 1.0
```

Confidence is evidence, not probability guaranteed to have identical statistical meaning across providers.

Threshold interpretation MUST remain configurable.

Missing confidence MUST remain distinct from zero.

---

# Confirmed Language

A user or trusted workflow MAY confirm or correct language.

Confirmation SHOULD preserve:

```text
scope
confirmedLanguage
previousDetection?
actor
confirmedAt
reason?
revision
```

Confirmed language has higher authority than automatic detection at the same scope.

A later detection MUST NOT silently replace it.

---

# Effective Language

`EffectiveLanguage` is the language selected for a particular operation after resolution.

It SHOULD be represented together with provenance.

Example:

```text
LanguageResolution
├── effectiveLanguage
├── resolutionSource
├── confidence?
├── alternatives[]
├── warnings[]
└── resolutionRevision
```

The effective language MUST NOT erase its source information.

---

# Resolution Is Operation-Specific

CRAI MUST NOT assume one global resolution hierarchy works for every operation.

For example:

```text
Translation
    prioritizes exact TextBlock language
```

```text
OCR
    may prioritize expected visual/source languages
```

```text
Glossary
    may match exact Translation language pair
```

```text
Presentation
    usually follows effective target-language metadata
```

Each consuming capability SHOULD define its own resolution policy.

---

# Generic Resolution Precedence

Where a generic source-language resolution is useful, recommended precedence is:

```text
Explicit Operation Override
        |
        v
Confirmed Content Language
        |
        v
Explicit TextBlock Override
        |
        v
Optional Page Override / Detection
        |
        v
Chapter Override
        |
        v
Optional Book Override
        |
        v
Project Default
        |
        v
Trusted Detection Result
        |
        v
und
```

Optional hierarchy levels MUST simply be skipped when they do not exist.

`Book` and `Page` MUST NOT be mandatory.

---

# Detection vs Configuration

Detected and configured values are not direct overwrite layers.

Example:

```text
Project declared: zh-Hans

TextBlock detected: zh-Hant
confidence: 0.97
```

The system MAY produce:

```text
effectiveLanguage: zh-Hant
resolutionSource: detected_text_block
warning: conflicts_with_project_default
```

or request user confirmation depending on policy.

It MUST NOT silently rewrite the Project declaration.

---

# Target Language

Target language defines the Translation output language.

Target language SHOULD normally be explicitly selected or resolved from explicit user/project configuration.

Possible sources:

* Project preference,
* optional Book/Chapter preference,
* reading-session preference,
* Translation Profile,
* explicit one-time request.

Automatic target-language detection SHOULD NOT be the normal Translation mechanism.

Every published Translation MUST preserve the exact target Language value used.

---

# Language Pair

A `LanguagePair` is a directional Value Object.

```text
LanguagePair
├── sourceLanguage
└── targetLanguage
```

Examples:

```text
zh-Hans -> vi
zh-Hant -> vi
ja -> vi
ko -> vi
en -> vi
```

Direction is part of identity.

Therefore:

```text
zh-Hans -> vi
```

is not equal to:

```text
vi -> zh-Hans
```

---

# Language Pair Use

LanguagePair MAY participate in:

* Translation identity,
* Translation Profile matching,
* provider capability matching,
* Glossary selection,
* cache identity,
* validation,
* metrics.

Provider fallback MUST NOT weaken the exact LanguagePair stored in Translation domain records.

---

# Language Compatibility

Compatibility and equality MUST remain separate.

Recommended match strength:

```text
EXACT
SCRIPT
BASE_LANGUAGE
RANGE
FALLBACK
NONE
```

Example:

```text
requested: zh-Hans
provider capability: zh
```

may be compatible through `BASE_LANGUAGE`.

But the requested Language remains:

```text
zh-Hans
```

---

# Fallback

Language matching MAY progressively reduce specificity.

Example:

```text
zh-Hant-TW
    |
    v
zh-Hant
    |
    v
zh
    |
    v
und
```

Fallback MAY support:

* provider matching,
* Glossary lookup,
* profile matching,
* font matching.

Fallback MUST NOT mutate persisted exact language identity.

The match result SHOULD indicate the fallback level used.

---

# Provider Language Mapping

Provider language identifiers remain adapter-level configuration.

Conceptually:

```text
Canonical Language
       |
       v
Provider Language Mapper
       |
       v
Provider Code
```

and:

```text
Provider Response Code
       |
       v
Provider Language Mapper
       |
       v
Canonical Language
```

Examples of provider-only codes might include:

```text
zh_chs
ChineseSimplified
auto
```

Such values MUST NOT escape provider boundaries as canonical domain language values.

---

# Provider Mapping Record

Provider configuration MAY define:

```text
ProviderLanguageMapping
├── providerId
├── operation
├── canonicalLanguageRange
├── providerCode
├── direction
├── mappingRevision
└── limitations
```

Limitations MAY include:

* detection only,
* OCR only,
* Translation only,
* no vertical-text support,
* no Script distinction,
* automatic Script conversion.

This configuration is NOT core immutable Language-domain truth.

---

# OCR Language Profile

OCR MAY use a language profile distinct from Translation configuration.

Example:

```text
OCRLanguageProfile
├── expectedLanguages[]
├── expectedScripts[]
├── primaryLanguage?
├── fallbackLanguages[]
├── detectionMode
└── confidenceThreshold?
```

A visual resource MAY be processed with multiple expected languages.

Individual TextBlocks SHOULD retain their own language analysis afterward.

---

# Translation Language Profile

Translation language configuration MAY include:

```text
sourceLanguageRange
targetLanguage
sourceScriptPolicy
targetScriptPolicy
mixedLanguagePolicy
transliterationPolicy
untranslatedTermPolicy
validationPolicy
```

This SHOULD integrate with Translation Profile rather than duplicate unrelated style settings.

---

# Glossary Language Scope

Glossary entries SHOULD declare language applicability.

Example:

```text
sourceRange: zh-Hans
targetRange: vi
sourceTerm: 灵力
targetTerm: linh lực
```

A Glossary entry MUST NOT automatically apply to unrelated LanguagePairs.

Exact match SHOULD be preferred over weaker fallback matches.

---

# Character Language Forms

Character names MAY have multiple language/script forms.

Conceptual model:

```text
CharacterNameForm
├── value
├── language
├── script?
├── type
└── approvalStatus?
```

Possible types include:

```text
ORIGINAL
TRANSLATED
TRANSLITERATED
ALIAS
```

Original name, Translation, alias, and romanization MUST remain distinguishable.

---

# Transliteration

Transliteration transforms writing representation without necessarily translating semantic meaning.

Examples:

* Chinese → Pinyin,
* Japanese → Romaji,
* Korean → Latin script,
* Cyrillic → Latin script.

A Transliteration result SHOULD preserve:

```text
sourceLanguage
sourceScript
targetScript
standard
result
revision
```

Transliteration MUST remain distinguishable from semantic Translation.

---

# Romanization

Romanization is Transliteration into Latin script.

Examples:

```text
Pinyin
Hepburn
Revised Romanization
Wade-Giles
```

The standard SHOULD be explicit.

Romanized output MUST NOT overwrite original or translated names automatically.

---

# Chinese Handling

CRAI SHOULD preserve explicit Chinese Script when known.

Preferred values:

```text
zh-Hans
zh-Hant
```

Region alone SHOULD NOT be treated as authoritative Script identity.

For Translation configuration, prefer:

```text
zh-Hans -> vi
zh-Hant -> vi
```

when Script is known.

---

# Simplified / Traditional Conversion

Conversion between:

```text
zh-Hans <-> zh-Hant
```

is not inherently semantic Translation.

It is primarily Script/orthographic transformation.

Some transformations may still require context.

CRAI SHOULD model Script conversion separately when that distinction matters.

---

# Japanese Handling

Japanese commonly combines:

* Kanji,
* Hiragana,
* Katakana,
* Latin characters,
* numbers.

Canonical Language can normally remain:

```text
ja
```

Mixed scripts do NOT imply mixed languages.

---

# Korean Handling

Korean may combine:

* Hangul,
* Hanja,
* Latin text,
* numbers.

Canonical language is normally:

```text
ko
```

Script information MAY still assist OCR and Presentation.

---

# Vietnamese Handling

Canonical Vietnamese language:

```text
vi
```

CRAI SHOULD preserve:

* Unicode diacritics,
* valid combining sequences,
* Vietnamese glyph coverage,
* correct punctuation spacing,
* proper-name distinctions.

Canonical Vietnamese translated text SHOULD normally use NFC normalization.

---

# Writing Direction

Writing direction and Language MUST remain distinct.

Possible values include:

```text
LTR
RTL
TTB
BTT
```

A Language MAY provide a default writing-direction hint.

That default is only fallback metadata.

TextBlock-specific direction MAY override it.

---

# Text Orientation

Text Orientation describes visual glyph arrangement.

Possible values:

```text
HORIZONTAL
VERTICAL
UPRIGHT
SIDEWAYS
ROTATED
MIXED
UNKNOWN
```

Language MUST NOT force every TextBlock into one orientation.

---

# Reading Direction

Reading Direction describes navigation/consumption order.

It is separate from:

* Language,
* Writing Direction,
* Text Orientation.

Reading Direction belongs primarily to content hierarchy and Presentation behavior.

Language MAY provide defaults only when useful.

---

# Punctuation

Language/Script metadata MAY reference punctuation profiles.

Possible profile concerns:

* quote pairs,
* brackets,
* sentence terminators,
* ellipsis,
* spacing,
* full-width punctuation,
* vertical punctuation.

Language identity alone MUST NOT automatically mutate punctuation.

Punctuation changes occur through explicit normalization or Translation policy.

---

# Unicode Normalization

Text-processing stages SHOULD use an explicit Unicode normalization policy.

Possible values:

```text
NFC
NFD
NFKC
NFKD
PRESERVE
```

Recommended canonical text default:

```text
NFC
```

Compatibility normalization such as NFKC MUST be applied deliberately because it can alter distinctions.

---

# Word Segmentation

Word segmentation is language-sensitive.

CRAI MUST NOT assume whitespace splitting works universally.

Examples:

* Chinese normally does not use spaces between words.
* Japanese combines several scripts.
* Thai commonly omits word spaces.
* Vietnamese whitespace does not always correspond to lexical units.

Segmentation execution belongs to a language-processing capability.

The domain MAY preserve segmentation profile/revision metadata.

---

# Sentence Segmentation

Sentence boundaries are derived linguistic structures.

Segmentation MAY support:

* Translation grouping,
* context building,
* novel processing,
* quality evaluation.

Derived sentences MUST preserve source mapping.

They MUST NOT replace original TextBlock content.

---

# Language Capability

CRAI MAY maintain runtime capability metadata such as:

```text
LanguageCapability
├── languageRange
├── operation
├── supportLevel
├── limitations
└── source
```

Possible support levels:

```text
SUPPORTED
EXPERIMENTAL
PARTIAL
UNSUPPORTED
PROVIDER_DEPENDENT
LOCAL_ONLY
CLOUD_ONLY
```

Capability MUST be evaluated per operation.

A Language may be supported for Translation but not OCR.

---

# Capability Is Runtime Knowledge

Language capability MUST NOT be treated as immutable domain truth.

Support can change when:

* providers change,
* models change,
* local runtimes improve,
* configuration changes.

Therefore static examples in documentation are illustrative only.

Runtime/provider architecture remains authoritative for actual support.

---

# Language Registry

A Language Registry MAY provide:

* canonicalization,
* validation,
* display metadata,
* native names,
* Script metadata,
* default-direction hints,
* parent fallback,
* deprecation metadata.

Business records SHOULD persist canonical tags.

They SHOULD NOT depend solely on database-specific numeric language IDs.

---

# Language Change Impact

Language changes MAY invalidate downstream artifacts.

Possible impact examples:

```text
display name change
    -> NONE

detection confidence change
    -> usually NONE

source language correction
    -> Translation may become STALE

Script correction
    -> OCR / Translation may become STALE

target language change
    -> new Translation identity

writing-direction override
    -> Presentation may become STALE
```

Dependency impact MUST be explicit.

A Language change MUST NOT indiscriminately invalidate all project data.

---

# Revision Model

Language-related registries and mappings MAY be versioned.

Examples:

```text
LanguageRegistryRevision
ProviderMappingRevision
DetectionResultRevision
LanguageResolutionRevision
PunctuationProfileRevision
NormalizationProfileRevision
SegmentationProfileRevision
```

Canonical Language tags themselves SHOULD remain stable.

Historical Translation records MUST preserve the exact Language values used at creation.

---

# Validation

Language validation SHOULD check:

* language-tag syntax,
* recognized base language or permitted private use,
* valid Script,
* valid Region,
* valid Variant structure,
* canonical casing,
* absence of provider-only codes,
* operation compatibility where required,
* valid LanguageRanges,
* valid LanguagePairs,
* valid span boundaries.

Invalid canonical values MUST NOT enter durable domain state.

---

# Private-Use Tags

Private-use tags MAY be supported for controlled experiments.

They SHOULD:

* follow valid syntax,
* be documented,
* remain isolated from ordinary provider mapping,
* not replace standard tags,
* include migration rules.

Private-use values SHOULD remain exceptional.

---

# Persistence

Recommended Language Value representation:

```text
Language
├── canonicalTag
├── baseLanguage
├── script?
├── region?
└── variants[]
```

Detection metadata SHOULD be stored separately:

```text
LanguageDetection
├── scopeType
├── scopeId
├── candidates
├── confidence
├── detector
├── inputHash
└── revision
```

Resolution SHOULD also remain explicit when persistence is required:

```text
LanguageResolution
├── scope
├── operation
├── effectiveLanguage
├── source
├── warnings
└── revision
```

Provider mapping belongs in runtime/provider configuration.

---

# Cache Participation

Language-significant state MAY participate in cache identity.

OCR cache MAY consider:

```text
expectedLanguageRange
expectedScripts
OCRLanguageProfileRevision
```

Translation cache SHOULD consider:

```text
sourceLanguage
targetLanguage
Script policy
Translation-language configuration revision
```

Presentation/rendering cache MAY consider:

```text
targetLanguage
Script
writingDirection
textOrientation
lineBreakProfile
fontProfile
```

Only affected caches SHOULD be invalidated when Language-significant configuration changes.

---

# Privacy

Language metadata is usually lower sensitivity than source content, but MAY still reveal user interests or content origins.

Rules SHOULD include:

* do not infer nationality from content Language,
* do not infer ethnicity from Language selection,
* do not use Language as authorization evidence,
* avoid logging raw content during detection,
* send only necessary samples to remote detectors,
* respect local-processing preferences,
* isolate Project-scoped preferences.

Language detection is technical classification, not personal identity inference.

---

# Events

Language Value Objects themselves do not emit events.

Events arise when scoped language-related domain state changes.

Possible events include:

```text
SourceLanguageConfigured
TargetLanguageConfigured
LanguageDetected
LanguageDetectionFailed
LanguageConfirmed
LanguageCorrected
LanguageResolutionChanged
MixedLanguageDetected
ScriptDetected
```

Provider/runtime configuration MAY separately emit:

```text
ProviderLanguageMappingChanged
LanguageCapabilityChanged
```

Events SHOULD preserve:

* scope,
* previous value,
* new value,
* resolution source,
* confidence when relevant,
* actor,
* revision,
* correlation identity.

Raw source text SHOULD NOT be included by default.

---

# Errors

Stable domain errors MAY include:

```text
LANGUAGE_TAG_INVALID
LANGUAGE_CODE_UNKNOWN
LANGUAGE_SCRIPT_INVALID
LANGUAGE_REGION_INVALID
LANGUAGE_RANGE_INVALID
LANGUAGE_PAIR_INVALID
LANGUAGE_SOURCE_UNSUPPORTED
LANGUAGE_TARGET_UNSUPPORTED
LANGUAGE_PAIR_UNSUPPORTED
LANGUAGE_MAPPING_MISSING
LANGUAGE_MAPPING_AMBIGUOUS
LANGUAGE_DETECTION_FAILED
LANGUAGE_MIXED_NOT_ALLOWED
LANGUAGE_SCRIPT_MISMATCH
LANGUAGE_SPAN_INVALID
LANGUAGE_CONFIRMED_OVERRIDE_CONFLICT
TRANSLITERATION_STANDARD_UNSUPPORTED
```

Provider-specific failures MUST be translated at provider/module boundaries.

---

# Architecture Invariants

1. Language identity uses a canonical normalized language tag.

2. Provider-specific language codes MUST NOT escape provider boundaries as canonical values.

3. Language, Script, Region, Writing Direction, Text Orientation, Reading Direction, and Locale remain distinct concepts.

4. Display names MUST NOT be canonical identifiers.

5. Configured, detected, confirmed, and effective Language values remain distinguishable.

6. User-confirmed Language has higher authority than automatic detection at the same scope.

7. Language equality requires equal normalized canonical tags.

8. Compatibility MUST NOT be confused with equality.

9. Target Language MUST be explicit before Translation publication.

10. LanguagePair is directional.

11. Reverse LanguagePair is a different value.

12. TextBlock Language MAY be more specific than parent content defaults.

13. Page MUST NOT be required for Language representation or resolution.

14. Book MUST NOT be required for Language representation or resolution.

15. Mixed-language content preserves primary/secondary or span information when required.

16. LanguageSpan offsets MUST use a stable documented indexing model.

17. Script conversion is distinct from semantic Translation.

18. Transliteration is distinct from semantic Translation.

19. UI localization is distinct from content Language.

20. Language MUST NOT directly determine Reading Direction.

21. Language MUST NOT directly select concrete font assets.

22. Text normalization policy MUST be explicit.

23. Language changes invalidate only dependent artifacts.

24. Historical Translation revisions preserve exact original Language identities.

25. Fallback matching MUST NOT mutate stored exact Language.

26. Language-sensitive cache identity includes relevant Language configuration.

27. Detection confidence is evidence, not guaranteed correctness.

28. Cross-Project Language preferences MUST NOT leak implicitly.

29. Language detection MUST NOT be used as a personal-identity claim.

30. Invalid canonical Language values MUST NOT enter durable state.

31. Language resolution MUST be operation-specific when consumer semantics differ.

32. Optional hierarchy levels MUST be skipped rather than synthesized.

33. Runtime Language capability MUST NOT be treated as immutable core-domain truth.

---

# Comic Example

```text
Project:
    declared source: zh-Hans
    target: vi

Page:
    detection suggests zh-Hans

TextBlocks:
    Block A -> zh-Hans
    Block B -> zh-Hans
    Block C -> en
```

Translation resolution:

```text
Block A:
    zh-Hans -> vi

Block B:
    zh-Hans -> vi

Block C:
    en -> vi
```

The Page MAY expose a primary language summary.

Each TextBlock preserves its own effective source Language.

---

# Text-Native Novel Example

```text
Project default:
    zh-Hans

Chapter:
    declared zh-Hans

Paragraph TextBlock:
    detected zh-Hans
```

Resolution:

```text
TextBlock effective source:
    zh-Hans

Translation:
    zh-Hans -> vi
```

No Page exists or is required.

---

# Conflict Example

```text
Project declared:
    zh-Hans

TextBlock detected:
    zh-Hant
    confidence: high
```

CRAI SHOULD preserve both values.

Policy MAY:

* use detected `zh-Hant`,
* request user confirmation,
* warn about the conflict.

It MUST NOT silently rewrite the Project default.

---

# Mixed-Language Example

Source:

```text
快使用 Ultimate Skill！
```

Possible LanguageAnalysis:

```text
primaryLanguage: zh-Hans

secondaryLanguages:
    - en

spans:
    zh-Hans -> 快使用
    en      -> Ultimate Skill
```

Translation policy MAY:

* translate the English term,
* preserve it,
* use Glossary terminology,
* transliterate it,
* display bilingual output.

That behavior belongs to Translation policy, not Language identity.

---

# Recommended MVP Scope

Initial CRAI Language support SHOULD include:

* canonical BCP 47-compatible tags,
* `zh-Hans`,
* `zh-Hant`,
* `en`,
* `vi`,
* optional `ja`,
* optional `ko`,
* explicit target-language selection,
* Project source default,
* Chapter/TextBlock overrides,
* optional Page language metadata,
* configured vs detected distinction,
* user confirmation,
* basic detection metadata,
* basic Script metadata,
* Chinese + English mixed-content handling,
* provider-language mapping,
* directional LanguagePair,
* Language-aware Translation cache,
* NFC Vietnamese output,
* horizontal/vertical orientation metadata.

MVP MAY defer:

* detailed LanguageSpans,
* automatic regional-variant detection,
* advanced Transliteration,
* Simplified/Traditional conversion,
* dynamic provider-capability discovery,
* complex Locale formatting,
* private-use tags,
* user-defined registries,
* advanced linguistic segmentation,
* dialect modeling.

---

# Open Decisions

The following SHOULD remain open until implementation/prototype validation:

* whether only canonical tags or parsed components are persisted,
* Language Registry implementation,
* when automatic detection runs,
* confidence threshold for confirmation,
* mixed-language detection granularity,
* Chinese Script detection strategy,
* Simplified/Traditional conversion support,
* handling of `zh-CN` versus explicit `zh-Hans`,
* weak provider Script mappings,
* supported romanization standards,
* Transliteration persistence model,
* LanguageSpan indexing representation,
* normalization policy per processing stage,
* Vietnamese line-breaking validation,
* font fallback strategy,
* runtime capability-discovery model,
* automatic retranslation after Language correction,
* short-text detection behavior,
* punctuation span inheritance.

---

# Related Documents

Domain:

* `README.md`
* `PROJECT.md`
* `BOOK.md`
* `CHAPTER.md`
* `PAGE.md`
* `TEXT_BLOCK.md`
* `TRANSLATION.md`
* `GLOSSARY.md`
* `CHARACTER.md`
* `PROFILE.md`
* `SESSION.md`

Architecture:

* `docs/architecture/CAPABILITY_MAP.md`
* `docs/architecture/OWNERSHIP_MAP.md`
* `docs/architecture/DATA_FLOW.md`
* `docs/architecture/STATE_MACHINE.md`
* `docs/architecture/EVENT_BUS.md`

AI / Translation:

* `docs/architecture/ai/PIPELINE.md`
* `docs/architecture/ai/CONTEXT.md`
* `docs/architecture/ai/PROMPTS.md`
* `docs/architecture/ai/MODELS.md`
* `docs/architecture/ai/ROUTING.md`
* `docs/architecture/ai/RESPONSE.md`

Presentation:

* `docs/architecture/presentation/FONTS.md`
* `docs/architecture/presentation/LAYOUT.md`
* `docs/architecture/presentation/TYPOGRAPHY.md`

Module contracts remain authoritative for runtime/provider capability and execution behavior.
