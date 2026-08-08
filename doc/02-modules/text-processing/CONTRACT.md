# Text Processing Module Contract

> **Project:** CRAI
> **Module:** Text Processing
> **Path:** `02-modules/text-processing/CONTRACT.md`
> **Version:** 1.0.0
> **Status:** Architecture Draft
> **Primary Output:** `SourceDocument`

---

# 1. Purpose

Tài liệu này định nghĩa public contract của Text Processing Module.

Nó đặc tả:

* Runtime-facing Attempt Input
* Runtime-facing Attempt Output
* Processing Profile
* Processing Options
* Source Context
* SourceDocument
* SourceBlock
* SourceBlock Sequence
* OCR Source References
* normalization traceability
* reconstruction traceability
* block classification
* block exclusion
* Candidate SourceDocument Artifact
* Published SourceDocument Artifact
* completeness
* module warnings
* module errors
* RetryHint
* compatibility metadata
* privacy requirements
* producer obligations
* consumer obligations
* Runtime obligations
* contract evolution

Text Processing Contract không định nghĩa lại:

* OCR Region
* OCR Geometry
* Recognition text hierarchy
* Text Direction
* Layout
* OCR Reading Order
* OCR Quality
* Translation Unit
* Translation Result
* Runtime lifecycle

---

# 2. Contract Boundary

Primary flow:

```text
TextProcessingAttemptInput
        ↓
Text Processing Module
        ↓
TextProcessingAttemptOutput
        ├── CandidateSourceDocumentArtifact?
        ├── TextProcessingModuleError?
        ├── RetryHint?
        └── DiagnosticsRef?
```

Publication flow:

```text
CandidateSourceDocumentArtifact
        ↓
Runtime Authority Validation
        ↓
Artifact Store Ownership Transfer
        ↓
SourceDocumentArtifact
```

Text Processing không expose application-level commands như:

```text
CancelTextProcessing

RetryTextProcessing

PublishSourceDocument
```

Những action này thuộc Runtime hoặc Artifact Store.

---

# 3. Contract Principles

## 3.1 Stable Source Representation

`SourceDocument` phải ổn định độc lập với:

* Translation Provider
* Translation model
* target language
* prompt strategy
* token limits
* batching
* pricing
* context-window strategy

---

## 3.2 Raw Source Preservation

Text Processing không overwrite raw recognized text.

```text
RawText
    remains available

NormalizedText
    is derived separately
```

---

## 3.3 Explicit Traceability

Mọi textual output phải map ngược được về source OCR evidence.

---

## 3.4 OCR Semantic Reuse

Text Processing reference canonical OCR contracts.

Nó không redefine:

```text
Region
Line
Paragraph
ReadingOrder
Layout
Quality
Geometry
```

---

## 3.5 Translation Independence

Text Processing không tạo:

```text
TranslationUnit[]
```

Translation segmentation thuộc Translation Module.

---

## 3.6 Immutable Publication

Candidate immutable sau module validation.

Published SourceDocument Artifact immutable sau publication.

---

## 3.7 Authority Separation

Text Processing không quyết định:

* Revision còn current hay không
* Attempt còn authority hay không
* retry
* publication
* downstream Translation scheduling

---

## 3.8 Explicit Uncertainty

Uncertain decisions phải giữ explicit.

Không fabricate:

* classification
* grouping
* hierarchy
* language
* reconstruction confidence

---

## 3.9 Conservative Reconstruction

Khi không đủ evidence:

```text
preserve separate source structures
```

thay vì destructive merge.

---

# 4. Contract Version

```text
TextProcessingContractVersion
├── Major
├── Minor
└── Patch
```

Initial:

```text
1.0.0
```

Semantics:

* Major = incompatible semantic change
* Minor = backward-compatible addition
* Patch = clarification/non-semantic correction

---

# 5. Shared Types

Text Processing sử dụng shared/runtime/artifact identifiers:

```text
SessionId
RevisionId
WorkItemId
AttemptId

ArtifactId
CandidateArtifactId

ConfigurationSnapshotId

TraceId
```

Text Processing không redefine identifier semantics.

Identifiers là opaque.

---

# 6. Shared Scalar Types

Common scalar/reference contracts:

```text
Timestamp
Duration
LanguageCode
ScriptCode
Metadata
ArtifactRef
GeometryRef
EntityRef
```

Recommended standards:

```text
Timestamp
    → ISO-8601 UTC

LanguageCode
    → BCP-47 compatible

ScriptCode
    → ISO-15924 compatible
```

---

# 7. Runtime Context

```text
TextProcessingRuntimeContext
├── ContractVersion
├── ApplicationInstanceId
├── SessionId?
├── RevisionId
├── WorkItemId
├── AttemptId
├── ConfigurationSnapshotId
├── TraceContext
└── CreatedAt
```

Rules:

1. `RevisionId`, `WorkItemId`, `AttemptId` required.
2. `SessionId` optional.
3. Queue priority không thuộc module contract.
4. Retry count không thuộc module contract.
5. Runtime identity không cấp authority cho module.

---

# 8. Recognition Artifact Reference

Primary upstream input:

```text
RecognitionArtifactRef
├── ArtifactId
├── ArtifactType
├── ContractVersion
├── ContentIdentity
├── OCRDocumentRef
├── ReadingOrderResultRef?
├── QualityReportRef?
└── Metadata?
```

Text Processing consume published Recognition Artifact.

Candidate Recognition Artifact không phải normal public input.

---

# 9. Recognition Input Rules

Recognition Artifact phải:

1. immutable
2. resolvable
3. contract-compatible
4. source-traceable
5. thuộc compatible Privacy Partition
6. có valid `OCRDocumentRef`
7. giữ required source identity

Optional:

```text
ReadingOrderResultRef

QualityReportRef
```

có thể thiếu nếu Processing Profile cho phép.

---

# 10. OCR Document Reference

```text
OCRDocumentRef
├── ArtifactId
├── ContractVersion
├── ContentIdentity?
└── Revision?
```

OCR Document semantics thuộc:

```text
01-architecture/ocr/POSTPROCESS.md
```

