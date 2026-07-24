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

namespace RechnungsGenerator
{
    public partial class MainWindow : Window
    {
        private DB db = new DB();
        private ObservableCollection<PositionsPosten> positionenListe = new ObservableCollection<PositionsPosten>();
        private Dokument aktuellesDokument;

        public MainWindow()
        {
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
            Kunde ausgewaehlterKunde = lstKunden.SelectedItem as Kunde;
            double.TryParse(txtRabatt.Text, out double rabattProzent);

            string typ = (rbAngebot.IsChecked == true) ? "Angebot" : "Rechnung";

            aktuellesDokument = new Dokument("ENTWURF", typ, ausgewaehlterKunde, rabattProzent);

            foreach (var pos in positionenListe)
            {
                aktuellesDokument.Positionen.Add(pos);
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

        private void lstKunden_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BerechneSummen();
        }

        private void dgPositionen_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(BerechneSummen));
        }
    }
}
