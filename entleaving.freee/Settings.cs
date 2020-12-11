using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;


namespace entleaving.freee {
  /// <summary></summary>
  [XmlRoot("Settings", Namespace = "freee")]
  public class Settings {
    private static readonly XmlSerializer serializer = new XmlSerializer(typeof(Settings));


    /// <summary></summary>
    [XmlElement("ClientId")]
    public string ClientId { set; get; } = string.Empty;

    /// <summary></summary>
    [XmlElement("ClientSecret")]
    public string ClientSecret { set; get; } = string.Empty;

    /// <summary></summary>
    [XmlElement("AccessToken")]
    public string AccessToken { set; get; } = string.Empty;

    /// <summary></summary>
    [XmlElement("RefreshToken")]
    public string RefreshToken { set; get; } = string.Empty;

    /// <summary></summary>
    [XmlAttribute("data-created-at")]
    public DateTime CreatedAt { set; get; } = DateTime.Now;


    /// <summary></summary>
    private static Settings Load(Stream stream) {
      var settings = serializer.Deserialize(stream) as Settings;

      if(settings == null) {
        throw new Exception();
      }

      return settings;
    }

    /// <summary></summary>
    private static Settings Load(string path) {
      using(var stream = File.Open(path, FileMode.Open, FileAccess.Read)) {
        return Load(stream);
      }
    }


    /// <summary></summary>
    public static Task<Settings> LodaAsync(Stream stream) {
      return Task.Run(() => Load(stream));
    }

    /// <summary></summary>
    public static Task<Settings> LoadAsync(string path) {
      return Task.Run(() => Load(path));
    }


    /// <summary></summary>
    public async Task SaveAsync(Stream stream) {
      serializer.Serialize(stream, this);
      await stream.FlushAsync();
    }

    /// <summary></summary>
    public async Task SaveAsync(string path) {
      var mode = File.Exists(path) ? FileMode.Truncate : FileMode.Create;

      using(var stream = File.Open(path, mode, FileAccess.Write)) {
        await SaveAsync(stream);
      }
    }
  }
}
