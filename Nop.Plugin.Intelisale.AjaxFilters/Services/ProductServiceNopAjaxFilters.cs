using Nop.Plugin.Intelisale.AjaxFilters.Domain;
using Nop.Plugin.Intelisale.AjaxFilters.Helpers;

//using LinqToDB;
using LinqToDB.Data;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Localization;
using SevenSpikes.Nop.Services.Catalog;
using SevenSpikes.Nop.Services.Catalog.DTO;
using SevenSpikes.Nop.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Infrastructure;
using Nop.IntelisalePlugin.Services;
using Nop.Plugin.IntelisalePlugin;
using Nop.Plugin.IntelisalePlugin.Settings;
using Nop.Plugin.IntelisalePlugin.Factories;
using Nop.Services.Catalog;
using Nop.Plugin.Intelisale.Core.Services.Interfaces;
using Nop.Plugin.Intelisale.Core.Domain.Models;
using Nop.Plugin.IntelisalePlugin.Services.Catalog;

namespace Nop.Plugin.Intelisale.AjaxFilters.Services
{
    public class ProductServiceNopAjaxFilters : IProductServiceNopAjaxFilters
    {
        private readonly ILanguageService _languageService;
        private readonly INopDataProvider _dataProvider;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICategoryService7Spikes _categoryService7Spikes;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
        private readonly IRepository<Manufacturer> _manufacturerRepository;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IRepository<ProductWarehouseInventory> _productWarehouseInventory;
        private readonly CommonSettings _commonSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly NopAjaxFiltersSettings _nopAjaxFiltersSettings;
        private IStaticCacheManager _cacheManager;
        private IAclHelper _aclHelper;
        private readonly ICustomerService _customerService;
        //Intelisale custom code
        private readonly IntelisalePluginProviderSettings _intelisalePluginProviderSettings;
        private readonly ICompanyService _companyService;
        //End Intelisale custom code

        private CacheKey GROUPPRODUCTIDS_BY_CATEGORYIDS => new CacheKey("nop.groupproductids.categoryids.{0}");
        private CacheKey GROUPPRODUCTIDS_BY_MANUFACTURERID => new CacheKey("nop.groupproductids.manufacturerid.{0}");
        private CacheKey GROUPPRODUCTIDS_BY_VENDORID => new CacheKey("nop.groupproductids.vendorid.{0}");
        private CacheKey ONSALESTATE_BY_PRODUCTIDS_KEY => new CacheKey("Nop.onsalestate.productids-{0}-{1}-{2}");
        private CacheKey INSTOCKSTATE_BY_PRODUCTIDS_KEY => new CacheKey("nop.instock.productids-{0}-{1}-{2}");

        public ProductServiceNopAjaxFilters(
            ILanguageService languageService,
            INopDataProvider dataProvider,
            ICategoryService7Spikes categoryService7Spikes,
            IRepository<Product> productRepository,
            IRepository<ProductManufacturer> productManufacturer,
            IRepository<Manufacturer> manufacturerRepository,
            IRepository<Vendor> vendorRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<ProductWarehouseInventory> productWarehouseInventory,
            IWorkContext workContext,
            IStoreContext storeContext,
            CommonSettings commonSettings,
            CatalogSettings catalogSettings,
            NopAjaxFiltersSettings nopAjaxFiltersSettings,
            IStaticCacheManager cacheManager,
            IAclHelper aclHelper,
            ICustomerService customerService,
            //Intelisale custom code
            IntelisalePluginProviderSettings intelisalePluginProviderSettings,
            ICompanyService companyService
            //End Intelisale custom code
            )
        {
            _languageService = languageService;
            _dataProvider = dataProvider;
            _categoryService7Spikes = categoryService7Spikes;
            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
            _productManufacturerRepository = productManufacturer;
            _manufacturerRepository = manufacturerRepository;
            _vendorRepository = vendorRepository;
            _productWarehouseInventory = productWarehouseInventory;
            _workContext = workContext;
            _storeContext = storeContext;
            _commonSettings = commonSettings;
            _cacheManager = cacheManager;
            _aclHelper = aclHelper;
            _nopAjaxFiltersSettings = nopAjaxFiltersSettings;
            _catalogSettings = catalogSettings;
            _customerService = customerService;
            //Intelisale custom code
            _intelisalePluginProviderSettings = intelisalePluginProviderSettings;
            _companyService = companyService;
            //End Intelisale custom code
        }

