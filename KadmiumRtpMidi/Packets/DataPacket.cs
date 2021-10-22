using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KadmiumRtpMidi.Packets
{
	public class DataPacket : Packet
	{
		public static new DataPacket Parse(ReadOnlyMemory<byte> buffer)
		{
			return new DataPacket();
		}
	}
}