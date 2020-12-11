using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using entleaving.Db.Data;


namespace entleaving.Db.DAO {
  using HistoryData = entleaving.Db.Data.History;

  /// <summary></summary>
  public static class History {
    /// <summary></summary>
    public static async Task InsertAsync(
        IDb            db,
        IDbConnection  connection,
        IDbTransaction transaction,
        Guid           readerId,
        int            employeeId,
        HistoryStatus  status) {
      await connection.ExecuteAsync(
          sql:         "insert into histories(reader_id, employee_id, status) values(@readerId, @employeeId, @status);",
          transaction: transaction,
          param:       new { readerId = readerId, employeeId = employeeId, status = status });
    }

    /// <summary></summary>
    public static async Task InsertAsync(
        Guid          readerId,
        int           employeeId,
        HistoryStatus status) {
      await Db.DbFactory.GetInstance().NonQueryAsync(
          (db, conn, trans) => InsertAsync(db, conn, trans, readerId, employeeId, status));
    }


    /// <summary></summary>
    public static async Task InsertByTagIdAsync(
        IDb            db,
        IDbConnection  connection,
        IDbTransaction transaction,
        Guid           readerId,
        string         tagId,
        HistoryStatus  status) {
      await connection.ExecuteAsync(
          sql:         "insert into histories(reader_id, employee_id, status) select @readerId, id, @status from employees where tag_id = @tagId limit 1;",
          transaction: transaction,
          param:       new {
            readerId = readerId,
            tagId    = tagId,
            status   = status
            }
          );
    }


    /// <summary></summary>
    public static async Task InsertByTagIdAsync(
        Guid          readerId,
        string        tagId,
        HistoryStatus status) {
      await Db.DbFactory.GetInstance().NonQueryAsync(
          (db, conn, trans) => InsertByTagIdAsync(db, conn, trans, readerId, tagId, status));
    }
  }
}
