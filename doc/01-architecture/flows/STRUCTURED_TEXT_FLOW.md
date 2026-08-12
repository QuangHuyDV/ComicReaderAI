# CRAI Structured Text Flow

> **Project:** CRAI
> **Path:** `doc/01-architecture/flows/STRUCTURED_TEXT_FLOW.md`
> **Version:** 1.0.0
> **Status:** Architecture Draft
> **Runtime Model:** Runtime v2 aligned
> **Owner:** CRAI Architecture
> **Last Updated:** 2026-08-10

---

# 1. Purpose

This document defines the end-to-end CRAI flow for reading and translating structured text sources.

Typical sources include:

```text
web novels

browser-readable text

DOM-extracted article/chapter content

clipboard text

plain-text files

structured document text

future text-native integrations
```

The defining characteristic is:

```text
usable source text already exists
```

Therefore the normal structured-text path does not require:

```text
screen capture
OCR
text detection
visual reading-order reconstruction
```

---

# 2. Why This Flow Exists

CRAI supports fundamentally different source paths.

Image-oriented reading:

```text
Visual Source
    ↓
Capture
    ↓
Recognition
    ↓
Text Processing
```

Structured-text reading:

```text
Structured Text Source
    ↓
Text Processing
```

Both may later converge at:

```text
SourceDocumentArtifact
    ↓
Translation
    ↓
Presentation
```

Without this distinction, CRAI could incorrectly force every reading source through OCR.

---

# 3. Central Rule

The primary rule is:

```text
If trustworthy structured text
is already available,

do not reconstruct that text
through OCR unnecessarily.
```

Capture and Recognition are capabilities used when required by the source.

They are not mandatory stages of every CRAI reading flow.

---

# 4. Scope

This flow covers:

```text
structured-source acquisition

source normalization

semantic text reconstruction

SourceDocumentArtifact creation

Translation

Presentation

Runtime execution

content change

supersession

cache reuse

reader UI projection

continuous structured reading
```

---

# 5. Out of Scope

This document does not define:

```text
DOM selector implementation

browser-extension implementation

HTML parser implementation

document-format parser implementation

Translation provider API

storage schema

frontend framework

exact Text Processing algorithms

exact Runtime scheduling algorithm
```

Those remain with their respective owners.

---

# 6. Main Participants

The flow may involve:

```text
User

UI Adapter

Application

Reading Session

Preferences

Structured Source Adapter

Text Processing

Translation

Presentation

Runtime

Diagnostics
```

Supporting mechanisms may include:

```text
Business Pipeline Orchestration

Event Bus

Scheduler

Resource Manager

Storage

Cache

Logging

Telemetry

Platform Adapters
```

---

# 7. Modules Normally Not Required

The normal structured-text path does not require:

```text
Capture

Recognition
```

because text already exists in machine-readable form.

They may still participate in exceptional hybrid scenarios.

---

# 8. Architecture Authorities

| Concern                              | Owner                   |
| ------------------------------------ | ----------------------- |
| Reading activity                     | Reading Session         |
| ReadingContext                       | Reading Session         |
| ReadingContextRevision               | Reading Session         |
| Persistent preferences               | Preferences             |
| Source/platform extraction mechanism | Platform/Source Adapter |
| Normalized source-document semantics | Text Processing         |
| Translation semantics                | Translation             |
| Presentation semantics               | Presentation            |
| Execution authority                  | Runtime                 |
| Cross-module coordination            | Application             |
| ViewModel                            | UI Adapter              |

---

# 9. High-Level User Flow

```text
Launch CRAI
    ↓
Start Structured Reading
    ↓
Select / Open Text Source
    ↓
Create or Update Reading Session
    ↓
Acquire Structured Content
    ↓
Build SourceDocumentArtifact
    ↓
Translate
    ↓
Build PresentationArtifact
    ↓
Project Reader UI
    ↓
Continue Reading
```

---

# 10. High-Level Semantic Flow

Canonical structured-text semantic path:

```text
Structured Source Snapshot
    ↓
Text Processing
    ↓
SourceDocumentArtifact
    ↓
Translation
    ↓
TranslationArtifact
    ↓
Presentation
    ↓
PresentationArtifact
```

UI Adapter then projects the PresentationArtifact into a ViewModel.

---

# 11. Comparison With Screen Comic Flow

Screen comic:

```text
Screen
    ↓
CaptureArtifact
    ↓
RecognitionArtifact
    ↓
SourceDocumentArtifact
    ↓
TranslationArtifact
    ↓
PresentationArtifact
```

Structured text:

```text
Structured Text
    ↓
SourceDocumentArtifact
    ↓
TranslationArtifact
    ↓
PresentationArtifact
```

The paths converge at:

```text
SourceDocumentArtifact
```

---

# 12. Why SourceDocumentArtifact Is the Convergence Boundary

Translation should not need to know whether text originated from:

```text
OCR

DOM

clipboard

plain text

document parser
```

Translation consumes the normalized semantic source-document contract.

Therefore:

```text
SourceDocumentArtifact
```

is the preferred architecture boundary between source interpretation and Translation.

---

# 13. Session Creation

User initiates structured reading.

Conceptually:

