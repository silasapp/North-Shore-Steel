using Microsoft.AspNetCore.Mvc;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using NSS.Plugin.Misc.SwiftApi.Delta;
using NSS.Plugin.Misc.SwiftApi.JSON.Serializers;
using NSS.Plugin.Misc.SwiftApi.ModelBinders;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace NSS.Plugin.Misc.SwiftApi.Controllers
{
    public class UsersController : BaseApiController
    {
        public UsersController(IJsonFieldsSerializer jsonFieldsSerializer, IAclService aclService, ICustomerService customerService, IStoreMappingService storeMappingService, IStoreService storeService, IDiscountService discountService, ICustomerActivityService customerActivityService, ILocalizationService localizationService, IPictureService pictureService) : base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService, localizationService, pictureService)
        {
        }

        //[HttpPost]
        //[Route("/api/users")]
        //[ProducesResponseType(typeof(CustomersRootObject), (int)HttpStatusCode.OK)]
        //[ProducesResponseType(typeof(ErrorsRootObject), 422)]
        //[ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        //public IActionResult CreateCustomer(
        //[ModelBinder(typeof(JsonModelBinder<CustomerDto>))]
        //    Delta<CustomerDto> customerDelta)
        //{
        //}
    }
}
