using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Core;
using Adalbertus.BudgetPlanner.Models;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public partial class BudgetPlanViewModel : BaseViewModel
    {
        #region Budget revenues: Income
        private BindableCollectionExt<Income> _availableIncomes;
        public BindableCollectionExt<Income> AvailableIncomes
        {
            get { return _availableIncomes; }
            set { _availableIncomes = value; }
        }

        private Income _selectedAvailableIncome;

        public Income SelectedAvailableIncome
        {
            get { return _selectedAvailableIncome; }
            set
            {
                _selectedAvailableIncome = value;
                NotifyOfPropertyChange(() => SelectedAvailableIncome);
            }
        }

        private DateTime _incomeValueDate;
        public DateTime IncomeValueDate
        {
            get { return _incomeValueDate; }
            set
            {
                _incomeValueDate = value;
                NotifyOfPropertyChange(() => IncomeValueDate);
                NotifyOfPropertyChange(() => CanAddIncomeValue);
            }
        }

        private decimal _incomeValueValue;
        public decimal IncomeValueValue
        {
            get { return _incomeValueValue; }
            set
            {
                _incomeValueValue = value;
                NotifyOfPropertyChange(() => IncomeValueValue);
                NotifyOfPropertyChange(() => CanAddIncomeValue);
            }
        }

        private string _incomeValueDescription;
        public string IncomeValueDescription
        {
            get { return _incomeValueDescription; }
            set
            {
                _incomeValueDescription = value;
                NotifyOfPropertyChange(() => IncomeValueDescription);
            }
        }


        public decimal SumOfRevenueIncomes
        {
            get
            {
                return _budget.IncomeValues.Sum(x => x.Value);
            }
        }

        public bool CanAddIncomeValue
        {
            get
            {
                bool isIncomeSelected = SelectedAvailableIncome != null;
                bool isIncomeEntryValueNotZero = IncomeValueValue > 0;

                return isIncomeSelected && isIncomeEntryValueNotZero;
            }
        }

        public IList<IncomeValue> BudgetIncomeValues
        {
            get { return _budget.IncomeValues; }
        }

        private void LoadAvailableIncomes()
        {
            var incomeValues = Database.Query<IncomeValue, Income, Budget>(PetaPoco.Sql.Builder
                    .Select("*")
                    .From("IncomeValue")
                    .InnerJoin("Income")
                    .On("Income.Id = IncomeValue.IncomeId")
                    .InnerJoin("Budget")
                    .On("Budget.Id = IncomeValue.BudgetId")
                    .Where("IncomeValue.BudgetId = @0", _budget.Id));
            BudgetIncomeValues.Clear();
            incomeValues.ForEach(x => BudgetIncomeValues.Add(x));

            AvailableIncomes.Clear();
            var incomes = Database.Query<Income>("ORDER BY Name ASC");
            AvailableIncomes.AddRange(incomes);

            IncomeValueDate = DateTime.Now;
            IncomeValueValue = 0;
            SelectedAvailableIncome = null;
            NotifyOfPropertyChange(() => BudgetIncomeValues);
            RefreshRevenueSums();
        }

        public void AddIncomeValue()
        {
            if (_budget == null)
            {
                throw new InvalidOperationException("Unable to find the Budget");
            }

            if (SelectedAvailableIncome != null)
            {
                using (var tx = Database.GetTransaction())
                {
                    var incomeValue = _budget.AddIncomeValue(SelectedAvailableIncome, IncomeValueValue, IncomeValueDate, IncomeValueDescription);
                    Database.Save(incomeValue);
                    tx.Complete();
                }
                LoadAvailableIncomes();
            }
        }

        public void RemoveIncomeValue(IncomeValue incomeValue)
        {
            if (_budget == null)
            {
                throw new InvalidOperationException("Unable to find Budget");
            }

            using (var tx = Database.GetTransaction())
            {
                _budget.RemoveIncomeValue(incomeValue);
                Database.Delete(incomeValue);
                tx.Complete();
            }
            LoadAvailableIncomes();
        }

        private void AttachToIncomeValues()
        {
            var incomeValues = (_budget.IncomeValues as BindableCollectionExt<IncomeValue>);
            incomeValues.CollectionChanged += BudgetIncomeValuesCollectionChanged;
            incomeValues.PropertyChanged += BudgetPropertyChanged;
        }

        private void DetachFromIncomeValues()
        {
            var incomeValues = (_budget.IncomeValues as BindableCollectionExt<IncomeValue>);
            incomeValues.CollectionChanged -= BudgetIncomeValuesCollectionChanged;
            incomeValues.PropertyChanged -= BudgetPropertyChanged;
        }

        private void BudgetIncomeValuesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshRevenueSums();
        }
        #endregion
    }
}
