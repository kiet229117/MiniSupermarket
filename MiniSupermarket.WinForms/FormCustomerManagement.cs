using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public partial class FormCustomerManagement : Form
    {
        private readonly List<CustomerDto> _customers = new List<CustomerDto>();

        // Controls giao diện
        private Panel panelTop;
        private DataGridView dgvCustomers;
        private Label lblTitle;
        private Label lblSearch;
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnReload;
        private Label lblStatus;

        private Panel panelRight;
        private GroupBox grbDetails;
        private Label lblCustomerID;
        private TextBox txtID;
        private Label lblCustomerName;
        private TextBox txtName;
        private Label lblPhone;
        private TextBox txtPhone;
        private Label lblAddress;
        private TextBox txtAddress;
        private Label lblPoints;
        private TextBox txtPoints;
        private Label lblRank;
        private ComboBox cboRank;

        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnClear;

        public FormCustomerManagement()
        {
            InitializeComponent();
            BuildUI();
            ApplyTheme();
        }

        private void BuildUI()
        {
            this.Text = "QUẢN LÝ KHÁCH HÀNG & TÍCH ĐIỂM";
            this.Size = new Size(1000, 600);
            this.Load += FormCustomerManagement_Load;

            // 1. Panel Top (Tiêu đề và Tìm kiếm)
            panelTop = new Panel { Dock = DockStyle.Top, Height = 65, BackColor = Color.White };

            lblTitle = new Label
            {
                Text = "QUẢN LÝ KHÁCH HÀNG & THÀNH VIÊN",
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

            lblSearch = new Label
            {
                Text = "Tìm kiếm:",
                Font = UITheme.FontBodyBold,
                ForeColor = UITheme.TextPrimary,
                Location = new Point(460, 20),
                AutoSize = true
            };
            panelTop.Controls.Add(lblSearch);

            txtSearch = new TextBox
            {
                Font = UITheme.FontBody,
                Location = new Point(530, 17),
                Size = new Size(200, 26)
            };
            UITheme.SetupSearchPlaceholder(txtSearch, "Tên hoặc Số điện thoại...");
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnSearch_Click(s, e); };
            panelTop.Controls.Add(txtSearch);

            btnSearch = new Button
            {
                Text = "Tìm",
                Location = new Point(740, 16),
                Size = new Size(70, 28)
            };
            UITheme.ApplyButtonTheme(btnSearch, UITheme.ButtonStyle.Primary);
            btnSearch.Click += BtnSearch_Click;
            panelTop.Controls.Add(btnSearch);

            btnReload = new Button
            {
                Text = "Tải lại",
                Location = new Point(820, 16),
                Size = new Size(75, 28)
            };
            UITheme.ApplyButtonTheme(btnReload, UITheme.ButtonStyle.Secondary);
            btnReload.Click += BtnReload_Click;
            panelTop.Controls.Add(btnReload);

            // 2. DataGridView hiển thị danh sách khách hàng
            dgvCustomers = new DataGridView { Dock = DockStyle.Fill };
            dgvCustomers.Columns.Add("CustomerId", "Mã KH");
            dgvCustomers.Columns.Add("CustomerName", "Họ và Tên");
            dgvCustomers.Columns.Add("PhoneNumber", "Số Điện Thoại");
            dgvCustomers.Columns.Add("Address", "Địa Chỉ");
            dgvCustomers.Columns.Add("RewardPoints", "Điểm Tích Lũy");
            dgvCustomers.Columns.Add("MembershipRank", "Hạng Thành Viên");

            dgvCustomers.Columns["CustomerId"].Width = 70;
            dgvCustomers.Columns["CustomerName"].Width = 160;
            dgvCustomers.Columns["PhoneNumber"].Width = 110;
            dgvCustomers.Columns["Address"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvCustomers.Columns["RewardPoints"].Width = 95;
            dgvCustomers.Columns["MembershipRank"].Width = 110;

            dgvCustomers.CellClick += DataGridView1_CellClick;

            // 3. Panel bên phải chứa GroupBox thông tin và các nút chức năng
            panelRight = new Panel
            {
                Dock = DockStyle.Right,
                Width = 330,
                BackColor = Color.White,
                Padding = new Padding(12)
            };

            grbDetails = new GroupBox
            {
                Text = "Thông tin khách hàng",
                Font = UITheme.FontSubtitle,
                ForeColor = UITheme.TextPrimary,
                Dock = DockStyle.Top,
                Height = 350
            };
            panelRight.Controls.Add(grbDetails);

            // Các trường nhập liệu
            lblCustomerID = new Label { Text = "Mã KH:", Font = UITheme.FontBodyBold, Location = new Point(15, 25), AutoSize = true };
            txtID = new TextBox { Font = UITheme.FontBody, Location = new Point(15, 45), Size = new Size(275, 25), ReadOnly = true, BackColor = Color.FromArgb(245, 245, 245) };

            lblCustomerName = new Label { Text = "Họ và tên (*):", Font = UITheme.FontBodyBold, Location = new Point(15, 75), AutoSize = true };
            txtName = new TextBox { Font = UITheme.FontBody, Location = new Point(15, 95), Size = new Size(275, 25) };

            lblPhone = new Label { Text = "Số điện thoại (*):", Font = UITheme.FontBodyBold, Location = new Point(15, 125), AutoSize = true };
            txtPhone = new TextBox { Font = UITheme.FontBody, Location = new Point(15, 145), Size = new Size(275, 25) };

            lblAddress = new Label { Text = "Địa chỉ:", Font = UITheme.FontBodyBold, Location = new Point(15, 175), AutoSize = true };
            txtAddress = new TextBox { Font = UITheme.FontBody, Location = new Point(15, 195), Size = new Size(275, 25) };

            lblPoints = new Label { Text = "Điểm tích lũy:", Font = UITheme.FontBodyBold, Location = new Point(15, 225), AutoSize = true };
            txtPoints = new TextBox { Font = UITheme.FontBody, Location = new Point(15, 245), Size = new Size(130, 25), Text = "0" };

            lblRank = new Label { Text = "Hạng TV:", Font = UITheme.FontBodyBold, Location = new Point(160, 225), AutoSize = true };
            cboRank = new ComboBox { Font = UITheme.FontBody, Location = new Point(160, 245), Size = new Size(130, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cboRank.Items.AddRange(new object[] { "Chuẩn", "Bạc", "Vàng", "Kim cương" });
            cboRank.SelectedIndex = 0;

            grbDetails.Controls.AddRange(new Control[] {
                lblCustomerID, txtID,
                lblCustomerName, txtName,
                lblPhone, txtPhone,
                lblAddress, txtAddress,
                lblPoints, txtPoints,
                lblRank, cboRank
            });

            panelRight.AutoScroll = true;

            // 4 Nút thao tác CRUD (Bố trí 2 hàng x 2 cột xuống dòng gọn gàng, không tràn khung)
            FlowLayoutPanel panelButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(2, 8, 2, 8)
            };

            int btnWidth = 145;
            int btnHeight = 36;

            btnAdd = new Button { Text = "➕ Thêm KH", Size = new Size(btnWidth, btnHeight), Margin = new Padding(4) };
            UITheme.ApplyButtonTheme(btnAdd, UITheme.ButtonStyle.Success);
            btnAdd.Click += BtnAdd_Click;

            btnUpdate = new Button { Text = "✏ Cập Nhật", Size = new Size(btnWidth, btnHeight), Margin = new Padding(4) };
            UITheme.ApplyButtonTheme(btnUpdate, UITheme.ButtonStyle.Warning);
            btnUpdate.Click += BtnUpdate_Click;

            btnDelete = new Button { Text = "🗑 Xóa KH", Size = new Size(btnWidth, btnHeight), Margin = new Padding(4) };
            UITheme.ApplyButtonTheme(btnDelete, UITheme.ButtonStyle.Danger);
            btnDelete.Click += BtnDelete_Click;

            btnClear = new Button { Text = "🔄 Làm Mới", Size = new Size(btnWidth, btnHeight), Margin = new Padding(4) };
            UITheme.ApplyButtonTheme(btnClear, UITheme.ButtonStyle.Secondary);
            btnClear.Click += (s, e) => ClearInput();

            panelButtons.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnClear });
            panelRight.Controls.Add(panelButtons);
            panelButtons.BringToFront();

            this.Controls.Add(dgvCustomers);
            this.Controls.Add(panelTop);
            this.Controls.Add(panelRight);
        }

        private void ApplyTheme()
        {
            UITheme.ApplyFormTheme(this);
            UITheme.ApplyGridTheme(dgvCustomers);
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
            else if (role == "CASHIER")
            {
                btnAdd.Enabled = true;
                btnUpdate.Enabled = true;
                btnDelete.Enabled = false; // Thu ngân không có quyền xoá khách hàng
            }
            else
            {
                btnAdd.Enabled = false;
                btnUpdate.Enabled = false;
                btnDelete.Enabled = false;
            }
        }

        private async void FormCustomerManagement_Load(object sender, EventArgs e)
        {
            SetPermission();
            await LoadDataAsync();
        }

        // ==========================================
        // GỌI API LẤY DANH SÁCH KHÁCH HÀNG
        // GET /api/Customers?keyword=...
        // ==========================================
        private async Task LoadDataAsync(string keyword = null)
        {
            lblStatus.Text = "Đang tải dữ liệu...";
            try
            {
                string endpoint = "Customers";
                if (!string.IsNullOrWhiteSpace(keyword) && keyword != "Tên hoặc Số điện thoại...")
                {
                    endpoint += $"?keyword={Uri.EscapeDataString(keyword)}";
                }

                var response = await SessionManager.ApiClientService.Client.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<List<CustomerDto>>();
                    _customers.Clear();
                    if (data != null) _customers.AddRange(data);

                    DisplayCustomers(_customers);
                    lblStatus.Text = $"Có {_customers.Count} khách hàng";
                }
                else
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(response, "Lấy danh sách khách hàng");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối API: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Lỗi kết nối API";
            }
        }

        private void DisplayCustomers(List<CustomerDto> list)
        {
            dgvCustomers.Rows.Clear();
            foreach (var item in list)
            {
                int idx = dgvCustomers.Rows.Add();
                var row = dgvCustomers.Rows[idx];
                row.Cells[0].Value = item.CustomerId;
                row.Cells[1].Value = item.CustomerName;
                row.Cells[2].Value = item.PhoneNumber;
                row.Cells[3].Value = item.Address;
                row.Cells[4].Value = item.RewardPoints;
                row.Cells[5].Value = item.MembershipRank;
            }

            if (list.Count > 0)
            {
                dgvCustomers.ClearSelection();
                dgvCustomers.Rows[0].Selected = true;
                ShowCustomer(0);
            }
            else
            {
                ClearInput();
            }
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            ShowCustomer(e.RowIndex);
        }

        private void ShowCustomer(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvCustomers.Rows.Count) return;
            var row = dgvCustomers.Rows[rowIndex];

            txtID.Text = Convert.ToString(row.Cells[0].Value);
            txtName.Text = Convert.ToString(row.Cells[1].Value);
            txtPhone.Text = Convert.ToString(row.Cells[2].Value);
            txtAddress.Text = Convert.ToString(row.Cells[3].Value);
            txtPoints.Text = Convert.ToString(row.Cells[4].Value);

            string rank = Convert.ToString(row.Cells[5].Value);
            if (!string.IsNullOrEmpty(rank) && cboRank.Items.Contains(rank))
                cboRank.SelectedItem = rank;
            else
                cboRank.SelectedIndex = 0;
        }

        private void ClearInput()
        {
            txtID.Clear();
            txtName.Clear();
            txtPhone.Clear();
            txtAddress.Clear();
            txtPoints.Text = "0";
            cboRank.SelectedIndex = 0;
            txtName.Focus();
        }

        private async void BtnSearch_Click(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            await LoadDataAsync(keyword);
        }

        private async void BtnReload_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "Tên hoặc Số điện thoại...";
            txtSearch.ForeColor = UITheme.TextMuted;
            await LoadDataAsync();
        }

        // ==========================================
        // THÊM MỚI KHÁCH HÀNG: POST /api/Customers
        // ==========================================
        private async void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Vui lòng nhập Họ tên và Số điện thoại!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            int.TryParse(txtPoints.Text.Trim(), out int points);

            var newCustomer = new CustomerDto
            {
                CustomerName = txtName.Text.Trim(),
                PhoneNumber = txtPhone.Text.Trim(),
                Address = txtAddress.Text.Trim(),
                RewardPoints = points,
                MembershipRank = cboRank.SelectedItem?.ToString() ?? "Chuẩn"
            };

            try
            {
                var response = await SessionManager.ApiClientService.Client.PostAsJsonAsync("Customers", newCustomer);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Thêm mới khách hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearInput();
                    await LoadDataAsync();
                }
                else
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(response, "Thêm khách hàng");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối API: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========================================
        // CẬP NHẬT KHÁCH HÀNG: PUT /api/Customers/{id}
        // ==========================================
        private async void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtID.Text, out int id))
            {
                MessageBox.Show("Vui lòng chọn khách hàng cần cập nhật!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Vui lòng nhập Họ tên và Số điện thoại!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int.TryParse(txtPoints.Text.Trim(), out int points);

            var updateDto = new CustomerDto
            {
                CustomerId = id,
                CustomerName = txtName.Text.Trim(),
                PhoneNumber = txtPhone.Text.Trim(),
                Address = txtAddress.Text.Trim(),
                RewardPoints = points,
                MembershipRank = cboRank.SelectedItem?.ToString() ?? "Chuẩn"
            };

            try
            {
                var response = await SessionManager.ApiClientService.Client.PutAsJsonAsync($"Customers/{id}", updateDto);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Cập nhật thông tin khách hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadDataAsync();
                }
                else
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(response, "Cập nhật khách hàng");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối API: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========================================
        // XÓA KHÁCH HÀNG: DELETE /api/Customers/{id}
        // ==========================================
        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtID.Text, out int id))
            {
                MessageBox.Show("Vui lòng chọn khách hàng cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa khách hàng '{txtName.Text}' không?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                var response = await SessionManager.ApiClientService.Client.DeleteAsync($"Customers/{id}");
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Xóa khách hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearInput();
                    await LoadDataAsync();
                }
                else
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(response, "Xóa khách hàng");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối API: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}