using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KadmiumRtpMidi.Packets
{
	public abstract class ControlPacket : Packet
	{
		public ControlPacketCommand Command { get; set; }

		public static new ControlPacket Parse(ReadOnlyMemory<byte> buffer)
		{
			var bytes = buffer.Span;
			var commandStr = System.Text.Encoding.ASCII.GetString(bytes[2..4]);
			var command = Enum.Parse<ControlPacketCommand>(commandStr);
			switch (command)
			{
				case ControlPacketCommand.IN:
					return InvitationPacket.Parse(buffer);
				case ControlPacketCommand.CK:
					return ClockSyncPacket.Parse(buffer);
				case ControlPacketCommand.BY:
					return SessionClosePacket.Parse(buffer);
				default:
					return null;
			}
		}
	}
}