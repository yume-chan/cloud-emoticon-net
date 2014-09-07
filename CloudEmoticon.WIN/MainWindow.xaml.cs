using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace CloudEmoticon
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SourceInitialized += MainWindow_SourceInitialized;
            Closing += MainWindow_Closing;

            DataContext = App.ViewModel;
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.Settings["windowstate"] = WindowState == WindowState.Maximized;
            App.Settings["position"] = RestoreBounds;
        }

        IntPtr handle = default(IntPtr);

        void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            if (App.Settings["position"] != null)
            {
                WindowState = (bool)App.Settings["windowstate"] ? WindowState.Maximized : WindowState.Normal;
                Rect position = (Rect)App.Settings["position"];
                Left = position.Left;
                Top = position.Top;
                Width = position.Width;
                Height = position.Height;
            }

            handle = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WinProc));
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern WindowStyles GetWindowLong(IntPtr hWnd, int nIndex);

        [Flags]
        enum WindowStyles : uint
        {
            MAXIMIZE = 0x1000000
        }

        private static IntPtr WinProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x83)
            {
                if (wParam != IntPtr.Zero)
                {
                    if (GetWindowLong(hwnd, -16).HasFlag(WindowStyles.MAXIMIZE))
                    {
                        RECT clientRegion = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));
                        clientRegion.left += 8;
                        clientRegion.top += 8;
                        clientRegion.right = clientRegion.right - 8;
                        clientRegion.bottom = clientRegion.bottom - 9;

                        Marshal.StructureToPtr(clientRegion, lParam, true);
                        handled = true;
                    }
                }
            }

            return (IntPtr)0;
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam = default(IntPtr));

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            PostMessage(handle, 0x112, new IntPtr(0xF060));
        }

        private void maximizeButton_Click(object sender, RoutedEventArgs e)
        {
            PostMessage(handle, 0x112, new IntPtr(WindowState == WindowState.Normal ? 0xF030 : 0xF120));
        }

        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            PostMessage(handle, 0x112, new IntPtr(0xF020));
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.ViewModel.Repositories.Count == 0)
                App.ViewModel.Repositories.Add("https://dl.dropboxusercontent.com/u/120725807/test.xml");
            await App.ViewModel.EmoticonList.UpdateRepositories();
        }

        private void ListBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debugger.Break();
        }
    }
}
