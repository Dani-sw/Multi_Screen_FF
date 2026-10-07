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
using System.Text.RegularExpressions;
using System.Windows.Input;


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
        public Process_Mover _mover { get; private set; }

        public MainWindow()
        {
            Utility.Check_dblInstance();
            InitializeComponent();         
            GlobalVar._GUI = this;
            Preferences.Load(); //occhio va esattamente qui per caricare i combobox            
            ApplicationsList.ItemsSource = GlobalVar.Applications_List;
            Application_initialize();
            

        }
 

        private void Application_initialize()
        {
            if (GlobalVar.Applications_List.Count != 0)
            {
                foreach (ApplicationItem application in GlobalVar.Applications_List)
                {

                    application.PropertyChanged += Application_PropertyChanged;
                }

                foreach (ApplicationItem application in GlobalVar.Applications_List)
                {
                   
                    if (application.Name == GlobalVar.AC_Name)
                    {
                        application.arguments = "-autodrive";

                    }
                
                    _mover = new Process_Mover(application.Name, application.Monitor, delayMs: application.Delay, pollMs: 500);
                    _mover.Start();

                }

                /*foreach (ProcessMonitorMover mover in mover_list)
                {

                    mover.Start();
                }*/

                try
                {
                    foreach (ApplicationItem application in GlobalVar.Applications_List)
                    {
                        if (application.Startup == true)
                        {
                     

                            Process_Action.App_Start(application.Path + "\\", application.Name+".exe",application.arguments);
                        }
                    }
                   
                }
                catch (Exception ex)
                {
                    MessageBox_Custom.Show(ex.Message, "No Application found", MessageBox_Custom.MessageType.Warning);
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
                application.Delay = 1000;
                application.Startup = false;
                application.Monitor = 1;

                GlobalVar.Applications_List.Add(application);
            }
        }


private static readonly Regex _nonDigit = new Regex("[^0-9]");

    // blocca lettere e simboli
    private void Delay_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = _nonDigit.IsMatch(e.Text);
    }

    // lo spazio non passa da PreviewTextInput, va bloccato qui
    private void Delay_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Space) e.Handled = true;
    }

    // blocca l'incolla di testo non numerico
    private void Delay_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            var text = (string)e.DataObject.GetData(typeof(string));
            if (_nonDigit.IsMatch(text)) e.CancelCommand();
        }
        else e.CancelCommand();
    }
    private void Remove_Application_btn(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.DataContext is ApplicationItem application)
            {
                GlobalVar.Applications_List.Remove(application);
            }
        }



    }

  
}
