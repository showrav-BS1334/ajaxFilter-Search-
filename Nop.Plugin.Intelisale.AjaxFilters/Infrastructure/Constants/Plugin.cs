using SevenSpikes.Nop.Framework.Plugin;
using System.Collections.Generic;

namespace Nop.Plugin.Intelisale.AjaxFilters.Infrastructure.Constants
{
    public class Plugin
    {
        public const string SystemName = "Intelisale.AjaxFilters";

        public const string FolderName = "Intelisale.AjaxFilters";

        public const string Name = "Nop Ajax Filters";

        public const string ResourceName = "Intelisale.AjaxFilters.Admin.Menu.MenuName";

        public const string UrlInStore = "http://www.nop-templates.com/ajax-filters-plugin-for-nopcommerce";

        public static List<MenuItem7Spikes> MenuItems => new List<MenuItem7Spikes>
        {
            new MenuItem7Spikes
            {
                SubMenuName = "SevenSpikes.NopAjaxFilters.Admin.Submenus.Settings",
                SubMenuRelativePath = "NopAjaxFiltersAdmin/Settings"
            }
        };

        public static bool IsTrialVersion => false;
    }
}