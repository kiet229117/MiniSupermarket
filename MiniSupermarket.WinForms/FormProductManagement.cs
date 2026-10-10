using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public partial class FormProductManagement : Form
    {
        private const string SearchPlaceholder = "Tên hoặc Mã vạch...";
        private readonly List<ProductDto> _products = new List<ProductDto>();
        private readonly List<CategoryDto> _categories = new List<CategoryDto>();

        // Controls giao diện
        private Panel panelTop;
        private Label lblTitle;
        private Label lblStatus;
        private Label lblSearch;
        private TextBox txtSearch;
        private Label lblFilterCategory;
        private ComboBox cboFilterCategory;
        private Button btnSearch;
        private Button btnReload;

        private DataGridView dgvProducts;

        private Panel panelRight;
        private GroupBox grbDetails;
        private Label lblId;
        private TextBox txtId;
        private Label lblBarcode;
        private TextBox txtBarcode;
        private Label lblProductName;
        private TextBox txtProductName;
        private Label lblCategory;
        private ComboBox cboCategory;
        private Label lblUnit;
        private TextBox txtUnit;
        private Label lblPrice;
        private NumericUpDown nudPrice;
        private Label lblCostPrice;
        private NumericUpDown nudCostPrice;
        private Label lblStock;
        private NumericUpDown nudStock;
        private CheckBox chkIsActive;

        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnClear;

        public FormProductManagement()
        {
            InitializeComponent();
            BuildUI();
            ApplyTheme();
            this.Shown += FormProductManagement_Shown;
        }

        private void BuildUI()
        {
            this.Text = "QUẢN LÝ SẢN PHẨM & KHO HÀNG";
            this.Size = new Size(1100, 650);
            this.Load += FormProductManagement_Load;

            // 1. Panel Top
            panelTop = new Panel { Dock = DockStyle.Top, Height = 65, BackColor = Color.White };

            lblTitle = new Label
            {
                Text = "QUẢN LÝ SẢN PHẨM & KHO HÀNG",
                Font = UITheme.FontTitle,
                ForeColor = UITheme.PrimaryColor,
                Location = new Point(16, 10),
                AutoSize = true
            };
            panelTop.Controls.Add(lblTitle);

            lblStatus = new Label
            {
                Text = "Sẵn sàng",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.TextMuted,
                Location = new Point(18, 36),
                AutoSize = true
            };
            panelTop.Controls.Add(lblStatus);

            FlowLayoutPanel panelSearchFilter = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 16, 12, 0)
            };

            lblSearch = new Label
            {
                Text = "Tìm kiếm:",
                Font = UITheme.FontBodyBold,
                AutoSize = true,
                Margin = new Padding(0, 6, 4, 0)
            };
            panelSearchFilter.Controls.Add(lblSearch);

            txtSearch = new TextBox
            {
                Font = UITheme.FontBody,
                Size = new Size(130, 26),
                Margin = new Padding(0, 2, 8, 0)
            };
            UITheme.SetupSearchPlaceholder(txtSearch, SearchPlaceholder);
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnSearch_Click(s, e); };
            panelSearchFilter.Controls.Add(txtSearch);

            lblFilterCategory = new Label
            {
                Text = "Nhóm hàng:",
                Font = UITheme.FontBodyBold,
                AutoSize = true,
                Margin = new Padding(0, 6, 4, 0)
            };
            panelSearchFilter.Controls.Add(lblFilterCategory);

            cboFilterCategory = new ComboBox
            {
                Font = UITheme.FontBody,
                Size = new Size(135, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(0, 2, 8, 0)
            };
            cboFilterCategory.SelectedIndexChanged += async (s, e) =>
            {
                if (cboFilterCategory.Focused) await LoadProductsAsync();
            };
            panelSearchFilter.Controls.Add(cboFilterCategory);

            btnSearch = new Button
            {
                Text = "Tìm",
                Size = new Size(60, 28),
                Margin = new Padding(0, 0, 6, 0)
            };
            UITheme.ApplyButtonTheme(btnSearch, UITheme.ButtonStyle.Primary);
            btnSearch.Click += BtnSearch_Click;
            panelSearchFilter.Controls.Add(btnSearch);

            btnReload = new Button
            {
                Text = "Tải lại",
                Size = new Size(65, 28),
                Margin = new Padding(0, 0, 0, 0)
            };
            UITheme.ApplyButtonTheme(btnReload, UITheme.ButtonStyle.Secondary);
            btnReload.Click += BtnReload_Click;
            panelSearchFilter.Controls.Add(btnReload);

            panelTop.Controls.Add(panelSearchFilter);

            // 2. DataGridView hiển thị danh sách sản phẩm
            dgvProducts = new DataGridView { Dock = DockStyle.Fill };
            dgvProducts.Columns.Add("ProductId", "Mã SP");
            dgvProducts.Columns.Add("Barcode", "Mã Vạch");
            dgvProducts.Columns.Add("ProductName", "Tên Sản Phẩm");
            dgvProducts.Columns.Add("CategoryName", "Nhóm Hàng");
            dgvProducts.Columns.Add("Unit", "ĐVT");
            dgvProducts.Columns.Add("Price", "Giá Bán");
            dgvProducts.Columns.Add("CostPrice", "Giá Nhập");
            dgvProducts.Columns.Add("StockQuantity", "Tồn Kho");
            dgvProducts.Columns.Add("IsActive", "Kinh Doanh");

            dgvProducts.Columns["ProductId"].Width = 65;
            dgvProducts.Columns["Barcode"].Width = 120;
            dgvProducts.Columns["ProductName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvProducts.Columns["CategoryName"].Width = 140;
            dgvProducts.Columns["Unit"].Width = 60;
            dgvProducts.Columns["Price"].Width = 90;
            dgvProducts.Columns["CostPrice"].Width = 90;
            dgvProducts.Columns["StockQuantity"].Width = 75;
            dgvProducts.Columns["IsActive"].Width = 85;

            // Format hiển thị số tiền cho Price & CostPrice
            dgvProducts.Columns["Price"].DefaultCellStyle.Format = "N0";
            dgvProducts.Columns["CostPrice"].DefaultCellStyle.Format = "N0";
            dgvProducts.Columns["StockQuantity"].DefaultCellStyle.Format = "N0";

            dgvProducts.CellClick += DgvProducts_CellClick;

            // 3. Panel bên phải chứa form nhập liệu
            panelRight = new Panel
            {
                Dock = DockStyle.Right,
                Width = 360,
                BackColor = Color.White,
                Padding = new Padding(10),
                AutoScroll = true
            };

            grbDetails = new GroupBox
            {
                Text = "Thông tin sản phẩm",
                Font = UITheme.FontSubtitle,
                ForeColor = UITheme.TextPrimary,
                Dock = DockStyle.Top,
                Height = 325
            };
            panelRight.Controls.Add(grbDetails);

            lblId = new Label { Text = "Mã SP:", Font = UITheme.FontBodyBold, Location = new Point(15, 25), AutoSize = true };
            txtId = new TextBox { Font = UITheme.FontBody, Location = new Point(15, 45), Size = new Size(130, 25), ReadOnly = true, BackColor = Color.FromArgb(245, 245, 245) };

            lblBarcode = new Label { Text = "Mã vạch (*):", Font = UITheme.FontBodyBold, Location = new Point(155, 25), AutoSize = true };
            txtBarcode = new TextBox { Font = UITheme.FontBody, Location = new Point(155, 45), Size = new Size(160, 25) };

            lblProductName = new Label { Text = "Tên sản phẩm (*):", Font = UITheme.FontBodyBold, Location = new Point(15, 75), AutoSize = true };
            txtProductName = new TextBox { Font = UITheme.FontBody, Location = new Point(15, 95), Size = new Size(300, 25) };

            lblCategory = new Label { Text = "Nhóm hàng (*):", Font = UITheme.FontBodyBold, Location = new Point(15, 125), AutoSize = true };
            cboCategory = new ComboBox { Font = UITheme.FontBody, Location = new Point(15, 145), Size = new Size(300, 25), DropDownStyle = ComboBoxStyle.DropDownList };

            lblUnit = new Label { Text = "Đơn vị tính:", Font = UITheme.FontBodyBold, Location = new Point(15, 175), AutoSize = true };
            txtUnit = new TextBox { Font = UITheme.FontBody, Location = new Point(15, 195), Size = new Size(130, 25), Text = "cái" };

            lblStock = new Label { Text = "Số lượng tồn:", Font = UITheme.FontBodyBold, Location = new Point(155, 175), AutoSize = true };
            nudStock = new NumericUpDown { Font = UITheme.FontBody, Location = new Point(155, 195), Size = new Size(160, 25), Maximum = 1000000 };

            lblPrice = new Label { Text = "Giá bán (VNĐ):", Font = UITheme.FontBodyBold, Location = new Point(15, 225), AutoSize = true };
            nudPrice = new NumericUpDown { Font = UITheme.FontBody, Location = new Point(15, 245), Size = new Size(130, 25), Maximum = 1000000000, Increment = 1000, ThousandsSeparator = true };

            lblCostPrice = new Label { Text = "Giá vốn (VNĐ):", Font = UITheme.FontBodyBold, Location = new Point(155, 225), AutoSize = true };
            nudCostPrice = new NumericUpDown { Font = UITheme.FontBody, Location = new Point(155, 245), Size = new Size(160, 25), Maximum = 1000000000, Increment = 1000, ThousandsSeparator = true };

            chkIsActive = new CheckBox { Text = "Đang kinh doanh", Font = UITheme.FontBodyBold, Location = new Point(15, 285), AutoSize = true, Checked = true };

            grbDetails.Controls.AddRange(new Control[] {
                lblId, txtId,
                lblBarcode, txtBarcode,
                lblProductName, txtProductName,
                lblCategory, cboCategory,
                lblUnit, txtUnit,
                lblStock, nudStock,
                lblPrice, nudPrice,
                lblCostPrice, nudCostPrice,
                chkIsActive
            });

            // 4 Nút thao tác CRUD (Bố trí 2 hàng x 2 cột xuống dòng gọn gàng, không tràn khung)
            FlowLayoutPanel panelButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(2, 8, 2, 8)
            };

            int btnWidth = 158;
            int btnHeight = 36;

            btnAdd = new Button
            {
                Text = "➕ Thêm SP",
                Size = new Size(btnWidth, btnHeight),
                Margin = new Padding(4)
            };
            UITheme.ApplyButtonTheme(btnAdd, UITheme.ButtonStyle.Success);
            btnAdd.Click += async (s, e) => await CreateProductAsync();

            btnUpdate = new Button
            {
                Text = "✏ Cập Nhật",
                Size = new Size(btnWidth, btnHeight),
                Margin = new Padding(4)
            };
            UITheme.ApplyButtonTheme(btnUpdate, UITheme.ButtonStyle.Warning);
            btnUpdate.Click += async (s, e) => await UpdateProductAsync();

            btnDelete = new Button
            {
                Text = "🗑 Xóa / Ngừng",
                Size = new Size(btnWidth, btnHeight),
                Margin = new Padding(4)
            };
            UITheme.ApplyButtonTheme(btnDelete, UITheme.ButtonStyle.Danger);
            btnDelete.Click += async (s, e) => await DeleteProductAsync();

            btnClear = new Button
            {
                Text = "🔄 Làm Mới Ô",
                Size = new Size(btnWidth, btnHeight),
                Margin = new Padding(4)
            };
            UITheme.ApplyButtonTheme(btnClear, UITheme.ButtonStyle.Secondary);
            btnClear.Click += (s, e) => ClearForm();

            panelButtons.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnClear });
            panelRight.Controls.Add(panelButtons);
            panelButtons.BringToFront();

            this.Controls.Add(dgvProducts);
            this.Controls.Add(panelTop);
            this.Controls.Add(panelRight);
        }

        private void ApplyTheme()
        {
            UITheme.ApplyFormTheme(this);
            UITheme.ApplyGridTheme(dgvProducts);
        }

        private void SetPermission()
        {
            string role = (SessionManager.CurrentRole ?? "").ToUpper();
            if (role == "ADMIN")
            {
                btnAdd.Enabled = true;
                btnUpdate.Enabled = true;
                btnDelete.Enabled = true;
            }
            else if (role == "WAREHOUSE")
            {
                btnAdd.Enabled = true;
                btnUpdate.Enabled = true;
                btnDelete.Enabled = false;
            }
            else
            {
                // Thu ngân: chỉ xem thông tin tồn kho
                btnAdd.Enabled = false;
                btnUpdate.Enabled = false;
                btnDelete.Enabled = false;
            }
        }

        private async void FormProductManagement_Load(object sender, EventArgs e)
        {
            SetPermission();
            await LoadCategoriesAsync();
            await LoadProductsAsync();
        }

        private async void FormProductManagement_Shown(object sender, EventArgs e)
        {
            if (_products.Count == 0)
            {
                await LoadProductsAsync();
            }
        }

        // ==========================================
        // TẢI DANH MỤC: GET /api/Categories
        // ==========================================
        private async Task LoadCategoriesAsync()
        {
            try
            {
                var response = await SessionManager.ApiClientService.Client.GetAsync("Categories");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
                    _categories.Clear();
                    if (data != null) _categories.AddRange(data);

                    // Gán vào dropdown chi tiết
                    cboCategory.DataSource = new BindingSource(_categories, null);
                    cboCategory.DisplayMember = "CategoryName";
                    cboCategory.ValueMember = "CategoryId";

                    // Gán vào dropdown lọc tìm kiếm
                    var filterList = new List<CategoryDto>
                    {
                        new CategoryDto { CategoryId = 0, CategoryName = "-- Tất cả nhóm hàng --" }
                    };
                    filterList.AddRange(_categories);

                    cboFilterCategory.DataSource = new BindingSource(filterList, null);
                    cboFilterCategory.DisplayMember = "CategoryName";
                    cboFilterCategory.ValueMember = "CategoryId";
                    cboFilterCategory.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nạp danh mục: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========================================
        // TẢI SẢN PHẨM: GET /api/Products
        // ==========================================
        private async Task LoadProductsAsync()
        {
            lblStatus.Text = "Đang tải danh sách sản phẩm...";
            try
            {
                string endpoint = "Products";
                var queryParams = new List<string>();

                string kw = txtSearch.Text.Trim();
                if (!string.IsNullOrWhiteSpace(kw) && kw != SearchPlaceholder && kw != "Tên / Mã vạch...")
                {
                    queryParams.Add($"keyword={Uri.EscapeDataString(kw)}");
                }

                if (cboFilterCategory.SelectedValue is int catId && catId > 0)
                {
                    queryParams.Add($"categoryId={catId}");
                }

                if (queryParams.Count > 0)
                {
                    endpoint += "?" + string.Join("&", queryParams);
                }

                var response = await SessionManager.ApiClientService.Client.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
                    _products.Clear();
                    if (data != null) _products.AddRange(data);

                    DisplayProducts(_products);
                    lblStatus.Text = $"Có {_products.Count} sản phẩm";
                }
                else
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(response, "Lấy danh sách sản phẩm");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối API sản phẩm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Lỗi kết nối API";
            }
        }

        private void DisplayProducts(List<ProductDto> list)
        {
            dgvProducts.Rows.Clear();
            foreach (var item in list)
            {
                int idx = dgvProducts.Rows.Add();
                var row = dgvProducts.Rows[idx];
                row.Cells[0].Value = item.ProductId;
                row.Cells[1].Value = item.Barcode;
                row.Cells[2].Value = item.ProductName;
                row.Cells[3].Value = item.Category != null ? item.Category.CategoryName : $"Nhóm #{item.CategoryId}";
                row.Cells[4].Value = item.Unit;
                row.Cells[5].Value = item.Price;
                row.Cells[6].Value = item.CostPrice;
                row.Cells[7].Value = item.StockQuantity;
                row.Cells[8].Value = item.IsActive ? "Đang bán" : "Ngừng bán";
            }

            if (list.Count > 0)
            {
                dgvProducts.ClearSelection();
                dgvProducts.Rows[0].Selected = true;
                ShowProduct(0);
            }
            else
            {
                ClearForm();
            }
        }

        private void DgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            ShowProduct(e.RowIndex);
        }

        private void ShowProduct(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= _products.Count) return;
            var p = _products[rowIndex];

            txtId.Text = p.ProductId.ToString();
            txtBarcode.Text = p.Barcode;
            txtProductName.Text = p.ProductName;
            txtUnit.Text = string.IsNullOrEmpty(p.Unit) ? "cái" : p.Unit;
            nudPrice.Value = Math.Min(nudPrice.Maximum, Math.Max(0, p.Price));
            nudCostPrice.Value = Math.Min(nudCostPrice.Maximum, Math.Max(0, p.CostPrice));
            nudStock.Value = Math.Min(nudStock.Maximum, Math.Max(0, p.StockQuantity));
            chkIsActive.Checked = p.IsActive;

            if (p.CategoryId > 0 && cboCategory.Items.Count > 0)
            {
                cboCategory.SelectedValue = p.CategoryId;
            }
        }

        private void ClearForm()
        {
            txtId.Clear();
            txtBarcode.Clear();
            txtProductName.Clear();
            txtUnit.Text = "cái";
            nudPrice.Value = 0;
            nudCostPrice.Value = 0;
            nudStock.Value = 0;
            chkIsActive.Checked = true;
            if (cboCategory.Items.Count > 0) cboCategory.SelectedIndex = 0;
            txtProductName.Focus();
        }

        private async void BtnSearch_Click(object sender, EventArgs e)
        {
            await LoadProductsAsync();
        }

        private async void BtnReload_Click(object sender, EventArgs e)
        {
            txtSearch.Text = SearchPlaceholder;
            txtSearch.ForeColor = UITheme.TextMuted;
            cboFilterCategory.SelectedIndex = 0;
            await LoadProductsAsync();
        }

        // ==========================================
        // THÊM MỚI: POST /api/Products
        // ==========================================
        private async Task CreateProductAsync()
        {
            if (string.IsNullOrWhiteSpace(txtProductName.Text) || string.IsNullOrWhiteSpace(txtBarcode.Text))
            {
                MessageBox.Show("Vui lòng nhập Tên sản phẩm và Mã vạch!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int catId = 1;
            if (cboCategory.SelectedValue is int cid && cid > 0) catId = cid;

            var newProduct = new ProductDto
            {
                Barcode = txtBarcode.Text.Trim(),
                ProductName = txtProductName.Text.Trim(),
                Unit = string.IsNullOrWhiteSpace(txtUnit.Text) ? "cái" : txtUnit.Text.Trim(),
                Price = nudPrice.Value,
                CostPrice = nudCostPrice.Value,
                StockQuantity = (int)nudStock.Value,
                CategoryId = catId,
                IsActive = chkIsActive.Checked
            };

            try
            {
                var response = await SessionManager.ApiClientService.Client.PostAsJsonAsync("Products", newProduct);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Thêm sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    await LoadProductsAsync();
                }
                else
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(response, "Thêm sản phẩm");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm sản phẩm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========================================
        // SỬA: PUT /api/Products/{id}
        // ==========================================
        private async Task UpdateProductAsync()
        {
            if (!int.TryParse(txtId.Text, out int id))
            {
                MessageBox.Show("Vui lòng chọn sản phẩm cần sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtProductName.Text) || string.IsNullOrWhiteSpace(txtBarcode.Text))
            {
                MessageBox.Show("Vui lòng nhập Tên sản phẩm và Mã vạch!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int catId = 1;
            if (cboCategory.SelectedValue is int cid && cid > 0) catId = cid;

            var updateProduct = new ProductDto
            {
                ProductId = id,
                Barcode = txtBarcode.Text.Trim(),
                ProductName = txtProductName.Text.Trim(),
                Unit = string.IsNullOrWhiteSpace(txtUnit.Text) ? "cái" : txtUnit.Text.Trim(),
                Price = nudPrice.Value,
                CostPrice = nudCostPrice.Value,
                StockQuantity = (int)nudStock.Value,
                CategoryId = catId,
                IsActive = chkIsActive.Checked
            };

            try
            {
                var response = await SessionManager.ApiClientService.Client.PutAsJsonAsync($"Products/{id}", updateProduct);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Cập nhật sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadProductsAsync();
                }
                else
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(response, "Cập nhật sản phẩm");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật sản phẩm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========================================
        // XÓA: DELETE /api/Products/{id}
        // ==========================================
        private async Task DeleteProductAsync()
        {
            if (!int.TryParse(txtId.Text, out int id))
            {
                MessageBox.Show("Vui lòng chọn sản phẩm cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa/ngừng kinh doanh sản phẩm '{txtProductName.Text}'?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                var response = await SessionManager.ApiClientService.Client.DeleteAsync($"Products/{id}");
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Đã xóa hoặc chuyển trạng thái ngừng kinh doanh sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    await LoadProductsAsync();
                }
                else
                {
                    await SessionManager.ApiClientService.ShowApiErrorAsync(response, "Xóa sản phẩm");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa sản phẩm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}