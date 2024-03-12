using Nop.Plugin.Intelisale.AjaxFilters.Domain.Enums;
using Nop.Plugin.Intelisale.AjaxFilters.Infrastructure.Cache;
using Nop.Plugin.Intelisale.AjaxFilters.Models.InStockFilter;
using Nop.Plugin.Intelisale.AjaxFilters.Services;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Stores;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;
using SevenSpikes.Nop.Framework.Components;
using SevenSpikes.Nop.Services.Helpers;
using System.Threading.Tasks;

namespace Nop.Plugin.Intelisale.AjaxFilters.Components
{
    [ViewComponent(Name = "NopAjaxFiltersInStockFilter")]
    public class InStockFilterComponent : Base7SpikesComponent
    {
        private readonly IAclHelper _aclHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IProductServiceNopAjaxFilters _productService7Spikes;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        public InStockFilterComponent(IAclHelper aclHelper, ILocalizationService localizationService, IProductServiceNopAjaxFilters productService7Spikes, IStaticCacheManager staticCacheManager, IStoreContext storeContext, IWorkContext workContext)
        {
            _aclHelper = aclHelper;
            _localizationService = localizationService;
            _productService7Spikes = productService7Spikes;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync(int categoryId, int manufacturerId, int vendorId)
        {
            InStockFilterModel7Spikes inStockFilterModel7Spikes = await GetInStockFilterInternalAsync(categoryId, manufacturerId, vendorId);
            if (inStockFilterModel7Spikes == null)
            {
                return base.Content(string.Empty);
            }
            return ((NopViewComponent)this).View<InStockFilterModel7Spikes>("InStockFilter", inStockFilterModel7Spikes);
        }

        private async Task<InStockFilterModel7Spikes> GetInStockFilterInternalAsync(int categoryId, int manufacturerId, int vendorId)
        {
            IStaticCacheManager staticCacheManager = _staticCacheManager;
            CacheKey nOP_AJAX_FILTERS_INSTOCK_FILTERS_MODEL_KEY = NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_INSTOCK_FILTERS_MODEL_KEY;
            object obj = (await _workContext.GetWorkingLanguageAsync()).Id;
            object obj2 = await _aclHelper.GetAllowedCustomerRolesIdsAsync();
            Store store = await _storeContext.GetCurrentStoreAsync();
            CacheKey key = staticCacheManager.PrepareKeyForDefaultCache(nOP_AJAX_FILTERS_INSTOCK_FILTERS_MODEL_KEY, obj, obj2, store.Id, categoryId, manufacturerId, vendorId);
            if (!(await _staticCacheManager.GetAsync(key, async () => await _productService7Spikes.HasProductsInStockAsync(categoryId, manufacturerId, vendorId))))
            {
                return null;
            }
            InStockFilterModel7Spikes inStockFilterModel7Spikes = new InStockFilterModel7Spikes
            {
                Id = 1
            };
            InStockFilterModel7Spikes inStockFilterModel7Spikes2 = inStockFilterModel7Spikes;
            inStockFilterModel7Spikes2.Name = await _localizationService.GetResourceAsync("SevenSpikes.NopAjaxFilters.Public.InStock.Option");
            inStockFilterModel7Spikes.CategoryId = categoryId;
            inStockFilterModel7Spikes.VendorId = vendorId;
            inStockFilterModel7Spikes.ManufacturerId = manufacturerId;
            inStockFilterModel7Spikes.FilterItemState = FilterItemState.Unchecked;
            return inStockFilterModel7Spikes;
        }
    }
}