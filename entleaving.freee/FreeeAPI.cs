using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace entleaving.freee {
  /// <summary></summary>
  public class FreeeAPI {
    /// <summary></summary>
    public string ClientId     { set; get; } = string.Empty;

    /// <summary></summary>
    public string ClientSecret { set; get; } = string.Empty;

    /// <summary></summary>
    public string AccessToken  { set; get; } = string.Empty;

    /// <summary></summary>
    public string RefreshToken { set; get; } = string.Empty;


    /// <summary></summary>
    public ResponseToken Token {
      set {
        this.AccessToken  = value.AccessToken;
        this.RefreshToken = value.RefreshToken;
      }
    }


    /// <summary></summary>
    public HttpRequestMessage GetRequest(string url) {
      if(this.AccessToken.Length == 0) {
        throw new Exception("アクセストークンが指定されていません.");
      }

      var req = new HttpRequestMessage(HttpMethod.Get, url);

      req.Headers.Add("accept", "application/json");
      req.Headers.Add("Authorization", $"Bearer {this.AccessToken}");
      //req.Headers.Add("Content-Type", "application/json");

      return req;
    }


    /// <summary></summary>
    public HttpRequestMessage PostRequest(
        string      url,
        HttpContent content,
        bool        useBearer   = true) {
      if(this.AccessToken.Length == 0) {
        throw new Exception("アクセストークンが指定されていません.");
      }

      var req = new HttpRequestMessage(HttpMethod.Post, url);

      if(useBearer) {
        req.Headers.Add("Authorization", $"Bearer {this.AccessToken}");
      }
      req.Content = content;

      return req;
    }


    /// <summary></summary>
    public async Task<string> GetContentAsync(HttpResponseMessage msg) {
      using(var ctn = msg.Content) {
        using(var mem = new MemoryStream()) {
          using(var stream = ctn.ReadAsStream()) {
            byte[] buffer = new byte[1024];
            int readSize = 0;

            while((readSize = await stream.ReadAsync(buffer, 0, 1024)) > 0) {
              await mem.WriteAsync(buffer, 0, readSize);
            }
          }

          await mem.FlushAsync();
          return Encoding.UTF8.GetString(mem.ToArray());
        }
      }
    }


    /// <summary></summary>
    public async Task<string> HrGetEmployeesAsync(
        int    per = 25,
        int    page = 0) {
      // NOTE: company_id を指定
      //string url = $"https://api.freee.co.jp/hr/api/v1/companies/{companyId}/employees?page={page}&per={per}";
      string url = $"https://api.freee.co.jp/hr/api/v1/companies/0/employees?page={page}&per={per}";
      //string url = $"https://api.freee.co.jp/hr/api/v1/employees?page={page}&per={per}";

      using(var req = this.GetRequest(url)) {
        var resp = await HttpFactory.GetInstance().SendAsync(req);

        return await this.GetContentAsync(resp);
      }
    }


    /// <summary></summary>
    public async Task<string> HrUsersMe() {
      string url = "https://api.freee.co.jp/hr/api/v1/users/me";

      using(var req = this.GetRequest(url)) {
        var resp = await HttpFactory.GetInstance().SendAsync(req);

        using(var ctn = resp.Content) {
          using(var mem = new MemoryStream()) {
            using(var stream = ctn.ReadAsStream()) {
              int readSize = 0;
              byte[] buffer = new byte[1024];

              while((readSize = await stream.ReadAsync(buffer, 0, 1024)) > 0) {
                await mem.WriteAsync(buffer, 0, readSize);
              }
            }

            await mem.FlushAsync();

            return Encoding.UTF8.GetString(mem.ToArray());
          }
        }
      }
    }


#pragma warning disable CS8620
    /// <summary></summary>
    public async Task<ResponseToken> RefreshAccessTokenAsync(string? path = null) {
      if(this.RefreshToken.Length == 0) {
        throw new Exception($"リフレッシュトークンを指定してください.");
      }

      var param = new Dictionary<string, string>();
      param.Add("grant_type",    "refresh_token");
      param.Add("client_id",     this.ClientId);
      param.Add("client_secret", this.ClientSecret);
      param.Add("refresh_token", this.RefreshToken);
      param.Add("redirect_uri", "urn:ietf:wg:oauth:2.0:oob");

      ResponseToken token;
      string strToken = string.Empty;

      using(var content = new FormUrlEncodedContent(param))
        using(var req = this.PostRequest(
              url:       "https://accounts.secure.freee.co.jp/public_api/token",
              content:   content,
              useBearer: false))
        using(var resp = await HttpFactory.GetInstance().SendAsync(req)) {
          strToken = await this.GetContentAsync(resp);
          token = JsonConvert.DeserializeObject<ResponseToken>(strToken);

          this.AccessToken  = token.AccessToken;
          this.RefreshToken = token.RefreshToken;
        }


      if(path != null && strToken.Length > 0) {
        try {
          await token.SaveAsync(path);
        } catch { }
      }

      return token;
    }
  }
}
