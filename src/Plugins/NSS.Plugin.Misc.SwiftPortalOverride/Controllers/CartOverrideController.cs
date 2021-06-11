using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
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
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Controllers
{
    public partial class CartOverrideController : ShoppingCartController
    {
        #region Fields

        private readonly CaptchaSettings _captchaSettings;
        private readonly CustomerSettings _customerSettings;
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
        private readonly ShippingSettings _shippingSettings;

        #endregion

        #region Constructor
        public CartOverrideController(
            CaptchaSettings captchaSettings, 
            ICustomerCompanyProductService customerCompanyProductService, 
            ICompanyService companyService, 
            ICustomerCompanyService customerCompanyService, 
            CustomerSettings customerSettings,  
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
             ShippingSettings shippingSettings,
            ShoppingCartSettings shoppingCartSettings) : base(
                                             captchaSettings,
                                            customerSettings,
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
                                         shoppingCartSettings,
                                         shippingSettings)
        {
            _captchaSettings = captchaSettings;
            _customerSettings = customerSettings;
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
        public override async Task<IActionResult> Cart()
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, (await _workContext.GetCurrentCustomerAsync()).Id);
            int eRPCompanyId = Convert.ToInt32(await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), compIdCookieKey));

            if (!await _customerCompanyService.AuthorizeAsync((await _workContext.GetCurrentCustomerAsync()).Id, eRPCompanyId, ERPRole.Buyer))
                return AccessDeniedView();

            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("Homepage");

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            var model = new ShoppingCartModel();
            model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);

           await CheckForUnavailableProductsInCart(cart);

            return View(model);
        }


        [HttpPost, ActionName("Cart")]
        [FormValueRequired("updatecart")]
        public override async Task<IActionResult> UpdateCart(IFormCollection form)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, currentCustomer.Id);
            int eRPCompanyId = Convert.ToInt32(await _genericAttributeService.GetAttributeAsync<string>(currentCustomer, compIdCookieKey));

            if (!await _customerCompanyService.AuthorizeAsync(currentCustomer.Id, eRPCompanyId, ERPRole.Buyer))
                return AccessDeniedView();

            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("Homepage");

            var cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            //get identifiers of items to remove
            var itemIdsToRemove = form["removefromcart"]
                .SelectMany(value => value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(idString => int.TryParse(idString, out var id) ? id : 0)
                .Distinct().ToList();

            var products = (await _productService.GetProductsByIdsAsync(cart.Select(item => item.ProductId).Distinct().ToArray()))
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
                .OrderByDescending(async cartItem =>
                    (cartItem.NewQuantity < cartItem.Item.Quantity &&
                     (cartItem.Product?.RequireOtherProducts ?? false)) ||
                    (cartItem.NewQuantity > cartItem.Item.Quantity && cartItem.Product != null && (await _shoppingCartService
                         .GetProductsRequiringProductAsync(cart, cartItem.Product)).Any()))
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
                   await UpdateShoppingCartItem(item, form, isNewQuantity: false);
                }
            }

            //try to update cart items with new quantities and get warnings
            var warnings = await orderedCart.SelectAwait(async cartItem => new
            {
                ItemId = cartItem.Item.Id,
                Warnings = await UpdateShoppingCartItem(cartItem, form)
            }).ToListAsync();

            //updated cart
            cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

           await CheckForUnavailableProductsInCart(cart);

            //parse and save checkout attributes
            await ParseAndSaveCheckoutAttributesAsync(cart, form);

            //prepare model
            var model = new ShoppingCartModel();
            model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);

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
                await _genericAttributeService.SaveAttributeAsync(currentCustomer, SwiftPortalOverrideDefaults.CartError, true);
            }
            else
            {
                await _genericAttributeService.SaveAttributeAsync(currentCustomer, SwiftPortalOverrideDefaults.CartError, "");
            }

            return View(model);
        }


        //add product to cart using AJAX
        //currently we use this method on catalog pages (category/manufacturer/etc)
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CustomAddProductToCart_Catalog(int productId, int shoppingCartTypeId,
            int quantity, bool forceredirection = false)
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, (await _workContext.GetCurrentCustomerAsync()).Id);
            int eRPCompanyId = Convert.ToInt32(await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), compIdCookieKey));

            if (!await _customerCompanyService.AuthorizeAsync((await _workContext.GetCurrentCustomerAsync()).Id, eRPCompanyId, ERPRole.Buyer))
                return AccessDeniedView();

            var cartType = (ShoppingCartType)shoppingCartTypeId;

            var product = await _productService.GetProductByIdAsync(productId);

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
                    redirect = Url.RouteUrl("Product", new { SeName = await _urlRecordService.GetSeNameAsync(product) })
                });
            }

            //products with "minimum order quantity" more than a specified qty
            if (product.OrderMinimumQuantity > quantity)
            {
                //we cannot add to the cart such products from category pages
                //it can confuse customers. That's why we redirect customers to the product details page
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = await _urlRecordService.GetSeNameAsync(product) })
                });
            }

            if (product.CustomerEntersPrice)
            {
                //cannot be added to the cart (requires a customer to enter price)
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = await _urlRecordService.GetSeNameAsync(product) })
                });
            }

            if (product.IsRental)
            {
                //rental products require start/end dates to be entered
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = await _urlRecordService.GetSeNameAsync(product) })
                });
            }

            var allowedQuantities = _productService.ParseAllowedQuantities(product);
            if (allowedQuantities.Length > 0)
            {
                //cannot be added to the cart (requires a customer to select a quantity from dropdownlist)
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = await _urlRecordService.GetSeNameAsync(product) })
                });
            }

            //allow a product to be added to the cart when all attributes are with "read-only checkboxes" type
            var productAttributes = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
            if (productAttributes.Count > 0 && productAttributes.Any(p => p.IsRequired))
            {
                //product has some attributes. let a customer see them
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = await _urlRecordService.GetSeNameAsync(product) })
                });
            }

            //creating XML for "read-only checkboxes" attributes
            var attXml = productAttributes.Aggregate(string.Empty, (attributesXml, attribute) =>
            {
                var attributeValues = _productAttributeService.GetProductAttributeValuesAsync(attribute.Id).Result;
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
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), cartType,( await _storeContext.GetCurrentStoreAsync()).Id);
            var shoppingCartItem = cart.FirstOrDefault(x => x.ProductId == product.Id);

            //if we already have the same product in the cart, then use the total quantity to validate
            var quantityToValidate = quantity;
            product.OrderMaximumQuantity = product.OrderMaximumQuantity > 0 ? product.OrderMaximumQuantity : 0;

            var addToCartWarnings = await _shoppingCartService
                .GetShoppingCartItemWarningsAsync(await _workContext.GetCurrentCustomerAsync(), cartType,
                product, (await _storeContext.GetCurrentStoreAsync()).Id, string.Empty,
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
            // if we already have the same product as a wishlist, then don't add again
            if (shoppingCartItem == null || (shoppingCartItem != null && cartType != ShoppingCartType.Wishlist)) {
                addToCartWarnings = await _shoppingCartService.AddToCartAsync(customer: await _workContext.GetCurrentCustomerAsync(),
                    product: product,
                    shoppingCartType: cartType,
                    storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                    attributesXml: shoppingCartItem?.AttributesXml ?? attXml,
                    quantity: quantity);
            }
            if (cartType == ShoppingCartType.ShoppingCart && addToCartWarnings.Any())
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
                       await  _customerActivityService.InsertActivityAsync("PublicStore.AddToWishlist",
                            string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToWishlist"), product.Name), product);

                        if (_shoppingCartSettings.DisplayWishlistAfterAddingProduct || forceredirection)
                        {
                            //redirect to the wishlist page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("Wishlist")
                            });
                        }

                        //display notification message and update appropriate blocks
                        var shoppingCarts = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.Wishlist,(await _storeContext.GetCurrentStoreAsync()).Id);

                        var updatetopwishlistsectionhtml = string.Format(await _localizationService.GetResourceAsync("Wishlist.HeaderQuantity"),
                            shoppingCarts.Count);
                        updatetopwishlistsectionhtml = Regex.Replace(updatetopwishlistsectionhtml, @"[()]+", "");

                        return Json(new
                        {
                            success = true,
                            message = string.Format(await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToTheWishlist.Link"), Url.RouteUrl("Wishlist")),
                            updatetopwishlistsectionhtml
                        });
                    }

                case ShoppingCartType.ShoppingCart:
                default:
                    {
                        //activity log
                       await _customerActivityService.InsertActivityAsync("PublicStore.AddToShoppingCart",
                            string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToShoppingCart"), product.Name), product);

                        if (_shoppingCartSettings.DisplayCartAfterAddingProduct || forceredirection)
                        {
                            //redirect to the shopping cart page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("ShoppingCart")
                            });
                        }

                        //display notification message and update appropriate blocks
                        var shoppingCarts = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart,(await _storeContext.GetCurrentStoreAsync()).Id);

                        var updatetopcartsectionhtml = string.Format(await _localizationService.GetResourceAsync("ShoppingCart.HeaderQuantity"),
                            shoppingCarts.Count);


                        updatetopcartsectionhtml = Regex.Replace(updatetopcartsectionhtml, @"[()]+", "");
                        var updateflyoutcartsectionhtml =  _shoppingCartSettings.MiniShoppingCartEnabled
                            ? await RenderViewComponentToStringAsync("FlyoutShoppingCart")
                            : string.Empty;

                        return Json(new
                        {
                            success = true,
                            message = string.Format(await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToTheCart.Link"), Url.RouteUrl("ShoppingCart")),
                            updatetopcartsectionhtml,
                            updateflyoutcartsectionhtml
                        });
                    }
            }
        }

        protected override async Task<IActionResult> GetProductToCartDetailsAsync(List<string> addToCartWarnings, ShoppingCartType cartType, Product product)
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
                        await _customerActivityService.InsertActivityAsync("PublicStore.AddToWishlist",
                            string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToWishlist"), product.Name), product);

                        if (_shoppingCartSettings.DisplayWishlistAfterAddingProduct)
                        {
                            //redirect to the wishlist page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("Wishlist")
                            });
                        }

                        //display notification message and update appropriate blocks
                        var shoppingCarts = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.Wishlist,(await _storeContext.GetCurrentStoreAsync()).Id);

                        var updatetopwishlistsectionhtml = string.Format(
                            await _localizationService.GetResourceAsync("Wishlist.HeaderQuantity"),
                            shoppingCarts.Count);

                        updatetopwishlistsectionhtml = Regex.Replace(updatetopwishlistsectionhtml, @"[()]+", "");

                        return Json(new
                        {
                            success = true,
                            message = string.Format(
                               await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToTheWishlist.Link"),
                                Url.RouteUrl("Wishlist")),
                            updatetopwishlistsectionhtml
                        });
                    }

                case ShoppingCartType.ShoppingCart:
                default:
                    {
                        //activity log
                        await _customerActivityService.InsertActivityAsync("PublicStore.AddToShoppingCart",
                            string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToShoppingCart"), product.Name), product);

                        if (_shoppingCartSettings.DisplayCartAfterAddingProduct)
                        {
                            //redirect to the shopping cart page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("ShoppingCart")
                            });
                        }

                        //display notification message and update appropriate blocks
                        var shoppingCarts = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart,(await _storeContext.GetCurrentStoreAsync()).Id);

                        var updatetopcartsectionhtml = string.Format(
                           await _localizationService.GetResourceAsync("ShoppingCart.HeaderQuantity"),
                            shoppingCarts.Count);

                        updatetopcartsectionhtml = Regex.Replace(updatetopcartsectionhtml, @"[()]+", "");

                        var updateflyoutcartsectionhtml = _shoppingCartSettings.MiniShoppingCartEnabled
                            ? await RenderViewComponentToStringAsync("FlyoutShoppingCart")
                            : string.Empty;

                        return Json(new
                        {
                            success = true,
                            message = string.Format(await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToTheCart.Link"),
                                Url.RouteUrl("ShoppingCart")),
                            updatetopcartsectionhtml,
                            updateflyoutcartsectionhtml
                        });
                    }
            }
        }

        public override async Task<IActionResult> StartCheckout(IFormCollection form)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey, currentCustomer.Id);
            int eRPCompanyId = Convert.ToInt32(await _genericAttributeService.GetAttributeAsync<string>(currentCustomer, compIdCookieKey));

            if (!await _customerCompanyService.AuthorizeAsync(currentCustomer.Id, eRPCompanyId, ERPRole.Buyer))
                return AccessDeniedView();

            //update cart
           await UpdateCart(form);
            bool checkoutError = await _genericAttributeService.GetAttributeAsync<bool>(currentCustomer, SwiftPortalOverrideDefaults.CartError);
            if (checkoutError)
            {
              await  _genericAttributeService.SaveAttributeAsync(currentCustomer, SwiftPortalOverrideDefaults.CartError, "");
                return View();
            }

            return await base.StartCheckout(form);
        }

        [HttpsRequirement]
        public override async Task<IActionResult> Wishlist(Guid? customerGuid)
        {
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey,(await _workContext.GetCurrentCustomerAsync()).Id);
            int eRPCompanyId = Convert.ToInt32(await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), compIdCookieKey));

            if (!await _customerCompanyService.AuthorizeAsync((await _workContext.GetCurrentCustomerAsync()).Id, eRPCompanyId, ERPRole.Buyer))
                return AccessDeniedView();

            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist))
                return RedirectToRoute("Homepage");

            var customer = customerGuid.HasValue ?
               await _customerService.GetCustomerByGuidAsync(customerGuid.Value)
                : await _workContext.GetCurrentCustomerAsync();
            if (customer == null)
                return RedirectToRoute("Homepage");

            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.Wishlist,(await _storeContext.GetCurrentStoreAsync()).Id);

            var model = new WishlistModel();
            model = await _shoppingCartModelFactory.PrepareWishlistModelAsync(model, cart, !customerGuid.HasValue);
            return View(model);
        }

        private async Task CheckForUnavailableProductsInCart(IList<ShoppingCartItem> cart)
        {
            var productIds = cart.Select(item => item.ProductId).Distinct().ToArray();
            var products = await _productService.GetProductsByIdsAsync(productIds);

            if (products.Any(p => !p.Published))
            {
                _notificationService.ErrorNotification("Remove products that are no longer available.");
            }
        }

        private async Task<IList<string>> UpdateShoppingCartItem(dynamic cartItem, IFormCollection form, bool isNewQuantity = true)
        {
            var attrsMapping =await _productAttributeService.GetProductAttributeMappingsByProductIdAsync((int)cartItem.Product.Id);

            foreach (var map in attrsMapping)
            {
                var formControlId = $"{NopCatalogDefaults.ProductAttributePrefix}{map.Id}_{cartItem.Item.Id}";
                if (form.TryGetValue(formControlId, out var value) && !string.IsNullOrEmpty(value))
                {
                    cartItem.Item.AttributesXml = _productAttributeParser.RemoveProductAttribute(cartItem.Item.AttributesXml, map);
                    cartItem.Item.AttributesXml = _productAttributeParser.AddProductAttribute(cartItem.Item.AttributesXml, map, value);
                }
            }

            var warnings = await _shoppingCartService.UpdateShoppingCartItemAsync(await _workContext.GetCurrentCustomerAsync(),
                                cartItem.Item.Id, cartItem.Item.AttributesXml, cartItem.Item.CustomerEnteredPrice,
                                cartItem.Item.RentalStartDateUtc, cartItem.Item.RentalEndDateUtc, isNewQuantity ? cartItem.NewQuantity : cartItem.Item.Quantity, true);

            // update cust part No
            CustomerCompany customerCompany = await GetCustomerCompanyDetails();

            if (customerCompany != null)
            {
                var controlId = $"customerpartNo{cartItem.Product.Id}";
                if (form.TryGetValue(controlId, out var value) && !string.IsNullOrEmpty(value.FirstOrDefault()))
                  await  _customerCompanyProductService.UpdateCustomerCompanyProduct(new CustomerCompanyProduct { CustomerCompanyId = customerCompany.Id, ProductId = cartItem.Product.Id, CustomerPartNo = value.FirstOrDefault() });
            }

            return warnings;
        }

        private async Task<CustomerCompany> GetCustomerCompanyDetails()
        {
            CustomerCompany customerCompany = new CustomerCompany();
            var compIdCookieKey = string.Format(SwiftPortalOverrideDefaults.ERPCompanyCookieKey,(await _workContext.GetCurrentCustomerAsync()).Id);
            int eRPCompanyId = Convert.ToInt32(await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), compIdCookieKey));

            if (eRPCompanyId > 0)
                customerCompany = await _customerCompanyService.GetCustomerCompanyByErpCompIdAsync((await _workContext.GetCurrentCustomerAsync()).Id, eRPCompanyId);

            return customerCompany;
        }
    }
}