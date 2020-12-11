namespace entleaving.Uhf {
  public delegate void ConnectionLostEventHandler(IUhfReader reader);
  public delegate void DetectedTagEventHandler(IUhfReader reader, TagData tag);
}
