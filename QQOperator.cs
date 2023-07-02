using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using CQCopyPasteAdapter.Helpers;
using CQCopyPasteAdapter.Logging;
using Newtonsoft.Json;

namespace CQCopyPasteAdapter
{
    public static class QQOperator
    {
        private class MessageBatch
        {
            public string? ChannelId { get; init; }
            public List<Dictionary<String, object>> Messages { get; } = new();
        }

        private static readonly Object Lock=new();
        private static readonly List<String> ProcessedMessage = new();

        private static Task _qqTask = Task.CompletedTask;


        public static void StartMainLoop()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {

                        var url = App.Settings.GetValueOrSetDefault("ServerUrl", "");
                        var token = App.Settings.GetValueOrSetDefault("ServerToken", "");

                        if (url == "" || token == "")
                        {
                            Thread.Sleep(1000);
                            continue;
                        }

                        var messageToProcess = new List<Dictionary<string, object>>();
                        while (true)
                        {
                            var error = HttpHelper.PostAction(url.TrimEnd('/') + "/kidnapper/getmessage",
                                JsonConvert.SerializeObject(new Dictionary<string, string>()),
                                new Dictionary<string, string>()
                                {
                                    { "authKey", token }
                                }).GetResponseData(out var getTaskResponse);

                            if (error != null)
                            {
                                Logger.Current.Report("联网获取消息报错:" + error);
                                Thread.Sleep(1000);
                                continue;
                            }

                            if (!getTaskResponse.ContainsKey("messages"))
                            {
                                Logger.Current.Report("联网获取消息报错。");
                                Thread.Sleep(1000);
                                continue;
                            }

                            var msgInThisLoop = new List<Dictionary<string, object>>();

                            if (getTaskResponse["messages"] is List<object> messageList)
                            {
                                foreach (var message in messageList.OfType<Dictionary<String, object>>())
                                {
                                    if (ProcessedMessage.Contains(message.GetValueOrDefault("uuid")?.ToString() ?? ""))
                                    {
                                        continue;
                                    }

                                    msgInThisLoop.Add(message);
                                    ProcessedMessage.Add(message.GetValueOrDefault("uuid")?.ToString() ?? "");
                                }
                            }

                            messageToProcess.AddRange(msgInThisLoop);

                            if (msgInThisLoop.Count == 0 || messageToProcess.Count > 20)
                            {
                                break;
                            }
                        }

                        var batchToAdd = new List<MessageBatch>();

                        foreach (var message in messageToProcess)
                        {
                            var channelId = message.GetValueOrDefault("channel_id")?.ToString();
                            if (batchToAdd.All(b => b.ChannelId != channelId))
                            {
                                batchToAdd.Add(new MessageBatch()
                                {
                                    ChannelId = channelId
                                });
                            }

                            var batch = batchToAdd.First(b => b.ChannelId == channelId);


                            switch (message.GetValueOrDefault("type"))
                            {
                                case "Text":
                                case "At":
                                case "Image":
                                    batch.Messages.Add(message);
                                    break;
                                case "MessageBreak":
                                    batch.Messages.Add(message);
                                    lock (Lock)
                                    {
                                        _qqTask = _qqTask.ContinueWith(_ =>
                                        {
                                            SendMessage(batch.ChannelId, batch.Messages);
                                        });
                                    }

                                    batchToAdd.Remove(batch);
                                    break;
                            }
                        }

                        Thread.Sleep(500);
                    }
                    catch (Exception e)
                    {
                        Logger.Current.Log(Logger.Current.FormatException(e, "接收消息出错"));
                    }
                }
                // ReSharper disable once FunctionNeverReturns
            });
        }

        private static IntPtr GetHwndFromChannel(string channel)
        {
            if (App.QQWindows.ContainsKey(channel))
            {
                var hwndStr = App.QQWindows[channel]["HWND"];
                if (!String.IsNullOrWhiteSpace(hwndStr) && IntPtr.TryParse(hwndStr, out var hwnd))
                {
                    return hwnd;
                }
            }

            return IntPtr.Zero;
        }

        private static void Delay(int time=500)
        {
            Task.Delay(time).Wait();
        }

        private static void SendMessage(String channel, List<Dictionary<String,object>> batch)
        {
            if (string.IsNullOrWhiteSpace(channel)||batch.Count==0)
            {
                Logger.Current.Report("消息无法发送，目标频道为空/目标消息为空。");
                return;
            }

            try
            {
                var hwnd = GetHwndFromChannel(channel);

                if (hwnd != IntPtr.Zero)
                {
                    App.PublicDispatcher?.Invoke(() =>
                    {
                        WindowHelper.BringWindowToFront(hwnd);

                        //删掉文本框中现有的内容
                        WindowHelper.SendCtrlA();
                        Delay();
                        WindowHelper.SendDelete();
                        Delay();

                        bool lastMsgIsBreak = false;

                        foreach (var message in batch)
                        {
                            lastMsgIsBreak = false;

                            switch (message.GetValueOrDefault("type"))
                            {
                                case "Image":
                                    var base64ImageData = message.GetValueOrDefault("data")?.ToString();
                                    if (!String.IsNullOrWhiteSpace(base64ImageData))
                                    {
                                        byte[] imageBytes = Convert.FromBase64String(base64ImageData);

                                        //var file = new FileInfo("TempImage.png");

                                        using (var stream = new MemoryStream(imageBytes))
                                        {
                                            stream.Position = 0; // Reset the stream position
                                            using (var image = Image.FromStream(stream))
                                            {
                                                //image.Save(file.FullName, System.Drawing.Imaging.ImageFormat.Png);
                                                var bitmap = new Bitmap(image);
                                                var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                                                    bitmap.GetHbitmap(),
                                                    IntPtr.Zero,
                                                    Int32Rect.Empty,
                                                    BitmapSizeOptions.FromEmptyOptions());

                                                Clipboard.Clear();
                                                Clipboard.SetImage(bitmapSource);
                                            }
                                        }

                                        WindowHelper.SendCtrlV();
                                        Delay();
                                    }

                                    break;
                                case "Text":
                                    var text = message.GetValueOrDefault("data")?.ToString();
                                    if (!String.IsNullOrWhiteSpace(text))
                                    {
                                        Clipboard.Clear();
                                        Clipboard.SetText(text);
                                        WindowHelper.SendCtrlV();
                                        Delay();
                                    }

                                    break;
                                case "At":
                                    var target = message.GetValueOrDefault("data")?.ToString();
                                    if (!String.IsNullOrWhiteSpace(target))
                                    {
                                        Clipboard.Clear();
                                        Clipboard.SetText("@");
                                        WindowHelper.SendCtrlV();
                                        Delay();

                                        Clipboard.Clear();
                                        Clipboard.SetText(target);
                                        WindowHelper.SendCtrlV();
                                        Delay();

                                        WindowHelper.SendEnter();
                                        Delay();
                                    }

                                    break;

                                case "MessageBreak":
                                    lastMsgIsBreak = true;
                                    WindowHelper.SendCtrlEnter();
                                    break;
                            }
                        }

                        if (lastMsgIsBreak == false)
                        {
                            WindowHelper.SendCtrlEnter();
                        }

                        Logger.Current.Report($"频道{channel}的消息已发送。");
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(Logger.Current.FormatException(ex, "发送消息出错"));
            }
        }
    }
}
