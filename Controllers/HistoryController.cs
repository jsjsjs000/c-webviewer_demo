using InteligentnyDomWebViewer.Model;
using Microsoft.AspNetCore.Mvc;
using System.Buffers;
using System.Text.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartHomeTool.SmartHomeLibrary;

namespace InteligentnyDomWebViewer.Controllers
{
	public class HistoryController : Controller
	{
		private readonly WebDbContext context;

		public HistoryController(WebDbContext context)
		{
			Common.SetDateFormat();
			this.context = context;
		}

		public IActionResult Index()
		{
			var heatings = context.DevicesHeatings.
					OrderBy(h => h.Order).
					Select(h => new SelectListItem { 
						Text = h.Name, Value = (((long)h.Address << 8) | h.Segment).ToString() }).
					AsEnumerable().
					Union(context.DevicesExternalThermometers.
						OrderBy(h => h.Order).
						Select(h => new SelectListItem {
							Text = h.Name, Value = ((long)1 << 40 | ((long)h.Address << 8) | h.Segment).ToString() })); /// 1 << 40 External temperature

			HistoryViewModel history = new()
			{
				Heatings = heatings,
			};
			return View(history);
		}

		public class GetHistoryDataParams
		{
			public DateTime Date { get; set; }
			public string Period { get; set; } = string.Empty;
			public long Heating { get; set; }
		}

