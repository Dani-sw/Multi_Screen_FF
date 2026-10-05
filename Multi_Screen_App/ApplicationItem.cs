using System.ComponentModel;


namespace Multi_Screen_App
{  
    public class ApplicationItem : INotifyPropertyChanged
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public string arguments { get; set; } = "";

        private bool _isChecked;
        private int _monitor;

        public bool Startup
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged("IsChecked");
                }
            }
        }

        public int Monitor
        {
            get { return _monitor; }
            set
            {
                if (_monitor != value)
                {
                    _monitor = value;
                    OnPropertyChanged("Monitor");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }




        public static void Changes_Warning()
        {
            GlobalVar._GUI.Warning_lbl.Visibility = System.Windows.Visibility.Visible;
        }



    }



}