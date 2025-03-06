
using Hub.Application;
using Hub.Domain;
using Microsoft.AspNetCore.SignalR;

namespace Hub.Presentation
{
    public class AppFocusHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly IApplicationRepository _appRepository;

        public AppFocusHub(IApplicationRepository appRepository)
        {
            _appRepository = appRepository;
        }

        public async Task StartApplicationTracking(string processName)
        {
            var currentDate = DateOnly.FromDateTime(DateTime.Now);
            var now = DateTime.Now;

            var app = await _appRepository.GetByProcessNameAsync(processName);

            if (app == null)
            {
                app = new ApplicationOS(processName, processName);
                var timeEntry = new TimeEntry(now) { Application = app };
                app.Sessions.Add(timeEntry);
                await _appRepository.AddAsync(app);
            }
            else
            {
                var entryDoDia = app.Sessions.FirstOrDefault(e => e.Date == currentDate.ToDateTime(TimeOnly.MinValue).Date);

                if (entryDoDia == null)
                {
                    var timeEntry = new TimeEntry(now) { Application = app };
                    app.Sessions.Add(timeEntry);
                }
                else
                {
                    entryDoDia.StartTracking(now);
                }
            }

            await _appRepository.SaveChangesAsync();
            await Clients.All.SendAsync("ApplicationTrackingStarted", processName);
        }

        public async Task StopApplicationTracking(string processName)
        {
            var currentDate = DateOnly.FromDateTime(DateTime.Now);
            var now = DateTime.Now;

            var app = await _appRepository.GetByProcessNameAsync(processName);
            if (app != null)
            {
                var activeEntry = app.Sessions.LastOrDefault(te => te.CurrentSessionStart.HasValue);
                if (activeEntry != null)
                {
                    activeEntry.StopTracking(now);
                }
                await _appRepository.SaveChangesAsync();

                await Clients.All.SendAsync("ApplicationTrackingStopped", processName, app.TotalTimeSpent.TotalSeconds);
            }
        }
    }
}
