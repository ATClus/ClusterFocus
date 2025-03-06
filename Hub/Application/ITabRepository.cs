using Hub.Domain;
using System.Threading.Tasks;

namespace Hub.Application
{
    /// <summary>
    /// Interface de repositório para gerenciar entidades Tab e seus TimeEntries associados,
    /// utilizando o modelo de "pausa e retomada" para acumular a duração.
    /// </summary>
    public interface ITabRepository
    {
        /// <summary>
        /// Adiciona uma nova entidade Tab, incluindo seu registro de tracking ativo (TimeEntry).
        /// </summary>
        /// <param name="tab">A entidade Tab a ser adicionada.</param>
        Task AddAsync(Tab tab);

        /// <summary>
        /// Obtém uma Tab com base no domínio (URL) e na data.
        /// Essa consulta é usada para verificar se já existe um registro para o dia corrente.
        /// </summary>
        /// <param name="domain">O domínio extraído da URL.</param>
        /// <param name="date">A data de referência.</param>
        /// <returns>A entidade Tab correspondente ou null se não encontrada.</returns>
        Task<Tab> GetByUrlAndDateAsync(string domain, DateOnly date);

        /// <summary>
        /// Persiste as alterações realizadas nas entidades Tab e nos seus TimeEntries associados.
        /// </summary>
        Task SaveChangesAsync();
    }
}
