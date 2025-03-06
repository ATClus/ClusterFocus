using Hub.Application;
using Hub.Domain;
using Microsoft.AspNetCore.SignalR;

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

            var tab = await _tabRepository.GetByUrlAsync(domain);

            if (tab == null)
            {
                tab = new Tab(title, url);
                var timeEntry = new TimeEntry(now) { Tab = tab };
                tab.Sessions.Add(timeEntry);
                await _tabRepository.AddAsync(tab);
            }
            else
            {
                var entryDoDia = tab.Sessions
                    .FirstOrDefault(e => e.Date == currentDate.ToDateTime(TimeOnly.MinValue).Date);

                if (entryDoDia == null)
                {
                    var timeEntry = new TimeEntry(now) { Tab = tab };
                    tab.Sessions.Add(timeEntry);
                }
                else
                {
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

            var tab = await _tabRepository.GetByUrlAsync(domain);
            if (tab != null)
            {
                var activeEntry = tab.Sessions.LastOrDefault(te => te.CurrentSessionStart.HasValue);
                if (activeEntry != null)
                {
                    activeEntry.StopTracking(now);
                }

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
