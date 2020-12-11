namespace entleaving.Uhf {
  public interface IUhfReader {
    UhfReaderType ReaderType  { get; }
    bool          IsConnected { get; }
    bool          IsReading   { get; }
    Settings?     Settings    { set; get; }

    event ConnectionLostEventHandler? ConnectionLost;
    event DetectedTagEventHandler? DetectedTag;

    void Open();
    void Close();

    void Start();
    void Stop();
  }
}