```text
User
    ↓
UI Adapter
    ↓
Application
    ↓
Reading Session
    ↓
SessionId
```

Session creation alone does not begin Translation.

A valid source/context is required.

---

# 14. Source Selection

Possible structured sources:

```text
browser page

web novel chapter

selected DOM region

clipboard text

text file

structured document

future provider-native text source
```

The native source reference is normalized before entering semantic processing.

---

# 15. ReadingContext

Reading Session may commit context such as:

```text
source identity

source type

source locator

source language

target language

reading mode

session-specific overrides
```

Exact schema remains Reading Session-owned.

---

# 16. ReadingContextRevision

A semantic context change may create:

```text
ReadingContextRevision N+1
```

Examples:

```text
different novel/site/source

different selected text region

language configuration changed

source mode changed
```

---

# 17. Content Change Is Still Separate

A chapter's visible/loaded content may change while:

```text
ReadingContextRevision
```

remains unchanged.

Example:

```text
same novel
same browser source
same reading configuration

user scrolls to later paragraphs
```

This may require new processing without changing ReadingContext.

---

# 18. Structured Source Adapter

Platform/source adapters obtain text from native sources.

Examples:

```text
browser DOM adapter

clipboard adapter

file parser adapter

document adapter
```

They isolate source-native APIs from semantic modules.

---

# 19. Adapter Responsibility

A source adapter may provide:

```text
native text

element hierarchy

source geometry

source locator

language hints

document metadata

source ordering information
```

It must not silently become owner of CRAI semantic document meaning.

---

# 20. Provider/Platform DTO Isolation

Native objects must not cross semantic module boundaries.

Examples that should remain inside adapters:

```text
DOM Node

HTMLElement

browser tab object

clipboard SDK object

parser library AST

native file handle
```

They are converted into platform-neutral contracts.

---

# 21. Structured Source Snapshot

The adapter produces a platform-neutral snapshot suitable for semantic processing.

Conceptually:

```text
StructuredSourceSnapshot
├── source identity
├── text blocks
├── structural hints
├── ordering hints
├── source locators
└── safe metadata
```

The exact contract must be owned by the relevant source/Text Processing boundary.

This document does not establish a new top-level module merely for the snapshot.

---

# 22. Snapshot Is Immutable Input

Once accepted for a processing revision:

```text
StructuredSourceSnapshot
```

should be treated as immutable input.

Native source changes create a new snapshot rather than mutating an in-flight one.

---

# 23. Source Acquisition

Conceptually:

```text
Native Source
    ↓
Source Adapter
    ↓
StructuredSourceSnapshot Candidate
    ↓
validation
    ↓
accepted structured input
```

Application/business orchestration determines whether semantic processing is required.

---

# 24. No OCR by Default

Forbidden normal path:

```text
DOM text
    ↓
render screenshot
    ↓
OCR screenshot
    ↓
recover same text
```

This introduces unnecessary:

```text
latency

OCR errors

CPU/GPU cost

provider cost

layout ambiguity
```

---

# 25. Text Processing

Text Processing receives structured source input and constructs normalized semantic source content.

Possible responsibilities:

```text
text normalization

whitespace normalization

paragraph reconstruction

semantic block grouping

source-order normalization

noise removal

metadata normalization

source-document construction
```

---

# 26. Text Processing Does Not Translate

Text Processing must not perform:

```text
language translation

provider selection for Translation

Translation retry

TranslationUnit execution
```

Those belong to Translation/Runtime.

---

# 27. Text Processing Does Not Own TranslationUnit

The output boundary is:

```text
SourceDocumentArtifact
```

Translation consumes that Artifact and derives Translation-specific units.

---

# 28. SourceDocumentArtifact

Text Processing produces:

```text
SourceDocumentArtifact Candidate
```

Possible semantic information includes:

```text
normalized source text

ordered source blocks

paragraph relationships

source locators

semantic structure

source-language hints

provenance
```

Exact schema belongs to Text Processing.

---

# 29. Candidate → Published

Text Processing does not immediately mutate current global state.

Conceptually:

```text
SourceDocumentArtifact Candidate
    ↓
semantic validation
    ↓
authority validation
    ↓
Published SourceDocumentArtifact
```

---

# 30. Runtime Authority

Execution remains Runtime-owned.

Conceptually:

```text
Application
    ↓
BusinessExecutionPlan
    ↓
RuntimeRevision
    ↓
WorkItems
    ↓
Attempts
```

Structured-text reading does not bypass Runtime simply because OCR is absent.

---

# 31. Structured Text Execution Graph

Typical graph:

```text
Acquire Structured Source
        ↓
Text Processing
        ↓
Translation
        ↓
Presentation
```

Depending on architecture placement, source acquisition itself may be an Application/source-adapter operation or Runtime-managed WorkItem.

The exact boundary remains an implementation/design decision.

---

# 32. Semantic Graph vs Call Graph

The semantic relationship:

```text
Text Processing
    ↓
Translation
    ↓
Presentation
```

does not mean:

```text
Text Processing calls Translation

Translation calls Presentation
```

Runtime/business orchestration determines executable dependencies.

---

# 33. Translation Input

Translation consumes:

```text
Published SourceDocumentArtifact
```

It does not consume:

