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
VALUES (1, N'Healthy Mart', N'Vi?t Nam', N'Thuong hi?u riêng c?a c?a hàng v?i các s?n ph?m rau c?, trái cây và th?c ph?m s?ch.', CAST(1 AS bit)),
(2, N'Quaker', N'M?', N'Thuong hi?u y?n m?ch và ngu c?c an sáng giàu ch?t xo.', CAST(1 AS bit)),
(3, N'TH true MILK', N'Vi?t Nam', N'Thuong hi?u s?a tuoi và các s?n ph?m t? s?a.', CAST(1 AS bit)),
(4, N'Vinamilk', N'Vi?t Nam', N'Thuong hi?u s?a và các s?n ph?m dinh du?ng ph? bi?n t?i Vi?t Nam.', CAST(1 AS bit)),
(5, N'Alsafi', N'Campuchia', N'Thuong hi?u s?a h?t và d? u?ng có ngu?n g?c th?c v?t.', CAST(1 AS bit)),
(6, N'Bertolli', N'Ý', N'Thuong hi?u d?u oliu và các s?n ph?m theo phong cách ?m th?c Ð?a Trung H?i.', CAST(1 AS bit)),
(7, N'Organic Farm VN', N'Vi?t Nam', N'Rau c?, tr?ng và các s?n ph?m nông nghi?p h?u co.', CAST(1 AS bit)),
(8, N'Eat Clean VN', N'Vi?t Nam', N'Các s?n ph?m ph?c v? ch? d? an Eat Clean và an u?ng lành m?nh.', CAST(1 AS bit)),
(9, N'Dalat Green', N'Vi?t Nam', N'Trà th?o m?c và các lo?i d? u?ng có ngu?n g?c t? nhiên.', CAST(1 AS bit)),
(10, N'Blue Diamond', N'M?', N'Thuong hi?u h?nh nhân và các s?n ph?m ch? bi?n t? h?nh nhân.', CAST(1 AS bit)),
(11, N'Bee Gold', N'Vi?t Nam', N'M?t ong nguyên ch?t và các s?n ph?m t? m?t ong.', CAST(1 AS bit)),
(12, N'Nutri Seed', N'Vi?t Nam', N'Superfood, h?t dinh du?ng và các s?n ph?m b? sung dinh du?ng.', CAST(1 AS bit)),
(13, N'Green Life', N'Vi?t Nam', N'Th?c ph?m chay và các s?n ph?m có ngu?n g?c t? th?c v?t.', CAST(1 AS bit)),
(14, N'Nature Food', N'Vi?t Nam', N'Th?c ph?m khô, ngu c?c và các s?n ph?m t? nhiên ti?n l?i.', CAST(1 AS bit)),
(15, N'Fresh Choice', N'Vi?t Nam', N'Th?c ph?m dông l?nh, th?t cá và các s?n ph?m ti?n l?i ch?t lu?ng cao.', CAST(1 AS bit));
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'BrandId', N'BrandName', N'Country', N'Description', N'IsActive') AND [object_id] = OBJECT_ID(N'[Brands]'))
    SET IDENTITY_INSERT [Brands] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CategoryId', N'CategoryName', N'Description') AND [object_id] = OBJECT_ID(N'[Categories]'))
    SET IDENTITY_INSERT [Categories] ON;
