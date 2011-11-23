using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Caliburn.Micro;
using System.ComponentModel;
using System.Collections.Specialized;
using Adalbertus.BudgetPlanner.Core;
using Adalbertus.BudgetPlanner.Models;
using Adalbertus.BudgetPlanner.Database;

namespace Adalbertus.BudgetPlanner.ViewModels
{
    public class ExternalSourcesViewModel : BaseViewModel
    {
        public ExternalSourcesViewModel(IShellViewModel shell, IDatabase database, IConfiguration configuration, ICachedService cashedService, IEventAggregator eventAggregator)
            : base(shell, database, configuration, cashedService, eventAggregator)
        {
            _savings = new BindableCollectionExt<Saving>();
            _incomes = new BindableCollectionExt<Income>();

            _savings.PropertyChanged += (s, e) => { OnPropertyChanged(s, e); };
            _incomes.PropertyChanged += (s, e) => { OnPropertyChanged(s, e); };
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
                Contract.Ensures(Contract.Result<BindableCollectionExt<Income>>() != null);
                return _incomes;
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            LoadSavingsData();
            LoadIncomesData();
        }

        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is Entity)
            {
                switch (e.PropertyName)
                {
                    case "Name":
                    case "Description":
                    case "Value":
                        if (sender is Saving)
                        {
                            UpdateSaving(sender as Saving, true);
                        }
                        else if (sender is SavingValue)
                        {
                            //(sender as SavingValue).Saving.Refresh();
                            Update(sender as Entity);
                        }
                        else
                        {
                            Update(sender as Entity);
                        }
                        break;
                }
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
        }

        private void LoadSavingsData()
        {
            Savings.Clear();
            var sql = PetaPoco.Sql.Builder
                .Select("*")
                .From("Saving")
                .InnerJoin("CashFlow")
                .On("CashFlow.Id = Saving.CashFlowId")
                .InnerJoin("CashFlowGroup")
                .On("CashFlow.CashFlowGroupId = CashFlowGroup.Id");


            var savingsList = Database.Query<Saving, CashFlow, CashFlowGroup>(sql);
            savingsList.ForEach(x =>
            {
                var savingValues = Database.Query<SavingValue, Budget, Expense>(PetaPoco.Sql.Builder
                    .Select("*")
                    .From("SavingValue")
                    .LeftJoin("Budget")
                    .On("Budget.Id = SavingValue.BudgetId")
                    .LeftJoin("Expense")
                    .On("Expense.Id = SavingValue.ExpenseId")
                    .Where("SavingId=@0", x.Id)).ToList();
                savingValues.ForEach(v =>
                {
                    if (v.Budget.IsTransient())
                    {
                        v.Budget = null;
                    }
                    if (v.Expense.IsTransient())
                    {
                        v.Expense = null;
                    }
                    v.Saving = x; x.Values.Add(v);
                });
                Savings.Add(x);
                var notifyPropertyChanged = (x.Values as BindableCollectionExt<SavingValue>);

                if (notifyPropertyChanged != null)
                {
                    notifyPropertyChanged.PropertyChanged += OnPropertyChanged;
                }
            });
        }

        private void LoadIncomesData()
        {
            Incomes.Clear();
            var incomesList = Database.Query<Income>().ToList();
            incomesList.ForEach(x =>
            {
                var values = Database.Query<IncomeValue>("WHERE IncomeId = @0", x.Id);
                x.Values.AddRange(values);
            });
            incomesList.ForEach(x => Incomes.Add(x));
        }

        private void Save(Entity entity)
        {
            using (var tx = Database.GetTransaction())
            {
                Database.Save(entity);
                tx.Complete();
            }
        }

        private void Update(Entity entity)
        {
            using (var tx = Database.GetTransaction())
            {
                Database.Update(entity);
                tx.Complete();
            }
        }

        private void Delete(Entity entity)
        {
            using (var tx = Database.GetTransaction())
            {
                Saving saving = entity as Saving;
                if (saving != null)
                {
                    Database.Delete(saving.CashFlow);
                    Database.Delete<SavingValue>("WHERE SavingId=@0", saving.Id);
                }
                Income income = entity as Income;
                if (income != null)
                {
                    Database.Delete<IncomeValue>("WHERE IncomeId=@0", income.Id);
                }
                Database.Delete(entity);
                tx.Complete();
            }
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
            Delete(saving);
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
            }
            LoadSavingsData();
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
            LoadIncomesData();
        }

        public void RemoveIncome(Income income)
        {
            Delete(income);
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
