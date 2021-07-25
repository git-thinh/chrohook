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
        readonly oHook __hookSite;
        readonly ChromiumWebBrowser browser;
        string __path_app = Path.GetDirectoryName(Application.ExecutablePath);
        static string __hookName = ConfigurationManager.AppSettings["HOOK"].ToLower();
        static StringBuilder __log = new StringBuilder();

        private JsAPI _apiJs;
        private JsLog _logJs;
        public BrowserForm(oHook hook)
        {
            __hookSite = hook;

            InitializeComponent();

            Text = "Test";
            //WindowState = FormWindowState.Maximized;

            this.Text = __hookSite.url;
            browser = new ChromiumWebBrowser(__hookSite.url);
            browser.RequestHandler = new CustomRequestHandler(this, __hookSite);
            browser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
            _apiJs = new JsAPI();
            _logJs = new JsLog();
            browser.JavascriptObjectRepository.Register("__api", _apiJs, isAsync: false, options: BindingOptions.DefaultBinder);
            browser.JavascriptObjectRepository.Register("__log", _logJs, isAsync: false, options: BindingOptions.DefaultBinder);

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
        public class JsAPI { public void call(string msg) => Program.callApi(msg); }
        public class JsLog { public void write(string msg) => Program.logWrite(msg); }
        public void logWrite(string text)
        {
            __log.Append(text + Environment.NewLine);
        }
        public void callApi(string message)
        {
            //MessageBox.Show(message);
        }


        public void logClear() => __log.Clear();

        private void OnLoadingStateChanged(object sender, LoadingStateChangedEventArgs args)
        {
            SetCanGoBack(args.CanGoBack);
            SetCanGoForward(args.CanGoForward);

            this.InvokeOnUiThreadIfRequired(() => SetIsLoading(!args.CanReload));
        }
        private void OnBrowserConsoleMessage(object sender, ConsoleMessageEventArgs args)
        {
            string s = string.Format("Line: {0}, Source: {1}, Message: {2} {3}", args.Line, args.Source, args.Message, 
                Environment.NewLine + Environment.NewLine);
            __log.Append(s);
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

        private void OnBrowserStatusMessage(object sender, StatusMessageEventArgs args)
        {
            this.InvokeOnUiThreadIfRequired(() => statusLabel.Text = args.Value);
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
                Text = __log.ToString()
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
