using Nop.Core.Domain.Localization;
using NSS.Plugin.Misc.SwiftApi.AutoMapper;
using NSS.Plugin.Misc.SwiftApi.DTO.Languages;

namespace NSS.Plugin.Misc.SwiftApi.MappingExtensions
{
    public static class LanguageDtoMappings
    {
        public static LanguageDto ToDto(this Language language)
        {
            return language.MapTo<Language, LanguageDto>();
        }
    }
}
