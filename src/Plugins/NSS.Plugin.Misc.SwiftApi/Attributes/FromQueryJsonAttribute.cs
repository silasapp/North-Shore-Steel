using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSS.Plugin.Misc.SwiftApi.ModelBinders;

namespace NSS.Plugin.Misc.SwiftApi.Attributes
{
	public class FromQueryJsonAttribute : ModelBinderAttribute
	{
        public FromQueryJsonAttribute()
        {
            BinderType = typeof(JsonQueryModelBinder);
        }

        public FromQueryJsonAttribute(string paramName)
            : this()
        {
            Name = paramName;
        }
    }
}
