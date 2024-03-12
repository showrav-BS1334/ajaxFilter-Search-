using Nop.Plugin.Intelisale.AjaxFilters.Domain.Enums;
using System.Collections.Generic;

namespace Nop.Plugin.Intelisale.AjaxFilters.Models.AttributeFilter
{
    public class AttributeFilterItem
    {
        public int ValueId { get; set; }
        public string Name { get; set; }
        public int AttributeId { get; set; }
        public IList<int> ProductVariantAttributeIds { get; set; }
        public FilterItemState FilterItemState { get; set; }
        public string ColorSquaresRgb { get; set; }
        public string ImageSquaresUrl { get; set; }

        public AttributeFilterItem()
        {
            ProductVariantAttributeIds = new List<int>();
        }
    }
}