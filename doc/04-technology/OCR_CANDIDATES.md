# CRAI OCR Candidates

Status: Candidate Evaluation
Version: 0.1.0
Updated: 2026-08-14
Path: 04-technology/OCR_CANDIDATES.md
Depends On:
- 04-technology/TECH_STACK.md
- 04-technology/WINDOWS_PLATFORM.md

## 1. Purpose

Tài liệu này xác định candidate set, evaluation method và decision gates cho OCR/Recognition technology của CRAI.

Tài liệu này không chọn OCR winner.

Nguyên tắc bắt buộc:

```text
OCR Engine
    → selected by benchmark evidence

OCR Runtime
    → selected after engine/model compatibility is known

Packaging
    → selected after OCR runtime is known
```

Không được biến popularity, benchmark của vendor hoặc preference thành CRAI decision.

## 2. CRAI OCR Scope

CRAI không chỉ cần plain OCR.

Recognition architecture phải có khả năng hỗ trợ:

```text
Source Image
    ↓
Preprocessing
    ↓
Text Detection
    ↓
Text Recognition
    ↓
Direction / Orientation
    ↓
Layout / Geometry
    ↓
Postprocessing
    ↓
OCR Document
    ↓
Reading Order
```

Một engine có thể cung cấp toàn pipeline hoặc chỉ một phần.

Architecture phải cho phép:

```text
Combined OCR Provider
```

và:

```text
Composed OCR Provider
    Detection Provider
    +
    Recognition Provider
    +
    Direction/Layout components
```

Technology benchmark phải đánh giá output theo contract CRAI, không chỉ raw recognized string.

## 3. Initial Language Scope

Initial source languages:

```text
Simplified Chinese
Traditional Chinese
English
```

Initial target Translation language:

```text
Vietnamese
```

OCR priority cao nhất là Chinese reading content.

Vietnamese OCR không phải primary MVP requirement.

## 4. Initial Content Scope

Benchmark dataset phải đại diện cho use case thật:

```text
Chinese Novel
Chinese Web Novel
Chinese Manhua
Comic Speech Bubbles
UI / Web Text
Mixed Chinese + English
```

Cần cover:

- horizontal text
- vertical text
- rotated text
- small text
- anti-aliased browser text
- low contrast
- colored background
- illustration background
- speech bubbles
- stylized fonts
- punctuation
- names
- numbers
- mixed Latin characters

Document-office OCR benchmark một mình không đủ để quyết định CRAI OCR.

## 5. Candidate Classes

Initial candidate classes:

```text
A. PaddleOCR family

B. RapidOCR family

C. Direct ONNX / Windows ML deployment
   of suitable OCR models

D. Windows built-in / Windows AI OCR

E. Remote OCR Provider

F. Specialized OCR model
   only when benchmark demonstrates a gap
```

Candidate list có thể thay đổi nếu benchmark discovery tìm được engine/model tốt hơn.

## 6. Candidate A - PaddleOCR

Status:

```text
Primary Benchmark Candidate
```

PaddleOCR là candidate quan trọng vì ecosystem tập trung mạnh vào OCR pipeline và Chinese/multilingual recognition.

Candidate evaluation phải xem riêng:

```text
Detection model
Recognition model
Orientation/direction components
Pipeline behavior
Runtime backend
```

Không coi `PaddleOCR` là một model duy nhất.

Version/model family phải được pin trong benchmark report.

## 7. PaddleOCR Strength Hypotheses

Các hypothesis cần kiểm chứng:

- Chinese recognition quality tốt;
- text detection phù hợp page/screenshot;
- có nhiều model size/performance trade-off;
- có orientation/layout-related pipeline components;
- ecosystem/model availability mạnh;
- có đường deployment ngoài pure Python trong một số configuration.

Đây là hypotheses.

Không ghi chúng thành CRAI benchmark result trước khi test.

## 8. PaddleOCR Risks

Cần đo:

- Python/Paddle runtime footprint;
- startup time;
- dependency size;
- native dependency complexity;
- GPU setup complexity;
- model packaging;
- memory;
- integration với .NET;
- process isolation requirement;
- model conversion compatibility nếu chuyển ONNX;
- output differences giữa runtime backends;
- vertical/stylized comic text quality.

Nếu best-quality Paddle configuration yêu cầu Python/Paddle runtime, packaging impact phải được đưa vào final score.