        public virtual async Task<ProductsResultDataDTO> SearchProductsAsync(IList<int> categoryIds, SpecificationFilterModelDTO specifiationFilterModelDTO, AttributeFilterModelDTO attributeFilterModelDTO, ManufacturerFilterModelDTO manufacturerFilterModelDTO, VendorFilterModelDTO vendorFilterModelDTO, int pageIndex = 0, int pageSize = int.MaxValue, int manufacturerId = 0, int vendorId = 0, int storeId = 0, bool? featuredProducts = null, decimal? priceMin = null, decimal? priceMax = null, int productTagId = 0, string keywords = null, bool searchDescriptions = false, bool searchSku = false, bool searchProductTags = false, int languageId = 0, ProductSortingEnum orderBy = ProductSortingEnum.Position, bool showHidden = false, bool onSale = false, bool inStock = false)
        {
            return await SearchProductsInternalAsync(categoryIds, specifiationFilterModelDTO, attributeFilterModelDTO, manufacturerFilterModelDTO, vendorFilterModelDTO, pageIndex, pageSize, manufacturerId, vendorId, storeId, featuredProducts, priceMin, priceMax, productTagId, keywords, searchDescriptions, searchSku, searchProductTags, languageId, orderBy, showHidden, onSale, inStock);
        }

        public async Task<bool> HasProductsOnSaleAsync(int categoryId, int manufacturerId, int vendorId)
        {
            return await HasProductsOnSaleInternalAsync(categoryId, manufacturerId, vendorId);
        }

        public async Task<bool> HasProductsInStockAsync(int categoryId, int manufacturerId, int vendorId)
        {
            return await HasProductsInStockInternalAsync(categoryId, manufacturerId, vendorId);
        }

        private async Task<ProductsResultDataDTO> SearchProductsInternalAsync(IList<int> categoryIds, SpecificationFilterModelDTO specifiationFilterModelDTO, AttributeFilterModelDTO attributeFilterModelDTO, ManufacturerFilterModelDTO manufacturerFilterModelDTO, VendorFilterModelDTO vendorFilterModelDTO, int pageIndex = 0, int pageSize = int.MaxValue, int manufacturerId = 0, int vendorId = 0, int storeId = 0, bool? featuredProducts = null, decimal? priceMin = null, decimal? priceMax = null, int productTagId = 0, string keywords = null, bool searchDescriptions = false, bool searchSku = false, bool searchProductTags = false, int languageId = 0, ProductSortingEnum orderBy = ProductSortingEnum.Position, bool showHidden = false, bool onSale = false, bool inStock = false)
        {
            bool searchLocalizedValue = false;
            if (languageId > 0)
            {
                if (showHidden)
                {
                    searchLocalizedValue = true;
                }
                else
                {
                    ILanguageService languageService = _languageService;
                    int count = (await languageService.GetAllLanguagesAsync(showHidden: false, (await _storeContext.GetCurrentStoreAsync()).Id)).Count;
                    searchLocalizedValue = count >= 2;
                }
            }
            if (categoryIds != null && categoryIds.Contains(0))
            {
                categoryIds.Remove(0);
            }
            ICustomerService customerService = _customerService;
            int[] values = await customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync());
            string text = (_catalogSettings.IgnoreAcl ? string.Empty : string.Join(",", values));
            string parameterValue = ((categoryIds == null) ? string.Empty : string.Join(",", categoryIds));
            string text2 = "";
            if (specifiationFilterModelDTO != null && specifiationFilterModelDTO.SpecificationFilterDTOs.Any())
            {
                List<int> list = specifiationFilterModelDTO.SpecificationFilterDTOs.SelectMany((SpecificationFilterDTO x) => x.SelectedFilterIds).ToList();
                list.Sort();
                for (int i = 0; i < list.Count; i++)
                {
                    text2 += list[i];
                    if (i != list.Count - 1)
                    {
                        text2 += ",";
                    }
                }
            }
            string text3 = "";
            if (attributeFilterModelDTO != null && attributeFilterModelDTO.AttributeFilterDTOs.Any())
            {
                List<int> list2 = attributeFilterModelDTO.AttributeFilterDTOs.SelectMany((AttributeFilterDTO x) => x.SelectedProductVariantIds).ToList();
                list2.Sort();
                for (int j = 0; j < list2.Count; j++)
                {
                    text3 += list2[j];
                    if (j != list2.Count - 1)
                    {
                        text3 += ",";
                    }
                }
            }
            string text4 = "";
            if (manufacturerFilterModelDTO != null && manufacturerFilterModelDTO.SelectedFilterIds.Any())
            {
                List<int> list3 = manufacturerFilterModelDTO.SelectedFilterIds.ToList();
                list3.Sort();
                for (int k = 0; k < list3.Count; k++)
                {
                    text4 += list3[k];
                    if (k != list3.Count - 1)
                    {
                        text4 += ",";
                    }
                }
            }
            string text5 = "";
            if (vendorFilterModelDTO != null && vendorFilterModelDTO.SelectedFilterIds.Any())
            {
                List<int> list4 = vendorFilterModelDTO.SelectedFilterIds.ToList();
                list4.Sort();
                for (int l = 0; l < list4.Count; l++)
                {
                    text5 += list4[l];
                    if (l != list4.Count - 1)
                    {
                        text5 += ",";
                    }
                }
            }
            if (pageSize == int.MaxValue)
            {
                pageSize = 2147483646;
            }

            var customOrderBy = orderBy;
            var customPageIndex = pageIndex;
            var customPageSize = pageSize;
            var customPriceMax = priceMax;
            var customPriceMin = priceMin;

