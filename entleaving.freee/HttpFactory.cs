using System.Net;
using System.Net.Http;


namespace entleaving.freee {
  /// <summary></summary>
  public static class HttpFactory {
    /// <summary></summary>
    private static HttpClient? instance = null;

    /// <summary></summary>
    public static HttpClient GetInstance() {
      if(instance == null) {
        instance = new HttpClient();
      }

      return instance;
    }
  }
}
