using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs;
using QTKD.Api.Exceptions;
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
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PriceListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PriceListDto>>>> GetAll()
    {
        var priceLists = await _priceListService.GetAllPriceListsAsync();
        return Ok(ApiResponse<IEnumerable<PriceListDto>>.Ok(priceLists, "Lấy danh sách bảng giá thành công"));
    }

    /// <summary>
    /// Lấy thông tin chi tiết bảng giá theo Id
    /// GET /api/pricelists/{id}
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PriceListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PriceListDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PriceListDto>>> GetById(int id)
    {
        var priceList = await _priceListService.GetPriceListByIdAsync(id);
        if (priceList == null)
        {
            return NotFound(ApiResponse<PriceListDto>.Fail($"Không tìm thấy bảng giá với mã Id = {id}"));
        }

        return Ok(ApiResponse<PriceListDto>.Ok(priceList, "Lấy thông tin bảng giá thành công"));
    }

    /// <summary>
    /// Tạo mới bảng giá
    /// POST /api/pricelists
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PriceListDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<PriceListDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PriceListDto>>> Create([FromBody] CreatePriceListDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PriceListDto>.Fail("Dữ liệu bảng giá không hợp lệ", errors));
        }

        var created = await _priceListService.CreatePriceListAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<PriceListDto>.Ok(created, "Tạo bảng giá thành công"));
    }

    /// <summary>
    /// Thêm mục giá dịch vụ vào bảng giá
    /// POST /api/pricelists/{id}/items
    /// </summary>
    [HttpPost("{id:int}/items")]
    [ProducesResponseType(typeof(ApiResponse<PriceItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<PriceItemDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<PriceItemDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PriceItemDto>>> AddPriceItem(int id, [FromBody] CreatePriceItemDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<PriceItemDto>.Fail("Dữ liệu chi tiết giá không hợp lệ", errors));
        }

        try
        {
            var createdItem = await _priceListService.AddPriceItemAsync(id, dto);
            return StatusCode(StatusCodes.Status201Created, ApiResponse<PriceItemDto>.Ok(createdItem, "Thêm mục giá vào bảng giá thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PriceItemDto>.Fail(ex.Message));
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(ApiResponse<PriceItemDto>.Fail(ex.Message, ex.Errors));
        }
    }

    /// <summary>
    /// Lấy danh sách các mục giá thuộc bảng giá
    /// GET /api/pricelists/{id}/items
    /// </summary>
    [HttpGet("{id:int}/items")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PriceItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PriceItemDto>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PriceItemDto>>>> GetPriceItems(int id)
    {
        try
        {
            var items = await _priceListService.GetPriceItemsByPriceListIdAsync(id);
            return Ok(ApiResponse<IEnumerable<PriceItemDto>>.Ok(items, "Lấy danh sách chi tiết giá thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<IEnumerable<PriceItemDto>>.Fail(ex.Message));
        }
    }
}