            if ((customOrderBy == ProductSortingEnum.PriceAsc || customOrderBy == ProductSortingEnum.PriceDesc || customOrderBy == ProductSortingEnum.DiscountAsc || customOrderBy == ProductSortingEnum.DiscountDesc || customOrderBy == ProductSortingEnum.DiscountAmount) &&
                (await ShouldFetchPricesFromRemoteAsync() || await ShouldUsePriceLogicFromNopDBAsync()))
            {
                orderBy = ProductSortingEnum.Position;
                pageIndex = 0;
                pageSize = 2147483644;
                priceMax = null;
                priceMin = null;
            }
            else if (customOrderBy == ProductSortingEnum.DiscountAsc || customOrderBy == ProductSortingEnum.DiscountDesc || customOrderBy == ProductSortingEnum.DiscountAmount)
            {
                orderBy = ProductSortingEnum.Position;
                pageIndex = 0;
                pageSize = 2147483644;
            }

            DataParameter stringParameter = SqlParameterHelper.GetStringParameter("CategoryIds", parameterValue);
            DataParameter int32Parameter = SqlParameterHelper.GetInt32Parameter("ManufacturerId", manufacturerId);
            DataParameter int32Parameter2 = SqlParameterHelper.GetInt32Parameter("StoreId", (!_catalogSettings.IgnoreStoreLimitations) ? storeId : 0);
            DataParameter int32Parameter3 = SqlParameterHelper.GetInt32Parameter("VendorId", vendorId);
            DataParameter int32Parameter4 = SqlParameterHelper.GetInt32Parameter("ParentGroupedProductId", 0);
            DataParameter int32Parameter5 = SqlParameterHelper.GetInt32Parameter("ProductTypeId", null);
            DataParameter booleanParameter = SqlParameterHelper.GetBooleanParameter("VisibleIndividuallyOnly", true);
            DataParameter int32Parameter6 = SqlParameterHelper.GetInt32Parameter("ProductTagId", productTagId);
            DataParameter booleanParameter2 = SqlParameterHelper.GetBooleanParameter("FeaturedProducts", featuredProducts);
            DataParameter decimalParameter = SqlParameterHelper.GetDecimalParameter("PriceMin", priceMin);
            DataParameter decimalParameter2 = SqlParameterHelper.GetDecimalParameter("PriceMax", priceMax);
            DataParameter stringParameter2 = SqlParameterHelper.GetStringParameter("Keywords", keywords);
            DataParameter booleanParameter3 = SqlParameterHelper.GetBooleanParameter("SearchDescriptions", searchDescriptions);
            DataParameter booleanParameter4 = SqlParameterHelper.GetBooleanParameter("SearchManufacturerPartNumber", true);
            DataParameter booleanParameter5 = SqlParameterHelper.GetBooleanParameter("SearchSku", searchSku);
            DataParameter booleanParameter6 = SqlParameterHelper.GetBooleanParameter("SearchProductTags", searchProductTags);
            DataParameter booleanParameter7 = SqlParameterHelper.GetBooleanParameter("UseFullTextSearch", false);
            DataParameter int32Parameter7 = SqlParameterHelper.GetInt32Parameter("FullTextMode", 0);
            DataParameter stringParameter3 = SqlParameterHelper.GetStringParameter("FilteredSpecs", text2);
            DataParameter stringParameter4 = SqlParameterHelper.GetStringParameter("FilteredProductVariantAttributes", text3);
            DataParameter stringParameter5 = SqlParameterHelper.GetStringParameter("FilteredManufacturers", text4);
            DataParameter stringParameter6 = SqlParameterHelper.GetStringParameter("FilteredVendors", text5);
            DataParameter booleanParameter8 = SqlParameterHelper.GetBooleanParameter("OnSale", onSale);
            DataParameter booleanParameter9 = SqlParameterHelper.GetBooleanParameter("InStock", inStock);
            DataParameter int32Parameter8 = SqlParameterHelper.GetInt32Parameter("LanguageId", searchLocalizedValue ? languageId : 0);
            DataParameter int32Parameter9 = SqlParameterHelper.GetInt32Parameter("OrderBy", (int)orderBy);
            DataParameter stringParameter7 = SqlParameterHelper.GetStringParameter("AllowedCustomerRoleIds", (!_catalogSettings.IgnoreAcl) ? text : string.Empty);
            DataParameter int32Parameter10 = SqlParameterHelper.GetInt32Parameter("PageIndex", pageIndex);
            DataParameter int32Parameter11 = SqlParameterHelper.GetInt32Parameter("PageSize", pageSize);
            DataParameter booleanParameter10 = SqlParameterHelper.GetBooleanParameter("ShowHidden", showHidden);
            DataParameter booleanParameter11 = SqlParameterHelper.GetBooleanParameter("LoadAvailableFilters", true);
            DataParameter pFilterableSpecificationAttributeOptionIds = SqlParameterHelper.GetOutputStringParameter("FilterableSpecificationAttributeOptionIds");
            pFilterableSpecificationAttributeOptionIds.Size = 2147483646;
            DataParameter pFilterableProductVariantAttributeIds = SqlParameterHelper.GetOutputStringParameter("FilterableProductVariantAttributeIds");
            pFilterableProductVariantAttributeIds.Size = 2147483646;
            DataParameter pFilterableManufacturerIds = SqlParameterHelper.GetOutputStringParameter("FilterableManufacturerIds");
            pFilterableManufacturerIds.Size = 2147483646;
            DataParameter pFilterableVendorIds = SqlParameterHelper.GetOutputStringParameter("FilterableVendorIds");
            pFilterableVendorIds.Size = 2147483646;
            DataParameter booleanParameter12 = SqlParameterHelper.GetBooleanParameter("IsOnSaleFilterEnabled", _nopAjaxFiltersSettings.EnableOnSaleFilter);
            DataParameter booleanParameter13 = SqlParameterHelper.GetBooleanParameter("IsInStockFilterEnabled", _nopAjaxFiltersSettings.EnableInStockFilter);

