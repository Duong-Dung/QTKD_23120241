using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QTKD.Api.Controllers;
using QTKD.Api.DTOs;
using QTKD.Api.Entities;
using QTKD.Api.Services;
using QTKD.Tests.Common;

namespace QTKD.Tests.IntegrationTests;

/// <summary>
/// Integration test cho Contract API: TC03 - TC08 và Luồng Demo xuyên suốt Tuần 06
/// Kiểm tra HTTP status code, cấu trúc phản hồi ApiResponse<T> và tính toàn vẹn dữ liệu trong Database
/// </summary>
public class ContractApiIntegrationTests
{
    [Fact]
    public async Task TC03_Api_UpdateContract_InDraftStatus_Returns200AndPersistsChanges()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new ContractService(context);
        var controller = new ContractsController(service);

        // Hợp đồng Id = 1 có sẵn trong seed data đang ở trạng thái Draft
        var updateDto = new UpdateContractDto
        {
            TotalAmount = 45000000m,
            Terms = "Điều khoản hợp đồng sau khi cập nhật sửa đổi bổ sung",
            AttachmentUrl = "https://storage.qtkd.vn/contracts/HD-2026-018-updated.pdf"
        };

        // Act: Gửi PUT /api/contracts/1
        var actionResult = await controller.Update(1, updateDto);

        // Assert 1: HTTP Status 200 OK
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        // Assert 2: Kiểm tra Response Body
        var body = Assert.IsType<ApiResponse<ContractDto>>(okResult.Value);
        Assert.True(body.Success);
        Assert.NotNull(body.Data);
        Assert.Equal(45000000m, body.Data.TotalAmount);
        Assert.Equal("Điều khoản hợp đồng sau khi cập nhật sửa đổi bổ sung", body.Data.Terms);

