using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MiniSupermarket.WinForms.SessionManager;

namespace MiniSupermarket.WinForms
{
    public partial class FormCategoryManagement : Form
    {
        // =====================================================
        // API
        // =====================================================



        // =====================================================
        // DANH SÁCH NHÓM HÀNG
        // =====================================================

        private List<ApiCategory> categories =
            new List<ApiCategory>();

        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        public FormCategoryManagement()
        {
            InitializeComponent();

           
        }

        // =====================================================
        // FORM LOAD
        // =====================================================

        private async void FormCategoryManagement_Load_1(
           object sender,
           EventArgs e)
        {
            UITheme.ApplyFormTheme(this);
            UITheme.ApplyGridTheme(dataGridView1);
            UITheme.ApplyButtonTheme(btnSearch, UITheme.ButtonStyle.Primary);
            UITheme.ApplyButtonTheme(btnReload, UITheme.ButtonStyle.Secondary);
            UITheme.ApplyButtonTheme(btnAdd, UITheme.ButtonStyle.Success);
            UITheme.ApplyButtonTheme(btnUpdate, UITheme.ButtonStyle.Warning);
            UITheme.ApplyButtonTheme(btnDelete, UITheme.ButtonStyle.Danger);

            txtSearch.Text = "Nhập từ khóa...";
            txtSearch.ForeColor = Color.Gray;

            lblStatus.Text = "Đang tải dữ liệu...";

            SetPermission();

            await LoadDataAsync();
        }

        // =====================================================
        // PHÂN QUYỀN
        // =====================================================

        private void SetPermission()
        {
            if (SessionManager.CurrentRole == "Admin")
            {
                btnAdd.Enabled = true;
                btnUpdate.Enabled = true;
                btnDelete.Enabled = true;
            }
            else
            {
                btnAdd.Enabled = false;
                btnUpdate.Enabled = false;
                btnDelete.Enabled = false;
            }
        }

 

      

        // =====================================================
        // GET ALL
        // GET /api/Categories
        // =====================================================

