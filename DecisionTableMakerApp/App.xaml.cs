using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;

namespace DecisionTableMakerApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // UIスレッドの未処理例外
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            // 非UIスレッド（Taskなど）の未処理例外
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Task系（async/await）の未観測例外
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            base.OnStartup(e);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"UIスレッドで例外が発生しました: {e.Exception.Message}");
            e.Handled = true; // アプリを強制終了させない
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            MessageBox.Show($"非UIスレッドで例外が発生しました: {ex?.Message}");
            // ※この後はプロセスが終了する可能性があります
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            MessageBox.Show($"非同期タスクで例外が発生しました: {e.Exception.Message}");
            e.SetObserved(); // 強制終了を防ぐ
        }
    }

}
