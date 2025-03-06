using System;
using System.Collections.Generic;

namespace WinTracker
{
    public class TimeRecord
    {
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public class TimeTracker : IDisposable
    {
        private ActiveWindowTracker _activeWindowTracker;
        private TimeRecord _currentRecord;
        private List<TimeRecord> _records;

        public IReadOnlyList<TimeRecord> Records => _records.AsReadOnly();

        public TimeTracker()
        {
            _records = new List<TimeRecord>();
            _activeWindowTracker = new ActiveWindowTracker();
            _activeWindowTracker.ActiveWindowChanged += OnActiveWindowChanged;
        }

        private void OnActiveWindowChanged(object sender, ActiveWindowEventArgs e)
        {
            DateTime now = DateTime.Now;

            // Finaliza o registro atual, se houver
            if (_currentRecord != null)
            {
                _currentRecord.Duration = now - _currentRecord.StartTime;
                _records.Add(_currentRecord);
            }

            // Inicia o registro para a nova janela, se válida
            if (e.ProcessId != 0)
            {
                _currentRecord = new TimeRecord
                {
                    ProcessId = e.ProcessId,
                    ProcessName = e.ProcessName,
                    StartTime = now
                };
            }
            else
            {
                _currentRecord = null;
            }
        }

        public void Dispose()
        {
            if (_activeWindowTracker != null)
            {
                _activeWindowTracker.ActiveWindowChanged -= OnActiveWindowChanged;
                _activeWindowTracker.Dispose();
                _activeWindowTracker = null;
            }
        }
    }
}
