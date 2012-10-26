using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class WrappedViewModel<T> : PropertyChangedBase
    {
        #region Private fields
        #endregion Private fields

        #region Constructors
        public WrappedViewModel(T item)
        {
            WrappedItem = item;
        }
        #endregion Constructors

        #region Properties
        public T WrappedItem { get; private set; }
        #endregion Properties

        #region Public methods
        #endregion Public methods

        #region Private methods
        #endregion Private methods
    }
}
