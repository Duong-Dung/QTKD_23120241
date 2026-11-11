using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QTKD.Api.Controllers;
using QTKD.Api.DTOs;
using QTKD.Api.Services;
using QTKD.Tests.Common;

namespace QTKD.Tests.IntegrationTests;

/// <summary>
/// Integration test cho Customer API: TC01 và TC02
/// Kiểm tra HTTP status code, nội dung phản hồi ApiResponse<T> và dữ liệu trong Database
/// </summary>
public class CustomerApiIntegrationTests
{
    [Fact]
    public async Task TC01_Api_CreateCustomer_WithValidData_Returns201AndPersistsInDatabase()
    {
        // Arrange: Khởi tạo database kiểm thử riêng biệt
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new CustomerService(context);
        var controller = new CustomersController(service);

        var dto = new CreateCustomerDto
        {
            Name = "Công ty TNHH Giải pháp Phần mềm Ánh Dương",
            Status = "Active"
        };

        // Act: Gọi API POST /api/customers
        var actionResult = await controller.Create(dto);

        // Assert 1: Kiểm tra HTTP status code trả về là 201 Created
        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);

        // Assert 2: Kiểm tra nội dung phản hồi ApiResponse<CustomerDto>
        var apiResponse = Assert.IsType<ApiResponse<CustomerDto>>(createdResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.True(apiResponse.Data.Id > 0);
        Assert.Equal("Công ty TNHH Giải pháp Phần mềm Ánh Dương", apiResponse.Data.Name);
        Assert.Equal("Active", apiResponse.Data.Status);

        var createdId = apiResponse.Data.Id;

        // Assert 3: Gọi API GET /api/customers/{id} để đọc lại
        var getActionResult = await controller.GetById(createdId);
        var okResult = Assert.IsType<OkObjectResult>(getActionResult.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        // Assert 4: Kiểm tra trực tiếp dữ liệu sau khi gọi API trong database
        var customerInDb = await context.Customers.FindAsync(createdId);
        Assert.NotNull(customerInDb);
        Assert.Equal("Công ty TNHH Giải pháp Phần mềm Ánh Dương", customerInDb.Name);
        Assert.Equal("Active", customerInDb.Status);
    }

    [Fact]
    public async Task TC02_Api_CreateCustomer_MissingRequiredName_Returns400BadRequestAndNotPersisted()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var service = new CustomerService(context);
        var controller = new CustomersController(service);

        var initialCount = context.Customers.Count();

        var invalidDto = new CreateCustomerDto
        {
            Name = "", // Tên rỗng vi phạm quy định bắt buộc
            Status = "Active"
        };
        controller.ModelState.AddModelError("Name", "Tên khách hàng không được để trống");

        // Act: Gọi API POST /api/customers
        var actionResult = await controller.Create(invalidDto);

        // Assert 1: HTTP status code trả về là 400 Bad Request
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

        // Assert 2: Phản hồi có thông báo lỗi cụ thể
        var apiResponse = Assert.IsType<ApiResponse<CustomerDto>>(badRequestResult.Value);
        Assert.False(apiResponse.Success);
        Assert.True(
            !string.IsNullOrWhiteSpace(apiResponse.Message) ||
            (apiResponse.Errors != null && apiResponse.Errors.Count > 0));

        // Assert 3: Dữ liệu trong database không thay đổi, không tạo thêm bản ghi rác
        Assert.Equal(initialCount, context.Customers.Count());
    }
}

