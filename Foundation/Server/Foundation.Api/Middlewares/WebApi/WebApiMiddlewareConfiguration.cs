﻿using Foundation.Api.Contracts;
using Foundation.Api.Middlewares.WebApi.Contracts;
using Foundation.Core.Contracts;
using Foundation.Core.Models;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Extensions;

namespace Foundation.Api.Middlewares.WebApi
{
    public class WebApiMiddlewareConfiguration : IOwinMiddlewareConfiguration, IDisposable
    {
        private readonly AppEnvironment _activeAppEnvironment;
        private readonly IEnumerable<IWebApiConfigurationCustomizer> _webApiConfgurationCustomizers;
        private readonly System.Web.Http.Dependencies.IDependencyResolver _webApiDependencyResolver;
        private readonly IWebApiOwinPipelineInjector _webApiOwinPipelineInjector;
        private HttpConfiguration _webApiConfig;

        protected WebApiMiddlewareConfiguration()
        {

        }

        public WebApiMiddlewareConfiguration(IAppEnvironmentProvider appEnvironmentProvider,
            IEnumerable<IWebApiConfigurationCustomizer> webApiConfgurationCustomizers, System.Web.Http.Dependencies.IDependencyResolver webApiDependencyResolver, IWebApiOwinPipelineInjector webApiOwinPipelineInjector)
        {
            if (appEnvironmentProvider == null)
                throw new ArgumentNullException(nameof(appEnvironmentProvider));

            if (webApiConfgurationCustomizers == null)
                throw new ArgumentNullException(nameof(webApiConfgurationCustomizers));

            if (webApiDependencyResolver == null)
                throw new ArgumentNullException(nameof(webApiDependencyResolver));

            if (webApiOwinPipelineInjector == null)
                throw new ArgumentNullException(nameof(webApiOwinPipelineInjector));

            _activeAppEnvironment = appEnvironmentProvider.GetActiveAppEnvironment();
            _webApiConfgurationCustomizers = webApiConfgurationCustomizers;
            _webApiDependencyResolver = webApiDependencyResolver;
            _webApiOwinPipelineInjector = webApiOwinPipelineInjector;
        }

        public virtual void Configure(IAppBuilder owinApp)
        {
            if (owinApp == null)
                throw new ArgumentNullException(nameof(owinApp));

            _webApiConfig = new HttpConfiguration();
            _webApiConfig.SuppressHostPrincipal();

            _webApiConfig.SetTimeZoneInfo(TimeZoneInfo.Utc);

            _webApiConfig.IncludeErrorDetailPolicy = _activeAppEnvironment.DebugMode ? IncludeErrorDetailPolicy.LocalOnly : IncludeErrorDetailPolicy.Never;

            _webApiConfgurationCustomizers.ToList()
                .ForEach(webApiConfigurationCustomizer =>
                {
                    webApiConfigurationCustomizer.CustomizeWebApiConfiguration(_webApiConfig);
                });

            _webApiConfig.DependencyResolver = _webApiDependencyResolver;

            HttpServer server = new HttpServer(_webApiConfig);

            _webApiConfig.MapHttpAttributeRoutes();

            _webApiConfig.Routes.MapHttpRoute(name: "default", routeTemplate: "api/{controller}/{action}", defaults: new { action = RouteParameter.Optional });

            owinApp.UseAutofacWebApi(_webApiConfig);

            _webApiOwinPipelineInjector.UseWebApiOData(owinApp, server);

            _webApiConfig.EnsureInitialized();
        }

        public virtual void Dispose()
        {
            _webApiConfig.Dispose();
        }
    }
}