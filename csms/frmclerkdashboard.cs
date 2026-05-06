using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Font = System.Drawing.Font;

namespace csms
{
    public partial class frmclerkdashboard : Form
    {
        private string connStr = @"Server=DESKTOP-NLHET7V\SQLEXPRESS;Database=CSMS;Trusted_Connection=True;";
        private string loggedInName;
        private Panel contentPanel;
        private int loggedInUserId;

        public frmclerkdashboard(int userId, string name)
        {
            InitializeComponent();
            loggedInName = name;
            loggedInUserId = userId;
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.Text = "Clerk Dashboard";
            CreateDashboard();
        }

        private void CreateDashboard()
        {
            Panel titleBar = new Panel { Height = 50, Dock = DockStyle.Top, BackColor = Color.FromArgb(44, 62, 80) };
            Label lblTitle = new Label { Text = "CLERK DASHBOARD", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.White, Location = new Point(20, 15), AutoSize = true };
            Label lblUser = new Label { Text = $"Welcome, {loggedInName}", Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(200, 200, 200), Location = new Point(this.Width - 180, 15), AutoSize = true };

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

            Panel sidebar = new Panel { Width = 240, Dock = DockStyle.Left, BackColor = Color.FromArgb(52, 73, 94) };
            Label lblLogo = new Label { Text = "CSMS", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.White, Location = new Point(90, 30), AutoSize = true };
            sidebar.Controls.Add(lblLogo);

            string[] menuItems = { "Dashboard", "Record Delivery", "Record Advance", "Manage Farmers", "View Deliveries", "View Advances", "Logout" };
            int yPos = 100;

            for (int i = 0; i < menuItems.Length; i++)
            {
                int idx = i;
                Button btn = new Button
                {
                    Text = menuItems[idx],
                    FlatStyle = FlatStyle.Flat,
                    FlatAppearance = { BorderSize = 0 },
                    BackColor = Color.Transparent,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 11),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Size = new Size(240, 45),
                    Location = new Point(0, yPos),
                    Cursor = Cursors.Hand,
                    Tag = menuItems[idx]
                };
                btn.Click += (s, e) =>
                {
                    Button b = s as Button;
                    string menu = b.Tag.ToString();
                    contentPanel.Controls.Clear();

                    if (menu == "Dashboard") LoadDashboard();
                    else if (menu == "Record Delivery") LoadRecordDelivery();
                    else if (menu == "Record Advance") LoadRecordAdvance();
                    else if (menu == "Manage Farmers") LoadManageFarmers();
                    else if (menu == "View Deliveries") LoadViewDeliveries();
                    else if (menu == "View Advances") LoadViewAdvances();
                    else if (menu == "Logout")
                    {
                        this.Close();
                        frmlogin login = new frmlogin();
                        login.Show();
                    }
                };
                sidebar.Controls.Add(btn);
                yPos += 50;
            }

