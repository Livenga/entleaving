using System;
using System.Linq;
using System.Threading.Tasks;
using Org.LLRP.LTK.LLRPV1;
using Org.LLRP.LTK.LLRPV1.DataType;
using Org.LLRP.LTK.LLRPV1.Impinj;


namespace entleaving.Uhf {
  using ServerTimer = System.Timers.Timer;


  /// <summary></summary>
  public class R420Reader : AbsLLRPReader, IUhfReader, IDisposable {
    /// <summary></summary>
    public UhfReaderType ReaderType =>
      UhfReaderType.ImpinjR420;

    /// <summary></summary>
    public bool IsReading { private set; get; } = false;

    /// <summary></summary>
    public event ConnectionLostEventHandler? ConnectionLost = null;
    /// <summary></summary>
    public event DetectedTagEventHandler?    DetectedTag    = null;


    private ServerTimer keepaliveTimer;
    private DateTime keepalivedAt = DateTime.Now;

    /// <summary></summary>
    public R420Reader() {
      this.keepaliveTimer = new ServerTimer(10000f);
      this.keepaliveTimer.Elapsed += this.OnKeepaliveTimer;
    }


    /// <summary></summary>
    private void OnKeepaliveTimer(object source, System.Timers.ElapsedEventArgs e) {
      DateTime now = DateTime.Now;

      ulong sec = (ulong)(now - this.keepalivedAt).TotalSeconds;
#if DEBUG
      //Console.Error.WriteLine($"Debug 経過秒: {sec}[s] # {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
#endif

      if(sec >= 30) {
        ((ServerTimer)source).Stop();
        this.ConnectionLost?.Invoke(this);
      }
    }


    /// <summary></summary>
    private void OnRoAccessReportReceived(MSG_RO_ACCESS_REPORT? msg) {
      if(msg == null) {
        return;
      }

      foreach(var report in msg.TagReportData) {
        ushort antennaId = report.AntennaID.AntennaID;
        string? epc = null;
        double? angle = null;
        double? rssi = null;

        for(int i = 0; i < report.Custom.Length; ++i) {
          switch(report.Custom[i]) {
            case PARAM_ImpinjRFPhaseAngle pAngle:
              angle = ((double)pAngle.PhaseAngle / 4096f) * 360f;
              break;

            case PARAM_ImpinjPeakRSSI pRssi:
              rssi = (double)pRssi.RSSI / 100f;
              break;
          }
        }

        switch(report.EPCParameter[0]) {
          case PARAM_EPCData pEpc:
            epc = pEpc.EPC.ToHexString();
            break;

          case PARAM_EPC_96 pEpc:
            epc = pEpc.EPC.ToHexString();
            break;
        }

        if(epc != null) {
          var tagData = new TagData(
              antennaId:  antennaId,
              epc:        epc,
              rssi:       rssi,
              phaseAngle: angle);

          this.DetectedTag?.Invoke(this, tagData);
        }
      }
    }


    /// <summary></summary>
    private void OnKeepAlive(MSG_KEEPALIVE msg) {
      this.keepalivedAt = DateTime.Now;

      var _msg = new MSG_KEEPALIVE_ACK();
      _msg.MSG_ID = msg.MSG_ID;

      MSG_ERROR_MESSAGE? msgErr = null;
      this.BaseClient?.KEEPALIVE_ACK(
          msg:      _msg,
          msg_err:  out msgErr,
          time_out: 3000);
    }


    /// <summary></summary>
    public void Open() {
      if(this.BaseClient != null && this.IsConnected) {
        return;
      }
      if(this.Settings == null) {
        throw new NullReferenceException();
      }

      this.BaseClient = new LLRPClient(port: this.Settings.Port);

      ENUM_ConnectionAttemptStatusType status;
      bool result = this.BaseClient.Open(
          llrp_reader_name: this.Settings.Hostname,
          timeout:          3000,
          status:           out status);

      if(! result || status != ENUM_ConnectionAttemptStatusType.Success) {
        this.BaseClient = null;
        throw new Exception($"{this.Settings.Hostname}: 接続失敗.");
      }

      this.ResetToFactoryDefault();
      this.EnableImpinjExtensions();

      //this.GetReaderConfig(true);

      try {
        this.DisableROSpec(0);
        this.DeleteROSpec(0);
      } catch(Exception except) {
        Console.Error.WriteLine($"{except.GetType().Name} {except.Message} {except.StackTrace}");
      }

      this.SetReaderConfig();

      this.BaseClient.OnKeepAlive              += this.OnKeepAlive;
      this.BaseClient.OnRoAccessReportReceived += this.OnRoAccessReportReceived;

      this.keepalivedAt = DateTime.Now;
      this.keepaliveTimer.Start();
    }