## 9. PaddleOCR Runtime Paths

Potential paths:

```text
Path A
C# CRAI
    ↓ IPC
Python Worker
    ↓
PaddleOCR / PaddlePaddle
```

hoặc nếu selected models/runtime hỗ trợ:

```text
Path B
C# CRAI
    ↓
ONNX / Windows ML Adapter
    ↓
Converted/compatible OCR Models
```

hoặc:

```text
Path C
C# CRAI
    ↓ IPC / native adapter
Native OCR Runtime
```

Không chọn runtime path trước model benchmark.

## 10. Candidate B - RapidOCR

Status:

```text
Primary Benchmark Candidate
```

RapidOCR là candidate đáng benchmark vì project tập trung vào deployment OCR đa backend và cross-platform.

Current project direction hỗ trợ nhiều inference engines và mặc định Chinese/English recognition.

Nó có liên hệ trực tiếp với PaddleOCR model ecosystem.

RapidOCR phải được benchmark như một implementation/runtime candidate riêng, không được giả định output giống PaddleOCR gốc.

## 11. RapidOCR Strength Hypotheses

Cần kiểm chứng:

- deployment nhẹ hơn full Paddle stack;
- ONNX Runtime path phù hợp desktop;
- Chinese/English models đủ tốt;
- integration path có thể đơn giản hơn;
- CPU performance tốt cho screenshot OCR;
- model/runtime portability tốt;
- có khả năng phù hợp isolated worker hoặc native/.NET integration.

Không coi `ONNX` đồng nghĩa tự động nhanh hơn Paddle runtime.

Performance phải đo trên cùng hardware và cùng model class.

## 12. RapidOCR Risks

Cần đo:

- model conversion fidelity;
- preprocessing/postprocessing differences;
- recognition differences so với Paddle implementation;
- ONNX operator/runtime compatibility;
- version/model synchronization;
- model license provenance;
- GPU backend complexity;
- C# integration maturity của exact selected path;
- vertical/stylized text quality;
- packaging footprint.

Nếu RapidOCR dùng converted Paddle model, benchmark phải ghi rõ model source và conversion path.

## 13. Candidate C - Direct ONNX / Windows ML

Status:

```text
Primary Runtime Candidate
```

Đây không nhất thiết là một OCR engine riêng.

Nó là deployment strategy:

```text
Selected Detection Model
+
Selected Recognition Model
+
CRAI preprocessing/postprocessing
+
ONNX-compatible inference runtime
```

Potential benefits:

- in-process C#;
- không cần Python worker;
- explicit resource ownership;
- model session reuse;
- simpler Runtime integration;
- potentially simpler packaging;
- hardware acceleration path.

Nhưng lợi ích chỉ tồn tại nếu selected OCR models chạy đúng và giữ quality.

## 14. ONNX Runtime and Windows ML Direction

ONNX Runtime có C# API và cho phép reusable inference sessions/buffers.

Trên Windows, Windows ML là candidate cần ưu tiên xem xét cho new Windows ONNX deployment khi phù hợp.

CRAI không khóa:

```text
ONNX Runtime CPU
vs
Windows ML
vs
CUDA
vs
other execution provider
```

trước benchmark.

Runtime backend là sub-decision sau model compatibility.

## 15. Direct ONNX Risks

Cần đánh giá:

- model export correctness;
- unsupported operators;
- preprocessing parity;
- postprocessing parity;
- dynamic shapes;
- text decoder implementation;
- dictionary/version matching;
- orientation pipeline;
- model upgrades;
- GPU/CPU output consistency;
- native runtime package size.

Một model chạy được không đồng nghĩa output tương đương reference implementation.

## 16. Candidate D - Windows OCR

Windows có hai classes cần phân biệt.

### 16.1 Legacy Windows.Media.Ocr

Status:

```text
Compatibility / Fallback Candidate
```

Không coi legacy Windows OCR là primary candidate mặc định.

Cần đánh giá:

- language support thực tế trên target machines;
- Chinese quality;
- geometry;
- package identity requirements;
- deployment constraint;
- output contract;
- quality trên comic/manhua.

### 16.2 Windows AI Text Recognition

Status:

```text
Hardware-Limited Candidate
```

Windows AI Text Recognition có richer recognition output như:

- characters
- words
- lines
- polygonal boundaries
- confidence

Nhưng current Windows AI OCR path yêu cầu NPU-capable supported devices.

Vì CRAI không được mặc định yêu cầu Copilot+ class hardware cho toàn user base, candidate này không thể là universal baseline nếu hardware requirement vẫn tồn tại.

Nó có thể trở thành:

```text
Accelerated Optional Provider
```

sau benchmark.

## 17. Windows OCR Packaging Concern

Legacy Windows OCR desktop usage có package-identity constraints trong current Windows documentation.

Do đó nếu candidate này được giữ:

```text
OCR Candidate
    ↓
Package Identity Constraint
    ↓
Packaging Impact
```

Đây là một ví dụ tại sao Packaging không được khóa trước OCR decision.

## 18. Candidate E - Remote OCR

Status:

```text
Optional Benchmark Candidate
```

Remote OCR có thể dùng làm:

- quality reference;
- fallback;
- optional provider;
- difficult-image escalation.

Không mặc định dùng remote OCR cho mọi capture.

Evaluation phải tính:

- Chinese quality;
- geometry;
- latency;
- network dependency;
- privacy;
- cost;
- rate limits;
- upload size;
- provider retention policy;
- failure behavior.

Raw screenshots có thể chứa sensitive content.

Remote OCR không được bật ngầm mà bỏ qua privacy/provider-routing policy.

## 19. Candidate F - Specialized Models

Status:

```text
Deferred Unless Gap Demonstrated
```

Không thêm specialized model chỉ vì có model dành cho manga/comic.

Chỉ mở candidate này nếu primary benchmark chứng minh gap rõ như:

- vertical text failure;
- stylized bubble text failure;
- text detection trên artwork kém;
- specific Traditional Chinese weakness.

Specialized provider phải giải quyết measurable gap.

## 20. Explicitly Not Selected

### Tesseract as Primary OCR

Status:

```text
Not Primary Candidate
```

Có thể dùng làm low-cost reference nếu cần nhưng không ưu tiên benchmark production path.

Lý do:

- CRAI cần modern Chinese/comic OCR;
- detection/layout/geometry requirements rộng;
- primary candidates phù hợp hơn với current target.

### EasyOCR as Primary OCR

Status:

```text
Secondary / Deferred
```

Không cần đưa vào first benchmark round nếu primary candidates đã cover use case.

Có thể thêm nếu evidence cho thấy cần comparator khác.

### Cloud-only Architecture

Status:

```text
Not Selected
```

Architecture phải giữ local OCR path.

## 21. Benchmark Philosophy

Benchmark phải trả lời:

```text
What works best for CRAI?
```

không phải:

```text
Which OCR has the highest published score?
```

Vendor benchmark có thể dùng để shortlist.

Final decision phải dựa trên CRAI dataset.

## 22. Benchmark Unit

Không chỉ benchmark whole image.

Cần ba level:

```text
Level 1
Recognition-only crops

Level 2
Detection + Recognition page/image

Level 3
End-to-End CRAI capture → OCR Document
```

Điều này giúp phân biệt lỗi:

- detection
- recognition
- orientation
- layout
- preprocessing
- capture quality

## 23. Dataset Structure

Recommended:

```text
benchmarks/ocr/
├── dataset/
│   ├── zh-hans/
│   │   ├── novel/
│   │   ├── manhua/
│   │   └── web-ui/
│   │
│   ├── zh-hant/
│   │   ├── novel/
│   │   ├── manhua/
│   │   └── web-ui/
│   │
│   ├── en/
│   └── mixed/
│
├── ground-truth/
├── configs/
└── results/
```

Actual repository path được quyết định trong implementation planning.

## 24. Dataset Provenance

Benchmark dataset phải có provenance rõ.

Ưu tiên:

- user-created test images;
- synthetic samples;
- public/licensed datasets;
- screenshots/content mà project có quyền sử dụng cho internal testing.

Không commit copyrighted comic/novel corpus vào repository nếu không có quyền phù hợp.

Private benchmark corpus có thể nằm ngoài public repository.

## 25. Ground Truth

Ground truth phải lưu ít nhất:

- expected text;
- language/script;
- text region;
- line grouping khi relevant;
- reading-order reference khi relevant;
- orientation;
- ignore/uncertain regions.

