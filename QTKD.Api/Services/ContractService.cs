using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.DTOs;
using QTKD.Api.Entities;
using QTKD.Api.Exceptions;

namespace QTKD.Api.Services;

/// <summary>
/// Cài đặt nghiệp vụ xử lý Hợp đồng và kiểm tra các quy tắc nghiệp vụ CTR-01, CTR-02, CTR-03
/// </summary>
public class ContractService : IContractService
{
    private readonly QtkdDbContext _context;

    public ContractService(QtkdDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ContractDto>> GetAllContractsAsync()
    {
        return await _context.Contracts
            .Include(c => c.Customer)
            .AsNoTracking()
            .Select(c => new ContractDto
            {
                Id = c.Id,
                ContractNo = c.ContractNo,
                CustomerId = c.CustomerId,
                CustomerName = c.Customer != null ? c.Customer.Name : string.Empty,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                TotalAmount = c.TotalAmount,
                Terms = c.Terms,
                AttachmentUrl = c.AttachmentUrl,
                Status = c.Status,
                SubmittedAt = c.SubmittedAt
            })
            .ToListAsync();
    }

    public async Task<ContractDto?> GetContractByIdAsync(int id)
    {
        var contract = await _context.Contracts
            .Include(c => c.Customer)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract == null) return null;

        return MapToDto(contract);
    }

    public async Task<ContractDto> CreateContractAsync(CreateContractDto dto)
    {
        // 1. Kiểm tra khách hàng tham chiếu có tồn tại không
        var customer = await _context.Customers.FindAsync(dto.CustomerId);
        if (customer == null)
        {
            throw new BusinessRuleException($"Khách hàng với mã Id = {dto.CustomerId} không tồn tại trong hệ thống.");
        }

        // 2. Kiểm tra ngày hiệu lực: StartDate < EndDate
        var startUtc = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc);
        var endUtc = DateTime.SpecifyKind(dto.EndDate, DateTimeKind.Utc);
        if (startUtc >= endUtc)
        {
            throw new BusinessRuleException("Ngày bắt đầu hợp đồng (StartDate) phải trước ngày kết thúc (EndDate).");
        }

        // 3. Kiểm tra số hợp đồng không được trùng lặp
        var existingNo = await _context.Contracts
            .AnyAsync(c => c.ContractNo.ToLower() == dto.ContractNo.Trim().ToLower());
        if (existingNo)
        {
            throw new BusinessRuleException($"Số hợp đồng '{dto.ContractNo}' đã tồn tại trong hệ thống.");
        }

        // 4. Khởi tạo hợp đồng mới - luôn ở trạng thái "Draft"
        var contract = new Contract
        {
            ContractNo = dto.ContractNo.Trim(),
            CustomerId = dto.CustomerId,
            StartDate = startUtc,
            EndDate = endUtc,
            TotalAmount = dto.TotalAmount,
            Terms = dto.Terms?.Trim(),
            AttachmentUrl = dto.AttachmentUrl?.Trim(),
            Status = "Draft",
            SubmittedAt = null
        };

        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync();

