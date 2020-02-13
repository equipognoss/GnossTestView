using System;
using System.IO;
using System.IO.Compression;

using System.Net;
using System.Text;

namespace GnossTestView.Areas.GnossTestView.Utilidades
{
    public class UtilManageViews
    {
        internal static string[] ObtenerProyectosVistas()
        {
            return Directory.GetDirectories(AppContext.BaseDirectory + "Views");
        }

        internal static void DescargarVistas(string proyecto)
        {
            string urlApiDespliegues = UtilConfiguration.GetConfiguration("urlApiDespliegues").TrimEnd('/');

            if (!string.IsNullOrWhiteSpace(urlApiDespliegues))
            {
                bool ecosistema = proyecto.Equals("ecosistema");
                string nombreProy = proyecto;

                if (ecosistema)
                {
                    nombreProy = "myGnoss";
                }

                // Construct HTTP request to get the logo
                HttpWebRequest httpRequest = (HttpWebRequest)
                    WebRequest.Create($"{urlApiDespliegues}/api/Vistas?ecosistema={ecosistema.ToString()}&nombreProy={nombreProy}");
                httpRequest.Method = WebRequestMethods.Http.Get;

                // Get back the HTTP response for web server
                HttpWebResponse httpResponse
                    = (HttpWebResponse)httpRequest.GetResponse();
                Stream httpResponseStream = httpResponse.GetResponseStream();

                // Define buffer and buffer size
                int bufferSize = 1024;
                byte[] buffer = new byte[bufferSize];
                int bytesRead = 0;

                // Read from response and write to file
                FileStream fileStream = System.IO.File.Create($"vistas_{proyecto}.zip");
                while ((bytesRead = httpResponseStream.Read(buffer, 0, bufferSize)) != 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);
                } // end while

                fileStream.Close();

                string rutaDirectorio = $"{AppContext.BaseDirectory}Views/{proyecto}";

                if (Directory.Exists(rutaDirectorio))
                {
                    string rutaDirectorioBK = $"{AppContext.BaseDirectory}ViewsBK/{proyecto}";

                    Directory.CreateDirectory(rutaDirectorioBK);
                    ZipFile.CreateFromDirectory(rutaDirectorio, $"{rutaDirectorioBK}/{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.rar");
                    Directory.Delete(rutaDirectorio, true);
                }
                Directory.CreateDirectory($"{AppContext.BaseDirectory}Views/{proyecto}");

                ZipFile.ExtractToDirectory(fileStream.Name, rutaDirectorio, Encoding.ASCII);

                File.Delete(fileStream.Name);
            }
        }
    }
}