using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;


namespace entleaving.freee {
  /// <summary></summary>
  [XmlRoot("ClientInfo")]
  public class FreeeAPIClientInfo {
    private static readonly XmlSerializer _s = new XmlSerializer(typeof(FreeeAPIClientInfo));


    /// <summary></summary>
    [XmlElement("Id")]
    public string Id { set; get; } = string.Empty;

    /// <summary></summary>
    [XmlElement("Secret")]
    public string Secret { set; get; } = string.Empty;

    /// <summary></summary>
    [XmlElement("RedirectUri")]
    public string RedirectUri { set; get; } = string.Empty;


    /// <summary></summary>
    private static FreeeAPIClientInfo Load(Stream stream) {
      var o = _s.Deserialize(stream) as FreeeAPIClientInfo;

      if(o == null) {
        throw new Exception($"変換に失敗.");
      }

      return o;
    }

    /// <summary></summary>
    private static FreeeAPIClientInfo Load(string path) {
      using(var stream = File.Open(path, FileMode.Open, FileAccess.Read)) {
        return Load(stream);
      }
    }

    /// <summary></summary>
    public static async Task<FreeeAPIClientInfo> LoadAsync(Stream stream) {
      return await Task.Run(() => Load(stream));
    }

    /// <summary></summary>
    public static async Task<FreeeAPIClientInfo> LoadAsync(string path) {
      return await Task.Run(() => Load(path));
    }

    /// <summary></summary>
    public async Task SaveAsync(Stream stream) {
      _s.Serialize(stream, this);
      await stream.FlushAsync();
    }

    /// <summary></summary>
    public async Task SaveAsync(string path) {
      var mode = File.Exists(path) ? FileMode.Truncate : FileMode.CreateNew;

      using(var stream = File.Open(path, mode, FileAccess.Write)) {
        stream.Position = 0;
        await SaveAsync(stream);
      }
    }
  }
}
