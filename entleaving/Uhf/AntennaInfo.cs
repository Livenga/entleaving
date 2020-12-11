using System;
using System.Xml.Serialization;


namespace entleaving.Uhf {
  /// <summary></summary>
  [XmlRoot("AntennaInfo")]
  public sealed class AntennaInfo {
    /// <summary></summary>
    [XmlAttribute("data-id")]
    public ushort Id { set; get; } = 1;

    /// <summary></summary>
    [XmlAttribute("data-is-enabled")]
    public bool IsEnabled { set; get; } = true;

    /// <summary></summary>
    [XmlElement("Tx")]
    public Tx Tx { set; get; } = new Tx();

    /// <summary></summary>
    [XmlElement("Rx")]
    public Rx Rx { set; get; } = new Rx();
  }
}
