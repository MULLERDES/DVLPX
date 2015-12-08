using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WSS
{
    public class SearchResult
    {
        public string Text { get; set; }
        public int Start { get; set; }
       

    }

    public class SearchItem
    {
        public string URL { get; set; }
        public IEnumerable<SearchResult> Results { get; set; }

    }

    interface ISearcher
    {
        IEnumerable<SearchResult> Results { get; set; }
        void DoSearch();
    } 

   


    public class SearchProvider
    {
        public string StartUrl { get; set; }
        public int MaxThread { get; set; } = 5;
        public int MaxURlsNumber { get; set; } = 0;



        public void Start(string s  )
        {
            //start new //
            // получение всех ссылок на странице
            // создание 
              /// ewr we
              /// await start pooling 
              /// list urls
              /// если позволяет добавить в список, то ок иначе отмена всего
              /// 
           
        }

        public void Pause()
        {

        }

        public void Resume() {}

        public void Stop() { }

    }
}
