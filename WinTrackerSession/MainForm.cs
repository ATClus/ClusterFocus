using Microsoft.AspNetCore.SignalR.Client;
using WinTracker;

namespace WinTrackerSession
{
    public partial class MainForm : Form
    {
        private NotifyIcon _trayIcon;
        private ContextMenuStrip _contextMenu;
        private HubConnection _hubConnection;
        private string _currentProcessName = string.Empty;
        private ActiveWindowTracker _activeWindowTracker;

        private ToolStripMenuItem _startItem;
        private ToolStripMenuItem _stopItem;
        private ToolStripMenuItem _quitItem;

        public MainForm()
        {
            // InitializeComponent();

            InitializeTrayIcon();
            InitializeSignalR();

            _activeWindowTracker = new ActiveWindowTracker();
            _activeWindowTracker.ActiveWindowChanged += ActiveWindowTracker_ActiveWindowChanged;

            UpdateMenuItems();
        }

        private async void InitializeSignalR()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7131/appfocused")
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.Closed += async (error) =>
            {
                await System.Threading.Tasks.Task.Delay(new Random().Next(0, 5) * 1000);
                await _hubConnection.StartAsync();
            };

            try
            {
                await _hubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to the Hub: {ex.Message}");
            }
        }

        private async void ActiveWindowTracker_ActiveWindowChanged(object sender, ActiveWindowEventArgs e)
        {
            string newProcess = e.ProcessName;

            if (!string.Equals(newProcess, _currentProcessName, StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(_currentProcessName))
                {
                    try
                    {
                        await _hubConnection.SendAsync("StopApplicationTracking", _currentProcessName);
                    }
                    catch (Exception ex)
                    {
                    }
                }

                _currentProcessName = newProcess;
                try
                {
                    await _hubConnection.SendAsync("StartApplicationTracking", _currentProcessName);
                }
                catch (Exception ex)
                {
                }
                UpdateMenuItems();
            }
        }

        private void InitializeTrayIcon()
        {
            _contextMenu = new ContextMenuStrip();

            _startItem = new ToolStripMenuItem("Start");
            _startItem.Click += async (sender, e) =>
            {
                if (!string.IsNullOrEmpty(_currentProcessName))
                {
                    MessageBox.Show("Tracking já iniciado para: " + _currentProcessName);
                }
                else
                {
                    _currentProcessName = "ClusterFocus";
                    try
                    {
                        await _hubConnection.SendAsync("StartApplicationTracking", _currentProcessName);
                        MessageBox.Show("Tracking iniciado para: " + _currentProcessName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erro ao iniciar tracking: " + ex.Message);
                    }
                    UpdateMenuItems();
                }
            };
            _contextMenu.Items.Add(_startItem);

            _stopItem = new ToolStripMenuItem("Stop");
            _stopItem.Click += async (sender, e) =>
            {
                if (!string.IsNullOrEmpty(_currentProcessName))
                {
                    try
                    {
                        await _hubConnection.SendAsync("StopApplicationTracking", _currentProcessName);
                        MessageBox.Show("Tracking parado para: " + _currentProcessName);
                        _currentProcessName = string.Empty;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erro ao parar tracking: " + ex.Message);
                    }
                    UpdateMenuItems();
                }
                else
                {
                    MessageBox.Show("Nenhum tracking ativo.");
                }
            };
            _contextMenu.Items.Add(_stopItem);

            _quitItem = new ToolStripMenuItem("Quit");
            _quitItem.Click += (sender, e) =>
            {
                _trayIcon.Visible = false;
                Application.Exit();
            };
            _contextMenu.Items.Add(_quitItem);

            _trayIcon = new NotifyIcon
            {
                Icon = new Icon("appicon.ico"),
                Text = "WinTracker Session",
                ContextMenuStrip = _contextMenu,
                Visible = true
            };
        }

        private void UpdateMenuItems()
        {
            if (!string.IsNullOrEmpty(_currentProcessName))
            {
                _startItem.Enabled = false;
                _stopItem.Enabled = true;
            }
            else
            {
                _startItem.Enabled = true;
                _stopItem.Enabled = false;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;
            base.OnLoad(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _trayIcon.Dispose();
            _activeWindowTracker?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
