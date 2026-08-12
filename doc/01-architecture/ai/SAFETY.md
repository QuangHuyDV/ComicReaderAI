# AI Safety

* **Document:** AI Architecture / Safety
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

This document defines the safety architecture used by CRAI AI execution.

Safety protects:

* users,
* Workspace data,
* Project content,
* sensitive context,
* provider boundaries,
* model execution,
* generated output,
* internal instruction authority,

through explicit safety evaluation and controls.

Safety MUST remain:

* provider-neutral,
* defense-in-depth,
* policy-aware,
* privacy-first,
* least-privilege,
* explainable,
* observable,
* fail-safe.

Safety constrains AI execution.

It MUST NOT become the owner of every validation, Policy or runtime concern.

---

# Core Principle

```text
AI Request
    |
    v
Request / Context Validation
    |
    v
Safety Evaluation
    |
    v
Safe Execution Constraints
    |
    v
Prompt / Input Construction
    |
    v
Routing / Model Execution
    |
    v
Incremental / Final Safety Evaluation
    |
    v
AI Response
```

Safety MAY operate at several execution boundaries.

It does NOT replace generic validation or domain ownership.

---

# Scope

Safety architecture covers:

* safety policy evaluation,
* trusted/untrusted instruction boundaries,
* prompt-injection resistance,
* sensitive-data minimization,
* external-provider exposure controls,
* output-safety evaluation,
* streaming safety evaluation,
* tool/external-action constraints,
* safety findings,
* safety decisions,
* safety observability,
* safety-related recovery constraints.

---

# Non-Goals

Safety does NOT own:

* AI Request schema validation,
* Context semantic resolution,
* Glossary semantics,
* Character semantics,
* Translation completeness,
* Response schema validation,
* Routing selection,
* Workspace Policy ownership,
* Provider Configuration,
* Retry algorithms,
* Fallback selection,
* Audit persistence infrastructure,
* secrets storage.

---

# Safety vs Validation

Critical distinction:

```text
Validation
    = does the data satisfy its required contract?
```

```text
Safety
    = is execution/use permitted and acceptably safe?
```

Examples:

```text
JSON schema invalid
    -> Response Validation
```

```text
required mapping missing
    -> capability validation
```

```text
sensitive data prohibited from cloud
    -> Safety / Policy constraint
```

These MUST remain separate.

---

# Safety vs Policy

Workspace/Governance owns authoritative Policy.

Safety consumes safety-relevant Policy decisions.

Conceptually:

```text
Workspace Policy
      |
      v
Policy Evaluation
      |
      v
Safety Constraints
      |
      v
AI Safety
```

Safety MUST NOT redefine Workspace Policy semantics independently.

---

# Safety Decision

Recommended:

```text
AISafetyDecision
├── decision
├── findings[]
├── requiredControls[]
├── prohibitedActions[]
├── allowedExecutionModes[]
├── policyReferences[]
├── evaluatorVersion
└── evaluatedAt
```

Possible decisions:

```text
ALLOW
ALLOW_WITH_CONTROLS
REQUIRE_TRANSFORMATION
DENY
REQUIRE_USER_ACTION
REQUIRE_ADMIN_ACTION
```

---

# Safety Findings

Recommended:

```text
AISafetyFinding
├── code
├── category
├── severity
├── source
├── fieldReference?
├── contextReference?
├── action?
├── policyReference?
└── metadata?
```

Possible severity:

```text
INFO
WARNING
ERROR
BLOCKING
```

---

# Safety Categories

Possible categories MAY include:

```text
INSTRUCTION_INJECTION
SENSITIVE_DATA_EXPOSURE
UNTRUSTED_CONTEXT
POLICY_CONFLICT
EXTERNAL_PROCESSING_RESTRICTED
DATA_RESIDENCY_RISK
TOOL_RISK
OUTPUT_SAFETY
SECRET_EXPOSURE
CROSS_TENANT_RISK
PRIVACY_RISK
CUSTOM
```

The exact taxonomy SHOULD remain versioned.

---

# Safety Architecture

Recommended logical flow:

