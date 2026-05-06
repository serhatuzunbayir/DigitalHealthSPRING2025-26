using Avalonia.Controls;
using Avalonia.Interactivity;
using DigitalHealthTrainer.Localization;
using DigitalHealthTrainer.Models;
using DigitalHealthTrainer.Services;

namespace DigitalHealthTrainer.Forms
{
    public partial class ProgressReportForm : Window
    {
        private readonly Trainer _trainer;
        private List<Client> _clients = new();

        // Kontroller
        private ComboBox _cmbClient = null!;
        private ComboBox _cmbExerciseType = null!;
        private DatePicker _dpFrom = null!;
        private DatePicker _dpTo = null!;
        private Button _btnGenerate = null!;
        private DataGrid _dgExercise = null!;
        private DataGrid _dgHealth = null!;
        private DataGrid _dgGoals = null!;
        private TextBlock _lblGoalTotal = null!;
        private TextBlock _lblGoalCompleted = null!;
        private TextBlock _lblGoalInProgress = null!;
        private TextBlock _lblGoalRate = null!;
        private TextBlock _lblPageTitle = null!;
        private TextBlock _lblSelectClient = null!;
        private TextBlock _lblExerciseType = null!;
        private TextBlock _lblFromDate = null!;
        private TextBlock _lblToDate = null!;

        // Avalonia XAML loader için parametresiz constructor
        public ProgressReportForm() : this(new Trainer()) { }

        public ProgressReportForm(Trainer trainer)
        {
            InitializeComponent();
            _trainer = trainer;

            // Kontrolleri bul
            _cmbClient = this.FindControl<ComboBox>("cmbClient")!;
            _cmbExerciseType = this.FindControl<ComboBox>("cmbExerciseType")!;
            _dpFrom = this.FindControl<DatePicker>("dpFrom")!;
            _dpTo = this.FindControl<DatePicker>("dpTo")!;
            _btnGenerate = this.FindControl<Button>("btnGenerate")!;
            _dgExercise = this.FindControl<DataGrid>("dgExercise")!;
            _dgHealth = this.FindControl<DataGrid>("dgHealth")!;
            _dgGoals = this.FindControl<DataGrid>("dgGoals")!;
            _lblGoalTotal = this.FindControl<TextBlock>("lblGoalTotal")!;
            _lblGoalCompleted = this.FindControl<TextBlock>("lblGoalCompleted")!;
            _lblGoalInProgress = this.FindControl<TextBlock>("lblGoalInProgress")!;
            _lblGoalRate = this.FindControl<TextBlock>("lblGoalRate")!;
            _lblPageTitle = this.FindControl<TextBlock>("lblPageTitle")!;
            _lblSelectClient = this.FindControl<TextBlock>("lblSelectClient")!;
            _lblExerciseType = this.FindControl<TextBlock>("lblExerciseType")!;
            _lblFromDate = this.FindControl<TextBlock>("lblFromDate")!;
            _lblToDate = this.FindControl<TextBlock>("lblToDate")!;

            // Varsayılan tarih aralığı: son 30 gün
            _dpFrom.SelectedDate = DateTimeOffset.Now.AddDays(-30);
            _dpTo.SelectedDate = DateTimeOffset.Now;

            // Events
            _btnGenerate.Click += BtnGenerate_Click;
            _cmbClient.SelectionChanged += CmbClient_Changed;

            UpdateTexts();
            LoadClients();
        }

        private void UpdateTexts()
        {
            this.Title = Lang.Get("app_title") + " - " + Lang.Get("reports_title");
            _lblPageTitle.Text = "📈 " + Lang.Get("reports_title");
            _lblSelectClient.Text = Lang.Get("select_client");
            _lblExerciseType.Text = Lang.Get("exercise_type");
            _lblFromDate.Text = Lang.Get("from_date");
            _lblToDate.Text = Lang.Get("to_date");
            _btnGenerate.Content = Lang.Get("generate_report");
        }

        private void LoadClients()
        {
            try
            {
                _clients = ClientService.GetAssignedClients(_trainer.TrainerId);
                _cmbClient.ItemsSource = _clients.Select(c => c.Username).ToList();

                if (_clients.Any())
                    _cmbClient.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                _lblPageTitle.Text = Lang.Get("connection_error") + ex.Message;
            }
        }

        private void CmbClient_Changed(object? sender, SelectionChangedEventArgs e)
        {
            if (_cmbClient.SelectedIndex < 0 || _cmbClient.SelectedIndex >= _clients.Count) return;

            var client = _clients[_cmbClient.SelectedIndex];

            // Egzersiz tiplerini yükle
            try
            {
                var types = ReportService.GetDistinctExerciseTypes(client.ClientId);
                var allTypes = new List<string> { Lang.Get("all_types") };
                allTypes.AddRange(types);
                _cmbExerciseType.ItemsSource = allTypes;
                _cmbExerciseType.SelectedIndex = 0;
            }
            catch { }
        }

        private void BtnGenerate_Click(object? sender, RoutedEventArgs e)
        {
            if (_cmbClient.SelectedIndex < 0 || _cmbClient.SelectedIndex >= _clients.Count) return;

            var client = _clients[_cmbClient.SelectedIndex];
            DateTime from = _dpFrom.SelectedDate?.DateTime ?? DateTime.Today.AddDays(-30);
            DateTime to = _dpTo.SelectedDate?.DateTime ?? DateTime.Today;

            string? exerciseType = null;
            if (_cmbExerciseType.SelectedIndex > 0)
                exerciseType = _cmbExerciseType.SelectedItem?.ToString();

            try
            {
                // ===== EXERCISE TAB — LINQ =====
                var logs = ReportService.GetExerciseLogs(client.ClientId);
                var weeklySummary = ReportService.GetWeeklyAverages(logs, from, to, exerciseType);
                _dgExercise.ItemsSource = weeklySummary;

                // ===== HEALTH TAB — LINQ =====
                var metrics = ReportService.GetHealthMetrics(client.ClientId);
                var filteredMetrics = ReportService.GetFilteredHealthMetrics(metrics, from, to);
                _dgHealth.ItemsSource = filteredMetrics;

                // ===== GOALS TAB — LINQ =====
                var goals = ReportService.GetGoals(client.ClientId);
                var filteredGoals = ReportService.GetFilteredGoals(goals, null);
                _dgGoals.ItemsSource = filteredGoals;

                var stats = ReportService.GetGoalStats(goals, null);
                _lblGoalTotal.Text = stats.total.ToString();
                _lblGoalCompleted.Text = stats.completed.ToString();
                _lblGoalInProgress.Text = stats.inProgress.ToString();
                _lblGoalRate.Text = $"{stats.rate}%";
            }
            catch (Exception ex)
            {
                _lblPageTitle.Text = Lang.Get("connection_error") + ex.Message;
            }
        }
    }
}
