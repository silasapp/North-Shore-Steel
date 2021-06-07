using System;
using System.Collections.Generic;

namespace NSS.Plugin.Misc.SwiftApi.Validators
{
    public interface IFieldsValidator
    {
        //TODO: Why this needs to be dictionary???
        Dictionary<string, bool> GetValidFields(string fields, Type type);
    }
}
