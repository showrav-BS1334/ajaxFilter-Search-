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
    public interface IQueryStringToModelUpdater
    {
        void UpdateModelsFromQueryString(string queryString, SpecificationFilterModel7Spikes specificationFiltersModel7Spikes, AttributeFilterModel7Spikes attributeFilterModel7Spikes, ManufacturerFilterModel7Spikes manufacturerFilterModel7Spikes, VendorFilterModel7Spikes vendorFilterModel7Spikes, PriceRangeFilterModel7Spikes priceRangeFilterModel7Spikes, CatalogProductsCommand pagingFilteringModel, OnSaleFilterModel7Spikes onSaleFilterModel, InStockFilterModel7Spikes inStockFilterModel7Spikes);

        void UpdateOnSaleModel(string queryStringParameter, OnSaleFilterModel7Spikes onSaleFilterModel);

        void UpdateSpecificationModel(string queryStringParameter, SpecificationFilterModel7Spikes specificationFilterModel7Spikes);

        void UpdatePagingFilterModelWithPageSize(string queryStringParameter, CatalogProductsCommand pagingFilteringModel);

        void UpdatePagingFilterModelWithViewMode(string queryStringParameter, CatalogProductsCommand pagingFilteringModel);

        void UpdatePagingFilterModelWithOrderBy(string queryStringParameter, CatalogProductsCommand pagingFilteringModel);

        void UpdatePagingFilterModelWithPageNumber(string queryStringParameter, CatalogProductsCommand pagingFilteringModel);

        void UpdatePriceRangeModel(string queryStringParameter, PriceRangeFilterModel7Spikes priceRangeFilterModel7Spikes);

        void UpdateManufacturerFilterModel(string queryStringParameter, ManufacturerFilterModel7Spikes manufacturerFilterModel7Spikes);

        void UpdateVendorFilterModel(string queryStringParameter, VendorFilterModel7Spikes vendorFilterModel7Spikes);

        void UpdateAttributesFilterModel(string queryStringParameter, AttributeFilterModel7Spikes attributeFilterModel7Spikes);

        void UpdateInStockModel(string queryStringParameter, InStockFilterModel7Spikes instockFilterModel);
    }
}