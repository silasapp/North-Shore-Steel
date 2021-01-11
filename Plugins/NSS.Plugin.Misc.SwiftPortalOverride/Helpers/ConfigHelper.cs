using Microsoft.Extensions.Configuration;
using NSS.Plugin.Misc.SwiftPortalOverride.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Helpers
{
    static class ConfigHelper
    {
        public static string GetConfigDirectory()
        {
            string assemblyPath = System.Reflection.Assembly.GetAssembly(typeof(PluginNopStartup)).Location;
            string pluginPath = Path.Combine(assemblyPath, @"..\..\Misc.SwiftPortalOverride");
            return Path.GetFullPath(pluginPath);
        }
        public static IConfigurationRoot GetConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(GetConfigDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
        }
    }
}
