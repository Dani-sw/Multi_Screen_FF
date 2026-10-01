using Ini.Net;
using Multi_Screen_App.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
            Save();
        }

        public static ObservableCollection<MonitorItem> Monitors { get; } = new ObservableCollection<MonitorItem>();

        public static void Load()
        {
            Populate_MonitorList();
            inifile();


            GlobalVar._GUI.Title_lbl.Text = Version.sw_title();
            GlobalVar._GUI.version_lbl.Text = Version.sw_version();
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

            try
            {
                GlobalVar.ConfigFile = new IniFile("System\\MSA_Config.ini");

                GlobalVar.ConfigFile_App_list = GlobalVar.ConfigFile.ReadSection_v2("APP");

                foreach (KeyValuePair<string, string> item in GlobalVar.ConfigFile_App_list)
                {
                    string[] app_info = item.Value.Split('.');
                    add_Application_from_inifile(app_info);
                }
            }
            catch (Exception)
            {

               
            }

        }


        public static void add_Application_from_inifile(string[] _app_info)
        {
            ApplicationItem application = new ApplicationItem();

            application.Name = _app_info[1];
            application.Path = _app_info[0];

            application.Startup = false;

            if (_app_info[2]=="True")
            {
                application.Startup = true;
            }
           
            application.Monitor = Convert.ToInt32(_app_info[3]);
            GlobalVar.Applications_List.Add(application);
        }


        public static void Save()
        {
            GlobalVar.ConfigFile.DeleteSection("APP");
            int i= 1;
            foreach (ApplicationItem _application in GlobalVar.Applications_List)
            {
               
                string app_value = _application.Path+ "." + Path.GetFileNameWithoutExtension(_application.Name) + "." + _application.Startup + "." + _application.Monitor;
                GlobalVar.ConfigFile.WriteString_v2("APP", "APP" + i.ToString(), app_value);
                i++;
            }
            MessageBox_Custom.Show("To apply the changes, the application will restart.", "Application Restart", MessageBox_Custom.MessageType.Warning);
            Process_Action.RestartApplication();

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
