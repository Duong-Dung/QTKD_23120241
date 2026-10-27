using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

/// <summary>
/// Cài đặt nghiệp vụ lấy dữ liệu Khách hàng
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly QtkdDbContext _context;

    public CustomerService(QtkdDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
    {
        return await _context.Customers
            .AsNoTracking()
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                Status = c.Status
            })
            .ToListAsync();
    }
}
