# Text Detection

> **Status:** Draft (V1)
> **Version:** 0.1.0
> **Owner:** OCR Architecture
> **Last Updated:** 2026-07-28

---

# 1. Purpose

Text Detection là thành phần chịu trách nhiệm xác định **vị trí của toàn bộ vùng chứa văn bản** trên một hình ảnh mà **không thực hiện nhận dạng nội dung chữ**.

Detection là bước đầu tiên của OCR Pipeline và đóng vai trò tạo nền tảng cho toàn bộ các giai đoạn phía sau như:

* Text Recognition
* Reading Order
* Text Processing
* Translation
* Presentation

Kết quả của Detection phải đủ chính xác để các module phía sau có thể hoạt động độc lập mà không cần tự tìm lại vị trí văn bản.

---

## Design Principles

Text Detection phải đảm bảo các nguyên tắc sau:

* Chỉ xác định **"Where is the text?"**
* Không trả lời **"What is the text?"**
* Không dịch.
* Không chỉnh sửa ảnh.
* Không render.
* Không thay đổi dữ liệu gốc.

Detection chỉ sinh ra metadata mô tả các vùng văn bản.

---

# 2. Scope

Detection chịu trách nhiệm:

* tìm toàn bộ vùng chứa văn bản
* xác định hình học của từng vùng
* phân loại sơ bộ vùng văn bản
* ước lượng hướng đọc
* tính confidence ban đầu
* chuẩn hóa kết quả thành Detection Result

Detection hoạt động trên mọi nguồn ảnh:

* Comic
* Manga
* Manhua
* Novel Screenshot
* Web Page
* Captured Screen
* Local Image

---

## Out of Scope

Detection **không chịu trách nhiệm**:

* OCR
* Translation
* Text Correction
* Grammar
* Spell Check
* Font Estimation
* Bubble Redrawing
* Image Enhancement
* Rendering

Các nhiệm vụ trên thuộc module khác.

---

# 3. Terminology

## Image

Ảnh đầu vào của OCR Pipeline.

---

## Region

Một vùng nghi ngờ chứa văn bản.

Một Region chưa chắc đã chứa chữ hợp lệ.

---

## Detection

Quá trình phát hiện Region.

---

## Bounding Box

Hình chữ nhật bao quanh Region.

---

## Polygon

Đa giác mô tả chính xác biên của Region.

---

## Mask

Ma trận biểu diễn từng pixel thuộc Region.

Mask có độ chính xác cao hơn Bounding Box.

---

## Detection Result

Danh sách Region được tạo ra sau Detection.

---

## Confidence

Độ tin cậy của Detection.

Confidence chỉ phản ánh khả năng vùng đó chứa văn bản.

Không phản ánh OCR đúng hay sai.

---

# 4. Goals

Detection được thiết kế nhằm đạt các mục tiêu sau.

## Accuracy

Phát hiện được nhiều vùng văn bản nhất có thể.

---

## Stability

Cùng một ảnh phải tạo ra kết quả gần như giống nhau.

---

## Performance

Hoạt động đủ nhanh cho chế độ đọc thời gian thực.

---

## Extensibility

Có thể thay đổi Detection Engine mà không ảnh hưởng Recognition.

---

## Independence

Không phụ thuộc Translation.

Không phụ thuộc Presentation.

---

## Deterministic Output

Đầu vào giống nhau nên tạo ra kết quả giống nhau trong cùng một cấu hình.

---

## Provider Agnostic

Có thể sử dụng:

* PaddleOCR
* MMOCR
* EasyOCR
* OpenCV
* Custom AI

mà không thay đổi contract.

---

# 5. Non-Goals

Detection không cố gắng:

* đọc nội dung
* đoán ngôn ngữ
* sửa lỗi OCR
* dịch
* suy luận ngữ nghĩa
* xác định người nói
* thay thế Bubble
* chỉnh sửa hình ảnh

Các bước này sẽ được xử lý ở pipeline phía sau.

---

# 6. Responsibilities

Detection chịu trách nhiệm:

* nhận Image đã được chuẩn hóa
* phân tích toàn bộ ảnh
* phát hiện vùng chứa văn bản
* tạo Detection Region
* chuẩn hóa tọa độ
* sinh Detection Metadata
* trả về Detection Result

