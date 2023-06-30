using CQCopyPasteAdapter.Logging;
using CQCopyPasteAdapter.Storage;
using System;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Threading;

namespace CQCopyPasteAdapter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static SqliteKvStore<NotifiedDictionary<string, string>> QQWindows { get; private set; } = null!;
        public static SqliteKvStore<string> Settings { get; private set; } = null!;
        public static Dispatcher? PublicDispatcher { get; set; }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            //读取Sqlite
            string dbFile = @"Data Source=CQCopyPasteData.db";
            SQLiteConnection cnn = new SQLiteConnection(dbFile);
            cnn.Open();

            QQWindows = new SqliteKvStore<NotifiedDictionary<String, String>>(cnn, "QQWindows");
            Settings = new SqliteKvStore<String>(cnn, "Settings");

            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            QQOperator.StartMainLoop();
        }


        void Current_DispatcherUnhandledException(object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            HandledException(e.Exception);
            e.Handled = true;
        }

        private static void HandledException(Exception exp)
        {
            Logger.Current.Alert(Logger.Current.FormatException(exp, "应用程序发生未知异常"));
        }
    }
}
