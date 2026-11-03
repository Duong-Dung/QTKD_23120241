using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

/// <summary>
/// Giao diện dịch vụ quản lý Bảng giá (PriceList) và Mục giá dịch vụ (PriceItem)
/// </summary>
public interface IPriceListService
{
    Task<IEnumerable<PriceListDto>> GetAllPriceListsAsync();
    Task<PriceListDto?> GetPriceListByIdAsync(int id);
    Task<PriceListDto> CreatePriceListAsync(CreatePriceListDto dto);
    Task<PriceItemDto> AddPriceItemAsync(int priceListId, CreatePriceItemDto dto);
    Task<IEnumerable<PriceItemDto>> GetPriceItemsByPriceListIdAsync(int priceListId);
}
