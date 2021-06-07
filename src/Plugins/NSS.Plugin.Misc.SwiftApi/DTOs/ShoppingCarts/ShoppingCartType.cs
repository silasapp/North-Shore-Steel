using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NSS.Plugin.Misc.SwiftApi.DTOs.ShoppingCarts
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum ShoppingCartType
	{
		ShoppingCart = 1,
		Wishlist = 2
	}
}
