using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Font = System.Drawing.Font;

namespace csms
{
    public partial class frmAccountantDashboard : Form
    {
        private string connStr = @"Server=DESKTOP-NLHET7V\SQLEXPRESS;Database=CSMS;Trusted_Connection=True;";
        private string loggedInName;
        private int loggedInUserId;
        private Panel contentPanel;

        public frmAccountantDashboard(int userId, string name)
        {
            InitializeComponent();
            loggedInUserId = userId;
            loggedInName = name;
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.Text = "Accountant Dashboard";
            CreateDashboard();
        }

        private void CreateDashboard()
        {
            // Title Bar
            Panel titleBar = new Panel { Height = 50, Dock = DockStyle.Top, BackColor = Color.FromArgb(44, 62, 80) };
            Label lblTitle = new Label { Text = "ACCOUNTANT DASHBOARD", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.White, Location = new Point(20, 15), AutoSize = true };
            Label lblUser = new Label { Text = "Welcome, " + loggedInName, Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(200, 200, 200), Location = new Point(this.Width - 180, 15), AutoSize = true };

            Button btnMinimize = new Button { Text = "─", Size = new Size(45, 45), Location = new Point(this.Width - 135, 2), BackColor = Color.Transparent, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 16, FontStyle.Bold), Cursor = Cursors.Hand };
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            Button btnMaximize = new Button { Text = "□", Size = new Size(45, 45), Location = new Point(this.Width - 90, 2), BackColor = Color.Transparent, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 16, FontStyle.Bold), Cursor = Cursors.Hand };
            btnMaximize.Click += (s, e) =>
            {
                if (this.WindowState == FormWindowState.Maximized)
                {
                    this.WindowState = FormWindowState.Normal;
                    btnMaximize.Text = "□";
                }
                else
                {
                    this.WindowState = FormWindowState.Maximized;
                    btnMaximize.Text = "❐";
                }
            };