```text
HTMLElement

raw browser DOM

parser-native AST

clipboard-native object
```

---

# 34. TranslationUnit Construction

Translation derives:

```text
TranslationUnit
```

from SourceDocumentArtifact.

Possible units include:

```text
paragraph

sentence group

dialogue block

semantic section

bounded context window
```

Exact unit strategy belongs to Translation.

---

# 35. Novel Translation Context

For novels, Translation may require context beyond one isolated paragraph.

Possible context:

```text
neighbor paragraphs

character names

speaker information

glossary

previous translated units

chapter metadata

style profile
```

Translation owns how this context is represented for Translation semantics.

---

# 36. TranslationBatch

Translation may group TranslationUnits into:

```text
TranslationBatch
```

to balance:

```text
context quality

provider limits

latency

cost

parallelism
```

Batching remains Translation-owned.

---

# 37. Translation Execution

Runtime executes Translation work.

Conceptually:

```text
Translation WorkItem
    ↓
Attempt T1
    ↓
TranslationProviderPort
```

Provider may be:

```text
local

remote

hybrid
```

---

# 38. Translation Candidate

Successful Translation execution produces:

```text
TranslationArtifact Candidate
```

Possible content:

```text
translated text

source-target mapping

TranslationUnit relationships

provenance

warnings

quality/completeness information
```

---

# 39. Translation Publication

Before publication:

```text
semantic validity
+
current Runtime authority
+
input provenance compatibility
```

must be satisfied.

Late Translation results for obsolete content cannot replace newer current output.

---

# 40. Presentation

Presentation consumes:

```text
Published TranslationArtifact
```

and constructs platform-neutral reading presentation.

---

# 41. Structured Text Presentation

Compared with comic presentation, structured text often emphasizes:

```text
paragraph layout

line wrapping

font sizing

line spacing

source/translation arrangement

chapter structure

reading width

text alignment
```

Presentation owns semantic presentation decisions.

---

# 42. PresentationArtifact

Presentation produces:

```text
PresentationArtifact Candidate
```

Possible content:

```text
ordered presentation units

paragraph grouping

source/translation relationships

semantic typography hints

interaction references

navigation anchors
```

It does not contain native UI controls.

---

# 43. UI Adapter

UI Adapter converts Presentation state into:

```text
ViewModel
```

for a concrete frontend.

Possible frontends:

```text
desktop reader

side panel

browser extension

overlay

future mobile frontend
```

---

# 44. Typography Boundary

Presentation may define semantic typography intent such as:

```text
heading

body text

annotation

emphasis

source text

translated text
```

UI Adapter/frontend determines native widget/font rendering.

---

# 45. Font Preferences

Persistent font preferences belong to:

```text
Preferences
```

Presentation may consume resolved presentation configuration.

It does not persistently own the user's font settings.

---

# 46. Effective Presentation Configuration

Application may resolve:

```text
Preferences
+
session overrides
+
presentation mode
+
frontend capability
```

into contextual configuration.

No single global mutable configuration object should become cross-module authority.

---

# 47. Continuous Structured Reading

A web novel may load or reveal content continuously.

Example:

```text
paragraphs 1–10 visible
    ↓
user scrolls
    ↓
paragraphs 8–18 relevant
```

CRAI should identify new useful content rather than retranslating everything blindly.

---

# 48. Structured Content Change

Conceptually:

```text
Structured Source Snapshot A
    ↓
new source mutation / viewport
    ↓
Structured Source Snapshot B
```

Application/Runtime determines whether B requires new processing.

---

# 49. Relationship to Content Change Flow

Structured sources follow:

```text
new useful content
    ↓
current execution authority
    ↓
RuntimeRevision
    ↓
supersession of obsolete work
```

as defined in:

```text
CONTENT_CHANGE_FLOW.md
```

---

# 50. DOM Mutation Is Not Automatically Semantic Change

Web pages frequently mutate unrelated content.

Examples:

```text
advertisement rotation

navigation counters

comments

recommendation widgets

analytics markup

style/class changes

loading indicators
```

Source isolation should prevent these from triggering unnecessary Translation.

---

# 51. Semantic Source Region

For browser reading, CRAI may identify a source region such as:

```text
chapter body

article body

selected container

reader content area
```

rather than treating the entire DOM as reading content.

---

# 52. Source Region Selection

Selection may be:

```text
automatic

user-assisted

site-profile driven

manual
```

Exact detection strategy remains open.

---

# 53. Automatic Detection

Future source adapters may infer likely readable content from:

```text
DOM structure

text density

semantic tags

repeated site patterns

reader mode metadata
```

This is a source-adapter capability.

It does not change downstream Artifact contracts.

---

# 54. Site Profiles

Known sites may eventually use:

```text
SourceProfile
```

containing adapter-specific extraction hints.

A profile must not leak site-specific selectors into Translation or Presentation.

---

# 55. Source Profile Failure

If a site changes structure:

```text
profile extraction fails
```

possible recovery includes:

```text
generic extraction

user region selection

manual text selection

source-adapter fallback
```

Reading Session may remain active.

---

# 56. Source Noise Removal

Text Processing/source adapters may need to remove:

```text
navigation

advertisements

chapter controls

author notes

recommendations

comment sections
```

