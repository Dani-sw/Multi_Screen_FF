using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multi_Screen_App
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Windows;

    public class WindowWatcher
    {
        private delegate void WinEventDelegate(
            IntPtr hWinEventHook,
            uint eventType,
            IntPtr hwnd,
            int idObject,
            int idChild,
            uint idEventThread,
            uint dwmsEventTime);

        private const uint EVENT_OBJECT_SHOW = 0x8002;
        private const int OBJID_WINDOW = 0;
        private const uint WINEVENT_OUTOFCONTEXT = 0x0000;

        private const int SW_MAXIMIZE = 3;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOSIZE = 0x0001;

        private WinEventDelegate _delegate;
        private IntPtr _hook;
        private uint _screenIndex;

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(
            uint eventMin,
            uint eventMax,
            IntPtr hmodWinEventProc,
            WinEventDelegate lpfnWinEventProc,
            uint idProcess,
            uint idThread,
            uint flags);

        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(
            IntPtr hWnd,
            out uint processId);

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

        public void Start(uint processId, uint app_ScreenIndex)
        {
            _screenIndex = app_ScreenIndex;
            _delegate = WindowCreated;


            _hook = SetWinEventHook(
                EVENT_OBJECT_SHOW,
                EVENT_OBJECT_SHOW,
                IntPtr.Zero,
                _delegate,
                processId,
                0,
                WINEVENT_OUTOFCONTEXT);
        }

        private void WindowCreated(
            IntPtr hook,
            uint eventType,
            IntPtr hwnd,
            int idObject,
            int idChild,
            uint threadId,
            uint time)


        {

            if (hwnd == IntPtr.Zero)
                return;

            if (idObject != OBJID_WINDOW)
                return;

            uint processId;
            GetWindowThreadProcessId(hwnd, out processId);


            var screen = System.Windows.Forms.Screen.AllScreens[_screenIndex];
            var bounds = screen.WorkingArea;

            SetWindowPos(hwnd, IntPtr.Zero, bounds.Left, bounds.Top, 0, 0,
                        SWP_NOZORDER | SWP_NOSIZE);
            // Poi massimizza
            ShowWindow(hwnd, SW_MAXIMIZE);

        }

        public void Stop()
        {
            if (_hook != IntPtr.Zero)
            {
                UnhookWinEvent(_hook);
                _hook = IntPtr.Zero;
            }
        }
    }
}