using GnossTestView.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace GnossTestView
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {

            RouteConfig.RegisterRoutes(RouteTable.Routes);

            //Quitamos los motores de vistas y añadimos nuestro motor Razor personalizado
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new CustomRazorViewEngine());
        }
    }
}
