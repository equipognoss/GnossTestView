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

namespace GnossTestView.Controllers
{
    public class HomeController : Controller
    {
        private const string TYPE_JSON = "$type\":\"";

        public ActionResult Index()
        {
            return View("~/Views/GnossTestView/AskURL.cshtml");
        }

        public ActionResult LoadURL(string url, string submit, string sessionID, string user, string password)
        {
            if(String.IsNullOrEmpty(url))
            {
                return View("~/Views/GnossTestView/AskURL.cshtml");
            }


            // Comprobamos si existe o no el json en App_Data, o si el usuario quiere recargar la página
            string fileName = url.Substring(url.IndexOf('/', url.IndexOf("//") + 2)).Trim('/');
            fileName = fileName.Replace('?','-');
            fileName = fileName.Replace('&', '-');
            fileName = fileName.Replace('=', '-');
            //string jsonURL = $"~/App_Data/{url.Replace("http://localhost:56680/", "")}.txt"; // cambiar o solo http://
            string jsonURL = $"~/App_Data/{fileName}.txt"; // cambiar o solo http://
            string responseFromServer;
            bool ficheroLeido = false;

            if(submit == "Recargar página" || !System.IO.File.Exists(Server.MapPath(jsonURL)))
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
                if (!string.IsNullOrEmpty(sessionID))
                {
                    myHttpWebRequest.CookieContainer.Add(new Cookie("ASP.NET_SessionId", sessionID, "/", myHttpWebRequest.RequestUri.Host));
                }
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
            else
            {
                responseFromServer = System.IO.File.ReadAllText(Server.MapPath(jsonURL));
                ficheroLeido = true;
            }

            // Dividimos el json obtenido, la primera string para el modelo y la segunda para el ViewBag 
            string[] divideJson = responseFromServer.Split(new string[] { "{ComienzoJsonViewData}" }, StringSplitOptions.None);
            string jsonModel = divideJson[0];
            string jsonViewData="";
            if (divideJson.Length > 1)
            {
                jsonViewData = divideJson[1];
            }
            

            // Deserializamos ViewData
            JsonSerializerSettings jsonSerializerSettingsVB = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            };
            ViewDataDictionary ViewDataDeserializado = JsonConvert.DeserializeObject<ViewDataDictionary>(jsonViewData, jsonSerializerSettingsVB);
            if (ViewDataDeserializado != null)
            {
                foreach (string item in ViewDataDeserializado.Keys)
                {
                    ViewData.Add(item, ViewDataDeserializado[item]);
                }
            }


            // Obtenemos tipo del modelo
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

            switch (modelType) 
            {
                case "CMSBlock":
                    List<CMSBlock> modelCB = (List<CMSBlock>)DeserializeObject(jsonModel);
                    return View("~/Views/CMSPagina/Index.cshtml", modelCB);

                case "SearchViewModel":
                    SearchViewModel modelSM = (SearchViewModel)DeserializeObject(jsonModel);
                    return View("~/Views/Busqueda/Index.cshtml", modelSM);

                case "AutenticationModel":
                    AutenticationModel modelAM = (AutenticationModel)DeserializeObject(jsonModel);
                    return View("~/Views/Registro/Index.cshtml", modelAM);
                case "ResourceViewModel":
                    
                    if (ficheroLeido == false)
                    {
                        //jsonModel = ChangeIdJson(jsonModel);

                        // volvemos a formar el fichero, pero con los ids cambiados para guardarlo
                        responseFromServer =  jsonModel + "{ComienzoJsonViewData}" + jsonViewData;
                        FileInfo file = new FileInfo(Server.MapPath(jsonURL)); 
                        file.Directory.Create();
                        System.IO.File.WriteAllText(file.FullName, responseFromServer);
                    }

                    // Asignación de propiedades estáticas (NamespaceOntologia y UrlOntología también pierden su valor)
                    //GestionOWL.URLIntragnoss = "http://didactalia.net/";  

                    //// Deserializamos ResourceViewModel
                    //JsonSerializerSettings jsonSerializerSettingsRM = new JsonSerializerSettings
                    //{
                    //    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                    //    ObjectCreationHandling = ObjectCreationHandling.Replace,
                    //    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    //    PreserveReferencesHandling = PreserveReferencesHandling.All,
                    //    TypeNameHandling = TypeNameHandling.All
                    //};
                    //ResourceViewModel modelRM = JsonConvert.DeserializeObject<ResourceViewModel>(jsonModel, jsonSerializerSettingsRM);

                    MemoryStream mStream = new MemoryStream(ASCIIEncoding.Default.GetBytes(jsonModel));
                    BinaryFormatter deserializer = new BinaryFormatter();
                    ResourceViewModel modelRM = (ResourceViewModel)deserializer.Deserialize(mStream);

                    // Comprobamos si el modelo contiene RdfType y mostramos la vista correspondiente
                    string rdfType = modelRM.Resource.RdfType;
                    if (!String.IsNullOrEmpty(rdfType))
                    {
                        return View($"~/Views/FichaRecurso/{rdfType}.cshtml", modelRM);
                    }
                    return View("~/Views/FichaRecurso/Index.cshtml", modelRM);
                case "ProfilePageViewModel":
                    ProfilePageViewModel modelProfile = (ProfilePageViewModel)DeserializeObject(jsonModel);
                    return View("~/Views/FichaPerfil/Index.cshtml", modelProfile);
                case "OlvidePasswordViewModel":
                    OlvidePasswordViewModel modelOlvidePass = (OlvidePasswordViewModel)DeserializeObject(jsonModel);
                    return View("~/Views/OlvidePassword/Index.cshtml", modelOlvidePass);
                case "bool":
                    bool listadoMensajeModel = (bool)DeserializeObject(jsonModel);
                    return View("~/Views/BandejaMensajes/Listado.cshtml", listadoMensajeModel);
                case "MessageModel":
                    MessageModel mensajeModel = (MessageModel)DeserializeObject(jsonModel);
                    if (url.EndsWith("?nuevo"))
                    {
                        return View("~/Views/BandejaMensajes/EdicionMensaje.cshtml", mensajeModel);
                    }
                    else
                    {
                        return View("~/Views/BandejaMensajes/Mensaje.cshtml", mensajeModel);
                    }
                default:
                    return View("~/Views/GnossTestView/AskURL.cshtml");
            }
        }