```text
Input / Context
      |
      v
Trust Classification
      |
      v
Safety Policy Evaluation
      |
      v
Input Controls
      |
      v
AI Execution
      |
      +--> Incremental Safety Checks
      |
      v
Final Safety Evaluation
      |
      v
Safety-Acceptable AI Response
```

Not every operation requires every check.

---

# Trust Classification

Safety SHOULD distinguish between:

```text
TRUSTED_INSTRUCTION
AUTHORIZED_CONTEXT
UNTRUSTED_SOURCE_DATA
EXTERNAL_CONTEXT
DERIVED_CONTEXT
UNKNOWN
```

Source content MUST NOT gain instruction authority merely because it contains instruction-like text.

---

# Instruction Authority

Safety consumes the instruction-authority model defined by Prompt Architecture.

Recommended semantic order:

```text
Mandatory Governance / Safety
        >
Protected Domain Authority
        >
Allowed Operation Intent
        >
Configured Intent
        >
Derived / Inferred Context
        >
Untrusted Source Data
```

Provider message roles MUST NOT redefine this ordering.

---

# Prompt Protection Boundary

Safety protects:

* instruction authority,
* separation of instructions from data,
* untrusted context boundaries,
* restricted instruction sources.

Safety does NOT require canonical concepts such as:

```text
system prompt
developer prompt
```

Those are provider-adapter representations.

---

# Prompt Injection

Prompt Injection is an attempt by untrusted content to alter instruction authority or execution behavior.

Possible indicators include:

* source content requesting hidden instructions,
* content telling the model to ignore operation rules,
* hidden markup attempting to redefine output contract,
* imported content attempting to activate tools,
* external/plugin content claiming administrative authority.

---

# Prompt Injection Is Contextual

Text such as:

```text
ignore previous instructions
```

is not automatically malicious.

For CRAI, it may simply appear inside:

* a novel,
* a comic,
* source dialogue,
* imported documentation.

Safety must evaluate whether the text is:

```text
data
```

or:

```text
authorized instruction
```

before acting on it.

---

# Injection Resistance

Recommended controls include:

* explicit data/instruction separation,
* context-source authority labels,
* provider-neutral instruction blocks,
* untrusted content delimiters,
* tool isolation,
* output contract validation,
* least privilege,
* context minimization.

Detection alone MUST NOT be the only defense.

---

# Injection Decision

When suspicious instruction-like text is found in untrusted content:

```text
preserve it as source data
```

where the requested capability requires processing it.

Do NOT automatically remove source text merely because it resembles instructions.

---

# Context Sanitization

Context sanitization MAY remove or transform:

* unsupported control characters,
* malicious markup,
* executable attachment metadata,
* unsafe tool directives.

It MUST NOT silently alter source semantics required for Translation or interpretation.

---

# Sanitization Boundary

Safety sanitization MUST distinguish:

```text
representation sanitization
```

from:

```text
semantic content modification
```

The latter requires explicit policy/capability semantics.

---

# Sensitive Data Protection

Sensitive information MAY include:

* personal information,
* private Project content,
* credentials,
* access tokens,
* internal secret references,
* licensed/unreleased material,
* Workspace-confidential content.

Safety SHOULD minimize unnecessary exposure.

---

# Secrets

Raw secrets MUST NOT enter ordinary AI Context or Prompt.

Examples:

```text
API key
OAuth token
private key
database password
```

Secrets remain in secure infrastructure.

Provider Adapter resolves credentials only at execution boundary.

---

# Secret References

Even secret references or internal identifiers SHOULD be omitted from model input unless they are semantically required.

---

# Sensitive Context

Sensitive context MAY still be legitimately required for an AI operation.

Safety evaluates:

```text
may this data be processed here?
```

rather than assuming:

```text
sensitive data may never be processed
```

Possible decisions:

```text
LOCAL_ONLY
APPROVED_PROVIDER_ONLY
REDACT
MINIMIZE
DENY
```

---

# Data Minimization

Only information required for the AI operation SHOULD be sent to the model.

Safety SHOULD cooperate with Context Assembly to minimize:

* unrelated chapters,
* unnecessary personal information,
* hidden metadata,
* irrelevant internal IDs,
* unneeded Memory,
* unnecessary full documents.

---

# Redaction

Redaction MAY be required before external execution.

