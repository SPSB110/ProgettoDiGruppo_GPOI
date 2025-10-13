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
                this.mainGrid.Margin = new Thickness(this.leftGrid.Width, this.TopGrid.Height, 0, 0);

                this.btnHome.Height = this.TopGrid.Height * 0.7;

            };
            this.SizeChanged += MainWindow_SizeChanged;
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

            CreaTabella(nRighe, nColonne);
        }

        private void CreaTabella(int nRighe, int nColonne)
        {
            mainGrid.Columns.Clear();

            // Prima colonna = intestazioni righe
            mainGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "",
                Binding = new Binding("Riga"),
                IsReadOnly = true
            });

            // Crea intestazioni colonne dinamiche (D1, D2, …)
            string[] intestazioniColonne = new string[nColonne+1];
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
            intestazioniColonne[nColonne] = "Totali";
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

            dict1["Riga"] = "Totali";

            foreach (var col in intestazioniColonne)
            {
                dict1[col] = ""; // celle inizialmente vuote
            }

            listaRighe.Add(row1);

            mainGrid.ItemsSource = listaRighe;
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
