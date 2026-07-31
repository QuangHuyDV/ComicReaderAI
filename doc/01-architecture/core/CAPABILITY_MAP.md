# CRAI Capability Map

> **Project:** CRAI  
> **Document:** Product Capability Map  
> **Version:** 0.1  
> **Status:** Architecture Draft  
> **Last Updated:** 2026-07-20

---

## 1. Purpose

This document describes the capabilities CRAI may require to support image-based and text-based reading translation.

A capability describes what CRAI must be able to do. It does not define source-code folders, services, classes, packages, or deployment units.

The capability map exists before the module map so that module boundaries can be derived from real product needs rather than guessed from technologies.

This document is expected to change during discovery, prototyping, and real reading tests.

All capability and module decisions must follow:

```text
.meta/AI_BOOT.md
.meta/PROJECT_RULE.md
.meta/MODULES_RULE.md
```

---

## 2. Current Product Direction

CRAI is intended to help Vietnamese users read foreign-language content with minimal interruption.

Initial language priorities:

```text
Simplified Chinese  → Vietnamese
Traditional Chinese → Vietnamese
English             → Vietnamese
```

The architecture must remain language-neutral even though Chinese-to-Vietnamese is the primary initial market direction.

CRAI currently recognizes two primary reading flows:

### 2.1 Text Reading Flow

Examples:

- web novels;
- light novels;
- HTML content;
- copied text;
- TXT or EPUB content in future versions.

Text content should use structured source text whenever available.

### 2.2 Image Reading Flow

Examples:

- manga;
- manhua;
- manhwa;
- screenshots;
- scanned pages;
- image-based documents.

Image content may require text detection, OCR, reading-order reconstruction, translation, and non-destructive presentation.

---

## 3. Capability Status Model

Each capability must use one of the following statuses.

### Candidate

The capability may be useful, but its need has not been confirmed.

### Proposed

The capability is currently expected to be needed, but its design is not final.

### Validated

The capability has been confirmed through research, prototype, or user testing.

### MVP

The capability is included in the first usable product scope.

### Deferred

The capability is useful but intentionally postponed.

### Rejected

The capability has been reviewed and is not currently suitable for CRAI.

Capability status is independent from implementation status.

---

## 4. Feasibility Scale

Each capability may be assessed using the following feasibility levels.

| Level | Meaning |
|---|---|
| High | Commonly achievable with known desktop, browser, OCR, or translation technology. |
| Medium | Achievable, but quality depends on providers, source content, operating system, or significant engineering effort. |
| Low | Technically possible, but currently too unreliable, expensive, complex, or unsuitable for the MVP. |
| Unknown | Requires research or prototype before a reliable conclusion can be made. |

Feasibility does not imply product priority.

---

## 5. Top-Level Capability Map

```text
CRAI
├── Content Acquisition
├── Content Observation
├── Content Classification
├── Content Extraction
├── Text Understanding
├── Translation
├── Knowledge and Consistency
├── Presentation
├── User Interaction
├── Reading Session
├── Storage and Recovery
├── Performance and Scheduling
├── Provider Management
├── Privacy and Security
├── Diagnostics and Quality
└── Import, Export, and Integration
```

These groups are product capabilities, not final modules.

---

# 6. Content Acquisition

## 6.1 Purpose

Obtain readable content from a supported source without performing translation or presentation work.

Acquisition should preserve the highest-quality representation available.

Preferred representation order:

```text
Structured text
    ↓
Document text
    ↓
Image with metadata
    ↓
Raw image
    ↓
Screen capture
```

## 6.2 Capabilities

| Capability | Description | Feasibility | Initial Status |
|---|---|---:|---|
| Screen Region Capture | Capture a user-selected rectangular screen area. | High | Proposed |
| Window Capture | Capture a selected application window. | High | Proposed |
| Continuous Screen Capture | Observe a selected region or window over time. | High | Proposed |
| Clipboard Text Input | Accept copied text. | High | Proposed |
| Clipboard Image Input | Accept copied images. | High | Proposed |
| Image File Input | Open common image files. | High | Proposed |
| Folder Image Input | Read ordered images from a folder. | High | Candidate |
| Folder Watch | Detect newly added or changed images. | High | Deferred |
| Native Website Text Extraction | Read structured text through a browser connector. | High | Proposed |
| Website Image Discovery | Discover page images through a browser connector. | Medium | Candidate |
| Direct URL Import | Import text or images from a supplied URL. | Medium | Deferred |
| PDF Text Import | Extract structured text from PDFs. | High | Deferred |
| Scanned PDF Import | Render scanned PDF pages as images. | High | Deferred |
| EPUB Import | Read chapter and paragraph structure from EPUB. | High | Deferred |

