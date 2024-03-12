using Nop.Plugin.Intelisale.AjaxFilters.Infrastructure.Cache;
using Nop.Plugin.Intelisale.AjaxFilters.Models.VendorFilter;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Web.Framework.Components;
using SevenSpikes.Nop.Framework.Components;
using SevenSpikes.Nop.Services.Catalog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Intelisale.AjaxFilters.Components
{
    [ViewComponent(Name = "NopAjaxFiltersVendorFilters")]
    public class VendorFilterComponent : Base7SpikesComponent
    {
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IVendorService7Spikes _vendorServce;
        private readonly CatalogSettings _catalogSettings;

        public VendorFilterComponent(IStaticCacheManager staticCacheManager,
            IStoreContext storeContext, IWorkContext workContext,
            IVendorService7Spikes vendorServce,
            CatalogSettings catalogSettings)
        {
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _workContext = workContext;
            _vendorServce = vendorServce;
            _catalogSettings = catalogSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync(int categoryId, int manufacturerId)
        {
            VendorFilterModel7Spikes vendorFilterModel7Spikes = new VendorFilterModel7Spikes();
            if (categoryId > 0)
            {
                vendorFilterModel7Spikes = await GetVendorFilterInternalAsync(categoryId);
            }
            else if (manufacturerId > 0)
            {
                vendorFilterModel7Spikes = await GetVendorFilterForManufacturerInternalAsync(manufacturerId);
            }
            if (vendorFilterModel7Spikes.VendorFilterItems.Count == 0)
            {
                return base.Content(string.Empty);
            }
            return ((NopViewComponent)this).View<VendorFilterModel7Spikes>("VendorFilter", vendorFilterModel7Spikes);
        }

        private async Task<VendorFilterModel7Spikes> GetVendorFilterInternalAsync(int categoryId)
        {
            IStaticCacheManager staticCacheManager = _staticCacheManager;
            CacheKey nOP_AJAX_FILTERS_VENDOR_FILTERS_MODEL_KEY = NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_VENDOR_FILTERS_MODEL_KEY;
            object obj = categoryId;
            object obj2 = (await _workContext.GetWorkingLanguageAsync()).Id;
            Store store = await _storeContext.GetCurrentStoreAsync();
            CacheKey key = staticCacheManager.PrepareKeyForDefaultCache(nOP_AJAX_FILTERS_VENDOR_FILTERS_MODEL_KEY, obj, obj2, store.Id);
            VendorFilterModel7Spikes vendorFilterModel7Spikes = await _staticCacheManager.GetAsync(key, async delegate
            {
                IList<Vendor> list = await _vendorServce.GetVendorsByCategoryIdAsync(categoryId, _catalogSettings.ShowProductsFromSubcategories);
                VendorFilterModel7Spikes vendorFilterModel7SpikesToReturn = new VendorFilterModel7Spikes();
                if (list.Count > 0)
                {
                    vendorFilterModel7SpikesToReturn.CategoryId = categoryId;
                    foreach (Vendor item in list)
                    {
                        VendorFilterItem vendorFilterItem = new VendorFilterItem
                        {
                            Id = item.Id
                        };
                        VendorFilterItem vendorFilterItem2 = vendorFilterItem;
                        vendorFilterItem2.Name = await base.LocalizationService.GetLocalizedAsync(item, (Vendor v) => v.Name);
                        vendorFilterModel7SpikesToReturn.VendorFilterItems.Add(vendorFilterItem);
                    }
                }
                return vendorFilterModel7SpikesToReturn;
            });
            if (vendorFilterModel7Spikes.CategoryId == 0)
            {
                return new VendorFilterModel7Spikes();
            }
            return vendorFilterModel7Spikes;
        }

        private async Task<VendorFilterModel7Spikes> GetVendorFilterForManufacturerInternalAsync(int manufacturerId)
        {
            IList<Vendor> list = await _vendorServce.GetVendorsByManufacturerIdAsync(manufacturerId);
            VendorFilterModel7Spikes vendorFilterModel7SpikesToReturn = new VendorFilterModel7Spikes();
            if (list.Count > 0)
            {
                vendorFilterModel7SpikesToReturn = new VendorFilterModel7Spikes
                {
                    CategoryId = 0
                };
                foreach (Vendor item in list)
                {
                    VendorFilterItem vendorFilterItem = new VendorFilterItem
                    {
                        Id = item.Id
                    };
                    VendorFilterItem vendorFilterItem2 = vendorFilterItem;
                    vendorFilterItem2.Name = await base.LocalizationService.GetLocalizedAsync(item, (Vendor v) => v.Name);
                    vendorFilterModel7SpikesToReturn.VendorFilterItems.Add(vendorFilterItem);
                }
            }
            return vendorFilterModel7SpikesToReturn;
        }
    }
}