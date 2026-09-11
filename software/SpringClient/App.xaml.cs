using System;
using System.IO;
using System.Windows;

namespace SpringClient
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                File.AppendAllText("crash_log.txt", $"[UnhandledException] {DateTime.Now}: {args.ExceptionObject}\n");
                MessageBox.Show($"Application Error: {args.ExceptionObject}", "Spring Client Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            DispatcherUnhandledException += (s, args) =>
            {
                File.AppendAllText("crash_log.txt", $"[DispatcherUnhandledException] {DateTime.Now}: {args.Exception}\n");
                MessageBox.Show($"UI Exception: {args.Exception.Message}\n{args.Exception.StackTrace}", "Spring Client Error", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };

            base.OnStartup(e);
        }
    }
}
