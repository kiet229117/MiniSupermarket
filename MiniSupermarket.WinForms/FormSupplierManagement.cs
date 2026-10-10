using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public class FormSupplierManagement : Form
    {
        private const string SearchPlaceholder = "Tên NCC hoặc Số điện thoại...";
        private readonly List<SupplierDto> _suppliers = new List<SupplierDto>();

        // Controls giao diện
        private Panel panelTop;
        private Label lblTitle;
        private Label lblStatus;
        private Label lblSearch;
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnReload;

        private DataGridView dgvSuppliers;

        private Panel panelRight;
        private GroupBox grbDetails;
        private Label lblId;
        private TextBox txtId;
        private Label lblName;
        private TextBox txtName;
        private Label lblPhone;
        private TextBox txtPhone;
        private Label lblEmail;
        private TextBox txtEmail;
        private Label lblAddress;
        private TextBox txtAddress;

        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnClear;

        public FormSupplierManagement()
        {
            BuildUI();
            ApplyTheme();
            this.Load += FormSupplierManagement_Load;
        }

        private void BuildUI()
        {
            this.Text = "QUẢN LÝ NHÀ CUNG CẤP";
            this.Size = new Size(1000, 600);
            this.Dock = DockStyle.Fill;

            // 1. Panel Top (Tiêu đề và Tìm kiếm)
            panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.White
            };

            lblTitle = new Label
            {
                Text = "QUẢN LÝ NHÀ CUNG CẤP & ĐỐI TÁC",
                Font = UITheme.FontTitle,
                ForeColor = UITheme.PrimaryColor,
                Location = new Point(16, 12),
                AutoSize = true
            };
            panelTop.Controls.Add(lblTitle);

            lblStatus = new Label
            {
                Text = "Sẵn sàng",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.TextMuted,
                Location = new Point(18, 38),
                AutoSize = true
            };
            panelTop.Controls.Add(lblStatus);

            FlowLayoutPanel panelSearchFilter = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 16, 16, 0)
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
            txtSearch.KeyDown += async (s, e) => { if (e.KeyCode == Keys.Enter) await SearchSupplierAsync(); };
            panelSearchFilter.Controls.Add(txtSearch);

            btnSearch = new Button
            {
                Text = "Tìm",
                Size = new Size(70, 28),
                Margin = new Padding(0, 0, 6, 0)
            };
            UITheme.ApplyButtonTheme(btnSearch, UITheme.ButtonStyle.Primary);
            btnSearch.Click += async (s, e) => await SearchSupplierAsync();
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

            // 2. DataGridView hiển thị danh sách nhà cung cấp
            dgvSuppliers = new DataGridView { Dock = DockStyle.Fill };
            dgvSuppliers.Columns.Add("SupplierId", "Mã ID");
            dgvSuppliers.Columns.Add("SupplierName", "Tên Nhà Cung Cấp");
            dgvSuppliers.Columns.Add("PhoneNumber", "Số Điện Thoại");
            dgvSuppliers.Columns.Add("Email", "Email");
            dgvSuppliers.Columns.Add("Address", "Địa Chỉ");

            dgvSuppliers.Columns["SupplierId"].Width = 70;
            dgvSuppliers.Columns["SupplierName"].Width = 200;
            dgvSuppliers.Columns["PhoneNumber"].Width = 125;
            dgvSuppliers.Columns["Email"].Width = 160;
            dgvSuppliers.Columns["Address"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dgvSuppliers.CellClick += DgvSuppliers_CellClick;

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
                Text = "Thông tin nhà cung cấp",
                Font = UITheme.FontSubtitle,
                ForeColor = UITheme.TextPrimary,
                Dock = DockStyle.Top,
                Height = 330
            };
            panelRight.Controls.Add(grbDetails);

            lblId = new Label { Text = "Mã ID:", Font = UITheme.FontBodyBold, Location = new Point(14, 25), AutoSize = true };
            txtId = new TextBox { Font = UITheme.FontBody, Location = new Point(14, 45), Size = new Size(305, 25), ReadOnly = true, BackColor = Color.FromArgb(245, 245, 245) };

            lblName = new Label { Text = "Tên nhà cung cấp (*):", Font = UITheme.FontBodyBold, Location = new Point(14, 75), AutoSize = true };
            txtName = new TextBox { Font = UITheme.FontBody, Location = new Point(14, 95), Size = new Size(305, 25) };

            lblPhone = new Label { Text = "Số điện thoại (*):", Font = UITheme.FontBodyBold, Location = new Point(14, 125), AutoSize = true };
            txtPhone = new TextBox { Font = UITheme.FontBody, Location = new Point(14, 145), Size = new Size(305, 25) };

            lblEmail = new Label { Text = "Email liên hệ:", Font = UITheme.FontBodyBold, Location = new Point(14, 175), AutoSize = true };
            txtEmail = new TextBox { Font = UITheme.FontBody, Location = new Point(14, 195), Size = new Size(305, 25) };

            lblAddress = new Label { Text = "Địa chỉ trụ sở:", Font = UITheme.FontBodyBold, Location = new Point(14, 225), AutoSize = true };
            txtAddress = new TextBox { Font = UITheme.FontBody, Location = new Point(14, 245), Size = new Size(305, 65), Multiline = true };

            grbDetails.Controls.AddRange(new Control[] {
                lblId, txtId,
                lblName, txtName,
                lblPhone, txtPhone,
                lblEmail, txtEmail,
                lblAddress, txtAddress
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
            btnAdd.Click += async (s, e) => await CreateSupplierAsync();

            btnUpdate = new Button
            {
                Text = "✏ Cập nhật",
                Size = new Size(btnWidth, btnHeight),
                Margin = new Padding(3)
            };
            UITheme.ApplyButtonTheme(btnUpdate, UITheme.ButtonStyle.Warning);
            btnUpdate.Click += async (s, e) => await UpdateSupplierAsync();

            btnDelete = new Button
            {
                Text = "🗑 Xóa",
                Size = new Size(btnWidth, btnHeight),
                Margin = new Padding(3)
            };
            UITheme.ApplyButtonTheme(btnDelete, UITheme.ButtonStyle.Danger);
            btnDelete.Click += async (s, e) => await DeleteSupplierAsync();

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

            this.Controls.Add(dgvSuppliers);
            this.Controls.Add(panelRight);
            this.Controls.Add(panelTop);
        }

        private void ApplyTheme()
        {
            UITheme.ApplyFormTheme(this);
            UITheme.ApplyGridTheme(dgvSuppliers);
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

        private async void FormSupplierManagement_Load(object sender, EventArgs e)
        {
            SetPermission();
            await LoadDataAsync();
        }

        private async Task LoadDataAsync(string keyword = null)
        {
            try
            {
                lblStatus.Text = "Đang tải dữ liệu...";
                string endpoint = "Suppliers";
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    endpoint += $"?keyword={Uri.EscapeDataString(keyword)}";
                }

                var response = await SessionManager.ApiClientService.Client.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(response, "Tải danh sách nhà cung cấp");
                    lblStatus.Text = "Lỗi tải dữ liệu";
                    return;
                }

                var list = await response.Content.ReadFromJsonAsync<List<SupplierDto>>();
                _suppliers.Clear();
                if (list != null) _suppliers.AddRange(list);

                DisplaySuppliers(_suppliers);
                lblStatus.Text = $"Có {_suppliers.Count} nhà cung cấp";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối API: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Lỗi tải dữ liệu";
            }
        }

        private void DisplaySuppliers(List<SupplierDto> list)
        {
            dgvSuppliers.Rows.Clear();
            foreach (var s in list)
            {
                int idx = dgvSuppliers.Rows.Add();
                var row = dgvSuppliers.Rows[idx];
                row.Cells[0].Value = s.SupplierId;
                row.Cells[1].Value = s.SupplierName;
                row.Cells[2].Value = s.PhoneNumber;
                row.Cells[3].Value = s.Email;
                row.Cells[4].Value = s.Address;
            }

            if (list.Count > 0)
            {
                dgvSuppliers.ClearSelection();
                dgvSuppliers.Rows[0].Selected = true;
                ShowSupplier(0);
            }
            else
            {
                ClearForm();
            }
        }

        private void DgvSuppliers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            ShowSupplier(e.RowIndex);
        }

        private void ShowSupplier(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= _suppliers.Count) return;
            var s = _suppliers[rowIndex];

            txtId.Text = s.SupplierId.ToString();
            txtName.Text = s.SupplierName;
            txtPhone.Text = s.PhoneNumber ?? "";
            txtEmail.Text = s.Email ?? "";
            txtAddress.Text = s.Address ?? "";
        }

        private void ClearForm()
        {
            txtId.Clear();
            txtName.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            txtAddress.Clear();
            txtName.Focus();
        }

        private async Task SearchSupplierAsync()
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

        private async Task CreateSupplierAsync()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên nhà cung cấp!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            var s = new SupplierDto
            {
                SupplierName = txtName.Text.Trim(),
                PhoneNumber = txtPhone.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Address = txtAddress.Text.Trim()
            };

            try
            {
                var resp = await SessionManager.ApiClientService.Client.PostAsJsonAsync("Suppliers", s);
                if (resp.IsSuccessStatusCode)
                {
                    MessageBox.Show("Thêm nhà cung cấp thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadDataAsync();
                    ClearForm();
                }
                else
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(resp, "Thêm nhà cung cấp");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task UpdateSupplierAsync()
        {
            if (!int.TryParse(txtId.Text, out int id) || id <= 0)
            {
                MessageBox.Show("Vui lòng chọn 1 nhà cung cấp từ danh sách để sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên nhà cung cấp!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            var s = new SupplierDto
            {
                SupplierId = id,
                SupplierName = txtName.Text.Trim(),
                PhoneNumber = txtPhone.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Address = txtAddress.Text.Trim()
            };

            try
            {
                var resp = await SessionManager.ApiClientService.Client.PutAsJsonAsync($"Suppliers/{id}", s);
                if (resp.IsSuccessStatusCode)
                {
                    MessageBox.Show("Cập nhật nhà cung cấp thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadDataAsync();
                }
                else
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(resp, "Cập nhật nhà cung cấp");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DeleteSupplierAsync()
        {
            if (!int.TryParse(txtId.Text, out int id) || id <= 0)
            {
                MessageBox.Show("Vui lòng chọn 1 nhà cung cấp từ danh sách để xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var conf = MessageBox.Show($"Bạn có chắc muốn xóa nhà cung cấp \"{txtName.Text}\"?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (conf != DialogResult.Yes) return;

            try
            {
                var resp = await SessionManager.ApiClientService.Client.DeleteAsync($"Suppliers/{id}");
                if (resp.IsSuccessStatusCode)
                {
                    MessageBox.Show("Xóa nhà cung cấp thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    await LoadDataAsync();
                }
                else
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(resp, "Xóa nhà cung cấp");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Backward compatibility
        public class ApiSupplier : SupplierDto
        {
            public string Phone
            {
                get => PhoneNumber;
                set => PhoneNumber = value;
            }
        }
    }
}
