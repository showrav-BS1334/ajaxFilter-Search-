using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Tax;
using System.Threading.Tasks;

namespace Nop.Plugin.Intelisale.AjaxFilters.Services
{
    public class TaxServiceNopAjaxFilters : TaxService, ITaxServiceNopAjaxFilters
    {
        public TaxServiceNopAjaxFilters(
            AddressSettings addressSettings,
            CustomerSettings customerSettings,
            IAddressService addressService,
            ICountryService countryService,
            ICustomerService customerService,
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
            IGeoLookupService geoLookupService,
            ILogger logger,
            IStateProvinceService stateProvinceService,
            IStaticCacheManager cacheManager,
            IStoreContext storeContext,
            ITaxPluginManager taxPluginManager,
            IWebHelper webHelper,
            IWorkContext workContext,
            ShippingSettings shippingSettings,
            TaxSettings taxSettings
            )
            : base(addressSettings,
                  customerSettings,
                  addressService,
                  countryService,
                  customerService,
                  eventPublisher,
                  genericAttributeService,
                  geoLookupService,
                  logger,
                  stateProvinceService,
                  storeContext,
                  taxPluginManager,
                  webHelper,
                  workContext,
                  shippingSettings,
                  taxSettings)
        {
        }

        public async Task<decimal> GetTaxRateForProductAsync(Product product, int taxCategoryId, Customer customer)
        {
            return (await GetProductPriceAsync(product, taxCategoryId, product.Price, includingTax: false, customer, priceIncludesTax: false)).Item2;
        }
    }
}