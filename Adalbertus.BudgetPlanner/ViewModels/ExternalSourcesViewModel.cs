using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Caliburn.Micro;
using System.ComponentModel;
using System.Collections.Specialized;
using Adalbertus.BudgetPlanner.Extensions;
using Adalbertus.BudgetPlanner.Core;
using Adalbertus.BudgetPlanner.Models;
using Adalbertus.BudgetPlanner.Database;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class ExternalSourcesViewModel : BaseViewModel
    {
        public ExternalSourcesViewModel(IShellViewModel shell, IDatabase database, IConfigurationManager configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(shell, database, configuration, cashedService, eventAggregator)
        {
            _savings = new BindableCollectionExt<Saving>();
            _incomes = new BindableCollectionExt<Income>();

            _savings.PropertyChanged += OnSavingPropertyChanged;
        }

        private BindableCollectionExt<Saving> _savings;
        public BindableCollectionExt<Saving> Savings
        {
            get
            {
                Contract.Ensures(Contract.Result<BindableCollectionExt<Saving>>() != null);
                return _savings;
            }
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
            LoadSavingsData();
            LoadIncomesData();
        }

        protected void OnSavingPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is Entity)
            {
                switch (e.PropertyName)
                {
                    case "Name":
                    case "Description":
                    case "Value":
                        var saving = sender as Saving;
                        var savingValue = sender as SavingValue;
                        if (saving != null)
                        {
                            UpdateSaving(saving, true);
                        }

                        if (savingValue != null)
                        {
                            Save(savingValue);
                            savingValue.Saving.Refresh();
                            CachedService.Clear(CachedServiceKeys.AllSavings);
                            CachedService.Clear(CachedServiceKeys.AllIncomes);
                        }
                        break;
                }
            }
        }

        private void LoadSavingsData()
        {
            Savings.Clear();

            var savingsList = CachedService.GetAllSavings();
            savingsList.ForEach(x =>
            {
                Savings.Add(x);
                var notifyPropertyChanged = (x.Values as BindableCollectionExt<SavingValue>);
                if (notifyPropertyChanged != null)
                {
                    notifyPropertyChanged.PropertyChanged += OnSavingPropertyChanged;
                }
            });
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

        #region Savings
        public void AddSaving()
        {
            string savingDefaultName = CreateUniqueName("Oszczędności", Savings.Select(x => x.Name).ToList());
            var cashFlowGroup = Database.Query<CashFlowGroup>("WHERE Name = 'Oszczędności'").First();
            var saving = new Saving
                {
                    Name = savingDefaultName,
                };

            saving.CashFlow.Group = cashFlowGroup;
            SaveSaving(saving);
            LoadSavingsData();
        }

        public void RemoveSaving(Saving saving)
        {
            DeleteSaving(saving);
            LoadSavingsData();
        }

        public void AddSavingValue(Saving saving)
        {
            var savingValue = saving.Deposit(null, 0, DateTime.Now);
            using (var tx = Database.GetTransaction())
            {
                Database.Save(savingValue);
                tx.Complete();
            }
            LoadSavingsData();
        }

        public void RemoveSavingValue(SavingValue savingValue)
        {
            using (var tx = Database.GetTransaction())
            {
                Database.Delete(savingValue);
                tx.Complete();
                CachedService.Clear(CachedServiceKeys.AllSavings);
            }
            LoadSavingsData();
        }

        private void DeleteSaving(Saving saving)
        {
            using (var tx = Database.GetTransaction())
            {
                Database.Delete(saving.CashFlow);
                Database.Delete(saving);
                tx.Complete();
                CachedService.Clear(CachedServiceKeys.AllSavings);
                CachedService.Clear(CachedServiceKeys.AllCashFlows);
            }
        }

        private CashFlow GetSavingCashFlow(Entity entity)
        {
            if (entity == null)
            {
                return null;
            }

            CashFlow cashFlow = null;

            if (entity is Saving)
            {
                Saving saving = entity as Saving;
                cashFlow = Database.SingleOrDefault<CashFlow>("WHERE Id=@0", saving.CashFlowId);
            }
            return cashFlow;
        }

        private void SaveSaving(Saving saving)
        {
            using (var tx = Database.GetTransaction())
            {
                if (Database.IsNew(saving.CashFlow))
                {
                    Database.Save(saving.CashFlow);
                }
                Database.Save(saving);

                tx.Complete();
                CachedService.Clear(CachedServiceKeys.AllSavings);
                CachedService.Clear(CachedServiceKeys.AllCashFlows);
            }
        }

        private void UpdateSaving(Saving saving, bool updateCashFlow = false)
        {
            using (var tx = Database.GetTransaction())
            {
                Database.Update(saving);
                if (updateCashFlow)
                {
                    saving.CashFlow.Name = saving.Name;
                    Database.Update(saving.CashFlow);
                }
                tx.Complete();
                CachedService.Clear(CachedServiceKeys.AllSavings);
                CachedService.Clear(CachedServiceKeys.AllCashFlows);
            }
        }
        #endregion

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
