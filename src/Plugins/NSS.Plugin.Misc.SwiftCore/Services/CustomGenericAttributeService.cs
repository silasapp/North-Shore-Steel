using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Common;
using Nop.Data;
using Nop.Services.Common;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public class CustomGenericAttributeService : GenericAttributeService
    {
        private readonly IRepository<GenericAttribute> _genericAttributeRepository;
        public CustomGenericAttributeService(IStaticCacheManager staticCacheManager, IRepository<GenericAttribute> genericAttributeRepository) : base(genericAttributeRepository, staticCacheManager)
        {
            _genericAttributeRepository = genericAttributeRepository;
        }

        /// <summary>
        /// Get attributes
        /// </summary>
        /// <param name="key">Attribute Key</param>>
        /// <param name="value">Attribute value</param>>
        /// <param name="keyGroup">Key group</param>
        /// <returns>Get attributes</returns>
        public async Task<GenericAttribute> GetAttributeByKeyValueAsync(string key, string value, string keyGroup)
        {
            var query = from ga in _genericAttributeRepository.Table
                        where ga.Key == key &&
                              ga.Value == value &&
                              ga.KeyGroup == keyGroup
                        select ga;

            var attribute = await query.FirstOrDefaultAsync();

            return attribute;
        }
    }
}
