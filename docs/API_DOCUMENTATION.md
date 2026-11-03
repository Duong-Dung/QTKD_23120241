# TÀI LIỆU CHI TIẾT API HỆ THỐNG QTKD (TUẦN 06)

**Hệ thống:** Quản lý Kinh doanh (QTKD)  
**Công nghệ:** ASP.NET Core Web API 10 · PostgreSQL · Entity Framework Core  
**Định dạng dữ liệu:** JSON (`application/json`)  
**Mô hình phản hồi chuẩn (Response Wrapper):** Thống nhất cho toàn bộ hệ thống theo cấu trúc:
```json
{
  "success": true,               // true nếu thành công, false nếu có lỗi
  "message": "Thông báo mô tả kết quả xử lý",
  "data": { ... },              // Dữ liệu đối tượng trả về (null nếu có lỗi)
  "errors": null                // Danh sách lỗi chi tiết nếu có
}
```

---

## 1. Danh sách tổng hợp các Endpoint API

| Nhóm chức năng | Phương thức | Đường dẫn (Route) | Mô tả |
|---|---|---|---|
| **Khách hàng** | `GET` | `/api/customers` | Lấy danh sách tất cả khách hàng |
| **Khách hàng** | `GET` | `/api/customers/{id}` | Lấy chi tiết thông tin khách hàng theo Id |
| **Khách hàng** | `POST` | `/api/customers` | Tạo mới khách hàng |
| **Khách hàng** | `PUT` | `/api/customers/{id}` | Cập nhật thông tin khách hàng |
| **Hợp đồng** | `GET` | `/api/contracts` | Lấy danh sách tất cả hợp đồng |
| **Hợp đồng** | `GET` | `/api/contracts/{id}` | Lấy chi tiết hợp đồng theo Id |
| **Hợp đồng** | `POST` | `/api/contracts` | Tạo mới hợp đồng ở trạng thái `Draft` |
| **Hợp đồng** | `PUT` | `/api/contracts/{id}` | Cập nhật hợp đồng (Tuân thủ CTR-01) |
| **Hợp đồng** | `POST` | `/api/contracts/{id}/submit` | Gửi duyệt hợp đồng (US03 / CTR-01, 02, 03) |
| **Bảng giá** | `GET` | `/api/pricelists` | Lấy danh sách bảng giá và các mục giá |
| **Bảng giá** | `GET` | `/api/pricelists/{id}` | Lấy thông tin chi tiết bảng giá theo Id |
| **Bảng giá** | `POST` | `/api/pricelists` | Tạo mới bảng giá |
| **Bảng giá** | `POST` | `/api/pricelists/{id}/items` | Thêm mục giá dịch vụ vào bảng giá |
| **Bảng giá** | `GET` | `/api/pricelists/{id}/items` | Lấy danh sách các mục giá của bảng giá |

---

## 2. Chi tiết các Endpoint Khách hàng (Customer)

### 2.1. Lấy danh sách khách hàng
- **Method:** `GET`
- **Route:** `/api/customers`
- **Response 200 OK:**
```json
{
  "success": true,
  "message": "Lấy danh sách khách hàng thành công",
  "data": [
    {
      "id": 1,
      "name": "Công ty TNHH Minh Phát",
      "status": "Active"
    },
    {
      "id": 2,
      "name": "Công ty Cổ phần Hoàng Gia",
      "status": "Active"
    }
  ],
  "errors": null
}
```

### 2.2. Lấy thông tin khách hàng theo Id
- **Method:** `GET`
- **Route:** `/api/customers/{id}`
- **Response 200 OK:**
```json
{
  "success": true,
  "message": "Lấy thông tin khách hàng thành công",
  "data": {
    "id": 1,
    "name": "Công ty TNHH Minh Phát",
    "status": "Active"
  },
  "errors": null
}
```
- **Response 404 Not Found:**
```json
{
  "success": false,
  "message": "Không tìm thấy khách hàng với mã Id = 999",
  "data": null,
  "errors": []
}
```

### 2.3. Tạo khách hàng mới
- **Method:** `POST`
- **Route:** `/api/customers`
- **Request Body:**
```json
{
  "name": "Tập đoàn VinaTech",
  "status": "Active"
}
```
- **Response 201 Created:**
```json
{
  "success": true,
  "message": "Tạo khách hàng thành công",
  "data": {
    "id": 3,
    "name": "Tập đoàn VinaTech",
    "status": "Active"
  },
  "errors": null
}
```
- **Response 400 Bad Request (Thiếu tên):**
```json
{
  "success": false,
  "message": "Dữ liệu khách hàng không hợp lệ",
  "data": null,
  "errors": [
    "Tên khách hàng không được để trống"
  ]
}
```