The exact ownership boundary depends on whether the operation is:

```text
source-native extraction
```

or:

```text
semantic document normalization
```

---

# 57. Extraction vs Text Processing

Preferred distinction:

```text
Source Adapter
    → what source-native content was extracted

Text Processing
    → what that content means as normalized CRAI source text
```

This keeps browser/site knowledge outside semantic modules.

---

# 58. Reading Order

For structured text, source order may already be explicit.

Text Processing should preserve trustworthy ordering information where possible.

Do not unnecessarily reconstruct reading order visually.

---

# 59. Mixed Direction Text

Structured content may contain:

```text
Chinese

Japanese

Latin text

numbers

inline code

mixed-direction fragments
```

Text Processing/Presentation should preserve semantic ordering without assuming every source is left-to-right.

---

# 60. Ruby / Furigana / Annotations

Structured sources may expose annotations such as:

```text
ruby text

furigana

footnotes

inline explanations
```

The architecture should preserve such structure when useful rather than flattening it prematurely.

Exact Artifact representation remains a Text Processing design decision.

---

# 61. Chapter Structure

A SourceDocumentArtifact may preserve:

```text
chapter title

section headings

paragraph boundaries

scene breaks

annotations
```

when present.

Translation should not need to rediscover them from plain concatenated text.

---

# 62. Incremental Translation

Long chapters should not require:

```text
translate entire chapter
before showing anything
```

The architecture may process bounded current/relevant units.

However, incremental processing must preserve coherent Artifact/publication semantics.

---

# 63. Incremental Does Not Mean Arbitrary Partial Mutation

Avoid:

```text
one global TranslationArtifact
mutated paragraph by paragraph
by concurrent Attempts
```

Prefer explicit immutable units/revisions/publication semantics.

Exact partial Artifact model remains an open design decision.

---

# 64. Visible-Range Translation

One possible policy:

```text
visible paragraphs
+
small context window
    ↓
high priority
```

while:

```text
near-future paragraphs
    ↓
prefetch priority
```

This is Runtime/Application policy.

---

# 65. Prefetch

Structured text is particularly suitable for prefetch.

Example:

```text
currently reading paragraph 20
    ↓
translate 21–25 opportunistically
```

Prefetch must not interfere with current user-visible work.

---

# 66. Prefetch Authority

Prefetch is speculative work.

It may produce reusable Artifacts.

It must not automatically become:

```text
current Presentation
```

until current reading authority requires it.

---

# 67. Prefetch Supersession

If user jumps elsewhere:

```text
prefetch work
```

may be:

```text
cancelled

deprioritized

allowed to finish for cache
```

according to Runtime policy.

---

# 68. Backpressure

Long structured documents may contain thousands of paragraphs.

CRAI must not create unbounded WorkItems for the whole document automatically.

Use:

```text
bounded current window

bounded prefetch window

bounded Translation batches

bounded concurrency
```

---

# 69. User Jump

Example:

```text
paragraph 20
    ↓
user jumps to paragraph 500
```

Runtime should prioritize paragraph 500-related work.

Pending paragraph 21–25 prefetch should not block current content.

---

# 70. Chapter Change

If user moves:

```text
Chapter 10
    ↓
Chapter 11
```

whether this creates a new ReadingContextRevision depends on Reading Session source semantics.

It always may require new execution/content authority.

---

# 71. Same Source vs New Source

Possible model:

```text
Novel A
    ├── Chapter 10
    └── Chapter 11
```

may remain one source with changing reading position.

Or each chapter may be represented as a source locator.

Reading Session contracts must decide.

This flow does not freeze that decision.

---

# 72. Reading Position

Structured reading may maintain:

```text
chapter

paragraph

viewport anchor

source locator
```

Reading Session may own reading-position semantics.

Translation does not own reading position.

---

# 73. Position and Runtime

Changing reading position may create new execution needs without necessarily changing ReadingContextRevision.

Example:

```text
same chapter
same configuration
scroll down
```

Runtime may simply process the newly relevant content window.

---

# 74. Duplicate Paragraphs

Web novels may repeat:

```text
chapter title

navigation text

author signature

site footer
```

Text Processing/source extraction should avoid treating repeated site chrome as novel content.

---

# 75. Cache

Structured text provides strong cache opportunities.

Possible cache keys may incorporate:

```text
normalized source text

source identity

language pair

Translation configuration

glossary/context version

provider/model profile

schema version
```

Exact cache-key policy belongs elsewhere.

---

# 76. Text Hash

Normalized text hashes can assist:

```text
duplicate detection

cache lookup

change detection
```

but hashes are not semantic authority.

---

# 77. Translation Cache

If the same normalized TranslationUnit appears with compatible context:

```text
cached translation
```

may be reusable.

Context-sensitive translation may require stronger compatibility checks than exact source text alone.

---

# 78. Context-Sensitive Cache Risk

The same sentence:

```text
"他回来了。"
```

may translate differently depending on:

```text
character identity

gender/context

previous paragraph

glossary

style
```

Therefore:

```text
source-text hash alone
```

may be insufficient for Translation cache correctness.

---

# 79. Glossary Change

If glossary/knowledge changes:

```text
Translation compatibility
```

