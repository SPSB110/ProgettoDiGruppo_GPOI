using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ProgettoDiGruppo_GPOI
{
    public partial class MainWindow : Window
    {
        public static List<Matrix> Tabelle;
        public static List<float> Produzione = new List<float>();
        public static List<float> Destinazioni = new List<float>();
        public static List<float> Totali = new List<float>();
        public static int nCol = 4;
        public static int nRow =3;
        private List<ExpandoObject> righe;

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
                this.leftGrid.Height = this.Height * (1- topGridHeight);
                this.leftGrid.Margin = new Thickness(0, this.TopGrid.Height, 0, 0);

                this.btnHome.Height = this.TopGrid.Height * 0.7;

            };
            this.SizeChanged += MainWindow_SizeChanged;
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            // Intestazioni
            string[] intestazioniColonne = { "D1", "D2", "D3", "D4", "D5", "Totali" };
            string[] intestazioniRighe = { "UP1", "UP2", "UP3", "Totali" };

            int nCol = intestazioniColonne.Length;
            int nRow = intestazioniRighe.Length;

            // Dati di esempio
            int[,] valori = {
                { 10, 12, 80, 30, 50 },
                { 15, 30, 40, 45, 60 },
                { 20, 35, 30, 20, 50 }
            };

            // Crea colonne
            mainGrid.Columns.Clear();
            mainGrid.AutoGenerateColumns = false;

            // Prima colonna = intestazione riga
            mainGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "",
                Binding = new Binding("Riga"),
                IsReadOnly = true
            });

            // Colonne dati
            for (int c = 0; c < nCol; c++)
            {
                bool isTotaleCol = (c == nCol - 1);
                mainGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = intestazioniColonne[c],
                    Binding = new Binding(intestazioniColonne[c]),
                    IsReadOnly = isTotaleCol
                });
            }

            // Crea righe
            var righe = new List<ExpandoObject>();

            for (int r = 0; r < nRow; r++)
            {
                dynamic row = new ExpandoObject();
                var dict = (IDictionary<string, object>)row;
                dict["Riga"] = intestazioniRighe[r];

                for (int c = 0; c < nCol; c++)
                {
                    // Ultima riga = Totali
                    if (r == nRow - 1)
                    {
                        // Totale colonna
                        int totCol = 0;
                        for (int rr = 0; rr < nRow - 1; rr++)
                        {
                            totCol += valori[rr, c];
                        }
                        dict[intestazioniColonne[c]] = totCol;
                    }
                    else
                    {
                        // Ultima colonna = Totale riga
                        if (c == nCol - 1)
                        {
                            int totRiga = 0;
                            for (int cc = 0; cc < nCol - 1; cc++)
                            {
                                totRiga += valori[r, cc];
                            }
                            dict[intestazioniColonne[c]] = totRiga;
                        }
                        else
                        {
                            dict[intestazioniColonne[c]] = valori[r, c];
                        }
                    }
                }

                righe.Add(row);
            }

            mainGrid.ItemsSource = righe;

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
    }
}
