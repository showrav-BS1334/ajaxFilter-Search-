using Nop.Plugin.Intelisale.AjaxFilters.Models.AttributeFilter;
using Nop.Plugin.Intelisale.AjaxFilters.Models.InStockFilter;
using Nop.Plugin.Intelisale.AjaxFilters.Models.ManufacturerFilter;
using Nop.Plugin.Intelisale.AjaxFilters.Models.OnSaleFilter;
using Nop.Plugin.Intelisale.AjaxFilters.Models.PriceRangeFilterSlider;
using Nop.Plugin.Intelisale.AjaxFilters.Models.SpecificationFilter;
using Nop.Plugin.Intelisale.AjaxFilters.Models.VendorFilter;

namespace Nop.Plugin.Intelisale.AjaxFilters.Models
{
    public class GetFilteredProductsModel
    {
        public int CategoryId { get; set; }
        public int ManufacturerId { get; set; }
        public int VendorId { get; set; }
        public PriceRangeFilterModel7Spikes PriceRangeFilterModel7Spikes { get; set; }
        public SpecificationFilterModel7Spikes SpecificationFiltersModel7Spikes { get; set; }
        public AttributeFilterModel7Spikes AttributeFiltersModel7Spikes { get; set; }
        public ManufacturerFilterModel7Spikes ManufacturerFiltersModel7Spikes { get; set; }
        public VendorFilterModel7Spikes VendorFiltersModel7Spikes { get; set; }
        public OnSaleFilterModel7Spikes OnSaleFilterModel { get; set; }
        public InStockFilterModel7Spikes InStockFilterModel { get; set; }
        public string QueryString { get; set; }
        public bool ShouldNotStartFromFirstPage { get; set; }
        public string Keyword { get; set; }
        public int SearchCategoryId { get; set; }
        public int SearchManufacturerId { get; set; }
        public int SearchVendorId { get; set; }
        public decimal? PriceFrom { get; set; }
        public decimal? PriceTo { get; set; }
        public bool IncludeSubcategories { get; set; }
        public bool SearchInProductDescriptions { get; set; }
        public bool AdvancedSearch { get; set; }
        public bool IsOnSearchPage { get; set; }
        public int? Orderby { get; set; }
        public string Viewmode { get; set; }
        public int? PageNumber { get; set; }
        public int Pagesize { get; set; }
    }
}