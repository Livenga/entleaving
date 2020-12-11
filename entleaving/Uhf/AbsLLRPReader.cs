using System;
using System.Linq;
using Org.LLRP.LTK.LLRPV1;
using Org.LLRP.LTK.LLRPV1.DataType;
using Org.LLRP.LTK.LLRPV1.Impinj;


namespace entleaving.Uhf {
  public abstract class AbsLLRPReader {
    public Settings? Settings       { set; get; } = null;
    public bool      IsConnected => this.BaseClient?.IsConnected ?? false;

    protected LLRPClient? BaseClient = null;


    /// <summary></summary>
    protected void CheckLLRPError(
        Message?           msg = null,
        MSG_ERROR_MESSAGE? error = null) {
      if(msg == null && error == null) {
        throw new NullReferenceException();
      }

      PARAM_LLRPStatus? status = error?.LLRPStatus;
      if(status == null) {
        status = msg?.GetType().GetField("LLRPStatus")?.GetValue(msg) as PARAM_LLRPStatus;
      }

      if(status == null) {
        throw new NullReferenceException();
      }

      if(status.StatusCode != ENUM_StatusCode.M_Success) {
        throw new Exception($"{status.ErrorDescription ?? string.Empty}");
      }
    }


    /// <summary></summary>
    protected void ResetToFactoryDefault() {
      MSG_SET_READER_CONFIG msg = new MSG_SET_READER_CONFIG();
      msg.ResetToFactoryDefault = true;

      MSG_ERROR_MESSAGE? msgErr = null;
      MSG_SET_READER_CONFIG_RESPONSE? msgResp = this.BaseClient?.SET_READER_CONFIG(
          msg:      msg,
          msg_err:  out msgErr,
          time_out: 3000);

      this.CheckLLRPError(msgResp, msgErr);
    }


    /// <summary></summary>
    protected void StartROSpec(uint roSpecId) {
      MSG_START_ROSPEC           msg     = new MSG_START_ROSPEC() { ROSpecID = roSpecId };
      MSG_ERROR_MESSAGE?         msgErr  = null;
      MSG_START_ROSPEC_RESPONSE? msgResp = this.BaseClient?.START_ROSPEC(
          msg:      msg,
          msg_err:  out msgErr,
          time_out: 3000);

      this.CheckLLRPError(msgResp, msgErr);
    }
      

    /// <summary>ROSpec 有効化</summary>
    protected void EnableROSpec(uint roSpecId) {
      MSG_ENABLE_ROSPEC           msg     = new MSG_ENABLE_ROSPEC() { ROSpecID = roSpecId };
      MSG_ERROR_MESSAGE?          msgErr  = null;
      MSG_ENABLE_ROSPEC_RESPONSE? msgResp = this.BaseClient?.ENABLE_ROSPEC(
          msg:      msg,
          msg_err:  out msgErr,
          time_out: 3000);

      this.CheckLLRPError(msgResp, msgErr);
    }


