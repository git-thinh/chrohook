// Copyright © 2010-2015 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using CefSharp.WinForms;
using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Linq;

namespace CefSharp.MinimalExample.WinForms
{
    public static class UrlEx{
        public static string __urlRoot(this string url) => url.Split('?')[0].Split('#')[0].ToLower();

        public static oHookSite __GetByJsHook(this oHook hook, string urlMain, string urlJsHook)
        {
            if (string.IsNullOrWhiteSpace(urlMain)) return null;
            string url = urlMain.__urlRoot();
            string jsHook = urlJsHook.__urlRoot();
            var site = hook.sites.Where(x => 
                x.url.ToLower() == url
                && x.js.ToLower() == jsHook).Take(1).SingleOrDefault();
            return site;
        }
    }

    public class Program
    {
        static IMain __main;
        static string __hookName = ConfigurationManager.AppSettings["HOOK"].ToLower();
        static string __path = ConfigurationManager.AppSettings["PATH"].ToLower();
        static oHook __hookSite = null;
        [STAThread]
        public static int Main(string[] args)
        {
            bool ok = !string.IsNullOrWhiteSpace(__hookName);
            string file = "";
            if (ok)
            {
                file = __path+ "hook/" + __hookName + "/setting.json";
                ok = File.Exists(file);
            }

            if (ok) {
                try
                {
                    __hookSite = JsonConvert.DeserializeObject<oHook>(File.ReadAllText(file));
                }
                catch {
                    ok = false;
                }
            }

            if (ok == false || __hookSite == null) {
                MessageBox.Show("File invalid: " + file);
                return 0;
            }


            //Only required for PlatformTarget of AnyCPU
            AppDomain.CurrentDomain.AssemblyResolve += Resolver;

            //For Windows 7 and above, best to include relevant app.manifest entries as well
            Cef.EnableHighDPISupport();

            string pathCache = Path.Combine(Application.StartupPath, "Cache");
            var settings = new CefSettings()
            {
                //By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
                CachePath = pathCache
            };
             

            //Example of setting a command line argument
            //Enables WebRTC
            // - CEF Doesn't currently support permissions on a per browser basis see https://bitbucket.org/chromiumembedded/cef/issues/2582/allow-run-time-handling-of-media-access
            // - CEF Doesn't currently support displaying a UI for media access permissions
            //
            //NOTE: WebRTC Device Id's aren't persisted as they are in Chrome see https://bitbucket.org/chromiumembedded/cef/issues/2064/persist-webrtc-deviceids-across-restart
            settings.CefCommandLineArgs.Add("enable-media-stream");
            //https://peter.sh/experiments/chromium-command-line-switches/#use-fake-ui-for-media-stream
            settings.CefCommandLineArgs.Add("use-fake-ui-for-media-stream");
            //For screen sharing add (see https://bitbucket.org/chromiumembedded/cef/issues/2582/allow-run-time-handling-of-media-access#comment-58677180)
            settings.CefCommandLineArgs.Add("enable-usermedia-screen-capturing");
             
            //Perform dependency check to make sure all relevant resources are in our output directory.
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

            var browser = new BrowserForm(__hookSite);
            __main = (IMain)browser;
            Application.Run(browser);

            return 0;
        }

        public static void callApi(string data) => __main.callApi(data);
        public static void logWrite(string text)=> __main.logWrite(text);

        // Will attempt to load missing assembly from either x86 or x64 subdir
        //when PlatformTarget is AnyCPU
        private static System.Reflection.Assembly Resolver(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("CefSharp.Core.Runtime"))
            {
                string assemblyName = args.Name.Split(new[] { ',' }, 2)[0] + ".dll";
                string archSpecificPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                                                       Environment.Is64BitProcess ? "x64" : "x86",
                                                       assemblyName);

                return File.Exists(archSpecificPath)
                           ? System.Reflection.Assembly.LoadFile(archSpecificPath)
                           : null;
            }

            return null;
        }
    }
}
