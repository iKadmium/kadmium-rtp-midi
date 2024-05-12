using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;
using Kadmium_Udp;
using KadmiumRtpMidi.Packets;
using Makaretu.Dns;

namespace KadmiumRtpMidi
{
	public class Session
	{
		private IUdpWrapper ControlPortListener { get; }
		private IUdpWrapper MidiPortListener { get; }
		private MulticastService Mdns { get; }
		private UInt32 Ssrc { get; }
		private string BonjourName { get; }
		private IPEndPoint ControlEndPoint { get; }
		private IPEndPoint MidiEndPoint { get; }
		private ushort LastSequenceNumber { get; set; } = 0;
		private Timer ResponseTimer { get; }
		private List<IPEndPoint> RemoteEndPoints { get; } = new List<IPEndPoint>();

		public event EventHandler<PacketReceivedEventArgs> OnPacketReceived;
		
		public Session(IPAddress address, ushort port, string serviceName)
		{
			BonjourName = serviceName;
			Ssrc = (uint)new Random().Next();

			ControlEndPoint = new IPEndPoint(IPAddress.Any, port);
			MidiEndPoint = new IPEndPoint(IPAddress.Any, port + 1);

			ControlPortListener = new UdpWrapper();
			ControlPortListener.OnPacketReceived += OnControlPortPacketReceived;
			ControlPortListener.Listen(ControlEndPoint);

			MidiPortListener = new UdpWrapper();
			MidiPortListener.OnPacketReceived += OnMidiPortPacketReceived;
			MidiPortListener.Listen(MidiEndPoint);

			ResponseTimer = new Timer(5000);
			ResponseTimer.Elapsed += async (sender, e) =>
			{
				if (RemoteEndPoints != null)
				{
					var ackPacket = new AcknowledgementPacket
					{
						Ssrc = Ssrc,
						SequenceNumber = LastSequenceNumber
					};
					using var owner = MemoryPool<byte>.Shared.Rent(AcknowledgementPacket.Length);
					var packetMem = owner.Memory[0..AcknowledgementPacket.Length];
					ackPacket.WriteTo(packetMem);
					var tasks = new List<Task>();
					foreach (var endpoint in RemoteEndPoints)
					{
						tasks.Add(ControlPortListener.Send(endpoint, ControlEndPoint, packetMem));
					}
					await Task.WhenAll(tasks);
				}
			};
			ResponseTimer.Start();

			Mdns = new MulticastService((nics) =>
			{
				var matchingNics = from nic in nics
								   from nicAddress in nic.GetIPProperties().UnicastAddresses
								   where nicAddress.Address.Equals(address)
								   select nic;
				return matchingNics;
			});

			Mdns.QueryReceived += (s, e) =>
			{
				var names = e.Message.Questions
					.Select(q => q.Name + " " + q.Type);
				Console.WriteLine($"got a query for {String.Join(", ", names)}");
			};
			Mdns.AnswerReceived += (s, e) =>
			{
				var names = e.Message.Answers
					.Select(q => q.Name + " " + q.Type)
					.Distinct();
				Console.WriteLine($"got answer for {String.Join(", ", names)}");
			};
			Mdns.NetworkInterfaceDiscovered += (s, e) =>
			{
				foreach (var nic in e.NetworkInterfaces)
				{
					Console.WriteLine($"discovered NIC '{nic.Name}'");
				}
			};

			var sd = new ServiceDiscovery(Mdns);
			sd.Advertise(new ServiceProfile("ipfs1", "_ipfs-discovery._udp", 5010));
			sd.Advertise(new ServiceProfile(BonjourName, "_apple-midi._udp", port));
			Mdns.Start();
		}

		private async void OnControlPortPacketReceived(object sender, UdpReceiveResult e)
		{
			var packet = Packet.Parse(e.Buffer);
			switch (packet)
			{
				case InvitationPacket invitation:
					Console.WriteLine("Control Port invitation received from " + invitation.Name + " at " + e.RemoteEndPoint);
					await Respond(invitation, e.RemoteEndPoint, ControlPortListener, ControlEndPoint);
					RemoteEndPoints.Add(e.RemoteEndPoint);
					break;
				case ClockSyncPacket sync:
					Console.WriteLine("Control Port clock sync received");
					await Respond(sync, e.RemoteEndPoint, ControlPortListener, ControlEndPoint);
					break;
				case SessionClosePacket close:
					Console.WriteLine("Control Port session close received");
					RemoteEndPoints.Remove(e.RemoteEndPoint);
					break;
				case InvitationResponsePacket response:
					Console.WriteLine("Invitation Response received");
					RemoteEndPoints.Add(e.RemoteEndPoint);
					var midiRemoteEndpoint = new IPEndPoint(e.RemoteEndPoint.Address, e.RemoteEndPoint.Port + 1);
					await Invite(midiRemoteEndpoint, MidiPortListener, MidiEndPoint);
					break;
				case ControlPacket ctrl:
					switch (ctrl.Command)
					{
						default:
							Console.WriteLine("Received " + ctrl.Command + " packet");
							break;
					}
					break;
				default:
					Console.WriteLine("non control packet received on control port");
					break;
			}
		}

