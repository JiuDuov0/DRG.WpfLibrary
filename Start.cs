using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;

namespace DRG.WpfLibrary.Demo
{
    public class Start
    {
        // ȫ��Ψһ�� Mutex ����
        private const string MutexName = "DRG.WpfLibrary.SingleInstanceMutex";

        public Start()
        {
            bool createdNew;
            // ��������������
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
            thread.SetApartmentState(ApartmentState.STA); // WPF ���� STA  
            thread.Start();
        }
    }
}
