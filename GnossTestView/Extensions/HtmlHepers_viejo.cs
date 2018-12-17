using Es.Riam.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Es.Riam.Gnoss.Web.MVC.Models;
using System.Web.Mvc;

namespace GnossTestView.Extensions
{
    public static class HtmlHelpers
    {
        public static Dictionary<string, string> GetParametrosAplicacion(this HtmlHelper helper)
        {
            return helper.ViewBag.ParametrosAplicacion;
        }

        public static Dictionary<Guid, ProfileModel> GetIdentitiesByUserID(this HtmlHelper helper, List<Guid> pListUsers, bool pExtraData = false)
        {
            throw new NotImplementedException();
        }

        public static Dictionary<Guid, ProfileModel> GetIdentitiesByID(this HtmlHelper helper, List<Guid> pListIdentities,bool pExtraData=false)
        {
            throw new NotImplementedException();
        }

        public static Dictionary<Guid, ResourceModel> GetResourcesByID(this HtmlHelper helper, List<Guid> pListResources,bool pGetExtraData,bool pGetIdentities, bool pExtraDataIdentities = false)
        {
            throw new NotImplementedException();
        }

        public static string GetText(this HtmlHelper helper, string pPage, string pText)
        {
            return helper.ViewBag.UtilIdiomas.GetText(pPage, pText);
        }

        public static string GetText(this HtmlHelper helper, string pPage, string pText, params string[] pParametros)
        {
            return helper.ViewBag.UtilIdiomas.GetText(pPage, pText, pParametros);
        }

        /// <summary>
        /// Obtiene una fecha a través de un string con 14 caracteres de la forma '20150501123000' o un string de la forma 20/08/1984
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="pDate"></param>
        /// <returns></returns>
        public static DateTime? GetDate(this HtmlHelper helper, string pDate)
        {
            DateTime? fecha = null;
            try
            {
                int anio = 0; ;
                int mes = 0; ;
                int dia = 0;
                int hora = 0;
                int minuto = 0;
                int segundo = 0;
                if (pDate.Length == 14)
                {
                    anio = int.Parse(pDate.Substring(0, 4));
                    mes = int.Parse(pDate.Substring(4, 2));
                    dia = int.Parse(pDate.Substring(6, 2));
                    hora = int.Parse(pDate.Substring(8, 2));
                    minuto = int.Parse(pDate.Substring(10, 2));
                    segundo = int.Parse(pDate.Substring(12, 2));
                }
                else if (pDate.Length >= 10 && pDate.Contains("/"))
                {
                    anio = int.Parse(pDate.Substring(6, 4));
                    mes = int.Parse(pDate.Substring(3, 2));
                    dia = int.Parse(pDate.Substring(0, 2));
                }

                fecha = new DateTime(anio, mes, dia, hora, minuto, segundo);
            }
            catch
            {
            }

            return fecha;
        }

        public static string EliminarCaracteresUrlSem(this HtmlHelper helper, string pText)
        {
            return UtilCadenas.EliminarCaracteresUrlSem(pText);
        }

        public static string ObtenerTextoDeIdioma(this HtmlHelper helper, string pText)
        {
            return ObtenerTextoDeIdioma(helper, pText, helper.ViewBag.UtilIdiomas.LanguageCode, "es");
        }

        public static string ObtenerTextoDeIdioma(this HtmlHelper helper, string pText, string pIdioma, string pIdiomaDefecto, bool pSoloIdiomaIndicado = false)
        {
            return UtilCadenas.ObtenerTextoDeIdioma(pText, pIdioma, pIdiomaDefecto, pSoloIdiomaIndicado);
        }

        public static string ObtenerTextoIdiomaUsuario(this HtmlHelper helper, string pTexto, bool pTraerPrimeroSiNoEncuentra = false)
        {
            throw new NotImplementedException();
        }