INSERT INTO [Categories] ([CategoryId], [CategoryName], [Description])
VALUES (1, N'Rau c? qu? h?u co', N'Các lo?i rau xanh, c? và qu? tuoi du?c tr?ng theo phuong pháp h?u co.'),
(2, N'Ngu c?c & H?t dinh du?ng', N'Y?n m?ch, h?t chia, h?nh nhân, óc chó và các lo?i h?t giàu dinh du?ng.'),
(3, N'S?a h?t & S?a chua h?u co', N'S?a h?nh nhân, s?a d?u nành, s?a óc chó và các lo?i s?a chua h?u co.'),
(4, N'Th?c ph?m Eat Clean', N'Các s?n ph?m phù h?p ch? d? an Eat Clean nhu ?c gà, com g?o l?t và salad.'),
(5, N'Gia v? & D?u th?c v?t t? nhiên', N'D?u oliu, m?t ong, mu?i h?ng và các lo?i gia v? có ngu?n g?c t? nhiên.'),
(6, N'Trái cây tuoi & nh?p kh?u', N'Các lo?i trái cây tuoi trong nu?c và nh?p kh?u nhu táo, nho, kiwi và bo.'),
(7, N'Th?t cá s?ch', N'Th?t bò, th?t heo, cá h?i và các lo?i h?i s?n tuoi s?ng du?c ki?m soát ch?t lu?ng.'),
(8, N'Tr?ng & Ch? ph?m t? tr?ng', N'Tr?ng gà ta, tr?ng gà h?u co, tr?ng cút và các s?n ph?m ch? bi?n t? tr?ng.'),
(9, N'Trà & Ð? u?ng t?t cho s?c kh?e', N'Trà th?o m?c, nu?c ép nguyên ch?t, kombucha và các lo?i d? u?ng lành m?nh.'),
(10, N'Bánh & Snack lành m?nh', N'Bánh y?n m?ch, snack rong bi?n, trái cây s?y và các món an nh? lành m?nh.'),
(11, N'Ð?u & S?n ph?m t? d?u', N'Ð?u hu, tempeh và các lo?i d?u khô giàu protein th?c v?t.'),
(12, N'Superfood & Th?c ph?m b? sung', N'Matcha, spirulina, b?t rau c? và các s?n ph?m th?c ph?m b? sung dinh du?ng.'),
(13, N'Th?c ph?m dông l?nh', N'Rau c?, th?t cá và các lo?i th?c ph?m du?c b?o qu?n dông l?nh ti?n l?i.'),
(14, N'Th?c ph?m khô', N'Các lo?i mì, bún, ph?, n?m khô và th?c ph?m khô ti?n l?i cho gia dình.'),
(15, N'Th?c ph?m chay', N'Các s?n ph?m chay t? th?c v?t nhu th?t chay, d?u ph? và th?c ph?m giàu protein th?c v?t.');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CategoryId', N'CategoryName', N'Description') AND [object_id] = OBJECT_ID(N'[Categories]'))
    SET IDENTITY_INSERT [Categories] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'CustomerId', N'Address', N'CustomerName', N'MembershipRank', N'PhoneNumber', N'RewardPoints') AND [object_id] = OBJECT_ID(N'[Customers]'))
    SET IDENTITY_INSERT [Customers] ON;
