using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InteligentnyDomWebViewer.Model;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.EntityFrameworkCore;
using SmartHomeTool.SmartHomeLibrary;
using static System.Formats.Asn1.AsnWriter;
using static InteligentnyDomWebViewer.Model.DevicesHeatings;
using static SmartHomeTool.SmartHomeLibrary.Commands;

namespace InteligentnyDomRelay
{
	public class CommunicationService : ThreadClass //: Communication
	{
		const int Port = 28501;    // object 1 - Adam Kukuc
		const int SocketTimeout = 5000;
		const int BufferSize = 128 * 1024;

		public InteligentnyDomWebViewer.Model.Devices cu;
		public DateTime lastCentralUnitStatusReceived = new();

		public static List<CentralUnitStatus> Statuses = new();
		public static List<HeatingVisualComponent> HeatingVisualComponents = new();
		public static uint CuUptime;
		public static float CuVin;

		readonly IServiceScope scope;
		TcpListener tcpListener = new(IPAddress.Any, Port);
		public readonly List<SocketItem> sockets = new();

		public CommunicationService(IServiceProvider serviceProvider) : base()
		{
			scope = serviceProvider.CreateScope();
			using WebDbContext databaseContext = scope.ServiceProvider.GetRequiredService<WebDbContext>();

			cu = databaseContext.Devices
					.Where(n => n.Active &&
											(n.HardwareType2 == InteligentnyDomWebViewer.Model.DeviceVersion.HardwareType2Enum.CU ||
											 n.HardwareType2 == InteligentnyDomWebViewer.Model.DeviceVersion.HardwareType2Enum.CU_WR))
					.First();

			StartThread();
		}

		void Connect()
		{
			tcpListener = new TcpListener(IPAddress.Any, Port);
			tcpListener.Start();
			tcpListener.BeginAcceptTcpClient(new AsyncCallback(OnAcceptTcpClient), tcpListener);
		}

		void Disconnect()
		{
			try
			{
				tcpListener?.Stop();
			}
			catch { }
		}

