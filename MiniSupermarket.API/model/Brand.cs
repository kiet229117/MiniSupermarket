using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MiniSupermarket.API.Models
{
    [Table("Brands")]
    public class Brand
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BrandId { get; set; }

        [Required(ErrorMessage = "Tên thương hiệu không được để trống")]
        [StringLength(100)]
        public string BrandName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Country { get; set; }               // Quốc gia xuất xứ

        [StringLength(255)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        [JsonIgnore] // Tránh lỗi lặp vòng vô tận khi serialize JSON
        // Một thương hiệu có nhiều sản phẩm
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}