using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

using System.Windows.Forms;

namespace MultiCod_FPS_Unlocker
{
    public partial class mw2maxfps : Form
    {
        public mw2maxfps()
        {
            InitializeComponent();
            setupconnection();
        }

        private const uint PROCESS_ALL_ACCESS = 0x1F0FFF;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
        byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesRead);

        private const long IMAGE_BASE = 0x140000000;

        private static IntPtr hProc = IntPtr.Zero;
        private static Process targetProcess = null;
        private static IntPtr moduleBase = IntPtr.Zero;

        private void setupconnection()
        {
            targetProcess = Process.GetProcessesByName("iw4mp").FirstOrDefault();
            if (targetProcess == null)
            {
                MessageBox.Show("iw4mp.exe not found. Start MW2 Multiplayer first.");
                return;
            }

            hProc = OpenProcess(PROCESS_ALL_ACCESS, false, targetProcess.Id);
            if (hProc == IntPtr.Zero)
            {
                MessageBox.Show("Failed to open process (try running as administrator).");
                return;
            }

            moduleBase = targetProcess.MainModule.BaseAddress;
        }

        private IntPtr Resolve(long staticAddr)
        {
            long rva = staticAddr - IMAGE_BASE;
            return new IntPtr(moduleBase.ToInt64() + rva);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int fps = int.Parse(maxfpsinput.Text);

            IntPtr dvarPtrAddr = Resolve(0x141D260A8);

            byte[] ptrBuf = new byte[8];
            IntPtr bytesRead;
            if (!ReadProcessMemory(hProc, dvarPtrAddr, ptrBuf, 8, out bytesRead))
            {
                MessageBox.Show("ReadProcessMemory failed: " + Marshal.GetLastWin32Error());
                return;
            }

            long dvarPtr = BitConverter.ToInt64(ptrBuf, 0);
            if (dvarPtr == 0)
            {
                MessageBox.Show("com_maxfps dvar not yet registered.");
                return;
            }

            IntPtr valueAddr = new IntPtr(dvarPtr + 0x10);
            byte[] valueBytes = BitConverter.GetBytes(fps);

            IntPtr bytesWritten;
            if (!WriteProcessMemory(hProc, valueAddr, valueBytes, (uint)valueBytes.Length, out bytesWritten))
            {
                MessageBox.Show("WriteProcessMemory failed: " + Marshal.GetLastWin32Error());
            }
        }
    }
}