using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniSupermarket.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderCustomers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "BrandId",
                keyValue: 1,
                column: "Description",
                value: "Thương hiệu riêng của cửa hàng: rau củ, trái cây, hải sản và thực phẩm sạch tuyển chọn");

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "BrandId",
                keyValue: 2,
                column: "Description",
                value: "Thương hiệu yến mạch và ngũ cốc ăn sáng, giàu chất xơ");

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "BrandId",
                keyValue: 3,
                column: "Description",
                value: "Sữa tươi sạch và các sản phẩm từ sữa như sữa chua, sữa hạt");

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "BrandId",
                keyValue: 4,
                column: "Description",
                value: "Thương hiệu sữa và sản phẩm từ sữa phổ biến tại Việt Nam");

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "BrandId",
                keyValue: 5,
                columns: new[] { "Country", "Description" },
                values: new object[] { "Campuchia", "Sữa hạt và đồ uống thực vật như sữa hạnh nhân" });

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "BrandId",
                keyValue: 6,
                column: "Description",
                value: "Dầu oliu và thực phẩm theo phong cách Địa Trung Hải");

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "BrandId",
                keyValue: 7,
                column: "Description",
                value: "Rau củ, trứng và đậu hũ hữu cơ từ trang trại (dữ liệu mẫu)");

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "BrandId",
                keyValue: 8,
                column: "Description",
                value: "Thực phẩm ăn kiêng: ức gà, cơm gạo lứt, bánh ít đường (dữ liệu mẫu)");

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "BrandId",
                keyValue: 9,
                column: "Description",
                value: "Trà thảo mộc và đồ uống tốt cho sức khoẻ từ Đà Lạt (dữ liệu mẫu)");

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "BrandId",
                keyValue: 10,
                column: "Description",
                value: "Hạnh nhân và các sản phẩm từ hạt hạnh nhân");

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "BrandId",
                keyValue: 11,
                column: "Description",
                value: "Mật ong nguyên chất, không pha đường (dữ liệu mẫu)");

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "BrandId",
                keyValue: 12,
                column: "Description",
                value: "Superfood và thực phẩm bổ sung: spirulina, bột rau củ, hạt dinh dưỡng (dữ liệu mẫu)");

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 1,
                column: "Address",
                value: "59 Nguyễn Thị Minh Khai, Quận 3, TP. Hồ Chí Minh");

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 2,
                column: "Address",
                value: "120 Võ Văn Tần, Quận 3, TP. Hồ Chí Minh");

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 3,
                column: "Address",
                value: "15 Lý Thường Kiệt, Quận 10, TP. Hồ Chí Minh");

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 4,
                column: "Address",
                value: "12 Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh");

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 5,
                column: "Address",
                value: "45 Lê Lợi, Phường Bến Thành, Quận 1, TP. Hồ Chí Minh");

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 7,
                column: "Address",
                value: "102 Điện Biên Phủ, Quận Bình Thạnh, TP. Hồ Chí Minh");

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 8,
                column: "Address",
                value: "36 Nguyễn Thị Thập, Quận 7, TP. Hồ Chí Minh");

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 9,
                column: "Address",
                value: "27 Phan Văn Trị, Quận Gò Vấp, TP. Hồ Chí Minh");

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 11,
                column: "Address",
                value: "210 Quang Trung, Quận Gò Vấp, TP. Hồ Chí Minh");

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 12,
                column: "Address",
                value: "5 Võ Văn Ngân, TP. Thủ Đức, TP. Hồ Chí Minh");

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 3,
                column: "CustomerId",
                value: 6);

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 6,
                column: "CustomerId",
                value: 8);

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 10,
                column: "CustomerId",
                value: 11);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "BrandId",
                keyValue: 1,
                column: "Description",
                value: "Thương hiệu riêng của cửa hàng");

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "BrandId",
                keyValue: 2,
                column: "Description",
                value: null);

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "BrandId",
                keyValue: 3,
                column: "Description",
                value: null);

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "BrandId",
                keyValue: 4,
                column: "Description",
                value: null);

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "BrandId",
                keyValue: 5,
                columns: new[] { "Country", "Description" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "BrandId",
                keyValue: 6,
                column: "Description",
                value: null);

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "BrandId",
                keyValue: 7,
                column: "Description",
                value: "Nông sản hữu cơ (dữ liệu mẫu)");

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "BrandId",
                keyValue: 8,
                column: "Description",
                value: "Thực phẩm ăn kiêng (dữ liệu mẫu)");

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "BrandId",
                keyValue: 9,
                column: "Description",
                value: "Trà thảo mộc (dữ liệu mẫu)");

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "BrandId",
                keyValue: 10,
                column: "Description",
                value: null);

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "BrandId",
                keyValue: 11,
                column: "Description",
                value: "Mật ong nguyên chất (dữ liệu mẫu)");

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "BrandId",
                keyValue: 12,
                column: "Description",
                value: "Superfood (dữ liệu mẫu)");

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 1,
                column: "Address",
                value: null);

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 2,
                column: "Address",
                value: null);

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 3,
                column: "Address",
                value: null);

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 4,
                column: "Address",
                value: "12 Nguyễn Huệ, Quận 1, TP. Hồ Chí Minh");

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 5,
                column: "Address",
                value: "45 Lê Lợi, Quận 3, TP. Hồ Chí Minh");

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 7,
                column: "Address",
                value: "102 Điện Biên Phủ, Bình Thạnh, TP. Hồ Chí Minh");

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 8,
                column: "Address",
                value: null);

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 9,
                column: "Address",
                value: "27 Phan Văn Trị, Gò Vấp, TP. Hồ Chí Minh");

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 11,
                column: "Address",
                value: null);

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 12,
                column: "Address",
                value: "5 Võ Văn Ngân, Thủ Đức, TP. Hồ Chí Minh");

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 3,
                column: "CustomerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 6,
                column: "CustomerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 10,
                column: "CustomerId",
                value: null);
        }
    }
}
