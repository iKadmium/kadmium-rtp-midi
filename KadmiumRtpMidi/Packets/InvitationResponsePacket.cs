using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KadmiumRtpMidi.Packets
{
	public class InvitationResponsePacket : SessionExchangePacket
	{
		public InvitationResponsePacket() : base()
		{
			Command = ControlPacketCommand.OK;
		}

		public static new InvitationResponsePacket Parse(ReadOnlyMemory<byte> buffer)
		{
			var packet = new InvitationResponsePacket();
			packet.ParseInternal(buffer);
			return packet;
		}
	}
}