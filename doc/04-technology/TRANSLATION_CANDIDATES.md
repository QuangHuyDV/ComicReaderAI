# CRAI Translation Candidates

Status: Candidate Evaluation
Version: 0.1.0
Updated: 2026-08-14
Path: 04-technology/TRANSLATION_CANDIDATES.md
Depends On:
- 04-technology/TECH_STACK.md
- 04-technology/OCR_CANDIDATES.md

## 1. Purpose

Tài liệu này xác định candidate set, benchmark methodology và decision gates cho Translation technology của CRAI.

Tài liệu này không chọn Translation provider winner.

Nguyên tắc bắt buộc:

```text
Translation Provider
    → selected by Chinese → Vietnamese quality evidence

Model
    → versioned candidate, not permanent identity

Provider integration
    → replaceable behind CRAI contracts

Cost and latency
    → evaluated after minimum quality threshold
```

Không chọn provider chỉ dựa trên reputation, model leaderboard hoặc preference.

## 2. CRAI Translation Scope

CRAI không chỉ dịch từng câu độc lập.

Translation capability phải hỗ trợ các requirement đã được kiến trúc hóa:

- segment translation;
- batch translation;
- context-aware translation;
- glossary injection;
- terminology consistency;
- character/name consistency;
- formatting preservation;
- partial/streaming result khi provider phù hợp;
- cancellation;
- provider replacement;
- Translation Memory integration.

Canonical:

```text
Processed Text
    ↓
Translation Request
    ↓
Translation Routing
    ↓
ExecutionBinding
    ↓
Provider Adapter
    ↓
Provider / Model
    ↓
Translation Result
```

Provider không sở hữu Translation business semantics.

## 3. Initial Language Scope

Highest priority:

```text
Simplified Chinese
    → Vietnamese

Traditional Chinese
    → Vietnamese
```

Secondary:

```text
English
    → Vietnamese
```

Simplified và Traditional Chinese phải được benchmark riêng.

Không suy ra chất lượng Traditional Chinese từ Simplified Chinese.

## 4. Initial Content Scope

Benchmark phải đại diện cho CRAI reading workloads:

```text
Chinese Web Novel
Chinese Novel
Manhua Dialogue
Narration
Mixed Dialogue + Narration
Short OCR Fragments
Long Context
```

Cần cover:

- formal speech;
- informal speech;
- historical/fantasy terms;
- cultivation/xianxia terms;
- names;
- titles;
- pronouns;
- kinship/address terms;
- idioms;
- slang;
- ellipsis;
- fragmented comic dialogue;
- chapter-to-chapter terminology.

Generic business/document translation benchmark không đủ.

## 5. Candidate Classes

Initial candidate classes:

```text
A. Dedicated Machine Translation API

B. General-purpose Cloud LLM

C. Translation-specialized / adaptive cloud model

D. Local LLM

E. Hybrid Routing
```

First benchmark round không cần test mọi provider trên thị trường.

Mục tiêu là chọn representative candidates đủ khác nhau về:

- quality;
- context handling;
- glossary control;
- latency;
- cost;
- privacy;
- deployment.

## 6. Candidate A - Google Cloud Translation

Status:

```text
Primary Dedicated-MT Candidate
```

Google Cloud Translation hiện hỗ trợ:

- Simplified Chinese;
- Traditional Chinese;
- Vietnamese;
- English.

Google Translation LLM cũng hỗ trợ các language variants như `zh-Hans`, `zh-Hant`, `zh-CN`, `zh-TW` và `vi`.

Do đó Google là candidate hợp lệ cho CRAI benchmark.

Supported-language declaration không chứng minh Chinese → Vietnamese quality cho fiction.

## 7. Google Strength Hypotheses

Cần kiểm chứng:

- predictable translation API;
- broad language coverage;
- low integration complexity;
- dedicated translation behavior;
- glossary/adaptive capabilities tùy API/model;
- potentially lower prompt-engineering complexity than general LLM;
- suitable baseline latency.

Đây là hypotheses.

Không ghi thành CRAI result trước benchmark.

## 8. Google Risks

Cần đo:

- natural Vietnamese prose;
- fiction dialogue quality;
- pronoun/address handling;
- names;
- idioms;
- xianxia/fantasy terminology;
- context continuity;
- glossary adherence;
- fragmented OCR input;
- API/model differences;
- cost;
- quotas;
- privacy/data handling.

Dedicated MT có thể mạnh ở sentence fidelity nhưng yếu hơn contextual literary adaptation.

Đây phải là measured question.

