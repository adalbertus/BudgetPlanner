using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interactivity;
using System.Windows;
using System.Windows.Input;

namespace Adalbertus.BudgetPlanner.Extensions
{
    public class KeyDownFocusAction : TriggerAction<UIElement>
    {
        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("Key", typeof(Key), typeof(KeyDownFocusAction));
        public Key Key
        {
            get { return (Key)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof(UIElement), typeof(KeyDownFocusAction), new UIPropertyMetadata(null));
        public UIElement Target
        {
            get { return (UIElement)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        public static readonly DependencyProperty ModifiersProperty =
        DependencyProperty.Register("Modifiers", typeof(ModifierKeys), typeof(KeyTrigger), null);
        public ModifierKeys Modifiers
        {
            get { return (ModifierKeys)GetValue(ModifiersProperty); }
            set { SetValue(ModifiersProperty, value); }
        }

        protected override void Invoke(object parameter)
        {
            if (Keyboard.IsKeyDown(Key))
            {
                Target.Focus();
            }
        }
    }
}