### 2.4. Cập nhật khách hàng
- **Method:** `PUT`
- **Route:** `/api/customers/{id}`
- **Request Body:**
```json
{
  "name": "Công ty TNHH Minh Phát - Chi nhánh Hà Nội",
  "status": "Active"
}
```
- **Response 200 OK:**
```json
{
  "success": true,
  "message": "Cập nhật khách hàng thành công",
  "data": {
    "id": 1,
    "name": "Công ty TNHH Minh Phát - Chi nhánh Hà Nội",
    "status": "Active"
  },
  "errors": null
}
```

---

## 3. Chi tiết các Endpoint Hợp đồng (Contract)

### 3.1. Lấy danh sách hợp đồng
- **Method:** `GET`
- **Route:** `/api/contracts`
- **Response 200 OK:**
```json
{
  "success": true,
  "message": "Lấy danh sách hợp đồng thành công",
  "data": [
    {
      "id": 1,
      "contractNo": "HD-2026-018",
      "customerId": 1,
      "customerName": "Công ty TNHH Minh Phát",
      "startDate": "2026-03-01T00:00:00Z",
      "endDate": "2027-03-01T00:00:00Z",
      "totalAmount": 50000000.00,
      "terms": "Cung cấp giải pháp phần mềm Quản lý Kinh doanh trọn gói",
      "attachmentUrl": "https://storage.qtkd.vn/contracts/HD-2026-018.pdf",
      "status": "Draft",
      "submittedAt": null
    }
  ],
  "errors": null
}
```

### 3.2. Lấy chi tiết hợp đồng theo Id
- **Method:** `GET`
- **Route:** `/api/contracts/{id}`
- **Response 200 OK:**
```json
{
  "success": true,
  "message": "Lấy chi tiết hợp đồng thành công",
  "data": {
    "id": 1,
    "contractNo": "HD-2026-018",
    "customerId": 1,
    "customerName": "Công ty TNHH Minh Phát",
    "startDate": "2026-03-01T00:00:00Z",
    "endDate": "2027-03-01T00:00:00Z",
    "totalAmount": 50000000.00,
    "terms": "Cung cấp giải pháp phần mềm Quản lý Kinh doanh trọn gói",
    "attachmentUrl": "https://storage.qtkd.vn/contracts/HD-2026-018.pdf",
    "status": "Draft",
    "submittedAt": null
  },
  "errors": null
}
```

