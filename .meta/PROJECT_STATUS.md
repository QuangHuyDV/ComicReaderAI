# CRAI — Project Status

**Updated:** 2026-08-03  
**Project key:** `CRAI`  
**Document role:** Current project entry point and architecture status summary  
**Approach:** Documentation First · Capability First · User Experience First · Provider Independent

> This document records the current architectural truth of CRAI and the history that led to it.  
> Detailed module and architecture documents remain the authoritative source for their own scope.

---

# 1. Executive Summary

## 1.1 Project Overview

CRAI is a desktop-first application that helps users read and translate foreign-language novels, comics, documents and on-screen content with minimal interruption to the reading experience.

The initial product focus is:

- source languages: Simplified Chinese, Traditional Chinese and English
- initial target language: Vietnamese
- direct structured-text acquisition whenever available
- OCR only when structured text cannot be obtained reliably
- Side Panel as the primary MVP presentation mode
- Overlay supported through a separate architectural boundary
- local-first and privacy-aware processing where practical
- replaceable OCR, translation and platform implementations

CRAI is not designed as a single OCR-to-translation script. It is designed as a modular reading system that separates content acquisition, recognition, text preparation, translation, presentation, session control and persistence responsibilities.

---

## 1.2 Product and Architecture Principles

The project currently follows these principles:

- **Documentation First** — architecture and contracts are clarified before implementation.
- **Capability First** — system capabilities are analyzed before deciding module boundaries.
- **User Experience First** — technical choices must minimize interruption while reading.
- **Provider Independent** — business architecture does not depend on a specific OCR or translation provider.
- **Explicit Ownership** — every state, model, command, event and persisted record has a defined owner.
- **Serializable Boundaries** — public contracts must be serializable and implementation-neutral.
- **Immutable Results** — cross-module results and artifacts are treated as immutable values or references.
- **Current Revision Authority** — only the current valid content revision may commit user-visible results.
- **Privacy by Default** — screenshots, clipboard content, OCR text and translated text are not logged or persisted by default.
- **Reversible Design** — implementation choices should remain replaceable until real constraints justify commitment.

The analysis order remains:

```text
Product Goal
    ↓
Capabilities
    ↓
User Journeys
    ↓
Use Cases and Workflows
    ↓
State, Event and Data Models
    ↓
Business Modules
    ↓
Runtime and Infrastructure
    ↓
Implementation
```

---

## 1.3 Current Architecture Status

| Architecture Area | Status | Current Meaning |
|---|---|---|
| Project Foundation | ✅ Complete | AI, project and module rules are established. |
| Product Analysis | ✅ Complete | Product goal, scope and primary user journeys are defined. |
| Capability Analysis | ✅ Complete | Core capabilities and their boundaries are documented. |
| Core Architecture | ✅ Complete | State, event, dependency and data-flow foundations are defined. |
| Runtime Architecture | ✅ Runtime v2 synchronized | Runtime documents now use the same WorkItem/Attempt, authority, Candidate Artifact, ownership, publication, Lease, retention and disposal model. |
| Business Module Architecture | ✅ Complete | Core business modules (including Translation, Presentation and Provider Management) have completed the standard document set. Cross-module Runtime v2 terminology review remains an ongoing maintenance task. |
| Detailed Recognition/OCR Architecture | 🟡 Next review area | The detailed `doc/01-architecture/ocr/` documents exist but have not yet been reconciled with the newly completed Recognition module contracts. |
| Infrastructure Architecture | 🟡 In Progress | Configuration Infrastructure module document set has been completed and Infrastructure documentation has started. |
| Technology Selection | ⏳ Not Started | Frameworks, languages, providers and process topology have not been finalized. |
| Implementation | ❌ Not Started | No production implementation has begun. |

The current business-module set is:

```text
Reading / Reading Session
Capture
Recognition
Text Processing
Translation
Presentation
Storage
Preferences
Diagnostics
UI Adapter
```

Capabilities such as OCR, preprocessing, region detection, layout interpretation, reading-order resolution, normalization, context preparation, rendering and persistence mechanics remain internal architectural responsibilities. They are not automatically standalone business modules.

Runtime v2 is now the shared execution language across the project:

```text
Session
    ↓
Revision
    ↓
BusinessExecutionPlan
    ↓
WorkItem
    ↓
Attempt
    ↓
Candidate Artifact
    ↓
Authority Validation
    ↓
Ownership Transfer
    ↓
Artifact Publication
    ↓
Presentation Commit
```

---

## 1.4 Current Module Progress

| Module | Status | Notes |
|---|---|---|
| Secret Management Infrastructure | ✅ Complete | Standard document set (README, MODULE, CONTRACT, STATES, EVENTS, ERRORS) completed. |
| Event Bus Infrastructure | ✅ Complete | Standard document set completed and aligned with Runtime execution model. |
| Logging Infrastructure | ✅ Complete | Structured logging architecture completed. |
| Telemetry Infrastructure | ✅ Complete | Metrics, tracing and observability contracts completed. |
| Scheduler Infrastructure | ✅ Complete | Scheduling lifecycle, retry, timeout, cancellation and orchestration documented. |


| Module | Status | Notes |
|---|---|---|
| Capture | ✅ Documented | Module overview, contract, events, states and errors exist; later Runtime v2 terminology review may still be needed. |
| Recognition | ✅ Runtime v2 synchronized | `README.md`, `MODULE.md`, `CONTRACT.md`, `STATES.md`, `EVENTS.md` and `ERRORS.md` now share the Candidate Artifact and Runtime authority model. |
| Text Processing | ✅ Documented | Produces normalized and structured source data for Translation; cross-check against the new Recognition Artifact boundary remains a later synchronization task. |
| Translation | ✅ Complete | Standard document set completed and synchronized. |
| Presentation | ✅ Complete | Standard document set completed. |
| Reading / Reading Session | ✅ Documented | Reading/session responsibilities and lifecycle have been designed. |
| Storage | ✅ Complete | Storage was consolidated as a Persistence Capability with README, contracts, models, migration, states, events and errors. |
| Provider Management | ✅ Complete | Standard document set (README, MODULE, CONTRACT, STATES, EVENTS, ERRORS) completed. |
| Configuration Infrastructure | ✅ Complete | MODULE, CONTRACT, STATES, EVENTS, ERRORS and README completed. |
| Preferences | ✅ Documented | Module document set exists. |
| Diagnostics | ✅ Documented | Module document set exists. |
| UI Adapter | ✅ Documented | Module document set exists and remains separate from Presentation semantics. |

Storage is treated as a **Persistence Capability**, not as the owner of business data and not as a generic Repository, Cache or Backend module.

The owning business module defines the meaning and lifecycle of its data. Storage provides implementation-independent persistence mechanisms such as versioned entries, metadata, snapshots, retention instructions, archival records, recovery points and schema-evolution support.

The following concepts remain separate:

```text
Runtime Artifact Store
Runtime Cache / Retention
Persistent Storage
```

Recognition is now defined as an image-to-structured-source module:

```text
Image Artifact
    ↓
Recognition Attempt
    ↓
Candidate Recognition Artifact
    ↓
Runtime Authority Validation
    ↓
Published Recognition Artifact
    ↓
Text Processing
```

Recognition does not own WorkItem/Attempt lifecycle, retry, cancellation authority, provider lifecycle or Artifact publication.

---

## 1.5 Current Focus

The immediate focus is completion of the Infrastructure Architecture before technology selection.

Current priorities are:

1. complete `03-infrastructure/resource-manager/`
2. continue the remaining Infrastructure modules
3. synchronize architecture terminology across modules
4. revisit OCR architecture after Infrastructure stabilization
5. update PROJECT_STATUS after each completed document group


## 1.6 Architecture Snapshot

The current high-level product flow is:

```text
Reading Session
    ↓
Capture or Structured Text Acquisition
    ↓
Recognition when image interpretation is required
    ↓
Text Processing
    ↓
Translation
    ↓
Presentation
    ↓
UI Adapter
```

The current Runtime execution flow is:

```text
Stable Content
    ↓
Revision
    ↓
BusinessExecutionPlan
    ↓
WorkItem
    ↓
Attempt
    ↓
Candidate Artifact
    ↓
Authority Validation
    ↓
Ownership Transfer
    ↓
Artifact Publication
    ↓
Presentation Commit
```

Supporting Runtime responsibilities:

```text
Runtime Control
├── revision and execution authority
├── WorkItem and Attempt lifecycle
├── scheduling and bounded queues
├── cancellation and retry coordination
├── Candidate acceptance and publication coordination
└── terminal outcome acceptance

Resource Manager / Artifact Store
├── Resource registration
├── ownership transfer
├── Resource Lease
├── retention
├── logical disposal
└── physical disposal

Runtime Observability
├── metrics
├── traces
├── structured logs
├── runtime events
└── diagnostic snapshots

Storage
├── implementation-independent persistence
├── versioned records and snapshots
├── retention and archival instructions
├── recovery
└── schema evolution
```

The architecture preserves two main input paths:

```text
Text Flow
    → prefer structured text and bypass Recognition where possible

Image Flow
    → capture image, recognize structured source content,
      process text, translate and present
```

Business modules own semantic meaning.

Runtime owns execution orchestration and authority.

Artifact Store owns accepted shared payload.

Storage owns persistence mechanisms.

Pipeline stages and module events never independently decide the next business stage.

---

## 1.7 How to Resume

For Infrastructure work, continue from the current unfinished module under `03-infrastructure/`.
 the Project

For a new AI session or a new contributor, use this reading order:

1. read this Executive Summary
2. read the current architecture snapshot and module progress sections
3. read the specific module or architecture document relevant to the assigned task
4. read Development History only when the reason behind a decision is needed
5. verify the current-focus and next-task sections before proposing new work
6. for Recognition work, read `doc/02-modules/recognition/README.md` before the detailed `doc/01-architecture/ocr/` documents

Do not rely on an older progress entry when it conflicts with the current summary or the latest detailed module documents.

When updating this file:

- keep the Executive Summary limited to current truth
- move chronological changes into Development History
- avoid duplicating detailed contracts already owned by module documents
- update status, current focus and next task together
- mark uncertainty explicitly instead of guessing

---

# 0. Development History

## 0.1 Mục đích

Phần này ghi lại quá trình hình thành kiến trúc của CRAI theo đúng thứ tự các cuộc thảo luận.

Khác với các chương phía sau chỉ mô tả kết quả cuối cùng, Development History tập trung vào:

- vấn đề ban đầu của dự án
- quá trình phân tích
- các quyết định đã được thống nhất
- các quyết định đã thay đổi
- lý do thay đổi
- trạng thái hiện tại

Mục tiêu là giúp AI hoặc lập trình viên mới hiểu **vì sao kiến trúc hiện tại được thiết kế như vậy**, thay vì chỉ biết kết quả cuối cùng.

Development History không thay thế các tài liệu kiến trúc chi tiết như:

- Capability Map
- User Journey
- State Machine
- Event Bus
- Data Flow
- Runtime Architecture
- Module Architecture

mà đóng vai trò là bản ghi lại quá trình phát triển của dự án.

---

# 0.2 Giai đoạn khởi đầu

CRAI bắt đầu từ một nhu cầu rất cụ thể.

Mục tiêu ban đầu là xây dựng một công cụ hỗ trợ đọc truyện tranh và tiểu thuyết nước ngoài mà không làm gián đoạn trải nghiệm đọc.

Trong quá trình phân tích, nhiều câu hỏi được đặt ra:

- nên làm Desktop App hay Web?
- nên làm Browser Extension hay ứng dụng độc lập?
- nên OCR toàn bộ màn hình hay chỉ OCR vùng cần thiết?
- nên dịch theo ảnh hay theo văn bản?
- có cần lưu dữ liệu hay không?
- có cần plugin không?
- overlay hay side panel?
- có nên phụ thuộc OCR Provider cụ thể không?

Sau nhiều lần phân tích, dự án thống nhất định hướng:

- Desktop First
- Documentation First
- Capability First
- UX First
- Provider Independent

Đây trở thành nền tảng cho toàn bộ các quyết định về sau.

---

# 0.3 Thiết lập nguyên tắc phát triển

Trước khi thiết kế kiến trúc, dự án thống nhất phải xây dựng bộ tài liệu nền.

Các tài liệu đầu tiên gồm:

- AI_BOOT
- PROJECT_RULE
- MODULES_RULE
- MODULES

Các tài liệu này không mô tả business logic mà định nghĩa:

- cách AI tiếp tục dự án
- phạm vi AI được phép sửa
- nguyên tắc tổ chức source code
- nguyên tắc thiết kế module
- quy tắc dependency
- quy tắc thay đổi kiến trúc

Từ thời điểm này, mọi quyết định kiến trúc đều phải tuân theo các nguyên tắc đã thống nhất.

---

# 0.4 Phân tích sản phẩm

Sau khi có bộ quy tắc nền, dự án chuyển sang phân tích sản phẩm.

Thay vì thiết kế module ngay, quá trình phân tích bắt đầu từ nhu cầu thực tế của người dùng.

Các nội dung đã phân tích gồm:

- Project Vision
- Capability Map
- User Journey

Trong giai đoạn này, nhiều quyết định quan trọng được thống nhất.