Text Processing không redefine OCR Document structure trong contract này.

---

# 11. Reading Order Reference

```text
ReadingOrderResultRef
├── ArtifactId
├── ContractVersion
└── ContentIdentity?
```

Reading Order semantics thuộc:

```text
01-architecture/ocr/READING_ORDER.md
```

Text Processing sử dụng Reading Order làm source sequencing evidence.

---

# 12. Quality Report Reference

```text
QualityReportRef
├── ArtifactId
├── ContractVersion
└── ContentIdentity?
```

Quality semantics thuộc:

```text
01-architecture/ocr/QUALITY.md
```

Text Processing không redefine:

* Quality Score
* Quality Grade
* OCR confidence semantics

---

# 13. Text Processing Attempt Input

```text
TextProcessingAttemptInput
├── RuntimeContext
├── RecognitionArtifactRef
├── ProcessingProfileRef
├── ProcessingOptions
├── SourceContext?
├── ExecutionContextRef
├── CancellationContextRef
├── PrivacyContextRef
└── DiagnosticsContextRef?
```

---

# 14. Attempt Input Preconditions

Input hợp lệ khi:

1. Runtime Context hợp lệ.
2. Recognition Artifact resolvable.
3. Recognition contract compatible.
4. OCRDocumentRef resolvable.
5. Processing Profile hợp lệ.
6. Processing Options hợp lệ.
7. Privacy Context compatible.
8. source identity consistent.
9. contract major version supported.

Text Processing không kiểm tra:

```text
Is this Revision still authoritative?
```

Runtime sở hữu authority validation.

---

# 15. Processing Profile Reference

```text
ProcessingProfileRef
├── ProfileId
├── ProfileVersion
└── ConfigurationRef?
```

Recommended profile IDs:

```text
COMIC_PAGE

COMIC_REGION

NOVEL_PAGE

NOVEL_PARAGRAPH

WEB_TEXT

INTERFACE_TEXT

GENERIC_DOCUMENT
```

---

# 16. Processing Profile Semantics

Processing Profile có thể định nghĩa:

```text
NormalizationPolicy

LineReconstructionPolicy

GroupingPolicy

ClassificationPolicy

ExclusionPolicy

BlockConstructionPolicy

SequenceMappingPolicy
```

Không được chứa:

```text
TargetLanguage

TranslationProvider

TranslationModel

PromptTemplate

TokenLimit

TranslationBatchSize

TranslationTemperature
```

---

# 17. Processing Options

```text
ProcessingOptions
├── PreserveRawText
├── AllowPartialCandidate
├── EnableNormalization
├── EnableReconstruction
├── EnableGrouping
├── EnableClassification
├── EnableExclusion
├── PreferredStructureMode
├── DiagnosticLevel
└── ExtensionOptions?
```

Recommended default:

```text
PreserveRawText = true
```

Không được disable source traceability.

---

# 18. Structure Mode

```text
SourceStructureMode
├── FLAT
├── HIERARCHICAL
└── AUTO
```

MVP default:

```text
FLAT
```

Hierarchy chỉ được tạo khi có sufficient evidence.

---

# 19. Diagnostic Level

```text
DiagnosticLevel
├── NONE
├── BASIC
├── DETAILED
└── PROTECTED_CONTENT
```

`PROTECTED_CONTENT` yêu cầu explicit authorization.

---

# 20. Source Context

Optional:

```text
SourceContext
├── SourceId?
├── ContentId?
├── FrameId?
├── PageIndex?
├── ChapterId?
├── DocumentTypeHint?
├── ExpectedLanguage?
├── PreviousSourceDocumentRef?
└── Metadata?
```

Các field này mặc định là hints trừ khi contract khác đánh dấu authoritative.

---

# 21. Execution Context Reference

```text
ExecutionContextRef
├── ExecutionClass
├── Deadline?
├── ResourceBudgetRef?
└── RuntimePolicyRef?
```

Text Processing chỉ consume.

---

# 22. Cancellation Context Reference

```text
CancellationContextRef
├── CancellationId
├── IsCancellationRequested
├── RequestedAt?
├── Reason?
└── CheckpointPolicyRef?
```

Text Processing cooperative-check context này.

Nó không sở hữu canonical cancellation state.

---

# 23. Privacy Context

```text
PrivacyContextRef
├── PrivacyMode
├── PrivacyPartition
├── DiagnosticContentAllowed
├── PersistenceAllowed
└── ExportAllowed?
```

Recommended modes:

```text
STANDARD
LOCAL_ONLY
EPHEMERAL
```

---

# 24. Internal Input Adaptation Contract

Public Recognition Artifact được adapt thành internal:

```text
ProcessingInputDocument
```

Đây không phải public Artifact.

Conceptually:

```text
ProcessingInputDocument
├── SourceIdentity
├── RecognitionArtifactRef
├── OCRDocumentRef
├── ReadingOrderResultRef?
├── QualityReportRef?
├── TextNodes[]
├── StructuralHints[]
├── LanguageHints[]
├── UpstreamWarnings[]
└── Metadata
```

---

# 25. Text Node

Internal conceptual model:

```text
TextNode
├── NodeId
├── OCRSourceRefs[]
├── RawText
├── NormalizedText?
├── GeometryRefs[]
├── DirectionRef?
├── LayoutRef?
├── SequenceRefs[]
├── LanguageHint?
├── Warnings[]
└── Metadata
```

`TextNode` không phải public SourceDocument model.

---

# 26. SourceDocument

Canonical Text Processing output:

```text
SourceDocument
├── DocumentId
├── ContractVersion
├── DocumentVersion
├── DocumentType
├── SourceIdentity
├── RecognitionArtifactRef
├── RootBlockIds[]
├── Blocks[]
├── BlockSequence[]
├── ExcludedBlocks[]
├── LanguageHints[]
├── Warnings[]
├── ReconstructionMetadata
├── CompatibilityMetadata
└── Metadata
```

---

# 27. SourceDocument Rules

