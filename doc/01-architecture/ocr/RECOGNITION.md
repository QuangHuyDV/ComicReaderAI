# Text Recognition

> **Status:** Draft
> **Version:** 1.1
> **Layer:** OCR Architecture
> **Depends On:** Detection
> **Next Layer:** Text Direction, Layout Analysis, OCR Postprocessing

---

# 1. Purpose

Text Recognition là giai đoạn chuyển visual text trong từng Detection Region thành dữ liệu văn bản có cấu trúc.

Nếu:

```text
Detection
    → "Text nằm ở đâu?"
```

thì Recognition trả lời:

```text
"What is the text?"
```

Recognition không chỉ tạo một chuỗi ký tự.

Nó tạo một representation có cấu trúc để các stage phía sau có thể sử dụng mà không cần truy cập lại OCR Provider.

---

# 2. Scope

Recognition chịu trách nhiệm:

* nhận Detection Result
* xử lý từng Region
* chuẩn bị Region cho recognition khi cần
* nhận dạng ký tự
* xác định source-language/script metadata
* xây dựng Character
* xây dựng Word khi phù hợp
* xây dựng Line
* xây dựng Paragraph trong phạm vi Region
* đánh giá Recognition Confidence
* tạo Recognition Result

Recognition không chịu trách nhiệm:

* Text Detection
* page-level Reading Order
* Layout Tree
* Translation
* grammar correction
* semantic rewriting
* rendering
* Runtime scheduling
* Runtime retry
* Runtime cancellation
* Event Bus semantics
* global cache lifecycle

---

# 3. Goals

Recognition hướng tới:

* Accuracy
* Consistency
* Provider Independence
* Structured Output
* Multi-language Support
* Traceability
* Replaceability
* Testability

---

# 4. Non-Goals

Recognition không thực hiện:

* Machine Translation
* semantic NLP
* text summarization
* source meaning correction
* speaker identification
* speech bubble detection
* page Reading Order
* Presentation layout

---

# 5. Architecture Position

```text
Processed Image
      │
      ▼
Detection
      │
      ▼
Detection Result
      │
      ▼
Recognition
      │
      ▼
Recognition Result
      │
      ├──► Text Direction
      ├──► Layout Analysis
      └──► OCR Postprocessing
```

Recognition không giao tiếp trực tiếp với business modules hoặc Presentation.

---

# 6. Terminology

## Recognition

Quá trình chuyển visual Region thành source-language text có cấu trúc.

---

## Recognition Result

Canonical output của Recognition.

---

## Recognized Region

Recognition output ứng với một Detection Region.

---

## Character

Đơn vị text nhỏ nhất mà Recognition contract biểu diễn.

---

## Word

Nhóm Character tạo thành một lexical unit khi language/provider/profile hỗ trợ khái niệm này.

---

## Line

Một chuỗi Character/Word thuộc cùng một visual line.

---

## Paragraph

Một nhóm Line thuộc cùng một textual unit trong phạm vi Region.

---

## Script

Writing system.

Ví dụ:

* Latin
* Han
* Hiragana
* Katakana
* Hangul
* Cyrillic
* Arabic
* Thai

---

## Language

Source language metadata được Recognition hoặc Provider suy luận.

Language không đồng nghĩa Script.

---

## Writing Mode Hint

Metadata sơ bộ về cách text được trình bày.

Authoritative direction semantics thuộc `TEXT_DIRECTION.md`.

---

## Recognition Provider

Engine/adapter thực hiện visual recognition.

Pipeline không phụ thuộc Provider cụ thể.

---

# 7. Core Input

Recognition nhận:

```text
Detection Result
+
Region Geometry
+
Region Type
+
Detection Metadata
+
Effective Recognition Profile
```

Có thể nhận thêm:

* language hint
* script hint
* writing-mode hint
* provider capability hint

---

# 8. Core Output

Recognition tạo:

```text
Recognition Result
```

bao gồm:

* Region Results
* Paragraphs
* Lines
* Words
* Characters
* Language
* Script
* Recognition Confidence
* Provider-neutral metadata

---

# 9. High-Level Recognition Flow

```text
Detection Result
      │
      ▼
1. Region Validation
      │
      ▼
2. Region Preparation
      │
      ▼
3. Recognition Context Resolution
      │
      ▼
4. Provider Invocation
      │
      ▼
5. Provider Result Normalization
      │
      ▼
6. Text Structure Construction
      │
      ▼
7. Confidence Assembly
      │
      ▼
8. Recognition Result Assembly
```

Một Provider có thể gộp nhiều bước nội bộ.

CRAI contract vẫn phải giữ cùng semantics.

---

# 10. Stage 1 — Region Validation

Recognition kiểm tra:

* Region ID tồn tại
* Geometry hợp lệ
* source image/version hợp lệ
* Region nằm trong bounds
* Region có thể xử lý
* Recognition Profile hợp lệ