Không đánh giá geometry nếu ground truth chỉ có plain text.

## 26. Simplified Chinese Set

Phải cover:

- common characters;
- rare characters;
- punctuation;
- names;
- dialogue;
- long paragraphs;
- small browser fonts;
- bold/colored text;
- speech bubbles;
- mixed Latin/numbers.

Đây là highest-priority dataset.

## 27. Traditional Chinese Set

Không suy ra Traditional Chinese quality từ Simplified Chinese result.

Phải có dataset riêng.

Đặc biệt test:

- Traditional-only characters;
- punctuation;
- names;
- mixed Traditional/Latin;
- manhua fonts.

## 28. English Set

English là secondary initial source language.

Dataset nhỏ hơn Chinese có thể chấp nhận nhưng vẫn phải đủ phát hiện regression.

## 29. Vertical Text

Vertical text phải là explicit benchmark dimension.

Cần phân biệt:

```text
Detection found vertical region?
Recognition decoded correctly?
Direction/orientation detected?
Reading order correct?
```

Nếu engine không support native vertical text nhưng CRAI preprocessing có thể rotate crop an toàn, composed solution vẫn có thể thắng.

## 30. Rotated Text

Test ít nhất:

```text
0°
90°
180°
270°
```

và một subset skewed/slanted nếu comic content cần.

Không bắt buộc engine tự xử lý mọi rotation nếu pipeline composition xử lý được với acceptable latency.

## 31. Comic Background

Test:

- clean bubble;
- transparent/colored bubble;
- text over artwork;
- border touching glyphs;
- low contrast;
- outlined fonts;
- decorative fonts.

Accuracy trên scanned office documents không đại diện cho nhóm này.

## 32. Browser Novel Text

Test phải include actual screen-rendered text:

- common browser rendering;
- dark mode;
- light mode;
- multiple font sizes;
- zoom;
- anti-aliasing;
- high-DPI scaling.

Đây có thể là workload dễ hơn comic nhưng rất quan trọng vì novel reading là core CRAI use case.

## 33. Preprocessing Variants

Benchmark không nên cho mỗi engine một preprocessing pipeline tùy ý rồi so raw score thiếu kiểm soát.

Cần ít nhất hai rounds:

```text
Round A
Engine default/recommended pipeline

Round B
CRAI-normalized preprocessing
where technically applicable
```

Mục tiêu là biết:

- engine quality out of box;
- engine quality khi integrated đúng CRAI architecture.

## 34. Detection Metrics

Candidate detection metrics:

- region precision;
- region recall;
- IoU;
- missed text regions;
- false text regions;
- fragmented regions;
- merged unrelated regions.

Metric final có thể dùng standard detector metrics nhưng phải bổ sung CRAI error categories.

## 35. Recognition Metrics

Core metrics:

```text
CER
Character Accuracy
Exact Line Match
```

Chinese OCR nên ưu tiên character-level error analysis.

WER có thể bổ sung nhưng không nên là metric duy nhất cho Chinese.

## 36. End-to-End Metrics

End-to-End OCR metrics phải include:

- final text accuracy;
- missing region rate;
- duplicate region rate;
- geometry usability;
- reading-order usability;
- latency;
- memory;
- initialization;
- package/runtime footprint.

Winner không được chọn chỉ theo CER.

## 37. Geometry Quality

CRAI cần geometry cho Overlay/Image flow.

Đánh giá:

- bounding box/polygon correctness;
- line grouping;
- coordinate stability;
- transform back to source image;
- crop offset preservation.

Một OCR có text accuracy cao nhưng geometry unusable có thể không phù hợp Overlay.

## 38. Reading Order

Reading Order là architecture concern riêng nhưng OCR output phải cung cấp đủ evidence.

Benchmark cần ghi:

```text
Does provider output enough geometry/layout data
for CRAI Reading Order resolution?
```

Không bắt buộc OCR engine tự quyết định canonical reading order.

## 39. Confidence

Nếu provider cung cấp confidence:

- normalize semantics carefully;
- không assume confidence comparable giữa engines;
- benchmark calibration nếu confidence được dùng cho fallback routing.

Không dùng `0.9` từ engine A như equivalent `0.9` engine B.

## 40. Latency Metrics

Đo riêng:

