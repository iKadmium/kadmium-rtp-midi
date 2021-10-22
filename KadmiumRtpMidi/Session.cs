using System;
using System.Buffers;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Kadmium_Udp;
using KadmiumRtpMidi.Packets;
using Makaretu.Dns;

namespace KadmiumRtpMidi
{
	public class Session
	{
		IUdpWrapper ControlPortWrapper { get; }
		IUdpWrapper MidiPortWrapper { get; }
		ServiceProfile ServiceProfile { get; }
		ServiceDiscovery ServiceDiscovery { get; }

		public Session(ushort port)
		{
			ControlPortWrapper = new UdpWrapper();
			ControlPortWrapper.OnPacketReceived += async (sender, e) =>
			{
				var packet = Packet.Parse(e.Buffer);
				if (packet is ControlPacket ctrl)
				{
					switch (ctrl.Command)
					{
						case ControlPacketCommand.IN:
							Console.WriteLine("Invitation received from " + ctrl.Name + " at " + e.RemoteEndPoint);
							await Respond(ctrl, e.RemoteEndPoint);
							break;
						default:
							Console.WriteLine("Received " + ctrl.Command + " packet");
							break;
					}
				}
			};
			ControlPortWrapper.Listen(new IPEndPoint(IPAddress.Any, port));

			MidiPortWrapper = new UdpWrapper();
			MidiPortWrapper.OnPacketReceived += (sender, e) =>
			{
				var packet = Packet.Parse(e.Buffer);
				if (packet is ControlPacket ctrl)
				{
					switch (ctrl.Command)
					{
						case ControlPacketCommand.IN:
							Console.WriteLine("Invitation received from " + ctrl.Name + " at " + e.RemoteEndPoint);
							var response = new ControlPacket
							{
								Command = ControlPacketCommand.OK,
								Name = "MyThing",
								Ssrc = 927230719,
								ProtocolVersion = 2,
								InitiatorToken = ctrl.InitiatorToken
							};
							using (var owner = MemoryPool<byte>.Shared.Rent(response.Length))
							{
								var packetMem = owner.Memory[0..response.Length];
								response.WriteTo(packetMem);
								MidiPortWrapper.Send(e.RemoteEndPoint, packetMem);
							}

							break;
					}
				}
			};
			MidiPortWrapper.Listen(new IPEndPoint(IPAddress.Any, port + 1));

			var mdns = new MulticastService();
			mdns.QueryReceived += (s, e) =>
			{
				var names = e.Message.Questions
					.Select(q => q.Name + " " + q.Type);
				Console.WriteLine($"got a query for {String.Join(", ", names)}");
			};
			mdns.AnswerReceived += (s, e) =>
			{
				var names = e.Message.Answers
					.Select(q => q.Name + " " + q.Type)
					.Distinct();
				Console.WriteLine($"got answer for {String.Join(", ", names)}");
			};
			mdns.NetworkInterfaceDiscovered += (s, e) =>
			{
				foreach (var nic in e.NetworkInterfaces)
				{
					Console.WriteLine($"discovered NIC '{nic.Name}'");
				}
			};

			ServiceProfile = new ServiceProfile("looool", "_apple-midi._udp", port);
			ServiceDiscovery = new ServiceDiscovery(mdns);
			ServiceDiscovery.Advertise(ServiceProfile);

			ServiceDiscovery.ServiceDiscovered += (sender, discoveredEvent) =>
			{
				Console.WriteLine(discoveredEvent);
			};
		}

		private async Task Respond(ControlPacket invitation, IPEndPoint remoteEndpoint)
		{
			var response = new ControlPacket
			{
				Command = ControlPacketCommand.OK,
				Name = "MyThing",
				Ssrc = 927230719,
				ProtocolVersion = 2,
				InitiatorToken = invitation.InitiatorToken
			};
			using var owner = MemoryPool<byte>.Shared.Rent(response.Length);

			var packetMem = owner.Memory[0..response.Length];
			response.WriteTo(packetMem);
			await ControlPortWrapper.Send(new IPEndPoint(remoteEndpoint.Address, 5004), packetMem);
		}
	}
}