            //begin intelisale custom code

            var customer = await _workContext.GetCurrentCustomerAsync();
            Company company = null;

            if (customer is not null)
            {
                company = await _companyService.GetByCustomerIdAsync(customer.Id);
            }


            DataParameter int32Parameter12 = SqlParameterHelper.GetInt32Parameter("CustomerGroupId", company is not null ? company.CustomerGroupId : 0);
            DataParameter int32Parameter13 = SqlParameterHelper.GetInt32Parameter("GroupOfCompanyId", company is not null ? company.GroupOfCompanyId : 0);

            DataParameter modifiedKeywords = SqlParameterHelper.GetStringParameter("ModifiedKeywords", keywords?.Trim()?.Replace(" ", "%"));


            //end intelisale custom code

            DataParameter val = new DataParameter();
            val.Name = "HasProductsOnSale";
            val.DataType = LinqToDB.DataType.Boolean;
            val.Direction = ParameterDirection.Output;
            DataParameter pHasProductsOnSale = val;
            DataParameter val2 = new DataParameter();
            val2.Name = "HasProductsInStock";
            val2.DataType = LinqToDB.DataType.Boolean;
            val2.Direction = ParameterDirection.Output;
            DataParameter pHasProductsInStock = val2;
            DataParameter pTotalRecords = SqlParameterHelper.GetOutputInt32Parameter("TotalRecords");
            List<Product> source = (await _productRepository.EntityFromSqlAsync("ProductLoadAllPagedNopAjaxFilters", stringParameter, int32Parameter, int32Parameter2, int32Parameter3, int32Parameter4, int32Parameter5, booleanParameter, int32Parameter6, booleanParameter2, decimalParameter, decimalParameter2, stringParameter2, booleanParameter3, booleanParameter4, booleanParameter5, booleanParameter6, booleanParameter7, int32Parameter7, stringParameter3, stringParameter4, stringParameter5, stringParameter6, booleanParameter8, booleanParameter9, int32Parameter8, int32Parameter9, stringParameter7, int32Parameter10, int32Parameter11, booleanParameter10, booleanParameter11, int32Parameter12, int32Parameter13, pFilterableSpecificationAttributeOptionIds, pFilterableProductVariantAttributeIds, pFilterableManufacturerIds, pFilterableVendorIds, booleanParameter12, booleanParameter13, modifiedKeywords, pHasProductsOnSale, pHasProductsInStock, pTotalRecords)).ToList();
            List<int> specificationOptionIds = new List<int>();
            string text6 = ((pFilterableSpecificationAttributeOptionIds.Value != DBNull.Value) ? ((string)pFilterableSpecificationAttributeOptionIds.Value) : "");
            if (!string.IsNullOrWhiteSpace(text6))
            {
                specificationOptionIds = (from x in text6.Split(new char[1]
                    {
                        ','
                    }, StringSplitOptions.RemoveEmptyEntries)
                                          select Convert.ToInt32(x.Trim())).ToList();
            }
            List<int> productVariantIds = new List<int>();
            string text7 = ((pFilterableProductVariantAttributeIds.Value != DBNull.Value) ? ((string)pFilterableProductVariantAttributeIds.Value) : "");
            if (!string.IsNullOrWhiteSpace(text7))
            {
                productVariantIds = (from x in text7.Split(new char[1]
                    {
                        ','
                    }, StringSplitOptions.RemoveEmptyEntries)
                                     select Convert.ToInt32(x.Trim())).ToList();
            }
            List<int> manufacturerIds = new List<int>();
            string text8 = ((pFilterableManufacturerIds.Value != DBNull.Value) ? ((string)pFilterableManufacturerIds.Value) : "");
            if (!string.IsNullOrWhiteSpace(text8))
            {
                manufacturerIds = (from x in text8.Split(new char[1]
                    {
                        ','
                    }, StringSplitOptions.RemoveEmptyEntries)
                                   select Convert.ToInt32(x.Trim())).ToList();
            }
            List<int> vendorIds = new List<int>();
            string text9 = ((pFilterableVendorIds.Value != DBNull.Value) ? ((string)pFilterableVendorIds.Value) : "");
            if (!string.IsNullOrWhiteSpace(text9))
            {
                vendorIds = (from x in text9.Split(new char[1]
                    {
                        ','
                    }, StringSplitOptions.RemoveEmptyEntries)
                             select Convert.ToInt32(x.Trim())).ToList();
            }
            int value = ((pTotalRecords.Value != DBNull.Value) ? Convert.ToInt32(pTotalRecords.Value) : 0);
            bool hasProductsOnSale = pHasProductsOnSale.Value != DBNull.Value && Convert.ToBoolean(pHasProductsOnSale.Value);
            bool hasProductsInStock = pHasProductsInStock.Value != DBNull.Value && Convert.ToBoolean(pHasProductsInStock.Value);

