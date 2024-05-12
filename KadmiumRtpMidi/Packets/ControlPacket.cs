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
            return command switch
            {
                ControlPacketCommand.IN => InvitationPacket.Parse(buffer),
                ControlPacketCommand.CK => ClockSyncPacket.Parse(buffer),
                ControlPacketCommand.BY => SessionClosePacket.Parse(buffer),
                ControlPacketCommand.OK => InvitationResponsePacket.Parse(buffer),
                _ => null,
            };
        }
	}
}