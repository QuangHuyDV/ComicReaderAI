# Text Detection

> **Status:** Draft
> **Version:** 1.1.0
> **Owner:** OCR Architecture
> **Layer:** OCR Architecture
> **Depends On:** Preprocessing
> **Next Layer:** Recognition, Text Direction, Layout Analysis

---

# 1. Purpose

Text Detection là thành phần chịu trách nhiệm xác định:

```text
Where is the text?
```

trên một hình ảnh.

Detection tìm các vùng có khả năng chứa văn bản và biểu diễn chúng dưới dạng `Region` có identity, geometry và metadata ổn định.

Detection không nhận dạng nội dung chữ.

Nó tạo nền tảng hình học cho:

* Recognition
* Text Direction
* Layout Analysis
* Reading Order
* Presentation

---

# 2. Scope

Detection chịu trách nhiệm:

* phát hiện vùng chứa văn bản
* tạo Region
* xác định geometry
* chuẩn hóa coordinate
* xác định Detection Confidence
* phân loại sơ bộ Region Type
* tạo Region hierarchy khi có đủ dữ liệu
* merge/split Region
* validate Region
* cung cấp direction hint khi phù hợp

Detection có thể xử lý:

* Manga
* Manhua
* Manhwa
* Novel Screenshot
* Web Page
* Captured Screen
* Local Image
* user-selected ROI

---

# 3. Non-Goals

Detection không thực hiện:

* Character Recognition
* Translation
* semantic text analysis
* grammar correction
* spell checking
* final Reading Order
* font estimation
* text rendering
* bubble redrawing
* Runtime scheduling
* Runtime retry
* Event Bus behavior
* global cache lifecycle

---

# 4. Architecture Position

```text
Image
  │
  ▼
Preprocessing
  │
  ▼
Text Detection
  │
  ▼
Detection Result
  │
  ├──► Recognition
  ├──► Text Direction
  └──► Layout Analysis
```

Detection chỉ giao tiếp thông qua CRAI contract.

Detection Engine implementation không được rò rỉ sang các stage phía sau.

---

# 5. Design Principles

Detection phải:

* chỉ trả lời "text nằm ở đâu"
* không trả lời "text là gì"
* giữ source image immutable
* provider-neutral
* deterministic khi cùng semantic input và strategy version
* serializable
* versionable
* giữ Region identity ổn định trong cùng Detection Result
* giữ geometry có thể truy vết về source image
* không tạo semantic dependency vào Translation hoặc Presentation

---

# 6. Terminology

## Image

Ảnh đầu vào đã được chuẩn bị cho Detection.

---

## Region

Một vùng nghi ngờ chứa văn bản.

Region chưa chắc chứa văn bản hợp lệ cho tới khi validation hoàn tất.

---

## Detection Result

Tập hợp Region và metadata được Detection tạo ra.

---

## Geometry

Biểu diễn hình học của Region.

Có thể gồm:

* Bounding Box
* Polygon
* optional Segmentation Mask
* Rotation
* Transform reference

---

## Detection Confidence

Độ tin cậy rằng Region thực sự chứa văn bản.

---

## Classification Confidence

Độ tin cậy của `Region Type`.

Hai confidence này độc lập.

---

## Region Type

Phân loại sơ bộ của Region.

---

## ROI

Region of Interest giới hạn phạm vi Detection.

---

# 7. High-Level Detection Pipeline

```text
Processed Image
      │
      ▼
1. Input Validation
      │
      ▼
2. Detection Context Resolution
      │
      ▼
3. Detection Engine
      │
      ▼
4. Region Generation
      │
      ▼
5. Geometry Normalization
      │
      ▼
6. Region Classification
      │
      ▼
7. Region Merge / Split
      │
      ▼
8. Region Validation
      │
      ▼
9. Detection Result Assembly
```

Implementation có thể gộp một số bước nếu provider hỗ trợ trực tiếp.

Contract cuối cùng vẫn phải giữ cùng semantics.

---

# 8. Stage 1 — Input Validation

Detection kiểm tra:

* image hợp lệ
* image readable
* dimensions hợp lệ
* coordinate space tồn tại
* ROI hợp lệ nếu có
* Detection Profile hợp lệ

Detection không nên tiếp tục khi geometry input không xác định được.

---

# 9. Stage 2 — Detection Context Resolution

Detection Context có thể sử dụng:

* Processed Image
* image dimensions
* ROI
* Detection Profile
* language/script hint
* source type
* previous compatible Detection Result

Hints chỉ hỗ trợ Detection.

Chúng không làm thay đổi semantic boundary của Detection.