INSERT INTO [Customers] ([CustomerId], [Address], [CustomerName], [MembershipRank], [PhoneNumber], [RewardPoints])
VALUES (1, N'59 Nguy?n Th? Minh Khai, Phu?ng Võ Th? Sáu, Qu?n 3, TP. H? Chí Minh', N'Nguy?n Van A', N'Vàng', N'0901122334', 150),
(2, N'120 Võ Van T?n, Phu?ng Võ Th? Sáu, Qu?n 3, TP. H? Chí Minh', N'Tr?n Th? B', N'B?c', N'0918877665', 50),
(3, N'15 Lý Thu?ng Ki?t, Phu?ng 14, Qu?n 10, TP. H? Chí Minh', N'Lê Van C', N'Chu?n', N'0983344556', 10),
(4, N'12 Nguy?n Hu?, Phu?ng B?n Nghé, Qu?n 1, TP. H? Chí Minh', N'Ph?m Minh Tu?n', N'Vàng', N'0905123456', 180),
(5, N'45 Lê L?i, Phu?ng B?n Thành, Qu?n 1, TP. H? Chí Minh', N'Hoàng Th? Lan', N'B?c', N'0912345678', 75),
(6, N'8 Tr?n Hung Ð?o, Phu?ng 6, Qu?n 5, TP. H? Chí Minh', N'Võ Qu?c B?o', N'Chu?n', N'0937654321', 20),
(7, N'102 Ði?n Biên Ph?, Phu?ng 17, Qu?n Bình Th?nh, TP. H? Chí Minh', N'Ð?ng Thu Hà', N'Kim cuong', N'0944556677', 520),
(8, N'36 Nguy?n Th? Th?p, Phu?ng Tân Quy, Qu?n 7, TP. H? Chí Minh', N'Bùi Anh Khoa', N'Chu?n', N'0966778899', 5),
(9, N'27 Phan Van Tr?, Phu?ng 10, Qu?n Gò V?p, TP. H? Chí Minh', N'Ngô Thanh Mai', N'B?c', N'0977889900', 90),
(10, N'63 Cách M?ng Tháng 8, Phu?ng 12, Qu?n 10, TP. H? Chí Minh', N'Ð? Hoài Nam', N'Vàng', N'0988990011', 210),
(11, N'210 Quang Trung, Phu?ng 10, Qu?n Gò V?p, TP. H? Chí Minh', N'Lý Gia Hân', N'Chu?n', N'0399123456', 0),
(12, N'5 Võ Van Ngân, Phu?ng Linh Chi?u, TP. Th? Ð?c, TP. H? Chí Minh', N'Truong Qu?c Vi?t', N'B?c', N'0868234567', 60),
(13, N'88 Nguy?n Oanh, Phu?ng 17, Qu?n Gò V?p, TP. H? Chí Minh', N'Nguy?n Minh Khôi', N'Vàng', N'0908765432', 230),
(14, N'156 Hoàng Di?u, Phu?ng 6, Qu?n 4, TP. H? Chí Minh', N'Phan Ng?c Anh', N'B?c', N'0938123456', 85),
(15, N'72 Kha V?n Cân, Phu?ng Hi?p Bình Chánh, TP. Th? Ð?c, TP. H? Chí Minh', N'Ðinh Thanh Tùng', N'Chu?n', N'0976234518', 15);
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
VALUES (1, N'15 Nguy?n Van C?, Phu?ng 4, TP. Ðà L?t, Lâm Ð?ng', N'lienhe@nongsandalat.vn', N'02633812345', N'Công ty TNHH Nông s?n s?ch Ðà L?t'),
(2, N'125 Nguy?n H?u Th?, Phu?ng Tân Phong, Qu?n 7, TP. H? Chí Minh', N'sales@freshviet.vn', N'02838123456', N'Công ty CP Trái cây nh?p kh?u Fresh Vi?t'),
(3, N'86 Ði?n Biên Ph?, Phu?ng 17, Qu?n Bình Th?nh, TP. H? Chí Minh', N'info@eatcleansg.vn', N'02838234567', N'Công ty TNHH Th?c ph?m Eat Clean Sài Gòn'),
(4, N'42 Tru?ng Chinh, Phu?ng 12, Qu?n Tân Bình, TP. H? Chí Minh', N'order@ngucocmyviet.vn', N'02838345678', N'Công ty TNHH Ngu c?c & H?t M? Vi?t'),
(5, N'28 Tr?n Thái Tông, Phu?ng D?ch V?ng, Qu?n C?u Gi?y, Hà N?i', N'cskh@suaxanh.vn', N'02438456789', N'Công ty CP S?a & Ch? ph?m s?a Xanh'),
(6, N'72 Nguy?n Thái Bình, Phu?ng Nguy?n Thái Bình, Qu?n 1, TP. H? Chí Minh', N'contact@diatrunghai.vn', N'02838567890', N'Công ty TNHH D?u an Ð?a Trung H?i'),
(7, N'18 Nguy?n T?t Thành, Th? tr?n U Minh, Huy?n U Minh, Cà Mau', N'htx@matonguminh.vn', N'02916678901', N'HTX M?t ong U Minh'),
(8, N'35 Phan Ðang Luu, Phu?ng 3, Qu?n Phú Nhu?n, TP. H? Chí Minh', N'sales@haisannauy.vn', N'02838789012', N'Công ty TNHH H?i s?n Na Uy Vi?t Nam'),
(9, N'125 Ðu?ng T?nh 825, Xã Ð?c Hòa H?, Huy?n Ð?c Hòa, Long An', N'trangtrai@trungsachla.vn', N'02723890123', N'Trang tr?i Tr?ng s?ch Long An'),
(10, N'56 Tr?n Phú, Phu?ng L?c Ti?n, TP. B?o L?c, Lâm Ð?ng', N'info@dalatgreen.vn', N'02633901234', N'Công ty TNHH Trà th?o m?c Dalat Green'),
(11, N'210 Võ Van Ngân, Phu?ng Bình Th?, TP. Th? Ð?c, TP. H? Chí Minh', N'ban-hang@snacklanhmanh.vn', N'02838012345', N'Công ty CP Bánh & Snack lành m?nh'),
(12, N'89 Quang Trung, Phu?ng 10, Qu?n Gò V?p, TP. H? Chí Minh', N'lienhe@superfoodxanh.vn', N'02838123400', N'Công ty TNHH Ð?u hu & Superfood Xanh'),
(13, N'145 Nguy?n Van C?, Phu?ng An Khánh, Qu?n Ninh Ki?u, C?n Tho', N'contact@rauhuucomekong.vn', N'02923678901', N'Công ty TNHH Rau c? h?u co Mekong'),
(14, N'320 Nguy?n Van Quá, Phu?ng Ðông Hung Thu?n, Qu?n 12, TP. H? Chí Minh', N'sales@vietnature.vn', N'02839876543', N'Công ty CP Th?c ph?m xanh Vi?t Nature'),
(15, N'78 Ð?i l? Bình Duong, Phu?ng Phú Hòa, TP. Th? D?u M?t, Bình Duong', N'info@nongnsanviet.vn', N'02743781234', N'Công ty TNHH Nông s?n h?u co An Vi?t');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SupplierId', N'Address', N'Email', N'PhoneNumber', N'SupplierName') AND [object_id] = OBJECT_ID(N'[Suppliers]'))
    SET IDENTITY_INSERT [Suppliers] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ProductId', N'Barcode', N'BrandId', N'CategoryId', N'CostPrice', N'IsActive', N'Price', N'ProductName', N'StockQuantity', N'SupplierId', N'Unit') AND [object_id] = OBJECT_ID(N'[Products]'))
    SET IDENTITY_INSERT [Products] ON;