## 9. Candidate B - DeepL API

Status:

```text
Benchmark Candidate Subject to Language/API Verification
```

DeepL thường được xem như dedicated translation provider candidate.

Tuy nhiên CRAI không được giả định:

```text
DeepL supports required Chinese → Vietnamese path
```

chỉ từ reputation.

Trước benchmark phải verify current official API language support, exact source/target codes, glossary behavior và account/API availability.

Nếu required Chinese → Vietnamese path không được official API hỗ trợ ở thời điểm benchmark:

```text
DeepL
    → remove from primary candidate matrix
```

Không dùng unofficial translation path.

## 10. Candidate C - OpenAI Models

Status:

```text
Primary General-Purpose LLM Candidate
```

OpenAI model candidate phải được pin bằng exact API model/version tại benchmark time.

Không benchmark một abstract label:

```text
GPT
```

Mà phải benchmark:

```text
Provider
+
Exact Model
+
Prompt Template Version
+
Generation Settings
```

Potential strengths cần đo:

- contextual Chinese → Vietnamese;
- literary Vietnamese;
- dialogue;
- glossary instruction;
- character consistency;
- structured output;
- larger context;
- repair of fragmented OCR input when explicitly allowed.

## 11. OpenAI Risks

Cần đo:

- translation faithfulness;
- hallucination/addition;
- omission;
- over-localization;
- style drift;
- prompt sensitivity;
- model-version behavior;
- latency;
- output-token cost;
- long-context cost;
- rate limits;
- structured-output overhead;
- streaming behavior.

LLM fluency không được nhầm với translation accuracy.

## 12. Candidate D - Gemini Models

Status:

```text
Primary General-Purpose LLM Candidate
```

Gemini candidate cũng phải pin exact model/version và prompt.

Potential strengths cần đo:

- Chinese understanding;
- Vietnamese generation;
- long-context handling;
- glossary/instruction adherence;
- batch context;
- latency/cost options.

Không dùng generic brand name `Gemini` làm benchmark identity.

## 13. Gemini Risks

Cần đo cùng error taxonomy với OpenAI:

- additions;
- omissions;
- semantic drift;
- pronoun drift;
- terminology inconsistency;
- style rewriting;
- formatting drift;
- refusal/safety interference on normal fiction;
- latency variance;
- cost;
- model lifecycle/version changes.

Provider-specific strengths không được assumed trước benchmark.

## 14. Translation-Specialized LLM / Adaptive Models

Status:

```text
Optional Candidate
```

Có thể benchmark nếu provider cung cấp translation-specialized model hoặc adaptive translation capability phù hợp.

Ví dụ class:

```text
Translation LLM
Adaptive Translation
Custom Translation
```

Nhưng CRAI không cần train custom model ở MVP nếu prompt/glossary/provider baseline đã đạt quality threshold.

Custom training chỉ justify khi measurable quality gap tồn tại.

## 15. Local LLM

Status:

```text
Secondary Candidate
```

Local LLM là desirable capability nhưng không bắt buộc MVP phải bundle.

Potential benefits:

- privacy;
- offline;
- no per-request API cost;
- controllable model lifecycle.

Costs:

- model size;
- RAM/VRAM;
- first-run download;
- inference latency;
- hardware variance;
- packaging/update complexity;
- lower quality depending on model;
- operational complexity.

## 16. Local LLM Decision Rule

Local candidate chỉ trở thành MVP baseline nếu:

```text
Chinese → Vietnamese quality
    ≥ minimum threshold

and

interactive latency
    acceptable

and

minimum hardware
    realistic

and

deployment complexity
    justified
```

Không chọn local chỉ vì "free per request".

Hardware và electricity/resource cost vẫn là real cost.

## 17. Local Runtime

Exact local runtime chưa khóa.

Possible candidates later:

- ONNX-compatible runtime;
- llama.cpp-compatible runtime;
- other maintained local inference runtime.

Không chọn Ollama làm embedded production runtime chỉ vì convenient development UX.

Development harness và shipping runtime là hai decisions khác nhau.

## 18. Hybrid Routing

Status:

```text
Deferred Until Evidence
```

Potential:

```text
Fast/Cheap Provider
    ↓
difficult segment
    ↓
Higher-quality LLM
```

hoặc:

```text
Local Provider
    ↓
optional Cloud Escalation
```

Hybrid routing chỉ thêm nếu benchmark cho thấy rõ:

- quality benefit;
- cost benefit;
- privacy-compatible behavior.

Không thêm multi-provider routing complexity trước evidence.