Recommended:

```text
RedactionResult
├── transformedInputReference
├── redactionRules
├── removedCategories[]
├── reversible?
├── mappingReference?
└── contentHash
```

Redaction MUST preserve enough semantics for the requested operation or the operation SHOULD fail.

---

# Redaction vs Translation

For Translation, careless redaction may destroy meaning.

Therefore redaction policy MUST be capability-aware.

---

# External Processing

Safety may constrain:

```text
LOCAL
APPROVED_CLOUD
ANY_ALLOWED_PROVIDER
```

depending on:

* Workspace Policy,
* classification,
* data residency,
* user/admin consent,
* content sensitivity.

---

# Data Residency

Safety MAY consume data-residency constraints.

Routing enforces them when selecting Deployment/Region.

Safety does not select the Route itself.

---

# Provider Restrictions

Safety MAY produce:

```text
allowedProviderClasses
deniedProviderIds
requiredExecutionMode
```

as constraints.

Routing applies those constraints.

---

# Model Restrictions

Model eligibility MAY depend on required safety controls.

Safety may require:

* approved model class,
* tool isolation,
* structured output support,
* no provider-side retention,
* local-only model.

Again, Routing chooses the actual candidate.

---

# Tool Safety

Future tool-capable models introduce external-action risk.

Safety SHOULD define:

* allowed tools,
* tool scopes,
* confirmation requirements,
* argument validation,
* side-effect boundaries,
* tool-call authorization.

MVP MAY defer tool execution.

---

# Least Privilege

AI execution SHOULD receive only the minimum privileges required.

A model SHOULD NOT receive:

* arbitrary filesystem access,
* arbitrary network access,
* broad Workspace credentials,
* unrestricted connector access,

unless explicitly required and authorized.

---

# Output Safety

Final AI output MAY require safety evaluation.

Safety checks MAY consider:

* policy violations,
* sensitive-data leakage,
* secret leakage,
* forbidden transformations,
* tool/action risk,
* privacy leakage,
* cross-tenant leakage.

Generic schema/completeness checks remain outside Safety ownership.

---

# Output Safety vs Output Validation

Example:

```text
missing translation block
    -> Response Validation
```

```text
response exposes hidden secret
    -> Safety
```

```text
invalid Language code
    -> Response / Language validation
```

This separation MUST remain explicit.

---

# Safety Correction

Safety MAY authorize transformations such as:

* redaction,
* suppression,
* masking,
* safe replacement,
* structured warning.

It MUST NOT silently rewrite semantic output in ways that violate the requested business contract.

---

# Output Rejection

If required safety cannot be preserved:

```text
DENY / BLOCK
```

the finalized semantic response MUST NOT be treated as acceptable output.

---

# Streaming Safety

Streaming safety MAY operate incrementally.

Possible behavior:

```text
provider stream
    |
    v
provisional chunk
    |
    v
incremental safety check
    |
    +--> deliver provisionally
    |
    +--> hold
    |
    +--> suppress
    |
    +--> terminate
```

The exact policy depends on capability and sensitivity.

---

# Provisional Output

Provisional output MUST NOT be considered fully safety-approved unless the applicable Safety Policy explicitly allows incremental delivery.

---

# Final Safety Evaluation

Even when incremental checks run, final output MAY require a complete Safety decision before finalization.

---

# Streaming Cancellation

A blocking Safety finding MAY request cancellation of the current attempt.

Cancellation semantics belong to runtime/Streaming architecture.

Safety provides the decision/reason.

---

# Safety and Retry

Safety DENY is normally non-retryable.

Retry MUST NOT repeat the same forbidden operation merely to obtain a different output.

---

# Safety and Repair

A safety-related transformation MAY be attempted where policy allows.

Example:

```text
sensitive field detected
    ->
redact
    ->
re-evaluate
```

This is not generic model Retry.

---

# Safety and Fallback

Fallback MUST preserve Safety constraints.

Example:

```text
Cloud route unsafe
    ->
Fallback
    ->
Local RoutePlan
```

may be valid.

But:

```text
Safety requires local
    ->
Fallback
    ->
Cloud
```

is forbidden.

---

# Safety and Routing

