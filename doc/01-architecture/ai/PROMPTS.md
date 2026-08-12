# Prompt Architecture

* **Document:** AI Architecture / Prompts
* **Version:** 2.0.0
* **Status:** Draft
* **Owner:** CRAI Architecture

---

# Purpose

This document defines how CRAI converts validated AI operation intent and an assembled AI Context Package into a model-facing input representation.

Prompt construction is a **derived execution concern**.

It transforms:

```text
AI Request Intent
        +
AI Context Package
        +
Output Requirements
        +
Applicable Instructions
        |
        v
Provider-Neutral Model Input
```

The Prompt architecture MUST remain independent from:

* provider message-role syntax,
* provider SDK objects,
* business-domain ownership,
* mutable Session state,
* mutable Glossary state,
* mutable Character state,
* runtime retry history.

---

# Core Principle

```text
Domain / Capability Intent
        |
        v
AI Request
        |
        v
Context Assembly
        |
        v
AI Context Package
        |
        v
Prompt / Input Construction
        |
        v
Provider-Neutral Model Input
        |
        v
Provider Adapter
        |
        v
Provider-Specific Request
```

Prompt construction does not own business truth.

It renders already-resolved intent and context into a form suitable for AI execution.

---

# Scope

Prompt Architecture MAY support:

* Translation,
* summarization,
* classification,
* structured extraction,
* Character inference,
* semantic validation,
* multimodal reasoning,
* optional OCR correction,
* other generation-oriented AI capabilities.

Not every AI operation requires a textual prompt.

For example:

```text
Embedding
```

may use direct input rather than instruction-oriented Prompt construction.

Therefore the broader stage SHOULD be understood as:

```text
Prompt / Input Construction
```

---

# Non-Goals

Prompt Architecture does NOT own:

* Glossary resolution,
* Character resolution,
* Profile resolution,
* Language resolution,
* Session lifecycle,
* Memory retrieval,
* context ranking,
* provider routing,
* provider credentials,
* business-domain validation,
* Translation publication,
* Presentation rendering.

---

# Design Principles

Prompt/Input Construction SHOULD be:

* provider-neutral,
* deterministic for identical resolved inputs,
* template-driven where useful,
* schema-aware,
* versioned,
* reusable,
* capability-specific,
* explicit,
* observable,
* minimal,
* safe against instruction/data ambiguity.

---

# Provider-Neutral Instruction Model

CRAI SHOULD NOT make provider-specific roles such as:

```text
system
developer
user
assistant
```

the canonical architecture model.

Instead CRAI SHOULD use semantic instruction categories.

Recommended:

```text
AIInstructionSet
├── governanceInstructions[]
├── capabilityInstructions[]
├── operationInstructions[]
├── contextualInstructions[]
├── outputInstructions[]
└── dataSections[]
```

Provider adapters MAY map these into provider-specific message roles.

---

# Instruction Categories

## Governance Instructions

Represent non-negotiable execution constraints already approved by policy/safety architecture.

Examples:

* do not expose hidden context,
* obey required output constraints,
* respect data-handling rules,
* enforce mandatory safety behavior.

Governance Instructions MUST derive from authoritative resolved policy.

They are not arbitrary prompt text.

---

# Capability Instructions

Describe the general semantics of the requested AI capability.

Examples:

```text
Translate source text into target Language.
```

```text
Classify the input according to the supplied schema.
```

```text
Extract structured entities from the supplied content.
```

These instructions SHOULD be reusable across operations.

---

# Operation Instructions

Represent explicit operation-specific intent.

Examples:

* translate literally,
* preserve ambiguity,
* summarize this Chapter,
* explain this paragraph,
* keep one term unchanged for this operation.

Operation Instructions MAY originate from:

* AI Request,
* Session override,
* explicit user intent,
* resolved Profile.

They MUST already satisfy applicable Policy before Prompt construction.

---

# Contextual Instructions

Contextual instructions explain how supplied context should be interpreted.

Examples:

