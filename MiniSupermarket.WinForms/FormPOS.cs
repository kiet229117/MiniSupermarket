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
        private Button btnLookupProduct;
        private Button btnIncreaseQty;
        private Button btnDecreaseQty;
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
        private Button btnQuickRegisterCustomer;
        private Label lblCustomerName;
        private Label lblCustomerPoints;

        private GroupBox grbPayment;
        private Label lblSubtotalTitle;
        private Label lblSubtotal;
        private Label lblDiscountTitle;
        private NumericUpDown nudDiscount;
        // Tiêu chí 47: Đổi điểm thưởng trừ tiền
        private Label lblRedeemPointsTitle;
        private NumericUpDown nudRedeemPoints;
        private Label lblRedeemPointsInfo;
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
            panelTop = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.White };

            lblScanTitle = new Label
            {
                Text = "Quét mã vạch (Barcode):",
                Font = UITheme.FontBodyBold,
                Location = new Point(16, 17),
                AutoSize = true
            };
            panelTop.Controls.Add(lblScanTitle);

            txtBarcode = new TextBox
            {
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(175, 12),
                Size = new Size(185, 27)
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
                Location = new Point(368, 11),
                Size = new Size(72, 30),
                Cursor = Cursors.Hand
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

            btnLookupProduct = new Button
            {
                Text = "🔍 Tra cứu",
                Location = new Point(446, 11),
                Size = new Size(88, 30),
                Cursor = Cursors.Hand
            };
            UITheme.ApplyButtonTheme(btnLookupProduct, UITheme.ButtonStyle.Info);
            btnLookupProduct.Click += async (s, e) =>
            {
                using (var lookup = new FormProductLookup())
                {
                    if (lookup.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(lookup.SelectedBarcode))
                    {
                        await AddProductToCartByBarcodeAsync(lookup.SelectedBarcode);
                    }
                }
            };
            panelTop.Controls.Add(btnLookupProduct);

            btnIncreaseQty = new Button
            {
                Text = "➕ Tăng",
                Location = new Point(540, 11),
                Size = new Size(78, 30),
                Cursor = Cursors.Hand
            };
            UITheme.ApplyButtonTheme(btnIncreaseQty, UITheme.ButtonStyle.Success);
            btnIncreaseQty.Click += (s, e) => IncreaseSelectedItem();
            panelTop.Controls.Add(btnIncreaseQty);

            btnDecreaseQty = new Button
            {
                Text = "➖ Giảm",
                Location = new Point(624, 11),
                Size = new Size(78, 30),
                Cursor = Cursors.Hand
            };
            UITheme.ApplyButtonTheme(btnDecreaseQty, UITheme.ButtonStyle.Secondary);
            btnDecreaseQty.Click += (s, e) => DecreaseSelectedItem();
            panelTop.Controls.Add(btnDecreaseQty);

            btnRemoveItem = new Button
            {
                Text = "🗑️ Xóa dòng",
                Location = new Point(708, 11),
                Size = new Size(95, 30),
                Cursor = Cursors.Hand
            };
            UITheme.ApplyButtonTheme(btnRemoveItem, UITheme.ButtonStyle.Warning);
            btnRemoveItem.Click += (s, e) => RemoveSelectedItem();
            panelTop.Controls.Add(btnRemoveItem);

            btnClearCart = new Button
            {
                Text = "Hủy toàn bộ giỏ",
                Location = new Point(809, 11),
                Size = new Size(118, 30),
                Cursor = Cursors.Hand
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
            dgvCart.Columns["Quantity"].HeaderText = "Số Lượng ✎";

            // Cho phép gõ sửa trực tiếp ô Số lượng
            dgvCart.ReadOnly = false;
            foreach (DataGridViewColumn col in dgvCart.Columns)
            {
                col.ReadOnly = (col.Name != "Quantity");
            }

            dgvCart.CellEndEdit += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < _cart.Count && dgvCart.Columns[e.ColumnIndex].Name == "Quantity")
                {
                    var cellVal = dgvCart.Rows[e.RowIndex].Cells["Quantity"].Value;
                    if (int.TryParse(cellVal?.ToString(), out int newQty) && newQty > 0)
                    {
                        _cart[e.RowIndex].Quantity = newQty;
                        UpdateCartGrid(e.RowIndex);
                    }
                    else if (newQty == 0)
                    {
                        _cart.RemoveAt(e.RowIndex);
                        int nextSelect = Math.Min(e.RowIndex, _cart.Count - 1);
                        UpdateCartGrid(nextSelect);
                    }
                    else
                    {
                        MessageBox.Show("Số lượng phải là số nguyên dương lớn hơn 0!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        UpdateCartGrid(e.RowIndex);
                    }
                }
            };

            // Phím tắt bàn phím (+, -, Delete) trên giỏ hàng
            dgvCart.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus)
                {
                    IncreaseSelectedItem();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus)
                {
                    DecreaseSelectedItem();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Delete)
                {
                    RemoveSelectedItem();
                    e.Handled = true;
                }
            };

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

            lblPhone = new Label { Text = "SĐT:", Font = UITheme.FontBodyBold, Location = new Point(12, 25), AutoSize = true };
            txtCustomerPhone = new TextBox { Font = UITheme.FontBody, Location = new Point(50, 22), Size = new Size(130, 25) };
            txtCustomerPhone.KeyDown += async (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) await LookupCustomerAsync();
            };

            btnFindCustomer = new Button { Text = "🔍 Tìm", Location = new Point(184, 21), Size = new Size(62, 27) };
            UITheme.ApplyButtonTheme(btnFindCustomer, UITheme.ButtonStyle.Primary);
            btnFindCustomer.Click += async (s, e) => await LookupCustomerAsync();

            btnQuickRegisterCustomer = new Button { Text = "➕ ĐK Mới", Location = new Point(250, 21), Size = new Size(78, 27) };
            UITheme.ApplyButtonTheme(btnQuickRegisterCustomer, UITheme.ButtonStyle.Secondary);
            btnQuickRegisterCustomer.Click += (s, e) => QuickRegisterCustomer();

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
                lblPhone, txtCustomerPhone, btnFindCustomer, btnQuickRegisterCustomer,
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
            top += 33;

            // Tiêu chí 47: Đổi Điểm Thưởng Trừ Tiền Hóa Đơn (Loyalty / Points)
            lblRedeemPointsTitle = new Label { Text = "Đổi điểm (-1k/đ):", Font = UITheme.FontBodyBold, Location = new Point(15, top), AutoSize = true };
            nudRedeemPoints = new NumericUpDown
            {
                Font = UITheme.FontBody,
                Location = new Point(180, top - 3),
                Size = new Size(130, 25),
                Maximum = 0,
                Increment = 1
            };
            nudRedeemPoints.ValueChanged += (s, e) => UpdateCalculations();
            top += 24;

            lblRedeemPointsInfo = new Label { Text = "(1 điểm = 1.000 đ)", Font = UITheme.FontSmall, ForeColor = UITheme.TextMuted, Location = new Point(180, top), AutoSize = true };
            top += 28;

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
                lblRedeemPointsTitle, nudRedeemPoints, lblRedeemPointsInfo,
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

        private void UpdateCartGrid(int selectedIndex = -1)
        {
            dgvCart.SuspendLayout();
            try
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
            }
            finally
            {
                dgvCart.ResumeLayout();
            }

            UpdateCalculations();
        }

        private void UpdateCalculations()
        {
            decimal subtotal = _cart.Sum(x => x.TotalPrice);
            decimal discount = nudDiscount.Value;

            if (_currentCustomer != null && nudRedeemPoints != null)
            {
                int maxPointsCanUse = Math.Min(_currentCustomer.RewardPoints, (int)(subtotal / 1000m));
                nudRedeemPoints.Maximum = Math.Max(0, maxPointsCanUse);
                lblRedeemPointsInfo.Text = $"(Có: {_currentCustomer.RewardPoints} đ | Dùng tối đa: {maxPointsCanUse} đ)";
            }

            decimal pointDiscount = nudRedeemPoints != null ? nudRedeemPoints.Value * 1000m : 0m;
            decimal total = Math.Max(0, subtotal - discount - pointDiscount);

            lblSubtotal.Text = $"{subtotal:N0} đ";
            lblTotalAmount.Text = $"{total:N0} đ";

            CalculateChange();
        }

        private void CalculateChange()
        {
            decimal subtotal = _cart.Sum(x => x.TotalPrice);
            decimal pointDiscount = nudRedeemPoints != null ? nudRedeemPoints.Value * 1000m : 0m;
            decimal total = Math.Max(0, subtotal - nudDiscount.Value - pointDiscount);

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

        private void IncreaseSelectedItem()
        {
            if (dgvCart.SelectedRows.Count > 0)
            {
                int index = dgvCart.SelectedRows[0].Index;
                if (index >= 0 && index < _cart.Count)
                {
                    _cart[index].Quantity++;
                    UpdateCartGrid(index);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn dòng sản phẩm muốn tăng số lượng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DecreaseSelectedItem()
        {
            if (dgvCart.SelectedRows.Count > 0)
            {
                int index = dgvCart.SelectedRows[0].Index;
                if (index >= 0 && index < _cart.Count)
                {
                    if (_cart[index].Quantity > 1)
                    {
                        _cart[index].Quantity--;
                        UpdateCartGrid(index);
                    }
                    else
                    {
                        var confirm = MessageBox.Show($"Số lượng của '{_cart[index].ProductName}' đang là 1. Bạn có muốn xóa sản phẩm này khỏi giỏ hàng?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (confirm == DialogResult.Yes)
                        {
                            _cart.RemoveAt(index);
                            int nextSelect = Math.Min(index, _cart.Count - 1);
                            UpdateCartGrid(nextSelect);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn dòng sản phẩm muốn giảm số lượng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                if (nudRedeemPoints != null) { nudRedeemPoints.Maximum = 0; nudRedeemPoints.Value = 0; lblRedeemPointsInfo.Text = "(1 điểm = 1.000 đ)"; }
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
                        if (nudRedeemPoints != null)
                        {
                            nudRedeemPoints.Maximum = _currentCustomer.RewardPoints;
                            nudRedeemPoints.Value = 0;
                            lblRedeemPointsInfo.Text = $"(Có: {_currentCustomer.RewardPoints} đ = {_currentCustomer.RewardPoints * 1000:N0} đ)";
                        }
                    }
                }
                else
                {
                    _currentCustomer = null;
                    lblCustomerName.Text = "Khách mới (Chưa có trong hệ thống)";
                    lblCustomerPoints.Text = "SĐT chưa được đăng ký thành viên";

                    var confirm = MessageBox.Show(
                        $"Số điện thoại '{phone}' chưa có tài khoản thành viên.\nBạn có muốn đăng ký nhanh khách hàng mới ngay bây giờ không?",
                        "Đăng ký khách mới",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (confirm == DialogResult.Yes)
                    {
                        QuickRegisterCustomer(phone);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tra cứu khách hàng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========================================
        // ==========================================
        // ĐĂNG KÝ NHANH KHÁCH HÀNG TẠI QUẦY
        // ==========================================
        private void QuickRegisterCustomer(string defaultPhone = null)
        {
            string phone = defaultPhone ?? txtCustomerPhone.Text.Trim();
            using (var dlg = new FormQuickCustomer(phone))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.CreatedCustomer != null)
                {
                    _currentCustomer = dlg.CreatedCustomer;
                    txtCustomerPhone.Text = _currentCustomer.PhoneNumber;
                    lblCustomerName.Text = $"Khách hàng: {_currentCustomer.CustomerName}";
                    lblCustomerPoints.Text = $"Điểm: {_currentCustomer.RewardPoints} | Hạng: {_currentCustomer.MembershipRank}";
                    txtBarcode.Focus();
                }
            }
        }

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
                PointsToRedeem = nudRedeemPoints != null ? (int)nudRedeemPoints.Value : 0,
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

                    decimal cashGiven = decimal.TryParse(txtCashReceived.Text.Trim(), out var cg) ? cg : (result?.Total ?? 0);
                    decimal changeDue = Math.Max(0, cashGiven - (result?.Total ?? 0));

                    var receipt = new InvoiceReceiptModel
                    {
                        OrderCode = result?.OrderCode ?? "HD0000",
                        CreatedAt = DateTime.Now,
                        CashierName = SessionManager.CurrentUsername,
                        CustomerName = !string.IsNullOrWhiteSpace(result?.CustomerName) ? result.CustomerName : (_currentCustomer?.CustomerName ?? "Khách vãng lai"),
                        CustomerPhone = _currentCustomer?.PhoneNumber ?? txtCustomerPhone.Text.Trim(),
                        EarnedPoints = (int)((result?.Total ?? 0) / 10000),
                        PaymentMethod = paymentMethod switch
                        {
                            "CASH" => "Tiền mặt",
                            "BANK_TRANSFER" => "Chuyển khoản",
                            "MOMO" => "Ví MoMo",
                            "CARD" => "Thẻ ngân hàng",
                            _ => paymentMethod
                        },
                        Subtotal = result?.Subtotal ?? _cart.Sum(x => x.TotalPrice),
                        Discount = result?.Discount ?? nudDiscount.Value,
                        Total = result?.Total ?? 0,
                        CashGiven = cashGiven,
                        ChangeDue = changeDue,
                        Items = _cart.Select(i => new InvoiceItemModel
                        {
                            ProductName = i.ProductName,
                            Quantity = i.Quantity,
                            Unit = i.Unit,
                            UnitPrice = i.UnitPrice,
                            LineTotal = i.TotalPrice
                        }).ToList()
                    };

                    _cart.Clear();
                    _currentCustomer = null;
                    txtCustomerPhone.Clear();
                    lblCustomerName.Text = "Khách vãng lai";
                    lblCustomerPoints.Text = "Điểm tích lũy: 0 | Hạng: Mới";
                    if (nudRedeemPoints != null)
                    {
                        nudRedeemPoints.Maximum = 0;
                        nudRedeemPoints.Value = 0;
                        lblRedeemPointsInfo.Text = "(1 điểm = 1.000 đ)";
                    }
                    nudDiscount.Value = 0;
                    txtCashReceived.Clear();
                    UpdateCartGrid();

                    var askPrint = MessageBox.Show(
                        $"THANH TOÁN THÀNH CÔNG!\n\n" +
                        $"Mã hóa đơn: {receipt.OrderCode}\n" +
                        $"Khách hàng: {receipt.CustomerName}\n" +
                        $"Tổng thanh toán: {receipt.Total:N0} đ\n" +
                        $"Hình thức: {receipt.PaymentMethod}\n\n" +
                        $"Bạn có muốn XEM & IN HÓA ĐƠN ngay không?",
                        "Hoàn tất đơn hàng",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (askPrint == DialogResult.Yes)
                    {
                        using (var formReceipt = new FormInvoiceReceipt(receipt))
                        {
                            formReceipt.ShowDialog(this);
                        }
                    }

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
