using System.ComponentModel.DataAnnotations;

namespace QTKD.Api.DTOs;

/// <summary>
/// DTO trả về chi tiết hợp đồng
/// </summary>
public class ContractDto
{
    public int Id { get; set; }
    public string ContractNo { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Terms { get; set; }
    public string? AttachmentUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
}

/// <summary>
/// DTO tạo mới hợp đồng (mặc định trạng thái là Draft)
/// </summary>
public class CreateContractDto
{
    [Required(ErrorMessage = "Số hợp đồng không được để trống")]
    [StringLength(50, ErrorMessage = "Số hợp đồng không được vượt quá 50 ký tự")]
    public string ContractNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mã khách hàng không được để trống")]
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Ngày kết thúc không được để trống")]
    public DateTime EndDate { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá trị hợp đồng không thể âm")]
    public decimal TotalAmount { get; set; } = 0m;

    public string? Terms { get; set; }

    public string? AttachmentUrl { get; set; }
}

/// <summary>
/// DTO cập nhật hợp đồng (chỉ áp dụng khi hợp đồng ở Draft hoặc Revision Requested - CTR-01)
/// </summary>
public class UpdateContractDto
{
    [StringLength(50, ErrorMessage = "Số hợp đồng không được vượt quá 50 ký tự")]
    public string? ContractNo { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá trị hợp đồng không thể âm")]
    public decimal? TotalAmount { get; set; }

    public string? Terms { get; set; }

    public string? AttachmentUrl { get; set; }

    /// <summary>
    /// Trường trạng thái để kiểm tra quy tắc CTR-03 (ngăn cản việc chuyển trạng thái bất hợp lệ sang Approved)
    /// </summary>
    public string? Status { get; set; }
}

/// <summary>
/// DTO kết quả sau khi gửi hợp đồng trình duyệt (US03)
/// </summary>
public class SubmitContractResultDto
{
    public int ContractId { get; set; }
    public string ContractNo { get; set; } = string.Empty;
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}