## Text Flow và Image Flow

Ban đầu ý tưởng chỉ là OCR rồi dịch.

Sau khi phân tích nhận thấy:

nếu nguồn đã có text thì OCR chỉ làm giảm chất lượng.

Do đó hệ thống được chia thành:

Text Flow

và

Image Flow

độc lập.

Text Flow luôn được ưu tiên trước.

OCR chỉ sử dụng khi không còn khả năng lấy văn bản có cấu trúc.

---

## Side Panel trước Overlay

Overlay mang lại trải nghiệm đẹp hơn.

Tuy nhiên:

- khó xử lý DPI
- khó mapping coordinate
- dễ bị capture lại
- phụ thuộc nền tảng

Do đó MVP ưu tiên Side Panel.

Overlay được thiết kế riêng để phát triển sau.

---

## Không phụ thuộc Provider

Ban đầu từng cân nhắc thiết kế quanh:

- PaddleOCR
- RapidOCR
- Google Translate

Sau đó thống nhất:

Provider chỉ là implementation.

Business Architecture không được phụ thuộc bất kỳ provider nào.

---

# 0.5 Phân tích Capability

Sau khi hiểu sản phẩm, dự án bắt đầu xác định hệ thống cần làm được gì.

Capability Map được xây dựng trước khi xuất hiện khái niệm Module.

Điều này dẫn tới một quyết định quan trọng:

Capability

không đồng nghĩa

Module.

Ví dụ:

OCR

không phải module.

OCR chỉ là một capability.

Reading Order

không phải module.

Segmentation

không phải module.

Context Preparation

không phải module.

Những capability này về sau được gom lại thành các Business Module.

---

# 0.6 Thiết kế Core Architecture

Sau Capability, dự án tiếp tục thiết kế phần lõi.

Các tài liệu lần lượt được tạo:

State Machine

↓

Event Bus

↓

Module Dependency

↓

Data Flow

Mục tiêu là xác định:

- hệ thống hoạt động thế nào
- dữ liệu đi đâu
- ai sở hữu dữ liệu
- module giao tiếp ra sao
- workflow được điều phối như thế nào

Từ đây hình thành toàn bộ kiến trúc nền của CRAI.

---

# 0.7 Thiết kế Runtime

Sau khi hoàn thiện kiến trúc tĩnh, dự án bắt đầu phân tích runtime.

Đây là giai đoạn lớn nhất của quá trình thiết kế.

Ban đầu Runtime chỉ dự kiến gồm:

- Scheduler
- Queue

Sau nhiều lần phân tích, Runtime dần mở rộng thành:

- Pipeline Runtime
- Work Queue
- Scheduler
- Cancellation
- Retry
- Cache Policy
- Memory Model
- Threading Model
- Resource Lifecycle
- Performance
- Error Model
- Runtime Observability

Trong quá trình này cũng xuất hiện nhiều khái niệm mới như:

- Revision
- WorkItem
- Attempt
- Artifact Store
- Runtime Control

Đây đều là kết quả của quá trình phân tích chứ không có ngay từ đầu.

---

# 0.8 Thay đổi lớn về Module Architecture

Ban đầu dự án dự định xây dựng module gần sát capability.

Ví dụ:

- OCR Module
- Segmentation Module
- Layout Module
- Context Module

Sau nhiều lần phân tích nhận thấy cách chia này làm dependency phức tạp.

Do đó toàn bộ được tổ chức lại thành Business Module.

Các module hiện tại gồm:

- Reading
- Capture
- Recognition
- Text Processing
- Translation
- Presentation
- Storage

Đây là một trong những thay đổi kiến trúc lớn nhất của dự án.

---

## Recognition

Recognition không còn chỉ là OCR.

Recognition trở thành module chịu trách nhiệm:

- OCR
- Reading Order
- Region Mapping
- Layout Recognition
- Traceability

Nhưng không thực hiện Translation.

---

## Text Processing

Ban đầu Translation nhận trực tiếp OCR Result.

Sau nhiều lần phân tích nhận thấy:

OCR Result chưa phải dữ liệu phù hợp để dịch.

Do đó xuất hiện Text Processing.

Text Processing chịu trách nhiệm:

- normalization
- grouping
- reconstruction
- validation
- SourceDocument

Translation chỉ làm việc với SourceDocument.

Đây là thay đổi quan trọng nhất của pipeline business.

---

## Translation

Translation tiếp tục được tách khỏi Presentation.

Translation chỉ chịu trách nhiệm tạo Translation Result.

Mọi việc hiển thị:

- font
- layout
- overlay
- side panel

đều thuộc Presentation.

---

# 0.9 Tại thời điểm cập nhật tài liệu này

Đến thời điểm hiện tại:

đã hoàn thành gần như toàn bộ kiến trúc nền của CRAI.

Bao gồm:

- Project Foundation
- Product Analysis
- Capability Analysis
- User Journey
- Core Architecture
- Runtime Architecture
- Capture Module
- Recognition Module
- Text Processing Module
- Translation Module

Các phần đang tiếp tục gồm:

- Presentation Module
- Reading Module
- Storage Module

Sau khi hoàn thành Business Module Architecture, dự án mới chuyển sang lựa chọn công nghệ và bắt đầu implementation.

---

# 0.10 Định hướng tiếp theo

Trong các giai đoạn tiếp theo, trọng tâm của dự án sẽ không còn là thiết kế kiến trúc tổng thể.

Thay vào đó sẽ tập trung vào:

- hoàn thiện Business Module
- thống nhất public contract
- chuẩn hóa event
- lựa chọn technology stack
- process topology
- provider architecture
- implementation

Mọi thay đổi kiến trúc mới đều cần được ghi lại trong Development History để bảo đảm quá trình phát triển của dự án luôn có thể truy vết.

## 1. Mục tiêu dự án

CRAI là ứng dụng desktop hỗ trợ đọc và dịch nhanh tiểu thuyết, truyện tranh và nội dung đang hiển thị trên màn hình mà không làm gián đoạn trải nghiệm đọc.

Định hướng chính:

- ưu tiên nội dung tiếng Trung giản thể, tiếng Trung phồn thể và tiếng Anh
- ngôn ngữ đích ban đầu là tiếng Việt
- không hardcode cặp ngôn ngữ
- tách biệt Text Flow và Image Flow
- ưu tiên lấy text trực tiếp nếu nguồn đã có text
- chỉ dùng OCR khi không thể lấy text có cấu trúc
- hỗ trợ Side Panel trước, Overlay theo kiến trúc riêng
- giữ kiến trúc độc lập provider và có thể thay đổi implementation
- xây dựng tài liệu đủ rõ để AI hoặc lập trình viên khác có thể tiếp tục dự án

---

## 2. Triết lý kiến trúc

Các nguyên tắc đã thống nhất:

- **Documentation First**
- **Capability First**
- **User Experience First**
- **Provider Independent**
- **Reversible Design**
- **Privacy by Default**
- **Explicit Ownership**
- **Serializable Boundaries**
- **Module là kết quả của quá trình phân tích, không phải điểm bắt đầu**

Quy trình phân tích:

```text
Product Goal
    ↓
Capability
    ↓
User Journey
    ↓
Use Case
    ↓
Workflow
    ↓
State Machine
    ↓
Event & Data Flow
    ↓
Module
    ↓
Component
    ↓
Implementation
```

---

## 3. Tài liệu nền tảng

### `.meta/AI_BOOT.md`

Vai trò:

- cung cấp ngữ cảnh khởi động cho AI
- định nghĩa cách AI đọc và tiếp tục dự án
- hạn chế việc AI tự ý thay đổi kiến trúc
- giúp chuyển cuộc thảo luận sang đoạn chat mới

### `.meta/PROJECT_RULE.md`

Vai trò:

- quy tắc kỹ thuật cấp dự án
- nguyên tắc tổ chức source code
- tiêu chuẩn thay đổi kiến trúc
- giới hạn đối với dependency và implementation

### `.meta/MODULES_RULE.md`

Đã thống nhất:

- User Experience First
- tách Text Flow và Image Flow
- Capability không đồng nghĩa Provider
- Translation tách khỏi Presentation
- không OCR khi đã có text
- plugin chỉ dùng khi có nhu cầu thật
- dependency phải hướng vào abstraction
- module phải có ownership rõ ràng
- thiết kế phải có khả năng thay thế
- không tạo module chỉ vì một class dài
- không dùng các module mơ hồ như `Utils`, `Common`, `Manager`

### `.meta/MODULES.md`

Đóng vai trò:

- danh mục module của dự án
- liên kết module với capability
- ghi nhận trạng thái module
- là chỉ mục, không thay thế tài liệu dependency chi tiết

### `.meta/USER_JOURNEY.md`

Đã xây dựng luồng người dùng cho:

- lần đầu mở ứng dụng
- bắt đầu Text Reading
- bắt đầu Image/Comic Reading
- chọn cửa sổ hoặc vùng đọc
- Clipboard Translation
- Manual Image Translation
- tiếp tục session
- pause, resume và stop
- lỗi OCR
- lỗi dịch
- fallback
- thoát ứng dụng

Các runtime state cấp cao:

```text
Idle
Watching
Capturing
OCR
Segmenting
Translating
Rendering
Paused
Error
```

---

## 4. Hai pipeline chính

## 4.1 Text Flow

```text
Text Source
    ↓
Acquire Structured Text
    ↓
Normalize
    ↓
Fingerprint
    ↓
Cache Lookup
    ↓
Extract Structure
    ↓
Segment
    ↓
Translate
    ↓
Post-process
    ↓
Layout
    ↓
Reader / Side Panel
```

Đặc điểm:

- không OCR nếu có thể đọc text trực tiếp
- giữ paragraph và reading order
- chú trọng font, khoảng cách dòng và khả năng đọc
- có thể dùng DOM, Accessibility, Clipboard hoặc Document Reader
- presentation không gắn với translation provider

## 4.2 Image Flow

```text
Window / Screen Region / Image
    ↓
Detect Change
    ↓
Wait for Stability
    ↓
Capture
    ↓
Normalize Image
    ↓
Fingerprint
    ↓
Cache Lookup
    ↓
OCR
    ↓
Segment
    ↓
Translate
    ↓
Post-process
    ↓
Comic Layout
    ↓
Overlay / Side Panel
```

Đặc điểm:

- cần theo dõi thay đổi màn hình
- không capture full-resolution liên tục nếu chưa cần
- phải giữ coordinate transformation
- OCR block không đồng nghĩa translation segment
- overlay phải chống bị capture lại
- Image Replacement và Inpainting để sau MVP

---

## 5. Ngôn ngữ

Nguồn ưu tiên:

- Simplified Chinese
- Traditional Chinese
- English

Ngôn ngữ đích ban đầu:

- Vietnamese

Nguyên tắc:

- không hardcode một cặp ngôn ngữ duy nhất
- source language có thể là `AUTO`
- cho phép người dùng override language detection
- glossary và translation memory phải gắn language pair
- có thể mở rộng thêm ngôn ngữ mà không đổi pipeline tổng thể

---

## 6. Presentation

## 6.1 Text Presentation

Các mối quan tâm chính:

- font phù hợp
- paragraph
- heading
- line height
- text alignment
- reader width
- source/translation display
- giữ cấu trúc dễ đọc

## 6.2 Comic Presentation

Các mode:

- Side Panel
- Overlay
- Floating Window
- Image Replacement — Future

Overlay cần xử lý:

- DPI scaling
- window bounds
- OCR polygon mapping
- text overflow
- click-through
- overlay exclusion khỏi capture
- thay đổi vùng khi cửa sổ resize hoặc di chuyển

---

## 7. `docs/architecture/CAPABILITY_MAP.md`

Capability Map đã xác định các nhóm năng lực:

- Acquire Content
- Detect & Understand Content
- Extract Content
- Understand Text
- Translate
- Present Content
- Reading Session
- Knowledge
- Performance
- Extension
- Interaction

Một số capability cụ thể:

```text
Screen Watching
Window Selection
Region Selection
Stability Detection
Screen Capture
Text Extraction
OCR
Language Detection
Segmentation
Translation
Post-processing
Glossary
Translation Memory
Manual Correction
Cache
Session Management
Rendering
Overlay
Side Panel
Provider Abstraction
Diagnostics
```

Nguyên tắc:

```text
Capability ≠ Module
Capability ≠ Provider
```

Một capability có thể có nhiều implementation.

Ví dụ:

```text
OCR capability
    ├── PaddleOCR
    ├── RapidOCR
    ├── Windows OCR
    └── Cloud OCR
```

---

## 8. `docs/architecture/STATE_MACHINE.md`

Tài liệu State Machine đã được xây dựng theo ba cấp:

```text
Application State
Session State
Pipeline State
```

Pipeline state chính:

```text
WAITING_FOR_STABILITY
ACQUIRING_CONTENT
NORMALIZING
FINGERPRINTING
CACHE_LOOKUP
TEXT_EXTRACTING
OCR_PROCESSING
SEGMENTING
TRANSLATING
POST_PROCESSING
PREPARING_RENDER
RENDERING
COMPLETED
SKIPPED
CANCELLED
ERROR
```

