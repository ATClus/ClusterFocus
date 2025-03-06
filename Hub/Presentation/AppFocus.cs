using Hub.Application;
using Hub.Domain;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;

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

            // Busca a ApplicationOS para o dia atual
            var app = await _appRepository.GetByProcessAndDateAsync(processName, currentDate);

            if (app == null)
            {
                // Cria a ApplicationOS e o TimeEntry do dia
                app = new ApplicationOS(processName, processName);
                var timeEntry = new TimeEntry(now) { Application = app };
                app.Sessions.Add(timeEntry);
                await _appRepository.AddAsync(app);
            }
            else
            {
                // Procura o TimeEntry "do dia"
                var entryDoDia = app.Sessions.FirstOrDefault(e => e.Date == currentDate.ToDateTime(TimeOnly.MinValue).Date);

                if (entryDoDia == null)
                {
                    // Se não existe registro para o dia, cria um novo
                    var timeEntry = new TimeEntry(now) { Application = app };
                    app.Sessions.Add(timeEntry);
                }
                else
                {
                    // Retoma o tracking do mesmo registro do dia
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

            // Busca a ApplicationOS pelo processName e data
            var app = await _appRepository.GetByProcessAndDateAsync(processName, currentDate);
            if (app != null)
            {
                // Busca o TimeEntry ativo (com CurrentSessionStart definido)
                var activeEntry = app.Sessions.LastOrDefault(te => te.CurrentSessionStart.HasValue);
                if (activeEntry != null)
                {
                    // Para o tracking: calcula e acumula a duração da sessão
                    activeEntry.StopTracking(now);
                }
                await _appRepository.SaveChangesAsync();

                await Clients.All.SendAsync("ApplicationTrackingStopped", processName, app.TotalTimeSpent.TotalSeconds);
            }
        }
    }
}
