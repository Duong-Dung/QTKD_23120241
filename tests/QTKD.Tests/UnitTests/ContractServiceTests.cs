using Microsoft.EntityFrameworkCore;
using QTKD.Api.DTOs;
using QTKD.Api.Entities;
using QTKD.Api.Exceptions;
using QTKD.Api.Services;
using QTKD.Tests.Common;

namespace QTKD.Tests.UnitTests;

/// <summary>
/// Unit test kiểm tra các quy tắc nghiệp vụ CTR-01, CTR-02, CTR-03 và chống gửi trùng lặp (TC03 - TC08)
/// </summary>
public class ContractServiceTests
{
    // =========================================================================
    // CTR-01: Chỉ cho sửa hợp đồng ở Draft hoặc Revision Requested
    // =========================================================================

    [Fact]
    public async Task TC03_UpdateContract_InDraftStatus_WithValidData_ShouldSucceed()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new ContractService(context);

        // Hợp đồng Id = 1 trong seed data đang ở trạng thái Draft
        var updateDto = new UpdateContractDto
        {
            TotalAmount = 75000000m,
            Terms = "Điều khoản hợp đồng đã được bổ sung đầy đủ và cập nhật chi tiết",
            AttachmentUrl = "https://storage.qtkd.vn/contracts/HD-2026-018-v2.pdf"
        };

        // Act
        var result = await service.UpdateContractAsync(1, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(75000000m, result.TotalAmount);
        Assert.Equal("Điều khoản hợp đồng đã được bổ sung đầy đủ và cập nhật chi tiết", result.Terms);
        Assert.Equal("https://storage.qtkd.vn/contracts/HD-2026-018-v2.pdf", result.AttachmentUrl);
        Assert.Equal("Draft", result.Status);

        // Xác minh trong DB
        var inDb = await context.Contracts.FindAsync(1);
        Assert.NotNull(inDb);
        Assert.Equal(75000000m, inDb.TotalAmount);
    }

    [Fact]
    public async Task UpdateContract_InRevisionRequestedStatus_ShouldSucceed()
    {
        // Arrange: Tạo hợp đồng ở trạng thái Revision Requested
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new ContractService(context);

        var contract = new Contract
        {
            ContractNo = "HD-2026-REV",
            CustomerId = 1,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6),
            TotalAmount = 20000000m,
            Terms = "Hợp đồng chờ chỉnh sửa theo yêu cầu lãnh đạo",
            AttachmentUrl = "https://storage.qtkd.vn/contracts/HD-REV.pdf",
            Status = "Revision Requested"
        };
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();

        var updateDto = new UpdateContractDto
        {
            TotalAmount = 25000000m,
            Terms = "Hợp đồng đã chỉnh sửa xong toàn bộ theo yêu cầu"
        };

        // Act
        var result = await service.UpdateContractAsync(contract.Id, updateDto);

