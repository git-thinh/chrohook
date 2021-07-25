using CefSharp.Handler;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefSharp.MinimalExample.WinForms
{
    public class CustomResourceRequestHandler : ResourceRequestHandler
    {
        static string __path = ConfigurationManager.AppSettings["PATH"].ToLower();

        readonly oHook m_hook;
        readonly oHookSite m_site;
        readonly IMain m_main;
        public CustomResourceRequestHandler(IMain main, oHook hook, oHookSite site) : base()
        {
            m_main = main;
            m_hook = hook;
            m_site = site;
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

            string data = string.Empty;
            
            string file = __path + "hook/" + m_hook.name + "/_.js";
            if (File.Exists(file)) data += File.ReadAllText(file);

            file = __path + "hook/" + m_hook.name + "/" + m_site.name + ".js";
            if (File.Exists(file)) data += File.ReadAllText(file);

            return ResourceHandler.FromString(data, includePreamble: true, mimeType: "application/javascript");
        }
    }

    public class CustomRequestHandler : RequestHandler
    {
        readonly oHook m_hook;
        readonly IMain m_main;
        public CustomRequestHandler(IMain main, oHook hook) : base()
        {
            m_main = main;
            m_hook = hook;
        }

        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            string refer = browser.MainFrame.Url;
            if (!string.IsNullOrWhiteSpace(refer))
            {
                string url = request.Url;
                m_main.logWrite(url);
                var site = m_hook.__GetByJsHook(refer, url);
                if (site != null)
                    return new CustomResourceRequestHandler(m_main, m_hook, site);
            }

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
