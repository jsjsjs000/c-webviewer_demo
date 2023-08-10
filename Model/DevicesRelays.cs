using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InteligentnyDomWebViewer.Model
{
	[Table("devices_relays")]
	public class DevicesRelays
	{
		public uint Address { get; set; }
		public byte Segment { get; set; }
		public string Name { get; set; } = string.Empty;
		public DateTime LastUpdated { get; set; }
		public bool Relay { get; set; }
		public bool Error { get; set; }
		public DateTime? ErrorFrom { get; set; }
		public uint Uptime { get; set; }
		public float Vin { get; set; }
	}
}
