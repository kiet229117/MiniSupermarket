-- =======================================================================
-- CÆ  Sá»ž Dá»® LIá»†U HOÃ€N CHá»ˆNH: VoAnhKiet_MiniMart
-- Äá»“ Ã¡n: Mini Supermarket (Quáº£n lÃ½ SiÃªu thá»‹ Mini)
-- Bao gá»“m Ä‘áº§y Ä‘á»§:
--   1. Táº¡o Database [VoAnhKiet_MiniMart]
--   2. Cáº¥u trÃºc toÃ n bá»™ 11 báº£ng dá»¯ liá»‡u & KhÃ³a chÃ­nh, KhÃ³a ngoáº¡i, Index
--   3. ToÃ n bá»™ dá»¯ liá»‡u máº«u (Seed Data): Roles, Users, Categories, Brands, 
--      Suppliers, Products, Customers, Orders, OrderItems, OrderDetails
--   4. Cáº¥u trÃºc phá»¥c vá»¥ POS Checkout bá»c SqlTransaction (ACID)
--   5. Cáº£ 3 Stored Procedures bÃ¡o cÃ¡o:
--      - sp_GetRevenueByCashier (Doanh thu theo thu ngÃ¢n)
--      - sp_GetRevenueByDateRange (Doanh thu theo khoáº£ng ngÃ y)
--      - sp_GetTop5SellingProducts (Top 5 sáº£n pháº©m bÃ¡n cháº¡y nháº¥t)
-- =======================================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'VoAnhKiet_MiniMart')
BEGIN
    CREATE DATABASE [VoAnhKiet_MiniMart];
END
GO

USE [VoAnhKiet_MiniMart];
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Brands] (
    [BrandId] int NOT NULL IDENTITY,
    [BrandName] nvarchar(100) NOT NULL,
    [Country] nvarchar(100) NULL,
    [Description] nvarchar(255) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Brands] PRIMARY KEY ([BrandId])
);
GO

CREATE TABLE [Categories] (
    [CategoryId] int NOT NULL IDENTITY,
    [CategoryName] nvarchar(100) NOT NULL,
    [Description] nvarchar(255) NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([CategoryId])
);
GO

CREATE TABLE [Customers] (
    [CustomerId] int NOT NULL IDENTITY,
    [CustomerName] nvarchar(100) NOT NULL,
    [PhoneNumber] nvarchar(15) NOT NULL,
    [Address] nvarchar(200) NULL,
    [RewardPoints] int NOT NULL,
    [MembershipRank] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([CustomerId])
);
GO

CREATE TABLE [Roles] (
    [RoleId] int NOT NULL IDENTITY,
    [RoleName] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([RoleId])
);
GO

CREATE TABLE [Suppliers] (
    [SupplierId] int NOT NULL IDENTITY,
    [SupplierName] nvarchar(150) NOT NULL,
    [PhoneNumber] nvarchar(15) NULL,
    [Email] nvarchar(100) NULL,
    [Address] nvarchar(255) NULL,
    CONSTRAINT [PK_Suppliers] PRIMARY KEY ([SupplierId])
);
GO