* terminology entries are authoritative,
* Character relationship context is informational,
* previous dialogue is supporting context,
* source block mapping keys must be preserved.

Prompt Builder MUST NOT invent new authority rules.

It serializes authority already decided by context/domain resolution.

---

# Output Instructions

Output Instructions define the expected logical result contract.

Examples:

* return one mapped result for every input key,
* produce valid structured object,
* output in Vietnamese,
* do not add commentary,
* preserve required identifiers.

These SHOULD derive from `AIOutputRequirements`.

---

# Data Sections

Data Sections contain semantic inputs and supporting context.

Examples:

```text
Primary Source
Glossary Context
Character Context
History Context
Memory Context
Structured Metadata
```

The set of sections is capability-specific.

No section is universally mandatory except what the operation contract requires.

---

# Prompt Is Not Domain Truth

Critical rule:

```text
Prompt
    = execution representation
```

not:

```text
Prompt
    = canonical business configuration
```

A Prompt MAY contain a serialized representation of:

* GlossarySnapshot,
* CharacterContextSnapshot,
* Profile intent,
* Session-derived context.

The owning truth remains outside Prompt.

---

# Prompt Artifact

Recommended conceptual structure:

```text
PromptArtifact
├── promptArtifactId?
├── capabilityType
├── templateReferences[]
├── instructionSet
├── contextPackageReference?
├── outputSchemaReference?
├── modelInputRepresentation
├── promptVersion
├── builderVersion
├── contentHash
└── provenance
```

Whether PromptArtifact is persisted is an implementation/retention decision.

---

# Prompt Template

A Prompt Template is a reusable model-input composition definition.

Recommended:

```text
PromptTemplate
├── templateId
├── templateVersion
├── capabilityType
├── compatibleInputContract
├── compatibleOutputContract
├── requiredContextTypes[]
├── optionalContextTypes[]
├── instructionLayout
├── formattingRules
└── compatibilityMetadata
```

---

# Template Identity

Every template SHOULD have:

```text
templateId
templateVersion
```

Template identity is separate from:

* Profile Revision,
* AI Request ID,
* model ID,
* provider ID.

---

# Template Ownership

Templates belong to AI Prompt architecture/configuration.

They MUST NOT become part of:

* Translation domain truth,
* Character domain truth,
* Glossary domain truth,
* Session state.

Historical execution MAY reference exact template versions for reproducibility.

---

# Template Selection

Template Selection SHOULD depend on semantic requirements such as:

* capability,
* input type,
* output contract,
* context types,
* model capabilities,
* structured-output requirements.

It SHOULD NOT primarily depend on provider identity.

---

# Provider-Specific Templates

Provider-specific template variants MAY exist when technically necessary.

They MUST remain adapter/runtime specializations.

Example:

```text
canonical Translation template
        |
        +--> generic chat representation
        +--> provider-X specialization
```

The provider-specific variant MUST NOT redefine business semantics.

---

# Prompt Composition

Prompt Composition converts:

```text
Instruction Set
+
Context Package
+
Output Requirements
+
Template
```

into:

```text
Provider-Neutral Model Input
```

Composition MUST preserve semantic distinctions.

---

# Model Input Representation

Possible provider-neutral representations include:

```text
InstructionBlocks
MessageLikeBlocks
StructuredInput
MultimodalInput
SchemaBoundInput
ToolAwareInput
```

CRAI MUST NOT require every model to use chat messages.

---

# Message-Like Representation

A provider-neutral message representation MAY exist.

Example:

```text
ModelInputMessage
├── semanticRole
├── contentBlocks[]
└── metadata?
```

Possible semantic roles MAY include:

```text
GOVERNANCE
CAPABILITY
OPERATION
CONTEXT
INPUT
OUTPUT_CONTRACT
```

Provider Adapter decides whether these map to:

```text
system
developer
user
```

or another provider-specific mechanism.

---

# System Prompt Boundary

CRAI MAY generate a provider-specific `system` message.

But:

```text
System Prompt
```