Detection phải hoạt động như một dịch vụ độc lập.

---

## Detection MUST

Detection phải:

* bất biến với ảnh gốc
* không thay đổi pixel gốc
* không tạo bản dịch
* không OCR
* không cache sai định dạng
* không làm mất Region hợp lệ

---

# 7. Architecture Position

Detection là bước đầu tiên của OCR.

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

Recognition

        │

        ▼

Reading Order

        │

        ▼

Text Processing

        │

        ▼

Translation

        │

        ▼

Presentation
```

Detection chỉ giao tiếp thông qua contract.

Không được gọi trực tiếp Recognition Engine.

Điều này giúp:

* dễ thay Detection
* dễ benchmark
* dễ cache
* dễ retry

---

# 8. High-Level Pipeline

Detection bao gồm các bước chính.

```text
Input Image

↓

Validation

↓

Normalization

↓

Preprocessing

↓

Detection Engine

↓

Region Generation

↓

Region Validation

↓

Confidence Estimation

↓

Output Assembly

↓

Detection Result
```

Mỗi bước phải có thể được thay thế bằng implementation khác mà không thay đổi contract.

---

## Detection Engine

Detection Engine là thành phần tìm Region.

Ví dụ:

* Deep Learning
* Classical Vision
* Hybrid Detection

Pipeline không phụ thuộc engine cụ thể.

---

# 9. Detection Lifecycle

Một Detection Request trải qua các trạng thái sau.

```text
Created

↓

Queued

↓

Running

↓

Region Detecting

↓

Region Validating

↓

Completed
```

Nếu xảy ra lỗi.

```text
Running

↓

Failed
```

Nếu bị hủy.

```text
Running

↓

Cancelled
```

Lifecycle này sẽ được mở rộng ở Runtime Architecture.

---

# 10. Inputs

Detection nhận các dữ liệu sau.

## Required

* Image
* Image Metadata
* Detection Profile

---

## Optional

* Previous Detection Result
* ROI
* Language Hint
* Runtime Configuration

---

## Input Requirements

Ảnh đầu vào phải:

* hợp lệ
* đọc được
* đã normalize
* có kích thước xác định
* có hệ tọa độ rõ ràng

Nếu không đáp ứng, Detection phải trả lỗi thay vì tiếp tục xử lý.

---

# 11. Outputs

Detection trả về một Detection Result.

Detection Result bao gồm:

* danh sách Region
* metadata
* confidence
* statistics
* execution information

Detection Result là đầu vào duy nhất của Recognition.

Recognition không được tự detect lại nếu không có yêu cầu đặc biệt.

---

## Output Characteristics

Output phải:

* immutable
* deterministic
* serializable
* versioned
* cacheable

Điều này cho phép lưu cache và tái sử dụng giữa nhiều lần OCR.

---

# 12. Region Model

Region là đơn vị cơ bản của Detection.

Mỗi Region đại diện cho một vùng nghi ngờ chứa văn bản.

Ở phiên bản V1, Region được định nghĩa ở mức khái niệm.

```text
Region

id

geometry

confidence

