namespace InteligentnyDomWebViewer.Model
{
	public class HomeViewModel
	{
		public IQueryable<DevicesCu>? cus { get; set; }
		public IQueryable<DevicesTemperatures>? temperatures { get; set; }
		public IQueryable<DevicesRelays>? relays { get; set; }
		public IQueryable<DevicesHeatings>? heatings { get; set; }
		public DateTime? cusLastUpdated { get; set; }
		public DateTime? temperaturesLastUpdated { get; set; }
		public DateTime? relaysLastUpdated { get; set; }
		public DateTime? heatingsLastUpdated { get; set; }
	}
}
