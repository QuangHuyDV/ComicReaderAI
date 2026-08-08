# Translation Module Errors

> **Project:** CRAI
> **Module:** Translation
> **Path:** `02-modules/translation/ERRORS.md`
> **Version:** 1.0
> **Status:** Architecture Draft
> **Related:** `MODULE.md`, `CONTRACT.md`, `STATES.md`, `EVENTS.md`

---

# 1. Purpose

Tài liệu này định nghĩa normalized errors và warnings mà Translation Module thực sự sở hữu.

Translation Error Model chịu trách nhiệm:

* stable Translation error codes
* error categories
* error scopes
* severity
* module-level retry hints
* recovery recommendations
* SourceDocument input errors
* Translation Plan errors
* Translation Unit planning errors
* context errors
* Knowledge / terminology errors
* Translation Batch errors
* provider-boundary normalization
* provider output errors
* translated-output validation
* alignment errors
* Candidate errors
* variant/correction semantic errors
* privacy/security errors
* internal invariant failures
* warning model
* partial-output error semantics
* logging and diagnostics rules
* compatibility
* testing

Translation Error Model không định nghĩa canonical failure semantics của:

* Runtime WorkItem
* Runtime Attempt
* Scheduler
* Work Queue
* retry scheduling
* deadline lifecycle
* cancellation lifecycle
* supersession
* stale-result authority
* Artifact publication
* Artifact retention
* Cache infrastructure
* Provider Management lifecycle
* Knowledge persistence
* Reading Session
* Presentation

---

# 2. Error Ownership

Translation owns:

```text id="0uppvq"
TranslationModuleError

TranslationWarning

TranslationRetryHint

Input Errors

Plan Errors

Translation Unit Planning Errors

Context Errors

Terminology Errors

Batch Planning Errors

Provider Boundary Errors

Provider Output Errors

Output Validation Errors

Alignment Errors

Candidate Errors

Translation Variant Semantic Errors

Privacy Errors

Security Errors

Translation State Errors

Internal Translation Errors
```

Translation does not own:

```text id="bgctaf"
QueueAdmissionError

SchedulerError

RuntimeAttemptError

RuntimeDeadlineError

RuntimeCancellationError

RuntimeStaleResultError

ArtifactPublicationError

ArtifactRetentionError

CacheInfrastructureError

ProviderRegistryError

ProviderCredentialLifecycleError

KnowledgeStorageError

ReadingSessionError

PresentationError
```

---

# 3. Error Architecture

```text id="ll5kk7"
External / Provider Failure
        ↓
ExternalErrorRef
        ↓
Translation Context
        ↓
TranslationModuleError?
        ↓
TranslationRetryHint?
        ↓
Runtime
        ↓
Runtime Error Normalization
        ↓
Retry / Fail / Cancel / Abandon
```

Translation may normalize how an external failure affects Translation semantics.

Normalization does not transfer original ownership.

---

# 4. Error vs Warning

```text id="1xe7my"
Warning
    = translation remains contract-valid
      but quality/completeness/capability degraded
```

```text id="8wwc2q"
ModuleError
    = Translation cannot produce
      a contract-valid Candidate
      under the current semantic execution path
```

Examples:

```text id="r3nj1z"
optional context missing
    → Warning
```

```text id="og6ti7"
provider output cannot be aligned
    → Error
```

```text id="llh7uf"
one Unit failed
+
PartialResultPolicy allows PARTIAL
    → Warning + PARTIAL Candidate
```

---

# 5. Lifecycle Control Is Not Translation Error

These Runtime outcomes are not Translation-owned errors:

```text id="5ei0v1"
CANCELED

ABANDONED

STALE

SUPERSEDED

RETRY_SCHEDULED
```

A valid Candidate rejected as stale is not a Translation failure.

---

# 6. Error Principles

1. Error codes are stable.
2. Public errors are provider-neutral.
3. Source alignment must never be guessed after failure.
4. Missing Translation Units remain explicit.
5. Partial valid output should be preserved.
6. RetryHint is advisory.
7. Runtime owns retry execution.
8. Provider credentials never enter errors.
9. Source/translated text are absent by default.
10. Upstream ownership remains visible.
11. Privacy violations fail closed.
12. Untrusted provider/source content never controls error metadata.

---

# 7. TranslationModuleError

```text id="lg6l03"
TranslationModuleError
├── ContractVersion
├── ErrorId
├── ErrorCode
├── SymbolicName
├── Category
├── Scope
├── Severity
├── OperationPhase
├── MessageKey
├── RetryHint?
├── RecoveryActions[]
├── ProviderErrorRef?
├── ExternalErrorRef?
├── TranslationPlanId?
├── TranslationBatchId?
├── TranslationUnitIds[]
├── SourceBlockRefs[]
├── CandidateArtifactId?
├── DiagnosticsRef?
├── Metadata?
└── OccurredAt
```

Runtime IDs may be included through correlation context:

```text id="l2teym"
RevisionId

WorkItemId

AttemptId

TraceId
```

---

# 8. Removed Error Identities

Do not use:

```text id="3uk8gd"
TranslationJobId

TranslationAttemptId

TranslationResultRevision

PreparedDocumentId

PreparedSegmentId
```

as primary Translation error identity.

Current semantic identities are:

```text id="9mdd70"
SourceDocumentArtifactId

TranslationIntentId

TranslationPlanId

TranslationUnitId

TranslationBatchId

CandidateArtifactId

TranslationVariantId
```

---

# 9. Error Code Format

Canonical:

```text id="vwja9r"
TRN-<CATEGORY>-<NUMBER>
```

Examples:

```text id="wqih9d"
TRN-INPUT-001

TRN-PLAN-001

TRN-UNIT-001

TRN-CTX-001

TRN-TERM-001

TRN-BATCH-001

TRN-PROV-001

TRN-OUT-001

TRN-ALIGN-001

TRN-CAND-001

TRN-VAR-001

TRN-PRIV-001

TRN-SEC-001

TRN-STATE-001

TRN-INT-001
```

Optional symbolic aliases may preserve readable names such as:

```text id="5oyn55"
TRANSLATION_PROVIDER_UNAVAILABLE
```

but numeric code is the stable canonical identifier.

---

# 10. Error Categories

| Prefix  | Category                                   |
| ------- | ------------------------------------------ |
| `INPUT` | Translation Attempt Input / SourceDocument |
| `PLAN`  | Translation Plan                           |
| `UNIT`  | Translation Unit planning                  |
| `CTX`   | Translation Context                        |
| `TERM`  | Knowledge / terminology                    |
| `BATCH` | Translation Batch planning                 |
| `PROV`  | provider boundary / execution              |
| `OUT`   | normalized provider output validation      |
| `ALIGN` | source ↔ translation alignment             |
| `CAND`  | Candidate assembly / validation            |
| `VAR`   | immutable variant/correction semantics     |
| `PRIV`  | privacy                                    |
| `SEC`   | security                                   |
| `STATE` | Translation-owned state invariants         |
| `INT`   | internal Translation failure               |

Removed Translation-owned categories:

```text id="f2yag0"
TIMEOUT

CANCELLATION

SUPERSESSION

CACHE

PUBLICATION
```

---

# 11. Error Scope

```text id="0v2ekd"
TranslationErrorScope
├── MODULE
├── INPUT
├── PLAN
├── TRANSLATION_UNIT
├── TRANSLATION_BATCH
├── PROVIDER_REQUEST
├── PROVIDER_OUTPUT
├── CANDIDATE
└── VARIANT
```

Do not use:

```text id="dcksmh"
JOB

TRANSLATION_ATTEMPT

RESULT_REVISION

CACHE
```

as core Translation scopes.

---

# 12. Severity

```text id="cr7jd6"
NOTICE

DEGRADED

ERROR

CRITICAL
```

