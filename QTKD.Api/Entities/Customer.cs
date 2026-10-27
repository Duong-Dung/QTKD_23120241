namespace QTKD.Api.Entities;

/// <summary>
/// Thực thể Khách hàng (Customer)
/// </summary>
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";

    // Quan hệ 1 - N: Một khách hàng có thể có nhiều hợp đồng
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
