using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KadmiumRtpMidi.Packets
{
	public class PacketReceivedEventArgs : EventArgs
	{
		public DataPacket Packet { get; }
		public PacketReceivedEventArgs(DataPacket packet)
		{
			Packet = packet;
		}
	}
}