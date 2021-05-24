// Copyright © 2010-2015 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using CefSharp.MinimalExample.WinForms.Controls;
using CefSharp.WinForms;
using System;
using System.Configuration;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using System.IO;

namespace CefSharp.MinimalExample.WinForms
{
    public partial class BrowserForm : Form, IMain
    {
        readonly ChromiumWebBrowser browser;
        string __site = ConfigurationManager.AppSettings["SITE"].ToLower();
        string __site_html = ConfigurationManager.AppSettings["SITE_HTML"].ToLower();
        string __domain = "";
        bool __same_domain = ConfigurationManager.AppSettings["SAME_DOMAIN"] == "true";
        string __path_root = ConfigurationManager.AppSettings["PATH_ROOT"];
        string[] __js = ConfigurationManager.AppSettings["JS"].Split('|').Select(x => x.ToLower()).ToArray();
        string[] __css = ConfigurationManager.AppSettings["CSS"].Split('|').Select(x => x.ToLower()).ToArray();
        string __path_app = System.IO.Path.GetDirectoryName(Application.ExecutablePath);

        

        public string getData(string url)
        {
            string file = string.Empty;
            string s = url.__urlRoot();
            if (__site_html.Length > 0 && s == __site_html)
            {
                file = __path_app + "\\index.html";
                file = file.Replace("\\\\", "\\");
                if (File.Exists(file))
                    return File.ReadAllText(file);
            }


            if (s.Contains(".css") || s.Contains(".js"))
            {
                if (__same_domain)
                    if (!s.Contains(__domain))
                        return string.Empty;

                for (var i = 0; i < __js.Length; i++)
                {
                    if (s.Contains(__js[i]))
                    {
                        file = __js[i];
                        break;
                    }
                }

                if (file.Length == 0)
                {
                    for (var i = 0; i < __css.Length; i++)
                    {
                        if (s.Contains(__css[i]))
                        {
                            file = __css[i];
                            break;
                        }
                    }
                }

                if (file.Length > 0)
                {
                    file = __path_root + file.Replace('/', '\\');
                    file = file.Replace("\\\\", "\\");
                    if (File.Exists(file))
                        return File.ReadAllText(file);
                }
            }

            return string.Empty;
        }

        public string getType(string url)
        {
            if (__site_html.Length > 0 && url.__urlRoot() == __site_html)
                return "text/html";
            return string.Empty;
        }

        public BrowserForm()
        {
            InitializeComponent();

            Text = "Test";
            //WindowState = FormWindowState.Maximized;

            this.Text = __site;
            if (__site.Split('/').Length > 2) __domain = __site.Split('/')[2].Trim(); ;
            if (__domain.StartsWith("www.")) __domain = __domain.Substring(4);

            browser = new ChromiumWebBrowser(__site);
            browser.RequestHandler = new CustomRequestHandler(this);
            toolStripContainer.ContentPanel.Controls.Add(browser);

            browser.IsBrowserInitializedChanged += OnIsBrowserInitializedChanged;
            browser.LoadingStateChanged += OnLoadingStateChanged;
            browser.ConsoleMessage += OnBrowserConsoleMessage;
            browser.StatusMessage += OnBrowserStatusMessage;
            browser.TitleChanged += OnBrowserTitleChanged;
            browser.AddressChanged += OnBrowserAddressChanged;

            outputLabel.Visible = false;
            this.Shown += browserForm_Shown;
            this.FormClosing += (se, ev) => __exit();
        }

        static StringBuilder m_stringBuilder = new StringBuilder();
        public void logClear() => m_stringBuilder.Clear();
        public void logUrl(string url)
        {
            string s = url.ToLower();
            if (s.ToLower().StartsWith("devtools://")) return;

            if (__site_html.Length > 0 && url.ToLower().StartsWith(__site_html))
                m_stringBuilder.Append(url + Environment.NewLine);
            else if (s.Contains(".css") || s.Contains(".js"))
                m_stringBuilder.Append(url + Environment.NewLine);
        }

