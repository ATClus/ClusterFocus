using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using WinTracker; // Sua library com ActiveWindowTracker e ActiveWindowEventArgs

namespace WinTrackerSession
{
    public partial class MainForm : Form
    {
        private NotifyIcon _trayIcon;
        private ContextMenuStrip _contextMenu;
        private HubConnection _hubConnection;
        // Armazena o nome do processo atualmente monitorado (tracking ativo)
        private string _currentProcessName = string.Empty;
        // Instância para capturar os eventos de mudança de janela
        private ActiveWindowTracker _activeWindowTracker;

        // Campos para os itens de menu (para controle do Enabled)
        private ToolStripMenuItem _startItem;
        private ToolStripMenuItem _stopItem;
        private ToolStripMenuItem _quitItem;

        public MainForm()
        {
            // Se não estiver usando o designer, não é necessário chamar InitializeComponent();
            // InitializeComponent();

            InitializeTrayIcon();
            InitializeSignalR();

            _activeWindowTracker = new ActiveWindowTracker();
            _activeWindowTracker.ActiveWindowChanged += ActiveWindowTracker_ActiveWindowChanged;

            // Atualiza o estado dos itens de menu conforme o tracking (inicialmente parado)
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
                MessageBox.Show($"Erro ao conectar ao Hub: {ex.Message}");
            }
        }

        // Callback do ActiveWindowTracker – chamado quando há mudança na janela ativa
        private async void ActiveWindowTracker_ActiveWindowChanged(object sender, ActiveWindowEventArgs e)
        {
            string newProcess = e.ProcessName;
            //Console.WriteLine($"Janela ativa detectada: {newProcess} (PID: {e.ProcessId})");

            // Se houve mudança de janela, envia os comandos de Stop para o tracking anterior e Start para o novo
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
                        //Console.WriteLine($"Erro ao parar tracking: {ex.Message}");
                    }
                }

                _currentProcessName = newProcess;
                try
                {
                    await _hubConnection.SendAsync("StartApplicationTracking", _currentProcessName);
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"Erro ao iniciar tracking: {ex.Message}");
                }
                UpdateMenuItems();
            }
        }

        private void InitializeTrayIcon()
        {
            _contextMenu = new ContextMenuStrip();

            // Cria e armazena o item "Start"
            _startItem = new ToolStripMenuItem("Start");
            _startItem.Click += async (sender, e) =>
            {
                if (!string.IsNullOrEmpty(_currentProcessName))
                {
                    MessageBox.Show("Tracking já iniciado para: " + _currentProcessName);
                }
                else
                {
                    // Exemplo: define um valor fixo ou permite escolha do usuário
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

            // Cria e armazena o item "Stop"
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

            // Cria e armazena o item "Quit"
            _quitItem = new ToolStripMenuItem("Quit");
            _quitItem.Click += (sender, e) =>
            {
                _trayIcon.Visible = false;
                Application.Exit();
            };
            _contextMenu.Items.Add(_quitItem);

            // Cria o NotifyIcon
            _trayIcon = new NotifyIcon
            {
                Icon = new Icon("appicon.ico"), // Certifique-se de que "appicon.ico" esteja na pasta de saída
                Text = "WinTracker Session",
                ContextMenuStrip = _contextMenu,
                Visible = true
            };
        }

        // Atualiza o estado dos itens do menu com base no tracking ativo
        private void UpdateMenuItems()
        {
            // Se _currentProcessName estiver preenchido (tracking ativo), desabilita "Start" e habilita "Stop"
            if (!string.IsNullOrEmpty(_currentProcessName))
            {
                _startItem.Enabled = false;
                _stopItem.Enabled = true;
            }
            else // Caso contrário, habilita "Start" e desabilita "Stop"
            {
                _startItem.Enabled = true;
                _stopItem.Enabled = false;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            // Oculta o formulário principal para que ele não apareça na taskbar
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
