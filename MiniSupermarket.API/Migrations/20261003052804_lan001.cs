using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MiniSupermarket.API.Migrations
{
    /// <inheritdoc />
    public partial class lan001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    BrandId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BrandName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.BrandId);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RewardPoints = table.Column<int>(type: "int", nullable: false),
                    MembershipRank = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    SupplierId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.SupplierId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Barcode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    BrandId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_Products_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "BrandId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    CashierId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_Orders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Users_CashierId",
                        column: x => x.CashierId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    RefreshTokenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Revoked = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.RefreshTokenId);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    OrderItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.OrderItemId);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Brands",
                columns: new[] { "BrandId", "BrandName", "Country", "Description", "IsActive" },
                values: new object[,]
                {
                    { 1, "Healthy Mart", "Việt Nam", "Thương hiệu riêng của cửa hàng", true },
                    { 2, "Quaker", "Mỹ", null, true },
                    { 3, "TH true MILK", "Việt Nam", null, true },
                    { 4, "Vinamilk", "Việt Nam", null, true },
                    { 5, "Alsafi", null, null, true },
                    { 6, "Bertolli", "Ý", null, true },
                    { 7, "Organic Farm VN", "Việt Nam", "Nông sản hữu cơ (dữ liệu mẫu)", true },
                    { 8, "Eat Clean VN", "Việt Nam", "Thực phẩm ăn kiêng (dữ liệu mẫu)", true },
                    { 9, "Dalat Green", "Việt Nam", "Trà thảo mộc (dữ liệu mẫu)", true },
                    { 10, "Blue Diamond", "Mỹ", null, true },
                    { 11, "Bee Gold", "Việt Nam", "Mật ong nguyên chất (dữ liệu mẫu)", true },
                    { 12, "Nutri Seed", "Việt Nam", "Superfood (dữ liệu mẫu)", true }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "CategoryName", "Description" },
                values: new object[,]
                {
                    { 1, "Rau củ quả hữu cơ", "Rau xanh, củ quả tươi đạt chuẩn organic" },
                    { 2, "Ngũ cốc & Hạt dinh dưỡng", "Yến mạch, hạt chia, hạnh nhân, óc chó" },
                    { 3, "Sữa hạt & Sữa chua hữu cơ", "Sữa hạnh nhân, sữa đậu nành, sữa chua Hy Lạp" },
                    { 4, "Thực phẩm Eat Clean", "Ức gà, cơm gạo lứt, salad đóng gói ăn kiêng" },
                    { 5, "Gia vị & Dầu thực vật tự nhiên", "Dầu oliu, mật ong nguyên chất, muối hồng Himalaya" },
                    { 6, "Trái cây tươi & nhập khẩu", "Táo, nho, kiwi, bơ, trái cây theo mùa" },
                    { 7, "Thịt cá sạch", "Cá hồi, thịt bò, thịt heo, hải sản tươi đạt chuẩn" },
                    { 8, "Trứng & Chế phẩm từ trứng", "Trứng gà ta, trứng gà hữu cơ, trứng cút" },
                    { 9, "Trà & Đồ uống tốt cho sức khỏe", "Trà thảo mộc, nước ép nguyên chất, kombucha" },
                    { 10, "Bánh & Snack lành mạnh", "Bánh yến mạch, snack rong biển, trái cây sấy" },
                    { 11, "Đậu & Sản phẩm từ đậu", "Đậu hũ, tempeh, các loại đậu khô" },
                    { 12, "Superfood & Thực phẩm bổ sung", "Spirulina, matcha, bột rau củ, collagen thực vật" }
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "CustomerId", "Address", "CustomerName", "MembershipRank", "PhoneNumber", "RewardPoints" },
                values: new object[,]
                {
                    { 1, null, "Nguyễn Văn A", "Vàng", "0901122334", 150 },
                    { 2, null, "Trần Thị B", "Bạc", "0918877665", 50 },
                    { 3, null, "Lê Văn C", "Chuẩn", "0983344556", 10 },
                    { 4, "12 Nguyễn Huệ, Quận 1, TP. Hồ Chí Minh", "Phạm Minh Tuấn", "Vàng", "0905123456", 180 },
                    { 5, "45 Lê Lợi, Quận 3, TP. Hồ Chí Minh", "Hoàng Thị Lan", "Bạc", "0912345678", 75 },
                    { 6, "8 Trần Hưng Đạo, Quận 5, TP. Hồ Chí Minh", "Võ Quốc Bảo", "Chuẩn", "0937654321", 20 },
                    { 7, "102 Điện Biên Phủ, Bình Thạnh, TP. Hồ Chí Minh", "Đặng Thu Hà", "Kim cương", "0944556677", 520 },
                    { 8, null, "Bùi Anh Khoa", "Chuẩn", "0966778899", 5 },
                    { 9, "27 Phan Văn Trị, Gò Vấp, TP. Hồ Chí Minh", "Ngô Thanh Mai", "Bạc", "0977889900", 90 },
                    { 10, "63 Cách Mạng Tháng 8, Quận 10, TP. Hồ Chí Minh", "Đỗ Hoài Nam", "Vàng", "0988990011", 210 },
                    { 11, null, "Lý Gia Hân", "Chuẩn", "0399123456", 0 },
                    { 12, "5 Võ Văn Ngân, Thủ Đức, TP. Hồ Chí Minh", "Trương Quốc Việt", "Bạc", "0868234567", 60 }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "RoleName" },
                values: new object[,]
                {
                    { 1, "ADMIN" },
                    { 2, "MANAGER" },
                    { 3, "CASHIER" }
                });

            migrationBuilder.InsertData(
                table: "Suppliers",
                columns: new[] { "SupplierId", "Address", "Email", "PhoneNumber", "SupplierName" },
                values: new object[,]
                {
                    { 1, "Đà Lạt, Lâm Đồng", "lienhe@nongsandalat.vn", "02633812345", "Công ty TNHH Nông sản sạch Đà Lạt" },
                    { 2, "Quận 7, TP. Hồ Chí Minh", "sales@freshviet.vn", "02838123456", "Công ty CP Trái cây nhập khẩu Fresh Việt" },
                    { 3, "Bình Thạnh, TP. Hồ Chí Minh", "info@eatcleansg.vn", "02838234567", "Công ty TNHH Thực phẩm Eat Clean Sài Gòn" },
                    { 4, "Tân Bình, TP. Hồ Chí Minh", "order@ngucocmyviet.vn", "02838345678", "Công ty TNHH Ngũ cốc & Hạt Mỹ Việt" },
                    { 5, "Cầu Giấy, Hà Nội", "cskh@suaxanh.vn", "02438456789", "Công ty CP Sữa & Chế phẩm sữa Xanh" },
                    { 6, "Quận 1, TP. Hồ Chí Minh", "contact@diatrunghai.vn", "02838567890", "Công ty TNHH Dầu ăn Địa Trung Hải" },
                    { 7, "U Minh, Cà Mau", "htx@matonguminh.vn", "02916678901", "HTX Mật ong U Minh" },
                    { 8, "Phú Nhuận, TP. Hồ Chí Minh", "sales@haisannauy.vn", "02838789012", "Công ty TNHH Hải sản Na Uy Việt Nam" },
                    { 9, "Đức Hòa, Long An", "trangtrai@trungsachla.vn", "02723890123", "Trang trại Trứng sạch Long An" },
                    { 10, "Bảo Lộc, Lâm Đồng", "info@dalatgreen.vn", "02633901234", "Công ty TNHH Trà thảo mộc Dalat Green" },
                    { 11, "Thủ Đức, TP. Hồ Chí Minh", "ban-hang@snacklanhmanh.vn", "02838012345", "Công ty CP Bánh & Snack lành mạnh" },
                    { 12, "Gò Vấp, TP. Hồ Chí Minh", "lienhe@superfoodxanh.vn", "02838123400", "Công ty TNHH Đậu hũ & Superfood Xanh" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductId", "Barcode", "BrandId", "CategoryId", "CostPrice", "IsActive", "Price", "ProductName", "StockQuantity", "SupplierId", "Unit" },
                values: new object[,]
                {
                    { 1, "8938501234561", 7, 1, 18000m, true, 25000m, "Rau cải bó xôi hữu cơ 300g", 50, 1, "gói" },
                    { 2, "8938501234562", 7, 1, 24000m, true, 32000m, "Cà chua bi hữu cơ 500g", 40, 1, "hộp" },
                    { 3, "8938501234563", 2, 2, 50000m, true, 65000m, "Yến mạch nguyên hạt Quaker 500g", 30, 4, "gói" },
                    { 4, "8938501234564", 10, 2, 72000m, true, 95000m, "Hạt hạnh nhân rang Mỹ 250g", 25, 4, "gói" },
                    { 5, "8938501234565", 5, 3, 60000m, true, 78000m, "Sữa hạnh nhân Alsafi 946ml", 35, 5, "hộp" },
                    { 6, "8938501234566", 3, 3, 41000m, true, 55000m, "Sữa chua Hy Lạp không đường 500g", 20, 5, "hộp" },
                    { 7, "8938501234567", 8, 4, 70000m, true, 89000m, "Ức gà tươi đông lạnh 1kg", 60, 3, "kg" },
                    { 8, "8938501234568", 8, 4, 31000m, true, 42000m, "Cơm gạo lứt đóng hộp ăn liền 250g", 45, 3, "hộp" },
                    { 9, "8938501234569", 6, 5, 110000m, true, 145000m, "Dầu oliu nguyên chất Extra Virgin 500ml", 20, 6, "chai" },
                    { 10, "8938501234570", 11, 5, 88000m, true, 120000m, "Mật ong nguyên chất U Minh 500ml", 15, 7, "chai" },
                    { 11, "8938501234571", 1, 6, 98000m, true, 129000m, "Táo Envy nhập khẩu 1kg", 30, 2, "kg" },
                    { 12, "8938501234572", 1, 7, 135000m, true, 169000m, "Cá hồi phi lê Na Uy 300g", 22, 8, "gói" },
                    { 13, "8938501234573", 7, 8, 40000m, true, 52000m, "Trứng gà ta hữu cơ hộp 10 quả", 70, 9, "hộp" },
                    { 14, "8938501234574", 9, 9, 34000m, true, 48000m, "Trà hoa cúc thảo mộc 20 gói", 55, 10, "hộp" },
                    { 15, "8938501234575", 8, 10, 28000m, true, 39000m, "Bánh quy yến mạch ít đường 200g", 65, 11, "gói" },
                    { 16, "8938501234576", 7, 11, 12000m, true, 18000m, "Đậu hũ non hữu cơ 300g", 80, 12, "hộp" },
                    { 17, "8938501234577", 12, 12, 100000m, true, 135000m, "Bột Spirulina tảo xoắn 100g", 18, 11, "hũ" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "Phone", "RoleId", "Username" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 5, 8, 0, 0, 0, DateTimeKind.Unspecified), "admin@healthymart.vn", "Quản trị hệ thống", true, "$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.", "0900000001", 1, "admin" },
                    { 2, new DateTime(2026, 1, 6, 8, 0, 0, 0, DateTimeKind.Unspecified), "manager01@healthymart.vn", "Nguyễn Thị Quản Lý", true, "$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.", "0900000002", 2, "manager01" },
                    { 3, new DateTime(2026, 1, 6, 8, 30, 0, 0, DateTimeKind.Unspecified), "manager02@healthymart.vn", "Trần Văn Điều Hành", true, "$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.", "0900000003", 2, "manager02" },
                    { 4, new DateTime(2026, 2, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "cashier01@healthymart.vn", "Lê Thị Thu Ngân 1", true, "$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.", "0900000004", 3, "cashier01" },
                    { 5, new DateTime(2026, 2, 1, 8, 10, 0, 0, DateTimeKind.Unspecified), "cashier02@healthymart.vn", "Phạm Văn Thu Ngân 2", true, "$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.", "0900000005", 3, "cashier02" },
                    { 6, new DateTime(2026, 2, 1, 8, 20, 0, 0, DateTimeKind.Unspecified), "cashier03@healthymart.vn", "Hoàng Thị Thu Ngân 3", true, "$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.", "0900000006", 3, "cashier03" },
                    { 7, new DateTime(2026, 2, 10, 8, 0, 0, 0, DateTimeKind.Unspecified), "cashier04@healthymart.vn", "Võ Minh Thu Ngân 4", true, "$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.", "0900000007", 3, "cashier04" },
                    { 8, new DateTime(2026, 2, 10, 8, 10, 0, 0, DateTimeKind.Unspecified), "cashier05@healthymart.vn", "Đặng Thị Thu Ngân 5", true, "$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.", "0900000008", 3, "cashier05" },
                    { 9, new DateTime(2026, 3, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "cashier06@healthymart.vn", "Bùi Văn Thu Ngân 6", true, "$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.", "0900000009", 3, "cashier06" },
                    { 10, new DateTime(2026, 3, 1, 8, 10, 0, 0, DateTimeKind.Unspecified), "cashier07@healthymart.vn", "Ngô Thị Thu Ngân 7", true, "$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.", "0900000010", 3, "cashier07" },
                    { 11, new DateTime(2026, 4, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "cashier08@healthymart.vn", "Đỗ Văn Thu Ngân 8", true, "$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.", "0900000011", 3, "cashier08" },
                    { 12, new DateTime(2026, 4, 1, 8, 10, 0, 0, DateTimeKind.Unspecified), "cashier09@healthymart.vn", "Lý Thị Thu Ngân 9", false, "$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.", "0900000012", 3, "cashier09" }
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "OrderId", "CashierId", "CreatedAt", "CustomerId", "Discount", "OrderCode", "PaymentMethod", "Status", "Subtotal", "Total" },
                values: new object[,]
                {
                    { 1, 4, new DateTime(2026, 9, 1, 9, 15, 0, 0, DateTimeKind.Unspecified), 1, 0m, "HD0001", "CASH", "PAID", 82000m, 82000m },
                    { 2, 4, new DateTime(2026, 9, 2, 10, 30, 0, 0, DateTimeKind.Unspecified), 2, 10000m, "HD0002", "MOMO", "PAID", 221000m, 211000m },
                    { 3, 5, new DateTime(2026, 9, 3, 11, 0, 0, 0, DateTimeKind.Unspecified), null, 0m, "HD0003", "CASH", "PAID", 173000m, 173000m },
                    { 4, 5, new DateTime(2026, 9, 4, 14, 20, 0, 0, DateTimeKind.Unspecified), 7, 20000m, "HD0004", "CARD", "PAID", 400000m, 380000m },
                    { 5, 6, new DateTime(2026, 9, 5, 16, 45, 0, 0, DateTimeKind.Unspecified), 4, 23000m, "HD0005", "BANK_TRANSFER", "PAID", 467000m, 444000m },
                    { 6, 6, new DateTime(2026, 9, 6, 8, 40, 0, 0, DateTimeKind.Unspecified), null, 0m, "HD0006", "CASH", "PAID", 158000m, 158000m },
                    { 7, 7, new DateTime(2026, 9, 7, 9, 5, 0, 0, DateTimeKind.Unspecified), 3, 0m, "HD0007", "CASH", "PAID", 126000m, 126000m },
                    { 8, 7, new DateTime(2026, 9, 8, 17, 10, 0, 0, DateTimeKind.Unspecified), 10, 12000m, "HD0008", "MOMO", "PAID", 237000m, 225000m },
                    { 9, 8, new DateTime(2026, 9, 9, 18, 30, 0, 0, DateTimeKind.Unspecified), 5, 0m, "HD0009", "CARD", "PAID", 151000m, 151000m },
                    { 10, 8, new DateTime(2026, 9, 10, 12, 0, 0, 0, DateTimeKind.Unspecified), null, 0m, "HD0010", "CASH", "CANCELLED", 129000m, 129000m },
                    { 11, 9, new DateTime(2026, 9, 11, 15, 25, 0, 0, DateTimeKind.Unspecified), 9, 12000m, "HD0011", "BANK_TRANSFER", "PAID", 262000m, 250000m },
                    { 12, 9, new DateTime(2026, 9, 12, 19, 0, 0, 0, DateTimeKind.Unspecified), 12, 0m, "HD0012", "MOMO", "PAID", 258000m, 258000m }
                });

            migrationBuilder.InsertData(
                table: "OrderItems",
                columns: new[] { "OrderItemId", "LineTotal", "OrderId", "ProductId", "Quantity", "UnitPrice" },
                values: new object[,]
                {
                    { 1, 50000m, 1, 1, 2, 25000m },
                    { 2, 32000m, 1, 2, 1, 32000m },
                    { 3, 65000m, 2, 3, 1, 65000m },
                    { 4, 156000m, 2, 5, 2, 78000m },
                    { 5, 89000m, 3, 7, 1, 89000m },
                    { 6, 84000m, 3, 8, 2, 42000m },
                    { 7, 145000m, 4, 9, 1, 145000m },
                    { 8, 120000m, 4, 10, 1, 120000m },
                    { 9, 135000m, 4, 17, 1, 135000m },
                    { 10, 338000m, 5, 12, 2, 169000m },
                    { 11, 129000m, 5, 11, 1, 129000m },
                    { 12, 104000m, 6, 13, 2, 52000m },
                    { 13, 54000m, 6, 16, 3, 18000m },
                    { 14, 48000m, 7, 14, 1, 48000m },
                    { 15, 78000m, 7, 15, 2, 39000m },
                    { 16, 95000m, 8, 4, 1, 95000m },
                    { 17, 110000m, 8, 6, 2, 55000m },
                    { 18, 32000m, 8, 2, 1, 32000m },
                    { 19, 126000m, 9, 8, 3, 42000m },
                    { 20, 25000m, 9, 1, 1, 25000m },
                    { 21, 129000m, 10, 11, 1, 129000m },
                    { 22, 130000m, 11, 3, 2, 65000m },
                    { 23, 96000m, 11, 14, 2, 48000m },
                    { 24, 36000m, 11, 16, 2, 18000m },
                    { 25, 89000m, 12, 7, 1, 89000m },
                    { 26, 169000m, 12, 12, 1, 169000m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Brands_BrandName",
                table: "Brands",
                column: "BrandName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CategoryName",
                table: "Categories",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PhoneNumber",
                table: "Customers",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CashierId",
                table: "Orders",
                column: "CashierId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderCode",
                table: "Orders",
                column: "OrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Barcode",
                table: "Products",
                column: "Barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_BrandId",
                table: "Products",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SupplierId",
                table: "Products",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