Các nội dung đã định nghĩa:

- state transition
- transition guard
- retry
- fallback
- timeout
- cancellation
- stale result prevention
- multi-session
- concurrency
- recovery
- persistence boundary
- metrics
- logging

Quyết định quan trọng:

- mỗi session có pipeline độc lập
- mỗi pipeline có `pipelineId`
- mỗi task có thể có `taskId`
- nội dung thay đổi làm tăng `contentRevision`
- result cũ phải bị từ chối trước khi render
- không restore pipeline đang chạy sau khi application restart

---

## 9. `docs/architecture/EVENT_BUS.md`

Event Bus được chọn làm cơ chế phối hợp nội bộ giữa các module.

Mô hình đề xuất cho MVP:

```text
In-process
In-memory
Typed events
Async handlers
At-most-once delivery
One ordered queue per session
```

Không dùng trong MVP:

- Kafka
- RabbitMQ
- Redis Pub/Sub
- external broker

Các loại event:

```text
Command
Domain
Result
Progress
System
```

Quy ước tên:

```text
REQUESTED
STARTED
COMPLETED
FAILED
SKIPPED
CANCELLED
CHANGED
```

Event Envelope gồm:

```text
eventId
eventName
eventVersion
occurredAt
publishedAt
sourceModule
correlationId
causationId
applicationInstanceId
sessionId
pipelineId
taskId
contentRevision
priority
payload
metadata
```

Các nguyên tắc đã chốt:

- event là immutable notification
- event không trực tiếp thay đổi state
- state transition đi qua state owner
- subscriber failure phải được cô lập
- event payload không chứa secret
- event payload lớn phải dùng artifact reference
- ordering cần đảm bảo theo session hoặc pipeline
- progress event phải được throttle
- duplicate và stale event phải được kiểm tra
- Event Bus không thay Query Interface
- Event Bus không thay Pipeline Orchestrator

Pipeline Orchestrator là nơi quyết định stage tiếp theo.

Không dùng chuỗi tự động kiểu:

```text
OCR_COMPLETED
    ↓ Segmentation tự chạy
SEGMENTATION_COMPLETED
    ↓ Translation tự chạy
```

---

## 10. `docs/architecture/MODULE_DEPENDENCY.md`

Tài liệu này đã được nâng thành bản thiết kế source code tổng thể.

Các layer:

```text
Presentation
Application
Feature
Core
Infrastructure
Platform
Shared
Composition
```

Hướng dependency:

```text
Presentation
    ↓
Application
    ↓
Feature
    ↓
Core Abstractions
```

Infrastructure và Platform triển khai port/interface hướng vào Core hoặc Feature.

Composition Root là nơi duy nhất được phép wiring implementation cụ thể.

### Các nhóm module đã xác định

#### Presentation

```text
App Shell
Onboarding
Session Controls
Source Selector
Region Selector
Translation Panel
Overlay
Settings
Provider Settings
Glossary Editor
History
Diagnostics
```

#### Application

```text
Application Lifecycle
Session Orchestrator
Pipeline Orchestrator
Command Router
Retry Coordinator
Fallback Coordinator
Cancellation Coordinator
Resource Coordinator
Recovery Coordinator
```

#### Feature

```text
Reading Session
Source Management
Source Watching
Stability Detection
Content Acquisition
Content Normalization
Content Fingerprint
Cache Coordination
Text Extraction
OCR
Segmentation
Translation
Post-processing
Rendering
Translation Editing
Glossary
Translation Memory
Provider Management
Language Detection
Export
Diagnostics
```

#### Core

```text
Events
State Machine
Scheduler
Cancellation
Errors
Result
IDs
Clock
Logging
Metrics
Tracing
Configuration
Lifecycle
Concurrency
Geometry
Text Primitives
Contracts
```

#### Infrastructure

```text
Event Bus
Persistence
Cache
Artifact Store
OCR Adapters
Translation Adapters
HTTP
Credentials
Logging
Metrics
Configuration Store
Diagnostics
```

#### Platform

```text
Screen Capture
Window Management
Active Window
Accessibility
Clipboard
Global Hotkey
Filesystem
File Dialog
Notifications
Network Status
Power State
Process
Secure Storage
```

### Quy tắc dependency quan trọng

- UI không gọi trực tiếp OCR, Translation, Database hoặc OS API
- Feature không import implementation từ Infrastructure
- Infrastructure không import Application
- Platform không quyết định nghiệp vụ
- Core không phụ thuộc Feature
- module ngang hàng không tự điều phối pipeline
- module ngoài chỉ import qua public API
- deep import vào `internal/` bị cấm
- Service Locator và global business singleton bị cấm
- repository interface thuộc feature sở hữu dữ liệu
- mỗi state và dữ liệu có một owner
- mỗi command có một consumer nghiệp vụ chính
- mỗi event có publisher owner rõ ràng
- public contract phải serializable
- module mới phải khai báo responsibility, ownership và dependency

### Ownership quan trọng

```text
Application state
    → Application Lifecycle

Session state
    → Session Orchestrator / Reading Session

Pipeline state
    → Pipeline Orchestrator

Current contentRevision
    → Session Orchestrator

Provider registry
    → Provider Management

Captured content
    → Artifact Store

Glossary
    → Glossary Feature

Translation Memory
    → Translation Memory Feature

UI local state
    → Presentation module tương ứng
```

---

## 11. `docs/architecture/DATA_FLOW.md`

Data Flow Architecture đã mô tả dữ liệu từ source đến presentation.

Các nguyên tắc cốt lõi:

- mỗi loại dữ liệu có một owner
- dữ liệu lớn truyền qua `ArtifactRef`
- public data phải serializable
- dữ liệu cần version hoặc revision phù hợp
- không truyền mutable object xuyên module
- user content không xuất hiện trong log mặc định
- artifact phải có lifecycle và cleanup policy
- stale result phải được kiểm tra trước mọi side effect

### Identity chính

```text
applicationInstanceId
sessionId
pipelineId
taskId
eventId
correlationId
causationId
artifactId
sourceId
segmentId
providerId
```

### Version chính

```text
contentRevision
regionVersion
sessionConfigurationVersion
glossaryVersion
translationProfileVersion
ocrProfileVersion
renderProfileVersion
```

### Artifact type

```text
SOURCE_SNAPSHOT
RAW_TEXT
RAW_IMAGE
NORMALIZED_TEXT
NORMALIZED_IMAGE
CONTENT_FINGERPRINT
EXTRACTED_TEXT
OCR_RESULT
TEXT_SEGMENTS
TRANSLATION_RESULT
POST_PROCESSED_RESULT
RENDER_LAYOUT
EXPORT_OUTPUT
DIAGNOSTICS_REPORT
```

### Artifact lifecycle

```text
TASK
PIPELINE
SESSION
APPLICATION
PERSISTENT
```

### Text data flow

```text
SourceRef
    ↓
RawTextRef
    ↓
NormalizedTextRef
    ↓
ContentFingerprint
    ↓
ExtractedTextRef
    ↓
SegmentsRef
    ↓
TranslationResultRef
    ↓
PostProcessedResultRef
    ↓
RenderLayoutRef
```

### Image data flow

```text
SourceRef
    ↓
RawImageRef
    ↓
NormalizedImageRef
    + ImageTransformMapRef
    ↓
ContentFingerprint
    ↓
OcrResultRef
    ↓
SegmentsRef
    ↓
TranslationResultRef
    ↓
PostProcessedResultRef
    ↓
RenderLayoutRef
```

### Các flow bổ sung đã mô tả

- Manual Image Translation
- Clipboard Text
- Clipboard Image
- Document Translation
- Document Chunking
- Glossary Snapshot
- Translation Memory
- Translation Editing
- Glossary Suggestion
- Export
- Diagnostics
- Retry
- Provider Fallback
- Cancellation
- Partial Result
- Full Cache Hit
- Partial Cache Hit
- Session Restoration
- Memory Pressure
- Disk Pressure
- Multi-process Artifact Reference

### Image coordinate mapping

Overlay không được dùng trực tiếp tọa độ OCR.

Luồng mapping:

```text
OCR normalized coordinates
    ↓
Image Transform inverse
    ↓
Capture region coordinates
    ↓
Target window coordinates
    ↓
Logical screen coordinates
    ↓
Overlay native coordinates
```

Phải kiểm tra:

```text
regionVersion
source identity version
window bounds version
contentRevision
```

### Privacy mode đề xuất

```text
STANDARD
LOCAL_ONLY
EPHEMERAL
```

Mặc định MVP:

- không lưu raw screenshot
- không lưu clipboard history
- không log source text
- không log translated text
- secret lưu trong secure storage
- cloud provider chỉ nhận vùng hoặc text tối thiểu cần thiết

---


## 12. Runtime Architecture đã hoàn thành

Runtime Architecture được tách thành thư mục:

```text
docs/architecture/runtime/
├── PIPELINE_RUNTIME.md
├── WORK_QUEUE.md
├── SCHEDULER.md
├── CANCELLATION.md
├── CACHE_POLICY.md
├── MEMORY_MODEL.md
├── THREADING_MODEL.md
├── RESOURCE_LIFECYCLE.md
├── PERFORMANCE_MODEL.md
├── ERROR_MODEL.md
├── RETRY_POLICY.md
└── RUNTIME_OBSERVABILITY.md
```

Mục tiêu của nhóm tài liệu này là mô tả cách pipeline vận hành thực tế trong application process, thay vì chỉ mô tả capability hoặc data flow ở mức tĩnh.

Mô hình runtime tổng quát:

```text
Screen Observation
    ↓
Stable Content Revision
    ↓
Scheduler Admission
    ↓
Bounded Work Queue
    ↓
Stage Execution
    ↓
Artifact Publication
    ↓
Authority Validation
    ↓
Presentation Commit
```

### `runtime/PIPELINE_RUNTIME.md`

Đã xác định:

- pipeline vận hành theo revision
- mỗi revision đại diện cho một phiên bản nội dung ổn định
- stage không tự gọi stage kế tiếp
- worker chỉ thực thi công việc đã được Scheduler cấp
- kết quả phải quay về Runtime Control để kiểm tra quyền hợp lệ
- downstream work chỉ được tạo sau khi upstream result đã được chấp nhận
- pipeline có thể kết thúc bằng success, failure, cancellation hoặc obsolete result
- current revision luôn có quyền ưu tiên cao nhất

Luồng điều phối:

```text
Worker completes
    ↓
WorkCompleted Command
    ↓
Runtime Control validates
    ↓
Artifact published
    ↓
Scheduler admits downstream work
```

### `runtime/WORK_QUEUE.md`

Đã thống nhất:

- queue phải bounded
- queue chỉ chứa reference và metadata nhẹ
- không đưa image buffer hoặc artifact payload lớn trực tiếp vào queue
- queue hỗ trợ current-revision priority
- obsolete queued work phải được loại bỏ sớm
- capture frame không tạo backlog vô hạn
- latest-frame replacement được ưu tiên hơn lưu mọi frame
- queue saturation phải tạo backpressure thay vì tăng memory không giới hạn

WorkItem là immutable và mang các identity tối thiểu:

```text
SessionId
RevisionId
WorkItemId
AttemptId
Stage
Priority
InputArtifactRefs
CancellationContext
```

### `runtime/SCHEDULER.md`

Scheduler là thành phần duy nhất quyết định công việc nào được chạy.

Đã thống nhất:

- Scheduler không thực thi nghiệp vụ stage
- worker không được tự schedule retry hoặc downstream stage
- Scheduler chạy trong Runtime Control context
- Scheduler xét revision authority, dependency, queue capacity, concurrency và resource budget
- current revision được ưu tiên
- obsolete work bị reject hoặc remove
- provider concurrency và worker concurrency đều phải bounded
- Scheduler phải giữ đủ capacity cho cancellation và control commands

Mô hình:

```text
Runtime State
    +
Pending Work
    +
Resource Capacity
    +
Revision Authority
    ↓
Scheduler Decision
    ↓
Admit / Defer / Reject / Replace
```

### `runtime/CANCELLATION.md`

Cancellation được coi là control flow bình thường, không phải lỗi mặc định.

Đã định nghĩa:

- cancellation theo application, session, revision, WorkItem và attempt
- revision mới có thể supersede revision cũ
- queued work phải được remove trước
- running work nhận cooperative cancellation
- provider không hỗ trợ abort vẫn phải mất commit authority
- late result sau cancellation trở thành stale
- cancellation phải propagates nhanh nhưng cleanup vẫn an toàn
- cancellation không được hồi sinh work đã terminal

Luồng:

```text
Cancellation Requested
    ↓
Authority Revoked
    ↓
Queued Work Removed
    ↓
Running Work Signaled
    ↓
Resources Drain
    ↓
Terminal Outcome
```

### `runtime/CACHE_POLICY.md`

Mô hình cache truyền thống đã được nâng thành:

```text
Artifact Store
```

thay vì để từng stage sở hữu cache riêng.

Artifact chính:

```text
SourceImageArtifact
OCRArtifact
LayoutArtifact
TranslationArtifact
PresentationArtifact
```

Đã thống nhất:

- artifact là immutable
- cache lookup diễn ra trước expensive admission khi có thể
- cache key phải chứa toàn bộ version ảnh hưởng kết quả
- cache hit trả về ArtifactId hoặc ArtifactRef
- cache miss không ảnh hưởng correctness
- failed, canceled hoặc stale output không được promote như artifact hợp lệ
- memory-only cache phù hợp MVP
- OCR và Translation artifact có giá trị reuse cao hơn temporary intermediate
- cache eviction phải tách khỏi logical revision ownership

### `runtime/MEMORY_MODEL.md`

Đã xác định ba thành phần logic:

```text
Revision Store
Artifact Store
Artifact Lease
```

Nguyên tắc chính:

- large objects không truyền trực tiếp giữa workers
- worker chỉ trao đổi `RevisionId` và `ArtifactId`
- worker phải acquire lease trước khi đọc artifact
- lease được release sau khi sử dụng
- revision ownership, cache ownership và worker lease là các loại retention khác nhau
- artifact chỉ được physical dispose khi không còn owner hoặc lease
- canceled work có thể đã mất logical authority nhưng resource vẫn cần thời gian drain
- Runtime Control là single logical writer đối với ownership metadata

Lease lifecycle:

```text
Acquire
    ↓
Read
    ↓
Release
```

### `runtime/THREADING_MODEL.md`

Đã xác định các execution context logic:

```text
Application Runtime
├── UI Context
├── Runtime Control Context
├── Capture Context
├── Observation Context
├── CPU Worker Pool
├── Provider I/O
├── GPU Context
└── Optional Isolated Process
```

Quyết định chính:

- UI Context chỉ cập nhật UI
- Runtime Control là single logical writer
- Scheduler thuộc Runtime Control
- Capture không chờ OCR hoặc Translation
- Observation xử lý tuần tự để giữ stability state nhất quán
- CPU Worker Pool xử lý OCR, layout hoặc presentation computation phù hợp
- Provider I/O async nhưng bounded
- provider callback không trực tiếp mutate runtime state
- worker chỉ gửi completion command
- UI commit phải đi qua authority validation
- logical execution context không đồng nghĩa một dedicated thread

### `runtime/RESOURCE_LIFECYCLE.md`

Lifecycle được định nghĩa theo ownership transfer thay vì chỉ create/delete.

Mô hình:

```text
Create
    ↓
Register
    ↓
Publish
    ↓
Acquire
    ↓
Use
    ↓
Release
    ↓
Eligible for Disposal
    ↓
Disposed
```

Đã thống nhất:

- creator sở hữu resource ban đầu
- resource phải register trước khi được chia sẻ
- publication là atomic
- ownership transfer phải explicit
- temporary resource không tự trở thành shared resource
- cache promotion thay đổi retention ownership mà không cần copy
- revision disposal không được phá artifact còn lease
- shutdown phải có disposal order xác định
- cleanup failure phải được quan sát và không làm resource sống vô hạn

`Resource Manager` hiện được coi là logical runtime responsibility, chưa bắt buộc thành module riêng trong MVP.

Trách nhiệm logic:

```text
Registration
Ownership Transfer
Lease Tracking
Eligibility Check
Physical Disposal
```

### `runtime/PERFORMANCE_MODEL.md`

Performance được đánh giá bằng kết quả hữu ích cho revision hiện tại, không phải raw throughput.

Metric trung tâm:

```text
Useful Translation Latency
```

Luồng đo:

```text
Stable Current Content
    ↓
Current Revision Created
    ↓
Required Work Completed
    ↓
Valid Presentation Committed
```

Đã phân biệt:

- interaction latency
- observation latency
- queue wait
- stage execution latency
- provider latency
- commit latency
- useful-result latency
- stale execution time
- recovery latency

Các nguyên tắc:

- current revision first
- UI responsiveness ưu tiên hơn throughput
- queue wait và execution time phải đo riêng
- P50, P90, P95 và P99 quan trọng hơn average đơn thuần
- stale completion không được tính là useful throughput
- overload phải được xử lý bằng cancellation, backpressure và graceful degradation
- long-session memory, thread, queue và artifact count phải bounded

Metric quan trọng:

```text
Current Revision Commit Ratio
Useful Translation Latency
Stale Work Ratio
Useful Work Ratio
Wasted Execution Time
```

### `runtime/ERROR_MODEL.md`

Đã phân biệt rõ năm terminal outcome:

```text
SUCCEEDED
FAILED
CANCELED
STALE
ABANDONED
```

Ý nghĩa:

- `FAILED`: không tạo được output hợp lệ
- `CANCELED`: dừng theo yêu cầu control flow
- `STALE`: kết quả có thể đúng kỹ thuật nhưng mất logical authority
- `ABANDONED`: runtime ngừng chờ nhưng chưa xác nhận physical execution đã kết thúc
- `SUCCEEDED`: output hợp lệ và có thể đi tiếp sau validation

Đã định nghĩa:

- normalized RuntimeError
- stable ErrorCode
- category
- severity
- retry class
- scope
- recoverability
- provider error normalization
- user-visible error mapping
- stale error suppression
- error deduplication
- cleanup failure
- fatal invariant handling
- privacy trong error diagnostics

Quy tắc trọng tâm:

```text
Technical Failure
    ≠
Cancellation
    ≠
Stale Result
```

### `runtime/RETRY_POLICY.md`

Retry thuộc Runtime Control, không thuộc Worker hoặc Provider Adapter.

Luồng:

```text
Work Failed
    ↓
Runtime validates relevance
    ↓
Retry Policy evaluates
    ↓
New Attempt created
    ↓
Scheduler admission
    ↓
Execution
```

Đã thống nhất:

- mỗi retry tạo `AttemptId` mới
- attempt cũ đã terminal, không được resume
- revision phải còn current
- session phải còn active
- retry budget phải còn
- cancellation phải chưa được yêu cầu
- delayed retry phải cancelable
- exponential backoff và jitter dùng cho repeated transient failure
- `Retry-After` của provider cần được tôn trọng
- retry phải check cache lại
- old attempt result không được overwrite new attempt
- provider fallback là một new attempt
- shutdown hủy toàn bộ pending retry
- worker và provider adapter không tự retry

### `runtime/RUNTIME_OBSERVABILITY.md`

Observability được định nghĩa rộng hơn logging:

```text
Runtime Observability
├── Metrics
├── Traces
├── Structured Logs
├── Runtime Events
└── Diagnostic Snapshots
```

Correlation model:

```text
ApplicationInstanceId
    ↓
SessionId
        ↓
RevisionId
            ↓
WorkItemId
                ↓
AttemptId
```

Identifier bổ sung:

```text
ArtifactId
ProviderRequestId
TraceId
SpanId
PresentationId
```

Đã xác định telemetry cho:

- revision lifecycle
- WorkItem và attempt
- queues
- Scheduler
- cancellation
- stale result
- retry
- provider health
- capture và observation
- OCR, layout, translation và presentation
- UI responsiveness
- cache
- artifact và resource lifecycle
- CPU, memory và GPU
- Runtime Control
- Event Bus
- startup và shutdown
- performance-budget violation

Nguyên tắc privacy:

```text
No Content by Default
```

Standard telemetry không chứa:

- screenshot
- OCR text
- translated text
- prompt
- source URL
- window title
- credential
- provider request body

MVP observability:

```text
In-Memory Metrics
Structured Local Logs
Revision Trace Timeline
Bounded Recent Event Buffer
Runtime Diagnostic Snapshot
```

Remote telemetry export chưa cần trong MVP.

### Các khái niệm runtime đã được làm rõ

#### Revision

Đại diện cho một phiên bản nội dung ổn định.

#### WorkItem

Đại diện cho một đơn vị công việc logic của stage.

#### Attempt

Đại diện cho một lần thực thi vật lý của WorkItem.

```text
Revision
└── WorkItem
    ├── Attempt 1
    ├── Attempt 2
    └── Attempt 3
```

`Attempt` chưa cần tài liệu riêng, trừ khi xuất hiện speculative execution, parallel providers hoặc retry đa nhánh.

#### Resource Manager

Hiện là logical responsibility, chưa phải standalone module bắt buộc.

#### Artifact Store

Là nguồn quản lý artifact immutable và lifecycle metadata, thay thế cách để mỗi stage tự giữ cache riêng.

#### Runtime Control

Là single logical writer cho runtime state, authority, scheduling decision và terminal outcome acceptance.

### Runtime invariants đã chốt

1. Current revision có quyền ưu tiên cao nhất.
2. Worker không tự schedule downstream work.
3. Worker và Provider Adapter không tự retry.
4. Queue và concurrency luôn bounded.
5. Large payload chỉ truyền bằng artifact reference.
6. Mỗi WorkItem chỉ có một terminal outcome được chấp nhận.
7. Mỗi retry tạo AttemptId mới.
8. Late attempt không được overwrite attempt mới.
9. Stale result không được commit hoặc hiển thị lỗi cho revision hiện tại.
10. Cancellation là control flow hợp lệ.
11. Failed hoặc stale artifact không được cache như success.
12. Artifact chỉ dispose khi không còn owner hoặc lease.
13. UI không bị block bởi OCR, provider hoặc telemetry.
14. Runtime Control không bị telemetry export chặn.
15. Performance được đo bằng useful current-revision output.
16. Standard observability không chứa user content.
17. Shutdown phải stop admission trước khi cleanup.
18. Telemetry failure không được làm sai runtime correctness.

---

## 13. MVP hiện tại

CRAI được xác định là Desktop App trước.

### Có trong MVP

- chọn cửa sổ
- chọn vùng đọc
- theo dõi thay đổi
- stability detection
- capture nội dung
- text extraction
- OCR
- segmentation
- translation
- post-processing
- Side Panel
- Overlay theo boundary riêng
- manual correction
- glossary cơ bản
- cache cơ bản
- session management
- provider abstraction
- cancellation
- retry
- stale result protection
- diagnostics cơ bản

### Chưa làm trong MVP

- Browser Extension
- Story Library
- Downloader
- Cloud Sync
- Plugin Marketplace
- Image Replacement
- Inpainting
- TTS
- distributed processing
- external message broker
- third-party dynamic plugin loading

---

## 14. Kiến trúc hiện tại

```text
AI_BOOT
    ↓
PROJECT_RULE
    ↓
MODULES_RULE
    ↓
CAPABILITY_MAP
    ↓
USER_JOURNEY
    ↓
STATE_MACHINE
    ↓
EVENT_BUS
    ↓
MODULE_DEPENDENCY
    ↓
DATA_FLOW
    ↓
RUNTIME ARCHITECTURE
```

Runtime Architecture hiện gồm:

```text
PIPELINE_RUNTIME
    ↓
WORK_QUEUE
    ↓
SCHEDULER
    ↓
CANCELLATION
    ↓
CACHE_POLICY
    ↓
MEMORY_MODEL
    ↓
THREADING_MODEL
    ↓
RESOURCE_LIFECYCLE
    ↓
PERFORMANCE_MODEL
    ↓
ERROR_MODEL
    ↓
RETRY_POLICY
    ↓
RUNTIME_OBSERVABILITY
```

Mối quan hệ:

```text
CAPABILITY_MAP
    → hệ thống cần làm được gì

USER_JOURNEY
    → người dùng sử dụng hệ thống thế nào

STATE_MACHINE
    → hệ thống đang ở trạng thái nào

EVENT_BUS
    → các module trao đổi signal thế nào

MODULE_DEPENDENCY
    → module nào được biết và phụ thuộc module nào

DATA_FLOW
    → dữ liệu nào đi qua đâu, thuộc owner nào và sống bao lâu

RUNTIME ARCHITECTURE
    → công việc được schedule, execute, cancel, retry, observe và cleanup thế nào
```

---

## 15. Các quyết định kiến trúc lớn đã chốt

1. CRAI ưu tiên Desktop App.
2. Text Flow và Image Flow độc lập.
3. Structured text được ưu tiên trước OCR.
4. Pipeline Orchestrator sở hữu workflow.
5. Event Bus chỉ phục vụ giao tiếp nội bộ.
6. MVP dùng in-memory, in-process Event Bus.
7. Mỗi session có pipeline riêng.
8. Mỗi nội dung có `contentRevision`.
9. Stale result không được render.
10. UI không gọi trực tiếp provider hoặc platform API.
11. Infrastructure và Platform triển khai abstraction.
12. Composition Root wiring implementation.
13. Dữ liệu lớn truyền qua `ArtifactRef`.
14. Public contract phải serializable.
15. Raw provider DTO không vượt adapter boundary.
16. Artifact phải được cleanup theo lifecycle.
17. Screenshot và clipboard không được persist mặc định.
18. Secret không đi qua Event Bus.
19. Cloud provider chỉ nhận dữ liệu tối thiểu.
20. Cache key phải chứa các version ảnh hưởng kết quả.
21. Glossary được dùng dưới dạng immutable snapshot.
22. Translation result tách khỏi Render Layout.
23. Overlay phải bị loại khỏi source capture.
24. Module mới phải có ownership và dependency rõ ràng.
25. AI chỉ được sửa trong module scope được giao.
26. Runtime Control là single logical writer cho runtime state.
27. Scheduler là nơi duy nhất quyết định admission của WorkItem.
28. Worker không tự chạy downstream stage.
29. Worker và Provider Adapter không tự retry.
30. Queue, worker concurrency và provider concurrency đều bounded.
31. Current revision được ưu tiên hơn raw throughput.
32. WorkItem chỉ truyền artifact reference, không truyền large payload trực tiếp.
33. Artifact là immutable.
34. Artifact physical disposal chỉ xảy ra khi không còn owner hoặc lease.
35. Retry luôn tạo AttemptId mới.
36. Mỗi WorkItem chỉ chấp nhận một terminal outcome.
37. Terminal outcome gồm `SUCCEEDED`, `FAILED`, `CANCELED`, `STALE`, `ABANDONED`.
38. Cancellation không được mặc định coi là failure.
39. Stale result là terminal outcome logic và không có commit authority.
40. Performance được đo bằng useful current-revision output.
41. Queue wait và execution duration phải được đo riêng.
42. Observability gồm metrics, traces, logs, runtime events và snapshots.
43. Standard telemetry không chứa screenshot, OCR text, translated text hoặc credential.
44. Telemetry failure không được phá runtime correctness.
45. `Resource Manager` và `Attempt` hiện là architectural concepts, chưa bắt buộc thành module hoặc tài liệu riêng.

