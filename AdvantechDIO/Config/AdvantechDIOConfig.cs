namespace AdvantechDIO.Config
{
    /// <summary>
    /// Configuration model for AdvantechDIO, mapped from XML settings.
    /// Drives device ID and DI/DO port/pin topology.
    /// </summary>
    public class AdvantechDIOConfig
    {
        /// <summary>
        /// Advantech device ID, mapped to DeviceID (XML: Index).
        /// </summary>
        public int DeviceID { get; set; }

        /// <summary>
        /// Maximum number of digital input pins (XML: DIPortMax).
        /// Used for GUI rendering and to derive <see cref="DIPortCount"/>.
        /// </summary>
        public int DIPortMax { get; set; }

        /// <summary>
        /// Maximum number of digital output pins (XML: DOPortMax).
        /// Used for GUI rendering and to derive <see cref="DOPortCount"/>.
        /// </summary>
        public int DOPortMax { get; set; }

        /// <summary>
        /// Number of pins (bits) per port, shared for both DI and DO on the same board (XML: PinCountPerPort).
        /// </summary>
        public int PinCountPerPort { get; set; }

        /// <summary>
        /// Computed number of digital input ports. 0 means DI is not configured.
        /// Derived from <see cref="DIPortMax"/> / <see cref="PinCountPerPort"/>.
        /// </summary>
        public int DIPortCount => PinCountPerPort > 0 ? DIPortMax / PinCountPerPort : 0;

        /// <summary>
        /// Computed number of digital output ports. 0 means DO is not configured.
        /// Derived from <see cref="DOPortMax"/> / <see cref="PinCountPerPort"/>.
        /// </summary>
        public int DOPortCount => PinCountPerPort > 0 ? DOPortMax / PinCountPerPort : 0;
    }
}