1. `DocumentId` unique.
2. Document immutable sau publication.
3. Every block ID unique.
4. RootBlockId phải reference existing block.
5. Hierarchy acyclic.
6. Every textual block must have source evidence.
7. Raw text preserved.
8. Normalized text must be derivable.
9. Translation-specific metadata forbidden.
10. Provider-native OCR metadata không được trở thành core dependency.

---

# 28. Document Type

Recommended:

```text
SourceDocumentType
├── COMIC_PAGE
├── COMIC_REGION
├── NOVEL_PAGE
├── NOVEL_PARAGRAPH
├── WEB_TEXT
├── INTERFACE_TEXT
├── GENERIC_DOCUMENT
└── UNKNOWN
```

Document Type không quyết định Translation Provider.

---

# 29. SourceBlock

```text
SourceBlock
├── BlockId
├── ParentBlockId?
├── ChildBlockIds[]
├── BlockType
├── RawText
├── NormalizedText
├── OCRSourceRefs[]
├── GeometryRefs[]
├── SequenceIndex?
├── LanguageHint?
├── ReconstructionConfidence?
├── Classification?
├── NormalizationChanges[]
├── ReconstructionDecisions[]
├── GroupingDecisions[]
├── Warnings[]
└── Metadata
```

---

# 30. SourceBlock Types

Recommended initial enum:

```text
PARAGRAPH

DIALOGUE

NARRATION

THOUGHT

CAPTION

HEADING

SOUND_EFFECT

ANNOTATION

INTERFACE_TEXT

PAGE_NUMBER

WATERMARK

UNKNOWN
```

Optional structural:

```text
PAGE

PANEL

SECTION

CONTAINER
```

---

# 31. Block Type Rules

Block classification:

* structural only
* source-oriented
* non-destructive
* uncertainty-aware

Không được infer story semantics không có evidence.

Unknown hợp lệ.

---

# 32. RawText

`RawText` là source text được preserve từ upstream recognized evidence.

Rules:

1. không semantic rewrite
2. không overwrite bởi normalization
3. không translate
4. không silently fix OCR
5. không fabricate punctuation

---

# 33. NormalizedText

`NormalizedText` là deterministic surface-normalized representation.

```text
NormalizedText
    derives from
RawText
```

Normalization có thể bao gồm:

* Unicode normalization
* control character removal
* whitespace normalization
* separator normalization
* safe width normalization
* safe punctuation formatting

---

# 34. Normalization Change

```text
NormalizationChange
├── ChangeId
├── RuleId
├── RuleVersion
├── ChangeType
├── SourceRange?
├── Before?
├── After?
├── Reversible
└── Metadata?
```

`Before`/`After` có thể bị omit ở privacy-safe transport view.

---

# 35. Normalization Change Types

Examples:

```text
UNICODE_NORMALIZED

WHITESPACE_COLLAPSED

CONTROL_CHARACTER_REMOVED

LINE_SEPARATOR_NORMALIZED

WIDTH_NORMALIZED

SAFE_PUNCTUATION_SPACING
```

Semantic correction không thuộc default normalization.

---

# 36. Reconstruction Decision

```text
ReconstructionDecision
├── DecisionId
├── DecisionType
├── InputSourceRefs[]
├── OutputBlockRef?
├── Evidence[]
├── Confidence?
├── RuleId?
├── RuleVersion?
└── Metadata?
```

---

# 37. Reconstruction Decision Types

```text
PRESERVE

JOIN_LINES

JOIN_FRAGMENTS

SPLIT_GROUP

PARAGRAPH_BOUNDARY

COLUMN_CONTINUATION

PRESERVE_SEPARATE
```

---

# 38. Reconstruction Confidence

`ReconstructionConfidence` chỉ phản ánh confidence của Text Processing reconstruction.

Nó không thay thế:

* Detection Confidence
* Recognition Confidence
* Direction Confidence
* Quality Score
* Reading Confidence

Recommended normalized representation:

```text
value: 0.0 .. 1.0
```

Optional.

Unknown phải được phép.

---

# 39. Grouping Decision

```text
GroupingDecision
├── DecisionId
├── InputRefs[]
├── ResultGroupRef
├── Method
├── Evidence[]
├── Confidence?
├── RuleId?
└── Metadata?
```

---

# 40. Grouping Methods

Recommended:

```text
REGION_PRESERVING

LAYOUT_CONTAINER_BASED

PARAGRAPH_BASED

COLUMN_BASED

GEOMETRY_ASSISTED

HYBRID

MANUAL_HINT
```

---

# 41. Grouping Safety

Khi confidence không đủ:

```text
PRESERVE_SEPARATE
```

là canonical conservative fallback.

---

# 42. Block Classification

```text
BlockClassification
├── BlockType
├── Confidence?
├── Method
├── Evidence[]
├── Alternatives[]
└── RuleRef?
```

---

# 43. Classification Alternative

```text
ClassificationAlternative
├── BlockType
└── Confidence?
```

Classification uncertainty không làm invalid SourceDocument.

---

# 44. OCR Source Reference

```text
OCRSourceRef
├── OCRDocumentRef
├── RegionRef?
├── ParagraphRef?
├── LineRef?
├── WordRefs[]?
├── CharacterRefs[]?
├── GeometryRefs[]
└── Metadata?
```

Text Processing không redefine semantics của các referenced OCR entities.

---

# 45. Source Traceability Rule

Every non-empty SourceBlock must satisfy:

```text
SourceBlock
    ↓
OCRSourceRef[]
    ↓
OCRDocument
```

Nếu traceability không thể đảm bảo:

```text
Candidate invalid
```

---

# 46. Geometry Reference

`GeometryRef` reference canonical OCR/shared geometry.

Text Processing không copy/redefine:

```text
Rectangle

Polygon

CoordinateSpace
```

trong contract này.

---

# 47. Source Block Sequence

```text
SourceBlockSequenceEntry
├── Index
├── BlockId
├── SourceOrderRefs[]
├── ReconstructionMethod
├── Confidence?
└── Metadata?
```

---

# 48. Sequence Semantics

`BlockSequence` định nghĩa sequence của **SourceBlocks sau reconstruction**.