## 19. Translation Benchmark Philosophy

Benchmark phải trả lời:

```text
Which configuration produces the best
Chinese reading experience for CRAI
within acceptable latency/cost/privacy?
```

Không trả lời đơn giản:

```text
Which model sounds most fluent?
```

Translation quality cần balance:

```text
Faithfulness
+
Natural Vietnamese
+
Context
+
Consistency
+
Terminology
```

## 20. Benchmark Configuration Identity

Mỗi configuration phải có stable ID.

Conceptually:

```text
tr-google-standard-01
tr-google-llm-01
tr-openai-modelX-context-01
tr-gemini-modelY-context-01
tr-local-modelZ-01
```

Metadata phải pin:

- provider;
- model;
- model version/date when available;
- endpoint/API mode;
- prompt template;
- glossary version;
- generation settings;
- context strategy.

Không dùng identity `best-translator`.

## 21. Dataset Structure

Recommended:

```text
benchmarks/translation/
├── dataset/
│   ├── zh-hans/
│   │   ├── novel/
│   │   ├── dialogue/
│   │   ├── manhua/
│   │   └── terminology/
│   │
│   ├── zh-hant/
│   ├── en/
│   └── ocr-noisy/
│
├── references/
├── glossary/
├── prompts/
├── configs/
└── results/
```

Actual repository path được quyết định trong implementation planning.

## 22. Dataset Provenance

Benchmark corpus phải có provenance rõ.

Ưu tiên:

- project-created samples;
- licensed/public text;
- synthetic examples;
- private evaluation corpus with lawful access.

Không commit copyrighted novel/manhua corpus công khai nếu không có quyền.

## 23. Clean Text Benchmark

Round đầu phải dùng clean source text.

Mục tiêu:

```text
Measure Translation
without OCR noise.
```

Không dùng OCR error để che hoặc làm sai provider comparison.

## 24. OCR-Noisy Benchmark

Sau clean-text round, phải có separate dataset chứa realistic OCR errors.

Mục tiêu đo:

- robustness;
- fragment recovery;
- punctuation loss;
- spacing errors;
- character substitution;
- broken lines.

Result phải ghi riêng.

Không merge clean và noisy scores thành một metric duy nhất.

## 25. Simplified Chinese Benchmark

Highest priority.

Phải cover:

- narration;
- dialogue;
- short sentence;
- long paragraph;
- idioms;
- names;
- titles;
- pronouns;
- modern slang;
- fantasy/xianxia terminology;
- ambiguous subjects.

## 26. Traditional Chinese Benchmark

Dataset riêng phải cover:

- Traditional-only characters;
- Taiwan/Hong Kong lexical variants khi relevant;
- dialogue;
- narration;
- names;
- idioms;
- comic fragments.

Provider phải được gọi với correct language identity khi API hỗ trợ distinction.

## 27. English Benchmark

English → Vietnamese là secondary.

Có thể dùng smaller set nhưng vẫn test:

- prose;
- dialogue;
- terminology;
- formatting.

English score không được bù cho weak Chinese score.

## 28. Manhua Dialogue

Manhua benchmark phải include:

- one-line bubbles;
- fragmented bubbles;
- interleaved speakers;
- sound-adjacent text if considered translatable;
- incomplete grammar;
- context from previous bubbles.

Provider phải được test cả:

```text
bubble independently
```

và:

```text
bubble batch with local context
```

khi architecture cho phép.

## 29. Novel Context

Novel benchmark phải test:

```text
Sentence only
Paragraph context
Rolling context
Chapter-local terminology context
```

Mục tiêu xác định context benefit thực tế.

Không gửi full chapter mặc định nếu smaller context đạt same quality.

## 30. Context Window Strategy

Context là resource, không phải càng nhiều càng tốt.

Benchmark phải đo:

- quality gain;
- latency;
- token/input cost;
- distraction from irrelevant context;
- consistency.

Possible outcome:

```text
small rolling context
    > full chapter context
```

về cost/latency mà quality tương đương.

Decision phải dựa evidence.

## 31. Context Separation

Provider request nên phân biệt:

```text
Text To Translate

Previous Context

Glossary

Character/Terminology Notes

Output Instructions
```

Không concatenate mọi thứ thành một ambiguous blob nếu provider supports structured roles/fields.

## 32. Glossary Benchmark

Glossary handling là critical CRAI dimension.

Test ít nhất:

- exact name;
- title;
- place;
- organization;
- cultivation term;
- ambiguous word;
- protected untranslated token;
- phrase mapping.

Metrics:

```text
Glossary Adherence Rate
Term Consistency
Incorrect Forced Replacement
```

## 33. Provider-Native Glossary vs Prompt Glossary

Dedicated MT có thể có native glossary feature.

LLM có thể consume glossary qua structured prompt/context.

Benchmark phải compare behavior, không assume equivalence.

Canonical CRAI glossary semantics vẫn provider-neutral.

Provider adapter chịu trách nhiệm mapping sang native mechanism khi available.

## 34. Character Consistency

Long-form reading cần consistency.

Dataset phải có repeated characters với:

- names;
- pronouns;
- titles;
- relationship terms;
- changing dialogue context.

Measure:

```text
same entity
    → stable Vietnamese naming/addressing
```

trừ khi context yêu cầu thay đổi.

## 35. Pronoun and Address Terms

Chinese → Vietnamese đặc biệt nhạy với:

- 我
- 你
- 他/她
- 哥/姐
- 师兄/师姐
- 师父
- 前辈
- 陛下
- 本王
- 本座
- relationship-dependent addressing.

Benchmark phải đánh giá semantics và natural Vietnamese, không chỉ lexical match.

## 36. Names

Test:

- transliteration;
- Sino-Vietnamese rendering where desired;
- glossary-forced names;
- ambiguous names/common words;
- repeated names.

CRAI không hard-code one universal Chinese-name policy trong provider adapter.

Name policy thuộc Translation/Text configuration.

## 37. Idioms

Chinese idioms cần đánh giá:

```text
meaning preserved?
Vietnamese natural?
tone preserved?
unnecessary literal translation?
unnecessary invention?
```

Không dùng string similarity metric một mình.

## 38. Genre Terminology

Dataset nên chia genre:

- modern;
- romance;
- historical;
- wuxia;
- xianxia/xuanhuan;
- game/system;
- science fiction.

Không cần equal weight.

Weights phải phản ánh intended CRAI use.

## 39. Formatting Preservation

Translation result phải preserve structure khi required:

- paragraph boundary;
- segment identity;
- line/bubble identity;
- placeholders;
- protected tokens;
- simple markup if supported.

Provider không được tự merge/split segment khiến geometry mapping mất identity mà adapter không thể recover.

## 40. Structured Output

LLM candidate nên dùng structured output hoặc equivalent constrained response khi practical.

Goal:

```text
Segment ID
    → Translation
```

Không dựa vào parsing free-form prose nếu provider có reliable structured mechanism.

Exact schema được quyết định trong implementation.

## 41. Streaming

Streaming là optional capability.

Benchmark phải xác định:

- time to first useful text;
- total latency;
- partial-result stability;
- UI usefulness;
- cancellation.

Không chọn streaming chỉ vì provider hỗ trợ.

Nếu Side Panel chỉ update khi complete segment, streaming có thể không đáng complexity.

## 42. Batch Translation

Batching phải benchmark.

Potential benefit:

- context;
- fewer network round trips;
- lower overhead;
- terminology consistency.

Potential cost:

- higher latency for first segment;
- larger failure unit;
- higher retry cost;
- harder partial cancellation.

Batch size phải là policy, không hard-code theo provider limit.

## 43. Translation Memory Interaction

Translation Memory phải được đánh giá trước provider call khi architecture/policy cho phép.

Conceptually:

```text
Normalized Translation Request
    ↓
Translation Memory Lookup
    ↓ hit
Reuse

or miss
    ↓
Provider
```

Provider benchmark phải có raw-provider round trước để không bị TM cache làm sai result.

## 44. Quality Evaluation Dimensions

Core dimensions:

```text
Semantic Faithfulness
Vietnamese Naturalness
Context Correctness
Terminology Consistency
Glossary Adherence
Character Consistency
Style Preservation
Formatting Preservation
Hallucination / Addition
Omission
```

Không dùng BLEU hoặc automated metric làm sole decision.

## 45. Human Evaluation

Human review là mandatory cho final Chinese → Vietnamese decision.

Recommended rubric per sample:

```text
1 - Unusable
2 - Major errors
3 - Understandable but weak
4 - Good
5 - Excellent
```

Reviewer phải đánh giá các dimensions riêng thay vì chỉ overall impression.

## 46. Blind Review

Khi practical:

```text
Hide provider/model identity
from evaluator.
```

Điều này giảm bias theo brand.

Output order nên randomized.

## 47. Reference Translation

Reference translation có thể giúp nhưng không phải absolute gold.

Literary translation có nhiều valid outputs.

Reviewer phải đánh giá source meaning trực tiếp khi có năng lực Chinese.

