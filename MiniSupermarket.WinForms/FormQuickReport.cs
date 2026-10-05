using System;
using System.Drawing;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public partial class FormQuickReport : Form
    {
        private Panel panelTop;
        private Label lblDateTitle;
        private DateTimePicker dtpReportDate;
        private Button btnRunReport;
        private Button btnToday;
        private Label lblStatus;

        // Metric Cards
        private Label lblTotalOrders;
        private Label lblTotalRevenue;
        private Label lblBestSeller;

        // DataGridView Hoá đơn trong ngày
        private Label lblGridTitle;
        private DataGridView dgvOrders;

        public FormQuickReport()
        {
            InitializeComponent();
            BuildReportUI();
            ApplyTheme();
        }

        private void BuildReportUI()
        {
            this.Text = "BÁO CÁO DOANH THU & HIỆU SUẤT";
            this.Size = new Size(1000, 600);

            // 1. Top Filter Panel
            panelTop = new Panel { Dock = DockStyle.Top, Height = 65, BackColor = Color.White };

            lblDateTitle = new Label
            {
                Text = "Chọn ngày báo cáo:",
                Font = UITheme.FontBodyBold,
                Location = new Point(16, 20),
                AutoSize = true
            };
            panelTop.Controls.Add(lblDateTitle);

            dtpReportDate = new DateTimePicker
            {
                Location = new Point(155, 17),
                Format = DateTimePickerFormat.Short,
                Font = UITheme.FontBody,
                Width = 140
            };
            panelTop.Controls.Add(dtpReportDate);

            btnRunReport = new Button
            {
                Text = "Xem báo cáo",
                Location = new Point(310, 15),
                Size = new Size(120, 30)
            };
            UITheme.ApplyButtonTheme(btnRunReport, UITheme.ButtonStyle.Primary);
            btnRunReport.Click += async (s, e) => await LoadReportDataAsync(dtpReportDate.Value.ToString("yyyy-MM-dd"));
            panelTop.Controls.Add(btnRunReport);

            btnToday = new Button
            {
                Text = "Hôm nay",
                Location = new Point(440, 15),
                Size = new Size(90, 30)
            };
            UITheme.ApplyButtonTheme(btnToday, UITheme.ButtonStyle.Secondary);
            btnToday.Click += async (s, e) =>
            {
                dtpReportDate.Value = DateTime.Today;
                await LoadReportDataAsync(dtpReportDate.Value.ToString("yyyy-MM-dd"));
            };
            panelTop.Controls.Add(btnToday);

            lblStatus = new Label
            {
                Text = "Sẵn sàng",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.TextMuted,
                Location = new Point(550, 22),
                AutoSize = true
            };
            panelTop.Controls.Add(lblStatus);

            // 2. Cards Panel
            Panel panelCards = new Panel
            {
                Dock = DockStyle.Top,
                Height = 150,
                BackColor = UITheme.FormBackground,
                Padding = new Padding(15, 15, 15, 5)
            };

            // Thẻ 1: Tổng số đơn
            Panel pnlCard1 = CreateMetricCard("TỔNG SỐ HÓA ĐƠN", Color.FromArgb(41, 128, 185), new Point(15, 15), new Size(290, 120));
            lblTotalOrders = new Label
            {
                Text = "0",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = Color.White
            };
            pnlCard1.Controls.Add(lblTotalOrders);
            lblTotalOrders.BringToFront();

            // Thẻ 2: Doanh thu
            Panel pnlCard2 = CreateMetricCard("TỔNG DOANH THU", Color.FromArgb(39, 174, 96), new Point(320, 15), new Size(320, 120));
            lblTotalRevenue = new Label
            {
                Text = "0 đ",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = Color.White
            };
            pnlCard2.Controls.Add(lblTotalRevenue);
            lblTotalRevenue.BringToFront();

            // Thẻ 3: Bán chạy nhất
            Panel pnlCard3 = CreateMetricCard("MẶT HÀNG BÁN CHẠY NHẤT", Color.FromArgb(142, 68, 173), new Point(655, 15), new Size(310, 120));
            lblBestSeller = new Label
            {
                Text = "Chưa có",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White
            };
            pnlCard3.Controls.Add(lblBestSeller);
            lblBestSeller.BringToFront();

            panelCards.Controls.AddRange(new Control[] { pnlCard1, pnlCard2, pnlCard3 });

            // 3. Bottom Panel: Bảng các hóa đơn
            Panel panelBottom = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(16)
            };

            lblGridTitle = new Label
            {
                Text = "DANH SÁCH HÓA ĐƠN TRONG NGÀY",
                Font = UITheme.FontSubtitle,
                ForeColor = UITheme.TextPrimary,
                Dock = DockStyle.Top,
                Height = 30
            };

            dgvOrders = new DataGridView { Dock = DockStyle.Fill };
            dgvOrders.Columns.Add("OrderId", "Mã Đơn");
            dgvOrders.Columns.Add("OrderCode", "Mã Hóa Đơn");
            dgvOrders.Columns.Add("CustomerName", "Khách Hàng");
            dgvOrders.Columns.Add("PaymentMethod", "Phương Thức");
            dgvOrders.Columns.Add("Total", "Tổng Tiền (VNĐ)");
            dgvOrders.Columns.Add("CreatedAt", "Thời Gian");

            dgvOrders.Columns["OrderId"].Width = 80;
            dgvOrders.Columns["OrderCode"].Width = 140;
            dgvOrders.Columns["CustomerName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvOrders.Columns["PaymentMethod"].Width = 130;
            dgvOrders.Columns["Total"].Width = 140;
            dgvOrders.Columns["CreatedAt"].Width = 150;

            dgvOrders.Columns["Total"].DefaultCellStyle.Format = "N0";
            dgvOrders.Columns["CreatedAt"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";

            panelBottom.Controls.Add(dgvOrders);
            panelBottom.Controls.Add(lblGridTitle);

            this.Controls.Add(panelBottom);
            this.Controls.Add(panelCards);
            this.Controls.Add(panelTop);

            this.Load += async (s, e) =>
            {
                await LoadReportDataAsync(dtpReportDate.Value.ToString("yyyy-MM-dd"));
            };
        }

        private Panel CreateMetricCard(string title, Color bg, Point location, Size size)
        {
            Panel card = new Panel
            {
                Location = location,
                Size = size,
                BackColor = bg
            };

            Label lbl = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 32,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = UITheme.FontBodyBold,
                ForeColor = Color.White
            };
            card.Controls.Add(lbl);
            return card;
        }

        private void ApplyTheme()
        {
            UITheme.ApplyFormTheme(this);
            UITheme.ApplyGridTheme(dgvOrders);
        }

        // ==========================================
        // GỌI API BÁO CÁO: GET /api/Reports/daily?date=...
        // ==========================================
        private async Task LoadReportDataAsync(string dateStr)
        {
            lblStatus.Text = "Đang tải báo cáo...";
            try
            {
                var response = await SessionManager.ApiClientService.Client.GetAsync($"Reports/daily?date={dateStr}");
                if (response.IsSuccessStatusCode)
                {
                    var report = await response.Content.ReadFromJsonAsync<ReportSummaryDto>();
                    if (report != null)
                    {
                        lblTotalOrders.Text = report.TotalOrders.ToString("N0");
                        lblTotalRevenue.Text = $"{report.TotalRevenue:N0} đ";
                        lblBestSeller.Text = string.IsNullOrEmpty(report.BestSeller) ? "Chưa có" : report.BestSeller;

                        dgvOrders.Rows.Clear();
                        if (report.RecentOrders != null)
                        {
                            foreach (var ord in report.RecentOrders)
                            {
                                int idx = dgvOrders.Rows.Add();
                                var row = dgvOrders.Rows[idx];
                                row.Cells[0].Value = ord.OrderId;
                                row.Cells[1].Value = ord.OrderCode;
                                row.Cells[2].Value = ord.CustomerName;
                                row.Cells[3].Value = ord.PaymentMethod;
                                row.Cells[4].Value = ord.Total;
                                row.Cells[5].Value = ord.CreatedAt;
                            }
                        }

                        lblStatus.Text = $"Hoàn tất tải dữ liệu ngày {dateStr}";
                    }
                }
                else
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(response, "Tải báo cáo");
                    lblStatus.Text = "Không tải được báo cáo";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu báo cáo: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Lỗi kết nối máy chủ";
            }
        }
    }
}