## NOTICE

Operation was rejected safely or non-fatal condition observed.

## DEGRADED

Valid output may still exist but with reduced quality/completeness.

## ERROR

Current semantic execution cannot produce required valid output.

## CRITICAL

Translation invariant, security, privacy, or contract integrity violation.

---

# 13. Retry Hint

```text id="0256xv"
TranslationRetryHint
├── Retryability
├── SuggestedStrategies[]
├── ProviderFallbackAllowed
├── ReasonCode
└── Metadata?
```

```text id="hq588c"
Retryability
├── RETRYABLE
├── CONDITIONALLY_RETRYABLE
└── NON_RETRYABLE
```

---

# 14. Retry Strategies

```text id="o8v4pm"
SAME_PROVIDER

ALTERNATIVE_PROVIDER

SMALLER_BATCH

REDUCE_CONTEXT

REDUCE_TERMINOLOGY_CONTEXT

ALTERNATIVE_CONTEXT_POLICY

ALTERNATIVE_TRANSLATION_PROFILE

RESOURCE_WAIT

USE_LOCAL_PROVIDER

NO_RETRY
```

No:

```text id="bm97jv"
NEW_JOB

NEW_TRANSLATION_ATTEMPT
```

because Runtime creates new Attempt.

---

# 15. Recovery Actions

Possible recommendations:

```text id="41ijki"
USE_FALLBACK_PROVIDER

SELECT_DIFFERENT_PROVIDER

USE_LOCAL_PROVIDER

REDUCE_BATCH_SIZE

REDUCE_CONTEXT_SIZE

CHANGE_TRANSLATION_PROFILE

CHANGE_TARGET_LANGUAGE

REFRESH_SOURCE_DOCUMENT

REVIEW_TERMINOLOGY

REMOVE_CONFLICTING_TERM

CREATE_NEW_TRANSLATION_VARIANT

CONTACT_SUPPORT

NONE
```

Recommendations are not automatic commands.

---

# 16. Provider Error Reference

```text id="ckj2v6"
TranslationProviderErrorRef
├── ProviderId
├── ModelIdentifier?
├── ProviderRequestId?
├── NormalizedProviderCode
├── ProviderCategory
├── HttpStatus?
├── RetryAfter?
├── DiagnosticsRef?
└── OccurredAt
```

Must not contain:

* API key
* token
* Authorization header
* raw provider response
* raw request
* raw prompt
* credential path

---

# 17. External Error Reference

```text id="odflpy"
ExternalErrorRef
├── Owner
├── ErrorCode
├── ErrorContractVersion
├── ScopeRef?
├── Retryability?
├── DiagnosticsRef?
└── Metadata?
```

Possible owners:

```text id="uyyozx"
RUNTIME

PROVIDER_MANAGEMENT

ARTIFACT_STORE

RESOURCE_MANAGER

TEXT_PROCESSING

KNOWLEDGE
```

---

# 18. Error Metadata

Allowed:

```text id="xdp18v"
unitCount

batchSize

estimatedTokenCount

providerLimit

missingUnitCount

failedUnitCount

contextEntryCount

terminologyConstraintCount

elapsedMs
```

Forbidden:

```text id="yiqjpr"
sourceText

translatedText

rawPrompt

rawProviderResponse

apiKey

authorizationHeader

credential
```

---

# 19. Input Errors

## TRN-INPUT-001 — TRANSLATION_INPUT_INVALID

TranslationAttemptInput malformed or inconsistent.

Examples:

* missing SourceDocumentArtifactRef
* missing TranslationIntent
* missing Runtime context
* invalid PrivacyContextRef
* invalid SourceSelection

Severity:

```text id="q2nvs1"
ERROR
```

Retry:

```text id="9pgn36"
NON_RETRYABLE
```

until input changes.

---

# 20. TRN-INPUT-002 — SOURCE_DOCUMENT_UNAVAILABLE

SourceDocument Artifact cannot be resolved or leased.

Retry:

```text id="dzcpmg"
CONDITIONALLY_RETRYABLE
```

Suggested:

```text id="jeihrn"
RESOURCE_WAIT
```

---

# 21. TRN-INPUT-003 — SOURCE_DOCUMENT_INCOMPATIBLE

SourceDocument exists but is incompatible with Translation contract.

Examples:

* unsupported contract major
* semantic content identity mismatch
* incompatible privacy partition
* missing required source structure

Retry:

```text id="7jetup"
NON_RETRYABLE
```

until compatible Artifact exists.

---

# 22. TRN-INPUT-004 — SOURCE_SELECTION_INVALID

Selected SourceBlocks cannot be resolved consistently.

Examples:

* missing SourceBlock IDs
* duplicated selections
* invalid sequence range
* selected block outside Artifact

Retry:

```text id="knhj7u"
NON_RETRYABLE
```

until selection changes.

---

# 23. Empty Source Is Not Error

If no selected content requires translation:

```text id="rnwe9o"
Completeness = EMPTY_VALID
```

Possible warning:

```text id="didjgd"
NO_TRANSLATABLE_CONTENT
```

Do not return:

```text id="givyfa"
TRANSLATION_SOURCE_EMPTY
```

as fatal error.

---

# 24. Source Language Errors

## TRN-INPUT-005 — SOURCE_LANGUAGE_UNRESOLVED

Source language required but cannot be determined.

Retry:

```text id="u5iwkf"
CONDITIONALLY_RETRYABLE
```

Possible recovery:

* explicit source language
* compatible language-detection provider
* alternate Translation Profile

---

# 25. TRN-INPUT-006 — TARGET_LANGUAGE_INVALID

Target language missing or invalid.

Retry:

```text id="9751wr"
NON_RETRYABLE
```

until Intent changes.

---

# 26. TRN-INPUT-007 — LANGUAGE_PAIR_UNSUPPORTED

No valid semantic/provider path supports requested pair.

Retry:

```text id="c8elxq"
CONDITIONALLY_RETRYABLE
```

when provider/configuration changes.

---

# 27. Plan Errors

## TRN-PLAN-001 — TRANSLATION_PLAN_INVALID

Plan cannot become READY.

Examples:

* contradictory Translation Intent
* impossible Provider Policy
* invalid PartialResultPolicy
* incompatible source/profile
* invalid terminology/context combination

---

# 28. TRN-PLAN-002 — TRANSLATION_PROFILE_UNSUPPORTED

Requested profile unsupported.

Retry:

```text id="th4wzs"
NON_RETRYABLE
```

until profile changes.

---

# 29. TRN-PLAN-003 — PROVIDER_POLICY_UNSATISFIABLE

No eligible execution path satisfies Provider Policy.

Examples:

```text id="a76vao"
LOCAL_REQUIRED
+
no local provider
```

or:

```text id="yk498p"
required provider excluded
```

Retry:

```text id="31s699"
CONDITIONALLY_RETRYABLE
```

---

# 30. TRN-PLAN-004 — PRIVACY_PROVIDER_POLICY_CONFLICT

Provider Policy conflicts with Privacy Context.

Example:

```text id="u4xsgh"
remote provider required
+
Privacy = LOCAL_ONLY
```

Severity:

```text id="f4fa58"
ERROR
```

Retry only after semantic policy change.

---

# 31. Translation Unit Errors

## TRN-UNIT-001 — TRANSLATION_UNIT_PLANNING_FAILED

Translation cannot construct valid Translation Units.

Retry:

```text id="o5uj0d"
CONDITIONALLY_RETRYABLE
```

---

# 32. TRN-UNIT-002 — TRANSLATION_UNIT_SOURCE_MISSING

TranslationUnit references unavailable SourceBlock.

Severity:

```text id="ddbs6s"
CRITICAL
```

Retry:

```text id="phcfpl"
NON_RETRYABLE
```

for unchanged source/plan.