INSERT INTO [Products] ([ProductId], [Barcode], [BrandId], [CategoryId], [CostPrice], [IsActive], [Price], [ProductName], [StockQuantity], [SupplierId], [Unit])
VALUES (1, N'8938501234561', 7, 1, 18000.0, CAST(1 AS bit), 25000.0, N'Rau c?i bó xôi h?u co 300g', 50, 1, N'gói'),
(2, N'8938501234562', 7, 1, 24000.0, CAST(1 AS bit), 32000.0, N'Cà chua bi h?u co 500g', 40, 1, N'h?p'),
(3, N'8938501234563', 2, 2, 50000.0, CAST(1 AS bit), 65000.0, N'Y?n m?ch nguyên h?t Quaker 500g', 30, 4, N'gói'),
(4, N'8938501234564', 10, 2, 72000.0, CAST(1 AS bit), 95000.0, N'H?nh nhân rang Blue Diamond 250g', 25, 4, N'gói'),
(5, N'8938501234565', 5, 3, 60000.0, CAST(1 AS bit), 78000.0, N'S?a h?nh nhân Alsafi 946ml', 35, 5, N'h?p'),
(6, N'8938501234566', 3, 3, 41000.0, CAST(1 AS bit), 55000.0, N'S?a chua TH true MILK không du?ng 500g', 20, 5, N'h?p'),
(7, N'8938501234567', 8, 4, 52000.0, CAST(1 AS bit), 69000.0, N'?c gà Eat Clean 500g', 60, 3, N'h?p'),
(8, N'8938501234568', 8, 4, 31000.0, CAST(1 AS bit), 42000.0, N'Com g?o l?t Eat Clean 250g', 45, 3, N'h?p'),
(9, N'8938501234569', 6, 5, 110000.0, CAST(1 AS bit), 145000.0, N'D?u oliu Bertolli Extra Virgin 500ml', 20, 6, N'chai'),
(10, N'8938501234570', 11, 5, 88000.0, CAST(1 AS bit), 120000.0, N'M?t ong nguyên ch?t Bee Gold 500ml', 15, 7, N'chai'),
(11, N'8938501234571', 1, 6, 98000.0, CAST(1 AS bit), 129000.0, N'Táo Envy nh?p kh?u 1kg', 30, 2, N'kg'),
(12, N'8938501234572', 1, 7, 135000.0, CAST(1 AS bit), 169000.0, N'Cá h?i phi lê Na Uy 300g', 22, 8, N'gói'),
(13, N'8938501234573', 7, 8, 40000.0, CAST(1 AS bit), 52000.0, N'Tr?ng gà ta h?u co 10 qu?', 70, 9, N'h?p'),
(14, N'8938501234574', 9, 9, 34000.0, CAST(1 AS bit), 48000.0, N'Trà hoa cúc Dalat Green 20 túi l?c', 55, 10, N'h?p'),
(15, N'8938501234575', 8, 10, 28000.0, CAST(1 AS bit), 39000.0, N'Bánh quy y?n m?ch Eat Clean 200g', 65, 11, N'gói'),
(16, N'8938501234576', 7, 11, 12000.0, CAST(1 AS bit), 18000.0, N'Ð?u hu non h?u co Organic Farm VN 300g', 80, 12, N'h?p'),
(17, N'8938501234577', 12, 12, 100000.0, CAST(1 AS bit), 135000.0, N'B?t Spirulina Nutri Seed 100g', 18, 12, N'hu');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ProductId', N'Barcode', N'BrandId', N'CategoryId', N'CostPrice', N'IsActive', N'Price', N'ProductName', N'StockQuantity', N'SupplierId', N'Unit') AND [object_id] = OBJECT_ID(N'[Products]'))
    SET IDENTITY_INSERT [Products] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserId', N'CreatedAt', N'Email', N'FullName', N'IsActive', N'PasswordHash', N'Phone', N'RoleId', N'Username') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([UserId], [CreatedAt], [Email], [FullName], [IsActive], [PasswordHash], [Phone], [RoleId], [Username])
