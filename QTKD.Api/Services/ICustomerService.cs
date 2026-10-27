using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

/// <summary>
/// Giao diện xử lý nghiệp vụ cho Khách hàng
/// </summary>
public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
}
