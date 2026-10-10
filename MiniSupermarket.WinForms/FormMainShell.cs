using System;
using System.Drawing;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public partial class FormMainShell : Form
    {
        private Form _activeForm = null;

        // Khai báo controls tương ứng trong FormMainShell.Designer.cs
        private Label lblUserInfo;
        private Panel panelMainContent;
        private Label lblTitle;
        private Panel panelSidebar;
        private Button btnLogout;
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
        }

        private void FormMainShell_Load(object sender, EventArgs e)
        {
            // 1. Hiển thị thông tin phiên làm việc
            lblUserInfo.Text = $"👤 {SessionManager.CurrentUsername ?? "User"} | Quyền: [{SessionManager.CurrentRole ?? "N/A"}]";

            // 2. Kích hoạt phân quyền giao diện theo vai trò (Role-Based Access)
            ApplyRolePermissions(SessionManager.CurrentRole);

            // 3. Mở màn hình mặc định tương ứng với vai trò
            OpenDefaultScreenByRole(SessionManager.CurrentRole);
        }

        /// <summary>
        /// Phân định quyền truy cập hiển thị theo vai trò người dùng
        /// </summary>
        private void ApplyRolePermissions(string role)
        {
            string r = (role ?? "").ToUpper();
            switch (r)
            {
                case "ADMIN":
                    btnPOS.Visible = true;
                    btnOrder.Visible = true;
                    btnProduct.Visible = true;
                    btnCategory.Visible = true;
                    btnBrand.Visible = true;
                    btnSupplier.Visible = true;
                    btnCustomer.Visible = true;
                    btnReports.Visible = true;
                    btnUserManage.Visible = true;
                    break;

                case "CASHIER":
                    btnPOS.Visible = true;
                    btnOrder.Visible = true;
                    btnCustomer.Visible = true;
                    btnProduct.Visible = false;
                    btnCategory.Visible = false;
                    btnBrand.Visible = false;
                    btnSupplier.Visible = false;
                    btnReports.Visible = false;
                    btnUserManage.Visible = false;
                    break;

                case "WAREHOUSE":
                    btnPOS.Visible = false;
                    btnOrder.Visible = false;
                    btnCustomer.Visible = false;
                    btnReports.Visible = false;
                    btnUserManage.Visible = false;
                    btnCategory.Visible = true;
                    btnProduct.Visible = true;
                    btnBrand.Visible = true;
                    btnSupplier.Visible = true;
                    break;

                default:
                    MessageBox.Show("Tài khoản chưa được cấp vai trò hợp lệ trong hệ thống!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.Close();
                    break;
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
                    OpenChildForm(new FormPOS(), "BÁN HÀNG & QUÉT MÃ VẠCH (POS)", btnPOS);
                    break;
                case "WAREHOUSE":
                    OpenChildForm(new FormProductManagement(), "QUẢN LÝ SẢN PHẨM & KHO HÀNG", btnProduct);
                    break;
                case "CASHIER":
                    OpenChildForm(new FormPOS(), "BÁN HÀNG & QUÉT MÃ VẠCH (POS)", btnPOS);
                    break;
            }
        }

        // ================= CÁC SỰ KIỆN CLICK NÚT TRÊN SIDEBAR =================

        private void btnPOS_Click(object sender, EventArgs e)
        {
            OpenChildForm(new FormPOS(), "BÁN HÀNG & QUÉT MÃ VẠCH (POS)", btnPOS);
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
            if (_activeForm != null)
            {
                _activeForm.Close();
            }

            HighlightActiveButton(senderButton);

            _activeForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            panelMainContent.Controls.Clear();
            panelMainContent.Controls.Add(childForm);
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
