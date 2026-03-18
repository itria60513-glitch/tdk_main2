namespace TDKController
{
    /// <summary>
    /// Shared configuration parameters for carrier ID reader workflows.
    /// </summary>
    public class CarrierIDReaderConfig
    {
        /// <summary>
        /// Gets or sets the single response wait timeout in milliseconds.
        /// </summary>
        public int TimeoutMs { get; set; } = 3000;

        /// <summary>
        /// Gets or sets the maximum retry count for the barcode reader.
        /// </summary>
        public int BarcodeReaderMaxRetryCount { get; set; } = 8;
    }
}
