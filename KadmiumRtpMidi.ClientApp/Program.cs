﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KadmiumRtpMidi;

namespace KadmiumRtpMidi.ClientApp
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var hostname = Dns.GetHostName();
			var addresses = await Dns.GetHostAddressesAsync(hostname);
			foreach (var address in addresses)
			{
				Console.WriteLine(address);
			}
			var session = new Session(addresses.First(), 5023, "Kadmium-rtp-midi");
			session.OnPacketReceived += async (sender, e) => await Console.Out.WriteLineAsync(e.Packet.ToString());
			var doreMidi = new IPEndPoint(IPAddress.Parse("192.168.0.93"), 5004);
			await session.Invite(doreMidi);
			while (true)
			{
				await Task.Delay(1000);
			}
		}
	}
}
