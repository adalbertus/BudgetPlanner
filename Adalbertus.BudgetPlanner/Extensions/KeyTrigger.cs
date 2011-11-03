using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interactivity;
using System.Windows;
using System.Windows.Input;

namespace Adalbertus.BudgetPlanner.Extensions
{
    /// <summary>
    /// <remarks>Based on http://caliburnmicro.codeplex.com/discussions/243905</remarks>
    /// </summary>
    public class KeyTrigger : TriggerBase<UIElement>
    {
        public static readonly DependencyProperty KeyProperty =
        DependencyProperty.Register("Key", typeof(Key), typeof(KeyTrigger), null);

        public static readonly DependencyProperty ModifiersProperty =
        DependencyProperty.Register("Modifiers", typeof(ModifierKeys), typeof(KeyTrigger), null);


        public KeyTrigger()
        {
        }

        public Key Key
        {
            get { return (Key)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }


        public ModifierKeys Modifiers
        {
            get { return (ModifierKeys)GetValue(ModifiersProperty); }
            set { SetValue(ModifiersProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.KeyDown += AssociatedObject_KeyDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            this.AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
        }

        private void AssociatedObject_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == this.Key) && (Keyboard.Modifiers == GetActualModifiers(e.Key, this.Modifiers)))
            {
                base.InvokeActions(e);
            }
        }

        private static ModifierKeys GetActualModifiers(Key key, ModifierKeys modifiers)
        {
            if (key == (Key.LeftCtrl | Key.RightCtrl))
            {
                modifiers |= ModifierKeys.Control;
                return modifiers;
            }
            if (key == (Key.LeftAlt | Key.RightAlt))
            {
                modifiers |= ModifierKeys.Alt;
                return modifiers;
            }
            if (key == Key.LeftShift)
            {
                modifiers |= ModifierKeys.Shift;
            }

            return modifiers;
        }
    }
}
