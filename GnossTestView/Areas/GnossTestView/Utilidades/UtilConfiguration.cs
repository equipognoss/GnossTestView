using GnossTestView.Areas.GnossTestView.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.XPath;

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
            if (node == null)
            { 
                node = createXPath(docXml, key);
            }
            node.InnerText = value;

            docXml.Save(rutaConfiguration);
        }

        private static XmlNode createXPath(XmlDocument doc, string xpath)
        {
            XmlNode node = doc;
            foreach (string part in xpath.Substring(1).Split('/'))
            {
                XmlNode nodeAux = node.SelectSingleNode(part);

                if (nodeAux != null) { node = nodeAux; continue; }

                if (part.StartsWith("@"))
                {
                    var anode = doc.CreateAttribute(part.Substring(1));
                    node.Attributes.Append(anode);
                    node = anode;
                }
                else
                {
                    string elName, attrib = null;
                    if (part.Contains("["))
                    {
                        SplitOnce(part, "[", out elName, out attrib);
                        if (!attrib.EndsWith("]")) throw new Exception("Unsupported XPath (missing ]): " + part);
                        attrib = attrib.Substring(0, attrib.Length - 1);
                    }
                    else elName = part;

                    XmlNode next = doc.CreateElement(elName);
                    node.AppendChild(next);
                    node = next;

                    if (attrib != null)
                    {
                        if (!attrib.StartsWith("@")) throw new Exception("Unsupported XPath attrib (missing @): " + part);
                        string name, value;
                        SplitOnce(attrib.Substring(1), "='", out name, out value);
                        if (string.IsNullOrEmpty(value) || !value.EndsWith("'")) throw new Exception("Unsupported XPath attrib: " + part);
                        value = value.Substring(0, value.Length - 1);
                        var anode = doc.CreateAttribute(name);
                        anode.Value = value;
                        node.Attributes.Append(anode);
                    }
                }
            }
            return node;
        }

        private static void SplitOnce(string value, string separator, out string part1, out string part2)
        {
            if (value != null)
            {
                int idx = value.IndexOf(separator);
                if (idx >= 0)
                {
                    part1 = value.Substring(0, idx);
                    part2 = value.Substring(idx + separator.Length);
                }
                else
                {
                    part1 = value;
                    part2 = null;
                }
            }
            else
            {
                part1 = "";
                part2 = null;
            }
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
            if(!autorizationList.Any())
            {
                ConfigurationModel.AutorizationModel autorization = new ConfigurationModel.AutorizationModel();
                autorization.url = "default";
                autorization.user = "";
                autorization.password = "";

                autorizationList.Add(autorization);
            }

            return autorizationList;
        }

        internal static void SetAuthorizationConfig(List<ConfigurationModel.AutorizationModel> autorizationList)
        {
            XmlDocument docXml = new XmlDocument();

            if (!autorizationList.Any(a => a.url.Equals("default")))
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