---

# 33. TRN-UNIT-003 — TRANSLATION_UNIT_DUPLICATE_SOURCE_MAPPING

Unit construction creates prohibited duplicate mapping.

Severity:

```text id="m77b5g"
ERROR
```

or `CRITICAL` for invariant defects.

---

# 34. TRN-UNIT-004 — TRANSLATION_UNIT_SPLIT_UNTRACEABLE

One SourceBlock was split into multiple Units without valid source-range traceability.

Severity:

```text id="qnstc5"
CRITICAL
```

---

# 35. TRN-UNIT-005 — TRANSLATION_UNIT_ORDER_INVALID

Unit order cannot be deterministically derived from SourceDocument sequence.

Retry:

```text id="ro143g"
NON_RETRYABLE
```

until source/plan changes.

Translation must not reconstruct OCR Reading Order.

---

# 36. Context Errors

## TRN-CTX-001 — REQUIRED_CONTEXT_UNAVAILABLE

A required context snapshot/reference cannot be resolved.

Strict policy:

```text id="yfb34r"
ERROR
```

Best-effort policy should instead emit warning.

---

# 37. TRN-CTX-002 — CONTEXT_IDENTITY_MISMATCH

Resolved context does not match immutable Plan identity.

Retry:

```text id="1cocf5"
NON_RETRYABLE
```

within current Plan.

---

# 38. TRN-CTX-003 — CONTEXT_TOO_LARGE

Context exceeds Provider/Plan limit.

Retry:

```text id="78ha76"
RETRYABLE
```

Suggested:

```text id="w29tzu"
REDUCE_CONTEXT
```

---

# 39. TRN-CTX-004 — CONTEXT_CONSTRUCTION_FAILED

Context entries cannot be safely assembled.

Examples:

* unresolved refs
* invalid relationship
* source/context ordering conflict
* unsupported context type

---

# 40. Missing Optional Context

Use warning:

```text id="bxb6p4"
MISSING_OPTIONAL_CONTEXT
```

not ModuleError.

---

# 41. Terminology Errors

## TRN-TERM-001 — KNOWLEDGE_SNAPSHOT_UNAVAILABLE

Required Knowledge Snapshot unavailable.

Strict policy:

```text id="43uwbz"
ERROR
```

Best-effort:

```text id="zs2j8l"
warning
```

---

# 42. TRN-TERM-002 — TERMINOLOGY_CONFLICT

Conflicting constraints cannot be resolved according to policy.

Example:

```text id="n3pqn3"
same SourceTerm
+
two LOCKED target mappings
```

Retry:

```text id="2bzhzm"
NON_RETRYABLE
```

until terminology changes.

---

# 43. TRN-TERM-003 — LOCKED_TERMINOLOGY_VIOLATED

Validated provider output violates a LOCKED constraint.

Retry:

```text id="ma1lgc"
RETRYABLE
```

Suggested:

```text id="0ztqa7"
SAME_PROVIDER
or
ALTERNATIVE_PROVIDER
```

depending policy.

---

# 44. TRN-TERM-004 — TERMINOLOGY_LIMIT_EXCEEDED

Required terminology constraints cannot fit execution limits.

Translation must not silently discard LOCKED constraints.

Suggested:

```text id="k5wch7"
REDUCE_TERMINOLOGY_CONTEXT

ALTERNATIVE_PROVIDER
```

---

# 45. Character / Pronoun Ambiguity

Ambiguous:

* speaker relationship
* pronoun
* honorific
* character identity

should normally produce warning:

```text id="m1shml"
PRONOUN_AMBIGUITY

SPEAKER_RELATIONSHIP_AMBIGUITY
```

not fatal error.

---

# 46. Batch Errors

## TRN-BATCH-001 — TRANSLATION_BATCH_EMPTY

Batch created with no Translation Units.

Severity:

```text id="aioc26"
CRITICAL
```

This is usually a planning/invariant defect.

---

# 47. TRN-BATCH-002 — TRANSLATION_BATCH_DUPLICATE_UNIT

Same Unit appears more than once within Batch.

Severity:

```text id="q26jaq"
CRITICAL
```

---

# 48. TRN-BATCH-003 — TRANSLATION_BATCH_LIMIT_EXCEEDED

Batch exceeds resolved execution/provider limits.

Retry:

```text id="4eymeq"
RETRYABLE
```

Suggested:

```text id="cqzakq"
SMALLER_BATCH
```

---

# 49. TRN-BATCH-004 — TRANSLATION_BATCH_CONSTRUCTION_FAILED

No valid Batch layout can satisfy:

* grouping constraints
* provider limits
* context requirements
* terminology requirements

Retry:

```text id="exl2tw"
CONDITIONALLY_RETRYABLE
```

---

# 50. Provider Ownership Boundary

Provider Management owns:

* registration
* enabled/disabled state
* credentials
* credential refresh
* provider lifecycle
* health
* availability
* local model residency

Translation may normalize provider failures only when they affect current translation execution.

---

# 51. Provider Errors

## TRN-PROV-001 — PROVIDER_UNAVAILABLE

Chosen execution provider unavailable.

Original owner may be Provider Management.

Retry:

```text id="l6glfn"
RETRYABLE
```

Suggested:

```text id="3z84qs"
ALTERNATIVE_PROVIDER

RESOURCE_WAIT
```

---

# 52. TRN-PROV-002 — NO_ELIGIBLE_PROVIDER

No provider satisfies resolved Plan requirements.

Retry:

```text id="xigh0w"
CONDITIONALLY_RETRYABLE
```

---

# 53. TRN-PROV-003 — PROVIDER_CAPABILITY_MISSING

Provider lacks required capability.

Examples:

* language pair
* context size
* structured output
* local execution
* required glossary semantics

Retry:

```text id="6il28q"
RETRYABLE
```

with another provider.

---

# 54. Provider Credentials

Original credential failures belong to Provider Management.

Translation should prefer:

```text id="0srq0t"
ExternalErrorRef.Owner = PROVIDER_MANAGEMENT
```

rather than duplicating credential lifecycle errors.

If current translation cannot execute:

```text id="821v09"
TRN-PROV-001
```

may reference the canonical external error.

---

# 55. TRN-PROV-004 — PROVIDER_REQUEST_REJECTED

Provider rejected a structurally valid Translation request.

Examples:

* unsupported provider parameters
* content rejected
* unavailable model
* account restrictions

Retry depends on normalized reason.

---

# 56. TRN-PROV-005 — PROVIDER_RATE_LIMITED

Provider rejected execution because of rate limit.

Retry:

```text id="ojc0zq"
RETRYABLE
```

`RetryAfter` may be advisory.

Runtime decides actual wait/backoff.

---

# 57. TRN-PROV-006 — PROVIDER_QUOTA_EXCEEDED

Provider/account execution quota unavailable.

Retry:

```text id="du9vxw"
CONDITIONALLY_RETRYABLE
```

Fallback may be possible.

---

# 58. TRN-PROV-007 — PROVIDER_REQUEST_TOO_LARGE

Provider rejects Batch/context size.

Retry:

```text id="ij0o3j"
RETRYABLE
```

Suggested:

```text id="j6xe84"
SMALLER_BATCH

REDUCE_CONTEXT
```

---

# 59. TRN-PROV-008 — PROVIDER_CONNECTION_FAILED

Provider request cannot establish/maintain connection.

Retry:

```text id="a7tyao"
RETRYABLE
```

---

# 60. Provider Timeout Boundary

Provider request timeout may be normalized as:

```text id="df9px0"
TRN-PROV-009
PROVIDER_REQUEST_TIMEOUT
```

when it specifically means:

```text id="6wtsqf"
one Translation provider call
failed to return within its provider/request limit
```

This is distinct from Runtime Attempt deadline.

---

