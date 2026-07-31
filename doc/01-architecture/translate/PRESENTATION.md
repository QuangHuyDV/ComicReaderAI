# Presentation Architecture

## Purpose

The Presentation Architecture converts translated content into a readable visual form.

It receives translated segments together with their source structure and presentation metadata, then determines how the content should be displayed to the user.

The Presentation layer supports two primary reading modes:

* Native text presentation for novels and text-based content
* Overlay presentation for comics and image-based content

The layer must preserve readability without modifying the underlying source or translation data.

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

Presentation is the final architecture layer before content is rendered in the user interface.

---

## Responsibilities

The Presentation Architecture is responsible for:

* Receiving translated segments
* Matching translations to source content
* Selecting an appropriate presentation mode
* Preserving logical reading order
* Applying typography and layout rules
* Fitting translated text into available display areas
* Displaying original and translated content
* Supporting provisional and partial results
* Handling user display preferences
* Producing presentation models for the UI renderer

---

## Non-Responsibilities

The Presentation Architecture is not responsible for:

* Capturing screenshots
* Detecting text regions
* Performing OCR
* Reconstructing source reading order
* Segmenting text
* Translating content
* Selecting translation providers
* Permanently modifying source text
* Managing application navigation
* Implementing operating-system window behavior

These concerns belong to other CRAI modules.

---

## Presentation Modes

CRAI supports two main presentation modes.

```text
Presentation
    ├── Native Text Presentation
    │
    └── Image Overlay Presentation
```

The selected mode depends on the source content and reading workflow.

---

## Native Text Presentation

Native text presentation is used for content such as:

* Web novels
* Imported text documents
* Browser articles
* Clipboard text
* Extracted DOM content
* User-selected text

The translated content is rendered as normal text rather than drawn over an image.

Primary goals include:

* Comfortable reading
* Clear paragraph structure
* Consistent typography
* Preserved dialogue formatting
* Easy scrolling
* Original-text comparison
* Text selection and copying

---

## Image Overlay Presentation

Image overlay presentation is used for:

* Comics
* Manga
* Manhua
* Manhwa
* Scanned pages
* Screenshot-based content
* Image-only documents

Translated content is displayed relative to detected text regions.

Primary goals include:

* Maintaining the original artwork
* Keeping text visually connected to its source region
* Avoiding unnecessary obstruction
* Preserving reading order
* Fitting translated text into speech bubbles or nearby overlay areas
* Allowing the original content to remain accessible

---

## High-Level Flow

```text
Translation Result
        │
        ▼
Result Validation
        │
        ▼
Source Mapping
        │
        ▼
Presentation Mode Selection
        │
        ├── Native Text Layout
        │
        └── Image Overlay Layout
        │
        ▼
Style Resolution
        │
        ▼
Layout Calculation
        │
        ▼
Presentation Model
        │
        ▼
UI Rendering
```

The Presentation layer produces a renderable model but does not directly depend on a specific UI framework.

---

## Presentation Input

The Presentation layer receives translated segments and source information.

```ts
interface PresentationRequest {
  requestId: string;

  sourceType: PresentationSourceType;
  presentationMode?: PresentationMode;

  segments: PresentationSegment[];

  document?: PresentationDocumentContext;
  viewport?: PresentationViewport;
  preferences?: PresentationPreferences;

  requestRevision: number;
}
```

Possible source types include:

```ts
type PresentationSourceType =
  | "native_text"
  | "web_page"
  | "document"
  | "comic_page"
  | "image"
  | "screen_capture"
  | "selection";
```

---

## Presentation Segment

A presentation segment connects translated text with its original source.

```ts
interface PresentationSegment {
  segmentId: string;

  sourceText: string;
  translatedText?: string;

  sourceOrder: number;
  segmentType: SegmentType;

  sourceRegion?: SourceRegion;
  blockId?: string;
  panelId?: string;
  pageId?: string;
  chapterId?: string;

  translationStatus: TranslationSegmentStatus;

  sourceRevision: number;
  translationRevision?: number;
}
```

