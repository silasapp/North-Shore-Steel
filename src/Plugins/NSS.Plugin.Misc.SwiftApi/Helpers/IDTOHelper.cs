using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using NSS.Plugin.Misc.SwiftApi.DTO.Languages;
using NSS.Plugin.Misc.SwiftApi.DTO.Products;
using NSS.Plugin.Misc.SwiftApi.DTOs.Products;

namespace NSS.Plugin.Misc.SwiftApi.Helpers
{
    public interface IDTOHelper
    {
        Task<LanguageDto> PrepareLanguageDtoAsync(Language language);
        Task<ProductDto> PrepareProductDTOAsync(Product product);
    }
}
