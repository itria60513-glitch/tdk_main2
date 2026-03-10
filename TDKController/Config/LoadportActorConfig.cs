namespace TDKController
{
    /// <summary>
    /// Configuration parameters for LoadportActor TAS300 communication.
    /// Timeout values are in milliseconds.
    /// </summary>
    public class LoadportActorConfig
    {
        /// <summary>
        /// TAS300 ACK response timeout in milliseconds. Default: 5000ms (5s).
        /// Per E191E372 §3.4 specification.
        /// </summary>
        public int AckTimeout { get; set; } = 5000;

        /// <summary>
        /// TAS300 INF/ABS completion timeout in milliseconds. Default: 10000ms (10s).
        /// Per E191E372 §3.4 specification.
        /// </summary>
        public int InfTimeout { get; set; } = 10000;
    }
}
