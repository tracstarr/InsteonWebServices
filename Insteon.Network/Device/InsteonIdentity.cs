namespace Insteon.Network.Device
{
    /// <summary>
    /// Represents the INSTEON device type.
    /// </summary>
    public struct InsteonIdentity
    {
        /// <summary>
        /// Initializes a new instance of the InsteonIdentity class.
        /// </summary>
        /// <param name="devCat">The device category representing a product family.</param>
        /// <param name="subCat">The device sub-category representing various types of products within a product family.</param>
        /// <param name="firmwareVersion">The firmware version running within the device.</param>
        public InsteonIdentity(byte devCat, byte subCat, byte firmwareVersion) : this()
        {
            DevCat = devCat;
            SubCat = subCat;
            FirmwareVersion = firmwareVersion;
            ProductKey = null;
        }

        public InsteonIdentity(byte devCat, byte subCat, byte firmwareVersion, InsteonProductKey productKey): this()
        {
            DevCat = devCat;
            SubCat = subCat;
            FirmwareVersion = firmwareVersion;
            ProductKey = productKey;
        }

        /// <summary>
        /// The device category representing a product family.
        /// </summary>
        public byte DevCat { get; private set; }

        /// <summary>
        /// The firmware version running within the device.
        /// </summary>
        public byte FirmwareVersion { get; private set; }

        public InsteonProductKey ProductKey { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this InsteonIdentity is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return DevCat == 0 && SubCat == 0 && FirmwareVersion == 0; }
        }

        /// <summary>
        /// The device sub-category representing various types of products within a product family.
        /// </summary>
        public byte SubCat { get; private set; }
    }
}