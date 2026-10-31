using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public class FormInvoiceReceipt : Form
    {
        private readonly InvoiceReceiptModel _receipt;
        private PrintDocument _printDocument;
        private PrintPreviewDialog _previewDialog;

        public FormInvoiceReceipt(InvoiceReceiptModel receipt)
        {
            _receipt = receipt ?? throw new ArgumentNullException(nameof(receipt));
            InitializeUI();
            InitializePrinting();
        }

        private void InitializeUI()
        {
            this.Text = $"Hóa Đơn Bán Lẻ - {_receipt.OrderCode}";
            this.Size = new Size(560, 720);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = UITheme.FormBackground;
            this.KeyPreview = true;
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.Close(); };

            // 1. TOOLBAR PANEL TẠI TOP
            Panel pnlToolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 54,
                BackColor = Color.White,
                Padding = new Padding(12, 10, 12, 10)
            };

            Button btnPrint = new Button
            {
                Text = "🖨️ In Hóa Đơn",
                Size = new Size(130, 34),
                Location = new Point(12, 10),
                Cursor = Cursors.Hand
            };
            UITheme.ApplyButtonTheme(btnPrint, UITheme.ButtonStyle.Primary);
            btnPrint.Click += BtnPrint_Click;
            pnlToolbar.Controls.Add(btnPrint);

            Button btnPreview = new Button
            {
                Text = "👁️ Xem Trước Bản In",
                Size = new Size(145, 34),
                Location = new Point(148, 10),
                Cursor = Cursors.Hand
            };
            UITheme.ApplyButtonTheme(btnPreview, UITheme.ButtonStyle.Secondary);
            btnPreview.Click += BtnPreview_Click;
            pnlToolbar.Controls.Add(btnPreview);

            Button btnExport = new Button
            {
                Text = "💾 Lưu File Text",
                Size = new Size(125, 34),
                Location = new Point(299, 10),
                Cursor = Cursors.Hand
            };
            UITheme.ApplyButtonTheme(btnExport, UITheme.ButtonStyle.Secondary);
            btnExport.Click += BtnExport_Click;
            pnlToolbar.Controls.Add(btnExport);

            Button btnClose = new Button
            {
                Text = "Đóng",
                Size = new Size(80, 34),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(pnlToolbar.Width - 92, 10),
                Cursor = Cursors.Hand
            };
            UITheme.ApplyButtonTheme(btnClose, UITheme.ButtonStyle.Secondary);
            btnClose.Click += (s, e) => this.Close();
            pnlToolbar.Controls.Add(btnClose);

            this.Controls.Add(pnlToolbar);

            // 2. MAIN SCROLLABLE CONTAINER
            Panel pnlScroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20, 15, 20, 20),
                BackColor = UITheme.FormBackground
            };
            this.Controls.Add(pnlScroll);

            // 3. TỜ HÓA ĐƠN (WHITE RECEIPT CARD)
            Panel pnlReceipt = new Panel
            {
                Width = 480,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.White,
                Padding = new Padding(24, 20, 24, 25),
                Location = new Point(25, 10)
            };
            // Border viền nhẹ quanh hóa đơn
            pnlReceipt.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 225, 235), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, pnlReceipt.Width - 1, pnlReceipt.Height - 1);
                }
            };
            pnlScroll.Controls.Add(pnlReceipt);

            int currentY = 15;

            // Header siêu thị
            var lblStore = new Label
            {
                Text = "🛒 MINI SUPERMARKET",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = UITheme.PrimaryColor,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(430, 28),
                Location = new Point(24, currentY)
            };
            pnlReceipt.Controls.Add(lblStore);
            currentY += 30;

            var lblAddr = new Label
            {
                Text = _receipt.StoreAddress,
                Font = UITheme.FontSmall,
                ForeColor = UITheme.TextMuted,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(430, 20),
                Location = new Point(24, currentY)
            };
            pnlReceipt.Controls.Add(lblAddr);
            currentY += 20;

            var lblHotline = new Label
            {
                Text = _receipt.StoreHotline,
                Font = UITheme.FontSmall,
                ForeColor = UITheme.TextMuted,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(430, 18),
                Location = new Point(24, currentY)
            };
            pnlReceipt.Controls.Add(lblHotline);
            currentY += 26;

            var lblTitle = new Label
            {
                Text = "HÓA ĐƠN BÁN LẺ",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = UITheme.TextPrimary,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(430, 26),
                Location = new Point(24, currentY)
            };
            pnlReceipt.Controls.Add(lblTitle);
            currentY += 32;

            pnlReceipt.Controls.Add(CreateDashedLine(24, currentY, 430));
            currentY += 14;

            // Thông tin hóa đơn
            currentY = AddInfoRow(pnlReceipt, "Số hóa đơn:", _receipt.OrderCode, currentY, isBold: true);
            currentY = AddInfoRow(pnlReceipt, "Thời gian:", _receipt.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss"), currentY);
            currentY = AddInfoRow(pnlReceipt, "Thu ngân:", _receipt.CashierName, currentY);
            currentY = AddInfoRow(pnlReceipt, "Khách hàng:", string.IsNullOrWhiteSpace(_receipt.CustomerPhone) ? _receipt.CustomerName : $"{_receipt.CustomerName} ({_receipt.CustomerPhone})", currentY);
            currentY = AddInfoRow(pnlReceipt, "Hình thức thanh toán:", _receipt.PaymentMethod, currentY);

            currentY += 6;
            pnlReceipt.Controls.Add(CreateDashedLine(24, currentY, 430));
            currentY += 14;

            // Header bảng mặt hàng
            var lblHCol1 = new Label { Text = "Mặt hàng", Font = UITheme.FontBodyBold, ForeColor = UITheme.TextPrimary, Location = new Point(24, currentY), Size = new Size(200, 20) };
            var lblHCol2 = new Label { Text = "SL", Font = UITheme.FontBodyBold, ForeColor = UITheme.TextPrimary, Location = new Point(225, currentY), Size = new Size(45, 20), TextAlign = ContentAlignment.MiddleRight };
            var lblHCol3 = new Label { Text = "Đơn giá", Font = UITheme.FontBodyBold, ForeColor = UITheme.TextPrimary, Location = new Point(275, currentY), Size = new Size(75, 20), TextAlign = ContentAlignment.MiddleRight };
            var lblHCol4 = new Label { Text = "Thành tiền", Font = UITheme.FontBodyBold, ForeColor = UITheme.TextPrimary, Location = new Point(355, currentY), Size = new Size(100, 20), TextAlign = ContentAlignment.MiddleRight };
            pnlReceipt.Controls.AddRange(new Control[] { lblHCol1, lblHCol2, lblHCol3, lblHCol4 });
            currentY += 24;

            pnlReceipt.Controls.Add(CreateDashedLine(24, currentY, 430));
            currentY += 10;

            // Danh sách mặt hàng
            foreach (var item in _receipt.Items)
            {
                var lblItemName = new Label
                {
                    Text = item.ProductName,
                    Font = UITheme.FontBody,
                    ForeColor = UITheme.TextPrimary,
                    Location = new Point(24, currentY),
                    Size = new Size(200, 20),
                    AutoEllipsis = true
                };

                var lblItemQty = new Label
                {
                    Text = item.Quantity.ToString("N0"),
                    Font = UITheme.FontBody,
                    ForeColor = UITheme.TextPrimary,
                    Location = new Point(225, currentY),
                    Size = new Size(45, 20),
                    TextAlign = ContentAlignment.MiddleRight
                };

                var lblItemPrice = new Label
                {
                    Text = $"{item.UnitPrice:N0}",
                    Font = UITheme.FontBody,
                    ForeColor = UITheme.TextPrimary,
                    Location = new Point(275, currentY),
                    Size = new Size(75, 20),
                    TextAlign = ContentAlignment.MiddleRight
                };

                var lblItemTotal = new Label
                {
                    Text = $"{item.LineTotal:N0}",
                    Font = UITheme.FontBodyBold,
                    ForeColor = UITheme.TextPrimary,
                    Location = new Point(355, currentY),
                    Size = new Size(100, 20),
                    TextAlign = ContentAlignment.MiddleRight
                };

                pnlReceipt.Controls.AddRange(new Control[] { lblItemName, lblItemQty, lblItemPrice, lblItemTotal });
                currentY += 22;
            }

            currentY += 4;
            pnlReceipt.Controls.Add(CreateDashedLine(24, currentY, 430));
            currentY += 12;

            // Tổng kết thanh toán
            currentY = AddSummaryRow(pnlReceipt, "Tổng tiền hàng:", $"{_receipt.Subtotal:N0} đ", currentY, false);
            if (_receipt.Discount > 0)
            {
                currentY = AddSummaryRow(pnlReceipt, "Chiết khấu / Giảm giá:", $"-{_receipt.Discount:N0} đ", currentY, false, Color.FromArgb(220, 38, 38));
            }

            currentY += 4;
            currentY = AddSummaryRow(pnlReceipt, "TỔNG CỘNG:", $"{_receipt.Total:N0} đ", currentY, true, UITheme.PrimaryColor, new Font("Segoe UI", 12.5F, FontStyle.Bold));
            currentY += 6;

            if (_receipt.CashGiven > 0)
            {
                currentY = AddSummaryRow(pnlReceipt, "Tiền khách đưa:", $"{_receipt.CashGiven:N0} đ", currentY, false);
                currentY = AddSummaryRow(pnlReceipt, "Tiền thối lại:", $"{_receipt.ChangeDue:N0} đ", currentY, false, Color.FromArgb(16, 185, 129));
            }

            if (_receipt.EarnedPoints > 0)
            {
                currentY = AddSummaryRow(pnlReceipt, "Điểm tích lũy cộng thêm:", $"+{_receipt.EarnedPoints} điểm", currentY, false, Color.FromArgb(124, 58, 237));
            }

            currentY += 6;
            pnlReceipt.Controls.Add(CreateDashedLine(24, currentY, 430));
            currentY += 16;

            // Footer lời cảm ơn
            var lblThanks = new Label
            {
                Text = "XIN CẢM ƠN QUÝ KHÁCH VÀ HẸN GẶP LẠI!",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = UITheme.TextPrimary,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(430, 22),
                Location = new Point(24, currentY)
            };
            pnlReceipt.Controls.Add(lblThanks);
            currentY += 24;

            var lblNote = new Label
            {
                Text = "(Quý khách vui lòng kiểm tra lại hàng hóa và hóa đơn trước khi rời quầy)",
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = UITheme.TextMuted,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(430, 20),
                Location = new Point(24, currentY)
            };
            pnlReceipt.Controls.Add(lblNote);
            currentY += 28;

            var lblBarcode = new Label
            {
                Text = $"* {_receipt.OrderCode} *",
                Font = new Font("Consolas", 12F, FontStyle.Bold),
                ForeColor = Color.DarkSlateGray,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(430, 24),
                Location = new Point(24, currentY)
            };
            pnlReceipt.Controls.Add(lblBarcode);
            currentY += 35;

            pnlReceipt.Height = currentY;
        }

        private Label CreateDashedLine(int x, int y, int width)
        {
            return new Label
            {
                Text = "--------------------------------------------------------------------------------",
                Font = new Font("Consolas", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(200, 205, 215),
                Location = new Point(x, y),
                Size = new Size(width, 14),
                TextAlign = ContentAlignment.MiddleCenter
            };
        }

        private int AddInfoRow(Panel parent, string title, string value, int y, bool isBold = false)
        {
            var lblTitle = new Label
            {
                Text = title,
                Font = isBold ? UITheme.FontBodyBold : UITheme.FontSmall,
                ForeColor = UITheme.TextMuted,
                Location = new Point(24, y),
                Size = new Size(150, 18)
            };

            var lblValue = new Label
            {
                Text = value,
                Font = isBold ? UITheme.FontBodyBold : UITheme.FontBody,
                ForeColor = UITheme.TextPrimary,
                Location = new Point(175, y),
                Size = new Size(280, 18),
                TextAlign = ContentAlignment.MiddleRight
            };

            parent.Controls.AddRange(new Control[] { lblTitle, lblValue });
            return y + 20;
        }

        private int AddSummaryRow(Panel parent, string title, string value, int y, bool isBold, Color? color = null, Font font = null)
        {
            Font f = font ?? (isBold ? UITheme.FontBodyBold : UITheme.FontBody);
            Color c = color ?? UITheme.TextPrimary;

            var lblTitle = new Label
            {
                Text = title,
                Font = f,
                ForeColor = c,
                Location = new Point(24, y),
                Size = new Size(200, 24)
            };

            var lblValue = new Label
            {
                Text = value,
                Font = f,
                ForeColor = c,
                Location = new Point(225, y),
                Size = new Size(230, 24),
                TextAlign = ContentAlignment.MiddleRight
            };

            parent.Controls.AddRange(new Control[] { lblTitle, lblValue });
            return y + 24;
        }

        // ==========================================
        // KHỞI TẠO CẤU HÌNH IN (PRINT DOCUMENT)
        // ==========================================
        private void InitializePrinting()
        {
            _printDocument = new PrintDocument();
            _printDocument.DocumentName = $"HoaDon_{_receipt.OrderCode}";
            _printDocument.PrintPage += PrintDocument_PrintPage;

            _previewDialog = new PrintPreviewDialog
            {
                Document = _printDocument,
                Width = 800,
                Height = 600,
                StartPosition = FormStartPosition.CenterParent
            };
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            using (PrintDialog printDlg = new PrintDialog())
            {
                printDlg.Document = _printDocument;
                printDlg.UseEXDialog = true;
                if (printDlg.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        _printDocument.Print();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi gửi lệnh in đến máy in: " + ex.Message, "Lỗi in ấn", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnPreview_Click(object sender, EventArgs e)
        {
            try
            {
                _previewDialog.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi mở cửa sổ xem trước bản in: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            float leftMargin = 30;
            float topMargin = 30;
            float receiptWidth = 320; // Chuẩn giấy in nhiệt 80mm
            float rightMargin = leftMargin + receiptWidth;
            float y = topMargin;

            Font fontStore = new Font("Segoe UI", 13, FontStyle.Bold);
            Font fontHeader = new Font("Segoe UI", 11, FontStyle.Bold);
            Font fontRegular = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            Font fontBold = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            Font fontLargeBold = new Font("Segoe UI", 11F, FontStyle.Bold);
            Font fontItalic = new Font("Segoe UI", 7.5F, FontStyle.Italic);
            Brush brush = Brushes.Black;
            Pen penDashed = new Pen(Color.Gray, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };

            StringFormat centerFormat = new StringFormat { Alignment = StringAlignment.Center };
            StringFormat rightFormat = new StringFormat { Alignment = StringAlignment.Far };
            StringFormat leftFormat = new StringFormat { Alignment = StringAlignment.Near };

            // 1. Tên siêu thị
            g.DrawString(_receipt.StoreName, fontStore, brush, new RectangleF(leftMargin, y, receiptWidth, 24), centerFormat);
            y += 24;
            g.DrawString(_receipt.StoreAddress, fontRegular, brush, new RectangleF(leftMargin, y, receiptWidth, 18), centerFormat);
            y += 18;
            g.DrawString(_receipt.StoreHotline, fontRegular, brush, new RectangleF(leftMargin, y, receiptWidth, 18), centerFormat);
            y += 22;

            g.DrawString("HÓA ĐƠN BÁN LẺ", fontHeader, brush, new RectangleF(leftMargin, y, receiptWidth, 22), centerFormat);
            y += 24;

            g.DrawLine(penDashed, leftMargin, y, rightMargin, y);
            y += 8;

            // 2. Thông tin đơn
            void DrawRow(string label, string val, bool bold = false)
            {
                Font f = bold ? fontBold : fontRegular;
                g.DrawString(label, f, brush, leftMargin, y);
                g.DrawString(val, f, brush, new RectangleF(leftMargin, y, receiptWidth, 18), rightFormat);
                y += 18;
            }

            DrawRow("Mã HĐ:", _receipt.OrderCode, true);
            DrawRow("Ngày:", _receipt.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss"));
            DrawRow("Thu ngân:", _receipt.CashierName);
            DrawRow("Khách:", string.IsNullOrWhiteSpace(_receipt.CustomerPhone) ? _receipt.CustomerName : $"{_receipt.CustomerName} ({_receipt.CustomerPhone})");
            DrawRow("Hình thức:", _receipt.PaymentMethod);

            y += 4;
            g.DrawLine(penDashed, leftMargin, y, rightMargin, y);
            y += 8;

            // 3. Tiêu đề bảng hàng
            g.DrawString("Mặt hàng", fontBold, brush, leftMargin, y);
            g.DrawString("SL", fontBold, brush, new RectangleF(leftMargin + 140, y, 35, 18), rightFormat);
            g.DrawString("Đơn giá", fontBold, brush, new RectangleF(leftMargin + 175, y, 65, 18), rightFormat);
            g.DrawString("T.Tiền", fontBold, brush, new RectangleF(leftMargin + 240, y, 80, 18), rightFormat);
            y += 18;

            g.DrawLine(penDashed, leftMargin, y, rightMargin, y);
            y += 6;

            // 4. Chi tiết các món
            foreach (var item in _receipt.Items)
            {
                g.DrawString(item.ProductName, fontRegular, brush, new RectangleF(leftMargin, y, 140, 18), leftFormat);
                g.DrawString(item.Quantity.ToString("N0"), fontRegular, brush, new RectangleF(leftMargin + 140, y, 35, 18), rightFormat);
                g.DrawString($"{item.UnitPrice:N0}", fontRegular, brush, new RectangleF(leftMargin + 175, y, 65, 18), rightFormat);
                g.DrawString($"{item.LineTotal:N0}", fontBold, brush, new RectangleF(leftMargin + 240, y, 80, 18), rightFormat);
                y += 18;
            }

            y += 4;
            g.DrawLine(penDashed, leftMargin, y, rightMargin, y);
            y += 8;

            // 5. Tổng kết
            DrawRow("Tổng tiền hàng:", $"{_receipt.Subtotal:N0} đ");
            if (_receipt.Discount > 0)
            {
                DrawRow("Giảm giá:", $"-{_receipt.Discount:N0} đ");
            }

            y += 2;
            g.DrawString("TỔNG THANH TOÁN:", fontLargeBold, brush, leftMargin, y);
            g.DrawString($"{_receipt.Total:N0} đ", fontLargeBold, brush, new RectangleF(leftMargin, y, receiptWidth, 22), rightFormat);
            y += 24;

            if (_receipt.CashGiven > 0)
            {
                DrawRow("Tiền khách đưa:", $"{_receipt.CashGiven:N0} đ");
                DrawRow("Tiền thối lại:", $"{_receipt.ChangeDue:N0} đ");
            }

            if (_receipt.EarnedPoints > 0)
            {
                DrawRow("Điểm thưởng:", $"+{_receipt.EarnedPoints} điểm");
            }

            y += 6;
            g.DrawLine(penDashed, leftMargin, y, rightMargin, y);
            y += 12;

            // 6. Lời cảm ơn
            g.DrawString("CẢM ƠN QUÝ KHÁCH VÀ HẸN GẶP LẠI!", fontBold, brush, new RectangleF(leftMargin, y, receiptWidth, 20), centerFormat);
            y += 20;
            g.DrawString("(Kiểm tra hàng hóa trước khi rời quầy)", fontItalic, brush, new RectangleF(leftMargin, y, receiptWidth, 18), centerFormat);
            y += 20;
            g.DrawString($"* {_receipt.OrderCode} *", fontRegular, brush, new RectangleF(leftMargin, y, receiptWidth, 20), centerFormat);

            e.HasMorePages = false;
        }

        // ==========================================
        // XUẤT HÓA ĐƠN RA FILE TEXT (.TXT)
        // ==========================================
        private void BtnExport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveDlg = new SaveFileDialog())
            {
                saveDlg.Filter = "File Văn Bản (*.txt)|*.txt|Tất cả tệp (*.*)|*.*";
                saveDlg.FileName = $"HoaDon_{_receipt.OrderCode}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                saveDlg.Title = "Lưu hóa đơn thanh toán";

                if (saveDlg.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        string content = BuildReceiptPlainText();
                        File.WriteAllText(saveDlg.FileName, content, Encoding.UTF8);
                        MessageBox.Show("Xuất hóa đơn thành công tại:\n" + saveDlg.FileName, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi lưu file hóa đơn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private string BuildReceiptPlainText()
        {
            var sb = new StringBuilder();
            sb.AppendLine("================================================================");
            sb.AppendLine($"                    {_receipt.StoreName}                    ");
            sb.AppendLine($"           {_receipt.StoreAddress}           ");
            sb.AppendLine($"              {_receipt.StoreHotline}             ");
            sb.AppendLine("----------------------------------------------------------------");
            sb.AppendLine("                         HÓA ĐƠN BÁN LẺ                         ");
            sb.AppendLine($"Mã HĐ: {_receipt.OrderCode,-20} Ngày: {_receipt.CreatedAt:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine($"Thu ngân: {_receipt.CashierName,-17} Hình thức: {_receipt.PaymentMethod}");
            sb.AppendLine($"Khách hàng: {_receipt.CustomerName} ({_receipt.CustomerPhone})");
            sb.AppendLine("----------------------------------------------------------------");
            sb.AppendLine(string.Format("{0,-30} {1,5} {2,12} {3,12}", "Tên mặt hàng", "SL", "Đơn giá", "Thành tiền"));
            sb.AppendLine("----------------------------------------------------------------");

            foreach (var item in _receipt.Items)
            {
                string name = item.ProductName.Length > 28 ? item.ProductName.Substring(0, 27) + "…" : item.ProductName;
                sb.AppendLine(string.Format("{0,-30} {1,5:N0} {2,12:N0} {3,12:N0}", name, item.Quantity, item.UnitPrice, item.LineTotal));
            }

            sb.AppendLine("----------------------------------------------------------------");
            sb.AppendLine(string.Format("{0,-45} {1,16:N0} đ", "Tổng tiền hàng:", _receipt.Subtotal));
            if (_receipt.Discount > 0)
            {
                sb.AppendLine(string.Format("{0,-45} -{1,15:N0} đ", "Chiết khấu / Giảm giá:", _receipt.Discount));
            }
            sb.AppendLine(string.Format("{0,-45} {1,16:N0} đ", "TỔNG CỘNG THANH TOÁN:", _receipt.Total));

            if (_receipt.CashGiven > 0)
            {
                sb.AppendLine(string.Format("{0,-45} {1,16:N0} đ", "Tiền khách đưa:", _receipt.CashGiven));
                sb.AppendLine(string.Format("{0,-45} {1,16:N0} đ", "Tiền thối lại:", _receipt.ChangeDue));
            }

            if (_receipt.EarnedPoints > 0)
            {
                sb.AppendLine(string.Format("{0,-45} +{1,15} điểm", "Điểm thưởng tích lũy:", _receipt.EarnedPoints));
            }

            sb.AppendLine("================================================================");
            sb.AppendLine("               CẢM ƠN QUÝ KHÁCH VÀ HẸN GẶP LẠI!                 ");
            sb.AppendLine("     (Vui lòng kiểm tra lại hàng hóa và hóa đơn trước khi rời quầy)     ");
            sb.AppendLine($"                         * {_receipt.OrderCode} *                         ");
            sb.AppendLine("================================================================");
            return sb.ToString();
        }
    }
}