Nó không phải canonical OCR Reading Order.

Example:

```text
OCR Reading Order:
    Region A
    Region B
    Region C

Text Processing:
    A + B → Block X
    C     → Block Y

Source Block Sequence:
    X
    Y
```

---

# 49. Sequence Rules

1. Index stable within Document revision.
2. BlockId unique in sequence unless explicitly supported otherwise.
3. Reference existing SourceBlock.
4. SourceOrderRefs preserve derivation.
5. Unknown sequence may remain partial.
6. Text Processing không resolve page ordering from scratch when canonical Reading Order exists.

---

# 50. Block Exclusion

```text
BlockExclusion
├── BlockId
├── Reason
├── Confidence?
├── RuleId?
├── RuleVersion?
├── Reversible
├── Evidence[]
└── Metadata?
```

---

# 51. Exclusion Reasons

Recommended:

```text
LIKELY_PAGE_NUMBER

REPEATED_HEADER

LIKELY_WATERMARK

INTERFACE_NOISE

LOW_CONFIDENCE_GARBAGE

PROFILE_EXCLUDED_TYPE

EXPLICIT_USER_EXCLUSION
```

---

# 52. Exclusion Rules

1. Exclusion does not delete SourceBlock.
2. Excluded blocks remain traceable.
3. Uncertain exclusion defaults to included.
4. Exclusion should be reversible.
5. Translation decides whether excluded blocks become Translation Units.
6. Text Processing does not delete source evidence.

---

# 53. Synthetic Structural Block

A SourceBlock may be structural without direct text.

Example:

```text
PANEL

PAGE

SECTION
```

Such block should contain:

```text
Synthetic = true
```

or equivalent metadata.

Its lineage derives from:

* child blocks
* canonical Layout references

Synthetic block may not claim invented RawText.

---

# 54. Language Hint

```text
SourceLanguageHint
├── LanguageCode
├── ScriptCode?
├── ConfidenceRef?
├── Source
├── ScopeRefs[]
└── Metadata?
```

Language hints remain hints unless explicitly authoritative.

Text Processing does not need to perform language detection in MVP.

---

# 55. Source Identity

```text
SourceIdentity
├── ContentIdentity
├── RecognitionArtifactRef
├── SourceScope
├── PageIndex?
├── ChapterId?
└── Metadata?
```

Source identity must remain stable across reconstruction.

---

# 56. Reconstruction Metadata

```text
ReconstructionMetadata
├── ProcessingProfileId
├── ProcessingProfileVersion
├── RuleSetVersions[]
├── ProcessingStrategyVersion
├── StructureMode
├── ProcessingFingerprint?
└── Metadata?
```

---

# 57. Processing Fingerprint

Conceptual identity:

```text
RecognitionSemanticIdentity
+
ProcessingProfileVersion
+
ProcessingOptions
+
RuleSetVersions
+
ProcessingStrategyVersion
```

produces:

```text
ProcessingFingerprint
```

Fingerprint supports compatibility.

It does not own cache policy.

---

# 58. Completeness

```text
TextProcessingCompleteness
├── COMPLETE
├── PARTIAL
├── EMPTY_VALID
└── UNKNOWN
```

---

# 59. COMPLETE

Required processable source content represented.

---

# 60. PARTIAL

Some source content could not be represented, but remaining document is usable and traceable.

Must include warnings.

---

# 61. EMPTY_VALID

Upstream processing is valid but no processable source text exists.

Example:

```text
Blocks = []

BlockSequence = []

Completeness = EMPTY_VALID
```

Not failure.

---

# 62. UNKNOWN

Completeness cannot be safely determined.

No automatic interpretation.

---

# 63. Candidate SourceDocument Artifact

```text
CandidateSourceDocumentArtifact
├── CandidateArtifactId
├── ArtifactType
├── OwnerModule
├── ContractVersion
├── RecognitionArtifactRef
├── SourceContentIdentity
├── SourceDocument
├── Completeness
├── Warnings[]
├── CompatibilityMetadata
├── TraceabilityMetadata
└── IntegrityMetadata
```

Alternative implementation may use:

```text
SourceDocumentRef
```

instead of embedded SourceDocument when artifact payload separation is needed.

Public semantics remain equivalent.

---

# 64. Candidate Rules

1. Candidate non-authoritative.
2. Candidate not published.
3. Candidate immutable after validation.
4. Candidate private to Runtime completion path.
5. Candidate no retry count.
6. Candidate no queue state.
7. Candidate no Attempt terminal state.
8. Candidate no Translation data.
9. Candidate cleanup required after rejection.

---

# 65. Published SourceDocument Artifact

```text
SourceDocumentArtifact
├── ArtifactId
├── ArtifactType
├── ContractVersion
├── RecognitionArtifactRef
├── SourceContentIdentity
├── SourceDocument
├── Completeness
├── Warnings[]
├── CompatibilityMetadata
├── TraceabilityMetadata
└── IntegrityMetadata
```

Published Artifact is:

* immutable
* Runtime accepted
* Artifact Store owned
* Translation-provider independent
* Recognition traceable
* reusable only when compatibility permits

---

# 66. Artifact Type

Recommended:

```text
SOURCE_DOCUMENT_ARTIFACT
```

Candidate và Published Artifact cùng semantic type nhưng khác authority/lifecycle.

---

# 67. Traceability Metadata

```text
TextProcessingTraceabilityMetadata
├── RecognitionArtifactRef
├── OCRDocumentRef
├── ReadingOrderResultRef?
├── QualityReportRef?
├── ConfigurationSnapshotId
├── ProcessingProfileRef
├── RuleSetVersions[]
└── TraceId?
```

---

# 68. Compatibility Metadata

```text
TextProcessingCompatibilityMetadata
├── InputContentIdentity
├── RecognitionArtifactContractVersion
├── OCRDocumentContractVersion
├── TextProcessingContractVersion
├── ProcessingProfileVersion
├── ProcessingStrategyVersion
├── RuleSetVersions[]
├── ProcessingOptionsFingerprint
├── PrivacyPartition
└── SemanticDependencies[]
```

