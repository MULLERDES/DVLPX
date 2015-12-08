using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Threading;
using System.Net.Http;

namespace WebSiteSearch
{
    /// <summary>
    /// Interaction logic for TWINDOW.xaml
    /// </summary>
    public partial class TWINDOW :Window
    {
        CancellationTokenSource cts;
        public TWINDOW()
        {
            InitializeComponent();
        }

        private async void startclick(object sender, RoutedEventArgs e)
        {
            cts = new CancellationTokenSource();

            try
            {
                await AccessTheWebAsync(cts.Token);
             MessageBox.Show(   "\r\nDownloads complete.)");
            }
            catch(OperationCanceledException)
            {
               MessageBox.Show( "\r\nDownloads canceled.\r\n");
            }
            catch(Exception)
            {
               MessageBox.Show( "\r\nDownloads failed.\r\n");
            }

            cts = null;
        }

        async Task AccessTheWebAsync(CancellationToken ct)
        {
            //List<Task<int>> UndoneTaskQuerry = new List<Task<int>>();
            //IEnumerable<string> Results = new List<string>();
            //while (UndoneTaskQuerry.Count()>0)
            //{
            //    Task<int> firstFinishedTask = await Task.WhenAny(UndoneTaskQuerry);
            //    UndoneTaskQuerry.Remove(firstFinishedTask);
            //    //results 
            //    var res = await firstFinishedTask;
            //    //if res count>0
                

            ////  if result > max urls return;
            //}

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
                Console.WriteLine( String.Format("\r\nLength of the download:  {0}", length));

             //   downloadTasks.Add(ProcessURL("http://msdn.microsoft.com", client, ct));
            }
        }

        private List<string> SetUpURLList()
        {
            List<string> urls = new List<string>
            {
                "http://msdn.microsoft.com"
                //,
                //"http://msdn.microsoft.com/library/windows/apps/br211380.aspx",
                //"http://msdn.microsoft.com/en-us/library/hh290136.aspx",
                //"http://msdn.microsoft.com/en-us/library/dd470362.aspx",
                //"http://msdn.microsoft.com/en-us/library/aa578028.aspx",
                //"http://msdn.microsoft.com/en-us/library/ms404677.aspx",
                //"http://msdn.microsoft.com/en-us/library/ff730837.aspx"
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