            var newProductsQuery = source.AsQueryable();
            IPagedList <Product> productsPagedList;
            var isSortedByPrice = false;

            if ((customOrderBy == ProductSortingEnum.PriceAsc || customOrderBy == ProductSortingEnum.PriceDesc ||
                customOrderBy == ProductSortingEnum.DiscountAsc || customOrderBy == ProductSortingEnum.DiscountDesc || customOrderBy == ProductSortingEnum.DiscountAmount) &&
                (await ShouldFetchPricesFromRemoteAsync() || await ShouldUsePriceLogicFromNopDBAsync()))
            {
                var intelisaleProductModelFactory = EngineContext.Current.Resolve<IIntelisaleProductModelFactory>();
                var (productsWithErpPrice, priceList) = await intelisaleProductModelFactory.GetProductsPriceForCategoryProductsAsync(source);

                priceList = priceList.OrderByPrice(customOrderBy);
                isSortedByPrice = true;

                if (priceMin != null)
                    priceList = priceList.Where(p => p.Price >= customPriceMin).ToList();
                if (priceMax != null)
                    priceList = priceList.Where(p => p.Price <= customPriceMax).ToList();

                productsWithErpPrice = priceList.Select(o => productsWithErpPrice.FirstOrDefault(x => x.Id == o.Id))
                                                .Where(item => item != null).ToList();

                newProductsQuery = productsWithErpPrice.AsQueryable();
            }

            if(isSortedByPrice)
            {
                productsPagedList = await newProductsQuery.ToPagedListAsync(customPageIndex, customPageSize);
            }
            else if (!isSortedByPrice && (customOrderBy == ProductSortingEnum.DiscountAsc || customOrderBy == ProductSortingEnum.DiscountDesc || customOrderBy == ProductSortingEnum.DiscountAmount))
            {
                productsPagedList = await newProductsQuery.OrderBy(customOrderBy).ToPagedListAsync(customPageIndex, customPageSize);
            }
            else
            {
                productsPagedList = new PagedList<Product>(source, pageIndex, pageSize, value);
            }

            return new ProductsResultDataDTO
            {
                ProductsPagedList = productsPagedList,
                SpecificationOptionIds = specificationOptionIds,
                ProductVariantIds = productVariantIds,
                ManufacturerIds = manufacturerIds,
                VendorIds = vendorIds,
                HasProductsOnSale = hasProductsOnSale,
                HasProductsInStock = hasProductsInStock
            };
        }

        private async Task<bool> ShouldFetchPricesFromRemoteAsync()
        {
            var intelisaleService = EngineContext.Current.Resolve<IIntelisaleService>();
            var buyerType = await intelisaleService.GetCommercialTransactionTypeAsync();
            return (buyerType == CommercialTransactionType.B2b && _intelisalePluginProviderSettings.AllowRemoteB2BPrices)
                || (buyerType == CommercialTransactionType.B2c && _intelisalePluginProviderSettings.AllowRemoteB2cPrices);
        }

        private async Task<bool> ShouldUsePriceLogicFromNopDBAsync()
        {
            var intelisaleService = EngineContext.Current.Resolve<IIntelisaleService>();
            var buyerType = await intelisaleService.GetCommercialTransactionTypeAsync();
            return (buyerType == CommercialTransactionType.B2b && _intelisalePluginProviderSettings.B2bPriceLogicUsingNopDb)
                || (buyerType == CommercialTransactionType.B2c && _intelisalePluginProviderSettings.B2cPriceLogicUsingNopDb);
        }

        public async Task<IList<int>> GetAllGroupProductIdsInCategoriesAsync(List<int> categoriesIds)
        {
            return (await GetAllGroupProductIdsInCategoriesInternalAsync(categoriesIds)).ToList();
        }

        public async Task<IList<int>> GetAllGroupProductIdsInCategoryAsync(int categoryId)
        {
            return (await GetAllGroupProductIdsInCategoriesInternalAsync(new List<int>
            {
                categoryId
            })).ToList();
        }

