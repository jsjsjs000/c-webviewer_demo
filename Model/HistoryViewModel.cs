using Microsoft.AspNetCore.Mvc.Rendering;

namespace InteligentnyDomWebViewer.Model
{
	public class HistoryViewModel
	{
		public IEnumerable<SelectListItem>? Heatings { get; set; }
	}
}
