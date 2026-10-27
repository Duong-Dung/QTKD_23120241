using Microsoft.EntityFrameworkCore;
using QTKD.Api.Entities;

namespace QTKD.Api.Data;

/// <summary>
/// DbContext quản lý kết nối và truy cập dữ liệu PostgreSQL cho QTKD
/// </summary>
public class QtkdDbContext : DbContext
{
    public QtkdDbContext(DbContextOptions<QtkdDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<PriceItem> PriceItems => Set<PriceItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Cấu hình bảng Khách hàng (Customer)
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(255);
            entity.Property(c => c.Status).IsRequired().HasMaxLength(50);
        });

        // 2. Cấu hình bảng Hợp đồng (Contract) và quan hệ với Customer (1 - N)
        modelBuilder.Entity<Contract>(entity =>
        {
            entity.ToTable("Contracts");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.ContractNo).IsRequired().HasMaxLength(50);
            entity.Property(c => c.Status).IsRequired().HasMaxLength(50);

            entity.HasOne(c => c.Customer)
                  .WithMany(cust => cust.Contracts)
                  .HasForeignKey(c => c.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 3. Cấu hình bảng Bảng giá (PriceList)
        modelBuilder.Entity<PriceList>(entity =>
        {
            entity.ToTable("PriceLists");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(255);
        });

        // 4. Cấu hình bảng Chi tiết bảng giá (PriceItem) và quan hệ với PriceList (1 - N)
        modelBuilder.Entity<PriceItem>(entity =>
        {
            entity.ToTable("PriceItems");
            entity.HasKey(pi => pi.Id);
            entity.Property(pi => pi.ServiceCode).IsRequired().HasMaxLength(50);
            entity.Property(pi => pi.UnitPrice).HasPrecision(18, 2);

            entity.HasOne(pi => pi.PriceList)
                  .WithMany(p => p.PriceItems)
                  .HasForeignKey(pi => pi.PriceListId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 5. Nạp dữ liệu mẫu ban đầu (Seed Data)
        // - Ít nhất 2 Customer
        modelBuilder.Entity<Customer>().HasData(
            new Customer
            {
                Id = 1,
                Name = "Công ty TNHH Minh Phát",
                Status = "Active"
            },
            new Customer
            {
                Id = 2,
                Name = "Công ty Cổ phần Hoàng Gia",
                Status = "Active"
            }
        );

        // - 1 Contract ở trạng thái Draft liên kết với Khách hàng Minh Phát (theo kịch bản bài tập)
        modelBuilder.Entity<Contract>().HasData(
            new Contract
            {
                Id = 1,
                ContractNo = "HD-2026-018",
                CustomerId = 1,
                StartDate = DateTime.SpecifyKind(new DateTime(2026, 3, 1), DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(new DateTime(2027, 3, 1), DateTimeKind.Utc),
                Status = "Draft"
            }
        );

        // - 1 PriceList có 2 PriceItem
        modelBuilder.Entity<PriceList>().HasData(
            new PriceList
            {
                Id = 1,
                Name = "Bảng giá dịch vụ phần mềm 2026",
                EffectiveDate = DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<PriceItem>().HasData(
            new PriceItem
            {
                Id = 1,
                PriceListId = 1,
                ServiceCode = "SRV-CLOUD",
                UnitPrice = 1500000m
            },
            new PriceItem
            {
                Id = 2,
                PriceListId = 1,
                ServiceCode = "SRV-SUPPORT",
                UnitPrice = 500000m
            }
        );
    }
}
