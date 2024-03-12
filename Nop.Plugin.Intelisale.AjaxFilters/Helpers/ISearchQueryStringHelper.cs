using Microsoft.AspNetCore.Routing;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Intelisale.AjaxFilters.Helpers
{
	public interface ISearchQueryStringHelper
	{
		SearchQueryStringParameters GetQueryStringParameters(string queryString);

		RouteValueDictionary PrepareSearchRouteValues(SearchModel model, CatalogProductsCommand command);
	}
}
