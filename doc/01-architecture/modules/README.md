# modules/README.md

# Module Architecture

## Purpose

Thư mục này mô tả kiến trúc module ở cấp hệ thống.

Mục tiêu là xác định:

- Hệ thống được chia thành những module nào.
- Quan hệ giữa các module.
- Dependency được phép.
- Boundary của từng module.

Không mô tả implementation hoặc API chi tiết của từng module.

---

# Scope

Bao gồm:

- Module Map.
- Module Dependency.
- Kiến trúc giao tiếp giữa các module.

Không bao gồm:

- Runtime.
- Scheduler.
- Thread.
- Source code.
- Public API của module.

---

# Documents

## MODULE_MAP.md

Định nghĩa toàn bộ module của hệ thống.

Ví dụ:

- Capture
- Observation
- Classification
- Extraction
- Understanding
- Translation
- Presentation
- Storage

Mô tả vai trò và boundary của từng module.

---

## MODULE_DEPENDENCY.md

Định nghĩa dependency giữa các module.

Trả lời:

- Module nào được phép phụ thuộc module nào.
- Dependency nào bị cấm.
- Quy tắc chống circular dependency.

---

# Reading Order

1. MODULE_MAP.md
2. MODULE_DEPENDENCY.md

---

# Relationship

Core Architecture

↓

Module Architecture

↓

Runtime

↓

Module Design

---

# Notes

Thư mục này chỉ mô tả kiến trúc ở cấp độ module.

Thiết kế chi tiết của từng module sẽ nằm trong:

```

doc/02-modules/

```

Mỗi module tại đó sẽ có tài liệu riêng mô tả:

- Responsibilities
- Public API
- Events
- Contracts
- Internal Components
- Dependencies