```text
Cold Start
Model Load
First Inference
Warm Inference
Detection
Recognition
Whole Pipeline
```

Không trộn model initialization vào every-frame latency.

## 41. Throughput

CRAI không phải bulk document server.

Throughput vẫn đo nhưng priority thấp hơn interactive latency.

Relevant scenario:

```text
Repeated changed regions
during active reading session
```

Không tối ưu cho thousands-of-pages batch nếu làm interactive UX tệ hơn.

## 42. Memory Metrics

Đo:

- idle process memory;
- model-loaded memory;
- peak inference memory;
- repeated inference stability;
- native memory;
- GPU memory nếu applicable.

Memory leak test phải chạy nhiều OCR cycles.

## 43. CPU Evaluation

CPU path là mandatory benchmark.

Reason:

```text
CRAI cannot assume dedicated GPU/NPU.
```

CPU baseline phải usable trên target minimum hardware.

Exact minimum hardware sẽ được khóa sau feasibility evidence.

## 44. GPU Evaluation

GPU path là optional but important.

Test khi candidate hỗ trợ:

- NVIDIA CUDA path;
- Windows ML/hardware-selected path;
- other practical Windows acceleration path.

GPU acceleration chỉ được production-enable nếu deployment complexity và compatibility đáng giá.

## 45. NPU Evaluation

NPU path là optional optimization.

Windows AI OCR phải test trên compatible hardware nếu available.

Không dùng NPU result làm universal baseline.

## 46. Runtime Candidates

For each OCR engine/model, evaluate applicable runtime:

```text
In-process C#

ONNX / Windows ML

Native Library

Python Worker

Remote Provider
```

Engine winner và runtime winner có thể là hai decisions liên quan nhưng khác nhau.

## 47. In-Process Preference

All else approximately equal:

```text
Prefer simpler in-process runtime.
```

Nhưng không sacrifice material OCR quality chỉ để tránh worker.

Decision order:

```text
Quality threshold
    ↓
Architecture compatibility
    ↓
Latency/resource
    ↓
Deployment simplicity
```

## 48. Python Worker

Python worker chỉ được chọn nếu measurable benefit justify cost.

Benefits có thể gồm:

- best model support;
- reference pipeline parity;
- easier model updates;
- quality advantage.

Costs:

- runtime bundle;
- environment;
- startup;
- IPC;
- memory;
- crash handling;
- packaging;
- update complexity.

Nếu chọn:

```text
CRAI Main Process
    ↓
Versioned OCR IPC Contract
    ↓
OCR Worker
```

Business Modules không biết Python tồn tại.

## 49. Worker Input Transfer

Image transfer mechanism chưa khóa.

Candidates:

- encoded memory payload;
- raw bounded buffer;
- shared memory;
- temporary file.

Không chọn shared memory trước measurement.

Small/medium OCR regions có thể không justify IPC optimization complexity.

## 50. Worker Lifecycle

Nếu worker cần thiết, evaluate:

- lazy start;
- warm persistent worker;
- idle shutdown;
- crash restart;
- model reload;
- cancellation;
- version compatibility.

Không spawn Python process cho từng OCR region.

## 51. Model Session Lifecycle

Model/session phải reusable.

Do not:

```text
Load Model
Recognize One Crop
Dispose Model
```

cho từng WorkItem nếu model supports safe reuse.

Session lifecycle phải integrate Resource Management architecture.

## 52. ONNX Resource Lifecycle

ONNX Runtime C# objects có native/disposable resources.

Implementation phải dispose deterministically.

Reusable tensor/buffer strategy chỉ optimize sau profiling.

Không để unmanaged buffers phụ thuộc hoàn toàn vào GC timing.

## 53. Model Files

Model assets phải có:

- model ID;
- version;
- source/provenance;
- checksum;
- license metadata;
- language capability;
- runtime compatibility metadata.

Không gọi file đơn giản `model.onnx` mà mất identity/version.

## 54. Model Updates

Automatic model update chưa phải MVP baseline.

Nhưng model identity phải versioned từ đầu để future update không phá:

- cache compatibility;
- Translation/OCR diagnostics;
- benchmark reproducibility;
- rollback.

## 55. License Review

Mỗi candidate phải review riêng:

```text
Engine Code License
Model License
Dictionary/Data License
Redistribution Rights
Commercial-use Rights
Attribution Requirements
```

