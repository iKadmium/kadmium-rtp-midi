using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KadmiumRtpMidi.Packets
{
	public class InvitationPacket : SessionExchangePacket
	{
		public InvitationPacket()
		{
			Command = ControlPacketCommand.IN;
		}

		public static new InvitationPacket Parse(ReadOnlyMemory<byte> buffer)
		{
			var packet = new InvitationPacket();
			packet.ParseInternal(buffer);
			return packet;
		}
	}
}