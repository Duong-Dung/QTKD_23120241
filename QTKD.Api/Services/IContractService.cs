using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

/// <summary>
/// Giao diện xử lý nghiệp vụ cho Hợp đồng
/// </summary>
public interface IContractService
{
    Task<ContractDto?> GetContractByIdAsync(int id);
}