Safety may provide hard routing constraints such as:

```text
localOnly
allowedRegions
approvedProviders
requiredSafetyCapabilities
```

Routing treats these as hard constraints.

---

# Safety and Cache

Cache hits MUST NOT bypass current safety/access requirements.

A cached result created under earlier execution conditions MAY still require:

* current authorization,
* current Policy check,
* current safety compatibility.

---

# Safety and Memory

Memory retrieval MUST obey Safety/Privacy constraints.

Sensitive Memory MUST NOT be exposed to providers merely because it is relevant.

---

# Safety and Context

Context Assembly SHOULD preserve:

* authority,
* sensitivity,
* provenance,
* Workspace scope.

Safety evaluates whether selected context may be used on the chosen execution class.

---

# Safety and Prompt

Prompt Builder must preserve:

* instruction authority,
* data/instruction separation,
* required safety controls.

Safety does not own Prompt Template semantics.

---

# Safety and Response

AIResponse MAY include normalized:

```text
safetyFindings[]
safetyDecisionReference?
```

if useful to the calling capability.

Detailed provider safety payloads MUST remain adapter/diagnostic data.

---

# Provider Safety Metadata

Providers may expose proprietary safety categories.

Provider Adapter SHOULD normalize these into CRAI Safety Findings where relevant.

Raw provider safety categories MUST NOT become canonical CRAI safety taxonomy automatically.

---

# Safety Decision Provenance

Material Safety decisions SHOULD preserve:

```text
policyRevision
evaluatorVersion
inputClassificationReference
contextHash?
routeClass?
decision
reasonCodes[]
```

without retaining unnecessary sensitive plaintext.

---

# Determinism

For identical:

* Safety Policy revision,
* normalized inputs,
* context classification,
* evaluator versions,
* route characteristics,

deterministic Safety rules SHOULD produce identical decisions.

AI-assisted safety evaluation MAY remain probabilistic and MUST be marked accordingly.

---

# AI-Assisted Safety Evaluation

If AI is used to evaluate Safety:

* it MUST NOT be sole control for high-risk hard Policy where deterministic checks are available,
* evaluator model/version SHOULD be traceable,
* recursive Safety failure MUST be handled,
* sensitive input exposure must still obey Policy.

MVP SHOULD prefer deterministic controls where practical.

---

# Safety Rules

Safety rules MAY be:

```text
DETERMINISTIC
CLASSIFIER_BASED
MODEL_ASSISTED
HYBRID
```

Rule type SHOULD be explicit.

---

# Safety Policy Revision

Safety-relevant rules SHOULD be versioned.

A durable execution SHOULD retain sufficient Safety Policy provenance when Safety materially affected the route/result.

---

# Safety Change During Execution

If Safety/Policy changes while an operation is running, architecture must define whether to:

```text
allow attempt to finish
cancel
re-evaluate before commit
re-evaluate before external send
```

This SHOULD remain explicit.

---

# Fail-Safe Behavior

When Safety state is unknown and the action may violate hard Policy:

```text
fail closed
```

is preferred.

For low-risk optional checks, policy MAY allow degraded operation.

---

# Fail Closed vs Fail Open

This MUST be defined per control.

Example:

```text
cannot verify cloud processing permission
    -> FAIL CLOSED
```

while:

```text
optional output classifier unavailable
    -> maybe continue with warning
```

depending on policy.

---

# Safety Failure Categories

Possible normalized failures:

```text
SAFETY_POLICY_DENIED
SAFETY_INPUT_RESTRICTED
SAFETY_CONTEXT_RESTRICTED
SAFETY_INSTRUCTION_BOUNDARY_INVALID
SAFETY_INJECTION_RISK
SAFETY_SECRET_EXPOSURE_RISK
SAFETY_EXTERNAL_PROCESSING_DENIED
SAFETY_DATA_RESIDENCY_VIOLATION
SAFETY_OUTPUT_BLOCKED
SAFETY_TOOL_DENIED
SAFETY_REDACTION_FAILED
SAFETY_EVALUATOR_UNAVAILABLE
SAFETY_POLICY_INVALID
SAFETY_CROSS_WORKSPACE_VIOLATION
```

---

# Recovery