---

# 69. Compatibility Evaluation

Two SourceDocument Artifacts may be semantically compatible when:

* input semantic identity matches
* major contracts compatible
* Processing Profile compatible
* Processing Options compatible
* relevant Rule versions compatible
* semantic dependencies compatible
* Privacy Partition compatible

`RevisionId` alone không phải reuse identity.

---

# 70. Compatibility vs Cache

Text Processing defines:

```text
Can these results be considered semantically equivalent?
```

Runtime Cache Policy decides:

```text
Should an existing result be reused?
```

Artifact Store decides:

```text
How long does the Artifact remain available?
```

---

# 71. Text Processing Attempt Output

```text
TextProcessingAttemptOutput
├── CandidateArtifact?
├── ModuleWarnings[]
├── ModuleError?
├── RetryHint?
├── DiagnosticsRef?
└── CompletionMetadata
```

---

# 72. Completion Metadata

```text
TextProcessingCompletionMetadata
├── StartedAt
├── CompletedAt
├── OperationPhase
├── ExecutionMetricsRef?
├── CancellationObserved
└── Metadata?
```

Completion metadata là Attempt-local.

Không mặc định copy vào published SourceDocument.

---

# 73. Module Warning

```text
TextProcessingWarning
├── WarningCode
├── Severity
├── OperationPhase
├── MessageKey
├── SourceScopeRef?
├── EvidenceRefs[]
├── Metadata?
└── RecordedAt
```

---

# 74. Warning Severity

```text
INFORMATION

DEGRADED

ATTENTION_REQUIRED
```

Warnings không invalidate Candidate.

---

# 75. Recommended Warning Codes

```text
NO_PROCESSABLE_TEXT

PARTIAL_SOURCE_DOCUMENT

NORMALIZATION_SKIPPED

RECONSTRUCTION_UNCERTAIN

GROUPING_UNCERTAIN

CLASSIFICATION_UNCERTAIN

STRUCTURE_FLATTENED

OPTIONAL_READING_ORDER_UNAVAILABLE

OPTIONAL_QUALITY_REPORT_UNAVAILABLE

BLOCK_EXCLUSION_UNCERTAIN

UPSTREAM_WARNING_PRESERVED
```

---

# 76. Warning Semantics

Examples:

```text
RECONSTRUCTION_UNCERTAIN
    → preserve separate blocks
```

```text
GROUPING_UNCERTAIN
    → conservative grouping
```

```text
CLASSIFICATION_UNCERTAIN
    → BlockType = UNKNOWN
```

---

# 77. Module Error

```text
TextProcessingModuleError
├── ContractVersion
├── ErrorCode
├── OperationPhase
├── MessageKey
├── RetryHint?
├── AffectedScopeRef?
├── UpstreamErrorRef?
├── DiagnosticsRef?
├── Metadata?
└── OccurredAt
```

Detailed taxonomy belongs in:

```text
ERRORS.md
```

---

# 78. Recommended Module Error Codes

At contract level:

```text
TEXT_PROCESSING_INPUT_INVALID

TEXT_PROCESSING_UPSTREAM_ARTIFACT_INCOMPATIBLE

TEXT_PROCESSING_PROFILE_INVALID

TEXT_PROCESSING_SOURCE_ADAPTATION_FAILED

TEXT_PROCESSING_NORMALIZATION_FAILED

TEXT_PROCESSING_RECONSTRUCTION_FAILED

TEXT_PROCESSING_GROUPING_FAILED

TEXT_PROCESSING_CLASSIFICATION_FAILED

TEXT_PROCESSING_DOCUMENT_BUILD_FAILED

TEXT_PROCESSING_TRACEABILITY_FAILED

TEXT_PROCESSING_CANDIDATE_INVALID

TEXT_PROCESSING_PRIVACY_VIOLATION

TEXT_PROCESSING_RESOURCE_EXHAUSTED

TEXT_PROCESSING_INTERNAL_ERROR
```

---

# 79. Retry Hint

```text
TextProcessingRetryHint
├── Retryability
├── SuggestedStrategies[]
├── ReasonCode
└── Metadata?
```

```text
Retryability
├── RETRYABLE
├── CONDITIONALLY_RETRYABLE
└── NON_RETRYABLE
```

Suggested strategies:

```text
SAME_PROFILE

CONSERVATIVE_PROFILE

DISABLE_OPTIONAL_GROUPING

DISABLE_OPTIONAL_CLASSIFICATION

FLAT_STRUCTURE

RESOURCE_WAIT

NO_RETRY
```

Runtime decides whether retry actually occurs.

---

# 80. Operation Phase

Diagnostic phase enum:

```text
TextProcessingOperationPhase
├── VALIDATING
├── ADAPTING_INPUT
├── NORMALIZING
├── RECONSTRUCTING
├── GROUPING
├── CLASSIFYING
├── BUILDING_DOCUMENT
├── VALIDATING_TRACEABILITY
├── ASSEMBLING_CANDIDATE
└── FINALIZING
```

These are not Runtime Attempt states.

---

# 81. Candidate Validation

Candidate validation checks:

* CandidateArtifactId
* ArtifactType
* owner module
* RecognitionArtifactRef
* SourceDocument contract
* Completeness consistency
* CompatibilityMetadata
* TraceabilityMetadata
* IntegrityMetadata
* privacy compliance
* no Translation-specific content
* no Runtime terminal state

---

# 82. SourceDocument Validation

Must verify:

1. DocumentId present.
2. Block IDs unique.
3. RootBlock refs valid.
4. Child refs valid.
5. Parent refs valid.
6. hierarchy acyclic.
7. BlockSequence refs valid.
8. exclusion refs valid.
9. RawText source evidence valid.
10. NormalizedText traceable.
11. OCR source refs resolvable.
12. source identity consistent.
13. no translated text.
14. no Provider SDK object.

---

# 83. Normalization Validation

For every changed NormalizedText:

```text
NormalizationChange[]
```

must be attributable when trace mode/profile requires it.

RawText must remain available in canonical Artifact unless an explicitly approved privacy projection removes it from a transport view.

