using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Nop.Core.Infrastructure;
using NSS.Plugin.Misc.SwiftApi.Authorization.Policies;
using NSS.Plugin.Misc.SwiftApi.Authorization.Requirements;
using NSS.Plugin.Misc.SwiftApi.Configuration;
using Nop.Web.Framework.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http.Features;

namespace NSS.Plugin.Misc.SwiftApi.Infrastructure
{
    public class ApiStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var apiConfigSection = configuration.GetSection("Api");

            if (apiConfigSection != null)
            {
                var apiConfig = services.ConfigureStartupConfig<ApiConfiguration>(apiConfigSection);

                if (!string.IsNullOrEmpty(apiConfig.SecurityKey))
                {
                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwtBearerOptions =>
                    {
                        jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey =
                                                                                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(apiConfig.SecurityKey)),
                            ValidateIssuer = false, // ValidIssuer = "The name of the issuer",
                            ValidateAudience = false, // ValidAudience = "The name of the audience",
                            ValidateLifetime =
                                                                                true, // validate the expiration and not before values in the token
                            ClockSkew = TimeSpan.FromMinutes(apiConfig.AllowedClockSkewInMinutes)
                        };
                    });

                    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
                    AddAuthorizationPipeline(services);
                    //services.AddHostedService<ApplicationPartsLogger>();
                }
            }

            services.Configure<FormOptions>(options =>
            {
                options.KeyLengthLimit = int.MaxValue;
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = int.MaxValue;
                options.MultipartHeadersLengthLimit = int.MaxValue;
            });

            // If using Kestrel:
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            // If using IIS:
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

        }

        public void Configure(IApplicationBuilder app)
        {
            var rewriteOptions = new RewriteOptions()
                .AddRewrite("api/token", "/token", true);

            app.UseRewriter(rewriteOptions);

            app.UseCors(x => x.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader());

            // Need to enable rewind so we can read the request body multiple times
            // This should eventually be refactored, but both JsonModelBinder and all of the DTO validators need to read this stream.
            //app.UseWhen(x => x.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase),
            //            builder =>
            //            {
            //                builder.Use(async (context, next) =>
            //                {
            //                    Console.WriteLine("API Call");
            //                    context.Request.EnableBuffering();
            //                    await next();
            //                });
            //            });

            app.MapWhen(
                context => (context.Request.Path
                    .StartsWithSegments(new PathString("/api"))
                ),
                a =>
                {

                    a.Use(async (context, next) =>
                                {
                                    Console.WriteLine("API Call");
                                    context.Request.EnableBuffering();
                                    await next();
                                });

                    a.UseExceptionHandler("/api/error/500/Error");

                    a.UseRouting();
                    a.UseAuthentication();
                    a.UseAuthorization();
                    a.UseEndpoints(endpoints =>
                    {
                        endpoints
                            .MapControllers();
                    });


                }
            );
        }

        public int Order => 1;

        private static void AddAuthorizationPipeline(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(JwtBearerDefaults.AuthenticationScheme,
                                  policy =>
                                  {
                                      policy.Requirements.Add(new ActiveApiPluginRequirement());
                                      policy.Requirements.Add(new AuthorizationSchemeRequirement());
                                      policy.Requirements.Add(new CustomerRoleRequirement());
                                      policy.RequireAuthenticatedUser();
                                  });
            });

            services.AddSingleton<IAuthorizationHandler, ActiveApiPluginAuthorizationPolicy>();
            services.AddSingleton<IAuthorizationHandler, ValidSchemeAuthorizationPolicy>();
            services.AddSingleton<IAuthorizationHandler, CustomerRoleAuthorizationPolicy>();

        }
    }
}
