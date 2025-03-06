using System;
using System.Threading;
using System.Threading.Tasks;
using Hub.Application;
using Hub.Domain;
using Hub.Presentation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WinTracker;

namespace Hub.Infrastructure.Services
{
    public class WindowsWindowTrackingService : BackgroundService
    {
        private readonly IHubContext<TabFocus> _hubContext;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WindowsWindowTrackingService> _logger;
        private readonly ActiveWindowTracker _activeWindowTracker;
        private string _lastProcessName = string.Empty;

        public WindowsWindowTrackingService(IHubContext<TabFocus> hubContext, IServiceScopeFactory scopeFactory, ILogger<WindowsWindowTrackingService> logger)
        {
            _hubContext = hubContext;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _logger.LogInformation("Inicializando WindowsWindowTrackingService...");

            try
            {
                _activeWindowTracker = new ActiveWindowTracker();
                _activeWindowTracker.ActiveWindowChanged += OnActiveWindowChanged;
                _logger.LogInformation("ActiveWindowTracker instanciado com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao instanciar o ActiveWindowTracker.");
            }
        }

        private async void OnActiveWindowChanged(object sender, ActiveWindowEventArgs e)
        {
            DateTime now = DateTime.Now;
            _logger.LogDebug("Evento ActiveWindowChanged disparado para: {ProcessName} às {Now}", e.ProcessName, now);

            if (e.ProcessName == _lastProcessName)
            {
                _logger.LogDebug("Processo inalterado ({ProcessName}), ignorando.", e.ProcessName);
                return;
            }

            using (var scope = _scopeFactory.CreateScope())
            {
                var appRepository = scope.ServiceProvider.GetRequiredService<IApplicationRepository>();

                // Se havia um aplicativo sendo trackeado, interrompe o tracking
                if (!string.IsNullOrEmpty(_lastProcessName))
                {
                    var previousApp = await appRepository.GetByProcessAndDateAsync(_lastProcessName, DateOnly.FromDateTime(now));
                    if (previousApp != null)
                    {
                        previousApp.StopTracking(now);
                        _logger.LogDebug("StopTracking chamado para: {ProcessName}", _lastProcessName);
                        await appRepository.SaveChangesAsync();
                        _logger.LogInformation("Tracking parado para: {ProcessName}. Tempo total: {TotalTime}",
                            _lastProcessName, previousApp.TotalTimeSpent);
                        await _hubContext.Clients.All.SendAsync("WindowTrackingStopped", _lastProcessName, previousApp.TotalTimeSpent.TotalSeconds);
                    }
                }

                // Atualiza o registro para o novo aplicativo
                _lastProcessName = e.ProcessName;
                var currentDate = DateOnly.FromDateTime(now);
                var appRecord = await appRepository.GetByProcessAndDateAsync(e.ProcessName, currentDate);

                if (appRecord == null)
                {
                    appRecord = new ApplicationOS(e.ProcessName, e.ProcessName);
                    appRecord.StartTracking(now);
                    await appRepository.AddAsync(appRecord);
                    _logger.LogInformation("Novo registro criado para: {ProcessName}", e.ProcessName);
                }
                else
                {
                    appRecord.StartTracking(now);
                    _logger.LogInformation("Tracking retomado para: {ProcessName}", e.ProcessName);
                }

                await appRepository.SaveChangesAsync();
                _logger.LogDebug("Dados salvos no banco para o processo: {ProcessName}", e.ProcessName);
                await _hubContext.Clients.All.SendAsync("WindowTrackingStarted", e.ProcessName);
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Execução do WindowsWindowTrackingService iniciada.");
            stoppingToken.Register(() => _logger.LogInformation("Token de cancelamento disparado."));
            return Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override void Dispose()
        {
            base.Dispose();
            if (_activeWindowTracker != null)
            {
                _activeWindowTracker.ActiveWindowChanged -= OnActiveWindowChanged;
                _activeWindowTracker.Dispose();
            }
            _logger.LogInformation("Serviço de tracking de janelas finalizado.");
        }
    }
}
