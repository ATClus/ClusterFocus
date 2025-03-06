using System;
using System.Collections.Generic;
using System.Linq;

namespace Hub.Domain
{
    public class ApplicationOS : IActivity
    {
        public int Id { get; set; }
        public string Title { get; private set; }
        public string ProcessName { get; private set; }
        public DateOnly Date { get; private set; }
        public List<TimeEntry> Sessions { get; private set; } = new List<TimeEntry>();

        public TimeSpan TotalTimeSpent => Sessions.Aggregate(TimeSpan.Zero, (total, session) => total + session.Duration);

        public ApplicationOS(string title, string processName)
        {
            Title = title;
            ProcessName = processName;
            Date = DateOnly.FromDateTime(DateTime.Now);
        }

        public void StartTracking(DateTime now)
        {
            var activeEntry = Sessions.LastOrDefault(e => e.CurrentSessionStart.HasValue);
            if (activeEntry == null)
            {
                var timeEntry = new TimeEntry(now) { Application = this };
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
    }
}