        contract.Customer = customer;
        return MapToDto(contract);
    }

    public async Task<ContractDto?> UpdateContractAsync(int id, UpdateContractDto dto)
    {
        var contract = await _context.Contracts
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract == null) return null;

        // [QUY TẮC CTR-01] Chỉ cho phép sửa hợp đồng ở trạng thái "Draft" hoặc "Revision Requested"
        // Thử sửa Contract ở PendingApproval: phải bị từ chối
        var allowedStatuses = new[] { "Draft", "Revision Requested" };
        if (!allowedStatuses.Contains(contract.Status, StringComparer.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException(
                $"[CTR-01] Không thể chỉnh sửa hợp đồng khi ở trạng thái '{contract.Status}'. " +
                "Chỉ cho phép sửa hợp đồng khi ở trạng thái 'Draft' hoặc 'Revision Requested'.");
        }

        // Kiểm tra số hợp đồng nếu có thay đổi
        if (!string.IsNullOrWhiteSpace(dto.ContractNo) && !dto.ContractNo.Equals(contract.ContractNo, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _context.Contracts
                .AnyAsync(c => c.Id != id && c.ContractNo.ToLower() == dto.ContractNo.Trim().ToLower());
            if (exists)
            {
                throw new BusinessRuleException($"Số hợp đồng '{dto.ContractNo}' đã được sử dụng bởi hợp đồng khác.");
            }
            contract.ContractNo = dto.ContractNo.Trim();
        }

        // Kiểm tra ngày hiệu lực
        var effectiveStart = dto.StartDate.HasValue ? DateTime.SpecifyKind(dto.StartDate.Value, DateTimeKind.Utc) : contract.StartDate;
        var effectiveEnd = dto.EndDate.HasValue ? DateTime.SpecifyKind(dto.EndDate.Value, DateTimeKind.Utc) : contract.EndDate;

        if (effectiveStart >= effectiveEnd)
        {
            throw new BusinessRuleException("Ngày bắt đầu hợp đồng (StartDate) phải trước ngày kết thúc (EndDate).");
        }

        if (dto.StartDate.HasValue) contract.StartDate = effectiveStart;
        if (dto.EndDate.HasValue) contract.EndDate = effectiveEnd;
        if (dto.TotalAmount.HasValue) contract.TotalAmount = dto.TotalAmount.Value;
        if (dto.Terms != null) contract.Terms = dto.Terms.Trim();
        if (dto.AttachmentUrl != null) contract.AttachmentUrl = dto.AttachmentUrl.Trim();

        // Lưu ý: Không cho phép cập nhật trực tiếp Status qua API sửa thông thường -> Bảo đảm CTR-03
        await _context.SaveChangesAsync();

        return MapToDto(contract);
    }

    public async Task<SubmitContractResultDto> SubmitContractAsync(int id)
    {
        var contract = await _context.Contracts
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy hợp đồng với mã Id = {id}.");
        }

        // 1. Kiểm tra trạng thái hiện tại (US03 / Yêu cầu 11 & 12)
        // Nếu đã ở PendingApproval -> Từ chối gửi lặp lại
        if (string.Equals(contract.Status, "PendingApproval", StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException(
                $"Hợp đồng '{contract.ContractNo}' đã được gửi trình duyệt trước đó (trạng thái hiện tại: PendingApproval). " +
                "Không được gửi trình duyệt trùng lặp.");
        }

        // Chỉ cho phép gửi duyệt khi đang ở trạng thái Draft hoặc Revision Requested
        var validSubmitStatuses = new[] { "Draft", "Revision Requested" };
        if (!validSubmitStatuses.Contains(contract.Status, StringComparer.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException(
                $"Chỉ hợp đồng ở trạng thái 'Draft' hoặc 'Revision Requested' mới được phép gửi trình duyệt. " +
                $"Trạng thái hiện tại: '{contract.Status}'.");
        }

        // 2. [QUY TẮC CTR-02] Kiểm tra các điều kiện bắt buộc trước khi gửi trình duyệt
        // Chỉ gửi khi Customer, ngày hiệu lực, nội dung/tệp bắt buộc hợp lệ. Thiếu một điều kiện: gửi thất bại, dữ liệu không đổi.
        var validationErrors = new List<string>();

        // a) Kiểm tra Customer hợp lệ
        if (contract.Customer == null)
        {
            validationErrors.Add("Hợp đồng chưa được liên kết với khách hàng hợp lệ.");
        }
        else if (!string.Equals(contract.Customer.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            validationErrors.Add($"Khách hàng '{contract.Customer.Name}' hiện không ở trạng thái hoạt động (Status: '{contract.Customer.Status}').");
        }

        // b) Kiểm tra ngày hiệu lực
        if (contract.StartDate >= contract.EndDate)
        {
            validationErrors.Add("Ngày bắt đầu hợp đồng phải trước ngày kết thúc.");
        }

        // c) Kiểm tra nội dung bắt buộc: Giá trị hợp đồng > 0 & Điều khoản hợp đồng
        if (contract.TotalAmount <= 0)
        {
            validationErrors.Add("Giá trị hợp đồng (TotalAmount) phải lớn hơn 0 VNĐ.");
        }

        if (string.IsNullOrWhiteSpace(contract.Terms) || contract.Terms.Trim().Length < 5)
        {
            validationErrors.Add("Hợp đồng thiếu nội dung điều khoản bắt buộc (Terms) hoặc nội dung quá ngắn.");
        }

        // d) Kiểm tra tệp tài liệu đính kèm bắt buộc (giả định theo yêu cầu kịch bản đề bài)
        if (string.IsNullOrWhiteSpace(contract.AttachmentUrl))
        {
            validationErrors.Add("Hợp đồng thiếu tệp tài liệu/hồ sơ đính kèm bắt buộc (AttachmentUrl).");
        }

        // Nếu thiếu bất kỳ điều kiện nào: gửi thất bại, dữ liệu trong Contract giữ nguyên không đổi!
        if (validationErrors.Count > 0)
        {
            throw new BusinessRuleException(
                "[CTR-02] Hợp đồng không đủ điều kiện gửi trình duyệt do thiếu thông tin hoặc thông tin không hợp lệ.",
                validationErrors);
        }

        // 3. [QUY TẮC CTR-03] Không được chuyển Draft thẳng sang Approved.
        // Submit thành công CHỈ CHUYỂN SANG PendingApproval.
        var previousStatus = contract.Status;
        contract.Status = "PendingApproval";
        contract.SubmittedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        await _context.SaveChangesAsync();

        return new SubmitContractResultDto
        {
            ContractId = contract.Id,
            ContractNo = contract.ContractNo,
            PreviousStatus = previousStatus,
            NewStatus = contract.Status,
            SubmittedAt = contract.SubmittedAt.Value,
            Message = $"Hợp đồng {contract.ContractNo} đã được gửi trình duyệt thành công và chuyển sang trạng thái PendingApproval."
        };
    }

    private static ContractDto MapToDto(Contract contract)
    {
        return new ContractDto
        {
            Id = contract.Id,
            ContractNo = contract.ContractNo,
            CustomerId = contract.CustomerId,
            CustomerName = contract.Customer != null ? contract.Customer.Name : string.Empty,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            TotalAmount = contract.TotalAmount,
            Terms = contract.Terms,
            AttachmentUrl = contract.AttachmentUrl,
            Status = contract.Status,
            SubmittedAt = contract.SubmittedAt
        };
    }
}
