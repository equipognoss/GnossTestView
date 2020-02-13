using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace GnossTestView.Areas.GnossTestView.Utilidades
{
    public class UtilPageCache
    {

        internal static string GetFileName(string pUrl)
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

            return HttpContext.Current.Server.MapPath($"~/App_Data/{fileName}.txt");
        }


        internal static void SaveCacheFile(string cacheFileName, string value)
        {
            // Guardamos el json obtenido en App_Data
            FileInfo file = new FileInfo(cacheFileName);
            file.Directory.Create();
            System.IO.File.WriteAllText(file.FullName, value);
        }

    }
}