using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace InteligentnyDomWebViewer.Model
{
	[Table("devices")]
	public class Devices
	{
		[Required, Key]
		public uint Address { get; set; }
		public CentralUnitDeviceItem.LineNumber LineNumber { get; set; }
		public DeviceVersion.HardwareType1Enum HardwareType1 { get; set; }
		public DeviceVersion.HardwareType2Enum HardwareType2 { get; set; }
		public byte HardwareSegmentsCount { get; set; }
		public byte HardwareVersion { get; set; }
		//public DbSet<Devices> ParentItem { get; set; }
		public bool Active { get; set; }
	}
}
