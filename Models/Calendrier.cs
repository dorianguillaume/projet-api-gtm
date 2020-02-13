using System;
using System.Collections.Generic;

namespace projetAPI_GTM.Models
{
    public partial class Calendrier
    {
        public Calendrier()
        {
            Reservation = new HashSet<Reservation>();
        }

        public DateTime Jour { get; set; }

        public virtual ICollection<Reservation> Reservation { get; set; }
    }
}
