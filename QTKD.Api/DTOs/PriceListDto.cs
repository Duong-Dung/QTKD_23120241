using System.ComponentModel.DataAnnotations;

namespace QTKD.Api.DTOs;

/// <summary>
/// DTO trả về bảng giá
/// </summary>
public class PriceListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public List<PriceItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO tạo mới bảng giá
/// </summary>
public class CreatePriceListDto
{
    [Required(ErrorMessage = "Tên bảng giá không được để trống")]
    [StringLength(255, ErrorMessage = "Tên bảng giá không được vượt quá 255 ký tự")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ngày hiệu lực không được để trống")]
    public DateTime EffectiveDate { get; set; }
}

/// <summary>
/// DTO trả về chi tiết giá dịch vụ
/// </summary>
public class PriceItemDto
{
    public int Id { get; set; }
    public int PriceListId { get; set; }
    public string ServiceCode { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
}

/// <summary>
/// DTO thêm chi tiết giá dịch vụ vào bảng giá
/// </summary>
public class CreatePriceItemDto
{
    [Required(ErrorMessage = "Mã dịch vụ không được để trống")]
    [StringLength(50, ErrorMessage = "Mã dịch vụ không được vượt quá 50 ký tự")]
    public string ServiceCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Đơn giá không được để trống")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn 0")]
    public decimal UnitPrice { get; set; }
}
