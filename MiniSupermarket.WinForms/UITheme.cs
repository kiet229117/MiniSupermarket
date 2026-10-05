using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public static class UITheme
    {
        // ==========================================
        // BẢNG MÀU CHUẨN THỐNG NHẤT TOÀN HỆ THỐNG
        // ==========================================
        public static readonly Color FormBackground = Color.FromArgb(245, 247, 250);
        public static readonly Color CardBackground = Color.White;
        public static readonly Color SidebarDark = Color.FromArgb(24, 30, 48);
        public static readonly Color HeaderDark = Color.FromArgb(30, 41, 59);

        // Màu các nút hành động (Action Buttons)
        public static readonly Color PrimaryColor = Color.FromArgb(0, 123, 255);     // Tìm kiếm / Hành động chính
        public static readonly Color SuccessColor = Color.FromArgb(40, 167, 69);     // Thêm mới
        public static readonly Color WarningColor = Color.FromArgb(245, 158, 11);    // Cập nhật / Sửa
        public static readonly Color DangerColor = Color.FromArgb(220, 53, 69);      // Xóa / Hủy
        public static readonly Color SecondaryColor = Color.FromArgb(108, 117, 125); // Làm mới / Tải lại
        public static readonly Color InfoColor = Color.FromArgb(23, 162, 184);       // Chi tiết / POS

        // Màu văn bản & Đường viền
        public static readonly Color TextPrimary = Color.FromArgb(33, 37, 41);
        public static readonly Color TextMuted = Color.FromArgb(108, 117, 125);
        public static readonly Color BorderColor = Color.FromArgb(222, 226, 230);
        public static readonly Color SelectionRowColor = Color.FromArgb(220, 235, 252);

        // Font chữ chuẩn Segoe UI
        public static readonly Font FontTitle = new Font("Segoe UI", 12F, FontStyle.Bold);
        public static readonly Font FontSubtitle = new Font("Segoe UI", 10F, FontStyle.Bold);
        public static readonly Font FontBody = new Font("Segoe UI", 9.5F, FontStyle.Regular);
        public static readonly Font FontBodyBold = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        public static readonly Font FontSmall = new Font("Segoe UI", 8.5F, FontStyle.Regular);

        public enum ButtonStyle
        {
            Primary,
            Success,
            Warning,
            Danger,
            Secondary,
            Info
        }

        // ==========================================
        // ÁP DỤNG THEME CHO FORM
        // ==========================================
        public static void ApplyFormTheme(Form form)
        {
            form.BackColor = FormBackground;
            form.Font = FontBody;
        }

        // ==========================================
        // ÁP DỤNG THEME CHO DATAGRIDVIEW
        // ==========================================
        public static void ApplyGridTheme(DataGridView dgv)
        {
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.GridColor = Color.FromArgb(235, 238, 242);

            // Cấu hình Header cột
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = HeaderDark,
                ForeColor = Color.White,
                Font = FontBodyBold,
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };
            dgv.ColumnHeadersHeight = 38;

            // Cấu hình dòng dữ liệu
            dgv.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = TextPrimary,
                Font = FontBody,
                SelectionBackColor = SelectionRowColor,
                SelectionForeColor = TextPrimary,
                Padding = new Padding(6, 0, 0, 0)
            };
            dgv.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(248, 250, 252),
                ForeColor = TextPrimary,
                Font = FontBody,
                SelectionBackColor = SelectionRowColor,
                SelectionForeColor = TextPrimary,
                Padding = new Padding(6, 0, 0, 0)
            };

            dgv.RowTemplate.Height = 34;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
        }

        // ==========================================
        // ÁP DỤNG THEME CHO NÚT BẤM (BUTTON)
        // ==========================================
        public static void ApplyButtonTheme(Button btn, ButtonStyle style)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = FontBodyBold;
            btn.Cursor = Cursors.Hand;
            btn.Height = 32;

            switch (style)
            {
                case ButtonStyle.Primary:
                    btn.BackColor = PrimaryColor;
                    btn.ForeColor = Color.White;
                    break;
                case ButtonStyle.Success:
                    btn.BackColor = SuccessColor;
                    btn.ForeColor = Color.White;
                    break;
                case ButtonStyle.Warning:
                    btn.BackColor = WarningColor;
                    btn.ForeColor = Color.White;
                    break;
                case ButtonStyle.Danger:
                    btn.BackColor = DangerColor;
                    btn.ForeColor = Color.White;
                    break;
                case ButtonStyle.Secondary:
                    btn.BackColor = SecondaryColor;
                    btn.ForeColor = Color.White;
                    break;
                case ButtonStyle.Info:
                    btn.BackColor = InfoColor;
                    btn.ForeColor = Color.White;
                    break;
            }
        }

        // ==========================================
        // ÁP DỤNG CHO CARD PANEL / GROUPBOX
        // ==========================================
        public static void ApplyCardPanel(Panel panel)
        {
            panel.BackColor = CardBackground;
            panel.Padding = new Padding(12);
        }

        // ==========================================
        // HỖ TRỢ PLACEHOLDER CHO Ô TÌM KIẾM
        // ==========================================
        public static void SetupSearchPlaceholder(TextBox txt, string placeholder = "Nhập từ khóa tìm kiếm...")
        {
            txt.Text = placeholder;
            txt.ForeColor = TextMuted;

            txt.Enter += (s, e) =>
            {
                if (txt.Text == placeholder)
                {
                    txt.Text = "";
                    txt.ForeColor = TextPrimary;
                }
            };

            txt.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txt.Text))
                {
                    txt.Text = placeholder;
                    txt.ForeColor = TextMuted;
                }
            };
        }
    }
}