Không reject natural correct translation chỉ vì wording khác reference.

## 48. Automated Metrics

Có thể dùng supporting metrics:

- BLEU;
- chrF;
- COMET-like evaluation if practical;
- terminology match;
- glossary adherence;
- placeholder preservation;
- structural validity.

Automated metrics là supporting evidence.

Human evaluation vẫn quyết định literary usability.

## 49. LLM-as-Judge

LLM-as-judge có thể dùng để triage hoặc secondary scoring.

Không được là sole final evaluator.

Nếu dùng:

- judge model/version phải pin;
- rubric phải fixed;
- provider identity hidden;
- subset phải human-audited.

Không để candidate tự chấm output của chính nó mà không kiểm soát bias.

## 50. Hallucination

LLM candidates phải có explicit hallucination metric.

Examples:

- added event;
- invented relationship;
- invented explanation;
- changed speaker intent;
- added title/honorific;
- omitted ambiguity by inventing detail.

Fluent hallucination là critical failure.

## 51. Omission

Measure:

- dropped clause;
- dropped negation;
- dropped number;
- dropped name;
- dropped bubble;
- skipped repeated phrase when semantically required.

Omission rate là critical quality metric.

## 52. Over-Translation

Provider không được biến Translation thành rewrite/summarization.

Test:

- source ambiguity retained appropriately;
- no added exposition;
- no unnecessary censorship/euphemism;
- no genre rewriting unless configured.

## 53. Safety Behavior

Fiction có thể chứa:

- violence;
- horror;
- mature themes;
- conflict.

Benchmark phải test normal lawful fictional passages để detect unnecessary refusal or sanitization.

CRAI không bypass provider safety systems.

Nếu provider frequently refuses ordinary supported reading content, đó là product suitability issue.

## 54. Latency Metrics

Measure:

```text
DNS/connect overhead where relevant
Time to First Token / First Result
Total Latency
Batch Latency
Retry Latency
```

Report:

- median;
- p95;
- failure rate;
- timeout rate.

## 55. Cost Metrics

Cost phải normalize theo CRAI workload.

Không chỉ ghi vendor price table.

Calculate conceptually:

```text
Cost per 1,000 Chinese characters

Cost per typical novel chapter

Cost per 100 manga bubbles

Cost per reading hour
```

LLM token accounting và dedicated MT character accounting phải normalize về comparable usage scenarios.

## 56. Context Cost

Context-heavy LLM requests có thể resend previous text repeatedly.

Benchmark phải tính:

```text
Source Segment
+
Repeated Context
+
Glossary
+
Instructions
```

Không chỉ tính characters translated.

## 57. Caching Cost

Provider-side context caching nếu available có thể được benchmark sau baseline.

Không thiết kế architecture phụ thuộc provider-specific caching.

CRAI Translation Memory và Runtime Cache vẫn là own concerns.

## 58. Rate Limits

Candidate phải record:

- request limits;
- token/character limits;
- concurrency limits;
- quota behavior;
- retry guidance.

Exact values có thể thay đổi và phải được captured tại benchmark time.

Không hard-code provider commercial limits vào Business contracts.

## 59. Reliability

Measure:

- success rate;
- transient errors;
- timeout;
- malformed output;
- retry behavior;
- model unavailable;
- quota errors.

Provider quality cao nhưng operationally unreliable có thể không phù hợp primary path.

## 60. Retry

Retry chỉ áp dụng transient failures phù hợp.

Không retry blindly:

- invalid request;
- unsupported language;
- auth failure;
- deterministic safety refusal;
- malformed contract repeatedly.

Retry policy thuộc Runtime/provider infrastructure.

## 61. Cancellation

Translation provider adapter phải support cooperative cancellation khi HTTP/runtime cho phép.

Nếu remote request đã gửi và không thể truly cancel provider computation:

```text
Runtime cancels local interest
    ↓
stale result rejected
```

Không publish canceled/stale result chỉ vì provider trả về sau đó.

## 62. Privacy

Remote translation sends user text outside local device.

Evaluation phải record:

- provider data-use terms relevant to selected API;
- retention controls;
- regional endpoint options if relevant;
- account/project configuration;
- whether content is used for model improvement under selected commercial API mode.

Không suy ra privacy từ consumer chat product behavior.

## 63. Data Minimization

Send only required translation data.

Không gửi:

- full screenshot;
- unrelated OCR regions;
- entire browsing history;
- unnecessary prior chapters;

chỉ để tăng context.

Context must be purposeful and bounded.

