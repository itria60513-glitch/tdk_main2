using System;

namespace DIO
{
    /// <summary>
    /// Interface for digital output (DO) capabilities.
    /// Inherits common device functionality from <see cref="IIOBoardBase"/>.
    /// </summary>
    public interface IDO : IIOBoardBase
    {
        /// <summary>
        /// Occurs when a digital output value changes.
        /// </summary>
        event EventHandler DO_ValueChanged;

        /// <summary>
        /// Gets the number of bits per output port.
        /// </summary>
        int OutputBitsPerPort { get; }

        /// <summary>
        /// Gets the total number of output ports.
        /// </summary>
        int OutputPortCount { get; }

        /// <summary>
        /// Sets the value of the specified output port.
        /// </summary>
        /// <param name="portIndex">The zero-based index of the output port.</param>
        /// <param name="value">The byte value to write to the port.</param>
        /// <returns>An error code indicating the result. Specific values are defined by the implementing project.</returns>
        int SetOutput(int portIndex, byte value);

        /// <summary>
        /// Sets the value of a specific bit within the specified output port.
        /// </summary>
        /// <param name="portIndex">The zero-based index of the output port.</param>
        /// <param name="bitIndex">The zero-based index of the bit within the port.</param>
        /// <param name="value">The bit value (0 or 1) as a byte.</param>
        /// <returns>An error code indicating the result. Specific values are defined by the implementing project.</returns>
        int SetOutputBit(int portIndex, int bitIndex, byte value);

        /// <summary>
        /// Reads the current value of the specified output port.
        /// </summary>
        /// <param name="portIndex">The zero-based index of the output port.</param>
        /// <param name="value">When this method returns, contains the byte value of the port.</param>
        /// <returns>An error code indicating the result. Specific values are defined by the implementing project.</returns>
        int GetOutput(int portIndex, out byte value);

        /// <summary>
        /// Reads the value of a specific bit within the specified output port.
        /// </summary>
        /// <param name="portIndex">The zero-based index of the output port.</param>
        /// <param name="bitIndex">The zero-based index of the bit within the port.</param>
        /// <param name="value">When this method returns, contains the bit value (0 or 1) as a byte.</param>
        /// <returns>An error code indicating the result. Specific values are defined by the implementing project.</returns>
        int GetOutputBit(int portIndex, int bitIndex, out byte value);
    }
}