Recognition không tự sửa Region geometry sai.

Geometry ownership thuộc Detection.

---

# 11. Stage 2 — Region Preparation

Recognition có thể cần chuẩn bị Region trước khi provider invocation.

Ví dụ:

* crop
* padding
* local rotation correction
* local upscale
* contrast adjustment
* format conversion

Đây là preparation cục bộ cho Recognition Region.

Global image preprocessing vẫn thuộc `PREPROCESS.md`.

---

# 12. Region Preparation Boundary

Recognition-specific preparation không được:

* thay đổi Detection Region semantics
* thay Region ID bằng identity không truy vết được
* làm mất mapping về source coordinates

Derived region image phải giữ lineage cần thiết.

---

# 13. Stage 3 — Recognition Context Resolution

Recognition Context có thể sử dụng:

* Recognition Profile
* language hint
* script hint
* Region Type
* writing-mode hint
* privacy classification
* provider capability requirements

Context resolution phải provider-neutral.

---

# 14. Stage 4 — Provider Invocation

Recognition gọi OCR Provider thông qua Provider Contract.

Conceptually:

```text
Recognition
    ↓
OCR Provider Contract
    ↓
Provider Adapter
    ↓
OCR Engine
```

Recognition không gọi trực tiếp SDK/API của provider.

---

# 15. Provider Request

Provider request có thể chứa:

* prepared region image reference
* expected language
* script hint
* region type
* recognition mode
* timeout hint
* privacy classification

Provider-specific request schema không được trở thành public OCR contract.

---

# 16. Stage 5 — Provider Result Normalization

Provider-native output phải được normalize trước khi rời adapter boundary.

Có thể normalize:

* text
* character alternatives
* word structure
* line structure
* geometry
* language
* script
* confidence
* provider metadata

CRAI downstream không phụ thuộc provider-native response.

---

# 17. Recognition Result Model

```text
Recognition Result
├── Metadata
├── Region Results[]
│   ├── Region Reference
│   ├── Paragraphs[]
│   ├── Lines[]
│   ├── Words[]
│   ├── Characters[]
│   ├── Language
│   ├── Script
│   ├── Writing Mode Hint
│   ├── Confidence
│   └── Optional Provider Metadata
└── Statistics
```

Provider Metadata phải optional và không được dùng làm dependency bắt buộc ở downstream.

---

# 18. Recognition Document Structure

Trong mỗi Region:

```text
Region
  └── Paragraph
       └── Line
            └── Word
                 └── Character
```

Không phải mọi language đều cần `Word`.

Contract phải cho phép Word layer optional hoặc provider/profile-dependent.

---

# 19. Character Model

Character có thể chứa:

* Character ID
* Unicode Value
* Geometry
* Recognition Confidence
* Script
* Rotation metadata
* Region Reference
* Metadata

Character phải giữ liên kết tới Region nguồn.

---

# 20. Character Identity

Character ID chỉ ổn định trong phạm vi Recognition Result revision tương ứng.

Nếu Recognition chạy lại và output thay đổi đáng kể, một revision mới phải được tạo.

Không silent-mutate published Character.

---

# 21. Word Model

Word có thể chứa:

* Word ID
* Text
* Character References
* Geometry
* Confidence
* Language
* Metadata

Với Chinese/Japanese hoặc các language không có clear word boundary, Word có thể:

* do Provider sinh
* do profile quyết định
* bị bỏ trống

Recognition không được ép segmentation giả tạo chỉ để luôn có Word.

---

# 22. Line Model

Line có thể chứa:

* Line ID
* Text
* Geometry
* Character/Word References
* Direction Hint
* Confidence
* Metadata

Authoritative line-direction semantics thuộc `TEXT_DIRECTION.md`.

---

# 23. Paragraph Model

Paragraph đại diện cho textual grouping trong phạm vi Region.

Có thể chứa:

* Paragraph ID
* Lines
* Text
* Geometry
* Language
* Confidence
* Metadata

Recognition không nối Paragraph semantic qua nhiều Region.

Cross-region structure thuộc các stage phía sau.

---

# 24. Recognized Text

Recognized Text phải giữ source-language content.

Recognition không tự:

* translate
* rewrite
* spell-correct
* grammar-correct
* glossary-replace
* semantic-normalize

Raw provider text và normalized recognition text có thể tách biệt nếu contract yêu cầu.

---

# 25. Language Metadata

Recognition có thể xác định:

* dominant language
* candidate languages
* mixed-language state

Language metadata là recognition signal.

Nó không được dùng một mình để quyết định page Reading Order.

---

# 26. Script Metadata

Script mô tả writing system.

Ví dụ:

```text
Han
Latin
Hiragana
Katakana
Hangul
```

Mixed Script phải được hỗ trợ.

