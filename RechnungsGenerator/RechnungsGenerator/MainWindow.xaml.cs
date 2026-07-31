using RechnungsGenerator.Daten;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace RechnungsGenerator
{
    public partial class MainWindow : Window
    {
        private DB db = new DB();
        private ObservableCollection<PositionsPosten> positionenListe = new ObservableCollection<PositionsPosten>();
        private Dokument aktuellesDokument;

        public MainWindow()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE");

            InitializeComponent();
            dgPositionen.ItemsSource = positionenListe;
            LadeKunden();
        }

        private void LadeKunden()
        {
            try
            {
                lstKunden.ItemsSource = db.LadeAlleKunden();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fehler beim Laden", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnNeuerKunde_Click(object sender, RoutedEventArgs e)
        {
            string knr = txtKundenNummer.Text.Trim();
            string name = txtName.Text.Trim();
            string adresse = txtAdresse.Text.Trim();

            if (string.IsNullOrEmpty(knr) || string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Bitte mindestens Kunden-Nummer und Name ausfüllen!",
                                "Eingabefehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                bool erfolg = db.AddKunde(knr, name, adresse);

                if (erfolg)
                {
                    MessageBox.Show("Kunde erfolgreich in Oracle gespeichert!",
                                    "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);

                    txtKundenNummer.Clear();
                    txtName.Clear();
                    txtAdresse.Clear();

                    LadeKunden();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Speichern: " + ex.Message,
                                "Datenbankfehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnPositionHinzufuegen_Click(object sender, RoutedEventArgs e)
        {
            positionenListe.Add(new PositionsPosten("Neue Position", 1, 0.00));
            BerechneSummen();
        }

        private void BerechneSummen()
        {
            if (lblZwischensumme == null || lblRabatt == null || lblMwSt == null || lblEndbetrag == null)
            {
                return;
            }

            Kunde ausgewaehlterKunde = lstKunden.SelectedItem as Kunde;

            double rabattProzent = 0;
            if (txtRabatt != null)
            {
                double.TryParse(txtRabatt.Text, out rabattProzent);
            }

            string typ = (rbAngebot != null && rbAngebot.IsChecked == true) ? "Angebot" : "Rechnung";
            string nummer = db.GetNaechsteNummer(typ);

            aktuellesDokument = new Dokument(nummer, typ, ausgewaehlterKunde, rabattProzent);

            if (positionenListe != null)
            {
                foreach (var pos in positionenListe)
                {
                    aktuellesDokument.Positionen.Add(pos);
                }
            }

            lblZwischensumme.Text = aktuellesDokument.Zwischensumme.ToString("C");
            lblRabatt.Text = aktuellesDokument.RabattBetrag.ToString("C");
            lblMwSt.Text = aktuellesDokument.Mehrwertsteuer.ToString("C");
            lblEndbetrag.Text = aktuellesDokument.Endbetrag.ToString("C");
        }

        private void txtRabatt_TextChanged(object sender, TextChangedEventArgs e)
        {
            BerechneSummen();
        }

        private void DokumentTyp_Changed(object sender, RoutedEventArgs e)
        {
            BerechneSummen();
        }

        private void LadeHistorie()
        {
            Kunde k = lstKunden.SelectedItem as Kunde;
            if (k != null)
            {
                try
                {
                    lstErstellteDokumente.ItemsSource = db.GetDokumenteFuerKunde(k.KundenID);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Laden der Dokumenten-Historie:\n" + ex.Message,
                                    "Datenbankfehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                lstErstellteDokumente.ItemsSource = null;
            }
        }
        private void lstKunden_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BerechneSummen();
            LadeHistorie();

            Kunde k = lstKunden.SelectedItem as Kunde;
            if (k != null)
            {
                txtKundenNummer.Text = k.KundenNummer;
                txtName.Text = k.Name;
                txtAdresse.Text = k.Adresse;
            }
        }

        // OPTIONALE ZUSATZFUNKTION Knopf zum Bearbeiten der Kunden
        private void btnKundeBearbeiten_Click(object sender, RoutedEventArgs e)
        {
            Kunde k = lstKunden.SelectedItem as Kunde;
            if (k == null)
            {
                MessageBox.Show("Bitte wählen Sie zuerst einen Kunden aus der Liste aus!", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                bool erfolg = db.UpdateKunde(k.KundenID, txtKundenNummer.Text.Trim(), txtName.Text.Trim(), txtAdresse.Text.Trim());
                if (erfolg)
                {
                    MessageBox.Show("Kundendaten wurden erfolgreich aktualisiert!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                    LadeKunden();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Ende der OPTIONALE ZUSATZFUNKTION

        // OPTIONALE ZUSATZFUNKTION Knopf zum Löschen der Kunden
        private void btnKundeLoeschen_Click(object sender, RoutedEventArgs e)
        {
            Kunde k = lstKunden.SelectedItem as Kunde;
            if (k == null)
            {
                MessageBox.Show("Bitte wählen Sie zuerst einen Kunden aus der Liste aus!", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult res = MessageBox.Show($"Möchten Sie den Kunden '{k.Name}' wirklich löschen?",
                                                   "Sicherheitsabfrage", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (res == MessageBoxResult.Yes)
            {
                try
                {
                    bool erfolg = db.DeleteKunde(k.KundenID);
                    if (erfolg)
                    {
                        MessageBox.Show("Kunde wurde erfolgreich gelöscht!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);

                        txtKundenNummer.Clear();
                        txtName.Clear();
                        txtAdresse.Clear();

                        LadeKunden();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Fehler beim Löschen", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        // Ende der OPTIONALE ZUSATZFUNKTION

        private void dgPositionen_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(BerechneSummen));
        }

        private void btnDokumentErstellen_Click(object sender, RoutedEventArgs e)
        {
            if (lstKunden.SelectedItem == null)
            {
                MessageBox.Show("Bitte wählen Sie zuerst einen Kunden aus!", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (positionenListe.Count == 0)
            {
                MessageBox.Show("Bitte fügen Sie mindestens eine Position hinzu!", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                bool erfolg = db.AddDokument(aktuellesDokument);

                if (erfolg)
                {
                    ExportiereAlsTextdatei(aktuellesDokument);

                    MessageBox.Show($"{aktuellesDokument.Typ} {aktuellesDokument.DokumentNummer} wurde erfolgreich in Oracle gespeichert und als Datei ausgegeben!",
                                    "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);

                    positionenListe.Clear();
                    BerechneSummen();
                    LadeHistorie();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Erstellen des Dokuments:\n" + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportiereAlsTextdatei(Dokument dok)
        {
            string dateiName = $"{dok.DokumentNummer}.txt";
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("==================================================");
            sb.AppendLine($"               {dok.Typ.ToUpper()} {dok.DokumentNummer}");
            sb.AppendLine("==================================================");
            sb.AppendLine($"Datum: {dok.Datum:dd.MM.yyyy HH:mm}");
            sb.AppendLine($"Kunde: {dok.Empfaenger.Name} (Nr: {dok.Empfaenger.KundenNummer})");
            sb.AppendLine($"Adresse: {dok.Empfaenger.Adresse}");
            sb.AppendLine("--------------------------------------------------");
            sb.AppendLine("POSITIONEN:");

            foreach (var pos in dok.Positionen)
            {
                sb.AppendLine($"- {pos.Bezeichnung} | {pos.Menge}x @ {pos.Einzelpreis:N2} € = {pos.Gesamtpreis:N2} €");
            }

            sb.AppendLine("--------------------------------------------------");
            sb.AppendLine($"Zwischensumme:        {dok.Zwischensumme:N2} €");
            sb.AppendLine($"Rabatt ({dok.RabattInProzent}%):         -{dok.RabattBetrag:N2} €");
            sb.AppendLine($"MwSt. (19%):          {dok.Mehrwertsteuer:N2} €");
            sb.AppendLine("==================================================");
            sb.AppendLine($"ENDBETRAG:            {dok.Endbetrag:N2} €");
            sb.AppendLine("==================================================");

            File.WriteAllText(dateiName, sb.ToString());
        }
    }
}