CREATE TABLE [Users] (
    [UserId] int NOT NULL IDENTITY,
    [Username] nvarchar(50) NOT NULL,
    [PasswordHash] nvarchar(255) NOT NULL,
    [FullName] nvarchar(100) NOT NULL,
    [Email] nvarchar(100) NULL,
    [Phone] nvarchar(15) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [RoleId] int NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserId]),
    CONSTRAINT [FK_Users_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([RoleId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Products] (
    [ProductId] int NOT NULL IDENTITY,
    [Barcode] nvarchar(50) NOT NULL,
    [ProductName] nvarchar(150) NOT NULL,
    [Unit] nvarchar(20) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [CostPrice] decimal(18,2) NOT NULL,
    [StockQuantity] int NOT NULL,
    [IsActive] bit NOT NULL,
    [CategoryId] int NOT NULL,
    [BrandId] int NOT NULL,
    [SupplierId] int NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([ProductId]),
    CONSTRAINT [FK_Products_Brands_BrandId] FOREIGN KEY ([BrandId]) REFERENCES [Brands] ([BrandId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([CategoryId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Products_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([SupplierId]) ON DELETE SET NULL
);
GO

CREATE TABLE [Orders] (
    [OrderId] int NOT NULL IDENTITY,
    [OrderCode] nvarchar(30) NOT NULL,
    [Subtotal] decimal(18,2) NOT NULL,
    [Discount] decimal(18,2) NOT NULL,
    [Total] decimal(18,2) NOT NULL,
    [PaymentMethod] nvarchar(20) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CustomerId] int NULL,
    [CashierId] int NOT NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY ([OrderId]),
    CONSTRAINT [FK_Orders_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Orders_Users_CashierId] FOREIGN KEY ([CashierId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [RefreshTokens] (
    [RefreshTokenId] int NOT NULL IDENTITY,
    [Token] nvarchar(512) NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [Revoked] bit NOT NULL,
    [UserId] int NOT NULL,
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([RefreshTokenId]),
    CONSTRAINT [FK_RefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);
GO

CREATE TABLE [OrderItems] (
    [OrderItemId] int NOT NULL IDENTITY,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [LineTotal] decimal(18,2) NOT NULL,
    [OrderId] int NOT NULL,
    [ProductId] int NOT NULL,
    CONSTRAINT [PK_OrderItems] PRIMARY KEY ([OrderItemId]),
    CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId]) ON DELETE CASCADE,
    CONSTRAINT [FK_OrderItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]) ON DELETE NO ACTION
);
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'BrandId', N'BrandName', N'Country', N'Description', N'IsActive') AND [object_id] = OBJECT_ID(N'[Brands]'))
    SET IDENTITY_INSERT [Brands] ON;
INSERT INTO [Brands] ([BrandId], [BrandName], [Country], [Description], [IsActive])
VALUES (1, N'Healthy Mart', N'Việt Nam', N'Thương hiệu riêng của cửa hàng với các sản phẩm rau củ, trái cây và thực phẩm sạch.', CAST(1 AS bit)),
(2, N'Quaker', N'Mỹ', N'Thương hiệu yến mạch và ngũ cốc ăn sáng giàu chất xơ.', CAST(1 AS bit)),
(3, N'TH true MILK', N'Việt Nam', N'Thương hiệu sữa tươi và các sản phẩm từ sữa.', CAST(1 AS bit)),
(4, N'Vinamilk', N'Việt Nam', N'Thương hiệu sữa và các sản phẩm dinh dưỡng phổ biến tại Việt Nam.', CAST(1 AS bit)),
(5, N'Alsafi', N'Campuchia', N'Thương hiệu sữa hạt và đồ uống có nguồn gốc thực vật.', CAST(1 AS bit)),
(6, N'Bertolli', N'Ý', N'Thương hiệu dầu oliu và các sản phẩm theo phong cách ẩm thực Địa Trung Hải.', CAST(1 AS bit)),
(7, N'Organic Farm VN', N'Việt Nam', N'Rau củ, trứng và các sản phẩm nông nghiệp hữu cơ.', CAST(1 AS bit)),
(8, N'Eat Clean VN', N'Việt Nam', N'Các sản phẩm phục vụ chế độ ăn Eat Clean và ăn uống lành mạnh.', CAST(1 AS bit)),
(9, N'Dalat Green', N'Việt Nam', N'Trà thảo mộc và các loại đồ uống có nguồn gốc tự nhiên.', CAST(1 AS bit)),
(10, N'Blue Diamond', N'Mỹ', N'Thương hiệu hạnh nhân và các sản phẩm chế biến từ hạnh nhân.', CAST(1 AS bit)),
(11, N'Bee Gold', N'Việt Nam', N'Mật ong nguyên chất và các sản phẩm từ mật ong.', CAST(1 AS bit)),
(12, N'Nutri Seed', N'Việt Nam', N'Superfood, hạt dinh dưỡng và các sản phẩm bổ sung dinh dưỡng.', CAST(1 AS bit)),
(13, N'Green Life', N'Việt Nam', N'Thực phẩm chay và các sản phẩm có nguồn gốc từ thực vật.', CAST(1 AS bit)),
(14, N'Nature Food', N'Việt Nam', N'Thực phẩm khô, ngũ cốc và các sản phẩm tự nhiên tiện lợi.', CAST(1 AS bit)),
(15, N'Fresh Choice', N'Việt Nam', N'Thực phẩm đông lạnh, thịt cá và các sản phẩm tiện lợi chất lượng cao.', CAST(1 AS bit));
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'BrandId', N'BrandName', N'Country', N'Description', N'IsActive') AND [object_id] = OBJECT_ID(N'[Brands]'))
    SET IDENTITY_INSERT [Brands] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CategoryId', N'CategoryName', N'Description') AND [object_id] = OBJECT_ID(N'[Categories]'))
    SET IDENTITY_INSERT [Categories] ON;
INSERT INTO [Categories] ([CategoryId], [CategoryName], [Description])
VALUES (1, N'Rau củ quả hữu cơ', N'Các loại rau xanh, củ và quả tươi được trồng theo phương pháp hữu cơ.'),
(2, N'Ngũ cốc & Hạt dinh dưỡng', N'Yến mạch, hạt chia, hạnh nhân, óc chó và các loại hạt giàu dinh dưỡng.'),
(3, N'Sữa hạt & Sữa chua hữu cơ', N'Sữa hạnh nhân, sữa đậu nành, sữa óc chó và các loại sữa chua hữu cơ.'),
(4, N'Thực phẩm Eat Clean', N'Các sản phẩm phù hợp chế độ ăn Eat Clean như ức gà, cơm gạo lứt và salad.'),
(5, N'Gia vị & Dầu thực vật tự nhiên', N'Dầu oliu, mật ong, muối hồng và các loại gia vị có nguồn gốc tự nhiên.'),
(6, N'Trái cây tươi & nhập khẩu', N'Các loại trái cây tươi trong nước và nhập khẩu như táo, nho, kiwi và bơ.'),
(7, N'Thịt cá sạch', N'Thịt bò, thịt heo, cá hồi và các loại hải sản tươi sống được kiểm soát chất lượng.'),
(8, N'Trứng & Chế phẩm từ trứng', N'Trứng gà ta, trứng gà hữu cơ, trứng cút và các sản phẩm chế biến từ trứng.'),
(9, N'Trà & Đồ uống tốt cho sức khỏe', N'Trà thảo mộc, nước ép nguyên chất, kombucha và các loại đồ uống lành mạnh.'),
(10, N'Bánh & Snack lành mạnh', N'Bánh yến mạch, snack rong biển, trái cây sấy và các món ăn nhẹ lành mạnh.'),
(11, N'Đậu & Sản phẩm từ đậu', N'Đậu hũ, tempeh và các loại đậu khô giàu protein thực vật.'),
(12, N'Superfood & Thực phẩm bổ sung', N'Matcha, spirulina, bột rau củ và các sản phẩm thực phẩm bổ sung dinh dưỡng.'),
(13, N'Thực phẩm đông lạnh', N'Rau củ, thịt cá và các loại thực phẩm được bảo quản đông lạnh tiện lợi.'),
(14, N'Thực phẩm khô', N'Các loại mì, bún, phở, nấm khô và thực phẩm khô tiện lợi cho gia đình.'),
(15, N'Thực phẩm chay', N'Các sản phẩm chay từ thực vật như thịt chay, đậu phụ và thực phẩm giàu protein thực vật.');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CategoryId', N'CategoryName', N'Description') AND [object_id] = OBJECT_ID(N'[Categories]'))
    SET IDENTITY_INSERT [Categories] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CustomerId', N'Address', N'CustomerName', N'MembershipRank', N'PhoneNumber', N'RewardPoints') AND [object_id] = OBJECT_ID(N'[Customers]'))
    SET IDENTITY_INSERT [Customers] ON;
INSERT INTO [Customers] ([CustomerId], [Address], [CustomerName], [MembershipRank], [PhoneNumber], [RewardPoints])
VALUES (1, N'59 Nguyễn Thị Minh Khai, Phường Võ Thị Sáu, Quận 3, TP. Hồ Chí Minh', N'Nguyễn Văn A', N'Vàng', N'0901122334', 150),
(2, N'120 Võ Văn Tần, Phường Võ Thị Sáu, Quận 3, TP. Hồ Chí Minh', N'Trần Thị B', N'Bạc', N'0918877665', 50),
(3, N'15 Lý Thường Kiệt, Phường 14, Quận 10, TP. Hồ Chí Minh', N'Lê Văn C', N'Chuẩn', N'0983344556', 10),
(4, N'12 Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh', N'Phạm Minh Tuấn', N'Vàng', N'0905123456', 180),
(5, N'45 Lê Lợi, Phường Bến Thành, Quận 1, TP. Hồ Chí Minh', N'Hoàng Thị Lan', N'Bạc', N'0912345678', 75),
(6, N'8 Trần Hưng Đạo, Phường 6, Quận 5, TP. Hồ Chí Minh', N'Võ Quốc Bảo', N'Chuẩn', N'0937654321', 20),
(7, N'102 Điện Biên Phủ, Phường 17, Quận Bình Thạnh, TP. Hồ Chí Minh', N'Đặng Thu Hà', N'Kim cương', N'0944556677', 520),
(8, N'36 Nguyễn Thị Thập, Phường Tân Quy, Quận 7, TP. Hồ Chí Minh', N'Bùi Anh Khoa', N'Chuẩn', N'0966778899', 5),
(9, N'27 Phan Văn Trị, Phường 10, Quận Gò Vấp, TP. Hồ Chí Minh', N'Ngô Thanh Mai', N'Bạc', N'0977889900', 90),
(10, N'63 Cách Mạng Tháng 8, Phường 12, Quận 10, TP. Hồ Chí Minh', N'Đỗ Hoài Nam', N'Vàng', N'0988990011', 210),
(11, N'210 Quang Trung, Phường 10, Quận Gò Vấp, TP. Hồ Chí Minh', N'Lý Gia Hân', N'Chuẩn', N'0399123456', 0),
(12, N'5 Võ Văn Ngân, Phường Linh Chiểu, TP. Thủ Đức, TP. Hồ Chí Minh', N'Trương Quốc Việt', N'Bạc', N'0868234567', 60),
(13, N'88 Nguyễn Oanh, Phường 17, Quận Gò Vấp, TP. Hồ Chí Minh', N'Nguyễn Minh Khôi', N'Vàng', N'0908765432', 230),
(14, N'156 Hoàng Diệu, Phường 6, Quận 4, TP. Hồ Chí Minh', N'Phan Ngọc Anh', N'Bạc', N'0938123456', 85),
(15, N'72 Kha Vạn Cân, Phường Hiệp Bình Chánh, TP. Thủ Đức, TP. Hồ Chí Minh', N'Đinh Thanh Tùng', N'Chuẩn', N'0976234518', 15);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CustomerId', N'Address', N'CustomerName', N'MembershipRank', N'PhoneNumber', N'RewardPoints') AND [object_id] = OBJECT_ID(N'[Customers]'))
    SET IDENTITY_INSERT [Customers] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RoleId', N'RoleName') AND [object_id] = OBJECT_ID(N'[Roles]'))
    SET IDENTITY_INSERT [Roles] ON;
INSERT INTO [Roles] ([RoleId], [RoleName])
VALUES (1, N'ADMIN'),
(2, N'WAREHOUSE'),
(3, N'CASHIER');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RoleId', N'RoleName') AND [object_id] = OBJECT_ID(N'[Roles]'))
    SET IDENTITY_INSERT [Roles] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SupplierId', N'Address', N'Email', N'PhoneNumber', N'SupplierName') AND [object_id] = OBJECT_ID(N'[Suppliers]'))
    SET IDENTITY_INSERT [Suppliers] ON;
INSERT INTO [Suppliers] ([SupplierId], [Address], [Email], [PhoneNumber], [SupplierName])
VALUES (1, N'15 Nguyễn Văn Cừ, Phường 4, TP. Đà Lạt, Lâm Đồng', N'lienhe@nongsandalat.vn', N'02633812345', N'Công ty TNHH Nông sản sạch Đà Lạt'),
(2, N'125 Nguyễn Hữu Thọ, Phường Tân Phong, Quận 7, TP. Hồ Chí Minh', N'sales@freshviet.vn', N'02838123456', N'Công ty CP Trái cây nhập khẩu Fresh Việt'),
(3, N'86 Điện Biên Phủ, Phường 17, Quận Bình Thạnh, TP. Hồ Chí Minh', N'info@eatcleansg.vn', N'02838234567', N'Công ty TNHH Thực phẩm Eat Clean Sài Gòn'),
(4, N'42 Trường Chinh, Phường 12, Quận Tân Bình, TP. Hồ Chí Minh', N'order@ngucocmyviet.vn', N'02838345678', N'Công ty TNHH Ngũ cốc & Hạt Mỹ Việt'),
(5, N'28 Trần Thái Tông, Phường Dịch Vọng, Quận Cầu Giấy, Hà Nội', N'cskh@suaxanh.vn', N'02438456789', N'Công ty CP Sữa & Chế phẩm sữa Xanh'),
(6, N'72 Nguyễn Thái Bình, Phường Nguyễn Thái Bình, Quận 1, TP. Hồ Chí Minh', N'contact@diatrunghai.vn', N'02838567890', N'Công ty TNHH Dầu ăn Địa Trung Hải'),
(7, N'18 Nguyễn Tất Thành, Thị trấn U Minh, Huyện U Minh, Cà Mau', N'htx@matonguminh.vn', N'02916678901', N'HTX Mật ong U Minh'),
(8, N'35 Phan Đăng Lưu, Phường 3, Quận Phú Nhuận, TP. Hồ Chí Minh', N'sales@haisannauy.vn', N'02838789012', N'Công ty TNHH Hải sản Na Uy Việt Nam'),
(9, N'125 Đường Tỉnh 825, Xã Đức Hòa Hạ, Huyện Đức Hòa, Long An', N'trangtrai@trungsachla.vn', N'02723890123', N'Trang trại Trứng sạch Long An'),
(10, N'56 Trần Phú, Phường Lộc Tiến, TP. Bảo Lộc, Lâm Đồng', N'info@dalatgreen.vn', N'02633901234', N'Công ty TNHH Trà thảo mộc Dalat Green'),
(11, N'210 Võ Văn Ngân, Phường Bình Thọ, TP. Thủ Đức, TP. Hồ Chí Minh', N'ban-hang@snacklanhmanh.vn', N'02838012345', N'Công ty CP Bánh & Snack lành mạnh'),
(12, N'89 Quang Trung, Phường 10, Quận Gò Vấp, TP. Hồ Chí Minh', N'lienhe@superfoodxanh.vn', N'02838123400', N'Công ty TNHH Đậu hũ & Superfood Xanh'),
(13, N'145 Nguyễn Văn Cừ, Phường An Khánh, Quận Ninh Kiều, Cần Thơ', N'contact@rauhuucomekong.vn', N'02923678901', N'Công ty TNHH Rau củ hữu cơ Mekong'),
(14, N'320 Nguyễn Văn Quá, Phường Đông Hưng Thuận, Quận 12, TP. Hồ Chí Minh', N'sales@vietnature.vn', N'02839876543', N'Công ty CP Thực phẩm xanh Việt Nature'),
(15, N'78 Đại lộ Bình Dương, Phường Phú Hòa, TP. Thủ Dầu Một, Bình Dương', N'info@nongnsanviet.vn', N'02743781234', N'Công ty TNHH Nông sản hữu cơ An Việt');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SupplierId', N'Address', N'Email', N'PhoneNumber', N'SupplierName') AND [object_id] = OBJECT_ID(N'[Suppliers]'))
    SET IDENTITY_INSERT [Suppliers] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ProductId', N'Barcode', N'BrandId', N'CategoryId', N'CostPrice', N'IsActive', N'Price', N'ProductName', N'StockQuantity', N'SupplierId', N'Unit') AND [object_id] = OBJECT_ID(N'[Products]'))
    SET IDENTITY_INSERT [Products] ON;
INSERT INTO [Products] ([ProductId], [Barcode], [BrandId], [CategoryId], [CostPrice], [IsActive], [Price], [ProductName], [StockQuantity], [SupplierId], [Unit])
VALUES (1, N'8938501234561', 7, 1, 18000.0, CAST(1 AS bit), 25000.0, N'Rau cải bó xôi hữu cơ 300g', 50, 1, N'gói'),
(2, N'8938501234562', 7, 1, 24000.0, CAST(1 AS bit), 32000.0, N'Cà chua bi hữu cơ 500g', 40, 1, N'hộp'),
(3, N'8938501234563', 2, 2, 50000.0, CAST(1 AS bit), 65000.0, N'Yến mạch nguyên hạt Quaker 500g', 30, 4, N'gói'),
(4, N'8938501234564', 10, 2, 72000.0, CAST(1 AS bit), 95000.0, N'Hạnh nhân rang Blue Diamond 250g', 25, 4, N'gói'),
(5, N'8938501234565', 5, 3, 60000.0, CAST(1 AS bit), 78000.0, N'Sữa hạnh nhân Alsafi 946ml', 35, 5, N'hộp'),
(6, N'8938501234566', 3, 3, 41000.0, CAST(1 AS bit), 55000.0, N'Sữa chua TH true MILK không đường 500g', 20, 5, N'hộp'),
(7, N'8938501234567', 8, 4, 52000.0, CAST(1 AS bit), 69000.0, N'Ức gà Eat Clean 500g', 60, 3, N'hộp'),
(8, N'8938501234568', 8, 4, 31000.0, CAST(1 AS bit), 42000.0, N'Cơm gạo lứt Eat Clean 250g', 45, 3, N'hộp'),
(9, N'8938501234569', 6, 5, 110000.0, CAST(1 AS bit), 145000.0, N'Dầu oliu Bertolli Extra Virgin 500ml', 20, 6, N'chai'),
(10, N'8938501234570', 11, 5, 88000.0, CAST(1 AS bit), 120000.0, N'Mật ong nguyên chất Bee Gold 500ml', 15, 7, N'chai'),
(11, N'8938501234571', 1, 6, 98000.0, CAST(1 AS bit), 129000.0, N'Táo Envy nhập khẩu 1kg', 30, 2, N'kg'),
(12, N'8938501234572', 1, 7, 135000.0, CAST(1 AS bit), 169000.0, N'Cá hồi phi lê Na Uy 300g', 22, 8, N'gói'),
(13, N'8938501234573', 7, 8, 40000.0, CAST(1 AS bit), 52000.0, N'Trứng gà ta hữu cơ 10 quả', 70, 9, N'hộp'),
(14, N'8938501234574', 9, 9, 34000.0, CAST(1 AS bit), 48000.0, N'Trà hoa cúc Dalat Green 20 túi lọc', 55, 10, N'hộp'),
(15, N'8938501234575', 8, 10, 28000.0, CAST(1 AS bit), 39000.0, N'Bánh quy yến mạch Eat Clean 200g', 65, 11, N'gói'),
(16, N'8938501234576', 7, 11, 12000.0, CAST(1 AS bit), 18000.0, N'Đậu hũ non hữu cơ Organic Farm VN 300g', 80, 12, N'hộp'),
(17, N'8938501234577', 12, 12, 100000.0, CAST(1 AS bit), 135000.0, N'Bột Spirulina Nutri Seed 100g', 18, 12, N'hũ');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ProductId', N'Barcode', N'BrandId', N'CategoryId', N'CostPrice', N'IsActive', N'Price', N'ProductName', N'StockQuantity', N'SupplierId', N'Unit') AND [object_id] = OBJECT_ID(N'[Products]'))
    SET IDENTITY_INSERT [Products] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserId', N'CreatedAt', N'Email', N'FullName', N'IsActive', N'PasswordHash', N'Phone', N'RoleId', N'Username') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([UserId], [CreatedAt], [Email], [FullName], [IsActive], [PasswordHash], [Phone], [RoleId], [Username])
VALUES (1, '2026-01-05T08:00:00.0000000', N'admin@healthymart.vn', N'Quản trị hệ thống', CAST(1 AS bit), N'$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.', N'0900000001', 1, N'admin'),
(2, '2026-01-06T08:00:00.0000000', N'warehouse01@healthymart.vn', N'Nguyễn Thị Mai', CAST(1 AS bit), N'$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.', N'0900000002', 2, N'warehouse01'),
(3, '2026-01-06T08:30:00.0000000', N'warehouse02@healthymart.vn', N'Trần Văn Điều', CAST(1 AS bit), N'$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.', N'0900000003', 2, N'warehouse02'),
(4, '2026-02-01T08:00:00.0000000', N'cashier01@healthymart.vn', N'Lê Thị Thu Ngân', CAST(1 AS bit), N'$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.', N'0900000004', 3, N'cashier01'),
(5, '2026-02-01T08:10:00.0000000', N'cashier02@healthymart.vn', N'Phạm Văn Ngân', CAST(1 AS bit), N'$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.', N'0900000005', 3, N'cashier02'),
(6, '2026-02-01T08:20:00.0000000', N'cashier03@healthymart.vn', N'Hoàng Thị Ngân', CAST(1 AS bit), N'$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.', N'0900000006', 3, N'cashier03'),
(7, '2026-02-10T08:00:00.0000000', N'cashier04@healthymart.vn', N'Võ Minh Ngân', CAST(1 AS bit), N'$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.', N'0900000007', 3, N'cashier04'),
(8, '2026-02-10T08:10:00.0000000', N'cashier05@healthymart.vn', N'Đặng ThịNgân', CAST(1 AS bit), N'$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.', N'0900000008', 3, N'cashier05'),
(9, '2026-03-01T08:00:00.0000000', N'cashier06@healthymart.vn', N'Bùi Văn Ngân', CAST(1 AS bit), N'$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.', N'0900000009', 3, N'cashier06'),
(10, '2026-03-01T08:10:00.0000000', N'cashier07@healthymart.vn', N'Ngô Thị Thu Ngân', CAST(1 AS bit), N'$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.', N'0900000010', 3, N'cashier07'),
(11, '2026-04-01T08:00:00.0000000', N'cashier08@healthymart.vn', N'Đỗ Văn Ngân', CAST(1 AS bit), N'$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.', N'0900000011', 3, N'cashier08'),
(12, '2026-04-01T08:10:00.0000000', N'cashier09@healthymart.vn', N'Lý Thị Thu Ngân', CAST(0 AS bit), N'$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.', N'0900000012', 3, N'cashier09');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserId', N'CreatedAt', N'Email', N'FullName', N'IsActive', N'PasswordHash', N'Phone', N'RoleId', N'Username') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'OrderId', N'CashierId', N'CreatedAt', N'CustomerId', N'Discount', N'OrderCode', N'PaymentMethod', N'Status', N'Subtotal', N'Total') AND [object_id] = OBJECT_ID(N'[Orders]'))
    SET IDENTITY_INSERT [Orders] ON;
INSERT INTO [Orders] ([OrderId], [CashierId], [CreatedAt], [CustomerId], [Discount], [OrderCode], [PaymentMethod], [Status], [Subtotal], [Total])
VALUES (1, 4, '2026-09-01T09:15:00.0000000', 1, 0.0, N'HD0001', N'CASH', N'PAID', 82000.0, 82000.0),
(2, 4, '2026-09-02T10:30:00.0000000', 2, 10000.0, N'HD0002', N'MOMO', N'PAID', 221000.0, 211000.0),
(3, 5, '2026-09-03T11:00:00.0000000', 6, 0.0, N'HD0003', N'CASH', N'PAID', 173000.0, 173000.0),
(4, 5, '2026-09-04T14:20:00.0000000', 7, 20000.0, N'HD0004', N'CARD', N'PAID', 400000.0, 380000.0),
(5, 6, '2026-09-05T16:45:00.0000000', 4, 23000.0, N'HD0005', N'BANK_TRANSFER', N'PAID', 467000.0, 444000.0),
(6, 6, '2026-09-06T08:40:00.0000000', 8, 0.0, N'HD0006', N'CASH', N'PAID', 158000.0, 158000.0),
(7, 7, '2026-09-07T09:05:00.0000000', 3, 0.0, N'HD0007', N'CASH', N'PAID', 126000.0, 126000.0),
(8, 7, '2026-09-08T17:10:00.0000000', 10, 12000.0, N'HD0008', N'MOMO', N'PAID', 237000.0, 225000.0),
(9, 8, '2026-09-09T18:30:00.0000000', 5, 0.0, N'HD0009', N'CARD', N'PAID', 151000.0, 151000.0),
(10, 8, '2026-09-10T12:00:00.0000000', 11, 0.0, N'HD0010', N'CASH', N'CANCELLED', 129000.0, 129000.0),
(11, 9, '2026-09-11T15:25:00.0000000', 9, 12000.0, N'HD0011', N'BANK_TRANSFER', N'PAID', 262000.0, 250000.0),
(12, 9, '2026-09-12T19:00:00.0000000', 12, 0.0, N'HD0012', N'MOMO', N'PAID', 258000.0, 258000.0);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'OrderId', N'CashierId', N'CreatedAt', N'CustomerId', N'Discount', N'OrderCode', N'PaymentMethod', N'Status', N'Subtotal', N'Total') AND [object_id] = OBJECT_ID(N'[Orders]'))
    SET IDENTITY_INSERT [Orders] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'OrderItemId', N'LineTotal', N'OrderId', N'ProductId', N'Quantity', N'UnitPrice') AND [object_id] = OBJECT_ID(N'[OrderItems]'))
    SET IDENTITY_INSERT [OrderItems] ON;
