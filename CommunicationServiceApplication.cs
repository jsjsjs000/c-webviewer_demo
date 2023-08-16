using InteligentnyDomRelay;
using InteligentnyDomWebViewer.Model;
using SmartHomeTool.SmartHomeLibrary;
using System.Net.Sockets;
using System.Net;
using static SmartHomeTool.SmartHomeLibrary.Commands;

namespace InteligentnyDomWebViewer
{
	public class CommunicationServiceApplication : ThreadClass
	{
		const int Port = 27501;    // object 1 - Adam Kukuc
		const int SocketTimeout = 5000;
		const int BufferSize = 128 * 1024;
		const int ReadTimeoutMs = 30;

		public Model.Devices cu;

		readonly IServiceScope scope;
		TcpListener tcpListener = new(IPAddress.Any, Port);
		readonly List<SocketItem> sockets = new();
		int lastReceiveMiliseconds;

		public CommunicationServiceApplication(IServiceProvider serviceProvider) : base()
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

									AnalizeAndAnswer(socket, buffer);

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



					if (ExitThread)
						continue;


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
				SocketItem item = new();
				item.socket = socket;
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

		void AnalizeAndAnswer(TcpClient tcp, byte[] receiveBuffer)
		{
			if (Packets.FindFrameAndDecodePacketInBuffer(receiveBuffer, receiveBuffer.Length, out uint packetId,
					out uint encryptionKey, out uint address, out byte[] data, out uint frameCrc, out uint calculatedCrc,
					out bool isAnswer) && data.Length >= 1 && !isAnswer)
			{
				if (address == Packets.Broadcast)
				{
					//NoAnswer();
					return;
				}

				//string comment = PacketsComments.GetCommentToCorrectFrame(packetId, encryptionKey, address, data, isAnswer, true);
				//if (comment.Length > 0)
				//	lock (packetsLogQueue)
				//		packetsLogQueue.Enqueue(new PacketLog(PacketLog.Type.Debug, DateTime.Now, Array.Empty<byte>(),
				//				Packets.PacketDirection.None, " " + comment, false));

				//if (ResponseTime > 0)
				//	Thread.Sleep(ResponseTime);

				byte command = data[0];
					/// Command: 'g' - Get CU Status (only CU)
				if (command == 'g' && data.Length == 4)
				{
					byte details = data[3];
					byte[] bytes = CentralUnitStatus.GetBytes(CommunicationService.Statuses, CommunicationService.HeatingVisualComponents,
							CommunicationService.CuUptime, CommunicationService.CuVin, details);
					SendPacket(tcp, packetId, encryptionKey, address, true, bytes);
				}
				/// Command: "sREL" - Set Relay State (only CU)
				//else if (command == 's' && data.Length == 10 && data[1] == 'R' && data[2] == 'E' && data[3] == 'L')
				//	SendPacket(packetId, encryptionKey, address, true, new byte[] { command, (byte)'R', (byte)'E', (byte)'L', 0 });
				//	/// Command: "gREL" - Get Relay State (only CU)
				//else if (command == 'g' && data.Length == 9 && data[1] == 'R' && data[2] == 'E' && data[3] == 'L')
				//	SendPacket(packetId, encryptionKey, address, true, new byte[] { command, (byte)'R', (byte)'E', (byte)'L', 0 });
					/// Command: "gCOMP" - Get Visual Components Configuration (only CU)
				else if (command == 'g' && data.Length == 10 && data[1] == 'C' && data[2] == 'O' && data[3] == 'M' && data[4] == 'P')
				{
					bool send = false;
					uint relayAddress = Common.Uint32FromBytes(data[5], data[6], data[7], data[8]);
					foreach (HeatingVisualComponent heatingVisualComponent in CommunicationService.HeatingVisualComponents)
						if (heatingVisualComponent.DeviceItem.Address == relayAddress)
						{
							byte[] bytes = heatingVisualComponent.GetBytes();
							SendPacket(tcp, packetId, encryptionKey, address, true, bytes);
							send = true;
						}
					if (!send)
						SendPacket(tcp, packetId, encryptionKey, address, true, Array.Empty<byte>());
				}
				//	/// Command: "sCOMP" - Set Visual Components Configuration (only CU)
				//else if (command == 'S' && data.Length == 21 && data[1] == 'C' && data[2] == 'O' && data[3] == 'M' && data[4] == 'P')
				//	SendPacket(packetId, encryptionKey, address, true, new byte[] { command, (byte)'C', (byte)'O', (byte)'M', (byte)'P', 0 });

				//receiveBufferIndex = 0;
			}
			else
			{
				//string comment = PacketsComments.GetCommentToErrorFrame(frameCrc, calculatedCrc);
				//if (comment.Length > 0)
				//	lock (packetsLogQueue)
				//		packetsLogQueue.Enqueue(new PacketLog(PacketLog.Type.Debug, DateTime.Now, Array.Empty<byte>(),
				//				Packets.PacketDirection.None, " " + comment, true));
			}
		}

