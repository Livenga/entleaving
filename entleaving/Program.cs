using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using entleaving.Uhf;
using entleaving.Db.Data;


namespace entleaving {
  using ServerTimer = System.Timers.Timer;
  using DbSettings = entleaving.Db.Settings;


  /// <summary></summary>
  public class Program {
    private static readonly object oLock = new object();
    private static readonly ServerTimer intervalTimer = new ServerTimer(1000f);

    private static Dictionary<string, List<TagInfo>> tagBuffer = new Dictionary<string, List<TagInfo>>();

    private static EventWaitHandle? eventHandler = null;
    private static Settings? readerSettings = null;

    private static Guid readerId = Guid.Empty;
    private static int[] insideNumbers = new int[] { 1, 2 };
    private static int[] outsideNumbers = new int[] { 3, 4 };



    /// <summary></summary>
    public static async Task Main(string[] args) {
      // NOTE: このプログラム中では社員データ(もしくはユーザデータ)の同期を行わせないため,
      // 外部より任意の時点でそれらデータの同期を行わせる必要がある.

      // 蓄積したデータを対象として追加及び削除などの更新処理を行わせない.

      // データベース設定
      Db.DbFactory.GetInstance(
          await DbSettings.LoadAsync("db.conf"));

      try {
        readerSettings = await Settings.LoadAsync("settings.conf");
      } catch(Exception) {
        // 設定ファイルが存在しない場合や xml パースに失敗.
        readerSettings = new Settings() {
          Id = Guid.NewGuid(),
          Hostname = "192.168.100.64"
        };
      } finally {
        if(readerSettings != null) {
          readerId = readerSettings.Id;
          insideNumbers  = readerSettings.InsideAntennas;
          outsideNumbers = readerSettings.OutsideAntennas;
        }
      }

      try {
        // データベースへ該当リーダのデータを追加
        await entleaving.Db.DAO.Reader.UpsertAsync(
            readerSettings.Id,
            readerSettings.Hostname,
            null,
            null);
      } catch {
        throw;
      }

      // FreeeAPI アクセストーク取得
      if(! File.Exists("access_token.json")) {
        throw new FileNotFoundException(
            message:  null,
            fileName: "access_token.json");
      }


      var freeeToken = await entleaving.freee.ResponseToken.LoadAsync("access_token.json");
      var freeeAPI = entleaving.freee.FreeeAPIFactory.GetInstance(freeeToken.AccessToken);
      freeeAPI.Token = freeeToken;
      try {
        var clientInfo = await entleaving.freee.FreeeAPIClientInfo.LoadAsync("freee_client.conf");

        freeeAPI.ClientId = clientInfo.Id;
        freeeAPI.ClientSecret = clientInfo.Secret;
      } catch {
        Console.Error.WriteLine($"Warning # Freeee API のクライアント情報の取得に失敗. アクセストークンの更新が行えない可能性があります.");
      }


      // UHF 帯 RFID リーダ動作開始
      IUhfReader reader = new R420Reader();
      reader.ConnectionLost += OnUhfReaderConnectionLost;
      reader.DetectedTag    += OnUhfReaderDetectedTag;

      reader.Settings = readerSettings;
      reader.Open();
      reader.Start();

      Console.CancelKeyPress += OnCancelKeyPress;
      intervalTimer.Elapsed += OnIntervalTimerElapsed;
      intervalTimer.Start();

      eventHandler = new EventWaitHandle(
          initialState: false,
          mode:         EventResetMode.ManualReset);
      eventHandler.WaitOne();

      reader.DetectedTag -= OnUhfReaderDetectedTag;
      intervalTimer.Elapsed -= OnIntervalTimerElapsed;

      intervalTimer.Stop();
#if DEBUG
      Console.Error.WriteLine($"Debug Exit.");
#endif

      reader.Stop();
      reader.Close();
    }


