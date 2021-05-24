using CefSharp.Handler;
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

    public class CustomResourceRequestHandler : ResourceRequestHandler
    {
        readonly string data;
        readonly string url;
        readonly string type;
        public CustomResourceRequestHandler(string url_, string data_, string type_) : base()
        {
            data = data_;
            url = url_;
            type = type_;
        }

        protected override IResourceHandler GetResourceHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
        {
            //ResourceHandler.FromStream(stream, mimeType);
            //ResourceHandler.FromString(htmlString, includePreamble: true, mimeType: Cef.GetMimeType(fileExtension));
            //ResourceHandler.FromFilePath("CefSharp.Core.xml", mimeType);

            //ResourceHandler has many static methods for dealing with Streams, 
            // byte[], files on disk, strings
            // Alternatively ou can inheir from IResourceHandler and implement
            // a custom behaviour that suites your requirements.
            //return ResourceHandler.FromString("Welcome to CefSharp!", mimeType: Cef.GetMimeType("html"));

            string mimeType = type;
            if(string.IsNullOrEmpty(mimeType)) mimeType = Cef.GetMimeType(url);
            return ResourceHandler.FromString(data, includePreamble: true, mimeType: mimeType);
        }
    }

    public class CustomRequestHandler : RequestHandler
    {
        readonly IMain m_main;
        public CustomRequestHandler(IMain main) : base() => m_main = main;

        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            string url = request.Url;
            m_main.logUrl(url);
            string data = m_main.getData(url);
            if (data.Length > 0)
                return new CustomResourceRequestHandler(url, data, m_main.getType(url));

            ////Only intercept specific Url's
            //if (request.Url == "http://cefsharp.test/" || request.Url == "https://cefsharp.test/")
            //{
            //	return new CustomResourceRequestHandler();
            //}

            //Default behaviour, url will be loaded normally.
            return null;
        }
    }
}
