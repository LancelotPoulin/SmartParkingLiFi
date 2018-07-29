using SmartParkingLiFi.CustomControls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Graphics.Drawables;
using Android.CustomRenderers;

[assembly: ExportRenderer(typeof(ButtonTransparent), typeof(ButtonTransparentRenderer))]
namespace Android.CustomRenderers
{
    class ButtonTransparentRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                GradientDrawable gradientDrawable = new GradientDrawable();
                gradientDrawable.SetColor(Element.BorderColor.ToAndroid(Color.Transparent));
                Control.SetBackground(gradientDrawable);
            }
        }
    }
}