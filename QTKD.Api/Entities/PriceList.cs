namespace QTKD.Api.Entities;

/// <summary>
/// Thực thể Bảng giá (PriceList)
/// </summary>
public class PriceList
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }

    // Quan hệ 1 - N: Một bảng giá có nhiều mục giá dịch vụ
    public ICollection<PriceItem> PriceItems { get; set; } = new List<PriceItem>();
}
