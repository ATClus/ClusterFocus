using System;

namespace Hub.Domain
{
    public class TimeEntry
    {
        // Construtor sem parâmetros para o EF Core
        private TimeEntry() { }

        // Construtor público para iniciar o tracking
        public TimeEntry(DateTime now)
        {
            Date = now.Date;
            StartTime = now;
            EndTime = now;
            Duration = TimeSpan.Zero;
            // Inicia o tracking: registra o início da sessão ativa
            CurrentSessionStart = now;
        }

        public int Id { get; set; }
        public DateTime Date { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime? EndTime { get; private set; }
        // Agora possui setter para que o EF Core possa persistir o valor
        public TimeSpan Duration { get; private set; }

        public int? TabId { get; set; }
        public Tab Tab { get; set; }
        public int? ApplicationId { get; set; }
        public ApplicationOS Application { get; set; }

        // Propriedade que indica se o tracking está ativo (null indica pausado)
        public DateTime? CurrentSessionStart { get; private set; }

        /// <summary>
        /// Retoma ou inicia o tracking: se não houver uma sessão ativa, define o início da sessão atual.
        /// </summary>
        /// <param name="now">Horário atual.</param>
        public void StartTracking(DateTime now)
        {
            if (CurrentSessionStart == null)
            {
                CurrentSessionStart = now;
            }
        }

        /// <summary>
        /// Para o tracking atual, atualizando o EndTime e acumulando a duração da sessão.
        /// </summary>
        /// <param name="now">Horário atual.</param>
        public void StopTracking(DateTime now)
        {
            if (CurrentSessionStart.HasValue)
            {
                EndTime = now;
                // Calcula a duração do período atual e acumula
                var sessionDuration = now - CurrentSessionStart.Value;
                Duration += sessionDuration;
                // Indica que o tracking foi pausado
                CurrentSessionStart = null;
            }
        }
    }
}