## 6.3 Important Constraints

- Acquisition must not call OCR or translation directly.
- Website integration must not assume that every site exposes stable HTML or direct image URLs.
- Screen capture should be treated as a general fallback, not automatically as the best source.
- Content access must respect user permissions and applicable source restrictions.
- Captured or imported content must retain source identifiers when possible.

## 6.4 MVP Consideration

The first MVP should likely validate one of these paths:

```text
Desktop screen/window capture
```

or:

```text
Browser text extraction
```

Supporting both at the same time may increase MVP scope substantially.

---

# 7. Content Observation

## 7.1 Purpose

Determine when the currently observed content has changed enough to require new processing.

This capability is especially important for continuous screen translation.

## 7.2 Capabilities

| Capability | Description | Feasibility | Initial Status |
|---|---|---:|---|
| Frame Difference Detection | Determine whether the captured image has materially changed. | High | Proposed |
| Region Difference Detection | Identify which subregions changed. | Medium | Candidate |
| Scroll Detection | Infer that the reader scrolled to new content. | Medium | Proposed |
| Page Transition Detection | Detect a likely page or chapter change. | Medium | Candidate |
| Stable Frame Detection | Wait until animation or scrolling has settled before processing. | High | Proposed |
| Duplicate Frame Detection | Avoid processing visually identical content. | High | Proposed |
| Zoom and Scale Change Detection | Detect layout changes caused by zoom. | Medium | Candidate |
| Browser DOM Change Observation | Detect structured page updates through an extension. | High | Proposed |

## 7.3 Key Risks

- advertisements, animations, video, cursors, and blinking elements may trigger false changes;
- overly sensitive detection wastes OCR and translation requests;
- overly conservative detection may miss new content;
- coordinate changes can invalidate existing overlays;
- page transitions must cancel obsolete processing jobs.

## 7.4 Validation Need

A prototype should measure:

- false-positive change rate;
- missed content changes;
- time required to consider a frame stable;
- CPU and GPU cost during continuous observation.

---

# 8. Content Classification

## 8.1 Purpose

Identify the type and basic structure of acquired content so CRAI can choose the correct processing flow.

## 8.2 Capabilities

| Capability | Description | Feasibility | Initial Status |
|---|---|---:|---|
| Content Type Detection | Distinguish structured text, image, scanned page, and mixed content. | High | Proposed |
| Language Detection | Detect probable source language. | High | Proposed |
| Script Detection | Distinguish Han characters, Latin text, and other scripts. | High | Proposed |
| Text Presence Detection | Determine whether an image contains readable text. | High | Proposed |
| Reading Direction Detection | Detect horizontal, vertical, left-to-right, or right-to-left ordering. | Medium | Proposed |
| Comic Page Detection | Estimate whether an image is comic-like. | Medium | Candidate |
| Novel Page Detection | Estimate whether content is mainly continuous prose. | Medium | Candidate |
| Mixed Content Detection | Detect pages containing both structured text and images. | Medium | Candidate |

## 8.3 Design Rule

Classification informs processing but must not permanently lock content into a flow.

Users should be able to override incorrect automatic classification.

---

# 9. Content Extraction

## 9.1 Purpose

Convert acquired content into machine-processable text and structural metadata.

Text extraction and image OCR are different paths.

## 9.2 Native Text Extraction

| Capability | Description | Feasibility | Initial Status |
|---|---|---:|---|
| DOM Text Extraction | Read text from supported browser pages. | High | Proposed |
| Paragraph Extraction | Preserve paragraph boundaries. | High | Proposed |
| Dialogue and Line Break Preservation | Retain meaningful source formatting. | Medium | Proposed |
| Chapter Metadata Extraction | Read title, chapter number, and navigation metadata. | Medium | Candidate |
| Noise Removal | Exclude menus, advertisements, comments, and unrelated text. | Medium | Proposed |

## 9.3 Image Text Extraction

