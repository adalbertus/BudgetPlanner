using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Models;

namespace Adalbertus.BudgetPlanner.Database
{
    public class CachedService : ICachedService
    {
        public struct Keys
        {
            public const string AllCashFlows = "{45C2830E-8256-4D20-A332-EFAC5BF34D6C}";
            public const string AllCashFlowGroups = "{D5D25298-C18D-4AD5-859C-45EEC9BADC58}";
        }

        public Dictionary<string, object> Cache { get; private set; }
        public IDatabase Database { get; private set; }

        public CachedService(IDatabase database)
        {
            Cache    = new Dictionary<string, object>();
            Database = database;
        }

        public void Clear(string key = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Cache[Keys.AllCashFlows] = null;
                Cache[Keys.AllCashFlowGroups] = null;
            }
            else if(Cache.ContainsKey(key))
            {
                Cache[key] = null;
            }
        }

        public IEnumerable<CashFlow> GetAllCashFlows()
        {
            if (Cache.ContainsKey(Keys.AllCashFlows) && Cache[Keys.AllCashFlows] != null)
            {
                return Cache[Keys.AllCashFlows] as IEnumerable<CashFlow>;
            }
            var cashFlowList = Database.Query<CashFlow, CashFlowGroup, Saving>(PetaPoco.Sql.Builder
                .Select("*")
                .From("CashFlow")
                .InnerJoin("CashFlowGroup")
                .On("CashFlow.CashFlowGroupId = CashFlowGroup.Id")
                .LeftJoin("Saving")
                .On("Saving.CashFlowId = CashFlow.Id")
                .OrderBy("CashFlow.Name ASC")).ToList();
            cashFlowList.ForEach(x =>
            {
                if (x.Saving.IsTransient())
                {
                    x.Saving = null;
                }
            });

            Cache[Keys.AllCashFlows] = cashFlowList;

            return Cache[Keys.AllCashFlows] as IEnumerable<CashFlow>;
        }

        public IEnumerable<CashFlowGroup> GetAllCashFlowGroups()
        {
            if (Cache.ContainsKey(Keys.AllCashFlowGroups) && Cache[Keys.AllCashFlowGroups] != null)
            {
                return Cache[Keys.AllCashFlowGroups] as IEnumerable<CashFlowGroup>;
            }
            Cache[Keys.AllCashFlowGroups] = Database.Query<CashFlowGroup>("ORDER BY Position ASC").ToList();

            return Cache[Keys.AllCashFlowGroups] as IEnumerable<CashFlowGroup>;
        }
    }
}