INSERT INTO [OrderItems] ([OrderItemId], [LineTotal], [OrderId], [ProductId], [Quantity], [UnitPrice])
VALUES (1, 50000.0, 1, 1, 2, 25000.0),
(2, 32000.0, 1, 2, 1, 32000.0),
(3, 65000.0, 2, 3, 1, 65000.0),
(4, 156000.0, 2, 5, 2, 78000.0),
(5, 89000.0, 3, 7, 1, 89000.0),
(6, 84000.0, 3, 8, 2, 42000.0),
(7, 145000.0, 4, 9, 1, 145000.0),
(8, 120000.0, 4, 10, 1, 120000.0),
(9, 135000.0, 4, 17, 1, 135000.0),
(10, 338000.0, 5, 12, 2, 169000.0),
(11, 129000.0, 5, 11, 1, 129000.0),
(12, 104000.0, 6, 13, 2, 52000.0),
(13, 54000.0, 6, 16, 3, 18000.0),
(14, 48000.0, 7, 14, 1, 48000.0),
(15, 78000.0, 7, 15, 2, 39000.0),
(16, 95000.0, 8, 4, 1, 95000.0),
(17, 110000.0, 8, 6, 2, 55000.0),
(18, 32000.0, 8, 2, 1, 32000.0),
(19, 126000.0, 9, 8, 3, 42000.0),
(20, 25000.0, 9, 1, 1, 25000.0),
(21, 129000.0, 10, 11, 1, 129000.0),
(22, 130000.0, 11, 3, 2, 65000.0),
(23, 96000.0, 11, 14, 2, 48000.0),
(24, 36000.0, 11, 16, 2, 18000.0),
(25, 89000.0, 12, 7, 1, 89000.0),
(26, 169000.0, 12, 12, 1, 169000.0);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'OrderItemId', N'LineTotal', N'OrderId', N'ProductId', N'Quantity', N'UnitPrice') AND [object_id] = OBJECT_ID(N'[OrderItems]'))
    SET IDENTITY_INSERT [OrderItems] OFF;
