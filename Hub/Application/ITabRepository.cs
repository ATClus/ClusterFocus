using Hub.Domain;

namespace Hub.Application
{
    public interface ITabRepository
    {
        Task AddAsync(Tab tab);
        Task<Tab> GetByUrlAndDateAsync(string domain, DateOnly date);
        Task<Tab> GetByUrlAsync(string domain);
        Task SaveChangesAsync();
    }
}
