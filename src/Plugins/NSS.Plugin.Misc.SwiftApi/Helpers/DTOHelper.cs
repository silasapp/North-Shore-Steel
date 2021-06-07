using System.Linq;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using NSS.Plugin.Misc.SwiftApi.DTO.Languages;
using NSS.Plugin.Misc.SwiftApi.MappingExtensions;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using System.Collections.Generic;
using NSS.Plugin.Misc.SwiftApi.DTO.Products;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using NSS.Plugin.Misc.SwiftApi.DTO.Images;
using NSS.Plugin.Misc.SwiftApi.DTOs.Products;
using Nop.Services.Common;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftApi.Helpers
{
    public class DTOHelper : IDTOHelper
    {
        private readonly IAclService _aclService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly ICustomerService _customerService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly IDiscountService _discountService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IOrderService _orderService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IAddressService _addressService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly IUrlRecordService _urlRecordService;

        public DTOHelper(
            IProductService productService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IPictureService pictureService,
            IProductAttributeService productAttributeService,
            ICustomerService customerApiService,
            IProductAttributeParser productAttributeParser,
            ILanguageService languageService,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            IStoreService storeService,
            ILocalizationService localizationService,
            IUrlRecordService urlRecordService,
            IProductTagService productTagService,
            IDiscountService discountService,
            IManufacturerService manufacturerService,
            IOrderService orderService,
            //IProductAttributeConverter productAttributeConverter,
            IShoppingCartService shoppingCartService,
            IAddressService addressService)
        {
            _productService = productService;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _pictureService = pictureService;
            _productAttributeService = productAttributeService;
            _customerService = customerApiService;
            _productAttributeParser = productAttributeParser;
            _languageService = languageService;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
            _storeService = storeService;
            _localizationService = localizationService;
            _urlRecordService = urlRecordService;
            _productTagService = productTagService;
            _discountService = discountService;
            _manufacturerService = manufacturerService;
            _orderService = orderService;
            //_productAttributeConverter = productAttributeConverter;
            _shoppingCartService = shoppingCartService;
            this._addressService = addressService;
        }

        public async Task<ProductDto> PrepareProductDTOAsync(Product product)
        {
            var productDto = product.ToDto();
            var productPictures = await _productService.GetProductPicturesByProductIdAsync(product.Id);
            await PrepareProductImagesAsync(productPictures, productDto);

            productDto.SeName = await _urlRecordService.GetSeNameAsync(product);
            productDto.DiscountIds = (await _discountService.GetAppliedDiscountsAsync(product)).Select(discount => discount.Id).ToList();
            productDto.ManufacturerIds = (await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id)).Select(pm => pm.Id).ToList();
            productDto.RoleIds = (await _aclService.GetAclRecordsAsync(product)).Select(acl => acl.CustomerRoleId).ToList();
            productDto.StoreIds = (await _storeMappingService.GetStoreMappingsAsync(product)).Select(mapping => mapping.StoreId).ToList();
            productDto.Tags = (await _productTagService.GetAllProductTagsByProductIdAsync(product.Id)).Select(tag => tag.Name).ToList();

            productDto.AssociatedProductIds = (await _productService.GetAssociatedProductsAsync(product.Id, showHidden: true))
                               .Select(associatedProduct => associatedProduct.Id)
                               .ToList();

            var allLanguages = await _languageService.GetAllLanguagesAsync();

            productDto.LocalizedNames = new List<LocalizedNameDto>();

            foreach (var language in allLanguages)
            {
                var localizedNameDto = new LocalizedNameDto
                {
                    LanguageId = language.Id,
                    LocalizedName = await _localizationService.GetLocalizedAsync(product, x => x.Name, language.Id)
                };

                productDto.LocalizedNames.Add(localizedNameDto);
            }

            productDto.RequiredProductIds = _productService.ParseRequiredProductIds(product);

            return productDto;
        }


        public async Task<LanguageDto> PrepareLanguageDtoAsync(Language language)
        {
            var languageDto = language.ToDto();

            languageDto.StoreIds = (await _storeMappingService.GetStoreMappingsAsync(language)).Select(mapping => mapping.StoreId)
                                                       .ToList();

            if (languageDto.StoreIds.Count == 0)
            {
                languageDto.StoreIds = (await _storeService.GetAllStoresAsync()).Select(s => s.Id).ToList();
            }

            return languageDto;
        }


        #region Private methods

        private async Task PrepareProductImagesAsync(IEnumerable<ProductPicture> productPictures, ProductDto productDto)
        {
            if (productDto.Images == null)
            {
                productDto.Images = new List<ImageMappingDto>();
            }

            // Here we prepare the resulted dto image.
            foreach (var productPicture in productPictures)
            {
                var imageDto = await PrepareImageDtoAsync(await _pictureService.GetPictureByIdAsync(productPicture.PictureId));

                if (imageDto != null)
                {
                    var productImageDto = new ImageMappingDto
                    {
                        Id = productPicture.Id,
                        PictureId = productPicture.PictureId,
                        Position = productPicture.DisplayOrder,
                        Src = imageDto.Src,
                        Attachment = imageDto.Attachment
                    };

                    productDto.Images.Add(productImageDto);
                }
            }
        }

        private async Task<ImageDto> PrepareImageDtoAsync(Picture picture)
        {
            ImageDto image = null;

            if (picture != null)
            {
                (string url, _) = await _pictureService.GetPictureUrlAsync(picture);

                // We don't use the image from the passed dto directly 
                // because the picture may be passed with src and the result should only include the base64 format.
                image = new ImageDto
                {
                    //Attachment = Convert.ToBase64String(picture.PictureBinary),
                    Src = url
                };
            }

            return image;
        }

        #endregion
    }

}
