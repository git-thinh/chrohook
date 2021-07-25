using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefSharp.MinimalExample.WinForms
{
    public interface IMain
    {
        void callApi(string message);
        void logClear();
        void logWrite(string text);
    }

    public class oHookSite
    {
        public string url { set; get; }
        public string js { set; get; }
        public string name { set; get; }
        public bool wait_event { set; get; }
    }

    public class oHook
    {
        public string name { set; get; }
        public string url { set; get; }
        public oHookSite[] sites { set; get; }
    }
}