---

# 84. Hierarchy Validation

Forbidden:

```text
Block A
    → child Block B

Block B
    → child Block A
```

All hierarchy graphs must be acyclic.

---

# 85. Sequence Validation

`BlockSequence` must:

* reference existing SourceBlock
* use valid indices
* contain no unintended duplicates
* remain deterministic
* preserve source-order derivation references where available

---

# 86. Input Invalid Conditions

Reject input when:

* unsupported contract major
* RecognitionArtifactRef missing
* Recognition Artifact unresolved
* OCRDocumentRef missing
* source identity contradictory
* invalid Processing Profile
* invalid Processing Options
* privacy conflict
* required upstream artifact incompatible

Do not revalidate every OCR internal entity unless needed to establish referenced artifact contract validity.

---

# 87. Empty Document Contract

Canonical empty result:

```text
SourceDocument
├── Blocks = []
├── RootBlockIds = []
├── BlockSequence = []
└── ExcludedBlocks = []

Completeness = EMPTY_VALID
```

Recommended warning:

```text
NO_PROCESSABLE_TEXT
```

---

# 88. Partial Document Contract

When partial output allowed:

```text
Completeness = PARTIAL
```

Requirements:

* usable blocks remain
* omitted/failed scopes explicit
* source traceability preserved
* no hidden data loss
* warning emitted
* Candidate remains contract-valid

---

# 89. Source Correction Boundary

Published SourceDocument must not be mutated by user correction.

Recommended future contract:

```text
SourceCorrectionSet
├── CorrectionId
├── SourceDocumentRef
├── Operations[]
├── Author
└── CreatedAt
```

Correction concern is outside this contract.

---

# 90. Translation Boundary

Translation consumes:

```text
SourceDocumentArtifact
```

Text Processing Contract does not expose:

```text
TranslationUnit

TranslationBatch

TranslationPrompt

TargetLanguagePlan

ProviderTokenBudget

TranslatedSegment
```

---

# 91. Translation Consumer Obligations

Translation must:

1. treat SourceDocument immutable
2. preserve SourceBlock identity
3. preserve source alignment
4. decide Translation Units itself
5. not mutate normalized source text
6. not write translated content back into SourceDocument
7. handle excluded blocks according to Translation policy
8. preserve source traceability

---

# 92. Presentation Consumer Boundary

Presentation may use:

* SourceBlock refs
* OCR source refs
* geometry refs
* SourceDocument structure

Presentation must not mutate SourceDocument.

---

# 93. Producer Obligations

Text Processing implementation must:

1. validate Attempt Input
2. preserve Runtime identity
3. consume published Recognition Artifact
4. preserve raw text
5. normalize non-destructively
6. preserve OCR lineage
7. use canonical Reading Order when available
8. avoid redefining OCR semantics
9. represent uncertainty explicitly
10. use conservative grouping
11. build valid SourceDocument
12. validate traceability
13. build immutable Candidate
14. enforce Privacy Context
15. return stable warnings/errors
16. release Attempt-local resources
17. never retry itself
18. never publish accepted Artifact
19. never invoke Translation implementation
20. never store translated text in SourceDocument

---

# 94. Consumer Obligations

Consumers must:

1. honor Artifact immutability
2. handle EMPTY_VALID
3. handle PARTIAL
4. preserve SourceBlock identity
5. preserve OCR traceability
6. treat `UNKNOWN` classification safely
7. not assume hierarchy exists
8. not assume every block has geometry
9. not assume every block has confidence
10. not infer OCR Reading Order from BlockSequence
11. not mutate SourceDocument for correction
12. respect ExcludedBlocks semantics

---

# 95. Runtime Obligations

Runtime must:

1. create WorkItem/Attempt identity
2. supply immutable RecognitionArtifactRef
3. provide ExecutionContext
4. provide CancellationContext
5. own deadline
6. own Scheduler admission
7. own retry
8. own authority validation
9. own terminal Attempt outcome
10. coordinate Candidate cleanup
11. transfer accepted Candidate
12. reject stale Candidate
13. provide Cache Policy
14. preserve Revision authority

---

# 96. Artifact Store Obligations

Artifact Store must:

* receive accepted Candidate transfer
* assign ArtifactId
* publish atomically
* own published payload lifecycle
* provide immutable lookup
* manage leases/retention
* reject invalid duplicate publication
* clean failed transfer

---

# 97. Cancellation Contract

When cancellation observed:

Text Processing should:

* stop starting new expensive work
* stop optional processing
* avoid Candidate submission when authority no longer valid
* release local resources
* return cancellation observation

Text Processing does not set:

```text
ATTEMPT_CANCELED
```

Runtime owns terminal state.

---

# 98. Deadline Contract

Runtime deadline may terminate useful processing budget.

Text Processing may:

* observe deadline
* stop optional work
* return module error/hint
* cleanup

It does not own global timeout semantics.

---

# 99. Privacy Contract

Normal module outputs/logs/events must not expose:

```text
full raw OCR text in logs

full normalized document in logs

image bytes

translated text

credentials

authorization headers

protected diagnostic samples
```

Artifact content may contain raw/normalized source text according to Privacy Policy.

---

# 100. Privacy Projection

A transport/view may omit sensitive fields such as:

```text
RawText

NormalizationChange.Before

NormalizationChange.After
```

only if:

* canonical Artifact retains required traceability
* projection is explicit
* consumer understands omitted fields
* contract view/version identifies projection

---

# 101. Serialization

Recommended:

```text
In-process
    → typed objects

Cross-process
    → Protocol Buffers / JSON / MessagePack
```

Large SourceDocument should use Artifact references rather than Event Bus payloads.

---

# 102. Provider Independence

Although Text Processing itself does not use Translation Provider, upstream OCR Provider may differ.

`SourceDocument` semantics must remain stable across equivalent Recognition Artifacts regardless of OCR provider when normalized OCR contracts are semantically equivalent.

---

# 103. Determinism

Given the same:

```text
Recognition semantic input
+
Processing Profile
+
Processing Options
+
Rule versions
+
Processing Strategy version
```

