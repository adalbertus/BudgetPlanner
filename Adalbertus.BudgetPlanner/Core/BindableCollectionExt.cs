using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Caliburn.Micro;

namespace Adalbertus.BudgetPlanner.Core
{
    public class BindableCollectionExt<T> : BindableCollection<T>
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        public BindableCollectionExt()
            : base()
        {
        }

        public BindableCollectionExt(IEnumerable<T> collection)
            : base(collection)
        {
        }

        protected override void InsertItemBase(int index, T item)
        {
            AttachPropertyChanged(item);
            base.InsertItemBase(index, item);
        }

        public override void AddRange(IEnumerable<T> items)
        {
            AttachPropertyChanged(items);
            base.AddRange(items);
        }

        protected override void RemoveItemBase(int index)
        {
            DeattachPropertyChanged(this[index]);
            base.RemoveItemBase(index);
        }

        public override void RemoveRange(IEnumerable<T> items)
        {
            DeattachPropertyChanged(items);
            base.RemoveRange(items);
        }

        private void AttachPropertyChanged(T item)
        {
            if (item is INotifyPropertyChanged)
            {
                (item as INotifyPropertyChanged).PropertyChanged += ItemPropertyChanged;
            }
        }

        private void AttachPropertyChanged(IEnumerable<T> items)
        {
            if (IsNotifying)
            {
                foreach (T item in items)
                {
                    AttachPropertyChanged(item);
                }
            }
        }

        private void DeattachPropertyChanged(T item)
        {
            if (IsNotifying)
            {
                if (item is INotifyPropertyChanged)
                {
                    (item as INotifyPropertyChanged).PropertyChanged -= ItemPropertyChanged;
                }
            }
        }

        private void DeattachPropertyChanged(IEnumerable<T> items)
        {
            if (IsNotifying)
            {
                foreach (T item in items)
                {
                    DeattachPropertyChanged(item);
                }
            }
        }

        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (IsNotifying)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(sender, e);
                }
            }
        }

        internal object Where(Func<Models.Expense, int, bool> func)
        {
            throw new NotImplementedException();
        }
    }
}
