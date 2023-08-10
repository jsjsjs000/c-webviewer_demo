namespace InteligentnyDomWebViewer.Model
{
	public class HistoryRelays
	{
		public int Id { get; set; }
		public DateTime Dt { get; set; }
		public uint Address { get; set; }
		public byte Segment { get; set; }
		public bool Relay { get; set; }
		public bool Error { get; set; }
		public float Vin { get; set; }
	}
}
