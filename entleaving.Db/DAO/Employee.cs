using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;


namespace entleaving.Db.DAO {
  using EmployeeData = entleaving.Db.Data.Employee;

  /// <summary></summary>
  public static class Employee {
    public static async Task<IReadOnlyList<EmployeeData>> FindAllAsync(
        IDb            db,
        IDbConnection  connection,
        IDbTransaction transaction) {
      var result = await connection.QueryAsync<EmployeeData>(
          sql:         "select * from employees;",
          transaction: transaction,
          param:       null);

      return result.ToList();
    }


    /// <summary></summary>
    public static Task<IReadOnlyList<EmployeeData>> FindAllAsync() {
      return Db.DbFactory.GetInstance().QueryAsync(
          (db, conn, trans) => FindAllAsync(db, conn, trans));
    }


    /// <summary></summary>
    public static async Task<EmployeeData?> FindByTagId(
        IDb            db,
        IDbConnection  connection,
        IDbTransaction transaction,
        string         tagId) {
      return await connection.QuerySingleAsync<EmployeeData?>(
          sql: "select * from employees where tag_id = @tagId;",
          transaction: transaction,
          param: new { tagId = tagId });
    }

    /// <summary></summary>
    public static async Task<EmployeeData?> FindByTagId(string tagId) {
      return await Db.DbFactory.GetInstance().QueryAsync(
          (db, conn, trans) => FindByTagId(db, conn, trans, tagId));
    }


    /// <summary></summary>
    public static async Task InsertAsync(
        IDb            db,
        IDbConnection  connection,
        IDbTransaction transaction,
        int            id,
        string?        tagId) {
      await connection.ExecuteAsync(
          sql:         "insert into employees(id, tag_id) values(@id, @tagId);",
          param:       new { id = id, tagId = tagId },
          transaction: transaction);
    }


    /// <summary></summary>
    public static async Task InsertAsync(
        int     id,
        string? tagId) {
      await Db.DbFactory.GetInstance().NonQueryAsync(
          (db, conn, trans) => InsertAsync(db, conn, trans, id, tagId));
    }


    /// <summary></summary>
    public static async Task UpdateAsync(
        IDb db,
        IDbConnection connection,
        IDbTransaction transaction,
        int id,
        string? tagId) {
      await connection.ExecuteAsync(
          sql:         "update employees set tag_id = @tagId where id = @id;",
          param:       new { id = id, tagId = tagId },
          transaction: transaction);
    }


    /// <summary></summary>
    public static async Task UpdateAsync(
        int id,
        string? tagId) {
      await Db.DbFactory.GetInstance().NonQueryAsync(
          (db, conn, trans) => UpdateAsync(db, conn, trans, id, tagId));
    }
  }
}
