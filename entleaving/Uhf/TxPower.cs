using System;
using System.Xml.Serialization;


namespace entleaving.Uhf {
  /// <summary></summary>
  [XmlRoot("Tx")]
  public sealed class Tx {
    /// <summary></summary>
    public static readonly ushort Min = 1;

    /// <summary></summary>
    public static readonly ushort Max = 81;


    /// <summary></summary>
    [XmlAttribute("data-id")]
    public ushort Id { set; get; } = Max;

    /// <summary></summary>
    [XmlIgnore]
    public double Value => 10f + (0.25f * (this.Id - 1));
  }
}