Không suy ra model license từ repository code license.

Ví dụ RapidOCR project code và upstream model ownership/provenance phải được ghi riêng trong final dependency review.

## 56. Offline Requirement

At least one viable local OCR path là strong CRAI requirement.

Remote provider không được là only OCR implementation.

Benchmark phải tìm local candidate vượt minimum quality threshold.

Nếu không candidate nào đạt, architecture/product scope phải review bằng evidence thay vì silently chuyển cloud-only.

## 57. Privacy

Local OCR được ưu tiên cho sensitive screenshot processing khi quality đủ.

Remote OCR requires explicit provider/privacy policy.

Không upload full screen nếu only region cần OCR.

Data minimization:

```text
Capture only required source
    ↓
Crop required region
    ↓
Send only required payload
```

khi remote path được dùng.

## 58. Cancellation

OCR provider phải có bounded cancellation behavior.

Nếu native/model inference không cancel giữa run:

- Runtime vẫn phải reject stale result;
- provider phải stop accepting unnecessary subsequent work;
- worker process không được leak;
- timeout policy phải rõ.

Cancellation capability là benchmark dimension.

## 59. Timeout

Không dùng một timeout constant cho mọi engine/hardware.

Benchmark phải thu latency distribution.

Timeout policy thuộc Runtime/Provider policy sau evidence.

## 60. Failure Mapping

Provider-native errors phải normalize.

Examples:

- model missing;
- model incompatible;
- runtime unavailable;
- GPU unavailable;
- unsupported language;
- invalid image;
- worker crash;
- inference failure;
- remote unavailable;
- rate limited.

Không expose Python traceback/ORT exception/HRESULT như Business error contract.

## 61. Fallback

Fallback routing chỉ được thêm nếu evidence cho thấy value.

Potential:

```text
Primary Local OCR
    ↓ low confidence / unsupported case
Optional Secondary OCR
```

Nhưng fallback làm tăng:

- latency;
- complexity;
- packaging;
- model memory;
- consistency issues.

Không giữ nhiều OCR engines production chỉ vì benchmark đã test chúng.

## 62. Combined vs Composed OCR

Benchmark phải cho phép outcome:

```text
Best Combined Provider
```

hoặc:

```text
Best Detection
    +
Best Recognition
```

Nếu composed solution tăng quality đáng kể với acceptable complexity, architecture đã cho phép.

Không ép winner phải là một monolithic engine.

## 63. Model Tier Evaluation

Nếu candidate có mobile/small/server/medium tiers:

Test ít nhất:

```text
one lightweight tier
one quality-oriented tier
```

khi practical.

Mục tiêu tìm Pareto frontier:

```text
Quality
vs
Latency
vs
Memory
vs
Package Size
```

Không benchmark mọi model variant.

## 64. Benchmark Hardware

Mỗi result phải ghi:

- CPU;
- RAM;
- GPU;
- NPU nếu có;
- Windows version;
- runtime version;
- model version;
- execution provider;
- power mode nếu relevant.

Không so results từ hai machines mà không ghi hardware.

## 65. Benchmark Reproducibility

Mỗi run phải pin:

```text
Candidate Version
Model Version
Runtime Version
Config
Dataset Version
Hardware
```

Benchmark result không có version metadata không đủ để khóa decision.

## 66. Warm-Up

ML runtime benchmark phải có:

```text
Cold measurement
Warm-up
Repeated warm measurements
```

Không dùng first inference làm steady-state performance.

## 67. Result Statistics

Latency report nên có:

- median;
- p95;
- max/outlier notes;
- cold start;
- sample count.

Không chỉ ghi average.

## 68. Quality Threshold

Final threshold numeric chưa khóa trước dataset.

Nhưng decision logic:

```text
Fail minimum Chinese quality
    → reject regardless of speed

Pass quality threshold
    → compare latency/resources/deployment

Near-equal quality
    → prefer simpler runtime/deployment
```

Quality threshold phải được ghi trong benchmark plan trước final winner selection để tránh bias.

## 69. Weighted Decision Matrix

Final evaluation nên có các groups:

