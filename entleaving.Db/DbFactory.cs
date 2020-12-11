using System;
using Dapper;


namespace entleaving.Db {
  /// <summary></summary>
  public static class DbFactory {
    private static IDb? instance = null;

    /// <summary></summary>
    public static IDb GetInstance(Settings? settings = null) {
      Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

      if(instance == null) {
        if(settings == null) {
          throw new ArgumentNullException(
              paramName: nameof(settings),
              message: "データベースの設定が無効です.");
        }

        instance = new NpgsqlClient();
      }

      if(settings != null) {
        instance.Settings = settings;
      }

      return instance;
    }
  }
}
