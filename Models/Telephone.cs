using System;
using System.Collections.Generic;

namespace projetAPI_GTM.Models
{
    public partial class Telephone
    {
        public string Numero { get; set; }
        public int IdClient { get; set; }
        public string CodeType { get; set; }
        public bool Pro { get; set; }

        public virtual Client IdClientNavigation { get; set; }
    }
}
