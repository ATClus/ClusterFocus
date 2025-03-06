namespace Hub.Domain
{
    public class Tab : IActivity
    {
        public int Id { get; set; }
        public string Title { get; private set; }
        public string Url { get; private set; }
        public DateOnly Date { get; private set; }

        public TimeSpan TotalTimeSpent => Sessions.Aggregate(TimeSpan.Zero, (total, entry) => total + entry.Duration);

        public List<TimeEntry> Sessions { get; private set; } = new List<TimeEntry>();

        public Tab(string title, string url)
        {
            Title = title;
            Url = GetDomain(url);
            Date = DateOnly.FromDateTime(DateTime.Now);
        }

        public void StartTracking(DateTime now)
        {
            var activeEntry = Sessions.LastOrDefault(e => e.CurrentSessionStart.HasValue);
            if (activeEntry == null)
            {
                var timeEntry = new TimeEntry(now) { Tab = this };
                Sessions.Add(timeEntry);
            }
            else
            {
                activeEntry.StartTracking(now);
            }
        }

        public void StopTracking(DateTime now)
        {
            var activeEntry = Sessions.LastOrDefault(e => e.CurrentSessionStart.HasValue);
            if (activeEntry != null)
            {
                activeEntry.StopTracking(now);
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
