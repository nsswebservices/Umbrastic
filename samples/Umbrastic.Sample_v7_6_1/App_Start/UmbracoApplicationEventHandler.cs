using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;

namespace Umbrastic.Sample_v7_6_1
{
    public class UmbracoApplicationEventHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            GlobalConfiguration.Configuration.MapHttpAttributeRoutes();
            RouteTable.Routes.MapMvcAttributeRoutes();
        }
    }
}