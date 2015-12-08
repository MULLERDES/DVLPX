using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WebSiteSearch
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow :Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }
        public class User
        {
            public string Name { get; set; }

            public int Age { get; set; }

            public string Mail { get; set; }
        }
        CancellationTokenSource cts;
        ObservableCollection<User> items = new ObservableCollection<User>();
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CancellationTokenSource s;

            items.Add(new User() { Name = "John Doe", Age = 42, Mail = "john@doe-family.com" });
            items.Add(new User() { Name = "Jane Doe", Age = 39, Mail = "jane@doe-family.com" });
            items.Add(new User() { Name = "Sammy Doe", Age = 13, Mail = "sammy.doe@gmail.com" });
            lvDataBinding.ItemsSource = items;

            string urlAddress = "http://google.com";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if(response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if(response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                response.Close();
                readStream.Close();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            items.Add(new User() { Name = "John Doe", Age = 42, Mail = "john@doe-family.com" });
            lvDataBinding.ItemsSource = items.OrderBy(tf => tf.Name);
            startButton_Click(this, e);
        }

        private async void startButton_Click(object sender, RoutedEventArgs e)
        {
            //resultsTextBox.Clear();

            // Instantiate the CancellationTokenSource.
            cts = new CancellationTokenSource();

            try
            {
                await AccessTheWebAsync(cts.Token);
                Console.WriteLine("Downloads complete");
                ///resultsTextBox.Text += "\r\nDownloads complete.";
            }
            catch(OperationCanceledException)
            {
                // resultsTextBox.Text += "\r\nDownloads canceled.\r\n";
                Console.WriteLine("Downloads canceled");
            }
            catch(Exception)
            {
                //resultsTextBox.Text += "\r\n\r\n";  
                Console.WriteLine("Downloads failed.");
            }

            cts = null;
        }


        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            if(cts != null)
            {
                cts.Cancel();
            }
        }


        async Task AccessTheWebAsync(CancellationToken ct)
        {
            HttpClient client = new HttpClient();

            // Make a list of web addresses.
            List<string> urlList = SetUpURLList();

            // ***Create a query that, when executed, returns a collection of tasks.
            IEnumerable<Task<int>> downloadTasksQuery =
                from url in urlList select ProcessURL(url, client, ct);

            // ***Use ToList to execute the query and start the tasks. 
            List<Task<int>> downloadTasks = downloadTasksQuery.ToList();

            // ***Add a loop to process the tasks one at a time until none remain.
            while(downloadTasks.Count > 0)
            {
                // Identify the first task that completes.
                Task<int> firstFinishedTask = await Task.WhenAny(downloadTasks);

                // ***Remove the selected task from the list so that you don't
                // process it more than once.
                downloadTasks.Remove(firstFinishedTask);

                // Await the completed task.
                int length = await firstFinishedTask;
                Console.WriteLine(String.Format("Length of the download:  { 0}", length));
                //resultsTextBox.Text += String.Format("\r\nLength of the download:  {0}", length);
            }
        }


        private List<string> SetUpURLList()
        {
            List<string> urls = new List<string>
            {
                "http://msdn.microsoft.com",

 "http://msdn.microsoft.com",
                "http://msdn.microsoft.com",
                "http://msdn.microsoft.com",
                "http://msdn.microsoft.com",
                "http://msdn.microsoft.com"
                                    };
            return urls;
        }


        async Task<int> ProcessURL(string url, HttpClient client, CancellationToken ct)
        {
            // GetAsync returns a Task<HttpResponseMessage>. 
            HttpResponseMessage response = await client.GetAsync(url, ct);

            // Retrieve the website contents from the HttpResponseMessage.
            byte[] urlContents = await response.Content.ReadAsByteArrayAsync();

            return urlContents.Length;
        }
    }
}
