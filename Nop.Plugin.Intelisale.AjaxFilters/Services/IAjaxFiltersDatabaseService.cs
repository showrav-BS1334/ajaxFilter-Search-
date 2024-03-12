using System.Threading.Tasks;

namespace Nop.Plugin.Intelisale.AjaxFilters.Services
{
    public interface IAjaxFiltersDatabaseService
    {
        Task CreateDatabaseScriptsAsync();

        Task UpdateDatabaseScriptsAsync();

        Task RemoveDatabaseScriptsAsync();
    }
}