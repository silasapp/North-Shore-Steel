using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using NSS.Plugin.Misc.SwiftApi.Attributes;
using NSS.Plugin.Misc.SwiftApi.Delta;
using NSS.Plugin.Misc.SwiftApi.DTO.Errors;
using NSS.Plugin.Misc.SwiftApi.DTOs.Products;
using NSS.Plugin.Misc.SwiftApi.JSON.Serializers;
using NSS.Plugin.Misc.SwiftApi.ModelBinders;
using NSS.Plugin.Misc.SwiftApiApi.Services;
using NSS.Plugin.Misc.SwiftCore.Configuration;
using NSS.Plugin.Misc.SwiftCore.Services;
using NSS.Plugin.Misc.SwiftCore.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NSS.Plugin.Misc.SwiftApi.Controllers
{
    public class ProductMTRController : BaseApiController
    {
        private readonly CustomGenericAttributeService _genericAttributeService;
        private readonly IStorageService _storageService;
        private readonly IProductApiService _productApiService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        public ProductMTRController(IStoreContext storeContext, ISettingService settingService, IProductApiService productApiService, CustomGenericAttributeService genericAttributeService, IStorageService storageService, IJsonFieldsSerializer jsonFieldsSerializer, IAclService aclService, ICustomerService customerService, IStoreMappingService storeMappingService, IStoreService storeService, IDiscountService discountService, ICustomerActivityService customerActivityService, ILocalizationService localizationService, IPictureService pictureService) : 
            base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService, localizationService, pictureService)
        {
            _genericAttributeService = genericAttributeService;
            _storageService = storageService;
            _productApiService = productApiService;
            _storeContext = storeContext;
            _settingService = settingService;
        }

        [HttpPut]
        [Route("/api/products/{id}/mtr")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorsRootObject), 400)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult CreateCustomerCompany(
            int id, ProductMTRDto mtrDelta
            )
        {
            if (!ModelState.IsValid)
            {
                return Error();
            }

            CustomerActivityService.InsertActivity("APIService", "Starting Product Update", null);

            var product = new Product();

            var attr = _genericAttributeService.GetAttributeByKeyValue(Constants.ItemIdFieldAttribute, id.ToString(), nameof(Product));
            if (attr == null)
                product = null;
            else
                product = _productApiService.GetProductById(attr.EntityId);

            if (product == null)
            {
                return Error(HttpStatusCode.NotFound, "product", "not found");
            }

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var swiftPortalOverrideSettings = _settingService.LoadSetting<SwiftCoreSettings>(storeScope);

            string accountName = swiftPortalOverrideSettings.StorageAccountName;
            string accountKey = swiftPortalOverrideSettings.StorageAccountKey;
            string containerName = swiftPortalOverrideSettings.StorageContainerName;

            if (string.IsNullOrEmpty(accountName) || string.IsNullOrEmpty(accountKey) || string.IsNullOrEmpty(containerName))
                return Error(HttpStatusCode.NotFound, "settings", "storage configuration not found");

            string blobName = string.Format(Constants.MTRBlobName, id);

            byte[] content = Convert.FromBase64String(mtrDelta.MTR);

            var blobUri = _storageService.UploadBlob(accountName, accountKey, containerName, blobName, content, "application/pdf");

            // update customer as NSS Approved
            _genericAttributeService.SaveAttribute(product, Constants.MTRFieldAttribute, blobUri);

            return NoContent();
        }

        [HttpDelete]
        [Route("/api/products/{id}/mtr")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorsRootObject), 400)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult DeleteCustomerCompany(
            int id
            )
        {
            if (!ModelState.IsValid)
            {
                return Error();
            }

            CustomerActivityService.InsertActivity("APIService", "Starting Product MTR Delete", null);

            var product = new Product();

            var attr = _genericAttributeService.GetAttributeByKeyValue(Constants.ItemIdFieldAttribute, id.ToString(), nameof(Product));
            if (attr == null)
                product = null;
            else
                product = _productApiService.GetProductById(attr.EntityId);

            if (product == null)
            {
                return Error(HttpStatusCode.NotFound, "product", "not found");
            }

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var swiftPortalOverrideSettings = _settingService.LoadSetting<SwiftCoreSettings>(storeScope);

            string accountName = swiftPortalOverrideSettings.StorageAccountName;
            string accountKey = swiftPortalOverrideSettings.StorageAccountKey;
            string containerName = swiftPortalOverrideSettings.StorageContainerName;

            if (string.IsNullOrEmpty(accountName) || string.IsNullOrEmpty(accountKey) || string.IsNullOrEmpty(containerName))
                return Error(HttpStatusCode.NotFound, "settings", "storage configuration not found");

            string blobName = string.Format(Constants.MTRBlobName, id);

            _storageService.DeleteBlob(accountName, accountKey, containerName, blobName);

            var attributes = _genericAttributeService.GetAttributesForEntity(product.Id, nameof(Product));

            var mtrAttribute = attributes.FirstOrDefault(x => x.Key == Constants.MTRFieldAttribute);
            if(mtrAttribute != null)
                _genericAttributeService.DeleteAttribute(mtrAttribute);

            return NoContent();
        }
    }
}
