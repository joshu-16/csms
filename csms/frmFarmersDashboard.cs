using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using Font = System.Drawing.Font;

namespace csms
{
    public partial class frmFarmersDashboard : Form
    {
        private string connStr = @"Server=DESKTOP-NLHET7V\SQLEXPRESS;Database=CSMS;Trusted_Connection=True;";
        private string loggedInFarmerName;
        private int currentFarmerId;
        private Panel announcementButtonPanel;
        private Button btnAnnouncements;
        private Label badge;
        private Label lblDateTime;
        private int lastAnnouncementCount = 0;
        private DataGridView dgvDeliveries;
        private DataGridView dgvAdvances;

        public frmFarmersDashboard(string FarmerName)
        {
            InitializeComponent();
            loggedInFarmerName = FarmerName;
            currentFarmerId = GetFarmerId();
            this.Resize += frmFarmersDashboard_Resize;
            this.Load += frmFarmersDashboard_Load;
        }

        private int GetFarmerId()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT FarmerID FROM Farmers WHERE FarmerName = @name";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@name", loggedInFarmerName);
                    object result = cmd.ExecuteScalar();
                    return result != null ? (int)result : 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        private void frmFarmersDashboard_Load(object sender, EventArgs e)
        {
            this.Text = "CSMS - Farmer Dashboard";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(240, 242, 245);

            Label lblWelcome = new Label
            {
                Text = "Welcome, " + loggedInFarmerName + "!",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(30, 20),
                AutoSize = true
            };
            this.Controls.Add(lblWelcome);

            lblDateTime = new Label
            {
                Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy | hh:mm tt"),
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Gray,
                Location = new Point(30, 55),
                AutoSize = true
            };
            this.Controls.Add(lblDateTime);

            System.Windows.Forms.Timer timeTimer = new System.Windows.Forms.Timer();
            timeTimer.Interval = 1000;
            timeTimer.Tick += (s, ev) =>
            {
                lblDateTime.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy | hh:mm tt");
            };
            timeTimer.Start();

            btnAnnouncements = new Button
            {
                Text = " Announcements",
                Size = new Size(160, 40),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Location = new Point(this.ClientSize.Width - 190, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnAnnouncements.FlatAppearance.BorderSize = 0;
            btnAnnouncements.Click += BtnAnnouncements_Click;
            this.Controls.Add(btnAnnouncements);

            Button btnLogout = new Button
            {
                Text = "Logout",
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Location = new Point(this.ClientSize.Width - 120, this.ClientSize.Height - 60),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) =>
            {
                this.Close();
                frmlogin login = new frmlogin();
                login.Show();
            };
            this.Controls.Add(btnLogout);

            badge = new Label
            {
                Text = "",
                BackColor = Color.Red,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(4, 1, 4, 1),
                Visible = false
            };
            badge.Paint += (s, ev) =>
            {
                Label lbl = s as Label;
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(0, 0, lbl.Width, lbl.Height);
                lbl.Region = new Region(path);
            };
            this.Controls.Add(badge);

            CreateStatisticsCards();
            CreateDeliveriesTable();
            CreateAdvancesTable();
            CreateReportButtons();
            CreateAIButton();

            CheckForNewAnnouncements();

            System.Windows.Forms.Timer checkTimer = new System.Windows.Forms.Timer();
            checkTimer.Interval = 30000;
            checkTimer.Tick += (s, ev) => CheckForNewAnnouncements();
            checkTimer.Start();
        }

        private void CreateStatisticsCards()
        {
            Panel statsPanel = new Panel
            {
                Location = new Point(30, 90),
                Size = new Size(this.ClientSize.Width - 60, 120),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var stats = GetFarmerStats();

            Panel card1 = CreateStatCard("Total Supplied", stats.TotalKg + " KG", Color.FromArgb(52, 152, 219), 0);
            Panel card2 = CreateStatCard("Total Earnings", "KES " + stats.TotalEarnings.ToString("N2"), Color.FromArgb(46, 204, 113), 220);
            Panel card3 = CreateStatCard("Total Advances", "KES " + stats.TotalAdvances.ToString("N2"), Color.FromArgb(241, 196, 15), 440);
            Panel card4 = CreateStatCard("Net Payable", "KES " + stats.NetPayable.ToString("N2"), Color.FromArgb(155, 89, 182), 660);

            statsPanel.Controls.AddRange(new Control[] { card1, card2, card3, card4 });
            this.Controls.Add(statsPanel);
        }

        private Panel CreateStatCard(string title, string value, Color color, int xPos)
        {
            Panel card = new Panel
            {
                Size = new Size(200, 100),
                Location = new Point(xPos, 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            Panel colorBar = new Panel
            {
                Size = new Size(200, 5),
                Location = new Point(0, 0),
                BackColor = color
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(15, 20),
                AutoSize = true
            };

            Label lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(15, 50),
                AutoSize = true
            };

            card.Controls.AddRange(new Control[] { colorBar, lblTitle, lblValue });
            return card;
        }

        private FarmerStats GetFarmerStats()
        {
            FarmerStats stats = new FarmerStats();
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string deliveryQuery = @"
                        SELECT ISNULL(SUM(d.WeightKg * cp.PricePerKg), 0), ISNULL(SUM(d.WeightKg), 0) 
                        FROM Deliveries d
                        LEFT JOIN CoffeePrices cp ON d.CoffeeGrade = cp.CoffeeGrade 
                            AND cp.EffectiveDate <= d.DeliveryDate
                        WHERE d.FarmerID = @fid";

                    SqlCommand cmd = new SqlCommand(deliveryQuery, conn);
                    cmd.Parameters.AddWithValue("@fid", currentFarmerId);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        stats.TotalEarnings = Convert.ToDecimal(reader[0]);
                        stats.TotalKg = Convert.ToDecimal(reader[1]);
                    }
                    reader.Close();

                    string advanceQuery = "SELECT ISNULL(SUM(TotalAmount), 0) FROM FarmerAdvances WHERE FarmerID = @fid";
                    cmd = new SqlCommand(advanceQuery, conn);
                    cmd.Parameters.AddWithValue("@fid", currentFarmerId);
                    stats.TotalAdvances = Convert.ToDecimal(cmd.ExecuteScalar());

                    stats.NetPayable = stats.TotalEarnings - stats.TotalAdvances;
                    if (stats.NetPayable < 0) stats.NetPayable = 0;
                }
            }
            catch
            {
                stats.TotalKg = 0;
                stats.TotalEarnings = 0;
                stats.TotalAdvances = 0;
                stats.NetPayable = 0;
            }
            return stats;
        }

        private void CreateDeliveriesTable()
        {
            GroupBox grpDeliveries = new GroupBox
            {
                Text = " MY DELIVERIES ",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(30, 220),
                Size = new Size(this.ClientSize.Width - 60, 200),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.White
            };

            dgvDeliveries = new DataGridView
            {
                Location = new Point(10, 25),
                Size = new Size(grpDeliveries.Width - 20, 165),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };

            dgvDeliveries.EnableHeadersVisualStyles = false;
            dgvDeliveries.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvDeliveries.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

            grpDeliveries.Controls.Add(dgvDeliveries);
            this.Controls.Add(grpDeliveries);

            LoadDeliveries();
        }

        private void LoadDeliveries()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            CONVERT(VARCHAR, d.DeliveryDate, 103) AS Date, 
                            d.WeightKg, 
                            d.CoffeeGrade AS Grade, 
                            ISNULL(d.WeightKg * cp.PricePerKg, 0) AS Amount,
                            d.PaymentStatus AS Status
                        FROM Deliveries d
                        LEFT JOIN CoffeePrices cp ON d.CoffeeGrade = cp.CoffeeGrade 
                            AND cp.EffectiveDate <= d.DeliveryDate
                        WHERE d.FarmerID = @fid 
                        ORDER BY d.DeliveryDate DESC";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@fid", currentFarmerId);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvDeliveries.DataSource = dt;

