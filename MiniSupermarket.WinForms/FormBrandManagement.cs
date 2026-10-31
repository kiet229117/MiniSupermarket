using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public class FormBrandManagement : Form
    {
        private const string SearchPlaceholder = "Nhập tên thương hiệu...";
        private readonly List<BrandDto> _brands = new List<BrandDto>();

        // Controls giao diện
        private Panel panelTop;
                private Label lblStatus;
        private PaginationControl<BrandDto> pager;
        private Label lblSearch;
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnReload;

        private DataGridView dgvBrands;

        private Panel panelRight;
        private GroupBox grbDetails;
        private Label lblId;
        private TextBox txtId;
        private Label lblName;
        private TextBox txtName;
        private Label lblCountry;
        private TextBox txtCountry;
        private Label lblDescription;
        private TextBox txtDescription;
        private CheckBox chkIsActive;

        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnClear;

        public FormBrandManagement()
        {
            BuildUI();
            ApplyTheme();
            this.Load += FormBrandManagement_Load;
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
            txtSearch.KeyDown += async (s, e) => { if (e.KeyCode == Keys.Enter) await SearchBrandAsync(); };
            panelSearchFilter.Controls.Add(txtSearch);

            btnSearch = new Button
            {
                Text = "Tìm",
                Size = new Size(70, 28),
                Margin = new Padding(0, 0, 6, 0)
            };
            UITheme.ApplyButtonTheme(btnSearch, UITheme.ButtonStyle.Primary);
            btnSearch.Click += async (s, e) => await SearchBrandAsync();
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

            // 2. DataGridView hiển thị danh sách thương hiệu
            dgvBrands = new DataGridView { Dock = DockStyle.Fill };
            dgvBrands.Columns.Add("BrandId", "Mã ID");
            dgvBrands.Columns.Add("BrandName", "Tên Thương Hiệu");
            dgvBrands.Columns.Add("Country", "Xuất Xứ / Quốc Gia");
            dgvBrands.Columns.Add("Description", "Mô Tả");
            dgvBrands.Columns.Add("IsActive", "Trạng Thái");

            dgvBrands.Columns["BrandId"].Width = 70;
            dgvBrands.Columns["BrandName"].Width = 180;
            dgvBrands.Columns["Country"].Width = 140;
            dgvBrands.Columns["Description"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvBrands.Columns["IsActive"].Width = 100;

            dgvBrands.CellClick += DgvBrands_CellClick;

            // 3. Panel bên phải chứa GroupBox thông tin và các nút chức năng
            panelRight = new Panel
            {
                Dock = DockStyle.Right,
                Width = 340,
                BackColor = Color.White,
                Padding = new Padding(12),
                AutoScroll = true
            };

            grbDetails = new GroupBox
            {
                Text = "Thông tin thương hiệu",
                Font = UITheme.FontSubtitle,
                ForeColor = UITheme.TextPrimary,
                Dock = DockStyle.Top,
                Height = 310
            };
            panelRight.Controls.Add(grbDetails);

            lblId = new Label { Text = "Mã ID:", Font = UITheme.FontBodyBold, Location = new Point(14, 25), AutoSize = true };
            txtId = new TextBox { Font = UITheme.FontBody, Location = new Point(14, 45), Size = new Size(295, 25), ReadOnly = true, BackColor = Color.FromArgb(245, 245, 245) };

            lblName = new Label { Text = "Tên thương hiệu (*):", Font = UITheme.FontBodyBold, Location = new Point(14, 75), AutoSize = true };
            txtName = new TextBox { Font = UITheme.FontBody, Location = new Point(14, 95), Size = new Size(295, 25) };

            lblCountry = new Label { Text = "Quốc gia / Xuất xứ:", Font = UITheme.FontBodyBold, Location = new Point(14, 125), AutoSize = true };
            txtCountry = new TextBox { Font = UITheme.FontBody, Location = new Point(14, 145), Size = new Size(295, 25) };

            lblDescription = new Label { Text = "Mô tả:", Font = UITheme.FontBodyBold, Location = new Point(14, 175), AutoSize = true };
            txtDescription = new TextBox { Font = UITheme.FontBody, Location = new Point(14, 195), Size = new Size(295, 55), Multiline = true };

            chkIsActive = new CheckBox { Text = "Đang hợp tác / Hoạt động", Font = UITheme.FontBodyBold, Location = new Point(14, 260), AutoSize = true, Checked = true };

            grbDetails.Controls.AddRange(new Control[] {
                lblId, txtId,
                lblName, txtName,
                lblCountry, txtCountry,
                lblDescription, txtDescription,
                chkIsActive
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

            int btnWidth = 150;
            int btnHeight = 36;

            btnAdd = new Button
            {
                Text = "➕ Thêm mới",
                Size = new Size(btnWidth, btnHeight),
                Margin = new Padding(3)
            };
            UITheme.ApplyButtonTheme(btnAdd, UITheme.ButtonStyle.Success);
            btnAdd.Click += async (s, e) => await CreateBrandAsync();

            btnUpdate = new Button
            {
                Text = "✏ Cập nhật",
                Size = new Size(btnWidth, btnHeight),
                Margin = new Padding(3)
            };
            UITheme.ApplyButtonTheme(btnUpdate, UITheme.ButtonStyle.Warning);
            btnUpdate.Click += async (s, e) => await UpdateBrandAsync();

            btnDelete = new Button
            {
                Text = "🗑 Xóa",
                Size = new Size(btnWidth, btnHeight),
                Margin = new Padding(3)
            };
            UITheme.ApplyButtonTheme(btnDelete, UITheme.ButtonStyle.Danger);
            btnDelete.Click += async (s, e) => await DeleteBrandAsync();

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

            pager = new PaginationControl<BrandDto>();
            pager.Bind(DisplayBrands, "thương hiệu");

            this.Controls.Add(dgvBrands);
            this.Controls.Add(pager);
            this.Controls.Add(panelRight);
            this.Controls.Add(panelTop);
        }

        private void ApplyTheme()
        {
            UITheme.ApplyFormTheme(this);
            UITheme.ApplyGridTheme(dgvBrands);
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

        private async void FormBrandManagement_Load(object sender, EventArgs e)
        {
            SetPermission();
            await LoadDataAsync();
        }

        private async Task LoadDataAsync(string keyword = null)
        {
            try
            {
                lblStatus.Text = "Đang tải dữ liệu...";
                string endpoint = "Brands";
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    endpoint += $"?keyword={Uri.EscapeDataString(keyword)}";
                }

                var response = await SessionManager.ApiClientService.Client.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(response, "Tải danh sách thương hiệu");
                    lblStatus.Text = "Lỗi tải dữ liệu";
                    return;
                }

                var list = await response.Content.ReadFromJsonAsync<List<BrandDto>>();
                _brands.Clear();
                if (list != null) _brands.AddRange(list);

                pager.SetDataSource(_brands);
                lblStatus.Text = $"Có {_brands.Count} thương hiệu";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối API: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Lỗi tải dữ liệu";
            }
        }

        private void DisplayBrands(List<BrandDto> list)
        {
            if (this.IsDisposed || this.Disposing || dgvBrands == null || dgvBrands.IsDisposed || dgvBrands.Columns.Count == 0) return;
            dgvBrands.SuspendLayout();
            try
            {
                dgvBrands.Rows.Clear();
                foreach (var b in list)
                {
                    int idx = dgvBrands.Rows.Add();
                    var row = dgvBrands.Rows[idx];
                    row.Cells[0].Value = b.BrandId;
                    row.Cells[1].Value = b.BrandName;
                    row.Cells[2].Value = b.Country;
                    row.Cells[3].Value = b.Description;
                    row.Cells[4].Value = b.IsActive ? "Đang hợp tác" : "Ngừng";
                }

                if (list.Count > 0)
                {
                    dgvBrands.ClearSelection();
                    dgvBrands.Rows[0].Selected = true;
                    ShowBrand(0);
                }
                else
                {
                    ClearForm();
                }
            }
            finally
            {
                dgvBrands.ResumeLayout();
            }
        }

        private void DgvBrands_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            ShowBrand(e.RowIndex);
        }

        private void ShowBrand(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvBrands.Rows.Count) return;
            var row = dgvBrands.Rows[rowIndex];
            if (row.Cells[0].Value == null) return;

            int bId = Convert.ToInt32(row.Cells[0].Value);
            var b = _brands.FirstOrDefault(x => x.BrandId == bId);
            if (b == null) return;

            txtId.Text = b.BrandId.ToString();
            txtName.Text = b.BrandName;
            txtCountry.Text = b.Country ?? "";
            txtDescription.Text = b.Description ?? "";
            chkIsActive.Checked = b.IsActive;
        }

        private void ClearForm()
        {
            txtId.Clear();
            txtName.Clear();
            txtCountry.Clear();
            txtDescription.Clear();
            chkIsActive.Checked = true;
            txtName.Focus();
        }

        private async Task SearchBrandAsync()
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

        private async Task CreateBrandAsync()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên thương hiệu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            var b = new BrandDto
            {
                BrandName = txtName.Text.Trim(),
                Country = txtCountry.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                IsActive = chkIsActive.Checked
            };

            try
            {
                var resp = await SessionManager.ApiClientService.Client.PostAsJsonAsync("Brands", b);
                if (resp.IsSuccessStatusCode)
                {
                    SessionManager.DataCache.Brands = null;
                    MessageBox.Show("Thêm thương hiệu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadDataAsync();
                    ClearForm();
                }
                else
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(resp, "Thêm thương hiệu");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task UpdateBrandAsync()
        {
            if (!int.TryParse(txtId.Text, out int id) || id <= 0)
            {
                MessageBox.Show("Vui lòng chọn 1 thương hiệu từ danh sách để sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên thương hiệu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            var b = new BrandDto
            {
                BrandId = id,
                BrandName = txtName.Text.Trim(),
                Country = txtCountry.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                IsActive = chkIsActive.Checked
            };

            try
            {
                var resp = await SessionManager.ApiClientService.Client.PutAsJsonAsync($"Brands/{id}", b);
                if (resp.IsSuccessStatusCode)
                {
                    SessionManager.DataCache.Brands = null;
                    MessageBox.Show("Cập nhật thương hiệu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadDataAsync();
                }
                else
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(resp, "Cập nhật thương hiệu");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DeleteBrandAsync()
        {
            if (!int.TryParse(txtId.Text, out int id) || id <= 0)
            {
                MessageBox.Show("Vui lòng chọn 1 thương hiệu từ danh sách để xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var conf = MessageBox.Show($"Bạn có chắc muốn xóa thương hiệu \"{txtName.Text}\"?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (conf != DialogResult.Yes) return;

            try
            {
                var resp = await SessionManager.ApiClientService.Client.DeleteAsync($"Brands/{id}");
                if (resp.IsSuccessStatusCode)
                {
                    SessionManager.DataCache.Brands = null;
                    MessageBox.Show("Xóa thương hiệu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    await LoadDataAsync();
                }
                else
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(resp, "Xóa thương hiệu");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Backward compatibility
        public class ApiBrand : BrandDto { }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // FormBrandManagement
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "FormBrandManagement";
            this.Load += new System.EventHandler(this.FormBrandManagement_Load_1);
            this.ResumeLayout(false);

        }

        private void FormBrandManagement_Load_1(object sender, EventArgs e)
        {

        }
    }
}
