using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

/// <summary>
/// Giao diện dịch vụ nghiệp vụ Hợp đồng
/// </summary>
public interface IContractService
{
    Task<IEnumerable<ContractDto>> GetAllContractsAsync();
    Task<ContractDto?> GetContractByIdAsync(int id);
    Task<ContractDto> CreateContractAsync(CreateContractDto dto);
    Task<ContractDto?> UpdateContractAsync(int id, UpdateContractDto dto);
    Task<SubmitContractResultDto> SubmitContractAsync(int id);
}