        private static bool TienePersonalizacion(this HtmlHelper htmlHelper)
        {
            CommunityModel Comunidad = htmlHelper.ViewBag.Comunidad;
            bool tienePersonalizacion = false;

            if ((!string.IsNullOrEmpty((string)htmlHelper.ViewBag.Personalizacion) || !string.IsNullOrEmpty((string)htmlHelper.ViewBag.PersonalizacionEcosistema)) && Comunidad != null)
            {
                tienePersonalizacion = true;
            }

            return tienePersonalizacion;
        }

        public static MvcHtmlString PartialView(this HtmlHelper htmlHelper, string partialViewName)
        {
            throw new NotImplementedException();
        }

        public static MvcHtmlString PartialView(this HtmlHelper htmlHelper, string partialViewName, Object model)
        {
            throw new NotImplementedException();

            //MvcHtmlString resultado = null;
            //UtilTrazas.AgregarEntrada("HtmlHelpers.PartialView 1");
            //CommunityModel Comunidad = htmlHelper.ViewBag.Comunidad;

            //if (TienePersonalizacion(htmlHelper))
            //{
            //    UtilTrazas.AgregarEntrada("TienePersonalizacion");

            //    string personalizacion = MultiViewResult.ComprobarPersonalizacion(htmlHelper.ViewBag, Comunidad, partialViewName);

            //    resultado = System.Web.Mvc.Html.PartialExtensions.Partial(htmlHelper, partialViewName + personalizacion);
            //}

            //if (resultado == null)
            //{
            //    resultado = System.Web.Mvc.Html.PartialExtensions.Partial(htmlHelper, partialViewName);
            //}

            //UtilTrazas.AgregarEntrada("Fin HtmlHelpers.PartialView 1");
            //return resultado;
        }

        public static string ObtenerImagenConTamano(this HtmlHelper helper, string pUrlImg, int pTamaño)
        {
            if (!string.IsNullOrEmpty(pUrlImg))
            {
                return pUrlImg.Substring(0, pUrlImg.LastIndexOf('.')) + "_" + pTamaño + pUrlImg.Substring(pUrlImg.LastIndexOf('.'));
            }
            else
            {
                return "";
            }
        }

        public static string AcortarTexto(this HtmlHelper helper, string pTexto, int pNumCaracteres)
        {
            return UtilCadenas.AcortarTexto(pTexto, pNumCaracteres);
        }

        private static string EliminarTablas(string pDescripcion)
        {
            return pDescripcion;
        }

        public static string AcortarDescripcionHtml(this HtmlHelper helper, string pTexto, int pNumCaracteres)
        {
            return UtilCadenas.AcortarDescripcionHtml(pTexto, pNumCaracteres);
        }

        public static string AcortarDescripcionHtmlPorNumeroParrafos(this HtmlHelper helper, string pTexto, int pNumParrafos)
        {
            return UtilCadenas.AcortarDescripcionHtmlPorNumeroParrafos(pTexto, pNumParrafos);
        }

        public static string ConvertirPrimeraLetraDeFraseAMayusculas(this HtmlHelper helper, string pTexto)
        {
            return Es.Riam.Util.UtilCadenas.ConvertirPrimeraLetraDeFraseAMayúsculas(pTexto);
        }

        public static string ConvertirPrimeraLetraPalabraAMayusculas(this HtmlHelper helper, string pTexto)
        {
            return Es.Riam.Util.UtilCadenas.ConvertirPrimeraLetraPalabraAMayusculas(pTexto);
        }

        public static string ObtenerUrlDeDoc(this HtmlHelper helper, Guid idDocumento, string pNombreDocumento)
        {
            throw new NotImplementedException();
        }

        public static string ObtenerNombreCompletoDeFichaIdentidad(this HtmlHelper helper, ProfileModel pIdentidad)
        {
            string nombreCompleto = "";
            if (pIdentidad.TypeProfile == ProfileType.Personal || pIdentidad.TypeProfile == ProfileType.Teacher)
            {
                nombreCompleto = pIdentidad.NamePerson;
            }
            else if (pIdentidad.TypeProfile == ProfileType.ProfessionalPersonal)
            {
                nombreCompleto = pIdentidad.NamePerson + " · " + pIdentidad.NameOrganization;
            }
            else if (pIdentidad.TypeProfile == ProfileType.ProfessionalCorporate && !string.IsNullOrEmpty(pIdentidad.NamePerson))
            {
                nombreCompleto = pIdentidad.NameOrganization + " (" + pIdentidad.NamePerson + ")";
            }
            else
            {
                nombreCompleto = pIdentidad.NameOrganization;
            }
            return nombreCompleto;
        }

