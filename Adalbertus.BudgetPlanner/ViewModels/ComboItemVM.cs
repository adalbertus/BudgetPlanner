using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class ComboItemVM<T>
    {
        public T Value { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public ComboItemVM()
        {
        }

        public ComboItemVM(string name, T value)
        {
            Name = name;
            Value = value;
        }
    }
}
