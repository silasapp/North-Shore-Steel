using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
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
        private readonly CustomGenericAttributeService _genericAttributeService;

        public ProductsController(CustomGenericAttributeService genericAttributeService, IShapeService shapeService, IProductService productService, IProductApiService productApiService, IFactory<Product> factory,
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
        }

        [HttpPost]
        [Route("/api/products")]
        [ProducesResponseType(typeof(ErpProductDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        public IActionResult CreateProduct([ModelBinder(typeof(JsonModelBinder<ErpProductDto>))] Delta<ErpProductDto> erpProductDelta)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }

            var attr = _genericAttributeService.GetAttributeByKeyValue("itemId", erpProductDelta.Dto.itemId.ToString(), nameof(Product));
            if (attr != null)
            {
                var existingProduct = _productApiService.GetProductById(attr.EntityId);

                if(existingProduct != null)
                    return Error(HttpStatusCode.BadRequest, "product", "duplicate product");
            }
                
            // check if shapes exist and if required attributes have value
            var shape = _shapeService.GetShapeById(erpProductDelta.Dto.shapeId);

            if (shape == null)
                return Error(HttpStatusCode.NotFound, "error", "No shape found for the Shape Id specified.");

            var request = erpProductDelta.ObjectPropertyNameValuePairs.FirstOrDefault();
            var requestData = (Dictionary<string,object>)request.Value;

            foreach (var attribute in (shape.Parent == null ? shape.Atttributes : shape.Parent.Atttributes))
            {
                if (!requestData.TryGetValue(attribute.Sort, out var value))
                    ModelState.AddModelError(nameof(attribute), $"{attribute.Sort} attribute requires a value.");
                else
                    if (string.IsNullOrWhiteSpace(value?.ToString()))
                        ModelState.AddModelError(nameof(attribute), $"{attribute.Sort} attribute requires a value.");
            }

            // serialized product field check
            if (erpProductDelta.Dto.serialized && string.IsNullOrWhiteSpace(erpProductDelta.Dto.itemTagNo))
                ModelState.AddModelError("ItemTagNo", $"ItemTagNo field requires a value");


            if (ModelState.ErrorCount > 0)
            {
                return BadRequest(ModelState);
            }

            CustomerActivityService.InsertActivity("APIService", "Starting Product Create", null);

            // Inserting the new product
            var product = _factory.Initialize();

            product.Name = erpProductDelta.Dto.itemName;
            product.Length = erpProductDelta.Dto.width;
            product.Height = erpProductDelta.Dto.height;
            product.Width = erpProductDelta.Dto.width;
            product.Weight = erpProductDelta.Dto.weight;
            if (erpProductDelta.Dto.serialized)
            {
                product.Sku = erpProductDelta.Dto.itemTagNo;
                product.ManageInventoryMethod = ManageInventoryMethod.ManageStock;
                product.StockQuantity = erpProductDelta.Dto.quantity;
            }
            else
            {
                product.Sku = erpProductDelta.Dto.itemNo.ToString();
                product.ManageInventoryMethod = ManageInventoryMethod.DontManageStock;
                product.OrderMaximumQuantity = erpProductDelta.Dto.quantity;
            }

            _productService.InsertProduct(product);

            foreach (var attribute in requestData)
            {
                // save generic data
                if (attribute.Value != null)
                    _genericAttributeService.SaveAttribute(product, attribute.Key.ToString(), attribute.Value);
            }

            //search engine name
            var seName = _urlRecordService.ValidateSeName(product, null, product.Name, true);
            _urlRecordService.SaveSlug(product, seName, 0);

            _productService.UpdateProduct(product);

            CustomerActivityService.InsertActivity("APIService", LocalizationService.GetResource("ActivityLog.AddNewProduct"), product);

            // Preparing the result dto of the new product
            var response = GetErpProduct(product);

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
        public IActionResult UpdateProduct([ModelBinder(typeof(JsonModelBinder<ErpProductDto>))] Delta<ErpProductDto> erpProductDelta)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }
            CustomerActivityService.InsertActivity("APIService", "Starting Product Update", null);

            var product = new Product();

            var attr = _genericAttributeService.GetAttributeByKeyValue("itemId", erpProductDelta.Dto.Id.ToString(), nameof(Product));
            if (attr == null)
                product = null;
            else
                product = _productApiService.GetProductById(attr.EntityId);


            if (product == null)
            {
                return Error(HttpStatusCode.NotFound, "product", "not found");
            }

            var request = (Dictionary<string, object>)erpProductDelta.ObjectPropertyNameValuePairs.FirstOrDefault().Value;

            if (request.ContainsKey("shapeId"))
            {
                // check if shapes exist and if required attributes have value
                var shape = _shapeService.GetShapeById(erpProductDelta.Dto.shapeId);

                if (shape == null)
                    return Error(HttpStatusCode.NotFound, "shape", "not found");
            }

            product.Name = request.ContainsKey("itemName") ? erpProductDelta.Dto.itemName : product.Name;
            product.Height = request.ContainsKey("height") ? erpProductDelta.Dto.height : product.Height;
            product.Width = request.ContainsKey("width") ? erpProductDelta.Dto.width : product.Width;
            product.Weight = request.ContainsKey("weight") ? erpProductDelta.Dto.weight : product.Weight;
            if (request.ContainsKey("serialized"))
            {
                if (erpProductDelta.Dto.serialized)
                {
                    product.Sku = request.ContainsKey("itemTagNo") ? erpProductDelta.Dto.itemTagNo : product.Sku;
                    product.ManageInventoryMethod = ManageInventoryMethod.ManageStock;
                    product.StockQuantity = request.ContainsKey("quantity") ? erpProductDelta.Dto.quantity : product.StockQuantity;
                }
                else
                {
                    product.Sku = request.ContainsKey("itemNo") ? erpProductDelta.Dto.itemNo.ToString() : product.Sku;
                    product.ManageInventoryMethod = ManageInventoryMethod.DontManageStock;
                    product.OrderMaximumQuantity = request.ContainsKey("quantity") ? erpProductDelta.Dto.quantity : product.OrderMaximumQuantity;
                }
            }

            product.UpdatedOnUtc = DateTime.UtcNow;
            _productService.UpdateProduct(product);

            foreach (var attribute in request)
            {
                if (attribute.Key == "Id" || attribute.Key == "itemId")
                    continue;
                // save generic data
                if (attribute.Value != null)
                    _genericAttributeService.SaveAttribute(product, attribute.Key.ToString(), attribute.Value);
            }

            CustomerActivityService.InsertActivity("APIService", LocalizationService.GetResource("ActivityLog.UpdateProduct"), product);

            var response = GetErpProduct(product);

            var json = JsonFieldsSerializer.Serialize(response, string.Empty);

            return new RawJsonActionResult(json);
        }

        private ErpProductDto GetErpProduct(Product product)
        {
            // Preparing the result dto of the new product
            var genericAttr = _genericAttributeService.GetAttributesForEntity(product.Id, nameof(Product));

            var keyValuePair = new Dictionary<string, object>();
            foreach (var item in genericAttr)
            {
                keyValuePair.Add(item.Key, item.Value);
            }

            var response = new Delta<ErpProductDto>(keyValuePair);
            return response.Dto;
        }

        public static string GetFields(Type modelType)
        {
            return string.Join(",",
                modelType.GetProperties()
                         .Select(p => p.GetCustomAttribute<JsonPropertyAttribute>())
                         .Select(jp => jp.PropertyName));
        }

        private void UpdateProductPictures(Product entityToUpdate, List<ImageMappingDto> setPictures)
        {
            // If no pictures are specified means we don't have to update anything
            if (setPictures == null)
            {
                return;
            }

            // delete unused product pictures
            var productPictures = _productService.GetProductPicturesByProductId(entityToUpdate.Id);
            var unusedProductPictures = productPictures.Where(x => setPictures.All(y => y.Id != x.Id)).ToList();
            foreach (var unusedProductPicture in unusedProductPictures)
            {
                var picture = PictureService.GetPictureById(unusedProductPicture.PictureId);
                if (picture == null)
                {
                    throw new ArgumentException("No picture found with the specified id");
                }
                PictureService.DeletePicture(picture);
            }

            foreach (var imageDto in setPictures)
            {
                if (imageDto.Id > 0)
                {
                    // update existing product picture
                    var productPictureToUpdate = productPictures.FirstOrDefault(x => x.Id == imageDto.Id);
                    if (productPictureToUpdate != null && imageDto.Position > 0)
                    {
                        productPictureToUpdate.DisplayOrder = imageDto.Position;
                        _productService.UpdateProductPicture(productPictureToUpdate);
                    }
                }
                else
                {
                    // add new product picture
                    var newPicture = PictureService.InsertPicture(imageDto.Binary, imageDto.MimeType, string.Empty);
                    _productService.InsertProductPicture(new ProductPicture
                    {
                        PictureId = newPicture.Id,
                        ProductId = entityToUpdate.Id,
                        DisplayOrder = imageDto.Position
                    });
                }
            }
        }

        private void UpdateProductAttributes(Product entityToUpdate, Delta<ProductDto> productDtoDelta)
        {
            // If no product attribute mappings are specified means we don't have to update anything
            if (productDtoDelta.Dto.ProductAttributeMappings == null)
            {
                return;
            }

            // delete unused product attribute mappings
            var toBeUpdatedIds = productDtoDelta.Dto.ProductAttributeMappings.Where(y => y.Id != 0).Select(x => x.Id);
            var productAttributeMappings = _productAttributeService.GetProductAttributeMappingsByProductId(entityToUpdate.Id);
            var unusedProductAttributeMappings = productAttributeMappings.Where(x => !toBeUpdatedIds.Contains(x.Id)).ToList();

            foreach (var unusedProductAttributeMapping in unusedProductAttributeMappings)
            {
                _productAttributeService.DeleteProductAttributeMapping(unusedProductAttributeMapping);
            }

            foreach (var productAttributeMappingDto in productDtoDelta.Dto.ProductAttributeMappings)
            {
                if (productAttributeMappingDto.Id > 0)
                {
                    // update existing product attribute mapping
                    var productAttributeMappingToUpdate = productAttributeMappings.FirstOrDefault(x => x.Id == productAttributeMappingDto.Id);
                    if (productAttributeMappingToUpdate != null)
                    {
                        productDtoDelta.Merge(productAttributeMappingDto, productAttributeMappingToUpdate, false);

                        _productAttributeService.UpdateProductAttributeMapping(productAttributeMappingToUpdate);

                        UpdateProductAttributeValues(productAttributeMappingDto, productDtoDelta);
                    }
                }
                else
                {
                    var newProductAttributeMapping = new ProductAttributeMapping
                    {
                        ProductId = entityToUpdate.Id
                    };

                    productDtoDelta.Merge(productAttributeMappingDto, newProductAttributeMapping);

                    // add new product attribute
                    _productAttributeService.InsertProductAttributeMapping(newProductAttributeMapping);
                }
            }
        }

        private void UpdateProductAttributeValues(ProductAttributeMappingDto productAttributeMappingDto, Delta<ProductDto> productDtoDelta)
        {
            // If no product attribute values are specified means we don't have to update anything
            if (productAttributeMappingDto.ProductAttributeValues == null)
                return;

            // delete unused product attribute values
            var toBeUpdatedIds = productAttributeMappingDto.ProductAttributeValues.Where(y => y.Id != 0).Select(x => x.Id);

            var unusedProductAttributeValues =
                _productAttributeService.GetProductAttributeValues(productAttributeMappingDto.Id).Where(x => !toBeUpdatedIds.Contains(x.Id)).ToList();

            foreach (var unusedProductAttributeValue in unusedProductAttributeValues)
            {
                _productAttributeService.DeleteProductAttributeValue(unusedProductAttributeValue);
            }

            foreach (var productAttributeValueDto in productAttributeMappingDto.ProductAttributeValues)
            {
                if (productAttributeValueDto.Id > 0)
                {
                    // update existing product attribute mapping
                    var productAttributeValueToUpdate =
                        _productAttributeService.GetProductAttributeValueById(productAttributeValueDto.Id);
                    if (productAttributeValueToUpdate != null)
                    {
                        productDtoDelta.Merge(productAttributeValueDto, productAttributeValueToUpdate, false);

                        _productAttributeService.UpdateProductAttributeValue(productAttributeValueToUpdate);
                    }
                }
                else
                {
                    var newProductAttributeValue = new ProductAttributeValue();
                    productDtoDelta.Merge(productAttributeValueDto, newProductAttributeValue);

                    newProductAttributeValue.ProductAttributeMappingId = productAttributeMappingDto.Id;
                    // add new product attribute value
                    _productAttributeService.InsertProductAttributeValue(newProductAttributeValue);
                }
            }
        }

        private void UpdateProductTags(Product product, IReadOnlyCollection<string> productTags)
        {
            if (productTags == null)
            {
                return;
            }

            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            var existingProductTags = _productTagService.GetAllProductTagsByProductId(product.Id);
            var productTagsToRemove = new List<ProductTag>();
            foreach (var existingProductTag in existingProductTags)
            {
                var found = false;
                foreach (var newProductTag in productTags)
                {
                    if (!existingProductTag.Name.Equals(newProductTag, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    found = true;
                    break;
                }

                if (!found)
                {
                    productTagsToRemove.Add(existingProductTag);
                }
            }

            try
            {
                this._productTagService.UpdateProductTags(product, productTagsToRemove.Select(o => o.Name).ToArray());

                foreach (var productTagName in productTags)
                {
                    ProductTag productTag;
                    var productTag2 = _productTagService.GetProductTagByName(productTagName);
                    if (productTag2 == null)
                    {
                        //add new product tag
                        productTag = new ProductTag
                        {
                            Name = productTagName
                        };
                        _productTagService.InsertProductTag(productTag);
                    }
                    else
                    {
                        productTag = productTag2;
                    }

                    var seName = _urlRecordService.ValidateSeName(productTag, string.Empty, productTag.Name, true);
                    _urlRecordService.SaveSlug(productTag, seName, 0);

                    //Perform a final check to deal with duplicates etc.
                    var currentProductTags = _productTagService.GetAllProductTagsByProductId(product.Id);
                    if (!currentProductTags.Any(o => o.Id == productTag.Id))
                    {
                        _productTagService.InsertProductProductTagMapping(new ProductProductTagMapping()
                        {
                            ProductId = product.Id,
                            ProductTagId = productTag.Id
                        });
                    }

                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void UpdateDiscountMappings(Product product, List<int> passedDiscountIds)
        {
            if (passedDiscountIds == null)
            {
                return;
            }

            var allDiscounts = DiscountService.GetAllDiscounts(DiscountType.AssignedToSkus, showHidden: true);
            var appliedProductDiscount = DiscountService.GetAppliedDiscounts(product);
            foreach (var discount in allDiscounts)
            {
                if (passedDiscountIds.Contains(discount.Id))
                {
                    //new discount
                    if (appliedProductDiscount.Count(d => d.Id == discount.Id) == 0)
                    {
                        appliedProductDiscount.Add(discount);
                    }
                }
                else
                {
                    //remove discount
                    if (appliedProductDiscount.Count(d => d.Id == discount.Id) > 0)
                    {
                        appliedProductDiscount.Remove(discount);
                    }
                }
            }

            _productService.UpdateProduct(product);
            _productService.UpdateHasDiscountsApplied(product);
        }

        private void UpdateProductManufacturers(Product product, List<int> passedManufacturerIds)
        {
            // If no manufacturers specified then there is nothing to map 
            if (passedManufacturerIds == null)
            {
                return;
            }
            var productmanufacturers = _manufacturerService.GetProductManufacturersByProductId(product.Id);
            var unusedProductManufacturers = productmanufacturers.Where(x => !passedManufacturerIds.Contains(x.Id)).ToList();

            // remove all manufacturers that are not passed
            foreach (var unusedProductManufacturer in unusedProductManufacturers)
            {
                //_manufacturerService.DeleteProductManufacturer(unusedProductManufacturer);
            }

            foreach (var passedManufacturerId in passedManufacturerIds)
            {
                // not part of existing manufacturers so we will create a new one
                if (productmanufacturers.All(x => x.Id != passedManufacturerId))
                {
                    // if manufacturer does not exist we simply ignore it, otherwise add it to the product
                    var manufacturer = _manufacturerService.GetManufacturerById(passedManufacturerId);
                    if (manufacturer != null)
                    {
                        _manufacturerService.InsertProductManufacturer(new ProductManufacturer
                        {
                            ProductId = product.Id,
                            ManufacturerId = manufacturer.Id
                        });
                    }
                }
            }
        }

        private void UpdateAssociatedProducts(Product product, List<int> passedAssociatedProductIds)
        {
            // If no associated products specified then there is nothing to map 
            if (passedAssociatedProductIds == null)
                return;

            var noLongerAssociatedProducts =
                _productService.GetAssociatedProducts(product.Id, showHidden: true)
                    .Where(p => !passedAssociatedProductIds.Contains(p.Id));

            // update all products that are no longer associated with our product
            foreach (var noLongerAssocuatedProduct in noLongerAssociatedProducts)
            {
                noLongerAssocuatedProduct.ParentGroupedProductId = 0;
                _productService.UpdateProduct(noLongerAssocuatedProduct);
            }

            var newAssociatedProducts = _productService.GetProductsByIds(passedAssociatedProductIds.ToArray());
            foreach (var newAssociatedProduct in newAssociatedProducts)
            {
                newAssociatedProduct.ParentGroupedProductId = product.Id;
                _productService.UpdateProduct(newAssociatedProduct);
            }
        }
    }
}
