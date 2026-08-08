# Image Preprocessing

> **Status:** Draft
> **Version:** 1.1
> **Layer:** OCR Architecture
> **Depends On:** Image Source
> **Next Layer:** Detection

---

# 1. Purpose

Image Preprocessing là giai đoạn chuẩn bị hình ảnh trước khi Detection.

Nếu:

```text
Detection
    → "Text nằm ở đâu?"

Recognition
    → "Text là gì?"
```

thì Preprocessing trả lời:

```text
"Làm thế nào để ảnh đạt trạng thái phù hợp cho OCR?"
```

Preprocessing không tạo dữ liệu OCR.

Nó chỉ tạo một image representation ổn định hơn cho các bước OCR phía sau.

---

# 2. Scope

Preprocessing chỉ thao tác trên dữ liệu hình ảnh.

Nó chịu trách nhiệm:

* image validation
* format normalization
* orientation correction
* resolution normalization
* noise reduction
* contrast enhancement
* brightness/color normalization
* ROI processing
* transform metadata
* processed image generation

Preprocessing không xử lý:

* Character
* Word
* Paragraph
* semantic Region
* Layout
* Reading Order
* Translation
* Rendering

---

# 3. Goals

Preprocessing hướng tới:

* chất lượng hình ảnh ổn định
* giảm noise ảnh hưởng đến OCR
* chuẩn hóa nhiều image format
* giữ source image immutable
* giữ geometry có thể truy vết
* hỗ trợ nhiều OCR Provider
* tránh preprocessing không cần thiết
* giữ khả năng tái lập kết quả

---

# 4. Non-Goals

Preprocessing không thực hiện:

* Text Detection
* Character Recognition
* Translation
* semantic image editing
* deep image restoration
* generative image enhancement
* final Layout Analysis
* Reading Order
* Runtime scheduling
* Runtime retry
* cache lifecycle management

---

# 5. Architecture Position

```text
Image Source
     │
     ▼
Image Preprocessing
     │
     ▼
Processed Image
     │
     ▼
Text Detection
     │
     ▼
Recognition
```

Preprocessing là image-processing boundary của OCR Architecture.

Detection và Recognition không nên phải tự xử lý lại các normalization đã thuộc ownership của Preprocessing.

---

# 6. Terminology

## Source Image

Image Artifact đầu vào của OCR processing.

Source Image phải giữ immutable.

---

## Working Image

Representation tạm thời được dùng trong quá trình Preprocessing.

Working Image không nhất thiết trở thành persistent artifact.

---

## Processed Image

Image output của Preprocessing.

Đây là representation mà Detection có thể sử dụng.

---

## Enhancement

Phép biến đổi nhằm cải thiện khả năng OCR nhưng không thay đổi semantic content của source.

---

## Normalization

Quá trình đưa image về một representation nhất quán.

---

## ROI

`Region of Interest`.

Phạm vi hình ảnh mà Preprocessing áp dụng xử lý.

ROI có thể là:

* toàn bộ image
* user-selected area
* pipeline-selected area
* previously known visual area

---

## Processing Profile

Cấu hình định nghĩa preprocessing behavior.

---

## Transform Metadata

Metadata mô tả các phép biến đổi giữa Source Image và Processed Image.

---

# 7. Core Input

Preprocessing nhận:

```text
Source Image
+
Image Metadata
+
Processing Profile
+
optional ROI
```

Image Metadata có thể chứa:

* dimensions
* format
* color space
* orientation
* image version
* source identity

---

# 8. Core Output

Preprocessing tạo:

```text
Processed Image
+
Image Metadata
+
Transform Metadata
+
Processing Metadata
```

Optional output:

```text
Processing Statistics
```

Output phải đủ thông tin để downstream geometry có thể ánh xạ về source image.

---

# 9. High-Level Processing Flow

```text
Source Image
     │
     ▼
1. Input Validation
     │
     ▼
2. Format Normalization
     │
     ▼
3. Orientation Normalization
     │
     ▼
4. Resolution Normalization
     │
     ▼
5. Noise Reduction
     │
     ▼
6. Contrast / Brightness / Color Processing
     │
     ▼
7. ROI Processing
     │
     ▼
8. Transform Recording
     │
     ▼
Processed Image
```

Không phải mọi image đều phải chạy qua mọi operation.

Processing Profile quyết định các operation cần thiết.

---

# 10. Stage 1 — Input Validation

Image phải được kiểm tra trước khi xử lý.

Validation có thể gồm:

* format hợp lệ
* dimensions hợp lệ
* image readable
* bit depth phù hợp
* metadata hợp lệ
* dữ liệu không corrupted
* ROI nằm trong bounds

Input không hợp lệ phải bị từ chối trước các operation tốn tài nguyên.

---