        public static string ObtenerNombrePerfil(this HtmlHelper helper, ProfileModel pPerfil)
        {
            string nombrePerfil = "";

            if (!string.IsNullOrEmpty(pPerfil.NamePerson))
            {
                nombrePerfil += pPerfil.NamePerson;
            }

            if (!string.IsNullOrEmpty(pPerfil.NameOrganization))
            {
                if (!string.IsNullOrEmpty(pPerfil.NamePerson))
                {
                    nombrePerfil += " · ";
                }
                nombrePerfil += pPerfil.NameOrganization;
            }

            return nombrePerfil;
        }

        public static string ObtenerUrlPerfil(this HtmlHelper helper, ProfileModel pPerfil)
        {
            string urlPerfil = "";

            if (!string.IsNullOrEmpty(pPerfil.UrlPerson))
            {
                urlPerfil += pPerfil.UrlPerson;
            }
            else if (!string.IsNullOrEmpty(pPerfil.UrlOrganization))
            {
                urlPerfil += pPerfil.UrlOrganization;
            }

            return urlPerfil;
        }

        /// <summary>
        /// Genera un parámetro con valor correctamente. Ejemplo: class="active".
        /// </summary>
        /// <param name="pParametro">Parámetro</param>
        /// <param name="pValor">Valor del parámetro</param>
        /// <returns>Parámetro con valor correctamente. Ejemplo: class="active"</returns>
        public static IHtmlString GetParam(this HtmlHelper helper, string pParametro, string pValor)
        {
            if (string.IsNullOrEmpty(pValor))
            {
                return helper.Raw("");
            }
            else
            {
                return helper.Raw(pParametro + "=\"" + pValor + "\"");
            }
        }

        /// <summary>
        /// Obtiene el aviso de aceptar cookies.
        /// </summary>
        public static string GetCookiesWarning(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.CookiesWarning;
        }

        /// <summary>
        /// Traduce el texto asociado al Texto ID al idioma de la comunidad.
        /// </summary>
        /// <param name="pTextoID">Identificador del texto</param>
        /// <param name="pIdioma">El Idioma del texto</param>
        /// <returns>El texto traducido</returns>
        public static string Translate(this HtmlHelper helper, string pTextoID, params string[] pParams)
        {
            string resultado = helper.ViewBag.UtilIdiomas.GetTextoPersonalizado(pTextoID, pParams);

            if (string.IsNullOrEmpty(resultado))
            {
                resultado = pTextoID;
            }

            return resultado;
        }

        /// <summary>
        /// Elimina el primer parrafo de un texto si lo tiene.
        /// </summary>
        /// <param name="helper">Helper</param>
        /// <param name="pText">Texto</param>
        /// <returns>Texto sin el primer parrafo de un texto si lo tiene</returns>
        public static string DeleteFirstParagraph(this HtmlHelper helper, string pText)
        {
            string texto = string.Empty;

            if (!string.IsNullOrEmpty(pText) && pText.StartsWith("<p>") && pText.EndsWith("</p>"))
            {
                texto = pText.Substring(3).Replace(pText.Substring(pText.LastIndexOf("</p>")), string.Empty);
            }
            else
            {
                texto = pText;
            }

            return texto;
        }

