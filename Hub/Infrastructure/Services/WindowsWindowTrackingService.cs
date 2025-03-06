using Hub.Application;
using Hub.Domain;
using Hub.Presentation;
using Microsoft.AspNetCore.SignalR;
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
            _logger.LogInformation("Initializing WindowsWindowTrackingService...");

            try
            {
                _activeWindowTracker = new ActiveWindowTracker();
                _activeWindowTracker.ActiveWindowChanged += OnActiveWindowChanged;
                _logger.LogInformation("ActiveWindowTracker instantiated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error instantiating ActiveWindowTracker.");
            }
        }

        private async void OnActiveWindowChanged(object sender, ActiveWindowEventArgs e)
        {
            DateTime now = DateTime.Now;
            _logger.LogDebug("ActiveWindowChanged event triggered for: {ProcessName} at {Now}", e.ProcessName, now);

            if (e.ProcessName == _lastProcessName)
            {
                _logger.LogDebug("Unchanged process ({ProcessName}), ignoring.", e.ProcessName);
                return;
            }

            using (var scope = _scopeFactory.CreateScope())
            {
                var appRepository = scope.ServiceProvider.GetRequiredService<IApplicationRepository>();

                if (!string.IsNullOrEmpty(_lastProcessName))
                {
                    var previousApp = await appRepository.GetByProcessAndDateAsync(_lastProcessName, DateOnly.FromDateTime(now));
                    if (previousApp != null)
                    {
                        previousApp.StopTracking(now);
                        _logger.LogDebug("StopTracking called for: {ProcessName}", _lastProcessName);
                        await appRepository.SaveChangesAsync();
                        _logger.LogInformation("Tracking stopped for: {ProcessName}. Total time: {TotalTime}",
                            _lastProcessName, previousApp.TotalTimeSpent);
                        await _hubContext.Clients.All.SendAsync("WindowTrackingStopped", _lastProcessName, previousApp.TotalTimeSpent.TotalSeconds);
                    }
                }

                _lastProcessName = e.ProcessName;
                var currentDate = DateOnly.FromDateTime(now);
                var appRecord = await appRepository.GetByProcessAndDateAsync(e.ProcessName, currentDate);

                if (appRecord == null)
                {
                    appRecord = new ApplicationOS(e.ProcessName, e.ProcessName);
                    appRecord.StartTracking(now);
                    await appRepository.AddAsync(appRecord);
                    _logger.LogInformation("New record created for: {ProcessName}", e.ProcessName);
                }
                else
                {
                    appRecord.StartTracking(now);
                    _logger.LogInformation("Tracking resumed for: {ProcessName}", e.ProcessName);
                }

                await appRepository.SaveChangesAsync();
                _logger.LogDebug("Data saved in database for process: {ProcessName}", e.ProcessName);
                await _hubContext.Clients.All.SendAsync("WindowTrackingStarted", e.ProcessName);
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WindowTrackingService execution started.");
            stoppingToken.Register(() => _logger.LogInformation("Cancellation token triggered."));
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
            _logger.LogInformation("Windows window tracking service has been terminated.");
        }
    }
}
