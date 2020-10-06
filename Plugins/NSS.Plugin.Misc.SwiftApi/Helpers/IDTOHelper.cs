using Nop.Core.Domain.Localization;
using NSS.Plugin.Misc.SwiftApi.DTO.Languages;

namespace NSS.Plugin.Misc.SwiftApi.Helpers
{
    public interface IDTOHelper
    {
        LanguageDto PrepareLanguageDto(Language language);
    }
}