| Capability | Description | Feasibility | Initial Status |
|---|---|---:|---|
| Image Preprocessing | Resize, denoise, sharpen, deskew, or adjust contrast. | High | Proposed |
| Text Region Detection | Locate text-containing image regions. | High | Proposed |
| OCR Recognition | Convert detected image text into characters. | High | Proposed |
| OCR Confidence | Report confidence or uncertainty. | High | Proposed |
| Vertical Chinese OCR | Recognize vertically written Chinese. | Medium | Proposed |
| Stylized Font OCR | Recognize decorative or distorted comic fonts. | Medium | Candidate |
| Reading Order Reconstruction | Order detected regions into a readable sequence. | Medium | Proposed |
| Speech Bubble Detection | Detect likely dialogue containers. | Medium | Candidate |
| Sound Effect Detection | Identify comic sound effects separately from dialogue. | Low to Medium | Deferred |
| Handwritten Text Recognition | Recognize handwritten text. | Low to Medium | Deferred |

## 9.4 Feasibility Conclusion

Basic OCR for clear Chinese and English text is feasible.

The main uncertainty is not whether OCR works, but whether it works reliably enough for:

- low-resolution comic images;
- vertical text;
- stylized fonts;
- text overlapping artwork;
- unusual reading order;
- compressed website images.

This area requires representative test images before provider selection.

---

# 10. Text Understanding

## 10.1 Purpose

Transform extracted text into coherent translation units while preserving meaning and reading structure.

OCR output must not be assumed to be translation-ready.

## 10.2 Capabilities

| Capability | Description | Feasibility | Initial Status |
|---|---|---:|---|
| Unicode Normalization | Normalize characters and punctuation. | High | Proposed |
| OCR Error Cleanup | Correct common recognition artifacts. | Medium | Proposed |
| Line Merge | Combine lines that belong to one sentence. | Medium | Proposed |
| Sentence Segmentation | Split text into translation-ready sentences. | High | Proposed |
| Paragraph Reconstruction | Rebuild prose paragraph structure. | Medium | Proposed |
| Dialogue Grouping | Group dialogue lines and speech regions. | Medium | Proposed |
| Reading Order Correction | Allow automatic and manual ordering correction. | Medium | Proposed |
| Proper Name Detection | Identify likely names. | Medium | Proposed |
| Term Detection | Identify recurring terms and special vocabulary. | Medium | Proposed |
| Speaker Attribution | Determine who speaks each line. | Low | Deferred |
| Context Building | Build nearby context for translation. | High | Proposed |
| Chapter Context | Reuse earlier chapter information when appropriate. | Medium | Candidate |

## 10.3 Chinese-Specific Considerations

Initial Chinese support should consider:

- simplified and traditional character variants;
- names that require consistent Vietnamese rendering;
- omitted subjects and context-dependent pronouns;
- idioms and cultivation or fantasy terminology;
- short comic fragments that require neighboring regions;
- vertical punctuation and OCR ordering;
- Chinese sentences split across several bubbles or lines.

The architecture should support language profiles without creating separate Chinese-only core modules.

---

# 11. Translation

## 11.1 Purpose

Convert prepared source-language content into Vietnamese while preserving meaning, tone, terminology, and structural alignment.

Translation must remain independent from final visual presentation.

## 11.2 Capabilities

| Capability | Description | Feasibility | Initial Status |
|---|---|---:|---|
| Single Segment Translation | Translate one prepared segment. | High | Proposed |
| Batch Translation | Translate several related segments efficiently. | High | Proposed |
| Context-Aware Translation | Include surrounding text or chapter context. | High | Proposed |
| Streaming Translation | Return partial results where providers support it. | Medium | Candidate |
| Language Pair Configuration | Select source and target languages. | High | Proposed |
| Automatic Language Detection | Infer source language when not specified. | High | Proposed |
| Provider Selection | Choose an available translation implementation. | High | Proposed |
| Provider Fallback | Retry through another provider when appropriate. | Medium | Candidate |
| Timeout and Cancellation | Stop obsolete or excessively slow requests. | High | Proposed |
| Retry Policy | Retry bounded recoverable failures. | High | Proposed |
| Translation Alignment | Preserve mapping between source and translated segments. | High | Proposed |
| Translation Confidence | Expose uncertainty when supported or inferred. | Medium | Candidate |
| Style Profile | Apply novel, comic, formal, or conversational preferences. | Medium | Candidate |
| User Correction | Allow users to edit an incorrect translation. | High | Proposed |

## 11.3 Quality Risks

Translation quality may degrade because of:

- missing context;
- incorrect OCR;
- ambiguous names;
- short isolated dialogue;
- provider content limits;
- inconsistent terminology;
- culturally specific expressions;
- excessive literal translation;
- long chapters exceeding provider context limits.

The product must not imply that all translations are authoritative.

---

# 12. Knowledge and Consistency

## 12.1 Purpose

Keep names, terms, user corrections, and repeated translations consistent across a reading session or series.