                    if (dgvDeliveries.Columns.Contains("Amount"))
                    {
                        dgvDeliveries.Columns["Amount"].DefaultCellStyle.Format = "N2";
                    }

                    if (dgvDeliveries.Columns.Contains("Status"))
                    {
                        foreach (DataGridViewRow row in dgvDeliveries.Rows)
                        {
                            if (row.Cells["Status"].Value != null)
                            {
                                string status = row.Cells["Status"].Value.ToString();
                                if (status == "Paid")
                                {
                                    row.DefaultCellStyle.ForeColor = Color.Green;
                                }
                                else if (status == "Pending")
                                {
                                    row.DefaultCellStyle.ForeColor = Color.OrangeRed;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading deliveries: " + ex.Message);
            }
        }

        private void CreateAdvancesTable()
        {
            GroupBox grpAdvances = new GroupBox
            {
                Text = " MY ADVANCES ",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(30, 430),
                Size = new Size(this.ClientSize.Width - 60, 180),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.White
            };

            dgvAdvances = new DataGridView
            {
                Location = new Point(10, 25),
                Size = new Size(grpAdvances.Width - 20, 145),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };

            dgvAdvances.EnableHeadersVisualStyles = false;
            dgvAdvances.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvAdvances.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

            grpAdvances.Controls.Add(dgvAdvances);
            this.Controls.Add(grpAdvances);

            LoadAdvances();
        }

        private void LoadAdvances()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string query = @"SELECT 
                                        CONVERT(VARCHAR, DateTaken, 103) AS Date, 
                                        ProductType, 
                                        Quantity, 
                                        PricePerUnit, 
                                        TotalAmount AS Amount,
                                        DeductionStatus AS Status
                                    FROM FarmerAdvances 
                                    WHERE FarmerID = @fid 
                                    ORDER BY DateTaken DESC";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@fid", currentFarmerId);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvAdvances.DataSource = dt;

                    if (dgvAdvances.Columns.Contains("Amount"))
                    {
                        dgvAdvances.Columns["Amount"].DefaultCellStyle.Format = "N2";
                    }

                    if (dgvAdvances.Columns.Contains("Status"))
                    {
                        foreach (DataGridViewRow row in dgvAdvances.Rows)
                        {
                            if (row.Cells["Status"].Value != null)
                            {
                                string status = row.Cells["Status"].Value.ToString();
                                if (status == "Deducted")
                                {
                                    row.DefaultCellStyle.ForeColor = Color.Green;
                                }
                                else if (status == "Pending")
                                {
                                    row.DefaultCellStyle.ForeColor = Color.OrangeRed;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading advances: " + ex.Message);
            }
        }

        private void CreateReportButtons()
        {
            Panel reportPanel = new Panel
            {
                Location = new Point(30, 620),
                Size = new Size(700, 50),
                BackColor = Color.Transparent
            };

            Button btnAdvanceReport = new Button
            {
                Text = "Advance Statements",
                Location = new Point(0, 10),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAdvanceReport.FlatAppearance.BorderSize = 0;
            btnAdvanceReport.Click += (s, e) => ShowAdvanceFilterDialog();

            Button btnDeliveryReport = new Button
            {
                Text = "Delivery Report",
                Location = new Point(170, 10),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDeliveryReport.FlatAppearance.BorderSize = 0;
            btnDeliveryReport.Click += (s, e) => ShowDeliveryFilterDialog();

            Button btnFullReport = new Button
            {
                Text = "Full Statement",
                Location = new Point(340, 10),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnFullReport.FlatAppearance.BorderSize = 0;
            btnFullReport.Click += (s, e) => ShowFullStatementFilterDialog();

            reportPanel.Controls.AddRange(new Control[] { btnAdvanceReport, btnDeliveryReport, btnFullReport });
            this.Controls.Add(reportPanel);
        }

        private void ShowAdvanceFilterDialog()
        {
            Form filterForm = new Form
            {
                Text = "Filter Advance Report",
                Size = new Size(300, 180),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                BackColor = Color.White
            };

            Label lblStatus = new Label
            {
                Text = "Select Status:",
                Location = new Point(20, 30),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };

            ComboBox cmbStatus = new ComboBox
            {
                Location = new Point(150, 27),
                Size = new Size(120, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11)
            };
            cmbStatus.Items.AddRange(new string[] { "All", "Pending", "Deducted" });
            cmbStatus.SelectedIndex = 0;

            Button btnGenerate = new Button
            {
                Text = "Generate Report",
                Location = new Point(80, 90),
                Size = new Size(130, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnGenerate.Click += (s, e) =>
            {
                string status = cmbStatus.SelectedItem.ToString();
                PrintAdvanceReportWithFilter(status);
                filterForm.Close();
            };

            filterForm.Controls.Add(lblStatus);
            filterForm.Controls.Add(cmbStatus);
            filterForm.Controls.Add(btnGenerate);
            filterForm.ShowDialog();
        }

        private void ShowDeliveryFilterDialog()
        {
            Form filterForm = new Form
            {
                Text = "Filter Delivery Report",
                Size = new Size(300, 180),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                BackColor = Color.White
            };

            Label lblStatus = new Label
            {
                Text = "Select Status:",
                Location = new Point(20, 30),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };

            ComboBox cmbStatus = new ComboBox
            {
                Location = new Point(150, 27),
                Size = new Size(120, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11)
            };
            cmbStatus.Items.AddRange(new string[] { "All", "Pending", "Paid" });
            cmbStatus.SelectedIndex = 0;

            Button btnGenerate = new Button
            {
                Text = "Generate Report",
                Location = new Point(80, 90),
                Size = new Size(130, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnGenerate.Click += (s, e) =>
            {
                string status = cmbStatus.SelectedItem.ToString();
                PrintDeliveryReportWithFilter(status);
                filterForm.Close();
            };

            filterForm.Controls.Add(lblStatus);
            filterForm.Controls.Add(cmbStatus);
            filterForm.Controls.Add(btnGenerate);
            filterForm.ShowDialog();
        }

        private void ShowFullStatementFilterDialog()
        {
            Form filterForm = new Form
            {
                Text = "Filter Full Statement",
                Size = new Size(350, 220),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                BackColor = Color.White
            };

            Label lblDeliveryStatus = new Label
            {
                Text = "Delivery Status:",
                Location = new Point(20, 30),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };

            ComboBox cmbDeliveryStatus = new ComboBox
            {
                Location = new Point(160, 27),
                Size = new Size(120, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11)
            };
            cmbDeliveryStatus.Items.AddRange(new string[] { "All", "Pending", "Paid" });
            cmbDeliveryStatus.SelectedIndex = 0;

            Label lblAdvanceStatus = new Label
            {
                Text = "Advance Status:",
                Location = new Point(20, 70),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };

            ComboBox cmbAdvanceStatus = new ComboBox
            {
                Location = new Point(160, 67),
                Size = new Size(120, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11)
            };
            cmbAdvanceStatus.Items.AddRange(new string[] { "All", "Pending", "Deducted" });
            cmbAdvanceStatus.SelectedIndex = 0;

            Button btnGenerate = new Button
            {
                Text = "Generate Report",
                Location = new Point(100, 120),
                Size = new Size(130, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnGenerate.Click += (s, e) =>
            {
                string deliveryStatus = cmbDeliveryStatus.SelectedItem.ToString();
                string advanceStatus = cmbAdvanceStatus.SelectedItem.ToString();
                PrintFullStatementWithFilter(deliveryStatus, advanceStatus);
                filterForm.Close();
            };

            filterForm.Controls.Add(lblDeliveryStatus);
            filterForm.Controls.Add(cmbDeliveryStatus);
            filterForm.Controls.Add(lblAdvanceStatus);
            filterForm.Controls.Add(cmbAdvanceStatus);
            filterForm.Controls.Add(btnGenerate);
            filterForm.ShowDialog();
        }

        private void PrintAdvanceReportWithFilter(string statusFilter)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string query = @"SELECT 
                                        CONVERT(VARCHAR, DateTaken, 103) AS Date, 
                                        ProductType, 
                                        Quantity, 
                                        PricePerUnit, 
                                        TotalAmount AS Amount, 
                                        DeductionStatus AS Status
                                    FROM FarmerAdvances 
                                    WHERE FarmerID = @fid";

                    if (statusFilter != "All")
                    {
                        query += " AND DeductionStatus = @status";
                    }

                    query += " ORDER BY DateTaken DESC";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@fid", currentFarmerId);
                    if (statusFilter != "All")
                    {
                        cmd.Parameters.AddWithValue("@status", statusFilter);
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("No advances found with status: " + statusFilter, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    GeneratePDFReport(dt, "ADVANCE STATEMENT", "Advances - Status: " + statusFilter);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void PrintDeliveryReportWithFilter(string statusFilter)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string query = @"SELECT 
                                        CONVERT(VARCHAR, d.DeliveryDate, 103) AS Date, 
                                        d.WeightKg, 
                                        d.CoffeeGrade AS Grade, 
                                        ISNULL(cp.PricePerKg, 0) AS PricePerKg, 
                                        ISNULL(d.WeightKg * cp.PricePerKg, 0) AS Amount,
                                        d.PaymentStatus AS Status
                                    FROM Deliveries d
                                    LEFT JOIN CoffeePrices cp ON d.CoffeeGrade = cp.CoffeeGrade 
                                        AND cp.EffectiveDate <= d.DeliveryDate
                                    WHERE d.FarmerID = @fid";

                    if (statusFilter != "All")
                    {
                        query += " AND d.PaymentStatus = @status";
                    }

                    query += " ORDER BY d.DeliveryDate DESC";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@fid", currentFarmerId);
                    if (statusFilter != "All")
                    {
                        cmd.Parameters.AddWithValue("@status", statusFilter);
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("No deliveries found with status: " + statusFilter, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    GeneratePDFReport(dt, "DELIVERY STATEMENT", "Deliveries - Status: " + statusFilter);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void PrintFullStatementWithFilter(string deliveryStatusFilter, string advanceStatusFilter)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string deliveriesQuery = @"SELECT 
                                                CONVERT(VARCHAR, d.DeliveryDate, 103) AS Date, 
                                                d.WeightKg, 
                                                d.CoffeeGrade AS Grade, 
                                                ISNULL(cp.PricePerKg, 0) AS PricePerKg, 
                                                ISNULL(d.WeightKg * cp.PricePerKg, 0) AS Amount,
                                                d.PaymentStatus AS Status
                                            FROM Deliveries d
                                            LEFT JOIN CoffeePrices cp ON d.CoffeeGrade = cp.CoffeeGrade 
                                                AND cp.EffectiveDate <= d.DeliveryDate
                                            WHERE d.FarmerID = @fid";

                    if (deliveryStatusFilter != "All")
                    {
                        deliveriesQuery += " AND d.PaymentStatus = @deliveryStatus";
                    }
                    deliveriesQuery += " ORDER BY d.DeliveryDate DESC";

                    SqlCommand deliveryCmd = new SqlCommand(deliveriesQuery, conn);
                    deliveryCmd.Parameters.AddWithValue("@fid", currentFarmerId);
                    if (deliveryStatusFilter != "All")
                    {
                        deliveryCmd.Parameters.AddWithValue("@deliveryStatus", deliveryStatusFilter);
                    }

                    SqlDataAdapter deliveryAdapter = new SqlDataAdapter(deliveryCmd);
                    DataTable dtDeliveries = new DataTable();
                    deliveryAdapter.Fill(dtDeliveries);

                    string advancesQuery = @"SELECT 
                                                CONVERT(VARCHAR, DateTaken, 103) AS Date, 
                                                ProductType, 
                                                Quantity, 
                                                PricePerUnit, 
                                                TotalAmount AS Amount,
                                                DeductionStatus AS Status
                                            FROM FarmerAdvances 
                                            WHERE FarmerID = @fid";

                    if (advanceStatusFilter != "All")
                    {
                        advancesQuery += " AND DeductionStatus = @advanceStatus";
                    }
                    advancesQuery += " ORDER BY DateTaken DESC";

                    SqlCommand advanceCmd = new SqlCommand(advancesQuery, conn);
                    advanceCmd.Parameters.AddWithValue("@fid", currentFarmerId);
                    if (advanceStatusFilter != "All")
                    {
                        advanceCmd.Parameters.AddWithValue("@advanceStatus", advanceStatusFilter);
                    }

                    SqlDataAdapter advanceAdapter = new SqlDataAdapter(advanceCmd);
                    DataTable dtAdvances = new DataTable();
                    advanceAdapter.Fill(dtAdvances);

                    if (dtDeliveries.Rows.Count == 0 && dtAdvances.Rows.Count == 0)
                    {
                        MessageBox.Show("No records found with selected filters.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    GenerateFullStatementPDF(dtDeliveries, dtAdvances, deliveryStatusFilter, advanceStatusFilter);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void CreateAIButton()
        {
            Button btnAI = new Button
            {
                Text = "AI Advisor",
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Location = new Point(this.ClientSize.Width - 360, this.ClientSize.Height - 60),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnAI.FlatAppearance.BorderSize = 0;
            btnAI.Click += (s, e) => ShowAIAdvisorPopup();
            this.Controls.Add(btnAI);
        }

        private void ShowAIAdvisorPopup()
        {
            Form aiForm = new Form
            {
                Text = "AI Coffee Farming Advisor - Smart Assistant",
                Size = new Size(550, 650),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            };

            Panel headerPanel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(52, 73, 94)
            };

            Label lblHeader = new Label
            {
                Text = "☕ AI Coffee Farming Advisor",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblHeader);
            aiForm.Controls.Add(headerPanel);

            TabControl tabControl = new TabControl
            {
                Location = new Point(10, 70),
                Size = new Size(515, 450),
                Font = new Font("Segoe UI", 10)
            };

            // Tab 1: Farm Assessment
            TabPage tabAssessment = new TabPage("Farm Assessment");
            Panel assessmentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15),
                AutoScroll = true
            };

            int yPos = 10;

            Label lblWeather = new Label
            {
                Text = "Current Weather / Season:",
                Location = new Point(10, yPos),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            ComboBox cmbWeather = new ComboBox
            {
                Location = new Point(200, yPos - 3),
                Size = new Size(180, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbWeather.Items.AddRange(new string[] { "Dry Season", "Rainy Season", "Harvest Season" });
            cmbWeather.SelectedIndex = 0;
            assessmentPanel.Controls.Add(lblWeather);
            assessmentPanel.Controls.Add(cmbWeather);

            yPos += 45;

            Label lblAction = new Label
            {
                Text = "Last Action Performed:",
                Location = new Point(10, yPos),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            ComboBox cmbAction = new ComboBox
            {
                Location = new Point(200, yPos - 3),
                Size = new Size(180, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbAction.Items.AddRange(new string[] { "None", "Fertilized", "Sprayed", "Pruned", "Watered", "Harvested" });
            cmbAction.SelectedIndex = 0;
            assessmentPanel.Controls.Add(lblAction);
            assessmentPanel.Controls.Add(cmbAction);

            yPos += 55;

            Label lblSymptoms = new Label
            {
                Text = "Observed Symptoms (select all that apply):",
                Location = new Point(10, yPos),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            assessmentPanel.Controls.Add(lblSymptoms);

            yPos += 30;

            GroupBox symptomGroup = new GroupBox
            {
                Text = "Leaf & Berry Symptoms",
                Location = new Point(10, yPos),
                Size = new Size(480, 150),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            CheckBox chkYellow = new CheckBox { Text = "Yellow/orange spots on leaves (Coffee Leaf Rust)", Location = new Point(15, 25), AutoSize = true, Font = new Font("Segoe UI", 9) };
            CheckBox chkBrown = new CheckBox { Text = "Brown spots on berries (Coffee Berry Disease)", Location = new Point(15, 50), AutoSize = true, Font = new Font("Segoe UI", 9) };
            CheckBox chkCurling = new CheckBox { Text = "Leaves curling or wilting", Location = new Point(15, 75), AutoSize = true, Font = new Font("Segoe UI", 9) };
            CheckBox chkHoles = new CheckBox { Text = "Small holes in berries (Berry Borer)", Location = new Point(15, 100), AutoSize = true, Font = new Font("Segoe UI", 9) };
            CheckBox chkPowdery = new CheckBox { Text = "Powdery white coating on leaves", Location = new Point(250, 25), AutoSize = true, Font = new Font("Segoe UI", 9) };
            CheckBox chkWilting = new CheckBox { Text = "Sudden wilting of branches", Location = new Point(250, 50), AutoSize = true, Font = new Font("Segoe UI", 9) };
            CheckBox chkNoSymptoms = new CheckBox { Text = "No symptoms observed - Healthy farm", Location = new Point(250, 75), AutoSize = true, Font = new Font("Segoe UI", 9) };
            CheckBox chkStunted = new CheckBox { Text = "Stunted growth / yellow leaves", Location = new Point(250, 100), AutoSize = true, Font = new Font("Segoe UI", 9) };

            symptomGroup.Controls.AddRange(new Control[] { chkYellow, chkBrown, chkCurling, chkHoles, chkPowdery, chkWilting, chkNoSymptoms, chkStunted });
            assessmentPanel.Controls.Add(symptomGroup);

            yPos += 165;

            Button btnGetAdvice = new Button
            {
                Text = "Get AI Advice",
                Location = new Point(10, yPos),
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            assessmentPanel.Controls.Add(btnGetAdvice);
            tabAssessment.Controls.Add(assessmentPanel);

            // Tab 2: Advice Output
            TabPage tabAdvice = new TabPage("AI Advice");
            Panel advicePanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            RichTextBox txtAdvice = new RichTextBox
            {
                Location = new Point(10, 10),
                Size = new Size(480, 340),
                ReadOnly = true,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(248, 249, 250),
                BorderStyle = BorderStyle.FixedSingle
            };
            advicePanel.Controls.Add(txtAdvice);

            Button btnPrintAdvice = new Button
            {
                Text = "Print Advice",
                Location = new Point(10, 360),
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            advicePanel.Controls.Add(btnPrintAdvice);

            Button btnSaveAdvice = new Button
            {
                Text = "Save to File",
                Location = new Point(160, 360),
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            advicePanel.Controls.Add(btnSaveAdvice);

            tabAdvice.Controls.Add(advicePanel);

            // Tab 3: Calendar & Schedule
            TabPage tabSchedule = new TabPage("Farm Calendar");
            Panel schedulePanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15),
                AutoScroll = true
            };

            Label lblScheduleTitle = new Label
            {
                Text = "Recommended Farm Activities Schedule",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(10, 10),
                AutoSize = true
            };
            schedulePanel.Controls.Add(lblScheduleTitle);

            string[] scheduleItems = {
                "January - February: Pruning and shaping trees",
                "March - April: Apply fertilizer, prepare for rainy season",
                "May - June: Monitor for fungal diseases, apply preventive spray",
                "July - August: Weeding and mulching",
                "September - October: Harvest preparation, clean equipment",
                "November - December: Main harvest season, process cherries"
            };

            int sYPos = 50;
            foreach (string item in scheduleItems)
            {
                Label lblItem = new Label
                {
                    Text = "• " + item,
                    Location = new Point(10, sYPos),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.FromArgb(52, 73, 94)
                };
                schedulePanel.Controls.Add(lblItem);
                sYPos += 30;
            }

            tabSchedule.Controls.Add(schedulePanel);

            // Tab 4: Tips
            TabPage tabTips = new TabPage("Farming Tips");
            Panel tipsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15),
                AutoScroll = true
            };

            string[] tips = {
                "💧 Water deeply 2-3 times per week during dry season",
                "🌿 Apply mulch to retain soil moisture and suppress weeds",
                "🐞 Introduce beneficial insects for natural pest control",
                "✂️ Prune after harvest to maintain tree shape and productivity",
                "🧪 Test soil pH annually - optimal range is 6.0-6.5",
                "🌱 Plant shade trees to protect coffee from extreme temperatures",
                "📝 Keep records of all inputs and harvests for better planning",
                "👥 Join farmer cooperative for better prices and support",
                "🚜 Rotate fertilizers to prevent nutrient depletion",
                "🌧️ During heavy rains, ensure proper drainage to prevent root rot"
            };

            int tYPos = 10;
            foreach (string tip in tips)
            {
                Label lblTip = new Label
                {
                    Text = tip,
                    Location = new Point(10, tYPos),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.FromArgb(44, 62, 80)
                };
                tipsPanel.Controls.Add(lblTip);
                tYPos += 30;
            }

            tabTips.Controls.Add(tipsPanel);

            tabControl.TabPages.Add(tabAssessment);
            tabControl.TabPages.Add(tabAdvice);
            tabControl.TabPages.Add(tabSchedule);
            tabControl.TabPages.Add(tabTips);
            aiForm.Controls.Add(tabControl);

            Button btnClose = new Button
            {
                Text = "Close",
                Location = new Point(aiForm.Width - 90, aiForm.Height - 55),
                Size = new Size(80, 35),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnClose.Click += (s, e) => aiForm.Close();
            aiForm.Controls.Add(btnClose);

            btnGetAdvice.Click += (s, e) =>
            {
                string weather = cmbWeather.SelectedItem?.ToString() ?? "";
                string action = cmbAction.SelectedItem?.ToString() ?? "";

                List<string> symptoms = new List<string>();
                if (chkYellow.Checked) symptoms.Add("Yellow spots on leaves");
                if (chkBrown.Checked) symptoms.Add("Brown spots on berries");
                if (chkCurling.Checked) symptoms.Add("Curling leaves");
                if (chkHoles.Checked) symptoms.Add("Holes in berries");
                if (chkPowdery.Checked) symptoms.Add("Powdery coating");
                if (chkWilting.Checked) symptoms.Add("Wilting branches");
                if (chkStunted.Checked) symptoms.Add("Stunted growth");
                if (chkNoSymptoms.Checked) symptoms.Add("No symptoms");

                string advice = GenerateComprehensiveAdvice(weather, action, symptoms);
                txtAdvice.Text = advice;
                tabControl.SelectedTab = tabAdvice;
            };

            btnPrintAdvice.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtAdvice.Text))
                {
                    MessageBox.Show("Please get AI advice first before printing.", "No Advice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string folder = Path.Combine(documentsPath, "CSMS_Advice");
                Directory.CreateDirectory(folder);
                string fileName = "AI_Advice_" + loggedInFarmerName.Replace(" ", "_") + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
                string filePath = Path.Combine(folder, fileName);
                File.WriteAllText(filePath, txtAdvice.Text);
                MessageBox.Show("Advice saved to:\n" + filePath, "Advice Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = filePath, UseShellExecute = true });
            };

            btnSaveAdvice.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtAdvice.Text))
                {
                    MessageBox.Show("Please get AI advice first before saving.", "No Advice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SaveFileDialog sfd = new SaveFileDialog
                {
                    Title = "Save AI Advice",
                    Filter = "Text Files|*.txt|PDF Files|*.pdf",
                    DefaultExt = "txt",
                    FileName = "AI_Advice_" + loggedInFarmerName.Replace(" ", "_") + "_" + DateTime.Now.ToString("yyyyMMdd")
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(sfd.FileName, txtAdvice.Text);
                    MessageBox.Show("Advice saved to:\n" + sfd.FileName, "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            aiForm.ShowDialog();
        }

        private string GenerateComprehensiveAdvice(string weather, string action, List<string> symptoms)
        {
            List<string> recommendations = new List<string>();

            recommendations.Add("AI COFFEE FARMING ADVISOR REPORT");
            recommendations.Add("========================================");
            recommendations.Add("");
            recommendations.Add("FARM PROFILE:");
            recommendations.Add("   Farmer: " + loggedInFarmerName);
            recommendations.Add("   Date: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
            recommendations.Add("   Weather/Season: " + weather);
            recommendations.Add("   Last Action: " + action);
            recommendations.Add("   Observed Symptoms: " + (symptoms.Count > 0 ? string.Join(", ", symptoms) : "None reported"));
            recommendations.Add("");

            recommendations.Add("SEASONAL RECOMMENDATIONS:");
            if (weather == "Rainy Season")
            {
                recommendations.Add("   ⚠️ HIGH RISK: Fungal diseases are common in rainy season");
                if (action != "Sprayed")
                    recommendations.Add("   → Apply preventive fungicide before heavy rains");
                recommendations.Add("   → Ensure proper drainage to prevent root rot");
                recommendations.Add("   → Avoid working in fields when plants are wet");
                recommendations.Add("   → Monitor for Coffee Berry Disease weekly");
            }
            else if (weather == "Dry Season")
            {
                recommendations.Add("   💧 FOCUS: Water conservation is critical");
                if (action != "Watered")
                    recommendations.Add("   → Water deeply 2-3 times per week (early morning or evening)");
                recommendations.Add("   → Apply mulch (grass, leaves) to retain soil moisture");
                recommendations.Add("   → Reduce fertilizer application until rains return");
                recommendations.Add("   → Prune dead or diseased branches");
            }
            else if (weather == "Harvest Season")
            {
                recommendations.Add("   🍒 HARVEST: Peak harvesting time");
                recommendations.Add("   → Harvest ripe cherries every 7-10 days");
                recommendations.Add("   → Sort cherries by color and size for better pricing");
                recommendations.Add("   → Process within 24 hours of harvest");
                recommendations.Add("   → Clean equipment between batches");
            }

            recommendations.Add("");

            if (symptoms.Count > 0 && !symptoms.Contains("No symptoms"))
            {
                recommendations.Add("SYMPTOM ANALYSIS & TREATMENT:");
                foreach (string symptom in symptoms)
                {
                    if (symptom.Contains("Yellow spots"))
                    {
                        recommendations.Add("   🟡 COFFEE LEAF RUST (Hemileia vastatrix)");
                        recommendations.Add("      - Apply copper-based fungicide immediately");
                        recommendations.Add("      - Spray every 14 days for 3 cycles");
                        recommendations.Add("      - Remove severely infected leaves");
                        recommendations.Add("      - Improve air circulation by pruning");
                    }
                    else if (symptom.Contains("Brown spots"))
                    {
                        recommendations.Add("   🟤 COFFEE BERRY DISEASE (CBD)");
                        recommendations.Add("      - Remove and destroy infected berries");
                        recommendations.Add("      - Apply fungicide after harvesting");
                        recommendations.Add("      - Plant resistant varieties next season");
                    }
                    else if (symptom.Contains("Curling"))
                    {
                        recommendations.Add("   🍃 LEAF CURL / WATER STRESS");
                        if (weather == "Dry Season")
                            recommendations.Add("      - Increase watering frequency immediately");
                        else
                            recommendations.Add("      - Check for aphids under leaves");
                        recommendations.Add("      - Apply organic insecticidal soap if aphids present");
                    }
                    else if (symptom.Contains("Holes"))
                    {
                        recommendations.Add("   🐛 COFFEE BERRY BORER (Hypothenemus hampei)");
                        recommendations.Add("      - Remove and destroy all infected berries");
                        recommendations.Add("      - Set up alcohol-baited traps (1 per 10 trees)");
                        recommendations.Add("      - Harvest all ripe berries promptly");
                        recommendations.Add("      - Apply Beauveria bassiana (biological control)");
                    }
                    else if (symptom.Contains("Powdery"))
                    {
                        recommendations.Add("   ⚪ POWDERY MILDEW");
                        recommendations.Add("      - Apply sulfur-based fungicide");
                        recommendations.Add("      - Improve air circulation through pruning");
                        recommendations.Add("      - Reduce shade density if excessive");
                    }
                    else if (symptom.Contains("Wilting"))
                    {
                        recommendations.Add("   🌿 SUDDEN WILTING");
                        if (weather == "Dry Season")
                            recommendations.Add("      - Emergency: Deep water immediately");
                        else
                            recommendations.Add("      - Check for root rot, improve drainage");
                        recommendations.Add("      - Consult extension officer urgently");
                    }
                    else if (symptom.Contains("Stunted"))
                    {
                        recommendations.Add("   📉 STUNTED GROWTH / NUTRIENT DEFICIENCY");
                        recommendations.Add("      - Test soil pH (optimal 6.0-6.5)");
                        recommendations.Add("      - Apply balanced NPK fertilizer (2:1:2 ratio)");
                        recommendations.Add("      - Add organic compost around trees");
                    }
                }
                recommendations.Add("");
            }
            else if (symptoms.Contains("No symptoms"))
            {
                recommendations.Add("✅ FARM HEALTH STATUS: GOOD");
                recommendations.Add("   Continue regular maintenance:");
                recommendations.Add("   → Weekly farm inspection for early detection");
                recommendations.Add("   → Maintain proper spacing between trees");
                recommendations.Add("   → Keep records of all activities");
                recommendations.Add("");
            }

            recommendations.Add("ACTION PLAN BASED ON LAST ACTION:");
            if (action == "Fertilized")
            {
                recommendations.Add("   ✓ Fertilizer applied - Rain will help absorption");
                recommendations.Add("   → Monitor plant response in 2-3 weeks");
                recommendations.Add("   → Avoid over-fertilizing (burn risk)");
            }
            else if (action == "Sprayed")
            {
                recommendations.Add("   ✓ Spray applied - Monitor effectiveness in 7 days");
                recommendations.Add("   → Wear protective gear during application");
                recommendations.Add("   → Follow re-entry interval on label");
            }
            else if (action == "Pruned")
            {
                recommendations.Add("   ✓ Pruning completed");
                recommendations.Add("   → Apply copper spray to cut wounds");
                recommendations.Add("   → Remove pruned branches from farm");
            }
            else if (action == "Watered")
            {
                recommendations.Add("   ✓ Watering done - Best done early morning");
                recommendations.Add("   → Deep watering encourages root growth");
            }
            else if (action == "Harvested")
            {
                recommendations.Add("   ✓ Harvest completed");
                recommendations.Add("   → Process cherries within 24 hours");
                recommendations.Add("   → Sort by size for better prices");
            }
            else
            {
                recommendations.Add("   ℹ️ No recent activity recorded");
                recommendations.Add("   → Start with basic maintenance: weeding, inspection");
            }

            recommendations.Add("");
            recommendations.Add("CONTACT & RESOURCES:");
            recommendations.Add("   📞 Extension Officer: 0712345678");
            recommendations.Add("   📍 Subukia Coffee Society Office");
            recommendations.Add("   📧 Email: support@subukiacoffee.co.ke");
            recommendations.Add("");
            recommendations.Add("========================================");
            recommendations.Add("This advice is AI-generated. For severe cases,");
            recommendations.Add("consult your local agricultural extension officer.");

            return string.Join(Environment.NewLine, recommendations);
        }

        private void GeneratePDFReport(DataTable data, string title, string subtitle)
        {
            try
            {
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string folder = Path.Combine(documentsPath, "CSMS_Reports");
                Directory.CreateDirectory(folder);

                string fileName = loggedInFarmerName.Replace(" ", "_") + "_" + title.Replace(" ", "_") + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf";
                string filePath = Path.Combine(folder, fileName);

                Document doc = new Document(PageSize.A4);
                PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                doc.Open();

                iTextSharp.text.Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                iTextSharp.text.Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                iTextSharp.text.Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                iTextSharp.text.Font tableHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

                Paragraph society = new Paragraph("SUBUKIA COFFEE SOCIETY", titleFont);
                society.Alignment = Element.ALIGN_CENTER;
                doc.Add(society);

                doc.Add(new Paragraph("Farmer: " + loggedInFarmerName, normalFont));
                doc.Add(new Paragraph("Farmer ID: " + currentFarmerId, normalFont));
                doc.Add(new Paragraph("Date Generated: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"), normalFont));
                doc.Add(new Paragraph(" ", normalFont));

                Paragraph reportTitle = new Paragraph(title, headerFont);
                reportTitle.Alignment = Element.ALIGN_CENTER;
                doc.Add(reportTitle);
                doc.Add(new Paragraph(subtitle, normalFont));
                doc.Add(new Paragraph(" ", normalFont));

                PdfPTable pdfTable = new PdfPTable(data.Columns.Count);
                pdfTable.WidthPercentage = 100;

                foreach (DataColumn col in data.Columns)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(col.ColumnName, tableHeaderFont));
                    cell.BackgroundColor = new BaseColor(52, 73, 94);
                    pdfTable.AddCell(cell);
                }

                decimal totalAmount = 0;
                foreach (DataRow row in data.Rows)
                {
                    foreach (DataColumn col in data.Columns)
                    {
                        string value = row[col] != DBNull.Value ? row[col].ToString() : "0";
                        if (col.ColumnName == "Amount" || col.ColumnName == "PricePerUnit")
                        {
                            decimal amount = row[col] != DBNull.Value ? Convert.ToDecimal(row[col]) : 0;
                            if (col.ColumnName == "Amount") totalAmount += amount;
                            value = "KES " + amount.ToString("N2");
                        }
                        pdfTable.AddCell(value);
                    }
                }

                doc.Add(pdfTable);

                if (totalAmount > 0)
                {
                    doc.Add(new Paragraph(" ", normalFont));
                    doc.Add(new Paragraph("TOTAL: KES " + totalAmount.ToString("N2"), headerFont));
                }
                doc.Add(new Paragraph(" ", normalFont));
                doc.Add(new Paragraph("Thank you for your partnership!", normalFont));

                doc.Close();

                MessageBox.Show("Report saved to:\n" + filePath, "Report Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = filePath, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating PDF: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerateFullStatementPDF(DataTable deliveries, DataTable advances, string deliveryFilter, string advanceFilter)
        {
            try
            {
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string folder = Path.Combine(documentsPath, "CSMS_Reports");
                Directory.CreateDirectory(folder);

                string fileName = loggedInFarmerName.Replace(" ", "_") + "_Full_Statement_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf";
                string filePath = Path.Combine(folder, fileName);

                Document doc = new Document(PageSize.A4);
                PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                doc.Open();

                iTextSharp.text.Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                iTextSharp.text.Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                iTextSharp.text.Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                iTextSharp.text.Font tableHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

                Paragraph society = new Paragraph("SUBUKIA COFFEE SOCIETY", titleFont);
                society.Alignment = Element.ALIGN_CENTER;
                doc.Add(society);

                doc.Add(new Paragraph("Farmer: " + loggedInFarmerName, normalFont));
                doc.Add(new Paragraph("Farmer ID: " + currentFarmerId, normalFont));
                doc.Add(new Paragraph("Date Generated: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"), normalFont));
                doc.Add(new Paragraph("Delivery Status Filter: " + deliveryFilter, normalFont));
                doc.Add(new Paragraph("Advance Status Filter: " + advanceFilter, normalFont));
                doc.Add(new Paragraph(" ", normalFont));

                Paragraph fullTitle = new Paragraph("FULL STATEMENT", headerFont);
                fullTitle.Alignment = Element.ALIGN_CENTER;
                doc.Add(fullTitle);
                doc.Add(new Paragraph(" ", normalFont));

                if (deliveries.Rows.Count > 0)
                {
                    Paragraph deliveriesTitle = new Paragraph("DELIVERIES", headerFont);
                    doc.Add(deliveriesTitle);
                    doc.Add(new Paragraph(" ", normalFont));

                    PdfPTable deliveryTable = new PdfPTable(6);
                    deliveryTable.WidthPercentage = 100;
                    string[] deliveryHeaders = { "Date", "Weight (KG)", "Grade", "Price/Kg", "Amount (KES)", "Status" };
                    foreach (string header in deliveryHeaders)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(header, tableHeaderFont));
                        cell.BackgroundColor = new BaseColor(52, 73, 94);
                        deliveryTable.AddCell(cell);
                    }

                    decimal totalDeliveries = 0;
                    foreach (DataRow row in deliveries.Rows)
                    {
                        deliveryTable.AddCell(row["Date"]?.ToString() ?? "N/A");
                        deliveryTable.AddCell(row["WeightKg"]?.ToString() ?? "0");
                        deliveryTable.AddCell(row["Grade"]?.ToString() ?? "N/A");
                        deliveryTable.AddCell(row["PricePerKg"] != DBNull.Value ? "KES " + Convert.ToDecimal(row["PricePerKg"]).ToString("N2") : "KES 0.00");
                        decimal amount = row["Amount"] != DBNull.Value ? Convert.ToDecimal(row["Amount"]) : 0;
                        deliveryTable.AddCell("KES " + amount.ToString("N2"));
                        deliveryTable.AddCell(row["Status"]?.ToString() ?? "N/A");
                        totalDeliveries += amount;
                    }
                    doc.Add(deliveryTable);
                    doc.Add(new Paragraph("Total Deliveries: KES " + totalDeliveries.ToString("N2"), normalFont));
                    doc.Add(new Paragraph(" ", normalFont));
                }

                if (advances.Rows.Count > 0)
                {
                    Paragraph advancesTitle = new Paragraph("ADVANCES", headerFont);
                    doc.Add(advancesTitle);
                    doc.Add(new Paragraph(" ", normalFont));

                    PdfPTable advanceTable = new PdfPTable(6);
                    advanceTable.WidthPercentage = 100;
                    string[] advanceHeaders = { "Date", "Product", "Quantity", "Price/Unit", "Amount (KES)", "Status" };
                    foreach (string header in advanceHeaders)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(header, tableHeaderFont));
                        cell.BackgroundColor = new BaseColor(52, 73, 94);
                        advanceTable.AddCell(cell);
                    }

                    decimal totalAdvances = 0;
                    foreach (DataRow row in advances.Rows)
                    {
                        advanceTable.AddCell(row["Date"]?.ToString() ?? "N/A");
                        advanceTable.AddCell(row["ProductType"]?.ToString() ?? "N/A");
                        advanceTable.AddCell(row["Quantity"]?.ToString() ?? "0");
                        advanceTable.AddCell(row["PricePerUnit"] != DBNull.Value ? "KES " + Convert.ToDecimal(row["PricePerUnit"]).ToString("N2") : "KES 0.00");
                        decimal amount = row["Amount"] != DBNull.Value ? Convert.ToDecimal(row["Amount"]) : 0;
                        advanceTable.AddCell("KES " + amount.ToString("N2"));
                        advanceTable.AddCell(row["Status"]?.ToString() ?? "N/A");
                        totalAdvances += amount;
                    }
                    doc.Add(advanceTable);
                    doc.Add(new Paragraph("Total Advances: KES " + totalAdvances.ToString("N2"), normalFont));
                    doc.Add(new Paragraph(" ", normalFont));
                }

                if (deliveries.Rows.Count > 0 || advances.Rows.Count > 0)
                {
                    decimal totalDeliveriesAmt = 0;
                    decimal totalAdvancesAmt = 0;

                    foreach (DataRow row in deliveries.Rows)
                    {
                        totalDeliveriesAmt += row["Amount"] != DBNull.Value ? Convert.ToDecimal(row["Amount"]) : 0;
                    }
                    foreach (DataRow row in advances.Rows)
                    {
                        totalAdvancesAmt += row["Amount"] != DBNull.Value ? Convert.ToDecimal(row["Amount"]) : 0;
                    }

                    doc.Add(new Paragraph("========================================", normalFont));
                    doc.Add(new Paragraph("SUMMARY", headerFont));
                    doc.Add(new Paragraph("Total Deliveries: KES " + totalDeliveriesAmt.ToString("N2"), normalFont));
                    doc.Add(new Paragraph("Total Advances:   KES " + totalAdvancesAmt.ToString("N2"), normalFont));
                    doc.Add(new Paragraph("----------------------------------------", normalFont));
                    doc.Add(new Paragraph("NET PAYABLE:      KES " + (totalDeliveriesAmt - totalAdvancesAmt).ToString("N2"), headerFont));
                    doc.Add(new Paragraph("========================================", normalFont));
                }

                doc.Add(new Paragraph(" ", normalFont));
                doc.Add(new Paragraph("Thank you for your partnership!", normalFont));

                doc.Close();

                MessageBox.Show("Full statement saved to:\n" + filePath, "Statement Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = filePath, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating PDF: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CheckForNewAnnouncements()
        {
            try
            {
                if (currentFarmerId == 0) return;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string query = @"
                        SELECT COUNT(*) 
                        FROM Announcements a
                        LEFT JOIN FarmerAnnouncementRead r ON a.AnnouncementID = r.AnnouncementID 
                            AND r.FarmerID = @farmerId
                        WHERE (r.IsRead IS NULL OR r.IsRead = 0)";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@farmerId", currentFarmerId);
                    int unreadCount = (int)cmd.ExecuteScalar();

                    if (unreadCount > 0)
                    {
                        badge.Text = unreadCount.ToString();
                        badge.Visible = true;
                        badge.Location = new Point(btnAnnouncements.Location.X + btnAnnouncements.Width - 25,
                                                  btnAnnouncements.Location.Y - 8);
                        badge.BringToFront();
                    }
                    else
                    {
                        badge.Visible = false;
                    }
                }
            }
            catch
            {
                badge.Visible = false;
            }
        }

        private void MarkAnnouncementsAsRead()
        {
            try
            {
                if (currentFarmerId == 0) return;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string getUnreadQuery = @"
                        SELECT a.AnnouncementID 
                        FROM Announcements a
                        LEFT JOIN FarmerAnnouncementRead r ON a.AnnouncementID = r.AnnouncementID 
                            AND r.FarmerID = @farmerId
                        WHERE r.IsRead IS NULL OR r.IsRead = 0";

                    SqlCommand getCmd = new SqlCommand(getUnreadQuery, conn);
                    getCmd.Parameters.AddWithValue("@farmerId", currentFarmerId);
                    SqlDataReader reader = getCmd.ExecuteReader();

                    List<int> unreadIds = new List<int>();
                    while (reader.Read())
                    {
                        unreadIds.Add(Convert.ToInt32(reader["AnnouncementID"]));
                    }
                    reader.Close();

                    foreach (int announcementId in unreadIds)
                    {
                        string insertQuery = @"
                            INSERT INTO FarmerAnnouncementRead (FarmerID, AnnouncementID, IsRead, DateRead)
                            VALUES (@farmerId, @announcementId, 1, @dateRead)";

                        SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
                        insertCmd.Parameters.AddWithValue("@farmerId", currentFarmerId);
                        insertCmd.Parameters.AddWithValue("@announcementId", announcementId);
                        insertCmd.Parameters.AddWithValue("@dateRead", DateTime.Now);
                        insertCmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
            }
        }

        private void BtnAnnouncements_Click(object sender, EventArgs e)
        {
            MarkAnnouncementsAsRead();
            badge.Visible = false;

            if (announcementButtonPanel != null && this.Controls.Contains(announcementButtonPanel))
            {
                this.Controls.Remove(announcementButtonPanel);
                announcementButtonPanel = null;
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM Announcements";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    lastAnnouncementCount = (int)cmd.ExecuteScalar();
                }
            }
            catch { }

            announcementButtonPanel = new Panel
            {
                Location = new Point(btnAnnouncements.Location.X, btnAnnouncements.Location.Y + 45),
                Size = new Size(160, 100),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Button btnLatest = new Button
            {
                Text = "Latest",
                Location = new Point(10, 10),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLatest.Click += (s, ev) => ShowLatestAnnouncements();

            Button btnAll = new Button
            {
                Text = "All",
                Location = new Point(10, 55),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAll.Click += (s, ev) => ShowAllAnnouncements();

            announcementButtonPanel.Controls.Add(btnLatest);
            announcementButtonPanel.Controls.Add(btnAll);
            this.Controls.Add(announcementButtonPanel);
            announcementButtonPanel.BringToFront();
        }

        private void ShowLatestAnnouncements()
        {
            if (announcementButtonPanel != null)
            {
                this.Controls.Remove(announcementButtonPanel);
                announcementButtonPanel = null;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT TOP 2 Title, Message, DatePosted FROM Announcements ORDER BY DatePosted DESC";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("No announcements found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    DisplayAnnouncements(dt, "Latest Announcements");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void ShowAllAnnouncements()
        {
            if (announcementButtonPanel != null)
            {
                this.Controls.Remove(announcementButtonPanel);
                announcementButtonPanel = null;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT Title, Message, DatePosted FROM Announcements ORDER BY DatePosted DESC";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("No announcements found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    DisplayAnnouncements(dt, "All Announcements");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void DisplayAnnouncements(DataTable announcements, string title)
        {
            Form announcementForm = new Form
            {
                Text = title,
                Size = new Size(600, 500),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.White,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Panel contentPanel = new Panel
            {
                Location = new Point(0, 10),
                Size = new Size(584, 450),
                AutoScroll = true,
                BackColor = Color.White
            };

            int yPos = 10;

            foreach (DataRow row in announcements.Rows)
            {
                Panel card = new Panel
                {
                    Location = new Point(15, yPos),
                    Size = new Size(540, 100),
                    BackColor = Color.FromArgb(248, 249, 250),
                    BorderStyle = BorderStyle.None
                };

                Label lblTitle = new Label
                {
                    Text = row["Title"].ToString(),
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(52, 73, 94),
                    Location = new Point(15, 10),
                    AutoSize = true
                };

                string dateStr = row["DatePosted"] != DBNull.Value
                    ? Convert.ToDateTime(row["DatePosted"]).ToString("dddd, MMMM dd, yyyy")
                    : "Date not available";

                Label lblDate = new Label
                {
                    Text = dateStr,
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.Gray,
                    Location = new Point(15, 35),
                    AutoSize = true
                };

                Label lblMessage = new Label
                {
                    Text = row["Message"].ToString(),
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.Black,
                    Location = new Point(15, 55),
                    AutoSize = true,
                    MaximumSize = new Size(500, 0)
                };

                card.Controls.Add(lblTitle);
                card.Controls.Add(lblDate);
                card.Controls.Add(lblMessage);
                contentPanel.Controls.Add(card);

                yPos += 115;
            }

            announcementForm.Controls.Add(contentPanel);

            Button btnClose = new Button
            {
                Text = "Close",
                Location = new Point(announcementForm.Width - 90, announcementForm.Height - 45),
                Size = new Size(70, 30),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClose.Click += (s, ev) => announcementForm.Close();
            announcementForm.Controls.Add(btnClose);

            announcementForm.ShowDialog();
        }

        private void frmFarmersDashboard_Resize(object sender, EventArgs e)
        {
            if (btnAnnouncements != null)
            {
                btnAnnouncements.Location = new Point(this.ClientSize.Width - 190, 25);
            }
        }

        private class FarmerStats
        {
            public decimal TotalKg { get; set; }
            public decimal TotalEarnings { get; set; }
            public decimal TotalAdvances { get; set; }
            public decimal NetPayable { get; set; }
        }
    }
}