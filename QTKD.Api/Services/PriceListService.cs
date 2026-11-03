using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.DTOs;
using QTKD.Api.Entities;
using QTKD.Api.Exceptions;

namespace QTKD.Api.Services;

/// <summary>
/// Cài đặt nghiệp vụ xử lý Bảng giá và Mục giá dịch vụ
/// </summary>
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
                    PriceListId = i.PriceListId,
                    ServiceCode = i.ServiceCode,
                    UnitPrice = i.UnitPrice
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<PriceListDto?> GetPriceListByIdAsync(int id)
    {
        var priceList = await _context.PriceLists
            .Include(p => p.PriceItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (priceList == null) return null;

        return new PriceListDto
        {
            Id = priceList.Id,
            Name = priceList.Name,
            EffectiveDate = priceList.EffectiveDate,
            Items = priceList.PriceItems.Select(i => new PriceItemDto
            {
                Id = i.Id,
                PriceListId = i.PriceListId,
                ServiceCode = i.ServiceCode,
                UnitPrice = i.UnitPrice
            }).ToList()
        };
    }

    public async Task<PriceListDto> CreatePriceListAsync(CreatePriceListDto dto)
    {
        var priceList = new PriceList
        {
            Name = dto.Name.Trim(),
            EffectiveDate = DateTime.SpecifyKind(dto.EffectiveDate, DateTimeKind.Utc)
        };

        _context.PriceLists.Add(priceList);
        await _context.SaveChangesAsync();

        return new PriceListDto
        {
            Id = priceList.Id,
            Name = priceList.Name,
            EffectiveDate = priceList.EffectiveDate,
            Items = new List<PriceItemDto>()
        };
    }

    public async Task<PriceItemDto> AddPriceItemAsync(int priceListId, CreatePriceItemDto dto)
    {
        // 1. Kiểm tra bảng giá liên kết có tồn tại không
        var priceList = await _context.PriceLists.FindAsync(priceListId);
        if (priceList == null)
        {
            throw new KeyNotFoundException($"Bảng giá với mã Id = {priceListId} không tồn tại.");
        }

        // 2. Kiểm tra đơn giá hợp lệ (> 0)
        if (dto.UnitPrice <= 0)
        {
            throw new BusinessRuleException("Đơn giá dịch vụ (UnitPrice) phải lớn hơn 0 VNĐ.");
        }

        var item = new PriceItem
        {
            PriceListId = priceListId,
            ServiceCode = dto.ServiceCode.Trim(),
            UnitPrice = dto.UnitPrice
        };

        _context.PriceItems.Add(item);
        await _context.SaveChangesAsync();

        return new PriceItemDto
        {
            Id = item.Id,
            PriceListId = item.PriceListId,
            ServiceCode = item.ServiceCode,
            UnitPrice = item.UnitPrice
        };
    }

    public async Task<IEnumerable<PriceItemDto>> GetPriceItemsByPriceListIdAsync(int priceListId)
    {
        var priceListExists = await _context.PriceLists.AnyAsync(p => p.Id == priceListId);
        if (!priceListExists)
        {
            throw new KeyNotFoundException($"Bảng giá với mã Id = {priceListId} không tồn tại.");
        }

        return await _context.PriceItems
            .AsNoTracking()
            .Where(pi => pi.PriceListId == priceListId)
            .Select(pi => new PriceItemDto
            {
                Id = pi.Id,
                PriceListId = pi.PriceListId,
                ServiceCode = pi.ServiceCode,
                UnitPrice = pi.UnitPrice
            })
            .ToListAsync();
    }
}