---

## 16. Những vấn đề còn cần chốt

### Technology Stack

- desktop framework
- ngôn ngữ cho core
- ngôn ngữ hoặc runtime cho OCR worker
- UI framework
- cách enforce dependency

### Process Topology

- một process hay nhiều process
- OCR có chạy worker riêng không
- capture có cần native process không
- image buffer truyền qua RAM, shared memory hay file tạm
- cách restart worker khi crash

### Artifact Store

- memory hay temporary file
- reference counting
- cleanup sau crash
- giới hạn theo pipeline hoặc session
- có persistent artifact hay không

### Cache

- memory, SQLite hay filesystem
- OCR cache retention
- translation cache retention
- cache trong `EPHEMERAL` mode
- cache cleanup policy

### OCR

- OCR provider đầu tiên
- local model
- language detection
- polygon hoặc rectangle
- reading order
- sound effect handling
- comic bubble grouping

### Translation

- provider đầu tiên
- batch hay segment
- context window
- streaming
- partial result
- lỗi một segment hay toàn batch
- glossary injection
- translation memory policy

### Rendering

- Side Panel trước hay song song Overlay
- text measurement boundary
- native overlay technology
- multi-monitor DPI
- overflow policy
- overlay exclusion

### Privacy

- history mặc định bật hay tắt
- cloud consent
- local-only UI indicator
- temporary file cleanup
- diagnostics redaction

---

## 17. Tài liệu nên thực hiện tiếp

Thứ tự đề xuất hiện tại:

1. `docs/architecture/runtime/RUNTIME_CONFIG.md`
2. `docs/architecture/runtime/RUNTIME_COMPONENTS.md`
3. rà soát chéo toàn bộ Runtime Architecture
4. cập nhật `docs/architecture/MODULE_DEPENDENCY.md` theo runtime component cuối cùng
5. cập nhật `docs/architecture/DATA_FLOW.md` với `RevisionId`, `WorkItemId` và `AttemptId`
6. `docs/architecture/PROCESS_TOPOLOGY.md`
7. `docs/architecture/PROVIDER_ARCHITECTURE.md`
8. `docs/architecture/SECURITY_PRIVACY.md`
9. `docs/architecture/RENDERING_ARCHITECTURE.md`
10. `docs/architecture/PERSISTENCE.md`

Các tài liệu cũ dự kiến như:

```text
ARTIFACT_STORE.md
CACHE_STRATEGY.md
CONCURRENCY_MODEL.md
ERROR_MODEL.md
OBSERVABILITY.md
```

cần được rà soát trước khi tạo vì nhiều trách nhiệm đã được bao phủ trong thư mục `runtime/`:

```text
CACHE_POLICY.md
MEMORY_MODEL.md
THREADING_MODEL.md
RESOURCE_LIFECYCLE.md
ERROR_MODEL.md
RUNTIME_OBSERVABILITY.md
```

Không nên tạo tài liệu trùng nội dung chỉ vì tên từng được đề xuất trước đó.

---

## 18. Tài liệu ưu tiên tiếp theo

Tài liệu phù hợp nhất để tiếp tục là:

```text
docs/architecture/runtime/RUNTIME_CONFIG.md
```

Tài liệu này cần chốt:

- configuration ownership
- application defaults
- session configuration
- provider configuration
- capture rate
- stability threshold
- queue capacity
- stage concurrency
- provider concurrency
- timeout
- retry budget
- cache budget
- memory budget
- performance threshold
- observability mode
- secret reference
- configuration validation
- immutable configuration snapshot
- hot reload boundary
- configuration versioning
- safe MVP defaults

Sau `RUNTIME_CONFIG.md`, cần hoàn thành:

```text
docs/architecture/runtime/RUNTIME_COMPONENTS.md
```

`RUNTIME_COMPONENTS.md` sẽ tổng hợp:

```text
Runtime Control
Scheduler
Work Queues
Revision Store
Artifact Store
Resource Manager
Provider Manager
Worker Execution
Retry Policy
Cancellation Coordinator
Observability
```

và xác định rõ:

- thành phần nào là module thực tế
- thành phần nào chỉ là logical responsibility
- thành phần nào thuộc Application, Core hoặc Infrastructure
- ownership của từng runtime state
- dependency giữa các thành phần
- boundary MVP và future architecture
- có cần tách `ATTEMPT_MODEL.md` hoặc `RESOURCE_MANAGER.md` hay không

---

## 19. Trạng thái tài liệu

| Tài liệu | Trạng thái |
|---|---|
| `.meta/AI_BOOT.md` | Đã có |
| `.meta/PROJECT_RULE.md` | Đã có |
| `.meta/MODULES_RULE.md` | Đã hoàn thiện ở mức nguyên tắc |
| `.meta/MODULES.md` | Đã có danh mục ban đầu |
| `.meta/USER_JOURNEY.md` | Đã hoàn thành bản kiến trúc |
| `docs/architecture/CAPABILITY_MAP.md` | Đã hoàn thành bản Draft |
| `docs/architecture/STATE_MACHINE.md` | Đã hoàn thành bản Draft |
| `docs/architecture/EVENT_BUS.md` | Đã hoàn thành bản Draft |
| `docs/architecture/MODULE_DEPENDENCY.md` | Đã hoàn thành bản Draft |
| `docs/architecture/DATA_FLOW.md` | Đã hoàn thành bản Draft |
| `docs/architecture/runtime/PIPELINE_RUNTIME.md` | Đã hoàn thành bản Draft |
| `docs/architecture/runtime/WORK_QUEUE.md` | Đã hoàn thành bản Draft |
| `docs/architecture/runtime/SCHEDULER.md` | Đã hoàn thành bản Draft |
| `docs/architecture/runtime/CANCELLATION.md` | Đã hoàn thành bản Draft |
| `docs/architecture/runtime/CACHE_POLICY.md` | Đã hoàn thành bản Draft |
| `docs/architecture/runtime/MEMORY_MODEL.md` | Đã hoàn thành bản Draft |
| `docs/architecture/runtime/THREADING_MODEL.md` | Đã hoàn thành bản Draft |
| `docs/architecture/runtime/RESOURCE_LIFECYCLE.md` | Đã hoàn thành bản Draft |
| `docs/architecture/runtime/PERFORMANCE_MODEL.md` | Đã hoàn thành bản Draft |
| `docs/architecture/runtime/ERROR_MODEL.md` | Đã hoàn thành bản Draft |
| `docs/architecture/runtime/RETRY_POLICY.md` | Đã hoàn thành bản Draft |
| `docs/architecture/runtime/RUNTIME_OBSERVABILITY.md` | Đã hoàn thành bản Draft |
| `docs/architecture/runtime/RUNTIME_CONFIG.md` | Chưa thực hiện |
| `docs/architecture/runtime/RUNTIME_COMPONENTS.md` | Chưa thực hiện |
| `docs/architecture/PROCESS_TOPOLOGY.md` | Chưa thực hiện |
| `docs/architecture/PROVIDER_ARCHITECTURE.md` | Chưa thực hiện |
| `docs/architecture/SECURITY_PRIVACY.md` | Chưa thực hiện |
| `docs/architecture/RENDERING_ARCHITECTURE.md` | Chưa thực hiện |
| `docs/architecture/PERSISTENCE.md` | Chưa thực hiện |

---

## 20. Cập nhật kiến ​​trúc mới nhất

Khi bắt đầu đoạn chat mới, dùng key:

```text
Tiếp tục CRAI
```

Sau đó chỉ rõ tài liệu hoặc module cần tiếp tục.

Ví dụ:

```text
Tiếp tục CRAI
docs/architecture/runtime/RUNTIME_CONFIG.md
```

AI cần đọc theo thứ tự tối thiểu:

```text
.meta/AI_BOOT.md
.meta/PROJECT_RULE.md
.meta/MODULES_RULE.md
docs/architecture/CAPABILITY_MAP.md
docs/architecture/STATE_MACHINE.md
docs/architecture/EVENT_BUS.md
docs/architecture/MODULE_DEPENDENCY.md
docs/architecture/DATA_FLOW.md
docs/architecture/runtime/PIPELINE_RUNTIME.md
docs/architecture/runtime/WORK_QUEUE.md
docs/architecture/runtime/SCHEDULER.md
docs/architecture/runtime/CANCELLATION.md
docs/architecture/runtime/CACHE_POLICY.md
docs/architecture/runtime/MEMORY_MODEL.md
docs/architecture/runtime/THREADING_MODEL.md
docs/architecture/runtime/RESOURCE_LIFECYCLE.md
docs/architecture/runtime/PERFORMANCE_MODEL.md
docs/architecture/runtime/ERROR_MODEL.md
docs/architecture/runtime/RETRY_POLICY.md
docs/architecture/runtime/RUNTIME_OBSERVABILITY.md
```

Khi chỉ làm một runtime document, có thể đọc rút gọn:

```text
.meta/AI_BOOT.md
.meta/PROJECT_RULE.md
docs/architecture/STATE_MACHINE.md
docs/architecture/EVENT_BUS.md
docs/architecture/DATA_FLOW.md
docs/architecture/runtime/<các tài liệu liên quan trực tiếp>
```

Không được tự ý thay đổi các architectural decision đã chốt mà không:

1. nêu rõ quyết định hiện tại
2. giải thích vấn đề
3. đưa ra phương án thay thế
4. phân tích tác động
5. cập nhật decision record

---

## 21. Điểm tiếp tục chính xác

Trạng thái hiện tại của Runtime Architecture:

```text
Behavior
    → đã mô tả

Scheduling
    → đã mô tả

Queueing
    → đã mô tả

Cancellation
    → đã mô tả

Caching and Memory
    → đã mô tả

Threading
    → đã mô tả

Resource Lifecycle
    → đã mô tả

Performance
    → đã mô tả

Errors and Retry
    → đã mô tả

Observability
    → đã mô tả

Configuration
    → chưa mô tả

Final Component Boundaries
    → chưa tổng hợp
```

Điểm tiếp tục:

```text
docs/architecture/runtime/RUNTIME_CONFIG.md
```

Sau đó:

```text
docs/architecture/runtime/RUNTIME_COMPONENTS.md
```


---

## 22. Cập nhật Module Architecture (2026-07-29 - Text Processing)

### Business Module Pipeline

```text
Capture / Observation
        ↓
Recognition
        ↓
Text Processing
        ↓
Translation
        ↓
Presentation
```

Đã thống nhất trách nhiệm:

- Recognition → What text exists?
- Text Processing → What clean source text should be translated?
- Translation → What does the text mean?

### Recognition Module

Đã hoàn thành:

```text
modules/recognition/
├── README.md
├── MODULE.md
├── CONTRACT.md
├── EVENTS.md
└── STATES.md
```

Recognition chỉ chịu trách nhiệm OCR, reading order, region mapping và traceability; không chịu trách nhiệm Translation hoặc Presentation.

### Text Processing Module

Đã hoàn thành:

```text
modules/text-processing/
├── README.md
├── MODULE.md
├── CONTRACT.md
├── EVENTS.md
└── STATES.md (điểm tiếp theo)
```

Các quyết định đã chốt:

- SourceDocument là output chuẩn của Text Processing.
- Translation chỉ làm việc với SourceDocument, không phụ thuộc trực tiếp RecognitionResult.
- SourceSegment giữ traceability với Recognition.
- TranslationUnit được tạo từ SourceDocument.
- Text Processing chịu trách nhiệm:
  - validation
  - reading-order refinement
  - normalization
  - line reconstruction
  - region grouping
  - block classification
  - document assembly
  - traceability validation
- CONTRACT đã định nghĩa:
  - TextProcessingRequest
  - TextProcessingResult
  - ResultReference
  - SourceDocument
  - SourceBlock
  - SourceSegment
  - ProcessingProfile
  - Metrics
  - Warning
  - Error
- EVENTS chuẩn hóa đầy đủ:
  - requested
  - started
  - completed
  - failed
  - cancellation_requested
  - cancelled
  - progress events
  - configuration events
  - module health events
