using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Database;

namespace Adalbertus.BudgetPlanner.Core
{
    public class Configuration : Dictionary<string, string>, IConfiguration
    {
        public struct Keys
        {
            public const string SavingValueDescriptionPrefix = "SavingValueDescriptionPrefix";
        }

        public IDatabase Database { get; private set; }

        public Configuration(IDatabase database)
        {
            Database = database;
            //CreateDefault();
            //var dictionaryItems = Database.Query<Models.Dictionary>();
            //AddRange(dictionaryItems);
        }

        private void CreateDefault()
        {
            var configurationItems = new List<Models.Dictionary> { new Models.Dictionary { Key = Keys.SavingValueDescriptionPrefix, Value = "Z budżetu" } };
                
        }

        public void AddRange(IEnumerable<Models.Dictionary> items)
        {
            items.ForEach(x => Add(x.Key, x.Value));
        }
    }

}
