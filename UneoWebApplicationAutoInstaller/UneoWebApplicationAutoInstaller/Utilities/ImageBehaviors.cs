using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace UneoWebApplicationAutoInstaller.Utilities
{
    public class ImageBehaviors
    {
        public static readonly DependencyProperty IsRotatingProperty =
            DependencyProperty.RegisterAttached(
                "IsRotating",
                typeof(bool),
                typeof(ImageBehaviors),
                new PropertyMetadata(false, OnIsRotatingChanged));

        public static bool GetIsRotating(UIElement element) => (bool)element.GetValue(IsRotatingProperty);
        public static void SetIsRotating(UIElement element, bool value) => element.SetValue(IsRotatingProperty, value);

        private static void OnIsRotatingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Image img)
            {
                if (img.RenderTransform is not RotateTransform rotateTransform)
                {
                    rotateTransform = new RotateTransform(0);
                    img.RenderTransform = rotateTransform;
                    img.RenderTransformOrigin = new Point(0.5, 0.5); // center
                }

                if ((bool)e.NewValue)
                {
                    var anim = new DoubleAnimation
                    {
                        From = 0,
                        To = 360,
                        Duration = TimeSpan.FromSeconds(1.92),
                        RepeatBehavior = RepeatBehavior.Forever
                    };
                    rotateTransform.BeginAnimation(RotateTransform.AngleProperty, anim);
                }
                else
                {
                    rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null);
                }
            }
        }
    }
}
