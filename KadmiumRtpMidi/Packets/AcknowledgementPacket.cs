using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KadmiumRtpMidi.Packets
{
	public class AcknowledgementPacket : ControlPacket
	{
		public const int Length = 12;
		public UInt32 SequenceNumber { get; set; }
		public uint Ssrc { get; set; }

		public AcknowledgementPacket()
		{
			Command = ControlPacketCommand.RS;
		}

		public void WriteTo(Memory<byte> bytes)
		{
			if (bytes.Length < Length)
			{
				throw new OutOfMemoryException($"Memory was not large enough - needed {Length}, received {bytes.Length}");
			}

			var span = bytes.Span[0..Length];
			span[0] = 0xff;
			span[1] = 0xff;
			System.Text.Encoding.ASCII.GetBytes(Command.ToString(), span[2..4]);
			BinaryPrimitives.WriteUInt32BigEndian(span[4..8], Ssrc);
			BinaryPrimitives.WriteUInt32BigEndian(span[8..12], SequenceNumber);
		}
	}
}