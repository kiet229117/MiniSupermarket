using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public partial class FormQuickReport : Form
    {
        private Panel panelTop;
        private Label lblFromDate;
        private DateTimePicker dtpFromDate;
        private Label lblToDate;
        private DateTimePicker dtpToDate;
        private Button btnRunReport;
        private Button btnLast7Days;
        private Button btnToday;
        private Button btnExportExcel;
        private Label lblStatus;

        // Metric Cards
        private Label lblTotalOrders;
        private Label lblTotalRevenue;
        private Label lblBestSeller;

        // Biểu đồ cột Doanh thu (Tiêu chí 45)
        private Panel pnlChartContainer;
        private Label lblChartTitle;
        private Panel pnlChart;
        private readonly List<DailyRevenuePoint> _dailyRevenueList = new List<DailyRevenuePoint>();

        // DataGridView Hoá đơn
        private Label lblGridTitle;
        private DataGridView dgvOrders;
        private PaginationControl<RecentOrderDto> pager;
        private readonly List<RecentOrderDto> _reportOrders = new List<RecentOrderDto>();

        public FormQuickReport()
        {
            InitializeComponent();
            BuildReportUI();
            ApplyTheme();
        }

        private void BuildReportUI()
        {
            this.Text = "BÁO CÁO DOANH THU & HIỆU SUẤT KINH DOANH";
            this.Size = new Size(1100, 750);

            // 1. Top Filter Panel
            panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Color.White,
                Padding = new Padding(12, 10, 12, 10)
            };

            lblFromDate = new Label
            {
                Text = "Từ ngày:",
                Font = UITheme.FontBodyBold,
                Location = new Point(12, 16),
                AutoSize = true
            };
            panelTop.Controls.Add(lblFromDate);

            dtpFromDate = new DateTimePicker
            {
                Location = new Point(78, 13),
                Format = DateTimePickerFormat.Short,
                Font = UITheme.FontBody,
                Width = 110,
                Value = DateTime.Today.AddDays(-6)
            };
            panelTop.Controls.Add(dtpFromDate);

            lblToDate = new Label
            {
                Text = "Đến ngày:",
                Font = UITheme.FontBodyBold,
                Location = new Point(198, 16),
                AutoSize = true
            };
            panelTop.Controls.Add(lblToDate);

            dtpToDate = new DateTimePicker
            {
                Location = new Point(272, 13),
                Format = DateTimePickerFormat.Short,
                Font = UITheme.FontBody,
                Width = 110,
                Value = DateTime.Today
            };
            panelTop.Controls.Add(dtpToDate);

            btnRunReport = new Button
            {
                Text = "📊 Xem báo cáo",
                Location = new Point(392, 11),
                Size = new Size(120, 30),
                Cursor = Cursors.Hand
            };
            UITheme.ApplyButtonTheme(btnRunReport, UITheme.ButtonStyle.Primary);
            btnRunReport.Click += async (s, e) => await LoadDateRangeReportAsync();
            panelTop.Controls.Add(btnRunReport);

            btnLast7Days = new Button
            {
                Text = "7 ngày",
                Location = new Point(518, 11),
                Size = new Size(75, 30),
                Cursor = Cursors.Hand
            };
            UITheme.ApplyButtonTheme(btnLast7Days, UITheme.ButtonStyle.Secondary);
            btnLast7Days.Click += async (s, e) =>
            {
                dtpFromDate.Value = DateTime.Today.AddDays(-6);
                dtpToDate.Value = DateTime.Today;
                await LoadDateRangeReportAsync();
            };
            panelTop.Controls.Add(btnLast7Days);

            btnToday = new Button
            {
                Text = "Hôm nay",
                Location = new Point(598, 11),
                Size = new Size(80, 30),
                Cursor = Cursors.Hand
            };
            UITheme.ApplyButtonTheme(btnToday, UITheme.ButtonStyle.Secondary);
            btnToday.Click += async (s, e) =>
            {
                dtpFromDate.Value = DateTime.Today;
                dtpToDate.Value = DateTime.Today;
                await LoadDateRangeReportAsync();
            };
            panelTop.Controls.Add(btnToday);

            btnExportExcel = new Button
            {
                Text = "📥 Xuất Excel",
                Location = new Point(683, 11),
                Size = new Size(105, 30),
                Cursor = Cursors.Hand
            };
            UITheme.ApplyButtonTheme(btnExportExcel, UITheme.ButtonStyle.Primary);
            btnExportExcel.Click += BtnExportExcel_Click;
            panelTop.Controls.Add(btnExportExcel);

            lblStatus = new Label
            {
                Text = "Sẵn sàng",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.TextMuted,
                Location = new Point(798, 18),
                AutoSize = true
            };
            panelTop.Controls.Add(lblStatus);

            // 2. Cards Panel
            Panel panelCards = new Panel
            {
                Dock = DockStyle.Top,
                Height = 115,
                BackColor = UITheme.FormBackground,
                Padding = new Padding(15, 10, 15, 6)
            };

            int cardWidth = 320;
            int cardHeight = 95;
            int spacing = 15;

            // Thẻ 1: Tổng số đơn
            Panel pnlCard1 = CreateMetricCard("TỔNG SỐ HÓA ĐƠN", Color.FromArgb(41, 128, 185), new Point(15, 10), new Size(cardWidth, cardHeight));
            lblTotalOrders = new Label
            {
                Text = "0",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = Color.White
            };
            pnlCard1.Controls.Add(lblTotalOrders);
            lblTotalOrders.BringToFront();

            // Thẻ 2: Doanh thu
            Panel pnlCard2 = CreateMetricCard("TỔNG DOANH THU", Color.FromArgb(39, 174, 96), new Point(15 + cardWidth + spacing, 10), new Size(cardWidth, cardHeight));
            lblTotalRevenue = new Label
            {
                Text = "0 đ",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.White
            };
            pnlCard2.Controls.Add(lblTotalRevenue);
            lblTotalRevenue.BringToFront();

            // Thẻ 3: Bán chạy nhất
            Panel pnlCard3 = CreateMetricCard("MẶT HÀNG BÁN CHẠY NHẤT", Color.FromArgb(142, 68, 173), new Point(15 + (cardWidth + spacing) * 2, 10), new Size(cardWidth, cardHeight));
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

            // 3. Middle Section: Biểu đồ cột Doanh thu theo ngày (Tiêu chí 45)
            pnlChartContainer = new Panel
            {
                Dock = DockStyle.Top,
                Height = 220,
                BackColor = Color.White,
                Padding = new Padding(15, 6, 15, 6)
            };

            lblChartTitle = new Label
            {
                Text = "📈 BIỂU ĐỒ CỘT DOANH THU THEO NGÀY (VNĐ)",
                Font = UITheme.FontSubtitle,
                ForeColor = UITheme.TextPrimary,
                Dock = DockStyle.Top,
                Height = 28
            };

            pnlChart = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            pnlChart.Paint += PnlChart_Paint;

            pnlChartContainer.Controls.Add(pnlChart);
            pnlChartContainer.Controls.Add(lblChartTitle);

            // 4. Bottom Panel: Bảng các hóa đơn
            Panel panelBottom = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(15, 8, 15, 12)
            };

            lblGridTitle = new Label
            {
                Text = "DANH SÁCH CHI TIẾT CÁC HÓA ĐƠN TRONG KHOẢNG THỜI GIAN",
                Font = UITheme.FontSubtitle,
                ForeColor = UITheme.TextPrimary,
                Dock = DockStyle.Top,
                Height = 28
            };

            dgvOrders = new DataGridView { Dock = DockStyle.Fill };
            dgvOrders.Columns.Add("OrderId", "Mã Đơn");
            dgvOrders.Columns.Add("OrderCode", "Mã Hóa Đơn");
            dgvOrders.Columns.Add("CustomerName", "Khách Hàng");
            dgvOrders.Columns.Add("PaymentMethod", "Phương Thức");
            dgvOrders.Columns.Add("Total", "Tổng Tiền (VNĐ)");
            dgvOrders.Columns.Add("CreatedAt", "Thời Gian Lập");

            dgvOrders.Columns["OrderId"].Width = 80;
            dgvOrders.Columns["OrderCode"].Width = 140;
            dgvOrders.Columns["CustomerName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvOrders.Columns["PaymentMethod"].Width = 130;
            dgvOrders.Columns["Total"].Width = 140;
            dgvOrders.Columns["CreatedAt"].Width = 160;

            dgvOrders.Columns["Total"].DefaultCellStyle.Format = "N0";
            dgvOrders.Columns["CreatedAt"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";

            pager = new PaginationControl<RecentOrderDto>();
            pager.Bind(DisplayOrders, "hóa đơn");

            panelBottom.Controls.Add(dgvOrders);
            panelBottom.Controls.Add(pager);
            panelBottom.Controls.Add(lblGridTitle);

            this.Controls.Add(panelBottom);
            this.Controls.Add(pnlChartContainer);
            this.Controls.Add(panelCards);
            this.Controls.Add(panelTop);

            this.Load += async (s, e) =>
            {
                await LoadDateRangeReportAsync();
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
                Height = 28,
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

        // =======================================================================
        // VẼ BIỂU ĐỒ CỘT BẰNG GDI+ (Tiêu chí 45: Biểu đồ Cột Doanh thu theo ngày)
        // =======================================================================
        private void PnlChart_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int width = pnlChart.Width;
            int height = pnlChart.Height;

            int paddingLeft = 60;
            int paddingRight = 20;
            int paddingTop = 25;
            int paddingBottom = 35;

            int chartWidth = width - paddingLeft - paddingRight;
            int chartHeight = height - paddingTop - paddingBottom;

            if (chartWidth <= 0 || chartHeight <= 0) return;

            // Đường trục tọa độ
            using (var axisPen = new Pen(Color.FromArgb(203, 213, 225), 1.5f))
            {
                g.DrawLine(axisPen, paddingLeft, paddingTop, paddingLeft, height - paddingBottom);
                g.DrawLine(axisPen, paddingLeft, height - paddingBottom, width - paddingRight, height - paddingBottom);
            }

            if (_dailyRevenueList == null || _dailyRevenueList.Count == 0)
            {
                using (var brush = new SolidBrush(UITheme.TextMuted))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("Không có dữ liệu doanh thu trong khoảng thời gian đã chọn", UITheme.FontBody, brush, new RectangleF(0, 0, width, height), sf);
                }
                return;
            }

            decimal maxRev = _dailyRevenueList.Max(x => x.Revenue);
            if (maxRev <= 0) maxRev = 1000000;

            // Vẽ các đường kẻ ngang chia mức
            using (var gridPen = new Pen(Color.FromArgb(241, 245, 249), 1))
            using (var textBrush = new SolidBrush(UITheme.TextMuted))
            {
                int steps = 4;
                for (int i = 0; i <= steps; i++)
                {
                    float y = paddingTop + (chartHeight * (steps - i) / (float)steps);
                    g.DrawLine(gridPen, paddingLeft, y, width - paddingRight, y);

                    decimal val = (maxRev * i) / steps;
                    string label = val >= 1000000 ? $"{(val / 1000000m):0.#}M" : (val >= 1000 ? $"{(val / 1000m):0}k" : "0");
                    g.DrawString(label, UITheme.FontSmall, textBrush, new PointF(10, y - 7));
                }
            }

            // Vẽ từng cột biểu đồ
            int count = _dailyRevenueList.Count;
            float slotWidth = chartWidth / (float)count;
            float barWidth = Math.Min(45f, slotWidth * 0.65f);

            using (var barBrush = new LinearGradientBrush(
                new Rectangle(0, 0, 10, chartHeight),
                Color.FromArgb(59, 130, 246),
                Color.FromArgb(37, 99, 235),
                LinearGradientMode.Vertical))
            using (var topValBrush = new SolidBrush(Color.FromArgb(30, 41, 59)))
            using (var dateBrush = new SolidBrush(Color.FromArgb(71, 85, 105)))
            {
                var sfCenter = new StringFormat { Alignment = StringAlignment.Center };

                for (int i = 0; i < count; i++)
                {
                    var item = _dailyRevenueList[i];
                    float barHeight = maxRev > 0 ? (float)((item.Revenue / maxRev) * chartHeight) : 0;
                    if (barHeight < 2 && item.Revenue > 0) barHeight = 2;

                    float xCenter = paddingLeft + (i + 0.5f) * slotWidth;
                    float x = xCenter - (barWidth / 2);
                    float y = (height - paddingBottom) - barHeight;

                    // Vẽ cột chữ nhật
                    if (barHeight > 0)
                    {
                        g.FillRectangle(barBrush, x, y, barWidth, barHeight);
                        using (var borderPen = new Pen(Color.FromArgb(29, 78, 216), 1f))
                        {
                            g.DrawRectangle(borderPen, x, y, barWidth, barHeight);
                        }
                    }

                    // Nhãn số tiền trên đỉnh cột
                    string revText = item.Revenue >= 1000000
                        ? $"{(item.Revenue / 1000000m):0.##}M"
                        : (item.Revenue > 0 ? $"{(item.Revenue / 1000m):0.#}k" : "0");
                    g.DrawString(revText, UITheme.FontSmall, topValBrush, new PointF(xCenter, y - 16), sfCenter);

                    // Nhãn ngày bên dưới cột (dd/MM)
                    string dateLabel = item.Date;
                    if (DateTime.TryParse(item.Date, out var parsedDate))
                    {
                        dateLabel = parsedDate.ToString("dd/MM");
                    }
                    g.DrawString(dateLabel, UITheme.FontSmall, dateBrush, new PointF(xCenter, height - paddingBottom + 6), sfCenter);
                }
            }
        }

        // =======================================================================
        // TẢI BÁO CÁO KHOẢNG THỜI GIAN: GET /api/Reports/date-range
        // =======================================================================
        private async Task LoadDateRangeReportAsync()
        {
            string fromStr = dtpFromDate.Value.ToString("yyyy-MM-dd");
            string toStr = dtpToDate.Value.ToString("yyyy-MM-dd");

            lblStatus.Text = "Đang tính toán số liệu...";
            btnRunReport.Enabled = false;

            try
            {
                var response = await SessionManager.ApiClientService.Client.GetAsync($"Reports/date-range?fromDate={fromStr}&toDate={toStr}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonDoc = await response.Content.ReadFromJsonAsync<JsonElement>();

                    int totalOrders = jsonDoc.GetProperty("totalOrders").GetInt32();
                    decimal totalRevenue = jsonDoc.GetProperty("totalRevenue").GetDecimal();

                    lblTotalOrders.Text = totalOrders.ToString("N0");
                    lblTotalRevenue.Text = $"{totalRevenue:N0} đ";

                    if (jsonDoc.TryGetProperty("topProducts", out var topProds) && topProds.GetArrayLength() > 0)
                    {
                        var first = topProds[0];
                        string pName = first.GetProperty("productName").GetString() ?? "";
                        int qty = first.GetProperty("totalQuantity").GetInt32();
                        lblBestSeller.Text = $"{pName} ({qty})";
                    }
                    else
                    {
                        lblBestSeller.Text = "Chưa có";
                    }

                    // Cập nhật dữ liệu vẽ biểu đồ cột
                    _dailyRevenueList.Clear();
                    if (jsonDoc.TryGetProperty("dailyRevenue", out var dailyRev))
                    {
                        foreach (var d in dailyRev.EnumerateArray())
                        {
                            _dailyRevenueList.Add(new DailyRevenuePoint
                            {
                                Date = d.GetProperty("date").GetString() ?? "",
                                Revenue = d.GetProperty("revenue").GetDecimal(),
                                OrderCount = d.GetProperty("orderCount").GetInt32()
                            });
                        }
                    }

                    // Tự động làm mịn khoảng ngày nếu ít dữ liệu (đảm bảo đủ các ngày trong dải)
                    FillMissingDates();
                    pnlChart.Invalidate(); // Yêu cầu vẽ lại biểu đồ

                    // Cập nhật DataGridView Hóa đơn qua phân trang
                    _reportOrders.Clear();
                    if (jsonDoc.TryGetProperty("recentOrders", out var ords))
                    {
                        foreach (var ord in ords.EnumerateArray())
                        {
                            string cName = ord.GetProperty("customerName").GetString();
                            _reportOrders.Add(new RecentOrderDto
                            {
                                OrderId = ord.GetProperty("orderId").GetInt32(),
                                OrderCode = ord.GetProperty("orderCode").GetString(),
                                CustomerName = (string.IsNullOrWhiteSpace(cName) || cName == "Khách vãng lai") ? "Khách mua tại quầy" : cName,
                                PaymentMethod = ord.GetProperty("paymentMethod").GetString(),
                                Total = ord.GetProperty("total").GetDecimal(),
                                CreatedAt = ord.TryGetProperty("createdAt", out var cr) ? cr.GetDateTime() : DateTime.Now
                            });
                        }
                    }
                    pager.SetDataSource(_reportOrders);

                    lblStatus.Text = $"Hoàn tất tải dữ liệu từ {fromStr} đến {toStr}";
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
            finally
            {
                btnRunReport.Enabled = true;
            }
        }

        private void DisplayOrders(List<RecentOrderDto> list)
        {
            if (this.IsDisposed || this.Disposing || dgvOrders == null || dgvOrders.IsDisposed) return;
            dgvOrders.SuspendLayout();
            try
            {
                dgvOrders.Rows.Clear();
                foreach (var ord in list)
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
            finally
            {
                dgvOrders.ResumeLayout();
            }
        }

        private void FillMissingDates()
        {
            var dict = _dailyRevenueList.ToDictionary(x => x.Date, x => x);
            var result = new List<DailyRevenuePoint>();

            DateTime curr = dtpFromDate.Value.Date;
            DateTime end = dtpToDate.Value.Date;

            // Nếu khoảng cách ngày hợp lý (<= 31 ngày), điền đủ các ngày có doanh thu = 0
            if ((end - curr).TotalDays <= 31)
            {
                while (curr <= end)
                {
                    string k = curr.ToString("yyyy-MM-dd");
                    if (dict.TryGetValue(k, out var found))
                    {
                        result.Add(found);
                    }
                    else
                    {
                        result.Add(new DailyRevenuePoint { Date = k, Revenue = 0, OrderCount = 0 });
                    }
                    curr = curr.AddDays(1);
                }
                _dailyRevenueList.Clear();
                _dailyRevenueList.AddRange(result);
            }
        }

        // =======================================================================
        // Tiêu chí 48: XUẤT BÁO CÁO DOANH THU & HÓA ĐƠN RA FILE EXCEL (CSV UTF-8)
        // =======================================================================
        private void BtnExportExcel_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Tệp Excel CSV (*.csv)|*.csv";
                sfd.FileName = $"Bao_Cao_Doanh_Thu_{dtpFromDate.Value:yyyyMMdd}_{dtpToDate.Value:yyyyMMdd}.csv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                        {
                            sw.WriteLine($"\"BÁO CÁO DOANH THU TỪ NGÀY {dtpFromDate.Value:dd/MM/yyyy} ĐẾN {dtpToDate.Value:dd/MM/yyyy}\"");
                            sw.WriteLine($"\"Tổng số hóa đơn:\",\"{lblTotalOrders.Text}\"");
                            sw.WriteLine($"\"Tổng doanh thu:\",\"{lblTotalRevenue.Text}\"");
                            sw.WriteLine($"\"Mặt hàng bán chạy nhất:\",\"{lblBestSeller.Text}\"");
                            sw.WriteLine("");

                            // Bảng thống kê theo ngày
                            sw.WriteLine("\"THỐNG KÊ DOANH THU THEO TỪNG NGÀY\"");
                            sw.WriteLine("Ngày,Số Hóa Đơn,Doanh Thu (VNĐ)");
                            foreach (var d in _dailyRevenueList)
                            {
                                sw.WriteLine($"\"{d.Date}\",\"{d.OrderCount}\",\"{d.Revenue}\"");
                            }
                            sw.WriteLine("");

                            // Bảng danh sách hóa đơn chi tiết
                            sw.WriteLine("\"DANH SÁCH HÓA ĐƠN CHI TIẾT\"");
                            sw.WriteLine("Mã Đơn,Mã Hóa Đơn,Khách Hàng,Phương Thức,Tổng Tiền (VNĐ),Thời Gian Lập");
                            foreach (DataGridViewRow row in dgvOrders.Rows)
                            {
                                if (row.IsNewRow) continue;
                                var vals = new List<string>();
                                for (int c = 0; c < dgvOrders.Columns.Count; c++)
                                {
                                    string v = row.Cells[c].Value?.ToString() ?? "";
                                    v = "\"" + v.Replace("\"", "\"\"") + "\"";
                                    vals.Add(v);
                                }
                                sw.WriteLine(string.Join(",", vals));
                            }
                        }

                        if (MessageBox.Show("Xuất báo cáo doanh thu ra Excel thành công!\nBạn có muốn mở tệp ngay không?", "Xuất Excel", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start(sfd.FileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }

    public class DailyRevenuePoint
    {
        public string Date { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }
}