## 64. Secrets

Provider API keys:

```text
Secret Manager
```

Không lưu:

```text
SQLite plaintext
config plaintext
log
benchmark result
```

Benchmark harness cũng phải tránh commit credentials.

## 65. Provider Adapter

Each provider adapter must map CRAI request into provider-specific API.

Conceptually:

```text
CraiTranslationRequest
    ↓
Provider Adapter
    ↓
Provider Request

Provider Response
    ↓
Provider Adapter
    ↓
CraiTranslationResult
```

Business code không chứa:

- HTTP endpoint;
- provider model name;
- API key;
- provider JSON schema.

## 66. Prompt Ownership

Prompt template là provider implementation/configuration artifact.

Business Translation Contract định nghĩa intent/context/glossary.

Adapter/prompt layer quyết định cách encode sang LLM.

Không để raw prompt string trở thành public Translation contract.

## 67. Prompt Versioning

LLM benchmark phải version prompt.

Example:

```text
translation-prompt-v1
translation-prompt-v2
```

Model score không reproducible nếu prompt thay đổi mà không ghi version.

## 68. Prompt Injection from Source Content

Source text là untrusted content.

LLM prompt design phải clearly delimit source content và instructions.

Source passage không được có quyền override system/application translation instruction.

Benchmark nên include adversarial-looking source text như:

```text
"ignore previous instructions..."
```

như literal content cần dịch.

## 69. Structured Input Identity

Each segment should retain stable identity through provider call.

Example conceptually:

```text
segment_001
segment_002
segment_003
```

Provider output must map back deterministically.

Không rely solely on output order nếu provider can restructure content.

## 70. Temperature / Generation Variance

Translation benchmark phải use deterministic/low-variance settings khi provider cho phép.

Repeated-run test cần đo output stability.

Creative variance không phải default goal.

## 71. Model Version Drift

Cloud model aliases có thể thay đổi.

Benchmark metadata phải capture exact model/version/date information available.

Nếu provider only exposes moving alias:

```text
Moving Alias
    → operational risk noted
```

Quality regression testing phải có trước model upgrade.

## 72. Provider Upgrade

Không auto-upgrade model trong production chỉ vì provider releases newer model.

Upgrade process:

```text
New Model
    ↓
Regression Benchmark
    ↓
Quality/Cost/Latency Comparison
    ↓
Explicit Provider Config Update
```

## 73. Fallback Provider

Fallback không mặc định required.

Chỉ thêm nếu:

- outage resilience đáng giá;
- secondary provider quality đủ;
- glossary/context semantics map được;
- cost/complexity acceptable.

Fallback output inconsistency phải được tính.

## 74. Local Fallback

Local model có thể là future offline fallback.

Không bundle multi-GB model chỉ để cover rare outage nếu user value không justify.

## 75. Translation Modes

Benchmark nên xem ít nhất:

```text
Fast Mode
Quality Mode
```

nhưng chỉ nếu evidence cho thấy hai configurations có useful trade-off.

Không tạo mode giả nếu một provider/config dominates.

## 76. Novel vs Comic Routing

Có thể xảy ra:

```text
Provider A
    better novel context

Provider B
    better short comic dialogue
```

Architecture cho phép routing theo requirement.

Nhưng MVP nên ưu tiên một primary provider nếu quality đủ để giảm complexity.

Multi-provider content routing chỉ khóa sau evidence.

## 77. Translation Quality Threshold

Exact numeric threshold phải được đặt trước final scoring.

Decision rule:

```text
Fails semantic minimum
    → reject

Passes semantic minimum
    ↓
compare naturalness/context/consistency

Comparable quality
    ↓
compare latency/cost/privacy/reliability
```

Không để low cost bù cho critical semantic errors.

## 78. Weighted Decision Matrix

Recommended groups:

| Group | Priority |
| --- | --- |
| Semantic Faithfulness | Critical |
| Chinese → Vietnamese Naturalness | Critical |
| Context Correctness | Critical |
| Hallucination/Omission | Critical |
| Glossary Adherence | High |
| Character/Terminology Consistency | High |
| Traditional Chinese Quality | High |
| Formatting/Segment Preservation | High |
| Reliability | High |
| Interactive Latency | High |
| Cost | High |
| Privacy | High |
| Integration Complexity | Medium |
| Streaming | Medium/Optional |
| Offline Capability | Medium |

Exact weights phải khóa trước scoring.

## 79. Suggested First Benchmark Matrix

First round nên giữ nhỏ.

