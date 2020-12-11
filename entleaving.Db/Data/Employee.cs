using System;


namespace entleaving.Db.Data {
  /// <summary></summary>
  public class Employee {
    /// <summary></summary>
    public int Id             { set; get; } = 0;

    /// <summary></summary>
    public string? TagId      { set; get; } = null;

    /// <summary></summary>
    public DateTime CreatedAt { set; get; } = DateTime.Now;
  }
}
