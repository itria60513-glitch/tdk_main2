namespace TDKController.Interface
{
    /// <summary>
    /// Hardware type of the Carrier ID reader.
    /// Used to identify and instantiate the correct sub-class via configuration.
    /// </summary>
    public enum CarrierIDReaderType
    {
        /// <summary>Keyence BL-600 barcode reader (type 1).</summary>
        BarcodeReader = 1,

        /// <summary>Hermes protocol RFID reader (type 2).</summary>
        HermesRFID = 2,

        /// <summary>Omron RFID reader in ASCII content format (type 3).</summary>
        OmronASCII = 3,

        /// <summary>Omron RFID reader in HEX content format (type 4).</summary>
        OmronHex = 4,
    }

    /// <summary>
    /// Contract for all Carrier ID reader sub-classes.
    /// Callers always program to this interface; the concrete type is selected
    /// at construction time via <see cref="CarrierIDReaderType"/> configuration.
    /// Action and query methods follow the ErrorCode / out-string convention
    /// established by ILoadPortActor:
    ///   - Return <see cref="ErrorCode.Success"/> (0) on success.
    ///   - Return a negative CarrierID-range code on failure.
    ///   - Methods that yield a string result use an <c>out string</c> parameter.
    /// </summary>
    public interface ICarrierIDReader
    {
        // === Properties ===

        /// <summary>
        /// File-system path of the reader configuration file.
        /// </summary>
        string CarrierIDReaderConfigPath { get; }

        /// <summary>
        /// Hardware type of this reader instance.
        /// Identifies the concrete sub-class in use.
        /// </summary>
        CarrierIDReaderType carrierIDReaderType { get; }

        // === Core Methods ===

        /// <summary>
        /// Read the Carrier ID from the hardware reader.
        /// </summary>
        /// <param name="carrierId">
        /// Carrier ID string on success; <see cref="string.Empty"/> on failure.
        /// </param>
        /// <returns>
        /// <see cref="ErrorCode.Success"/> (0) on success;
        /// negative CarrierID-range code on failure (see FR-010).
        /// </returns>
        ErrorCode GetCarrierID(out string carrierId);

        /// <summary>
        /// Write a Carrier ID to the hardware reader / tag.
        /// Not supported by all reader types (e.g. barcode readers are read-only).
        /// </summary>
        /// <param name="carrierId">
        /// Carrier ID string to write. Must not exceed 16 characters.
        /// </param>
        /// <param name="page">Target page number on the RFID tag (1-based).</param>
        /// <returns>
        /// <see cref="ErrorCode.Success"/> (0) on success;
        /// negative CarrierID-range code on failure.
        /// </returns>
        ErrorCode SetCarrierID(string carrierId, int page);

        /// <summary>
        /// Parse raw protocol response bytes into a Carrier ID string.
        /// Each sub-class applies its own protocol-specific parsing logic.
        /// </summary>
        /// <param name="rawData">Raw response byte array from the hardware reader.</param>
        /// <param name="carrierId">
        /// Parsed Carrier ID string on success; <see cref="string.Empty"/> on failure.
        /// </param>
        /// <returns>
        /// <see cref="ErrorCode.Success"/> (0) on success;
        /// negative CarrierID-range code on failure.
        /// </returns>
        ErrorCode ParseCarrierIDReaderData(byte[] rawData, out string carrierId);
    }
}
