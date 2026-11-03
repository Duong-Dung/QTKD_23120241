using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

/// <summary>
/// Giao diện dịch vụ nghiệp vụ Khách hàng
/// </summary>
public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
    Task<CustomerDto?> GetCustomerByIdAsync(int id);
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto);
    Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerDto dto);
}
