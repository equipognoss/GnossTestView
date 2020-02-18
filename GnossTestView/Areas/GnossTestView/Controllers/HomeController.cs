using Es.Riam.Gnoss.Web.MVC.Models.ViewModels;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Runtime.Serialization.Formatters.Binary;
using Es.Riam.Gnoss.Web.MVC.Models;
using GnossTestView.Areas.GnossTestView.Utilidades;
using System.Collections.Generic;

namespace GnossTestView.Areas.GnossTestView.Controllers
{
    public class HomeController : Controller
    {

        private const string TYPE_JSON = "$type\":\"";

        public ActionResult Index()
        {
            List<string> lastURLs = UtilConfiguration.GetAutocompleteData("url");
            lastURLs.Reverse();

            ViewBag.LastURLs = lastURLs;
            ViewBag.LastSessionID = UtilConfiguration.GetAutocompleteData("sessionID");
            
            return View();
        }

        public ActionResult LoadURL(string url, string submit, string sessionID, string user, string password, bool contentLocal)
        {
            try
            {
                GestionarDatosSession(url, ref user, ref password);

                if (sessionID != "")
                {
                    UtilConfiguration.SetAutocompleteData("sessionID", sessionID);
                }

                List<string> urlsAutocomplete = UtilConfiguration.GetAutocompleteData("url");
                if(urlsAutocomplete.Contains(url))
                {
                    urlsAutocomplete.Remove(url);
                }
                else if (urlsAutocomplete.Count > 5) {
                    urlsAutocomplete.RemoveAt(0);
                }
                urlsAutocomplete.Add(url);

                UtilConfiguration.SetAutocompleteData("url", urlsAutocomplete);
            }
            catch
            {
                return View("Error");
            }

            string responseFromServer = null;
            string jsonModel = null;

            if (!string.IsNullOrEmpty(url))
            {
                responseFromServer = GetData(url, submit, sessionID, user, password);
            }

            string proyectoSeleccionado;

            if (string.IsNullOrEmpty(responseFromServer))
            {
                return View("Error");
            }
            else
            {
                jsonModel = GetJson(responseFromServer, contentLocal, out proyectoSeleccionado);
            }
            
            string[] proyectos = UtilManageViews.ObtenerProyectosVistas(); 

            if (!proyectos.Contains(AppContext.BaseDirectory + "Views\\ecosistema"))
            {
                UtilManageViews.DescargarVistasInicial("ecosistema");
            }
            if (!proyectos.Contains(AppContext.BaseDirectory + "Views\\" + proyectoSeleccionado))
            {
                UtilManageViews.DescargarVistasInicial(proyectoSeleccionado);
            }

            return GetView(jsonModel, proyectoSeleccionado);
        }

        private void GestionarDatosSession(string url, ref string user, ref string password)
        {
            Session.Remove("ErrorAskURL");

            string domainUri = "";
            try
            {
                domainUri = new Uri(url).Host;
            }
            catch (Exception ex)
            {
                Session["ErrorAskURL"] = "La url no es valida";
                throw ex;
            }

            if (string.IsNullOrEmpty(user) && string.IsNullOrEmpty(password))
            {
                UtilConfiguration.GetAuthorization(domainUri, ref user, ref password);
            }
            else
            {
                UtilConfiguration.SetAuthorization(domainUri, user, password);
            }
        }

        private ActionResult GetView(string jsonModel, string proyectoSeleccionado)
        {
            string controllerName = (string)ViewData["ControllerName"];
            string actionName = (string)ViewData["ActionName"];
            if (actionName == null) { actionName = "Index"; }

            string modelType = "undefined";

            string rutaVistasPersonalizadas = $"Views/{proyectoSeleccionado}/Proyectos/{proyectoSeleccionado}/Vistas";
            string rutaVistasPersonalizadasEcosistema = $"Views/ecosistema/Proyectos/mygnoss/Vistas";

            ViewBag.rutaVistasPersonalizadas = rutaVistasPersonalizadas;
            ViewBag.rutaVistasPersonalizadasEcosistema = rutaVistasPersonalizadasEcosistema;

            if (!string.IsNullOrEmpty(controllerName))
            {
                modelType = GetModelType(jsonModel);

                if (modelType.Equals("ResourceViewModel") && controllerName.Equals("FichaRecurso"))
                {
                    MemoryStream mStream = new MemoryStream(ASCIIEncoding.Default.GetBytes(jsonModel));
                    BinaryFormatter deserializer = new BinaryFormatter();
                    ResourceViewModel modelRM = (ResourceViewModel)deserializer.Deserialize(mStream);

                    // Comprobamos si el modelo contiene RdfType y mostramos la vista correspondiente
                    string rdfType = modelRM.Resource.RdfType;
                    if (!string.IsNullOrEmpty(rdfType))
                    {
                        string viewName = $"~/{rutaVistasPersonalizadas}/Recursos/{rdfType}.cshtml";
                        if (System.IO.File.Exists(AppContext.BaseDirectory + viewName.Replace("~/", "")))
                        {
                            return View(viewName, modelRM);
                        }
                    }
                    return View($"~/{rutaVistasPersonalizadas}/Views/FichaRecurso/Index.cshtml", modelRM);
                }
                else
                {
                    if (controllerName.Equals("HomeComunidad") && modelType.Equals("CMSBlock")) { controllerName = "CMSPagina"; actionName = "Index"; }
                    if (controllerName.Equals("Busqueda")) { actionName = "Index"; }
                    if (modelType.Equals("Error404ViewModel")) { controllerName = "Error"; actionName = "Error404"; }

                    string view = $"~/{rutaVistasPersonalizadas}/Views/{controllerName}/{actionName}.cshtml";

                    if (System.IO.File.Exists(AppContext.BaseDirectory + view.Replace($"~/{rutaVistasPersonalizadas}/Views/", "GenericViews/")))
                    {
                        //Solamente pintamos la vista si existe una vista generica con esta ruta.
                        Type type = (Type.GetType(modelType));
                        return View(view, DeserializeObject(jsonModel, type));
                    }
                }
            }

            Session["ErrorAskURL"] = $"No se ha podido pintar la página de tipo '{controllerName}/{actionName}' con el Modelo de tipo '{modelType}'";

            return View("Error");
        }

