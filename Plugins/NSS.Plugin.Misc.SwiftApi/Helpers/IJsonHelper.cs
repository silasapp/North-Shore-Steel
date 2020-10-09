﻿using System.Collections.Generic;
using System.IO;

namespace NSS.Plugin.Misc.SwiftApi.Helpers
{
    public interface IJsonHelper
    {
        Dictionary<string, object> GetRequestJsonDictionaryFromStream(Stream stream, bool rewindStream);
        string GetRootPropertyName<T>() where T : class, new();
    }
}