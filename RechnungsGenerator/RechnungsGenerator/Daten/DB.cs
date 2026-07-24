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
    }
}