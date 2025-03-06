using System.Threading.Tasks;
using Hub.Domain;

namespace Hub.Application
{
    public interface IApplicationRepository
    {
        Task<ApplicationOS> GetByProcessAndDateAsync(string processName, DateOnly date);
        Task AddAsync(ApplicationOS application);
        Task SaveChangesAsync();
    }
}
