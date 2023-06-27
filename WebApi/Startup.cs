using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Owin;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Xml;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;

namespace CQCopyPasteAdapter.WebApi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            try
            {
                HttpConfiguration config = new HttpConfiguration();
                config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
                config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{id}",
                    defaults: new { id = RouteParameter.Optional }

                );
                app.UseWebApi(config);
            }
            catch (Exception ex)
            {
                //
            }

        }
    }
}