| Group | Priority |
| --- | --- |
| Chinese OCR Quality | Critical |
| Geometry / Detection | Critical |
| Interactive Latency | High |
| Reliability | High |
| Runtime Integration | High |
| Memory | High |
| Packaging | High |
| Offline Capability | High |
| Traditional Chinese | High |
| Vertical/Comic Handling | High |
| GPU/NPU Optional Acceleration | Medium |
| Cross-platform Potential | Medium |
| Remote Capability | Low/Optional |

Exact weights được khóa trước final benchmark scoring.

## 70. Suggested First Benchmark Matrix

First round:

```text
PaddleOCR
    reference recommended Chinese configuration
    lightweight configuration if available

RapidOCR
    comparable Chinese configuration
    ONNX Runtime CPU

Direct ONNX / Windows ML
    only when compatible model path is ready

Windows legacy OCR
    compatibility baseline

Windows AI OCR
    only on supported NPU hardware
```

Remote provider có thể chạy separate quality-reference round.

## 71. Do Not Compare Mismatched Models Blindly

Nếu PaddleOCR và RapidOCR sử dụng khác:

- detection model;
- recognition model;
- preprocessing;
- dictionary;
- model tier;

result không được ghi đơn giản là:

```text
Paddle vs Rapid
```

Phải ghi exact stack.

Ví dụ conceptually:

```text
Engine
+
Detection Model
+
Recognition Model
+
Runtime
+
Config
```

là một benchmark candidate configuration.

## 72. Candidate Configuration Identity

Mỗi configuration cần stable ID.

Ví dụ:

```text
ocr-paddle-zh-quality-01
ocr-rapid-zh-onnx-cpu-01
ocr-win-legacy-zh-01
```

Tên model/version cụ thể nằm trong benchmark metadata.

Không encode winner semantics như `best-ocr`.

## 73. Benchmark Output

Expected output:

```text
04-technology/FEASIBILITY_RESULTS.md
```

hoặc supporting benchmark artifact referenced từ file đó.

Result phải chứa:

- dataset version;
- hardware;
- candidate configs;
- quality metrics;
- latency;
- memory;
- package/runtime impact;
- known failures;
- license notes;
- recommendation.

## 74. Decision Outputs

Gate 3 phải khóa ít nhất:

```text
Initial OCR Provider
Initial OCR Engine/Model Configuration
Initial OCR Runtime
Process Topology for OCR
Model Asset Strategy
Known Unsupported OCR Cases
Fallback Strategy if any
```

Sau đó mới có đủ input cho Packaging decision.

## 75. Possible Decision Outcomes

Outcome A:

```text
Rapid/ONNX configuration
    wins quality threshold
    +
    deployment simplicity

→ in-process/local OCR
```

Outcome B:

```text
Paddle reference runtime
    materially better quality

→ isolated Python/native worker justified
```

Outcome C:

```text
Different detector + recognizer
    materially better

→ composed OCR provider
```

Outcome D:

```text
Windows AI OCR
    excellent on NPU devices

→ optional accelerated provider
    not universal baseline
```

Đây chỉ là possible outcomes.

Không outcome nào đã được chọn.

## 76. Relationship to Capture

Capture technology phải output canonical image/artifact data.

OCR không phụ thuộc trực tiếp:

```text
Windows.Graphics.Capture object
DXGI surface
HWND
```

Canonical:

```text
Windows Capture Provider
    ↓
Capture Artifact
    ↓
OCR Provider Adapter
```

Specialized zero-copy optimization chỉ thêm sau profiling.

## 77. Relationship to Runtime

Runtime owns:

- execution authority;
- scheduling;
- cancellation authority;
- accepted result publication;
- stale-result rejection.

OCR Provider owns:

- model/runtime invocation;
- OCR-specific resource usage;
- native/worker error mapping;
- provider result construction.

OCR engine không tự publish accepted Runtime Artifact ngoài Runtime rules.

## 78. Relationship to Text Processing

OCR output không phải final normalized reading text.

```text
OCR
    ↓
OCR Document
    ↓
Reading Order
    ↓
Text Processing
```

Không đưa Translation-specific normalization vào OCR adapter để tăng benchmark score.

## 79. Relationship to Translation

Translation quality benchmark phải dùng OCR-realistic input ở một later end-to-end round.

Nhưng OCR engine selection không được tối ưu bằng cách dựa vào một Translation provider che lỗi OCR.

Đầu tiên phải đo OCR correctness độc lập.

## 80. Relationship to Packaging

