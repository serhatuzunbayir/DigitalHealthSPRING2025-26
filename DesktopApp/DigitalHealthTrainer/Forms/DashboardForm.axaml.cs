using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using DigitalHealthTrainer.Localization;
using DigitalHealthTrainer.Models;
using DigitalHealthTrainer.Services;

namespace DigitalHealthTrainer.Forms
{
    public partial class DashboardForm : Window
    {
        private readonly Trainer _trainer;
        private List<ClientDashboardSummary> _dashboardData = new();

        // Kontroller
        private DataGrid _dgClients = null!;
        private TextBlock _lblTrainerName = null!;
        private TextBlock _lblTrainerEmail = null!;
        private TextBlock _lblPageTitle = null!;
        private TextBlock _lblAssignedClients = null!;
        private TextBlock _lblTotalClients = null!;
        private TextBlock _lblTotalClientsLabel = null!;
        private TextBlock _lblTotalActiveGoals = null!;
        private TextBlock _lblActiveGoalsLabel = null!;
        private TextBlock _lblAvgAdherence = null!;
        private TextBlock _lblAvgAdherenceLabel = null!;
        private Button _btnRefresh = null!;
        private Button _btnLogout = null!;
        private Button _btnDashboard = null!;
        private Button _btnReports = null!;
        private Button _btnWorkouts = null!;
        private Button _btnSessions = null!;
        private ComboBox _cmbLanguage = null!;

        // Avalonia XAML loader için parametresiz constructor
        public DashboardForm() : this(new Trainer()) { }

        public DashboardForm(Trainer trainer)
        {
            InitializeComponent();
            _trainer = trainer;

            // Kontrolleri bul
            _dgClients = this.FindControl<DataGrid>("dgClients")!;
            _lblTrainerName = this.FindControl<TextBlock>("lblTrainerName")!;
            _lblTrainerEmail = this.FindControl<TextBlock>("lblTrainerEmail")!;
            _lblPageTitle = this.FindControl<TextBlock>("lblPageTitle")!;
            _lblAssignedClients = this.FindControl<TextBlock>("lblAssignedClients")!;
            _lblTotalClients = this.FindControl<TextBlock>("lblTotalClients")!;
            _lblTotalClientsLabel = this.FindControl<TextBlock>("lblTotalClientsLabel")!;
            _lblTotalActiveGoals = this.FindControl<TextBlock>("lblTotalActiveGoals")!;
            _lblActiveGoalsLabel = this.FindControl<TextBlock>("lblActiveGoalsLabel")!;
            _lblAvgAdherence = this.FindControl<TextBlock>("lblAvgAdherence")!;
            _lblAvgAdherenceLabel = this.FindControl<TextBlock>("lblAvgAdherenceLabel")!;
            _btnRefresh = this.FindControl<Button>("btnRefresh")!;
            _btnLogout = this.FindControl<Button>("btnLogout")!;
            _btnDashboard = this.FindControl<Button>("btnDashboard")!;
            _btnReports = this.FindControl<Button>("btnReports")!;
            _btnWorkouts = this.FindControl<Button>("btnWorkouts")!;
            _btnSessions = this.FindControl<Button>("btnSessions")!;
            _cmbLanguage = this.FindControl<ComboBox>("cmbLanguage")!;

            // Trainer bilgisi
            _lblTrainerName.Text = _trainer.Username;
            _lblTrainerEmail.Text = _trainer.Email;

            // Events
            _btnRefresh.Click += (s, e) => LoadDashboard();
            _btnLogout.Click += BtnLogout_Click;
            _btnDashboard.Click += (s, e) => LoadDashboard();
            _btnReports.Click += BtnReports_Click;
            _btnWorkouts.Click += BtnWorkouts_Click;
            _btnSessions.Click += BtnSessions_Click;
            _cmbLanguage.SelectionChanged += CmbLanguage_Changed;

            UpdateTexts();
            LoadDashboard();
        }

        private void CmbLanguage_Changed(object? sender, SelectionChangedEventArgs e)
        {
            Lang.Current = _cmbLanguage.SelectedIndex == 0 ? AppLanguage.English : AppLanguage.Turkish;
            UpdateTexts();
        }

        private void UpdateTexts()
        {
            this.Title = Lang.Get("app_title") + " - " + Lang.Get("dashboard_title");
            _lblPageTitle.Text = Lang.Get("btn_dashboard");
            _lblAssignedClients.Text = Lang.Get("assigned_clients");
            _btnRefresh.Content = "⟳  " + Lang.Get("refresh");
            _lblTotalClientsLabel.Text = Lang.Get("total_clients");
            _lblActiveGoalsLabel.Text = Lang.Get("progress");
            _lblAvgAdherenceLabel.Text = Lang.Get("adherence_rate");

            _btnDashboard.Content = "📊  " + Lang.Get("btn_dashboard");
            _btnWorkouts.Content = "🏋️  " + Lang.Get("workout_title");
            _btnSessions.Content = "📅  " + Lang.Get("sessions_title");
            _btnReports.Content = "📈  " + Lang.Get("btn_reports");
            _btnLogout.Content = "🚪  " + Lang.Get("btn_logout");
        }

        private void LoadDashboard()
        {
            try
            {
                _dashboardData = ClientService.GetDashboardData(_trainer.TrainerId);
                _dgClients.ItemsSource = _dashboardData;

                _lblTotalClients.Text = _dashboardData.Count.ToString();
                _lblTotalActiveGoals.Text = _dashboardData.Sum(d => d.ActiveGoals).ToString();

                double avgAdherence = _dashboardData.Any()
                    ? _dashboardData.Average(d => d.AdherenceRate)
                    : 0;
                _lblAvgAdherence.Text = $"{Math.Round(avgAdherence, 1)}%";

                // Renk kodlama: green >=80, yellow >=50, red <50
                _dgClients.LoadingRow -= DgClients_LoadingRow;
                _dgClients.LoadingRow += DgClients_LoadingRow;
            }
            catch (Exception ex)
            {
                _lblPageTitle.Text = Lang.Get("connection_error") + ex.Message;
            }
        }

        private void DgClients_LoadingRow(object? sender, DataGridRowEventArgs e)
        {
            if (e.Row.DataContext is ClientDashboardSummary summary)
            {
                if (summary.AdherenceRate >= 80)
                    e.Row.Background = new SolidColorBrush(Color.Parse("#F0FDF4"));
                else if (summary.AdherenceRate >= 50)
                    e.Row.Background = new SolidColorBrush(Color.Parse("#FFFBEB"));
                else
                    e.Row.Background = new SolidColorBrush(Color.Parse("#FEF2F2"));
            }
        }

        private void BtnLogout_Click(object? sender, RoutedEventArgs e)
        {
            var loginForm = new LoginForm();
            loginForm.Show();
            this.Close();
        }

        private void BtnReports_Click(object? sender, RoutedEventArgs e)
        {
            var reportForm = new ProgressReportForm(_trainer);
            reportForm.Show();
        }

        private void BtnWorkouts_Click(object? sender, RoutedEventArgs e)
        {
            // Ecem'in formu
        }

        private void BtnSessions_Click(object? sender, RoutedEventArgs e)
        {
            // Ecem'in formu
        }
    }
}
