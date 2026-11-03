# KỊCH BẢN DEMO THỰC HÀNH TUẦN 06

Kịch bản này được thiết kế và thực thi nghiêm ngặt theo đúng yêu cầu tại **Mục 4.3 (Bước 13)** của tài liệu hướng dẫn Phase 2:
> *"13. Thực hiện demo theo thứ tự: tạo Customer → tạo Contract Draft → gửi khi thiếu dữ liệu (thất bại) → bổ sung dữ liệu → gửi lại (thành công) → xem trạng thái trong database."*

---

## 1. Chuẩn bị môi trường
- Đảm bảo cơ sở dữ liệu PostgreSQL đã được khởi động và áp dụng migration mới nhất:
  ```bash
  dotnet ef database update --project QTKD.Api
  ```
- Khởi động ứng dụng Web API:
  ```bash
  dotnet run --project QTKD.Api
  ```
- Truy cập Swagger UI tại: `http://localhost:<port>/swagger` (hoặc mở file [QTKD.Api.http](file:///d:/download/23120241_TH6/QTKD.Api/QTKD.Api.http) trên VS Code / Rider).

---

## 2. Các bước thực hiện Demo

### Bước 1: Tạo Customer mới
- **Mục đích:** Tạo một đối tác/khách hàng hợp lệ để chuẩn bị ký kết hợp đồng.
- **Thao tác:** Gửi HTTP `POST /api/customers`
- **Request Body:**
  ```json
  {
    "name": "Tập đoàn VinaTech Demo Tuần 06",
    "status": "Active"
  }
  ```
- **Kết quả trả về:** HTTP `201 Created`
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

---

### Bước 2: Tạo Contract ở trạng thái Draft (còn thiếu dữ liệu)
- **Mục đích:** Nhân viên kinh doanh mới nhập thông tin ban đầu của hợp đồng nhưng chưa hoàn thiện giá trị hợp đồng và tài liệu đính kèm.
- **Thao tác:** Gửi HTTP `POST /api/contracts`
- **Request Body:**
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
- **Kết quả trả về:** HTTP `201 Created` (Hợp đồng được tạo thành công với trạng thái mặc định là `Draft`)
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
      "totalAmount": 0.00,
      "terms": "",
      "attachmentUrl": "",
      "status": "Draft",
      "submittedAt": null
    },
    "errors": null
  }
  ```

---

### Bước 3: Gửi trình duyệt khi thiếu dữ liệu (Kiểm tra CTR-02 -> Thất bại)
- **Mục đích:** Thử gửi trình duyệt khi hợp đồng chưa có `totalAmount > 0`, chưa có `terms` và chưa có `attachmentUrl`.
- **Thao tác:** Gửi HTTP `POST /api/contracts/2/submit`
- **Kết quả trả về:** HTTP `400 Bad Request`
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
- **Đối chiếu trạng thái:** Kiểm tra hợp đồng vẫn giữ nguyên trạng thái `Draft`, dữ liệu không bị thay đổi sai lệch.

---

### Bước 4: Bổ sung dữ liệu còn thiếu cho hợp đồng
- **Mục đích:** Nhân viên kinh doanh cập nhật giá trị hợp đồng, nội dung điều khoản và tệp scan hợp đồng đính kèm.
- **Thao tác:** Gửi HTTP `PUT /api/contracts/2`
- **Request Body:**
  ```json
  {
    "contractNo": "HD-2026-DEMO-01",
    "startDate": "2026-04-01T00:00:00Z",
    "endDate": "2027-04-01T00:00:00Z",
    "totalAmount": 120000000,
    "terms": "Cung cấp bản quyền giải pháp Quản lý Kinh doanh Cloud và dịch vụ hỗ trợ 24/7",
    "attachmentUrl": "https://storage.qtkd.vn/contracts/HD-2026-DEMO-01.pdf"
  }
  ```
- **Kết quả trả về:** HTTP `200 OK` (Cập nhật thành công do hợp đồng đang ở trạng thái `Draft` - tuân thủ CTR-01)
  ```json
  {
    "success": true,
    "message": "Cập nhật hợp đồng thành công",
    "data": {
      "id": 2,
      "contractNo": "HD-2026-DEMO-01",
      "customerId": 3,
      "customerName": "Tập đoàn VinaTech Demo Tuần 06",
      "startDate": "2026-04-01T00:00:00Z",
      "endDate": "2027-04-01T00:00:00Z",
      "totalAmount": 120000000.00,
      "terms": "Cung cấp bản quyền giải pháp Quản lý Kinh doanh Cloud và dịch vụ hỗ trợ 24/7",
      "attachmentUrl": "https://storage.qtkd.vn/contracts/HD-2026-DEMO-01.pdf",
      "status": "Draft",
      "submittedAt": null
    },
    "errors": null
  }
  ```

---

### Bước 5: Gửi lại hợp đồng trình duyệt (Thành công -> Chuyển sang PendingApproval)
- **Mục đích:** Gửi lại hợp đồng khi mọi điều kiện CTR-02 đã được thỏa mãn.
- **Thao tác:** Gửi HTTP `POST /api/contracts/2/submit`
- **Kết quả trả về:** HTTP `200 OK` (Tuân thủ CTR-03: Chuyển sang `PendingApproval`)
  ```json
  {
    "success": true,
    "message": "Hợp đồng đã được gửi trình duyệt thành công",
    "data": {
      "contractId": 2,
      "contractNo": "HD-2026-DEMO-01",
      "previousStatus": "Draft",
      "newStatus": "PendingApproval",
      "submittedAt": "2026-11-03T13:42:15Z",
      "message": "Hợp đồng HD-2026-DEMO-01 đã được gửi trình duyệt thành công và chuyển sang trạng thái PendingApproval."
    },
    "errors": null
  }
  ```

---

### Bước 6: Xem trạng thái trong cơ sở dữ liệu
- **Mục đích:** Đối chiếu trạng thái hợp đồng đã được cập nhật chính xác trong cơ sở dữ liệu PostgreSQL.
- **Cách 1: Qua API:** Gửi HTTP `GET /api/contracts/2`
  ```json
  {
    "success": true,
    "message": "Lấy chi tiết hợp đồng thành công",
    "data": {
      "id": 2,
      "contractNo": "HD-2026-DEMO-01",
      "customerId": 3,
      "customerName": "Tập đoàn VinaTech Demo Tuần 06",
      "startDate": "2026-04-01T00:00:00Z",
      "endDate": "2027-04-01T00:00:00Z",
      "totalAmount": 120000000.00,
      "terms": "Cung cấp bản quyền giải pháp Quản lý Kinh doanh Cloud và dịch vụ hỗ trợ 24/7",
      "attachmentUrl": "https://storage.qtkd.vn/contracts/HD-2026-DEMO-01.pdf",
      "status": "PendingApproval",
      "submittedAt": "2026-11-03T13:42:15Z"
    },
    "errors": null
  }
  ```
- **Cách 2: Qua câu truy vấn SQL trực tiếp trong PostgreSQL:**
  ```sql
  SELECT "Id", "ContractNo", "Status", "TotalAmount", "SubmittedAt", "AttachmentUrl"
  FROM "Contracts"
  WHERE "Id" = 2;
  ```
  **Kết quả hiển thị:**
  | Id | ContractNo | Status | TotalAmount | SubmittedAt | AttachmentUrl |
  |---|---|---|---|---|---|
  | 2 | HD-2026-DEMO-01 | **PendingApproval** | 120000000.00 | 2026-11-03 13:42:15+00 | https://storage.qtkd.vn/... |

---

## 3. Các bước kiểm tra bổ sung các quy tắc an toàn

### Thử sửa hợp đồng khi đã ở PendingApproval (Kiểm tra CTR-01):
- Gửi `PUT /api/contracts/2` với nội dung sửa đổi.
- **Kết quả:** HTTP `400 Bad Request`
  ```json
  {
    "success": false,
    "message": "[CTR-01] Không thể chỉnh sửa hợp đồng khi ở trạng thái 'PendingApproval'. Chỉ cho phép sửa hợp đồng khi ở trạng thái 'Draft' hoặc 'Revision Requested'.",
    "data": null,
    "errors": [ ... ]
  }
  ```

### Thử gửi lặp lại hợp đồng khi đã ở PendingApproval (Kiểm tra Yêu cầu 12):
- Gửi `POST /api/contracts/2/submit` một lần nữa.
- **Kết quả:** HTTP `400 Bad Request`
  ```json
  {
    "success": false,
    "message": "Hợp đồng 'HD-2026-DEMO-01' đã được gửi trình duyệt trước đó (trạng thái hiện tại: PendingApproval). Không được gửi trình duyệt trùng lặp.",
    "data": null,
    "errors": [ ... ]
  }
  ```
