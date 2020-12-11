using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;


namespace entleaving.Db.DAO {
  using ReaderData = entleaving.Db.Data.Reader;

  /// <summary></summary>
  public static class Reader {
    /// <summary></summary>
    public static async Task InsertAsync(
        IDb db,
        IDbConnection connection,
        IDbTransaction transaction,
        Guid id,
        string hostname,
        string? locationName,
        string? remarks) {
      await connection.ExecuteAsync(
          sql:         "insert into readers(id, hostname, location_name, remarks) values(@id, @hostname, @locationName, @remarks);",
          transaction: transaction,
          param:       new { id = id, hostname = hostname, locationName = locationName, remarks = remarks });
    }

    /// <summary></summary>
    public static async Task InsertAsync(
        Guid    id,
        string  hostname,
        string? locationName,
        string? remarks) {
      await Db.DbFactory.GetInstance().NonQueryAsync(
          (db, conn, trans) => InsertAsync(db, conn, trans, id, hostname, locationName, remarks));
    }

    /// <summary></summary>
    public static async Task UpdateAsync(
        IDb            db,
        IDbConnection  connection,
        IDbTransaction transaction,
        Guid           id,
        string         hostname,
        string?        locationName,
        string?        remarks) {
      await connection.ExecuteAsync(
          sql: "update readers set hostname = @hostname, location_name = @locationName, remarks = @remarks where id = @id;",
          transaction: transaction,
          param: new { id = id, hostname = hostname, locationName = locationName, remarks = remarks });
    }


    /// <summary></summary>
    public static async Task<ReaderData?> FindById(
        IDb            db,
        IDbConnection  connection,
        IDbTransaction transaction,
        Guid           id) {
      try {
      return await connection.QuerySingleAsync<ReaderData?>(
          sql:         "select * from readers where id = @id;",
          transaction: transaction,
          param:       new { id = id });
      } catch(InvalidOperationException) {
        return null;
      } catch {
        throw;
      }
    }


    /// <summary></summary>
    public static async Task UpsertAsync(
        IDb            db,
        IDbConnection  connection,
        IDbTransaction transaction,
        Guid           id,
        string         hostname,
        string?        locationName,
        string?        remarks) {
      var reader = await FindById(db, connection, transaction, id);

      if(reader == null) {
        await InsertAsync(db, connection, transaction, id, hostname, locationName, remarks);
      } else {
        await UpdateAsync(db, connection, transaction, id, hostname, locationName, remarks);
      }
    }


    /// <summary></summary>
    public static async Task UpsertAsync(
        Guid    id,
        string  hostname,
        string? locationName,
        string? remarks) {
      await Db.DbFactory.GetInstance().NonQueryAsync(
          (db, connection, transaction) => UpsertAsync(
            db, connection, transaction,
            id, hostname, locationName, remarks));
    }
  }
}