        private void browserForm_Shown(object sender, EventArgs e)
        {
            this.Left = 0;
            this.Top = 0;
            this.Height = Screen.PrimaryScreen.WorkingArea.Height;
            this.Width = 1070;
        }

        private void OnIsBrowserInitializedChanged(object sender, EventArgs e)
        {
            var b = ((ChromiumWebBrowser)sender);

            this.InvokeOnUiThreadIfRequired(() => b.Focus());
        }

        private void OnBrowserConsoleMessage(object sender, ConsoleMessageEventArgs args)
        {
            DisplayOutput(string.Format("Line: {0}, Source: {1}, Message: {2}", args.Line, args.Source, args.Message));
        }

        private void OnBrowserStatusMessage(object sender, StatusMessageEventArgs args)
        {
            this.InvokeOnUiThreadIfRequired(() => statusLabel.Text = args.Value);
        }

        private void OnLoadingStateChanged(object sender, LoadingStateChangedEventArgs args)
        {
            SetCanGoBack(args.CanGoBack);
            SetCanGoForward(args.CanGoForward);

            this.InvokeOnUiThreadIfRequired(() => SetIsLoading(!args.CanReload));
        }

        private void OnBrowserTitleChanged(object sender, TitleChangedEventArgs args)
        {
            this.InvokeOnUiThreadIfRequired(() => Text = args.Title);
        }

        private void OnBrowserAddressChanged(object sender, AddressChangedEventArgs args)
        {
            this.InvokeOnUiThreadIfRequired(() => urlTextBox.Text = args.Address);
        }

        private void SetCanGoBack(bool canGoBack)
        {
            //this.InvokeOnUiThreadIfRequired(() => backButton.Enabled = canGoBack);
        }

        private void SetCanGoForward(bool canGoForward)
        {
            //this.InvokeOnUiThreadIfRequired(() => forwardButton.Enabled = canGoForward);
        }

        private void SetIsLoading(bool isLoading)
        {
            goButton.Text = isLoading ?
                "Stop" :
                "Go";
            goButton.Image = isLoading ?
                Properties.Resources.nav_plain_red :
                Properties.Resources.nav_plain_green;

            HandleToolStripLayout();
        }

        public void DisplayOutput(string output)
        {
            this.InvokeOnUiThreadIfRequired(() => outputLabel.Text = output);
        }

        private void HandleToolStripLayout(object sender, LayoutEventArgs e)
        {
            HandleToolStripLayout();
        }

        private void HandleToolStripLayout()
        {
            var width = toolStrip1.Width;
            foreach (ToolStripItem item in toolStrip1.Items)
            {
                if (item != urlTextBox)
                {
                    width -= item.Width - item.Margin.Horizontal;
                }
            }
            urlTextBox.Width = Math.Max(0, width - urlTextBox.Margin.Horizontal - 18);
        }

        void __exit()
        {
            browser.Dispose();
            Cef.Shutdown();
            //Close();
        }

        private void GoButtonClick(object sender, EventArgs e)
        {
            logClear();
            LoadUrl(urlTextBox.Text);
        }

        private void BackButtonClick(object sender, EventArgs e)
        {
            browser.Back();
        }

        private void ForwardButtonClick(object sender, EventArgs e)
        {
            browser.Forward();
        }

        private void UrlTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            LoadUrl(urlTextBox.Text);
        }

        private void LoadUrl(string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                browser.Load(url);
            }
        }

        void __showDevTools()
        {
            browser.ShowDevTools();
        }

        private void btnDevTool_Click(object sender, EventArgs e) => __showDevTools();

        private void btnReload_Click(object sender, EventArgs e)
        {
            browser.Refresh();
        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            var t = new TextBox()
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                BackColor = Color.Black,
                ForeColor = Color.White,
                Font = new Font("Arial", 15.0f),
                Text = m_stringBuilder.ToString()
            };
            var f = new Form()
            {
                Text = "LOG",
                BackColor = Color.Black,
                WindowState = FormWindowState.Maximized,
                Padding = new Padding(10, 0, 0, 0)
            };
            f.Controls.Add(t);
            f.Show();
        }

        private void btnClearLog_Click(object sender, EventArgs e) => logClear();
    }
}
