using System.ComponentModel.DataAnnotations.Schema;

namespace InteligentnyDomWebViewer.Model
{
	[Table("devices_heating")]
	public class DevicesHeatings
	{
		public enum HeatingMode { Off = 0, Auto = 1, Manual = 2 }

		public uint Address { get; set; }
		public byte Segment { get; set; }
		public string Name { get; set; } = string.Empty;
		public DateTime LastUpdated { get; set; }
		public HeatingMode Mode { get; set; }
		public float SettingTemperature { get; set; }
		public bool Relay { get; set; }
		public int Order { get; set; }

		public static string HeatingModeToString(HeatingMode heatingMode) => heatingMode switch
		{
			HeatingMode.Off => "Wyłączone",
			HeatingMode.Auto => "Auto",
			HeatingMode.Manual => "Manualne",
			_ => "",
		};
	}
}