# 61. No Queue / Attempt / Job Timeout Errors

Remove Translation-owned:

```text id="dvob9b"
TRANSLATION_QUEUE_TIMEOUT

TRANSLATION_ATTEMPT_TIMEOUT

TRANSLATION_JOB_TIMEOUT

TRANSLATION_BATCH_TIMEOUT
```

when those represent Runtime execution deadlines.

Runtime owns canonical deadline outcome.

---

# 62. TRN-PROV-010 — PROVIDER_INTERNAL_FAILURE

Provider reports transient internal error.

Retry:

```text id="39xyis"
RETRYABLE
```

---

# 63. TRN-PROV-011 — PROVIDER_OUTPUT_LIMIT_REACHED

Provider generation stopped because output limit reached.

May result in:

```text id="yo24vq"
PARTIAL
```

if valid aligned Units exist and policy permits.

Otherwise error.

---

# 64. Provider Cancellation Is Not Error

Provider acknowledgement of cancellation is:

```text id="3w7ihw"
ProviderExecutionObservation
```

not:

```text id="bd2zdn"
TranslationModuleError
```

Runtime owns cancellation.

---

# 65. Local Provider Resource Errors

Provider/local-model resource exhaustion should normally reference:

```text id="fg0h0h"
Provider Management

Resource Manager
```

Translation may return:

```text id="xdhxhc"
TRN-PROV-001 PROVIDER_UNAVAILABLE
```

with external cause.

Do not duplicate infrastructure resource ownership unless the buffer/resource is Translation-local.

---

# 66. Provider Output Errors

## TRN-OUT-001 — PROVIDER_OUTPUT_EMPTY

Provider returned no usable Translation Unit outputs.

Retry:

```text id="3zsyrm"
RETRYABLE
```

---

# 67. TRN-OUT-002 — PROVIDER_OUTPUT_MALFORMED

Adapter cannot parse provider result into provider-neutral output.

Retry:

```text id="8cfco4"
RETRYABLE
```

Raw provider response must not appear in public error.

---

# 68. TRN-OUT-003 — PROVIDER_OUTPUT_TRUNCATED

Structured output ended prematurely.

Complete validated Units may be preserved.

Severity:

```text id="t27yi9"
DEGRADED
or
ERROR
```

depending PartialResultPolicy.

---

# 69. TRN-OUT-004 — PROVIDER_OUTPUT_UNEXPECTED_FORMAT

Provider output parses syntactically but violates requested structure.

Retry:

```text id="1hmi6r"
RETRYABLE
```

---

# 70. TRN-OUT-005 — PROVIDER_OUTPUT_UNKNOWN_UNIT

Provider returned TranslationUnitId not present in Batch.

Severity:

```text id="hx6spc"
ERROR
```

Never attach unknown output heuristically.

---

# 71. TRN-OUT-006 — PROVIDER_OUTPUT_DUPLICATE_UNIT

Provider returned incompatible multiple outputs for same Unit.

Retry:

```text id="3ztwn6"
RETRYABLE
```

---

# 72. TRN-OUT-007 — PROVIDER_OUTPUT_UNIT_MISSING

Provider omitted required Units.

If policy allows partial:

```text id="n1cn1d"
PARTIAL Candidate
+
warning
```

Otherwise ModuleError.

---

# 73. Output Validation Errors

## TRN-OUT-008 — OUTPUT_VALIDATION_FAILED

Generic validator failure when no more specific code applies.

Prefer precise codes.

---

# 74. TRN-OUT-009 — OUTPUT_EMPTY

A Unit requiring translation produced empty result.

Retry:

```text id="xb1new"
RETRYABLE
```

Intentionally preserved/empty Units must be explicitly modeled and must not use this error.

---

# 75. TRN-OUT-010 — TARGET_LANGUAGE_MISMATCH

Output is not primarily in required target language.

Retry:

```text id="76kbzs"
RETRYABLE
```

Names, proper nouns and locked source terms must not automatically trigger this.

---

# 76. TRN-OUT-011 — EXCESSIVE_SOURCE_LEAKAGE

Output retains excessive untranslated source content beyond allowed policy.

Severity:

```text id="5hv7nb"
DEGRADED
or
ERROR
```

---

# 77. TRN-OUT-012 — CONTROL_CONTENT_LEAKED

Provider/system control text leaked into translated output.

Examples:

```text id="txu66i"
SYSTEM:

TRANSLATION:

JSON wrappers

internal Unit markers

provider refusal prefix
```

Candidate must not accept unsafe output.

---

# 78. TRN-OUT-013 — OUTPUT_LENGTH_INVALID

Output violates a hard semantic/configured limit.

A merely long translation should use warning:

```text id="3fa20u"
OUTPUT_LENGTH_ANOMALY
```

not error.

---

# 79. TRN-OUT-014 — OUTPUT_DUPLICATED

Suspicious repeated outputs indicate provider/assembly failure.

Retry:

```text id="rdsgyn"
RETRYABLE
```

---

# 80. TRN-OUT-015 — OUTPUT_INCOMPLETE

Output is structurally/semantically incomplete.

Examples:

* sentence truncated
* incomplete clause
* only subset translated
* Unit partially generated

May become PARTIAL if alignment is safe.

---

# 81. Alignment Errors

## TRN-ALIGN-001 — TRANSLATION_ALIGNMENT_FAILED

Provider outputs cannot be safely mapped to Translation Units.

Severity:

```text id="ni9ye9"
ERROR
```

No ambiguous attachment allowed.

---

# 82. TRN-ALIGN-002 — TRANSLATED_UNIT_SOURCE_MISSING

TranslatedUnit references missing TranslationUnit or SourceBlock lineage.

Severity:

```text id="580aea"
CRITICAL
```

---

# 83. TRN-ALIGN-003 — DUPLICATE_TRANSLATED_UNIT

More than one authoritative TranslatedUnit exists for one Unit within Candidate without explicit variant semantics.

Severity:

```text id="xc4dah"
CRITICAL
```

---

# 84. TRN-ALIGN-004 — UNKNOWN_SOURCE_REFERENCE

Translated output references source outside current SourceDocument identity.

Severity:

```text id="8g434q"
CRITICAL
```

Candidate invalid.

---

# 85. TRN-ALIGN-005 — TRANSLATION_SEQUENCE_INVALID

Translated Unit ordering cannot be reconciled with TranslationUnit source sequence.

Translation must not infer a replacement source Reading Order.

---

# 86. TRN-ALIGN-006 — SOURCE_IDENTITY_MISMATCH

Translated output/Unit belongs to incompatible SourceDocument semantic identity.

Retry:

```text id="dvgyyv"
NON_RETRYABLE
```

within current Plan.

---

# 87. Candidate Errors

## TRN-CAND-001 — CANDIDATE_ASSEMBLY_FAILED

Validated Translation outputs cannot be assembled into Candidate.

Possible causes:

* conflicting mappings
* missing required Unit metadata
* invalid completeness calculation
* incompatible provenance

---

# 88. TRN-CAND-002 — CANDIDATE_INVALID

Candidate fails Translation contract validation.

Examples:

* duplicate TranslatedUnit
* invalid TranslationUnit mapping
* invalid completeness
* missing SourceDocumentRef
* missing TraceabilityMetadata
* invalid target language
* Runtime state leaked into Candidate

---

# 89. TRN-CAND-003 — CANDIDATE_COMPLETENESS_INVALID

Candidate claims completeness inconsistent with Units.

Examples:

```text id="ed4nkc"
COMPLETE
+
missing TranslationUnits
```

or:

```text id="nc67t3"
PARTIAL
+
no missing/failed Units
```

Severity:

```text id="i0my63"
CRITICAL
```

---

# 90. TRN-CAND-004 — CANDIDATE_SUBMISSION_FAILED

