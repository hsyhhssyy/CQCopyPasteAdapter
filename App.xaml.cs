using CQCopyPasteAdapter.Logging;
using CQCopyPasteAdapter.Storage;
using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Windows;
using CQCopyPasteAdapter.WebApi;
using System.Net;
using System.Windows.Threading;

namespace CQCopyPasteAdapter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static SqliteKvStore<NotifiedDictionary<String, String>> QQWindows { get; private set; }
        public static SqliteKvStore<string> Settings { get; set; }
        public static Dispatcher PublicDispatcher { get; set; }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            //读取Sqlite
            string dbfile = @"Data Source=CQCopyPasteData.db";
            SQLiteConnection cnn = new SQLiteConnection(dbfile);
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

        public static void HandledException(Exception exp)
        {
            Logger.Current.Alert(Logger.Current.FormatException(exp, "应用程序发生未知异常"));
        }
    }
}
