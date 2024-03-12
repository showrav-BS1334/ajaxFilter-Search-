using Nop.Plugin.Intelisale.AjaxFilters.Infrastructure.Cache;
using Nop.Plugin.Intelisale.AjaxFilters.Models.AttributeFilter;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Plugin.Intelisale.AjaxFilters.Services
{
    public class ProductAttributeServiceAjaxFilters : IProductAttributeServiceAjaxFilters
    {
        private readonly IRepository<PredefinedProductAttributeValue> _predefinedAttributeValueRepository;

        private readonly IStaticCacheManager _staticCacheManager;

        public ProductAttributeServiceAjaxFilters(IRepository<PredefinedProductAttributeValue> predefinedAttributeValueRepository, IStaticCacheManager staticCacheManager)
        {
            _predefinedAttributeValueRepository = predefinedAttributeValueRepository;
            _staticCacheManager = staticCacheManager;
        }

        public async Task<IList<AttributeFilterItem>> GetSortedAttributeValuesBasedOnTheirPredefinedDisplayOrderAsync(IEnumerable<AttributeFilterItem> attributeValues)
        {
            return await (from attributeValue in attributeValues
                          join predefinedAttribute in await _staticCacheManager.GetAsync(NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_PREDEFINED_ATTRIBUTE_VALUES_KEY, async () => await _predefinedAttributeValueRepository.Table.ToListAsync()) on new
                          {
                              AttributeId = attributeValue.AttributeId,
                              AttributeValue = attributeValue.Name
                          } equals new
                          {
                              AttributeId = predefinedAttribute.ProductAttributeId,
                              AttributeValue = predefinedAttribute.Name
                          } into temp
                          from predefinedAttributeValue in temp.DefaultIfEmpty(new PredefinedProductAttributeValue
                          {
                              DisplayOrder = int.MaxValue
                          })
                          orderby predefinedAttributeValue.DisplayOrder
                          select attributeValue).ToListAsync();
        }
    }
}