		[HttpPost]
		public IActionResult GetHistoryData([FromBody] GetHistoryDataParams parameters)
		{
			DateTime minDate = parameters.Date.Date;
			DateTime maxDate;
			int minuteDivider;
			if (parameters.Period == "week")
			{
				maxDate = parameters.Date.Date.AddDays(7);
				minuteDivider = 15;
			}
			else
			{
				maxDate = parameters.Date.Date.AddDays(1);
				minuteDivider = 5;
			}
			byte type = (byte)(parameters.Heating >> 40);
			uint relayAddress = (uint)((parameters.Heating >> 8) & 0xffffffff);
			byte relaySegment = (byte)(parameters.Heating & 0xff);

			if (type == 0) /// Heating
			{
				var heatingStructure = context.DevicesHeatingsStructure.
						Where(h => h.ParentAddress == relayAddress && h.ParentSegment == relaySegment).
						OrderBy(h => h.Order);
				if (heatingStructure.Count() == 0)
					return BadRequest();

				uint addressAir = 0;
				uint segmentAir = 0;
				uint addressFloor = 0;
				uint segmentFloor = 1;
				foreach (DevicesHeatingsStructure hs in heatingStructure)
					if (hs.Order == 0)
					{
						addressAir = hs.Address;
						segmentAir = hs.Segment;
					}
					else if (hs.Order == 1)
					{
						addressFloor = hs.Address;
						segmentFloor = hs.Segment;
					}

				IQueryable<HistoryTemperatureViewModel> historyTemperaturesAir = context.Set<HistoryTemperatures>().
						FromSqlRaw($"SELECT Id, Dt, Address, Segment, Temperature, Error, Vin FROM history_temperatures_{minDate.Year}_{minDate.Month:d2}").
						AsNoTracking().
						Where(h => h.Address == addressAir && h.Segment == segmentAir).
						Where(h => h.Dt >= minDate && h.Dt < maxDate).
						Where(h => h.Dt.Minute % minuteDivider == 0).
						Where(h => !h.Error && h.Temperature < 100).  // $$ 4082,19 - 4095,88
						Select(h => new HistoryTemperatureViewModel()
						{
							D = h.Dt,
							T = h.Temperature,
						});
				IQueryable<HistoryTemperatureViewModel> historyTemperaturesFloor = context.Set<HistoryTemperatures>().
						FromSqlRaw($"SELECT Id, Dt, Address, Segment, Temperature, Error, Vin FROM history_temperatures_{minDate.Year}_{minDate.Month:d2}").
						AsNoTracking().
						Where(h => h.Address == addressFloor && h.Segment == segmentFloor).
						Where(h => h.Dt >= minDate && h.Dt < maxDate).
						Where(h => h.Dt.Minute % minuteDivider == 0).
						Where(h => !h.Error && h.Temperature < 100).  // $$ 4082,19 - 4095,88
						Select(h => new HistoryTemperatureViewModel()
						{
							D = h.Dt,
							T = h.Temperature,
						});

				var historyTemperaturesMinMax = context.Set<HistoryTemperatures>().
						FromSqlRaw($"SELECT Id, Dt, Address, Segment, Temperature, Error, Vin FROM history_temperatures_{minDate.Year}_{minDate.Month:d2}").
						AsNoTracking().
						Where(h => (h.Address == addressAir && h.Segment == segmentAir) || (h.Address == addressFloor && h.Segment == segmentFloor)).
						Where(h => h.Dt >= minDate && h.Dt < maxDate).
						Where(h => h.Dt.Minute % minuteDivider == 0).
						Where(h => !h.Error && h.Temperature < 100).  // $$ 4082,19 - 4095,88
						GroupBy(_ => 1, (_, r) => new
						{
							Min = r.Min(r => r.Temperature),
							Max = r.Max(r => r.Temperature),
						});
				float min = 0;
				float max = 0;
				var historyTemperaturesMinMax_ = historyTemperaturesMinMax.SingleOrDefault();
				if (historyTemperaturesMinMax_ != null)
				{
					min = historyTemperaturesMinMax_.Min;
					max = historyTemperaturesMinMax_.Max;
				}

				IQueryable<HistoryTemperatureViewModel> historyRelays = context.Set<HistoryRelays>().
						FromSqlRaw($"SELECT Id, Dt, Address, Segment, Relay, Error, Vin FROM history_relays_{minDate.Year}_{minDate.Month:d2}").
						AsNoTracking().
						Where(h => h.Address == relayAddress && h.Segment == relaySegment).
						Where(h => h.Dt >= minDate && h.Dt < maxDate).
						Where(h => h.Dt.Minute % minuteDivider == 0).
						Where(h => !h.Error).
						Select(h => new HistoryTemperatureViewModel()
						{
							D = h.Dt,
							T = h.Relay ? 1 : 0,
						});

				IQueryable<HistoryTemperatureViewModel> historyHeating = context.Set<HistoryHeating>().
						FromSqlRaw($"SELECT Id, Dt, Address, Segment, Mode, SettingTemperature, Relay FROM history_heating_{minDate.Year}_{minDate.Month:d2}").
						AsNoTracking().
						Where(h => h.Address == relayAddress && h.Segment == relaySegment).
						Where(h => h.Dt >= minDate && h.Dt < maxDate).
						Where(h => h.Dt.Minute % minuteDivider == 0).
						Where(h => h.Mode > 1).
						Select(h => new HistoryTemperatureViewModel()
						{
							D = h.Dt,
							T = h.SettingTemperature,
						});

				IQueryable<HistoryTemperatureViewModel> HistoryExternalTemperatures = context.Set<HistoryTemperatures>().
						FromSqlRaw($"SELECT Id, Dt, Address, Segment, Temperature, Error, Vin FROM history_temperatures_{minDate.Year}_{minDate.Month:d2}").
						AsNoTracking().
						Where(h => h.Address == 0x86246f30 && h.Segment == 0). // $$ external temperature
						Where(h => h.Dt >= minDate && h.Dt < maxDate).
						Where(h => h.Dt.Minute % minuteDivider == 0).
						Where(h => !h.Error && h.Temperature < 100).  // $$ 4082,19 - 4095,88
						Select(h => new HistoryTemperatureViewModel()
						{
							D = h.Dt,
							T = h.Temperature,
						});

				HttpContext.Response.Headers.ContentType = "application/json";
				var jsonSerializerOptions = new JsonSerializerOptions
				{
					ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
				};
				var data = new
				{
					HistoryTemperaturesAir = historyTemperaturesAir,
					HistoryTemperaturesFloor = historyTemperaturesFloor,
					HistoryExternalTemperatures = HistoryExternalTemperatures,
					MinTemperature = min,
					MaxTemperature = max,
					HistoryRelays = historyRelays,
					HistoryHeating = historyHeating,
					Type = "Heating",
					MinDate = minDate,
					MaxDate = maxDate,
				};
				string json = JsonSerializer.Serialize(data, jsonSerializerOptions);
				HttpContext.Response.BodyWriter.Write(UTF8Encoding.UTF8.GetBytes(json));
			}
			else if (type == 1) /// External temperature
			{
				IQueryable<HistoryTemperatureViewModel> HistoryTemperaturesAir = context.Set<HistoryTemperatures>().
						FromSqlRaw($"SELECT Id, Dt, Address, Segment, Temperature, Error, Vin FROM history_temperatures_{minDate.Year}_{minDate.Month:d2}").
						AsNoTracking().
						Where(h => h.Address == relayAddress && h.Segment == relaySegment).
						Where(h => h.Dt >= minDate && h.Dt < maxDate).
						Where(h => h.Dt.Minute % minuteDivider == 0).
						Where(h => !h.Error && h.Temperature < 100).  // $$ 4082,19 - 4095,88
						Select(h => new HistoryTemperatureViewModel()
						{
							D = h.Dt,
							T = h.Temperature,
						});

				HttpContext.Response.Headers.ContentType = "application/json";
				var jsonSerializerOptions = new JsonSerializerOptions
				{
					ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
				};
				var data = new
				{
					HistoryTemperaturesAir = HistoryTemperaturesAir,
					Type = "ExternalTemperature",
					MinDate = minDate,
					MaxDate = maxDate,
				};
				string json = JsonSerializer.Serialize(data, jsonSerializerOptions);
				HttpContext.Response.BodyWriter.Write(UTF8Encoding.UTF8.GetBytes(json));
			}

			return Ok();
		}
	}
}
