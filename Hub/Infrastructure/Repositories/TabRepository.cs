using Hub.Application;
using Hub.Domain;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Hub.Infrastructure.Repositories
{
    public class TabRepository : ITabRepository
    {
        private readonly HubDbContext _context;

        public TabRepository(HubDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Tab tab)
        {
            // Adiciona a nova entidade Tab, que já contém o TimeEntry ativo (ou acumulado)
            await _context.Tabs.AddAsync(tab);
        }

        public async Task<Tab> GetByUrlAndDateAsync(string domain, DateOnly date)
        {
            // Busca a Tab correspondente ao domínio e data, incluindo os registros de TimeEntry (Sessions)
            return await _context.Tabs
                .Include(t => t.Sessions)
                .FirstOrDefaultAsync(t => t.Url == domain && t.Date == date);
        }

        public async Task SaveChangesAsync()
        {
            // Persiste as alterações no contexto, atualizando as informações acumuladas de tracking
            await _context.SaveChangesAsync();
        }
    }
}