- Event chỉ truyền metadata và ResultReference; không truyền toàn bộ SourceDocument hoặc OCR payload.

### Điểm tiếp tục

```text
modules/text-processing/STATES.md
        ↓
modules/translation/
```


---

## 23. Cập nhật Module Architecture (2026-07-29 - Translation)

### Translation Module

Đã hoàn thành đầy đủ bộ tài liệu:

```text
modules/translation/
├── README.md
├── MODULE.md
├── CONTRACT.md
├── EVENTS.md
├── STATES.md
└── ERRORS.md
```

### Các quyết định kiến trúc đã chốt

#### Vị trí trong pipeline

```text
Capture / Observation
        ↓
Recognition
        ↓
Text Processing
        ↓
Translation
        ↓
Presentation
```

Translation chỉ nhận đầu vào từ Text Processing.

#### Trách nhiệm

Translation chịu trách nhiệm:

- translation job lifecycle
- attempt và retry
- batching
- provider orchestration
- context
- glossary snapshot
- validation
- authority check
- translation variants
- cache coordination
- normalized errors
- events

Không chịu trách nhiệm:

- OCR
- extraction
- rendering
- overlay
- browser
- presentation

#### Mô hình cốt lõi

```text
PreparedSegment
    ↓
TranslationJob
    ↓
TranslationAttempt
    ↓
TranslationBatch
    ↓
TranslationResult
    ↓
TranslationVariant
```

Các nguyên tắc đã chốt:

- Retry tạo TranslationAttempt mới, không tạo TranslationJob mới.
- Batch là execution unit, PreparedSegment là alignment unit.
- TranslationVariant là immutable.
- Provider Adapter che giấu toàn bộ DTO và SDK của provider.
- Late result không được ghi đè authoritative result.
- Cancellation và Superseded là lifecycle bình thường.
- Progressive publication hỗ trợ cho comic.
- Translation không phụ thuộc provider cụ thể.

### Bộ tài liệu đã hoàn thành

MODULE.md
- Boundary
- Ownership
- Core Concepts
- Profiles
- Retry
- Variants

CONTRACT.md
- Public Commands
- Queries
- Data Contracts
- Identifiers
- Policies

EVENTS.md
- Event Envelope
- Lifecycle Events
- Retry/Fallback Events
- Variant Events
- Completion Events

STATES.md
- Job State Machine
- Attempt State Machine
- Batch State Machine
- Result State Machine
- Variant State Machine

ERRORS.md
- Normalized Error Codes
- Warning Codes
- Retryability
- Lifecycle Outcomes

README.md
- Module Overview
- Responsibilities
- Architecture Summary
- Reading Order
- Documentation Map

### Điểm tiếp tục

```text
modules/presentation/MODULE.md
```


---

## 24. Cập nhật Domain Architecture (2026-07-29)

### Domain Architecture

Đã thống nhất mô hình Domain theo DDD với ranh giới Aggregate rõ ràng.

### Aggregate Hierarchy

```text
Project Aggregate
└── Book Aggregate
    └── Chapter Aggregate
        └── Page Aggregate
            ├── Image
            ├── Text Block
            ├── Translation
            └── Diagnostics
```

### Domain Documents

Đã hoàn thành:

```text
docs/architecture/domain/
├── README.md
├── PROJECT.md
├── BOOK.md
├── CHAPTER.md
├── PAGE.md
└── IMAGE.md
```

### Quyết định kiến trúc

- Aggregate liên kết bằng ID, không giữ toàn bộ object graph.
- Page là processing aggregate chính.
- OCR, AI Translation, Rendering chạy trong phạm vi Page.
- Image không phải Aggregate Root.
- Image chỉ giữ metadata, lineage và Asset Reference.
- Binary image thuộc Storage.
- Pixel modification luôn tạo Derived Image.
- Page là cache, retry và parallel boundary.

### Điểm tiếp tục

```text
TEXT_BLOCK.md
↓
TRANSLATION.md
↓
CHARACTER.md
↓
GLOSSARY.md
↓
SESSION.md
```


---

## 25. Cập nhật kiến trúc Domain, OCR & Text Model (2026-07-29)

### Domain Architecture

Đã thống nhất chuyển sang mô hình Domain-Driven Design (DDD) theo hướng Provider-Neutral và Immutable.

Các Aggregate/Concept đã hoàn thành:

```text
README.md
WORKSPACE.md
PROFILE.md
SESSION.md
CHARACTER.md
GLOSSARY.md
LANGUAGE.md
TEXT_BLOCK.md
TRANSLATION.md
```

Các nguyên tắc đã chốt:

- Domain chỉ mô tả business model và business truth.
- Stable Identity + Immutable Revision + Snapshot.
- Domain không mô tả provider, runtime hoặc implementation.
- Translation luôn tham chiếu immutable snapshots.
- Workspace là boundary cộng tác; Project là boundary nội dung.
- Session là working context; không phải runtime job.

### OCR Architecture

Đã hoàn thành nhóm tài liệu kiến trúc OCR độc lập với Domain và OCR Provider cụ thể.

```text
docs/architecture/ocr/
├── README.md
├── PIPELINE.md
├── PREPROCESS.md
├── DETECTION.md
├── RECOGNITION.md
├── TEXT_DIRECTION.md
├── LAYOUT.md
├── POSTPROCESS.md
├── QUALITY.md
├── PROVIDERS.md
└── READING_ORDER.md
```

#### Pipeline đã thống nhất

```text
Image
    ↓
Preprocessing
    ↓
Detection
    ↓
Recognition
    ↓
Text Direction
    ↓
Layout Analysis
    ↓
OCR Postprocessing
    ↓
Quality Assessment
    ↓
Reading Order
    ↓
Text Model
```

Thứ tự triển khai nội bộ có thể thay đổi hoặc chạy kết hợp theo Provider Capability, nhưng Contract đầu ra phải được chuẩn hóa trước khi chuyển sang bước tiếp theo.

#### Các quyết định OCR đã chốt

- OCR là processing architecture, không phải business domain.
- Pipeline và các tầng phía trên không được phụ thuộc trực tiếp SDK hoặc DTO của OCR Provider.
- Detection, Recognition, Text Direction và Layout giữ trách nhiệm riêng biệt.
- Recognition không chỉ trả về chuỗi phẳng mà tạo mô hình phân cấp từ Region đến Character.
- OCR Postprocessing hợp nhất kết quả thành OCR Document chuẩn.
- Quality Assessment chỉ đánh giá và sinh Quality Report; không sửa OCR Document.
- Quality và Confidence là hai khái niệm khác nhau.
- Provider Adapter che giấu API, SDK, lỗi và capability đặc thù của Provider.
- Reading Order là bước cuối còn phụ thuộc mạnh vào Geometry và Layout.
- Reading Order phải hỗ trợ LTR, RTL, Webtoon, vertical text và mixed layout.
- Main Reading Sequence, Auxiliary Sequence và Excluded Entity phải được phân biệt rõ.
- Mọi phần tử được sắp xếp phải ánh xạ ngược về Entity trong OCR Document.
- Cùng Input, Profile và Strategy Version phải tạo kết quả deterministic.

#### Reading Order

`docs/architecture/ocr/READING_ORDER.md` đã hoàn thành với các nội dung chính:

- Global Order và Local Order.
- Panel, Container, Block, Region, Paragraph và Line ordering.
- Left-to-Right, Right-to-Left, Top-to-Bottom và Vertical Column.
- Manga, comic, Webtoon, novel và mixed-language layout.
- Candidate relationship, precedence scoring và rule priority.
- Reading Order Graph, conflict resolution và cycle detection.
- Topological ordering và deterministic tie-breaker.
- Main Sequence, Auxiliary Sequence và exclusion policy.
- Provider hints, confidence, diagnostics và explainability.
- Incremental ordering, cache, events, state machine và manual override.

### Text Architecture

Đã bắt đầu nhóm tài liệu kiến trúc văn bản sau OCR:

```text
docs/architecture/text/
└── TEXT_MODEL.md
```

`TEXT_MODEL.md` định nghĩa ranh giới chuyển đổi:

```text
Visual Domain
    ↓
OCR Document + Reading Order
    ↓
Text Model Builder
    ↓
Language Domain
```

#### Text Document hierarchy

```text
Text Document
└── Page
    └── Section
        └── Block
            └── Paragraph
                └── Sentence
                    └── Span
                        └── Token
```

Không phải mọi tài liệu đều bắt buộc dùng đầy đủ mọi cấp. Các Node cấp thấp có thể được tạo lazy hoặc ở trạng thái provisional.

#### Các quyết định Text Model đã chốt

- Text Document là Contract trung tâm của Language Domain.
- Text Model tách Translation, Search, Export và Presentation khỏi OCR Provider.
- Mọi nội dung sinh từ OCR phải giữ Source Reference và khả năng ánh xạ ngược.
- Text Node và OCR Entity hỗ trợ quan hệ many-to-many, không giả định ánh xạ 1:1.
- OCR Word và Text Token là hai khái niệm khác nhau.
- Source Text, Normalized Text, Corrected Text và Display Text phải được tách riêng.
- Bản dịch không được ghi đè Source Text.
- Correction không được ghi đè Source Text mà không có Revision hoặc Audit Record.
- Canonical Text ưu tiên Corrected Text đã được chấp nhận, sau đó Normalized Text và Source Text.
- Text Model phải hỗ trợ Unicode, grapheme cluster, CJK, ruby/furigana và mixed language.
- Language Metadata hỗ trợ kế thừa và ghi đè theo từng cấp Node.
- Geometry vẫn thuộc Source/OCR Domain; Text Model chỉ giữ Reference hoặc Snapshot tối thiểu.
- Main Content và Auxiliary Content phải được phân biệt.
- Node ID phải ổn định và không phụ thuộc Order Index hoặc Provider Result Index.
- Text Document phải hỗ trợ versioning, incremental update, serialization và migration.
- Translation Unit sẽ được xây dựng từ Text Document, không từ OCR Provider Result.

### Trạng thái luồng hiện tại

```text
OCR Document
    ↓
Reading Order                    ✅ Hoàn thành
    ↓
Text Document                    ✅ Hoàn thành kiến trúc
    ↓
Text Segmentation                ⏳ Tiếp theo
    ↓
Translation Context
    ↓
Translation
    ↓
Presentation
```

### Tài liệu hoàn thành trong đoạn chat này

```text
docs/architecture/ocr/PREPROCESS.md
docs/architecture/ocr/DETECTION.md
docs/architecture/ocr/RECOGNITION.md
docs/architecture/ocr/TEXT_DIRECTION.md
docs/architecture/ocr/LAYOUT.md
docs/architecture/ocr/POSTPROCESS.md
docs/architecture/ocr/QUALITY.md
docs/architecture/ocr/PROVIDERS.md
docs/architecture/ocr/READING_ORDER.md
docs/architecture/text/TEXT_MODEL.md
```

### Điểm tiếp tục chính xác

```text
docs/architecture/text/SEGMENTATION.md
```

## 26. Architecture Evolution

### Giai đoạn đầu

Project
    ↓
OCR
    ↓
Translate
    ↓
Overlay

Đây là ý tưởng ban đầu khi dự án mới hình thành.

Sau quá trình phân tích nhận thấy kiến trúc này có nhiều hạn chế:

- OCR luôn chạy kể cả khi đã có text.
- Translation phụ thuộc trực tiếp OCR.
- Overlay trở thành presentation mặc định.
- Không hỗ trợ nhiều nguồn dữ liệu.
- Không tách được capability.

Do đó kiến trúc tiếp tục được mở rộng.

Sau đó tiếp tục theo luồng:

```text
docs/architecture/translation/CONTEXT.md
    ↓
docs/architecture/translation/TRANSLATION.md
    ↓
docs/architecture/presentation/PRESENTATION.md
```

---

### Giai đoạn Capability

Acquire Content
Detect Content
OCR
Translation
Rendering

↓

Capability Map

Capability bắt đầu được phân tích độc lập.

Tuy nhiên lúc này Capability vẫn gần giống Module.

---

### Giai đoạn Core Architecture

Capability

↓

State Machine

↓

Event Bus

↓

Module Dependency

↓

Data Flow

Ở giai đoạn này dự án bắt đầu chuyển từ mô tả tính năng sang mô tả hệ thống.

Các khái niệm như:

- ownership
- event
- state
- artifact

được hình thành.

---

### Giai đoạn Runtime

Ban đầu Runtime chỉ dự kiến gồm:

- Queue
- Scheduler

Sau đó mở rộng thành:

- Pipeline Runtime
- Runtime Control
- Revision
- WorkItem
- Attempt
- Artifact Store
- Memory Model
- Retry
- Cancellation
- Observability

Runtime trở thành lớp chịu trách nhiệm điều phối toàn bộ pipeline.

---

### Giai đoạn Business Module

Ban đầu:

OCR Module

↓

Segmentation Module

↓

Translation Module

↓

Presentation

Sau nhiều lần phân tích:

Recognition

↓

Text Processing

↓

Translation

↓

Presentation

được xác định là mô hình phù hợp hơn.

---

### Những thay đổi lớn

Observation Module

↓

Capability của Recognition

-------------------------

