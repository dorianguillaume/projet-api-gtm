using System;
using System.Collections.Generic;

namespace projetAPI_GTM.Models
{
    public partial class ModePaiement
    {
        public ModePaiement()
        {
            Facture = new HashSet<Facture>();
        }

        public string Code { get; set; }
        public string Libelle { get; set; }

        public virtual ICollection<Facture> Facture { get; set; }
    }
}
