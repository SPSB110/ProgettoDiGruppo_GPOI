using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Text;
using System.Text.RegularExpressions;

namespace ProgettoDiGruppo_GPOI
{
    public partial class MainWindow : Window
    {
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

                this.Height = (SystemParameters.PrimaryScreenHeight);
                this.Width = (SystemParameters.PrimaryScreenWidth);

                float topGridHeight = 0.07f;
                this.TopGrid.Height = this.Height * topGridHeight;
                this.leftGrid.Height = this.Height * (1 - topGridHeight);
                this.leftGrid.Margin = new Thickness(0, this.TopGrid.Height - 1, 0, 0);
                this.mainGrid.Margin = new Thickness(this.leftGrid.Width, this.TopGrid.Height, 0, 0);
                this.rightPanel.Margin = new Thickness(0, this.TopGrid.Height, 0, 0);

                //this.btnHome.Height = this.TopGrid.Height * 0.7;
            };
            //this.SizeChanged += MainWindow_SizeChanged;
        }

        /*private void AggiungiLog(string messaggio)
        {
            logCalcoli.AppendLine(messaggio);
            txtCalcoli.Text = logCalcoli.ToString();
        }*/

        private void PulisciLog()
        {
            logCalcoli = new StringBuilder();
            txtCalcoli.Text = "";
        }

        private void btnCreaTabella_Click(object sender, RoutedEventArgs e)
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

            //da rivedere (regex fa già sta roba per la maggior parte)

            CreaTabella(nRighe, nColonne);
        }

        private void CreaTabella(int nRighe, int nColonne)
        {
            mainGrid.Visibility = Visibility.Visible;

            // Pulisci i log
            PulisciLog();

            // Salva le dimensioni (la tabella rimane invariata anche se si modificano le textbox)
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
            var listaRighe = new List<ExpandoObject>(); // da ristutturare con array bidimensionale già pronto sopra
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
            this.btnAutofill.Visibility = Visibility.Visible;
        }

        private void rowTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Regex.IsMatch(rowTextBox.Text, @"[^0-9]"))
            {
                rowTextBox.Text = Regex.Replace(rowTextBox.Text, @"[^0-9]", "");
            }
        }

        private void colTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Regex.IsMatch(colTextBox.Text, @"[^0-9]"))
            {
                colTextBox.Text = Regex.Replace(colTextBox.Text, @"[^0-9]", "");
            }
        }

        /*private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }*/

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
            variabiliReset();
            RiempiTabellaAutomaticamente();
        }

        private void variabiliReset()
        {
            costiTrasporto = new int[nRigheCorrente, nColonneCorrente];
            offerte = new int[nRigheCorrente];
            domande = new int[nColonneCorrente];
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

            // Calcola il range necessario per le offerte in base al numero di domande
            int domandaMinimaPerColonna = 50;
            int totaleMinimoDomande = nColonneCorrente * domandaMinimaPerColonna;

            // Calcola il minimo necessario per offerta per garantire il bilanciamento
            int minOffertaCalcolato = (int)Math.Ceiling((double)totaleMinimoDomande / nRigheCorrente);

            // Usa il massimo tra 50 e il minimo calcolato per garantire bilanciamento
            int minOfferta = Math.Max(50, minOffertaCalcolato);
            int maxOfferta = Math.Max(200, minOfferta + 150);

            // Genera offerte casuali con range adattato
            int[] offerteTemp = new int[nRigheCorrente];
            int totaleOfferte = 0;
            for (int r = 0; r < nRigheCorrente; r++)
            {
                offerteTemp[r] = random.Next(minOfferta, maxOfferta + 1);
                totaleOfferte += offerteTemp[r];
            }

            // NUOVO APPROCCIO: distribuisci le domande in modo più equilibrato
            int[] demandeTemp = new int[nColonneCorrente];

            // Inizializza tutte le domande al minimo
            for (int c = 0; c < nColonneCorrente; c++)
            {
                demandeTemp[c] = domandaMinimaPerColonna;
            }

            // Calcola quanto "extra" abbiamo da distribuire
            int extra = totaleOfferte - (nColonneCorrente * domandaMinimaPerColonna);

            // Distribuisci l'extra in modo casuale tra le colonne
            while (extra > 0)
            {
                int colonnaScelta = random.Next(0, nColonneCorrente);
                int quantitaDaAggiungere = random.Next(1, Math.Min(extra + 1, 51)); // Aggiungi max 50 alla volta
                demandeTemp[colonnaScelta] += quantitaDaAggiungere;
                extra -= quantitaDaAggiungere;
            }

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
            logCalcoli.AppendLine("═══════════════════════════════════════════");
            logCalcoli.AppendLine("   METODO NORD-OVEST");
            logCalcoli.AppendLine("═══════════════════════════════════════════\n");

            // Mostra stato iniziale
            logCalcoli.AppendLine(" STATO INIZIALE:");
            logCalcoli.AppendLine("-------------------");
            for (int x = 0; x < nRigheCorrente; x++)
                logCalcoli.AppendLine($"  Offerta UP{x + 1}: {offerte[x]}");
            logCalcoli.AppendLine("");
            for (int z = 0; z < nColonneCorrente; z++)
                logCalcoli.AppendLine($"  Domanda D{z + 1}: {domande[z]}");
            logCalcoli.AppendLine("\n");

            int[,] allocazioni = new int[nRigheCorrente, nColonneCorrente];
            int[] offerteRimanenti = (int[])offerte.Clone();
            int[] domandeRimanenti = (int[])domande.Clone();

            int i = 0, j = 0;
            int passo = 1;

            // Algoritmo Nord-Ovest
            logCalcoli.AppendLine(" ESECUZIONE ALGORITMO:");
            logCalcoli.AppendLine("========================\n");

            while (i < nRigheCorrente && j < nColonneCorrente)
            {
                int quantita = Math.Min(offerteRimanenti[i], domandeRimanenti[j]);
                allocazioni[i, j] = quantita;

                logCalcoli.AppendLine($"Passo {passo}:");
                logCalcoli.AppendLine($"  Cella [UP{i + 1}, D{j + 1}]");
                logCalcoli.AppendLine($"  Offerta rimanente: {offerteRimanenti[i]}");
                logCalcoli.AppendLine($"  Domanda rimanente: {domandeRimanenti[j]}");
                logCalcoli.AppendLine($"  Quantità allocata: {quantita}");
                logCalcoli.AppendLine($"  Costo unitario: {costiTrasporto[i, j]}");
                logCalcoli.AppendLine($"  Costo parziale: {costiTrasporto[i, j]} × {quantita} = {costiTrasporto[i, j] * quantita}");

                offerteRimanenti[i] -= quantita;
                domandeRimanenti[j] -= quantita;

                if (offerteRimanenti[i] == 0)
                {
                    logCalcoli.AppendLine($"  -> Offerta UP{i + 1} ESAURITA, avanzo alla riga successiva\n");
                    i++;
                }
                else
                {
                    logCalcoli.AppendLine($"  -> Domanda D{j + 1} SODDISFATTA, avanzo alla colonna successiva\n");
                    j++;
                }

                passo++;
            }

            MostraRisultato(allocazioni, "Metodo Nord-Ovest");
        }

        private void ApplicaMetodoMinimiCosti()
        {
            PulisciLog();
            logCalcoli.AppendLine("═══════════════════════════════════════════");
            logCalcoli.AppendLine("   METODO MINIMI COSTI");
            logCalcoli.AppendLine("═══════════════════════════════════════════\n");

            // Mostra stato iniziale
            logCalcoli.AppendLine(" STATO INIZIALE:");
            logCalcoli.AppendLine("-------------------");
            for (int i = 0; i < nRigheCorrente; i++)
                logCalcoli.AppendLine($"  Offerta UP{i + 1}: {offerte[i]}");
            logCalcoli.AppendLine("");
            for (int j = 0; j < nColonneCorrente; j++)
                logCalcoli.AppendLine($"  Domanda D{j + 1}: {domande[j]}");
            logCalcoli.AppendLine("\n");

            logCalcoli.AppendLine(" MATRICE DEI COSTI:");
            logCalcoli.AppendLine("---------------------");
            StringBuilder matriceCosti = new StringBuilder("     ");
            for (int j = 0; j < nColonneCorrente; j++)
                matriceCosti.Append($"D{j + 1}".PadRight(6));
            logCalcoli.AppendLine(matriceCosti.ToString());

            for (int i = 0; i < nRigheCorrente; i++)
            {
                StringBuilder riga = new StringBuilder($"UP{i + 1}".PadRight(5));
                for (int j = 0; j < nColonneCorrente; j++)
                {
                    riga.Append(costiTrasporto[i, j].ToString().PadRight(6));
                }
                logCalcoli.AppendLine(riga.ToString());
            }
            logCalcoli.AppendLine("\n");

            int[,] allocazioni = new int[nRigheCorrente, nColonneCorrente];
            int[] offerteRimanenti = (int[])offerte.Clone();
            int[] demandeRimanenti = (int[])domande.Clone();
            bool[,] usato = new bool[nRigheCorrente, nColonneCorrente];

            int passo = 1;

            // Algoritmo Minimi Costi
            logCalcoli.AppendLine(" ESECUZIONE ALGORITMO:");
            logCalcoli.AppendLine("========================\n");

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

                logCalcoli.AppendLine($"Passo {passo}:");
                logCalcoli.AppendLine($"  Costo minimo trovato: {minCosto}");
                logCalcoli.AppendLine($"  Cella [UP{minI + 1}, D{minJ + 1}]");
                logCalcoli.AppendLine($"  Offerta rimanente UP{minI + 1}: {offerteRimanenti[minI]}");
                logCalcoli.AppendLine($"  Domanda rimanente D{minJ + 1}: {demandeRimanenti[minJ]}");
                logCalcoli.AppendLine($"  Quantità allocata: {quantita}");
                logCalcoli.AppendLine($"  Costo parziale: {minCosto} × {quantita} = {minCosto * quantita}");

                offerteRimanenti[minI] -= quantita;
                demandeRimanenti[minJ] -= quantita;

                // Marca come usata se offerta o domanda è esaurita
                if (offerteRimanenti[minI] == 0 || demandeRimanenti[minJ] == 0)
                {
                    usato[minI, minJ] = true;
                    if (offerteRimanenti[minI] == 0)
                        logCalcoli.AppendLine($"  -> Offerta UP{minI + 1} ESAURITA");
                    if (demandeRimanenti[minJ] == 0)
                        logCalcoli.AppendLine($"  -> Domanda D{minJ + 1} SODDISFATTA");
                }

                logCalcoli.AppendLine("");
                passo++;
            }

            MostraRisultato(allocazioni, "Metodo Minimi Costi");
        }

        private void MostraRisultato(int[,] allocazioni, string metodo)
        {
            var items = mainGrid.ItemsSource as List<ExpandoObject>;
            if (items == null) return;

            int costoTotale = 0;

            logCalcoli.AppendLine("\n═══════════════════════════════════════════");
            logCalcoli.AppendLine("   RISULTATO FINALE");
            logCalcoli.AppendLine("═══════════════════════════════════════════\n");

            logCalcoli.AppendLine(" ALLOCAZIONI:");
            logCalcoli.AppendLine("--------------");

            // Aggiorna la tabella con le allocazioni
            for (int r = 0; r < nRigheCorrente; r++)
            {
                var dict = (IDictionary<string, object>)items[r];
                bool haAllocazioni = false;

                for (int c = 0; c < nColonneCorrente; c++)
                {
                    if (allocazioni[r, c] > 0)
                    {
                        dict[$"D{c + 1}"] = $"{costiTrasporto[r, c]}";
                        int costoParzialeRiga = costiTrasporto[r, c] * allocazioni[r, c];
                        costoTotale += costoParzialeRiga;

                        if (!haAllocazioni)
                        {
                            logCalcoli.AppendLine($"\n  UP{r + 1}:");
                            haAllocazioni = true;
                        }
                        logCalcoli.AppendLine($"    -> D{c + 1}: {allocazioni[r, c]} unità a costo {costiTrasporto[r, c]} = {costoParzialeRiga}");
                    }
                    else
                    {
                        dict[$"D{c + 1}"] = costiTrasporto[r, c].ToString();
                    }
                }
            }

            logCalcoli.AppendLine("\n");
            logCalcoli.AppendLine("═══════════════════════════════════════════");
            logCalcoli.AppendLine($" COSTO TOTALE DI TRASPORTO: {costoTotale}");
            logCalcoli.AppendLine("═══════════════════════════════════════════");

            txtCalcoli.Text = logCalcoli.ToString(); // mette tutto lo stringbuilder nella textbox

            mainGrid.Items.Refresh();

            /*MessageBox.Show($"{metodo} applicato con successo!\n\nCosto totale di trasporto: {costoTotale}",
                           "Risultato",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);*/
        }
    }
}