---

# 10. Stage 3 — Detection Engine

Detection Engine chịu trách nhiệm tìm candidate region.

Implementation có thể là:

* Deep Learning
* Classical Computer Vision
* Hybrid Detection
* OCR Provider with detection capability
* custom AI model

Pipeline không phụ thuộc engine cụ thể.

---

# 11. Stage 4 — Region Generation

Engine output được chuẩn hóa thành CRAI `Region`.

Mỗi Region tối thiểu cần:

```text
Region
├── Region ID
├── Geometry
├── Detection Confidence
└── Metadata
```

Có thể bổ sung:

```text
Region Type
Classification Confidence
Rotation
Direction Hint
Parent ID
Child IDs
Provider Metadata
```

---

# 12. Detection Result

Canonical output:

```text
Detection Result
├── Metadata
├── Image Reference
├── Image Version
├── Regions[]
├── Statistics
└── Diagnostics
```

Detection Result phải:

* immutable sau khi publish
* serializable
* versioned
* provider-neutral
* giữ exact image identity
* giữ Region IDs ổn định trong cùng revision

---

# 13. Coordinate System

Detection sử dụng coordinate system thống nhất của image artifact mà nó tham chiếu.

Default origin:

```text
(0,0)
┌────────────────────► X
│
│
│
▼
Y
```

* origin nằm ở góc trên trái
* X tăng từ trái sang phải
* Y tăng từ trên xuống dưới

Đơn vị chuẩn là pixel trong coordinate space được khai báo.

---

# 14. Coordinate Precision

Geometry nên hỗ trợ floating-point coordinates.

Điều này giúp giảm sai số khi:

* resize
* rotate
* perspective transform
* polygon mapping
* nhiều transform liên tiếp

Integer rounding chỉ nên xảy ra khi downstream operation thực sự yêu cầu.

---

# 15. Coordinate Invariants

Geometry phải đảm bảo:

* cùng Region luôn tham chiếu cùng visual location trong cùng image version
* không phụ thuộc OCR Provider
* không thay đổi sau Recognition
* không thay đổi bởi Translation
* mọi transform có lineage rõ ràng

---

# 16. Bounding Box

Bounding Box là hình chữ nhật bao quanh Region.

Conceptual model:

```text
BoundingBox
├── x
├── y
├── width
└── height
```

Bounding Box phù hợp cho:

* indexing
* quick crop
* preview
* spatial search
* viewport optimization

Bounding Box không phải biểu diễn chính xác nhất của text contour.

---

# 17. Polygon

Polygon mô tả Region chính xác hơn Bounding Box.

Conceptual model:

```text
Polygon
└── points[]
     ├── x
     └── y
```

Polygon nên:

* giữ point ordering nhất quán
* hỗ trợ Region nghiêng
* hỗ trợ irregular shape
* hỗ trợ vertical text
* hỗ trợ geometry mapping

Bounding Box và Polygon có thể cùng tồn tại.

---

# 18. Segmentation Mask

Segmentation Mask là biểu diễn pixel-level của Region.

Mask có thể hữu ích cho:

* precise region extraction
* advanced OCR
* text removal
* inpainting

Mask không bắt buộc phải nằm trực tiếp trong mọi Detection Result.

Có thể lưu dưới dạng:

* optional artifact
* compressed representation
* lazily generated artifact

Mask lifecycle thuộc resource/artifact owner tương ứng.

---

# 19. Rotation

Region có thể có góc xoay.

Rotation chỉ là geometry metadata.

Ví dụ:

```text
0°
90°
180°
270°
```

hoặc góc bất kỳ nếu provider hỗ trợ.

Rotation không thay đổi source image.

---

# 20. Geometry Transformation

Detection sử dụng transform metadata để ánh xạ Region giữa các image version hoặc derived image.

Ví dụ:

```text
Source Image
    ↓
Resize
    ↓
Deskew
    ↓
Crop
    ↓
Processed Image
    ↓
Detection
```

Mọi Region phải có thể truy vết về coordinate space nguồn khi transform metadata đủ.

Detection không sở hữu toàn bộ artifact lifecycle của transform.

---

# 21. Region Type

Detection có thể phân loại sơ bộ Region để hỗ trợ downstream processing.

Built-in types hiện tại:

* Speech Bubble
* Narration Box
* Sound Effects
* Background Text
* UI Text
* Watermark
* Advertisement
* Unknown Region

Nếu không đủ bằng chứng:

```text
Unknown Region
```

phải được sử dụng thay vì ép classification.

---

# 22. Region Type Semantics

## Speech Bubble

