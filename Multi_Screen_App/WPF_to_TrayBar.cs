using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Multi_Screen_App
{

    /// <summary>
    /// Windows 10 balloontip not showed: on traybar click notification center icon
    /// than "manage notificatons"->"get notification from apps..."->ON
    /// </summary>
    static class WPF_to_TrayBar
    {

     
        static System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();

        static public void inizialize()
        {
            ni.BalloonTipText = "The app has been minimised. Click the tray icon to show.";
            ni.BalloonTipTitle = "Multi Screen App " + Version.sw_version();
            ni.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            ni.Text = "Multi Screen App " + Version.sw_version(); ;
            ni.Icon = new Icon(Application.GetResourceStream(new Uri("pack://application:,,,/MSA.ico")).Stream);
            ni.Click += ni_Click;

            GlobalVar._GUI.Closed += _AC_Core_GUI_Closed;
            GlobalVar._GUI.StateChanged += _AC_Core_GUI_StateChanged;

            GlobalVar._GUI.Hide();
            ni.Visible = true;
            ni.ShowBalloonTip(1000); //deprecated the OS control this(W7 or W10)

        }

        public static void open_GUI()
        {
            ni_Click(null,null);
        }

        private static void _AC_Core_GUI_StateChanged(object sender, EventArgs e)
        {

            if (GlobalVar._GUI.WindowState == WindowState.Minimized)
            {
                GlobalVar._GUI.Hide();
                ni.Visible = true;           
                ni.ShowBalloonTip(1000); //deprecated the OS control this(W7 or W10)

            }
        }
    
        private static void _AC_Core_GUI_Closed(object sender, EventArgs e)
        {
            ni.Dispose();
            ni = null;
        }

        private static void ni_Click(object sender, EventArgs e)
        {
            ni.Visible = false;
            GlobalVar._GUI.Show();
            GlobalVar._GUI.WindowState = WindowState.Normal;
        }

        

   
    }
}
