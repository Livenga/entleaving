using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace entleaving.freee {
  /// <summary></summary>
  public class ResponseToken {
    [JsonProperty("access_token")]
    public string AccessToken { set; get; } = string.Empty;

    [JsonProperty("token_type")]
    public string TokenType { set; get; } = string.Empty;

    [JsonProperty("expires_in")]
    public int ExpiresIn { set; get; } = 0;

    [JsonProperty("refresh_token")]
    public string RefreshToken { set; get; } = string.Empty;

    [JsonProperty("scope")]
    public string Scope { set; get; } = string.Empty;

    [JsonProperty("created_at")]
    public int CreatedAt { set; get; } = 0;


    /// <summary></summary>
    public static async Task<ResponseToken> LoadAsync(Stream stream) {
      stream.Position = 0;
      using(StreamReader reader = new StreamReader(stream)) {
        var strToken = await reader.ReadToEndAsync();
        return JsonConvert.DeserializeObject<ResponseToken>(value: strToken);
      }
    }

    /// <summary></summary>
    public static async Task<ResponseToken> LoadAsync(string path) {
      using(var stream = File.Open(path, FileMode.Open, FileAccess.Read)) {
        return await LoadAsync(stream);
      }
    }


    /// <summary></summary>
    public async Task SaveAsync(Stream stream) {
      using(var writer = new StreamWriter(
            stream:   stream,
            encoding: Encoding.UTF8)) {
        await writer.WriteAsync(JsonConvert.SerializeObject(this));
      }
    }

    /// <summary></summary>
    public async Task SaveAsync(string path) {
      var mode = File.Exists(path) ? FileMode.Truncate : FileMode.CreateNew;

      using(var stream = File.Open(path, mode, FileAccess.Write)) {
        await stream.FlushAsync();
      }
    }
  }
}
