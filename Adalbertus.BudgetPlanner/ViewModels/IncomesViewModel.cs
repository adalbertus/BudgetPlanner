using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Core;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Extensions;
using Adalbertus.BudgetPlanner.Models;
using Caliburn.Micro;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class IncomesViewModel : BaseViewModel
    {
        public IncomesViewModel(IShellViewModel shell, IDatabase database, IConfigurationManager configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(shell, database, configuration, cashedService, eventAggregator)
        {
            _incomes = new BindableCollectionExt<Income>();
        }

        private BindableCollectionExt<Income> _incomes;
        public BindableCollectionExt<Income> Incomes
        {
            get
            {
                return _incomes;
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            LoadIncomesData();
        }

        private void LoadIncomesData()
        {
            Incomes.Clear();
            var incomesList = CachedService.GetAllIncomes();
            incomesList.ForEach(x =>
            {
                Incomes.Add(x);
                x.PropertyChanged += (s, e) =>
                {
                    Save(x);
                    CachedService.Clear(CachedServiceKeys.AllIncomes);
                };
            });
        }

        #region Incomes
        public void AddIncome()
        {
            string incomeDefaultName = CreateUniqueName("Dochody", Incomes.Select(x => x.Name).ToList());
            var income = new Income
            {
                Name = incomeDefaultName,
            };
            Save(income);
            CachedService.Clear(CachedServiceKeys.AllIncomes);
            LoadIncomesData();
        }

        public void RemoveIncome(Income income)
        {
            RemoveIncome(income, false);
        }

        public void RemoveIncome(Income income, bool omitConfirmation)
        {
            if (!omitConfirmation)
            {
                var hasIncomeValuesDefined = Database.ExecuteScalar<int>(PetaPoco.Sql.Builder
                        .Select("COUNT(*)")
                        .From("IncomeValue")
                        .Where("IncomeId = @0", income.Id)) > 0;
                if (hasIncomeValuesDefined)
                {
                    var message = string.Format("Dochód \"{0}\" jest już używany w budżecie. Usunięcie go spowoduje usunięcie jego wystąpień we wszystkich budżetach.\r\n\r\nCzy chcesz kontynuować?", income.Name);
                    Shell.ShowMessage(message, () => RemoveIncome(income, true), null);
                    return;
                }
            }
            using (var tx = Database.GetTransaction())
            {
                Database.Delete<IncomeValue>("WHERE IncomeId = @0", income.Id);
                Database.Delete(income);
                tx.Complete();
            }
            CachedService.Clear(CachedServiceKeys.AllIncomes);
            LoadIncomesData();
        }

        #endregion

        private string CreateUniqueName(string prefix, List<string> names)
        {
            string uniqueNameFormat = "{0} {1}";
            string uniqueName = string.Format(uniqueNameFormat, prefix, names.Count + 1);
            int counter = 2;
            while (names.Any(x => x.Equals(uniqueName)))
            {
                uniqueName = string.Format(uniqueNameFormat, prefix, names.Count + counter++);
            }

            return uniqueName;
        }


    }
}
