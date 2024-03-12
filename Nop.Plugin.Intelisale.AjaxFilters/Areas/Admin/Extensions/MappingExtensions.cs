using SevenSpikes.Nop.Framework.AutoMapper;
using Nop.Plugin.Intelisale.AjaxFilters.Areas.Admin.Models;
using Nop.Plugin.Intelisale.AjaxFilters.Domain;

namespace Nop.Plugin.Intelisale.AjaxFilters.Areas.Admin.Extensions
{
    public static class MappingExtensions
    {
        public static NopAjaxFiltersSettingsModel ToModel(this NopAjaxFiltersSettings nopAjaxFiltersSettings)
        {
            return nopAjaxFiltersSettings.MapTo<NopAjaxFiltersSettings, NopAjaxFiltersSettingsModel>();
        }

        public static NopAjaxFiltersSettings ToEntity(this NopAjaxFiltersSettingsModel nopAjaxFiltersSettingsModel)
        {
            return nopAjaxFiltersSettingsModel.MapTo<NopAjaxFiltersSettingsModel, NopAjaxFiltersSettings>();
        }
    }
}