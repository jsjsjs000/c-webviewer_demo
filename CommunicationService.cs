using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InteligentnyDomWebViewer.Model;
using Microsoft.EntityFrameworkCore;
using SmartHomeTool.SmartHomeLibrary;
using static SmartHomeTool.SmartHomeLibrary.Commands;
using static SmartHomeTool.SmartHomeLibrary.Communication;

namespace InteligentnyDomRelay
{
	internal class CommunicationService : Communication
	{
		public Devices? cu;

		public CommunicationService(Devices cu) : base()
		{
			this.cu = cu;
		}

		protected override void ThreadProc()
		{
			ExitedThread = false;
			Common.SetDateFormat();
			this.CanLogPackets = true;
			this.CanLogWrongPackets = true;
			byte[] received = Array.Empty<byte>();
			//DateTime lastReceived = new();
			//DateTime lastPool = new();
			DateTime lastCommandSend = new();
			DateTime lastWriteStatus = new();

			while (!ExitThread)
			{
				try
				{
					bool isConnected = IsConnected();
					while (!isConnected && !ExitThread)
					{
						if (Connect())
							break;
						else
						{
							using DatabaseContext databaseContext = new();
							var dcu = new DevicesCu()
							{
								Address = cu.Address,
								LastUpdated = DateTime.Now,
								Error = true,
								ErrorFrom = DateTime.Now, // $$
								Uptime = uint.MaxValue,
							};
							databaseContext.DevicesCu.Update(dcu);
							databaseContext.SaveChanges();

							for (int i = 0; i < 20 && !ExitThread; i++)
								Thread.Sleep(50);
						}
					}

					if (ExitThread)
						continue;

					if (DateTime.Now.Subtract(lastCommandSend).TotalMilliseconds >= 20 * 1000)
					{
						uint address = cu.Address;

//bool ok = cmd.SendGetDeviceVersion(1, 0x08080808, address, out Commands.DeviceVersion version);
//Console.WriteLine($"{address:x8} {ok} {version.Uptime}");

						List<Commands.CentralUnitStatus> allStatuses = new();
						int fromItem = 0;
						int itemsCount = 0;
						DateTime nowWithoutSeconds = Common.DateTimeWithoutSeconds(DateTime.Now);
						bool writeHistory = Math.Abs(nowWithoutSeconds.Subtract(Common.DateTimeWithoutSeconds(lastWriteStatus)).TotalSeconds) >= 1;
						do
						{
							using DatabaseContext databaseContext = new();

							if (cmd.SendGetCentralUnitStatus(1, 0x08080808, address,
									out List<CentralUnitStatus> statuses,
									out List<HeatingVisualComponent> heatingVisualComponents,
									fromItem, true, out itemsCount, out uint cuUptime, out float cuVin))
							{
Console.WriteLine($"{DateTime.Now} cu: {address:x8} ok - from: {fromItem}, count: {statuses.Count} + {heatingVisualComponents.Count}");

								allStatuses.AddRange(statuses);

								if (fromItem == 0)
								{
									var dcu = new DevicesCu()
									{
										Address = address,
										LastUpdated = DateTime.Now,
										Error = false,
										ErrorFrom = null,
										Uptime = cuUptime,
										Vin = cuVin,
									};
									databaseContext.DevicesCu.Update(dcu);
									databaseContext.Entry(dcu).Property(x => x.Name).IsModified = false;
									databaseContext.SaveChanges();
								}

								foreach (Commands.CentralUnitStatus status in statuses)
								{
//Console.WriteLine($"  address: {status.address:x8}");
									if (status is Commands.TemperatureStatus temperatureStatus)
									{
										for (byte i = 0; i < temperatureStatus.temperatures?.Length; i++)
											if (databaseContext.DevicesTemperatures.
													Where(t => t.Address == temperatureStatus.address && t.Segment == i).
													Any())
											{
												if (writeHistory)
												{
													var ht = new HistoryTemperatures()
													{
														Dt = nowWithoutSeconds,
														Address = temperatureStatus.address,
														Segment = i,
														Temperature = temperatureStatus.temperatures[i],
														Error = temperatureStatus.error,
														Vin = temperatureStatus.vin,
													};
													databaseContext.HistoryTemperatures.Add(ht);
													databaseContext.SaveChanges();
												}

												var dt = new DevicesTemperatures()
												{
													Address = temperatureStatus.address,
													Segment = i,
													LastUpdated = DateTime.Now,
													Temperature = temperatureStatus.temperatures[i],
													Error = temperatureStatus.error,
													ErrorFrom = null,
													Uptime = temperatureStatus.uptime,
													Vin = temperatureStatus.vin,
												};
												databaseContext.DevicesTemperatures.Update(dt);
												databaseContext.Entry(dt).Property(x => x.Name).IsModified = false;
												databaseContext.SaveChanges();
											}
									}
									else if (status is Commands.RelayStatus relaysStatus)
									{
										for (byte i = 0; i < relaysStatus.relaysStates?.Length; i++)
											if (databaseContext.DevicesRelays.
													Where(t => t.Address == relaysStatus.address && t.Segment == i).
													Any())
											{
												if (writeHistory)
												{
													var hr = new HistoryRelays()
													{
														Dt = nowWithoutSeconds,
														Address = relaysStatus.address,
														Segment = i,
														Relay = relaysStatus.relaysStates[i],
														Error = relaysStatus.error,
														Vin = relaysStatus.vin,
													};
													databaseContext.HistoryRelays.Add(hr);
													databaseContext.SaveChanges();
												}

												var dr = new DevicesRelays()
												{
													Address = relaysStatus.address,
													Segment = i,
													LastUpdated = DateTime.Now,
													Relay = relaysStatus.relaysStates[i],
													Error = relaysStatus.error,
													ErrorFrom = null,
													Uptime = relaysStatus.uptime,
													Vin = relaysStatus.vin,
												};
												databaseContext.DevicesRelays.Update(dr);
												databaseContext.Entry(dr).Property(x => x.Name).IsModified = false;
												databaseContext.SaveChanges();
											}
									}
								}

								foreach (HeatingVisualComponent heatingComponent in heatingVisualComponents)
								{
									bool ok = false;
									bool relay = false;
									foreach (Commands.CentralUnitStatus status in allStatuses)
										if (status is Commands.RelayStatus relaysStatus && relaysStatus.relaysStates != null &&
												relaysStatus.address == heatingComponent.DeviceItem.Address)
										{
											relay = relaysStatus.relaysStates[heatingComponent.DeviceSegment];
											ok = true;
											break;
										}

									if (writeHistory)
									{
										var hh = new HistoryHeating()
										{
											Dt = nowWithoutSeconds,
											Address = heatingComponent.DeviceItem.Address,
											Segment = heatingComponent.DeviceSegment,
											Mode = (byte)(heatingComponent.Control.HeatingMode + 1),
										};
										if (heatingComponent.Control.HeatingMode == HeatingVisualComponentControl.Mode.Auto)
											hh.SettingTemperature = heatingComponent.Control.DayTemperature;
										else if (heatingComponent.Control.HeatingMode == HeatingVisualComponentControl.Mode.Manual)
											hh.SettingTemperature = heatingComponent.Control.ManualTemperature;
										hh.Relay = relay;
										if (ok)
											databaseContext.HistoryHeating.Add(hh);
									}

									var dh = new DevicesHeating()
									{
										Address = heatingComponent.DeviceItem.Address,
										Segment = heatingComponent.DeviceSegment,
										Mode = (byte)(heatingComponent.Control.HeatingMode + 1),
										LastUpdated = nowWithoutSeconds,
									};
									if (heatingComponent.Control.HeatingMode == HeatingVisualComponentControl.Mode.Auto)
										dh.SettingTemperature = heatingComponent.Control.DayTemperature;
									else if (heatingComponent.Control.HeatingMode == HeatingVisualComponentControl.Mode.Manual)
										dh.SettingTemperature = heatingComponent.Control.ManualTemperature;
									dh.Relay = relay;
									if (ok)
									{
										databaseContext.DevicesHeatings.Update(dh);
										databaseContext.Entry(dh).Property(x => x.Name).IsModified = false;
										databaseContext.SaveChanges();
									}
								}

								fromItem += statuses.Count + heatingVisualComponents.Count;
							}
							else
							{
								var dcu = new DevicesCu()
								{
									Address = address,
									LastUpdated = DateTime.Now,
									Error = true,
									ErrorFrom = DateTime.Now, // $$
									Uptime = uint.MaxValue,
									Vin = 0,
								};
								databaseContext.DevicesCu.Update(dcu);
								databaseContext.SaveChanges();
								lastCommandSend = DateTime.Now;
								break;
							}

							databaseContext.SaveChanges();
							lastCommandSend = DateTime.Now;
						}
						while (fromItem < itemsCount);

						if (writeHistory)
							lastWriteStatus = nowWithoutSeconds;
					}

					#region
					//lock (waitingForAnswerLock)
					//	if (isConnected && !waitingForAnswer)
					//	{
					//if (connectionType == ConnectionType.Com && com.BytesToRead > 0 ||
					//		connectionType == ConnectionType.Tcp && tcp.Available > 0)
					//{
					//	int bytesToRead = 0;
					//	if (connectionType == ConnectionType.Com && com.BytesToRead > 0)
					//		bytesToRead = com.BytesToRead;
					//	if (connectionType == ConnectionType.Tcp && tcp.Available > 0)
					//		bytesToRead = tcp.Available;

					//	if (receiveBufferIndex + bytesToRead >= receiveBuffer.Length)
					//		receiveBufferIndex = 0;  /// buffer overflow


					//	int readedBytes = 0;
					//	if (connectionType == ConnectionType.Com && com.BytesToRead > 0)
					//		readedBytes = com.Read(receiveBuffer, receiveBufferIndex, bytesToRead);
					//	if (connectionType == ConnectionType.Tcp && tcp.Available > 0)
					//		readedBytes = tcp.Receive(receiveBuffer, receiveBufferIndex, bytesToRead, System.Net.Sockets.SocketFlags.None);

					//	int prevReceivedLength = received.Length;
					//	Array.Resize(ref received, received.Length + readedBytes);
					//	Array.Copy(receiveBuffer, receiveBufferIndex, received, prevReceivedLength, readedBytes);

					//	byte[] received_ = Array.Empty<byte>();
					//	do
					//	{
					//		received_ = Packets.GetFirstPacketFromData(received, out received);

					//		receiveBufferIndex += readedBytes;
					//		string comment = PacketsComments.DecodeFrameAndGetComment(received_, out bool isError);
					//		if (isError)
					//			lastReceived = DateTime.Now;
					//		else
					//		{
					//			lastReceived = new DateTime();
					//			lock (packetsLogQueue)
					//				packetsLogQueue.Enqueue(new PacketLog(PacketLog.Type.Packet, DateTime.Now, received_, Packets.PacketDirection.In));
					//			if (comment.Length > 0)
					//				lock (packetsLogQueue)
					//					packetsLogQueue.Enqueue(new PacketLog(PacketLog.Type.Debug, DateTime.Now, Array.Empty<byte>(),
					//							Packets.PacketDirection.None, " " + comment, isError));
					//		}
					//	}
					//	while (received_.Length > 0);
					//}
					//Thread.Sleep(1);

					//if (lastReceived != new DateTime() && DateTime.Now.Subtract(lastReceived).TotalMilliseconds >= ReadTimeoutMs &&
					//		received.Length > 0)
					//{
					//	byte[] received_ = Array.Empty<byte>();
					//	do
					//	{
					//		received_ = Packets.GetFirstPacketFromData(received, out received);

					//		string comment = PacketsComments.DecodeFrameAndGetComment(received_, out bool isError);
					//		lastReceived = new DateTime();
					//		lock (packetsLogQueue)
					//			packetsLogQueue.Enqueue(new PacketLog(PacketLog.Type.Packet, DateTime.Now, received_, Packets.PacketDirection.In));
					//		if (comment.Length > 0)
					//			lock (packetsLogQueue)
					//				packetsLogQueue.Enqueue(new PacketLog(PacketLog.Type.Debug, DateTime.Now, Array.Empty<byte>(),
					//						Packets.PacketDirection.None, " " + comment, isError));
					//		//received = Array.Empty<byte>();
					//	}
					//	while (received_.Length > 0);
					//}

					//byte[] packet = Packets.EncodePacket(0x01020304, 0x11121314, 0x21222324, new byte[] { 0xf0, 0xf1 }, false);
					//bool ok = SendPacket(0x21222324, 0x11121314, 0x01020304, new byte[] { (byte)'p', 0xf0, 0xf1, 0xf2, 0xf3, 0xf4, 0xf5, 0xf6 },
					//		out uint outPacketId, out uint outEncryptionKey, out uint outAddress, out byte[] outData);
					//byte[] packet1 = Packets.GetFirstPacketFromData(packet, out byte[] rest);
					//bool ok = Packets.FindFrameAndDecodePacketInBuffer(packet, packet.Length, out uint packetId,
					//		out uint encryptionKey, out uint address, out byte[] data, out bool isAnswer);
					//System.Diagnostics.Debug.WriteLine($"{ok} {lastReceiveMiliseconds}");

					//Thread.Sleep(1000);
					//}
					//else
					//	Thread.Sleep(1);

					//if (tcp != null &&
					//		DateTime.Now.Subtract(lastReceived).TotalMilliseconds >= 2000 &&
					//		DateTime.Now.Subtract(lastPool).TotalMilliseconds >= 2000)
					//{
					//	if (!tcp.Poll(200 * 1000, System.Net.Sockets.SelectMode.SelectWrite))
					//		tcp.Disconnect(true);
					//	else
					//		lastPool = DateTime.Now;
					//}
					#endregion

					Thread.Sleep(1);
				}
				catch // (Exception ex)
				{
				}
			}

			Disconnect();
			ExitedThread = true;
		}
	}
}
