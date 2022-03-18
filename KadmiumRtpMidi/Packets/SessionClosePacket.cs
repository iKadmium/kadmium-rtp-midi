using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KadmiumRtpMidi.Packets
{
	public class SessionClosePacket : SessionExchangePacket
	{
		public SessionClosePacket()
		{
			Command = ControlPacketCommand.BY;
		}

		public static new SessionClosePacket Parse(ReadOnlyMemory<byte> buffer)
		{
			var packet = new SessionClosePacket();
			packet.ParseInternal(buffer);
			return packet;
		}
	}
}