		protected override void ThreadProc()
		{
			ExitedThread = false;
			Common.SetDateFormat();
			DateTime lastWriteStatus = new();
			List<Commands.CentralUnitStatus> allStatuses = new();

			Disconnect();
			Connect();

			while (!ExitThread)
			{
				try
				{
					List<SocketItem> socketsToRemove = new();
					for (int s = 0; s < sockets.Count; s++)
					{
						SocketItem socketItem = sockets[s];
						lock (socketItem)
						{
							TcpClient socket = sockets[s].socket;
							byte[] socketsData = sockets[s].data;
							try
							{
								if (!socket.Connected)
								{
									socketsToRemove.Add(socketItem);
									continue;
								}

								int available = socket.Available;
								if (available > 0)
								{
									socketItem.lastDataResponse = DateTime.Now;
									byte[] buffer;
									int readed;
									if (socketsData.Length > 0)
									{
										buffer = socketsData;
										int oldLength = buffer.Length;
										Array.Resize(ref buffer, buffer.Length + available);
										socketItem.data = Array.Empty<byte>();
										readed = socket.GetStream().Read(buffer, oldLength, available);
										////" " + (packetNr++).ToString("d3"));
										//Console.WriteLine("    Readed part " + readed + " " + buffer.Length);
									}
									else
									{
										buffer = new byte[available];
										readed = socket.GetStream().Read(buffer);
										////" " + (packetNr++).ToString("d3"));
										//Console.WriteLine("    Readed all " + readed + " " + buffer.Length);
									}

									if (CentralUnitStatus.ParseFromBytes(buffer[0..readed],
											out List<CentralUnitStatus> statuses,
											out List<HeatingVisualComponent> heatingVisualComponents, true, 
											out int itemsCount, out uint cuUptime, out float cuVin))
									{
										lock (Statuses)
											Statuses = statuses;
										lock (HeatingVisualComponents)
											HeatingVisualComponents = heatingVisualComponents;
										CuUptime = cuUptime;
										CuVin = cuVin;

										using WebDbContext databaseContext = scope.ServiceProvider.GetRequiredService<WebDbContext>();
										DateTime nowWithoutSeconds = Common.DateTimeWithoutSeconds(DateTime.Now);
										bool writeHistory = Math.Abs(nowWithoutSeconds.Subtract(Common.DateTimeWithoutSeconds(lastWriteStatus)).TotalSeconds) >= 1;

										allStatuses.AddRange(statuses);

										var dcu = new DevicesCu()
										{
											Address = cu.Address,
											LastUpdated = DateTime.Now,
											Error = false,
											ErrorFrom = null,
											Uptime = cuUptime,
											Vin = cuVin,
										};
										databaseContext.DevicesCu.Update(dcu);
										databaseContext.Entry(dcu).Property(x => x.Name).IsModified = false;
										databaseContext.SaveChanges();

										foreach (Commands.CentralUnitStatus status in statuses)
										{
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
													hh.SettingTemperature = heatingComponent.Control.ManualTemperature; // $$
												else if (heatingComponent.Control.HeatingMode == HeatingVisualComponentControl.Mode.Manual)
													hh.SettingTemperature = heatingComponent.Control.ManualTemperature;
												hh.Relay = relay;
												if (ok)
													databaseContext.HistoryHeating.Add(hh);
											}

											var dh = new DevicesHeatings()
											{
												Address = heatingComponent.DeviceItem.Address,
												Segment = heatingComponent.DeviceSegment,
												//Mode = (byte)(heatingComponent.Control.HeatingMode + 1), // $$
												Mode = (HeatingMode)heatingComponent.Control.HeatingMode,
												LastUpdated = nowWithoutSeconds,
											};
											if (heatingComponent.Control.HeatingMode == HeatingVisualComponentControl.Mode.Auto)
												dh.SettingTemperature = heatingComponent.Control.ManualTemperature; // $$
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

										lastCentralUnitStatusReceived = DateTime.Now;
										if (writeHistory)
											lastWriteStatus = nowWithoutSeconds;
									}
								}

								//if (DateTime.Now.Subtract(socketItem.lastDataResponse).TotalSeconds > DisconnectIfNoPingTimeout)
								//{
								//	socketsToRemove.Add(socketItem);
								//	continue;
								//}
							}
							catch (Exception e)
							{
								//logError.WriteDateTimeLog(LogErrorHeader, e.ToString());
							}
						}
					}

					foreach (SocketItem item in socketsToRemove)
					{
						//SendOnChangeConnectionState(item.socket, false);
						sockets.Remove(item);
					}

					foreach (SocketItem item in sockets)
					{
            CentralUnitRequest? centralUnitDeviceItem = null;
						lock (item.commandsToSend)
							if (item.commandsToSend.Count > 0)
								centralUnitDeviceItem = item.commandsToSend.Dequeue();
						if (centralUnitDeviceItem != null)
						{
							if (centralUnitDeviceItem is SetHeatingControlConfigurationRequest setHeatingControlConfigurationRequest)
							{
								//byte[] bytes = setHeatingControlConfigurationRequest.HeatingVisualComponent!.GetBytes();
								item.socket.GetStream().Write(setHeatingControlConfigurationRequest.Data);
							}
						}
					}

					if (ExitThread)
						continue;

#region
//					if (DateTime.Now.Subtract(lastCommandSend).TotalMilliseconds >= 20 * 1000)
//					{
//						uint address = cu.Address;

////bool ok = cmd.SendGetDeviceVersion(1, 0x08080808, address, out Commands.DeviceVersion version);
////Console.WriteLine($"{address:x8} {ok} {version.Uptime}");

//						List<Commands.CentralUnitStatus> allStatuses = new();
//						int fromItem = 0;
//						int itemsCount = 0;
//						DateTime nowWithoutSeconds = Common.DateTimeWithoutSeconds(DateTime.Now);
//						bool writeHistory = Math.Abs(nowWithoutSeconds.Subtract(Common.DateTimeWithoutSeconds(lastWriteStatus)).TotalSeconds) >= 1;
//						do
//						{
//							//using DatabaseContext databaseContext = new();

//							if (cmd.SendGetCentralUnitStatus(1, 0x08080808, address,
//									out List<CentralUnitStatus> statuses,
//									out List<HeatingVisualComponent> heatingVisualComponents,
//									fromItem, true, out itemsCount, out uint cuUptime, out float cuVin))
//							{
//Console.WriteLine($"{DateTime.Now} cu: {address:x8} ok - from: {fromItem}, count: {statuses.Count} + {heatingVisualComponents.Count}");

//								allStatuses.AddRange(statuses);

//								if (fromItem == 0)
//								{
//									var dcu = new DevicesCu()
//									{
//										Address = address,
//										LastUpdated = DateTime.Now,
//										Error = false,
//										ErrorFrom = null,
//										Uptime = cuUptime,
//										Vin = cuVin,
//									};
//									databaseContext.DevicesCu.Update(dcu);
//									databaseContext.Entry(dcu).Property(x => x.Name).IsModified = false;
//									databaseContext.SaveChanges();
//								}

//								foreach (Commands.CentralUnitStatus status in statuses)
//								{
////Console.WriteLine($"  address: {status.address:x8}");
//									if (status is Commands.TemperatureStatus temperatureStatus)
//									{
//										for (byte i = 0; i < temperatureStatus.temperatures?.Length; i++)
//											if (databaseContext.DevicesTemperatures.
//													Where(t => t.Address == temperatureStatus.address && t.Segment == i).
//													Any())
//											{
//												if (writeHistory)
//												{
//													var ht = new HistoryTemperatures()
//													{
//														Dt = nowWithoutSeconds,
//														Address = temperatureStatus.address,
//														Segment = i,
//														Temperature = temperatureStatus.temperatures[i],
//														Error = temperatureStatus.error,
//														Vin = temperatureStatus.vin,
//													};
//													databaseContext.HistoryTemperatures.Add(ht);
//													databaseContext.SaveChanges();
//												}

//												var dt = new DevicesTemperatures()
//												{
//													Address = temperatureStatus.address,
//													Segment = i,
//													LastUpdated = DateTime.Now,
//													Temperature = temperatureStatus.temperatures[i],
//													Error = temperatureStatus.error,
//													ErrorFrom = null,
//													Uptime = temperatureStatus.uptime,
//													Vin = temperatureStatus.vin,
//												};
//												databaseContext.DevicesTemperatures.Update(dt);
//												databaseContext.Entry(dt).Property(x => x.Name).IsModified = false;
//												databaseContext.SaveChanges();
//											}
//									}
//									else if (status is Commands.RelayStatus relaysStatus)
//									{
//										for (byte i = 0; i < relaysStatus.relaysStates?.Length; i++)
//											if (databaseContext.DevicesRelays.
//													Where(t => t.Address == relaysStatus.address && t.Segment == i).
//													Any())
//											{
//												if (writeHistory)
//												{
//													var hr = new HistoryRelays()
//													{
//														Dt = nowWithoutSeconds,
//														Address = relaysStatus.address,
//														Segment = i,
//														Relay = relaysStatus.relaysStates[i],
//														Error = relaysStatus.error,
//														Vin = relaysStatus.vin,
//													};
//													databaseContext.HistoryRelays.Add(hr);
//													databaseContext.SaveChanges();
//												}

//												var dr = new DevicesRelays()
//												{
//													Address = relaysStatus.address,
//													Segment = i,
//													LastUpdated = DateTime.Now,
//													Relay = relaysStatus.relaysStates[i],
//													Error = relaysStatus.error,
//													ErrorFrom = null,
//													Uptime = relaysStatus.uptime,
//													Vin = relaysStatus.vin,
//												};
//												databaseContext.DevicesRelays.Update(dr);
//												databaseContext.Entry(dr).Property(x => x.Name).IsModified = false;
//												databaseContext.SaveChanges();
//											}
//									}
//								}

//								foreach (HeatingVisualComponent heatingComponent in heatingVisualComponents)
//								{
//									bool ok = false;
//									bool relay = false;
//									foreach (Commands.CentralUnitStatus status in allStatuses)
//										if (status is Commands.RelayStatus relaysStatus && relaysStatus.relaysStates != null &&
//												relaysStatus.address == heatingComponent.DeviceItem.Address)
//										{
//											relay = relaysStatus.relaysStates[heatingComponent.DeviceSegment];
//											ok = true;
//											break;
//										}

//									if (writeHistory)
//									{
//										var hh = new HistoryHeating()
//										{
//											Dt = nowWithoutSeconds,
//											Address = heatingComponent.DeviceItem.Address,
//											Segment = heatingComponent.DeviceSegment,
//											Mode = (byte)(heatingComponent.Control.HeatingMode + 1),
//										};
//										if (heatingComponent.Control.HeatingMode == HeatingVisualComponentControl.Mode.Auto)
//											hh.SettingTemperature = heatingComponent.Control.DayTemperature;
//										else if (heatingComponent.Control.HeatingMode == HeatingVisualComponentControl.Mode.Manual)
//											hh.SettingTemperature = heatingComponent.Control.ManualTemperature;
//										hh.Relay = relay;
//										if (ok)
//											databaseContext.HistoryHeating.Add(hh);
//									}

//									var dh = new DevicesHeating()
//									{
//										Address = heatingComponent.DeviceItem.Address,
//										Segment = heatingComponent.DeviceSegment,
//										Mode = (byte)(heatingComponent.Control.HeatingMode + 1),
//										LastUpdated = nowWithoutSeconds,
//									};
//									if (heatingComponent.Control.HeatingMode == HeatingVisualComponentControl.Mode.Auto)
//										dh.SettingTemperature = heatingComponent.Control.DayTemperature;
//									else if (heatingComponent.Control.HeatingMode == HeatingVisualComponentControl.Mode.Manual)
//										dh.SettingTemperature = heatingComponent.Control.ManualTemperature;
//									dh.Relay = relay;
//									if (ok)
//									{
//										databaseContext.DevicesHeatings.Update(dh);
//										databaseContext.Entry(dh).Property(x => x.Name).IsModified = false;
//										databaseContext.SaveChanges();
//									}
//								}

//								fromItem += statuses.Count + heatingVisualComponents.Count;
//							}
//							else
//							{
//								var dcu = new DevicesCu()
//								{
//									Address = address,
//									LastUpdated = DateTime.Now,
//									Error = true,
//									ErrorFrom = DateTime.Now, // $$
//									Uptime = uint.MaxValue,
//									Vin = 0,
//								};
//								databaseContext.DevicesCu.Update(dcu);
//								databaseContext.SaveChanges();
//								lastCommandSend = DateTime.Now;
//								break;
//							}

//							databaseContext.SaveChanges();
//							lastCommandSend = DateTime.Now;
//						}
//						while (fromItem < itemsCount);

//						if (writeHistory)
//							lastWriteStatus = nowWithoutSeconds;
//					}
#endregion


					Thread.Sleep(1);
				}
				catch // (Exception ex)
				{
				}
			}

			//Disconnect();
			ExitedThread = true;
		}