    /// <summary>ROSpec 追加</summary>
    protected virtual void AddROSpec(uint roSpecId, bool isEnabledImpinjExtensions = false) {
      MSG_ADD_ROSPEC msg     = new MSG_ADD_ROSPEC();

      PARAM_ROSpec pROSpec = new PARAM_ROSpec();
      msg.ROSpec = pROSpec;

      pROSpec.ROSpecID = roSpecId;
      pROSpec.Priority = 0;
      pROSpec.CurrentState = ENUM_ROSpecState.Disabled;

      //
      PARAM_ROBoundarySpec pBoundary = new PARAM_ROBoundarySpec();
      pROSpec.ROBoundarySpec = pBoundary;

      // 開始トリガ
      pBoundary.ROSpecStartTrigger = new PARAM_ROSpecStartTrigger();
      pBoundary.ROSpecStartTrigger.ROSpecStartTriggerType = ENUM_ROSpecStartTriggerType.Null;
      // 停止トリガ
      pBoundary.ROSpecStopTrigger = new PARAM_ROSpecStopTrigger();
      pBoundary.ROSpecStopTrigger.ROSpecStopTriggerType = ENUM_ROSpecStopTriggerType.Null;
      pBoundary.ROSpecStopTrigger.DurationTriggerValue = 0;

      // レポートスペック
      PARAM_ROReportSpec pReport = new PARAM_ROReportSpec();
      pROSpec.ROReportSpec = pReport;
      pReport.N = 1;
      pReport.ROReportTrigger = ENUM_ROReportTriggerType.Upon_N_Tags_Or_End_Of_ROSpec;


      PARAM_TagReportContentSelector pContentSelector = new PARAM_TagReportContentSelector();
      pReport.TagReportContentSelector = pContentSelector;
      pContentSelector.EnableAntennaID = true;
      pContentSelector.EnablePeakRSSI = true;
      pContentSelector.EnableROSpecID = true;
      pContentSelector.EnableFirstSeenTimestamp = true;


      if(isEnabledImpinjExtensions) {
        var pImpinjContentSelector = new PARAM_ImpinjTagReportContentSelector();
        pReport.Custom.Add(pImpinjContentSelector);

        pImpinjContentSelector.ImpinjEnablePeakRSSI = new PARAM_ImpinjEnablePeakRSSI() {
          PeakRSSIMode = ENUM_ImpinjPeakRSSIMode.Enabled
        };
        pImpinjContentSelector.ImpinjEnableRFPhaseAngle = new PARAM_ImpinjEnableRFPhaseAngle() {
          RFPhaseAngleMode = ENUM_ImpinjRFPhaseAngleMode.Enabled
        };
      }

      pROSpec.SpecParameter = new UNION_SpecParameter();


      // AISpec
      PARAM_AISpec pAI = new PARAM_AISpec();
      pROSpec.SpecParameter.Add(pAI);
      pAI.AntennaIDs = new UInt16Array();
      pAI.AntennaIDs.Add(0);

      // AISpec Stop Trigger
      PARAM_AISpecStopTrigger pAISpecStopTrigger = new PARAM_AISpecStopTrigger();
      pAI.AISpecStopTrigger = pAISpecStopTrigger;
      pAISpecStopTrigger.AISpecStopTriggerType = ENUM_AISpecStopTriggerType.Null;

      PARAM_InventoryParameterSpec pInventory = new PARAM_InventoryParameterSpec();
      pAI.InventoryParameterSpec = new PARAM_InventoryParameterSpec[1];
      pAI.InventoryParameterSpec[0] = pInventory;

      pInventory.InventoryParameterSpecID = 4567;
      pInventory.ProtocolID = ENUM_AirProtocols.EPCGlobalClass1Gen2;

      if(this.Settings != null) {
        pAI.AntennaIDs = new UInt16Array();
        foreach(var ant in this.Settings.Antennas.Where(ant => ant.IsEnabled)) {
          pAI.AntennaIDs.Add(ant.Id);
        }

        pInventory.AntennaConfiguration =
          new PARAM_AntennaConfiguration[this.Settings.Antennas.Count];

        for(ushort aid = 0; aid < this.Settings.Antennas.Count; ++aid) {
          PARAM_AntennaConfiguration pAntenna = new PARAM_AntennaConfiguration();
          pInventory.AntennaConfiguration[aid] = pAntenna;

          var antenna = this.Settings.Antennas[aid];
          pAntenna.AntennaID = antenna.Id;
          pAntenna.RFTransmitter = new PARAM_RFTransmitter();
          pAntenna.RFTransmitter.ChannelIndex  = 1;
          pAntenna.RFTransmitter.HopTableID    = 0;
          pAntenna.RFTransmitter.TransmitPower = antenna.Tx.Id;

          pAntenna.RFReceiver = new PARAM_RFReceiver();
          pAntenna.RFReceiver.ReceiverSensitivity = antenna.Rx.Id;


          pAntenna.AirProtocolInventoryCommandSettings = new UNION_AirProtocolInventoryCommandSettings();
          var pInventoryCommand = new PARAM_C1G2InventoryCommand();
          pAntenna.AirProtocolInventoryCommandSettings.Add(pInventoryCommand);

          pInventoryCommand.TagInventoryStateAware = false;
          pInventoryCommand.C1G2RFControl = new PARAM_C1G2RFControl();
          pInventoryCommand.C1G2RFControl.ModeIndex = 1000;
          pInventoryCommand.C1G2RFControl.Tari = 0;

          pInventoryCommand.C1G2SingulationControl = new PARAM_C1G2SingulationControl();
          pInventoryCommand.C1G2SingulationControl.Session = new TwoBits(false, false);
          pInventoryCommand.C1G2SingulationControl.TagPopulation = 32;
          pInventoryCommand.C1G2SingulationControl.TagTransitTime = 0;
        }
      }

      MSG_ERROR_MESSAGE?       msgErr  = null;
      MSG_ADD_ROSPEC_RESPONSE? msgResp = this.BaseClient?.ADD_ROSPEC(
            msg:      msg,
            msg_err:  out msgErr,
            time_out: 3000);

      this.CheckLLRPError(msgResp, msgErr);
    }


    /// <summary>ROSpec 停止</summary>
    protected void StopROSpec(uint roSpecId = 0) {
      MSG_STOP_ROSPEC           msg     = new MSG_STOP_ROSPEC() { ROSpecID = roSpecId };
      MSG_ERROR_MESSAGE?        msgErr  = null;
      MSG_STOP_ROSPEC_RESPONSE? msgResp = this.BaseClient?.STOP_ROSPEC(
          msg:    msg,
          msg_err: out msgErr,
          time_out: 3000);

      this.CheckLLRPError(msgResp, msgErr);
    }


    /// <summary>ROSpec 無効化</summary>
    protected void DisableROSpec(uint roSpecId = 0) {
      MSG_DISABLE_ROSPEC           msg     = new MSG_DISABLE_ROSPEC() { ROSpecID = roSpecId };
      MSG_ERROR_MESSAGE?           msgErr  = null;
      MSG_DISABLE_ROSPEC_RESPONSE? msgResp = this.BaseClient?.DISABLE_ROSPEC(
          msg:      msg,
          msg_err:  out msgErr,
          time_out: 3000);

      this.CheckLLRPError(msgResp, msgErr);
    }


    /// <summary>ROSpec 削除</summary>
    protected void DeleteROSpec(uint roSpecId = 0) {
      MSG_DELETE_ROSPEC           msg     = new MSG_DELETE_ROSPEC() { ROSpecID = roSpecId };
      MSG_ERROR_MESSAGE?          msgErr  = null;
      MSG_DELETE_ROSPEC_RESPONSE? msgResp = this.BaseClient?.DELETE_ROSPEC(
          msg:      msg,
          msg_err:  out msgErr,
          time_out: 3000);

      this.CheckLLRPError(msgResp, msgErr);
    }
  }
}
