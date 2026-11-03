using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs;
using QTKD.Api.Services;

namespace QTKD.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// Lấy danh sách tất cả khách hàng
    /// GET /api/customers
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CustomerDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CustomerDto>>>> GetAll()
    {
        var customers = await _customerService.GetAllCustomersAsync();
        return Ok(ApiResponse<IEnumerable<CustomerDto>>.Ok(customers, "Lấy danh sách khách hàng thành công"));
    }

    /// <summary>
    /// Lấy thông tin chi tiết khách hàng theo Id
    /// GET /api/customers/{id}
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetById(int id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null)
        {
            return NotFound(ApiResponse<CustomerDto>.Fail($"Không tìm thấy khách hàng với mã Id = {id}"));
        }

        return Ok(ApiResponse<CustomerDto>.Ok(customer, "Lấy thông tin khách hàng thành công"));
    }

    /// <summary>
    /// Tạo mới khách hàng
    /// POST /api/customers
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Create([FromBody] CreateCustomerDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<CustomerDto>.Fail("Dữ liệu khách hàng không hợp lệ", errors));
        }

        var created = await _customerService.CreateCustomerAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<CustomerDto>.Ok(created, "Tạo khách hàng thành công"));
    }

    /// <summary>
    /// Cập nhật thông tin khách hàng
    /// PUT /api/customers/{id}
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Update(int id, [FromBody] UpdateCustomerDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<CustomerDto>.Fail("Dữ liệu cập nhật không hợp lệ", errors));
        }

        var updated = await _customerService.UpdateCustomerAsync(id, dto);
        if (updated == null)
        {
            return NotFound(ApiResponse<CustomerDto>.Fail($"Không tìm thấy khách hàng với mã Id = {id}"));
        }

        return Ok(ApiResponse<CustomerDto>.Ok(updated, "Cập nhật khách hàng thành công"));
    }
}
