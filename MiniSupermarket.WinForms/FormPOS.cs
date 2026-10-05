using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public partial class FormPOS : Form
    {
        private readonly List<CartItemDto> _cart = new List<CartItemDto>();
        private CustomerDto _currentCustomer = null;

        // Controls Top
        private Panel panelTop;
        private Label lblScanTitle;
        private TextBox txtBarcode;
        private Button btnAddBarcode;
        private Button btnRemoveItem;
        private Button btnClearCart;

        // DataGridView Cart
        private DataGridView dgvCart;

        // Payment Panel Right
        private Panel panelRight;
        private GroupBox grbCustomer;
        private Label lblPhone;
        private TextBox txtCustomerPhone;
        private Button btnFindCustomer;
        private Label lblCustomerName;
        private Label lblCustomerPoints;

        private GroupBox grbPayment;
        private Label lblSubtotalTitle;
        private Label lblSubtotal;
        private Label lblDiscountTitle;
        private NumericUpDown nudDiscount;
        private Label lblPaymentMethodTitle;
        private ComboBox cboPaymentMethod;
        private Label lblTotalTitle;
        private Label lblTotalAmount;
        private Label lblCashTitle;
        private TextBox txtCashReceived;
        private Label lblChangeTitle;
        private Label lblChange;
        private Button btnCheckout;

        public FormPOS()
        {
            InitializeComponent();
            BuildUI();
            ApplyTheme();
        }

        private void BuildUI()
        {
            this.Text = "BÁN HÀNG & QUÉT MÃ VẠCH (POS)";
            this.Size = new Size(1100, 650);

            // 1. Panel Top (Quét Barcode)
            panelTop = new Panel { Dock = DockStyle.Top, Height = 65, BackColor = Color.White };

            lblScanTitle = new Label
            {
                Text = "Quét mã vạch (Barcode):",
                Font = UITheme.FontBodyBold,
                Location = new Point(16, 20),
                AutoSize = true
            };
            panelTop.Controls.Add(lblScanTitle);

            txtBarcode = new TextBox
            {
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(180, 16),
                Size = new Size(260, 27)
            };
            txtBarcode.KeyDown += async (s, e) =>
            {
                if (e.KeyCode == Keys.Enter && !string.IsNullOrWhiteSpace(txtBarcode.Text))
                {
                    string code = txtBarcode.Text.Trim();
                    txtBarcode.Clear();
                    await AddProductToCartByBarcodeAsync(code);
                }
            };
            panelTop.Controls.Add(txtBarcode);

            btnAddBarcode = new Button
            {
                Text = "Thêm SP",
                Location = new Point(450, 15),
                Size = new Size(85, 30)
            };
            UITheme.ApplyButtonTheme(btnAddBarcode, UITheme.ButtonStyle.Primary);
            btnAddBarcode.Click += async (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(txtBarcode.Text))
                {
                    string code = txtBarcode.Text.Trim();
                    txtBarcode.Clear();
                    await AddProductToCartByBarcodeAsync(code);
                }
            };
            panelTop.Controls.Add(btnAddBarcode);

            btnRemoveItem = new Button
            {
                Text = "Xóa dòng chọn",
                Location = new Point(545, 15),
                Size = new Size(115, 30)
            };
            UITheme.ApplyButtonTheme(btnRemoveItem, UITheme.ButtonStyle.Warning);
            btnRemoveItem.Click += (s, e) => RemoveSelectedItem();
            panelTop.Controls.Add(btnRemoveItem);

            btnClearCart = new Button
            {
                Text = "Hủy toàn bộ giỏ",
                Location = new Point(670, 15),
                Size = new Size(125, 30)
            };
            UITheme.ApplyButtonTheme(btnClearCart, UITheme.ButtonStyle.Danger);
            btnClearCart.Click += (s, e) => ClearCart();
            panelTop.Controls.Add(btnClearCart);

            // 2. DataGridView Giỏ hàng
            dgvCart = new DataGridView { Dock = DockStyle.Fill };
            dgvCart.Columns.Add("ProductId", "Mã SP");
            dgvCart.Columns.Add("Barcode", "Mã Vạch");
            dgvCart.Columns.Add("ProductName", "Tên Sản Phẩm");
            dgvCart.Columns.Add("Unit", "ĐVT");
            dgvCart.Columns.Add("UnitPrice", "Đơn Giá");
            dgvCart.Columns.Add("Quantity", "Số Lượng");
            dgvCart.Columns.Add("TotalPrice", "Thành Tiền");

            dgvCart.Columns["ProductId"].Width = 65;
            dgvCart.Columns["Barcode"].Width = 120;
            dgvCart.Columns["ProductName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvCart.Columns["Unit"].Width = 60;
            dgvCart.Columns["UnitPrice"].Width = 100;
            dgvCart.Columns["Quantity"].Width = 80;
            dgvCart.Columns["TotalPrice"].Width = 120;

            dgvCart.Columns["UnitPrice"].DefaultCellStyle.Format = "N0";
            dgvCart.Columns["Quantity"].DefaultCellStyle.Format = "N0";
            dgvCart.Columns["TotalPrice"].DefaultCellStyle.Format = "N0";

            // 3. Panel Bên phải (Thanh toán & Khách hàng)
            panelRight = new Panel
            {
                Dock = DockStyle.Right,
                Width = 360,
                BackColor = Color.White,
                Padding = new Padding(12)
            };

            // GroupBox Khách hàng
            grbCustomer = new GroupBox
            {
                Text = "Khách hàng & Tích điểm",
                Font = UITheme.FontSubtitle,
                ForeColor = UITheme.TextPrimary,
                Dock = DockStyle.Top,
                Height = 135
            };
            panelRight.Controls.Add(grbCustomer);

            lblPhone = new Label { Text = "SĐT:", Font = UITheme.FontBodyBold, Location = new Point(15, 25), AutoSize = true };
            txtCustomerPhone = new TextBox { Font = UITheme.FontBody, Location = new Point(55, 22), Size = new Size(170, 25) };
            txtCustomerPhone.KeyDown += async (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) await LookupCustomerAsync();
            };

            btnFindCustomer = new Button { Text = "Tìm", Location = new Point(235, 21), Size = new Size(60, 27) };
            UITheme.ApplyButtonTheme(btnFindCustomer, UITheme.ButtonStyle.Primary);
            btnFindCustomer.Click += async (s, e) => await LookupCustomerAsync();

            lblCustomerName = new Label
            {
                Text = "Khách vãng lai",
                Font = UITheme.FontBodyBold,
                ForeColor = UITheme.PrimaryColor,
                Location = new Point(15, 60),
                AutoSize = true
            };

            lblCustomerPoints = new Label
            {
                Text = "Điểm tích lũy: 0 | Hạng: Mới",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.TextMuted,
                Location = new Point(15, 90),
                AutoSize = true
            };

            grbCustomer.Controls.AddRange(new Control[] {
                lblPhone, txtCustomerPhone, btnFindCustomer,
                lblCustomerName, lblCustomerPoints
            });

            // GroupBox Thanh toán
            grbPayment = new GroupBox
            {
                Text = "Chi tiết thanh toán",
                Font = UITheme.FontSubtitle,
                ForeColor = UITheme.TextPrimary,
                Dock = DockStyle.Fill
            };
            panelRight.Controls.Add(grbPayment);
            grbPayment.BringToFront();

            int top = 30;
            lblSubtotalTitle = new Label { Text = "Tổng tiền hàng:", Font = UITheme.FontBodyBold, Location = new Point(15, top), AutoSize = true };
            lblSubtotal = new Label { Text = "0 đ", Font = UITheme.FontBodyBold, Location = new Point(180, top), AutoSize = true };
            top += 35;

            lblDiscountTitle = new Label { Text = "Giảm giá (VNĐ):", Font = UITheme.FontBodyBold, Location = new Point(15, top), AutoSize = true };
            nudDiscount = new NumericUpDown
            {
                Font = UITheme.FontBody,
                Location = new Point(180, top - 3),
                Size = new Size(130, 25),
                Maximum = 100000000,
                Increment = 1000,
                ThousandsSeparator = true
            };
            nudDiscount.ValueChanged += (s, e) => UpdateCalculations();
            top += 35;

            lblPaymentMethodTitle = new Label { Text = "Phương thức:", Font = UITheme.FontBodyBold, Location = new Point(15, top), AutoSize = true };
            cboPaymentMethod = new ComboBox
            {
                Font = UITheme.FontBody,
                Location = new Point(180, top - 3),
                Size = new Size(130, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboPaymentMethod.Items.AddRange(new object[] { "Tiền mặt (CASH)", "Chuyển khoản (BANK_TRANSFER)", "Ví MoMo (MOMO)", "Thẻ (CARD)" });
            cboPaymentMethod.SelectedIndex = 0;
            top += 40;

            lblTotalTitle = new Label { Text = "TỔNG THANH TOÁN:", Font = new Font("Segoe UI", 11F, FontStyle.Bold), Location = new Point(15, top), AutoSize = true };
            lblTotalAmount = new Label
            {
                Text = "0 đ",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.Red,
                Location = new Point(180, top - 3),
                AutoSize = true
            };
            top += 45;

            lblCashTitle = new Label { Text = "Tiền khách đưa:", Font = UITheme.FontBodyBold, Location = new Point(15, top), AutoSize = true };
            txtCashReceived = new TextBox { Font = new Font("Segoe UI", 10F, FontStyle.Bold), Location = new Point(180, top - 3), Size = new Size(130, 25) };
            txtCashReceived.TextChanged += (s, e) => CalculateChange();
            top += 35;

            lblChangeTitle = new Label { Text = "Tiền thừa trả khách:", Font = UITheme.FontBodyBold, Location = new Point(15, top), AutoSize = true };
            lblChange = new Label { Text = "0 đ", Font = new Font("Segoe UI", 11F, FontStyle.Bold), Location = new Point(180, top), AutoSize = true };
            top += 50;

            btnCheckout = new Button
            {
                Text = "XÁC NHẬN THANH TOÁN",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(15, top),
                Size = new Size(300, 48),
                BackColor = UITheme.SuccessColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCheckout.FlatAppearance.BorderSize = 0;
            btnCheckout.Click += async (s, e) => await CheckoutAsync();

            grbPayment.Controls.AddRange(new Control[] {
                lblSubtotalTitle, lblSubtotal,
                lblDiscountTitle, nudDiscount,
                lblPaymentMethodTitle, cboPaymentMethod,
                lblTotalTitle, lblTotalAmount,
                lblCashTitle, txtCashReceived,
                lblChangeTitle, lblChange,
                btnCheckout
            });

            this.Controls.Add(dgvCart);
            this.Controls.Add(panelRight);
            this.Controls.Add(panelTop);
        }

        private void ApplyTheme()
        {
            UITheme.ApplyFormTheme(this);
            UITheme.ApplyGridTheme(dgvCart);
        }

        // ==========================================
        // QUÉT BARCODE: GET /api/Products/barcode/{barcode}
        // ==========================================
        private async Task AddProductToCartByBarcodeAsync(string barcode)
        {
            try
            {
                var response = await SessionManager.ApiClientService.Client.GetAsync($"Products/barcode/{barcode}");
                if (response.IsSuccessStatusCode)
                {
                    var product = await response.Content.ReadFromJsonAsync<ProductDto>();
                    if (product == null)
                    {
                        MessageBox.Show("Không tìm thấy sản phẩm có mã vạch này!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var existing = _cart.FirstOrDefault(c => c.ProductId == product.ProductId);
                    if (existing != null)
                    {
                        existing.Quantity++;
                    }
                    else
                    {
                        _cart.Add(new CartItemDto
                        {
                            ProductId = product.ProductId,
                            Barcode = product.Barcode,
                            ProductName = product.ProductName,
                            Unit = string.IsNullOrEmpty(product.Unit) ? "cái" : product.Unit,
                            UnitPrice = product.Price,
                            Quantity = 1
                        });
                    }

                    UpdateCartGrid();
                }
                else
                {
                    MessageBox.Show($"Không tìm thấy sản phẩm với mã vạch '{barcode}'!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối máy chủ: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateCartGrid()
        {
            dgvCart.Rows.Clear();
            foreach (var item in _cart)
            {
                int idx = dgvCart.Rows.Add();
                var row = dgvCart.Rows[idx];
                row.Cells[0].Value = item.ProductId;
                row.Cells[1].Value = item.Barcode;
                row.Cells[2].Value = item.ProductName;
                row.Cells[3].Value = item.Unit;
                row.Cells[4].Value = item.UnitPrice;
                row.Cells[5].Value = item.Quantity;
                row.Cells[6].Value = item.TotalPrice;
            }

            UpdateCalculations();
        }

        private void UpdateCalculations()
        {
            decimal subtotal = _cart.Sum(x => x.TotalPrice);
            decimal discount = nudDiscount.Value;
            decimal total = Math.Max(0, subtotal - discount);

            lblSubtotal.Text = $"{subtotal:N0} đ";
            lblTotalAmount.Text = $"{total:N0} đ";

            CalculateChange();
        }

        private void CalculateChange()
        {
            decimal subtotal = _cart.Sum(x => x.TotalPrice);
            decimal total = Math.Max(0, subtotal - nudDiscount.Value);

            if (decimal.TryParse(txtCashReceived.Text.Trim(), out decimal cashReceived))
            {
                decimal change = cashReceived - total;
                if (change >= 0)
                {
                    lblChange.Text = $"{change:N0} đ";
                    lblChange.ForeColor = Color.Black;
                }
                else
                {
                    lblChange.Text = $"Thiếu: {Math.Abs(change):N0} đ";
                    lblChange.ForeColor = Color.Red;
                }
            }
            else
            {
                lblChange.Text = "0 đ";
                lblChange.ForeColor = Color.Black;
            }
        }

        private void RemoveSelectedItem()
        {
            if (dgvCart.SelectedRows.Count > 0)
            {
                int index = dgvCart.SelectedRows[0].Index;
                if (index >= 0 && index < _cart.Count)
                {
                    _cart.RemoveAt(index);
                    UpdateCartGrid();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn dòng sản phẩm muốn xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ClearCart()
        {
            if (_cart.Count == 0) return;
            var confirm = MessageBox.Show("Bạn có chắc muốn hủy bỏ toàn bộ giỏ hàng hiện tại?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                _cart.Clear();
                _currentCustomer = null;
                txtCustomerPhone.Clear();
                lblCustomerName.Text = "Khách vãng lai";
                lblCustomerPoints.Text = "Điểm tích lũy: 0 | Hạng: Mới";
                nudDiscount.Value = 0;
                txtCashReceived.Clear();
                UpdateCartGrid();
            }
        }

        // ==========================================
        // TRA CỨU KHÁCH HÀNG: GET /api/Customers/phone/{phone}
        // ==========================================
        private async Task LookupCustomerAsync()
        {
            string phone = txtCustomerPhone.Text.Trim();
            if (string.IsNullOrWhiteSpace(phone))
            {
                _currentCustomer = null;
                lblCustomerName.Text = "Khách vãng lai";
                lblCustomerPoints.Text = "Điểm tích lũy: 0 | Hạng: Mới";
                return;
            }

            try
            {
                var response = await SessionManager.ApiClientService.Client.GetAsync($"Customers/phone/{phone}");
                if (response.IsSuccessStatusCode)
                {
                    _currentCustomer = await response.Content.ReadFromJsonAsync<CustomerDto>();
                    if (_currentCustomer != null)
                    {
                        lblCustomerName.Text = $"Khách hàng: {_currentCustomer.CustomerName}";
                        lblCustomerPoints.Text = $"Điểm: {_currentCustomer.RewardPoints} | Hạng: {_currentCustomer.MembershipRank}";
                    }
                }
                else
                {
                    _currentCustomer = null;
                    lblCustomerName.Text = "Khách mới (Chưa có trong hệ thống)";
                    lblCustomerPoints.Text = "SĐT chưa được đăng ký thành viên";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tra cứu khách hàng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========================================
        // XÁC NHẬN THANH TOÁN: POST /api/Orders/checkout
        // ==========================================
        private async Task CheckoutAsync()
        {
            if (_cart.Count == 0)
            {
                MessageBox.Show("Giỏ hàng đang trống! Vui lòng quét mã vạch sản phẩm trước khi thanh toán.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBarcode.Focus();
                return;
            }

            string paymentMethod = "CASH";
            if (cboPaymentMethod.SelectedIndex == 1) paymentMethod = "BANK_TRANSFER";
            else if (cboPaymentMethod.SelectedIndex == 2) paymentMethod = "MOMO";
            else if (cboPaymentMethod.SelectedIndex == 3) paymentMethod = "CARD";

            var orderRequest = new
            {
                CashierUsername = SessionManager.CurrentUsername,
                CustomerPhone = _currentCustomer?.PhoneNumber ?? txtCustomerPhone.Text.Trim(),
                PaymentMethod = paymentMethod,
                Discount = nudDiscount.Value,
                Items = _cart.Select(i => new { i.ProductId, i.Quantity, i.UnitPrice }).ToList()
            };

            btnCheckout.Enabled = false;
            btnCheckout.Text = "Đang xử lý...";

            try
            {
                var response = await SessionManager.ApiClientService.Client.PostAsJsonAsync("Orders/checkout", orderRequest);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<CheckoutResultDto>();

                    string msg = $"THANH TOÁN THÀNH CÔNG!\n\n" +
                                 $"Mã hóa đơn: {result?.OrderCode}\n" +
                                 $"Khách hàng: {result?.CustomerName}\n" +
                                 $"Tổng thanh toán: {result?.Total:N0} đ\n" +
                                 $"Hình thức: {paymentMethod}\n\n" +
                                 $"Hóa đơn đã được lưu và cập nhật tồn kho.";

                    MessageBox.Show(msg, "Hoàn tất đơn hàng", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    _cart.Clear();
                    _currentCustomer = null;
                    txtCustomerPhone.Clear();
                    lblCustomerName.Text = "Khách vãng lai";
                    lblCustomerPoints.Text = "Điểm tích lũy: 0 | Hạng: Mới";
                    nudDiscount.Value = 0;
                    txtCashReceived.Clear();
                    UpdateCartGrid();

                    txtBarcode.Focus();
                }
                else
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(response, "Thanh toán hóa đơn");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối máy chủ khi thanh toán: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCheckout.Enabled = true;
                btnCheckout.Text = "XÁC NHẬN THANH TOÁN";
            }
        }
    }

    public class CheckoutResultDto
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public string CustomerName { get; set; } = string.Empty;
    }
}