        public async Task<IList<int>> GetAllGroupProductIdsInManufacturerAsync(int manufacturerId)
        {
            return (await GetAllGroupProductIdsInManufacturerInternalAsync(manufacturerId)).ToList();
        }

        public async Task<IList<int>> GetAllGroupProductIdsInVendorAsync(int vendorId)
        {
            return (await GetAllGroupProductIdsInVendorInternalAsync(vendorId)).ToList();
        }

        public async Task<bool> HasProductsOnSaleInternalAsync(int categoryId, int manufacturerId, int vendorId)
        {
            CacheKey key = _cacheManager.PrepareKeyForDefaultCache(ONSALESTATE_BY_PRODUCTIDS_KEY, categoryId, manufacturerId, vendorId);
            return await _cacheManager.GetAsync(key, async delegate
            {
                bool hasProducts = false;
                IQueryable<Product> availableProducts = await _aclHelper.GetAvailableProductsForCurrentCustomerAsync();
                bool showProductsFromSubcategories = _catalogSettings.ShowProductsFromSubcategories;
                bool includeFeaturedProducts = _catalogSettings.IncludeFeaturedProductsInNormalLists;
                if (categoryId > 0)
                {
                    List<int> categoryIds = new List<int>
                    {
                        categoryId
                    };
                    if (showProductsFromSubcategories)
                    {
                        IEnumerable<int> collection = (await _categoryService7Spikes.GetCategoriesByParentCategoryIdAsync(categoryId, includeSubCategoriesFromAllLevels: true)).Select((Category x) => x.Id);
                        categoryIds.AddRange(collection);
                    }
                    IList<int> groupProductIds = await GetAllGroupProductIdsInCategoriesInternalAsync(categoryIds);
                    hasProducts = await HasAvailableProductsOnSaleInCategoryAsync(availableProducts, groupProductIds, categoryIds, includeFeaturedProducts);
                }
                else if (manufacturerId > 0)
                {
                    IList<int> groupProductIds2 = await GetAllGroupProductIdsInManufacturerInternalAsync(manufacturerId);
                    hasProducts = await HasAvailableProductsOnSaleInManufacturerAsync(availableProducts, groupProductIds2, manufacturerId, includeFeaturedProducts);
                }
                else if (vendorId > 0)
                {
                    IList<int> groupProductIds3 = await GetAllGroupProductIdsInVendorInternalAsync(vendorId);
                    hasProducts = await HasAvailableProductsOnSaleInVendorAsync(availableProducts, groupProductIds3, vendorId, includeFeaturedProducts);
                }
                return hasProducts;
            });
        }

        public async Task<bool> HasProductsInStockInternalAsync(int categoryId, int manufacturerId, int vendorId)
        {
            CacheKey key = _cacheManager.PrepareKeyForDefaultCache(INSTOCKSTATE_BY_PRODUCTIDS_KEY, categoryId, manufacturerId, vendorId);
            return await _cacheManager.GetAsync(key, async delegate
            {
                bool hasProducts = false;
                IQueryable<Product> availableProducts = await _aclHelper.GetAvailableProductsForCurrentCustomerAsync();
                bool showProductsFromSubcategories = _catalogSettings.ShowProductsFromSubcategories;
                bool includeFeaturedProducts = _catalogSettings.IncludeFeaturedProductsInNormalLists;
                if (categoryId > 0)
                {
                    List<int> categoryIds = new List<int>
                    {
                        categoryId
                    };
                    if (showProductsFromSubcategories)
                    {
                        IEnumerable<int> collection = (await _categoryService7Spikes.GetCategoriesByParentCategoryIdAsync(categoryId, includeSubCategoriesFromAllLevels: true)).Select((Category x) => x.Id);
                        categoryIds.AddRange(collection);
                    }
                    IList<int> groupProductIds = await GetAllGroupProductIdsInCategoriesInternalAsync(categoryIds);
                    hasProducts = await HasAvailableProductsInStockInCategoryAsync(availableProducts, groupProductIds, categoryIds, includeFeaturedProducts);
                }
                else if (manufacturerId > 0)
                {
                    IList<int> groupProductIds2 = await GetAllGroupProductIdsInManufacturerInternalAsync(manufacturerId);
                    hasProducts = await HasAvailableProductsInStockInManufacturerAsync(availableProducts, groupProductIds2, manufacturerId, includeFeaturedProducts);
                }
                else if (vendorId > 0)
                {
                    IList<int> groupProductIds3 = await GetAllGroupProductIdsInVendorInternalAsync(vendorId);
                    hasProducts = await HasAvailableProductsInStockInVendorAsync(availableProducts, groupProductIds3, vendorId, includeFeaturedProducts);
                }
                return hasProducts;
            });
        }