is NOT the canonical highest-authority business concept.

Authority is resolved before provider adaptation.

A provider that lacks a System Prompt must still preserve the same CRAI semantic constraints through another supported representation.

---

# Developer Prompt Boundary

Likewise:

```text
Developer Prompt
```

is not a required CRAI domain concept.

Application/capability instructions SHOULD be represented semantically before adapter mapping.

---

# User Prompt Boundary

Explicit user intent is represented through operation instructions.

It MAY map to provider `user` content.

It MUST NOT be treated as unrestricted authority.

User intent remains constrained by:

* Policy,
* Safety,
* protected domain authority,
* output contract.

---

# Instruction Authority

Recommended semantic ordering:

```text
Mandatory Governance / Safety
        >
Protected Domain Authority
        >
Explicit Allowed Operation Intent
        >
Configured Capability Intent
        >
Derived / Inferred Context
```

Exact conflict semantics remain capability-specific.

Provider message roles MUST NOT redefine this authority model.

---

# Glossary Context

Glossary terminology comes from:

```text
GlossarySnapshot
```

through the AI Context Package.

Prompt Builder MAY serialize selected Glossary context.

It MUST NOT:

* resolve competing Glossary Entries,
* choose terminology precedence,
* mutate terminology,
* fetch mutable Glossary state.

---

# Character Context

Character context comes from:

```text
CharacterContextSnapshot
```

through AI Context.

Prompt Builder MAY represent:

* speaker,
* listener,
* relevant names,
* relationship,
* speech style,
* spoiler-safe facts.

Prompt Builder MUST NOT resolve Character truth itself.

---

# Memory Context

Memory content enters Prompt only after:

```text
Memory Retrieval
        |
        v
Context Selection
        |
        v
AI Context Package
```

Prompt Builder MUST NOT independently retrieve hidden Memory.

---

# Session Context

Prompt Builder MUST NOT read mutable Session state directly.

Relevant Session intent must already be represented in:

* AI Request,
* SessionContextSnapshot,
* Context Package,
* Resolved Configuration.

---

# Profile Boundary

Profile does not become raw prompt text directly.

Recommended:

```text
Profile Revision
        |
        v
Resolved Configuration
        |
        v
Instruction Semantics
        |
        v
Prompt Composition
```

This allows Prompt templates to evolve without changing Profile business identity.

---

# Profile Intent Mapping

Example:

```text
Translation Profile:
    style = NATURAL
    preserveAmbiguity = true
```

may become semantic instructions:

```text
Produce natural Vietnamese.
Do not resolve source ambiguity without evidence.
```

The mapping itself SHOULD be versioned.

---

# Prompt Compiler

A Prompt Compiler MAY translate structured Profile/Context semantics into canonical instructions.

Recommended:

```text
PromptCompiler
├── compilerVersion
├── capabilityMappings
├── instructionMappings
└── outputMappings
```

Changing compiler behavior MAY affect output and cache compatibility.

---

# Output Schema

Expected output structure comes from the AI Request output contract.

Prompt Builder MAY serialize it for the model.

Example:

```text
OutputSchemaReference
    translation-block-map.v1
```

Provider Adapter may then translate that into:

* JSON Schema,
* structured-output API,
* tool schema,
* text instructions.

---

# Output Schema Is Not Prompt-Owned

Prompt Architecture presents the output contract.

It does not own the logical response schema.

The canonical logical schema belongs to AI Request/Response contracts or capability-specific contracts.

---

# Formatting Rules

Prompt-level formatting rules SHOULD describe model output representation.

Examples:

* valid structured object,
* one item per mapping key,
* no extra commentary.

Visual formatting such as:

* font,
* line height,
* overlay layout,
* rich UI,

belongs to Presentation, not Prompt.

---

# Markdown

Markdown MAY be an output encoding when explicitly required.

It SHOULD NOT be used as a universal default merely because chat models understand it.

---

# Context Injection

Context Injection serializes already-selected Context Package items.

Prompt Builder MUST preserve:

* context type,
* relevant authority,
* data/instruction separation,
* mapping identifiers,
* source boundaries.

---

# Data vs Instruction Separation

Untrusted source content MUST be clearly represented as data.

Example conceptual structure:

```text
Instruction:
    Translate the supplied source.

Data:
    <source>
        ...
    </source>
```

The exact delimiters are template-specific.

The security goal is semantic separation.

---

# Prompt Injection Resistance

Source content, Memory, external context and plugin context MAY contain instruction-like text.

Prompt Builder SHOULD treat them as untrusted data unless the context source is explicitly authorized as instruction authority.

This reduces accidental instruction injection.

---

# Instruction Provenance

Material instructions SHOULD be traceable to sources such as:

```text
Workspace Policy
Resolved Profile Revision
AI Request
Session Override
Capability Default
Output Contract
```

Prompt strings SHOULD NOT become unexplained hidden policy.

---

# Prompt Determinism

For identical:

* Prompt Template version,
* Instruction Set,
* AI Context Package,
* Output Requirements,
* Prompt Compiler version,

Prompt Composition SHOULD produce semantically equivalent model input.

---

# Semantic vs Byte Determinism

Byte-identical output is desirable for deterministic builders but not always required.

For example:

* stable JSON formatting may differ,
* metadata ordering may differ.

Cache identity SHOULD use normalized semantic representation where practical.

---

# Prompt Hash

A finalized model input SHOULD expose a deterministic semantic fingerprint.

Possible:

```text
promptHash
```

Inputs SHOULD include:

* template version,
* compiler version,
* instruction semantics,
* selected context hash,
* output contract,
* capability.

---

# Prompt Version

`Prompt Version` SHOULD identify the effective prompt-generation contract.

It MAY be composed from:

```text
templateVersion
+
compilerVersion
+
instructionMappingVersion
```

This is more meaningful than one mutable application-wide prompt version.

---

# Pipeline Compatibility

Templates MAY declare compatible:

* AI Request schema,
* AI Response schema,
* capability version,
* Context Package version,
* model capabilities.

A single hard-coded:

```text
Compatible Pipeline Version
```

MAY be too broad.

Compatibility SHOULD be explicit by contract/capability.

---

# Model Compatibility

A Prompt Template MAY require capabilities such as:

```text
SYSTEM_INSTRUCTION
STRUCTURED_OUTPUT
MULTIMODAL_INPUT
TOOL_CALLING
LONG_CONTEXT
```

It SHOULD NOT require a specific provider unless the template is explicitly provider-specialized.

---

# Prompt Optimization Boundary

The previous Prompt architecture assigned:

* duplicate removal,
* history compression,
* low-priority truncation,
* context reordering

to Prompt Builder.

These SHOULD primarily belong to Context Assembly.

Prompt Builder MAY perform only representation-level optimization.

---

# Representation Optimization

Allowed examples:

* remove empty sections,
* compact redundant labels,
* choose shorter equivalent schema notation,
* serialize data efficiently,
* omit unsupported optional wrappers.

Representation optimization MUST NOT change selected semantic context.

---

# Context Reduction Is Not Prompt Optimization

If a model input exceeds context limits because too much semantic context was selected:

```text
return to Context Budget / Route Resolution
```

rather than silently dropping arbitrary context inside Prompt Builder.

---

# Final Context Compatibility Check

After model routing, Prompt Builder MAY detect that the selected model cannot accept the final input size.

It SHOULD return a structured condition such as:

```text
PROMPT_CONTEXT_LIMIT_EXCEEDED
```

Orchestration may then:

* choose a larger-context model,
* rerun Context Reduction,
* reject the operation.

Prompt Builder MUST NOT silently discard required context.

---

# Prompt Validation

Prompt/Input Validation SHOULD verify:

* required semantic instruction categories,
* required primary input,
* required context sections,
* output contract representability,
* template compatibility,
* input-size compatibility,
* model-capability compatibility,
* data/instruction boundaries,
* forbidden provider leakage,
* unresolved placeholders.

