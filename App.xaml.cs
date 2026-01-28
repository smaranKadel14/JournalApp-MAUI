using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using System.Runtime.InteropServices;

namespace JournalApp
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Microsoft.Maui.Controls.Window CreateWindow(IActivationState? activationState)
        {
            var window = new Microsoft.Maui.Controls.Window(new MainPage()) { Title = "JournalApp" };
            
            // Set window to truly maximized state on Windows
            if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
            {
                // Handle window created event to properly maximize the window
                window.Created += (s, e) =>
                {
                    // Apply maximize after window is fully created
                    Dispatcher.Dispatch(async () =>
                    {
                        await Task.Delay(200); // Wait for window to be fully ready
                        
                        try
                        {
                            // Get the native window handle
                            if (window.Handler?.PlatformView != null)
                            {
                                var nativeView = window.Handler.PlatformView;
                                var windowHandleProperty = nativeView.GetType().GetProperty("WindowHandle");
                                if (windowHandleProperty?.GetValue(nativeView) is IntPtr windowHandle)
                                {
                                    // Use ShowWindow with SW_MAXIMIZE to truly maximize the window
                                    ShowWindow(windowHandle, SW_MAXIMIZE);
                                }
                            }
                        }
                        catch
                        {
                            // Fallback to manual sizing if maximize fails
                            var screenWidth = DeviceDisplay.Current.MainDisplayInfo.Width;
                            var screenHeight = DeviceDisplay.Current.MainDisplayInfo.Height;
                            window.Width = screenWidth;
                            window.Height = screenHeight;
                            window.X = 0;
                            window.Y = 0;
                        }
                    });
                };
            }
            
            return window;
        }
        
        // Win32 API declarations for maximizing window
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        
        // Constants for ShowWindow
        private const int SW_MAXIMIZE = 3;
    }
}