GO

CREATE UNIQUE INDEX [IX_Brands_BrandName] ON [Brands] ([BrandName]);
GO

CREATE UNIQUE INDEX [IX_Categories_CategoryName] ON [Categories] ([CategoryName]);
GO

CREATE UNIQUE INDEX [IX_Customers_PhoneNumber] ON [Customers] ([PhoneNumber]);
GO

CREATE INDEX [IX_OrderItems_OrderId] ON [OrderItems] ([OrderId]);
GO

CREATE INDEX [IX_OrderItems_ProductId] ON [OrderItems] ([ProductId]);
GO

CREATE INDEX [IX_Orders_CashierId] ON [Orders] ([CashierId]);
GO

CREATE INDEX [IX_Orders_CustomerId] ON [Orders] ([CustomerId]);
GO

CREATE UNIQUE INDEX [IX_Orders_OrderCode] ON [Orders] ([OrderCode]);
GO

CREATE UNIQUE INDEX [IX_Products_Barcode] ON [Products] ([Barcode]);
GO

CREATE INDEX [IX_Products_BrandId] ON [Products] ([BrandId]);
GO

CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);
GO

CREATE INDEX [IX_Products_SupplierId] ON [Products] ([SupplierId]);
GO

CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);
GO

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]) WHERE [Email] IS NOT NULL;
GO

