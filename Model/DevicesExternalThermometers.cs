using System.ComponentModel.DataAnnotations.Schema;

namespace InteligentnyDomWebViewer.Model
{
	[Table("devices_external_thermometers")]
	public class DevicesExternalThermometers
	{
		public uint Address { get; set; }
		public byte Segment { get; set; }
		public string Name { get; set; } = string.Empty;
		public int Order { get; set; }
	}
}
