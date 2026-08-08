# CRAI — Project Status

**Updated:** 2026-08-07
**Project key:** `CRAI`

> Đây là điểm vào (entry point) của dự án. Chỉ giữ trạng thái hiện tại và bước tiếp theo. Mọi chi tiết thuộc về các tài liệu chuyên biệt.

---

# 1. Executive Summary

## Project

CRAI là ứng dụng desktop hỗ trợ đọc và dịch truyện, tài liệu và nội dung trên màn hình với mục tiêu giảm tối đa gián đoạn khi đọc.

Các nguyên tắc đã chốt:

- Documentation First
- Capability First
- User Experience First
- Provider Independent
- Explicit Ownership
- Serializable Boundaries
- Privacy by Default

---

# 2. Architecture Status

| Area | Status |
|---|---|
| Project Foundation | ✅ Complete |
| Product Analysis | ✅ Complete |
| Capability Analysis | ✅ Complete |
| Core Architecture | ✅ Complete |
| Runtime Architecture | ✅ Complete |
| Business Modules | ✅ Complete |
| Infrastructure | ✅ Complete |
| Technology Selection | ⏳ Not Started |
| Implementation | ❌ Not Started |

---

# 3. Business Modules

- ✅ Reading
- ✅ Capture
- ✅ Recognition
- ✅ Text Processing
- ✅ Translation
- ✅ Presentation
- ✅ Storage
- ✅ Preferences
- ✅ Diagnostics
- ✅ UI Adapter

---

# 4. Infrastructure Modules

- ✅ Configuration
- ✅ Secret Management
- ✅ Event Bus
- ✅ Logging
- ✅ Telemetry
- ✅ Scheduler
- ✅ Resource Manager

---

# 5. Current Architecture

Business Modules
→ Business semantics

Runtime
→ execution authority

Infrastructure
→ technical capabilities

Storage
→ persistence

---

# 6. Current Focus

1. Rà soát và đơn giản hóa toàn bộ tài liệu.
2. Đồng bộ terminology giữa Runtime, Architecture và Module.
3. Đối chiếu `doc/01-architecture/ocr/` với `doc/02-modules/recognition/`.
4. Loại bỏ nội dung trùng lặp giữa Architecture và Module.
5. Sau khi ổn định kiến trúc mới bắt đầu Technology Selection.

---

# 7. Reading Order

1. PROJECT_STATUS.md
2. AI_BOOT.md
3. PROJECT_RULE.md
4. Architecture document liên quan
5. Module document liên quan

---

# 8. Next Task

Tiếp tục:

`doc/01-architecture/ocr/PIPELINE.md`

Đối chiếu với:

`doc/02-modules/recognition/`

Mục tiêu:

- thống nhất ownership
- loại bỏ phần trùng
- giữ Runtime là execution authority
- giữ Module chỉ mô tả business responsibility

---

# 9. Maintenance Rules

PROJECT_STATUS chỉ ghi:

- trạng thái hiện tại
- tiến độ
- bước tiếp theo
- quyết định kiến trúc lớn

Không sao chép nội dung chi tiết từ:

- Capability
- Runtime
- Module
- Infrastructure

Mọi chi tiết phải được đọc từ tài liệu gốc.
