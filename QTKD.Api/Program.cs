using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình kết nối cơ sở dữ liệu PostgreSQL với Entity Framework Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<QtkdDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Đăng ký các tầng dịch vụ nghiệp vụ (Business Logic Layer)
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IPriceListService, PriceListService>();

// 3. Đăng ký Controllers
builder.Services.AddControllers();

// 4. Cấu hình Swagger / OpenAPI để kiểm thử API trực quan trên trình duyệt
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 5. Cấu hình HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "QTKD API v1");
        c.RoutePrefix = "swagger"; // Truy cập tại /swagger
    });
}

// Điều hướng trang chủ (/) trực tiếp sang Swagger UI để tiện kiểm thử
app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseAuthorization();

app.MapControllers();

app.Run();