## 12.2 Capabilities

| Capability | Description | Feasibility | Initial Status |
|---|---|---:|---|
| User Glossary | Store preferred translations for names and terms. | High | Proposed |
| Series Glossary | Apply terms only to a selected story or series. | High | Candidate |
| Global Glossary | Apply user-wide preferences. | High | Candidate |
| Translation Memory | Reuse identical or similar previous translations. | High | Proposed |
| Name Dictionary | Maintain source name and Vietnamese rendering pairs. | High | Proposed |
| User Override | Persist manual translation corrections. | High | Proposed |
| Context Notes | Store character, world, or terminology notes. | High | Deferred |
| Similarity Matching | Reuse translations for near-duplicate text. | Medium | Candidate |
| Automatic Learning | Infer preferences from repeated corrections. | Medium | Deferred |

## 12.3 Data Scope

Knowledge must define scope explicitly:

```text
Current segment
Reading session
Chapter
Series
Source website
User account or local profile
Global
```

A term from one series must not silently affect another unrelated series unless configured globally.

---

# 13. Presentation

## 13.1 Purpose

Display translated content in a readable, minimally disruptive form.

Text and image content require different presentation strategies.

## 13.2 Text Presentation

| Capability | Description | Feasibility | Initial Status |
|---|---|---:|---|
| Reader View | Present translated prose in a controlled reading surface. | High | Proposed |
| In-Page Replacement | Replace or supplement website text through an extension. | High | Candidate |
| Parallel Text View | Show source and translation together. | High | Candidate |
| Font Selection | Select readable fonts. | High | Proposed |
| Font Size and Line Height | Control typography. | High | Proposed |
| Paragraph Spacing | Preserve readable separation. | High | Proposed |
| Reading Width | Limit line length for comfortable reading. | High | Proposed |
| Theme | Provide light, dark, and custom reading themes. | High | Proposed |
| Alignment and Indentation | Control text alignment and first-line indentation. | High | Proposed |
| Chapter Navigation | Move between structured chapters. | High | Deferred |

## 13.3 Image Presentation

| Capability | Description | Feasibility | Initial Status |
|---|---|---:|---|
| Translation Side Panel | Show ordered translated entries beside the image. | High | Proposed |
| Region Numbering | Link source regions to translated entries. | High | Proposed |
| Non-Destructive Overlay | Display translation near source regions without editing the image. | Medium | Proposed |
| Hover or Focus Translation | Show translation only for the active region. | High | Candidate |
| Overlay Scaling | Keep overlay aligned during zoom or resize. | Medium | Proposed |
| Overflow Detection | Detect when Vietnamese text cannot fit. | High | Proposed |
| Adaptive Text Wrapping | Wrap translated text based on available space. | High | Proposed |
| Source Text Removal | Remove original text from the image. | Medium | Deferred |
| Background Reconstruction | Rebuild the image behind removed text. | Medium to Low | Deferred |
| Bubble-Aware Text Insertion | Insert Vietnamese text into speech bubbles. | Medium to Low | Deferred |
| Export Translated Image | Save a rendered translated image. | High | Deferred |

## 13.4 Initial Presentation Direction

The MVP should prefer non-destructive strategies:

```text
Side panel
Region-linked translation list
Simple overlay
```

Permanent image modification should not be an MVP requirement.

---

# 14. User Interaction

## 14.1 Purpose

Allow the user to control how CRAI observes, translates, and presents reading content without interrupting the reading flow unnecessarily.

## 14.2 Capabilities

| Capability | Description | Feasibility | Initial Status |
|---|---|---:|---|
| Source Selection | Select a screen region, window, browser tab, file, or text source. | High | Proposed |
| One-Time Region Selection | Keep observing a previously selected region. | High | Proposed |
| Pause and Resume | Temporarily stop observation or processing. | High | Proposed |
| Manual Retranslate | Request a new translation. | High | Proposed |
| Manual OCR Correction | Correct recognized source text. | High | Candidate |
| Manual Region Adjustment | Move or resize detected text regions. | Medium | Candidate |
| Reading Mode Selection | Choose text reader, side panel, or overlay. | High | Proposed |
| Provider Selection | Choose OCR or translation provider. | High | Proposed |
| Language Selection | Choose source and target language. | High | Proposed |
| Quick Glossary Edit | Add or change a name or term while reading. | High | Proposed |
| Translation Edit | Correct translated content. | High | Proposed |
| Keyboard Shortcuts | Control translation without leaving the reading flow. | High | Proposed |
| Mouse Hover Behavior | Reveal or hide region translations. | High | Candidate |
| Zoom Reaction | Re-align overlays after zoom. | Medium | Proposed |
| Scroll Reaction | Delay, cancel, or preload work during scrolling. | Medium | Proposed |