        private string GetData(string url, string submit, string sessionID, string user, string password)
        {
            string cacheFileName = UtilPageCache.GetFileName(url);

            string pageResult;

            if (submit == "Recargar" || !System.IO.File.Exists(cacheFileName))
            {
                pageResult = UtilPageRequest.GetRequest(url, user, password, sessionID);

                if(!string.IsNullOrWhiteSpace(pageResult))
                {
                    UtilPageCache.SaveCacheFile(cacheFileName, pageResult);
                }
            }
            else
            {
                pageResult = System.IO.File.ReadAllText(cacheFileName);
            }

            return pageResult;
        }

        private string GetJson(string responseFromServer, bool contentLocal, out string proyectoSeleccionado)
        {
            // Dividimos el json obtenido, la primera string para el modelo y la segunda para el ViewBag 
            string[] divideJson = responseFromServer.Split(new string[] { "{ComienzoJsonViewData}" }, StringSplitOptions.None);
            string jsonModel = divideJson[0];
            string jsonViewData = "";
            if (divideJson.Length > 1)
            {
                jsonViewData = divideJson[1];
            }

            // Deserializamos ViewData
            JsonSerializerSettings jsonSerializerSettingsSimple = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            };
            ViewDataDictionary ViewDataDeserializado = JsonConvert.DeserializeObject<ViewDataDictionary>(jsonViewData, jsonSerializerSettingsSimple);

            object Comunidad = ViewDataDeserializado.FirstOrDefault(item => item.Key.Equals("Comunidad")).Value;
            if (Comunidad != null)
            {
                proyectoSeleccionado = ((CommunityModel)Comunidad).ShortName;
            }
            else
            {
                proyectoSeleccionado = "ecosistema";
            }

            if (ViewDataDeserializado != null)
            {
                foreach (string item in ViewDataDeserializado.Keys)
                {
                    if (item.Equals("BaseUrlPersonalizacion") && contentLocal)
                    {
                        ViewData.Add(item, $"styles/{proyectoSeleccionado}/Proyectos/{proyectoSeleccionado}/Estilos");
                    }
                    else if(item.Equals("BaseUrlPersonalizacionEcosistema") && contentLocal)
                    {
                        ViewData.Add(item, $"styles/ecosistema/Proyectos/Proyectos/Estilos");
                    }
                    else
                    {
                        ViewData.Add(item, ViewDataDeserializado[item]);
                    }
                }
            }

            return jsonModel;
        }

        private string GetModelType(string jsonModel)
        {
            //// Obtenemos tipo del modelo
            int from = jsonModel.IndexOf(TYPE_JSON);
            string modelType = "";
            if (from >= 0)
            {
                from += TYPE_JSON.Length;
                int to = jsonModel.IndexOf(",", from);
                modelType = (jsonModel.Substring(from, to - from)).Split('.').Last();
            }
            else if (jsonModel.Equals("false") || jsonModel.Equals("true"))
            {
                modelType = "bool";
            }
            else if (ViewData.ContainsKey("ControllerName") && ViewData["ControllerName"].Equals("FichaRecurso"))
            {
                modelType = "ResourceViewModel";
            }
            return modelType;
        }

        /// <summary>
        /// Se crea un objeto ViewResult utilizando el nombre de la vista, el nombre de la página maestra y el modelo que presenta una vista.
        /// Guarda en ViewBag.ViewPath la ruta relativa de la vista.
        /// </summary>
        protected override ViewResult View(string viewName, string masterName, object model)
        {
            string rutaVistasPersonalizadas = ViewBag.rutaVistasPersonalizadas;

            if (rutaVistasPersonalizadas != null && viewName.Contains(rutaVistasPersonalizadas))
            {
                ViewBag.ViewPath = viewName.Substring(0, viewName.LastIndexOf('/'));

                if (!System.IO.File.Exists(AppContext.BaseDirectory + viewName.Replace("~/", "")))
                {
                    string rutaVistasPersonalizadasEcosistema = ViewBag.rutaVistasPersonalizadasEcosistema;
                    viewName = viewName.Replace($"/{rutaVistasPersonalizadas}/Views/", $"/{rutaVistasPersonalizadasEcosistema}/Views/");

                    if (!System.IO.File.Exists(AppContext.BaseDirectory + viewName.Replace("~/", "")))
                    {
                        viewName = viewName.Replace($"/{rutaVistasPersonalizadasEcosistema}/Views/", "/GenericViews/");
                    }
                }
            }

            return base.View(viewName, masterName, model);
        }

        /// <summary>
        /// Obtenemos un objeto deserializado con las opciones de deserialización PreserveReferencesHandling.Objects y TypeNameHandling.All
        /// </summary>
        private object DeserializeObject(string json, Type type)
        {
            JsonSerializerSettings jsonSerializerSettingsObject = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = TypeNameHandling.All
            };

            return JsonConvert.DeserializeObject(json, type, jsonSerializerSettingsObject);
        }

        public bool IsValidDirName(string dirName)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidPathChars()));

            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return !Regex.IsMatch(dirName, invalidRegStr);
        }
    }
}