Every presentation segment must preserve its original `segmentId`.

---

## Presentation Output

The Presentation layer produces a framework-independent presentation model.

```ts
interface PresentationModel {
  presentationId: string;
  requestId: string;

  mode: PresentationMode;

  items: PresentationItem[];

  viewport?: PresentationViewport;
  warnings?: PresentationWarning[];

  requestRevision: number;
}
```

Possible modes include:

```ts
type PresentationMode =
  | "translated_only"
  | "original_only"
  | "side_by_side"
  | "interleaved"
  | "replace_text"
  | "overlay"
  | "overlay_on_demand";
```

---

## Presentation Item

Each item represents one renderable unit.

```ts
interface PresentationItem {
  itemId: string;
  segmentId: string;

  sourceOrder: number;
  content: PresentationContent;

  layout: PresentationLayout;
  style: PresentationStyle;

  state: PresentationItemState;
}
```

Possible states include:

```ts
type PresentationItemState =
  | "ready"
  | "provisional"
  | "missing_translation"
  | "failed"
  | "hidden"
  | "stale";
```

---

## Source Mapping

Presentation must preserve the relationship between translated content and its source.

For native text, mapping may point to:

* Paragraph
* Sentence
* DOM node
* Document block
* User selection

For comics, mapping may point to:

* OCR region
* Speech bubble
* Caption box
* Panel
* Page coordinates

A presentation item must not be displayed when its source mapping is invalid or stale.

---

## Logical Order

Presentation order must follow the logical order provided by the Text and OCR architectures.

```text
Physical Position
        ≠
Logical Reading Order
        ≠
Visual Rendering Order
```

The Presentation layer may place items differently on screen, but it must preserve the logical sequence for:

* Navigation
* Keyboard movement
* Screen readers
* Translation history
* Reading progression

---

# Native Text Presentation

## Native Text Model

Native text content should preserve the document structure.

```ts
interface NativeTextPresentation {
  blocks: NativeTextBlock[];
}
```

```ts
interface NativeTextBlock {
  blockId: string;
  blockType: NativeTextBlockType;

  items: PresentationItem[];
  sourceOrder: number;
}
```

Possible block types include:

```ts
type NativeTextBlockType =
  | "title"
  | "heading"
  | "paragraph"
  | "dialogue"
  | "quote"
  | "list"
  | "caption"
  | "separator"
  | "unknown";
```

---

## Paragraph Preservation

Segments originating from the same paragraph should remain visually connected.

A paragraph may contain several translation segments, but presentation should avoid displaying each segment as an unrelated block.

```text
Paragraph
    ├── Segment 1
    ├── Segment 2
    └── Segment 3
```

The Presentation layer may combine their visual output while preserving internal segment identities.

---

## Dialogue Presentation

Dialogue should remain distinguishable from narration.

Possible formatting includes:

* Separate lines
* Indentation
* Quotation marks
* Speaker labels
* Dialogue-specific spacing

The layer should not invent a speaker label when the speaker is unknown.

Speaker information may only be displayed when it is available from the source or context data.

---

## Original and Translation Display

Users may choose how original and translated text are shown.

### Translated Only

```text
Translated paragraph
```

### Original Only

```text
Original paragraph
```

### Interleaved

```text
Original paragraph

Translated paragraph
```

### Side by Side

```text
Original text     Translated text
```

Side-by-side mode should only be used when sufficient display width is available.

On narrow screens, it should fall back to interleaved mode.

---

## Text Typography

Native text presentation should support configurable typography.

```ts
interface NativeTextTypography {
  fontFamily?: string;
  fontSize: number;
  lineHeight: number;

  paragraphSpacing: number;
  letterSpacing?: number;

  textAlignment?: "left" | "center" | "right" | "justify";
  maxLineWidth?: number;
}
```

Default settings should prioritize long-form readability.

The presentation system should avoid:

* Excessively long lines
* Very small text
* Insufficient line spacing
* Dense paragraph spacing
* Forced justification that creates large visual gaps

---