# 11. Stage 2 — Format Normalization

CRAI có thể nhận nhiều định dạng image.

Ví dụ:

* PNG
* JPEG
* WebP
* BMP
* TIFF

Preprocessing có thể chuyển các format này về một Working Format thống nhất.

Mục tiêu là để các stage phía sau không phụ thuộc source format.

---

# 12. Working Format

Working Format phải:

* được Detection hỗ trợ
* giữ đủ visual information
* có color representation rõ ràng
* có dimensions rõ ràng
* có coordinate space xác định

Working Format là internal representation.

Public OCR contract không nên phụ thuộc trực tiếp vào một image library cụ thể.

---

# 13. Stage 3 — Orientation Normalization

Source Image có thể:

* bị xoay
* có EXIF Orientation
* bị lật
* có orientation metadata không phù hợp

Preprocessing phải tạo orientation ổn định cho OCR.

Orientation correction không được làm mất khả năng ánh xạ về Source Image.

---

# 14. Rotation vs Text Direction

Image orientation và text direction là hai khái niệm khác nhau.

Preprocessing xử lý:

```text
Image Orientation
```

Trong khi:

```text
TEXT_DIRECTION.md
```

sở hữu:

* Writing Mode
* Line Direction
* Paragraph Direction
* Character Flow

Preprocessing không được suy luận final Text Direction.

---

# 15. Stage 4 — Resolution Normalization

Resolution quá thấp có thể làm giảm Detection/Recognition quality.

Resolution quá lớn có thể làm tăng:

* latency
* memory usage
* provider cost

Processing Profile có thể định nghĩa:

* minimum useful resolution
* maximum dimensions
* scale policy
* aspect-ratio preservation

Aspect Ratio không được thay đổi tùy tiện.

---

# 16. Upscaling

Upscaling có thể được sử dụng khi text quá nhỏ.

Tuy nhiên upscaling phải policy-driven.

Không nên upscale mọi image mặc định.

Nếu upscaling thay đổi dimensions:

```text
Transform Metadata
```

phải được ghi lại.

---

# 17. Stage 5 — Noise Reduction

Noise Reduction nhằm giảm:

* sensor noise
* JPEG artifacts
* random pixels
* scanning noise
* image grain

Noise Reduction không được làm mất:

* character stroke
* punctuation
* thin line
* small glyph details

---

# 18. Noise Reduction Principle

Preprocessing ưu tiên:

```text
preserve information
```

hơn:

```text
produce visually prettier image
```

Một image đẹp hơn về mặt thị giác không nhất thiết tốt hơn cho OCR.

---

# 19. Stage 6 — Contrast Enhancement

Contrast Enhancement làm tăng sự phân biệt giữa foreground text và background.

Có thể sử dụng:

* global contrast
* adaptive contrast
* histogram equalization
* CLAHE

Tùy Processing Profile.

Không có một enhancement strategy phù hợp cho mọi input.

---

# 20. Brightness Normalization

Brightness adjustment có thể cần khi:

* image quá tối
* image quá sáng
* background uneven
* scan lighting không đồng đều

Brightness normalization phải tránh clipping chi tiết.

---

# 21. Gamma Processing

Gamma correction có thể được dùng để:

* tăng visibility ở dark regions
* cân bằng luminance
* chuẩn bị cho thresholding

Gamma policy phải thuộc Processing Profile.

---

# 22. Color Normalization

Preprocessing có thể chuẩn hóa:

* Color Space
* White Balance
* Channel representation

Ví dụ:

```text
RGB
↓
Grayscale
```

nếu Detection/Recognition strategy cho phép.

Không được chuyển grayscale nếu operation đó làm mất information cần thiết cho downstream strategy.

---

# 23. Binarization

Binarization có thể hữu ích cho:

* scanned documents
* high-contrast text
* some classical OCR engines

Nhưng không phải default requirement.

Modern OCR Provider có thể hoạt động tốt hơn với original or grayscale input.

Binarization phải profile-driven.

---

# 24. Background Suppression

Một số image có visual background phức tạp.

Preprocessing có thể hỗ trợ:

* background suppression
* channel isolation
* local contrast adjustment

nhưng không được biến Preprocessing thành semantic image segmentation.

Detection vẫn sở hữu text-region discovery.

---

# 25. Sharpening

Sharpening có thể tăng visibility của text stroke.

Nhưng excessive sharpening có thể tạo:

* halo
* false edges
* noise amplification

Do đó sharpening phải bounded.

---

# 26. Stage 7 — ROI Processing

Preprocessing phải hỗ trợ processing theo ROI.

ROI giúp:

* giảm processing latency
* giảm memory
* hỗ trợ user-selected translation area
* hỗ trợ incremental OCR
* hỗ trợ partial-page processing

---

