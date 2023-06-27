using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CQCopyPasteAdapter.Logging
{
    /// <summary>
    /// 可用于Wpf，亦或任何带有磁盘的应用程序的日志处理程序，他会在日志落盘的同时，尝试同步一个服务器上
    /// </summary>
    [UsedImplicitly]
    public class Logger : IDisposable
    {
        public class LogReceivedEventArgs : EventArgs
        {
            public String LogLevel { get; set; }
            public String Message { get; set; }
        }

        /// <summary>
        /// 获取当前的全局日志提供程序
        /// </summary>
        [UsedImplicitly] public static Logger Current { get; } = new();

        public EventHandler OnLog;

        private readonly Dictionary<DateTime, string> _alertHistory = new();
        private readonly Dictionary<DateTime, string> _reportHistory = new();
        private DateTime _lastTimeReport;
        private DateTime _changeLevelTime;
        private int _level;
        private int _tenReports;

        private readonly object _lockObject = new();
        private Task _logTask = Task.CompletedTask;

        Logger()
        {
            try
            {
                //检查文件夹
                if (!Directory.Exists("logs"))
                {
                    Directory.CreateDirectory("logs");
                }
            }
            catch (Exception exp)
            {
                WriteToDisk("Critical", "无法创建文件夹" + exp.Message, "Ctor");
            }

            try
            {
                //执行清理
                var files = Directory.GetFiles("logs");
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < DateTime.Today.AddDays(-60))
                    {
                        try
                        {
                            fileInfo.Delete();
                        }
                        catch (Exception exp1)
                        {
                            WriteToDisk("Critical", "无法删除文件" + exp1.Message, "Ctor");
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                WriteToDisk("Critical", "无法执行清理" + exp.Message, "Ctor");
            }
        }

        /// <summary>
        /// 一个用于格式化异常的帮助函数，可以将异常的StackTrace，InnerException等都格式化到输出的字符串中。
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [UsedImplicitly]
        public String FormatException(Exception exp, String message)
        {
            try
            {
                var msg = exp.Message;
                var trace = exp.StackTrace;

                var expStack = new List<Exception> { exp };

                while (exp.InnerException != null && !expStack.Contains(exp.InnerException))
                {
                    exp = exp.InnerException;
                    msg += "[InnerException]" + exp.Message;
                    expStack.Add(exp);
                }



                return message + msg + trace;
            }
            catch (Exception)
            {
                return exp.Message;
            }
        }

        /// <summary>
        /// Log记录的内容会放在磁盘上，并定期清理，对应的日志级别为Information
        /// </summary>
        /// <param name="content"></param>
        /// <param name="callerName"></param>
        [UsedImplicitly]
        public void Log(String content, [CallerMemberName] string callerName = "UnknownMethod")
        {
            Task.Run(() =>
            {
                lock (_lockObject)
                {
                    _logTask = _logTask.ContinueWith(_ =>
                    {
                        try
                        {
                            WriteToDisk("Information", content, callerName);
                            OnLog?.Invoke(this,new LogReceivedEventArgs(){LogLevel = "Information",Message = content});
                        }
                        catch (Exception)
                        {
                            //
                        }
                    });
                }

            });
        }

        /// <summary>
        /// Report的内容会放在磁盘上，定期清理。此外，每隔60分钟或者10条Report，会将内容推送到服务器一次。对应的日志级别为Warning。
        /// </summary>
        /// <param name="content"></param>
        /// <param name="callerName"></param>
        /// <returns></returns>
        [UsedImplicitly]
        public void Report(String content, [CallerMemberName] string callerName = "UnknownMethod")
        {
            Task.Run(() =>
            {
                lock (_lockObject)
                {
                    _logTask = _logTask.ContinueWith(_ =>
                    {
                        try
                        {
                            lock (_reportHistory)
                            {
                                _reportHistory.Add(DateTime.Now, content);
                            }

                            WriteToDisk("Warning", content, callerName);

                            Dictionary<DateTime, string> contents = null;

                            lock (_reportHistory)
                            {

                                if (_reportHistory.Count > 10 ||
                                    _reportHistory.Keys.Min() < DateTime.Now.AddMinutes(-60))
                                {
                                    contents = _reportHistory.ToDictionary(p => p.Key, p => p.Value);
                                    _reportHistory.Clear();
                                }
                            }

                            if (contents != null)
                            {
                                ReportToServer("Warning", contents, callerName);
                            }

                            OnLog?.Invoke(this, new LogReceivedEventArgs() { LogLevel = "Warning", Message = content });
                        }
                        catch (Exception)
                        {
                            //
                        }
                    });
                }

            });
        }

        /// <summary>
        /// Alert函数传入的内容，会立刻通过网络推送到服务器，此外也会放在磁盘上，并定期清理。对应的日志级别为Error。
        /// Alert的汇报频率会进行降级，初始为0级，10s超过10条则降到1级，每10秒才推送到服务器一次。
        /// 1级后，一小时内如果1级汇报每次包含10条以上的情况超过10次，第11次时降到2级，每一小时汇报一次，
        /// 否则，一小时后升回0级。
        /// 2级固定在6小时后降级回1级。
        /// </summary>
        /// <param name="content"></param>
        /// <param name="callerName"></param>
        /// <returns></returns>
        [UsedImplicitly]
        public void Alert(String content, [CallerMemberName] string callerName = "UnknownMethod")
        {
            Task.Run(() =>
            {
                lock (_lockObject)
                {
                    _logTask = _logTask.ContinueWith(_ =>
                    {
                        try
                        {

                            WriteToDisk("Error", content, callerName);

                            var tenSecondsAgo = DateTime.Now - TimeSpan.FromSeconds(10);

                            Dictionary<DateTime, string> contents = new Dictionary<DateTime, string>();

                            lock (_alertHistory)
                            {

                                switch (_level)
                                {
                                    case 0:

                                        _alertHistory.Add(DateTime.Now, content);
                                        contents.Add(DateTime.Now, content);

                                        var keys = _alertHistory.Keys;

                                        var keyList = new List<DateTime>();

                                        foreach (var key in keys)
                                        {
                                            if (key < tenSecondsAgo)
                                            {
                                                keyList.Add(key);
                                            }
                                        }

                                        foreach (var deleteKey in keyList)
                                        {
                                            _alertHistory.Remove(deleteKey);
                                        }

                                        if (_alertHistory.Keys.Count > 10)
                                        {
                                            _level = 1;

                                            _changeLevelTime = DateTime.Now;
                                            _alertHistory.Clear();
                                            break;
                                        }

                                        _lastTimeReport = DateTime.Now;
                                        break;
                                    case 1:
                                        _alertHistory.Add(DateTime.Now, content);
                                        if (tenSecondsAgo > _lastTimeReport)
                                        {
                                            contents = _alertHistory.ToDictionary(p => p.Key, p => p.Value);
                                            _lastTimeReport = DateTime.Now;
                                            if (_alertHistory.Keys.Count > 10)
                                            {
                                                _tenReports++;
                                            }

                                            if (_tenReports >= 10 && DateTime.Now - TimeSpan.FromHours(1) < _changeLevelTime)
                                            {
                                                _level = 2;
                                                _changeLevelTime = DateTime.Now;
                                                _tenReports = 0;
                                            }
                                            else if (DateTime.Now - TimeSpan.FromHours(1) > _changeLevelTime)
                                            {
                                                _level = 0;
                                                _tenReports = 0;
                                            }

                                            _alertHistory.Clear();
                                        }

                                        break;
                                    case 2:
                                        _alertHistory.Add(DateTime.Now, content);
                                        if (DateTime.Now - TimeSpan.FromHours(1) > _lastTimeReport)
                                        {
                                            contents = _alertHistory.ToDictionary(p => p.Key, p => p.Value);
                                            _lastTimeReport = DateTime.Now;
                                        }

                                        if (DateTime.Now - _changeLevelTime > TimeSpan.FromHours(6))
                                        {
                                            _level = 1;
                                            _changeLevelTime = DateTime.Now;
                                        }

                                        _alertHistory.Clear();
                                        break;
                                }

                            }

                            if (contents.Count > 0)
                            {
                                ReportToServer("Error", contents, callerName);
                            }

                            OnLog?.Invoke(this, new LogReceivedEventArgs() { LogLevel = "Error", Message = content });
                        }
                        catch (Exception)
                        {
                            //
                        }
                    });
                }

            });
        }

        private void WriteToDisk(String folder, String content, string source)
        {
            try
            {
                var filename = "logs\\log." + folder + "." + DateTime.Now.ToString("yyyyMMdd") + ".log";
                File.AppendAllText(filename, Environment.NewLine + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + source + "\t" + content);
            }
            catch (Exception)
            {
                // ignored
                //如果写入磁盘都出错，就没有可以回退的地方了，只能忽略错误
            }
        }

        protected virtual void ReportToServer(String folder, Dictionary<DateTime, string> logs, String topic)
        {

        }

        /// <summary>
        /// 析构函数，用于在程序退出前最后上传一次日志
        /// </summary>
        ~Logger()
        {
            ReleaseUnmanagedResources();
        }

        private void ReleaseUnmanagedResources()
        {
            Dictionary<DateTime, string>? contents = null;

            lock (_reportHistory)
            {
                if (_reportHistory.Count > 0)
                {
                    contents = _reportHistory.ToDictionary(p => p.Key, p => p.Value);
                }
            }

            if (contents != null)
            {
                ReportToServer("Warning", contents, "~Ctor");
            }

            contents = null;

            lock (_alertHistory)
            {
                if (_alertHistory.Count > 0)
                {
                    contents = _alertHistory.ToDictionary(p => p.Key, p => p.Value);
                }
            }

            if (contents != null)
            {
                ReportToServer("Error", contents, "~Ctor");
            }
        }

        /// <summary>
        /// 在程序退出前最后上传一次日志。（可重入）
        /// </summary>
        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }
    }
}
