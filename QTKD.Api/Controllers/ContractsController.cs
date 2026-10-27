using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs;
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
    /// Lấy thông tin chi tiết hợp đồng theo Id
    /// GET /api/contracts/{id}
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ContractDto>> GetById(int id)
    {
        var contract = await _contractService.GetContractByIdAsync(id);
        if (contract == null)
        {
            return NotFound(new { message = $"Không tìm thấy hợp đồng với mã Id = {id}" });
        }

        return Ok(contract);
    }
}