Possible Safety-directed recovery actions:

```text
REDACT_AND_REEVALUATE
MINIMIZE_CONTEXT
REQUIRE_LOCAL_ROUTE
REQUIRE_APPROVED_PROVIDER
SUPPRESS_PROVISIONAL_OUTPUT
REQUEST_USER_ACTION
REQUEST_ADMIN_ACTION
DENY
```

Safety SHOULD NOT directly execute Retry/Fallback.

---

# Safety vs Recovery Ownership

Safety says:

```text
what is allowed
```

Recovery orchestration says:

```text
what to try next
```

This boundary MUST remain explicit.

---

# Audit

Material Safety decisions SHOULD be auditable when appropriate.

Examples:

* external processing denied,
* redaction applied,
* prompt-injection boundary violation,
* sensitive output blocked,
* tool call denied,
* local-only constraint applied.

---

# Audit Boundary

Safety does not own audit-storage infrastructure.

It emits safe structured audit information.

Audit persistence belongs to Audit/Infrastructure.

---

# Sensitive Audit

Audit records SHOULD NOT contain raw:

* Prompt,
* source content,
* secrets,
* full generated output,

unless explicitly required and protected.

Prefer:

```text
decisionId
finding codes
resource references
policy revision
hashes
```

---

# Observability

Recommended metrics:

* safety evaluations,
* allow/deny counts,
* allow-with-controls count,
* redaction count,
* context-minimization count,
* injection-risk findings,
* blocked output count,
* local-only enforcement count,
* provider restriction count,
* Safety evaluator latency,
* Safety evaluator failures.

---

# Safety Metrics vs Audit

Metrics answer:

```text
how often?
```

Audit answers:

```text
what materially happened and why?
```

They MUST remain distinct.

---

# Sensitive Observability

Safety telemetry MUST avoid raw sensitive content by default.

---

# Architecture Invariants

1. Safety is provider-neutral.

2. Safety uses defense in depth.

3. Safety is Policy-aware but does not own all Workspace Policy semantics.

4. Safety does not replace generic Request validation.

5. Safety does not replace Response schema validation.

6. Safety does not own Translation completeness validation.

7. Safety does not own Routing.

8. Safety does not own Retry.

9. Safety does not own Fallback.

10. Safety does not own Audit persistence.

11. Safety decisions SHOULD be explicit and structured.

12. Safety constraints affecting Routing MUST be expressed as hard constraints where mandatory.

13. Safety DENY MUST NOT be bypassed by Retry.

14. Safety DENY MUST NOT be bypassed by Fallback.

15. Fallback MUST preserve mandatory Safety requirements.

16. Prompt Safety protects semantic instruction authority, not provider-specific role names.

17. `system/developer/user` roles are not canonical Safety authority concepts.

18. Untrusted source content MUST NOT gain instruction authority merely because it contains instruction-like text.

19. Prompt Injection defense MUST rely on authority separation, not keyword detection alone.

20. Instruction-like text inside books/comics MUST normally remain processable as source data.

21. Context sanitization MUST NOT silently alter required semantic source content.

22. Secrets MUST NOT enter ordinary AI Context/Prompt.

23. Credentials remain in secure infrastructure.

24. Sensitive context SHOULD be minimized.

25. Sensitive context MAY require local/approved-provider execution rather than blanket rejection.

26. Data residency constraints MUST be preserved through Routing/Fallback.

27. Provider restrictions MUST be enforced before external execution.

28. Model restrictions MAY be safety constraints.

29. Least privilege applies to AI execution and tool access.

30. Output Safety and Output Contract Validation remain distinct.

31. Safety MAY operate incrementally during Streaming.

32. Provisional output MAY require Safety gating.

33. Final required Safety evaluation MUST complete before acceptable finalization where policy requires it.

34. Safety may request cancellation but runtime owns cancellation execution.

35. Safety Retry behavior MUST follow normalized recovery policy.

36. Safety-related transformations require re-evaluation when necessary.

37. Cache hits MUST NOT bypass current safety/access checks.

38. Memory retrieval MUST obey Safety/Privacy constraints.

39. Context authority/sensitivity metadata SHOULD remain available to Safety.

