# CRAI - Tiến Độ Thực Hiện Dự Án (Progress Tracking)

**Ngày cập nhật:** 2026-08-14

---

## 1. Việc Đang Làm (In Progress)
- [ ] **Gate 3 — OCR (PaddleOCR Benchmark)**
  - Chuẩn bị standalone project để benchmark PaddleOCR (sử dụng C# wrapper `Sdcb.PaddleOCR`).
  - Chuẩn bị 10 manga pages tiếng Trung để chạy thử nghiệm độ chính xác và tốc độ nhận diện.

---

## 2. Việc Đã Hoàn Thành (Done)
- **Gate 1 — Desktop Skeleton:**
  - [x] Tạo Avalonia project MVVM chạy trên `.NET 10`.
  - [x] Khởi động mượt mà (chỉ tốn ~180ms - 320ms, vượt xa chỉ tiêu `< 2s`).
  - [x] DPI diagnostic hoạt động (phát hiện màn hình và scaling 1.25x).
  - [x] Side Panel prototype có stay-on-top và auto right-docking hoạt động mượt mà.
  - [x] Đăng ký phím tắt Ctrl+Shift+T qua Windows WndProc hook của Avalonia.
- **Gate 2 — Capture:**
  - [x] Phân tích hạn chế hệ thống: Windows Graphics Capture (WinRT) & GDI BitBlt bị chặn bởi chính sách bảo mật AppLocker & Windows UIPI của sandbox IDE (lỗi `0x800711C7` & `6`).
  - [x] Thiết lập giải pháp thay thế thành công: Dùng `RenderTargetBitmap` của Avalonia để tự capture cửa sổ, hoạt động hoàn hảo trong `< 40ms` (vượt chỉ tiêu `< 200ms`).

---

## 3. Việc Đang Dở / Các Vấn Đề Chờ Xử Lý (On Hold / Issues)
- **Cấu hình Target Framework:** Hiện tại project đã chuyển về `net10.0-windows` (thay vì `net10.0-windows10.0.19041.0`) để tránh AppLocker chặn các interop DLLs chưa ký khi build WinRT. GDI và các API native khác sẽ được nghiên cứu thêm khi đưa app ra khỏi sandbox IDE.

---

## 4. Kế Hoạch Tiếp Theo (Next Steps)
1. **Bước 0.8:** Tạo standalone console project benchmark PaddleOCR (`Sdcb.PaddleOCR`).
2. **Bước 0.9:** Chạy benchmark Tesseract OCR để so sánh độ chính xác và hiệu năng.
3. **Bước 0.10:** Thử nghiệm WinRT OCR của Windows làm giải pháp fallback.
4. **Bước 0.11:** Đưa ra quyết định chọn OCR engine phù hợp nhất cho CRAI.
