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

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

// =========================
// Database
// =========================

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<SupermarketDbContext>(options =>
    options.UseSqlServer(connectionString));

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

// =========================
// Auto Migrate Database
// =========================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = services.GetRequiredService<SupermarketDbContext>();
        try
        {
            logger.LogInformation("Đang kiểm tra và cập nhật Database migrations...");
            db.Database.Migrate();
            logger.LogInformation("Database migrations hoàn tất.");
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
                    WHERE UPPER(r.RoleName) = 'CASHIER'
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

            db.Database.ExecuteSqlRaw(createSp1);
            db.Database.ExecuteSqlRaw(createSp2);
            db.Database.ExecuteSqlRaw(createSp3);
            logger.LogInformation("Tất cả Stored Procedures báo cáo đã sẵn sàng!");

            // Tự động đảm bảo cấu trúc bảng cho chức năng Checkout POS (Orders bổ sung cột, bảng OrderDetails, triggers)
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
            db.Database.ExecuteSqlRaw(setupCheckoutSql);
            logger.LogInformation("Cấu trúc bảng Checkout POS đã sẵn sàng!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi khởi tạo Stored Procedures hoặc bảng Checkout.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Lỗi khi tự động migrate Database.");
    }
}

// =========================
// Middleware
// =========================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
