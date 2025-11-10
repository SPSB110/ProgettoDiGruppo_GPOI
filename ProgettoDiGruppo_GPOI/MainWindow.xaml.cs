using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Text;

namespace ProgettoDiGruppo_GPOI
{
    public partial class MainWindow : Window
    {
        public static List<Matrix> Tabelle;
        public static List<float> Produzione = new List<float>();
        public static List<float> Destinazioni = new List<float>();
        public static List<float> Totali = new List<float>();
        public static int nCol = 4;
        public static int nRow = 3;
        private int[,] costiTrasporto;
        private int[] offerte;
        private int[] domande;
        private int nRigheCorrente;
        private int nColonneCorrente;
        private StringBuilder logCalcoli;

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += (s, e) =>
            {
                this.Title = "Lavoro di gruppo - GPOI";
                this.WindowState = WindowState.Maximized;
                this.FontFamily = new FontFamily("Segoe UI");
                this.FontSize = 14;
                this.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));

                this.Height = (System.Windows.SystemParameters.PrimaryScreenHeight);
                this.Width = (System.Windows.SystemParameters.PrimaryScreenWidth);

                float topGridHeight = 0.07f;
                this.TopGrid.Height = this.Height * topGridHeight;
                this.leftGrid.Height = this.Height * (1 - topGridHeight);
                this.leftGrid.Margin = new Thickness(0, this.TopGrid.Height, 0, 0);
                this.mainGrid.Margin = new Thickness(this.leftGrid.Width, this.TopGrid.Height, 0, 0);
                this.rightPanel.Margin = new Thickness(0, this.TopGrid.Height, 0, 0);

