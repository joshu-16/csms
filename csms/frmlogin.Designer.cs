namespace csms
{
    partial class frmlogin
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
            // frmlogin
            // 
            ClientSize = new Size(284, 261);
            Name = "frmlogin";
            Load += frmlogin_Load;
            ResumeLayout(false);
        }
    }
}