using DigitalHealthTrainer.Localization;
using DigitalHealthTrainer.Models;
using DigitalHealthTrainer.Services;
using Microsoft.Maui.Controls.Shapes;

namespace DigitalHealthTrainer.Forms
{
    public partial class DashboardPage : ContentPage
    {
        private readonly Trainer _trainer;
        private string _activeTab = "dashboard";

        public DashboardPage(Trainer trainer)
        {
            InitializeComponent();
            _trainer = trainer;

            lblTrainerName.Text = _trainer.Username;
            lblTrainerEmail.Text = _trainer.Email;

            // Events
            btnNavDashboard.Clicked += (s, e) => SwitchTab("dashboard");
            btnNavClients.Clicked  += (s, e) => SwitchTab("clients");
            btnNavWorkouts.Clicked += (s, e) => SwitchTab("workouts");
            btnNavSessions.Clicked += (s, e) => SwitchTab("sessions");
            btnNavGoals.Clicked    += (s, e) => SwitchTab("goals");
            btnNavReports.Clicked  += (s, e) => SwitchTab("reports");
            btnLogout.Clicked += (s, e) => Application.Current!.Windows[0].Page = new LoginPage();

            // Subscribe to notification events (DELEGATE requirement)
            NotificationService.OnSessionCanceled  += OnNotification;
            NotificationService.OnSessionCompleted += OnNotification;
            NotificationService.OnSessionCreated   += OnNotification;
            NotificationService.OnSessionStarted   += OnNotification;

            SwitchTab("dashboard");
        }

