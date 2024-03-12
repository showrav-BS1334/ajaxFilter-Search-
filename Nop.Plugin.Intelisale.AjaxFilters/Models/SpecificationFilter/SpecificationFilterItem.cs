using Nop.Plugin.Intelisale.AjaxFilters.Domain.Enums;

namespace Nop.Plugin.Intelisale.AjaxFilters.Models.SpecificationFilter
{
    public class SpecificationFilterItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public FilterItemState FilterItemState { get; set; }
        public string ColorSquaresRgb { get; set; }
    }
}