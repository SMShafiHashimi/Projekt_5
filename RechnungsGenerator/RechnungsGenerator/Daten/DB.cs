using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Windows;

namespace RechnungsGenerator.Daten
{
    public class DB
    {
        private OracleConnection con;
        private OracleDataReader reader;
        private OracleCommand cmd;

        private string kennung = "bbm3h23mhs";
        private string password = "info1234";

        public DB()
        {
            string connString = "Data Source=dbserver2.bg.bib.de:1521/ora10.bg.bib.de;User ID="
                + kennung + ";Password=" + password;

            con = new OracleConnection(connString);

            try
            {
                con.Open();
                string serverVersion = con.ServerVersion;
                Console.WriteLine("Verbindung erfolgreich. Oracle Version: " + serverVersion);
                con.Close();
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open) con.Close();
                Console.WriteLine("Fehler beim Verbindungsaufbau: " + ex.Message);
            }
        }

        public List<Kunde> LadeAlleKunden()
        {
            List<Kunde> kundenListe = new List<Kunde>();
            try
            {
                cmd = new OracleCommand();
                cmd.Connection = con;
                cmd.CommandText = "SELECT kundenid, kundennummer, name, adresse FROM kunden ORDER BY name";

                cmd.Connection.Open();
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int id = Convert.ToInt32(reader["kundenid"]);
                    string knr = reader["kundennummer"] != DBNull.Value ? reader["kundennummer"].ToString() : "";
                    string name = reader["name"] != DBNull.Value ? reader["name"].ToString() : "";
                    string adresse = reader["adresse"] != DBNull.Value ? reader["adresse"].ToString() : "";

                    kundenListe.Add(new Kunde(id, knr, name, adresse));
                }

                reader.Close();
                con.Close();
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open) con.Close();
                throw new Exception("Fehler in LadeAlleKunden: " + ex.Message);
            }

            return kundenListe;
        }

        public bool AddKunde(string kundenNummer, string name, string adresse)
        {
            int rows = 0;
            try
            {
                cmd = con.CreateCommand();
                cmd.CommandText = "INSERT INTO kunden (kundennummer, name, adresse) " +
                                  "VALUES ('" + kundenNummer + "', '" + name + "', '" + adresse + "')";

                if (con.State != ConnectionState.Open) con.Open();
                rows = cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open) con.Close();
                throw new Exception("Fehler in AddKunde: " + ex.Message);
            }

            return rows > 0;
        }
        public bool AddDokument(Dokument dok)
        {
            if (dok == null || dok.Empfaenger == null) return false;

            try
            {
                if (con.State != ConnectionState.Open) con.Open();

                cmd = con.CreateCommand();
                cmd.CommandText = "SELECT NVL(MAX(dokumentid), 0) + 1 FROM dokumente";
                int neueDokumentId = Convert.ToInt32(cmd.ExecuteScalar());

                string datumSQL = "TO_DATE('" + dok.Datum.ToString("dd.MM.yyyy HH:mm:ss") + "', 'DD.MM.YYYY HH24:MI:SS')";

                cmd.CommandText = "INSERT INTO dokumente (dokumentid, dokumentnummer, typ, erstellungsdatum, rabattinprozent, kundenid) " +
                                  "VALUES (" + neueDokumentId + ", '" + dok.DokumentNummer + "', '" + dok.Typ + "', " +
                                  datumSQL + ", " + dok.RabattInProzent.ToString().Replace(',', '.') + ", " + dok.Empfaenger.KundenID + ")";

                cmd.ExecuteNonQuery();

                foreach (var pos in dok.Positionen)
                {
                    cmd.CommandText = "INSERT INTO dokumentpositionen (bezeichnung, menge, einzelpreis, dokumentid) " +
                                      "VALUES ('" + pos.Bezeichnung + "', " +
                                      pos.Menge.ToString().Replace(',', '.') + ", " +
                                      pos.Einzelpreis.ToString().Replace(',', '.') + ", " +
                                      neueDokumentId + ")";

                    cmd.ExecuteNonQuery();
                }

                con.Close();
                return true;
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open) con.Close();
                throw new Exception("Fehler beim Speichern in Oracle: " + ex.Message);
            }
        }

        public string GetNaechsteNummer(string typ)
        {
            string praefix = (typ == "Rechnung") ? "RE" : "AN";
            int naechsteZahl = 1;

            try
            {
                cmd = con.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM dokumente WHERE typ = '" + typ + "'";

                if (con.State != ConnectionState.Open) con.Open();
                object result = cmd.ExecuteScalar();

                if (result != DBNull.Value && result != null)
                {
                    naechsteZahl = Convert.ToInt32(result) + 1;
                }
                con.Close();
            }
            catch
            {
                if (con.State == ConnectionState.Open) con.Close();
            }

            return praefix + "-" + DateTime.Now.Year + "-" + naechsteZahl.ToString("D3");
        }

        public List<string> GetDokumenteFuerKunde(int kundenId)
        {
            List<string> liste = new List<string>();
            try
            {
                cmd = new OracleCommand();
                cmd.Connection = con;
                cmd.CommandText = "SELECT dokumentnummer, typ, erstellungsdatum FROM dokumente " +
                                  "WHERE kundenid = " + kundenId + " ORDER BY erstellungsdatum DESC";

                cmd.Connection.Open();
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string nr = reader["dokumentnummer"].ToString();
                    string typ = reader["typ"].ToString();
                    DateTime datum = Convert.ToDateTime(reader["erstellungsdatum"]);

                    liste.Add($"[{typ}] {nr} vom {datum:dd.MM.yyyy}");
                }

                reader.Close();
                con.Close();
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open) con.Close();
                throw new Exception("Fehler beim Laden der Historie: " + ex.Message);
            }

            return liste;
        }
        // OPTIONALE ZUSATZFUNKTION Kunden bearbeiten
        public bool UpdateKunde(int kundenId, string kundenNummer, string name, string adresse)
        {
            int rows = 0;
            try
            {
                cmd = con.CreateCommand();
                cmd.CommandText = "UPDATE kunden SET " +
                                  "kundennummer = '" + kundenNummer + "', " +
                                  "name = '" + name + "', " +
                                  "adresse = '" + adresse + "' " +
                                  "WHERE kundenid = " + kundenId;

                if (con.State != ConnectionState.Open) con.Open();
                rows = cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open) con.Close();
                throw new Exception("Fehler beim Aktualisieren des Kunden: " + ex.Message);
            }

            return rows > 0;
        }
        // Ende der OPTIONALE ZUSATZFUNKTION

        // OPTIONALE ZUSATZFUNKTION Kunden löschen
        public bool DeleteKunde(int kundenId)
        {
            int rows = 0;
            try
            {
                cmd = con.CreateCommand();
                cmd.CommandText = "DELETE FROM kunden WHERE kundenid = " + kundenId;

                if (con.State != ConnectionState.Open) con.Open();
                rows = cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open) con.Close();
                throw new Exception("Fehler beim Löschen des Kunden (mögliche verknüpfte Dokumente vorhanden): " + ex.Message);
            }

            return rows > 0;
        }
        // Ende der OPTIONALE ZUSATZFUNKTION
    }
}