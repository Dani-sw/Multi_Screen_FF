using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows.Forms; // per Screen
using System.Timers;

namespace Multi_Screen_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string[] processname = new string[10];
        private ProcessMonitorMover mover1;
        private ProcessMonitorMover mover2;
        private ProcessMonitorMover mover3;

        public MainWindow()
        {
            InitializeComponent();
            
        }
        public class ProcessMonitorMover
        {
            [DllImport("user32.dll")]
            private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
                int X, int Y, int cx, int cy, uint uFlags);
            [DllImport("user32.dll")]
            private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern IntPtr FindWindow(string lpClassName,string lpWindowName);

            private int temp_pid;

            private const int SW_MAXIMIZE = 3;
            private const uint SWP_NOZORDER = 0x0004;
            private const uint SWP_NOSIZE = 0x0001;

            private readonly string _processName; // senza .exe, es. "notepad"
            private readonly int _targetScreenIndex; // 0 = primo monitor, 1 = secondo
            private int IsAC = 0;
            private ManagementEventWatcher watcher;
            MainWindow GUI = new MainWindow();
            private int _screenindex;

            public ProcessMonitorMover(string processName, int targetScreenIndex,MainWindow _gui)
            {
                GUI = _gui;
                _processName = processName;
                _targetScreenIndex = targetScreenIndex;
                if (processName == "acs_pro")
                {
                    IsAC = 1;
                }
                else
                {
                    IsAC = 0;
                }
            }




            public void Start()
            {
                var query = new WqlEventQuery(
                    "SELECT * FROM Win32_ProcessStartTrace WHERE ProcessName = '" + _processName + ".exe'");
                watcher = new ManagementEventWatcher(query);
                watcher.EventArrived += Watcher_EventArrived;
                watcher.Start();
            }

            private void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
            {
                int pid = Convert.ToInt32(e.NewEvent.Properties["ProcessID"].Value);
                MoveProcessWindowToScreen(pid, _targetScreenIndex);
            }

            private void MoveProcessWindowToScreen(int pid, int screenIndex)
            {
                var proc = Process.GetProcessById(pid);
                try
                {
                    if (IsAC==1)
                    {
                        

                        // Aspetta che la finestra principale sia pronta (max ~5 secondi)
                        IntPtr hWnd = IntPtr.Zero;
                        for (int i = 0; i < 50; i++)
                        {
                            proc.Refresh();
                            hWnd = proc.MainWindowHandle;
                            if (hWnd != IntPtr.Zero) break;
                            System.Threading.Thread.Sleep(100);
                        }

                        if (hWnd == IntPtr.Zero) return; // niente finestra trovata

                        if (screenIndex >= Screen.AllScreens.Length) return; // monitor inesistente

                        var screen = Screen.AllScreens[screenIndex];
                        var bounds = screen.WorkingArea;

                        SetWindowPos(hWnd, IntPtr.Zero, bounds.Left, bounds.Top, 0, 0,
                            SWP_NOZORDER | SWP_NOSIZE);
                        // Poi massimizza
                        ShowWindow(hWnd, SW_MAXIMIZE);
                    }
                    else
                    {

                        temp_pid = pid;
                        _screenindex = screenIndex;
                        System.Timers.Timer timer = new System.Timers.Timer(300);                      
                        timer.Start();
                        timer.Elapsed += Timer_Elapsed;


                    }


                    GUI.Dispatcher.Invoke(() =>
                    {
                        GUI.Debug1_lbl.Content = proc.MainWindowHandle.ToString();
                    });
                }
                  
                catch { /* processo già terminato o accesso negato: ignora */ }
            }

            private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
            {
                System.Timers.Timer _timer =sender as System.Timers.Timer;
                var proc = Process.GetProcessById(temp_pid);
                IntPtr hWnd = IntPtr.Zero;
                proc.Refresh();
                hWnd = proc.MainWindowHandle;
                string title = proc.MainWindowTitle;
                if (!string.IsNullOrEmpty(title) && title != "Waiting_Dialog")
                {
                    //sempre stoppare prima il timer
                   
                    _timer.Stop();
                   // System.Windows.MessageBox.Show("Here");
                    if (hWnd == IntPtr.Zero) return; // niente finestra trovata

                    if (_screenindex >= Screen.AllScreens.Length) return; // monitor inesistente

                    var screen = Screen.AllScreens[_screenindex];
                    var bounds = screen.WorkingArea;

                    SetWindowPos(hWnd, IntPtr.Zero, bounds.Left, bounds.Top, 0, 0,
                        SWP_NOZORDER | SWP_NOSIZE);
                    // Poi massimizza
                    ShowWindow(hWnd, SW_MAXIMIZE);
                    //System.Windows.MessageBox.Show("Here");

                }
            }

  
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            processname[0] = "acs_pro";
            processname[1] = "AC_LRM_Manager";
            processname[2] = "AC_LRM_Client";
            mover1 = new ProcessMonitorMover(processname[0],1,this); // sposta "acs.exe" sul secondo monitor
            mover2 = new ProcessMonitorMover(processname[1],0,this); // sposta "acs.exe" sul secondo monitor
            mover3 = new ProcessMonitorMover(processname[2],1, this); // sposta "acs.exe" sul secondo monitor
            mover1.Start();
            mover2.Start();
            mover3.Start();
            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
            Environment.Exit(0);
        }
    }
}
