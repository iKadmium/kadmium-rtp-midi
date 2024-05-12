using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace KadmiumRtpMidi.Packets
{
	public class InvitationPacket : SessionExchangePacket
	{
		public InvitationPacket()
		{
			Command = ControlPacketCommand.IN;
			InitiatorToken = (uint)RandomNumberGenerator.GetInt32((int)(uint.MaxValue >> 1));
		}

		public static new InvitationPacket Parse(ReadOnlyMemory<byte> buffer)
		{
			var packet = new InvitationPacket();
			packet.ParseInternal(buffer);
			return packet;
		}
	}
}