Valid Candidate cannot cross Translation → Runtime boundary due to Translation-local transfer/serialization failure.

Not used for:

* Runtime stale rejection
* Runtime cancellation rejection
* Runtime authority rejection
* Artifact Store publication failure

---

# 91. Candidate Stale Rejection Is Not Error

Example:

```text id="bo1eoc"
Candidate VALID
    ↓
Runtime detects stale Revision
    ↓
REJECTED_STALE
```

No TranslationModuleError.

---

# 92. Partial Candidate Semantics

If required Unit fails but policy permits partial:

```text id="o79cdl"
Completeness = PARTIAL
```

with:

* MissingTranslationUnitIds
* FailedTranslationUnitIds
* warnings

Do not silently drop failure.

---

# 93. Variant Errors

Translation owns only immutable variant semantic construction.

It does not own active variant selection.

---

# 94. TRN-VAR-001 — VARIANT_CREATION_FAILED

Valid translated semantic result cannot be represented as immutable variant.

Retry:

```text id="ux0kmi"
CONDITIONALLY_RETRYABLE
```

---

# 95. TRN-VAR-002 — CORRECTION_INVALID

Correction is malformed or references incompatible Translation Units.

Retry:

```text id="12p10x"
NON_RETRYABLE
```

until correction changes.

---

# 96. TRN-VAR-003 — CORRECTION_BASE_MISMATCH

Correction references incompatible SourceDocument/TranslationArtifact variant.

Requires new compatible correction base.

---

# 97. Removed Variant Errors

Remove Translation-owned:

```text id="ac5ceu"
VARIANT_ACTIVATION_CONFLICT

VARIANT_INVALIDATED

DUPLICATE_ACTIVE_VARIANT
```

because active variant belongs to Reading Session/application projection.

---

# 98. Privacy Errors

## TRN-PRIV-001 — REMOTE_EXECUTION_PROHIBITED

Resolved execution path would send content remotely against Privacy Context.

Retry:

```text id="nk3fme"
RETRYABLE
```

with compatible local provider if available.

---

# 99. TRN-PRIV-002 — DATA_REGION_PROHIBITED

Selected provider region violates privacy requirements.

Retry with compatible provider/region.

---

# 100. TRN-PRIV-003 — CONTEXT_PRIVACY_CONFLICT

Required context cannot be transmitted under selected provider/privacy combination.

Possible handling:

* reduce optional context
* use local provider
* choose compliant provider
* fail if context is mandatory

---

# 101. TRN-PRIV-004 — CANDIDATE_PRIVACY_VIOLATION

Candidate contains content/metadata forbidden by Privacy Context.

Severity:

```text id="vwyw9a"
CRITICAL
```

Candidate must not be submitted.

---

# 102. Security Errors

## TRN-SEC-001 — CREDENTIAL_EXPOSURE_DETECTED

Translation is about to expose credential through:

* Candidate
* Event
* Error
* Log
* diagnostics

Severity:

```text id="2wlgha"
CRITICAL
```

Operation must fail closed.

---

# 103. TRN-SEC-002 — SENSITIVE_CONTENT_LOGGING_DETECTED

Raw source/translated content attempted to enter prohibited logging channel.

Severity:

```text id="v5r6cj"
CRITICAL
```

Content must not be logged.

---

# 104. Untrusted Instructions

Instruction-like source text alone is not an error.

Source remains untrusted data.

Possible warning:

```text id="k2xe14"
UNTRUSTED_INSTRUCTION_PATTERN
```

Translation should continue through hardened provider boundary unless actual contract safety is compromised.

---

# 105. TRN-SEC-003 — PROVIDER_INSTRUCTION_LEAKAGE

Provider output reveals:

* system instructions
* prompt fragments
* control metadata
* structured internal markers

Severity:

```text id="n5n4zr"
ERROR
```

Output must not become Candidate content.

---

# 106. State Errors

## TRN-STATE-001 — STATE_INVARIANT_VIOLATION

Translation-owned local state violates `STATES.md`.

Examples:

```text id="7nks2k"
Plan READY → BUILDING

Batch INVALID → READY

Candidate VALID → ASSEMBLING
```

Severity:

```text id="shxepl"
CRITICAL
```

---

# 107. TRN-STATE-002 — DUPLICATE_CANDIDATE_SUBMISSION

Translation logic semantically submits same Candidate more than once.

Severity:

```text id="m5eppp"
CRITICAL
```

---

# 108. TRN-STATE-003 — BATCH_STATE_CONFLICT

Concurrent local operations violate Batch state contract.

Example:

```text id="pzb7qh"
Batch VALID
+
new provider output mutation
```

---

# 109. Removed State Errors

Remove lifecycle-specific errors such as:

```text id="sw19yv"
RETRY_ALREADY_ACTIVE

ALREADY_TERMINAL

CANCELLATION_NOT_ALLOWED

STALE_ATTEMPT_REJECTED

DUPLICATE_ACTIVE_ATTEMPT
```

Runtime owns those concerns.

---

# 110. Internal Errors

## TRN-INT-001 — INTERNAL_FAILURE

Unexpected Translation implementation failure.

Retry:

```text id="hloecj"
CONDITIONALLY_RETRYABLE
```

Runtime decides if a new Attempt is useful.

---

# 111. TRN-INT-002 — CONFIGURATION_RESOLUTION_FAILED

Translation cannot resolve immutable required configuration snapshot.

Retry may become possible after configuration/runtime recovery.

---

# 112. TRN-INT-003 — PROVIDER_ADAPTER_FAILURE

Provider Adapter fails outside recognized provider categories.

Public contract must still hide:

* stack traces
* provider-native objects
* raw payload

---

# 113. TRN-INT-004 — SERIALIZATION_FAILED

Translation-owned provider-neutral object cannot be serialized.

Examples:

* provider-neutral request
* Candidate transfer
* diagnostics metadata

---

# 114. TRN-INT-005 — DESERIALIZATION_FAILED

Translation-owned object cannot be decoded according to expected contract.

---

# 115. TRN-INT-006 — ARCHITECTURE_INVARIANT_VIOLATION

Core architecture invariant violated.

Examples:

* TranslatedUnit without TranslationUnit
* TranslationUnit without SourceBlock lineage
* Translation mutates SourceDocument
* Translation publishes Artifact directly
* Provider credential reaches Candidate
* provider-specific SDK type crosses public boundary
* Candidate mutated after VALID
* Translation assumes Runtime authority

Severity:

```text id="guwonl"
CRITICAL
```

---

# 116. Errors Not Owned by Translation

Do not create Translation aliases for:

```text id="9xif7a"
QUEUE_ADMISSION_FAILED

QUEUE_TIMEOUT

RUNTIME_ATTEMPT_TIMEOUT

RUNTIME_CANCELLATION

RUNTIME_STALE_RESULT

RUNTIME_RETRY_LIMIT

WORKER_CRASH

ARTIFACT_PUBLICATION_FAILED

ARTIFACT_RETENTION_FAILED

CACHE_READ_FAILED

CACHE_WRITE_FAILED

PROVIDER_REGISTRY_FAILED

PROVIDER_HEALTH_CHECK_FAILED

PROVIDER_CREDENTIAL_REFRESH_FAILED

READING_SESSION_ACTIVATION_CONFLICT
```

Reference owner error instead.

---

# 117. Publication Errors Removed

Legacy:

```text id="mgbi7t"
TRANSLATION_RESULT_PUBLICATION_FAILED

TRANSLATION_RESULT_PERSISTENCE_FAILED
```

are removed from Translation ownership.

Current:

```text id="466iol"
Candidate
    ↓
Runtime
    ↓
Artifact Store
```

Artifact publication/persistence failures belong downstream.

---

# 118. Cache Errors Removed

Legacy:

```text id="totp3m"
TRANSLATION_CACHE_READ_FAILED

TRANSLATION_CACHE_WRITE_FAILED

TRANSLATION_CACHE_ENTRY_CORRUPTED

TRANSLATION_CACHE_ALIGNMENT_MISMATCH
```

are no longer Translation-owned.

Translation owns:

```text id="arvxmd"
semantic compatibility
```

Runtime Cache Policy/Artifact Store own actual reuse/storage mechanics.

---

# 119. Retry Errors Removed

Remove:

```text id="rpteie"
TRANSLATION_RETRY_NOT_ALLOWED

TRANSLATION_RETRY_LIMIT_EXCEEDED

TRANSLATION_RETRY_ALREADY_ACTIVE

TRANSLATION_FALLBACK_LIMIT_EXCEEDED
```

when they describe Runtime retry budget/lifecycle.

Translation only returns RetryHint.

---

# 120. Cancellation / Supersession Errors Removed

Remove:

```text id="ov9hiw"
TRANSLATION_CANCELLED

TRANSLATION_SUPERSEDED

TRANSLATION_STALE_RESULT_REJECTED

TRANSLATION_STALE_ATTEMPT_REJECTED
```

These are Runtime authority/lifecycle outcomes.

---

# 121. Warning Contract

```text id="uyfsq0"
TranslationWarning
├── WarningCode
├── Severity
├── OperationPhase
├── TranslationUnitIds[]
├── SourceBlockRefs[]
├── TranslationBatchId?
├── ProviderId?
├── CandidateArtifactId?
├── MessageKey
├── SuggestedActions[]
├── Metadata?
└── RecordedAt
```

---

# 122. Warning Severity

```text id="15iqwf"
INFORMATION

NOTICE

DEGRADED
```

---

# 123. Recommended Warning Codes

```text id="2oefzm"
NO_TRANSLATABLE_CONTENT

MISSING_OPTIONAL_CONTEXT

CONTEXT_TRUNCATED

LOW_TRANSLATION_CONFIDENCE

AMBIGUOUS_MEANING

TERMINOLOGY_CONFLICT

SOURCE_INCOMPLETE

SOURCE_LANGUAGE_UNCERTAIN

UNTRANSLATED_FRAGMENT

OUTPUT_LENGTH_ANOMALY

PROVIDER_FALLBACK_USED

PARTIAL_TRANSLATION

SOUND_EFFECT_PRESERVED

MIXED_LANGUAGE_CONTENT

PRONOUN_AMBIGUITY

SPEAKER_RELATIONSHIP_AMBIGUITY

KNOWLEDGE_UNAVAILABLE

UNTRUSTED_INSTRUCTION_PATTERN
```

---

# 124. NO_TRANSLATABLE_CONTENT

Used when:

```text id="605810"
Completeness = EMPTY_VALID
```

No ModuleError required.

---

# 125. MISSING_OPTIONAL_CONTEXT

Context unavailable but policy permits best-effort translation.

Candidate may remain valid.

---

# 126. CONTEXT_TRUNCATED

Context reduced to meet limits.

Requires deterministic/defined truncation policy.

---

# 127. LOW_TRANSLATION_CONFIDENCE

Confidence is advisory.

Must not be presented as objective truth.

---

# 128. TERMINOLOGY_CONFLICT Warning

Use only when conflict policy allows continuation.

If policy requires failure:

```text id="jtxjcm"
TRN-TERM-002
```

---

# 129. PROVIDER_FALLBACK_USED

Describes provenance degradation/variation.

It is not itself failure.

---

# 130. PARTIAL_TRANSLATION

Candidate:

```text id="z13vz7"
Completeness = PARTIAL
```

may include warning.

Missing/failed Unit IDs remain explicit.

---

# 131. OUTPUT_LENGTH_ANOMALY

Use warning when output is unusually long/short but still valid.

Presentation may later use length information for layout decisions.

Translation does not own visual fit.

---

# 132. Sound Effect Warning

```text id="f5cbu9"
SOUND_EFFECT_PRESERVED
```

may describe intentional source-language preservation.

Not an error.

---

# 133. Logging Contract

Safe fields:

```text id="qnnfvj"
ErrorCode

SymbolicName

Category

Scope

Severity

OperationPhase

RevisionId

WorkItemId

AttemptId

SourceDocumentArtifactId

TranslationIntentId

TranslationPlanId?

TranslationBatchId?

CandidateArtifactId?

ProviderId?

Retryability

ConfigurationSnapshotId

TraceId

OccurredAt
```

---

# 134. Forbidden Logging Fields

Normal logs must not contain:

```text id="ydb93c"
full source text

full translated text

raw provider prompt

provider raw response

credential

Authorization header

API key

refresh token
```

---

# 135. Diagnostics Reference

Protected detailed diagnostics should use:

```text id="4g0wp5"
DiagnosticsRef
```

rather than embedding content.

Protected diagnostics require:

* explicit authorization
* redaction
* bounded retention
* secure storage
* Privacy Context compliance

---

# 136. Metrics

Useful Translation-owned error metrics:

```text id="nfqh4y"
translation.error.total

translation.error.by_code

translation.error.by_category

translation.error.by_phase

translation.warning.total

translation.warning.by_code

translation.provider_output_invalid_total

translation.alignment_failure_total

translation.candidate_invalid_total

translation.partial_total

translation.provider_fallback_total

translation.privacy_violation_total

translation.invariant_violation_total
```

---

# 137. Metrics Not Owned by Translation

Do not redefine:

```text id="4l7pds"
runtime.queue_failure_total

runtime.attempt_timeout_total

runtime.retry_exhausted_total

runtime.cancellation_total

artifact.publication_failure_total

cache.read_failure_total

provider.health_failure_total
```

---

# 138. Error and Runtime Relationship

```text id="grb3f4"
TranslationModuleError
        ↓
Runtime
        ↓
Retry Policy
Cancellation Policy
Authority Policy
        ↓
Attempt Disposition
```

No fixed one-to-one mapping from Translation error to Runtime terminal state.

---

# 139. Error and Candidate Relationship

```text id="0npqlb"
Valid Candidate
    → no fatal ModuleError
    → warnings allowed
```

```text id="4nr12y"
Invalid Candidate
    → ModuleError
    → no valid Candidate submission
```

```text id="vn5bxb"
Valid Candidate
    → Runtime rejects stale
    → no Translation error
```

---

# 140. Provider Failure and Partial Candidate

Example:

```text id="4rdwrp"
Batch A VALID

Batch B VALID

Batch C provider failed
```

If policy permits:

```text id="derk96"
Candidate
Completeness = PARTIAL
```

with:

```text id="5s1sn5"
FailedTranslationUnitIds
```

and warnings.

Translation does not have to discard successful Units.

---

# 141. Retry Classification

Typically retryable:

```text id="09ni84"
TRN-PROV-001 PROVIDER_UNAVAILABLE

TRN-PROV-005 PROVIDER_RATE_LIMITED

TRN-PROV-008 PROVIDER_CONNECTION_FAILED

TRN-PROV-009 PROVIDER_REQUEST_TIMEOUT

TRN-PROV-010 PROVIDER_INTERNAL_FAILURE

TRN-OUT-001 PROVIDER_OUTPUT_EMPTY

TRN-OUT-002 PROVIDER_OUTPUT_MALFORMED
```

---

# 142. Retryable After Adjustment

```text id="0bb92v"
TRN-CTX-003 CONTEXT_TOO_LARGE

TRN-TERM-004 TERMINOLOGY_LIMIT_EXCEEDED

TRN-BATCH-003 TRANSLATION_BATCH_LIMIT_EXCEEDED

TRN-PROV-007 PROVIDER_REQUEST_TOO_LARGE
```

Requires:

