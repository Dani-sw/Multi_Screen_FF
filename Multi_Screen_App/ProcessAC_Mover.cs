
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Windows.Forms;
using System.Threading;

namespace Multi_Screen_App { 

    public static class ProcessAC_Mover
    {
        private static DispatcherTimer _AC_Find_Timer;
        private static DispatcherTimer _AC_OFF_Timer;
        public static int _screenIndex;

        // =========================================================
        // WIN32
        // =========================================================

        private const uint SWP_NOZORDER = 0x0004;
        private const int SW_MAXIMIZE = 3;

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(
            string lpClassName,
            string lpWindowName);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(
            IntPtr hWnd,
            out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(
            IntPtr hWnd,
            int nCmdShow);



        public static void inizialize(int monitor_index)
        {
            _AC_Find_Timer = new DispatcherTimer();
            _AC_Find_Timer.Interval = TimeSpan.FromMilliseconds(1000);
            _AC_Find_Timer.Tick += AC_Find_Tick;
            _AC_Find_Timer.Start();
            _screenIndex = monitor_index;

            _AC_OFF_Timer = new DispatcherTimer();
            _AC_OFF_Timer.Interval = TimeSpan.FromMilliseconds(1000);
            _AC_OFF_Timer.Tick += _AC_OFF_Timer_Tick;
        }

        private static void _AC_OFF_Timer_Tick(object sender, EventArgs e)
        {
            _AC_OFF_Timer.Stop();
            Process[] list = Process.GetProcessesByName("acs_pro");
            if (list.Length == 0){
                Thread.Sleep(1000);
                _AC_Find_Timer.Start();
            }
            _AC_OFF_Timer.Start();
        }

        private static void AC_Find_Tick(object sender, EventArgs e)
        {
            _AC_Find_Timer.Stop();
            Process[] list = Process.GetProcessesByName("acs_pro");
            if (list.Length > 0)
            {
                //Debug.WriteLine("AC start");
                Thread.Sleep(2000);
                IntPtr hWnd = FindWindow("acsW", "Assetto Corsa");



                Screen screen = Screen.AllScreens[_screenIndex];
                var bounds = screen.WorkingArea;

                SetWindowPos(
                    hWnd,
                    IntPtr.Zero,
                    bounds.Left,
                    bounds.Top,
                    bounds.Width,
                    bounds.Height,
                    SWP_NOZORDER
                );

                ShowWindow(
                    hWnd,
                    SW_MAXIMIZE
                );
                _AC_OFF_Timer.Start();
            }
            else
            {
                //Debug.WriteLine("ancora niente");
                _AC_Find_Timer.Start();
            }
        }
    }
}



