using Nop.Plugin.Intelisale.AjaxFilters.Infrastructure.Cache;
using Nop.Plugin.Intelisale.AjaxFilters.Models.ManufacturerFilter;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Web.Framework.Components;
using SevenSpikes.Nop.Framework.Components;
using SevenSpikes.Nop.Services.Catalog;
using SevenSpikes.Nop.Services.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Intelisale.AjaxFilters.Components
{
    [ViewComponent(Name = "NopAjaxFiltersManufacturerFilters")]
    public class ManufacturerFilterComponent : Base7SpikesComponent
    {
        private readonly IAclHelper _aclHelper;
        private readonly IManufacturerService7Spikes _manufacturerService7Spikes;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly CatalogSettings _catalogSettings;

        public ManufacturerFilterComponent(IAclHelper aclHelper,
            IManufacturerService7Spikes manufacturerService7Spikes,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IWorkContext workContext,
            CatalogSettings catalogSettings)
        {
            _aclHelper = aclHelper;
            _manufacturerService7Spikes = manufacturerService7Spikes;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _workContext = workContext;
            _catalogSettings = catalogSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync(int categoryId, int vendorId)
        {
            ManufacturerFilterModel7Spikes manufacturerFilterModel7Spikes = await GetManufacturerFilterInternalAsync(categoryId, vendorId);
            if (manufacturerFilterModel7Spikes.ManufacturerFilterItems.Count == 0)
            {
                return base.Content(string.Empty);
            }
            return ((NopViewComponent)this).View<ManufacturerFilterModel7Spikes>("ManufacturerFilter", manufacturerFilterModel7Spikes);
        }

        private async Task<ManufacturerFilterModel7Spikes> GetManufacturerFilterInternalAsync(int categoryId, int vendorId)
        {
            IStaticCacheManager staticCacheManager = _staticCacheManager;
            CacheKey nOP_AJAX_FILTERS_MANUFACTURER_FILTERS_MODEL_KEY = NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_MANUFACTURER_FILTERS_MODEL_KEY;
            object obj = categoryId;
            object obj2 = vendorId;
            object obj3 = (await _workContext.GetWorkingLanguageAsync()).Id;
            object obj4 = await _aclHelper.GetAllowedCustomerRolesIdsAsync();
            Store store = await _storeContext.GetCurrentStoreAsync();
            CacheKey key = staticCacheManager.PrepareKeyForDefaultCache(nOP_AJAX_FILTERS_MANUFACTURER_FILTERS_MODEL_KEY, obj, obj2, obj3, obj4, store.Id);
            ManufacturerFilterModel7Spikes manufacturerFilterModel7Spikes = await _staticCacheManager.GetAsync(key, async delegate
            {
                IList<Manufacturer> list = new List<Manufacturer>();
                if (categoryId > 0)
                {
                    list = await _manufacturerService7Spikes.GetManufacturersByCategoryIdAsync(categoryId, _catalogSettings.ShowProductsFromSubcategories);
                }
                else if (vendorId > 0)
                {
                    list = await _manufacturerService7Spikes.GetManufacturersByVendorIdAsync(vendorId);
                }
                ManufacturerFilterModel7Spikes manufacturerFilterModel7SpikesToReturn = new ManufacturerFilterModel7Spikes();
                if (list.Count > 0)
                {
                    manufacturerFilterModel7SpikesToReturn = new ManufacturerFilterModel7Spikes
                    {
                        CategoryId = categoryId,
                        VendorId = vendorId
                    };
                    foreach (Manufacturer item in list)
                    {
                        ManufacturerFilterItem manufacturerFilterItem = new ManufacturerFilterItem
                        {
                            Id = item.Id
                        };
                        ManufacturerFilterItem manufacturerFilterItem2 = manufacturerFilterItem;
                        manufacturerFilterItem2.Name = await base.LocalizationService.GetLocalizedAsync(item, (Manufacturer x) => x.Name);
                        manufacturerFilterModel7SpikesToReturn.ManufacturerFilterItems.Add(manufacturerFilterItem);
                    }
                }
                return manufacturerFilterModel7SpikesToReturn;
            });
            if (manufacturerFilterModel7Spikes.CategoryId == 0 && manufacturerFilterModel7Spikes.VendorId == 0)
            {
                return new ManufacturerFilterModel7Spikes();
            }
            return manufacturerFilterModel7Spikes;
        }
    }
}