		private async void OnMidiPortPacketReceived(object sender, UdpReceiveResult e)
		{
			var packet = Packet.Parse(e.Buffer);
			switch (packet)
			{
				case InvitationPacket invitation:
					Console.WriteLine("MIDI Invitation received from " + invitation.Name + " at " + e.RemoteEndPoint);
					await Respond(invitation, e.RemoteEndPoint, MidiPortListener, MidiEndPoint);
					break;
				case ClockSyncPacket sync:
					Console.WriteLine("MIDI clock sync received");
					await Respond(sync, e.RemoteEndPoint, MidiPortListener, MidiEndPoint);
					break;
				case InvitationResponsePacket response:
					Console.WriteLine("MIDI Invitation response received from " + response.Name + " at " + e.RemoteEndPoint);
					break;
				case ControlPacket ctrl:
					switch (ctrl.Command)
					{
						default:
							Console.WriteLine("MIDI Received " + ctrl.Command + " packet");
							break;
					}
					break;
				case DataPacket data:
					if (
						(data.SequenceNumber > LastSequenceNumber)
						|| ((ushort.MaxValue - LastSequenceNumber) < 100 && (data.SequenceNumber - ushort.MinValue) < 100)
					)
					{
						LastSequenceNumber = data.SequenceNumber;
					}
					var tasks = new List<Task>();
					foreach (var endpoint in RemoteEndPoints.Except(new[] { e.RemoteEndPoint }))
					{
						tasks.Add(this.MidiPortListener.Send(endpoint, e.Buffer));
					}
					await Task.WhenAll(tasks);
					OnPacketReceived?.Invoke(e.RemoteEndPoint, new PacketReceivedEventArgs(data));
					break;
				default:
					Console.WriteLine("non control packet received on midi port");
					break;
			}
		}

		public async Task Invite(IPEndPoint remoteEndpoint)
		{
			await Invite(remoteEndpoint, ControlPortListener, ControlEndPoint);
		}

		private async Task Invite(IPEndPoint remoteEndpoint, IUdpWrapper localWrapper, IPEndPoint localEndpoint)
		{
			var invitation = new InvitationPacket
			{
				Command = ControlPacketCommand.IN,
				Name = BonjourName,
				Ssrc = Ssrc
			};
			using var owner = MemoryPool<byte>.Shared.Rent(invitation.Length);
			var packetMem = owner.Memory[0..invitation.Length];
			invitation.WriteTo(packetMem);
			await localWrapper.Send(remoteEndpoint, localEndpoint, packetMem);
		}

		private async Task Respond(InvitationPacket invitation, IPEndPoint remoteEndpoint, IUdpWrapper localWrapper, IPEndPoint localEndpoint)
		{
			var response = new InvitationPacket
			{
				Command = ControlPacketCommand.OK,
				Name = BonjourName,
				Ssrc = Ssrc,
				InitiatorToken = invitation.InitiatorToken
			};
			using var owner = MemoryPool<byte>.Shared.Rent(response.Length);
			var packetMem = owner.Memory[0..response.Length];
			response.WriteTo(packetMem);
			await localWrapper.Send(remoteEndpoint, localEndpoint, packetMem);
		}

		private async Task Respond(ClockSyncPacket initial, IPEndPoint remoteEndpoint, IUdpWrapper localWrapper, IPEndPoint localEndpoint)
		{
			var response = new ClockSyncPacket
			{
				Command = ControlPacketCommand.OK,
				Ssrc = Ssrc,
				TimestampCount = (byte)(initial.TimestampCount + 1),
				Timestamps = new UInt64[3]
			};
			initial.Timestamps.CopyTo(response.Timestamps, 0);
			var timestamp = DateTime.Now.Ticks / 1000;
			response.Timestamps[1] = (ulong)timestamp;

			using var owner = MemoryPool<byte>.Shared.Rent(response.Length);
			var packetMem = owner.Memory[0..response.Length];
			response.WriteTo(packetMem);
			await localWrapper.Send(remoteEndpoint, localEndpoint, packetMem);
		}

		public IEnumerable<IPEndPoint> Peers => RemoteEndPoints;
	}
}