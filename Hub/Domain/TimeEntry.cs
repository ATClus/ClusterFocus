namespace Hub.Domain
{
    public class TimeEntry
    {
        private TimeEntry() { }

        public TimeEntry(DateTime now)
        {
            Date = now.Date;
            StartTime = now;
            EndTime = now;
            Duration = TimeSpan.Zero;
            CurrentSessionStart = now;
        }

        public int Id { get; set; }
        public DateTime Date { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime? EndTime { get; private set; }
        public TimeSpan Duration { get; private set; }
        public int? TabId { get; set; }
        public Tab Tab { get; set; }
        public int? ApplicationId { get; set; }
        public ApplicationOS Application { get; set; }
        public DateTime? CurrentSessionStart { get; private set; }

        public void StartTracking(DateTime now)
        {
            if (CurrentSessionStart == null)
            {
                CurrentSessionStart = now;
            }
        }

        public void StopTracking(DateTime now)
        {
            if (CurrentSessionStart.HasValue)
            {
                EndTime = now;
                var sessionDuration = now - CurrentSessionStart.Value;
                Duration += sessionDuration;
                CurrentSessionStart = null;
            }
        }
    }
}
