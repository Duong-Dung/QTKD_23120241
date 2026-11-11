using QTKD.Api.DTOs;
using QTKD.Api.Exceptions;
using QTKD.Api.Services;
using QTKD.Tests.Common;

namespace QTKD.Tests.UnitTests;

/// <summary>
/// Unit test kiểm tra tầng nghiệp vụ CustomerService (TC01, TC02)
/// </summary>
public class CustomerServiceTests
{
    [Fact]
    public async Task TC01_CreateCustomer_WithValidData_ShouldSucceedAndPersistInDb()
    {
        // Arrange: Chuẩn bị DbContext in-memory độc lập
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new CustomerService(context);

        var dto = new CreateCustomerDto
        {
            Name = "Công ty TNHH Đầu tư Nam Long",
            Status = "Active"
        };

        // Act: Gọi service tạo khách hàng mới
        var result = await service.CreateCustomerAsync(dto);

        // Assert: Kiểm tra dữ liệu trả về và dữ liệu lưu trong DB
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("Công ty TNHH Đầu tư Nam Long", result.Name);
        Assert.Equal("Active", result.Status);

        // Đọc lại từ database để xác minh tính toàn vẹn
        var dbCustomer = await context.Customers.FindAsync(result.Id);
        Assert.NotNull(dbCustomer);
        Assert.Equal("Công ty TNHH Đầu tư Nam Long", dbCustomer.Name);
        Assert.Equal("Active", dbCustomer.Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task TC02_CreateCustomer_MissingRequiredName_ShouldThrowBusinessRuleException(string? invalidName)
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new CustomerService(context);

        var dto = new CreateCustomerDto
        {
            Name = invalidName!,
            Status = "Active"
        };

        // Act & Assert: Service phải từ chối và ném BusinessRuleException
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateCustomerAsync(dto));
        Assert.Contains("Tên khách hàng không được để trống", ex.Message);
    }

    [Fact]
    public async Task UpdateCustomer_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new CustomerService(context);

        // Khách hàng Id = 1 đã có sẵn trong Seed Data (Công ty TNHH Minh Phát)
        var dto = new UpdateCustomerDto
        {
            Name = "Tập đoàn Công nghệ Minh Phát",
            Status = "Inactive"
        };

        // Act
        var updated = await service.UpdateCustomerAsync(1, dto);

        // Assert
        Assert.NotNull(updated);
        Assert.Equal("Tập đoàn Công nghệ Minh Phát", updated.Name);
        Assert.Equal("Inactive", updated.Status);

        var inDb = await context.Customers.FindAsync(1);
        Assert.NotNull(inDb);
        Assert.Equal("Tập đoàn Công nghệ Minh Phát", inDb.Name);
        Assert.Equal("Inactive", inDb.Status);
    }

    [Fact]
    public async Task UpdateCustomer_WithEmptyName_ShouldThrowBusinessRuleException()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new CustomerService(context);

        var dto = new UpdateCustomerDto
        {
            Name = "   ",
            Status = "Active"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.UpdateCustomerAsync(1, dto));
        Assert.Contains("Tên khách hàng không được để trống", ex.Message);
    }
}

