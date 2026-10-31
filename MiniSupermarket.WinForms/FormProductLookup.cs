using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public class FormProductLookup : Form
    {
        private readonly List<ProductDto> _allProducts = new List<ProductDto>();
        private TextBox txtSearch;
        private DataGridView dgvProducts;
        private Label lblStatus;
        private PaginationControl<ProductDto> pager;
        private Button btnSelect;
        private Button btnCancel;

        public string SelectedBarcode { get; private set; } = string.Empty;

        public FormProductLookup()
        {
            InitializeUI();
            this.Load += async (s, e) => await LoadProductsAsync();
        }

        private void InitializeUI()
        {
            this.Text = "🔍 TRA CỨU & CHỌN SẢN PHẨM BÁN HÀNG";
            this.Size = new Size(780, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = UITheme.FormBackground;
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape) this.Close();
                else if (e.KeyCode == Keys.Enter && dgvProducts.Focused) SelectCurrentProduct();
            };

            // 1. Top Panel
            Panel pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Color.White,
                Padding = new Padding(12, 10, 12, 10)
            };

            Label lblPrompt = new Label
            {
                Text = "Tìm kiếm:",
                Font = UITheme.FontBodyBold,
                Location = new Point(14, 16),
                AutoSize = true
            };
            pnlTop.Controls.Add(lblPrompt);

            txtSearch = new TextBox
            {
                Font = UITheme.FontBody,
                Location = new Point(88, 12),
                Size = new Size(320, 26)
            };
            UITheme.SetupSearchPlaceholder(txtSearch, "Nhập tên hoặc mã vạch cần tìm...");
            txtSearch.TextChanged += (s, e) => FilterProducts();
            txtSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Down && dgvProducts.Rows.Count > 0)
                {
                    dgvProducts.Focus();
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    SelectCurrentProduct();
                }
            };
            pnlTop.Controls.Add(txtSearch);

            lblStatus = new Label
            {
                Text = "Đang tải dữ liệu...",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.TextMuted,
                Location = new Point(420, 16),
                AutoSize = true
            };
            pnlTop.Controls.Add(lblStatus);

            this.Controls.Add(pnlTop);

            // 2. Bottom Panel
            Panel pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.White,
                Padding = new Padding(12, 8, 12, 8)
            };

            btnSelect = new Button
            {
                Text = "✓ Chọn đưa vào giỏ (Enter)",
                Size = new Size(190, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(pnlBottom.Width - 300, 9),
                Cursor = Cursors.Hand
            };
            UITheme.ApplyButtonTheme(btnSelect, UITheme.ButtonStyle.Primary);
            btnSelect.Click += (s, e) => SelectCurrentProduct();
            pnlBottom.Controls.Add(btnSelect);

            btnCancel = new Button
            {
                Text = "Đóng (Esc)",
                Size = new Size(95, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(pnlBottom.Width - 105, 9),
                Cursor = Cursors.Hand
            };
            UITheme.ApplyButtonTheme(btnCancel, UITheme.ButtonStyle.Secondary);
            btnCancel.Click += (s, e) => this.Close();
            pnlBottom.Controls.Add(btnCancel);

            this.Controls.Add(pnlBottom);

            // 3. Grid Panel
            dgvProducts = new DataGridView
            {
                Dock = DockStyle.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false
            };

            dgvProducts.Columns.Add("Barcode", "Mã Vạch");
            dgvProducts.Columns.Add("ProductName", "Tên Sản Phẩm");
            dgvProducts.Columns.Add("Unit", "ĐVT");
            dgvProducts.Columns.Add("Price", "Giá Bán");
            dgvProducts.Columns.Add("StockQuantity", "Tồn Kho");

            dgvProducts.Columns["Barcode"].Width = 130;
            dgvProducts.Columns["ProductName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvProducts.Columns["Unit"].Width = 60;
            dgvProducts.Columns["Price"].Width = 110;
            dgvProducts.Columns["StockQuantity"].Width = 80;

            dgvProducts.Columns["Price"].DefaultCellStyle.Format = "N0";
            dgvProducts.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvProducts.Columns["StockQuantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvProducts.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0) SelectCurrentProduct();
            };

            UITheme.ApplyGridTheme(dgvProducts);
            pager = new PaginationControl<ProductDto>();
            pager.Bind(DisplayPagedProducts, "sản phẩm");

            this.Controls.Add(dgvProducts);
            this.Controls.Add(pager);
            dgvProducts.BringToFront();
        }

        private async Task LoadProductsAsync()
        {
            lblStatus.Text = "Đang tải danh sách sản phẩm...";
            try
            {
                if (SessionManager.DataCache.ActiveProducts != null && SessionManager.DataCache.ActiveProducts.Count > 0)
                {
                    _allProducts.Clear();
                    _allProducts.AddRange(SessionManager.DataCache.ActiveProducts);
                    FilterProducts();
                    txtSearch.Focus();
                    return;
                }

                var response = await SessionManager.ApiClientService.Client.GetAsync("Products");
                if (response.IsSuccessStatusCode)
                {
                    var list = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
                    _allProducts.Clear();
                    if (list != null)
                    {
                        var activeList = list.Where(p => p.IsActive).ToList();
                        _allProducts.AddRange(activeList);
                        SessionManager.DataCache.ActiveProducts = activeList;
                    }
                    FilterProducts();
                    txtSearch.Focus();
                }
                else
                {
                    lblStatus.Text = "Lỗi khi nạp dữ liệu!";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi kết nối: " + ex.Message;
            }
        }

        private void FilterProducts()
        {
            string kw = txtSearch.Text.Trim().ToLower();
            if (kw == "nhập tên hoặc mã vạch cần tìm...") kw = string.Empty;

            var filtered = _allProducts.Where(p =>
                string.IsNullOrEmpty(kw) ||
                (!string.IsNullOrEmpty(p.ProductName) && p.ProductName.ToLower().Contains(kw)) ||
                (!string.IsNullOrEmpty(p.Barcode) && p.Barcode.ToLower().Contains(kw))
            ).ToList();

            pager.SetDataSource(filtered);
            lblStatus.Text = $"Tìm thấy {filtered.Count} sản phẩm";
        }

        private void DisplayPagedProducts(List<ProductDto> list)
        {
            dgvProducts.SuspendLayout();
            try
            {
                dgvProducts.Rows.Clear();
                foreach (var p in list)
                {
                    int idx = dgvProducts.Rows.Add(
                        p.Barcode,
                        p.ProductName,
                        p.Unit,
                        p.Price,
                        p.StockQuantity
                    );
                }
            }
            finally
            {
                dgvProducts.ResumeLayout();
            }

            if (dgvProducts.Rows.Count > 0)
            {
                dgvProducts.Rows[0].Selected = true;
            }
        }

        private void SelectCurrentProduct()
        {
            if (dgvProducts.SelectedRows.Count > 0)
            {
                string barcode = dgvProducts.SelectedRows[0].Cells["Barcode"].Value?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(barcode))
                {
                    SelectedBarcode = barcode;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một sản phẩm từ danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // FormProductLookup
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "FormProductLookup";
            this.Load += new System.EventHandler(this.FormProductLookup_Load);
            this.ResumeLayout(false);

        }

        private void FormProductLookup_Load(object sender, EventArgs e)
        {

        }
    }
}