may change even though SourceDocumentArtifact remains identical.

Application/Translation cache policy must account for this.

---

# 80. Preference Change

If Translation style changes:

```text
literal
    ↓
natural
```

existing TranslationArtifact may no longer satisfy current requirements.

A new Runtime execution revision may be required.

---

# 81. Font Change

Changing only font size normally should not require:

```text
Text Processing

Translation
```

It may require only:

```text
Presentation/UI reprojection
```

depending on Presentation contracts.

---

# 82. Theme Change

Theme changes should normally remain:

```text
Preferences
    ↓
UI Adapter / Presentation configuration
```

without retranslating content.

---

# 83. Translation Style Change

A semantic Translation style change may require:

```text
new Translation execution
```

because the translated semantic output itself changes.

---

# 84. Source Language Detection

If source language is automatic:

```text
language detection
```

may occur during source/Text Processing capability handling.

Exact owner should follow the architecture's language-detection contract.

Do not make Translation silently own source-language detection by default.

---

# 85. Mixed-Language Documents

A structured document may contain:

```text
Chinese prose

English names

Japanese quotes

code snippets
```

Translation should receive sufficient structure to avoid translating inappropriate content blindly.

---

# 86. Translatable vs Non-Translatable Content

Text Processing may classify semantic blocks or provide hints.

Translation determines Translation-specific eligibility according to its contract.

Avoid embedding provider-specific translation decisions inside source adapters.

---

# 87. User Selection Translation

User may select only one paragraph or passage.

Conceptually:

```text
current SourceDocumentArtifact
    ↓
selected semantic range
    ↓
Application
    ↓
Translation requirement
```

There is no need to rebuild the whole source from OCR.

---

# 88. Retranslation

User may request:

```text
retranslate current paragraph

alternate wording

different provider

different style
```

Application determines the semantic request.

Runtime executes new work.

---

# 89. Retranslation Does Not Mutate Old Attempt

New Translation execution creates:

```text
new Attempt
```

and potentially a new TranslationArtifact.

Old Attempts remain historical execution records.

---

# 90. Source Changes During Translation

Example:

```text
Paragraph A Translation running
    ↓
user jumps to Paragraph B
```

Runtime may supersede/deprioritize A for current-display authority.

If A finishes late:

```text
it must not replace B
```

---

# 91. Late Result Protection

Conceptually:

```text
Translation Candidate A
    ↓
current authority still A?
    ├── yes → publish/use
    └── no  → non-current
```

Cancellation is not required for correctness.

---

# 92. Presentation During New Translation

While new text is translating:

```text
previous valid PresentationArtifact
```

may remain visible.

UI policy may additionally show:

```text
loading indicator

pending marker

current-source highlight
```

without changing Presentation ownership.

---

# 93. Atomic Presentation

Avoid mixing:

```text
source paragraph B

translation paragraph A

presentation ordering C
```

as one current semantic projection.

Provenance compatibility must be preserved.

---

# 94. Progressive Translation

Future structured reading may benefit from progressive publication.

Example:

```text
paragraph 1 translated
paragraph 2 translating
paragraph 3 pending
```

This requires explicit partial Artifact semantics.

Provider streaming alone does not establish such semantics.

---

# 95. Streaming Provider

A Translation provider may stream tokens.

Those tokens are:

```text
provider execution output
```

not automatically Published TranslationArtifact state.

Translation must define how streaming output becomes semantic Candidates/Publications.

---

# 96. Error Handling

Module errors retain their owner.

Examples:

```text
TXT-*
TRN-*
PRES-*
RUN-*
SES-*
```

Source-adapter/platform failures remain with the corresponding adapter/platform error domain.

---

# 97. Source Extraction Failure

Possible causes:

```text
DOM unavailable

browser permission denied

site structure changed

clipboard unavailable

file parser failure
```

Application may:

```text
retry

fallback

request user selection

switch source mode

surface user action
```

according to policy.

---

# 98. Text Processing Failure

A malformed structured source may fail normalization.

Possible response:

```text
degraded SourceDocumentArtifact

retry with alternate normalization

request different source region

terminal current-content failure
```

according to Text Processing contracts.

---

# 99. Translation Failure

Translation failure may lead to:

```text
retry

provider fallback

partial/degraded result

user action

terminal WorkItem failure
```

according to Translation + Runtime policy.

Reading Session may remain ACTIVE.

---

# 100. Presentation Failure

Presentation failure must not corrupt the last valid PresentationArtifact.

Previous current presentation may remain usable.

---

# 101. Source Loss

If browser tab/file/source disappears:

```text
source unavailable
```

Application and Reading Session determine whether to:

```text
pause

request another source

recover

stop
```

This is not merely a Translation failure.

---

# 102. Browser Navigation

Navigation may produce:

```text
same source with new locator
```

or:

```text
entirely new reading source
```

Reading Session/source adapter determines the semantic distinction.

---

# 103. SPA Navigation

Single-page applications may change chapter content without full browser navigation.

Therefore:

```text
URL change
```

cannot be the only content-change signal.

---

# 104. URL Is Not Content Identity

The same URL may display:

```text
different chapter content

dynamically loaded paragraphs

user-specific reader state
```

while different URLs may contain identical text.

