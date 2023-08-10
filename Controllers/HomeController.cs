using InteligentnyDomWebViewer.Model;
using Microsoft.AspNetCore.Mvc;
using SmartHomeTool.SmartHomeLibrary;

namespace InteligentnyDomWebViewer.Controllers
{
	public class HomeController : Controller
	{
		private readonly WebDbContext context;

		public HomeController(WebDbContext context)
		{
			this.context = context;
			Common.SetDateFormat();
		}

		public IActionResult Index()
		{
			IQueryable<DevicesCu> cus = context.DevicesCu;
			IQueryable<DevicesTemperatures> temperatures = context.DevicesTemperatures;
			IQueryable<DevicesRelays> relays = context.DevicesRelays;
			IQueryable<DevicesHeatings> heatings = context.DevicesHeatings;
			return View(new HomeViewModel
			{
				cus = cus,
				temperatures = temperatures,
				relays = relays,
				heatings = heatings,
				cusLastUpdated = cus.Min(h => h.LastUpdated),
				temperaturesLastUpdated = temperatures.Min(h => h.LastUpdated),
				relaysLastUpdated = relays.Min(h => h.LastUpdated),
				heatingsLastUpdated = heatings.Min(h => h.LastUpdated),
			});
		}
	}
}
