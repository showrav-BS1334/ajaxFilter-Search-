using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Intelisale.AjaxFilters.Areas.Admin.Extensions
{
    public static class SpecificationAttributeServiceExtensions
    {
        public static SpecificationAttribute GetSpecificationAttributeByName(this ISpecificationAttributeService specificationAttributeService, string name)
        {
            return EngineContext.Current.Resolve<IRepository<SpecificationAttribute>>().Table.Where((SpecificationAttribute sa) => sa.Name.Equals(name)).FirstOrDefault();
        }

        public static IList<SpecificationAttribute> GetSpecificationAttributeByIds(this ISpecificationAttributeService specificationAttributeService, int[] ids)
        {
            List<SpecificationAttribute> list = (from sa in EngineContext.Current.Resolve<IRepository<SpecificationAttribute>>().Table
                                                 where ids.Contains(sa.Id)
                                                 orderby sa.Id
                                                 select sa).ToList();
            List<SpecificationAttribute> list2 = new List<SpecificationAttribute>();
            int[] array = ids;
            foreach (int id in array)
            {
                SpecificationAttribute specificationAttribute = list.Find((SpecificationAttribute x) => x.Id == id);
                if (specificationAttribute != null)
                {
                    list2.Add(specificationAttribute);
                }
            }
            return list2;
        }
    }
}