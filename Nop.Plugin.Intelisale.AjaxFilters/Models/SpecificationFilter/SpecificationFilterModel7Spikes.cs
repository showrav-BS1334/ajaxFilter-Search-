using System.Collections.Generic;

namespace Nop.Plugin.Intelisale.AjaxFilters.Models.SpecificationFilter
{
    public class SpecificationFilterModel7Spikes
    {
        public int CategoryId { get; set; }
        public int ManufacturerId { get; set; }
        public int VendorId { get; set; }
        public int Priority { get; set; }
        public IList<SpecificationFilterGroup> SpecificationFilterGroups { get; set; }

        public SpecificationFilterModel7Spikes()
        {
            SpecificationFilterGroups = new List<SpecificationFilterGroup>();
        }
    }
}