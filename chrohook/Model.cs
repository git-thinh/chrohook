using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefSharp.MinimalExample.WinForms
{
    public interface IMain
    {
        void logClear();
        void logUrl(string url);
        string getData(string url);
        string getType(string url);
    }

    public class oHookSite
    {
        public string url { set; get; }
        public string js { set; get; }
        public string name { set; get; }
    }

    public class oHook
    {
        public string url { set; get; }
        public oHookSite[] sites { set; get; }
    }
}
