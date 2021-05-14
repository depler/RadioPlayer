using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace RadioPlayer.Code
{    
    public static class Radio
    {
        private static async Task<string> DownloadUrl(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            using var response = await request.GetResponseAsync();
            using var stream = response.GetResponseStream();
            using var reader = new StreamReader(stream);

            return await reader.ReadToEndAsync();
        }

        private static string ParseRadioDachaHtml(string html, int bitrate)
        {
            var pattern1 = "<input id=\"kbps64\"";
            int pos = html.IndexOf(pattern1);
            if (pos < 0)
                return null;

            var pattern2 = "value=\"";
            pos = html.IndexOf(pattern2, pos + pattern1.Length);
            if (pos < 0)
                return null;

            int urlStart = pos + pattern2.Length;
            pos = html.IndexOf('"', urlStart);
            if (pos < 0)
                return null;

            int urlLength = pos - urlStart;
            var url = html.Substring(urlStart, urlLength);
            url = url.Replace("dacha_64", $"dacha_{bitrate}");

            return url;
        }

        public static async Task<string> GetRadioDachaUrl(int region, int bitrate)
        {            
            var html = await DownloadUrl($"http://www.radiodacha.ru/player.htm?region={region}");
            var url = ParseRadioDachaHtml(html, bitrate);
            return url;
        }
    }
}