40. Prompt Builder MUST preserve required Safety controls.

41. Provider-specific Safety metadata MUST be normalized.

42. Raw provider Safety taxonomy MUST NOT become canonical automatically.

43. Material Safety decisions SHOULD preserve policy/evaluator provenance.

44. Deterministic Safety rules SHOULD produce deterministic decisions for identical explicit inputs.

45. AI-assisted Safety decisions MUST be identifiable as model-assisted.

46. High-risk hard controls SHOULD prefer deterministic enforcement where practical.

47. Safety rules SHOULD be versioned.

48. Unknown state for hard safety controls SHOULD fail closed.

49. Safety recovery actions MUST NOT silently weaken hard Policy.

50. Safety observability MUST avoid sensitive plaintext by default.

51. Safety audit and telemetry remain distinct.

52. Cross-Workspace data leakage is always a blocking safety violation.

53. Safety MUST preserve tenant isolation.

54. New providers/models MUST integrate through normalized Safety capabilities rather than provider-specific business logic.

---

# Recommended MVP Scope

CRAI MVP SHOULD support:

* provider-neutral `AISafetyDecision`,
* structured Safety Findings,
* deterministic Policy-based safety rules,
* instruction/data separation,
* prompt-injection resistance,
* sensitive-context classification,
* secret exclusion,
* context minimization,
* local-only enforcement,
* provider allow/deny enforcement,
* data-residency constraint,
* basic redaction hooks,
* output secret-leak checks,
* external-processing checks,
* Streaming provisional-output gating,
* normalized provider safety metadata,
* Safety audit events,
* Safety observability,
* fail-closed behavior for mandatory external-processing constraints.

MVP MAY defer:

* tool-call safety,
* autonomous-agent safety,
* advanced jailbreak classifiers,
* AI-assisted policy interpretation,
* semantic PII detection,
* complex reversible redaction,
* advanced human-review workflows,
* adaptive safety models,
* cross-provider safety calibration,
* multimodal safety classifiers,
* advanced risk scoring.

---

# Open Decisions

The following SHOULD remain open until prototype validation:

* exact `AISafetyDecision` schema,
* exact Safety Finding taxonomy,
* exact severity levels,
* instruction-authority representation shared with `PROMPTS.md`,
* prompt-injection detection depth,
* deterministic vs classifier-based detection split,
* Context sensitivity classification,
* redaction ownership,
* redaction mapping persistence,
* whether redaction occurs before or during Context Assembly,
* local-only Policy representation,
* provider data-retention capability metadata,
* data-residency capability metadata,
* output-safety evaluator ownership,
* Streaming gating strategy,
* fail-open/fail-closed matrix,
* Safety Policy revision model,
* Safety evaluator versioning,
* Safety change behavior during running operation,
* cache-hit Safety revalidation,
* Memory safety filtering,
* provider-native safety metadata retention,
* audit retention,
* user-visible Safety explanation policy,
* administrator override model,
* future tool/action safety architecture.

---

# Related Documents

AI Architecture:

* `README.md`
* `PIPELINE.md`
* `STAGES.md`
* `REQUEST.md`
* `RESPONSE.md`
* `CONTEXT.md`
* `MEMORY.md`
* `PROMPTS.md`
* `MODELS.md`
* `ROUTING.md`
* `STREAMING.md`
* `RETRY.md`
* `FALLBACK.md`
* `CACHE.md`
* `COST_CONTROL.md`
* `OBSERVABILITY.md`

Domain:

* `../domain/WORKSPACE.md`
* `../domain/PROJECT.md`
* `../domain/PROFILE.md`
* `../domain/SESSION.md`

Modules:

* `../../02-modules/provider-management/`
* `../../02-modules/preferences/`
* `../../02-modules/translation/`
* `../../02-modules/recognition/`

Infrastructure:

* `../../03-infrastructure/logging/`
* `../../03-infrastructure/telemetry/`
* `../../03-infrastructure/storage/`

Runtime:

* `../runtime/PIPELINE_RUNTIME.md`
* `../runtime/CANCELLATION.md`
* `../runtime/RUNTIME_CONFIG.md`
* `../runtime/RUNTIME_OBSERVABILITY.md`
