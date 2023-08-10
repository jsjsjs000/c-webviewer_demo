namespace InteligentnyDomWebViewer.Model
{
	public class HistoryTemperatures
	{
		public int Id { get; set; }
		public DateTime Dt { get; set; }
		public uint Address { get; set; }
		public byte Segment { get; set; }
		public float Temperature { get; set; }
		public bool Error { get; set; }
		public float Vin { get; set; }
	}
}