        /// <summary>
        /// Obtiene la url para obtener los contextos de un recurso
        /// </summary>
        /// <param name="helper">Helper</param>
        /// <param name="pResourceUrl">Url completa del recurso</param>
        /// <returns>Url a la que hay que realizar una petición POST que obtendría todos los contextos del recurso</returns>
        public static string GetUrlContext(this HtmlHelper helper, string pResourceUrl, Guid pResourceID, string pCommunityShortName)
        {
            throw new NotImplementedException();
            ////string urlContexto = ControladorProyectoMVC.ObtenerUrlRecursoParaPeticionServicioContextos(helper.ViewBag.UrlServicioContextos, pResourceUrl, pResourceID, pCommunityShortName);
            //string urlContexto = pResourceUrl;
            //if (!string.IsNullOrEmpty(helper.ViewBag.UrlServicioContextos))
            //{
            //    urlContexto = string.Concat(helper.ViewBag.UrlServicioContextos, "/", pCommunityShortName, "/", pResourceID.ToString());
            //}
            ////

            //if (!string.IsNullOrEmpty(urlContexto) && !urlContexto.Equals(pResourceUrl))
            //{
            //    //Si es distinta, significa que hay un servicio de contextos
            //urlContexto += "/context";

            //if (helper.GetUtilIdiomas() != null)
            //{
            //    urlContexto += "/" + helper.GetUtilIdiomas().LanguageCode;
            //}
            //}
            //return urlContexto;
        }

        /// <summary>
        /// Obtiene la url principal del ecosistema
        /// </summary>
        /// <param name="helper">Helper</param>
        /// <param name="pResourceUrl">Url completa del recurso</param>
        /// <returns>Url a la que hay que realizar una petición POST que obtendría todos los contextos del recurso</returns>
        public static string GetUrlComunidadPrincipal(this HtmlHelper helper)
        {
            return helper.ViewBag.UrlComunidadPrincipal;
        }

        public static string GetUrlLogout(this HtmlHelper helper)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Permite obtener una SemanticProperty en el idioma de navegación
        /// </summary>
        /// <param name="helper">Helper</param>
        /// <param name="propiedad">Lista con los diferentes idiomas de la propiedad</param>
        /// <returns></returns>
        public static string ObtenerPropiedadTraducida(this HtmlHelper helper, List<SemanticPropertieModel> propiedad)
        {
            throw new NotImplementedException();
        }
       


        #region ViewBag Getters

        public static CommunityModel GetComunidad(this HtmlHelper helper)
        {
            return (CommunityModel)helper.ViewBag.Comunidad;
        }

        public static UserProfileModel GetPerfil(this HtmlHelper helper)
        {
            return (UserProfileModel)helper.ViewBag.Perfil;
        }

        public static string GetBodyClass(this HtmlHelper helper)
        {
            string bodyClass = (string)helper.ViewBag.BodyClass;

            if (string.IsNullOrEmpty(bodyClass))
            {
                return "";
            }

            return (string)helper.ViewBag.BodyClass;
        }

        public static string GetBodyClassPestanya(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.BodyClassPestanya;
        }

        public static string GetBaseUrlIdioma(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.BaseUrlIdioma;
        }

        public static bool GetOcultarPersonalizacion(this HtmlHelper helper)
        {
            if (helper.ViewBag.OcultarPersonalizacion != null)
            {
                return (bool)helper.ViewBag.OcultarPersonalizacion;
            }

            return false;
        }

        public static string GetPersonalizacion(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.Personalizacion;
        }

        public static string GetPersonalizacionLayout(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.PersonalizacionLayout;
        }

        public static string GetTextoSubida(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.TextoSubida;
        }

        public static ResultadoModel GetResultado(this HtmlHelper helper)
        {
            return (ResultadoModel)helper.ViewBag.Resultado;
        }

        public static List<FacetModel> GetFacetas(this HtmlHelper helper)
        {
            return (List<FacetModel>)helper.ViewBag.Facetas;
        }

        public static List<FacetItemModel> GetFiltros(this HtmlHelper helper)
        {
            return (List<FacetItemModel>)helper.ViewBag.Filtros;
        }

        public static SortedDictionary<string, List<short>> GetListaPaginasItem(this HtmlHelper helper)
        {
            return (SortedDictionary<string, List<short>>)helper.ViewBag.ListaPaginasItem;
        }

        public static string GetTituloPagina(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.TituloPagina;
        }

        public static string GetUrlPagina(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.UrlPagina;
        }

        public static string GetURLRSS(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.URLRSS;
        }

