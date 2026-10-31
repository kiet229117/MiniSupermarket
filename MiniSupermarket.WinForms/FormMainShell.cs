using System;
using System.Drawing;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public partial class FormMainShell : Form
    {
        private Form _activeForm = null;
        private FormPOS _cachedFormPOS = null;

        // Khai báo controls tương ứng trong FormMainShell.Designer.cs
        private Label lblUserInfo;
        private Panel panelMainContent;
        private Label lblTitle;
        private Panel panelSidebar;
        private Button btnLogout;
        private Button btnDashboard;
        private Button btnPOS;

        private Button btnOrder;
        private Button btnBrand;
        private Button btnSupplier;
        private Button btnCategory;
        private Button btnProduct;
        private Button btnCustomer;
        private Button btnReports;
        private Button btnUserManage;

        public FormMainShell()
        {
            InitializeComponent();
            if (panelSidebar != null)
            {
                panelSidebar.Resize += (s, e) => RelayoutLogoutButton();
            }
            // Đảm bảo khi Form đã hiển thị (Shown) thì dồn lại một lần nữa để chắc chắn 100%
            this.Shown += (s, e) => ApplyRolePermissions(SessionManager.CurrentRole);
        }

        private void FormMainShell_Load(object sender, EventArgs e)
        {
            // 1. Hiển thị thông tin phiên làm việc
            lblUserInfo.Text = $"👤 {SessionManager.CurrentUsername ?? "User"} | Quyền: [{SessionManager.CurrentRole ?? "N/A"}]";

            // 2. Kích hoạt phân quyền giao diện và dồn nút ngay từ lúc nạp
            ApplyRolePermissions(SessionManager.CurrentRole);

            // 3. Mở màn hình mặc định tương ứng với vai trò
            OpenDefaultScreenByRole(SessionManager.CurrentRole);
        }

        /// <summary>
        /// Phân định quyền truy cập hiển thị theo vai trò người dùng và dồn nút liền mạch không để lại khoảng trống
        /// </summary>
        private void ApplyRolePermissions(string role)
        {
            string r = (role ?? "").ToUpper();

            // Danh sách toàn bộ 10 nút chức năng của hệ thống
            var allButtons = new Button[] {
                btnDashboard,
                btnPOS,
                btnOrder,
                btnProduct,
                btnCategory,
                btnBrand,
                btnSupplier,
                btnCustomer,
                btnReports,
                btnUserManage
            };

            Button[] activeButtons;
            switch (r)
            {
                case "CASHIER":
                    // Thu ngân: Chỉ hiển thị 3 nút POS, Đơn hàng, Khách hàng dồn sát nhau trên cùng
                    activeButtons = new Button[] {
                        btnPOS,
                        btnOrder,
                        btnCustomer
                    };
                    break;

                case "WAREHOUSE":
                    // Thủ kho: Chỉ hiển thị 4 nút Kho hàng, dồn sát nhau trên cùng
                    activeButtons = new Button[] {
                        btnProduct,
                        btnCategory,
                        btnBrand,
                        btnSupplier
                    };
                    break;

                case "ADMIN":
                    // Quản trị viên: Toàn quyền hiển thị cả 10 nút
                    activeButtons = allButtons;
                    break;

                default:
                    MessageBox.Show("Tài khoản chưa được cấp vai trò hợp lệ trong hệ thống!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.Close();
                    return;
            }

            // 1. Ẩn tất cả các nút không thuộc vai trò
            foreach (var btn in allButtons)
            {
                if (btn != null && Array.IndexOf(activeButtons, btn) < 0)
                {
                    btn.Visible = false;
                }
            }

            // 2. Xếp dồn các nút của vai trò từ trên xuống dưới (KHÔNG check btn.Visible vì lúc Load nó luôn là false)
            int topPos = 20;
            foreach (var btn in activeButtons)
            {
                if (btn != null)
                {
                    btn.Visible = true;
                    btn.Top = topPos;
                    topPos += 50; // Chiều cao nút 42 + khoảng cách 8
                }
            }

            // 3. Neo nút Đăng xuất ở đáy thanh sidebar
            RelayoutLogoutButton();
        }

        /// <summary>
        /// Neo cố định nút Đăng xuất ở đáy thanh Sidebar, ngay phía trên thông tin tài khoản
        /// </summary>
        private void RelayoutLogoutButton()
        {
            if (btnLogout != null && panelSidebar != null)
            {
                btnLogout.Visible = true;
                int bottomPadding = (lblUserInfo != null && lblUserInfo.Visible) ? lblUserInfo.Height + 12 : 16;
                btnLogout.Top = Math.Max(180, panelSidebar.ClientSize.Height - btnLogout.Height - bottomPadding);
                btnLogout.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            }
        }

        /// <summary>
        /// Điều hướng vào màn hình làm việc mặc định theo vai trò
        /// </summary>
        private void OpenDefaultScreenByRole(string role)
        {
            string r = (role ?? "").ToUpper();
            switch (r)
            {
                case "ADMIN":
                    if (_cachedFormPOS == null || _cachedFormPOS.IsDisposed) _cachedFormPOS = new FormPOS();
                    OpenChildForm(_cachedFormPOS, "BÁN HÀNG & QUÉT MÃ VẠCH (POS)", btnPOS);
                    break;
                case "WAREHOUSE":
                    OpenChildForm(new FormProductManagement(), "QUẢN LÝ SẢN PHẨM & KHO HÀNG", btnProduct);
                    break;
                case "CASHIER":
                    if (_cachedFormPOS == null || _cachedFormPOS.IsDisposed) _cachedFormPOS = new FormPOS();
                    OpenChildForm(_cachedFormPOS, "BÁN HÀNG & QUÉT MÃ VẠCH (POS)", btnPOS);
                    break;
            }
        }

        // ================= CÁC SỰ KIỆN CLICK NÚT TRÊN SIDEBAR =================

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            OpenChildForm(new FormDashboard(), "DASHBOARD QUẢN TRỊ & HIỆU SUẤT", btnDashboard);
        }

        private void btnPOS_Click(object sender, EventArgs e)
        {
            if (_cachedFormPOS == null || _cachedFormPOS.IsDisposed)
            {
                _cachedFormPOS = new FormPOS();
            }
            OpenChildForm(_cachedFormPOS, "BÁN HÀNG & QUÉT MÃ VẠCH (POS)", btnPOS);
        }

        private void btnProduct_Click(object sender, EventArgs e)
        {
            OpenChildForm(new FormProductManagement(), "QUẢN LÝ SẢN PHẨM & KHO HÀNG", btnProduct);
        }

        private void btnCategory_Click(object sender, EventArgs e)
        {
            OpenChildForm(new FormCategoryManagement(), "QUẢN LÝ DANH MỤC NHÓM HÀNG", btnCategory);
        }

        private void btnOrder_Click(object sender, EventArgs e)
        {
            OpenChildForm(new FormOrderManagement(), "QUẢN LÝ ĐƠN HÀNG", btnOrder);
        }

        private void btnBrand_Click(object sender, EventArgs e)
        {
            OpenChildForm(new FormBrandManagement(), "QUẢN LÝ THƯƠNG HIỆU", btnBrand);
        }

        private void btnSupplier_Click(object sender, EventArgs e)
        {
            OpenChildForm(new FormSupplierManagement(), "QUẢN LÝ NHÀ CUNG CẤP", btnSupplier);
        }

        private void btnCustomer_Click(object sender, EventArgs e)
        {
            OpenChildForm(new FormCustomerManagement(), "QUẢN LÝ KHÁCH HÀNG & THÀNH VIÊN", btnCustomer);
        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            if (string.Equals(SessionManager.CurrentRole, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                OpenChildForm(new FormQuickReport(), "BÁO CÁO DOANH THU & HIỆU SUẤT", btnReports);
            }
            else
            {
                MessageBox.Show("Chức năng xem báo cáo tài chính chỉ dành riêng cho Quản trị viên (Admin)!", "Từ chối", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private void btnUser_Click(object sender, EventArgs e)
        {
            if (string.Equals(SessionManager.CurrentRole, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                OpenChildForm(new FormUserManagement(), "QUẢN TRỊ TÀI KHOẢN NGƯỜI DÙNG", btnUserManage);
            }
            else
            {
                MessageBox.Show("Chức năng quản lý tài khoản người dùng chỉ dành cho Quản trị viên (Admin)!", "Từ chối", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất phiên làm việc hiện tại?", "Xác nhận đăng xuất", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                SessionManager.Logout();
                if (_cachedFormPOS != null && !_cachedFormPOS.IsDisposed)
                {
                    _cachedFormPOS.Dispose();
                    _cachedFormPOS = null;
                }

                this.Hide();
                using (FormLogin formLogin = new FormLogin())
                {
                    if (formLogin.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(SessionManager.JwtToken))
                    {
                        // Đăng nhập lại với tài khoản mới -> làm mới giao diện
                        lblUserInfo.Text = $"👤 {SessionManager.CurrentUsername} | Quyền: [{SessionManager.CurrentRole}]";
                        ApplyRolePermissions(SessionManager.CurrentRole);
                        OpenDefaultScreenByRole(SessionManager.CurrentRole);
                        this.Show();
                    }
                    else
                    {
                        this.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Hàm nhúng động một Form con vào panelMainContent
        /// </summary>
        private void OpenChildForm(Form childForm, string screenTitle, Button senderButton)
        {
            if (_activeForm == childForm)
            {
                HighlightActiveButton(senderButton);
                return;
            }

            if (_activeForm != null)
            {
                if (_activeForm == _cachedFormPOS)
                {
                    panelMainContent.Controls.Remove(_activeForm);
                    _activeForm.Hide();
                }
                else
                {
                    _activeForm.Close();
                }
            }

            HighlightActiveButton(senderButton);

            _activeForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            if (!panelMainContent.Controls.Contains(childForm))
            {
                panelMainContent.Controls.Add(childForm);
            }
            panelMainContent.Tag = childForm;

            lblTitle.Text = screenTitle;
            childForm.BringToFront();
            childForm.Show();
        }

        /// <summary>
        /// Làm nổi bật nút menu sidebar đang được chọn
        /// </summary>
        private void HighlightActiveButton(Button activeButton)
        {
            foreach (Control ctrl in panelSidebar.Controls)
            {
                if (ctrl is Button btn && btn != btnLogout)
                {
                    btn.BackColor = Color.FromArgb(24, 30, 48);
                    btn.ForeColor = Color.White;
                }
            }
            if (activeButton != null)
            {
                activeButton.BackColor = Color.FromArgb(37, 99, 235); // Vibrant Royal Blue
                activeButton.ForeColor = Color.White;
            }
        }
    }
}