---

# Safety Boundary

Safety policy MUST be resolved through the Safety/Policy stages.

Prompt validation MAY verify that required safety/governance instructions are represented correctly.

Prompt Builder does NOT independently define global Safety Policy.

---

# Prompt Injection Validation

Validation MAY inspect whether:

* untrusted content was placed into instruction sections,
* external/plugin context claims unauthorized authority,
* delimiters/structured encoding are malformed.

This is execution safety validation, not content-domain truth.

---

# Missing Template

If no compatible template exists, Prompt Builder SHOULD fail explicitly.

It MUST NOT silently use an unrelated template with different semantics.

---

# Template Fallback

Template fallback MAY occur only among semantically compatible variants.

Example:

```text
structured-output template
    ->
plain structured-instruction template
```

if the output contract remains equivalent.

---

# Prompt Recovery

Possible recovery includes:

* select compatible template,
* select provider-neutral alternate representation,
* rerun Context Reduction,
* choose another model route,
* reject unsupported capability.

Recovery MUST preserve operation intent.

---

# Prompt Simplification

Prompt simplification MAY remove representational redundancy.

It MUST NOT remove:

* mandatory governance,
* required output rules,
* required source content,
* authoritative required context.

---

# Provider Request Adaptation

After Prompt/Input Construction:

```text
Provider-Neutral Model Input
        |
        v
Provider Request Adapter
```

The adapter owns:

* provider roles,
* API message shape,
* tool-definition format,
* schema dialect,
* multimodal parts,
* provider-specific metadata.

---

# Provider Role Mapping

Example:

```text
GOVERNANCE
    -> system

CAPABILITY + OPERATION
    -> developer

INPUT + selected context
    -> user
```

for one provider.

Another provider may use a completely different mapping.

Both should preserve the same CRAI semantic input.

---

# Provider Constraint Degradation

If a provider cannot represent a required instruction hierarchy safely, Route Planning SHOULD consider the provider incompatible.

Prompt Builder MUST NOT silently weaken mandatory semantics to fit the provider.

---

# Prompt vs Request

```text
AI Request
    = semantic execution intent
```

```text
Prompt / Model Input
    = derived representation of that intent
```

Therefore:

```text
AIRequest
    !=
Prompt
```

---

# Prompt vs Context

```text
AI Context Package
    = selected structured semantic context
```

```text
Prompt
    = representation of context + instructions for a model
```

Prompt Builder MAY reorganize representation.

It MUST NOT silently change Context truth.

---

# Prompt vs Memory

Memory is a retrievable knowledge source.

Prompt sees only Memory items that already entered the Context Package.

---

# Prompt vs Profile

Profile expresses reusable business intent.

Prompt serializes the effective resolved meaning.

Prompt strings are not Profile truth.

---

# Prompt vs Provider Request

```text
Provider-Neutral Model Input
    !=
Provider-Specific Request
```

The latter belongs to Provider Adapter.

---

# Prompt vs Cache

Prompt hash MAY participate in AI-execution cache identity.

However cache SHOULD also consider other semantic/execution inputs such as:

* model identity,
* context/configuration hashes,
* capability,
* parameters.

Prompt hash alone may be insufficient.

---

# Prompt Retention

Prompt artifacts MAY contain copyrighted/private content.

Default retention SHOULD be limited.

Possible policy:

```text
Prompt metadata/hash
    longer-lived

Full Prompt content
    short-lived or disabled

Provider-specific request
    diagnostic retention only
```

Historical business artifacts SHOULD NOT require long-term full Prompt retention to remain meaningful.

---

# Prompt Reproducibility

When debugging/reproducibility requires it, preserve:

```text
templateId
templateVersion
compilerVersion
contextHash
instructionSetHash
outputSchemaReference
promptHash
modelInputContractVersion
```

This MAY be sufficient without retaining plaintext Prompt indefinitely.

---

# Observability

Prompt-generation observability MAY include:

* template ID/version,
* compiler version,
* build duration,
* input size,
* estimated tokens/units,
* context size,
* instruction count,
* data section count,
* output schema reference,
* prompt hash,
* compatibility failure type.

---

# Observability Boundary

Prompt content MUST NOT be logged by default.

Prefer:

```text
promptHash
size
templateVersion
compilerVersion
```

Raw Prompt logging requires explicit policy.

---

# Usage Estimation

Prompt Builder MAY estimate:

* token count,
* characters,
* bytes,
* image/input units.

Final provider usage remains execution metadata.

Prompt Builder MUST NOT become authoritative Usage accounting.

---

# Failure Conditions

Possible stable failures:

```text
PROMPT_TEMPLATE_NOT_FOUND
PROMPT_TEMPLATE_INCOMPATIBLE
PROMPT_INPUT_INVALID
PROMPT_CONTEXT_INVALID
PROMPT_OUTPUT_SCHEMA_INVALID
PROMPT_REQUIRED_INSTRUCTION_MISSING
PROMPT_UNRESOLVED_PLACEHOLDER
PROMPT_CONTEXT_LIMIT_EXCEEDED
PROMPT_MODEL_CAPABILITY_INCOMPATIBLE
PROMPT_PROVIDER_NEUTRAL_REPRESENTATION_FAILED
PROMPT_INSTRUCTION_AUTHORITY_INVALID
PROMPT_DATA_INSTRUCTION_BOUNDARY_INVALID
PROMPT_BUILD_FAILED
```

---

# Architecture Invariants

1. Prompt construction is a derived AI execution concern.

2. Prompt does not own canonical business truth.

3. AI Request exists before Prompt Construction.

4. AI Context Package exists before Prompt Composition when context is required.

5. Prompt Builder MUST NOT independently resolve mutable Glossary state.

6. Prompt Builder MUST NOT independently resolve Character truth.

7. Prompt Builder MUST NOT read mutable Session state directly.

8. Prompt Builder MUST NOT retrieve arbitrary Memory directly.

9. Prompt Builder consumes already-resolved semantic inputs.

10. Prompt architecture MUST remain provider-neutral.

11. Provider-specific message roles MUST NOT be canonical CRAI instruction semantics.

12. `system/developer/user` roles MAY exist only as provider-adapter representations.

13. System Prompt MUST NOT be treated as the canonical business authority model.

14. Mandatory Policy/Safety authority exists before prompt role mapping.

15. Explicit user intent MUST NOT bypass mandatory Policy or protected domain authority.

16. Prompt Templates are versioned.

17. Prompt Compiler/mapping behavior SHOULD be versioned.

18. Template identity is separate from Profile identity.

19. Template identity is separate from provider identity.

20. Provider-specific template variants MUST NOT redefine business intent.

21. Prompt structure is capability-specific.

22. Glossary is not a mandatory universal Prompt section.

23. Character Context is not a mandatory universal Prompt section.

24. Not every AI capability requires a textual Prompt.

25. Prompt Builder MUST distinguish instructions from untrusted source/context data.

26. External/plugin/source content MUST NOT gain instruction authority merely because it contains instruction-like text.

27. Prompt Injection resistance SHOULD be part of composition/validation.

28. Context selection, ranking and semantic reduction primarily belong to Context Assembly.

29. Prompt Builder MUST NOT silently truncate authoritative required context.

30. Representation-level optimization MUST preserve semantic context.

31. Output contract belongs to AI Request/Response or capability contract, not Prompt ownership.

32. Prompt may serialize a logical output schema.

33. Provider-specific schema conversion belongs to Provider Adapter.

34. Prompt composition SHOULD be deterministic for identical resolved inputs and versions.

35. Prompt semantic hash SHOULD be deterministic.

36. Prompt versioning SHOULD preserve template/compiler provenance.

37. Prompt Validation occurs before model execution.

38. Missing/incompatible templates MUST fail explicitly.

39. Template fallback MUST preserve semantic compatibility.