Use source identity + semantic content/provenance appropriately.

---

# 105. Privacy

Structured source content may be sensitive.

Examples:

```text
private documents

logged-in novel sites

clipboard text

personal notes
```

Only required data should be sent to remote providers.

---

# 106. Browser Privacy

Do not send unrelated page content when only the chapter body is required.

Prefer:

```text
selected semantic text
```

over:

```text
entire DOM
```

for remote Translation.

---

# 107. Credential Isolation

Browser cookies, authentication tokens and session objects must remain inside source/platform boundaries.

They must never appear in:

```text
SourceDocumentArtifact

TranslationArtifact

provider Translation payload
```

unless an explicit unrelated architecture contract requires them.

---

# 108. Logging Safety

Do not indiscriminately log:

```text
full novel text

private document text

clipboard contents

full URLs with sensitive parameters

translated content
```

Diagnostics should prefer safe metadata.

---

# 109. History

If CRAI stores reading history:

```text
source identity

chapter

position

translation history
```

retention/privacy policy must be explicit.

Reading history is not automatically required by this flow.

---

# 110. Offline Structured Reading

If source text is locally available and Translation capability supports offline execution:

```text
structured reading
```

may continue without network access.

Provider availability determines actual capability.

---

# 111. Export

Future functionality may export:

```text
translated chapter

bilingual text

reading notes
```

Export should consume Published semantic Artifacts.

It should not scrape UI controls.

---

# 112. Search

Future search may operate on:

```text
SourceDocumentArtifact

TranslationArtifact
```

depending on search semantics.

Search capability does not need access to browser-native DOM objects.

---

# 113. Notes and Highlights

Future annotations should reference stable semantic locators where possible.

Avoid tying durable annotations solely to:

```text
pixel coordinates

temporary DOM node identity
```

for structured sources.

---

# 114. Semantic Locator

A future semantic locator may include:

```text
source identity

chapter/section identity

block identity

text anchor

offset/range
```

Exact design remains open.

---

# 115. Source Mutation and Locator Stability

If the source site changes DOM structure:

```text
semantic locator
```

should ideally survive better than raw CSS/XPath selectors.

This is a future robustness requirement.

---

# 116. Runtime Priority

Suggested priority:

```text
current visible/current requested text
    ↓
explicit user retranslation
    ↓
near-future prefetch
    ↓
background chapter processing
```

Exact scheduler priorities belong to Runtime.

---

# 117. Provider Cost

Structured novels can contain large amounts of text.

Runtime/Translation should avoid:

```text
translating whole unread books automatically
```

unless explicitly requested.

Current/prefetch windows should remain bounded.

---

# 118. Translation Context vs Cost

Larger context may improve quality but increases:

```text
latency

token usage

cost

provider limits
```

Translation batching/context policy must balance these concerns.

---

# 119. Current Reading Window

A useful conceptual model:

```text
Previous Context
        ↓
Current Reading Window
        ↓
Near-Future Prefetch Window
```

Only the current window has highest user-visible priority.

---

# 120. Normal Web Novel Flow

```text
User starts structured reading
    ↓
Session S1
    ↓
browser source selected
    ↓
ReadingContextRevision C1
    ↓
chapter body acquired
    ↓
StructuredSourceSnapshot
    ↓
RuntimeRevision R1
    ↓
Text Processing
    ↓
SourceDocumentArtifact D1
    ↓
Translation
    ↓
TranslationArtifact T1
    ↓
Presentation
    ↓
PresentationArtifact P1
    ↓
UI Adapter
    ↓
Reader ViewModel
```

---

# 121. Scroll Flow

```text
paragraphs 1–10 current
    ↓
user scrolls
    ↓
paragraphs 8–18 relevant
    ↓
source snapshot/update
    ↓
new useful content identified
    ↓
RuntimeRevision R2
    ↓
translate required units only
    ↓
new current Presentation
```

---

# 122. Jump Flow

```text
paragraph 20
    ↓
user jumps to paragraph 500
    ↓
current work priority changes
    ↓
obsolete prefetch deprioritized/cancelled
    ↓
paragraph 500 processing prioritized
```

---

# 123. Chapter Change Flow

```text
Chapter 10
    ↓
Chapter 11
    ↓
source/context evaluation
    ↓
new source snapshot
    ↓
Runtime execution requirements
    ↓
SourceDocumentArtifact
    ↓
Translation
    ↓
Presentation
```

Whether ReadingContextRevision changes is Reading Session-defined.

---

# 124. Cached Paragraph Flow

```text
Paragraph P
    ↓
normalized semantic identity
    ↓
compatible Translation cache hit
    ↓
context compatibility validation
    ↓
current authority validation
    ↓
reuse TranslationArtifact/unit
    ↓
Presentation
```

---

# 125. Late Translation Flow

```text
RuntimeRevision R5
Paragraph A Translation running
    ↓
user jumps to B
    ↓
RuntimeRevision R6
    ↓
R5 superseded for current display
    ↓
A Translation returns
    ↓
Candidate A cannot replace B
```

---

# 126. Font Change Flow

```text
User changes font size
    ↓
Preferences / session presentation config
    ↓
Presentation/UI reprojection
```

Normally:

```text
no Text Processing
no Translation
```

---

# 127. Translation Style Change Flow

```text
User changes Translation style
    ↓
effective Translation configuration changes
    ↓
Application evaluates current content
    ↓
new Runtime execution
    ↓
new TranslationArtifact
    ↓
new PresentationArtifact
```

---

# 128. Source Adapter Fallback

```text
Site Profile Adapter
    ↓
extraction fails
    ↓
Generic Structured Extraction
    ↓
fails?
    ↓
User-assisted selection
```

Exact fallback chain remains source-adapter policy.

---

# 129. Hybrid Source Flow

Some sources may contain:

```text
structured novel text
+
comic/image panels
```

The architecture may branch:

```text
Structured text
    ↓
Text Processing
```

and:

```text
Image region
    ↓
Capture / Image Input
    ↓
Recognition
    ↓
Text Processing
```

before later semantic composition.

Exact mixed-document composition remains an open architecture topic.

---

# 130. Image Embedded in Novel

An embedded image does not automatically require OCR.

Only images containing relevant readable content should enter Recognition.

Decorative images may remain Presentation/source metadata only.

---

# 131. Screenshot Fallback

If structured extraction is impossible:

```text
structured source
    ↓
fallback to visual capture
```

may be supported.

At that point the flow transitions into the image-based path:

```text
Capture
    ↓
Recognition
```

This is a fallback capability, not the default structured-text path.

---

# 132. Flow Convergence

Regardless of source path:

```text
Visual Source
    ↓
Capture
    ↓
Recognition
       \
        \
         → SourceDocumentArtifact
        /
Structured Source
    ↓
Text Processing
```

More precisely:

```text
Visual:
RecognitionArtifact
    ↓
Text Processing
    ↓
SourceDocumentArtifact

Structured:
StructuredSourceSnapshot
    ↓
Text Processing
    ↓
SourceDocumentArtifact
```

After convergence:

```text
SourceDocumentArtifact
    ↓
Translation
    ↓
Presentation
```

---

# 133. Event Bus

Owner modules may publish committed facts.

This flow does not define new canonical event names.

Events may inform:

```text
Diagnostics

history

analytics

UI refresh

other asynchronous observers
```

They do not command downstream processing.

---

# 134. No Event-Driven Stage Chain

Forbidden:

```text
StructuredTextExtracted
    ↓
TranslateRequested
    ↓
TranslationCompleted
    ↓
RenderRequested
```

Execution dependencies belong to Runtime/business orchestration.

---

# 135. Error Ownership

Do not create a generic structured-flow error namespace.

Errors remain with:

```text
Reading Session

source/platform adapter

Text Processing

Translation

Presentation

Runtime
```

---

# 136. Diagnostics

Useful structured-reading observations include:

```text
source extraction latency

normalized block count

Text Processing latency

TranslationUnit count

Translation batch size

Translation latency

cache hit ratio

prefetch hit ratio

superseded Translation count

late result count

time to current visible translation
```

---

# 137. Correlation

Useful correlation chain:

```text
SessionId
    ↓
ReadingContextRevision
    ↓
RuntimeRevisionId
    ↓
SourceDocumentArtifactId
    ↓
TranslationArtifactId
    ↓
PresentationArtifactId
```

WorkItemId/AttemptId may be included for execution tracing.

---

# 138. Performance

Structured text should generally avoid OCR latency.

The target user experience is:

```text
readable structured content
    ↓
useful translated presentation
```

with minimal interruption.

No fixed SLA is established by this document.

---

# 139. Performance Opportunities

Structured text enables:

```text
skip Capture

skip Recognition

incremental normalization

paragraph-level cache

bounded Translation batching

prefetch

fast presentation reprojection
```

These should be exploited without weakening semantic boundaries.

---

# 140. Design Principles

The Structured Text Flow follows:

```text
use native text when available

do not OCR machine-readable text

isolate source-native APIs

normalize before Translation

converge at SourceDocumentArtifact

Translation owns TranslationUnit

Runtime owns execution

bound long-document work

prioritize current reading position

prefetch opportunistically

protect against stale results

preserve semantic structure

minimize private data propagation
```

---

# 141. Critical Invariants

1. Structured text does not require Capture by default.

2. Structured text does not require Recognition by default.

3. Source-native objects remain inside adapters.

4. Text Processing owns SourceDocumentArtifact.

5. Translation consumes SourceDocumentArtifact.

6. Translation owns TranslationUnit.

7. Translation owns TranslationBatch.

8. Presentation consumes TranslationArtifact.

9. UI Adapter owns ViewModel.

10. Runtime remains execution authority.

11. Structured sources do not bypass WorkItem/Attempt semantics where Runtime execution applies.

12. Semantic flow is not a direct module call chain.

13. Event Bus does not command downstream stages.

14. ReadingContextRevision remains separate from RuntimeRevisionId.

15. Content/position change does not automatically mean ReadingContext change.

16. DOM mutation does not automatically mean semantic content change.

17. URL is not sufficient content identity.

18. Native DOM nodes never become public semantic Artifacts.

19. SourceDocumentArtifact preserves useful semantic structure.

20. Translation cache considers context compatibility.

21. Long documents do not create unbounded work.