            Label lblVersion = new Label { Text = "Version 2.0", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(150, 150, 150), Location = new Point(90, this.Height - 60), AutoSize = true };
            sidebar.Controls.Add(lblVersion);
            this.Controls.Add(sidebar);

            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 242, 245),
                Padding = new Padding(20),
                AutoScroll = true
            };
            this.Controls.Add(contentPanel);
            contentPanel.BringToFront();

            LoadDashboard();

            this.Resize += (s, e) =>
            {
                lblUser.Location = new Point(this.Width - 180, 15);
                btnMinimize.Location = new Point(this.Width - 135, 2);
                btnMaximize.Location = new Point(this.Width - 90, 2);
                btnClose.Location = new Point(this.Width - 45, 2);
                lblVersion.Location = new Point(90, this.Height - 60);
            };
        }

        private void LoadDashboard()
        {
            contentPanel.Controls.Clear();

            Panel welcomeCard = new Panel { Width = contentPanel.Width - 40, Height = 80, BackColor = Color.White };
            Label lblWelcome = new Label { Text = $"Welcome back, {loggedInName}!", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.FromArgb(44, 62, 80), Location = new Point(20, 20), AutoSize = true };
            welcomeCard.Controls.Add(lblWelcome);
            contentPanel.Controls.Add(welcomeCard);

            FlowLayoutPanel statsPanel = new FlowLayoutPanel { Location = new Point(0, 100), Width = contentPanel.Width - 40, Height = 100 };

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Deliveries WHERE CAST(DeliveryDate AS DATE) = CAST(GETDATE() AS DATE)", conn);
                    int today = (int)cmd.ExecuteScalar();
                    cmd = new SqlCommand("SELECT COUNT(*) FROM Deliveries WHERE DeliveryDate >= DATEADD(day, -7, GETDATE())", conn);
                    int week = (int)cmd.ExecuteScalar();
                    cmd = new SqlCommand("SELECT COUNT(*) FROM FarmerAdvances WHERE DeductionStatus = 'Pending'", conn);
                    int advances = (int)cmd.ExecuteScalar();
                    cmd = new SqlCommand("SELECT COUNT(*) FROM Deliveries WHERE PaymentStatus = 'Pending'", conn);
                    int pending = (int)cmd.ExecuteScalar();

                    statsPanel.Controls.Add(CreateStatCard("Today's Deliveries", today.ToString(), Color.FromArgb(52, 152, 219)));
                    statsPanel.Controls.Add(CreateStatCard("This Week", week.ToString(), Color.FromArgb(46, 204, 113)));
                    statsPanel.Controls.Add(CreateStatCard("Total Advances", advances.ToString(), Color.FromArgb(241, 196, 15)));
                    statsPanel.Controls.Add(CreateStatCard("Pending Payments", pending.ToString(), Color.FromArgb(155, 89, 182)));
                }
            }
            catch
            {
                statsPanel.Controls.Add(CreateStatCard("Today's Deliveries", "0", Color.FromArgb(52, 152, 219)));
                statsPanel.Controls.Add(CreateStatCard("This Week", "0", Color.FromArgb(46, 204, 113)));
                statsPanel.Controls.Add(CreateStatCard("Total Advances", "0", Color.FromArgb(241, 196, 15)));
                statsPanel.Controls.Add(CreateStatCard("Pending Payments", "0", Color.FromArgb(155, 89, 182)));
            }
            contentPanel.Controls.Add(statsPanel);

            GroupBox grpActions = new GroupBox { Text = " Quick Actions ", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(0, 220), Size = new Size(contentPanel.Width - 40, 100), BackColor = Color.White };

            Button btnDelivery = new Button
            {
                Text = "Record Delivery",
                Size = new Size(160, 50),
                Location = new Point(30, 25),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDelivery.Click += (s, e) => LoadRecordDelivery();

            Button btnAdvance = new Button
            {
                Text = "Record Advance",
                Size = new Size(160, 50),
                Location = new Point(210, 25),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAdvance.Click += (s, e) => LoadRecordAdvance();

            Button btnFarmers = new Button
            {
                Text = "Manage Farmers",
                Size = new Size(160, 50),
                Location = new Point(390, 25),
                BackColor = Color.FromArgb(241, 196, 15),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnFarmers.Click += (s, e) => LoadManageFarmers();

            grpActions.Controls.Add(btnDelivery);
            grpActions.Controls.Add(btnAdvance);
            grpActions.Controls.Add(btnFarmers);
            contentPanel.Controls.Add(grpActions);
        }

        private void LoadRecordDelivery()
        {
            contentPanel.Controls.Clear();

            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White
            };

            GroupBox grp = new GroupBox { Text = " Record Coffee Delivery ", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(10, 10), Width = contentPanel.Width - 40, Height = 500, BackColor = Color.White, Padding = new Padding(25), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            int yPos = 25, labelX = 35, fieldX = 200, fieldWidth = 300;

            Label lblNID = new Label { Text = "Farmer National ID:", Location = new Point(labelX, yPos), AutoSize = true, Font = new Font("Segoe UI", 11) };
            TextBox txtNID = new TextBox { Location = new Point(fieldX, yPos - 3), Width = fieldWidth - 100, Height = 35, Font = new Font("Segoe UI", 11) };
            Button btnSearch = new Button { Text = "Search", Location = new Point(fieldX + fieldWidth - 90, yPos - 3), Size = new Size(80, 35), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            yPos += 55;

            Label lblName = new Label { Text = "Farmer Name:", Location = new Point(labelX, yPos), AutoSize = true, Font = new Font("Segoe UI", 11) };
            TextBox txtName = new TextBox { Location = new Point(fieldX, yPos - 3), Width = fieldWidth, Height = 35, ReadOnly = true, BackColor = Color.FromArgb(245, 245, 245), Font = new Font("Segoe UI", 11) };
            yPos += 55;

            Label lblWeight = new Label { Text = "Weight (KG):", Location = new Point(labelX, yPos), AutoSize = true, Font = new Font("Segoe UI", 11) };
            TextBox txtWeight = new TextBox { Location = new Point(fieldX, yPos - 3), Width = fieldWidth, Height = 35, Font = new Font("Segoe UI", 11) };
            yPos += 55;

            Label lblGrade = new Label { Text = "Coffee Grade:", Location = new Point(labelX, yPos), AutoSize = true, Font = new Font("Segoe UI", 11) };
            ComboBox cmbGrade = new ComboBox { Location = new Point(fieldX, yPos - 3), Width = fieldWidth, Height = 35, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11) };
            cmbGrade.Items.AddRange(new string[] { "A01", "A02", "B" });
            yPos += 70;

            Button btnSave = new Button { Text = "Save & Print Receipt", Location = new Point(fieldX, yPos), Size = new Size(180, 45), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
            Button btnClose = new Button { Text = "Close", Location = new Point(fieldX + 190, yPos), Size = new Size(100, 45), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };

            btnSearch.Click += (s, e) => LoadFarmerNameForDelivery(txtNID.Text, txtName);
            btnSave.Click += (s, e) => SaveDeliveryOnly(txtNID.Text, txtName.Text, txtWeight.Text, cmbGrade.SelectedItem?.ToString(), txtNID, txtName, txtWeight, cmbGrade);
            btnClose.Click += (s, e) => LoadDashboard();

            grp.Controls.AddRange(new Control[] { lblNID, txtNID, btnSearch, lblName, txtName, lblWeight, txtWeight, lblGrade, cmbGrade, btnSave, btnClose });
            mainPanel.Controls.Add(grp);
            contentPanel.Controls.Add(mainPanel);

            this.Resize += (s, e) =>
            {
                grp.Width = contentPanel.Width - 40;
            };
        }

        private void LoadRecordAdvance()
        {
            contentPanel.Controls.Clear();

            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White
            };

            GroupBox grp = new GroupBox { Text = " Record Advance ", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(10, 10), Width = contentPanel.Width - 40, Height = 600, BackColor = Color.White, Padding = new Padding(25), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            int yPos = 25, labelX = 35, fieldX = 200, fieldWidth = 300;
            Label lblNID = new Label { Text = "Farmer National ID:", Location = new Point(labelX, yPos), AutoSize = true, Font = new Font("Segoe UI", 11) };
            TextBox txtNID = new TextBox { Location = new Point(fieldX, yPos - 3), Width = fieldWidth - 100, Height = 35, Font = new Font("Segoe UI", 11) };
            Button btnSearch = new Button { Text = "Search", Location = new Point(fieldX + fieldWidth - 90, yPos - 3), Size = new Size(80, 35), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            yPos += 55;
            Label lblName = new Label { Text = "Farmer Name:", Location = new Point(labelX, yPos), AutoSize = true, Font = new Font("Segoe UI", 11) };
            TextBox txtName = new TextBox { Location = new Point(fieldX, yPos - 3), Width = fieldWidth, Height = 35, ReadOnly = true, BackColor = Color.FromArgb(245, 245, 245), Font = new Font("Segoe UI", 11) };
            yPos += 55;
            Label lblProduct = new Label { Text = "Product Type:", Location = new Point(labelX, yPos), AutoSize = true, Font = new Font("Segoe UI", 11) };
            ComboBox cmbProduct = new ComboBox { Location = new Point(fieldX, yPos - 3), Width = fieldWidth, Height = 35, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11) };
            cmbProduct.Items.AddRange(new string[] { "DAP Fertilizer (50kg)", "CAN Fertilizer (50kg)", "NPK Fertilizer (50kg)", "Organic Manure (50kg)", "Copper Fungicide (1L)", "Pesticide (1L)", "Weed Killer (5L)", "Pruning Shears", "Spray Pump", "Coffee Picking Basket" });
            yPos += 55;
            Label lblQty = new Label { Text = "Quantity:", Location = new Point(labelX, yPos), AutoSize = true, Font = new Font("Segoe UI", 11) };
            TextBox txtQty = new TextBox { Location = new Point(fieldX, yPos - 3), Width = fieldWidth, Height = 35, Font = new Font("Segoe UI", 11) };
            yPos += 55;
            Label lblPrice = new Label { Text = "Price Per Unit (KES):", Location = new Point(labelX, yPos), AutoSize = true, Font = new Font("Segoe UI", 11) };
            TextBox txtPrice = new TextBox { Location = new Point(fieldX, yPos - 3), Width = fieldWidth, Height = 35, Font = new Font("Segoe UI", 11) };
            yPos += 55;
            Label lblTotal = new Label { Text = "Total Amount:", Location = new Point(labelX, yPos), AutoSize = true, Font = new Font("Segoe UI", 11) };
            TextBox txtTotal = new TextBox { Location = new Point(fieldX, yPos - 3), Width = fieldWidth, Height = 35, ReadOnly = true, BackColor = Color.FromArgb(245, 245, 245), Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(52, 152, 219) };
            yPos += 70;
            Button btnSave = new Button { Text = "Save & Print Receipt", Location = new Point(fieldX, yPos), Size = new Size(180, 45), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
            Button btnClose = new Button { Text = "Close", Location = new Point(fieldX + 190, yPos), Size = new Size(100, 45), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };

            btnSearch.Click += (s, e) => LoadFarmerNameForAdvance(txtNID.Text, txtName);
            txtQty.TextChanged += (s, e) => CalculateTotal(txtQty, txtPrice, txtTotal);
            txtPrice.TextChanged += (s, e) => CalculateTotal(txtQty, txtPrice, txtTotal);
            btnSave.Click += (s, e) => SaveAdvanceOnly(txtNID.Text, txtName.Text, cmbProduct.SelectedItem?.ToString(), txtQty.Text, txtPrice.Text, txtTotal.Text, txtNID, txtName, cmbProduct, txtQty, txtPrice);
            btnClose.Click += (s, e) => LoadDashboard();

            grp.Controls.AddRange(new Control[] { lblNID, txtNID, btnSearch, lblName, txtName, lblProduct, cmbProduct, lblQty, txtQty, lblPrice, txtPrice, lblTotal, txtTotal, btnSave, btnClose });
            mainPanel.Controls.Add(grp);
            contentPanel.Controls.Add(mainPanel);

            this.Resize += (s, e) =>
            {
                grp.Width = contentPanel.Width - 40;
            };
        }

        private void LoadManageFarmers()
        {
            contentPanel.Controls.Clear();

            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White
            };

            GroupBox grp = new GroupBox { Text = " Manage Farmers ", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(10, 10), Width = contentPanel.Width - 40, Height = 550, BackColor = Color.White, Padding = new Padding(20), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            Button btnAdd = new Button { Text = "Add Farmer", Location = new Point(20, 25), Size = new Size(150, 40), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
            btnAdd.Click += (s, e) => ShowAddFarmerDialog();

            Label lblSearch = new Label { Text = "Search ID to Delete:", Location = new Point(20, 85), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            TextBox txtSearch = new TextBox { Location = new Point(20, 115), Width = 250, Height = 35, Font = new Font("Segoe UI", 11) };
            Button btnSearch = new Button { Text = "Search", Location = new Point(280, 113), Size = new Size(100, 35), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };

            DataGridView dgvFarmers = new DataGridView
            {
                Location = new Point(20, 170),
                Width = grp.Width - 50,
                Height = 320,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                Font = new Font("Segoe UI", 11),
                RowTemplate = { Height = 35 },
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            LoadFarmersGrid(dgvFarmers);

            int currentFarmerId = 0;
            Button btnDelete = new Button { Text = "Delete This Farmer", Location = new Point(20, 500), Size = new Size(150, 40), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand, Visible = false };

            btnSearch.Click += (s, e) =>
            {
                string nid = txtSearch.Text.Trim();
                if (string.IsNullOrEmpty(nid)) { MessageBox.Show("Enter National ID to search"); return; }
                try
                {
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("SELECT FarmerID, FarmerName, NationalID, Phone, FarmLocation FROM Farmers WHERE NationalID = @nid", conn);
                        cmd.Parameters.AddWithValue("@nid", nid);
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            currentFarmerId = Convert.ToInt32(reader["FarmerID"]);
                            btnDelete.Visible = true;
                            dgvFarmers.ClearSelection();
                            foreach (DataGridViewRow row in dgvFarmers.Rows)
                            {
                                if (row.Cells["FarmerID"].Value != null && Convert.ToInt32(row.Cells["FarmerID"].Value) == currentFarmerId)
                                {
                                    row.Selected = true;
                                    dgvFarmers.FirstDisplayedScrollingRowIndex = row.Index;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Farmer not found with this National ID");
                            btnDelete.Visible = false;
                            currentFarmerId = 0;
                        }
                        reader.Close();
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            };

            btnDelete.Click += (s, e) =>
            {
                if (currentFarmerId == 0) return;
                DialogResult result = MessageBox.Show("Delete this farmer?\n\nAll their deliveries and advances will also be deleted.", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    DeleteFarmer(currentFarmerId);
                    LoadFarmersGrid(dgvFarmers);
                    btnDelete.Visible = false;
                    txtSearch.Clear();
                    currentFarmerId = 0;
                    MessageBox.Show("Farmer deleted successfully!");
                }
            };

            grp.Controls.Add(btnAdd);
            grp.Controls.Add(lblSearch);
            grp.Controls.Add(txtSearch);
            grp.Controls.Add(btnSearch);
            grp.Controls.Add(dgvFarmers);
            grp.Controls.Add(btnDelete);

            mainPanel.Controls.Add(grp);
            contentPanel.Controls.Add(mainPanel);

            this.Resize += (s, e) =>
            {
                grp.Width = contentPanel.Width - 40;
                dgvFarmers.Width = grp.Width - 50;
            };
        }

        private void LoadViewDeliveries()
        {
            contentPanel.Controls.Clear();

            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White
            };

            GroupBox grp = new GroupBox
            {
                Text = " View Deliveries ",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, 10),
                Width = contentPanel.Width - 40,
                Height = 650,
                BackColor = Color.White,
                Padding = new Padding(20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblSearch = new Label { Text = "Farmer National ID:", Location = new Point(20, 25), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            TextBox txtSearch = new TextBox { Location = new Point(180, 22), Width = 250, Height = 35, Font = new Font("Segoe UI", 12) };
            Button btnSearch = new Button { Text = "Search", Location = new Point(450, 20), Size = new Size(100, 35), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };

            GroupBox grpFarmerDetails = new GroupBox
            {
                Text = " Farmer Information ",
                Location = new Point(20, 75),
                Size = new Size(grp.Width - 50, 110),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(248, 249, 250)
            };

            Label lblFarmerName = new Label { Text = "Name: ", Location = new Point(15, 30), AutoSize = true, Font = new Font("Segoe UI", 11) };
            Label lblFarmerNID = new Label { Text = "National ID: ", Location = new Point(15, 55), AutoSize = true, Font = new Font("Segoe UI", 11) };
            Label lblFarmerPhone = new Label { Text = "Phone: ", Location = new Point(15, 80), AutoSize = true, Font = new Font("Segoe UI", 11) };

            grpFarmerDetails.Controls.Add(lblFarmerName);
            grpFarmerDetails.Controls.Add(lblFarmerNID);
            grpFarmerDetails.Controls.Add(lblFarmerPhone);

            DataGridView dgv = new DataGridView
            {
                Location = new Point(20, 200),
                Width = grp.Width - 50,
                Height = 320,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                Font = new Font("Segoe UI", 11),
                RowTemplate = { Height = 35 },
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblTotalWeight = new Label
            {
                Text = "Total Weight: 0 KG",
                Location = new Point(20, 535),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 125, 50)
            };

            Button btnPrint = new Button { Text = "Print Summary Report (PDF)", Location = new Point(20, 575), Size = new Size(200, 45), BackColor = Color.FromArgb(155, 89, 182), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };

            btnSearch.Click += (s, e) => LoadFarmerDeliveries(txtSearch.Text, lblFarmerName, lblFarmerNID, lblFarmerPhone, dgv, lblTotalWeight);
            btnPrint.Click += (s, e) => PrintDeliveriesReport(dgv, lblFarmerName.Text);

            grp.Controls.Add(lblSearch);
            grp.Controls.Add(txtSearch);
            grp.Controls.Add(btnSearch);
            grp.Controls.Add(grpFarmerDetails);
            grp.Controls.Add(dgv);
            grp.Controls.Add(lblTotalWeight);
            grp.Controls.Add(btnPrint);

            mainPanel.Controls.Add(grp);
            contentPanel.Controls.Add(mainPanel);

            this.Resize += (s, e) =>
            {
                grp.Width = contentPanel.Width - 40;
                dgv.Width = grp.Width - 50;
                grpFarmerDetails.Width = grp.Width - 50;
            };
        }

        private void LoadViewAdvances()
        {
            contentPanel.Controls.Clear();

            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White
            };

            GroupBox grp = new GroupBox
            {
                Text = " View Advances ",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, 10),
                Width = contentPanel.Width - 40,
                Height = 650,
                BackColor = Color.White,
                Padding = new Padding(20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label lblSearch = new Label
            {
                Text = "Farmer National ID:",
                Location = new Point(20, 25),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };

            TextBox txtSearch = new TextBox
            {
                Location = new Point(180, 22),
                Width = 250,
                Height = 35,
                Font = new Font("Segoe UI", 12),
                BorderStyle = BorderStyle.FixedSingle
            };

            Button btnSearch = new Button
            {
                Text = "Search",
                Location = new Point(450, 20),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            GroupBox grpFarmerDetails = new GroupBox
            {
                Text = " Farmer Information ",
                Location = new Point(20, 75),
                Size = new Size(grp.Width - 50, 110),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(248, 249, 250)
            };

            Label lblFarmerName = new Label
            {
                Text = "Name: ",
                Location = new Point(15, 30),
                AutoSize = true,
                Font = new Font("Segoe UI", 11)
            };

            Label lblFarmerNID = new Label
            {
                Text = "National ID: ",
                Location = new Point(15, 55),
                AutoSize = true,
                Font = new Font("Segoe UI", 11)
            };

            Label lblFarmerPhone = new Label
            {
                Text = "Phone: ",
                Location = new Point(15, 80),
                AutoSize = true,
                Font = new Font("Segoe UI", 11)
            };

            grpFarmerDetails.Controls.Add(lblFarmerName);
            grpFarmerDetails.Controls.Add(lblFarmerNID);
            grpFarmerDetails.Controls.Add(lblFarmerPhone);

            DataGridView dgv = new DataGridView
            {
                Location = new Point(20, 200),
                Width = grp.Width - 50,
                Height = 320,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                Font = new Font("Segoe UI", 11),
                RowTemplate = { Height = 35 },
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 40;

            Label lblTotalAmount = new Label
            {
                Text = "Total Amount: KES 0.00",
                Location = new Point(20, 535),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 125, 50)
            };

            Button btnPrint = new Button
            {
                Text = "Print Summary Report (PDF)",
                Location = new Point(20, 575),
                Size = new Size(200, 45),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnSearch.Click += (s, e) => LoadFarmerAdvances(txtSearch.Text, lblFarmerName, lblFarmerNID, lblFarmerPhone, dgv, lblTotalAmount);
            btnPrint.Click += (s, e) => PrintAdvancesReport(dgv, lblFarmerName.Text);

            grp.Controls.Add(lblSearch);
            grp.Controls.Add(txtSearch);
            grp.Controls.Add(btnSearch);
            grp.Controls.Add(grpFarmerDetails);
            grp.Controls.Add(dgv);
            grp.Controls.Add(lblTotalAmount);
            grp.Controls.Add(btnPrint);

            mainPanel.Controls.Add(grp);
            contentPanel.Controls.Add(mainPanel);

            this.Resize += (s, e) =>
            {
                grp.Width = contentPanel.Width - 40;
                dgv.Width = grp.Width - 50;
                grpFarmerDetails.Width = grp.Width - 50;
            };
        }

        private void LoadFarmerNameForDelivery(string nid, TextBox txtName)
        {
            if (string.IsNullOrEmpty(nid))
            {
                txtName.Text = "";
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT FarmerName FROM Farmers WHERE NationalID = @nid", conn);
                    cmd.Parameters.AddWithValue("@nid", nid);
                    object result = cmd.ExecuteScalar();
                    txtName.Text = result?.ToString() ?? "";

                    if (string.IsNullOrEmpty(txtName.Text))
                    {
                        MessageBox.Show("Farmer not found with this National ID", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void LoadFarmerNameForAdvance(string nid, TextBox txtName)
        {
            if (string.IsNullOrEmpty(nid))
            {
                txtName.Text = "";
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT FarmerName FROM Farmers WHERE NationalID = @nid", conn);
                    cmd.Parameters.AddWithValue("@nid", nid);
                    object result = cmd.ExecuteScalar();
                    txtName.Text = result?.ToString() ?? "";

                    if (string.IsNullOrEmpty(txtName.Text))
                    {
                        MessageBox.Show("Farmer not found with this National ID", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void CalculateTotal(TextBox txtQty, TextBox txtPrice, TextBox txtTotal)
        {
            if (decimal.TryParse(txtQty.Text, out decimal qty) && decimal.TryParse(txtPrice.Text, out decimal price))
            {
                txtTotal.Text = (qty * price).ToString("N2");
            }
            else txtTotal.Text = "";
        }

        private void SaveDeliveryOnly(string nid, string farmerName, string weight, string grade, TextBox txtNID, TextBox txtName, TextBox txtWeight, ComboBox cmbGrade)
        {
            if (string.IsNullOrEmpty(nid) || string.IsNullOrEmpty(farmerName))
            {
                MessageBox.Show("Please search and select a valid Farmer National ID first", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(weight, out decimal w) || w <= 0)
            {
                MessageBox.Show("Enter valid weight", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(grade))
            {
                MessageBox.Show("Select coffee grade", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT FarmerID FROM Farmers WHERE NationalID = @nid", conn);
                    cmd.Parameters.AddWithValue("@nid", nid);
                    int farmerId = (int)cmd.ExecuteScalar();

                    string insertQuery = @"INSERT INTO Deliveries (FarmerID, DeliveryDate, WeightKg, CoffeeGrade, RecordedBy, PaymentStatus) 
                                           VALUES (@fid, @date, @w, @g, @RecordedBy, 'Pending'); SELECT SCOPE_IDENTITY();";
                    cmd = new SqlCommand(insertQuery, conn);
                    cmd.Parameters.AddWithValue("@fid", farmerId);
                    cmd.Parameters.AddWithValue("@date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@w", w);
                    cmd.Parameters.AddWithValue("@g", grade);
                    cmd.Parameters.AddWithValue("@recordedBy", loggedInUserId);
                    int deliveryId = Convert.ToInt32(cmd.ExecuteScalar());

                    GeneratePOSDeliveryReceipt(farmerName, nid, weight, grade, deliveryId);

                    MessageBox.Show("Delivery recorded successfully! Receipt has been generated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    txtNID.Clear();
                    txtName.Clear();
                    txtWeight.Clear();
                    cmbGrade.SelectedIndex = -1;
                    txtNID.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveAdvanceOnly(string nid, string farmerName, string product, string qty, string price, string total, TextBox txtNID, TextBox txtName, ComboBox cmbProduct, TextBox txtQty, TextBox txtPrice)
        {
            if (string.IsNullOrEmpty(nid) || string.IsNullOrEmpty(farmerName))
            {
                MessageBox.Show("Please search and select a valid Farmer National ID first", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(product))
            {
                MessageBox.Show("Select product", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(qty, out decimal quantity) || quantity <= 0)
            {
                MessageBox.Show("Enter valid quantity", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(price, out decimal unitPrice) || unitPrice <= 0)
            {
                MessageBox.Show("Enter valid price", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT FarmerID FROM Farmers WHERE NationalID = @nid", conn);
                    cmd.Parameters.AddWithValue("@nid", nid);
                    int farmerId = (int)cmd.ExecuteScalar();

                    string insertQuery = @"INSERT INTO FarmerAdvances (FarmerID, DateTaken, ProductType, Quantity, PricePerUnit, TotalAmount, RecordedBy, DeductionStatus) 
                                           VALUES (@fid, @date, @p, @q, @pr, @t, @UserID, 'Pending'); SELECT SCOPE_IDENTITY();";
                    cmd = new SqlCommand(insertQuery, conn);
                    cmd.Parameters.AddWithValue("@fid", farmerId);
                    cmd.Parameters.AddWithValue("@date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@p", product);
                    cmd.Parameters.AddWithValue("@q", quantity);
                    cmd.Parameters.AddWithValue("@pr", unitPrice);
                    cmd.Parameters.AddWithValue("@t", decimal.Parse(total));
                    cmd.Parameters.AddWithValue("@userId", loggedInUserId);
                    int advanceId = Convert.ToInt32(cmd.ExecuteScalar());

                    GeneratePOSAdvanceReceipt(farmerName, nid, product, qty, price, total, advanceId);

                    MessageBox.Show("Advance recorded successfully! Receipt has been generated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    txtNID.Clear();
                    txtName.Clear();
                    cmbProduct.SelectedIndex = -1;
                    txtQty.Clear();
                    txtPrice.Clear();
                    txtNID.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GeneratePOSDeliveryReceipt(string farmerName, string nid, string weight, string grade, int deliveryId)
        {
            try
            {
                // Get Documents folder path
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string folder = Path.Combine(documentsPath, "CSMS_Receipts");

                // Create folder if it doesn't exist
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string filePath = Path.Combine(folder, $"Delivery_Receipt_{deliveryId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

                // Show where it's trying to save
                MessageBox.Show($"Attempting to save receipt to:\n{filePath}", "Saving Receipt", MessageBoxButtons.OK, MessageBoxIcon.Information);

                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    Document doc = new Document(new iTextSharp.text.Rectangle(280f, 350f));
                    PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                    doc.Open();

                    iTextSharp.text.Font boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                    iTextSharp.text.Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                    iTextSharp.text.Font smallFont = FontFactory.GetFont(FontFactory.HELVETICA, 7);
                    iTextSharp.text.Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);

                    doc.Add(new Paragraph("SUBUKIA COFFEE SOCIETY", titleFont) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(new Paragraph("P.O Box 123-00200, Nairobi", smallFont) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(new Paragraph("Tel: 0712345678", smallFont) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(new Paragraph("DELIVERY RECEIPT", boldFont) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(new Paragraph("=================================", normalFont) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(new Paragraph($"Receipt No: DEL-{deliveryId}", normalFont));
                    doc.Add(new Paragraph($"Date: {DateTime.Now:dd/MM/yyyy HH:mm}", normalFont));
                    doc.Add(new Paragraph("---------------------------------", normalFont));
                    doc.Add(new Paragraph($"Farmer: {farmerName}", normalFont));
                    doc.Add(new Paragraph($"ID Number: {nid}", normalFont));
                    doc.Add(new Paragraph("---------------------------------", normalFont));
                    doc.Add(new Paragraph($"Coffee Grade: {grade}", normalFont));
                    doc.Add(new Paragraph($"Weight: {weight} KG", normalFont));
                    doc.Add(new Paragraph("---------------------------------", normalFont));
                    doc.Add(new Paragraph($"Recorded By: {loggedInName}", smallFont));
                    doc.Add(new Paragraph("=================================", normalFont) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(new Paragraph("Thank you for your delivery", smallFont) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(new Paragraph("This is a system generated receipt", smallFont) { Alignment = Element.ALIGN_CENTER });

                    doc.Close();
                    writer.Close();
                }

                // Verify file was created
                if (File.Exists(filePath))
                {
                    MessageBox.Show($"Receipt saved successfully!\nLocation: {filePath}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = filePath, UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show($"Receipt file was not created at:\n{filePath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating receipt:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Receipt Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GeneratePOSAdvanceReceipt(string farmerName, string nid, string product, string qty, string price, string total, int advanceId)
        {
            try
            {
                // Get Documents folder path
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string folder = Path.Combine(documentsPath, "CSMS_Receipts");

                // Create folder if it doesn't exist
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string filePath = Path.Combine(folder, $"Advance_Receipt_{advanceId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

                // Show where it's trying to save
                MessageBox.Show($"Attempting to save receipt to:\n{filePath}", "Saving Receipt", MessageBoxButtons.OK, MessageBoxIcon.Information);

                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    Document doc = new Document(new iTextSharp.text.Rectangle(280f, 450f));
                    PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                    doc.Open();

                    iTextSharp.text.Font boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                    iTextSharp.text.Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                    iTextSharp.text.Font smallFont = FontFactory.GetFont(FontFactory.HELVETICA, 7);
                    iTextSharp.text.Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);

                    doc.Add(new Paragraph("SUBUKIA COFFEE SOCIETY", titleFont) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(new Paragraph("P.O Box 123-00200, Nairobi", smallFont) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(new Paragraph("Tel: 0712345678", smallFont) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(new Paragraph("ADVANCE RECEIPT", boldFont) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(new Paragraph("=================================", normalFont) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(new Paragraph($"Receipt No: ADV-{advanceId}", normalFont));
                    doc.Add(new Paragraph($"Date: {DateTime.Now:dd/MM/yyyy HH:mm}", normalFont));
                    doc.Add(new Paragraph("---------------------------------", normalFont));
                    doc.Add(new Paragraph($"Farmer: {farmerName}", normalFont));
                    doc.Add(new Paragraph($"ID Number: {nid}", normalFont));
                    doc.Add(new Paragraph("---------------------------------", normalFont));
                    doc.Add(new Paragraph($"Product: {product}", normalFont));
                    doc.Add(new Paragraph($"Quantity: {qty}", normalFont));
                    doc.Add(new Paragraph($"Unit Price: KES {decimal.Parse(price):N2}", normalFont));
                    doc.Add(new Paragraph("---------------------------------", normalFont));
                    doc.Add(new Paragraph($"TOTAL AMOUNT: KES {decimal.Parse(total):N2}", boldFont));
                    doc.Add(new Paragraph("---------------------------------", normalFont));
                    doc.Add(new Paragraph($"Recorded By: {loggedInName}", smallFont));
                    doc.Add(new Paragraph("=================================", normalFont) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(new Paragraph("Thank you for your patronage", smallFont) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(new Paragraph("This is a system generated receipt", smallFont) { Alignment = Element.ALIGN_CENTER });

                    doc.Close();
                    writer.Close();
                }

                // Verify file was created
                if (File.Exists(filePath))
                {
                    MessageBox.Show($"Receipt saved successfully!\nLocation: {filePath}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = filePath, UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show($"Receipt file was not created at:\n{filePath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating receipt:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Receipt Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadFarmersGrid(DataGridView dgv)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter("SELECT FarmerID, FarmerName, NationalID, FarmLocation, Phone FROM Farmers ORDER BY FarmerName", conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgv.DataSource = dt;

                    if (dgv.Columns["FarmerID"] != null)
                        dgv.Columns["FarmerID"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading farmers: " + ex.Message);
            }
        }

        private void ShowAddFarmerDialog()
        {
            Form dialog = new Form { Text = "Add New Farmer", Size = new Size(500, 420), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false };
            int yPos = 25;
            TextBox txtName = new TextBox { Location = new Point(150, yPos), Width = 280, Height = 32 };
            yPos += 50;
            TextBox txtNID = new TextBox { Location = new Point(150, yPos), Width = 280, Height = 32 };
            yPos += 50;
            TextBox txtPhone = new TextBox { Location = new Point(150, yPos), Width = 280, Height = 32 };
            yPos += 50;
            TextBox txtLocation = new TextBox { Location = new Point(150, yPos), Width = 280, Height = 32 };
            yPos += 60;
            Button btnSave = new Button { Text = "Save Farmer", Location = new Point(150, yPos), Size = new Size(130, 40), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };

            dialog.Controls.Add(new Label { Text = "Farmer Name:", Location = new Point(30, 28), AutoSize = true });
            dialog.Controls.Add(txtName);
            dialog.Controls.Add(new Label { Text = "National ID:", Location = new Point(30, 78), AutoSize = true });
            dialog.Controls.Add(txtNID);
            dialog.Controls.Add(new Label { Text = "Phone:", Location = new Point(30, 128), AutoSize = true });
            dialog.Controls.Add(txtPhone);
            dialog.Controls.Add(new Label { Text = "Farm Location:", Location = new Point(30, 178), AutoSize = true });
            dialog.Controls.Add(txtLocation);
            dialog.Controls.Add(btnSave);

            btnSave.Click += (ev, ea) =>
            {
                if (string.IsNullOrEmpty(txtName.Text) || string.IsNullOrEmpty(txtNID.Text))
                {
                    MessageBox.Show("Name and National ID required");
                    return;
                }
                try
                {
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("INSERT INTO Farmers (FarmerName, NationalID, Phone, FarmLocation, DateRegistered) VALUES (@n, @id, @p, @l, @d)", conn);
                        cmd.Parameters.AddWithValue("@n", txtName.Text);
                        cmd.Parameters.AddWithValue("@id", txtNID.Text);
                        cmd.Parameters.AddWithValue("@p", txtPhone.Text);
                        cmd.Parameters.AddWithValue("@l", txtLocation.Text);
                        cmd.Parameters.AddWithValue("@d", DateTime.Now);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Farmer added successfully!");
                        dialog.Close();
                        LoadManageFarmers();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            };
            dialog.ShowDialog();
        }

        private void DeleteFarmer(int farmerId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("DELETE FROM Deliveries WHERE FarmerID = @id", conn);
                    cmd.Parameters.AddWithValue("@id", farmerId);
                    cmd.ExecuteNonQuery();
                    cmd = new SqlCommand("DELETE FROM FarmerAdvances WHERE FarmerID = @id", conn);
                    cmd.Parameters.AddWithValue("@id", farmerId);
                    cmd.ExecuteNonQuery();
                    cmd = new SqlCommand("DELETE FROM Farmers WHERE FarmerID = @id", conn);
                    cmd.Parameters.AddWithValue("@id", farmerId);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting farmer: " + ex.Message);
            }
        }

        private void LoadFarmerDeliveries(string nid, Label lblName, Label lblNID, Label lblPhone, DataGridView dgv, Label lblTotalWeight)
        {
            if (string.IsNullOrEmpty(nid))
            {
                MessageBox.Show("Please enter National ID", "Search", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        MessageBox.Show("Farmer not found with this National ID", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        lblName.Text = "Name: ";
                        lblNID.Text = "National ID: ";
                        lblPhone.Text = "Phone: ";
                        dgv.DataSource = null;
                        lblTotalWeight.Text = "Total Weight: 0 KG";
                        return;
                    }

                    reader.Read();
                    int farmerId = Convert.ToInt32(reader["FarmerID"]);
                    lblName.Text = $"Name: {reader["FarmerName"].ToString()}";
                    lblNID.Text = $"National ID: {reader["NationalID"].ToString()}";
                    lblPhone.Text = $"Phone: {(reader["Phone"] != DBNull.Value ? reader["Phone"].ToString() : "N/A")}";
                    reader.Close();

                    SqlDataAdapter adapter = new SqlDataAdapter(
                        "SELECT DeliveryDate, WeightKg, CoffeeGrade FROM Deliveries WHERE FarmerID = @fid ORDER BY DeliveryDate DESC", conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@fid", farmerId);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgv.DataSource = dt;

                    decimal totalWeight = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        totalWeight += Convert.ToDecimal(row["WeightKg"]);
                    }
                    lblTotalWeight.Text = $"Total Weight: {totalWeight} KG";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadFarmerAdvances(string nid, Label lblName, Label lblNID, Label lblPhone, DataGridView dgv, Label lblTotalAmount)
        {
            if (string.IsNullOrEmpty(nid))
            {
                MessageBox.Show("Please enter National ID", "Search", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        MessageBox.Show("Farmer not found with this National ID", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        lblName.Text = "Name: ";
                        lblNID.Text = "National ID: ";
                        lblPhone.Text = "Phone: ";
                        dgv.DataSource = null;
                        lblTotalAmount.Text = "Total Amount: KES 0.00";
                        return;
                    }

                    reader.Read();
                    int farmerId = Convert.ToInt32(reader["FarmerID"]);
                    lblName.Text = $"Name: {reader["FarmerName"].ToString()}";
                    lblNID.Text = $"National ID: {reader["NationalID"].ToString()}";
                    lblPhone.Text = $"Phone: {(reader["Phone"] != DBNull.Value ? reader["Phone"].ToString() : "N/A")}";
                    reader.Close();

                    SqlDataAdapter adapter = new SqlDataAdapter(
                        "SELECT DateTaken, ProductType, Quantity, PricePerUnit, TotalAmount FROM FarmerAdvances WHERE FarmerID = @fid ORDER BY DateTaken DESC", conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@fid", farmerId);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgv.DataSource = dt;

                    decimal totalAmount = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        totalAmount += Convert.ToDecimal(row["TotalAmount"]);
                    }
                    lblTotalAmount.Text = $"Total Amount: KES {totalAmount:N2}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintDeliveriesReport(DataGridView dgv, string farmerInfo)
        {
            if (dgv.Rows.Count == 0)
            {
                MessageBox.Show("No deliveries to print");
                return;
            }

            try
            {
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string folder = Path.Combine(documentsPath, "CSMS_Reports");
                Directory.CreateDirectory(folder);

                string fileName = farmerInfo.Replace("Name: ", "").Replace(" ", "_");
                string filePath = Path.Combine(folder, $"Deliveries_{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

                Document doc = new Document(PageSize.A4);
                PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                doc.Open();

                iTextSharp.text.Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                iTextSharp.text.Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                iTextSharp.text.Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                iTextSharp.text.Font grandTotalFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);

                doc.Add(new Paragraph("SUBUKIA COFFEE SOCIETY", titleFont) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph("Deliveries Summary Report", headerFont) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph($"{farmerInfo}", normalFont));
                doc.Add(new Paragraph($"Generated: {DateTime.Now:dd/MM/yyyy HH:mm}", normalFont));
                doc.Add(new Paragraph("\n"));

                decimal grandTotalWeight = 0;
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.Cells["WeightKg"].Value != null)
                    {
                        grandTotalWeight += Convert.ToDecimal(row.Cells["WeightKg"].Value);
                    }
                }

                PdfPTable grandTotalTable = new PdfPTable(2);
                grandTotalTable.WidthPercentage = 50;
                grandTotalTable.HorizontalAlignment = Element.ALIGN_RIGHT;

                PdfPCell totalLabelCell = new PdfPCell(new Phrase("GRAND TOTAL WEIGHT:", grandTotalFont));
                totalLabelCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                totalLabelCell.Padding = 5;
                grandTotalTable.AddCell(totalLabelCell);

                PdfPCell totalValueCell = new PdfPCell(new Phrase($"{grandTotalWeight} KG", grandTotalFont));
                totalValueCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                totalValueCell.Padding = 5;
                grandTotalTable.AddCell(totalValueCell);

                doc.Add(grandTotalTable);
                doc.Add(new Paragraph("\n"));

                PdfPTable table = new PdfPTable(dgv.Columns.Count);
                table.WidthPercentage = 100;

                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(col.HeaderText, headerFont));
                    cell.BackgroundColor = new BaseColor(52, 73, 94);
                    table.AddCell(cell);
                }

                foreach (DataGridViewRow row in dgv.Rows)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        table.AddCell(cell.Value?.ToString() ?? "");
                    }
                }
                doc.Add(table);
                doc.Close();

                MessageBox.Show($"Report saved to:\n{filePath}", "Report Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = filePath, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating report: " + ex.Message);
            }
        }

        private void PrintAdvancesReport(DataGridView dgv, string farmerInfo)
        {
            if (dgv.Rows.Count == 0)
            {
                MessageBox.Show("No advances to print");
                return;
            }

            try
            {
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string folder = Path.Combine(documentsPath, "CSMS_Reports");
                Directory.CreateDirectory(folder);

                string fileName = farmerInfo.Replace("Name: ", "").Replace(" ", "_");
                string filePath = Path.Combine(folder, $"Advances_{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

                Document doc = new Document(PageSize.A4);
                PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                doc.Open();

                iTextSharp.text.Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                iTextSharp.text.Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                iTextSharp.text.Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                iTextSharp.text.Font grandTotalFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);

                doc.Add(new Paragraph("SUBUKIA COFFEE SOCIETY", titleFont) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph("Advances Summary Report", headerFont) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph($"{farmerInfo}", normalFont));
                doc.Add(new Paragraph($"Generated: {DateTime.Now:dd/MM/yyyy HH:mm}", normalFont));
                doc.Add(new Paragraph("\n"));

                decimal grandTotalAmount = 0;
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.Cells["TotalAmount"].Value != null)
                    {
                        grandTotalAmount += Convert.ToDecimal(row.Cells["TotalAmount"].Value);
                    }
                }

                PdfPTable grandTotalTable = new PdfPTable(2);
                grandTotalTable.WidthPercentage = 50;
                grandTotalTable.HorizontalAlignment = Element.ALIGN_RIGHT;

                PdfPCell totalLabelCell = new PdfPCell(new Phrase("GRAND TOTAL AMOUNT:", grandTotalFont));
                totalLabelCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                totalLabelCell.Padding = 5;
                grandTotalTable.AddCell(totalLabelCell);

                PdfPCell totalValueCell = new PdfPCell(new Phrase($"KES {grandTotalAmount:N2}", grandTotalFont));
                totalValueCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                totalValueCell.Padding = 5;
                grandTotalTable.AddCell(totalValueCell);

                doc.Add(grandTotalTable);
                doc.Add(new Paragraph("\n"));

                PdfPTable table = new PdfPTable(dgv.Columns.Count);
                table.WidthPercentage = 100;

                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(col.HeaderText, headerFont));
                    cell.BackgroundColor = new BaseColor(52, 73, 94);
                    table.AddCell(cell);
                }

                foreach (DataGridViewRow row in dgv.Rows)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        table.AddCell(cell.Value?.ToString() ?? "");
                    }
                }
                doc.Add(table);
                doc.Close();

                MessageBox.Show($"Report saved to:\n{filePath}", "Report Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = filePath, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating report: " + ex.Message);
            }
        }

        private Panel CreateStatCard(string title, string value, Color color)
        {
            Panel card = new Panel { Size = new Size(200, 80), BackColor = Color.White, Margin = new Padding(0, 0, 15, 0) };
            Label lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 11), ForeColor = Color.Gray, Location = new Point(15, 15), AutoSize = true };
            Label lblValue = new Label { Text = value, Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = color, Location = new Point(15, 45), AutoSize = true };
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);
            return card;
        }

        private void frmclerkdashboard_Load(object sender, EventArgs e)
        {

        }
    }
}