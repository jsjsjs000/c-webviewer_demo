using static InteligentnyDomWebViewer.Model.DeviceVersion;

namespace InteligentnyDomWebViewer.Model
{
	public class CentralUnitDeviceItem
	{
		public enum LineNumber
		{
			None = 0,
			UART1 = 1,
			UART2 = 2,
			UART3 = 3,
			UART4 = 4,
			Radio = 64,
			LAN = 65,
		}

		public uint address;
		public LineNumber lineNumber;
		public HardwareType1Enum hardwareType1;
		public HardwareType2Enum hardwareType2;
		public byte hardwareSegmentsCount;
		public byte hardwareVersion;
		public List<CentralUnitDeviceItem> devicesItems = new();
	}
}
