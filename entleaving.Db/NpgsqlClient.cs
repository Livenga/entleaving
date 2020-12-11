using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;


namespace entleaving.Db {
  /// <summary></summary>
  public class NpgsqlClient : IDb {
    public Settings? Settings { set; get; } = null;


    //private readonly NpgsqlClient client = new NpgsqlClient();

    /// <summary></summary>
    public IDbConnection CreateConnection() {
      if(this.Settings == null) {
        throw new NullReferenceException();
      }

      var conn = new NpgsqlConnection();

      var sb = new NpgsqlConnectionStringBuilder();

      sb.Timeout  = 5;
      sb.Host     = this.Settings.Hostname;
      sb.Port     = this.Settings.Port;
      sb.Database = this.Settings.Database;
      sb.Username = this.Settings.Username;
      sb.Password = this.Settings.Password;

#if DEBUG
      Console.Error.WriteLine($"Debug {sb.ToString()}");
#endif
      conn.ConnectionString = sb.ToString();

      return conn;
    }


    /// <summary></summary>
    public T Query<T>(
        Func<IDb, IDbConnection, IDbTransaction, T> func,
        IsolationLevel il = IsolationLevel.ReadUncommitted) {
      using(var conn = this.CreateConnection()) {
        conn.Open();

        using(var trans = conn.BeginTransaction(il)) {
          try {
            return func(this, conn, trans);
          } catch {
            trans.Rollback();
            throw;
          }
        }
      }
    }


    /// <summary></summary>
    public async Task<T> QueryAsync<T>(
        Func<IDb, IDbConnection, IDbTransaction, Task<T>> func,
        IsolationLevel il = IsolationLevel.ReadUncommitted) {
      using(var conn = this.CreateConnection()) {
        await ((NpgsqlConnection)conn).OpenAsync();

        using(var trans = conn.BeginTransaction(il)) {
          try {
            return await func(this, conn, trans);
          } catch {
            trans.Rollback();
            throw;
          }
        }
      }
    }


    /// <summary></summary>
    public void NonQuery(
        Action<IDb, IDbConnection, IDbTransaction> action,
        IsolationLevel il = IsolationLevel.ReadUncommitted) {
      using(var conn = this.CreateConnection()) {
        conn.Open();

        using(var trans = conn.BeginTransaction(il)) {
          try {
            action(this, conn, trans);

            trans.Commit();
          } catch {
            trans.Rollback();
            throw;
          }
        }
      }
    }

    /// <summary></summary>
    public async Task NonQueryAsync(
        Func<IDb, IDbConnection, IDbTransaction, Task> action,
        IsolationLevel il = IsolationLevel.ReadUncommitted) {
      using(var conn = this.CreateConnection()) {
        conn.Open();

        using(var trans = conn.BeginTransaction(il)) {
          try {
            await action(this, conn, trans);

            trans.Commit();
          } catch {
            trans.Rollback();
            throw;
          }
        }
      }
    }
  }
}
