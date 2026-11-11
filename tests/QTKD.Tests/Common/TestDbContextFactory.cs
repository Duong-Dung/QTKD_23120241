using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;

namespace QTKD.Tests.Common;

/// <summary>
/// Lớp hỗ trợ tạo DbContext InMemory dùng riêng cho kiểm thử, đảm bảo cách ly dữ liệu
/// </summary>
public static class TestDbContextFactory
{
    public static QtkdDbContext CreateInMemoryDbContext(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<QtkdDbContext>()
            .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        var context = new QtkdDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}

