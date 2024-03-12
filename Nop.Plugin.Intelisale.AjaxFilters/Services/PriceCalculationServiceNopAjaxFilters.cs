using Nop.Plugin.Intelisale.AjaxFilters.Models.PriceRangeFilterSlider;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Directory;
using SevenSpikes.Nop.Services.Catalog;
using SevenSpikes.Nop.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Nop.Plugin.Intelisale.AjaxFilters.Services
{
    public class PriceCalculationServiceNopAjaxFilters : IPriceCalculationServiceNopAjaxFilters
    {
        private readonly IWorkContext _workContext;
        private readonly CatalogSettings _catalogSettings;
        private readonly TaxSettings _taxSettings;
        private readonly ICategoryService7Spikes _categoryService7Spikes;
        private readonly IAclHelper _aclHelper;
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly ITaxServiceNopAjaxFilters _taxServiceNopAjaxFilters;
        private readonly ICurrencyService _currencyService;
        private readonly IProductServiceNopAjaxFilters _productServiceNopAjaxFilters;
        private readonly IStoreHelper _storeHelper;
        private readonly ICustomerService _customerService;

        public PriceCalculationServiceNopAjaxFilters(
            IWorkContext workContext,
            CatalogSettings catalogSettings,
            TaxSettings taxSettings,
            ICategoryService7Spikes categoryService7Spikes,
            IAclHelper aclHelper,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<ProductManufacturer> productManufacturerRepository,
            IRepository<Vendor> vendorRepository,
            ITaxServiceNopAjaxFilters taxServiceNopAjaxFilters,
            ICurrencyService currencyService,
            IProductServiceNopAjaxFilters productServiceNopAjaxFilters,
            IStoreHelper storeHelper,
            ICustomerService customerService
            )
        {
            _workContext = workContext;
            _catalogSettings = catalogSettings;
            _taxSettings = taxSettings;
            _categoryService7Spikes = categoryService7Spikes;
            _aclHelper = aclHelper;
            _vendorRepository = vendorRepository;
            _productCategoryRepository = productCategoryRepository;
            _productManufacturerRepository = productManufacturerRepository;
            _taxServiceNopAjaxFilters = taxServiceNopAjaxFilters;
            _currencyService = currencyService;
            _productServiceNopAjaxFilters = productServiceNopAjaxFilters;
            _storeHelper = storeHelper;
            _customerService = customerService;
        }

        public async Task<PriceRangeFilterDto> GetPriceRangeFilterDtoAsync(int categoryId, int manufacturerId, int vendorId)
        {
            PriceRangeFilterDto model = new PriceRangeFilterDto();
            if (categoryId > 0)
            {
                SetDiscountAmountPercentageForCategory(categoryId, model);
            }
            switch (await _workContext.GetTaxDisplayTypeAsync())
            {
                case TaxDisplayType.ExcludingTax:
                    model.TaxDisplayTypeIncludingTax = false;
                    break;

                case TaxDisplayType.IncludingTax:
                    model.TaxDisplayTypeIncludingTax = true;
                    break;
            }
            model.TaxPriceIncludeTax = _taxSettings.PricesIncludeTax;
            await SetMinMaxPricesAsync(categoryId, manufacturerId, vendorId, model);
            PriceRangeFilterDto priceRangeFilterDto = model;
            ICurrencyService currencyService = _currencyService;
            decimal minPrice = model.MinPrice;
            priceRangeFilterDto.MinPrice = await currencyService.ConvertFromPrimaryStoreCurrencyAsync(minPrice, await _workContext.GetWorkingCurrencyAsync());
            priceRangeFilterDto = model;
            currencyService = _currencyService;
            minPrice = model.MaxPrice;
            priceRangeFilterDto.MaxPrice = await currencyService.ConvertFromPrimaryStoreCurrencyAsync(minPrice, await _workContext.GetWorkingCurrencyAsync());
            return model;
        }

        public async Task<decimal> CalculateBasePriceAsync(decimal price, PriceRangeFilterDto priceRangeModel, bool isFromPrice)
        {
            price = GetPriceWithoutDiscount(price, priceRangeModel);
            price = GetPriceWithoutTax(price, priceRangeModel);
            ICurrencyService currencyService = _currencyService;
            decimal amount = price;
            price = await currencyService.ConvertToPrimaryStoreCurrencyAsync(amount, await _workContext.GetWorkingCurrencyAsync());
            price = Math.Round(price, 2);
            return price;
        }

        private static decimal GetPriceWithoutTax(decimal price, PriceRangeFilterDto priceRangeModel)
        {
            decimal num = price;
            if (priceRangeModel.TaxPriceIncludeTax)
            {
                if (!priceRangeModel.TaxDisplayTypeIncludingTax)
                {
                    num = CalculatePriceWithoutTax(price, priceRangeModel.TaxRatePercentage, increase: false);
                }
            }
            else if (priceRangeModel.TaxDisplayTypeIncludingTax)
            {
                num = CalculatePriceWithoutTax(num, priceRangeModel.TaxRatePercentage, increase: true);
            }
            return num;
        }

        private static decimal CalculatePriceWithoutTax(decimal price, decimal percent, bool increase)
        {
            decimal num = default(decimal);
            if (percent == 0m)
            {
                return price;
            }
            if (increase)
            {
                return price / (1m + percent / 100m);
            }
            return price * (100m + percent) / (100m + percent - percent);
        }

        private static decimal GetPriceWithoutDiscount(decimal price, PriceRangeFilterDto priceRangeModel)
        {
            if (priceRangeModel.MaxDiscountAmount == 0m && priceRangeModel.MaxDiscountPercentage == 0m)
            {
                return price;
            }
            decimal result = price;
            decimal num = (decimal)((float)price * 100f / (100f - (float)priceRangeModel.MaxDiscountPercentage));
            decimal num2 = price + priceRangeModel.MaxDiscountAmount;
            if (num > 0m && num > num2)
            {
                result = num;
            }
            else if (num2 > 0m)
            {
                result = num2;
            }
            return result;
        }

        private async Task SetMinMaxPricesAsync(int categoryId, int manufacturerId, int vendorId, PriceRangeFilterDto priceRangeModel)
        {
            IQueryable<Product> source = await PrepareMinMaxPriceProductVariantQueryAsync(categoryId, manufacturerId, vendorId);
            if (source.Any())
            {
                decimal? minPrice = source.Min((Expression<Func<Product, decimal?>>)((Product pv) => pv.Price));
                decimal? maxPrice = source.Max((Expression<Func<Product, decimal?>>)((Product pv) => pv.Price));
                Product productVariant = await source.Take(1).FirstOrDefaultAsync();
                SetMinPrice(priceRangeModel, minPrice);
                SetMaxPrice(priceRangeModel, maxPrice);
                priceRangeModel.TaxRatePercentage = await GetTaxRatePercentageAsync(productVariant);
                if (priceRangeModel.TaxRatePercentage > 0m)
                {
                    SetTaxForMinMaxPrice(priceRangeModel);
                }
            }
        }

        private void SetTaxForMinMaxPrice(PriceRangeFilterDto priceRangeModel)
        {
            if (priceRangeModel.TaxPriceIncludeTax)
            {
                if (!priceRangeModel.TaxDisplayTypeIncludingTax)
                {
                    priceRangeModel.MinPrice = CalculatePrice(priceRangeModel.MinPrice, priceRangeModel.TaxRatePercentage, increase: false);
                    priceRangeModel.MaxPrice = CalculatePrice(priceRangeModel.MaxPrice, priceRangeModel.TaxRatePercentage, increase: false);
                }
            }
            else if (priceRangeModel.TaxDisplayTypeIncludingTax)
            {
                priceRangeModel.MinPrice = CalculatePrice(priceRangeModel.MinPrice, priceRangeModel.TaxRatePercentage, increase: true);
                priceRangeModel.MaxPrice = CalculatePrice(priceRangeModel.MaxPrice, priceRangeModel.TaxRatePercentage, increase: true);
            }
        }

        private decimal CalculatePrice(decimal? nulablePrice, decimal percent, bool increase)
        {
            decimal num = default(decimal);
            if (nulablePrice.HasValue)
            {
                num = nulablePrice.Value;
            }
            decimal num2 = default(decimal);
            if (percent == 0m)
            {
                return num;
            }
            if (increase)
            {
                return num * (1m + percent / 100m);
            }
            return num - num / (100m + percent) * percent;
        }

        private async Task<decimal> GetTaxRatePercentageAsync(Product productVariant)
        {
            Customer customer = await _workContext.GetCurrentCustomerAsync();
            if (customer != null)
            {
                if (customer.IsTaxExempt)
                {
                    return default(decimal);
                }
                if ((await _customerService.GetCustomerRolesAsync(customer)).Any((CustomerRole cr) => cr.TaxExempt))
                {
                    return default(decimal);
                }
            }
            return await _taxServiceNopAjaxFilters.GetTaxRateForProductAsync(productVariant, 0, customer);
        }

        private void SetMaxPrice(PriceRangeFilterDto priceRangeModel, decimal? maxPrice)
        {
            if (!maxPrice.HasValue)
            {
                priceRangeModel.MaxPrice = 0m;
                return;
            }
            maxPrice = ApplyDiscount(maxPrice.Value, priceRangeModel);
            priceRangeModel.MaxPrice = maxPrice.Value;
        }

        private void SetMinPrice(PriceRangeFilterDto priceRangeModel, decimal? minPrice)
        {
            if (!minPrice.HasValue)
            {
                priceRangeModel.MinPrice = 0m;
                return;
            }
            minPrice = ApplyDiscount(minPrice.Value, priceRangeModel);
            priceRangeModel.MinPrice = minPrice.Value;
        }

        private decimal ApplyDiscount(decimal price, PriceRangeFilterDto priceRangeModel)
        {
            decimal result = default(decimal);
            decimal num = price - (decimal)((float)price * (float)priceRangeModel.MaxDiscountPercentage / 100f);
            decimal num2 = price - priceRangeModel.MaxDiscountAmount;
            if (num > 0m && num < num2)
            {
                return num;
            }
            if (num2 > 0m)
            {
                return num2;
            }
            return result;
        }

        private async Task<IQueryable<Product>> PrepareMinMaxPriceProductVariantQueryAsync(int categoryId, int manufacturerId, int vendorId)
        {
            DateTime nowUtc = DateTime.UtcNow;
            IQueryable<Product> productsQuery = await _aclHelper.GetAvailableProductsForCurrentCustomerAsync();
            productsQuery = await _storeHelper.GetProductsForCurrentStoreAsync(productsQuery);
            if (manufacturerId > 0)
            {
                return await PrepareMinMaxPriceProductVariantForManufacturerQueryAsync(manufacturerId, productsQuery, nowUtc);
            }
            if (vendorId > 0)
            {
                return await PrepareMinMaxPriceProductVariantForVendorQueryAsync(vendorId, productsQuery, nowUtc);
            }
            return await PrepareMinMaxPriceProductVariantForCategoryQueryAsync(categoryId, productsQuery, nowUtc);
        }

        private async Task<IQueryable<Product>> PrepareMinMaxPriceProductVariantForManufacturerQueryAsync(int manufacturerId, IQueryable<Product> availableProductsQuery, DateTime nowUtc)
        {
            bool includeFeaturedProducts = _catalogSettings.IncludeFeaturedProductsInNormalLists;
            IList<int> groupProductIds = await _productServiceNopAjaxFilters.GetAllGroupProductIdsInManufacturerAsync(manufacturerId);
            return from p in availableProductsQuery
                   join pm in _productManufacturerRepository.Table on p.Id equals pm.ProductId into p_pm
                   from pm in p_pm.DefaultIfEmpty()
                   where ((pm != null && pm.ManufacturerId == manufacturerId && (pm.IsFeaturedProduct == includeFeaturedProducts || !pm.IsFeaturedProduct)) || (pm == null && groupProductIds.Contains(p.ParentGroupedProductId))) && p.Published && !p.Deleted && (!p.AvailableStartDateTimeUtc.HasValue || (DateTime)p.AvailableStartDateTimeUtc <= (DateTime)(DateTime?)nowUtc) && (!p.AvailableEndDateTimeUtc.HasValue || (DateTime)p.AvailableEndDateTimeUtc >= (DateTime)(DateTime?)nowUtc)
                   select p;
        }

        private async Task<IQueryable<Product>> PrepareMinMaxPriceProductVariantForVendorQueryAsync(int vendorId, IQueryable<Product> availableProductsQuery, DateTime nowUtc)
        {
            IList<int> groupProductIds = await _productServiceNopAjaxFilters.GetAllGroupProductIdsInVendorAsync(vendorId);
            return from p in availableProductsQuery
                   join v in _vendorRepository.Table on p.VendorId equals v.Id into p_pv
                   from v in p_pv.DefaultIfEmpty()
                   where ((v != null && v.Id == vendorId) || (v == null && groupProductIds.Contains(p.ParentGroupedProductId))) && p.Published && !p.Deleted && (!p.AvailableStartDateTimeUtc.HasValue || (DateTime)p.AvailableStartDateTimeUtc <= (DateTime)(DateTime?)nowUtc) && (!p.AvailableEndDateTimeUtc.HasValue || (DateTime)p.AvailableEndDateTimeUtc >= (DateTime)(DateTime?)nowUtc)
                   select p;
        }

        private async Task<IQueryable<Product>> PrepareMinMaxPriceProductVariantForCategoryQueryAsync(int categoryId, IQueryable<Product> availableProductsQuery, DateTime nowUtc)
        {
            bool showProductsFromSubcategories = _catalogSettings.ShowProductsFromSubcategories;
            bool includeFeaturedProducts = _catalogSettings.IncludeFeaturedProductsInNormalLists;
            List<int> categoryIds = new List<int>
            {
                categoryId
            };
            if (showProductsFromSubcategories)
            {
                List<int> collection = await _categoryService7Spikes.GetCategoryIdsByParentCategoryAsync(categoryId);
                categoryIds.AddRange(collection);
            }
            IList<int> groupProductIds = await _productServiceNopAjaxFilters.GetAllGroupProductIdsInCategoriesAsync(categoryIds);
            return from p in availableProductsQuery
                   join pc in _productCategoryRepository.Table on p.Id equals pc.ProductId into p_pc
                   from pc in p_pc.DefaultIfEmpty()
                   where ((pc != null && categoryIds.Contains(pc.CategoryId) && p.ProductTypeId != 10 && (pc.IsFeaturedProduct == includeFeaturedProducts || !pc.IsFeaturedProduct)) || (pc == null && groupProductIds.Contains(p.ParentGroupedProductId))) && p.Published && !p.Deleted && (!p.AvailableStartDateTimeUtc.HasValue || (DateTime)p.AvailableStartDateTimeUtc <= (DateTime)(DateTime?)nowUtc) && (!p.AvailableEndDateTimeUtc.HasValue || (DateTime)p.AvailableEndDateTimeUtc >= (DateTime)(DateTime?)nowUtc)
                   select p;
        }

        private void SetDiscountAmountPercentageForCategory(int categoryId, PriceRangeFilterDto priceRangeModel)
        {
            IEnumerable<Discount> allowedDiscountsForCategory = GetAllowedDiscountsForCategory(categoryId);
            decimal num = default(decimal);
            decimal num2 = default(decimal);
            foreach (Discount item in allowedDiscountsForCategory)
            {
                if (item.DiscountAmount > num)
                {
                    num = item.DiscountAmount;
                }
                if (item.DiscountPercentage > num2)
                {
                    num2 = item.DiscountPercentage;
                }
            }
            priceRangeModel.MaxDiscountAmount = num;
            priceRangeModel.MaxDiscountPercentage = num2;
        }

        private IEnumerable<Discount> GetAllowedDiscountsForCategory(int categoryId)
        {
            return new List<Discount>();
        }
    }
}