		public void SendPacket(TcpClient tcp, uint packetId, uint encryptionKey, uint address, bool isAnswer, byte[] data)
		{
			byte[] send = Packets.EncodePacket(packetId, encryptionKey, address, data, isAnswer);
			tcp.GetStream().Write(send);

			//DateTime startRead = DateTime.Now;
			//byte[] receiveBuffer = new byte[2048];
			//int receiveBufferIndex = 0;

			//while (DateTime.Now.Subtract(startRead).TotalMilliseconds < ReadTimeoutMs)
			//{
			//	int read = tcp.GetStream().Read(receiveBuffer, receiveBufferIndex, receiveBuffer.Length - receiveBufferIndex);
			//	if (read > 0)
			//	{
			//		receiveBufferIndex += read;
			//		if (Packets.FindFrameAndDecodePacketInBuffer(receiveBuffer, receiveBufferIndex, out uint outPacketId,
			//				out uint outEncryptionKey, out uint outAddress, out byte[] outData, out isAnswer) && outData.Length >= 1 && isAnswer)
			//		{
			//			//byte[] received = new byte[receiveBufferIndex];
			//			//Array.Copy(receiveBuffer, prevReceiveBufferIndex, received, 0, received.Length);

			//			return packetId == outPacketId && encryptionKey == outEncryptionKey && address == outAddress;
			//		}
			//	}
			//	else
			//		Thread.Sleep(2);
			//}
			
			//return false;
		}

		//bool Send_Receive(byte[] send, bool onlyOneAnswer, out uint outPacketId, out uint outEncryptionKey,
		//		out uint outAddress, out List<byte[]> outDatas)
		//{
		//	outPacketId = 0;
		//	outEncryptionKey = 0;
		//	outAddress = 0;
		//	outDatas = new List<byte[]>();

		//	try
		//	{
		//		//DateTime startRead = DateTime.Now;
		//		//receiveBufferIndex = 0;
		//		//int prevReceiveBufferIndex = receiveBufferIndex;
		//		//while (DateTime.Now.Subtract(startRead).TotalMilliseconds < ReadTimeoutMs)
		//		//{
		//		//	if (connectionType == ConnectionType.Com && com.BytesToRead > 0 ||
		//		//			connectionType == ConnectionType.Tcp && tcp.Available > 0)
		//		//	{
		//		//		if (connectionType == ConnectionType.Com &&
		//		//				receiveBufferIndex + com.BytesToRead >= receiveBuffer.Length ||
		//		//				connectionType == ConnectionType.Tcp &&
		//		//				receiveBufferIndex + tcp.Available >= receiveBuffer.Length)  /// buffer overflow
		//		//		{
		//		//			receiveBufferIndex = 0;
		//		//			prevReceiveBufferIndex = 0;
		//		//		}

		//		//		if (connectionType == ConnectionType.Com)
		//		//			receiveBufferIndex += com.Read(receiveBuffer, receiveBufferIndex, com.BytesToRead);
		//		//		else
		//		//			receiveBufferIndex += tcp.Receive(receiveBuffer, receiveBufferIndex, tcp.Available, SocketFlags.None);

		//		//		if (Packets.FindFrameAndDecodePacketInBuffer(receiveBuffer, receiveBufferIndex, out outPacketId,
		//		//				out outEncryptionKey, out outAddress, out byte[] outData, out bool isAnswer) && outData.Length >= 1 && isAnswer)
		//		//		{
		//		//			byte[] received = new byte[receiveBufferIndex - prevReceiveBufferIndex];
		//		//			Array.Copy(receiveBuffer, prevReceiveBufferIndex, received, 0, received.Length);
		//		//			if (CanLogPackets)
		//		//			{
		//		//				lock (packetsLogQueue)
		//		//					packetsLogQueue.Enqueue(new PacketLog(PacketLog.Type.Packet, DateTime.Now, received, Packets.PacketDirection.In));

		//		//				string comment = PacketsComments.DecodeFrameAndGetComment(received, out bool isError);
		//		//				lock (packetsLogQueue)
		//		//					if (comment.Length > 0)
		//		//						packetsLogQueue.Enqueue(new PacketLog(PacketLog.Type.Debug, DateTime.Now, Array.Empty<byte>(), Packets.PacketDirection.None,
		//		//								" " + comment + Environment.NewLine, isError));
		//		//					else
		//		//						packetsLogQueue.Enqueue(new PacketLog(PacketLog.Type.Debug, DateTime.Now, Array.Empty<byte>(), Packets.PacketDirection.None,
		//		//								"", isError));
		//		//			}

		//		//			outDatas.Add(outData);
		//		//			receiveBufferIndex = 0;
		//		//			waitingForAnswer = false;
		//		//			lastReceiveMiliseconds = (int)DateTime.Now.Subtract(startRead).TotalMilliseconds;
		//		//			if (onlyOneAnswer)
		//		//				return true;
		//		//		}
		//		//	}
		//		//	Thread.Sleep(1);
		//		//}

		//		//if (outDatas.Count == 0)
		//		//{
		//		//	if (!CanLogPackets && CanLogWrongPackets)
		//		//		lock (packetsLogQueue)
		//		//		{
		//		//			packetsLogQueue.Enqueue(new PacketLog(PacketLog.Type.Packet, DateTime.Now, send, Packets.PacketDirection.Out));
		//		//			string comment = PacketsComments.DecodeFrameAndGetComment(send, out bool isError);
		//		//			if (comment.Length > 0)
		//		//				packetsLogQueue.Enqueue(new PacketLog(PacketLog.Type.Debug, DateTime.Now, Array.Empty<byte>(), Packets.PacketDirection.None,
		//		//						" " + comment, isError));
		//		//		}

		//		//	byte[] received2 = new byte[receiveBufferIndex - prevReceiveBufferIndex];
		//		//	Array.Copy(receiveBuffer, prevReceiveBufferIndex, received2, 0, received2.Length);
		//		//	if (CanLogPackets || CanLogWrongPackets)
		//		//		lock (packetsLogQueue)
		//		//			packetsLogQueue.Enqueue(new PacketLog(PacketLog.Type.Packet, DateTime.Now, received2, Packets.PacketDirection.In));
		//		//}
		//	}
		//	catch { }

		//	//receiveBufferIndex = 0;
		//	//waitingForAnswer = false;
		//	return outDatas.Count > 0;
		//}
	}
}
