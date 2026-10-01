
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Windows.Forms;


namespace Multi_Screen_App
{
    public class ProcessAC_Mover
    {
        private DispatcherTimer _moveTimer;
        private DispatcherTimer _monitorTimer;

        private string _processName;
        private string _windowTitle;
        private int _screenIndex;

        private int _currentPid = -1;
        private bool _windowMoved = false;

        public ProcessAC_Mover()
        {
            // Timer che cerca/sposta la finestra
            _moveTimer = new DispatcherTimer();
            _moveTimer.Interval = TimeSpan.FromMilliseconds(300);
            _moveTimer.Tick += MoveTimer_Tick;

            // Timer che controlla se il processo è ancora vivo
            _monitorTimer = new DispatcherTimer();
            _monitorTimer.Interval = TimeSpan.FromMilliseconds(1000);
            _monitorTimer.Tick += MonitorTimer_Tick;
        }

        public void Start(string processName, string windowTitle, int screenIndex)
        {
            _processName = processName;
            _windowTitle = windowTitle;
            _screenIndex = screenIndex;

            _currentPid = -1;
            _windowMoved = false;

            _moveTimer.Start();
            _monitorTimer.Start();
        }

        public void Stop()
        {
            _moveTimer.Stop();
            _monitorTimer.Stop();

            _currentPid = -1;
            _windowMoved = false;
        }

        // =========================================================
        // CERCA IL PROCESSO E SPOSTA LA FINESTRA
        // =========================================================

        private void MoveTimer_Tick(object sender, EventArgs e)
        {
            Process[] processes = Process.GetProcessesByName(_processName);

            foreach (Process process in processes)
            {
                try
                {
                    if (process.HasExited)
                        continue;

                    int pid = process.Id;

                    // Se è un nuovo processo, aggiorniamo il PID
                    if (_currentPid != pid)
                    {
                        _currentPid = pid;
                        _windowMoved = false;
                    }

                    if (_windowMoved)
                        return;

                    // Cerca la finestra
                    IntPtr hWnd = FindWindow("acsW", _windowTitle);

                    if (hWnd == IntPtr.Zero)
                        continue;

                    // Verifica che la finestra appartenga
                    // proprio al processo trovato
                    uint windowPid;

                    GetWindowThreadProcessId(
                        hWnd,
                        out windowPid
                    );

                    if (windowPid != pid)
                        continue;

                    if (_screenIndex < 0 ||
                        _screenIndex >= Screen.AllScreens.Length)
                    {
                        _moveTimer.Stop();
                        return;
                    }

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

                    _windowMoved = true;

                    // La finestra è stata trovata e spostata.
                    // Fermiamo solo il timer di movimento.
                    _moveTimer.Stop();

                    return;
                }
                catch
                {
                    // Il processo può essere ancora in avvio
                }
            }
        }

        // =========================================================
        // MONITORA CONTINUAMENTE IL PROCESSO
        // =========================================================

        private void MonitorTimer_Tick(object sender, EventArgs e)
        {
            Process[] processes = Process.GetProcessesByName(_processName);

            bool processFound = false;

            foreach (Process process in processes)
            {
                try
                {
                    if (!process.HasExited)
                    {
                        processFound = true;

                        // Salviamo il PID se è un nuovo processo
                        if (_currentPid != process.Id)
                        {
                            _currentPid = process.Id;
                            _windowMoved = false;
                        }

                        break;
                    }
                }
                catch
                {
                }
            }

            // =====================================================
            // PROCESSO CHIUSO
            // =====================================================

            if (!processFound)
            {
                _currentPid = -1;
                _windowMoved = false;

                // Riattiva il timer che cerca la finestra
                if (!_moveTimer.IsEnabled)
                    _moveTimer.Start();
            }
        }


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
    }
}



