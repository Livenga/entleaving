using System;
using System.Xml.Serialization;


namespace entleaving.Uhf {
  /// <summary></summary>
  [XmlRoot("Rx")]
  public sealed class Rx {
    /// <summary></summary>
    public static readonly ushort Min = 1;

    /// <summary></summary>
    public static readonly ushort Max = 42;


    /// <summary></summary>
    [XmlAttribute("data-id")]
    public ushort Id { set; get; } = Min;

    /// <summary></summary>
    [XmlIgnore]
    public double Value => (this.Id == Min)
      ? -80f
      : -70f + (this.Id - 2);
  }
}