# 27. ROI Sources

ROI có thể đến từ:

* user selection
* Capture
* OCR request
* previous Detection Result
* Pipeline coordination

Preprocessing chỉ nhận ROI.

Nó không sở hữu semantic Region Type.

---

# 28. ROI Geometry

ROI phải tham chiếu một coordinate space rõ ràng.

Khi ROI bị:

* crop
* resize
* rotate
* pad

mapping ngược về source coordinates phải được giữ.

---

# 29. Geometry Preservation

Bất kỳ operation nào thay đổi geometry đều phải sinh Transform Metadata.

Ví dụ:

```text
Source Image
     │
     ▼
Resize
     │
     ▼
Rotate
     │
     ▼
Crop
     │
     ▼
Processed Image
```

Downstream phải có khả năng xác định vị trí tương ứng trên Source Image.

---

# 30. Transform Chain

Transform Metadata có thể mô tả:

* resize
* scale
* crop
* rotation
* flip
* perspective correction
* padding
* translation

Transform chain phải giữ đúng order.

---

# 31. Derived Image

Nếu Preprocessing thay đổi:

* pixels
* dimensions
* coordinate space

thì output nên được xem là một derived image artifact.

Derived Image phải có:

* own identity
* parent identity
* dimensions
* transform metadata
* content/version identity

Artifact lifecycle thuộc Runtime/Resource ownership tương ứng.

---

# 32. Source Immutability

Preprocessing không được mutate Source Image.

Conceptually:

```text
Source Image
    │
    ├── remains unchanged
    │
    ▼
Derived Processed Image
```

Điều này cho phép:

* reprocessing
* debugging
* comparison
* alternative profile execution

---

# 33. Processing Profile

Processing Profile có thể chứa:

* format policy
* orientation policy
* resolution policy
* scaling thresholds
* noise-reduction strategy
* contrast strategy
* color strategy
* ROI policy
* operation version

Profile phải versioned khi thay đổi có thể ảnh hưởng semantic output.

---

# 34. Profile-Driven Processing

Preprocessing không nên chạy tất cả operation mặc định.

Ví dụ:

```text
Clean Manga Scan
    → minimal preprocessing

Low-quality Screenshot
    → noise + contrast

Small Text Region
    → upscale

Dark UI Capture
    → brightness / contrast
```

Processing strategy phụ thuộc profile và input characteristics.

---

# 35. Conditional Processing

Operation có thể bị skip nếu:

* image đã ở orientation đúng
* resolution phù hợp
* noise thấp
* contrast đủ tốt
* format đã được hỗ trợ

Mục tiêu là:

```text
minimum necessary transformation
```

---

# 36. Processing Metadata

Mỗi Processed Image nên giữ metadata như:

* Source Image ID
* Source Image Version
* dimensions trước/sau
* orientation trước/sau
* Color Space
* applied operations
* Processing Profile
* operation versions
* transform chain

---

# 37. Statistics

Preprocessing có thể tạo statistics như:

* operation duration
* input/output dimensions
* scaling factor
* processing operation count

Statistics có thể được xuất sang Telemetry.

Telemetry lifecycle không thuộc Preprocessing.

---

# 38. Diagnostics

Diagnostics có thể ghi:

* invalid image metadata
* unsupported transform
* suspicious dimensions
* operation warning
* quality degradation warning

Diagnostics không được chứa raw sensitive image data mặc định.

---

# 39. Determinism

Cùng:

```text
Source Image semantic identity
+
Processing Profile version
+
algorithm version
```

nên tạo structurally equivalent Processed Image metadata.

Một số image algorithms có thể không hoàn toàn bit-deterministic, nhưng transform semantics phải stable.

---

# 40. Provider Independence

Preprocessing không được phụ thuộc một OCR Provider cụ thể.

Có thể tồn tại provider-specific preference, nhưng public Preprocessing contract phải giữ provider-neutral.

Ví dụ:

```text
Provider A prefers grayscale
```

là processing hint.

Không phải dependency trực tiếp vào Provider SDK.

---

# 41. Detection Integration

Detection nhận:

```text
Processed Image
+
Transform Metadata
```

Detection không cần biết toàn bộ internal preprocessing implementation.

Detection chỉ cần:

* image identity
* image dimensions
* coordinate space
* transform lineage cần thiết

---

# 42. Recognition Integration

Recognition có thể cần derived crop hoặc enhanced image.

Tuy nhiên Recognition-specific Region Preparation không làm thay đổi ownership của global Preprocessing.

Global image preparation thuộc `PREPROCESS.md`.

Region-specific preparation có thể nằm trong Recognition/OCR Pipeline flow.

---

# 43. Runtime Integration

Preprocessing không sở hữu:

* execution queue
* retry
* cancellation authority
* WorkItem state
* resource allocation
* task scheduling