        // Assert 3: Kiểm tra trực tiếp Database
        var inDb = await context.Contracts.FindAsync(1);
        Assert.NotNull(inDb);
        Assert.Equal(45000000m, inDb.TotalAmount);
        Assert.Equal("Draft", inDb.Status);
    }

    [Fact]
    public async Task TC04_Api_UpdateContract_InPendingApprovalStatus_Returns400BadRequestAndKeepsData()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new ContractService(context);
        var controller = new ContractsController(service);

        var contract = await context.Contracts.FindAsync(1);
        Assert.NotNull(contract);
        contract.Status = "PendingApproval";
        await context.SaveChangesAsync();

        var originalAmount = contract.TotalAmount;
        var originalTerms = contract.Terms;

        var illegalUpdate = new UpdateContractDto
        {
            TotalAmount = 999999999m,
            Terms = "Cố ý chỉnh sửa dữ liệu khi hợp đồng đã chuyển sang PendingApproval"
        };

        // Act: Gửi PUT /api/contracts/1
        var actionResult = await controller.Update(1, illegalUpdate);

        // Assert 1: HTTP Status 400 Bad Request
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

        // Assert 2: Phản hồi nêu rõ vi phạm CTR-01
        var body = Assert.IsType<ApiResponse<ContractDto>>(badRequestResult.Value);
        Assert.False(body.Success);
        Assert.Contains("[CTR-01]", body.Message);

        // Assert 3: Kiểm tra dữ liệu trong DB tuyệt đối KHÔNG bị thay đổi
        var inDb = await context.Contracts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == 1);
        Assert.NotNull(inDb);
        Assert.Equal(originalAmount, inDb.TotalAmount);
        Assert.Equal(originalTerms, inDb.Terms);
        Assert.Equal("PendingApproval", inDb.Status);
    }

    [Fact]
    public async Task TC05_Api_SubmitContract_MissingCTR02Requirements_Returns400BadRequestAndStaysDraft()
    {
        // Arrange: Tạo hợp đồng thiếu file đính kèm và TotalAmount = 0
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new ContractService(context);
        var controller = new ContractsController(service);

        var contract = new Contract
        {
            ContractNo = "HD-TC05-API",
            CustomerId = 1,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6),
            TotalAmount = 0m,
            Terms = "Chưa có file hồ sơ đính kèm",
            AttachmentUrl = null,
            Status = "Draft"
        };
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();

        // Act: Gửi POST /api/contracts/{id}/submit
        var actionResult = await controller.Submit(contract.Id);

        // Assert 1: HTTP Status 400 Bad Request
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

        // Assert 2: Phản hồi chứa thông điệp lỗi CTR-02 và danh sách lỗi
        var body = Assert.IsType<ApiResponse<SubmitContractResultDto>>(badRequestResult.Value);
        Assert.False(body.Success);
        Assert.Contains("[CTR-02]", body.Message);
        Assert.NotNull(body.Errors);
        Assert.NotEmpty(body.Errors);

        // Assert 3: Kiểm tra trực tiếp Database: trạng thái vẫn là Draft, submittedAt là null
        var inDb = await context.Contracts.FindAsync(contract.Id);
        Assert.NotNull(inDb);
        Assert.Equal("Draft", inDb.Status);
        Assert.Null(inDb.SubmittedAt);
    }

    [Fact]
    public async Task TC06_Api_SubmitContract_WithValidData_Returns200AndTransitionsToPendingApproval()
    {
        // Arrange: Hợp đồng Id = 1 đã có đầy đủ thông tin hợp lệ
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new ContractService(context);
        var controller = new ContractsController(service);

        // Act: Gửi POST /api/contracts/1/submit
        var actionResult = await controller.Submit(1);

        // Assert 1: HTTP Status 200 OK
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        // Assert 2: Response body chứa thông tin chuyển trạng thái
        var body = Assert.IsType<ApiResponse<SubmitContractResultDto>>(okResult.Value);
        Assert.True(body.Success);
        Assert.NotNull(body.Data);
        Assert.Equal("Draft", body.Data.PreviousStatus);
        Assert.Equal("PendingApproval", body.Data.NewStatus);
        Assert.NotEqual(default, body.Data.SubmittedAt);

        // Assert 3: Kiểm tra trực tiếp Database
        var inDb = await context.Contracts.FindAsync(1);
        Assert.NotNull(inDb);
        Assert.Equal("PendingApproval", inDb.Status);
        Assert.NotNull(inDb.SubmittedAt);
    }

    [Fact]
    public async Task TC07_Api_AttemptDirectTransitionToApproved_Returns400BadRequest()
    {
        // Arrange: Hợp đồng đang ở Draft
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new ContractService(context);
        var controller = new ContractsController(service);

        var attemptApprovedDto = new UpdateContractDto
        {
            Status = "Approved" // Cố ý gửi cập nhật đổi thẳng sang Approved
        };

        // Act: Gửi PUT /api/contracts/1
        var actionResult = await controller.Update(1, attemptApprovedDto);

        // Assert 1: Bị từ chối với HTTP 400 Bad Request
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

        // Assert 2: Thông báo vi phạm quy tắc CTR-03
        var body = Assert.IsType<ApiResponse<ContractDto>>(badRequestResult.Value);
        Assert.False(body.Success);
        Assert.Contains("[CTR-03]", body.Message);

        // Assert 3: Kiểm tra Database: trạng thái vẫn là Draft, không bị đổi thành Approved
        var inDb = await context.Contracts.FindAsync(1);
        Assert.NotNull(inDb);
        Assert.Equal("Draft", inDb.Status);
    }

    [Fact]
    public async Task TC08_Api_SubmitContract_DuplicateSubmission_Returns400BadRequest()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new ContractService(context);
        var controller = new ContractsController(service);

        // Lần 1: Submit thành công
        var firstResult = await controller.Submit(1);
        var okResult = Assert.IsType<OkObjectResult>(firstResult.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var firstBody = Assert.IsType<ApiResponse<SubmitContractResultDto>>(okResult.Value);
        var originalSubmittedAt = firstBody.Data!.SubmittedAt;

        // Act: Gửi lần 2 khi hợp đồng đã ở trạng thái PendingApproval
        var secondResult = await controller.Submit(1);

        // Assert 1: Phải bị từ chối với HTTP 400 Bad Request
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(secondResult.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

        // Assert 2: Phản hồi giải thích rõ không được gửi trùng lặp
        var secondBody = Assert.IsType<ApiResponse<SubmitContractResultDto>>(badRequestResult.Value);
        Assert.False(secondBody.Success);
        Assert.Contains("trước đó", secondBody.Message);

        // Assert 3: Kiểm tra DB - trạng thái vẫn là PendingApproval, submittedAt không bị ghi đè
        var inDb = await context.Contracts.FindAsync(1);
        Assert.NotNull(inDb);
        Assert.Equal("PendingApproval", inDb.Status);
        Assert.Equal(originalSubmittedAt, inDb.SubmittedAt);
    }

    [Fact]
    public async Task FullWorkflowDemo_Week06_ShouldExecuteAllStepsSuccessfully()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var customerService = new CustomerService(context);
        var contractService = new ContractService(context);

        var customersController = new CustomersController(customerService);
        var contractsController = new ContractsController(contractService);

        // =====================================================================
        // Bước 1: Tạo Customer mới (POST /api/customers)
        // =====================================================================
        var customerDto = new CreateCustomerDto
        {
            Name = "Công ty Cổ phần Giải pháp Số 2026",
            Status = "Active"
        };
        var createCustResult = await customersController.Create(customerDto);
        var custActionResult = Assert.IsType<CreatedAtActionResult>(createCustResult.Result);
        Assert.Equal(StatusCodes.Status201Created, custActionResult.StatusCode);
        var custResponse = Assert.IsType<ApiResponse<CustomerDto>>(custActionResult.Value);
        var customerId = custResponse.Data!.Id;

        // =====================================================================
        // Bước 2: Tạo Contract Draft (thiếu tệp đính kèm và totalAmount = 0)
        // =====================================================================
        var contractDto = new CreateContractDto
        {
            ContractNo = "HD-DEMO-2026-X",
            CustomerId = customerId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            TotalAmount = 0m, // Thiếu giá trị hợp lệ
            Terms = "Điều khoản hợp đồng sơ bộ",
            AttachmentUrl = null // Thiếu file đính kèm
        };
        var createContractResult = await contractsController.Create(contractDto);
        var contractActionResult = Assert.IsType<CreatedAtActionResult>(createContractResult.Result);
        Assert.Equal(StatusCodes.Status201Created, contractActionResult.StatusCode);
        var contractResponse = Assert.IsType<ApiResponse<ContractDto>>(contractActionResult.Value);
        var contractId = contractResponse.Data!.Id;
        Assert.Equal("Draft", contractResponse.Data.Status);

        // =====================================================================
        // Bước 3: Gửi khi thiếu dữ liệu -> Thất bại (HTTP 400 Bad Request - CTR-02)
        // =====================================================================
        var submitFailResult = await contractsController.Submit(contractId);
        var failActionResult = Assert.IsType<BadRequestObjectResult>(submitFailResult.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, failActionResult.StatusCode);

        // Kiểm tra hợp đồng trong DB vẫn giữ nguyên là Draft
        var dbAfterFail = await context.Contracts.FindAsync(contractId);
        Assert.NotNull(dbAfterFail);
        Assert.Equal("Draft", dbAfterFail.Status);

        // =====================================================================
        // Bước 4: Bổ sung dữ liệu -> Thành công (HTTP 200 OK - CTR-01)
        // =====================================================================
        var updateDto = new UpdateContractDto
        {
            TotalAmount = 150000000m,
            Terms = "Cung cấp bản quyền phần mềm và dịch vụ triển khai trọn gói doanh nghiệp",
            AttachmentUrl = "https://storage.qtkd.vn/contracts/HD-DEMO-2026-X.pdf"
        };
        var updateResult = await contractsController.Update(contractId, updateDto);
        var updateActionResult = Assert.IsType<OkObjectResult>(updateResult.Result);
        Assert.Equal(StatusCodes.Status200OK, updateActionResult.StatusCode);

        // =====================================================================
        // Bước 5: Gửi lại -> Thành công (HTTP 200 OK - CTR-03: Draft -> PendingApproval)
        // =====================================================================
        var submitSuccessResult = await contractsController.Submit(contractId);
        var successActionResult = Assert.IsType<OkObjectResult>(submitSuccessResult.Result);
        Assert.Equal(StatusCodes.Status200OK, successActionResult.StatusCode);
        var submitResponse = Assert.IsType<ApiResponse<SubmitContractResultDto>>(successActionResult.Value);
        Assert.Equal("PendingApproval", submitResponse.Data!.NewStatus);

        // =====================================================================
        // Bước 6: Xem trạng thái trong database
        // =====================================================================
        var finalDbContract = await context.Contracts.FindAsync(contractId);
        Assert.NotNull(finalDbContract);
        Assert.Equal("PendingApproval", finalDbContract.Status);
        Assert.NotNull(finalDbContract.SubmittedAt);
        Assert.Equal(150000000m, finalDbContract.TotalAmount);
        Assert.Equal("https://storage.qtkd.vn/contracts/HD-DEMO-2026-X.pdf", finalDbContract.AttachmentUrl);
    }
}

