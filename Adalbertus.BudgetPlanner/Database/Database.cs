using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adalbertus.BudgetPlanner.Database
{
    public class Database : PetaPoco.Database, IDatabase
    {
        public Database()
            : base("default")           
        {
        }

        #region IDatabase Members


        public void SaveAll<TModel>(IEnumerable<TModel> items) where TModel : Models.Entity
        {
            foreach (object item in items)
            {
                Save(item);
            }
        }

        public IEnumerable<T> Query<T>()
        {
            return Query<T>(string.Empty);
        }

        public int Count<T>()
        {
            var pd = PocoData.ForType(typeof(T));
            return Count(pd.TableInfo.TableName);
        }

        public int Count(string tableName)
        {
            return ExecuteScalar<int>(PetaPoco.Sql.Builder
                .Select("COUNT(*)")
                .From(tableName));
        }

        public T FirstOrDefault<T>()
        {
            return FirstOrDefault<T>(string.Empty);
        }

        #endregion
    }
}
