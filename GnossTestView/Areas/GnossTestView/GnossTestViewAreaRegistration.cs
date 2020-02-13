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
                "GnossTestView_LoadURL",
                "LoadURL",
                new { controller = "Home", action = "LoadURL" }
            );

            context.MapRoute(
                "GnossTestView_default",
                "{controller}/{action}",
                new { controller = "Home", action = "Index" }
            );

        }
    }
}