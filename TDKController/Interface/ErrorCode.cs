namespace TDKController
{
    /// <summary>
    /// Unified error code enumeration for all modules.
    /// Return type for all public methods in module layer.
    /// Error codes use int base type with module-specific negative ranges.
    /// </summary>
    public enum ErrorCode : int
    {
        // === Common codes ===

        /// <summary>Operation completed successfully.</summary>
        Success = 0,

        // === LoadportActor range (-100 ~ -199) ===

        /// <summary>Base LoadportActor error.</summary>
        LoadportError = -100,

        /// <summary>TAS300 ACK response timeout (-101).</summary>
        AckTimeout = -101,

        /// <summary>TAS300 INF/ABS completion response timeout (-102).</summary>
        InfTimeout = -102,

        /// <summary>TAS300 command failed — NAK received or ABS completion (-103).</summary>
        CommandFailed = -103,

        // === Other module ranges (reserved) ===

        /// <summary>Base E84 error.</summary>
        E84Error = -1,

        /// <summary>Base N2 Purge error.</summary>
        N2PurgeError = -200,

        /// <summary>Base Carrier ID Reader error.</summary>
        CarrierIdError = -300,

        /// <summary>Response synchronization reset failed.</summary>
        CarrierIdSemaphoreResetFailed = -301,

        /// <summary>Reader response timed out.</summary>
        CarrierIdTimeout = -302,

        /// <summary>Reader command failed or returned an invalid response.</summary>
        CarrierIdCommandFailed = -303,

        /// <summary>Reader rejected the operation because another operation is still active.</summary>
        CarrierIdBusy = -304,

        /// <summary>Requested RFID page is outside the valid range for the current reader.</summary>
        CarrierIdInvalidPage = -305,

        /// <summary>Provided parameter payload is invalid for the current reader.</summary>
        CarrierIdInvalidParameter = -306,

        /// <summary>Response payload exceeded the configured or supported limits.</summary>
        CarrierIdResponseTooLong = -307,

        /// <summary>Hermes checksum validation failed.</summary>
        CarrierIdChecksumError = -308,

        /// <summary>Reader connection could not be established.</summary>
        CarrierIdConnectFailed = -309,

        /// <summary>Barcode motor-on handshake failed.</summary>
        CarrierIdMotorOnFailed = -310,

        /// <summary>Barcode motor-off handshake failed.</summary>
        CarrierIdMotorOffFailed = -311,

        /// <summary>Read operation exhausted all allowed attempts without a valid identifier.</summary>
        CarrierIdReadFailed = -312,

        /// <summary>Internal carrier reader processing failed.</summary>
        CarrierIdInternalError = -313,

        /// <summary>Base Light Curtain error.</summary>
        LightCurtainError = -400,
    }
}
