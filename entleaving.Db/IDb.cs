using System;
using System.Data;
//using System.Data.Common;
using System.Threading.Tasks;


namespace entleaving.Db {
  /// <summary></summary>
  public interface IDb {
    Settings? Settings { set; get; }

    IDbConnection CreateConnection();

    T Query<T>(
        Func<IDb, IDbConnection, IDbTransaction, T> func,
        IsolationLevel il = IsolationLevel.ReadUncommitted);

    Task<T> QueryAsync<T>(
        Func<IDb, IDbConnection, IDbTransaction, Task<T>> func,
        IsolationLevel il = IsolationLevel.ReadUncommitted);

    void NonQuery(
        Action<IDb, IDbConnection, IDbTransaction> action,
        IsolationLevel il = IsolationLevel.ReadUncommitted);

    Task NonQueryAsync(
        Func<IDb, IDbConnection, IDbTransaction, Task> action,
        IsolationLevel il = IsolationLevel.ReadUncommitted);
  }
}