Vùng chứa dialogue.

Detection có thể nhận diện:

* outer bubble geometry
* inner text region
* bubble relationship

Detection không xác định speaker identity.

---

## Narration Box

Vùng chứa narration hoặc descriptive text.

Không được phân loại thành Speech Bubble chỉ dựa vào shape.

---

## Sound Effects

Text visual effect thường:

* xoay
* nghiêng
* biến dạng
* hòa vào illustration

Detection tập trung vào geometry.

---

## Background Text

Text thuộc visual background:

* sign
* label
* poster
* shop name
* environmental text

---

## UI Text

Text thuộc application/web interface.

---

## Watermark

Text biểu thị source, copyright hoặc publishing mark.

---

## Advertisement

Text/region thuộc nội dung quảng cáo.

---

## Unknown Region

Fallback khi classification chưa đủ chắc chắn.

Unknown không phải lỗi.

---

# 23. Region Type Invariants

Region Type:

* độc lập Translation
* độc lập Presentation
* không phụ thuộc OCR Provider-specific enum
* giữ stable semantics trong cùng contract version
* có Classification Confidence riêng

---

# 24. Region Hierarchy

Region có thể có parent-child relationship.

Ví dụ:

```text
Page
 ├── Speech Bubble
 │    ├── Text Region A
 │    └── Text Region B
 └── Narration Box
      └── Text Region C
```

Hierarchy giúp downstream:

* Layout
* Recognition
* Reading Order
* Presentation

không phải suy luận lại toàn bộ visual grouping.

---

# 25. Hierarchy Rules

Một Region:

* có tối đa một direct parent trong tree model
* có thể có nhiều child
* không được tạo cycle
* child phải nằm trong semantic/geometry relation hợp lệ với parent

Complex graph relationships ngoài hierarchy thuộc Layout Analysis.

---

# 26. Detection Relationships vs Layout Relationships

Detection chỉ sở hữu relationship cần thiết để mô tả Region generation và hierarchy sơ bộ.

Các quan hệ không gian tổng quát như:

```text
above
below
left_of
right_of
adjacent
overlaps
```

được Layout Analysis chuẩn hóa ở bước sau.

Detection không nên trở thành owner của toàn bộ Layout graph.

---

# 27. Region Merge

Merge kết hợp nhiều Region thành một Region logic.

Có thể merge khi:

* geometry gần nhau
* cùng Region Type
* cùng orientation
* cùng probable visual entity
* profile cho phép

Merge không được dùng để thực hiện semantic text reconstruction.

---

# 28. Merge Result

Sau merge:

* tạo hoặc giữ Region identity theo strategy versioned
* Geometry được tính lại
* Bounding Box được cập nhật
* Polygon được cập nhật nếu cần
* Confidence được đánh giá lại
* lineage của source Regions phải giữ được khi cần diagnostics

---

# 29. Region Split

Split tách một Region thành nhiều Region nhỏ hơn.

Có thể split khi:

* chứa nhiều visual text blocks
* có whitespace separation rõ
* có orientation khác nhau
* có Region Type khác nhau
* provider detection quá coarse

---

# 30. Split Result

Mỗi Region mới phải có:

* Region ID riêng
* Geometry riêng
* Detection Confidence riêng
* source lineage phù hợp

Split không được làm mất visual content đã được detect hợp lệ.

---

# 31. Region Validation

Trước khi publish Detection Result, Region phải được validate.

Checks có thể gồm:

* non-zero area
* coordinates nằm trong image bounds
* Bounding Box hợp lệ
* Polygon hợp lệ
* Polygon không tự cắt khi contract yêu cầu
* Confidence trong range hợp lệ
* Region Type hợp lệ
* identity uniqueness

---

# 32. Invalid Region

Invalid Region có thể:

* bị reject
* được giữ với warning

tùy Detection Profile và downstream requirement.

Không được âm thầm sửa geometry khi thiếu đủ bằng chứng.

---

# 33. Detection Confidence

Detection Confidence trả lời:

```text
How likely is this Region to contain text?
```

Nó không phản ánh:

* recognition correctness
* translation correctness
* layout correctness

---

# 34. Classification Confidence

Classification Confidence trả lời:

```text
How likely is this Region Type correct?
```

Ví dụ:

```text
Region contains text = 0.97
Region type = Speech Bubble = 0.62
```

Hai confidence phải được giữ riêng.

---

# 35. Confidence Propagation

Detection Confidence có thể được downstream sử dụng như signal.

Nhưng không được tự động chuyển thành:

* Recognition Confidence
* Layout Confidence
* Reading Confidence
* Quality Score

