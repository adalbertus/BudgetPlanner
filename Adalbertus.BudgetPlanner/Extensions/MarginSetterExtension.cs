using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Adalbertus.BudgetPlanner.Extensions
{
    /// <summary>
    /// http://blogs.microsoft.co.il/blogs/eladkatz/archive/2011/05/29/what-is-the-easiest-way-to-set-spacing-between-items-in-stackpanel.aspx
    /// </summary>
    public class MarginSetterExtension
    {
        public static Thickness GetMargin(DependencyObject obj)
        {
            return (Thickness)obj.GetValue(MarginProperty);
        }

        public static void SetMargin(DependencyObject obj, Thickness value)
        {
            obj.SetValue(MarginProperty, value);
        }

        public static readonly DependencyProperty MarginProperty =
            DependencyProperty.RegisterAttached("Margin", typeof(Thickness), typeof(MarginSetterExtension), new UIPropertyMetadata(new Thickness(), OnMarginChanged));

        private static void OnMarginChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Make sure this is put on a panel
            var panel = sender as Panel;

            if (panel == null)
            {
                return;
            }

            panel.Loaded += (s, ea) =>
            {
                var p = sender as Panel;

                // Go over the children and set margin for them:
                foreach (var child in p.Children)
                {
                    var fe = child as FrameworkElement;
                    if (fe == null) continue;
                    fe.Margin = MarginSetterExtension.GetMargin(p);
                }
            };
        }

        private enum MarginSides
        {
            Left,
            Top,
            Right,
            Bottom
        }

        public static double GetMarginLeft(DependencyObject obj)
        {
            return (double)obj.GetValue(MarginLeftProperty);
        }

        public static void SetMarginLeft(DependencyObject obj, double value)
        {
            obj.SetValue(MarginLeftProperty, value);
        }

        public static readonly DependencyProperty MarginLeftProperty =
            DependencyProperty.RegisterAttached("MarginLeft", typeof(double), typeof(MarginSetterExtension), new UIPropertyMetadata(default(double), OnMarginLeftChanged));

        private static void OnMarginLeftChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            MarginSetterExtension.ChangeMarginSide(sender as FrameworkElement, MarginSides.Left);
        }

        public static double GetMarginTop(DependencyObject obj)
        {
            return (double)obj.GetValue(MarginTopProperty);
        }

        public static void SetMarginTop(DependencyObject obj, double value)
        {
            obj.SetValue(MarginTopProperty, value);
        }

        public static readonly DependencyProperty MarginTopProperty =
            DependencyProperty.RegisterAttached("MarginTop", typeof(double), typeof(MarginSetterExtension), new UIPropertyMetadata(default(double), OnMarginTopChanged));

        private static void OnMarginTopChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            MarginSetterExtension.ChangeMarginSide(sender as FrameworkElement, MarginSides.Top);
        }

        public static double GetMarginRight(DependencyObject obj)
        {
            return (double)obj.GetValue(MarginRightProperty);
        }

        public static void SetMarginRight(DependencyObject obj, double value)
        {
            obj.SetValue(MarginRightProperty, value);
        }

        public static readonly DependencyProperty MarginRightProperty =
            DependencyProperty.RegisterAttached("MarginRight", typeof(double), typeof(MarginSetterExtension), new UIPropertyMetadata(default(double), OnMarginRightChanged));

        private static void OnMarginRightChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            MarginSetterExtension.ChangeMarginSide(sender as FrameworkElement, MarginSides.Right);
        }

        public static double GetMarginBottom(DependencyObject obj)
        {
            return (double)obj.GetValue(MarginBottomProperty);
        }

        public static void SetMarginBottom(DependencyObject obj, double value)
        {
            obj.SetValue(MarginBottomProperty, value);
        }

        public static readonly DependencyProperty MarginBottomProperty =
            DependencyProperty.RegisterAttached("MarginBottom", typeof(double), typeof(MarginSetterExtension), new UIPropertyMetadata(default(double), OnMarginBottomChanged));

        private static void OnMarginBottomChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            MarginSetterExtension.ChangeMarginSide(sender as FrameworkElement, MarginSides.Bottom);
        }

        
        private static void ChangeMarginSide(FrameworkElement frameworkElement, MarginSides marginSide)
        {
            if (frameworkElement == null)
            {
                return;
            }
            var margin = frameworkElement.Margin;
            switch(marginSide)
            {
                case MarginSides.Left:
                    margin.Left = MarginSetterExtension.GetMarginLeft(frameworkElement);
                    break;
                case MarginSides.Top:
                    margin.Top = MarginSetterExtension.GetMarginTop(frameworkElement);
                    break;
                case MarginSides.Right:
                    margin.Right = MarginSetterExtension.GetMarginRight(frameworkElement);
                    break;
                case MarginSides.Bottom:
                    margin.Bottom = MarginSetterExtension.GetMarginBottom(frameworkElement);
                    break;
            }
            frameworkElement.Margin = margin;
        }

    }
}
