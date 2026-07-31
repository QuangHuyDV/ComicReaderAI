# core/README.md

# Core Architecture

## Purpose

Thư mục này chứa các tài liệu mô tả kiến trúc cốt lõi của ComicReaderAI.

Các tài liệu trong thư mục này định nghĩa những nguyên lý nền tảng của toàn bộ hệ thống và được xem là nguồn tham chiếu chính cho mọi thiết kế sau này.

Các tài liệu tại đây không phụ thuộc vào framework, ngôn ngữ lập trình hoặc implementation.

---

# Scope

Bao gồm:

- Capability của hệ thống.
- Luồng dữ liệu.
- State Machine.
- Event Model.

Không bao gồm:

- Thiết kế module.
- Runtime implementation.
- Provider implementation.
- Chi tiết source code.

---

# Documents

## CAPABILITY_MAP.md

Định nghĩa toàn bộ khả năng (Capability) của hệ thống.

Trả lời:

- Hệ thống có thể làm gì.
- MVP bao gồm những gì.
- Những capability nào sẽ mở rộng sau.

---

## DATA_FLOW.md

Định nghĩa cách dữ liệu di chuyển trong toàn hệ thống.

Trả lời:

- Input đi qua các bước nào.
- Output được tạo như thế nào.
- Boundary giữa các giai đoạn xử lý.

---

## EVENT_BUS.md

Định nghĩa Event Model.

Trả lời:

- Event được publish như thế nào.
- Event được subscribe như thế nào.
- Quy tắc đặt tên Event.
- Ownership của Event.

---

## STATE_MACHINE.md

Định nghĩa trạng thái của Runtime và Reading Session.

Trả lời:

- Runtime có những state nào.
- Session chuyển trạng thái như thế nào.
- Transition hợp lệ.

---

# Reading Order

1. CAPABILITY_MAP.md
2. DATA_FLOW.md
3. EVENT_BUS.md
4. STATE_MACHINE.md

---

# Dependency

Core Architecture không phụ thuộc vào bất kỳ tài liệu nào khác.

Các phần khác của hệ thống phải tuân theo các nguyên tắc được định nghĩa trong thư mục này.