metadata
```

Trong các phiên bản tiếp theo, Region sẽ được mở rộng với:

* Bounding Box
* Polygon
* Segmentation Mask
* Rotation
* Region Type
* Reading Direction Hint
* Parent / Child Relationship
* Detection Provider Metadata
* Runtime Metadata

Region phải có định danh ổn định trong suốt vòng đời của một Detection Result để các bước Recognition, Translation và Presentation có thể tham chiếu cùng một thực thể mà không cần tạo lại.

---

# Summary

Text Detection là tầng chịu trách nhiệm **xác định vị trí của văn bản**, không phải **đọc hay hiểu nội dung văn bản**.

Việc tách Detection khỏi Recognition giúp:

* thay đổi OCR Engine mà không ảnh hưởng pipeline;
* cache Detection lâu hơn Recognition;
* xử lý song song nhiều Recognition Provider;
* hỗ trợ incremental OCR trong tương lai;
* giữ kiến trúc OCR của CRAI theo hướng module hóa và dễ mở rộng.

## 13. Coordinate System

### Overview

Text Detection phải sử dụng một hệ tọa độ thống nhất trong toàn bộ OCR Pipeline nhằm đảm bảo mọi module đều tham chiếu đến cùng một vị trí trên ảnh.

Mọi Detection Result phải được biểu diễn theo **Image Coordinate System** của ảnh đầu vào sau khi đã hoàn thành bước Normalization.

Detection không được sử dụng hệ tọa độ riêng của từng OCR Provider trong contract công khai.

---

### Design Goals

Coordinate System phải đảm bảo:

* nhất quán giữa mọi module
* độc lập với OCR Provider
* không phụ thuộc độ phân giải hiển thị
* có thể chuyển đổi giữa nhiều phiên bản ảnh
* hỗ trợ lưu cache lâu dài
* hỗ trợ nhiều phép biến đổi hình học

---

### Coordinate Origin

Gốc tọa độ được định nghĩa:

```text
(0,0)
┌────────────────────────────► X
│
│
│
│
▼
Y
```

Trong đó:

* gốc tọa độ nằm tại góc trên bên trái ảnh
* trục X tăng từ trái sang phải
* trục Y tăng từ trên xuống dưới

Đây là hệ tọa độ chuẩn được sử dụng xuyên suốt CRAI.

---

### Coordinate Unit

Đơn vị mặc định là **pixel**.

Không sử dụng:

* inch
* point
* phần trăm
* đơn vị phụ thuộc DPI

Mọi Region phải có thể ánh xạ trực tiếp về pixel trên ảnh nguồn.

---

### Coordinate Precision

Hệ thống phải hỗ trợ tọa độ dấu phẩy động (`float`) nhằm:

* giảm sai số khi resize
* giảm sai số khi rotate
* hỗ trợ polygon chính xác
* tránh lỗi tích lũy khi transform nhiều lần

Việc làm tròn sang số nguyên chỉ được thực hiện khi thực sự cần thiết (ví dụ: render hoặc crop ảnh).

---

### Coordinate Invariants

Coordinate System phải đảm bảo:

* cùng một Region luôn tham chiếu đến cùng vị trí trên ảnh
* không thay đổi khi thay OCR Provider
* không thay đổi sau Recognition
* không thay đổi sau Translation
* chỉ Geometry Transformation mới được phép sinh hệ tọa độ mới

---

## 14. Bounding Box

### Purpose

Bounding Box là hình chữ nhật nhỏ nhất bao phủ toàn bộ Region.

Bounding Box được sử dụng cho:

* crop ảnh
* preview
* indexing
* spatial search
* collision detection
* viewport optimization

Bounding Box không nhằm mô tả chính xác hình dạng văn bản.

---

### Structure

Một Bounding Box tối thiểu bao gồm:

```text
BoundingBox

x

y

width

height
```

Trong đó:

* `(x, y)` là góc trên bên trái
* `width` là chiều rộng
* `height` là chiều cao

---

### Characteristics

Bounding Box:

* luôn song song với trục ảnh
* dễ tính toán
* chi phí lưu trữ thấp
* phù hợp cho cache

Nhược điểm:

* không mô tả chính xác chữ cong
* không mô tả bubble nghiêng
* chứa nhiều khoảng trắng dư

---

### Usage

Bounding Box được khuyến nghị cho:

* UI
* Cache
* Index
* Crop
* Quick Selection

Không nên sử dụng Bounding Box cho:

* OCR chính xác cao
* Text Wrapping
* Bubble Reconstruction

---

## 15. Polygon

### Purpose

Polygon mô tả chính xác đường bao của Region.

Polygon là hình học chuẩn được khuyến nghị sử dụng trong CRAI.

---

### Design Goals

Polygon phải:

* mô tả sát biên văn bản
* hỗ trợ nhiều đỉnh
* hỗ trợ hình dạng bất quy tắc
* hỗ trợ văn bản cong
* hỗ trợ bong bóng thoại méo

---

### Structure

```text
Polygon

points[]

Point

x

y
```

Ví dụ:

```text
P1(x1,y1)

P2(x2,y2)

P3(x3,y3)

