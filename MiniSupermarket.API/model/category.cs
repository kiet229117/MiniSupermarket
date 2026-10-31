<<<<<<< HEAD
﻿namespace MiniSupermarket.API.Models
{
    // Lớp biểu diễn thực thể Nhóm hàng hóa trong siêu thị mini
    public class Category
    {
        // Mã định danh nhóm hàng (Khóa chính)
        public int CategoryId { get; set; }

        // Tên nhóm hàng (Bắt buộc, không được để trống)
        public string CategoryName { get; set; } = string.Empty;

        // Mô tả chi tiết về nhóm hàng (Có thể để trống)
        public string? Description { get; set; }
=======
﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MiniSupermarket.API.Models
{
    [Table("Categories")] // Đặt tên bảng rõ ràng trong SQL Server
    public class Category
    {
        [Key] // Khóa chính
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Tự động tăng (Identity 1,1)
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên nhóm hàng không được để trống!")]
        [StringLength(100, ErrorMessage = "Tên nhóm hàng không vượt quá 100 ký tự")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        // Quan hệ 1 - N: Một danh mục có nhiều sản phẩm
        [JsonIgnore] // Tránh lỗi lặp vòng vô tận khi serialize JSON
        public virtual ICollection<Product>? Products { get; set; }
>>>>>>> buoi-5
    }
}