CREATE INDEX [IX_Users_RoleId] ON [Users] ([RoleId]);
GO

CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261010100142_lan001', N'8.0.20');
GO

COMMIT;
GO



-- =======================================================================
-- PHáº¦N Bá»” SUNG: Báº¢NG OrderDetails & Cá»˜T Há»– TRá»¢ POS CHECKOUT (ACID)
-- =======================================================================

-- Bá»• sung cÃ¡c cá»™t vÃ o báº£ng Orders náº¿u chÆ°a cÃ³
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
GO

-- RÃ ng buá»™c giÃ¡ trá»‹ máº·c Ä‘á»‹nh an toÃ n cho Orders
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
GO

-- Táº¡o báº£ng OrderDetails phá»¥c vá»¥ thanh toÃ¡n POS
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
END
GO

-- Äá»“ng bá»™ dá»¯ liá»‡u Orders sáºµn cÃ³
UPDATE o
SET 
    o.CreatedDate = ISNULL(o.CreatedDate, o.CreatedAt),
    o.CashierUsername = ISNULL(o.CashierUsername, u.Username),
    o.FinalAmount = ISNULL(o.FinalAmount, o.Total),
    o.TotalItems = ISNULL(o.TotalItems, ISNULL((SELECT SUM(Quantity) FROM OrderItems WHERE OrderId = o.OrderId), 0)),
    o.RewardPoints = ISNULL(o.RewardPoints, CAST(FLOOR(o.Total / 10000.0) AS INT))