        public static string GetURLRDF(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.URLRDF;
        }

        public static List<KeyValuePair<string, string>> GetListaMetas(this HtmlHelper helper)
        {
            if (helper.ViewBag.ListaMetas != null)
            {
                return (List<KeyValuePair<string, string>>)helper.ViewBag.ListaMetas;
            }
            return new List<KeyValuePair<string, string>>();
        }

        public static List<string> GetListaJS(this HtmlHelper helper)
        {
            if (helper.ViewBag.ListaJS != null)
            {
                return (List<string>)helper.ViewBag.ListaJS;
            }
            return new List<string>();
        }

        public static string GetJSGraficos(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.JSGraficos;
        }

        public static string GetJSMapa(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.JSMapa;
        }

        public static string GetUrlLoadResourceActions(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.UrlLoadResourceActions;
        }

        public static string GetBaseUrl(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.BaseUrl;
        }

        public static string GetBaseUrlStatic(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.BaseUrlStatic;
        }

        public static string GetBaseUrlContent(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.BaseUrlContent;
        }

        public static string GetStyleVersion(this HtmlHelper helper)
        {
            string styleVersion = string.Empty;

            if (HttpContext.Current.Session["VersionEstilos"] != null)
            {
                styleVersion = string.Format("/versiones/{0}", HttpContext.Current.Session["VersionEstilos"]);
            }

            return styleVersion;
        }

        public static string GetBaseUrlPersonalizacion(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.BaseUrlPersonalizacion;
        }

        public static bool GetEsEcosistemaSinMetaProyecto(this HtmlHelper helper)
        {
            if (helper.ViewBag.EsEcosistemaSinMetaProyecto != null)
            {
                return (bool)helper.ViewBag.EsEcosistemaSinMetaProyecto;
            }

            return false;
        }

        public static string GetBaseUrlMetaProyecto(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.BaseUrlMetaProyecto;
        }

        public static UserIdentityModel GetIdentidadActual(this HtmlHelper helper)
        {
            return (UserIdentityModel)helper.ViewBag.IdentidadActual;
        }

        public static string GetNombreProyectoEcosistema(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.NombreProyectoEcosistema;
        }

        public static string GetNombreEspacioPersonal(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.NombreEspacioPersonal;
        }

        public static HeaderModel GetCabecera(this HtmlHelper helper)
        {
            return (HeaderModel)helper.ViewBag.Cabecera;
        }

        public static string GetUrlCanonical(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.UrlCanonical;
        }

        public static string GetControllerName(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.ControllerName;
        }

        public static string GetJSExtra(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.JSExtra;
        }

        public static List<Dictionary<string, string>> GetListaMetasComplejas(this HtmlHelper helper)
        {
            if (helper.ViewBag.ListaMetasComplejas != null)
            {
                return (List<Dictionary<string, string>>)helper.ViewBag.ListaMetasComplejas;
            }
            return new List<Dictionary<string, string>>();
        }

        public static string GetVersion(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.Version;
        }

        public static bool? GetJSYCSSunificado(this HtmlHelper helper)
        {
            if (helper.ViewBag.JSYCSSunificado != null)
            {
                return (bool)helper.ViewBag.JSYCSSunificado;
            }
            return null;
        }

        public static string GetGoogleAnalytics(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.GoogleAnalytics;
        }

        public static List<string> GetListaCSS(this HtmlHelper helper)
        {
            if (helper.ViewBag.ListaCSS != null)
            {
                return (List<string>)helper.ViewBag.ListaCSS;
            }
            return new List<string>();
        }

        public static List<string> GetBusquedasXml(this HtmlHelper helper)
        {
            if (helper.ViewBag.BusquedasXml != null)
            {
                return (List<string>)helper.ViewBag.BusquedasXml;
            }
            return new List<string>();
        }

        public static List<KeyValuePair<string, string>> GetListaInputHidden(this HtmlHelper helper)
        {
            if (helper.ViewBag.ListaInputHidden != null)
            {
                return (List<KeyValuePair<string, string>>)helper.ViewBag.ListaInputHidden;
            }
            return new List<KeyValuePair<string, string>>();
            
        }

