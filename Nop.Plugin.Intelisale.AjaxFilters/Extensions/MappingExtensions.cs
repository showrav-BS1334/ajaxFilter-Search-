using Nop.Plugin.Intelisale.AjaxFilters.Models.AttributeFilter;
using Nop.Plugin.Intelisale.AjaxFilters.Models.ManufacturerFilter;
using Nop.Plugin.Intelisale.AjaxFilters.Models.SpecificationFilter;
using Nop.Plugin.Intelisale.AjaxFilters.Models.VendorFilter;
using SevenSpikes.Nop.Framework.AutoMapper;
using SevenSpikes.Nop.Services.Catalog.DTO;

namespace Nop.Plugin.Intelisale.AjaxFilters.Extensions
{
    public static class MappingExtensions
    {
        public static SpecificationFilterDTO ToDTO(this SpecificationFilterGroup specificationFilterGroup)
        {
            return specificationFilterGroup.MapTo<SpecificationFilterGroup, SpecificationFilterDTO>();
        }

        public static SpecificationFilterModelDTO ToDTO(this SpecificationFilterModel7Spikes specificationFilterModel7Spikes)
        {
            return specificationFilterModel7Spikes.MapTo<SpecificationFilterModel7Spikes, SpecificationFilterModelDTO>();
        }

        public static AttributeFilterDTO ToDTO(this AttributeFilterGroup attributeFilterGroup)
        {
            return attributeFilterGroup.MapTo<AttributeFilterGroup, AttributeFilterDTO>();
        }

        public static AttributeFilterModelDTO ToDTO(this AttributeFilterModel7Spikes attributeFilterModel7Spikes)
        {
            return attributeFilterModel7Spikes.MapTo<AttributeFilterModel7Spikes, AttributeFilterModelDTO>();
        }

        public static ManufacturerFilterModelDTO ToDTO(this ManufacturerFilterModel7Spikes manufacturerFilterModel7Spikes)
        {
            return manufacturerFilterModel7Spikes.MapTo<ManufacturerFilterModel7Spikes, ManufacturerFilterModelDTO>();
        }

        public static VendorFilterModelDTO ToDTO(this VendorFilterModel7Spikes vendorFilterModel7Spikes)
        {
            return vendorFilterModel7Spikes.MapTo<VendorFilterModel7Spikes, VendorFilterModelDTO>();
        }
    }
}