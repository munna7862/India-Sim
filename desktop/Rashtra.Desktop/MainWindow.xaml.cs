using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace Rashtra.Desktop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeWebViewAsync();
        }

        private async Task InitializeWebViewAsync()
        {
            try
            {
                // Locate dist-web directory
                string? distWebPath = FindDistWebDirectory();
                if (distWebPath == null || !File.Exists(Path.Combine(distWebPath, "index.html")))
                {
                    MessageBox.Show(
                        "Unable to find the 'dist-web' directory containing 'index.html'.\nPlease build the web assets using 'npm run build' before running the desktop application.",
                        "Rashtra: Sovereign Republic - Asset Missing",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                // Initialize WebView2 environment
                var env = await CoreWebView2Environment.CreateAsync(null, Path.Combine(Path.GetTempPath(), "RashtraGame_WebView2Data"));
                await WebView.EnsureCoreWebView2Async(env);

                // Configure virtual host name mapping to bypass CORS for local GeoJSON and scenario fetches
                WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "rashtra.game",
                    distWebPath,
                    CoreWebView2HostResourceAccessKind.Allow);

                // Disable default context menu in production release
                WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                WebView.CoreWebView2.Settings.IsStatusBarEnabled = false;

                // Navigate to the offline game bundle
                WebView.CoreWebView2.Navigate("https://rashtra.game/index.html");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error initializing desktop game engine:\n{ex.Message}\n\nPlease ensure Microsoft Edge WebView2 runtime is installed.",
                    "Rashtra Initialization Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private static string? FindDistWebDirectory()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Direct sibling in release bundle: ./dist-web
            string candidate = Path.Combine(baseDir, "dist-web");
            if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "index.html")))
            {
                return Path.GetFullPath(candidate);
            }

            // Developer dev build traversal: walk up to repository root
            var dir = new DirectoryInfo(baseDir);
            for (int i = 0; i < 6 && dir != null; i++)
            {
                candidate = Path.Combine(dir.FullName, "dist-web");
                if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "index.html")))
                {
                    return Path.GetFullPath(candidate);
                }
                dir = dir.Parent;
            }

            return null;
        }
    }
}
