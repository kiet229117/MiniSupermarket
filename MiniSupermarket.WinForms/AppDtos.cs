using System;
using System.Collections.Generic;

namespace MiniSupermarket.WinForms
{
    // ==========================================
    // CÁC DTO DÙNG CHUNG TOÀN ỨNG DỤNG WINFORMS
    // ==========================================

    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }

        // Các khóa ngoại và tên để hiển thị
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty; // Lấy từ bảng Category

        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;     // Lấy từ bảng Brand

        public int? SupplierId { get; set; }
        public string SupplierName { get; set; }                 // (Tuỳ chọn) Lấy từ Supplier nếu cần
    }

    public class CustomerDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int RewardPoints { get; set; }
        public string MembershipRank { get; set; } = "Chuẩn";
    }

    public class UserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; } = "CASHIER";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }

    public class CartItemDto
    {
        public int ProductId { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Unit { get; set; } = "cái";
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
    }

    public class ReportSummaryDto
    {
        public string Date { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public string BestSeller { get; set; } = string.Empty;
        public List<RecentOrderDto> RecentOrders { get; set; } = new List<RecentOrderDto>();
    }

    public class RecentOrderDto
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
    }

    public class BrandDto
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class SupplierDto
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class OrderHeaderDto
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public string CustomerName { get; set; }
        public string CashierName { get; set; }
        public int ItemCount { get; set; }
    }

    public class OrderDetailDto
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public int? CustomerId { get; set; }
        public int? CashierId { get; set; }
        public string CustomerName { get; set; }
        public string CashierName { get; set; }
        public List<OrderItemDetailDto> Items { get; set; } = new List<OrderItemDetailDto>();
    }

    public class OrderItemDetailDto
    {
        public int OrderItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    // ==========================================
    // DTOs BÃO CÃO & DASHBOARD (STORED PROCEDURES)
    // ==========================================
    public class TopSellingProductItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int TotalSoldQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TopSellingProductsResponseDto
    {
        public bool Success { get; set; }
        public string GeneratedAt { get; set; }
        public int TotalCount { get; set; }
        public List<TopSellingProductItemDto> Data { get; set; } = new List<TopSellingProductItemDto>();
    }

    public class CashierRevenueItemDto
    {
        public string CashierUsername { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalPointsAwarded { get; set; }
    }

    public class CashierRevenueResponseDto
    {
        public bool Success { get; set; }
        public string GeneratedAt { get; set; }
        public int TotalCashiers { get; set; }
        public decimal GrandTotalRevenue { get; set; }
        public List<CashierRevenueItemDto> Data { get; set; } = new List<CashierRevenueItemDto>();
    }

    // ==========================================
    // DTOs IN HÓA ĐƠN & BIÊN LAI (RECEIPT PRINTING)
    // ==========================================
    public class InvoiceReceiptModel
    {
        public string StoreName { get; set; } = "MINI SUPERMARKET";
        public string StoreAddress { get; set; } = "123 Đường Nguyễn Huệ, P. Bến Nghé, Quận 1, TP. HCM";
        public string StoreHotline { get; set; } = "Hotline: 1900 1234 - 0987 654 321";
        public string OrderCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CashierName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = "Khách vãng lai";
        public string CustomerPhone { get; set; } = string.Empty;
        public int EarnedPoints { get; set; }
        public string PaymentMethod { get; set; } = "Tiền mặt";
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public decimal CashGiven { get; set; }
        public decimal ChangeDue { get; set; }
        public List<InvoiceItemModel> Items { get; set; } = new List<InvoiceItemModel>();
    }

    public class InvoiceItemModel
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Unit { get; set; } = "cái";
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}