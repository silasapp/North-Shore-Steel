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
using System.Threading.Tasks;

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
        public async Task<IActionResult> CreateShapes(
            ShapesDto shapesDto
            )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            await _shapeService.DeleteShapesAsync();

            List<Shape> shapes = ShapeDtoMappings.ToEntity(shapesDto.Shapes);

            await _shapeService.InsertShapesAsync(shapes);

            // create product attributes

            // cut options
            if (!(await _productAttributeService.GetAllProductAttributesAsync()).Any(x => x.Name == Constants.CutOptionsAttribute))
                await _productAttributeService.InsertProductAttributeAsync(new ProductAttribute { Name = Constants.CutOptionsAttribute });
            // work order instructions
            if (!(await _productAttributeService.GetAllProductAttributesAsync()).Any(x => x.Name == Constants.WorkOrderInstructionsAttribute))
                await _productAttributeService.InsertProductAttributeAsync(new ProductAttribute { Name = Constants.WorkOrderInstructionsAttribute });            
            // notes
            if (!(await _productAttributeService.GetAllProductAttributesAsync()).Any(x => x.Name == Constants.NotesAttribute))
                await _productAttributeService.InsertProductAttributeAsync(new ProductAttribute { Name = Constants.NotesAttribute });
            // tolerance cut
            if (!(await _productAttributeService.GetAllProductAttributesAsync()).Any(x => x.Name == Constants.LengthToleranceCutAttribute))
                await _productAttributeService.InsertProductAttributeAsync(new ProductAttribute { Name = Constants.LengthToleranceCutAttribute });
            // cust part no
            if (!(await _productAttributeService.GetAllProductAttributesAsync()).Any(x => x.Name == Constants.CustomerPartNoAttribute))
                await _productAttributeService.InsertProductAttributeAsync(new ProductAttribute { Name = Constants.CustomerPartNoAttribute });
            // purchase unit
            if (!(await _productAttributeService.GetAllProductAttributesAsync()).Any(x => x.Name == Constants.PurchaseUnitAttribute))
                await _productAttributeService.InsertProductAttributeAsync(new ProductAttribute { Name = Constants.PurchaseUnitAttribute });


            // create spec attributes
            if (!(await _specificationAttributeService.GetSpecificationAttributesAsync()).Any(x => x.Name == Constants.MetalFieldAttribute))
            {
                // metals
                await _specificationAttributeService.InsertSpecificationAttributeAsync(new SpecificationAttribute { Name = Constants.MetalFieldAttribute });
                // grades
                await _specificationAttributeService.InsertSpecificationAttributeAsync(new SpecificationAttribute { Name = Constants.GradeFieldAttribute });
                // coating
                await _specificationAttributeService.InsertSpecificationAttributeAsync(new SpecificationAttribute { Name = Constants.CoatingFieldAttribute });
                //thickness
                await _specificationAttributeService.InsertSpecificationAttributeAsync(new SpecificationAttribute { Name = Constants.ThicknessFieldAttribute });
                // condition
                await _specificationAttributeService.InsertSpecificationAttributeAsync(new SpecificationAttribute { Name = Constants.ConditionFieldAttribute });
                // countryOfOrigin
                await _specificationAttributeService.InsertSpecificationAttributeAsync(new SpecificationAttribute { Name = Constants.CountryOfOriginFieldAttribute });
                // min_width
                await _specificationAttributeService.InsertSpecificationAttributeAsync(new SpecificationAttribute { Name = Constants.WidthFieldAttribute });
            }


            // create checkout attribute
            if (!(await _checkoutAttributeService.GetAllCheckoutAttributesAsync()).Any(x => x.Name == Constants.CheckoutPONoAttribute))
            {
                var checkoutAttribute = new CheckoutAttribute { AttributeControlType = AttributeControlType.TextBox, Name = Constants.CheckoutPONoAttribute };
                await _checkoutAttributeService.InsertCheckoutAttributeAsync(checkoutAttribute);
            }

            IList<Shape> createdShapes = await _shapeService.GetShapesAsync();

            return new RawJsonActionResult(JsonConvert.SerializeObject(ShapeDtoMappings.ToDto((List<Shape>)createdShapes)));
        }

    }
}
