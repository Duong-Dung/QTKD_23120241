# HƯỚNG DẪN CHI TIẾT KỊCH BẢN TEST & DEMO THỰC HÀNH TUẦN 06

Tài liệu này được thiết kế để sinh viên tự kiểm tra (test) toàn bộ hệ thống trước khi demo và sử dụng làm **kịch bản trình bày trực tiếp (demo)** cho Giảng viên.

---

## 1. Hướng dẫn khởi động hệ thống trước khi test

### Bước 1.1: Khởi động Cơ sở dữ liệu PostgreSQL
- **Cách 1 (Dùng Docker Compose):**
  ```powershell
  docker compose up -d
  ```
- **Cách 2 (Dùng PostgreSQL cài trên máy):**
  Đảm bảo PostgreSQL đang chạy và cơ sở dữ liệu `qtkd_db` đã được tạo.

### Bước 1.2: Cấu hình chuỗi kết nối (nếu cần)
- Mở file [appsettings.json](file:///d:/download/23120241_TH6/QTKD.Api/appsettings.json).
- Kiểm tra cổng (`Port=5432` hoặc `5433`) và mật khẩu (`Password=...`) khớp với máy của bạn.

### Bước 1.3: Cập nhật Migration vào cơ sở dữ liệu
Tại thư mục gốc dự án (`d:\download\23120241_TH6`), mở terminal và chạy:
```powershell
dotnet ef database update --project QTKD.Api
```
> **Kết quả:** EF Core sẽ tự động áp dụng cả `InitialCreate` và `AddContractFieldsForWeek06` vào database, nạp sẵn dữ liệu mẫu.

### Bước 1.4: Khởi động Web API
```powershell
dotnet run --project QTKD.Api
```
- Khi ứng dụng chạy thành công, console sẽ hiển thị địa chỉ truy cập (ví dụ: `http://localhost:5293` hoặc cổng tương đương).
- Mở trình duyệt và truy cập: **`http://localhost:5293/swagger`** để mở giao diện Swagger UI.

---

## 2. Kịch bản Demo chính: Luồng xuyên suốt 6 bước (Mục 13 Đề bài)

> **Mục tiêu:** Mô phỏng quá trình Nhân viên Kinh doanh tạo khách hàng mới -> Lập hợp đồng mới ở dạng Draft -> Gửi duyệt thiếu thông tin (thất bại) -> Bổ sung thông tin -> Gửi duyệt lại (thành công) -> Kiểm tra trạng thái trong cơ sở dữ liệu.

---

### BƯỚC 1: Tạo Khách hàng mới (Customer)
- **Mục đích:** Tạo khách hàng hợp lệ trước khi lập hợp đồng.
- **Thao tác trên Swagger:**
  1. Tìm đến nhóm **Customers** -> Chọn `POST /api/Customers`.
  2. Bấm nút **Try it out**.
  3. Dán nội dung JSON sau vào Request body:
     ```json
     {
       "name": "Tập đoàn VinaTech Demo Tuần 06",
       "status": "Active"
     }
     ```
  4. Bấm **Execute**.
- **Kết quả mong đợi:** HTTP Status **`201 Created`**.
  ```json
  {
    "success": true,
    "message": "Tạo khách hàng thành công",
    "data": {
      "id": 3,
      "name": "Tập đoàn VinaTech Demo Tuần 06",
      "status": "Active"
    },
    "errors": null
  }
  ```
- **Giải thích khi demo:** *"Hệ thống đã lưu thành công khách hàng mới với `Id = 3` và trạng thái `Active`."*

---

### BƯỚC 2: Tạo Hợp đồng mới ở trạng thái Draft (còn thiếu dữ liệu)
- **Mục đích:** Khởi tạo hợp đồng nháp ban đầu, cố ý để trống tệp đính kèm và giá trị hợp đồng bằng 0 để kiểm tra tính chặt chẽ của quy trình.
- **Thao tác trên Swagger:**
  1. Tìm đến nhóm **Contracts** -> Chọn `POST /api/Contracts`.
  2. Bấm nút **Try it out**.
  3. Dán nội dung JSON sau:
     ```json
     {
       "contractNo": "HD-2026-DEMO-01",
       "customerId": 3,
       "startDate": "2026-04-01T00:00:00Z",
       "endDate": "2027-04-01T00:00:00Z",
       "totalAmount": 0,
       "terms": "",
       "attachmentUrl": ""
     }
     ```
  4. Bấm **Execute**.
- **Kết quả mong đợi:** HTTP Status **`201 Created`**.
  ```json
  {
    "success": true,
    "message": "Tạo hợp đồng thành công ở trạng thái Draft",
    "data": {
      "id": 2,
      "contractNo": "HD-2026-DEMO-01",
      "customerId": 3,
      "customerName": "Tập đoàn VinaTech Demo Tuần 06",
      "startDate": "2026-04-01T00:00:00Z",
      "endDate": "2027-04-01T00:00:00Z",
      "totalAmount": 0.0,
      "terms": "",
      "attachmentUrl": "",
      "status": "Draft",
      "submittedAt": null
    },
    "errors": null
  }
  ```
- **Giải thích khi demo:** *"Hợp đồng mới được tạo thành công với mã `Id = 2`, trạng thái mặc định luôn là `Draft`."*

---

### BƯỚC 3: Gửi hợp đồng trình duyệt khi thiếu dữ liệu (Kiểm tra quy tắc CTR-02)
- **Mục đích:** Thử gửi trình duyệt khi hợp đồng chưa đủ điều kiện bắt buộc theo quy tắc CTR-02.
- **Thao tác trên Swagger:**
  1. Tìm đến nhóm **Contracts** -> Chọn `POST /api/Contracts/{id}/submit`.
  2. Bấm nút **Try it out**.
  3. Nhập `id`: **`2`**.
  4. Bấm **Execute**.
- **Kết quả mong đợi:** HTTP Status **`400 Bad Request`**.
  ```json
  {
    "success": false,
    "message": "[CTR-02] Hợp đồng không đủ điều kiện gửi trình duyệt do thiếu thông tin hoặc thông tin không hợp lệ.",
    "data": null,
    "errors": [
      "Giá trị hợp đồng (TotalAmount) phải lớn hơn 0 VNĐ.",
      "Hợp đồng thiếu nội dung điều khoản bắt buộc (Terms) hoặc nội dung quá ngắn.",
      "Hợp đồng thiếu tệp tài liệu/hồ sơ đính kèm bắt buộc (AttachmentUrl)."
    ]
  }
  ```
- **Kiểm chứng an toàn dữ liệu:**
  - Gọi `GET /api/Contracts/2`.
  - Hợp đồng vẫn giữ nguyên trạng thái là **`Draft`**, không có dữ liệu nào bị sửa đổi sai lệch.
- **Giải thích khi demo:** *"Theo quy tắc CTR-02, khi thiếu giá trị hợp đồng, điều khoản hoặc tệp tài liệu đính kèm, hệ thống lập tức từ chối và giữ nguyên trạng thái Draft."*

---

### BƯỚC 4: Bổ sung dữ liệu còn thiếu cho Hợp đồng (Kiểm tra quy tắc CTR-01)
- **Mục đích:** Nhân viên kinh doanh bổ sung giá trị hợp đồng, nội dung điều khoản và tệp scan hợp đồng đính kèm.
- **Thao tác trên Swagger:**
  1. Tìm đến nhóm **Contracts** -> Chọn `PUT /api/Contracts/{id}`.
  2. Bấm nút **Try it out**.
  3. Nhập `id`: **`2`**.
  4. Dán nội dung JSON cập nhật:
     ```json
     {
       "contractNo": "HD-2026-DEMO-01",
       "startDate": "2026-04-01T00:00:00Z",
       "endDate": "2027-04-01T00:00:00Z",
       "totalAmount": 120000000,
       "terms": "Cung cấp bản quyền giải pháp Quản lý Kinh doanh Cloud và dịch vụ hỗ trợ kỹ thuật 24/7",
       "attachmentUrl": "https://storage.qtkd.vn/contracts/HD-2026-DEMO-01.pdf"
     }
     ```
  5. Bấm **Execute**.
- **Kết quả mong đợi:** HTTP Status **`200 OK`**.
  ```json
  {
    "success": true,
    "message": "Cập nhật hợp đồng thành công",
    "data": {
      "id": 2,
      "contractNo": "HD-2026-DEMO-01",
      "totalAmount": 120000000.0,
      "terms": "Cung cấp bản quyền giải pháp Quản lý Kinh doanh Cloud và dịch vụ hỗ trợ kỹ thuật 24/7",
      "attachmentUrl": "https://storage.qtkd.vn/contracts/HD-2026-DEMO-01.pdf",
      "status": "Draft",
      "submittedAt": null
    },
    "errors": null
  }
  ```
- **Giải thích khi demo:** *"Theo quy tắc CTR-01, vì hợp đồng đang ở trạng thái Draft nên hệ thống cho phép chỉnh sửa và cập nhật thành công."*

---

### BƯỚC 5: Gửi lại hợp đồng trình duyệt (Thành công -> Chuyển sang PendingApproval)
- **Mục đích:** Gửi lại hợp đồng sau khi đã hoàn thiện đầy đủ mọi điều kiện hợp lệ.
- **Thao tác trên Swagger:**
  1. Tìm đến nhóm **Contracts** -> Chọn `POST /api/Contracts/{id}/submit`.
  2. Bấm nút **Try it out**.
  3. Nhập `id`: **`2`**.
  4. Bấm **Execute**.
- **Kết quả mong đợi:** HTTP Status **`200 OK`**.
  ```json
  {
    "success": true,
    "message": "Hợp đồng đã được gửi trình duyệt thành công",
    "data": {
      "contractId": 2,
      "contractNo": "HD-2026-DEMO-01",
      "previousStatus": "Draft",
      "newStatus": "PendingApproval",
      "submittedAt": "2026-11-04T02:00:00Z",
      "message": "Hợp đồng HD-2026-DEMO-01 đã được gửi trình duyệt thành công và chuyển sang trạng thái PendingApproval."
    },
    "errors": null
  }
  ```
- **Giải thích khi demo:** *"Theo quy tắc CTR-03, khi gửi duyệt thành công, trạng thái hợp đồng được chuyển sang PendingApproval kèm thời điểm gửi `submittedAt`, không nhảy cóc sang Approved."*

---

### BƯỚC 6: Kiểm tra trạng thái trong Cơ sở dữ liệu
- **Mục đích:** Chứng minh dữ liệu đã thực sự được lưu vào PostgreSQL chứ không phải chỉ xử lý tạm trên bộ nhớ.
- **Cách 1: Qua API:**
  - Gọi `GET /api/Contracts/2`.
  - Kết quả: `status = "PendingApproval"`, `submittedAt` có giá trị thời gian thực.
- **Cách 2: Qua PostgreSQL (pgAdmin / DBeaver / terminal psql):**
  ```sql
  SELECT "Id", "ContractNo", "Status", "TotalAmount", "SubmittedAt", "AttachmentUrl"
  FROM "Contracts"
  WHERE "Id" = 2;
  ```
  **Kết quả hiển thị:**
  | Id | ContractNo | Status | TotalAmount | SubmittedAt | AttachmentUrl |
  |---|---|---|---|---|---|
  | 2 | HD-2026-DEMO-01 | **PendingApproval** | 120000000.00 | 2026-11-04 02:00:00+00 | https://storage.qtkd.vn/... |

---

## 3. Kịch bản Demo kiểm tra các Quy tắc Bảo vệ Nâng cao

### Kiểm tra Quy tắc CTR-01 (Chống sửa khi đã trình duyệt):
- **Thao tác:** Gọi `PUT /api/Contracts/2` với nội dung cố tình sửa đổi `totalAmount` hoặc `terms`.
- **Kết quả:** HTTP **`400 Bad Request`**
  ```json
  {
    "success": false,
    "message": "[CTR-01] Không thể chỉnh sửa hợp đồng khi ở trạng thái 'PendingApproval'. Chỉ cho phép sửa hợp đồng khi ở trạng thái 'Draft' hoặc 'Revision Requested'.",
    "data": null,
    "errors": [ ... ]
  }
  ```
- **Giải thích:** *"Quy tắc CTR-01 đã chặn thành công việc sửa đổi trái phép khi hợp đồng đang chờ duyệt."*

---

### Kiểm tra Yêu cầu 12 (Chống gửi duyệt trùng lặp):
- **Thao tác:** Tiếp tục gọi `POST /api/Contracts/2/submit` một lần nữa.
- **Kết quả:** HTTP **`400 Bad Request`**
  ```json
  {
    "success": false,
    "message": "Hợp đồng 'HD-2026-DEMO-01' đã được gửi trình duyệt trước đó (trạng thái hiện tại: PendingApproval). Không được gửi trình duyệt trùng lặp.",
    "data": null,
    "errors": [ ... ]
  }
  ```
- **Giải thích:** *"Đảm bảo tính Idempotent: Hợp đồng đã gửi duyệt thì không thể gửi lặp lại."*

---

### Kiểm tra Nghiệp vụ Bảng giá (Pricing):
1. **Tạo bảng giá:** Gọi `POST /api/PriceLists` với `Name: "Bảng giá Dịch vụ Cloud 2026"`, `EffectiveDate: "2026-06-01"`. (Trả về 201).
2. **Thêm mục giá hợp lệ:** Gọi `POST /api/PriceLists/1/items` với `ServiceCode: "SRV-SECURITY"`, `UnitPrice: 2500000`. (Trả về 201).
3. **Thêm mục giá lỗi (UnitPrice <= 0):** Gọi `POST /api/PriceLists/1/items` với `UnitPrice: -100`. (Trả về **400 Bad Request** với thông báo đơn giá phải lớn hơn 0).

---

## 4. Cách chạy nhanh bằng file `QTKD.Api.http`
Nếu bạn dùng **VS Code** (đã cài extension *REST Client*):
1. Mở file [QTKD.Api/QTKD.Api.http](file:///d:/download/23120241_TH6/QTKD.Api/QTKD.Api.http).
2. Click trực tiếp vào chữ **`Send Request`** màu xanh ở trên từng request.
3. Cửa sổ bên phải sẽ hiển thị trực quan toàn bộ HTTP Status, Header và Body JSON kết quả.
