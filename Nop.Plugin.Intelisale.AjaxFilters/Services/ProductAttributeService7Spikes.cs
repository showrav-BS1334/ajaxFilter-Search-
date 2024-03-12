using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using SevenSpikes.Nop.Services.Catalog;
using SevenSpikes.Nop.Services.Catalog.DTO;
using SevenSpikes.Nop.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Plugin.Intelisale.AjaxFilters.Services
{
    public class ProductAttributeService7Spikes : IProductAttributeService7Spikes
    {
        private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IAclHelper _aclHelper;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IProductServiceNopAjaxFilters _productServiceNopAjaxFilters;
        private readonly ILanguageService _languageService;
        private IStoreHelper _storeHelper;

        private CacheKey PRODUCTATTRIBUTES_BY_CATEGORYID_KEY => new CacheKey("nop.pres.nop.ajax.filters.productattribute.categoryid-{0}.includeproductsinsubcategories-{1}.showhidden-{2}.store.id-{3}", "nop.pres.nop.ajax.filters");

        private CacheKey PRODUCTATTRIBUTES_BY_MANUFACTURERID_KEY => new CacheKey("nop.pres.nop.ajax.filters.productattribute.manufacturerid-{0}.showhidden-{1}.store.id-{2}", "nop.pres.nop.ajax.filters");

        private CacheKey PRODUCTATTRIBUTES_BY_VENDORID_KEY => new CacheKey("nop.pres.nop.ajax.filters.productattribute.vendorid-{0}.showhidden-{1}.store.id-{2}", "nop.pres.nop.ajax.filters");

        private CacheKey PRODUCTATTRIBUTES_BY_PRODUCTVARIANTIDS_KEY => new CacheKey("nop.pres.nop.ajax.filters..productattribute.productvariantids-{0}.showhidden-{1}.store.id-{2}", "nop.pres.nop.ajax.filters");

        private CacheKey PRODUCTATTRIBUTEVALUES_BY_PRODUCTATTRIBUTEID_AND_CATEGORYID_KEY => new CacheKey("nop.pres.nop.ajax.filters.productattributevalues.productattributeid-{0}.categoryid-{1}.includeproductsinsubcategories-{2}.showhidden-{3}.store.id-{4}.language.id-{5}", "nop.pres.nop.ajax.filters");

        private CacheKey PRODUCTATTRIBUTEVALUES_BY_PRODUCTATTRIBUTEID_AND_MANUFACTURERID_KEY => new CacheKey("nop.pres.nop.ajax.filters.productattributevalues.productattributeid-{0}.manufacturerid-{1}.showhidden-{2}.store.id-{3}.language.id-{4}", "nop.pres.nop.ajax.filters");

        private CacheKey PRODUCTATTRIBUTEVALUES_BY_PRODUCTATTRIBUTEID_AND_VENDORID_KEY => new CacheKey("nop.pres.nop.ajax.filters.productattributevalues.productattributeid-{0}.vendorid-{1}.showhidden-{2}.store.id-{3}.language.id-{4}", "nop.pres.nop.ajax.filters");

        private CacheKey PRODUCTATTRIBUTEVALUES_AND_PRODUCT_ATTRIBUTES_BY_PRODUCTATTRIBUTEMAPPINGS_KEY => new CacheKey("nop.pres.nop.ajax.filters.productattributevalues.productattributemappingids-{0}.store.id-{1}.language.id-{2}", "nop.pres.nop.ajax.filters");

        private CacheKey PRODUCTVARIANTATTRIBUTES_BY_PRODUCTID_KEY => new CacheKey("nop.pres.nop.ajax.filters.productvariantattribute.productid-{0}.store.id-{1}", "nop.pres.nop.ajax.filters");

        private CacheKey PRODUCTVARIANTATTRIBUTES_WHICH_HAVE_VALUES_BY_PRODUCTID_KEY => new CacheKey("nop.pres.nop.ajax.filters.productvariantattributeswhichhavevalues.productid-{0}.store.id-{1}", "nop.pres.nop.ajax.filters");

        private CacheKey PRODUCTVARIANTATTRIBUTES_WHICH_HAVE_VALUES_BY_PRODUCTIDS_KEY => new CacheKey("nop.pres.nop.ajax.filters.productvariantattributeswhichhavevalues.productids-{0}.store.id-{1}", "nop.pres.nop.ajax.filters");

        private IRepository<Product> ProductRepository { get; set; }
        private IRepository<Category> CategoryRepository { get; set; }
        private IRepository<ProductAttribute> ProductAttributeRepository { get; set; }
        private IRepository<ProductAttributeMapping> ProductAttributeMappingRepository { get; set; }
        private IRepository<ProductAttributeValue> ProductAttributeValueRepository { get; set; }
        private IRepository<ProductCategory> ProductCategoryRepository { get; set; }
        private IRepository<LocalizedProperty> LocalizedPropertyRepository { get; set; }
        private ICategoryService7Spikes CategoryService7Spikes { get; set; }
        private ISettingService SettingService { get; set; }
        private IStaticCacheManager CacheManager { get; set; }

        public ProductAttributeService7Spikes(
            IRepository<Product> productRepository,
            IRepository<ProductManufacturer> productManufacturerRepository,
            IRepository<Vendor> vendorRepository,
            IRepository<Category> categoryRepository,
            IRepository<ProductAttribute> productAttributeRepository,
            IRepository<ProductAttributeMapping> productVariantAttributeRepository,
            IRepository<ProductAttributeValue> productVariantAttributeValueRepository,
            IRepository<ProductCategory> productCategoryRepository,
            ICategoryService7Spikes categoryService7Spikes,
            ISettingService settingService,
            IStaticCacheManager cacheManager,
            IAclHelper aclHelper,
            IStoreContext storeContext,
            IProductServiceNopAjaxFilters productServiceNopAjaxFilters,
            IStoreHelper storeHelper,
            IRepository<LocalizedProperty> localizedProperty,
            ILanguageService languageService,
            IWorkContext workContext
            )
        {
            _productManufacturerRepository = productManufacturerRepository;
            _vendorRepository = vendorRepository;
            _aclHelper = aclHelper;
            _storeContext = storeContext;
            _workContext = workContext;
            _productServiceNopAjaxFilters = productServiceNopAjaxFilters;
            _storeHelper = storeHelper;
            _languageService = languageService;
            ProductRepository = productRepository;
            CategoryRepository = categoryRepository;
            ProductAttributeRepository = productAttributeRepository;
            ProductAttributeMappingRepository = productVariantAttributeRepository;
            ProductAttributeValueRepository = productVariantAttributeValueRepository;
            ProductCategoryRepository = productCategoryRepository;
            LocalizedPropertyRepository = localizedProperty;
            CategoryService7Spikes = categoryService7Spikes;
            SettingService = settingService;
            CacheManager = cacheManager;
        }

        public virtual async Task<IList<ProductAttribute>> GetAllProductAttributesByCategoryIdAsync(int categoryId, bool includeProductsInSubcategories = false, bool showHiddenProducts = false)
        {
            return await GetAllProductAttributesByCategoryIdInternalAsync(categoryId, includeProductsInSubcategories, showHiddenProducts);
        }

        public async Task<IList<ProductAttribute>> GetAllProductAttributesByManufacturerIdAsync(int manufacturerId, bool showHiddenProducts = false)
        {
            return await GetAllProductAttributesByManufacturerIdInternalAsync(manufacturerId, showHiddenProducts);
        }

        public async Task<IList<ProductAttribute>> GetAllProductAttributesByVendorIdAsync(int vendorId, bool showHiddenProducts = false)
        {
            return await GetAllProductAttributesByVendorIdInternalAsync(vendorId, showHiddenProducts);
        }

        public async Task<IList<ProductAttribute>> GetAllProductAttributesByProductAttributeMappingIdsAsync(IList<int> productAttributeMappingIds, bool showHiddenProducts = false)
        {
            return await GetAllProductAttributesByProductAttributeMappingIdsInternalAsync(productAttributeMappingIds, showHiddenProducts);
        }

        public virtual async Task<IList<ProductAttributeValue>> GetAllProductVariantAttributeValuesByProductAttributeIdAndCategoryIdAsync(int productAttributeId, int categoryId, bool includeProductsInSubcategories = false, bool showHiddenProducts = false)
        {
            return await GetAllProductVariantAttributeValuesByProductAttributeIdAndCategoryIdInternalAsync(productAttributeId, categoryId, includeProductsInSubcategories, showHiddenProducts);
        }

        public async Task<IList<ProductAttributeValue>> GetAllProductVariantAttributeValuesByProductAttributeIdAndManufacturerIdAsync(int productAttributeId, int manufacturerId, bool showHiddenProducts = false)
        {
            return await GetAllProductVariantAttributeValuesByProductAttributeIdAndManufacturerIdInternalAsync(productAttributeId, manufacturerId, showHiddenProducts);
        }

        public async Task<IList<ProductAttributeValue>> GetAllProductVariantAttributeValuesByProductAttributeIdAndVendorIdAsync(int productAttributeId, int vendorId, bool showHiddenProducts = false)
        {
            return await GetAllProductVariantAttributeValuesByProductAttributeIdAndVendorIdInternalAsync(productAttributeId, vendorId, showHiddenProducts);
        }

        public virtual async Task<IList<ProductAttributeMapping>> GetAllProductVariantAttributesByProductIdAsync(int productId, bool showHiddenProducts = false)
        {
            return await GetAllProductVariantAttributesByProductIdInternalAsync(productId, showHiddenProducts);
        }

        public virtual async Task<IList<ProductAttributeMapping>> GetAllProductVariantAttributesWhichHaveValuesByProductIdAsync(int productId, bool showHiddenProducts = false)
        {
            return await GetAllProductVariantAttributesWhichHaveValuesByProductIdInternalAsync(productId, showHiddenProducts);
        }

        public virtual async Task<IList<ProductAttributeMapping>> GetAllProductVariantAttributesWhichHaveValuesByProductIdsAsync(IList<int> productIds, bool showHiddenProducts = false)
        {
            return await GetAllProductVariantAttributesWhichHaveValuesByProductIdsInternalAsync(productIds, showHiddenProducts);
        }

        public async Task<IList<ProductAttributeProductAttributeValueDTO>> GetProductAttributeProductAttributeValueDtosByProductAttributeMappingIdsAsync(IList<int> productAttributeMappingIds)
        {
            return await GetProductAttributeProductAttributeValueDtosByProductAttributeMappingIdsInternalAsync(productAttributeMappingIds);
        }

        private async Task<IList<ProductAttribute>> GetAllProductAttributesByCategoryIdInternalAsync(int categoryId, bool includeProductsInSubcategories, bool showHiddenProducts)
        {
            if (categoryId == 0)
            {
                return null;
            }
            DateTime nowUtc = DateTime.UtcNow;
            IStaticCacheManager cacheManager = CacheManager;
            CacheKey pRODUCTATTRIBUTES_BY_CATEGORYID_KEY = PRODUCTATTRIBUTES_BY_CATEGORYID_KEY;
            object obj = categoryId;
            object obj2 = includeProductsInSubcategories;
            object obj3 = showHiddenProducts;
            Store store = await _storeContext.GetCurrentStoreAsync();
            CacheKey key = cacheManager.PrepareKeyForDefaultCache(pRODUCTATTRIBUTES_BY_CATEGORYID_KEY, obj, obj2, obj3, store.Id);
            return new List<ProductAttribute>(await CacheManager.GetAsync(key, (Func<Task<IEnumerable<ProductAttribute>>>)async delegate
            {
                List<int> categoryIds = new List<int>
                {
                    categoryId
                };
                if (includeProductsInSubcategories)
                {
                    List<int> collection = await CategoryService7Spikes.GetCategoryIdsByParentCategoryAsync(categoryId);
                    categoryIds.AddRange(collection);
                }
                IList<int> groupProductIds = await _productServiceNopAjaxFilters.GetAllGroupProductIdsInCategoriesAsync(categoryIds);
                IQueryable<Product> availableProductsQuery = await _aclHelper.GetAvailableProductsForCurrentCustomerAsync();
                availableProductsQuery = await _storeHelper.GetProductsForCurrentStoreAsync(availableProductsQuery);
                bool includeFeaturedProductsInNormalList = await GetIncludeFeaturedProductsInNormalListAsync();
                return await (from pa in ProductAttributeRepository.Table
                              join pva in ProductAttributeMappingRepository.Table on pa.Id equals pva.ProductAttributeId
                              join p in availableProductsQuery on pva.ProductId equals p.Id
                              join pc in ProductCategoryRepository.Table on p.Id equals pc.ProductId into p_pc
                              from pc in p_pc.DefaultIfEmpty()
                              where ((pc != null && categoryIds.Contains(pc.CategoryId) && (p.ParentGroupedProductId == 0 || p.VisibleIndividually)) || (pc == null && groupProductIds.Contains(p.ParentGroupedProductId))) && (includeFeaturedProductsInNormalList || pc == null || !pc.IsFeaturedProduct) && (showHiddenProducts || p.Published) && !p.Deleted && (!p.AvailableStartDateTimeUtc.HasValue || (DateTime)p.AvailableStartDateTimeUtc <= (DateTime)(DateTime?)nowUtc) && (!p.AvailableEndDateTimeUtc.HasValue || (DateTime)p.AvailableEndDateTimeUtc >= (DateTime)(DateTime?)nowUtc)
                              select pa).Distinct().ToListAsync();
            }));
        }

        private async Task<IList<ProductAttribute>> GetAllProductAttributesByManufacturerIdInternalAsync(int manufacturerId, bool showHiddenProducts)
        {
            if (manufacturerId == 0)
            {
                return null;
            }
            DateTime nowUtc = DateTime.UtcNow;
            IStaticCacheManager cacheManager = CacheManager;
            CacheKey pRODUCTATTRIBUTES_BY_MANUFACTURERID_KEY = PRODUCTATTRIBUTES_BY_MANUFACTURERID_KEY;
            object obj = manufacturerId;
            object obj2 = showHiddenProducts;
            Store store = await _storeContext.GetCurrentStoreAsync();
            CacheKey key = cacheManager.PrepareKeyForDefaultCache(pRODUCTATTRIBUTES_BY_MANUFACTURERID_KEY, obj, obj2, store.Id);
            return new List<ProductAttribute>(await CacheManager.GetAsync(key, (Func<Task<IEnumerable<ProductAttribute>>>)async delegate
            {
                IList<int> groupProductIds = await _productServiceNopAjaxFilters.GetAllGroupProductIdsInManufacturerAsync(manufacturerId);
                IQueryable<Product> availableProductsQuery = await _aclHelper.GetAvailableProductsForCurrentCustomerAsync();
                availableProductsQuery = await _storeHelper.GetProductsForCurrentStoreAsync(availableProductsQuery);
                bool includeFeaturedProductsInNormalList = await GetIncludeFeaturedProductsInNormalListAsync();
                return await (from pa in ProductAttributeRepository.Table
                              join pva in ProductAttributeMappingRepository.Table on pa.Id equals pva.ProductAttributeId
                              join p in availableProductsQuery on pva.ProductId equals p.Id
                              join pm in _productManufacturerRepository.Table on p.Id equals pm.ProductId into p_pm
                              from pm in p_pm.DefaultIfEmpty()
                              where ((pm != null && pm.ManufacturerId == manufacturerId && (includeFeaturedProductsInNormalList || !pm.IsFeaturedProduct) && (p.ParentGroupedProductId == 0 || p.VisibleIndividually)) || (pm == null && groupProductIds.Contains(p.ParentGroupedProductId))) && (showHiddenProducts || p.Published) && !p.Deleted && (!p.AvailableStartDateTimeUtc.HasValue || (DateTime)p.AvailableStartDateTimeUtc <= (DateTime)(DateTime?)nowUtc) && (!p.AvailableEndDateTimeUtc.HasValue || (DateTime)p.AvailableEndDateTimeUtc >= (DateTime)(DateTime?)nowUtc)
                              select pa).Distinct().ToListAsync();
            }));
        }

        private async Task<IList<ProductAttribute>> GetAllProductAttributesByVendorIdInternalAsync(int vendorId, bool showHiddenProducts)
        {
            if (vendorId == 0)
            {
                return null;
            }
            DateTime nowUtc = DateTime.UtcNow;
            IStaticCacheManager cacheManager = CacheManager;
            CacheKey pRODUCTATTRIBUTES_BY_VENDORID_KEY = PRODUCTATTRIBUTES_BY_VENDORID_KEY;
            object obj = vendorId;
            object obj2 = showHiddenProducts;
            Store store = await _storeContext.GetCurrentStoreAsync();
            CacheKey key = cacheManager.PrepareKeyForDefaultCache(pRODUCTATTRIBUTES_BY_VENDORID_KEY, obj, obj2, store.Id);
            return new List<ProductAttribute>(await CacheManager.GetAsync(key, (Func<Task<IEnumerable<ProductAttribute>>>)async delegate
            {
                IList<int> groupProductIds = await _productServiceNopAjaxFilters.GetAllGroupProductIdsInVendorAsync(vendorId);
                IQueryable<Product> productsQuery = await _aclHelper.GetAvailableProductsForCurrentCustomerAsync();
                productsQuery = await _storeHelper.GetProductsForCurrentStoreAsync(productsQuery);
                return await (from pa in ProductAttributeRepository.Table
                              join pva in ProductAttributeMappingRepository.Table on pa.Id equals pva.ProductAttributeId
                              join p in productsQuery on pva.ProductId equals p.Id
                              join v in _vendorRepository.Table on p.VendorId equals v.Id into p_pv
                              from v in p_pv.DefaultIfEmpty()
                              where ((v != null && v.Id == vendorId && (p.ParentGroupedProductId == 0 || p.VisibleIndividually)) || (v == null && groupProductIds.Contains(p.ParentGroupedProductId))) && (showHiddenProducts || p.Published) && !p.Deleted && (!p.AvailableStartDateTimeUtc.HasValue || (DateTime)p.AvailableStartDateTimeUtc <= (DateTime)(DateTime?)nowUtc) && (!p.AvailableEndDateTimeUtc.HasValue || (DateTime)p.AvailableEndDateTimeUtc >= (DateTime)(DateTime?)nowUtc)
                              select pa).Distinct().ToListAsync();
            }));
        }

        private async Task<IList<ProductAttribute>> GetAllProductAttributesByProductAttributeMappingIdsInternalAsync(IList<int> productAttributeMappingIds, bool showHiddenProducts)
        {
            string text = string.Join(",", productAttributeMappingIds);
            IStaticCacheManager cacheManager = CacheManager;
            CacheKey pRODUCTATTRIBUTES_BY_PRODUCTVARIANTIDS_KEY = PRODUCTATTRIBUTES_BY_PRODUCTVARIANTIDS_KEY;
            object obj = text;
            object obj2 = showHiddenProducts;
            Store store = await _storeContext.GetCurrentStoreAsync();
            CacheKey key = cacheManager.PrepareKeyForDefaultCache(pRODUCTATTRIBUTES_BY_PRODUCTVARIANTIDS_KEY, obj, obj2, store.Id);
            return new List<ProductAttribute>(await CacheManager.GetAsync(key, (Func<Task<IEnumerable<ProductAttribute>>>)(async () => await (from pa in ProductAttributeRepository.Table
                                                                                                                                              from pva in ProductAttributeMappingRepository.Table
                                                                                                                                              where productAttributeMappingIds.Contains(pva.Id) && pva.ProductAttributeId == pa.Id
                                                                                                                                              select pa).Distinct().ToListAsync())));
        }

        private async Task<IList<ProductAttributeValue>> GetAllProductVariantAttributeValuesByProductAttributeIdAndCategoryIdInternalAsync(int productAttributeId, int categoryId, bool includeProductsInSubcategories, bool showHiddenProducts)
        {
            if (productAttributeId == 0 || categoryId == 0)
            {
                return null;
            }
            DateTime nowUtc = DateTime.UtcNow;
            IStaticCacheManager cacheManager = CacheManager;
            CacheKey pRODUCTATTRIBUTEVALUES_BY_PRODUCTATTRIBUTEID_AND_CATEGORYID_KEY = PRODUCTATTRIBUTEVALUES_BY_PRODUCTATTRIBUTEID_AND_CATEGORYID_KEY;
            object obj = productAttributeId;
            object obj2 = categoryId;
            object obj3 = includeProductsInSubcategories;
            object obj4 = showHiddenProducts;
            object obj5 = (await _storeContext.GetCurrentStoreAsync()).Id;
            Language language = await _workContext.GetWorkingLanguageAsync();
            CacheKey key = cacheManager.PrepareKeyForDefaultCache(pRODUCTATTRIBUTEVALUES_BY_PRODUCTATTRIBUTEID_AND_CATEGORYID_KEY, obj, obj2, obj3, obj4, obj5, language.Id);
            return await CacheManager.GetAsync(key, async delegate
            {
                List<int> categoryIds = new List<int>
                {
                    categoryId
                };
                if (includeProductsInSubcategories)
                {
                    List<int> collection = await CategoryService7Spikes.GetCategoryIdsByParentCategoryAsync(categoryId);
                    categoryIds.AddRange(collection);
                }
                IList<int> groupProductIds = await _productServiceNopAjaxFilters.GetAllGroupProductIdsInCategoriesAsync(categoryIds);
                IQueryable<Product> availableProductsQuery = await _aclHelper.GetAvailableProductsForCurrentCustomerAsync();
                availableProductsQuery = await _storeHelper.GetProductsForCurrentStoreAsync(availableProductsQuery);
                bool includeFeaturedProductsInNormalList = await GetIncludeFeaturedProductsInNormalListAsync();
                var query2 = from pvav in ProductAttributeValueRepository.Table
                             join pva in ProductAttributeMappingRepository.Table on pvav.ProductAttributeMappingId equals pva.Id
                             join pa in ProductAttributeRepository.Table on new
                             {
                                 ProductAttributeId1 = pva.ProductAttributeId,
                                 ProductAttributeId2 = pva.ProductAttributeId
                             } equals new
                             {
                                 ProductAttributeId1 = pa.Id,
                                 ProductAttributeId2 = productAttributeId
                             }
                             join p in availableProductsQuery on pva.ProductId equals p.Id
                             join pc in from x in ProductCategoryRepository.Table
                                        where categoryIds.Contains(x.CategoryId)
                                        select x on p.Id equals pc.ProductId into p_pc
                             from pc in p_pc.DefaultIfEmpty()
                             where (showHiddenProducts || p.Published) && !p.Deleted && ((pc != null && (includeFeaturedProductsInNormalList || !pc.IsFeaturedProduct) && (p.ParentGroupedProductId == 0 || p.VisibleIndividually)) || (pc == null && groupProductIds.Contains(p.ParentGroupedProductId))) && (!p.AvailableStartDateTimeUtc.HasValue || (DateTime)p.AvailableStartDateTimeUtc <= (DateTime)(DateTime?)nowUtc) && (!p.AvailableEndDateTimeUtc.HasValue || (DateTime)p.AvailableEndDateTimeUtc >= (DateTime)(DateTime?)nowUtc)
                             orderby pvav.DisplayOrder
                             select new
                             {
                                 ProductAttributeValue = pvav,
                                 LocalizedName = string.Empty
                             };
                new List<ProductAttributeValue>();
                IList<ProductAttributeValue> result;
                if ((await _languageService.GetAllLanguagesAsync()).Count >= 2)
                {
                    int workingLanguageId = (await _workContext.GetWorkingLanguageAsync()).Id;
                    query2 = from pvav in query2
                             join lp in from x in LocalizedPropertyRepository.Table
                                        where x.LocaleKeyGroup == "ProductAttributeValue" && x.LocaleKey == "Name" && x.LanguageId == workingLanguageId
                                        select x on pvav.ProductAttributeValue.Id equals lp.EntityId into pvav_lp
                             from lp in pvav_lp.DefaultIfEmpty()
                             select new
                             {
                                 ProductAttributeValue = pvav.ProductAttributeValue,
                                 LocalizedName = lp.LocaleValue
                             };
                    result = (await query2.ToListAsync()).Select(x =>
                    {
                        if (!string.IsNullOrEmpty(x.LocalizedName))
                        {
                            x.ProductAttributeValue.Name = x.LocalizedName;
                        }
                        return x.ProductAttributeValue;
                    }).ToList();
                }
                else
                {
                    result = await query2.Select(x => x.ProductAttributeValue).ToListAsync();
                }
                return result;
            });
        }

        private async Task<IList<ProductAttributeValue>> GetAllProductVariantAttributeValuesByProductAttributeIdAndManufacturerIdInternalAsync(int productAttributeId, int manufacturerId, bool showHiddenProducts)
        {
            if (productAttributeId == 0 || manufacturerId == 0)
            {
                return null;
            }
            DateTime nowUtc = DateTime.UtcNow;
            IStaticCacheManager cacheManager = CacheManager;
            CacheKey pRODUCTATTRIBUTEVALUES_BY_PRODUCTATTRIBUTEID_AND_MANUFACTURERID_KEY = PRODUCTATTRIBUTEVALUES_BY_PRODUCTATTRIBUTEID_AND_MANUFACTURERID_KEY;
            object obj = productAttributeId;
            object obj2 = manufacturerId;
            object obj3 = showHiddenProducts;
            object obj4 = (await _storeContext.GetCurrentStoreAsync()).Id;
            Language language = await _workContext.GetWorkingLanguageAsync();
            CacheKey key = cacheManager.PrepareKeyForDefaultCache(pRODUCTATTRIBUTEVALUES_BY_PRODUCTATTRIBUTEID_AND_MANUFACTURERID_KEY, obj, obj2, obj3, obj4, language.Id);
            return await CacheManager.GetAsync(key, async delegate
            {
                IList<int> groupProductIds = await _productServiceNopAjaxFilters.GetAllGroupProductIdsInManufacturerAsync(manufacturerId);
                IQueryable<Product> availableProductsQuery = await _aclHelper.GetAvailableProductsForCurrentCustomerAsync();
                availableProductsQuery = await _storeHelper.GetProductsForCurrentStoreAsync(availableProductsQuery);
                bool includeFeaturedProductsInNormalList = await GetIncludeFeaturedProductsInNormalListAsync();
                var query2 = from pvav in ProductAttributeValueRepository.Table
                             join pva in ProductAttributeMappingRepository.Table on pvav.ProductAttributeMappingId equals pva.Id
                             join pa in ProductAttributeRepository.Table on new
                             {
                                 ProductAttributeId1 = pva.ProductAttributeId,
                                 ProductAttributeId2 = pva.ProductAttributeId
                             } equals new
                             {
                                 ProductAttributeId1 = pa.Id,
                                 ProductAttributeId2 = productAttributeId
                             }
                             join p in availableProductsQuery on pva.ProductId equals p.Id
                             join pm in from x in _productManufacturerRepository.Table
                                        where x.ManufacturerId == manufacturerId
                                        select x on p.Id equals pm.ProductId into p_pm
                             from pm in p_pm.DefaultIfEmpty()
                             where (showHiddenProducts || p.Published) && !p.Deleted && ((pm != null && (includeFeaturedProductsInNormalList || !pm.IsFeaturedProduct) && (p.ParentGroupedProductId == 0 || p.VisibleIndividually)) || (pm == null && groupProductIds.Contains(p.ParentGroupedProductId))) && (!p.AvailableStartDateTimeUtc.HasValue || (DateTime)p.AvailableStartDateTimeUtc <= (DateTime)(DateTime?)nowUtc) && (!p.AvailableEndDateTimeUtc.HasValue || (DateTime)p.AvailableEndDateTimeUtc >= (DateTime)(DateTime?)nowUtc)
                             orderby pvav.DisplayOrder
                             select new
                             {
                                 ProductAttributeValue = pvav,
                                 LocalizedName = string.Empty
                             };
                new List<ProductAttributeValue>();
                IList<ProductAttributeValue> result;
                if ((await _languageService.GetAllLanguagesAsync()).Count >= 2)
                {
                    int workingLanguageId = (await _workContext.GetWorkingLanguageAsync()).Id;
                    query2 = from pvav in query2
                             join lp in from x in LocalizedPropertyRepository.Table
                                        where x.LocaleKeyGroup == "ProductAttributeValue" && x.LocaleKey == "Name" && x.LanguageId == workingLanguageId
                                        select x on pvav.ProductAttributeValue.Id equals lp.EntityId into pvav_lp
                             from lp in pvav_lp.DefaultIfEmpty()
                             select new
                             {
                                 ProductAttributeValue = pvav.ProductAttributeValue,
                                 LocalizedName = lp.LocaleValue
                             };
                    result = (await query2.ToListAsync()).Select(x =>
                    {
                        if (!string.IsNullOrEmpty(x.LocalizedName))
                        {
                            x.ProductAttributeValue.Name = x.LocalizedName;
                        }
                        return x.ProductAttributeValue;
                    }).ToList();
                }
                else
                {
                    result = await query2.Select(x => x.ProductAttributeValue).ToListAsync();
                }
                return result;
            });
        }

        private async Task<IList<ProductAttributeValue>> GetAllProductVariantAttributeValuesByProductAttributeIdAndVendorIdInternalAsync(int productAttributeId, int vendorId, bool showHiddenProducts)
        {
            if (productAttributeId == 0 || vendorId == 0)
            {
                return null;
            }
            DateTime nowUtc = DateTime.UtcNow;
            IStaticCacheManager cacheManager = CacheManager;
            CacheKey pRODUCTATTRIBUTEVALUES_BY_PRODUCTATTRIBUTEID_AND_VENDORID_KEY = PRODUCTATTRIBUTEVALUES_BY_PRODUCTATTRIBUTEID_AND_VENDORID_KEY;
            object obj = productAttributeId;
            object obj2 = vendorId;
            object obj3 = showHiddenProducts;
            object obj4 = (await _storeContext.GetCurrentStoreAsync()).Id;
            Language language = await _workContext.GetWorkingLanguageAsync();
            CacheKey key = cacheManager.PrepareKeyForDefaultCache(pRODUCTATTRIBUTEVALUES_BY_PRODUCTATTRIBUTEID_AND_VENDORID_KEY, obj, obj2, obj3, obj4, language.Id);
            return await CacheManager.GetAsync(key, async delegate
            {
                IList<int> groupProductIds = await _productServiceNopAjaxFilters.GetAllGroupProductIdsInVendorAsync(vendorId);
                IQueryable<Product> productsQuery = await _aclHelper.GetAvailableProductsForCurrentCustomerAsync();
                productsQuery = await _storeHelper.GetProductsForCurrentStoreAsync(productsQuery);
                var query = from pvav in ProductAttributeValueRepository.Table
                            join pva in ProductAttributeMappingRepository.Table on pvav.ProductAttributeMappingId equals pva.Id
                            join pa in ProductAttributeRepository.Table on new
                            {
                                ProductAttributeId1 = pva.ProductAttributeId,
                                ProductAttributeId2 = pva.ProductAttributeId
                            } equals new
                            {
                                ProductAttributeId1 = pa.Id,
                                ProductAttributeId2 = productAttributeId
                            }
                            join p in productsQuery on pva.ProductId equals p.Id
                            join v in from x in _vendorRepository.Table
                                      where x.Id == vendorId
                                      select x on p.VendorId equals v.Id into p_pv
                            from v in p_pv.DefaultIfEmpty()
                            where (showHiddenProducts || p.Published) && !p.Deleted && ((v != null && (p.ParentGroupedProductId == 0 || p.VisibleIndividually)) || (v == null && groupProductIds.Contains(p.ParentGroupedProductId))) && (!p.AvailableStartDateTimeUtc.HasValue || (DateTime)p.AvailableStartDateTimeUtc <= (DateTime)(DateTime?)nowUtc) && (!p.AvailableEndDateTimeUtc.HasValue || (DateTime)p.AvailableEndDateTimeUtc >= (DateTime)(DateTime?)nowUtc)
                            orderby pvav.DisplayOrder
                            select new
                            {
                                ProductAttributeValue = pvav,
                                LocalizedName = string.Empty
                            };
                new List<ProductAttributeValue>();
                IList<ProductAttributeValue> result;
                if ((await _languageService.GetAllLanguagesAsync()).Count >= 2)
                {
                    int workingLanguageId = (await _workContext.GetWorkingLanguageAsync()).Id;
                    query = from pvav in query
                            join lp in from x in LocalizedPropertyRepository.Table
                                       where x.LocaleKeyGroup == "ProductAttributeValue" && x.LocaleKey == "Name" && x.LanguageId == workingLanguageId
                                       select x on pvav.ProductAttributeValue.Id equals lp.EntityId into pvav_lp
                            from lp in pvav_lp.DefaultIfEmpty()
                            select new
                            {
                                ProductAttributeValue = pvav.ProductAttributeValue,
                                LocalizedName = lp.LocaleValue
                            };
                    result = (await query.ToListAsync()).Select(x =>
                    {
                        if (!string.IsNullOrEmpty(x.LocalizedName))
                        {
                            x.ProductAttributeValue.Name = x.LocalizedName;
                        }
                        return x.ProductAttributeValue;
                    }).ToList();
                }
                else
                {
                    result = await query.Select(x => x.ProductAttributeValue).ToListAsync();
                }
                return result;
            });
        }

        private async Task<IList<ProductAttributeMapping>> GetAllProductVariantAttributesByProductIdInternalAsync(int productId, bool showHiddenProducts)
        {
            if (productId == 0)
            {
                return null;
            }
            IStaticCacheManager cacheManager = CacheManager;
            CacheKey pRODUCTVARIANTATTRIBUTES_BY_PRODUCTID_KEY = PRODUCTVARIANTATTRIBUTES_BY_PRODUCTID_KEY;
            object obj = productId;
            Store store = await _storeContext.GetCurrentStoreAsync();
            CacheKey key = cacheManager.PrepareKeyForDefaultCache(pRODUCTVARIANTATTRIBUTES_BY_PRODUCTID_KEY, obj, store.Id);
            return new List<ProductAttributeMapping>(await CacheManager.GetAsync(key, (Func<Task<IEnumerable<ProductAttributeMapping>>>)async delegate
            {
                DateTime nowUtc = DateTime.UtcNow;
                IQueryable<Product> inner = await _aclHelper.GetAvailableProductsForCurrentCustomerAsync();
                return await (from pva in ProductAttributeMappingRepository.Table
                              join pa in ProductAttributeRepository.Table on pva.ProductAttributeId equals pa.Id
                              join p in inner on pva.ProductId equals p.Id
                              where p.Id == productId && (showHiddenProducts || p.Published) && !p.Deleted && (!p.AvailableStartDateTimeUtc.HasValue || (DateTime)p.AvailableStartDateTimeUtc <= (DateTime)(DateTime?)nowUtc) && (!p.AvailableEndDateTimeUtc.HasValue || (DateTime)p.AvailableEndDateTimeUtc >= (DateTime)(DateTime?)nowUtc)
                              select pva).ToListAsync();
            }));
        }

        private async Task<IList<ProductAttributeMapping>> GetAllProductVariantAttributesWhichHaveValuesByProductIdInternalAsync(int productId, bool showHiddenProducts)
        {
            if (productId == 0)
            {
                return null;
            }
            IStaticCacheManager cacheManager = CacheManager;
            CacheKey pRODUCTVARIANTATTRIBUTES_WHICH_HAVE_VALUES_BY_PRODUCTID_KEY = PRODUCTVARIANTATTRIBUTES_WHICH_HAVE_VALUES_BY_PRODUCTID_KEY;
            object obj = productId;
            Store store = await _storeContext.GetCurrentStoreAsync();
            CacheKey key = cacheManager.PrepareKeyForDefaultCache(pRODUCTVARIANTATTRIBUTES_WHICH_HAVE_VALUES_BY_PRODUCTID_KEY, obj, store.Id);
            return new List<ProductAttributeMapping>(await CacheManager.GetAsync(key, (Func<Task<IEnumerable<ProductAttributeMapping>>>)async delegate
            {
                DateTime nowUtc = DateTime.UtcNow;
                IQueryable<Product> inner = await _aclHelper.GetAvailableProductsForCurrentCustomerAsync();
                return await (from pva in ProductAttributeMappingRepository.Table
                              join pa in ProductAttributeRepository.Table on pva.ProductAttributeId equals pa.Id
                              join pvav in ProductAttributeValueRepository.Table on pva.Id equals pvav.ProductAttributeMappingId
                              join p in inner on pva.ProductId equals p.Id
                              where p.Id == productId && (showHiddenProducts || p.Published) && !p.Deleted && (!p.AvailableStartDateTimeUtc.HasValue || (DateTime)p.AvailableStartDateTimeUtc <= (DateTime)(DateTime?)nowUtc) && (!p.AvailableEndDateTimeUtc.HasValue || (DateTime)p.AvailableEndDateTimeUtc >= (DateTime)(DateTime?)nowUtc)
                              select pva).ToListAsync();
            }));
        }

        private async Task<IList<ProductAttributeMapping>> GetAllProductVariantAttributesWhichHaveValuesByProductIdsInternalAsync(IList<int> productIds, bool showHiddenProducts)
        {
            if (productIds == null || productIds.Count == 0)
            {
                return null;
            }
            CacheKey key = CacheManager.PrepareKeyForDefaultCache(PRODUCTVARIANTATTRIBUTES_WHICH_HAVE_VALUES_BY_PRODUCTIDS_KEY, productIds, _storeContext.GetCurrentStoreAsync().Id);
            return (await CacheManager.GetAsync(key, (Func<Task<IEnumerable<ProductAttributeMapping>>>)async delegate
            {
                DateTime nowUtc = DateTime.UtcNow;
                List<int> productIdsArray = productIds.ToList();
                IQueryable<Product> inner = await _aclHelper.GetAvailableProductsForCurrentCustomerAsync();
                return await (from pva in ProductAttributeMappingRepository.Table
                              join pa in ProductAttributeRepository.Table on pva.ProductAttributeId equals pa.Id
                              join pvav in ProductAttributeValueRepository.Table on pva.Id equals pvav.ProductAttributeMappingId
                              join p in inner on pva.ProductId equals p.Id
                              where productIdsArray.Contains(p.Id) && (showHiddenProducts || p.Published) && !p.Deleted && (!p.AvailableStartDateTimeUtc.HasValue || (DateTime)p.AvailableStartDateTimeUtc <= (DateTime)(DateTime?)nowUtc) && (!p.AvailableEndDateTimeUtc.HasValue || (DateTime)p.AvailableEndDateTimeUtc >= (DateTime)(DateTime?)nowUtc)
                              select pva).ToListAsync();
            })).ToList();
        }

        private async Task<IList<ProductAttributeProductAttributeValueDTO>> GetProductAttributeProductAttributeValueDtosByProductAttributeMappingIdsInternalAsync(IList<int> productAttributeMappingIds)
        {
            if (productAttributeMappingIds == null || !productAttributeMappingIds.Any())
            {
                return null;
            }
            IStaticCacheManager cacheManager = CacheManager;
            CacheKey pRODUCTATTRIBUTEVALUES_AND_PRODUCT_ATTRIBUTES_BY_PRODUCTATTRIBUTEMAPPINGS_KEY = PRODUCTATTRIBUTEVALUES_AND_PRODUCT_ATTRIBUTES_BY_PRODUCTATTRIBUTEMAPPINGS_KEY;
            object obj = productAttributeMappingIds;
            object obj2 = (await _storeContext.GetCurrentStoreAsync()).Id;
            Language language = await _workContext.GetWorkingLanguageAsync();
            CacheKey key = cacheManager.PrepareKeyForDefaultCache(pRODUCTATTRIBUTEVALUES_AND_PRODUCT_ATTRIBUTES_BY_PRODUCTATTRIBUTEMAPPINGS_KEY, obj, obj2, language.Id);
            return await CacheManager.GetAsync(key, async delegate
            {
                new List<ProductAttributeProductAttributeValueDTO>();
                IQueryable<ProductAttributeProductAttributeValueDTO> query = from pvav in ProductAttributeValueRepository.Table
                                                                             join pva in ProductAttributeMappingRepository.Table on pvav.ProductAttributeMappingId equals pva.Id
                                                                             where productAttributeMappingIds.Contains(pva.Id)
                                                                             orderby pvav.DisplayOrder
                                                                             select new ProductAttributeProductAttributeValueDTO
                                                                             {
                                                                                 ProductAttributeValue = pvav,
                                                                                 ProductAttributeId = pva.ProductAttributeId,
                                                                                 LocalizedName = string.Empty
                                                                             };
                IList<ProductAttributeProductAttributeValueDTO> result;
                if ((await _languageService.GetAllLanguagesAsync()).Count >= 2)
                {
                    int workingLanguageId = (await _workContext.GetWorkingLanguageAsync()).Id;
                    query = from pvav in query
                            join lp in from x in LocalizedPropertyRepository.Table
                                       where x.LocaleKeyGroup == "ProductAttributeValue" && x.LocaleKey == "Name" && x.LanguageId == workingLanguageId
                                       select x on pvav.ProductAttributeValue.Id equals lp.EntityId into pvav_lp
                            from lp in pvav_lp.DefaultIfEmpty()
                            select new ProductAttributeProductAttributeValueDTO
                            {
                                ProductAttributeValue = pvav.ProductAttributeValue,
                                ProductAttributeId = pvav.ProductAttributeId,
                                LocalizedName = lp.LocaleValue
                            };
                    result = (await query.ToListAsync()).Select(delegate (ProductAttributeProductAttributeValueDTO x)
                    {
                        if (!string.IsNullOrEmpty(x.LocalizedName))
                        {
                            x.ProductAttributeValue.Name = x.LocalizedName;
                        }
                        return x;
                    }).ToList();
                }
                else
                {
                    result = await query.ToListAsync();
                }
                return result;
            });
        }

        private async Task<bool> GetIncludeFeaturedProductsInNormalListAsync()
        {
            return await SettingService.GetSettingByKeyAsync("catalogsettings.includefeaturedproductsinnormallists", defaultValue: false);
        }
    }
}