## Source Formatting

Useful source formatting may be preserved when safe.

Examples:

* Paragraph breaks
* Headings
* Emphasis
* Quotes
* Lists
* Dialogue markers

Source-specific CSS or document styling should not be copied directly into CRAI without validation.

Presentation should use normalized semantic styles.

---

## Native Text Scrolling

Long-form content should support continuous scrolling.

Presentation should allow:

* Rendering visible blocks first
* Incremental loading
* Preserving reading position
* Restoring chapter position
* Navigating by segment or paragraph
* Updating individual translations without rebuilding the entire document

---

# Image Overlay Presentation

## Overlay Model

An overlay presentation places translated content relative to a source region.

```ts
interface OverlayPresentationItem {
  segmentId: string;

  sourceRegion: SourceRegion;
  overlayRegion: OverlayRegion;

  translatedText: string;

  style: OverlayTextStyle;
  strategy: OverlayStrategy;
}
```

---

## Source Region

A source region represents the location of original text.

```ts
interface SourceRegion {
  x: number;
  y: number;
  width: number;
  height: number;

  coordinateSpace: "image" | "viewport";
  rotation?: number;

  confidence?: number;
}
```

Image-based coordinates should normally be stored relative to the source image rather than the current screen size.

This allows overlays to remain aligned during scaling and resizing.

---

## Coordinate Transformation

The renderer converts image coordinates into viewport coordinates.

```text
Source Image Coordinates
        │
        ▼
Image Scale and Position
        │
        ▼
Viewport Coordinates
        │
        ▼
Rendered Overlay
```

Transformations may account for:

* Image scaling
* Zoom
* Scrolling
* Cropping
* Rotation
* Device pixel ratio
* Window resizing

The original source region must not be permanently rewritten when the viewport changes.

---

## Overlay Strategies

Different text regions may require different overlay strategies.

```ts
type OverlayStrategy =
  | "replace"
  | "cover"
  | "adjacent"
  | "floating"
  | "tooltip"
  | "on_demand";
```

### Replace

The translated text is rendered inside the original text region.

Useful when:

* The background is simple
* The speech bubble is large enough
* Text removal or background reconstruction is available

### Cover

A readable background is placed over the original text, then translated text is rendered above it.

Useful for an initial implementation when background reconstruction is not available.

### Adjacent

The translation is displayed near the original text region.

Useful when the original region is too small or contains important artwork.

### Floating

The translation is displayed in a separate overlay panel connected to the source region.

Useful for dense pages or low-confidence layouts.

### Tooltip

The translation appears when the user hovers, focuses, or selects the source region.

Useful when the user wants to preserve the original artwork.

### On Demand

Translations remain hidden until explicitly requested.

Useful for language learning and reduced visual obstruction.

---

## Overlay Selection

The overlay strategy may depend on:

* Region size
* Text length
* Background complexity
* Speech bubble boundaries
* User preference
* OCR confidence
* Translation status
* Available screen space
* Presentation performance

Example decision flow:

```text
Region Supports Replacement?
        │
       Yes
        │
        ▼
Use Replace or Cover

        No
        │
        ▼
Nearby Space Available?
        │
       Yes
        │
        ▼
Use Adjacent

        No
        │
        ▼
Use Floating or On Demand
```

The selected strategy should be reversible and should not modify the source image.

---

## Speech Bubble Presentation

When speech bubble boundaries are known, translated text may be fitted inside the bubble.

The system should consider:

* Inner bubble bounds
* Bubble shape
* Tail position
* Text orientation
* Source text rotation
* Available safe area
* Neighboring artwork

The OCR text box should not automatically be treated as the full speech bubble area.

A detected text region and a bubble region are different concepts.

```text
Text Region
    ≠
Speech Bubble Region
```

---

## Text Fitting

Translated text frequently differs in length from the source text.

The overlay layout must handle:

* Longer Vietnamese translations
* Short source phrases
* Narrow speech bubbles
* Vertical original text
* Multi-line dialogue
* Rotated captions

A fitting process may follow:

```text
Preferred Font Size
        │
        ▼
Line Wrapping
        │
        ▼
Fit Validation
        │
        ├── Fits
        │
        └── Does Not Fit
                │
                ▼
        Reduce Font Within Limit
                │
                ▼
        Expand Overlay if Allowed
                │
                ▼
        Use Alternative Strategy
```

The system should not reduce text below a readable minimum solely to force it into the source region.

---

## Text Length Constraints

Presentation may provide recommended length information to the Translation layer.

Example:

```ts
interface PresentationConstraint {
  segmentId: string;

  preferredMaxLines?: number;
  preferredMaxCharacters?: number;
  availableWidth?: number;
  availableHeight?: number;
}
```

These constraints are advisory.

The Translation layer may produce a shorter alternative, but it must not remove essential meaning only to satisfy visual limits.

Presentation remains responsible for selecting a fallback layout when the translation does not fit.

---

## Overlay Typography

Overlay text style may include:

```ts
interface OverlayTextStyle {
  fontFamily?: string;
  fontSize: number;
  lineHeight: number;

  alignment: "left" | "center" | "right";
  verticalAlignment: "top" | "center" | "bottom";

  padding: number;
  rotation?: number;

  backgroundMode?: "transparent" | "solid" | "blurred";
  borderMode?: "none" | "outline" | "shadow";
}
```

Typography should remain legible against varied image backgrounds.

The specific visual theme belongs to UI styling, while this architecture defines the required style properties.

---

## Vertical Text

Chinese and Japanese comics may contain vertical source text.

The translated Vietnamese output will usually be displayed horizontally.

Possible approaches include:

* Horizontal text inside the original region
* Rotated horizontal text
* Adjacent translation
* Tooltip translation
* Expanded floating panel

The system should not force Vietnamese into vertical character-by-character layout by default.

---

## Sound Effects

Sound effects may require special presentation.

Possible display modes include:

* Preserve original only
* Replace with translated effect
* Show translation below the source
* Display a small annotation
* Show on demand

Sound-effect presentation should preserve visual artwork whenever possible.

Large stylized sound effects should not automatically be covered by a rectangular overlay.

---

## Background Handling

When covering original text, the Presentation layer may request a background treatment.

Possible treatments include:

* Solid fill
* Semi-transparent fill
* Blur
* Sampled surrounding color
* Inpainted background
* Original bubble fill

Advanced image reconstruction belongs to the image-processing architecture.

Presentation only selects and applies the available background result.

---

## Collision Handling

Overlay items may overlap each other or important page content.

The layout system should detect:

* Overlay-to-overlay collisions
* Overlay-to-source-region displacement
* Overlay outside viewport bounds
* Overlay covering linked regions
* Floating label connector collisions

Possible recovery strategies include:

* Reducing padding
* Repositioning adjacent overlays
* Stacking translations
* Using numbered markers
* Moving content to a translation side panel
* Switching to on-demand display

Logical reading order must remain clear after repositioning.

---

## Overlay Interaction

Users may interact with overlay items.

Possible actions include:

* Show or hide translation
* Show original text
* Compare original and translated text
* Edit translation
* Copy source text
* Copy translated text
* Report OCR error
* Re-run translation
* Change overlay strategy
* Lock overlay position

The Presentation layer exposes these actions through presentation item identities.

Actual command handling belongs to the application or runtime layer.

---

# Shared Presentation Behavior

## Partial Results

Translation may complete segment by segment.

Presentation should support:

* Showing completed segments immediately
* Displaying placeholders for pending segments
* Marking failed segments
* Updating individual items
* Preserving stable layout where possible

A failed translation must not remove the original source content.

---

## Provisional Results

Streaming translation may produce provisional text.

Provisional content must be visually distinguishable from final content.

Possible indicators include:

* Loading state
* Reduced emphasis
* Progress marker
* Temporary placeholder
* Explicit provisional status

Provisional content should be replaced only when the final result has the same active request and segment revision.

---

## Stale Result Protection

