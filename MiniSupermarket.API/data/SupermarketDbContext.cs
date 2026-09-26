using Microsoft.EntityFrameworkCore;

using MiniSupermarket.API.Models;

namespace MiniSupermarket.API.Data
{
    // DbContext đại diện cho phiên làm việc với cơ sở dữ liệu SQL Server
    public class SupermarketDbContext : DbContext
    {
        public SupermarketDbContext(DbContextOptions<SupermarketDbContext> options) : base(options) { }

        // Khai báo các bảng dữ liệu ánh xạ từ Model
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        // Cấu hình dữ liệu mồi ban đầu (Data Seeding)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

      


            // Nạp sẵn danh mục theo chủ đề Healthy Mart - Cửa hàng thực phẩm sạch & hữu cơ
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, CategoryName = "Rau củ quả hữu cơ", Description = "Rau xanh, củ quả tươi đạt chuẩn organic" },
                new Category { CategoryId = 2, CategoryName = "Ngũ cốc & Hạt dinh dưỡng", Description = "Yến mạch, hạt chia, hạnh nhân, óc chó" },
                new Category { CategoryId = 3, CategoryName = "Sữa hạt & Sữa chua hữu cơ", Description = "Sữa hạnh nhân, sữa đậu nành, sữa chua Hy Lạp" },
                new Category { CategoryId = 4, CategoryName = "Thực phẩm Eat Clean", Description = "Ức gà, cơm gạo lứt, salad đóng gói ăn kiêng" },
                new Category { CategoryId = 5, CategoryName = "Gia vị & Dầu thực vật tự nhiên", Description = "Dầu oliu, mật ong nguyên chất, muối hồng Himalaya" }
            );

            // Nạp sẵn sản phẩm mẫu
            modelBuilder.Entity<Product>().HasData(
                new Product { ProductId = 1, Barcode = "8938501234561", ProductName = "Rau cải bó xôi hữu cơ 300g", Price = 25000, StockQuantity = 50, CategoryId = 1 },
                new Product { ProductId = 2, Barcode = "8938501234562", ProductName = "Cà chua bi hữu cơ 500g", Price = 32000, StockQuantity = 40, CategoryId = 1 },
                new Product { ProductId = 3, Barcode = "8938501234563", ProductName = "Yến mạch nguyên hạt Quaker 500g", Price = 65000, StockQuantity = 30, CategoryId = 2 },
                new Product { ProductId = 4, Barcode = "8938501234564", ProductName = "Hạt hạnh nhân rang Mỹ 250g", Price = 95000, StockQuantity = 25, CategoryId = 2 },
                new Product { ProductId = 5, Barcode = "8938501234565", ProductName = "Sữa hạnh nhân Alsafi 946ml", Price = 78000, StockQuantity = 35, CategoryId = 3 },
                new Product { ProductId = 6, Barcode = "8938501234566", ProductName = "Sữa chua Hy Lạp không đường 500g", Price = 55000, StockQuantity = 20, CategoryId = 3 },
                new Product { ProductId = 7, Barcode = "8938501234567", ProductName = "Ức gà tươi đông lạnh 1kg", Price = 89000, StockQuantity = 60, CategoryId = 4 },
                new Product { ProductId = 8, Barcode = "8938501234568", ProductName = "Cơm gạo lứt đóng hộp ăn liền 250g", Price = 42000, StockQuantity = 45, CategoryId = 4 },
                new Product { ProductId = 9, Barcode = "8938501234569", ProductName = "Dầu oliu nguyên chất Extra Virgin 500ml", Price = 145000, StockQuantity = 20, CategoryId = 5 },
                new Product { ProductId = 10, Barcode = "8938501234570", ProductName = "Mật ong nguyên chất U Minh 500ml", Price = 120000, StockQuantity = 15, CategoryId = 5 }
            );

            // Nạp sẵn khách hàng mẫu
            modelBuilder.Entity<Customer>().HasData(
            new Customer { CustomerId = 1, CustomerName = "Nguyễn Văn A", PhoneNumber = "0901122334", Address = null, MembershipRank = "Vàng", RewardPoints = 150 },
            new Customer { CustomerId = 2, CustomerName = "Trần Thị B", PhoneNumber = "0918877665", Address = null, MembershipRank = "Bạc", RewardPoints = 50 },
            new Customer { CustomerId = 3, CustomerName = "Lê Văn C", PhoneNumber = "0983344556", Address = null, MembershipRank = "Chuẩn", RewardPoints = 10 }
);
        }
    }
}
