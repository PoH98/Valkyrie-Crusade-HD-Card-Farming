using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace HDCardDownloader
{
	// Token: 0x02000009 RID: 9
	internal class Program
	{
		// Token: 0x06000017 RID: 23 RVA: 0x000033E4 File Offset: 0x000015E4
		private static void Main(string[] args)
		{
			string[] urls = new string[]
			{
				"/wiki/Cards/Passion",
				"/wiki/Cards/Cool",
				"/wiki/Cards/Dark",
				"/wiki/Cards/Light",
				"/wiki/Cards/Special"
			};
			WebClient wc = new WebClient();
			List<string> parsedUrls = new List<string>();
			wc.BaseAddress = "https://valkyriecrusade.fandom.com";
			foreach (string url in urls)
			{
				Console.WriteLine("Parsing fandom wiki " + url);
				string html = wc.DownloadString(url);
				HtmlDocument doc = new HtmlDocument();
				doc.LoadHtml(html);
				foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//table//a"))
				{
					string hrefValue = link.GetAttributeValue("href", string.Empty);
					bool flag = hrefValue.Contains("http") || string.IsNullOrEmpty(hrefValue) || parsedUrls.Contains(hrefValue);
					if (!flag)
					{
						parsedUrls.Add(hrefValue);
						Console.WriteLine(hrefValue);
					}
				}
			}
			Console.WriteLine("Fetched " + parsedUrls.Count.ToString());
			bool flag2 = !Directory.Exists("Download");
			if (flag2)
			{
				Directory.CreateDirectory("Download");
			}
			wc.Dispose();
			IEnumerable<string> source = parsedUrls;
			ParallelOptions parallelOptions = new ParallelOptions();
			parallelOptions.MaxDegreeOfParallelism = 8;
			Parallel.ForEach<string>(source, parallelOptions, delegate (string parsed)
			{
				do
				{
					try
					{
						WebClient client = new WebClient();
						Console.WriteLine("Downloading " + parsed);
						client.BaseAddress = "https://valkyriecrusade.fandom.com";
						string html2 = client.DownloadString(parsed);
						HtmlDocument doc2 = new HtmlDocument();
						doc2.LoadHtml(html2);
						foreach (HtmlNode img in doc2.DocumentNode.SelectNodes("//img"))
						{
							string href = img.GetAttributeValue("src", string.Empty);
							string imgkey = img.GetAttributeValue("data-image-key", string.Empty);
							string fileName = Path.Combine("Download", imgkey);
							bool flag3 = string.IsNullOrEmpty(imgkey) || !imgkey.StartsWith(parsed.Split(new char[]
							{
							'/'
							}).Last<string>()) || href.Contains("data:image/gif");
							if (!flag3)
							{
								string realurl = href.Remove(href.IndexOf("/scale-to-width-down/"), "/scale-to-width-down/".Length + 3);
								client.DownloadFile(realurl, fileName);
								break;
							}
						}
						client.Dispose();
						break;
					}
					catch
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("Failed to download " + parsed + " retrying....");
						Console.ResetColor();
					}
				}
				while (true);
				
			});
			Console.WriteLine("Done!");
			Console.ReadLine();
		}
	}
}
