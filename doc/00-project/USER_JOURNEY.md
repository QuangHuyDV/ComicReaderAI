# USER_JOURNEY.md

Version: 0.1
Status: Draft

---

# Mục tiêu

Mô tả toàn bộ hành trình của người dùng khi sử dụng CRAI.

Đây KHÔNG phải tài liệu UI.

Đây là tài liệu mô tả:

- người dùng muốn gì
- hệ thống cần làm gì
- thứ tự các hành động
- điểm chờ
- dữ liệu đi qua đâu

Sau này:

USER JOURNEY
↓

SCREEN FLOW

↓

UI

↓

IMPLEMENTATION

---

# Nguyên tắc

Luôn thiết kế theo:

Người đọc truyện KHÔNG muốn bị ngắt mạch đọc.

Mọi thao tác phải:

- ít click
- ít suy nghĩ
- ít chờ

Nếu phải thao tác nhiều hơn 2 lần cho cùng một việc
→ cần xem lại thiết kế.

---

# Hai hành trình chính

CRAI có hai loại nội dung.

## 1. Text Reading Flow

Ví dụ:

- Web Novel
- Light Novel
- TXT
- EPUB
- HTML
- Copy Text

---

## 2. Image Reading Flow

Ví dụ:

- Manga
- Manhua
- Manhwa
- Ảnh
- Scan

Đây là hai luồng gần như độc lập.

Chỉ chia sẻ:

- Translation Engine
- Dictionary
- Glossary
- Cache
- AI

---

# USER JOURNEY

---

# Journey 1

Đọc truyện chữ

```
Open CRAI

↓

Chọn nguồn

↓

CRAI đọc nội dung

↓

OCR (nếu cần)

↓

Phân đoạn

↓

Dịch

↓

Hiển thị

↓

Người dùng đọc

↓

Cuộn trang

↓

Phát hiện nội dung mới

↓

Dịch phần mới

↓

Lặp
```

---

Điểm quan trọng

Không được dịch lại toàn bộ.

Chỉ dịch phần mới.

---

# Journey 2

Đọc truyện tranh

```
Open CRAI

↓

Chọn vùng đọc

↓

CRAI ghi nhớ vùng

↓

Theo dõi thay đổi

↓

Ảnh ổn định

↓

OCR

↓

Ghép bubble

↓

Dịch

↓

Hiển thị

↓

Người dùng lật trang

↓

Phát hiện trang mới

↓

Lặp
```

---

Điểm quan trọng

Không OCR liên tục.

Chỉ OCR khi ảnh đã ổn định.

---

# Journey 3

Dịch ảnh thủ công

```
Open CRAI

↓

Kéo ảnh vào

↓

OCR

↓

Dịch

↓

Hiển thị

↓

Xuất kết quả
```

---

# Journey 4

Copy đoạn văn

```
Copy

↓

CRAI phát hiện

↓

Translate

↓

Popup

↓

Copy bản dịch
```

---

# Journey 5

Tra cứu

```
Chọn từ

↓

Dictionary

↓

Glossary

↓

AI Explanation
```

---

# Journey 6

Sửa bản dịch

```
Người dùng sửa

↓

Lưu

↓

Glossary cập nhật

↓

Lần sau ưu tiên cách dịch này
```

---

# Journey 7

Đổi Provider

```
Open Settings

↓

Chọn Provider

↓

Kiểm tra

↓

Áp dụng

↓

Provider mới hoạt động
```

---

# Journey 8

Khởi động lần đầu

```
Install

↓

Open

↓

Chọn ngôn ngữ

↓

Thiết lập OCR

↓

Thiết lập AI

↓

Hoàn tất
```

Không yêu cầu cấu hình phức tạp.

---

# Journey 9

Tiếp tục phiên đọc

```
Open CRAI

↓

Khôi phục Session

↓

Khôi phục Region

↓

Tiếp tục theo dõi

↓

Đọc tiếp
```

---

# Journey 10

Mất OCR

```
OCR thất bại

↓

Retry

↓

Nếu vẫn lỗi

↓

Thông báo

↓

Cho phép OCR thủ công
```

---

# Journey 11

Mất kết nối AI

```
Translate thất bại

↓

Retry

↓

Fallback Provider

↓

Thông báo
```

---

# Journey 12

Thoát

```
Close

↓

Lưu Session

↓

Lưu Cache

↓

Thoát
```

---

# Các trạng thái của CRAI

```
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

# Chuyển trạng thái

```
Idle

↓

Watching

↓

Capturing

↓

OCR

↓

Translation

↓

Rendering

↓

Watching
```

---

# Các sự kiện người dùng

- Open App
- Close App
- Select Region
- Change Region
- Scroll
- Next Page
- Previous Page
- Retry
- Pause
- Resume
- Edit Translation
- Copy
- Export
- Change Provider
- Settings

---

# Các sự kiện hệ thống

- Screen Changed
- Stable Image
- OCR Finished
- Translation Finished
- Cache Hit
- Cache Miss
- Provider Timeout
- Provider Error
- Session Saved
- Session Restored

---

# Những quyết định chưa chốt (Open Questions)

## Chế độ đọc

- Overlay trực tiếp lên trang?
- Panel cạnh màn hình?
- Cửa sổ nổi?
- Tách cửa sổ độc lập?

## Điều khiển

- Hotkey?
- Chuột?
- Auto?

## Đồng bộ

- Có cần nhiều Session?
- Có cần đồng bộ nhiều cửa sổ đọc cùng lúc?

## Khả năng mở rộng

- Một phiên đọc có thể kết hợp cả Text Reading và Image Reading trong tương lai không?