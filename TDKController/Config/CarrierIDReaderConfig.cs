namespace TDKController
{
    /// <summary>
    /// Configuration parameters for carrier ID reader workflows.
    /// </summary>
    public class CarrierIDReaderConfig
    {
        /// <summary>
        /// Gets or sets the single response wait timeout in milliseconds.
        /// </summary>
        public int TimeoutMs { get; set; } = 10000;

        /// <summary>
        /// Gets or sets the maximum retry count for the barcode reader.
        /// </summary>
        public int MaxRetryCount { get; set; } = 8;

        /// <summary>
        /// Gets or sets the RFID page number.
        /// </summary>
        public int Page { get; set; } = 1;
    }
}
