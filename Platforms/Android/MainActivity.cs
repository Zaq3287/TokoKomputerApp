using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace TokoMAUI;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{

    protected override void OnCreate(Bundle bundle)
    {
        base.OnCreate(bundle);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
        {
            Window.SetStatusBarColor(Android.Graphics.Color.Argb(255, 0, 0, 0)); //here
        }
    }
}