Mỗi component sở hữu confidence semantics của chính nó.

---

# 36. Reading Direction Hint

Detection có thể tạo direction hint sơ bộ cho Region.

Ví dụ:

* LTR
* RTL
* TopToBottom
* BottomToTop
* Unknown

Đây chỉ là hint.

Authoritative writing-direction semantics thuộc:

```text
TEXT_DIRECTION.md
```

Detection không sở hữu final Reading Order.

---

# 37. Incremental Detection Semantics

Detection có thể hỗ trợ semantic reuse khi chỉ một phần visual input thay đổi.

Ví dụ:

* long scrolling image
* screen region update
* user-selected ROI change

Semantic requirement:

```text
unchanged Region
    → may preserve identity when compatibility is proven

changed scope
    → recompute affected Region set
```

Execution scheduling của incremental work thuộc Runtime.

---

# 38. Detection Compatibility

Detection Result có thể được tái sử dụng khi semantic inputs vẫn tương thích.

Compatibility có thể phụ thuộc:

* Image ID
* Image Version
* content hash
* Detection Profile version
* Detection Strategy version
* provider capability version
* ROI

Detection chỉ định nghĩa semantic compatibility.

Global cache policy thuộc Runtime.

---

# 39. Provider Independence

Detection Provider có thể là:

* PaddleOCR detector
* MMOCR
* EasyOCR
* custom model
* classical CV
* cloud OCR capability

Provider output phải được normalize thành CRAI Detection Result.

Provider-native type không được crossing public boundary.

---

# 40. Provider Metadata

Detection Result có thể giữ provider metadata để:

* diagnostics
* reproducibility
* compatibility
* benchmarking

Nhưng downstream logic không được phụ thuộc trực tiếp vào provider-native schema.

---

# 41. Detection Profile

Detection Profile có thể ảnh hưởng:

* detector strategy
* Region Type policy
* confidence threshold
* merge/split policy
* ROI policy
* output geometry detail
* optional mask generation

Profile phải versioned nếu thay đổi semantics có thể ảnh hưởng output.

---

# 42. Determinism

Với cùng:

```text
Image semantic identity
+
Detection Profile
+
Detection Strategy Version
```

Detection nên tạo structurally equivalent result, ngoại trừ provider nondeterminism được ghi nhận rõ.

Determinism quan trọng cho:

* debugging
* cache compatibility
* regression detection
* repeatability

---

# 43. Immutability

Source image không được mutate.

Published Detection Result cũng nên immutable.

Nếu Detection cần thay đổi:

```text
new Detection Result revision
```

phải được tạo thay vì sửa silent result cũ.

---

# 44. OCR Pipeline Integration

Detection chỉ là một stage của OCR Pipeline.

```text
Processed Image
    ↓
Detection
    ↓
Detection Result
    ↓
Recognition
```

Detection không tự gọi downstream stage theo business/runtime semantics.

Runtime/Pipeline orchestration quyết định execution flow.

---

# 45. Recognition Integration

Recognition sử dụng:

* Region identity
* Region Geometry
* Region Type
* Detection Confidence
* Detection metadata

Recognition không được tự detect lại Region nếu không có explicit reason/policy.

---

# 46. Text Direction Integration

Text Direction có thể sử dụng:

* Region Geometry
* Detection orientation hint
* Recognition geometry
* Character/Line positions

Direction semantics vẫn thuộc `TEXT_DIRECTION.md`.

---

# 47. Layout Integration

Layout sử dụng:

* Regions
* Geometry
* Region Type
* hierarchy hints

để xây:

* Panel
* Container
* Block
* Layout Tree
* Spatial Relationship Graph

Layout mới là owner của page-level spatial structure.

---

# 48. Presentation Integration

Presentation có thể sử dụng Detection Geometry để:

* highlight
* overlay
* map source text
* locate visual areas

Presentation không được thay đổi Detection semantics.

---

# 49. Runtime Integration

Detection không sở hữu:

* Queued state
* Running state
* retry attempts
* execution timeout policy
* cancellation authority
* Scheduler behavior
* stale authority

Runtime chịu trách nhiệm những phần này.

Detection chỉ tạo semantic result hoặc semantic failure information.

---

# 50. Cache Integration

Detection có thể định nghĩa khi một Detection Result không còn compatible.

Ví dụ:

* image content thay đổi
* ROI thay đổi
* Detection Profile thay đổi
* Detection Strategy thay đổi

Eviction, retention và cache storage thuộc Runtime.

---

# 51. Event Integration

Detection có thể tạo domain facts như:

```text
DetectionCompleted
DetectionFailed
RegionDetected
```

Ý nghĩa Detection-specific thuộc Detection.

Event transport, envelope và delivery semantics thuộc Event Bus.

---

# 52. Error Integration

Detection có thể sinh semantic errors như:

```text
InvalidImage
InvalidROI
InvalidGeometry
DetectionUnavailable
DetectionResultInvalid
UnsupportedDetectionMode
```

Provider-specific errors phải được map trước khi crossing Detection boundary.

Runtime Error Model sở hữu normalization ở cấp execution.

---

# 53. Observability Integration

Detection có thể cung cấp measurements như:

* Region count
* Detection duration
* Detection confidence distribution
* invalid Region count
* merge/split count
* provider identity
* strategy version

Telemetry transport và lifecycle thuộc Runtime/Infrastructure.

---

# 54. Architecture Invariants

Detection luôn phải đảm bảo:

1. Không thay đổi source image.

2. Chỉ xác định vị trí và visual classification của text-like Regions.

3. Không thực hiện Recognition.

4. Không thực hiện Translation.

5. Region identity phải ổn định trong cùng Detection Result revision.

6. Geometry luôn tham chiếu exact image version.

7. Geometry không phụ thuộc OCR Provider-native schema.

8. Detection Confidence và Classification Confidence là hai khái niệm độc lập.

9. Unknown Region là fallback hợp lệ.

10. Provider-native response không được crossing Detection contract.

11. Detection không sở hữu final Reading Order.

12. Detection không sở hữu Layout Tree.

13. Detection không sở hữu Runtime scheduling.

14. Detection không sở hữu Runtime retry.

15. Detection không sở hữu cancellation authority.

16. Detection không sở hữu global cache lifecycle.

17. Detection Result phải serializable và provider-neutral.

18. Published Detection Result không bị mutate âm thầm.

19. Region merge/split phải giữ đủ lineage để truy vết khi cần.

20. Detection-specific semantics chỉ được định nghĩa authoritative tại tài liệu này.

---

# 55. Recommended MVP Detection

MVP nên ưu tiên mô hình đơn giản:

```text
Processed Image
      ↓
General Text Detector
      ↓
Region Generation
      ↓
Bounding Box / Polygon
      ↓
Basic Region Classification
      ↓
Region Validation
      ↓
Detection Result
```

MVP nên hỗ trợ:

* full-page detection
* manual ROI
* Bounding Box
* Polygon khi provider hỗ trợ
* Detection Confidence
* Region Type cơ bản
* Speech Bubble
* Narration
* SFX
* Background Text
* UI Text
* Unknown Region
* stable Region identity trong một result
* provider-neutral contract

Không bắt buộc MVP phải có:

* segmentation mask mọi Region
* semantic AI detection
* 3D geometry
* video temporal detection
* distributed Detection
* learned merge/split strategies

---

# 56. Ownership References

| Concern                    | Owner               |
| -------------------------- | ------------------- |
| Preprocessing              | `PREPROCESS.md`     |
| Detection Result           | `DETECTION.md`      |
| Region                     | `DETECTION.md`      |
| Region Type                | `DETECTION.md`      |
| Detection Geometry         | `DETECTION.md`      |
| Detection Confidence       | `DETECTION.md`      |
| Recognition Text           | `RECOGNITION.md`    |
| Writing Direction          | `TEXT_DIRECTION.md` |
| Layout Tree                | `LAYOUT.md`         |
| Spatial Relationship Graph | `LAYOUT.md`         |
| OCR Document               | `POSTPROCESS.md`    |
| Reading Order              | `READING_ORDER.md`  |
| Retry                      | Runtime             |
| Cancellation               | Runtime             |
| Scheduling                 | Runtime             |
| Cache Lifecycle            | Runtime             |
| Event Transport            | Event Bus           |
| Telemetry Transport        | Infrastructure      |

---

# 57. Summary

Text Detection chuyển:

```text
Processed Image
```

thành:

```text
Detection Result
```

gồm:

```text
Regions
+
Geometry
+
Region Type
+
Detection Confidence
+
Metadata
```

Detection trả lời duy nhất:

```text
Where is the text?
```

Các stage phía sau lần lượt chịu trách nhiệm:

```text
Recognition
    → What is the text?

Text Direction
    → How is it written?

Layout
    → How is it organized?

Reading Order
    → In what order should it be read?
```

Nguyên tắc cốt lõi:

```text
Detection owns Region semantics.

Layout owns spatial structure.

Recognition owns recognized text.

Runtime owns execution.
```
