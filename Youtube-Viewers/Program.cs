﻿using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Util;
using System.Windows.Forms;
using Youtube_Viewers.Helpers;

namespace Youtube_Viewers
{
    class Program
    {
        static string id;
        static int threadsCount;
        static ProxyScraper scraper;
        public static int proxyType = 0;
        static Random random = new Random();
        static Object locker = new Object();
        static Object loglocker = new Object();
        static int botted = 0;
        static int errors = 0;
        static int pos = 0;
        static bool holdViewers = true;


        static string intro = @"/_/\/_/\   /________/\ /_______/\     /_____/\     /________/\ 
\ \ \ \ \  \__.::.__\/ \::: _  \ \    \:::_ \ \    \__.::.__\/ 
 \:\_\ \ \    \::\ \    \::(_)  \/_    \:\ \ \ \      \::\ \   
  \::::_\/     \::\ \    \::  _  \ \    \:\ \ \ \      \::\ \  
    \::\ \      \::\ \    \::(_)  \ \    \:\_\ \ \      \::\ \ 
     \__\/       \__\/     \_______\/     \_____\/       \__\/ ";

        [STAThread]
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(intro);

            pos = Console.CursorTop;

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Enter Video ID: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            id = Console.ReadLine().Trim();

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Enter Threads Count: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            threadsCount = Convert.ToInt32(Console.ReadLine().Trim());

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Select proxy type:\r\n0. Public (Http/s autoscrape)\r\n1. Http/s\r\n2. Socks4\r\n3. Socks5");
                Console.Write("Your choice: ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                ConsoleKey k = Console.ReadKey().Key;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
                if (k == ConsoleKey.D0)
                {
                    Console.WriteLine("Selected public proxy");
                    proxyType = 0;
                    break;
                }
                if (k == ConsoleKey.D1)
                {
                    Console.WriteLine("Selected Http/s type of proxy");
                    proxyType = 1;
                    break;
                }
                else if (k == ConsoleKey.D2)
                {
                    Console.WriteLine("Selected Socks4 type of proxy");
                    proxyType = 2;
                    break;
                }
                else if (k == ConsoleKey.D3)
                {
                    Console.WriteLine("Selected Socks5 type of proxy");
                    proxyType = 3;
                    break;
                }
            }

            if(proxyType != 0)
            {
                Console.WriteLine("Open file with proxy list");

                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Proxy list (*.txt)|*.txt";

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                scraper = new ProxyScraper(dialog.FileName);
            }
                
            else
                scraper = new ProxyScraper();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Clear();
            Console.WriteLine(intro);

            List<Thread> threads = new List<Thread>();

            for(int i = 0; i < threadsCount; i++)
            {
                Thread t = new Thread(worker);
                t.Start();
                threads.Add(t);
            }

            Console.ReadKey();
        }

        static void log()
        {
            Console.SetCursorPosition(0, pos);
            Console.WriteLine($"\r\n\r\nBotted: {botted}\r\nErrors: {errors}\r\nProxies: {scraper.Proxies.Count}\r\nThreads: {threadsCount}\r\n");
        }

        static void worker(Object s)
        {
            while (true)
            {
                try
                {
                    using (HttpRequest req = new HttpRequest())
                    {
                        
                        HttpResponse res;
                        Proxy proxy;
                        string url;
                        string urlToGet;
                        string cl;
                        string ei;
                        string of;
                        string vm;

                        lock (locker)
                        {
                            if (proxyType == 0 && scraper.Time < (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds - 300) 
                                scraper.Scrape();
                            proxy = scraper.Next();
                        }

                        switch (proxyType)
                        {
                            case 0:
                            case 1:
                                req.Proxy = proxy.Http;
                                break;
                            case 2:
                                req.Proxy = proxy.Socks4;
                                break;
                            case 3:
                                req.Proxy = proxy.Socks5;
                                break;
                        }

                        req.UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 12_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.1 Mobile/15E148 Safari/604.1";

                        res = req.Get($"https://m.youtube.com/watch?v={id}");

                        url = res.ToString().Split(new string[] { "videostatsWatchtimeUrl\\\":{\\\"baseUrl\\\":\\\"" }, StringSplitOptions.None)[1];
                        url = url.Split(new string[] { "\\\"}" }, StringSplitOptions.None)[0];
                        url = url.Replace(@"\\u0026", "&").Replace("%2C", ",").Replace(@"\/", "/");

                        cl = url.Split(new string[] { "cl=" }, StringSplitOptions.None)[1].Split('&')[0];
                        ei = url.Split(new string[] { "ei=" }, StringSplitOptions.None)[1].Split('&')[0];
                        of = url.Split(new string[] { "of=" }, StringSplitOptions.None)[1].Split('&')[0];
                        vm = url.Split(new string[] { "vm=" }, StringSplitOptions.None)[1].Split('&')[0];

                        urlToGet = $"https://s.youtube.com/api/stats/watchtime?ns=yt&el=detailpage&cpn=isWmmj2C9Y2vULKF&docid={id}&ver=2&cmt=7334&ei={ei}&fmt=133&fs=0&rt=1003&of={of}&euri&lact=4418&live=dvr&cl={cl}&state=playing&vm={vm}&volume=100&c=MWEB&cver=2.20200313.03.00&cplayer=UNIPLAYER&cbrand=apple&cbr=Safari%20Mobile&cbrver=12.1.15E148&cmodel=iphone&cos=iPhone&cosver=12_2&cplatform=MOBILE&delay=5&hl=ru&cr=GB&rtn=1303&afmt=140&lio=1556394045.182&idpj=&ldpj=&rti=1003&muted=0&st=7334&et=7634";

                        req.AddHeader("Referrer", $"https://m.youtube.com/watch?v={id}");

                        req.Get(urlToGet);

                        lock (loglocker)
                        {
                            botted++;
                            log();
                        }
                    }
                }
                catch (Exception) 
                {
                    lock (locker)
                    {
                        errors++; 
                        log();
                    }
                    
                }
            }
        }
    }
}
