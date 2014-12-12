using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

[assembly: OwinStartup(typeof(visualisations_web.App_Start.Startup))]

namespace visualisations_web.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            app.UseWebApi(GetDefaultHttpConfiguration());
        }

        private System.Web.Http.HttpConfiguration GetDefaultHttpConfiguration()
        {
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute("default", "api/{Controller}");
            return config;
        }
    }
}