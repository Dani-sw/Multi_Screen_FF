using Ini.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Multi_Screen_App
{
    class Preferences
    {
        //public static List<MonitorItem> Monitors { get; private set; }
        //cosi con questa lista classica non funionava il binding nella combobox

        
        private static void Save_Btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBox.Show(GlobalVar.applications[0].Name.ToString()); ;
        }

        public static ObservableCollection<MonitorItem> Monitors { get; } = new ObservableCollection<MonitorItem>();

        public static void Load()
        {
            Populate_MonitorList();
            inifile();
            GlobalVar._GUI.Save_Btn.Click += Save_Btn_Click;
        }

        private static void Populate_MonitorList()
        {
            Monitors.Clear();
            foreach (var m in GetMonitors())
                Monitors.Add(m);
        }


        public static void inifile()
        {

            GlobalVar.ConfigFile = new IniFile("System\\MSA_Config.ini");
             GlobalVar.ConfigFile_App_list = GlobalVar.ConfigFile.ReadSection_v2("APP");

            foreach (KeyValuePair<string, string> item in GlobalVar.ConfigFile_App_list)
            {
                string[] app_info = item.Value.Split('.');
                add_Application_from_inifile(app_info);
     

               //MessageBox.Show(item.Key + " " + parts[0]);
            }

        }


        public static void add_Application_from_inifile(string[] _app_info)
        {
            ApplicationItem application = new ApplicationItem();

            application.Name = _app_info[1];
            application.Path = _app_info[0];

            application.Startup = false;
            if (_app_info[2]=="1")
            {
                application.Startup = true;
            }
           
            application.Monitor = Convert.ToInt32(_app_info[3]);

            GlobalVar.applications.Add(application);
        }


        public static List<MonitorItem> GetMonitors()
        {
            List<MonitorItem> monitors = new List<MonitorItem>();

            Screen[] screens = Screen.AllScreens;

            for (int i = 0; i < screens.Length; i++)
            {
                monitors.Add(new MonitorItem
                {
                    Index = i
                });
            }

            return monitors;
        }



        public class MonitorItem
        {
            public int Index { get; set; }

            public string Name
            {
                get
                {
                    return "Monitor " + (Index + 1);
                }
            }
        }


        
    }
}
