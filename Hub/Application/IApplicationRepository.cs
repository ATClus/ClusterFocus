using Hub.Domain;

namespace Hub.Application
{
    public interface IApplicationRepository
    {
        Task<ApplicationOS> GetByProcessAndDateAsync(string processName, DateOnly date);
        Task<ApplicationOS> GetByProcessNameAsync(string processName);
        Task AddAsync(ApplicationOS application);
        Task SaveChangesAsync();
    }
}
