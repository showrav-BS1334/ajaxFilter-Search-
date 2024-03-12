using Nop.Plugin.Intelisale.AjaxFilters.Areas.Admin.Models;
using Nop.Web.Models.Catalog;
using SevenSpikes.Nop.Framework.Models;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Intelisale.AjaxFilters.Models
{
    public record ProductsModel : Base7SpikesProductsModel
    {
        public IEnumerable<ProductOverviewModel> Products { get; set; }
        public IList<int> ProductIdsToDetermineFilters { get; set; }
        public string SpecificationFilterModel7SpikesJson { get; set; }
        public string AttributeFilterModel7SpikesJson { get; set; }
        public string ManufacturerFilterModel7SpikesJson { get; set; }
        public string VendorFilterModel7SpikesJson { get; set; }
        public string OnSaleFilterModel7SpikesJson { get; set; }
        public string InStockFilterModel7SpikesJson { get; set; }
        public string ViewMode { get; set; }
        public CatalogProductsCommand PagingFilteringContext { get; set; }
        public NopAjaxFiltersSettingsModel NopAjaxFiltersSettingsModel { get; set; }
        public string HashQuery { get; set; }
        public string PriceRangeFromJson { get; set; }
        public string PriceRangeToJson { get; set; }
        public string CurrentPageSizeJson { get; set; }
        public string CurrentViewModeJson { get; set; }
        public string CurrentOrderByJson { get; set; }
        public string CurrentPageNumberJson { get; set; }
        public int TotalCount { get; set; }

        public ProductsModel()
        {
            Products = new List<ProductOverviewModel>();
            PagingFilteringContext = new CatalogProductsCommand();
        }

        public override IList<ProductOverviewModel> GetProductOverviewModels(string consumerName)
        {
            return Products.ToList();
        }

        public override IList<ProductOverviewModel> GetRollOverModel()
        {
            return Products.ToList();
        }
    }
}