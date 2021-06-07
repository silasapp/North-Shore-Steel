using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NSS.Plugin.Misc.SwiftApi.Maps
{
    public interface IJsonPropertyMapper
    {
        Task<Dictionary<string, Tuple<string, Type>>> GetMapAsync(Type type);
    }
}