```text
Google Cloud Translation
    dedicated translation baseline

Google Translation LLM / adaptive-capable path
    if practical

OpenAI
    one quality-oriented current API model
    one cost/latency-oriented model only if useful

Gemini
    one quality-oriented current API model
    one cost/latency-oriented model only if useful

DeepL
    only if current official Chinese → Vietnamese API path is verified

Local LLM
    one candidate only if hardware/runtime setup is ready
```

Không benchmark 20 models trong first round.

## 80. Benchmark Phases

### Phase A - Eligibility

Verify:

- language pair;
- API availability;
- legal/commercial use;
- basic integration;
- output structure;
- privacy constraints.

### Phase B - Clean Quality

Run curated Chinese → Vietnamese dataset.

Reject weak candidates.

### Phase C - Context and Glossary

Test:

- rolling context;
- names;
- terminology;
- glossary;
- character consistency.

### Phase D - OCR-Noisy Input

Test robustness to realistic OCR output.

### Phase E - Operational

Measure:

- latency;
- reliability;
- rate limits;
- cost;
- cancellation.

### Phase F - End-to-End Reading

Run:

```text
Capture
    ↓
OCR
    ↓
Text Processing
    ↓
Translation
    ↓
Side Panel
```

with surviving candidate(s).

## 81. Benchmark Hardware and Network

Remote provider result must record:

- client machine;
- Windows version;
- network type;
- test region;
- provider region/endpoint when configurable;
- timestamp/date;
- concurrency.

Latency comparison without network context is incomplete.

## 82. Benchmark Reproducibility

Each run pins:

```text
Dataset Version
Provider
Model
API Mode
Prompt Version
Glossary Version
Context Strategy
Generation Settings
Date
```

Provider commercial pricing/quota snapshot should also be recorded separately because it can change.

## 83. Result Statistics

Quality:

- per-category score;
- critical error count;
- hallucination count;
- omission count;
- glossary adherence;
- consistency.

Operational:

- median latency;
- p95 latency;
- failure rate;
- cost normalization.

Do not collapse everything into one score before reviewing category failures.

## 84. Gate 4 Output

Gate 4 must decide:

```text
Initial Translation Provider
Initial Model/API Mode
Prompt Strategy
Context Strategy
Glossary Mapping Strategy
Batch Strategy
Streaming Strategy
Fallback Strategy if any
Known Unsupported/Weak Cases
```

If local model is not selected:

```text
Local Translation
    → Deferred
```

not failed architecture.

## 85. Possible Outcomes

Outcome A:

```text
Dedicated MT passes quality threshold
and is much cheaper/faster

→ dedicated MT primary
```

Outcome B:

```text
LLM materially improves fiction/context quality

→ LLM primary
```

Outcome C:

```text
Dedicated MT for simple text
+
LLM for contextual/difficult text
produces meaningful benefit

→ hybrid considered
```

Outcome D:

```text
Local model reaches acceptable quality
on realistic hardware

→ local option considered
```

These are possible outcomes only.

## 86. Relationship to OCR

Translation benchmark has two stages:

```text
Clean Text
    → isolate Translation quality

OCR-Noisy Text
    → measure integration robustness
```

Do not choose Translation provider solely because it silently rewrites severe OCR mistakes.

OCR quality remains OCR responsibility.

## 87. Relationship to Text Processing

Text Processing owns normalization before Translation according to architecture.

Provider adapter must not secretly duplicate business normalization rules.

Provider-specific safe formatting/escaping is allowed.

Semantic text transformation belongs upstream.

## 88. Relationship to Glossary

Glossary semantics remain provider-independent.

```text
CRAI Glossary
    ↓
Translation Requirement
    ↓
Provider Adapter
    ↓
Native Glossary or Prompt Representation
```

Provider-native glossary object is not CRAI canonical glossary model.

## 89. Relationship to Translation Memory

Translation Memory is independent of provider.

TM key compatibility may include:

- source identity;
- language pair;
- glossary/profile version;
- translation policy/model compatibility where required.

Exact semantics belong authoritative Translation/Storage architecture.

Provider change must not silently reuse incompatible cached translation.

## 90. Relationship to Runtime

Runtime owns:

- execution authority;
- scheduling;
- cancellation;
- stale-result rejection;
- accepted publication.

Translation provider owns:

- provider invocation;
- provider-specific request;
- streaming parsing;
- provider error mapping.

Provider response does not become accepted result until Runtime rules permit.

## 91. Relationship to Persistence

Persistence may store:

- provider non-secret configuration;
- Translation Memory;
- glossary;
- selected model/profile metadata.

