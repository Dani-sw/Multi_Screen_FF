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
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Timers;
using System.Reflection;

namespace Multi_Screen_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    //TOFIX mi frega una cartella nella path in salvataggio e aggiunge anche l'exe name in add da pulsante 
    //TOFIX2 e così dopo alcuni salvataggi consuma le path più corte e crasha cancellando ttutto l'ini
    //TODO startup automatico processi
    

    public partial class MainWindow : Window
    {
        private string[] processname = new string[10];
        private List<ProcessMonitorMover> mover_list=new List<ProcessMonitorMover>();

        //private WindowWatcher _watcher;
        public MainWindow()
        {
            Utility.Check_dblInstance();
            InitializeComponent();
            GlobalVar._GUI = this;
            Preferences.Load(); //occhio va esattamente qui per caricare i combobox            
            ApplicationsList.ItemsSource = GlobalVar.Applications_List;

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
           
            private int _screenindex;

            public WindowWatcher _watcher_window { get; private set; }

            public ProcessMonitorMover(string processName, int targetScreenIndex)
            {
                
                _processName = processName;
                _targetScreenIndex = targetScreenIndex;

              /*  if (processName != "AC_LRM_Manager" || processName=="AC_LRM_Client")
                {
                    IsAC = 0;
                }
                else
                {
                    IsAC = 1;
                }*/
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
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _watcher_window = new WindowWatcher();
                    _watcher_window.Start((uint)pid,(uint)_targetScreenIndex);
                }));
            }

            private void Watcher_EventArrived2(object sender, EventArrivedEventArgs e)
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




             foreach (ApplicationItem application in GlobalVar.Applications_List)
             {
                 mover_list.Add(new ProcessMonitorMover(application.Name, application.Monitor));
             }

             foreach (ProcessMonitorMover mover in mover_list)
             {
                 mover.Start();
             }

   



           /* ProcessStartInfo infostart = new ProcessStartInfo();
            infostart.WorkingDirectory = @"C:\Users\assir\Source\Repos\AC-Lan_Race_Multiplayer\AC_LRM_Client\bin\Debug\";  //FONDAMENTALE PER FUNZIONARE
            infostart.FileName = @"C:\Users\assir\Source\Repos\AC-Lan_Race_Multiplayer\AC_LRM_Client\bin\Debug\AC_LRM_Client.exe";


            Process process=Process.Start(infostart);*/
         

        }
        public void RestartApplication()
        {
            string exePath = Assembly.GetEntryAssembly().Location;

            Process.Start(exePath);
            System.Windows.Application.Current.Shutdown();
        }





        private void AddApplication_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();

            dialog.Title = "Select Application";
            dialog.Filter = "Executable files (*.exe)|*.exe";
            dialog.Multiselect = false;

            if (dialog.ShowDialog() == true)
            {
                ApplicationItem application = new ApplicationItem();

                application.Name = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
                application.Path = dialog.FileName;
                application.Startup = false;
                application.Monitor = 1;

                GlobalVar.Applications_List.Add(application);
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.DataContext is ApplicationItem application)
            {
                GlobalVar.Applications_List.Remove(application);
            }
        }


        #region Windows Layout command
        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (WindowState == WindowState.Normal)
                {
                    WindowState = WindowState.Maximized;
                }
                else
                {
                    WindowState = WindowState.Normal;
                }

                return;
            }

            DragMove();
        }


        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }


        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }


        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
            Environment.Exit(0);

        }

        #endregion


       


    }

  
}
