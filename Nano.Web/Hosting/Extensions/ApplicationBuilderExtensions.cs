using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Nano.App;
using Nano.Config;
using Nano.Web.Hosting.Enums;
using Nano.Web.Hosting.Middleware;
using NWebsec.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerUI;
using ReferrerPolicy = NWebsec.Core.Common.HttpHeaders.ReferrerPolicy;

namespace Nano.Web.Hosting.Extensions
{
    /// <summary>
    /// Application Builder Extensions.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds exception handling middleware to the <see cref="IApplicationBuilder"/>.
        /// </summary>
        /// <param name="applicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        internal static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
                throw new ArgumentNullException(nameof(applicationBuilder));

            applicationBuilder
                .UseMiddleware<ExceptionHandlingMiddleware>();

            return applicationBuilder;
        }
        
        /// <summary>
        /// Adds options action middleware to the <see cref="IApplicationBuilder"/>.
        /// </summary>
        /// <param name="applicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        internal static IApplicationBuilder UseHttpRequestOptions(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
                throw new ArgumentNullException(nameof(applicationBuilder));

            applicationBuilder
                .UseMiddleware<HttpRequestOptionsMiddleware>();

            return applicationBuilder;
        }
 
        /// <summary>
        /// Adds request identifier middleware to the <see cref="IApplicationBuilder"/>.
        /// </summary>
        /// <param name="applicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        internal static IApplicationBuilder UseHttpRequestIdentifier(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
                throw new ArgumentNullException(nameof(applicationBuilder));

            applicationBuilder
                .UseMiddleware<HttpRequestIdentifierMiddleware>();

            return applicationBuilder;
        }

        /// <summary>
        /// Adds documentation middleware to the <see cref="IApplicationBuilder"/>.
        /// </summary>
        /// <param name="applicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        internal static IApplicationBuilder UseHttpDocumentataion(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
                throw new ArgumentNullException(nameof(applicationBuilder));

            var services = applicationBuilder.ApplicationServices;
            var appOptions = services.GetService<AppOptions>() ?? new AppOptions();

            applicationBuilder
                .UseSwagger(x =>
                {
                    x.RouteTemplate = "docs/{documentName}/swagger.json";
                })
                .UseSwaggerUI(x =>
                {
                    x.EnableFilter();
                    x.EnableDeepLinking();
                    x.EnableValidator(null);
                    x.ShowExtensions();
                    x.DisplayOperationId();
                    x.DisplayRequestDuration();
                    x.MaxDisplayedTags(-1);
                    x.DefaultModelExpandDepth(2);
                    x.DefaultModelsExpandDepth(1);
                    x.DefaultModelRendering(ModelRendering.Example);
                    x.DocExpansion(DocExpansion.None);

                    x.RoutePrefix = "docs";
                    x.DocumentTitle = $"Nano - {appOptions.Name} Docs v{appOptions.Version} ({ConfigManager.Environment})";
                    x.SwaggerEndpoint($"/docs/{appOptions.Version}/swagger.json", $"Nano - {appOptions.Name} v{appOptions.Version} ({ConfigManager.Environment})");
                });

            return applicationBuilder;
        }

        /// <summary>
        /// Adds localization middleware to the <see cref="IApplicationBuilder"/>.
        /// </summary>
        /// <param name="applicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        internal static IApplicationBuilder UseHttpLocalization(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
                throw new ArgumentNullException(nameof(applicationBuilder));

            var services = applicationBuilder.ApplicationServices;
            var appOptions = services.GetService<AppOptions>() ?? new AppOptions();

            applicationBuilder
                .UseRequestLocalization(new RequestLocalizationOptions
                {
                    DefaultRequestCulture = new RequestCulture(appOptions.Cultures.Default),
                    SupportedCultures = appOptions.Cultures.Supported
                        .Select(x => new CultureInfo(x)).ToArray(),
                    SupportedUICultures = appOptions.Cultures.Supported
                        .Select(x => new CultureInfo(x)).ToArray()
                });

            return applicationBuilder;
        }

        /// <summary>
        /// Adds http session middleware to the <see cref="IApplicationBuilder"/>.
        /// </summary>
        /// <param name="applicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        internal static IApplicationBuilder UseHttpSession(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
                throw new ArgumentNullException(nameof(applicationBuilder));

            var services = applicationBuilder.ApplicationServices;
            var webOptions = services.GetService<WebOptions>() ?? new WebOptions();

            applicationBuilder
                .UseSession(new SessionOptions
                {
                    IdleTimeout = webOptions.Hosting.UseHttpSessionTimeout
                });
            
            return applicationBuilder;
        }
                
        /// <summary>
        /// Adds no cache middleware to the <see cref="IApplicationBuilder"/>.
        /// </summary>
        /// <param name="applicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        internal static IApplicationBuilder UseHttpResponseCompression(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
                throw new ArgumentNullException(nameof(applicationBuilder));

            var services = applicationBuilder.ApplicationServices;
            var webOptions = services.GetService<WebOptions>() ?? new WebOptions();

            if (webOptions.Hosting.UseHttpResponseCompression)
            {
                applicationBuilder
                    .UseResponseCompression();
            }

            return applicationBuilder;
        }

        /// <summary>
        /// Adds no cache middleware to the <see cref="IApplicationBuilder"/>.
        /// </summary>
        /// <param name="applicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        internal static IApplicationBuilder UseHttpNoCacheHeader(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
                throw new ArgumentNullException(nameof(applicationBuilder));

            var services = applicationBuilder.ApplicationServices;
            var webOptions = services.GetService<WebOptions>() ?? new WebOptions();

            if (webOptions.Hosting.UseHttpNoCacheHeader)
            {
                applicationBuilder
                    .UseNoCacheHttpHeaders();
            }

            return applicationBuilder;
        }

        /// <summary>
        /// Adds no cache middleware to the <see cref="IApplicationBuilder"/>.
        /// </summary>
        /// <param name="applicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        internal static IApplicationBuilder UseHttpXForwardedHeaders(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
                throw new ArgumentNullException(nameof(applicationBuilder));

            var services = applicationBuilder.ApplicationServices;
            var webOptions = services.GetService<WebOptions>() ?? new WebOptions();

            if (webOptions.Hosting.UseHttpXForwardedHeaders)
            {
                applicationBuilder
                    .UseForwardedHeaders();
            }

            return applicationBuilder;
        }

        /// <summary>
        /// Adds x-download options middleware to the <see cref="IApplicationBuilder"/>.
        /// </summary>
        /// <param name="applicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        internal static IApplicationBuilder UseHttpXDownloadOptionsHeader(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
                throw new ArgumentNullException(nameof(applicationBuilder));

            var services = applicationBuilder.ApplicationServices;
            var webOptions = services.GetService<WebOptions>() ?? new WebOptions();

            if (webOptions.Hosting.UseHttpXDownloadOptionsHeader)
            {
                applicationBuilder
                    .UseXDownloadOptions();
            }
            
            return applicationBuilder;
        }

        /// <summary>
        /// Adds x-content-type options middleware to the <see cref="IApplicationBuilder"/>.
        /// </summary>
        /// <param name="applicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        internal static IApplicationBuilder UseHttpXContentTypeOptionsHeader(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
                throw new ArgumentNullException(nameof(applicationBuilder));

            var services = applicationBuilder.ApplicationServices;
            var webOptions = services.GetService<WebOptions>() ?? new WebOptions();

            if (webOptions.Hosting.UseHttpXContentTypeOptionsHeader)
            {
                applicationBuilder
                    .UseXContentTypeOptions();
            }
            
            return applicationBuilder;
        }

        /// <summary>
        /// Adds x-Robots tag middleware to the <see cref="IApplicationBuilder"/>.
        /// </summary>
        /// <param name="applicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        internal static IApplicationBuilder UseHttpXRobotsTagHeaders(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
                throw new ArgumentNullException(nameof(applicationBuilder));

            var services = applicationBuilder.ApplicationServices;
            var webOptions = services.GetService<WebOptions>() ?? new WebOptions();

            if (webOptions.Hosting.UseHttpXRobotsTagHeaders.IsEnabled)
            {
                var xRobotTags = webOptions.Hosting.UseHttpXRobotsTagHeaders;

                applicationBuilder
                    .UseXRobotsTag(x =>
                    {
                        if (xRobotTags.UseNoIndex)
                            x.NoIndex();

                        if (xRobotTags.UseNoFollow)
                            x.NoFollow();

                        if (xRobotTags.UseNoSnippet)
                            x.NoSnippet();

                        if (xRobotTags.UseNoArchive)
                            x.NoArchive();

                        if (xRobotTags.UseNoOdp)
                            x.NoOdp();

                        if (xRobotTags.UseNoTranslate)
                            x.NoTranslate();

                        if (xRobotTags.UseNoImageIndex)
                            x.NoImageIndex();
                    });
            }
            
            return applicationBuilder;
        }

        /// <summary>
        /// Adds referrer policy middleware to the <see cref="IApplicationBuilder"/>.
        /// </summary>
        /// <param name="applicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        internal static IApplicationBuilder UseHttpReferrerPolicyHeader(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
                throw new ArgumentNullException(nameof(applicationBuilder));

            var services = applicationBuilder.ApplicationServices;
            var webOptions = services.GetService<WebOptions>() ?? new WebOptions();

            if (webOptions.Hosting.UseHttpReferrerPolicyHeader != ReferrerPolicy.Disabled)
            {
                applicationBuilder
                    .UseReferrerPolicy(x =>
                    {
                        switch (webOptions.Hosting.UseHttpReferrerPolicyHeader)
                        {
                            case ReferrerPolicy.NoReferrer:
                                x.NoReferrer();
                                break;

                            case ReferrerPolicy.NoReferrerWhenDowngrade:
                                x.NoReferrerWhenDowngrade();
                                break;

                            case ReferrerPolicy.SameOrigin:
                                x.SameOrigin();
                                break;

                            case ReferrerPolicy.Origin:
                                x.Origin();
                                break;
                            
                            case ReferrerPolicy.StrictOrigin:
                                x.StrictOrigin();
                                break;
                            
                            case ReferrerPolicy.OriginWhenCrossOrigin:
                                x.OriginWhenCrossOrigin();
                                break;
                            
                            case ReferrerPolicy.StrictOriginWhenCrossOrigin:
                                x.StrictOriginWhenCrossOrigin();
                                break;

                            case ReferrerPolicy.UnsafeUrl:
                                x.UnsafeUrl();
                                break;
                        }
                    });
            }
            
            return applicationBuilder;
        }

        /// <summary>
        /// Adds X-frame options policy middleware to the <see cref="IApplicationBuilder"/>.
        /// </summary>
        /// <param name="applicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        internal static IApplicationBuilder UseHttpXFrameOptionsPolicyHeader(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
                throw new ArgumentNullException(nameof(applicationBuilder));

            var services = applicationBuilder.ApplicationServices;
            var webOptions = services.GetService<WebOptions>() ?? new WebOptions();

            if (webOptions.Hosting.UseHttpXFrameOptionsPolicyHeader != XFrameOptionsPolicy.Disabled)
            {
                applicationBuilder
                    .UseXfo(x =>
                    {
                        switch (webOptions.Hosting.UseHttpXFrameOptionsPolicyHeader)
                        {
                            case XFrameOptionsPolicy.Deny:
                                x.Deny();
                                break;

                            case XFrameOptionsPolicy.SameOrigin:
                                x.SameOrigin();
                                break;
                        }
                    });
            }
            
            return applicationBuilder;
        }

        /// <summary>
        /// Adds XXss protection policy middleware to the <see cref="IApplicationBuilder"/>.
        /// </summary>
        /// <param name="applicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        internal static IApplicationBuilder UseHttpXXssProtectionPolicyHeader(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
                throw new ArgumentNullException(nameof(applicationBuilder));

            var services = applicationBuilder.ApplicationServices;
            var webOptions = services.GetService<WebOptions>() ?? new WebOptions();

            if (webOptions.Hosting.UseHttpXXssProtectionPolicyHeader != XXssProtectionPolicyBlockMode.Disabled)
            {
                applicationBuilder
                    .UseXXssProtection(x =>
                    {
                        switch (webOptions.Hosting.UseHttpXXssProtectionPolicyHeader)
                        {
                            case XXssProtectionPolicyBlockMode.FilterDisabled:
                                x.Disabled();
                                break;

                            case XXssProtectionPolicyBlockMode.FilterEnabled:
                                x.Enabled();
                                break;

                            case XXssProtectionPolicyBlockMode.FilterEnabledBlockMode:
                                x.EnabledWithBlockMode();
                                break;
                        }
                    });
            }

            return applicationBuilder;
        }

        /// <summary>
        /// Adds Hsts middleware to the <see cref="IApplicationBuilder"/>.
        /// </summary>
        /// <param name="applicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        internal static IApplicationBuilder UseHttpStrictTransportSecurityPolicyHeader(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
                throw new ArgumentNullException(nameof(applicationBuilder));

            var services = applicationBuilder.ApplicationServices;
            var webOptions = services.GetService<WebOptions>() ?? new WebOptions();
            var hsts = webOptions.Hosting.Hsts;
                    
            if (hsts.IsEnabled)
            {
                var maxAge = hsts.MaxAge;

                applicationBuilder
                    .UseHsts(x =>
                    {
                        if (hsts.IncludeSubdomains)
                        {
                            x.IncludeSubdomains();

                            if (maxAge.HasValue)
                            {
                                const int MAX_WEEKS = 18;
                                var weeks = maxAge.Value.TotalDays / 7;  

                                if (hsts.UsePreload && weeks >= MAX_WEEKS)
                                    x.Preload();
                            }
                        }

                        if (maxAge.HasValue)
                        {
                            x.MaxAge(maxAge.Value.Days, maxAge.Value.Hours, maxAge.Value.Minutes, maxAge.Value.Seconds);
                        }
                    });
            }
            
            return applicationBuilder;
        }

        /// <summary>
        /// Adds rerdirect valiation middleware to the <see cref="IApplicationBuilder"/>.
        /// </summary>
        /// <param name="applicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        internal static IApplicationBuilder UseHttpsRedirect(this IApplicationBuilder applicationBuilder)
        {
            if (applicationBuilder == null)
                throw new ArgumentNullException(nameof(applicationBuilder));

            var services = applicationBuilder.ApplicationServices;
            var webOptions = services.GetService<WebOptions>() ?? new WebOptions();

            if (webOptions.Hosting.UseHttpsRedirect)
            {
                applicationBuilder
                    .UseCsp(x => x.UpgradeInsecureRequests());

                applicationBuilder
                    .UseRedirectValidation(x =>
                    {
                        x.AllowSameHostRedirectsToHttps(webOptions.Hosting.PortsHttps);
                    });
            }
            
            return applicationBuilder;
        }
    }
}