using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

public interface IPriceListService
{
    Task<IEnumerable<PriceListDto>> GetAllPriceListsAsync();
}

public class PriceListService : IPriceListService
{
    private readonly QtkdDbContext _context;

    public PriceListService(QtkdDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PriceListDto>> GetAllPriceListsAsync()
    {
        return await _context.PriceLists
            .Include(p => p.PriceItems)
            .AsNoTracking()
            .Select(p => new PriceListDto
            {
                Id = p.Id,
                Name = p.Name,
                EffectiveDate = p.EffectiveDate,
                Items = p.PriceItems.Select(i => new PriceItemDto
                {
                    Id = i.Id,
                    ServiceCode = i.ServiceCode,
                    UnitPrice = i.UnitPrice
                }).ToList()
            })
            .ToListAsync();
    }
}
