class History
{
	static Months = [ "styczeń", "luty", "marzec", "kwiecień", "maj", "czerwiec", "lipiec",
			"sierpień", "wrzesień", "październik", "listopad", "grudzień" ];
	static WeekDays = [ "nd", "pn", "wt", "śr", "cz", "pt", "sb" ];

	#chart;

	constructor()
	{
		document.querySelector(".button-prev-week").addEventListener("click",
				this.OnPrevWeekButtonClick.bind(this));
		document.querySelector(".button-prev-day").addEventListener("click",
				this.OnPrevDayButtonClick.bind(this));
		document.querySelector(".button-next-day").addEventListener("click",
				this.OnNextDayButtonClick.bind(this));
		document.querySelector(".button-next-week").addEventListener("click",
				this.OnNextWeekButtonClick.bind(this));

		document.querySelector("#input-date").addEventListener("change",
				this.DownloadData.bind(this));
		document.querySelector("#select-heating").addEventListener("change",
				this.DownloadData.bind(this));

		document.querySelector("#btnradio-day").addEventListener("click",
				this.DownloadData.bind(this));
		document.querySelector("#btnradio-week").addEventListener("click",
				this.DownloadData.bind(this));

		document.querySelector("#button-refresh").addEventListener("click",
				this.DownloadData.bind(this));

		document.querySelector("#checkbox-line").addEventListener("change",
				this.RefreshChart.bind(this));

		this.DrawChart();
		this.DownloadData();
	}

	DateToString(date)
	{
		return date.getFullYear().toString().padStart(4, "0") + "-" +
				((date.getMonth() + 1).toString().padStart(2, "0")) + "-" +
				date.getDate().toString().padStart(2, "0");
	}

	ChangeDate(addDays)
	{
		let dt = document.querySelector("#input-date").value;
		if (dt == "")
			return false;

		let date = new Date(dt);
		date.setTime(date.getTime() + addDays * 24 * 60 * 60 * 1000);
		document.querySelector("#input-date").value = this.DateToString(date);
		return true;
	}

	OnPrevWeekButtonClick()
	{
		if (this.ChangeDate(-7))
			this.DownloadData();
	}
	
	OnPrevDayButtonClick()
	{
		if (this.ChangeDate(-1))
			this.DownloadData();
	}

	OnNextDayButtonClick()
	{
		if (this.ChangeDate(1))
			this.DownloadData();
	}

	OnNextWeekButtonClick()
	{
		if (this.ChangeDate(7))
			this.DownloadData();
	}

	ShowAlert(show = true)
	{
		const toast = new bootstrap.Toast(document.querySelector("#error-toast"));
		if (show)
			toast.show();
		else
			toast.hide();
	}

	DownloadData()
	{
		this.ShowAlert(false);

		let url = location.href + "/getHistoryData";

		let dt = document.querySelector("#input-date").value;
		if (dt == "")
			return false;

		let period = "";
		if (document.querySelector("#btnradio-day").checked)
			period = "day";
		else if (document.querySelector("#btnradio-week").checked)
			period = "week";
		else
			return;

		let body =
		{
			Date: dt,
			Period: period,
			Heating: parseInt(document.querySelector("#select-heating").value),
		};

		const controller = new AbortController();
		setTimeout(() => controller.abort(), 8000);
		fetch(url,
		{
			method: "POST",
			headers: new Headers({ "Content-Type": "application/json" }),
			body: JSON.stringify(body),
			cache: "no-store",
			mode: "same-origin",
			redirect: "error",
			signal: controller.signal,
		})
		.then(response =>
		{
			if (response.ok && response.headers.get("Content-Type") != null &&
					response.headers.get("Content-Type").indexOf("application/json") >= 0)
				return response.json();
			else
				throw new Error(`Nieoczekiwany kod stanu ${response.status} lub błędny typ odpowiedzi`);
		})
		.then(document =>
		{
			this.UpdateChart(document);
		})
		.catch(error =>
		{
			console.error(error);
			this.ShowAlert();
		});
	}

	UpdateChart(data)
	{
		this.#chart.data.datasets[0].data = data.HistoryTemperaturesAir;
		this.#chart.data.datasets[1].data = data.HistoryTemperaturesFloor;
		this.#chart.data.datasets[2].data = data.HistoryHeating;
		this.#chart.data.datasets[3].data = data.HistoryRelays;
		this.#chart.data.datasets[4].data = data.HistoryExternalTemperatures;
		//this.#chart.scales.xAxis.min = new Date(data.MinDate); // $$ not works
		//this.#chart.scales.xAxis.max = new Date(data.MaxDate);
		this.RefreshChart();
	}

	RefreshChart()
	{
		for (let i = 0; i < 5; i++)
			this.#chart.data.datasets[i].showLine = document.querySelector("#checkbox-line").checked;
		this.#chart.update();
	}

