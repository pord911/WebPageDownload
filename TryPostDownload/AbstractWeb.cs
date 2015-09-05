using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Web;
using System.Collections.Specialized;
using HtmlAgilityPack;

namespace TryPostDownload
{
    public struct WebData
    {
        public string url;
        public string year;
        public string date;
    };

    public abstract class AbstractPage
    {

        protected  CookieContainer cookieContainer
        {
            get;
            set;
        }

        protected virtual string downloadHtml(string url)
        {
            HttpWebRequest client = (HttpWebRequest)WebRequest.Create(url);
            string downloadString = null;
            cookieContainer = new CookieContainer();

            client.CookieContainer = cookieContainer;
            try
            {
                var response = (HttpWebResponse)client.GetResponse();
                var sr = new StreamReader(response.GetResponseStream());
                downloadString = sr.ReadToEnd();
            }
            catch(Exception e)
            {
                Console.WriteLine("Konekcija nije uspjela, provjerite internet konekciju ili URL");
            }

            return downloadString;
        }

        protected virtual string getId(string url, string[] param)
        {
            string html = downloadHtml(url);

            if (html == null)
                return null;

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);

            var session = document.DocumentNode.Descendants(param[0]).Where(x => x.Attributes[param[1]].Value.Contains(param[2])).FirstOrDefault();

            if (session == null)
                return null;

            return session.Attributes["value"].Value;
        }

        protected virtual StringBuilder createRequest(NameValueCollection collection)
        {
            StringBuilder buildString = new StringBuilder();
            string str = string.Empty;

            foreach (var key in collection.AllKeys)
            {
                buildString.Append(str);
                buildString.Append(HttpUtility.UrlEncode(key));
                buildString.Append("=");
                buildString.Append(HttpUtility.UrlEncode(collection[key]));
                str = "&";
            }

            return buildString;
        }

        public abstract string downloadFile();

    }

    class WebPageSrbija : AbstractPage
    {

        string method = "POST";
        WebData data;
        string[] idParams = { "input", "value", "_id" };

        public WebPageSrbija(WebData data)
        {
            this.data = data;
        }
        
        public override string downloadFile()
        {
            string recString = null;

            string fid = getId(data.url, idParams);

            if (fid == null)
            {
                Console.WriteLine("Failed to parse web page.");
                return null;
            }

            NameValueCollection collection = new NameValueCollection
                {
                    { "index:brKursneListe", "" },
                    { "index:year" , data.year },
                    { "index:inputCalendar1" , data.date },
                    { "index:vrsta" , "3" },
                    { "index:prikaz" , "1" },
                    { "index:buttonShow" , "Prikaži" },
                    { "com.sun.faces.VIEW" , fid },
                    { "index" , "index" },
                };

            StringBuilder buildString = createRequest(collection);

            HttpWebRequest client = (HttpWebRequest)WebRequest.Create(data.url);
            client.CookieContainer = cookieContainer;
            client.Method = method;

            client.ServicePoint.Expect100Continue = false;
            client.ContentType = "application/x-www-form-urlencoded";
            try
            {
                using (var writer = new StreamWriter(client.GetRequestStream()))
                {
                    writer.Write(buildString.ToString());
                }

                var response2 = (HttpWebResponse)client.GetResponse();
                StreamReader sr2 = new StreamReader(response2.GetResponseStream());
                recString = sr2.ReadToEnd();
            }
            catch(WebException we)
            {
                Console.WriteLine(we.Message);
            }

            return recString;
        }

    }

    class WebPageMakedonija : AbstractPage
    {

        string method = "POST";
        WebData data;
        string[] idParamValid = { "input", "id", "__EVENTVALIDATION" };
        string[] idParamState = { "input", "id", "__VIEWSTATE" } ;

        public WebPageMakedonija(WebData data)
        {
            this.data = data;
        }

       

        public override string downloadFile()
        {
            string idParamS = getId(data.url, idParamState);
            string idParamV = getId(data.url, idParamValid);

            if (idParamS == null || idParamV == null)
            {
                Console.WriteLine("Failed to parse web page.");

                return null;
            }

            NameValueCollection collection = new NameValueCollection
                {
                    { "__VIEWSTATE", idParamS },
                    { "__EVENTVALIDATION" , idParamV },
                    { "Button1" , "Download" }
                };

            StringBuilder buildString = createRequest(collection);

            HttpWebRequest client = (HttpWebRequest)WebRequest.Create(data.url);
            client.CookieContainer = cookieContainer;
            client.Method = method;

            client.ServicePoint.Expect100Continue = false;
            client.ContentType = "application/x-www-form-urlencoded";

            using (var writer = new StreamWriter(client.GetRequestStream()))
            {
                writer.Write(buildString.ToString());
            }

            var response2 = (HttpWebResponse)client.GetResponse();
            StreamReader sr2 = new StreamReader(response2.GetResponseStream());

            string recString = sr2.ReadToEnd();

            return recString;
        }

    }

    class WebPageBosna : AbstractPage
    {
        WebData data;
        string[] idParam = { "tr", ".csv" };

        public WebPageBosna(WebData data)
        {
            this.data = data;
        }

        protected override string getId(string url, string[] param)
        {
            string html = downloadHtml(url);

            if (html == null)
                return null;

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);

            var session = document.DocumentNode.Descendants(param[0]).Where(x => x.InnerText.Contains(data.date) && x.InnerText.Contains(param[1])).FirstOrDefault();
            string[] file = session.InnerText.Split('.');

            return file[0] + ".csv";
        }

        public override string downloadFile()
        {
            string fileName = getId(data.url, idParam);

            if (fileName == null)
            {
                Console.WriteLine("Failed to parse web page.");
                return null;
            }

            string buildString = data.url + fileName;

            WebClient client = new WebClient();

            string recString = client.DownloadString(buildString);

            return recString;
        }

    }

}
