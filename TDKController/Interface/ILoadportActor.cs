using System;
using Communication.Interface;

namespace TDKController
{
    // === Delegate Definitions ===

    /// <summary>
    /// Delegate for LED status change notification.
    /// </summary>
    /// <param name="ledNo">LED number (1-10).</param>
    /// <param name="status">New status: 0=OFF, 1=ON, 2=BLINK.</param>
    public delegate void LedChangedEventHandler(int ledNo, int status);

    /// <summary>
    /// Delegate for slot map scan completed notification.
    /// </summary>
    /// <param name="slotMap">SEMI format slot map array (25 slots).</param>
    public delegate void SlotMapScannedEventHandler(int[] slotMap);

    /// <summary>
    /// Delegate for FOUP report started notification.
    /// </summary>
    /// <param name="reportType">Report type identifier.</param>
    public delegate void FoupReportStartedEventHandler(int reportType);

    /// <summary>
    /// Delegate for loadport status change notification.
    /// Raised when GET:STATE response differs from previous cached status.
    /// </summary>
    /// <param name="newStatus">New status snapshot.</param>
    public delegate void StatusChangedEventHandler(LoadportStatus newStatus);

    /// <summary>
    /// Interface for loadport hardware actor operations.
    /// Action commands return ErrorCode only (no out parameter).
    /// Query commands return ErrorCode and output data via 'out string' parameter.
    /// </summary>
    public interface ILoadPortActor : IDisposable
    {
        // === Properties ===

        /// <summary>
        /// Gets or sets the communication channel to TAS300 hardware.
        /// Replacing the connector at runtime automatically re-wires
        /// the DataReceived event subscription inside the implementation.
        /// External consumers MUST use this property to swap connectors;
        /// the implementation handles unsubscribe/subscribe internally.
        /// </summary>
        IConnector Connector { get; set; }

        /// <summary>
        /// Cached SEMI format slot map string from last scan.
        /// Null if no scan has been performed.
        /// </summary>
        string SlotMap { get; }

        /// <summary>
        /// Cached TAS300 equipment status from last GET:STATE query.
        /// </summary>
        LoadportStatus Status { get; }

        /// <summary>
        /// Current FOUP mechanical status (FPS_UNKNOWN through FPS_OPENED).
        /// Thread-safe volatile read.
        /// </summary>
        int FoupStatus { get; }

        /// <summary>
        /// Most recent FOUP event type (FPEVT_NONE through FPEVT_PODON).
        /// Thread-safe volatile read.
        /// </summary>
        int FoupEvent { get; }

        // === Events ===

        /// <summary>
        /// Raised when an LED status changes via LedOn/LedOff/LedBlink.
        /// </summary>
        event LedChangedEventHandler LedChanged;

        /// <summary>
        /// Raised when TAS300 GET:STATE response differs from previously cached status.
        /// </summary>
        event StatusChangedEventHandler StatusChanged;

        // === Initialization ===

        /// <summary>
        /// Origin search — sends MOV:ORGSH to TAS300.
        /// </summary>
        /// <returns>ErrorCode: Success(0), AckTimeout(-101), InfTimeout(-102), CommandFailed(-103).</returns>
        ErrorCode Init();

        /// <summary>
        /// Abort origin — sends MOV:ABORG to TAS300.
        /// </summary>
        /// <returns>ErrorCode: Success(0), AckTimeout(-101), InfTimeout(-102), CommandFailed(-103).</returns>
        ErrorCode InitForce();

        /// <summary>
        /// Program initialization — sends SET:INITL to TAS300.
        /// </summary>
        /// <returns>ErrorCode: Success(0), AckTimeout(-101), InfTimeout(-102), CommandFailed(-103).</returns>
        ErrorCode InitProgram();

        // === Load Operations ===

        /// <summary>
        /// Full load sequence — sends MOV:CLOAD to TAS300.
        /// </summary>
        /// <returns>ErrorCode: Success(0), AckTimeout(-101), InfTimeout(-102), CommandFailed(-103).</returns>
        ErrorCode Load();

        /// <summary>
        /// Pod clamp — sends MOV:PODCL to TAS300.
        /// </summary>
        /// <returns>ErrorCode: Success(0), AckTimeout(-101), InfTimeout(-102), CommandFailed(-103).</returns>
        ErrorCode Clamp();

        /// <summary>
        /// Dock (CLoad Y Direction) — sends MOV:CLDYD to TAS300.
        /// </summary>
        /// <returns>ErrorCode: Success(0), AckTimeout(-101), InfTimeout(-102), CommandFailed(-103).</returns>
        ErrorCode Dock();

        // === Unload Operations ===

        /// <summary>
        /// Unload FOUP — sends MOV:PODOP (if clamped) or MOV:ABORG (otherwise).
        /// </summary>
        /// <returns>ErrorCode: Success(0), AckTimeout(-101), InfTimeout(-102), CommandFailed(-103).</returns>
        ErrorCode Unload();

        /// <summary>
        /// Undock — sends MOV:CULFC to TAS300.
        /// </summary>
        /// <returns>ErrorCode: Success(0), AckTimeout(-101), InfTimeout(-102), CommandFailed(-103).</returns>
        ErrorCode Undock();

