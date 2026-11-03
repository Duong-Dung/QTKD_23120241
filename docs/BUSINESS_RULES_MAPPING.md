# BẢNG ĐỐI CHIẾU USER STORY US03 VÀ CÁC QUY TẮC NGHIỆP VỤ CTR-01, CTR-02, CTR-03

Tài liệu này đối chiếu trực tiếp các yêu cầu của bài thực hành Tuần 06 với các đoạn code cài đặt cụ thể trong dự án.

---

## 1. Giả định nghiệp vụ về Hồ sơ/Tệp đính kèm và Nội dung bắt buộc

Theo ghi chú trong hướng dẫn Phase 2 (Mục 4.4):
> *"Nếu Phase 1 chưa xác định danh sách tệp/nội dung bắt buộc, sinh viên phải ghi rõ giả định dùng trong bản demo và phần cần xác nhận; không tự xem đó là yêu cầu chính thức."*

### Các giả định được áp dụng trong hệ thống:
1. **Khách hàng (Customer):**
   - Hợp đồng phải được gán với một khách hàng (`CustomerId`) tồn tại trong database.
   - Trạng thái của khách hàng phải là `"Active"`. Nếu khách hàng bị khóa hoặc ngưng hoạt động (`"Inactive"`), không cho phép gửi duyệt hợp đồng.
2. **Thời hạn hiệu lực (Validity Period):**
   - Hợp đồng bắt buộc phải có ngày bắt đầu (`StartDate`) và ngày kết thúc (`EndDate`).
   - Ngày bắt đầu phải nhỏ hơn ngày kết thúc (`StartDate < EndDate`).
3. **Nội dung điều khoản bắt buộc (Mandatory Terms):**
   - Trường `Terms` không được để trống hoặc chỉ có khoảng trắng, tối thiểu phải có 5 ký tự mô tả phạm vi cung cấp dịch vụ hoặc điều khoản cốt lõi.
   - Giá trị hợp đồng `TotalAmount` phải lớn hơn 0 VNĐ (`TotalAmount > 0`).
4. **Tệp hồ sơ/tài liệu đính kèm bắt buộc (Mandatory Attachment):**
   - Trường `AttachmentUrl` không được để trống (mô phỏng đường dẫn tải lên tệp hợp đồng PDF/scan đã ký nháy hoặc biên bản thỏa thuận).
5. **Quy tắc không thay đổi dữ liệu khi thất bại:**
   - Nếu vi phạm bất kỳ điều kiện nào ở trên, hợp đồng giữ nguyên trạng thái `Draft`, không có bất kỳ thuộc tính nào bị thay đổi sai lệch.

---

## 2. Bảng đối chiếu chi tiết Quy tắc nghiệp vụ và Đoạn xử lý trong Code