FROM Orders o
LEFT JOIN Users u ON o.CashierId = u.UserId;
GO

-- Náº¡p dá»¯ liá»‡u vÃ o OrderDetails tá»« OrderItems náº¿u OrderDetails rá»—ng
IF NOT EXISTS (SELECT 1 FROM OrderDetails)
BEGIN
    INSERT INTO OrderDetails (OrderId, ProductId, Quantity, UnitPrice, LineTotal)
    SELECT OrderId, ProductId, Quantity, UnitPrice, LineTotal FROM OrderItems;
END
GO

-- Trigger tá»± Ä‘á»™ng Ä‘á»“ng bá»™ OrderDetails sang OrderItems
CREATE OR ALTER TRIGGER trg_OrderDetails_SyncItems
ON OrderDetails
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, LineTotal)
    SELECT i.OrderId, i.ProductId, i.Quantity, i.UnitPrice, i.LineTotal
    FROM inserted i;
END
GO

-- Trigger tá»± Ä‘á»™ng xÃ³a OrderItems khi xÃ³a OrderDetails
CREATE OR ALTER TRIGGER trg_OrderDetails_SyncDelete
ON OrderDetails
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DELETE oi
    FROM OrderItems oi
    INNER JOIN deleted d ON oi.OrderId = d.OrderId AND oi.ProductId = d.ProductId;
