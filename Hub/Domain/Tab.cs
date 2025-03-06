using System;
using System.Collections.Generic;
using System.Linq;

namespace Hub.Domain
{
    public class Tab : IActivity
    {
        public int Id { get; set; }
        public string Title { get; private set; }
        public string Url { get; private set; }
        public DateOnly Date { get; private set; }

        // A duração total é a soma das durations de todos os registros de TimeEntry associados.
        public TimeSpan TotalTimeSpent => Sessions.Aggregate(TimeSpan.Zero, (total, entry) => total + entry.Duration);

        // Registros de tracking para a Tab.
        public List<TimeEntry> Sessions { get; private set; } = new List<TimeEntry>();

        public Tab(string title, string url)
        {
            Title = title;
            Url = GetDomain(url);
            Date = DateOnly.FromDateTime(DateTime.Now);
        }

        /// <summary>
        /// Inicia ou retoma o tracking para esta Tab.
        /// Se não houver um registro ativo, cria um novo.
        /// </summary>
        /// <param name="now">Data/hora atual.</param>
        public void StartTracking(DateTime now)
        {
            // Procura um registro ativo (a sessão ativa tem CurrentSessionStart definido)
            var activeEntry = Sessions.LastOrDefault(e => e.CurrentSessionStart.HasValue);
            if (activeEntry == null)
            {
                // Cria um novo TimeEntry com o tracking iniciado
                var timeEntry = new TimeEntry(now) { Tab = this };
                Sessions.Add(timeEntry);
            }
            else
            {
                // Se já houver um registro, retoma o tracking (o método pode ser implementado para ignorar se já estiver ativo)
                activeEntry.StartTracking(now);
            }
        }

        /// <summary>
        /// Para o tracking ativo, atualizando a duração acumulada.
        /// </summary>
        /// <param name="now">Data/hora atual.</param>
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
