using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ini.Net;

namespace Multi_Screen_App
{
    class GlobalVar
    {
        static public IniFile ConfigFile;
        static public List<KeyValuePair<string, string>> ConfigFile_App_list;
        public static MainWindow _GUI;
        public static ObservableCollection<ApplicationItem> Applications_List= new ObservableCollection<ApplicationItem>();

        public static string Is_AC_PRO { get; set; }

        public static string AC_Name { get; set; }


        public static string logs_path = @"Logs\\";



    }
}
