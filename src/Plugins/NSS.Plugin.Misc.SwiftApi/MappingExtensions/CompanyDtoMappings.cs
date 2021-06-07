using NSS.Plugin.Misc.SwiftApi.DTOs.Companies;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Text;
using NSS.Plugin.Misc.SwiftApi.AutoMapper;

namespace NSS.Plugin.Misc.SwiftApi.MappingExtensions
{
    public static class CompanyDtoMappings
    {
        public static CompanyDto ToDto(this Company company)
        {
            return company.MapTo<Company, CompanyDto>();
        }
    }
}
