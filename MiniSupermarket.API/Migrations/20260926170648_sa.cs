using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MiniSupermarket.API.Migrations
{
    /// <inheritdoc />
    public partial class sa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Barcode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
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
                    { 5, "Gia vị & Dầu thực vật tự nhiên", "Dầu oliu, mật ong nguyên chất, muối hồng Himalaya" }
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "CustomerId", "Address", "CustomerName", "MembershipRank", "PhoneNumber", "RewardPoints" },
                values: new object[,]
                {
                    { 1, null, "Nguyễn Văn A", "Vàng", "0901122334", 150 },
                    { 2, null, "Trần Thị B", "Bạc", "0918877665", 50 },
                    { 3, null, "Lê Văn C", "Chuẩn", "0983344556", 10 }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductId", "Barcode", "CategoryId", "Price", "ProductName", "StockQuantity" },
                values: new object[,]
                {
                    { 1, "8938501234561", 1, 25000m, "Rau cải bó xôi hữu cơ 300g", 50 },
                    { 2, "8938501234562", 1, 32000m, "Cà chua bi hữu cơ 500g", 40 },
                    { 3, "8938501234563", 2, 65000m, "Yến mạch nguyên hạt Quaker 500g", 30 },
                    { 4, "8938501234564", 2, 95000m, "Hạt hạnh nhân rang Mỹ 250g", 25 },
                    { 5, "8938501234565", 3, 78000m, "Sữa hạnh nhân Alsafi 946ml", 35 },
                    { 6, "8938501234566", 3, 55000m, "Sữa chua Hy Lạp không đường 500g", 20 },
                    { 7, "8938501234567", 4, 89000m, "Ức gà tươi đông lạnh 1kg", 60 },
                    { 8, "8938501234568", 4, 42000m, "Cơm gạo lứt đóng hộp ăn liền 250g", 45 },
                    { 9, "8938501234569", 5, 145000m, "Dầu oliu nguyên chất Extra Virgin 500ml", 20 },
                    { 10, "8938501234570", 5, 120000m, "Mật ong nguyên chất U Minh 500ml", 15 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
