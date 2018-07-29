using System;

using Android.App;
using Android.Content.PM;
using Android.OS;

using ImageCircle.Forms.Plugin.Droid;
using Android.Graphics;

namespace SmartParkingLiFi.Droid
{
	[Activity (Label = "SmartParkingLiFi", Icon = "@drawable/icon", Theme="@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate (bundle);

            ImageCircleRenderer.Init();

            global::Xamarin.Forms.Forms.Init (this, bundle);
			LoadApplication (new SmartParkingLiFi.App());
		}
	}
}

