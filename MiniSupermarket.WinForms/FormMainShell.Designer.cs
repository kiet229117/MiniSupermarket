namespace MiniSupermarket.WinForms
{
    partial class FormMainShell
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // panelSidebar
            // 
            this.panelSidebar = new System.Windows.Forms.Panel();
            this.panelSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelSidebar.Width = 220;
            this.panelSidebar.BackColor = System.Drawing.Color.FromArgb(24, 30, 48);

            // Buttons on sidebar
            this.btnPOS = new System.Windows.Forms.Button();
            this.btnOrder = new System.Windows.Forms.Button();
            this.btnProduct = new System.Windows.Forms.Button();
            this.btnCategory = new System.Windows.Forms.Button();
            this.btnBrand = new System.Windows.Forms.Button();
            this.btnSupplier = new System.Windows.Forms.Button();
            this.btnCustomer = new System.Windows.Forms.Button();
            this.btnReports = new System.Windows.Forms.Button();
            this.btnUserManage = new System.Windows.Forms.Button();
            this.btnLogout = new System.Windows.Forms.Button();

            // Common button style
            var sidebarButtons = new System.Windows.Forms.Button[] {
                this.btnPOS, this.btnOrder, this.btnProduct, this.btnCategory,
                this.btnBrand, this.btnSupplier, this.btnCustomer, this.btnReports, this.btnUserManage, this.btnLogout };

            int topPos = 20;
            foreach (var btn in sidebarButtons)
            {
                btn.Width = this.panelSidebar.Width - 24;
                btn.Height = 42;
                btn.Left = 12;
                btn.Top = topPos;
                btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.BackColor = System.Drawing.Color.FromArgb(24, 30, 48);
                btn.ForeColor = System.Drawing.Color.White;
                btn.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
                btn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
                btn.Cursor = System.Windows.Forms.Cursors.Hand;
                topPos += 50;
                this.panelSidebar.Controls.Add(btn);
            }

            this.btnPOS.Text = "  🛒 Bán Hàng (POS)";
            this.btnOrder.Text = "  🧾 Đơn Hàng";
            this.btnProduct.Text = "  📦 Sản Phẩm & Kho";
            this.btnCategory.Text = "  📁 Nhóm Hàng";
            this.btnBrand.Text = "  🔖 Thương Hiệu";
            this.btnSupplier.Text = "  🚚 Nhà Cung Cấp";
            this.btnCustomer.Text = "  👥 Khách Hàng";
            this.btnReports.Text = "  📊 Báo Cáo";
            this.btnUserManage.Text = "  👤 Quản Trị User";
            this.btnLogout.Text = "  🚪 Đăng Xuất";
            this.btnLogout.BackColor = System.Drawing.Color.FromArgb(35, 42, 65);

            // Wire click handlers
            this.btnPOS.Click += new System.EventHandler(this.btnPOS_Click);
            this.btnProduct.Click += new System.EventHandler(this.btnProduct_Click);
            this.btnOrder.Click += new System.EventHandler(this.btnOrder_Click);
            this.btnCategory.Click += new System.EventHandler(this.btnCategory_Click);
            this.btnBrand.Click += new System.EventHandler(this.btnBrand_Click);
            this.btnSupplier.Click += new System.EventHandler(this.btnSupplier_Click);
            this.btnCustomer.Click += new System.EventHandler(this.btnCustomer_Click);
            this.btnReports.Click += new System.EventHandler(this.btnReports_Click);
            this.btnUserManage.Click += new System.EventHandler(this.btnUser_Click);
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);

            // 
            // panelMainContent
            // 
            this.panelMainContent = new System.Windows.Forms.Panel();
            this.panelMainContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMainContent.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);

            // 
            // lblTitle (header)
            // 
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Height = 52;
            this.lblTitle.Text = "HỆ THỐNG BÁCH HÓA THỰC PHẨM HEALTHY MART";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitle.BackColor = System.Drawing.Color.White;
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.lblTitle.Padding = new System.Windows.Forms.Padding(16, 0, 0, 0);

            // 
            // lblUserInfo (display current user)
            // 
            this.lblUserInfo = new System.Windows.Forms.Label();
            this.lblUserInfo.AutoSize = false;
            this.lblUserInfo.Height = 40;
            this.lblUserInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblUserInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblUserInfo.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.lblUserInfo.ForeColor = System.Drawing.Color.LightSkyBlue;
            this.lblUserInfo.BackColor = System.Drawing.Color.FromArgb(15, 20, 32);
            this.lblUserInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);

            // Add controls to form
            this.Controls.Add(this.panelMainContent);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.panelSidebar);

            // Add user info label into sidebar bottom
            this.panelSidebar.Controls.Add(this.lblUserInfo);
            this.lblUserInfo.BringToFront();

            // 
            // FormMainShell
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1180, 700);
            this.MinimumSize = new System.Drawing.Size(1100, 650);
            this.Name = "FormMainShell";
            this.Text = "Hệ thống Bách Hóa Thực Phẩm Healthy Mart";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.FormMainShell_Load);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