Secrets stay outside plaintext SQLite.

Full translated content is not persisted automatically merely because provider returned it.

## 92. Relationship to Packaging

Remote Translation has relatively low packaging impact.

Local Translation can materially affect:

- installer size;
- model distribution;
- runtime libraries;
- hardware requirements;
- update mechanism.

Therefore local model packaging is a separate later decision if local Translation is selected.

OCR runtime remains the primary blocker for initial Packaging decision.

## 93. Relationship to Plugin System

Translation providers remain replaceable implementations.

MVP benchmark does not require third-party dynamic plugin marketplace.

Provider discovery/loading must follow Plugin Architecture if implemented dynamically later.

## 94. Provider Documentation Is Time-Sensitive

The following must be rechecked at benchmark execution time:

- model availability;
- language support;
- pricing;
- quotas;
- API versions;
- glossary capabilities;
- data-use/privacy terms;
- deprecation schedules.

Do not copy these commercial/runtime details permanently into Business Architecture.

## 95. Evidence Rules

Allowed final decision evidence:

- CRAI human evaluation;
- reproducible benchmark;
- official API documentation;
- measured latency;
- measured cost;
- privacy/legal review;
- integration prototype.

Supporting only:

- public leaderboard;
- vendor marketing;
- Reddit/community preference;
- generic translation reviews.

Supporting evidence cannot select winner alone.

## 96. Current Candidate Summary

```text
Google Cloud Translation
    → primary dedicated-MT candidate
    → Chinese/Vietnamese language support verified
    → fiction quality must be measured

DeepL
    → candidate only after exact language-pair/API verification
    → do not assume support/quality

OpenAI
    → primary general LLM candidate
    → context/glossary/naturalness hypothesis
    → hallucination/cost must be measured

Gemini
    → primary general LLM candidate
    → context/cost variants worth testing
    → quality must be measured

Translation-specialized/adaptive cloud model
    → optional candidate

Local LLM
    → secondary candidate
    → privacy/offline advantage
    → hardware/quality/package cost

Hybrid Routing
    → deferred until benchmark evidence
```

## 97. Decisions Locked by This Document

Locked:

```text
Translation winner
    → not selected before Gate 4

Primary quality direction
    → Chinese → Vietnamese

Simplified Chinese
    → benchmarked independently

Traditional Chinese
    → benchmarked independently

Human evaluation
    → mandatory

Clean-text benchmark
    → mandatory

OCR-noisy benchmark
    → separate mandatory phase

Glossary/context
    → critical evaluation dimensions

LLM hallucination/omission
    → critical failure dimensions

Provider/model/prompt
    → versioned benchmark identity

Remote-only architecture
    → not required

Local LLM in MVP
    → optional, not assumed

Hybrid routing
    → evidence-gated
```

## 98. Decisions Still Open

1. exact Google API/model configuration;
2. DeepL eligibility at benchmark time;
3. exact OpenAI model candidates;
4. exact Gemini model candidates;
5. whether Translation LLM/adaptive path adds value;
6. local model candidate;
7. prompt template;
8. context size/strategy;
9. glossary mapping;
10. batch size;
11. streaming;
12. quality threshold;
13. scoring weights;
14. primary provider;
15. fallback provider;
16. novel/comic routing;
17. fast/quality modes;
18. local runtime;
19. provider upgrade policy details;
20. exact cost ceiling.

## 99. Next Technology Work

After candidate definitions:

```text
04-technology/FEASIBILITY_RESULTS.md
```

should become the evidence ledger for:

```text
Gate 1 - Desktop
Gate 2 - Capture
Gate 3 - OCR
Gate 4 - Translation
Gate 5 - End-to-End
Gate 6 - Overlay
Gate 7 - Packaging
```

Before actual results exist, `FEASIBILITY_RESULTS.md` must contain test plans/status rather than invented conclusions.

After enough OCR/runtime evidence exists:

```text
04-technology/BUILD_AND_PACKAGING.md
```

can be finalized.

## 100. Final Principle

CRAI Translation selection must preserve:

```text
Chinese quality before provider preference.

Faithfulness before fluency alone.

Context is measured, not maximized blindly.

Glossary semantics stay provider-neutral.

LLM fluency does not excuse hallucination.

Cost does not excuse semantic failure.

Provider APIs remain replaceable.

Benchmark evidence selects the initial provider.
```

The goal is not to predict which company has the best translation model.

The goal is to identify the configuration that gives CRAI users the best sustainable Chinese-to-Vietnamese reading experience under real CRAI workloads.
