using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public partial class FormDashboard : Form
    {
        // Controls
        private Panel panelHeader;
        private Button btnRefresh;
        private Label lblLastUpdated;

        // Metric Cards
        private Panel panelCards;
        private Label lblTop1Product;
        private Label lblTotalRevenue;
        private Label lblTotalOrders;
        private Label lblTotalCashiers;

        // Tables Section
        private SplitContainer splitMain;
        private DataGridView dgvTopProducts;
        private DataGridView dgvCashiers;

        public FormDashboard()
        {
            InitializeComponent();
            BuildUI();
            ApplyTheme();
        }

        private void BuildUI()
        {
            //this.Text = "TỔNG QUAN HỆ THỐNG & HIỆU SUẤT KINH DOANH";
            //this.Size = new Size(1150, 720);
            //this.BackColor = UITheme.FormBackground;

            // 1. TOOLBAR / STATUS PANEL (Tiêu đề chính đã hiển thị ở thanh top của FormMainShell)
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 46,
                BackColor = Color.White,
                Padding = new Padding(15, 6, 15, 6)
            };

            lblLastUpdated = new Label
            {
                Text = "Chưa tải dữ liệu",
                Font = UITheme.FontBody,
                ForeColor = UITheme.TextMuted,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Location = new Point(15, 14),
                AutoSize = true
            };
            panelHeader.Controls.Add(lblLastUpdated);

            btnRefresh = new Button
            {
                Text = "🔄 Làm mới dữ liệu",
                Size = new Size(140, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(panelHeader.Width - 155, 7),
                Cursor = Cursors.Hand
            };
            UITheme.ApplyButtonTheme(btnRefresh, UITheme.ButtonStyle.Primary);
            btnRefresh.Click += async (s, e) => await LoadDashboardDataAsync();
            panelHeader.Controls.Add(btnRefresh);

            // 2. METRIC CARDS PANEL
            panelCards = new Panel
            {
                Dock = DockStyle.Top,
                Height = 125,
                BackColor = UITheme.FormBackground,
                Padding = new Padding(15, 12, 15, 6)
            };

            int cardWidth = 260;
            int cardHeight = 100;
            int spacing = 15;

            // Card 1: Top 1 Bán chạy
            var cardTop1 = CreateCard("🏆 MẶT HÀNG BÁN CHẠY NHẤT", Color.FromArgb(124, 58, 237), new Point(15, 12), new Size(cardWidth, cardHeight));
            lblTop1Product = new Label
            {
                Text = "Đang tải...",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                ForeColor = Color.White
            };
            cardTop1.Controls.Add(lblTop1Product);
            lblTop1Product.BringToFront();

            // Card 2: Tổng Doanh thu
            var cardRev = CreateCard("💰 TỔNG DOANH THU THU VỀ", Color.FromArgb(16, 185, 129), new Point(15 + (cardWidth + spacing), 12), new Size(cardWidth, cardHeight));
            lblTotalRevenue = new Label
            {
                Text = "0 đ",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White
            };
            cardRev.Controls.Add(lblTotalRevenue);
            lblTotalRevenue.BringToFront();

            // Card 3: Tổng Đơn hàng
            var cardOrders = CreateCard("🧾 TỔNG ĐƠN ĐÃ XỬ LÝ", Color.FromArgb(37, 99, 235), new Point(15 + (cardWidth + spacing) * 2, 12), new Size(cardWidth, cardHeight));
            lblTotalOrders = new Label
            {
                Text = "0",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.White
            };
            cardOrders.Controls.Add(lblTotalOrders);
            lblTotalOrders.BringToFront();

            // Card 4: Tổng Thu ngân
            var cardCashiers = CreateCard("👥 THU NGÂN PHỤC VỤ", Color.FromArgb(245, 158, 11), new Point(15 + (cardWidth + spacing) * 3, 12), new Size(cardWidth, cardHeight));
            lblTotalCashiers = new Label
            {
                Text = "0",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.White
            };
            cardCashiers.Controls.Add(lblTotalCashiers);
            lblTotalCashiers.BringToFront();

            panelCards.Controls.AddRange(new Control[] { cardTop1, cardRev, cardOrders, cardCashiers });

            // 3. MAIN SPLIT SECTION (Top 5 Products & Cashier Performance)
            splitMain = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 650,
                SplitterWidth = 8,
                BackColor = UITheme.FormBackground,
                Padding = new Padding(15, 6, 15, 15)
            };

            // LEFT PANEL: TOP 5 PRODUCTS
            Panel pnlLeft = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(14)
            };

            Label lblTopTitle = new Label
            {
                Text = "🔥 TOP 5 SẢN PHẨM BÁN CHẠY NHẤT",
                Font = UITheme.FontSubtitle,
                ForeColor = UITheme.TextPrimary,
                Dock = DockStyle.Top,
                Height = 36
            };
            pnlLeft.Controls.Add(lblTopTitle);

            dgvTopProducts = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            dgvTopProducts.Columns.Add("Rank", "Hạng");
            dgvTopProducts.Columns.Add("ProductId", "Mã SP");
            dgvTopProducts.Columns.Add("ProductName", "Tên Sản Phẩm");
            dgvTopProducts.Columns.Add("Unit", "ĐVT");
            dgvTopProducts.Columns.Add("Price", "Đơn Giá");
            dgvTopProducts.Columns.Add("TotalSoldQuantity", "Số Lượng Bán");
            dgvTopProducts.Columns.Add("TotalRevenue", "Tổng Doanh Thu");

            dgvTopProducts.Columns["Rank"].Width = 65;
            dgvTopProducts.Columns["ProductId"].Width = 65;
            dgvTopProducts.Columns["ProductName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvTopProducts.Columns["Unit"].Width = 65;
            dgvTopProducts.Columns["Price"].Width = 100;
            dgvTopProducts.Columns["TotalSoldQuantity"].Width = 110;
            dgvTopProducts.Columns["TotalRevenue"].Width = 135;

            dgvTopProducts.Columns["Price"].DefaultCellStyle.Format = "N0";
            dgvTopProducts.Columns["TotalSoldQuantity"].DefaultCellStyle.Format = "N0";
            dgvTopProducts.Columns["TotalRevenue"].DefaultCellStyle.Format = "N0";
            dgvTopProducts.Columns["TotalSoldQuantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvTopProducts.Columns["TotalRevenue"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            pnlLeft.Controls.Add(dgvTopProducts);
            dgvTopProducts.BringToFront();
            splitMain.Panel1.Controls.Add(pnlLeft);

            // RIGHT PANEL: CASHIER REVENUE
            Panel pnlRight = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(14)
            };

            Label lblCashierTitle = new Label
            {
                Text = "👤 DOANH THU THEO THU NGÂN",
                Font = UITheme.FontSubtitle,
                ForeColor = UITheme.TextPrimary,
                Dock = DockStyle.Top,
                Height = 36
            };
            pnlRight.Controls.Add(lblCashierTitle);

            dgvCashiers = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            dgvCashiers.Columns.Add("CashierUsername", "Thu Ngân");
            dgvCashiers.Columns.Add("TotalOrders", "Số Đơn");
            dgvCashiers.Columns.Add("TotalRevenue", "Doanh Thu");
            dgvCashiers.Columns.Add("TotalPointsAwarded", "Điểm Đã Tặng");

            dgvCashiers.Columns["CashierUsername"].Width = 110;
            dgvCashiers.Columns["TotalOrders"].Width = 80;
            dgvCashiers.Columns["TotalRevenue"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvCashiers.Columns["TotalPointsAwarded"].Width = 105;

            dgvCashiers.Columns["TotalOrders"].DefaultCellStyle.Format = "N0";
            dgvCashiers.Columns["TotalRevenue"].DefaultCellStyle.Format = "N0";
            dgvCashiers.Columns["TotalPointsAwarded"].DefaultCellStyle.Format = "N0";
            dgvCashiers.Columns["TotalOrders"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCashiers.Columns["TotalRevenue"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvCashiers.Columns["TotalPointsAwarded"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            pnlRight.Controls.Add(dgvCashiers);
            dgvCashiers.BringToFront();
            splitMain.Panel2.Controls.Add(pnlRight);

            // ADD PANELS TO FORM
            this.Controls.Add(splitMain);
            this.Controls.Add(panelCards);
            this.Controls.Add(panelHeader);

            this.Load += async (s, e) =>
            {
                await LoadDashboardDataAsync();
            };
        }

        private Panel CreateCard(string title, Color bg, Point location, Size size)
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
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White
            };
            card.Controls.Add(lbl);
            return card;
        }

        private void ApplyTheme()
        {
            UITheme.ApplyFormTheme(this);
            UITheme.ApplyGridTheme(dgvTopProducts);
            UITheme.ApplyGridTheme(dgvCashiers);
        }

        public async Task LoadDashboardDataAsync()
        {
            btnRefresh.Enabled = false;
            btnRefresh.Text = "⏳ Đang tải...";
            lblLastUpdated.Text = "Đang đồng bộ dữ liệu từ máy chủ API...";

            try
            {
                // Gọi song song cả 2 API báo cáo
                var taskTop5 = SessionManager.ApiClientService.Client.GetAsync("Reports/top-5-selling-products");
                var taskCashier = SessionManager.ApiClientService.Client.GetAsync("Reports/revenue-by-cashier");

                await Task.WhenAll(taskTop5, taskCashier);

                var resTop5 = await taskTop5;
                var resCashier = await taskCashier;

                // 1. XỬ LÝ DỮ LIỆU TOP 5 SẢN PHẨM
                if (resTop5.IsSuccessStatusCode)
                {
                    var top5Result = await resTop5.Content.ReadFromJsonAsync<TopSellingProductsResponseDto>();
                    dgvTopProducts.SuspendLayout();
                    try
                    {
                        dgvTopProducts.Rows.Clear();

                        if (top5Result != null && top5Result.Data != null && top5Result.Data.Count > 0)
                        {
                            var top1 = top5Result.Data.FirstOrDefault();
                            if (top1 != null)
                            {
                                lblTop1Product.Text = $"{top1.ProductName} ({top1.TotalSoldQuantity} {top1.Unit})";
                            }

                            int rank = 1;
                            string[] medals = { "🥇 #1", "🥈 #2", "🥉 #3", "🏅 #4", "🏅 #5" };
                            foreach (var item in top5Result.Data)
                            {
                                string medal = rank <= medals.Length ? medals[rank - 1] : $"#{rank}";
                                int idx = dgvTopProducts.Rows.Add(
                                    medal,
                                    item.ProductId,
                                    item.ProductName,
                                    item.Unit,
                                    item.Price,
                                    item.TotalSoldQuantity,
                                    item.TotalRevenue
                                );

                                // Highlight top 1 row
                                if (rank == 1)
                                {
                                    dgvTopProducts.Rows[idx].DefaultCellStyle.BackColor = Color.FromArgb(243, 232, 255);
                                    dgvTopProducts.Rows[idx].DefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                                }
                                rank++;
                            }
                        }
                        else
                        {
                            lblTop1Product.Text = "Chưa có dữ liệu";
                        }
                    }
                    finally
                    {
                        dgvTopProducts.ResumeLayout();
                    }
                }
                else
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(resTop5, "Tải Top 5 sản phẩm");
                }

                // 2. XỬ LÝ DỮ LIỆU DOANH THU THU NGÂN
                if (resCashier.IsSuccessStatusCode)
                {
                    var cashierResult = await resCashier.Content.ReadFromJsonAsync<CashierRevenueResponseDto>();
                    dgvCashiers.SuspendLayout();
                    try
                    {
                        dgvCashiers.Rows.Clear();

                        if (cashierResult != null && cashierResult.Data != null)
                        {
                            lblTotalRevenue.Text = $"{cashierResult.GrandTotalRevenue:N0} đ";
                            lblTotalOrders.Text = cashierResult.Data.Sum(c => c.TotalOrders).ToString("N0");
                            lblTotalCashiers.Text = cashierResult.TotalCashiers.ToString("N0");

                            foreach (var c in cashierResult.Data)
                            {
                                dgvCashiers.Rows.Add(
                                    c.CashierUsername,
                                    c.TotalOrders,
                                    c.TotalRevenue,
                                    c.TotalPointsAwarded
                                );
                            }
                        }
                    }
                    finally
                    {
                        dgvCashiers.ResumeLayout();
                    }
                }
                else
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(resCashier, "Tải Doanh thu thu ngân");
                }

                lblLastUpdated.Text = $"Cập nhật lúc: {DateTime.Now:HH:mm:ss dd/MM/yyyy}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối khi tải dữ liệu Dashboard: " + ex.Message, "Lỗi Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblLastUpdated.Text = "Lỗi nạp dữ liệu!";
            }
            finally
            {
                btnRefresh.Enabled = true;
                btnRefresh.Text = "🔄 Làm mới dữ liệu";
            }
        }

        private void FormDashboard_Load(object sender, EventArgs e)
        {

        }
    }
}