```text id="g3l0j2"
SMALLER_BATCH

REDUCE_CONTEXT

REDUCE_TERMINOLOGY_CONTEXT

ALTERNATIVE_PROVIDER
```

---

# 143. Typically Non-Retryable Without Semantic Change

```text id="j8v5l7"
TRN-INPUT-004 SOURCE_SELECTION_INVALID

TRN-INPUT-006 TARGET_LANGUAGE_INVALID

TRN-PLAN-002 TRANSLATION_PROFILE_UNSUPPORTED

TRN-TERM-002 TERMINOLOGY_CONFLICT

TRN-UNIT-004 TRANSLATION_UNIT_SPLIT_UNTRACEABLE

TRN-ALIGN-004 UNKNOWN_SOURCE_REFERENCE

TRN-PRIV-004 CANDIDATE_PRIVACY_VIOLATION

TRN-SEC-001 CREDENTIAL_EXPOSURE_DETECTED
```

---

# 144. Retry Does Not Mutate Failed State

A retry creates a new Runtime Attempt.

Do not reset:

```text id="oh4j2j"
Batch INVALID
    → READY
```

or:

```text id="v37hvx"
Candidate INVALID
    → VALID
```

within same instance.

---

# 145. Error Contract Evolution

Backward-compatible:

* new ErrorCode
* new WarningCode
* optional metadata
* optional DiagnosticsRef
* new RetryStrategy
* new provider normalized category

Breaking:

* changing code meaning
* changing original owner
* weakening privacy
* changing Candidate acceptance semantics
* making provider-native type public
* changing retry authority
* changing SourceBlock/TranslationUnit alignment guarantees

Breaking change requires major contract version.

---

# 146. Unknown Codes

Consumers must:

* preserve unknown ErrorCode
* use known category when possible
* not crash
* not fabricate retry behavior
* reject unsupported contract major if required

Fallback:

```text id="axfgls"
TRN-INT-001
INTERNAL_FAILURE
```

may be used only when no specific code is possible.

---

# 147. Testing — Error Contract

Verify:

* every ErrorCode unique
* one semantic meaning per code
* valid category
* valid scope
* valid severity
* RetryHint advisory
* no Runtime terminal state
* no TranslationJob identity
* no TranslationAttempt identity
* no credentials
* no source/translated text by default

---

# 148. Testing — Input

Test:

* missing SourceDocumentArtifactRef
* unavailable Artifact
* incompatible Artifact
* invalid SourceBlock selection
* unsupported target language
* unsupported language pair
* EMPTY_VALID source
* Privacy Context conflict

---

# 149. Testing — Unit Planning

Test:

* missing SourceBlock
* duplicate source mapping
* valid N→1 Unit
* valid 1→N Unit
* untraceable split
* invalid order
* source lineage preserved

---

# 150. Testing — Context

Test:

* required context available
* optional context missing
* required context missing
* context too large
* context identity mismatch
* privacy-restricted context
* deterministic context reduction

---

# 151. Testing — Terminology

Test:

* LOCKED term honored
* conflicting LOCKED terms
* PREFERRED conflict
* optional Knowledge unavailable
* terminology limit exceeded
* locked-term provider violation

---

# 152. Testing — Batch

Test:

* empty Batch
* duplicate Unit
* oversized Batch
* valid multi-Unit Batch
* context-limit conflict
* immutable after READY

---

# 153. Testing — Provider Boundary

Test:

* unavailable provider
* rate limit
* quota exhausted
* request too large
* connection failure
* provider timeout
* provider internal error
* malformed provider output
* raw provider failure sanitization

---

# 154. Testing — Output

Test:

* valid output
* empty output
* missing Unit
* unknown Unit
* duplicate Unit
* wrong target language
* source leakage
* control leakage
* truncated output
* length anomaly
* incomplete translation

---

# 155. Testing — Alignment

Test:

* valid Unit alignment
* unknown source ref
* duplicate target
* missing TranslationUnit
* invalid source sequence
* SourceDocument identity mismatch

No ambiguous auto-realignment.

---

# 156. Testing — Candidate

Test:

* COMPLETE Candidate
* PARTIAL Candidate
* EMPTY_VALID Candidate
* invalid completeness
* missing lineage
* duplicate translated Unit
* provider credential leakage
* Runtime state leakage
* immutable after VALID
* submit once

---

# 157. Testing — Runtime Boundary

Test:

```text id="xj3ezs"
Candidate VALID
    ↓
Runtime rejects stale
```

Expect:

```text id="r3138p"
no TranslationModuleError
```

Test:

```text id="8ad9p7"
Cancellation observed
```

Expect:

```text id="n0seya"
no TRANSLATION_CANCELLED error
```

Test:

```text id="08n65l"
RetryHint
    ↓
Runtime creates new Attempt
```

---

# 158. Testing — Privacy / Security

Verify:

* LOCAL_ONLY blocks remote execution
* region restrictions enforced
* credential leakage blocked
* source content not logged
* translated content not logged
* prompt fragments rejected from output
* source instruction-like text remains data
* provider output cannot control metadata

---

# 159. Testing — Partial Output

Example:

```text id="84inuc"
10 TranslationUnits

7 valid
2 provider failed
1 missing
```

When allowed:

```text id="qjthbg"
Completeness = PARTIAL
```

with all failed/missing IDs explicit.

No silent omission.

---

# 160. Property Tests

```text id="66mc2e"
every TranslatedUnit
maps to valid TranslationUnit
```

```text id="av0jip"
every TranslationUnit
maps to SourceBlock evidence
```

```text id="csy166"
Candidate COMPLETE
implies no required Unit missing
```

```text id="msxl5h"
Candidate PARTIAL
lists every missing/failed required Unit
```

```text id="kpmkz7"
Candidate INVALID
cannot be submitted as VALID
```

```text id="db5f5p"
RetryHint
never creates Runtime Attempt
```

```text id="3953mf"
Runtime stale rejection
does not create Translation error
```

```text id="uwnfnd"
public error
contains no credentials
```

```text id="qne0fw"
public error
contains no full source or translated content
```

---

# 161. Core Error Invariants

1. Translation errors remain provider-neutral.
2. ErrorCode semantics are stable.
3. Retryability is explicit.
4. RetryHint is advisory.
5. Runtime owns retry execution.
6. Runtime owns cancellation.
7. Runtime owns deadline outcome.
8. Runtime owns stale authority.
9. Translation does not own Queue errors.
10. Translation does not own Scheduler errors.
11. Translation does not own Artifact publication errors.
12. Translation does not own Cache infrastructure errors.
13. Provider Management retains Provider lifecycle ownership.
14. Knowledge retains Knowledge persistence ownership.
15. Reading Session owns active variant selection.
16. SourceDocument is immutable.
17. TranslationUnit always preserves SourceBlock lineage.
18. Provider output is never trusted before validation.
19. Unknown Provider Unit IDs are never guessed.
20. Invalid alignment never becomes Candidate output.
21. Missing Units are explicit.
22. Failed Units are explicit.
23. PARTIAL is explicit.
24. EMPTY_VALID is valid.
25. Warnings do not masquerade as failures.
26. Fallback provider usage may be warning.
27. Provider credentials never enter errors.
28. Raw prompts never enter errors.
29. Raw provider responses never enter errors.
30. Source/translated content is minimized.
31. LOCKED terminology is not silently discarded.
32. Context privacy constraints cannot be weakened.
33. Remote execution cannot bypass LOCAL_ONLY.
34. Source instruction-like content remains untrusted data.
35. Provider control leakage invalidates affected output.
36. Candidate privacy violations fail closed.
37. Candidate validation failure prevents valid submission.
38. Candidate submission does not imply publication.
39. Runtime stale rejection is not Translation failure.
40. Active variant conflicts are not Translation execution errors.
41. Immutable variants are not mutated.
42. Translation does not maintain Job failure lifecycle.
43. Translation does not maintain Attempt failure lifecycle.
44. Translation-local failed state is not reset for retry.
45. External errors preserve original ownership.

