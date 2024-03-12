using Nop.Plugin.Intelisale.AjaxFilters.Models.AttributeFilter;
using Nop.Plugin.Intelisale.AjaxFilters.Models.InStockFilter;
using Nop.Plugin.Intelisale.AjaxFilters.Models.ManufacturerFilter;
using Nop.Plugin.Intelisale.AjaxFilters.Models.OnSaleFilter;
using Nop.Plugin.Intelisale.AjaxFilters.Models.PriceRangeFilterSlider;
using Nop.Plugin.Intelisale.AjaxFilters.Models.SpecificationFilter;
using Nop.Plugin.Intelisale.AjaxFilters.Models.VendorFilter;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Intelisale.AjaxFilters.QueryStringManipulation
{
    public interface IQueryStringBuilder
    {
        void SetDataForQueryString(SpecificationFilterModel7Spikes specificationFilterModel7Spikes, AttributeFilterModel7Spikes attributeFilterModel7Spikes, ManufacturerFilterModel7Spikes manufacturerFilterModel7Spikes, VendorFilterModel7Spikes vendorFilterModel7Spikes, PriceRangeFilterModel7Spikes priceRangeFilterModel7Spikes, CatalogProductsCommand catalogPagingFilteringModel, OnSaleFilterModel7Spikes onSaleFilterModel, InStockFilterModel7Spikes inStockFilterModel);

        string GetQueryString(bool shouldRebuildQueryString);
    }
}