namespace MiniSupermarket.WinForms
{
    partial class FormCategoryManagement
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.Label lblID;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblDescription;

        private System.Windows.Forms.Label lblListTitle;
        private System.Windows.Forms.Label lblInfoTitle;

        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.TextBox txtID;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.TextBox txtDescription;

        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnReload;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button btnDelete;

        private System.Windows.Forms.DataGridView dataGridView1;

        private System.Windows.Forms.DataGridViewTextBoxColumn colID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDescription;

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.lblSearch = new System.Windows.Forms.Label();
            this.lblID = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();

            this.lblListTitle = new System.Windows.Forms.Label();
            this.lblInfoTitle = new System.Windows.Forms.Label();

            this.txtSearch = new System.Windows.Forms.TextBox();
            this.txtID = new System.Windows.Forms.TextBox();
            this.txtName = new System.Windows.Forms.TextBox();
            this.txtDescription = new System.Windows.Forms.TextBox();

            this.btnSearch = new System.Windows.Forms.Button();
            this.btnReload = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();

            this.dataGridView1 = new System.Windows.Forms.DataGridView();

            this.colID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();

            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();

            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();

            // =====================================================
            // FORM
            // =====================================================

            this.AutoScaleDimensions =
                new System.Drawing.SizeF(7F, 15F);

            this.AutoScaleMode =
                System.Windows.Forms.AutoScaleMode.Font;

            this.BackColor =
                System.Drawing.Color.White;

            this.ClientSize =
                new System.Drawing.Size(900, 560);

            this.MinimumSize =
                new System.Drawing.Size(900, 560);

            this.StartPosition =
                System.Windows.Forms.FormStartPosition.CenterScreen;

            this.Name =
                "FormCategoryManagement";

            this.Text =
                "Quản lý Nhóm hàng";

            // =====================================================
            // SEARCH LABEL
            // =====================================================

            this.lblSearch.AutoSize = true;

