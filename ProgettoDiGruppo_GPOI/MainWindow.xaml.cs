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
        // Variabili di istanza per la gestione del problema di trasporto
        /// <summary>Matrice bidimensionale che contiene i costi di trasporto tra fornitori (righe) e destinazioni (colonne)</summary>
        private int[,] costiTrasporto;
        
        /// <summary>Array che contiene le quantità disponibili (offerte) per ogni fornitore</summary>
        private int[] offerte;
        
        /// <summary>Array che contiene le quantità richieste (domande) per ogni destinazione</summary>
        private int[] domande;
        
        /// <summary>Memorizza il numero di righe della tabella corrente (numero di fornitori)</summary>
        private int nRigheCorrente;
        
        /// <summary>Memorizza il numero di colonne della tabella corrente (numero di destinazioni)</summary>
        private int nColonneCorrente;
        
        /// <summary>StringBuilder che accumula i log dei calcoli per visualizzarli nell'interfaccia</summary>
        private StringBuilder logCalcoli;

        /// <summary>
        /// Costruttore della finestra principale
        /// Inizializza i componenti e configura le proprietà della finestra al caricamento
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Evento che si scatena quando la finestra è completamente caricata
            this.Loaded += (s, e) =>
            {
                // Imposta il titolo della finestra
                this.Title = "Lavoro di gruppo - GPOI";
                
                // Massimizza la finestra al caricamento
                this.WindowState = WindowState.Maximized;
                
                // Imposta il font della finestra
                this.FontFamily = new FontFamily("Segoe UI");
                this.FontSize = 14;
                this.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));

                // Ottiene le dimensioni dello schermo primario e le applica alla finestra
                this.Height = (SystemParameters.PrimaryScreenHeight);
                this.Width = (SystemParameters.PrimaryScreenWidth);

                // Calcola l'altezza della griglia superiore come il 7% dell'altezza totale della finestra
                float topGridHeight = 0.07f;
                this.TopGrid.Height = this.Height * topGridHeight;
                
                // Assegna alla griglia sinistra l'altezza rimanente
                this.leftGrid.Height = this.Height * (1 - topGridHeight);
                
                // Posiziona la griglia sinistra sotto la griglia superiore
                this.leftGrid.Margin = new Thickness(0, this.TopGrid.Height - 1, 0, 0);
                
                // Posiziona la griglia principale a destra della griglia sinistra e sotto quella superiore
                this.mainGrid.Margin = new Thickness(this.leftGrid.Width, this.TopGrid.Height, 0, 0);
                
                // Posiziona il pannello destro in alto a destra
                this.rightPanel.Margin = new Thickness(0, this.TopGrid.Height, 0, 0);
            };
        }

        /// <summary>
        /// Pulisce il log dei calcoli azzerando il StringBuilder e la TextBox
        /// Viene chiamata all'inizio di ogni esecuzione di un algoritmo
        /// </summary>
        private void PulisciLog()
        {
            // Inizializza un nuovo StringBuilder vuoto per accumulare i log
            logCalcoli = new StringBuilder();
            
            // Svuota il contenuto della TextBox destinata ai log
            txtCalcoli.Text = "";
        }

        /// <summary>
        /// Gestore dell'evento click del pulsante "Crea Tabella"
        /// Valida gli input dell'utente e crea una nuova tabella
        /// </summary>
        /// <param name="sender">Oggetto che ha generato l'evento (il pulsante)</param>
        /// <param name="e">Argomenti dell'evento RoutedEventArgs</param>
        private void btnCreaTabella_Click(object sender, RoutedEventArgs e)
        {
            // Tenta di convertire il testo da stringhe a interi per il numero di righe
            if (!int.TryParse(rowTextBox.Text, out int nRighe) || nRighe <= 0)
            {
                MessageBox.Show("Numero di righe non valido!");
                return;
            }

            // Tenta di convertire il testo da stringhe a interi per il numero di colonne
            if (!int.TryParse(colTextBox.Text, out int nColonne) || nColonne <= 0)
            {
                MessageBox.Show("Numero di colonne non valido!");
                return;
            }

            // Chiama la funzione per creare la tabella con le dimensioni validate
            CreaTabella(nRighe, nColonne);
        }

        /// <summary>
        /// Crea una tabella dinamica con il numero specificato di righe e colonne
        /// Inizializza tutti gli array e configura la DataGrid per visualizzare i dati
        /// </summary>
        /// <param name="nRighe">Numero di fornitori (righe della tabella)</param>
        /// <param name="nColonne">Numero di destinazioni (colonne della tabella)</param>
        private void CreaTabella(int nRighe, int nColonne)
        {
            // Pulisce i log precedenti
            PulisciLog();

            // Memorizza le dimensioni della tabella nel contesto della finestra
            // così rimangono uguali anche se l'utente modifica le TextBox
            nRigheCorrente = nRighe;
            nColonneCorrente = nColonne;

            // Inizializza l'array bidimensionale dei costi di trasporto
            costiTrasporto = new int[nRighe, nColonne];
            
            // Inizializza l'array delle offerte con una cella per ogni fornitore
            offerte = new int[nRighe];
            
            // Inizializza l'array delle domande con una cella per ogni destinazione
            domande = new int[nColonne];

            // Rimuove tutte le colonne precedentemente aggiunte alla DataGrid
            mainGrid.Columns.Clear();

            // Aggiunge la prima colonna (senza header) che contiene le intestazioni delle righe
            mainGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "",
                Binding = new Binding("Riga"),  // Legge il valore dalla proprietà "Riga" dell'oggetto dinamico
                IsReadOnly = true                // La colonna non è modificabile
            });

            // Crea gli header delle colonne dinamiche (D1, D2, … Dn)
            string[] intestazioniColonne = new string[nColonne + 1];
            for (int c = 0; c < nColonne; c++)
            {
                // Assegna il nome dinamico alla colonna (D1, D2, ecc.)
                intestazioniColonne[c] = $"D{c + 1}";
                
                // Aggiunge una colonna alla DataGrid con il binding alla proprietà corrispondente
                mainGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = intestazioniColonne[c],
                    Binding = new Binding(intestazioniColonne[c]),  // Binding al nome colonna dinamico
                    IsReadOnly = false                              // La colonna è modificabile
                });
            }
            
            // Assegna il nome "Offerta" all'ultima colonna dell'array
            intestazioniColonne[nColonne] = "Offerta";
            
            // Aggiunge la colonna "Offerta" alla DataGrid
            mainGrid.Columns.Add(new DataGridTextColumn
            {
                Header = intestazioniColonne[nColonne],
                Binding = new Binding(intestazioniColonne[nColonne]),  // Binding alla proprietà "Offerta"
                IsReadOnly = false                                      // Modificabile
            });

            // Crea le righe dinamiche della tabella per i fornitori (UP1, UP2, …)
            var listaRighe = new List<ExpandoObject>();
            
            // Ciclo per creare una riga per ogni fornitore
            for (int r = 0; r < nRighe; r++)
            {
                // Crea un oggetto dinamico che può avere proprietà aggiunte al runtime
                dynamic row = new ExpandoObject();
                
                // Ottiene il dizionario sottostante dell'oggetto dinamico per manipolare le proprietà
                var dict = (IDictionary<string, object>)row;

                // Aggiunge la proprietà "Riga" con il nome del fornitore (UP1, UP2, ecc.)
                dict["Riga"] = $"UP{r + 1}";

                // Aggiunge una proprietà nel dizionario per ogni intestazione di colonna
                // Inizialmente tutte le celle sono vuote (stringhe vuote)
                foreach (var col in intestazioniColonne)
                {
                    dict[col] = "";
                }

                // Aggiunge l'oggetto dinamico completato alla lista
                listaRighe.Add(row);
            }
            
            // Crea una riga speciale per le domande (l'ultima riga della tabella)
            dynamic row1 = new ExpandoObject();
            
            // Ottiene il dizionario sottostante della riga delle domande
            var dict1 = (IDictionary<string, object>)row1;

            // Assegna l'etichetta "Domanda" alla prima colonna
            dict1["Riga"] = "Domanda";

            // Inizializza tutte le celle della riga con stringhe vuote
            foreach (var col in intestazioniColonne)
            {
                dict1[col] = "";
            }

            // Aggiunge la riga delle domande alla lista
            listaRighe.Add(row1);

            // Assegna la lista di oggetti dinamici come sorgente dati della DataGrid
            mainGrid.ItemsSource = listaRighe;

            // Abilita tutti i pulsanti degli algoritmi e le funzioni ausiliarie
            btnNordOvest.IsEnabled = true;
            btnMinimiCosti.IsEnabled = true;
            btnReset.IsEnabled = true;
            btnAutofill.IsEnabled = true;
            
            // Rende visibile il pulsante di autofill
            this.btnAutofill.Visibility = Visibility.Visible;
            
            // Rende visibile la griglia principale
            mainGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Gestore dell'evento TextChanged per la TextBox del numero di righe
        /// Consente solo l'inserimento di cifre, eliminando automaticamente i caratteri non numerici
        /// </summary>
        /// <param name="sender">La TextBox che ha generato l'evento</param>
        /// <param name="e">Argomenti dell'evento TextChangedEventArgs</param>
        private void rowTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Verifica se il testo contiene caratteri che non sono cifre
            if (Regex.IsMatch(rowTextBox.Text, @"[^0-9]"))
            {
                // Rimuove tutti i caratteri non numerici dal testo
                rowTextBox.Text = Regex.Replace(rowTextBox.Text, @"[^0-9]", "");
            }
        }

        /// <summary>
        /// Gestore dell'evento TextChanged per la TextBox del numero di colonne
        /// Consente solo l'inserimento di cifre, eliminando automaticamente i caratteri non numerici
        /// </summary>
        /// <param name="sender">La TextBox che ha generato l'evento</param>
        /// <param name="e">Argomenti dell'evento TextChangedEventArgs</param>
        private void colTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Verifica se il testo contiene caratteri che non sono cifre
            if (Regex.IsMatch(colTextBox.Text, @"[^0-9]"))
            {
                // Rimuove tutti i caratteri non numerici dal testo
                colTextBox.Text = Regex.Replace(colTextBox.Text, @"[^0-9]", "");
            }
        }

        /// <summary>
        /// Gestore dell'evento click del pulsante "Metodo Nord-Ovest"
        /// Valida i dati della tabella, verifica il bilanciamento e applica l'algoritmo
        /// </summary>
        /// <param name="sender">Oggetto che ha generato l'evento (il pulsante)</param>
        /// <param name="e">Argomenti dell'evento RoutedEventArgs</param>
        private void btnNordOvest_Click(object sender, RoutedEventArgs e)
        {
            // Tenta di leggere i dati dalla tabella negli array
            if (!LeggiDatiTabella())
            {
                MessageBox.Show("Inserisci tutti i costi, le offerte e le domande prima di applicare l'algoritmo!");
                return;
            }

            // Verifica che la somma delle offerte sia uguale alla somma delle domande
            if (!VerificaBilanciamento())
            {
                MessageBox.Show("Problema non bilanciato! La somma delle offerte deve essere uguale alla somma delle domande.");
                return;
            }

            // Se tutti i controlli sono passati, applica l'algoritmo Nord-Ovest
            ApplicaMetodoNordOvest();
        }

        /// <summary>
        /// Gestore dell'evento click del pulsante "Metodo Minimi Costi"
        /// Valida i dati della tabella, verifica il bilanciamento e applica l'algoritmo
        /// </summary>
        /// <param name="sender">Oggetto che ha generato l'evento (il pulsante)</param>
        /// <param name="e">Argomenti dell'evento RoutedEventArgs</param>
        private void btnMinimiCosti_Click(object sender, RoutedEventArgs e)
        {
            // Tenta di leggere i dati dalla tabella negli array
            if (!LeggiDatiTabella())
            {
                MessageBox.Show("Inserisci tutti i costi, le offerte e le domande prima di applicare l'algoritmo!");
                return;
            }

            // Verifica che la somma delle offerte sia uguale alla somma delle domande
            if (!VerificaBilanciamento())
            {
                MessageBox.Show("Problema non bilanciato! La somma delle offerte deve essere uguale alla somma delle domande.");
                return;
            }

            // Se tutti i controlli sono passati, applica l'algoritmo Minimi Costi
            ApplicaMetodoMinimiCosti();
        }

        /// <summary>
        /// Gestore dell'evento click del pulsante "Reset"
        /// Ricrea la tabella svuotando tutti i dati inseriti
        /// </summary>
        /// <param name="sender">Oggetto che ha generato l'evento (il pulsante)</param>
        /// <param name="e">Argomenti dell'evento RoutedEventArgs</param>
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            // Ricrea la tabella mantenendo le stesse dimensioni
            CreaTabella(nRigheCorrente, nColonneCorrente);
        }

        /// <summary>
        /// Gestore dell'evento click del pulsante "Autofill"
        /// Riempie la tabella automaticamente con valori casuali realistici
        /// </summary>
        /// <param name="sender">Oggetto che ha generato l'evento (il pulsante)</param>
        /// <param name="e">Argomenti dell'evento RoutedEventArgs</param>
        private void btnAutofill_Click(object sender, RoutedEventArgs e)
        {
            // Resetta le variabili per prepararsi al riempimento automatico
            variabiliReset();
            
            // Riempie la tabella con dati casuali
            RiempiTabellaAutomaticamente();
        }

        /// <summary>
        /// Reinizializza tutti gli array principali
        /// Viene usata prima di riempire automaticamente la tabella
        /// </summary>
        private void variabiliReset()
        {
            // Crea nuovi array vuoti per i costi di trasporto
            costiTrasporto = new int[nRigheCorrente, nColonneCorrente];
            
            // Crea un nuovo array vuoto per le offerte
            offerte = new int[nRigheCorrente];
            
            // Crea un nuovo array vuoto per le domande
            domande = new int[nColonneCorrente];
        }

        /// <summary>
        /// Riempie la tabella automaticamente con dati casuali realistici
        /// Genera costi casuali, offerte e domande garantendo il bilanciamento
        /// </summary>
        private void RiempiTabellaAutomaticamente()
        {
            // Ottiene la lista di oggetti dinamici dalla sorgente dati della DataGrid
            var items = mainGrid.ItemsSource as List<ExpandoObject>;
            if (items == null) return;

            // Crea un'istanza di Random per generare numeri casuali
            Random random = new Random();

            // ===== FASE 1: Generazione dei costi di trasporto =====
            // Ciclo per ogni fornitore (riga)
            for (int r = 0; r < nRigheCorrente; r++)
            {
                // Ottiene il dizionario della riga corrente
                var dict = (IDictionary<string, object>)items[r];
                
                // Ciclo per ogni destinazione (colonna)
                for (int c = 0; c < nColonneCorrente; c++)
                {
                    // Aggiunge al dizionario un costo casuale tra 1 e 50
                    dict[$"D{c + 1}"] = random.Next(1, 51).ToString();
                }
            }

            // ===== FASE 2: Calcolo dei parametri per le offerte =====
            // Definisce il minimo di domanda richiesto per ogni destinazione
            int domandaMinimaPerColonna = 50;
            
            // Calcola il totale minimo di domande per tutte le destinazioni
            int totaleMinimoDomande = nColonneCorrente * domandaMinimaPerColonna;

            // Calcola l'offerta minima necessaria per garantire il bilanciamento
            // Divide il totale minimo per il numero di fornitori e arrotonda per eccesso
            int minOffertaCalcolato = (int)Math.Ceiling((double)totaleMinimoDomande / nRigheCorrente);

            // Usa il massimo tra 50 e il minimo calcolato
            int minOfferta = Math.Max(50, minOffertaCalcolato);
            
            // Definisce il massimo dell'offerta come minOfferta + 150, ma almeno 200
            int maxOfferta = Math.Max(200, minOfferta + 150);

            // ===== FASE 3: Generazione delle offerte casuali =====
            // Crea un array temporaneo per le offerte
            int[] offerteTemp = new int[nRigheCorrente];
            
            // Variabile per accumulare il totale delle offerte
            int totaleOfferte = 0;
            
            // Ciclo per ogni fornitore
            for (int r = 0; r < nRigheCorrente; r++)
            {
                // Genera un'offerta casuale nel range calcolato
                offerteTemp[r] = random.Next(minOfferta, maxOfferta + 1);
                
                // Accumula il totale
                totaleOfferte += offerteTemp[r];
            }

            // ===== FASE 4: Generazione delle domande bilanciate =====
            // Crea un array temporaneo per le domande
            int[] demandeTemp = new int[nColonneCorrente];

            // Inizializza tutte le domande al minimo stabilito
            for (int c = 0; c < nColonneCorrente; c++)
            {
                demandeTemp[c] = domandaMinimaPerColonna;
            }

            // Calcola quanto "extra" abbiamo da distribuire
            // (la differenza tra le offerte totali e il totale minimo delle domande)
            int extra = totaleOfferte - (nColonneCorrente * domandaMinimaPerColonna);

            // Distribuisce l'extra in modo casuale tra le colonne
            while (extra > 0)
            {
                // Sceglie casualmente una colonna
                int colonnaScelta = random.Next(0, nColonneCorrente);
                
                // Genera una quantità casuale da aggiungere (max 50 alla volta)
                int quantitaDaAggiungere = random.Next(1, Math.Min(extra + 1, 51));
                
                // Aggiunge la quantità alla domanda della colonna scelta
                demandeTemp[colonnaScelta] += quantitaDaAggiungere;
                
                // Decrementa l'extra rimanente
                extra -= quantitaDaAggiungere;
            }

            // ===== FASE 5: Inserimento dei dati nella tabella =====
            // Ciclo per ogni fornitore
            for (int r = 0; r < nRigheCorrente; r++)
            {
                // Ottiene il dizionario della riga corrente
                var dict = (IDictionary<string, object>)items[r];
                
                // Aggiunge l'offerta al dizionario della riga
                dict["Offerta"] = offerteTemp[r].ToString();
            }

            // Ottiene il dizionario dell'ultima riga (riga delle domande)
            var dictDomande = (IDictionary<string, object>)items[nRigheCorrente];
            
            // Ciclo per ogni destinazione
            for (int c = 0; c < nColonneCorrente; c++)
            {
                // Aggiunge la domanda al dizionario della riga delle domande
                dictDomande[$"D{c + 1}"] = demandeTemp[c].ToString();
            }
            
            // Lascia vuota la cella nell'angolo in basso a destra (intersezione domande-offerta)
            dictDomande["Offerta"] = "";

            // Aggiorna la visualizzazione della DataGrid
            mainGrid.Items.Refresh();
        }

        /// <summary>
        /// Legge i dati dalla tabella e li memorizza negli array di classe
        /// Restituisce true se la lettura è riuscita, false se ci sono errori di formattazione
        /// </summary>
        /// <returns>True se tutti i dati sono validi, false altrimenti</returns>
        private bool LeggiDatiTabella()
        {
            try
            {
                // Ottiene la lista di oggetti dinamici dalla sorgente dati della DataGrid
                var items = mainGrid.ItemsSource as List<ExpandoObject>;
                if (items == null) return false;

                // ===== FASE 1: Lettura dei costi e delle offerte =====
                // Ciclo per ogni fornitore (tutte le righe tranne l'ultima)
                for (int r = 0; r < nRigheCorrente; r++)
                {
                    // Ottiene il dizionario della riga corrente
                    var dict = (IDictionary<string, object>)items[r];

                    // Tenta di leggere l'offerta e convertirla in intero
                    if (!int.TryParse(dict["Offerta"]?.ToString(), out offerte[r]))
                        return false;

                    // Ciclo per ogni destinazione (ogni colonna di costi)
                    for (int c = 0; c < nColonneCorrente; c++)
                    {
                        // Tenta di leggere il costo e convertirlo in intero
                        if (!int.TryParse(dict[$"D{c + 1}"]?.ToString(), out costiTrasporto[r, c]))
                            return false;
                    }
                }

                // ===== FASE 2: Lettura delle domande =====
                // Ottiene il dizionario dell'ultima riga (riga delle domande)
                var dictDomande = (IDictionary<string, object>)items[nRigheCorrente];
                
                // Ciclo per ogni destinazione
                for (int c = 0; c < nColonneCorrente; c++)
                {
                    // Tenta di leggere la domanda e convertirla in intero
                    if (!int.TryParse(dictDomande[$"D{c + 1}"]?.ToString(), out domande[c]))
                        return false;
                }

                // Se siamo arrivati qui, tutti i dati sono validi
                return true;
            }
            catch
            {
                // Se si verifica un'eccezione, restituisce false
                return false;
            }
        }

        /// <summary>
        /// Verifica che il problema sia bilanciato
        /// Controlla che la somma delle offerte sia uguale alla somma delle domande
        /// </summary>
        /// <returns>True se il problema è bilanciato, false altrimenti</returns>
        private bool VerificaBilanciamento()
        {
            // Inizializza la variabile per accumulare il totale delle offerte
            int totaleOfferte = 0;
            
            // Inizializza la variabile per accumulare il totale delle domande
            int totaleDomande = 0;

            // Somma tutti i valori dell'array delle offerte
            for (int i = 0; i < nRigheCorrente; i++)
                totaleOfferte += offerte[i];

            // Somma tutti i valori dell'array delle domande
            for (int i = 0; i < nColonneCorrente; i++)
                totaleDomande += domande[i];

            // Restituisce true solo se i due totali sono uguali
            return totaleOfferte == totaleDomande;
        }

        /// <summary>
        /// Applica il metodo Nord-Ovest per risolvere il problema di trasporto
        /// Alloca i trasporti partendo dalla cella nord-ovest (alto-sinistra) della tabella
        /// </summary>
        private void ApplicaMetodoNordOvest()
        {
            // Pulisce i log precedenti
            PulisciLog();
            
            // Aggiunge l'intestazione del metodo ai log
            logCalcoli.AppendLine("═══════════════════════════════════════════");
            logCalcoli.AppendLine("   METODO NORD-OVEST");
            logCalcoli.AppendLine("═══════════════════════════════════════════\n");

            // ===== FASE 1: Visualizzazione dello stato iniziale =====
            logCalcoli.AppendLine(" STATO INIZIALE:");
            logCalcoli.AppendLine("-------------------");
            
            // Elenca tutte le offerte
            for (int x = 0; x < nRigheCorrente; x++)
                logCalcoli.AppendLine($"  Offerta UP{x + 1}: {offerte[x]}");
            logCalcoli.AppendLine("");
            
            // Elenca tutte le domande
            for (int z = 0; z < nColonneCorrente; z++)
                logCalcoli.AppendLine($"  Domanda D{z + 1}: {domande[z]}");
            logCalcoli.AppendLine("\n");

            // ===== FASE 2: Inizializzazione delle variabili di lavoro =====
            // Crea una matrice per memorizzare le allocazioni effettuate
            int[,] allocazioni = new int[nRigheCorrente, nColonneCorrente];
            
            // Crea copie degli array di offerte e domande (saranno decrementate durante l'algoritmo)
            int[] offerteRimanenti = (int[])offerte.Clone();
            int[] domandeRimanenti = (int[])domande.Clone();

            // Inizializza i puntatori per la posizione della cella nord-ovest
            int i = 0, j = 0;
            
            // Variabile contatore per i passi dell'algoritmo
            int passo = 1;

            // ===== FASE 3: Esecuzione dell'algoritmo =====
            logCalcoli.AppendLine(" ESECUZIONE ALGORITMO:");
            logCalcoli.AppendLine("========================\n");

            // Continua finché ci sono ancora fornitori e destinazioni da processare
            while (i < nRigheCorrente && j < nColonneCorrente)
            {
                // Alloca la quantità minima tra l'offerta rimanente e la domanda rimanente
                int quantita = Math.Min(offerteRimanenti[i], domandeRimanenti[j]);
                
                // Registra l'allocazione nella matrice
                allocazioni[i, j] = quantita;

                // ===== Log dettagliato del passo corrente =====
                logCalcoli.AppendLine($"Passo {passo}:");
                logCalcoli.AppendLine($"  Cella [UP{i + 1}, D{j + 1}]");
                logCalcoli.AppendLine($"  Offerta rimanente: {offerteRimanenti[i]}");
                logCalcoli.AppendLine($"  Domanda rimanente: {domandeRimanenti[j]}");
                logCalcoli.AppendLine($"  Quantità allocata: {quantita}");
                logCalcoli.AppendLine($"  Costo unitario: {costiTrasporto[i, j]}");
                logCalcoli.AppendLine($"  Costo parziale: {costiTrasporto[i, j]} × {quantita} = {costiTrasporto[i, j] * quantita}");

                // Decrementa l'offerta rimanente del fornitore
                offerteRimanenti[i] -= quantita;
                
                // Decrementa la domanda rimanente della destinazione
                domandeRimanenti[j] -= quantita;

                // ===== Logica di avanzamento nel prossimo passo =====
                if (offerteRimanenti[i] == 0)
                {
                    // Se l'offerta è esaurita, passa al fornitore successivo
                    logCalcoli.AppendLine($"  -> Offerta UP{i + 1} ESAURITA, avanzo alla riga successiva\n");
                    i++;
                }
                else
                {
                    // Se la domanda è soddisfatta, passa alla destinazione successiva
                    logCalcoli.AppendLine($"  -> Domanda D{j + 1} SODDISFATTA, avanzo alla colonna successiva\n");
                    j++;
                }

                // Incrementa il contatore dei passi
                passo++;
            }

            // Mostra il risultato finale sullo schermo
            MostraRisultato(allocazioni, "Metodo Nord-Ovest");
        }

        /// <summary>
        /// Applica il metodo dei Minimi Costi per risolvere il problema di trasporto
        /// Alloca i trasporti scegliendo sempre la cella con il costo minore disponibile
        /// </summary>
        private void ApplicaMetodoMinimiCosti()
        {
            // Pulisce i log precedenti
            PulisciLog();
            
            // Aggiunge l'intestazione del metodo ai log
            logCalcoli.AppendLine("═══════════════════════════════════════════");
            logCalcoli.AppendLine("   METODO MINIMI COSTI");
            logCalcoli.AppendLine("═══════════════════════════════════════════\n");

            // ===== FASE 1: Visualizzazione dello stato iniziale =====
            logCalcoli.AppendLine(" STATO INIZIALE:");
            logCalcoli.AppendLine("-------------------");
            
            // Elenca tutte le offerte
            for (int i = 0; i < nRigheCorrente; i++)
                logCalcoli.AppendLine($"  Offerta UP{i + 1}: {offerte[i]}");
            logCalcoli.AppendLine("");
            
            // Elenca tutte le domande
            for (int j = 0; j < nColonneCorrente; j++)
                logCalcoli.AppendLine($"  Domanda D{j + 1}: {domande[j]}");
            logCalcoli.AppendLine("\n");

            // ===== FASE 2: Visualizzazione della matrice dei costi =====
            logCalcoli.AppendLine(" MATRICE DEI COSTI:");
            logCalcoli.AppendLine("---------------------");
            
            // Crea l'intestazione delle colonne (D1, D2, ecc.)
            StringBuilder matriceCosti = new StringBuilder("     ");
            for (int j = 0; j < nColonneCorrente; j++)
                matriceCosti.Append($"D{j + 1}".PadRight(6));
            logCalcoli.AppendLine(matriceCosti.ToString());

            // Crea ogni riga della matrice dei costi
            for (int i = 0; i < nRigheCorrente; i++)
            {
                // Crea la riga con l'intestazione del fornitore
                StringBuilder riga = new StringBuilder($"UP{i + 1}".PadRight(5));
                
                // Aggiunge il costo di ogni cella
                for (int j = 0; j < nColonneCorrente; j++)
                {
                    riga.Append(costiTrasporto[i, j].ToString().PadRight(6));
                }
                logCalcoli.AppendLine(riga.ToString());
            }
            logCalcoli.AppendLine("\n");

            // ===== FASE 3: Inizializzazione delle variabili di lavoro =====
            // Crea una matrice per memorizzare le allocazioni effettuate
            int[,] allocazioni = new int[nRigheCorrente, nColonneCorrente];
            
            // Crea copie degli array di offerte e domande
            int[] offerteRimanenti = (int[])offerte.Clone();
            int[] demandeRimanenti = (int[])domande.Clone();
            
            // Matrice booleana per tracciare quali celle sono già state processate
            bool[,] usato = new bool[nRigheCorrente, nColonneCorrente];

            // Variabile contatore per i passi dell'algoritmo
            int passo = 1;

            // ===== FASE 4: Esecuzione dell'algoritmo =====
            logCalcoli.AppendLine(" ESECUZIONE ALGORITMO:");
            logCalcoli.AppendLine("========================\n");

            // Continua finché ci sono celle disponibili da processare
            while (true)
            {
                // ===== Ricerca della cella con costo minimo non utilizzata =====
                // Inizializza il costo minimo a un valore infinito
                int minCosto = int.MaxValue;
                
                // Inizializza gli indici della cella minima a -1 (non trovata)
                int minI = -1, minJ = -1;

                // Scansiona tutta la matrice
                for (int i = 0; i < nRigheCorrente; i++)
                {
                    for (int j = 0; j < nColonneCorrente; j++)
                    {
                        // Controlla se la cella è candidata per l'allocazione
                        if (!usato[i, j] && offerteRimanenti[i] > 0 && demandeRimanenti[j] > 0)
                        {
                            // Se il costo è minore del minimo trovato finora, aggiorna
                            if (costiTrasporto[i, j] < minCosto)
                            {
                                minCosto = costiTrasporto[i, j];
                                minI = i;
                                minJ = j;
                            }
                        }
                    }
                }

                // Se nessuna cella è stata trovata, l'algoritmo è terminato
                if (minI == -1) break;

                // ===== Allocazione nella cella con costo minimo =====
                // Alloca la quantità minima tra l'offerta rimanente e la domanda rimanente
                int quantita = Math.Min(offerteRimanenti[minI], demandeRimanenti[minJ]);
                
                // Registra l'allocazione nella matrice
                allocazioni[minI, minJ] = quantita;

                // ===== Log dettagliato del passo corrente =====
                logCalcoli.AppendLine($"Passo {passo}:");
                logCalcoli.AppendLine($"  Costo minimo trovato: {minCosto}");
                logCalcoli.AppendLine($"  Cella [UP{minI + 1}, D{minJ + 1}]");
                logCalcoli.AppendLine($"  Offerta rimanente UP{minI + 1}: {offerteRimanenti[minI]}");
                logCalcoli.AppendLine($"  Domanda rimanente D{minJ + 1}: {demandeRimanenti[minJ]}");
                logCalcoli.AppendLine($"  Quantità allocata: {quantita}");
                logCalcoli.AppendLine($"  Costo parziale: {minCosto} × {quantita} = {minCosto * quantita}");

                // Decrementa l'offerta rimanente del fornitore
                offerteRimanenti[minI] -= quantita;
                
                // Decrementa la domanda rimanente della destinazione
                demandeRimanenti[minJ] -= quantita;

                // ===== Marcatura della cella se esaurita =====
                // Se l'offerta o la domanda è stata completamente soddisfatta, marca la cella come usata
                if (offerteRimanenti[minI] == 0 || demandeRimanenti[minJ] == 0)
                {
                    usato[minI, minJ] = true;
                    
                    // Log del completamento
                    if (offerteRimanenti[minI] == 0)
                        logCalcoli.AppendLine($"  -> Offerta UP{minI + 1} ESAURITA");
                    if (demandeRimanenti[minJ] == 0)
                        logCalcoli.AppendLine($"  -> Domanda D{minJ + 1} SODDISFATTA");
                }

                logCalcoli.AppendLine("");
                
                // Incrementa il contatore dei passi
                passo++;
            }

            // Mostra il risultato finale sullo schermo
            MostraRisultato(allocazioni, "Metodo Minimi Costi");
        }

        /// <summary>
        /// Visualizza i risultati dell'algoritmo sulla tabella e nel log
        /// Aggiorna la DataGrid con i costi e le allocazioni, calcola il costo totale
        /// </summary>
        /// <param name="allocazioni">Matrice bidimensionale con le quantità allocate</param>
        /// <param name="metodo">Stringa che identifica quale metodo è stato usato</param>
        private void MostraRisultato(int[,] allocazioni, string metodo)
        {
            // Ottiene la lista di oggetti dinamici dalla sorgente dati della DataGrid
            var items = mainGrid.ItemsSource as List<ExpandoObject>;
            if (items == null) return;

            // Inizializza la variabile per accumulare il costo totale
            int costoTotale = 0;

            // ===== Intestazione della sezione dei risultati =====
            logCalcoli.AppendLine("\n═══════════════════════════════════════════");
            logCalcoli.AppendLine("   RISULTATO FINALE");
            logCalcoli.AppendLine("═══════════════════════════════════════════\n");

            logCalcoli.AppendLine(" ALLOCAZIONI:");
            logCalcoli.AppendLine("--------------");

            // ===== Aggiornamento della tabella con i risultati =====
            // Ciclo per ogni fornitore
            for (int r = 0; r < nRigheCorrente; r++)
            {
                // Ottiene il dizionario della riga corrente
                var dict = (IDictionary<string, object>)items[r];
                
                // Flag per tracciare se la riga ha allocazioni
                bool haAllocazioni = false;

                // Ciclo per ogni destinazione
                for (int c = 0; c < nColonneCorrente; c++)
                {
                    // Controlla se c'è un'allocazione in questa cella
                    if (allocazioni[r, c] > 0)
                    {
                        // Mostra il costo nella cella
                        dict[$"D{c + 1}"] = $"{costiTrasporto[r, c]}";
                        
                        // Calcola il costo parziale (costo unitario × quantità allocata)
                        int costoParzialeRiga = costiTrasporto[r, c] * allocazioni[r, c];
                        
                        // Aggiunge il costo parziale al costo totale
                        costoTotale += costoParzialeRiga;

                        // Se è la prima allocazione di questa riga, aggiunge l'intestazione
                        if (!haAllocazioni)
                        {
                            logCalcoli.AppendLine($"\n  UP{r + 1}:");
                            haAllocazioni = true;
                        }
                        
                        // Aggiunge il dettaglio dell'allocazione al log
                        logCalcoli.AppendLine($"    -> D{c + 1}: {allocazioni[r, c]} unità a costo {costiTrasporto[r, c]} = {costoParzialeRiga}");
                    }
                    else
                    {
                        // Se non c'è allocazione, mostra solo il costo
                        dict[$"D{c + 1}"] = costiTrasporto[r, c].ToString();
                    }
                }
            }

            // ===== Visualizzazione del costo totale =====
            logCalcoli.AppendLine("\n");
            logCalcoli.AppendLine("═══════════════════════════════════════════");
            logCalcoli.AppendLine($" COSTO TOTALE DI TRASPORTO: {costoTotale}");
            logCalcoli.AppendLine("═══════════════════════════════════════════");

            // Aggiorna il contenuto della TextBox con tutti i log accumulati
            txtCalcoli.Text = logCalcoli.ToString();

            // ===== Aggiornamento della cella del costo totale =====
            // Ottiene il dizionario dell'ultima riga (riga delle domande)
            var ultimaRiga = (IDictionary<string, object>)items[nRigheCorrente];
            
            // Inserisce il costo totale nella cella "Offerta" dell'ultima riga
            ultimaRiga["Offerta"] = costoTotale;

            // Aggiorna la visualizzazione della DataGrid con i nuovi dati
            mainGrid.Items.Refresh();
        }
    }
}
