using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Multi_Screen_App
{
    static class Process_Action
    {

        public static void App_Start(string path,string exename) //path with final \
        {

             ProcessStartInfo infostart = new ProcessStartInfo();
             infostart.WorkingDirectory = path;  //FONDAMENTALE PER FUNZIONARE
             infostart.FileName =path+exename;
             Process process = Process.Start(infostart);

        }

        static public void RestartApplication()
        {
            string exePath = Assembly.GetEntryAssembly().Location;
            Process.Start(exePath);
            System.Windows.Application.Current.Shutdown();
        }


    }
}