---

# 27. Writing Mode Hint

Recognition Provider có thể trả:

* Horizontal
* Vertical
* Mixed
* Unknown

Đây chỉ là hint.

Authoritative writing-direction result thuộc `TEXT_DIRECTION.md`.

---

# 28. Recognition Confidence

Recognition Confidence phản ánh mức độ tin cậy của recognized text.

Có thể tồn tại ở:

* Character
* Word
* Line
* Paragraph
* Region
* Recognition Result

---

# 29. Confidence Semantics

Recognition Confidence không được trộn trực tiếp với:

* Detection Confidence
* Direction Confidence
* Reading Confidence

Mỗi confidence có owner riêng.

Quality Assessment có thể aggregate chúng ở bước sau.

---

# 30. Confidence Aggregation

Parent confidence không bắt buộc bằng trung bình của child confidence.

Ví dụ:

```text
Region Confidence
≠ average(Character Confidence)
```

Aggregation algorithm có thể phụ thuộc Provider/Recognition Profile.

Contract chỉ yêu cầu semantics ổn định và metadata đủ rõ.

---

# 31. Geometry Ownership

Recognition có thể nhận hoặc bổ sung Character/Word/Line geometry.

Nhưng canonical Region geometry vẫn thuộc Detection.

Recognition không được thay đổi Region bounds để sửa Detection một cách âm thầm.

Nếu phát hiện geometry mismatch:

* report diagnostics
* tạo recognition-local geometry
* giữ source relationship rõ ràng

---

# 32. Result Immutability

Published Recognition Result phải immutable.

Nếu cần:

* rerun
* manual correction
* provider fallback
* improved model

thì phải tạo:

```text
new Recognition Result revision
```

không sửa silent result cũ.

---

# 33. Manual Correction Boundary

Recognition output có thể được người dùng hoặc downstream correction workflow sửa.

Nhưng manual correction không được overwrite raw machine output.

Nên giữ:

```text
Machine Recognition
+
Correction Layer / New Revision
```

Ownership của user correction workflow thuộc module tương ứng.

---

# 34. Provider Independence

Recognition Provider có thể là:

* PaddleOCR
* EasyOCR
* Tesseract
* Google Vision
* Azure Vision
* custom model

Public Recognition Result không thay đổi theo provider.

---

# 35. Multi-Provider Compatibility

Nếu CRAI dùng nhiều Provider, mỗi output vẫn phải normalize về cùng Recognition Contract.

Ví dụ:

```text
Region A → Provider 1
Region B → Provider 2
```

Downstream không cần biết sự khác nhau của Provider API.

---

# 36. Detection Integration

Recognition sử dụng Detection-owned concepts:

* Region
* Region Geometry
* Region Type
* Detection Confidence
* Region hierarchy hints

Recognition không redefine chúng.

---

# 37. Text Direction Integration

Text Direction sử dụng:

* recognized Character/Line geometry
* language/script metadata
* provider direction hint

Recognition chỉ cung cấp dữ liệu.

Text Direction mới là owner của final direction semantics.

---

# 38. Layout Integration

Layout Analysis có thể dùng Recognition Result để hiểu:

* text block extent
* line grouping
* textual occupancy

Layout vẫn sở hữu Page/Panel/Container/Block organization.

Recognition không sở hữu Layout Tree.

---

# 39. Postprocessing Integration

OCR Postprocessing hợp nhất:

```text
Detection Result
+
Recognition Result
+
Direction Result
+
Layout Result
```

thành canonical `OCR Document`.

Recognition không tự xây OCR Document cuối cùng.

---

# 40. Quality Integration

Quality Assessment có thể dùng:

* Recognition Confidence
* empty result rate
* missing Character/Line
* suspicious language/script mismatch

Quality không thay đổi Recognition Result.

---

# 41. Runtime Integration

Recognition không sở hữu:

* Queued / Running lifecycle
* execution attempts
* Scheduler behavior
* Runtime retry
* cancellation authority
* stale-result authority

Runtime điều phối execution.

Recognition chỉ tạo semantic output/failure information.

---

# 42. Retry Integration

Recognition có thể cung cấp:

* recognition failure category
* low-confidence result
* provider availability hint
* fallback recommendation

Nhưng không tự schedule retry.

Retry ownership thuộc Runtime.

---

# 43. Cache Integration

Recognition có thể định nghĩa semantic compatibility.

Ví dụ result không còn compatible khi:

* Region changes
* image version changes
* Recognition Profile changes
* provider/model capability version changes
* relevant language/script hint changes

Global cache lifecycle thuộc Runtime.

---

# 44. Event Integration

Recognition có thể tạo domain facts như:

```text
RecognitionCompleted
RecognitionFailed
RegionRecognized
```

Meaning thuộc Recognition.

Event delivery/envelope thuộc Event Bus.

