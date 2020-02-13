using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GnossTestView.Areas.GnossTestView.Model
{
    public class ConfigurationModel
    {
        public string urlApiDespliegues { get; set; }

        public List<AutorizationModel> autorizacion { get; set; }

        public string[] proyectos { get; set; }

        public class AutorizationModel
        {
            public string url { get; set; }
            public string user { get; set; }
            public string password { get; set; }
        }
    }
}