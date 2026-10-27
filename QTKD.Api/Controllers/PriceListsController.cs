using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs;
using QTKD.Api.Services;

namespace QTKD.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PriceListsController : ControllerBase
{
    private readonly IPriceListService _priceListService;

    public PriceListsController(IPriceListService priceListService)
    {
        _priceListService = priceListService;
    }

    /// <summary>
    /// Lấy danh sách bảng giá và các chi tiết giá
    /// GET /api/pricelists
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PriceListDto>>> GetAll()
    {
        var priceLists = await _priceListService.GetAllPriceListsAsync();
        return Ok(priceLists);
    }
}
