using System;
using System.IO;
using System.IO.Compression;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System.Net;
using System.Text;
using System.Linq;

namespace GnossTestView.Areas.GnossTestView.Utilidades
{
    public class UtilManageViews
    {
        [ThreadStatic]
        private static CredentialsHandler mCredential;

        internal static string[] ObtenerProyectosVistas()
        {
            return Directory.GetDirectories(AppContext.BaseDirectory + "Views");
        }

        internal static string ObrenerRamaRepositorioLocal(string proyecto)
        {
            string rutaDirectorio = $"{AppContext.BaseDirectory}Views/{proyecto}\\.git\\refs\\heads";

            if (Directory.Exists(rutaDirectorio))
            {
                string[] filesRamas = Directory.GetFiles($"{rutaDirectorio}");

                return filesRamas.First().Split('\\').Last();
            }
            return "";
        }
        
        private static string GetProyectRepository(string proyecto)
        {
            string repositorio = ""; //Pedirselo al api de integracioncontinua
           

            return repositorio;
        }
        private static string GetActualBranchRepository(string proyecto)
        {
            string ramaActual = ""; //Pedirselo al api de integracioncontinua

            
            return ramaActual;
        }

        internal static void DescargarVistasInicial(string proyecto)
        {
            if (!HasCredentials())
            {
                string rootUser = UtilConfiguration.GetConfiguration("usuarioRootRepositorio/user");
                string rootPassword = UtilConfiguration.GetConfiguration("usuarioRootRepositorio/password");

                CreateCredentials(rootUser, rootPassword);
            }

            string repositorio = GetProyectRepository(proyecto);
            string ramaActual = GetActualBranchRepository(proyecto);

            string rutaDirectorio = $"{AppContext.BaseDirectory}Views/{proyecto}";

            ForkRespositorio(new Uri(repositorio), rutaDirectorio, ramaActual);
        }

        internal static void DescargarVistas(string proyecto)
        {
            string ramaActual = GetActualBranchRepository(proyecto);

            string rutaDirectorio = $"{AppContext.BaseDirectory}Views/{proyecto}";
           
            using (var repo = new Repository(rutaDirectorio))
            {
                Branch rama = FindBranch(ramaActual, repo);

                CheckoutOptions option = new CheckoutOptions();
                option.CheckoutModifiers = CheckoutModifiers.Force;

                Commands.Checkout(repo, rama, option);
                repo.Index.Write();
            }
        }

        internal static void SubirVistas(string proyectName, string serverURL, string userFTP, string passwordFTP)
        {
            UploadFile($"{serverURL}/{proyectName}/Vistas", $"{AppContext.BaseDirectory}Views/{proyectName}/Proyectos/{proyectName}/Vistas", userFTP, passwordFTP);
        }

        internal static void SubirEstilos(string proyectName, string serverURL, string userFTP, string passwordFTP)
        {
            UploadFile($"{serverURL}/{proyectName}/Estilos", $"{AppContext.BaseDirectory}Views/{proyectName}/Proyectos/{proyectName}/Estilos", userFTP, passwordFTP);
        }

        private static void UploadFile(string FilePath, string RemotePath, string Login, string Password)
        {
            try
            {
                string[] Files = Directory.GetFiles(FilePath, "*.*");
                string[] Paths = Directory.GetDirectories(FilePath);
                foreach (string file in Files)
                {
                    using (FileStream fs = File.Open(FilePath, FileMode.OpenOrCreate, FileAccess.Read))
                    {
                        FtpWebRequest ftp = (FtpWebRequest)WebRequest.Create(RemotePath + Path.GetFileName(file));
                        ftp.Credentials = new NetworkCredential(Login, Password);
                        ftp.KeepAlive = false;
                        ftp.Method = WebRequestMethods.Ftp.UploadFile;
                        ftp.UseBinary = true;
                        ftp.ContentLength = fs.Length;
                        ftp.Proxy = null;
                        byte[] buff = new byte[fs.Length];
                        fs.Read(buff, 0, buff.Length);
                        fs.Close();
                        Stream ftpstream = ftp.GetRequestStream();
                        ftpstream.Write(buff, 0, buff.Length);
                        ftpstream.Close();
                    }
                }
                foreach (string path in Paths)
                {
                    UploadFile(path, RemotePath + "/" + Path.GetFileName(path), Login, Password);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void CreateCredentials(string pUser, string pPass)
        {
            mCredential = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = pUser, Password = pPass };
        }

        private static bool HasCredentials()
        {
            return mCredential != null;
        }

        private static void ForkRespositorio(Uri pURL, string pPathDirectorio, string pNombreRama)
        {
            CloneOptions option = new CloneOptions();
            if (!string.IsNullOrEmpty(pNombreRama))
            {
                option.BranchName = pNombreRama;
            }
            option.CredentialsProvider = mCredential;

            string mPath = Repository.Clone(pURL.ToString(), pPathDirectorio, option);
        }

        private static Branch FindBranch(string pBranchName, Repository pRepo)
        {
            return pRepo.Branches.FirstOrDefault(x => x.FriendlyName == pBranchName);
        }

        internal static void DescargarVistas2(string proyecto)
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