using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Services.Stores;
using Nop.Plugin.Intelisale.AjaxFilters.Domain;

namespace Nop.Plugin.Intelisale.AjaxFilters.Helpers
{
	public static class FilterSettingHelper
	{
		private static ISettingService _settingService;

		private static ISettingService SettingService
		{
			get
			{
				if (_settingService == null)
				{
					_settingService = EngineContext.Current.Resolve<ISettingService>();
				}
				return _settingService;
			}
		}

		public static async Task UpdateNopCommerceFilterSettings()
		{
			IList<Store> stores = await EngineContext.Current.Resolve<IStoreService>().GetAllStoresAsync();
			await UpdateFilterSettingForStore(0);
			if (stores.Count <= 1 || !(await SettingService.GetSettingByKeyAsync("SevenSpikesCommonSettings.LoadStoreSettingsOnLoad", defaultValue: true)))
			{
				return;
			}
			foreach (Store item in stores)
			{
				await UpdateFilterSettingForStore(item.Id);
			}
		}

		private static async Task UpdateFilterSettingForStore(int storeId)
		{
			CatalogSettings catalogSettings = await SettingService.LoadSettingAsync<CatalogSettings>(storeId);
			if (!(await SettingService.LoadSettingAsync<NopAjaxFiltersSettings>(storeId)).EnableAjaxFilters)
			{
				return;
			}
			bool flag = catalogSettings.EnableManufacturerFiltering;
			if (flag)
			{
				flag = await SettingService.SettingExistsAsync(catalogSettings, (CatalogSettings x) => x.EnableManufacturerFiltering, storeId);
			}
			if (flag)
			{
				catalogSettings.EnableManufacturerFiltering = false;
				await SettingService.SaveSettingAsync(catalogSettings, (CatalogSettings x) => x.EnableManufacturerFiltering, storeId);
			}
			flag = catalogSettings.EnablePriceRangeFiltering;
			if (flag)
			{
				flag = await SettingService.SettingExistsAsync(catalogSettings, (CatalogSettings x) => x.EnablePriceRangeFiltering, storeId);
			}
			if (flag)
			{
				catalogSettings.EnablePriceRangeFiltering = false;
				await SettingService.SaveSettingAsync(catalogSettings, (CatalogSettings x) => x.EnablePriceRangeFiltering, storeId);
			}
		}
	}
}
