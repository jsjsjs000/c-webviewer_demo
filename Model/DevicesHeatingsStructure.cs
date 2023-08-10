using System.ComponentModel.DataAnnotations.Schema;

namespace InteligentnyDomWebViewer.Model
{
	[Table("devices_heating_structure")]
	public class DevicesHeatingsStructure
	{
		public uint ParentAddress { get; set; }
		public byte ParentSegment { get; set; }
		public uint Address { get; set; }
		public byte Segment { get; set; }
		public int Order { get; set; }
	}
}