VALUES (1, '2026-01-05T08:00:00.0000000', N'admin@healthymart.vn', N'Qu?n tr? h? th?ng', CAST(1 AS bit), N'$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.', N'0900000001', 1, N'admin'),
(2, '2026-01-06T08:00:00.0000000', N'warehouse01@healthymart.vn', N'Nguy?n Th? Mai', CAST(1 AS bit), N'$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.', N'0900000002', 2, N'warehouse01'),
(3, '2026-01-06T08:30:00.0000000', N'warehouse02@healthymart.vn', N'Tr?n Van Ði?u', CAST(1 AS bit), N'$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.', N'0900000003', 2, N'warehouse02'),
(4, '2026-02-01T08:00:00.0000000', N'cashier01@healthymart.vn', N'Lê Th? Thu Ngân', CAST(1 AS bit), N'$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.', N'0900000004', 3, N'cashier01'),
(5, '2026-02-01T08:10:00.0000000', N'cashier02@healthymart.vn', N'Ph?m Van Ngân', CAST(1 AS bit), N'$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.', N'0900000005', 3, N'cashier02'),
(6, '2026-02-01T08:20:00.0000000', N'cashier03@healthymart.vn', N'Hoàng Th? Ngân', CAST(1 AS bit), N'$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.', N'0900000006', 3, N'cashier03'),
(7, '2026-02-10T08:00:00.0000000', N'cashier04@healthymart.vn', N'Võ Minh Ngân', CAST(1 AS bit), N'$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.', N'0900000007', 3, N'cashier04'),
(8, '2026-02-10T08:10:00.0000000', N'cashier05@healthymart.vn', N'Ð?ng Th?Ngân', CAST(1 AS bit), N'$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.', N'0900000008', 3, N'cashier05'),
(9, '2026-03-01T08:00:00.0000000', N'cashier06@healthymart.vn', N'Bùi Van Ngân', CAST(1 AS bit), N'$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.', N'0900000009', 3, N'cashier06'),
(10, '2026-03-01T08:10:00.0000000', N'cashier07@healthymart.vn', N'Ngô Th? Thu Ngân', CAST(1 AS bit), N'$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.', N'0900000010', 3, N'cashier07'),
(11, '2026-04-01T08:00:00.0000000', N'cashier08@healthymart.vn', N'Ð? Van Ngân', CAST(1 AS bit), N'$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.', N'0900000011', 3, N'cashier08'),
(12, '2026-04-01T08:10:00.0000000', N'cashier09@healthymart.vn', N'Lý Th? Thu Ngân', CAST(0 AS bit), N'$2b$11$Kce6ZlNDqAZeWGjWjezVDuvhT5YGuqyxFMjEc6g8Zx1VbYe0Rq7B.', N'0900000012', 3, N'cashier09');
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
    WHERE UPPER(r.RoleName) IN ('CASHIER', 'ADMIN') OR o.OrderId IS NOT NULL
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
