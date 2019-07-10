using System.Web.Mvc;

namespace GnossTestView.Areas.GnossTestView
{
    public class GnossTestViewAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "GnossTestView";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "GnossTestView_askUrl",
                "AskURL",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            context.MapRoute(
                "GnossTestView_home",
                "home",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            context.MapRoute(
                "GnossTestView_default",
                "{action}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            context.MapRoute(
                "GnossTestView_default2",
                "GnossTestView/{action}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}