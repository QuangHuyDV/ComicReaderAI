# Comic Reader AI (CRAI)

**Comic Reader AI (CRAI)** là trợ lý dịch thuật thông minh trên máy tính (Windows), được thiết kế tối ưu để đọc truyện tranh, chơi game hoặc đọc tài liệu tiếng nước ngoài (Trung/Anh) một cách liền mạch, không gián đoạn.

---

## ✨ Tính Năng Nổi Bật

1. **Dịch đè màn hình thông minh (Overlay)**:
   - Nhận diện và dịch đè văn bản tiếng Việt khớp trực tiếp lên vị trí của khung thoại gốc.
   - Hỗ trợ **gom gộp dòng (MergeLines)** theo khối hình học (speech bubble) để dịch cả câu trọn vẹn, tránh ngắt quãng.
2. **Tương tác xuyên thấu (Click-Through)**:
   - Cửa sổ dịch đè ở chế độ xuyên thấu hoàn toàn. Bạn có thể tự do cuộn chuột (scroll), di chuột và click tương tác với ứng dụng/trình duyệt truyện phía dưới mà không bị cản trở.
3. **Đa dạng kiểu hiển thị**:
   - *Dịch đè (Che 100%)*: Che tuyệt đối chữ gốc bằng nền tối giản.
   - *Dịch đè (Nhìn xuyên nền)*: Nền mờ bán trong suốt (độ đục ~65%) giúp giữ nguyên vẹn hình vẽ/artwork bên dưới.
   - *Bảng phụ bên cạnh (Side Panel)*: Hiển thị bản dịch song song trong sidebar cố định bên phải màn hình.
4. **Dịch liên tục tự động (Continuous Mode)**:
   - Tự động quét và cập nhật bản dịch sau mỗi 1 giây.
   - Cơ chế tự động ẩn/hiển thị overlay khi quét giúp ảnh chụp luôn sạch, không bị dịch đè lặp lại bản dịch cũ.
   - Nút nổi (Floating Bubble) không bị nhấp nháy (flicker) và luôn có sẵn để thao tác.
5. **Cơ chế định tuyến động (Central Router)**:
   - Dịch thuật tốc độ cao bằng Google Translate (miễn phí) hoặc chuyển tiếp sang Gemini AI (Glossary tu tiên) nếu cấu hình API Key.
   - Tự động fallback về Google Translate nếu Gemini gặp lỗi hoặc hết hạn mức.

---

## 🚀 Hướng Dẫn Sử Dụng Nhanh

Ứng dụng chạy độc lập (Self-contained), đã được đóng gói sẵn tại thư mục [`/app`](file:///f:/mydata/myproject/ComicReaderAI/app):

1. **Khởi chạy**: Kích đúp vào file [`app/Crai.Desktop.exe`](file:///f:/mydata/myproject/ComicReaderAI/app/Crai.Desktop.exe) để mở nút nổi màu xanh hình tròn (`CRAI`) ở góc màn hình.
2. **Kéo/Thả**: Click giữ chuột trái vào nút nổi để kéo di chuyển đến vị trí mong muốn trên màn hình.
3. **Kích hoạt Dịch**:
   - Click chuột trái vào nút nổi, hoặc nhấn phím tắt toàn cục **`Ctrl + Shift + T`** để quét toàn màn hình và dịch.
4. **Cài đặt & Tùy chỉnh**:
   - Click chuột phải vào nút nổi để mở Menu cài đặt:
     - Bật/tắt dịch gộp câu.
     - Bật/tắt dịch liên tục tự động.
     - Thay đổi thời gian tự đóng bản dịch (5 giây, 8 giây, 15 giây, ..., hoặc Vô hạn).
     - Thay đổi kiểu hiển thị (Dịch đè che 100%, nhìn xuyên nền, hoặc Bảng phụ bên cạnh).
     - Thay đổi Engine dịch (Google Translate hoặc Gemini AI).
     - Nhập hoặc cập nhật Gemini API Key nhanh.

---

## 🔑 Cấu Hình Gemini API Key

Bạn có thể nhập API Key của mình theo 2 cách cực kỳ đơn giản và bảo mật:
- **Cách 1 (Qua giao diện UI)**: Click chuột phải vào nút nổi -> chọn **`🔑 Cấu hình Gemini API Key...`** -> Dán key của bạn vào và chọn **Lưu lại**. Key sẽ được mã hoá bảo mật qua Windows DPAPI.
- **Cách 2 (Qua cấu hình)**: Mở file `appsettings.json` trong thư mục chạy, dán key của bạn vào mục `"GeminiApiKey": "AIzaSy..."`. Khi khởi chạy, app sẽ tự động mã hoá key vào file `secrets.dat` bảo mật và xoá key thô khỏi file cấu hình để bảo vệ key của bạn.

---

## 🛠️ Biên Dịch & Đóng Gói (Cho Nhà Phát Triển)

Ứng dụng được phát triển trên nền tảng **.NET 10** và **AvaloniaUI**.

Để biên dịch và đóng gói ứng dụng thành một file chạy duy nhất siêu sạch (Single-File) vào thư mục `/app` ở gốc dự án:
- Chạy script đóng gói trên PowerShell:
  ```powershell
  powershell -ExecutionPolicy Bypass -File .\publish.ps1
  ```
- Hoặc chạy qua Makefile (nếu có công cụ `make`):
  ```bash
  make publish
  ```
Lệnh này sẽ tự động tắt tiến trình app cũ đang chạy (tránh lock file DLL) và xuất ra file chạy đã nhúng sẵn Icon logo đẹp mắt tại thư mục `/app`.
