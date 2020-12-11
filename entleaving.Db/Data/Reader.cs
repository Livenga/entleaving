using System;


namespace entleaving.Db.Data {
  /// <summary></summary>
  public class Reader {
    /// <summary>ID</summary>
    public Guid Id { set; get; } = Guid.Empty;

    /// <summary>ホスト名(NOTE: この値を使用してリーダの設定は行わない.)</summary>
    public string Hostname { set; get; } = string.Empty;

    /// <summary>設置場所名</summary>
    public string? LocationName { set; get; } = null;

    /// <summary>備考</summary>
    public string? Remarks { set; get; } = null;

    /// <summary>登録日時</summary>
    public DateTime CreatedAt { set; get; } = DateTime.Now;
  }
}
