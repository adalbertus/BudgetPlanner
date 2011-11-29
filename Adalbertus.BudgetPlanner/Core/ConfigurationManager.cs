using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adalbertus.BudgetPlanner.Database;
using Adalbertus.BudgetPlanner.Models;

namespace Adalbertus.BudgetPlanner.Core
{
    public struct ConfigurationKeys
    {
        public const string DatabaseVersion = "DatabaseVersion";
    }

    public class ConfigurationManager : Dictionary<string, string>, IConfigurationManager
    {
        public IDatabase Database { get; private set; }

        public ConfigurationManager(IDatabase database)
        {
            Database = database;            
        }

        public T GetGetValueOrDefault<T>(string key, T defaultValue = default(T))
        {
            if (!ContainsKey(key))
            {
                LoadAllConfigurationFromDatabase();
            }

            return (T)Convert.ChangeType(this[key], typeof(T));
        }

        private void LoadAllConfigurationFromDatabase()
        {
            var allConfigurationItems = Database.Query<Configuration>().ToList();
            allConfigurationItems.ForEach(x => Add(x.Key, x.Value));
        }
    }

}
