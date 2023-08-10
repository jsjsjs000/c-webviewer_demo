﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections.Generic;

namespace InteligentnyDomWebViewer.Model
{
	public class ModelCacheKeyFactory : IModelCacheKeyFactory
	{
		private static string GetKey() => DateTime.Now.ToString("yyyy-MM");

		public object Create(DbContext context, bool designTime)
		{
			return new
			{
				Type = context.GetType(),
				Schema = GetKey(),
			};
		}
	}

	public class WebDbContext : DbContext
	{
		public int m;
		public WebDbContext(DbContextOptions options) : base(options) { }

		public DbSet<Devices> Devices => Set<Devices>();
		public DbSet<DevicesCu> DevicesCu => Set<DevicesCu>();
		public DbSet<DevicesRelays> DevicesRelays => Set<DevicesRelays>();
		public DbSet<DevicesTemperatures> DevicesTemperatures => Set<DevicesTemperatures>();
		public DbSet<DevicesHeatings> DevicesHeatings => Set<DevicesHeatings>();
		public DbSet<DevicesHeatingsStructure> DevicesHeatingsStructure => Set<DevicesHeatingsStructure>();
		public DbSet<DevicesExternalThermometers> DevicesExternalThermometers => Set<DevicesExternalThermometers>();
		public DbSet<HistoryRelays> HistoryRelays => Set<HistoryRelays>();
		public DbSet<HistoryTemperatures> HistoryTemperatures => Set<HistoryTemperatures>();
		public DbSet<HistoryHeating> HistoryHeating => Set<HistoryHeating>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<DevicesRelays>().HasKey(dt => new { dt.Address, dt.Segment });
			modelBuilder.Entity<DevicesTemperatures>().HasKey(dt => new { dt.Address, dt.Segment });
			modelBuilder.Entity<DevicesHeatings>().HasKey(dt => new { dt.Address, dt.Segment });
			modelBuilder.Entity<DevicesHeatingsStructure>().HasKey(dt => new { dt.Address, dt.Segment, dt.Order });
			modelBuilder.Entity<DevicesExternalThermometers>().HasKey(dt => new { dt.Address, dt.Segment });

				/// EF Core Enum - https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions?tabs=data-annotations
			modelBuilder.Entity<DevicesHeatings>()
					.Property(e => e.Mode)
					.HasConversion(
							v => v.ToString(),
							v => (DevicesHeatings.HeatingMode)Enum.Parse(typeof(DevicesHeatings.HeatingMode), v));
		}
	}
}

/*
https://www.alwaysdeveloping.net/p/11-2020-dynamic-context
https://www.thinktecture.com/en/entity-framework-core/ef-core-user-defined-fields-and-tables/
https://stackoverflow.com/questions/31033055/dynamic-table-names-in-entity-framework-linq
https://stackoverflow.com/questions/31035238/dynamic-table-name-with-entity-framework
https://stackoverflow.com/questions/48041821/dynamically-access-table-in-ef-core-2-0
*/