END
GO

-- =======================================================================
-- PHáº¦N CÃC STORED PROCEDURES BÃO CÃO DOANH THU & DASHBOARD
-- =======================================================================

-- 1. BÃ¡o cÃ¡o Doanh thu theo NhÃ¢n viÃªn thu ngÃ¢n (sp_GetRevenueByCashier)
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
END
GO

-- 2. BÃ¡o cÃ¡o Doanh thu theo Khoáº£ng ngÃ y (sp_GetRevenueByDateRange)
CREATE OR ALTER PROCEDURE sp_GetRevenueByDateRange
    @FromDate DATETIME,
    @ToDate DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        ISNULL(u.Username, N'KhÃ´ng xÃ¡c Ä‘á»‹nh') AS CashierUsername,
        COUNT(o.OrderId) AS TotalOrders,
        ISNULL(SUM(o.Total), 0) AS TotalRevenue
    FROM Orders o
    LEFT JOIN Users u ON o.CashierId = u.UserId
    WHERE o.CreatedAt >= @FromDate 
      AND o.CreatedAt <= @ToDate
      AND o.Status = 'PAID'
    GROUP BY u.Username
    ORDER BY TotalRevenue DESC;
END
GO

-- 3. Top 5 Sáº£n pháº©m BÃ¡n cháº¡y nháº¥t (sp_GetTop5SellingProducts)
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
END
GO