Text Processing should produce structurally equivalent SourceDocument.

Generated IDs/timestamps may differ if excluded from semantic comparison.

---

# 104. Contract Evolution

Backward-compatible changes:

* optional fields
* new warning codes
* new module errors
* new optional BlockType
* new optional metadata
* new Processing Profile
* new diagnostic references
* new optional reconstruction evidence

Breaking changes:

* changing SourceBlock meaning
* changing RawText/NormalizedText semantics
* changing OCR traceability guarantees
* changing BlockSequence semantics
* removing required lineage
* changing Candidate/publication boundary
* changing authority ownership
* changing Privacy guarantee
* changing Translation boundary

Breaking changes require new major version.

---

# 105. Unknown Values

Consumers must:

* tolerate unknown additive enums
* preserve unknown metadata when possible
* fall back safely
* reject unsupported major version
* not fabricate meaning

Examples:

```text
unknown BlockType
    → treat as UNKNOWN-compatible

unknown warning
    → preserve/report

unknown reconstruction method
    → preserve SourceBlock content
```

---

# 106. Example Attempt Input

```json
{
  "runtime_context": {
    "contract_version": "1.0.0",
    "session_id": "session_01",
    "revision_id": "revision_104",
    "work_item_id": "work_text_processing_104",
    "attempt_id": "attempt_01",
    "configuration_snapshot_id": "config_42"
  },
  "recognition_artifact_ref": {
    "artifact_id": "recognition_artifact_104",
    "artifact_type": "RECOGNITION_ARTIFACT",
    "contract_version": "1.1"
  },
  "processing_profile_ref": {
    "profile_id": "COMIC_PAGE",
    "profile_version": "1"
  },
  "processing_options": {
    "preserve_raw_text": true,
    "allow_partial_candidate": true,
    "enable_normalization": true,
    "enable_reconstruction": true,
    "enable_grouping": true,
    "enable_classification": true,
    "enable_exclusion": true,
    "preferred_structure_mode": "FLAT",
    "diagnostic_level": "BASIC"
  }
}
```

---

# 107. Example SourceDocument

```json
{
  "document_id": "source_document_104",
  "contract_version": "1.0.0",
  "document_version": 1,
  "document_type": "COMIC_PAGE",
  "source_identity": {
    "recognition_artifact_id": "recognition_artifact_104"
  },
  "root_block_ids": [
    "block_01",
    "block_02"
  ],
  "blocks": [
    {
      "block_id": "block_01",
      "block_type": "DIALOGUE",
      "raw_text": "你好！",
      "normalized_text": "你好！",
      "ocr_source_refs": [
        {
          "ocr_document_ref": {
            "artifact_id": "ocr_document_104"
          },
          "region_ref": "region_01",
          "line_ref": "line_01"
        }
      ],
      "sequence_index": 0,
      "warnings": []
    },
    {
      "block_id": "block_02",
      "block_type": "NARRATION",
      "raw_text": "第二天",
      "normalized_text": "第二天",
      "ocr_source_refs": [
        {
          "ocr_document_ref": {
            "artifact_id": "ocr_document_104"
          },
          "region_ref": "region_02"
        }
      ],
      "sequence_index": 1,
      "warnings": []
    }
  ],
  "block_sequence": [
    {
      "index": 0,
      "block_id": "block_01",
      "source_order_refs": ["reading_node_01"],
      "reconstruction_method": "CANONICAL_ORDER_MAPPING"
    },
    {
      "index": 1,
      "block_id": "block_02",
      "source_order_refs": ["reading_node_02"],
      "reconstruction_method": "CANONICAL_ORDER_MAPPING"
    }
  ],
  "excluded_blocks": [],
  "warnings": []
}
```

---

# 108. Example Candidate Artifact

```json
{
  "candidate_artifact_id": "candidate_source_document_104",
  "artifact_type": "SOURCE_DOCUMENT_ARTIFACT",
  "owner_module": "text-processing",
  "contract_version": "1.0.0",
  "recognition_artifact_ref": {
    "artifact_id": "recognition_artifact_104"
  },
  "source_content_identity": {
    "identity_algorithm": "sha256",
    "identity_version": "1",
    "value": "content_identity_redacted"
  },
  "source_document": {
    "document_id": "source_document_104",
    "document_version": 1,
    "document_type": "COMIC_PAGE"
  },
  "completeness": "COMPLETE",
  "warnings": [],
  "compatibility_metadata": {
    "processing_profile_version": "1",
    "processing_strategy_version": "1"
  }
}
```

---

# 109. Contract Test Requirements — Input

Test:

* valid Recognition Artifact
* missing Recognition Artifact
* unresolved OCRDocumentRef
* unsupported Recognition contract
* unsupported Text Processing contract
* invalid Profile
* invalid Options
* privacy conflict
* incompatible source identity

---

# 110. Contract Test Requirements — SourceDocument

Test:

* valid flat document
* valid hierarchy
* duplicate block ID
* missing root block
* cyclic hierarchy
* invalid BlockSequence ref
* invalid ExcludedBlock ref
* text without OCR lineage
* normalization without raw evidence
* empty-valid document
* partial document
* unknown classification

---

# 111. Contract Test Requirements — Candidate

Test:

* valid Candidate
* missing CandidateArtifactId
* wrong ArtifactType
* missing RecognitionArtifactRef
* invalid SourceDocument
* invalid Completeness
* missing CompatibilityMetadata
* missing TraceabilityMetadata
* translated text leakage
* Runtime state leakage
* privacy violation

---

# 112. Contract Test Requirements — Runtime Boundary

Test:

```text
Candidate valid
    → Runtime accepts
    → Artifact published
```

```text
Candidate valid
    → Revision stale
    → Runtime rejects
```

```text
Cancellation observed
    → local processing ends
    → module does not publish
```

```text
retry requested externally
    → new Attempt
    → original SourceDocument unchanged
```

---

# 113. Contract Test Requirements — Translation Boundary

Verify:

* SourceDocument contains no TranslationUnit
* no TargetLanguage field in SourceBlock
* no ProviderPolicy
* no PromptTemplate
* no token budget
* no translated text
* Translation can create different Translation Plans from same SourceDocument

