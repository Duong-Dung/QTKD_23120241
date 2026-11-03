using System.ComponentModel.DataAnnotations;

namespace QTKD.Api.DTOs;

/// <summary>
/// DTO trả về thông tin khách hàng
/// </summary>
public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
}

/// <summary>
/// DTO tạo khách hàng mới
/// </summary>
public class CreateCustomerDto
{
    [Required(ErrorMessage = "Tên khách hàng không được để trống")]
    [StringLength(255, MinimumLength = 2, ErrorMessage = "Tên khách hàng phải từ 2 đến 255 ký tự")]
    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = "Active";
}

/// <summary>
/// DTO cập nhật khách hàng
/// </summary>
public class UpdateCustomerDto
{
    [Required(ErrorMessage = "Tên khách hàng không được để trống")]
    [StringLength(255, MinimumLength = 2, ErrorMessage = "Tên khách hàng phải từ 2 đến 255 ký tự")]
    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = "Active";
}