            this.lblSearch.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F,
                    System.Drawing.FontStyle.Bold);

            this.lblSearch.ForeColor =
                System.Drawing.Color.FromArgb(50, 50, 50);

            this.lblSearch.Location =
                new System.Drawing.Point(25, 22);

            this.lblSearch.Name =
                "lblSearch";

            this.lblSearch.Text =
                "Tìm kiếm";

            // =====================================================
            // SEARCH TEXTBOX
            // =====================================================

            this.txtSearch.BorderStyle =
                System.Windows.Forms.BorderStyle.FixedSingle;

            this.txtSearch.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    10F);

            this.txtSearch.Location =
                new System.Drawing.Point(25, 43);

            this.txtSearch.Name =
                "txtSearch";

            this.txtSearch.Size =
                new System.Drawing.Size(350, 25);

            this.txtSearch.TabIndex = 0;

            this.txtSearch.ForeColor =
                System.Drawing.Color.Gray;

            this.txtSearch.Enter +=
                new System.EventHandler(
                    this.txtSearch_Enter);

            this.txtSearch.Leave +=
                new System.EventHandler(
                    this.txtSearch_Leave);

            // =====================================================
            // SEARCH BUTTON
            // =====================================================

            this.btnSearch.BackColor =
                System.Drawing.Color.FromArgb(0, 123, 255);

            this.btnSearch.FlatAppearance.BorderSize = 0;

            this.btnSearch.FlatStyle =
                System.Windows.Forms.FlatStyle.Flat;

            this.btnSearch.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F,
                    System.Drawing.FontStyle.Bold);

            this.btnSearch.ForeColor =
                System.Drawing.Color.White;

            this.btnSearch.Location =
                new System.Drawing.Point(385, 42);

            this.btnSearch.Name =
                "btnSearch";

            this.btnSearch.Size =
                new System.Drawing.Size(90, 28);

            this.btnSearch.TabIndex = 1;

            this.btnSearch.Text =
                "Tìm kiếm";

            this.btnSearch.UseVisualStyleBackColor = false;

            this.btnSearch.Click +=
                new System.EventHandler(
                    this.btnSearch_Click);

            // =====================================================
            // RELOAD BUTTON
            // =====================================================

            this.btnReload.BackColor =
                System.Drawing.Color.FromArgb(108, 117, 125);

            this.btnReload.FlatAppearance.BorderSize = 0;

            this.btnReload.FlatStyle =
                System.Windows.Forms.FlatStyle.Flat;

            this.btnReload.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F,
                    System.Drawing.FontStyle.Bold);

            this.btnReload.ForeColor =
                System.Drawing.Color.White;

            this.btnReload.Location =
                new System.Drawing.Point(483, 42);

            this.btnReload.Name =
                "btnReload";

            this.btnReload.Size =
                new System.Drawing.Size(80, 28);

            this.btnReload.TabIndex = 2;

            this.btnReload.Text =
                "Tải lại";

            this.btnReload.UseVisualStyleBackColor = false;

            this.btnReload.Click +=
                new System.EventHandler(
                    this.btnReload_Click);

            // =====================================================
            // LIST TITLE
            // =====================================================

            this.lblListTitle.AutoSize = true;

            this.lblListTitle.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    11F,
                    System.Drawing.FontStyle.Bold);

            this.lblListTitle.ForeColor =
                System.Drawing.Color.FromArgb(33, 37, 41);

            this.lblListTitle.Location =
                new System.Drawing.Point(25, 90);

            this.lblListTitle.Name =
                "lblListTitle";

            this.lblListTitle.Text =
                "Danh sách Nhóm hàng";

            // =====================================================
            // DATA GRID
            // =====================================================

            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeRows = false;

            this.dataGridView1.AutoGenerateColumns = false;

            this.dataGridView1.BackgroundColor =
                System.Drawing.Color.White;

            this.dataGridView1.BorderStyle =
                System.Windows.Forms.BorderStyle.FixedSingle;

            this.dataGridView1.CellBorderStyle =
                System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;

            this.dataGridView1.ColumnHeadersBorderStyle =
                System.Windows.Forms.DataGridViewHeaderBorderStyle.None;

            this.dataGridView1.ColumnHeadersDefaultCellStyle =
                new System.Windows.Forms.DataGridViewCellStyle
                {
                    BackColor = System.Drawing.Color.FromArgb(0, 123, 255),
                    ForeColor = System.Drawing.Color.White,
                    Font = new System.Drawing.Font(
                        "Segoe UI",
                        9F,
                        System.Drawing.FontStyle.Bold),
                    Alignment =
                        System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
                };

            this.dataGridView1.ColumnHeadersHeight = 35;

            this.dataGridView1.DefaultCellStyle =
                new System.Windows.Forms.DataGridViewCellStyle
                {
                    BackColor = System.Drawing.Color.White,
                    ForeColor = System.Drawing.Color.FromArgb(50, 50, 50),
                    Font = new System.Drawing.Font(
                        "Segoe UI",
                        9F),
                    SelectionBackColor =
                        System.Drawing.Color.FromArgb(220, 235, 252),
                    SelectionForeColor =
                        System.Drawing.Color.FromArgb(30, 30, 30)
                };

            this.dataGridView1.EnableHeadersVisualStyles = false;

            this.dataGridView1.GridColor =
                System.Drawing.Color.FromArgb(230, 230, 230);

            this.dataGridView1.Location =
                new System.Drawing.Point(25, 120);

            this.dataGridView1.MultiSelect = false;

            this.dataGridView1.Name =
                "dataGridView1";

            this.dataGridView1.ReadOnly = true;

            this.dataGridView1.RowHeadersVisible = false;

            this.dataGridView1.RowTemplate.Height = 32;

            this.dataGridView1.SelectionMode =
                System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;

            this.dataGridView1.Size =
                new System.Drawing.Size(540, 350);

            this.dataGridView1.TabIndex = 3;

            this.dataGridView1.Columns.AddRange(
                new System.Windows.Forms.DataGridViewColumn[]
                {
                this.colID,
                this.colName,
                this.colDescription
                });

            this.dataGridView1.CellClick +=
                new System.Windows.Forms.DataGridViewCellEventHandler(
                    this.dataGridView1_CellClick);

            // =====================================================
            // COLUMN ID
            // =====================================================

            this.colID.DataPropertyName = "ID";
            this.colID.HeaderText = "Mã ID";
            this.colID.Name = "colID";
            this.colID.ReadOnly = true;
            this.colID.Width = 70;

            // =====================================================
            // COLUMN NAME
            // =====================================================

            this.colName.DataPropertyName = "Name";
            this.colName.HeaderText = "Tên Nhóm hàng";
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            this.colName.Width = 180;

            // =====================================================
            // COLUMN DESCRIPTION
            // =====================================================

            this.colDescription.DataPropertyName =
                "Description";

            this.colDescription.HeaderText =
                "Mô tả";

            this.colDescription.Name =
                "colDescription";

            this.colDescription.ReadOnly =
                true;

            this.colDescription.AutoSizeMode =
                System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;

            // =====================================================
            // INFO TITLE
            // =====================================================

            this.lblInfoTitle.AutoSize = true;

            this.lblInfoTitle.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    11F,
                    System.Drawing.FontStyle.Bold);

            this.lblInfoTitle.ForeColor =
                System.Drawing.Color.FromArgb(33, 37, 41);

            this.lblInfoTitle.Location =
                new System.Drawing.Point(595, 90);

            this.lblInfoTitle.Name =
                "lblInfoTitle";

            this.lblInfoTitle.Text =
                "Thông tin Nhóm hàng";

            // =====================================================
            // ID LABEL
            // =====================================================

            this.lblID.AutoSize = true;

            this.lblID.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F,
                    System.Drawing.FontStyle.Bold);

            this.lblID.Location =
                new System.Drawing.Point(595, 122);

            this.lblID.Name =
                "lblID";

            this.lblID.Text =
                "Mã ID";

            // =====================================================
            // ID TEXTBOX
            // =====================================================

            this.txtID.BackColor =
                System.Drawing.Color.FromArgb(245, 245, 245);

            this.txtID.BorderStyle =
                System.Windows.Forms.BorderStyle.FixedSingle;

            this.txtID.Location =
                new System.Drawing.Point(595, 143);

            this.txtID.Name =
                "txtID";

            this.txtID.ReadOnly = true;

            this.txtID.Size =
                new System.Drawing.Size(275, 23);

            this.txtID.TabIndex = 4;

            // =====================================================
            // NAME LABEL
            // =====================================================

            this.lblName.AutoSize = true;

            this.lblName.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F,
                    System.Drawing.FontStyle.Bold);

            this.lblName.Location =
                new System.Drawing.Point(595, 178);

            this.lblName.Name =
                "lblName";

            this.lblName.Text =
                "Tên Nhóm hàng";

            // =====================================================
            // NAME TEXTBOX
            // =====================================================

            this.txtName.BorderStyle =
                System.Windows.Forms.BorderStyle.FixedSingle;

            this.txtName.Location =
                new System.Drawing.Point(595, 199);

            this.txtName.Name =
                "txtName";

            this.txtName.Size =
                new System.Drawing.Size(275, 23);

            this.txtName.TabIndex = 5;

            // =====================================================
            // DESCRIPTION LABEL
            // =====================================================

            this.lblDescription.AutoSize = true;

            this.lblDescription.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F,
                    System.Drawing.FontStyle.Bold);

            this.lblDescription.Location =
                new System.Drawing.Point(595, 234);

            this.lblDescription.Name =
                "lblDescription";

            this.lblDescription.Text =
                "Mô tả";

            // =====================================================
            // DESCRIPTION TEXTBOX
            // =====================================================

            this.txtDescription.BorderStyle =
                System.Windows.Forms.BorderStyle.FixedSingle;

            this.txtDescription.Location =
                new System.Drawing.Point(595, 255);

            this.txtDescription.Multiline = true;

            this.txtDescription.Name =
                "txtDescription";

            this.txtDescription.ScrollBars =
                System.Windows.Forms.ScrollBars.Vertical;

            this.txtDescription.Size =
                new System.Drawing.Size(275, 100);

            this.txtDescription.TabIndex = 6;

            // =====================================================
            // ADD BUTTON
            // =====================================================

            this.btnAdd.BackColor =
                System.Drawing.Color.FromArgb(40, 167, 69);

            this.btnAdd.FlatAppearance.BorderSize = 0;

            this.btnAdd.FlatStyle =
                System.Windows.Forms.FlatStyle.Flat;

            this.btnAdd.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F,
                    System.Drawing.FontStyle.Bold);

            this.btnAdd.ForeColor =
                System.Drawing.Color.White;

            this.btnAdd.Location =
                new System.Drawing.Point(595, 375);

            this.btnAdd.Name =
                "btnAdd";

            this.btnAdd.Size =
                new System.Drawing.Size(85, 32);

            this.btnAdd.TabIndex = 7;

            this.btnAdd.Text =
                "Thêm mới";

            this.btnAdd.UseVisualStyleBackColor = false;

            this.btnAdd.Click +=
                new System.EventHandler(
                    this.btnAdd_Click);

            // =====================================================
            // UPDATE BUTTON
            // =====================================================

            this.btnUpdate.BackColor =
                System.Drawing.Color.FromArgb(255, 193, 7);

            this.btnUpdate.FlatAppearance.BorderSize = 0;

            this.btnUpdate.FlatStyle =
                System.Windows.Forms.FlatStyle.Flat;

            this.btnUpdate.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F,
                    System.Drawing.FontStyle.Bold);

            this.btnUpdate.ForeColor =
                System.Drawing.Color.Black;

            this.btnUpdate.Location =
                new System.Drawing.Point(690, 375);

            this.btnUpdate.Name =
                "btnUpdate";

            this.btnUpdate.Size =
                new System.Drawing.Size(85, 32);

            this.btnUpdate.TabIndex = 8;

            this.btnUpdate.Text =
                "Cập nhật";

            this.btnUpdate.UseVisualStyleBackColor = false;

            this.btnUpdate.Click +=
                new System.EventHandler(
                    this.btnUpdate_Click);

            // =====================================================
            // DELETE BUTTON
            // =====================================================

            this.btnDelete.BackColor =
                System.Drawing.Color.FromArgb(220, 53, 69);

            this.btnDelete.FlatAppearance.BorderSize = 0;

            this.btnDelete.FlatStyle =
                System.Windows.Forms.FlatStyle.Flat;

            this.btnDelete.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F,
                    System.Drawing.FontStyle.Bold);

            this.btnDelete.ForeColor =
                System.Drawing.Color.White;

            this.btnDelete.Location =
                new System.Drawing.Point(785, 375);

            this.btnDelete.Name =
                "btnDelete";

            this.btnDelete.Size =
                new System.Drawing.Size(85, 32);

            this.btnDelete.TabIndex = 9;

            this.btnDelete.Text =
                "Xóa";

            this.btnDelete.UseVisualStyleBackColor = false;

            this.btnDelete.Click +=
                new System.EventHandler(
                    this.btnDelete_Click);

            // =====================================================
            // STATUS STRIP
            // =====================================================

            this.statusStrip1.BackColor =
                System.Drawing.Color.FromArgb(248, 249, 250);

            this.statusStrip1.Items.AddRange(
                new System.Windows.Forms.ToolStripItem[]
                {
                this.lblStatus
                });

            this.statusStrip1.Location =
                new System.Drawing.Point(0, 538);

            this.statusStrip1.Name =
                "statusStrip1";

            this.statusStrip1.Size =
                new System.Drawing.Size(900, 22);

            this.statusStrip1.TabIndex = 10;

            // =====================================================
            // STATUS
            // =====================================================

            this.lblStatus.ForeColor =
                System.Drawing.Color.FromArgb(80, 80, 80);

            this.lblStatus.Name =
                "lblStatus";

            this.lblStatus.Size =
                new System.Drawing.Size(39, 17);

            this.lblStatus.Text =
                "Ready";

            // =====================================================
            // ADD CONTROLS
            // =====================================================

            this.Controls.Add(this.lblSearch);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.btnReload);

            this.Controls.Add(this.lblListTitle);
            this.Controls.Add(this.dataGridView1);

            this.Controls.Add(this.lblInfoTitle);

            this.Controls.Add(this.lblID);
            this.Controls.Add(this.txtID);

            this.Controls.Add(this.lblName);
            this.Controls.Add(this.txtName);

            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.txtDescription);

            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.btnDelete);

            this.Controls.Add(this.statusStrip1);

            // =====================================================
            // LOAD EVENT
            // =====================================================

            this.Load +=
                new System.EventHandler(
                    this.FormCategoryManagement_Load_1);

            ((System.ComponentModel.ISupportInitialize)
                (this.dataGridView1)).EndInit();

            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();

            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }

}