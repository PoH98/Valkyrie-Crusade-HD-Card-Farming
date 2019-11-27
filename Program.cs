using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text.RegularExpressions;
using System.Threading;

namespace HDCardDownloader
{
    class Program
    {
        private static long lastunixtime = 1360920650, EstimateLoopTimes;
        private static List<string> found_cd = new List<string>();
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                if (!Directory.Exists("thumb") && !File.Exists("thumb.txt"))
                {
                    Console.WriteLine("Please pull the thumb folder from /sdcard/Android/data/com.nubee.valkyriecrusade/files/card and put it into here to continue!");
                    Console.ReadLine();
                    return;
                }
                if (File.Exists("url.txt"))
                {
                    var templist = File.ReadAllLines("url.txt");
                    found_cd = templist.Select(cd => { 
                        string tempcd = cd.Remove(0, cd.LastIndexOf('/') + 1);
                        tempcd = tempcd.Remove(tempcd.LastIndexOf("."));
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Url had gained as " + cd);
                        return tempcd;
                    }).ToList();
                    var tempunix = templist.Select(link =>
                    {
                        return link.Substring(link.LastIndexOf(".") + 1);
                    }).ToArray();
                    Array.Sort(tempunix, templist);
                    lastunixtime = Convert.ToInt64(tempunix[tempunix.Length-1]);
                    File.WriteAllLines("url.txt", templist);
                }
                string[] cd_id = new string[] { };
                if (!File.Exists("thumb.txt"))
                {
                    cd_id = Directory.GetFiles("thumb").Select(card => card.Remove(0, card.LastIndexOf("\\") + 1)).ToArray();
                }
                else
                {
                    cd_id = File.ReadAllLines("thumb.txt");
                }
                Array.Sort(cd_id);
                File.WriteAllLines("thumb.txt", cd_id);
                long startTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                List<Thread> check = new List<Thread>();
                Console.ForegroundColor = ConsoleColor.Red;
                EstimateLoopTimes = (startTimeStamp - 1360920650) * cd_id.Length;
                long MaxLoopTimes = EstimateLoopTimes;
                EstimateLoopTimes = EstimateLoopTimes - ((lastunixtime - 1360920650) * cd_id.Length);
                Console.WriteLine("Esitmate loops times: " + MaxLoopTimes.ToString("###,###,###,###,###,###,###"));
                int threadnum = 5;
                do
                {
                    Console.WriteLine("How much thread you want to use?");
                    if(int.TryParse(Console.ReadLine(), out threadnum))
                    {
                        if(threadnum > 0)
                        {
                            break;
                        }
                        else
                        {
                            Console.WriteLine("You can't input a thread number less than 1!");
                        }
                    }
                }
                while (true);
                ServicePointManager.DefaultConnectionLimit = threadnum;
                ServicePointManager.UseNagleAlgorithm = true;
                ServicePointManager.Expect100Continue = false;
                ServicePointManager.CheckCertificateRevocationList = false;
                Console.WriteLine("Fetching previous event list...");
                WebClient wc = new WebClient();
                var listraw = wc.DownloadString("https://d2n1d3zrlbtx8o.cloudfront.net/news/info/en/index.html");
                List<DateTimeOffset> eventdate = findDate(listraw);
                Console.WriteLine("Event fetch completed. Loaded " + eventdate.Count + " events!");
                eventdate.Sort();
                for (long unixtime = lastunixtime; unixtime < startTimeStamp; unixtime++)
                {
                    while (check.Count(tc => tc.IsAlive) >= threadnum)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        double percentage = 100 - (((double)(EstimateLoopTimes * 100)) / (double)MaxLoopTimes);
                        Console.Write("Estimate loops left: " + EstimateLoopTimes.ToString("###,###,###,###,###,###,###") + ". Completed ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine(percentage.ToString("##0.00000") + "%");
                        Thread.Sleep(10000);
                    }
                    Thread.Sleep(100);
                    bool isInrange = false;
                    DateTimeOffset currentUnixDateTime = DateTimeOffset.FromUnixTimeSeconds(unixtime).ToUniversalTime();
                    for(int x = 0; x< eventdate.Count(); x++)
                    {
                        var eventsearchstart = eventdate[x].AddHours(-10).ToUniversalTime();
                        var eventsearchend = eventdate[x].AddHours(-8).ToUniversalTime();
                        if (eventsearchstart <= currentUnixDateTime)
                        {
                            if(currentUnixDateTime <= eventsearchend)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                Console.WriteLine("Start search at " + eventsearchstart + "-" + eventsearchend);
                                isInrange = true;
                                break;
                            }
                        }
                    }
                    if (isInrange)
                    {
                        int hour = currentUnixDateTime.Hour;
                        if (hour >= 1 && hour <= 14)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkBlue;
                            Console.Write("Starting Thread on ");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write(unixtime);
                            Console.ForegroundColor = ConsoleColor.DarkBlue;
                            Console.Write(". (");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write(currentUnixDateTime.ToString());
                            Console.ForegroundColor = ConsoleColor.DarkBlue;
                            Console.WriteLine(")");
                            long checkunix = unixtime;
                            while ((check.Count(td => td.Name == checkunix.ToString()) > 0))
                            {
                                checkunix++;
                            }
                            Thread t = new Thread(() =>
                            {
                                CheckFile(cd_id, unixtime);
                            });
                            t.Name = unixtime.ToString();
                            check.Add(t);
                            t.Start();
                        }
                        else
                        {
                            EstimateLoopTimes = EstimateLoopTimes - cd_id.Length;
                            continue;
                        }
                    }
                    else
                    {
                        unixtime = currentUnixDateTime.AddHours(1).ToUnixTimeSeconds();
                        Console.ForegroundColor = ConsoleColor.Green;
                        long reduced = ((cd_id.Length * 3600) + 3600);
                        Console.WriteLine("Skipping timestamp " + currentUnixDateTime + ". Reduced " + reduced + " loops.");
                        EstimateLoopTimes = EstimateLoopTimes - reduced;
                        continue;
                    }
                }
                if(found_cd.Count() < cd_id.Length)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Some files missed, retrying...");
                    lastunixtime = 1360920650;
                    check.Clear();
                    for (long unixtime = lastunixtime; unixtime < startTimeStamp; unixtime++)
                    {
                        while (check.Count(tc => tc.IsAlive) >= threadnum)
                        {
                            Thread.Sleep(10000);
                        }
                        Thread t = new Thread(() =>
                        {
                            Console.WriteLine("Gaining cards again from timestamp " + unixtime);
                            CheckFile(cd_id, unixtime);
                        });
                        check.Add(t);
                        t.Start();
                    }
                }
                Array.Resize(ref args, 1);
                args[0] = "-dl";
                Main(args);
            }
            else
            {
                if (args[0] == "-d")
                {
                    string path = "Download";
                    if(args.Length > 1)
                    {
                        path = args[1];
                    }
                    if (!Directory.Exists("Decrypted"))
                    {
                        Directory.CreateDirectory("Decrypted");
                    }
                    if (!Directory.Exists(path))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Path not exist! Am I a joke for you?");
                        Console.ReadLine();
                        return;
                    }
                    foreach (var filename in Directory.GetFiles(path))
                    {
                        FileStream fileStream = new FileStream(filename, FileMode.Open);
                        byte[] array = new byte[fileStream.Length];
                        fileStream.Read(array, 0, (int)fileStream.Length);
                        uint num = (uint)fileStream.Length;
                        bool flag = false;
                        if (array[0] == 67 && array[1] == 79 && array[2] == 68 && array[3] == 69)
                        {
                            flag = true;
                        }
                        if (flag)
                        {
                            fileStream.Close();
                            uint num2 = BitConverter.ToUInt32(array, 12);
                            uint num3 = 1169124957u;
                            byte[] array2 = new byte[array.Length - 16];
                            int num4 = array2.Length / 4;
                            for (int i = 0; i < num4; i++)
                            {
                                uint num5 = BitConverter.ToUInt32(array, 16 + i * 4);
                                uint value = (num5 ^ num3) - num2;
                                byte[] bytes = BitConverter.GetBytes(value);
                                bytes.CopyTo(array2, i * 4);
                            }
                            byte[] array3 = new byte[4];
                            int num6 = 0;
                            for (int i = 16 + num4 * 4; i < array.Length; i++)
                            {
                                array3[num6] = array[i];
                                num6++;
                            }
                            byte[] array4 = new byte[num6];
                            for (int i = 0; i < num6; i++)
                            {
                                array4[i] = array3[i];
                            }
                            array4.CopyTo(array2, num4 * 4);
                            string str = ".dat";
                            if (array2[0] == 137 && array2[1] == 80 && array2[2] == 78 && array2[3] == 71)
                            {
                                str = ".png";
                            }
                            byte[] buffer = array2;
                            FileStream output = new FileStream("Decrypted"+filename.Remove(0, filename.LastIndexOf("\\")) + str, FileMode.OpenOrCreate);
                            BinaryWriter binaryWriter = new BinaryWriter(output);
                            binaryWriter.Write(buffer);
                            binaryWriter.Close();
                        }
                    }
                }
                else if (args[0] == "-dl")
                {
                    if (File.Exists("url.txt"))
                    {
                        var link = File.ReadAllLines("url.txt");
                        if (!Directory.Exists("Download"))
                        {
                            Directory.CreateDirectory("Download");
                        }
                        foreach(var l in link)
                        {
                            string filename = "Download\\"+l.Substring(l.LastIndexOf("/"));
                            if (File.Exists(filename))
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Filename " + filename + " was downloaded. Skipping...");
                                continue;
                            }
                            Retry:
                            try
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Download Start on url " + l + " by WebClient");
                                WebClient wc = new WebClient();
                                wc.DownloadFile(l, filename);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Download Completed on url " + l + " by WebClient");
                                wc.Dispose();
                            }
                            catch
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Exception found! Download failed on url "+ l + ". Retrying...");
                                goto Retry;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("url.txt not found! Please run this program again without arguments to gain url.txt!");
                        Console.ReadLine();
                        return;
                    }
                    args[0] = "-d";
                    Main(args);
                }
            }
        }
        private static void CheckFile(string[] cd_id, long unixtime)
        {
            bool found = false;
            int tried = 0;
            foreach (var card in cd_id)
            {
                EstimateLoopTimes--;
                if (found_cd.Contains(card))
                {
                    continue;
                }
                if (found)
                {
                    tried++;
                    if(tried >= 100)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("No cards on " + unixtime + " anymore!");
                        EstimateLoopTimes = EstimateLoopTimes - (cd_id.Length - Array.IndexOf(cd_id, card));
                        return;
                    }
                }
                Uri.TryCreate("https://d2n1d3zrlbtx8o.cloudfront.net/download/CardHD.zip/" + card + "." + unixtime.ToString("0000000000"), UriKind.RelativeOrAbsolute, out Uri url);
                Retry:
                try
                {
                    if (RemoteFileExists(url))
                    {
                        File.AppendAllText("url.txt", url + "\n");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Url found on " + url + " by webclient");
                        found_cd.Add(card);
                        found = true;
                    }
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Unknown Exception found, retrying access...");
                    goto Retry;
                }
            }
            Console.ForegroundColor = ConsoleColor.Magenta;
            DateTime currentUnixDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            currentUnixDateTime = currentUnixDateTime.AddSeconds(unixtime);
            Console.Write("Reset thread as there is no card found on ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(unixtime);
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(" unix time. ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("(DateTime: " + currentUnixDateTime.ToString() + ")");
        }

        private static bool RemoteFileExists(Uri url)
        {
            try
            {
                HttpWebRequest request = WebRequest.CreateHttp(url);
                request.UserAgent = "???";
                request.Proxy = null;
                request.AllowAutoRedirect = false;
                request.KeepAlive = true;
                request.UnsafeAuthenticatedConnectionSharing = true;
                request.AuthenticationLevel = AuthenticationLevel.None;
                request.AllowReadStreamBuffering = false;
                request.AllowWriteStreamBuffering = false;
                HttpWebResponse response = request.GetResponseNoException();
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error accesssing server on url " + url);
                //Any exception will returns false.
                return false;
            }
        }
        private static List<DateTimeOffset> findDate(string url)
        {
            var list = url.Split('\n');
            List<DateTimeOffset> reallist = new List<DateTimeOffset>();
            CultureInfo japan = new CultureInfo("ja-JP");
            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            foreach (var l in list)
            {
                if (l.Contains("span class=\"iro5\">"))
                {
                    string date = l.Remove(0, 19).Replace("</span><BR>", "").Replace("</span><br />", "").Replace("\r","");
                    int day = Convert.ToInt32(date.Split('.')[0]);
                    int month = Convert.ToInt32(date.Split('.')[1]);
                    int year = Convert.ToInt32(date.Split('.')[2]);
                    if(month > 12)
                    {
                        //it should be day
                        var tempm = month;
                        month = day;
                        day = tempm;
                    }
                    date = day.ToString("00") + "/" + month.ToString("00") + "/" + year.ToString("0000") ;
                    reallist.Add(TimeZoneInfo.ConvertTimeFromUtc(DateTime.ParseExact(date, "dd/MM/yyyy", japan), cstZone));
                }
            }
            return reallist;
        }
    }
    public static class HttpWebResponseExt
    {
        public static HttpWebResponse GetResponseNoException(this HttpWebRequest req)
        {
            try
            {
                return (HttpWebResponse)req.GetResponse();
            }
            catch (WebException we)
            {
                var resp = we.Response as HttpWebResponse;
                if (resp == null)
                    throw;
                return resp;
            }
        }
    }
}
