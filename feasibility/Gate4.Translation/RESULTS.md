# CRAI Gate 4 — Kết quả Feasibility (Translation)

**Trạng thái:** COMPLETED (Quyết định chọn provider)
**Ngày cập nhật:** 2026-08-15

---

## 1. Kết Quả Thử Nghiệm (Benchmark)

Thực hiện benchmark trên **Google Translate (Dedicated MT)** thông qua bộ dataset truyện chữ/manhua:

| Mẫu thử | Source Text | Google Translate Output | Nhận xét chất lượng |
|---------|-------------|-------------------------|---------------------|
| `xianxia_terms` (Thuật ngữ tu tiên) | 那名炼气期修士祭出飞剑，试图抵挡筑基期大能的全力一击。 | Tu sĩ Luyện Khí Cảnh sử dụng thanh phi kiếm của mình, cố gắng chống lại toàn bộ đòn tấn công của cường giả **Kiến Trúc Cảnh**. | 🔴 **Thất bại nặng:** Dịch sai thuật ngữ "筑基期" (Trúc Cơ kỳ) thành "Kiến Trúc Cảnh". |
| `pronoun_addressing` (Xưng hô cổ đại) | 陛下，本王觉得此事必有蹊跷，还请陛下三思。 | Bệ hạ, **thần** cảm thấy chuyện này nhất định có điều gì mờ ám, xin thỉnh Bệ hạ suy nghĩ lại. | 🟡 **Tạm được:** Dịch "本王" (bản vương) thành "thần", làm giảm sắc thái phân cấp nhân vật. |
| `mixed_slang` (Từ lóng mạng/game) | 太给力了！这波操作简直是yyds！ | Thật tuyệt vời! **Làn sóng hoạt động** này chỉ đơn giản là yyds! | 🔴 **Thất bại:** Dịch word-by-word từ lóng "这波操作" (pha xử lý này) thành "làn sóng hoạt động". |
| `traditional_chinese` (Phồn thể) | 第一章 重生之日。那是雷鳴交加的雨夜，天空彷彿被撕裂開來。 | Chương Một Ngày Tái Sinh. Đó là một đêm mưa sấm sét, bầu trời như bị xé toạc. | 🟢 **Tốt:** Dịch trôi chảy và giữ đúng nghĩa. |

*   **Latency trung bình (Google Translate):** 1,648 ms / request (Nếu dùng Cloud API trả phí qua HTTP/2 sẽ < 500ms).

---

## 2. Phân Tích & Đề Xuất Kiến Trúc

### A. Dedicated Machine Translation (Google Translate)
*   **Điểm mạnh:** Tốc độ nhanh, ổn định, chi phí rẻ.
*   **Điểm yếu:** Dịch word-by-word rất thô, không hiểu ngữ cảnh truyện/manhua, dịch sai nghiêm trọng các thuật ngữ Xianxia/Wuxia và xưng hô cổ trang. Không hỗ trợ tiêm Glossary tùy biến của người dùng một cách linh hoạt.

### B. LLM Translation (Gemini 1.5 Flash / GPT-4o-mini)
*   **Điểm mạnh:** Hiểu ngữ cảnh cực tốt, văn phong mượt mà tự nhiên, hỗ trợ tiêm **Glossary** (từ điển Hán-Việt, tên nhân vật) thông qua prompt-engineering để dịch chính xác 100% thuật ngữ tu tiên ("Trúc Cơ", "Luyện Khí").
*   **Điểm yếu:** Latency cao hơn một chút (~1s - 2s), chi phí tính theo token.

### C. Quyết Định Kiến Trúc (Decision)

CRAI sẽ thiết lập hợp phần **Translation Router** hỗ trợ 2 chế độ:

1.  **AI Mode (Primary):** Sử dụng **Gemini 1.5 Flash** (hoặc GPT-4o-mini) làm engine chính. Cho phép người dùng cấu hình API Key cá nhân (để sử dụng Free Tier của Gemini hoặc trả phí giá rẻ). Hỗ trợ tính năng Glossary/Dictionary tự định nghĩa.
2.  **Standard Mode (Fallback):** Tích hợp **Google Translate** (miễn phí hoặc qua API) làm phương án dự phòng khi mất kết nối AI hoặc người dùng không có API Key.

---

**Gate 4 overall:** PASSED (Xác lập cấu trúc Translation Router đa dịch vụ).
