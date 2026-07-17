using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RechnungsGenerator.Daten
{
    public class PositionsPosten
    {
        public string Bezeichnung {  get; set; }
        public double Menge {  get; set; }
        public double Einzelpreis {  get; set; }

        public double Gesamtpreis => Menge * Einzelpreis;

        public PositionsPosten(string bezeichnug, double menge, double einzelpreis)
        {
            Bezeichnung = bezeichnug;
            Menge = menge;
            Einzelpreis = einzelpreis;
        }
    }
}