40. Prompt simplification MUST NOT weaken mandatory semantics.

41. Route Planning SHOULD reject providers/models incapable of safely representing required semantics.

42. Provider Request Adaptation occurs after Prompt/Input Construction.

43. Prompt is not AI Request.

44. Prompt is not AI Context Package.

45. Prompt is not Memory.

46. Prompt is not Profile.

47. Prompt is not Provider Request.

48. Prompt is not domain historical truth.

49. Prompt content SHOULD NOT be logged by default.

50. Full Prompt retention SHOULD be policy-controlled.

51. Reproducibility SHOULD prefer hashes/version references over unnecessary plaintext retention.

52. Prompt-generation observability MUST remain separate from semantic business results.

53. Usage estimation by Prompt Builder is not authoritative billing/usage truth.

54. Prompt failures SHOULD use stable normalized categories.

55. New Prompt components MUST preserve instruction authority and provider neutrality.

---

# Recommended MVP Scope

CRAI MVP SHOULD support:

* provider-neutral instruction categories,
* Translation Prompt Template,
* Language Detection Input Template where needed,
* structured Translation output template,
* template ID/version,
* Prompt Compiler version,
* capability-based Template Selection,
* Governance Instructions,
* Capability Instructions,
* Operation Instructions,
* Output Instructions,
* Primary Input section,
* optional Glossary Context section,
* optional Character Context section,
* optional History Context section,
* explicit data/instruction separation,
* structured output schema reference,
* deterministic Prompt Composition,
* prompt semantic hash,
* size/token estimation,
* context-limit validation,
* template compatibility validation,
* unresolved-placeholder validation,
* provider-neutral model-input contract,
* safe observability.

MVP MAY defer:

* generic tool-calling templates,
* agent prompts,
* multi-agent instruction hierarchy,
* provider-specific optimized templates,
* AI-generated Prompt optimization,
* adaptive Prompt experimentation,
* automatic template selection learning,
* complex multimodal Prompt DSL,
* prompt marketplace,
* cross-Workspace template sharing,
* full Prompt retention,
* advanced injection classifiers.

---

# Open Decisions

The following SHOULD remain open until prototype validation:

* exact provider-neutral instruction representation,
* exact semantic-role taxonomy,
* whether `PromptArtifact` is persisted,
* whether `PromptArtifact` receives its own ID,
* exact Template schema,
* exact Prompt Compiler abstraction,
* whether Template and Compiler versions form one combined Prompt version,
* exact capability-to-template selection mechanism,
* provider-specific template specialization rules,
* exact Profile-to-instruction mapping,
* exact Context-to-prompt serialization format,
* XML-like vs JSON vs message-block representation,
* exact data/instruction delimiter strategy,
* injection-defense validation depth,
* exact output-schema representation,
* tool-schema integration,
* context-limit feedback mechanism,
* token estimator ownership,
* prompt hash construction,
* Prompt retention,
* encrypted Prompt diagnostics,
* whether prompts are reproducible without retaining plaintext,
* Template migration,
* Prompt evaluation framework,
* A/B testing,
* template approval workflow,
* future plugin-contributed Prompt components.

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
* `MODELS.md`
* `ROUTING.md`
* `STREAMING.md`
* `RETRY.md`
* `FALLBACK.md`
* `COST_CONTROL.md`
* `CACHE.md`
* `SAFETY.md`
* `OBSERVABILITY.md`

Domain:

* `../domain/LANGUAGE.md`
* `../domain/GLOSSARY.md`
* `../domain/CHARACTER.md`
* `../domain/PROFILE.md`
* `../domain/SESSION.md`
* `../domain/TRANSLATION.md`
* `../domain/WORKSPACE.md`

Modules:

* `../../02-modules/translation/`
* `../../02-modules/provider-management/`
* `../../02-modules/preferences/`

Runtime:

* `../runtime/PIPELINE_RUNTIME.md`
* `../runtime/RUNTIME_CONFIG.md`
* `../runtime/RUNTIME_OBSERVABILITY.md`
