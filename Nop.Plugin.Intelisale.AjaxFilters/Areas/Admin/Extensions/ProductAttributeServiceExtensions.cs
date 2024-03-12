using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Intelisale.AjaxFilters.Areas.Admin.Extensions
{
    public static class ProductAttributeServiceExtensions
    {
        public static ProductAttribute GetProductAttributeByName(this IProductAttributeService productAttributeService, string name)
        {
            return EngineContext.Current.Resolve<IRepository<ProductAttribute>>().Table.Where((ProductAttribute sa) => sa.Name.Equals(name)).FirstOrDefault();
        }

        public static IList<ProductAttribute> GetProductAttributesByIds(this IProductAttributeService productAttributeService, int[] ids)
        {
            List<ProductAttribute> list = (from sa in EngineContext.Current.Resolve<IRepository<ProductAttribute>>().Table
                                           where ids.Contains(sa.Id)
                                           orderby sa.Id
                                           select sa).ToList();
            List<ProductAttribute> list2 = new List<ProductAttribute>();
            int[] array = ids;
            foreach (int id in array)
            {
                ProductAttribute productAttribute = list.Find((ProductAttribute x) => x.Id == id);
                if (productAttribute != null)
                {
                    list2.Add(productAttribute);
                }
            }
            return list2;
        }
    }
}