                this.btnHome.Height = this.TopGrid.Height * 0.7;
            };
            this.SizeChanged += MainWindow_SizeChanged;
        }

        private void AggiungiLog(string messaggio)
        {
            logCalcoli.AppendLine(messaggio);
            txtCalcoli.Text = logCalcoli.ToString();
        }

        private void PulisciLog()
        {
            logCalcoli = new StringBuilder();
            txtCalcoli.Text = "";
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(rowTextBox.Text, out int nRighe) || nRighe <= 0)
            {
                MessageBox.Show("Numero di righe non valido!");
                return;
            }

            if (!int.TryParse(colTextBox.Text, out int nColonne) || nColonne <= 0)
            {
                MessageBox.Show("Numero di colonne non valido!");
                return;
            }

            // 🥚 EASTER EGG: 67 righe e 41 colonne
            /*
            if (nRighe == 67 && nColonne == 41)
            {
                MostraEasterEgg();
                return;
            }*/

            CreaTabella(nRighe, nColonne);
        }

        private void MostraEasterEgg()
        {
            // Nascondi la DataGrid
            mainGrid.Visibility = Visibility.Collapsed;

            // Mostra l'immagine
            // Carica l'immagine dal percorso assoluto
            easterEggImage.Source = new BitmapImage(new Uri(@"C:\Users\vassalli.21096\Desktop\Compiti GPO\ProgettoDiGruppo_GPOI-master\ProgettoDiGruppo_GPOI\bin\Debug\ps1.jpg"));

            easterEggImage.Visibility = Visibility.Visible;

            // Mostra un messaggio
            MessageBox.Show("🎉 Hai trovato l'Easter Egg!\n67 righe e 41 colonne... numeri speciali!",
                            "Segreto Svelato",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        private void CreaTabella(int nRighe, int nColonne)
        {
            // Nascondi easter egg e mostra DataGrid
            easterEggImage.Visibility = Visibility.Collapsed;
            mainGrid.Visibility = Visibility.Visible;

            // Pulisci i log
            PulisciLog();

            // Salva le dimensioni
            nRigheCorrente = nRighe;
            nColonneCorrente = nColonne;

            // Inizializza gli array
            costiTrasporto = new int[nRighe, nColonne];
            offerte = new int[nRighe];
            domande = new int[nColonne];

            mainGrid.Columns.Clear();

            // Prima colonna = intestazioni righe
            mainGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "",
                Binding = new Binding("Riga"),
                IsReadOnly = true
            });

            // Crea intestazioni colonne dinamiche (D1, D2, …)
            string[] intestazioniColonne = new string[nColonne + 1];
            for (int c = 0; c < nColonne; c++)
            {
                intestazioniColonne[c] = $"D{c + 1}";
                mainGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = intestazioniColonne[c],
                    Binding = new Binding(intestazioniColonne[c]),
                    IsReadOnly = false
                });
            }
            intestazioniColonne[nColonne] = "Offerta";
            mainGrid.Columns.Add(new DataGridTextColumn
            {
                Header = intestazioniColonne[nColonne],
                Binding = new Binding(intestazioniColonne[nColonne]),
                IsReadOnly = false
            });

            // Crea righe dinamiche (UP1, UP2, …)
            var listaRighe = new List<ExpandoObject>();
            for (int r = 0; r < nRighe; r++)
            {
                dynamic row = new ExpandoObject();
                var dict = (IDictionary<string, object>)row;

                dict["Riga"] = $"UP{r + 1}";

                foreach (var col in intestazioniColonne)
                {
                    dict[col] = ""; // celle inizialmente vuote
                }

                listaRighe.Add(row);
            }
            dynamic row1 = new ExpandoObject();
            var dict1 = (IDictionary<string, object>)row1;

            dict1["Riga"] = "Domanda";

            foreach (var col in intestazioniColonne)
            {
                dict1[col] = ""; // celle inizialmente vuote
            }

            listaRighe.Add(row1);

            mainGrid.ItemsSource = listaRighe;

            // Abilita i pulsanti degli algoritmi
            btnNordOvest.IsEnabled = true;
            btnMinimiCosti.IsEnabled = true;
            btnReset.IsEnabled = true;
            btnAutofill.IsEnabled = true;
        }

        private void rowTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void colTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void btnNordOvest_Click(object sender, RoutedEventArgs e)
        {
            if (!LeggiDatiTabella())
            {
                MessageBox.Show("Inserisci tutti i costi, le offerte e le domande prima di applicare l'algoritmo!");
                return;
            }

            if (!VerificaBilanciamento())
            {
                MessageBox.Show("Problema non bilanciato! La somma delle offerte deve essere uguale alla somma delle domande.");
                return;
            }

            ApplicaMetodoNordOvest();
        }

        private void btnMinimiCosti_Click(object sender, RoutedEventArgs e)
        {
            if (!LeggiDatiTabella())
            {
                MessageBox.Show("Inserisci tutti i costi, le offerte e le domande prima di applicare l'algoritmo!");
                return;
            }

            if (!VerificaBilanciamento())
            {
                MessageBox.Show("Problema non bilanciato! La somma delle offerte deve essere uguale alla somma delle domande.");
                return;
            }

            ApplicaMetodoMinimiCosti();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            CreaTabella(nRigheCorrente, nColonneCorrente);
        }

        private void btnAutofill_Click(object sender, RoutedEventArgs e)
        {
            RiempiTabellaAutomaticamente();
        }

        private void RiempiTabellaAutomaticamente()
        {
            var items = mainGrid.ItemsSource as List<ExpandoObject>;
            if (items == null) return;

            Random random = new Random();

            // Riempimento dei costi (valori da 1 a 50)
            for (int r = 0; r < nRigheCorrente; r++)
            {
                var dict = (IDictionary<string, object>)items[r];
                for (int c = 0; c < nColonneCorrente; c++)
                {
                    dict[$"D{c + 1}"] = random.Next(1, 51).ToString();
                }
            }

            // Genera offerte casuali (valori da 50 a 200)
            int[] offerteTemp = new int[nRigheCorrente];
            for (int r = 0; r < nRigheCorrente; r++)
            {
                offerteTemp[r] = random.Next(50, 201);
            }

            // Genera domande casuali che bilancino le offerte
            int totaleOfferte = 0;
            foreach (int off in offerteTemp)
                totaleOfferte += off;

            int[] demandeTemp = new int[nColonneCorrente];
            int rimanente = totaleOfferte;

            // Distribuisci le domande in modo casuale ma bilanciato
            for (int c = 0; c < nColonneCorrente - 1; c++)
            {
                int max = rimanente - (nColonneCorrente - c - 1) * 50; // Assicura che rimanga abbastanza per le altre colonne
                int min = Math.Max(50, rimanente - (nColonneCorrente - c - 1) * 200);
                demandeTemp[c] = random.Next(Math.Min(min, max), Math.Min(max + 1, rimanente));
                rimanente -= demandeTemp[c];
            }
            demandeTemp[nColonneCorrente - 1] = rimanente; // L'ultima prende tutto il resto per bilanciare

            // Inserisci offerte nella tabella
            for (int r = 0; r < nRigheCorrente; r++)
            {
                var dict = (IDictionary<string, object>)items[r];
                dict["Offerta"] = offerteTemp[r].ToString();
            }

            // Inserisci domande nella tabella (ultima riga)
            var dictDomande = (IDictionary<string, object>)items[nRigheCorrente];
            for (int c = 0; c < nColonneCorrente; c++)
            {
                dictDomande[$"D{c + 1}"] = demandeTemp[c].ToString();
            }
            dictDomande["Offerta"] = ""; // Cella vuota nell'angolo

            mainGrid.Items.Refresh();

            MessageBox.Show($"Tabella riempita automaticamente!\n\nTotale Offerte: {totaleOfferte}\nTotale Domande: {totaleOfferte}\n\nProblema bilanciato e pronto per gli algoritmi! 🚚",
                           "Riempimento Completato",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);
        }

        private bool LeggiDatiTabella()
        {
            try
            {
                var items = mainGrid.ItemsSource as List<ExpandoObject>;
                if (items == null) return false;

                // Leggi i costi e le offerte
                for (int r = 0; r < nRigheCorrente; r++)
                {
                    var dict = (IDictionary<string, object>)items[r];

                    // Leggi offerta
                    if (!int.TryParse(dict["Offerta"]?.ToString(), out offerte[r]))
                        return false;

                    // Leggi costi
                    for (int c = 0; c < nColonneCorrente; c++)
                    {
                        if (!int.TryParse(dict[$"D{c + 1}"]?.ToString(), out costiTrasporto[r, c]))
                            return false;
                    }
                }

                // Leggi le domande (ultima riga)
                var dictDomande = (IDictionary<string, object>)items[nRigheCorrente];
                for (int c = 0; c < nColonneCorrente; c++)
                {
                    if (!int.TryParse(dictDomande[$"D{c + 1}"]?.ToString(), out domande[c]))
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool VerificaBilanciamento()
        {
            int totaleOfferte = 0;
            int totaleDomande = 0;

            for (int i = 0; i < nRigheCorrente; i++)
                totaleOfferte += offerte[i];

            for (int i = 0; i < nColonneCorrente; i++)
                totaleDomande += domande[i];

            return totaleOfferte == totaleDomande;
        }

        private void ApplicaMetodoNordOvest()
        {
            PulisciLog();
            AggiungiLog("═══════════════════════════════════════════");
            AggiungiLog("   METODO NORD-OVEST");
            AggiungiLog("═══════════════════════════════════════════\n");

            // Mostra stato iniziale
            AggiungiLog(" STATO INIZIALE:");
            AggiungiLog("-------------------");
            for (int x = 0; x < nRigheCorrente; x++)
                AggiungiLog($"  Offerta UP{x + 1}: {offerte[x]}");
            AggiungiLog("");
            for (int z = 0; z < nColonneCorrente; z++)
                AggiungiLog($"  Domanda D{z + 1}: {domande[z]}");
            AggiungiLog("\n");

            int[,] allocazioni = new int[nRigheCorrente, nColonneCorrente];
            int[] offerteRimanenti = (int[])offerte.Clone();
            int[] demandeRimanenti = (int[])domande.Clone();

            int i = 0, j = 0;
            int passo = 1;

            // Algoritmo Nord-Ovest
            AggiungiLog(" ESECUZIONE ALGORITMO:");
            AggiungiLog("========================\n");

            while (i < nRigheCorrente && j < nColonneCorrente)
            {
                int quantita = Math.Min(offerteRimanenti[i], demandeRimanenti[j]);
                allocazioni[i, j] = quantita;

                AggiungiLog($"Passo {passo}:");
                AggiungiLog($"  Cella [UP{i + 1}, D{j + 1}]");
                AggiungiLog($"  Offerta rimanente: {offerteRimanenti[i]}");
                AggiungiLog($"  Domanda rimanente: {demandeRimanenti[j]}");
                AggiungiLog($"  Quantità allocata: {quantita}");
                AggiungiLog($"  Costo unitario: {costiTrasporto[i, j]}");
                AggiungiLog($"  Costo parziale: {costiTrasporto[i, j]} × {quantita} = {costiTrasporto[i, j] * quantita}");

                offerteRimanenti[i] -= quantita;
                demandeRimanenti[j] -= quantita;

                if (offerteRimanenti[i] == 0)
                {
                    AggiungiLog($"  -> Offerta UP{i + 1} ESAURITA, avanzo alla riga successiva\n");
                    i++;
                }
                else
                {
                    AggiungiLog($"  -> Domanda D{j + 1} SODDISFATTA, avanzo alla colonna successiva\n");
                    j++;
                }

                passo++;
            }

            MostraRisultato(allocazioni, "Metodo Nord-Ovest");
        }

        private void ApplicaMetodoMinimiCosti()
        {
            PulisciLog();
            AggiungiLog("═══════════════════════════════════════════");
            AggiungiLog("   METODO MINIMI COSTI");
            AggiungiLog("═══════════════════════════════════════════\n");

            // Mostra stato iniziale
            AggiungiLog(" STATO INIZIALE:");
            AggiungiLog("-------------------");
            for (int i = 0; i < nRigheCorrente; i++)
                AggiungiLog($"  Offerta UP{i + 1}: {offerte[i]}");
            AggiungiLog("");
            for (int j = 0; j < nColonneCorrente; j++)
                AggiungiLog($"  Domanda D{j + 1}: {domande[j]}");
            AggiungiLog("\n");

            AggiungiLog(" MATRICE DEI COSTI:");
            AggiungiLog("---------------------");
            StringBuilder matriceCosti = new StringBuilder("     ");
            for (int j = 0; j < nColonneCorrente; j++)
                matriceCosti.Append($"D{j + 1}".PadRight(6));
            AggiungiLog(matriceCosti.ToString());

            for (int i = 0; i < nRigheCorrente; i++)
            {
                StringBuilder riga = new StringBuilder($"UP{i + 1}".PadRight(5));
                for (int j = 0; j < nColonneCorrente; j++)
                {
                    riga.Append(costiTrasporto[i, j].ToString().PadRight(6));
                }
                AggiungiLog(riga.ToString());
            }
            AggiungiLog("\n");

            int[,] allocazioni = new int[nRigheCorrente, nColonneCorrente];
            int[] offerteRimanenti = (int[])offerte.Clone();
            int[] demandeRimanenti = (int[])domande.Clone();
            bool[,] usato = new bool[nRigheCorrente, nColonneCorrente];

            int passo = 1;

            // Algoritmo Minimi Costi
            AggiungiLog(" ESECUZIONE ALGORITMO:");
            AggiungiLog("========================\n");

            while (true)
            {
                // Trova la cella con costo minimo non ancora utilizzata
                int minCosto = int.MaxValue;
                int minI = -1, minJ = -1;

                for (int i = 0; i < nRigheCorrente; i++)
                {
                    for (int j = 0; j < nColonneCorrente; j++)
                    {
                        if (!usato[i, j] && offerteRimanenti[i] > 0 && demandeRimanenti[j] > 0)
                        {
                            if (costiTrasporto[i, j] < minCosto)
                            {
                                minCosto = costiTrasporto[i, j];
                                minI = i;
                                minJ = j;
                            }
                        }
                    }
                }

                if (minI == -1) break; // Nessuna cella disponibile

                // Alloca la quantità minima
                int quantita = Math.Min(offerteRimanenti[minI], demandeRimanenti[minJ]);
                allocazioni[minI, minJ] = quantita;

                AggiungiLog($"Passo {passo}:");
                AggiungiLog($"  Costo minimo trovato: {minCosto}");
                AggiungiLog($"  Cella [UP{minI + 1}, D{minJ + 1}]");
                AggiungiLog($"  Offerta rimanente UP{minI + 1}: {offerteRimanenti[minI]}");
                AggiungiLog($"  Domanda rimanente D{minJ + 1}: {demandeRimanenti[minJ]}");
                AggiungiLog($"  Quantità allocata: {quantita}");
                AggiungiLog($"  Costo parziale: {minCosto} × {quantita} = {minCosto * quantita}");

                offerteRimanenti[minI] -= quantita;
                demandeRimanenti[minJ] -= quantita;

                // Marca come usata se offerta o domanda è esaurita
                if (offerteRimanenti[minI] == 0 || demandeRimanenti[minJ] == 0)
                {
                    usato[minI, minJ] = true;
                    if (offerteRimanenti[minI] == 0)
                        AggiungiLog($"  -> Offerta UP{minI + 1} ESAURITA");
                    if (demandeRimanenti[minJ] == 0)
                        AggiungiLog($"  -> Domanda D{minJ + 1} SODDISFATTA");
                }

                AggiungiLog("");
                passo++;
            }

            MostraRisultato(allocazioni, "Metodo Minimi Costi");
        }

        private void MostraRisultato(int[,] allocazioni, string metodo)
        {
            var items = mainGrid.ItemsSource as List<ExpandoObject>;
            if (items == null) return;

            int costoTotale = 0;

            AggiungiLog("\n═══════════════════════════════════════════");
            AggiungiLog("   RISULTATO FINALE");
            AggiungiLog("═══════════════════════════════════════════\n");

            AggiungiLog(" ALLOCAZIONI:");
            AggiungiLog("--------------");

            // Aggiorna la tabella con le allocazioni
            for (int r = 0; r < nRigheCorrente; r++)
            {
                var dict = (IDictionary<string, object>)items[r];
                bool haAllocazioni = false;

                for (int c = 0; c < nColonneCorrente; c++)
                {
                    if (allocazioni[r, c] > 0)
                    {
                        dict[$"D{c + 1}"] = $"{costiTrasporto[r, c]} ({allocazioni[r, c]})";
                        int costoParzialeRiga = costiTrasporto[r, c] * allocazioni[r, c];
                        costoTotale += costoParzialeRiga;

                        if (!haAllocazioni)
                        {
                            AggiungiLog($"\n  UP{r + 1}:");
                            haAllocazioni = true;
                        }
                        AggiungiLog($"    -> D{c + 1}: {allocazioni[r, c]} unità a costo {costiTrasporto[r, c]} = {costoParzialeRiga}");
                    }
                    else
                    {
                        dict[$"D{c + 1}"] = costiTrasporto[r, c].ToString();
                    }
                }
            }

            AggiungiLog("\n");
            AggiungiLog("═══════════════════════════════════════════");
            AggiungiLog($" COSTO TOTALE DI TRASPORTO: {costoTotale}");
            AggiungiLog("═══════════════════════════════════════════");

            mainGrid.Items.Refresh();

            MessageBox.Show($"{metodo} applicato con successo!\n\nCosto totale di trasporto: {costoTotale}",
                           "Risultato",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);
        }
    }
}