using System;
using Communication.Interface;
using TDKController.CarrierIDReader.ModuleConfig;
using TDKController.Interface;
using TDKLogUtility.Module;

namespace TDKController.CarrierIDReader.Module
{
    /// <summary>
    /// Abstract base class for all Carrier ID reader sub-classes.
    /// Stores the injected dependencies and declares the abstract members that
    /// each concrete sub-class must implement.
    /// Follows the same constructor-injection / null-guard pattern used by
    /// LoadportActor (see constitution §4.1).
    /// </summary>
    public abstract class CarrierIDReader : ICarrierIDReader
    {
        // === Injected Dependencies ===

        /// <summary>Communication channel used to send/receive hardware frames.</summary>
        protected readonly IConnector _connector;

        /// <summary>Logger used to record commands, responses and errors.</summary>
        protected readonly ILogUtility _logger;

        /// <summary>Reader configuration (port, type, config-file path, etc.).</summary>
        protected readonly CarrierIDReaderConfig _config;

        // === Constructor ===

        /// <summary>
        /// Initializes the base class and validates all injected dependencies.
        /// </summary>
        /// <param name="config">Reader configuration. Must not be null.</param>
        /// <param name="connector">Serial/TCP communication channel. Must not be null.</param>
        /// <param name="logger">Log utility instance. Must not be null.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the parameters is null.
        /// </exception>
        protected CarrierIDReader(
            CarrierIDReaderConfig config,
            IConnector connector,
            ILogUtility logger)
        {
            _config    = config    ?? throw new ArgumentNullException(nameof(config));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _logger    = logger    ?? throw new ArgumentNullException(nameof(logger));
        }

        // === Abstract Interface Members ===
        // Each sub-class provides its own protocol-specific implementation.

        /// <inheritdoc/>
        public abstract string CarrierIDReaderConfigPath { get; }

        /// <inheritdoc/>
        public abstract CarrierIDReaderType carrierIDReaderType { get; }

        /// <inheritdoc/>
        public abstract ErrorCode GetCarrierID(out string carrierId);

        /// <inheritdoc/>
        public abstract ErrorCode SetCarrierID(string carrierId, int page);

        /// <inheritdoc/>
        public abstract ErrorCode ParseCarrierIDReaderData(byte[] rawData, out string carrierId);
    }
}

