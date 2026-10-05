using System;
using System.Drawing;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
            ApplyCustomStyling();
        }

        private void ApplyCustomStyling()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Text = "ĐĂNG NHẬP - HỆ THỐNG BÁCH HÓA THỰC PHẨM HEALTHY MART";
            this.BackColor = UITheme.FormBackground;

            groupBox1.Text = "THÔNG TIN ĐĂNG NHẬP";
            groupBox1.Font = UITheme.FontSubtitle;
            groupBox1.ForeColor = UITheme.TextPrimary;

            label1.Font = UITheme.FontBodyBold;
            label2.Font = UITheme.FontBodyBold;

            txtUser.Font = UITheme.FontBody;
            txtPassword.Font = UITheme.FontBody;

            UITheme.ApplyButtonTheme(btnLogin, UITheme.ButtonStyle.Primary);
            btnLogin.Text = "ĐĂNG NHẬP HỆ THỐNG";

            // Nhấn Enter ở ô mật khẩu sẽ tự động đăng nhập
            txtPassword.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    btnLogin.PerformClick();
                }
            };
            txtUser.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    txtPassword.Focus();
                }
            };
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUser.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Tên đăng nhập và Mật khẩu!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnLogin.Enabled = false;
            btnLogin.Text = "Đang xác thực...";

            try
            {
                bool success = await SessionManager.ApiClientService.LoginAsync(username, password);

                if (success)
                {
                    MessageBox.Show($"Đăng nhập thành công!\nChào mừng: {SessionManager.CurrentUsername} [{SessionManager.CurrentRole}]", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Sai tài khoản hoặc mật khẩu! Vui lòng thử lại.", "Đăng nhập thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = "ĐĂNG NHẬP HỆ THỐNG";
            }
        }
    }
}