## 14.3 Interaction Principle

The default reading experience should minimize repeated manual selection.

Manual correction must remain available when automatic detection is wrong.

---

# 15. Reading Session

## 15.1 Purpose

Maintain the lifecycle and state of an active reading experience.

## 15.2 Capabilities

| Capability | Description | Feasibility | Initial Status |
|---|---|---:|---|
| Start Session | Begin reading from a selected source. | High | Proposed |
| Pause Session | Stop new work while preserving state. | High | Proposed |
| Resume Session | Continue from preserved state. | High | Proposed |
| Stop Session | Cancel work and release resources. | High | Proposed |
| Current Content Tracking | Track the latest page, frame, or chapter. | High | Proposed |
| Processing State Tracking | Track acquired, recognized, translated, and presented states. | High | Proposed |
| Stale Result Rejection | Prevent old results from replacing newer content. | High | Proposed |
| Navigation State | Track reading position. | High | Candidate |
| Progress Tracking | Track chapter or session progress. | High | Deferred |
| Bookmark | Save a reading position. | High | Deferred |
| Session Recovery | Restore an interrupted session. | High | Deferred |

## 15.3 Required Identifiers

Likely trace identifiers include:

```text
ReadingSessionId
SourceId
ContentId
FrameId
RegionId
SegmentId
TranslationRequestId
```

The final identifier model will be defined during module and contract design.

---

# 16. Storage and Recovery

## 16.1 Purpose

Retain only the information necessary for performance, consistency, recovery, and user-requested history.

## 16.2 Capabilities

| Capability | Description | Feasibility | Initial Status |
|---|---|---:|---|
| User Settings Storage | Persist reading and provider preferences. | High | Proposed |
| OCR Cache | Reuse recognition results for identical content. | High | Proposed |
| Translation Cache | Reuse translations for identical requests. | High | Proposed |
| Image Fingerprint Storage | Identify repeated image content. | High | Proposed |
| Glossary Storage | Persist preferred names and terms. | High | Proposed |
| Translation Memory Storage | Persist reusable translations. | High | Proposed |
| Session State Storage | Preserve active reading state. | High | Deferred |
| Reading History | Store previously opened content. | High | Deferred |
| Imported Content Library | Store user-imported chapters or files. | High | Deferred |
| Offline Content Package | Bundle source and translated content. | High | Deferred |
| Backup and Restore | Export and restore CRAI data. | High | Deferred |

## 16.3 Initial Storage Policy

The MVP should default to local storage.

The initial design should avoid requiring a CRAI cloud account.

Persistent storage must distinguish:

- temporary processing data;
- reusable cache;
- user-created knowledge;
- imported copyrighted content;
- optional reading history.

Raw captured images should not be stored permanently by default.

---

# 17. Performance and Scheduling

## 17.1 Purpose

Keep translation responsive while preventing wasteful processing and protecting the reading experience.

## 17.2 Capabilities

| Capability | Description | Feasibility | Initial Status |
|---|---|---:|---|
| Background Processing | Run heavy work outside the UI thread. | High | Proposed |
| Work Queue | Schedule capture, OCR, translation, and rendering jobs. | High | Proposed |
| Cancellation | Cancel obsolete operations. | High | Proposed |
| Debouncing | Delay work until scrolling or animation settles. | High | Proposed |
| Deduplication | Avoid repeated processing of identical content. | High | Proposed |
| Priority Scheduling | Prioritize visible content over preloaded content. | High | Candidate |
| Parallel Processing | Process independent regions concurrently. | High | Candidate |
| Batch Requests | Group OCR or translation requests. | High | Proposed |
| Prefetch | Process likely upcoming content. | Medium | Deferred |
| Resource Limits | Limit CPU, memory, queue size, and network concurrency. | High | Proposed |
| Adaptive Quality | Trade quality for speed based on device or mode. | Medium | Candidate |
| Offline Fallback | Use local capabilities when remote services are unavailable. | Medium | Deferred |

## 17.3 Key Performance Questions

- What translation delay remains comfortable during scrolling?
- Should CRAI translate only visible content or also nearby content?
- How much CPU usage is acceptable during continuous screen observation?
- When should a running OCR or translation job be cancelled?
- How large may image and translation caches grow?

These questions require prototypes and measurements.

---

