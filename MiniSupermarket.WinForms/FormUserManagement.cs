using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public partial class FormUserManagement : Form
    {
        private readonly List<UserDto> _users = new List<UserDto>();

        // Controls
        private Panel panelTop;
        private Label lblTitle;
        private Label lblStatus;
        private Label lblSearch;
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnReload;

        private DataGridView dgvUsers;

        private Panel panelRight;
        private GroupBox grbDetails;
        private Label lblId;
        private TextBox txtId;
        private Label lblUsername;
        private TextBox txtUsername;
        private Label lblPassword;
        private TextBox txtPassword;
        private Label lblFullName;
        private TextBox txtFullName;
        private Label lblRole;
        private ComboBox cboRole;
        private Label lblEmail;
        private TextBox txtEmail;
        private Label lblPhone;
        private TextBox txtPhone;
        private CheckBox chkIsActive;

        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnClear;

        public FormUserManagement()
        {
            InitializeComponent();
            BuildUI();
            ApplyTheme();
        }

        private void BuildUI()
        {
            this.Text = "QUẢN LÝ TÀI KHOẢN NGƯỜI DÙNG";
            this.Size = new Size(1000, 600);
            this.Load += FormUserManagement_Load;

            // 1. Top Panel
            panelTop = new Panel { Dock = DockStyle.Top, Height = 65, BackColor = Color.White };

            lblTitle = new Label
            {
                Text = "QUẢN TRỊ TÀI KHOẢN & PHÂN QUYỀN",
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
                Location = new Point(480, 20),
                AutoSize = true
            };
            panelTop.Controls.Add(lblSearch);

            txtSearch = new TextBox
            {
                Font = UITheme.FontBody,
                Location = new Point(550, 17),
                Size = new Size(180, 26)
            };
            UITheme.SetupSearchPlaceholder(txtSearch, "Tên đăng nhập hoặc Họ tên...");
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

            // 2. DataGridView
            dgvUsers = new DataGridView { Dock = DockStyle.Fill };
            dgvUsers.Columns.Add("UserId", "ID");
            dgvUsers.Columns.Add("Username", "Tên Đăng Nhập");
            dgvUsers.Columns.Add("FullName", "Họ Và Tên");
            dgvUsers.Columns.Add("Role", "Vai Trò");
            dgvUsers.Columns.Add("Email", "Email");
            dgvUsers.Columns.Add("Phone", "Điện Thoại");
            dgvUsers.Columns.Add("IsActive", "Trạng Thái");

            dgvUsers.Columns["UserId"].Width = 60;
            dgvUsers.Columns["Username"].Width = 120;
            dgvUsers.Columns["FullName"].Width = 150;
            dgvUsers.Columns["Role"].Width = 100;
            dgvUsers.Columns["Email"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvUsers.Columns["Phone"].Width = 110;
            dgvUsers.Columns["IsActive"].Width = 90;

            dgvUsers.CellClick += DgvUsers_CellClick;

            // 3. Right Details Panel
            panelRight = new Panel
            {
                Dock = DockStyle.Right,
                Width = 330,
                BackColor = Color.White,
                Padding = new Padding(12)
            };

            grbDetails = new GroupBox
            {
                Text = "Thông tin tài khoản",
                Font = UITheme.FontSubtitle,
                ForeColor = UITheme.TextPrimary,
                Dock = DockStyle.Top,
                Height = 360
            };
            panelRight.Controls.Add(grbDetails);

            lblId = new Label { Text = "Mã NV:", Font = UITheme.FontBodyBold, Location = new Point(15, 25), AutoSize = true };
            txtId = new TextBox { Font = UITheme.FontBody, Location = new Point(15, 45), Size = new Size(275, 25), ReadOnly = true, BackColor = Color.FromArgb(245, 245, 245) };

            lblUsername = new Label { Text = "Tên đăng nhập (*):", Font = UITheme.FontBodyBold, Location = new Point(15, 75), AutoSize = true };
            txtUsername = new TextBox { Font = UITheme.FontBody, Location = new Point(15, 95), Size = new Size(275, 25) };

            lblPassword = new Label { Text = "Mật khẩu (để trống nếu không đổi):", Font = UITheme.FontBodyBold, Location = new Point(15, 125), AutoSize = true };
            txtPassword = new TextBox { Font = UITheme.FontBody, Location = new Point(15, 145), Size = new Size(275, 25), PasswordChar = '*' };

            lblFullName = new Label { Text = "Họ và tên (*):", Font = UITheme.FontBodyBold, Location = new Point(15, 175), AutoSize = true };
            txtFullName = new TextBox { Font = UITheme.FontBody, Location = new Point(15, 195), Size = new Size(275, 25) };

            lblRole = new Label { Text = "Vai trò:", Font = UITheme.FontBodyBold, Location = new Point(15, 225), AutoSize = true };
            cboRole = new ComboBox { Font = UITheme.FontBody, Location = new Point(15, 245), Size = new Size(130, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cboRole.Items.AddRange(new object[] { "ADMIN", "WAREHOUSE", "CASHIER" });
            cboRole.SelectedIndex = 2;

            lblPhone = new Label { Text = "SĐT:", Font = UITheme.FontBodyBold, Location = new Point(155, 225), AutoSize = true };
            txtPhone = new TextBox { Font = UITheme.FontBody, Location = new Point(155, 245), Size = new Size(135, 25) };

            lblEmail = new Label { Text = "Email:", Font = UITheme.FontBodyBold, Location = new Point(15, 275), AutoSize = true };
            txtEmail = new TextBox { Font = UITheme.FontBody, Location = new Point(15, 295), Size = new Size(180, 25) };

            chkIsActive = new CheckBox { Text = "Kích hoạt", Font = UITheme.FontBodyBold, Location = new Point(205, 297), AutoSize = true, Checked = true };

            grbDetails.Controls.AddRange(new Control[] {
                lblId, txtId,
                lblUsername, txtUsername,
                lblPassword, txtPassword,
                lblFullName, txtFullName,
                lblRole, cboRole,
                lblPhone, txtPhone,
                lblEmail, txtEmail,
                chkIsActive
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

            btnAdd = new Button { Text = "➕ Thêm User", Size = new Size(btnWidth, btnHeight), Margin = new Padding(4) };
            UITheme.ApplyButtonTheme(btnAdd, UITheme.ButtonStyle.Success);
            btnAdd.Click += async (s, e) => await CreateUserAsync();

            btnUpdate = new Button { Text = "✏ Cập Nhật", Size = new Size(btnWidth, btnHeight), Margin = new Padding(4) };
            UITheme.ApplyButtonTheme(btnUpdate, UITheme.ButtonStyle.Warning);
            btnUpdate.Click += async (s, e) => await UpdateUserAsync();

            btnDelete = new Button { Text = "🔒 Khóa/Xóa", Size = new Size(btnWidth, btnHeight), Margin = new Padding(4) };
            UITheme.ApplyButtonTheme(btnDelete, UITheme.ButtonStyle.Danger);
            btnDelete.Click += async (s, e) => await DeleteUserAsync();

            btnClear = new Button { Text = "🔄 Làm Mới", Size = new Size(btnWidth, btnHeight), Margin = new Padding(4) };
            UITheme.ApplyButtonTheme(btnClear, UITheme.ButtonStyle.Secondary);
            btnClear.Click += (s, e) => ClearForm();

            panelButtons.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnClear });
            panelRight.Controls.Add(panelButtons);
            panelButtons.BringToFront();

            this.Controls.Add(dgvUsers);
            this.Controls.Add(panelTop);
            this.Controls.Add(panelRight);
        }

        private void ApplyTheme()
        {
            UITheme.ApplyFormTheme(this);
            UITheme.ApplyGridTheme(dgvUsers);
        }

        private async void FormUserManagement_Load(object sender, EventArgs e)
        {
            await LoadUsersAsync();
        }

        // ==========================================
        // GỌI API LẤY DANH SÁCH USER: GET /api/Users
        // ==========================================
        private async Task LoadUsersAsync(string keyword = null)
        {
            lblStatus.Text = "Đang tải dữ liệu...";
            try
            {
                string endpoint = "Users";
                if (!string.IsNullOrWhiteSpace(keyword) && keyword != "Tên đăng nhập hoặc Họ tên...")
                {
                    endpoint += $"?keyword={Uri.EscapeDataString(keyword)}";
                }

                var response = await SessionManager.ApiClientService.Client.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<List<UserDto>>();
                    _users.Clear();
                    if (data != null) _users.AddRange(data);

                    DisplayUsers(_users);
                    lblStatus.Text = $"Có {_users.Count} tài khoản";
                }
                else
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(response, "Lấy danh sách tài khoản");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối API: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Lỗi kết nối API";
            }
        }

        private void DisplayUsers(List<UserDto> list)
        {
            dgvUsers.Rows.Clear();
            foreach (var u in list)
            {
                int idx = dgvUsers.Rows.Add();
                var row = dgvUsers.Rows[idx];
                row.Cells[0].Value = u.UserId;
                row.Cells[1].Value = u.Username;
                row.Cells[2].Value = u.FullName;
                row.Cells[3].Value = u.Role;
                row.Cells[4].Value = u.Email;
                row.Cells[5].Value = u.Phone;
                row.Cells[6].Value = u.IsActive ? "Hoạt động" : "Bị khóa";
            }

            if (list.Count > 0)
            {
                dgvUsers.ClearSelection();
                dgvUsers.Rows[0].Selected = true;
                ShowUser(0);
            }
            else
            {
                ClearForm();
            }
        }

        private void DgvUsers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            ShowUser(e.RowIndex);
        }

        private void ShowUser(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= _users.Count) return;
            var u = _users[rowIndex];

            txtId.Text = u.UserId.ToString();
            txtUsername.Text = u.Username;
            txtUsername.ReadOnly = true; // Không sửa username của tài khoản đã tạo
            txtPassword.Clear();
            txtFullName.Text = u.FullName;
            txtEmail.Text = u.Email ?? "";
            txtPhone.Text = u.Phone ?? "";
            chkIsActive.Checked = u.IsActive;

            string r = (u.Role ?? "CASHIER").ToUpper();
            if (cboRole.Items.Contains(r)) cboRole.SelectedItem = r;
            else cboRole.SelectedIndex = 2;
        }

        private void ClearForm()
        {
            txtId.Clear();
            txtUsername.Clear();
            txtUsername.ReadOnly = false;
            txtPassword.Clear();
            txtFullName.Clear();
            txtEmail.Clear();
            txtPhone.Clear();
            cboRole.SelectedIndex = 2;
            chkIsActive.Checked = true;
            txtUsername.Focus();
        }

        private async void BtnSearch_Click(object sender, EventArgs e)
        {
            await LoadUsersAsync(txtSearch.Text.Trim());
        }

        private async void BtnReload_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "Tên đăng nhập hoặc Họ tên...";
            txtSearch.ForeColor = UITheme.TextMuted;
            await LoadUsersAsync();
        }

        // ==========================================
        // THÊM USER: POST /api/Users
        // ==========================================
        private async Task CreateUserAsync()
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Vui lòng nhập Tên đăng nhập và Mật khẩu!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var newUser = new
            {
                Username = txtUsername.Text.Trim(),
                Password = txtPassword.Text.Trim(),
                FullName = string.IsNullOrWhiteSpace(txtFullName.Text) ? txtUsername.Text.Trim() : txtFullName.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Role = cboRole.SelectedItem?.ToString() ?? "CASHIER",
                IsActive = chkIsActive.Checked
            };

            try
            {
                var response = await SessionManager.ApiClientService.Client.PostAsJsonAsync("Users", newUser);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Tạo tài khoản người dùng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    await LoadUsersAsync();
                }
                else
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(response, "Thêm tài khoản");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối API: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========================================
        // SỬA USER: PUT /api/Users/{id}
        // ==========================================
        private async Task UpdateUserAsync()
        {
            if (!int.TryParse(txtId.Text, out int id))
            {
                MessageBox.Show("Vui lòng chọn tài khoản cần cập nhật!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var updateUser = new
            {
                FullName = txtFullName.Text.Trim(),
                Password = string.IsNullOrWhiteSpace(txtPassword.Text) ? null : txtPassword.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Role = cboRole.SelectedItem?.ToString() ?? "CASHIER",
                IsActive = chkIsActive.Checked
            };

            try
            {
                var response = await SessionManager.ApiClientService.Client.PutAsJsonAsync($"Users/{id}", updateUser);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Cập nhật thông tin tài khoản thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadUsersAsync();
                }
                else
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(response, "Cập nhật tài khoản");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối API: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========================================
        // XÓA / KHÓA USER: DELETE /api/Users/{id}
        // ==========================================
        private async Task DeleteUserAsync()
        {
            if (!int.TryParse(txtId.Text, out int id))
            {
                MessageBox.Show("Vui lòng chọn tài khoản cần xóa / khóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa hoặc khóa tài khoản '{txtUsername.Text}'?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                var response = await SessionManager.ApiClientService.Client.DeleteAsync($"Users/{id}");
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Thao tác thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    await LoadUsersAsync();
                }
                else
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(response, "Khóa / Xóa tài khoản");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối API: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
