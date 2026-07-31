using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RechnungsGenerator.Daten
{
    public class Dokument
    {
        public string DokumentNummer {  get; set; }
        public string Typ {  get; set; }
        public DateTime Datum { get; set; }
        public Kunde Empfaenger { get; set; }
        public List<PositionsPosten> Positionen {  get; set; } = new List<PositionsPosten>();
        public double RabattInProzent { get; set; }

        public double Zwischensumme
        {
            get
            {
                double summe = 0;
                foreach(var pos in Positionen)
                {
                    summe += pos.Gesamtpreis;
                }
                return summe;
            }
        }

        public double RabattBetrag => Zwischensumme * (RabattInProzent / 100);

        public double NettoNachRabatt => Zwischensumme - RabattBetrag;

        public double Mehrwertsteuer => NettoNachRabatt * 0.19;

        public double Endbetrag => NettoNachRabatt + Mehrwertsteuer;

        public Dokument(string nummer, string typ, Kunde empfaenger, double rabatt = 0)
        {
            DokumentNummer = nummer;
            Typ = typ;
            Empfaenger = empfaenger;
            Datum = DateTime.Now;
            RabattInProzent = rabatt;
        }
    }
}
