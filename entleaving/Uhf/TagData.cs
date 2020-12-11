namespace entleaving.Uhf {
  /// <summary></summary>
  public class TagData {
    /// <summary></summary>
    public ushort AntennaId => this.antennaId; 

    /// <summary></summary>
    public string Epc => this.epc;

    /// <summary></summary>
    public double? Rssi => this.rssi;

    /// <summary></summary>
    public double? PhaseAngle => this.phaseAngle;


    private readonly ushort  antennaId;
    private readonly string  epc;
    private readonly double? rssi;
    private readonly double? phaseAngle;


    /// <summary></summary>
    public TagData(
        ushort  antennaId,
        string  epc,
        double? rssi,
        double? phaseAngle) {
      this.antennaId  = antennaId;
      this.epc        = epc;
      this.rssi       = rssi;
      this.phaseAngle = phaseAngle;
    }
  }
}
