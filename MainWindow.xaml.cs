using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DRG.WpfLibrary.Demo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool interceptInput = false;
        private IntPtr _hookID = IntPtr.Zero;
        private LowLevelKeyboardProc _proc;
        private Key _configKey = Key.None;
        private FileSystemWatcher overlayWatcher;
        private SettingsWindow? settingsWindowInstance;
        private System.Threading.CancellationTokenSource? hookReloadCts;

        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            this.Topmost = true;
            this.Loaded += MainWindow_Loaded;
            this.KeyDown += MainWindow_KeyDown;
            this.Focusable = true;
            this.Focus();
            StartPipeServer();
            StartOverlayWatcher();
            //StartProcessMonitor("FSD-Win64-Shipping");
            LoadConfigKey(); // 读取配置
            SetGlobalKeyboardHook(); // 设置钩子
            InitOverlayFileWatcher();// 启动文件监听
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateWindowStyle();
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.J)
            //{
            //    interceptInput = !interceptInput;
            //    UpdateWindowStyle();
            //}
        }

        private void UpdateWindowStyle()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int exStyle = (int)GetWindowLong(hwnd, GWL_EXSTYLE);

            if (interceptInput)
            {
                // 移除 WS_EX_TRANSPARENT，窗口拦截输入
                exStyle &= ~WS_EX_TRANSPARENT;
            }
            else
            {
                // 添加 WS_EX_TRANSPARENT，窗口不拦截输入
                exStyle |= WS_EX_TRANSPARENT;
            }
            // 始终保持 WS_EX_NOACTIVATE
            exStyle |= WS_EX_NOACTIVATE;

            SetWindowLong(hwnd, GWL_EXSTYLE, (IntPtr)exStyle);
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        public void StartProcessMonitor(string processName)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    bool found = Process.GetProcessesByName(processName).Length > 0;
                    if (!found)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Application.Current.Shutdown();
                        });
                        break;
                    }
                    System.Threading.Thread.Sleep(1000);
                }
            });
        }

        private void StartOverlayWatcher()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string overlayDir = System.IO.Path.Combine(baseDir, "Overlay");
            if (Directory.Exists(overlayDir))
            {
                // 先清除所有 WebView2 控件
                Application.Current.Dispatcher.Invoke(() =>
                {
                    for (int i = overlayCanvas.Children.Count - 1; i >= 0; i--)
                    {
                        if (overlayCanvas.Children[i] is WebView2)
                            overlayCanvas.Children.RemoveAt(i);
                    }
                });

                var subDirs = Directory.GetDirectories(overlayDir);
                foreach (var dir in subDirs)
                {
                    string folderName = System.IO.Path.GetFileName(dir);
                    string htmlFile = System.IO.Path.Combine(dir, $"Index.html");
                    string jsonFile = System.IO.Path.Combine(dir, $"settings.json");
                    if (File.Exists(htmlFile) && File.Exists(jsonFile))
                    {
                        try
                        {
                            string json = File.ReadAllText(jsonFile);
                            var settings = JsonSerializer.Deserialize<OverlayWindowSettings>(json);
                            if (settings != null)
                            {
                                Application.Current.Dispatcher.Invoke(async () =>
                                {
                                    var webview = new WebView2();
                                    webview.Width = settings.width;
                                    webview.Height = settings.height;
                                    Canvas.SetLeft(webview, settings.x);
                                    Canvas.SetTop(webview, settings.y);
                                    overlayCanvas.Children.Add(webview);
                                    await webview.EnsureCoreWebView2Async();
                                    // 设置背景透明
                                    webview.DefaultBackgroundColor = System.Drawing.Color.FromArgb(0, 0, 0, 0);
                                    webview.Source = new Uri(htmlFile);
                                    webview.Tag = folderName;
                                });
                            }
                        }
                        catch { /* 可选：异常处理 */ }
                    }
                }
            }
        }

        private void ReloadWebView2(string folderName)
        {
            string overlayDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Overlay");
            string targetDir = System.IO.Path.Combine(overlayDir, folderName);
            string htmlFile = System.IO.Path.Combine(targetDir, "Index.html");
            string jsonFile = System.IO.Path.Combine(targetDir, "settings.json");

            if (File.Exists(htmlFile) && File.Exists(jsonFile))
            {
                try
                {
                    string json = File.ReadAllText(jsonFile);
                    var settings = JsonSerializer.Deserialize<OverlayWindowSettings>(json);
                    if (settings != null)
                    {
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            // 移除旧的 WebView2
                            for (int i = overlayCanvas.Children.Count - 1; i >= 0; i--)
                            {
                                if (overlayCanvas.Children[i] is WebView2 webview && webview.Tag is string tag && tag == folderName)
                                {
                                    overlayCanvas.Children.RemoveAt(i);
                                }
                            }
                            // 添加新的 WebView2
                            var newWebView = new WebView2();
                            newWebView.Width = settings.width;
                            newWebView.Height = settings.height;
                            Canvas.SetLeft(newWebView, settings.x);
                            Canvas.SetTop(newWebView, settings.y);
                            overlayCanvas.Children.Add(newWebView);
                            await newWebView.EnsureCoreWebView2Async();
                            // 设置背景透明
                            newWebView.DefaultBackgroundColor = System.Drawing.Color.FromArgb(0, 0, 0, 0);
                            newWebView.Source = new Uri(htmlFile);
                            newWebView.Tag = folderName;
                        });
                    }
                }
                catch { /* 可选：异常处理 */ }
            }
        }

        public void SendToMain(string message)
        {
            using (var pipe = new NamedPipeClientStream(".", "DRGModOverlayPipeBack", PipeDirection.Out))
            {
                pipe.Connect(1000); // 超时时间 单位：毫秒
                var bytes = Encoding.UTF8.GetBytes(message);
                pipe.Write(bytes, 0, bytes.Length);
            }
        }

        private void StartPipeServer()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    using (var pipe = new NamedPipeServerStream("DRGModOverlayPipe", PipeDirection.In))
                    {
                        pipe.WaitForConnection();
                        byte[] buffer = new byte[1024];
                        int len = pipe.Read(buffer, 0, buffer.Length);
                        string cmd = Encoding.UTF8.GetString(buffer, 0, len).Trim();

                        try
                        {
                            var jsonDoc = JsonDocument.Parse(cmd);
                            var root = jsonDoc.RootElement;
                            string Type = root.GetProperty("Type").GetString();

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (Type == "ModData")
                                {
                                    var Data = root.GetProperty("Data").ToString();
                                    var ModName = root.GetProperty("ModName").GetString();
                                    // 根据 data 内容做处理

                                    foreach (var child in overlayCanvas.Children)
                                    {
                                        if (child is WebView2 webview && webview.Tag is string tag && tag == ModName)
                                        {
                                            // 通过 JS 向 HTML 传递 Data
                                            if (webview.CoreWebView2 != null)
                                            {
                                                string jsArg;
                                                // 判断 Data 是否为 JSON 对象还是字符串
                                                if (Data.StartsWith("{") || Data.StartsWith("["))
                                                {
                                                    jsArg = Data; // 对象或数组直接传
                                                }
                                                else
                                                {
                                                    jsArg = $"\"{Data.Replace("\"", "\\\"")}\""; // 字符串加引号并转义
                                                }
                                                webview.CoreWebView2.ExecuteScriptAsync($"window.receiveModData({jsArg})");
                                            }
                                        }
                                    }
                                }
                                // 可扩展更多类型
                            });
                        }
                        catch (Exception ex)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (cmd == "hide")
                                {
                                    //this.Hide();
                                }
                            });
                        }
                    }
                }
            });
        }

        private void LoadConfigKey()
        {
            string jsonFile = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Overlay", "WindowSettings", "settings.json");
            if (File.Exists(jsonFile))
            {
                try
                {
                    string json = File.ReadAllText(jsonFile);
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("Key", out var keyProp))
                    {
                        string keyStr = keyProp.GetString();
                        if (!string.IsNullOrEmpty(keyStr) && Enum.TryParse<Key>(keyStr, true, out var key))
                        {
                            _configKey = key;
                        }
                    }
                }
                catch { }
            }
        }

        private void SetGlobalKeyboardHook()
        {
            _proc = HookCallback;
            _hookID = SetHook(_proc);
            this.Closed += (s, e) => UnhookWindowsHookEx(_hookID);
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            const int WM_KEYDOWN = 0x0100;
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Key pressedKey = KeyInterop.KeyFromVirtualKey(vkCode);
                if (pressedKey == _configKey)
                {
                    ToggleSettingsWindow();
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public void ReloadKeyboardHook()
        {
            // 取消上一次的重载请求
            hookReloadCts?.Cancel();
            hookReloadCts = new System.Threading.CancellationTokenSource();
            var token = hookReloadCts.Token;

            // 异步防抖延迟，避免频繁重载
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(300, token);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (_hookID != IntPtr.Zero)
                        {
                            UnhookWindowsHookEx(_hookID);
                            _hookID = IntPtr.Zero;
                        }
                        LoadConfigKey();
                        SetGlobalKeyboardHook();
                    });
                }
                catch (TaskCanceledException) {  }
            }, token);
        }

        private const int WH_KEYBOARD_LL = 13;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private void InitOverlayFileWatcher()
        {
            string overlayDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Overlay");
            overlayWatcher = new FileSystemWatcher(overlayDir)
            {
                IncludeSubdirectories = true,
                Filter = "settings.json",
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
            };
            overlayWatcher.Changed += OverlaySettingsChanged;
            overlayWatcher.Created += OverlaySettingsChanged;
            overlayWatcher.Renamed += OverlaySettingsChanged;
            overlayWatcher.EnableRaisingEvents = true;
        }

        private void OverlaySettingsChanged(object sender, FileSystemEventArgs e)
        {
            string windowSettingsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Overlay", "WindowSettings", "settings.json");
            string changedPath = e.FullPath;

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (string.Equals(changedPath, windowSettingsPath, StringComparison.OrdinalIgnoreCase))
                {
                    ReloadKeyboardHook();
                    if (settingsWindowInstance != null && settingsWindowInstance.IsLoaded)
                    {
                        settingsWindowInstance.Close();
                    }
                }
                else
                {
                    string overlayDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Overlay");
                    string folderName = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(changedPath));
                    ReloadWebView2(folderName);
                }
            });
        }

        public void ToggleSettingsWindow()
        {
            if (settingsWindowInstance == null || !settingsWindowInstance.IsLoaded)
            {
                this.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 0, 0, 0));
                settingsWindowInstance = new SettingsWindow();
                settingsWindowInstance.Owner = this;
                settingsWindowInstance.Closed += (s, e) =>
                {
                    this.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
                    settingsWindowInstance = null;
                };
                settingsWindowInstance.Show();
            }
            else
            {
                settingsWindowInstance.Close();
            }
        }
    }

    public class OverlayWindowSettings
    {
        public int x { get; set; }
        public int y { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }
}
