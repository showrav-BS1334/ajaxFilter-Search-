using Nop.Plugin.Intelisale.AjaxFilters.Domain;
using Nop.Plugin.Intelisale.AjaxFilters.Helpers;
using Nop.Plugin.Intelisale.AjaxFilters.Infrastructure.Cache;
using Nop.Plugin.Intelisale.AjaxFilters.Models.SpecificationFilter;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Web.Framework.Components;
using SevenSpikes.Nop.Framework.Components;
using SevenSpikes.Nop.Services.Catalog;
using SevenSpikes.Nop.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Plugin.Intelisale.AjaxFilters.Components
{
    [ViewComponent(Name = "NopAjaxFiltersSpecificationFilters")]
    public class SpecificationFilterComponent : Base7SpikesComponent
    {
        private readonly IAclHelper _aclHelper;
        private readonly ISearchQueryStringHelper _searchQueryStringHelper;
        private readonly ISpecificationAttributeService7Spikes _specificationAttributeService7Spikes;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly CatalogSettings _catalogSettings;
        private readonly NopAjaxFiltersSettings _nopAjaxFilterSettings;
        private readonly ISpecificationAttributeService _specificationAttributeService;

        public SpecificationFilterComponent(IAclHelper aclHelper,
            ISearchQueryStringHelper searchQueryStringHelper,
            ISpecificationAttributeService7Spikes specificationAttributeService7Spikes,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IWorkContext workContext,
            CatalogSettings catalogSettings,
            NopAjaxFiltersSettings nopAjaxFilterSettings,
            ISpecificationAttributeService specificationAttributeService)
        {
            _aclHelper = aclHelper;
            _searchQueryStringHelper = searchQueryStringHelper;
            _specificationAttributeService7Spikes = specificationAttributeService7Spikes;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _workContext = workContext;
            _catalogSettings = catalogSettings;
            _nopAjaxFilterSettings = nopAjaxFilterSettings;
            _specificationAttributeService = specificationAttributeService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int categoryId, int manufacturerId, int vendorId)
        {
            SpecificationFilterModel7Spikes specificationFilterModel7Spikes = await GetSpecificationFilterInternalAsync(categoryId, manufacturerId, vendorId);
            if (specificationFilterModel7Spikes.SpecificationFilterGroups.Count == 0)
            {
                return base.Content(string.Empty);
            }
            return ((NopViewComponent)this).View<SpecificationFilterModel7Spikes>("SpecificationFilter", specificationFilterModel7Spikes);
        }

        private async Task<SpecificationFilterModel7Spikes> GetSpecificationFilterInternalAsync(int categoryId, int manufacturerId, int vendorId)
        {
            SearchQueryStringParameters searchQueryStringParameters = _searchQueryStringHelper.GetQueryStringParameters(base.Request.QueryString.Value);
            SpecificationFilterModel7Spikes specificationFilterModel7Spikes = new SpecificationFilterModel7Spikes();
            if (searchQueryStringParameters.IsOnSearchPage)
            {
                ICustomerService customerService = base.CustomerService;
                string customerRolesIds = string.Join(",", await customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync()));
                IStaticCacheManager staticCacheManager = _staticCacheManager;
                CacheKey nOP_AJAX_FILTERS_SPECIFICATION_OPTION_IDS_KEY = NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_SPECIFICATION_OPTION_IDS_KEY;
                object obj = searchQueryStringParameters.SearchCategoryId;
                object obj2 = searchQueryStringParameters.SearchManufacturerId;
                object obj3 = searchQueryStringParameters.SearchVendorId;
                object obj4 = (await _workContext.GetWorkingLanguageAsync()).Id;
                object obj5 = customerRolesIds;
                Store store = await _storeContext.GetCurrentStoreAsync();
                CacheKey key = staticCacheManager.PrepareKey(nOP_AJAX_FILTERS_SPECIFICATION_OPTION_IDS_KEY, obj, obj2, obj3, obj4, obj5, store.Id, searchQueryStringParameters.Keyword, searchQueryStringParameters.PriceFrom, searchQueryStringParameters.PriceTo, searchQueryStringParameters.IncludeSubcategories, searchQueryStringParameters.SearchInProductDescriptions, searchQueryStringParameters.AdvancedSearch);
                IList<int> list = await _staticCacheManager.GetAsync(key, (Func<Task<IList<int>>>)(async () => null));
                IList<SpecificationAttributeOption> list2 = new List<SpecificationAttributeOption>();
                if (list != null && list.Any())
                {
                    list2 = await _specificationAttributeService7Spikes.GetSpecificationAttributeOptionsByIdsAsync(list);
                }
                if (list2.Count > 0)
                {
                    specificationFilterModel7Spikes = await GetSpecificationFilterModel7SpikesAsync(categoryId, manufacturerId, vendorId, list2);
                }
            }
            else
            {
                IStaticCacheManager staticCacheManager = _staticCacheManager;
                CacheKey nOP_AJAX_FILTERS_SPECIFICATION_OPTION_IDS_KEY = NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_SPECIFICATION_FILTERS_MODEL_KEY;
                object obj5 = categoryId;
                object obj4 = manufacturerId;
                object obj3 = vendorId;
                object obj2 = (await _workContext.GetWorkingLanguageAsync()).Id;
                object obj = await _aclHelper.GetAllowedCustomerRolesIdsAsync();
                Store store = await _storeContext.GetCurrentStoreAsync();
                CacheKey key2 = staticCacheManager.PrepareKeyForDefaultCache(nOP_AJAX_FILTERS_SPECIFICATION_OPTION_IDS_KEY, obj5, obj4, obj3, obj2, obj, store.Id);
                specificationFilterModel7Spikes = await _staticCacheManager.GetAsync(key2, async delegate
                {
                    IList<SpecificationAttributeOption> list3 = new List<SpecificationAttributeOption>();
                    if (categoryId > 0)
                    {
                        list3 = await _specificationAttributeService7Spikes.GetSpecificationAttributeOptionsByCategoryIdAsync(categoryId, _catalogSettings.ShowProductsFromSubcategories);
                    }
                    else if (manufacturerId > 0)
                    {
                        list3 = await _specificationAttributeService7Spikes.GetSpecificationAttributeOptionsByManufacturerIdAsync(manufacturerId);
                    }
                    else if (vendorId > 0)
                    {
                        list3 = await _specificationAttributeService7Spikes.GetSpecificationAttributeOptionsByVendorIdAsync(vendorId);
                    }
                    SpecificationFilterModel7Spikes result = new SpecificationFilterModel7Spikes();
                    if (list3.Count > 0)
                    {
                        result = await GetSpecificationFilterModel7SpikesAsync(categoryId, manufacturerId, vendorId, list3);
                    }
                    return result;
                });
            }
            return specificationFilterModel7Spikes;
        }

        private async Task<SpecificationFilterModel7Spikes> GetSpecificationFilterModel7SpikesAsync(int categoryId, int manufacturerId, int vendorId, IEnumerable<SpecificationAttributeOption> specificationAttributeOptions)
        {
            SpecificationFilterModel7Spikes specificationFilterModel7Spikes = new SpecificationFilterModel7Spikes
            {
                CategoryId = categoryId,
                ManufacturerId = manufacturerId,
                VendorId = vendorId
            };
            Dictionary<int, SpecificationFilterGroup> specificationFilterGroupsDictionary = new Dictionary<int, SpecificationFilterGroup>();
            foreach (SpecificationAttributeOption specificationAttributeOption in specificationAttributeOptions)
            {
                specificationFilterGroupsDictionary.TryGetValue(specificationAttributeOption.SpecificationAttributeId, out var value);
                if (value == null)
                {
                    SpecificationAttribute specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(specificationAttributeOption.SpecificationAttributeId);
                    SpecificationFilterGroup specificationFilterGroup = new SpecificationFilterGroup
                    {
                        Id = specificationAttributeOption.SpecificationAttributeId,
                        DisplayOrder = specificationAttribute.DisplayOrder
                    };
                    SpecificationFilterGroup specificationFilterGroup2 = specificationFilterGroup;
                    specificationFilterGroup2.Name = await base.LocalizationService.GetLocalizedAsync(specificationAttribute, (SpecificationAttribute x) => x.Name);
                    value = specificationFilterGroup;
                    specificationFilterModel7Spikes.SpecificationFilterGroups.Add(value);
                    specificationFilterGroupsDictionary.Add(value.Id, value);
                }
                string colorSquaresRgb = ((!string.IsNullOrEmpty(specificationAttributeOption.ColorSquaresRgb)) ? specificationAttributeOption.ColorSquaresRgb.ToLower().Trim() : null);
                IList<SpecificationFilterItem> filterItems = value.FilterItems;
                SpecificationFilterItem specificationFilterItem = new SpecificationFilterItem
                {
                    Id = specificationAttributeOption.Id
                };
                SpecificationFilterItem specificationFilterItem2 = specificationFilterItem;
                specificationFilterItem2.Name = await base.LocalizationService.GetLocalizedAsync(specificationAttributeOption, (SpecificationAttributeOption x) => x.Name);
                specificationFilterItem.DisplayOrder = specificationAttributeOption.DisplayOrder;
                specificationFilterItem.ColorSquaresRgb = colorSquaresRgb;
                filterItems.Add(specificationFilterItem);
            }
            foreach (SpecificationFilterGroup specificationFilterGroup3 in specificationFilterModel7Spikes.SpecificationFilterGroups)
            {
                specificationFilterGroup3.FilterItems = (from x in specificationFilterGroup3.FilterItems
                                                         orderby x.DisplayOrder, x.Name
                                                         select x).ToList();
            }
            specificationFilterModel7Spikes.SpecificationFilterGroups = specificationFilterModel7Spikes.SpecificationFilterGroups.OrderBy((SpecificationFilterGroup x) => x.DisplayOrder).Take(_nopAjaxFilterSettings.NumberOfSpecificationFilters).ToList();
            return specificationFilterModel7Spikes;
        }
    }
}