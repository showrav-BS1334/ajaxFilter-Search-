using Nop.Plugin.Intelisale.AjaxFilters.Models.AttributeFilter;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Intelisale.AjaxFilters.Services
{
    public interface IProductAttributeServiceAjaxFilters
    {
        Task<IList<AttributeFilterItem>> GetSortedAttributeValuesBasedOnTheirPredefinedDisplayOrderAsync(IEnumerable<AttributeFilterItem> attributeValues);
    }
}