OCR Module

↓

Recognition

-------------------------

Translation nhận OCR Result

↓

Translation nhận SourceDocument

-------------------------

Layout

↓

Presentation

-------------------------

Cache

↓

Artifact Store

-------------------------

Retry trong Worker

↓

Retry thuộc Runtime Control

-------------------------

Event điều phối Pipeline

↓

Pipeline Orchestrator điều phối Pipeline

-------------------------

Provider

↓

Provider Independent

# 27. Architecture Evolution

## 27.1 Mục đích

Kiến trúc của CRAI không được thiết kế hoàn chỉnh ngay từ đầu.

Trong quá trình phân tích, nhiều thành phần đã được thay đổi, hợp nhất hoặc tách nhỏ khi hiểu rõ hơn về bài toán.

Phần này ghi lại những thay đổi lớn của kiến trúc để giải thích:

- vì sao kiến trúc hiện tại được lựa chọn;
- những phương án nào đã từng được cân nhắc;
- lý do thay đổi;
- tác động của từng quyết định.

Đây không phải Change Log theo thời gian mà là lịch sử tiến hóa của kiến trúc.

---

## 27.2 Từ OCR Pipeline đến Content Pipeline

### Ý tưởng ban đầu

Ở giai đoạn đầu, hệ thống được hình dung khá đơn giản:

```text
Capture
    ↓
OCR
    ↓
Translate
    ↓
Render
```

Pipeline này phù hợp với bài toán dịch ảnh nhưng nhanh chóng bộc lộ nhiều hạn chế.

Các vấn đề chính:

- mọi nguồn dữ liệu đều phải OCR;
- không tận dụng được văn bản có cấu trúc;
- OCR trở thành nút thắt của toàn bộ pipeline;
- khó mở rộng sang web novel, document hoặc clipboard.

---

### Kiến trúc hiện tại

Sau quá trình phân tích, pipeline được chia thành hai hướng độc lập:

```text
Text Flow
```

và

```text
Image Flow
```

Text Flow luôn được ưu tiên nếu nguồn dữ liệu đã chứa văn bản.

OCR chỉ còn là một khả năng xử lý trong Image Flow.

Điều này giúp:

- giảm độ trễ;
- tăng chất lượng dịch;
- giảm lỗi OCR;
- mở rộng nguồn dữ liệu dễ dàng hơn.

---

## 27.3 Từ Capability đến Business Module

Trong những phiên bản đầu, nhiều Capability được xem như Module độc lập.

Ví dụ:

```text
OCR Module

Segmentation Module

Layout Module

Context Module

Translation Module
```

Sau khi phân tích dependency và responsibility, nhận thấy cách chia này làm pipeline bị phân mảnh và tạo quá nhiều điểm phụ thuộc.

Kiến trúc được tổ chức lại thành Business Module.

```text
Reading

Capture

Recognition

Text Processing

Translation

Presentation

Storage
```

Business Module phản ánh đúng trách nhiệm nghiệp vụ thay vì kỹ thuật xử lý.

---

## 27.4 Recognition thay thế OCR

Ban đầu OCR được coi là trung tâm của pipeline.

Sau nhiều lần phân tích, nhận thấy OCR chỉ là một bước trong quá trình nhận diện nội dung.

Recognition được mở rộng để bao gồm:

- OCR;
- Reading Order;
- Region Mapping;
- Layout Analysis;
- Traceability.

OCR từ đó trở thành một implementation bên trong Recognition thay vì một Module độc lập.

---

## 27.5 Text Processing được tách khỏi Translation

Ban đầu Translation nhận trực tiếp kết quả OCR hoặc Text Extraction.

Mô hình này làm Translation phải xử lý nhiều công việc không thuộc trách nhiệm của nó như:

- chuẩn hóa văn bản;
- ghép dòng;
- nhóm đoạn;
- sửa reading order;
- tạo document.

Sau nhiều lần phân tích, Text Processing được tách thành một Business Module riêng.

Pipeline hiện tại:

```text
Recognition
    ↓
Text Processing
    ↓
Translation
```

Translation chỉ làm việc với SourceDocument.

Điều này giúp giảm coupling giữa Recognition và Translation, đồng thời chuẩn hóa dữ liệu đầu vào cho toàn bộ hệ thống.

---

## 27.6 Translation tách khỏi Presentation

Một thay đổi quan trọng khác là tách hoàn toàn Translation khỏi Presentation.

Translation chịu trách nhiệm:

- tạo Translation Result;
- quản lý Provider;
- Retry;
- Context;
- Glossary;
- Translation Memory.

Presentation chịu trách nhiệm:

- Side Panel;
- Overlay;
- Font;
- Layout;
- Rendering;
- User Interaction.

Nhờ đó có thể thay đổi cách hiển thị mà không ảnh hưởng đến quá trình dịch.

---

## 27.7 Event Bus không còn điều phối Pipeline

Ở giai đoạn đầu từng cân nhắc để Event Bus tự kích hoạt bước tiếp theo.

Ví dụ:

```text
OCR Completed
        ↓
Segmentation
        ↓
Translation
```

Sau khi đánh giá, cách làm này làm luồng điều khiển bị phân tán và khó kiểm soát.

Kiến trúc hiện tại quy định:

- Event Bus chỉ truyền thông tin;
- Pipeline Orchestrator quyết định bước tiếp theo;
- Scheduler quyết định thời điểm thực thi.

Nhờ đó pipeline có một điểm điều phối duy nhất.

---

## 27.8 Runtime ngày càng hoàn chỉnh

Runtime ban đầu chỉ bao gồm:

- Scheduler;
- Queue.

Trong quá trình phân tích, Runtime dần bổ sung thêm:

- Runtime Control;
- Revision;
- WorkItem;
- Attempt;
- Artifact Store;
- Cancellation;
- Retry Policy;
- Memory Model;
- Resource Lifecycle;
- Performance Model;
- Error Model;
- Runtime Observability.

Runtime từ một cơ chế thực thi đơn giản đã phát triển thành lớp điều phối toàn bộ vòng đời pipeline.

---

## 27.9 Các quyết định giữ nguyên

Trong suốt quá trình phát triển, một số nguyên tắc chưa từng thay đổi:

- Documentation First;
- Capability First;
- User Experience First;
- Provider Independent;
- Explicit Ownership;
- Serializable Boundaries;
- Module là kết quả của phân tích.

Đây vẫn là nền tảng của toàn bộ kiến trúc CRAI.

---

## 27.10 Hướng phát triển tiếp theo

Sau khi hoàn thành Business Module Architecture, trọng tâm của dự án sẽ chuyển sang:

- hoàn thiện public contract;
- chuẩn hóa event;
- lựa chọn technology stack;
- process topology;
- implementation.

Các thay đổi kiến trúc lớn trong tương lai cần được bổ sung vào mục này để lưu lại quá trình tiến hóa của hệ thống.

# 28. Known Trade-offs

## 28.1 Mục đích

Không có kiến trúc nào tối ưu cho mọi bài toán.

Trong quá trình thiết kế CRAI, nhiều quyết định được lựa chọn dựa trên mục tiêu của MVP, thời gian phát triển và khả năng mở rộng trong tương lai.

Phần này ghi lại các trade-off đã được chấp nhận để tránh việc sau này AI hoặc lập trình viên khác "tối ưu" mà vô tình phá vỡ định hướng ban đầu.

---

## 28.2 Desktop First

### Quyết định

CRAI được phát triển dưới dạng Desktop Application trước.

### Ưu điểm

- Chủ động capture màn hình.
- Điều khiển Overlay dễ hơn.
- Truy cập Accessibility API.
- Hỗ trợ Clipboard và Global Hotkey.
- Không phụ thuộc trình duyệt.

### Hạn chế

- Phải xử lý đa nền tảng.
- Khó phân phối hơn Web.
- Overlay và DPI phức tạp hơn.

### Chấp nhận

Ưu tiên trải nghiệm đọc hơn khả năng triển khai nhanh.

---

## 28.3 Side Panel trước Overlay

### Quyết định

MVP ưu tiên Side Panel.

Overlay sẽ được phát triển sau.

### Lý do

Overlay yêu cầu:

- coordinate mapping
- DPI scaling
- click-through
- resize tracking
- capture exclusion

Side Panel đơn giản hơn rất nhiều nhưng vẫn đáp ứng mục tiêu đọc.

### Hạn chế

Trải nghiệm chưa liền mạch như Overlay.

---

## 28.4 Structured Text trước OCR

### Quyết định

Nếu có thể lấy text trực tiếp thì không OCR.

### Ưu điểm

- nhanh hơn
- ít lỗi hơn
- giữ paragraph
- giữ reading order

### Hạn chế

Cần nhiều adapter hơn cho từng nguồn dữ liệu.

---

## 28.5 Provider Independent

### Quyết định

Không thiết kế quanh PaddleOCR, RapidOCR hoặc Google Translate.

### Ưu điểm

- thay provider dễ
- dễ test
- giảm coupling

### Hạn chế

Phải xây abstraction từ đầu.

---

## 28.6 Event Bus chỉ truyền Event

### Quyết định

Event Bus không điều phối Pipeline.

Pipeline Orchestrator mới là nơi quyết định workflow.

### Ưu điểm

- workflow tập trung
- dễ debug
- dễ trace

### Hạn chế

Runtime Control phức tạp hơn.

---

## 28.7 Runtime Control là Single Logical Writer

### Quyết định

Runtime state chỉ có một logical owner.

### Ưu điểm

- tránh race condition
- authority rõ ràng
- dễ reasoning

### Hạn chế

Runtime Control trở thành thành phần quan trọng nhất của hệ thống.

---

## 28.8 Artifact Reference thay vì truyền dữ liệu lớn

### Quyết định

Module chỉ truyền ArtifactRef.

Không truyền Image hoặc OCR Result trực tiếp.

### Ưu điểm

- giảm copy memory
- queue nhỏ
- worker độc lập

### Hạn chế

Cần Artifact Store và lifecycle phức tạp hơn.

---

## 28.9 Business Module thay vì Technical Module

### Quyết định

Module được chia theo trách nhiệm nghiệp vụ.

Không chia theo kỹ thuật.

Ví dụ:

Recognition

thay vì

OCR Module.

### Ưu điểm

- boundary rõ
- ít dependency
- dễ mở rộng

### Hạn chế

Một module sẽ lớn hơn so với cách chia kỹ thuật.

---

## 28.10 Kết luận

Các trade-off trên đều là quyết định có chủ đích.

Nếu trong tương lai muốn thay đổi, cần đánh giá lại toàn bộ ảnh hưởng đến:

- Capability
- Runtime
- Module Boundary
- Public Contract
- User Experience

thay vì chỉ tối ưu một thành phần riêng lẻ.

# 29. Architecture Backlog

## 29.1 Purpose

This section records architecture work that remains after the Runtime v2 and Recognition v2 consolidation.

Completed work is not kept as an active backlog item.

---

## 29.2 High Priority

### Detailed Recognition/OCR Architecture Synchronization

**Status:** In Progress

**Scope:**

```text
doc/01-architecture/ocr/
├── PIPELINE.md
├── PREPROCESS.md
├── DETECTION.md
├── RECOGNITION.md
├── POSTPROCESS.md
├── LAYOUT.md
├── READING_ORDER.md
├── TEXT_DIRECTION.md
├── QUALITY.md
└── PROVIDERS.md
```

**Goal:**

- align detailed OCR architecture with the Recognition module boundary;
- use `RecognitionArtifact` rather than legacy OCR Result terminology where appropriate;
- keep algorithms and provider mechanics in `01-architecture/ocr/`;
- keep module ownership, public contracts, states, events and errors in `02-modules/recognition/`;
- prevent detailed stages from owning Runtime scheduling, retry, cancellation, authority or publication;
- align provider descriptions with capability-based Provider Manager boundaries.

---

### Recognition → Text Processing Boundary Review

**Status:** Planned after OCR architecture synchronization

**Goal:**

- verify the exact published Recognition Artifact consumed by Text Processing;
- remove assumptions that Text Processing receives raw OCR strings;
- preserve geometry, order, confidence, provenance and source traceability;
- keep semantic reconstruction outside Recognition;
- verify downstream WorkItem creation remains Runtime-orchestrated.

---

### Cross-Module Runtime v2 Terminology Review

**Status:** Planned

**Modules to review:**

- Capture
- Text Processing
- Translation
- Presentation
- Reading Session
- Preferences
- Diagnostics
- UI Adapter

**Goal:**

Remove or reconcile legacy concepts such as:

```text
request-owned lifecycle
module-owned retry
module-owned cancellation registry
module terminal event as execution authority
Result object containing task status
direct downstream event triggering
provider lifecycle owned by business module
```

---

## 29.3 Medium Priority

### Provider Architecture

Define:

- Provider Manager boundary;
- provider registry and capability discovery;
- provider health/lifecycle;
- credential resolution;
- local/remote execution policies;
- Recognition and Translation adapter contracts;
- provider capacity and isolation.

Detailed provider selection must remain separate from business semantics.

---

### Process Topology

Decide:

- single process or multi-process;
- provider/model isolation;
- native capture process requirements;
- shared-memory versus file/reference transfer;
- worker crash recovery;
- GPU/native resource ownership.

