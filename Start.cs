using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;

namespace DRG.WpfLibrary.Demo
{
    public class Start
    {
        // 全局唯一的 Mutex 名称
        private const string MutexName = "DRG.WpfLibrary.SingleInstanceMutex";

        public Start()
        {
            bool createdNew;
            // 创建命名互斥锁
            using var mutex = new Mutex(true, MutexName, out createdNew);

            if (!createdNew)
            {
                return;
            }

            var thread = new Thread(() =>
            {
                var app = new Application();
                var window = new MainWindow();
                app.Run(window);
            })
            {
                Name = "WPF UI Thread"
            };
            thread.SetApartmentState(ApartmentState.STA); // WPF 必须 STA  
            thread.Start();
        }
    }
}