    /// <summary></summary>
    public void Close() {
      if(! this.IsConnected) {
        return;
      }

      this.keepaliveTimer.Stop();
      this.BaseClient?.Close();
    }

    /// <summary></summary>
    public void Start() {
      if(this.BaseClient == null) {
        return;
      }
      if(this.IsReading) {
        return;
      }

      this.AddROSpec(14150, true);
      this.EnableROSpec(14150);
      this.StartROSpec(14150);

      this.IsReading = true;
    }

    /// <summary></summary>
    public void Stop() {
      if(! this.IsReading) {
        return;
      }

      this.IsReading = false;

      this.StopROSpec(14150);
      this.DisableROSpec(14150);
      this.DeleteROSpec(14150);
    }


    /// <summary></summary>
    public void Dispose() {
      try {
        this.Close();
      } catch {
      }
    }


    /// <summary></summary>
    private void EnableImpinjExtensions() {
      MSG_IMPINJ_ENABLE_EXTENSIONS msg = new MSG_IMPINJ_ENABLE_EXTENSIONS();

      MSG_ERROR_MESSAGE? msgErr = null;
      MSG_CUSTOM_MESSAGE? msgResp = this.BaseClient?.CUSTOM_MESSAGE(
          msg:      msg,
          msg_err:  out msgErr,
          time_out: 3000);

      this.CheckLLRPError(msgResp, msgErr);
    }

    /// <summary></summary>
    private void SetReaderConfig() {
      MSG_SET_READER_CONFIG msg = new MSG_SET_READER_CONFIG();

      PARAM_KeepaliveSpec pKeepalive = new PARAM_KeepaliveSpec();
      msg.KeepaliveSpec = pKeepalive;
      pKeepalive.KeepaliveTriggerType = ENUM_KeepaliveTriggerType.Periodic;
      pKeepalive.PeriodicTriggerValue = 15000;

      PARAM_ImpinjLinkMonitorConfiguration pLinkMonitor = new PARAM_ImpinjLinkMonitorConfiguration();
      pLinkMonitor.LinkDownThreshold = 4;
      pLinkMonitor.LinkMonitorMode   = ENUM_ImpinjLinkMonitorMode.Enabled;
      msg.Custom.Add(pLinkMonitor);

      //
      msg.AntennaConfiguration = new PARAM_AntennaConfiguration[1];
      var pAntConfig = new PARAM_AntennaConfiguration();
      msg.AntennaConfiguration[0] = pAntConfig;

      pAntConfig.AntennaID = 0;
      pAntConfig.AirProtocolInventoryCommandSettings = new UNION_AirProtocolInventoryCommandSettings();

      var pInventoryCommand = new PARAM_C1G2InventoryCommand();
      pAntConfig.AirProtocolInventoryCommandSettings.Add(pInventoryCommand);
      pInventoryCommand.TagInventoryStateAware = false;

      MSG_ERROR_MESSAGE? msgErr = null;
      var msgResp = this.BaseClient?.SET_READER_CONFIG(
          msg:      msg,
          msg_err:  out msgErr,
          time_out: 3000);

      this.CheckLLRPError(msgResp, msgErr);
    }


    /// <summary></summary>
    private void GetReaderConfig(bool isSaved = false) {
      var msg = new MSG_GET_READER_CONFIG();
      msg.AntennaID = 0;
      msg.RequestedData = ENUM_GetReaderConfigRequestedData.All;

      MSG_ERROR_MESSAGE? msgErr = null;
      var resp = this.BaseClient?.GET_READER_CONFIG(msg, out msgErr, 3000);
      this.CheckLLRPError(resp, msgErr);

      if(resp != null) {
        Console.Error.WriteLine($"{resp.ToString()}");

        if(isSaved) {
          System.IO.File.WriteAllTextAsync(
              path:     "get_reader_config.xml",
              contents: resp.ToString())
            .Wait();
        }
      }
    }
  }
}
