using System;

namespace DIO
{
    /// <summary>
    /// Interface for digital input (DI) capabilities.
    /// Inherits common device functionality from <see cref="IIOBoardBase"/>.
    /// </summary>
    public interface IDI : IIOBoardBase
    {
        /// <summary>
        /// Occurs when a digital input value changes.
        /// </summary>
        event EventHandler DI_ValueChanged;

        /// <summary>
        /// Gets the number of bits per input port.
        /// </summary>
        int InputBitsPerPort { get; }

        /// <summary>
        /// Gets the total number of input ports.
        /// </summary>
        int InputPortCount { get; }

        /// <summary>
        /// Reads the current value of the specified input port.
        /// </summary>
        /// <param name="portIndex">The zero-based index of the input port.</param>
        /// <param name="value">When this method returns, contains the byte value read from the port.</param>
        /// <returns>An error code indicating the result. Specific values are defined by the implementing project.</returns>
        int GetInput(int portIndex, out byte value);

        /// <summary>
        /// Reads the value of a specific bit within the specified input port.
        /// </summary>
        /// <param name="portIndex">The zero-based index of the input port.</param>
        /// <param name="bitIndex">The zero-based index of the bit within the port.</param>
        /// <param name="value">When this method returns, contains the bit value (0 or 1) as a byte.</param>
        /// <returns>An error code indicating the result. Specific values are defined by the implementing project.</returns>
        int GetInputBit(int portIndex, int bitIndex, out byte value);

        /// <summary>
        /// Starts snapshot (interrupt-driven) monitoring of input changes.
        /// </summary>
        /// <returns>An error code indicating the result. Specific values are defined by the implementing project.</returns>
        int SnapStart();

        /// <summary>
        /// Stops snapshot (interrupt-driven) monitoring of input changes.
        /// </summary>
        /// <returns>An error code indicating the result. Specific values are defined by the implementing project.</returns>
        int SnapStop();
    }
}