P4(x4,y4)
```

Số lượng điểm không bị giới hạn.

---

### Ordering

Các điểm của Polygon phải được lưu theo cùng một chiều (clockwise hoặc counter-clockwise) và nhất quán trong toàn bộ hệ thống.

Việc thay đổi thứ tự điểm có thể làm sai kết quả khi:

* tính diện tích
* clipping
* rendering
* collision detection

---

### Advantages

Polygon:

* chính xác hơn Bounding Box
* hỗ trợ OCR tốt hơn
* giảm nền dư
* hỗ trợ bubble méo
* hỗ trợ text nghiêng
* hỗ trợ manga dọc

---

### Trade-offs

Polygon có:

* chi phí lưu trữ lớn hơn
* nhiều phép tính hơn
* khó debug hơn Bounding Box

Do đó Bounding Box và Polygon nên cùng tồn tại trong Detection Result.

---

## 16. Segmentation Mask

### Purpose

Segmentation Mask biểu diễn chính xác từng pixel thuộc về Region.

Mask là biểu diễn có độ chính xác cao nhất.

---

### Characteristics

Mask cho phép:

* tách nền chính xác
* OCR chất lượng cao
* bubble reconstruction
* text removal
* inpainting
* AI editing

---

### Data Model

Khái niệm:

```text
Mask

width

height

binary data
```

Mỗi pixel chỉ thuộc một trong hai trạng thái:

* thuộc Region
* không thuộc Region

---

### Storage

Do kích thước lớn, Mask không nên luôn được lưu trong Detection Result.

Có thể:

* sinh theo yêu cầu
* cache riêng
* lưu dưới dạng nén

---

### Usage

Mask phù hợp cho:

* Accurate OCR
* Image Editing
* AI Inpainting
* Bubble Replacement

Không phù hợp cho:

* Quick Preview
* Simple UI
* Lightweight Cache

---

## 17. Rotation

### Purpose

Không phải mọi Region đều song song với ảnh.

Detection phải hỗ trợ xác định góc xoay của từng Region.

---

### Rotation Angle

Rotation được biểu diễn theo đơn vị:

```text
Degree
```

hoặc

```text
Radian
```

Toàn bộ hệ thống nên thống nhất một chuẩn duy nhất.

---

### Use Cases

Rotation xuất hiện trong:

* manga
* manhwa
* sound effects
* poster
* quảng cáo
* camera capture
* ảnh chụp màn hình bị nghiêng

---

### Design Requirements

Rotation phải:

* độc lập với OCR Provider
* không làm thay đổi Coordinate System
* hỗ trợ Deskew
* hỗ trợ Render

---

### Notes

Rotation chỉ mô tả hình học của Region.

Rotation không làm thay đổi ảnh nguồn.

---

## 18. Geometry Transformation

### Purpose

Geometry Transformation mô tả mọi phép biến đổi hình học giữa các phiên bản ảnh.

Detection chỉ sử dụng kết quả của Transformation, không trực tiếp thực hiện các phép biến đổi.

---

### Supported Transformations

Geometry Transformation có thể bao gồm:

* Resize
* Crop
* Rotate
* Flip
* Perspective Correction
* Padding
* Scale
* Translation

---

### Coordinate Mapping

Mọi phép biến đổi phải cho phép ánh xạ hai chiều:

```text
Original Image

⇄

Normalized Image
```

Điều này đảm bảo mọi Region luôn có thể quy đổi ngược về ảnh gốc.

---

### Transformation Chain

Trong quá trình xử lý, một Region có thể đi qua nhiều phép biến đổi liên tiếp.

Ví dụ:

```text
Original

↓

Resize

↓

Deskew

↓

Crop

↓

