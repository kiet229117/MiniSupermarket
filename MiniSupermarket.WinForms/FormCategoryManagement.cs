using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public partial class FormCategoryManagement : Form
    {
        private const string SearchPlaceholder = "Nhập tên nhóm hàng...";
        private readonly List<CategoryDto> _categories = new List<CategoryDto>();

        // Controls giao diện
        private Panel panelTop;
                private Label lblStatus;
        private PaginationControl<CategoryDto> pager;
        private Label lblSearch;
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnReload;

        private DataGridView dgvCategories;

        private Panel panelRight;
        private GroupBox grbDetails;
        private Label lblId;
        private TextBox txtId;
        private Label lblName;
        private TextBox txtName;
        private Label lblDescription;
        private TextBox txtDescription;

        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnClear;

        public FormCategoryManagement()
        {
            InitializeComponent();
            BuildUI();
            ApplyTheme();
            this.Load += FormCategoryManagement_Load;
        }

        private void BuildUI()
        {
           

            // 1. Panel Top (Tiêu đề và Tìm kiếm)
            panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Color.White
            };

            lblStatus = new Label
            {
                Text = "Sẵn sàng",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.TextMuted,
                Location = new Point(18, 17),
                AutoSize = true
            };
            panelTop.Controls.Add(lblStatus);

            FlowLayoutPanel panelSearchFilter = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 10, 16, 0)
            };

            lblSearch = new Label
            {
                Text = "Tìm kiếm:",
                Font = UITheme.FontBodyBold,
                ForeColor = UITheme.TextPrimary,
                AutoSize = true,
                Margin = new Padding(0, 6, 6, 0)
            };
            panelSearchFilter.Controls.Add(lblSearch);

            txtSearch = new TextBox
            {
                Font = UITheme.FontBody,
                Size = new Size(220, 26),
                Margin = new Padding(0, 2, 8, 0)
            };
            UITheme.SetupSearchPlaceholder(txtSearch, SearchPlaceholder);
            txtSearch.KeyDown += async (s, e) => { if (e.KeyCode == Keys.Enter) await SearchCategoryAsync(); };
            panelSearchFilter.Controls.Add(txtSearch);

            btnSearch = new Button
            {
                Text = "Tìm",
                Size = new Size(70, 28),
                Margin = new Padding(0, 0, 6, 0)
            };
            UITheme.ApplyButtonTheme(btnSearch, UITheme.ButtonStyle.Primary);
            btnSearch.Click += async (s, e) => await SearchCategoryAsync();
            panelSearchFilter.Controls.Add(btnSearch);

            btnReload = new Button
            {
                Text = "Tải lại",
                Size = new Size(75, 28),
                Margin = new Padding(0, 0, 0, 0)
            };
            UITheme.ApplyButtonTheme(btnReload, UITheme.ButtonStyle.Secondary);
            btnReload.Click += async (s, e) => await ReloadDataAsync();
            panelSearchFilter.Controls.Add(btnReload);

            panelTop.Controls.Add(panelSearchFilter);

            // 2. DataGridView hiển thị danh sách nhóm hàng
            dgvCategories = new DataGridView { Dock = DockStyle.Fill };
            dgvCategories.Columns.Add("CategoryId", "Mã ID");
            dgvCategories.Columns.Add("CategoryName", "Tên Nhóm Hàng");
            dgvCategories.Columns.Add("Description", "Mô Tả");

            dgvCategories.Columns["CategoryId"].Width = 80;
            dgvCategories.Columns["CategoryName"].Width = 260;
            dgvCategories.Columns["Description"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dgvCategories.CellClick += DgvCategories_CellClick;

            // 3. Panel bên phải chứa GroupBox thông tin và các nút chức năng
            panelRight = new Panel
            {
                Dock = DockStyle.Right,
                Width = 350,
                BackColor = Color.White,
                Padding = new Padding(12),
                AutoScroll = true
            };

            grbDetails = new GroupBox
            {
                Text = "Thông tin nhóm hàng",
                Font = UITheme.FontSubtitle,
                ForeColor = UITheme.TextPrimary,
                Dock = DockStyle.Top,
                Height = 245
            };
            panelRight.Controls.Add(grbDetails);

            lblId = new Label { Text = "Mã ID:", Font = UITheme.FontBodyBold, Location = new Point(14, 25), AutoSize = true };
            txtId = new TextBox { Font = UITheme.FontBody, Location = new Point(14, 45), Size = new Size(305, 25), ReadOnly = true, BackColor = Color.FromArgb(245, 245, 245) };

            lblName = new Label { Text = "Tên nhóm hàng (*):", Font = UITheme.FontBodyBold, Location = new Point(14, 75), AutoSize = true };
            txtName = new TextBox { Font = UITheme.FontBody, Location = new Point(14, 95), Size = new Size(305, 25) };

            lblDescription = new Label { Text = "Mô tả:", Font = UITheme.FontBodyBold, Location = new Point(14, 125), AutoSize = true };
            txtDescription = new TextBox { Font = UITheme.FontBody, Location = new Point(14, 145), Size = new Size(305, 80), Multiline = true, ScrollBars = ScrollBars.Vertical };

            grbDetails.Controls.AddRange(new Control[] {
                lblId, txtId,
                lblName, txtName,
                lblDescription, txtDescription
            });

            // 4 Nút thao tác CRUD (2 hàng x 2 cột gọn gàng)
            FlowLayoutPanel panelButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(0, 10, 0, 10)
            };

            int btnWidth = 155;
            int btnHeight = 36;

            btnAdd = new Button
            {
                Text = "➕ Thêm mới",
                Size = new Size(btnWidth, btnHeight),
                Margin = new Padding(3)
            };
            UITheme.ApplyButtonTheme(btnAdd, UITheme.ButtonStyle.Success);
            btnAdd.Click += async (s, e) => await CreateCategoryAsync();

            btnUpdate = new Button
            {
                Text = "✏ Cập nhật",
                Size = new Size(btnWidth, btnHeight),
                Margin = new Padding(3)
            };
            UITheme.ApplyButtonTheme(btnUpdate, UITheme.ButtonStyle.Warning);
            btnUpdate.Click += async (s, e) => await UpdateCategoryAsync();

            btnDelete = new Button
            {
                Text = "🗑 Xóa",
                Size = new Size(btnWidth, btnHeight),
                Margin = new Padding(3)
            };
            UITheme.ApplyButtonTheme(btnDelete, UITheme.ButtonStyle.Danger);
            btnDelete.Click += async (s, e) => await DeleteCategoryAsync();

            btnClear = new Button
            {
                Text = "🔄 Làm mới ô",
                Size = new Size(btnWidth, btnHeight),
                Margin = new Padding(3)
            };
            UITheme.ApplyButtonTheme(btnClear, UITheme.ButtonStyle.Secondary);
            btnClear.Click += (s, e) => ClearForm();

            panelButtons.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnClear });
            panelRight.Controls.Add(panelButtons);
            panelButtons.BringToFront();

            pager = new PaginationControl<CategoryDto>();
            pager.Bind(DisplayCategories, "nhóm hàng");

            this.Controls.Add(dgvCategories);
            this.Controls.Add(pager);
            this.Controls.Add(panelRight);
            this.Controls.Add(panelTop);
        }

        private void ApplyTheme()
        {
            UITheme.ApplyFormTheme(this);
            UITheme.ApplyGridTheme(dgvCategories);
        }

        private void SetPermission()
        {
            string role = (SessionManager.CurrentRole ?? "").ToUpper();
            if (role == "ADMIN")
            {
                btnAdd.Enabled = true;
                btnUpdate.Enabled = true;
                btnDelete.Enabled = true;
            }
            else if (role == "WAREHOUSE")
            {
                btnAdd.Enabled = true;
                btnUpdate.Enabled = true;
                btnDelete.Enabled = false;
            }
            else
            {
                btnAdd.Enabled = false;
                btnUpdate.Enabled = false;
                btnDelete.Enabled = false;
            }
        }

        private async void FormCategoryManagement_Load(object sender, EventArgs e)
        {
            SetPermission();
            await LoadDataAsync();
        }

        private async Task LoadDataAsync(string keyword = null)
        {
            try
            {
                lblStatus.Text = "Đang tải dữ liệu...";
                string endpoint = "Categories";
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    endpoint = $"Categories/search?keyword={Uri.EscapeDataString(keyword)}";
                }

                var response = await SessionManager.ApiClientService.Client.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(response, "Tải danh sách nhóm hàng");
                    lblStatus.Text = "Lỗi tải dữ liệu";
                    return;
                }

                var list = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
                _categories.Clear();
                if (list != null) _categories.AddRange(list);

                pager.SetDataSource(_categories);
                lblStatus.Text = $"Có {_categories.Count} nhóm hàng";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối API: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Lỗi tải dữ liệu";
            }
        }

        private void DisplayCategories(List<CategoryDto> list)
        {
            if (this.IsDisposed || this.Disposing || dgvCategories == null || dgvCategories.IsDisposed || dgvCategories.Columns.Count == 0) return;
            dgvCategories.SuspendLayout();
            try
            {
                dgvCategories.Rows.Clear();
                foreach (var c in list)
                {
                    int idx = dgvCategories.Rows.Add();
                    var row = dgvCategories.Rows[idx];
                    row.Cells[0].Value = c.CategoryId;
                    row.Cells[1].Value = c.CategoryName;
                    row.Cells[2].Value = c.Description;
                }

                if (list.Count > 0)
                {
                    dgvCategories.ClearSelection();
                    dgvCategories.Rows[0].Selected = true;
                    ShowCategory(0);
                }
                else
                {
                    ClearForm();
                }
            }
            finally
            {
                dgvCategories.ResumeLayout();
            }
        }

        private void DgvCategories_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            ShowCategory(e.RowIndex);
        }

        private void ShowCategory(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvCategories.Rows.Count) return;
            var row = dgvCategories.Rows[rowIndex];
            if (row.Cells[0].Value == null) return;

            int cId = Convert.ToInt32(row.Cells[0].Value);
            var c = _categories.FirstOrDefault(x => x.CategoryId == cId);
            if (c == null) return;

            txtId.Text = c.CategoryId.ToString();
            txtName.Text = c.CategoryName;
            txtDescription.Text = c.Description ?? "";
        }

        private void ClearForm()
        {
            txtId.Clear();
            txtName.Clear();
            txtDescription.Clear();
            txtName.Focus();
        }

        private async Task SearchCategoryAsync()
        {
            string kw = txtSearch.Text.Trim();
            if (string.IsNullOrWhiteSpace(kw) || kw == SearchPlaceholder)
            {
                await LoadDataAsync();
                return;
            }
            await LoadDataAsync(kw);
        }

        private async Task ReloadDataAsync()
        {
            txtSearch.Text = SearchPlaceholder;
            txtSearch.ForeColor = UITheme.TextMuted;
            await LoadDataAsync();
        }

        private async Task CreateCategoryAsync()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên nhóm hàng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            var c = new CategoryDto
            {
                CategoryName = txtName.Text.Trim(),
                Description = txtDescription.Text.Trim()
            };

            try
            {
                var resp = await SessionManager.ApiClientService.Client.PostAsJsonAsync("Categories", c);
                if (resp.IsSuccessStatusCode)
                {
                    SessionManager.DataCache.Categories = null;
                    MessageBox.Show("Thêm nhóm hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadDataAsync();
                    ClearForm();
                }
                else
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(resp, "Thêm nhóm hàng");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task UpdateCategoryAsync()
        {
            if (!int.TryParse(txtId.Text, out int id) || id <= 0)
            {
                MessageBox.Show("Vui lòng chọn 1 nhóm hàng từ danh sách để sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên nhóm hàng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            var c = new CategoryDto
            {
                CategoryId = id,
                CategoryName = txtName.Text.Trim(),
                Description = txtDescription.Text.Trim()
            };

            try
            {
                var resp = await SessionManager.ApiClientService.Client.PutAsJsonAsync($"Categories/{id}", c);
                if (resp.IsSuccessStatusCode)
                {
                    SessionManager.DataCache.Categories = null;
                    MessageBox.Show("Cập nhật nhóm hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadDataAsync();
                    SelectCategoryById(id);
                }
                else
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(resp, "Cập nhật nhóm hàng");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DeleteCategoryAsync()
        {
            if (!int.TryParse(txtId.Text, out int id) || id <= 0)
            {
                MessageBox.Show("Vui lòng chọn 1 nhóm hàng từ danh sách để xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa nhóm hàng \"{txtName.Text}\" (ID: {id}) không?\n\nLưu ý: Nếu nhóm hàng này đã có sản phẩm thuộc về nó thì không thể xóa.",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                var resp = await SessionManager.ApiClientService.Client.DeleteAsync($"Categories/{id}");
                if (resp.IsSuccessStatusCode)
                {
                    SessionManager.DataCache.Categories = null;
                    MessageBox.Show("Xóa nhóm hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    await LoadDataAsync();
                }
                else
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(resp, "Xóa nhóm hàng");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectCategoryById(int id)
        {
            for (int i = 0; i < dgvCategories.Rows.Count; i++)
            {
                if (Convert.ToInt32(dgvCategories.Rows[i].Cells[0].Value) == id)
                {
                    dgvCategories.ClearSelection();
                    dgvCategories.Rows[i].Selected = true;
                    dgvCategories.CurrentCell = dgvCategories.Rows[i].Cells[0];
                    ShowCategory(i);
                    break;
                }
            }
        }

        private void FormCategoryManagement_Load_1(object sender, EventArgs e)
        {

        }
    }
}
