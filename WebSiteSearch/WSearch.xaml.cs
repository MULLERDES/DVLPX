using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WSS;

namespace WebSiteSearch
{
    /// <summary>
    /// Логика взаимодействия для WSearch.xaml
    /// </summary>
    public partial class WSearch :Window
    {

        ISearcher<SearchResult> SearchProvider;

        ObservableCollection<SearchResult> resultCollection;

        CancellationTokenSource cts;
        public WSearch()
        {
            InitializeComponent();
            lvResults.SelectionChanged += LvResults_SelectionChanged;
            lvResults.PreviewMouseDoubleClick += LvResults_PreviewMouseDoubleClick;
            slThreadNumber.ValueChanged += SlThreadNumber_ValueChanged;

        }

        private void LvResults_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(lvResults.SelectedItem is SearchResult)
            {
                SearchResult rez = (lvResults.SelectedItem as SearchResult);
                try
                {
                    Process.Start(rez.URL);
                }
                catch(Exception)
                {

                }
            }
            //    
        }

        private void SlThreadNumber_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(SearchProvider is ISearcher<SearchResult>)
                SearchProvider.MaximumDownloadThreads = (int)slThreadNumber.Value;
        }

        private void LvResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }



        bool isStarted = false;
        bool isPaused = false;
        private async void StatrClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if(isStarted)
            {
                if(isPaused)
                {
                    isPaused = false;
                    SearchProvider?.Resume();
                    btn.Content = "Pause";
                    return;
                }

                SearchProvider.Pause();
                isPaused = true;
                btn.Content = "Continue";
                return;
            }

            btn.Content = "Pause";
            int MaxUrls;
            if(!int.TryParse(tbMaxURLS.Text, out MaxUrls) || MaxUrls < 1)
            {
                MessageBox.Show("Incorrect maximum urls parametr");
                return;
            }


            StartUI();
            mainProgressbar.Value = 0;
            mainProgressbar.Maximum = MaxUrls;

            resultCollection = new ObservableCollection<SearchResult>();
            lvResults.ItemsSource = resultCollection;

            SearchProvider = new WebFinder() { StartUrl = tbURL.Text, MaxUrls = MaxUrls };

            SearchProvider.MaximumDownloadThreads = (int)slThreadNumber.Value;
            int b = 0;
            SearchProvider.ItemScaned += (s, args) =>
            {
                   Console.WriteLine($"Found on {b++ }");
                mainProgressbar.Value++;
                resultCollection.Insert(0,args.SR);
            };
            cts = new CancellationTokenSource();
            try
            {
                tb1.Text = "Download started";// +Environment.NewLine;
                await SearchProvider.Start(tbPhrase.Text, cts.Token);
                tb1.Text = "Download complete";// +Environment.NewLine;

            }
            catch(OperationCanceledException)
            {
                tb1.Text = "Download canceled";// + Environment.NewLine;
            }
            catch(Exception)
            {
                tb1.Text = "Download failed";// + Environment.NewLine;
            }

            cts = null;

            btn.Content = "Start";
            StopUI();
        }


        void StartUI()
        {
            isStarted = true;
            tbMaxURLS.IsEnabled = false;
        }

        void StopUI()
        {
            isStarted = false;
            tbMaxURLS.IsEnabled = true;
        }

        private void lvResults_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }



        private void itemButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            bStart.Content = "Start";
            cts?.Cancel();
            tbMaxURLS.IsEnabled = true;
        }




    }
}
