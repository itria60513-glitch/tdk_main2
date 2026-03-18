using Communication.Interface;

namespace TDKController
{
    /// <summary>
    /// Defines the unified contract for carrier identifier reader operations.
    /// </summary>
    public interface ICarrierIDReader
    {
        /// <summary>
        /// Gets the shared carrier ID reader configuration.
        /// </summary>
        CarrierIDReaderConfig Config { get; }

        /// <summary>
        /// Gets the active carrier ID reader type.
        /// </summary>
        CarrierIDReaderType CarrierIDReaderType { get; }

        /// <summary>
        /// Gets or sets the communication connector to the reader hardware.
        /// External consumers MUST use this property to replace the connector at runtime;
        /// the implementation automatically re-wires DataReceived event subscriptions
        /// in the setter so that event routing stays consistent.
        /// </summary>
        IConnector Connector { get; set; }

        /// <summary>
        /// Reads the carrier identifier from the specified reader page.
        /// </summary>
        /// <param name="page">The reader page number to read. Barcode readers ignore this value.</param>
        /// <param name="carrierID">The returned carrier identifier when the call succeeds.</param>
        /// <returns>The read result.</returns>
        ErrorCode GetCarrierID(int page, out string carrierID);

        /// <summary>
        /// Writes the carrier identifier to the specified reader page.
        /// </summary>
        /// <param name="page">The reader page number to write. Barcode readers ignore this value.</param>
        /// <param name="carrierID">The carrier identifier payload to write.</param>
        /// <returns>The write result.</returns>
        ErrorCode SetCarrierID(int page, string carrierID);
    }

    /// <summary>
    /// Supported carrier ID reader types.
    /// </summary>
    public enum CarrierIDReaderType
    {
        BarcodeReader = 0,
        OmronASCII = 1,
        OmronHex = 2,
        HermesRFID = 3,
    }
}