22. Current reading content outranks prefetch.

23. Prefetch is speculative, not current authority.

24. Superseded Translation cannot replace current Translation.

25. Provider streaming does not automatically become Published Artifact state.

26. Font/theme-only changes should not automatically trigger Translation.

27. Translation-semantic configuration changes may require new Translation.

28. Previous valid Presentation remains safe during new processing.

29. Credentials/cookies never leak into semantic Artifacts.

30. Structured extraction may fall back to visual processing without changing downstream contracts.

---

# 142. Deprecated Architecture

Deprecated universal pipeline:

```text
Source
    ↓
Capture
    ↓
OCR
    ↓
Translation
    ↓
Render
```

Current architecture:

```text
                ┌─ Visual Source
                │      ↓
                │   Capture
                │      ↓
                │ Recognition
                │      ↓
                │
Source Path ────┤
                │
                │ Structured Source
                │      ↓
                │ Structured Extraction
                │      ↓
                └──── Text Processing
                         ↓
                 SourceDocumentArtifact
                         ↓
                    Translation
                         ↓
                  TranslationArtifact
                         ↓
                    Presentation
                         ↓
                  PresentationArtifact
```

---

# 143. Relationship to READING_SESSION_FLOW.md

`READING_SESSION_FLOW.md` defines:

```text
SessionId

ReadingContext

ReadingContextRevision

pause/resume/stop
```

This document applies that session model to structured text sources.

---

# 144. Relationship to CONTENT_CHANGE_FLOW.md

`CONTENT_CHANGE_FLOW.md` defines reusable:

```text
content change

Runtime supersession

cancellation

late-result protection

backpressure
```

Structured reading applies those rules to source snapshots, paragraph windows and chapter changes.

---

# 145. Relationship to SCREEN_COMIC_FLOW.md

`SCREEN_COMIC_FLOW.md` defines the image/screen path.

Both flows converge at:

```text
SourceDocumentArtifact
```

and share:

```text
Translation

Presentation

Runtime

UI Adapter
```

---

# 146. Related Documents

```text
doc/01-architecture/core/
├── STATE_MACHINE.md
├── DATA_FLOW.md
├── CAPABILITY_MAP.md
├── EVENT_CONVENTION.md
└── EVENT_BUS.md

doc/01-architecture/modules/
├── MODULE_MAP.md
├── OWNERSHIP_MAP.md
└── MODULE_DEPENDENCY.md

doc/01-architecture/runtime/
├── BUSINESS_PIPELINE_ORCHESTRATION.md
├── PIPELINE_RUNTIME.md
├── WORK_QUEUE.md
├── SCHEDULER.md
├── RETRY_POLICY.md
├── CANCELLATION.md
└── CACHE_POLICY.md

doc/01-architecture/flows/
├── READING_SESSION_FLOW.md
├── CONTENT_CHANGE_FLOW.md
├── SCREEN_COMIC_FLOW.md
└── STRUCTURED_TEXT_FLOW.md

doc/02-modules/
├── text-processing/
├── translation/
├── presentation/
├── reading-session/
├── preferences/
├── diagnostics/
└── ui-adapter/
```

---

# 147. Open Decisions

The following remain open:

```text
browser integration strategy

extension vs desktop browser integration

DOM extraction contract

StructuredSourceSnapshot ownership/schema

automatic readable-region detection

site-profile architecture

semantic locator design

chapter vs source identity

reading-position revision semantics

incremental SourceDocumentArtifact model

partial TranslationArtifact publication

Translation prefetch window

Translation context window

cache compatibility model

mixed image/text document composition

annotation/ruby representation

structured-source fallback strategy

offline document support

history/annotation persistence
```

These should not be silently frozen by implementation.

---

# 148. Completion Criteria

This flow is synchronized when:

* structured text bypasses Capture/Recognition by default;
* visual and structured paths converge at SourceDocumentArtifact;
* source-native objects remain inside adapters;
* Text Processing owns normalized source-document semantics;
* TranslationUnit remains Translation-owned;
* Runtime remains execution authority;
* ReadingContextRevision remains separate from content/Runtime revisions;
* DOM mutation is separated from semantic content change;
* long-document processing is bounded;
* current content is prioritized over speculative prefetch;
* cache compatibility accounts for Translation context;
* stale Translation cannot replace current content;
* Presentation remains separate from UI projection;
* source credentials are isolated;
* screenshot/OCR fallback remains optional rather than universal.

---

# 149. Summary

The structured-text flow is:

```text
Structured Source
    ↓
Source Adapter
    ↓
Structured Source Snapshot
    ↓
Text Processing
    ↓
SourceDocumentArtifact
    ↓
Translation
    ↓
TranslationArtifact
    ↓
Presentation
    ↓
PresentationArtifact
    ↓
UI Adapter
    ↓
ViewModel
```

Unlike screen-comic reading:

```text
Capture
Recognition
```

are normally unnecessary.

Both source families converge on:

```text
SourceDocumentArtifact
```

so downstream modules do not need to know whether source text came from:

```text
OCR

DOM

clipboard

file

document parser
```

The central invariant is:

```text
Use the strongest semantic source
already available.

Do not destroy structured information
only to reconstruct it later.
```
