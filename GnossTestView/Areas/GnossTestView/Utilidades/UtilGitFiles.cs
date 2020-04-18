﻿using System;
using System.IO;
using System.IO.Compression;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System.Net;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace GnossTestView.Areas.GnossTestView.Utilidades
{
    public class UtilGitFiles
    {
        [ThreadStatic]
        private static CredentialsHandler mCredential;

        private static CredentialsHandler Credential
        {
            get
            {
                if (mCredential == null)
                {
                    string rootUser = UtilConfiguration.GetConfiguration("usuarioRootRepositorio/user");
                    string rootPassword = UtilConfiguration.GetConfiguration("usuarioRootRepositorio/password");

                    mCredential = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = rootUser, Password = rootPassword };
                }
                return mCredential;
            }
        }

        internal static string[] ObtenerProyectosVistas()
        {
            return Directory.GetDirectories(AppContext.BaseDirectory + "Views");
        }

        internal static string ObrenerRamaRepositorioLocal(string proyecto)
        {
            string rutaDirectorio = $"{AppContext.BaseDirectory}Views/{proyecto}";

            using (var repo = new Repository(rutaDirectorio))
            {
                return repo.Head.FriendlyName;
            }
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
            string repositorio = GetProyectRepository(proyecto);
            string ramaActual = GetActualBranchRepository(proyecto);

            string rutaDirectorio = $"{AppContext.BaseDirectory}Views/{proyecto}";

            Directory.CreateDirectory(rutaDirectorio);

            ForkRespositorio(new Uri(repositorio), rutaDirectorio, ramaActual);
        }

        internal static void DescargarVistas(string proyecto)
        {
            string rutaDirectorio = $"{AppContext.BaseDirectory}Views/{proyecto}";

            using (var repo = new Repository(rutaDirectorio))
            {
                try
                {
                    // Comprobar que la rama actual es la última
                     string ramaActual = GetActualBranchRepository(proyecto);
                     Branch rama = FindBranch(ramaActual, repo);

                    var fetchOptions = new FetchOptions()
                    {
                        CredentialsProvider = Credential
                    };

                    PullOptions options = new PullOptions();
                    options.FetchOptions = fetchOptions;
                    var signature = new LibGit2Sharp.Signature(
                    new Identity("MERGE_USER_NAME", "MERGE_USER_EMAIL"), DateTimeOffset.Now);

                    MergeResult merge = Commands.Pull(repo, signature, options);
                }
                catch(Exception ex)
                {
                    //repo.Info.WorkingDirectory
                }
            }
        }

        internal static void ActualizarUltimaRama(string proyecto)
        {
            string ramaActual = GetActualBranchRepository(proyecto);

            string rutaDirectorio = $"{AppContext.BaseDirectory}Views/{proyecto}";

            using (var repo = new Repository(rutaDirectorio))
            {
                Branch rama = FindBranch(ramaActual, repo);

                if (rama == null)
                {
                    // Let's get a reference on the remote tracking branch...
                    string trackedBranchName = "origin/" + ramaActual;
                    Branch trackedBranch = repo.Branches[trackedBranchName];

                    // ...and create a local branch pointing at the same Commit
                    Branch branch = repo.CreateBranch(ramaActual, trackedBranch.Tip);


                    // So, let's configure the local branch to track the remote one.
                    rama = repo.Branches.Update(branch,
                        b => b.TrackedBranch = trackedBranch.CanonicalName);
                }

                CheckoutOptions option = new CheckoutOptions();
                option.CheckoutModifiers = CheckoutModifiers.None;

                Commands.Checkout(repo, rama, option);
                repo.Index.Write();
            }
        }
        
        private static void ForkRespositorio(Uri pURL, string pPathDirectorio, string pNombreRama)
        {
            CloneOptions option = new CloneOptions();
            if (!string.IsNullOrEmpty(pNombreRama))
            {
                option.BranchName = pNombreRama;
            }
            option.CredentialsProvider = Credential;

            Repository.Clone(pURL.ToString(), pPathDirectorio, option);
        }

        private static Branch FindBranch(string pBranchName, Repository pRepo)
        {
            return pRepo.Branches.FirstOrDefault(x => x.FriendlyName == pBranchName);
        }

        internal static Dictionary<string, FileStatus> GetFilesWithChanges(string proyecto)
        {
            Dictionary<string, FileStatus> cambios = new Dictionary<string, FileStatus>();

            string rutaDirectorio = $"{AppContext.BaseDirectory}Views/{proyecto}";

            using (var repo = new Repository(rutaDirectorio))
            {
                RepositoryStatus status = repo.RetrieveStatus();

                foreach (var item in status)
                {
                    cambios.Add(item.FilePath, item.State);
                }
            }
            return cambios;
        }

        internal static bool CheckServerChanges(string proyecto)
        {
            string ramaActual = GetActualBranchRepository(proyecto);

            string rutaDirectorio = $"{AppContext.BaseDirectory}Views/{proyecto}";

            using (var repo = new Repository(rutaDirectorio))
            {
                var fetchOptions = new FetchOptions()
                {
                    CredentialsProvider = Credential
                };

                string logMessage = "";
                var remote = repo.Network.Remotes["origin"];
                IEnumerable<string> refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                Commands.Fetch(repo, "origin", refSpecs, fetchOptions, logMessage);

                Branch rama = FindBranch(ramaActual, repo);
                return rama == null || rama.TrackingDetails.BehindBy > 0;
            }
        }

        internal static bool CheckBranchChange(string proyecto)
        {
            string ramaRepositorio = GetActualBranchRepository(proyecto);

            string ramaLocal = ObrenerRamaRepositorioLocal(proyecto);

            return !ramaRepositorio.Equals(ramaLocal);
        }
        
    }
}