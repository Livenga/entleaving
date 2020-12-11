using System;


namespace entleaving.freee {
  /// <summary></summary>
  public static class FreeeAPIFactory {
    private static FreeeAPI? instance = null;

    /// <summary></summary>
    public static FreeeAPI GetInstance(string? accessToken = null) {
      if(instance == null) {
        instance = new FreeeAPI();
      }
      if(accessToken != null) {
        instance.AccessToken = accessToken;
      }

      if(instance.AccessToken.Length == 0) {
        throw new Exception($"アクセストークンが指定されていません.");
      }

      return instance;
    }
  }
}
