using DigitalHealthTrainer.Localization;
using DigitalHealthTrainer.Models;
using DigitalHealthTrainer.Services;

namespace DigitalHealthTrainer.Forms
{
    public partial class LoginPage : ContentPage
    {
        private bool isRegisterMode = false;

        public LoginPage()
        {
            InitializeComponent();
            btnLogin.Clicked += BtnLogin_Click;
            btnRegister.Clicked += BtnRegister_Click;
            pckLanguage.SelectedIndexChanged += PckLanguage_Changed;
        }

        private void PckLanguage_Changed(object? sender, EventArgs e)
        {
            Lang.Current = pckLanguage.SelectedIndex == 0 ? AppLanguage.English : AppLanguage.Turkish;
            UpdateTexts();
        }

        private void UpdateTexts()
        {
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

        private async void BtnLogin_Click(object? sender, EventArgs e)
        {
            if (isRegisterMode) { ToggleMode(false); return; }

            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            { ShowStatus(Lang.Get("login_fields_required"), Colors.Red); return; }

            try
            {
                Trainer? trainer = AuthService.Login(txtUsername.Text.Trim(), txtPassword.Text);
                if (trainer != null)
                {
                    ShowStatus(Lang.Get("login_success"), Colors.Green);
                    Application.Current!.Windows[0].Page = new DashboardPage(trainer);
                }
                else
                {
                    ShowStatus(Lang.Get("login_failed"), Colors.Red);
                }
            }
            catch (Exception ex)
            {
                ShowStatus(Lang.Get("connection_error") + ex.Message, Colors.Red);
            }
        }

        private void BtnRegister_Click(object? sender, EventArgs e)
        {
            if (!isRegisterMode) { ToggleMode(true); return; }

            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text))
            { ShowStatus(Lang.Get("fields_required"), Colors.Red); return; }

            try
            {
                bool success = AuthService.Register(txtUsername.Text.Trim(), txtPassword.Text, txtEmail.Text.Trim());
                if (success)
                { ShowStatus(Lang.Get("register_success"), Colors.Green); ToggleMode(false); }
                else
                { ShowStatus(Lang.Get("register_duplicate"), Colors.Orange); }
            }
            catch (Exception ex)
            { ShowStatus(Lang.Get("register_error") + ex.Message, Colors.Red); }
        }

        private void ToggleMode(bool registerMode)
        {
            isRegisterMode = registerMode;
            lblEmail.IsVisible = registerMode;
            txtEmail.IsVisible = registerMode;

            if (registerMode)
            {
                lblTitle.Text = Lang.Get("register_title");
                btnLogin.Text = Lang.Get("back");
                btnLogin.BackgroundColor = Colors.Gray;
            }
            else
            {
                lblTitle.Text = Lang.Get("login_title");
                btnLogin.Text = Lang.Get("btn_login");
                btnLogin.BackgroundColor = Color.FromArgb("#4F46E5");
            }
            txtEmail.Text = "";
            lblStatus.Text = "";
        }

        private void ShowStatus(string message, Color color)
        {
            lblStatus.Text = message;
            lblStatus.TextColor = color;
        }
    }
}
