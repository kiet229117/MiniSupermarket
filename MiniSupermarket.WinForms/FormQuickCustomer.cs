using System;
using System.Drawing;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public class FormQuickCustomer : Form
    {
        private TextBox txtPhone;
        private TextBox txtName;
        private TextBox txtAddress;
        private Button btnSave;
        private Button btnCancel;
        private Label lblError;

        public CustomerDto CreatedCustomer { get; private set; }

        public FormQuickCustomer(string initialPhone = "")
        {
            InitializeUI(initialPhone);
        }

        private void InitializeUI(string initialPhone)
        {
            this.Text = "➕ ĐĂNG KÝ NHANH KHÁCH HÀNG MỚI";
            this.Size = new Size(460, 360);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = UITheme.FormBackground;
            this.KeyPreview = true;

            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
            };

            // 1. Header Panel
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 62,
                BackColor = Color.White,
                Padding = new Padding(20, 10, 20, 10)
            };

            Label lblTitle = new Label
            {
                Text = "ĐĂNG KÝ THÀNH VIÊN TẠI QUẦY POS",
                Font = UITheme.FontSubtitle,
                ForeColor = UITheme.PrimaryColor,
                Location = new Point(18, 10),
                AutoSize = true
            };

            Label lblSub = new Label
            {
                Text = "Khách hàng mới sẽ được cấp hồ sơ và tích điểm ngay cho đơn này",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.TextMuted,
                Location = new Point(18, 34),
                AutoSize = true
            };

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSub });

            // 2. Body Panel (Inputs)
            Panel pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24, 16, 24, 16),
                BackColor = UITheme.FormBackground
            };

            Label lblPhone = new Label
            {
                Text = "Số điện thoại (*):",
                Font = UITheme.FontBodyBold,
                ForeColor = UITheme.TextPrimary,
                Location = new Point(24, 16),
                AutoSize = true
            };

            txtPhone = new TextBox
            {
                Text = initialPhone ?? string.Empty,
                Font = UITheme.FontBody,
                Location = new Point(24, 40),
                Size = new Size(392, 26)
            };

            Label lblName = new Label
            {
                Text = "Họ và tên khách (*):",
                Font = UITheme.FontBodyBold,
                ForeColor = UITheme.TextPrimary,
                Location = new Point(24, 76),
                AutoSize = true
            };

            txtName = new TextBox
            {
                Font = UITheme.FontBody,
                Location = new Point(24, 100),
                Size = new Size(392, 26)
            };

            Label lblAddress = new Label
            {
                Text = "Địa chỉ (tuỳ chọn):",
                Font = UITheme.FontBodyBold,
                ForeColor = UITheme.TextPrimary,
                Location = new Point(24, 136),
                AutoSize = true
            };

            txtAddress = new TextBox
            {
                Text = "Tại quầy",
                Font = UITheme.FontBody,
                Location = new Point(24, 160),
                Size = new Size(392, 26)
            };

            lblError = new Label
            {
                Text = "",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.DangerColor,
                Location = new Point(24, 196),
                AutoSize = true
            };

            // Enter key handling
            txtPhone.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) txtName.Focus(); };
            txtName.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) txtAddress.Focus(); };
            txtAddress.KeyDown += async (s, e) => { if (e.KeyCode == Keys.Enter) await SaveCustomerAsync(); };

            pnlBody.Controls.AddRange(new Control[] {
                lblPhone, txtPhone,
                lblName, txtName,
                lblAddress, txtAddress,
                lblError
            });

            // 3. Bottom Button Panel
            Panel pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                BackColor = Color.White,
                Padding = new Padding(15, 10, 20, 10)
            };

            btnCancel = new Button
            {
                Text = "Hủy bỏ (ESC)",
                Size = new Size(110, 34),
                Location = new Point(190, 11),
                Cursor = Cursors.Hand
            };
            UITheme.ApplyButtonTheme(btnCancel, UITheme.ButtonStyle.Secondary);
            btnCancel.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            btnSave = new Button
            {
                Text = "💾 Lưu & Chọn ngay",
                Size = new Size(140, 34),
                Location = new Point(308, 11),
                Cursor = Cursors.Hand
            };
            UITheme.ApplyButtonTheme(btnSave, UITheme.ButtonStyle.Primary);
            btnSave.Click += async (s, e) => await SaveCustomerAsync();

            pnlBottom.Controls.AddRange(new Control[] { btnCancel, btnSave });

            this.Controls.Add(pnlBody);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(pnlHeader);

            this.Shown += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtPhone.Text))
                    txtPhone.Focus();
                else
                    txtName.Focus();
            };
        }

        private async Task SaveCustomerAsync()
        {
            lblError.Text = "";
            string phone = txtPhone.Text.Trim();
            string name = txtName.Text.Trim();
            string address = txtAddress.Text.Trim();

            if (string.IsNullOrWhiteSpace(phone))
            {
                lblError.Text = "Vui lòng nhập số điện thoại!";
                txtPhone.Focus();
                return;
            }

            if (phone.Length < 9)
            {
                lblError.Text = "Số điện thoại không hợp lệ (tối thiểu 9 số)!";
                txtPhone.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                lblError.Text = "Vui lòng nhập họ và tên khách hàng!";
                txtName.Focus();
                return;
            }

            btnSave.Enabled = false;
            btnCancel.Enabled = false;
            btnSave.Text = "Đang lưu...";

            try
            {
                var newCustomer = new CustomerDto
                {
                    CustomerName = name,
                    PhoneNumber = phone,
                    Address = string.IsNullOrWhiteSpace(address) ? "Tại quầy" : address,
                    RewardPoints = 0,
                    MembershipRank = "Chuẩn"
                };

                var response = await SessionManager.ApiClientService.Client.PostAsJsonAsync("Customers", newCustomer);

                if (response.IsSuccessStatusCode)
                {
                    CreatedCustomer = await response.Content.ReadFromJsonAsync<CustomerDto>();
                    if (CreatedCustomer == null)
                    {
                        CreatedCustomer = newCustomer;
                    }
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    // Đã tồn tại khách hàng với SĐT này -> tự động tra cứu lại
                    var getResp = await SessionManager.ApiClientService.Client.GetAsync($"Customers/phone/{phone}");
                    if (getResp.IsSuccessStatusCode)
                    {
                        CreatedCustomer = await getResp.Content.ReadFromJsonAsync<CustomerDto>();
                        MessageBox.Show($"Số điện thoại {phone} đã có tài khoản: {CreatedCustomer?.CustomerName}.\nHệ thống sẽ tự động chọn khách hàng này cho đơn hiện tại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        lblError.Text = "Số điện thoại đã được đăng ký trước đó!";
                    }
                }
                else
                {
                    string errContent = await response.Content.ReadAsStringAsync();
                    lblError.Text = string.IsNullOrWhiteSpace(errContent) ? "Lỗi không xác định khi lưu khách hàng" : errContent;
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "Lỗi kết nối: " + ex.Message;
            }
            finally
            {
                btnSave.Enabled = true;
                btnCancel.Enabled = true;
                btnSave.Text = "💾 Lưu & Chọn ngay";
            }
        }
    }
}
