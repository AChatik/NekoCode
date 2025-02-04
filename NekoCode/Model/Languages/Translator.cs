using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoCode
{
    public static class Translator
    {
        static LanguageBase EN = new LanguageBase();
        static RULanguage RU = new RULanguage();
        public static dynamic GetLanguage()
        {
            switch (Properties.Settings.Default.Language)
            {
                case "EN":
                    return EN;
                case "RU":
                    return RU;

            }
            return EN;
        }
    }
}
