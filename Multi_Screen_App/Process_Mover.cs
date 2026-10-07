using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;              // solo per Screen
using System.Windows.Threading;

namespace Multi_Screen_App
{
    public class Process_Mover : IDisposable
    {
        private const int SW_RESTORE = 9;
        private const int SW_MAXIMIZE = 3;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOSIZE = 0x0001;

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr after, int x, int y, int cx, int cy, uint flags);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int cmd);

        private readonly string _processName;
        private readonly int _screenIndex;
        private readonly int _pollMs;

        private readonly DispatcherTimer _searchTimer;   // 1) cerca l'app
        private readonly DispatcherTimer _delayTimer;   // 2) attesa splash, poi sposta
        private readonly DispatcherTimer _watchTimer;    // 3) controlla quando l'app si chiude

        private Process _proc;

        private readonly int _delayMs;


        public Process_Mover(string processName, int screenIndex, int delayMs = 0, int pollMs = 1000)
        {
            _processName = processName;
            _screenIndex = screenIndex;
            _delayMs = delayMs;
            _pollMs = pollMs;

            _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(_pollMs) };
            _searchTimer.Tick += SearchTick;

            _delayTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Math.Max(1, _delayMs)) };
            _delayTimer.Tick += DelayTick;

            _watchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(_pollMs) };
            _watchTimer.Tick += WatchTick;
        }

        private void DelayTick(object sender, EventArgs e)
        {
            _delayTimer.Stop();      // una sola volta

            try
            {
                _proc.Refresh();

                if (_proc.HasExited)
                {
                    StartSearching();
                    return;
                }

                IntPtr hwnd = _proc.MainWindowHandle;
                if (hwnd != IntPtr.Zero)
                    MoveAndMaximize(hwnd);

                _watchTimer.Start(); // in ogni caso passa al monitoraggio
            }
            catch
            {
                StartSearching();
            }
        }
        public void Start()
        {
            StartSearching();
        }
        public void Stop()
        {
            _searchTimer.Stop();
            _delayTimer.Stop();
            _watchTimer.Stop();
            ReleaseProcess();
        }

        private void StartSearching()
        {
            _watchTimer.Stop();
            _delayTimer.Stop();
            ReleaseProcess();
            _searchTimer.Start();
        }

        private void SearchTick(object sender, EventArgs e)
        {
            var p = Process.GetProcessesByName(_processName)
                           .FirstOrDefault(x => !x.HasExited);

            if (p == null) return;

            _proc = p;
            _searchTimer.Stop();     // trovata
            _delayTimer.Start();     // un'unica attesa
        }


        // ---------- 3) MONITORAGGIO CHIUSURA ----------
        private void WatchTick(object sender, EventArgs e)
        {
            try
            {
                _proc.Refresh();
                if (!_proc.HasExited) return;      // ancora aperta
            }
            catch { /* processo non più accessibile = chiuso */ }

            _watchTimer.Stop();
            StartSearching();                      // torna a cercarla
        }

        // ---------- UTILITY ----------
        private void MoveAndMaximize(IntPtr hwnd)
        {
            var screens = Screen.AllScreens;
            if (_screenIndex >= screens.Length) return;

            var b = screens[_screenIndex].WorkingArea;

            ShowWindow(hwnd, SW_RESTORE);
            SetWindowPos(hwnd, IntPtr.Zero, b.Left, b.Top, 0, 0, SWP_NOZORDER | SWP_NOSIZE);
            ShowWindow(hwnd, SW_MAXIMIZE);
        }

        private void ReleaseProcess()
        {
            _proc?.Dispose();
            _proc = null;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}