        // Assert: Thành công cập nhật
        Assert.NotNull(result);
        Assert.Equal(25000000m, result.TotalAmount);
        Assert.Equal("Revision Requested", result.Status);
    }

    [Fact]
    public async Task TC04_UpdateContract_InPendingApprovalStatus_ShouldBeRejectedAndKeepDataUnchanged()
    {
        // Arrange: Đặt hợp đồng sang trạng thái PendingApproval
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new ContractService(context);

        var contract = await context.Contracts.FindAsync(1);
        Assert.NotNull(contract);
        contract.Status = "PendingApproval";
        await context.SaveChangesAsync();

        var originalAmount = contract.TotalAmount;
        var originalTerms = contract.Terms;

        var updateDto = new UpdateContractDto
        {
            TotalAmount = 999999999m,
            Terms = "Cố ý sửa nội dung khi đã gửi duyệt"
        };

        // Act & Assert: Vi phạm CTR-01, phải ném BusinessRuleException
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.UpdateContractAsync(1, updateDto));
        Assert.Contains("[CTR-01]", ex.Message);
        Assert.Contains("PendingApproval", ex.Message);

        // Kiểm tra dữ liệu trong DB không bị thay đổi
        var inDb = await context.Contracts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == 1);
        Assert.NotNull(inDb);
        Assert.Equal(originalAmount, inDb.TotalAmount);
        Assert.Equal(originalTerms, inDb.Terms);
        Assert.Equal("PendingApproval", inDb.Status);
    }

    // =========================================================================
    // CTR-02: Chỉ gửi khi Customer, ngày hiệu lực, nội dung/tệp bắt buộc hợp lệ
    // =========================================================================

    [Fact]
    public async Task TC05_SubmitContract_MissingRequiredAttachmentOrZeroAmount_ShouldBeRejectedAndRemainDraft()
    {
        // Arrange: Tạo hợp đồng Draft nhưng thiếu AttachmentUrl và TotalAmount = 0
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new ContractService(context);

        var contract = new Contract
        {
            ContractNo = "HD-2026-INCOMPLETE",
            CustomerId = 1, // Khách hàng Active
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6),
            TotalAmount = 0m,           // Vi phạm: Giá trị = 0
            Terms = "Điều khoản tạm",
            AttachmentUrl = null,        // Vi phạm: Thiếu file tài liệu đính kèm
            Status = "Draft"
        };
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();

        // Act & Assert: Phải ném BusinessRuleException chứa danh sách lỗi CTR-02
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.SubmitContractAsync(contract.Id));
        Assert.Contains("[CTR-02]", ex.Message);
        Assert.NotNull(ex.Errors);
        Assert.Contains(ex.Errors, err => err.Contains("TotalAmount"));
        Assert.Contains(ex.Errors, err => err.Contains("AttachmentUrl"));

        // Xác nhận trạng thái vẫn giữ nguyên là Draft
        var inDb = await context.Contracts.FindAsync(contract.Id);
        Assert.NotNull(inDb);
        Assert.Equal("Draft", inDb.Status);
        Assert.Null(inDb.SubmittedAt);
    }

    [Fact]
    public async Task TC05_SubmitContract_WithInactiveCustomer_ShouldBeRejected()
    {
        // Arrange: Khách hàng bị Inactive
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new ContractService(context);

        var inactiveCustomer = new Customer
        {
            Name = "Công ty Giải Thể XYZ",
            Status = "Inactive"
        };
        context.Customers.Add(inactiveCustomer);
        await context.SaveChangesAsync();

        var contract = new Contract
        {
            ContractNo = "HD-2026-INACTIVE-CUST",
            CustomerId = inactiveCustomer.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6),
            TotalAmount = 10000000m,
            Terms = "Điều khoản hợp đồng với khách hàng ngưng hoạt động",
            AttachmentUrl = "https://storage.qtkd.vn/contracts/doc.pdf",
            Status = "Draft"
        };
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.SubmitContractAsync(contract.Id));
        Assert.Contains("[CTR-02]", ex.Message);
        Assert.Contains(ex.Errors, err => err.Contains("không ở trạng thái hoạt động"));

        var inDb = await context.Contracts.FindAsync(contract.Id);
        Assert.NotNull(inDb);
        Assert.Equal("Draft", inDb.Status);
    }

    [Fact]
    public async Task TC06_SubmitContract_WithAllValidConditions_ShouldChangeToPendingApproval()
    {
        // Arrange: Hợp đồng Id = 1 có đầy đủ Customer Active, Ngày hợp lệ, TotalAmount > 0, Terms, AttachmentUrl
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new ContractService(context);

        // Act
        var result = await service.SubmitContractAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Draft", result.PreviousStatus);
        Assert.Equal("PendingApproval", result.NewStatus);
        Assert.NotEqual(default, result.SubmittedAt);

        // Kiểm tra trong DB
        var inDb = await context.Contracts.FindAsync(1);
        Assert.NotNull(inDb);
        Assert.Equal("PendingApproval", inDb.Status);
        Assert.NotNull(inDb.SubmittedAt);
    }

    // =========================================================================
    // CTR-03: Không được chuyển Draft thẳng sang Approved
    // =========================================================================

    [Fact]
    public async Task TC06_CTR03_SubmitContract_MustOnlyTransitionToPendingApproval_NeverApproved()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new ContractService(context);

        // Act
        var result = await service.SubmitContractAsync(1);

        // Assert: Tuyệt đối không được chuyển thành Approved
        Assert.Equal("PendingApproval", result.NewStatus);
        Assert.NotEqual("Approved", result.NewStatus);

        var inDb = await context.Contracts.FindAsync(1);
        Assert.NotNull(inDb);
        Assert.Equal("PendingApproval", inDb.Status);
    }

    [Fact]
    public async Task TC07_DirectUpdateToApprovedStatus_ShouldBeRejected()
    {
        // Arrange: Cố ý gửi yêu cầu cập nhật chuyển Status sang "Approved"
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new ContractService(context);

        var updateDto = new UpdateContractDto
        {
            Status = "Approved" // Cố ý nhảy cóc sang Approved mà không qua phê duyệt
        };

        // Act & Assert: Phải bị từ chối theo CTR-03
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.UpdateContractAsync(1, updateDto));
        Assert.Contains("[CTR-03]", ex.Message);
        Assert.Contains("Approved", ex.Message);

        // Kiểm tra hợp đồng vẫn giữ nguyên trạng thái Draft
        var inDb = await context.Contracts.FindAsync(1);
        Assert.NotNull(inDb);
        Assert.Equal("Draft", inDb.Status);
    }

    // =========================================================================
    // Yêu cầu 12 (Idempotency): Chống gửi trình duyệt trùng lặp
    // =========================================================================

    [Fact]
    public async Task TC08_SubmitContract_WhenAlreadyPendingApproval_ShouldThrowException()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new ContractService(context);

        // Lần 1: Gửi duyệt thành công -> Chuyển sang PendingApproval
        var firstResult = await service.SubmitContractAsync(1);
        Assert.Equal("PendingApproval", firstResult.NewStatus);

        // Act & Assert (Lần 2): Gửi lại cùng hợp đồng đang ở PendingApproval
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.SubmitContractAsync(1));
        Assert.Contains("đã được gửi trình duyệt trước đó", ex.Message);
        Assert.Contains("PendingApproval", ex.Message);

        // Xác nhận trạng thái không bị thay đổi và không tạo thêm dữ liệu thừa
        var inDb = await context.Contracts.FindAsync(1);
        Assert.NotNull(inDb);
        Assert.Equal("PendingApproval", inDb.Status);
        Assert.Equal(firstResult.SubmittedAt, inDb.SubmittedAt);
    }
}