		void OnAcceptTcpClient(IAsyncResult ar)
		{
			try
			{
				if (ExitThread)
					return;

				TcpListener? listener = ar.AsyncState as TcpListener;
				TcpClient socket = listener!.EndAcceptTcpClient(ar);
				socket.ReceiveBufferSize = BufferSize;
				socket.SendBufferSize = BufferSize;
				socket.ReceiveTimeout = SocketTimeout;
				socket.SendTimeout = SocketTimeout;
				//socket.DontFragment = true;
				SocketItem item = new()
				{
					socket = socket
				};
				sockets.Add(item);
			}
			catch (Exception e)
			{
				//logError.WriteDateTimeLog(LogErrorHeader, e.ToString());
			}

			try
			{
				tcpListener.Start();
				tcpListener.BeginAcceptTcpClient(OnAcceptTcpClient, tcpListener);
			}
			catch (Exception e)
			{
				//logError.WriteDateTimeLog(LogErrorHeader, e.ToString());
			}

			//if (socket != null)
			//	SendOnChangeConnectionState(socket, true);
		}

		//void SendOnChangeConnectionState(Socket socket, bool connection)
		//{
		//	if (OnChangeConnectionState != null)
		//		try
		//		{
		//			OnChangeConnectionState(socket, connection);
		//		}
		//		catch (Exception e)
		//		{
		//			logError.WriteDateTimeLog(LogErrorHeader, e.ToString());
		//		}
		//}
	}

	public class SocketItem
	{
		public TcpClient socket;
		public string sessionKey;
		public byte[] data = new byte[0];
		public DateTime lastDataResponse = DateTime.Now;
		public List<string> logsToSend = new();
		//public uint cuAddress = 0xffffffff;
		public Queue<CentralUnitRequest> commandsToSend = new();
	}

	public class CentralUnitRequest { }

	public class SetHeatingControlConfigurationRequest : CentralUnitRequest
	{
		public HeatingVisualComponent? HeatingVisualComponent;
		public byte[] Data;
	}
}
