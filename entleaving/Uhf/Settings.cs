using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;


namespace entleaving.Uhf {
  /// <summary></summary>
  [XmlRoot("Settings")]
  public sealed class Settings {
    /// <summary></summary>
    [XmlAttribute("data-id")]
    public Guid Id { set; get; } = Guid.Empty;

    /// <summary></summary>
    [XmlElement("Hostname")]
    public string Hostname { set; get; } = "192.168.100.100";

    /// <summary></summary>
    [XmlElement("Port")]
    public int Port { set; get; } = 5084;

    /// <summary></summary>
    [XmlElement("UseTLS")]
    public bool UseTLS { set; get; } = false;

    /// <summary></summary>
    [XmlElement("AntennaInfos")]
    public List<AntennaInfo> Antennas { set; get; } = Array.Empty<AntennaInfo>().ToList();

    /// <summary></summary>
    [XmlArray("InsideAntennas")]
    public int[] InsideAntennas { set; get; } = new int[] { 1, 2 };

    /// <summary></summary>
    [XmlArray("OutsideAntennas")]
    public int [] OutsideAntennas { set; get; } = new int[] { 3, 4 };


    /// <summary></summary>
    private static Settings Load(Stream stream) {
      var s = new XmlSerializer(typeof(Settings));
      Settings? settings = s.Deserialize(stream) as Settings;

      if(settings == null) {
        return new Settings();
      }

      return settings;
    }

    /// <summary></summary>
    private static Settings Load(string path) {
      using(var strm = File.Open(path, FileMode.Open, FileAccess.Read)) {
        return Load(strm);
      }
    }


    /// <summary></summary>
    public static Task<Settings> LoadAsync(string path) {
      return Task.Run(() => Load(path));
    }

    /// <summary></summary>
    public static Task<Settings> LoadAsync(Stream stream) {
      return Task.Run(() => Load(stream));
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
