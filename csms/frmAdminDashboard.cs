using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Font = System.Drawing.Font;

namespace csms
{
    public partial class frmAdminDashboard : Form
    {
        private string connStr = @"Server=DESKTOP-NLHET7V\SQLEXPRESS;Database=CSMS;Trusted_Connection=True;";
        private string loggedInName;
        private int loggedInUserId;
        private Panel contentPanel;

        public frmAdminDashboard(int userId, string name)
        {
            InitializeComponent();
            loggedInUserId = userId;
            loggedInName = name;
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.Text = "Admin Dashboard";
            CreateDashboard();
        }

        private void CreateDashboard()
        {
            // Title Bar
            Panel titleBar = new Panel();
            titleBar.Height = 50;
            titleBar.Dock = DockStyle.Top;
            titleBar.BackColor = Color.FromArgb(44, 62, 80);

            Label lblTitle = new Label();
            lblTitle.Text = "ADMIN DASHBOARD";
            lblTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(20, 15);
            lblTitle.AutoSize = true;

            Label lblUser = new Label();
            lblUser.Text = "Welcome, " + loggedInName;
            lblUser.Font = new Font("Segoe UI", 10);
            lblUser.ForeColor = Color.FromArgb(200, 200, 200);
            lblUser.Location = new Point(this.Width - 180, 15);
            lblUser.AutoSize = true;

            Button btnMinimize = new Button();
            btnMinimize.Text = "─";
            btnMinimize.Size = new Size(45, 45);
            btnMinimize.Location = new Point(this.Width - 135, 2);
            btnMinimize.BackColor = Color.Transparent;
            btnMinimize.ForeColor = Color.White;
            btnMinimize.FlatStyle = FlatStyle.Flat;
            btnMinimize.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            btnMinimize.Cursor = Cursors.Hand;
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            Button btnMaximize = new Button();
            btnMaximize.Text = "□";
            btnMaximize.Size = new Size(45, 45);
            btnMaximize.Location = new Point(this.Width - 90, 2);
            btnMaximize.BackColor = Color.Transparent;
            btnMaximize.ForeColor = Color.White;
            btnMaximize.FlatStyle = FlatStyle.Flat;
            btnMaximize.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            btnMaximize.Cursor = Cursors.Hand;
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

            Button btnClose = new Button();
            btnClose.Text = "X";
            btnClose.Size = new Size(45, 45);
            btnClose.Location = new Point(this.Width - 45, 2);
            btnClose.BackColor = Color.Transparent;
            btnClose.ForeColor = Color.White;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            btnClose.Cursor = Cursors.Hand;
            btnClose.Click += (s, e) => Application.Exit();

            titleBar.Controls.Add(lblTitle);
            titleBar.Controls.Add(lblUser);
            titleBar.Controls.Add(btnMinimize);
            titleBar.Controls.Add(btnMaximize);
            titleBar.Controls.Add(btnClose);
            this.Controls.Add(titleBar);

            // Sidebar
            Panel sidebar = new Panel();
            sidebar.Width = 240;
            sidebar.Dock = DockStyle.Left;
            sidebar.BackColor = Color.FromArgb(52, 73, 94);

            Label lblLogo = new Label();
            lblLogo.Text = "CSMS";
            lblLogo.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            lblLogo.ForeColor = Color.White;
            lblLogo.Location = new Point(85, 30);
            lblLogo.AutoSize = true;
            sidebar.Controls.Add(lblLogo);

            string[] menuItems = { "Dashboard", "Coffee Prices", "Announcements", "Reports", "Insights", "Logout" };
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

            Label lblVersion = new Label();
            lblVersion.Text = "Version 2.0";
            lblVersion.Font = new Font("Segoe UI", 9);
            lblVersion.ForeColor = Color.FromArgb(150, 150, 150);
            lblVersion.Location = new Point(85, this.Height - 60);
            lblVersion.AutoSize = true;
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
            else if (menu == "Coffee Prices") LoadCoffeePrices();
            else if (menu == "Announcements") LoadAnnouncements();
            else if (menu == "Reports") LoadReports();
            else if (menu == "Insights") LoadInsights();
            else if (menu == "Logout")
            {
                Logout();
            }
        }

        private void Logout()
        {
            this.Close();
            Form1 welcome = new Form1();
            welcome.Show();
        }

