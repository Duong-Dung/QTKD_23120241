# BÁO CÁO KẾT QUẢ KIỂM THỬ - TUẦN 07 (PHASE 2)

**Môn học:** Phát triển ứng dụng Hệ thống thông tin hiện đại  
**Sinh viên thực hiện:** Dương Việt Dũng - MSSV: 23120241  
**GitHub Repository:** [Duong-Dung/QTKD_23120241](https://github.com/Duong-Dung/QTKD_23120241)  
**Công nghệ kiểm thử:** xUnit · Microsoft.NET.Test.Sdk · EF Core InMemory Database · C# .NET 10  

---

## 1. Mục tiêu kiểm thử Tuần 07
Theo hướng dẫn thực hành Phase 2:
- Không xây dựng thêm hệ thống khác; sử dụng mã nguồn tuần 06 để viết bộ kiểm thử tự động.
- Tách biệt rõ ràng giữa **Kiểm thử đơn vị (Unit Test)** cho các quy tắc nghiệp vụ CTR-01, CTR-02, CTR-03 và **Kiểm thử tích hợp (Integration Test)** cho API và cơ sở dữ liệu.
- Sử dụng cơ sở dữ liệu kiểm thử độc lập (InMemory Database) để không gây ảnh hưởng đến dữ liệu quan trọng hoặc môi trường triển khai thực tế.
- Kiểm tra toàn diện cả HTTP Status code, cấu trúc nội dung phản hồi `ApiResponse<T>` và dữ liệu được ghi nhận trong database.
- Phát hiện và khắc phục các lỗi tiềm ẩn từ tuần trước, đảm bảo ứng dụng hoạt động ổn định và sẵn sàng bàn giao.

---

## 2. Bảng tổng hợp kết quả kiểm thử tối thiểu (TC01 – TC08)

Toàn bộ **8/8 test cases tối thiểu** (và các test case mở rộng cho từng quy tắc) đều được **tự động hóa 100%** bằng framework xUnit và đều đạt trạng thái **PASS**.

| Mã Test | Mô tả Dữ liệu / Thao tác | Kết quả mong đợi | Hình thức | Kết quả thực tế | Trạng thái |
|:---:|---|---|:---:|---|:---:|
| **TC01** | Tạo Customer hợp lệ với tên và trạng thái Active | Tạo thành công, trả về HTTP `201 Created`, lưu và đọc lại chính xác từ DB | Tự động (Unit & Integration) | `TC01_CreateCustomer_WithValidData_ShouldSucceedAndPersistInDb`<br/>`TC01_Api_CreateCustomer_WithValidData_Returns201AndPersistsInDatabase` | **PASS** |
| **TC02** | Tạo Customer thiếu thông tin bắt buộc (tên rỗng hoặc khoảng trắng) | Bị từ chối với HTTP `400 Bad Request`, có thông báo lỗi rõ ràng, không tạo bản ghi rác trong DB | Tự động (Unit & Integration) | `TC02_CreateCustomer_MissingRequiredName_ShouldThrowBusinessRuleException`<br/>`TC02_Api_CreateCustomer_MissingRequiredName_Returns400BadRequestAndNotPersisted` | **PASS** |
| **TC03** | Sửa Contract khi đang ở trạng thái `Draft` (dữ liệu hợp lệ) | Cập nhật thành công, trả về HTTP `200 OK`, dữ liệu mới được lưu vào DB, giữ nguyên trạng thái `Draft` (CTR-01) | Tự động (Unit & Integration) | `TC03_UpdateContract_InDraftStatus_WithValidData_ShouldSucceed`<br/>`TC03_Api_UpdateContract_InDraftStatus_Returns200AndPersistsChanges` | **PASS** |
| **TC04** | Sửa Contract khi đang ở trạng thái `PendingApproval` | Bị từ chối với HTTP `400 Bad Request`, thông báo vi phạm CTR-01, dữ liệu gốc trong DB giữ nguyên không đổi | Tự động (Unit & Integration) | `TC04_UpdateContract_InPendingApprovalStatus_ShouldBeRejectedAndKeepDataUnchanged`<br/>`TC04_Api_UpdateContract_InPendingApprovalStatus_Returns400BadRequestAndKeepsData` | **PASS** |
| **TC05** | Submit Contract thiếu điều kiện CTR-02 (Customer Inactive, StartDate >= EndDate, TotalAmount = 0, thiếu Terms hoặc thiếu AttachmentUrl) | Bị từ chối với HTTP `400 Bad Request`, trả về danh sách lỗi cụ thể, trạng thái trong DB vẫn là `Draft` | Tự động (Unit & Integration) | `TC05_SubmitContract_MissingRequiredAttachmentOrZeroAmount_ShouldBeRejectedAndRemainDraft`<br/>`TC05_SubmitContract_WithInactiveCustomer_ShouldBeRejected`<br/>`TC05_Api_SubmitContract_MissingCTR02Requirements_Returns400BadRequestAndStaysDraft` | **PASS** |
| **TC06** | Submit Contract với đầy đủ thông tin hợp lệ | Gửi duyệt thành công, HTTP `200 OK`, trạng thái chuyển sang `PendingApproval`, cập nhật `SubmittedAt` | Tự động (Unit & Integration) | `TC06_SubmitContract_WithAllValidConditions_ShouldChangeToPendingApproval`<br/>`TC06_Api_SubmitContract_WithValidData_Returns200AndTransitionsToPendingApproval` | **PASS** |
| **TC07** | Thử chuyển hợp đồng từ `Draft` thẳng sang `Approved` (bỏ qua bước phê duyệt) | Bị từ chối với HTTP `400 Bad Request`, thông báo vi phạm quy tắc CTR-03, không cho phép đổi status tùy tiện | Tự động (Unit & Integration) | `TC07_DirectUpdateToApprovedStatus_ShouldBeRejected`<br/>`TC07_Api_AttemptDirectTransitionToApproved_Returns400BadRequest`<br/>`TC06_CTR03_SubmitContract_MustOnlyTransitionToPendingApproval_NeverApproved` | **PASS** |
| **TC08** | Submit lặp lại một hợp đồng đã ở trạng thái `PendingApproval` (chống gửi trùng) | Bị từ chối với HTTP `400 Bad Request`, thông báo hợp đồng đã được gửi duyệt trước đó, không ghi đè `SubmittedAt` | Tự động (Unit & Integration) | `TC08_SubmitContract_WhenAlreadyPendingApproval_ShouldThrowException`<br/>`TC08_Api_SubmitContract_DuplicateSubmission_Returns400BadRequest` | **PASS** |

---

## 3. Cấu trúc tổ chức dự án kiểm thử (`tests/QTKD.Tests`)

Dự án kiểm thử được cấu hình tách bạch rõ ràng giữa Unit Test và Integration Test:
```
tests/QTKD.Tests/
├── QTKD.Tests.csproj                     # File cấu hình xUnit test project (.NET 10)
├── Common/
│   └── TestDbContextFactory.cs           # Factory cấp phát InMemory DbContext độc lập cho mỗi ca kiểm thử
├── UnitTests/                             # [Tầng Unit Test - Nghiệp vụ thuần túy]
│   ├── CustomerServiceTests.cs            # Kiểm tra CustomerService (TC01, TC02, Update)
│   └── ContractServiceTests.cs            # Kiểm tra ContractService (CTR-01, CTR-02, CTR-03, TC03-TC08)
├── IntegrationTests/                      # [Tầng Integration Test - API & Database]
│   ├── CustomerApiIntegrationTests.cs     # Kiểm tra CustomersController: HTTP Status, ApiResponse, DB state
│   └── ContractApiIntegrationTests.cs     # Kiểm tra ContractsController: TC03-TC08 & Luồng Demo xuyên suốt tuần 06
└── TestResults/
    └── TestResults.trx                   # Báo cáo kết quả kiểm thử tự động định dạng chuẩn Visual Studio TRX
```

---

## 4. Báo cáo các lỗi phát hiện được trong quá trình kiểm thử và cách khắc phục

Trong quá trình xây dựng bộ kiểm thử và đối soát quy tắc nghiệp vụ theo yêu cầu Tuần 07, sinh viên đã phát hiện và khắc phục các vấn đề sau:

### Lỗi 1: Thiếu kiểm tra tính hợp lệ của tên khách hàng ở tầng Service (`CustomerService`)
- **Cách tái hiện lỗi:**
  Khi viết Unit Test gọi trực tiếp phương thức `CustomerService.CreateCustomerAsync(new CreateCustomerDto { Name = "" })`, trước đây hàm không ném lỗi mà thực hiện lưu khách hàng có tên rỗng vào database.
- **Nguyên nhân:**
  Trong mã nguồn Tuần 06, việc kiểm tra trường bắt buộc chỉ được khai báo thông qua DataAnnotation `[Required]` ở `CreateCustomerDto`. Tầng Controller kiểm tra qua `ModelState.IsValid`, nhưng tầng Service (nghiệp vụ cốt lõi) chưa có cơ chế phòng vệ độc lập.
- **Cách khắc phục:**
  Bổ sung kiểm tra tường minh tại `CustomerService.CreateCustomerAsync` và `UpdateCustomerAsync`:
  ```csharp
  if (string.IsNullOrWhiteSpace(dto.Name))
  {
      throw new BusinessRuleException("Tên khách hàng không được để trống.");
  }
  ```
  Nhờ đó, dù gọi qua API Controller hay gọi trực tiếp tầng nghiệp vụ, hệ thống đều từ chối tạo khách hàng rỗng và ném thông báo lỗi chuẩn xác.
- **Mã Git commit sửa lỗi:** `438b826` (*"fix: bo sung validate bat buoc Name o CustomerService va chan chuyen sang Approved theo CTR-03"*)

### Lỗi 2: Chưa có cơ chế bảo vệ ngăn chặn việc cố ý đổi trạng thái sang `Approved` qua API cập nhật (CTR-03)
- **Cách tái hiện lỗi:**
  Khi một client gửi payload `PUT /api/contracts/{id}` chứa thuộc tính `Status = "Approved"` nhằm nhảy cóc từ `Draft` sang `Approved`.
- **Nguyên nhân:**
  Trước đây `UpdateContractDto` không khai báo trường `Status`, nên EF Core bỏ qua và không đổi trạng thái; tuy nhiên hệ thống không phản hồi thông báo lỗi rõ ràng giải thích việc từ chối chuyển trạng thái sang `Approved` theo đúng quy tắc CTR-03.
- **Cách khắc phục:**
  Bổ sung trường `Status` vào `UpdateContractDto` và cài đặt logic chặn chặt chẽ tại `ContractService.UpdateContractAsync`:
  ```csharp
  // [QUY TẮC CTR-03] Không được phép tự ý thay đổi trạng thái qua cập nhật, đặc biệt không được chuyển từ Draft sang Approved
  if (!string.IsNullOrWhiteSpace(dto.Status) && !string.Equals(dto.Status, contract.Status, StringComparison.OrdinalIgnoreCase))
  {
      throw new BusinessRuleException(
          $"[CTR-03] Không được phép tự ý thay đổi trực tiếp trạng thái hợp đồng sang '{dto.Status}' (không được chuyển thẳng sang Approved). " +
          "Trạng thái hợp đồng chỉ được chuyển sang PendingApproval thông qua quy trình gửi trình duyệt.");
  }
  ```
  Khi người dùng cố ý gửi `Status = "Approved"`, API lập tức phản hồi HTTP `400 Bad Request` kèm thông điệp cảnh báo vi phạm CTR-03.
- **Mã Git commit sửa lỗi:** `438b826` (*"fix: bo sung validate bat buoc Name o CustomerService va chan chuyen sang Approved theo CTR-03"*)

---

## 5. Minh chứng chạy kiểm thử (Console Log & File TRX)

### Kết quả chạy lệnh `dotnet test`
```
Test run for D:\23120241_TH7\tests\QTKD.Tests\bin\Debug\net10.0\QTKD.Tests.dll (.NETCoreApp,Version=v10.0)
A total of 1 test files matched the specified pattern.
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v3.1.4+50e68bbb8b (64-bit .NET 10.0.13)
[xUnit.net 00:00:00.06]   Discovering: QTKD.Tests
[xUnit.net 00:00:00.10]   Discovered:  QTKD.Tests
[xUnit.net 00:00:00.12]   Starting:    QTKD.Tests
  Passed QTKD.Tests.UnitTests.CustomerServiceTests.TC01_CreateCustomer_WithValidData_ShouldSucceedAndPersistInDb [485 ms]
  Passed QTKD.Tests.IntegrationTests.CustomerApiIntegrationTests.TC01_Api_CreateCustomer_WithValidData_Returns201AndPersistsInDatabase [564 ms]
  Passed QTKD.Tests.UnitTests.CustomerServiceTests.UpdateCustomer_WithEmptyName_ShouldThrowBusinessRuleException [84 ms]
  Passed QTKD.Tests.IntegrationTests.CustomerApiIntegrationTests.TC02_Api_CreateCustomer_MissingRequiredName_Returns400BadRequestAndNotPersisted [9 ms]
  Passed QTKD.Tests.UnitTests.CustomerServiceTests.TC02_CreateCustomer_MissingRequiredName_ShouldThrowBusinessRuleException(invalidName: "   ") [1 ms]
  Passed QTKD.Tests.UnitTests.CustomerServiceTests.TC02_CreateCustomer_MissingRequiredName_ShouldThrowBusinessRuleException(invalidName: "") [< 1 ms]
  Passed QTKD.Tests.UnitTests.CustomerServiceTests.TC02_CreateCustomer_MissingRequiredName_ShouldThrowBusinessRuleException(invalidName: null) [< 1 ms]
  Passed QTKD.Tests.UnitTests.CustomerServiceTests.UpdateCustomer_WithValidData_ShouldUpdateSuccessfully [1 ms]
  Passed QTKD.Tests.IntegrationTests.ContractApiIntegrationTests.TC03_Api_UpdateContract_InDraftStatus_Returns200AndPersistsChanges [576 ms]
  Passed QTKD.Tests.UnitTests.ContractServiceTests.TC06_CTR03_SubmitContract_MustOnlyTransitionToPendingApproval_NeverApproved [577 ms]
  Passed QTKD.Tests.IntegrationTests.ContractApiIntegrationTests.TC06_Api_SubmitContract_WithValidData_Returns200AndTransitionsToPendingApproval [4 ms]
  Passed QTKD.Tests.UnitTests.ContractServiceTests.TC05_SubmitContract_MissingRequiredAttachmentOrZeroAmount_ShouldBeRejectedAndRemainDraft [5 ms]
  Passed QTKD.Tests.UnitTests.ContractServiceTests.TC06_SubmitContract_WithAllValidConditions_ShouldChangeToPendingApproval [1 ms]
  Passed QTKD.Tests.UnitTests.ContractServiceTests.UpdateContract_InRevisionRequestedStatus_ShouldSucceed [1 ms]
  Passed QTKD.Tests.IntegrationTests.ContractApiIntegrationTests.TC08_Api_SubmitContract_DuplicateSubmission_Returns400BadRequest [3 ms]
  Passed QTKD.Tests.UnitTests.ContractServiceTests.TC03_UpdateContract_InDraftStatus_WithValidData_ShouldSucceed [< 1 ms]
  Passed QTKD.Tests.IntegrationTests.ContractApiIntegrationTests.TC07_Api_AttemptDirectTransitionToApproved_Returns400BadRequest [< 1 ms]
  Passed QTKD.Tests.UnitTests.ContractServiceTests.TC04_UpdateContract_InPendingApprovalStatus_ShouldBeRejectedAndKeepDataUnchanged [9 ms]
  Passed QTKD.Tests.IntegrationTests.ContractApiIntegrationTests.TC04_Api_UpdateContract_InPendingApprovalStatus_Returns400BadRequestAndKeepsData [8 ms]
  Passed QTKD.Tests.IntegrationTests.ContractApiIntegrationTests.TC05_Api_SubmitContract_MissingCTR02Requirements_Returns400BadRequestAndStaysDraft [2 ms]
  Passed QTKD.Tests.UnitTests.ContractServiceTests.TC08_SubmitContract_WhenAlreadyPendingApproval_ShouldThrowException [2 ms]
  Passed QTKD.Tests.UnitTests.ContractServiceTests.TC07_DirectUpdateToApprovedStatus_ShouldBeRejected [1 ms]
  Passed QTKD.Tests.UnitTests.ContractServiceTests.TC05_SubmitContract_WithInactiveCustomer_ShouldBeRejected [3 ms]
  Passed QTKD.Tests.IntegrationTests.ContractApiIntegrationTests.FullWorkflowDemo_Week06_ShouldExecuteAllStepsSuccessfully [11 ms]
Results File: D:\23120241_TH7\tests\QTKD.Tests\TestResults\TestResults.trx

Test Run Successful.
Total tests: 24
     Passed: 24
 Total time: 1.1515 Seconds
```

File báo cáo chi tiết: [docs/TestResults.trx](file:///d:/download/23120241_TH7/docs/TestResults.trx)