        private async Task LoadDataAsync()
        {
            try
            {
                var response =
                    await SessionManager.ApiClientService.Client
                        .GetAsync("Categories");

                if (!response.IsSuccessStatusCode)
                {
                    string error =
                        await response.Content.ReadAsStringAsync();

                    MessageBox.Show(
                        $"Không lấy được dữ liệu từ API!\n\n" +
                        $"HTTP: {(int)response.StatusCode}\n\n" +
                        error,
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                var apiCategories =
                    await response.Content
                        .ReadFromJsonAsync<List<ApiCategory>>();

                categories.Clear();

                if (apiCategories != null)
                {
                    categories.AddRange(apiCategories);
                }

                DisplayCategories(categories);

                SelectFirstRow();

                lblStatus.Text =
                    "Có " + categories.Count + " nhóm hàng";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi kết nối API:\n\n" + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =====================================================
        // HIỂN THỊ DANH SÁCH
        // =====================================================

        private void DisplayCategories(
            List<ApiCategory> list)
        {
            dataGridView1.Rows.Clear();

            foreach (ApiCategory category in list)
            {
                int rowIndex =
                    dataGridView1.Rows.Add();

                DataGridViewRow row =
                    dataGridView1.Rows[rowIndex];

                row.Cells[0].Value =
                    category.CategoryId;

                row.Cells[1].Value =
                    category.CategoryName;

                row.Cells[2].Value =
                    category.Description;
            }

            lblStatus.Text =
                "Có " + list.Count + " nhóm hàng";
        }

        // =====================================================
        // CLICK VÀO DÒNG
        // =====================================================

        private void dataGridView1_CellClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            ShowCategory(e.RowIndex);
        }

        // =====================================================
        // HIỂN THỊ CHI TIẾT
        // =====================================================

        private void ShowCategory(
            int rowIndex)
        {
            if (rowIndex < 0 ||
                rowIndex >= dataGridView1.Rows.Count)
            {
                return;
            }

            DataGridViewRow row =
                dataGridView1.Rows[rowIndex];

            txtID.Text =
                Convert.ToString(
                    row.Cells[0].Value);

            txtName.Text =
                Convert.ToString(
                    row.Cells[1].Value);

            txtDescription.Text =
                Convert.ToString(
                    row.Cells[2].Value);
        }

        // =====================================================
        // CHỌN DÒNG ĐẦU TIÊN
        // =====================================================

        private void SelectFirstRow()
        {
            if (dataGridView1.Rows.Count > 0)
            {
                dataGridView1.ClearSelection();

                dataGridView1.Rows[0].Selected =
                    true;

                dataGridView1.CurrentCell =
                    dataGridView1.Rows[0].Cells[0];

                ShowCategory(0);
            }
            else
            {
                ClearInput();
            }
        }

        // =====================================================
        // XÓA Ô NHẬP
        // =====================================================

        private void ClearInput()
        {
            txtID.Clear();
            txtName.Clear();
            txtDescription.Clear();
        }

        // =====================================================
        // TÌM KIẾM
        // GET /api/Categories/search?keyword=
        // =====================================================

        private async void btnSearch_Click(
    object sender,
    EventArgs e)
        {
            string keyword =
                txtSearch.Text.Trim();

            if (string.IsNullOrWhiteSpace(keyword) ||
                keyword == "Nhập từ khóa...")
            {
                await LoadDataAsync();
                return;
            }

            try
            {
                string url =
                    "Categories/search?keyword=" +
                    Uri.EscapeDataString(keyword);

                var response =
                    await SessionManager.ApiClientService.Client
                        .GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    string error =
                        await response.Content.ReadAsStringAsync();

                    MessageBox.Show(
                        $"Tìm kiếm thất bại!\n\n" +
                        $"HTTP: {(int)response.StatusCode}\n\n" +
                        error,
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                var result =
                    await response.Content
                        .ReadFromJsonAsync<List<ApiCategory>>();

                if (result == null)
                {
                    result = new List<ApiCategory>();
                }

                DisplayCategories(result);

                if (result.Count > 0)
                {
                    dataGridView1.ClearSelection();

                    dataGridView1.Rows[0].Selected = true;

                    dataGridView1.CurrentCell =
                        dataGridView1.Rows[0].Cells[0];

                    ShowCategory(0);

                    lblStatus.Text =
                        "Tìm thấy " +
                        result.Count +
                        " nhóm hàng";
                }
                else
                {
                    ClearInput();

                    lblStatus.Text =
                        "Không tìm thấy";

                    MessageBox.Show(
                        "Không tìm thấy nhóm hàng!",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi tìm kiếm:\n\n" + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =====================================================
        // TẢI LẠI
        // =====================================================

        private async void btnReload_Click(
            object sender,
            EventArgs e)
        {
            txtSearch.Text =
                "Nhập từ khóa...";

            txtSearch.ForeColor =
                Color.Gray;

            ClearInput();

            await LoadDataAsync();

            lblStatus.Text =
                "Đã tải lại dữ liệu";
        }

        // =====================================================
        // THÊM MỚI
        // POST /api/Categories
        // =====================================================

        private async void btnAdd_Click(
     object sender,
     EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show(
                    "Vui lòng nhập Tên Nhóm hàng!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtName.Focus();
                return;
            }

            try
            {
                ApiCategory category =
                    new ApiCategory
                    {
                        CategoryName =
                            txtName.Text.Trim(),

                        Description =
                            txtDescription.Text.Trim()
                    };

                var response =
                    await SessionManager.ApiClientService.Client
                        .PostAsJsonAsync(
                            "Categories",
                            category);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "Thêm mới thành công!",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    ClearInput();

                    await LoadDataAsync();
                }
                else if (
                    response.StatusCode ==
                    System.Net.HttpStatusCode.Forbidden)
                {
                    MessageBox.Show(
                        "Chỉ Admin mới được thêm nhóm hàng!",
                        "Không có quyền",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                else
                {
                    string error =
                        await response.Content.ReadAsStringAsync();

                    MessageBox.Show(
                        "Thêm thất bại!\n\n" + error,
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi khi thêm:\n\n" + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =====================================================
        // CẬP NHẬT
        // PUT /api/Categories/{id}
        // =====================================================

        private async void btnUpdate_Click(
    object sender,
    EventArgs e)
        {
            if (!int.TryParse(txtID.Text, out int id))
            {
                MessageBox.Show(
                    "Vui lòng chọn nhóm hàng cần cập nhật!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show(
                    "Vui lòng nhập Tên Nhóm hàng!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtName.Focus();
                return;
            }

            try
            {
                ApiCategory category =
                    new ApiCategory
                    {
                        CategoryId = id,

                        CategoryName =
                            txtName.Text.Trim(),

                        Description =
                            txtDescription.Text.Trim()
                    };

                var response =
                    await SessionManager.ApiClientService.Client
                        .PutAsJsonAsync(
                            $"Categories/{id}",
                            category);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "Cập nhật thành công!",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    await LoadDataAsync();

                    SelectCategoryByID(id);
                }
                else if (
                    response.StatusCode ==
                    System.Net.HttpStatusCode.Forbidden)
                {
                    MessageBox.Show(
                        "Chỉ Admin mới được cập nhật nhóm hàng!",
                        "Không có quyền",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                else
                {
                    string error =
                        await response.Content.ReadAsStringAsync();

                    MessageBox.Show(
                        "Cập nhật thất bại!\n\n" + error,
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi khi cập nhật:\n\n" + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =====================================================
        // XÓA
        // DELETE /api/Categories/{id}
        // =====================================================

        private async void btnDelete_Click(
     object sender,
     EventArgs e)
        {
            if (!int.TryParse(txtID.Text, out int id))
            {
                MessageBox.Show(
                    "Vui lòng chọn nhóm hàng cần xóa!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            DialogResult result =
                MessageBox.Show(
                    "Bạn có chắc muốn xóa nhóm hàng \"" +
                    txtName.Text +
                    "\" không?",

                    "Xác nhận xóa",

                    MessageBoxButtons.YesNo,

                    MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                var response =
                    await SessionManager.ApiClientService.Client
                        .DeleteAsync(
                            $"Categories/{id}");

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "Xóa thành công!",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    ClearInput();

                    await LoadDataAsync();
                }
                else if (
                    response.StatusCode ==
                    System.Net.HttpStatusCode.Forbidden)
                {
                    MessageBox.Show(
                        "Chỉ Admin mới được xóa nhóm hàng!",
                        "Không có quyền",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                else
                {
                    string error =
                        await response.Content.ReadAsStringAsync();

                    string displayMessage = error;
                    try
                    {
                        using (var doc = System.Text.Json.JsonDocument.Parse(error))
                        {
                            if (doc.RootElement.TryGetProperty("message", out var msgProp))
                            {
                                displayMessage = msgProp.GetString();
                            }
                        }
                    }
                    catch
                    {
                        // Giữ nguyên chuỗi nếu không phải JSON
                    }

                    MessageBox.Show(
                        displayMessage,
                        "Không thể xóa nhóm hàng",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi khi xóa:\n\n" + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =====================================================
        // CHỌN NHÓM HÀNG THEO ID
        // =====================================================

        private void SelectCategoryByID(
            int id)
        {
            for (int i = 0;
                 i < dataGridView1.Rows.Count;
                 i++)
            {
                string currentID =
                    Convert.ToString(
                        dataGridView1.Rows[i]
                            .Cells[0].Value);

                if (currentID ==
                    id.ToString())
                {
                    dataGridView1.ClearSelection();

                    dataGridView1.Rows[i].Selected =
                        true;

                    dataGridView1.CurrentCell =
                        dataGridView1.Rows[i].Cells[0];

                    ShowCategory(i);

                    return;
                }
            }
        }

        // =====================================================
        // PLACEHOLDER - ENTER
        // =====================================================

        private void txtSearch_Enter(
            object sender,
            EventArgs e)
        {
            if (txtSearch.Text ==
                "Nhập từ khóa...")
            {
                txtSearch.Clear();

                txtSearch.ForeColor =
                    Color.Black;
            }
        }

        // =====================================================
        // PLACEHOLDER - LEAVE
        // =====================================================

        private void txtSearch_Leave(
            object sender,
            EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(
                txtSearch.Text))
            {
                txtSearch.Text =
                    "Nhập từ khóa...";

                txtSearch.ForeColor =
                    Color.Gray;
            }
        }

        // =====================================================
        // MODEL CATEGORY
        // =====================================================

        public class ApiCategory
        {
            public int CategoryId { get; set; }

            public string CategoryName { get; set; }
                = string.Empty;

            public string Description { get; set; }
                = string.Empty;
        }
    }
}