        private async Task<bool> HasAvailableProductsInStockInVendorAsync(IQueryable<Product> availableProducts, IList<int> groupProductIds, int vendorId, bool includeFeaturedProducts)
        {
            return await (from p in availableProducts
                          join v in from v in _vendorRepository.Table
                                    where v.Active && !v.Deleted
                                    select v on p.VendorId equals v.Id into p_pv
                          from v in p_pv.DefaultIfEmpty()
                          where ((v != null && v.Id == vendorId && p.ProductTypeId != 10 && p.VisibleIndividually) || groupProductIds.Contains(p.ParentGroupedProductId)) && p.Published && !p.Deleted && (p.ManageInventoryMethodId == 0 || (p.ManageInventoryMethodId == 1 && ((p.StockQuantity > 0 && !p.UseMultipleWarehouses) || (p.UseMultipleWarehouses && _productWarehouseInventory.Table.Where((ProductWarehouseInventory pwi) => pwi.StockQuantity > 0 && pwi.StockQuantity > pwi.ReservedQuantity).Any((ProductWarehouseInventory pwi) => pwi.ProductId == p.Id)))))
                          select p).AnyAsync();
        }

        private async Task<bool> HasAvailableProductsInStockInManufacturerAsync(IQueryable<Product> availableProducts, IList<int> groupProductIds, int manufacturerId, bool includeFeaturedProducts)
        {
            IQueryable<ProductManufacturer> inner = from pm in _productManufacturerRepository.Table
                                                    join m in _manufacturerRepository.Table on pm.ManufacturerId equals m.Id
                                                    where m.Published && !m.Deleted
                                                    select pm;
            return await (from p in availableProducts
                          join pm in inner on p.Id equals pm.ProductId into p_pm
                          from pm in p_pm.DefaultIfEmpty()
                          where (pm != null && pm.ManufacturerId == manufacturerId && p.ProductTypeId != 10 && p.VisibleIndividually && (pm.IsFeaturedProduct == includeFeaturedProducts || !pm.IsFeaturedProduct)) || (groupProductIds.Contains(p.ParentGroupedProductId) && p.Published && !p.Deleted && (p.ManageInventoryMethodId == 0 || (p.ManageInventoryMethodId == 1 && ((p.StockQuantity > 0 && !p.UseMultipleWarehouses) || (p.UseMultipleWarehouses && _productWarehouseInventory.Table.Where((ProductWarehouseInventory pwi) => pwi.StockQuantity > 0 && pwi.StockQuantity > pwi.ReservedQuantity).Any((ProductWarehouseInventory pwi) => pwi.ProductId == p.Id))))))
                          select p).AnyAsync();
        }

        private async Task<bool> HasAvailableProductsInStockInCategoryAsync(IQueryable<Product> availableProducts, IList<int> groupProductIds, List<int> categoryIds, bool includeFeaturedProducts)
        {
            return await (from p in availableProducts
                          join pc in _productCategoryRepository.Table on p.Id equals pc.ProductId into p_pc
                          from pc in p_pc.DefaultIfEmpty()
                          where ((pc != null && categoryIds.Contains(pc.CategoryId) && p.ProductTypeId != 10 && p.VisibleIndividually && (pc.IsFeaturedProduct == includeFeaturedProducts || !pc.IsFeaturedProduct)) || groupProductIds.Contains(p.ParentGroupedProductId)) && p.Published && !p.Deleted && (p.ManageInventoryMethodId == 0 || (p.ManageInventoryMethodId == 1 && ((p.StockQuantity > 0 && !p.UseMultipleWarehouses) || (p.UseMultipleWarehouses && _productWarehouseInventory.Table.Where((ProductWarehouseInventory pwi) => pwi.StockQuantity > 0 && pwi.StockQuantity > pwi.ReservedQuantity).Any((ProductWarehouseInventory pwi) => pwi.ProductId == p.Id)))))
                          select p).AnyAsync();
        }

        private async Task<IList<int>> GetAllGroupProductIdsInCategoriesInternalAsync(List<int> categoryIds)
        {
            CacheKey key = _cacheManager.PrepareKeyForDefaultCache(GROUPPRODUCTIDS_BY_CATEGORYIDS, categoryIds);
            return await _cacheManager.GetAsync(key, async delegate
            {
                IQueryable<Product> source = (await _aclHelper.GetAvailableProductsForCurrentCustomerAsync()).Where((Product p) => !p.Deleted);
                source = source.Where((Product p) => p.Published);
                source = source.Where((Product p) => p.ProductTypeId == 10);
                if (categoryIds != null && categoryIds.Count > 0)
                {
                    source = from p in source
                             from pc in from pc in _productCategoryRepository.Table
                                        where categoryIds.Contains(pc.CategoryId)
                                        select pc
                             where pc.ProductId == p.Id
                             select p;
                }
                return await source.Select((Product x) => x.Id).ToListAsync();
            });
        }

