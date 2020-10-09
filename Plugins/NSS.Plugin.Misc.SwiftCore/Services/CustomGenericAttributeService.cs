using System;
using System.Linq;
using Nop.Core.Domain.Common;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Caching;
using Nop.Services.Caching.Extensions;
using Nop.Services.Common;
using Nop.Services.Events;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public class CustomGenericAttributeService : GenericAttributeService
    {
        private readonly IRepository<GenericAttribute> _genericAttributeRepository;
        public CustomGenericAttributeService(IEventPublisher eventPublisher, IRepository<GenericAttribute> genericAttributeRepository) : base(eventPublisher, genericAttributeRepository)
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
        public GenericAttribute GetAttributeByKeyValue(string key, string value, string keyGroup)
        {
            var query = from ga in _genericAttributeRepository.Table
                        where ga.Key == key &&
                              ga.Value == value &&
                              ga.KeyGroup == keyGroup
                        select ga;

            var attribute = query.FirstOrDefault();

            return attribute;
        }
    }
}
