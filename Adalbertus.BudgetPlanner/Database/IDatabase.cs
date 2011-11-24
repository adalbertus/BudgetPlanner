using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;
using Adalbertus.BudgetPlanner.Models;

namespace Adalbertus.BudgetPlanner.Database
{
    public interface IDatabase : IDisposable
    {
        string LastSQL { get; }
        object[] LastArgs { get ; }
        string LastCommand { get; }

        int Count<T>();
        int Count(string tableName);

        IEnumerable<T> Query<T>();
        IEnumerable<T> Query<T>(Sql sql);
        IEnumerable<T> Query<T>(string sql, params object[] args);
        IEnumerable<TRet> Query<T1, T2, TRet>(Func<T1, T2, TRet> cb, Sql sql);
        IEnumerable<TRet> Query<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, Sql sql);
        IEnumerable<TRet> Query<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, Sql sql);
        IEnumerable<T1> Query<T1, T2>(Sql sql);
        IEnumerable<T1> Query<T1, T2, T3>(Sql sql);
        IEnumerable<T1> Query<T1, T2, T3, T4>(Sql sql);


        Transaction GetTransaction();
        
        void Save(object poco);
        int Update(object poco);
        int Update<T>(string sql, params object[] args);
        int Update<T>(Sql sql);

        int Delete(object poco);
        int Delete<T>(object pocoOrPrimaryKey);
        int Delete<T>(Sql sql);
        int Delete<T>(string sql, params object[] args);

        T FirstOrDefault<T>();
        T FirstOrDefault<T>(Sql sql);
        T FirstOrDefault<T>(string sql, params object[] args);
        T SingleOrDefault<T>(Sql sql);
        T SingleOrDefault<T>(string sql, params object[] args);

        int Execute(Sql sql);
        int Execute(string sql, params object[] args);
        T ExecuteScalar<T>(string sql, params object[] args);
        T ExecuteScalar<T>(Sql sql);

        bool IsNew(object poco);

        void SaveAll<TModel>(IEnumerable<TModel> items) where TModel : Entity;
    }
}
