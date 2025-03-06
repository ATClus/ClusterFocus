using Hub.Application;
using Hub.Domain;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Hub.Infrastructure.Repositories
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly HubDbContext _context;

        public ApplicationRepository(HubDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationOS> GetByProcessAndDateAsync(string processName, DateOnly date)
        {
            return await _context.Applications
                .Include(a => a.Sessions)
                .FirstOrDefaultAsync(a => a.ProcessName == processName && a.Date == date);
        }

        public async Task AddAsync(ApplicationOS application)
        {
            await _context.Applications.AddAsync(application);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