        /// <summary>
        /// Close door — sends MOV:CULYD to TAS300.
        /// </summary>
        /// <returns>ErrorCode: Success(0), AckTimeout(-101), InfTimeout(-102), CommandFailed(-103).</returns>
        ErrorCode CloseDoor();

        // === Error Recovery ===

        /// <summary>
        /// Reset TAS300 recoverable error — sends SET:RESET as Operation Command.
        /// </summary>
        /// <returns>ErrorCode: Success(0), AckTimeout(-101), InfTimeout(-102), CommandFailed(-103).</returns>
        ErrorCode ResetError();

        // === Status Query ===

        /// <summary>
        /// Query TAS300 status — sends GET:STATE (Quick Command).
        /// Updates internal Status cache on success.
        /// </summary>
        /// <param name="data">20-character status string on success; empty string on failure.</param>
        /// <returns>ErrorCode: Success(0), AckTimeout(-101), CommandFailed(-103).</returns>
        ErrorCode GetLPStatus(out string data);

        /// <summary>
        /// Read cached LED status for specified LED number.
        /// </summary>
        /// <param name="data">LED status value ("0"=OFF, "1"=ON, "2"=BLINK); empty if invalid ledNo.</param>
        /// <param name="ledNo">LED number (1-10).</param>
        /// <returns>ErrorCode.Success always.</returns>
        ErrorCode GetLedStatus(out string data, int ledNo);

        /// <summary>
        /// Query FOUP status — calls GetFxlAmhsStatus() internally to refresh
        /// FoupStatus and produce statfxl hex code.
        /// </summary>
        /// <param name="statfxl">Hex status code (e.g. "0x69"); empty string on failure.</param>
        /// <returns>ErrorCode: Success(0), AckTimeout(-101), CommandFailed(-103).</returns>
        ErrorCode GetFOUPStatus(out string statfxl);

        // === LED Control ===

        /// <summary>
        /// Turn on LED — sends SET:LON{nn} as Operation Command.
        /// </summary>
        /// <param name="ledNo">LED number (1-10).</param>
        /// <returns>ErrorCode: Success(0), AckTimeout(-101), InfTimeout(-102), CommandFailed(-103).</returns>
        ErrorCode LedOn(int ledNo);

        /// <summary>
        /// Set LED to blink — sends SET:LBL{nn} as Operation Command.
        /// </summary>
        /// <param name="ledNo">LED number (1-10).</param>
        /// <returns>ErrorCode: Success(0), AckTimeout(-101), InfTimeout(-102), CommandFailed(-103).</returns>
        ErrorCode LedBlink(int ledNo);

        /// <summary>
        /// Turn off LED — sends SET:LOF{nn} as Operation Command.
        /// </summary>
        /// <param name="ledNo">LED number (1-10).</param>
        /// <returns>ErrorCode: Success(0), AckTimeout(-101), InfTimeout(-102), CommandFailed(-103).</returns>
        ErrorCode LedOff(int ledNo);

        // === Slot Map ===

        /// <summary>
        /// Execute wafer mapping — sends MOV:MAPDO then GET:MAPRD.
        /// Converts result to SEMI format and caches.
        /// </summary>
        /// <param name="data">SEMI format slot map string on success; empty on failure.</param>
        /// <returns>ErrorCode: Success(0), AckTimeout(-101), InfTimeout(-102), CommandFailed(-103).</returns>
        ErrorCode ScanSlotMapStatus(out string data);

        /// <summary>
        /// Return cached slot map from last scan.
        /// </summary>
        /// <param name="data">Cached SEMI format string; empty if no prior scan.</param>
        /// <returns>ErrorCode.Success always.</returns>
        ErrorCode ReturnSlotMapStatus(out string data);

        // === Event Reporting ===

        /// <summary>
        /// Enable all TAS300 events — sends EVT:EVTON + EVT:FPEON.
        /// </summary>
        /// <returns>ErrorCode: Success(0), AckTimeout(-101), CommandFailed(-103).</returns>
        ErrorCode StartReportLoadport();

        /// <summary>
        /// Enable FOUP events — sends EVT:FPEON.
        /// </summary>
        /// <returns>ErrorCode: Success(0), AckTimeout(-101), CommandFailed(-103).</returns>
        ErrorCode StartReportFOUP();

        /// <summary>
        /// Disable all TAS300 events — sends EVT:EVTOF.
        /// </summary>
        /// <returns>ErrorCode: Success(0), AckTimeout(-101), CommandFailed(-103).</returns>
        ErrorCode StopReportLoadport();

        /// <summary>
        /// Disable FOUP events — sends EVT:FPEOF.
        /// </summary>
        /// <returns>ErrorCode: Success(0), AckTimeout(-101), CommandFailed(-103).</returns>
        ErrorCode StopReportFOUP();

        // === Debug / Pass-through ===

        /// <summary>
        /// Send arbitrary command to TAS300 with auto-selected handshake by prefix.
        /// MOV/SET use ACK→INF/ABS; MOD/GET/EVT/TCH use ACK-only.
        /// Unknown prefixes are rejected and return CommandFailed.
        /// </summary>
        /// <param name="data">Raw response data on success; empty on failure.</param>
        /// <param name="command">TAS300 command string (e.g. "GET:STATE").</param>
        /// <returns>ErrorCode: Success(0), AckTimeout(-101), InfTimeout(-102), CommandFailed(-103).</returns>
        ErrorCode SendLoadportCommand(out string data, string command);
    }
}