---

# 162. MVP Error Set

Required MVP:

```text id="76244n"
TRN-INPUT-001
TRANSLATION_INPUT_INVALID

TRN-INPUT-002
SOURCE_DOCUMENT_UNAVAILABLE

TRN-INPUT-003
SOURCE_DOCUMENT_INCOMPATIBLE

TRN-INPUT-004
SOURCE_SELECTION_INVALID

TRN-INPUT-006
TARGET_LANGUAGE_INVALID


TRN-PLAN-001
TRANSLATION_PLAN_INVALID

TRN-PLAN-002
TRANSLATION_PROFILE_UNSUPPORTED

TRN-PLAN-003
PROVIDER_POLICY_UNSATISFIABLE


TRN-UNIT-001
TRANSLATION_UNIT_PLANNING_FAILED

TRN-UNIT-002
TRANSLATION_UNIT_SOURCE_MISSING


TRN-CTX-001
REQUIRED_CONTEXT_UNAVAILABLE

TRN-CTX-003
CONTEXT_TOO_LARGE


TRN-TERM-002
TERMINOLOGY_CONFLICT

TRN-TERM-003
LOCKED_TERMINOLOGY_VIOLATED


TRN-BATCH-003
TRANSLATION_BATCH_LIMIT_EXCEEDED

TRN-BATCH-004
TRANSLATION_BATCH_CONSTRUCTION_FAILED


TRN-PROV-001
PROVIDER_UNAVAILABLE

TRN-PROV-002
NO_ELIGIBLE_PROVIDER

TRN-PROV-005
PROVIDER_RATE_LIMITED

TRN-PROV-007
PROVIDER_REQUEST_TOO_LARGE

TRN-PROV-008
PROVIDER_CONNECTION_FAILED

TRN-PROV-009
PROVIDER_REQUEST_TIMEOUT

TRN-PROV-010
PROVIDER_INTERNAL_FAILURE


TRN-OUT-001
PROVIDER_OUTPUT_EMPTY

TRN-OUT-002
PROVIDER_OUTPUT_MALFORMED

TRN-OUT-005
PROVIDER_OUTPUT_UNKNOWN_UNIT

TRN-OUT-007
PROVIDER_OUTPUT_UNIT_MISSING

TRN-OUT-010
TARGET_LANGUAGE_MISMATCH

TRN-OUT-012
CONTROL_CONTENT_LEAKED


TRN-ALIGN-001
TRANSLATION_ALIGNMENT_FAILED

TRN-ALIGN-002
TRANSLATED_UNIT_SOURCE_MISSING


TRN-CAND-001
CANDIDATE_ASSEMBLY_FAILED

TRN-CAND-002
CANDIDATE_INVALID


TRN-PRIV-001
REMOTE_EXECUTION_PROHIBITED

TRN-SEC-001
CREDENTIAL_EXPOSURE_DETECTED


TRN-STATE-001
STATE_INVARIANT_VIOLATION


TRN-INT-001
INTERNAL_FAILURE

TRN-INT-006
ARCHITECTURE_INVARIANT_VIOLATION
```

---

# 163. MVP Warning Set

Required:

```text id="xbdklg"
NO_TRANSLATABLE_CONTENT

MISSING_OPTIONAL_CONTEXT

CONTEXT_TRUNCATED

LOW_TRANSLATION_CONFIDENCE

TERMINOLOGY_CONFLICT

SOURCE_LANGUAGE_UNCERTAIN

UNTRANSLATED_FRAGMENT

OUTPUT_LENGTH_ANOMALY

PROVIDER_FALLBACK_USED

PARTIAL_TRANSLATION

PRONOUN_AMBIGUITY

SPEAKER_RELATIONSHIP_AMBIGUITY
```

---

# 164. Removed Legacy Error Families

The following old concerns are intentionally removed or re-owned:

```text id="3s2d6a"
Translation command/job lifecycle validation
    → Runtime/API validation where lifecycle-specific

PreparedDocument errors
    → SourceDocumentArtifact input errors

PreparedSegment errors
    → TranslationUnit / SourceBlock errors

TranslationJob errors
    → Runtime WorkItem concerns

TranslationAttempt errors
    → Runtime Attempt concerns

Queue timeout
    → Runtime

Attempt timeout
    → Runtime

Job timeout
    → Runtime

Batch lifecycle timeout
    → Runtime unless specifically Provider request timeout

Retry limit exceeded
    → Runtime Retry Policy

Retry already active
    → Runtime concurrency

Cancellation
    → Runtime

Supersession
    → Runtime authority

Stale result
    → Runtime authority

Result publication failure
    → Artifact Store

Result persistence failure
    → Artifact Store / Storage

Cache read/write failure
    → Runtime Cache / Artifact infrastructure

Variant activation conflict
    → Reading Session/application

Duplicate active variant
    → Reading Session/application
```

---

# 165. Completion Criteria

This error contract is complete when:

* all Translation-owned failures have stable codes
* warning vs error distinction is explicit
* TranslationUnit failures are explicit
* Batch failures are explicit
* Provider failures are normalized
* provider output validation is explicit
* alignment failures are explicit
* Candidate failures are explicit
* partial behavior is explicit
* privacy/security failures are explicit
* Runtime lifecycle failures are external
* Provider lifecycle failures remain external
* Artifact publication failures remain external
* Cache infrastructure failures remain external
* active variant conflicts remain external
* RetryHint remains advisory
* source alignment is never guessed
* content/credential privacy is enforceable
* contract evolution is defined

---

# 166. Related Documents

```text id="6hikcs"
02-modules/translation/README.md
02-modules/translation/MODULE.md
02-modules/translation/CONTRACT.md
02-modules/translation/STATES.md
02-modules/translation/EVENTS.md

02-modules/text-processing/CONTRACT.md
02-modules/text-processing/ERRORS.md

02-modules/provider-management/
02-modules/knowledge/
02-modules/reading-session/
02-modules/presentation/

01-architecture/runtime/ERROR_MODEL.md
01-architecture/runtime/CANCELLATION.md
01-architecture/runtime/RETRY_POLICY.md
01-architecture/runtime/CACHE_POLICY.md
01-architecture/runtime/RESOURCE_LIFECYCLE.md

03-infrastructure/artifact-store/
03-infrastructure/resource-manager/
```

---

# 167. Summary

Translation Error Model covers failures inside:

```text id="kaptpp"
SourceDocumentArtifact
        ↓
Translation Plan
        ↓
Translation Units
        ↓
Context / Terminology
        ↓
Translation Batches
        ↓
Provider Boundary
        ↓
Provider Output Validation
        ↓
Alignment
        ↓
Candidate Assembly
```

Translation owns:

```text id="5nfln8"
translation semantic errors

provider-boundary normalization

provider output validation

Translation Unit alignment

Candidate validation

translation warnings

RetryHint
```

Runtime owns:

```text id="p196ny"
Queue

WorkItem

Attempt

Deadline

Retry execution

Cancellation

Supersession

Stale authority

Terminal outcome
```

Provider Management owns:

```text id="c7phwk"
Provider registry

Provider lifecycle

credentials

health

availability

local model residency
```

Artifact infrastructure owns:

```text id="u0zulu"
publication

retention

durable persistence

cache mechanics
```

Reading Session owns:

```text id="0ggpqn"
active translation selection
```

Core rule:

```text id="nh8r4t"
Translation owns errors
while turning a stable SourceDocument
into aligned translated semantic output.

Runtime owns
whether execution continues or still matters.

Provider Management owns
provider lifecycle failures.

Artifact Store owns
publication failures.

Reading Session owns
which translation is active for the reader.
```
