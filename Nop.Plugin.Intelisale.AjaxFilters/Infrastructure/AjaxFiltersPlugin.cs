using Nop.Plugin.Intelisale.AjaxFilters.Domain;
using Nop.Plugin.Intelisale.AjaxFilters.Services;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Configuration;
using SevenSpikes.Nop.Framework.Plugin;
using System.Threading.Tasks;

namespace Nop.Plugin.Intelisale.AjaxFilters.Infrastructure
{
    public class AjaxFiltersPlugin : BaseAdminWidgetPlugin7Spikes
    {
        private readonly WidgetSettings _widgetSettings;
        private readonly ISettingService _settingService;
        private readonly IAjaxFiltersDatabaseService _ajaxFiltersDatabaseService;
        private readonly INopDataProvider _nopDataProvider;

        public AjaxFiltersPlugin(WidgetSettings widgetSettings,
            ISettingService settingService,
            IAjaxFiltersDatabaseService ajaxFiltersDatabaseService,
            INopDataProvider nopDataProvider
            )
            : base(Constants.Plugin.MenuItems,
                  "Intelisale.AjaxFilters.Admin.Menu.MenuName",
                  "Intelisale.AjaxFilters",
                  Constants.Plugin.IsTrialVersion,
                  "http://www.nop-templates.com/ajax-filters-plugin-for-nopcommerce")
        {
            _widgetSettings = widgetSettings;
            _settingService = settingService;
            _ajaxFiltersDatabaseService = ajaxFiltersDatabaseService;
            _nopDataProvider = nopDataProvider;
        }

        public override string GetConfigurationPageUrl()
        {
            return base.StoreLocation + "Admin/NopAjaxFiltersAdmin/Settings";
        }

        public override string GetWidgetViewComponentName(string widgetZone)
        {
            return "NopAjaxFilters";
        }

        protected override async Task InstallAdditionalSettingsAsync()
        {
            if (!_widgetSettings.ActiveWidgetSystemNames.Contains("Intelisale.AjaxFilters"))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add("Intelisale.AjaxFilters");
                await _settingService.SaveSettingAsync(_widgetSettings);
            }
            await _ajaxFiltersDatabaseService.RemoveDatabaseScriptsAsync();
            await _ajaxFiltersDatabaseService.CreateDatabaseScriptsAsync();
        }

        protected override async Task UninstallAdditionalSettingsAsync()
        {
            NopAjaxFiltersSettings nopAjaxFiltersSettings = EngineContext.Current.Resolve<NopAjaxFiltersSettings>();
            nopAjaxFiltersSettings.WidgetZone = string.Empty;
            await _settingService.SaveSettingAsync(nopAjaxFiltersSettings);
            await _ajaxFiltersDatabaseService.RemoveDatabaseScriptsAsync();
        }

        public override async Task InstallAsync()
        {
            if ((await DataSettingsManager.LoadSettingsAsync()).DataProvider == DataProviderType.PostgreSQL)
            {
                throw new NopException("There is no PostgreSQL support in the Ajax Filters plugin");
            }
            await base.InstallAsync();
        }
    }
}