---

### Technology Selection

Decide only after architecture synchronization:

- desktop framework;
- core language/runtime;
- UI framework;
- OCR/Recognition implementation candidates;
- local database/persistence technology;
- build system;
- dependency enforcement;
- testing stack.

---

### Security and Privacy Implementation Plan

Define implementation choices for:

- secret storage;
- remote-processing consent;
- privacy modes;
- protected diagnostics;
- temporary Artifact cleanup;
- support bundle redaction;
- local-only guarantees.

---

## 29.4 Future

Outside MVP:

- Browser Extension;
- Plugin Marketplace;
- Story Library;
- Image Replacement;
- Inpainting;
- OCR model training;
- distributed Runtime;
- distributed Artifact Store;
- cloud synchronization;
- remote collaborative workspace;
- dynamic third-party plugin loading.

---

## 29.5 Conditions Before Implementation

Implementation should begin only after:

1. detailed Recognition/OCR architecture is synchronized;
2. Recognition → Text Processing boundary is verified;
3. remaining module documents are checked for Runtime v2 conflicts;
4. public contracts and Event Convention are internally consistent;
5. technology and process-topology decisions are recorded explicitly;
6. MVP acceptance and test strategy are defined.

The architecture is already broad enough for implementation planning, but starting production code before these synchronization steps may reintroduce conflicting ownership models.

---

## 30. Presentation Module Architecture Completed

**Status:** ✅ Completed

### Overview

Completed the first full architectural version of the Presentation module.

The module architecture has been refactored from the original PresentationDocument-centric design into the new PresentationSnapshot + RenderPlan architecture, establishing Presentation as a pure business module independent from rendering platforms.

---

### Architecture Changes

#### Core Model Refactoring

Replaced the original architecture:

Reading Session
→ Presentation
→ PresentationDocument
→ UI

with the new architecture:

Reading Session
→ Presentation
→ PresentationSnapshot
→ RenderPlan
→ UI Adapter

This separates presentation state from rendering execution and clearly defines ownership boundaries.

---

#### Unified Terminology

Standardized module terminology across all Presentation documents.

Introduced:

- PresentationSnapshot
- RenderPlan
- PresentationContextId
- PresentationProfile
- PresentationTarget
- PresentationMode
- PresentationStrategy
- PresentationRevision

Removed legacy concepts:

- PresentationDocument
- PresentationResult
- LayoutDocument

---

### Documentation Completed

The following documents have been rewritten and synchronized.

#### MODULE.md

Completed:

- module responsibilities
- architecture boundaries
- ownership definition
- public concepts
- dependency definition

---

#### CONTRACT.md

Completed:

- command contracts
- query contracts
- command lifecycle
- immutable outputs
- operation ownership

Primary commands:

- BuildPresentation
- UpdatePresentationContent
- RecomputePresentationLayout
- UpdatePresentationFocus
- ApplyPresentationProfile
- ChangePresentationMode
- ClearPresentation

---

#### STATES.md

Completed:

Presentation lifecycle:

Empty
→ Preparing
→ Ready
→ Updating
→ Reflowing
→ Clearing
→ Empty

Failure path:

→ Failed

State transitions synchronized with command contracts.

---

#### EVENTS.md

Completely redesigned.

Defined:

Consumed Events

- SessionContentAccepted
- TranslationUpdated
- TranslationCompleted
- ViewportChanged
- PresentationPreferenceChanged
- PresentationProfileChanged

Published Events

- PresentationPrepared
- PresentationUpdated
- PresentationLayoutChanged
- PresentationModeChanged
- PresentationRejected
- PresentationFailed
- PresentationCleared

Also completed:

- event ownership
- event envelope
- revision ownership
- ordering rules
- idempotency
- retry behavior
- observability
- testing requirements

---

#### ERRORS.md

Completely rewritten.

Defined:

- error philosophy
- ownership
- error categories
- severity model
- retry policies
- recovery policies
- fallback policies
- diagnostics
- logging
- metrics
- compatibility rules
- architecture invariants

Introduced stable error families:

- PRS-VAL
- PRS-CTX
- PRS-REV
- PRS-GEO
- PRS-LAY
- PRS-MODE
- PRS-STATE
- PRS-EVENT
- PRS-RES
- PRS-REC
- PRS-INT

---

#### README.md

Rewritten as the module entry point.

Reduced duplicated information while providing navigation to:

- MODULE.md
- CONTRACT.md
- EVENTS.md
- STATES.md
- ERRORS.md

---

### Architectural Improvements

Established explicit ownership for revisions.

| Revision | Owner |
|----------|-------|
| ContentRevision | Reading Session |
| TranslationRevision | Translation |
| PreferenceRevision | Preferences |
| ProfileRevision | Preferences |
| ViewportRevision | UI Adapter |
| PresentationRevision | Presentation |

Presentation now owns only PresentationRevision.

---

### Module Characteristics

Presentation is now defined as:

- platform independent
- deterministic
- immutable
- event-driven
- revision-safe
- geometry-aware
- atomic
- UI independent

Rendering responsibility belongs exclusively to UI Adapters.

---

### Current Status

Presentation documentation is considered internally consistent.

The following documents are synchronized:

- README.md
- MODULE.md
- CONTRACT.md
- STATES.md
- EVENTS.md
- ERRORS.md

No known architectural inconsistencies remain within the Presentation module.

---

### Next Recommended Step

Continue with the next application module using the same documentation standard and ownership model established for Presentation.

---

# 31. Runtime v2 Architecture Consolidation Completed

**Status:** ✅ Completed  
**Completed:** 2026-08-03

## 31.1 Purpose

The Runtime document set was re-reviewed as one architecture rather than as isolated files.

The consolidation removed remaining Stage-centric and request-centric assumptions and standardized Runtime around:

```text
Revision
WorkItem
Attempt
Authority
Candidate Artifact
Ownership Transfer
Publication
Resource Lease
Retention
Logical Disposal
Physical Disposal
```

## 31.2 Documents Consolidated

```text
doc/01-architecture/runtime/
├── README.md
├── RUNTIME_COMPONENTS.md
├── PIPELINE_RUNTIME.md
├── BUSINESS_PIPELINE_ORCHESTRATION.md
├── SCHEDULER.md
├── WORK_QUEUE.md
├── CANCELLATION.md
├── RETRY_POLICY.md
├── ERROR_MODEL.md
├── MEMORY_MODEL.md
├── CACHE_POLICY.md
├── RESOURCE_LIFECYCLE.md
├── THREADING_MODEL.md
├── PERFORMANCE_MODEL.md
├── RUNTIME_OBSERVABILITY.md
├── RUNTIME_CONFIG.md
└── BOOT_SEQUENCE.md
```

`runtime/README.md` was created as the entry point for the complete Runtime document set.

## 31.3 Major Changes

### Work-centric Runtime

The canonical execution flow is now:

```text
BusinessExecutionPlan
    ↓
WorkItem
    ↓
Attempt
    ↓
Attempt Completion
```

Business-specific stages remain module semantics, not Runtime-owned state taxonomy.

### Centralized Authority

Runtime Control is the single logical owner for:

- Revision relevance;
- WorkItem and Attempt acceptance;
- cancellation authority;
- retry coordination;
- Candidate disposition;
- terminal outcome acceptance;
- publication coordination.

### Candidate and Publication Boundary

Workers and business modules produce Candidate Artifacts.

```text
Candidate Artifact
    ↓
Runtime Authority Validation
    ↓
Ownership Transfer
    ↓
Atomic Publication
```

A technically correct Candidate may still be rejected because it is stale, canceled, duplicate or unauthorized.

### Resource Lifecycle

Resource handling now distinguishes:

```text
creation
registration
payload ownership
retention ownership
Resource Lease
logical disposal
draining
physical disposal
```

Loss of logical authority does not imply immediate physical disposal.

### Performance and Observability

The main performance measure is useful current output, not raw throughput.

Runtime Observability now traces:

```text
Revision
    ↓
BusinessExecutionPlan
    ↓
WorkItem
    ↓
Attempt
    ↓
Authority Validation
    ↓
Ownership Transfer
    ↓
Publication
    ↓
Presentation Commit
```

### Configuration and Boot

Runtime Configuration now includes explicit policy groups for:

- scheduling;
- queues;
- retry;
- cancellation;
- authority;
- publication;
- resources;
- leases;
- cache;
- Storage;
- diagnostics.

Boot now initializes dependency-aware Runtime v2 components before opening admission.

## 31.4 Runtime v2 Invariants

1. Business modules own semantics; Runtime owns orchestration.
2. Worker never schedules downstream work directly.
3. Retry always creates a new Attempt.
4. Cancellation revokes authority before physical drain completes.
5. Candidate creation does not imply publication.
6. Artifact Store owns published shared payload.
7. Resource Lease grants temporary use, not ownership.
8. Retention and payload ownership are separate.
9. Logical disposal and physical disposal are separate.
10. Queue and concurrency remain bounded.
11. Current Revision is prioritized.
12. Observability failure does not break Runtime correctness.
13. Runtime events do not replace state ownership.
14. UI commit requires current authority.
15. Performance optimizations never bypass authority or ownership.

---

# 32. Recognition Module Runtime v2 Synchronization Completed

**Status:** ✅ Completed  
**Completed:** 2026-08-03

## 32.1 Documents Synchronized

```text
doc/02-modules/recognition/
├── README.md
├── MODULE.md
├── CONTRACT.md
├── STATES.md
├── EVENTS.md
└── ERRORS.md
```

## 32.2 Recognition Boundary

Recognition is now defined as:

```text
image-based input
    ↓
structured spatial source content
```

It owns:

- image Recognition semantics;
- region and line models;
- geometry and source-coordinate mapping;
- initial reading order;
- confidence and quality semantics;
- normalized provider output;
- Candidate Recognition Artifact construction;
- Recognition compatibility metadata;
- module warnings and errors.

It does not own:

- WorkItem/Attempt lifecycle;
- Scheduler or Queue;
- Runtime retry;
- cancellation authority;
- provider lifecycle;
- Artifact publication;
- Cache retention;
- durable persistence;
- semantic text reconstruction;
- Translation;
- Presentation.

## 32.3 Contract Changes

The old autonomous request/result model was replaced:

```text
RecognizeImage Request
    ↓
RecognitionResult
```

with:

```text
RecognitionAttemptInput
    ↓
RecognitionAttemptOutput
    ↓
CandidateRecognitionArtifact
    ↓
Runtime Authority Validation
    ↓
RecognitionArtifact
```

`RecognitionArtifact` is immutable and contains no task status, retry count, queue timing or terminal execution state.

## 32.4 State Changes

Recognition-owned states are now limited to:

```text
RecognitionAvailabilityState
RecognitionPlanState
RecognitionOperationPhase
CandidateValidationState
RecognitionQualityState
RecognitionCompleteness
ProviderExecutionObservation
```

Runtime owns WorkItem, Attempt, retry, cancellation and terminal outcome.

Provider Manager owns provider lifecycle and health.

Artifact Store owns publication and shared payload lifecycle.

## 32.5 Event Changes

Recognition no longer emits authoritative terminal lifecycle events such as:

```text
recognition.completed
recognition.failed
recognition.cancelled
```

Recognition may emit optional content-free facts:

```text
RECOGNITION_PLAN_CREATED
RECOGNITION_PREPARATION_COMPLETED
RECOGNITION_REGIONS_DETECTED
RECOGNITION_PROVIDER_OUTPUT_NORMALIZED
RECOGNITION_READING_ORDER_RESOLVED
RECOGNITION_CANDIDATE_VALIDATED
RECOGNITION_WARNING_RECORDED
RECOGNITION_MODULE_ERROR_RECORDED
```

These facts do not grant authority, publish Artifact or trigger Text Processing directly.

## 32.6 Error Changes

Recognition Errors now distinguish:

```text
RecognitionWarning
RecognitionModuleError
RetryHint
ProviderErrorRef
```

Expected/degraded outcomes such as:

- no readable text;
- low confidence;
- uncertain reading order;
- inferred geometry;
- safely suppressed overlapping regions;

are warnings or quality/completeness metadata when a usable Candidate still exists.

Runtime decides retry and Attempt disposition.

## 32.7 Recognition Documents vs Detailed OCR Documents

The project now keeps a clear division:

```text
doc/02-modules/recognition/
    → module boundary, public contracts, states, events and errors

doc/01-architecture/ocr/
    → internal Recognition/OCR pipeline, preprocessing, detection,
      provider mechanics, quality and reading-order algorithms
```

The next task is to synchronize the detailed `ocr/` documents with this completed module boundary.

---

# 33. Current Next Step

Continue with:

```text
doc/01-architecture/ocr/PIPELINE.md
```

Review goals:

1. align detailed pipeline with `RecognitionAttemptInput`;
2. produce `CandidateRecognitionArtifact`, not legacy `RecognitionResult`;
3. keep operation phases diagnostic and Attempt-local;
4. prevent detailed OCR stages from owning scheduling, retry, cancellation or publication;
5. preserve source-coordinate mapping;
6. align provider selection with capability requirements and Provider Manager;
7. verify output compatibility with Text Processing.

