using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NSS.Plugin.Misc.SwiftPortalOverride.Helpers;
using System;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var config = ConfigHelper.GetConfig();

            string instrumentationKey = config["ApplicationInsights:InstrumentationKey"];

            if (!string.IsNullOrEmpty(instrumentationKey))
            {
                bool enableQuickPulseMetricStream = Convert.ToBoolean(config["ApplicationInsights:Settings:EnableQuickPulseMetricStream"]);
                bool enableAdaptiveSampling = Convert.ToBoolean(config["ApplicationInsights:Settings:EnableAdaptiveSampling"]);
                bool enableHeartbeat = Convert.ToBoolean(config["ApplicationInsights:Settings:EnableHeartbeat"]);
                bool addAutoCollectedMetricExtractor = Convert.ToBoolean(config["ApplicationInsights:Settings:AddAutoCollectedMetricExtractor"]);

                var aiOptions = new Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions
                {
                    // Configures instrumentation key
                    InstrumentationKey = instrumentationKey,

                    // Disables or enables Application Insights features
                    EnableQuickPulseMetricStream = enableQuickPulseMetricStream,
                    EnableAdaptiveSampling = enableAdaptiveSampling,
                    EnableHeartbeat = enableHeartbeat,
                    AddAutoCollectedMetricExtractor = addAutoCollectedMetricExtractor
                };

                if (string.IsNullOrEmpty(aiOptions.InstrumentationKey))
                {
                    // Make sure InsutrmentationKey is not empty
                    aiOptions.InstrumentationKey = "11111111-2222-3333-4444-555555555555";
                }

                services.AddApplicationInsightsTelemetry(aiOptions);
            }


            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });
        }

        public void Configure(IApplicationBuilder application)
        {
            //Register Syncfusion license
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzQxMDAzQDMxMzgyZTMzMmUzMFVQa3R0NWkwN09PSkRKbjJsQ0NadWdFemovWkxxbEU4TGg4cWVYTjhnN2c9");
        }

        public int Order => 11;
    }
}