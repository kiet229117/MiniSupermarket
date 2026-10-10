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
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ================= RÀNG BUỘC & QUAN HỆ =================

            // Chống trùng
            modelBuilder.Entity<Category>().HasIndex(c => c.CategoryName).IsUnique();
            modelBuilder.Entity<Brand>().HasIndex(b => b.BrandName).IsUnique();
            modelBuilder.Entity<Product>().HasIndex(p => p.Barcode).IsUnique();
            modelBuilder.Entity<Customer>().HasIndex(c => c.PhoneNumber).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Order>().HasIndex(o => o.OrderCode).IsUnique();

            // Product
            modelBuilder.Entity<Product>().HasOne(p => p.Category).WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Product>().HasOne(p => p.Brand).WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Product>().HasOne(p => p.Supplier).WithMany()
                .HasForeignKey(p => p.SupplierId).OnDelete(DeleteBehavior.SetNull);

            // User - Role
            modelBuilder.Entity<User>().HasOne(u => u.Role).WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId).OnDelete(DeleteBehavior.Restrict);

            // Order
            modelBuilder.Entity<Order>().HasOne(o => o.Customer).WithMany()
                .HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Order>().HasOne(o => o.Cashier).WithMany()
                .HasForeignKey(o => o.CashierId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<OrderItem>().HasOne(i => i.Product).WithMany()
                .HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);

            // ================= DỮ LIỆU MẪU (SEED) =================
            // Lưu ý: HasData cần giá trị cố định (không dùng DateTime.UtcNow, không hash lúc chạy)

            // ---------- Roles (3 vai trò) ----------
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "ADMIN" },
                new Role { RoleId = 2, RoleName = "WAREHOUSE" },
                new Role { RoleId = 3, RoleName = "CASHIER" }
            );
            // ---------- Categories (15) ----------
            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    CategoryId = 1,
                    CategoryName = "Rau củ quả hữu cơ",
                    Description = "Các loại rau xanh, củ và quả tươi được trồng theo phương pháp hữu cơ."
                },
                new Category
                {
                    CategoryId = 2,
                    CategoryName = "Ngũ cốc & Hạt dinh dưỡng",
                    Description = "Yến mạch, hạt chia, hạnh nhân, óc chó và các loại hạt giàu dinh dưỡng."
                },
                new Category
                {
                    CategoryId = 3,
                    CategoryName = "Sữa hạt & Sữa chua hữu cơ",
                    Description = "Sữa hạnh nhân, sữa đậu nành, sữa óc chó và các loại sữa chua hữu cơ."
                },
                new Category
                {
                    CategoryId = 4,
                    CategoryName = "Thực phẩm Eat Clean",
                    Description = "Các sản phẩm phù hợp chế độ ăn Eat Clean như ức gà, cơm gạo lứt và salad."
                },
                new Category
                {
                    CategoryId = 5,
                    CategoryName = "Gia vị & Dầu thực vật tự nhiên",
                    Description = "Dầu oliu, mật ong, muối hồng và các loại gia vị có nguồn gốc tự nhiên."
                },
                new Category
                {
                    CategoryId = 6,
                    CategoryName = "Trái cây tươi & nhập khẩu",
                    Description = "Các loại trái cây tươi trong nước và nhập khẩu như táo, nho, kiwi và bơ."
                },
                new Category
                {
                    CategoryId = 7,
                    CategoryName = "Thịt cá sạch",
                    Description = "Thịt bò, thịt heo, cá hồi và các loại hải sản tươi sống được kiểm soát chất lượng."
                },
                new Category
                {
                    CategoryId = 8,
                    CategoryName = "Trứng & Chế phẩm từ trứng",
                    Description = "Trứng gà ta, trứng gà hữu cơ, trứng cút và các sản phẩm chế biến từ trứng."
                },
                new Category
                {
                    CategoryId = 9,
                    CategoryName = "Trà & Đồ uống tốt cho sức khỏe",
                    Description = "Trà thảo mộc, nước ép nguyên chất, kombucha và các loại đồ uống lành mạnh."
                },
                new Category
                {
                    CategoryId = 10,
                    CategoryName = "Bánh & Snack lành mạnh",
                    Description = "Bánh yến mạch, snack rong biển, trái cây sấy và các món ăn nhẹ lành mạnh."
                },
                new Category
                {
                    CategoryId = 11,
                    CategoryName = "Đậu & Sản phẩm từ đậu",
                    Description = "Đậu hũ, tempeh và các loại đậu khô giàu protein thực vật."
                },
                new Category
                {
                    CategoryId = 12,
                    CategoryName = "Superfood & Thực phẩm bổ sung",
                    Description = "Matcha, spirulina, bột rau củ và các sản phẩm thực phẩm bổ sung dinh dưỡng."
                },
                new Category
                {
                    CategoryId = 13,
                    CategoryName = "Thực phẩm đông lạnh",
                    Description = "Rau củ, thịt cá và các loại thực phẩm được bảo quản đông lạnh tiện lợi."
                },
                new Category
                {
                    CategoryId = 14,
                    CategoryName = "Thực phẩm khô",
                    Description = "Các loại mì, bún, phở, nấm khô và thực phẩm khô tiện lợi cho gia đình."
                },
                new Category
                {
                    CategoryId = 15,
                    CategoryName = "Thực phẩm chay",
                    Description = "Các sản phẩm chay từ thực vật như thịt chay, đậu phụ và thực phẩm giàu protein thực vật."
                }
            );

            // ---------- Brands (15) ----------
            modelBuilder.Entity<Brand>().HasData(
                new Brand
                {
                    BrandId = 1,
                    BrandName = "Healthy Mart",
                    Country = "Việt Nam",
                    Description = "Thương hiệu riêng của cửa hàng với các sản phẩm rau củ, trái cây và thực phẩm sạch.",
                    IsActive = true
                },
                new Brand
                {
                    BrandId = 2,
                    BrandName = "Quaker",
                    Country = "Mỹ",
                    Description = "Thương hiệu yến mạch và ngũ cốc ăn sáng giàu chất xơ.",
                    IsActive = true
                },
                new Brand
                {
                    BrandId = 3,
                    BrandName = "TH true MILK",
                    Country = "Việt Nam",
                    Description = "Thương hiệu sữa tươi và các sản phẩm từ sữa.",
                    IsActive = true
                },
                new Brand
                {
                    BrandId = 4,
                    BrandName = "Vinamilk",
                    Country = "Việt Nam",
                    Description = "Thương hiệu sữa và các sản phẩm dinh dưỡng phổ biến tại Việt Nam.",
                    IsActive = true
                },
                new Brand
                {
                    BrandId = 5,
                    BrandName = "Alsafi",
                    Country = "Campuchia",
                    Description = "Thương hiệu sữa hạt và đồ uống có nguồn gốc thực vật.",
                    IsActive = true
                },
                new Brand
                {
                    BrandId = 6,
                    BrandName = "Bertolli",
                    Country = "Ý",
                    Description = "Thương hiệu dầu oliu và các sản phẩm theo phong cách ẩm thực Địa Trung Hải.",
                    IsActive = true
                },
                new Brand
                {
                    BrandId = 7,
                    BrandName = "Organic Farm VN",
                    Country = "Việt Nam",
                    Description = "Rau củ, trứng và các sản phẩm nông nghiệp hữu cơ.",
                    IsActive = true
                },
                new Brand
                {
                    BrandId = 8,
                    BrandName = "Eat Clean VN",
                    Country = "Việt Nam",
                    Description = "Các sản phẩm phục vụ chế độ ăn Eat Clean và ăn uống lành mạnh.",
                    IsActive = true
                },
                new Brand
                {
                    BrandId = 9,
                    BrandName = "Dalat Green",
                    Country = "Việt Nam",
                    Description = "Trà thảo mộc và các loại đồ uống có nguồn gốc tự nhiên.",
                    IsActive = true
                },
                new Brand
                {
                    BrandId = 10,
                    BrandName = "Blue Diamond",
                    Country = "Mỹ",
                    Description = "Thương hiệu hạnh nhân và các sản phẩm chế biến từ hạnh nhân.",
                    IsActive = true
                },
                new Brand
                {
                    BrandId = 11,
                    BrandName = "Bee Gold",
                    Country = "Việt Nam",
                    Description = "Mật ong nguyên chất và các sản phẩm từ mật ong.",
                    IsActive = true
                },
                new Brand
                {
                    BrandId = 12,
                    BrandName = "Nutri Seed",
                    Country = "Việt Nam",
                    Description = "Superfood, hạt dinh dưỡng và các sản phẩm bổ sung dinh dưỡng.",
                    IsActive = true
                },
                new Brand
                {
                    BrandId = 13,
                    BrandName = "Green Life",
                    Country = "Việt Nam",
                    Description = "Thực phẩm chay và các sản phẩm có nguồn gốc từ thực vật.",
                    IsActive = true
                },
                new Brand
                {
                    BrandId = 14,
                    BrandName = "Nature Food",
                    Country = "Việt Nam",
                    Description = "Thực phẩm khô, ngũ cốc và các sản phẩm tự nhiên tiện lợi.",
                    IsActive = true
                },
                new Brand
                {
                    BrandId = 15,
                    BrandName = "Fresh Choice",
                    Country = "Việt Nam",
                    Description = "Thực phẩm đông lạnh, thịt cá và các sản phẩm tiện lợi chất lượng cao.",
                    IsActive = true
                }
            );

            modelBuilder.Entity<Supplier>().HasData(
       new Supplier
       {
           SupplierId = 1,
           SupplierName = "Công ty TNHH Nông sản sạch Đà Lạt",
           PhoneNumber = "02633812345",
           Email = "lienhe@nongsandalat.vn",
           Address = "15 Nguyễn Văn Cừ, Phường 4, TP. Đà Lạt, Lâm Đồng"
       },
       new Supplier
       {
           SupplierId = 2,
           SupplierName = "Công ty CP Trái cây nhập khẩu Fresh Việt",
           PhoneNumber = "02838123456",
           Email = "sales@freshviet.vn",
           Address = "125 Nguyễn Hữu Thọ, Phường Tân Phong, Quận 7, TP. Hồ Chí Minh"
       },
       new Supplier
       {
           SupplierId = 3,
           SupplierName = "Công ty TNHH Thực phẩm Eat Clean Sài Gòn",
           PhoneNumber = "02838234567",
           Email = "info@eatcleansg.vn",
           Address = "86 Điện Biên Phủ, Phường 17, Quận Bình Thạnh, TP. Hồ Chí Minh"
       },
       new Supplier
       {
           SupplierId = 4,
           SupplierName = "Công ty TNHH Ngũ cốc & Hạt Mỹ Việt",
           PhoneNumber = "02838345678",
           Email = "order@ngucocmyviet.vn",
           Address = "42 Trường Chinh, Phường 12, Quận Tân Bình, TP. Hồ Chí Minh"
       },
       new Supplier
       {
           SupplierId = 5,
           SupplierName = "Công ty CP Sữa & Chế phẩm sữa Xanh",
           PhoneNumber = "02438456789",
           Email = "cskh@suaxanh.vn",
           Address = "28 Trần Thái Tông, Phường Dịch Vọng, Quận Cầu Giấy, Hà Nội"
       },
       new Supplier
       {
           SupplierId = 6,
           SupplierName = "Công ty TNHH Dầu ăn Địa Trung Hải",
           PhoneNumber = "02838567890",
           Email = "contact@diatrunghai.vn",
           Address = "72 Nguyễn Thái Bình, Phường Nguyễn Thái Bình, Quận 1, TP. Hồ Chí Minh"
       },
       new Supplier
       {
           SupplierId = 7,
           SupplierName = "HTX Mật ong U Minh",
           PhoneNumber = "02916678901",
           Email = "htx@matonguminh.vn",
           Address = "18 Nguyễn Tất Thành, Thị trấn U Minh, Huyện U Minh, Cà Mau"
       },
       new Supplier
       {
           SupplierId = 8,
           SupplierName = "Công ty TNHH Hải sản Na Uy Việt Nam",
           PhoneNumber = "02838789012",
           Email = "sales@haisannauy.vn",
           Address = "35 Phan Đăng Lưu, Phường 3, Quận Phú Nhuận, TP. Hồ Chí Minh"
       },
       new Supplier
       {
           SupplierId = 9,
           SupplierName = "Trang trại Trứng sạch Long An",
           PhoneNumber = "02723890123",
           Email = "trangtrai@trungsachla.vn",
           Address = "125 Đường Tỉnh 825, Xã Đức Hòa Hạ, Huyện Đức Hòa, Long An"
       },
       new Supplier
       {
           SupplierId = 10,
           SupplierName = "Công ty TNHH Trà thảo mộc Dalat Green",
           PhoneNumber = "02633901234",
           Email = "info@dalatgreen.vn",
           Address = "56 Trần Phú, Phường Lộc Tiến, TP. Bảo Lộc, Lâm Đồng"
       },
       new Supplier
       {
           SupplierId = 11,
           SupplierName = "Công ty CP Bánh & Snack lành mạnh",
           PhoneNumber = "02838012345",
           Email = "ban-hang@snacklanhmanh.vn",
           Address = "210 Võ Văn Ngân, Phường Bình Thọ, TP. Thủ Đức, TP. Hồ Chí Minh"
       },
       new Supplier
       {
           SupplierId = 12,
           SupplierName = "Công ty TNHH Đậu hũ & Superfood Xanh",
           PhoneNumber = "02838123400",
           Email = "lienhe@superfoodxanh.vn",
           Address = "89 Quang Trung, Phường 10, Quận Gò Vấp, TP. Hồ Chí Minh"
       },
       new Supplier
       {
           SupplierId = 13,
           SupplierName = "Công ty TNHH Rau củ hữu cơ Mekong",
           PhoneNumber = "02923678901",
           Email = "contact@rauhuucomekong.vn",
           Address = "145 Nguyễn Văn Cừ, Phường An Khánh, Quận Ninh Kiều, Cần Thơ"
       },
       new Supplier
       {
           SupplierId = 14,
           SupplierName = "Công ty CP Thực phẩm xanh Việt Nature",
           PhoneNumber = "02839876543",
           Email = "sales@vietnature.vn",
           Address = "320 Nguyễn Văn Quá, Phường Đông Hưng Thuận, Quận 12, TP. Hồ Chí Minh"
       },
       new Supplier
       {
           SupplierId = 15,
           SupplierName = "Công ty TNHH Nông sản hữu cơ An Việt",
           PhoneNumber = "02743781234",
           Email = "info@nongnsanviet.vn",
           Address = "78 Đại lộ Bình Dương, Phường Phú Hòa, TP. Thủ Dầu Một, Bình Dương"
       }
   );

            // ---------- Products (17) ----------
            modelBuilder.Entity<Product>().HasData(
                // Category 1 - Rau củ quả hữu cơ
                new Product
                {
                    ProductId = 1,
                    Barcode = "8938501234561",
                    ProductName = "Rau cải bó xôi hữu cơ 300g",
                    Unit = "gói",
                    Price = 25000,
                    CostPrice = 18000,
                    StockQuantity = 50,
                    IsActive = true,
                    CategoryId = 1,
                    BrandId = 7,
                    SupplierId = 1
                },
                new Product
                {
                    ProductId = 2,
                    Barcode = "8938501234562",
                    ProductName = "Cà chua bi hữu cơ 500g",
                    Unit = "hộp",
                    Price = 32000,
                    CostPrice = 24000,
                    StockQuantity = 40,
                    IsActive = true,
                    CategoryId = 1,
                    BrandId = 7,
                    SupplierId = 1
                },

                // Category 2 - Ngũ cốc & Hạt dinh dưỡng
                new Product
                {
                    ProductId = 3,
                    Barcode = "8938501234563",
                    ProductName = "Yến mạch nguyên hạt Quaker 500g",
                    Unit = "gói",
                    Price = 65000,
                    CostPrice = 50000,
                    StockQuantity = 30,
                    IsActive = true,
                    CategoryId = 2,
                    BrandId = 2,
                    SupplierId = 4
                },
                new Product
                {
                    ProductId = 4,
                    Barcode = "8938501234564",
                    ProductName = "Hạnh nhân rang Blue Diamond 250g",
                    Unit = "gói",
                    Price = 95000,
                    CostPrice = 72000,
                    StockQuantity = 25,
                    IsActive = true,
                    CategoryId = 2,
                    BrandId = 10,
                    SupplierId = 4
                },

                // Category 3 - Sữa hạt & Sữa chua hữu cơ
                new Product
                {
                    ProductId = 5,
                    Barcode = "8938501234565",
                    ProductName = "Sữa hạnh nhân Alsafi 946ml",
                    Unit = "hộp",
                    Price = 78000,
                    CostPrice = 60000,
                    StockQuantity = 35,
                    IsActive = true,
                    CategoryId = 3,
                    BrandId = 5,
                    SupplierId = 5
                },
                new Product
                {
                    ProductId = 6,
                    Barcode = "8938501234566",
                    ProductName = "Sữa chua TH true MILK không đường 500g",
                    Unit = "hộp",
                    Price = 55000,
                    CostPrice = 41000,
                    StockQuantity = 20,
                    IsActive = true,
                    CategoryId = 3,
                    BrandId = 3,
                    SupplierId = 5
                },

                // Category 4 - Thực phẩm Eat Clean
                new Product
                {
                    ProductId = 7,
                    Barcode = "8938501234567",
                    ProductName = "Ức gà Eat Clean 500g",
                    Unit = "hộp",
                    Price = 69000,
                    CostPrice = 52000,
                    StockQuantity = 60,
                    IsActive = true,
                    CategoryId = 4,
                    BrandId = 8,
                    SupplierId = 3
                },
                new Product
                {
                    ProductId = 8,
                    Barcode = "8938501234568",
                    ProductName = "Cơm gạo lứt Eat Clean 250g",
                    Unit = "hộp",
                    Price = 42000,
                    CostPrice = 31000,
                    StockQuantity = 45,
                    IsActive = true,
                    CategoryId = 4,
                    BrandId = 8,
                    SupplierId = 3
                },

                // Category 5 - Gia vị & Dầu thực vật tự nhiên
                new Product
                {
                    ProductId = 9,
                    Barcode = "8938501234569",
                    ProductName = "Dầu oliu Bertolli Extra Virgin 500ml",
                    Unit = "chai",
                    Price = 145000,
                    CostPrice = 110000,
                    StockQuantity = 20,
                    IsActive = true,
                    CategoryId = 5,
                    BrandId = 6,
                    SupplierId = 6
                },
                new Product
                {
                    ProductId = 10,
                    Barcode = "8938501234570",
                    ProductName = "Mật ong nguyên chất Bee Gold 500ml",
                    Unit = "chai",
                    Price = 120000,
                    CostPrice = 88000,
                    StockQuantity = 15,
                    IsActive = true,
                    CategoryId = 5,
                    BrandId = 11,
                    SupplierId = 7
                },

                // Category 6 - Trái cây tươi & nhập khẩu
                new Product
                {
                    ProductId = 11,
                    Barcode = "8938501234571",
                    ProductName = "Táo Envy nhập khẩu 1kg",
                    Unit = "kg",
                    Price = 129000,
                    CostPrice = 98000,
                    StockQuantity = 30,
                    IsActive = true,
                    CategoryId = 6,
                    BrandId = 1,
                    SupplierId = 2
                },

                // Category 7 - Thịt cá sạch
                new Product
                {
                    ProductId = 12,
                    Barcode = "8938501234572",
                    ProductName = "Cá hồi phi lê Na Uy 300g",
                    Unit = "gói",
                    Price = 169000,
                    CostPrice = 135000,
                    StockQuantity = 22,
                    IsActive = true,
                    CategoryId = 7,
                    BrandId = 1,
                    SupplierId = 8
                },

                // Category 8 - Trứng & Chế phẩm từ trứng
                new Product
                {
                    ProductId = 13,
                    Barcode = "8938501234573",
                    ProductName = "Trứng gà ta hữu cơ 10 quả",
                    Unit = "hộp",
                    Price = 52000,
                    CostPrice = 40000,
                    StockQuantity = 70,
                    IsActive = true,
                    CategoryId = 8,
                    BrandId = 7,
                    SupplierId = 9
                },

                // Category 9 - Trà & Đồ uống tốt cho sức khỏe
                new Product
                {
                    ProductId = 14,
                    Barcode = "8938501234574",
                    ProductName = "Trà hoa cúc Dalat Green 20 túi lọc",
                    Unit = "hộp",
                    Price = 48000,
                    CostPrice = 34000,
                    StockQuantity = 55,
                    IsActive = true,
                    CategoryId = 9,
                    BrandId = 9,
                    SupplierId = 10
                },

                // Category 10 - Bánh & Snack lành mạnh
                new Product
                {
                    ProductId = 15,
                    Barcode = "8938501234575",
                    ProductName = "Bánh quy yến mạch Eat Clean 200g",
                    Unit = "gói",
                    Price = 39000,
                    CostPrice = 28000,
                    StockQuantity = 65,
                    IsActive = true,
                    CategoryId = 10,
                    BrandId = 8,
                    SupplierId = 11
                },

                // Category 11 - Đậu & Sản phẩm từ đậu
                new Product
                {
                    ProductId = 16,
                    Barcode = "8938501234576",
                    ProductName = "Đậu hũ non hữu cơ Organic Farm VN 300g",
                    Unit = "hộp",
                    Price = 18000,
                    CostPrice = 12000,
                    StockQuantity = 80,
                    IsActive = true,
                    CategoryId = 11,
                    BrandId = 7,
                    SupplierId = 12
                },

                // Category 12 - Superfood & Thực phẩm bổ sung
                new Product
                {
                    ProductId = 17,
                    Barcode = "8938501234577",
                    ProductName = "Bột Spirulina Nutri Seed 100g",
                    Unit = "hũ",
                    Price = 135000,
                    CostPrice = 100000,
                    StockQuantity = 18,
                    IsActive = true,
                    CategoryId = 12,
                    BrandId = 12,
                    SupplierId = 12
                }
            );

            // ---------- Customers (15) ----------
            modelBuilder.Entity<Customer>().HasData(
                new Customer
                {
                    CustomerId = 1,
                    CustomerName = "Nguyễn Văn A",
                    PhoneNumber = "0901122334",
                    Address = "59 Nguyễn Thị Minh Khai, Phường Võ Thị Sáu, Quận 3, TP. Hồ Chí Minh",
                    MembershipRank = "Vàng",
                    RewardPoints = 150
                },
                new Customer
                {
                    CustomerId = 2,
                    CustomerName = "Trần Thị B",
                    PhoneNumber = "0918877665",
                    Address = "120 Võ Văn Tần, Phường Võ Thị Sáu, Quận 3, TP. Hồ Chí Minh",
                    MembershipRank = "Bạc",
                    RewardPoints = 50
                },
                new Customer
                {
                    CustomerId = 3,
                    CustomerName = "Lê Văn C",
                    PhoneNumber = "0983344556",
                    Address = "15 Lý Thường Kiệt, Phường 14, Quận 10, TP. Hồ Chí Minh",
                    MembershipRank = "Chuẩn",
                    RewardPoints = 10
                },
                new Customer
                {
                    CustomerId = 4,
                    CustomerName = "Phạm Minh Tuấn",
                    PhoneNumber = "0905123456",
                    Address = "12 Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh",
                    MembershipRank = "Vàng",
                    RewardPoints = 180
                },
                new Customer
                {
                    CustomerId = 5,
                    CustomerName = "Hoàng Thị Lan",
                    PhoneNumber = "0912345678",
                    Address = "45 Lê Lợi, Phường Bến Thành, Quận 1, TP. Hồ Chí Minh",
                    MembershipRank = "Bạc",
                    RewardPoints = 75
                },
                new Customer
                {
                    CustomerId = 6,
                    CustomerName = "Võ Quốc Bảo",
                    PhoneNumber = "0937654321",
                    Address = "8 Trần Hưng Đạo, Phường 6, Quận 5, TP. Hồ Chí Minh",
                    MembershipRank = "Chuẩn",
                    RewardPoints = 20
                },
                new Customer
                {
                    CustomerId = 7,
                    CustomerName = "Đặng Thu Hà",
                    PhoneNumber = "0944556677",
                    Address = "102 Điện Biên Phủ, Phường 17, Quận Bình Thạnh, TP. Hồ Chí Minh",
                    MembershipRank = "Kim cương",
                    RewardPoints = 520
                },
                new Customer
                {
                    CustomerId = 8,
                    CustomerName = "Bùi Anh Khoa",
                    PhoneNumber = "0966778899",
                    Address = "36 Nguyễn Thị Thập, Phường Tân Quy, Quận 7, TP. Hồ Chí Minh",
                    MembershipRank = "Chuẩn",
                    RewardPoints = 5
                },
                new Customer
                {
                    CustomerId = 9,
                    CustomerName = "Ngô Thanh Mai",
                    PhoneNumber = "0977889900",
                    Address = "27 Phan Văn Trị, Phường 10, Quận Gò Vấp, TP. Hồ Chí Minh",
                    MembershipRank = "Bạc",
                    RewardPoints = 90
                },
                new Customer
                {
                    CustomerId = 10,
                    CustomerName = "Đỗ Hoài Nam",
                    PhoneNumber = "0988990011",
                    Address = "63 Cách Mạng Tháng 8, Phường 12, Quận 10, TP. Hồ Chí Minh",
                    MembershipRank = "Vàng",
                    RewardPoints = 210
                },
                new Customer
                {
                    CustomerId = 11,
                    CustomerName = "Lý Gia Hân",
                    PhoneNumber = "0399123456",
                    Address = "210 Quang Trung, Phường 10, Quận Gò Vấp, TP. Hồ Chí Minh",
                    MembershipRank = "Chuẩn",
                    RewardPoints = 0
                },
                new Customer
                {
                    CustomerId = 12,
                    CustomerName = "Trương Quốc Việt",
                    PhoneNumber = "0868234567",
                    Address = "5 Võ Văn Ngân, Phường Linh Chiểu, TP. Thủ Đức, TP. Hồ Chí Minh",
                    MembershipRank = "Bạc",
                    RewardPoints = 60
                },
                new Customer
                {
                    CustomerId = 13,
                    CustomerName = "Nguyễn Minh Khôi",
                    PhoneNumber = "0908765432",
                    Address = "88 Nguyễn Oanh, Phường 17, Quận Gò Vấp, TP. Hồ Chí Minh",
                    MembershipRank = "Vàng",
                    RewardPoints = 230
                },
                new Customer
                {
                    CustomerId = 14,
                    CustomerName = "Phan Ngọc Anh",
                    PhoneNumber = "0938123456",
                    Address = "156 Hoàng Diệu, Phường 6, Quận 4, TP. Hồ Chí Minh",
                    MembershipRank = "Bạc",
                    RewardPoints = 85
                },
                new Customer
                {
                    CustomerId = 15,
                    CustomerName = "Đinh Thanh Tùng",
                    PhoneNumber = "0976234518",
                    Address = "72 Kha Vạn Cân, Phường Hiệp Bình Chánh, TP. Thủ Đức, TP. Hồ Chí Minh",
                    MembershipRank = "Chuẩn",
                    RewardPoints = 15
                }
            );
            // ---------- Users (12) ----------
            // Mật khẩu của TẤT CẢ tài khoản mẫu: Healthy@123  (BCrypt đã hash sẵn, cố định)
            const string seedHash = "$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.";
            modelBuilder.Entity<User>().HasData(
                new User { UserId = 1, Username = "admin", PasswordHash = seedHash, FullName = "Quản trị hệ thống", Email = "admin@healthymart.vn", Phone = "0900000001", IsActive = true, RoleId = 1, CreatedAt = new DateTime(2026, 1, 5, 8, 0, 0) },
                new User { UserId = 2, Username = "warehouse01", PasswordHash = seedHash, FullName = "Nguyễn Thị Mai", Email = "warehouse01@healthymart.vn", Phone = "0900000002", IsActive = true, RoleId = 2, CreatedAt = new DateTime(2026, 1, 6, 8, 0, 0) },
                new User { UserId = 3, Username = "warehouse02", PasswordHash = seedHash, FullName = "Trần Văn Điều", Email = "warehouse02@healthymart.vn", Phone = "0900000003", IsActive = true, RoleId = 2, CreatedAt = new DateTime(2026, 1, 6, 8, 30, 0) },
                new User { UserId = 4, Username = "cashier01", PasswordHash = seedHash, FullName = "Lê Thị Thu Ngân", Email = "cashier01@healthymart.vn", Phone = "0900000004", IsActive = true, RoleId = 3, CreatedAt = new DateTime(2026, 2, 1, 8, 0, 0) },
                new User { UserId = 5, Username = "cashier02", PasswordHash = seedHash, FullName = "Phạm Văn Ngân", Email = "cashier02@healthymart.vn", Phone = "0900000005", IsActive = true, RoleId = 3, CreatedAt = new DateTime(2026, 2, 1, 8, 10, 0) },
                new User { UserId = 6, Username = "cashier03", PasswordHash = seedHash, FullName = "Hoàng Thị Ngân", Email = "cashier03@healthymart.vn", Phone = "0900000006", IsActive = true, RoleId = 3, CreatedAt = new DateTime(2026, 2, 1, 8, 20, 0) },
                new User { UserId = 7, Username = "cashier04", PasswordHash = seedHash, FullName = "Võ Minh Ngân", Email = "cashier04@healthymart.vn", Phone = "0900000007", IsActive = true, RoleId = 3, CreatedAt = new DateTime(2026, 2, 10, 8, 0, 0) },
                new User { UserId = 8, Username = "cashier05", PasswordHash = seedHash, FullName = "Đặng ThịNgân", Email = "cashier05@healthymart.vn", Phone = "0900000008", IsActive = true, RoleId = 3, CreatedAt = new DateTime(2026, 2, 10, 8, 10, 0) },
                new User { UserId = 9, Username = "cashier06", PasswordHash = seedHash, FullName = "Bùi Văn Ngân", Email = "cashier06@healthymart.vn", Phone = "0900000009", IsActive = true, RoleId = 3, CreatedAt = new DateTime(2026, 3, 1, 8, 0, 0) },
                new User { UserId = 10, Username = "cashier07", PasswordHash = seedHash, FullName = "Ngô Thị Thu Ngân", Email = "cashier07@healthymart.vn", Phone = "0900000010", IsActive = true, RoleId = 3, CreatedAt = new DateTime(2026, 3, 1, 8, 10, 0) },
                new User { UserId = 11, Username = "cashier08", PasswordHash = seedHash, FullName = "Đỗ Văn Ngân", Email = "cashier08@healthymart.vn", Phone = "0900000011", IsActive = true, RoleId = 3, CreatedAt = new DateTime(2026, 4, 1, 8, 0, 0) },
                new User { UserId = 12, Username = "cashier09", PasswordHash = seedHash, FullName = "Lý Thị Thu Ngân", Email = "cashier09@healthymart.vn", Phone = "0900000012", IsActive = false, RoleId = 3, CreatedAt = new DateTime(2026, 4, 1, 8, 10, 0) }
            );

            // ---------- Orders (12) ----------
            // Total = Subtotal - Discount
            modelBuilder.Entity<Order>().HasData(
                new Order { OrderId = 1, OrderCode = "HD0001", CustomerId = 1, CashierId = 4, Subtotal = 82000, Discount = 0, Total = 82000, PaymentMethod = "CASH", Status = "PAID", CreatedAt = new DateTime(2026, 9, 1, 9, 15, 0) },
                new Order { OrderId = 2, OrderCode = "HD0002", CustomerId = 2, CashierId = 4, Subtotal = 221000, Discount = 10000, Total = 211000, PaymentMethod = "MOMO", Status = "PAID", CreatedAt = new DateTime(2026, 9, 2, 10, 30, 0) },
                new Order { OrderId = 3, OrderCode = "HD0003", CustomerId = 6, CashierId = 5, Subtotal = 173000, Discount = 0, Total = 173000, PaymentMethod = "CASH", Status = "PAID", CreatedAt = new DateTime(2026, 9, 3, 11, 0, 0) },
                new Order { OrderId = 4, OrderCode = "HD0004", CustomerId = 7, CashierId = 5, Subtotal = 400000, Discount = 20000, Total = 380000, PaymentMethod = "CARD", Status = "PAID", CreatedAt = new DateTime(2026, 9, 4, 14, 20, 0) },
                new Order { OrderId = 5, OrderCode = "HD0005", CustomerId = 4, CashierId = 6, Subtotal = 467000, Discount = 23000, Total = 444000, PaymentMethod = "BANK_TRANSFER", Status = "PAID", CreatedAt = new DateTime(2026, 9, 5, 16, 45, 0) },
                new Order { OrderId = 6, OrderCode = "HD0006", CustomerId = 8, CashierId = 6, Subtotal = 158000, Discount = 0, Total = 158000, PaymentMethod = "CASH", Status = "PAID", CreatedAt = new DateTime(2026, 9, 6, 8, 40, 0) },
                new Order { OrderId = 7, OrderCode = "HD0007", CustomerId = 3, CashierId = 7, Subtotal = 126000, Discount = 0, Total = 126000, PaymentMethod = "CASH", Status = "PAID", CreatedAt = new DateTime(2026, 9, 7, 9, 5, 0) },
                new Order { OrderId = 8, OrderCode = "HD0008", CustomerId = 10, CashierId = 7, Subtotal = 237000, Discount = 12000, Total = 225000, PaymentMethod = "MOMO", Status = "PAID", CreatedAt = new DateTime(2026, 9, 8, 17, 10, 0) },
                new Order { OrderId = 9, OrderCode = "HD0009", CustomerId = 5, CashierId = 8, Subtotal = 151000, Discount = 0, Total = 151000, PaymentMethod = "CARD", Status = "PAID", CreatedAt = new DateTime(2026, 9, 9, 18, 30, 0) },
                new Order { OrderId = 10, OrderCode = "HD0010", CustomerId = 11, CashierId = 8, Subtotal = 129000, Discount = 0, Total = 129000, PaymentMethod = "CASH", Status = "CANCELLED", CreatedAt = new DateTime(2026, 9, 10, 12, 0, 0) },
                new Order { OrderId = 11, OrderCode = "HD0011", CustomerId = 9, CashierId = 9, Subtotal = 262000, Discount = 12000, Total = 250000, PaymentMethod = "BANK_TRANSFER", Status = "PAID", CreatedAt = new DateTime(2026, 9, 11, 15, 25, 0) },
                new Order { OrderId = 12, OrderCode = "HD0012", CustomerId = 12, CashierId = 9, Subtotal = 258000, Discount = 0, Total = 258000, PaymentMethod = "MOMO", Status = "PAID", CreatedAt = new DateTime(2026, 9, 12, 19, 0, 0) }
            );

            // ---------- OrderItems (26) ----------
            // LineTotal = Quantity * UnitPrice ; UnitPrice = giá bán tại thời điểm tạo hoá đơn
            modelBuilder.Entity<OrderItem>().HasData(
                // HD0001
                new OrderItem { OrderItemId = 1, OrderId = 1, ProductId = 1, Quantity = 2, UnitPrice = 25000, LineTotal = 50000 },
                new OrderItem { OrderItemId = 2, OrderId = 1, ProductId = 2, Quantity = 1, UnitPrice = 32000, LineTotal = 32000 },
                // HD0002
                new OrderItem { OrderItemId = 3, OrderId = 2, ProductId = 3, Quantity = 1, UnitPrice = 65000, LineTotal = 65000 },
                new OrderItem { OrderItemId = 4, OrderId = 2, ProductId = 5, Quantity = 2, UnitPrice = 78000, LineTotal = 156000 },
                // HD0003
                new OrderItem { OrderItemId = 5, OrderId = 3, ProductId = 7, Quantity = 1, UnitPrice = 89000, LineTotal = 89000 },
                new OrderItem { OrderItemId = 6, OrderId = 3, ProductId = 8, Quantity = 2, UnitPrice = 42000, LineTotal = 84000 },
                // HD0004
                new OrderItem { OrderItemId = 7, OrderId = 4, ProductId = 9, Quantity = 1, UnitPrice = 145000, LineTotal = 145000 },
                new OrderItem { OrderItemId = 8, OrderId = 4, ProductId = 10, Quantity = 1, UnitPrice = 120000, LineTotal = 120000 },
                new OrderItem { OrderItemId = 9, OrderId = 4, ProductId = 17, Quantity = 1, UnitPrice = 135000, LineTotal = 135000 },
                // HD0005
                new OrderItem { OrderItemId = 10, OrderId = 5, ProductId = 12, Quantity = 2, UnitPrice = 169000, LineTotal = 338000 },
                new OrderItem { OrderItemId = 11, OrderId = 5, ProductId = 11, Quantity = 1, UnitPrice = 129000, LineTotal = 129000 },
                // HD0006
                new OrderItem { OrderItemId = 12, OrderId = 6, ProductId = 13, Quantity = 2, UnitPrice = 52000, LineTotal = 104000 },
                new OrderItem { OrderItemId = 13, OrderId = 6, ProductId = 16, Quantity = 3, UnitPrice = 18000, LineTotal = 54000 },
                // HD0007
                new OrderItem { OrderItemId = 14, OrderId = 7, ProductId = 14, Quantity = 1, UnitPrice = 48000, LineTotal = 48000 },
                new OrderItem { OrderItemId = 15, OrderId = 7, ProductId = 15, Quantity = 2, UnitPrice = 39000, LineTotal = 78000 },
                // HD0008
                new OrderItem { OrderItemId = 16, OrderId = 8, ProductId = 4, Quantity = 1, UnitPrice = 95000, LineTotal = 95000 },
                new OrderItem { OrderItemId = 17, OrderId = 8, ProductId = 6, Quantity = 2, UnitPrice = 55000, LineTotal = 110000 },
                new OrderItem { OrderItemId = 18, OrderId = 8, ProductId = 2, Quantity = 1, UnitPrice = 32000, LineTotal = 32000 },
                // HD0009
                new OrderItem { OrderItemId = 19, OrderId = 9, ProductId = 8, Quantity = 3, UnitPrice = 42000, LineTotal = 126000 },
                new OrderItem { OrderItemId = 20, OrderId = 9, ProductId = 1, Quantity = 1, UnitPrice = 25000, LineTotal = 25000 },
                // HD0010 (đã huỷ)
                new OrderItem { OrderItemId = 21, OrderId = 10, ProductId = 11, Quantity = 1, UnitPrice = 129000, LineTotal = 129000 },
                // HD0011
                new OrderItem { OrderItemId = 22, OrderId = 11, ProductId = 3, Quantity = 2, UnitPrice = 65000, LineTotal = 130000 },
                new OrderItem { OrderItemId = 23, OrderId = 11, ProductId = 14, Quantity = 2, UnitPrice = 48000, LineTotal = 96000 },
                new OrderItem { OrderItemId = 24, OrderId = 11, ProductId = 16, Quantity = 2, UnitPrice = 18000, LineTotal = 36000 },
                // HD0012
                new OrderItem { OrderItemId = 25, OrderId = 12, ProductId = 7, Quantity = 1, UnitPrice = 89000, LineTotal = 89000 },
                new OrderItem { OrderItemId = 26, OrderId = 12, ProductId = 12, Quantity = 1, UnitPrice = 169000, LineTotal = 169000 }
            );
        }
    }
}