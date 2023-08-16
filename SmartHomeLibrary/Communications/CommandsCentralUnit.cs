using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHomeTool.SmartHomeLibrary
{
	public partial class Commands
	{
		public class Devices
		{
			public uint Address { get; set; }
			public CentralUnitDeviceItem.LineNumber LineNumber { get; set; }
			public DeviceVersion.HardwareType1Enum HardwareType1 { get; set; }
			public DeviceVersion.HardwareType2Enum HardwareType2 { get; set; }
			public byte HardwareSegmentsCount { get; set; }
			public byte HardwareVersion { get; set; }
			//public DbSet<Devices> ParentItem { get; set; }
			public bool Active { get; set; }
		}

		public abstract class VisualComponent
		{
			public string Name = string.Empty;
			public List<VisualComponent> VisualSubItems = new();
		}

		public class GroupVisualComponent : VisualComponent { }

		public class HeatingVisualComponent : VisualComponent
		{
			public Devices DeviceItem;
			public byte DeviceSegment;
			public List<HeatingVisualComponentSubItem> SubItems = new();
			public HeatingVisualComponentControl Control = new();

			public byte[] GetBytes()
			{
				bool ok = true;
				int j = 0;
				byte[] bytes = new byte[512];
				bytes[j++] = (byte)'g';
				bytes[j++] = (byte)'C';
				bytes[j++] = (byte)'O';
				bytes[j++] = (byte)'M';
				bytes[j++] = (byte)'P';
				bytes[j++] = Convert.ToByte(!ok);
				if (ok)
				{
					bytes[j++] = (byte)Control.HeatingMode;
					bytes[j++] = (byte)Control.PeriodsPnPtCount;
					bytes[j++] = (byte)Control.PeriodsSaCount;
					bytes[j++] = (byte)Control.PeriodsSuCount;
					for (int i = 0; i < 4; i++)
						bytes[j++] = TimeSpanToByte(Control.PeriodPnPtFrom[i]);
					for (int i = 0; i < 4; i++)
						bytes[j++] = TimeSpanToByte(Control.PeriodSaFrom[i]);
					for (int i = 0; i < 4; i++)
						bytes[j++] = TimeSpanToByte(Control.PeriodSuFrom[i]);
					bytes[j++] = Common.Uint32_1Byte((uint)Control.ManualTemperature * 10);
					bytes[j++] = Common.Uint32_0Byte((uint)Control.ManualTemperature * 10);
					for (int i = 0; i < 4; i++)
					{
						bytes[j++] = Common.Uint32_1Byte((uint)Control.PeriodPnPtTemperature[i] * 10);
						bytes[j++] = Common.Uint32_0Byte((uint)Control.PeriodPnPtTemperature[i] * 10);
					}
					for (int i = 0; i < 4; i++)
					{
						bytes[j++] = Common.Uint32_1Byte((uint)Control.PeriodSaTemperature[i] * 10);
						bytes[j++] = Common.Uint32_0Byte((uint)Control.PeriodSaTemperature[i] * 10);
					}
					for (int i = 0; i < 4; i++)
					{
						bytes[j++] = Common.Uint32_1Byte((uint)Control.PeriodSuTemperature[i] * 10);
						bytes[j++] = Common.Uint32_0Byte((uint)Control.PeriodSuTemperature[i] * 10);
					}
					bytes[j++] = Common.Uint32_1Byte((uint)Control.MaxTemperature * 10);
					bytes[j++] = Common.Uint32_0Byte((uint)Control.MaxTemperature * 10);
					bytes[j++] = Common.Uint32_1Byte((uint)Control.HysteresisTemperature * 100);
					bytes[j++] = Common.Uint32_0Byte((uint)Control.HysteresisTemperature * 100);
				}
				return bytes[0..j];
			}

			static byte TimeSpanToByte(TimeSpan ts)
			{
				return (byte)(ts.Hours * 10 + ts.Minutes / 10);
			}

			static TimeSpan ByteToTimeSpan(byte b)
			{
				return new TimeSpan(b / 10, b - (int)(b / 10) * 10, 0);
			}
		}

		public class HeatingVisualComponentSubItem
		{
			public string Name = string.Empty;
			public Devices DeviceItem;
			public byte DeviceSegment;
		}

		public class HeatingVisualComponentControl
		{
			public enum Mode { Off, Auto, Manual };
			public Mode HeatingMode;
			public byte PeriodsPnPtCount = 2;
			public byte PeriodsSaCount = 1;
			public byte PeriodsSuCount = 1;
			public TimeSpan[] PeriodPnPtFrom = new TimeSpan[4];
			public TimeSpan[] PeriodSaFrom = new TimeSpan[4];
			public TimeSpan[] PeriodSuFrom = new TimeSpan[4];
			public float[] PeriodPnPtTemperature = new float[4];
			public float[] PeriodSaTemperature = new float[4];
			public float[] PeriodSuTemperature = new float[4];
			public float ManualTemperature = 21;
			public float MaxTemperature = 30;
			public float HysteresisTemperature = 0.5f;

			public HeatingVisualComponentControl()
			{
				PeriodPnPtFrom[0] = new TimeSpan(6, 0, 0);
				PeriodPnPtFrom[1] = new TimeSpan(13, 0, 0);
				PeriodPnPtFrom[2] = new TimeSpan(15, 0, 0);
				PeriodPnPtFrom[3] = new TimeSpan(22, 0, 0);
				PeriodSaFrom[0] = new TimeSpan(6, 0, 0);
				PeriodSaFrom[1] = new TimeSpan(13, 0, 0);
				PeriodSaFrom[2] = new TimeSpan(15, 0, 0);
				PeriodSaFrom[3] = new TimeSpan(22, 0, 0);
				PeriodSuFrom[0] = new TimeSpan(6, 0, 0);
				PeriodSuFrom[1] = new TimeSpan(13, 0, 0);
				PeriodSuFrom[2] = new TimeSpan(15, 0, 0);
				PeriodSuFrom[3] = new TimeSpan(22, 0, 0);
				PeriodPnPtTemperature[0] = 18;
				PeriodPnPtTemperature[1] = 21;
				PeriodPnPtTemperature[2] = 18;
				PeriodPnPtTemperature[3] = 21;
				PeriodSaTemperature[0] = 18;
				PeriodSaTemperature[1] = 21;
				PeriodSaTemperature[2] = 18;
				PeriodSaTemperature[3] = 21;
				PeriodSuTemperature[0] = 18;
				PeriodSuTemperature[1] = 21;
				PeriodSuTemperature[2] = 18;
				PeriodSuTemperature[3] = 21;
			}

			public static string ModeToString(Mode mode) => mode switch
			{
				Mode.Off => "Wyłączone",
				Mode.Auto => "Auto",
				Mode.Manual => "Manualne",
				_ => "",
			};
		}

		public class CentralUnitStatus
		{
			enum StatusType { Temperature = 0x40, Relay = 0x41, HeatingVisualComponent = 0x50 }

			public uint address;
			public bool initialized;
			public bool error;
			public uint uptime;
			public float vin;

			public static bool ParseFromBytes(byte[] bytes, out List<CentralUnitStatus> status,
					out List<HeatingVisualComponent> heatingVisualComponents,
					bool detail, out int itemsCount, out uint uptime, out float vin)
			{
				itemsCount = 0;
				uptime = uint.MaxValue;
				vin = 0;
				int j = 0;
				status = new List<CentralUnitStatus>();
				heatingVisualComponents = new List<HeatingVisualComponent>();
				List<CentralUnitStatus> statusTmp = new List<CentralUnitStatus>();
				if (bytes.Length < 1 + 2 + (detail ? 4 + 2 : 0))
					return false;

				int itemsReceived = bytes[j++];
				itemsCount = (bytes[j++] << 8) | bytes[j++];

				if (detail)
				{
					uptime = (uint)((bytes[j++] << 24) | (bytes[j++] << 16) | (bytes[j++] << 8) | bytes[j++]);
					vin = ((bytes[j++] << 8) | bytes[j++]) / 1000f;
				}

				for (int i = 0; i < itemsReceived; i++)
				{
					if (j + 5 >= bytes.Length)
						return false;

					StatusType type = (StatusType)bytes[j++];
					uint address = (uint)((bytes[j++] << 24) | (bytes[j++] << 16) | (bytes[j++] << 8) | bytes[j++]);
					if (type == StatusType.Temperature || type == StatusType.Relay)
					{
						if (j + 1 + 1 + (detail ? 4 + 2 + 1 : 0) >= bytes.Length)
							return false;

						bool initialized = Convert.ToBoolean(bytes[j++]);
						bool error = Convert.ToBoolean(bytes[j++]);
						uint uptime_ = 0;
						float vin_ = 0;
						byte communicationPercent_;
						if (detail)
						{
							uptime_ = (uint)((bytes[j++] << 24) | (bytes[j++] << 16) | (bytes[j++] << 8) | bytes[j++]);
							vin_ = ((bytes[j++] << 8) | bytes[j++]) / 1000f;
							communicationPercent_ = bytes[j++];
						}
						 
						if (j + 1 >= bytes.Length)
							return false;
						byte segmentsCount = bytes[j++];

						CentralUnitStatus statusItem;
						if (type == StatusType.Temperature)
						{
							if (!TemperatureStatus.ParseFromBytes(bytes[j..(j + segmentsCount * TemperatureStatus.BytesCount)],
									segmentsCount, out statusItem))
								return false;
							j += segmentsCount * TemperatureStatus.BytesCount;
						}
						else if (type == StatusType.Relay)
						{
							if (!RelayStatus.ParseFromBytes(bytes[j..(j + segmentsCount * RelayStatus.BytesCount)],
									segmentsCount, out statusItem))
								return false;
							j += segmentsCount * RelayStatus.BytesCount;
						}
						else
							return false;

						statusItem.address = address;
						statusItem.initialized = initialized;
						statusItem.error = error;
						statusItem.uptime = uptime_;
						statusItem.vin = vin_;
						statusTmp.Add(statusItem);
					}
					else if (type == StatusType.HeatingVisualComponent)
					{
						byte segment = bytes[j++];
						HeatingVisualComponentControl.Mode mode = (HeatingVisualComponentControl.Mode)bytes[j++];
						float temperature = ((bytes[j++] << 8) | bytes[j++]) / 16f;

						HeatingVisualComponent heatingVisualComponent = new HeatingVisualComponent()
						{
							DeviceItem = new Devices()
							{
								Address = address,
							},
							DeviceSegment = segment,
							Control = new HeatingVisualComponentControl()
							{
								HeatingMode = mode,
							},
						};
						if (heatingVisualComponent.Control.HeatingMode == HeatingVisualComponentControl.Mode.Auto)
							heatingVisualComponent.Control.ManualTemperature = temperature; // $$
						else if (heatingVisualComponent.Control.HeatingMode == HeatingVisualComponentControl.Mode.Manual)
							heatingVisualComponent.Control.ManualTemperature = temperature;
						heatingVisualComponents.Add(heatingVisualComponent);
					}
					else
						return false;
				}
				if (j != bytes.Length)
					return false;

				status = statusTmp;
				return true;
			}

			public static byte[] GetBytes(List<CentralUnitStatus> statuses, List<HeatingVisualComponent> heatingVisualComponents,
					uint cuUptime, float cuVin, byte details)
			{
				int j = 0;
				byte[] bytes = new byte[2048];
				bytes[j++] = (byte)'g';
				bytes[j++] = (byte)(statuses.Count + heatingVisualComponents.Count);
				bytes[j++] = Common.Uint32_1Byte((uint)(statuses.Count + heatingVisualComponents.Count));
				bytes[j++] = Common.Uint32_0Byte((uint)(statuses.Count + heatingVisualComponents.Count));

				if (details >= 1)
				{
					bytes[j++] = Common.Uint32_3Byte(cuUptime);
					bytes[j++] = Common.Uint32_2Byte(cuUptime);
					bytes[j++] = Common.Uint32_1Byte(cuUptime);
					bytes[j++] = Common.Uint32_0Byte(cuUptime);
					bytes[j++] = Common.Uint32_1Byte((uint)(cuVin * 1000));
					bytes[j++] = Common.Uint32_0Byte((uint)(cuVin * 1000));
				}

				foreach (CentralUnitStatus status in statuses)
				{
					if (status is TemperatureStatus)
						bytes[j++] = (byte)StatusType.Temperature;
					else if (status is RelayStatus)
						bytes[j++] = (byte)StatusType.Relay;
					else
						bytes[j++] = 0;

					bytes[j++] = Common.Uint32_3Byte(status.address);
					bytes[j++] = Common.Uint32_2Byte(status.address);
					bytes[j++] = Common.Uint32_1Byte(status.address);
					bytes[j++] = Common.Uint32_0Byte(status.address);

					if (status is TemperatureStatus statusTemperature)
					{
						bytes[j++] = Convert.ToByte(statusTemperature.initialized);
						bytes[j++] = Convert.ToByte(statusTemperature.error);
						if (details >= 1)
						{
							bytes[j++] = Common.Uint32_3Byte(statusTemperature.uptime);
							bytes[j++] = Common.Uint32_2Byte(statusTemperature.uptime);
							bytes[j++] = Common.Uint32_1Byte(statusTemperature.uptime);
							bytes[j++] = Common.Uint32_0Byte(statusTemperature.uptime);
							bytes[j++] = Common.Uint32_1Byte((uint)(statusTemperature.vin * 1000));
							bytes[j++] = Common.Uint32_0Byte((uint)(statusTemperature.vin * 1000));
							bytes[j++] = 0; // $$ communication percent
						}
						bytes[j++] = (byte)statusTemperature.temperatures!.Length;
						for (int i = 0; i < statusTemperature.temperatures.Length; i++)
						{
							float temperature = (statusTemperature.temperatures[i] == 0x7fff) ?
									statusTemperature.temperatures[i] : statusTemperature.temperatures[i] * 16f;
							bytes[j++] = Common.Uint32_1Byte((ushort)temperature);
							bytes[j++] = Common.Uint32_0Byte((ushort)temperature);
						}
					}
					else if (status is RelayStatus statusRelay)
					{
						bytes[j++] = Convert.ToByte(statusRelay.initialized);
						bytes[j++] = Convert.ToByte(statusRelay.error);
						if (details >= 1)
						{
							bytes[j++] = Common.Uint32_3Byte(statusRelay.uptime);
							bytes[j++] = Common.Uint32_2Byte(statusRelay.uptime);
							bytes[j++] = Common.Uint32_1Byte(statusRelay.uptime);
							bytes[j++] = Common.Uint32_0Byte(statusRelay.uptime);
							bytes[j++] = Common.Uint32_1Byte((uint)(statusRelay.vin * 1000));
							bytes[j++] = Common.Uint32_0Byte((uint)(statusRelay.vin * 1000));
							bytes[j++] = 0; // $$ communication percent
						}
						bytes[j++] = (byte)statusRelay.relaysStates!.Length;
						for (int i = 0; i < statusRelay.relaysStates.Length; i++)
							bytes[j++] = Convert.ToByte(statusRelay.relaysStates[i]);
					}
				}

				foreach (HeatingVisualComponent heatingVisualComponent in heatingVisualComponents)
				{
					bytes[j++] = (byte)StatusType.HeatingVisualComponent;
					bytes[j++] = Common.Uint32_3Byte(heatingVisualComponent.DeviceItem.Address);
					bytes[j++] = Common.Uint32_2Byte(heatingVisualComponent.DeviceItem.Address);
					bytes[j++] = Common.Uint32_1Byte(heatingVisualComponent.DeviceItem.Address);
					bytes[j++] = Common.Uint32_0Byte(heatingVisualComponent.DeviceItem.Address);
					bytes[j++] = heatingVisualComponent.DeviceSegment;
					bytes[j++] = (byte)heatingVisualComponent.Control.HeatingMode;

					float setTemperature = 31f;
					bytes[j++] = Common.Uint32_1Byte((uint)(setTemperature * 16f));
					bytes[j++] = Common.Uint32_0Byte((uint)(setTemperature * 16f));
				}

				return bytes[0..j];
			}
		}

		public class RelayStatus : CentralUnitStatus
		{
			public const int BytesCount = 1;
			public bool[]? relaysStates;

			public static bool ParseFromBytes(byte[] bytes, int segmentsCount, out CentralUnitStatus relaysStatus)
			{
				int j = 0;
				relaysStatus = new();
				RelayStatus relaysStatus_ = new RelayStatus();
				if (segmentsCount * BytesCount != bytes.Length)
					return false;

				relaysStatus_.relaysStates = new bool[segmentsCount];
				for (int i = 0; i < segmentsCount; i++)
					relaysStatus_.relaysStates[i] = Convert.ToBoolean(bytes[j++]);

				relaysStatus = relaysStatus_;
				return true;
			}
		}

		public class TemperatureStatus : CentralUnitStatus
		{
			public const int BytesCount = 2;
			public float[]? temperatures;

			public static bool ParseFromBytes(byte[] bytes, int segmentsCount, out CentralUnitStatus temperatureStatus)
			{
				int j = 0;
				temperatureStatus = new();
				TemperatureStatus temperatureStatus_ = new();
				if (segmentsCount * BytesCount != bytes.Length)
					return false;

				temperatureStatus_.temperatures = new float[segmentsCount];
				for (int i = 0; i < segmentsCount; i++)
				{
					temperatureStatus_.temperatures[i] = (short)((bytes[j++] << 8) | bytes[j++]);
					if (temperatureStatus_.temperatures[i] != 0x7fff)
						temperatureStatus_.temperatures[i] /= 16f;
				}

				temperatureStatus = temperatureStatus_;
				return true;
			}
		}

		//public bool SendGetCentralUnitStatus(uint packetId, uint encryptionKey, uint address,
		//		out List<CentralUnitStatus> status, out List<HeatingVisualComponent> heatingVisualComponents,
		//		int fromItem, bool detail, out int itemsCount, out uint cuUptime, out float cuVin)
		//{
		//	status = new List<CentralUnitStatus>();
		//	heatingVisualComponents = new List<HeatingVisualComponent>();
		//	itemsCount = 0;
		//	cuUptime = uint.MaxValue;
		//	cuVin = 0;
		//	byte[] data = new byte[] { (byte)'g', (byte)(fromItem >> 8), (byte)(fromItem & 0xff), (byte)(detail ? 1 : 0) };
		//	if (!com.SendPacket(packetId, encryptionKey, address, data, out uint outPacketId, out uint _, out uint outAddress, out byte[] dataOut))
		//		return false;

		//	bool ok = dataOut.Length >= 1 && dataOut[0] == data[0] && address == outAddress && packetId == outPacketId;
		//	if (ok)
		//		ok = CentralUnitStatus.ParseFromBytes(dataOut[1..], out status, out heatingVisualComponents,
		//				detail, out itemsCount, out cuUptime, out cuVin);
		//	return ok;
		//}

		//public bool SendSetRelay(uint packetId, uint encryptionKey, uint address, uint relayAddress, byte segment, bool set)
		//{
		//	byte[] data = new byte[] {
		//			(byte)'s', (byte)'R', (byte)'E', (byte)'L', Common.Uint32_3Byte(relayAddress),
		//			Common.Uint32_2Byte(relayAddress), Common.Uint32_1Byte(relayAddress), Common.Uint32_0Byte(relayAddress),
		//			segment, (byte)(set ? 1 : 0) };
		//	if (!com.SendPacket(packetId, encryptionKey, address, data, out uint outPacketId, out uint _, out uint outAddress, out byte[] dataOut))
		//		return false;

		//	return dataOut.Length >= 5 && dataOut[0] == data[0] && dataOut[1] == data[1] && dataOut[2] == data[2] && dataOut[3] == data[3] &&
		//			address == outAddress && packetId == outPacketId && dataOut[4] == 0;
		//}

		//public bool SendGetRelay(uint packetId, uint encryptionKey, uint address, uint relayAddress, byte segment, out bool? set)
		//{
		//	set = null;
		//	byte[] data = new byte[] {
		//			(byte)'g', (byte)'R', (byte)'E', (byte)'L', Common.Uint32_3Byte(relayAddress),
		//			Common.Uint32_2Byte(relayAddress), Common.Uint32_1Byte(relayAddress), Common.Uint32_0Byte(relayAddress), segment };
		//	if (!com.SendPacket(packetId, encryptionKey, address, data, out uint outPacketId, out uint _, out uint outAddress, out byte[] dataOut))
		//		return false;

		//	if (dataOut[4] == 0)
		//		set = false;
		//	else if (dataOut[4] == 1)
		//		set = true;
		//	return dataOut.Length >= 5 && dataOut[0] == data[0] && dataOut[1] == data[1] && dataOut[2] == data[2] && dataOut[3] == data[3] &&
		//			address == outAddress && packetId == outPacketId;
		//}

		//public bool SendSetConfiguration(uint packetId, uint encryptionKey, uint address, uint relayAddress, byte segment,
		//		HeatingVisualComponent heating)
		//{
		//	byte[] data = new byte[] {
		//			(byte)'s', (byte)'C', (byte)'O', (byte)'N', (byte)'F', Common.Uint32_3Byte(relayAddress),
		//			Common.Uint32_2Byte(relayAddress), Common.Uint32_1Byte(relayAddress), Common.Uint32_0Byte(relayAddress), segment,
		//			(byte)heating.Control.HeatingMode, (byte)heating.Control.DayFrom.Hours, (byte)heating.Control.DayFrom.Minutes,
		//			(byte)heating.Control.NightFrom.Hours, (byte)heating.Control.NightFrom.Minutes,
		//			Common.Uint32_1Byte((uint)(heating.Control.ManualTemperature * 10f)), Common.Uint32_0Byte((uint)(heating.Control.ManualTemperature * 10f)),
		//			Common.Uint32_1Byte((uint)(heating.Control.DayTemperature * 10f)), Common.Uint32_0Byte((uint)(heating.Control.DayTemperature * 10f)),
		//			Common.Uint32_1Byte((uint)(heating.Control.NightTemperature * 10f)), Common.Uint32_0Byte((uint)(heating.Control.NightTemperature * 10f)) };
		//	if (!com.SendPacket(packetId, encryptionKey, address, data, out uint outPacketId, out uint _, out uint outAddress, out byte[] dataOut))
		//		return false;

		//	return dataOut.Length >= 6 && dataOut[0] == data[0] && dataOut[1] == data[1] && dataOut[2] == data[2] && dataOut[3] == data[3] &&
		//			dataOut[4] == data[4] && address == outAddress && packetId == outPacketId && dataOut[5] == 0;
		//}

		//public bool SendGetConfiguration(uint packetId, uint encryptionKey, uint address, uint relayAddress, byte segment,
		//		out HeatingVisualComponent heating)
		//{
		//	heating = null;
		//	byte[] data = new byte[] {
		//			(byte)'g', (byte)'C', (byte)'O', (byte)'N', (byte)'F', Common.Uint32_3Byte(relayAddress),
		//			Common.Uint32_2Byte(relayAddress), Common.Uint32_1Byte(relayAddress), Common.Uint32_0Byte(relayAddress), segment };
		//	if (!com.SendPacket(packetId, encryptionKey, address, data, out uint outPacketId, out uint _, out uint outAddress, out byte[] dataOut))
		//		return false;

		//	if (dataOut.Length != 17 || dataOut[5] != 0)
		//		return false;

		//	heating = new HeatingVisualComponent
		//	{
		//		DeviceItem = new Devices()
		//		{
		//			Address = relayAddress,
		//		},
		//		DeviceSegment = segment,
		//		Control = new HeatingVisualComponentControl()
		//		{
		//			HeatingMode = (HeatingVisualComponentControl.Mode)dataOut[6],
		//			DayFrom = new TimeSpan(dataOut[7], dataOut[8], 0),
		//			NightFrom = new TimeSpan(dataOut[9], dataOut[10], 0),
		//			ManualTemperature = ((dataOut[11] << 8) | dataOut[12]) / 10f,
		//			DayTemperature = ((dataOut[13] << 8) | dataOut[14]) / 10f,
		//			NightTemperature = ((dataOut[15] << 8) | dataOut[16]) / 10f,
		//		},
		//	};
		//	return dataOut.Length >= 5 && dataOut[0] == data[0] && dataOut[1] == data[1] && dataOut[2] == data[2] && dataOut[3] == data[3] &&
		//			dataOut[4] == data[4] && address == outAddress && packetId == outPacketId;
		//}
	}
}