# 18. Provider Management

## 18.1 Purpose

Allow CRAI to use replaceable OCR, translation, and optional AI providers without leaking provider-specific details into product workflows.

## 18.2 Capabilities

| Capability | Description | Feasibility | Initial Status |
|---|---|---:|---|
| OCR Provider Registration | Register one or more OCR implementations. | High | Proposed |
| Translation Provider Registration | Register one or more translation implementations. | High | Proposed |
| Local Provider Support | Use local models or libraries. | High | Proposed |
| Remote Provider Support | Use network APIs. | High | Proposed |
| Provider Capability Discovery | Report supported languages, limits, and modes. | High | Proposed |
| Provider Health Check | Determine whether a provider is available. | High | Candidate |
| Provider Configuration | Configure model, endpoint, timeout, and credentials. | High | Proposed |
| Provider Fallback | Switch providers after recoverable failures. | Medium | Candidate |
| Usage and Cost Tracking | Estimate API usage and cost. | High | Candidate |
| Runtime Plugin Loading | Load independently distributed providers at runtime. | Medium | Deferred |

## 18.3 MVP Rule

The MVP should use clear provider contracts and adapters.

A dynamic plugin framework should be deferred until a real extension need exists.

---

# 19. Privacy and Security

## 19.1 Purpose

Protect captured reading content, credentials, user corrections, and stored reading data.

## 19.2 Capabilities

| Capability | Description | Feasibility | Initial Status |
|---|---|---:|---|
| Local-Only Mode | Restrict relevant processing to the local device. | Medium | Candidate |
| Remote Data Disclosure | Clearly show when content is sent to an external provider. | High | Proposed |
| Credential Protection | Store provider credentials securely. | High | Proposed |
| Sensitive Logging Prevention | Exclude credentials and raw private content from normal logs. | High | Proposed |
| Temporary Data Cleanup | Remove temporary captures and files. | High | Proposed |
| Storage Retention Controls | Let users control cache and history retention. | High | Candidate |
| Private Session Mode | Avoid persistent history for selected sessions. | High | Candidate |
| Export Consent | Require explicit action before exporting source or translated content. | High | Proposed |

## 19.3 Privacy Questions

- Which providers retain submitted text or images?
- Can local OCR satisfy the initial quality requirement?
- Should translation cache store full source and translated text?
- Should screen captures ever be written to disk?
- How should imported chapters be separated from temporary screen content?

Provider-specific answers must be researched before implementation decisions.

---

# 20. Diagnostics and Quality

## 20.1 Purpose

Make failures, poor recognition, incorrect ordering, and translation quality problems observable and correctable.

## 20.2 Capabilities

| Capability | Description | Feasibility | Initial Status |
|---|---|---:|---|
| Stage Timing | Measure acquisition, OCR, translation, and presentation duration. | High | Proposed |
| Structured Error Reporting | Identify the failed capability and reason. | High | Proposed |
| OCR Confidence Display | Surface uncertain recognition. | High | Candidate |
| Translation Issue Reporting | Mark or report poor translations. | High | Candidate |
| Processing Trace | Link frames, regions, segments, and translation requests. | High | Proposed |
| Provider Diagnostics | Report provider status, timeout, rate limit, or invalid configuration. | High | Proposed |
| Debug Overlay | Show detected regions and reading order. | High | Candidate |
| Quality Test Dataset | Maintain representative Chinese, English, comic, and novel samples. | High | Proposed |
| Regression Evaluation | Compare OCR and translation behavior after changes. | Medium | Proposed |
| User Feedback Capture | Record corrections and usability issues. | High | Candidate |

## 20.3 Quality Validation Areas

CRAI should eventually evaluate:

- OCR character accuracy;
- region detection accuracy;
- reading-order correctness;
- terminology consistency;
- translation usefulness;
- time to first visible translation;
- overlay alignment;
- user correction frequency;
- CPU and memory cost.

Exact metrics will be defined after the first prototypes.

---

# 21. Import, Export, and Integration

## 21.1 Purpose

Connect CRAI to browsers, files, readers, and external tools without placing integration-specific behavior inside core translation flows.

## 21.2 Capabilities

