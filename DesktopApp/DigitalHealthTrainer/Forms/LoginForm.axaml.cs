using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using DigitalHealthTrainer.Localization;
using DigitalHealthTrainer.Models;
using DigitalHealthTrainer.Services;

namespace DigitalHealthTrainer.Forms
{
    public partial class LoginForm : Window
    {
        private bool isRegisterMode = false;

        public LoginForm()
        {
            InitializeComponent();

            btnLogin.Click += BtnLogin_Click;
            btnRegister.Click += BtnRegister_Click;
            cmbLanguage.SelectionChanged += CmbLanguage_Changed;
        }

        private void CmbLanguage_Changed(object? sender, SelectionChangedEventArgs e)
        {
            Lang.Current = cmbLanguage.SelectedIndex == 0 ? AppLanguage.English : AppLanguage.Turkish;
            UpdateTexts();
        }

        private void UpdateTexts()
        {
            this.Title = Lang.Get("app_title");
            lblLanguage.Text = Lang.Get("language");
            lblUsername.Text = Lang.Get("username");
            lblPassword.Text = Lang.Get("password");
            lblEmail.Text = Lang.Get("email");
            lblStatus.Text = "";

            if (isRegisterMode)
            {
                lblTitle.Text = Lang.Get("register_title");
                btnLogin.Content = Lang.Get("back");
                btnRegister.Content = Lang.Get("btn_register");
            }
            else
            {
                lblTitle.Text = Lang.Get("login_title");
                btnLogin.Content = Lang.Get("btn_login");
                btnRegister.Content = Lang.Get("btn_register");
            }
        }

        private void BtnLogin_Click(object? sender, RoutedEventArgs e)
        {
            if (isRegisterMode)
            {
                ToggleMode(false);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                ShowStatus(Lang.Get("login_fields_required"), Brushes.Red);
                return;
            }

            try
            {
                Trainer? trainer = AuthService.Login(txtUsername.Text.Trim(), txtPassword.Text);

                if (trainer != null)
                {
                    ShowStatus(Lang.Get("login_success"), Brushes.Green);

                    // TODO: DashboardForm açılacak
                    // var dashboard = new DashboardForm(trainer);
                    // dashboard.Show();
                    // this.Close();

                    var msgBox = new Window
                    {
                        Title = Lang.Get("success"),
                        Width = 300, Height = 150,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        Content = new TextBlock
                        {
                            Text = Lang.Format("login_welcome", trainer.Username),
                            FontSize = 14,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                        }
                    };
                    msgBox.ShowDialog(this);
                }
                else
                {
                    ShowStatus(Lang.Get("login_failed"), Brushes.Red);
                }
            }
            catch (Exception ex)
            {
                ShowStatus(Lang.Get("connection_error") + ex.Message, Brushes.Red);
            }
        }

        private void BtnRegister_Click(object? sender, RoutedEventArgs e)
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
                ShowStatus(Lang.Get("fields_required"), Brushes.Red);
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
                    ShowStatus(Lang.Get("register_success"), Brushes.Green);
                    ToggleMode(false);
                }
                else
                {
                    ShowStatus(Lang.Get("register_duplicate"), Brushes.Orange);
                }
            }
            catch (Exception ex)
            {
                ShowStatus(Lang.Get("register_error") + ex.Message, Brushes.Red);
            }
        }

        private void ToggleMode(bool registerMode)
        {
            isRegisterMode = registerMode;
            lblEmail.IsVisible = registerMode;
            txtEmail.IsVisible = registerMode;

            if (registerMode)
            {
                lblTitle.Text = Lang.Get("register_title");
                btnLogin.Content = Lang.Get("back");
                btnLogin.Background = Brushes.Gray;
                btnRegister.Content = Lang.Get("btn_register");
            }
            else
            {
                lblTitle.Text = Lang.Get("login_title");
                btnLogin.Content = Lang.Get("btn_login");
                btnLogin.Background = new SolidColorBrush(Color.Parse("#2980B5"));
                btnRegister.Content = Lang.Get("btn_register");
            }

            txtEmail.Text = "";
            lblStatus.Text = "";
        }

        private void ShowStatus(string message, IBrush color)
        {
            lblStatus.Text = message;
            lblStatus.Foreground = color;
        }
    }
}
