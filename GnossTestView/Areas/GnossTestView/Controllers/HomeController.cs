using Es.Riam.Gnoss.Web.MVC.Models.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;

namespace GnossTestView.Areas.GnossTestView.Controllers
{
    public class HomeController : Controller
    {
        private const string TYPE_JSON = "$type\":\"";
        
        public ActionResult Index()
        {
            if(Session["Proyecto"] == null)
            {
                string[] proyectos = System.IO.Directory.GetDirectories(AppContext.BaseDirectory + "Views");
                ViewBag.Proyectos = proyectos.Select(proy => proy.Split('\\').Last());

                return View("AskProyecto");
            }

            return View("AskURL");
        }
        public ActionResult ChangeProyecto()
        {
            string[] proyectos = System.IO.Directory.GetDirectories(AppContext.BaseDirectory + "Views");
            ViewBag.Proyectos = proyectos.Select(proy => proy.Split('\\').Last());

            return View("AskProyecto");                
        }

        public ActionResult SelectProyecto(string proySelected, string newProy)
        {
            string[] proyectos = Directory.GetDirectories(AppContext.BaseDirectory + "Views");
            if (proySelected != "newProy")
            {
                if (proyectos.Select(proy => proy.Split('\\').Last()).Contains(proySelected))
                {
                    Session.Add("Proyecto", proySelected);
                }
                else
                {
                    ViewBag.Proyectos = proyectos.Select(proy => proy.Split('\\').Last());
                    ViewBag.Error = "Selecciona un proyecto de la lista";
                    return View("AskProyecto");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(newProy) && IsValidDirName(newProy)){
                    Directory.CreateDirectory(AppContext.BaseDirectory + "Views/" + newProy);
                    Session.Add("Proyecto", newProy);
                }
                else
                {
                    ViewBag.Proyectos = proyectos.Select(proy => proy.Split('\\').Last());
                    ViewBag.Error = "El nombre no puedeser vacio ni contener caracteres especiales";
                    return View("AskProyecto");
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult LoadURL(string url, string submit, string sessionID, string user, string password, bool contentLocal)
        {
            try
            {
                GestionarDatosSession(url, sessionID, user, password, contentLocal);
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

            if(string.IsNullOrEmpty(responseFromServer))
            {
                return View("Error");
            }
            else
            {
                jsonModel = GetJson(responseFromServer, contentLocal);
            }

            return GetView(jsonModel);
        }

        private void GestionarDatosSession(string url, string sessionID, string user, string password, bool contentLocal)
        {
            Session.Remove("ErrorAskURL");

            Session.Add("URL", url);
            Session.Add("SessionID", sessionID);
            
            Session.Add("ContentLocal", contentLocal);

            string domainUri = "";
            try
            {
                domainUri = new Uri(url).Host;
            }
            catch(Exception ex)
            {
                Session["ErrorAskURL"] = "La url no es valida";
                throw ex;
            }

            if (string.IsNullOrEmpty(user) && string.IsNullOrEmpty(password))
            {
                GetAuthorization(domainUri, ref user, ref password);
            }
            else
            {
                SetAuthorization(domainUri, user, password);
            }
        }

        private ActionResult GetView(string jsonModel)
        {
            string controllerName = (string)ViewData["ControllerName"];
            string actionName = (string)ViewData["ActionName"];
            if(actionName == null) { actionName = "Index"; }

            string modelType = "undefined";

            string rutaVistasPersonalizadas = $"Views/{Session["Proyecto"]}";

            ViewBag.rutaVistasPersonalizadas = rutaVistasPersonalizadas;

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
                        string viewName = $"~/{rutaVistasPersonalizadas}/FichaRecurso/{rdfType}.cshtml";
                        if (System.IO.File.Exists(AppContext.BaseDirectory + viewName.Replace("~/", "")))
                        {
                            return View(viewName, modelRM);
                        }
                    }
                    return View($"~/{rutaVistasPersonalizadas}/FichaRecurso/Index.cshtml", modelRM);
                }
                else
                {
                    if (controllerName.Equals("HomeComunidad") && modelType.Equals("CMSBlock")) { controllerName = "CMSPagina"; actionName = "Index"; }
                    if (controllerName.Equals("Busqueda")) { actionName = "Index"; }
                    if (modelType.Equals("Error404ViewModel")) { controllerName = "Error"; actionName = "Error404"; }

                    string view = $"~/{rutaVistasPersonalizadas}/{controllerName}/{actionName}.cshtml";

                    if (System.IO.File.Exists(AppContext.BaseDirectory + view.Replace($"~/{rutaVistasPersonalizadas}/", "GenericViews/")))
                    {
                        Type type = (Type.GetType(modelType));
                        return View(view, DeserializeObject(jsonModel, type));
                    }
                }
            }

            Session["ErrorAskURL"] = $"No se ha podido pintar la página de tipo '{controllerName}/{actionName}' con el Modelo de tipo '{modelType}'";

            return View("Error");
        }

        private string GetFileName(string pUrl)
        {
            string fileName = pUrl.Substring(pUrl.IndexOf("//") + 2).Trim('/');

            if (fileName.Contains("/"))
            {
                fileName = pUrl.Substring(pUrl.IndexOf('/'));
                fileName = fileName.Replace('?', '-');
                fileName = fileName.Replace('&', '-');
                fileName = fileName.Replace('=', '-');
                fileName = fileName.Replace(':', '-');
                fileName = fileName.Replace(';', '-');
            }
            if (string.IsNullOrEmpty(fileName)) { fileName = "home"; }

            return $"~/App_Data/{fileName}.txt";
        }

        private void GetAuthorization(string url, ref string user, ref string password)
        {
            KeyValuePair<string, string>? datosAuth = ObtenerAuthorizationConfig(url);
            if (!datosAuth.HasValue)
            {
                datosAuth = ObtenerAuthorizationConfig("default");
            }

            if(datosAuth.HasValue)
            {
                user = datosAuth.Value.Key;
                password = datosAuth.Value.Value;
            }
        }

        private void SetAuthorization(string url, string user, string password)
        {
            string rutaConfig = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "Config\\Authorization.config";
            if (System.IO.File.Exists(rutaConfig))
            {
                XmlDocument docXml = new XmlDocument();
                docXml.Load(rutaConfig);

                XmlNode parentNode = docXml.SelectSingleNode("config");

                XmlNode nodeProy = parentNode.SelectSingleNode($"proyecto[@url=\"{url}\"]");
                if (nodeProy != null)
                {
                    parentNode.RemoveChild(nodeProy);
                }

                XmlElement newConfig = docXml.CreateElement("proyecto");
                newConfig.SetAttribute("url", url);

                XmlElement nodeUser = docXml.CreateElement("user");
                nodeUser.InnerText = user;
                newConfig.AppendChild(nodeUser);

                XmlElement nodePass = docXml.CreateElement("pass");
                nodePass.InnerText = password;
                newConfig.AppendChild(nodePass);

                parentNode.AppendChild(newConfig);

                docXml.Save(rutaConfig);
            }
        }
    
        private KeyValuePair<string, string>? ObtenerAuthorizationConfig(string url)
        {
            string rutaConfig = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "Config\\Authorization.config";
            if (System.IO.File.Exists(rutaConfig))
            {
                XmlDocument docXml = new XmlDocument();
                docXml.Load(rutaConfig);

                XmlNode nodeProy = docXml.SelectSingleNode($"config/proyecto[@url=\"{url}\"]");
                if (nodeProy != null)
                {
                    XmlNode userNode = nodeProy.SelectSingleNode("user");
                    XmlNode passwordNode = nodeProy.SelectSingleNode("pass");

                    if(userNode != null && passwordNode != null)
                    {
                        return new KeyValuePair<string, string>(userNode.InnerText, passwordNode.InnerText);
                    }
                }
            }
            return null;
        }

        private string GetData(string url, string submit, string sessionID, string user, string password)
        {
            string jsonURL = GetFileName(url);

            string responseFromServer;

            if (submit == "Recargar" || !System.IO.File.Exists(Server.MapPath(jsonURL)))
            {
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);

                // Cambiamos la propiedad 'Accept' para aceptar json
                myHttpWebRequest.Accept = "application/json";
                myHttpWebRequest.Timeout = 100000000;

                if (!string.IsNullOrEmpty(user))
                {
                    var plainTextBytes = System.Text.Encoding.UTF8.GetBytes($"{user}:{password}");
                    string base64Auth = System.Convert.ToBase64String(plainTextBytes);
                    myHttpWebRequest.Headers.Add(HttpRequestHeader.Authorization, $"Basic {base64Auth}");
                }

                myHttpWebRequest.CookieContainer = new CookieContainer();
                if (!string.IsNullOrEmpty(sessionID) )
                {
                   myHttpWebRequest.CookieContainer.Add(new Cookie("ASP.NET_SessionId", sessionID, "/", myHttpWebRequest.RequestUri.Host));
                }

                try
                {
                    HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();

                    Stream dataStream = myHttpWebResponse.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    // Leemos el contenido de la respuesta
                    responseFromServer = reader.ReadToEnd();
                    reader.Close();
                    dataStream.Close();
                    myHttpWebResponse.Close();

                    // Guardamos el json obtenido en App_Data
                    FileInfo file = new FileInfo(Server.MapPath(jsonURL));
                    file.Directory.Create();
                    System.IO.File.WriteAllText(file.FullName, responseFromServer);
                }
                catch (Exception ex)
                {
                    string error = ex.Message;
                    Session["ErrorAskURL"] = error;
                    responseFromServer = null;
                }
            }
            else
            {
                responseFromServer = System.IO.File.ReadAllText(Server.MapPath(jsonURL));
            }

            return responseFromServer;
        }

        private string GetJson(string responseFromServer, bool contentLocal)
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
            if (ViewDataDeserializado != null)
            {
                foreach (string item in ViewDataDeserializado.Keys)
                {
                    if (item.Equals("BaseUrlPersonalizacion") && contentLocal)
                    {
                        ViewData.Add(item, $"styles/{Session["Proyecto"]}");
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
                    viewName = viewName.Replace($"/{rutaVistasPersonalizadas}/", "/GenericViews/");
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