        private async Task<IList<int>> GetAllGroupProductIdsInManufacturerInternalAsync(int manufacturerId)
        {
            CacheKey key = _cacheManager.PrepareKeyForDefaultCache(GROUPPRODUCTIDS_BY_MANUFACTURERID, manufacturerId);
            return await _cacheManager.GetAsync(key, async () => await (from p in await _aclHelper.GetAvailableProductsForCurrentCustomerAsync()
                                                                        where !p.Deleted
                                                                        where p.Published
                                                                        where p.ProductTypeId == 10
                                                                        from pm in _productManufacturerRepository.Table
                                                                        where p.Id == pm.ProductId && pm.ManufacturerId == manufacturerId
                                                                        select p into x
                                                                        select x.Id).ToListAsync());
        }

        private async Task<IList<int>> GetAllGroupProductIdsInVendorInternalAsync(int vendorId)
        {
            CacheKey key = _cacheManager.PrepareKeyForDefaultCache(GROUPPRODUCTIDS_BY_VENDORID, vendorId);
            return await _cacheManager.GetAsync(key, async () => await (from p in await _aclHelper.GetAvailableProductsForCurrentCustomerAsync()
                                                                        where !p.Deleted
                                                                        where p.Published
                                                                        where p.ProductTypeId == 10
                                                                        where p.VendorId == vendorId
                                                                        select p into x
                                                                        select x.Id).ToListAsync());
        }

        private async Task<bool> HasAvailableProductsOnSaleInCategoryAsync(IQueryable<Product> availableProducts, IList<int> groupProductIds, IList<int> categoryIds, bool includeFeaturedProducts)
        {
            DateTime nowUtc = DateTime.UtcNow;
            return await (from p in availableProducts
                          join pc in _productCategoryRepository.Table on p.Id equals pc.ProductId into p_pc
                          from pc in p_pc.DefaultIfEmpty()
                          where ((pc != null && categoryIds.Contains(pc.CategoryId) && p.ProductTypeId != 10 && p.VisibleIndividually && (pc.IsFeaturedProduct == includeFeaturedProducts || !pc.IsFeaturedProduct)) || groupProductIds.Contains(p.ParentGroupedProductId)) && p.Published && !p.Deleted && (!p.AvailableStartDateTimeUtc.HasValue || (DateTime)p.AvailableStartDateTimeUtc <= (DateTime)(DateTime?)nowUtc) && (!p.AvailableEndDateTimeUtc.HasValue || (DateTime)p.AvailableEndDateTimeUtc >= (DateTime)(DateTime?)nowUtc) && p.OldPrice > 0m && p.OldPrice != p.Price
                          select p).AnyAsync();
        }

        private async Task<bool> HasAvailableProductsOnSaleInVendorAsync(IQueryable<Product> availableProducts, IList<int> groupProductIds, int vendorId, bool includeFeaturedProducts)
        {
            DateTime nowUtc = DateTime.UtcNow;
            return await (from p in availableProducts
                          join v in from v in _vendorRepository.Table
                                    where v.Active && !v.Deleted
                                    select v on p.VendorId equals v.Id into p_pv
                          from v in p_pv.DefaultIfEmpty()
                          where ((v != null && v.Id == vendorId && p.ProductTypeId != 10 && p.VisibleIndividually) || groupProductIds.Contains(p.ParentGroupedProductId)) && p.Published && !p.Deleted && (!p.AvailableStartDateTimeUtc.HasValue || (DateTime)p.AvailableStartDateTimeUtc <= (DateTime)(DateTime?)nowUtc) && (!p.AvailableEndDateTimeUtc.HasValue || (DateTime)p.AvailableEndDateTimeUtc >= (DateTime)(DateTime?)nowUtc) && p.OldPrice > 0m && p.OldPrice != p.Price
                          select p).AnyAsync();
        }

        private async Task<bool> HasAvailableProductsOnSaleInManufacturerAsync(IQueryable<Product> availableProducts, IList<int> groupProductIds, int manufacturerId, bool includeFeaturedProducts)
        {
            DateTime nowUtc = DateTime.UtcNow;
            IQueryable<ProductManufacturer> inner = from pm in _productManufacturerRepository.Table
                                                    join m in _manufacturerRepository.Table on pm.ManufacturerId equals m.Id
                                                    where m.Published && !m.Deleted
                                                    select pm;
            return await (from p in availableProducts
                          join pm in inner on p.Id equals pm.ProductId into p_pm
                          from pm in p_pm.DefaultIfEmpty()
                          where ((pm != null && pm.ManufacturerId == manufacturerId && p.ProductTypeId != 10 && p.VisibleIndividually && (pm.IsFeaturedProduct == includeFeaturedProducts || !pm.IsFeaturedProduct)) || groupProductIds.Contains(p.ParentGroupedProductId)) && p.Published && !p.Deleted && (!p.AvailableStartDateTimeUtc.HasValue || (DateTime)p.AvailableStartDateTimeUtc <= (DateTime)(DateTime?)nowUtc) && (!p.AvailableEndDateTimeUtc.HasValue || (DateTime)p.AvailableEndDateTimeUtc >= (DateTime)(DateTime?)nowUtc) && p.OldPrice > 0m && p.OldPrice != p.Price
                          select p).AnyAsync();
        }
    }
}