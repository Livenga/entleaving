using System;
using System.Xml.Serialization;


namespace entleaving.Uhf {
  /// <summary></summary>
  public abstract class AbsIdValue<TID, TVALUE>
    where TID : struct
    where TVALUE : struct  {
      public abstract TID Id { set; get; }
      public abstract TVALUE Value { get; }
    }
}