        private void OnNotification(string message, int sessionId, string status)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await DisplayAlert(Lang.Get("notification"), message, "OK");
                }
                catch { }
            });
        }

        private void SwitchTab(string tab)
        {
            _activeTab = tab;
            var navBtn = (Style)Application.Current!.Resources["NavBtn"];
            var navActive = (Style)Application.Current!.Resources["NavActiveBtn"];

            btnNavDashboard.Style = navBtn;
            btnNavClients.Style   = navBtn;
            btnNavWorkouts.Style  = navBtn;
            btnNavSessions.Style  = navBtn;
            btnNavGoals.Style     = navBtn;
            btnNavReports.Style   = navBtn;

            contentArea.Children.Clear();

            switch (tab)
            {
                case "dashboard":
                    btnNavDashboard.Style = navActive;
                    BuildDashboardView();
                    break;
                case "clients":
                    btnNavClients.Style = navActive;
                    BuildClientsView();
                    break;
                case "workouts":
                    btnNavWorkouts.Style = navActive;
                    BuildWorkoutsView();
                    break;
                case "sessions":
                    btnNavSessions.Style = navActive;
                    BuildSessionsView();
                    break;
                case "goals":
                    btnNavGoals.Style = navActive;
                    BuildGoalsView();
                    break;
                case "reports":
                    btnNavReports.Style = navActive;
                    BuildReportsView();
                    break;
            }
        }

        // Helper: creates a card with shadow using Border
        private Border CreateCard(View content, Color? bgColor = null)
        {
            var card = new Border
            {
                BackgroundColor = bgColor ?? Colors.White,
                Stroke = Color.FromArgb("#E2E8F0"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Padding = new Thickness(20, 16),
                Shadow = new Shadow { Brush = new SolidColorBrush(Color.FromArgb("#20000000")), Offset = new Point(0, 2), Radius = 6, Opacity = 0.3f },
                Content = content
            };
            return card;
        }

        // Helper: creates a flat card (no shadow)
        private Border CreateFlatCard(View content, Color? bgColor = null, int radius = 8)
        {
            return new Border
            {
                BackgroundColor = bgColor ?? Color.FromArgb("#F8F9FC"),
                Stroke = Colors.Transparent,
                StrokeShape = new RoundRectangle { CornerRadius = radius },
                Padding = 12,
                Content = content
            };
        }

        // ===================================================================
        // DASHBOARD TAB — Client Monitoring (LINQ QUERY #1)
        // ===================================================================
        private void BuildDashboardView()
        {
            try
            {
                var data = ClientService.GetDashboardData(_trainer.TrainerId);

                // Page title
                contentArea.Children.Add(new Label
                {
                    Text = Lang.Get("btn_dashboard"),
                    FontSize = 24, FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#1E293B")
                });

                // Stat cards
                var statsGrid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition(GridLength.Star),
                        new ColumnDefinition(GridLength.Star),
                        new ColumnDefinition(GridLength.Star)
                    },
                    ColumnSpacing = 14
                };

                double avgAdherence = data.Any() ? data.Average(d => d.AdherenceRate) : 0;

                statsGrid.Add(CreateStatCard("👥", data.Count.ToString(), Lang.Get("total_clients"), "#4F46E5"), 0);
                statsGrid.Add(CreateStatCard("🎯", data.Sum(d => d.ActiveGoals).ToString(), Lang.Get("progress"), "#059669"), 1);
                statsGrid.Add(CreateStatCard("📊", $"{Math.Round(avgAdherence, 1)}%", Lang.Get("adherence_rate"), "#D97706"), 2);
                contentArea.Children.Add(statsGrid);

                // Client list
                contentArea.Children.Add(new Label
                {
                    Text = Lang.Get("assigned_clients"),
                    FontSize = 16, FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#334155"), Margin = new Thickness(0, 8, 0, 4)
                });

                // Header row
                var header = CreateClientRow("Client", "Email", "Last Activity", "Weekly", "Adherence", "Goals", true);
                contentArea.Children.Add(header);

                foreach (var client in data)
                {
                    Color bgColor;
                    if (client.AdherenceRate >= 80) bgColor = Color.FromArgb("#F0FDF4");
                    else if (client.AdherenceRate >= 50) bgColor = Color.FromArgb("#FFFBEB");
                    else bgColor = Color.FromArgb("#FEF2F2");

                    var row = CreateClientRow(
                        client.Username, client.Email,
                        client.LastActivityDisplay,
                        client.WeeklyExerciseCount.ToString(),
                        client.AdherenceDisplay,
                        client.ActiveGoals.ToString(), false, bgColor);
                    contentArea.Children.Add(row);
                }

                if (!data.Any())
                {
                    contentArea.Children.Add(new Label
                    {
                        Text = Lang.Get("no_clients"),
                        TextColor = Color.FromArgb("#94A3B8"),
                        HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 20)
                    });
                }
            }
            catch (Exception ex)
            {
                contentArea.Children.Add(new Label { Text = Lang.Get("connection_error") + ex.Message, TextColor = Colors.Red });
            }
        }

        private Border CreateStatCard(string icon, string value, string label, string color)
        {
            return CreateCard(new VerticalStackLayout
            {
                Children =
                {
                    new Label { Text = icon, FontSize = 20 },
                    new Label { Text = value, FontSize = 28, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb(color) },
                    new Label { Text = label, FontSize = 12, TextColor = Color.FromArgb("#94A3B8") }
                }
            });
        }

        private Border CreateClientRow(string c1, string c2, string c3, string c4, string c5, string c6, bool isHeader, Color? bg = null)
        {
            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                    new ColumnDefinition(new GridLength(1.3, GridUnitType.Star)),
                    new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                    new ColumnDefinition(new GridLength(0.7, GridUnitType.Star)),
                    new ColumnDefinition(new GridLength(0.8, GridUnitType.Star)),
                    new ColumnDefinition(new GridLength(0.6, GridUnitType.Star)),
                },
                Padding = new Thickness(12, 8)
            };

            var attrs = isHeader ? FontAttributes.Bold : FontAttributes.None;
            var color = isHeader ? Color.FromArgb("#334155") : Color.FromArgb("#1E293B");
            int size = isHeader ? 12 : 13;

            grid.Add(new Label { Text = c1, FontSize = size, FontAttributes = attrs, TextColor = color, VerticalOptions = LayoutOptions.Center }, 0);
            grid.Add(new Label { Text = c2, FontSize = size, FontAttributes = attrs, TextColor = color, VerticalOptions = LayoutOptions.Center }, 1);
            grid.Add(new Label { Text = c3, FontSize = size, FontAttributes = attrs, TextColor = color, VerticalOptions = LayoutOptions.Center }, 2);
            grid.Add(new Label { Text = c4, FontSize = size, FontAttributes = attrs, TextColor = color, VerticalOptions = LayoutOptions.Center }, 3);
            grid.Add(new Label { Text = c5, FontSize = size, FontAttributes = attrs, TextColor = color, VerticalOptions = LayoutOptions.Center }, 4);
            grid.Add(new Label { Text = c6, FontSize = size, FontAttributes = attrs, TextColor = color, VerticalOptions = LayoutOptions.Center }, 5);

            return new Border
            {
                BackgroundColor = bg ?? (isHeader ? Color.FromArgb("#F1F5F9") : Colors.White),
                Stroke = Colors.Transparent,
                StrokeShape = new RoundRectangle { CornerRadius = isHeader ? 8 : 0 },
                Padding = 0,
                Content = grid
            };
        }

        // ===================================================================
        // CLIENTS TAB — Assign / Remove Clients
        // ===================================================================
        private void BuildClientsView()
        {
            try
            {
                contentArea.Children.Add(new Label
                {
                    Text = "👥 Client Management",
                    FontSize = 24, FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#1E293B")
                });

                // ── Assigned Clients ─────────────────────────────────────────
                var assignedClients = ClientService.GetAssignedClients(_trainer.TrainerId);

                contentArea.Children.Add(new Label
                {
                    Text = $"Assigned Clients ({assignedClients.Count})",
                    FontSize = 16, FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#334155"), Margin = new Thickness(0, 4, 0, 4)
                });

                if (!assignedClients.Any())
                {
                    contentArea.Children.Add(new Label
                    {
                        Text = "No clients assigned yet. Assign one from the list below.",
                        TextColor = Color.FromArgb("#94A3B8"),
                        HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 8)
                    });
                }
                else
                {
                    var assignedStack = new VerticalStackLayout { Spacing = 6 };
                    foreach (var client in assignedClients)
                    {
                        var row = BuildClientRow(client, isAssigned: true, onAction: () =>
                        {
                            AssignmentService.UnassignClient(_trainer.TrainerId, client.ClientId);
                            SwitchTab("clients");
                        });
                        assignedStack.Children.Add(row);
                    }
                    contentArea.Children.Add(CreateCard(assignedStack, Color.FromArgb("#F0FDF4")));
                }

                // ── Unassigned Clients ───────────────────────────────────────
                var unassigned = AssignmentService.GetUnassignedClients(_trainer.TrainerId);

                contentArea.Children.Add(new Label
                {
                    Text = $"Available Clients ({unassigned.Count})",
                    FontSize = 16, FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#334155"), Margin = new Thickness(0, 12, 0, 4)
                });

                if (!unassigned.Any())
                {
                    contentArea.Children.Add(new Label
                    {
                        Text = "All registered clients are already assigned to you.",
                        TextColor = Color.FromArgb("#94A3B8"),
                        HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 8)
                    });
                }
                else
                {
                    var unassignedStack = new VerticalStackLayout { Spacing = 6 };
                    foreach (var client in unassigned)
                    {
                        var row = BuildClientRow(client, isAssigned: false, onAction: () =>
                        {
                            AssignmentService.AssignClient(_trainer.TrainerId, client.ClientId);
                            SwitchTab("clients");
                        });
                        unassignedStack.Children.Add(row);
                    }
                    contentArea.Children.Add(CreateCard(unassignedStack));
                }
            }
            catch (Exception ex)
            {
                contentArea.Children.Add(new Label { Text = Lang.Get("connection_error") + ex.Message, TextColor = Colors.Red });
            }
        }

        private Border BuildClientRow(Client client, bool isAssigned, Action onAction)
        {
            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(new GridLength(1.4, GridUnitType.Star)),
                    new ColumnDefinition(GridLength.Auto)
                },
                Padding = new Thickness(12, 8)
            };

            grid.Add(new Label { Text = client.Username, FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1E293B"), VerticalOptions = LayoutOptions.Center }, 0);
            grid.Add(new Label { Text = client.Email, FontSize = 13, TextColor = Color.FromArgb("#64748B"), VerticalOptions = LayoutOptions.Center }, 1);

            var btn = new Button
            {
                Text = isAssigned ? "✕ Remove" : "+ Assign to Me",
                HeightRequest = 32, FontSize = 12,
                Style = isAssigned
                    ? (Style)Application.Current!.Resources["DangerBtn"]
                    : (Style)Application.Current!.Resources["SuccessBtn"]
            };
            btn.Clicked += (s, e) => onAction();
            grid.Add(btn, 2);

            return new Border
            {
                BackgroundColor = isAssigned ? Color.FromArgb("#F0FDF4") : Colors.White,
                Stroke = Color.FromArgb("#E2E8F0"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                Padding = 0, Margin = new Thickness(0, 2),
                Content = grid
            };
        }

        // ===================================================================
        // WORKOUTS TAB — Workout Program Management
        // ===================================================================
        private void BuildWorkoutsView()
        {
            var clients = ClientService.GetAssignedClients(_trainer.TrainerId);

            contentArea.Children.Add(new Label { Text = Lang.Get("workout_title"), FontSize = 24, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1E293B") });

            // Form
            var formStack = new VerticalStackLayout { Spacing = 8 };

            var pckClient = new Picker { Title = Lang.Get("select_client"), TextColor = Color.FromArgb("#1E293B") };
            foreach (var c in clients) pckClient.Items.Add(c.Username);

            var txtProgramName = new Entry { Placeholder = Lang.Get("program_name"), Style = (Style)Application.Current!.Resources["ModernEntry"] };
            var txtDescription = new Editor { Placeholder = Lang.Get("description"), HeightRequest = 80, Style = (Style)Application.Current!.Resources["ModernEditor"] };

            var btnSave = new Button { Text = Lang.Get("save"), Style = (Style)Application.Current!.Resources["PrimaryBtn"] };
            var btnDelete = new Button { Text = Lang.Get("delete"), Style = (Style)Application.Current!.Resources["DangerBtn"] };

            formStack.Children.Add(new Label { Text = Lang.Get("select_client"), TextColor = Color.FromArgb("#64748B"), FontSize = 13 });
            formStack.Children.Add(pckClient);
            formStack.Children.Add(new Label { Text = Lang.Get("program_name"), TextColor = Color.FromArgb("#64748B"), FontSize = 13 });
            formStack.Children.Add(txtProgramName);
            formStack.Children.Add(new Label { Text = Lang.Get("description"), TextColor = Color.FromArgb("#64748B"), FontSize = 13 });
            formStack.Children.Add(txtDescription);

            var btnGrid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 10 };
            btnGrid.Add(btnSave, 0);
            btnGrid.Add(btnDelete, 1);
            formStack.Children.Add(btnGrid);

            contentArea.Children.Add(CreateCard(formStack));

            // Programs list
            var programsCollection = new CollectionView();
            var listStack = new VerticalStackLayout { Spacing = 4 };
            listStack.Children.Add(new Label { Text = Lang.Get("workout_title"), FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#334155"), Margin = new Thickness(0, 0, 0, 8) });
            listStack.Children.Add(programsCollection);
            contentArea.Children.Add(CreateCard(listStack));

            // Load programs when client selected
            pckClient.SelectedIndexChanged += (s, e) =>
            {
                if (pckClient.SelectedIndex < 0 || pckClient.SelectedIndex >= clients.Count) return;
                var client = clients[pckClient.SelectedIndex];
                LoadPrograms(client.ClientId, programsCollection);
            };

            // Save
            btnSave.Clicked += (s, e) =>
            {
                if (pckClient.SelectedIndex < 0 || string.IsNullOrWhiteSpace(txtProgramName.Text)) return;
                var client = clients[pckClient.SelectedIndex];
                WorkoutService.CreateProgram(_trainer.TrainerId, client.ClientId, txtProgramName.Text, txtDescription.Text ?? "");
                txtProgramName.Text = "";
                txtDescription.Text = "";
                LoadPrograms(client.ClientId, programsCollection);
            };

            // Delete
            btnDelete.Clicked += async (s, e) =>
            {
                if (pckClient.SelectedIndex < 0) return;
                var client = clients[pckClient.SelectedIndex];
                var programs = WorkoutService.GetProgramsByClient(_trainer.TrainerId, client.ClientId);
                if (!programs.Any()) return;

                bool confirm = await DisplayAlert(Lang.Get("confirm_delete"), Lang.Get("confirm_delete"), Lang.Get("yes"), Lang.Get("no"));
                if (confirm)
                {
                    WorkoutService.DeleteProgram(programs.Last().ProgramId);
                    LoadPrograms(client.ClientId, programsCollection);
                }
            };
        }

        private void LoadPrograms(int clientId, CollectionView cv)
        {
            var programs = WorkoutService.GetProgramsByClient(_trainer.TrainerId, clientId);
            cv.ItemsSource = programs;
            cv.ItemTemplate = new DataTemplate(() =>
            {
                var stack = new VerticalStackLayout();
                var nameLabel = new Label { FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1E293B") };
                nameLabel.SetBinding(Label.TextProperty, "ProgramName");
                var descLabel = new Label { FontSize = 12, TextColor = Color.FromArgb("#64748B") };
                descLabel.SetBinding(Label.TextProperty, "Description");
                var dateLabel = new Label { FontSize = 11, TextColor = Color.FromArgb("#94A3B8") };
                dateLabel.SetBinding(Label.TextProperty, "CreatedDate", stringFormat: "{0:dd/MM/yyyy}");
                stack.Children.Add(nameLabel);
                stack.Children.Add(descLabel);
                stack.Children.Add(dateLabel);

                return CreateFlatCard(stack);
            });
        }

        // ===================================================================
        // SESSIONS TAB — Virtual Session Management + DELEGATE Events + Live Sessions
        // ===================================================================
        private void BuildSessionsView()
        {
            var clients = ClientService.GetAssignedClients(_trainer.TrainerId);

            contentArea.Children.Add(new Label
            {
                Text = Lang.Get("sessions_title"),
                FontSize = 24, FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1E293B")
            });

            // ── Live Sessions Banner ─────────────────────────────────────────
            var liveStack = new VerticalStackLayout { Spacing = 6 };
            var liveBannerCard = CreateCard(liveStack, Color.FromArgb("#ECFDF5"));
            liveBannerCard.Stroke = Color.FromArgb("#10B981");
            liveBannerCard.StrokeThickness = 2;
            contentArea.Children.Add(liveBannerCard);

            var sessionsCollection = new CollectionView { SelectionMode = SelectionMode.Single };

            // ── Filters ──────────────────────────────────────────────────────
            var filterGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                },
                ColumnSpacing = 10
            };

            var pckClientFilter = new Picker { Title = Lang.Get("client_name"), TextColor = Color.FromArgb("#1E293B") };
            pckClientFilter.Items.Add(Lang.Get("all_clients"));
            foreach (var c in clients) pckClientFilter.Items.Add(c.Username);
            pckClientFilter.SelectedIndex = 0;

            var pckStatus = new Picker { Title = Lang.Get("status"), TextColor = Color.FromArgb("#1E293B") };
            pckStatus.Items.Add(Lang.Get("all_types"));
            pckStatus.Items.Add("scheduled");
            pckStatus.Items.Add("active");
            pckStatus.Items.Add("completed");
            pckStatus.Items.Add("canceled");
            pckStatus.SelectedIndex = 0;

            var btnRefreshSessions = new Button { Text = "⟳ " + Lang.Get("refresh"), Style = (Style)Application.Current!.Resources["PrimaryBtn"] };

            filterGrid.Add(pckClientFilter, 0);
            filterGrid.Add(pckStatus, 1);
            filterGrid.Add(btnRefreshSessions, 2);
            contentArea.Children.Add(CreateCard(filterGrid));
            contentArea.Children.Add(CreateCard(sessionsCollection));

            // ── Action Buttons ───────────────────────────────────────────────
            var actionGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Star)
                },
                ColumnSpacing = 10
            };
            var btnStartLive = new Button { Text = "🟢 Start Session", Style = (Style)Application.Current!.Resources["PrimaryBtn"], BackgroundColor = Color.FromArgb("#059669") };
            var btnComplete  = new Button { Text = Lang.Get("mark_completed"), Style = (Style)Application.Current!.Resources["SuccessBtn"] };
            var btnCancel    = new Button { Text = Lang.Get("cancel_session"), Style = (Style)Application.Current!.Resources["DangerBtn"] };
            actionGrid.Add(btnStartLive, 0);
            actionGrid.Add(btnComplete, 1);
            actionGrid.Add(btnCancel, 2);
            contentArea.Children.Add(actionGrid);

            // ── Load Sessions (LINQ QUERY #2 — filtering) ───────────────────
            void LoadSessions()
            {
                List<SessionDisplay> allSessions;
                try { allSessions = SessionService.GetSessionsByTrainer(_trainer.TrainerId, clients); }
                catch (Exception ex)
                {
                    sessionsCollection.ItemsSource = null;
                    liveStack.Children.Clear();
                    liveBannerCard.IsVisible = false;
                    _ = DisplayAlert(Lang.Get("connection_error"), ex.Message, "OK");
                    return;
                }

                // Live sessions banner
                liveStack.Children.Clear();
                var liveSessions = allSessions.Where(s => s.Status == "active").ToList();
                if (liveSessions.Any())
                {
                    liveBannerCard.IsVisible = true;
                    liveStack.Children.Add(new Label
                    {
                        Text = $"🟢  {liveSessions.Count} ACTIVE SESSION{(liveSessions.Count > 1 ? "S" : "")}",
                        FontSize = 16, FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#059669")
                    });
                    foreach (var live in liveSessions)
                    {
                        var liveRow = new HorizontalStackLayout { Spacing = 12, Margin = new Thickness(0, 4, 0, 0) };
                        liveRow.Children.Add(new Label { Text = "▶", FontSize = 14, TextColor = Color.FromArgb("#059669"), VerticalOptions = LayoutOptions.Center });
                        liveRow.Children.Add(new Label
                        {
                            Text = $"{live.ClientName}  ·  started {live.SessionTimeDisplay}  ·  {live.DurationMinutes} min",
                            FontSize = 13, TextColor = Color.FromArgb("#065F46"), VerticalOptions = LayoutOptions.Center
                        });
                        liveStack.Children.Add(liveRow);
                    }
                }
                else
                {
                    liveBannerCard.IsVisible = false;
                }

                // LINQ filtering
                var filtered = allSessions.AsEnumerable();
                if (pckClientFilter.SelectedIndex > 0)
                {
                    var clientName = pckClientFilter.Items[pckClientFilter.SelectedIndex];
                    filtered = filtered.Where(s => s.ClientName == clientName);
                }
                if (pckStatus.SelectedIndex > 0)
                {
                    var status = pckStatus.Items[pckStatus.SelectedIndex];
                    filtered = filtered.Where(s => s.Status == status);
                }
                var result = filtered.OrderByDescending(s => s.SessionTime).ToList();

                sessionsCollection.ItemsSource = result;
                sessionsCollection.ItemTemplate = new DataTemplate(() =>
                {
                    var grid = new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitionCollection
                        {
                            new ColumnDefinition(GridLength.Star),
                            new ColumnDefinition(GridLength.Star),
                            new ColumnDefinition(new GridLength(0.6, GridUnitType.Star)),
                            new ColumnDefinition(new GridLength(0.9, GridUnitType.Star))
                        },
                        Padding = new Thickness(12, 10)
                    };

                    var clientLabel = new Label { FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1E293B"), VerticalOptions = LayoutOptions.Center };
                    clientLabel.SetBinding(Label.TextProperty, "ClientName");

                    var timeLabel = new Label { FontSize = 13, TextColor = Color.FromArgb("#64748B"), VerticalOptions = LayoutOptions.Center };
                    timeLabel.SetBinding(Label.TextProperty, "SessionTimeDisplay");

                    var durLabel = new Label { FontSize = 13, TextColor = Color.FromArgb("#64748B"), VerticalOptions = LayoutOptions.Center };
                    durLabel.SetBinding(Label.TextProperty, "DurationMinutes", stringFormat: "{0} min");

                    var statusLabel = new Label { FontSize = 13, FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center };
                    statusLabel.SetBinding(Label.TextProperty, "StatusDisplay");
                    statusLabel.SetBinding(Label.TextColorProperty, "StatusColor");

                    grid.Add(clientLabel, 0);
                    grid.Add(timeLabel, 1);
                    grid.Add(durLabel, 2);
                    grid.Add(statusLabel, 3);

                    var border = new Border
                    {
                        Stroke = Color.FromArgb("#E2E8F0"),
                        StrokeThickness = 1,
                        StrokeShape = new RoundRectangle { CornerRadius = 8 },
                        Padding = 0, Margin = new Thickness(0, 2),
                        Content = grid
                    };
                    border.SetBinding(Border.BackgroundColorProperty, new Binding("IsLive",
                        converter: new LiveColorConverter()));
                    return border;
                });
            }

            LoadSessions();
            btnRefreshSessions.Clicked += (s, e) => LoadSessions();
            pckClientFilter.SelectedIndexChanged += (s, e) => LoadSessions();
            pckStatus.SelectedIndexChanged += (s, e) => LoadSessions();

            // Start Live — DELEGATE event fire
            btnStartLive.Clicked += async (s, e) =>
            {
                if (sessionsCollection.SelectedItem is not SessionDisplay session)
                {
                    await DisplayAlert("Start Live", "Please select a session from the list first.", "OK");
                    return;
                }
                if (session.Status != "scheduled")
                {
                    await DisplayAlert("Start Session", $"Only 'scheduled' sessions can be started. Selected session is '{session.StatusDisplay}'.", "OK");
                    return;
                }
                SessionService.UpdateSessionStatus(session.SessionId, "active");
                NotificationService.NotifySessionStarted(session.SessionId, session.ClientName, session.SessionTime);
                LoadSessions();
            };

            // Complete — DELEGATE event fire
            btnComplete.Clicked += (s, e) =>
            {
                if (sessionsCollection.SelectedItem is SessionDisplay session &&
                    (session.Status == "scheduled" || session.Status == "active"))
                {
                    SessionService.UpdateSessionStatus(session.SessionId, "completed");
                    NotificationService.NotifySessionCompleted(session.SessionId, session.ClientName, session.SessionTime);
                    LoadSessions();
                }
            };

            // Cancel — DELEGATE event fire
            btnCancel.Clicked += (s, e) =>
            {
                if (sessionsCollection.SelectedItem is SessionDisplay session &&
                    (session.Status == "scheduled" || session.Status == "active"))
                {
                    SessionService.UpdateSessionStatus(session.SessionId, "canceled");
                    NotificationService.NotifySessionCanceled(session.SessionId, session.ClientName, session.SessionTime);
                    LoadSessions();
                }
            };
        }

        // ===================================================================
        // GOALS TAB — Set / View Goals per Client
        // ===================================================================
        private void BuildGoalsView()
        {
            var clients = ClientService.GetAssignedClients(_trainer.TrainerId);

            contentArea.Children.Add(new Label
            {
                Text = "🎯 " + Lang.Get("goals_title"),
                FontSize = 24, FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1E293B")
            });

            // ── Create Goal Form ──────────────────────────────────────────────
            var formStack = new VerticalStackLayout { Spacing = 8 };

            var pckClient = new Picker { Title = Lang.Get("select_client"), TextColor = Color.FromArgb("#1E293B") };
            foreach (var c in clients) pckClient.Items.Add(c.Username);

            var pckGoalType = new Picker { Title = Lang.Get("goal_type"), TextColor = Color.FromArgb("#1E293B") };
            pckGoalType.Items.Add("weight_loss");
            pckGoalType.Items.Add("strength_target");
            pckGoalType.Items.Add("weekly_exercise_frequency");
            pckGoalType.Items.Add("calorie_target");

            var txtTarget  = new Entry { Placeholder = Lang.Get("target_value"), Keyboard = Keyboard.Numeric, Style = (Style)Application.Current!.Resources["ModernEntry"] };
            var dpDeadline = new DatePicker { Date = DateTime.Today.AddMonths(1), TextColor = Color.FromArgb("#1E293B"), BackgroundColor = Color.FromArgb("#F1F5F9") };
            var btnCreate  = new Button { Text = Lang.Get("create_goal"), Style = (Style)Application.Current!.Resources["PrimaryBtn"] };

            formStack.Children.Add(new Label { Text = Lang.Get("select_client"), TextColor = Color.FromArgb("#64748B"), FontSize = 13 });
            formStack.Children.Add(pckClient);
            formStack.Children.Add(new Label { Text = Lang.Get("goal_type"),     TextColor = Color.FromArgb("#64748B"), FontSize = 13 });
            formStack.Children.Add(pckGoalType);

            var targetRow = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 10 };
            var targetCol = new VerticalStackLayout { Spacing = 4 };
            targetCol.Children.Add(new Label { Text = Lang.Get("target_value"), TextColor = Color.FromArgb("#64748B"), FontSize = 13 });
            targetCol.Children.Add(txtTarget);
            var deadlineCol = new VerticalStackLayout { Spacing = 4 };
            deadlineCol.Children.Add(new Label { Text = Lang.Get("deadline") + " (optional)", TextColor = Color.FromArgb("#64748B"), FontSize = 13 });
            deadlineCol.Children.Add(dpDeadline);
            targetRow.Add(targetCol,   0);
            targetRow.Add(deadlineCol, 1);
            formStack.Children.Add(targetRow);
            formStack.Children.Add(btnCreate);

            contentArea.Children.Add(CreateCard(formStack));

            // ── Goals List ────────────────────────────────────────────────────
            var goalsListStack = new VerticalStackLayout { Spacing = 4 };
            var goalsHeader = new Label
            {
                Text = Lang.Get("client_goals"),
                FontSize = 16, FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#334155"), Margin = new Thickness(0, 0, 0, 8)
            };
            goalsListStack.Children.Add(goalsHeader);
            var goalsCard = CreateCard(goalsListStack);
            contentArea.Children.Add(goalsCard);

            void LoadGoals()
            {
                if (pckClient.SelectedIndex < 0 || pckClient.SelectedIndex >= clients.Count) return;
                var client = clients[pckClient.SelectedIndex];
                var goals  = GoalService.GetGoalsForClient(client.ClientId);

                while (goalsListStack.Children.Count > 1)
                    goalsListStack.Children.RemoveAt(goalsListStack.Children.Count - 1);

                if (!goals.Any())
                {
                    goalsListStack.Children.Add(new Label
                    {
                        Text = Lang.Get("no_goals"),
                        TextColor = Color.FromArgb("#94A3B8"),
                        HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 12)
                    });
                    return;
                }

                foreach (var goal in goals)
                {
                    var unit = goal.GoalType switch
                    {
                        "weight_loss"                => "kg",
                        "weekly_exercise_frequency"  => "sessions/wk",
                        "calorie_target"             => "kcal",
                        _                            => ""
                    };

                    var rowGrid = new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitionCollection
                        {
                            new ColumnDefinition(new GridLength(1.3, GridUnitType.Star)),
                            new ColumnDefinition(GridLength.Star),
                            new ColumnDefinition(GridLength.Star),
                            new ColumnDefinition(new GridLength(0.9, GridUnitType.Star)),
                            new ColumnDefinition(GridLength.Auto)
                        },
                        Padding = new Thickness(10, 8)
                    };

                    var typeLabel = goal.GoalType.Replace("_", " ");
                    typeLabel = char.ToUpper(typeLabel[0]) + typeLabel[1..];

                    rowGrid.Add(new Label { Text = typeLabel,                         FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1E293B"), VerticalOptions = LayoutOptions.Center }, 0);
                    rowGrid.Add(new Label { Text = $"Target: {goal.TargetValue} {unit}",  FontSize = 12, TextColor = Color.FromArgb("#64748B"), VerticalOptions = LayoutOptions.Center }, 1);
                    rowGrid.Add(new Label { Text = $"Current: {goal.CurrentValue?.ToString() ?? "—"}", FontSize = 12, TextColor = Color.FromArgb("#64748B"), VerticalOptions = LayoutOptions.Center }, 2);

                    var statusColor = goal.Status switch
                    {
                        "completed"   => Color.FromArgb("#059669"),
                        "missed"      => Color.FromArgb("#DC2626"),
                        _             => Color.FromArgb("#D97706")
                    };
                    rowGrid.Add(new Label { Text = (goal.Status ?? "in_progress").Replace("_", " ").ToUpper(), FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = statusColor, VerticalOptions = LayoutOptions.Center }, 3);

                    var delBtn = new Button { Text = "✕", HeightRequest = 32, WidthRequest = 36, FontSize = 11, Style = (Style)Application.Current!.Resources["DangerBtn"] };
                    var gid    = goal.GoalId;
                    delBtn.Clicked += (s, e) => { GoalService.DeleteGoal(gid); LoadGoals(); };
                    rowGrid.Add(delBtn, 4);

                    goalsListStack.Children.Add(new Border
                    {
                        Stroke = Color.FromArgb("#E2E8F0"), StrokeThickness = 1,
                        StrokeShape = new RoundRectangle { CornerRadius = 6 },
                        Padding = 0, Margin = new Thickness(0, 2),
                        Content = rowGrid
                    });
                }
            }

            pckClient.SelectedIndexChanged += (s, e) => LoadGoals();

            btnCreate.Clicked += (s, e) =>
            {
                if (pckClient.SelectedIndex < 0 || pckClient.SelectedIndex >= clients.Count) return;
                if (pckGoalType.SelectedIndex < 0) return;
                if (!decimal.TryParse(txtTarget.Text, out var targetVal) || targetVal <= 0) return;

                var client   = clients[pckClient.SelectedIndex];
                var goalType = pckGoalType.Items[pckGoalType.SelectedIndex];
                GoalService.CreateGoal(client.ClientId, goalType, targetVal, dpDeadline.Date);

                txtTarget.Text         = "";
                pckGoalType.SelectedIndex = -1;
                LoadGoals();
            };
        }

        // ===================================================================
        // REPORTS TAB — Progress Reports (LINQ)
        // ===================================================================
        private void BuildReportsView()
        {
            var clients = ClientService.GetAssignedClients(_trainer.TrainerId);

            contentArea.Children.Add(new Label { Text = Lang.Get("reports_title"), FontSize = 24, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1E293B") });

            // Filters
            var filterGrid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 10 };

            var pckClient = new Picker { Title = Lang.Get("select_client"), TextColor = Color.FromArgb("#1E293B") };
            foreach (var c in clients) pckClient.Items.Add(c.Username);
            if (clients.Any()) pckClient.SelectedIndex = 0;

            var dpFrom = new DatePicker { Date = DateTime.Today.AddDays(-30), TextColor = Color.FromArgb("#1E293B"), BackgroundColor = Color.FromArgb("#F1F5F9") };
            var dpTo   = new DatePicker { Date = DateTime.Today,              TextColor = Color.FromArgb("#1E293B"), BackgroundColor = Color.FromArgb("#F1F5F9") };
            var btnGenerate = new Button { Text = Lang.Get("generate_report"), Style = (Style)Application.Current!.Resources["PrimaryBtn"] };

            filterGrid.Add(pckClient, 0); filterGrid.Add(dpFrom, 1); filterGrid.Add(dpTo, 2); filterGrid.Add(btnGenerate, 3);
            contentArea.Children.Add(CreateCard(filterGrid));

            // Results area
            var resultsStack = new VerticalStackLayout { Spacing = 12 };
            contentArea.Children.Add(resultsStack);

            btnGenerate.Clicked += (s, e) =>
            {
                if (pckClient.SelectedIndex < 0 || pckClient.SelectedIndex >= clients.Count) return;
                var client = clients[pckClient.SelectedIndex];
                resultsStack.Children.Clear();

                // Exercise LINQ analysis
                var logs = ReportService.GetExerciseLogs(client.ClientId);
                var weekly = ReportService.GetWeeklyAverages(logs, dpFrom.Date ?? DateTime.Today.AddDays(-30), dpTo.Date ?? DateTime.Today, null);

                resultsStack.Children.Add(new Label { Text = "🏃 " + Lang.Get("weekly_avg"), FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#334155") });
                foreach (var w in weekly)
                {
                    resultsStack.Children.Add(CreateFlatCard(new HorizontalStackLayout
                    {
                        Spacing = 20,
                        Children =
                        {
                            new Label { Text = w.WeekLabel, FontAttributes = FontAttributes.Bold, WidthRequest = 80, TextColor = Color.FromArgb("#1E293B") },
                            new Label { Text = $"Avg: {w.AvgDuration} min", TextColor = Color.FromArgb("#64748B") },
                            new Label { Text = $"Cal: {w.AvgCalories}", TextColor = Color.FromArgb("#64748B") },
                            new Label { Text = $"Sessions: {w.TotalSessions}", TextColor = Color.FromArgb("#64748B") }
                        }
                    }));
                }

                // Goals LINQ analysis
                var goals = ReportService.GetGoals(client.ClientId);
                var stats = ReportService.GetGoalStats(goals, null);

                resultsStack.Children.Add(new Label { Text = "🎯 " + Lang.Get("goal_completion"), FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#334155"), Margin = new Thickness(0, 8, 0, 0) });

                var goalGrid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 10 };
                goalGrid.Add(CreateStatCard("📋", stats.total.ToString(), "Total", "#4F46E5"), 0);
                goalGrid.Add(CreateStatCard("✅", stats.completed.ToString(), "Completed", "#059669"), 1);
                goalGrid.Add(CreateStatCard("⏳", stats.inProgress.ToString(), "In Progress", "#D97706"), 2);
                goalGrid.Add(CreateStatCard("📊", $"{stats.rate}%", "Rate", "#DC2626"), 3);
                resultsStack.Children.Add(goalGrid);

                // Health metrics
                var metrics = ReportService.GetHealthMetrics(client.ClientId);
                var filtered = ReportService.GetFilteredHealthMetrics(metrics, dpFrom.Date ?? DateTime.Today.AddDays(-30), dpTo.Date ?? DateTime.Today);

                resultsStack.Children.Add(new Label { Text = "❤️ Health Metrics", FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#334155"), Margin = new Thickness(0, 8, 0, 0) });
                foreach (var m in filtered)
                {
                    resultsStack.Children.Add(CreateFlatCard(new HorizontalStackLayout
                    {
                        Spacing = 16,
                        Children =
                        {
                            new Label { Text = m.DateDisplay, FontAttributes = FontAttributes.Bold, WidthRequest = 90, TextColor = Color.FromArgb("#1E293B") },
                            new Label { Text = $"W: {m.WeightDisplay}kg", TextColor = Color.FromArgb("#64748B") },
                            new Label { Text = $"HR: {m.HeartRateDisplay}", TextColor = Color.FromArgb("#64748B") },
                            new Label { Text = $"Sleep: {m.SleepDisplay}h", TextColor = Color.FromArgb("#64748B") },
                            new Label { Text = $"Water: {m.WaterDisplay}L", TextColor = Color.FromArgb("#64748B") }
                        }
                    }));
                }
            };
        }
    }
}
