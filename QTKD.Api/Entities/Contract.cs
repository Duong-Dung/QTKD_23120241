namespace QTKD.Api.Entities;

/// <summary>
/// Thực thể Hợp đồng (Contract)
/// </summary>
public class Contract
{
    public int Id { get; set; }
    public string ContractNo { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Draft";

    // Khóa ngoại liên kết tới Customer
    public Customer? Customer { get; set; }
}
