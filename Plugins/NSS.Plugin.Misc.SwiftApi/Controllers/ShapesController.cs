using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NSS.Plugin.Misc.SwiftApi.Attributes;
using NSS.Plugin.Misc.SwiftApi.Controllers;
using NSS.Plugin.Misc.SwiftApi.DTO.Errors;
using NSS.Plugin.Misc.SwiftApi.JSON.ActionResults;
using NSS.Plugin.Misc.SwiftApi.JSON.Serializers;
using NSS.Plugin.Misc.SwiftApi.DTOs.Shapes;
using NSS.Plugin.Misc.SwiftApi.MappingExtensions;
using NSS.Plugin.Misc.SwiftApi.Services;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using System.Collections.Generic;
using System.Net;
using NSS.Plugin.Misc.SwiftCore.Domain.Shapes;
using NSS.Plugin.Misc.SwiftCore.Services;

namespace NSS.Plugin.Misc.SwiftApi.Controllers
{
    public class ShapesController : BaseApiController
    {
        private readonly IShapeService _shapeService;

        public ShapesController (
            IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            IShapeService shapeService) :
            base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService,
                 localizationService, pictureService)
        {
            _shapeService = shapeService;
        }

        [HttpPost]
        [Route("/api/shapes")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ErrorsRootObject), 400)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult CreateShapes(
            ShapesDto shapesDto
            )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            _shapeService.DeleteShapes();

            List<Shape> shapes = ShapeDtoMappings.ToEntity(shapesDto.Shapes);

            _shapeService.InsertShapes(shapes);

            IList<Shape> createdShapes = _shapeService.GetShapes();

            return new RawJsonActionResult(JsonConvert.SerializeObject(ShapeDtoMappings.ToDto((List<Shape>)createdShapes)));
        }

    }
}
