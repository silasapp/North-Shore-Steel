using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Nop.Core.Caching;
using Nop.Core.Infrastructure;
using NSS.Plugin.Misc.SwiftApi.Attributes;
using NSS.Plugin.Misc.SwiftApi.Infrastructure;
using Nop.Services.Caching;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftApi.Maps
{
    public class JsonPropertyMapper : IJsonPropertyMapper
    {
        private IStaticCacheManager _staticCacheManager;

        private IStaticCacheManager StaticCacheManager => _staticCacheManager ?? (_staticCacheManager = EngineContext.Current.Resolve<IStaticCacheManager>());

        public async Task<Dictionary<string, Tuple<string, Type>>> GetMapAsync(Type type)
        {
            var cacheKey = StaticCacheManager.PrepareKeyForDefaultCache(Constants.Configurations.JsonTypeMapsPattern);

            var typeMaps = await StaticCacheManager.GetAsync(cacheKey, () => Task.FromResult<Dictionary<string, Dictionary<string, Tuple<string, Type>>>>(null));

            if (typeMaps == null)
            {
                await StaticCacheManager.SetAsync(cacheKey, new Dictionary<string, Dictionary<string, Tuple<string, Type>>>());

                typeMaps = await StaticCacheManager.GetAsync(cacheKey, () => new Dictionary<string, Dictionary<string, Tuple<string, Type>>>());
            }

            if (!typeMaps.ContainsKey(type.Name))
            {
                await BuildAsync(type);
            }

            return typeMaps[type.Name];
        }

        private async Task BuildAsync(Type type)
        {
            var cacheKey = StaticCacheManager.PrepareKeyForDefaultCache(Constants.Configurations.JsonTypeMapsPattern);

            var typeMaps = await StaticCacheManager.GetAsync(cacheKey, () => Task.FromResult<Dictionary<string, Dictionary<string, Tuple<string, Type>>>>(null));

            var mapForCurrentType = new Dictionary<string, Tuple<string, Type>>();

            var typeProps = type.GetProperties();

            foreach (var property in typeProps)
            {
                var jsonAttribute = property.GetCustomAttribute(typeof(JsonPropertyAttribute)) as JsonPropertyAttribute;
                var doNotMapAttribute = property.GetCustomAttribute(typeof(DoNotMapAttribute)) as DoNotMapAttribute;

                // If it has json attribute set and is not marked as doNotMap
                if (jsonAttribute != null && doNotMapAttribute == null)
                {
                    if (!mapForCurrentType.ContainsKey(jsonAttribute.PropertyName))
                    {
                        var value = new Tuple<string, Type>(property.Name, property.PropertyType);
                        mapForCurrentType.Add(jsonAttribute.PropertyName, value);
                    }
                }
            }

            if (!typeMaps.ContainsKey(type.Name))
            {
                typeMaps.Add(type.Name, mapForCurrentType);
            }
        }
    }
}
