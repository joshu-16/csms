using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace csms
{
    public partial class frmlogin : Form
    {
        // Form controls
        private TextBox txtusername;
        private TextBox txtpass;
        private Label lblError;
        private bool isPasswordVisible = false;

        public frmlogin()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.Text = "Login - Coffee Society Management System";

            CreateTitleBar();
            CreateLoginForm();
        }

        private Image LoadLargeIconImage()
        {
            try
            {
                string imagePath = @"C:\Users\JOSHUA\Downloads\Paisaje Cafetero en un grano de Café.png";

                if (File.Exists(imagePath))
                {
                    using (Image temp = Image.FromFile(imagePath))
                    {
                        return new Bitmap(temp, new Size(150, 150));
                    }
                }
            }
            catch (Exception)
            {
                // Fallback - create simple coffee cup icon
                Bitmap fallback = new Bitmap(150, 150);
                using (Graphics g = Graphics.FromImage(fallback))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.FromArgb(46, 125, 50));

                    // Create circular shape
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        path.AddEllipse(0, 0, 150, 150);
                        g.SetClip(path);
                        using (Font font = new Font("Segoe UI", 80, FontStyle.Bold))
                        {
                            g.DrawString("☕", font, Brushes.White, 35, 30);
                        }
                        g.ResetClip();
                    }

                    // Draw border
                    using (Pen borderPen = new Pen(Color.FromArgb(139, 69, 19), 3))
                    {
                        g.DrawEllipse(borderPen, 1, 1, 148, 148);
                    }
                }
                return fallback;
            }
            return null;
        }

        private Image LoadSmallIconImage()
        {
            try
            {
                string imagePath = @"C:\Users\JOSHUA\Downloads\Paisaje Cafetero en un grano de Café.png";

                if (File.Exists(imagePath))
                {
                    using (Image temp = Image.FromFile(imagePath))
                    {
                        return new Bitmap(temp, new Size(32, 32));
                    }
                }
            }
            catch (Exception)
            {
                Bitmap fallback = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(fallback))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.FromArgb(46, 125, 50));
                    using (Font font = new Font("Segoe UI", 16, FontStyle.Bold))
                    {
                        g.DrawString("☕", font, Brushes.White, 6, 4);
                    }
                }
                return fallback;
            }
            return null;
        }

        private void CreateTitleBar()
        {
            // Title bar panel
            Panel titleBar = new Panel
            {
                Height = 45,
                Dock = DockStyle.Top,
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // Icon picture box for title bar (small)
            PictureBox picAppIcon = new PictureBox
            {
                Size = new Size(32, 32),
                Location = new Point(15, 6),
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            picAppIcon.Image = LoadSmallIconImage();

            // Form title
            Label lblTitle = new Label
            {
                Text = "Login - Coffee Society Management System",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(55, 12),
                AutoSize = true
            };

            // Minimize button
            Button btnMinimize = new Button
            {
                Text = "─",
                Size = new Size(40, 40),
                Location = new Point(this.Width - 110, 2),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(100, 100, 100),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            btnMinimize.MouseEnter += (s, e) => btnMinimize.BackColor = Color.FromArgb(240, 240, 240);
            btnMinimize.MouseLeave += (s, e) => btnMinimize.BackColor = Color.Transparent;

            // Maximize/Restore button
            Button btnMaximize = new Button
            {
                Text = "□",
                Size = new Size(40, 40),
                Location = new Point(this.Width - 70, 2),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(100, 100, 100),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnMaximize.FlatAppearance.BorderSize = 0;
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
            btnMaximize.MouseEnter += (s, e) => btnMaximize.BackColor = Color.FromArgb(240, 240, 240);
            btnMaximize.MouseLeave += (s, e) => btnMaximize.BackColor = Color.Transparent;

            // Close button
            Button btnClose = new Button
            {
                Text = "X",
                Size = new Size(40, 40),
                Location = new Point(this.Width - 30, 2),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(100, 100, 100),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Application.Exit();
            btnClose.MouseEnter += (s, e) => { btnClose.BackColor = Color.Red; btnClose.ForeColor = Color.White; };
            btnClose.MouseLeave += (s, e) => { btnClose.BackColor = Color.Transparent; btnClose.ForeColor = Color.FromArgb(100, 100, 100); };

            titleBar.Controls.Add(picAppIcon);
            titleBar.Controls.Add(lblTitle);
            titleBar.Controls.Add(btnMinimize);
            titleBar.Controls.Add(btnMaximize);
            titleBar.Controls.Add(btnClose);
            this.Controls.Add(titleBar);

            // Handle resize to reposition title bar buttons
            this.Resize += (s, e) =>
            {
                btnMinimize.Location = new Point(this.Width - 110, 2);
                btnMaximize.Location = new Point(this.Width - 70, 2);
                btnClose.Location = new Point(this.Width - 30, 2);
            };
        }

        private void CreateLoginForm()
        {
            // Main background panel
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            // Centered login panel - increased height to accommodate larger icon
            Panel loginPanel = new Panel
            {
                Size = new Size(500, 580),
                BackColor = Color.White
            };
            loginPanel.Location = new Point((this.Width - 500) / 2, (this.Height - 580) / 2);
            loginPanel.Anchor = AnchorStyles.None;

            // Large Circular Icon (150x150 - Passport photo size) above Welcome Back text
            PictureBox picLargeIcon = new PictureBox
            {
                Size = new Size(150, 150),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Location = new Point(175, 25)
            };

            // Make the icon circular
            Bitmap iconImage = LoadLargeIconImage() as Bitmap;
            if (iconImage != null)
            {
                picLargeIcon.Image = MakeCircularImage(iconImage, 150);
            }

            // Title - moved down to accommodate larger icon
            Label lblWelcome = new Label
            {
                Text = "Welcome Back",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(125, 195),
                AutoSize = true
            };

            // Username field
            Label lblUsername = new Label
            {
                Text = "Username",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(50, 255),
                AutoSize = true
            };

            txtusername = new TextBox
            {
                Location = new Point(50, 280),
                Size = new Size(400, 35),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 249, 250),
                ForeColor = Color.Black
            };
            txtusername.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) PerformLogin(); };

            // Password field
            Label lblPassword = new Label
            {
                Text = "Password",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(50, 335),
                AutoSize = true
            };

            Panel passwordContainer = new Panel
            {
                Location = new Point(50, 360),
                Size = new Size(400, 35),
                BackColor = Color.FromArgb(248, 249, 250),
                BorderStyle = BorderStyle.FixedSingle
            };

            txtpass = new TextBox
            {
                Location = new Point(5, 5),
                Size = new Size(360, 25),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(248, 249, 250),
                ForeColor = Color.Black,
                PasswordChar = '●'
            };
            txtpass.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) PerformLogin(); };

            Button btnEye = new Button
            {
                Text = "👁",
                Size = new Size(30, 25),
                Location = new Point(365, 5),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.Gray,
                Cursor = Cursors.Hand
            };
            btnEye.FlatAppearance.BorderSize = 0;
            btnEye.Click += (s, e) =>
            {
                isPasswordVisible = !isPasswordVisible;
                txtpass.PasswordChar = isPasswordVisible ? '\0' : '●';
                btnEye.Text = isPasswordVisible ? "🙈" : "👁";
            };

            passwordContainer.Controls.Add(txtpass);
            passwordContainer.Controls.Add(btnEye);

            // Error label
            lblError = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Red,
                Location = new Point(50, 410),
                Size = new Size(400, 35),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            // Login button
            Button btnLogin = new Button
            {
                Text = "LOGIN",
                Size = new Size(400, 45),
                Location = new Point(50, 455),
                BackColor = Color.FromArgb(46, 125, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += (s, e) => PerformLogin();
            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = Color.FromArgb(56, 145, 60);
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = Color.FromArgb(46, 125, 50);

            // Back button
            Button btnBack = new Button
            {
                Text = "← Back to Welcome",
                Size = new Size(140, 30),
                Location = new Point(180, 520),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(100, 100, 100),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += (s, e) =>
            {
                Form1 welcome = new Form1();
                welcome.Show();
                this.Hide();
            };

            // Add all controls to login panel
            loginPanel.Controls.Add(picLargeIcon);
            loginPanel.Controls.Add(lblWelcome);
            loginPanel.Controls.Add(lblUsername);
            loginPanel.Controls.Add(txtusername);
            loginPanel.Controls.Add(lblPassword);
            loginPanel.Controls.Add(passwordContainer);
            loginPanel.Controls.Add(lblError);
            loginPanel.Controls.Add(btnLogin);
            loginPanel.Controls.Add(btnBack);

            mainPanel.Controls.Add(loginPanel);
            this.Controls.Add(mainPanel);

            // Handle window resize
            this.Resize += (s, e) =>
            {
                loginPanel.Location = new Point((this.Width - 500) / 2, (this.Height - 580) / 2);
            };
        }

        private Bitmap MakeCircularImage(Image sourceImage, int size)
        {
            Bitmap circularImage = new Bitmap(size, size);

            using (Graphics g = Graphics.FromImage(circularImage))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.Clear(Color.Transparent);

                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddEllipse(0, 0, size, size);
                    g.SetClip(path);

                    if (sourceImage != null)
                    {
                        g.DrawImage(sourceImage, 0, 0, size, size);
                    }
                    else
                    {
                        using (Font font = new Font("Segoe UI", size / 2, FontStyle.Bold))
                        {
                            g.Clear(Color.FromArgb(46, 125, 50));
                            g.DrawString("☕", font, Brushes.White, size / 4, size / 4);
                        }
                    }
                    g.ResetClip();
                }

                using (Pen borderPen = new Pen(Color.FromArgb(139, 69, 19), 3))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.DrawEllipse(borderPen, 1, 1, size - 2, size - 2);
                }
            }

            return circularImage;
        }

        private void PerformLogin()
        {
            lblError.Text = "";
            lblError.Visible = false;

            string username = txtusername.Text.Trim();
            string password = txtpass.Text;

            if (string.IsNullOrEmpty(username))
            {
                lblError.Text = "Please enter your username.";
                lblError.Visible = true;
                txtusername.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                lblError.Text = "Please enter your password.";
                lblError.Visible = true;
                txtpass.Focus();
                return;
            }

            string connStr = @"Server=DESKTOP-NLHET7V\SQLEXPRESS;Database=CSMS;Trusted_Connection=True;";

            using (SqlConnection con = new SqlConnection(connStr))
            {
                try
                {
                    con.Open();

                    // Check Users table - SQL Server is case-insensitive by default for Latin general
                    // Using COLLATE to make it case-sensitive
                    string query = "SELECT UserId, Username, Role FROM Users WHERE Username=@u AND Password=@p COLLATE SQL_Latin1_General_CP1_CS_AS";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@u", username);
                    cmd.Parameters.AddWithValue("@p", password);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        int userId = Convert.ToInt32(reader["UserId"]);
                        string role = reader["Role"].ToString();
                        string name = reader["Username"].ToString();
                        reader.Close();

                        this.Hide();

                        if (role == "Admin")
                        {
                            frmAdminDashboard admin = new frmAdminDashboard(userId, name);
                            admin.ShowDialog();
                        }
                        else if (role == "Clerk")
                        {
                            frmclerkdashboard clerk = new frmclerkdashboard(userId, name);
                            clerk.ShowDialog();
                        }
                        else if (role == "Accountant")
                        {
                            frmAccountantDashboard acc = new frmAccountantDashboard(userId, name);
                            acc.ShowDialog();
                        }

                        // After dashboard closes, show welcome page
                        Form1 welcome = new Form1();
                        welcome.Show();
                        this.Close();
                        return;
                    }
                    reader.Close();

                    // Check Farmers table - case-sensitive for National ID
                    string farmerQuery = "SELECT FarmerName FROM Farmers WHERE FarmerName=@name AND NationalID=@nid COLLATE SQL_Latin1_General_CP1_CS_AS";
                    SqlCommand farmerCmd = new SqlCommand(farmerQuery, con);
                    farmerCmd.Parameters.AddWithValue("@name", username);
                    farmerCmd.Parameters.AddWithValue("@nid", password);
                    SqlDataReader farmerReader = farmerCmd.ExecuteReader();

                    if (farmerReader.Read())
                    {
                        string farmerName = farmerReader["FarmerName"].ToString();
                        farmerReader.Close();

                        this.Hide();

                        frmFarmersDashboard farmer = new frmFarmersDashboard(farmerName);
                        farmer.ShowDialog();

                        // After dashboard closes, show welcome page
                        Form1 welcome = new Form1();
                        welcome.Show();
                        this.Close();
                        return;
                    }
                    farmerReader.Close();

                    lblError.Text = "Invalid username or password. Please try again.";
                    lblError.Visible = true;
                    txtpass.Clear();
                    txtpass.Focus();
                }
                catch (Exception ex)
                {
                    lblError.Text = "Database error: " + ex.Message;
                    lblError.Visible = true;
                }
            }
        }

        private void frmlogin_Load(object sender, EventArgs e)
        {

        }
    }
}