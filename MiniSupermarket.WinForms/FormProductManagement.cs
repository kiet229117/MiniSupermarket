using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public partial class FormProductManagement : Form
    {
        private const string SearchPlaceholder = "Tên hoặc Mã vạch...";
        private readonly List<ProductDto> _products = new List<ProductDto>();
        private readonly List<CategoryDto> _categories = new List<CategoryDto>();
        private readonly List<BrandDto> _brands = new List<BrandDto>();
        private readonly List<SupplierDto> _suppliers = new List<SupplierDto>();

        // Controls giao diện
        private Panel panelTop;
                private Label lblStatus;
        private Label lblSearch;
        private TextBox txtSearch;
        private Label lblFilterCategory;
        private ComboBox cboFilterCategory;
        private Button btnSearch;
        private Button btnReload;
        private Button btnFilterLowStock;
        private Button btnExportExcel;
        private bool _isFilterLowStock = false;

        // Phân trang danh sách sản phẩm
        private Panel panelPagination;
        private Button btnFirstPage;
        private Button btnPrevPage;
        private Label lblPageInfo;
        private Button btnNextPage;
        private Button btnLastPage;
        private Label lblPageSize;
        private ComboBox cboPageSize;
        private Label lblTotalRecords;

        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalRecords = 0;
        private int _totalPages = 1;
        private readonly List<ProductDto> _filteredProducts = new List<ProductDto>();

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

        private Label lblBrand;
        private ComboBox cboBrand;

        private Label lblSupplier;
        private ComboBox cboSupplier;

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
            // Shown event removed to prevent duplicate concurrent LoadProductsAsync calls
        }

        private void BuildUI()
        {
            

            // 1. Panel Top
            panelTop = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.White };

            lblStatus = new Label
            {
                Text = "Sẵn sàng",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.TextMuted,
                Location = new Point(18, 17),
                AutoSize = true
            };
            panelTop.Controls.Add(lblStatus);

            FlowLayoutPanel panelSearchFilter = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 10, 12, 0)
            };

            lblSearch = new Label { Text = "Tìm kiếm:", Font = UITheme.FontBodyBold, AutoSize = true, Margin = new Padding(0, 6, 4, 0) };
            panelSearchFilter.Controls.Add(lblSearch);

            txtSearch = new TextBox { Font = UITheme.FontBody, Size = new Size(130, 26), Margin = new Padding(0, 2, 8, 0) };
            UITheme.SetupSearchPlaceholder(txtSearch, SearchPlaceholder);
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnSearch_Click(s, e); };
            panelSearchFilter.Controls.Add(txtSearch);

            lblFilterCategory = new Label { Text = "Nhóm hàng:", Font = UITheme.FontBodyBold, AutoSize = true, Margin = new Padding(0, 6, 4, 0) };
            panelSearchFilter.Controls.Add(lblFilterCategory);

            cboFilterCategory = new ComboBox { Font = UITheme.FontBody, Size = new Size(135, 26), DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 2, 8, 0) };
            cboFilterCategory.SelectedIndexChanged += async (s, e) =>
            {
                if (cboFilterCategory.Focused) await LoadProductsAsync();
            };
            panelSearchFilter.Controls.Add(cboFilterCategory);

            btnSearch = new Button { Text = "Tìm", Size = new Size(60, 28), Margin = new Padding(0, 0, 6, 0) };
            UITheme.ApplyButtonTheme(btnSearch, UITheme.ButtonStyle.Primary);
            btnSearch.Click += BtnSearch_Click;
            panelSearchFilter.Controls.Add(btnSearch);

            btnReload = new Button { Text = "Tải lại", Size = new Size(65, 28), Margin = new Padding(0, 0, 6, 0) };
            UITheme.ApplyButtonTheme(btnReload, UITheme.ButtonStyle.Secondary);
            btnReload.Click += BtnReload_Click;
            panelSearchFilter.Controls.Add(btnReload);

            btnFilterLowStock = new Button { Text = "⚠️ Sắp hết", Size = new Size(82, 28), Margin = new Padding(0, 0, 6, 0) };
            UITheme.ApplyButtonTheme(btnFilterLowStock, UITheme.ButtonStyle.Secondary);
            btnFilterLowStock.Click += BtnFilterLowStock_Click;
            panelSearchFilter.Controls.Add(btnFilterLowStock);

            btnExportExcel = new Button { Text = "📊 Xuất Excel", Size = new Size(95, 28), Margin = new Padding(0, 0, 0, 0) };
            UITheme.ApplyButtonTheme(btnExportExcel, UITheme.ButtonStyle.Primary);
            btnExportExcel.Click += BtnExportExcel_Click;
            panelSearchFilter.Controls.Add(btnExportExcel);

            panelTop.Controls.Add(panelSearchFilter);

            // 2. DataGridView hiển thị danh sách sản phẩm
            dgvProducts = new DataGridView { Dock = DockStyle.Fill };
            EnsureColumns();
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
                Height = 440 // Mở rộng chiều cao GroupBox để chứa đủ các điều khiển mới
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

            lblBrand = new Label { Text = "Thương hiệu (*):", Font = UITheme.FontBodyBold, Location = new Point(15, 175), AutoSize = true };
            cboBrand = new ComboBox { Font = UITheme.FontBody, Location = new Point(15, 195), Size = new Size(300, 25), DropDownStyle = ComboBoxStyle.DropDownList };

            lblSupplier = new Label { Text = "Nhà cung cấp (*):", Font = UITheme.FontBodyBold, Location = new Point(15, 225), AutoSize = true };
            cboSupplier = new ComboBox { Font = UITheme.FontBody, Location = new Point(15, 245), Size = new Size(300, 25), DropDownStyle = ComboBoxStyle.DropDownList };

            lblUnit = new Label { Text = "Đơn vị tính:", Font = UITheme.FontBodyBold, Location = new Point(15, 275), AutoSize = true };
            txtUnit = new TextBox { Font = UITheme.FontBody, Location = new Point(15, 295), Size = new Size(130, 25), Text = "cái" };

            lblStock = new Label { Text = "Số lượng tồn:", Font = UITheme.FontBodyBold, Location = new Point(155, 275), AutoSize = true };
            nudStock = new NumericUpDown { Font = UITheme.FontBody, Location = new Point(155, 295), Size = new Size(160, 25), Maximum = 1000000 };

            lblPrice = new Label { Text = "Giá bán (VNĐ):", Font = UITheme.FontBodyBold, Location = new Point(15, 325), AutoSize = true };
            nudPrice = new NumericUpDown { Font = UITheme.FontBody, Location = new Point(15, 345), Size = new Size(130, 25), Maximum = 1000000000, Increment = 1000, ThousandsSeparator = true };

            lblCostPrice = new Label { Text = "Giá vốn (VNĐ):", Font = UITheme.FontBodyBold, Location = new Point(155, 325), AutoSize = true };
            nudCostPrice = new NumericUpDown { Font = UITheme.FontBody, Location = new Point(155, 345), Size = new Size(160, 25), Maximum = 1000000000, Increment = 1000, ThousandsSeparator = true };

            chkIsActive = new CheckBox { Text = "Đang kinh doanh", Font = UITheme.FontBodyBold, Location = new Point(15, 385), AutoSize = true, Checked = true };

            grbDetails.Controls.AddRange(new Control[] {
                lblId, txtId,
                lblBarcode, txtBarcode,
                lblProductName, txtProductName,
                lblCategory, cboCategory,
                lblBrand, cboBrand,
                lblSupplier, cboSupplier,
                lblUnit, txtUnit,
                lblStock, nudStock,
                lblPrice, nudPrice,
                lblCostPrice, nudCostPrice,
                chkIsActive
            });

            // 4 Nút thao tác CRUD
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

            btnAdd = new Button { Text = "➕ Thêm SP", Size = new Size(btnWidth, btnHeight), Margin = new Padding(4) };
            UITheme.ApplyButtonTheme(btnAdd, UITheme.ButtonStyle.Success);
            btnAdd.Click += async (s, e) => await CreateProductAsync();

            btnUpdate = new Button { Text = "✏ Cập Nhật", Size = new Size(btnWidth, btnHeight), Margin = new Padding(4) };
            UITheme.ApplyButtonTheme(btnUpdate, UITheme.ButtonStyle.Warning);
            btnUpdate.Click += async (s, e) => await UpdateProductAsync();

            btnDelete = new Button { Text = "🗑 Xóa / Ngừng", Size = new Size(btnWidth, btnHeight), Margin = new Padding(4) };
            UITheme.ApplyButtonTheme(btnDelete, UITheme.ButtonStyle.Danger);
            btnDelete.Click += async (s, e) => await DeleteProductAsync();

            btnClear = new Button { Text = "🔄 Làm Mới Ô", Size = new Size(btnWidth, btnHeight), Margin = new Padding(4) };
            UITheme.ApplyButtonTheme(btnClear, UITheme.ButtonStyle.Secondary);
            btnClear.Click += (s, e) => ClearForm();

            panelButtons.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnClear });
            panelRight.Controls.Add(panelButtons);
            panelButtons.BringToFront();

            // 4. Panel Thanh điều hướng Phân trang (Tiêu chí 25)
            panelPagination = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 42,
                BackColor = Color.White,
                Padding = new Padding(12, 6, 12, 6)
            };

            lblPageSize = new Label
            {
                Text = "Hiển thị:",
                Font = UITheme.FontBody,
                ForeColor = UITheme.TextPrimary,
                Location = new Point(14, 11),
                AutoSize = true
            };
            panelPagination.Controls.Add(lblPageSize);

            cboPageSize = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UITheme.FontBody,
                Size = new Size(115, 26),
                Location = new Point(72, 7)
            };
            cboPageSize.Items.AddRange(new object[] { "10 sp / trang", "15 sp / trang", "25 sp / trang", "50 sp / trang", "Tất cả" });
            cboPageSize.SelectedIndex = 0;
            cboPageSize.SelectedIndexChanged += (s, e) =>
            {
                switch (cboPageSize.SelectedIndex)
                {
                    case 0: _pageSize = 10; break;
                    case 1: _pageSize = 15; break;
                    case 2: _pageSize = 25; break;
                    case 3: _pageSize = 50; break;
                    case 4: _pageSize = 0; break; // Tất cả
                }
                _currentPage = 1;
                RenderPage();
            };
            panelPagination.Controls.Add(cboPageSize);

            lblTotalRecords = new Label
            {
                Text = "Tổng: 0 sản phẩm",
                Font = UITheme.FontBodyBold,
                ForeColor = UITheme.TextMuted,
                Location = new Point(200, 11),
                AutoSize = true
            };
            panelPagination.Controls.Add(lblTotalRecords);

            FlowLayoutPanel panelPageNav = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 4, 10, 0)
            };

            btnFirstPage = new Button { Text = "⏮", Size = new Size(36, 28), Margin = new Padding(2), Cursor = Cursors.Hand };
            UITheme.ApplyButtonTheme(btnFirstPage, UITheme.ButtonStyle.Secondary);
            btnFirstPage.Click += (s, e) => { _currentPage = 1; RenderPage(); };

            btnPrevPage = new Button { Text = "◀", Size = new Size(36, 28), Margin = new Padding(2), Cursor = Cursors.Hand };
            UITheme.ApplyButtonTheme(btnPrevPage, UITheme.ButtonStyle.Secondary);
            btnPrevPage.Click += (s, e) => { if (_currentPage > 1) { _currentPage--; RenderPage(); } };

            lblPageInfo = new Label
            {
                Text = "Trang 1 / 1",
                Font = UITheme.FontBodyBold,
                ForeColor = UITheme.TextPrimary,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(110, 26),
                Margin = new Padding(2, 6, 2, 0)
            };

            btnNextPage = new Button { Text = "▶", Size = new Size(36, 28), Margin = new Padding(2), Cursor = Cursors.Hand };
            UITheme.ApplyButtonTheme(btnNextPage, UITheme.ButtonStyle.Secondary);
            btnNextPage.Click += (s, e) => { if (_currentPage < _totalPages) { _currentPage++; RenderPage(); } };

            btnLastPage = new Button { Text = "⏭", Size = new Size(36, 28), Margin = new Padding(2), Cursor = Cursors.Hand };
            UITheme.ApplyButtonTheme(btnLastPage, UITheme.ButtonStyle.Secondary);
            btnLastPage.Click += (s, e) => { _currentPage = _totalPages; RenderPage(); };

            panelPageNav.Controls.AddRange(new Control[] { btnFirstPage, btnPrevPage, lblPageInfo, btnNextPage, btnLastPage });
            panelPagination.Controls.Add(panelPageNav);

            this.Controls.Add(dgvProducts);
            this.Controls.Add(panelPagination);
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
                btnAdd.Enabled = false;
                btnUpdate.Enabled = false;
                btnDelete.Enabled = false;
            }
        }

        private async void FormProductManagement_Load(object sender, EventArgs e)
        {
            SetPermission();
            await Task.WhenAll(
                LoadCategoriesAsync(),
                LoadBrandsAsync(),
                LoadSuppliersAsync(),
                LoadProductsAsync()
            );
        }



        private async Task LoadCategoriesAsync(bool forceReload = false)
        {
            try
            {
                List<CategoryDto> data = null;
                if (!forceReload && SessionManager.DataCache.Categories != null)
                {
                    data = SessionManager.DataCache.Categories;
                }
                else
                {
                    var response = await SessionManager.ApiClientService.Client.GetAsync("Categories");
                    if (response.IsSuccessStatusCode)
                    {
                        data = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
                        SessionManager.DataCache.Categories = data;
                    }
                }

                if (data != null)
                {
                    _categories.Clear();
                    _categories.AddRange(data);

                    cboCategory.DataSource = new BindingSource(_categories, null);
                    cboCategory.DisplayMember = "CategoryName";
                    cboCategory.ValueMember = "CategoryId";

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

        private async Task LoadBrandsAsync(bool forceReload = false)
        {
            try
            {
                List<BrandDto> data = null;
                if (!forceReload && SessionManager.DataCache.Brands != null)
                {
                    data = SessionManager.DataCache.Brands;
                }
                else
                {
                    var response = await SessionManager.ApiClientService.Client.GetAsync("Brands");
                    if (response.IsSuccessStatusCode)
                    {
                        data = await response.Content.ReadFromJsonAsync<List<BrandDto>>();
                        SessionManager.DataCache.Brands = data;
                    }
                }

                if (data != null)
                {
                    _brands.Clear();
                    _brands.AddRange(data);

                    cboBrand.DataSource = new BindingSource(_brands, null);
                    cboBrand.DisplayMember = "BrandName";
                    cboBrand.ValueMember = "BrandId";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nạp thương hiệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadSuppliersAsync(bool forceReload = false)
        {
            try
            {
                List<SupplierDto> data = null;
                if (!forceReload && SessionManager.DataCache.Suppliers != null)
                {
                    data = SessionManager.DataCache.Suppliers;
                }
                else
                {
                    var response = await SessionManager.ApiClientService.Client.GetAsync("Suppliers");
                    if (response.IsSuccessStatusCode)
                    {
                        data = await response.Content.ReadFromJsonAsync<List<SupplierDto>>();
                        SessionManager.DataCache.Suppliers = data;
                    }
                }

                if (data != null)
                {
                    _suppliers.Clear();
                    _suppliers.AddRange(data);

                    cboSupplier.DataSource = new BindingSource(_suppliers, null);
                    cboSupplier.DisplayMember = "SupplierName";
                    cboSupplier.ValueMember = "SupplierId";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nạp nhà cung cấp: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool _isLoading = false;

        private void EnsureColumns()
        {
            if (dgvProducts == null || dgvProducts.IsDisposed) return;
            if (dgvProducts.Columns.Count > 0) return;

            dgvProducts.Columns.Add("ProductId", "Mã SP");
            dgvProducts.Columns.Add("Barcode", "Mã Vạch");
            dgvProducts.Columns.Add("ProductName", "Tên Sản Phẩm");
            dgvProducts.Columns.Add("CategoryName", "Nhóm Hàng");
            dgvProducts.Columns.Add("BrandName", "Thương Hiệu");
            dgvProducts.Columns.Add("SupplierName", "Nhà cung cấp");
            dgvProducts.Columns.Add("Unit", "ĐVT");
            dgvProducts.Columns.Add("Price", "Giá Bán");
            dgvProducts.Columns.Add("CostPrice", "Giá Nhập");
            dgvProducts.Columns.Add("StockQuantity", "Tồn Kho");
            dgvProducts.Columns.Add("IsActive", "Kinh Doanh");

            dgvProducts.Columns["ProductId"].Width = 65;
            dgvProducts.Columns["Barcode"].Width = 120;
            dgvProducts.Columns["ProductName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvProducts.Columns["CategoryName"].Width = 130;
            dgvProducts.Columns["BrandName"].Width = 130;
            dgvProducts.Columns["SupplierName"].Width = 130;
            dgvProducts.Columns["Unit"].Width = 60;
            dgvProducts.Columns["Price"].Width = 90;
            dgvProducts.Columns["CostPrice"].Width = 90;
            dgvProducts.Columns["StockQuantity"].Width = 75;
            dgvProducts.Columns["IsActive"].Width = 85;

            dgvProducts.Columns["Price"].DefaultCellStyle.Format = "N0";
            dgvProducts.Columns["CostPrice"].DefaultCellStyle.Format = "N0";
            dgvProducts.Columns["StockQuantity"].DefaultCellStyle.Format = "N0";
        }

        private async Task LoadProductsAsync()
        {
            if (_isLoading || this.IsDisposed || this.Disposing) return;
            _isLoading = true;

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
                if (this.IsDisposed || this.Disposing) return;

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
                    if (this.IsDisposed || this.Disposing) return;

                    _products.Clear();
                    if (data != null) _products.AddRange(data);

                    _filteredProducts.Clear();
                    _filteredProducts.AddRange(_products);
                    _currentPage = 1;
                    RenderPage();
                }
                else
                {
                    if (this.IsDisposed || this.Disposing) return;
                    await SessionManager.ApiClientService.ShowApiErrorAsync(response, "Lấy danh sách sản phẩm");
                }
            }
            catch (Exception ex)
            {
                if (this.IsDisposed || this.Disposing) return;
                MessageBox.Show("Lỗi kết nối API sản phẩm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Lỗi kết nối API";
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void DisplayProducts(List<ProductDto> list)
        {
            if (this.IsDisposed || this.Disposing || dgvProducts == null || dgvProducts.IsDisposed) return;
            EnsureColumns();
            if (dgvProducts.Columns.Count == 0) return;

            dgvProducts.SuspendLayout();
            try
            {
                dgvProducts.Rows.Clear();
                int lowStockCount = 0;

                foreach (var item in list)
                {
                    int idx = dgvProducts.Rows.Add();
                    var row = dgvProducts.Rows[idx];
                    row.Cells[0].Value = item.ProductId;
                    row.Cells[1].Value = item.Barcode;
                    row.Cells[2].Value = item.ProductName;
                    row.Cells[3].Value = item.CategoryName;
                    row.Cells[4].Value = item.BrandName;
                    row.Cells[5].Value = item.SupplierName;
                    row.Cells[6].Value = item.Unit;
                    row.Cells[7].Value = item.Price;
                    row.Cells[8].Value = item.CostPrice;

                    // Tiêu chí 43: Huy hiệu & Cảnh báo hàng sắp hết kho (tồn kho < 10)
                    if (item.StockQuantity < 10)
                    {
                        lowStockCount++;
                        row.Cells[9].Value = $"⚠️ {item.StockQuantity}";
                        if (item.StockQuantity <= 5)
                        {
                            row.DefaultCellStyle.BackColor = Color.FromArgb(254, 242, 242);
                            row.DefaultCellStyle.ForeColor = Color.FromArgb(185, 28, 28);
                        }
                        else
                        {
                            row.DefaultCellStyle.BackColor = Color.FromArgb(254, 252, 232);
                            row.DefaultCellStyle.ForeColor = Color.FromArgb(180, 83, 9);
                        }
                    }
                    else
                    {
                        row.Cells[9].Value = item.StockQuantity;
                    }

                    row.Cells[10].Value = item.IsActive ? "Đang bán" : "Ngừng bán";
                }

                if (lowStockCount > 0)
                {
                    lblStatus.Text = $"Tổng {list.Count} SP | ⚠️ CẢNH BÁO: Có {lowStockCount} mặt hàng sắp hết kho (tồn kho < 10)!";
                    lblStatus.ForeColor = Color.FromArgb(220, 38, 38);
                }
                else
                {
                    lblStatus.Text = $"Tổng {list.Count} sản phẩm";
                    lblStatus.ForeColor = UITheme.TextMuted;
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
            finally
            {
                dgvProducts.ResumeLayout();
            }
        }

        // =======================================================================
        // ĐIỀU KHIỂN PHÂN TRANG (Tiêu chí 25: Phân trang danh sách sản phẩm)
        // =======================================================================
        private void RenderPage()
        {
            _totalRecords = _filteredProducts.Count;
            if (_pageSize > 0)
            {
                _totalPages = Math.Max(1, (int)Math.Ceiling((double)_totalRecords / _pageSize));
                if (_currentPage > _totalPages) _currentPage = _totalPages;
                if (_currentPage < 1) _currentPage = 1;

                var pagedItems = _filteredProducts.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();
                DisplayProducts(pagedItems);
            }
            else
            {
                _totalPages = 1;
                _currentPage = 1;
                DisplayProducts(_filteredProducts);
            }

            lblPageInfo.Text = $"Trang {_currentPage} / {_totalPages}";
            lblTotalRecords.Text = $"Tổng: {_totalRecords} sản phẩm";
            btnFirstPage.Enabled = _currentPage > 1;
            btnPrevPage.Enabled = _currentPage > 1;
            btnNextPage.Enabled = _currentPage < _totalPages;
            btnLastPage.Enabled = _currentPage < _totalPages;
        }

        // Tiêu chí 43: Lọc nhanh hàng sắp hết kho (< 10)
        private void BtnFilterLowStock_Click(object sender, EventArgs e)
        {
            _isFilterLowStock = !_isFilterLowStock;
            _filteredProducts.Clear();
            if (_isFilterLowStock)
            {
                btnFilterLowStock.Text = "✅ Tất cả";
                _filteredProducts.AddRange(_products.Where(p => p.StockQuantity < 10));
            }
            else
            {
                btnFilterLowStock.Text = "⚠️ Sắp hết";
                _filteredProducts.AddRange(_products);
            }
            _currentPage = 1;
            RenderPage();
        }

        // Tiêu chí 48: Xuất Báo cáo Danh mục Sản phẩm ra Excel (CSV định dạng UTF-8)
        private void BtnExportExcel_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Tệp Excel CSV (*.csv)|*.csv";
                sfd.FileName = $"Danh_Sach_San_Pham_{DateTime.Now:yyyyMMdd_HHmm}.csv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var sw = new System.IO.StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                        {
                            sw.WriteLine("Mã SP,Mã Vạch,Tên Sản Phẩm,Nhóm Hàng,Thương Hiệu,Nhà Cung Cấp,ĐVT,Giá Bán,Giá Nhập,Tồn Kho,Trạng Thái");
                            foreach (DataGridViewRow row in dgvProducts.Rows)
                            {
                                if (row.IsNewRow) continue;
                                var vals = new List<string>();
                                for (int c = 0; c <= 10; c++)
                                {
                                    string v = row.Cells[c].Value?.ToString() ?? "";
                                    v = "\"" + v.Replace("\"", "\"\"") + "\"";
                                    vals.Add(v);
                                }
                                sw.WriteLine(string.Join(",", vals));
                            }
                        }

                        if (MessageBox.Show("Xuất danh mục sản phẩm ra Excel thành công!\nBạn có muốn mở tệp ngay không?", "Xuất Excel", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
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

        private void DgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            ShowProduct(e.RowIndex);
        }

        private void ShowProduct(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvProducts.Rows.Count) return;
            var row = dgvProducts.Rows[rowIndex];
            if (row.Cells[0].Value == null) return;

            int prodId = Convert.ToInt32(row.Cells[0].Value);
            var p = _products.FirstOrDefault(x => x.ProductId == prodId);
            if (p == null) return;

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
            if (p.BrandId > 0 && cboBrand.Items.Count > 0)
            {
                cboBrand.SelectedValue = p.BrandId;
            }
            if (p.SupplierId != null && p.SupplierId > 0 && cboSupplier.Items.Count > 0)
            {
                cboSupplier.SelectedValue = p.SupplierId;
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
            if (cboBrand.Items.Count > 0) cboBrand.SelectedIndex = 0;
            if (cboSupplier.Items.Count > 0) cboSupplier.SelectedIndex = 0;
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
            _isFilterLowStock = false;
            btnFilterLowStock.Text = "⚠️ Sắp hết";
            await LoadProductsAsync();
        }

        private async Task CreateProductAsync()
        {
            if (string.IsNullOrWhiteSpace(txtProductName.Text) || string.IsNullOrWhiteSpace(txtBarcode.Text))
            {
                MessageBox.Show("Vui lòng nhập Tên sản phẩm và Mã vạch!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int catId = cboCategory.SelectedValue is int cid ? cid : 1;
            int brandId = cboBrand.SelectedValue is int bid ? bid : 1;
            int supplierId = cboSupplier.SelectedValue is int sid ? sid : 1;

            var newProduct = new ProductDto
            {
                Barcode = txtBarcode.Text.Trim(),
                ProductName = txtProductName.Text.Trim(),
                Unit = string.IsNullOrWhiteSpace(txtUnit.Text) ? "cái" : txtUnit.Text.Trim(),
                Price = nudPrice.Value,
                CostPrice = nudCostPrice.Value,
                StockQuantity = (int)nudStock.Value,
                IsActive = chkIsActive.Checked,
                CategoryId = catId,
                BrandId = brandId,
                SupplierId = supplierId
            };

            try
            {
                var response = await SessionManager.ApiClientService.Client.PostAsJsonAsync("Products", newProduct);
                if (response.IsSuccessStatusCode)
                {
                    SessionManager.DataCache.ActiveProducts = null;
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

            int catId = cboCategory.SelectedValue is int cid ? cid : 1;
            int brandId = cboBrand.SelectedValue is int bid ? bid : 1;
            int supplierId = cboSupplier.SelectedValue is int sid ? sid : 1;

            var updateProduct = new ProductDto
            {
                ProductId = id,
                Barcode = txtBarcode.Text.Trim(),
                ProductName = txtProductName.Text.Trim(),
                Unit = string.IsNullOrWhiteSpace(txtUnit.Text) ? "cái" : txtUnit.Text.Trim(),
                Price = nudPrice.Value,
                CostPrice = nudCostPrice.Value,
                StockQuantity = (int)nudStock.Value,
                IsActive = chkIsActive.Checked,
                CategoryId = catId,
                BrandId = brandId,
                SupplierId = supplierId
            };

            try
            {
                var response = await SessionManager.ApiClientService.Client.PutAsJsonAsync($"Products/{id}", updateProduct);
                if (response.IsSuccessStatusCode)
                {
                    SessionManager.DataCache.ActiveProducts = null;
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
                    SessionManager.DataCache.ActiveProducts = null;
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