using Nop.Plugin.Intelisale.AjaxFilters.Domain.Enums;

namespace Nop.Plugin.Intelisale.AjaxFilters.Models.VendorFilter
{
    public class VendorFilterItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public FilterItemState FilterItemState { get; set; }
    }
}