using Nop.Plugin.Intelisale.AjaxFilters.Models.PriceRangeFilterSlider;
using System.Threading.Tasks;

namespace Nop.Plugin.Intelisale.AjaxFilters.Services
{
    public interface IPriceCalculationServiceNopAjaxFilters
    {
        Task<PriceRangeFilterDto> GetPriceRangeFilterDtoAsync(int categoryId, int manufacturerId, int vendorId);

        Task<decimal> CalculateBasePriceAsync(decimal price, PriceRangeFilterDto priceRangeModel, bool isFromPrice);
    }
}