Detection
```

Hệ thống phải lưu đủ thông tin để truy vết chuỗi biến đổi nếu cần.

---

### Geometry Invariants

Geometry Transformation phải đảm bảo:

* không làm mất Region hợp lệ
* không thay đổi thứ tự Region
* có thể đảo ngược khi đủ dữ liệu
* không phụ thuộc Detection Engine
* không phụ thuộc Recognition Engine

Geometry là lớp nền tảng cho toàn bộ OCR Pipeline. Mọi module phía sau (Recognition, Reading Order, Translation và Presentation) đều phải sử dụng cùng một mô hình hình học nhằm đảm bảo tính nhất quán của dữ liệu.

## 19. Region Types

### Purpose

Region Type mô tả **ý nghĩa của một vùng văn bản** thay vì chỉ mô tả hình học của vùng đó.

Việc phân loại Region ngay từ giai đoạn Detection giúp các module phía sau giảm đáng kể lượng suy luận cần thực hiện.

---

### Classification Principles

Mỗi Region:

* chỉ có một Region Type chính
* có thể chứa metadata bổ sung
* phải được phân loại độc lập với OCR
* không phụ thuộc Translation
* không phụ thuộc Presentation

Nếu không đủ thông tin để xác định loại, Detection phải sử dụng `Unknown Region`.

---

### Built-in Region Types

Phiên bản đầu tiên của CRAI định nghĩa các loại Region sau:

* Speech Bubble
* Narration Box
* Sound Effects (SFX)
* Background Text
* UI Text
* Watermark
* Advertisement
* Unknown Region

Trong tương lai có thể bổ sung thêm mà không phá vỡ Detection Contract.

---

### Region Type Invariants

Region Type phải đảm bảo:

* ổn định trong cùng một Detection Result
* không thay đổi sau Recognition
* không phụ thuộc OCR Provider
* không thay đổi bởi Translation

---

## 20. Speech Bubble

### Purpose

Speech Bubble đại diện cho vùng chứa lời thoại của nhân vật.

Đây là Region quan trọng nhất trong truyện tranh và luôn được ưu tiên xử lý.

---

### Characteristics

Speech Bubble thường có:

* viền khép kín
* nền sáng
* chứa một hoặc nhiều đoạn văn
* có đuôi hướng về nhân vật

Tuy nhiên hệ thống không được giả định tất cả Bubble đều có đầy đủ các đặc điểm trên.

---

### Detection Strategy

Detection nên ưu tiên:

* xác định toàn bộ vùng Bubble
* xác định vùng Text bên trong
* tách Bubble độc lập với nền ảnh

Detection không cần xác định nhân vật đang nói.

---

### Common Variations

Speech Bubble có thể xuất hiện dưới dạng:

* hình elip
* hình tròn
* hình chữ nhật
* không viền
* nhiều ngăn
* nhiều đoạn văn
* bubble nối nhau
* bubble bị cắt bởi mép trang

---

### Detection Challenges

Các trường hợp khó:

* Bubble chồng nhau
* Bubble trong suốt
* Bubble bị che khuất
* Bubble méo
* Bubble có nền tối

---

### Classification Rules

Speech Bubble được ưu tiên hơn:

* Background Text
* UI Text

Nếu chưa đủ bằng chứng, Region phải được đánh dấu `Unknown Region`.

---

## 21. Narration Box

### Purpose

Narration Box biểu diễn lời dẫn chuyện hoặc mô tả bối cảnh.

---

### Characteristics

Thông thường:

* có khung chữ nhật
* nền sáng hoặc tối
* không có đuôi
* không gắn với nhân vật cụ thể

---

### Detection Strategy

Detection cần nhận diện toàn bộ khung thay vì chỉ nhận diện từng dòng chữ.

---

### Classification Rules

Narration Box không được phân loại thành Speech Bubble chỉ vì có hình chữ nhật bao quanh.

---

## 22. Sound Effects (SFX)

### Purpose

Sound Effects đại diện cho chữ biểu diễn âm thanh trong truyện.

Ví dụ:

* BOOM
* BAM
* ゴゴゴ
* ドン
* 啪
* 轰

---

### Characteristics

SFX thường:

* có font lớn
* nghiêng
* xoay
* biến dạng
* hòa vào hình minh họa

---

### Detection Strategy

Detection cần ưu tiên phát hiện hình học chính xác thay vì cố xác định ý nghĩa.

---

### Detection Challenges

Khó khăn phổ biến:

* chữ cong
* chữ nhiều màu
* chữ bị biến dạng
* chữ hòa nền

---

### Classification Rules

SFX không được gộp vào Speech Bubble chỉ vì nằm gần Bubble.

---

## 23. Background Text

### Purpose

Background Text là văn bản xuất hiện trong bối cảnh của hình ảnh.

Ví dụ:

* biển hiệu
* bảng tên
* cửa hàng
* chỉ dẫn
* áp phích

---

### Characteristics

Background Text:

* không thuộc lời thoại
* không thuộc narration
* thường là một phần của hình minh họa

---

### Detection Strategy

Detection cần nhận diện nhưng không cần ưu tiên cao hơn Speech Bubble.

---

### Classification Rules

Background Text có thể được Presentation bỏ qua tùy cấu hình người dùng.

---

## 24. UI Text

### Purpose

UI Text là văn bản thuộc giao diện ứng dụng hoặc website.

Ví dụ:

* Login
* Share
* Menu
* Next Chapter
* Back

---

### Characteristics

UI Text:

* thường nằm sát mép màn hình
* có bố cục cố định
* không thuộc nội dung truyện

---

### Detection Strategy

Detection nên nhận diện riêng để hệ thống có thể lọc hoặc dịch tùy chế độ.

---

### Classification Rules

UI Text không được gộp với Background Text nếu có đủ bằng chứng nhận biết giao diện.

---

## 25. Watermark

### Purpose

Watermark là văn bản biểu thị nguồn phát hành hoặc bản quyền.

---

### Characteristics

Ví dụ:

* MangaDex
* NetTruyen
* Bilibili
* Copyright

Watermark thường xuất hiện lặp lại trên nhiều trang.

---

### Detection Strategy

Detection chỉ cần nhận diện vùng chứa Watermark.

Không cần xác minh tính hợp lệ của nội dung.

---

### Classification Rules

Watermark mặc định có thể bị Translation và Presentation bỏ qua.

---

## 26. Advertisement

### Purpose

Advertisement đại diện cho các nội dung quảng cáo không thuộc truyện.

---

### Characteristics

Có thể bao gồm:

* banner
* popup
* khuyến mại
* hình quảng cáo

---

### Detection Strategy

Detection nên tách Advertisement khỏi nội dung truyện càng sớm càng tốt.

---

### Classification Rules

Advertisement không được tham gia Reading Order mặc định.

---

## 27. Unknown Region

### Purpose

Unknown Region là loại dự phòng khi Detection không đủ bằng chứng để phân loại.

---

### Fallback Strategy

Thay vì phân loại sai, hệ thống phải:

* giữ nguyên Region
* gán Unknown Region
* chuyển tiếp cho Recognition

---

### Classification Rules

Unknown Region luôn là lựa chọn cuối cùng.

Không được ép Region vào loại khác chỉ để tăng tỷ lệ phân loại.

---

## 28. Region Hierarchy

### Purpose

Một Region có thể bao gồm nhiều Region con.

Hierarchy giúp biểu diễn cấu trúc logic của trang truyện.

---

### Parent-Child Relationship

Ví dụ:

```text
Page
 ├── Speech Bubble
 │    ├── Text Region
 │    ├── Text Region
 │    └── Tail
 ├── Narration Box
 └── SFX
