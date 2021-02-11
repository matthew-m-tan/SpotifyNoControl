using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace WindowsFormsApp1
{
    static class Program
    {

        #region Imports
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        #endregion

        #region enums & consts
        enum GetWindow_CMD
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }

        const int SWP_NOOWNERZORDER = 0x0200;

        const string CHILD_LAYER_CLASS = "Chrome_Widgetwin_0";
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            // Helper function to launch Spotify
            LaunchSpotify();

            // Get the windows UI element that is a child of the Spotify main window using 'Chrome_Widgetwin_0' layer class
            IntPtr child = new IntPtr(0), parent = new IntPtr(0);
            int parentPId = 0;

            // Need to loop to prevent race condition i.e. Spotify launches after we looked for its thread
            while(parentPId == 0)
            {
                child = FindWindow(CHILD_LAYER_CLASS, null);
                parent = GetParent(child);
                GetWindowThreadProcessId(parent, out parentPId);
                Thread.Sleep(100);
            }

            // Use the parent process ID to get the the window control - Window controls are always prepended to the main window
            IntPtr dots = GetWindow(parent, (uint)GetWindow_CMD.GW_HWNDPREV);
            IntPtr minMaxClose = GetWindow(dots, (uint)GetWindow_CMD.GW_HWNDPREV);

            // Set window positions to "hide" the controls
            SetWindowPos(dots, 1, 0, 0, 0, 0, SWP_NOOWNERZORDER);
            SetWindowPos(minMaxClose, 1, 0, 0, 0, 0, SWP_NOOWNERZORDER);
        }

        /// <summary>
        /// Launches spotify application by getting the executable in AppData
        /// </summary>
        private static void LaunchSpotify()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            StringBuilder spotifyPath = new StringBuilder(appData).Append("\\Spotify\\Spotify.exe");
            Process.Start(spotifyPath.ToString());
        }
    }
}