---

# 45. Error Integration

Recognition-specific semantic errors có thể gồm:

* InvalidRegion
* UnsupportedLanguage
* ProviderUnavailable
* InvalidProviderResponse
* RecognitionResultInvalid
* EmptyRecognitionResult

Provider-native errors phải được map trước khi crossing Recognition boundary.

Runtime Error Model sở hữu execution normalization.

---

# 46. Observability Integration

Recognition có thể cung cấp measurements như:

* Region recognition duration
* recognized character count
* empty result count
* confidence distribution
* language/script distribution
* provider identity
* model version

Telemetry transport thuộc Runtime/Infrastructure.

---

# 47. Privacy

Recognition có thể xử lý private text content.

Do đó:

* không log full recognized text mặc định
* provider request phải tuân thủ privacy policy
* provider metadata không được chứa secret
* local-only content không được gửi remote provider

---

# 48. Determinism

Cùng:

```text
Prepared Region semantic identity
+
Recognition Profile
+
Recognition Strategy / Model Version
```

nên tạo structurally equivalent Recognition Result, ngoại trừ provider nondeterminism được record rõ.

---

# 49. Architecture Invariants

Recognition phải luôn đảm bảo:

1. Chỉ nhận dạng source-language text.

2. Không thực hiện Translation.

3. Không sửa semantic meaning.

4. Không sở hữu Region semantics.

5. Không thay đổi Detection Geometry âm thầm.

6. Recognition Result phải provider-neutral.

7. Provider-native response không crossing public boundary.

8. Character / Word / Line / Paragraph phải giữ mapping về Region nguồn.

9. Word là optional khi language không có clear lexical boundary.

10. Recognition Confidence không đồng nghĩa Detection Confidence.

11. Writing Mode từ Provider chỉ là hint cho Text Direction.

12. Published Recognition Result phải immutable.

13. Rerun tạo revision mới.

14. Recognition không sở hữu Reading Order.

15. Recognition không sở hữu Layout Tree.

16. Recognition không sở hữu Runtime scheduling.

17. Recognition không sở hữu Runtime retry.

18. Recognition không sở hữu cancellation authority.

19. Recognition không sở hữu global cache lifecycle.

20. Downstream không cần truy cập lại OCR Provider để hiểu Recognition Result.

---

# 50. Recommended MVP Recognition

MVP nên giữ đơn giản:

```text
Detection Region
      ↓
Region Validation
      ↓
Crop / Minimal Preparation
      ↓
Primary OCR Provider
      ↓
Provider Result Normalization
      ↓
Character / Line Construction
      ↓
Basic Confidence
      ↓
Recognition Result
```

MVP nên hỗ trợ:

* Simplified Chinese
* Traditional Chinese
* English
* horizontal text
* vertical text khi Provider hỗ trợ
* Character output
* Line output
* optional Word output
* Paragraph trong Region
* language/script metadata
* Recognition Confidence
* provider-neutral Result

Không bắt buộc ngay:

* multi-provider voting
* advanced lexical segmentation
* semantic correction
* learned reconstruction
* full document-language inference

---

# 51. Ownership References

| Concern                | Owner               |
| ---------------------- | ------------------- |
| Region                 | `DETECTION.md`      |
| Region Geometry        | `DETECTION.md`      |
| Recognition Result     | `RECOGNITION.md`    |
| Character              | `RECOGNITION.md`    |
| Word                   | `RECOGNITION.md`    |
| Line                   | `RECOGNITION.md`    |
| Paragraph              | `RECOGNITION.md`    |
| Recognition Confidence | `RECOGNITION.md`    |
| Writing Direction      | `TEXT_DIRECTION.md` |
| Layout Tree            | `LAYOUT.md`         |
| OCR Document           | `POSTPROCESS.md`    |
| Quality                | `QUALITY.md`        |
| Reading Order          | `READING_ORDER.md`  |
| Provider Contract      | `PROVIDERS.md`      |
| Retry                  | Runtime             |
| Cancellation           | Runtime             |
| Scheduling             | Runtime             |
| Cache Lifecycle        | Runtime             |
| Event Transport        | Event Bus           |
| Telemetry Transport    | Infrastructure      |

---

# 52. Summary

Text Recognition chuyển:

```text
Detection Region
```

thành:

```text
Recognition Result
```

với:

```text
Source Text
+
Character / Word / Line / Paragraph
+
Language / Script
+
Recognition Confidence
+
Metadata
```

Recognition trả lời:

```text
What is the text?
```

Boundary tổng quát:

```text
Detection
    → where

Recognition
    → what

Text Direction
    → how written

Layout
    → how organized

Reading Order
    → in what order
```

Nguyên tắc cốt lõi:

```text
Detection owns Region.

Recognition owns recognized text.

Text Direction owns writing direction.

Runtime owns execution.
```
