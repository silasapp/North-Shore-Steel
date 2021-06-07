using NSS.Plugin.Misc.SwiftApi.Areas.Admin.Models;
using NSS.Plugin.Misc.SwiftApi.AutoMapper;
using NSS.Plugin.Misc.SwiftApi.Domain;

namespace NSS.Plugin.Misc.SwiftApi.MappingExtensions
{
    public static class ConfigurationMappings
    {
        public static ConfigurationModel ToModel(this ApiSettings apiSettings)
        {
            return apiSettings.MapTo<ApiSettings, ConfigurationModel>();
        }

        public static ApiSettings ToEntity(this ConfigurationModel apiSettingsModel)
        {
            return apiSettingsModel.MapTo<ConfigurationModel, ApiSettings>();
        }
    }
}
