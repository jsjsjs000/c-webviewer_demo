using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InteligentnyDomWebViewer.Model
{
	[Table("devices_cu")]
	public class DevicesCu
	{
		[Required, Key]
		public uint Address { get; set; }
		public string Name { get; set; } = string.Empty;
		public DateTime LastUpdated { get; set; }
		public bool Error { get; set; }
		public DateTime? ErrorFrom { get; set; }
		public uint Uptime { get; set; }
		public float Vin { get; set; }
	}
}