Presentation must reject outdated results.

Before applying an update, it should verify:

* Request ID
* Request revision
* Segment ID
* Source revision
* Translation revision
* Active page or chapter
* Presentation revision

A stale result may be retained for diagnostics or cache evaluation, but it must not replace active content.

---

## Missing Translation

When translated text is unavailable, presentation may:

* Display the original text
* Display a translation unavailable indicator
* Keep the overlay hidden
* Offer retry
* Show a previous valid translation marked as outdated

The system must not display fabricated placeholder translations.

---

## User Corrections

Users may edit presented translations.

Presentation should submit corrections to the translation or domain layer using the source `segmentId`.

It should not silently change the canonical translation only inside the visual model.

After a correction is accepted, Presentation receives a new translation revision and updates the corresponding item.

---

## Display Preferences

Presentation preferences may include:

```ts
interface PresentationPreferences {
  preferredMode?: PresentationMode;

  showOriginalText?: boolean;
  showTranslatedText?: boolean;

  nativeTextTypography?: NativeTextTypography;
  overlayTextStyle?: Partial<OverlayTextStyle>;

  minimumFontSize?: number;
  maximumFontSize?: number;

  preserveArtwork?: boolean;
  showProvisionalResults?: boolean;

  overlayOpacity?: number;
  preferredOverlayStrategy?: OverlayStrategy;
}
```

Preferences should affect display behavior without changing source or translation records.

---

## Accessibility

Presentation should support:

* Keyboard navigation
* Screen-reader ordering
* Sufficient text contrast
* Configurable text size
* Reduced motion
* Focus indicators
* Original and translated text labels
* Non-hover alternatives
* Logical tab order

For image overlays, screen-reader content should follow logical reading order rather than physical screen coordinates.

---

## Responsive Behavior

Presentation must adapt to different window sizes.

For native text:

* Side-by-side may become interleaved
* Text width should remain readable
* Typography may scale within configured limits

For comic overlays:

* Overlay coordinates follow image scaling
* Floating items remain within viewport
* Dense overlays may move to a side panel
* Small screens may prefer on-demand presentation

Responsive changes must not alter segment identity or logical reading order.

---

## Zoom

Image zoom and text zoom are separate concerns.

```text
Image Zoom
    ≠
Overlay Font Scale
```

Users may enlarge the source image without wanting oversized overlay text.

Overlay positioning should follow image transformation, while typography may use an independent readable scale policy.

---

## Presentation State

Presentation state may be represented as:

```ts
interface PresentationState {
  presentationId: string;

  status:
    | "idle"
    | "preparing"
    | "ready"
    | "updating"
    | "partial"
    | "failed"
    | "cancelled";

  activeMode: PresentationMode;

  visibleSegmentIds: string[];
  focusedSegmentId?: string;

  revision: number;
}
```

Presentation state describes current visual readiness, not translation progress ownership.

---

## Incremental Rendering

Large chapters and comic collections should be rendered incrementally.

Recommended behavior:

* Render visible content first
* Prepare nearby content second
* Defer distant content
* Reuse unchanged presentation items
* Update only affected segments
* Cancel work for content no longer visible

Incremental rendering should not change logical output.

---

## Virtualization

Native text chapters may contain many blocks.

The UI renderer may virtualize off-screen content, but it must preserve:

* Scroll position
* Block height estimates
* Segment navigation
* Focus restoration
* Reading progress
* Accessibility order

Virtualization is an implementation optimization and must not leak into canonical presentation contracts.

---

## Performance

Interactive presentation should prioritize stable and immediate feedback.

Recommended behavior includes:

* Reusing calculated layouts
* Avoiding full-page recalculation
* Updating only changed regions
* Debouncing viewport resize
* Limiting expensive text-fitting iterations
* Precomputing nearby overlay positions
* Separating image transformations from text layout
* Rendering placeholders during translation

---

## Caching

Presentation models may be cached using inputs such as:

```text
Source Revision
+ Translation Revision
+ Presentation Mode
+ Viewport Class
+ Typography Preferences
+ Overlay Strategy
+ Presentation Engine Version
```

