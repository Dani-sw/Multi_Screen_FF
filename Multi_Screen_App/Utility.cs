using Multi_Screen_App.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Multi_Screen_App
{
    static class Utility
    {
        public static void Check_dblInstance()
        {
            string processName = Process.GetCurrentProcess().ProcessName;

            int count = Process.GetProcessesByName(processName).Length;

            if (count > 1)
            {   
                MessageBox_Custom.Show("The application is already running.", "Double instance", MessageBox_Custom.MessageType.Warning);
                Application.Current.Shutdown();
            }
        }
    }
}
