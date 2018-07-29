using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Graphics.Drawables;
using SmartParkingLiFi.CustomControls;
using Android.CustomRenderers;

[assembly: ExportRenderer(typeof(ButtonL), typeof(ButtonLRenderer))]
namespace Android.CustomRenderers
{
    class ButtonLRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                GradientDrawable gradientDrawable = new GradientDrawable();
                gradientDrawable.SetColor(Element.BorderColor.ToAndroid(SmartParkingLiFi.Graphics.MainColor));
                gradientDrawable.SetCornerRadius(15);
                Control.SetBackground(gradientDrawable);
            }
        }
    }
}