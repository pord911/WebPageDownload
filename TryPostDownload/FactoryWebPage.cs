using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryPostDownload
{
    public enum Banke
    {
        Srbija,
        Makedonija,
        BiH
    };
    public class FactoryWebPage
    {

        public static AbstractPage getPage(Banke banka, WebData data)
        {
            switch (banka)
            {
                case Banke.Srbija:
                    return new WebPageSrbija(data);
                case Banke.Makedonija:
                    return new WebPageMakedonija(data);
                case Banke.BiH:
                    return new WebPageBosna(data);
            }
            return null;
        }
    }
}
