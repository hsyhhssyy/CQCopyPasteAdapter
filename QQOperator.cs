﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CQCopyPasteAdapter.Helpers;
using CQCopyPasteAdapter.Logging;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using static CQCopyPasteAdapter.QQOperator;

namespace CQCopyPasteAdapter
{
    public static class QQOperator
    {
        public class MessageBatch
        {
            public string ChannelId { get; set; }
            public List<Dictionary<String, object>> Messages { get; set; } = new List<Dictionary<string, object>>();
        }

        private static Object Lock=new Object();
        private static Task QQTask = Task.CompletedTask;
        private static List<String> processedTask = new List<String>();


        public static void StartMainLoop()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    var url = App.Settings.GetValueOrSetDefault("ServerUrl","");
                    var token = App.Settings.GetValueOrSetDefault("ServerToken", "");

                    if (url == ""|| token == "")
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    var error = HttpHelper.PostAction(url.TrimEnd('/') + "/kidnapper/getmessage", JsonConvert.SerializeObject(new Dictionary<string, string>()
                    {
                    }), new Dictionary<string, string>()
                    {
                        {"authKey",token}
                    }).GetResponseData(out var getTaskResponse);

                    if (error != null)
                    {
                        Logger.Current.Report("联网获取消息报错:" + error);
                        continue;
                    }

                    if (!getTaskResponse.ContainsKey("messages"))
                    {
                        Logger.Current.Report("联网获取消息报错。");
                        continue;
                    }

                    if (getTaskResponse["messages"] is List<object> messageList)
                    {
                        var batchToAdd = new List<MessageBatch>();

                        foreach (var message in messageList.OfType<Dictionary<String, object>>())
                        {
                            if (processedTask.Contains(message.GetValueOrDefault("uuid")?.ToString() ?? ""))
                            {
                                continue;
                            }

                            processedTask.Add(message.GetValueOrDefault("uuid")?.ToString() ?? "");

                            var channelId = message.GetValueOrDefault("channel_id")?.ToString();
                            if (!batchToAdd.Any(b=>b.ChannelId==channelId))
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
                            }
                        }

                        foreach (var batch in batchToAdd)
                        {
                            lock (Lock)
                            {
                                QQTask = QQTask.ContinueWith(_ =>
                                {
                                    SendMessage(batch.ChannelId, batch.Messages);
                                });
                            }
                        }

                    }


                    Thread.Sleep(500);
                }
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
            if (string.IsNullOrWhiteSpace(channel))
            {
                Logger.Current.Report("消息无法发送，目标频道为空。");
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

                        foreach (var message in batch)
                        {
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
                                            using (var image = System.Drawing.Image.FromStream(stream))
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
                            }
                        }

                        WindowHelper.SendCtrlEnter();
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
