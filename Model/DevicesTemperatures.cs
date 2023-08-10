using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InteligentnyDomWebViewer.Model
{
	[Table("devices_temperatures")]
	public class DevicesTemperatures
	{
		public uint Address { get; set; }
		public string Name { get; set; } = string.Empty;
		public byte Segment { get; set; }
		public DateTime LastUpdated { get; set; }
		public float Temperature { get; set; }
		public bool Error { get; set; }
		public DateTime? ErrorFrom { get; set; }
		public uint Uptime { get; set; }
		public float Vin { get; set; }
	}
}
