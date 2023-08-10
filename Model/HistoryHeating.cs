namespace InteligentnyDomWebViewer.Model
{
	public class HistoryHeating
	{
		public int Id { get; set; }
		public DateTime Dt { get; set; }
		public uint Address { get; set; }
		public byte Segment { get; set; }
		public byte Mode { get; set; }
		public float SettingTemperature { get; set; }
		public bool Relay { get; set; }
	}
}