### 3.3. Tạo hợp đồng mới (ở trạng thái Draft)
- **Method:** `POST`
- **Route:** `/api/contracts`
- **Request Body:**
```json
{
  "contractNo": "HD-2026-DEMO-01",
  "customerId": 1,
  "startDate": "2026-04-01T00:00:00Z",
  "endDate": "2027-04-01T00:00:00Z",
  "totalAmount": 0,
  "terms": "",
  "attachmentUrl": ""
}
```
- **Response 201 Created:**
```json
{
  "success": true,
  "message": "Tạo hợp đồng thành công ở trạng thái Draft",
  "data": {
    "id": 2,
    "contractNo": "HD-2026-DEMO-01",
    "customerId": 1,
    "customerName": "Công ty TNHH Minh Phát",
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

### 3.4. Cập nhật hợp đồng (Tuân thủ CTR-01)
- **Method:** `PUT`
- **Route:** `/api/contracts/{id}`
- **Request Body (Thành công khi hợp đồng ở Draft):**
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
- **Response 200 OK:**
```json
{
  "success": true,
  "message": "Cập nhật hợp đồng thành công",
  "data": {
    "id": 2,
    "contractNo": "HD-2026-DEMO-01",
    "customerId": 1,
    "customerName": "Công ty TNHH Minh Phát",
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
- **Response 400 Bad Request (Khi vi phạm CTR-01 - Hợp đồng đang ở PendingApproval):**
```json
{
  "success": false,
  "message": "[CTR-01] Không thể chỉnh sửa hợp đồng khi ở trạng thái 'PendingApproval'. Chỉ cho phép sửa hợp đồng khi ở trạng thái 'Draft' hoặc 'Revision Requested'.",
  "data": null,
  "errors": [
    "[CTR-01] Không thể chỉnh sửa hợp đồng khi ở trạng thái 'PendingApproval'. Chỉ cho phép sửa hợp đồng khi ở trạng thái 'Draft' hoặc 'Revision Requested'."
  ]
}
```

### 3.5. Gửi hợp đồng trình duyệt (User Story US03)
- **Method:** `POST`
- **Route:** `/api/contracts/{id}/submit`
- **Trường hợp 1: Thất bại do thiếu thông tin (Quy tắc CTR-02)**:
  - **Response 400 Bad Request:**
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
- **Trường hợp 2: Thành công sau khi bổ sung đầy đủ dữ liệu (Quy tắc CTR-03)**:
  - **Response 200 OK:**
```json
{
  "success": true,
  "message": "Hợp đồng đã được gửi trình duyệt thành công",
  "data": {
    "contractId": 2,
    "contractNo": "HD-2026-DEMO-01",
    "previousStatus": "Draft",
    "newStatus": "PendingApproval",
    "submittedAt": "2026-11-03T13:40:00Z",
    "message": "Hợp đồng HD-2026-DEMO-01 đã được gửi trình duyệt thành công và chuyển sang trạng thái PendingApproval."
  },
  "errors": null
}
```
- **Trường hợp 3: Thử gửi lặp lại hợp đồng đã ở trạng thái PendingApproval (Yêu cầu 12)**:
  - **Response 400 Bad Request:**
```json
{
  "success": false,
  "message": "Hợp đồng 'HD-2026-DEMO-01' đã được gửi trình duyệt trước đó (trạng thái hiện tại: PendingApproval). Không được gửi trình duyệt trùng lặp.",
  "data": null,
  "errors": [
    "Hợp đồng 'HD-2026-DEMO-01' đã được gửi trình duyệt trước đó (trạng thái hiện tại: PendingApproval). Không được gửi trình duyệt trùng lặp."
  ]
}
```

---

## 4. Chi tiết các Endpoint Bảng giá & Chi tiết giá (Pricing)

### 4.1. Lấy danh sách bảng giá
- **Method:** `GET`
- **Route:** `/api/pricelists`
- **Response 200 OK:**
```json
{
  "success": true,
  "message": "Lấy danh sách bảng giá thành công",
  "data": [
    {
      "id": 1,
      "name": "Bảng giá dịch vụ phần mềm 2026",
      "effectiveDate": "2026-01-01T00:00:00Z",
      "items": [
        {
          "id": 1,
          "priceListId": 1,
          "serviceCode": "SRV-CLOUD",
          "unitPrice": 1500000.00
        },
        {
          "id": 2,
          "priceListId": 1,
          "serviceCode": "SRV-SUPPORT",
          "unitPrice": 500000.00
        }
      ]
    }
  ],
  "errors": null
}
```

### 4.2. Lấy thông tin chi tiết bảng giá theo Id
- **Method:** `GET`
- **Route:** `/api/pricelists/{id}`
- **Response 200 OK:**
```json
{
  "success": true,
  "message": "Lấy thông tin bảng giá thành công",
  "data": {
    "id": 1,
    "name": "Bảng giá dịch vụ phần mềm 2026",
    "effectiveDate": "2026-01-01T00:00:00Z",
    "items": [
      {
        "id": 1,
        "priceListId": 1,
        "serviceCode": "SRV-CLOUD",
        "unitPrice": 1500000.00
      },
      {
        "id": 2,
        "priceListId": 1,
        "serviceCode": "SRV-SUPPORT",
        "unitPrice": 500000.00
      }
    ]
  },
  "errors": null
}
```

### 4.3. Tạo mới bảng giá
- **Method:** `POST`
- **Route:** `/api/pricelists`
- **Request Body:**
```json
{
  "name": "Bảng giá Dịch vụ Hạ tầng Cloud 2026 - 2027",
  "effectiveDate": "2026-06-01T00:00:00Z"
}
```
- **Response 201 Created:**
```json
{
  "success": true,
  "message": "Tạo bảng giá thành công",
  "data": {
    "id": 2,
    "name": "Bảng giá Dịch vụ Hạ tầng Cloud 2026 - 2027",
    "effectiveDate": "2026-06-01T00:00:00Z",
    "items": []
  },
  "errors": null
}
```

### 4.4. Thêm mục giá dịch vụ vào bảng giá
- **Method:** `POST`
- **Route:** `/api/pricelists/{id}/items`
- **Request Body:**
```json
{
  "serviceCode": "SRV-SECURITY",
  "unitPrice": 2500000
}
```
- **Response 201 Created:**
```json
{
  "success": true,
  "message": "Thêm mục giá vào bảng giá thành công",
  "data": {
    "id": 3,
    "priceListId": 1,
    "serviceCode": "SRV-SECURITY",
    "unitPrice": 2500000.00
  },
  "errors": null
}
```
- **Response 400 Bad Request (Đơn giá <= 0):**
```json
{
  "success": false,
  "message": "Đơn giá dịch vụ (UnitPrice) phải lớn hơn 0 VNĐ.",
  "data": null,
  "errors": [
    "Đơn giá dịch vụ (UnitPrice) phải lớn hơn 0 VNĐ."
  ]
}
```

### 4.5. Lấy danh sách các mục giá của bảng giá
- **Method:** `GET`
- **Route:** `/api/pricelists/{id}/items`
- **Response 200 OK:**
```json
{
  "success": true,
  "message": "Lấy danh sách chi tiết giá thành công",
  "data": [
    {
      "id": 1,
      "priceListId": 1,
      "serviceCode": "SRV-CLOUD",
      "unitPrice": 1500000.00
    },
    {
      "id": 2,
      "priceListId": 1,
      "serviceCode": "SRV-SUPPORT",
      "unitPrice": 500000.00
    },
    {
      "id": 3,
      "priceListId": 1,
      "serviceCode": "SRV-SECURITY",
      "unitPrice": 2500000.00
    }
  ],
  "errors": null
}
```