Viewport-specific coordinates should not replace reusable source-relative layout data.

The cache must be invalidated when source regions, translations, or relevant preferences change.

---

## Error Model

Presentation errors should use stable error codes.

```ts
type PresentationErrorCode =
  | "INVALID_PRESENTATION_REQUEST"
  | "SOURCE_MAPPING_MISSING"
  | "INVALID_SOURCE_REGION"
  | "TRANSLATION_NOT_AVAILABLE"
  | "LAYOUT_FAILED"
  | "TEXT_FIT_FAILED"
  | "OVERLAY_COLLISION_UNRESOLVED"
  | "UNSUPPORTED_PRESENTATION_MODE"
  | "STALE_PRESENTATION_RESULT"
  | "PRESENTATION_CANCELLED"
  | "UNKNOWN_PRESENTATION_ERROR";
```

Presentation failures should normally affect individual items rather than blocking the entire page.

---

## Failure Handling

Examples:

* Missing translation → show original content
* Invalid overlay region → use floating or side-panel display
* Text cannot fit → use a larger or alternative overlay
* Unknown segment type → use generic text style
* Missing speech bubble → use OCR region or adjacent display
* Layout calculation fails → fall back to basic readable presentation
* Stale translation → ignore the update

The preferred fallback is always readable content rather than invisible content.

---

## Events

The Presentation Architecture may publish events such as:

```text
PresentationRequested
PresentationPreparationStarted
PresentationModeSelected
PresentationItemPrepared
PresentationLayoutCalculated
PresentationPartiallyReady
PresentationReady
PresentationItemUpdated
PresentationItemFailed
PresentationResultRejectedAsStale
PresentationCancelled
PresentationFailed
```

Interaction events may include:

```text
PresentationItemFocused
OriginalTextRequested
TranslationEditRequested
TranslationRetryRequested
OverlayStrategyChanged
```

Events should reference item and segment identities.

---

## Observability

Useful metrics include:

* Time to first visible translation
* Time to complete presentation
* Number of rendered items
* Number of provisional items
* Number of missing translations
* Layout recalculation count
* Text-fit fallback count
* Overlay collision count
* Average overlay fitting time
* Presentation cache hit rate
* Stale update rejection count
* User-selected presentation modes
* Translation edit frequency

Raw source and translated text should not be included in metrics.

---

## Privacy

Presentation may display private or copyrighted content.

The layer should:

* Avoid logging displayed text
* Prevent hidden overlay content from leaking into diagnostics
* Respect screenshot and screen-sharing privacy settings where available
* Clear sensitive presentation data when documents are closed
* Avoid retaining rendered content longer than necessary

Presentation preferences may be persisted, but private reading content should not be stored as preference data.

---

## Security

Source and translated text must be treated as untrusted content.

The renderer must prevent content from executing:

* HTML scripts
* Embedded event handlers
* Unsafe URLs
* UI commands
* Provider instructions
* Application actions

Native text presentation should use sanitized semantic rendering rather than injecting source HTML directly.

---

## Interaction with Other Modules

### OCR Architecture

Provides:

* Text regions
* Region coordinates
* Reading order
* Rotation
* Confidence
* Optional speech bubble or panel information

Related document:

```text
../ocr/READING_ORDER.md
```

### Text Architecture

Provides:

* Segment identities
* Document structure
* Paragraph relationships
* Segment types
* Logical order
* Source mappings

Related documents:

```text
../text/TEXT_MODEL.md
../text/SEGMENTATION.md
```

### Translation Context

Provides the context used to generate the translation.

Presentation does not rebuild translation context.

Related document:

```text
../translation/CONTEXT.md
```

### Translation Architecture

Provides:

* Translated segments
* Translation status
* Translation revision
* Warnings
* Execution metadata

Related document:

```text
../translation/TRANSLATION.md
```

### Image Processing

May provide:

* Speech bubble regions
* Background masks
* Inpainted regions
* Image transformations
* Panel boundaries