	DrawChart(data)
	{
		const verticalLine = {
			id: 'verticalLine',
			beforeDraw(chart, args, options)
			{
				const {
					ctx,
					chartArea: { top, right, bottom, left, width, height },
					//scales: { x, y },
				} = chart;
				ctx.save();

					/// draw line on canvas
					/// https://itguytec.com/2021/12/14/chartjs-draw-horizontal-and-vertical-lines
					/// options.xPosition.getTime()
				ctx.strokeStyle = options.lineColor;
				const dayTicks = 24 * 60 * 60 * 1000;
				let x_ = chart.scales.xAxis.min + dayTicks;
				x_ = Math.floor(x_ / dayTicks) * dayTicks;
				x_ += new Date().getTimezoneOffset() * 60 * 1000;
				if (x_ <= chart.scales.xAxis.min)
					x_ += dayTicks;
				while (x_ < chart.scales.xAxis.max)
				{
					ctx.strokeRect(chart.scales.xAxis.getPixelForValue(x_), top, 0, height);
					x_ += dayTicks;
				}

				ctx.restore();
			}
		};

		const ctx = document.getElementById("chart");
		this.#chart = new Chart(ctx, {
			type: "line",
			data: {
				datasets: [{
					label: "Powietrze",
					data: [],
					borderWidth: 1,
					showLine: false,
					backgroudColor: "#FAB1C2",
					pointBackgroundColor: "#FAB1C2",
					borderColor: "#F66F91",
					//hoverBackgroundColor: "#68A3ED",
					//hoverBorderColor: "#68A3ED",
					//pointBorderColor: "#68A3ED",
					//pointHoverBackgroundColor: "#68A3ED",
					//pointHoverBorderColor: "#68A3ED",
				},
				{
					label: "Podłoga",
					data: [],
					borderWidth: 1,
					showLine: false,
					backgroudColor: "#ACCDF5",
					pointBackgroundColor: "#ACCDF5",
					borderColor: "#68A3ED",
				},
				{
					label: "Ustawiona temperatura lub wyłączone ogrzewanie",
					data: [],
					borderWidth: 1,
					showLine: false,
					backgroudColor: "#444444",
					pointBackgroundColor: "#444444",
					borderColor: "#333333",
				},
				{
					label: "Grzanie włączone/wyłączone",
					data: [],
					borderWidth: 1,
					showLine: false,
					backgroudColor: "#FAD1A1",
					pointBackgroundColor: "#FAD1A1",
					borderColor: "#F6AB53",
					//fill: { value: 0 },
				},
				{
					label: "Temperatura zewnętrzna",
					data: [],
					borderWidth: 1,
					showLine: false,
					backgroudColor: "#40ff00",
					pointBackgroundColor: "#40ff00",
					borderColor: "#40b000",
				}]
			},
			options: {
				locale: "pl-PL",
				responsive: true,
				//interaction: { // $$ źle wyświetla dla dziurawych danych - przesuwa w lewo
				//	intersect: false,
				//	mode: 'index',
				//	includeInvisible: true,
				//},
				animation: {
					duration: 333,
				},
				plugins: {
					legend: {
						position: "top",
						labels: {
							generateLabels: function(chart)
							{
								const labels = [];
								let i = 0;
								for (const ds of chart.data.datasets)
								{
										// https://www.chartjs.org/docs/latest/configuration/legend.html#legend-item-interface
									labels.push(
									{
										datasetIndex: i++,
										text: ds.label,
										fillStyle: ds.borderColor,
										fontColor: "black",
									});
								}
								return labels;
							}
						},
					},
					title: {
						display: false,
						text: "Wykresy temperatur i działania ogrzewania"
					},
					tooltip: {
						callbacks: {
							label: function(context)
							{
								if (context.datasetIndex == 0)
									return "Temperatura powietrza: " + (Math.round(context.parsed.y * 10) / 10) + " °C";
								if (context.datasetIndex == 1)
									return "Temperatura podłogi: " + (Math.round(context.parsed.y * 10) / 10) + " °C";
								if (context.datasetIndex == 2)
									return "Temperatura ustawiona: " + (Math.round(context.parsed.y * 10) / 10) + " °C";
								if (context.datasetIndex == 3)
								{
									if (context.parsed.y == 0)
										return "Grzanie: wyłączone";
									if (context.parsed.y == 1)
										return "Grzanie: WŁĄCZONE";
									return "Błąd";
								}
							},
							title: function(context)
							{
								let dt = new Date(context[0].raw.D);
								return dt.getFullYear() + "-" + (dt.getMonth() + 1).toString().padStart(2, "0") + "-" +
										dt.getDate().toString().padStart(2, "0") + " " + History.WeekDays[dt.getDay()] + " " +
										dt.getHours().toString() + ":" + dt.getMinutes().toString().padStart(2, "0") + ":";
							}
						}
					},
					verticalLine: {
						lineColor: "#ccc",
						//xPosition: new Date("2023-02-11T10:00:00"),
					}
				},
				parsing: {
					xAxisKey: "D",
					yAxisKey: "T"
				},
				scales: {
					xAxis: {
						type: "time",
						time: {
							unit: "hour",
							displayFormats: {
								"hour": "HH:00"
							}
						},
						//min: new Date((new Date()).getTime() - 7 * 24 * 60 * 60 * 1000),
						//max: new Date((new Date()).getTime() + 0 * 24 * 60 * 60 * 1000),
					},
					y: {
						//min: -1,
						//max: 30
					}
				}
			},
			plugins: [ verticalLine ],
		});

		this.#chart.hide(4);
		this.#chart.update();
	}
}
