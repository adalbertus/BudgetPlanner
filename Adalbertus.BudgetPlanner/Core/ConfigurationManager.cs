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
        public const string IsFirstRun = "IsFirstRun";
        public const string AuthorEmail = "AuthorEmail";
        public const string HomePage = "HomePage";
        public const string HelpPage = "HelpPage";
        public const string UpdatePage = "UpdatePage";
        public const string UpdateMinutesInterval = "UpdateMinutesInterval";
    }

    public class ConfigurationManager : Dictionary<string, Configuration>, IConfigurationManager
    {
        public IDatabase Database { get; private set; }

        public ConfigurationManager(IDatabase database)
        {
            Database = database;
        }

        public T GetValueOrDefault<T>(string key, T defaultValue = default(T))
        {
            if (!ContainsKey(key))
            {
                LoadAllConfigurationFromDatabase();
            }

            if (!ContainsKey(key))
            {
                SaveValue(key, defaultValue);
                return defaultValue;
            }

            return (T)Convert.ChangeType(this[key].Value, typeof(T));
        }

        public void SaveValue(string key, object value)
        {
            using (var tx = Database.GetTransaction())
            {
                if (ContainsKey(key))
                {
                    this[key].Value = value.ToString();
                    Database.Save(this[key]);
                }
                else
                {
                    Add(key, new Configuration { Key = key, Value = value.ToString(), IsActive = true });
                    //var sql = PetaPoco.Sql.Builder
                    //    .Append("
                    Database.Execute("INSERT INTO Configuration (Key, IsActive, Value, Decription) VALUES (@0, @1, @2, @3)",
                        key, this[key].IsActive, this[key].Value, this[key].Decription);
                }
                tx.Complete();
            }
        }

        private void LoadAllConfigurationFromDatabase()
        {
            var allConfigurationItems = Database.Query<Configuration>().ToList();
            Clear();
            allConfigurationItems.ForEach(x => Add(x.Key, x));
        }
    }

}
