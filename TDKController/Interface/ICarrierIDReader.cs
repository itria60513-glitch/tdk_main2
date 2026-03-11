namespace TDKController.Interface
{
    /// <summary>
    /// Defines the unified contract for carrier identifier reader operations.
    /// </summary>
    public interface ICarrierIDReader
    {
        /// <summary>
        /// Gets the active carrier ID reader type.
        /// </summary>
        CarrierIDReaderType CarrierIDReaderType { get; }

        /// <summary>
        /// Parses raw device response data and updates reader state.
        /// </summary>
        /// <param name="command">Raw device response payload.</param>
        /// <returns>The parsing result.</returns>
        ErrorCode ParseCarrierIDReaderData(string command);

        /// <summary>
        /// Reads the carrier identifier from the configured reader.
        /// </summary>
        /// <param name="carrierID">The returned carrier identifier when the call succeeds.</param>
        /// <returns>The read result.</returns>
        ErrorCode GetCarrierID(out string carrierID);

        /// <summary>
        /// Writes the carrier identifier to a supported RFID reader.
        /// </summary>
        /// <param name="carrierID">The carrier identifier payload to write.</param>
        /// <returns>The write result.</returns>
        ErrorCode SetCarrierID(string carrierID);
    }
}

namespace TDKController
{
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
