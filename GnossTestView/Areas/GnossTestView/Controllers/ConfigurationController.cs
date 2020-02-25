using GnossTestView.Areas.GnossTestView.Model;
using GnossTestView.Areas.GnossTestView.Utilidades;
using System;
using System.Collections.Generic;
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
            configuracion.urlApiIntegracionEntornos = UtilConfiguration.GetConfiguration("urlApiIntegracionEntornos");

            configuracion.usuarioRootRepositorio = new ConfigurationModel.UserPaswordModel();
            configuracion.usuarioRootRepositorio.user = UtilConfiguration.GetConfiguration("usuarioRootRepositorio/user");
            configuracion.usuarioRootRepositorio.password = UtilConfiguration.GetConfiguration("usuarioRootRepositorio/password");

            configuracion.serverFtpUpload = new ConfigurationModel.SerferFtpModel();
            configuracion.serverFtpUpload.server = UtilConfiguration.GetConfiguration("serverFtpUpload/server");
            configuracion.serverFtpUpload.port = UtilConfiguration.GetConfiguration("serverFtpUpload/port");

            configuracion.autorizacion = UtilConfiguration.GetAuthorizationConfig();

            configuracion.proyectos = new List<ConfigurationModel.ProyectoRamaModel>();
            string[] proyectos =  UtilGitFiles.ObtenerProyectosVistas();

            foreach (string nombreProy in proyectos)
            {
                ConfigurationModel.ProyectoRamaModel proyecto = new ConfigurationModel.ProyectoRamaModel();
                proyecto.nombreCortoProyecto = nombreProy.Split('\\').Last();
                proyecto.rama = UtilGitFiles.ObrenerRamaRepositorioLocal(proyecto.nombreCortoProyecto);
                
                proyecto.userFTP = UtilConfiguration.GetConfiguration($"proyectos/proyecto[@name=\'{proyecto.nombreCortoProyecto}\']/userFTP");
                proyecto.passwordFTP = UtilConfiguration.GetConfiguration($"proyectos/proyecto[@name=\'{proyecto.nombreCortoProyecto}\']/passwordFTP");

                proyecto.localChanges = UtilGitFiles.GetFilesWithChanges(proyecto.nombreCortoProyecto);

                proyecto.hasRemoteChanges = UtilGitFiles.CheckServerChanges(proyecto.nombreCortoProyecto);

                configuracion.proyectos.Add(proyecto);
            }



            return View(configuracion);
        }

        [HttpPost]
        public ActionResult SaveConfiguration(ConfigurationModel configuracion)
        {
            UtilConfiguration.SetConfiguracion("/config/urlApiIntegracionEntornos", configuracion.urlApiIntegracionEntornos);

            UtilConfiguration.SetConfiguracion("/config/usuarioRootRepositorio/user", configuracion.usuarioRootRepositorio.user);
            UtilConfiguration.SetConfiguracion("/config/usuarioRootRepositorio/password", configuracion.usuarioRootRepositorio.password);

            UtilConfiguration.SetConfiguracion("/config/serverFtpUpload/server", configuracion.serverFtpUpload.server);
            UtilConfiguration.SetConfiguracion("/config/serverFtpUpload/port", configuracion.serverFtpUpload.port);

            UtilConfiguration.SetAuthorizationConfig(configuracion.autorizacion);

            return Json(true);
        }

        [HttpPost]
        public ActionResult DownloadProject(string proyectName)
        {
            string[] proyectos = UtilGitFiles.ObtenerProyectosVistas();

            if (proyectos.Contains(AppContext.BaseDirectory + "Views\\" + proyectName))
            {
                UtilGitFiles.DescargarVistas(proyectName);
                return Json(true);
            }

            return Json(false);
        }

        [HttpPost]
        public ActionResult UploadProject(string proyectName, string userFTP, string passwordFTP)
        {
            string[] proyectos = UtilGitFiles.ObtenerProyectosVistas();

            if (proyectos.Contains(AppContext.BaseDirectory + "Views\\" + proyectName))
            {
                UtilConfiguration.SetConfiguracion($"/config/proyectos/proyecto[@name=\'{proyectName}\']/userFTP", userFTP);
                UtilConfiguration.SetConfiguracion($"/config/proyectos/proyecto[@name=\'{proyectName}\']/passwordFTP", passwordFTP);

                string server = UtilConfiguration.GetConfiguration("serverFtpUpload/server");
                string port = UtilConfiguration.GetConfiguration("serverFtpUpload/port");

                //No se pueden subir las vistas, la estructura del FTP es distinta a la de GIT y el compilador
                UtilFtpFiles.SubirVistas(proyectName, $"{server}:{port}", userFTP, passwordFTP);

                return Json(true);
            }

            return Json(false);
        }
    }
}