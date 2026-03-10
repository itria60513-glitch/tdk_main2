using System;

namespace DIO
{
    /// <summary>
    /// Base interface for all DIO board devices.
    /// Defines common connection management, device identification, and error reporting capabilities.
    /// </summary>
    public interface IIOBoardBase
    {
        /// <summary>
        /// Occurs when an exception is detected by the device.
        /// </summary>
        event EventHandler ExceptionOccurred;

        /// <summary>
        /// Gets a value indicating whether the device is currently connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets a value indicating whether the device is operating in virtual (simulated) mode.
        /// </summary>
        bool IsVirtual { get; }

        /// <summary>
        /// Gets the numeric identifier of the device.
        /// </summary>
        int DeviceID { get; }

        /// <summary>
        /// Gets the display name of the device.
        /// </summary>
        string DeviceName { get; }

        /// <summary>
        /// Establishes a connection to the device.
        /// </summary>
        /// <returns>An error code indicating the result. Specific values are defined by the implementing project.</returns>
        int Connect();

        /// <summary>
        /// Closes the connection to the device.
        /// </summary>
        /// <returns>An error code indicating the result. Specific values are defined by the implementing project.</returns>
        int Disconnect();
    }
}
