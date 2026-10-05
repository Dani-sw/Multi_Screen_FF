using System;
using System.Collections.Generic;
using System.Windows;
using System.Management;
using System.Runtime.InteropServices;
using Multi_Screen_App.Controls;
using MahApps.Metro.Controls;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace Multi_Screen_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    //TOFIX mi frega una cartella nella path in salvataggio e aggiunge anche l'exe name in add da pulsante 
    //TOFIX2 e così dopo alcuni salvataggi consuma le path più corte e crasha cancellando ttutto l'ini
    //TODO startup automatico processi
    

    public partial class MainWindow : MetroWindow
    {
      
        private List<ProcessMonitorMover> mover_list=new List<ProcessMonitorMover>();

        private const int SW_MAXIMIZE = 3;
         public const int SW_MINIMIZE = 6;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOSIZE = 0x0001;
        public const int SWP_SHOWWINDOW = 0x0040;
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(
            IntPtr hWnd,
            out RECT lpRect);

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
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName,
          string lpWindowName);

        //private WindowWatcher _watcher;
        public MainWindow()
        {
            Utility.Check_dblInstance();
            InitializeComponent();         
            GlobalVar._GUI = this;
            Preferences.Load(); //occhio va esattamente qui per caricare i combobox            
            ApplicationsList.ItemsSource = GlobalVar.Applications_List;
            Application_initialize();
            

        }
        public class ProcessMonitorMover
        {
           
            private readonly string _processName; // senza .exe, es. "notepad"
            private readonly int _targetScreenIndex; // 0 = primo monitor, 1 = secondo
            private ManagementEventWatcher watcher;

            public WindowWatcher _watcher_window { get; private set; }

            public ProcessMonitorMover(string processName, int targetScreenIndex)
            {               
                _processName = processName;
                _targetScreenIndex = targetScreenIndex;
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
                string nameprocess = (string)e.NewEvent.Properties["ProcessName"].Value;

                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    
                        _watcher_window = new WindowWatcher();
                        _watcher_window.Start((uint)pid, (uint)_targetScreenIndex);

                }
                ));
            }


        }


        private void Application_initialize()
        {
            if (GlobalVar.Applications_List.Count != 0)
            {
                foreach (ApplicationItem application in GlobalVar.Applications_List)
                {
                    if (application.Name != "acs_pro")
                    {
                        mover_list.Add(new ProcessMonitorMover(application.Name, application.Monitor));
                    }
                    else
                    {
                        ProcessAC_Mover AC_mover = new ProcessAC_Mover();
                        AC_mover.Start("acs_pro", "Assetto Corsa", application.Monitor);
                    }
                }

                foreach (ProcessMonitorMover mover in mover_list)
                {

                    mover.Start();
                }

                try
                {
                    foreach (ApplicationItem application in GlobalVar.Applications_List)
                    {
                        if (application.Startup == true)
                        {
                            Process_Action.App_Start(application.Path + "\\", application.Name);
                        }
                    }
                    foreach (ApplicationItem application in GlobalVar.Applications_List)
                    {

                        application.PropertyChanged += Application_PropertyChanged;
                    }
                }
                catch (Exception)
                {

                    MessageBox_Custom.Show("No applications found or Path is incorrect, now the app will be reset!", "No Application found", MessageBox_Custom.MessageType.Warning);
                    Process_Action.Reset();
                }

                WPF_to_TrayBar.inizialize();
            }
            else
            {
                MessageBox_Custom.Show("No applications found. Populate the list by adding applications, then save.", "No Application found", MessageBox_Custom.MessageType.Warning);
            }

            
        }



        private void Application_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ApplicationItem.Changes_Warning();
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
                application.Path = System.IO.Path.GetDirectoryName(dialog.FileName);
                application.Startup = false;
                application.Monitor = 1;

                GlobalVar.Applications_List.Add(application);
            }
        }
        private void Remove_Application_btn(object sender, RoutedEventArgs e)
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
