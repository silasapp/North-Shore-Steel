using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using NSS.Plugin.Misc.SwiftCore.Helpers;
using NSS.Plugin.Misc.SwiftApi.Delta;
using NSS.Plugin.Misc.SwiftApi.DTO.Errors;
using NSS.Plugin.Misc.SwiftApi.DTO.Products;
using NSS.Plugin.Misc.SwiftApi.JSON.ActionResults;
using NSS.Plugin.Misc.SwiftApi.JSON.Serializers;
using NSS.Plugin.Misc.SwiftApi.ModelBinders;
using System;
using System.Collections.Generic;
using System.Net;
using Nop.Services.Catalog;
using Nop.Services.Seo;
using System.Linq;
using static NSS.Plugin.Misc.SwiftApi.Infrastructure.Constants;
using NSS.Plugin.Misc.SwiftApi.Helpers;
using NSS.Plugin.Misc.SwiftApi.Factories;
using NSS.Plugin.Misc.SwiftApiApi.Services;
using NSS.Plugin.Misc.SwiftApi.DTO.Images;
using Nop.Core.Domain.Discounts;
using NSS.Plugin.Misc.SwiftApi.DTOs.Products;
using System.Reflection;
using Newtonsoft.Json;
using NSS.Plugin.Misc.SwiftCore.Services;
using Nop.Services.Common;
using Nop.Services.Tax;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftApi.Controllers
{
    public class ProductsController : BaseApiController
    {
        private readonly IProductApiService _productApiService;
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IFactory<Product> _factory;
        private readonly IProductTagService _productTagService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IDTOHelper _dtoHelper;
        private readonly ILogger _logger;
        private readonly IShapeService _shapeService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly CustomGenericAttributeService _genericAttributeService;
        private readonly ITaxCategoryService _taxCategoryService;

        private const string MATERIAL_TAX_NAME = "PM020760";

        public ProductsController(ITaxCategoryService taxCategoryService, ISpecificationAttributeService specificationAttributeService, CustomGenericAttributeService genericAttributeService, IShapeService shapeService, IProductService productService, IProductApiService productApiService, IFactory<Product> factory,
            IManufacturerService manufacturerService, IProductTagService productTagService, IUrlRecordService urlRecordService,
            IProductAttributeService productAttributeService, ILogger logger, IDTOHelper dtoHelper,
            IJsonFieldsSerializer jsonFieldsSerializer, IAclService aclService, ICustomerService customerService,
            IStoreMappingService storeMappingService, IStoreService storeService, IDiscountService discountService,
            ICustomerActivityService customerActivityService, ILocalizationService localizationService, IPictureService pictureService) :
            base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService, localizationService, pictureService)
        {
            _productApiService = productApiService;
            _factory = factory;
            _manufacturerService = manufacturerService;
            _productTagService = productTagService;
            _urlRecordService = urlRecordService;
            _productService = productService;
            _productAttributeService = productAttributeService;
            _logger = logger;
            _dtoHelper = dtoHelper;
            _shapeService = shapeService;
            _genericAttributeService = genericAttributeService;
            _specificationAttributeService = specificationAttributeService;
            _taxCategoryService = taxCategoryService;
        }

        [HttpPost]
        [Route("/api/products")]
        [ProducesResponseType(typeof(ErpProductDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        public async Task<IActionResult> CreateProduct([ModelBinder(typeof(JsonModelBinder<ErpProductDto>))] Delta<ErpProductDto> erpProductDelta)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }

            await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, $"Swift API - CreateProduct - itemId = {erpProductDelta.Dto.itemId}", $"request => {JsonConvert.SerializeObject(erpProductDelta.Dto)}");

            var attr = await _genericAttributeService.GetAttributeByKeyValueAsync("itemId", erpProductDelta.Dto.itemId.ToString(), nameof(Product));
            if (attr != null)
            {
                var existingProduct = _productApiService.GetProductById(attr.EntityId);

                if (existingProduct != null)
                    return await UpdateErpProductAsync(erpProductDelta, existingProduct);
            }

            // check if shapes exist and if required attributes have value
            var shape = await _shapeService.GetShapeByIdAsync(erpProductDelta.Dto.shapeId);

            if (shape == null)
                return Error(HttpStatusCode.NotFound, "error", "No shape found for the Shape Id specified.");

            var request = erpProductDelta.ObjectPropertyNameValuePairs.FirstOrDefault();
            var requestData = (Dictionary<string, object>)request.Value;

            foreach (var attribute in (shape.Parent == null ? shape.Atttributes : shape.Parent.Atttributes))
            {
                if (!requestData.TryGetValue(attribute.Sort, out var value))
                {
                    if (attribute.Sort?.ToLower() != "millname" && attribute.Sort?.ToLower() != "countryoforigin")
                        ModelState.AddModelError(nameof(attribute), $"{attribute.Sort} attribute requires a value.");
                }
                else
                {
                    if (attribute.Sort?.ToLower() != "millname" && attribute.Sort?.ToLower() != "countryoforigin")
                    {
                        if (string.IsNullOrWhiteSpace(value?.ToString()))
                            ModelState.AddModelError(nameof(attribute), $"{attribute.Sort} attribute requires a value.");
                    }
                }     
            }

            // serialized product field check
            if (erpProductDelta.Dto.serialized && string.IsNullOrWhiteSpace(erpProductDelta.Dto.itemTagNo))
                ModelState.AddModelError("itemTagNo", $"itemTagNo field requires a value for serialized products");

            // price check
            if (erpProductDelta.Dto.pricePerCWT == null && erpProductDelta.Dto.pricePerFt == null && erpProductDelta.Dto.pricePerPiece == null)
                ModelState.AddModelError("price", $"at least one price item must not be null");


            if (ModelState.ErrorCount > 0)
            {
                return BadRequest(ModelState);
            }

            await CustomerActivityService.InsertActivityAsync("APIService", "Starting Product Create", null);

            // Inserting the new product
            var product = await _factory.InitializeAsync();

            product.Name = erpProductDelta.Dto.itemName;
            product.Length = erpProductDelta.Dto.lengthFt ?? decimal.Zero;
            product.Height = erpProductDelta.Dto.height ?? decimal.Zero;
            product.Width = erpProductDelta.Dto.width ?? decimal.Zero;
            product.Weight = erpProductDelta.Dto.weight ?? decimal.Zero;
            product.Price = erpProductDelta.Dto.pricePerPiece ?? product.Price;
            product.Published = erpProductDelta.Dto.visible;

            // serialized product
            if (erpProductDelta.Dto.serialized)
            {
                product.Sku = erpProductDelta.Dto.itemTagNo;
                product.ManageInventoryMethod = ManageInventoryMethod.ManageStock;
                product.StockQuantity = erpProductDelta.Dto.quantity ?? 0;
            }
            else
            {
                product.Sku = erpProductDelta.Dto.itemNo;
                product.ManageInventoryMethod = ManageInventoryMethod.DontManageStock;
                product.OrderMaximumQuantity = erpProductDelta.Dto.quantity ?? 0;
            }

            // if an item is out of stock - unpublish
            product.LowStockActivity = LowStockActivity.Unpublish;

            // associate to material taxid
            var taxCategories = await _taxCategoryService.GetAllTaxCategoriesAsync();
            var materialTaxCategory = taxCategories.FirstOrDefault(x => x.Name == MATERIAL_TAX_NAME);

            product.TaxCategoryId = materialTaxCategory?.Id ?? 0;

            await _productService.InsertProductAsync(product);

            foreach (var attribute in requestData)
            {
                // save generic data
                await _genericAttributeService.SaveAttributeAsync(product, attribute.Key.ToString(), attribute.Value);
            }

            //search engine name
            var seName = await _urlRecordService.ValidateSeNameAsync(product, null, product.Name, true);
            await _urlRecordService.SaveSlugAsync(product, seName, 0);

            // create product attr
            await MapProductAttributesAsync(product);

            // create spec option for metals, coating, grades, thickness, condition, country_of_origin, min width 
            await MapProductSpecificationAttributeOptionAsync(product, requestData);

            await _productService.UpdateProductAsync(product);
            
            await CustomerActivityService.InsertActivityAsync("APIService", await LocalizationService.GetResourceAsync("ActivityLog.AddNewProduct"), product);

            // Preparing the result dto of the new product
            var response = await GetErpProductAsync (product);

            var json = JsonFieldsSerializer.Serialize(response, string.Empty);

            return new RawJsonActionResult(json);
        }

        [HttpGet]
        [Route("/api/products/{id}")]
        [ProducesResponseType(typeof(ErpProductDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetProductById(int id)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }

            await CustomerActivityService.InsertActivityAsync("APIService", "Starting Product get By Id", null);

            var product = new Product();

            var attr = await _genericAttributeService.GetAttributeByKeyValueAsync("itemId", id.ToString(), nameof(Product));
            if (attr == null)
                product = null;
            else
                product = _productApiService.GetProductById(attr.EntityId);

            if (product == null)
            {
                return Error(HttpStatusCode.NotFound, "product", "not found");
            }

            var response = await GetErpProductAsync (product);

            await CustomerActivityService.InsertActivityAsync("APIService", "Finished Get Product by Id", product);

            var json = JsonFieldsSerializer.Serialize(response, string.Empty);

            return new RawJsonActionResult(json);
        }

        [HttpPut]
        [Route("/api/products/{id}")]
        [ProducesResponseType(typeof(ErpProductDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateProduct([ModelBinder(typeof(JsonModelBinder<ErpProductDto>))] Delta<ErpProductDto> erpProductDelta)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }

            var product = new Product();

            var attr = await _genericAttributeService.GetAttributeByKeyValueAsync("itemId", erpProductDelta.Dto.Id.ToString(), nameof(Product));
            if (attr == null)
                product = null;
            else
                product = _productApiService.GetProductById(attr.EntityId);

            if (product == null)
            {
                return Error(HttpStatusCode.NotFound, "product", "not found");
            }

            return await UpdateErpProductAsync(erpProductDelta, product);
        }

        private async Task<IActionResult> UpdateErpProductAsync(Delta<ErpProductDto> erpProductDelta, Product product)
        {
            await CustomerActivityService.InsertActivityAsync("APIService", "Starting Product Update", null);

            var request = (Dictionary<string, object>)erpProductDelta.ObjectPropertyNameValuePairs.FirstOrDefault().Value;

            if (request.ContainsKey("shapeId"))
            {
                // check if shapes exist and if required attributes have value
                var shape = _shapeService.GetShapeByIdAsync(erpProductDelta.Dto.shapeId);

                if (shape == null)
                    return Error(HttpStatusCode.NotFound, "shape", "not found");
            }

            product.Name = request.ContainsKey("itemName") ? erpProductDelta.Dto.itemName : product.Name;
            product.Length = request.ContainsKey("lengthFt") ? erpProductDelta.Dto.lengthFt ?? decimal.Zero : product.Length;
            product.Height = request.ContainsKey("height") ? erpProductDelta.Dto.height ?? decimal.Zero : product.Height;
            product.Width = request.ContainsKey("width") ? erpProductDelta.Dto.width ?? decimal.Zero : product.Width;
            product.Weight = request.ContainsKey("weight") ? erpProductDelta.Dto.weight ?? decimal.Zero : product.Weight;
            product.Price = request.ContainsKey("pricePerPiece") ? erpProductDelta.Dto.pricePerPiece ?? product.Price : product.Price;
            product.Published = request.ContainsKey("visible") ? erpProductDelta.Dto.visible : product.Published;

            var isSerialized = await _genericAttributeService.GetAttributeAsync<bool>(product, "serialized", defaultValue: false);
            isSerialized = request.ContainsKey("serialized") ? erpProductDelta.Dto.serialized : isSerialized;
            if (isSerialized)
            {
                product.Sku = request.ContainsKey("itemTagNo") ? erpProductDelta.Dto.itemTagNo : product.Sku;
                product.ManageInventoryMethod = ManageInventoryMethod.ManageStock;
                product.StockQuantity = request.ContainsKey("quantity") ? erpProductDelta.Dto.quantity ?? 0 : product.StockQuantity;
            }
            else
            {
                product.Sku = request.ContainsKey("itemNo") ? erpProductDelta.Dto.itemNo : product.Sku;
                product.ManageInventoryMethod = ManageInventoryMethod.DontManageStock;
                product.OrderMaximumQuantity = request.ContainsKey("quantity") ? erpProductDelta.Dto.quantity ?? 0 : product.OrderMaximumQuantity;
            }

            product.UpdatedOnUtc = DateTime.UtcNow;
            await _productService.UpdateProductAsync(product);

            foreach (var attribute in request)
            {
                if (attribute.Key == "Id" || attribute.Key == "itemId")
                    continue;
                // save generic data
                await _genericAttributeService.SaveAttributeAsync(product, attribute.Key.ToString(), attribute.Value);
            }

            await MapProductAttributesAsync(product);

            await CustomerActivityService.InsertActivityAsync("APIService", await LocalizationService.GetResourceAsync("ActivityLog.UpdateProduct"), product);

            var response = await GetErpProductAsync(product);

            var json = JsonFieldsSerializer.Serialize(response, string.Empty);

            return new RawJsonActionResult(json);
        }


        #region Attribute Methods
        private async Task MapProductSpecificationAttributeOptionAsync(Product entity, Dictionary<string, object> data)
        {
            // shapeId
            data.TryGetValue(Constants.ShapeFieldAttribute, out var shapeId);
            // get value option id for maping
            var attributes = await _specificationAttributeService.GetSpecificationAttributesAsync();

            foreach (var attr in attributes)
            {
                var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(attr.Id);

                bool containsValue = data.TryGetValue(attr.Name, out object value);

                if (containsValue && !string.IsNullOrEmpty(value?.ToString()))
                {
                    string stringVal = value.ToString().Trim();

                    // not plate individual shape id - 13 should have metal and coating filter omly
                    if (!shapeId?.ToString()?.StartsWith("13") ?? false)
                    {
                        if (attr.Name == Constants.CoatingFieldAttribute || attr.Name == Constants.MetalFieldAttribute)
                            CreateProductSpecFilter(entity, attr, options, stringVal);
                    }
                    else
                    {
                        if (attr.Name == Constants.CountryOfOriginFieldAttribute)
                        {
                            CreateProductSpecFilter(entity, attr, options, stringVal.ToUpper());
                        }
                        else if (attr.Name == Constants.WidthFieldAttribute)
                        {
                            if (int.TryParse(stringVal, out int width))
                            {
                                // min width - 36, 48, 60, 72
                                if (width >= 36)
                                {
                                    CreateProductSpecFilter(entity, attr, options, "36");
                                }

                                if (width >= 48)
                                {
                                    CreateProductSpecFilter(entity, attr, options, "48");
                                }

                                if (width >= 60)
                                {
                                    CreateProductSpecFilter(entity, attr, options, "60");
                                }

                                if (width >= 72)
                                {
                                    CreateProductSpecFilter(entity, attr, options, "72");
                                }
                            }
                        }
                        else
                        {
                            CreateProductSpecFilter(entity, attr, options, stringVal);
                        }
                    }

                   
                }
            }
        }

        private async Task CreateProductSpecFilter(Product entity, SpecificationAttribute attr, IList<SpecificationAttributeOption> options, string optionName)
        {
            var option = options.FirstOrDefault(x => x.Name == optionName);

            if (option == null)
            {
                //create option
                option = new SpecificationAttributeOption { Name = optionName, SpecificationAttributeId = attr.Id };
                await _specificationAttributeService.InsertSpecificationAttributeOptionAsync(option);
            }

            //map
            var prodSpec = new ProductSpecificationAttribute
            {
                AllowFiltering = true,
                ProductId = entity.Id,
                SpecificationAttributeOptionId = option.Id,
                AttributeType = SpecificationAttributeType.Option,
                ShowOnProductPage = true
            };
            await _specificationAttributeService.InsertProductSpecificationAttributeAsync(prodSpec);
        }

        private async Task MapProductAttributesAsync(Product product)
        {
            var productAttributes = await _productAttributeService.GetAllProductAttributesAsync();

            var attributeMappings = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
            foreach (var productAttribute in productAttributes)
            {
                bool hasMapping = attributeMappings.Any(x => x.ProductAttributeId == productAttribute.Id);

                if (!hasMapping)
                {
                    // insert new

                    if (productAttribute.Name == Constants.CutOptionsAttribute)
                    {
                        var attributeMapping = new ProductAttributeMapping { AttributeControlType = AttributeControlType.RadioList, ProductAttributeId = productAttribute.Id, ProductId = product.Id };
                        await _productAttributeService.InsertProductAttributeMappingAsync(attributeMapping);

                        // options
                        await _productAttributeService.InsertProductAttributeValueAsync(new ProductAttributeValue { AttributeValueType = AttributeValueType.Simple, Name = "None", ProductAttributeMappingId = attributeMapping.Id, DisplayOrder = 0, IsPreSelected = true});
                        await _productAttributeService.InsertProductAttributeValueAsync(new ProductAttributeValue { AttributeValueType = AttributeValueType.Simple, Name = "Saw in half", ProductAttributeMappingId = attributeMapping.Id, DisplayOrder = 1});
                        await _productAttributeService.InsertProductAttributeValueAsync(new ProductAttributeValue { AttributeValueType = AttributeValueType.Simple, Name = "Saw in thirds", ProductAttributeMappingId = attributeMapping.Id, DisplayOrder = 2 });
                        await _productAttributeService.InsertProductAttributeValueAsync(new ProductAttributeValue { AttributeValueType = AttributeValueType.Simple, Name = "Saw in quarters", ProductAttributeMappingId = attributeMapping.Id, DisplayOrder = 3 });
                        await _productAttributeService.InsertProductAttributeValueAsync(new ProductAttributeValue { AttributeValueType = AttributeValueType.Simple, Name = "Other", ProductAttributeMappingId = attributeMapping.Id, DisplayOrder = 4 });

                    }
                    else if (productAttribute.Name == Constants.WorkOrderInstructionsAttribute)
                    {
                        var attributeMapping = new ProductAttributeMapping { AttributeControlType = AttributeControlType.MultilineTextbox, ProductAttributeId = productAttribute.Id, ProductId = product.Id, ValidationMaxLength = 100 };
                        await _productAttributeService.InsertProductAttributeMappingAsync(attributeMapping);
                    }
                    else if (productAttribute.Name == Constants.LengthToleranceCutAttribute)
                    {
                        var attributeMapping = new ProductAttributeMapping { AttributeControlType = AttributeControlType.TextBox, ProductAttributeId = productAttribute.Id, ProductId = product.Id, ValidationMaxLength = 100 };
                        await _productAttributeService.InsertProductAttributeMappingAsync(attributeMapping);
                    }
                    else if (productAttribute.Name == Constants.NotesAttribute)
                    {
                        var attributeMapping = new ProductAttributeMapping { AttributeControlType = AttributeControlType.TextBox, ProductAttributeId = productAttribute.Id, ProductId = product.Id, ValidationMaxLength = 100 };
                        await _productAttributeService.InsertProductAttributeMappingAsync(attributeMapping);
                    }
                    else if (productAttribute.Name == Constants.CustomerPartNoAttribute)
                    {
                        var attributeMapping = new ProductAttributeMapping { AttributeControlType = AttributeControlType.TextBox, ProductAttributeId = productAttribute.Id, ProductId = product.Id, ValidationMaxLength = 100 };
                        await _productAttributeService.InsertProductAttributeMappingAsync(attributeMapping);
                    }
                    else if (productAttribute.Name == Constants.PurchaseUnitAttribute)
                    {
                        var attributeMapping = new ProductAttributeMapping { AttributeControlType = AttributeControlType.DropdownList, ProductAttributeId = productAttribute.Id, ProductId = product.Id };
                        await _productAttributeService.InsertProductAttributeMappingAsync(attributeMapping);

                        // options
                        await _productAttributeService.InsertProductAttributeValueAsync(new ProductAttributeValue { AttributeValueType = AttributeValueType.Simple, Name = Constants.UnitPerPieceField, ProductAttributeMappingId = attributeMapping.Id, DisplayOrder = 1, IsPreSelected = true });
                        await _productAttributeService.InsertProductAttributeValueAsync(new ProductAttributeValue { AttributeValueType = AttributeValueType.Simple, Name = Constants.UnitPerWeightField, ProductAttributeMappingId = attributeMapping.Id, DisplayOrder = 2 });
                        await _productAttributeService.InsertProductAttributeValueAsync(new ProductAttributeValue { AttributeValueType = AttributeValueType.Simple, Name = Constants.UnitPerFtField, ProductAttributeMappingId = attributeMapping.Id, DisplayOrder = 3 });
                    }
                }

            }
        }

        #endregion

        #region API Methods

        private async Task<ErpProductDto> GetErpProductAsync(Product product)
        {
            // Preparing the result dto of the new product
            var genericAttr = await _genericAttributeService.GetAttributesForEntityAsync(product.Id, nameof(Product));

            var keyValuePair = new Dictionary<string, object>();
            foreach (var item in genericAttr)
            {
                keyValuePair.Add(item.Key, item.Value);
            }

            var response = new Delta<ErpProductDto>(keyValuePair);
            // add product id
            response.Dto.Id = product.Id;
            return response.Dto;
        }

        public static string GetFields(Type modelType)
        {
            return string.Join(",",
                modelType.GetProperties()
                         .Select(p => p.GetCustomAttribute<JsonPropertyAttribute>())
                         .Select(jp => jp.PropertyName));
        }

        #endregion
    }
}
