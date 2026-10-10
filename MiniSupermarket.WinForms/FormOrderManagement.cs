using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public class FormOrderManagement : Form
    {
        private const string SearchPlaceholder = "Mã đơn hoặc Khách hàng...";
        private readonly List<OrderHeaderDto> _orders = new List<OrderHeaderDto>();
        private OrderDetailDto _currentOrder = null;

        // Controls giao diện
        private Panel panelTop;
        private Label lblTitle;
        private Label lblStatus;
        private Label lblFilterStatus;
        private ComboBox cboFilterStatus;
        private Label lblSearch;
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnReload;

        private DataGridView dgvOrders;

        private Panel panelRight;
        private GroupBox grbDetails;
        private Label lblDetailCode;
        private Label lblDetailDate;
        private Label lblDetailCustomer;
        private Label lblDetailCashier;
        private Label lblDetailPayment;
        private Label lblDetailStatus;
        private Label lblDetailSubtotal;
        private Label lblDetailDiscount;
        private Label lblDetailTotal;

        private DataGridView dgvOrderItems;
        private Button btnCancelOrder;

        public FormOrderManagement()
        {
            BuildUI();
            ApplyTheme();
            this.Load += FormOrderManagement_Load;
        }

        private void BuildUI()
        {
            this.Text = "QUẢN LÝ ĐƠN HÀNG";
            this.Size = new Size(1100, 650);
            this.Dock = DockStyle.Fill;

            // 1. Panel Top (Tiêu đề và Bộ lọc / Tìm kiếm)
            panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.White
            };

            lblTitle = new Label
            {
                Text = "QUẢN LÝ ĐƠN HÀNG & LỊCH SỬ GIAO DỊCH",
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

            lblFilterStatus = new Label
            {
                Text = "Trạng thái:",
                Font = UITheme.FontBodyBold,
                ForeColor = UITheme.TextPrimary,
                AutoSize = true,
                Margin = new Padding(0, 6, 4, 0)
            };
            panelSearchFilter.Controls.Add(lblFilterStatus);

            cboFilterStatus = new ComboBox
            {
                Font = UITheme.FontBody,
                Size = new Size(130, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 2, 8, 0)
            };
            cboFilterStatus.Items.AddRange(new object[] { "Tất cả", "PAID", "CANCELLED" });
            cboFilterStatus.SelectedIndex = 0;
            cboFilterStatus.SelectedIndexChanged += async (s, e) =>
            {
                if (cboFilterStatus.Focused) await SearchOrdersAsync();
            };
            panelSearchFilter.Controls.Add(cboFilterStatus);

            lblSearch = new Label
            {
                Text = "Tìm:",
                Font = UITheme.FontBodyBold,
                ForeColor = UITheme.TextPrimary,
                AutoSize = true,
                Margin = new Padding(0, 6, 4, 0)
            };
            panelSearchFilter.Controls.Add(lblSearch);

            txtSearch = new TextBox
            {
                Font = UITheme.FontBody,
                Size = new Size(160, 26),
                Margin = new Padding(0, 2, 8, 0)
            };
            UITheme.SetupSearchPlaceholder(txtSearch, SearchPlaceholder);
            txtSearch.KeyDown += async (s, e) => { if (e.KeyCode == Keys.Enter) await SearchOrdersAsync(); };
            panelSearchFilter.Controls.Add(txtSearch);

            btnSearch = new Button
            {
                Text = "Tìm",
                Size = new Size(65, 28),
                Margin = new Padding(0, 0, 6, 0)
            };
            UITheme.ApplyButtonTheme(btnSearch, UITheme.ButtonStyle.Primary);
            btnSearch.Click += async (s, e) => await SearchOrdersAsync();
            panelSearchFilter.Controls.Add(btnSearch);

            btnReload = new Button
            {
                Text = "Tải lại",
                Size = new Size(70, 28),
                Margin = new Padding(0, 0, 0, 0)
            };
            UITheme.ApplyButtonTheme(btnReload, UITheme.ButtonStyle.Secondary);
            btnReload.Click += async (s, e) => await ReloadDataAsync();
            panelSearchFilter.Controls.Add(btnReload);

            panelTop.Controls.Add(panelSearchFilter);

            // 2. DataGridView hiển thị danh sách đơn hàng
            dgvOrders = new DataGridView { Dock = DockStyle.Fill };
            dgvOrders.Columns.Add("OrderId", "ID");
            dgvOrders.Columns.Add("OrderCode", "Mã Đơn");
            dgvOrders.Columns.Add("CreatedAt", "Ngày Tạo");
            dgvOrders.Columns.Add("CustomerName", "Khách Hàng");
            dgvOrders.Columns.Add("CashierName", "Thu Ngân");
            dgvOrders.Columns.Add("PaymentMethod", "Hình Thức");
            dgvOrders.Columns.Add("Total", "Tổng Tiền");
            dgvOrders.Columns.Add("Status", "Trạng Thái");

            dgvOrders.Columns["OrderId"].Width = 55;
            dgvOrders.Columns["OrderCode"].Width = 110;
            dgvOrders.Columns["CreatedAt"].Width = 125;
            dgvOrders.Columns["CustomerName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvOrders.Columns["CashierName"].Width = 100;
            dgvOrders.Columns["PaymentMethod"].Width = 85;
            dgvOrders.Columns["Total"].Width = 100;
            dgvOrders.Columns["Status"].Width = 90;

            dgvOrders.Columns["Total"].DefaultCellStyle.Format = "N0";
            dgvOrders.Columns["Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            dgvOrders.CellClick += DgvOrders_CellClick;

            // 3. Panel bên phải chứa chi tiết hóa đơn & danh sách mặt hàng
            panelRight = new Panel
            {
                Dock = DockStyle.Right,
                Width = 400,
                BackColor = Color.White,
                Padding = new Padding(12),
                AutoScroll = true
            };

            grbDetails = new GroupBox
            {
                Text = "Chi tiết hóa đơn",
                Font = UITheme.FontSubtitle,
                ForeColor = UITheme.TextPrimary,
                Dock = DockStyle.Fill
            };
            panelRight.Controls.Add(grbDetails);

            Panel infoPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 195,
                Padding = new Padding(8)
            };

            lblDetailCode = new Label { Text = "Mã đơn: --", Font = UITheme.FontBodyBold, ForeColor = UITheme.PrimaryColor, Location = new Point(10, 8), AutoSize = true };
            lblDetailDate = new Label { Text = "Ngày tạo: --", Font = UITheme.FontSmall, ForeColor = UITheme.TextMuted, Location = new Point(10, 30), AutoSize = true };
            lblDetailCustomer = new Label { Text = "Khách hàng: --", Font = UITheme.FontBody, Location = new Point(10, 52), AutoSize = true };
            lblDetailCashier = new Label { Text = "Thu ngân: --", Font = UITheme.FontBody, Location = new Point(10, 74), AutoSize = true };
            lblDetailPayment = new Label { Text = "Thanh toán: --", Font = UITheme.FontBody, Location = new Point(10, 96), AutoSize = true };
            lblDetailStatus = new Label { Text = "Trạng thái: --", Font = UITheme.FontBodyBold, Location = new Point(10, 118), AutoSize = true };
            lblDetailSubtotal = new Label { Text = "Tạm tính: 0 đ", Font = UITheme.FontSmall, Location = new Point(10, 142), AutoSize = true };
            lblDetailDiscount = new Label { Text = "Giảm giá: 0 đ", Font = UITheme.FontSmall, Location = new Point(180, 142), AutoSize = true };
            lblDetailTotal = new Label { Text = "TỔNG: 0 đ", Font = new Font("Segoe UI", 11.5F, FontStyle.Bold), ForeColor = UITheme.DangerColor, Location = new Point(10, 164), AutoSize = true };

            infoPanel.Controls.AddRange(new Control[] {
                lblDetailCode, lblDetailDate,
                lblDetailCustomer, lblDetailCashier,
                lblDetailPayment, lblDetailStatus,
                lblDetailSubtotal, lblDetailDiscount, lblDetailTotal
            });

            grbDetails.Controls.Add(infoPanel);

            // Grid các sản phẩm trong đơn
            Label lblItemsHeader = new Label
            {
                Text = "Danh sách mặt hàng:",
                Font = UITheme.FontBodyBold,
                Dock = DockStyle.Top,
                Height = 25,
                Padding = new Padding(6, 4, 0, 0)
            };
            grbDetails.Controls.Add(lblItemsHeader);

            dgvOrderItems = new DataGridView
            {
                Dock = DockStyle.Fill
            };
            dgvOrderItems.Columns.Add("ProductName", "Tên SP");
            dgvOrderItems.Columns.Add("Quantity", "SL");
            dgvOrderItems.Columns.Add("UnitPrice", "Đơn Giá");
            dgvOrderItems.Columns.Add("LineTotal", "T.Tiền");

            dgvOrderItems.Columns["ProductName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvOrderItems.Columns["Quantity"].Width = 45;
            dgvOrderItems.Columns["UnitPrice"].Width = 70;
            dgvOrderItems.Columns["LineTotal"].Width = 80;

            dgvOrderItems.Columns["UnitPrice"].DefaultCellStyle.Format = "N0";
            dgvOrderItems.Columns["LineTotal"].DefaultCellStyle.Format = "N0";
            dgvOrderItems.Columns["Quantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            grbDetails.Controls.Add(dgvOrderItems);

            // Nút hủy đơn hàng ở bottom của panelRight
            Panel bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                Padding = new Padding(0, 8, 0, 4)
            };

            btnCancelOrder = new Button
            {
                Text = "🚫 HỦY ĐƠN HÀNG (HOÀN KHO)",
                Dock = DockStyle.Fill
            };
            UITheme.ApplyButtonTheme(btnCancelOrder, UITheme.ButtonStyle.Danger);
            btnCancelOrder.Click += async (s, e) => await CancelOrderAsync();
            bottomPanel.Controls.Add(btnCancelOrder);

            grbDetails.Controls.Add(bottomPanel);

            this.Controls.Add(dgvOrders);
            this.Controls.Add(panelRight);
            this.Controls.Add(panelTop);
        }

        private void ApplyTheme()
        {
            UITheme.ApplyFormTheme(this);
            UITheme.ApplyGridTheme(dgvOrders);
            UITheme.ApplyGridTheme(dgvOrderItems);
        }

        private void SetPermission()
        {
            string role = (SessionManager.CurrentRole ?? "").ToUpper();
            // Chỉ Admin mới được quyền hủy đơn hàng và hoàn kho
            btnCancelOrder.Enabled = (role == "ADMIN");
        }

        private async void FormOrderManagement_Load(object sender, EventArgs e)
        {
            SetPermission();
            await LoadDataAsync();
        }

        private async Task LoadDataAsync(string statusFilter = null, string keyword = null)
        {
            try
            {
                lblStatus.Text = "Đang tải dữ liệu...";
                string endpoint = "Orders";
                var q = new List<string>();

                if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "Tất cả")
                {
                    q.Add($"status={Uri.EscapeDataString(statusFilter)}");
                }

                if (q.Count > 0)
                {
                    endpoint += "?" + string.Join("&", q);
                }

                var response = await SessionManager.ApiClientService.Client.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(response, "Tải danh sách đơn hàng");
                    lblStatus.Text = "Lỗi tải dữ liệu";
                    return;
                }

                var list = await response.Content.ReadFromJsonAsync<List<OrderHeaderDto>>();
                _orders.Clear();

                if (list != null)
                {
                    // Lọc keyword cục bộ nếu người dùng nhập tìm theo mã đơn hoặc tên khách
                    if (!string.IsNullOrWhiteSpace(keyword) && keyword != SearchPlaceholder)
                    {
                        string kwLower = keyword.ToLower();
                        foreach (var o in list)
                        {
                            if ((o.OrderCode != null && o.OrderCode.ToLower().Contains(kwLower)) ||
                                (o.CustomerName != null && o.CustomerName.ToLower().Contains(kwLower)) ||
                                (o.CashierName != null && o.CashierName.ToLower().Contains(kwLower)))
                            {
                                _orders.Add(o);
                            }
                        }
                    }
                    else
                    {
                        _orders.AddRange(list);
                    }
                }

                DisplayOrders(_orders);
                lblStatus.Text = $"Có {_orders.Count} đơn hàng";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối API: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Lỗi tải dữ liệu";
            }
        }

        private void DisplayOrders(List<OrderHeaderDto> list)
        {
            dgvOrders.Rows.Clear();
            foreach (var o in list)
            {
                int idx = dgvOrders.Rows.Add();
                var row = dgvOrders.Rows[idx];
                row.Cells[0].Value = o.OrderId;
                row.Cells[1].Value = o.OrderCode;
                row.Cells[2].Value = o.CreatedAt.ToString("dd/MM/yyyy HH:mm");
                row.Cells[3].Value = string.IsNullOrEmpty(o.CustomerName) ? "Khách vãng lai" : o.CustomerName;
                row.Cells[4].Value = string.IsNullOrEmpty(o.CashierName) ? "--" : o.CashierName;
                row.Cells[5].Value = o.PaymentMethod;
                row.Cells[6].Value = o.Total;
                row.Cells[7].Value = o.Status;

                if (o.Status == "CANCELLED")
                {
                    row.DefaultCellStyle.ForeColor = Color.IndianRed;
                }
            }

            if (list.Count > 0)
            {
                dgvOrders.ClearSelection();
                dgvOrders.Rows[0].Selected = true;
                _ = ShowOrderDetailAsync(list[0].OrderId);
            }
            else
            {
                ClearDetails();
            }
        }

        private async void DgvOrders_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _orders.Count) return;
            int orderId = _orders[e.RowIndex].OrderId;
            await ShowOrderDetailAsync(orderId);
        }

        private async Task ShowOrderDetailAsync(int orderId)
        {
            try
            {
                var resp = await SessionManager.ApiClientService.Client.GetAsync($"Orders/{orderId}");
                if (!resp.IsSuccessStatusCode)
                {
                    ClearDetails();
                    return;
                }

                _currentOrder = await resp.Content.ReadFromJsonAsync<OrderDetailDto>();
                if (_currentOrder == null)
                {
                    ClearDetails();
                    return;
                }

                lblDetailCode.Text = $"Mã đơn: {_currentOrder.OrderCode}";
                lblDetailDate.Text = $"Ngày tạo: {_currentOrder.CreatedAt:dd/MM/yyyy HH:mm:ss}";
                lblDetailCustomer.Text = $"Khách hàng: {(string.IsNullOrEmpty(_currentOrder.CustomerName) ? "Khách vãng lai" : _currentOrder.CustomerName)}";
                lblDetailCashier.Text = $"Thu ngân: {(string.IsNullOrEmpty(_currentOrder.CashierName) ? "--" : _currentOrder.CashierName)}";
                lblDetailPayment.Text = $"Thanh toán: {_currentOrder.PaymentMethod}";
                lblDetailStatus.Text = $"Trạng thái: {_currentOrder.Status}";

                if (_currentOrder.Status == "PAID")
                {
                    lblDetailStatus.ForeColor = UITheme.SuccessColor;
                    btnCancelOrder.Enabled = (SessionManager.CurrentRole ?? "").ToUpper() == "ADMIN";
                }
                else
                {
                    lblDetailStatus.ForeColor = UITheme.DangerColor;
                    btnCancelOrder.Enabled = false;
                }

                lblDetailSubtotal.Text = $"Tạm tính: {_currentOrder.Subtotal:N0} đ";
                lblDetailDiscount.Text = $"Giảm giá: {_currentOrder.Discount:N0} đ";
                lblDetailTotal.Text = $"TỔNG: {_currentOrder.Total:N0} đ";

                // Đổ vào dgvOrderItems
                dgvOrderItems.Rows.Clear();
                if (_currentOrder.Items != null)
                {
                    foreach (var item in _currentOrder.Items)
                    {
                        int idx = dgvOrderItems.Rows.Add();
                        var row = dgvOrderItems.Rows[idx];
                        row.Cells[0].Value = item.ProductName ?? $"SP #{item.ProductId}";
                        row.Cells[1].Value = item.Quantity;
                        row.Cells[2].Value = item.UnitPrice;
                        row.Cells[3].Value = item.LineTotal;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nạp chi tiết đơn hàng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearDetails()
        {
            _currentOrder = null;
            lblDetailCode.Text = "Mã đơn: --";
            lblDetailDate.Text = "Ngày tạo: --";
            lblDetailCustomer.Text = "Khách hàng: --";
            lblDetailCashier.Text = "Thu ngân: --";
            lblDetailPayment.Text = "Thanh toán: --";
            lblDetailStatus.Text = "Trạng thái: --";
            lblDetailStatus.ForeColor = UITheme.TextPrimary;
            lblDetailSubtotal.Text = "Tạm tính: 0 đ";
            lblDetailDiscount.Text = "Giảm giá: 0 đ";
            lblDetailTotal.Text = "TỔNG: 0 đ";
            dgvOrderItems.Rows.Clear();
            btnCancelOrder.Enabled = false;
        }

        private async Task SearchOrdersAsync()
        {
            string status = cboFilterStatus.SelectedItem?.ToString();
            string kw = txtSearch.Text.Trim();
            await LoadDataAsync(status, kw);
        }

        private async Task ReloadDataAsync()
        {
            txtSearch.Text = SearchPlaceholder;
            txtSearch.ForeColor = UITheme.TextMuted;
            cboFilterStatus.SelectedIndex = 0;
            await LoadDataAsync();
        }

        private async Task CancelOrderAsync()
        {
            if (_currentOrder == null)
            {
                MessageBox.Show("Vui lòng chọn 1 đơn hàng cần hủy!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_currentOrder.Status != "PAID")
            {
                MessageBox.Show("Đơn hàng này không ở trạng thái PAID để hủy!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn HỦY đơn hàng [{_currentOrder.OrderCode}]?\n\n" +
                "Hệ thống sẽ:\n" +
                "1. Hoàn lại số lượng tồn kho cho các sản phẩm.\n" +
                "2. Thu hồi điểm tích lũy của khách hàng.\n" +
                "3. Chuyển trạng thái hóa đơn sang CANCELLED.",
                "Xác nhận hủy hóa đơn",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                var response = await SessionManager.ApiClientService.Client.PutAsync($"Orders/{_currentOrder.OrderId}/cancel", null);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Hủy hóa đơn thành công! Đã hoàn kho và cập nhật trạng thái.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await ReloadDataAsync();
                }
                else
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(response, "Hủy hóa đơn");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi hủy đơn hàng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Backward compatibility
        public class ApiOrder : OrderHeaderDto { }
    }
}
