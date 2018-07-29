using SmartParkingLiFi.CustomControls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Graphics.Drawables;
using Android.CustomRenderers;

[assembly: ExportRenderer(typeof(EntryL), typeof(EntryLRenderer))]
namespace Android.CustomRenderers
{
    class EntryLRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                GradientDrawable gradientDrawable = new GradientDrawable();
                gradientDrawable.SetColor(Element.BackgroundColor.ToAndroid(Color.White));
                gradientDrawable.SetCornerRadius(15);
                Control.SetBackground(gradientDrawable);
                Control.SetPadding(185, 35, 0, 0);
            }
        }
        
    }
}