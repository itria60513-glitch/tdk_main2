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

        /// <summary>Base Light Curtain error.</summary>
        LightCurtainError = -400,
    }
}