```

---

### Nested Regions

Detection phải cho phép Region lồng nhau khi cần thiết.

Ví dụ:

* Bubble chứa nhiều đoạn văn
* Narration chứa nhiều dòng
* UI chứa nhiều thành phần

---

### Hierarchy Rules

Một Region chỉ có:

* tối đa một Parent
* nhiều Child

Không được tạo vòng lặp trong cây Region.

---

## 29. Detection Rules

### Rule Evaluation Order

Detection Rules phải được đánh giá theo thứ tự xác định trước nhằm đảm bảo tính nhất quán.

---

### Merge Rules

Hai Region có thể được gộp khi:

* cùng loại
* giao nhau vượt ngưỡng
* thuộc cùng đối tượng

---

### Split Rules

Một Region có thể bị tách khi:

* chứa nhiều vùng độc lập
* khoảng cách lớn
* khác hướng đọc

---

### Reject Rules

Region phải bị loại bỏ khi:

* diện tích bằng 0
* ngoài ảnh
* confidence quá thấp
* không hợp lệ

---

### Overlap Rules

Khi hai Region giao nhau:

* ưu tiên Region có confidence cao hơn
* hoặc giữ cả hai nếu mang ý nghĩa khác nhau

---

### Priority Rules

Thứ tự ưu tiên mặc định:

1. Speech Bubble
2. Narration Box
3. SFX
4. Background Text
5. UI Text
6. Watermark
7. Advertisement
8. Unknown Region

Thứ tự này có thể thay đổi theo Detection Profile.

---

## 30. Classification Confidence

### Detection Confidence

Đánh giá khả năng Region thực sự chứa văn bản.

---

### Classification Confidence

Đánh giá mức độ tin cậy của Region Type.

Detection Confidence và Classification Confidence là hai giá trị độc lập.

---

### Confidence Levels

Khuyến nghị chia thành:

* Very High
* High
* Medium
* Low
* Very Low

Ngưỡng cụ thể được cấu hình theo Detection Profile.

---

### Confidence Propagation

Confidence của Detection không được tự động kế thừa sang Recognition.

Mỗi giai đoạn trong OCR Pipeline phải tự đánh giá độ tin cậy của mình.

---

### Confidence Invariants

Classification Confidence phải:

* độc lập với OCR Provider
* ổn định với cùng đầu vào
* không bị thay đổi bởi Translation
* luôn gắn với đúng Region trong suốt vòng đời của Detection Result.

## 31. Reading Direction Hint

### Purpose

Reading Direction Hint mô tả hướng đọc được ước lượng của từng Region.

Detection chỉ cung cấp **gợi ý** về hướng đọc, không xác định thứ tự đọc cuối cùng của toàn trang.

Việc tính toán Reading Order thuộc tài liệu và module riêng.

---

### Supported Directions

Detection nên hỗ trợ các hướng đọc sau:

* Left to Right (LTR)
* Right to Left (RTL)
* Top to Bottom (TTB)
* Bottom to Top (BTT)
* Unknown

---

### Direction Estimation

Hướng đọc có thể được suy luận từ:

* hình dạng Region
* tỷ lệ chiều rộng và chiều cao
* bố cục ký tự
* OCR Provider metadata
* Detection Profile

Detection không được phụ thuộc hoàn toàn vào OCR Result để xác định Reading Direction.

---

### Direction Invariants

Reading Direction Hint:

* không thay đổi Geometry
* không thay đổi Region Type
* chỉ là metadata
* có thể được cập nhật ở bước Reading Order

---

## 32. Region Merge

### Purpose

Region Merge kết hợp nhiều Region thành một Region logic khi chúng đại diện cho cùng một thực thể.

---

### Merge Conditions

Hai hoặc nhiều Region có thể được gộp khi:

* cùng Region Type
* có khoảng cách nhỏ hơn ngưỡng cấu hình
* có hướng đọc tương thích
* có khả năng thuộc cùng đoạn văn

---

### Merge Restrictions

Không được Merge khi:

* khác Region Type
* khác hướng đọc rõ ràng
* thuộc các Bubble khác nhau
* vượt quá ngưỡng khoảng cách

---

### Merge Result

Sau khi Merge:

* Geometry phải được tính lại
* Bounding Box được cập nhật
* Polygon được tái tạo nếu cần
* Confidence được tính lại
* Parent-Child Relationship được bảo toàn

---

## 33. Region Split

### Purpose

Region Split chia một Region thành nhiều Region nhỏ hơn khi Detection xác định rằng Region hiện tại chứa nhiều thực thể độc lập.

---

### Split Conditions

Region nên được tách khi:

* chứa nhiều khối văn bản độc lập
* khoảng trắng lớn chia Region
* khác hướng đọc
* khác loại nội dung

---

### Split Restrictions

Không được Split nếu:

* làm mất liên kết của đoạn văn
* tạo Region quá nhỏ
* làm giảm đáng kể độ tin cậy

---

### Split Result

Mỗi Region mới phải:

* có ID riêng
* có Geometry riêng
* có Confidence riêng
* vẫn tham chiếu đến cùng Detection Result

---

## 34. Region Validation

### Purpose

Region Validation kiểm tra tính hợp lệ của Detection Result trước khi chuyển sang Recognition.

---

### Validation Rules

Region phải được kiểm tra:

* Geometry hợp lệ
* Bounding Box hợp lệ
* Polygon không tự cắt
* Confidence trong khoảng hợp lệ
* Region Type hợp lệ

---

### Invalid Region

Region được coi là không hợp lệ nếu:

* diện tích bằng 0
* nằm ngoài ảnh
* Polygon lỗi
* tọa độ không hợp lệ
* dữ liệu bị thiếu

---

### Validation Result

Sau Validation:

* Region hợp lệ được giữ lại
* Region không hợp lệ bị loại hoặc đánh dấu lỗi tùy Detection Profile

---

## 35. Incremental Detection

### Purpose

Incremental Detection chỉ xử lý phần ảnh thay đổi thay vì thực hiện Detection trên toàn bộ ảnh.

Điều này đặc biệt quan trọng trong chế độ đọc thời gian thực.

---

### Supported Scenarios

Incremental Detection phù hợp với:

* cuộn trang
* phóng to
* thu nhỏ
* cập nhật một phần màn hình
* video frame

---

### Design Principles

Incremental Detection phải:

* tái sử dụng Region cũ
* chỉ Detect vùng mới
* giữ nguyên ID của Region không đổi
* giảm chi phí tính toán

---

## 36. Detection Cache

### Purpose

Detection Cache lưu kết quả Detection để tái sử dụng giữa nhiều lần xử lý.

---

### Cache Key

Cache nên dựa trên:

* Image ID
* Image Version
* Detection Profile
* Detection Provider
* Configuration Version

---

### Cache Content

Cache có thể lưu:

* Region List
* Geometry
* Confidence
* Metadata
* Statistics

Không nên lưu dữ liệu tạm thời của Runtime.

---

### Cache Invalidation

Cache phải bị hủy khi:

* ảnh thay đổi
* profile thay đổi
* provider thay đổi
* cấu hình thay đổi

---

## 37. Detection Events

### Purpose

Detection phát sinh các sự kiện phục vụ Runtime và Event Bus.

---

### Recommended Events

Ví dụ:

```text
DetectionRequested