        private void LoadInsights()
        {
            contentPanel.Controls.Clear();

            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.AutoScroll = true;

            GroupBox grp = new GroupBox();
            grp.Text = " INSIGHTS & ALERTS ";
            grp.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            grp.Location = new Point(10, 10);
            grp.Size = new Size(contentPanel.Width - 40, 600);
            grp.BackColor = Color.White;
            grp.Padding = new Padding(20);

            int yPos = 30;

            var insights = GetInsightsData();

            // CRITICAL ALERTS
            Label lblCritical = new Label();
            lblCritical.Text = "CRITICAL ALERTS";
            lblCritical.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblCritical.ForeColor = Color.FromArgb(231, 76, 60);
            lblCritical.Location = new Point(20, yPos);
            lblCritical.AutoSize = true;
            grp.Controls.Add(lblCritical);
            yPos += 30;

            foreach (string alert in insights.CriticalAlerts)
            {
                Label lblAlert = new Label();
                lblAlert.Text = "- " + alert;
                lblAlert.Font = new Font("Segoe UI", 11);
                lblAlert.ForeColor = Color.FromArgb(231, 76, 60);
                lblAlert.Location = new Point(35, yPos);
                lblAlert.AutoSize = true;
                grp.Controls.Add(lblAlert);
                yPos += 28;
            }
            yPos += 15;

            // WARNINGS
            Label lblWarnings = new Label();
            lblWarnings.Text = "WARNINGS";
            lblWarnings.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblWarnings.ForeColor = Color.FromArgb(241, 196, 15);
            lblWarnings.Location = new Point(20, yPos);
            lblWarnings.AutoSize = true;
            grp.Controls.Add(lblWarnings);
            yPos += 30;

            foreach (string warning in insights.Warnings)
            {
                Label lblWarning = new Label();
                lblWarning.Text = "- " + warning;
                lblWarning.Font = new Font("Segoe UI", 11);
                lblWarning.ForeColor = Color.FromArgb(241, 196, 15);
                lblWarning.Location = new Point(35, yPos);
                lblWarning.AutoSize = true;
                grp.Controls.Add(lblWarning);
                yPos += 28;
            }
            yPos += 15;

            // RECOMMENDATIONS
            Label lblRecommendations = new Label();
            lblRecommendations.Text = "RECOMMENDATIONS";
            lblRecommendations.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblRecommendations.ForeColor = Color.FromArgb(46, 204, 113);
            lblRecommendations.Location = new Point(20, yPos);
            lblRecommendations.AutoSize = true;
            grp.Controls.Add(lblRecommendations);
            yPos += 30;

            foreach (string rec in insights.Recommendations)
            {
                Label lblRec = new Label();
                lblRec.Text = "- " + rec;
                lblRec.Font = new Font("Segoe UI", 11);
                lblRec.ForeColor = Color.FromArgb(46, 204, 113);
                lblRec.Location = new Point(35, yPos);
                lblRec.AutoSize = true;
                grp.Controls.Add(lblRec);
                yPos += 28;
            }
            yPos += 15;

            // TRENDS
            Label lblTrends = new Label();
            lblTrends.Text = "TRENDS";
            lblTrends.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblTrends.ForeColor = Color.FromArgb(52, 152, 219);
            lblTrends.Location = new Point(20, yPos);
            lblTrends.AutoSize = true;
            grp.Controls.Add(lblTrends);
            yPos += 30;

            foreach (string trend in insights.Trends)
            {
                Label lblTrend = new Label();
                lblTrend.Text = "- " + trend;
                lblTrend.Font = new Font("Segoe UI", 11);
                lblTrend.ForeColor = Color.FromArgb(52, 152, 219);
                lblTrend.Location = new Point(35, yPos);
                lblTrend.AutoSize = true;
                grp.Controls.Add(lblTrend);
                yPos += 28;
            }

            mainPanel.Controls.Add(grp);
            contentPanel.Controls.Add(mainPanel);
        }

        private InsightsData GetInsightsData()
        {
            InsightsData data = new InsightsData();
            data.CriticalAlerts = new List<string>();
            data.Warnings = new List<string>();
            data.Recommendations = new List<string>();
            data.Trends = new List<string>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string diseaseQuery = @"
                        SELECT COUNT(*) as Count
                        FROM FarmerAdvances 
                        WHERE (ProductType LIKE '%fungicide%' OR ProductType LIKE '%pesticide%')
                        AND DateTaken >= DATEADD(day, -30, GETDATE())";

                    SqlCommand cmd = new SqlCommand(diseaseQuery, conn);
                    int count = (int)cmd.ExecuteScalar();

                    if (count > 20)
                    {
                        data.CriticalAlerts.Add("Fungicide/Pesticide requests: " + count + " in last 30 days. Possible disease outbreak.");
                    }

                    string stockQuery = @"
                        SELECT TOP 1 ProductType, COUNT(*) as UsageCount
                        FROM FarmerAdvances 
                        WHERE DateTaken >= DATEADD(month, -1, GETDATE())
                        GROUP BY ProductType
                        ORDER BY UsageCount DESC";

                    cmd = new SqlCommand(stockQuery, conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        string topProduct = reader["ProductType"].ToString();
                        int usageCount = Convert.ToInt32(reader["UsageCount"]);
                        data.Warnings.Add("'" + topProduct + "' is the most requested product (" + usageCount + " units last month). Monitor stock levels.");
                    }
                    reader.Close();

                    string farmerQuery = @"
                        SELECT TOP 3 f.FarmerName
                        FROM Farmers f
                        LEFT JOIN FarmerAdvances a ON f.FarmerID = a.FarmerID
                        LEFT JOIN Deliveries d ON f.FarmerID = d.FarmerID
                        GROUP BY f.FarmerName
                        HAVING COUNT(a.AdvanceID) > 0 AND COUNT(d.DeliveryID) = 0
                        ORDER BY COUNT(a.AdvanceID) DESC";

                    cmd = new SqlCommand(farmerQuery, conn);
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        string farmerName = reader["FarmerName"].ToString();
                        data.Warnings.Add("Farmer " + farmerName + " has taken advances but no deliveries recorded. Follow-up recommended.");
                    }
                    reader.Close();

                    int currentMonth = DateTime.Now.Month;
                    if (currentMonth >= 3 && currentMonth <= 5)
                    {
                        data.Recommendations.Add("Rainy season detected. Remind farmers about fungal disease prevention and proper drainage.");
                    }
                    else if (currentMonth >= 6 && currentMonth <= 8)
                    {
                        data.Recommendations.Add("Dry season detected. Encourage farmers to mulch and conserve water.");
                    }
                    else if (currentMonth >= 10 && currentMonth <= 12)
                    {
                        data.Recommendations.Add("Harvest season approaching. Ensure adequate coffee picking baskets are available.");
                    }
                    else
                    {
                        data.Recommendations.Add("Regular farm maintenance. Monitor for early signs of disease.");
                    }

                    data.Recommendations.Add("Regular farm visits help identify issues early. Schedule monthly farm inspections.");

                    string trendQuery = @"
                        SELECT TOP 3 ProductType, COUNT(*) as TotalCount
                        FROM FarmerAdvances
                        GROUP BY ProductType
                        ORDER BY TotalCount DESC";

                    cmd = new SqlCommand(trendQuery, conn);
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        string product = reader["ProductType"].ToString();
                        int total = Convert.ToInt32(reader["TotalCount"]);
                        data.Trends.Add(product + " is the most requested advance (" + total + " units all time).");
                    }
                    reader.Close();