| Capability | Description | Feasibility | Initial Status |
|---|---|---:|---|
| Browser Connector | Exchange structured text, image references, and page events with CRAI. | High | Proposed |
| Browser In-Page Presentation | Present translation inside a supported webpage. | High | Candidate |
| File Import | Import text, image, PDF, or EPUB content. | High | Deferred |
| Translation Export | Export translated text. | High | Deferred |
| Bilingual Export | Export source and translated content together. | High | Deferred |
| Translated Image Export | Save rendered translated images. | High | Deferred |
| Subtitle-Like Region Export | Export ordered regions and translated text. | High | Deferred |
| Local API | Allow trusted local integrations to request processing. | High | Deferred |
| Automation Hook | Trigger import or translation from external tools. | Medium | Deferred |
| Cloud Synchronization | Synchronize settings and knowledge across devices. | High | Deferred |

## 21.3 Website Integration Risk

Website support may vary because of:

- different HTML structures;
- dynamic rendering;
- canvas-based readers;
- protected or temporary image URLs;
- authentication;
- anti-automation mechanisms;
- frequent page redesigns.

The initial browser connector should prefer generic browser capabilities and a limited set of validated adapters rather than promising universal website compatibility.

---

# 22. Cross-Capability Product Flows

## 22.1 Screen Comic Translation Flow

```text
User selects a window or region
    ↓
CRAI observes visual changes
    ↓
A stable new frame is captured
    ↓
Text regions are detected
    ↓
OCR produces ordered source segments
    ↓
Source segments are normalized and grouped
    ↓
Segments are translated into Vietnamese
    ↓
Translations are shown in a side panel or overlay
    ↓
Results are cached and associated with the session
```

Primary capabilities involved:

- Content Acquisition
- Content Observation
- Content Classification
- Content Extraction
- Text Understanding
- Translation
- Presentation
- User Interaction
- Reading Session
- Performance and Scheduling

## 22.2 Browser Novel Translation Flow

```text
Browser connector receives structured page text
    ↓
Relevant chapter content is extracted
    ↓
Paragraph and dialogue structure are preserved
    ↓
Translation context is built
    ↓
Text is translated into Vietnamese
    ↓
Reader layout or in-page presentation is applied
    ↓
Glossary and translation memory maintain consistency
```

Primary capabilities involved:

- Content Acquisition
- Content Classification
- Content Extraction
- Text Understanding
- Translation
- Knowledge and Consistency
- Presentation
- Reading Session

## 22.3 Imported Content Flow

```text
User imports a file, folder, or URL
    ↓
CRAI determines content type
    ↓
The appropriate text or image flow runs
    ↓
Processed content is stored locally
    ↓
The user reads through a CRAI-controlled reader
```

This flow is useful but should remain deferred until core translation quality and presentation are validated.

---

# 23. Initial Feasibility Assessment

## 23.1 Clearly Feasible

The following capabilities are considered technically straightforward enough to proceed to design or prototype:

- screen region and window capture;
- clipboard and image-file input;
- native browser text extraction through an extension;
- basic Chinese and English OCR;
- Chinese, English, and Vietnamese language detection;
- provider-based translation;
- text reader typography;
- translation side panel;
- caching and cancellation;
- user glossary and translation corrections;
- local settings storage;
- diagnostics and processing traces.

## 23.2 Feasible but Quality-Sensitive

The following are achievable but require representative tests:

- vertical Chinese OCR;
- stylized comic text OCR;
- comic reading-order reconstruction;
- speech bubble detection;
- continuous screen change detection;
- overlay alignment during scroll and zoom;
- context-aware comic translation;
- name and terminology consistency;
- universal browser-page content extraction.

## 23.3 Feasible but Not Suitable for the First MVP

- downloading and managing complete online series;
- translated-content library management;
- EPUB and scanned PDF reader support;
- permanent image text replacement;
- background reconstruction and inpainting;
- cloud synchronization;
- third-party runtime plugin marketplace;
- automatic speaker attribution;
- automatic long-term learning from user behavior.

## 23.4 Main Product Risk

The largest risk is not whether individual technologies exist.

The largest risk is whether the complete flow is fast, accurate, and unobtrusive enough for comfortable continuous reading.

Therefore, CRAI should validate complete reading flows rather than testing OCR or translation only in isolation.

---

# 24. Recommended MVP Capability Scope

The initial MVP should prove one complete image-reading flow.

Recommended scope:

```text
Desktop application
    ↓
Select a window or screen region once
    ↓
Detect stable content changes
    ↓
Recognize Simplified Chinese and English text
    ↓
Translate into Vietnamese
    ↓
Show ordered translations in a side panel
    ↓
Allow manual retranslation and glossary correction
```

Recommended MVP capabilities:

