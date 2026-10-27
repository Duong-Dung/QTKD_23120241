namespace QTKD.Api.Entities;

/// <summary>
/// Thực thể Chi tiết giá dịch vụ (PriceItem)
/// </summary>
public class PriceItem
{
    public int Id { get; set; }
    public int PriceListId { get; set; }
    public string ServiceCode { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }

    // Khóa ngoại liên kết tới PriceList
    public PriceList? PriceList { get; set; }
}
