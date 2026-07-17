using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RechnungsGenerator.Daten
{
    public class Kunde
    {
        public int KundenID { get; set; }
        public string KundenNummer { get; set; }
        public string Name { get; set; }
        public string Adresse { get; set; }

        public Kunde(int id, string kundenNummer, string name, string adresse) 
        {
            KundenID = id;
            KundenNummer = kundenNummer;
            Name = name;
            Adresse = adresse;
        }

        public override string ToString()
        {
            return $"[{KundenNummer}] {Name}";
        }
    }
}