- Screen Region Capture
- Window Capture
- Stable Frame Detection
- Duplicate Frame Detection
- Text Region Detection
- OCR Recognition
- Reading Order Reconstruction
- Basic Text Normalization
- Context-Aware Batch Translation
- Translation Side Panel
- Region Numbering
- Language and Provider Settings
- Cancellation and Deduplication
- OCR and Translation Cache
- Basic User Glossary
- Processing Diagnostics

Explicitly excluded from the first MVP:

- downloading full stories from websites;
- permanent storage of captured chapters;
- automatic source-text removal;
- background inpainting;
- speech-bubble text replacement;
- universal browser extension support;
- cloud accounts and synchronization;
- complex plugin loading.

---

# 25. Prototype Gates

Before converting capabilities into stable modules, CRAI should pass several prototype gates.

## Gate A — Capture and Change Detection

Validate that CRAI can continuously observe a selected reading area without excessive CPU usage or repeated processing.

Success questions:

- Can the user select the region only once?
- Can scrolling and stable content be distinguished?
- Can duplicate frames be ignored?
- Can stale work be cancelled?

## Gate B — Chinese Comic OCR

Validate OCR against representative Simplified Chinese comic images.

Success questions:

- Are horizontal and vertical text recognized?
- Are detected regions ordered correctly?
- Are confidence and errors visible?
- Is processing fast enough for normal reading?

## Gate C — Translation Usefulness

Validate Chinese-to-Vietnamese translation with comic context.

Success questions:

- Are names and terms consistent?
- Does grouping several regions improve translation?
- Can the user correct bad results quickly?
- Is the result understandable enough to continue reading?

## Gate D — Presentation Experience

Validate side-panel and simple-overlay presentation.

Success questions:

- Can the user match each translation to its source region?
- Does the UI obstruct the website or image?
- Does it remain aligned after zoom, resize, and scroll?
- Is reading less interrupted than manual copy and translation?

## Gate E — Text Novel Flow

After the image MVP is validated, test browser-based structured text extraction and reader formatting.

Success questions:

- Can the correct chapter text be isolated?
- Are paragraphs and dialogue preserved?
- Can long chapters be translated incrementally?
- Is the reading layout comfortable for Vietnamese text?

---

# 26. Open Decisions

The following decisions should remain open until prototype evidence is available.

## Product Interaction

- Is the first product desktop-only?
- Is the main presentation a side panel, overlay, or hybrid?
- Should the app automatically follow the active browser window?
- Should the user explicitly start and stop each reading session?

## OCR

- Which local OCR provider best supports Simplified Chinese and vertical text?
- Is one OCR provider sufficient for both comics and general screenshots?
- Should text detection and text recognition use separate providers?

## Translation

- Which provider offers the best Chinese-to-Vietnamese quality and acceptable cost?
- Should the MVP use a general translation API, an LLM, or both?
- How much surrounding context should be sent?
- How should provider privacy and retention policies affect defaults?

## Presentation

- How should region numbering appear without covering artwork?
- When should translation use a side panel instead of an overlay?
- What minimum font size is considered readable?
- How should overflow and long Vietnamese translations be handled?

## Storage

- How long should OCR and translation cache entries remain?
- Should raw screenshots remain memory-only?
- Should manual corrections be stored globally or per series?
- When should reading history become opt-in?

## Website Integration

- Should browser integration communicate with the desktop app or work independently?
- Should CRAI support generic extraction only or site-specific adapters?
- Is downloading content necessary, or is live reading sufficient?

---

# 27. Capability-to-Module Transition Rules

A capability may be used to propose a module only when:

- its responsibility is understood;
- its inputs and outputs are visible;
- its lifecycle differs meaningfully from neighboring capabilities;
- it owns data or behavior that requires a boundary;
- it needs independent replacement or testing;
- combining it with another responsibility would create undesirable coupling.

Several capabilities may belong to one module.

One complex capability may require several modules.

The mapping must be documented in:

```text
.meta/MODULES.md
```

The capability map must not be modified merely to match a preferred code structure.

---

# 28. Next Architecture Work

Recommended next documents:

```text
docs/architecture/flows/SCREEN_COMIC_FLOW.md
docs/architecture/flows/BROWSER_TEXT_FLOW.md
docs/architecture/FEASIBILITY_MATRIX.md
.meta/MODULES.md
```

Recommended order:

```text
CAPABILITY_MAP.md
    ↓
SCREEN_COMIC_FLOW.md
    ↓
Prototype gates and feasibility validation
    ↓
MODULES.md
    ↓
Detailed module contracts
```

The immediate next task should be to define the first complete screen-comic reading flow in enough detail to identify state, events, cancellation points, and user interactions.
