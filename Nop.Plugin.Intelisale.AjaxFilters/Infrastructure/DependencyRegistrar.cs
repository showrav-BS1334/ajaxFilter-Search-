using AutoMapper;
using Nop.Plugin.Intelisale.AjaxFilters.Areas.Admin.Models;
using Nop.Plugin.Intelisale.AjaxFilters.Domain;
using Nop.Plugin.Intelisale.AjaxFilters.Extensions;
using Nop.Plugin.Intelisale.AjaxFilters.Helpers;
using Nop.Plugin.Intelisale.AjaxFilters.Models.AttributeFilter;
using Nop.Plugin.Intelisale.AjaxFilters.Models.ManufacturerFilter;
using Nop.Plugin.Intelisale.AjaxFilters.Models.SpecificationFilter;
using Nop.Plugin.Intelisale.AjaxFilters.Models.VendorFilter;
using Nop.Plugin.Intelisale.AjaxFilters.QueryStringManipulation;
using Nop.Plugin.Intelisale.AjaxFilters.Services;
using Microsoft.Extensions.DependencyInjection;
using Nop.Data;
using SevenSpikes.Nop.Core.Helpers;
using SevenSpikes.Nop.Framework.AutoMapper;
using SevenSpikes.Nop.Framework.DependancyRegistrar;
using SevenSpikes.Nop.Services.Catalog;
using SevenSpikes.Nop.Services.Catalog.DTO;
using SevenSpikes.Nop.Services.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Intelisale.AjaxFilters.Infrastructure
{
    public class DependencyRegistrar : BaseDependancyRegistrar7Spikes
    {
        protected override void CreateModelMappings()
        {
            AutoMapperConfiguration7Spikes.MapperConfigurationExpression.CreateMap<SpecificationFilterModel7Spikes, SpecificationFilterModelDTO>().ForMember<IList<SpecificationFilterDTO>>((SpecificationFilterModelDTO dest) => dest.SpecificationFilterDTOs, delegate (IMemberConfigurationExpression<SpecificationFilterModel7Spikes, SpecificationFilterModelDTO, IList<SpecificationFilterDTO>> opt)
            {
                opt.MapFrom<IList<SpecificationFilterDTO>>((SpecificationFilterModel7Spikes x) => x.SpecificationFilterGroups.Select((SpecificationFilterGroup group) => group.ToDTO()).ToList());
            });
            AutoMapperConfiguration7Spikes.MapperConfigurationExpression.CreateMap<SpecificationFilterGroup, SpecificationFilterDTO>().ForMember<int>((SpecificationFilterDTO dest) => dest.Id, delegate (IMemberConfigurationExpression<SpecificationFilterGroup, SpecificationFilterDTO, int> opt)
            {
                opt.MapFrom<int>((SpecificationFilterGroup x) => x.Id);
            }).ForMember<bool>((SpecificationFilterDTO dest) => dest.IsMain, delegate (IMemberConfigurationExpression<SpecificationFilterGroup, SpecificationFilterDTO, bool> opt)
            {
                opt.MapFrom<bool>((SpecificationFilterGroup x) => x.IsMain);
            })
                .ForMember<IList<int>>((SpecificationFilterDTO dest) => dest.SelectedFilterIds, delegate (IMemberConfigurationExpression<SpecificationFilterGroup, SpecificationFilterDTO, IList<int>> opt)
                {
                    opt.MapFrom<IList<int>>((SpecificationFilterGroup x) => (from filterItem in x.FilterItems
                                                                             where (int)filterItem.FilterItemState == 1 || (int)filterItem.FilterItemState == 2
                                                                             select filterItem into item
                                                                             select item.Id).ToList());
                });
            AutoMapperConfiguration7Spikes.MapperConfigurationExpression.CreateMap<AttributeFilterModel7Spikes, AttributeFilterModelDTO>().ForMember<IList<AttributeFilterDTO>>((AttributeFilterModelDTO dest) => dest.AttributeFilterDTOs, delegate (IMemberConfigurationExpression<AttributeFilterModel7Spikes, AttributeFilterModelDTO, IList<AttributeFilterDTO>> opt)
            {
                opt.MapFrom<IList<AttributeFilterDTO>>((AttributeFilterModel7Spikes x) => x.AttributeFilterGroups.Select((AttributeFilterGroup group) => group.ToDTO()).ToList());
            });
            AutoMapperConfiguration7Spikes.MapperConfigurationExpression.CreateMap<AttributeFilterGroup, AttributeFilterDTO>().ForMember<int>((AttributeFilterDTO dest) => dest.Id, delegate (IMemberConfigurationExpression<AttributeFilterGroup, AttributeFilterDTO, int> opt)
            {
                opt.MapFrom<int>((AttributeFilterGroup x) => x.Id);
            }).ForMember<bool>((AttributeFilterDTO dest) => dest.IsMain, delegate (IMemberConfigurationExpression<AttributeFilterGroup, AttributeFilterDTO, bool> opt)
            {
                opt.MapFrom<bool>((AttributeFilterGroup x) => x.IsMain);
            })
                .ForMember<IList<int>>((AttributeFilterDTO dest) => dest.AllProductVariantIds, delegate (IMemberConfigurationExpression<AttributeFilterGroup, AttributeFilterDTO, IList<int>> opt)
                {
                    opt.MapFrom<IList<int>>((AttributeFilterGroup x) => x.FilterItems.SelectMany((AttributeFilterItem item) => item.ProductVariantAttributeIds).ToList());
                })
                .ForMember<IList<int>>((AttributeFilterDTO dest) => dest.SelectedProductVariantIds, delegate (IMemberConfigurationExpression<AttributeFilterGroup, AttributeFilterDTO, IList<int>> opt)
                {
                    opt.MapFrom<IList<int>>((AttributeFilterGroup x) => x.FilterItems.Where((AttributeFilterItem filterItem) => (int)filterItem.FilterItemState == 1 || (int)filterItem.FilterItemState == 2).SelectMany((AttributeFilterItem item) => item.ProductVariantAttributeIds).ToList());
                });
            AutoMapperConfiguration7Spikes.MapperConfigurationExpression.CreateMap<ManufacturerFilterModel7Spikes, ManufacturerFilterModelDTO>().ForMember<IList<int>>((ManufacturerFilterModelDTO dest) => dest.SelectedFilterIds, delegate (IMemberConfigurationExpression<ManufacturerFilterModel7Spikes, ManufacturerFilterModelDTO, IList<int>> opt)
            {
                opt.MapFrom<List<int>>((ManufacturerFilterModel7Spikes x) => (from filterItem in x.ManufacturerFilterItems
                                                                              where (int)filterItem.FilterItemState == 1 || (int)filterItem.FilterItemState == 2
                                                                              select filterItem into item
                                                                              select item.Id).ToList());
            });
            AutoMapperConfiguration7Spikes.MapperConfigurationExpression.CreateMap<VendorFilterModel7Spikes, VendorFilterModelDTO>().ForMember<IList<int>>((VendorFilterModelDTO dest) => dest.SelectedFilterIds, delegate (IMemberConfigurationExpression<VendorFilterModel7Spikes, VendorFilterModelDTO, IList<int>> opt)
            {
                opt.MapFrom<List<int>>((VendorFilterModel7Spikes x) => (from filterItem in x.VendorFilterItems
                                                                        where (int)filterItem.FilterItemState == 1 || (int)filterItem.FilterItemState == 2
                                                                        select filterItem into item
                                                                        select item.Id).ToList());
            });
            CreateMvcModelMap<NopAjaxFiltersSettingsModel, NopAjaxFiltersSettings>();
        }

        protected override void RegisterPluginServices(IServiceCollection services)
        {
            services.AddTransient<IProductServiceNopAjaxFilters, ProductServiceNopAjaxFilters>();
            services.AddTransient<IProductAttributeService7Spikes, ProductAttributeService7Spikes>();
            services.AddTransient<ISpecificationAttributeService7Spikes, SpecificationAttributeService7Spikes>();
            services.AddTransient<IQueryStringBuilder, QueryStringBuilder>();
            services.AddTransient<IProductAttributeServiceAjaxFilters, ProductAttributeServiceAjaxFilters>();
            services.AddTransient<IPriceCalculationServiceNopAjaxFilters, PriceCalculationServiceNopAjaxFilters>();
            services.AddTransient<ISpecificationFilterOptionsHelper, SpecificationFilterOptionsHelper>();
            services.AddTransient<IAttributeFilterOptionsHelper, AttributeFilterOptionsHelper>();
            services.AddScoped<IFiltersPageHelper, FiltersPageHelper>();
            services.AddTransient<IQueryStringToModelUpdater, QueryStringToModelUpdater>();
            services.AddTransient<ITaxServiceNopAjaxFilters, TaxServiceNopAjaxFilters>();
            services.AddTransient<ISearchQueryStringHelper, SearchQueryStringHelper>();
            services.AddTransient<IProductServiceNopAjaxFilters, ProductServiceNopAjaxFilters>();
            DataSettings dataSettings = DataSettingsManager.LoadSettings();
            if (dataSettings.DataProvider == DataProviderType.MySql)
            {
                services.AddTransient<IAjaxFiltersDatabaseService, AjaxFiltersDatabaseServiceMySQL>();
            }
            else if (dataSettings.DataProvider == DataProviderType.PostgreSQL)
            {
                services.AddTransient<IAjaxFiltersDatabaseService, AjaxFiltersDatabaseServicePostgreSQL>();
            }
            else
            {
                services.AddTransient<IAjaxFiltersDatabaseService, AjaxFiltersDatabaseService>();
            }
            services.AddTransient<IConvertToDictionaryHelper, ConvertToDictionaryHelper>();
        }
    }
}