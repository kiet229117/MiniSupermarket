using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    // =========================================================================
    // THANH ĐIỀU KHIỂN PHÂN TRANG TÁI SỬ DỤNG CHO TOÀN BỘ CÁC BẢNG (Tiêu chí 25)
    // Tự động chia trang, tính toán tổng số trang và hỗ trợ chọn số dòng/trang
    // =========================================================================
    public class PaginationControl<T> : Panel
    {
        private List<T> _sourceList = new List<T>();
        private Action<List<T>> _onPageChanged;
        private string _unitName = "dòng";

        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalPages = 1;

        // UI Controls
        private Label lblPageSize;
        private ComboBox cboPageSize;
        private Label lblTotalRecords;
        private Button btnFirstPage;
        private Button btnPrevPage;
        private Label lblPageInfo;
        private Button btnNextPage;
        private Button btnLastPage;

        public int CurrentPage => _currentPage;
        public int PageSize => _pageSize;
        public int TotalRecords => _sourceList.Count;

        public PaginationControl()
        {
            this.Dock = DockStyle.Bottom;
            this.Height = 42;
            this.BackColor = Color.White;
            this.Padding = new Padding(12, 6, 12, 6);
            BuildUI();
        }

        public void Bind(Action<List<T>> onPageChanged, string unitName = "dòng")
        {
            _onPageChanged = onPageChanged;
            _unitName = unitName;
        }

        public void SetDataSource(IEnumerable<T> source, int? targetPage = null)
        {
            _sourceList = source != null ? source.ToList() : new List<T>();
            if (targetPage.HasValue)
            {
                _currentPage = targetPage.Value;
            }
            else
            {
                _currentPage = 1;
            }
            Render();
        }

        private void BuildUI()
        {
            lblPageSize = new Label
            {
                Text = "Hiển thị:",
                Font = UITheme.FontBody,
                ForeColor = UITheme.TextPrimary,
                Location = new Point(14, 11),
                AutoSize = true
            };
            this.Controls.Add(lblPageSize);

            cboPageSize = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UITheme.FontBody,
                Size = new Size(115, 26),
                Location = new Point(72, 7)
            };
            cboPageSize.Items.AddRange(new object[] { "10 dòng / trang", "15 dòng / trang", "25 dòng / trang", "50 dòng / trang", "Tất cả" });
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
                Render();
            };
            this.Controls.Add(cboPageSize);

            lblTotalRecords = new Label
            {
                Text = "Tổng: 0",
                Font = UITheme.FontBodyBold,
                ForeColor = UITheme.TextMuted,
                Location = new Point(200, 11),
                AutoSize = true
            };
            this.Controls.Add(lblTotalRecords);

            FlowLayoutPanel panelNav = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 4, 10, 0)
            };

            btnFirstPage = new Button { Text = "⏮", Size = new Size(36, 28), Margin = new Padding(2), Cursor = Cursors.Hand };
            UITheme.ApplyButtonTheme(btnFirstPage, UITheme.ButtonStyle.Secondary);
            btnFirstPage.Click += (s, e) => { _currentPage = 1; Render(); };

            btnPrevPage = new Button { Text = "◀", Size = new Size(36, 28), Margin = new Padding(2), Cursor = Cursors.Hand };
            UITheme.ApplyButtonTheme(btnPrevPage, UITheme.ButtonStyle.Secondary);
            btnPrevPage.Click += (s, e) => { if (_currentPage > 1) { _currentPage--; Render(); } };

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
            btnNextPage.Click += (s, e) => { if (_currentPage < _totalPages) { _currentPage++; Render(); } };

            btnLastPage = new Button { Text = "⏭", Size = new Size(36, 28), Margin = new Padding(2), Cursor = Cursors.Hand };
            UITheme.ApplyButtonTheme(btnLastPage, UITheme.ButtonStyle.Secondary);
            btnLastPage.Click += (s, e) => { _currentPage = _totalPages; Render(); };

            panelNav.Controls.AddRange(new Control[] { btnFirstPage, btnPrevPage, lblPageInfo, btnNextPage, btnLastPage });
            this.Controls.Add(panelNav);
        }

        public void Render()
        {
            int total = _sourceList.Count;
            List<T> pageItems;
            if (_pageSize > 0)
            {
                _totalPages = Math.Max(1, (int)Math.Ceiling((double)total / _pageSize));
                if (_currentPage > _totalPages) _currentPage = _totalPages;
                if (_currentPage < 1) _currentPage = 1;

                pageItems = _sourceList.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();
            }
            else
            {
                _totalPages = 1;
                _currentPage = 1;
                pageItems = _sourceList;
            }

            lblPageInfo.Text = $"Trang {_currentPage} / {_totalPages}";
            lblTotalRecords.Text = $"Tổng: {total} {_unitName}";

            btnFirstPage.Enabled = _currentPage > 1;
            btnPrevPage.Enabled = _currentPage > 1;
            btnNextPage.Enabled = _currentPage < _totalPages;
            btnLastPage.Enabled = _currentPage < _totalPages;

            _onPageChanged?.Invoke(pageItems);
        }
    }
}
