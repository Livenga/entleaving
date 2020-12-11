using System;

namespace entleaving {
  /// <summary></summary>
  public sealed class TagInfo {
    /// <summary></summary>
    public ushort AntennaId => this.antennaId;

    /// <summary></summary>
    public double? Angle => this.angle;

    /// <summary></summary>
    public DateTime DetectedAt => this.detectedAt;


    private readonly ushort antennaId;
    private readonly DateTime detectedAt;
    private readonly double? angle;


    /// <summary></summary>
    public TagInfo(
        ushort   antennaId,
        double?  angle,
        DateTime detectedAt) {
      this.antennaId  = antennaId;
      this.angle      = angle;
      this.detectedAt = detectedAt;
    }
  }
}
