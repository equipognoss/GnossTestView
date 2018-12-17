using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GnossTestView.App_Start
{
    public class CustomRazorViewEngine : RazorViewEngine
    {
        public CustomRazorViewEngine()
        {

            base.ViewLocationFormats = new string[] {
                "~/Views/{1}/{0}.cshtml",
            };

            DirectoryInfo directorioVistas = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Views"));
            DirectoryInfo[] directoriosVistas = directorioVistas.GetDirectories();

            List<string> partialViewLocationFormats = new List<string>() {
                "~/Views/{1}/{0}.cshtml",
                "~/Views/Shared/{0}.cshtml",
                "~/Views/CMSPagina/{0}.cshtml"
            };

            foreach (DirectoryInfo directorio in directoriosVistas)
            {
                partialViewLocationFormats.Add($"~/Views/{directorio.Name}/{{0}}.cshtml");
            }

            base.PartialViewLocationFormats = partialViewLocationFormats.ToArray();

            base.MasterLocationFormats = new string[] {
                "~/Views/Shared/{0}.cshtml"
            };

            base.FileExtensions = new string[] {
                "cshtml"
            };

        }
    }
}