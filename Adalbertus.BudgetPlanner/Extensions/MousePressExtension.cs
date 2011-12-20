using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Adalbertus.BudgetPlanner.Extensions
{
    public class MousePressExtension
    {


        public static bool GetIsLeftButtonPressed(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsLeftButtonPressedProperty);
        }

        public static void SetIsLeftButtonPressed(DependencyObject obj, bool value)
        {
            obj.SetValue(IsLeftButtonPressedProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsLeftButtonPressed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsLeftButtonPressedProperty =
            DependencyProperty.RegisterAttached("IsLeftButtonPressed", typeof(bool), typeof(MousePressExtension), new UIPropertyMetadata(false));



        public static bool GetEnableMousePressEvents(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableMousePressEventsProperty);
        }

        public static void SetEnableMousePressEvents(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableMousePressEventsProperty, value);
        }

        // Using a DependencyProperty as the backing store for EnableMousePressEvents.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableMousePressEventsProperty =
            DependencyProperty.RegisterAttached("EnableMousePressEvents", typeof(bool), typeof(MousePressExtension), new UIPropertyMetadata(false, OnIsLeftButtonPressedPropertyChanged));


        private static void OnIsLeftButtonPressedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var uie = (UIElement)d;
            if (uie == null)
            {
                return;
            }
            if (MousePressExtension.GetEnableMousePressEvents(d))
            {
                uie.MouseLeftButtonDown += (s, ee) =>
                    {
                        MousePressExtension.SetIsLeftButtonPressed(d, true);
                    };
            }
        }

        
    }
}