| Mã quy tắc / User Story | Tên quy tắc & Ý nghĩa nghiệp vụ | Cách kiểm tra / Kết quả mong đợi | Vị trí code xử lý cụ thể |
|---|---|---|---|
| **US03** | **Gửi hợp đồng trình duyệt**<br/>Nhân viên kinh doanh gửi hợp đồng để cấp trên xét duyệt. Chỉ cho phép gửi khi hợp đồng đủ điều kiện. Trạng thái chuyển từ `Draft` sang `PendingApproval`. | Gửi thành công khi đủ điều kiện; Từ chối nếu thiếu dữ liệu; Cập nhật `SubmittedAt`. | - [ContractsController.cs](file:///d:/download/23120241_TH6/QTKD.Api/Controllers/ContractsController.cs#L104-L123) (`Submit` action)<br/>- [ContractService.cs](file:///d:/download/23120241_TH6/QTKD.Api/Services/ContractService.cs#L150-L245) (`SubmitContractAsync`) |
| **CTR-01** | **Giới hạn quyền chỉnh sửa hợp đồng**<br/>Chỉ cho phép sửa hợp đồng khi ở trạng thái `Draft` hoặc `Revision Requested`. | Thử sửa Contract ở `PendingApproval` hoặc `Approved`: Bị từ chối với mã lỗi `400 Bad Request`. | - [ContractService.cs](file:///d:/download/23120241_TH6/QTKD.Api/Services/ContractService.cs#L106-L115)<br/>Kiểm tra `allowedStatuses.Contains(contract.Status)` |
| **CTR-02** | **Kiểm tra tính đầy đủ và hợp lệ trước khi gửi**<br/>Chỉ gửi khi Customer hợp lệ (`Active`), ngày hiệu lực hợp lệ (`StartDate < EndDate`), giá trị `TotalAmount > 0`, nội dung `Terms` và tệp đính kèm `AttachmentUrl` đầy đủ. | Thiếu một điều kiện: Gửi thất bại (`400 Bad Request`), trả về danh sách lỗi cụ thể, dữ liệu và trạng thái `Draft` không đổi. | - [ContractService.cs](file:///d:/download/23120241_TH6/QTKD.Api/Services/ContractService.cs#L173-L226)<br/>Kiểm tra tập trung vào `validationErrors` và ném `BusinessRuleException` |
| **CTR-03** | **Không được chuyển Draft thẳng sang Approved**<br/>Quy trình duyệt bắt buộc phải qua phê duyệt. Submit hợp đồng thành công chỉ chuyển trạng thái sang `PendingApproval`. API cập nhật không cho phép tự ý đổi `Status`. | Khi submit thành công, trạng thái luôn là `PendingApproval`. Không tồn tại endpoint nào cho phép chuyển thẳng từ `Draft` sang `Approved`. | - [ContractService.cs](file:///d:/download/23120241_TH6/QTKD.Api/Services/ContractService.cs#L228-L245)<br/>Gán `contract.Status = "PendingApproval"` |
| **Yêu cầu 12** | **Xử lý gửi lặp (Idempotency / Anti-duplicate submit)**<br/>Thử gửi cùng một hợp đồng hai lần. Lần sau không được tạo thêm một lần xử lý tương đương. | Hợp đồng đã ở `PendingApproval` nếu bị submit tiếp sẽ bị từ chối (`400 Bad Request`) với thông báo rõ ràng. | - [ContractService.cs](file:///d:/download/23120241_TH6/QTKD.Api/Services/ContractService.cs#L158-L167)<br/>Kiểm tra nếu `contract.Status == "PendingApproval"` |

---

## 3. Trích dẫn đoạn mã kiểm tra trực tiếp trong Code

### 3.1. Kiểm tra CTR-01 trong `ContractService.cs`:
```csharp
// [QUY TẮC CTR-01] Chỉ cho phép sửa hợp đồng ở trạng thái "Draft" hoặc "Revision Requested"
// Thử sửa Contract ở PendingApproval: phải bị từ chối
var allowedStatuses = new[] { "Draft", "Revision Requested" };
if (!allowedStatuses.Contains(contract.Status, StringComparer.OrdinalIgnoreCase))
{
    throw new BusinessRuleException(
        $"[CTR-01] Không thể chỉnh sửa hợp đồng khi ở trạng thái '{contract.Status}'. " +
        "Chỉ cho phép sửa hợp đồng khi ở trạng thái 'Draft' hoặc 'Revision Requested'.");
}
```

### 3.2. Kiểm tra CTR-02 trong `ContractService.cs`:
```csharp
// 2. [QUY TẮC CTR-02] Kiểm tra các điều kiện bắt buộc trước khi gửi trình duyệt
var validationErrors = new List<string>();

// a) Kiểm tra Customer hợp lệ
if (contract.Customer == null)
{
    validationErrors.Add("Hợp đồng chưa được liên kết với khách hàng hợp lệ.");
}
else if (!string.Equals(contract.Customer.Status, "Active", StringComparison.OrdinalIgnoreCase))
{
    validationErrors.Add($"Khách hàng '{contract.Customer.Name}' hiện không ở trạng thái hoạt động (Status: '{contract.Customer.Status}').");
}

// b) Kiểm tra ngày hiệu lực
if (contract.StartDate >= contract.EndDate)
{
    validationErrors.Add("Ngày bắt đầu hợp đồng phải trước ngày kết thúc.");
}

// c) Kiểm tra nội dung bắt buộc: Giá trị hợp đồng > 0 & Điều khoản hợp đồng
if (contract.TotalAmount <= 0)
{
    validationErrors.Add("Giá trị hợp đồng (TotalAmount) phải lớn hơn 0 VNĐ.");
}

if (string.IsNullOrWhiteSpace(contract.Terms) || contract.Terms.Trim().Length < 5)
{
    validationErrors.Add("Hợp đồng thiếu nội dung điều khoản bắt buộc (Terms) hoặc nội dung quá ngắn.");
}

// d) Kiểm tra tệp tài liệu đính kèm bắt buộc
if (string.IsNullOrWhiteSpace(contract.AttachmentUrl))
{
    validationErrors.Add("Hợp đồng thiếu tệp tài liệu/hồ sơ đính kèm bắt buộc (AttachmentUrl).");
}

// Nếu thiếu bất kỳ điều kiện nào: gửi thất bại, dữ liệu trong Contract giữ nguyên không đổi!
if (validationErrors.Count > 0)
{
    throw new BusinessRuleException(
        "[CTR-02] Hợp đồng không đủ điều kiện gửi trình duyệt do thiếu thông tin hoặc thông tin không hợp lệ.",
        validationErrors);
}
```

### 3.3. Kiểm tra CTR-03 và Yêu cầu 12 trong `ContractService.cs`:
```csharp
// Yêu cầu 12: Kiểm tra gửi lặp lại
if (string.Equals(contract.Status, "PendingApproval", StringComparison.OrdinalIgnoreCase))
{
    throw new BusinessRuleException(
        $"Hợp đồng '{contract.ContractNo}' đã được gửi trình duyệt trước đó (trạng thái hiện tại: PendingApproval). " +
        "Không được gửi trình duyệt trùng lặp.");
}

// [QUY TẮC CTR-03] Chuyển trạng thái sang PendingApproval, không chuyển thẳng sang Approved
var previousStatus = contract.Status;
contract.Status = "PendingApproval";
contract.SubmittedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

await _context.SaveChangesAsync();
```
