using InteligentnyDomRelay;
using InteligentnyDomWebViewer.Model;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using SmartHomeTool.SmartHomeLibrary;
using System.Globalization;

Common.SetDateFormat();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

string mySqlConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
builder.Services.AddDbContext<WebDbContext>(opts => {
	opts.UseMySql(mySqlConnectionString, ServerVersion.AutoDetect(mySqlConnectionString));
	opts.EnableSensitiveDataLogging();
});

builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
	/// prevent JSON Serializer camelCase output
builder.Services.AddControllersWithViews().AddJsonOptions(
	opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
	app.UseStatusCodePages();
}

CommunicationService communicationService = new(app.Services);

Console.CancelKeyPress += (object? sender, ConsoleCancelEventArgs e) =>
{
	communicationService.ExitThread = true;
};

app.UseStaticFiles();

app.MapControllerRoute("Home", "{action=Index}", new { Controller = "Home" });
app.MapControllerRoute("History", "{controller=History}/{action=Index}/{id?}");

var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<WebDbContext>();

app.Run();

/*
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
# dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Pomelo.EntityFrameworkCore.MySql
dotnet add package SharpZipLib
dotnet add package System.Management
dotnet add package System.IO.Ports

dotnet tool install --global dotnet-ef
dotnet tool install --global Microsoft.Web.LibraryManager.Cli

libman init -p cdnjs # https://cdnjs.com
libman install bootstrap -d wwwroot/lib/bootstrap
libman install Chart.js -d wwwroot/lib/chart.js
libman install moment.js -d wwwroot/lib/moment.js
libman install chartjs-adapter-moment -d wwwroot/lib/chartjs-adapter-moment

dotnet ef migrations add Initial
dotnet ef database update

dotnet ef database drop --force
del Migrations\*; dotnet ef database drop --force; dotnet ef migrations add Initial; dotnet ef database update

https://www.daveops.co.in/post/code-first-entity-framework-core-mysql
*/
