using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NSS.Plugin.Misc.SwiftApi.Attributes;
using NSS.Plugin.Misc.SwiftApi.Controllers;
using NSS.Plugin.Misc.SwiftApi.DTO.Errors;
using NSS.Plugin.Misc.SwiftApi.JSON.ActionResults;
using NSS.Plugin.Misc.SwiftApi.JSON.Serializers;
using NSS.Plugin.Misc.SwiftApi.DTOs.Shapes;
using NSS.Plugin.Misc.SwiftApi.MappingExtensions;
using NSS.Plugin.Misc.SwiftCore.Helpers;
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
using Nop.Services.Catalog;
using Nop.Core.Domain.Catalog;
using System.Linq;
using Nop.Services.Orders;
using Nop.Core.Domain.Orders;

namespace NSS.Plugin.Misc.SwiftApi.Controllers
{
    public class ShapesController : BaseApiController
    {
        private readonly IShapeService _shapeService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ICheckoutAttributeService _checkoutAttributeService;

        public ShapesController(
            ICheckoutAttributeService checkoutAttributeService,
            IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            IShapeService shapeService,
            ISpecificationAttributeService specificationAttributeService,
            IProductAttributeService productAttributeService) :
            base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService,
                 localizationService, pictureService)
        {
            _shapeService = shapeService;
            _productAttributeService = productAttributeService;
            _specificationAttributeService = specificationAttributeService;
            _checkoutAttributeService = checkoutAttributeService;
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

            // create product attributes

            // cut options
            if (!_productAttributeService.GetAllProductAttributes().Any(x => x.Name == Constants.CutOptionsAttribute))
                _productAttributeService.InsertProductAttribute(new ProductAttribute { Name = Constants.CutOptionsAttribute });
            // work order instructions
            if (!_productAttributeService.GetAllProductAttributes().Any(x => x.Name == Constants.WorkOrderInstructionsAttribute))
                _productAttributeService.InsertProductAttribute(new ProductAttribute { Name = Constants.WorkOrderInstructionsAttribute });
            // tolerance cut
            if (!_productAttributeService.GetAllProductAttributes().Any(x => x.Name == Constants.LengthToleranceCutAttribute))
                _productAttributeService.InsertProductAttribute(new ProductAttribute { Name = Constants.LengthToleranceCutAttribute });
            // cust part no
            if (!_productAttributeService.GetAllProductAttributes().Any(x => x.Name == Constants.CustomerPartNoAttribute))
                _productAttributeService.InsertProductAttribute(new ProductAttribute { Name = Constants.CustomerPartNoAttribute });
            // purchase unit
            if (!_productAttributeService.GetAllProductAttributes().Any(x => x.Name == Constants.PurchaseUnitAttribute))
                _productAttributeService.InsertProductAttribute(new ProductAttribute { Name = Constants.PurchaseUnitAttribute });


            // create spec attributes
            if (!_specificationAttributeService.GetSpecificationAttributes().Any(x => x.Name == Constants.MetalFieldAttribute))
            {
                // metals
                _specificationAttributeService.InsertSpecificationAttribute(new SpecificationAttribute { Name = Constants.MetalFieldAttribute });
                // grades
                _specificationAttributeService.InsertSpecificationAttribute(new SpecificationAttribute { Name = Constants.GradeFieldAttribute });
                // coating
                _specificationAttributeService.InsertSpecificationAttribute(new SpecificationAttribute { Name = Constants.CoatingFieldAttribute });
                //thickness
                _specificationAttributeService.InsertSpecificationAttribute(new SpecificationAttribute { Name = Constants.DisplayThicknessFieldAttribute });
                // condition
                _specificationAttributeService.InsertSpecificationAttribute(new SpecificationAttribute { Name = Constants.ConditionFieldAttribute });
                // countryOfOrigin
                _specificationAttributeService.InsertSpecificationAttribute(new SpecificationAttribute { Name = Constants.CountryOfOriginFieldAttribute });
                // min_width
                _specificationAttributeService.InsertSpecificationAttribute(new SpecificationAttribute { Name = Constants.DisplayWidthFieldAttribute });
            }


            // create checkout attribute
            if (!_checkoutAttributeService.GetAllCheckoutAttributes().Any(x => x.Name == "Purchase Order #"))
            {
                var checkoutAttribute = new CheckoutAttribute { AttributeControlType = AttributeControlType.TextBox, Name = Constants.CheckoutPONoAttribute };
                _checkoutAttributeService.InsertCheckoutAttribute(checkoutAttribute);

                checkoutAttribute = new CheckoutAttribute { AttributeControlType = AttributeControlType.RadioList, IsRequired = true, Name = Constants.CheckoutDeliveryOptionAttribute };
                _checkoutAttributeService.InsertCheckoutAttribute(checkoutAttribute);

                _checkoutAttributeService.InsertCheckoutAttributeValue(new CheckoutAttributeValue { CheckoutAttributeId = checkoutAttribute.Id, Name = "Ship to Customer", DisplayOrder = 1, IsPreSelected = true });
                _checkoutAttributeService.InsertCheckoutAttributeValue(new CheckoutAttributeValue { CheckoutAttributeId = checkoutAttribute.Id, Name = "Pickup from North Shore Steel", DisplayOrder = 2 });
            }

            IList<Shape> createdShapes = _shapeService.GetShapes();

            return new RawJsonActionResult(JsonConvert.SerializeObject(ShapeDtoMappings.ToDto((List<Shape>)createdShapes)));
        }

    }
}
