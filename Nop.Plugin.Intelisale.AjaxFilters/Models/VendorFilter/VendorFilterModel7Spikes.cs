using System.Collections.Generic;

namespace Nop.Plugin.Intelisale.AjaxFilters.Models.VendorFilter
{
    public class VendorFilterModel7Spikes
    {
        public int CategoryId { get; set; }
        public int Priority { get; set; }
        public IList<VendorFilterItem> VendorFilterItems { get; set; }

        public VendorFilterModel7Spikes()
        {
            VendorFilterItems = new List<VendorFilterItem>();
        }
    }
}