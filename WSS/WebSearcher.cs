using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.IO;
using System.Net.Http;

namespace WSS
{
    public class Boogle :ISearcher<SearchResult>
    {
        private object ToPauseSyncObject = new object();
        private bool isPaused = false;
        public event EventHandler<FoundItemEventArgs> ItemScaned;

        public string StartUrl { get; set; } = "http://metanit.8com/";
        private int MaxInTimeThreads { get; set; } = 4;
        public int MaxUrls { get; set; } = 10;

        public int MaximumDownloadThreads
        {
            get
            {
                return MaxInTimeThreads;
            }

            set
            {
                MaxInTimeThreads = value;
            }
        }

        public void Pause()
        {
            if(!isPaused)
            {
                isPaused = true;

                Monitor.Enter(ToPauseSyncObject);
            }
        }

        public void Resume()
        {
            if(isPaused)
            {
                isPaused = false;

                Monitor.Exit(ToPauseSyncObject);
            }
        }

        public async Task<SearchResult> Start(string TextToFind, CancellationToken cts)
        {

            HttpClient httpclient = new HttpClient();
            int NumberOfUrls = 0;
            SearchResult StartResult = new SearchResult() { URL = StartUrl };
            List<string> uncompledetUrls = new List<string>();
            List<Task<SearchResult>> currenttasks = new List<Task<SearchResult>>();
            List<SearchResult> uncompletedResults = new List<SearchResult>();
            currenttasks.Add(ProcessURL(StartResult, httpclient, cts, TextToFind));
            while(currenttasks.Count > 0)
            {


                await Task.Run(new Action(() =>
                {
                    while(isPaused)
                    {
                        Thread.Sleep(50);
                    }
                }));


                Task<SearchResult> fistFinished = await Task.WhenAny(currenttasks);
                currenttasks.Remove(fistFinished);
                SearchResult res = await fistFinished;

                ItemScaned?.Invoke(this, new FoundItemEventArgs(res));
                if(++NumberOfUrls == MaxUrls)
                {
                    return StartResult;
                }

                foreach(var item in res.Childresults)
                {
                    if(StartResult.ContainsURL(item.URL))
                        continue;
                    uncompletedResults.Add(item);
                }
                while(currenttasks.Count < MaxInTimeThreads && uncompletedResults.Count > 0)
                {
                    var tmp = uncompletedResults.First();
                    {
                        currenttasks.Add(ProcessURL(tmp, httpclient, cts, TextToFind));

                    }
                    uncompletedResults.Remove(tmp);
                }
            }
            return StartResult;
        }

        private async Task<SearchResult> ProcessURL(SearchResult root, HttpClient client, CancellationToken cts, string comparer)
        {
            SearchResult ret = new SearchResult()
            {
                URL = root.URL
            };

            HttpResponseMessage response = null;// 

            try
            {
                response = await client.GetAsync(root.URL, cts);
            }
            catch(UriFormatException)
            {

                ret.Status = HttpStatusCode.NotFound;
                return ret;
            }
            catch(HttpRequestException)
            {
                ret.Status = HttpStatusCode.NotFound;
                return ret;
            }

            try
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                string s = Encoding.UTF8.GetString(bytes);

                ret.Status = response.StatusCode;
                ret.PageSize = bytes.Length;

                if(ret.Status == HttpStatusCode.OK)
                {
                    int idx = 0;
                    while(idx>=0&&idx<s.Length&& (idx = s.IndexOf("<a", idx)) != -1)
                    {
                        int idxEnd = s.IndexOf("</a>", idx);
                        int hrefidx = s.IndexOf("href=", idx);
                        if(hrefidx > idx && hrefidx < idxEnd)
                        {
                            int lastQ = s.IndexOf('"', hrefidx + 6);
                            if(lastQ > hrefidx && lastQ < idxEnd)
                            {
                                string url = s.Substring(hrefidx + 6, lastQ - (hrefidx + 6));
                                if(!url.Contains("http://") && !url.Contains("https://"))
                                    url = url.Insert(0, "http://");

                                Uri o;
                                if(Uri.TryCreate(url, UriKind.Absolute, out o))
                                    ret.AddChildren(new SearchResult() { URL = url });
                            }
                        }
                        idx = idxEnd;
                    }
                }
                ret.FirstMatchOffset = s.IndexOf(comparer);
                if(ret.FirstMatchOffset > 0)
                {
                    int f = ret.FirstMatchOffset;
                    int maxCAT = 100;
                    StringBuilder sb = new StringBuilder(comparer);
                    for(int i = 0; i < maxCAT; i++)
                    {
                        char C;
                        if(i + f + comparer.Length < s.Length)
                        {
                           C = s[i + f + comparer.Length]; 
                            sb.Append(C=='\n'?' ':C);
                        }
                        if(f - i > 0)
                        {
                            C = s[f - i];
                            sb.Insert(0, C == '\n' ? ' ' : C);
                        }
                    }
                    ret.Substring = sb.ToString();
                }

                ret.DistinctMe();

            }
            catch(Exception)
            {

            }

            root.AddChildren(ret);
            return ret;
        }

        public void Stop()
        {
            isPaused = false;
        }
    }

    public class SearchResult :IEquatable<SearchResult>
    {
        public int PageSize { get; set; }
        public HttpStatusCode Status { get; set; } = HttpStatusCode.NotImplemented;
        public int FirstMatchOffset { get; set; }
        public bool IsUseful { get { return FirstMatchOffset > 0; } }
        public string Substring { get; set; }
        public string URL { get; set; }
        
        public IEnumerable<SearchResult> Childresults
        {
            get
            {
                return _childresults;
            }
        }
        public void AddChildren(SearchResult r)
        {
            (_childresults as List<SearchResult>).Add(r);
        }
        private IEnumerable<SearchResult> _childresults = new List<SearchResult>();

        internal void DistinctMe()
        {
            _childresults = _childresults.Distinct().Where(tf => !(tf.URL.Contains("javascript")))
                .ToList();
        }

        public bool ContainsURL(string url)
        {
            return ContainsUrl(this, url);
        }

        private bool ContainsUrl(SearchResult sr, string url)
        {
            foreach(var item in sr.Childresults)
            {
                if(item.URL == url)
                    return true;
                else return ContainsUrl(item, url);
            }
            return false;
        }

        public bool Equals(SearchResult other)
        {
            if(Object.ReferenceEquals(other, null)) return false;
            if(Object.ReferenceEquals(this, other)) return true;
            return URL.Equals(other.URL);
        }
        public override int GetHashCode()
        {

            return URL == null ? 0 : URL.GetHashCode();
        }

        public override string ToString()
        {
            return $"{ URL} {Status.ToString()}";
        }
    }

    public interface ISearcher<SR> where SR : class
    {
        event EventHandler<FoundItemEventArgs> ItemScaned;
        Task<SR> Start(string comparer, CancellationToken cts);
        void Pause();
        void Resume();
        void Stop();

        int MaximumDownloadThreads { get; set; }

    }


    public class FoundItemEventArgs :EventArgs
    {
        public SearchResult SR { get; private set; }
        public FoundItemEventArgs(SearchResult sr)
        {
            SR = sr;
        }
    }
}
