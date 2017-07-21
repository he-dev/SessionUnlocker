using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SessionUnlocker
{
    public partial class SessionChangeHandler : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        // Constants that can be passed for the dwFlags parameter.
        private const int NOTIFY_FOR_THIS_SESSION = 0;
        private const int NOTIFY_FOR_ALL_SESSIONS = 1;

        private const int WM_WTSSESSION_CHANGE = 0x2b1;
        private const int WTS_SESSION_LOCK = 0x7;
        private const int WTS_SESSION_UNLOCK = 0x8;

        private const int HWND_BROADCAST = 0xFFFF;

        [DllImport("WtsApi32.dll")]
        private static extern bool WTSRegisterSessionNotification(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)]int dwFlags);

        [DllImport("WtsApi32.dll")]
        private static extern bool WTSUnRegisterSessionNotification(IntPtr hWnd);

        public SessionChangeHandler()
        {
            InitializeComponent();
            if (!WTSRegisterSessionNotification(Handle, NOTIFY_FOR_THIS_SESSION))
            {
                Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
            }            
        }

        //public event EventHandler MachineLocked;

        //public event EventHandler MachineUnlocked;

        protected override void OnHandleDestroyed(EventArgs e)
        {
            // Unregister the handle before it gets destroyed.
            WTSUnRegisterSessionNotification(Handle);
            base.OnHandleDestroyed(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_WTSSESSION_CHANGE)
            {
                int value = m.WParam.ToInt32();
                switch (m.WParam.ToInt32())
                {
                    case WTS_SESSION_LOCK:
                        OnMachineLocked(EventArgs.Empty);
                        break;
                    case WTS_SESSION_UNLOCK:
                        OnMachineUnlocked(EventArgs.Empty);
                        break;
                    default:
                        base.WndProc(ref m);
                        break;
                }
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        private void OnMachineLocked(EventArgs e)
        {
            //MachineLocked?.Invoke(this, e);

            PostMessage((IntPtr)HWND_BROADCAST, WM_WTSSESSION_CHANGE, WTS_SESSION_UNLOCK, Process.GetCurrentProcess().SessionId);
        }

        private void OnMachineUnlocked(EventArgs e)
        {
            //MachineUnlocked?.Invoke(this, e);
        }

        private void SessionChangeHandler_Load(object sender, EventArgs e)
        {

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void SessionChangeHandler_Resize(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case FormWindowState.Minimized:
                    Hide();
                    break;
                case FormWindowState.Normal:
                    //Visible = false;
                    break;
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }
    }
}