                    string topFarmerQuery = @"
                        SELECT TOP 1 f.FarmerName, COUNT(d.DeliveryID) as DeliveryCount
                        FROM Deliveries d
                        JOIN Farmers f ON d.FarmerID = f.FarmerID
                        GROUP BY f.FarmerName
                        ORDER BY DeliveryCount DESC";

                    cmd = new SqlCommand(topFarmerQuery, conn);
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        string farmerName = reader["FarmerName"].ToString();
                        int deliveryCount = Convert.ToInt32(reader["DeliveryCount"]);
                        data.Trends.Add("Most active farmer: " + farmerName + " (" + deliveryCount + " deliveries)");
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                data.CriticalAlerts.Add("Unable to load insights: " + ex.Message);
            }

            if (data.CriticalAlerts.Count == 0)
            {
                data.CriticalAlerts.Add("No critical alerts at this time.");
            }
            if (data.Warnings.Count == 0)
            {
                data.Warnings.Add("No warnings at this time.");
            }
            if (data.Recommendations.Count == 0)
            {
                data.Recommendations.Add("Continue regular monitoring of farm activities.");
            }
            if (data.Trends.Count == 0)
            {
                data.Trends.Add("Collect more data to see trends.");
            }

            return data;
        }

        private class InsightsData
        {
            public List<string> CriticalAlerts { get; set; }
            public List<string> Warnings { get; set; }
            public List<string> Recommendations { get; set; }
            public List<string> Trends { get; set; }
        }

        private void LoadDashboard()
        {
            contentPanel.Controls.Clear();

            Panel welcomeCard = new Panel();
            welcomeCard.Width = contentPanel.Width - 40;
            welcomeCard.Height = 80;
            welcomeCard.BackColor = Color.White;
            welcomeCard.Paint += (s, e) => ApplyRoundedCorners(welcomeCard, 15);

            Label lblWelcome = new Label();
            lblWelcome.Text = "Welcome back, " + loggedInName + "!";
            lblWelcome.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            lblWelcome.ForeColor = Color.FromArgb(44, 62, 80);
            lblWelcome.Location = new Point(20, 20);
            lblWelcome.AutoSize = true;
            welcomeCard.Controls.Add(lblWelcome);
            contentPanel.Controls.Add(welcomeCard);

            FlowLayoutPanel statsPanel = new FlowLayoutPanel();
            statsPanel.Location = new Point(0, 100);
            statsPanel.Width = contentPanel.Width - 40;
            statsPanel.Height = 120;

            var stats = GetDashboardStats();

            statsPanel.Controls.Add(CreateStatCard("Total Farmers", stats.TotalFarmers.ToString(), Color.FromArgb(52, 152, 219)));
            statsPanel.Controls.Add(CreateStatCard("Total Deliveries", stats.TotalDeliveries.ToString(), Color.FromArgb(46, 204, 113)));
            statsPanel.Controls.Add(CreateStatCard("Total Announcements", stats.TotalAnnouncements.ToString(), Color.FromArgb(241, 196, 15)));
            statsPanel.Controls.Add(CreateStatCard("Total Payments", "KES " + stats.TotalPayments.ToString("N0"), Color.FromArgb(155, 89, 182)));

            contentPanel.Controls.Add(statsPanel);

            GroupBox grpRecent = new GroupBox();
            grpRecent.Text = " Recent Announcements ";
            grpRecent.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            grpRecent.Location = new Point(0, 240);
            grpRecent.Size = new Size(contentPanel.Width - 40, 250);
            grpRecent.BackColor = Color.White;

            ListView lvRecent = new ListView();
            lvRecent.View = View.Details;
            lvRecent.FullRowSelect = true;
            lvRecent.GridLines = true;
            lvRecent.Dock = DockStyle.Fill;
            lvRecent.Font = new Font("Segoe UI", 10);
            lvRecent.BackColor = Color.White;
            lvRecent.Columns.Add("Date", 120);
            lvRecent.Columns.Add("Title", 200);
            lvRecent.Columns.Add("Message", 450);

            LoadRecentAnnouncements(lvRecent);

            grpRecent.Controls.Add(lvRecent);
            contentPanel.Controls.Add(grpRecent);
        }

        private void LoadCoffeePrices()
        {
            contentPanel.Controls.Clear();

            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.AutoScroll = true;

            GroupBox grp = new GroupBox();
            grp.Text = " COFFEE PRICE MANAGEMENT ";
            grp.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            grp.Location = new Point(10, 10);
            grp.Size = new Size(contentPanel.Width - 40, 450);
            grp.BackColor = Color.White;
            grp.Padding = new Padding(20);

            Button btnAdd = new Button();
            btnAdd.Text = "+ Set New Price";
            btnAdd.Size = new Size(160, 45);
            btnAdd.Location = new Point(20, 20);
            btnAdd.BackColor = Color.FromArgb(52, 152, 219);
            btnAdd.ForeColor = Color.White;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnAdd.Cursor = Cursors.Hand;
            btnAdd.Click += (s, e) => ShowAddPriceDialog();

            Label lblCurrent = new Label();
            lblCurrent.Text = "Current Coffee Prices (Latest)";
            lblCurrent.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblCurrent.ForeColor = Color.FromArgb(44, 62, 80);
            lblCurrent.Location = new Point(20, 85);
            lblCurrent.AutoSize = true;

            DataGridView dgv = new DataGridView();
            dgv.Location = new Point(20, 120);
            dgv.Size = new Size(grp.Width - 60, 270);
            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.BackgroundColor = Color.White;
            dgv.Font = new Font("Segoe UI", 11);
            dgv.RowTemplate.Height = 40;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);

            dgv.Columns.Add("CoffeeGrade", "Grade");
            dgv.Columns.Add("PricePerKg", "Price (KES)");
            dgv.Columns.Add("EffectiveDate", "Effective Date");
            dgv.Columns.Add("SetBy", "Set By");

            RefreshCoffeePrices(dgv);

            grp.Controls.Add(btnAdd);
            grp.Controls.Add(lblCurrent);
            grp.Controls.Add(dgv);
            mainPanel.Controls.Add(grp);
            contentPanel.Controls.Add(mainPanel);
        }

        private void RefreshCoffeePrices(DataGridView dgv)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    dgv.Rows.Clear();

                    string query = @"
                        WITH LatestPrices AS (
                            SELECT 
                                CoffeeGrade,
                                PricePerKg,
                                EffectiveDate,
                                SetBy,
                                ROW_NUMBER() OVER (PARTITION BY CoffeeGrade ORDER BY EffectiveDate DESC) as rn
                            FROM CoffeePrices
                        )
                        SELECT CoffeeGrade, PricePerKg, EffectiveDate, SetBy
                        FROM LatestPrices
                        WHERE rn = 1
                        ORDER BY CoffeeGrade";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string grade = reader["CoffeeGrade"].ToString();
                        decimal price = Convert.ToDecimal(reader["PricePerKg"]);
                        string priceText = "KES " + price.ToString("N2");
                        string date = Convert.ToDateTime(reader["EffectiveDate"]).ToString("dd/MM/yyyy");
                        string setBy = reader["SetBy"].ToString();

                        dgv.Rows.Add(grade, priceText, date, setBy);
                    }
                    reader.Close();

                    string[] allGrades = { "A01", "A02", "B" };
                    foreach (string grade in allGrades)
                    {
                        bool found = false;
                        foreach (DataGridViewRow row in dgv.Rows)
                        {
                            if (row.Cells[0].Value.ToString() == grade)
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            dgv.Rows.Add(grade, "Not set", "N/A", "N/A");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading coffee prices: " + ex.Message);
            }
        }

        private void ShowAddPriceDialog()
        {
            Form dialog = new Form();
            dialog.Text = "Set Coffee Price";
            dialog.Size = new Size(500, 420);
            dialog.StartPosition = FormStartPosition.CenterParent;
            dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            dialog.MaximizeBox = false;
            dialog.BackColor = Color.White;

            Label lblGrade = new Label();
            lblGrade.Text = "Coffee Grade:";
            lblGrade.Location = new Point(30, 30);
            lblGrade.AutoSize = true;
            lblGrade.Font = new Font("Segoe UI", 11, FontStyle.Bold);

            ComboBox cmbGrade = new ComboBox();
            cmbGrade.Location = new Point(30, 60);
            cmbGrade.Width = 420;
            cmbGrade.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGrade.Font = new Font("Segoe UI", 11);
            cmbGrade.Items.AddRange(new string[] { "A01", "A02", "B" });

            Label lblPrice = new Label();
            lblPrice.Text = "Price Per Kg (KES):";
            lblPrice.Location = new Point(30, 110);
            lblPrice.AutoSize = true;
            lblPrice.Font = new Font("Segoe UI", 11, FontStyle.Bold);

            TextBox txtPrice = new TextBox();
            txtPrice.Location = new Point(30, 140);
            txtPrice.Width = 420;
            txtPrice.Font = new Font("Segoe UI", 11);

            Label lblDate = new Label();
            lblDate.Text = "Effective Date:";
            lblDate.Location = new Point(30, 190);
            lblDate.AutoSize = true;
            lblDate.Font = new Font("Segoe UI", 11, FontStyle.Bold);

            DateTimePicker dtpDate = new DateTimePicker();
            dtpDate.Location = new Point(30, 220);
            dtpDate.Width = 420;
            dtpDate.Format = DateTimePickerFormat.Short;
            dtpDate.Font = new Font("Segoe UI", 11);
            dtpDate.Value = DateTime.Now;

            Label lblInfo = new Label();
            lblInfo.Text = "Note: This will become the new current price for this grade.";
            lblInfo.Location = new Point(30, 270);
            lblInfo.AutoSize = true;
            lblInfo.Font = new Font("Segoe UI", 10);
            lblInfo.ForeColor = Color.Gray;

            Panel buttonPanel = new Panel();
            buttonPanel.Location = new Point(0, 320);
            buttonPanel.Size = new Size(500, 60);
            buttonPanel.BackColor = Color.FromArgb(248, 249, 250);

            Button btnSave = new Button();
            btnSave.Text = "SET PRICE";
            btnSave.Size = new Size(140, 45);
            btnSave.Location = new Point(170, 8);
            btnSave.BackColor = Color.FromArgb(46, 204, 113);
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnSave.Cursor = Cursors.Hand;

            Button btnCancel = new Button();
            btnCancel.Text = "CANCEL";
            btnCancel.Size = new Size(140, 45);
            btnCancel.Location = new Point(320, 8);
            btnCancel.BackColor = Color.FromArgb(231, 76, 60);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.Click += (s, e) => dialog.Close();

            btnSave.Click += (ev, ea) =>
            {
                if (cmbGrade.SelectedIndex == -1)
                {
                    MessageBox.Show("Please select a coffee grade.");
                    return;
                }
                if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
                {
                    MessageBox.Show("Please enter a valid price.");
                    return;
                }

                try
                {
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("INSERT INTO CoffeePrices (CoffeeGrade, PricePerKg, EffectiveDate, SetBy) VALUES (@g, @p, @d, @s)", conn);
                        cmd.Parameters.AddWithValue("@g", cmbGrade.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@p", price);
                        cmd.Parameters.AddWithValue("@d", dtpDate.Value);
                        cmd.Parameters.AddWithValue("@s", loggedInName);
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Price set successfully!");
                        dialog.Close();
                        LoadCoffeePrices();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            };

            buttonPanel.Controls.Add(btnSave);
            buttonPanel.Controls.Add(btnCancel);

            dialog.Controls.Add(lblGrade);
            dialog.Controls.Add(cmbGrade);
            dialog.Controls.Add(lblPrice);
            dialog.Controls.Add(txtPrice);
            dialog.Controls.Add(lblDate);
            dialog.Controls.Add(dtpDate);
            dialog.Controls.Add(lblInfo);
            dialog.Controls.Add(buttonPanel);
            dialog.ShowDialog();
        }

        private void LoadAnnouncements()
        {
            contentPanel.Controls.Clear();

            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.AutoScroll = true;

            GroupBox grp = new GroupBox();
            grp.Text = " ANNOUNCEMENTS MANAGEMENT ";
            grp.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            grp.Location = new Point(10, 10);
            grp.Size = new Size(contentPanel.Width - 40, 500);
            grp.BackColor = Color.White;
            grp.Padding = new Padding(20);

            Button btnAdd = new Button();
            btnAdd.Text = "+ New Announcement";
            btnAdd.Size = new Size(160, 45);
            btnAdd.Location = new Point(20, 20);
            btnAdd.BackColor = Color.FromArgb(46, 204, 113);
            btnAdd.ForeColor = Color.White;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnAdd.Cursor = Cursors.Hand;
            btnAdd.Click += (s, e) => ShowAddAnnouncementDialog();

            Label lblAll = new Label();
            lblAll.Text = "All Announcements";
            lblAll.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblAll.ForeColor = Color.FromArgb(44, 62, 80);
            lblAll.Location = new Point(20, 85);
            lblAll.AutoSize = true;

            DataGridView dgv = new DataGridView();
            dgv.Location = new Point(20, 120);
            dgv.Size = new Size(grp.Width - 60, 330);
            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.BackgroundColor = Color.White;
            dgv.Font = new Font("Segoe UI", 11);
            dgv.RowTemplate.Height = 40;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);

            dgv.Columns.Add("DatePosted", "Date");
            dgv.Columns.Add("Title", "Title");
            dgv.Columns.Add("Message", "Message");
            dgv.Columns.Add("PostedBy", "Posted By");

            LoadAnnouncementsData(dgv);

            grp.Controls.Add(btnAdd);
            grp.Controls.Add(lblAll);
            grp.Controls.Add(dgv);
            mainPanel.Controls.Add(grp);
            contentPanel.Controls.Add(mainPanel);
        }

        private void LoadAnnouncementsData(DataGridView dgv)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT CONVERT(VARCHAR, DatePosted, 103) AS DatePosted, Title, Message, PostedBy FROM Announcements ORDER BY DatePosted DESC";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    dgv.Rows.Clear();

                    while (reader.Read())
                    {
                        string date = reader["DatePosted"].ToString();
                        string title = reader["Title"].ToString();
                        string message = reader["Message"].ToString();
                        string postedBy = reader["PostedBy"] != DBNull.Value ? reader["PostedBy"].ToString() : "Admin";

                        dgv.Rows.Add(date, title, message, postedBy);
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading announcements: " + ex.Message);
            }
        }

        private void ShowAddAnnouncementDialog()
        {
            Form dialog = new Form();
            dialog.Text = "Create New Announcement";
            dialog.Size = new Size(550, 450);
            dialog.StartPosition = FormStartPosition.CenterParent;
            dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            dialog.MaximizeBox = false;
            dialog.BackColor = Color.White;

            Label lblTitle = new Label();
            lblTitle.Text = "Title:";
            lblTitle.Location = new Point(20, 25);
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);

            TextBox txtTitle = new TextBox();
            txtTitle.Location = new Point(20, 55);
            txtTitle.Width = 490;
            txtTitle.Font = new Font("Segoe UI", 11);

            Label lblMessage = new Label();
            lblMessage.Text = "Message:";
            lblMessage.Location = new Point(20, 105);
            lblMessage.AutoSize = true;
            lblMessage.Font = new Font("Segoe UI", 11, FontStyle.Bold);

            TextBox txtMessage = new TextBox();
            txtMessage.Location = new Point(20, 135);
            txtMessage.Width = 490;
            txtMessage.Height = 150;
            txtMessage.Multiline = true;
            txtMessage.Font = new Font("Segoe UI", 11);

            Button btnSave = new Button();
            btnSave.Text = "Publish Announcement";
            btnSave.Location = new Point(20, 310);
            btnSave.Size = new Size(180, 45);
            btnSave.BackColor = Color.FromArgb(46, 204, 113);
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnSave.Cursor = Cursors.Hand;

            btnSave.Click += (ev, ea) =>
            {
                if (string.IsNullOrEmpty(txtTitle.Text) || string.IsNullOrEmpty(txtMessage.Text))
                {
                    MessageBox.Show("Please enter both title and message.");
                    return;
                }
                try
                {
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("INSERT INTO Announcements (Title, Message, DatePosted, PostedBy) VALUES (@t, @m, @d, @p)", conn);
                        cmd.Parameters.AddWithValue("@t", txtTitle.Text);
                        cmd.Parameters.AddWithValue("@m", txtMessage.Text);
                        cmd.Parameters.AddWithValue("@d", DateTime.Now);
                        cmd.Parameters.AddWithValue("@p", loggedInName);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Announcement published successfully!");
                        dialog.Close();
                        LoadAnnouncements();
                        LoadDashboard();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            };

            dialog.Controls.Add(lblTitle);
            dialog.Controls.Add(txtTitle);
            dialog.Controls.Add(lblMessage);
            dialog.Controls.Add(txtMessage);
            dialog.Controls.Add(btnSave);
            dialog.ShowDialog();
        }

        private void LoadReports()
        {
            contentPanel.Controls.Clear();

            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.AutoScroll = true;

            GroupBox grp = new GroupBox();
            grp.Text = " REPORTS ";
            grp.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            grp.Location = new Point(10, 10);
            grp.Size = new Size(contentPanel.Width - 40, 350);
            grp.BackColor = Color.White;
            grp.Padding = new Padding(30);

            int yPos = 30;
            int btnWidth = 220;
            int btnHeight = 50;
            int spacing = 30;

            Button btnPayment = new Button();
            btnPayment.Text = "Payment Reports";
            btnPayment.Location = new Point(30, yPos);
            btnPayment.Size = new Size(btnWidth, btnHeight);
            btnPayment.BackColor = Color.FromArgb(52, 152, 219);
            btnPayment.ForeColor = Color.White;
            btnPayment.FlatStyle = FlatStyle.Flat;
            btnPayment.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnPayment.Cursor = Cursors.Hand;
            btnPayment.Click += (s, e) => ShowPaymentReportDialog();
            grp.Controls.Add(btnPayment);

            Button btnFarmers = new Button();
            btnFarmers.Text = "Farmers Report";
            btnFarmers.Location = new Point(30 + btnWidth + spacing, yPos);
            btnFarmers.Size = new Size(btnWidth, btnHeight);
            btnFarmers.BackColor = Color.FromArgb(46, 204, 113);
            btnFarmers.ForeColor = Color.White;
            btnFarmers.FlatStyle = FlatStyle.Flat;
            btnFarmers.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnFarmers.Cursor = Cursors.Hand;
            btnFarmers.Click += (s, e) => GenerateFarmersReport();
            grp.Controls.Add(btnFarmers);

            yPos += btnHeight + spacing;

            Button btnDeliveries = new Button();
            btnDeliveries.Text = "Deliveries Report";
            btnDeliveries.Location = new Point(30, yPos);
            btnDeliveries.Size = new Size(btnWidth, btnHeight);
            btnDeliveries.BackColor = Color.FromArgb(241, 196, 15);
            btnDeliveries.ForeColor = Color.White;
            btnDeliveries.FlatStyle = FlatStyle.Flat;
            btnDeliveries.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnDeliveries.Cursor = Cursors.Hand;
            btnDeliveries.Click += (s, e) => ShowDeliveryReportDialog();
            grp.Controls.Add(btnDeliveries);

            Button btnAdvances = new Button();
            btnAdvances.Text = "Advances Report";
            btnAdvances.Location = new Point(30 + btnWidth + spacing, yPos);
            btnAdvances.Size = new Size(btnWidth, btnHeight);
            btnAdvances.BackColor = Color.FromArgb(231, 76, 60);
            btnAdvances.ForeColor = Color.White;
            btnAdvances.FlatStyle = FlatStyle.Flat;
            btnAdvances.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnAdvances.Cursor = Cursors.Hand;
            btnAdvances.Click += (s, e) => GenerateAdvancesReport();
            grp.Controls.Add(btnAdvances);

            Label lblInfo = new Label();
            lblInfo.Text = "Click any report button above to generate and view the report.";
            lblInfo.Font = new Font("Segoe UI", 11);
            lblInfo.ForeColor = Color.Gray;
            lblInfo.Location = new Point(30, yPos + btnHeight + 20);
            lblInfo.AutoSize = true;
            grp.Controls.Add(lblInfo);

            mainPanel.Controls.Add(grp);
            contentPanel.Controls.Add(mainPanel);
        }

        private void ShowPaymentReportDialog()
        {
            Form dialog = new Form();
            dialog.Text = "Payment Report";
            dialog.Size = new Size(450, 200);
            dialog.StartPosition = FormStartPosition.CenterParent;
            dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            dialog.MaximizeBox = false;
            dialog.BackColor = Color.White;

            Label lblFrom = new Label();
            lblFrom.Text = "From Date:";
            lblFrom.Location = new Point(30, 30);
            lblFrom.AutoSize = true;
            lblFrom.Font = new Font("Segoe UI", 11);

            DateTimePicker dtpFrom = new DateTimePicker();
            dtpFrom.Location = new Point(130, 27);
            dtpFrom.Size = new Size(160, 30);
            dtpFrom.Format = DateTimePickerFormat.Short;
            dtpFrom.Font = new Font("Segoe UI", 11);
            dtpFrom.Value = DateTime.Now.AddMonths(-1);

            Label lblTo = new Label();
            lblTo.Text = "To Date:";
            lblTo.Location = new Point(30, 70);
            lblTo.AutoSize = true;
            lblTo.Font = new Font("Segoe UI", 11);

            DateTimePicker dtpTo = new DateTimePicker();
            dtpTo.Location = new Point(130, 67);
            dtpTo.Size = new Size(160, 30);
            dtpTo.Format = DateTimePickerFormat.Short;
            dtpTo.Font = new Font("Segoe UI", 11);
            dtpTo.Value = DateTime.Now;

            Button btnGenerate = new Button();
            btnGenerate.Text = "Generate Report";
            btnGenerate.Location = new Point(130, 120);
            btnGenerate.Size = new Size(160, 40);
            btnGenerate.BackColor = Color.FromArgb(52, 152, 219);
            btnGenerate.ForeColor = Color.White;
            btnGenerate.FlatStyle = FlatStyle.Flat;
            btnGenerate.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnGenerate.Cursor = Cursors.Hand;

            btnGenerate.Click += (s, e) =>
            {
                GeneratePaymentReport(dtpFrom.Value, dtpTo.Value);
                dialog.Close();
            };

            dialog.Controls.Add(lblFrom);
            dialog.Controls.Add(dtpFrom);
            dialog.Controls.Add(lblTo);
            dialog.Controls.Add(dtpTo);
            dialog.Controls.Add(btnGenerate);
            dialog.ShowDialog();
        }

        private void ShowDeliveryReportDialog()
        {
            Form dialog = new Form();
            dialog.Text = "Deliveries Report";
            dialog.Size = new Size(450, 250);
            dialog.StartPosition = FormStartPosition.CenterParent;
            dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            dialog.MaximizeBox = false;
            dialog.BackColor = Color.White;

            Label lblFrom = new Label();
            lblFrom.Text = "From Date:";
            lblFrom.Location = new Point(30, 30);
            lblFrom.AutoSize = true;
            lblFrom.Font = new Font("Segoe UI", 11);

            DateTimePicker dtpFrom = new DateTimePicker();
            dtpFrom.Location = new Point(130, 27);
            dtpFrom.Size = new Size(160, 30);
            dtpFrom.Format = DateTimePickerFormat.Short;
            dtpFrom.Font = new Font("Segoe UI", 11);
            dtpFrom.Value = DateTime.Now.AddMonths(-1);

            Label lblTo = new Label();
            lblTo.Text = "To Date:";
            lblTo.Location = new Point(30, 70);
            lblTo.AutoSize = true;
            lblTo.Font = new Font("Segoe UI", 11);

            DateTimePicker dtpTo = new DateTimePicker();
            dtpTo.Location = new Point(130, 67);
            dtpTo.Size = new Size(160, 30);
            dtpTo.Format = DateTimePickerFormat.Short;
            dtpTo.Font = new Font("Segoe UI", 11);
            dtpTo.Value = DateTime.Now;

            Label lblFarmer = new Label();
            lblFarmer.Text = "Farmer National ID (optional):";
            lblFarmer.Location = new Point(30, 110);
            lblFarmer.AutoSize = true;
            lblFarmer.Font = new Font("Segoe UI", 11);

            TextBox txtFarmer = new TextBox();
            txtFarmer.Location = new Point(230, 107);
            txtFarmer.Width = 180;
            txtFarmer.Font = new Font("Segoe UI", 11);

            Button btnGenerate = new Button();
            btnGenerate.Text = "Generate Report";
            btnGenerate.Location = new Point(130, 160);
            btnGenerate.Size = new Size(160, 40);
            btnGenerate.BackColor = Color.FromArgb(52, 152, 219);
            btnGenerate.ForeColor = Color.White;
            btnGenerate.FlatStyle = FlatStyle.Flat;
            btnGenerate.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnGenerate.Cursor = Cursors.Hand;

            btnGenerate.Click += (s, e) =>
            {
                GenerateDeliveriesReport(dtpFrom.Value, dtpTo.Value, txtFarmer.Text.Trim());
                dialog.Close();
            };

            dialog.Controls.Add(lblFrom);
            dialog.Controls.Add(dtpFrom);
            dialog.Controls.Add(lblTo);
            dialog.Controls.Add(dtpTo);
            dialog.Controls.Add(lblFarmer);
            dialog.Controls.Add(txtFarmer);
            dialog.Controls.Add(btnGenerate);
            dialog.ShowDialog();
        }

        private void GeneratePaymentReport(DateTime fromDate, DateTime toDate)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            f.FarmerName,
                            ISNULL(p.TotalAmount, 0) AS Gross,
                            ISNULL(p.AdvanceDeductions, 0) AS Deductions,
                            ISNULL(p.TotalPaid, 0) AS [Net Paid],
                            CONVERT(VARCHAR, p.PaymentDate, 103) AS PaymentDate
                        FROM Payments p
                        JOIN Farmers f ON p.FarmerID = f.FarmerID
                        WHERE CAST(p.PaymentDate AS DATE) BETWEEN @fromDate AND @toDate
                        ORDER BY p.PaymentDate DESC";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@fromDate", fromDate);
                    cmd.Parameters.AddWithValue("@toDate", toDate);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("No payment records found for the selected period.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    ShowReport("Payment Report", dt, "Period: " + fromDate.ToString("dd/MM/yyyy") + " - " + toDate.ToString("dd/MM/yyyy"));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void GenerateFarmersReport()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            FarmerID,
                            FarmerName,
                            NationalID,
                            Phone,
                            FarmLocation,
                            CONVERT(VARCHAR, DateRegistered, 103) AS DateRegistered
                        FROM Farmers
                        ORDER BY FarmerName";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("No farmers found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    ShowReport("Farmers Report", dt, "Complete list of registered farmers");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void GenerateDeliveriesReport(DateTime fromDate, DateTime toDate, string nationalId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            f.FarmerName,
                            COUNT(d.DeliveryID) AS TotalDeliveries
                        FROM Farmers f
                        LEFT JOIN Deliveries d ON f.FarmerID = d.FarmerID
                        WHERE CAST(d.DeliveryDate AS DATE) BETWEEN @fromDate AND @toDate";

                    if (!string.IsNullOrEmpty(nationalId))
                    {
                        query += " AND f.NationalID = @nid";
                    }

                    query += @" GROUP BY f.FarmerName
                                HAVING COUNT(d.DeliveryID) > 0
                                ORDER BY TotalDeliveries DESC";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@fromDate", fromDate);
                    cmd.Parameters.AddWithValue("@toDate", toDate);

                    if (!string.IsNullOrEmpty(nationalId))
                    {
                        cmd.Parameters.AddWithValue("@nid", nationalId);
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("No delivery records found for the selected criteria.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    string title = string.IsNullOrEmpty(nationalId) ? "All Farmers" : "Farmer ID: " + nationalId;
                    ShowReport("Deliveries Report", dt, "Period: " + fromDate.ToString("dd/MM/yyyy") + " - " + toDate.ToString("dd/MM/yyyy") + " | " + title);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void GenerateAdvancesReport()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            f.FarmerName,
                            COUNT(a.AdvanceID) AS NumberOfAdvances,
                            ISNULL(SUM(a.TotalAmount), 0) AS TotalAmount,
                            MAX(CONVERT(VARCHAR, a.DateTaken, 103)) AS LastAdvanceDate
                        FROM Farmers f
                        LEFT JOIN FarmerAdvances a ON f.FarmerID = a.FarmerID
                        GROUP BY f.FarmerName
                        HAVING COUNT(a.AdvanceID) > 0
                        ORDER BY TotalAmount DESC";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("No advance records found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    ShowReport("Advances Report", dt, "Summary of advances by farmer");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void ShowReport(string title, DataTable data, string subtitle)
        {
            Form f = new Form();
            f.Text = title;
            f.Size = new Size(1000, 600);
            f.StartPosition = FormStartPosition.CenterParent;
            f.BackColor = Color.White;

            DataGridView dgv = new DataGridView();
            dgv.Location = new Point(12, 12);
            dgv.Size = new Size(f.ClientSize.Width - 24, f.ClientSize.Height - 80);
            dgv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            dgv.DataSource = data;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgv.BackgroundColor = Color.White;
            dgv.Font = new Font("Segoe UI", 10);
            dgv.RowTemplate.Height = 35;

            Button btnPrint = new Button();
            btnPrint.Text = "Print PDF";
            btnPrint.Size = new Size(130, 40);
            btnPrint.Location = new Point(f.ClientSize.Width - 280, f.ClientSize.Height - 55);
            btnPrint.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnPrint.BackColor = Color.FromArgb(46, 204, 113);
            btnPrint.ForeColor = Color.White;
            btnPrint.FlatStyle = FlatStyle.Flat;
            btnPrint.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnPrint.Cursor = Cursors.Hand;
            btnPrint.Click += (s, e) => PrintReportToPDF(title, data, subtitle);

            Button btnClose = new Button();
            btnClose.Text = "Close";
            btnClose.Size = new Size(130, 40);
            btnClose.Location = new Point(f.ClientSize.Width - 140, f.ClientSize.Height - 55);
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.BackColor = Color.FromArgb(231, 76, 60);
            btnClose.ForeColor = Color.White;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnClose.Cursor = Cursors.Hand;
            btnClose.Click += (s, e) => f.Close();

            f.Controls.Add(dgv);
            f.Controls.Add(btnPrint);
            f.Controls.Add(btnClose);

            f.Shown += (s, e) =>
            {
                btnPrint.Location = new Point(f.ClientSize.Width - 280, f.ClientSize.Height - 55);
                btnClose.Location = new Point(f.ClientSize.Width - 140, f.ClientSize.Height - 55);
                dgv.Size = new Size(f.ClientSize.Width - 24, f.ClientSize.Height - 80);
            };

            f.Resize += (s, e) =>
            {
                btnPrint.Location = new Point(f.ClientSize.Width - 280, f.ClientSize.Height - 55);
                btnClose.Location = new Point(f.ClientSize.Width - 140, f.ClientSize.Height - 55);
                dgv.Size = new Size(f.ClientSize.Width - 24, f.ClientSize.Height - 80);
            };

            f.ShowDialog();
        }

        private void PrintReportToPDF(string title, DataTable data, string subtitle)
        {
            try
            {
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string folder = Path.Combine(documentsPath, "CSMS_Reports");
                Directory.CreateDirectory(folder);
                string fileName = title.Replace(" ", "_") + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf";
                string filePath = Path.Combine(folder, fileName);

                Document doc = new Document(PageSize.A4.Rotate());
                PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                doc.Open();

                iTextSharp.text.Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                iTextSharp.text.Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                iTextSharp.text.Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                doc.Add(new Paragraph("SUBUKIA COFFEE SOCIETY", titleFont) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph(title, headerFont) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph(subtitle, normalFont) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph("Generated: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"), normalFont) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph("\n"));

                PdfPTable table = new PdfPTable(data.Columns.Count);
                table.WidthPercentage = 100;

                foreach (DataColumn col in data.Columns)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(col.ColumnName, headerFont));
                    cell.BackgroundColor = new BaseColor(52, 73, 94);
                    table.AddCell(cell);
                }

                foreach (DataRow row in data.Rows)
                {
                    foreach (DataColumn col in data.Columns)
                    {
                        string value = row[col].ToString();
                        if (col.DataType == typeof(decimal) || col.ColumnName.Contains("Amount") || col.ColumnName.Contains("Gross") || col.ColumnName.Contains("Paid"))
                        {
                            if (decimal.TryParse(value, out decimal decValue))
                            {
                                value = "KES " + decValue.ToString("N2");
                            }
                        }
                        table.AddCell(value);
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
                MessageBox.Show("Error generating PDF: " + ex.Message);
            }
        }

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

                    cmd = new SqlCommand("SELECT COUNT(*) FROM Deliveries", conn);
                    stats.TotalDeliveries = (int)cmd.ExecuteScalar();

                    cmd = new SqlCommand("SELECT COUNT(*) FROM Announcements", conn);
                    stats.TotalAnnouncements = (int)cmd.ExecuteScalar();

                    cmd = new SqlCommand("SELECT ISNULL(SUM(TotalPaid), 0) FROM Payments", conn);
                    stats.TotalPayments = Convert.ToDecimal(cmd.ExecuteScalar());
                }
            }
            catch
            {
                stats.TotalFarmers = 0;
                stats.TotalDeliveries = 0;
                stats.TotalAnnouncements = 0;
                stats.TotalPayments = 0;
            }
            return stats;
        }

        private void LoadRecentAnnouncements(ListView lv)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT TOP 5 Title, Message, DatePosted FROM Announcements ORDER BY DatePosted DESC", conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        ListViewItem item = new ListViewItem(Convert.ToDateTime(reader["DatePosted"]).ToString("dd/MM/yyyy"));
                        item.SubItems.Add(reader["Title"].ToString());
                        string msg = reader["Message"].ToString();
                        if (msg.Length > 70) msg = msg.Substring(0, 70) + "...";
                        item.SubItems.Add(msg);
                        lv.Items.Add(item);
                    }
                    reader.Close();
                }
            }
            catch { }
        }

        private class DashboardStats
        {
            public int TotalFarmers { get; set; }
            public int TotalDeliveries { get; set; }
            public int TotalAnnouncements { get; set; }
            public decimal TotalPayments { get; set; }
        }

        private void frmAdminDashboard_Load(object sender, EventArgs e)
        {

        }
    }
}