        /// <summary>
        /// Se crea un objeto ViewResult utilizando el nombre de la vista, el nombre de la página maestra y el modelo que presenta una vista.
        /// Guarda en ViewBag.ViewPath la ruta relativa de la vista.
        /// </summary>
        protected override ViewResult View(string viewName, string masterName, object model)
        {
            ViewBag.ViewPath = viewName.Substring(0, viewName.LastIndexOf('/'));
            return base.View(viewName, masterName, model);
        }

        /// <summary>
        /// Obtenemos un objeto deserializado con las opciones de deserialización PreserveReferencesHandling.Objects y TypeNameHandling.All
        /// </summary>
        private object DeserializeObject(string json)
        {
            JsonSerializerSettings jsonSerializerSettingsSM = new JsonSerializerSettings 
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = TypeNameHandling.All
            };
            return JsonConvert.DeserializeObject(json, jsonSerializerSettingsSM); 
        }

        /// <summary>
        /// Cambia los ids repetidos del json pasado como parámetro
        /// </summary>
        private string ChangeIdJson(string json)
        {
            string pattern = @"(""\$id"":)("")(\d+)("")";
            Regex rg = new Regex(pattern);
            MatchCollection matchCollectionID = rg.Matches(json);
            string stringID = matchCollectionID[matchCollectionID.Count - 1].Value;
            string stringFinalID = Regex.Match(stringID, @"\d+").Value;
            int finalID = Int32.Parse(stringFinalID);

            int newNumber = finalID + 1;
            int desplazar = 0;
            int nthOccurance;
            int desplazamiento = 0;
            int cont = 0;
            int ids = 0;

            //Stopwatch timer = Stopwatch.StartNew();

            while (ids <= finalID)
            {
                MatchCollection matchCollection = Regex.Matches(json, Regex.Escape("\"$id\":\"" + ids + "\""));
                cont = matchCollection.Count;

                if (cont >= 2)
                {
                    desplazamiento = 0;
                    desplazar = ((int)Math.Log10(newNumber) + 1) - ((int)Math.Log10(ids) + 1);

                    for (nthOccurance = 2; cont >= nthOccurance; nthOccurance++)
                    {
                        Match match = matchCollection[nthOccurance - 1];
                        json = json.Remove(match.Index + desplazamiento, match.Length).Insert(match.Index + desplazamiento, "\"$id\":\"" + newNumber + "\"");
                        newNumber++;
                        desplazamiento += desplazar;
                    }
                }
                ids++;
            }

            //timer.Stop();
            //TimeSpan timespan = timer.Elapsed;
            //System.Diagnostics.Debug.WriteLine(String.Format("{0:00}:{1:00}:{2:00}", timespan.Minutes, timespan.Seconds, timespan.Milliseconds / 10));

            return json;
        }
    }
}
