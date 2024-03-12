using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Configuration;
using SevenSpikes.Nop.Services.Catalog;
using SevenSpikes.Nop.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Plugin.Intelisale.AjaxFilters.Services
{
    public class SpecificationAttributeService7Spikes : ISpecificationAttributeService7Spikes
    {
        private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
        private readonly IAclHelper _aclHelper;
        private readonly IStoreContext _storeContext;
        private readonly IStoreHelper _storeHelper;

        private CacheKey SPECIFICATIONATTRIBUTEOPTIONS_DICTIONARY_KEY => new CacheKey("Nop.specificationattributeoptions.dictionary.store.id-{0}");

        private CacheKey SPECIFICATIONATTRIBUTEOPTIONS_BY_CATEGORYID_KEY => new CacheKey("Nop.specificationattributeoptions.categoryid-{0}.includeproductsinsubcategories-{1}.showhidden-{2}.store.id-{3}");

        private CacheKey SPECIFICATIONATTRIBUTEOPTIONS_BY_MANUFACTURERID_KEY => new CacheKey("Nop.specificationattributeoptions.manufacturerid-{0}.showhidden-{1}.store.id-{2}");

        private CacheKey SPECIFICATIONATTRIBUTEOPTIONS_BY_VENDORID_KEY => new CacheKey("Nop.specificationattributeoptions.vendorid-{0}.showhidden-{1}.store.id-{2}");

        private CacheKey SPECIFICATIONATTRIBUTEOPTIONS_BY_IDS_KEY => new CacheKey("Nop.specificationattributeoptions.ids-{0}.store.id-{1}");

        private CacheKey SPECIFICATIONATTRIBUTEOPTIONS_BY_PRODUCTID_KEY => new CacheKey("Nop.specificationattributeoptions.productid-{0}.store.id-{1}");

        private CacheKey SPECIFICATIONATTRIBUTEOPTIONS_BY_PRODUCTIDS_KEY => new CacheKey("Nop.specificationattributeoptions.productids-{0}.store.id-{1}");

        private CacheKey SPECIFICATIONATTRIBUTEOPTIONS_ALL_KEY => new CacheKey("Nop.specificationattributeoptions.all.store.id-{0}");

        private CacheKey SPECIFICATIONATTRIBUTEOPTIONS_BY_IDS_AND_SPECIFICATIONID_KEY => new CacheKey("Nop.specificationattributeoptions.ids-{0}.specification.id-{1}.store.id-{2}");

        private IRepository<ProductCategory> ProductCategoryRepository { get; set; }
        private IRepository<ProductSpecificationAttribute> ProductSpecificationAttributeRepository { get; set; }
        private IRepository<SpecificationAttribute> SpecificationAttributeRepository { get; set; }
        private IRepository<SpecificationAttributeOption> SpecificationAttributeOptionRepository { get; set; }
        private ICategoryService7Spikes CategoryService7Spikes { get; set; }
        private ISettingService SettingService { get; set; }
        private IStaticCacheManager CacheManager { get; set; }

        public SpecificationAttributeService7Spikes(
            IRepository<ProductManufacturer> productManufacturerRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository,
            IRepository<SpecificationAttribute> specificationAttributeRepository,
            IRepository<SpecificationAttributeOption> specificationAttributeOptionFilterRepository,
            ISettingService settingService,
            IStaticCacheManager cacheManager,
            IAclHelper aclHelper,
            IStoreContext storeContext,
            IStoreHelper storeHelper,
            ICategoryService7Spikes categoryService7Spikes
            )
        {
            _productManufacturerRepository = productManufacturerRepository;
            _aclHelper = aclHelper;
            _storeContext = storeContext;
            _storeHelper = storeHelper;
            ProductCategoryRepository = productCategoryRepository;
            ProductSpecificationAttributeRepository = productSpecificationAttributeRepository;
            SpecificationAttributeRepository = specificationAttributeRepository;
            SpecificationAttributeOptionRepository = specificationAttributeOptionFilterRepository;
            CategoryService7Spikes = categoryService7Spikes;
            SettingService = settingService;
            CacheManager = cacheManager;
        }

        public virtual async Task<IList<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByIdsAndSpecificationIdAsync(IList<int> specificationAttributeOptionIds, int specificationId)
        {
            return await GetSpecificationAttributeOptionsByIdsAndSpecificationIdInternalAsync(specificationAttributeOptionIds, specificationId);
        }

        public virtual async Task<IList<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByCategoryIdAsync(int categoryId, bool includeProductsInSubcategories = false, bool showHiddenProducts = false)
        {
            return await GetSpecificationAttributeOptionsByCategoryIdInternalAsync(categoryId, includeProductsInSubcategories, showHiddenProducts);
        }

        public async Task<IList<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByManufacturerIdAsync(int manufacturerId, bool showHiddenProducts = false)
        {
            return await GetSpecificationAttributeOptionsByManufacturerIdInternalAsync(manufacturerId, showHiddenProducts);
        }

        public async Task<IList<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByVendorIdAsync(int vendorId, bool showHiddenProducts = false)
        {
            return await GetSpecificationAttributeOptionsByVendorIdInternalAsync(vendorId, showHiddenProducts);
        }

        public virtual async Task<IList<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByIdsAsync(IList<int> specificationAttributeOptionIds)
        {
            return await GetSpecificationAttributeOptionsByIdsInternalAsync(specificationAttributeOptionIds);
        }

        public virtual async Task<IList<SpecificationAttributeOption>> GetAllSpecificationAttributeOptionsAsync(bool includeNotAllowFilteringOptions = false)
        {
            return await GetAllSpecificationAttributeOptionsInternalAsync(includeNotAllowFilteringOptions);
        }

        public virtual async Task<IList<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByProductIdAsync(int productId, bool includeNotAllowFilteringOptions = false)
        {
            return await GetSpecificationAttributeOptionsByProductIdInternalAsync(productId, includeNotAllowFilteringOptions);
        }

        public virtual async Task<IEnumerable<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByProductIdsAsync(IList<int> productIds, bool showHiddenProducts = false)
        {
            return await GetSpecificationAttributeOptionsByProductIdsInternalAsync(productIds, showHiddenProducts);
        }

        public virtual async Task<IDictionary<int, IList<int>>> GetSpecificationAttributeOptionsDictionaryForProductsAsync(IQueryable<Product> products)
        {
            return await GetSpecificationAttributeOptionsDictionaryForProductsInternalAsync(products);
        }

        public virtual async Task<IDictionary<int, IList<int>>> GetSpecificationAttributeOptionsDictionaryAsync()
        {
            return await GetSpecificationAttributeOptionsDictionaryInternalAsync();
        }

        private async Task<IList<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByIdsAndSpecificationIdInternalAsync(IList<int> specificationAttributeOptionIds, int specificationId)
        {
            if (specificationAttributeOptionIds == null || specificationAttributeOptionIds.Count == 0)
            {
                return null;
            }
            IStaticCacheManager cacheManager = CacheManager;
            CacheKey sPECIFICATIONATTRIBUTEOPTIONS_BY_IDS_AND_SPECIFICATIONID_KEY = SPECIFICATIONATTRIBUTEOPTIONS_BY_IDS_AND_SPECIFICATIONID_KEY;
            object obj = specificationAttributeOptionIds;
            object obj2 = specificationId;
            Store store = await _storeContext.GetCurrentStoreAsync();
            CacheKey key = cacheManager.PrepareKeyForDefaultCache(sPECIFICATIONATTRIBUTEOPTIONS_BY_IDS_AND_SPECIFICATIONID_KEY, obj, obj2, store.Id);
            return new List<SpecificationAttributeOption>(await CacheManager.GetAsync(key, (Func<Task<IEnumerable<SpecificationAttributeOption>>>)(async () => await SpecificationAttributeOptionRepository.Table.Where((SpecificationAttributeOption sao) => specificationAttributeOptionIds.Contains(sao.Id) && sao.SpecificationAttributeId == specificationId).ToListAsync())));
        }

        private async Task<IList<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByCategoryIdInternalAsync(int categoryId, bool includeProductsInSubcategories, bool showHiddenProducts)
        {
            if (categoryId == 0)
            {
                return null;
            }
            DateTime nowUtc = DateTime.UtcNow;
            IStaticCacheManager cacheManager = CacheManager;
            CacheKey sPECIFICATIONATTRIBUTEOPTIONS_BY_CATEGORYID_KEY = SPECIFICATIONATTRIBUTEOPTIONS_BY_CATEGORYID_KEY;
            object obj = categoryId;
            object obj2 = includeProductsInSubcategories;
            object obj3 = showHiddenProducts;
            Store store = await _storeContext.GetCurrentStoreAsync();
            CacheKey key = cacheManager.PrepareKeyForDefaultCache(sPECIFICATIONATTRIBUTEOPTIONS_BY_CATEGORYID_KEY, obj, obj2, obj3, store.Id);
            return (await CacheManager.GetAsync(key, (Func<Task<IEnumerable<SpecificationAttributeOption>>>)async delegate
            {
                List<int> subCategoryIds = new List<int>();
                if (includeProductsInSubcategories)
                {
                    subCategoryIds = await CategoryService7Spikes.GetCategoryIdsByParentCategoryAsync(categoryId);
                }
                IQueryable<Product> availableProductsQuery = await _aclHelper.GetAvailableProductsForCurrentCustomerAsync();
                availableProductsQuery = await _storeHelper.GetProductsForCurrentStoreAsync(availableProductsQuery);
                bool includeFeaturedProductsInNormalList = await GetIncludeFeaturedProductsInNormalListAsync();
                return await (from saof in SpecificationAttributeOptionRepository.Table
                              join psa in ProductSpecificationAttributeRepository.Table on saof.Id equals psa.SpecificationAttributeOptionId
                              join sa in SpecificationAttributeRepository.Table on saof.SpecificationAttributeId equals sa.Id
                              join p in availableProductsQuery on psa.ProductId equals p.Id
                              join pc in ProductCategoryRepository.Table on p.Id equals pc.ProductId
                              where (pc.CategoryId == categoryId || (includeProductsInSubcategories && subCategoryIds.Contains(pc.CategoryId))) && (includeFeaturedProductsInNormalList || !pc.IsFeaturedProduct) && (p.ParentGroupedProductId == 0 || p.VisibleIndividually) && (showHiddenProducts || p.Published) && !p.Deleted && (!p.AvailableStartDateTimeUtc.HasValue || (DateTime)p.AvailableStartDateTimeUtc <= (DateTime)(DateTime?)nowUtc) && (!p.AvailableEndDateTimeUtc.HasValue || (DateTime)p.AvailableEndDateTimeUtc >= (DateTime)(DateTime?)nowUtc) && psa.AllowFiltering
                              select saof).Distinct().ToListAsync();
            })).ToList().ToList();
        }

        private async Task<IList<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByManufacturerIdInternalAsync(int manufacturerId, bool showHiddenProducts)
        {
            if (manufacturerId == 0)
            {
                return null;
            }
            DateTime nowUtc = DateTime.UtcNow;
            IStaticCacheManager cacheManager = CacheManager;
            CacheKey sPECIFICATIONATTRIBUTEOPTIONS_BY_MANUFACTURERID_KEY = SPECIFICATIONATTRIBUTEOPTIONS_BY_MANUFACTURERID_KEY;
            object obj = manufacturerId;
            object obj2 = showHiddenProducts;
            Store store = await _storeContext.GetCurrentStoreAsync();
            CacheKey key = cacheManager.PrepareKeyForDefaultCache(sPECIFICATIONATTRIBUTEOPTIONS_BY_MANUFACTURERID_KEY, obj, obj2, store.Id);
            return (await CacheManager.GetAsync(key, (Func<Task<IEnumerable<SpecificationAttributeOption>>>)async delegate
            {
                IQueryable<Product> availableProductsQuery = await _aclHelper.GetAvailableProductsForCurrentCustomerAsync();
                availableProductsQuery = await _storeHelper.GetProductsForCurrentStoreAsync(availableProductsQuery);
                bool includeFeaturedProductsInNormalList = await GetIncludeFeaturedProductsInNormalListAsync();
                return await (from saof in SpecificationAttributeOptionRepository.Table
                              join psa in ProductSpecificationAttributeRepository.Table on saof.Id equals psa.SpecificationAttributeOptionId
                              join sa in SpecificationAttributeRepository.Table on saof.SpecificationAttributeId equals sa.Id
                              join p in availableProductsQuery on psa.ProductId equals p.Id
                              join pm in _productManufacturerRepository.Table on p.Id equals pm.ProductId
                              where pm.ManufacturerId == manufacturerId && (includeFeaturedProductsInNormalList || !pm.IsFeaturedProduct) && (p.ParentGroupedProductId == 0 || p.VisibleIndividually) && (showHiddenProducts || p.Published) && !p.Deleted && (!p.AvailableStartDateTimeUtc.HasValue || (DateTime)p.AvailableStartDateTimeUtc <= (DateTime)(DateTime?)nowUtc) && (!p.AvailableEndDateTimeUtc.HasValue || (DateTime)p.AvailableEndDateTimeUtc >= (DateTime)(DateTime?)nowUtc) && psa.AllowFiltering
                              select saof).Distinct().ToListAsync();
            })).ToList();
        }

        private async Task<IList<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByVendorIdInternalAsync(int vendorId, bool showHiddenProducts)
        {
            if (vendorId == 0)
            {
                return null;
            }
            DateTime nowUtc = DateTime.UtcNow;
            IStaticCacheManager cacheManager = CacheManager;
            CacheKey sPECIFICATIONATTRIBUTEOPTIONS_BY_VENDORID_KEY = SPECIFICATIONATTRIBUTEOPTIONS_BY_VENDORID_KEY;
            object obj = vendorId;
            object obj2 = showHiddenProducts;
            Store store = await _storeContext.GetCurrentStoreAsync();
            CacheKey key = cacheManager.PrepareKeyForDefaultCache(sPECIFICATIONATTRIBUTEOPTIONS_BY_VENDORID_KEY, obj, obj2, store.Id);
            return (await CacheManager.GetAsync(key, (Func<Task<IEnumerable<SpecificationAttributeOption>>>)async delegate
            {
                IQueryable<Product> productsQuery = await _aclHelper.GetAvailableProductsForCurrentCustomerAsync();
                productsQuery = await _storeHelper.GetProductsForCurrentStoreAsync(productsQuery);
                return await (from sao in SpecificationAttributeOptionRepository.Table
                              join psa in ProductSpecificationAttributeRepository.Table on sao.Id equals psa.SpecificationAttributeOptionId
                              join p in productsQuery on psa.ProductId equals p.Id
                              where p.VendorId == vendorId && (p.ParentGroupedProductId == 0 || p.VisibleIndividually) && (showHiddenProducts || p.Published) && !p.Deleted && (!p.AvailableStartDateTimeUtc.HasValue || (DateTime)p.AvailableStartDateTimeUtc <= (DateTime)(DateTime?)nowUtc) && (!p.AvailableEndDateTimeUtc.HasValue || (DateTime)p.AvailableEndDateTimeUtc >= (DateTime)(DateTime?)nowUtc) && psa.AllowFiltering
                              select sao).Distinct().ToListAsync();
            })).ToList();
        }

        private async Task<IList<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByIdsInternalAsync(IList<int> specificationAttributeOptionIds)
        {
            if (specificationAttributeOptionIds == null || specificationAttributeOptionIds.Count == 0)
            {
                return null;
            }
            IStaticCacheManager cacheManager = CacheManager;
            CacheKey sPECIFICATIONATTRIBUTEOPTIONS_BY_IDS_KEY = SPECIFICATIONATTRIBUTEOPTIONS_BY_IDS_KEY;
            object obj = specificationAttributeOptionIds;
            Store store = await _storeContext.GetCurrentStoreAsync();
            CacheKey key = cacheManager.PrepareKeyForDefaultCache(sPECIFICATIONATTRIBUTEOPTIONS_BY_IDS_KEY, obj, store.Id);
            return new List<SpecificationAttributeOption>(await CacheManager.GetAsync(key, (Func<Task<IEnumerable<SpecificationAttributeOption>>>)async delegate
            {
                List<int> specificationAttributeOptionIdsArray = specificationAttributeOptionIds.ToList();
                return await (from sao in SpecificationAttributeOptionRepository.Table
                              join psa in ProductSpecificationAttributeRepository.Table on sao.Id equals psa.SpecificationAttributeOptionId
                              where psa.AllowFiltering && specificationAttributeOptionIdsArray.Contains(sao.Id)
                              select sao).Distinct().ToListAsync();
            }));
        }

        private async Task<IList<SpecificationAttributeOption>> GetAllSpecificationAttributeOptionsInternalAsync(bool includeNotAllowFilteringOptions)
        {
            IStaticCacheManager cacheManager = CacheManager;
            CacheKey sPECIFICATIONATTRIBUTEOPTIONS_ALL_KEY = SPECIFICATIONATTRIBUTEOPTIONS_ALL_KEY;
            Store store = await _storeContext.GetCurrentStoreAsync();
            CacheKey key = cacheManager.PrepareKeyForDefaultCache(sPECIFICATIONATTRIBUTEOPTIONS_ALL_KEY, store.Id);
            return new List<SpecificationAttributeOption>(await CacheManager.GetAsync(key, (Func<Task<IEnumerable<SpecificationAttributeOption>>>)(async () => await (from sao in SpecificationAttributeOptionRepository.Table
                                                                                                                                                                      join psa in ProductSpecificationAttributeRepository.Table on sao.Id equals psa.SpecificationAttributeOptionId
                                                                                                                                                                      where includeNotAllowFilteringOptions || psa.AllowFiltering
                                                                                                                                                                      select sao).Distinct().ToListAsync())));
        }

        private async Task<IList<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByProductIdInternalAsync(int productId, bool includeNotAllowFilteringOptions)
        {
            if (productId == 0)
            {
                return null;
            }
            IStaticCacheManager cacheManager = CacheManager;
            CacheKey sPECIFICATIONATTRIBUTEOPTIONS_BY_PRODUCTID_KEY = SPECIFICATIONATTRIBUTEOPTIONS_BY_PRODUCTID_KEY;
            object obj = productId;
            Store store = await _storeContext.GetCurrentStoreAsync();
            CacheKey key = cacheManager.PrepareKeyForDefaultCache(sPECIFICATIONATTRIBUTEOPTIONS_BY_PRODUCTID_KEY, obj, store.Id);
            return new List<SpecificationAttributeOption>(await CacheManager.GetAsync(key, (Func<Task<IEnumerable<SpecificationAttributeOption>>>)async delegate
            {
                IQueryable<Product> inner = await _aclHelper.GetAvailableProductsForCurrentCustomerAsync();
                return await (from sao in SpecificationAttributeOptionRepository.Table
                              join psa in ProductSpecificationAttributeRepository.Table on sao.Id equals psa.SpecificationAttributeOptionId
                              join p in inner on psa.ProductId equals p.Id
                              where (includeNotAllowFilteringOptions || psa.AllowFiltering) && p.Id == productId
                              select sao).ToListAsync();
            }));
        }

        private async Task<IEnumerable<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByProductIdsInternalAsync(IList<int> productIds, bool showHiddenProducts)
        {
            if (productIds == null || productIds.Count == 0)
            {
                return null;
            }
            IStaticCacheManager cacheManager = CacheManager;
            CacheKey sPECIFICATIONATTRIBUTEOPTIONS_BY_PRODUCTIDS_KEY = SPECIFICATIONATTRIBUTEOPTIONS_BY_PRODUCTIDS_KEY;
            object obj = productIds;
            Store store = await _storeContext.GetCurrentStoreAsync();
            CacheKey key = cacheManager.PrepareKeyForDefaultCache(sPECIFICATIONATTRIBUTEOPTIONS_BY_PRODUCTIDS_KEY, obj, store);
            return await CacheManager.GetAsync(key, (Func<Task<IEnumerable<SpecificationAttributeOption>>>)async delegate
            {
                DateTime nowUtc = DateTime.UtcNow;
                List<int> productIdsArray = productIds.ToList();
                IQueryable<Product> inner = await _aclHelper.GetAvailableProductsForCurrentCustomerAsync();
                return await (from x in (from saof in SpecificationAttributeOptionRepository.Table
                                         join psa in ProductSpecificationAttributeRepository.Table on saof.Id equals psa.SpecificationAttributeOptionId
                                         join p in inner on psa.ProductId equals p.Id
                                         where productIdsArray.Contains(p.Id) && (showHiddenProducts || p.Published) && !p.Deleted && (!p.AvailableStartDateTimeUtc.HasValue || (DateTime)p.AvailableStartDateTimeUtc <= (DateTime)(DateTime?)nowUtc) && (!p.AvailableEndDateTimeUtc.HasValue || (DateTime)p.AvailableEndDateTimeUtc >= (DateTime)(DateTime?)nowUtc) && psa.AllowFiltering
                                         select saof).Distinct()
                              orderby x.DisplayOrder
                              select x).ToListAsync();
            });
        }

        private async Task<IDictionary<int, IList<int>>> GetSpecificationAttributeOptionsDictionaryForProductsInternalAsync(IQueryable<Product> products)
        {
            if (products == null || !products.Any())
            {
                return null;
            }
            List<int> productIds = products.Select((Product x) => x.Id).ToList();
            IQueryable<Product> inner = await _aclHelper.GetAvailableProductsForCurrentCustomerAsync();
            var obj = await (from saof in SpecificationAttributeOptionRepository.Table
                             join psa in ProductSpecificationAttributeRepository.Table on saof.Id equals psa.SpecificationAttributeOptionId
                             join p in inner on psa.ProductId equals p.Id
                             where productIds.Contains(p.Id) && psa.AllowFiltering
                             select new
                             {
                                 ProductId = p.Id,
                                 SpecificationAttributeOptionId = saof.Id
                             } into x
                             orderby x.ProductId
                             select x).ToListAsync();
            Dictionary<int, IList<int>> dictionary = new Dictionary<int, IList<int>>();
            int num = 0;
            List<int> list = null;
            foreach (var item in obj)
            {
                if (item.ProductId != num)
                {
                    list = new List<int>();
                    dictionary.Add(item.ProductId, list);
                    list.Add(item.SpecificationAttributeOptionId);
                    num = item.ProductId;
                }
                else
                {
                    list.Add(item.SpecificationAttributeOptionId);
                }
            }
            return dictionary;
        }

        private async Task<IDictionary<int, IList<int>>> GetSpecificationAttributeOptionsDictionaryInternalAsync()
        {
            IStaticCacheManager cacheManager = CacheManager;
            IStaticCacheManager cacheManager2 = CacheManager;
            CacheKey sPECIFICATIONATTRIBUTEOPTIONS_DICTIONARY_KEY = SPECIFICATIONATTRIBUTEOPTIONS_DICTIONARY_KEY;
            Store store = await _storeContext.GetCurrentStoreAsync();
            var obj = await cacheManager.GetAsync(cacheManager2.PrepareKeyForDefaultCache(sPECIFICATIONATTRIBUTEOPTIONS_DICTIONARY_KEY, store), async () => await (from x in (from saof in SpecificationAttributeOptionRepository.Table
                                                                                                                                                                              join sa in SpecificationAttributeRepository.Table on saof.SpecificationAttributeId equals sa.Id
                                                                                                                                                                              join psa in ProductSpecificationAttributeRepository.Table on saof.Id equals psa.SpecificationAttributeOptionId
                                                                                                                                                                              where psa.AllowFiltering
                                                                                                                                                                              select new
                                                                                                                                                                              {
                                                                                                                                                                                  SpecificationAttributeId = sa.Id,
                                                                                                                                                                                  SpecificationAttributeOptionId = saof.Id
                                                                                                                                                                              }).Distinct()
                                                                                                                                                                   orderby x.SpecificationAttributeId
                                                                                                                                                                   select x).ToListAsync());
            Dictionary<int, IList<int>> dictionary = new Dictionary<int, IList<int>>();
            int num = 0;
            List<int> list = null;
            foreach (var item in obj)
            {
                if (item.SpecificationAttributeId != num)
                {
                    if (list != null)
                    {
                        foreach (int item2 in list)
                        {
                            dictionary.Add(item2, list);
                        }
                    }
                    num = item.SpecificationAttributeId;
                    list = new List<int>
                    {
                        item.SpecificationAttributeOptionId
                    };
                }
                else
                {
                    list.Add(item.SpecificationAttributeOptionId);
                }
            }
            foreach (int item3 in list)
            {
                dictionary.Add(item3, list);
            }
            return dictionary;
        }

        private async Task<bool> GetIncludeFeaturedProductsInNormalListAsync()
        {
            return await SettingService.GetSettingByKeyAsync("catalogsettings.includefeaturedproductsinnormallists", defaultValue: false);
        }
    }
}