            Button btnClose = new Button { Text = "X", Size = new Size(45, 45), Location = new Point(this.Width - 45, 2), BackColor = Color.Transparent, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 14, FontStyle.Bold), Cursor = Cursors.Hand };
            btnClose.Click += (s, e) => Application.Exit();

            titleBar.Controls.Add(lblTitle);
            titleBar.Controls.Add(lblUser);
            titleBar.Controls.Add(btnMinimize);
            titleBar.Controls.Add(btnMaximize);
            titleBar.Controls.Add(btnClose);
            this.Controls.Add(titleBar);

            // Sidebar
            Panel sidebar = new Panel { Width = 240, Dock = DockStyle.Left, BackColor = Color.FromArgb(52, 73, 94) };
            Label lblLogo = new Label { Text = "CSMS", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Color.White, Location = new Point(85, 30), AutoSize = true };
            sidebar.Controls.Add(lblLogo);

            string[] menuItems = { "Dashboard", "Process Payments", "Payment History", "Logout" };
            int yPos = 100;

            for (int i = 0; i < menuItems.Length; i++)
            {
                Button btn = new Button();
                btn.Text = menuItems[i];
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.BackColor = Color.Transparent;
                btn.ForeColor = Color.White;
                btn.Font = new Font("Segoe UI", 11, FontStyle.Regular);
                btn.TextAlign = ContentAlignment.MiddleLeft;
                btn.Size = new Size(240, 45);
                btn.Location = new Point(0, yPos);
                btn.Cursor = Cursors.Hand;
                btn.Tag = menuItems[i];
                btn.Click += MenuButton_Click;
                sidebar.Controls.Add(btn);
                yPos += 50;
            }

            Label lblVersion = new Label { Text = "Version 2.0", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(150, 150, 150), Location = new Point(85, this.Height - 60), AutoSize = true };
            sidebar.Controls.Add(lblVersion);
            this.Controls.Add(sidebar);

            // Content Panel
            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = Color.FromArgb(240, 242, 245);
            contentPanel.Padding = new Padding(25);
            contentPanel.AutoScroll = true;
            this.Controls.Add(contentPanel);
            contentPanel.BringToFront();

            LoadDashboard();

            this.Resize += (s, e) =>
            {
                lblUser.Location = new Point(this.Width - 180, 15);
                btnMinimize.Location = new Point(this.Width - 135, 2);
                btnMaximize.Location = new Point(this.Width - 90, 2);
                btnClose.Location = new Point(this.Width - 45, 2);
                lblVersion.Location = new Point(85, this.Height - 60);
            };
        }

        private void MenuButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            string menu = btn.Tag.ToString();
            contentPanel.Controls.Clear();

            if (menu == "Dashboard") LoadDashboard();
            else if (menu == "Process Payments") LoadProcessPayments();
            else if (menu == "Payment History") LoadPaymentHistory();
            else if (menu == "Logout")
            {
                this.Close();
                frmlogin login = new frmlogin();
                login.Show();
            }
        }

        private void LoadDashboard()
        {
            contentPanel.Controls.Clear();

            FlowLayoutPanel statsPanel = new FlowLayoutPanel();
            statsPanel.Location = new Point(0, 0);
            statsPanel.Width = contentPanel.Width - 40;
            statsPanel.Height = 120;

            var stats = GetDashboardStats();

            statsPanel.Controls.Add(CreateStatCard("Total Farmers", stats.TotalFarmers.ToString(), Color.FromArgb(52, 152, 219)));
            statsPanel.Controls.Add(CreateStatCard("Pending Deliveries", stats.PendingDeliveries.ToString(), Color.FromArgb(241, 196, 15)));
            statsPanel.Controls.Add(CreateStatCard("Pending Advances", stats.PendingAdvances.ToString(), Color.FromArgb(231, 76, 60)));
            statsPanel.Controls.Add(CreateStatCard("Total Payments Made", "KES " + stats.TotalPaymentsMade.ToString("N0"), Color.FromArgb(46, 204, 113)));

            contentPanel.Controls.Add(statsPanel);

            GroupBox grpRecent = new GroupBox();
            grpRecent.Text = " Recent Payments ";
            grpRecent.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            grpRecent.Location = new Point(0, 140);
            grpRecent.Size = new Size(contentPanel.Width - 40, 300);
            grpRecent.BackColor = Color.White;

            ListView lvRecent = new ListView();
            lvRecent.View = View.Details;
            lvRecent.FullRowSelect = true;
            lvRecent.GridLines = true;
            lvRecent.Dock = DockStyle.Fill;
            lvRecent.Font = new Font("Segoe UI", 10);
            lvRecent.BackColor = Color.White;
            lvRecent.Columns.Add("Date", 100);
            lvRecent.Columns.Add("Farmer", 150);
            lvRecent.Columns.Add("Gross", 120);
            lvRecent.Columns.Add("Advances", 120);
            lvRecent.Columns.Add("Net Paid", 120);

            LoadRecentPayments(lvRecent);

            grpRecent.Controls.Add(lvRecent);
            contentPanel.Controls.Add(grpRecent);
        }

        private void LoadProcessPayments()
        {
            contentPanel.Controls.Clear();

            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.AutoScroll = true;

            // Search Section
            GroupBox grpSearch = new GroupBox();
            grpSearch.Text = " Search Farmer ";
            grpSearch.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            grpSearch.Location = new Point(10, 10);
            grpSearch.Size = new Size(contentPanel.Width - 40, 80);
            grpSearch.BackColor = Color.White;

            Label lblSearch = new Label();
            lblSearch.Text = "National ID:";
            lblSearch.Location = new Point(20, 30);
            lblSearch.AutoSize = true;
            lblSearch.Font = new Font("Segoe UI", 11);

            TextBox txtSearch = new TextBox();
            txtSearch.Location = new Point(120, 27);
            txtSearch.Width = 200;
            txtSearch.Height = 32;
            txtSearch.Font = new Font("Segoe UI", 11);

            Button btnSearch = new Button();
            btnSearch.Text = "Search";
            btnSearch.Location = new Point(340, 25);
            btnSearch.Size = new Size(100, 35);
            btnSearch.BackColor = Color.FromArgb(52, 152, 219);
            btnSearch.ForeColor = Color.White;
            btnSearch.FlatStyle = FlatStyle.Flat;
            btnSearch.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnSearch.Cursor = Cursors.Hand;

            grpSearch.Controls.Add(lblSearch);
            grpSearch.Controls.Add(txtSearch);
            grpSearch.Controls.Add(btnSearch);
            mainPanel.Controls.Add(grpSearch);

            // Farmer Info Panel
            GroupBox grpFarmerInfo = new GroupBox();
            grpFarmerInfo.Text = " Farmer Information ";
            grpFarmerInfo.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            grpFarmerInfo.Location = new Point(10, 100);
            grpFarmerInfo.Size = new Size(contentPanel.Width - 40, 130);
            grpFarmerInfo.BackColor = Color.White;

            Label lblFarmerName = new Label();
            lblFarmerName.Name = "lblFarmerName";
            lblFarmerName.Text = "Name: --";
            lblFarmerName.Location = new Point(20, 30);
            lblFarmerName.AutoSize = true;
            lblFarmerName.Font = new Font("Segoe UI", 11);

            Label lblFarmerNID = new Label();
            lblFarmerNID.Name = "lblFarmerNID";
            lblFarmerNID.Text = "National ID: --";
            lblFarmerNID.Location = new Point(20, 55);
            lblFarmerNID.AutoSize = true;
            lblFarmerNID.Font = new Font("Segoe UI", 11);

            Label lblTotalDeliveries = new Label();
            lblTotalDeliveries.Name = "lblTotalDeliveries";
            lblTotalDeliveries.Text = "Total Deliveries: 0 KG";
            lblTotalDeliveries.Location = new Point(350, 30);
            lblTotalDeliveries.AutoSize = true;
            lblTotalDeliveries.Font = new Font("Segoe UI", 11, FontStyle.Bold);

            Label lblTotalEarnings = new Label();
            lblTotalEarnings.Name = "lblTotalEarnings";
            lblTotalEarnings.Text = "Total Earnings: KES 0";
            lblTotalEarnings.Location = new Point(350, 55);
            lblTotalEarnings.AutoSize = true;
            lblTotalEarnings.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblTotalEarnings.ForeColor = Color.FromArgb(46, 125, 50);

            Label lblTotalAdvances = new Label();
            lblTotalAdvances.Name = "lblTotalAdvances";
            lblTotalAdvances.Text = "Total Advances: KES 0";
            lblTotalAdvances.Location = new Point(350, 80);
            lblTotalAdvances.AutoSize = true;
            lblTotalAdvances.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblTotalAdvances.ForeColor = Color.FromArgb(231, 76, 60);

            Label lblNetPayable = new Label();
            lblNetPayable.Name = "lblNetPayable";
            lblNetPayable.Text = "Net Payable: KES 0";
            lblNetPayable.Location = new Point(350, 105);
            lblNetPayable.AutoSize = true;
            lblNetPayable.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblNetPayable.ForeColor = Color.FromArgb(52, 152, 219);

            grpFarmerInfo.Controls.Add(lblFarmerName);
            grpFarmerInfo.Controls.Add(lblFarmerNID);
            grpFarmerInfo.Controls.Add(lblTotalDeliveries);
            grpFarmerInfo.Controls.Add(lblTotalEarnings);
            grpFarmerInfo.Controls.Add(lblTotalAdvances);
            grpFarmerInfo.Controls.Add(lblNetPayable);
            mainPanel.Controls.Add(grpFarmerInfo);

            // Deliveries Grid
            GroupBox grpDeliveries = new GroupBox();
            grpDeliveries.Text = " Pending Deliveries ";
            grpDeliveries.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            grpDeliveries.Location = new Point(10, 240);
            grpDeliveries.Size = new Size(contentPanel.Width - 40, 200);
            grpDeliveries.BackColor = Color.White;

            DataGridView dgvDeliveries = new DataGridView();
            dgvDeliveries.Name = "dgvDeliveries";
            dgvDeliveries.Location = new Point(10, 25);
            dgvDeliveries.Size = new Size(grpDeliveries.Width - 30, 160);
            dgvDeliveries.AllowUserToAddRows = false;
            dgvDeliveries.ReadOnly = true;
            dgvDeliveries.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDeliveries.BackgroundColor = Color.White;
            dgvDeliveries.Font = new Font("Segoe UI", 10);
            dgvDeliveries.RowTemplate.Height = 30;
            dgvDeliveries.Columns.Add("Date", "Date");
            dgvDeliveries.Columns.Add("Weight", "Weight");
            dgvDeliveries.Columns.Add("Grade", "Grade");
            dgvDeliveries.Columns.Add("Amount", "Amount");

            grpDeliveries.Controls.Add(dgvDeliveries);
            mainPanel.Controls.Add(grpDeliveries);

            // Advances Grid
            GroupBox grpAdvances = new GroupBox();
            grpAdvances.Text = " Pending Advances ";
            grpAdvances.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            grpAdvances.Location = new Point(10, 450);
            grpAdvances.Size = new Size(contentPanel.Width - 40, 150);
            grpAdvances.BackColor = Color.White;

            DataGridView dgvAdvances = new DataGridView();
            dgvAdvances.Name = "dgvAdvances";
            dgvAdvances.Location = new Point(10, 25);
            dgvAdvances.Size = new Size(grpAdvances.Width - 30, 110);
            dgvAdvances.AllowUserToAddRows = false;
            dgvAdvances.ReadOnly = true;
            dgvAdvances.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvAdvances.BackgroundColor = Color.White;
            dgvAdvances.Font = new Font("Segoe UI", 10);
            dgvAdvances.RowTemplate.Height = 30;
            dgvAdvances.Columns.Add("Date", "Date");
            dgvAdvances.Columns.Add("Product", "Product");
            dgvAdvances.Columns.Add("Quantity", "Qty");
            dgvAdvances.Columns.Add("Amount", "Amount");

            grpAdvances.Controls.Add(dgvAdvances);
            mainPanel.Controls.Add(grpAdvances);

            // Payment Details Panel
            GroupBox grpPayment = new GroupBox();
            grpPayment.Text = " Payment Summary ";
            grpPayment.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            grpPayment.Location = new Point(10, 610);
            grpPayment.Size = new Size(contentPanel.Width - 40, 150);
            grpPayment.BackColor = Color.White;

            Label lblGrossTotal = new Label();
            lblGrossTotal.Name = "lblGrossTotal";
            lblGrossTotal.Text = "Total Deliveries: KES 0.00";
            lblGrossTotal.Location = new Point(20, 25);
            lblGrossTotal.AutoSize = true;
            lblGrossTotal.Font = new Font("Segoe UI", 11, FontStyle.Bold);

            Label lblAdvancesTotal = new Label();
            lblAdvancesTotal.Name = "lblAdvancesTotal";
            lblAdvancesTotal.Text = "Total Advances: KES 0.00";
            lblAdvancesTotal.Location = new Point(20, 50);
            lblAdvancesTotal.AutoSize = true;
            lblAdvancesTotal.Font = new Font("Segoe UI", 11);

            Label lblNetTotal = new Label();
            lblNetTotal.Name = "lblNetTotal";
            lblNetTotal.Text = "Net Payment: KES 0.00";
            lblNetTotal.Location = new Point(20, 75);
            lblNetTotal.AutoSize = true;
            lblNetTotal.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblNetTotal.ForeColor = Color.FromArgb(46, 125, 50);

            Label lblMethod = new Label();
            lblMethod.Text = "Payment Method:";
            lblMethod.Location = new Point(400, 25);
            lblMethod.AutoSize = true;
            lblMethod.Font = new Font("Segoe UI", 11);

            ComboBox cmbMethod = new ComboBox();
            cmbMethod.Name = "cmbMethod";
            cmbMethod.Location = new Point(530, 22);
            cmbMethod.Size = new Size(150, 28);
            cmbMethod.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMethod.Font = new Font("Segoe UI", 11);
            cmbMethod.Items.AddRange(new string[] { "M-Pesa", "Bank Transfer" });

            Label lblRef = new Label();
            lblRef.Text = "Transaction Ref:";
            lblRef.Location = new Point(400, 60);
            lblRef.AutoSize = true;
            lblRef.Font = new Font("Segoe UI", 11);

            TextBox txtRef = new TextBox();
            txtRef.Name = "txtRef";
            txtRef.Location = new Point(530, 57);
            txtRef.Width = 200;
            txtRef.Height = 30;
            txtRef.Font = new Font("Segoe UI", 11);

            Button btnProcess = new Button();
            btnProcess.Text = "Process Payment";
            btnProcess.Location = new Point(530, 100);
            btnProcess.Size = new Size(150, 40);
            btnProcess.BackColor = Color.FromArgb(46, 204, 113);
            btnProcess.ForeColor = Color.White;
            btnProcess.FlatStyle = FlatStyle.Flat;
            btnProcess.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnProcess.Cursor = Cursors.Hand;

            Button btnPrint = new Button();
            btnPrint.Text = "Print Receipt";
            btnPrint.Location = new Point(370, 100);
            btnPrint.Size = new Size(140, 40);
            btnPrint.BackColor = Color.FromArgb(155, 89, 182);
            btnPrint.ForeColor = Color.White;
            btnPrint.FlatStyle = FlatStyle.Flat;
            btnPrint.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnPrint.Cursor = Cursors.Hand;
            btnPrint.Enabled = false;

            grpPayment.Controls.Add(lblGrossTotal);
            grpPayment.Controls.Add(lblAdvancesTotal);
            grpPayment.Controls.Add(lblNetTotal);
            grpPayment.Controls.Add(lblMethod);
            grpPayment.Controls.Add(cmbMethod);
            grpPayment.Controls.Add(lblRef);
            grpPayment.Controls.Add(txtRef);
            grpPayment.Controls.Add(btnProcess);
            grpPayment.Controls.Add(btnPrint);
            mainPanel.Controls.Add(grpPayment);

            contentPanel.Controls.Add(mainPanel);

            int currentFarmerId = 0;
            decimal totalDeliveriesAmount = 0;
            decimal totalAdvancesAmount = 0;
            int lastPaymentId = 0;

            btnSearch.Click += (s, e) =>
            {
                string nid = txtSearch.Text.Trim();
                if (string.IsNullOrEmpty(nid))
                {
                    MessageBox.Show("Enter National ID to search");
                    return;
                }

                try
                {
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();

                        SqlCommand cmd = new SqlCommand("SELECT FarmerID, FarmerName, NationalID, Phone FROM Farmers WHERE NationalID = @nid", conn);
                        cmd.Parameters.AddWithValue("@nid", nid);
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (!reader.HasRows)
                        {
                            reader.Close();
                            MessageBox.Show("Farmer not found");
                            return;
                        }

                        reader.Read();
                        currentFarmerId = Convert.ToInt32(reader["FarmerID"]);
                        lblFarmerName.Text = "Name: " + reader["FarmerName"].ToString();
                        lblFarmerNID.Text = "National ID: " + reader["NationalID"].ToString();
                        reader.Close();

                        string deliveriesQuery = @"
                            SELECT d.DeliveryDate, d.WeightKg, d.CoffeeGrade, 
                                   ISNULL((d.WeightKg * cp.PricePerKg), 0) AS Amount
                            FROM Deliveries d
                            LEFT JOIN CoffeePrices cp ON d.CoffeeGrade = cp.CoffeeGrade
                            WHERE d.FarmerID = @fid AND d.PaymentStatus = 'Pending'
                            ORDER BY d.DeliveryDate DESC";

                        SqlDataAdapter deliveryAdapter = new SqlDataAdapter(deliveriesQuery, conn);
                        deliveryAdapter.SelectCommand.Parameters.AddWithValue("@fid", currentFarmerId);
                        DataTable dtDeliveries = new DataTable();
                        deliveryAdapter.Fill(dtDeliveries);

                        dgvDeliveries.Rows.Clear();
                        totalDeliveriesAmount = 0;
                        decimal totalWeight = 0;

                        foreach (DataRow row in dtDeliveries.Rows)
                        {
                            string dateStr = Convert.ToDateTime(row["DeliveryDate"]).ToString("dd/MM/yyyy");
                            decimal weight = Convert.ToDecimal(row["WeightKg"]);
                            string grade = row["CoffeeGrade"].ToString();
                            decimal amount = Convert.ToDecimal(row["Amount"]);
                            dgvDeliveries.Rows.Add(dateStr, weight + " KG", grade, "KES " + amount.ToString("N2"));
                            totalDeliveriesAmount += amount;
                            totalWeight += weight;
                        }

                        lblTotalDeliveries.Text = "Total Deliveries: " + totalWeight.ToString("N2") + " KG";
                        lblTotalEarnings.Text = "Total Earnings: KES " + totalDeliveriesAmount.ToString("N2");
                        lblGrossTotal.Text = "Total Deliveries: KES " + totalDeliveriesAmount.ToString("N2");

                        string advancesQuery = @"
                            SELECT DateTaken, ProductType, Quantity, TotalAmount
                            FROM FarmerAdvances 
                            WHERE FarmerID = @fid AND DeductionStatus = 'Pending'
                            ORDER BY DateTaken DESC";

                        SqlDataAdapter advanceAdapter = new SqlDataAdapter(advancesQuery, conn);
                        advanceAdapter.SelectCommand.Parameters.AddWithValue("@fid", currentFarmerId);
                        DataTable dtAdvances = new DataTable();
                        advanceAdapter.Fill(dtAdvances);

                        dgvAdvances.Rows.Clear();
                        totalAdvancesAmount = 0;
                        foreach (DataRow row in dtAdvances.Rows)
                        {
                            string dateStr = Convert.ToDateTime(row["DateTaken"]).ToString("dd/MM/yyyy");
                            decimal amount = Convert.ToDecimal(row["TotalAmount"]);
                            dgvAdvances.Rows.Add(dateStr, row["ProductType"], row["Quantity"], "KES " + amount.ToString("N2"));
                            totalAdvancesAmount += amount;
                        }

                        lblTotalAdvances.Text = "Total Advances: KES " + totalAdvancesAmount.ToString("N2");
                        lblAdvancesTotal.Text = "Total Advances: KES " + totalAdvancesAmount.ToString("N2");

                        decimal netPayable = totalDeliveriesAmount - totalAdvancesAmount;
                        if (netPayable < 0) netPayable = 0;
                        lblNetPayable.Text = "Net Payable: KES " + netPayable.ToString("N2");
                        lblNetTotal.Text = "Net Payment: KES " + netPayable.ToString("N2");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            };

            btnProcess.Click += (s, e) =>
            {
                if (currentFarmerId == 0)
                {
                    MessageBox.Show("Please search for a farmer first");
                    return;
                }

                if (cmbMethod.SelectedIndex == -1)
                {
                    MessageBox.Show("Please select payment method");
                    return;
                }

                if (totalDeliveriesAmount == 0)
                {
                    MessageBox.Show("No pending deliveries to process");
                    return;
                }

                decimal netPayment = totalDeliveriesAmount - totalAdvancesAmount;
                if (netPayment < 0) netPayment = 0;

                DialogResult confirm = MessageBox.Show("Process payment for this farmer?\n\nTotal Deliveries: KES " + totalDeliveriesAmount.ToString("N2") + "\nTotal Advances: KES " + totalAdvancesAmount.ToString("N2") + "\nNet Payment: KES " + netPayment.ToString("N2"), "Confirm Payment", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirm == DialogResult.Yes)
                {
                    try
                    {
                        using (SqlConnection conn = new SqlConnection(connStr))
                        {
                            conn.Open();
                            SqlTransaction transaction = conn.BeginTransaction();

                            try
                            {
                                SqlCommand updateDeliveries = new SqlCommand("UPDATE Deliveries SET PaymentStatus = 'Paid' WHERE FarmerID = @fid AND PaymentStatus = 'Pending'", conn, transaction);
                                updateDeliveries.Parameters.AddWithValue("@fid", currentFarmerId);
                                updateDeliveries.ExecuteNonQuery();

                                SqlCommand updateAdvances = new SqlCommand("UPDATE FarmerAdvances SET DeductionStatus = 'Deducted' WHERE FarmerID = @fid AND DeductionStatus = 'Pending'", conn, transaction);
                                updateAdvances.Parameters.AddWithValue("@fid", currentFarmerId);
                                updateAdvances.ExecuteNonQuery();

                                string insertPayment = "INSERT INTO Payments (FarmerID, PaymentDate, TotalAmount, AdvanceDeductions, TotalPaid, PaymentMethod, TransactionRef, ProcessedBy, PaymentStatus) VALUES (@fid, @date, @gross, @adv, @net, @method, @ref, @proc, 'Processed'); SELECT SCOPE_IDENTITY();";
                                SqlCommand cmdPayment = new SqlCommand(insertPayment, conn, transaction);
                                cmdPayment.Parameters.AddWithValue("@fid", currentFarmerId);
                                cmdPayment.Parameters.AddWithValue("@date", DateTime.Now);
                                cmdPayment.Parameters.AddWithValue("@gross", totalDeliveriesAmount);
                                cmdPayment.Parameters.AddWithValue("@adv", totalAdvancesAmount);
                                cmdPayment.Parameters.AddWithValue("@net", netPayment);
                                cmdPayment.Parameters.AddWithValue("@method", cmbMethod.SelectedItem.ToString());
                                cmdPayment.Parameters.AddWithValue("@ref", txtRef.Text.Trim());
                                cmdPayment.Parameters.AddWithValue("@proc", loggedInUserId);
                                lastPaymentId = Convert.ToInt32(cmdPayment.ExecuteScalar());

                                transaction.Commit();

                                MessageBox.Show("Payment processed successfully!\nNet Payment: KES " + netPayment.ToString("N2"), "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                GeneratePaymentReceipt(currentFarmerId, lblFarmerName.Text.Replace("Name: ", ""), totalDeliveriesAmount, totalAdvancesAmount, netPayment, cmbMethod.SelectedItem.ToString(), txtRef.Text.Trim(), lastPaymentId);

                                btnPrint.Enabled = true;

                                txtSearch.Clear();
                                dgvDeliveries.Rows.Clear();
                                dgvAdvances.Rows.Clear();
                                lblFarmerName.Text = "Name: --";
                                lblFarmerNID.Text = "National ID: --";
                                lblTotalDeliveries.Text = "Total Deliveries: 0 KG";
                                lblTotalEarnings.Text = "Total Earnings: KES 0";
                                lblTotalAdvances.Text = "Total Advances: KES 0";
                                lblNetPayable.Text = "Net Payable: KES 0";
                                lblGrossTotal.Text = "Total Deliveries: KES 0.00";
                                lblAdvancesTotal.Text = "Total Advances: KES 0.00";
                                lblNetTotal.Text = "Net Payment: KES 0.00";
                                totalDeliveriesAmount = 0;
                                totalAdvancesAmount = 0;
                                currentFarmerId = 0;
                                LoadDashboard();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                MessageBox.Show("Error processing payment: " + ex.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            };

            btnPrint.Click += (s, e) =>
            {
                if (lastPaymentId > 0 && currentFarmerId > 0)
                {
                    GeneratePaymentReceipt(currentFarmerId, lblFarmerName.Text.Replace("Name: ", ""), totalDeliveriesAmount, totalAdvancesAmount, (totalDeliveriesAmount - totalAdvancesAmount), cmbMethod.SelectedItem.ToString(), txtRef.Text.Trim(), lastPaymentId);
                }
            };
        }

        private void LoadPaymentHistory()
        {
            contentPanel.Controls.Clear();

            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.AutoScroll = true;
            mainPanel.BackColor = Color.FromArgb(240, 242, 245);

            GroupBox grp = new GroupBox();
            grp.Text = " PAYMENT HISTORY ";
            grp.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            grp.Location = new Point(10, 10);
            grp.Size = new Size(contentPanel.Width - 40, 750);
            grp.BackColor = Color.White;
            grp.Padding = new Padding(15);

            // Search Panel
            Panel searchPanel = new Panel();
            searchPanel.Location = new Point(10, 25);
            searchPanel.Size = new Size(grp.Width - 40, 80);

            Label lblFilter = new Label();
            lblFilter.Text = "National ID:";
            lblFilter.Location = new Point(5, 15);
            lblFilter.Size = new Size(130, 20);
            lblFilter.Font = new Font("Segoe UI", 11);

            TextBox txtFilter = new TextBox();
            txtFilter.Name = "txtFilter";
            txtFilter.Location = new Point(140, 10);
            txtFilter.Width = 200;
            txtFilter.Height = 35;
            txtFilter.Font = new Font("Segoe UI", 12);

            Label lblFromDate = new Label();
            lblFromDate.Text = "From Date:";
            lblFromDate.Location = new Point(380, 15);
            lblFromDate.AutoSize = true;
            lblFromDate.Font = new Font("Segoe UI", 11);

            DateTimePicker dtpFrom = new DateTimePicker();
            dtpFrom.Name = "dtpFrom";
            dtpFrom.Location = new Point(460, 10);
            dtpFrom.Size = new Size(140, 30);
            dtpFrom.Format = DateTimePickerFormat.Short;
            dtpFrom.Font = new Font("Segoe UI", 11);
            dtpFrom.Value = DateTime.Now.AddMonths(-1);

            Label lblToDate = new Label();
            lblToDate.Text = "To Date:";
            lblToDate.Location = new Point(620, 15);
            lblToDate.AutoSize = true;
            lblToDate.Font = new Font("Segoe UI", 11);

            DateTimePicker dtpTo = new DateTimePicker();
            dtpTo.Name = "dtpTo";
            dtpTo.Location = new Point(690, 10);
            dtpTo.Size = new Size(140, 30);
            dtpTo.Format = DateTimePickerFormat.Short;
            dtpTo.Font = new Font("Segoe UI", 11);
            dtpTo.Value = DateTime.Now;

            Button btnFilter = new Button();
            btnFilter.Text = "SEARCH";
            btnFilter.Location = new Point(850, 8);
            btnFilter.Size = new Size(100, 35);
            btnFilter.BackColor = Color.FromArgb(52, 152, 219);
            btnFilter.ForeColor = Color.White;
            btnFilter.FlatStyle = FlatStyle.Flat;
            btnFilter.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnFilter.Cursor = Cursors.Hand;

            Button btnClear = new Button();
            btnClear.Text = "CLEAR";
            btnClear.Location = new Point(960, 8);
            btnClear.Size = new Size(100, 35);
            btnClear.BackColor = Color.FromArgb(155, 89, 182);
            btnClear.ForeColor = Color.White;
            btnClear.FlatStyle = FlatStyle.Flat;
            btnClear.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnClear.Cursor = Cursors.Hand;

            searchPanel.Controls.Add(lblFilter);
            searchPanel.Controls.Add(txtFilter);
            searchPanel.Controls.Add(lblFromDate);
            searchPanel.Controls.Add(dtpFrom);
            searchPanel.Controls.Add(lblToDate);
            searchPanel.Controls.Add(dtpTo);
            searchPanel.Controls.Add(btnFilter);
            searchPanel.Controls.Add(btnClear);

            Label lblFarmerInfo = new Label();
            lblFarmerInfo.Name = "lblFarmerInfo";
            lblFarmerInfo.Text = "Enter search criteria and click SEARCH";
            lblFarmerInfo.Location = new Point(15, 115);
            lblFarmerInfo.AutoSize = true;
            lblFarmerInfo.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblFarmerInfo.ForeColor = Color.FromArgb(46, 125, 50);

            // DataGridView
            DataGridView dgv = new DataGridView();
            dgv.Name = "dgvPaymentHistory";
            dgv.Location = new Point(15, 145);
            dgv.Size = new Size(grp.Width - 50, 420);
            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.BackgroundColor = Color.White;
            dgv.Font = new Font("Segoe UI", 11);
            dgv.RowTemplate.Height = 35;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;

            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 40;

            // Summary Panel
            Panel summaryPanel = new Panel();
            summaryPanel.Location = new Point(15, 580);
            summaryPanel.Size = new Size(grp.Width - 50, 100);
            summaryPanel.BackColor = Color.FromArgb(248, 249, 250);
            summaryPanel.BorderStyle = BorderStyle.FixedSingle;

            Label lblSummaryTitle = new Label();
            lblSummaryTitle.Text = "SUMMARY";
            lblSummaryTitle.Location = new Point(15, 10);
            lblSummaryTitle.AutoSize = true;
            lblSummaryTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblSummaryTitle.ForeColor = Color.FromArgb(44, 62, 80);

            Label lblTotalGross = new Label();
            lblTotalGross.Name = "lblTotalGross";
            lblTotalGross.Text = "Total Gross: KES 0.00";
            lblTotalGross.Location = new Point(15, 40);
            lblTotalGross.AutoSize = true;
            lblTotalGross.Font = new Font("Segoe UI", 11);

            Label lblTotalAdvances = new Label();
            lblTotalAdvances.Name = "lblTotalAdvances";
            lblTotalAdvances.Text = "Total Advances: KES 0.00";
            lblTotalAdvances.Location = new Point(250, 40);
            lblTotalAdvances.AutoSize = true;
            lblTotalAdvances.Font = new Font("Segoe UI", 11);

            Label lblTotalNet = new Label();
            lblTotalNet.Name = "lblTotalNet";
            lblTotalNet.Text = "Total Net Paid: KES 0.00";
            lblTotalNet.Location = new Point(500, 40);
            lblTotalNet.AutoSize = true;
            lblTotalNet.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblTotalNet.ForeColor = Color.FromArgb(46, 125, 50);

            summaryPanel.Controls.Add(lblSummaryTitle);
            summaryPanel.Controls.Add(lblTotalGross);
            summaryPanel.Controls.Add(lblTotalAdvances);
            summaryPanel.Controls.Add(lblTotalNet);

            // Buttons Panel
            Panel buttonPanel = new Panel();
            buttonPanel.Location = new Point(15, 695);
            buttonPanel.Size = new Size(grp.Width - 50, 45);

            Button btnPrintReceipt = new Button();
            btnPrintReceipt.Text = "Print Selected Receipt";
            btnPrintReceipt.Location = new Point(0, 5);
            btnPrintReceipt.Size = new Size(160, 40);
            btnPrintReceipt.BackColor = Color.FromArgb(155, 89, 182);
            btnPrintReceipt.ForeColor = Color.White;
            btnPrintReceipt.FlatStyle = FlatStyle.Flat;
            btnPrintReceipt.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnPrintReceipt.Cursor = Cursors.Hand;
            btnPrintReceipt.Enabled = false;

            Button btnPrintSummary = new Button();
            btnPrintSummary.Text = "Print Summary Report";
            btnPrintSummary.Location = new Point(175, 5);
            btnPrintSummary.Size = new Size(160, 40);
            btnPrintSummary.BackColor = Color.FromArgb(52, 152, 219);
            btnPrintSummary.ForeColor = Color.White;
            btnPrintSummary.FlatStyle = FlatStyle.Flat;
            btnPrintSummary.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnPrintSummary.Cursor = Cursors.Hand;

            buttonPanel.Controls.Add(btnPrintReceipt);
            buttonPanel.Controls.Add(btnPrintSummary);

            grp.Controls.Add(searchPanel);
            grp.Controls.Add(lblFarmerInfo);
            grp.Controls.Add(dgv);
            grp.Controls.Add(summaryPanel);
            grp.Controls.Add(buttonPanel);
            mainPanel.Controls.Add(grp);
            contentPanel.Controls.Add(mainPanel);

            int currentFarmerId = 0;
            string currentFarmerName = "";

            void UpdateSummaryTotals()
            {
                decimal totalGross = 0;
                decimal totalAdvances = 0;
                decimal totalNet = 0;

                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.Cells["Gross"].Value != null && row.Cells["Gross"].Value != DBNull.Value)
                        totalGross += Convert.ToDecimal(row.Cells["Gross"].Value);
                    if (row.Cells["Advances"].Value != null && row.Cells["Advances"].Value != DBNull.Value)
                        totalAdvances += Convert.ToDecimal(row.Cells["Advances"].Value);
                    if (row.Cells["Net Paid"].Value != null && row.Cells["Net Paid"].Value != DBNull.Value)
                        totalNet += Convert.ToDecimal(row.Cells["Net Paid"].Value);
                }

                lblTotalGross.Text = "Total Gross: KES " + totalGross.ToString("N2");
                lblTotalAdvances.Text = "Total Advances: KES " + totalAdvances.ToString("N2");
                lblTotalNet.Text = "Total Net Paid: KES " + totalNet.ToString("N2");
            }

            void LoadPaymentData(string nationalId, DateTime fromDate, DateTime toDate)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();

                        dgv.Columns.Clear();

                        DataTable dt = new DataTable();

                        if (!string.IsNullOrEmpty(nationalId))
                        {
                            // Get farmer ID and name first
                            SqlCommand farmerCmd = new SqlCommand("SELECT FarmerID, FarmerName FROM Farmers WHERE NationalID = @nid", conn);
                            farmerCmd.Parameters.AddWithValue("@nid", nationalId);
                            SqlDataReader farmerReader = farmerCmd.ExecuteReader();
                            if (farmerReader.Read())
                            {
                                currentFarmerId = Convert.ToInt32(farmerReader["FarmerID"]);
                                currentFarmerName = farmerReader["FarmerName"].ToString();
                                farmerReader.Close();

                                string query = @"
                                    SELECT 
                                        CONVERT(VARCHAR, p.PaymentDate, 103) AS Date,
                                        ISNULL(p.TotalAmount, 0) AS Gross,
                                        ISNULL(p.AdvanceDeductions, 0) AS Advances,
                                        ISNULL(p.TotalPaid, 0) AS [Net Paid],
                                        p.PaymentID,
                                        p.PaymentMethod,
                                        p.TransactionRef
                                    FROM Payments p
                                    WHERE p.FarmerID = @fid
                                        AND CAST(p.PaymentDate AS DATE) BETWEEN @fromDate AND @toDate
                                    ORDER BY p.PaymentDate DESC";

                                SqlCommand cmd = new SqlCommand(query, conn);
                                cmd.Parameters.AddWithValue("@fid", currentFarmerId);
                                cmd.Parameters.AddWithValue("@fromDate", fromDate);
                                cmd.Parameters.AddWithValue("@toDate", toDate);

                                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                                adapter.Fill(dt);
                                dgv.DataSource = dt;

                                if (dgv.Columns.Contains("PaymentID")) dgv.Columns["PaymentID"].Visible = false;
                                if (dgv.Columns.Contains("PaymentMethod")) dgv.Columns["PaymentMethod"].Visible = false;
                                if (dgv.Columns.Contains("TransactionRef")) dgv.Columns["TransactionRef"].Visible = false;

                                dgv.Columns["Date"].HeaderText = "Date";
                                dgv.Columns["Gross"].HeaderText = "Gross (KES)";
                                dgv.Columns["Advances"].HeaderText = "Advances (KES)";
                                dgv.Columns["Net Paid"].HeaderText = "Net Paid (KES)";

                                dgv.Columns["Gross"].DefaultCellStyle.Format = "N2";
                                dgv.Columns["Advances"].DefaultCellStyle.Format = "N2";
                                dgv.Columns["Net Paid"].DefaultCellStyle.Format = "N2";

                                if (dt.Rows.Count > 0)
                                {
                                    lblFarmerInfo.Text = "Farmer: " + currentFarmerName + " | Period: " + fromDate.ToString("dd/MM/yyyy") + " - " + toDate.ToString("dd/MM/yyyy") + " | Records: " + dt.Rows.Count;
                                }
                                else
                                {
                                    lblFarmerInfo.Text = "No payment records found for this farmer in the selected period.";
                                }
                            }
                            else
                            {
                                farmerReader.Close();
                                MessageBox.Show("Farmer not found with this National ID");
                                lblFarmerInfo.Text = "Farmer not found. Enter a valid National ID.";
                                dgv.DataSource = null;
                                currentFarmerId = 0;
                                currentFarmerName = "";
                            }
                        }
                        else
                        {
                            currentFarmerId = 0;
                            currentFarmerName = "";

                            string query = @"
                                SELECT 
                                    CONVERT(VARCHAR, p.PaymentDate, 103) AS Date,
                                    ISNULL(f.FarmerName, 'Unknown') AS Farmer,
                                    ISNULL(p.TotalAmount, 0) AS Gross,
                                    ISNULL(p.AdvanceDeductions, 0) AS Advances,
                                    ISNULL(p.TotalPaid, 0) AS [Net Paid],
                                    p.PaymentID,
                                    p.FarmerID,
                                    p.PaymentMethod,
                                    p.TransactionRef
                                FROM Payments p
                                LEFT JOIN Farmers f ON p.FarmerID = f.FarmerID
                                WHERE CAST(p.PaymentDate AS DATE) BETWEEN @fromDate AND @toDate
                                ORDER BY p.PaymentDate DESC";

                            SqlCommand cmd = new SqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@fromDate", fromDate);
                            cmd.Parameters.AddWithValue("@toDate", toDate);

                            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                            adapter.Fill(dt);
                            dgv.DataSource = dt;

                            if (dgv.Columns.Contains("PaymentID")) dgv.Columns["PaymentID"].Visible = false;
                            if (dgv.Columns.Contains("FarmerID")) dgv.Columns["FarmerID"].Visible = false;
                            if (dgv.Columns.Contains("PaymentMethod")) dgv.Columns["PaymentMethod"].Visible = false;
                            if (dgv.Columns.Contains("TransactionRef")) dgv.Columns["TransactionRef"].Visible = false;

                            dgv.Columns["Date"].HeaderText = "Date";
                            dgv.Columns["Farmer"].HeaderText = "Farmer";
                            dgv.Columns["Gross"].HeaderText = "Gross (KES)";
                            dgv.Columns["Advances"].HeaderText = "Advances (KES)";
                            dgv.Columns["Net Paid"].HeaderText = "Net Paid (KES)";

                            dgv.Columns["Gross"].DefaultCellStyle.Format = "N2";
                            dgv.Columns["Advances"].DefaultCellStyle.Format = "N2";
                            dgv.Columns["Net Paid"].DefaultCellStyle.Format = "N2";

                            if (dt.Rows.Count > 0)
                            {
                                lblFarmerInfo.Text = "All Farmers | Period: " + fromDate.ToString("dd/MM/yyyy") + " - " + toDate.ToString("dd/MM/yyyy") + " | Records: " + dt.Rows.Count;
                            }
                            else
                            {
                                lblFarmerInfo.Text = "No payment records found for the selected period.";
                            }
                        }

                        btnPrintReceipt.Enabled = dt.Rows.Count > 0;
                        UpdateSummaryTotals();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading payment history: " + ex.Message);
                }
            }

            void PrintSelectedReceipt()
            {
                if (dgv.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select a payment to print receipt");
                    return;
                }

                try
                {
                    DataGridViewRow row = dgv.SelectedRows[0];

                    int paymentId = 0;
                    int farmerId = 0;
                    string farmerName = "";
                    decimal gross = 0;
                    decimal advances = 0;
                    decimal net = 0;
                    string method = "";
                    string reference = "";

                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();

                        // Get PaymentID from hidden column or use the row data
                        if (dgv.Columns.Contains("PaymentID") && row.Cells["PaymentID"].Value != null && row.Cells["PaymentID"].Value != DBNull.Value)
                        {
                            paymentId = Convert.ToInt32(row.Cells["PaymentID"].Value);
                        }
                        else
                        {
                            // Try to get from database using date and farmer
                            string dateStr = row.Cells["Date"].Value.ToString();
                            string farmerNameForQuery = currentFarmerName;

                            if (string.IsNullOrEmpty(farmerNameForQuery) && dgv.Columns.Contains("Farmer"))
                            {
                                farmerNameForQuery = row.Cells["Farmer"].Value.ToString();
                            }

                            SqlCommand getIdCmd = new SqlCommand(@"
                                SELECT TOP 1 p.PaymentID 
                                FROM Payments p
                                JOIN Farmers f ON p.FarmerID = f.FarmerID
                                WHERE CONVERT(VARCHAR, p.PaymentDate, 103) = @date
                                AND f.FarmerName = @name", conn);
                            getIdCmd.Parameters.AddWithValue("@date", dateStr);
                            getIdCmd.Parameters.AddWithValue("@name", farmerNameForQuery);
                            object result = getIdCmd.ExecuteScalar();
                            if (result != null)
                                paymentId = Convert.ToInt32(result);
                        }

                        if (paymentId > 0)
                        {
                            SqlCommand getDetailsCmd = new SqlCommand(@"
                                SELECT p.FarmerID, f.FarmerName, p.TotalAmount, p.AdvanceDeductions, p.TotalPaid, p.PaymentMethod, p.TransactionRef
                                FROM Payments p
                                JOIN Farmers f ON p.FarmerID = f.FarmerID
                                WHERE p.PaymentID = @pid", conn);
                            getDetailsCmd.Parameters.AddWithValue("@pid", paymentId);
                            SqlDataReader reader = getDetailsCmd.ExecuteReader();
                            if (reader.Read())
                            {
                                farmerId = Convert.ToInt32(reader["FarmerID"]);
                                farmerName = reader["FarmerName"].ToString();
                                gross = Convert.ToDecimal(reader["TotalAmount"]);
                                advances = Convert.ToDecimal(reader["AdvanceDeductions"]);
                                net = Convert.ToDecimal(reader["TotalPaid"]);
                                method = reader["PaymentMethod"]?.ToString() ?? "N/A";
                                reference = reader["TransactionRef"]?.ToString() ?? "";
                            }
                            reader.Close();
                        }
                    }

                    if (farmerId > 0 && paymentId > 0)
                    {
                        GeneratePaymentReceipt(farmerId, farmerName, gross, advances, net, method, reference, paymentId);
                    }
                    else
                    {
                        // Fallback: use data from grid
                        gross = Convert.ToDecimal(row.Cells["Gross"].Value);
                        advances = Convert.ToDecimal(row.Cells["Advances"].Value);
                        net = Convert.ToDecimal(row.Cells["Net Paid"].Value);
                        farmerName = currentFarmerName;
                        if (string.IsNullOrEmpty(farmerName) && dgv.Columns.Contains("Farmer"))
                            farmerName = row.Cells["Farmer"].Value.ToString();

                        GeneratePaymentReceipt(currentFarmerId > 0 ? currentFarmerId : 1, farmerName, gross, advances, net, "N/A", "", paymentId);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error printing receipt: " + ex.Message);
                }
            }

            void PrintSummaryReport()
            {
                if (dgv.Rows.Count == 0)
                {
                    MessageBox.Show("No payment records to print");
                    return;
                }

                try
                {
                    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string folder = Path.Combine(documentsPath, "CSMS_Reports");
                    Directory.CreateDirectory(folder);
                    string fileName = "Payment_Summary_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf";
                    string filePath = Path.Combine(folder, fileName);

                    Document doc = new Document(PageSize.A4.Rotate());
                    PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                    doc.Open();

                    iTextSharp.text.Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                    iTextSharp.text.Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                    iTextSharp.text.Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                    doc.Add(new Paragraph("SUBUKIA COFFEE SOCIETY", titleFont) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(new Paragraph("PAYMENT HISTORY REPORT", headerFont) { Alignment = Element.ALIGN_CENTER });

                    string title = string.IsNullOrEmpty(txtFilter.Text) ? "All Farmers" : "Farmer: " + currentFarmerName;
                    doc.Add(new Paragraph(title, normalFont));
                    doc.Add(new Paragraph("Period: " + dtpFrom.Value.ToString("dd/MM/yyyy") + " - " + dtpTo.Value.ToString("dd/MM/yyyy"), normalFont));
                    doc.Add(new Paragraph("Generated: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"), normalFont));
                    doc.Add(new Paragraph("\n"));

                    decimal totalGross = 0;
                    decimal totalAdvances = 0;
                    decimal totalNet = 0;

                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        totalGross += Convert.ToDecimal(row.Cells["Gross"].Value);
                        totalAdvances += Convert.ToDecimal(row.Cells["Advances"].Value);
                        totalNet += Convert.ToDecimal(row.Cells["Net Paid"].Value);
                    }

                    PdfPTable summaryTable = new PdfPTable(2);
                    summaryTable.WidthPercentage = 50;
                    summaryTable.HorizontalAlignment = Element.ALIGN_CENTER;

                    AddSummaryCell(summaryTable, "Total Gross Paid:", "KES " + totalGross.ToString("N2"), headerFont);
                    AddSummaryCell(summaryTable, "Total Advances Deducted:", "KES " + totalAdvances.ToString("N2"), headerFont);
                    AddSummaryCell(summaryTable, "Total Net Paid:", "KES " + totalNet.ToString("N2"), headerFont);

                    doc.Add(summaryTable);
                    doc.Add(new Paragraph("\n"));

                    bool isSpecificFarmer = !string.IsNullOrEmpty(txtFilter.Text);
                    int columnCount = isSpecificFarmer ? 4 : 5;
                    PdfPTable table = new PdfPTable(columnCount);
                    table.WidthPercentage = 100;

                    if (isSpecificFarmer)
                    {
                        table.SetWidths(new float[] { 20f, 25f, 25f, 30f });
                        string[] headers = { "Date", "Gross (KES)", "Advances (KES)", "Net Paid (KES)" };
                        foreach (string h in headers)
                        {
                            PdfPCell cell = new PdfPCell(new Phrase(h, headerFont));
                            cell.BackgroundColor = new BaseColor(52, 73, 94);
                            table.AddCell(cell);
                        }

                        foreach (DataGridViewRow row in dgv.Rows)
                        {
                            table.AddCell(row.Cells["Date"].Value.ToString());
                            table.AddCell(Convert.ToDecimal(row.Cells["Gross"].Value).ToString("N2"));
                            table.AddCell(Convert.ToDecimal(row.Cells["Advances"].Value).ToString("N2"));
                            table.AddCell(Convert.ToDecimal(row.Cells["Net Paid"].Value).ToString("N2"));
                        }
                    }
                    else
                    {
                        table.SetWidths(new float[] { 15f, 20f, 20f, 20f, 25f });
                        string[] headers = { "Date", "Farmer", "Gross (KES)", "Advances (KES)", "Net Paid (KES)" };
                        foreach (string h in headers)
                        {
                            PdfPCell cell = new PdfPCell(new Phrase(h, headerFont));
                            cell.BackgroundColor = new BaseColor(52, 73, 94);
                            table.AddCell(cell);
                        }

                        foreach (DataGridViewRow row in dgv.Rows)
                        {
                            table.AddCell(row.Cells["Date"].Value.ToString());
                            table.AddCell(row.Cells["Farmer"].Value.ToString());
                            table.AddCell(Convert.ToDecimal(row.Cells["Gross"].Value).ToString("N2"));
                            table.AddCell(Convert.ToDecimal(row.Cells["Advances"].Value).ToString("N2"));
                            table.AddCell(Convert.ToDecimal(row.Cells["Net Paid"].Value).ToString("N2"));
                        }
                    }

                    doc.Add(table);
                    doc.Add(new Paragraph("\nThank you!", normalFont) { Alignment = Element.ALIGN_CENTER });
                    doc.Close();

                    MessageBox.Show("Report saved to:\n" + filePath, "Report Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = filePath, UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error generating report: " + ex.Message);
                }
            }

            void AddSummaryCell(PdfPTable table, string label, string value, iTextSharp.text.Font font)
            {
                PdfPCell labelCell = new PdfPCell(new Phrase(label, font));
                labelCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                labelCell.Padding = 5;
                table.AddCell(labelCell);

                PdfPCell valueCell = new PdfPCell(new Phrase(value, font));
                valueCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                valueCell.Padding = 5;
                table.AddCell(valueCell);
            }

            // Event handlers
            btnFilter.Click += (s, e) =>
            {
                LoadPaymentData(txtFilter.Text.Trim(), dtpFrom.Value.Date, dtpTo.Value.Date);
            };

            btnClear.Click += (s, e) =>
            {
                txtFilter.Clear();
                dtpFrom.Value = DateTime.Now.AddMonths(-1);
                dtpTo.Value = DateTime.Now;
                LoadPaymentData("", dtpFrom.Value.Date, dtpTo.Value.Date);
            };

            btnPrintReceipt.Click += (s, e) => PrintSelectedReceipt();
            btnPrintSummary.Click += (s, e) => PrintSummaryReport();

            dgv.SelectionChanged += (s, e) =>
            {
                btnPrintReceipt.Enabled = dgv.SelectedRows.Count > 0;
            };

            // Load initial data (last 30 days)
            LoadPaymentData("", dtpFrom.Value.Date, dtpTo.Value.Date);
        }

        // Helper Methods
        private void ApplyRoundedCorners(Panel panel, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(panel.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(panel.Width - radius, panel.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, panel.Height - radius, radius, radius, 90, 90);
            panel.Region = new Region(path);
        }

        private Panel CreateStatCard(string title, string value, Color color)
        {
            Panel card = new Panel();
            card.Size = new Size(230, 100);
            card.BackColor = Color.White;
            card.Margin = new Padding(0, 0, 20, 0);
            card.Paint += (s, e) => ApplyRoundedCorners(card, 10);

            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.Font = new Font("Segoe UI", 11);
            lblTitle.ForeColor = Color.Gray;
            lblTitle.Location = new Point(15, 20);
            lblTitle.AutoSize = true;

            Label lblValue = new Label();
            lblValue.Text = value;
            lblValue.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            lblValue.ForeColor = color;
            lblValue.Location = new Point(15, 55);
            lblValue.AutoSize = true;

            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);
            return card;
        }

        private DashboardStats GetDashboardStats()
        {
            DashboardStats stats = new DashboardStats();
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Farmers", conn);
                    stats.TotalFarmers = (int)cmd.ExecuteScalar();

                    cmd = new SqlCommand("SELECT COUNT(*) FROM Deliveries WHERE PaymentStatus = 'Pending'", conn);
                    stats.PendingDeliveries = (int)cmd.ExecuteScalar();

                    cmd = new SqlCommand("SELECT COUNT(*) FROM FarmerAdvances WHERE DeductionStatus = 'Pending'", conn);
                    stats.PendingAdvances = (int)cmd.ExecuteScalar();

                    cmd = new SqlCommand("SELECT ISNULL(SUM(TotalPaid), 0) FROM Payments", conn);
                    stats.TotalPaymentsMade = Convert.ToDecimal(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading stats: " + ex.Message);
                stats.TotalFarmers = 0;
                stats.PendingDeliveries = 0;
                stats.PendingAdvances = 0;
                stats.TotalPaymentsMade = 0;
            }
            return stats;
        }

        private void LoadRecentPayments(ListView lv)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT TOP 10 p.PaymentDate, f.FarmerName, p.TotalAmount, p.AdvanceDeductions, p.TotalPaid FROM Payments p JOIN Farmers f ON p.FarmerID = f.FarmerID ORDER BY p.PaymentDate DESC";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        ListViewItem item = new ListViewItem(Convert.ToDateTime(reader["PaymentDate"]).ToString("dd/MM/yyyy"));
                        item.SubItems.Add(reader["FarmerName"].ToString());
                        item.SubItems.Add("KES " + Convert.ToDecimal(reader["TotalAmount"]).ToString("N2"));
                        item.SubItems.Add("KES " + Convert.ToDecimal(reader["AdvanceDeductions"]).ToString("N2"));
                        item.SubItems.Add("KES " + Convert.ToDecimal(reader["TotalPaid"]).ToString("N2"));
                        lv.Items.Add(item);
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading recent payments: " + ex.Message);
            }
        }

        private void GeneratePaymentReceipt(int farmerId, string farmerName, decimal gross, decimal advances, decimal net, string method, string reference, int paymentId)
        {
            try
            {
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string folder = Path.Combine(documentsPath, "CSMS_Receipts");
                Directory.CreateDirectory(folder);
                string filePath = Path.Combine(folder, "Payment_Receipt_" + paymentId + "_" + farmerName.Replace(" ", "_") + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf");

                Document doc = new Document(PageSize.A5);
                PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                doc.Open();

                iTextSharp.text.Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                iTextSharp.text.Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                iTextSharp.text.Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                string nationalId = "";
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT NationalID FROM Farmers WHERE FarmerID = @id", conn);
                    cmd.Parameters.AddWithValue("@id", farmerId);
                    object result = cmd.ExecuteScalar();
                    if (result != null) nationalId = result.ToString();
                }

                doc.Add(new Paragraph("SUBUKIA COFFEE SOCIETY", titleFont) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph("PAYMENT RECEIPT", headerFont) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph("=================================", normalFont));
                doc.Add(new Paragraph("Receipt No: PAY-" + paymentId, normalFont));
                doc.Add(new Paragraph("Date: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"), normalFont));
                doc.Add(new Paragraph("---------------------------------", normalFont));
                doc.Add(new Paragraph("Farmer: " + farmerName, normalFont));
                doc.Add(new Paragraph("National ID: " + nationalId, normalFont));
                doc.Add(new Paragraph("---------------------------------", normalFont));
                doc.Add(new Paragraph("Gross Amount: KES " + gross.ToString("N2"), normalFont));
                doc.Add(new Paragraph("Advances Deducted: KES " + advances.ToString("N2"), normalFont));
                doc.Add(new Paragraph("---------------------------------", normalFont));
                doc.Add(new Paragraph("NET PAYMENT: KES " + net.ToString("N2"), headerFont));
                doc.Add(new Paragraph("---------------------------------", normalFont));
                doc.Add(new Paragraph("Payment Method: " + method, normalFont));
                if (!string.IsNullOrEmpty(reference))
                    doc.Add(new Paragraph("Transaction Ref: " + reference, normalFont));
                doc.Add(new Paragraph("Processed By: " + loggedInName, normalFont));
                doc.Add(new Paragraph("=================================", normalFont));
                doc.Add(new Paragraph("Thank you!", normalFont) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph("This is a system generated receipt", normalFont) { Alignment = Element.ALIGN_CENTER });

                doc.Close();

                MessageBox.Show("Receipt saved to:\n" + filePath, "Receipt Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = filePath, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating receipt: " + ex.Message);
            }
        }

        private class DashboardStats
        {
            public int TotalFarmers { get; set; }
            public int PendingDeliveries { get; set; }
            public int PendingAdvances { get; set; }
            public decimal TotalPaymentsMade { get; set; }
        }

        private void frmAccountantDashboard_Load(object sender, EventArgs e)
        {

        }
    }
}