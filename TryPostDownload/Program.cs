using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace TryPostDownload
{
    class Program
    {

        static void Main(string[] args)
        {
            WebData webData = new WebData();
            webData.url = "http://www.nbrm.mk/klservicewebclient/KursnaLista.aspx?lang=EN&list=KL&OD=10.08.2015&DD=";
            webData.year = "2015";
            webData.date = "2015-02-17";

            AbstractPage myWebPage = FactoryWebPage.getPage(Banke.Makedonija, webData);

            string recString = myWebPage.downloadFile();

            FileStream fstream = new FileStream("C:\\Users\\pordi\\kursnalista.csv", FileMode.Create);
            fstream.Write(Encoding.UTF8.GetBytes(recString), 0, recString.Length);

        }
    }
}
