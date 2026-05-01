using DigitalHealthTrainer.Localization;
using DigitalHealthTrainer.Models;
using DigitalHealthTrainer.Services;

namespace DigitalHealthTrainer.Forms
{
    public class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private TextBox txtEmail;
        private Button btnLogin;
        private Button btnRegister;
        private Label lblTitle;
        private Label lblUsername;
        private Label lblPassword;
        private Label lblEmail;
        private Label lblStatus;
        private Label lblLanguage;
        private ComboBox cmbLanguage;
        private Panel panelMain;
        private bool isRegisterMode = false;

        public LoginForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = Lang.Get("app_title");
            this.Size = new Size(420, 440);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(245, 245, 245);

            // Dil seçici (formun üstünde, panel dışında)
            lblLanguage = new Label
            {
                Text = Lang.Get("language"),
                Font = new Font("Segoe UI", 9),
                Location = new Point(230, 8),
                Size = new Size(60, 20),
                TextAlign = ContentAlignment.MiddleRight
            };

            cmbLanguage = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9),
                Location = new Point(295, 5),
                Size = new Size(100, 25)
            };
            cmbLanguage.Items.AddRange(new object[] { "English", "Türkçe" });
            cmbLanguage.SelectedIndex = Lang.Current == AppLanguage.English ? 0 : 1;
            cmbLanguage.SelectedIndexChanged += CmbLanguage_Changed;

            panelMain = new Panel
            {
                Location = new Point(30, 35),
                Size = new Size(340, 350),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblTitle = new Label
            {
                Text = Lang.Get("login_title"),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                Location = new Point(20, 15),
                Size = new Size(300, 35),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblUsername = new Label
            {
                Text = Lang.Get("username"),
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 65),
                Size = new Size(300, 22)
            };
            txtUsername = new TextBox
            {
                Font = new Font("Segoe UI", 11),
                Location = new Point(20, 90),
                Size = new Size(300, 30)
            };

            lblPassword = new Label
            {
                Text = Lang.Get("password"),
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 125),
                Size = new Size(300, 22)
            };
            txtPassword = new TextBox
            {
                Font = new Font("Segoe UI", 11),
                Location = new Point(20, 150),
                Size = new Size(300, 30),
                PasswordChar = '●'
            };

            lblEmail = new Label
            {
                Text = Lang.Get("email"),
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 185),
                Size = new Size(300, 22),
                Visible = false
            };
            txtEmail = new TextBox
            {
                Font = new Font("Segoe UI", 11),
                Location = new Point(20, 210),
                Size = new Size(300, 30),
                Visible = false
            };

            btnLogin = new Button
            {
                Text = Lang.Get("btn_login"),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(20, 195),
                Size = new Size(145, 40),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            btnRegister = new Button
            {
                Text = Lang.Get("btn_register"),
                Font = new Font("Segoe UI", 11),
                Location = new Point(175, 195),
                Size = new Size(145, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click += BtnRegister_Click;

            lblStatus = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                Location = new Point(20, 245),
                Size = new Size(300, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            panelMain.Controls.AddRange(new Control[]
            {
                lblTitle, lblUsername, txtUsername,
                lblPassword, txtPassword,
                lblEmail, txtEmail,
                btnLogin, btnRegister, lblStatus
            });

            this.Controls.Add(lblLanguage);
            this.Controls.Add(cmbLanguage);
            this.Controls.Add(panelMain);

            this.AcceptButton = btnLogin;
        }

        private void CmbLanguage_Changed(object? sender, EventArgs e)
        {
            Lang.Current = cmbLanguage.SelectedIndex == 0 ? AppLanguage.English : AppLanguage.Turkish;
            UpdateTexts();
        }

        private void UpdateTexts()
        {
            this.Text = Lang.Get("app_title");
            lblLanguage.Text = Lang.Get("language");
            lblUsername.Text = Lang.Get("username");
            lblPassword.Text = Lang.Get("password");
            lblEmail.Text = Lang.Get("email");
            lblStatus.Text = "";

            if (isRegisterMode)
            {
                lblTitle.Text = Lang.Get("register_title");
                btnLogin.Text = Lang.Get("back");
                btnRegister.Text = Lang.Get("btn_register");
            }
            else
            {
                lblTitle.Text = Lang.Get("login_title");
                btnLogin.Text = Lang.Get("btn_login");
                btnRegister.Text = Lang.Get("btn_register");
            }
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            if (isRegisterMode)
            {
                ToggleMode(false);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                ShowStatus(Lang.Get("login_fields_required"), Color.Red);
                return;
            }

            try
            {
                Trainer? trainer = AuthService.Login(txtUsername.Text.Trim(), txtPassword.Text);

                if (trainer != null)
                {
                    ShowStatus(Lang.Get("login_success"), Color.Green);

                    MessageBox.Show(
                        Lang.Format("login_welcome", trainer.Username),
                        Lang.Get("success"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    // TODO: DashboardForm açılacak
                    // var dashboard = new DashboardForm(trainer);
                    // dashboard.Show();
                    // this.Hide();
                }
                else
                {
                    ShowStatus(Lang.Get("login_failed"), Color.Red);
                }
            }
            catch (Exception ex)
            {
                ShowStatus(Lang.Get("connection_error") + ex.Message, Color.Red);
            }
        }

        private void BtnRegister_Click(object? sender, EventArgs e)
        {
            if (!isRegisterMode)
            {
                ToggleMode(true);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                ShowStatus(Lang.Get("fields_required"), Color.Red);
                return;
            }

            try
            {
                bool success = AuthService.Register(
                    txtUsername.Text.Trim(),
                    txtPassword.Text,
                    txtEmail.Text.Trim());

                if (success)
                {
                    ShowStatus(Lang.Get("register_success"), Color.Green);
                    ToggleMode(false);
                }
                else
                {
                    ShowStatus(Lang.Get("register_duplicate"), Color.Orange);
                }
            }
            catch (Exception ex)
            {
                ShowStatus(Lang.Get("register_error") + ex.Message, Color.Red);
            }
        }

        private void ToggleMode(bool registerMode)
        {
            isRegisterMode = registerMode;
            lblEmail.Visible = registerMode;
            txtEmail.Visible = registerMode;

            if (registerMode)
            {
                lblTitle.Text = Lang.Get("register_title");
                btnLogin.Text = Lang.Get("back");
                btnLogin.BackColor = Color.Gray;
                btnRegister.Text = Lang.Get("btn_register");
                btnLogin.Location = new Point(20, 255);
                btnRegister.Location = new Point(175, 255);
                lblStatus.Location = new Point(20, 300);
            }
            else
            {
                lblTitle.Text = Lang.Get("login_title");
                btnLogin.Text = Lang.Get("btn_login");
                btnLogin.BackColor = Color.FromArgb(41, 128, 185);
                btnRegister.Text = Lang.Get("btn_register");
                btnLogin.Location = new Point(20, 195);
                btnRegister.Location = new Point(175, 195);
                lblStatus.Location = new Point(20, 245);
            }

            txtEmail.Text = "";
            lblStatus.Text = "";
        }

        private void ShowStatus(string message, Color color)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = color;
        }
    }
}
