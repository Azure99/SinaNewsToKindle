﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Timers;
using RssToKindle.Model;
using RssToKindle.Parser;
using RssToKindle.Parser.PageParser;
using RssToKindle.Utils;

namespace RssToKindle.Controller
{
    class MainService
    {
        private Timer _timer;
        /// <summary>
        /// 上次推送时间
        /// </summary>
        private DateTime lastSendTime = ConfigManager.Config.LastSendTime;
        public MainService()
        {
            _timer = new Timer();
            _timer.Enabled = false;
            _timer.Interval = 20000;
            _timer.Elapsed += new ElapsedEventHandler(WorkEvent);
        }

        ~MainService()
        {
            _timer.Dispose();
        }

        public void StartService()
        {
            _timer.Start();
        }

        public void StopService()
        {
            _timer.Stop();
        }

        private void WorkEvent(object sender, ElapsedEventArgs e)
        {
            LogManager.WriteLine("Checking time...");
            try
            {
                Work();
            }
            catch (Exception ex)
            {
                LogManager.ShowException(ex);
                if (!_timer.Enabled)
                {
                    _timer.Start();
                }
            }
        }

        private void Work()
        {
            Config config = ConfigManager.Config;
            DateTime nowDate = DateTime.Parse(DateTime.Now.ToShortDateString());
            DateTime lastDate = DateTime.Parse(config.LastSendTime.ToShortDateString());
            DateTime nowTime = DateTime.Parse(DateTime.Now.ToShortTimeString());
            DateTime sendTime = DateTime.Parse(config.SendTime);

            if (nowDate > lastDate && nowTime > sendTime)
            {
                GetNewsAndSendToKindle();
            }
        }
        /// <summary>
        /// 抓取并推送
        /// </summary>
        public void GetNewsAndSendToKindle()
        {
            _timer.Stop();


            NewsBody[] bodies = CrawleNews();

            LogManager.WriteLine("Build news html page...");
            string html = BuildHTML(bodies);

            string filePath = DateTime.Now.ToString("yy-MM-dd") + "News.html";
            try
            {
                File.WriteAllText(filePath, html, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                LogManager.ShowException(ex, "Cannot create news file, push failed!");
                return;
            }

            LogManager.WriteLine("Send mail to kindle...");
            SendEmail(filePath);

            LogManager.WriteLine("All done!");
            LogManager.WriteLine("----------");

            ConfigManager.Config.LastSendTime = DateTime.Now;
            ConfigManager.SaveConfig();

            _timer.Start();
        }

        /// <summary>
        /// 抓取新闻
        /// </summary>
        /// <returns></returns>
        private NewsBody[] CrawleNews()
        {
            LogManager.WriteLine("Crawle news rss...");

            List<NewsHeader> headers = new List<NewsHeader>();
            foreach (string xmlUrl in ConfigManager.Config.RssUrls)
            {
                try
                {
                    string xml = Client.GET(xmlUrl);
                    headers.AddRange(RssParser.Parse(xml));
                }
                catch (Exception ex)
                {
                    LogManager.ShowException(ex, "Cannot get " + xmlUrl);
                }
            }


            LogManager.WriteLine("Crawle news body...");
            int count = 0;

            List<NewsBody> bodyList = new List<NewsBody>();
            foreach (NewsHeader header in headers)
            {
                count++;
                if (count % 10 == 0)
                {
                    LogManager.WriteLine(string.Format("<{0}> items done...", count));
                }
                try
                {
                    NewsBody body = HeaderParser.Parse(header);

                    System.Diagnostics.Debug.WriteLine("标题");
                    System.Diagnostics.Debug.WriteLine(body.Title);
                    System.Diagnostics.Debug.WriteLine("简介");
                    System.Diagnostics.Debug.WriteLine(body.Description);
                    System.Diagnostics.Debug.WriteLine("内容");
                    System.Diagnostics.Debug.WriteLine(body.Content);

                    bodyList.Add(body);
                }
                catch(Exception ex)
                {
                    LogManager.ShowException(ex);
                }

            }

            return bodyList.ToArray();
        }

        /// <summary>
        /// 构建适合Kindle阅读的HTML
        /// </summary>
        /// <param name="bodies"></param>
        /// <returns></returns>
        private string BuildHTML(NewsBody[] bodies)
        {
            KindleHtmlPageBuilder builder = new KindleHtmlPageBuilder();

            foreach (NewsBody body in bodies)
            {
                builder.AddNews(body);
            }

            return builder.GetHtml();
        }

        /// <summary>
        /// 发送到Kindle
        /// </summary>
        /// <param name="path"></param>
        /// <param name="maxTry"></param>
        private void SendEmail(string path, int maxTry = 2)
        {
            while (maxTry-- > 0)
            {
                try
                {
                    Config config = ConfigManager.Config;
                    EmailSender sender = new EmailSender(
                        config.SenderAddress,
                        config.SenderPassword,
                        config.EmailServer,
                        config.EmailPort,
                        config.EnableSSL);

                    string title = "convert";
                    if (ConfigManager.Config.DynamicTitle)
                    {
                        title = Path.GetFileNameWithoutExtension(path) + new Random().Next(0, 99);
                    }

                    sender.SendMail(config.ReceiverAddress, title, "", path);
                    break;
                }
                catch (Exception ex)
                {
                    LogManager.ShowException(ex, "Send email failed!");
                }
            }
        }
    }
}