using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core.Domain.Cms;
using Nop.Core.Infrastructure;
using Nop.Plugin.Intelisale.AjaxFilters.Domain;

namespace Nop.Plugin.Intelisale.AjaxFilters.ActionFilters
{
    public class SearchActionFilterAttribute : ActionFilterAttribute
    {
        private IUrlHelperFactory _urlHelperFactory;
        private NopAjaxFiltersSettings _ajaxFiltersSettings;
        private WidgetSettings _widgetSettings;

        private IUrlHelperFactory UrlHelperFactory => _urlHelperFactory ?? (_urlHelperFactory = EngineContext.Current.Resolve<IUrlHelperFactory>());
        private NopAjaxFiltersSettings AjaxFiltersSettings => _ajaxFiltersSettings ?? (_ajaxFiltersSettings = EngineContext.Current.Resolve<NopAjaxFiltersSettings>());
        private WidgetSettings WidgetSettings => _widgetSettings ?? (_widgetSettings = EngineContext.Current.Resolve<WidgetSettings>());

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (AjaxFiltersSettings.EnableAjaxFilters && WidgetSettings.ActiveWidgetSystemNames.Contains("Intelisale.AjaxFilters") && !string.IsNullOrEmpty(AjaxFiltersSettings.WidgetZone) && AjaxFiltersSettings.ShowFiltersOnSearchPage)
            {
                string value = UrlHelperFactory.GetUrlHelper(filterContext).RouteUrl("FilterProductSearch") + filterContext.HttpContext.Request.QueryString.ToUriComponent();
                filterContext.HttpContext.Response.StatusCode = 302;
                filterContext.HttpContext.Response.Headers["Location"] = value;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}