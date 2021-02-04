using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Infrastructure;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Models.ShoppingCart;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using NSS.Plugin.Misc.SwiftCore.Helpers;
using NSS.Plugin.Misc.SwiftCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{
    public partial class CartOverrideController : ShoppingCartController
    {
        #region Fields

        private readonly CaptchaSettings _captchaSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly ICacheKeyService _cacheKeyService;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IDownloadService _downloadService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGiftCardService _giftCardService;
        private readonly ILocalizationService _localizationService;
        private readonly INopFileProvider _fileProvider;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly MediaSettings _mediaSettings;
        private readonly OrderSettings _orderSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly ICustomerCompanyProductService _customerCompanyProductService;
        private readonly ICustomerCompanyService _customerCompanyService;
        private readonly ICompanyService _companyService;

        #endregion

        #region Constructor
        public CartOverrideController(
            CaptchaSettings captchaSettings, 
            ICustomerCompanyProductService customerCompanyProductService, 
            ICompanyService companyService, 
            ICustomerCompanyService customerCompanyService, 
            CustomerSettings customerSettings, 
            ICacheKeyService cacheKeyService, 
            ICheckoutAttributeParser checkoutAttributeParser, 
            ICheckoutAttributeService checkoutAttributeService, 
            ICurrencyService currencyService, 
            ICustomerActivityService customerActivityService, 
            ICustomerService customerService, 
            IDiscountService discountService, 
            IDownloadService downloadService, 
            IGenericAttributeService genericAttributeService, 
            IGiftCardService giftCardService, 
            ILocalizationService localizationService, 
            INopFileProvider fileProvider, 
            INotificationService notificationService, 
            IPermissionService permissionService, 
            IPictureService pictureService, 
            IPriceFormatter priceFormatter, 
            IProductAttributeParser productAttributeParser, 
            IProductAttributeService productAttributeService, 
            IProductService productService, 
            IShippingService shippingService, 
            IShoppingCartModelFactory shoppingCartModelFactory, 
            IShoppingCartService shoppingCartService, 
            IStaticCacheManager staticCacheManager, 
            IStoreContext storeContext, 
            ITaxService taxService, 
            IUrlRecordService urlRecordService, 
            IWebHelper webHelper, 
            IWorkContext workContext, 
            IWorkflowMessageService workflowMessageService, 
            MediaSettings mediaSettings, 
            OrderSettings orderSettings, 
            ShoppingCartSettings shoppingCartSettings) : base(captchaSettings, 
                                                              customerSettings, 
                                                              cacheKeyService, 
                                                              checkoutAttributeParser, 
                                                              checkoutAttributeService, 
                                                              currencyService, 
                                                              customerActivityService, 
                                                              customerService, 
                                                              discountService, 
                                                              downloadService, 
                                                              genericAttributeService, 
                                                              giftCardService, 
                                                              localizationService, 
                                                              fileProvider, 
                                                              notificationService, 
                                                              permissionService, 
                                                              pictureService, 
                                                              priceFormatter, 
                                                              productAttributeParser, 
                                                              productAttributeService, 
                                                              productService, 
                                                              shippingService, 
                                                              shoppingCartModelFactory, 
                                                              shoppingCartService, 
                                                              staticCacheManager, 
                                                              storeContext, 
                                                              taxService, 
                                                              urlRecordService, 
                                                              webHelper, 
                                                              workContext, 
                                                              workflowMessageService, 
                                                              mediaSettings, 
                                                              orderSettings, 
                                                              shoppingCartSettings)
        {
            _captchaSettings = captchaSettings;
            _customerSettings = customerSettings;
            _cacheKeyService = cacheKeyService;
            _checkoutAttributeParser = checkoutAttributeParser;
            _checkoutAttributeService = checkoutAttributeService;
            _currencyService = currencyService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _discountService = discountService;
            _downloadService = downloadService;
            _genericAttributeService = genericAttributeService;
            _giftCardService = giftCardService;
            _localizationService = localizationService;
            _fileProvider = fileProvider;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productService = productService;
            _shippingService = shippingService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _shoppingCartService = shoppingCartService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _taxService = taxService;
            _urlRecordService = urlRecordService;
            _webHelper = webHelper;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _mediaSettings = mediaSettings;
            _orderSettings = orderSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _customerCompanyProductService = customerCompanyProductService;
            _customerCompanyService = customerCompanyService;
            _companyService = companyService;
        }


        #endregion

        [HttpsRequirement]
        public override IActionResult Cart()
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, _workContext.CurrentCustomer.Id);
            int eRPCompanyId = Convert.ToInt32(_genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, compIdCookieKey));

            if (!_customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.Buyer))
                return AccessDeniedView();

            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("Homepage");

            var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);
            var model = new ShoppingCartModel();
            model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart);

            CheckForUnavailableProductsInCart(cart);

            return View(model);
        }


        [HttpPost, ActionName("Cart")]
        [FormValueRequired("updatecart")]
        public override IActionResult UpdateCart(IFormCollection form)
        {
            var currentCustomer = _workContext.CurrentCustomer;
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, currentCustomer.Id);
            int eRPCompanyId = Convert.ToInt32(_genericAttributeService.GetAttribute<string>(currentCustomer, compIdCookieKey));

            if (!_customerCompanyService.Authorize(currentCustomer.Id, eRPCompanyId, ERPRole.Buyer))
                return AccessDeniedView();

            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("Homepage");

            var cart = _shoppingCartService.GetShoppingCart(currentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

            //get identifiers of items to remove
            var itemIdsToRemove = form["removefromcart"]
                .SelectMany(value => value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(idString => int.TryParse(idString, out var id) ? id : 0)
                .Distinct().ToList();

            var products = _productService.GetProductsByIds(cart.Select(item => item.ProductId).Distinct().ToArray())
                .ToDictionary(item => item.Id, item => item);

            //get order items with changed quantity
            var itemsWithNewQuantity = cart.Select(item => new
            {
                //try to get a new quantity for the item, set 0 for items to remove
                NewQuantity = itemIdsToRemove.Contains(item.Id) ? 0 : int.TryParse(form[$"itemquantity{item.Id}"], out var quantity) ? quantity : item.Quantity,
                Item = item,
                Product = products.ContainsKey(item.ProductId) ? products[item.ProductId] : null
            }).Where(item => item.NewQuantity != item.Item.Quantity);


            var errors = new List<string>();
            // var attributeXml = _productAttributeParser.ParseProductAttributes(product, form, errors);

            //order cart items
            //first should be items with a reduced quantity and that require other products; or items with an increased quantity and are required for other products
            var orderedCart = itemsWithNewQuantity
                .OrderByDescending(cartItem =>
                    (cartItem.NewQuantity < cartItem.Item.Quantity &&
                     (cartItem.Product?.RequireOtherProducts ?? false)) ||
                    (cartItem.NewQuantity > cartItem.Item.Quantity && cartItem.Product != null && _shoppingCartService
                         .GetProductsRequiringProduct(cart, cartItem.Product).Any()))
                .ToList();

            if (orderedCart.Count == 0)
            {
                var cartItems = cart.Select(item => new
                {
                    Item = item,
                    Product = products.ContainsKey(item.ProductId) ? products[item.ProductId] : null
                });

                foreach (var item in cartItems)
                {
                    UpdateShoppingCartItem(item, form, isNewQuantity: false);
                }
            }

            //try to update cart items with new quantities and get warnings
            var warnings = orderedCart.Select(cartItem => new
            {
                ItemId = cartItem.Item.Id,
                Warnings = UpdateShoppingCartItem(cartItem, form)
            }).ToList();

            //updated cart
            cart = _shoppingCartService.GetShoppingCart(currentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

            CheckForUnavailableProductsInCart(cart);

            //parse and save checkout attributes
            ParseAndSaveCheckoutAttributes(cart, form);

            //prepare model
            var model = new ShoppingCartModel();
            model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart);

            //update current warnings
            foreach (var warningItem in warnings.Where(warningItem => warningItem.Warnings.Any()))
            {
                //find shopping cart item model to display appropriate warnings
                var itemModel = model.Items.FirstOrDefault(item => item.Id == warningItem.ItemId);
                if (itemModel != null)
                    itemModel.Warnings = warningItem.Warnings.Concat(itemModel.Warnings).Distinct().ToList();
            }

            var checkoutError = model.Items.FirstOrDefault(it => it.Warnings.Count > 0);
            if (checkoutError != null)
            {
                _genericAttributeService.SaveAttribute(currentCustomer, SwiftPortalOverrideDefaults.CartError, true);
            }
            else
            {
                _genericAttributeService.SaveAttribute(currentCustomer, SwiftPortalOverrideDefaults.CartError, "");
            }

            return View(model);
        }


        //add product to cart using AJAX
        //currently we use this method on catalog pages (category/manufacturer/etc)
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult CustomAddProductToCart_Catalog(int productId, int shoppingCartTypeId,
            int quantity, bool forceredirection = false)
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, _workContext.CurrentCustomer.Id);
            int eRPCompanyId = Convert.ToInt32(_genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, compIdCookieKey));

            if (!_customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.Buyer))
                return AccessDeniedView();

            var cartType = (ShoppingCartType)shoppingCartTypeId;

            var product = _productService.GetProductById(productId);

            if (quantity == 0)
                //we can only add when quantity is more than zero
                return Json(new
                {
                    success = false,
                    message = "Quantity should be more than zero (0)"
                });

            if (product == null)
                //no product found
                return Json(new
                {
                    success = false,
                    message = "No product found with the specified ID"
                });

            //we can add only simple products
            if (product.ProductType != ProductType.SimpleProduct)
            {
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = _urlRecordService.GetSeName(product) })
                });
            }

            //products with "minimum order quantity" more than a specified qty
            if (product.OrderMinimumQuantity > quantity)
            {
                //we cannot add to the cart such products from category pages
                //it can confuse customers. That's why we redirect customers to the product details page
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = _urlRecordService.GetSeName(product) })
                });
            }

            if (product.CustomerEntersPrice)
            {
                //cannot be added to the cart (requires a customer to enter price)
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = _urlRecordService.GetSeName(product) })
                });
            }

            if (product.IsRental)
            {
                //rental products require start/end dates to be entered
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = _urlRecordService.GetSeName(product) })
                });
            }

            var allowedQuantities = _productService.ParseAllowedQuantities(product);
            if (allowedQuantities.Length > 0)
            {
                //cannot be added to the cart (requires a customer to select a quantity from dropdownlist)
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = _urlRecordService.GetSeName(product) })
                });
            }

            //allow a product to be added to the cart when all attributes are with "read-only checkboxes" type
            var productAttributes = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
            if (productAttributes.Count > 0 && productAttributes.Any(p => p.IsRequired))
            {
                //product has some attributes. let a customer see them
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = _urlRecordService.GetSeName(product) })
                });
            }

            //creating XML for "read-only checkboxes" attributes
            var attXml = productAttributes.Aggregate(string.Empty, (attributesXml, attribute) =>
            {
                var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                foreach (var selectedAttributeId in attributeValues
                    .Where(v => v.IsPreSelected)
                    .Select(v => v.Id)
                    .ToList())
                {
                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                        attribute, selectedAttributeId.ToString());
                }

                return attributesXml;
            });

            //get standard warnings without attribute validations
            //first, try to find existing shopping cart item
            var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, cartType, _storeContext.CurrentStore.Id);
            var shoppingCartItem = _shoppingCartService.FindShoppingCartItemInTheCart(cart, cartType, product);
            //if we already have the same product in the cart, then use the total quantity to validate
            var quantityToValidate = shoppingCartItem != null ? shoppingCartItem.Quantity + quantity : quantity;
            product.OrderMaximumQuantity = product.OrderMaximumQuantity > 0 ? product.OrderMaximumQuantity : 0;
            if (cartType == ShoppingCartType.Wishlist)
                product.OrderMaximumQuantity = 100001;

            var addToCartWarnings = _shoppingCartService
                .GetShoppingCartItemWarnings(_workContext.CurrentCustomer, cartType,
                product, _storeContext.CurrentStore.Id, string.Empty,
                decimal.Zero, null, null, quantityToValidate, false, shoppingCartItem?.Id ?? 0, true, false, false, false);
            if (cartType == ShoppingCartType.ShoppingCart && addToCartWarnings.Any())
            {
                //cannot be added to the cart
                //let's display standard warnings
                return Json(new
                {
                    success = false,
                    message = addToCartWarnings.ToArray()
                });
            }

            //now let's try adding product to the cart (now including product attribute validation, etc)
            addToCartWarnings = _shoppingCartService.AddToCart(customer: _workContext.CurrentCustomer,
                product: product,
                shoppingCartType: cartType,
                storeId: _storeContext.CurrentStore.Id,
                attributesXml: attXml,
                quantity: quantity);
            if (cartType == ShoppingCartType.ShoppingCart &&  addToCartWarnings.Any())
            {
                //cannot be added to the cart
                //but we do not display attribute and gift card warnings here. let's do it on the product details page
                return Json(new
                {
                    success = false,
                    message = addToCartWarnings.ToArray()
                    //redirect = Url.RouteUrl("Product", new { SeName = _urlRecordService.GetSeName(product) })
                });
            }

            //added to the cart/wishlist
            switch (cartType)
            {
                case ShoppingCartType.Wishlist:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToWishlist",
                            string.Format(_localizationService.GetResource("ActivityLog.PublicStore.AddToWishlist"), product.Name), product);

                        if (_shoppingCartSettings.DisplayWishlistAfterAddingProduct || forceredirection)
                        {
                            //redirect to the wishlist page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("Wishlist")
                            });
                        }

                        //display notification message and update appropriate blocks
                        var shoppingCarts = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id);

                        var updatetopwishlistsectionhtml = string.Format(_localizationService.GetResource("Wishlist.HeaderQuantity"),
                            shoppingCarts.Count);
                        updatetopwishlistsectionhtml = Regex.Replace(updatetopwishlistsectionhtml, @"[()]+", "");

                        return Json(new
                        {
                            success = true,
                            message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToTheWishlist.Link"), Url.RouteUrl("Wishlist")),
                            updatetopwishlistsectionhtml
                        });
                    }

                case ShoppingCartType.ShoppingCart:
                default:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToShoppingCart",
                            string.Format(_localizationService.GetResource("ActivityLog.PublicStore.AddToShoppingCart"), product.Name), product);

                        if (_shoppingCartSettings.DisplayCartAfterAddingProduct || forceredirection)
                        {
                            //redirect to the shopping cart page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("ShoppingCart")
                            });
                        }

                        //display notification message and update appropriate blocks
                        var shoppingCarts = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

                        var updatetopcartsectionhtml = string.Format(_localizationService.GetResource("ShoppingCart.HeaderQuantity"),
                            shoppingCarts.Count);


                        updatetopcartsectionhtml = Regex.Replace(updatetopcartsectionhtml, @"[()]+", "");
                        var updateflyoutcartsectionhtml = _shoppingCartSettings.MiniShoppingCartEnabled
                            ? RenderViewComponentToString("FlyoutShoppingCart")
                            : string.Empty;

                        return Json(new
                        {
                            success = true,
                            message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToTheCart.Link"), Url.RouteUrl("ShoppingCart")),
                            updatetopcartsectionhtml,
                            updateflyoutcartsectionhtml
                        });
                    }
            }
        }

        //add product to cart using AJAX
        //currently we use this method on the product details pages
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public override IActionResult AddProductToCart_Details(int productId, int shoppingCartTypeId, IFormCollection form)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
            {
                return Json(new
                {
                    redirect = Url.RouteUrl("Homepage")
                });
            }

            //we can add only simple products
            if (product.ProductType != ProductType.SimpleProduct)
            {
                return Json(new
                {
                    success = false,
                    message = "Only simple products could be added to the cart"
                });
            }

            //update existing shopping cart item
            var updatecartitemid = 0;
            foreach (var formKey in form.Keys)
                if (formKey.Equals($"addtocart_{productId}.UpdatedShoppingCartItemId", StringComparison.InvariantCultureIgnoreCase))
                {
                    int.TryParse(form[formKey], out updatecartitemid);
                    break;
                }

            ShoppingCartItem updatecartitem = null;
            if (_shoppingCartSettings.AllowCartItemEditing && updatecartitemid > 0)
            {
                //search with the same cart type as specified
                var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, (ShoppingCartType)shoppingCartTypeId, _storeContext.CurrentStore.Id);

                updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
                //not found? let's ignore it. in this case we'll add a new item
                //if (updatecartitem == null)
                //{
                //    return Json(new
                //    {
                //        success = false,
                //        message = "No shopping cart item found to update"
                //    });
                //}
                //is it this product?
                if (updatecartitem != null && product.Id != updatecartitem.ProductId)
                {
                    return Json(new
                    {
                        success = false,
                        message = "This product does not match a passed shopping cart item identifier"
                    });
                }
            }

            var addToCartWarnings = new List<string>();

            //customer entered price
            var customerEnteredPriceConverted = _productAttributeParser.ParseCustomerEnteredPrice(product, form);

            //entered quantity
            var quantity = _productAttributeParser.ParseEnteredQuantity(product, form);

            //product and gift card attributes
            var attributes = _productAttributeParser.ParseProductAttributes(product, form, addToCartWarnings);

            //rental attributes
            _productAttributeParser.ParseRentalDates(product, form, out var rentalStartDate, out var rentalEndDate);

            var cartType = updatecartitem == null ? (ShoppingCartType)shoppingCartTypeId :
                //if the item to update is found, then we ignore the specified "shoppingCartTypeId" parameter
                updatecartitem.ShoppingCartType;

            SaveItem(updatecartitem, addToCartWarnings, product, cartType, attributes, customerEnteredPriceConverted, rentalStartDate, rentalEndDate, quantity);

            //return result
            return GetProductToCartDetails(addToCartWarnings, cartType, product);
        }

        protected override IActionResult GetProductToCartDetails(List<string> addToCartWarnings, ShoppingCartType cartType, Product product)
        {
            if (addToCartWarnings.Any())
            {
                //cannot be added to the cart/wishlist
                //let's display warnings
                return Json(new
                {
                    success = false,
                    message = addToCartWarnings.ToArray()
                });
            }

            //added to the cart/wishlist
            switch (cartType)
            {
                case ShoppingCartType.Wishlist:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToWishlist",
                            string.Format(_localizationService.GetResource("ActivityLog.PublicStore.AddToWishlist"), product.Name), product);

                        if (_shoppingCartSettings.DisplayWishlistAfterAddingProduct)
                        {
                            //redirect to the wishlist page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("Wishlist")
                            });
                        }

                        //display notification message and update appropriate blocks
                        var shoppingCarts = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id);

                        var updatetopwishlistsectionhtml = string.Format(
                            _localizationService.GetResource("Wishlist.HeaderQuantity"),
                            shoppingCarts.Count);

                        updatetopwishlistsectionhtml = Regex.Replace(updatetopwishlistsectionhtml, @"[()]+", "");

                        return Json(new
                        {
                            success = true,
                            message = string.Format(
                                _localizationService.GetResource("Products.ProductHasBeenAddedToTheWishlist.Link"),
                                Url.RouteUrl("Wishlist")),
                            updatetopwishlistsectionhtml
                        });
                    }

                case ShoppingCartType.ShoppingCart:
                default:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToShoppingCart",
                            string.Format(_localizationService.GetResource("ActivityLog.PublicStore.AddToShoppingCart"), product.Name), product);

                        if (_shoppingCartSettings.DisplayCartAfterAddingProduct)
                        {
                            //redirect to the shopping cart page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("ShoppingCart")
                            });
                        }

                        //display notification message and update appropriate blocks
                        var shoppingCarts = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

                        var updatetopcartsectionhtml = string.Format(
                            _localizationService.GetResource("ShoppingCart.HeaderQuantity"),
                            shoppingCarts.Count);

                        updatetopcartsectionhtml = Regex.Replace(updatetopcartsectionhtml, @"[()]+", "");

                        var updateflyoutcartsectionhtml = _shoppingCartSettings.MiniShoppingCartEnabled
                            ? RenderViewComponentToString("FlyoutShoppingCart")
                            : string.Empty;

                        return Json(new
                        {
                            success = true,
                            message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToTheCart.Link"),
                                Url.RouteUrl("ShoppingCart")),
                            updatetopcartsectionhtml,
                            updateflyoutcartsectionhtml
                        });
                    }
            }
        }

        public override IActionResult StartCheckout(IFormCollection form)
        {
            var currentCustomer = _workContext.CurrentCustomer;
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, currentCustomer.Id);
            int eRPCompanyId = Convert.ToInt32(_genericAttributeService.GetAttribute<string>(currentCustomer, compIdCookieKey));

            if (!_customerCompanyService.Authorize(currentCustomer.Id, eRPCompanyId, ERPRole.Buyer))
                return AccessDeniedView();

            //update cart
            UpdateCart(form);
            bool checkoutError = _genericAttributeService.GetAttribute<bool>(currentCustomer, SwiftPortalOverrideDefaults.CartError);
            if (checkoutError)
            {
                _genericAttributeService.SaveAttribute(currentCustomer, SwiftPortalOverrideDefaults.CartError, "");
                return View();
            }

            return base.StartCheckout(form);
        }

        [HttpsRequirement]
        public override IActionResult Wishlist(Guid? customerGuid)
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, _workContext.CurrentCustomer.Id);
            int eRPCompanyId = Convert.ToInt32(_genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, compIdCookieKey));

            if (!_customerCompanyService.Authorize(_workContext.CurrentCustomer.Id, eRPCompanyId, ERPRole.Buyer))
                return AccessDeniedView();

            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
                return RedirectToRoute("Homepage");

            var customer = customerGuid.HasValue ?
                _customerService.GetCustomerByGuid(customerGuid.Value)
                : _workContext.CurrentCustomer;
            if (customer == null)
                return RedirectToRoute("Homepage");

            var cart = _shoppingCartService.GetShoppingCart(customer, ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id);

            var model = new WishlistModel();
            model = _shoppingCartModelFactory.PrepareWishlistModel(model, cart, !customerGuid.HasValue);
            return View(model);
        }

        private void CheckForUnavailableProductsInCart(IList<ShoppingCartItem> cart)
        {
            var productIds = cart.Select(item => item.ProductId).Distinct().ToArray();
            var products = _productService.GetProductsByIds(productIds);

            if (products.Any(p => !p.Published))
            {
                _notificationService.ErrorNotification("Remove products that are no longer available.");
            }
        }

        private IList<string> UpdateShoppingCartItem(dynamic cartItem, IFormCollection form, bool isNewQuantity = true)
        {
            var attrsMapping = _productAttributeService.GetProductAttributeMappingsByProductId((int)cartItem.Product.Id);

            foreach (var map in attrsMapping)
            {
                var formControlId = $"{NopCatalogDefaults.ProductAttributePrefix}{map.Id}{cartItem.Item.Id}";
                if (form.TryGetValue(formControlId, out var value) && !string.IsNullOrEmpty(value))
                {
                    cartItem.Item.AttributesXml = _productAttributeParser.RemoveProductAttribute(cartItem.Item.AttributesXml, map);
                    cartItem.Item.AttributesXml = _productAttributeParser.AddProductAttribute(cartItem.Item.AttributesXml, map, value);
                }
            }

            var warnings = _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
                                cartItem.Item.Id, cartItem.Item.AttributesXml, cartItem.Item.CustomerEnteredPrice,
                                cartItem.Item.RentalStartDateUtc, cartItem.Item.RentalEndDateUtc, isNewQuantity ? cartItem.NewQuantity : cartItem.Item.Quantity, true);

            // update cust part No
            CustomerCompany customerCompany = GetCustomerCompanyDetails();

            if (customerCompany != null)
            {
                var controlId = $"customerpartNo{cartItem.Product.Id}";
                if (form.TryGetValue(controlId, out var value) && !string.IsNullOrEmpty(value.FirstOrDefault()))
                    _customerCompanyProductService.UpdateCustomerCompanyProduct(new CustomerCompanyProduct { CustomerCompanyId = customerCompany.Id, ProductId = cartItem.Product.Id, CustomerPartNo = value.FirstOrDefault() });
            }

            return warnings;
        }

        private CustomerCompany GetCustomerCompanyDetails()
        {
            CustomerCompany customerCompany = new CustomerCompany();
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, _workContext.CurrentCustomer.Id);
            int eRPCompanyId = Convert.ToInt32(_genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, compIdCookieKey));

            if (eRPCompanyId > 0)
                customerCompany = _customerCompanyService.GetCustomerCompanyByErpCompId(_workContext.CurrentCustomer.Id, eRPCompanyId);

            return customerCompany;
        }
    }
}