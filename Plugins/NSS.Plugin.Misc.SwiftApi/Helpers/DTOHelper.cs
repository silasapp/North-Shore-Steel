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
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IGenericAttributeService _genericAttributeService;

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
            IShoppingCartService shoppingCartService,
            IGenericAttributeService genericAttributeService)
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
            _shoppingCartService = shoppingCartService;
            _genericAttributeService = genericAttributeService;
        }

        public ProductDto PrepareProductDTO(Product product)
        {
            var productDto = product.ToDto();

            var productPictures = _productService.GetProductPicturesByProductId(product.Id);
            PrepareProductImages(productPictures, productDto);


            productDto.SeName = _urlRecordService.GetSeName(product);
            productDto.DiscountIds = _discountService.GetAppliedDiscounts(product).Select(discount => discount.Id).ToList();
            productDto.ManufacturerIds = _manufacturerService.GetProductManufacturersByProductId(product.Id).Select(pm => pm.Id).ToList();
            productDto.RoleIds = _aclService.GetAclRecords(product).Select(acl => acl.CustomerRoleId).ToList();
            productDto.StoreIds = _storeMappingService.GetStoreMappings(product).Select(mapping => mapping.StoreId)
                .ToList();
            productDto.Tags = _productTagService.GetAllProductTagsByProductId(product.Id).Select(tag => tag.Name)
                .ToList();

            productDto.AssociatedProductIds =
                _productService.GetAssociatedProducts(product.Id, showHidden: true)
                    .Select(associatedProduct => associatedProduct.Id)
                    .ToList();

            var allLanguages = _languageService.GetAllLanguages();

            productDto.LocalizedNames = new List<LocalizedNameDto>();

            foreach (var language in allLanguages)
            {
                var localizedNameDto = new LocalizedNameDto
                {
                    LanguageId = language.Id,
                    LocalizedName = _localizationService.GetLocalized(product, x => x.Name, language.Id)
                };

                productDto.LocalizedNames.Add(localizedNameDto);
            }

            return productDto;
        }

        private void PrepareProductImages(IEnumerable<ProductPicture> productPictures, ProductDto productDto)
        {
            if (productDto.Images == null)
            {
                productDto.Images = new List<ImageMappingDto>();
            }

            // Here we prepare the resulted dto image.
            foreach (var productPicture in productPictures)
            {
                var imageDto = PrepareImageDto(_pictureService.GetPictureById(productPicture.PictureId));

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

        protected ImageDto PrepareImageDto(Picture picture)
        {
            ImageDto image = null;

            if (picture != null)
            {
                // We don't use the image from the passed dto directly 
                // because the picture may be passed with src and the result should only include the base64 format.
                image = new ImageDto
                {
                    //Attachment = Convert.ToBase64String(picture.PictureBinary),
                    Src = _pictureService.GetPictureUrl(picture.Id)
                };
            }

            return image;
        }

        public LanguageDto PrepareLanguageDto(Language language)
        {
            var languageDto = language.ToDto();

            languageDto.StoreIds = _storeMappingService.GetStoreMappings(language).Select(mapping => mapping.StoreId)
                                                       .ToList();

            if (languageDto.StoreIds.Count == 0)
            {
                languageDto.StoreIds = _storeService.GetAllStores().Select(s => s.Id).ToList();
            }

            return languageDto;
        }

        public ErpProductDto PrepareErpProductDTO(Product product)
        {
            var genericAttr = _genericAttributeService.GetAttributesForEntity(product.Id, nameof(Product));

            var x = new ErpProductDto()
            {

            };

            return x;
        }
    }
        
}
