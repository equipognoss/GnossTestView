using GnossTestView.Areas.GnossTestView.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace GnossTestView.Areas.GnossTestView.Utilidades
{
    public class UtilConfiguration
    {
        private static readonly string rutaAutorization = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "Config\\Authorization.config";
        private static readonly string rutaConfiguration = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "Config\\Configuration.config";

        internal static void SetConfiguracion(string key, string value)
        {
            XmlDocument docXml = new XmlDocument();

            if (System.IO.File.Exists(rutaConfiguration))
            {
                docXml.Load(rutaConfiguration);
            }
            else
            {
                docXml.LoadXml("<config></config>");
            }

            XmlNode parentNode = docXml.SelectSingleNode("config");

            XmlNode node = parentNode.SelectSingleNode(key);
            if (node != null)
            {
                parentNode.RemoveChild(node);
            }

            XmlElement newConfig = docXml.CreateElement(key);
            newConfig.InnerText = value;

            parentNode.AppendChild(newConfig);

            docXml.Save(rutaConfiguration);
        }

        internal static string GetConfiguration(string key)
        {
            if (System.IO.File.Exists(rutaConfiguration))
            {
                XmlDocument docXml = new XmlDocument();
                docXml.Load(rutaConfiguration);

                XmlNode nodeProy = docXml.SelectSingleNode($"config/{key}");
                if (nodeProy != null)
                {
                    return nodeProy.InnerText;
                }
            }
            return "";
        }


        internal static void GetAuthorization(string url, ref string user, ref string password)
        {
            KeyValuePair<string, string>? datosAuth = ObtenerAuthorizationConfig(url);
            if (!datosAuth.HasValue)
            {
                datosAuth = ObtenerAuthorizationConfig("default");
            }

            if (datosAuth.HasValue)
            {
                user = datosAuth.Value.Key;
                password = datosAuth.Value.Value;
            }
        }

        internal static void SetAuthorization(string url, string user, string password)
        {
            XmlDocument docXml = new XmlDocument();

            if (System.IO.File.Exists(rutaAutorization))
            {
                docXml.Load(rutaAutorization);
            }
            else
            {
                docXml.LoadXml("<config>  <proyecto url=\"default\"><user></user><pass></pass></proyecto></config>");
            }

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

            docXml.Save(rutaAutorization);
        }

        internal static KeyValuePair<string, string>? ObtenerAuthorizationConfig(string url)
        {
            if (System.IO.File.Exists(rutaAutorization))
            {
                XmlDocument docXml = new XmlDocument();
                docXml.Load(rutaAutorization);

                XmlNode nodeProy = docXml.SelectSingleNode($"config/proyecto[@url=\"{url}\"]");
                if (nodeProy != null)
                {
                    XmlNode userNode = nodeProy.SelectSingleNode("user");
                    XmlNode passwordNode = nodeProy.SelectSingleNode("pass");

                    if (userNode != null && passwordNode != null)
                    {
                        return new KeyValuePair<string, string>(userNode.InnerText, passwordNode.InnerText);
                    }
                }
            }
            return null;
        }

        internal static List<ConfigurationModel.AutorizationModel> GetAuthorizationConfig()
        {
            List<ConfigurationModel.AutorizationModel> autorizationList = new List<ConfigurationModel.AutorizationModel>();

            if (System.IO.File.Exists(rutaAutorization))
            {
                XmlDocument docXml = new XmlDocument();
                docXml.Load(rutaAutorization);

                XmlNodeList nodesProy = docXml.SelectNodes($"config/proyecto");
                foreach (XmlNode nodeProy in nodesProy)
                {
                    XmlNode userNode = nodeProy.SelectSingleNode("user");
                    XmlNode passwordNode = nodeProy.SelectSingleNode("pass");

                    if (userNode != null && passwordNode != null)
                    {
                        ConfigurationModel.AutorizationModel autorization = new ConfigurationModel.AutorizationModel();
                        autorization.url = nodeProy.Attributes["url"].Value;
                        autorization.user = userNode.InnerText;
                        autorization.password = passwordNode.InnerText;

                        autorizationList.Add(autorization);
                    }
                }
            }

            return autorizationList;
        }

        internal static void SetAuthorizationConfig(List<ConfigurationModel.AutorizationModel> autorizationList)
        {
            XmlDocument docXml = new XmlDocument();

            if (System.IO.File.Exists(rutaAutorization))
            {
                docXml.Load(rutaAutorization);
            }
            else if (!autorizationList.Any(a => a.url.Equals("default")))
            {
                docXml.LoadXml("<config>  <proyecto url=\"default\"><user></user><pass></pass></proyecto></config>");
            }
            else 
            {
                docXml.LoadXml("<config></config>");
            }

            XmlNode parentNode = docXml.SelectSingleNode("config");
            

            foreach (ConfigurationModel.AutorizationModel autorization in autorizationList)
            {
                XmlElement newConfig = docXml.CreateElement("proyecto");
                newConfig.SetAttribute("url", autorization.url);

                XmlElement nodeUser = docXml.CreateElement("user");
                nodeUser.InnerText = autorization.user;
                newConfig.AppendChild(nodeUser);

                XmlElement nodePass = docXml.CreateElement("pass");
                nodePass.InnerText = autorization.password;
                newConfig.AppendChild(nodePass);

                parentNode.AppendChild(newConfig);
            }

            docXml.Save(rutaAutorization);
        }

    }
}