        public static string GetUrlActionLogin(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.UrlActionLogin;
        }

        public static int GetEdadMinimaRegistro(this HtmlHelper helper)
        {
            if (helper.ViewBag.EdadMinimaRegistro != null)
            {
                return (int)helper.ViewBag.EdadMinimaRegistro;
            }
            return 0;
        }

        public static Guid GetIdentidadMensajeID(this HtmlHelper helper)
        {
            return (Guid)helper.ViewBag.IdentidadMensajeID;
        }

        public static string GetNombreUrlPestanya(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.NombreUrlPestanya;
        }

        public static List<ProfileModel> GetPerfilesAceptarinvitacion(this HtmlHelper helper)
        {
            if (helper.ViewBag.PerfilesAceptarinvitacion != null)
            {
                return (List<ProfileModel>)helper.ViewBag.PerfilesAceptarinvitacion;
            }
            return new List<ProfileModel>();
        }

        public static bool? GetSoloIdentidadPersonal(this HtmlHelper helper)
        {
            if (helper.ViewBag.SoloIdentidadPersonal != null)
            {
                return (bool)helper.ViewBag.SoloIdentidadPersonal;
            }
            return null;
        }

        public static string GetUrlOrigen(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.UrlOrigen;
        }

        public static string GetTitle(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.Title;
        }

        public static string GetGrafoID(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.GrafoID;
        }

        public static string GetParametros(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.Parametros;
        }

        public static string GetFiltroContextoWhere(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.FiltroContextoWhere;
        }

        public static bool? GetPintarH1(this HtmlHelper helper)
        {
            if (helper.ViewBag.PintarH1 != null)
            {
                return (bool)helper.ViewBag.PintarH1;
            }
            return null;

        }

        public static bool? GetOcultarMenusComunidad(this HtmlHelper helper)
        {
            if (helper.ViewBag.OcultarMenusComunidad != null)
            {
                return (bool)helper.ViewBag.OcultarMenusComunidad;
            }
            return null;
        }

        public static string GetFavicon(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.Favicon;
        }

        public static string GetNombrePestanya(this HtmlHelper helper)
        {
            return (string)helper.ViewBag.NombrePestanya;
        }

        public static CommunityModel.TabModel BuscarPestanyaPorID(this HtmlHelper helper, Guid pPestanyaID, List<CommunityModel.TabModel> listaPestanyas)
        {
            CommunityModel.TabModel pestanya = listaPestanyas.FirstOrDefault(tab => tab.Key.Equals(pPestanyaID));
            if (pestanya == null)
            {
                foreach (CommunityModel.TabModel tab in listaPestanyas.Where(t => t.SubTab != null && t.SubTab.Count > 0))
                {
                    pestanya = BuscarPestanyaPorID(helper, pPestanyaID, tab.SubTab);
                    if (pestanya != null) { break; }
                }
            }
            return pestanya;
        }

        #endregion

        #region ViewBag Setters

        public static void AddBodyClass(this HtmlHelper helper, string pBodyClass)
        {
            helper.ViewBag.BodyClass = $"{ helper.ViewBag.BodyClass } { pBodyClass }";
        }

        public static void SetBodyClass(this HtmlHelper helper, string pBodyClass)
        {
            helper.ViewBag.BodyClass = pBodyClass;
        }

        public static void SetBodyClassPestanya(this HtmlHelper helper, string pBodyClassPestanya)
        {
            helper.ViewBag.BodyClassPestanya = pBodyClassPestanya;
        }

        public static void SetTitle(this HtmlHelper helper, string pTitle)
        {
            helper.ViewBag.Title = pTitle;
        }

        public static void SetPintarH1(this HtmlHelper helper, Boolean pPintarH1)
        {
            helper.ViewBag.PintarH1 = pPintarH1;
        }

        public static void SetOcultarMenusComunidad(this HtmlHelper helper, Boolean pOcultarMenusComunidad)
        {
            helper.ViewBag.OcultarMenusComunidad = pOcultarMenusComunidad;
        }
        #endregion
    }
}