DetectionStarted

RegionDetected

RegionMerged

RegionSplit

RegionRejected

DetectionCompleted

DetectionFailed

DetectionCancelled
```

Tên sự kiện cụ thể phải tuân theo tài liệu `EVENT_CONVENTION.md`.

---

### Event Principles

Mỗi Event phải:

* bất biến (Immutable)
* có Timestamp
* có Correlation ID
* có Detection ID

---

## 38. Detection State Machine

### Purpose

State Machine mô tả trạng thái của Detection Job.

---

### States

```text
Created

↓

Queued

↓

Running

↓

Validating

↓

Completed
```

Ngoài luồng chính:

```text
Running

↓

Failed
```

hoặc

```text
Running

↓

Cancelled
```

---

### State Transition Rules

* Không được bỏ qua trạng thái.
* Không được quay ngược từ Completed về Running.
* Failed và Cancelled là trạng thái kết thúc.

---

## 39. Performance Considerations

### Objectives

Detection phải đáp ứng:

* độ trễ thấp
* khả năng mở rộng
* khả năng xử lý song song
* hiệu quả bộ nhớ

---

### Optimization Strategies

Khuyến nghị:

* xử lý đa luồng
* cache kết quả
* incremental detection
* giới hạn vùng xử lý (ROI)
* sử dụng nhiều Detection Provider khi cần

---

### Scalability

Detection phải hỗ trợ:

* một ảnh đơn
* nhiều trang
* xử lý hàng loạt
* xử lý song song nhiều Session

---

## 40. Detection Architecture Invariants

### Purpose

Architecture Invariants xác định các quy tắc bất biến của Detection.

Mọi implementation đều phải tuân thủ.

---

### Invariants

Detection phải đảm bảo:

* không thay đổi ảnh nguồn
* không thực hiện OCR
* không thực hiện Translation
* không phụ thuộc Presentation
* Geometry luôn nhất quán
* Region ID ổn định trong Detection Result
* Detection Result có thể tuần tự hóa (Serializable)
* Detection Result có thể Cache
* Detection Provider có thể thay thế mà không đổi Contract
* Detection luôn là bước độc lập trong OCR Pipeline

---

### Future Extensions

Kiến trúc hiện tại phải cho phép mở rộng trong tương lai mà không phá vỡ Contract hiện có, bao gồm:

* AI-based Semantic Detection
* Multi-layer Region Detection
* 3D Geometry
* Temporal Detection cho video
* Multi-page Context Detection
* Plugin Detection Provider
* Distributed Detection Runtime

