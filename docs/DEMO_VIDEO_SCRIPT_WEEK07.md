# KỊCH BẢN QUAY VIDEO DEMO BÁO CÁO THỰC HÀNH TUẦN 07
## HỆ THỐNG QUẢN LÝ KINH DOANH (QTKD) - PHASE 2

- **Môn học:** Phát triển ứng dụng Hệ thống thông tin hiện đại
- **Sinh viên:** Dương Việt Dũng - MSSV: 23120241
- **GitHub Repository:** [Duong-Dung/QTKD_23120241](https://github.com/Duong-Dung/QTKD_23120241)
- **Thời lượng dự kiến:** 4 - 6 phút
- **Công cụ khuyến nghị:** OBS Studio hoặc Windows Game Bar (`Win + Alt + R`), dùng tai nghe/mic rõ tiếng.

---

## TÓM TẮT DÀN Ý VIDEO DEMO (MỤC 5.4 ĐỀ BÀI)
1. **Phần 1 (0:00 - 0:45):** Giới thiệu thông tin sinh viên, mục tiêu tuần 07 và tổng quan cấu trúc thư mục Solution.
2. **Phần 2 (0:45 - 2:00):** Chạy bộ kiểm thử tự động xUnit (`dotnet test`), trình bày bảng đối chiếu TC01 – TC08 (24 tests Pass) và file báo cáo TRX.
3. **Phần 3 (2:00 - 3:00):** Báo cáo lỗi phát hiện được trong quá trình kiểm thử, giải thích nguyên nhân và minh chứng Git commit sửa lỗi (`438b826`).
4. **Phần 4 (3:00 - 4:45):** Chạy thực tế Web API và kiểm tra luồng xuyên suốt (Tạo Customer -> Lập Contract Draft -> Gửi duyệt thiếu dữ liệu CTR-02 -> Bổ sung CTR-01 -> Gửi lại CTR-03 -> Chống gửi lặp).
5. **Phần 5 (4:45 - 5:00):** Kết luận và nghiệm thu.

---

## KỊCH BẢN CHI TIẾT TỪNG PHÂN CẢNH

### PHẦN 1: MỞ ĐẦU & GIỚI THIỆU CẤU TRÚC DỰ ÁN (0:00 - 0:45)

- **Màn hình hiển thị:** Mở VS Code tại thư mục `23120241_TH7`, hiển thị cây thư mục bên trái và mở sẵn file `README.md`.
- **Thao tác:** Dùng chuột lướt qua các thư mục `QTKD.Api`, `tests/QTKD.Tests`, `docs`, `README.md`.
- **Lời thoại / Thuyết minh:**
  > *"Em xin kính chào Thầy/Cô. Em tên là Dương Việt Dũng, mã số sinh viên 23120241. Hôm nay em xin phép quay video báo cáo kết quả thực hành Tuần 07 - Phase 2 môn Phát triển ứng dụng Hệ thống thông tin hiện đại.*
  >
  > *Ở tuần 07, mục tiêu chính là xây dựng bộ kiểm thử tự động cho hệ thống Quản lý kinh doanh QTKD đã hoàn thành ở tuần 06, sửa các lỗi phát hiện được và chuẩn bị phiên bản chạy ổn định.*
  >
  > *Dự án của em được kế thừa xuyên suốt từ tuần 05 và tuần 06, tổ chức theo đúng quy định gồm có: project Web API `QTKD.Api`, project kiểm thử riêng biệt `tests/QTKD.Tests`, thư mục `docs` chứa toàn bộ tài liệu đặc tả cùng file kết quả kiểm thử TRX, và file `README.md` hướng dẫn chi tiết."*

---

### PHẦN 2: CHẠY BỘ KIỂM THỬ TỰ ĐỘNG XUNIT (0:45 - 2:00)

- **Màn hình hiển thị:** Mở Terminal trong VS Code và mở file `docs/TEST_REPORT_WEEK07.md`.
- **Thao tác:**
  1. Mở file `tests/QTKD.Tests/QTKD.Tests.csproj` và lướt qua 2 thư mục `UnitTests/` và `IntegrationTests/`.
  2. Gõ và chạy lệnh:
     ```bash
     dotnet test tests/QTKD.Tests/QTKD.Tests.csproj --logger "trx;LogFileName=TestResults.trx" --logger "console;verbosity=normal"
     ```
  3. Chỉ vào kết quả màn hình console hiển thị **24/24 Passed** và đường dẫn file `TestResults.trx`.
  4. Mở bảng kết quả TC01 - TC08 trong `docs/TEST_REPORT_WEEK07.md`.
- **Lời thoại / Thuyết minh:**
  > *"Tiếp theo, em xin phép trình bày bộ kiểm thử tự động xUnit trong thư mục `tests/QTKD.Tests`.*
  >
  > *Bộ test được em tách bạch rõ ràng thành hai phần:*
  > *- Thứ nhất là **UnitTests**: Kiểm tra trực tiếp các quy tắc nghiệp vụ ở tầng Service gồm CTR-01, CTR-02, CTR-03 và chống gửi duyệt trùng lặp.*
  > *- Thứ hai là **IntegrationTests**: Kiểm tra toàn diện ở tầng Controller API, xác minh cả mã HTTP Status, cấu trúc phản hồi `ApiResponse`, và kiểm tra dữ liệu thực tế được ghi nhận trong cơ sở dữ liệu.*
  > *- Cả hai phần đều sử dụng cơ sở dữ liệu InMemory biệt lập thông qua `TestDbContextFactory`, đảm bảo hoàn toàn không ảnh hưởng đến cơ sở dữ liệu thật.*
  >
  > *Bây giờ em sẽ chạy lệnh `dotnet test`:*
  > *(Đợi test chạy xong ~1 giây)*
  > *Như Thầy/Cô có thể thấy trên màn hình console, toàn bộ 24 ca kiểm thử đều đạt trạng thái **PASS 100%**, bao phủ toàn bộ 8 test case tối thiểu từ TC01 đến TC08 theo đề bài:*
  > *- TC01: Tạo Customer hợp lệ thành công và đọc lại từ DB.*
  > *- TC02: Chặn tạo Customer khi thiếu tên bắt buộc.*
  > *- TC03: Sửa hợp đồng ở trạng thái Draft thành công (CTR-01).*
  > *- TC04: Chặn sửa hợp đồng khi ở trạng thái PendingApproval (CTR-01).*
  > *- TC05: Chặn gửi duyệt khi thiếu tài liệu đính kèm, khách hàng ngưng hoạt động hoặc giá trị bằng 0 (CTR-02).*
  > *- TC06: Gửi duyệt thành công chuyển sang PendingApproval (CTR-03).*
  > *- TC07: Chặn hành vi cố ý đổi Draft thẳng sang Approved qua API cập nhật (CTR-03).*
  > *- TC08: Chống gửi duyệt trùng lặp khi hợp đồng đã ở PendingApproval.*
  >
  > *Kết quả kiểm thử cũng được tự động xuất ra file `docs/TestResults.trx` theo chuẩn của Visual Studio."*

---

### PHẦN 3: BÁO CÁO LỖI PHÁT HIỆN ĐƯỢC & COMMIT SỬA LỖI (2:00 - 3:00)

- **Màn hình hiển thị:** Mở file `docs/TEST_REPORT_WEEK07.md` (Mục 4) và mở tab Git/Terminal gõ `git log -n 3 --oneline`.
- **Thao tác:**
  1. Gõ lệnh:
     ```bash
     git show 438b826 --stat
     ```
  2. Mở file `QTKD.Api/Services/CustomerService.cs` (chỉ vào dòng `string.IsNullOrWhiteSpace(dto.Name)`).
  3. Mở file `QTKD.Api/Services/ContractService.cs` (chỉ vào đoạn kiểm tra `dto.Status == "Approved"`).
- **Lời thoại / Thuyết minh:**
  > *"Theo yêu cầu mục 5.4 của đề bài về việc ghi nhận lỗi phát hiện được và commit sửa lỗi, trong quá trình viết kiểm thử, em đã phát hiện và xử lý hai vấn đề nghiệp vụ tiềm ẩn từ tuần trước:*
  >
  > *1. **Lỗi thứ nhất ở `CustomerService`**: Trước đây chỉ kiểm tra `[Required]` ở DTO tầng Controller. Khi viết Unit Test gọi trực tiếp phương thức Service với tên rỗng, dữ liệu rỗng vẫn được lưu. Em đã bổ sung cơ chế phòng vệ độc lập tại `CustomerService.CreateCustomerAsync`: nếu `Name` rỗng thì lập tức ném `BusinessRuleException`.*
  >
  > *2. **Lỗi thứ hai về quy tắc CTR-03**: Khi client cố ý gửi yêu cầu cập nhật chuyển trạng thái sang `Approved` để bỏ qua quy trình duyệt, hệ thống cần phản hồi rõ ràng lỗi vi phạm CTR-03. Em đã bổ sung trường `Status` vào `UpdateContractDto` và cài đặt logic chặn tại `ContractService.UpdateContractAsync`, trả về mã lỗi `400 Bad Request` kèm thông điệp cảnh báo rõ ràng.*
  >
  > *Các sửa lỗi này đã được em đóng gói độc lập trong commit Git có mã hash là `438b826` với mô tả rõ ràng như trên màn hình."*

---

### PHẦN 4: CHẠY THỰC TẾ WEB API & KIỂM TRA LUỒNG DEMO (3:00 - 4:45)

- **Màn hình hiển thị:** Khởi động Web API trên terminal và mở trình duyệt tại `http://localhost:5293/swagger` (hoặc mở file `QTKD.Api/QTKD.Api.http`).
- **Thao tác:**
  1. Trên terminal gõ:
     ```bash
     dotnet run --project QTKD.Api
     ```
  2. Mở trình duyệt vào Swagger UI: `http://localhost:5293/swagger`.
  3. Thực hiện lần lượt kịch bản 6 bước:
     - **Bước 4.1 (Tạo Customer):** `POST /api/Customers` với tên `"Công ty Cổ phần Alpha 2026"` -> Bấm Execute -> Nhận `201 Created`, ghi nhớ `Id` (ví dụ: `3`).
     - **Bước 4.2 (Tạo Contract Draft):** `POST /api/Contracts` với `customerId = 3`, `totalAmount = 0`, `attachmentUrl = null` -> Execute -> Nhận `201 Created`, hợp đồng ở trạng thái `Draft` (ví dụ Id: `2`).
     - **Bước 4.3 (Gửi khi thiếu dữ liệu - CTR-02):** `POST /api/Contracts/2/submit` -> Execute -> Nhận lỗi HTTP `400 Bad Request` do vi phạm CTR-02 (thiếu tệp đính kèm và amount = 0), hợp đồng vẫn giữ nguyên `Draft`.
     - **Bước 4.4 (Bổ sung dữ liệu - CTR-01):** `PUT /api/Contracts/2` bổ sung `totalAmount = 85000000`, `attachmentUrl = "https://storage.qtkd.vn/alpha.pdf"` -> Execute -> Nhận HTTP `200 OK`.
     - **Bước 4.5 (Gửi lại thành công - CTR-03):** `POST /api/Contracts/2/submit` -> Execute -> Nhận HTTP `200 OK`, trạng thái chuyển từ `Draft` sang `PendingApproval`, có `submittedAt`.
     - **Bước 4.6 (Kiểm tra chống gửi lặp - Yêu cầu 12):** Bấm Execute `POST /api/Contracts/2/submit` thêm lần nữa -> Nhận lỗi HTTP `400 Bad Request` thông báo hợp đồng đã được gửi duyệt trước đó.
- **Lời thoại / Thuyết minh:**
  > *"Bây giờ em xin phép khởi động Web API và thực hiện demo trực quan luồng nghiệp vụ tuần 06 trên giao diện Swagger UI.*
  >
  > *Bước 1: Em tạo khách hàng mới qua `POST /api/Customers`. Kết quả trả về `201 Created` với mã Id là 3.*
  >
  > *Bước 2: Em lập hợp đồng sơ bộ qua `POST /api/Contracts` ở trạng thái Draft nhưng chưa có file đính kèm và giá trị bằng 0.*
  >
  > *Bước 3: Em thử gửi trình duyệt qua `POST /api/Contracts/{id}/submit`. Hệ thống lập tức trả về mã lỗi `400 Bad Request` theo đúng quy tắc CTR-02 vì thiếu tài liệu bắt buộc và giá trị hợp đồng bằng 0. Hợp đồng vẫn được giữ nguyên ở trạng thái Draft.*
  >
  > *Bước 4: Em tiến hành cập nhật bổ sung hồ sơ đính kèm và giá trị hợp đồng qua `PUT /api/Contracts/{id}`. Hệ thống cho phép cập nhật thành công vì hợp đồng đang ở Draft theo đúng quy tắc CTR-01.*
  >
  > *Bước 5: Em gửi trình duyệt lại. Lần này hợp đồng đã hợp lệ và được chuyển sang trạng thái `PendingApproval` kèm thời gian `submittedAt` theo quy tắc CTR-03.*
  >
  > *Bước 6: Em thử bấm gửi duyệt lại một lần nữa. Hệ thống phát hiện hợp đồng đã ở `PendingApproval` và từ chối xử lý trùng theo Yêu cầu 12.*
  >
  > *Mọi dữ liệu đều được lưu trữ và kiểm soát nhất quán trong cơ sở dữ liệu PostgreSQL."*

---

### PHẦN 5: TỔNG KẾT & KẾT THÚC (4:45 - 5:00)

- **Màn hình hiển thị:** Mở trang repository GitHub [Duong-Dung/QTKD_23120241](https://github.com/Duong-Dung/QTKD_23120241).
- **Thao tác:** Chỉ vào danh sách 3 commits mới nhất của Tuần 07.
- **Lời thoại / Thuyết minh:**
  > *"Toàn bộ mã nguồn, các ca kiểm thử tự động xUnit đạt 100% Pass và file báo cáo TRX đã được em đồng bộ đầy đủ lên GitHub repository cá nhân tại địa chỉ Duong-Dung/QTKD_23120241.*
  >
  > *Em xin chân thành cảm ơn Thầy/Cô đã theo dõi video demo báo cáo thực hành của em ạ!"*
