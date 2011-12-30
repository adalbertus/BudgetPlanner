using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Models;
using Adalbertus.BudgetPlanner.Core;

namespace Adalbertus.BudgetPlanner.Database
{
    public struct CachedServiceKeys
    {
        public const string AllIncomes = "{D6582D43-C97A-4E0A-A467-766113E77639}";
        public const string AllSavings = "{C73C3425-F43F-4CAC-A2E7-C8438F79F528}";
        public const string AllCashFlows = "{45C2830E-8256-4D20-A332-EFAC5BF34D6C}";
        public const string AllCashFlowGroups = "{D5D25298-C18D-4AD5-859C-45EEC9BADC58}";
        public const string AllEquations = "{990C0C95-7153-48B1-8890-5AD7C034A8E9}";
    }

    public class CachedService : ICachedService
    {
        public Dictionary<string, object> Cache { get; private set; }
        public IDatabase Database { get; private set; }

        public CachedService(IDatabase database)
        {
            Cache    = new Dictionary<string, object>();
            Database = database;
        }

        public void LoadAll()
        {
            GetAllCashFlows();
            GetAllCashFlowGroups();
            GetAllIncomes();
            GetAllSavings();
            GetAllEquations();
        }

        public void Clear(string key = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Cache[CachedServiceKeys.AllCashFlows] = null;
                Cache[CachedServiceKeys.AllCashFlowGroups] = null;
                Cache[CachedServiceKeys.AllIncomes] = null;
                Cache[CachedServiceKeys.AllSavings] = null;
                Cache[CachedServiceKeys.AllEquations] = null;
            }
            else if(Cache.ContainsKey(key))
            {
                Cache[key] = null;
            }
        }

        public IEnumerable<CashFlow> GetAllCashFlows()
        {
            if (Cache.ContainsKey(CachedServiceKeys.AllCashFlows) && Cache[CachedServiceKeys.AllCashFlows] != null)
            {
                return Cache[CachedServiceKeys.AllCashFlows] as IEnumerable<CashFlow>;
            }
            var sql = PetaPoco.Sql.Builder
                .Select("*")
                .From("CashFlow")
                .InnerJoin("CashFlowGroup")
                .On("CashFlow.CashFlowGroupId = CashFlowGroup.Id")
                .LeftJoin("Saving")
                .On("Saving.CashFlowId = CashFlow.Id")
                .OrderBy("CashFlowGroup.Position ASC");
            var cashFlowList = Database.Query<CashFlow, CashFlowGroup, Saving>(sql).ToList();
            cashFlowList.ForEach(x =>
            {
                if (x.Saving.IsTransient())
                {
                    x.Saving = null;
                }
            });
            Cache[CachedServiceKeys.AllCashFlows] = cashFlowList;

            return Cache[CachedServiceKeys.AllCashFlows] as IEnumerable<CashFlow>;
        }

        public IEnumerable<CashFlowGroup> GetAllCashFlowGroups()
        {
            if (Cache.ContainsKey(CachedServiceKeys.AllCashFlowGroups) && Cache[CachedServiceKeys.AllCashFlowGroups] != null)
            {
                return Cache[CachedServiceKeys.AllCashFlowGroups] as IEnumerable<CashFlowGroup>;
            }
            Cache[CachedServiceKeys.AllCashFlowGroups] = Database.Query<CashFlowGroup>("ORDER BY Position ASC").ToList();

            return Cache[CachedServiceKeys.AllCashFlowGroups] as IEnumerable<CashFlowGroup>;
        }

        public IEnumerable<Income> GetAllIncomes()
        {
            if (Cache.ContainsKey(CachedServiceKeys.AllIncomes) && Cache[CachedServiceKeys.AllIncomes] != null)
            {
                return Cache[CachedServiceKeys.AllIncomes] as IEnumerable<Income>;
            }
            var incomesList = Database.Query<Income>("ORDER BY Name ASC").ToList();
            incomesList.ForEach(income =>
            {
                var values = Database.Query<IncomeValue>("WHERE IncomeId = @0", income.Id).ToList();
                values.ForEach(value => value.Income = income);
                income.Values.AddRange(values);
            });

            Cache[CachedServiceKeys.AllIncomes] = incomesList;

            return Cache[CachedServiceKeys.AllIncomes] as IEnumerable<Income>;
        }

        public IEnumerable<Saving> GetAllSavings()
        {
            if (Cache.ContainsKey(CachedServiceKeys.AllSavings) && Cache[CachedServiceKeys.AllSavings] != null)
            {
                return Cache[CachedServiceKeys.AllSavings] as IEnumerable<Saving>;
            }
            
            var sql = PetaPoco.Sql.Builder
                        .Select("*")
                        .From("Saving")
                        .InnerJoin("CashFlow")
                        .On("CashFlow.Id = Saving.CashFlowId")
                        .InnerJoin("CashFlowGroup")
                        .On("CashFlow.CashFlowGroupId = CashFlowGroup.Id");

            var savingsList = Database.Query<Saving, CashFlow, CashFlowGroup>(sql).ToList();
            savingsList.ForEach(saving =>
            {
                var savingValues = Database.Query<SavingValue, Budget, Expense>(PetaPoco.Sql.Builder
                    .Select("*")
                    .From("SavingValue")
                    .LeftJoin("Budget")
                    .On("Budget.Id = SavingValue.BudgetId")
                    .LeftJoin("Expense")
                    .On("Expense.Id = SavingValue.ExpenseId")
                    .Where("SavingId=@0", saving.Id)).ToList();
                savingValues.ForEach(savingValue =>
                {
                    if (savingValue.Budget.IsTransient())
                    {
                        savingValue.Budget = null;
                    }
                    if (savingValue.Expense.IsTransient())
                    {
                        savingValue.Expense = null;
                    }
                    savingValue.Saving = saving;
                    saving.Values.Add(savingValue);
                });                               
            });

            Cache[CachedServiceKeys.AllSavings] = savingsList;
            return Cache[CachedServiceKeys.AllSavings]  as IEnumerable<Saving>;
        }

        public IEnumerable<BudgetCalculatorEquation> GetAllEquations()
        {
            if (Cache.ContainsKey(CachedServiceKeys.AllEquations) && Cache[CachedServiceKeys.AllEquations] != null)
            {
                return Cache[CachedServiceKeys.AllEquations] as IEnumerable<BudgetCalculatorEquation>;
            }
            var equations = Database.Query<BudgetCalculatorEquation>("ORDER BY Id").ToList();
            equations.ForEach(eq =>
            {
                var equationItems = Database.Query<BudgetCalculatorItem>("WHERE BudgetCalculatorEquationId = @0 ORDER BY Id", eq.Id).ToList();
                equationItems.ForEach(x => x.Equation = eq);
                eq.Items.AddRange(equationItems);
            });
            
            Cache[CachedServiceKeys.AllEquations] = equations;
            return Cache[CachedServiceKeys.AllEquations] as IEnumerable<BudgetCalculatorEquation>;
        }
    }
}
