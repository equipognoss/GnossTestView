using Es.Riam.Gnoss.Web.MVC.Models;
using Es.Riam.Gnoss.Web.MVC.Models.ViewModels;
using Es.Riam.Semantica.OWL;
using Es.Riam.Semantica.Plantillas;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data.SQLite;

namespace GnossTestView.Controllers
{
    public class HomeController : Controller
    {
        private const string TYPE_JSON = "$type\":\"";
        
        public ActionResult Index()
        {
            ViewBag.Browser = GetBrowser();
            return View("~/GenericViews/GnossTestView/AskURL.cshtml");
        }

        public ActionResult LoadURL(string url, string submit, string sessionID, string user, string password)
        {
            ViewBag.Browser = GetBrowser();

            string responseFromServer = null;
            string jsonModel = null;

            if (!string.IsNullOrEmpty(url))
            {
                responseFromServer = GetData(url, submit, sessionID, user, password);
            }

            if(string.IsNullOrEmpty(responseFromServer))
            {
                return View("~/Views/GnossTestView/AskURL.cshtml");
            }
            else
            {
                jsonModel = GetJson(responseFromServer);
            }

            return GetView(jsonModel);
        }

        private ActionResult GetView(string jsonModel)
        {
            string controllerName = (string)ViewData["ControllerName"];
            string actionName = (string)ViewData["ActionName"];
            if(actionName == null) { actionName = "Index"; }

            string modelType = "undefined";

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
                        string viewName = $"~/Views/FichaRecurso/{rdfType}.cshtml";
                        if (System.IO.File.Exists(AppContext.BaseDirectory + viewName.Replace("~/", "")))
                        {
                            return View(viewName, modelRM);
                        }
                    }
                    return View("~/Views/FichaRecurso/Index.cshtml", modelRM);
                }
                else
                {
                    if (controllerName.Equals("HomeComunidad") && modelType.Equals("CMSBlock")) { controllerName = "CMSPagina"; actionName = "Index"; }
                    if (controllerName.Equals("Busqueda")) { actionName = "Index"; }
                    if (modelType.Equals("Error404ViewModel")) { controllerName = "Error"; actionName = "Error404"; }

                    string view = $"~/Views/{controllerName}/{actionName}.cshtml";

                    if (System.IO.File.Exists(AppContext.BaseDirectory + view.Replace("~/Views/", "GenericViews/")))
                    {
                        Type type = (Type.GetType(modelType));
                        return View(view, DeserializeObject(jsonModel, type));
                    }
                }
            }

            ViewBag.ErrorAskURL = $"No se ha podido pintar la página de tipo '{controllerName}/{actionName}' con el Modelo de tipo '{modelType}'";
            return View("~/Views/GnossTestView/AskURL.cshtml");
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
        
        private string GetData(string url, string submit, string sessionID, string user, string password)
        {
            string jsonURL = GetFileName(url);

            string responseFromServer;

            if (submit == "Recargar página" || !System.IO.File.Exists(Server.MapPath(jsonURL)))
            {
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);

                // Cambiamos la propiedad 'Accept' para aceptar json
                myHttpWebRequest.Accept = "application/json";
                myHttpWebRequest.Timeout = 100000000;

                if (!string.IsNullOrEmpty(user))
                {
                    if (ViewBag.Browser == "chrome" && user == "true")
                    {
                        string strHost = new Uri(url).Host;
                        KeyValuePair<string, string> userAutentication = GetLoginData_Chrome(strHost);

                        if (userAutentication.Key != "")
                        {
                            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes($"{userAutentication.Key}:{userAutentication.Value}");
                            string base64Auth = System.Convert.ToBase64String(plainTextBytes);
                            myHttpWebRequest.Headers.Add(HttpRequestHeader.Authorization, $"Basic {base64Auth}");
                        }
                    }
                    else if (!string.IsNullOrEmpty(user))
                    {
                        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes($"{user}:{password}");
                        string base64Auth = System.Convert.ToBase64String(plainTextBytes);
                    myHttpWebRequest.Headers.Add(HttpRequestHeader.Authorization, $"Basic {base64Auth}");
                    }
                }

                myHttpWebRequest.CookieContainer = new CookieContainer();
                if (!string.IsNullOrEmpty(sessionID) )
                {
                    if (ViewBag.Browser == "chrome" && sessionID == "true")
                    {
                        myHttpWebRequest.CookieContainer.Add(GetSessionIDCookie(url));
                    }
                    else
                    {
                        myHttpWebRequest.CookieContainer.Add(new Cookie("ASP.NET_SessionId", sessionID, "/", myHttpWebRequest.RequestUri.Host));
                    }
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
                    ViewBag.ErrorAskURL = error;
                    responseFromServer = null;
                }
            }
            else
            {
                responseFromServer = System.IO.File.ReadAllText(Server.MapPath(jsonURL));
            }

            return responseFromServer;
        }

        private string GetBrowser()
        {
            string browser = Request.Browser.Browser.ToLower();

            if (browser.Equals("chrome") && Request.UserAgent.IndexOf("Edge") > -1)
            {
                browser = "edge";
            }

            if (string.IsNullOrEmpty(Request.Params["__utmc"]))
            {
                browser += "_private";
            }
            return browser;
        }

        private Cookie GetSessionIDCookie(string url)
        {
            string strHost = new Uri(url).Host;

            string SESSION_ID = "ASP.NET_SessionId";
            string cookieValue = string.Empty;

            string browser = GetBrowser();

            switch (browser)
            {
                case "chrome":
                    cookieValue = GetCookie_Chrome(strHost, SESSION_ID);
                    break;
                //case "firefox":
                //    cookieValue = GetCookie_FireFox(strHost, SESSION_ID);
                //    break;
                //case "edge":
                //    cookieValue = GetCookie_Edge(strHost, SESSION_ID);
                //    break;
                //case "internetexplorer":
                //    cookieValue = GetCookie_InternetExplorer(strHost, SESSION_ID);
                //    break;
                //case "opera":
                //    cookieValue = GetCookie_Chrome(strHost, SESSION_ID);
                //    break;
            }

            return new Cookie(SESSION_ID, cookieValue, "/", strHost);
        }

        private string GetJson(string responseFromServer)
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
                    ViewData.Add(item, ViewDataDeserializado[item]);
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
            ViewBag.ViewPath = viewName.Substring(0, viewName.LastIndexOf('/'));

            if(!System.IO.File.Exists(AppContext.BaseDirectory + viewName.Replace("~/", "")))
            {
                viewName = viewName.Replace("/Views/", "/GenericViews/");
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

        private static string GetChromeCookiePath()
        {
            string s = Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);
            s += @"\Google\Chrome\User Data\Default\cookies";

            if (!System.IO.File.Exists(s))
            {
                return string.Empty;
            }
            else
            {
                FileInfo file = new FileInfo(s);
                string newFileName = Path.GetTempPath() + "Chrome_cookies" + DateTime.Now.ToString("yyyyMMddhhmmssffff");

                file.CopyTo(newFileName);
                return newFileName;
            }
        }

        private static string GetChromeLoginDataPath()
        {
            string s = Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);
            s += @"\Google\Chrome\User Data\Default\Login Data";

            if (!System.IO.File.Exists(s))
            {
                return string.Empty;
            }
            else
            {
                FileInfo file = new FileInfo(s);
                string newFileName = Path.GetTempPath() + "Chrome_Login Data" + DateTime.Now.ToString("yyyyMMddhhmmssffff");

                file.CopyTo(newFileName);
                return newFileName;
            }
        }

        private static string GetCookie_Chrome(string strHost, string strField)
        {
            string cookieValue = string.Empty;
            string strPath = GetChromeCookiePath();

            if (string.Empty == strPath)
                return string.Empty;

            try
            {
                string strDb = "Data Source=" + strPath + ";pooling=false";

                using (SQLiteConnection conn = new SQLiteConnection(strDb))
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT encrypted_value FROM cookies WHERE (host_key = '" + strHost + "' OR host_key = '." + strHost + "') " + 
                           " AND name LIKE '%" + strField + "%';";

                        conn.Open();
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                var encryptedValue = (byte[])reader[0];
                                var decryptedValue = System.Security.Cryptography.ProtectedData.Unprotect(encryptedValue, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
                                cookieValue = Encoding.ASCII.GetString(decryptedValue);

                                if (!cookieValue.Equals(string.Empty))
                                {
                                    break;
                                }
                            }
                        }
                        conn.Close();
                    }
                }
            }
            finally
            {
                System.IO.File.Delete(strPath);
            }

            return cookieValue;
        }


        private KeyValuePair<string, string> GetLoginData_Chrome(string strHost)
        {
            KeyValuePair<string, string> userValue = new KeyValuePair<string, string>("", "");
            string strPath = GetChromeLoginDataPath();

            if (string.Empty == strPath)
                return userValue;

            try
            {
                string strDb = "Data Source=" + strPath + ";pooling=false";

                using (SQLiteConnection conn = new SQLiteConnection(strDb))
                {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT username_value, password_value FROM logins WHERE origin_url LIKE '%://" +  strHost + "%' AND action_url = '';";

                        conn.Open();
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                var userName = reader.GetString(0);
                                var encryptedValue = (byte[])reader[1];
                                var decryptedValue = System.Security.Cryptography.ProtectedData.Unprotect(encryptedValue, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
                                userValue = new KeyValuePair<string, string>(userName, Encoding.ASCII.GetString(decryptedValue)) ;

                                if (!userName.Equals(string.Empty))
                                {
                                    break;
                                }
                            }
                        }
                        conn.Close();
                    }
                }
            }
            finally
            {
                System.IO.File.Delete(strPath);
            }

            return userValue;
        }

    }
}