Preprocessing chỉ:

```text
input image
    ↓
processing semantics
    ↓
processed image
```

Runtime điều phối cách operation được thực thi.

---

# 44. Resource Integration

Image processing có thể sử dụng nhiều:

* RAM
* CPU
* temporary buffers

Preprocessing implementation phải release temporary buffers khi không còn cần thiết.

Tuy nhiên resource lifecycle contract thuộc Runtime/Infrastructure.

---

# 45. Cache Integration

Preprocessing có thể xác định semantic compatibility.

Ví dụ output không còn tương thích khi:

* source pixels thay đổi
* Processing Profile thay đổi
* transform strategy version thay đổi
* ROI thay đổi

Global cache:

* storage
* eviction
* retention
* cleanup

thuộc Runtime.

---

# 46. Event Integration

Preprocessing có thể tạo semantic facts nếu hệ thống cần, ví dụ:

```text
PreprocessingCompleted
PreprocessingFailed
```

Nhưng Event Bus:

* envelope
* transport
* delivery
* subscriber semantics

thuộc Event Bus owner.

---

# 47. Observability Integration

Useful measurements có thể gồm:

* preprocessing duration
* operation count
* input dimensions
* output dimensions
* scaling ratio
* warning count

Telemetry transport thuộc Runtime/Infrastructure.

---

# 48. Privacy

Preprocessing có thể xử lý screenshot hoặc private reading content.

Do đó:

* không log raw image mặc định
* diagnostics chỉ lưu metadata cần thiết
* temporary image artifact phải tuân thủ retention policy
* local-only data phải giữ local boundary

---

# 49. Architecture Invariants

Preprocessing phải luôn đảm bảo:

1. Không thực hiện Detection.

2. Không thực hiện Recognition.

3. Không thực hiện Translation.

4. Không sở hữu Reading Order.

5. Source Image không bị mutate.

6. Geometry-changing operation phải giữ Transform Metadata.

7. Processed Image phải có coordinate space rõ ràng.

8. Processing phải provider-neutral ở public contract.

9. Operation phải được điều khiển bởi Processing Profile hoặc explicit policy.

10. Không áp dụng enhancement không cần thiết một cách mù quáng.

11. ROI phải giữ mapping về source coordinates.

12. Derived Image phải giữ parent lineage.

13. Preprocessing không sở hữu Runtime scheduling.

14. Preprocessing không sở hữu Runtime retry.

15. Preprocessing không sở hữu cancellation authority.

16. Preprocessing không sở hữu global cache lifecycle.

17. Telemetry implementation không thuộc Preprocessing.

18. Published Processed Image không bị thay đổi âm thầm.

---

# 50. Recommended MVP Preprocessing

MVP nên giữ Preprocessing đơn giản.

```text
Source Image
    ↓
Validation
    ↓
Format Normalization
    ↓
Orientation Correction
    ↓
Resolution Check
    ↓
Optional Contrast / Grayscale
    ↓
Processed Image
```

MVP nên hỗ trợ:

* PNG
* JPEG
* WebP
* orientation normalization
* basic dimension constraints
* optional grayscale
* optional contrast adjustment
* manual ROI
* transform metadata
* immutable Source Image

Không cần ngay:

* advanced AI enhancement
* complex restoration
* multi-candidate preprocessing
* learned preprocessing strategy
* provider-specific preprocessing graph

---

# 51. Ownership References

| Concern             | Owner                      |
| ------------------- | -------------------------- |
| Image Preprocessing | `PREPROCESS.md`            |
| Text Detection      | `DETECTION.md`             |
| Region              | `DETECTION.md`             |
| Recognition         | `RECOGNITION.md`           |
| Text Direction      | `TEXT_DIRECTION.md`        |
| Layout              | `LAYOUT.md`                |
| OCR Pipeline        | `PIPELINE.md`              |
| Resource Lifecycle  | Runtime / Resource Manager |
| Retry               | Runtime                    |
| Cancellation        | Runtime                    |
| Cache Lifecycle     | Runtime                    |
| Telemetry Transport | Infrastructure             |
| Event Transport     | Event Bus                  |

---

# 52. Summary

Image Preprocessing chuyển:

```text
Source Image
```

thành:

```text
Processed Image
+
Transform Metadata
```

để Detection có một visual input ổn định.

Flow cơ bản:

```text
Validate
    ↓
Normalize Format
    ↓
Normalize Orientation
    ↓
Normalize Resolution
    ↓
Apply Necessary Enhancement
    ↓
Process ROI
    ↓
Record Transform
    ↓
Processed Image
```

Nguyên tắc cốt lõi:

```text
Preprocessing prepares the image.

Detection finds the text.

Recognition reads the text.

Runtime executes the work.
```
