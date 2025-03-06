using Hub.Application;
using Hub.Domain;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hub.Presentation
{
    public class TabFocus : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly ITabRepository _tabRepository;

        public TabFocus(ITabRepository tabRepository)
        {
            _tabRepository = tabRepository;
        }

        public async Task StartTabTracking(string title, string url)
        {
            var domain = GetDomain(url);
            var currentDate = DateOnly.FromDateTime(DateTime.Now);
            var now = DateTime.Now;

            // Busca a Tab para o dia atual
            var tab = await _tabRepository.GetByUrlAndDateAsync(domain, currentDate);

            if (tab == null)
            {
                // Cria a Tab e o TimeEntry do dia
                tab = new Tab(title, url);
                var timeEntry = new TimeEntry(now) { Tab = tab };
                tab.Sessions.Add(timeEntry);
                await _tabRepository.AddAsync(tab);
            }
            else
            {
                // Em vez de procurar "activeEntry", procure o TimeEntry "do dia"
                var entryDoDia = tab.Sessions
                    .FirstOrDefault(e => e.Date == currentDate.ToDateTime(TimeOnly.MinValue).Date);

                if (entryDoDia == null)
                {
                    // Não existe registro do dia: cria um
                    var timeEntry = new TimeEntry(now) { Tab = tab };
                    tab.Sessions.Add(timeEntry);
                }
                else
                {
                    // Retoma o tracking do mesmo registro do dia
                    entryDoDia.StartTracking(now);
                }
            }

            await _tabRepository.SaveChangesAsync();
            await Clients.All.SendAsync("TabTrackingStarted", domain);
        }


        public async Task StopTabTracking(string url)
        {
            var domain = GetDomain(url);
            var currentDate = DateOnly.FromDateTime(DateTime.Now);
            var now = DateTime.Now;

            // Busca a Tab pelo domínio e data
            var tab = await _tabRepository.GetByUrlAndDateAsync(domain, currentDate);
            if (tab != null)
            {
                // Busca o registro ativo (TimeEntry com CurrentSessionStart definido)
                var activeEntry = tab.Sessions.LastOrDefault(te => te.CurrentSessionStart.HasValue);
                if (activeEntry != null)
                {
                    // Para o tracking: atualiza EndTime, acumula o tempo do período e "pausa" o tracking
                    activeEntry.StopTracking(now);
                }

                // Atualiza o banco imediatamente na troca de foco
                await _tabRepository.SaveChangesAsync();

                await Clients.All.SendAsync("TabTrackingStopped", domain, tab.TotalTimeSpent.TotalSeconds);
            }
        }

        private string GetDomain(string url)
        {
            try
            {
                var uri = new Uri(url);
                return uri.Host;
            }
            catch (UriFormatException)
            {
                return string.Empty;
            }
        }
    }
}
