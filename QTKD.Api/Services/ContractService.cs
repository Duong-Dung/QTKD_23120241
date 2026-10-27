using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

/// <summary>
/// Cài đặt nghiệp vụ lấy dữ liệu Hợp đồng
/// </summary>
public class ContractService : IContractService
{
    private readonly QtkdDbContext _context;

    public ContractService(QtkdDbContext context)
    {
        _context = context;
    }

    public async Task<ContractDto?> GetContractByIdAsync(int id)
    {
        var contract = await _context.Contracts
            .Include(c => c.Customer)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract == null)
        {
            return null;
        }

        return new ContractDto
        {
            Id = contract.Id,
            ContractNo = contract.ContractNo,
            CustomerId = contract.CustomerId,
            CustomerName = contract.Customer?.Name ?? string.Empty,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            Status = contract.Status
        };
    }
}
