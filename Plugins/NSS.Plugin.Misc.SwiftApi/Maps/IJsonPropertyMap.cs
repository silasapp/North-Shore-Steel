using System;
using System.Collections.Generic;

namespace NSS.Plugin.Misc.SwiftApi.Maps
{
    public interface IJsonPropertyMapper
    {
        Dictionary<string, Tuple<string, Type>> GetMap(Type type);
    }
}
