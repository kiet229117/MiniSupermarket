using System;

using System.Net.Http;
using System.Net.Http.Json; 

using System.Text.Json; 

using System.Windows.Forms;
using MiniSupermarket.WinForms;

namespace MiniSupermarket.WinForms
{
    public partial class Form1 : Form
    {
        // Khởi tạo HttpClient trỏ đến địa chỉ của Web API Backend
        private static readonly HttpClient _client = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7054/api/")
        };

        public Form1()
        {
            InitializeComponent();
        }

        // Sự kiện khi người dùng bấm nút Đăng nhập
        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUser.Text.Trim();
            string password = txtPassword.Text.Trim();

            // Kiểm tra ràng buộc cơ bản phía Client
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tài khoản và mật khẩu!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Đóng gói dữ liệu gửi lên endpoint POST /api/auth/login
                var loginData = new { Username = username, Password = password };
                var response = await _client.PostAsJsonAsync("auth/login", loginData);

                if (response.IsSuccessStatusCode)
                {
                    // Đọc chuỗi JSON trả về từ Server khi đăng nhập thành công
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var doc = JsonDocument.Parse(jsonString);

                    // Trích xuất Token và Role lưu vào lớp tĩnh SessionManager dùng chung toàn ứng dụng
                    SessionManager.JwtToken = doc.RootElement.GetProperty("token").GetString() ?? string.Empty;
                    SessionManager.CurrentRole = doc.RootElement.GetProperty("role").GetString() ?? string.Empty;

                    MessageBox.Show($"Đăng nhập thành công với quyền: {SessionManager.CurrentRole}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Mở Form quản lý chính (FormCategoryManagement) và ẩn Form đăng nhập đi
                    FormCategoryManagement mainForm = new FormCategoryManagement();
                    this.Hide();
                    mainForm.ShowDialog();
                    this.Close(); // Đóng hẳn ứng dụng khi form chính tắt
                }
                else
                {
                    MessageBox.Show("Sai tài khoản hoặc mật khẩu!", "Đăng nhập thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối đến Server: " + ex.Message, "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
