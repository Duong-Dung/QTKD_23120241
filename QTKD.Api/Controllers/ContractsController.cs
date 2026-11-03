using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs;
using QTKD.Api.Exceptions;
using QTKD.Api.Services;

namespace QTKD.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;

    public ContractsController(IContractService contractService)
    {
        _contractService = contractService;
    }

    /// <summary>
    /// Lấy danh sách tất cả hợp đồng
    /// GET /api/contracts
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ContractDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ContractDto>>>> GetAll()
    {
        var contracts = await _contractService.GetAllContractsAsync();
        return Ok(ApiResponse<IEnumerable<ContractDto>>.Ok(contracts, "Lấy danh sách hợp đồng thành công"));
    }

    /// <summary>
    /// Lấy thông tin chi tiết hợp đồng theo Id
    /// GET /api/contracts/{id}
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ContractDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ContractDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ContractDto>>> GetById(int id)
    {
        var contract = await _contractService.GetContractByIdAsync(id);
        if (contract == null)
        {
            return NotFound(ApiResponse<ContractDto>.Fail($"Không tìm thấy hợp đồng với mã Id = {id}"));
        }

        return Ok(ApiResponse<ContractDto>.Ok(contract, "Lấy chi tiết hợp đồng thành công"));
    }

    /// <summary>
    /// Tạo mới hợp đồng ở trạng thái Draft
    /// POST /api/contracts
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ContractDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ContractDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ContractDto>>> Create([FromBody] CreateContractDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ContractDto>.Fail("Dữ liệu tạo hợp đồng không hợp lệ", errors));
        }

        try
        {
            var created = await _contractService.CreateContractAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<ContractDto>.Ok(created, "Tạo hợp đồng thành công ở trạng thái Draft"));
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(ApiResponse<ContractDto>.Fail(ex.Message, ex.Errors));
        }
    }

    /// <summary>
    /// Cập nhật thông tin hợp đồng (Tuân thủ quy tắc CTR-01: Chỉ cho phép sửa ở Draft hoặc Revision Requested)
    /// PUT /api/contracts/{id}
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ContractDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ContractDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ContractDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ContractDto>>> Update(int id, [FromBody] UpdateContractDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ContractDto>.Fail("Dữ liệu cập nhật không hợp lệ", errors));
        }

        try
        {
            var updated = await _contractService.UpdateContractAsync(id, dto);
            if (updated == null)
            {
                return NotFound(ApiResponse<ContractDto>.Fail($"Không tìm thấy hợp đồng với mã Id = {id}"));
            }

            return Ok(ApiResponse<ContractDto>.Ok(updated, "Cập nhật hợp đồng thành công"));
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(ApiResponse<ContractDto>.Fail(ex.Message, ex.Errors));
        }
    }

    /// <summary>
    /// Gửi hợp đồng trình duyệt (User Story US03)
    /// Kiểm tra toàn bộ quy tắc CTR-01, CTR-02, CTR-03 trước khi chuyển sang trạng thái PendingApproval
    /// POST /api/contracts/{id}/submit
    /// </summary>
    [HttpPost("{id:int}/submit")]
    [ProducesResponseType(typeof(ApiResponse<SubmitContractResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SubmitContractResultDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<SubmitContractResultDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SubmitContractResultDto>>> Submit(int id)
    {
        try
        {
            var result = await _contractService.SubmitContractAsync(id);
            return Ok(ApiResponse<SubmitContractResultDto>.Ok(result, "Hợp đồng đã được gửi trình duyệt thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<SubmitContractResultDto>.Fail(ex.Message));
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(ApiResponse<SubmitContractResultDto>.Fail(ex.Message, ex.Errors));
        }
    }
}