Packaging remains blocked by OCR Runtime Decision.

Potential impact:

```text
ONNX / Windows ML
    → model + native runtime assets

Paddle/Python
    → Python + packages + models + worker

Native engine
    → native DLLs + models

Windows OCR
    → OS/package capability constraints

Remote OCR
    → minimal local model assets
```

Final installer format không được khóa trước Gate 3.

## 81. Relationship to Plugin System

OCR provider có thể trở thành replaceable/provider/plugin implementation theo Plugin Architecture.

Technology benchmark không được biến provider plugin thành architecture owner.

Plugin loading/isolation policy vẫn theo Plugin System.

MVP không cần dynamic third-party OCR plugin marketplace để benchmark providers.

## 82. First Implementation Spike

Recommended Gate 3 spike:

```text
1. Define benchmark Capture Artifact input.

2. Prepare versioned Chinese benchmark subset.

3. Implement one thin adapter per candidate configuration.

4. Run CPU benchmark first.

5. Record quality + geometry.

6. Remove candidates failing minimum quality.

7. Benchmark latency/memory of survivors.

8. Test optional acceleration.

9. Evaluate runtime/packaging impact.

10. Select initial OCR stack.
```

Không tối ưu GPU trước khi biết candidate có đạt quality threshold hay không.

## 83. Evidence Rules

Allowed decision evidence:

- CRAI benchmark;
- reproducible local measurement;
- official compatibility/runtime documentation;
- license documentation;
- measured packaging prototype.

Supporting evidence only:

- vendor benchmark;
- GitHub popularity;
- blog benchmark;
- anecdotal recommendation.

Không dùng supporting evidence một mình để khóa winner.

## 84. Current Candidate Summary

```text
PaddleOCR
    → Primary quality candidate
    → runtime cost must be measured

RapidOCR
    → Primary deployment/ONNX candidate
    → quality parity must be measured

Direct ONNX / Windows ML
    → Primary in-process runtime strategy
    → model compatibility must be proven

Windows Legacy OCR
    → Compatibility/fallback candidate
    → packaging/language/quality constraints

Windows AI OCR
    → Optional NPU candidate
    → cannot be universal baseline currently

Remote OCR
    → Optional reference/fallback
    → privacy/cost/network constraints

Specialized OCR
    → Deferred until gap demonstrated
```

## 85. Decisions Locked by This Document

Locked:

```text
OCR winner
    → not selected before benchmark

Primary benchmark candidates
    → PaddleOCR
    → RapidOCR
    → ONNX/Windows ML compatible path
    → Windows OCR

CPU benchmark
    → mandatory

Simplified Chinese
    → highest priority

Traditional Chinese
    → independently benchmarked

Geometry
    → critical criterion

Packaging impact
    → part of OCR decision

Local OCR path
    → required target

Remote-only OCR
    → not selected

Python primary app runtime
    → not selected
```

## 86. Decisions Still Open

1. exact PaddleOCR model/configuration;
2. exact RapidOCR model/configuration;
3. whether latest model family is stable enough for CRAI;
4. best detector;
5. best recognizer;
6. combined vs composed provider;
7. in-process vs worker;
8. ONNX Runtime vs Windows ML vs other backend;
9. CPU baseline model tier;
10. optional GPU backend;
11. optional NPU backend;
12. worker IPC if required;
13. image-transfer mechanism if worker exists;
14. OCR fallback policy;
15. confidence normalization;
16. final model asset layout;
17. model update strategy;
18. exact quality threshold;
19. exact decision weights;
20. specialized comic OCR need.

## 87. Next Technology Document

After candidate definition:

```text
04-technology/TRANSLATION_CANDIDATES.md
```

OCR and Translation candidate documents can be completed before actual feasibility execution.

Actual winner decisions belong after benchmark evidence in:

```text
04-technology/FEASIBILITY_RESULTS.md
```

## 88. Final Principle

CRAI OCR selection must preserve:

```text
Architecture before engine.

Chinese quality before popularity.

Geometry matters, not text alone.

CPU path must remain viable.

Acceleration is optional.

Runtime simplicity matters after quality threshold.

Packaging cost is part of OCR cost.

Benchmark evidence selects the winner.
```

The purpose of this document is not to predict the best OCR engine.

It is to make the eventual OCR decision reproducible, measurable and difficult to bias by preference.
