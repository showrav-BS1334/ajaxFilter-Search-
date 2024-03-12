using Nop.Plugin.Intelisale.AjaxFilters.Domain.Enums;

namespace Nop.Plugin.Intelisale.AjaxFilters.Models.InStockFilter
{
    public class InStockFilterModel7Spikes
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public int ManufacturerId { get; set; }
        public int VendorId { get; set; }
        public int Priority { get; set; }
        public FilterItemState FilterItemState { get; set; }
    }
}