    /// <summary></summary>
    private static void OnUhfReaderDetectedTag(IUhfReader reader, TagData tag) {
      var tagInfo = new TagInfo(
          antennaId:  tag.AntennaId,
          angle:      tag.PhaseAngle,
          detectedAt: DateTime.Now);

      //Console.Error.WriteLine($"Debug {tag.AntennaId} {tag.Epc}");

      lock(oLock) {
        if(! tagBuffer.ContainsKey(tag.Epc)) {
          // 新規検出タグ
          tagBuffer.Add(tag.Epc, new List<TagInfo>());
        }

        var infos = tagBuffer[tag.Epc];
        if(infos.Count == 25) {
          infos.RemoveAt(13);
        }

        infos.Add(tagInfo);
      }
    }


    /// <summary>
    /// リーダ接続検出時にプログラムを終了させる.
    /// XXX: 再接続処理の検討
    /// </summary>
    private static void OnUhfReaderConnectionLost(IUhfReader reader) {
      eventHandler?.Set();
    }


    /// <summary>
    /// 一定時間経過後のタグ情報を処理
    /// 3[s] 以上検出されなかったタグを処理
    /// </summary>
    private static async void OnIntervalTimerElapsed(object source, System.Timers.ElapsedEventArgs e) {
      KeyValuePair<string, List<TagInfo>>[] targets;

      lock(oLock) {
        targets = tagBuffer
          .Where(kvp => (DateTime.Now - kvp.Value.Max(info => info.DetectedAt)).TotalSeconds >= 3)
          .ToArray();

        foreach(var target in targets) {
          // 最大検出時刻から 3[s] 以上経過したタグ情報をバッファから削除を行い.
          // 必要な処理を行う.

          // タグ情報の削除
          tagBuffer.Remove(target.Key);

#if DEBUG
          Console.Error.WriteLine($"Debug  deleted {target.Key}, number of infos #{target.Value.Count}");
#endif
        }
      }

      var readerId = readerSettings?.Id;
      if(readerId == null) {
        return;
      }


      // XXX
      // 極端にデータが少ないものは対象外
      foreach(var target in targets.Where(t => t.Value.Count >= 4)) {
        // NOTE: リーダに接続するアンテナが互いに干渉しない環境下を想定.
        // より正確に検出を行わせる場合は, List 内のデータを正常に処理させる必要がある.
        var firstStatus = insideNumbers.Any(value => (ushort)value == target.Value.First().AntennaId)
          ? InOutStatus.Inside
          : InOutStatus.Outside;

        var lastStatus = outsideNumbers.Any(value => (ushort)value == target.Value.Last().AntennaId)
          ? InOutStatus.Outside
          : InOutStatus.Inside;

        // 入退室状態が同値でない場合が正常.
        // 入口, 出口付近を通過した際にどちらか一方のみで検出される可能性がある.
        if(firstStatus != lastStatus) {
          HistoryStatus historyStatus = (firstStatus == InOutStatus.Inside)
            ? HistoryStatus.Leaving // 内側から先に検出し, 外側へ状態の遷移: 退室
            : HistoryStatus.Entry;  // 外側から先に検出し, 内側へ状態の遷移: 入室

          // データ追加処理
          try {
#if DEBUG
            Console.Error.WriteLine($"Debug Reader:{readerId.Value}, Tag: {target.Key}, Status:{historyStatus}");
#endif

            await entleaving.Db.DAO.History.InsertByTagIdAsync(
                readerId: readerId.Value,
                tagId:    target.Key,
                status:   historyStatus);
          } catch(Exception except) {
            Console.Error.WriteLine($"Debug {except.GetType().Name} {except.Message} {except.StackTrace}");
          }
        }
      }
    }


    /// <summary></summary>
    private static void OnCancelKeyPress(object? source, ConsoleCancelEventArgs e) {
      if(eventHandler != null) {
        eventHandler.Set();
        e.Cancel = true;
      }
    }
  }
}
