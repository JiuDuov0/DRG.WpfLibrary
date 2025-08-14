using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DRG.WpfLibrary.Demo
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private string pendingFolderName;
        private double _leftPanelExpandedWidth = 280;
        private bool _leftPanelCollapsed = false;
        public SettingsWindow()
        {
            InitializeComponent();

            // 读取主题配置
            string configPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Overlay", "WindowSettings", "settings.json");
            if (File.Exists(configPath))
            {
                try
                {
                    string json = File.ReadAllText(configPath);
                    using var doc = System.Text.Json.JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("theme", out var themeProp))
                    {
                        string theme = themeProp.GetString();
                        if (string.Equals(theme, "dark", StringComparison.OrdinalIgnoreCase))
                        {
                            var darkTheme = new ResourceDictionary
                            {
                                Source = new Uri("pack://application:,,,/DRG.WpfLibrary.Demo;component/DarkTheme.xaml", UriKind.Absolute)
                            };
                            this.Resources.MergedDictionaries.Add(darkTheme);
                        }
                        else
                        {
                            var normalTheme = new ResourceDictionary
                            {
                                Source = new Uri("pack://application:,,,/DRG.WpfLibrary.Demo;component/NormalTheme.xaml", UriKind.Absolute)
                            };
                            this.Resources.MergedDictionaries.Add(normalTheme);
                        }
                    }
                }
                catch { /* 可选：异常处理 */ }
            }

            // 获取主屏幕宽高
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            // 设置窗口宽高
            this.Width = screenWidth * 0.7;
            this.Height = screenHeight * 0.5;

            // 设置窗口位置
            this.Left = screenWidth * 0.15;
            this.Top = screenHeight * 0.5;

            LoadOverlayFolders();

            webView2.NavigationCompleted += WebView2_NavigationCompleted;
            webView2.WebMessageReceived += WebView2_WebMessageReceived;
            this.Loaded += Window_Loaded;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await webView2.EnsureCoreWebView2Async();
            // 设置 WebView2 背景为透明
            webView2.DefaultBackgroundColor = System.Drawing.Color.FromArgb(0, 0, 0, 0);
        }

        private void btnToggleListBox_Click(object sender, RoutedEventArgs e)
        {
            ToggleLeftPanel();
        }

        private void ToggleLeftPanel(bool? collapse = null)
        {
            bool targetCollapse = collapse ?? !_leftPanelCollapsed;

            // 计算起止宽度
            double from = LeftPanel.ActualWidth > 0 ? LeftPanel.ActualWidth : LeftPanel.Width;
            if (!_leftPanelCollapsed && from <= 0) from = _leftPanelExpandedWidth;

            double to = targetCollapse ? 0 : _leftPanelExpandedWidth;

            // 展开之前先可见；收起完成后再设为 Collapsed，避免拦截事件
            if (!targetCollapse)
            {
                LeftPanel.Visibility = Visibility.Visible;
                // 避免首次展开时没有记住宽度
                if (_leftPanelExpandedWidth <= 0) _leftPanelExpandedWidth = 280;
            }

            var duration = TimeSpan.FromMilliseconds(220);

            var widthAnim = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = duration,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            var opacityAnim = new DoubleAnimation
            {
                From = targetCollapse ? 1 : 0,
                To = targetCollapse ? 0 : 1,
                Duration = TimeSpan.FromMilliseconds(160),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            widthAnim.Completed += (s, a) =>
            {
                _leftPanelCollapsed = targetCollapse;

                if (targetCollapse)
                {
                    // 收起后隐藏，避免 0 宽度时仍拦截点击
                    LeftPanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    // 展开结束时，记录最终宽度，方便下次收起/展开
                    _leftPanelExpandedWidth = LeftPanel.Width > 0 ? LeftPanel.Width : _leftPanelExpandedWidth;
                }
            };

            LeftPanel.BeginAnimation(FrameworkElement.WidthProperty, widthAnim);
            LeftPanel.BeginAnimation(UIElement.OpacityProperty, opacityAnim);
        }

        private void listBoxFolders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBoxFolders.SelectedItem is string folderName)
            {
                LoadWebView2Content(folderName);
            }
        }

        public void LoadOverlayFolders()
        {
            string overlayDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Overlay");
            if (!Directory.Exists(overlayDir)) return;

            var folders = Directory.GetDirectories(overlayDir)
                .Where(dir =>
                    File.Exists(System.IO.Path.Combine(dir, "settings.html")) &&
                    File.Exists(System.IO.Path.Combine(dir, "settings.json")))
                .Select(dir => System.IO.Path.GetFileName(dir))
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            listBoxFolders.ItemsSource = folders;

            // 默认选中第一个
            if (folders.Count > 0)
                listBoxFolders.SelectedIndex = 0;
        }

        public void LoadWebView2Content(string folderName)
        {
            string overlayDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Overlay");
            string htmlPath = System.IO.Path.Combine(overlayDir, folderName, "settings.html");
            if (File.Exists(htmlPath))
            {
                webView2.Source = new Uri(htmlPath, UriKind.Absolute);
                pendingFolderName = folderName;
            }
        }

        public async Task SendSettingsJsonToWebView2Async(string folderName)
        {
            string overlayDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Overlay");
            string jsonPath = System.IO.Path.Combine(overlayDir, folderName, "settings.json");
            if (File.Exists(jsonPath) && webView2.CoreWebView2 != null)
            {
                string json = File.ReadAllText(jsonPath);
                // 判断 json 是否为对象或数组
                string jsArg = (json.StartsWith("{") || json.StartsWith("[")) ? json : $"\"{json.Replace("\"", "\\\"")}\"";
                await webView2.CoreWebView2.ExecuteScriptAsync($"window.receiveSettingsData({jsArg})");
            }
        }

        private async void WebView2_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!string.IsNullOrEmpty(pendingFolderName))
            {
                await SendSettingsJsonToWebView2Async(pendingFolderName);
                pendingFolderName = null;
            }
        }

        private void WebView2_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            string message = e.TryGetWebMessageAsString();

            // 获取当前选中的文件夹
            string folderName = listBoxFolders.SelectedItem as string;
            if (!string.IsNullOrEmpty(folderName))
            {
                string overlayDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Overlay");
                string jsonPath = System.IO.Path.Combine(overlayDir, folderName, "settings.json");
                try
                {
                    File.WriteAllText(jsonPath, message, Encoding.UTF8);
                    //MessageBox.Show($"设置已保存到:\n{jsonPath}", "保存成功");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("保存设置失败: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("未选中文件夹，无法保存设置。", "保存失败");
            }
        }
    }
}
