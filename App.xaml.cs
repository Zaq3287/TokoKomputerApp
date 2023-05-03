using TokoMAUI.Pages;
#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
#endif

namespace TokoMAUI;

public partial class App : Application
{
    const int WindowWidth = 480;
    const int WindowHeight = 720;

    public App()
	{
		InitializeComponent();

        Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
        {
#if WINDOWS
                var mauiWindow = handler.VirtualView;
                var nativeWindow = handler.PlatformView;
                nativeWindow.Activate();
                IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
                WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
                AppWindow appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
                appWindow.Resize(new SizeInt32(WindowWidth, WindowHeight));

                //var p = appWindow.Presenter as OverlappedPresenter;
                //p.IsResizable = false;
                //p.IsMaximizable = false;
                //p.SetBorderAndTitleBar(false, false);
#endif
        });

        MainPage = new NavigationPage(new pgMainMenu());
        MainPage.BindingContext = new pgMainMenuVM();
    }
}
