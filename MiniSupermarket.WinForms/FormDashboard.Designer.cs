namespace MiniSupermarket.WinForms
{
    partial class FormDashboard
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // FormDashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.Name = "FormDashboard";
            this.Text = "Dashboard Tổng Quan & Hiệu Suất";
            this.Load += new System.EventHandler(this.FormDashboard_Load);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
