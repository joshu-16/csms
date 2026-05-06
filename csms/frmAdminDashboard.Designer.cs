namespace csms
{
    partial class frmAdminDashboard
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

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // frmAdminDashboard
            // 
            ClientSize = new Size(284, 261);
            Name = "frmAdminDashboard";
            Load += frmAdminDashboard_Load;
            ResumeLayout(false);
        }
    }
}