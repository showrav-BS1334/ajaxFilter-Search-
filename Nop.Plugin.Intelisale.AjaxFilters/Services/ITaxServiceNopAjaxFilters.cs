using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using System.Threading.Tasks;

namespace Nop.Plugin.Intelisale.AjaxFilters.Services
{
    public interface ITaxServiceNopAjaxFilters
    {
        Task<decimal> GetTaxRateForProductAsync(Product product, int taxCategoryId, Customer customer);
    }
}