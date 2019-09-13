using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GnossTestView.Extensions
{
    public class UtilUsuario
    {
        private static string mUrlServicioLogin = null;

        /// <summary>
        /// Obtiene la URL del servicio de login
        /// </summary>
        public static string UrlServicioLogin
        {
            get
            {
                if (mUrlServicioLogin == null)
                {
                    return "http://localhost";
                }

                return mUrlServicioLogin;
            }
        }

        /// <summary>
        /// Obtiene un token para que el usuario se pueda loguear
        /// </summary>
        public static string TokenLoginUsuario
        {
            get
            {
                if (HttpContext.Current.Session["tokenCookie"] == null)
                {
                    string ticks = DateTime.Now.Ticks.ToString();
                    HttpContext.Current.Session["tokenCookie"] = ticks;
                }
                return (string)HttpContext.Current.Session["tokenCookie"];
            }
        }
    }
}