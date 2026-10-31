using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MiniSupermarket.API.Models
{
    [Table("Products")]
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Mã v?ch s?n ph?m không ???c trùng")]
        [StringLength(50)]
        public string Barcode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên s?n ph?m không ???c trùng")]
        [StringLength(150)]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(20)]
        public string Unit { get; set; } = string.Empty;   // kg, gói, h?p

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá bán ph?i >= 0")]
        public decimal Price { get; set; }                 // Giá bán

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPrice { get; set; }             // Giá nh?p

        [Range(0, int.MaxValue, ErrorMessage = "T?n kho ph?i >= 0")]
        public int StockQuantity { get; set; }             // T?n kho

        public bool IsActive { get; set; } = true;         // Ng?ng kinh doanh thì false

        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public int BrandId { get; set; }
        [ForeignKey("BrandId")]
        public virtual Brand? Brand { get; set; }
        public int? SupplierId { get; set; }               // nullable: s?n ph?m chua có NCC v?n luu du?c
        [ForeignKey("SupplierId")]
        [JsonIgnore] // Tránh l?i l?p vòng vô t?n khi serialize JSON
        public virtual Supplier? Supplier { get; set; }
    }
}