---

# 114. Contract Test Requirements — Traceability

Property:

```text
every non-empty SourceBlock
has at least one valid OCR source path
```

Property:

```text
every NormalizedText
is attributable to RawText
```

Property:

```text
every BlockSequence entry
references existing SourceBlock
```

Property:

```text
SourceDocument hierarchy
is acyclic
```

---

# 115. Contract Invariants

1. Text Processing input is a published Recognition Artifact.

2. Recognition Artifact is immutable.

3. OCRDocumentRef is canonical upstream OCR source.

4. Text Processing does not redefine OCR Region.

5. Text Processing does not redefine OCR Geometry.

6. Text Processing does not redefine OCR Reading Order.

7. Text Processing does not redefine OCR Quality.

8. Text Processing owns SourceDocument.

9. Text Processing owns SourceBlock.

10. Text Processing owns post-reconstruction BlockSequence.

11. RawText is preserved.

12. NormalizedText is separate.

13. Normalized text remains traceable to raw evidence.

14. SourceBlock remains traceable to OCR evidence.

15. Geometry references remain source-traceable.

16. Uncertain reconstruction is explicit.

17. Uncertain grouping prefers separation.

18. Unknown classification is valid.

19. Exclusion does not delete source evidence.

20. Exclusion is reversible where practical.

21. Empty source content may be successful.

22. Partial output is explicit.

23. Candidate and Published Artifact are distinct.

24. Text Processing creates Candidate only.

25. Runtime owns authority.

26. Runtime owns retry.

27. Runtime owns cancellation terminal outcome.

28. Artifact Store owns published lifecycle.

29. Text Processing does not publish authoritative Artifact.

30. Text Processing does not own WorkItem state.

31. Text Processing does not own Attempt state.

32. Runtime state is not embedded in SourceDocument.

33. Translation-specific fields are forbidden in SourceDocument.

34. TranslationUnit is not owned by Text Processing.

35. Target language does not affect SourceDocument semantics.

36. Translation Provider does not affect SourceDocument contract.

37. Processing Profile is versioned.

38. Processing Rules are versioned.

39. Semantic compatibility is explicit.

40. Cache policy is external.

41. Durable persistence is external.

42. Source correction never silently mutates original SourceDocument.

43. Normal logs do not expose complete source text.

44. Privacy Partition is preserved.

45. Provider SDK objects never cross the module boundary.

46. Unknown additive contract values are handled safely.

47. Unsupported major versions are rejected.

48. Attempt-local state is ephemeral.

49. Candidate rejection requires cleanup.

50. SourceDocument remains provider-independent and translation-independent.

---

# 116. MVP Contract Subset

Required input:

```text
RecognitionArtifactRef

ProcessingProfileRef

ProcessingOptions

RuntimeContext

ExecutionContextRef

CancellationContextRef

PrivacyContextRef
```

Required SourceDocument:

```text
DocumentId

DocumentVersion

DocumentType

SourceIdentity

Blocks[]

BlockSequence[]

RecognitionArtifactRef
```

Required SourceBlock:

```text
BlockId

BlockType

RawText

NormalizedText

OCRSourceRefs[]
```

Required block types:

```text
PARAGRAPH

DIALOGUE

NARRATION

CAPTION

HEADING

SOUND_EFFECT

ANNOTATION

PAGE_NUMBER

UNKNOWN
```

Required Candidate:

```text
CandidateArtifactId

RecognitionArtifactRef

SourceDocument

Completeness

CompatibilityMetadata

TraceabilityMetadata
```

---

# 117. MVP Optional Fields

Optional initially:

* hierarchy
* ChildBlockIds
* BlockExclusion
* classification alternatives
* ReconstructionConfidence
* GroupingConfidence
* merged geometry
* language detection
* previous-document context
* protected diagnostics
* WEB_TEXT adapter

---

# 118. Deferred Contract Extensions

Future:

```text
MultiPageSourceDocument

SourceCorrectionSet

CorrectedSourceDocumentView

CrossPageBlockReference

DOMSourceReference

EPUBSourceReference

PDFTextLayerReference

IncrementalSourceDocumentPatch

SourceDocumentDiff
```

Add only when concrete functionality requires them.

---

# 119. Related Documents

```text
02-modules/text-processing/README.md
02-modules/text-processing/MODULE.md
02-modules/text-processing/CONTRACT.md
02-modules/text-processing/STATES.md
02-modules/text-processing/EVENTS.md
02-modules/text-processing/ERRORS.md

02-modules/recognition/README.md
02-modules/recognition/MODULE.md
02-modules/recognition/CONTRACT.md

01-architecture/ocr/POSTPROCESS.md
01-architecture/ocr/QUALITY.md
01-architecture/ocr/READING_ORDER.md

01-architecture/runtime/CANCELLATION.md
01-architecture/runtime/RETRY_POLICY.md
01-architecture/runtime/CACHE_POLICY.md
01-architecture/runtime/RESOURCE_LIFECYCLE.md

02-modules/translation/
```

---

# 120. Summary

Text Processing Contract defines:

```text
RecognitionArtifact
        ↓
TextProcessingAttemptInput
        ↓
Text Processing
        ↓
SourceDocument
        ↓
CandidateSourceDocumentArtifact
        ↓
Runtime Authority Validation
        ↓
Published SourceDocument Artifact
```

Core ownership:

```text
Recognition
    owns Recognition Artifact.

OCR Architecture
    owns OCR semantics
    and canonical Reading Order.

Text Processing
    owns source reconstruction,
    SourceDocument,
    SourceBlock
    and SourceBlock Sequence.

Runtime
    owns execution authority,
    cancellation and retry.

Artifact Store
    owns accepted Artifact lifecycle.

Translation
    owns Translation Units,
    Translation Planning
    and translated content.
```

The essential contract is:

```text
SourceDocument
    is a stable representation
    of source content.

It is not an OCR result.

It is not a Translation Plan.

It is the boundary between
source reconstruction
and translation.
```
