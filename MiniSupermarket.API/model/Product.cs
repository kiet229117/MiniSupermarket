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

        [Required(ErrorMessage = "Mã vạch sản phẩm không được trống")]
        [StringLength(50)]
        public string Barcode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [StringLength(150)]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(20)]
        public string Unit { get; set; } = string.Empty;   // kg, gói, hộp

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải >= 0")]
        public decimal Price { get; set; }                 // Giá bán

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPrice { get; set; }             // Giá nhập

        [Range(0, int.MaxValue, ErrorMessage = "Tồn kho phải >= 0")]
        public int StockQuantity { get; set; }             // Tồn kho

        public bool IsActive { get; set; } = true;         // Ngừng kinh doanh thì false

        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public int BrandId { get; set; }
        [ForeignKey("BrandId")]
        public virtual Brand? Brand { get; set; }
        public int? SupplierId { get; set; }               // nullable: sản phẩm chưa có NCC vẫn lưu được
        [ForeignKey("SupplierId")]
        [JsonIgnore] // Tránh lỗi lặp vòng vô tận khi serialize JSON
        public virtual Supplier? Supplier { get; set; }
    }
}
