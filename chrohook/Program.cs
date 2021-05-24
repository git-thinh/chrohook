// Copyright © 2010-2015 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using CefSharp.WinForms;
using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace CefSharp.MinimalExample.WinForms
{
    public static class UrlEx{
        public static string __urlRoot(this string url) => url.Split('?')[0].Split('#')[0].ToLower();
    }

    public class Program
    {
        static string __HOOK = ConfigurationManager.AppSettings["HOOK"].ToLower();

        [STAThread]
        public static int Main(string[] args)
        {
            bool ok = string.IsNullOrWhiteSpace(__HOOK);
            string file = "";
            if (ok)
            {
                file = "hook/" + __HOOK + "/setting.json";
                ok = File.Exists(file);
            }

            if (ok) { 
            
            }

            if (ok == false) {
                MessageBox.Show("Can not find the file: " + file);
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

            var browser = new BrowserForm();
            Application.Run(browser);

            return 0;
        }

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
