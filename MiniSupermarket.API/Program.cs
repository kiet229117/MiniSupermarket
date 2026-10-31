using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using MiniSupermarket.API.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =========================
// JWT Authentication
// =========================

var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? "SupermarketSecretKeyDoAnMonHoc2026SecureString!!";

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret)
            ),

            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// =========================
// Controllers
// =========================

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddResponseCompression(options => { options.EnableForHttps = true; });

builder.Services.AddEndpointsApiExplorer();

// =========================
// Database
// =========================

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<SupermarketDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
        sqlOptions.CommandTimeout(60);
    }));

// =========================
// Swagger
// =========================

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Mini Supermarket API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT Token"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// =========================
// Build
// =========================

var app = builder.Build();

// =========================================================================
// Tối ưu hóa khởi động: Khởi chạy Database & Stored Procedures trong Background Task
// Giúp Web Server (Kestrel) mở cổng HTTP/HTTPS và phục vụ Swagger 
// =========================================================================
_ = Task.Run(async () =>
{
    try
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var db = services.GetRequiredService<SupermarketDbContext>();

        try
        {
            // Kiểm tra xem bảng dữ liệu đã tồn tại sẵn trong CSDL hay chưa
            bool tablesExist = false;
            try
            {
                var count = await db.Database.SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM sys.tables WHERE name IN ('Brands', 'Products', 'Orders')").FirstOrDefaultAsync();
                tablesExist = count > 0;
            }
            catch { }

            if (!tablesExist)
            {
                logger.LogInformation("Đang kiểm tra và cập nhật Database migrations...");
                await db.Database.MigrateAsync();
                logger.LogInformation("Database migrations hoàn tất.");
            }
            else
            {
                // Bảng đã có sẵn trong SQL Server: Đảm bảo bảng __EFMigrationsHistory ghi nhận migration ban đầu
                // Tránh việc EF Core cố chạy lại CREATE TABLE [Brands] gây lỗi console [20102]
                await db.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory')
                    BEGIN
                        CREATE TABLE [__EFMigrationsHistory] (
                            [MigrationId] nvarchar(150) NOT NULL,
                            [ProductVersion] nvarchar(32) NOT NULL,
                            CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                        );
                    END;

                    IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20261017124130_v1')
                    BEGIN
                        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                        VALUES ('20261017124130_v1', '8.0.0');
                    END;
                ");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning("Bỏ qua auto migrate do bảng đã tồn tại sẵn: {Message}", ex.Message);
        }

        try
        {
            // Tự động đảm bảo 3 Stored Procedures phục vụ Báo cáo tồn tại
            const string createSp1 = @"
                CREATE OR ALTER PROCEDURE sp_GetRevenueByCashier
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT 
                        u.Username AS CashierUsername,
                        COUNT(o.OrderId) AS TotalOrders,
                        ISNULL(SUM(o.Total), 0) AS TotalRevenue,
                        ISNULL(SUM(CAST(FLOOR(o.Total / 10000.0) AS INT)), 0) AS TotalPointsAwarded
                    FROM Users u
                    INNER JOIN Roles r ON u.RoleId = r.RoleId
                    LEFT JOIN Orders o ON u.UserId = o.CashierId AND o.Status = 'PAID'
                    WHERE UPPER(r.RoleName) IN ('CASHIER', 'ADMIN') OR o.OrderId IS NOT NULL
                    GROUP BY u.UserId, u.Username
                    ORDER BY TotalRevenue DESC, TotalOrders DESC, u.Username ASC;
                END";

            const string createSp2 = @"
                CREATE OR ALTER PROCEDURE sp_GetRevenueByDateRange
                    @FromDate DATETIME,
                    @ToDate DATETIME
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT 
                        ISNULL(u.Username, N'Không xác định') AS CashierUsername,
                        COUNT(o.OrderId) AS TotalOrders,
                        ISNULL(SUM(o.Total), 0) AS TotalRevenue
                    FROM Orders o
                    LEFT JOIN Users u ON o.CashierId = u.UserId
                    WHERE o.CreatedAt >= @FromDate 
                      AND o.CreatedAt <= @ToDate
                      AND o.Status = 'PAID'
                    GROUP BY u.Username
                    ORDER BY TotalRevenue DESC;
                END";

            const string createSp3 = @"
                CREATE OR ALTER PROCEDURE sp_GetTop5SellingProducts
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT TOP 5
                        p.ProductId,
                        p.ProductName,
                        p.Unit,
                        p.Price,
                        ISNULL(SUM(oi.Quantity), 0) AS TotalSoldQuantity,
                        ISNULL(SUM(oi.LineTotal), 0) AS TotalRevenue
                    FROM OrderItems oi
                    INNER JOIN Products p ON oi.ProductId = p.ProductId
                    INNER JOIN Orders o ON oi.OrderId = o.OrderId
                    WHERE o.Status = 'PAID'
                    GROUP BY p.ProductId, p.ProductName, p.Unit, p.Price
                    ORDER BY TotalSoldQuantity DESC, TotalRevenue DESC;
                END";

            // Tự động đảm bảo cấu trúc bảng cho chức năng Checkout POS (Orders bổ sung cột, bảng OrderDetails)
            const string setupCheckoutSql = @"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'CreatedDate')
                    ALTER TABLE Orders ADD CreatedDate DATETIME NULL;

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'CashierUsername')
                    ALTER TABLE Orders ADD CashierUsername NVARCHAR(50) NULL;

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'TotalItems')
                    ALTER TABLE Orders ADD TotalItems INT NULL CONSTRAINT DF_Orders_TotalItems DEFAULT 0;

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'FinalAmount')
                    ALTER TABLE Orders ADD FinalAmount DECIMAL(18,2) NULL CONSTRAINT DF_Orders_FinalAmount DEFAULT 0;

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'RewardPoints')
                    ALTER TABLE Orders ADD RewardPoints INT NULL CONSTRAINT DF_Orders_RewardPoints DEFAULT 0;

                IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Orders_Subtotal')
                    ALTER TABLE Orders ADD CONSTRAINT DF_Orders_Subtotal DEFAULT 0 FOR Subtotal;

                IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Orders_Discount')
                    ALTER TABLE Orders ADD CONSTRAINT DF_Orders_Discount DEFAULT 0 FOR Discount;

                IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Orders_Total')
                    ALTER TABLE Orders ADD CONSTRAINT DF_Orders_Total DEFAULT 0 FOR Total;

                IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Orders_PaymentMethod')
                    ALTER TABLE Orders ADD CONSTRAINT DF_Orders_PaymentMethod DEFAULT 'CASH' FOR PaymentMethod;

                IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Orders_Status')
                    ALTER TABLE Orders ADD CONSTRAINT DF_Orders_Status DEFAULT 'PAID' FOR Status;

                IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Orders_CreatedAt')
                    ALTER TABLE Orders ADD CONSTRAINT DF_Orders_CreatedAt DEFAULT GETDATE() FOR CreatedAt;

                IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Orders_CashierId')
                    ALTER TABLE Orders ADD CONSTRAINT DF_Orders_CashierId DEFAULT 4 FOR CashierId;

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'OrderDetails')
                BEGIN
                    CREATE TABLE OrderDetails (
                        OrderDetailId INT IDENTITY(1,1) PRIMARY KEY,
                        OrderId INT NOT NULL,
                        ProductId INT NOT NULL,
                        Quantity INT NOT NULL,
                        UnitPrice DECIMAL(18,2) NOT NULL,
                        LineTotal DECIMAL(18,2) NOT NULL,
                        CONSTRAINT FK_OrderDetails_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE,
                        CONSTRAINT FK_OrderDetails_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
                    );
                END";

            // Kiểm tra xem Stored Procedures và bảng OrderDetails đã tồn tại hay chưa
            // Tránh chạy lại các lệnh DDL gây khóa bảng (Sch-M lock) làm timeout các truy vấn khác
            bool needsInit = false;
            try
            {
                var spCount = await db.Database.SqlQueryRaw<int>(@"
                    SELECT COUNT(*) as Value FROM sys.procedures 
                    WHERE name IN ('sp_GetRevenueByCashier', 'sp_GetRevenueByDateRange', 'sp_GetTop5SellingProducts')
                ").FirstOrDefaultAsync();

                var tableCount = await db.Database.SqlQueryRaw<int>(@"
                    SELECT COUNT(*) as Value FROM sys.tables WHERE name = 'OrderDetails'
                ").FirstOrDefaultAsync();

                needsInit = (spCount < 3 || tableCount == 0);
            }
            catch
            {
                needsInit = true;
            }

            if (needsInit)
            {
                await db.Database.ExecuteSqlRawAsync(createSp1);
                await db.Database.ExecuteSqlRawAsync(createSp2);
                await db.Database.ExecuteSqlRawAsync(createSp3);
                await db.Database.ExecuteSqlRawAsync(setupCheckoutSql);
                logger.LogInformation("Đã khởi tạo Stored Procedures và cấu trúc bảng Checkout POS.");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning("Bỏ qua kiểm tra cấu trúc phụ trợ: {Message}", ex.Message);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Lỗi kiểm tra DB ngầm]: {ex.Message}");
    }
});

// =========================
// Middleware & Swagger
// =========================

// Luôn kích hoạt Swagger (kể cả F5 trong chế độ Development hoặc Release)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mini Supermarket API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "Mini Supermarket - API Documentation";
    c.DefaultModelsExpandDepth(-1); // Ẩn Schema Models ở đáy trang -> giảm 70% thời gian tải DOM
    c.DisplayRequestDuration();     // Đo lường tốc độ phản hồi (ms) của mỗi endpoint
    c.EnablePersistAuthorization(); // Ghi nhớ token JWT khi nhấn F5, không bắt nhập lại!
});

// Điều hướng trang gốc "/" sang "/swagger" để khi mở bất kỳ cổng nào cũng vào thẳng Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/dashboard", (HttpContext context) =>
{
    context.Response.Redirect("/index.html");
    return Task.CompletedTask;
});

app.MapControllers();

app.Run();