Presentation consumes these results but does not perform image reconstruction.

### Preferences

Provides:

* Typography settings
* Presentation mode
* Overlay behavior
* Original-text visibility
* Accessibility preferences

### Runtime

Coordinates:

* Cancellation
* Active document state
* Navigation
* Request lifecycle
* Event delivery
* Stale-result checks

### User Interface

Consumes the framework-independent presentation model and renders it using the selected UI technology.

---

## MVP Scope

The first implementation should support:

### Native Text

* Translated-only mode
* Interleaved original and translation mode
* Paragraph preservation
* Dialogue formatting
* Configurable font size and line spacing
* Incremental segment updates
* Copying original and translated text

### Comic Overlay

* Source-relative overlay coordinates
* Basic cover overlay
* Basic adjacent overlay
* Automatic line wrapping
* Minimum readable font size
* Show or hide translation
* Original-text comparison
* Per-segment retry and editing
* Fallback to a side or floating panel

### Shared

* Stable segment mapping
* Partial translation display
* Stale-result protection
* Presentation preferences
* Basic accessibility
* Framework-independent presentation contracts

The MVP does not require:

* Automatic image inpainting
* Complex speech bubble shape fitting
* Full collision optimization
* Stylized sound-effect replacement
* Advanced animation
* Automatic typography matching
* Multi-column novel layout
* AI-generated visual redesign

---

## Future Extensions

Possible future capabilities include:

* Automatic speech bubble detection
* Bubble-shape-aware text fitting
* Image inpainting
* Typography matching
* Handwritten font selection
* Curved text rendering
* Improved vertical text handling
* Panel-aware overlay movement
* Smart overlay collision resolution
* Reader themes
* EPUB-style pagination
* Dual-language learning mode
* Word-level vocabulary interaction
* Pronunciation display
* Translation alternatives
* Animated overlay transitions
* AR-style screen translation
* Multi-monitor reading layouts

These capabilities should extend presentation strategies without changing the core source-to-segment mapping.

---

## Design Principles

### Readability First

Translated content must remain readable even when it cannot fit the original layout.

### Source Preservation

Presentation must not permanently modify source images, text, or translations.

### Stable Identity

Every presentation item must remain linked to its original segment.

### Logical Order

Visual placement must not break the logical reading sequence.

### Mode Separation

Native text and image overlays share contracts but use different layout strategies.

### Graceful Fallback

When advanced layout fails, Presentation should fall back to a simpler readable mode.

### Incremental Updates

Individual segments should be updateable without rebuilding the entire presentation.

### User Control

Users should be able to choose how original and translated content are displayed.

### Framework Independence

Presentation models must not depend on a specific desktop or web UI framework.

### Explicit State

Pending, provisional, failed, stale, and completed content must remain distinguishable.

---

## Invariants

1. Every presentation item must reference an existing segment.
2. Presentation must not modify source text or canonical translation data.
3. Logical reading order must be preserved.
4. Native text and comic overlays must use separate layout strategies.
5. Source-relative image coordinates must remain independent of viewport size.
6. Stale translation results must not update active presentation items.
7. Failed translations must not hide available original content.
8. Provisional results must not be represented as final.
9. Presentation preferences must not change source or translation identity.
10. Text must not be reduced below the configured readable minimum.
11. Presentation constraints are advisory and must not remove essential meaning.
12. User corrections must be submitted through canonical translation updates.
13. Unsafe source markup must not execute inside the renderer.
14. Rendering completion order must not determine logical reading order.
15. Overlay fallback must remain available when text fitting fails.
16. Physical proximity must not replace logical reading order.
17. Virtualization must not break navigation or accessibility order.
18. Presentation errors should be isolated to affected items where possible.
19. Original content must remain accessible.
20. Presentation contracts must remain independent of the UI framework.

---

## Related Documents

```text
../ocr/READING_ORDER.md
../text/TEXT_MODEL.md
../text/SEGMENTATION.md
../translation/CONTEXT.md
../translation/TRANSLATION.md
```
