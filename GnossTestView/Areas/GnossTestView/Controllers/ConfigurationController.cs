using GnossTestView.Areas.GnossTestView.Model;
using GnossTestView.Areas.GnossTestView.Utilidades;
using System;
using System.Linq;
using System.Web.Mvc;

namespace GnossTestView.Areas.GnossTestView.Controllers
{
    public class ConfigurationController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            ConfigurationModel configuracion = new ConfigurationModel();
            configuracion.urlApiDespliegues = UtilConfiguration.GetConfiguration("urlApiDespliegues");
            configuracion.autorizacion = UtilConfiguration.GetAuthorizationConfig();
            configuracion.proyectos = UtilManageViews.ObtenerProyectosVistas();

            return View(configuracion);
        }

        [HttpPost]
        public ActionResult SaveConfiguration(ConfigurationModel configuracion)
        {
            UtilConfiguration.SetConfiguracion("urlApiDespliegues", configuracion.urlApiDespliegues);
            UtilConfiguration.SetAuthorizationConfig(configuracion.autorizacion);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult DownloadViews(string proyectName)
        {
            string[] proyectos = UtilManageViews.ObtenerProyectosVistas();

            if (proyectos.Contains(AppContext.BaseDirectory + "Views\\" + proyectName))
            {
                UtilManageViews.DescargarVistas(proyectName);
                return Json(true);
            }

            return Json(false);
        }

        [HttpPost]
        public ActionResult UploadViews(string proyectName, string userUpload)
        {
            string[] proyectos = UtilManageViews.ObtenerProyectosVistas();

            if (proyectos.Contains(AppContext.BaseDirectory + "Views\\" + proyectName))
            {
                
                return Json(true);
            }

            return Json(false);
        }
    }
}