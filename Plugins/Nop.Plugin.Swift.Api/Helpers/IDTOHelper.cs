using Nop.Core.Domain.Localization;
using Nop.Plugin.Api.DTO.Languages;

namespace Nop.Plugin.Api.Helpers
{
    public interface IDTOHelper
    {
        LanguageDto PrepareLanguageDto(Language language);
    }
}
