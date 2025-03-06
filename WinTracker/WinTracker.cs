using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinTracker
{
    public class ActiveWindowEventArgs : EventArgs
    {
        public int ProcessId { get; }
        public string ProcessName { get; }

        public ActiveWindowEventArgs(int processId, string processName)
        {
            ProcessId = processId;
            ProcessName = processName;
        }
    }

    public class ActiveWindowTracker : IDisposable
    {
        private const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
        private WinEventDelegate _winEventDelegate;
        private IntPtr _hook;

        public event EventHandler<ActiveWindowEventArgs> ActiveWindowChanged;

        public ActiveWindowTracker()
        {
            _winEventDelegate = new WinEventDelegate(WinEventProc);
            _hook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _winEventDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);

            if (_hook == IntPtr.Zero)
            {
                Debug.WriteLine("Falha ao registrar o hook de eventos.");
            }
            else
            {
                Debug.WriteLine("Hook de eventos registrado com sucesso.");
            }
        }


        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
            WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hwnd == IntPtr.Zero) return;

            try
            {
                GetWindowThreadProcessId(hwnd, out uint processId);
                Process proc = Process.GetProcessById((int)processId);
                string processName = proc.ProcessName;
                Debug.WriteLine($"Evento disparado: ProcessId = {processId}, ProcessName = {processName}");
                ActiveWindowChanged?.Invoke(this, new ActiveWindowEventArgs((int)processId, processName));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro no WinEventProc: {ex.Message}");
                ActiveWindowChanged?.Invoke(this, new ActiveWindowEventArgs(0, "Unknown"));
            }
        }

        public void Dispose()
        {
            if (_hook != IntPtr.Zero)
            {
                UnhookWinEvent(_hook);
                _hook = IntPtr.Zero;
            }
        }
    }
}
