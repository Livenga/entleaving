using System;


namespace entleaving.Db.Data {
  /// <summary></summary>
  public class History {
    /// <summary></summary>
    public long Id { set; get; } = 0;

    /// <summary></summary>
    public Guid ReaderId { set; get; } = Guid.Empty;

    /// <summary></summary>
    public int EmployeeId { set; get; } = 0;

    /// <summary></summary>
    public HistoryStatus Status { set; get; } = HistoryStatus.Entry;

    /// <summary></summary>
    public DateTime CreatedAt { set; get; } = DateTime.Now;
  }
}
