using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;

namespace csms
{
    public partial class Form1 : Form
    {
        private Image highQualityIcon;
        private Button btnStart;
        private Panel separator;
        private PictureBox picCircularIcon;
        private Label lblVersion;
        private Button btnMinimize;
        private Button btnMaximize;
        private Button btnClose;
        private Label lblMainTitle;
        private Label lblSubTitle;
        private Label lblTagline;

        public Form1()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.Text = "Coffee Society Management System";

            LoadHighQualityIcon();
            SetupWelcomePage();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Required by designer - kept empty
        }

        private void LoadHighQualityIcon()
        {
            try
            {
                string imagePath = @"C:\Users\JOSHUA\Downloads\Paisaje Cafetero en un grano de Café.png";

                if (File.Exists(imagePath))
                {
                    using (Image temp = Image.FromFile(imagePath))
                    {
                        highQualityIcon = new Bitmap(temp, new Size(256, 256));
                    }
                }
                else
                {
                    highQualityIcon = CreateHighQualityFallbackIcon();
                }
            }
            catch (Exception)
            {
                highQualityIcon = CreateHighQualityFallbackIcon();
            }
        }

        private Image CreateHighQualityFallbackIcon()
        {
            Bitmap bmp = new Bitmap(256, 256);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.FromArgb(46, 125, 50));
                using (Font font = new Font("Segoe UI", 120, FontStyle.Bold))
                {
                    g.DrawString("☕", font, Brushes.White, 60, 60);
                }
            }
            return bmp;
        }

        private void SetupWelcomePage()
        {
            // Title Bar
            Panel titleBar = new Panel
            {
                Height = 45,
                Dock = DockStyle.Top,
                BackColor = Color.White
            };

            PictureBox picAppIconSmall = new PictureBox
            {
                Size = new Size(32, 32),
                Location = new Point(15, 6),
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            if (highQualityIcon != null)
            {
                Bitmap smallIcon = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(smallIcon))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(highQualityIcon, 0, 0, 32, 32);
                }
                picAppIconSmall.Image = smallIcon;
            }

            Label lblTitle = new Label
            {
                Text = "Coffee Society Management System",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(55, 12),
                AutoSize = true
            };

            // Minimize button
            btnMinimize = new Button
            {
                Text = "─",
                Size = new Size(45, 45),
                Location = new Point(this.Width - 135, 0),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(100, 100, 100),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            btnMinimize.BringToFront();

            // Maximize button
            btnMaximize = new Button
            {
                Text = "□",
                Size = new Size(45, 45),
                Location = new Point(this.Width - 90, 0),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(100, 100, 100),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Cursor = Cursors.Hand
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
            btnMaximize.BringToFront();

            // Close button - FIXED and VISIBLE
            btnClose = new Button
            {
                Text = "X",
                Size = new Size(45, 45),
                Location = new Point(this.Width - 45, 0),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(100, 100, 100),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Application.Exit();
            btnClose.MouseEnter += (s, e) => { btnClose.BackColor = Color.Red; btnClose.ForeColor = Color.White; };
            btnClose.MouseLeave += (s, e) => { btnClose.BackColor = Color.Transparent; btnClose.ForeColor = Color.FromArgb(100, 100, 100); };
            btnClose.BringToFront();

            titleBar.Controls.Add(picAppIconSmall);
            titleBar.Controls.Add(lblTitle);
            titleBar.Controls.Add(btnMinimize);
            titleBar.Controls.Add(btnMaximize);
            titleBar.Controls.Add(btnClose);
            this.Controls.Add(titleBar);

            // Make sure title bar stays on top
            titleBar.BringToFront();

            // Main content panel
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                AutoScroll = true
            };

            // Create a panel to hold and center everything vertically
            Panel centeringPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            // Calculate center Y position - moved UPWARD for classy look
            int iconSize = 150;
            int titleHeight = 80;
            int subtitleHeight = 40;
            int taglineHeight = 30;
            int separatorHeight = 2;
            int buttonHeight = 65;

            // INCREASED SPACING between elements
            int spacingAfterIcon = 25;
            int spacingAfterTitle = 15;
            int spacingAfterSubtitle = 20;
            int spacingAfterTagline = 15;
            int spacingAfterSeparator = 25;

            int totalContentHeight = iconSize + spacingAfterIcon + titleHeight + spacingAfterTitle +
                                    subtitleHeight + spacingAfterSubtitle + taglineHeight +
                                    spacingAfterTagline + separatorHeight + spacingAfterSeparator + buttonHeight;

            // Moved UPWARD - using 35% instead of 50% to shift content up
            int startY = (this.Height - totalContentHeight) * 35 / 100;
            if (startY < 40) startY = 40;

            int currentY = startY;

            // Circular Icon (150x150)
            picCircularIcon = new PictureBox
            {
                Size = new Size(iconSize, iconSize),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent
            };
            picCircularIcon.Image = MakeHighQualityCircularImage(highQualityIcon, iconSize);
            picCircularIcon.Location = new Point((this.Width - iconSize) / 2, currentY);
            currentY += iconSize + spacingAfterIcon;

            // Main Title
            lblMainTitle = new Label
            {
                Text = "SUBUKIA",
                Font = new Font("Segoe UI", 42, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Width = this.Width,
                Height = titleHeight,
                Location = new Point(0, currentY)
            };
            currentY += titleHeight + spacingAfterTitle;

            // Sub Title
            lblSubTitle = new Label
            {
                Text = "COFFEE SOCIETY MANAGEMENT SYSTEM",
                Font = new Font("Segoe UI", 18, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 100, 100),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Width = this.Width,
                Height = subtitleHeight,
                Location = new Point(0, currentY)
            };
            currentY += subtitleHeight + spacingAfterSubtitle;

            // Tagline
            lblTagline = new Label
            {
                Text = "Empowering Coffee Farmers Through Technology",
                Font = new Font("Segoe UI", 12, FontStyle.Italic),
                ForeColor = Color.FromArgb(128, 128, 128),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Width = this.Width,
                Height = taglineHeight,
                Location = new Point(0, currentY)
            };
            currentY += taglineHeight + spacingAfterTagline;

            // Separator Line
            separator = new Panel
            {
                Size = new Size(400, separatorHeight),
                BackColor = Color.FromArgb(139, 69, 19)
            };
            separator.Location = new Point((this.Width - 400) / 2, currentY);
            currentY += separatorHeight + spacingAfterSeparator;

            // BIG START BUTTON
            btnStart = new Button
            {
                Text = "GET STARTED",
                Size = new Size(280, buttonHeight),
                BackColor = Color.FromArgb(46, 125, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
            btnStart.Location = new Point((this.Width - 280) / 2, currentY);

            // Button hover effects
            btnStart.MouseEnter += (s, e) => btnStart.BackColor = Color.FromArgb(56, 145, 60);
            btnStart.MouseLeave += (s, e) => btnStart.BackColor = Color.FromArgb(46, 125, 50);

            // THIS OPENS THE LOGIN FORM
            btnStart.Click += (sender, args) =>
            {
                frmlogin loginForm = new frmlogin();
                loginForm.StartPosition = FormStartPosition.CenterScreen;
                loginForm.Show();
                this.Hide();
            };

            // Add all controls to centering panel
            centeringPanel.Controls.Add(picCircularIcon);
            centeringPanel.Controls.Add(lblMainTitle);
            centeringPanel.Controls.Add(lblSubTitle);
            centeringPanel.Controls.Add(lblTagline);
            centeringPanel.Controls.Add(separator);
            centeringPanel.Controls.Add(btnStart);

            mainPanel.Controls.Add(centeringPanel);

            // Footer
            Panel footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 110,
                BackColor = Color.FromArgb(44, 62, 80)
            };

            Label lblContactHeader = new Label
            {
                Text = "CONTACT US",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 215, 0),
                Location = new Point(30, 12),
                AutoSize = true
            };

            Label lblAddress = new Label
            {
                Text = "P.O Box 123 - 20100, Subukia, Kenya",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(200, 200, 200),
                Location = new Point(30, 38),
                AutoSize = true
            };

            Label lblPhone = new Label
            {
                Text = "Phone: +254 700 000 000 | +254 711 111 111",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(200, 200, 200),
                Location = new Point(30, 58),
                AutoSize = true
            };

            Label lblEmail = new Label
            {
                Text = "Email: info@subukiacoffee.co.ke | support@subukiacoffee.co.ke",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(200, 200, 200),
                Location = new Point(30, 78),
                AutoSize = true
            };

            lblVersion = new Label
            {
                Text = "Version 2.0",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(200, 200, 200),
                Location = new Point(this.Width - 100, 78),
                AutoSize = true,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            footerPanel.Controls.Add(lblContactHeader);
            footerPanel.Controls.Add(lblAddress);
            footerPanel.Controls.Add(lblPhone);
            footerPanel.Controls.Add(lblEmail);
            footerPanel.Controls.Add(lblVersion);

            this.Controls.Add(mainPanel);
            this.Controls.Add(footerPanel);
            this.Controls.Add(titleBar);

            // Ensure proper z-order - title bar on top, then footer, then main panel
            titleBar.BringToFront();
            footerPanel.BringToFront();

            // Make sure close button is on top of everything
            btnClose.BringToFront();
            btnMaximize.BringToFront();
            btnMinimize.BringToFront();

            // Resize events
            this.Resize += (s, e) =>
            {
                // Update title bar buttons
                if (btnMinimize != null) btnMinimize.Location = new Point(this.Width - 135, 0);
                if (btnMaximize != null) btnMaximize.Location = new Point(this.Width - 90, 0);
                if (btnClose != null) btnClose.Location = new Point(this.Width - 45, 0);

                // Update footer
                if (footerPanel != null)
                {
                    footerPanel.Location = new Point(0, this.Height - 110);
                    footerPanel.Size = new Size(this.Width, 110);
                }
                if (lblVersion != null)
                    lblVersion.Location = new Point(this.Width - 100, 78);

                // Recalculate positions with spacing
                int newIconSize = 150;
                int newTitleHeight = 80;
                int newSubtitleHeight = 40;
                int newTaglineHeight = 30;
                int newSeparatorHeight = 2;
                int newButtonHeight = 65;

                int newSpacingAfterIcon = 25;
                int newSpacingAfterTitle = 15;
                int newSpacingAfterSubtitle = 20;
                int newSpacingAfterTagline = 15;
                int newSpacingAfterSeparator = 25;

                int newTotalHeight = newIconSize + newSpacingAfterIcon + newTitleHeight + newSpacingAfterTitle +
                                    newSubtitleHeight + newSpacingAfterSubtitle + newTaglineHeight +
                                    newSpacingAfterTagline + newSeparatorHeight + newSpacingAfterSeparator + newButtonHeight;

                int newStartY = (this.Height - newTotalHeight) * 35 / 100;
                if (newStartY < 40) newStartY = 40;

                int newCurrentY = newStartY;

                if (picCircularIcon != null)
                {
                    picCircularIcon.Location = new Point((this.Width - newIconSize) / 2, newCurrentY);
                    newCurrentY += newIconSize + newSpacingAfterIcon;
                }
                if (lblMainTitle != null)
                {
                    lblMainTitle.Width = this.Width;
                    lblMainTitle.Location = new Point(0, newCurrentY);
                    newCurrentY += newTitleHeight + newSpacingAfterTitle;
                }
                if (lblSubTitle != null)
                {
                    lblSubTitle.Width = this.Width;
                    lblSubTitle.Location = new Point(0, newCurrentY);
                    newCurrentY += newSubtitleHeight + newSpacingAfterSubtitle;
                }
                if (lblTagline != null)
                {
                    lblTagline.Width = this.Width;
                    lblTagline.Location = new Point(0, newCurrentY);
                    newCurrentY += newTaglineHeight + newSpacingAfterTagline;
                }
                if (separator != null)
                {
                    separator.Location = new Point((this.Width - 400) / 2, newCurrentY);
                    newCurrentY += newSeparatorHeight + newSpacingAfterSeparator;
                }
                if (btnStart != null)
                    btnStart.Location = new Point((this.Width - 280) / 2, newCurrentY);

                // Keep buttons on top
                if (btnClose != null) btnClose.BringToFront();
                if (btnMaximize != null) btnMaximize.BringToFront();
                if (btnMinimize != null) btnMinimize.BringToFront();
            };
        }

        private Bitmap MakeHighQualityCircularImage(Image sourceImage, int size)
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
                        using (Bitmap highQualityImage = new Bitmap(size, size))
                        {
                            using (Graphics g2 = Graphics.FromImage(highQualityImage))
                            {
                                g2.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                g2.DrawImage(sourceImage, 0, 0, size, size);
                            }
                            g.DrawImage(highQualityImage, 0, 0, size, size);
                        }
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
    }
}