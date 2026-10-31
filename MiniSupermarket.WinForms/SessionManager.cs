using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public static class SessionManager
    {
        public static string JwtToken { get; set; } = string.Empty;
        public static string CurrentRole { get; set; } = string.Empty;
        public static string CurrentUsername { get; set; } = string.Empty;

        public static void Logout()
        {
            JwtToken = string.Empty;
            CurrentRole = string.Empty;
            CurrentUsername = string.Empty;
            DataCache.Clear();
            if (ApiClientService.RawClient != null)
            {
                ApiClientService.RawClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public static class DataCache
        {
            public static System.Collections.Generic.List<CategoryDto> Categories { get; set; }
            public static System.Collections.Generic.List<BrandDto> Brands { get; set; }
            public static System.Collections.Generic.List<SupplierDto> Suppliers { get; set; }
            public static System.Collections.Generic.List<ProductDto> ActiveProducts { get; set; }

            public static void Clear()
            {
                Categories = null;
                Brands = null;
                Suppliers = null;
                ActiveProducts = null;
            }
        }

        public static class ApiClientService
        {
            private static readonly HttpClientHandler _handler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
            private static readonly HttpClient _client = new HttpClient(_handler)
            {
                BaseAddress = new Uri("https://localhost:7054/api/")
            };

            public static HttpClient RawClient => _client;

            // 👉 Property Client công khai: Tự động gắn Bearer Token nếu đã đăng nhập
            public static HttpClient Client
            {
                get
                {
                    if (!string.IsNullOrEmpty(SessionManager.JwtToken))
                    {
                        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SessionManager.JwtToken);
                    }
                    else
                    {
                        _client.DefaultRequestHeaders.Authorization = null;
                    }
                    return _client;
                }
            }

            // Hàm gọi API đăng nhập lấy Token
            public static async Task<bool> LoginAsync(string username, string password)
            {
                try
                {
                    var loginObj = new { Username = username, Password = password };
                    var response = await _client.PostAsJsonAsync("auth/login", loginObj);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        var doc = JsonDocument.Parse(jsonString);
                        SessionManager.JwtToken = doc.RootElement.GetProperty("token").GetString() ?? string.Empty;
                        SessionManager.CurrentRole = doc.RootElement.GetProperty("role").GetString() ?? string.Empty;
                        SessionManager.CurrentUsername = username;
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể kết nối đến máy chủ API (https://localhost:7054)!\n\nChi tiết lỗi: " + ex.Message, "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            // Hiển thị thông báo lỗi API chuẩn hóa bằng tiếng Việt
            public static async Task ShowApiErrorAsync(HttpResponseMessage response, string actionTitle = "Thao tác")
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    MessageBox.Show("Phiên làm việc đã hết hạn hoặc chưa đăng nhập. Vui lòng đăng nhập lại!", "Yêu cầu đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    MessageBox.Show($"Bạn không có quyền thực hiện {actionTitle.ToLower()}!\n(Vai trò hiện tại: {SessionManager.CurrentRole})", "Từ chối quyền hạn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string content = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"{actionTitle} không thành công!\nMã lỗi: {(int)response.StatusCode} ({response.StatusCode})\n\n{content}", "Thông báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
