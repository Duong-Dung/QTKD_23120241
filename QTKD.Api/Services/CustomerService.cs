using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.DTOs;
using QTKD.Api.Entities;
using QTKD.Api.Exceptions;

namespace QTKD.Api.Services;

/// <summary>
/// Cài đặt nghiệp vụ xử lý dữ liệu Khách hàng
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

    public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null) return null;

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Status = customer.Status
        };
    }

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new BusinessRuleException("Tên khách hàng không được để trống.");
        }

        var customer = new Customer
        {
            Name = dto.Name.Trim(),
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status.Trim()
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Status = customer.Status
        };
    }

    public async Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerDto dto)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return null;

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new BusinessRuleException("Tên khách hàng không được để trống.");
        }

        customer.Name = dto.Name.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            customer.Status = dto.Status.Trim();
        }

        await _context.SaveChangesAsync();

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Status = customer.Status
        };
    }
}
