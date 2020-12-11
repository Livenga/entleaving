using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;


namespace entleaving.Db {
  /// <summary></summary>
  [XmlRoot("Settings", Namespace = "Db")]
  public class Settings {
    /// <summary></summary>
    [XmlElement("Hostname")]
    public string Hostname { set; get; } = "localhost";

    /// <summary></summary>
    [XmlElement("Port")]
    public int    Port     { set; get; } = 5432;

    /// <summary></summary>
    [XmlElement("Database")]
    public string Database { set; get; } = "entleaving";

    /// <summary></summary>
    [XmlElement("Username")]
    public string Username { set; get; } = "postgres";

    /// <summary></summary>
    [XmlElement("Password")]
    public string Password { set; get; } = "admin";


    /// <summary></summary>
    private static Settings Load(Stream stream) {
      var s = new XmlSerializer(typeof(Settings));
      var settings = s.Deserialize(stream) as Settings;

      if(settings == null) {
        throw new NullReferenceException();
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
    public static Task<Settings> LoadAsync(Stream stream) {
      return Task.Run(() => Load(stream));
    }

    /// <summary></summary>
    public static Task<Settings> LoadAsync(string path) {
      return Task.Run(() => Load(path));
    }


    /// <summary></summary>
    public async Task SaveAsync(Stream stream) {
      var s = new XmlSerializer(typeof(Settings));

      s.Serialize(stream, this);
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
